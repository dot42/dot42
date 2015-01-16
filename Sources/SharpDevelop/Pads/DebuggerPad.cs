using System;
using System.Reflection;
using System.Windows.Controls;
using Dot42.SharpDevelop.Debugger;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui;

namespace Dot42.SharpDevelop.Pads
{
	/// <summary>
	/// Base class for debugger pads.
	/// </summary>
	[Obfuscation(Exclude = false)]
	public abstract class DebuggerPad : AbstractPadContent
	{
		protected readonly DockPanel panel;
		private ToolBar toolbar;
		private Dot42Debugger debugger;
		private bool invalidatePadEnqueued;
				
		/// <summary>
		/// Gets the outer control
		/// </summary>
		public override object Control {
			get {
				return panel;
			}
		}
		
		/// <summary>
		/// Register resources.
		/// </summary>
		static DebuggerPad() {
			ResourceService.RegisterStrings("Dot42.SharpDevelop.Strings", typeof(DebuggerPad).Assembly);
		}
		
		/// <summary>
		/// Default cto
		/// </summary>
		protected DebuggerPad()
		{
			// UI
			this.panel = new DockPanel();
			this.toolbar = BuildToolBar();
			
			if (this.toolbar != null) {
				this.toolbar.SetValue(DockPanel.DockProperty, Dock.Top);
				
				this.panel.Children.Add(toolbar);
			}
			
			InitializeComponents();
			AttachToDebugger();
		}
		
		/// <summary>
		/// Gets the debugger.
		/// </summary>
		protected Dot42Debugger Debugger {
			get {
				AttachToDebugger();
				return debugger;
			}
		}
		
		/// <summary>
		/// Load the debugger (if needed) and attach to it.
		/// </summary>
		private void AttachToDebugger() {
			if (debugger == null) {
				debugger = DebuggerService.CurrentDebugger as Dot42Debugger;
				if (debugger != null) {
					AttachToDebugger(debugger);
				}
			}
		}

		/// <summary>
		/// Attach to the given debugger.
		/// </summary>
		protected virtual void AttachToDebugger(Dot42Debugger debugger) {
			debugger.IsProcessRunningChanged += (s, x) => InvalidatePad();
			debugger.DebugStopped += (s, x) => InvalidatePad();
		}

		/// <summary>
		/// Initialize this control
		/// </summary>
		protected abstract void InitializeComponents();
		
		/// <summary>
		/// Never call this directly. Always use InvalidatePad()
		/// </summary>
		protected abstract void RefreshPad();
		
		/// <summary>
		/// Refresh this pad on the main thread.
		/// </summary>
		public void InvalidatePad()
		{
			if (invalidatePadEnqueued || WorkbenchSingleton.Workbench == null)
				return;
			invalidatePadEnqueued = true;
			Dot42Addin.InvokeAsyncAndForget(() => {
					invalidatePadEnqueued = false;
					RefreshPad();
				});
			
		}
		
		/// <summary>
		/// Build a toolbar (if any).
		/// </summary>
		protected virtual ToolBar BuildToolBar()
		{
			return null;
		}
	}
}
