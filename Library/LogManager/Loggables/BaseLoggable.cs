using System;

namespace Microarea.Library.LogManager.Loggables
{
	/// <summary>
	/// BaseLoggable.
	/// </summary>
	//=========================================================================
	public abstract class BaseLoggable : ILoggable
	{
		public event EventHandler<Loggables.LoggableEventArgs> Event;

		//---------------------------------------------------------------------
		protected void OnLoggableEvent(LoggableEventArgs le)
		{
			if (this.Event != null)
				Event(this, le);
		}
	}
}
