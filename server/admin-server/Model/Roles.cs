using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using System;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Roles : IRoles
    {
        string roleName;
        int roleId;
        string description;
        bool disabled;
        bool existsOnDB;
        IDataProvider dataProvider;

        public string RoleName { get => roleName; set => roleName = value; }
        public int RoleId { get => roleId; set => roleId = value; }
        public string Description { get => description; set => description = value; }
        public bool Disabled { get => disabled; set => disabled = value; }
        public bool ExistsOnDB { get => existsOnDB; set => existsOnDB = value; }

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
