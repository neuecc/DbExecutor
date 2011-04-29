/*--------------------------------------------------------------------------
 * DbExecutor
 * ver 2.0.0.1 (Apr. 30th, 2011)
 *
 * created and maintained by neuecc <ils@neue.cc - @neuecc on Twitter>
 * licensed under Microsoft Public License(Ms-PL)
 * http://dbexecutor.codeplex.com/
 *-------------------------------------------------------------------------*/

// Description

Simple and Lightweight Database Executor for .NET 4 Client Profile and all ADO.NET DbProviders.

// Features

set command parameter by AnonymousType
auto type mapping
dynamic IDataRecord accessor
transaction
Code Contracts
basic Select/Insert/Delete/Update query build
available NuGet install

// Standard Usage

// using Codeplex.Data;

// connection string
var connStr = @"Data Source=NORTHWIND";

// arguments are IDbConnection, query, command parameter(by AnonymousType or class instance)
// connection is closed when done.
// ExecuteReader returns IEnumerable<IDataRecord>.
// It means query results can use Linq to Objects.
var systable = DbExecutor.ExecuteReader(new SqlConnection(connStr), @"
        select * from sys.tables where modify_date > @ModDate
        ", new { ModDate = "2005-01-01" })
    .Select(dr => new
    {
        Name = (string)dr["name"],
        CreateDate = (DateTime)dr["create_date"]
    })
    .ToArray();

// if you want to hold open connection, use instance methods
using (var exec = new DbExecutor(new SqlConnection(connStr)))
{
    var hardwares = new[] { "Xbox360", "PS3", "Wii" };

    foreach (var item in hardwares.Select(x => new { Name = x }))
    {
        exec.ExecuteNonQuery(@"insert into Products(ProductName) values(@Name)", item);
    }
} // when Dispose close connection

// if you want to use transaction, set IsolationLevel at constructor
using (var exec = new DbExecutor(new SqlConnection(connStr), IsolationLevel.ReadCommitted))
{
    // this Insert same as above example's ExecuteNonQuery
    exec.Insert("Products", new { ProductName = "Nintendo DS" });

    // call TransactionComplete then "Commit"
    // if not called TransactionComplete and called Dispose then "Rollback"
    exec.TransactionComplete();
}

/* more api information see Codeplex site. */

// notice for build from source code

source code marked contracts.
need Code Contracts binary rewriter.

unit test written by MSTest.
need Visual Studio Professional or over.

testing library downloaded by NuGet.
* ChainingAssertion
* EntityFramework
* Sql Server Compact

// history

2011-04-30 ver 2.0.0.1
    Fix ExecuteScalar remove Contracts result != null

2011-04-29 ver 2.0.0.0
    Full scratch renew

2010-04-08 ver 1.0.0.1
    Name Change(Executer -> Executor)

2010-04-07 ver 1.0.0.0
    1st Release