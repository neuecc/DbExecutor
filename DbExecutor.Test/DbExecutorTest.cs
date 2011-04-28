using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using Codeplex.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbExecutorTest
{
    [TestClass]
    public class DbExecutorTest
    {
        public TestContext TestContext { get; set; }
        private static Func<IDbConnection> connectionFactory;

        [ClassInitialize]
        public static void Setup(TestContext tc)
        {
            var connStr = new CSharpStructure().Database.Connection.ConnectionString;
            connectionFactory = () => new SqlCeConnection(connStr);
        }

        [TestMethod]
        public void ExecuteReader()
        {
            var rs = DbExecutor.ExecuteReader(connectionFactory(),
                    "select * from Methods where TypeId = @TypeId", new { TypeId = 2 })
                .Select(dr => new Method
                {
                    Name = (string)dr["Name"],
                    MethodId = (int)dr["MethodId"],
                })
                .ToArray();
            rs.Length.Is(3);
            rs.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");

            using (var exec = new DbExecutor(connectionFactory()))
            {
                var ri = exec.ExecuteReader("select * from Methods where TypeId = @TypeId", new { TypeId = 2 })
                    .Select(dr => new Method
                    {
                        Name = (string)dr["Name"],
                        MethodId = (int)dr["MethodId"],
                    })
                    .ToArray();
                ri.Length.Is(3);
                ri.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");
            }
        }

        [TestMethod]
        public void ExecuteReaderDynamic()
        {
            var rs = DbExecutor.ExecuteReaderDynamic(connectionFactory(),
                    "select * from Methods where TypeId = @TypeId", new { TypeId = 2 })
                .Select(d => new Method
                {
                    Name = d.Name,
                    MethodId = d.MethodId,
                })
                .ToArray();
            rs.Length.Is(3);
            rs.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");

            using (var exec = new DbExecutor(connectionFactory()))
            {
                var ri = exec.ExecuteReaderDynamic("select * from Methods where TypeId = @TypeId", new { TypeId = 2 })
                    .Select(d => new Method
                    {
                        Name = d.Name,
                        MethodId = d.MethodId,
                    })
                    .ToArray();
                ri.Length.Is(3);
                ri.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");
            }
        }

        [TestMethod]
        public void ExecuteNonQuery()
        {
            using (var exec = new DbExecutor(connectionFactory(), IsolationLevel.ReadCommitted))
            {
                var affected = exec.ExecuteNonQuery(
                    "insert into Types(Name) values(@Name)",
                    new { Name = "NewTypeEXECUTE" });
                affected.Is(1);

                var f = exec.Select<Type>("select top 1 * from Types order by TypeId desc").First();
                f.Is(t => t.Name == "NewTypeEXECUTE");

                // Transaction Uncommit
            }

            // Transaction Rollback test.
            var xs = DbExecutor.Select<Type>(connectionFactory(), "select * from Types where TypeId = 5").ToArray();
            xs.Count().Is(0);
        }

        [TestMethod]
        public void ExecuteScalar()
        {
            using (var exec = new DbExecutor(connectionFactory()))
            {
                var r = exec.ExecuteScalar<int>("select max(TypeId) from Types where TypeId <= @TypeId", new { TypeId = 2 });
                r.Is(2);
            }

            var dt = DbExecutor.ExecuteScalar<DateTime>(connectionFactory(), "select GETDATE()");
            dt.Day.Is(DateTime.Now.Day);
        }

        [TestMethod]
        public void Select()
        {
            using (var exec = new DbExecutor(connectionFactory()))
            {
                var r = exec.Select<Type>("select * from Types where TypeId <= @TypeId", new { TypeId = 2 }).ToArray();
                r.Length.Is(2);
                r[0].Is(t => t.TypeId == 1 && t.Name == "Int32");
                r[1].Is(t => t.TypeId == 2 && t.Name == "String");
            }

            var methods = DbExecutor.Select<Method>(connectionFactory(), @"
                    select M.*
                    from Methods M
                    join Types T on M.TypeId = T.TypeId
                    where T.Name = @TypeName
                    ", new { TypeName = "String" })
                .ToArray();
            methods.Length.Is(3);
            methods.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");
        }

        [TestMethod]
        public void SelectDynamic()
        {
            using (var exec = new DbExecutor(connectionFactory()))
            {
                var r = exec.SelectDynamic("select * from Types where TypeId <= @TypeId", new { TypeId = 2 }).ToArray();
                r.Length.Is(2);
                r.Select(x => x.TypeId).Is(1, 2);
                r.Select(x => x.Name).Is("Int32", "String");
            }

            var methods = DbExecutor.SelectDynamic(connectionFactory(), @"
                    select M.*
                    from Methods M
                    join Types T on M.TypeId = T.TypeId
                    where T.Name = @TypeName
                    ", new { TypeName = "String" })
                .ToArray();
            methods.Length.Is(3);
            methods.Select(x => x.Name).Is("StartsWith", "EndsWith", "Contains");
        }

        [TestMethod]
        public void Delete()
        {
            using (var exec = new DbExecutor(connectionFactory(), IsolationLevel.ReadCommitted))
            {
                exec.Select<Type>("select * from Types")
                    .Any(x => x.TypeId == 2)
                    .Is(true);

                exec.Delete("Types", new { TypeId = 2 });

                exec.Select<Type>("select * from Types")
                    .Any(x => x.TypeId == 2)
                    .Is(false);
            }
        }

        [TestMethod]
        public void Insert()
        {
            using (var exec = new DbExecutor(connectionFactory(), IsolationLevel.ReadCommitted))
            {
                exec.Insert("Types", new { Name = "NewType1" });
                exec.TransactionComplete(); // Transaction Commit
            }
            DbExecutor.Insert(connectionFactory(), "Types", new { Name = "NewType2" });

            DbExecutor.Select<Type>(connectionFactory(),
                    "select * from Types where Name like @Name", new { Name = "NewType%" })
                .Count()
                .Is(2);

            DbExecutor.Delete(connectionFactory(), "Types", new { Name = "NewType1" });
            DbExecutor.Delete(connectionFactory(), "Types", new { Name = "NewType2" });

            DbExecutor.Select<Type>(connectionFactory(),
                 "select * from Types where Name like @Name", new { Name = "NewType%" })
             .Count()
             .Is(0);
        }

        [TestMethod]
        public void Update()
        {
            using (var exec = new DbExecutor(connectionFactory(), IsolationLevel.ReadCommitted))
            {
                exec.Select<Type>("select * from Types where TypeId = 1")
                    .First()
                    .Is(x => x.Name == "Int32");

                exec.Update("Types", new { Name = "UpdateName" }, new { TypeId = 1 });

                exec.Select<Type>("select * from Types where TypeId = 1")
                    .First()
                    .Is(x => x.Name == "UpdateName");
            }

            DbExecutor.Select<Type>(connectionFactory(), "select * from Types where TypeId = 1")
                .First()
                .Is(x => x.Name == "Int32");
            
            DbExecutor.Update(connectionFactory(), "Types", new { Name = "UpdateName" }, new { TypeId = 1 });

            DbExecutor.Select<Type>(connectionFactory(), "select * from Types where TypeId = 1")
                .First()
                .Is(x => x.Name == "UpdateName");

            DbExecutor.Update(connectionFactory(), "Types", new { Name = "Int32" }, new { TypeId = 1 });
        }
    }
}
