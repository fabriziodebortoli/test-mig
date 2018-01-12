using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer
{
    //================================================================================
    public class AppOptions
    {
        public DatabaseInfo DatabaseInfo { get; set; }
		public ExternalUrls ExternalUrls { get; set; }
		public SecretsKeys SecretsKeys { get; set; }
		public TBFS TBFS { get; set; }
	}

    //================================================================================
    public class DatabaseInfo
    {
        public string ConnectionString { get; set; }
		public string DBServer { get; set; } // for demo
		public string DBUser { get; set; } // for demo
		public string DBPassword { get; set; } // for demo
	}

    //================================================================================
    public class ExternalUrls
    {
        public string GWAMUrl { get; set; }
		public string DatabaseServiceUrl { get; set; }
	}

	//================================================================================
	public class SecretsKeys
	{
		public string TokenHashingKey { get; set; }
	}

	//================================================================================
	public class TBFS
	{
		public string UploadFolder { get; set; }
	}
}
