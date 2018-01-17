using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Commons;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.ExternalUrlCallers
{
	//================================================================================
	public class DatabaseServiceCaller
	{
		IHttpHelper httpHelper;
		string databaseServiceUrl;
		bool databaseServiceIsDown = false;

		//----------------------------------------------------------------------
		public DatabaseServiceCaller(IHttpHelper httpHelper, string databaseServiceUrl)
		{
			this.httpHelper = httpHelper;
			this.databaseServiceUrl = databaseServiceUrl;
			this.databaseServiceIsDown = false;
		}

		//----------------------------------------------------------------------
		public async Task<Task<string>> CheckDatabaseServiceStatus()
		{
			this.databaseServiceIsDown = true;

			OperationResult opRes = await httpHelper.GetDataAsync(databaseServiceUrl + "status");

			if (opRes.Result)
				return (Task<string>)opRes.Content;

			opRes.Result = false;
			opRes.Code = GwamMessageStrings.GoOnDespiteGWAM; // gwam non risponde ma possiamo lavorare offline
			return (Task<string>)opRes.Content;
		}

		//----------------------------------------------------------------------
		/*private async Task<Task<string>> VerifyAccountModificationGWAMAsync(AccountModification accMod)
		{
			string url = String.Format(
				"{0}accounts/{1}/{2}/{3}",
				this.databaseServiceUrl, accMod.AccountName, accMod.InstanceKey, accMod.Ticks);

			// call GWAM API 
			OperationResult opRes = await httpHelper.PostDataAsync(
				url, new List<KeyValuePair<string, string>>(), JsonConvert.SerializeObject(authInfo));

			if (opRes.Result) return (Task<string>)opRes.Content;

			return GwamNotRespondingManager();
		}*/

		//----------------------------------------------------------------------
		/*internal OperationResult GetInstance()
		{
			if (databaseServiceIsDown)
				return new OperationResult();

			Task<string> res = GetInstanceAsync(instance.InstanceKey, instance.Ticks).Result;
			return JsonConvert.DeserializeObject<OperationResult>(res.Result);
		}*/

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
