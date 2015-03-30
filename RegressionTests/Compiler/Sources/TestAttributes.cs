using System;
using System.Linq;
using Android;
using Javax.Security.Auth.X500;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestAttributes : TestCase
    {
        public void testClassAttributes1()
        {
            var type = typeof(TestClass);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 2, attr.Length);
        }

        public void testClassAttributes2()
        {
            var type = typeof(TestClass2);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 1, attr.Length);

            AssertEquals("#x", 5, ((MyAttribute)attr[0]).GetX());
            AssertEquals("#y", 0, ((MyAttribute)attr[0]).GetY());
        }

        public void testClassAttributes3()
        {
            var type = typeof(TestClass3);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes should be 1", 1, attr.Length);

            AssertEquals("#x", 7, ((MyAttribute)attr[0]).GetX());
            AssertEquals("#y", 7, ((MyAttribute)attr[0]).GetY());
        }

        public void testClassAttributes4()
        {
            var type = typeof(TestClass);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 2, attr.Length);

            var first = (MyAttribute)attr[0];
            var second = (MyAttribute)attr[1];
            if (first.GetX() == 5) AssertEquals("#y0", 0, first.GetY());
            else AssertEquals("#y0", 7, first.GetY());
            if (second.GetX() == 7) AssertEquals("#y0", 7, second.GetY());
            else AssertEquals("#y0", 0, second.GetY());
        }

        public void testClassAttributesWithProperties1()
        {
            var type = typeof(TestClassProp1);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 1, attr.Length);
            AssertEquals("type", typeof(MyAttributeWithProperties), attr.First().GetType());
            AssertEquals("name", "MyAttributeWithProperties", attr.First().GetType().Name);
        }

        public void testClassAttributesWithProperties2()
        {
            var type = typeof(TestClassProp2);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithProperties), false)
                           .Cast<MyAttributeWithProperties>().ToList();
            AssertEquals("#attributes", 1, attr.Count);
            AssertEquals("name", "MyAttributeWithProperties", attr.First().GetType().Name);
        }

        public void testClassAttributesWithProperties3()
        {
            var type = typeof(TestClassProp1);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithProperties), false)
                           .Cast<MyAttributeWithProperties>().First();
            AssertEquals("X", 0, attr.X);
            AssertEquals("Y", 0, attr.Y);
        }


        public void testClassAttributesWithProperties4()
        {
            var type = typeof(TestClassProp2);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithProperties), false)
                           .Cast<MyAttributeWithProperties>().First();
            AssertEquals("X", 5, attr.X);
            AssertEquals("Y", 0, attr.Y);
        }

        public void testClassAttributesWithProperties5()
        {
            var type = typeof(TestClassProp3);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithProperties), false)
                           .Cast<MyAttributeWithProperties>().First();
            AssertEquals("X", 5, attr.X);
            AssertEquals("Y", 7, attr.Y);
        }

        public void testPropertyAttributesOn()
        {
            var type = typeof(TestClassOnProp1);
            var prop = type.GetProperty("Property");
            var attr = prop.GetCustomAttributes(false);

            AssertEquals("#number", 1, attr.Length);
            AssertEquals("X", 5, ((MyAttribute)attr[0]).GetX());
        }

        public void testInheritedAttributes()
        {
            var type = typeof(TestClassInheritedAttribute);
            var attrs = type.GetCustomAttributes(false);

            AssertEquals("#number", 1, attrs.Length);

            var attr = (MyInheritedAttribute)attrs[0];
            AssertEquals("Z", 9, attr.Z);
            AssertEquals("X", 5, attr.X);
            AssertEquals("Y", 7, attr.Y);

        }

        public void testInheritedAttributesGetThroughBase()
        {
            var type = typeof(TestClassInheritedAttribute);
            var attrs = type.GetCustomAttributes(typeof(MyAttributeWithProperties), false);

            AssertEquals("#number", 1, attrs.Length);
            AssertEquals("type", typeof(MyInheritedAttribute), attrs[0].GetType());

            var attr = (MyInheritedAttribute)attrs[0];
            AssertEquals("X", 5, attr.X);
            AssertEquals("Y", 7, attr.Y);
            AssertEquals("Z", 9, attr.Z);
        }

        public void testInheritedAttributesWithAbstractBase()
        {
            var type = typeof(TestClassInheritedFromAbstractAttribute);
            var attrs = type.GetCustomAttributes(typeof(MyAbstractAttribute), false);

            AssertEquals("#number", 1, attrs.Length);
            AssertEquals("type", typeof(MyInheritedAttributeFromAbstract), attrs[0].GetType());
            var attr = (MyInheritedAttributeFromAbstract)attrs[0];

            AssertEquals("Unique", true, attr.Unique);
            AssertEquals("Order", 2, attr.Order);
            AssertEquals("Name", "Cool", attr.Name);
        }

        public void testAttributesWithString()
        {
            var type = typeof(TestClassWithString);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithString), false)
                            .Cast<MyAttributeWithString>().First();
            AssertEquals("Val", "ABC", attr.Val);
        }

        public void testAttributesWithNullString()
        {
            var type = typeof(TestClassWithNullString);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithString), false)
                            .Cast<MyAttributeWithString>().First();
            AssertEquals("Val", null, attr.Val);
        }


        public void testAttributesWithArrays()
        {
            var type = typeof(TestClassWithArrays);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithArray), false)
                            .Cast<MyAttributeWithArray>().First();
            AssertEquals("Val1", null, attr.StringVals);
            AssertEquals("IVal-len", 2, attr.IVals.Length);
            AssertEquals("IVal-0", 5, attr.IVals[0]);
            AssertEquals("IVal-1", 7, attr.IVals[1]);
            AssertEquals("Types-1", typeof(Nullable<>), attr.Types[1]);
        }

        public void testAttributesWithAllKinds()
        {
            var type = typeof(TestClassAllKinds);
            var attr = type.GetCustomAttributes(typeof(MyAttributeWithAllKindsOfValues), false)
                            .Cast<MyAttributeWithAllKindsOfValues>().First();
            AssertEquals("Val1", null, attr.Val1);
            AssertEquals("Val2", (byte)255, attr.Val2);
            AssertEquals("Val3", (sbyte)-2, attr.Val3);
            AssertEquals("Val4", (short)-2, attr.Val4);
            AssertEquals("Val5", ushort.MaxValue, attr.Val5);
            AssertEquals("Val6", (long)-2, attr.Val6);
            AssertEquals("Val7", ulong.MaxValue, attr.Val7);
            AssertEquals("Val8", -2f, attr.Val8);
            AssertEquals("Val9", -2.0, attr.Val9);
        }



        [MyAttribute(5)]
        [MyAttribute(7, 7)]
        public class TestClass { }

        [MyAttribute(5)]
        public class TestClass2 { }

        [MyAttribute(7, 7)]
        public class TestClass3 { }

        [MyAttributeWithProperties]
        public class TestClassProp1 { }

        [MyAttribute(5)]
        [MyAttributeWithProperties(X = 5)]
        public class TestClassProp2 { }

        [MyAttributeWithProperties(X = 5, Y = 7)]
        public class TestClassProp3 { }

        [IncludeType]
        public class TestClassOnProp1
        {
            [MyAttribute(5)]
            public int Property { get; set; }
        }

        [IncludeType]
        [MyInheritedAttribute(X = 5, Y = 7, Z = 9)]
        public class TestClassInheritedAttribute
        {
        }

        [IncludeType]
        [MyInheritedAttributeFromAbstract(Name = "Cool", Order = 2)]
        public class TestClassInheritedFromAbstractAttribute
        {
        }

        [IncludeType]
        [MyAttributeWithString("ABC")]
        public class TestClassWithString
        {
        }

        [IncludeType]
        [MyAttributeWithString(null)]
        public class TestClassWithNullString
        {
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        public class MyAttributeWithArray : Attribute
        {
            public string[] StringVals { get; set; }
            public int[] IVals { get; set; }
            public Type[] Types { get; set; }

            public MyAttributeWithArray(params Type[] types)
            {
                Types = types;
            }
        }
        [IncludeType]
        [MyAttributeWithArray(typeof(TestAttributes), typeof(Nullable<>), IVals = new[] { 5, 7 })]
        public class TestClassWithArrays
        {
        }

        [IncludeType]
        [MyAttributeWithAllKindsOfValues(Val1 = null, Val2 = 255, Val3 = -2, Val4 = -2,
                                         Val5 = ushort.MaxValue, Val6 = -2, Val7 = ulong.MaxValue,
                                         Val8 = -2f, Val9 = -2.0)]
        public class TestClassAllKinds
        {
        }

    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyAttribute : Attribute
    {
        private readonly int x;
        private readonly int y;

        public MyAttribute(int x)
        {
            this.x = x;
        }

        public MyAttribute(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyAttributeWithProperties : Attribute
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyInheritedAttribute : MyAttributeWithProperties
    {
        public int Z { get; set; }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public abstract class MyAbstractAttribute : Attribute
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public abstract bool Unique { get; set; }

    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyInheritedAttributeFromAbstract : MyAbstractAttribute
    {
        public override bool Unique
        {
            get { return true; }
            set { /* throw?  */ }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyAttributeWithString : Attribute
    {
        public string Val { get; set; }


        public MyAttributeWithString(string val)
        {
            Val = val;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyAttributeWithAllKindsOfValues : Attribute
    {
        public string Val1 { get; set; }
        public byte Val2 { get; set; }
        public sbyte Val3 { get; set; }
        public short Val4 { get; set; }
        public ushort Val5 { get; set; }
        public long Val6 { get; set; }
        public ulong Val7 { get; set; }
        public float Val8 { get; set; }
        public double Val9 { get; set; }
    }

}

