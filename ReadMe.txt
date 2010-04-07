/*--------------------------------------------------------------------------
* DbExecuter - linq based database executer
* ver 1.0.0.0 (Apr. 07th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dbexecuter.codeplex.com/
*--------------------------------------------------------------------------*/

// InstallGuide

over VS2008 and target framework 3.5
- DbExecuter.cs

VS2005 or target framework 2.0
- DbExecuter.NET2.cs

// HowToUse

using Codeplex.Data;

var executer = new DbExecuter(DbConnection);
executer.Methods(query, parameters)
or
var result = DbExecuter.Methods(DbConnection, query, parameters);

query's DbParameter is applied "@p0, @p1, ...";

// History

2010-04-07 ver 1.0.0.0
1st Release