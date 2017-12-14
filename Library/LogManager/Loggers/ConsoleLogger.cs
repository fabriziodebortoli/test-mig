using System;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// ConsoleLogger.
	/// </summary>
	//=========================================================================
	public class ConsoleLogger : BaseLogger
	{
		private readonly object instanceLockTicket = new object();

		//---------------------------------------------------------------------
		public ConsoleLogger()
			: base()
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

				Console.WriteLine(eventDescriptor.Serialize(MessageFormatter));
			}
		}

		#endregion
	}
}
