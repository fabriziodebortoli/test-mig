using System;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.DataService.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Microarea.DataService.Controllers
{
    [Route("data-service/parameters")]
    public class ParameterController : Controller
    {
        private readonly IParameterManager _parameterManager;

        public ParameterController(IParameterManager parameterManager)
        {
            _parameterManager = parameterManager;
        }

        [Route("getparameters")]
        public IActionResult GetParameters([FromBody] JObject value)
        {
            var ui = GetLoginInformation();
            if (ui == null) return Unauthorized();

            var temp = _parameterManager.GetParameters(value["table"].ToString(), ui.CompanyDbConnection);
            return new JsonResult(temp);
        }

        // TODO: sarebbe meglio mettere questa funzione a fattor comune da qualche parte
        #region helpers
        private UserInfo GetLoginInformation()
        {
            string sAuthT = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(sAuthT))
                return null;
            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = HttpContext.Session;
            }
            catch (Exception) { }
            var loginInfo = LoginInfoMessage.GetLoginInformation(hsession, sAuthT);
            return new UserInfo(loginInfo, sAuthT);
        }
        #endregion
    }
}
