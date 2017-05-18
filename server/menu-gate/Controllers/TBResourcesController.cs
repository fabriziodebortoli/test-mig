﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using System.Collections;
using Microarea.Common.Applications;

namespace Microarea.Menu.Controllers
{
    public enum ObjectType { Report, Image, Document };

    //[Route("explorer-open")]
    //==============================================================================
    public class TBResourcesController : Controller
    {
        #region pregresso
        private NameSpace nameSpace;

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

        [Route("explorer-open/get-applications")]
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

        [Route("explorer-open/get-folders/{applicationPath}")]
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

        [Route("explorer-open/get-folderFiles/{folderPath}")]
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

        #endregion pregresso

        [Route("FileSystemMonitor/Init")]
        //-----------------------------------------------------------------------
        public bool Init(string authenticationToken)
        {
            //    if (!FileSystemMonitor.Engine.IsValidToken(authenticationToken))
            //        return false;

            //    return FileSystemMonitor.Engine.Init();
            return true;
        }

        [Route("FileSystemMonitor/IsAlive")]
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            return true;
        }
    }
}