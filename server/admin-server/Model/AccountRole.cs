using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class AccountRoles : IAccountRoles, IModelObject
    {
		string roleName;
		string accountName;
        string entityKey;
		string level;
        int ticks = TicksHelper.GetTicks();

        public string RoleName { get => roleName; set => roleName = value; }
		public string AccountName { get => accountName; set => accountName = value; }
        public string EntityKey { get => entityKey; set => entityKey = value; }
		public string Level { get => level; set => level = value; }
        public int Ticks { get => ticks; set => ticks = value; }

        //---------------------------------------------------------------------
        public AccountRoles()
		{
			roleName = string.Empty;
			accountName = string.Empty;
			entityKey = string.Empty;
			level = string.Empty;
		}

		//--------------------------------------------------------------------------------
		public IModelObject Fetch(IDataReader dataReader)
		{
			AccountRoles account = new AccountRoles();
			account.RoleName = dataReader["RoleName"] as string;
			account.AccountName = dataReader["AccountName"] as string;
			account.EntityKey = dataReader["EntityKey"] as string;
			account.Level = dataReader["Level"] as string;
            account.Ticks = (int)dataReader["Ticks"] ;

            return account;
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
            burgerDataParameters.Add(new BurgerDataParameter("@RoleName", this.roleName));
            burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.accountName));
            burgerDataParameters.Add(new BurgerDataParameter("@EntityKey", this.entityKey));
            burgerDataParameters.Add(new BurgerDataParameter("@Level", this.level));
            burgerDataParameters.Add(new BurgerDataParameter("@Ticks", this.ticks));

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
        public string GetKey()
        {
            return String.Concat(" ( AccountName = '", this.accountName, "' ) ");
        }

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();
			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.AccountName));
			burgerDataParameters.Add(new BurgerDataParameter("@RoleName", this.RoleName));
			burgerDataParameters.Add(new BurgerDataParameter("@EntityKey", this.EntityKey));
			opRes.Result = burgerData.Delete(ModelTables.AccountRoles, burgerDataParameters);
			return opRes;
		}
	}
}
