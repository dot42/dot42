namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Single variable or parameter on the stack frame
    /// </summary>
    public class DalvikStackFrameValue : DalvikValue
    {
        public readonly VariableInfo Variable;
        public bool IsParameter { get; private set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikStackFrameValue(Value value, VariableInfo variable, bool isParameter, DalvikProcess process)
            : base(value, process)
        {
            IsParameter = isParameter;
            Variable = variable;
        }

        /// <summary>
        /// Gets my name
        /// </summary>
        public override string Name
        {
            get { return Variable.Name; }
        }
    }
}
