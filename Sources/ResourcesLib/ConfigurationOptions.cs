using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Set of related options in resource configuration qualifiers
    /// </summary>
    public abstract class ConfigurationOptions : IEnumerable<ConfigurationOption>
    {
        /// <summary>
        /// Is the given option string contain in me?
        /// </summary>
        public abstract bool Contains(string option);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract IEnumerator<ConfigurationOption> GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Set of related options in resource configuration qualifiers
    /// </summary>
    public sealed class ConfigurationOptions<T> : ConfigurationOptions, IEnumerable<ConfigurationOption<T>>
        where T : struct
    {
        private readonly Dictionary<string, ConfigurationOption<T>> options = new Dictionary<string, ConfigurationOption<T>>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConfigurationOptions(IEnumerable<ConfigurationOption<T>> options)
        {
            foreach (var option in options)
            {
                this.options.Add(option.Option, option);
            }
        }

        /// <summary>
        /// Is the given option string contain in me?
        /// </summary>
        public override bool Contains(string option)
        {
            return options.ContainsKey(option);
        }

        /// <summary>
        /// Gets the option string for the given value.
        /// </summary>
        public string GetOption(T value)
        {
            var option = options.Values.First(x => Equals(x.Value, value));
            return option.Option;
        }

        /// <summary>
        /// Gets the option string for the given value.
        /// </summary>
        public T GetValue(string option)
        {
            return options[option].Value;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator<ConfigurationOption> GetEnumerator()
        {
            return options.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<ConfigurationOption<T>> IEnumerable<ConfigurationOption<T>>.GetEnumerator()
        {
            return options.Values.GetEnumerator();
        }
    }
}
