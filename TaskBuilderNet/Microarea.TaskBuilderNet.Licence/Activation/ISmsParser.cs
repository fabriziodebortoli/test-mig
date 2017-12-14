
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	public interface ISmsParser
	{
		//---------------------------------------------------------------------
		string this[string tag] { get; }
		void ParseSms(string smsText);
	}
}
