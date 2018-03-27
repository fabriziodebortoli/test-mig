using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TaskBuilderNetCore.EasyStudio;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio")]
	public class EasyStudioController : BaseController
	{
		//---------------------------------------------------------------------
		Service<PreferencesService> ServicePreferences { get; set; }
		PreferencesService PrefService { get => ServicePreferences.Obj; }
		public override IDiagnosticProvider Diagnostic => PrefService.Diagnostic;
		//---------------------------------------------------------------------
		Service<ApplicationService> ServiceApplications { get; set; }
		ApplicationService AppService { get => ServiceApplications.Obj; }

		//---------------------------------------------------------------------
		public EasyStudioController(IServiceManager serviceManager)
			: base(serviceManager)
		{
			ServicePreferences = Services?.GetService<PreferencesService>();
			ServiceApplications = Services?.GetService<ApplicationService>();
		}

		//---------------------------------------------------------------------
		[Route("getCurrentContextFor"), HttpPost]
		public IActionResult GetCurrentContext([FromBody] JObject value)
		{
			try
			{
				var getDefault = value[Strings.DefaultReq]?.Value<bool>();
				string user = value[Strings.User]?.Value<string>();
				if (string.IsNullOrEmpty(user))
					throw new Exception();

				string res = GetContextPair(user, getDefault ?? false);
				return ToResult(res);
			}
			catch (Exception e)
			{
				return ToResult(e.Message, 502);
			}
		}

		//---------------------------------------------------------------------
		[Route("setCurrentContextFor"), HttpPost]
		public IActionResult SetAppAndModule([FromBody] JObject value)
		{
			try
			{
				string appName = value[Strings.ApplicationName]?.Value<string>();
				string modName = value[Strings.ModuleName]?.Value<string>();
				bool? isPairDefault = value[Strings.DefaultPair]?.Value<bool>();
				string user = value[Strings.User]?.Value<string>();

				if (string.IsNullOrEmpty(user))
					throw new Exception();

				bool outcome = PrefService.SetContextPreferences(appName, modName, isPairDefault ?? false, user);
				return ToResult(outcome.ToString());
			}
			catch (Exception e)
			{
				return ToResult(e.Message, 502);
			}
		}

		//---------------------------------------------------------------------
		[Route("checkAfterRefresh"), HttpPost]
		public IActionResult CheckAfterRefresh([FromBody] JObject value)
		{
			try
			{
				var applicationType = value[Strings.ApplicationType]?.ToObject<ApplicationType>();
				ApplicationType appType = AppService.CheckAppType(applicationType);

				string user = value[Strings.User]?.Value<string>();
				if (string.IsNullOrEmpty(user))
					throw new Exception();

				var defaultContext = GetContextPair(user, true);
				bool outcome = true;
				if (! AppService.StillExist(defaultContext, appType))
					outcome = PrefService.SetContextPreferences(string.Empty, string.Empty, true, user);

				return ToResult(outcome.ToString());
			}
			catch (Exception e)
			{
				return ToResult(e.Message, 502);
			}
		}

		//---------------------------------------------------------------------
		[Route("isEasyStudioDocument"), HttpPost]
		public IActionResult IsEasyStudioDocument([FromBody] JObject value)
		{
			return ToResult(true.ToString()); //TODOROBY
		}

		//---------------------------------------------------------------------
		public string GetContextPair(string user, bool getDefault)
		{
			return PrefService.GetContextPreferences(user, getDefault);
		}


	}
}