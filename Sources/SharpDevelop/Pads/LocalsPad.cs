using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dot42.DebuggerLib.Model;
using Dot42.SharpDevelop.Pads.Controls;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.Pads;
using Dot42.SharpDevelop.Debugger;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Pads
{
	public partial class LocalsPad : DebuggerPad
	{
		private WatchList localVarList;

		/// <summary>
		/// Default ctor
		/// </summary>
		public LocalsPad()
		{
			InvalidatePad();
		}
		
		/// <summary>
		/// Initialize the control.
		/// </summary>
		protected override void InitializeComponents()
		{
			localVarList = new WatchList(WatchListType.LocalVar);
			panel.Children.Add(localVarList);
		}
		
		protected override void AttachToDebugger(Dot42Debugger debugger)
		{
			base.AttachToDebugger(debugger);
			debugger.CurrentStackFrameChanged += (s, x) => InvalidatePad();
		}
		
		protected override void RefreshPad()
		{
			LoggingService.Info("Local Variables refresh");
			try {
				localVarList.WatchItems.Clear();
				var debugger = Debugger;
				var process = Dot42Debugger.CurrentProcess;
				var frame = debugger.CurrentStackFrame;
				if ((frame == null) || (debugger == null) || (process == null) || debugger.IsProcessRunning) {
					return;
				}

				frame.GetValuesAsync().ContinueWith(t => {
				                                    	if (t.CompletedOk()) {
				                                    		Dot42Addin.InvokeAsyncAndForget(() => FillValues(t.Result));
				                                    	}
				                                    });
				
			} catch (Exception ex) {
				MessageService.ShowException(ex);
			}
		}
		
		private void FillValues(IEnumerable<DalvikValue> values) {
			localVarList.WatchItems.Clear();
			foreach (var v in values) {
				localVarList.WatchItems.Add(new DebugValueProperty(v));
			}
		}
	}
}
