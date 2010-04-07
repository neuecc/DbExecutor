/*--------------------------------------------------------------------------
* DbExecutor - linq based database executor
* ver 1.0.0.1 (Apr. 08th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dbexecutor.codeplex.com/
*--------------------------------------------------------------------------*/

// InstallGuide

over VS2008 and target framework 3.5
- DbExecutor.cs

VS2005 or target framework 2.0
- DbExecutor.NET2.cs

// HowToUse

using Codeplex.Data;

var executor = new DbExecutor(DbConnection);
executor.Methods(query, parameters)
or
var result = DbExecutor.Methods(DbConnection, query, parameters);

query's DbParameter is applied "@p0, @p1, ...";

// History

2010-04-08 ver 1.0.0.1
Name Change(Executer -> Executor)

2010-04-07 ver 1.0.0.0
1st Release