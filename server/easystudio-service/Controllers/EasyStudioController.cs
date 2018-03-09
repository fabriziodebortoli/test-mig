using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using TaskBuilderNetCore.EasyStudio;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio")]
    public class EasyStudioController : BaseController
    {
        //---------------------------------------------------------------------
        Service<PreferencesService> Service { get; set; }
        PreferencesService PrefService { get => Service.Obj; }
        public override IDiagnosticProvider Diagnostic => PrefService.Diagnostic;

        //---------------------------------------------------------------------
        public EasyStudioController(IServiceManager serviceManager)
            : base(serviceManager)
        {
            Service = Services?.GetService<PreferencesService>();
        }

        //---------------------------------------------------------------------
        [Route("getCurrentContextFor"), HttpPost]
        public IActionResult GetCurrentContext([FromBody] JObject value)
        {
            try
            {
                var getDefault = value["getDefault"]?.Value<bool>();
                string user = value["user"]?.Value<string>();

                string res = PrefService.GetCurrentContext(user, getDefault ?? false);
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
                string appName = value["app"]?.Value<string>();
                string modName = value["mod"]?.Value<string>();
                bool? isPairDefault = value["def"]?.Value<bool>();
                string user = value["user"]?.Value<string>();

                bool outcome = PrefService.SetPreferences(appName, modName, isPairDefault ?? false);
                return ToResult(outcome.ToString());
            }
            catch (Exception e)
            {
                return ToResult(e.Message, 502);
            }
        }
    }
}
