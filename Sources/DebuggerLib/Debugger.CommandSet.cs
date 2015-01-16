namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP based debugger.
    /// </summary>
    partial class Debugger
    {
        public abstract class CommandSet
        {
            protected readonly Debugger Debugger;
            protected readonly int Nr;

            /// <summary>
            /// Default ctor
            /// </summary>
            protected CommandSet(Debugger debugger, int nr)
            {
                Debugger = debugger;
                Nr = nr;
            }

            /// <summary>
            /// Gets the current connection 
            /// </summary>
            protected JdwpConnection ConnectionOrError
            {
                get { return Debugger.ConnectionOrError; }
            }
        }
    }
}
