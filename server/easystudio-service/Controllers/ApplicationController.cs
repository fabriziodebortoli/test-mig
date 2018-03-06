using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Services;
using System;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Microarea.EasyStudio.Common;
using Newtonsoft.Json.Linq;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio/application")]
    public class ApplicationController : Microsoft.AspNetCore.Mvc.Controller
    {
		private IServiceManager Manager { get; set; }
		private ApplicationService ApplicationService { get => Manager?.GetService(typeof(ApplicationService)) as ApplicationService; }
        //---------------------------------------------------------------------
        public ApplicationController(IServiceManager serviceManager)
        {
            Manager = serviceManager;
		
		}

        //-----------------------------------------------------------------------
        [Route("create")]
        public IActionResult Create(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[EasyStudioControllerParameters.Strings.applicationName]?.Value<string>();
            var applicationType = jsonParams[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();
            var moduleName = jsonParams[EasyStudioControllerParameters.Strings.moduleName]?.Value<string>();

            if (string.IsNullOrEmpty(applicationName))
                return this.ToContentResult(203, MsgType.Error, ControllerDiagnostic.Strings.MissingApplicationName);

            bool success = true;
            if (!ApplicationService.ExistsApplication(applicationName))
            {
                if (applicationType == null)
                    return this.ToContentResult(203, MsgType.Error, ControllerDiagnostic.Strings.MissingApplicationType);

                success = ApplicationService.CreateApplication(applicationName, (ApplicationType) applicationType);
            }
                
            if (success && !string.IsNullOrEmpty(moduleName))
                success = ApplicationService.CreateModule(applicationName, moduleName);

            if (success)            
                return this.ToContentResult(200, MsgType.Success, ControllerDiagnostic.Strings.ObjectSuccessfullyCreated);

            return this.ToContentResult(500, MsgType.Error, ControllerDiagnostic.Strings.ErrorCreatingObject);
        }

        //-----------------------------------------------------------------------
        [Route("delete")]
        public IActionResult Delete(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[EasyStudioControllerParameters.Strings.applicationName]?.Value<string>();
            var moduleName = jsonParams[EasyStudioControllerParameters.Strings.moduleName]?.Value<string>();

            if (string.IsNullOrEmpty(applicationName))
                return this.ToContentResult(203, MsgType.Error, ControllerDiagnostic.Strings.MissingApplicationName);

            bool success = true;
            if (!string.IsNullOrEmpty(moduleName))
                success = ApplicationService.DeleteModule(applicationName, moduleName);
            else
                success = ApplicationService.DeleteApplication(applicationName);

            if (success)
                return this.ToContentResult(200, MsgType.Success, ControllerDiagnostic.Strings.ObjectSuccessfullyDeleted);

            return this.ToContentResult(500, MsgType.Error, ControllerDiagnostic.Strings.ErrorDeletingObject);
        }

        //---------------------------------------------------------------------
        [Route("getAllAppsAndModules")]
        public IActionResult GetAllAppsAndModules([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType)applicationType;

                var json = ApplicationService.GetAppsModsAsJson(appType);
                return this.ToContentResult(200, MsgType.Success, json);
            }
            catch (Exception e)
            {
                return this.ToContentResult(502, MsgType.Error, e.Message);
            }
        }

        //-----item-customizations-dropdown--------------------------------------------------------------
        [Route("getEasyStudioCustomizationsListFor")]
		public IActionResult GetEasyStudioCustomizationsListFor([FromBody] JObject value)
		{
			try
			{
				string docNS = value["ns"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				var res = ApplicationService.GetEasyStudioCustomizationsListFor(docNS, user);

                return this.ToContentResult(200, MsgType.Error, res);
			}
			catch (Exception e)
			{
                return this.ToContentResult(502, MsgType.Error, e.Message);
			}
		}

        //---------------------------------------------------------------------
        [Route("refreshAll")]
        public IActionResult RefreshAll([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType) applicationType;

                var json = ApplicationService.RefreshAll(appType);

                return this.ToContentResult(200, MsgType.Success, json);
            }
            catch (Exception e)
            {
                return this.ToContentResult(502, MsgType.Error, e.Message);
            }
        }

	}
}
