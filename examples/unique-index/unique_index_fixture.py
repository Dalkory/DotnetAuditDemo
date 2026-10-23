"""Original synthetic SQLite unique-index fixture, dotnet-audit-studio.

Only two independent SQLite ':memory:' connections are opened. No database
filename, SQL from arguments, private input, external request or remediation.
"""
import json
import platform
import sqlite3
from pathlib import Path


def snapshot(connection):
    rows = connection.execute(
        "SELECT demo_row, external_key, payload, typeof(demo_row), "
        "typeof(external_key), typeof(payload) FROM demo_items ORDER BY demo_row"
    ).fetchall()
    index_flags = {
        entry[1]: entry for entry in connection.execute("PRAGMA index_list('demo_items')")
    }
    indexes = []
    for name, sql in connection.execute(
        "SELECT name, sql FROM sqlite_schema WHERE type = 'index' "
        "AND tbl_name = 'demo_items' ORDER BY name"
    ):
        flags = index_flags[name]
        indexes.append({"name": name, "sql": sql, "unique": flags[2],
                        "origin": flags[3], "partial": flags[4]})
    return {
        "rows": [list(row[:3]) for row in rows],
        "sqlite_storage_types": [list(row[3:]) for row in rows],
        "python_value_types": [[type(value).__name__ for value in row[:3]] for row in rows],
        "duplicate_groups": [list(row) for row in connection.execute(
            "SELECT external_key, COUNT(*) FROM demo_items GROUP BY external_key "
            "HAVING COUNT(*) > 1 ORDER BY external_key"
        )],
        "indexes": indexes,
        "in_transaction": connection.in_transaction,
    }


def observe_statement(connection, sql, parameters=()):
    try:
        connection.execute(sql, parameters)
        return {"outcome": "success", "exception_class": None,
                "sqlite_errorcode": None, "sqlite_errorname": None, "message": None}
    except sqlite3.Error as error:
        return {"outcome": "error", "exception_class": type(error).__name__,
                "sqlite_errorcode": error.sqlite_errorcode,
                "sqlite_errorname": error.sqlite_errorname, "message": str(error)}


def new_memory_database(schema, rows):
    # isolation_level=None means these statements use SQLite autocommit.
    # This is not an explicit transaction, rollback or recovery test.
    connection = sqlite3.connect(":memory:", isolation_level=None)
    connection.execute(schema)
    connection.executemany(
        "INSERT INTO demo_items (demo_row, external_key, payload) VALUES (?, ?, ?)", rows
    )
    assert connection.execute("PRAGMA database_list").fetchall() == [(0, "main", "")]
    return connection


def run():
    own_input = json.loads(Path(__file__).with_name("unique_index_input.json").read_text(encoding="utf-8"))
    assert own_input["input_kind"] == "own_synthetic"
    schema = own_input["schema_sql"]
    index_sql = own_input["unique_index_sql"]
    assert schema == "CREATE TABLE demo_items (demo_row INTEGER PRIMARY KEY, external_key TEXT NOT NULL, payload TEXT NOT NULL)"
    assert index_sql == "CREATE UNIQUE INDEX ux_demo_external_key ON demo_items(external_key)"
    duplicate_connection = new_memory_database(schema, own_input["duplicate_data_case"]["seed_rows"])
    unique_connection = new_memory_database(schema, own_input["independent_unique_baseline_case"]["seed_rows"])
    try:
        duplicates_before = snapshot(duplicate_connection)
        duplicate_index = observe_statement(duplicate_connection, index_sql)
        duplicates_after = snapshot(duplicate_connection)
        unique_before = snapshot(unique_connection)
        unique_index = observe_statement(unique_connection, index_sql)
        unique_after_index = snapshot(unique_connection)
        duplicate_insert = observe_statement(
            unique_connection,
            "INSERT INTO demo_items (demo_row, external_key, payload) VALUES (?, ?, ?)",
            own_input["independent_unique_baseline_case"]["duplicate_insert_row"],
        )
        unique_after_insert = snapshot(unique_connection)
        return {
            "brand": "dotnet-audit-studio", "input_kind": "own_synthetic",
            "scope": "Two independent in-memory SQLite connections; fixed literal input; plain CREATE UNIQUE INDEX and one plain INSERT. No private database, real migration runner, EF Core, SQL Server, load, concurrency, deduplication or rollback test.",
            "runtime": {"python": platform.python_version(), "sqlite_library": sqlite3.sqlite_version,
                        "sqlite_source_id": duplicate_connection.execute("SELECT sqlite_source_id()").fetchone()[0]},
            "connection_boundary": {"database": ":memory:", "independent_connections": 2,
                                    "isolation_level": None, "explicit_transactions": False},
            "duplicate_data_case": {"before": duplicates_before, "index_creation": duplicate_index,
                                    "after_failed_index": duplicates_after},
            "independent_unique_baseline_case": {"before": unique_before, "index_creation": unique_index,
                                                 "after_index": unique_after_index,
                                                 "duplicate_insert": duplicate_insert,
                                                 "after_rejected_insert": unique_after_insert},
        }
    finally:
        duplicate_connection.close()
        unique_connection.close()


if __name__ == "__main__":
    print(json.dumps(run(), ensure_ascii=False, indent=2))
