# DbExecutor
Simple and Lightweight Database Executor

Info
---
Archive, import from Codeplex.

Features
---
* Set command parameter by AnonymousType
* Micro-ORM(Select) and Insert/Delete/Update support
* Dynamic IDataRecord accessor
* Easy transaction
* Support Code Contracts
* Available NuGet install Install-Package [DbExecutor](http://www.nuget.org/List/Packages/DbExecutor)

Standard Usage
---
```csharp
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
```

API List
---
All api have same static method and instance method.

Basic ADO.NET DbCommand Wrapper
---

* ExecuteReader
* ExecuteReaderDynamic
* ExecuteNonQuery
* ExecuteScalar<T>

```csharp
// connection string
var connStr = @"Data Source=NORTHWIND";

// ExecuteReader is simple ExecuteReader wrapper and returns IEnumerable<IDataRecord>
// It means lazy evaluation and can use Linq to Objects.
var products1 = DbExecutor.ExecuteReader(new SqlConnection(connStr), @"
        select ProductName, QuantityPerUnit from Products
        where SupplierID = @SupplierID and UnitPrice > @UnitPrice
        ", new { SupplierID = 1, UnitPrice = 10 })
    .Select(dr => new
    {
        ProductName = (string)dr["ProductName"],
        QuantityPerUnit = (string)dr["QuantityPerUnit"]
    })
    .ToArray();

// ExecuteReaderDynamic is similar with ExecuteReader
// A diffrence is returns IEnumerable<dynamic>
// dynamic is IDataRecord wrapper by DynamicObject.
// It can access column by dot syntax and unnecessary type cast.
var products2 = DbExecutor.ExecuteReaderDynamic(new SqlConnection(connStr), @"
        select ProductName, QuantityPerUnit from Products
        where SupplierID = @SupplierID and UnitPrice > @UnitPrice
        ", new { SupplierID = 1, UnitPrice = 10 })
    .Select(d => new Product
    {
        ProductName = d.ProductName,
        QuantityPerUnit = d.QuantityPerUnit
    })
    .ToArray();

// ExecuteNonQuery is simple ExecuteNonQuery wrapper
DbExecutor.ExecuteNonQuery(new SqlConnection(connStr), @"
        insert into Products(ProductName, QuantityPerUnit)
        values (@ProductName, @QuantityPerUnit)
    ", new { ProductName = "HomuHomu", QuantityPerUnit = "MoguMogu" });

// ExecuteScalar<T> is simple ExecuteScalar wrapper
// T is return type.
var serverTime = DbExecutor.ExecuteScalar<DateTime>(new SqlConnection(connStr), @"
    select GetDate()");
```

Micro-ORM Methods
---

* Select<T>
* SelectDynamic
* Insert
* Delete
* Update

```csharp
// Data Model
public class Product
{
    public string ProductName { get; set; }
    public string QuantityPerUnit { get; set; }
}

// Select returns IEnumerable<T>
// Mapping objects by ColumnName - PropertyName.
var products3 = DbExecutor.Select<Product>(new SqlConnection(connStr), @"
        select ProductName, QuantityPerUnit from Products
        where SupplierID = @SupplierID and UnitPrice > @UnitPrice
        ", new { SupplierID = 1, UnitPrice = 10 })
    .ToArray();

// SelectDynamic returns IEnumerable<dynamic>
// This dynamic is ExpandoObject, all value have mapped by ColumnName.
var products4 = DbExecutor.SelectDynamic(new SqlConnection(connStr), @"
        select ProductName, QuantityPerUnit from Products
        where SupplierID = @SupplierID and UnitPrice > @UnitPrice
        ", new { SupplierID = 1, UnitPrice = 10 })
    .ToArray();

// Insert is simple insert query builder(and execute)
DbExecutor.Insert(new SqlConnection(connStr), "Products",
    new Product { ProductName = "Homu2", QuantityPerUnit = "MamiMami" });

// above method convert to following query
insert into Products(ProductName, QuantityPerUnit)
values(ProductName = @ProductName, QuantityPerUnit = @QuantityPerUnit)

// Update is simple update query builder(and execute)
DbExecutor.Update(new SqlConnection(connStr), "Products",
    new { ProductName = "what" }), // update target
    new { ProductName = "howhow", SupplierID = 100 }); // where condition

// above method convert to following query
// __extra__ is thing to distinguish target from where
// where condition is bound with "and" operator
update Products set ProductName = @ProductName
where ProductName = @__extra__ProductName
  and SupplierID = @__extra__SupplierID

// Delete is simple update query builder(and execute)
// where condition is bound with "and" operator
DbExecutor.Delete(new SqlConnection(connStr), "Products",
    new { ProductName = "anything!" });

// above method convert to following query
delete from Products where ProductName = @ProductName
```

StoredProcedure
---

```csharp
// if you want to execute stored procedure then use CommandType.StoredProcedure
var twoyears = exec.SelectDynamic("Sales by Year",
        new { Beginning_Date = "1996-1-1", Ending_Date = "1997-12-31" },
        CommandType.StoredProcedure)
    .ToArray();
```

Tips of ExecuteReaderDynamic
---
dynamic is can watch  all column name and value and type in debugger dynamic view.

![image](https://cloud.githubusercontent.com/assets/46207/24585000/3375d732-17bb-11e7-9ce6-09ab70dbe4b3.png)
