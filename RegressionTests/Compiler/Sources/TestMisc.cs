using System;
using System.Collections;
using System.IO;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestMisc : TestCase
    {
        public void test1()
        {
            int[] freqs = new[] {1};
            int[] heap = new int[1];
          
            int pos = 0;
            int ppos;
            while (pos > 0 && freqs[heap[ppos = (pos - 1)/2]] > 0)
            {
                pos = ppos;
            }

        }

        public void test2()
        {
            char ch;

            long length = 100;
            long position = 100;

            string bof = "%PDF-";
            int bofIndex = 0;

            while (position < length && position < 1024)
            {
                ch = (char)ReadByte();

                if (ch == bof[bofIndex])
                {
                    bofIndex++;
                    if (bofIndex == bof.Length)
                    {
                        if (position < length)
                        {
                            int minorVersion = 7;
                            int versionLength = 0;

                            ch = (char)ReadByte();

                            bool isMajorVersionPresent = (ch >= '0' && ch <= '9');
                            bool isDotPresent = false;
                            bool isMinorVersionPresent = false;

                            if (isMajorVersionPresent)
                            {
                                versionLength++;
                                ch = (char)ReadByte();
                                isDotPresent = ch == '.';
                            }

                            if (isDotPresent)
                            {
                                //dot present
                                versionLength++;
                                //is there a minor version specified?
                                ch = (char)ReadByte();
                                isMinorVersionPresent = (ch >= '0' && ch <= '9');
                            }

                            if (isMinorVersionPresent)
                            {
                                //minor version is present
                                //store minor version
                                minorVersion = int.Parse(ch.ToString());
                                versionLength++;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    bofIndex = 0;
                }
            }
        }

        private byte ReadByte()
        {
            return (byte)'7';
        }

        public void test3()
        {
            uint window_start = 0;

            byte[] window = new byte[] { 0x78, 0x9C, 0x2D };

            uint buffer = window[window_start++];
            buffer |= (uint)((window[window_start++] & 0xff | (window[window_start++] & 0xff) << 8) << 8);

            AssertEquals(0x2D9C78, buffer);
        }

        public void test4()
        {
            var operandStack = new ArrayList();

            operandStack.Add(1.0);
            operandStack.Add(2.0);
            operandStack.Add(3.0);
            operandStack.Add(4.0);
            operandStack.Add(5.0);
            operandStack.Add(6.0);

            var i = 0;

            AppendBezier(
                 Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]),
                 Convert.ToDouble(operandStack[i + 2]), Convert.ToDouble(operandStack[i + 3]),
                 Convert.ToDouble(operandStack[i + 4]), Convert.ToDouble(operandStack[i + 5]));
        }

        private void AppendBezier(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            AssertEquals(1.0, x1);
            AssertEquals(2.0, y1);
            AssertEquals(3.0, x2);
            AssertEquals(4.0, y2);
            AssertEquals(5.0, x3);
            AssertEquals(6.0, y3);
        }

        public void test5()
        {
            operation(Operator.deprecated, null, null, null);
        }

        public enum Operator
        {
            // meta
            unknown,
            deprecated, // (12 0)
            // path construction
            rmoveto,
            hmoveto,
            vmoveto,
            rlineto,
            hlineto,
            vlineto,
            rrcurveto,
            hhcurveto,
            hvcurveto,
            rcurveline,
            rlinecurve,
            vhcurveto,
            vvcurveto,
            flex,
            hflex,
            hflex1,
            flex1,
            // starting/finishing
            endchar,
            // hints
            hintmask,
            cntrmask,
            hstem,
            vstem,
            hstemhm,
            vstemhm,
            // subroutine
            callsubr,
            callgsubr,
            @return,
            // arithmetic
            eq,
        }

        public interface ICharStringListener
        {
            /// <summary>
            /// 
            /// </summary>
            void ClosePath();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dx"></param>
            /// <param name="dy"></param>
            void AbsoluteMoveTo(double dx, double dy);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dx"></param>
            /// <param name="dy"></param>
            void RelativeMoveTo(double dx, double dy);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dx"></param>
            /// <param name="dy"></param>
            void RelativeLineTo(double dx, double dy);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dxa"></param>
            /// <param name="dya"></param>
            /// <param name="dxb"></param>
            /// <param name="dyb"></param>
            /// <param name="dxc"></param>
            /// <param name="dyc"></param>
            void AppendBezier(double dxa, double dya, double dxb, double dyb, double dxc, double dyc);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dxa"></param>
            /// <param name="dya"></param>
            /// <param name="dxb"></param>
            /// <param name="dyb"></param>
            void AppendQuadraticBezier(double dxa, double dya, double dxb, double dyb);
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            double XPos();
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            double YPos();

            /// <summary>
            /// Resets the move flag.
            /// </summary>
            void ResetMoveFlag();
        }

        private bool calculatedWidth;
        private int width;
        private int nominalWidth;
        private int hintCount;

        void operation(Operator op, ICharStringListener listener, ArrayList operandStack, BinaryReader reader)
        {
            int i;
            byte[] subRoutine;
            switch (op)
            {
                case Operator.@return:
                    reader.BaseStream.Seek(0, SeekOrigin.End);
                    break;
                case Operator.endchar:
                    if (!calculatedWidth)
                    {
                        calculatedWidth = true;
                        if (1 == (operandStack.Count % 2))
                        {
                            //odd number of arguments.
                            //the first is the width offset
                            int widthDiff = Convert.ToInt32(operandStack[0]);
                            this.width = this.nominalWidth + widthDiff;
                        }
                    }
                    if (operandStack.Count >= 4)
                    {
                        //seac(
                        //   (int)operandStack[operandStack.Count - 4],
                        //   (int)operandStack[operandStack.Count - 3],
                        //   (int)operandStack[operandStack.Count - 2],
                        //   (int)operandStack[operandStack.Count - 1],
                        //   listener);
                    }
                    listener.ClosePath();
                    reader.BaseStream.Seek(0, SeekOrigin.End);
                    operandStack.Clear();
                    break;
                case Operator.callsubr:
                    //subRoutine = this.localSubrs.GetData((int)operandStack[operandStack.Count - 1] + this.localSubrsBias);
                    operandStack.RemoveAt(operandStack.Count - 1); // remove the top element (subroutine number)
                    //call(subRoutine, listener);
                    break;
                case Operator.callgsubr:
                    //subRoutine = this.globalSubrs.GetData((int)operandStack[operandStack.Count - 1] + this.globalSubrsBias);
                    operandStack.RemoveAt(operandStack.Count - 1); // remove the top element (subroutine number)
                    //call(subRoutine, listener);
                    break;
                case Operator.vstem:
                case Operator.hstem:
                case Operator.vstemhm:
                case Operator.hstemhm:
                case Operator.hintmask:
                case Operator.cntrmask:
                    //for now assume there is always one of the operators above, to see if a width is specified.
                    hintCount += (operandStack.Count / 2);

                    if (!calculatedWidth)
                    {
                        calculatedWidth = true;

                        if (1 == (operandStack.Count % 2))
                        {
                            //odd number of arguments.
                            //the first is the width offset
                            int widthDiff = Convert.ToInt32(operandStack[0]);
                            this.width = this.nominalWidth + widthDiff;
                        }
                    }
                    operandStack.Clear();
                    break;
                case Operator.flex:
                    System.Diagnostics.Debug.Assert(operandStack.Count == 13);
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[0]), Convert.ToDouble(operandStack[1]),
                        Convert.ToDouble(operandStack[2]), Convert.ToDouble(operandStack[3]),
                        Convert.ToDouble(operandStack[4]), Convert.ToDouble(operandStack[5]));
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[6]), Convert.ToDouble(operandStack[7]),
                        Convert.ToDouble(operandStack[8]), Convert.ToDouble(operandStack[9]),
                        Convert.ToDouble(operandStack[10]), Convert.ToDouble(operandStack[11]));
                    // ignore flex depth
                    operandStack.Clear();
                    break;
                case Operator.hflex:
                    System.Diagnostics.Debug.Assert(operandStack.Count == 7);
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[0]), 0,
                        Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                        Convert.ToDouble(operandStack[3]), 0);
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[4]), 0,
                        Convert.ToDouble(operandStack[5]), 0,
                        Convert.ToDouble(operandStack[6]), 0);
                    operandStack.Clear();
                    break;
                case Operator.flex1:
                    System.Diagnostics.Debug.Assert(operandStack.Count == 11);
                    double dx6 = 0;
                    double dy6 = 0;
                    // The last value on the stack is a value for the X or Y of the last end point
                    // depending on the distance from the start point to the last control point.
                    // If 
                    if (Math.Abs((int)operandStack[0] + (int)operandStack[2] + (int)operandStack[4] +
                                   (int)operandStack[6] + (int)operandStack[8]) >
                         Math.Abs((int)operandStack[1] + (int)operandStack[3] + (int)operandStack[5] +
                                   (int)operandStack[7] + (int)operandStack[9]))
                        dx6 = Convert.ToDouble(operandStack[10]);
                    else
                        dy6 = Convert.ToDouble(operandStack[10]);
                    listener.AppendBezier(
                       Convert.ToDouble(operandStack[0]), Convert.ToDouble(operandStack[1]),
                       Convert.ToDouble(operandStack[2]), Convert.ToDouble(operandStack[3]),
                       Convert.ToDouble(operandStack[4]), Convert.ToDouble(operandStack[5]));
                    listener.AppendBezier(
                       Convert.ToDouble(operandStack[6]), Convert.ToDouble(operandStack[7]),
                       Convert.ToDouble(operandStack[8]), Convert.ToDouble(operandStack[9]),
                       dx6, dy6);
                    operandStack.Clear();
                    break;
                case Operator.hflex1:
                    System.Diagnostics.Debug.Assert(operandStack.Count == 9);
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[0]), Convert.ToDouble(operandStack[1]),
                        Convert.ToDouble(operandStack[2]), Convert.ToDouble(operandStack[3]),
                        Convert.ToDouble(operandStack[4]), 0);
                    listener.AppendBezier(
                        Convert.ToDouble(operandStack[5]), 0,
                        Convert.ToDouble(operandStack[6]), Convert.ToDouble(operandStack[7]),
                        Convert.ToDouble(operandStack[8]), 0);
                    operandStack.Clear();
                    break;
                case Operator.rmoveto:
                    System.Diagnostics.Debug.Assert(2 == operandStack.Count || (!calculatedWidth && 3 == operandStack.Count));
                    if (!calculatedWidth)
                    {
                        calculatedWidth = true;
                        if (3 == operandStack.Count)
                        {
                            //3 arguments, assuming 2
                            //the first is the width offset
                            int widthDiff = Convert.ToInt32(operandStack[0]);
                            this.width = this.nominalWidth + widthDiff;
                        }
                    }
                    listener.ClosePath();
                    listener.RelativeMoveTo(Convert.ToDouble(operandStack[operandStack.Count - 2]), Convert.ToDouble(operandStack[operandStack.Count - 1]));
                    operandStack.Clear();
                    break;
                case Operator.hmoveto:
                    System.Diagnostics.Debug.Assert(1 == operandStack.Count || (!calculatedWidth && 2 == operandStack.Count));
                    if (!calculatedWidth)
                    {
                        calculatedWidth = true;
                        if (2 == operandStack.Count)
                        {
                            //2 arguments, assuming 1
                            //the first is the width offset
                            int widthDiff = Convert.ToInt32(operandStack[0]);
                            this.width = this.nominalWidth + widthDiff;
                        }
                    }
                    listener.ClosePath();
                    listener.RelativeMoveTo(Convert.ToDouble(operandStack[operandStack.Count - 1]), 0);
                    operandStack.Clear();
                    break;
                case Operator.vmoveto:
                    System.Diagnostics.Debug.Assert(1 == operandStack.Count || (!calculatedWidth && 2 == operandStack.Count));
                    if (!calculatedWidth)
                    {
                        calculatedWidth = true;
                        if (2 == operandStack.Count)
                        {
                            //2 arguments, assuming 1
                            //the first is the width offset
                            int widthDiff = Convert.ToInt32(operandStack[0]);
                            this.width = this.nominalWidth + widthDiff;
                        }
                    }
                    listener.ClosePath();
                    listener.RelativeMoveTo(0, Convert.ToDouble(operandStack[operandStack.Count - 1]));
                    operandStack.Clear();
                    break;
                case Operator.rlineto:
                    System.Diagnostics.Debug.Assert(operandStack.Count % 2 == 0);
                    for (i = 0; i < operandStack.Count; i += 2)
                    {
                        listener.RelativeLineTo(Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]));
                    }
                    operandStack.Clear();
                    break;
                case Operator.hlineto:
                    System.Diagnostics.Debug.Assert(operandStack.Count >= 1);
                    if (1 == (operandStack.Count % 2))
                    {
                        listener.RelativeLineTo(Convert.ToDouble(operandStack[0]), 0);
                        for (i = 1; i < operandStack.Count; i += 2)
                        {
                            listener.RelativeLineTo(0, Convert.ToDouble(operandStack[i]));
                            listener.RelativeLineTo(Convert.ToDouble(operandStack[i + 1]), 0);
                        }
                    }
                    else
                    {
                        for (i = 0; i < operandStack.Count; i += 2)
                        {
                            listener.RelativeLineTo(Convert.ToDouble(operandStack[i]), 0);
                            listener.RelativeLineTo(0, Convert.ToDouble(operandStack[i + 1]));
                        }
                    }
                    operandStack.Clear();
                    break;
                case Operator.vlineto:
                    System.Diagnostics.Debug.Assert(operandStack.Count >= 1);
                    if (1 == (operandStack.Count % 2))
                    {
                        listener.RelativeLineTo(0, Convert.ToDouble(operandStack[0]));
                        for (i = 1; i < operandStack.Count; i += 2)
                        {
                            listener.RelativeLineTo(Convert.ToDouble(operandStack[i]), 0);
                            listener.RelativeLineTo(0, Convert.ToDouble(operandStack[i + 1]));
                        }
                    }
                    else
                    {
                        for (i = 0; i < operandStack.Count; i += 2)
                        {
                            listener.RelativeLineTo(0, Convert.ToDouble(operandStack[i]));
                            listener.RelativeLineTo(Convert.ToDouble(operandStack[i + 1]), 0);
                        }
                    }
                    operandStack.Clear();
                    break;
                case Operator.rrcurveto:
                    System.Diagnostics.Debug.Assert(operandStack.Count % 6 == 0);
                    for (i = 0; i < (operandStack.Count - 5); i += 6)
                    {
                        listener.AppendBezier(
                           Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]),
                           Convert.ToDouble(operandStack[i + 2]), Convert.ToDouble(operandStack[i + 3]),
                           Convert.ToDouble(operandStack[i + 4]), Convert.ToDouble(operandStack[i + 5]));
                    }
                    operandStack.Clear();
                    break;
                case Operator.hhcurveto:
                    System.Diagnostics.Debug.Assert(operandStack.Count % 4 == 0 || operandStack.Count % 4 == 1);
                    i = 0;
                    if ((operandStack.Count % 4) == 1)
                    {
                        listener.AppendBezier(
                            Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[0]),
                            Convert.ToDouble(operandStack[2]), Convert.ToDouble(operandStack[3]),
                            Convert.ToDouble(operandStack[4]), 0);
                        i += 5;
                    }
                    for (; (i + 3) < operandStack.Count; i += 4)
                    {
                        listener.AppendBezier(
                            Convert.ToDouble(operandStack[i]), 0,
                            Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                            Convert.ToDouble(operandStack[i + 3]), 0);
                    }
                    operandStack.Clear();
                    break;
                case Operator.hvcurveto:
                    switch (operandStack.Count % 8)
                    {
                        default:
                            System.Diagnostics.Debug.Assert(false);
                            break;
                        case 4:
                            listener.AppendBezier(
                                Convert.ToDouble(operandStack[0]), 0,
                                Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                0, Convert.ToDouble(operandStack[3]));
                            for (i = 4; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i]),
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    Convert.ToDouble(operandStack[i + 3]), 0);
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i + 4]), 0,
                                    Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                    0, Convert.ToDouble(operandStack[i + 7]));
                            }
                            break;
                        case 5:
                            if (5 < operandStack.Count)
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[0]), 0,
                                    Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                    0, Convert.ToDouble(operandStack[3]));
                            }
                            else
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[0]), 0,
                                    Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                    Convert.ToDouble(operandStack[4]), Convert.ToDouble(operandStack[3]));
                            }
                            for (i = 4; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i]),
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    Convert.ToDouble(operandStack[i + 3]), 0);
                                if ((i + 9) < operandStack.Count)
                                {
                                    listener.AppendBezier(
                                        Convert.ToDouble(operandStack[i + 4]), 0,
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        0, Convert.ToDouble(operandStack[i + 7]));
                                }
                                else
                                {
                                    listener.AppendBezier(
                                        Convert.ToDouble(operandStack[i + 4]), 0,
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 8]), Convert.ToDouble(operandStack[i + 7]));
                                    break;
                                }
                            }
                            break;
                        case 0:
                            for (i = 0; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i]), 0,
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    0, Convert.ToDouble(operandStack[i + 3]));
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i + 4]),
                                    Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                    Convert.ToDouble(operandStack[i + 7]), 0);
                            }
                            break;
                        case 1:
                            for (i = 0; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i]), 0,
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    0, Convert.ToDouble(operandStack[i + 3]));
                                if ((i + 9) < operandStack.Count)
                                {
                                    listener.AppendBezier(
                                        0, Convert.ToDouble(operandStack[i + 4]),
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 7]), 0);
                                }
                                else
                                {
                                    listener.AppendBezier(
                                        0, Convert.ToDouble(operandStack[i + 4]),
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 7]), Convert.ToDouble(operandStack[i + 8]));
                                }
                            }
                            break;
                    }
                  
                    operandStack.Clear();
                    break;
                case Operator.rcurveline:
                    System.Diagnostics.Debug.Assert(operandStack.Count % 6 == 2);
                    for (i = 0; (i + 2) < operandStack.Count; i += 6)
                    {
                        listener.AppendBezier(
                           Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]),
                           Convert.ToDouble(operandStack[i + 2]), Convert.ToDouble(operandStack[i + 3]),
                           Convert.ToDouble(operandStack[i + 4]), Convert.ToDouble(operandStack[i + 5]));
                    }
                    listener.RelativeLineTo(Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]));
                    operandStack.Clear();
                    break;
                case Operator.rlinecurve:
                    System.Diagnostics.Debug.Assert(operandStack.Count >= 6 && (operandStack.Count - 6) % 2 == 0);
                    for (i = 0; (i + 6) < operandStack.Count; i += 2)
                    {
                        listener.RelativeLineTo(Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]));
                    }
                    listener.AppendBezier(
                       Convert.ToDouble(operandStack[i]), Convert.ToDouble(operandStack[i + 1]),
                       Convert.ToDouble(operandStack[i + 2]), Convert.ToDouble(operandStack[i + 3]),
                       Convert.ToDouble(operandStack[i + 4]), Convert.ToDouble(operandStack[i + 5]));
                    operandStack.Clear();
                    break;
                case Operator.vhcurveto:
                    switch (operandStack.Count % 8)
                    {
                        default:
                            System.Diagnostics.Debug.Assert(false);
                            break;
                        case 4:
                            listener.AppendBezier(
                                0, Convert.ToDouble(operandStack[0]),
                                Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                Convert.ToDouble(operandStack[3]), 0);
                            for (i = 4; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i]), 0,
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    0, Convert.ToDouble(operandStack[i + 3]));
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i + 4]),
                                    Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                    Convert.ToDouble(operandStack[i + 7]), 0);
                            }
                            break;
                        case 5:
                            if (5 < operandStack.Count)
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[0]),
                                    Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                    Convert.ToDouble(operandStack[3]), 0);
                            }
                            else
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[0]),
                                    Convert.ToDouble(operandStack[1]), Convert.ToDouble(operandStack[2]),
                                    Convert.ToDouble(operandStack[3]), Convert.ToDouble(operandStack[4]));
                            }
                            for (i = 4; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i]), 0,
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    0, Convert.ToDouble(operandStack[i + 3]));
                                if ((i + 9) < operandStack.Count)
                                {
                                    listener.AppendBezier(
                                        0, Convert.ToDouble(operandStack[i + 4]),
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 7]), 0);
                                }
                                else
                                {
                                    listener.AppendBezier(
                                        0, Convert.ToDouble(operandStack[i + 4]),
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 7]), Convert.ToDouble(operandStack[i + 8]));
                                    break;
                                }
                            }
                            break;
                        case 0:
                            for (i = 0; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i]),
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    Convert.ToDouble(operandStack[i + 3]), 0);
                                listener.AppendBezier(
                                    Convert.ToDouble(operandStack[i + 4]), 0,
                                    Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                    0, Convert.ToDouble(operandStack[i + 7]));
                            }
                            break;
                        case 1:
                            for (i = 0; (i + 7) < operandStack.Count; i += 8)
                            {
                                listener.AppendBezier(
                                    0, Convert.ToDouble(operandStack[i]),
                                    Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                                    Convert.ToDouble(operandStack[i + 3]), 0);
                                if ((i + 9) < operandStack.Count)
                                {
                                    listener.AppendBezier(
                                        Convert.ToDouble(operandStack[i + 4]), 0,
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        0, Convert.ToDouble(operandStack[i + 7]));
                                }
                                else
                                {
                                    listener.AppendBezier(
                                        Convert.ToDouble(operandStack[i + 4]), 0,
                                        Convert.ToDouble(operandStack[i + 5]), Convert.ToDouble(operandStack[i + 6]),
                                        Convert.ToDouble(operandStack[i + 8]), Convert.ToDouble(operandStack[i + 7]));
                                }
                            }
                            break;
                    }

                    operandStack.Clear();
                    break;
                case Operator.vvcurveto:
                    System.Diagnostics.Debug.Assert(operandStack.Count % 4 == 0 || operandStack.Count % 4 == 1);
                    i = 0;
                    if ((operandStack.Count % 4) == 1)
                    {
                        listener.AppendBezier(
                           Convert.ToDouble(operandStack[0]), Convert.ToDouble(operandStack[1]),
                           Convert.ToDouble(operandStack[2]), Convert.ToDouble(operandStack[3]),
                           0, Convert.ToDouble(operandStack[4]));
                        i = 5;
                    }
                    for (; (i + 3) < operandStack.Count; i += 4)
                    {
                        listener.AppendBezier(
                           0, Convert.ToDouble(operandStack[i]),
                           Convert.ToDouble(operandStack[i + 1]), Convert.ToDouble(operandStack[i + 2]),
                           0, Convert.ToDouble(operandStack[i + 3]));
                    }
                    operandStack.Clear();
                    break;
                case Operator.eq:
                    System.Diagnostics.Debug.Assert(operandStack.Count >= 2);
                    double num1 = Convert.ToDouble(operandStack[operandStack.Count - 1]);
                    double num2 = Convert.ToDouble(operandStack[operandStack.Count - 2]);
                    operandStack.Clear();
                    operandStack.Add(num1 == num2 ? 1 : 0);
                    break;
                case Operator.unknown:
                    throw new ArgumentException("Unnown operator in type 2 charstring");
                case Operator.deprecated:
                    // do nothing
                    break;
            }
        }
    }
}
