namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain thread information
    /// </summary>
    public abstract class DalvikProcessChild
    {
        private readonly DalvikProcess process;

        protected DalvikProcessChild(DalvikProcess process)
        {
            this.process = process;
        }

        /// <summary>
        /// Provide access to the containing process.
        /// </summary>
        public DalvikProcess Process
        {
            get { return process; }
        }

        /// <summary>
        /// Provide access to the low level debugger connection.
        /// </summary>
        public Debugger Debugger
        {
            get { return process.Debugger; }
        }
    }
}
