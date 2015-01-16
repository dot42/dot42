namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Single element in an array
    /// </summary>
    public class DalvikArrayElementValue : DalvikValue
    {
        private readonly int index;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikArrayElementValue(Value value, int index, DalvikProcess process)
            : base(value, process)
        {
            this.index = index;
        }

        /// <summary>
        /// Gets my name
        /// </summary>
        public override string Name
        {
            get { return string.Format("[{0}]", index); }
        }
    }
}
