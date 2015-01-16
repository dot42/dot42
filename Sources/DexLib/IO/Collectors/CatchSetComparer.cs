using System;
using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal sealed class CatchSetComparer : IComparer<CatchSet>
    {
        public int Compare(CatchSet x, CatchSet y)
        {
            var xOffsets = CollectOffsets(x);
            var yOffsets = CollectOffsets(y);

            int minp = Math.Min(xOffsets.Count, yOffsets.Count);
            for (int i = 0; i < minp; i++)
            {
                int cp = xOffsets[i].CompareTo(yOffsets[i]);
                if (cp != 0)
                    return cp;
            }

            return 0;
        }

        public List<int> CollectOffsets(CatchSet set)
        {
            var result = new List<int>();

            foreach (var @catch in set)
                result.Add(@catch.Instruction.Offset);

            if (set.CatchAll != null)
                result.Add(set.CatchAll.Offset);

            return result;
        }
    }
}