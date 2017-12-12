using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers.Helpers.APIQuery
{
	//======================================================================
	public class APIQueryHelper
    {
		//-----------------------------------------------------------------------------	
		public static OperationResult Query(ModelTables modelTable, APIQueryData apiQueryData, BurgerData burgerData)
		{
			// load Body data in QueryInfo object

			SelectScript selectScript = new SelectScript(SqlScriptManager.GetTableName(modelTable));

			foreach (KeyValuePair<string, string> kvp in apiQueryData.MatchingFields)
			{
				selectScript.AddWhereParameter(kvp.Key, kvp.Value, QueryComparingOperators.IsEqual, false);
			}

			foreach (KeyValuePair<string, string> kvp in apiQueryData.LikeFields)
			{
				selectScript.AddWhereParameter(kvp.Key, kvp.Value, QueryComparingOperators.Like, false);
			}

			OperationResult opRes = new OperationResult();
			opRes.Result = true;

			switch (modelTable)
			{
				case ModelTables.Accounts:
					opRes.Content = burgerData.GetList<Account, IAccount>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Subscriptions:
					opRes.Content = burgerData.GetList<Subscription, ISubscription>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Roles:
					opRes.Content = burgerData.GetList<Role, IRole>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.AccountRoles:
					opRes.Content = burgerData.GetList<AccountRoles, IAccountRoles>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Instances:
					opRes.Content = burgerData.GetList<Instance, IInstance>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.SubscriptionAccounts:
					opRes.Content = burgerData.GetList<SubscriptionAccount, ISubscriptionAccount>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.SubscriptionInstances:
					opRes.Content = burgerData.GetList<SubscriptionInstance, ISubscriptionInstance>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.None:
				default:
					opRes.Result = false;
					opRes.Code = (int)AppReturnCodes.UnknownModelName;
					opRes.Message = Strings.UnknownModelName;
					break;
			}

			return opRes;
		}
	}
}
