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
    public interface IFileSystemDriver
    {
        TBFile GetTBFile(string strCompleteFileName);
        ArrayList GetAllApplicationInfo(string dir);
        ArrayList GetAllModuleInfo(string strAppName);

        string GetDriverDescription() ;
        bool IsAManagedObject(string sFileName);
        string GetServerConnectionConfig(string filePath);
        Stream GetStream(string sFileName, bool readStream);
        String GetFileTextFromFileName(string sFileName);
        bool SaveTextFileFromXml(string sFileName, XmlDocument dom);
        bool SaveTextFileFromStream(string sFileName, Stream sFileContent);
        byte[] GetBinaryFile(string sFileName, int nLen);
	    bool SaveBinaryFile( string sFileName, byte[] pBinaryContent, int nLen);
        bool ExistFile( string sFileName);
        bool RemoveFile(string sFileName);
	    bool RenameFile(string sOldFileName, string sNewName);
        bool RenameFolder(string sOldName, string sNewName);
        FileInfo GetFileStatus(string sFileName);
        int[] GetFileAttributes(string sFileName);
	    bool CopySingleFile(string sOldFileName, string sNewName, bool bOverWrite);

        bool ExistPath(string sPathName);
        bool CreateFolder(string sPathName, bool bRecursive);
	    void RemoveFolder(string sPathName, bool bRecursive, bool bRemoveRoot, bool bAndEmptyParents = false);
        bool CopyFolder(string sOldPathName, string sNewPathName, bool bOverwrite, bool bRecursive);
        List<TBDirectoryInfo> GetSubFolders(string sPathName);
        bool GetPathContent(string strPathName, bool bFolders, out ArrayList pSubFolders, bool bFiles, string strFileExt, out ArrayList pFiles);
        List<TBFile> GetFiles(string sPathName, string sFileExt);

        bool Start();
        bool Stop();
        bool IsStarted() ;
    }
}
