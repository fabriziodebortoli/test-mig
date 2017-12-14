using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.XmlPersister;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//================================================================================
	public class DatabaseBindingInfo : IDisposable
	{
		[XmlIgnore]
		private ISourceControlDatabase sourceDB			= null;
		
		public TFSBinding TFSBinding;
		
		private string		sourceSafeDatabase;
		//--------------------------------------------------------------------------------
		public	 string	SourceSafeDatabase	{get {return sourceSafeDatabase; }	set { sourceSafeDatabase = value; sourceDB = null; } }

		private string serviceUrl;
		//--------------------------------------------------------------------------------
		public string ServiceUrl { get { return serviceUrl; } set { serviceUrl = value; sourceDB = null; } }

		private string user;
		//--------------------------------------------------------------------------------
		public	 string	User	{get {return user; }	set { user = value; } }

		private string password;
		//--------------------------------------------------------------------------------
		public	 string	Password { get { return password; } set { password = value; } }

		private bool remoteDatabase;
		//--------------------------------------------------------------------------------
		public	 bool	RemoteDatabase	{get {return remoteDatabase; }	set { remoteDatabase = value; }}

		//--------------------------------------------------------------------------------
		public ISourceControlDatabase GetSourceControlDatabase()
		{ 
			try
			{
				if (sourceDB != null)
					return sourceDB;

				if (TFSBinding.IsValid)
				{
					sourceDB = new TFSDBWrapper(TFSBinding.Server, TFSBinding.Port);
					if (!sourceDB.Open(TFSBinding.Workspace, TFSBinding.User, TFSBinding.Password))
						sourceDB = null;

				}
				return sourceDB;
			}
			catch 
			{
				sourceDB = null;
				return null;
			}
		}


		#region IDisposable Members

		public void Dispose()
		{
			if (sourceDB != null)
				sourceDB.Dispose();
		}

		#endregion
	}

	//================================================================================
	public class ProjectBindingInfo : IDisposable
	{
		[XmlIgnore]
		public const string CryptKey		= "ProjectBindingInfo";
		[XmlIgnore]
		public const string CryptBlock		= "SourceBinding";
		
		[XmlIgnore]
		private DatabaseBindingInfo dbBindingInfo;
		
		public enum VerifyMode { OnlyLocal, OnlySourceSafe, All }

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public DatabaseBindingInfo DatabaseBindingInfo  { get { return dbBindingInfo; } } 
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	 string	SourceSafeDatabase	{get {return dbBindingInfo.SourceSafeDatabase; }	set { dbBindingInfo.SourceSafeDatabase = value; } }
	
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	 string	ServiceUrl	{get {return dbBindingInfo.ServiceUrl; }	set { dbBindingInfo.ServiceUrl = value; } }
	
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	 string	User	{get {return dbBindingInfo.User; }	set { dbBindingInfo.User = value; } }
	
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	 string	Password	
		{
			get
			{
				return Crypto.Decrypt(dbBindingInfo.Password, CryptKey, CryptBlock);
			}
			set
			{
				dbBindingInfo.Password = Crypto.Encrypt(value, CryptKey, CryptBlock);
			}
		}
	
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	 bool	RemoteDatabase	{get {return dbBindingInfo.RemoteDatabase; }	set { dbBindingInfo.RemoteDatabase = value; } }
	
		private string		lastError		= string.Empty;

		private SSafeNodeInfo	rootInfo;
		
		//--------------------------------------------------------------------------------
		public	SSafeNodeInfo	RootInfo		{get {return rootInfo;}				set { rootInfo = value; } }
		
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public	string 	LastError		{get {return lastError; } }

		public ProjectBindingInfo()
		{
			this.dbBindingInfo = new DatabaseBindingInfo();
		}
		//--------------------------------------------------------------------------------
		public void InitParents()
		{
			InitParents(RootInfo);
		}

		//--------------------------------------------------------------------------------
		private void InitParents(SSafeNodeInfo info)
		{
			if (info == null)
				return;
			foreach (SSafeNodeInfo child in info.Childs)
			{
				child.Parent = info;
				InitParents(child);
			}
		}
		
		//--------------------------------------------------------------------------------
		private string GetLocalInfoPath(string sourceSafeFilePath)
		{	
			return Path.ChangeExtension(sourceSafeFilePath, CommonFunctions.sourceSafeLocalFileExt);
		}

		//--------------------------------------------------------------------------------
		public void LoadLocalInfos(string sourceSafeFilePath)
		{
			string path = GetLocalInfoPath(sourceSafeFilePath);

			if (File.Exists(path)) 
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				dbBindingInfo = SerializerUtility.DeserializeFromXmlNode(doc.DocumentElement, typeof(DatabaseBindingInfo)) as DatabaseBindingInfo;
			}

			if (dbBindingInfo == null)
				dbBindingInfo = new DatabaseBindingInfo();
		}

		//--------------------------------------------------------------------------------
		public void SaveLocalInfos(string sourceSafeFilePath)
		{
			if (sourceSafeFilePath == null || sourceSafeFilePath.Length == 0)
				return;

			XmlNode n = SerializerUtility.SerializeToXmlNode(dbBindingInfo);

			if (n == null)
				return;

			string path = GetLocalInfoPath(sourceSafeFilePath);

			CommonUtilities.Functions.SafeDeleteFile(path);
			n.OwnerDocument.Save(path);
		}

		//--------------------------------------------------------------------------------
		public ISourceControlDatabase GetSourceControlDatabase()
		{
			if (dbBindingInfo == null)
				return null;

			return dbBindingInfo.GetSourceControlDatabase();
		}

		//--------------------------------------------------------------------------------
		public string GetSourceControlPath(string localPath, NodeType type)
		{
			SSafeNodeInfo pathInfo = SearchPath(RootInfo, localPath, type);
			if (pathInfo == null)
				return GetTFSPath(localPath);

			localPath = localPath.Substring(pathInfo.PhysicalLocalPath.Length).Replace("\\", "/"); 
			
			return pathInfo.GetSourceSafeFullPath() + localPath;
		}

		//--------------------------------------------------------------------------------
		private string GetTFSPath(string localPath)
		{
			TFSDBWrapper tfsDb = GetSourceControlDatabase() as TFSDBWrapper;
			if (tfsDb == null)
				return null;

			return tfsDb.GetServerPath(localPath);
		}

		//--------------------------------------------------------------------------------
		public string GetSourceControlRootPath(string localPath, NodeType type)
		{
			SSafeNodeInfo pathInfo = SearchPath(RootInfo, localPath, type);
			if (pathInfo == null)
				return GetTFSPath(localPath);

			return pathInfo.GetSourceSafeFullPath(); 
		}


		//--------------------------------------------------------------------------------
		private SSafeNodeInfo SearchPath(SSafeNodeInfo info, string localPath, NodeType type)
		{
			if (info == null) return null;

			string a = localPath.ToLower().Replace("/", "\\");
			string b = info.PhysicalLocalPath.ToLower().Replace("/", "\\");
			
			if (
				info.Active &&
				info.NodeType == type 
				&& 
				( a == b || a.StartsWith(b + "\\"))
				)
				return info;
			
			foreach (SSafeNodeInfo childInfo in info.Childs)
			{
				SSafeNodeInfo pathInfo = SearchPath(childInfo, localPath, type);
				if (pathInfo != null)
					return pathInfo;
			}

			return null;
		}

		

		//--------------------------------------------------------------------------------
		public bool VerifyBindings(VerifyMode mode)
		{
			ArrayList dummy1, dummy2, dummy3, dummy4;
			return VerifyBindings(mode, out dummy1, out dummy2, out dummy3, out dummy4);
		}

		//--------------------------------------------------------------------------------
		public bool VerifyBindings (
			VerifyMode mode,
			out ArrayList invalidSourceSafePaths, 
			out ArrayList localPaths,
			out ArrayList invalidLocalPaths, 
			out ArrayList sourceSafePaths
			)
		{
			return VerifyBindings(mode, null, out invalidSourceSafePaths, out localPaths, out invalidLocalPaths, out sourceSafePaths);
		}

		//--------------------------------------------------------------------------------
		public bool VerifyBindings (
			VerifyMode mode,
			TreeNode rootNode,
			out ArrayList invalidSourceSafePaths, 
			out ArrayList localPaths,
			out ArrayList invalidLocalPaths, 
			out ArrayList sourceSafePaths
			)
		{
			
			invalidSourceSafePaths = new ArrayList();
			localPaths = new ArrayList();
			invalidLocalPaths = new ArrayList();
			sourceSafePaths = new ArrayList();
	
			SSafeNodeInfo info = null;
			if (rootNode != null && rootNode.Tag != null)
				info = SearchPath(RootInfo, ((NodeTag)rootNode.Tag).ToString(), ((NodeTag)rootNode.Tag).GetNodeType());
			
			if (info == null)
				info = RootInfo;
			
			WaitDialog waitDialog = new WaitDialog(new WaitDialog.VerifyBindingsProcedure(VerifyBindings), info, invalidSourceSafePaths, localPaths, invalidLocalPaths, sourceSafePaths);
	
			try
			{
								
				if (GetSourceControlDatabase() == null)
				{
					lastError = Strings.InvalidDatabase;
					return false;
				}
			
				waitDialog.ShowDialog();
							
				switch (mode)
				{
					case VerifyMode.OnlyLocal:		if (invalidLocalPaths.Count == 0) return true; break;
					case VerifyMode.OnlySourceSafe: if (invalidSourceSafePaths.Count == 0) return true; break;
					case VerifyMode.All:			if (invalidSourceSafePaths.Count == 0 && invalidLocalPaths.Count == 0) return true; break;
				}

				lastError = string.Empty;

				if (invalidSourceSafePaths.Count > 0)
				{
					StringBuilder paths = new StringBuilder();
					foreach (string path in invalidSourceSafePaths)
					{
						paths.Append(path);
						paths.Append("\r\n");
					}
				
					lastError = string.Format(Strings.InvalidSourceSafePaths, paths.ToString());
				}

				if (invalidLocalPaths.Count > 0)
				{
					StringBuilder paths = new StringBuilder();
					foreach (string path in invalidLocalPaths)
					{
						paths.Append(path);
						paths.Append("\r\n");
					}
				
					if (lastError != string.Empty) 
						lastError += "\r\n";

					lastError += string.Format(Strings.InvalidLocalPaths, paths.ToString());
				}
			}
			catch (Exception ex)
			{
				lastError += ex.Message;
				return false;
			}
			finally 
			{
				waitDialog.Close();								
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		private void VerifyBindings	(
			SSafeNodeInfo info, 
			ArrayList invalidSourceSafePaths, 
			ArrayList localPaths,
			ArrayList invalidLocalPaths, 
			ArrayList sourceSafePaths
			)
		{
			try
			{
				if (info.Active)
				{
					string path = info.GetSourceSafeFullPath();
					if (GetSourceControlDatabase().GetItem(path) == null)
					{
						if (!invalidSourceSafePaths.Contains(path))
						{
							invalidSourceSafePaths.Add(path);
							localPaths.Add(info.PhysicalLocalPath);
						}
					}

					if (!Directory.Exists(info.PhysicalLocalPath))
					{
						if (!invalidLocalPaths.Contains(info.PhysicalLocalPath))
						{
							invalidLocalPaths.Add(info.PhysicalLocalPath);
							sourceSafePaths.Add(path);
						}
					}
				}

				foreach (SSafeNodeInfo childInfo in info.Childs)
					VerifyBindings(childInfo, invalidSourceSafePaths, localPaths, invalidLocalPaths, sourceSafePaths);
			}
			catch (Exception ex)
			{
				lastError += ex.Message;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (dbBindingInfo != null)
				dbBindingInfo.Dispose();
		}

		#endregion
	}

	//================================================================================
	public class SSafeNodeInfo
	{
		public string		LocalPath = null;
		public NodeType		NodeType;
		public ArrayList	PathComponents = new ArrayList();
		public bool			Active;

		public ArrayList	Childs = new ArrayList();

		[XmlIgnore]
		public SSafeNodeInfo		Parent = null;

		[XmlIgnore]
		public string PhysicalLocalPath
		{
			get
			{
				return CommonFunctions.LogicalPathToPhysicalPath(LocalPath);
			}
		}

		//--------------------------------------------------------------------------------
		public int			GetDeepLevel() { return PathComponents.Count; }
		
		//--------------------------------------------------------------------------------
		public ArrayList GetDynamicPathComponents()
		{ 
			ArrayList list = new ArrayList();
			for (int i = PathComponents.Count - 1; i >= 0 ; i--)
			{
				if (PathComponents[i] == null)
					list.Insert(0, (Parent != null) ? Parent.GetDynamicPathComponents()[i] : string.Empty);
				else
				{
					string s = PathComponents[i] as String;
					list.Insert(0, s == string.Empty ? GetRelativePath(i + 1) : s);
				}
			}
			return list; 
		}

		//--------------------------------------------------------------------------------
		public string GetSourceSafeFullPath()
		{ 
			if (!Active) return string.Empty;

			string path = "";
			foreach (string component in GetDynamicPathComponents())
			{
				if (component.StartsWith("$"))
					path = component;
				else
					path += component;
			}
			return path;
		}

		//--------------------------------------------------------------------------------
		private string AdjustPath(string path)
		{
			if (path == string.Empty) return path;

			// se non esiste come directory, allora è un file -> ne calcolo la directory
			if (!Directory.Exists(path))
				path = System.IO.Path.GetDirectoryName(path);

			return path.ToLower().Replace("\\", "/");
		}

		//--------------------------------------------------------------------------------
		public string GetRelativePath()
		{
			return GetRelativePath(GetDeepLevel());
		}

		//--------------------------------------------------------------------------------
		public string GetRelativePath(int deepLevel)
		{
			SSafeNodeInfo i = this;
			while (i.GetDeepLevel() != deepLevel)
				i = i.Parent;
	
			if (i.Parent == null)
				return ConvertToRootPath(i.PhysicalLocalPath);

			string parentPath = AdjustPath(i.Parent.PhysicalLocalPath);
			string currentPath = AdjustPath(i.PhysicalLocalPath);
			if (!currentPath.StartsWith(parentPath))
				return ConvertToRootPath(currentPath);
			
			return (currentPath.Substring(parentPath.Length));
		}

		//--------------------------------------------------------------------------------
		private string ConvertToRootPath(string path)
		{
			path = AdjustPath(path);
			string root = AdjustPath(System.IO.Path.GetPathRoot(path));
			return path.Replace(root, "$/");
		}

		//--------------------------------------------------------------------------------
		public bool Match(SSafeNodeInfo info)
		{
			return string.Compare(PhysicalLocalPath, info.PhysicalLocalPath, true) == 0 && NodeType == info.NodeType;
		}

	}
	
	//================================================================================
	public class NodeInfoContainer
	{
		private ArrayList nodeInfos = new ArrayList();
		
		//--------------------------------------------------------------------------------
		public int Count { get { return nodeInfos.Count; } }

		//--------------------------------------------------------------------------------
		public SSafeNodeInfo NodeInfo { get { return (nodeInfos.Count == 1) ? nodeInfos[0] as SSafeNodeInfo : null; } }

		//--------------------------------------------------------------------------------
		public bool Active 
		{
			get 
			{
				foreach (SSafeNodeInfo i in nodeInfos)
					if (!i.Active) return false;
				return true;
			}
			set
			{
				foreach (SSafeNodeInfo i in nodeInfos)
					i.Active = value;
			}
		}

		//--------------------------------------------------------------------------------
		public string FullPath
		{ 
			get 
			{ 
				string path = "";
				foreach (string component in GroupedDynamicPathComponents)  
					path += component;
				return path;
			}
		}

		
		//--------------------------------------------------------------------------------
		public string LocalPhysicalPath
		{ 
			get 
			{ 
				if (nodeInfos.Count != 1)
					return CommonFunctions.multipleSelections;
				
				return ((SSafeNodeInfo) nodeInfos[0]).PhysicalLocalPath;
			}
		}

		//--------------------------------------------------------------------------------
		public string LocalLogicalPath
		{ 
			get 
			{ 
				if (nodeInfos.Count != 1)
					return CommonFunctions.multipleSelections;
				
				return ((SSafeNodeInfo) nodeInfos[0]).LocalPath;
			}

			set
			{
				if (nodeInfos.Count != 1)
				{
					System.Diagnostics.Debug.Fail("Cannot set LocalPath property when multiple node infos are present");
					return;
				}

				((SSafeNodeInfo) nodeInfos[0]).LocalPath = value;
			}
		}
		//--------------------------------------------------------------------------------
		public int Add(SSafeNodeInfo info)
		{
			return nodeInfos.Add(info);
		}

		//--------------------------------------------------------------------------------
		public void Clear()
		{
			nodeInfos.Clear();
		}

		//--------------------------------------------------------------------------------
		public int GetMaxComponentSize()
		{
			int max = 0;
			foreach (SSafeNodeInfo info in nodeInfos)
			{
				ArrayList components = info.GetDynamicPathComponents();
				if (components.Count > max)
					max = components.Count;
			}
			return max;
		}

		//--------------------------------------------------------------------------------
		public ArrayList GroupedDynamicPathComponents
		{
			get
			{
				ArrayList list = new ArrayList();
				
				int max = GetMaxComponentSize();
				bool foundSpecificPath = false;
				// se la componente di più basso livello è un path completo (inizia per $)
				// allora le altre componenti non le considero (vengono nascoste dal path specifico)
				for (int i = max-1; i >= 0 ; i--)
				{
					string element = foundSpecificPath ? string.Empty : GetCommonElementAt(i);
					if (element != null && element.StartsWith("$"))
						foundSpecificPath = true;

					list.Insert(0, element == null ? CommonFunctions.multipleSelections : element);
				}
				return list;

			}
		}
		
		//--------------------------------------------------------------------------------
		public void SetPathComponentAt(int index, string aValue)
		{
			foreach (SSafeNodeInfo info in nodeInfos)
				info.PathComponents[index] = aValue;
		}

		//--------------------------------------------------------------------------------
		private string GetCommonElementAt(int index)
		{
			
			string s = null;
			foreach (SSafeNodeInfo info in nodeInfos)
			{
				ArrayList components = info.GetDynamicPathComponents();
				if (components.Count <= index)
				{
					s = null;
					break;
				}
				else if (s == null)
				{
					s = components[index] as string;
				}
				else if (s != components[index] as string)
				{
					s = null;
					break;
				}
			}

			return s;
		}
	}

	public class TFSBinding
	{
		[XmlIgnore]
		private string invalidReason = "";

		public TFSBinding()
		{
			this.Port = 8080;
		}
		public TFSBinding(TFSBinding binding)
		{
			if (binding == null)
				return;

			this.Server = binding.Server;
			this.Port = binding.Port;
			this.User = binding.User;
			this.Password = binding.Password;
			this.Workspace = binding.Workspace;
		}

		public string Server { get; set; }
		public int Port { get; set; }
		public string User { get; set; }
		[XmlIgnore]
		public string Password { get; set; }
		[XmlIgnore]
		public string InvalidReason { get { return invalidReason; } }
		public string CryptedPassword
		{
			get { return Crypto.Encrypt(Password, ProjectBindingInfo.CryptKey, ProjectBindingInfo.CryptBlock); }
			set { Password = Crypto.Decrypt(value, ProjectBindingInfo.CryptKey, ProjectBindingInfo.CryptBlock); }
		}
		public string Workspace { get; set; }

		[XmlIgnore]
		public bool IsValid
		{
			get
			{
				try
				{
					using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(CommonFunctions.GetUri(Server, Port), new NetworkCredential(User, Password)))
					{
						VersionControlServer vcs = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));

						if (vcs.QueryWorkspaces(Workspace, User, Environment.MachineName).Length == 1)
							return true;
						invalidReason = "Workspace not found";
						return false;
					}
				}
				catch (Exception ex)
				{
					invalidReason = ex.Message;
					return false;
				}
			}
		}

		
	}
}
