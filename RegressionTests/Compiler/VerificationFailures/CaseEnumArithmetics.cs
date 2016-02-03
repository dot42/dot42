namespace Dot42.Tests.Compiler.VerificationFailures
{
#if false // if enabled, stops all testing...

    public class CaseEnumArithmetics : TestCase
    {

        public void testDayOfWeekArithmetics()
        {
            DateTime start = DateTime.Now;
            var firstDayOfWeek = DayOfWeek.Thursday;
            var added = start.AddDays(-(int)start.DayOfWeek + (int)firstDayOfWeek);
            
            Assert.AssertTrue(added > start);
        }

        private enum E
        {
            A = -1,
            B = 0,
            C = 1
        };

        public void testSimpleArithmetics()
        {
            var a = E.A; 
            var b = (E)(-(int)a);

            Assert.AssertSame(E.C, b);
        }
    }
#endif
}
