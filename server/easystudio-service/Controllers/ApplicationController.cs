using System;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio/application")]
	public class ApplicationController : BaseController
	{
		//=========================================================================
		internal class Strings
		{
            // parameters
			internal static readonly string applicationName = "applicationName";
			internal static readonly string applicationType = "applicationType";
			internal static readonly string moduleName = "moduleName";

            // messages
            internal static readonly string MissingApplicationType = "Missing parameter applicationType";
         }

        //---------------------------------------------------------------------
        Service<ApplicationService> Service { get; set; }
        ApplicationService AppService { get => Service.Obj; }
        public override IDiagnosticProvider Diagnostic => AppService.Diagnostic;

		//---------------------------------------------------------------------
		public ApplicationController(IServiceManager serviceManager)
			:
			base(serviceManager)
		{
            Service = Services?.GetService<ApplicationService>();
        }    
        
        //-----------------------------------------------------------------------
        [Route("create"), HttpGet]
        public IActionResult Create([FromQuery] string applicationName, ApplicationType applicationType, string moduleName = "")
        {
            // la get la lasciamo verbose
            if (AppService.Create(applicationName, (ApplicationType)applicationType, moduleName))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(applicationName, " ", moduleName, BaseStrings.ObjectSuccessfullyCreated));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("create"), HttpPost]
        public IActionResult Create([FromBody] JObject value)
        {
            string applicationName = value[Strings.applicationName]?.Value<string>();
            var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();
            var moduleName = value[Strings.moduleName]?.Value<string>();
 
            if (applicationType == null)
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationType);
            else
                AppService.Create(applicationName, (ApplicationType)applicationType, moduleName);

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete"), HttpGet]
        public IActionResult Delete(string applicationName, string moduleName = "")
        {
            // la get la lasciamo verbose
            if (AppService.Delete(applicationName, moduleName))
                Diagnostic.Add(DiagnosticType.Information, string.Concat(applicationName, " ", moduleName, BaseStrings.ObjectSuccessfullyDeleted));

            return ToResult(Diagnostic);
        }

        //-----------------------------------------------------------------------
        [Route("delete"), HttpDelete]
		public IActionResult Delete(JObject jsonParams)
        {
			string applicationName = jsonParams[Strings.applicationName]?.Value<string>();
			var moduleName = jsonParams[Strings.moduleName]?.Value<string>();

            AppService.Delete(applicationName, moduleName);

            return ToResult(Diagnostic);
        }

		//---------------------------------------------------------------------
		[Route("getAllAppsAndModules"), HttpPost]
		public IActionResult GetAllAppsAndModules([FromBody] JObject value)
		{
			try
			{
				var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();

				ApplicationType appType = ApplicationType.All;
				if (applicationType != null)
					appType = (ApplicationType)applicationType;

				return ToResult(AppService.GetAppsModsAsJson(appType));
			}
			catch (Exception e)
			{
                Diagnostic.Add(DiagnosticType.Error, e.Message);
				return ToResult(Diagnostic);
			}
		}

        //---------------------------------------------------------------------
        [Route("getEasyStudioCustomizationsListFor"), HttpPost]
		public IActionResult GetEasyStudioCustomizationsListFor([FromBody] JObject value)
		{
			try
			{
				string docNS = value["ns"]?.Value<string>();
				string user = value["user"]?.Value<string>();
				var res = AppService.GetEasyStudioCustomizationsListFor(docNS, user);
				return ToResult(res);
			}
			catch (Exception e)
			{
				Diagnostic.Add(DiagnosticType.Error, e.Message);
				return ToResult(Diagnostic);
			}
		}

		//---------------------------------------------------------------------
		[Route("refreshAll"), HttpPost]
		public IActionResult RefreshAll([FromBody] JObject value)
		{
			try
			{
				var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();

				ApplicationType appType = ApplicationType.All;
				if (applicationType != null)
					appType = (ApplicationType)applicationType;

				var json = AppService.RefreshAll(appType);

				return ToResult(json);
			}
			catch (Exception e)
			{
                Diagnostic.Add(DiagnosticType.Error, e.Message);
                return ToResult(Diagnostic);
			}
		}
	}
}
