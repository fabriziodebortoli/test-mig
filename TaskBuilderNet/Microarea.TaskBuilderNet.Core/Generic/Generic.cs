using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microsoft.Win32;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Classe per la lettura scrittura del file contenente le informazioni di connessione ad una installazione remota
	/// </summary>
	/// <remarks>Questa classe viene usata da path finder per leggere, in ultima istanza, le informazioni di connessione da file system (usato da EasyLook in modalità remota)</remarks>
	//=========================================================================
	[Serializable]
	public class InstallationInfo
	{
		public const string FileName = "InstallationInfo.xml";

		/// <summary>
		/// Server dell'installazione che espone le cartelle condivise
		/// </summary>
		public string FileSystemServer;
		/// <summary>
		/// Server dell'installazione che espone le cartelle virtuali di IIS
		/// </summary>
		public string WebServer;
		/// <summary>
		/// Nome dell'installazione
		/// </summary>
		public string InstallationName;
		/// <summary>
		/// Il percorso del file xml in cui viene serializzata questa classe
		/// </summary>
		public static string FilePath;

		/// <summary>
		/// Verifica se esiste il corrispondente file su file system
		/// </summary>
		public static bool Exists;
		//---------------------------------------------------------------------------
		static InstallationInfo()
		{
			InitPathInfo();
		}

		//---------------------------------------------------------------------------
		private static void InitPathInfo()
		{
            string path = Functions.GetExecutingAssemblyFolderPath();
            FilePath = Path.Combine(path, FileName);
            Exists = File.Exists(FilePath);
        }

		//---------------------------------------------------------------------------
		public InstallationInfo()
		{
			WebServer = FileSystemServer = InstallationName = "";
			InitPathInfo();
		}

		//---------------------------------------------------------------------------
		public InstallationInfo(string webServer, string fileSystemServer, string installationName)
		{
			this.WebServer = webServer;
			this.FileSystemServer = fileSystemServer;
			this.InstallationName = installationName;
			InitPathInfo();
		}

		//---------------------------------------------------------------------------
		/// <summary>
		/// Salva la struttura su file system
		/// </summary>
		public void Save(string path)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(GetType());
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					x.Serialize(fs, this);

				Exists = true;
                FilePath = path;
			}
			catch
			{
				try
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				catch { }

				throw;
			}
		}

		//---------------------------------------------------------------------------
		/// <summary>
		/// Salva la struttura su file system
		/// </summary>
		public void Save()
		{
			Save(FilePath);
		}

		//---------------------------------------------------------------------------
		public void SaveToFolder(string tmpFolder)
		{
			Save(Path.Combine(tmpFolder, FileName));
		}

		/// <summary>
		/// Carica la struttura in memoria da file system
		/// </summary>
		//---------------------------------------------------------------------------
		public static InstallationInfo Load()
		{
            try
            {
                if (!File.Exists(FilePath))
                    return new InstallationInfo("", "", "");

                XmlSerializer x = new XmlSerializer(typeof(InstallationInfo));
                using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Read))
                    return (InstallationInfo)x.Deserialize(fs);
            }
            catch
            {
                try
                {
                    if (File.Exists(FilePath))
                        File.Delete(FilePath);
                }
                catch { }

                throw;
            }
        }

		//---------------------------------------------------------------------------
		public void Delete()
		{
			if (File.Exists(FilePath))
				File.Delete(FilePath);
		}

		//--------------------------------------------------------------------------
		public static void TestInstallation()
		{
			//se sto girando all'interno dell'installazione non mi devo porre problemi di compatibilita`
			if (BasePathFinder.BasePathFinderInstance.IsRunningInsideInstallation)
				return;
			//se sono un client clickonce non mi devo porre problemi di compatibilita`
			if (InstallationData.IsClickOnceInstallation)
				return;
			string prodVer, serverVer;
			if (!SameVersion(out prodVer, out serverVer))
				throw new ApplicationException(string.Format(GenericStrings.IncompatibleVersion, prodVer, serverVer));
		}

		//--------------------------------------------------------------------------------
		public static bool SameVersion(out string localVer, out string serverVer)
		{
			string path = BasePathFinder.BasePathFinderInstance.GetApplicationModulePath(NameSolverStrings.WebFramework, NameSolverStrings.LoginManager);
			if (!Directory.Exists(path))
				throw new ApplicationException(string.Format(GenericStrings.InvalidInstallation, BasePathFinder.BasePathFinderInstance.Installation, path));

			//confronto le versioni della dll corrente con quella del server 
			Assembly assembly = Assembly.GetExecutingAssembly();
			path = Path.Combine(path, string.Format("bin\\{0}", Path.GetFileName(assembly.Location)));

			AssemblyName serverInterfacesAssemblyName = AssemblyName.GetAssemblyName(path);
			serverVer = serverInterfacesAssemblyName.Version.ToString();
			localVer = assembly.GetName().Version.ToString();
			return serverVer == localVer;
		}

	}

	/// <summary>
	/// Contiene funzioni generiche relative al file system.
	/// </summary>
	//=========================================================================
	public class Functions
	{
        /// <summary>
        /// clear cached data
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public static void ClearCachedData(string currentUser)
        {
            Microarea.TaskBuilderNet.Core.StringLoader.StringLoader.ClearDictionaryCache();
            MenuInfo.CachedMenuInfos.Delete(currentUser);
            ClearThumbnails();
           
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        private static void ClearThumbnails()
        {
            try
            {
                string file = BasePathFinder.BasePathFinderInstance.GetMenuThumbnailsFolderPath(false);
                DirectoryInfo di = new DirectoryInfo(file);
                if (di != null && di.Exists)
                    di.Delete(true);
            }
            catch (Exception)
            {
            }
        }
         
		/// <summary>
		/// DevelopmentIstance
		/// Legge la variabile di ambiente MicroareaVersion
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetDevelopmentIstance()
		{
			string MicroareaVersion = string.Empty;
			MicroareaVersion = Environment.GetEnvironmentVariable("MicroareaVersion");

			return MicroareaVersion;
		}

		//---------------------------------------------------------------------
		public static bool IsDebug()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		//---------------------------------------------------------------------
		public static string DebugOrRelease()
		{
			return IsDebug() ? NameSolverStrings.Debug : NameSolverStrings.Release;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Returns the folder which contains the assembly that is calling this method
		/// </summary>
		/// <returns></returns>
		public static string GetExecutingAssemblyFolderPath()
		{
			Assembly asm = Assembly.GetCallingAssembly();
			return GetAssemblyPath(asm);
		}

		//---------------------------------------------------------------------
		public static string GetAssemblyPath(Assembly asm)
		{
			string codeBase = asm.CodeBase;
			UriBuilder uriBuilder = new UriBuilder(codeBase);
			string filePath = Uri.UnescapeDataString(uriBuilder.Path);
			return Path.GetDirectoryName(filePath); //converte da url a path di file system
		}
		/// <summary>
		/// Restituisce il path parent di una cartella
		/// </summary>
		/// <param name="path">path</param>
		/// <param name="steps">Numero di livelli di parentela</param>
		/// <returns>path del parent</returns>
		//---------------------------------------------------------------------
		public static string GetDirectoryAncestor(string path, int steps)
		{
			if (steps == 0)
				return path;

			try
			{
				return GetDirectoryAncestor(Directory.GetParent(path).FullName, --steps);
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return string.Empty;
			}
		}

		/// <summary>
		/// Restituisce la parent ennesima di una cartella
		/// </summary>
		/// <param name="path">la path di partenza</param>
		/// <param name="steps">il numero di cartelle padre</param>
		/// <returns>la path risultante</returns>
		/// <remarks>E' stata fatta perchè Directory.GetParent() richiede particolari permessi</remarks>
		//---------------------------------------------------------------------
		public static string GetParentDirectory(string path, int steps)
		{
			if (path == null || path.Length == 0)
				return string.Empty;

			if (steps == 0)
				return path;

			int i = path.Length;

			if (path[i - 1] == Path.DirectorySeparatorChar)
				i -= 1;

			while (path[i - 1] != Path.DirectorySeparatorChar && i >= 0)
				i--;

			return GetParentDirectory(path.Substring(0, path.Length - (path.Length - i) - 1), steps - 1);
		}

		/// <summary>
		/// Restituisce il path parent di una cartella
		/// </summary>
		/// <param name="directoryInfo">Direnctory info della cartella</param>
		/// <param name="steps">Numero di livelli di parentela</param>
		/// <returns>path del parent</returns>
		//---------------------------------------------------------------------
		public static DirectoryInfo GetDirectoryAncestor(DirectoryInfo directoryInfo, int steps)
		{
			if (steps == 0)
				return directoryInfo;
			return GetDirectoryAncestor(directoryInfo.Parent, --steps);
		}

		/// <summary>
		/// Dato un path di directory ricava il nome della directory
		/// </summary>
		/// <param name="path">path della directory</param>
		/// <returns>nome della directory</returns>
		//---------------------------------------------------------------------
		public static string GetDirectoryName(string dirPath)
		{
			if (dirPath == null || dirPath == string.Empty)
			{
				Debug.Fail(string.Format("Error in Functions.GetDirectoryName. Invalid path {0}", dirPath));
				return string.Empty;
			}
			if (dirPath[dirPath.Length - 1] == Path.DirectorySeparatorChar)
				return Path.GetDirectoryName(dirPath);
			else
				return Path.GetFileName(dirPath);
		}

		/// <summary>
		/// Carica in un'array di stringhe l'elenco delle sottodirectory da caricare
		/// </summary>
		/// <param name="aHomeDir">Path di partenza</param>
		/// <param name="aSubDirs">Elenco delle sub dir</param>
		//---------------------------------------------------------------------
		public static void ReadSubDirectoryList(string aHomeDir, out ArrayList aSubDirs)
		{
			aSubDirs = new ArrayList();

			// controllo prima che esista la directory
			if (!Directory.Exists(aHomeDir))
				return;

			DirectoryInfo di = new DirectoryInfo(aHomeDir);
			foreach (DirectoryInfo sd in di.GetDirectories())
				aSubDirs.Add(sd.Name);
		}

		/// <summary>
		/// Copia un file se la data di creazione è differente. La destinazione deve mantiene i flag di stato della origine.
		/// </summary>
		/// <param name="sourceFile">path del file origine</param>
		/// <param name="destinationFile">path del file destinazione</param>
		/// <returns>true se la copia ha vuto successo</returns>
		//---------------------------------------------------------------------
		public static bool CopyDifferentFile(string sourceFile, string destinationFile, ref int modifiedFiles)
		{
			string error;
			return CopyDifferentFile(sourceFile, destinationFile, out error, ref modifiedFiles);
		}

		/// <summary>
		/// Copia un file se la data di creazione è differente. La destinazione deve mantiene i flag di stato della origine.
		/// </summary>
		/// <param name="sourceFile">file origine</param>
		/// <param name="destinationFile">file destinazione</param>
		/// <param name="error">Se c'è stato un errore qui viene indicato.</param>
		/// <param name="modifiedFiles">1 se è stato modificato il file destinazione 0 se non è cambiato.</param>
		/// <returns>true se la copia ha vuto successo</returns>
		//---------------------------------------------------------------------
		public static bool CopyDifferentFile(string sourceFile, string destinationFile, out string error, ref int modifiedFiles)
		{
			error = string.Empty;

			if (sourceFile == null || sourceFile.Length == 0 || destinationFile == null || destinationFile.Length == 0)
			{
				Debug.Fail("Error in Functions.CopyDifferentFile");
				return false;
			}

			if (!System.IO.File.Exists(sourceFile))
			{
				error = sourceFile;
				return false;
			}

			FileInfo sourceFileInfo = new FileInfo(sourceFile);
			FileInfo destinationFileInfo = new FileInfo(destinationFile);

			try
			{
				if (!Directory.Exists(destinationFileInfo.Directory.FullName))
					Directory.CreateDirectory(destinationFileInfo.Directory.FullName);

				if (destinationFileInfo.LastWriteTimeUtc.ToString("s") != sourceFileInfo.LastWriteTimeUtc.ToString("s"))
				{

					if (System.IO.File.Exists(destinationFile) && (destinationFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
						destinationFileInfo.Attributes -= FileAttributes.ReadOnly;

					System.IO.File.Copy(sourceFileInfo.FullName, destinationFile, true);

					modifiedFiles++;
				}
			}
			catch (IOException exc)
			{
				Debug.Fail(exc.Message);
				error = exc.Message;
				return false;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				error = e.Message;
				return false;
			}

			return true;
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		public static  void SetNoSecurityOnMutex(Mutex mutex)
		{
			//Create the security descriptor.
			ExternalAPI.SECURITY_DESCRIPTOR securityDescriptor = new ExternalAPI.SECURITY_DESCRIPTOR();
			//The only thing required is to set the revision to one and the DACL to IntPtr.Zero
			securityDescriptor.revision = 1;
			securityDescriptor.dacl = IntPtr.Zero;
			//Apply the DACL to the mutex
			if (!ExternalAPI.SetKernelObjectSecurity((int)mutex.SafeWaitHandle.DangerousGetHandle(), ExternalAPI.DACL_SECURITY_INFORMATION, ref securityDescriptor))
			{
				Debug.Fail("Unable to set security on the mutex, error " + Marshal.GetLastWin32Error());
			}
		}
		//------------------------------------------------------------------------------
		public static void CopyDirectory(string source, string destination, bool recursive)
		{
			CopyDirectory(source, destination, recursive, true);
		}

		//------------------------------------------------------------------------------
		public static void CopyDirectory(string source, string destination, bool recursive, bool overwrite)
		{
			if (!Directory.Exists(source))
				throw new ArgumentException();

			if (destination[destination.Length - 1] != Path.DirectorySeparatorChar)
				destination += Path.DirectorySeparatorChar;

			if (!Directory.Exists(destination))
				Directory.CreateDirectory(destination);

			string[] files = Directory.GetFiles(source);
			foreach (string element in files)
			{
				string destinationFile = destination + Path.GetFileName(element);
				if (!overwrite && System.IO.File.Exists(destinationFile))
					continue;
				System.IO.File.Copy(element, destinationFile, overwrite);
			}

			if (!recursive)
				return;

			string[] directories = Directory.GetDirectories(source);
			foreach (string element in directories)
				CopyDirectory(element, destination + Path.GetFileName(element), recursive);
		}

		/// <summary>
		/// Copia files da una dir ad un'altra
		/// </summary>
		/// <param name="sourcePath">Cartella origine</param>
		/// <param name="destinationPath">Cartella destinazione</param>
		/// <param name="search">Filtro di copia es. *.dll</param>
		/// <param name="recursive">true se si vogliono copiare anche le sottocartelle</param>
		/// <param name="modifiedFiles">numero di file copiati</param>
		/// <param name="errorFile">file che non si sono copiati</param>
		/// <returns>true se ha successo</returns>
		//---------------------------------------------------------------------
		public static bool CopyFiles(string sourcePath, string destinationPath, string searchPattern, bool recursive, ref int modifiedFiles, out string errorFile)
		{
			errorFile = string.Empty;

			if (sourcePath == null || sourcePath == string.Empty || destinationPath == null || destinationPath == string.Empty)
				return true;

			if (searchPattern == null || searchPattern == string.Empty)
				searchPattern = "*.*";

			DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);

			if (!sourceDirectory.Exists)
				return true;

			FileInfo[] files = sourceDirectory.GetFiles(searchPattern);
			foreach (FileInfo file in files)
			{
				if (!CopyDifferentFile(file.FullName, destinationPath + Path.DirectorySeparatorChar + file.Name, ref modifiedFiles))
				{
					errorFile = file.FullName;
					return false;
				}
			}

			if (recursive)
			{
				DirectoryInfo[] subFolders = sourceDirectory.GetDirectories();
				foreach (DirectoryInfo subFolder in subFolders)
				{
					if (!CopyFiles(subFolder.FullName, destinationPath + Path.DirectorySeparatorChar + subFolder.Name, searchPattern, recursive, ref modifiedFiles, out errorFile))
					{
						errorFile = subFolder.FullName;
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Salva il contenuto di uno Stream in un file.
		/// Il file viene creato, se esiste viene cancellato e ricreato.
		/// </summary>
		/// <param name="aStream">Lo Stream da salvare su file.</param>
		/// <param name="fileFullName">Il nome completo del file da salvare.</param>
		/// <param name="lastWriteTimeUtc">La data di ultima modifica da impostare sul file.</param>
		/// <returns>true se riesce a salvare lo stream in un file, false altrimenti.</returns>
		/// <remarks>
		/// Il file viene creato ex novo, se dovesse servire un metodo simile
		/// per l'inserimento in coda fare un overload.
		/// </remarks>
		//---------------------------------------------------------------------
		public static bool CopyToFile(Stream aStream, string fileFullName, DateTime lastWriteTimeUtc, out string error)
		{
			bool ok = CopyToFile(aStream, fileFullName, out error);

			if (ok && error == string.Empty)
				System.IO.File.SetLastWriteTimeUtc(fileFullName, lastWriteTimeUtc);

			return ok;
		}

		/// <summary>
		/// Salva il contenuto di uno Stream in un file.
		/// Il file viene creato, se esiste viene cancellato e ricreato.
		/// </summary>
		/// <param name="aStream">Lo Stream da salvare su file.</param>
		/// <param name="fileFullName">Il nome completo del file da salvare.</param>
		/// <returns>true se riesce a salvare lo stream in un file, false altrimenti.</returns>
		/// <remarks>
		/// Il file viene creato ex novo, se dovesse servire un metodo simile
		/// per l'inserimento in coda fare un overload.
		/// </remarks>
		//---------------------------------------------------------------------
		public static bool CopyToFile(Stream aStream, string fileFullName, out string error)
		{
			error = string.Empty;

			try
			{
				string dir = Path.GetDirectoryName(fileFullName);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				if (System.IO.File.Exists(fileFullName))
					System.IO.File.SetAttributes(fileFullName, FileAttributes.Normal);

				FileStream streamWriter;
				streamWriter = System.IO.File.Create(fileFullName);

				int size = 2048;
				byte[] buffer = new byte[2048];

				while (true)
				{
					size = aStream.Read(buffer, 0, buffer.Length);

					if (size > 0)
						streamWriter.Write(buffer, 0, size);
					else
						break;
				}

				streamWriter.Close();

				return true;
			}
			catch (Exception exc)
			{
				error = exc.Message;
				return false;
			}
		}

		/// <summary>
		/// Cancella tutti i file di un certo tipo ricorsivamente.
		/// </summary>
		/// <param name="rootPath">Cartella in cui cercare i file da cancellare.</param>
		/// <param name="fileMask">Tipo di file da cancellare.</param>
		/// <param name="recursive">true per cancellare ricorsivamente nelle sottocartelle, false altrimenti</param>
		/// <returns>true se ha successo</returns>
		//---------------------------------------------------------------------
		public static bool DeleteFiles(string rootPath, string fileMask, bool recursive)
		{
			DirectoryInfo sourceDirectory = new DirectoryInfo(rootPath);

			if (!sourceDirectory.Exists)
				return true;

			FileInfo[] files = sourceDirectory.GetFiles(fileMask);
			foreach (FileInfo file in files)
				DeleteFile(file);

			if (recursive)
			{
				DirectoryInfo[] subFolders = sourceDirectory.GetDirectories();
				foreach (DirectoryInfo subFolder in subFolders)
				{
					DeleteFiles(subFolder.FullName, fileMask, recursive);
					if (subFolder.GetDirectories().Length == 0 && subFolder.GetFiles().Length == 0)
						subFolder.Delete();
				}
			}

			return true;
		}

		/// <summary>
		/// Cancella i tipi di file elencati ricorsivamente.
		/// </summary>
		/// <param name="rootPath">Cartella in cui cercare i file da cancellare.</param>
		/// <param name="searchPatterns">Tipi di file da cancellare.</param>
		/// <param name="recursive">true per cancellare ricorsivamente nelle sottocartelle, false altrimenti</param>
		/// <returns>true se ha successo</returns>
		//---------------------------------------------------------------------
		public static bool DeleteFiles(string rootPath, string[] searchPatterns, bool recursive)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(rootPath);

			if (!dirInfo.Exists)
				return true;

			foreach (string searchPattern in searchPatterns)
			{
				FileInfo[] files = dirInfo.GetFiles(searchPattern);

				foreach (FileInfo file in files)
					DeleteFile(file);
			}

			if (recursive)
			{
				DirectoryInfo[] subFolders = dirInfo.GetDirectories();

				foreach (DirectoryInfo subFolder in subFolders)
				{
					DeleteFiles(subFolder.FullName, searchPatterns, recursive);

					if (subFolder.GetDirectories().Length == 0 && subFolder.GetFiles().Length == 0)
						subFolder.Delete();
				}
			}

			return true;
		}

		/// <summary>
		/// Elimina un file
		/// </summary>
		/// <param name="file">Il file da cancellare</param>
		//---------------------------------------------------------------------
		public static void DeleteFile(FileInfo file)
		{
			if (!file.Exists)
				return;

			// si assicura che non sia read-only per cancellarlo
			file.Attributes = ~FileAttributes.ReadOnly;
			file.Delete();
		}


		/// <summary>
		/// funzione che controlla che la stringa passata come parametro non contenga
		/// caratteri invalidi (intesa come nome di file o folder, non intero path)
		/// </summary>
		/// <param name="name">stringa da controllare</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static bool IsValidName(string name)
		{
			// prima controllo i caratteri non validi per il path (", <, >, |)
			if (name.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
				return false;

			// poi controllo gli altri caratteri speciali (/, \, ;, :, ?, *)
			char[] charChars = new char[] 
								{	
									Path.AltDirectorySeparatorChar ,
									Path.DirectorySeparatorChar,
									Path.PathSeparator,
									Path.VolumeSeparatorChar,
									'?',
									'*'
								};

			if (name.IndexOfAny(charChars) > -1)
				return false;

			return true;
		}

		//=====================================================================

		public static bool IsUrlReachable(string url)
		{
			if (url == string.Empty)
				return false;

			try
			{
				HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
				//myRequest.MaximumAutomaticRedirections = 0;
				myRequest.AllowAutoRedirect = false;
				// Return the response. 
				WebResponse myResponse = myRequest.GetResponse();

				// Close the response to free resources.
				myResponse.Close();
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return false;
			}

			return true;
		}

		//-------------------------------------------------------------------------
		public static void ShowHelp(Control parent, string helpUrl, string searchParameter, string loginMngASMX)
		{
			if (parent == null || !parent.IsHandleCreated || Control.FromHandle(parent.Handle) == null)
			{
				Debug.Fail("ShowHelp failed: invalid parent specification.");
				return;
			}

			if (helpUrl == null || helpUrl.Length == 0)
			{
				Debug.Fail("Invalid Help request: missing Help URL specification.");
				return;
			}

			if (!System.IO.File.Exists(helpUrl))
			{
				Debug.Fail(String.Format("Invalid Help request: cannot find Help file ({0}).", helpUrl));
				return;
			}

			// If the keyword is a null reference the table of contents for the Help file will be displayed.
			string keyWord = (searchParameter != null && searchParameter.Length > 0) ? string.Format("index.htm#search={0}&url={1}", searchParameter, loginMngASMX) : null;

			Help.ShowHelp(parent, helpUrl, keyWord);
		}

		//-------------------------------------------------------------------------
		public static ArrayList GetFiles(string directoryPath, string filter)
		{
			ArrayList files = new ArrayList();
			if (directoryPath == null || !Directory.Exists(directoryPath))
				return files;
			string[] f = Directory.GetFiles(directoryPath, filter);
			if (f != null && f.Length > 0)
				files.AddRange(f);
			string[] dirs = Directory.GetDirectories(directoryPath);
			if (dirs != null && dirs.Length > 0)
			{
				foreach (string dir in dirs)
					files.AddRange(GetFiles(dir, filter));
			}
			return files;
		}

		//-------------------------------------------------------------------------
		public static void GetFileDate(string filePath, out DateTime lastWriteTimeUtc, out DateTime creationTimeUtc, out DateTime accessTimeUtc)
		{
			lastWriteTimeUtc = DateTime.Now;
			creationTimeUtc = DateTime.Now;
			accessTimeUtc = DateTime.Now;

			try
			{
				FileInfo fi = null;
				if (System.IO.File.Exists(filePath))
				{
					fi = new FileInfo(filePath);
					lastWriteTimeUtc = fi.LastWriteTimeUtc;
					creationTimeUtc = fi.CreationTimeUtc;
					accessTimeUtc = fi.LastAccessTimeUtc;
				}
			}
			catch { }
		}

		//-------------------------------------------------------------------------
		public static void SetFileDate(string filePath, DateTime lastWriteTimeUtc, DateTime creationTimeUtc, DateTime accessTimeUtc)
		{
			try
			{
				FileInfo fi = null;
				if (System.IO.File.Exists(filePath))
				{
					fi = new FileInfo(filePath);
					fi.LastWriteTimeUtc = lastWriteTimeUtc;
					fi.CreationTimeUtc = creationTimeUtc;
					fi.LastAccessTimeUtc = accessTimeUtc;
				}
			}
			catch { }
		}

		/// <summary>
		/// Creates a relative path from one file
		/// or folder to another.
		/// </summary>
		/// <param name="fromDirectory">
		/// Contains the directory that defines the
		/// start of the relative path.
		/// </param>
		/// <param name="toPath">
		/// Contains the path that defines the
		/// endpoint of the relative path.
		/// </param>
		/// <returns>
		/// The relative path from the start
		/// directory to the end path.
		/// </returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string RelativePathTo(string fromDirectory, string toPath)
		{
			if (fromDirectory == null)
				throw new ArgumentNullException("fromDirectory");
			if (toPath == null)
				throw new ArgumentNullException("toPath");

			bool isRooted = Path.IsPathRooted(fromDirectory)
				&& Path.IsPathRooted(toPath);
			if (isRooted)
			{
				bool isDifferentRoot = string.Compare(
					Path.GetPathRoot(fromDirectory),
					Path.GetPathRoot(toPath), true) != 0;

				if (isDifferentRoot)
					return toPath;
			}

			StringCollection relativePath = new StringCollection();
			string[] fromDirectories = fromDirectory.Split(
				Path.DirectorySeparatorChar);
			string[] toDirectories = toPath.Split(
				Path.DirectorySeparatorChar);
			int length = Math.Min(
				fromDirectories.Length,
				toDirectories.Length);
			int lastCommonRoot = -1;
			// find common root
			for (int x = 0; x < length; x++)
			{
				if (string.Compare(fromDirectories[x],
					toDirectories[x], true) != 0)
					break;
				lastCommonRoot = x;
			}
			if (lastCommonRoot == -1)
				return toPath;
			// add relative folders in from path
			for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
				if (fromDirectories[x].Length > 0)
					relativePath.Add("..");
			// add to folders to path
			for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
				relativePath.Add(toDirectories[x]);
			// create relative path
			string[] relativeParts = new string[relativePath.Count];
			relativePath.CopyTo(relativeParts, 0);
			string newPath = string.Join(
				Path.DirectorySeparatorChar.ToString(),
				relativeParts);
			return newPath;
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// to avoid deadlocks, uses another thread and responds to events
		/// </summary>
		/// <returns></returns>
		public static void DoParallelProcedure(Action start)
		{
			Task t = new Task(start);
			t.Start();
			while (!t.Wait(10))
				Application.DoEvents(); //responds to messages
		}

		/// <summary>
		/// Starts a new Thread with "Lowest" priority
		/// </summary>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------------------------------------------------
		public static void LowPriorityThread(ThreadStart start)
		{
			Thread t = new Thread(start);
			t.Priority = ThreadPriority.Lowest;
			t.Start();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Enum Printers (see also PrinterSettings.InstalledPrinters)
		/// </summary>
		/// <returns></returns>
		public static bool EnumPrinters(String[] printers)
		{
			try
			{
				StringBuilder lpReturnedString = new StringBuilder(2024);
				uint ui = ExternalAPI.GetProfileString
					("devices", "", "", lpReturnedString, 2024);

				string s = lpReturnedString.ToString();

				printers = s.Split(new char[1] { '\0' });

			}
			catch (System.Exception ex)
			{
				printers = new string[1];
				printers[0] = ex.Message;
				return false;
			}
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static string ConvertToBase64(string sFileName)
		{
			using (FileStream fsAccessoFile = new FileStream(sFileName, FileMode.Open, FileAccess.Read))
			{
				byte[] binaryData = new Byte[fsAccessoFile.Length];
				long bytesRead = fsAccessoFile.Read(binaryData, 0, (int)fsAccessoFile.Length);
				fsAccessoFile.Close();
				string base64String = Convert.ToBase64String(binaryData, 0, binaryData.Length);
				return base64String;
			}
		}
        //--------------------------------------------------------------------------------------------------------------------------------
        public static string ConvertToBase64Str(string cmd)
        {
            byte[] result = new byte[cmd.Length];
            for (int i = 0; i < cmd.Length; i++)
                result[i] = (byte)cmd[i];
            string base64String = Convert.ToBase64String(result);
            return base64String;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static string ConvertToAESStr(string cmd)
        {
            byte[] encrypted;

            using (Aes myAes = Aes.Create())
            {

                myAes.Key = Encoding.ASCII.GetBytes("987MIC123XYZIMIC");
                myAes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                myAes.Mode = CipherMode.ECB;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(cmd);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            string b64str =  Convert.ToBase64String(encrypted, 0, encrypted.Length);
            b64str = b64str.Replace("+", "!");
            b64str = b64str.Replace("/", "$");
            return b64str;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static string GetEmbeddedBase64Image(string fileName)
		{
			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists)
				return string.Empty;

			string extension = fi.Extension.Replace(".", "");

			return string.Format("data:image/{0};base64,{1}", extension, ConvertToBase64(fileName));
		}

		private static Dictionary<string, string> isoToCountryDictionary = new Dictionary<string, string>();

		//--------------------------------------------------------------------------------------------------------------------------------
		public static string GetPostaliteCountryFromIsoCode(string isoCode)
		{
			string country;
			isoToCountryDictionary.TryGetValue(isoCode, out country);

			if (country != null)
				return country;

			const string defaultCountry = "Italia";
			const string defaultIso = "IT";
			const string xPathMainQuery = "//Auxdata/Elements/Element/Field[@name='Code' and text()= '{0}']";
			const string xPathSecondaryQuery = "Field[@name='Country']";

			string path = BasePathFinder.BasePathFinderInstance.GetStandardDataManagerPath(NameSolverStrings.Extensions, NameSolverStrings.TbMailer);
			path = Path.Combine(path, NameSolverStrings.DataFile, defaultIso, "State.xml");

			if (!File.Exists(path))
				return defaultCountry;

			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(path);

				//cerco nel file xml "state.xml" nel modulo tbmailer: cerco un nodo che abbio Code uguale
				//a quello desiderato
				XmlNode xNode = xDoc.SelectSingleNode(string.Format(xPathMainQuery, isoCode));
				if (xNode == null)
					return defaultCountry;

				//se lo trovo, cerco il nodo "fratello" di tipo Country
				XmlNode countryNode = xNode.ParentNode.SelectSingleNode(xPathSecondaryQuery);
				if (countryNode == null)
					return defaultCountry;

				isoToCountryDictionary.Add(isoCode, countryNode.InnerText);
				return countryNode.InnerText;
			}
			catch { }

			return defaultCountry;
		}

		///<summary>
		/// Dato il path di un file .crs passato come parametro, ne decripta il contenuto e ritorna la stringa in chiaro
		///</summary>
		///<remarks>Ad uso del RowSecurityLayer in fase di caricamento dell'estensione in C++</remarks>
		//--------------------------------------------------------------------------------------------------------------------------------
		public static string OpenCrsFile(string filePath)
		{
			string decryptedContent = string.Empty;

			if (string.IsNullOrWhiteSpace(filePath))
				return decryptedContent;

			if (Path.GetExtension(filePath).ToUpperInvariant() != NameSolverStrings.CrsExtension.ToUpperInvariant())
				return decryptedContent;

			try
			{
				byte[] content = File.ReadAllBytes(filePath);
				decryptedContent = Crypto.Decrypt(content);
			}
			catch
			{
				decryptedContent = string.Empty;
			}

			return decryptedContent;
		}

        //data function

        //todo duplicato da data 
        //----------------------------------------------------------------------
        public static float GetDBPercentageUsedSize(string connectionStr)
        {
            long maxDBSize = GetDBSizeInKByte(connectionStr);

            return ((maxDBSize * 100) / MaxDBSize);
        }

        // numero massimo di dimensione in KB per i database in Pro-Lite
        private const int MaxDBSize = 2097152; // equivalente a 2GB

        //----------------------------------------------------------------------
        /// <summary>
        /// Verifica se la dimensione del db è vicina al massimo indicato nel serverconnection.config
        /// se la size è compresa (estremi inclusi) tra la 'limit' (es: 1.95GB) e la max (2GB),  o se è minore di 0, per me è warning
        /// </summary>
        /// <param name="tbConnection"></param>
        /// <returns></returns>
        public static bool IsDBSizeNearMaxLimit(SqlConnection tbConnection, out string freePercentage)
        {
            long DBSize = GetDBSizeInKByte(tbConnection);
            double freePercentageD = Math.Round((((double)(MaxDBSize - DBSize) * 100) / MaxDBSize), 1);
         
           freePercentage = freePercentageD.ToString();
            return ((DBSize >= InstallationData.ServerConnectionInfo.MinDBSizeToWarn && DBSize <= MaxDBSize) || DBSize < 0);
        }

        //----------------------------------------------------------------------
        private static long GetDBSizeInKByte(string connectionStr)
        {
            long fullSize = 0;
            if (string.IsNullOrEmpty(connectionStr)) return 0;

            SqlConnection myConnection = new SqlConnection();

            try
            {
                myConnection.ConnectionString = connectionStr;
                myConnection.Open();
                fullSize = GetDBSizeInKByte(myConnection);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
                return 0;
            }
            finally
            {
                if (myConnection != null && myConnection.State != ConnectionState.Closed)
                {
                    myConnection.Close();
                    myConnection.Dispose();
                }
            }

            return fullSize;
        }

        /// <summary>
        /// IsDBSizeOverMaxLimit
        /// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
        /// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
        /// </summary>
        /// <param name="connectionStr">stringa di connessione al db</param>
        /// <param name="dbmsType">dbmstype per la connessione</param>
        /// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
        //----------------------------------------------------------------------
        public static bool IsDBSizeOverMaxLimit(string connectionStr)
        {
            long maxDBSize = GetDBSizeInKByte(connectionStr);

            if (maxDBSize > MaxDBSize || maxDBSize < 0)
                return true;

            return false;
        }

        /// <summary>
        /// IsDBSizeOverMaxLimit
        /// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
        /// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
        /// </summary>
        /// <param name="tbConnection">connessione aperta sul db</param>
        /// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
        //----------------------------------------------------------------------
        public static bool IsDBSizeOverMaxLimit(SqlConnection tbConnection)
        {
            long maxDBSize = GetDBSizeInKByte(tbConnection);

            if (maxDBSize > MaxDBSize || maxDBSize < 0)
                return true;

            return false;
        }

        //----------------------------------------------------------------------
        private static long GetDBSizeInKByte(SqlConnection connection)
        {
            long fullSize = 0;


            SqlCommand myCommand = new SqlCommand();

            IDataReader reader = null;

            try
            {
                myCommand.Connection = connection;
                myCommand.CommandText = "sp_helpfile";
                myCommand.CommandType = CommandType.StoredProcedure;
                reader = myCommand.ExecuteReader();

                while (reader.Read())
                {
                    object filegroup = reader["filegroup"];
                    if (filegroup == DBNull.Value)
                        continue;

                    string size = ((string)reader["size"]).Trim().Replace("KB", "");

                    if (string.Compare(size, "Unlimited", true, CultureInfo.InvariantCulture) == 0)
                        return -1;

                    fullSize += Int32.Parse(size);
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                return -1;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }

            return fullSize;
        }

    }

    //=========================================================================
    public class MemoryManagement
	{
		[DllImport("kernel32.dll")]
		public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

		//---------------------------------------------------------------------
		public static void Flush()
		{
			//GC.Collect();
			//GC.WaitForPendingFinalizers();

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
			}
		}
	}

	//=========================================================================
	public class RegisterKeyChecker
	{
        
        //---------------------------------------------------------------------
        public static bool IsOfficeInstalled()
        {
            return IsOffice2003Installed() || IsOffice2007Installed() || IsOffice2013Installed();
        }

        //---------------------------------------------------------------------
        public static RegistryKey GetRegistryKey(RegistryHive rHive, string keyPath, bool writable = false)
        {
            if (string.IsNullOrEmpty(keyPath))
                return null;

            RegistryKey rootRegistryKey = RegistryKey.OpenBaseKey
                (
                rHive,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32
                );			

			return rootRegistryKey.OpenSubKey(keyPath);
		}

		//---------------------------------------------------------------------
		public static bool IsOffice2003Installed()
		{
			try
			{
                RegistryKey RKOffice2003Installed = RegisterKeyChecker.GetRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Office\\11.0\\Common\\InstallRoot");
                if (RKOffice2003Installed == null)
					return false;

				string installPath = RKOffice2003Installed.GetValue("Path", "").ToString();
				RKOffice2003Installed.Close();

				return (!string.IsNullOrEmpty(installPath));
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static bool IsOffice2007Installed()
		{
			try
			{
                RegistryKey RKOffice2007Installed = RegisterKeyChecker.GetRegistryKey(RegistryHive.LocalMachine, "Software\\Microsoft\\Office\\12.0\\Common\\InstallRoot");
                if (RKOffice2007Installed == null)
					return false;

				string installPath = RKOffice2007Installed.GetValue("Path", "").ToString();
				RKOffice2007Installed.Close();

				return (!string.IsNullOrEmpty(installPath));
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static bool IsOffice2013Installed()
		{
			try
			{
				RegistryKey RKOffice2013Installed = RegisterKeyChecker.GetRegistryKey(RegistryHive.LocalMachine, "Software\\Microsoft\\Office\\15.0\\Common\\InstallRoot");
				if (RKOffice2013Installed == null)
					return false;

				string installPath = RKOffice2013Installed.GetValue("Path", "").ToString();
				RKOffice2013Installed.Close();

				return (!string.IsNullOrEmpty(installPath));
			}
			catch
			{
				return false;
			}
		}       
	}
}
