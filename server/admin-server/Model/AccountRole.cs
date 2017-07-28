using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using System;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class AccountRoles : IAccountRoles, IModelObject
    {
        string accountName;
        int roleId;
        string entityKey;

        public string AccountName { get => accountName; set => accountName = value; }
        public int RoleId { get => roleId; set => roleId = value; }
        public string EntityKey { get => entityKey; set => entityKey = value; }

        //---------------------------------------------------------------------
        public OperationResult Save()
        {
			return new OperationResult();
        }

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
            AccountRoles account = new AccountRoles();
            account.AccountName = dataReader["AccountName"] as string;
            account.EntityKey= dataReader["EntityKey"] as string;
            account.RoleId = (int)dataReader["RoleId"];
            return account;
        }

        //--------------------------------------------------------------------------------
        public string GetKey()
        {
            return String.Concat(" ( AccountName = '", this.accountName, "' ) ");
        }
    }
}
