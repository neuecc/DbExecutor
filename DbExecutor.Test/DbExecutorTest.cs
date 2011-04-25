using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlServerCe;
using Codeplex.Data;
using System.Data;
using System.Diagnostics.Contracts;

namespace DbExecutorTest
{
    [TestClass]
    public class DbExecutorTest
    {
        public class MyClass : IEquatable<MyClass>
        {
            public string Hoge { get; set; }
            public int Huga { get; set; }
            public string Tako { get; set; }

            public static string TableName
            {
                get { return "MyClass"; }
            }

            public static string CreateTableString
            {
                get
                {
                    return string.Format(@"
create table {0}
(
    Hoge nvarchar(10),
    Huga int,
    Tako nvarchar(10)
)
", TableName);
                }
            }

            public bool Equals(MyClass other)
            {
                return this.Hoge == other.Hoge
                    && this.Huga == other.Huga
                    && this.Tako == other.Tako;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                return (obj is MyClass)
                    ? Equals((MyClass)obj)
                    : false;
            }

            public override int GetHashCode()
            {
                return Hoge.GetHashCode() + Huga.GetHashCode() + Tako.GetHashCode();
            }
        }

        public static MyClass[] Data = new[]
        {
            new MyClass{ Hoge = "hoge", Huga = 100, Tako = "yaki"},
            new MyClass{ Hoge = "hage", Huga = -100, Tako = "ika"},
        };


        public TestContext TestContext { get; set; }
        private static string connectionString;
        private static IDbConnection CreateConnection()
        {
            return new SqlCeConnection(connectionString);
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext tc)
        {
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetUnwind();
                Assert.Fail(e.FailureKind.ToString() + ":" + e.Message);
            };
        }

        [ClassInitialize]
        public static void Setup(TestContext tc)
        {
            connectionString = new SqlCeConnectionStringBuilder()
            {
                DataSource = Path.Combine(tc.TestDir, "testdb.sdf")
            }.ToString();

            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }

            using (var exec = new DbExecutor(CreateConnection(), IsolationLevel.ReadCommitted))
            {
                exec.ExecuteNonQuery(MyClass.CreateTableString);
                Array.ForEach(Data, mc => exec.Insert(MyClass.TableName, mc));

                exec.TransactionComplete();
            }
        }

        [TestMethod]
        public void ExecuteReader()
        {
            using (var exec = new DbExecutor(CreateConnection()))
            {
                var r = exec.ExecuteReader("select * from MyClass where Huga = @Huga", new { Huga = 100 })
                    .Select(dr => new MyClass
                    {
                        Hoge = (string)dr["Hoge"],
                        Huga = (int)dr["Huga"],
                        Tako = (string)dr["Tako"]
                    })
                    .ToArray();
                r.Length.Is(1);
                r[0].Is(Data[0]);
            }

            DbExecutor.ExecuteReader(CreateConnection(), "select * from MyClass")
                .Select(dr => new MyClass
                {
                    Hoge = (string)dr["Hoge"],
                    Huga = (int)dr["Huga"],
                    Tako = (string)dr["Tako"]
                })
                .Is(Data);
        }

        [TestMethod]
        public void ExecuteReaderDynamic()
        {
            using (var exec = new DbExecutor(CreateConnection()))
            {
                var r = exec.ExecuteReaderDynamic("select * from MyClass where Huga = @Huga", new { Huga = 100 })
                    .Select(d => new MyClass
                    {
                        Hoge = d.Hoge,
                        Huga = d.Huga,
                        Tako = d.Tako
                    })
                    .ToArray();
                r.Length.Is(1);
                r[0].Is(Data[0]);
            }

            DbExecutor.ExecuteReaderDynamic(CreateConnection(), "select * from MyClass")
                .Select(d => new MyClass
                {
                    Hoge = d.Hoge,
                    Huga = d.Huga,
                    Tako = d.Tako
                })
                .Is(Data);
        }

        [TestMethod]
        public void ExecuteNonQuery()
        {
            using (var exec = new DbExecutor(CreateConnection(), IsolationLevel.ReadCommitted))
            {
                var affected = exec.ExecuteNonQuery("insert into MyClass values(@Hoge, @Huga, @Tako)",
                    new MyClass
                    {
                        Hoge = "buhi",
                        Huga = -9999,
                        Tako = "osaka"
                    });
                affected.Is(1);

                var f = exec.Select<MyClass>("select * from MyClass where Huga = -9999").First();
                f.Is(new MyClass
                {
                    Hoge = "buhi",
                    Huga = -9999,
                    Tako = "osaka"
                });

                // Transaction Uncommit
            }

            // Transaction Rollback test.
            DbExecutor.Select<MyClass>(CreateConnection(), "select * from MyClass where Huga = -9999")
                .Count()
                .Is(0);
        }

        [TestMethod]
        public void ExecuteScalar()
        {
            using (var exec = new DbExecutor(CreateConnection(), IsolationLevel.ReadCommitted))
            {
                var r = exec.ExecuteScalar<int>("select max(Huga) from MyClass where Huga > @Huga", new { Huga = 0 });
                r.Is(100);
            }

            var dt = DbExecutor.ExecuteScalar<DateTime>(CreateConnection(), "select GETDATE()");
            dt.Day.Is(DateTime.Now.Day);
        }

        [TestMethod]
        public void Select()
        {

        }

        [TestMethod]
        public void SelectDynamic()
        {

        }

        [TestMethod]
        public void Delete()
        {

        }

        [TestMethod]
        public void Insert()
        {


        }

        [TestMethod]
        public void Update()
        {
            DbExecutor.Update(CreateConnection(), "MyClass",
                new { Hoge = "hoge", Huga = 100, Tako = "yaki" },
                new { Huga = 10000, Tako = "YAKI!" });
        }
    }
}
