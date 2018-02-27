﻿using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Services;
using System;
using TaskBuilderNetCore.EasyStudio;
using Microarea.Common.Generic;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;
using System.IO;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================

	//[Produces("application/json")]
	[Route("easystudio")]
    public class EasyStudioController : Microsoft.AspNetCore.Mvc.Controller
    {
		private ServicesManager Manager { get; set; }
		private ApplicationService applicationServiceInstance;
        //---------------------------------------------------------------------
        public EasyStudioController()
        {
			Manager = ServicesManager.ServicesManagerInstance;
			applicationServiceInstance = Manager?.GetService(typeof(ApplicationService)) as ApplicationService;
			if(applicationServiceInstance == null)
			{
				//TODOROBY
			}
		}

		//-----item-customizations-dropdown--------------------------------------------------------------
		[Route("getCustomizationsForDocument")]
		public IActionResult GetCustomizationsForDocumentFunction([FromBody] JObject value)
		{
			try
			{
				string docNS = value["ns"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				var res = applicationServiceInstance.GetListCustomizations(docNS, user);
				if (res == null || !IsDesignable(new NameSpace(docNS)))
					res = "";
				return new ContentResult { StatusCode = 200, Content = res, ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		[Route("getAllAppsAndModules")]
		public IActionResult GetAllAppsAndModules()
		{
			try
			{
				var json = applicationServiceInstance.GetAppsModsAsJson(TaskBuilderNetCore.Interfaces.ApplicationType.Customization, IsDeveloperEdition());
				return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		[Route("refreshEasyBuilderApps")]
		public IActionResult RefreshEasyBuilderApps()
		{
			try
			{
				Manager.PathFinder.ApplicationInfos.Clear();
				var json = applicationServiceInstance.GetAppsModsAsJson(TaskBuilderNetCore.Interfaces.ApplicationType.Customization, IsDeveloperEdition());
				return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		private bool IsDeveloperEdition()
		{
			return true;
		//TODOROBY	return LoginFacilities.loginManager.IsActivated(NameSolverStrings.TBS, "DevelopmentEd");
		}

		//---------------------------------------------------------------------
		[Route("getCurrentContext")]
		public IActionResult GetCurrentContext()
		{
			try
			{
				string app = applicationServiceInstance.CurrentApplication;
				string mod = applicationServiceInstance.CurrentModule;
				string res = ((app != null) && (mod != null)) ? app + ";" + mod : "";

				return new ContentResult { StatusCode = 200, Content = res, ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		[Route("createNewContext")]
		public IActionResult CreateNewContext([FromBody] JObject value)
		{
			try
			{
				string appName = value["app"]?.Value<string>();
				string modName = value["mod"]?.Value<string>();
				var type = value["type"]?.ToObject<ApplicationType>();
				if (type == null)
					type = ApplicationType.Customization;

				{
					ApplicationInfo newAppInfo = CreateNeededFiles(appName, modName, type);
					//BaseCustomizationContext.CustomizationContextInstance.EasyStudioApplications.Add(context);	//DIVENTA 
					Manager.PathFinder.ApplicationInfos.Add(newAppInfo);
				}

				return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		private ApplicationInfo CreateNeededFiles(string appName, string modName, ApplicationType? type)
		{
			if (!applicationServiceInstance.ExistsApplication(appName))
				applicationServiceInstance.CreateApplication(appName, type ?? ApplicationType.Customization);
			ApplicationInfo ai = Manager.PathFinder.GetApplicationInfoByName(appName);
			if (ai == null)
				return null;
			if (!applicationServiceInstance.ExistsModule(appName, modName))
			{
				ai.AddDynamicModule(modName);
				if(!applicationServiceInstance.CreateModule(appName, modName) )
					return null;
			}
			//TODOROBY
			/*
						//1 load customlist
						//2 aggiungo l'application config alla custom list, 3 salvo
						EasyStudioAppFileListManager.AddToCustomList(manager.PathFinder.GetApplicationConfigFullName(applicationName), false);
						EasyBuilderAppFileListManager.AddToCustomList(manager.PathFinder.GetModuleConfigFullName(applicationName, moduleName));

						// avvisa l'applicazione di ricaricare
						CUtility.ReloadApplication(applicationName);*/
			return ai;
		}

		//---------------------------------------------------------------------
		[Route("setAppAndModule")]
		public IActionResult SetAppAndModule([FromBody] JObject value)
		{
			try
			{
				string appName = value["app"]?.Value<string>();
				string modName = value["mod"]?.Value<string>();
				string isPairDefault = value["def"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				string company = value["company"]?.Value<string>();

				applicationServiceInstance.CurrentApplication= appName;
				applicationServiceInstance.CurrentModule = modName;

				/*TODOROBY SCRIVERE ESPREFERENCES.json
				ritornare l'esito di questa operazione ad angular ????????
				bool e1 = false, e2 = false;
				if (Convert.ToBoolean(isPairDefault))
				{
					e1 = EsPreferences.Write(defaultContextApplication, appName, user, company);
					e2 = EsPreferences.Write(defaultContextModule, modName, user, company);
				}*/

				return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		[Route("getDefaultContext")]
		public IActionResult GetDefaultContextXml([FromBody] JObject value)
		{
			try
			{
				string user = value["user"]?.Value<string>();
				string company = value["company"]?.Value<string>();
				string app = "";
				string mod = "";
				//TODOROBY leggere dalle EsPreferences
				//app = EsPreferences.ReadProperty(defaultContextApplication);
				//mod = EsPreferences.ReadProperty(defaultContextModule);

				string directPath = Path.Combine(Manager.PathFinder.GetCustomESHomePath(), app, mod);
				if (!Manager.PathFinder.FileSystemManager.ExistPath(directPath))
				{
					app = mod = ""; //TODOROBY togliere la coppia da preferences
				}

				string res = ((app != null) && (mod != null)) ? app + ";" + mod : null;

				return new ContentResult { StatusCode = 200, Content = res, ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		[Route("isEasyStudioDocument")]
		public IActionResult IsEasyStudioDocumentFunction([FromBody] JObject value)
		{
			try
			{
				string docNs = value["ns"]?.Value<string>();
				bool res = true; //TODORY IsEasyStudioDocument(docNs);
				return new ContentResult { StatusCode = 200, Content = res.ToString(), ContentType = "application/json" };
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
			}
		}

		//---------------------------------------------------------------------
		private bool IsDesignable(NameSpace nameSpace)
		{
			IDocumentInfo info = Manager.PathFinder.GetDocumentInfo(nameSpace);
			return true; // info.IsDesignable; 
		}
	}
}
