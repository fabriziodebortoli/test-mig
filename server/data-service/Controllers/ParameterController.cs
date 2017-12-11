using System;
using System.Collections.Generic;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.DataService.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        [HttpPost]
        public IActionResult GetParameters([FromBody]string request)
        {
            var ui = GetLoginInformation();
            if (ui == null) return Unauthorized();

            var parameters = JsonConvert.DeserializeObject<List<string>>(request);

            var temp = _parameterManager.GetParameters(parameters, ui.CompanyDbConnection);
            return new JsonResult(temp);
        }

        [HttpPost("update")]
        public IActionResult UpdateCache([FromBody]string request)
        {
            var ui = GetLoginInformation();
            if (ui == null) return Unauthorized();

            var parameters = JsonConvert.DeserializeObject<List<string>>(request);
            _parameterManager.UpdateCache(parameters, ui.CompanyDbConnection);

            return Ok();
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
