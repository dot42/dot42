using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Debugger
{
	/// <summary>
	/// Description of ThreadManager.
	/// </summary>
	public class ThreadManager : DalvikThreadManager
	{
		
		public ThreadManager(DebugProcess process) : base(process)
		{
		}

		/// <summary>
		/// Refresh the list of threads.
		/// </summary>
		public Task RefreshAsync()
		{
			return RefreshThreadsAsync();
		}
				
		/// <summary>
		/// Gets all known threads.
		/// This method is thread safe.
		/// </summary>
		public new IEnumerable<DebugThread> Threads
		{
			get { return base.Threads.Cast<DebugThread>(); }
		}
		
		protected override DalvikThread CreateThread(Dot42.DebuggerLib.ThreadId id)
		{
			return new DebugThread(id, this);
		}
		
		protected override void OnThreadEnd(DalvikThread thread)
		{
			base.OnThreadEnd(thread);
			if (thread == Dot42Debugger.Instance.CurrentThread) {
				Dot42Debugger.Instance.CurrentThread = null;
			}
		}
	}
}
