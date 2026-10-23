# Unique index: existing duplicates and later inserts

An own synthetic **dotnet-audit-studio** Python/SQLite example using two independent in-memory databases and fixed input. It does not connect to a customer database or remove business data.

Inspect [the fixture](unique_index_fixture.py), [synthetic input](unique_index_input.json), [independent literal expectations](unique_index_literal_expected.json), [historical observations](unique_index_observed.json), [checker](verify_unique_index_expected.py) and [saved check result](unique_index_expectation_check.json).

The original capture shows two separate scenarios:

1. Existing duplicate keys cause `CREATE UNIQUE INDEX` to fail. The recorded row identities, complete values and types remain unchanged, and the index is absent.
2. A separate unique baseline permits index creation. A later duplicate `INSERT` fails, while the recorded original rows and index remain unchanged.

The saved checker passed **45 field and invariant checks across these two scenarios**, not 45 independent scenarios. The captured environment was **Python 3.12.4 / SQLite 3.45.3**; the SQLite source ID is retained in the observation's `runtime` object. Adding these files does not constitute a fresh run.

## Optional reproduction

With the recorded Python/SQLite environment already available, run from this directory:

```text
python unique_index_fixture.py
```

To compare only the saved observations with literal expectations, without executing SQL:

```text
python verify_unique_index_expected.py
```

Both commands print JSON; the fixture uses only its fixed local synthetic input and in-memory databases. A different Python/SQLite version is not the historical environment. This does not verify a real migration runner, EF Core, SQL Server, backup, rollback, load, concurrency or a business rule for resolving duplicates. Autocommit observations are not an explicit transaction/recovery test.

Read the [Russian note](https://dotnet-audit-studio.dtauskanov3.chatgpt.site/ru/notes/unique-index-duplicates) or return to the [four-note library](../).
