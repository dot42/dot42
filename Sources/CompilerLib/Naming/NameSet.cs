using System.Collections.Generic;

namespace Dot42.CompilerLib.Naming
{
    /// <summary>
    /// Set of names
    /// </summary>
    public class NameSet
    {
        private readonly HashSet<string> names = new HashSet<string>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public NameSet()
        {            
        }

        /// <summary>
        /// Copy ctor
        /// </summary>
        public NameSet(IEnumerable<string> nameSet)
        {
            AddRange(nameSet);
        }

        /// <summary>
        /// Add the given name to this set.
        /// </summary>
        public void Add(string name)
        {
            names.Add(name);
        }

        /// <summary>
        /// Add all given names to this set.
        /// </summary>
        public void AddRange(IEnumerable<string> nameSet)
        {
            foreach (var name in nameSet)
            {
                names.Add(name);
            }
        }

        /// <summary>
        /// Convert the given name to a name that is unique.
        /// The returned name is added to this set.
        /// </summary>
        public string GetUniqueName(string originalName)
        {
            var result = originalName;
            var index = 0;
            while (names.Contains(result))
            {
                result = originalName + index++;
            }
            names.Add(result);
            return result;
        }
    }
}
