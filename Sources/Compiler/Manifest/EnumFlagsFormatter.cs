using System;
using System.Collections.Generic;

namespace Dot42.Compiler.Manifest
{
    public class EnumFlagsFormatter : EnumFormatter
    {
        public EnumFlagsFormatter(params Tuple<int, string>[] values)
            : base(values)
        {
        }

        /// <summary>
        /// Format an enum value
        /// </summary>
        public override string Format(int value)
        {
            var originalValue = value;
            string name;
            if (TryFindExact(value, out name))
                return name;
            var list = new List<string>();
            foreach (var tuple in Values)
            {
                var mask = tuple.Item1;
                if ((value & mask) == mask)
                {
                    // Options is included
                    list.Add(tuple.Item2);
                    value = value & ~mask;
                }
            }
            if (value != 0)
                throw new ArgumentException(string.Format("Unknown enum value {0}", originalValue));
            return string.Join("|", list);
        }
    }
}
