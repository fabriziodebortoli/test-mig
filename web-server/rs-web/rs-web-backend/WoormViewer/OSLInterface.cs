
namespace Microarea.TaskBuilderNet.Woorm.WoormViewer
{
	public enum OSLTypes {Report, Form};

	/// <summary>
	/// Summary description for OSLInterface.
	/// </summary>
	/// ================================================================================
	public class OSLInfo
	{

		public string	NickName;
		public string	Guid;
		public OSLTypes	EType;

		//------------------------------------------------------------------------------
		public OSLInfo()
		{
		}
	}

	/// <summary>
	/// Summary description for OSLInterface.
	/// </summary>
	/// ================================================================================
	public class OSLInterface
	{
		//------------------------------------------------------------------------------
		public OSLInterface()
		{
		}

		//ITRI OslInterface da implementare
		//------------------------------------------------------------------------------
		public static void ObjectGrant(OSLInfo info) { info.NickName = "todo"; }
	}
}
