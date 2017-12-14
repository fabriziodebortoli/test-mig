using System.Security.Permissions;

namespace Microarea.Library.LogManager
{
	//=========================================================================
	public interface IApplicationDescriptor
	{
		string ApplicationName
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get;
		}

		string ApplicationPath
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get;
		}
	}
}