using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TaskBuilderNetCore.Interfaces
{
    enum EncodingType { ANSI, UTF8, UTF16_BE, UTF16_LE }; //UTF16_BE: Big Endian (swap sui byte); UTF16_LE: Little Endian 
    //=========================================================================
    public class TBFile
    {
        
        public long     fileID;
        public long     parentID;
        public string   name;
        
        public string   PathName;
        public string   fileNamespace;
        public string   appName;
        public string   moduleName;
        public string   objectType;
        public long     fileSize;
        public byte[]   fileContent;
        public string   fileContentString; //contenuto del file di tipo testo
        public string   completeFileName;
        public bool     isCustomPath;
        public bool     isDirectory = false;
        //serve per la custom
        public string   accountName;




        public IFileSystemDriver    alternativeDriver = null;
        public FileInfo             fileInfo = null;
        public TBFile               tbFile = null;
        public DateTime             creationTime;
        public DateTime             lastWriteTime;
        public bool                 isReadOnly;
        public string               FileExtension;


        //----------------------------------------------------------------------------
        public DateTime CreationTime
        {
            get
            {
                if (alternativeDriver == null)
                    return fileInfo.CreationTime;

                return alternativeDriver.GetTBFile(completeFileName).creationTime;
            }
        }

        //----------------------------------------------------------------------------
        public bool Readonly
        {
            get
            {
                if (alternativeDriver == null)
                    return fileInfo.IsReadOnly;

                return alternativeDriver.GetTBFile(completeFileName).isReadOnly;
            }
            set
            {
                if (alternativeDriver == null)
                    fileInfo.IsReadOnly = value;

                alternativeDriver.GetTBFile(completeFileName).isReadOnly = value;
            }
        }

        //----------------------------------------------------------------------------
        public DateTime LastWriteTime
        {
            get
            {
                if (alternativeDriver == null)
                    return fileInfo.LastWriteTime;

                return alternativeDriver.GetTBFile(completeFileName).lastWriteTime;
            }
        }

        //----------------------------------------------------------------------------
        public TBFile(string strCompleteFileName, IFileSystemDriver alternativeDriver)
        {

            //TODO LARA AGGIUNGI PATHNAME
            if (alternativeDriver == null)
            {
                FileInfo fileInfo = new FileInfo(strCompleteFileName);
                this.fileInfo = fileInfo;
            }
            else
                tbFile = alternativeDriver.GetTBFile(strCompleteFileName);


            this.alternativeDriver = alternativeDriver;
            completeFileName = strCompleteFileName;
            string path = strCompleteFileName.Substring (0, strCompleteFileName.LastIndexOf('\\'));
            PathName = path;
            name = strCompleteFileName.Substring(strCompleteFileName.LastIndexOf('\\') + 1);
            FileExtension = strCompleteFileName.Substring(strCompleteFileName.LastIndexOf('.'));

        }



        //----------------------------------------------------------------------------
        public TBFile(string strName, string strPathName, IFileSystemDriver alternativeDriver)
        {
            this.alternativeDriver = alternativeDriver;
            completeFileName = Path.Combine(strPathName, strName);

            name = strName;
            PathName = strPathName;
            isCustomPath = false;
            fileContent = null;
            fileSize = 0;

            FileInfo fileInfo = new FileInfo(strPathName);
            if (alternativeDriver == null)
                this.fileInfo = fileInfo;
            else
                tbFile = alternativeDriver.GetTBFile(completeFileName);

            FileExtension = fileInfo.Extension;
        }

        //----------------------------------------------------------------------------
        ~TBFile()
        {
            if (fileContent != null)
                fileContent = null;
        }

        //-------------------------------------------------------------
        EncodingType GetEncodingType(byte[] pBinaryContent, long nSize)
        {
            EncodingType encodingType = EncodingType.ANSI;
            if (pBinaryContent == null || nSize < 3)
                return encodingType;

            if (pBinaryContent[0] == 0xFF && pBinaryContent[1] == 0xFE)  //UTF-16 (LE)  little endian -- Unicode
                encodingType = EncodingType.UTF16_LE;
            else
            {
                if (pBinaryContent[0] == 0xFF && pBinaryContent[1] == 0xFF) //UTF- 16 (BE) big endian
                    encodingType = EncodingType.UTF16_BE;
                else
                 if (pBinaryContent[0] == 0xEF && pBinaryContent[1] == 0xBB && pBinaryContent[2] == 0xBF) //UTF8
                    encodingType = EncodingType.UTF8;
            }

            return encodingType;
        }
            
        //----------------------------------------------------------------------------
        public string GetContentAsString()
        {
            if (!string.IsNullOrEmpty(fileContentString))
                return fileContentString;

            if (fileContent == null || fileSize == 0)
                return string.Empty;

            //AfxGetPathFinder()->GetMetadataManager()->StartTimeOperation(CONVERT_METADATA);
            string strContent;
            //devo convertire il binario nella giusta stringa a seconda del suo tipo
            EncodingType encodingType = GetEncodingType(fileContent, fileSize);
            switch (encodingType)
            {
                case EncodingType.ANSI:
                    {
                        return strContent = System.Text.Encoding.ASCII.GetString(fileContent); 
                    }

                case EncodingType.UTF16_BE:
                case EncodingType.UTF16_LE:
                    {
                        Debug.Fail("EncodingType.UTF16_BE");
                        break;
                    }

                case EncodingType.UTF8:
                    {
                        return strContent = System.Text.Encoding.UTF8.GetString(fileContent);
                    }
            }
            //AfxGetPathFinder()->GetMetadataManager()->StopTimeOperation(CONVERT_METADATA);

            return string.Empty;
        }

        //----------------------------------------------------------------------------
        public byte[] GetContentAsBinary()
        {
            if (fileContent == null)
                return fileContent;

            if (string.IsNullOrEmpty(fileContentString))
                return null;

            return  Encoding.ASCII.GetBytes(fileContentString);
        }
    }

    //=========================================================================
    public class TBDirectoryInfo 
    {
        public bool fromDB = false;
        public long fileID;
        public long parentID;
        public string name;

        public string Namespace;
        public string appName;
        public string moduleName;
        public string objectType;
        public long   Size;
        public bool isDirectory;

        public bool isCustomPath;
        public DateTime creationTime;
        public DateTime lastWriteTime;
        public bool isReadOnly;
        public DirectoryInfo direcotryInfo = null;
        public IFileSystemDriver alternativeDriver = null;
        //serve per la custom
        public string accountName;


        public string CompleteDirectoryPath = string.Empty;

        //----------------------------------------------------------------------------
        public TBDirectoryInfo(string strCompleteFileName, IFileSystemDriver alternativeDriver)
        {
            this.alternativeDriver = alternativeDriver;
            CompleteDirectoryPath = strCompleteFileName;
            isCustomPath = false;
            Size = 0;

            DirectoryInfo dir = new DirectoryInfo(strCompleteFileName);

            if (alternativeDriver == null)    
                this.direcotryInfo = dir;
            else


            name = dir.Name;
            CompleteDirectoryPath = dir.FullName; //CHiedi anna
        }

        //----------------------------------------------------------------------------
        public TBDirectoryInfo(string strName, string strPathName, IFileSystemDriver alternativeDriver)
        {
            name = strName;
            CompleteDirectoryPath = Path.Combine(strPathName, strName);
            isCustomPath = false;

            Size = 0;

            DirectoryInfo dir = new DirectoryInfo(CompleteDirectoryPath);

            if (alternativeDriver == null)
                this.direcotryInfo = dir;

            CompleteDirectoryPath = strPathName + "\\" + strName;

        }

     }
}
