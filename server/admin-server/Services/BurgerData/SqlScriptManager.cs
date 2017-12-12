using System;

namespace Microarea.AdminServer.Services.BurgerData
{
    public class SqlScriptManager
    {
        //--------------------------------------------------------------------------------
        public static string GetTableName(ModelTables table)
        {
            switch (table)
            {
                case ModelTables.Accounts:
                    return "MP_Accounts";
                case ModelTables.AccountRoles:
                    return "MP_AccountRoles";
                case ModelTables.Roles:
                    return "MP_Roles";
                case ModelTables.RegisteredApps:
                    return "MP_RegisteredApps";
                case ModelTables.Instances:
                    return "MP_Instances";
				case ModelTables.InstanceAccounts:
					return "MP_InstanceAccounts";
				case ModelTables.RecoveryCode:
                    return "MP_RecoveryCodes";
                case ModelTables.ServerURLs:
                    return "MP_ServerURLs";
				case ModelTables.SecurityTokens:
					return "MP_SecurityTokens";
                case ModelTables.Subscriptions:
                    return "MP_Subscriptions";
				case ModelTables.SubscriptionDatabases:
					return "MP_SubscriptionDatabases";
				case ModelTables.SubscriptionExternalSources:
					return "MP_SubscriptionExternalSources";
				default:
                    return String.Empty;
            }
        }

        //--------------------------------------------------------------------------------
        public static string GetOperatorText(QueryComparingOperators comparingOperator)
        {
            switch (comparingOperator)
            {
                case QueryComparingOperators.IsEqual:
                    return " = ";
                case QueryComparingOperators.IsNotEqual:
                    return " != ";
                case QueryComparingOperators.IsGreater:
                    return " > ";
                case QueryComparingOperators.IsGreaterOrEqual:
                    return " >= ";
                case QueryComparingOperators.IsSmaller:
                    return " < ";
                case QueryComparingOperators.IsSmallerOrEqual:
                    return " <= ";
                case QueryComparingOperators.Like:
                    return " LIKE ";
                default:
                    break;
            }

            return String.Empty;
        }

        //--------------------------------------------------------------------------------
        public static string GetExistQueryByModel(ModelTables table)
        {
            switch (table)
            {
                case ModelTables.None:
                    return String.Empty;

                case ModelTables.Accounts:
                    return Queries.ExistAccount;

				case ModelTables.AccountRoles:
					return Queries.ExistAccountRoles;

				case ModelTables.Instances:
					return Queries.ExistInstance;

				case ModelTables.InstanceAccounts:
					return Queries.ExistInstanceAccount;

				case ModelTables.Roles:
					return Queries.ExistRole;

				case ModelTables.Subscriptions:
                    return Queries.ExistSubscription;

                case ModelTables.SubscriptionAccounts:
                    return Queries.ExistSubscriptionAccount;

				case ModelTables.SubscriptionDatabases:
					return Queries.ExistSubscriptionDatabase;

				case ModelTables.SubscriptionExternalSources:
					return Queries.ExistSubscriptionExternalSource;

				case ModelTables.SubscriptionInstances:
					return Queries.ExistSubscriptionInstances;

				case ModelTables.SecurityTokens:
					return Queries.ExistSecurityToken;

				default:
                    return String.Empty;
            }
        }

        //--------------------------------------------------------------------------------
        public static string GetUpdateQueryByModel(ModelTables table)
        {
            switch (table)
            {
                case ModelTables.None:
                    return String.Empty;

                case ModelTables.Accounts:
                    return Queries.UpdateAccount;

				case ModelTables.AccountRoles:
					return Queries.UpdateAccountRoles;

				case ModelTables.Instances:
					return Queries.UpdateInstance;

				case ModelTables.InstanceAccounts:
					return Queries.UpdateInstanceAccount;

				case ModelTables.Roles:
					return Queries.UpdateRole;

				case ModelTables.Subscriptions:
                    return Queries.UpdateSubscription;

                case ModelTables.SubscriptionAccounts:
                    return Queries.UpdateSubscriptionAccount;

				case ModelTables.SubscriptionDatabases:
					return Queries.UpdateSubscriptionDatabase;

				case ModelTables.SubscriptionExternalSources:
					return Queries.UpdateSubscriptionExternalSource;

				case ModelTables.SecurityTokens:
					return Queries.UpdateSecurityToken;

				case ModelTables.SubscriptionInstances:
					return Queries.UpdateSubscriptionInstances;

				default:
                    return String.Empty;
            }
        }

        //--------------------------------------------------------------------------------
        public static string GetInsertQueryByModel(ModelTables table)
        {
            switch (table)
            {
                case ModelTables.None:
                    return String.Empty;

                case ModelTables.Accounts:
                    return Queries.InsertAccount;

				case ModelTables.AccountRoles:
					return Queries.InsertAccountRoles;

				case ModelTables.Instances:
					return Queries.InsertInstance;

				case ModelTables.InstanceAccounts:
					return Queries.InsertInstanceAccount;

				case ModelTables.Roles:
					return Queries.InsertRole;

				case ModelTables.Subscriptions:
                    return Queries.InsertSubscription;

                case ModelTables.SubscriptionAccounts:
                    return Queries.InsertSubscriptionAccount;

				case ModelTables.SubscriptionDatabases:
					return Queries.InsertSubscriptionDatabase;

				case ModelTables.SubscriptionExternalSources:
					return Queries.InsertSubscriptionExternalSource;

				case ModelTables.SecurityTokens:
					return Queries.InsertSecurityToken;

				case ModelTables.SubscriptionInstances:
					return Queries.InsertSubscriptionInstances;

				default:
                    return String.Empty;
            }
        }

		//--------------------------------------------------------------------------------
		public static string GetDeleteQueryByModel(ModelTables table)
		{
			switch (table)
			{
				case ModelTables.None:
					return String.Empty;

				case ModelTables.Accounts:
					return Queries.DeleteAccount;

				case ModelTables.AccountRoles:
					return Queries.DeleteAccountRole;

				case ModelTables.Instances:
					return Queries.DeleteInstance;

				case ModelTables.InstanceAccounts:
					return Queries.DeleteInstanceAccount;

				case ModelTables.Roles:
					return Queries.DeleteRole;

				case ModelTables.Subscriptions:
					return String.Empty;

				case ModelTables.SubscriptionAccounts:
					return String.Empty;

				case ModelTables.SubscriptionDatabases:
					return Queries.DeleteSubscriptionDatabase;

				case ModelTables.SubscriptionExternalSources:
					return String.Empty;

				case ModelTables.SecurityTokens:
					return String.Empty;

				case ModelTables.SubscriptionInstances:
					return String.Empty;

				default:
					return String.Empty;
			}
		}

		//--------------------------------------------------------------------------------
		public static ModelTables GetModelTable(string modelName)
        {
            switch (modelName.ToUpperInvariant())
            {
                case "ACCOUNTS":
                    return ModelTables.Accounts;
                case "SUBSCRIPTIONS":
                    return ModelTables.Subscriptions;
                case "ROLES":
                    return ModelTables.Roles;
                case "REGISTEREDAPPS":
                    return ModelTables.RegisteredApps;
                case "RECOVERYCODES":
                    return ModelTables.RecoveryCode;
                case "INSTANCES":
                    return ModelTables.Instances;
				case "SUBSCRIPTIONINSTANCES":
					return ModelTables.SubscriptionInstances;
                default:
                    return ModelTables.None;
            }
        }

	}
}