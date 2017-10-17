using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using static SQLHelper;

namespace ErpService.Controllers
{
    [Route("erp-core")]
    public class ErpCoreController : Controller
    {
        //-----------------------------------------------------------------------------------------
        [Route("CheckVatDuplicate")]
        public IActionResult CheckVatDuplicate([FromBody] string vat)
        {
            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 504, Content = "no auth" };
            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                "select * from MA_CustSupp where TaxIdNumber = @p1", new[] { new SqlParameter("p1", vat) }))
                if (reader.Read())
                    return new JsonResult(new { IsDuplicate = true, Message = $"Already found in {reader["CustSupp"]}" });
            return new JsonResult(new { IsDuplicate = false, Message = "" });
        }

        #region helpers
        UserInfo GetLoginInformation()
        {
            string sAuthT = HttpContext.Request.Cookies[UserInfo.AuthenticationTokenKey];
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
