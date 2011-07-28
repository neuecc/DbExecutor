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
        private static Func<IDbConnection> connectionFactory;

        [ClassInitialize]
        public static void Setup(TestContext tc)
        {
            var connStr = new CSharpStructure().Database.Connection.ConnectionString;
            connectionFactory = () => new SqlConnection(@"Data Source=SQL2008;Initial Catalog=CSharpTypeStructure;Integrated Security=True");
        }

        [TestMethod]
        public void ExecuteReader()
        {
            var executor = new DbExecutor(connectionFactory());

            var multiple = executor.ExecuteMultiple(@"
select * from Property where TypeId = @TypeId
select * from Type
select * from Property
            ", new { TypeId = 1 });

            var a = multiple.ExecuteReader(dr => new { TypeId = dr.GetInt32(0), Name = dr.GetString(1) });
            var b = multiple.ExecuteReader(dr => new { Id = dr.GetInt32(0), Name = dr.GetString(1) });
            var c = multiple.ExecuteReader(dr => new { TypeId = dr.GetInt32(0), Name = dr.GetString(1) });

            var d = a;
        }

        [TestMethod]
        public void ExecuteReaderDynamic()
        {
            var executor = new DbExecutor(connectionFactory());

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

        [TestMethod]
        public void ExecuteScalar()
        {
            var executor = new DbExecutor(connectionFactory());

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

        public class Property
        {
            public int TypeId { get; set; }
            public string Name { get; set; }
        }

        public class Type
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void Select()
        {
            var executor = new DbExecutor(connectionFactory());

            var multiple = executor.ExecuteMultiple(@"
select * from Property where TypeId = @TypeId
select * from Type
            ", new { TypeId = 1 });

            var a = multiple.Select<Property>();
            var b = multiple.Select<Type>();

            var dz = a;


        }

        [TestMethod]
        public void SelectDynamic()
        {
            var executor = new DbExecutor(connectionFactory());

            var multiple = executor.ExecuteMultiple(@"
select * from Property where TypeId = @TypeId
select * from Type
            ", new { TypeId = 1 });

            var a = multiple.SelectDynamic();
            var b = multiple.SelectDynamic();

            var dz = a;
        }
    }
}
