using System.Security.Permissions;

namespace Microarea.Library.LogManager
{
	//=========================================================================
	public interface IProcessDescriptor
	{
		int ProcessId
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get;
		}
		
		string ProcessName
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get;
		}
		
		string AccountName
		{
			get;
		}
	}
}