using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Microarea.EasyStudio.Common;

namespace Microarea.EasyStudio.Controllers
{
	//=========================================================================
	[Route("easystudio")]
    public class EasyStudioController : Microsoft.AspNetCore.Mvc.Controller
    {
		private IServiceManager Manager { get; set; }
        //---------------------------------------------------------------------
        public EasyStudioController(IServiceManager serviceManager)
        {
            Manager = serviceManager;
		
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

                return this.ToContentResult(200, res);
            }
			catch (Exception e)
			{
                return this.ToContentResult(502, e.Message);
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
	}
}
