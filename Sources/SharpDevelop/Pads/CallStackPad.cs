using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dot42.DebuggerLib.Model;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.Pads;
using Dot42.SharpDevelop.Debugger;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Pads
{
	/// <summary>
	/// Call stack pad.
	/// </summary>
	public partial class CallStackPad : DebuggerPad
	{
		private ListView framesList;
		private ObservableCollection<FrameWrapper> frames;

		/// <summary>
		/// Default ctor
		/// </summary>
		public CallStackPad()
		{
			InvalidatePad();
		}
		
		/// <summary>
		/// Initialize this pad.
		/// </summary>
		protected override void InitializeComponents()
		{
			frames = new ObservableCollection<FrameWrapper>();
			framesList = new ListView();
			framesList.ContextMenu = CreateContextMenuStrip();
			framesList.MouseDoubleClick += FrameListItemActivate;
			framesList.ItemsSource = frames;
			framesList.View = new GridView();
			panel.Children.Add(framesList);
			
			RedrawContent();
			ResourceService.LanguageChanged += delegate { RedrawContent(); };
		}
		
		/// <summary>
		/// Prepare the listview.
		/// </summary>
		private void RedrawContent()
		{
			framesList.ClearColumns();
			framesList.AddColumn(ResourceService.GetString("Global.Name"),
			                     new Binding { Path = new PropertyPath("Name") },
			                     300);
			framesList.AddColumn(ResourceService.GetString("AddIns.HtmlHelp2.Location"),
			                     new Binding { Path = new PropertyPath("Location") },
			                     250);
		}
		
		protected override void AttachToDebugger(Dot42Debugger debugger)
		{
			base.AttachToDebugger(debugger);
			debugger.CurrentThreadChanged += (s, x) => InvalidatePad();
		}

		protected override void RefreshPad()
		{
			frames.Clear();
			var debugger = Debugger;
			var process = Dot42Debugger.CurrentProcess;
			if ((debugger == null) || (process == null) || debugger.IsProcessRunning) {
				return;
			}
			
			LoggingService.Info("CallStack refresh");

			var currentThread = debugger.CurrentThread;
			if (currentThread != null) {
				foreach (var frame in currentThread.GetCallStack()) {
					frames.Add(new FrameWrapper(frame));
				}
			}
		}

		/// <summary>
		/// Make selected thread the current thread.
		/// </summary>
		private void FrameListItemActivate(object sender, EventArgs e)
		{
			var selection = SelectedItem;
			Debugger.CurrentStackFrame = (selection != null) ? selection.Frame : null;
		}
		
		/// <summary>
		/// Create a context menu.
		/// </summary>
		/// <returns></returns>
		private ContextMenu CreateContextMenuStrip()
		{
			ContextMenu menu = new ContextMenu();
			menu.Opened += FillContextMenuStrip;
			return menu;
		}

		/// <summary>
		/// Fill the context menu for the selected thread.
		/// </summary>
		void FillContextMenuStrip(object sender, RoutedEventArgs e)
		{
			var item = SelectedItem;
			if (item == null)
			{
				e.Handled = true;
				return;
			}
			
			ContextMenu menu = sender as ContextMenu;
			menu.Items.Clear();
			
			/*			MenuItem freezeItem;
			freezeItem = new MenuItem();
			freezeItem.Header = ResourceService.GetString("MainWindow.Windows.Debug.Threads.Freeze");
			freezeItem.IsChecked = item.Thread.Suspended;
			freezeItem.Click +=
				delegate {
				if (items == null || items.Count == 0) {
					e.Handled = true;
					return;
				}
				bool suspended = item.Thread.Suspended;
				
				if (!debuggedProcess.IsPaused) {
					MessageService.ShowMessage("${res:MainWindow.Windows.Debug.Threads.CannotFreezeWhileRunning}", "${res:MainWindow.Windows.Debug.Threads.Freeze}");
					return;
				}
				
				foreach(ThreadModel current in items.OfType<ThreadModel>()) {
					current.Thread.Suspended = !suspended;
				}
				InvalidatePad();
			};
			
			menu.Items.Add(freezeItem);*/
		}
		
		/// <summary>
		/// Gets the selected frame (or null)
		/// </summary>
		private FrameWrapper SelectedItem {
			get {
				var items = framesList.SelectedItems;
				if (items == null || items.Count == 0) {
					return null;
				}
				return items[0] as FrameWrapper;
			}
		}
		
		/// <summary>
		/// Listview item wrapper for stack frames.
		/// </summary>
		private class FrameWrapper {
			private readonly DalvikStackFrame frame;
			
			internal FrameWrapper(DalvikStackFrame frame) {
				this.frame = frame;
			}
			
			public DalvikStackFrame Frame  {get{return frame; }}
			
			[Obfuscation(Feature = "@Xaml")]
			public string Name {
				get{
					return frame.GetDescriptionAsync().Await(DalvikProcess.VmTimeout);
				}
			}
			
			[Obfuscation(Feature = "@Xaml")]
			public string Location {
				get{
					var location = frame.GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
					return (location != null) ? location.Description : null;
				}
			}
		}
	}
}
