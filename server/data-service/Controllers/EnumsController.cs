using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using Microarea.Common;
using Newtonsoft.Json;
using System.Collections;
using Microarea.Common.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataService.Controllers
{
	[Route("enums-service")]
	public class EnumsController : Controller
	{
		//---------------------------------------------------------------------
		[Route("getEnumsTable")]
		public IActionResult GetEnumsTable()
		{
			string content = Enums.GetJsonEnumsTable();
			return new SuccessResult(content);
        }
	}

    [Route("formatters-service")]
    public class FormattersController : Controller
    {
        UserInfo GetLoginInformation()
        {
            string sAuthT = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(sAuthT))
                return null;

            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = HttpContext.Session;
            }
            catch (Exception)
            {
            }

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(hsession, sAuthT);

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            return ui;
        }

        //---------------------------------------------------------------------
        [Route("getFormattersTable")]
        public IActionResult GetFormattersTable()
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new NoAuthResult("missing authentication token");

            TbSession session = new TbSession(ui, null);

            ApplicationFormatStyles formatStyles = new ApplicationFormatStyles(session);
            formatStyles.Load();

            string content = formatStyles.GetJsonFormattersTable();
            return new SuccessResult(content);
        }
    }

}
