using System;
using System.Timers;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public class TimerManager : IDisposable
	{
		private Timer timer;

		//---------------------------------------------------------------------
		public TimerManager(TimeSpan freq)
		{
			double ms = freq.TotalMilliseconds;
			//long ms = 1000 * 30; // pari a 30 secondi, solo per TEST
			//long ms = 1000 * 60 * 15; // pari a 15 minuti
			timer = new Timer();
			timer.Enabled = false;
			timer.Interval = ms;
			timer.Elapsed += new ElapsedEventHandler(OnTimerTickFired);
			timer.AutoReset = false;
		}

		//---------------------------------------------------------------------
		public void Start()
		{
			timer.Start();
		}

		//---------------------------------------------------------------------
		public void Stop()
		{
			timer.Stop();
		}

		//---------------------------------------------------------------------
		public void TestFire()
		{
			OnTimerTickFired(this, EventArgs.Empty);
		}

		//---------------------------- EVENTS ---------------------------------
		//public event ElapsedEventHandler TimerTickFired;
		//public virtual void OnTimerTickFired(object sender, ElapsedEventArgs e)
		//{
		//    ElapsedEventHandler handler = this.TimerTickFired;
		//    if (handler != null)
		//        handler.BeginInvoke(sender, e, null, null);
		//}
		public event EventHandler TimerTickFired;
		protected virtual void OnTimerTickFired(object sender, ElapsedEventArgs e)
		{
			OnTimerTickFired(sender, (EventArgs)e);
		}
		protected virtual void OnTimerTickFired(object sender, EventArgs e)
		{
			EventHandler handler = this.TimerTickFired;
			if (handler != null)
				handler.BeginInvoke(sender, e, null, null);
		}
		//---------------------------------------------------------------------
		public void Dispose()
		{
			if (this.timer != null)
			{
				timer.Stop();
				this.timer.Dispose();
			}
		}
	}
}
