﻿using TaskBuilderNetCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using System.Data;
using System.IO;
using System.Xml;

namespace Microarea.Common.FileSystemManager
{
    //=========================================================================
    public class DatabaseDriver : IFileSystemDriver
    {
        private string standardConnectionString = string.Empty;
        private string customConnectionString = string.Empty;
        private const string szInstanceKey = "I-M4";
        private const string szMPInstanceTBFS = "MP_InstanceTBFS";
        private const string szTBCustomMetadata = "TB_CustomMetadata";
        private PathFinder pathFinder = null;
        private bool started = false;

		//---------------------------------------------------------------------
		public DatabaseDriver(PathFinder pathFinder, string standardConnectionString, string customConnectionString) //TODO LARA
		{
			this.pathFinder = pathFinder;
			this.customConnectionString = customConnectionString;
			this.standardConnectionString = standardConnectionString;
		}

        //-----------------------------------------------------------------------------
        public string GetDriverDescription()
        {
            return "DatabaseDriver";
        }
        
        //---------------------------------------------------------------------
        public string GetType(string fileFullName)
        {
	        INameSpace aTBNamespace = pathFinder.GetNamespaceFromPath(fileFullName);
	        if (aTBNamespace.IsValid())
	        {
		        Type strType = aTBNamespace.GetType();
		        return strType.ToString().ToUpper();
	        }

            string lowerFileName = fileFullName;
            lowerFileName = lowerFileName.ToLower();
	        if (lowerFileName.Contains("application.config"))
		        return "APPLICATION";

	        if (lowerFileName.Contains("module.config")) 
		        return "MODULE";

	        if (lowerFileName.Contains(".menu")) 
		        return "MENU";

	        if (lowerFileName.Contains("description"))
		        return "DESCRIPTION";

	        if (lowerFileName.Contains("exportprofiles")) 
		        return "EXPORTPROFILES";

	        if (lowerFileName.Contains("datamanager"))
		        return "DATA";

	        if (lowerFileName.Contains("brand"))
		        return "BRAND";

	        if (lowerFileName.Contains("themes")) 
		        return "THEMES";

	        if (lowerFileName.Contains(".sql")) 
		        return "SQL";

	        if (lowerFileName.Contains(".hjson") || lowerFileName.Contains(".tbjson"))
		        return "JSONFORM";

	        if (lowerFileName.Contains("settings.config") || lowerFileName.Contains("\\settings\\"))
		        return "SETTING";

	        if (lowerFileName.Contains(".gif") || lowerFileName.Contains(".jpg")|| lowerFileName.Contains(".png"))
		        return "IMAGE";

	        if (lowerFileName.Contains("\\moduleobjects\\"))
	        {
                string fileName = fileFullName.Substring(fileFullName.LastIndexOf('\\') + 1);
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                return  fileName.ToUpper();
            }
	        return "";
        }

        //---------------------------------------------------------------------
        //è possibile che le richieste vengano eseguite anche con della path che contengono URL_@"\\"
        // mentre sul DB le path sono sempre salvate con @"\\"
        public string GetTBFSFileCompleteName(  string strPathFileName)
        {
            string strFileName = strPathFileName;
            strFileName.Replace('/', '\\');
            return strFileName;
        }

        //----------------------------------------------------------------------------
        bool IsARootPath(string strTBFSFolder)
        {
            return (strTBFSFolder.CompareTo(pathFinder.CalculateRemoteStandardPath()) == 0 || strTBFSFolder.CompareTo(pathFinder.GetCustomPath()) == 0 || 
                strTBFSFolder.CompareTo(pathFinder.GetCustomCompaniesPath()) == 0 || strTBFSFolder.CompareTo(pathFinder.GetCustomCompanyPath()) == 0);
        }

        //toglie la parte assoluto della path c:\InstallationName\Standard o c:\InstallationName\Custom\Companies\Companyname
        //----------------------------------------------------------------------------
        public string GetRelativePath( string strPathFileName, bool bCustom)
        {
            string strRelativePath = strPathFileName;
            strRelativePath =strRelativePath.ToUpper();

            string strStartPath = (bCustom) ? pathFinder.GetCustomCompaniesPath() : pathFinder.CalculateRemoteStandardPath();
            strStartPath = strStartPath.ToUpper();
            int nPos = strRelativePath.IndexOf(strStartPath);
            int nEscape = (strStartPath.Length + 1);
            //alla posizione devo aggiungere 1 per il backslash
            return (nPos >= 0) ? strRelativePath.Right(strPathFileName.Length - nEscape) : strPathFileName;
        }

        //--------------------------------------------------------------------------
        string GetAbsolutePath(string strPathFileName, bool bCustom)
        {
            string initialPath = (bCustom) ? pathFinder.GetCustomCompaniesPath() : pathFinder.CalculateRemoteStandardPath();
            return initialPath + @"\\" + strPathFileName;
        }

        //---------------------------------------------------------------------
        public bool IsAManagedObject(string sFileName)
        {
            return sFileName.Contains(pathFinder.CalculateRemoteStandardPath()) ;
        }

        //se non esiste il parent lo inserisco, vado in ricorsione
        //----------------------------------------------------------------------------------------------------------------
        public int InsertMetadataFolder(SqlConnection sqlConnection, String strFolder, String application, String module, bool bCustom, String accountName, bool toCreate)
        {
	        if (strFolder == string.Empty)
		        return -1;

	        
	        SqlTransaction trans = null;
	        String tableName = (bCustom) ? "TB_CustomMetadata" : "MP_InstanceTBFS";
	        int parentID = -1;
	        //verifico se la path esiste
	        String commandText = string.Format("SELECT FileID from {0} WHERE PathName = '{1}' AND IsDirectory = '1'", tableName, strFolder);
	        if (!bCustom)
		        commandText += string.Format(" AND InstanceKey = \'{0}\'", szInstanceKey);

            SqlCommand command = null;

            try
	        {

                command = new SqlCommand(commandText, sqlConnection);
		        System.Object value = command.ExecuteScalar();
		        if (value != null)
			        parentID = (Int32)value;
		        //se la path non esiste vado in ricorsione sul parent
		        int lastBackSlash = strFolder.LastIndexOf('\\');
		        String parent = (lastBackSlash > 0) ? strFolder.Substring(0, lastBackSlash) : strFolder;
                command = null;

		        if (parentID == -1 && lastBackSlash > 0 && toCreate)
			        parentID = InsertMetadataFolder(sqlConnection, parent, application, module, bCustom, accountName, toCreate);
		        else
			        //la directory esiste già
			        if (parentID > -1)
				        return parentID;

		        String strInsertCommandText = (bCustom)
			        ? string.Format("INSERT INTO {0} (ParentID, PathName, Application, Module, CompleteFileName, ObjectType, IsDirectory, TBCreatedID, TBModifiedID)  VALUES ( {1}, '{2}', '{3}', '{4}', '{2}\\', 'DIRECTORY', '1', '{5}', '{5}')", tableName, (parentID == -1) ? "null" : parentID.ToString(), strFolder, application, module, GetWorkerId())
			        : string.Format("INSERT INTO {0} (ParentID, PathName, Application, Module, CompleteFileName, ObjectType, IsDirectory, InstanceKey)  VALUES ( {1}, '{2}', '{3}', '{4}', '{2}\\', 'DIRECTORY', '1', {5})", tableName, (parentID == -1) ? "null" : parentID.ToString(), parent, application, module, szInstanceKey);

                trans = sqlConnection.BeginTransaction();
                command = new SqlCommand(strInsertCommandText, sqlConnection, trans);

                command.ExecuteNonQuery();
                command.CommandText = "SELECT SCOPE_IDENTITY()";
		        value = command.ExecuteScalar();
		        if (value != null)
			        parentID = Convert.ToInt32(value);
                trans.Commit();
                command.Dispose();
		        trans.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (command != null)
                    command.Dispose();
		        if (trans != null)
			        trans.Dispose();
		        throw(e);
	        }
	        return parentID;
        }

        private int GetWorkerId()
        {
            return -1; //TODO LARA
        }
        //-----------------------------------------------------------------------------
        public string  GetServerConnectionConfig(string filePath)
        {
            return GetStream(filePath, true).ToString();
        }

        //----------------------------------------------------------------------------
        public String GetStreamToString(string sFileName)
        {
            if (String.IsNullOrEmpty(sFileName))
                return null;

            TBFile pTBFile = GetTBFile(sFileName);
            String strTextContent = (pTBFile != null) ? pTBFile.GetContentAsString() : "";
            Debug.Fail(pTBFile.completeFileName);
            if (pTBFile != null)
                pTBFile = null;
            Debug.Fail("!strTextContent.IsEmpty()");
            StreamReader sr;
            String fileContent = string.Empty;
            try
            {
                // file content
                sr = new StreamReader(strTextContent, true);
                fileContent = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return fileContent;
        }

        //----------------------------------------------------------------------------
        public Stream GetStream(string strPathFileName, bool readStream)
        {
            //TODO LARA READSTREAM
            if (String.IsNullOrEmpty(strPathFileName))
                return null;

            TBFile pTBFile = GetTBFile(strPathFileName);
            string strTextContent = (pTBFile != null) ? pTBFile.GetContentAsString() : "";
            Debug.Fail(pTBFile.completeFileName);
            if (pTBFile!= null)
                pTBFile = null;
            Debug.Fail("!strTextContent.IsEmpty()");
            StreamReader sr;
            try
            {
                // file content
                sr = new StreamReader(strTextContent, true);
                string fileContent = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
            }
            catch (Exception)
            {
                return null;
            }

            return sr.BaseStream;
        }
        //----------------------------------------------------------------------------
        public TBFile GetTBFile(string strPathFileName)
        {
	        if (string.IsNullOrEmpty(strPathFileName))
		        return null;

	        string strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);
            ArrayList aMetadataArray;

	        string strRelativePath;
	        try
	        {
		        if (pathFinder.IsStandardPath(strTBFSFileName))
		        {
			        strRelativePath = GetRelativePath(strTBFSFileName, false);

                    aMetadataArray = GetStandardTBFileInfo(string.Format(" InstanceKey = \'%s\' AND CompleteFileName = \'%s\'", szInstanceKey, strRelativePath));
		        }
		        else
		        {
			        strRelativePath = GetRelativePath(strTBFSFileName, true);

                    aMetadataArray = GetCustomTBFileInfo(string.Format(" CompleteFileName = \'%s\'", strRelativePath));
		        }		
	        }
	        catch (SqlException e)
	        {

                Debug.Fail(e.Message);
		        return null;
	        }	

	        return (aMetadataArray.Count  > 0) ? ((TBFile)aMetadataArray[0]) : null;
        }
        //----------------------------------------------------------------------------
        public ArrayList GetAllApplicationInfo()
        {
            ArrayList array = new ArrayList();

	        SqlConnection connection = null;
	        SqlCommand	  command = null;
	        SqlDataReader reader = null;

	        String commandText;
	        try
	        {
		        commandText = string.Format("Select PathName FROM {0} WHERE ObjectType = \'APPLICATION\' AND InstanceKey = \'{1}\' ORDER BY FileID", szMPInstanceTBFS, szInstanceKey);

                connection = new SqlConnection(standardConnectionString);
                connection.Open();
                command = new SqlCommand(commandText, connection);
                reader = command.ExecuteReader();
		        while (reader.Read())
			        array.Add(GetAbsolutePath((String)reader["PathName"], false));

		        if (reader != null)
		        {
                    reader.Close();
			        reader.Dispose();
		        }
		        command.Dispose();
                connection.Close();
		        connection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (reader != null)
		        {
                    reader.Close();
                    reader.Dispose();
		        }
		        if (command != null)
			        command.Dispose();

		        if (connection != null)
		        {
			        connection.Close();
			        connection.Dispose();
		        }

		        Debug.Fail(e.Message);
	        }

            return array;
        }

        //----------------------------------------------------------------------------
        public ArrayList GetAllModuleInfo(string strAppName)
        {
            ArrayList pModulesPath = null;

            if (string.IsNullOrEmpty(strAppName))
		        return null;
	
	        if (pModulesPath == null)
		        pModulesPath = new ArrayList();

	        SqlConnection connection = null;
	        SqlCommand	   sqlCommand = null;
	        SqlDataReader reader = null;
	        String commandText;
	        try
	        {
		        commandText = string.Format("Select PathName FROM {0} WHERE ObjectType = \'MODULE\' AND Application = \'{1}\' AND InstanceKey = \'{2}\' ORDER BY FileID", szMPInstanceTBFS, strAppName, szInstanceKey);
                connection = new SqlConnection(standardConnectionString);
                connection.Open();
		        sqlCommand = new SqlCommand(commandText, connection);
                reader = sqlCommand.ExecuteReader();
		        while (reader.Read())
			        pModulesPath.Add(GetAbsolutePath((String)reader["PathName"], false));

		        if (reader != null)
		        {
                    reader.Close();
                    reader.Dispose();
		        }
		        sqlCommand.Dispose();
                connection.Close();
		        connection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (reader != null)
		        {
                    reader.Close();
                    reader.Dispose();
		        }
		        if (sqlCommand != null)
			        sqlCommand.Dispose();

		        if (connection != null)
		        {
                    connection.Close();
			        connection.Dispose();
		        }
		        
                Debug.Fail(e.Message);
	        }

            return pModulesPath;
        }

        //TODO LARA
        //----------------------------------------------------------------------------
        public string GetCustomConnectionString() 
        {
            //stringa di connessione della company chiedi a ilaria
            return customConnectionString;
        //       return (AfxGetLoginInfos()) ? AfxGetLoginInfos().m_strNonProviderCompanyConnectionString : "";
        }


        //----------------------------------------------------------------------------
        public int GetFolder( string strPathName,  bool bCreate)
        {
	        if (string.IsNullOrEmpty(strPathName))
		        return -1;

	        SqlConnection connection = null;

	        string strRelativePath;
	        string strApplication = "", strModule = "", accountName = "";
	        String connectionString;
	        bool isCustom = false;
	        int fileID = -1;
	        try
	        {
		        //effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		        if (pathFinder.IsCustomPath(strPathName))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return -1;			
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(strPathName, isCustom);

                //TODO LARA nn mi ricordo perche' 
		        pathFinder.GetApplicationModuleNameFromPath(strPathName, out strApplication, out strModule);
                        connection = new SqlConnection(connectionString);
                        connection.Open();
		        fileID = InsertMetadataFolder(connection, strRelativePath, strApplication, strModule, isCustom, accountName, bCreate == true);

                        connection.Close();
		        connection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (connection !=  null)
		        {
                    connection.Close();
			        connection.Dispose();
		        }
		        Debug.Fail(e.Message);
		        return -1;
	        }

	        return fileID;
        }

        //----------------------------------------------------------------------------
        public bool CreateFolder( string strPathName,  bool bRecursive)
        {
	        return GetFolder(strPathName, true) != -1;
        }

        //----------------------------------------------------------------------------
        public byte[] GetBinaryFile( string strPathFileName, int nLen)
        {
	        if (String.IsNullOrEmpty(strPathFileName))
		        return null;

	        TBFile tBFile = GetTBFile(strPathFileName);
	        nLen = 0;
	        byte[] pBinaryContent = (tBFile != null) ? tBFile.GetContentAsBinary() : null;
	        Debug.Fail(tBFile.completeFileName);
	        if (tBFile != null)
	        {
		        nLen = (pBinaryContent != null) ? (int) tBFile.fileSize : 0; //TODO LARA CHIEDI A ANNA
                //lo metto a null così il distrutture del TBFile non va a cancellare l'area di memoria di m_pFileContent che è stata assegnata a pBinaryContent
                tBFile.fileContent = null;
                tBFile = null;
	        }
	        Debug.Fail("GetBinaryFile error pBinaryContent");

            return pBinaryContent;
        }


        //----------------------------------------------------------------------------
        public bool SaveBinaryFile( string strPathFileName, byte[] pBinaryContent, int nLen)
        {
	        if (string.IsNullOrEmpty(strPathFileName))
		        return false;

	        string strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);
	        TBFile pTBFile = new TBFile(strTBFSFileName, this);
	        pTBFile.fileContent = pBinaryContent;
	        pTBFile.objectType = GetType(strTBFSFileName);

	        pTBFile.isReadOnly = false;
	        pTBFile.fileSize = nLen;
	        pathFinder.GetApplicationModuleNameFromPath(strTBFSFileName, out pTBFile.appName, out pTBFile.moduleName);
	        if (pTBFile.isCustomPath = pathFinder.IsCustomPath(strTBFSFileName))
		        pTBFile.accountName = pathFinder.GetUserNameFromPath(strTBFSFileName);

	        bool bResult = SaveTBFile(pTBFile, true);
            pTBFile = null;

	        return bResult;
        }

        //----------------------------------------------------------------------------
        public int GetFile( string strPathFileName)
        {
	        if (string.IsNullOrEmpty(strPathFileName))
		        return -1;
	        string strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	        SqlConnection connection = null;
	        SqlCommand command = null;

	        string strRelativePath;
	        String commandText;
	        String connectionString;
	        String tableName;
	        bool isCustom = false;
	        try
	        {
		        //effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		        if (pathFinder.IsCustomPath(strTBFSFileName))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return -1;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(strTBFSFileName, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;
                connection = new SqlConnection(connectionString);
                connection.Open();
		        commandText = string.Format("Select FileID from {0} where CompleteFileName = '{1}'", tableName, strRelativePath);
		        command = new SqlCommand(commandText, connection);
		        System.Object value = command.ExecuteScalar();

		        command.Dispose();
		        connection.Close();
		        connection.Dispose();

		        int nResult;
		        nResult = (value != null) ? (Int32)value : -1;

		        return nResult;
	        }
	        catch (SqlException)
	        {

		        if (command != null)
			        command.Dispose();

		        if (connection != null)
                { 
                    connection.Close();
                    connection.Dispose();
		        }
		        Debug.Assert(false);
		        return -1;
	        }
        }


        //----------------------------------------------------------------------------
        public bool SaveTextFileFromXml(string sFileName, XmlDocument dom)
        {
            if (string.IsNullOrEmpty(sFileName))
                return false;
            string strTBFSFileName = GetTBFSFileCompleteName(sFileName);

            TBFile pTBFile = new TBFile(strTBFSFileName, this);
            pTBFile.fileContentString = dom.InnerXml; //todo lara
            pTBFile.objectType = GetType(strTBFSFileName);

            pTBFile.isReadOnly = false;
            pTBFile.fileSize = dom.InnerXml.Length;
            pathFinder.GetApplicationModuleNameFromPath(strTBFSFileName, out pTBFile.appName, out pTBFile.moduleName);
            if (pTBFile.isCustomPath = pathFinder.IsCustomPath(strTBFSFileName))
                pTBFile.accountName = pathFinder.GetUserNameFromPath(strTBFSFileName);

            bool bResult = SaveTBFile(pTBFile, true);
            pTBFile = null;

            return bResult;
        }

        //----------------------------------------------------------------------------
        public bool SaveTextFileFromStream( string strPathFileName,  Stream fileTextContent)
        {
	        if (string.IsNullOrEmpty(strPathFileName))
		        return false;
	        string strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	        TBFile pTBFile = new TBFile(strTBFSFileName,  this);
            pTBFile.fileContentString = fileTextContent.ToString(); //todo lara
	        pTBFile.objectType = GetType(strTBFSFileName);

	        pTBFile.isReadOnly = false;
	        pTBFile.fileSize = fileTextContent.Length;
	        pathFinder.GetApplicationModuleNameFromPath(strTBFSFileName, out pTBFile.appName, out pTBFile.moduleName);
	        if (pTBFile.isCustomPath = pathFinder.IsCustomPath(strTBFSFileName))
		        pTBFile.accountName = pathFinder.GetUserNameFromPath(strTBFSFileName);
	
	        bool bResult = SaveTBFile(pTBFile, true);
	        pTBFile = null;

	        return bResult;
        }

        //----------------------------------------------------------------------------
        public bool ExistFile( string strPathFileName)
        {
	        return GetFile(strPathFileName) > 0;
        }

        //----------------------------------------------------------------------------
        public bool RemoveFile( string strPathFileName)
        {
	        if (string.IsNullOrEmpty(strPathFileName))
		        return false;
	        string strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	        SqlConnection connection = null;
	        SqlCommand	   sqlCommand = null;

	        string strRelativePath;
	        String commandText;
	        String connectionString;
	        String tableName;
	        bool isCustom = false;
	        try
	        {
		        if (pathFinder.IsCustomPath(strTBFSFileName))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return false;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(strTBFSFileName, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata: szMPInstanceTBFS;

                connection = new SqlConnection(connectionString);
                connection.Open();
		        commandText = string.Format("DELETE {0} WHERE CompleteFileName = '{1}'", tableName, strRelativePath);
		        sqlCommand = new SqlCommand(commandText, connection);
		        int nResult = sqlCommand.ExecuteNonQuery();

		        sqlCommand.Dispose(); 
                connection.Close();
		        connection.Dispose();
		        return nResult == 1;
	        }
	        catch (SqlException)
	        {

                if (sqlCommand != null)
                    sqlCommand.Dispose();

                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose(); ;
		        }
		        Debug.Assert(false);
		        return false;
	        }
        }

        //----------------------------------------------------------------------------
        public bool RenameFile( string strOldFileName,  string strNewFileName)
        {
	        if (string.IsNullOrEmpty(strOldFileName) || string.IsNullOrEmpty(strNewFileName))
		        return false;

	        SqlConnection sqlConnection = null;
	        SqlCommand	   sqlCommand = null;

	        string strOldRelativePath, strNewRelativePath;
	        String commandText;
	        String connectionString;
	        String tableName;
	        bool isCustom = false;
	        strOldRelativePath = GetTBFSFileCompleteName(strOldFileName);
	        strNewRelativePath = GetTBFSFileCompleteName(strNewFileName);
	        try
	        {


		        ///effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		        if (pathFinder.IsCustomPath(strOldRelativePath))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return false;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strOldRelativePath = GetRelativePath(strOldRelativePath, isCustom);
		        strNewRelativePath = GetRelativePath(strNewRelativePath, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;
			

		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        FileInfo file = new FileInfo(strNewFileName);
		
		        commandText = string.Format("UPDATE {0} SET  FileName = {1}, FileType = {2}, CompleteFileName = {3} WHERE CompleteFileName = '{4}'", tableName, file.Name, file.Extension, strNewRelativePath, strOldRelativePath);
		        if (!isCustom)
			        commandText += string.Format(" AND InstanceKey = \'{0}\'", szInstanceKey);

		        sqlCommand = new SqlCommand(commandText, sqlConnection);
		        int nResult = sqlCommand.ExecuteNonQuery();

		         sqlCommand.Dispose(); 
		        sqlConnection.Close();
		         sqlConnection.Dispose();
		        return nResult == 1;
	        }
	        catch (SqlException)
	        {

                if (sqlCommand != null)
                    sqlCommand.Dispose();

                if (sqlConnection != null)
                {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }
		        Debug.Assert(false);
		        return false;
	        }
        }

        //----------------------------------------------------------------------------
        public bool RenameFolder(string sOldFileName, string sNewFileName)
        {
            // TODO x Anna
            return true;
        }

        //todo Lara problema sul settare la size
        //----------------------------------------------------------------------------
            public FileInfo GetFileStatus(string sFileName)
        {
                FileInfo fs = new FileInfo(sFileName);
                 
            //    TBFile pTBFSFile = GetTBFile(sFileName);
            // if (pTBFSFile != null)
            //  return false;
            // fs.LastWriteTime = pTBFSFile.lastWriteTime;
            // fs.CreationTime  = pTBFSFile.creationTime;
            // fs.Attributes. = pTBFSFile.fileSize;
            // lstrcpyn(fs.fullName, pTBFSFile.completeFileName, pTBFSFile.fileSize);

            // if (pTBFSFile.isDirectory)
            //  fs.m_attribute |= 0x10; // Attribute.directory;
            // if (pTBFSFile.isReadOnly)
            //  fs.m_attribute |= 0x01;

            // pTBFSFile;
            return fs;
        }

        //----------------------------------------------------------------------------
        public int[] GetFileAttributes(string sFileName)
        {
            int[] a = null;
            return a;
        }

         //----------------------------------------------------------------------------
         public ArrayList GetTBFilesInfo( string strConnectionString,  string strCommandText, bool bFromCustom)
         {
            ArrayList pArray = new ArrayList();
            SqlConnection sqlConnection = null;
	        SqlCommand	   sqlCommand = null;
	        SqlDataReader dr = null;
	        TBFile pMetadataFile = null;
	        try
	        {
		        sqlConnection = new SqlConnection(strConnectionString);
		        sqlConnection.Open();
		        sqlCommand = new SqlCommand(strCommandText, sqlConnection);

		        dr = sqlCommand.ExecuteReader();
		        while (dr.Read())
		        {
			        pMetadataFile = new TBFile(((String)dr["FileName"]), GetAbsolutePath((String)dr["PathName"], bFromCustom), this);
			        pMetadataFile.fileID = (Int32)dr["FileID"];
			        pMetadataFile.parentID = (Int32)dr["ParentID"];
			        pMetadataFile.fileNamespace = (String)dr["Namespace"];
			        pMetadataFile.appName = (String)dr["Application"];
			        pMetadataFile.moduleName = (String)dr["Module"];
			        pMetadataFile.fileSize = (Int32)dr["FileSize"];	
			        pMetadataFile.isReadOnly = ((String)dr["IsReadOnly"] == "1");
			        pMetadataFile.isDirectory = ((String)dr["IsDirectory"] == "1");
			        DateTime aDT = (DateTime)dr["CreationTime"];
			        pMetadataFile.creationTime = new DateTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			        aDT = (DateTime)dr["LastWriteTime"];
			        pMetadataFile.lastWriteTime = new  DateTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			        //se sono nella custom devo valorizzare anche i seguenti campi
			        if (bFromCustom)
				        pMetadataFile.accountName = (String)dr["AccountName"];

			        pMetadataFile.isCustomPath = bFromCustom;
			        if (dr["FileContent"] == DBNull.Value || pMetadataFile.fileSize <= 0)
				        pMetadataFile.fileContent = null;
			        else
			        {
				        byte[] byteData = ((byte[])dr["FileContent"]);
				    }

			        if (dr["FileTextContent"] != DBNull.Value)
				        pMetadataFile.fileContentString = dr["FileTextContent"].ToString();
			
			        pArray.Add(pMetadataFile);
		        }
		        if (dr != null)
		        {
			        dr.Close();
			         dr.Dispose();
		        }
		        if (sqlCommand != null)
			         sqlCommand.Dispose();

		        if (sqlConnection != null)
		        {
			        sqlConnection.Close();
			         sqlConnection.Dispose();
		        }

                return pArray;

            }
	        catch (SqlException e)
	        {
		        if (dr != null)
		        {
			        dr.Close();
			         dr.Dispose();
		        }
		        if (sqlCommand != null)
			         sqlCommand.Dispose();

		        if (sqlConnection != null)
		        {
			        sqlConnection.Close();
			         sqlConnection.Dispose();
		        }

		        throw(e);
	        }
        }

        //----------------------------------------------------------------------------
        public ArrayList GetStandardTBFileInfo( string whereClause)
        {
            ArrayList pArray = new ArrayList();

            if (pArray == null || string.IsNullOrEmpty(whereClause))
		        return null;
	        try
	        {
		        string strCommandText = string.Format("Select * from %s where %s", szMPInstanceTBFS, whereClause);
                pArray = GetTBFilesInfo(standardConnectionString, strCommandText, false);
	        }
	        catch (SqlException e)
	        {
		        throw(e);
	        }

            return pArray;
        }

        //----------------------------------------------------------------------------
        public ArrayList GetCustomTBFileInfo( string whereClause)
        {
            ArrayList pArray = new ArrayList();

            string strCustConnectionString = GetCustomConnectionString();

	        if (pArray == null || string.IsNullOrEmpty(whereClause) || string.IsNullOrEmpty(strCustConnectionString))
		        return null;
	        try
	        {
		        string strCommandText = String.Format("Select * from %s where %s", szTBCustomMetadata, whereClause);
                pArray = GetTBFilesInfo(strCustConnectionString, strCommandText,  true);
	        }
	        catch (SqlException e)
	        {
		        throw(e);
	        }

            return pArray;
        }

        //----------------------------------------------------------------------------
        public bool SaveTBFile(TBFile pTBFile,  bool bOverWrite)
        {
	        if (string.IsNullOrEmpty(pTBFile.completeFileName))
		        return false;

		        //esiste allora lo devo solo modificare FileContent, LastWriteTime, FileSize, TBModified e TBModifiedID

	        SqlConnection sqlConnection = null;
	        SqlCommand	   sqlCommand = null;

	        String connectionString;
	        String tableName;	
	        string strRelativePath;
	        string strApplication = "" , strModule = "", strAccountName = "";
	        bool isCustom = false;
	        int parentID = -1;

	        //verifico se il file esiste
	        int fileID = GetFile(pTBFile.completeFileName);
	        if (fileID != -1 && !bOverWrite)
		        return true;


	        String strCommandText;
	        try
	        {
		        if (pathFinder.IsCustomPath(pTBFile.completeFileName))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return false;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(pTBFile.completeFileName, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;
		

		        strCommandText = (fileID > -1)
			        ? string.Format("UPDATE {0} SET LastWriteTime = GetDate(), FileSize = @FileSize, FileContent = @BinaryContent, FileTextContent = @FileTextContent, TBModified = GetDate(), TBModifiedID = @TBModifiedID WHERE FileID = @FileID", tableName)
			        : (isCustom)
			        ? string.Format("INSERT INTO {0} (ParentID, PathName, FileName, CompleteFileName, FileType, FileSize, Application, Module, ObjectType,IsDirectory,IsReadOnly,FileContent,FileTextContent, AccountName, TbCreatedID, TBModifiedID )  VALUES ( @ParentID, @PathName, @FileName, @CompleteFileName, @FileType, @FileSize, @Application, @Module, @ObjectType, @IsDirectory, @IsReadOnly, @BinaryContent, @FileTextContent, @AccountName, @TbCreatedID, @TBModifiedID)", tableName)
			        : string.Format("INSERT INTO {0} (ParentID, PathName, FileName, CompleteFileName, FileType, FileSize, Application, Module, ObjectType,IsDirectory,IsReadOnly,FileContent,FileTextContent, InstanceKey, TbCreatedID, TBModifiedID )  VALUES ( @ParentID, @PathName, @FileName, @CompleteFileName, @FileType, @FileSize, @Application, @Module, @ObjectType, @IsDirectory, @IsReadOnly ,@BinaryContent,  @FileTextContent, @InstanceKey @TbCreatedID, @TBModifiedID)", tableName);

		        //se non ho ancora inserito il file mi devo far dare il parentID e se non esiste inserirlo
		        if (fileID == -1)
		        {
			        if (IsARootPath(pTBFile.completeFileName))
				        parentID = -1;
			        parentID = GetFolder(pTBFile.completeFileName, true);
			        pathFinder.GetApplicationModuleNameFromPath(pTBFile.completeFileName, out strApplication, out strModule);
			        strAccountName = pathFinder.GetUserNameFromPath(pTBFile.completeFileName);
		        }
		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        sqlCommand = new SqlCommand(strCommandText, sqlConnection);
		        //aggiungo i parametri
		        //prima quelli comuni tra update ed insert
		        sqlCommand.Parameters.AddWithValue("@FileSize", (Int32)pTBFile.fileSize);
		        sqlCommand.Parameters.AddWithValue("@FileTextContent", pTBFile.fileContent);
		        SqlParameter binaryContentParam = new SqlParameter("@BinaryContent", SqlDbType.VarBinary);
		        if (pTBFile.fileContent != null)
		        {
			        byte[] arr = new byte[pTBFile.fileSize];
			        binaryContentParam.Value = arr;
		        }
		        else
			        binaryContentParam.Value = DBNull.Value;

		        sqlCommand.Parameters.Add(binaryContentParam);
		        sqlCommand.Parameters.AddWithValue("@TBModifiedID", GetWorkerId());

		        //se sto aggiungendo una nuova riga allora devo fare il bind anche degli altri parametri
		        if (fileID <= 0)
		        {
			        String relativePath = strRelativePath;
			        SqlParameter parentParam = new SqlParameter("@ParentID", SqlDbType.Int);
			        if (parentID > -1)
				        parentParam.Value = parentID;
			        else
				        parentParam.Value = DBNull.Value;
			        sqlCommand.Parameters.Add(parentParam);

			        sqlCommand.Parameters.AddWithValue("@PathName", relativePath);
			        sqlCommand.Parameters.AddWithValue("@FileName", pTBFile.name);
			        sqlCommand.Parameters.AddWithValue("@CompleteFileName", relativePath);
			        sqlCommand.Parameters.AddWithValue("@FileType", pTBFile.FileExtension);
			        sqlCommand.Parameters.AddWithValue("@Application", strApplication);
			        sqlCommand.Parameters.AddWithValue("@Module", strModule);
			        sqlCommand.Parameters.AddWithValue("@ObjectType", pTBFile.objectType);
			        sqlCommand.Parameters.AddWithValue("@IsDirectory", (pTBFile.isDirectory) ? "1" : "0");
			        sqlCommand.Parameters.AddWithValue("@IsReadOnly", (pTBFile.isReadOnly) ? "1" : "0");
			        sqlCommand.Parameters.AddWithValue("@TbCreatedID", (Int32)GetWorkerId());
			        if (isCustom)
				        sqlCommand.Parameters.AddWithValue("@AccountName", strAccountName);
			        else
				        sqlCommand.Parameters.AddWithValue("@InstanceKey", szInstanceKey);
		        }
		        else //parametro della where clause nel caso di update
			        sqlCommand.Parameters.AddWithValue("@FileID", (Int32)fileID);

		        sqlCommand.ExecuteNonQuery();
		         sqlCommand.Dispose();
	        }

	        catch (SqlException e)
	        {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
                if (sqlConnection != null)
                {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }

                Debug.Fail(e.Message);
		        return false;
	        }

	        return true;

        }

        //----------------------------------------------------------------------------
        public bool CopyTBFile(TBFile pTBOldFileInfo,  string strNewName,  bool bOverWrite)
        {
	        if (pTBOldFileInfo == null || string.IsNullOrEmpty(strNewName))
		        return false;

	        if (string.Compare(pTBOldFileInfo.completeFileName, strNewName, true) == 0)
		        return true;

            FileInfo file = new FileInfo(strNewName);
            string strNewPath = file.Directory.FullName;
	        //le path sono diverse
	        if (string.Compare(strNewPath, pTBOldFileInfo.PathName, true) != 0)
	        {
		        pathFinder.GetApplicationModuleNameFromPath(strNewPath, out pTBOldFileInfo.appName, out pTBOldFileInfo.moduleName);
		        if (pTBOldFileInfo.isCustomPath = pathFinder.IsCustomPath(strNewPath))
			        pTBOldFileInfo.accountName = pathFinder.GetUserNameFromPath(strNewPath);
	        }

	        pTBOldFileInfo.completeFileName = GetTBFSFileCompleteName(strNewName);
	        pTBOldFileInfo.name = file.Name;
	        pTBOldFileInfo.FileExtension = file.Extension;

	        return SaveTBFile(pTBOldFileInfo, bOverWrite);

        }

        //----------------------------------------------------------------------------
        public bool CopySingleFile( string strOldFileName,  string strNewName,  bool bOverWrite)
        {
	        if (string.Compare(strOldFileName, strNewName, true) == 0)
		        return true;
	        if (string.IsNullOrEmpty(strOldFileName) || string.IsNullOrEmpty(strNewName))
		        return false;

	        TBFile pTBOldFileInfo = GetTBFile(strOldFileName);
	        if (pTBOldFileInfo == null)
		        return false;

	        return CopyTBFile(pTBOldFileInfo, strNewName, bOverWrite);
        }

        //----------------------------------------------------------------------------
        public bool ExistPath( string strPathName)
        {
	        if (IsARootPath(strPathName))
		        return true;

	        return GetFolder(strPathName, false) != 0;
        }

        //----------------------------------------------------------------------------
        public bool RemoveParentFolders(SqlConnection sqlConnection, String tableName, Int32 fileID)
        {
	        SqlCommand sqlCommand = null;
		        try
	        {
		        // verifico se il folder è vuoto o meno
		        sqlCommand = new SqlCommand(string.Format("SELECT FileID FROM {0} WHERE ParentID = {2}", tableName, fileID.ToString()), sqlConnection);
		        Object value = sqlCommand.ExecuteScalar();
		        //ci sono dei file o altri subfolder. Non la posso cancellare
		        if (value != null && (Int32)value > 0)
		        {
			         sqlCommand.Dispose();
			        return true;
		        }
		        sqlCommand.CommandText = string.Format("SELECT ParentID FROM {0} WHERE FileID = {1}", tableName, fileID.ToString());
		        value = sqlCommand.ExecuteScalar();		
		        //cancello il folder e poi vado in ricorsione sul parent se esiste			
		        //se non ho parent vuol dire che sono arrivata alla root
		        sqlCommand.CommandText = string.Format("DELETE FROM {0} WHERE FileID = {1}", tableName, fileID.ToString());
		        sqlCommand.ExecuteNonQuery();
		         sqlCommand.Dispose();
		        if (value != null && (Int32)value > 0)
			        return RemoveParentFolders(sqlConnection, tableName, (Int32)value);

		        return true;
	        }
	        catch (SqlException e)
	        {
                if (sqlCommand != null)
                    sqlCommand.Dispose();

		        throw(e);
	        }
            
        }

        //----------------------------------------------------------------------------
        public bool RemoveRecursiveFolder(SqlConnection sqlConnection, String tableName, Int32 parentID)
        {
	        SqlCommand sqlCommand = null;
	        SqlDataAdapter ad = null;
	        try
	        {	
		        ad = new SqlDataAdapter(string.Format("SELECT FileID FROM {0} WHERE ParentID = {1} and IsDirectory = '1'", tableName, parentID.ToString()), sqlConnection);
		        DataTable filesDT = new DataTable();
		        ad.Fill(filesDT);
		        foreach (DataRow row in filesDT.Rows)
		        {
			        int nFolderID = (Int32)row[0];
			        for (int i = 0; i < filesDT.Rows.Count; i++)
			        {
				        RemoveRecursiveFolder(sqlConnection, tableName, nFolderID);
				        sqlCommand = new SqlCommand(string.Format("DELETE FROM {0} WHERE ParentID = {1}", tableName, parentID.ToString()), sqlConnection);
				        sqlCommand.ExecuteNonQuery();
				        sqlCommand.Dispose();
			        }
		        }
		        return true;
	        }
	        catch (SqlException e)
	        {
		        if (ad != null)
			        ad.Dispose();
                if (sqlCommand != null)
                    sqlCommand.Dispose();

		        throw(e);
	        }

        }


        //----------------------------------------------------------------------------
        public void RemoveFolder( string strPathName,  bool bRecursive,  bool bRemoveRoot,  bool bAndEmptyParents /*= false*/)
        {
            if (string.IsNullOrEmpty(strPathName))
                return;

	        string strTBFSFolder = GetTBFSFileCompleteName(strPathName);
	        if (IsARootPath(strTBFSFolder))
		        return;


	        SqlConnection sqlConnection = null;
	        SqlCommand   sqlCommand = null;
	        Object value = null;

	        string strRelativePath;
	        String connectionString;
	        bool isCustom = false;
	        int fileID = 0;
	        int parentID = -1;
	        try
	        {
		
		        //effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		        if (pathFinder.IsCustomPath(strTBFSFolder))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return;
			        isCustom = true;
		        }

		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(strTBFSFolder, isCustom);
		        String tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;


		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        fileID = GetFolder(strTBFSFolder, false);
		        if (bRecursive)
			        RemoveRecursiveFolder(sqlConnection, tableName, fileID);

		        parentID = 0;
		        if (bAndEmptyParents)
		        {
			        sqlCommand = new SqlCommand(string.Format("SELECT ParentID FROM {0} WHERE FileID = {1}", tableName, fileID.ToString()), sqlConnection);
			        value = sqlCommand.ExecuteScalar();
			        parentID = (value != null) ? (Int32)value : -1;
		        }
		
		        if (bRemoveRoot)
		        {
			        if (sqlCommand != null)
				        new SqlCommand("",sqlConnection);
			        sqlCommand.CommandText = string.Format("DELETE FROM {0} WHERE fileID = {1}", tableName, fileID.ToString());
			        sqlCommand.ExecuteNonQuery();			
		        }

		        if (sqlCommand != null)
			        sqlCommand.Dispose();

		        if (bRemoveRoot && bAndEmptyParents)
			        RemoveParentFolders(sqlConnection, tableName, fileID);

		        sqlConnection.Close();
		        sqlConnection.Dispose();
	        }
	        catch (SqlException e)
	        {
                if (sqlCommand != null)
                    sqlCommand.Dispose();

		        if (sqlConnection != null)
		        {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }

                Debug.Fail(e.Message);
		        return;
	        }

        }

        //----------------------------------------------------------------------------
        public bool CopyFolder( string sOldPathName,  string sNewPathName,  bool bOverwrite,  bool bRecursive)
        {
	        string strSourcePath, strTargetPath;
	        strSourcePath = GetTBFSFileCompleteName(sOldPathName);
	        strTargetPath = GetTBFSFileCompleteName(sNewPathName);

	        if (sOldPathName.Substring(sOldPathName.Length - 1) == "/" || sOldPathName.Substring(sOldPathName.Length - 1) == "\\")
		        strSourcePath = sOldPathName.Left(sOldPathName.Length - 1);
	        else
		        strSourcePath = sOldPathName;

            if (sOldPathName.Substring(sOldPathName.Length - 1) == "/" || sOldPathName.Substring(sOldPathName.Length - 1) == "\\")
                strTargetPath = sNewPathName.Left(sNewPathName.Length - 1);
	        else
		        strTargetPath = sNewPathName;

	        try
	        {

		        int sourceFolderID = GetFolder(strSourcePath, false);
		        int targetFolderID = GetFolder(strTargetPath, false);
		        //se esiste il folder sorgente 
		        if (sourceFolderID == -1)
			        return false;

		        //se esiste il folder di destinazione e non posso sovvrascriverlo
		        if (!bOverwrite && targetFolderID != -1)
			        return false;

		        if (targetFolderID == -1)
		        {
			        //creo il foldel di destinazione
			        targetFolderID = GetFolder(strTargetPath, true);
			        if (targetFolderID == -1)
				        return false;
		        }

		        //vado in ricorsione sul contenuto 
		        ArrayList pTBFiles = new ArrayList();
		        TBFile pTBFile = null;
		        string strFolderName;
                pTBFiles = GetTBFolderContent(strSourcePath, true, true, "");
		        for (int i = 0; i < pTBFiles.Count; i++)
		        {
			        pTBFile = (TBFile)pTBFiles[i];

			        if (pTBFile.isDirectory)
			        {
				        int nPos = pTBFile.PathName.LastIndexOf(@"\\");
				        strFolderName = pTBFile.PathName.Right(pTBFile.PathName.Length - (nPos + 1));

				        if (!CopyFolder(pTBFile.PathName, strTargetPath + @"\\" + strFolderName, bOverwrite, bRecursive))
					        return false;
			        }
			        else
			        {
				        if (!CopyTBFile(pTBFile, strTargetPath + @"\\" + pTBFile.name, bOverwrite))
					        return false;
			        }
		        }
	        }
	        catch (SqlException e)
	        {
                        Debug.Fail(e.Message);
		        return false;
	        }
	        return true;
        }

        //----------------------------------------------------------------------------
        public List<TBDirectoryInfo> GetSubFolders( string strPathName)
        {
            List<TBDirectoryInfo> subfolders = new List<TBDirectoryInfo>();

            if (string.IsNullOrEmpty(strPathName))
		        return null;

	        string strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	        SqlConnection sqlConnection = null;
	        SqlCommand   sqlCommand = null;
	        SqlDataReader dr = null;

	        String relativePath;
	        String connectionString;
	        String commandText;
	        String tableName;
	        bool isCustom = false;
	        try
	        {
		        if (pathFinder.IsCustomPath(strTBFSFolder))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return null;

			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        relativePath = GetRelativePath(strTBFSFolder, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;

		        commandText = string.Format("Select X.PathName from {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\' AND X.IsDirectory = \'1\'", tableName, relativePath);
		        //devo aggiungere il filtro per l'instance name
		        if (!isCustom)
			        commandText += string.Format(" AND X.InstanceKey = \'{0}\'", szInstanceKey);

		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        sqlCommand = new SqlCommand(commandText, sqlConnection);
		        dr = sqlCommand.ExecuteReader();
		        while (dr.Read())
                    subfolders.Add(new  TBDirectoryInfo(GetAbsolutePath((String)dr["PathName"], isCustom), this));
		
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
		        sqlCommand.Dispose();
		        sqlConnection.Close();
		        sqlConnection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
                if (sqlCommand != null)
                    sqlCommand.Dispose();

                if (sqlConnection != null)
                {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }

		        Debug.Fail(e.Message);
		        return null;
	        }
	        return subfolders;
        }

        //----------------------------------------------------------------------------
        public ArrayList GetTBFolderContent( string strPathName, bool bFolders, bool bFiles,  string strFileExt)
        {
            if (string.IsNullOrEmpty(strPathName))
		        return null;

	        string strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	        string strConnectionString;
	        string strCommandText;
	        string strTableName;
	        string strRelativePath;
	        bool isCustom = false;
	        try
	        {
		        if (pathFinder.IsCustomPath(strTBFSFolder))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return null;
			        isCustom = true;
		        }
		        strConnectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        strRelativePath = GetRelativePath(strTBFSFolder, isCustom);
		        strTableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;

		        strCommandText = string.Format("Select X.* FROM {%s} X,  {%s} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{%s}\''", strTableName, strRelativePath);

		        if (bFiles && (!string.IsNullOrEmpty(strFileExt) || string.Compare(strFileExt, "*.*", true) != 0))
		        {
			        string fileType = (!strFileExt.Contains("*")) ? strFileExt.Right(strFileExt.Length - 1) : strFileExt;
			        strCommandText += (bFolders)
				        ? string.Format(" AND (X.IsDirectory = \'0\' OR (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))", fileType)
				        : string.Format(" AND (X.IsDirectory = \'1\' AND X.FileType = \'{0}\')", fileType);
		        }
		        else
			        if (bFolders || bFiles)
				        strCommandText += string.Format(" AND X.IsDirectory = \'%s\'", (bFolders) ? '0' : '1');

		        if (!isCustom)
			        strCommandText += string.Format(" AND Y.InstanceKey = \'%s\'", szInstanceKey);

                //TODO LARA
                ArrayList pArray = GetTBFilesInfo(standardConnectionString, strCommandText, false);

                return pArray;
            }
	        catch (SqlException e)
	        {
		        Debug.Fail(e.Message);
		        return null;
	        }	
	        
        }


        //----------------------------------------------------------------------------
        public bool GetPathContent( string strPathName, bool bFolders, out ArrayList pSubFolders, bool bFiles,  string strFileExt, out ArrayList pFiles)
        {
            pSubFolders = new ArrayList();
            pFiles = new ArrayList();

            if (string.IsNullOrEmpty(strPathName))
		        return true;
	        string strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	        SqlConnection sqlConnection = null;
	        SqlCommand   sqlCommand = null;
	        SqlDataReader dr = null;

	        String relativePath;
	        String connectionString;
	        String commandText;
	        String tableName;
	        bool isCustom = false;
	        try
	        {
		        if (pathFinder.IsCustomPath(strTBFSFolder))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return false;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        relativePath = GetRelativePath(strTBFSFolder, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;

		        commandText = string.Format("Select X.CompleteFileName, X.PathName, X.IsDirectory FROM {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\''", tableName, relativePath);

		        if (bFiles && (!string.IsNullOrEmpty(strFileExt) || string.Compare(strFileExt, "*.*" , true) != 0))
		        {
			        string fileType = (!strFileExt.Contains('*')) ? strFileExt.Right(strFileExt.Length - 1) : strFileExt;
			        commandText += (bFolders)
				        ? string.Format(" AND (X.IsDirectory = \'0\' OR (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))", fileType)
				        : string.Format(" AND (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))", fileType);
		        }
		        else
			        if (bFolders || bFiles)
				        commandText += string.Format(" AND X.IsDirectory = \'{0}\'))", (bFolders) ? '0' : '1');
		
		        if (!isCustom)
			        commandText += string.Format(" AND Y.InstanceKey =\'{0}\'", szInstanceKey);

		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        sqlCommand = new SqlCommand(commandText, sqlConnection);
		        dr = sqlCommand.ExecuteReader();
		        while (dr.Read())
		        {
			        if ((String)dr["IsDirectory"] == "1")
				        pSubFolders.Add(GetAbsolutePath((String)dr["PathName"], isCustom));
			        else
				        pFiles.Add(GetAbsolutePath((String)dr["CompleteFileName"], isCustom));
		        }
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
		        sqlCommand.Dispose();
		        sqlConnection.Close();
		        sqlConnection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
                if (sqlCommand != null)
                    sqlCommand.Dispose();

                if (sqlConnection != null)
                {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }
		        Debug.Fail(e.Message);
		        return false;
	        }
	        return true;
        }

        //----------------------------------------------------------------------------
        public List<TBFile> GetFiles( string strPathName,  string strFileExt)
        {
            List<TBFile> pFiles = new List<TBFile>();

            if (pFiles == null || string.IsNullOrEmpty(strPathName))
		        return null;
	        string strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	        SqlConnection sqlConnection = null;
	        SqlCommand   sqlCommand = null;
	        SqlDataReader dr = null;

	        String relativePath;
	        String connectionString;
	        String commandText;
	        String tableName;
	        bool isCustom = false;
	        try
	        {
		        if (pathFinder.IsCustomPath(strTBFSFolder))
		        {
			        if (string.IsNullOrEmpty(GetCustomConnectionString()))
				        return null;
			        isCustom = true;
		        }
		        connectionString = (isCustom) ? GetCustomConnectionString() : standardConnectionString;
		        relativePath = GetRelativePath(strTBFSFolder, isCustom);
		        tableName = (isCustom) ? szTBCustomMetadata : szMPInstanceTBFS;

		        commandText = string.Format("Select X.CompleteFileName from {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\' AND X.IsDirectory = \'0\'", tableName, relativePath);

		        if (!string.IsNullOrEmpty(strFileExt) || string.Compare(strFileExt,"*.*", true) != 0)
		        {
			        string fileType = (!strFileExt.Contains('*') ) ? strFileExt.Right(strFileExt.Length - 1) : strFileExt;			
			        commandText += string.Format(" AND X.FileType = \'{0}\'", fileType);
		        }

		        if (!isCustom)
			        commandText += string.Format(" AND Y.InstanceKey =\'{0}\'", szInstanceKey);

		        sqlConnection = new SqlConnection(connectionString);
		        sqlConnection.Open();
		        sqlCommand = new SqlCommand(commandText, sqlConnection);
		        dr = sqlCommand.ExecuteReader();
                while (dr.Read())
                {
                    //TODO LARA istanzia i tbfile e popola le property
                    pFiles.Add(new TBFile((String)dr["CompleteFileName"], this));
                }
			       // pFiles.Add(GetAbsolutePath((String)dr["CompleteFileName"], isCustom));
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
		        sqlCommand.Dispose();
		        sqlConnection.Close();
		        sqlConnection.Dispose();
	        }
	        catch (SqlException e)
	        {
		        if (dr != null)
		        {
			        dr.Close();
			        dr.Dispose();
		        }
		        if (sqlCommand != null)
			        sqlCommand.Dispose();

		        if (sqlConnection != null)
		        {
			        sqlConnection.Close();
			        sqlConnection.Dispose();
		        }
		        Debug.Fail(e.Message);
		        return null;
	        }
            return pFiles;
        }

      

        //---------------------------------------------------------------------
        public bool Start()
        {
            return started;
        }

        //----------------------------------------------------------------------
        public bool Stop()
        {
            return started;
        }

        //-----------------------------------------------------------------------------
        public bool IsStarted()
        {
            return started;
        }
    }
}