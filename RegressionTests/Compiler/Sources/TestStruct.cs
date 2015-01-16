using System;
using System.Runtime.CompilerServices;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestStruct : TestCase
    {
        private MyStruct myInstanceStruct;  
        private static MyStruct myStaticStruct;

        public void testDefaultCtor1()
        {
			MyStruct x = new MyStruct();
            AssertEquals(x.x, 0);
        }

        public void testDefaultCtor2()
        {
            myInstanceStruct = new MyStruct();
            AssertEquals(myInstanceStruct.x, 0);
        }

        public void testDefaultCtor3()
        {
            myStaticStruct = new MyStruct();
            AssertEquals(myStaticStruct.x, 0);
        }

        public void testCtor1()
        {
			MyStruct x = new MyStruct(25);
            AssertEquals(x.x, 25);
        }

        public void testCtor2()
        {
            myInstanceStruct = new MyStruct(25);
            AssertEquals(myInstanceStruct.x, 25);
        }

        public void testCtor3()
        {
            myStaticStruct = new MyStruct(25);
            AssertEquals(myStaticStruct.x, 25);
        }

        public void testCtor4()
        {
            var cls = new StructInside();

            var a = cls.getA();
            AssertEquals(42, a);

            var a2 = cls.getA2();
            AssertEquals(0, a2);
        }

        public void testCtor5()
        {
            var cls = new StructInside2();

            var a = cls.getA();
            AssertEquals(42, a);
        }

        public void testSetField1()
        {
            MyStruct x = new MyStruct(25);
            x.x = 42;
            AssertEquals(x.x, 42);
        }

        public void testSetField2()
        {
            myInstanceStruct = new MyStruct(25);
            myInstanceStruct.x = 42;
            AssertEquals(myInstanceStruct.x, 42);
        }

        public void testSetField3()
        {
            myStaticStruct = new MyStruct(25);
            myStaticStruct.x = 42;
            AssertEquals(myStaticStruct.x, 42);
        }

        public void testMyStruct2()
        {
            var s = new MyStruct2(25);
            AssertEquals(25, s.x);
            AssertEquals(-1, s.y);
        }

        public void testMyStruct3()
        {
            var s = new MyStruct3();
            AssertNotNull(s);
        }

        public void testMyStruct4()
        {
            var s = new MyStruct4(42);
            AssertNotNull(s);
            AssertEquals(42, s.MyInt);
        }

        public void testMyStruct5()
        {
            var s = new MyStruct5();
            var r = s.ReturnThis();
            AssertNotNull(r);
        }

        public void testMyStruct6()
        {
            var s = new MyStruct6();
            var r = s.ReturnThis();
            AssertNotNull(r);
        }

        public void testMyClass1()
        {
            var c = new MyClass();
            var r = c.MyStruct;
            AssertNotNull(r);
        }

        public void testMyStaticClass1()
        {
            var r = MyStaticClass.MyStruct;
            AssertNotNull(r);
        }

        public void testMyStructInArray()
        {
            var array = new MyStruct[1];

            array[0].x = 42;

            AssertNotNull(array);
            AssertNotNull(array[0]);
            AssertEquals(42, array[0].x);
        }

        public void testJP2PaletteParams()
        {
            var paletteParams = JP2PaletteParams.EMPTY;

            AssertNotNull(paletteParams);
        }

        public void testMyStruct7()
        {
            var ms7 = new MyStruct7<MyClass>( new MyClass());
            var dummy = ms7.Dummy;
            AssertNotNull(dummy);
        }

        public void testMyStruct8()
        {
            var ms8 = new MyClass8();
            Test8( out ms8.str.x );
            AssertEquals(66, ms8.str.x);
        }

        public static void Test8(out uint x)
        {
            x = 66;
        }

        public void _testMyStruct9Struct()
        {
            var mc9 = new MyClass9<MyStruct>();
            AssertEquals(16, mc9.array.Length);

            var myStruct = mc9.array[0];
            AssertNotNull(myStruct);
        }

        public void _testMyStruct9Int()
        {
            var mc9 = new MyClass9<int>();
            AssertEquals(16, mc9.array.Length);

            var myStruct = mc9.array[0];
            AssertNotNull(myStruct);
        }

        public void testMyStruct9Class()
        {
            var mc9 = new MyClass9<MyClass>();
            AssertEquals(16, mc9.array.Length);

            var myClass = mc9.array[0];
            AssertNull(myClass);
        }

        public void testMyStruct10()
        {
            var mc10 = new MyClass10();
            mc10.FooNotInitialized();
            AssertTrue(true);
        }

        public void test11()
        {
            var myClass11 = new MyClass11();

            AssertNotNull(myClass11);
        }

        public void test12()
        {
            var myStruct = new MyStruct11a();

            AssertNotNull(myStruct);
            AssertNotNull(myStruct.myInnerStruct);
            AssertSame(0, myStruct.myInnerStruct.myT);
        }

        public void test13()
        {
            MyStruct12Outer outer;
            outer.Inner = new MyStruct12Inner();

            AssertNotNull(outer);
            AssertNotNull(outer.Inner);
            AssertSame(0, outer.Inner.Dummy);
        }

        public void test14()
        {
            var myStruct = new MyStruct14();
            var myNotifyCompletion = new MyNotifyCompletion();
            var myAsyncStateMachine = new MyAsyncStateMachine();

            myStruct.AwaitOnCompleted(ref myNotifyCompletion, ref myAsyncStateMachine);

            AssertNotNull(myStruct);
            AssertTrue(myAsyncStateMachine.Called);
        }

        public void test15()
        {
            var myStruct15 = new MyStruct15();
            myStruct15.MyClass = new MyClass15();

            var result = Invoke15(ref myStruct15);
            AssertSame(42, result);
        }

        internal int Invoke15(ref MyStruct15 myStruct)
        {
            return myStruct.MyClass.MyInt;
        }

        public void test16()
        {
            var myStruct = new MyStruct16();
            myStruct.DoIt();

            AssertNotNull(myStruct);
            AssertTrue(myStruct.Called);
        }
    }



    public class MyClass11
    {
        public static MyStruct zeroed_object;
    }

    internal class StructInside
    {
        private StructCtr structCtr = new StructCtr(42);
        private StructCtr structCtr2;

        public int getA()
        {
            return structCtr.a;
        }

        public int getA2()
        {
            return structCtr2.a;
        }
    }

    internal class StructInside2
    {
        private StructCtr structCtr = new StructCtr(42);
        private StructCtr structCtr2;

        public int getA()
        {
            return structCtr.a;
        }
    }

    public struct StructCtr
    {
        public StructCtr(int initA)
        {
            a = initA;
        }

        internal int a;
    }	

    public class MyClass9<T>
    {
        public MyClass9()
        {
           array = new T[16];
        }

        public T[] array;
    }

    public class MyClass
    {
        public MyStruct MyStruct;
    }

    public class MyClass10
    {
        public void FooNotInitialized()
        {
            MyStruct ms;
            ms.x = 10;
        }
    }

    public static class MyStaticClass
    {
        public static MyStruct MyStruct;
    }

	public struct MyStruct 
	{
		public int x;
		public MyStruct(int x) 
		{
			this.x = x;
		}
	}

    public struct MyStruct2
    {
        public int x;
        public int y;

        public MyStruct2(int x)
            :this(x, -1)
        {
        }

        public MyStruct2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct MyStruct3
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct MyStruct4
    {
        public int MyInt { get; private set; }

        public MyStruct4(int myInt)
        {
            var st4 = new MyStruct4();
            st4.MyInt = myInt;
            this = st4;
        }
    }

    public struct MyStruct5
    {
        public MyStruct5 ReturnThis()
        {
            return this;
        }
    }

    public struct MyStruct6
    {
        private bool returnThis;

        public MyStruct6 ReturnThis()
        {
            if (returnThis)
                return this;

            return new MyStruct6();
        }
    }

    internal struct JP2PaletteParams
    {
        public static JP2PaletteParams EMPTY = new JP2PaletteParams();
    }

    internal struct MyStruct7<T> where T : class
    {
        public MyStruct7(T dummy)
        {
            this = new MyStruct7<T> {Dummy = dummy};
        }

        public T Dummy { get; private set; }
    }

    public class MyClass8
    {
        public MyStruct8 str = new MyStruct8() { x = 88 };
    }

    public struct MyStruct8
    {
        public uint x;
    }

    public struct MyStruct11a
    {
        public MyStruct11b<int> myInnerStruct;
    }

    public struct MyStruct11b<T>
    {
        public T myT;
    }

    public struct MyStruct12Outer
    {
        public MyStruct12Inner Inner;
    }

    public struct MyStruct12Inner
    {
        public int Dummy;
    }

    public struct MyStruct14
    {
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var action = new Action(stateMachine.MoveNext);
            awaiter.OnCompleted(action);
        }
    }

    public struct MyNotifyCompletion: INotifyCompletion
    {
        public void OnCompleted(Action continuation)
        {
            continuation.Invoke();
        }
    }

    public struct MyAsyncStateMachine: IAsyncStateMachine
    {
        public bool Called;

        public void MoveNext()
        {
            Called = true;
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            //nothing to do...
        }
    }

    public struct MyStruct15
    {
        public MyClass15 MyClass;
    }

    public class MyClass15
    {
        public int MyInt = 42;
    }

    public interface IFoo16
    {
        void Foo();
    }

    public struct MyStruct16 : IFoo16
    {
        public bool Called;

        public void Foo()
        {
            Called = true;
        }

        public void FooCaller<T>(ref T fooer)
            where T : IFoo16
        {
            fooer.Foo();
        }

        public void DoIt()
        {
            FooCaller(ref this);
        }
    }
}
