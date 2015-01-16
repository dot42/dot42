using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.Compiler.Manifest
{
    public class EnumFormatter
    {
        protected readonly List<Tuple<int, string>> Values;

        public EnumFormatter(params Tuple<int, string>[] values)
        {
            this.Values = values.ToList();
        }

        /// <summary>
        /// Try to find an exact match for the given value.
        /// </summary>
        protected bool TryFindExact(int value, out string name)
        {
            name = Values.Where(x => x.Item1 == value).Select(x => x.Item2).FirstOrDefault();
            return (name != null);
        }

        /// <summary>
        /// Format an enum value
        /// </summary>
        public virtual string Format(int value)
        {
            string name;
            if (TryFindExact(value, out name))
                return name;
            throw new ArgumentException(string.Format("Unknown enum value {0}", value));
        }
    }
}
