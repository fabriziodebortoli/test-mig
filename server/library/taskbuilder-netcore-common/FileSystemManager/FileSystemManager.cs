using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Microarea.Common.FileSystemManager
{
    //=========================================================================
    public class FileSystemManager : IFileSystemManager
    {
        private IFileSystemDriver fileSystemDriver = null;
        private IFileSystemDriver alternativeDriver = null;
        private bool fileSystemDriverOwner;
        private bool alternativeDriverOwner;
        private PathFinder pathFinder;
        private FileSystemManagerInfo configFile;

        //-----------------------------------------------------------------------------
        public IFileSystemDriver FileSystemDriver { get { return fileSystemDriver; }
            set {
                AttachFileSystemDriver(value, false);

                fileSystemDriverOwner = true;
            } }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver AlternativeDriver
        {
            get { return alternativeDriver; }
            set
            {
                AttachAlternativeDriver(value, false);
                alternativeDriverOwner = true;
            }
        }
        //-----------------------------------------------------------------------------
        public void Init(string sServer, string sInstallationName, String sMasterSolution)
        {
            configFile.LoadFile(); //file config con i parametri x configurare il tutto in debug
            String serverName = sServer;
            String masterSolution = sMasterSolution;
            String installation = sInstallationName;

            if (configFile.GetDriver() == DriverType.FileSystem)
            {
                if (!string.IsNullOrEmpty(configFile.GetFSServerName()))
                    serverName = configFile.GetFSServerName();
                if (!string.IsNullOrEmpty(configFile.GetFSInstanceName()))
                    installation = configFile.GetFSInstanceName();
            }
        }

        //-----------------------------------------------------------------------------
        public FileSystemManager(PathFinder aPathFinder)
        {
            this.pathFinder = aPathFinder;
        }

        //-----------------------------------------------------------------------------
        public FileSystemManager(IFileSystemDriver pFileSystem, IFileSystemDriver pAlternative /*NULL*/, PathFinder aPathFinder)
        {
            //TODO LARA
            AttachFileSystemDriver(pFileSystem, false);

            fileSystemDriverOwner = true;
            if (pAlternative != null)
            {
                AttachAlternativeDriver(pAlternative, false);
                alternativeDriverOwner = true;
            }

            this.pathFinder = aPathFinder;
        }

        //-----------------------------------------------------------------------------
        public bool DetectAndAttachAlternativeDriver()
        {
            bool wSAvailable = false;
            //string strSysDBConnectionString = ""; // aPathFinder.st(); todo Lara
            string strSysDBConnectionString = configFile.GetStandardConnectionString();


            bool dBAvailable = string.IsNullOrEmpty(strSysDBConnectionString);
            bool existFileSystem = Directory.Exists(pathFinder.CalculateRemoteStandardPath()) && Directory.Exists(pathFinder.GetCustomCompaniesPath());

            // I cannot work without file system and web service
            if (!existFileSystem && !wSAvailable && !dBAvailable)
            {
                Debug.Assert(false);
                //tODO LARA DIAGNOSTICA
                //AfxGetDiagnostic()->Add(_T("The \\Standard and \\Custom folders are not reacheable on server structure!"));
                //AfxGetDiagnostic()->Add(_T("Please, check network sharings if you are on a client/server installation."));
                return false;
            }

            if (configFile.IsAutoDetectDriver())
            {
                if (configFile.GetDriver() == DriverType.FileSystem && !existFileSystem)
                    configFile.SetDriver(DriverType.Database);
                else
                {
                    if (configFile.GetDriver() == DriverType.Database && !dBAvailable)
                        configFile.SetDriver(DriverType.WebService);
                    else
                        if (configFile.GetDriver() == DriverType.WebService && !wSAvailable)
                        configFile.SetDriver(DriverType.FileSystem);
                }

            }

            if (configFile.GetDriver() == DriverType.Database)
            {
                int nPos = strSysDBConnectionString.LastIndexOf("Data Source");
                if (nPos > 0)
                    strSysDBConnectionString = strSysDBConnectionString.Substring(strSysDBConnectionString.Length - nPos);

                //TODO LARA X CUSTOM CONNECTION
                string customConnection = "";
                AttachAlternativeDriver(new DatabaseDriver(pathFinder, strSysDBConnectionString, customConnection), true);
            }

            return false;
        }



        //-----------------------------------------------------------------------------
        public string GetServerConnectionConfig()
        {
            if (IsManagedByAlternativeDriver(pathFinder.ServerConnectionFile))
                return GetAlternativeDriver().GetServerConnectionConfig(pathFinder.ServerConnectionFile);
            else
                return GetFileSystemDriver().GetServerConnectionConfig(pathFinder.ServerConnectionFile);
        }


        //-----------------------------------------------------------------------------
        public XmlDocument LoadXmlDocument(XmlDocument dom, string filename)
        {
            if (dom == null)
                dom = new XmlDocument();
            if (IsManagedByAlternativeDriver(filename))
                dom.Load(PathFinder.PathFinderInstance.FileSystemManager.GetStream(filename, true));
            else
                dom.Load(filename);

            return dom;
        }

        //-----------------------------------------------------------------------------
        public String GetStreamToString(string sFileName)
        {
            if (IsManagedByAlternativeDriver(sFileName))
                return GetAlternativeDriver().GetStreamToString(sFileName);
            else
                return GetFileSystemDriver().GetStreamToString(sFileName);
        }

        //-----------------------------------------------------------------------------
        public Stream GetStream(string sFileName, bool readStream)
        {
            // no lock is required as methods invoked work only on local variables
            // an they don't locks other objects

            if (IsManagedByAlternativeDriver(sFileName))
                return GetAlternativeDriver().GetStream(sFileName, readStream);
            else
                return GetFileSystemDriver().GetStream(sFileName, readStream);

        }

        //-----------------------------------------------------------------------------
        public bool SaveTextFileFromStream(string sFileName, Stream sFileContent)
        {
	        // no lock is required as methods invoked work only on local variables
	        // an they don't locks other objects

	        bool bOk = false;

	        if (IsManagedByAlternativeDriver(sFileName))
		        bOk = GetAlternativeDriver().SaveTextFileFromStream(sFileName, sFileContent);
	        else
		        bOk = GetFileSystemDriver().SaveTextFileFromStream(sFileName, sFileContent);

	        return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool SaveTextFileFromXml(string sFileName, XmlDocument dom)
        {
            // no lock is required as methods invoked work only on local variables
            // an they don't locks other objects

            bool bOk = false;

            if (IsManagedByAlternativeDriver(sFileName))
                bOk = GetAlternativeDriver().SaveTextFileFromXml(sFileName, dom);
            else
                bOk = GetFileSystemDriver().SaveTextFileFromXml(sFileName, dom);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public byte[] GetBinaryFile(string sFileName, int nLen)
        {
            byte[] binaryContent; ;

            if (IsManagedByAlternativeDriver(sFileName))
                binaryContent = GetAlternativeDriver().GetBinaryFile(sFileName, nLen);
            else
                binaryContent = GetFileSystemDriver().GetBinaryFile(sFileName, nLen);

            return binaryContent;
        }

        //-----------------------------------------------------------------------------
        public bool ExistFile(string sFileName)
        {
            if (String.IsNullOrEmpty(sFileName))
                return false;

            bool bOk = false;

            if (IsManagedByAlternativeDriver(sFileName))
                bOk = GetAlternativeDriver().ExistFile(sFileName);
            else
                bOk = GetFileSystemDriver().ExistFile(sFileName);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool ExistPath(string sPathName)
        {
            if (String.IsNullOrEmpty(sPathName))
                return false;

            bool bOk = false;

            if (IsManagedByAlternativeDriver(sPathName))
                bOk = GetAlternativeDriver().ExistPath(sPathName);
            else
                bOk = GetFileSystemDriver().ExistPath(sPathName);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool CreateFolder(string sPathName, bool bRecursive)
        {
            if (String.IsNullOrEmpty(sPathName))
                return false;

            bool bOk = false;

            if (IsManagedByAlternativeDriver(sPathName))
                bOk = GetAlternativeDriver().CreateFolder(sPathName, bRecursive);
            else
                bOk = GetFileSystemDriver().CreateFolder(sPathName, bRecursive);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public void RemoveFolder(string sPathName, bool bRecursive, bool bRemoveRoot, bool bAndEmptyParents)
        {
            if (IsManagedByAlternativeDriver(sPathName))
                GetAlternativeDriver().RemoveFolder(sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);
            else
                GetFileSystemDriver().RemoveFolder(sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);
        }

        //-----------------------------------------------------------------------------
        public bool RemoveFile(string sFileName)
        {
            bool bOk = false;

            if (IsManagedByAlternativeDriver(sFileName))
                bOk = GetAlternativeDriver().RemoveFile(sFileName);
            else
                bOk = GetFileSystemDriver().RemoveFile(sFileName);


            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool RenameFile(string sOldFileName, string sNewFileName)
        {
            bool bOk = false;

            bool bOldOnAlternative = IsManagedByAlternativeDriver(sOldFileName);
            bool bNewOnAlternative = IsManagedByAlternativeDriver(sNewFileName);
            Stream sContent = null;
            if (IsManagedByAlternativeDriver(sOldFileName) || IsManagedByAlternativeDriver(sNewFileName))
                bOk = GetAlternativeDriver().RenameFile(sOldFileName, sNewFileName);
            else if (bOldOnAlternative && !bNewOnAlternative)
            {
                sContent = GetAlternativeDriver().GetStream(sOldFileName, true);
                bOk = sContent == null || GetFileSystemDriver().SaveTextFileFromStream(sNewFileName, sContent);
            }
            else if (!bOldOnAlternative && bNewOnAlternative)
            {
                sContent = GetFileSystemDriver().GetStream(sOldFileName, true);
                bOk = sContent == null || GetAlternativeDriver().SaveTextFileFromStream(sNewFileName, sContent);
            }
            else
                bOk = GetFileSystemDriver().RenameFile(sOldFileName, sNewFileName);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool RenameFolder(string sOldName, string sNewName)
        {
            bool bOk = false;

            bool bOldOnAlternative = IsManagedByAlternativeDriver(sOldName);
            bool bNewOnAlternative = IsManagedByAlternativeDriver(sNewName);
            Stream sContent = null;
            if (IsManagedByAlternativeDriver(sOldName) || IsManagedByAlternativeDriver(sNewName))
                bOk = GetAlternativeDriver().RenameFolder(sOldName, sNewName);
            else if (bOldOnAlternative && !bNewOnAlternative)
            {
                sContent = GetAlternativeDriver().GetStream(sOldName, true);
                bOk = sContent == null || GetFileSystemDriver().SaveTextFileFromStream(sNewName, sContent);
            }
            else if (!bOldOnAlternative && bNewOnAlternative)
            {
                sContent = GetFileSystemDriver().GetStream(sOldName, true);
                bOk = sContent == null || GetAlternativeDriver().SaveTextFileFromStream(sNewName, sContent);
            }
            else
                bOk = GetFileSystemDriver().RenameFolder(sOldName, sNewName);

            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool CopyFile(string sOldFileName, string sNewFileName, bool bOverWrite)
        {
            bool bOk = false;

            bool bOldOnAlternative = IsManagedByAlternativeDriver(sOldFileName);
            bool bNewOnAlternative = IsManagedByAlternativeDriver(sNewFileName);
            Stream sContent = null;

            if (bOldOnAlternative && bOldOnAlternative)
                bOk = GetAlternativeDriver().CopySingleFile(sOldFileName, sNewFileName, bOverWrite);
            else if (bOldOnAlternative && !bNewOnAlternative)
            {
                sContent = GetAlternativeDriver().GetStream(sOldFileName, true);
                bOk = sContent == null || GetFileSystemDriver().SaveTextFileFromStream(sNewFileName, sContent);
            }
            else if (!bOldOnAlternative && bNewOnAlternative)
            {
                sContent = GetFileSystemDriver().GetStream(sOldFileName, true);
                bOk = sContent == null || GetAlternativeDriver().SaveTextFileFromStream(sNewFileName, sContent);
            }
            else
                bOk = GetFileSystemDriver().CopySingleFile(sOldFileName, sNewFileName, bOverWrite);



            return bOk;
        }


        //-----------------------------------------------------------------------------
        public FileInfo GetFileStatus(string sFileName, FileInfo fs)
        {
            if (IsManagedByAlternativeDriver(sFileName))
                return GetAlternativeDriver().GetFileStatus(sFileName);
            else
                return GetFileSystemDriver().GetFileStatus(sFileName);

        }

        //-----------------------------------------------------------------------------
        public int[] GetFileAttributes(string sFileName)
        {
            int[] attributes = null;

            if (IsManagedByAlternativeDriver(sFileName))
                attributes = GetAlternativeDriver().GetFileAttributes(sFileName);
            else
                attributes = GetFileSystemDriver().GetFileAttributes(sFileName);

            return attributes;
        }

        //-----------------------------------------------------------------------------
        public bool CopyFolder(string sOldPathName, string sNewPathName, bool bOverwrite, bool bRecursive)
        {
            bool bOk = false;

            bool bOldOnAlternative = IsManagedByAlternativeDriver(sOldPathName);
            bool bNewOnAlternative = IsManagedByAlternativeDriver(sNewPathName);

            if (bOldOnAlternative && bOldOnAlternative)
                bOk = GetAlternativeDriver().CopyFolder(sOldPathName, sNewPathName, bOverwrite, bRecursive);
            //else if (bOldOnAlternative && !bNewOnAlternative)
            //    ;//TODOBRUNA situazioni miste in ricorsione
            //else if (!bOldOnAlternative && bNewOnAlternative)
            //    ;//TODOBRUNA situazioni miste in ricorsione
            else
                bOk = GetFileSystemDriver().CopyFolder(sOldPathName, sNewPathName, bOverwrite, bRecursive);

            return bOk;
        }


        //-----------------------------------------------------------------------------
        public List<TBDirectoryInfo> GetSubFolders(string sPathName)
        {
            if (IsManagedByAlternativeDriver(sPathName))
                return GetAlternativeDriver().GetSubFolders(sPathName);
            else
                return GetFileSystemDriver().GetSubFolders(sPathName);
        }

        //-----------------------------------------------------------------------------
        public List<TBFile> GetFiles(string sPathName, string sFileExt)
        {

            if (IsManagedByAlternativeDriver(sPathName))
                return GetAlternativeDriver().GetFiles(sPathName, sFileExt);
            else
                return GetFileSystemDriver().GetFiles(sPathName, sFileExt);

        }

        //-----------------------------------------------------------------------------
        public bool GetPathContent(string sPathName, bool bFolders, out ArrayList pSubFolders, bool bFiles, string strFileExt, out ArrayList pFiles)
        {
            pSubFolders = new ArrayList();
            pFiles = new ArrayList();

            bool result = false; 
            if (IsManagedByAlternativeDriver(sPathName))
                result =GetAlternativeDriver().GetPathContent(sPathName, bFolders, out pSubFolders, bFiles, strFileExt, out pFiles);
            else
                result = GetFileSystemDriver().GetPathContent(sPathName, bFolders, out pSubFolders, bFiles, strFileExt, out pFiles);

            return result;

        }


        //-----------------------------------------------------------------------------
        public bool SaveBinaryFile(string sFileName, byte[] pBinaryContent, int nLen)
        {
            return (IsManagedByAlternativeDriver(sFileName))
                    ? GetAlternativeDriver().SaveBinaryFile(sFileName, pBinaryContent, nLen)
                    : GetFileSystemDriver().SaveBinaryFile(sFileName, pBinaryContent, nLen);
        }

        // gets a binary file and it stores it into the temp directory
        //-----------------------------------------------------------------------------
        public string GetTemporaryBinaryFile(string sFileName)
        {
            int nLen = 0;
            byte[] pBinaryContent = GetBinaryFile(sFileName, nLen);
            if (pBinaryContent == null)
                return sFileName;

            FileInfo file = new FileInfo(sFileName);
            string sExtension = file.Extension;
            string result = Path.GetTempPath();
            string sTempFileName = result + "ITR"  + sExtension;
            File.WriteAllBytes(sTempFileName, pBinaryContent);

            return sTempFileName;

        }

        //-----------------------------------------------------------------------------
        public bool IsAlternativeDriverEnabled()
        {
            // no lock is required as pointer newed on InitInstance and never changed
            return alternativeDriver != null;
        }

        //-----------------------------------------------------------------------------
        public bool IsFileSystemDriverEnabled()
        {
            // no lock is required as pointer newed on InitInstance and never changed
            return fileSystemDriver != null;
        }

        //-----------------------------------------------------------------------------
        public bool IsManagedByAlternativeDriver(string sName)
        {
            return IsAlternativeDriverEnabled() && alternativeDriver.IsAManagedObject(sName);
        }


        //-----------------------------------------------------------------------------
        public void AttachAlternativeDriver(IFileSystemDriver pDriver,  bool bDriverOwner)
        {
            // no lock is required as pointer newed on InitInstance and never changed
            alternativeDriver = pDriver;

            if (alternativeDriver != null)
            {
                alternativeDriverOwner = false;
                return;
            }

            alternativeDriverOwner = bDriverOwner;

        }

        //-----------------------------------------------------------------------------
        public void AttachFileSystemDriver(IFileSystemDriver pDriver, bool bDriverOwner /*true*/)
        {
            // no lock is required as pointer newed on InitInstance and never changed
            fileSystemDriver = pDriver;

            if (fileSystemDriver != null)
            {
                fileSystemDriverOwner = false;
                return;
            }

            fileSystemDriverOwner = bDriverOwner;

        }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetFileSystemDriver()
        {
            // no lock is required as pointer to the object are newed in InitInstance
            // and never changed. The objects pointer are wrapper class to operations
            // (see comment on IFileSystemDriver object)
            return fileSystemDriver;
        }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetAlternativeDriver()
        {
            // no lock is required as pointer to the object are newed in InitInstance
            // and never changed. The objects pointer are wrapper class to operations
            // (see comment on IFileSystemDriver object)
            return alternativeDriver;
        }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetAlternativeDriverIfManagedFile(string sName)
        {
            bool ok = IsAlternativeDriverEnabled() && alternativeDriver.IsAManagedObject(sName);
            if (ok)
                return GetAlternativeDriver();
            
            return null;
        }

        //-----------------------------------------------------------------------------
        public bool Start(bool bLoadCaches  /*true*/)
        {
            // no lock is required as invoked in InitInstance
            if (fileSystemDriver != null)
                fileSystemDriver.Start();

            if (alternativeDriver != null)
                alternativeDriver.Start();

            return true;
        }

        //-----------------------------------------------------------------------------
        public bool Stop(bool bLoadCaches  /*true*/)
        {
            // no lock is required as invoked in ExitInstance
            if (fileSystemDriver != null)
                fileSystemDriver.Stop();

            if (alternativeDriver != null)
                alternativeDriver.Stop();

            return true;
        }

        //-----------------------------------------------------------------------------
        public ArrayList GetAllApplicationInfo()
        { 
            if (GetAlternativeDriver() != null)
                return GetAlternativeDriver().GetAllApplicationInfo();
            else
                return GetFileSystemDriver().GetAllApplicationInfo();
        }

        //-----------------------------------------------------------------------------
        public ArrayList GetAllModuleInfo(string strAppName)
        {
	        if (GetAlternativeDriver() != null)
                return GetAlternativeDriver().GetAllModuleInfo(strAppName);
	        else
                return GetFileSystemDriver().GetAllModuleInfo(strAppName);
        }

        ////-----------------------------------------------------------------------------
        //public string GetFormattedQueryTime()
        //{
        //    if (IsAlternativeDriverEnabled() && GetAlternativeDriver() is DatabaseDriver)
        //        return ((DatabaseDriver)m_pAlternativeDriver).GetFormattedQueryTime();

        //    return string.Empty;
        //}
        ////-----------------------------------------------------------------------------
        //public string GetFormattedFetchTime()
        //{
        //    if (IsAlternativeDriverEnabled() && GetAlternativeDriver() is DatabaseDriver)
        //        return ((DatabaseDriver)m_pAlternativeDriver).GetFormattedFetchTime();

        //    return string.Empty;
        //}

    }
}

