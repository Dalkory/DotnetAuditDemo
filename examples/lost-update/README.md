# Lost update: original revision and blind retry

An own synthetic Python/SQLite example from **dotnet-audit-studio**. It uses two in-memory databases and deliberately interleaves stale reads and writes; the blind retry continues the guarded sequence. It does not create parallel clients or run EF Core.

Read [lost_update_fixture.py](lost_update_fixture.py) alongside the [original saved observation](lost-update-verification.json), which records **SQLite 3.45.3**:

| Sequence | Saved result |
| --- | --- |
| Both writes filter only by ID | Both affect one row; the later edit replaces the earlier value. |
| Each write also checks the original revision | The first affects one row; the stale second write affects zero rows. |
| Reload and blindly retry the stale edit | The retry changes the retained result. This illustrates a policy hazard, not a recommendation. |

The copied historical observation is retained as evidence. A current runtime may report a different SQLite version. No new run is claimed by this repository addition.

## Optional reproduction

With Python and its standard-library `sqlite3` module already installed, run from this directory:

```text
python lost_update_fixture.py
```

The fixture prints JSON and asserts its synthetic invariants. It uses in-memory databases and performs no network or file writes. It is not a production concurrency benchmark, an EF Core integration test, a conflict-resolution policy, or a guarantee for another database provider.

Read the [Russian note](https://dotnet-audit-studio.dtaatdaa.chatgpt.site/ru/notes/lost-update) or return to the [four-note library](../).
