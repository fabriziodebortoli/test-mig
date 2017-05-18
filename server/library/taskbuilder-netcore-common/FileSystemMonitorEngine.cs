using System;
using System.Collections;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using System.Xml;

using System.Resources;
using Microarea.Common.NameSolver;
using Microarea.Common.DiagnosticManager;
using TaskBuilderNetCore.Interfaces;
using System.IO;
using Microarea.Common.Generic;


//////////////////////////////////////////////
///				TODOBRUNA
//////////////////////////////////////////////
/// Per ora il monitoring del FSdisabilitato
/// Le estensioni dei files da includere in cache sono cablate
/// Diagnostica:
///		- da sistemare
///	Gestione del database
///		- vedere cosa tenere solo (niente o solo standard ?)
///	Gestione File System
//		- concludere removefolder
//  Da testare

///   LARA

//////////////////////////////////////////////
/// problema loginmanager  che a me serve solo x il token
/// lo lascio in sospeso come luca x il menu xche lo
/// metteremo apposto dopo
/// /////////////////////////////////////////
namespace Microarea.Common
{
	/// <summary>
	/// static object to refer FileSystem monitor engine into Web Methods
	/// </summary>
	//=========================================================================
	internal class FileSystemMonitor
	{
		#region Data Members
		private static FileSystemMonitorEngine engine = new FileSystemMonitorEngine();
		#endregion

		#region Properties
        internal static FileSystemMonitorEngine Engine { get { return engine; } }
        #endregion

		#region Construction and Destruction

		//-----------------------------------------------------------------------
		FileSystemMonitor ()
		{
		}

		#endregion
	}

	/// <summary>
	/// Engine to manage monitoring of file system
	/// </summary>
	//=========================================================================
	internal class FileSystemMonitorEngine
	{
		#region Data Members

		private const string cacheFileName = "FileSystemCache{0}.xml";

		// utility data members
	//	private LoginManager		loginManager		= null; TODO LARA
		private BasePathFinder		pathFinder			= null;
		private Diagnostic			diagnostic			= new Diagnostic("FileSystemMonitor"); 

		// system db data members
		private bool				databaseExist		= false;
		private	SqlConnection		connection			= new SqlConnection();

		// file system data members
		private enum				Action				{ Change, Add, Delete };
		private FileSystemWatcher	watcher				= null; //TODO LARA
		private string[]			managedExtensions	= null;
		private DateTime			lastTimeStamp		= System.DateTime.MinValue;
		private string				lastFileAccess		= string.Empty;

        #endregion

        #region Properties

        //       internal LoginManager		LoginManager	{ get { return loginManager; } } TODO LARA
        internal FileSystemWatcher	Watcher			{ get { return watcher; } }

        #endregion

        #region Construction and Destruction

        //-----------------------------------------------------------------------
        public FileSystemMonitorEngine()
		{
			diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "FileSystemMonitorEngine Init");
			managedExtensions = Database.Strings.ManagedExtensions.Split (';');

			pathFinder = BasePathFinder.BasePathFinderInstance;
            //LARA Dovrebbe farla gia dentro a .BasePathFinderInstance
            //pathFinder.Init ();
            FileSystem.InitServerPath (pathFinder);

// TODO LARA	loginManager = new LoginManager			(pathFinder.LoginManagerUrl, pathFinder.ServerConnectionInfo.WebServicesTimeOut);
			//watcher		 = new FileSystemWatcher	(pathFinder.GetRunningPath()); Lara
            watcher = new FileSystemWatcher(pathFinder.GetStandardPath());


            watcher.Filter		 = "*.*";
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
				
			watcher.IncludeSubdirectories = true;

			// Add event handlers.
			watcher.Created += new FileSystemEventHandler(OnFileCreated);
			watcher.Changed += new FileSystemEventHandler(OnFileChanged);
			watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
			watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
		}

		//-----------------------------------------------------------------------
		internal bool Init ()
		{
			if (CheckDatabase())
			{
				diagnostic.Set(DiagnosticType.LogInfo, "Correctly Initialized ....");
				return StartMonitor ();
			}

			return false;
		}

		//-----------------------------------------------------------------------
		public void Dispose()
		{
			CloseConnection ();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Checking methods
		//-----------------------------------------------------------------------
		internal bool IsValidToken (string authenticationToken)
		{
            //TODO LARA
            return true;
//#if DEBUG
//			if (authenticationToken == string.Empty)
//				return true;
//#endif
//			bool ok = loginManager.IsValidToken (authenticationToken);
//			if (!ok)
//				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, FileSystemMonitorStrings.AuthenticationFailed);
				
//			return ok;
		}

		#endregion

		#region Monitoring Management

		//-----------------------------------------------------------------------
		internal bool StartMonitor ()
		{
			if (!databaseExist)
				return false;
			
			diagnostic.Set(DiagnosticType.LogInfo, FileSystemMonitorStrings.MonitoringStarted);
			watcher.EnableRaisingEvents = false;
			return true;
		}

		//-----------------------------------------------------------------------
		internal bool StopMonitor ()
		{
			diagnostic.Set(DiagnosticType.LogInfo, FileSystemMonitorStrings.MonitoringStopped);
			watcher.EnableRaisingEvents = false;
			return true;
		}

		//-----------------------------------------------------------------------
		private bool IsFileAlreadyChanged (string fileName, DateTime lastWrite)
		{
			if( lastWrite == lastTimeStamp && lastFileAccess == fileName)
				return true;
			else
			{
				lastTimeStamp =lastWrite;
				lastFileAccess = fileName;
				return false;
			}
		}

		//-----------------------------------------------------------------------
		private void OnFileCreated (object source, FileSystemEventArgs e)
		{
			// not a directory
			if (!IsADirectory (e.FullPath))
			{	
				if (!IsAManagedFile (e.FullPath))
					return;
				if (pathFinder.IsStandardPath(e.FullPath))
					UpdateStandardFileIntoDatabase (e.FullPath, Action.Add);
				else
					UpdateCustomFileIntoDatabase (e.FullPath, Action.Add);
				return;
			}
			if (pathFinder.IsStandardPath(e.FullPath))
				SynchronizeStandardIntoDatabase(e.FullPath);
			else
				SynchronizeCustomIntoDatabase(e.FullPath);
		}

		//-----------------------------------------------------------------------
		private void OnFileChanged (object source, FileSystemEventArgs e)
		{	
			// not a directory
			if (IsFileAlreadyChanged(e.FullPath, File.GetLastWriteTime(e.FullPath)))
				return;

			if (!IsADirectory (e.FullPath))
			{
				if (!IsAManagedFile (e.FullPath))
					return;
				if (pathFinder.IsStandardPath(e.FullPath))
					UpdateStandardFileIntoDatabase (e.FullPath, Action.Change);
				else
					UpdateCustomFileIntoDatabase (e.FullPath, Action.Change);
			}
		}

		//-----------------------------------------------------------------------
		private void OnFileRenamed (object source, RenamedEventArgs e)
		{
			// a directory
			if (IsADirectory (e.FullPath))
			{
				if (pathFinder.IsStandardPath(e.FullPath))
					RenameStandardDirectoryIntoDataBase(e.FullPath,e.OldFullPath );							
				else 
					RenameCustomDirectoryIntoDataBase(e.FullPath,e.OldFullPath );
				return; 
			}

			// a file
			if (IsAManagedFile (e.OldFullPath))
			{
				if (pathFinder.IsStandardPath(e.OldFullPath))
					UpdateStandardFileIntoDatabase (e.OldFullPath, Action.Delete);
				else
					UpdateCustomFileIntoDatabase (e.OldFullPath, Action.Delete);
			}

			if (IsAManagedFile (e.FullPath))
			{
				if (pathFinder.IsStandardPath(e.FullPath))
					UpdateStandardFileIntoDatabase (e.FullPath, Action.Add);
				else
					UpdateCustomFileIntoDatabase (e.FullPath, Action.Add);
			}	
		}

		//-----------------------------------------------------------------------
		private void OnFileDeleted (object source, FileSystemEventArgs e)
		{
			//se è una directory cancella dal database tutte le righe che hanno questo path
			if (IsADirectory (e.FullPath))
			{
				if (pathFinder.IsStandardPath(e.FullPath))
					DeleteStandardDirectoryIntoDataBase(e.FullPath);
				else
					DeleteCustomDirectoryIntoDataBase(e.FullPath);
				return;
			}
			//Se non è una directory	
			if (!IsAManagedFile (e.FullPath))
				return;

			if (pathFinder.IsStandardPath(e.FullPath))
				UpdateStandardFileIntoDatabase (e.FullPath, Action.Delete);
			else
				UpdateCustomFileIntoDatabase (e.FullPath, Action.Delete);
		}

		#endregion

		#region File System Management

		//-----------------------------------------------------------------------
		private bool IsAManagedFile (string fileName)
		{
			// is a directory
			FileInfo info = new FileInfo (fileName);
			if (string.Compare (info.DirectoryName, fileName, true) == 0)
				return true;

			string fileExt = "*" + info.Extension;

			foreach (string extension in managedExtensions)
				if (string.Compare (fileExt, extension, true) == 0)
					return true;
		
			return false;
		}

        //TODO LARA
        ////-------------------------------------------------------------------------
        private string GetTbCacheFileName()
        {
            //	string fileName = string.Format (cacheFileName, pathFinder.InstallationAbstract.LatestUpdate);

            //	fileName = fileName.Replace (":", "-");
            //	fileName = pathFinder.GetCustomPath () + Path.DirectorySeparatorChar + fileName;

            //	return fileName;
            return "";
        }		

        //-------------------------------------------------------------------------
        internal bool GetTbCacheFile (out string fileContent)
		{
			fileContent = string.Empty;

			string fileName = GetTbCacheFileName();

			try
			{
				XmlDocument	doc = new XmlDocument();

				if (!File.Exists (fileName))
					return true;

				doc.Load(File.OpenRead(fileName));
				fileContent = doc.InnerXml.ToString();
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format("TODODIAGNOSTIC", e.Message)
					);
				return false;
			}
			
			return true;
		}

		//-------------------------------------------------------------------------
		internal bool CreateTbCacheFile ()
		{
			// header
			XmlDocument doc = new XmlDocument();

			try
			{
				XmlNode ms = doc.CreateNode(XmlNodeType.Element, "MicroareaServer", "" );
				doc.AppendChild(ms);

				// only standard, custom is excluded
				XmlNode st = doc.CreateNode(XmlNodeType.Element, "Standard", "" );
				ms.AppendChild(st);
				WriteCacheFile (ref st, ref doc, pathFinder.GetStandardPath(), FileSystem.ExcludedPath, FileSystem.IncludedFiles);

				// save della cache
				string fileName = GetTbCacheFileName();
				doc.Save (File.OpenRead(fileName));
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return false;
			}

			return true;
		}

		
		//-------------------------------------------------------------------------
		private bool WriteCacheFile (ref XmlNode currentNode, ref XmlDocument doc, string path, Hashtable pathExclusions, Hashtable fileInclusions)
		{
			if (path == null || !Directory.Exists(path))
				return true;

			try
			{
				// directories
				string[] dirs		= Directory.GetDirectories(path);
				string	 lowerPath	= path.ToLower () + Path.DirectorySeparatorChar;
			
				foreach(string dir in dirs) 
				{
					string name = dir.ToLower().Replace (lowerPath, "");
					
					if (pathExclusions.ContainsKey(name))
						continue;

					XmlNode newNode = null;

					switch (currentNode.Name)
					{
						case "Standard":
							newNode = doc.CreateNode(XmlNodeType.Element, "Container", "" );
							break;
						case "Container":
							newNode = doc.CreateNode(XmlNodeType.Element, "Application", "" );
							break;
						case "Application":
							newNode = doc.CreateNode(XmlNodeType.Element, "Module", "" );
							break;
						default:
							newNode = doc.CreateNode(XmlNodeType.Element, "Path", "" );
							break;
					}

					currentNode.AppendChild(newNode);

					XmlAttribute nameAttr = doc.CreateAttribute ("name");
					nameAttr.Value = name;
                    newNode.Attributes.Append (nameAttr);

					WriteCacheFile (ref newNode, ref doc, dir, pathExclusions, fileInclusions);
				}

				// files
				string[] files = Directory.GetFiles(path, "*.*");
				
				foreach(string file in files) 
				{
					string name = file.ToLower().Replace (lowerPath, "");
					string ext	= Path.GetExtension(name);

					if (!fileInclusions.ContainsKey(ext))
						continue;

					XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "File", "" );

					currentNode.AppendChild(newNode);
					XmlAttribute nameAttr = doc.CreateAttribute ("name");
					nameAttr.Value = name;
					newNode.Attributes.Append (nameAttr);
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return false;
			}
			return true;
		}

		//-------------------------------------------------------------------------
		internal bool GetServerConnectionConfig (out string fileContent)
		{
			return GetTextFile (pathFinder.ServerConnectionFile, out fileContent);
		}

		//-----------------------------------------------------------------------
		internal bool GetTextFile (string theFileName, out string fileContent)
		{
			fileContent = string.Empty;

			if (theFileName == string.Empty)
				return false;

			string fileName = GetAdjustedPath(theFileName); 
			
			if (!File.Exists(fileName))
				return false;
			try
			{
				// file content
				StreamReader sr = new StreamReader(File.OpenRead(fileName), true);
				fileContent = sr.ReadToEnd();
                //sr.Close (); Lara
                sr.Dispose();
			}
			catch (Exception e)
			{
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		internal bool SetTextFile (string theFileName, string fileContent)
		{
			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			StopMonitor();
			string path = Path.GetDirectoryName (fileName);
			
			// recursive path create
			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);

			try
			{
				// write operation
				StartMonitor();
                //Lara
                //	StreamWriter sw = new StreamWriter(File.OpenRead(fileName), false,System.Text.Encoding.UTF8);
                StreamWriter sw = new StreamWriter(File.OpenRead(fileName), System.Text.Encoding.UTF8);
                sw.Write(fileContent);
                //sw.Close (); Lara
                sw.Dispose();
				
				return true;
			}
			catch (IOException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning,	
					string.Format(FileSystemMonitorStrings.WriteTextError, e.Message)
					);
				return false;
			}
		}

		//-----------------------------------------------------------------------
		internal bool ExistFile (string theFileName)
		{
			if (theFileName == string.Empty)
				return false;

			string fileName = GetAdjustedPath(theFileName); 
			
			return File.Exists(fileName);
		}

		//-----------------------------------------------------------------------
		internal bool ExistPath (string thePathName)
		{
			if (thePathName == string.Empty)
				return false;

			string pathName = GetAdjustedPath(thePathName); 

			return Directory.Exists(pathName);
		}

		//-----------------------------------------------------------------------
		private ArrayList GetFileSystemStructure (string thePathName, string[] extensions)
		{
			string path = GetAdjustedPath(thePathName); 

			ArrayList files = new ArrayList();
			if (path == null || !Directory.Exists(path))
				return files;
	
			try
			{
				string[] dirs = Directory.GetDirectories(path);
			
				foreach (string extension in extensions)
					files.AddRange(Directory.GetFiles(path, extension));

			
				foreach(string dir in dirs) 
				{
					files.Add (dir);
					files.AddRange(GetFileSystemStructure(dir, extensions));
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return null;
			}
			return files;
		}

		//-----------------------------------------------------------------------
		internal bool GetPathContent (string thePathName, string fileExtension, out string returnDoc, bool folders, bool files)
		{
			returnDoc = string.Empty;

			if (thePathName == string.Empty)
				return false;

			string pathName = GetAdjustedPath(thePathName); 

			if (!Directory.Exists(pathName))
				return false;

			
			try
			{
				string [] subFolders = null;
				if (folders)
					subFolders = Directory.GetDirectories (pathName);

				string [] subFiles = null;
				if (files)
					subFiles = Directory.GetFiles (pathName, fileExtension);

				if (subFolders == null && subFiles == null)
					return true;

				XmlDocument doc = new XmlDocument();
				XmlNode ln = doc.CreateNode(XmlNodeType.Element, "List", "" );
				doc.AppendChild(ln);

				if (folders)
				{
					foreach (string folder in subFolders)
					{
						XmlNode obj = doc.CreateNode(XmlNodeType.Element, "Folder", "" );
						XmlAttribute nameAttr = doc.CreateAttribute ("name");
						nameAttr.Value = Path.GetFileName(folder);
						obj.Attributes.Append (nameAttr);
						ln.AppendChild(obj);
					}
				}

				if (files)
				{
					foreach (string file in subFiles)
					{
						XmlNode obj = doc.CreateNode(XmlNodeType.Element, "File", "" );
						XmlAttribute nameAttr = doc.CreateAttribute ("name");
						nameAttr.Value = Path.GetFileName(file);
						obj.Attributes.Append (nameAttr);
						ln.AppendChild(obj);
					}
				}

				returnDoc = doc.InnerXml.ToString();			
			}
			catch(SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format( "TODODIAGNOSTIC", e.Message)
					);
				return false;
			}

			return true;
		}

		//-------------------------------------------------------------------------
		private ArrayList GetFiles (string thePath, string[] extensions)
		{
			string path = GetAdjustedPath(thePath); 

			ArrayList files = new ArrayList();
			if (path == null || !Directory.Exists(path))
				return files;
			try
			{
				string[] dirs = Directory.GetDirectories(path);
				
				foreach (string extension in extensions)
					files.AddRange(Directory.GetFiles(path, extension));
				
				foreach(string dir in dirs) 
				{
					files.AddRange(GetFiles(dir, extensions));
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return null;
			}
			return files;
		}
		
		//-------------------------------------------------------------------------
		private bool IsADirectory (string fileName)
		{
			if (fileName == null)
				return false;

			// is a directory
			FileAttributes attributes = File.GetAttributes (fileName);
			return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
		}

		//-------------------------------------------------------------------------
		private bool CheckFile (string fileName)
		{
			if (fileName == null)
				return false;

			try
			{
				FileAttributes attributes = File.GetAttributes (fileName);
				bool readOnly	= (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
				bool hidden		= (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
				bool isADir		= (attributes & FileAttributes.Directory) == FileAttributes.Directory;
				bool system		= (attributes & FileAttributes.System) == FileAttributes.System;
			}
			catch(Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.AttributesCheckFailed, fileName)
					);
				return false;
			}

			return true;
		}


        //-----------------------------------------------------------------------
        private bool SetFileWriteAttributes (string fileName)
        {
			if (fileName == null)
				return false;

			try
			{
                // Setting write attributes out from monitoring the file
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Exists)
                {
					// remove read-only attribute to the file
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        fileInfo.Attributes -= FileAttributes.ReadOnly;
                 //   Lara
				//	FileIOPermission fp = new FileIOPermission (FileIOPermissionAccess.Write, fileInfo.FullName);
                }
            }
            catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.WriteAttributesFailed, fileName)
					);
				return false;
			}

            return true;
        }

        //-----------------------------------------------------------------------
        private bool SetFolderWriteAttributes(string pathName)
        {
			if (pathName == null)
				return false;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(pathName);
                if (dirInfo.Exists)
                {
					// remove read-only attribute to the file
                    if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        dirInfo.Attributes -= FileAttributes.ReadOnly;
                    //Lara
					//FileIOPermission fp = new FileIOPermission (FileIOPermissionAccess.Write, pathName);
                }
            }
            catch (Exception e)
            {
                diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.WriteAttributesFailed , pathName)
					);
                return false;
            }
			return true;
        }

        //-----------------------------------------------------------------------
        internal bool RemoveFolder(string thePathName, bool recursive, bool emptyOnly)
        {
			string pathName = GetAdjustedPath(thePathName); 

			if (pathName == null)
				return false;

            StopMonitor();
            if (!Directory.Exists(pathName))
            {
                StartMonitor();
                return true;
            }
            StartMonitor();

			try
			{
                if (SetFolderWriteAttributes (pathName))
                    Directory.Delete(pathName, recursive);
            }
            catch(Exception e)
			{
				diagnostic.Set(				
					DiagnosticType.LogInfo | DiagnosticType.Error, 					
					string.Format(FileSystemMonitorStrings.DeleteAttemptFailed, pathName)
					);
				return false;
			}

            return true;
        }

		//-----------------------------------------------------------------------
		internal bool CreateFolder (string thePathName, bool recursive)
		{
			string pathName = GetAdjustedPath(thePathName); 

			StopMonitor();
			if (Directory.Exists(pathName))
			{
				StartMonitor();
				return false;
			}

			StartMonitor();

			try
			{
				Directory.CreateDirectory(pathName);
			}
			catch(Exception e)
			{
				diagnostic.Set(				
					DiagnosticType.LogInfo | DiagnosticType.Error, 					
					string.Format("TODODIAGNOSTIC", pathName)
					);
				return false;
			}

			return true;
		}

		//-----------------------------------------------------------------------
        internal bool CopyFolder(string theOldPathName, string theNewPathName, bool recursive)
        {
			if (theOldPathName == null || theNewPathName == null)
				return false;

			string oldPathName = GetAdjustedPath(theOldPathName); 
			string newPathName = GetAdjustedPath(theNewPathName); 

			if (!FileAlreadyExists(oldPathName))
				return false;

            try
            {
				if (SetFileWriteAttributes(oldPathName) && SetFileWriteAttributes(newPathName))
					CopyAllDirectories(oldPathName,newPathName);
            }
            catch (Exception e)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.CopyAttemptFailed , newPathName)
					);
                return false;
            }

            return true;
        }

		//-----------------------------------------------------------------------
		private static void CopyAllDirectories(string oldPath, string newPath)
		{
			String[] files;

			if (newPath[newPath.Length-1] != Path.DirectorySeparatorChar) 
				newPath += Path.DirectorySeparatorChar;

			if (!Directory.Exists(newPath)) 
				Directory.CreateDirectory(newPath);

			files = Directory.GetFileSystemEntries(oldPath);
			foreach (string file in files)
			{
				// Sub-folders
				if (Directory.Exists(file)) 
					CopyAllDirectories(file, newPath + Path.GetFileName(file));
				
				// Files 
				else 
					File.Copy(file, newPath + Path.GetFileName(file), true);
			}
		}

        //-----------------------------------------------------------------------
        internal bool RemoveFile(string theFileName)
        {
			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			if (!FileAlreadyExists(fileName))
				return false;

            try
            {
                if (SetFileWriteAttributes (fileName))
                    File.Delete(fileName);
            }
            catch (Exception e)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DeleteAttemptFailed ,fileName)
					);
                return false;
            }

            return true;
        }

		//-----------------------------------------------------------------------
		internal bool CopyFile(string theOldFileName, string theNewFileName, bool overWrite)
		{
			if (theOldFileName == null || theNewFileName == null)
				return false;

			string oldFileName = GetAdjustedPath(theOldFileName); 
			string newFileName = GetAdjustedPath(theNewFileName); 

			if (!FileAlreadyExists(oldFileName))
				return false;

			try
			{
				if (SetFileWriteAttributes(oldFileName) && SetFileWriteAttributes(newFileName))
					File.Copy(oldFileName, newFileName);
			}
			catch (Exception e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.CopyAttemptFailed , newFileName)
					);
				return false;
			}
			return true;
		}

        //-----------------------------------------------------------------------
        internal bool RenameFile(string theOldFileName, string theNewFileName)
        {
			if (theOldFileName == null || theNewFileName == null)
				return false;

			string oldFileName = GetAdjustedPath(theOldFileName); 
			string newFileName = GetAdjustedPath(theNewFileName); 

			if (!FileAlreadyExists(oldFileName))
				return false;

            try
            {
                if (SetFileWriteAttributes(oldFileName) && SetFileWriteAttributes(newFileName))
                    File.Move(oldFileName, newFileName);
            }
            catch (Exception e)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.RenameAttemptFailed , newFileName)
					);
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------
        internal bool GetFileStatus (
                                            string       theFileName, 
                                            out DateTime creation,
                                            out DateTime lastAccess,
                                            out DateTime lastWrite,
                                            out long     length
                                        )
        {
			creation   = DateTime.MinValue;
			lastAccess = DateTime.MinValue;
			lastWrite  = DateTime.MinValue;
			length     = 0;

			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			FileInfo fileInfo = new FileInfo(fileName);	

			if (!fileInfo.Exists)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return false;
			}
            try
            {
                creation   = fileInfo.CreationTime;
                lastAccess = fileInfo.LastAccessTime;
                lastWrite  = fileInfo.LastWriteTime;
                length     = fileInfo.Length;
		    }
            catch (Exception e)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.ReadAttributesFailed , fileName)
					);
                StartMonitor();
                return false;
            }
            
            StartMonitor();
            return true;
        }

        //-----------------------------------------------------------------------
        internal int GetFileAttributes (string theFileName)
        {
			if (theFileName == null)
				return 0;

			string fileName = GetAdjustedPath(theFileName); 

			FileInfo fileInfo = new FileInfo(fileName);	
			if (!fileInfo.Exists)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return 0;
			}

			FileAttributes enumsAttributes = fileInfo.Attributes;
			return (int)enumsAttributes;
        }

		//-----------------------------------------------------------------------
		private bool FileAlreadyExists (string fileName)
		{
			if (!File.Exists(fileName))
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return false;
			}
			else 
				return true;
		}

		//-----------------------------------------------------------------------
		private string GetAdjustedPath (string pathName)
		{
			string path = pathName.ToLower();

			if (!path.StartsWith (FileSystem.ServerPath))
				return pathName;

			string localPath = path.Replace (FileSystem.ServerPath, "");

			if (localPath.StartsWith (NameSolverStrings.Running.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Running.ToLower(), "");
				//Lara
                //localPath = pathFinder.GetRunningPath() + localPath;
                localPath = pathFinder.GetStandardPath() + localPath;

            } 
			else if (localPath.StartsWith (NameSolverStrings.Standard.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Standard.ToLower(), "");
				localPath = pathFinder.GetStandardPath() + localPath;
			}
			else if (localPath.StartsWith (NameSolverStrings.Custom.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Custom.ToLower(), "");
				localPath = pathFinder.GetCustomPath() + localPath;
			}

			return localPath;
		}

        #endregion

		#region Database Management

		//-----------------------------------------------------------------------
		internal bool SynchronizeDatabase()
		{
			if (!databaseExist)
				return false;
			
			StopMonitor ();

			// I check the connection to the database
			if (!OpenConnection())
				return false;
			
			bool ok =  SynchronizeStandardIntoDatabase () && SynchronizeCustomIntoDatabase ();
			StartMonitor ();
			return ok; 
		}

		//-----------------------------------------------------------------------
		internal bool SynchronizeStandardIntoDatabase()
		{
			if (!CleanTable (Database.Strings.StandardTableName))
				return false;

			return SynchronizeStandardIntoDatabase(pathFinder.GetStandardPath());
		}

		//-----------------------------------------------------------------------
		internal bool SynchronizeStandardIntoDatabase (string filepath)
		{
			if (!OpenConnection ())
				return false;
				
			SqlCommand sqlCommand = new SqlCommand();
			try
			{
				sqlCommand.CommandText			= Database.Strings.StandardInsertAFileQuery;
				sqlCommand.Connection			= connection;
				Database.Parameters parameters = new Database.Parameters ();
				parameters.PrepareAll (sqlCommand);
				sqlCommand.Prepare();

				// text extensions selections
				ArrayList files = GetFileSystemStructure (filepath, managedExtensions);

				if (files != null)
				{
					string file = string.Empty;
					for (int i= files.Count-1; i >= 0; i--)
					{
						file = files[i] as string;
						AssignRecordFromFileName (file, parameters);
						sqlCommand.ExecuteNonQuery();	
						files.RemoveAt (i);
					}
				}
			}
			catch(Exception e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted , e.Message)
					);
				return false;
			}
			sqlCommand.Dispose();
			return true;
		}

		//-----------------------------------------------------------------------
		internal bool SynchronizeCustomIntoDatabase()
		{
			if (!CleanTable (Database.Strings.CustomTableName))
			{
				return false;
			}
			return SynchronizeCustomIntoDatabase(pathFinder.GetCustomPath());
		}

		//-----------------------------------------------------------------------
		internal bool SynchronizeCustomIntoDatabase(string filePath)
		{
			SqlCommand sqlCommand = new SqlCommand();

			try
			{
				sqlCommand.CommandText			= Database.Strings.CustomInsertAFileQuery;
				sqlCommand.Connection			= connection;

				Database.Parameters parameters = new Database.Parameters ();
				parameters.PrepareAll (sqlCommand);
				sqlCommand.Prepare();

				// text extensions selections
				ArrayList files = GetFileSystemStructure (filePath, managedExtensions);

				string file = string.Empty;
				for (int i= files.Count-1; i >= 0; i--)
				{
					file = files[i] as string;
					AssignRecordFromFileName (file, parameters);
					sqlCommand.ExecuteNonQuery();	
					files.RemoveAt (i);
				}
			}
			catch(Exception e)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(FileSystemMonitorStrings.QueryNotExecuted,e.Message));
				return false;
			}

			sqlCommand.Dispose();
			return true;
		}

		
		//-----------------------------------------------------------------------
		private bool CleanTable(string tableName)
		{
			if (tableName == string.Empty)
			{
				Debug.Assert(false);
				return false;
			}

			string query = string.Format(Database.Strings.DeleteAllQuery, tableName);

			try
			{

				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.CommandText = query;
				sqlCommand.Connection = connection;
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();
			}
			catch (Exception e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted,e.Message)
					);
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		private bool RenameStandardDirectoryIntoDataBase (string  newfileName, string oldfileName)
		{
			if (!OpenConnection ())
				return false;
				
			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				string relativeOldfileName = GetPathNameColumnValue (oldfileName);
				string relativeNewfileName = GetPathNameColumnValue (newfileName);

				sqlCommand.CommandText = string.Format(Database.Strings.StandardDirectoryRenameQuery, relativeNewfileName, relativeOldfileName );

				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}
			return true;
		}

		
		//-----------------------------------------------------------------------
		private bool RenameCustomDirectoryIntoDataBase (string  newfileName, string oldfileName)
		{
			if (!OpenConnection ())
				return false;

			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				string relativeOldfileName = GetPathNameColumnValue (oldfileName);
				string relativeNewfileName = GetPathNameColumnValue (newfileName);

				sqlCommand.CommandText = string.Format(Database.Strings.CustomDirectoryRenameQuery, relativeNewfileName, relativeOldfileName );

				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}
			return true;
		}


		//-----------------------------------------------------------------------
		private bool DeleteStandardDirectoryIntoDataBase (string fileName)
		{
			if (!OpenConnection ())
				return false;
			
			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				string relativeFileName = GetPathNameColumnValue (fileName);
				sqlCommand.CommandText	= string.Format(Database.Strings.StandardDirectoryDeleteQuery, relativeFileName);

				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}

			return true;

		}
		//-----------------------------------------------------------------------
		private bool DeleteCustomDirectoryIntoDataBase (string fileName)
		{
			if (!OpenConnection ())
				return false;
				
			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				string relativeFileName = GetPathNameColumnValue (fileName);
				sqlCommand.CommandText	= string.Format(Database.Strings.CustomDirectoryDeleteQuery, relativeFileName);

				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}

			return true;

		}

		//-----------------------------------------------------------------------
		private bool UpdateStandardFileIntoDatabase (string fileName, Action action)
		{
			if (action != Action.Delete &!CheckFile (fileName))
				return false;

			// I check the connection to the database
			if (!OpenConnection ())
				return false;

			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;
				Database.Parameters parameters = new Database.Parameters ();

				switch (action) 
				{
					case Action.Add:
						sqlCommand.CommandText	= Database.Strings.StandardInsertAFileQuery;
						parameters.PrepareAll (sqlCommand);
						break;
					case Action.Delete:
						sqlCommand.CommandText	= string.Format (Database.Strings.DeleteAFileQuery, Database.Strings.StandardTableName);
						parameters.PreparePathName		(sqlCommand);
						parameters.PrepareFileName		(sqlCommand);
						break;
					case Action.Change:
						sqlCommand.CommandText	= Database.Strings.StandardUpdateAFileQuery;
						parameters.PreparePathName		(sqlCommand);
						parameters.PrepareFileName		(sqlCommand);
						parameters.PrepareFileDataText	(sqlCommand);
						break;
				}

				AssignRecordFromFileName (fileName, parameters);
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		private bool UpdateCustomFileIntoDatabase (string fileName, Action action)
		{
			if (!CheckFile (fileName))
				return false;

			if (!OpenConnection ())
				return false;
				
			try
			{
				SqlCommand sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;
				Database.Parameters parameters = new Database.Parameters ();

				switch (action) 
				{
					case Action.Add:
						sqlCommand.CommandText	= Database.Strings.CustomInsertAFileQuery;
						parameters.PrepareAll (sqlCommand);
						break;
					case Action.Delete:
						sqlCommand.CommandText	= string.Format (Database.Strings.DeleteAFileQuery, Database.Strings.CustomTableName);
						parameters.PreparePathName		(sqlCommand);
						parameters.PrepareFileName		(sqlCommand);
						break;
					case Action.Change:
						sqlCommand.CommandText	= Database.Strings.CustomUpdateAFileQuery;
						parameters.PreparePathName		(sqlCommand);
						parameters.PrepareFileName		(sqlCommand);
						parameters.PrepareFileDataText	(sqlCommand);
						break;
				}

				AssignRecordFromFileName (fileName, parameters);
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose ();
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.QueryNotExecuted	, e.Message)
					);
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		private bool CheckSingleColumn (Hashtable columnsFound, string columnName, string columnType)
		{
			if (!columnsFound.Contains(columnName))
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.ColumnNotFound , columnName ,Environment.NewLine)
					);
				return false;
			}

            //Lara    
            //DataRow column = (DataRow) columnsFound[columnName];

            //TBType providerType = (TBType) ((SqlDbType) column["ProviderType"]);
            //string typeFound = TBDatabaseType.GetDBDataType(providerType , DBMSType.SQLSERVER);

   //         if (
			//		string.Compare(typeFound, "N" + columnType, true) != 0 &&
			//		string.Compare(typeFound, columnType, true) != 0
			//	) 
			//{
			//	diagnostic.Set(
			//		DiagnosticType.LogInfo | DiagnosticType.Error,
			//		string.Format( FileSystemMonitorStrings.WrongTypeColumn , Database.Strings.FileNameColumnName)
			//		);
			//	return false;
			//}
			return true;
		}
		//-----------------------------------------------------------------------
		private bool CheckStandardTableColumns(DataTable table)
		{
			Hashtable columnsFound = new Hashtable();
			bool ok = true;
			try
			{
                //Lara
				//foreach (DataRow col in table.Rows)
				//	columnsFound.Add(col["ColumnName"].ToString(), col);

				ok = CheckSingleColumn (columnsFound, Database.Strings.FileNameColumnName, SqlDbType.VarChar.ToString ());
				ok = CheckSingleColumn (columnsFound, Database.Strings.PathNameColumnName, SqlDbType.VarChar.ToString ())		&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.CultureColumnName,  SqlDbType.VarChar.ToString ())		&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.LocalizationColumnName, SqlDbType.VarChar.ToString ())	&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.FileDataTextColumnName, SqlDbType.Text.ToString ())		&& ok;
			}

			catch (Exception e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format(FileSystemMonitorStrings.ErrorInDatabase , Database.Strings.StandardTableName)
					);
				return false;
			}

			return ok;
		}

		//-----------------------------------------------------------------------
		private bool CheckCustomTableColumns(DataTable table)
		{
			Hashtable columnsFound = new Hashtable();
			bool ok = true;
			try
			{
                //Lara
				//foreach (DataRow col in table.Rows)
				//	columnsFound.Add(col["ColumnName"].ToString(), col);

				ok = CheckSingleColumn (columnsFound, Database.Strings.FileNameColumnName, SqlDbType.VarChar.ToString ());
				ok = CheckSingleColumn (columnsFound, Database.Strings.PathNameColumnName, SqlDbType.VarChar.ToString ())		&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.CultureColumnName,  SqlDbType.VarChar.ToString ())		&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.LocalizationColumnName, SqlDbType.VarChar.ToString ())	&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.FileDataTextColumnName, SqlDbType.Text.ToString ())		&& ok;
/*				ok = CheckSingleColumn (columnsFound, Database.Strings.CompanyIDColumnName, SqlDbType.Int.ToString ())			&& ok;
TODOBRUNA		ok = CheckSingleColumn (columnsFound, Database.Strings.RoleIDColumnName, SqlDbType.Int.ToString ())				&& ok;
				ok = CheckSingleColumn (columnsFound, Database.Strings.LoginIDColumnName, SqlDbType.Int.ToString ())			&& ok;
*/
			}
			catch (Exception e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning,	
					string.Format(FileSystemMonitorStrings.ErrorInDatabase , Database.Strings.CustomTableName)
					);
				return false;
			}

			return ok;
		}

		//-----------------------------------------------------------------------
		private bool CheckDatabase()
		{
            //Lara
		//	TBConnection myConnection = null;			
			OpenConnection();

            //try
            //{
            //	myConnection = new TBConnection(connection.ConnectionString, DBMSType.SQLSERVER);
            //	myConnection.Open();
            //	TBDatabaseSchema mySchema = new TBDatabaseSchema(myConnection);

            //	// table existance
            //	if	(
            //		!mySchema.ExistTable(Database.Strings.StandardTableName) || 	
            //		!mySchema.ExistTable(Database.Strings.CustomTableName)
            //		)
            //	{
            //		myConnection.Close();
            //		myConnection.Dispose();
            //		databaseExist = false;
            //	}

            //	// columns checking
            //	DataTable standardCols = mySchema.GetTableSchema(Database.Strings.StandardTableName, false);
            //	DataTable customCols = mySchema.GetTableSchema(Database.Strings.CustomTableName, false);

            //	if (CheckStandardTableColumns(standardCols) && CheckCustomTableColumns(customCols))
            //		databaseExist = true;
            //	else 
            //		databaseExist = false;
            //}
            //catch(TBException e)
            //{
            //	diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning,
            //		string.Format(FileSystemMonitorStrings.GenericDatabaseError , e.Message)
            //		);
            //	return databaseExist = false;
            //}
            //finally
            //{
            //	if (myConnection.State == ConnectionState.Open || myConnection.State == ConnectionState.Broken)
            //	{
            //		myConnection.Close(); 
            //		myConnection.Dispose();
            //	}
            //}
            //return databaseExist;
            return true;
		}

		//----------------------------------------------------------------------
		private bool OpenConnection ()
		{
			if (connection.State == ConnectionState.Open)
				return true;

			string connectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;
			if (connectionString == string.Empty)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning,	FileSystemMonitorStrings.PathFinderStringEmpty);
				return false;
			}

			try
			{
				connection.ConnectionString = connectionString;
				connection.Open ();
			}
			catch(SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format( FileSystemMonitorStrings.ConnectionNotOpened, e.Message)
					);
				return false;
			}

			return true;
		}

		//----------------------------------------------------------------------
		private void CloseConnection()
		{
			try
			{
				if (connection.State == ConnectionState.Open)
					connection.Close();

				connection.ConnectionString = string.Empty;
			}
			catch (SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.ConnectionNotClosed , e.Message)
					);
			}
		}

		//-------------------------------------------------------------------------
		private string GetPathNameColumnValue (string file)
		{
			// da correggere, se è una directory non deve amputare la parte finale dello string
			if (IsADirectory(file))
				return PathReplace(file);

			string relativePath = Path.GetDirectoryName (file).ToLower();
			return PathReplace(relativePath);
		}

		//-------------------------------------------------------------------------
		private string PathReplace(string filepath)
		{
			string pathToReplace = pathFinder.IsStandardPath(filepath) ? pathFinder.GetStandardPath() : pathFinder.GetCustomPath();
			pathToReplace = pathToReplace.ToLower();
			filepath = filepath.ToLower().Replace(pathToReplace, "");

			if (filepath.StartsWith (Path.DirectorySeparatorChar.ToString()))
				filepath = filepath.Substring (1, filepath.Length-1);

			return filepath;
		}

		//-------------------------------------------------------------------------
		private void AssignRecordFromFileName (string file, Database.Parameters parameters)
		{
			if (file == string.Empty)
				return;

			bool isADirectory = IsADirectory (file);

			if (parameters.Namespace != null)
			{
                //Lara
                //NameSpace nameSpace = pathFinder.GetNamespaceFromPath (file);
                INameSpace nameSpace = pathFinder.GetNamespaceFromPath(file);
                if (nameSpace == null)
					parameters.Namespace.Value = "";
				else
					parameters.Namespace.Value = nameSpace.ToString();
			}
			
			if (parameters.Type != null)
				parameters.Type.Value = Database.TypeValues.Startup;

			if (parameters.AdditionalKey != null)
				parameters.AdditionalKey.Value = "";
	
			if (parameters.FileName != null)
			{
				if (isADirectory)
					parameters.FileName.Value = string.Empty;
				else
					parameters.FileName.Value = Path.GetFileName (file);
			}
					
			if (parameters.PathName != null)
				parameters.PathName.Value = GetPathNameColumnValue(file);

			if (parameters.Culture != null)
				parameters.Culture.Value = "";

			if (parameters.Localization != null)
				parameters.Localization.Value = "";

			if (parameters.FileDataText != null) 
			{
				if (isADirectory)
					parameters.FileDataText.Value = "";
				else
				{
					try
					{
                        // file content
                        //Lara
                        StreamReader sr = new StreamReader(File.OpenRead(file), true);

                       // StreamReader sr = new StreamReader(file, true);
						parameters.FileDataText.Value = sr.ReadToEnd();
                        //sr.Close ();
                        sr.Dispose();
					}
					catch (IOException e)
					{
						diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Error,
							string.Format(FileSystemMonitorStrings.ReadTextError , e.Message)
							);
					}
				}
			}
	
			#endregion

		}
		
		//=========================================================================
		internal class FileSystem
		{
			#region Data Members

			private static Hashtable includedFiles = new Hashtable();
			private static Hashtable excludedPath  = new Hashtable();
			private static string	 serverPath	   = string.Empty;

			#endregion 

			#region Properties

			public static Hashtable IncludedFiles { get { if (includedFiles.Count == 0) Init (); return includedFiles; } }
			public static Hashtable ExcludedPath  { get { if (excludedPath.Count == 0) Init (); return excludedPath; } }
			public static string	ServerPath	  { get { return serverPath; } }

			#endregion 

			#region Strings

			public static string DesignToolsNet		= "design tools .net";
			public static string DeveloperRefGuide	= "developerrefguide";
			public static string Tools				= "tools";
			public static string Library			= "library";
			public static string Obj				= "obj";
			public static string Res				= "res";
			public static string DbInfo				= "dbinfo";

			#endregion

			#region construction and initialization

			//-----------------------------------------------------------------------
			public FileSystem ()
			{
			}

			//-----------------------------------------------------------------------
			public static void Init ()
			{
				// files
				includedFiles.Add (".xml",		"");
				includedFiles.Add (".config",	"");
				includedFiles.Add (".menu",		"");
				includedFiles.Add (".txt",		"");
				includedFiles.Add (".ini",		"");
				includedFiles.Add (".bmp", 		"");
				includedFiles.Add (".gif", 		"");
				includedFiles.Add (".jpg", 		"");
				includedFiles.Add (".wrm", 		"");
				includedFiles.Add (".exe", 		"");
				includedFiles.Add (".dll", 		"");
				includedFiles.Add (".ocx", 		"");
				includedFiles.Add (".drv", 		"");
				includedFiles.Add (".xls", 		"");
				includedFiles.Add (".xlt", 		"");
				includedFiles.Add (".doc", 		"");
				includedFiles.Add (".dot", 		"");
				includedFiles.Add (".tbf", 		"");
				includedFiles.Add (".rad", 		"");
				includedFiles.Add (".sql", 		"");

				// directories
				excludedPath.Add (DesignToolsNet,		"");
				excludedPath.Add (DeveloperRefGuide,	"");
				excludedPath.Add (Tools,				"");	
				excludedPath.Add (Library,				"");	
				excludedPath.Add (Obj,					"");	
				excludedPath.Add (Res,					"");	
				excludedPath.Add (DbInfo,				"");	
                //Lara
			//	excludedPath.Add (NameSolverStrings.ToolsNet.ToLower(), ""); nn c e la stringa fai add
       
				excludedPath.Add (NameSolverStrings.Licenses.ToLower(), "");
			}

			//-----------------------------------------------------------------------
			public static void InitServerPath (BasePathFinder pathFinder)
			{
                //Lara
				//serverPath = string.Concat (Path.DirectorySeparatorChar,  Path.DirectorySeparatorChar, 
				//			pathFinder.RemoteServer, Path.DirectorySeparatorChar + pathFinder.Installation + "_");
				
				serverPath = serverPath.ToLower ();
			}

			#endregion
		}

		//=========================================================================
		internal class Database
		{
			#region Data Members 

			internal enum TypeValues 
			{ 
				Empty, 
				Startup, 
				Scripts, 
				Default, 
				Sample, 
				Report, 
				Menu, 
				Migration, 
				ModuleObject, 
				ReferenceObject, 
				Settings, 
				Ini, 
				ExportProfile, 
				Word, 
				Excel, 
				Text, 
				Image, 
				Schema
			};

			#endregion

			/// <summary>
			/// static database strings
			/// </summary>
			//=========================================================================
			internal class Strings 
			{
				#region Data Members
			
				// names
				internal static string StandardTableName		= "MSD_Standard";
				internal static string CustomTableName			= "MSD_Custom";

				internal static string TypeColumnName			=  "Type";
				internal static string NamespaceColumnName		=  "Namespace";
				internal static string AdditionalKeyColumnName	=  "AdditionalKey";
				internal static string FileNameColumnName		=  "FileName";
				internal static string PathNameColumnName		=  "PathName";
				internal static string CultureColumnName		=  "Culture";
				internal static string LocalizationColumnName	=  "Localization";
				internal static string FileDataTextColumnName	=  "FileDataText";
				internal static string LoginIDColumnName		=  "LoginID";
				internal static string RoleIDColumnName			=  "RoleID";
				internal static string CompanyIDColumnName		=  "CompanyID";


				// queries
				internal static string DeleteAllQuery				= "DELETE FROM {0}";
				internal static string DeleteAFileQuery				= "DELETE FROM {0} WHERE FileName = @FileName AND PathName = @PathName ";

				// standard queries
				internal static string CustomInsertAFileQuery		= "INSERT INTO MSD_Custom ([Type], [Namespace], [AdditionalKey], [FileName], [PathName], [Culture], [Localization], [FileDataText]) VALUES (@Type, @Namespace, @AdditionalKey, @FileName, @PathName, @Culture, @Localization, @FileDataText)"; 
				internal static string CustomSelectAFileQuery		= "SELECT * FROM MSD_Custom WHERE FileName = @FileName AND PathName = @PathName"; 
				internal static string CustomUpdateAFileQuery		= "UPDATE MSD_Custom SET FileDataText = @FileDataText WHERE FileName = @FileName AND PathName = @PathName ";
				
				// custom queries
				internal static string StandardInsertAFileQuery		= "INSERT INTO MSD_Standard ([Type], [Namespace], [AdditionalKey], [FileName], [PathName], [Culture], [Localization], [FileDataText]) VALUES (@Type, @Namespace, @AdditionalKey, @FileName, @PathName, @Culture, @Localization, @FileDataText)"; 
				internal static string StandardSelectAFileQuery		= "SELECT * FROM MSD_Standard WHERE FileName = @FileName AND PathName = @PathName"; 
				internal static string StandardUpdateAFileQuery		= "UPDATE MSD_Standard SET FileDataText = @FileDataText WHERE FileName = @FileName AND PathName = @PathName ";

				// standard rename and update queries
				internal static string StandardDirectoryRenameQuery	= "UPDATE MSD_Standard SET PathName = '{0}' WHERE PathName LIKE '{1}%'" ;
				internal static string StandardDirectoryDeleteQuery	= "DELETE FROM MSD_Standard WHERE PathName LIKE '{0}%'";
				
				// custom rename and update queries
				internal static string CustomDirectoryRenameQuery	= "UPDATE MSD_Custom SET PathName = '{0}' WHERE PathName LIKE '{1}%'" ;
				internal static string CustomDirectoryDeleteQuery	= "DELETE FROM MSD_Custom WHERE PathName LIKE '{0}%'";

				// content
				internal static string ManagedExtensions = "*.xml;*.config;*.menu;*.txt;*.ini;*.wrm;*.tbf;*.rad";
				
				#endregion

				#region Construction and Destruction

				//-------------------------------------------------------------------------
				Strings ()
				{
				}

				#endregion
			}

			//=========================================================================
			internal class Parameters 
			{
				#region Data Members

				internal SqlParameter Type				= null;
				internal SqlParameter Namespace			= null;
				internal SqlParameter AdditionalKey		= null;
				internal SqlParameter FileName			= null;
				internal SqlParameter PathName			= null;
				internal SqlParameter Culture			= null;
				internal SqlParameter Localization		= null;
				internal SqlParameter FileDataText		= null;

				#endregion

				#region Construction and Destruction

				//-------------------------------------------------------------------------
				internal Parameters  ()
				{
				}

				#endregion
	
				#region Prepare Methods

				//-------------------------------------------------------------------------
				internal void PrepareAll (SqlCommand sqlCommand)
				{
					PrepareType			(sqlCommand);
					PrepareNamespace	(sqlCommand);
					PrepareAdditionalKey(sqlCommand);
					PrepareFileName		(sqlCommand);
					PreparePathName		(sqlCommand);
					PrepareCulture		(sqlCommand);
					PrepareLocalization	(sqlCommand);
					PrepareFileDataText	(sqlCommand);
				}

				//-------------------------------------------------------------------------
				internal void PrepareType (SqlCommand sqlCommand)
				{
					Type = sqlCommand.Parameters.Add("@" + Strings.TypeColumnName, SqlDbType.Int, 4);
				}

				//-------------------------------------------------------------------------
				internal void PrepareNamespace (SqlCommand sqlCommand)
				{
					Namespace = sqlCommand.Parameters.Add("@" + Strings.NamespaceColumnName, SqlDbType.VarChar, 200);
				}
			
				//-------------------------------------------------------------------------
				internal void PrepareAdditionalKey (SqlCommand sqlCommand)
				{
					AdditionalKey	= sqlCommand.Parameters.Add("@" + Strings.AdditionalKeyColumnName, SqlDbType.VarChar, 50);
				}

				//-------------------------------------------------------------------------
				internal void PrepareFileName (SqlCommand sqlCommand)
				{
					FileName = sqlCommand.Parameters.Add("@" + Strings.FileNameColumnName, SqlDbType.VarChar, 100);
				}

				//-------------------------------------------------------------------------
				internal void PreparePathName (SqlCommand sqlCommand)
				{
					PathName = sqlCommand.Parameters.Add("@" + Strings.PathNameColumnName, SqlDbType.VarChar, 255);
				}

				//-------------------------------------------------------------------------
				internal void PrepareCulture (SqlCommand sqlCommand)
				{
					Culture	= sqlCommand.Parameters.Add("@" + Strings.CultureColumnName, SqlDbType.VarChar, 10);
				}

				//-------------------------------------------------------------------------
				internal void PrepareLocalization (SqlCommand sqlCommand)
				{
					Localization = sqlCommand.Parameters.Add("@" + Strings.LocalizationColumnName, SqlDbType.VarChar, 10);
				}

				//-------------------------------------------------------------------------
				internal void PrepareFileDataText (SqlCommand sqlCommand)
				{
					FileDataText = sqlCommand.Parameters.Add("@" + Strings.FileDataTextColumnName, SqlDbType.Text, 200000);
				}

				#endregion
			}

			#region Construction and Destruction

			//-------------------------------------------------------------------------
			Database ()
			{
			}

			#endregion
		}
	}
	
	//=========================================================================
	internal class FileSystemMonitorStrings
	{
		private static ResourceManager rm = new ResourceManager (typeof(FileSystemMonitorStrings));	
			
		internal static string ConnectionNotOpened		{ get { return rm.GetString ("ConnectionNotOpened");	}}
		internal static string ConnectionNotClosed		{ get { return rm.GetString ("ConnectionNotClosed");	}}
		internal static string PathFinderStringEmpty	{ get { return rm.GetString ("PathFinderStringEmpty");	}}
		internal static string QueryNotExecuted			{ get { return rm.GetString ("QueryNotExecuted");		}}
		internal static string AuthenticationFailed		{ get { return rm.GetString ("AuthenticationFailed");	}}
		internal static string DirectorySearchFailed	{ get { return rm.GetString ("DirectorySearchFailed");	}}
		internal static string FileNotFound				{ get { return rm.GetString ("FileNotFound");			}}
		internal static string DirectoryNotFound		{ get { return rm.GetString ("DirectoryNotFound");		}}
		internal static string WriteAttributesFailed	{ get { return rm.GetString ("WriteAttributesFailed");	}}
		internal static string ReadAttributesFailed		{ get { return rm.GetString ("ReadAttributesFailed");	}}
		internal static string AttributesCheckFailed	{ get { return rm.GetString ("AttributesCheckFailed");	}}
		internal static string MonitoringStarted		{ get { return rm.GetString ("MonitoringStarted");		}}
		internal static string MonitoringStopped		{ get { return rm.GetString ("MonitoringStopped");		}}
		internal static string DeleteAttemptFailed		{ get { return rm.GetString ("DeleteAttemptFailed");	}}
		internal static string RenameAttemptFailed		{ get { return rm.GetString ("RenameAttemptFailed");	}}
		internal static string CopyAttemptFailed		{ get { return rm.GetString ("CopyAttemptFailed");		}}
		internal static string WrongTypeColumn			{ get { return rm.GetString ("WrongTypeColumn");		}}
		internal static string ColumnNotFound			{ get { return rm.GetString ("ColumnNotFound");			}}
		internal static string ErrorInDatabase			{ get { return rm.GetString ("ErrorInDatabase");		}}
		internal static string GenericDatabaseError		{ get { return rm.GetString ("GenericDatabaseError");	}}
		internal static string WriteTextError			{ get { return rm.GetString ("WriteTextError");	}}
		internal static string ReadTextError			{ get { return rm.GetString ("ReadTextError");	}}
	}
}
