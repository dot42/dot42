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
	/// Running threads pad.
	/// </summary>
	public partial class ThreadsPad : DebuggerPad
	{
		private ListView runningThreadsList;
		private ObservableCollection<ThreadWrapper> runningThreads;

		/// <summary>
		/// Default ctor
		/// </summary>
		public ThreadsPad()
		{
			InvalidatePad();
		}
		
		/// <summary>
		/// Initialize this control.
		/// </summary>
		protected override void InitializeComponents()
		{
			runningThreads = new ObservableCollection<ThreadWrapper>();
			runningThreadsList = new ListView();
			runningThreadsList.ContextMenu = CreateContextMenuStrip();
			runningThreadsList.MouseDoubleClick += RunningThreadsListItemActivate;
			runningThreadsList.ItemsSource = runningThreads;
			runningThreadsList.View = new GridView();
			panel.Children.Add(runningThreadsList);
			
			SetupColumns();
			ResourceService.LanguageChanged += (s, x) => SetupColumns();
		}
		
		/// <summary>
		/// Setup listview columns
		/// </summary>
		private void SetupColumns()
		{
			runningThreadsList.ClearColumns();
			runningThreadsList.AddColumn(ResourceService.GetString("Global.Name"),
			                             new Binding { Path = new PropertyPath("Name") },
			                             300);
			runningThreadsList.AddColumn(ResourceService.GetString("AddIns.HtmlHelp2.Location"),
			                             new Binding { Path = new PropertyPath("Location") },
			                             250);
			runningThreadsList.AddColumn(ResourceService.GetString("dot42.ThreadsPad.Status"),
			                             new Binding { Path = new PropertyPath("Status") },
			                             80);
		}

		/// <summary>
		/// Reload the contents of this pad.
		/// </summary>
		protected override void RefreshPad()
		{
			runningThreads.Clear();
			var debugger = Debugger;
			var process = Dot42Debugger.CurrentProcess;
			if ((debugger == null) || (process == null) || debugger.IsProcessRunning) {
				return;
			}
			
			LoggingService.Info("Threads refresh");

			foreach (var thread in process.ThreadManager.Threads) {
				runningThreads.Add(new ThreadWrapper(thread));
			}
		}

		/// <summary>
		/// Selected thread activated. Make it the current thread.
		/// </summary>
		private void RunningThreadsListItemActivate(object sender, EventArgs e)
		{
			var selection = SelectedItem;
			var process = Dot42Debugger.CurrentProcess;
			if (process != null) {
				Dot42Debugger.Instance.CurrentThread = (selection != null) ? selection.Thread : null;
			}
		}
		
		/// <summary>
		/// Create a context menu.
		/// </summary>
		private ContextMenu CreateContextMenuStrip()
		{
			ContextMenu menu = new ContextMenu();
			menu.Opened += FillContextMenuStrip;
			return menu;
		}

		/// <summary>
		/// Fill the context menu
		/// </summary>
		private void FillContextMenuStrip(object sender, RoutedEventArgs e)
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
		/// Gets the selected listview item
		/// </summary>
		private ThreadWrapper SelectedItem {
			get {
				var items = runningThreadsList.SelectedItems;
				if (items == null || items.Count == 0) {
					return null;
				}
				return items[0] as ThreadWrapper;
			}
		}
		
		/// <summary>
		/// Listview item wrapping a thread.
		/// </summary>
		private class ThreadWrapper {
			private readonly DebugThread thread;
			
			internal ThreadWrapper(DebugThread thread) {
				this.thread = thread;
			}
			
			public DebugThread Thread { get{return thread; }}
			
			[Obfuscation(Feature = "@Xaml")]
			public string Name {
				get{
					return thread.GetNameAsync().Await(DalvikProcess.VmTimeout);
				}
			}
			
			[Obfuscation(Feature = "@Xaml")]
			public string Location {
				get{
					var frame = thread.GetCallStack().FirstOrDefault();
					return (frame == null) ? "N/A" : frame.GetDescriptionAsync().Await(DalvikProcess.VmTimeout);
				}
			}
			
			[Obfuscation(Feature = "@Xaml")]
			public string Status {
				get{
					return thread.GetStatusAsync().Await(DalvikProcess.VmTimeout).ToString();
				}
			}
			
		}
	}
}
