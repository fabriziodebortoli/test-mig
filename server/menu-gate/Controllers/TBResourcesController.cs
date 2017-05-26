﻿
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using System.Collections;
using Microarea.Common.Applications;
using Microarea.Common;
using System;

namespace Microarea.Menu.Controllers
{
    public enum ObjectType { Report, Image, Document };

    [Route("FileSystemMonitor")]
    //==============================================================================
    public class TBResourcesController : Controller
    {

        private NameSpace nameSpace;

        #region Riky
        //--------------------------------------------------------------------------
        private IList GetApplications()
        {
            IList apps=  BasePathFinder.BasePathFinderInstance.ApplicationInfos;
            return apps; 
        }

        private bool GetCurrentCompanyUser(out string company, out string user)
        {
            company = string.Empty;
            user = string.Empty;

            string sAuthT = HttpContext.Request.Cookies[UserInfo.AuthenticationTokenKey];
            if (string.IsNullOrEmpty(sAuthT))
                return false; //  StatusCode = 504, Content = "non sei autenticato!" 

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT);

            company = loginInfo.companyName;
            user = loginInfo.userName;
            return true;
        }

        #endregion Riky

        #region vecchio codice
        [Route("get-applications")]
        //--------------------------------------------------------------------------
        public IActionResult GetApplicationsJson()
        {
            IList applications = GetApplications();

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Applications");

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Application");

            jsonWriter.WriteStartArray();
            
            foreach (BaseApplicationInfo item in applications)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(item.Name);
                jsonWriter.WritePropertyName("path");
                jsonWriter.WriteValue(item.Path);
                jsonWriter.WriteEndObject();
            }
            jsonWriter.WriteEndArray();

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("get-folders/{applicationPath}")]
        //--------------------------------------------------------------------------
        public IActionResult GetFolders(string applicationPath)
        {  
            if (string.IsNullOrEmpty(applicationPath))
                return null;
            
            string app = HttpContext.Request.Query["applicationPath"];
            DirectoryInfo currentAppDir = new DirectoryInfo(app);
            IList folders = currentAppDir.GetDirectories();
            
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Folders");

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Folder");

            jsonWriter.WriteStartArray();

            foreach (DirectoryInfo item in folders)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(item.Name);
                jsonWriter.WritePropertyName("path");
                jsonWriter.WriteValue(item.FullName);
                jsonWriter.WriteEndObject();
            }
            jsonWriter.WriteEndArray();

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("get-folderFiles/{folderPath}")]
        //--------------------------------------------------------------------------
        public IActionResult GetFolderFiles(string folderPath)
        {
            //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
            if (string.IsNullOrEmpty(folderPath))
                return null;

            string folder = HttpContext.Request.Query["folderPath"];
            folder = Path.Combine(folder, "Report");
            DirectoryInfo currentFolderDir = new DirectoryInfo(folder);

            if (!currentFolderDir.Exists)
                return null;

            IList files = currentFolderDir.GetFiles("*.wrm");

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Files");

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("File");

            jsonWriter.WriteStartArray();

            foreach (FileInfo item in files)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(item.Name);
                jsonWriter.WritePropertyName("path");
                jsonWriter.WriteValue(item.FullName);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        #endregion vecchio codice
        //[Route("getdata/{namespace}/{selectiontype}")]
        //public IActionResult GetData(string nameSpace, string selectionType)
        [Route("Init/{authenticationToken}")]
        //-----------------------------------------------------------------------
        public bool Init(string authenticationToken)
        {
            //    if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
            //        return false;

            //    return FileSystemMonitor.Engine.Init();
            return true;
        }

        [Route("IsAlive")]
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            return true;
        }
        [Route("StartMonitor/{authenticationToken}")]
        //-----------------------------------------------------------------------
        public bool StartMonitor(string authenticationToken)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.StartMonitor();
        }

        [Route("StopMonitor/{authenticationToken}")]
        //-----------------------------------------------------------------------
        public bool StopMonitor(string authenticationToken)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.StopMonitor();
        }

        [Route("GetTbCacheFile/{fileContent}")]
        //-------------------------------------------------------------------------
        public bool GetTbCacheFile(out string fileContent)
        {
            fileContent = string.Empty;
            return FileSystemMonitor.Engine.GetTbCacheFile(out fileContent);
        }

        [Route("CreateTbCacheFilefileContent/{authenticationToken}")]
        //-----------------------------------------------------------------------
        public bool CreateTbCacheFile(string authenticationToken)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.CreateTbCacheFile();
        }

        [Route("GetServerConnectionConfig/{fileContent}")]
        //-------------------------------------------------------------------------
        public bool GetServerConnectionConfig(out string fileContent)
        {
            fileContent = string.Empty;
            return FileSystemMonitor.Engine.GetServerConnectionConfig(out fileContent);
        }


        [Route("SetTextFile/{authenticationToken}/{fileName}/{fileContent}")]
        //-----------------------------------------------------------------------
        public bool SetTextFile(string authenticationToken, string fileName, string fileContent)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.SetTextFile(fileName, fileContent);
        }

        [Route("RemoveFolder/{authenticationToken}/{pathName}/{recursive}/{emptyOnly}")]
        //-----------------------------------------------------------------------
        public bool RemoveFolder(string authenticationToken, string pathName, bool recursive, bool emptyOnly)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.RemoveFolder(pathName, recursive, emptyOnly);
        }

        [Route("CreateFolder/{authenticationToken}/{pathName}/{recursive}")]
        //-----------------------------------------------------------------------
        public bool CreateFolder(string authenticationToken, string pathName, bool recursive)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.CreateFolder(pathName, recursive);
        }

        [Route("CopyFolder/{authenticationToken}/{oldPathName}/{newPathName}/{recursive}")]
        //-----------------------------------------------------------------------
        public bool CopyFolder(string authenticationToken, string oldPathName, string newPathName, bool recursive)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.CopyFolder(oldPathName, newPathName, recursive);
        }

        [Route("CopyFile/{authenticationToken}/{oldPathName}/{newPathName}/{recursive}")]
        //-----------------------------------------------------------------------
        public bool CopyFile(string authenticationToken, string oldPathName, string newPathName, bool overwrite)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.CopyFile(oldPathName, newPathName, overwrite);
        }

        [Route("RemoveFile/{authenticationToken}/{fileName}")]
        //-----------------------------------------------------------------------
        public bool RemoveFile(string authenticationToken, string fileName)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.RemoveFile(fileName);
        }

        [Route("RenameFile/{authenticationToken}/{oldFileName}/{newFileName}")]
        //-----------------------------------------------------------------------
        public bool RenameFile(string authenticationToken, string oldFileName, string newFileName)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.RenameFile(oldFileName, newFileName);
        }
        //TODO LARA
        [Route("GetFileStatus/{authenticationToken}/{fileName}")]
        //-----------------------------------------------------------------------
      //  public bool GetFileStatus(
      //                                      string authenticationToken,
      //                                      string fileName,
      //                                      out DateTime creation,
      //                                      out DateTime lastAccess,
     //                                       out DateTime lastWrite,
      //                                      out long length
      //                                 )
         public  IActionResult GetFileStatus(string authenticationToken, string fileName)
         {
            DateTime creation = DateTime.MinValue;
            DateTime lastAccess = DateTime.MinValue;
            DateTime lastWrite = DateTime.MinValue;
            long length = 0;

            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return null;

            FileSystemMonitor.Engine.GetFileStatus(fileName, out creation, out lastAccess, out lastWrite, out length );

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("creation");
            jsonWriter.WriteValue(creation);

            jsonWriter.WritePropertyName("lastAccess");
            jsonWriter.WriteValue(lastAccess);

            jsonWriter.WritePropertyName("lastWrite");
            jsonWriter.WriteValue(lastWrite);

            jsonWriter.WritePropertyName("length");
            jsonWriter.WriteValue(length);

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("GetFileAttributes/{authenticationToken}/{fileName}")]
        //-----------------------------------------------------------------------
        public int GetFileAttributes(string authenticationToken, string fileName)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return 0;

            return FileSystemMonitor.Engine.GetFileAttributes(fileName);
        }
        //todo lara
        [Route("GetTextFile/{authenticationToken}/{fileName}")]
        //-----------------------------------------------------------------------
        //out string fileContent
        public IActionResult GetTextFile(string authenticationToken, string fileName)
        {
            string fileContent = string.Empty;

            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return null;

           FileSystemMonitor.Engine.GetTextFile(fileName, out fileContent);
           StringBuilder sb = new StringBuilder();
           StringWriter sw = new StringWriter(sb);
           JsonWriter jsonWriter = new JsonTextWriter(sw);
           jsonWriter.Formatting = Formatting.Indented;

           jsonWriter.WriteStartObject();

           jsonWriter.WritePropertyName("fileContent");
           jsonWriter.WriteValue(fileContent);

           jsonWriter.WriteEndObject();
           jsonWriter.WriteEndObject();

           string s = sb.ToString();
           return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("ExistFile/{authenticationToken}/{fileName}")]
        //-----------------------------------------------------------------------
        public bool ExistFile(string authenticationToken, string fileName)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.ExistFile(fileName);
        }

        [Route("ExistPath/{authenticationToken}/{pathName}")]
        //-----------------------------------------------------------------------
        public bool ExistPath(string authenticationToken, string pathName)
        {
            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return false;

            return FileSystemMonitor.Engine.ExistPath(pathName);
        }
        //todo lara
        [Route("GetPathContent/{authenticationToken}/{pathName}/{fileExtension}/{folders}/{files}")]
        //-----------------------------------------------------------------------
        //, out string returnDoc, )
        public IActionResult GetPathContent(string authenticationToken, string pathName, string fileExtension, bool folders, bool files)
        {
            string returnDoc = string.Empty;

            if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
                return null;

            FileSystemMonitor.Engine.GetPathContent(pathName, fileExtension, out returnDoc, folders, files);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("returnDoc");
            jsonWriter.WriteValue(returnDoc);

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }
    }
}