using System;
using System.Diagnostics;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// DebugLogger.
	/// </summary>
	//=========================================================================
	public class DebugLogger : BaseLogger
	{
		private readonly object instanceLockTicket = new object();

		//---------------------------------------------------------------------
		public DebugLogger() : base()
		{}

		#region ILogger Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override void Log(ILoggableEventDescriptor eventDescriptor)
		{
			lock (instanceLockTicket)
			{
				if (Object.ReferenceEquals(eventDescriptor, null))
					throw new ArgumentNullException("eventDescriptor", "'eventDescriptor' cannot be null");

				base.Log(eventDescriptor);

				Debug.WriteLine(eventDescriptor.Serialize(MessageFormatter));
			}
		}

		#endregion

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				Debug.Flush();
		}
	}
}
