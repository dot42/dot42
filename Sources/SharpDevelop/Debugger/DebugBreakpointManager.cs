
using System;
using System.Collections.Generic;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using ICSharpCode.SharpDevelop.Debugging;

namespace Dot42.SharpDevelop.Debugger
{
	/// <summary>
	/// #Develop implementation of breakpoint manager.
	/// </summary>
	public class DebugBreakpointManager : DalvikBreakpointManager
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugBreakpointManager(DebugProcess process) : base(process)
		{
		}

		/// <summary>
		/// Create custom breakpoint.
		/// </summary>
		protected override DalvikLocationBreakpoint CreateLocationBreakpoint(SourceCodePosition sourcePosition, TypeEntry typeEntry, MethodEntry methodEntry, object data)
		{
            return new DebugLocationBreakpoint(Jdwp.EventKind.BreakPoint, sourcePosition, typeEntry, methodEntry, (BreakpointBookmark)data);
		}

		
		/// <summary>
		/// Setup all existing breakpoints.
		/// </summary>
		internal void InitializeBreakpoints(IEnumerable<BreakpointBookmark> bookmarks) {
			foreach (var bb in DebuggerService.Breakpoints) {
				AddBreakpoint(bb);
			}
		}
		
		/// <summary>
		/// Add a breakpoint for the given bookmark.
		/// </summary>
		internal void AddBreakpoint(BreakpointBookmark bb) {
			try {
				if (bb.IsEnabled) {
					this.SetAtLocation(bb.FileName, bb.LineNumber, bb.ColumnNumber, bb.LineNumber, int.MaxValue, bb);
				}
			} catch(Exception ex) {
				Dot42Addin.InvokeAsyncAndForget(() => {
				                                	bb.IsHealthy = false;
				                                	bb.Tooltip = ex.Message;
				                                });
			}
		}
		
		/// <summary>
		/// Remove a breakpoint for the given bookmark.
		/// </summary>
		internal void RemoveBreakpoint(BreakpointBookmark bb) {
			var breakpoint = FirstOrDefault<DebugLocationBreakpoint>(x => x.Bookmark == bb);
			if (breakpoint != null) {
				ResetAsync(breakpoint);
			}
		}
	}
}
