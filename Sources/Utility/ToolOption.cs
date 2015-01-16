using System;

namespace Dot42.Utility
{
    /// <summary>
    /// Defines for command line arguments of the various tools
    /// </summary>
    public sealed class ToolOption
    {
        private readonly string prototype;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ToolOption(string prototype)
        {
            this.prototype = prototype;
        }

        /// <summary>
        /// Gets the prototype used by OptionSet.
        /// </summary>
        public string Prototype
        {
            get { return prototype; }
        }

        /// <summary>
        /// Gets the prototype.
        /// </summary>
        public static implicit operator string(ToolOption option)
        {
            return option.prototype;
        }

        /// <summary>
        /// Create a command line argument based on the given prototype without value.
        /// </summary>
        public string AsArg()
        {
            var key = "--" + prototype.Split(new[] {'|'})[0];            
            return (key[key.Length-1] == '=') ? key.Substring(0, key.Length - 1) : key;
        }

        /// <summary>
        /// Create a command line argument based on the given prototype with an optional value.
        /// </summary>
        public string CreateArg(string value = null)
        {
            var key = "--" + prototype.Split(new[] { '|' })[0];

            if (key[key.Length - 1] == '=')
            {
                // Add value
                if (value == null)
                    throw new ArgumentNullException("value");
                return key + value;
            }
            if (value != null)
                throw new ArgumentException("value should be null");
            return key;
        }
    }
}
