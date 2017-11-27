using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using Microarea.Common;

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
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
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
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbSession session = new TbSession(ui, null);

            ApplicationFormatStyles formatStyles = new ApplicationFormatStyles(session);
            formatStyles.Load();

            //FormatStylesGroup dtStyle = (FormatStylesGroup)formatStyles.Fs["Double"];

            string content = ""; //TODO GIANLUCA formatStyles.GetJsonEnumsTable();
            return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
        }
    }

}
