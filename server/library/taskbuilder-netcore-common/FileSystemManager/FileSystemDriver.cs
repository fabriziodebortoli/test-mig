﻿using TaskBuilderNetCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microarea.Common.FileSystemManager
{
    //=========================================================================
    public class FileSystemDriver : IFileSystemDriver
    {
        bool m_bStarted = false;

        public FileSystemDriver()
        {

        }

        //---------------------------------------------------------------------
        public bool Start()
        {
            return m_bStarted;
        }

        //----------------------------------------------------------------------
        public bool Stop()
        {
            return m_bStarted;
        }

        //---------------------------------------------------------------------
        public bool IsAManagedObject( string sFileName) 
        {
	        return true;
        }

        //---------------------------------------------------------------------
        public TBFile GetTBFile(string strCompleteFileName)
        {
            return new TBFile(strCompleteFileName, null);
        }

        //---------------------------------------------------------------------
        public bool SaveTextFileFromXml(string sFileName, XmlDocument dom)
        {
            try
            {
                dom.Save(File.OpenWrite(sFileName));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //----------------------------------------------------------------------
        public byte[] GetBinaryFile(string sFileName, int nLen)
        {

	        return null;
        }
        //-----------------------------------------------------------------------------
        public string GetDriverDescription() 
        {
	        return "File System from LAN Sharings";
        }


        // see use of CPiture, CImage, etc...
        //-----------------------------------------------------------------------------
        public bool SaveBinaryFile( string sFileName, byte[] sBinaryContent, int nLen)
        {
            return true;
        }

        ////-----------------------------------------------------------------------
        //private string GetAdjustedPath(string pathName)
        //{
        //    string path = pathName.ToLower();

        //    if (!path.StartsWith(FileSystemMonitorEngine.FileSystem.ServerPath))
        //        return pathName;

        //    string localPath = path.Replace(FileSystem.ServerPath, "");

        //    if (localPath.StartsWith(NameSolverStrings.Running.ToLower()))
        //    {
        //        localPath = localPath.Replace(NameSolverStrings.Running.ToLower(), "");
        //        //Lara
        //        //localPath = pathFinder.GetRunningPath() + localPath;
        //        localPath = pathFinder.GetStandardPath + localPath;

        //    }
        //    else if (localPath.StartsWith(NameSolverStrings.Standard.ToLower()))
        //    {
        //        localPath = localPath.Replace(NameSolverStrings.Standard.ToLower(), "");
        //        localPath = pathFinder.GetStandardPath + localPath;
        //    }
        //    else if (localPath.StartsWith(NameSolverStrings.Custom.ToLower()))
        //    {
        //        localPath = localPath.Replace(NameSolverStrings.Custom.ToLower(), "");
        //        localPath = pathFinder.GetCustomPath() + localPath;
        //    }

        //    return localPath;
        //}
        // see use of CLineFile, CXmlSaxReader, CXmlDocObj
        //-----------------------------------------------------------------------------
        public Stream GetStream( string sFileName, bool readStream)
        {
            string fileContent = string.Empty;
            StreamReader sr = null;

            if (sFileName == string.Empty)
                return null;

        
            if (!File.Exists(sFileName))
                return null;

            

            try
            {
                // file content
                if (! readStream)
                 return  File.Open(sFileName, FileMode.Open);

                sr = new StreamReader(File.OpenRead(sFileName), true);
                fileContent = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
            }
            catch (Exception exx)
            {
                return null;
            }

            return sr.BaseStream;
        }

        //-----------------------------------------------------------------------------
        public String GetStreamToString(string sFileName)
        {
            string fileContent = string.Empty;
            StreamReader sr = null;

            if (sFileName == string.Empty)
                return null;

            if (!File.Exists(sFileName))
                return null;
            try
            {
                // file content
                sr = new StreamReader(File.OpenRead(sFileName), true);
                fileContent = sr.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }

            return fileContent;
        }

        //
        //-----------------------------------------------------------------------------
        public bool SaveTextFileFromStream(string sFileName, Stream sFileContent)
        { 
            FileStream fileStrean = File.Create(sFileName);

            sFileContent.CopyTo(fileStrean);
            return true;
        }

        //-----------------------------------------------------------------------------
        public bool ExistFile(string sFileName)
        {
            return File.Exists(sFileName);
        }

        //-----------------------------------------------------------------------------
        public bool ExistPath(string sPathName)
        {
            if (String.IsNullOrEmpty(sPathName))
                return false;

            return Directory.Exists(sPathName);
        }
        //-----------------------------------------------------------------------------
        public ArrayList  GetAllApplicationInfo()
        {
            ArrayList allAppArray = new ArrayList();

            //const CString sAppContainerPath = AfxGetPathFinder()->GetContainerPath(CPathFinder::TB);
            //CString strFolder = szTaskBuilderApp;
            //CString strPath = sAppContainerPath + SLASH_CHAR + strFolder;
            //strPath.MakeLower();
            //pAppsPath->Add(strPath);

            //strFolder = szExtensionsApp;
            //strPath = sAppContainerPath + SLASH_CHAR + strFolder;
            //strPath.MakeLower();
            //pAppsPath->Add(strPath);

            //AddApplicationDirectories(AfxGetPathFinder()->GetContainerPath(CPathFinder::TB_APPLICATION), pAppsPath);
            //AddApplicationDirectories(AfxGetPathFinder()->GetCustomApplicationsPath(), pAppsPath);

            return allAppArray;
        }

        //-----------------------------------------------------------------------------
        public ArrayList GetAllModuleInfo(string strAppName)
        {
            ArrayList allModulesArray = new ArrayList();
            //   ASSERT(pModulesPath);
            //   // load modules namespaces into map form file system
            //   AddApplicationModules(AfxGetPathFinder()->GetApplicationPath(strAppName, CPathFinder::STANDARD), pModulesPath, false);
            //if (pModulesPath->GetSize() == 0) //non ho moduli: si tratta di un'applicazione nella custom?

            //   AddApplicationModules(AfxGetPathFinder()->GetApplicationPath(strAppName, CPathFinder::CUSTOM, FALSE, CPathFinder::ALL_COMPANIES), pModulesPath, true);

            return allModulesArray;
        }
        //-----------------------------------------------------------------------------
        public bool CreateFolder(string sPathName, bool bRecursive)
        {
            if (string.IsNullOrEmpty(sPathName))
                return false;

            DirectoryInfo dir = Directory.CreateDirectory(sPathName);
            return dir != null;
            //}
            //bool bOk = false;
            ////string strParentPath;

            //if (sPathName.Substring(sPathName.Length - 1) == "\\")
            //    strParentPath = GetPath(sPathName.Left(sPathName.GetLength() - 1));
            //else
            //    strParentPath = GetPath(sPathName);

            //if (!Directory.Exists(strParentPath))
            //{ !CreateFolder(strParentPath, bRecursive))
            //    bOk = false;
            //else
            //    bOk =  ::CreateDirectory(sPathName, null);

            //    return bOk;
        }

        //-----------------------------------------------------------------------------
        public void  RemoveFolder( string sPathName,  bool  bRecursive,  bool  bRemoveRoot,  bool  bAndEmptyParents)
        {
            if (bRecursive)
                Directory.Delete(sPathName, bRecursive);

            if (bRemoveRoot)
                Directory.Delete(sPathName);

            if (bAndEmptyParents)
                Directory.GetParent(sPathName).Delete();

        }

        //-----------------------------------------------------------------------------
        public bool RemoveChildsFolders(string sPathName)
        {
            Directory.Delete(sPathName, true);

            return true;
        }

        //-----------------------------------------------------------------------------
        public bool RemoveParentFolders(string sPathName)
        {
            Directory.GetParent(sPathName).Delete();

            return true;
        }

        //-----------------------------------------------------------------------------
        public bool RemoveFile(string sFileName)
        {
            File.Delete(sFileName);

            return true;
        }

        //-----------------------------------------------------------------------------
        public bool RenameFile(string sOldFileName, string sNewFileName)
        {
            bool bOk = false;

            try
            { 
                System.IO.File.Move(sOldFileName, sNewFileName);
                bOk = true;
            }
            catch(Exception exx)
            {
                Debug.WriteLine(exx.Message);
            }
            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool RenameFolder(string sOldFileName, string sNewFileName)
        {
            bool bOk = false;

            try
            {
                System.IO.Directory.Move(sOldFileName, sNewFileName);
                bOk = true;
            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx.Message);
            }
            return bOk;
        }

        //-----------------------------------------------------------------------------
        public bool CopySingleFile( string sOldFileName, string sNewFileName, bool  bOverWrite)
        {
            bool bOk = false;

            try
            {
                FileInfo file = new FileInfo(sNewFileName);

                if (!Directory.Exists(file.Directory.FullName))
                    Directory.CreateDirectory(file.Directory.FullName);

                System.IO.File.Copy(sOldFileName, sNewFileName, bOverWrite);
                bOk = true;
            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx.Message);
            }
            return bOk;
        }

        //-----------------------------------------------------------------------------
        public FileInfo  GetFileStatus(string sFileName )
        {
            return new FileInfo(sFileName);
        }

        //-----------------------------------------------------------------------------
        public int[] GetFileAttributes(string sFileName)
        {
            int[] a = null;
            return a; // GetFileAttributes(sFileName); TODO LARA NN HO CAPITO
        }

        //-----------------------------------------------------------------------------
        public bool CopyFolder( string sOldPathName,  string sNewPathName,  bool  bOverwrite,  bool  bRecursive)
        {
            bool bOk = false;
            //DirectoryInfo dir = new DirectoryInfo(sOldPathName);
            //try
            //{
            //    System.IO.DirectoryInfo(sOldPathName, sNewPathName, bOverwrite);
            //    foreach(DirectoryInfo in )
                bOk = true;
            //}
            //catch (Exception exx)
            //{
            //    Debug.WriteLine(exx.Message);
            //}
            return bOk;

        }

        //-----------------------------------------------------------------------------
        public List<TBDirectoryInfo> GetSubFolders( string sPathName)
        {
            DirectoryInfo dir = new DirectoryInfo(sPathName);
            List<TBDirectoryInfo> subfolder = new List<TBDirectoryInfo>();
            foreach (DirectoryInfo subdir in dir.GetDirectories())
                subfolder.Add(new TBDirectoryInfo(subdir.FullName, null));
            return subfolder;
        }

        //-----------------------------------------------------------------------------
        public List<TBFile> GetFiles(string sPathName, string extension)
        {
            List<TBFile> files = new List<TBFile>();
            string[] filesFullpath = Directory.GetFiles(sPathName, extension);

            foreach (string fileFullName in filesFullpath)
                files.Add(new TBFile(fileFullName, null));

            return files;
        }
      
        //-----------------------------------------------------------------------------
        public bool IsStarted()
        {
            return m_bStarted;
        }
        //-----------------------------------------------------------------------------
        public bool GetPathContent( string sPathName, bool bFolders, out ArrayList dirs, bool bFiles,  string sFileExt, out ArrayList elements)
        {
            elements = new ArrayList();
            dirs = new ArrayList();

            DirectoryInfo dir = new DirectoryInfo(sPathName);

            if (bFolders)
                elements.AddRange(dir.GetDirectories(sPathName, SearchOption.AllDirectories));

            if (bFiles)
                dirs.AddRange(dir.GetFiles(sFileExt));

            return true;
        }

        //-----------------------------------------------------------------------------
        public string GetServerConnectionConfig(string filePath)
        {
            // no lock is required
            System.Xml.XmlDocument lookUpDocument = null;
            lookUpDocument = new System.Xml.XmlDocument();
            lookUpDocument.Load(filePath);

            return lookUpDocument.InnerText;
        }
    }
}