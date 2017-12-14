using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;

namespace Microarea.Library.SystemServices.Iis
{
	//=========================================================================
	public class WebSiteInfo
	{
		private DirectoryEntry siteMetaData;

		private string identifier;
		private string description;
		private int port;

		//---------------------------------------------------------------------
		public string Identifier    { get { return this.identifier; } }
		public string Description   { get { return this.description; } }
		public int    Port          { get { return this.port; } }

		public ServerState State
		{
			get
			{
				try
				{
					siteMetaData.RefreshCache();
					return (ServerState)siteMetaData.Properties["ServerState"].Value;
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.Message);
					return ServerState.Unknown;
				}
			}
		}

		public int Win32Error
		{
			get
			{
				try
				{
					siteMetaData.RefreshCache();

					// workaround for WinXP, where accessing to the "Win32Error" property (apparently non supported) raises an exception; useless for Win2003
					bool propFound = false;
					foreach (string pName in siteMetaData.Properties.PropertyNames)
						if (pName == "Win32Error") // case-sensitive
						{
							propFound = true;
							break;
						}
					if (!propFound)
						return 0;

					PropertyValueCollection pvc = siteMetaData.Properties["Win32Error"];
					if (pvc == null) // works for Win2003, but WinXP needs the workaround above
						return 0;
					object errObj = pvc.Value;
					if (errObj == null)
						return 0;
					return (int)errObj;
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.Message);
					return -1;
				}
			}
		}

		//---------------------------------------------------------------------
		private WebSiteInfo(){} // force using factory method
		public static WebSiteInfo GetWebSiteInfo(string machineName, string siteIdentifier)
		{
			string sitePath = string.Format("IIS://{0}/W3SVC", machineName);
			if (!IisManager.MyDirectoryEntryExists(sitePath))
				return null;
			DirectoryEntry w3objs = new DirectoryEntry(sitePath);
			DirectoryEntry siteMetaData = IisManager.FindChildDirectoryEntry(w3objs, siteIdentifier, "IIsWebServer");
			return GetWebSiteInfo(siteMetaData);
		}
		public static WebSiteInfo GetWebSiteInfo(DirectoryEntry siteMetaData)
		{
			if (string.Compare(siteMetaData.SchemaClassName, "IIsWebServer", true, CultureInfo.InvariantCulture) != 0)
				throw new ArgumentException();

			WebSiteInfo wsi = new WebSiteInfo();
			wsi.siteMetaData = siteMetaData;
			wsi.identifier = siteMetaData.Name;
			wsi.description = siteMetaData.Properties["ServerComment"].Value as string;
			int defaultPort = 80;
			try
			{
				string portString = siteMetaData.Properties["ServerBindings"].Value as string;
				// e.g. ":80:", or "192.168.168.151:80:"
				// port number is between the first and the second column
				if (portString == null || portString.Length == 0)
					wsi.port = defaultPort; // fall into the default value and hope for the best
				else
				{
					int idx = portString.IndexOf(':');
					if (idx == -1)
						wsi.port = defaultPort; // fall into the default value and hope for the best
					else
					{
						int idx2 = portString.IndexOf(':', idx + 1);
						portString = portString.Substring(idx + 1, idx2 - idx - 1);
						wsi.port = Int32.Parse(portString, CultureInfo.InvariantCulture);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				wsi.port = defaultPort; // fall into the default value and hope for the best
			}
			return wsi;
		}

		//---------------------------------------------------------------------
		public void Start()
		{
			siteMetaData.Invoke("Start", null);
			siteMetaData.CommitChanges();
			siteMetaData.RefreshCache();
		}

		//---------------------------------------------------------------------
		public enum ServerState
		{
			Unknown    = 0,
			Starting   = 1, // MD_SERVER_STATE_STARTING
			Started    = 2, // MD_SERVER_STATE_STARTED
			Stopping   = 3, // MD_SERVER_STATE_STOPPING
			Stopped    = 4, // MD_SERVER_STATE_STOPPED
			Pausing    = 5, // MD_SERVER_STATE_PAUSING
			Paused     = 6, // MD_SERVER_STATE_PAUSED
			Continuing = 7  // MD_SERVER_STATE_CONTINUING
		}
	}
}
