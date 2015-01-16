namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Value of a field
    /// </summary>
    public class DalvikFieldValue : DalvikValue
    {
        public readonly DalvikField Field;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikFieldValue(Value value, DalvikField field, DalvikProcess process)
            : base(value, process)
        {
            Field = field;
        }

        /// <summary>
        /// Gets my field's name
        /// </summary>
        public override string Name
        {
            get { return Field.Name; }
        }
    }
}
