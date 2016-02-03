namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Value object holding an exception.
    /// </summary>
    public class DalvikExceptionValue : DalvikValue
    {
        public DalvikExceptionValue(Value value, DalvikProcess process)
            : base(value, process)
        {
        }

        /// <summary>
        /// Gets the name of the item that has this value
        /// </summary>
        public override string Name
        {
            get { return "$exception"; }
        }
    }
}
