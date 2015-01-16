using System;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {

	class RegexTrial {
		public string pattern;
		public RegexOptions options;
		public string input;

		public string expected;
		public string error = "";

		public RegexTrial (string pattern, RegexOptions options, string input, string expected)
		{
			this.pattern = pattern;
			this.options = options;
			this.input = input;
            this.expected = SingleCapture(expected);
		}


        private static string SingleCapture(string expected)
        {
            var start = expected.IndexOf("Group[");
            if (start == -1) return expected;

            var end = expected.IndexOf("Group[", start + 8);
            if (end == -1) return expected;

            var index = 0;

            var builder = new StringBuilder(expected.Length);

            while (true)
            {
                start = expected.IndexOf("]=", start) + 2;

                builder.Append(expected, index, start - index);

                var open = LastIndexOfImproved(expected, '(', start, end - start);
                if (open != -1)
                {
                    var close = expected.IndexOf(")", open, end - open + 1);
                    if (close != -1)
                    {
                        builder.Append(expected, open, close - open + 1);
                    }

                    index = close + 1;
                }
                else
                {
                    index = start;
                }

                if (end + 8 > expected.Length) break;
                start = end;
                end = expected.IndexOf("Group[", start + 8);
                if (end == -1) end = expected.Length - 1;
            }

            return builder.ToString();
        }

        private static int LastIndexOfImproved(string str, char ch, int start, int count)
        {
            if (count == -1) return -1;

            var index = str.IndexOf(ch.ToString(), start, count);
            var prevIndex = index;

            while (index != -1)
            {
                count -= index - prevIndex;
                prevIndex = index;

                if (count == 1) break;
                index = str.IndexOf(ch.ToString(), index + 1, count - 1);
            }

            return prevIndex;
        }


		public string Expected {
			get { return expected; }
		}
		
		public string Error {
			get { return this.error; }
		}

		public void Execute ()
		{
            //We do not support RightToLeft, so skip
		    if ((options & RegexOptions.RightToLeft) > 0) return;
            
            string result;

			for (int compiled = 0; compiled < 2; ++compiled) {
				RegexOptions real_options = (compiled == 1) ? (options | RegexOptions.Compiled) : options;
				try
				{
				    Regex re = new Regex(pattern, real_options);
				    int[] group_nums = re.GetGroupNumbers();
				    Match m = re.Match(input);

				    if (m.Success)
				    {
				        result = "Pass.";

				        for (int i = 0; i < m.Groups.Count; ++ i)
				        {
				            int gid = group_nums[i];
				            Group group = m.Groups[gid];

				            result += " Group[" + gid + "]=";
				            foreach (Capture cap in group.Captures)
				                result += "(" + cap.Index + "," + cap.Length + ")";
				        }
				    }
				    else
				    {
				        result = "Fail.";
				    }
				}
				catch (NotSupportedException e)
				{
                    //we do not support conditions. skip
				    if (e.Message.StartsWith("Conditions")) return;

                    result = "NotSupportedException.";
				}
				catch (Exception e) {
					error = e.Message + "\n" + e.StackTrace + "\n\n";

					result = "Error.";
				}

			    //if (expected.StartsWith("Fail.") || expected.StartsWith("Error."))
			    {
			        Assert.AreEqual (expected, result, "Matching input '{0}' against pattern '{1}' with options '{2}'", input, pattern, real_options);
			    }
			}
		}
	}

	class Checksum {
		public Checksum () {
			this.sum = 0;
		}

		public uint Value {
			get { return sum; }
		}

		public void Add (string str) {
			for (int i = 0; i < str.Length; ++ i)
				Add (str[i], 16);
		}

		public void Add (uint n) {
			Add (n, 32);
		}

		public void Add (ulong n, int bits) {
			ulong mask = 1ul << (bits - 1);
			for (int i = 0; i < bits; ++ i) {
				Add ((n & mask) != 0);
				mask >>= 1;
			}
		}

		public void Add (bool bit) {
			bool top = (sum & 0x80000000) != 0;
			sum <<= 1;
			sum ^= bit ? (uint)1 : (uint)0;

			if (top)
				sum ^= key;
		}

		private uint sum;
		private readonly uint key = 0x04c11db7;
	}
}
