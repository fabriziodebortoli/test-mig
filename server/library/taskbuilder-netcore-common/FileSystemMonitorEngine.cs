using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Resources;

using Microarea.Common.NameSolver;
using Microarea.Common.DiagnosticManager;
using TaskBuilderNetCore.Interfaces;



//////////////////////////////////////////////
///				TODOBRUNA
//////////////////////////////////////////////
/// Per ora il monitoring del FSdisabilitato
/// Le estensioni dei files da includere in cache sono cablate
/// Diagnostica:
///		- da sistemare
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
    public class FileSystemMonitor
	{
		#region Data Members
		private static FileSystemMonitorEngine engine = new FileSystemMonitorEngine();
		#endregion

		#region Properties
        public static FileSystemMonitorEngine Engine { get { return engine; } }
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
    public class FileSystemMonitorEngine
	{
		#region Data Members

		private const string cacheFileName = "FileSystemCache{0}.xml";

		// utility data members
	//	private LoginManager		loginManager		= null; TODO LARA
		private BasePathFinder		pathFinder			= null;
		private Diagnostic			diagnostic			= new Diagnostic("FileSystemMonitor"); 

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
			managedExtensions = Strings.ManagedExtensions.Split (';');

			pathFinder = BasePathFinder.BasePathFinderInstance;
            //LARA Dovrebbe farla gia dentro a .BasePathFinderInstance
            //pathFinder.Init ();
            FileSystem.InitServerPath (pathFinder);

            // TODO LARA
            //loginManager = new LoginManager (pathFinder.LoginManagerUrl, pathFinder.ServerConnectionInfo.WebServicesTimeOut);
            //watcher = new FileSystemWatcher (pathFinder.GetRunningPath()); 
            watcher = new FileSystemWatcher(pathFinder.GetStandardPath());

            watcher.Filter		 = "*.*";
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
    		watcher.IncludeSubdirectories = true;

            //Lara
			//// Add event handlers.
			//watcher.Created += new FileSystemEventHandler(OnFileCreated);
			//watcher.Changed += new FileSystemEventHandler(OnFileChanged);
			//watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
			//watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
		}

		//-----------------------------------------------------------------------
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

        #endregion

        #region Checking methods
        //-----------------------------------------------------------------------
        public bool IsValidToken (string authenticationToken)
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
        public bool StartMonitor ()
		{
			diagnostic.Set(DiagnosticType.LogInfo, FileSystemMonitorStrings.MonitoringStarted);
			watcher.EnableRaisingEvents = false;
			return true;
		}

        //-----------------------------------------------------------------------
        public bool StopMonitor ()
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
            //Lara
            // serviva solo x dare un nome univoco al file
            // string fileName = string.Format(cacheFileName, pathFinder.InstallationAbstract.LatestUpdate);

            string fileName = string.Empty;
            fileName.Replace(":", "-");
            fileName = pathFinder.GetCustomPath() + Path.DirectorySeparatorChar + fileName;

            return fileName;

        }

        //-------------------------------------------------------------------------
        public bool GetTbCacheFile (out string fileContent)
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
        public bool CreateTbCacheFile ()
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
		public bool WriteCacheFile (ref XmlNode currentNode, ref XmlDocument doc, string path, Hashtable pathExclusions, Hashtable fileInclusions)
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
        public bool GetServerConnectionConfig (out string fileContent)
		{
			return GetTextFile (pathFinder.ServerConnectionFile, out fileContent);
		}

        //-----------------------------------------------------------------------
        public bool GetTextFile (string theFileName, out string fileContent)
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
			catch (Exception)
			{
				return false;
			}
			return true;
		}

        //-----------------------------------------------------------------------
        public bool SetTextFile (string theFileName, string fileContent)
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
                //StreamWriter sw = new StreamWriter(File.OpenRead(fileName), false,System.Text.Encoding.UTF8);
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
        public bool ExistFile (string theFileName)
		{
			if (theFileName == string.Empty)
				return false;

			string fileName = GetAdjustedPath(theFileName); 
			
			return File.Exists(fileName);
		}

        //-----------------------------------------------------------------------
        public bool ExistPath (string thePathName)
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
        public bool GetPathContent (string thePathName, string fileExtension, out string returnDoc, bool folders, bool files)
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
			catch(Exception e)//SqlException e)
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
			catch(Exception)
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
            catch (Exception)
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
            catch (Exception)
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
        public bool RemoveFolder(string thePathName, bool recursive, bool emptyOnly)
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
            catch(Exception)
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
        public bool CreateFolder (string thePathName, bool recursive)
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
			catch(Exception)
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
        public bool CopyFolder(string theOldPathName, string theNewPathName, bool recursive)
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
            catch (Exception)
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
        public bool RemoveFile(string theFileName)
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
            catch (Exception)
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
        public bool CopyFile(string theOldFileName, string theNewFileName, bool overWrite)
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
			catch (Exception)
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
        public bool RenameFile(string theOldFileName, string theNewFileName)
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
            catch (Exception)
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
        public bool GetFileStatus (
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
            catch (Exception)
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
        public int GetFileAttributes (string theFileName)
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

        #endregion

        //=========================================================================
        public class FileSystem
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
          //      serverPath = string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar,
                   //           pathFinder.RemoteServer, Path.DirectorySeparatorChar + pathFinder.Installation + "_");
                serverPath = string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar,
                pathFinder.RemoteWebServer, Path.DirectorySeparatorChar + pathFinder.Installation + "_");

                serverPath = serverPath.ToLower ();
			}

			#endregion
		}

        ////=========================================================================
        //internal class Database
        //{
        //	#region Data Members 

        //	internal enum TypeValues 
        //	{ 
        //		Empty, 
        //		Startup, 
        //		Scripts, 
        //		Default, 
        //		Sample, 
        //		Report, 
        //		Menu, 
        //		Migration, 
        //		ModuleObject, 
        //		ReferenceObject, 
        //		Settings, 
        //		Ini, 
        //		ExportProfile, 
        //		Word, 
        //		Excel, 
        //		Text, 
        //		Image, 
        //		Schema
        //	};

        //	#endregion




        //}

        /// <summary>
        /// static database strings
        /// </summary>
        //=========================================================================
        internal class Strings
        {
            #region Data Members

            // content
            internal static string ManagedExtensions = "*.xml;*.config;*.menu;*.txt;*.ini;*.wrm;*.tbf;*.rad";

            #endregion

            #region Construction and Destruction

            //-------------------------------------------------------------------------
            Strings()
            {
            }

            #endregion
        }
    }

//=========================================================================
public class FileSystemMonitorStrings
	{
		private static ResourceManager rm = new ResourceManager (typeof(FileSystemMonitorStrings));	
		internal static string PathFinderStringEmpty	{ get { return rm.GetString ("PathFinderStringEmpty");	}}
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
		internal static string WriteTextError			{ get { return rm.GetString ("WriteTextError");	}}
		internal static string ReadTextError			{ get { return rm.GetString ("ReadTextError");	}}
	}
}
