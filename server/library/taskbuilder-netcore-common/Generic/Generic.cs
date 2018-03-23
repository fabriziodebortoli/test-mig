using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.MenuLoader;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Microarea.Common.Generic
{
    /// <summary>
    /// Classe per la lettura scrittura del file contenente le informazioni di connessione ad una installazione remota
    /// </summary>
    /// <remarks>Questa classe viene usata da path finder per leggere, in ultima istanza, le informazioni di connessione da file system (usato da EasyLook in modalit� remota)</remarks>
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

        // numero massimo di dimensione in KB per i database in Pro-Lite
        private const int MaxDBSize = 2097152; // equivalente a 2GB


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
                    if (PathFinder.PathFinderInstance.ExistFile(FilePath))
                        PathFinder.PathFinderInstance.RemoveFile(path);
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
                if (!PathFinder.PathFinderInstance.ExistFile(FilePath))
                    return new InstallationInfo("", "", "");

                XmlSerializer x = new XmlSerializer(typeof(InstallationInfo));
                using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Read))
                    return (InstallationInfo)x.Deserialize(fs);
            }
            catch
            {
                try
                {
                    if (PathFinder.PathFinderInstance.ExistFile(FilePath))
                        PathFinder.PathFinderInstance.RemoveFile(FilePath);
                }
                catch { }

                throw;
            }
        }

        //---------------------------------------------------------------------------
        public void Delete()
        {
            if (PathFinder.PathFinderInstance.ExistFile(FilePath))
                PathFinder.PathFinderInstance.RemoveFile(FilePath);
        }

        //--------------------------------------------------------------------------
        public static void TestInstallation()
        {
            //se sto girando all'interno dell'installazione non mi devo porre problemi di compatibilita`
            if (PathFinder.PathFinderInstance.IsRunningInsideInstallation)
                return;
            //se sono un client clickonce non mi devo porre problemi di compatibilita`
            if (InstallationData.IsClickOnceInstallation)
                return;
            string prodVer, serverVer;
            if (!SameVersion(out prodVer, out serverVer))
                throw new Exception(string.Format(GenericStrings.IncompatibleVersion, prodVer, serverVer));
        }

        //--------------------------------------------------------------------------------
        public static bool SameVersion(out string localVer, out string serverVer)
        {
            string path = PathFinder.PathFinderInstance.GetApplicationModulePath(NameSolverStrings.WebFramework, NameSolverStrings.LoginManager);
            if (!PathFinder.PathFinderInstance.ExistPath(path))
                throw new Exception(string.Format(GenericStrings.InvalidInstallation, PathFinder.PathFinderInstance.Installation, path));

            //confronto le versioni della dll corrente con quella del server 
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //path = Path.Combine(path, string.Format("bin\\{0}", Path.GetFileName(assembly.Location)));

            //AssemblyName serverInterfacesAssemblyName = AssemblyName.GetAssemblyName(path);
            //serverVer = serverInterfacesAssemblyName.Version.ToString();                               TODO rsweb
            //localVer = assembly.GetName().Version.ToString();
            //return serverVer == localVer; 
            //}  
            localVer = serverVer = "1";          //temp
            return true;


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
                Microarea.Common.StringLoader.StringLoader.ClearDictionaryCache();//TODO LARA
                MenuInfo.CachedMenuInfos.Delete(currentUser);
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
                Assembly asm = Assembly.GetEntryAssembly();//.GetCallingAssembly();            TODO rsweb
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
            /// <remarks>E' stata fatta perch� Directory.GetParent() richiede particolari permessi</remarks>
            //---------------------------------------------------------------------
            public static string GetParentDirectory(string path, int steps)
            {
                if (path == null || path.Length == 0)
                    return string.Empty;

                if (steps == 0)
                    return path;

                int i = path.Length;

                if (path[i - 1] == NameSolverStrings.Directoryseparetor)
                    i -= 1;

                while (path[i - 1] != NameSolverStrings.Directoryseparetor && i >= 0)
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
                if (dirPath[dirPath.Length - 1] == NameSolverStrings.Directoryseparetor)
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
            public static void ReadSubDirectoryList(string aHomeDir, out List<string> aSubDirs)
            {
                aSubDirs = new List<string>();

                // controllo prima che esista la directory
                if (!PathFinder.PathFinderInstance.ExistPath(aHomeDir))
                    return;

                List<TBDirectoryInfo> folders = new List<TBDirectoryInfo>();
                List<TBFile> files = new List<TBFile>();
                PathFinder.PathFinderInstance.GetPathContent(aHomeDir, true, out folders, false, string.Empty, out files);
                for(int i = 0; i < folders.Count; i ++)// OK
                {
                    TBDirectoryInfo dir = ((TBDirectoryInfo)folders[i]);
                    aSubDirs.Add(dir.name);
                }
                    

            }

            /// <summary>
            /// Copia un file se la data di creazione � differente. La destinazione deve mantiene i flag di stato della origine.
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
            /// Copia un file se la data di creazione � differente. La destinazione deve mantiene i flag di stato della origine.
            /// </summary>
            /// <param name="sourceFile">file origine</param>
            /// <param name="destinationFile">file destinazione</param>
            /// <param name="error">Se c'� stato un errore qui viene indicato.</param>
            /// <param name="modifiedFiles">1 se � stato modificato il file destinazione 0 se non � cambiato.</param>
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

                if (!PathFinder.PathFinderInstance.ExistFile(sourceFile))
                {
                    error = sourceFile; 
                    return false;
                }

                FileInfo sourceFileInfo = new FileInfo(sourceFile);
                FileInfo destinationFileInfo = new FileInfo(destinationFile);

                try 
                {
                    if (!PathFinder.PathFinderInstance.ExistPath(destinationFileInfo.Directory.FullName))
                        PathFinder.PathFinderInstance.CreateFolder(destinationFileInfo.Directory.FullName, false);

                    if (destinationFileInfo.LastWriteTimeUtc.ToString("s") != sourceFileInfo.LastWriteTimeUtc.ToString("s"))
                    {
                        if (PathFinder.PathFinderInstance.ExistFile(destinationFile)&& (destinationFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            destinationFileInfo.Attributes -= FileAttributes.ReadOnly;

                        PathFinder.PathFinderInstance.CopyFile(sourceFileInfo.FullName, destinationFile, true);

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
            public static void SetNoSecurityOnMutex(Mutex mutex)
            {
                //Create the security descriptor.
                ExternalAPI.SECURITY_DESCRIPTOR securityDescriptor = new ExternalAPI.SECURITY_DESCRIPTOR();
                //The only thing required is to set the revision to one and the DACL to IntPtr.Zero
                securityDescriptor.revision = 1;
                securityDescriptor.dacl = IntPtr.Zero;
                //Apply the DACL to the mutex
                if (!ExternalAPI.SetKernelObjectSecurity((int)mutex.GetSafeWaitHandle().DangerousGetHandle(), ExternalAPI.DACL_SECURITY_INFORMATION, ref securityDescriptor))
                {
                    Debug.Fail("Unable to set security on the mutex, error " + Marshal.GetLastWin32Error());
                }
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
                                    NameSolverStrings.Directoryseparetor,
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


            //-------------------------------------------------------------------------
            /*TODO RSWEB
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
    */
      

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
                    NameSolverStrings.Directoryseparetor);
                string[] toDirectories = toPath.Split(
                    NameSolverStrings.Directoryseparetor);
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
                    NameSolverStrings.Directoryseparetor.ToString(),
                    relativeParts);
                return newPath;
            }
            //--------------------------------------------------------------------------------------------------------------------------------
            /*
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
            */
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
                    fsAccessoFile.Dispose();
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
                string b64str = Convert.ToBase64String(encrypted, 0, encrypted.Length);
                b64str = b64str.Replace("+", "!");
                b64str = b64str.Replace("/", "$");
                return b64str;
            }

            //----------------------------------------------------------------------
            public static float GetDBPercentageUsedSize(string connectionStr)
            {
                long maxDBSize = GetDBSizeInKByte(connectionStr);

                return ((maxDBSize * 100) / MaxDBSize);
            }


            //----------------------------------------------------------------------
            /// <summary>
            /// Verifica se la dimensione del db � vicina al massimo indicato nel serverconnection.config
            /// se la size � compresa (estremi inclusi) tra la 'limit' (es: 1.95GB) e la max (2GB),  o se � minore di 0, per me � warning
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

                string path = PathFinder.PathFinderInstance.GetStandardDataManagerPath(NameSolverStrings.Extensions, NameSolverStrings.TbMailer);
                path = Path.Combine(path, NameSolverStrings.DataFile, defaultIso, "State.xml");

                if (!PathFinder.PathFinderInstance.ExistFile(path))
                    return defaultCountry;

                try
                {

                    XmlDocument xDoc = null;
                    xDoc = PathFinder.PathFinderInstance.LoadXmlDocument(xDoc, path);

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

                //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                //{
                //    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);                     TODO rsweb
                //}
            }
        }
    }
}
