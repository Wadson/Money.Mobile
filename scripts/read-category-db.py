import sqlite3,json
c=sqlite3.connect('file:Data/moneypro.db?mode=ro',uri=True)
print('version',c.execute('pragma user_version').fetchone())
print('mains',c.execute('select nome,icone from CategoriaPilar where id_usuario=1').fetchall())
print('subcategories',c.execute('select nome_categoria,icone from Categorias where id_usuario=1').fetchall())
print('integrity',c.execute('pragma integrity_check').fetchall(),'foreign keys',c.execute('pragma foreign_key_check').fetchall())
print('main triggers',c.execute("select name from sqlite_master where type='trigger' and tbl_name='CategoriaPilar'").fetchall())
print('sub triggers',c.execute("select name from sqlite_master where type='trigger' and tbl_name='Categorias'").fetchall())
