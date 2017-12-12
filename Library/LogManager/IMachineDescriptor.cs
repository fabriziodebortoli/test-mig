
namespace Microarea.Library.LogManager
{
	//=========================================================================
	public interface IMachineDescriptor
	{
		string OSVersion
		{
			get;
		}

		string ClrVersion
		{
			get;
		}

		string MachineName
		{
			get;
		}
	}
}
