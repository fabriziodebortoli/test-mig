using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
    class TFSItem : ISourceControlItem
    {
		string item;
		TFSDBWrapper database;

		internal Workspace Workspace { get { return database.Workspace;  } }
		//---------------------------------------------------------------------
		internal TFSItem(string serverPath, TFSDBWrapper database)
		{
			this.database = database;
			item = serverPath;
		}
		
        #region ISourceControlItem Members

		//---------------------------------------------------------------------
		private void RefreshPendingChanges()
		{
			database.RefreshPendingChanges();
		}

		//---------------------------------------------------------------------
		public ISourceControlItem Add(string local, string comment, bool isProject)
        {
			if (Workspace.PendAdd(local, false) == 0)
				return null;
			RefreshPendingChanges();
			return new TFSItem(Workspace.TryGetServerItemForLocalItem(local), database); 
        }

		//---------------------------------------------------------------------
		public void CheckIn(string local, string comment)
        {
			PendingChange[] changes = Workspace.GetPendingChanges(item);

			Workspace.CheckIn(changes, comment);
			RefreshPendingChanges();
        }

		//---------------------------------------------------------------------
		public void CheckOut(string local, string comment, bool updateLocal)
        {
			if (updateLocal)
				GetLatestVersion(local);

			Workspace.PendEdit(item);
			RefreshPendingChanges();
        }

		//---------------------------------------------------------------------
		public void Delete()
        {
			Workspace.PendDelete(item);
			RefreshPendingChanges();
        }

		//---------------------------------------------------------------------
		public ISourceControlItemCollection GetItems()
        {
			ItemSet items = Workspace.VersionControlServer.GetItems(System.IO.Path.Combine(item, "*"));
			List<string> list = new List<string>();
			foreach (Item i in items.Items)
				list.Add(i.ServerItem);
			foreach (PendingChange pc in database.PendingChanges)
			{
				if (!pc.IsAdd)
					continue;

				if (System.IO.Path.GetDirectoryName(pc.ServerItem).Replace("\\", "/") == item)
					list.Add(pc.ServerItem);
			}

			return new TFSItemCollection(list, database); 
        }

		//---------------------------------------------------------------------
		public void GetLatestVersion(string local)
        {
			GetRequest req = new GetRequest(item, RecursionType.Full, VersionSpec.Latest);
			Workspace.Get(req, GetOptions.None);
			RefreshPendingChanges();
        }

		//---------------------------------------------------------------------
		public bool IsCheckedOut
        {
			get
			{
				return IsCheckedOutToMe;
			}
        }

		//---------------------------------------------------------------------
		public bool IsCheckedOutToMe
        {
            get 
			{
				foreach (PendingChange pc in database.PendingChanges)
				{
					if (pc.ServerItem == this.Path)
						return true;
				}
				return false;
			}
        }

		//---------------------------------------------------------------------
		public bool IsDifferent(string localPath)
        {
			if (IsFolder)
				return false;

			if (!Workspace.VersionControlServer.ServerItemExists(item, Microsoft.TeamFoundation.VersionControl.Client.ItemType.File))
				return true;

			Item i = Workspace.VersionControlServer.GetItem(item);
			IDiffItem target = Difference.CreateTargetDiffItem(Workspace.VersionControlServer, item, VersionSpec.Latest, 0, VersionSpec.Latest);
			FileInfo fi = new FileInfo(localPath);
			
			DiffItemLocalFile source = new DiffItemLocalFile(localPath, i.Encoding, fi.LastWriteTime, false);
			
			DiffOptions diffOptions = new DiffOptions();
			diffOptions.OutputType = DiffOutputType.Unified;
			MemoryStream ms = new MemoryStream();
			using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
			{
				diffOptions.StreamWriter = sw;
				
				Difference.DiffFiles(Workspace.VersionControlServer, source, target, diffOptions, null, true);
				string s = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int) ms.Length);
				return s.Length > 0;
			}
        }

		//---------------------------------------------------------------------
		public bool IsFolder
        {
			get 
			{
				if (Workspace.VersionControlServer.ServerItemExists(item, Microsoft.TeamFoundation.VersionControl.Client.ItemType.Folder))
					return true;

				return Directory.Exists(LocalPath);
			}
        }

		//---------------------------------------------------------------------
		public string LocalPath
        {
            get
            {
				return Workspace.TryGetLocalItemForServerItem(item);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

		//---------------------------------------------------------------------
		public string Name
        {
            get { return System.IO.Path.GetFileName(item); }
        }

		//---------------------------------------------------------------------
		public string Path
        {
			get { return item; }
        }

		//---------------------------------------------------------------------
		public void Rename(string newName)
        {
			Workspace.PendRename(item, newName);
			item = newName;
			RefreshPendingChanges();
        }

		//---------------------------------------------------------------------
		public ItemType Type
        {
			get
			{
				return IsFolder
					? ItemType.Folder
					: ItemType.File;
			}
        }

		//---------------------------------------------------------------------
		public void UndoCheckOut(string local)
        {
			PendingChange[] changes = Workspace.GetPendingChanges(item);

			foreach (PendingChange pc in changes)
			{
				Workspace.Undo(pc.ServerItem);
				if (pc.IsAdd)
				{
					if (pc.ItemType == Microsoft.TeamFoundation.VersionControl.Client.ItemType.Folder)
						Directory.Delete(Workspace.TryGetLocalItemForServerItem(item));
					else
						File.Delete(Workspace.TryGetLocalItemForServerItem(item));
				}
			}
			RefreshPendingChanges();
        }

        #endregion
    }

    class TFSItemCollection : ISourceControlItemCollection
    {
        private List<string> items;
		TFSDBWrapper database;
		
		//--------------------------------------------------------------------------------
		public virtual int Count { get { if (items == null) return 0; return items.Count; }}
		
		
		//---------------------------------------------------------------------
		public TFSItemCollection(List<string> items, TFSDBWrapper database)
		{
			if (items == null) throw new NullReferenceException();

			this.items = items;
			this.database = database;
		}

		//---------------------------------------------------------------------
		public virtual System.Collections.IEnumerator GetEnumerator()
		{
			return new TFSItemCollectionEnumerator(items.GetEnumerator(), this.database);
		}

		//---------------------------------------------------------------------
		internal string[] Items { get { return items.ToArray(); } }
    }

	//=========================================================================
	class TFSItemCollectionEnumerator : IEnumerator
	{
		TFSDBWrapper database;
		protected IEnumerator innerEnumerator;

		//---------------------------------------------------------------------
		public TFSItemCollectionEnumerator(IEnumerator innerEnumerator, TFSDBWrapper database)
		{
			if (innerEnumerator == null) throw new NullReferenceException();

			this.innerEnumerator = innerEnumerator;
			this.database = database;
		}

		//---------------------------------------------------------------------
		public virtual object Current { get { return new TFSItem(((string)innerEnumerator.Current), database); } }
		//---------------------------------------------------------------------
		public bool MoveNext() { return innerEnumerator.MoveNext(); }
		//---------------------------------------------------------------------
		public void Reset() { innerEnumerator.Reset(); }
	}
}
