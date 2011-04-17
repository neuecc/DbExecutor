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
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }
        private string connectionString;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext tc)
        {
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetUnwind();
                Assert.Fail(e.FailureKind.ToString() + ":" + e.Message);
            };
        }

        [TestInitialize]
        public void Setup()
        {
            var exec2 = new DbExecutor(null);

            connectionString = new SqlCeConnectionStringBuilder()
            {
                DataSource = Path.Combine(TestContext.TestDir, "testdb.sdf")
            }.ToString();

            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }
            using (var exec = new DbExecutor(new SqlCeConnection(connectionString), IsolationLevel.ReadCommitted))
            {
                exec.ExecuteNonQuery(@"
create table TestTable(
    Hoge nvarchar(10),
    Huga int,
    Tako nvarchar(10)
)");

                exec.InsertTo("TestTable", new
                {
                    Hoge = "aiueo",
                    Huga = 100,
                    Tako = "takotako"
                });

                exec.InsertTo("TestTable", new
                {
                    Hoge = "なのこんぼ",
                    Huga = -1000,
                    Tako = "jigokuno"
                });

                exec.TransactionComplete();
            }
        }

        [TestMethod]
        public void TestMethod1()
        {

            
            

            using (var exec = new DbExecutor(new SqlCeConnection(connectionString)))
            {
                var r = exec.ExecuteReader("select * from TestTable")
                    .Select(dr => new
                    {
                        Hoge = dr.GetString(0),
                        Huga = dr.GetInt32(1),
                        Tako = dr.GetString(2)
                    })
                    .ToArray();
            }
        }
    }
}
