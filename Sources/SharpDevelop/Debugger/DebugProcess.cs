
using System;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Debugger
{
	/// <summary>
	/// Description of DebugProcess.
	/// </summary>
	public class DebugProcess : DalvikProcess
	{
		/// <summary>
		/// Fired when this program has been terminated.
		/// </summary>
		public event EventHandler Terminated;

		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugProcess(Dot42.DebuggerLib.Debugger debugger, MapFile mapFile) : base(debugger, mapFile)
		{
		}
		
		/// <summary>
		/// Stop executing
		/// </summary>
		public void CauseBreak()
		{
			DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.CauseBreak");
			SuspendAsync();
		}

		/// <summary>
		/// Continue debugging
		/// </summary>
		internal void Continue() {
			DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.Continue");
			ResumeAsync();
		}

		/// <summary>
		/// Terminate the program.
		/// </summary>
		public void Terminate()
		{
			DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.Terminate");
			// Stop VM
			const int exitCode = 0;
			ExitAndDisconnect(exitCode);

			// Notify listeners
			Terminated.Fire(this);
		}
		
		/// <summary>
		/// Step into the current call
		/// </summary>
		public void StepInto(DebugThread thread) {
			DLog.Debug(DContext.VSDebuggerComCall, "Dot42Debugger.StepInto");
			var stepDepth = Jdwp.StepDepth.Into;
			StepAsync(new StepRequest(thread, stepDepth));
		}
		
		/// <summary>
		/// Step over the current call
		/// </summary>
		public void StepOver(DebugThread thread) {
			DLog.Debug(DContext.VSDebuggerComCall, "Dot42Debugger.StepOver");
			var stepDepth = Jdwp.StepDepth.Over;
			StepAsync(new StepRequest(thread, stepDepth));			
		}
		
		/// <summary>
		/// Step out of the current method
		/// </summary>
		public void StepOut(DebugThread thread) {
						DLog.Debug(DContext.VSDebuggerComCall, "Dot42Debugger.StepOut");
			var stepDepth = Jdwp.StepDepth.Out;
			StepAsync(new StepRequest(thread, stepDepth));
		}

		/// <summary>
		/// Debugger has disconnected.
		/// </summary>
		protected override void OnConnectionLost()
		{
			base.OnConnectionLost();
			Terminated.Fire(this);
		}

		/// <summary>
		/// Gets my thread manager
		/// </summary>
		internal new ThreadManager ThreadManager { get { return (ThreadManager) base.ThreadManager; } }

		/// <summary>
		/// Gets our breakpoint manager
		/// </summary>
		internal new DebugBreakpointManager BreakpointManager { get{return (DebugBreakpointManager)base.BreakpointManager; }}
		
		/// <summary>
		/// Create our thread manager.
		/// </summary>
		protected override DalvikThreadManager CreateThreadManager()
		{
			return new ThreadManager(this);
		}
		
		/// <summary>
		/// Create out breakpoint manager.
		/// </summary>
		protected override DalvikBreakpointManager CreateBreakpointManager()
		{
			return new DebugBreakpointManager(this);
		}
	}
}
