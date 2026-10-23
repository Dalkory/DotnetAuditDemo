"""Synthetic teaching fixture. No network, credentials, client data, or files.

This deterministic interleaving illustrates stale edits with SQLite SQL.
It is not an EF Core integration test or a production concurrency benchmark.
"""
import json
import sqlite3


def initial_database():
    db = sqlite3.connect(':memory:')
    db.execute('CREATE TABLE WorkItems (Id INTEGER PRIMARY KEY, Status TEXT NOT NULL, Revision INTEGER NOT NULL)')
    db.execute('INSERT INTO WorkItems VALUES (1, ?, 1)', ('Open',))
    return db


def current(db):
    row = db.execute('SELECT Status, Revision FROM WorkItems WHERE Id = 1').fetchone()
    return {'status': row[0], 'revision': row[1]}


def run():
    unsafe = initial_database()
    a = current(unsafe)
    b = current(unsafe)
    assert a == b == {'status': 'Open', 'revision': 1}
    a_rows = unsafe.execute('UPDATE WorkItems SET Status = ? WHERE Id = 1', ('Done',)).rowcount
    b_rows = unsafe.execute('UPDATE WorkItems SET Status = ? WHERE Id = 1', ('InProgress',)).rowcount
    unsafe_result = current(unsafe)
    assert a_rows == b_rows == 1
    assert unsafe_result == {'status': 'InProgress', 'revision': 1}

    guarded = initial_database()
    a = current(guarded)
    b = current(guarded)
    statement = 'UPDATE WorkItems SET Status = ?, Revision = Revision + 1 WHERE Id = ? AND Revision = ?'
    a_rows = guarded.execute(statement, ('Done', 1, a['revision'])).rowcount
    b_rows = guarded.execute(statement, ('InProgress', 1, b['revision'])).rowcount
    guarded_result = current(guarded)
    assert a_rows == 1 and b_rows == 0
    assert guarded_result == {'status': 'Done', 'revision': 2}
    guarded.commit()
    assert current(guarded) == guarded_result

    # Illustrates the hazard of a blind reload-and-retry policy, not a recommendation.
    fresh = current(guarded)
    retry_rows = guarded.execute(statement, ('InProgress', 1, fresh['revision'])).rowcount
    retry_result = current(guarded)
    assert retry_rows == 1
    assert retry_result == {'status': 'InProgress', 'revision': 3}

    result = {
        'synthetic': True,
        'sqlite_version': sqlite3.sqlite_version,
        'coverage': 'deterministic stale-read interleaving; no parallel connections or EF Core runtime',
        'unsafe': {'a_affected': 1, 'b_affected': 1, 'final': unsafe_result},
        'guarded': {'a_affected': a_rows, 'b_affected': b_rows, 'after_commit': guarded_result},
        'blind_retry': {'affected': retry_rows, 'final': retry_result},
        'acceptance': 'PASS: stale write rejected; first result retained; blind retry changes outcome',
    }
    unsafe.close()
    guarded.close()
    return result


if __name__ == '__main__':
    print(json.dumps(run(), ensure_ascii=False, indent=2))
