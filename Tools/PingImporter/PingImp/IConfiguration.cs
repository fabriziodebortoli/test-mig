using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	public interface IConfiguration
	{
		//---------------------------------------------------------------------
		Uri FtpUri { get; }
		string UserName { get; }
		string Password { get; }
	}
}
