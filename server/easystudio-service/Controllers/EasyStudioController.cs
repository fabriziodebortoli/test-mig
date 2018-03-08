using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Microarea.EasyStudio.AspNetCore;
using Microarea.Common;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio")]
    public class EasyStudioController : BaseController
    {
        //---------------------------------------------------------------------
        public EasyStudioController(IServiceManager serviceManager)
            : base(serviceManager)
        {
		} 
  
        //---------------------------------------------------------------------
        [Route("getCurrentContextFor")]
		public IActionResult GetCurrentContext([FromBody] JObject value)
		{
			try
			{
				var getDefault = value["getDefault"]?.Value<bool>();
				string user = value["user"]?.Value<string>();

				string res = string.Empty;
				res = ApplicationServiceProp.GetCurrentContext(user, getDefault ?? false);
                return ToContentResult(res);
            }
			catch (Exception e)
			{
                return ToContentResult(e.Message, 502);
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
				bool? isPairDefault = value["def"]?.Value<bool>();
				string user = value["user"]?.Value<string>();
				string company = value["company"]?.Value<string>();

				bool outcome= ApplicationServiceProp.SetCurrentContext(appName, modName, isPairDefault ?? false);
				return ToContentResult(outcome.ToString());
			}
			catch (Exception e)
			{
                return ToContentResult(e.Message, 502);
			}
		}
	}
}
