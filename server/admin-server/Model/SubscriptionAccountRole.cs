using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class AccountRoles : IAccountRoles
    {
        string accountName;
        int roleId;
        string entityKey;
        bool existsOnDB;
        IDataProvider dataProvider;

        public bool ExistsOnDB { get => existsOnDB; set => existsOnDB = value; }
        public string AccountName { get => accountName; set => accountName = value; }
        public int RoleId { get => roleId; set => roleId = value; }
        public string EntityKey { get => entityKey; set => entityKey = value; }

        //---------------------------------------------------------------------
        public void SetDataProvider(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        //---------------------------------------------------------------------
        public OperationResult Save()
        {
            return this.dataProvider.Save(this);
        }

        //---------------------------------------------------------------------
        public IAdminModel Load()
        {
            return this.dataProvider.Load(this);
        }

        //---------------------------------------------------------------------
        public OperationResult Query(QueryInfo qi)
        {
            return this.dataProvider.Query(qi);
        }
    }
}
