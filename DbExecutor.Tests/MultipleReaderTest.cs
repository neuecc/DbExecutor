using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using Codeplex.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DbExecutorTest
{
    [TestClass]
    public class MultipleReaderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ExecuteReader()
        {
            using (var executor = new DbExecutor(Db.ConnectionFactory()))
            {
                var multiple = executor.ExecuteMultiple(@"
select * from Method where TypeId = @TypeId
select * from Type
select * from Method
            ", new { TypeId = 1 });

                var a = multiple.ExecuteReader(dr => new { TypeId = dr.GetInt32(0), MethodId = dr.GetInt32(1), Name = dr.GetString(2) });
                var b = multiple.ExecuteReader(dr => new { TypeId = dr.GetInt32(0), Name = dr.GetString(1) });
                var c = multiple.ExecuteReader(dr => new { TypeId = dr.GetInt32(0), MethodId = dr.GetInt32(1), Name = dr.GetString(2) });

                var d = a;
            }
        }

        [TestMethod]
        public void ExecuteReaderDynamic()
        {
            using (var executor = new DbExecutor(Db.ConnectionFactory()))
            {

                var multiple = executor.ExecuteMultiple(@"
select * from Property where TypeId = @TypeId
select * from Type
select * from Property
            ", new { TypeId = 1 });



                var a = multiple.ExecuteReaderDynamic(d => new { d.TypeId, d.Name });
                var b = multiple.ExecuteReaderDynamic(d => new { d.Id, d.Name });
                var c = multiple.ExecuteReaderDynamic(d => new { d.TypeId, d.Name });

                var dz = a;
            }
        }

        [TestMethod]
        public void ExecuteScalar()
        {
            using (var executor = new DbExecutor(Db.ConnectionFactory()))
            {
                var rr = executor.ExecuteScalar<int?>(@"select TypeId from Property  where TypeId = 9999");


                var multiple = executor.ExecuteMultiple(@"
select count(*) from Property where TypeId = @TypeId
select CURRENT_TIMESTAMP
select TypeId from Property  where TypeId = 9999
            ", new { TypeId = 1 });


                var a = multiple.ExecuteScalar<int>();
                var b = multiple.ExecuteScalar<DateTime>();
                var c = multiple.ExecuteScalar<int?>();

                var d = a;
            }
        }

        [TestMethod]
        public void Select()
        {
            using (var executor = new DbExecutor(Db.ConnectionFactory()))
            {

                var multiple = executor.ExecuteMultiple(@"
select * from Method where TypeId = @TypeId
select * from Type
            ", new { TypeId = 1 });

                var a = multiple.Select<Method>();
                var b = multiple.Select<Type>();

                var dz = a;
            }
        }

        [TestMethod]
        public void SelectDynamic()
        {
            using (var executor = new DbExecutor(Db.ConnectionFactory()))
            {
                var multiple = executor.ExecuteMultiple(@"
select * from Method where TypeId = @TypeId
select * from Type
            ", new { TypeId = 1 });

                var a = multiple.SelectDynamic();
                var b = multiple.SelectDynamic();

                var dz = a;
            }
        }
    }
}
