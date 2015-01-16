
using System;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;

namespace Dot42.SharpDevelop.Debugger
{
	/// <summary>
	/// Description of DebugThread.
	/// </summary>
	public sealed class DebugThread : DalvikThread
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugThread(ThreadId id, ThreadManager manager) : base(id, manager)
		{
		}
		
		/// <summary>
		/// Record myself as current thread if needed.
		/// </summary>
		protected override void OnProcessSuspended(SuspendReason reason, bool isCurrentThread)
		{
			base.OnProcessSuspended(reason, isCurrentThread);
			if (isCurrentThread) {
				// Record in debugger
				Dot42Debugger.Instance.CurrentThread = this;
			}
		}
		
		/// <summary>
		/// Gets strong typed manager
		/// </summary>
		private new ThreadManager Manager {
			get{return (ThreadManager)base.Manager; }
		}
	}
}
