using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestRegisterSpilling : TestCase
    {
        public void test1()
        {
            var value = ReadInt64();
            AssertTrue(value == 0L);
        }

        byte[] buffer;

        protected virtual void FillBuffer(int numBytes)
        {
            buffer = new byte[numBytes];
        }

        public virtual long ReadInt64()
        {
            FillBuffer(8);

            uint ret_low = (uint)(buffer[0] |
                                   (buffer[1] << 8) |
                                   (buffer[2] << 16) |
                                   (buffer[3] << 24)
                                   );

            uint ret_high = (uint)(buffer[4] |
                                   (buffer[5] << 8) |
                                   (buffer[6] << 16) |
                                   (buffer[7] << 24)
                                   );

            return (long)((((ulong)ret_high) << 32) | ret_low);
        }
		
		public void testCase730() 
		{
			var m = new Case730();
			m.DrawText(m.Width* 0.1, m.Height * 0.2, "Test1", 40, 255, 255, 255, 255); 
			m.DrawText(m.Width* 0.1, m.Height * 0.4, "Test2", 40, 255, 255, 255, 255); 
		}
		
		private class Case730 
		{
			public double Width;
			public double Height;
			public void DrawText(double x, double y, string str, int some, int a, int b, int c, int d) 
			{
			}
		}

        public void test2()
        {
            var matrix = new TransformationMatrix(1, 0, 0, 1, 0, 0);
            matrix.Multiply(new TransformationMatrix(1, 0, 0, 1, 0, 0));
            AssertEquals(1.0, matrix.A);
        }

        public class TransformationMatrix
        {
            double a, b, c, d, e, f;

            public double A
            {
                get{return a;}
            }

            public TransformationMatrix(double a, double b, double c, double d, double e, double f)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.d = d;
                this.e = e;
                this.f = f;
            }

            public void Multiply(TransformationMatrix m)
            {
                //performance optimalization: try to avoid multiple calculations.
                if (1 == m.a &&
                     0 == m.b &&
                     0 == m.c &&
                     1 == m.d
                )
                {
                    //offset only.
                    //this.Translate(m.OffsetX, m.OffsetY);
                }
                else
                {
                    //complex matrix
                    TransformationMatrix n = this.Clone();

                    a = m.a * n.a + m.b * n.c;
                    b = m.a * n.b + m.b * n.d;
                    c = m.c * n.a + m.d * n.c;
                    d = m.c * n.b + m.d * n.d;
                    e = m.e * n.a + m.f * n.c + n.e;
                    f = m.e * n.b + m.f * n.d + n.f;
                }
            }

            public TransformationMatrix Clone()
            {
                return new TransformationMatrix(a, b, c, d, e, f);
            }
        }
    }
}
