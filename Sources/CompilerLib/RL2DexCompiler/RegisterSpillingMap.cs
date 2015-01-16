using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    internal class RegisterSpillingMap
    {
        private readonly List<RegisterSpillingMapping> mappings = new List<RegisterSpillingMapping>();

        /// <summary>
        /// Add an entry
        /// </summary>
        public void Add(RegisterSpillingMapping mapping)
        {
            mappings.Add(mapping);
        }

        /// <summary>
        /// Find all mappings for the given high register.
        /// </summary>
        public IEnumerable<RegisterSpillingMapping> Find(Register highRegister)
        {
            return mappings.Where(x => x.HighRegister == highRegister);
        }
    }
}
