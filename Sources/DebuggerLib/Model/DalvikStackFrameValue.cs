namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Single variable or parameter on the stack frame
    /// </summary>
    public class DalvikStackFrameValue : DalvikValue
    {
        public readonly VariableInfo Variable;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikStackFrameValue(Value value, VariableInfo variable, DalvikProcess process)
            : base(value, process)
        {
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
