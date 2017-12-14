using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	class Configuration : IConfiguration
	{
		private Uri ftpUri;
		private string userName;
		private string password;

		//---------------------------------------------------------------------
		public Configuration(Uri ftpUri, string userName, string password)
		{
			if (ftpUri == null)
				throw new ArgumentNullException("ftpUri");

			if (ftpUri.Scheme != Uri.UriSchemeFtp)
				throw new ArgumentException("ftpUri is not a FTP uri", "ftpUri");

			this.ftpUri = ftpUri;

			this.userName = userName;
			this.password = password;
		}

		#region IConfiguration Members

		//---------------------------------------------------------------------
		public Uri FtpUri
		{
			get { return this.ftpUri; }
		}

		//---------------------------------------------------------------------
		public string UserName
		{
			get { return this.userName; }
		}

		//---------------------------------------------------------------------
		public string Password
		{
			get { return this.password; }
		}

		#endregion
	}
}
