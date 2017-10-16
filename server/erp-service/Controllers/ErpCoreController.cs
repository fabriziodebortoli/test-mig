using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;

namespace erp_service.Controllers
{
    [Route("erp-core")]
    public class ErpCoreController : Controller
    {
        //-----------------------------------------------------------------------------------------
        [Route("CheckVatDuplicate")]
        public IActionResult CheckVatDuplicate()
        {
            var userInfo = GetLoginInformation();
            var connectionString = userInfo.CompanyDbConnection;

            return new JsonResult(new { Success = true, Message = "" });
        }

        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            var cmd = conn.CreateCommand();
            //PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

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
    }
}
