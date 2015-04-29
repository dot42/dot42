
using System;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui;

namespace Dot42.SharpDevelop.Debugger
{
	/// <summary>
	/// #Develop location breakpoint.
	/// </summary>
	public class DebugLocationBreakpoint : DalvikLocationBreakpoint
	{
		private readonly BreakpointBookmark bookmark;
		
		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugLocationBreakpoint(Jdwp.EventKind eventKind, Document document, DocumentPosition documentPosition, TypeEntry typeEntry, MethodEntry methodEntry, BreakpointBookmark bookmark) :
			base(eventKind, document, documentPosition, typeEntry, methodEntry)
		{
			this.bookmark = bookmark;
			InvalidateBookmark();
		}
		
		/// <summary>
		/// Gets my bookmark.
		/// </summary>
		public BreakpointBookmark Bookmark { get{return bookmark; }}
		
		/// <summary>
		/// This breakpoint is bound to it's target
		/// </summary>
		protected override void OnBound(int requestId, DalvikBreakpointManager breakpointManager)
		{
			base.OnBound(requestId, breakpointManager);
			InvalidateBookmark();
		}
		
		/// <summary>
		/// This breakpoint is reached.
		/// </summary>
		protected override void OnTrigger(Dot42.DebuggerLib.Events.Jdwp.Breakpoint @event)
		{
			Dot42Debugger.Instance.OnBreakpointTriggered(bookmark);
		}
		
		/// <summary>
		/// Update the color and tooltip of the bookmark.
		/// </summary>
		private void InvalidateBookmark() {
			Dot42Addin.InvokeAsyncAndForget(OnUpdateBookmark);
		}
		
		/// <summary>
		/// Update the bookmark.
		/// This method must be called on the GUI thread.
		/// </summary>
		private void OnUpdateBookmark() {
			if (IsBound) {
				bookmark.IsHealthy = true;
				bookmark.Tooltip = null;
			} else {
				bookmark.IsHealthy = false;
				bookmark.Tooltip = "Breakpoint cannot be set.";
			}
		}
	}
}
