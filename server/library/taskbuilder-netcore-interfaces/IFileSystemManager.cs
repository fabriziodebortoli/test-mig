using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TaskBuilderNetCore.Interfaces
{
    //=========================================================================
    public interface IFileSystemManager
    {
        IFileSystemDriver GetAlternativeDriverIfManagedFile(string sName);
        bool SaveTextFileFromXml(string sFileName, XmlDocument dom);
        XmlDocument LoadXmlDocument(XmlDocument dom, string filename);
        bool DetectAndAttachAlternativeDriver();
        List<string> GetAllApplicationInfo(string dir);
        List<string> GetAllModuleInfo(string strAppName);
        void Init(string sServer, string sInstallationName, String sMasterSolution);
        string GetServerConnectionConfig();
        Stream GetStream(string sFileName, bool readStream);
        String GetFileTextFromFileName(string sFileName);
        bool SaveTextFileFromStream(string sFileName, Stream sFileContent);
        byte[] GetBinaryFile(string sFileName, int nLen);
        bool ExistFile(string sFileName);
        bool ExistPath(string sPathName);
        bool CreateFolder(string sPathName, bool bRecursive);
        void RemoveFolder(string sPathName, bool bRecursive, bool bRemoveRoot, bool bAndEmptyParents);
        bool RemoveFile(string sFileName);
        bool RenameFile(string sOldFileName, string sNewFileName);
        bool CopyFile(string sOldFileName, string sNewFileName, bool bOverWrite);
        FileInfo GetFileStatus(string sFileName, FileInfo fs);
        int[] GetFileAttributes(string sFileName);
        bool CopyFolder(string sOldPathName, string sNewPathName, bool bOverwrite, bool bRecursive);
        List<TBDirectoryInfo> GetSubFolders(string sPathName);
        List<TBFile> GetFiles(string sPathName, string sFileExt);
        bool GetPathContent(string strPathName, bool bFolders, out List<TBDirectoryInfo> pSubFolders, bool bFiles, string strFileExt, out List<TBFile> pFiles);
        bool SaveBinaryFile(string sFileName, byte[] pBinaryContent, int nLen);
        string GetTemporaryBinaryFile(string sFileName);
        bool IsAlternativeDriverEnabled();
        bool IsFileSystemDriverEnabled();
        bool IsManagedByAlternativeDriver(string sName);
        void AttachAlternativeDriver(IFileSystemDriver pDriver, bool bDriverOwner);
        void AttachFileSystemDriver(IFileSystemDriver pDriver, bool bDriverOwner /*true*/);
        IFileSystemDriver GetFileSystemDriver();
        IFileSystemDriver GetAlternativeDriver();
        bool Start(bool bLoadCaches  /*true*/);
        bool Stop(bool bLoadCaches  /*true*/);
    }
}
