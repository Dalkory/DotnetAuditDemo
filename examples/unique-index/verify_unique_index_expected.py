"""Compare saved observations to independent literal expectations, without SQL.

Checks every row identity, full payload value and SQLite/Python value type;
does not generate expectations from the input, before-state or observed JSON.
"""
import hashlib
import json
from pathlib import Path


def verify():
    folder = Path(__file__).parent
    expected_path = folder / "unique_index_literal_expected.json"
    immutable_expected_hash = hashlib.sha256(expected_path.read_bytes()).hexdigest()
    expected = json.loads(expected_path.read_text(encoding="utf-8"))
    actual = json.loads((folder / "unique_index_observed.json").read_text(encoding="utf-8"))
    checks = []

    def check(label, condition):
        checks.append({"check": label, "passed": bool(condition)})

    def snapshot_checks(label, state, rows, groups, indexes, transaction):
        check(label + ": full row identity and values", state["rows"] == rows)
        check(label + ": SQLite storage types", state["sqlite_storage_types"] == [expected["sqlite_storage_types_per_row"] for _ in rows])
        check(label + ": Python value types", state["python_value_types"] == [expected["python_value_types_per_row"] for _ in rows])
        check(label + ": duplicate groups", state["duplicate_groups"] == groups)
        check(label + ": exact index state", state["indexes"] == indexes)
        check(label + ": autocommit observation", state["in_transaction"] is transaction)

    def operation_checks(label, operation, wanted):
        for field, value in wanted.items():
            check(label + ": " + field, operation[field] == value)

    check("input explicitly synthetic", actual["input_kind"] == "own_synthetic")
    check("two separate in-memory connections", actual["connection_boundary"] == {"database": ":memory:", "independent_connections": 2, "isolation_level": None, "explicit_transactions": False})
    first = actual["duplicate_data_case"]
    want = expected["duplicate_data_case"]
    for stage in ("before", "after_failed_index"):
        snapshot_checks("duplicate-data " + stage, first[stage], want["expected_rows_for_before_and_after_failed_index"], want["expected_duplicate_groups"], want["expected_indexes_before_and_after"], want["expected_in_transaction_before_and_after"])
    operation_checks("duplicate-data CREATE UNIQUE INDEX", first["index_creation"], want["expected_index_creation"])
    second = actual["independent_unique_baseline_case"]
    want = expected["independent_unique_baseline_case"]
    snapshot_checks("unique-data before", second["before"], want["expected_rows_for_before_after_index_and_after_rejected_insert"], want["expected_duplicate_groups"], [], want["expected_in_transaction_at_each_snapshot"])
    for stage in ("after_index", "after_rejected_insert"):
        snapshot_checks("unique-data " + stage, second[stage], want["expected_rows_for_before_after_index_and_after_rejected_insert"], want["expected_duplicate_groups"], [want["expected_index"]], want["expected_in_transaction_at_each_snapshot"])
    operation_checks("unique-data CREATE UNIQUE INDEX", second["index_creation"], want["expected_index_creation"])
    operation_checks("unique-data future duplicate INSERT", second["duplicate_insert"], want["expected_duplicate_insert"])
    check("literal expected file unchanged by verification", hashlib.sha256(expected_path.read_bytes()).hexdigest() == immutable_expected_hash)
    result = {"input_kind": "own_synthetic", "expected_derivation": "Independent manually declared literal file; not copied from observed state.",
              "all_checks_passed": all(item["passed"] for item in checks), "check_count": len(checks), "checks": checks,
              "expected_file_sha256": immutable_expected_hash,
              "observed_file_sha256": hashlib.sha256((folder / "unique_index_observed.json").read_bytes()).hexdigest(),
              "limits": "This verifies local SQLite observations and complete typed synthetic rows. It does not verify a customer database, migration provider, backup, rollback, load or business duplicate resolution."}
    return result


if __name__ == "__main__":
    result = verify()
    print(json.dumps(result, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result["all_checks_passed"] else 1)
