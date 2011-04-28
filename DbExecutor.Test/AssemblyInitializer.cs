using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;

namespace DbExecutorTest
{
    // codefirst model

    public class Type
    {
        public int TypeId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Method> Properties { get; set; }

        public override string ToString()
        {
            return string.Format(@"{{TypeId = {0}, Name = {1}}}", TypeId, Name);
        }
    }

    public class Method
    {
        public int TypeId { get; set; }
        public int MethodId { get; set; }
        public string Name { get; set; }

        public virtual Type Type { get; set; }
    }

    public class CSharpStructure : DbContext
    {
        public DbSet<Type> Types { get; set; }
        public DbSet<Method> Methods { get; set; }
    }

    [TestClass]
    public class AssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetUnwind();
                Assert.Fail(e.FailureKind.ToString() + ":" + e.Message);
            };

            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
            Database.SetInitializer(new DropCreateDatabaseAlways<CSharpStructure>());

            using (var cx = new CSharpStructure())
            {
                var types = new[]
                {
                    new Type{Name = "Int32"},
                    new Type{Name = "String"},
                    new Type{Name = "ListOfT"},
                    new Type{Name = "DictionaryOfTOfT"},
                };
                foreach (var item in types) cx.Types.Add(item);

                var methods = new[]
                {
                    new Method{Name = "CompareTo", Type = types[0]},
                    new Method{Name = "StartsWith", Type= types[1]},
                    new Method{Name = "EndsWith", Type= types[1]},
                    new Method{Name = "Contains", Type= types[1]},
                    new Method{Name = "TrueForAll", Type= types[2]},
                    new Method{Name = "ForEach", Type= types[2]},
                    new Method{Name = "ContainsKey", Type= types[3]},
                    new Method{Name = "TryGetValue", Type= types[3]},
                };
                foreach (var item in methods) cx.Methods.Add(item);

                cx.SaveChanges();
            }
        }
    }
}
