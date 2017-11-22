using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers.Helpers.DataController
{
	//================================================================================
	public class DataControllerHelper
	{
		//--------------------------------------------------------------------------------
		public static List<IModelObject> GetModelListFromCluster(dynamic dataCluster)
		{
			List<IModelObject> modelList = new List<IModelObject>();
			string rowName;
			JToken rowValue;

			try
			{
				foreach (var c in ((Newtonsoft.Json.Linq.JObject)dataCluster).Children())
				{
					rowName = ((Newtonsoft.Json.Linq.JProperty)c).Name;
					rowValue = ((Newtonsoft.Json.Linq.JProperty)c).Value;
					modelList.Add(GetItemByName(rowName, rowValue));
				}
			}
			catch (Exception)
			{
				// tolog
				modelList.Clear();
			}

			return modelList;
		}

		//--------------------------------------------------------------------------------
		static IModelObject GetItemByName(string name, JToken jToken)
		{
			switch (name)
			{
				case "accounts":
					return jToken.ToObject<Account>();

				case "roles":
					return jToken.ToObject<Role>();

				case "accountRolesForInstance":
				case "accountRolesForSubscription":
					return jToken.ToObject<AccountRoles>();

				case "instance":
					return jToken.ToObject<Instance>();

				case "subscriptionAccount":
					return jToken.ToObject<SubscriptionAccount>();

				case "subscriptionInstances":
					return jToken.ToObject<SubscriptionInstance>();

				case "subscriptions":
					return jToken.ToObject<Subscription>();

				default:
					return null;
			}
		}
	}
}
