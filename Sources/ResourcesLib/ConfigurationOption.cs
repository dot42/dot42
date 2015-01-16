namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Single option in resource configuration qualifiers
    /// </summary>
    public abstract class ConfigurationOption
    {
        private readonly string option;
        private readonly string description;

        protected ConfigurationOption(string option, string description)
        {
            this.option = option;
            this.description = description;
        }

        /// <summary>
        /// Human readable description
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// Option qualifier
        /// </summary>
        public string Option
        {
            get { return option; }
        }
    }

    /// <summary>
    /// Single option in resource configuration qualifiers
    /// </summary>
    public sealed class ConfigurationOption<T> : ConfigurationOption
        where T : struct 
    {
        private readonly T value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConfigurationOption(T value, string option, string description) : base(option, description)
        {
            this.value = value;
        }

        /// <summary>
        /// Enum value
        /// </summary>
        public T Value { get { return value; } }
    }
}
