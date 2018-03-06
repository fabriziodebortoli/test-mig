using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Services;
using System;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
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
            internal static readonly string verbose = "verbose";

            internal static readonly string MissingApplicationType = "Missing parameter applicationType";
            internal static readonly string ObjectSuccessfullyCreated = "Successfully Created";
            internal static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
        }

        private ApplicationService service;
        //---------------------------------------------------------------------
        private ApplicationService ApplicationService
        {
            get
            {
                if (service == null)
                    service = Services?.GetService(typeof(ApplicationService)) as ApplicationService;

                return service;
            }
        }

        public override IDiagnosticProvider Diagnostic { get => ApplicationService.Diagnostic; }
        //---------------------------------------------------------------------
        public ApplicationController(IServiceManager serviceManager)
            :
            base(serviceManager)
        {
		}

        //-----------------------------------------------------------------------
        [Route("create")]
        public IActionResult Create([FromBody] JObject value)
        {
            string applicationName = value[EasyStudioControllerParameters.Strings.applicationName]?.Value<string>();
            var applicationType = value[EasyStudioControllerParameters.Strings.applicationType]?.ToObject<ApplicationType>();
            var moduleName = value[EasyStudioControllerParameters.Strings.moduleName]?.Value<string>();
            var verbose = jsonParams[Strings.verbose];

            if (!ApplicationService.ExistsApplication(applicationName))
            {
                if (applicationType == null)
                {
                    ApplicationService.Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationType);
                    return BadRequest(ApplicationService.Diagnostic);
                }
            }
            return Create(applicationName, (ApplicationType) applicationType, moduleName, verbose != null);
        }

        //-----------------------------------------------------------------------
        [Route("delete")]
        public IActionResult Delete(string parameters)
        {
            JObject jsonParams = JObject.Parse(parameters);
            string applicationName = jsonParams[Strings.applicationName]?.Value<string>();
            var moduleName = jsonParams[Strings.moduleName]?.Value<string>();
            var verbose = jsonParams[Strings.verbose];

            return Delete(applicationName, moduleName, verbose != null);
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
                return ToContentResult(json);
            }
            catch (Exception e)
            {
                ApplicationService.Diagnostic.Add(DiagnosticType.Error, e.Message);
                return Ok(ApplicationService.Diagnostic);
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

                return ToContentResult(res);
			}
			catch (Exception e)
			{
                ApplicationService.Diagnostic.Add(DiagnosticType.Error, e.Message);
                return Ok(ApplicationService.Diagnostic);
			}
		}

    /*    //---------------------------------------------------------------------
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

                return ToContentResult(json);
            }
            catch (Exception e)
            {
                ApplicationService.Diagnostic.Add(DiagnosticType.Error, e.Message);
                return Ok(ApplicationService.Diagnostic);
            }
        }*/
	}
}
