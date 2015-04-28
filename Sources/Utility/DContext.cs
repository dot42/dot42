namespace Dot42.Utility
{
    /// <summary>
    /// Dot42 logging context
    /// </summary>
    public enum DContext
    {
        CompilerStart = 1000,
        CompilerAssemblyResolver = CompilerStart + 1,
        CompilerCodeGenerator = CompilerStart + 2,
        CompilerILConverter = CompilerStart + 3,

        DebuggerLibStart = 2000,
        DebuggerLibDebugger = DebuggerLibStart + 1,
        DebuggerLibJdwpConnection = DebuggerLibStart + 2,
        DebuggerLibModel = DebuggerLibStart + 3,
        DebuggerLibEvent = DebuggerLibDebugger + 4,
        DebuggerLibClassPrepare = DebuggerLibDebugger + 5,
        DebuggerLibCommands = DebuggerLibDebugger + 6,

        ResourceCompilerStart = 3000,
        ResourceCompilerAaptError = ResourceCompilerStart + 1,

        BarDeployStart = 4000,
        BarDeploy = BarDeployStart + 1,

        VSProjectStart = 10000,

        VSDebuggerStart = 11000,
        VSDebuggerEvent = VSDebuggerStart + 1,
        VSDebuggerComCall = VSDebuggerStart + 2,
        VSDebuggerLauncher = VSDebuggerStart + 3,
        /// <summary>
        /// use this to send output to Visual Studios Debug output pane.
        /// </summary>
        VSDebuggerMessage = VSDebuggerStart + 4,
        /// <summary>
        /// use this to send output to Visual Studios Status bar.
        /// </summary>
        VSStatusBar = VSDebuggerStart + 5,
    }
}
