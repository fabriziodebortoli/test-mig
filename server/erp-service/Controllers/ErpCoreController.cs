using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microarea.Common;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static SQLHelper;

namespace ErpService.Controllers
{
    [Route("erp-core")]
    public class ErpCoreController : Controller
    {
        [Route("CheckVatDuplicate")]
        public IActionResult CheckVatDuplicate([FromBody] string vat)
        {
            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };
            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                "select * from MA_CustSupp where TaxIdNumber = @p1", new[] { new SqlParameter("p1", vat) }))
                if (reader.Read())
                    return new JsonResult(new { IsDuplicate = true, Message = $"Already found in {reader["CustSupp"]}" });
            return new JsonResult(new { IsDuplicate = false, Message = "" });
        }

        [Route("CheckBinUsesStructure")]
        public IActionResult CheckBinUsesStructure([FromBody] string jsonValue)
        {
            var result = new JsonResult(new { UseBinStructure = false });
            if (jsonValue == null) return result;

            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };

            var json = JObject.Parse(jsonValue);
            var zone = json.SelectToken("zone")?.Value<string>();
            var storage = json.SelectToken("storage")?.Value<string>();

            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                "select UseBinStructure from MA_WMZone where Zone = @zone and Storage = @storage", new[] {
                    new SqlParameter("zone", zone),
                    new SqlParameter("storage", storage)
                }))
                if (reader.Read())
                    return new JsonResult(new { UseBinStructure = reader[0] });
            return result;
        }

        [Route("CheckItemsAutoNumbering")]
        public IActionResult CheckItemsAutoNumbering()
        {
            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };

            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                "select ItemAutoNum from MA_ItemParameters", null))
            {
                if (reader.Read())
                {
                    bool itemautonum = reader["ItemAutoNum"].ToString() == "1";

                    var result = new JsonResult(new { ItemsAutoNumbering = itemautonum });
                    return result;
                }
            }
            return new JsonResult(new { ItemsAutoNumbering = false });
        }

        [Route("GetItemsSearchList")]
        public IActionResult GetItemsSearchList([FromBody] string queryType)
        {
            var result = new Dictionary<string, string>();

            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };

            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = GetItemsSearchReader(connection, queryType))
            {
                if (reader != null)
                {
                    while (reader.Read())
                        result.Add(reader[0].ToString(), reader[1].ToString());
                }
            }

            return new JsonResult(result);
        }

        private SqlDataReader GetItemsSearchReader(SqlConnection connection, string searchType)
        {
            switch (searchType)
            {
                case "producers":
                    return ExecuteReader(connection, System.Data.CommandType.Text, "select Producer, CompanyName from MA_Producers ", null);

                case "categories":
                    return ExecuteReader(connection, System.Data.CommandType.Text, "select Category, Description from MA_ProductCtg ", null);

                default:
                    return null;
            }
        }

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
