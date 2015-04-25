using System.Collections.Generic;
using System.Linq;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Control how the debugger behaves on a specific exceptions.
    /// </summary>
    public sealed class ExceptionBehaviorMap
    {
        private readonly Dictionary<string, ExceptionBehavior> map = new Dictionary<string, ExceptionBehavior>();
        private readonly object mapLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        public ExceptionBehaviorMap()
        {
            ResetDefaults();
        }

        /// <summary>
        /// If set, the process will stop when the exception is thrown.
        /// </summary>
        public bool DefaultStopOnThrow { get; set; }

        /// <summary>
        /// If set, the process will stop when the exception is not caught.
        /// </summary>
        public bool DefaultStopUncaught { get; set; }

        /// <summary>
        /// Gets/sets exception behavior.
        /// </summary>
        public ExceptionBehavior this[string exceptionName]
        {
            get
            {
                lock (mapLock)
                {
                    ExceptionBehavior result;
                    if (map.TryGetValue(exceptionName, out result))
                        return result;
                    return new ExceptionBehavior(exceptionName, DefaultStopOnThrow, DefaultStopUncaught);
                }
            }
            set
            {
                lock (mapLock)
                {
                    map[value.ExceptionName] = value;
                }
            }
        }

        public IEnumerable<ExceptionBehavior> Behaviors
        {
            get
            {
                lock (mapLock)
                    return map.Values.ToList();
            }
        }

        /// <summary>
        /// Set to default values to the initial state.
        /// </summary>
        public void ResetDefaults()
        {
            DefaultStopOnThrow = false;
            DefaultStopUncaught = true;
        }

        /// <summary>
        /// Set to defaults.
        /// </summary>
        public void ResetAll()
        {
            lock (mapLock)
            {
                ResetDefaults();
                map.Clear();
            }
        }

        /// <summary>
        /// Copy the state of the given object to me.
        /// 
        /// returns all values that have changed.
        /// </summary>
        public IEnumerable<ExceptionBehavior> CopyFrom(ExceptionBehaviorMap source)
        {
            List<string> changed = new List<string>();
            lock (mapLock)
            {
                // find changed values
                foreach (var entry in source.map)
                {
                    if (!Equals(this[entry.Key], entry.Value))
                        changed.Add(entry.Key);
                }

                changed.AddRange(map.Keys.ToList()
                                         .Where(key => !source.map.ContainsKey(key)));

                map.Clear();
                foreach (var entry in source.map)
                {
                    map[entry.Key] = entry.Value;
                }

                DefaultStopOnThrow = source.DefaultStopOnThrow;
                DefaultStopUncaught = source.DefaultStopUncaught;

                return changed.Select(x => this[x]).ToList();
            }
        }
    }
}
