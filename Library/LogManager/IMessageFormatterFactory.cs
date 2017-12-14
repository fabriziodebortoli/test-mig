
namespace Microarea.Library.LogManager
{
	//=========================================================================
	public interface IMessageFormatterFactory
	{
		//---------------------------------------------------------------------
		IMessageFormatter CreateMessageFormatter(MessageFormatterType messageFormatterType);
	}

	//=========================================================================
	public enum MessageFormatterType
	{
		None,
		Text,
		Xml,
		Html
	}
}
