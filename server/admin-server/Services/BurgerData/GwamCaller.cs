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
        AuthorizationInfo authInfo;

        //----------------------------------------------------------------------
        public GwamCaller(IHttpHelper httpHelper, string GWAMUrl, AuthorizationInfo authInfo)
        {
            this.httpHelper = httpHelper;
            this.GWAMUrl = GWAMUrl;
            this.authInfo = authInfo;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod)
        {
            string url = String.Format(
                "{0}accounts/{1}/{2}/{3}",
                this.GWAMUrl, accMod.AccountName, accMod.InstanceKey, accMod.Ticks);

            // call GWAM API 
            OperationResult opRes = await httpHelper.PostDataAsync(
                url, new List<KeyValuePair<string, string>>(), JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials, string instanceKey)
        {
            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
            entries.Add(new KeyValuePair<string, string>("accountName", credentials.AccountName));
            entries.Add(new KeyValuePair<string, string>("password", credentials.Password));
            entries.Add(new KeyValuePair<string, string>("instanceKey", instanceKey));

            OperationResult opRes = await httpHelper.PostDataAsync(GWAMUrl + "accounts", entries, JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> CheckRecoveryCode(string accountName, string recoveryCode)
        {
            // call GWAM API // todo onpremises ilaria
            OperationResult opRes = await httpHelper.PostDataAsync(
                this.GWAMUrl + "recoveryCode/" + accountName + "/" + recoveryCode,
                new List<KeyValuePair<string, string>>(),
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        // [HttpGet("/api/accountRoles/{accountName}/{roleName}/{entityKey}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>>GetAccountRoles(string accountName, string roleName, string entityKey, int ticks )
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "accountRoles/" + accountName + "/" + roleName + "/" + entityKey + "/" + ticks,
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //[HttpGet("/api/instances/{instanceKey}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>> GetInstance(string instanceKey, int ticks)
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "instances/" + instanceKey + "/" + ticks,
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //[HttpGet("/api/subscription/{subscriptionKey}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>> GetSubscription(string subscriptionKey, int ticks)
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "subscription/" + subscriptionKey + "/" + ticks,
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        // [HttpGet("/api/subscriptionInstances/{subscriptionKey}/{instanceKey}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>> GetSubscriptionInstances(string subscriptionKey, string instanceKey, int ticks)
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "subscriptionInstances/" + subscriptionKey + "/" + instanceKey + "/" + ticks,
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //[HttpGet("/api/subscriptionAccounts/{subscriptionKey}/{accountName}/{ticks}")]
        //----------------------------------------------------------------------
        internal async Task<Task<string>> GetSubscriptionAccounts(string subscriptionKey, string accountName, int ticks)
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                this.GWAMUrl + "subscriptionAccounts/" + subscriptionKey + "/" + accountName + "/" + ticks,
                 JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

		//----------------------------------------------------------------------
		public static async Task<Task<string>> ValidateGWAMToken(string token, IHttpHelper httpHelper, string GWAMUrl)
		{
			OperationResult opRes = await httpHelper.GetDataAsync(GWAMUrl + "permissions/" + token);

			if (!opRes.Result)
				return Task.FromException<string>(new Exception());

			return (Task<string>)opRes.Content;
		}

    }
}
