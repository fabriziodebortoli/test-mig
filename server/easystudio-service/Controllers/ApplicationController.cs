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
			internal static readonly string applicationName = "applicationName";
			internal static readonly string applicationType = "applicationType";
			internal static readonly string moduleName = "moduleName";
			internal static readonly string verbose = "verbose";

			internal static readonly string MissingApplicationType = "Missing parameter applicationType";
			internal static readonly string ObjectSuccessfullyCreated = "Successfully Created";
			internal static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
		}



		public override IDiagnosticProvider Diagnostic => ApplicationServiceProp.Diagnostic;
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
			string applicationName = value[Strings.applicationName]?.Value<string>();
			var applicationType = value[Strings.applicationType]?.ToObject<ApplicationType>();
			var moduleName = value[Strings.moduleName]?.Value<string>();
			var verbose = value[Strings.verbose]?.Value<bool>();

			if (!ApplicationServiceProp.ExistsApplication(applicationName))
			{
				if (applicationType == null)
				{
					ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationType);
					return BadRequest(ApplicationServiceProp.Diagnostic);
				}
			}

			//ApplicationServiceProp.Create(applicationName, applicationType, moduleName, verbose);

			return Create(applicationName, (ApplicationType)applicationType, moduleName, verbose != null);
		}

		//-----------------------------------------------------------------------
		private IActionResult Create(string applicationName, ApplicationType applicationType, string moduleName = "", bool verbose = false)
		{
			bool success = true;
			if (!ApplicationServiceProp.ExistsApplication(applicationName))
				success = ApplicationServiceProp.CreateApplication(applicationName, applicationType);

			if (success && !string.IsNullOrEmpty(moduleName))
				success = ApplicationServiceProp.CreateModule(applicationName, moduleName);

			if (success && verbose)
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Information, string.Concat(applicationName, " ", moduleName, Strings.ObjectSuccessfullyCreated));

			if (success)
			{
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Information, Strings.ObjectSuccessfullyCreated);
				return ToContentResult(200, ApplicationServiceProp.Diagnostic);
			}
			return 	ToContentResult(500, ApplicationServiceProp.Diagnostic);
			//return Ok(ApplicationServiceProp.Diagnostic.AsJson);
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

		//-----------------------------------------------------------------------
		private IActionResult Delete(string applicationName, string moduleName, bool verbose = false)
		{
			bool success = true;
			if (!string.IsNullOrEmpty(moduleName))
				success = ApplicationServiceProp.DeleteModule(applicationName, moduleName);
			else
				success = ApplicationServiceProp.DeleteApplication(applicationName);

			if (success && verbose)
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Information, string.Concat(applicationName, " ", moduleName, Strings.ObjectSuccessfullyDeleted));

			return Ok(ApplicationServiceProp.Diagnostic);
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

				string json = ApplicationServiceProp.GetAppsModsAsJson(appType);
				return ToContentResult(json);
			}
			catch (Exception e)
			{
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Error, e.Message);
				return Ok(ApplicationServiceProp.Diagnostic);
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
				var res = ApplicationServiceProp.GetEasyStudioCustomizationsListFor(docNS, user);

				return ToContentResult(res);
			}
			catch (Exception e)
			{
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Error, e.Message);
				return Ok(ApplicationServiceProp.Diagnostic);
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
					appType = (ApplicationType)applicationType;

				var json = ApplicationServiceProp.RefreshAll(appType);

				return ToContentResult(json);
			}
			catch (Exception e)
			{
				ApplicationServiceProp.Diagnostic.Add(DiagnosticType.Error, e.Message);
				return Ok(ApplicationServiceProp.Diagnostic.AsJson);
			}
		}
	}
}
