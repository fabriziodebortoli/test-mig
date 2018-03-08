using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Microarea.EasyStudio.AspNetCore;

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
		public IActionResult GetCurrentContext(string user)
		{
			try
			{
                // TODOROBY
                // queste informazioni le leggiamo da file system su un file e ritorniamo l'oggettino che lo 
                // rappresenta in json
                // il file sarà per utente e conterrà per adesso currentApplication e CurrentModule
                string res = string.Empty;
				res = ApplicationServiceProp.GetCurrentContext(user);
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
