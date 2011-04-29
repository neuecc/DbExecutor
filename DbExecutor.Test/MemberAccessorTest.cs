using System;
using System.Linq;
using Codeplex.Data.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var accessors = AccessorCache.Lookup(typeof(TestMockClass))
                .OrderBy(x => x.Name)
                .ToArray();

            accessors.Length.Is(10);
            accessors.All(a => a.DeclaringType == typeof(TestMockClass));
            accessors.Count(a => a.IsReadable).Is(7);
            accessors.Count(a => a.IsWritable).Is(7);
            accessors.Select(a => a.Name).Is("Field1", "Field2", "Property1", "Property2", "Property3", "Property4", "Property5", "Property6", "PropertyReadOnly", "PropertySetOnly");

            var target = new TestMockClass();
            accessors[0].SetValue(target, "a");
            accessors[2].SetValue(target, "b");
            accessors[4].SetValue(target, "c");
            accessors[6].IsWritable.Is(false);
            accessors[1].SetValue(target, 1);
            accessors[3].SetValue(target, 2);
            accessors[5].SetValue(target, 3);
            accessors[7].IsWritable.Is(false);
            accessors.Where(a => a.IsReadable).Select(a => a.GetValue(target))
                .Is("a", 1, "b", 2, null, 0, null);
            Enumerable.Repeat((target as TestMockClass), 1)
                .SelectMany(m => new object[] { m.Field1, m.Field2, m.Property1, m.Property2, m.Property5, m.Property6 })
                .Is("a", 1, "b", 2, null, 0);

            var read = accessors[8];
            read.IsReadable.Is(true);
            read.IsWritable.Is(false);

            var write = accessors[9];
            write.IsReadable.Is(false);
            write.IsWritable.Is(true);
            write.SetValue(target, "test");
            Assert.AreEqual("test", (target as TestMockClass).AsDynamic().hiddenField);
        }

        [TestMethod]
        public void StructTest()
        {
            var accessors = AccessorCache.Lookup(typeof(TestMockStruct))
                .OrderBy(x => x.Name)
                .ToArray();

            accessors.Length.Is(10);
            accessors.All(a => a.DeclaringType == typeof(TestMockStruct));
            accessors.Take(8).All(a => a.IsReadable);
            accessors.Take(8).All(a => a.IsWritable);
            accessors.Select(a => a.Name).Is("Field1", "Field2", "Property1", "Property2", "Property3", "Property4", "Property5", "Property6", "PropertyReadOnly", "PropertySetOnly");

            object target = new TestMockStruct();
            accessors[0].SetValue(target, "a");
            accessors[2].SetValue(target, "b");
            accessors[4].SetValue(target, "c");
            accessors[6].IsWritable.Is(false);
            accessors[1].SetValue(target, 1);
            accessors[3].SetValue(target, 2);
            accessors[5].SetValue(target, 3);
            accessors[7].IsWritable.Is(false);
            accessors.Where(a => a.IsReadable).Select(a => a.GetValue(target))
                .Is("a", 1, "b", 2, null, 0, null);
            Enumerable.Repeat((TestMockStruct)target, 1)
                .SelectMany(m => new object[] { m.Field1, m.Field2, m.Property1, m.Property2, m.Property5, m.Property6 })
                .Is("a", 1, "b", 2, null, 0);

            var read = accessors[8];
            read.IsReadable.Is(true);
            read.IsWritable.Is(false);

            var write = accessors[9];
            write.IsReadable.Is(false);
            write.IsWritable.Is(true);
            write.SetValue(target, "test");
            Assert.AreEqual("test", ((TestMockStruct)target).AsDynamic().hiddenField);
        }

        [TestMethod]
        public void AnonymousType()
        {
            var anon = new { P1 = "HOGE", P2 = 1000 };
            var xs = anon.GetType().GetProperties().Select(p => new ExpressionAccessor(p)).OrderBy(x => x.Name).ToArray();

            xs.Length.Is(2);
            xs.Select(a => a.DeclaringType).All(t => t == anon.GetType());
            xs.All(a => a.IsReadable);
            xs.All(a => !a.IsWritable);

            object an = anon;
            xs[0].GetValue(an).Is("HOGE");
            xs[1].GetValue(an).Is(1000);
        }
    }
}