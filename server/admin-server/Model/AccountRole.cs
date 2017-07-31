using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class AccountRoles : IAccountRoles, IModelObject
    {
		string roleName;
		string accountName;
        string entityKey;
		string level;

		public string RoleName { get => roleName; set => roleName = value; }
		public string AccountName { get => accountName; set => accountName = value; }
        public string EntityKey { get => entityKey; set => entityKey = value; }
		public string Level { get => level; set => level = value; }

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
        {
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@RoleName", this.roleName));
			burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.accountName));
			burgerDataParameters.Add(new BurgerDataParameter("@EntityKey", this.entityKey));
			burgerDataParameters.Add(new BurgerDataParameter("@Level", this.level));

			BurgerDataParameter roleNameKeyColumnParameter = new BurgerDataParameter("@RoleName", this.roleName);
			BurgerDataParameter accountNameKeyColumnParameter = new BurgerDataParameter("@AccountName", this.accountName);
			BurgerDataParameter entityKeyColumnParameter = new BurgerDataParameter("@EntityKey", this.entityKey);
			BurgerDataParameter[] keyParameters = new BurgerDataParameter[] {
				roleNameKeyColumnParameter,
				accountNameKeyColumnParameter,
				entityKeyColumnParameter
			};

			opRes.Result = burgerData.Save(ModelTables.AccountRoles, keyParameters, burgerDataParameters);
			return opRes;
		}

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
            AccountRoles account = new AccountRoles();
			account.RoleName = dataReader["RoleName"] as string;
			account.AccountName = dataReader["AccountName"] as string;
            account.EntityKey= dataReader["EntityKey"] as string;
			account.Level = dataReader["Level"] as string;
			return account;
        }

        //--------------------------------------------------------------------------------
        public string GetKey()
        {
            return String.Concat(" ( AccountName = '", this.accountName, "' ) ");
        }
    }
}
