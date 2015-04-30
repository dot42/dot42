using System.Collections.Generic;
using System.Linq;
using Dot42.ApkLib.Resources;

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

        private ExceptionBehaviorMap(ExceptionBehaviorMap copy)
        {
            DefaultStopOnThrow = copy.DefaultStopOnThrow;
            DefaultStopUncaught = copy.DefaultStopUncaught;
            map = new Dictionary<string, ExceptionBehavior>(copy.map);

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

        private ExceptionBehaviorMap Clone()
        {
            lock (mapLock)
            {
                return new ExceptionBehaviorMap(this);
            }
        }

        /// <summary>
        /// Copy the state of the given object to me.
        /// 
        /// Returns all behaviors that have changed, with their new  values.
        /// If the default behavior was changed, the first returned behavior
        /// will be null.
        /// </summary>
        public IList<ExceptionBehavior> CopyFrom(ExceptionBehaviorMap source)
        {
            source = source.Clone();

            lock (mapLock)
            {
                List<string> changed = new List<string>();

                // find changed values
                foreach (var entry in source.map)
                {
                    if (!Equals(this[entry.Key], entry.Value))
                        changed.Add(entry.Key);
                }

                // add all values that have been removed.
                changed.AddRange(map.Keys.ToList()
                                         .Where(key => !source.map.ContainsKey(key)));

                map.Clear();
                foreach (var entry in source.map)
                {
                    map[entry.Key] = entry.Value;
                }

                List<ExceptionBehavior> ret = new List<ExceptionBehavior>();

                if(DefaultStopUncaught != source.DefaultStopUncaught || DefaultStopOnThrow  != source.DefaultStopOnThrow)
                    ret.Insert(0, null);

                ret.AddRange(changed.Select(x => this[x]));

                DefaultStopOnThrow = source.DefaultStopOnThrow;
                DefaultStopUncaught = source.DefaultStopUncaught;

                return ret;
            }
        }
    }
}
