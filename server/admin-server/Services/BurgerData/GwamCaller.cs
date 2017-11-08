using Microarea.AdminServer.Controllers.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.BurgerData
{
    //================================================================================
    public class GwamCaller
    {
        IHttpHelper httpHelper;
        string GWAMUrl;

        //----------------------------------------------------------------------
        public GwamCaller(IHttpHelper httpHelper, string GWAMUrl)
        {
            this.httpHelper = httpHelper;
            this.GWAMUrl = GWAMUrl;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);

            string url = String.Format(
                "{0}accounts/{1}/{2}/{3}",
                this.GWAMUrl, accMod.AccountName, accMod.InstanceKey, accMod.Ticks);

            // call GWAM API 
            OperationResult opRes = await httpHelper.PostDataAsync(
                url, new List<KeyValuePair<string, string>>(), authHeader);

            if (!opRes.Result)
            {
                return Task.FromException<string>(new Exception());
            }

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials, AuthorizationInfo authInfo, string instanceKey)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);

            string url = GWAMUrl + "accounts";

            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
            entries.Add(new KeyValuePair<string, string>("accountName", credentials.AccountName));
            entries.Add(new KeyValuePair<string, string>("password", credentials.Password));
            entries.Add(new KeyValuePair<string, string>("instanceKey", instanceKey));

            OperationResult opRes = await httpHelper.PostDataAsync(url, entries, authHeader);

            if (!opRes.Result)
            {
                return Task.FromException<string>(new Exception());
            }

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> CheckRecoveryCode(string accountName, string recoveryCode, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);
            // call GWAM API // todo onpremises ilaria
            OperationResult opRes = await httpHelper.PostDataAsync(
                this.GWAMUrl + "recoveryCode/" + accountName + "/" + recoveryCode,
                new List<KeyValuePair<string, string>>(),
                authHeader);

            if (!opRes.Result)
            {
                return Task.FromException<string>(new Exception());
            }

            return (Task<string>)opRes.Content;
        }

        // [HttpGet("/api/AccountRoles/{accountName}/{roleName}/{entityKey}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>>GetAccountRoles(string accountName, string roleName, string entityKey, int ticks, AuthorizationInfo authInfo )
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);

            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "AccountRoles/" + accountName + "/" + roleName + "/" + entityKey + "/" + ticks,
                authHeader);

            if (!opRes.Result)
            {
                return Task.FromException<string>(new Exception());
            }

            return (Task<string>)opRes.Content;
        }


    }
}
