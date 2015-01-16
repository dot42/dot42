using Android.Util;
using System;
using System.Collections.Generic;
using Luxmate.MMT.Utilities;

namespace Luxmate.MMT.LmBusMgr.Timer
{
	public enum LmTimerType
	{
		typeSingle = 0,
		typeMultiple = 1
	}

	public interface ILmTimerPauser : IDisposable
	{
		int Id { get; }
	}

	public class LmTimer : IDisposable
	{
		private class LmTimerItem : ICachedListIntItem
		{
			public readonly int Id;
			int ICachedListIntItem.CacheID { get { return Id; } }

			public int Delta;
			public LmTimerType TimerType;

			public bool IsPaused = true;
			//public DateTime EndTime;
			public long EndTime;

			public LmTimerItem(int id, int delta, LmTimerType type)
			{
				Id = id;
				Delta = delta;
				TimerType = type;
			}

			public void UpdateEndTime(long now)
			{
				//EndTime = now.AddMilliseconds(Delta);
				EndTime = now + Delta;
			}
		}

		private class LmTimerPauser : ILmTimerPauser
		{
			private readonly LmTimer Owner;
			private int _Id;
			public int Id { get { return _Id; } }

			public LmTimerPauser(LmTimer owner, int id)
			{
				Owner = owner;
				_Id = id;
				if (_Id != 0)
					Owner.PauseTimer(id);
			}

			~LmTimerPauser()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (_Id != 0)
				{
					Owner.RestartTimer(_Id);
					_Id = 0;
				}
			}
		}

		public const int DefaultTimerResolution = 250;

		private int _TimerResolution = DefaultTimerResolution;
		public int TimerResolution
		{
			get { return _TimerResolution; }
			set
			{
				if (value <= 0)
					value = DefaultTimerResolution;

				if (_TimerResolution != value)
				{
					_TimerResolution = value;
					Restart();
				}
			}
		}

		private bool _IsAutoStart = true;
		public bool IsAutoStart { get { return _IsAutoStart; } set { _IsAutoStart = value; } }

		public event LmTimerEventHandler LmTimerEvent;

		private bool _IsRunning;

		//private System.Threading.Timer _Timer;
		private Java.Util.Timer _Timer;
		private LmTimerTask _TimerTask;

		private long _CurrTime;
		public long CurrTime { get { return _CurrTime; } }

		private object _TimerLock = new object();
		private CachedListInt<LmTimerItem> _Items = new CachedListInt<LmTimerItem>();

		private int m_nTimerIdCtr = 1;

		public LmTimer()
		{
		}

		public void Dispose()
		{
			Stop();
		}

		public void Start()
		{
			if (_IsRunning) return;

			if (_Timer == null)
			{
				//_Timer = new System.Threading.Timer(new System.Threading.TimerCallback(_TimerCallback),
				//    null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

				_Timer = new Java.Util.Timer("LmTimer");

				_IsRunning = true;
				_CurrTime = GetCurrentTime();

				lock (_TimerLock)
				{
					//_Items.ForEach(t => t.UpdateEndTime(_CurrTime));
					foreach (var t in _Items)
					{
						t.UpdateEndTime(_CurrTime);
					}
				}

				//_Timer.Change(TimerResolution, TimerResolution);
				_TimerTask = new LmTimerTask(this);
				_Timer.Schedule(_TimerTask, TimerResolution, TimerResolution);
			}
		}

		public void Stop()
		{
			RemoveAllTimers();
			m_nTimerIdCtr = 1;

			_IsRunning = false;
			if (_Timer != null)
			{
				//_Timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				//_Timer.Dispose();
				_TimerTask.Cancel();
				_TimerTask = null;
				_Timer.Cancel();
				_Timer.Purge();
				_Timer = null;
			}
		}

		public void Restart()
		{
			lock (_TimerLock)
			{
				bool wasRunning = _IsRunning;
				Stop();
				if (wasRunning)
					Start();
			}
		}

		private class LmTimerTask : Java.Util.TimerTask
		{
			private readonly LmTimer Owner;
			public LmTimerTask(LmTimer owner)
			{
				Owner = owner;
			}
			public override void Run()
			{
				Owner.CheckTimer();
			}
		}

		//private void _TimerCallback(object stateInfo)
		//{
		//    CheckTimer();
		//}

		public long GetCurrentTime()
		{
			//return DateTime.UtcNow;
			return Java.Lang.System.CurrentTimeMillis();
		}

		private LmTimerItem GetTimer(int timerId)
		{
			if (timerId == 0) return null;

			lock (_TimerLock)
			{
				LmTimerItem timer = _Items.GetByID(timerId);
				return timer;
			}
		}

		public int AddTimer(int delta, LmTimerType timerType)
		{
			LmTimerItem timer = null;

			lock (_TimerLock)
			{
				int timerId = m_nTimerIdCtr++;
				timer = new LmTimerItem(timerId, delta, timerType);
				//if (_Items.ContainsID(timerId))
				//{
				//}
				_Items.Add(timer);

				// start timer manager
				if (_IsAutoStart && _IsRunning == false)
				{
					Start();
				}
			}

			// reinit timer
			RestartTimer(timer.Id);

			return timer.Id;
		}

		public void RemoveTimer(int timerId)
		{
			if (timerId == 0) return;

			lock (_TimerLock)
			{
				LmTimerItem timer = _Items.GetByID(timerId);
				if (timer != null)
				{
					timer.IsPaused = true;
					_Items.Remove(timer);
				}
				else
				{
				}
#if DEBUG
				if (timer != null)
				{
					TRACE("LmTimer.RemoveTimer ID={0}", timerId);
				}
				else
				{
					TRACE("LmTimer.RemoveTimer ID={0} (not existing)", timerId);
				}
#endif
			}
		}

		public void RemoveAllTimers()
		{
			lock (_TimerLock)
			{
				_Items.Clear();
			}
		}

		public void PauseTimer(int timerId)
		{
			if (timerId == 0) return;

			lock (_TimerLock)
			{
				LmTimerItem timer = _Items.GetByID(timerId);
				if (timer != null)
				{
					timer.IsPaused = true;
				}
#if DEBUG
				if (timer != null)
				{
					TRACE("LmTimer.PauseTimer ID={0}", timerId);
				}
				else
				{
					TRACE("LmTimer.PauseTimer ID={0} (not existing)", timerId);
				}
#endif
			}
		}

		// helper for pause in using constuct
		public ILmTimerPauser GetTimerPauser(int timerId)
		{
			int pauserId = 0;

			lock (_TimerLock)
			{
				LmTimerItem timer = _Items.GetByID(timerId);
				if (timer != null)
				{
					pauserId = timerId;
				}
			}

#if DEBUG
			if (pauserId == 0)
			{
				TRACE("LmTimer.GetTimerPauser ID={0} (not existing)", timerId);
			}
#endif
			var pauser = new LmTimerPauser(this, pauserId);
			return pauser;
		}

		public void RestartTimer(int timerId)
		{
			if (timerId == 0) return;
			RestartTimer(timerId, 0);
		}

		public void RestartTimer(int timerId, int delta)
		{
			if (timerId == 0) return;

			lock (_TimerLock)
			{
				LmTimerItem timer = _Items.GetByID(timerId);
				if (timer != null)
				{
					if (delta != 0)
						timer.Delta = delta;

					//timer.EndTime = GetCurrentTime().AddMilliseconds(timer.Delta);
					timer.UpdateEndTime(GetCurrentTime());
					timer.IsPaused = false;
				}
#if DEBUG
				if (timer != null)
				{
					TRACE("LmTimer.RestartTimer ID={0}", timerId);
				}
				else
				{
					TRACE("LmTimer.RestartTimer ID={0} (not existing)", timerId);
				}
#endif
			}
		}

		[Dot42.Include]
		internal void CheckTimer()
		{
			if (_IsRunning == false) return;

			List<LmTimerItem> fireTimers = null;

			lock (_TimerLock)
			{
				_CurrTime = GetCurrentTime();

				if (_Items.Count == 0) return;

				List<LmTimerItem> removeTimers = null;

				foreach (var timer in _Items)
				{
					if (_IsRunning == false) return;

					if (timer.IsPaused) continue;
					if (timer.EndTime > _CurrTime) continue;

					switch (timer.TimerType)
					{
						case LmTimerType.typeMultiple:
							//timer.EndTime = _CurrTime.AddMilliseconds(timer.Delta);
							timer.UpdateEndTime(_CurrTime);
							if (fireTimers == null)
								fireTimers = new List<LmTimerItem>();
							fireTimers.Add(timer);
							break;

						case LmTimerType.typeSingle:
						default:
							timer.IsPaused = true;

							if (fireTimers == null)
								fireTimers = new List<LmTimerItem>();
							fireTimers.Add(timer);

							if (removeTimers == null)
								removeTimers = new List<LmTimerItem>();
							removeTimers.Add(timer);
							break;
					}
				}

				if (removeTimers != null)
				{
					foreach (var t in removeTimers)
					{
						_Items.Remove(t);
					}
					//removeTimers.ForEach(t => _Items.Remove(t));
					removeTimers.Clear();
				}
			}

			if (fireTimers != null)
			{
				foreach (var t in fireTimers)
				{
					OnLmTimerEvent(t.Id);
				}
				//fireTimers.ForEach(t => OnLmTimerEvent(t.Id));
				fireTimers.Clear();
			}
		}

		private void OnLmTimerEvent(int timerId)
		{
			//System.Diagnostics.Debug.WriteLine("OnLmTimerEvent timerId=" + timerId.ToString());
			if (LmTimerEvent != null)
				LmTimerEvent(null, timerId);
		}

#if DEBUG
		internal static void TRACE(string format, params object[] args)
		{
			//string msg = String.Format(format, args);
			//string dt = DateTime.Now.ToLongTimeString();
			//msg = dt + " " + msg;
			//Log.Debug(msg);
			//Log.Debug(format, args);
		}
#endif
	}

	public delegate void LmTimerEventHandler(object sender, int timerId);

	public static class LmTimerTest
	{
		public static void Test()
		{
			LmTimer timer = new LmTimer();
			timer.LmTimerEvent += timer_LmTimerEvent;
			//timer.Start();
			int id1 = timer.AddTimer(500, LmTimerType.typeMultiple);
			int id2 = timer.AddTimer(1000, LmTimerType.typeSingle);
			System.Threading.Thread.Sleep(5000);
			timer.RemoveTimer(id1);
			timer.RemoveAllTimers();
			timer.Stop();
		}

		static void timer_LmTimerEvent(object sender, int timerId)
		{
			//Log.Debug("LmTimerEvent Id={0}", timerId);
		}
	}
}
