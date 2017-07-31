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
                case ModelTables.RegisteredApps:
                    return "MP_RegisteredApps";
                case ModelTables.Instances:
                    return "MP_Instances";
                case ModelTables.RecoveryCode:
                    return "MP_RecoveryCodes";
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

				case ModelTables.Roles:
					return Queries.ExistRole;

				case ModelTables.Subscriptions:
                    return Queries.ExistSubscription;

                case ModelTables.SubscriptionAccounts:
                    return Queries.ExistSubscriptionAccount;

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

				case ModelTables.Roles:
					return Queries.UpdateRole;

				case ModelTables.Subscriptions:
                    return Queries.UpdateSubscription;

                case ModelTables.SubscriptionAccounts:
                    return String.Empty;

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

				case ModelTables.Roles:
					return Queries.InsertRole;

				case ModelTables.Subscriptions:
                    return Queries.InsertSubscription;

                case ModelTables.SubscriptionAccounts:
                    return Queries.InsertSubscriptionAccount;

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

                default:
                    return ModelTables.None;
            }
        }
    }
}