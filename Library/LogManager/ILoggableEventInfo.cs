using System;

namespace Microarea.Library.LogManager
{
	/// <summary>
	/// ILoggableEventInfo.
	/// </summary>
	//=========================================================================
	public interface ILoggableEventInfo: ICloneable
	{
		//---------------------------------------------------------------------
		ILogger BubblingLogger
		{
			get;
			set;
		}

		//---------------------------------------------------------------------
		int EventId
		{
			get;
		}
	}
}
