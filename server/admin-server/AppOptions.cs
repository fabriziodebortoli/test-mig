﻿using System;
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
		public InstanceIdentity InstanceIdentity { get; set; }
	}

    //================================================================================
    public class DatabaseInfo
    {
        public string ConnectionString { get; set; }
    }

    //================================================================================
    public class ExternalUrls
    {
        public string GWAMUrl { get; set; }
    }

	//================================================================================
	public class InstanceIdentity
	{
		public string InstanceKey { get; set; }
	}
}
