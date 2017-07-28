using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;
using System;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Role : IRole, IModelObject
    {
        string roleName;
        string description;
        string parentRoleName;
        bool disabled;

        public string RoleName { get => roleName; set => roleName = value; }
        public string Description { get => description; set => description = value; }
		public string ParentRoleName { get => parentRoleName; set => parentRoleName = value; }
		public bool Disabled { get => disabled; set => disabled = value; }

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			Role role = new Role();
			role.roleName = reader["RoleName"] as string;
			role.description = reader["Description"] as string;
			role.parentRoleName = reader["ParentRoleName"] as string;
			role.disabled = (bool)reader["Disabled"];
			return role;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
        {
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@RoleName", this.roleName));
			burgerDataParameters.Add(new BurgerDataParameter("@Description", this.description));
			burgerDataParameters.Add(new BurgerDataParameter("@ParentRoleName", this.parentRoleName));
			burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.disabled));

			BurgerDataParameter keyColumnParameter = new BurgerDataParameter("@RoleName", this.roleName);

			opRes.Result = burgerData.Save(ModelTables.Instances, keyColumnParameter, burgerDataParameters);
			return opRes;
		}
    }

    //================================================================================
    public static class RolesStrings
    {
        public static string Admin = "Admin";
		public static string All = "*";
	}

	//================================================================================
	public static class RoleLevelsStrings
	{
		public static string Instance = "Instance";
		public static string Subscription = "Subscription";
	}
}