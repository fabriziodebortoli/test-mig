using System;
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

namespace Microarea.Menu.Controllers
{
    public enum ObjectType { Report, Image, Document };

    //[Route("explorer-open")]
    //==============================================================================
    public class TBResourcesController : Controller
    {
        private NameSpace nameSpace;

        //--------------------------------------------------------------------------
        private IList GetApplications()
        {

            IList apps=  BasePathFinder.BasePathFinderInstance.ApplicationInfos;
            return apps; 
        } 

        [Route("explorer-open/get-applications")]
        //--------------------------------------------------------------------------
        public IActionResult GetApplicationsJson()
        {
            //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
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

        [Route("get-folders")]
        //--------------------------------------------------------------------------
        public IActionResult GetFolders(string applicationName)
        {
            //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
           
           /// string[] folders = task.Result;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Folders");

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Folder");

            jsonWriter.WriteStartArray();

            //foreach (string item in folders)
            //{
            //    jsonWriter.WriteStartObject();
            //    jsonWriter.WritePropertyName("name");
            //    jsonWriter.WriteValue(item);
            //    jsonWriter.WriteEndObject();
            //}
            jsonWriter.WriteEndArray();

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        ////--------------------------------------------------------------------------
        //public TBResourcesExplorer(ObjectType aObjectType, NameSpace aNameSpace)
        //{
        //    if (aNameSpace.NameSpaceType.Type == NameSpaceObjectType.NotValid)
        //        return;

        //    if (aNameSpace.IsValid())              //costruzione del NameSpace 
        //    {
        //        nameSpace = aNameSpace;
        //    }
        //       // string typeNs = aNameSpace.NameSpaceType.Type.ToString() + ".";

        //        //typeNs = typeNs + 
        //        //nameSpace.CreateNameSpace();
        //        //        if (aNameSpace.GetType() == CTBNamespace::NOT_VALID)
        //        //            m_NameSpace.SetType(CTBNamespace::REPORT);
        //        //        else
        //        //            m_NameSpace.SetType(aNameSpace.GetType());

        //        //        if (aNameSpace.GetApplicationName().IsEmpty())
        //        //        {
        //        //            AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
        //        //            m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
        //        //        }
        //        //        else
        //        //            m_NameSpace.SetApplicationName(aNameSpace.GetApplicationName());

        //        //        if (aNameSpace.GetObjectName(CTBNamespace::MODULE).IsEmpty())
        //        //        {
        //        //            AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
        //        //            if (pAddOnApp)
        //        //                pMods = pAddOnApp->m_pAddOnModules;

        //        //            if (pMods && pMods->GetSize())
        //        //            {
        //        //                for (int i = 0; i <= pMods->GetUpperBound(); i++)
        //        //                {
        //        //                    //tra i moduli cerca il primo attivo e lo inizializza come primo namespace
        //        //                    if (!AfxIsActivated(pMods->GetAt(i)->GetApplicationName(), pMods->GetAt(i)->GetModuleName()))
        //        //                        continue;

        //        //                    m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMods->GetAt(i)->GetModuleName());
        //        //                    break;
        //        //                }
        //        //            }
        //        //        }
        //        //        else
        //        //            m_NameSpace.SetObjectName(CTBNamespace::MODULE, aNameSpace.GetObjectName(CTBNamespace::MODULE));

        //        //        CTBExplorerCachePtr cache = GetExplorerCache();

        //        //        //memorizza l'ultimo modulo selezionato
        //        //        if (!cache->m_LastUsedNameSpace.IsEmpty())
        //        //            m_NameSpace.SetObjectName(CTBNamespace::MODULE, cache->m_LastUsedNameSpace.GetModuleName());

        //        //    }
        //        //    else
        //        //        m_NameSpace = aNameSpace;
        //        //}



        //        //    {
        //        //        CString strApp;
        //        //        AddOnModsArray* pMods = NULL;


        //        //        m_ExplorerType = aType;
        //        //        m_bCanLink = FALSE;

        //        //        //Cancella le informazioni relative all'ultimo modulo selezionato
        //        //        CTBExplorer::ClearStoredInfo();

        //        //        if (!aNameSpace.IsValid())              //costruzione del NameSpace 
        //        //        {
        //        //            if (aNameSpace.GetType() == CTBNamespace::NOT_VALID)
        //        //                m_NameSpace.SetType(CTBNamespace::REPORT);
        //        //            else
        //        //                m_NameSpace.SetType(aNameSpace.GetType());

        //        //            if (aNameSpace.GetApplicationName().IsEmpty())
        //        //            {
        //        //                AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
        //        //                m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
        //        //            }
        //        //            else
        //        //                m_NameSpace.SetApplicationName(aNameSpace.GetApplicationName());

        //        //            if (aNameSpace.GetObjectName(CTBNamespace::MODULE).IsEmpty())
        //        //            {
        //        //                AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
        //        //                if (pAddOnApp)
        //        //                    pMods = pAddOnApp->m_pAddOnModules;

        //        //                if (pMods && pMods->GetSize())
        //        //                {
        //        //                    for (int i = 0; i <= pMods->GetUpperBound(); i++)
        //        //                    {
        //        //                        //tra i moduli cerca il primo attivo e lo inizializza come primo namespace
        //        //                        if (!AfxIsActivated(pMods->GetAt(i)->GetApplicationName(), pMods->GetAt(i)->GetModuleName()))
        //        //                            continue;

        //        //                        m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMods->GetAt(i)->GetModuleName());
        //        //                        break;
        //        //                    }
        //        //                }
        //        //            }
        //        //            else
        //        //                m_NameSpace.SetObjectName(CTBNamespace::MODULE, aNameSpace.GetObjectName(CTBNamespace::MODULE));

        //        //            CTBExplorerCachePtr cache = GetExplorerCache();

        //        //            //memorizza l'ultimo modulo selezionato
        //        //            if (!cache->m_LastUsedNameSpace.IsEmpty())
        //        //                m_NameSpace.SetObjectName(CTBNamespace::MODULE, cache->m_LastUsedNameSpace.GetModuleName());

        //        //        }
        //        //        else
        //        //            m_NameSpace = aNameSpace;
        //    //}
        //}


    }
}