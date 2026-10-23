"""Independent expectations for the synthetic JSON experiment.

Does not execute a serializer or derive expected values from fixture output.
Literal cases preserve the mapped Id/Status; extra property policy is checked
separately from a known-property type error.
"""
import json
import sys
from pathlib import Path

report = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
by_id = {case["CaseId"]: case for case in report["cases"]}
assert len(by_id) == len(report["cases"]) == 7
expected_success = ["default_baseline", "default_extra", "skip_extra", "strict_baseline"]
checks = []
for case_id in expected_success:
    case = by_id[case_id]
    assert case["Outcome"] == "success", (case_id, case)
    assert type(case["Id"]) is int and case["Id"] == 42
    assert case["Status"] == "Open"
    assert case["ExceptionType"] is None and case["ExceptionPath"] is None
    checks.append({"case": case_id, "result": "PASS", "expected": "Id=42, Status=Open"})

for case_id, expected_path in [
    ("strict_extra", "$.Priority"),
    ("default_wrong_type", "$.Id"),
    ("strict_wrong_type", "$.Id"),
]:
    case = by_id[case_id]
    assert case["Outcome"] == "JsonException", (case_id, case)
    assert case["ExceptionType"] == "System.Text.Json.JsonException"
    assert case["ExceptionPath"] == expected_path
    assert case["Id"] is None and case["Status"] is None
    checks.append({"case": case_id, "result": "PASS", "expected_exception_path": expected_path})

# Independently parse the input to establish that this is an added member,
# not a changed mapped field, and that the negative control changes a type.
baseline = json.loads(by_id["default_baseline"]["Input"])
extended = json.loads(by_id["default_extra"]["Input"])
assert baseline == {"Id": 42, "Status": "Open"}
assert extended == {"Id": 42, "Status": "Open", "Priority": "High"}
assert by_id["default_extra"]["Input"] == by_id["skip_extra"]["Input"] == by_id["strict_extra"]["Input"]
assert by_id["strict_baseline"]["Input"] == by_id["default_baseline"]["Input"]
assert by_id["strict_extra"]["Mode"] == "Disallow"
assert by_id["skip_extra"]["Mode"] == "Skip"
assert type(json.loads(by_id["default_wrong_type"]["Input"])["Id"]) is str
assert report["input_kind"] == "synthetic"
assert report["framework"] == ".NET 10.0.9"
assert report["target_framework"] == ".NETCoreApp,Version=v10.0"
print(json.dumps({"check": "independent_literal_expectations", "status": "PASS", "cases": checks, "runtime": report["framework"]}, ensure_ascii=False, indent=2))
