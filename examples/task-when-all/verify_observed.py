"""Check observed JSON against an independent literal contract; never run the fixture."""
import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def verify(observed, expected):
    checks = []

    def check(condition, label):
        checks.append({"check": label, "status": "PASS" if condition else "FAIL"})

    def exact_type(value, kind, label):
        check(type(value) is kind, label)

    def errors(value, label):
        exact_type(value, list, label + ":array")
        if not isinstance(value, list):
            return []
        pairs = []
        for index, error in enumerate(value):
            exact_type(error, dict, f"{label}:{index}:object")
            if not isinstance(error, dict):
                continue
            exact_type(error.get("type"), str, f"{label}:{index}:type-string")
            exact_type(error.get("message"), str, f"{label}:{index}:message-string")
            pairs.append((error.get("type"), error.get("message")))
        return pairs

    flags = {
        "WaitingForActivation": (False, False, False),
        "RanToCompletion": (True, False, False),
        "Faulted": (True, True, False),
        "Canceled": (True, False, True),
    }

    def task_state(task, task_id, status, expected_faults, label):
        exact_type(task, dict, label + ":object")
        if not isinstance(task, dict):
            return
        exact_type(task.get("id"), str, label + ":id-string")
        exact_type(task.get("status"), str, label + ":status-string")
        check(task.get("id") == task_id, label + ":id")
        check(task.get("status") == status, label + ":literal-status")
        for field, value in zip(("is_completed", "is_faulted", "is_canceled"), flags[status]):
            exact_type(task.get(field), bool, label + ":" + field + ":boolean")
            check(task.get(field) is value, label + ":" + field + ":invariant")
        exact_type(task.get("exception_is_null"), bool, label + ":exception-null-boolean")
        check(task.get("exception_is_null") is (status != "Faulted"), label + ":exception-null-status")
        pairs = errors(task.get("faults"), label + ":faults")
        expected_pairs = [(error["type"], error["message"]) for error in expected_faults]
        # Exact length preserves multiplicity; a set alone would hide duplicate errors.
        check(len(pairs) == len(expected_pairs), label + ":exact-fault-length")
        check(sorted(pairs) == sorted(expected_pairs), label + ":literal-fault-multiset")
        check(pairs == sorted(pairs), label + ":presentation-sorted")
        exact_type(task.get("fault_count"), int, label + ":fault-count-integer")
        check(task.get("fault_count") == len(pairs), label + ":fault-count-consistency")

    exact_type(observed, dict, "document:object")
    check(observed.get("schema_version") == 1, "document:schema-version")
    exact_type(observed.get("schema_version"), int, "document:schema-version-integer")
    check(observed.get("proof_kind") == expected["proof_kind"], "document:synthetic-marker")
    exact_type(observed.get("observed_at_utc"), str, "document:timestamp-string")
    stamp = datetime.fromisoformat(observed["observed_at_utc"].replace("Z", "+00:00"))
    check(stamp.utcoffset() == timezone.utc.utcoffset(stamp), "document:timestamp-utc")
    runtime = observed["runtime"]
    for field in ("runtime_version", "framework_description", "sdk_version"):
        exact_type(runtime.get(field), str, "runtime:" + field + ":string")
    check(runtime["runtime_version"] == expected["runtime_version"], "runtime:target-version")
    check(runtime["sdk_version"] == expected["sdk_version"], "runtime:target-sdk")
    check(runtime["framework_description"] == ".NET " + expected["runtime_version"], "runtime:framework-description")
    scenarios = observed["scenarios"]
    exact_type(scenarios, list, "scenarios:array")
    check(len(scenarios) == 3 == len(expected["scenarios"]), "scenarios:exactly-three-not-check-count")
    by_id = {scenario["id"]: scenario for scenario in scenarios}
    check(len(by_id) == len(scenarios), "scenarios:unique-ids")
    check(set(by_id) == {scenario["id"] for scenario in expected["scenarios"]}, "scenarios:literal-ids")
    for contract in expected["scenarios"]:
        observed_case = by_id[contract["id"]]
        for stage, observed_key in (("before", "before_final_completion"), ("after", "after_final_completion")):
            literal = contract[stage]
            snapshot = observed_case[observed_key]
            prefix = contract["id"] + ":" + stage
            task_state(snapshot["combined"], "demo:when-all", literal["combined_status"], literal["combined_faults"], prefix + ":combined")
            inputs = snapshot["inputs"]
            exact_type(inputs, list, prefix + ":inputs-array")
            check(len(inputs) == len(literal["inputs"]), prefix + ":input-count")
            inputs_by_id = {task["id"]: task for task in inputs}
            check(len(inputs_by_id) == len(inputs), prefix + ":input-ids-unique")
            check(set(inputs_by_id) == {task["id"] for task in literal["inputs"]}, prefix + ":input-ids-literal")
            for task in literal["inputs"]:
                task_state(inputs_by_id[task["id"]], task["id"], task["status"], task["faults"], prefix + ":" + task["id"])
            if stage == "before":
                check(any(task["is_completed"] is False for task in inputs), prefix + ":a-real-input-is-pending")
                check(snapshot["combined"]["is_completed"] is False, prefix + ":combined-does-not-finish-early")
            else:
                check(all(task["is_completed"] is True for task in inputs), prefix + ":all-inputs-terminal-after-await")
                input_faults = [error for task in inputs for error in task["faults"]]
                derived = "Faulted" if input_faults else "Canceled" if any(task["is_canceled"] for task in inputs) else "RanToCompletion"
                check(snapshot["combined"]["status"] == derived, prefix + ":fault-dominates-cancellation")
                check(sorted((e["type"], e["message"]) for e in snapshot["combined"]["faults"]) == sorted((e["type"], e["message"]) for e in input_faults), prefix + ":aggregate-has-input-faults-only")
        awaited = observed_case["await"]
        exact_type(awaited.get("returned_normally"), bool, contract["id"] + ":await-normal-boolean")
        check(awaited["returned_normally"] is False, contract["id"] + ":await-threw")
        caught = awaited["caught"]
        exact_type(caught, dict, contract["id"] + ":caught-object")
        exact_type(caught.get("type"), str, contract["id"] + ":caught-type-string")
        exact_type(caught.get("message"), str, contract["id"] + ":caught-message-string")
        exact_type(caught.get("is_operation_canceled_exception"), bool, contract["id"] + ":caught-cancellation-boolean")
        if contract["after"]["combined_status"] == "Faulted":
            known = {(error["type"], error["message"]) for error in contract["after"]["combined_faults"]}
            check((caught["type"], caught["message"]) in known, contract["id"] + ":await-is-one-known-fault-no-selection-contract")
            check(caught["is_operation_canceled_exception"] is False, contract["id"] + ":await-is-not-cancellation")
        else:
            # The C# source records actual runtime assignability. No specific subtype/message/token is required.
            check(caught["is_operation_canceled_exception"] is True, contract["id"] + ":await-operation-canceled-compatible")
    return checks


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--observed", type=Path, required=True)
    parser.add_argument("--expected", type=Path, required=True)
    parser.add_argument("--receipt", type=Path, required=True)
    args = parser.parse_args()
    checks = []
    error = None
    try:
        checks = verify(json.loads(args.observed.read_bytes()), json.loads(args.expected.read_bytes()))
    except (KeyError, TypeError, ValueError, AttributeError) as failure:
        error = type(failure).__name__ + ": " + str(failure)
    failed = [item for item in checks if item["status"] == "FAIL"]
    status = "PASS" if checks and not failed and error is None else "FAIL"
    receipt = {
        "kind": "independent_literal_task_when_all_verification",
        "verified_at_utc": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "status": status,
        "scenario_count": 3,
        "check_count": len(checks),
        "check_count_is_not_case_count": True,
        "observed_file": args.observed.name,
        "observed_sha256": digest(args.observed),
        "expected_file": args.expected.name,
        "expected_sha256": digest(args.expected),
        "verifier_sha256": digest(Path(__file__)),
        "failed_checks": failed,
        "structural_error": error,
        "checks": checks,
        "await_fault_choice_or_order_asserted": False,
        "cancellation_subtype_message_or_token_asserted": False,
    }
    args.receipt.write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: receipt[key] for key in ("status", "scenario_count", "check_count", "failed_checks", "structural_error")}))
    return 0 if status == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
