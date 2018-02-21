using TaskBuilderNetCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Microarea.Common.Generic.InstallationInfo;

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
        public bool IsAManagedObject( string sFileName) //TODO LARA
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
                using (FileStream fileStrean = File.Create(sFileName))
                {
                    fileStrean.Write(Encoding.ASCII.GetBytes(dom.InnerXml), 0, Encoding.ASCII.GetByteCount(dom.InnerXml));
                    fileStrean.Flush();
                    fileStrean.Dispose();
                }
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
            return File.ReadAllBytes(sFileName);
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
            try
            {
                File.WriteAllBytes(sFileName, sBinaryContent);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

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
                 return  File.Open(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                sr = new StreamReader(File.OpenRead(sFileName), true);
                fileContent = sr.ReadToEnd();

            }
            catch (Exception)
            {
                return null;
            }

            return sr.BaseStream;
        }

        //-----------------------------------------------------------------------------
        public String GetFileTextFromFileName(string sFileName)
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
        public List<string> GetAllApplicationInfo(string apps)
        {
            List<string> tempApplications = new List<string>();
            //prendo tutte le applicazioni di tb tb.net tbapps tools apps.net
            Functions.ReadSubDirectoryList(apps, out tempApplications);

            return tempApplications;
        }

        //-----------------------------------------------------------------------------
        public List<string> GetAllModuleInfo(string strAppName)
        {
            List<string> allModulesArray = new List<string>();
            Functions.ReadSubDirectoryList(strAppName, out allModulesArray);
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
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sOldPathName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sOldPathName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(sNewPathName))
            {
                Directory.CreateDirectory(sNewPathName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(sNewPathName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (bRecursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(sNewPathName, subdir.Name);
                    CopyFolder(subdir.FullName, temppath, bOverwrite, bRecursive);
                }
            }

          
            return true;

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
        public bool GetPathContent( string sPathName, bool bFolders, out List<TBDirectoryInfo> dirs, bool bFiles,  string sFileExt, out List<TBFile> elements)
        {
            elements = new List<TBFile>();
            dirs = new List<TBDirectoryInfo>();

            DirectoryInfo dir = new DirectoryInfo(sPathName);

            if (bFolders)
            {
                foreach (string path in Directory.GetDirectories(sPathName))
                    dirs.Add(new TBDirectoryInfo(path, null));
            }
               
            if (bFiles)
            {
                foreach (FileInfo file in dir.GetFiles(sFileExt))
                    elements.Add(new TBFile(file.FullName, null));
            }


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
