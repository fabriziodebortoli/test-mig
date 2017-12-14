using System;
using System.IO;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
    public class TFSDBWrapper : ISourceControlDatabase
    {
		private string server;
		private int port;
		private string latestError;

		Workspace workspace;
		TfsTeamProjectCollection tfsServer;
		PendingChange[] pendingChanges;

		//---------------------------------------------------------------------
		internal Workspace Workspace { get { return workspace; } }
		//---------------------------------------------------------------------
		internal PendingChange[] PendingChanges { get { return pendingChanges; } }

		//---------------------------------------------------------------------
		public TFSDBWrapper(string server, int port)
		{
			this.server = server;
			this.port = port;
		}

        #region ISourceControlDatabase Members

		//---------------------------------------------------------------------
		public bool CheckInFile(string file, string localPath)
        {
			ISourceControlItem item = GetItem(file);
			if (item == null)
				return false;

			item.CheckIn(localPath, "");
			return !item.IsCheckedOutToMe;
        }

		//---------------------------------------------------------------------
		public ISourceControlItem AddItem(string file)
		{
			if (workspace.PendAdd(file, false) == 0)
				return null;

			RefreshPendingChanges();

			return new TFSItem(file, this);
		}

		//---------------------------------------------------------------------
		public virtual bool RemoveFile(string file)
		{
			ISourceControlItem item = GetItem(file);
			if (item == null)
				return false;

			item.Delete();
			return true;
		}

		//---------------------------------------------------------------------
		public bool CheckOutFile(string file, string localPath)
        {
			ISourceControlItem item = GetItem(file);
			if (item == null)
				return false;
			
			item.CheckOut(localPath, "", false);
			return item.IsCheckedOutToMe;
        }

		//---------------------------------------------------------------------
		public ISourceControlItem CreateProject(string path, string comment, bool recursive)
        {
			string local = GetLocalPath(path);
			if (!Directory.Exists(local))
				Directory.CreateDirectory(local);

			if (workspace.PendAdd(path, false) == 0)
				return null;
			RefreshPendingChanges();
			return new TFSItem(path, this);
        }

		//---------------------------------------------------------------------
		public ISourceControlItem GetItem(string aPath)
        {
			if (string.IsNullOrEmpty(aPath))
				return null;

			foreach (PendingChange pc in pendingChanges)
			{
				if (!pc.IsAdd)
					continue;

				if (pc.ServerItem.StartsWith(aPath, StringComparison.InvariantCultureIgnoreCase))
					return new TFSItem(aPath, this);
			}

			if (!workspace.VersionControlServer.ServerItemExists(aPath, Microsoft.TeamFoundation.VersionControl.Client.ItemType.Any))
				return null;

			return new TFSItem(aPath, this);
        }

		//---------------------------------------------------------------------
		public ISourceControlItemCollection GetItems(string aPath)
        {
			ISourceControlItem item = GetItem(aPath);
			if (item == null)
				return null;
			return item.GetItems();
        }

		//---------------------------------------------------------------------
		public bool IsOpen
        {
            get { return workspace != null; }
        }

		//---------------------------------------------------------------------
		public string LastError
        {
            get { return latestError; }
        }

		//---------------------------------------------------------------------
		public bool Open(string iniPath)
        {
			return Open(iniPath, Environment.UserDomainName, "");
        }

		//---------------------------------------------------------------------
		public bool Open(string iniPath, string userName, string password)
        {
			try
			{
				UriBuilder ub = new UriBuilder();
                ub.Scheme = "http";
				ub.Host = server;
				ub.Port = port;
                ub.Path = "tfs/Microarea";

				tfsServer = new TfsTeamProjectCollection(ub.Uri, new NetworkCredential(userName, password));
				VersionControlServer vcs = (VersionControlServer)tfsServer.GetService(typeof(VersionControlServer));
				workspace = vcs.GetWorkspace(iniPath, userName);
				if (workspace == null)
				{
					latestError = "Invalid workspace";
					return false;
				}
				RefreshPendingChanges();
				return true;
				
			}
			catch (Exception ex)
			{
				latestError = ex.Message;
				return false;
			}
        }

        #endregion

		//---------------------------------------------------------------------
		public string GetServerPath(string localPath)
		{
			if (workspace == null)
				return null;

			if (string.IsNullOrEmpty(localPath))
				return null;
			return workspace.TryGetServerItemForLocalItem(localPath);
		}

		//---------------------------------------------------------------------
		public string GetLocalPath(string serverPath)
		{
			if (workspace == null)
				return null;

			return workspace.TryGetLocalItemForServerItem(serverPath);
		}

		//---------------------------------------------------------------------
		internal void RefreshPendingChanges()
		{
			pendingChanges = workspace.GetPendingChanges();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (tfsServer != null)
				tfsServer.Dispose();
		}

		#endregion
	}
}
