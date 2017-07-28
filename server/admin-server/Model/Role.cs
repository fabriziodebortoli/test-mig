using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Role : IRole
    {
        string roleName;
        int roleId;
        string description;
        int parentRoleId;
        string level;
        bool disabled;
        bool existsOnDB;
        IDataProvider dataProvider;

        public string RoleName { get => roleName; set => roleName = value; }
        public int RoleId { get => roleId; set => roleId = value; }
        public string Description { get => description; set => description = value; }
        public bool Disabled { get => disabled; set => disabled = value; }
        public bool ExistsOnDB { get => existsOnDB; set => existsOnDB = value; }
        public int ParentRoleId { get => parentRoleId; set => parentRoleId = value; }
        public string Level { get => level; set => level = value; }

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

    //================================================================================
    public static class RolesStrings
    {
        public static string CloudAdmin = "CloudAdmin";
        public static string ProvisioningAdmin = "ProvisioningAdmin";
        public static string AccountManager = "AccountManager";
        public static string DbManager = "DbManager";
        public static string TestDatabaseUser = "TestDatabaseUser";
        public static string NextInstanceUser = "NextInstanceUser";
        public static string PreviousInstanceUser = "PreviousInstanceUser";
        public static string WebSiteAdmin = "WebSiteAdmin";
    }
}