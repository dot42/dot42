using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.CompilerLib.RL
{
    [DebuggerDisplay("{TryStart.Index}-{TryEnd.Index}")]
    public class ExceptionHandler
    {
        private readonly List<Catch> catches;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ExceptionHandler()
        {
            catches = new List<Catch>();
        }

        /// <summary>
        /// Copy ctor
        /// </summary>
        public ExceptionHandler(ExceptionHandler source)
        {
            catches = source.catches.Select(x => new Catch(x)).ToList();
            TryStart = source.TryStart;
            TryEnd = source.TryEnd;
            CatchAll = source.CatchAll;
        }

        public Instruction TryStart { get; set; }
        public Instruction TryEnd { get; set; }
        public Instruction CatchAll { get; set; }
        public List<Catch> Catches
        {
            get { return catches; }
        }
    }
}