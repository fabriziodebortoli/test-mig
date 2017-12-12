namespace Microarea.Library.LogManager
{
	/// <summary>
	/// ILoggableEventDescriptor.
	/// </summary>
	//=========================================================================
	public interface ILoggableEventDescriptor : System.ICloneable
	{
		//---------------------------------------------------------------------
		int EventCode { get;}
		
		//---------------------------------------------------------------------
		EventTypes EventType { get;}
		
		//---------------------------------------------------------------------
		string Message { get;}
		
		//---------------------------------------------------------------------
		System.DateTime EventTimestamp { get;}
		
		//---------------------------------------------------------------------
		ILoggableEventInfo LoggableEventInfo { get; set;}

		//---------------------------------------------------------------------
		string Serialize(IMessageFormatter formatter);
	}
}
