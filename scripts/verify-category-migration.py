import sqlite3,json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf-8')
before=Path('outputs/category-audit-20260911/moneypro-before-v29.db')
after=Path('Data/moneypro.db')
c=sqlite3.connect(f'file:{after.as_posix()}?mode=ro',uri=True)
c.execute('ATTACH DATABASE ? AS original',(str(before.resolve()),))
result={'before':c.execute('PRAGMA original.user_version').fetchone()[0],'after':c.execute('PRAGMA main.user_version').fetchone()[0],'integrity':c.execute('PRAGMA integrity_check').fetchall(),'foreign_keys':c.execute('PRAGMA foreign_key_check').fetchall(),'preserved':{}}
for table in ['Transacoes','Orcamentos','MovimentosConta','Contas','CategorySeed26','CategoryLegacy26','CategoryMappingLegacy26','CategoryMigration26Log','CategoryCatalogRepair27']:
    if c.execute("select 1 from original.sqlite_master where type='table' and name=?",(table,)).fetchone():
        lost=c.execute(f'SELECT COUNT(*) FROM (SELECT * FROM original."{table}" EXCEPT SELECT * FROM main."{table}")').fetchone()[0]
        result['preserved'][table]={'old_rows':c.execute(f'SELECT COUNT(*) FROM original."{table}"').fetchone()[0],'lost_or_changed':lost}
result['preserved']['Logs']={'lost_or_changed':c.execute('SELECT COUNT(*) FROM (SELECT * FROM original.Logs EXCEPT SELECT * FROM main.Logs)').fetchone()[0]}
result['preserved']['category_ids']={'lost':c.execute('SELECT COUNT(*) FROM (SELECT id_categoria FROM original.Categorias EXCEPT SELECT id_categoria FROM main.Categorias)').fetchone()[0]}
result['preserved']['pillar_mappings']={'lost_or_changed':c.execute('SELECT COUNT(*) FROM (SELECT * FROM original.CategoriaPilarOrcamentario EXCEPT SELECT * FROM main.CategoriaPilarOrcamentario)').fetchone()[0]}
result['catalog']=c.execute('SELECT p.nome,p.icone,c.nome_categoria,c.icone FROM CategoriaPilar p JOIN Categorias c ON c.id_categoria_pilar=p.id_categoria_pilar WHERE p.id_usuario=1 ORDER BY p.ordem,c.nome_categoria').fetchall()
Path('outputs/category-audit-20260911/database-verification.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k!='catalog'},ensure_ascii=False,indent=2))
assert result['after']==29
assert result['integrity']==[('ok',)] and result['foreign_keys']==[]
assert all(v.get('lost_or_changed',v.get('lost',0))==0 for v in result['preserved'].values())
