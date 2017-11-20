using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
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
        IInstance instance;
        bool GwamDown = false;

        //----------------------------------------------------------------------
        public GwamCaller(IHttpHelper httpHelper, string GWAMUrl, IInstance instance)
        {
            this.httpHelper = httpHelper;
            this.GWAMUrl = GWAMUrl;
            this.instance = instance == null ? new Instance() : instance;
            this.authInfo =  instance.GetAuthorizationInfo();
        }

        //----------------------------------------------------------------------
        private OperationResult VerifyPendingFlag(Task<string> res)
        {
            OperationResult opRes = new OperationResult();

            // GWAM call could not end correctly: so we check the object
            if (res.Status == TaskStatus.Faulted)
            {
                GwamDown = true;
                //imposto il flag pending per capire quanto tempo passa fuori copertura
                if (!instance.VerifyPendingDate())
                {
                    // gwam non risponde e non possiamo lavorare offline
                    opRes.Result = false;
                    opRes.Code = (int)AppReturnCodes.GWAMCommunicationError;
                    opRes.Message = Strings.GWAMCommunicationError;
                    return opRes;
                }
                opRes.Result = true;
                opRes.Code = GwamMessageStrings.GoOnDespiteGWAM; // gwam non risponde ma possiamo lavorare offline
            }
            opRes = JsonConvert.DeserializeObject<OperationResult>(res.Result as string);
            return opRes;
        }

        //----------------------------------------------------------------------
        internal async Task<Task<string>> GetInstancesListFromGWAM(string accountName)
        {
            OperationResult opRes = new OperationResult();
            if (GwamDown)
                return (Task<string>)opRes.Content;

            string url = String.Format("{0}listInstances/{1}", this.GWAMUrl, accountName);

            // call GWAM API 
            opRes = await httpHelper.PostDataAsync(
                url, new List<KeyValuePair<string, string>>(), String.Empty);

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        internal OperationResult VerifyAccountModificationGWAM(AccountModification accMod)
        {
            OperationResult opRes = new OperationResult();
            if (GwamDown)
                return opRes;

            Task<string> res = VerifyAccountModificationGWAMAsync(accMod).Result;
            
            opRes = VerifyPendingFlag(res);

            return opRes;
        }


        //----------------------------------------------------------------------
        private async Task<Task<string>> VerifyAccountModificationGWAMAsync(AccountModification accMod)
        {
            string url = String.Format(
                "{0}accounts/{1}/{2}/{3}",
                this.GWAMUrl, accMod.AccountName, accMod.InstanceKey, accMod.Ticks);

            // call GWAM API 
            OperationResult opRes = await httpHelper.PostDataAsync(
                url, new List<KeyValuePair<string, string>>(), JsonConvert.SerializeObject(authInfo));

            if (!opRes.Result)
                return Task.FromException<string>(new Exception());//FAULTED, POTREBBE NON RISPONDERE

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

        //----------------------------------------------------------------------
        internal OperationResult GetInstance()
        {
            OperationResult opRes = new OperationResult();
            if (GwamDown)
                return opRes;

           Task<string>  res = GetInstanceAsync(instance.InstanceKey, instance.Ticks).Result;

           opRes= VerifyPendingFlag(res);

           return opRes;
        }

        //[HttpGet("/api/instances/{instanceKey}/{ticks}")]
        //----------------------------------------------------------------------
        private async Task<Task<string>> GetInstanceAsync(string instanceKey, int ticks)
        {
            OperationResult opRes = await httpHelper.GetDataAsync(
                GWAMUrl + "instances/" + instanceKey + "/" + ticks,
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
