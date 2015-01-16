using System;
using System.Collections.Generic;
using System.Text;
using Dot42.DexLib.Instructions;

namespace Dot42.DexLib.IO
{
    internal class CatchSet : List<Catch>, IEquatable<CatchSet>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public CatchSet(ExceptionHandler handler)
        {
            AddRange(handler.Catches);
            CatchAll = handler.CatchAll;
        }

        public Instruction CatchAll { get; set; }

        public bool Equals(CatchSet other)
        {
            if ((other == null) || (Count != other.Count) || (!Equals(CatchAll, other.CatchAll)))
                return false;
            for (var i = 0; i < Count; i++)
                if (!this[i].Equals(other[i]))
                    return false;
            return true;
        }

        public override bool Equals(object obj)
        {
           return Equals(obj as CatchSet);
        }

        public override int GetHashCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine(CatchAll == null ? "0" : CatchAll.Offset.ToString());
            foreach (Catch @catch in this)
                builder.AppendLine(@catch.GetHashCode().ToString());
            return builder.ToString().GetHashCode();
        }
    }
}