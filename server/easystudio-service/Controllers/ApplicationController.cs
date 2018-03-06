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
    public class ApplicationController : BaseController
    {
        //=========================================================================
        internal class Strings
        {
            internal static readonly string applicationName = "applicationName";
            internal static readonly string applicationType = "applicationType";
            internal static readonly string moduleName = "moduleName";

            internal static readonly string MissingApplicationType = "Missing parameter applicationType";
            internal static readonly string ObjectSuccessfullyCreated = "Object Successfully Created";
            internal static readonly string ObjectSuccessfullyDeleted = "Object Successfully Deleted";
        }

        private ApplicationService service;
		private ApplicationService ApplicationService
        {
            get
            {
                if (service == null)
                    service = Manager?.GetService(typeof(ApplicationService)) as ApplicationService;

                return service;
            }
        }
        //---------------------------------------------------------------------
        public ApplicationController(IServiceManager serviceManager)
            :
            base(serviceManager)
        {
		}

        //-----------------------------------------------------------------------
        [Route("create")]
        public IActionResult Create(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[Strings.applicationName]?.Value<string>();
            var applicationType = jsonParams[Strings.applicationType]?.ToObject<ApplicationType>();
            var moduleName = jsonParams[Strings.moduleName]?.Value<string>();

            bool success = true;
            if (!ApplicationService.ExistsApplication(applicationName))
            {
                if (applicationType == null)
                    return this.ToContentResult(203, Strings.MissingApplicationType);

                success = ApplicationService.CreateApplication(applicationName, (ApplicationType) applicationType);
            }
                
            if (success && !string.IsNullOrEmpty(moduleName))
                success = ApplicationService.CreateModule(applicationName, moduleName);

            return success ?
                this.ToContentResult(200, Strings.ObjectSuccessfullyCreated) :
                this.ToContentResult(500, ApplicationService.Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete")]
        public IActionResult Delete(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[Strings.applicationName]?.Value<string>();
            var moduleName = jsonParams[Strings.moduleName]?.Value<string>();

              bool success = true;
            if (!string.IsNullOrEmpty(moduleName))
                success = ApplicationService.DeleteModule(applicationName, moduleName);
            else
                success = ApplicationService.DeleteApplication(applicationName);

           return success ?
                    this.ToContentResult(200, Strings.ObjectSuccessfullyDeleted) :
                    this.ToContentResult(500, ApplicationService.Diagnostic);
        }

        //---------------------------------------------------------------------
        [Route("getAllAppsAndModules")]
        public IActionResult GetAllAppsAndModules([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType)applicationType;

                var json = ApplicationService.GetAppsModsAsJson(appType);
                return this.ToContentResult(200, json);
            }
            catch (Exception e)
            {
                return this.ToContentResult(502,  e.Message);
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

                return this.ToContentResult(200,  res);
			}
			catch (Exception e)
			{
                return this.ToContentResult(502,  e.Message);
			}
		}

        //---------------------------------------------------------------------
        [Route("refreshAll")]
        public IActionResult RefreshAll([FromBody] JObject value)
        {
            try
            {
                var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();

                ApplicationType appType = ApplicationType.All;
                if (applicationType != null)
                    appType = (ApplicationType) applicationType;

                var json = ApplicationService.RefreshAll(appType);

                return this.ToContentResult(200,  json);
            }
            catch (Exception e)
            {
                return this.ToContentResult(502,  e.Message);
            }
        }
	}
}
