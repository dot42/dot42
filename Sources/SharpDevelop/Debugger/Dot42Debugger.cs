using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Model;
using Dot42.Ide.Debugger;
using Dot42.Mapping;
using Dot42.Utility;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using System.Diagnostics;
using TallComponents.Common.Extensions;
using Dot42.SharpDevelop.Debugger;

namespace Dot42.SharpDevelop // Keep this namespace
{
	/// <summary>
	/// Description of DebugEngine.
	/// </summary>
	public class Dot42Debugger : IDebugger
	{
		internal static Dot42Debugger Instance;
		private DebugThread currentThread;
		private DalvikStackFrame currentStackFrame;
		
		/// <summary>
		/// Default ctor
		/// </summary>
		public Dot42Debugger()
		{
			Instance = this;
			DebuggerService.BreakPointAdded += OnBreakPointAdded;
			DebuggerService.BreakPointRemoved += OnBreakPointRemoved;
			DebuggerService.BreakPointChanged += OnBreakPointChanged;
		}
		
		/// <summary>
		/// Debugging is starting
		/// </summary>
		public event EventHandler DebugStarting;
		
		/// <summary>
		/// Debugging has started
		/// </summary>
		public event EventHandler DebugStarted;
		
		/// <summary>
		/// IsProcessRunning property has changed
		/// </summary>
		public event EventHandler IsProcessRunningChanged;
		
		/// <summary>
		/// Debugging has stopped
		/// </summary>
		public event EventHandler DebugStopped;
		
		/// <summary>
		/// Currently selected thread has changed
		/// </summary>
		public event EventHandler CurrentThreadChanged;
		
		/// <summary>
		/// Currently select stack frame has changed.
		/// </summary>
		public event EventHandler CurrentStackFrameChanged;

		private Action<LauncherStates, string> stateUpdate;
		private DebugProcess _debugProcess;
		
		public static Action RefreshingPads;
		
		/// <summary>
		/// Refresh all debugger pads
		/// </summary>
		public static void RefreshPads()
		{
			if (RefreshingPads != null) {
				RefreshingPads();
			}
		}

		/// <summary>
		/// Cleanup
		/// </summary>
		public void Dispose() {
			
		}
		
		/// <summary>
		/// Are we debugging something right now?
		/// </summary>
		public bool IsDebugging
		{
			get  {return (DebugProcess != null); }
		}
		
		/// <summary>
		/// Is a process currently running?
		/// </summary>
		public bool IsProcessRunning
		{
			get {
				var p = DebugProcess;
				return (p == null) || !p.IsSuspended; }
		}
		
		/// <summary>
		/// Should we break directly when running?
		/// </summary>
		public bool BreakAtBeginning
		{
			get;
			set;
		}

		/// <summary>
		/// Is this debugger attached to a process?
		/// </summary>
		public bool IsAttached
		{
			get { return (DebugProcess != null); }
		}
		
		/// <summary>
		/// Can we debug the given project?
		/// </summary>
		public bool CanDebug(IProject project) {
			var p = project as AbstractProject;
			return (p != null) && p.HasProjectType(new Guid("{337B7DB7-2D1E-448D-BEBF-17E887A46E37}"));
		}
		
		public void Start(ProcessStartInfo processStartInfo) {
			
		}
		
		public void StartWithoutDebugging(ProcessStartInfo processStartInfo) {
			
		}
		
		/// <summary>
		/// Stop the debugger
		/// </summary>
		public void Stop() {
			var p = DebugProcess;
			if (p != null)
				p.Terminate();
		}
		
		/// <summary>
		/// Break into the debugger
		/// </summary>
		public void Break() {
			var p = DebugProcess;
			if (p != null)
				p.CauseBreak();
		}
		
		/// <summary>
		/// Continue running
		/// </summary>
		public void Continue() {
			var p = DebugProcess;
			if (p != null)
				p.Continue();
		}
		
		/// <summary>
		/// Step into the current call
		/// </summary>
		public void StepInto() {
			var p = DebugProcess;
			var thread = CurrentThread;
			if ((p != null) && (thread != null))
			{
				p.StepInto(CurrentThread);
			}
		}
		
		/// <summary>
		/// Step over the current call
		/// </summary>
		public void StepOver() {
			var p = DebugProcess;
			var thread = CurrentThread;
			if ((p != null) && (thread != null))
			{
				p.StepOver(CurrentThread);
			}
		}
		
		/// <summary>
		/// Step out of the current method
		/// </summary>
		public void StepOut() {
			var p = DebugProcess;
			var thread = CurrentThread;
			if ((p != null) && (thread != null))
			{
				p.StepOut(CurrentThread);
			}
		}
		
		/// <summary>
		/// Show the attach dialog.
		/// Not implemented.
		/// </summary>
		public void ShowAttachDialog() {
			
		}
		
		/// <summary>
		/// Attach to the given windows process.
		/// Not implemented.
		/// </summary>
		public void Attach(Process process) {
			
		}
		
		/// <summary>
		/// Attach to the given debugger and start debugging.
		/// </summary>
		public void Attach(string apkPath, Dot42.DebuggerLib.Debugger debugger, Guid debuggerGuid) {
			// Cleanup static state
			Launcher.GetAndRemoveDebugger(debuggerGuid, out stateUpdate);

			// Notify SD
			Dot42Addin.InvokeAsyncAndForget(() => DebugStarting.Fire(this));
			
			// Load map file
			var mapFilePath = Path.ChangeExtension(apkPath, ".d42map");
			var mapFile = File.Exists(mapFilePath) ? new MapFile(mapFilePath) : new MapFile();

			// Suspend and prepare the VM
			var suspend = debugger.VirtualMachine.SuspendAsync();
			var prepare = suspend.ContinueWith(t => {
			                                   	t.ForwardException();
			                                   	return debugger.PrepareAsync();
			                                   }).Unwrap();
			var debugProcess = new DebugProcess(debugger, mapFile);
			DebugProcess = debugProcess;
			var initializeBreakpoints = prepare.ContinueWith(t => {
			                                                 	t.ForwardException();
			                                                 	// Setup breakpoints
			                                                 	Dot42Addin.Invoke(() => debugProcess.BreakpointManager.InitializeBreakpoints(DebuggerService.Breakpoints));
			                                                 });
			var loadThreads = initializeBreakpoints.ContinueWith(t => {
			                                                     	t.ForwardException();
			                                                     	return debugProcess.ThreadManager.RefreshAsync();
			                                                     }).Unwrap();
			loadThreads.ContinueWith(t => {
			                         	t.ForwardException();
			                         	OnLoadThreadsDone(debugger, debugProcess);
			                         });
		}
		
		/// <summary>
		/// We're done loading the initial threads.
		/// Notify the GUI that we're good to go.
		/// </summary>
		private void OnLoadThreadsDone(Dot42.DebuggerLib.Debugger debugger, DebugProcess debugProcess) {
			// Notify module
			//eventCallback.Send(program, new ModuleLoadEvent(program.MainModule, "Loading module", true));
			//eventCallback.Send(program, new SymbolSearchEvent(program.MainModule, "Symbols loaded", enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED));

			var mainThread = debugProcess.ThreadManager.MainThread();
			if (mainThread != null)
			{
				// Threads loaded
				// Load complete
				//eventCallback.Send(mainThread, new LoadCompleteEvent());
				//eventCallback.Send(mainThread, new EntryPointEvent());

				// Resume now
				debugger.VirtualMachine.ResumeAsync();
				
				// Notify SD
				Action onDebugStarted = () => {
					if (stateUpdate != null) {
						stateUpdate(LauncherStates.Attached, string.Empty);
						stateUpdate = null;
					}
					DebugStarted.Fire(this);
				};
				Dot42Addin.InvokeAsyncAndForget(onDebugStarted);
			}
			else
			{
				DLog.Error(DContext.VSDebuggerLauncher, "No main thread found");
			}
		}

		/// <summary>
		/// Detach from the current process.
		/// Not implemented.
		/// </summary>
		public void Detach() {
			
		}
		
		/// <summary>
		/// Gets the value of the given variable.
		/// Not yet implemented.
		/// </summary>
		public string GetValueAsString(string variable) {
			return null;
		}
		
		/// <summary>
		/// Can we set the current instruction point?
		/// Not yet.
		/// </summary>
		public bool CanSetInstructionPointer(string filename, int line, int column) {
			return false;
		}
		
		/// <summary>
		/// Set the current instruction point?
		/// Not implemented.
		/// </summary>
		public bool SetInstructionPointer(string filename, int line, int column) {
			return false;
		}
		
		/// <summary>
		/// Set the current instruction point?
		/// Not implemented.
		/// </summary>
		public bool SetInstructionPointer(string filename, int line, int column, bool dryRun)
		{
			//throw new NotImplementedException();
			return false;
		}

		/// <summary>
		/// Handle tooltips.
		/// Not yet implemented.
		/// </summary>
		public void HandleToolTipRequest(ICSharpCode.SharpDevelop.Editor.ToolTipRequestEventArgs e)
		{
			//throw new NotImplementedException();
		}
		
		/// <summary>
		/// Getsa tooltip control.
		/// Not yet implemented.
		/// </summary>
		public object GetTooltipControl(ICSharpCode.NRefactory.Location location, string x) {
			return null;
		}
		
		/// <summary>
		/// Open the source code at the current location.
		/// </summary>
		public void JumpToCurrentLine()
		{
			DebuggerService.RemoveCurrentLineMarker();
			var process = CurrentProcess;
			if (process == null)
				return;
			
			// Activate the main window
			WorkbenchSingleton.MainWindow.Activate();

			// Get the current frame
			var frame = CurrentStackFrame;
			if (frame == null)
				return;
			
			// Get the document location
			var location = frame.GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
			if ((location != null) && (location.Document != null) && (location.Position != null)) {
				DebuggerService.RemoveCurrentLineMarker();
				var p = location.Position;
				#if DEBUG
				Debug.WriteLine(string.Format("Current location: ({0},{1})-({2},{3})  {4:X4}", p.Start.Line, p.Start.Column, p.End.Line, p.End.Column, frame.Location.Index));
				#endif
				DebuggerService.JumpToCurrentLine(location.Document.Path, p.Start.Line, p.Start.Column, p.End.Line, p.End.Column);
			}
		}
		
		/// <summary>
		/// Gets the current debug process (static accessor).
		/// </summary>
		internal static DebugProcess CurrentProcess {
			get{return (Instance !=null) ? Instance.DebugProcess : null; }
		}
		
		/// <summary>
		/// Gets the currently selected thread.
		/// </summary>
		internal DebugThread CurrentThread {
			get {
				if (currentThread == null) {
					var process = CurrentProcess;
					currentThread = ((process != null) && !IsProcessRunning) ? process.ThreadManager.Threads.FirstOrDefault() : null;
				}
				return currentThread;
			}
			set {
				var changed = (currentThread != value);
				currentThread = value;
				if (changed)
				{
					// Notify listeners
					CurrentThreadChanged.Fire(this);
					// Reset stack frame
					CurrentStackFrame = null;
				}
			}
		}
		
		/// <summary>
		/// Gets the currently selected stack frame.
		/// </summary>
		internal DalvikStackFrame CurrentStackFrame {
			get {
				if (currentStackFrame == null) {
					var thread = CurrentThread;
					if (thread != null) {
						CurrentStackFrame = thread.GetCallStack().FirstOrDefault();
					}
				} else if (CurrentProcess == null) {
					CurrentStackFrame = null;
				}
				return currentStackFrame;
			}
			set {
				if (currentStackFrame != value) {
					currentStackFrame = value;
					// Notify listeners
					CurrentStackFrameChanged.Fire(this);
					// Show code
					if (value == null)
						Dot42Addin.InvokeAsyncAndForget(() => DebuggerService.RemoveCurrentLineMarker());
					else
						Dot42Addin.InvokeAsyncAndForget(() => JumpToCurrentLine());
				}
			}
		}

		/// <summary>
		/// Gets the current debug process.
		/// </summary>
		private DebugProcess DebugProcess {
			get { return _debugProcess; }
			set {
				if (_debugProcess != value) {
					var oldValue = _debugProcess;
					if (oldValue != null) {
						oldValue.IsSuspendedChanged -= OnDebugProcessIsSuspendedChanged;
						oldValue.Terminated -= OnDebugProcessTerminated;
					}
					_debugProcess = value;
					CurrentThread = null;
					if (value != null) {
						value.IsSuspendedChanged += OnDebugProcessIsSuspendedChanged;
						value.Terminated += OnDebugProcessTerminated;
					}
				}
			}
		}
		
		/// <summary>
		/// Fire the IsProcessRunningChanged event.
		/// </summary>
		private void OnDebugProcessIsSuspendedChanged(object sender, EventArgs e) {
			var isProcessRunning = IsProcessRunning;
			if (isProcessRunning) {
				// Reset state
				CurrentThread = null;
			}
			Dot42Addin.InvokeAsyncAndForget(() => OnDebugProcessIsSuspendedChangedOnMainThread(isProcessRunning));
		}
		
		/// <summary>
		/// Fire the IsProcessRunningChanged event.
		/// This method must be called on the main thread.
		/// </summary>
		private void OnDebugProcessIsSuspendedChangedOnMainThread(bool isProcessRunning) 
		{
			this.IsProcessRunningChanged.Fire(this);
			if (isProcessRunning)
			{
				DebuggerService.RemoveCurrentLineMarker();
			} 
			else 
			{
				JumpToCurrentLine();
			}
		}
		
		/// <summary>
		/// Debug process has stopped.
		/// </summary>
		private void OnDebugProcessTerminated(object sender, EventArgs e) {
			DebugProcess = null;
			Dot42Addin.InvokeAsyncAndForget(() => {
			                                	try {
			                                		DebugStopped.Fire(this);
			                                	} catch (Exception ex) {
			                                		LoggingService.Warn("Error in DebugStopped", ex);
			                                		// Ignore
			                                	}
			                                	ResetBreakpointBookmark();
			                                });
		}
		
		/// <summary>
		/// Fire the CurrentThreadChanged event.
		/// </summary>
		private void OnCurrentThreadChanged(object sender, EventArgs e) {
			Dot42Addin.InvokeAsyncAndForget(() => CurrentThreadChanged.Fire(this));
		}

		/// <summary>
		/// A breakpoint has been added.
		/// </summary>
		private void OnBreakPointAdded(object sender, BreakpointBookmarkEventArgs e)
		{
			var process = DebugProcess;
			if (process == null)
				return;
			process.BreakpointManager.AddBreakpoint(e.BreakpointBookmark);
		}

		/// <summary>
		/// A breakpoint has been removed.
		/// </summary>
		private void OnBreakPointRemoved(object sender, BreakpointBookmarkEventArgs e)
		{
			var process = DebugProcess;
			if (process == null)
				return;
			process.BreakpointManager.RemoveBreakpoint(e.BreakpointBookmark);
		}

		/// <summary>
		/// A breakpoint has been changed.
		/// </summary>
		private void OnBreakPointChanged(object sender, BreakpointBookmarkEventArgs e)
		{
			var process = DebugProcess;
			if (process == null)
				return;
			process.BreakpointManager.RemoveBreakpoint(e.BreakpointBookmark);
			process.BreakpointManager.AddBreakpoint(e.BreakpointBookmark);
		}
		
		/// <summary>
		/// Called when a breakpoint has been reached.
		/// </summary>
		internal void OnBreakpointTriggered(BreakpointBookmark bb) {
			OnDebugProcessIsSuspendedChanged(this, EventArgs.Empty);
			Dot42Addin.InvokeAsyncAndForget(() => JumpToCurrentLine());
		}
		
		/// <summary>
		/// Reset all breakpoint bookmarks to the state they are when the debugger is not running.
		/// </summary>
		private void ResetBreakpointBookmark() {
			foreach (var breakpoint in DebuggerService.Breakpoints) {
				breakpoint.IsHealthy = true;
				breakpoint.Tooltip = null;
			}
		}
	}
}
