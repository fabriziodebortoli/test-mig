using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        [Route("CheckVatEU")]
        public async Task<IActionResult> CheckVatEU([FromBody] JObject value)
        {
            var countryCode = value["countryCode"]?.Value<string>();
            var vatNumber = value["vatNumber"]?.Value<string>();
            var result = await callCheckEUAsync(countryCode, vatNumber);
            return new JsonResult(new { IsValid = result });
        }

        [Route("CheckVatRO")]
        public async Task<IActionResult> CheckVatRO([FromBody] JObject value)
        {
            const string url = "https://webservicesp.anaf.ro/PlatitorTvaRest/api/v1/ws/tva";

            using (var _httpClient = new HttpClient())
            {
                var cui = value["cui"]?.Value<string>();
                var data = value["data"]?.Value<string>();
                var postData = $"[{{\"cui\":\"{cui}\",\"data\":\"{data}\"}}]";
                var stringdata = new StringContent(content: postData,
                             encoding: Encoding.UTF8,
                             mediaType: "application/json");

                var response = await _httpClient.PostAsync(url, stringdata);
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json" };
            }
        }

        private async Task<bool> callCheckEUAsync(string countryCode, string vatNumber)
        {
            var check = new CheckVatReference.checkVatPortTypeClient();
            var refer = new CheckVatReference.checkVatRequest(countryCode, vatNumber);
            var result = await check.checkVatAsync(refer);
            var isValid = result.valid;

            return isValid;
        }



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
        public IActionResult CheckBinUsesStructure([FromBody] JObject jsonValue)
        {
            var result = new JsonResult(new { UseBinStructure = false });
            if (jsonValue == null) return result;

            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };

            var zone = jsonValue["zone"]?.Value<string>();
            var storage = jsonValue["storage"]?.Value<string>();

            if (zone == null || storage == null) return result;

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

        [Route("GetItemsSearchList")]
        public IActionResult GetItemsSearchList([FromBody] string queryType)
        {
            var result = new List<Dictionary<string, object>>();

            var ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth" };

            var connection = new SqlConnection(ui.CompanyDbConnection);
            using (var reader = GetItemsSearchReader(connection, queryType))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        // define the dictionary
                        var fieldValues = new Dictionary<string, object>();

                        // fill up each column and values on the dictionary                 
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            fieldValues.Add(reader.GetName(i), reader[i]);
                        }

                        // add the dictionary on the values list
                        result.Add(fieldValues);
                    }
                }
            }

            var jr = new JsonResult(result);
            return jr;
        }

        private SqlDataReader GetItemsSearchReader(SqlConnection connection, string searchType)
        {
            switch (searchType)
            {
                case "producers":
                    return ExecuteReader(connection, System.Data.CommandType.Text, "select Producer, CompanyName from MA_Producers ", null);

                case "categories":
                    return ExecuteReader(connection, System.Data.CommandType.Text, "select Category, Description from MA_ProductCtg ", null);

                case "producersByCategory":
                    {
                        string query = @"   select  MA_ProducersCategories.Producer, MA_Producers.CompanyName, MA_ProducersCategories.Category
                                            from    MA_ProducersCategories
                                                inner join MA_Producers on MA_ProducersCategories.Producer = MA_Producers.Producer";

                        return ExecuteReader(connection, System.Data.CommandType.Text, query, null);
                    }

                case "categoriesByProducer":
                    {
                        string query = @"   select  MA_ProducersCategories.Category, MA_ProductCtg.Description, MA_ProducersCategories.Producer
                                            from    MA_ProducersCategories
                                                inner join MA_ProductCtg on MA_ProducersCategories.Category = MA_ProductCtg.Category";

                        return ExecuteReader(connection, System.Data.CommandType.Text, query, null);
                    }

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
