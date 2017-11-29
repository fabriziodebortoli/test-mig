using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;

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
				case "accountRolesForAccount":
					return jToken.ToObject<AccountRoles>();

				case "instance":
					return jToken.ToObject<Instance>();

				case "instanceAccounts":
					return jToken.ToObject<InstanceAccount>();

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

		//--------------------------------------------------------------------------------
		public static OperationResult SaveCluster(List<IModelObject> modelList, BurgerData burgerData)
		{
			OperationResult saveResult = new OperationResult(true, String.Empty);

			// a list of all saved models. We'll use this list in case we need to rollback the savings
			Stack<IModelObject> savedModelStack = new Stack<IModelObject>();

			// log of each model saving
			Dictionary<string, bool> saveLog = new Dictionary<string, bool>();

			try
			{
				bool saveClusterResult = true;

				foreach (IModelObject iModel in modelList)
				{
					saveResult = iModel.Save(burgerData);
					saveLog.Add(iModel.GetHashCode().ToString() + ": " + saveResult.Message, saveResult.Result);

					if (!saveResult.Result)
					{
						saveClusterResult = false;
						break;
					}

					savedModelStack.Push(iModel);
				}

				if (saveClusterResult == false)
				{
					// an error occurred while saving, rollback all items
					// in savedModelList

					while (savedModelStack.Count > 0)
					{
						savedModelStack.Pop().Delete(burgerData);
					}
				}
			}
			catch (Exception e)
			{
				saveResult.Result = false;
				saveResult.Message = "An error occurred in DataControllerHelper.SaveCluster " + e.Message;
			}

			saveResult.Content = saveLog;
			return saveResult;
		}
	}
}
