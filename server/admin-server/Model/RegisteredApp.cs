using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class RegisteredApp : IRegisteredApp, IModelObject
    {
        public string AppId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string SecurityValue { get; set; }

		//---------------------------------------------------------------------
		public RegisteredApp()
		{
			AppId = string.Empty;
			SecurityValue = string.Empty;
			Description = string.Empty;
			URL = string.Empty;
			Name = string.Empty;
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            // data parameters
            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
            burgerDataParameters.Add(new BurgerDataParameter("@AppId", this.AppId));
            burgerDataParameters.Add(new BurgerDataParameter("@Name", this.Name));
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.Description));
            burgerDataParameters.Add(new BurgerDataParameter("@URL", this.URL));
            burgerDataParameters.Add(new BurgerDataParameter("@SecurityValue", this.SecurityValue));

            // keys parameter
            BurgerDataParameter keyColumnAppId = new BurgerDataParameter("@AppId", this.AppId);

            // saving
            opRes.Result = burgerData.Save(ModelTables.Accounts, keyColumnAppId, burgerDataParameters);
            return opRes;
        }

        //---------------------------------------------------------------------
        public IModelObject Fetch(IDataReader reader)
        {
			RegisteredApp registeredApp = new RegisteredApp
			{
				AppId = reader["AppId"] as string,
				SecurityValue = reader["SecurityValue"] as string,
				Description = reader["Description"] as string,
				URL = reader["URL"] as string,
				Name = reader["Name"] as string
			};
			return registeredApp;
        }

        //---------------------------------------------------------------------
        public string GetKey()
        {
            throw new NotImplementedException();
        }
    }
}
