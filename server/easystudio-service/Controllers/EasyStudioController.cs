using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Services;
using System;
using System.Text;
using Microarea.Common.Generic;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;
using System.IO;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	//[Produces("application/json")]
	[Route("easystudio")]
    public class EasyStudioController : Microsoft.AspNetCore.Mvc.Controller
    {
		private IServiceManager Manager { get; set; }
		private ApplicationService ApplicationService { get => Manager?.GetService(typeof(ApplicationService)) as ApplicationService; }
        //---------------------------------------------------------------------
        public EasyStudioController(IServiceManager serviceManager)
        {
            Manager = serviceManager;
		
		}

        //---------------------------------------------------------------------
        private static IActionResult ToContentResult(int statusCode, MessageType type, string text)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = ControllerDiagnostic.ToJson(type, text),
                ContentType = "application/json"
            };
        }

        //-----item-customizations-dropdown--------------------------------------------------------------
        [Route("discovery")]
        public IActionResult Discovery()
        {
            return new ContentResult { StatusCode = 200, Content =  "{ 'Message': 'Buongiorno!  Sono il servizio ES!' }", ContentType = "application/json" };
        }
  
        //-----------------------------------------------------------------------
        [Route("application/create")]
        public IActionResult Application_Create(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[EasyStudioControllerParameters.Strings.applicationName]?.Value<string>();
            var applicationType = jsonParams[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();
            var moduleName = jsonParams[EasyStudioControllerParameters.Strings.moduleName]?.Value<string>();

            if (string.IsNullOrEmpty(applicationName))
                return ToContentResult(203, MessageType.Error, ControllerDiagnostic.Strings.MissingApplicationName);

            bool success = true;
            if (!ApplicationService.ExistsApplication(applicationName))
            {
                if (applicationType == null)
                    return ToContentResult(203, MessageType.Error, ControllerDiagnostic.Strings.MissingApplicationType);

                success = ApplicationService.CreateApplication(applicationName, (ApplicationType) applicationType);
            }
                
            if (success && !string.IsNullOrEmpty(moduleName))
                success = ApplicationService.CreateModule(applicationName, moduleName);

            if (success)            
                return ToContentResult(200, MessageType.Success, ControllerDiagnostic.Strings.ObjectSuccessfullyCreated);

            return ToContentResult(500, MessageType.Error, ControllerDiagnostic.Strings.ErrorCreatingObject);
        }

        //-----------------------------------------------------------------------
        [Route("application/delete")]
        public IActionResult Application_Delete(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[EasyStudioControllerParameters.Strings.applicationName]?.Value<string>();
            var moduleName = jsonParams[EasyStudioControllerParameters.Strings.moduleName]?.Value<string>();

            if (string.IsNullOrEmpty(applicationName))
                return ToContentResult(203, MessageType.Error, ControllerDiagnostic.Strings.MissingApplicationName);

            bool success = true;
            if (!string.IsNullOrEmpty(moduleName))
                success = ApplicationService.DeleteModule(applicationName, moduleName);
            else
                success = ApplicationService.DeleteApplication(applicationName);

            if (success)
                return ToContentResult(200, MessageType.Success, ControllerDiagnostic.Strings.ObjectSuccessfullyDeleted);

            return ToContentResult(500, MessageType.Error, ControllerDiagnostic.Strings.ErrorDeletingObject);
        }

        //---------------------------------------------------------------------
        [Route("application/getAllAppsAndModules")]
        public IActionResult Application_GetAllAppsAndModules([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType)applicationType;

                var json = ApplicationService.GetAppsModsAsJson(appType);
                return ToContentResult(200, MessageType.Success, json);
            }
            catch (Exception e)
            {
                return ToContentResult(502, MessageType.Error, e.Message);
            }
        }

        //-----item-customizations-dropdown--------------------------------------------------------------
        [Route("application/getEasyStudioCustomizationsListFor")]
		public IActionResult Application_GetEasyStudioCustomizationsListFor([FromBody] JObject value)
		{
			try
			{
				string docNS = value["ns"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				var res = ApplicationService.GetEasyStudioCustomizationsListFor(docNS, user);

                return ToContentResult(200, MessageType.Error, res);
			}
			catch (Exception e)
			{
                return ToContentResult(502, MessageType.Error, e.Message);
			}
		}

        //---------------------------------------------------------------------
        [Route("application/RefreshAll")]
        public IActionResult Application_RefreshAll([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType) applicationType;

                var json = ApplicationService.RefreshAll(appType);

                return ToContentResult(200, MessageType.Success, json);
            }
            catch (Exception e)
            {
                return ToContentResult(502, MessageType.Error, e.Message);
            }
        }

        //---------------------------------------------------------------------
        [Route("getCurrentContextFor")]
		public IActionResult GetCurrentContext(string user)
		{
			try
			{
                // TODOROBY
                // queste informazioni le leggiamo da file system su un file e ritorniamo l'oggettino che lo 
                // rappresenta in json
                // il file sarà per utente e conterrà per adesso currentApplication e CurrentModule
                string res = string.Empty;

                return ToContentResult(200, MessageType.Success, res);
            }
			catch (Exception e)
			{
                return ToContentResult(502, MessageType.Error, e.Message);
            }
		}

		//---------------------------------------------------------------------
		[Route("setCurrentContextFor")]
		public IActionResult SetAppAndModule([FromBody] JObject value)
		{
			try
			{
				string appName = value["app"]?.Value<string>();
				string modName = value["mod"]?.Value<string>();
				string isPairDefault = value["def"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				string company = value["company"]?.Value<string>();

            //    ApplicationService.CurrentApplication= appName;
            //    ApplicationService.CurrentModule = modName;

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
        /*
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
		}*/
	}
}
