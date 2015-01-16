using System;
using System.Diagnostics;
using Android.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestEnumPerformance : TestCase
    {
        [Flags]
        public enum MyFlagsEnum
        {
            V1 = 1,
            V2 = 2,
            V4 = 4,
            V8 = 8,
            V16 = 16,
            V32 = 32,
            V64 = 64,
            V128 = 128
        };

        public void testFlags()
        {
            var iterationCount = 20000;
     
            var myFlagsEnum = MyFlagsEnum.V1 | MyFlagsEnum.V16 | MyFlagsEnum.V64;

            var calculatedEnum = CalculateFlagsEnum(myFlagsEnum);
            AssertSame((int)myFlagsEnum, calculatedEnum);

            var stopwatchEnum = Stopwatch.StartNew();
            for (int i = 0; i < iterationCount; i++)
            {
                CalculateFlagsEnum(myFlagsEnum);
            }
            stopwatchEnum.Stop();

            var canculatedInt = CalculateFlagsInt(calculatedEnum);
            AssertSame(canculatedInt, calculatedEnum);

            var stopwatchInt = Stopwatch.StartNew();
            for (int i = 0; i < iterationCount; i++)
            {
                CalculateFlagsInt(calculatedEnum);
            }
            stopwatchInt.Stop();

            Log.D("EnumPerf", string.Format("Perf Enum: {0} Int: {1}", stopwatchEnum.ElapsedMilliseconds, stopwatchInt.ElapsedMilliseconds));

        }

        private static int CalculateFlagsEnum(MyFlagsEnum myFlagsEnum)
        {
            int result = 0;

            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V1)) result += 1;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V2)) result += 2;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V4)) result += 4;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V8)) result += 8;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V16)) result += 16;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V32)) result += 32;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V64)) result += 64;
            if (HasEnabledEnum(myFlagsEnum, MyFlagsEnum.V128)) result += 128;

            return result;
        }

        private static bool HasEnabledEnum(MyFlagsEnum myFlagsEnum, MyFlagsEnum flag)
        {
            return (myFlagsEnum & flag) == flag;
        }

        private static int CalculateFlagsInt(int myInt)
        {
            int result = 0;

            if (HasEnabledInt(myInt, 1)) result += 1;
            if (HasEnabledInt(myInt, 2)) result += 2;
            if (HasEnabledInt(myInt, 4)) result += 4;
            if (HasEnabledInt(myInt, 8)) result += 8;
            if (HasEnabledInt(myInt, 16)) result += 16;
            if (HasEnabledInt(myInt, 32)) result += 32;
            if (HasEnabledInt(myInt, 64)) result += 64;
            if (HasEnabledInt(myInt, 128)) result += 128;

            return result;
        }

        private static bool HasEnabledInt(int myInt, int flag)
        {
            return (myInt & flag) == flag;
        }
    }
}
