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
using Codeplex.Data.Internal;

namespace DbExecutorTest
{
    [TestClass]
    public class MemberAccessorTest
    {
        public TestContext TestContext { get; set; }

        public class TestMockClass
        {
            public string Field1;
            public int Field2;
            private string Field3;
            private int Field4;
            public string Property1 { get; set; }
            public int Property2 { get; set; }
            public string Property3 { private get; set; }
            public int Property4 { private get; set; }
            public string Property5 { get; private set; }
            public int Property6 { get; private set; }
            private int Property7 { get; set; }
            private string Property8 { get; set; }

            public string PropertyReadOnly
            {
                get { return null; }
            }

            private string hiddenField;
            public string PropertySetOnly
            {
                set { hiddenField = value; }
            }

            private void _()
            {
                Field3 = "";
                Field4 = 0;
                Console.WriteLine(Field3);
                Console.WriteLine(Field4);
            }
        }

        public struct TestMockStruct
        {
            public string Field1;
            public int Field2;
            private string Field3;
            private int Field4;
            public string Property1 { get; set; }
            public int Property2 { get; set; }
            public string Property3 { private get; set; }
            public int Property4 { private get; set; }
            public string Property5 { get; private set; }
            public int Property6 { get; private set; }
            private int Property7 { get; set; }
            private string Property8 { get; set; }

            public string PropertyReadOnly
            {
                get { return null; }
            }

            private string hiddenField;
            public string PropertySetOnly
            {
                set { hiddenField = value; }
            }

            private void _()
            {
                Field3 = "";
                Field4 = 0;
                Console.WriteLine(Field3);
                Console.WriteLine(Field4);
            }
        }

        [TestMethod]
        public void ClassTest()
        {
            var accessors = Enumerable.Concat(
                    typeof(TestMockClass).GetProperties().Select(pi => new ExpressionAccessor(pi)),
                    typeof(TestMockClass).GetFields().Select(fi => new ExpressionAccessor(fi)))
                .OrderBy(x => x.Name)
                .ToArray();

            accessors.Length.Is(10);
            accessors.All(a => a.DelaringType == typeof(TestMockClass));
            accessors.Take(8).All(a => a.IsReadable);
            accessors.Take(8).All(a => a.IsWritable);
            accessors.Select(a => a.Name).Is("Field1", "Field2", "Property1", "Property2", "Property3", "Property4", "Property5", "Property6", "PropertyReadOnly", "PropertySetOnly");

            object target = new TestMockClass();
            accessors[0].SetValue(ref target, "a");
            accessors[2].SetValue(ref target, "b");
            accessors[4].SetValue(ref target, "c");
            accessors[6].SetValue(ref target, "d");
            accessors[1].SetValue(ref target, 1);
            accessors[3].SetValue(ref target, 2);
            accessors[5].SetValue(ref target, 3);
            accessors[7].SetValue(ref target, 4);
            accessors.Where(a => a.IsReadable).Select(a => a.GetValue(ref target)).Is("a", 1, "b", 2, "c", 3, "d", 4, null);
            Enumerable.Repeat((target as TestMockClass), 1)
                .SelectMany(m => new object[] { m.Field1, m.Field2, m.Property1, m.Property2, m.Property5, m.Property6 })
                .Is("a", 1, "b", 2, "d", 4);

            var read = accessors[8];
            read.IsReadable.Is(true);
            read.IsWritable.Is(false);

            var write = accessors[9];
            write.IsReadable.Is(false);
            write.IsWritable.Is(true);
            write.SetValue(ref target, "test");
            Assert.AreEqual("test", (target as TestMockClass).AsDynamic().hiddenField);
        }

        [TestMethod]
        public void StructTest()
        {
            var accessors = Enumerable.Concat(
                    typeof(TestMockStruct).GetProperties().Select(pi => new ExpressionAccessor(pi)),
                    typeof(TestMockStruct).GetFields().Select(fi => new ExpressionAccessor(fi)))
                .OrderBy(x => x.Name)
                .ToArray();

            accessors.Length.Is(10);
            accessors.All(a => a.DelaringType == typeof(TestMockStruct));
            accessors.Take(8).All(a => a.IsReadable);
            accessors.Take(8).All(a => a.IsWritable);
            accessors.Select(a => a.Name).Is("Field1", "Field2", "Property1", "Property2", "Property3", "Property4", "Property5", "Property6", "PropertyReadOnly", "PropertySetOnly");

            object target = new TestMockStruct();
            accessors[0].SetValue(ref target, "a");
            accessors[2].SetValue(ref target, "b");
            accessors[4].SetValue(ref target, "c");
            accessors[6].SetValue(ref target, "d");
            accessors[1].SetValue(ref target, 1);
            accessors[3].SetValue(ref target, 2);
            accessors[5].SetValue(ref target, 3);
            accessors[7].SetValue(ref target, 4);
            accessors.Where(a => a.IsReadable).Select(a => a.GetValue(ref target)).Is("a", 1, "b", 2, "c", 3, "d", 4, null);
            Enumerable.Repeat((TestMockStruct)target, 1)
                .SelectMany(m => new object[] { m.Field1, m.Field2, m.Property1, m.Property2, m.Property5, m.Property6 })
                .Is("a", 1, "b", 2, "d", 4);

            var read = accessors[8];
            read.IsReadable.Is(true);
            read.IsWritable.Is(false);

            var write = accessors[9];
            write.IsReadable.Is(false);
            write.IsWritable.Is(true);
            write.SetValue(ref target, "test");
            Assert.AreEqual("test", ((TestMockStruct)target).AsDynamic().hiddenField);
        }

        [TestMethod]
        public void AnonymousType()
        {
            var anon = new { P1 = "HOGE", P2 = 1000 };
            var xs = anon.GetType().GetProperties().Select(p => new ExpressionAccessor(p)).OrderBy(x => x.Name).ToArray();

            xs.Length.Is(2);
            xs.Select(a => a.DelaringType).All(t => t == anon.GetType());
            xs.All(a => a.IsReadable);
            xs.All(a => !a.IsWritable);

            object an = anon;
            xs[0].GetValue(ref an).Is("HOGE");
            xs[1].GetValue(ref an).Is(1000);
        }
    }
}