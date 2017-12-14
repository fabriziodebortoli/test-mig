using System;
using System.DirectoryServices;	// per creare network share
using System.Globalization;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for ShareManager.
	/// </summary>
	public class ShareManager
	{
		public ShareManager()
		{
		}
		
		//---------------------------------------------------------------------
		public void CreateNetworkShare(string shareName, string localPath)
		{
			DirectoryEntry lan = new DirectoryEntry("WinNT://" + Environment.MachineName + "/LanmanServer");
			DirectoryEntry share = lan.Children.Add(shareName, "fileshare"); // shareName + "$" if you want it hidden

			share.Properties["Path"].Add(localPath);
			share.Properties["MaxUserCount"].Add(-1); // No Limit
			share.CommitChanges();
		}

		//---------------------------------------------------------------------
		public void RemoveNetworkShare(string shareName)
		{
			DirectoryEntry lan = new DirectoryEntry("WinNT://" + Environment.MachineName + "/LanmanServer");
			DirectoryEntry share = FindNetworkShare(lan, shareName);
			if (share != null)
				lan.Children.Remove(share);
		}

		//---------------------------------------------------------------------
		public bool NetworkShareExists(string shareName)
		{
			DirectoryEntry lan = new DirectoryEntry("WinNT://" + Environment.MachineName + "/LanmanServer");
			DirectoryEntry share = FindNetworkShare(lan, shareName);
			return share != null;
		}

		//---------------------------------------------------------------------
		private DirectoryEntry FindNetworkShare(DirectoryEntry lan, string shareName)
		{
			foreach (DirectoryEntry share in lan.Children)
			{
				string name = share.Name;
				string csName = share.SchemaClassName;
				if (string.Compare(name, shareName, true, CultureInfo.InvariantCulture) == 0 &&
					string.Compare(csName, "fileshare", true, CultureInfo.InvariantCulture) == 0)
					return share;
			}
			return null;
//			DirectoryEntry share = lan.Children.Find(shareName, "fileshare");	// dà accezione se non esiste!
//			lan.Children.Remove(share);
		}

		//---------------------------------------------------------------------
	}
}
