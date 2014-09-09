MvcLib
======
How to use subtree
```
git remote add --fetch MvcLib https://jpolvora@bitbucket.org/jpolvora/mvclib.git

git subtree add --prefix=MvcLib MvcLib master --squash

//update from MvcLib
git fetch MvcLib master
git subtree pull --prefix=MvcLib MvcLib master --squash

//push back to MvcLib (fork)
git remote add MvcLib_Push https://jpolvora@bitbucket.org/jpolvora/mvclib.git
git subtree push --prefix=MvcLib MvcLib_Push master
```

Link para o Post do Wordpress: (http://jpolvora.wordpress.com/2014/08/31/trabalhando-com-git-subtrees-em-projetos-compartilhados/)

Um projeto que reúne vários componentes, que juntos, formam uma base para aplicações ASP.NET dinâmicas

### MvcLib.Bootstrapper:
  
Utiliza WebActivatorEx para executar configurações antes que a aplicação web seja iniciada.
Existem tarefas como: Compilação dinâmica, Carregamento de plugins (dlls) que precisam ser executadas no Pre_Start.

### MvcLib.Common:

Classes e extensões compartilhadas entre vários projetos.

### MvcLib.Common.Mvc:

Classes e extensões voltadas para WebPages, Mvc, Razor, HttpRequest/Response, etc.

### MvcLib.CustomVPP:

Contém várias implementações de Virtual Path Providers. 

### MvcLib.DbFileSystem:

Contém classes para acesso ao sistema de arquivos virtual, utilizando Entity Framework 6.1.1 (Code First). Faz utilização de Migrations, possui o contexto chamado DbFileContext com uma entidade DbFile.
  
### MvcLib.FsDump:

Faz o download dos arquivos do sistema de arquivos virtual (DbFileSystem) para o sistema de arquivos local.

### MvcLib.HttpModules:

Contém 2 módulos: Um módulo de TRACE, ou seja, imprime várias informações sobre toda a pipeline de um request ASP.NET, para fins de debug/informação, e outro módulo para tratamento de erros (OnApplication_Error).

### MvcLib.Kompiler: 

Compila um assembly baseado nos arquivos .cs encontrados no sistema de arquivos virtual. Utliza Roslyn.

### MvcLib.PluginLoader: 

Responsável pelo carregamento de Plugins (dlls), inclusive do assembly previamente compilado no Pre_start pelo Kompiler. 
