"""Merge the supplied legacy database into the planner, without replacing target data.

Run: python tools/migrate_legacy_sqlite.py moneyproAntigo.db moneyproNovo.db
SQLite backups, ID mappings and reconciliation make the import repeatable/auditable.
"""
import datetime
import hashlib
import json
import sqlite3
import sys
from pathlib import Path

source_path, target_path = [Path(p).resolve(strict=True) for p in sys.argv[1:]]
assert source_path != target_path
source_hash = hashlib.sha256(source_path.read_bytes()).hexdigest()
source = sqlite3.connect(source_path.as_uri() + '?mode=ro', uri=True)
source.row_factory = sqlite3.Row
target = sqlite3.connect(target_path)
target.row_factory = sqlite3.Row
schema_query = "SELECT type,name,sql FROM sqlite_master WHERE name NOT LIKE 'sqlite_%' ORDER BY type,name"
original_schema = [tuple(r) for r in target.execute(schema_query)]
assert source.execute('PRAGMA integrity_check').fetchone()[0] == 'ok'
assert target.execute('PRAGMA integrity_check').fetchone()[0] == 'ok'
backup_dir = target_path.parent / 'MigrationBackups' / datetime.datetime.now().strftime('merge-legacy-%Y%m%d-%H%M%S-%f')
backup_dir.mkdir(parents=True)
for db, name in [(source, 'source.db'), (target, 'target-before.db')]:
    with sqlite3.connect(backup_dir / name) as backup:
        db.backup(backup)
target.execute('PRAGMA foreign_keys=ON')
target.execute('BEGIN IMMEDIATE')
target.execute('PRAGMA defer_foreign_keys=ON')
report = {'source_sha256': source_hash, 'backup': str(backup_dir), 'tables': {}}
maps = {}
ledger_path = target_path.parent / 'MigrationBackups' / (target_path.stem + '-migration-map.json')
ledger = json.loads(ledger_path.read_text(encoding='utf-8')) if ledger_path.exists() else {}
def ledger_key(table, old_id):
    return f'{source_hash}:{table}:{old_id}'

def rows(table):
    return [dict(r) for r in source.execute(f'SELECT * FROM "{table}"')]

def insert(table, row):
    columns = {r['name'] for r in target.execute(f'PRAGMA table_info("{table}")')}
    data = {k: v for k, v in row.items() if k in columns}
    names = ','.join('"' + k + '"' for k in data)
    return target.execute(f'INSERT INTO "{table}" ({names}) VALUES ({",".join("?" for _ in data)})', list(data.values())).lastrowid

try:

    def merge(table, pk, keys, refs=None, transform=None):
        mapping = maps[table] = {}
        added = reused = 0
        for original in rows(table):
            row = original.copy()
            old_id = row.pop(pk)
            previous = ledger.get(ledger_key(table, old_id))
            if previous:
                mapping[old_id] = previous
                reused += 1
                continue
            for col, parent in (refs or {}).items():
                if row.get(col) is not None:
                    row[col] = maps[parent][row[col]]
            if transform:
                transform(row)
            existing = None
            if keys:
                where = ' AND '.join(f'"{k}" IS ?' for k in keys)
                existing = target.execute(f'SELECT "{pk}" FROM "{table}" WHERE {where}', [row[k] for k in keys]).fetchone()
            new_id = existing[0] if existing else insert(table, row)
            added += not bool(existing)
            reused += bool(existing)
            mapping[old_id] = new_id
            ledger[ledger_key(table, old_id)] = new_id
        report['tables'][table] = {'inserted': added, 'reused': reused, 'source': len(mapping)}

    merge('Usuarios', 'id_usuario', ['email'])
    merge('Categorias', 'id_categoria', ['id_usuario', 'nome_categoria', 'tipo'], {'id_usuario': 'Usuarios'}, lambda r: r.update(id_categoria_pai=None))
    merge('CartoesCredito', 'id_cartao', ['id_usuario', 'nome_cartao'], {'id_usuario': 'Usuarios'})
    merge('Fornecedores', 'id_fornecedor', ['id_usuario', 'nome_fornecedor'], {'id_usuario': 'Usuarios'})
    merge('Configuracoes', 'id_configuracao', ['id_usuario'], {'id_usuario': 'Usuarios'})
    merge('FaturasCartao', 'id_fatura', ['id_cartao', 'mes_referencia', 'ano_referencia'], {'id_cartao': 'CartoesCredito'})

    transactions = [r for r in rows('Transacoes') if r['tipo'].lower() in ('receita', 'despesa')]
    transaction_map = {}
    pending = []
    for original in transactions:
        old_id = original['id_transacao']
        previous = ledger.get(ledger_key('Transacoes', old_id))
        if previous:
            transaction_map[old_id] = previous
            continue
        row = original.copy()
        row.pop('id_transacao')
        for col, table in [('id_usuario', 'Usuarios'), ('id_categoria', 'Categorias'), ('id_cartao', 'CartoesCredito'), ('id_fornecedor', 'Fornecedores'), ('id_fatura', 'FaturasCartao')]:
            if row.get(col) is not None:
                row[col] = maps[table][row[col]]
        row['id_transacao_pai'] = None
        row['tipo'] = row['tipo'].lower()
        row['data_vencimento'] = (row['data_vencimento'] or row['data'])[:10]
        for col, default in [('parcelado', 0), ('pago', 0), ('numero_parcela', 1), ('total_parcelas', 1), ('recorrente', 0)]:
            if row.get(col) is None:
                row[col] = default
        new_id = insert('Transacoes', row)
        transaction_map[old_id] = new_id
        pending.append(original)
        ledger[ledger_key('Transacoes', old_id)] = new_id
    for row in pending:
        if row['id_transacao_pai'] is not None:
            target.execute('UPDATE Transacoes SET id_transacao_pai=? WHERE id_transacao=?', (transaction_map[row['id_transacao_pai']], transaction_map[row['id_transacao']]))

    # This source contains no Receitas rows; stop rather than silently omit an unsupported variant.
    assert not rows('Receitas'), 'Separate legacy income rows require explicit conversion'
    for original in transactions:
        migrated = dict(target.execute('SELECT * FROM Transacoes WHERE id_transacao=?', (transaction_map[original['id_transacao']],)).fetchone())
        for field in ['valor', 'data', 'descricao', 'pago', 'data_pagamento', 'numero_parcela', 'total_parcelas']:
            assert migrated[field] == original[field], (original['id_transacao'], field)
        assert migrated['data_vencimento'] == (original['data_vencimento'] or original['data'])[:10]
        for field, table in [('id_usuario', 'Usuarios'), ('id_categoria', 'Categorias'), ('id_cartao', 'CartoesCredito'), ('id_fornecedor', 'Fornecedores'), ('id_fatura', 'FaturasCartao')]:
            assert migrated[field] == (maps[table][original[field]] if original[field] is not None else None)
        assert migrated['id_transacao_pai'] == (transaction_map[original['id_transacao_pai']] if original['id_transacao_pai'] is not None else None)
    assert original_schema == [tuple(r) for r in target.execute(schema_query)], 'Destination schema changed'
    assert not target.execute('PRAGMA foreign_key_check').fetchall(), 'Broken relationships'
    assert target.execute('PRAGMA integrity_check').fetchone()[0] == 'ok'
    report['tables']['Transacoes'] = {'source': len(transactions), 'inserted': len(pending), 'reused': len(transactions)-len(pending)}
    report['financial_totals'] = [dict(r) for r in source.execute("SELECT tipo,pago,count(*) quantidade,round(sum(valor),2) valor FROM Transacoes WHERE tipo IN ('receita','despesa') GROUP BY tipo,pago")]
    report['ignored_accounts'] = len(rows('Contas'))
    report['ignored_transfers'] = sum(r['tipo'] == 'transferencia' for r in rows('Transacoes'))
    target.commit()
    ledger_path.write_text(json.dumps(ledger, indent=2), encoding='utf-8')
    assert hashlib.sha256(source_path.read_bytes()).hexdigest() == source_hash
    (backup_dir / 'report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))
except Exception:
    target.rollback()
    raise
finally:
    target.close()
    source.close()
