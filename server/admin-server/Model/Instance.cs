using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Instance : IInstance, IModelObject
    {
        string instanceKey = string.Empty;
        string description = string.Empty;
        string origin = string.Empty;
        string tags = string.Empty;
        bool disabled = false;
        bool underMaintenance = false;
        DateTime pendingDate;
        int ticks = TicksHelper.GetTicks();
        string verificationCode = string.Empty;
		string securityValue = string.Empty;

        public string InstanceKey { get => this.instanceKey; set => this.instanceKey = value; }
        public string Description { get => this.description;  set => this.description = value; }
		public bool Disabled { get => this.disabled; set => this.disabled = value; }
        public string Origin { get => origin; set => origin = value; }
        public string Tags { get => tags; set => tags = value; }
        public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }
        public DateTime PendingDate { get => pendingDate; set => pendingDate = value; }
        public string VerificationCode { get => verificationCode; set => verificationCode = value; }
        public int Ticks { get => ticks; set => ticks = value; }
		public string SecurityValue { get => securityValue; set => securityValue = value; }

        //---------------------------------------------------------------------
        public Instance()
		{
			this.description = String.Empty;
			this.pendingDate = BurgerData.MinDateTimeValue;
			this.securityValue = String.Empty;
		}

		//---------------------------------------------------------------------
		public Instance(string instanceKey) : this()
		{
			this.instanceKey = instanceKey;
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
            burgerDataParameters.Add(new BurgerDataParameter("@InstanceKey", this.instanceKey));
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.description));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.disabled));
            burgerDataParameters.Add(new BurgerDataParameter("@Origin", this.origin));
            burgerDataParameters.Add(new BurgerDataParameter("@Tags", this.tags));
            burgerDataParameters.Add(new BurgerDataParameter("@UnderMaintenance", this.underMaintenance));
            burgerDataParameters.Add(new BurgerDataParameter("@PendingDate", this.pendingDate));
            burgerDataParameters.Add(new BurgerDataParameter("@VerificationCode", this.verificationCode));
            burgerDataParameters.Add(new BurgerDataParameter("@Ticks", this.ticks));
			burgerDataParameters.Add(new BurgerDataParameter("@SecurityValue", this.securityValue));
			BurgerDataParameter keyColumnParameter = new BurgerDataParameter("@InstanceKey", this.instanceKey);

            opRes.Result = burgerData.Save(ModelTables.Instances, keyColumnParameter, burgerDataParameters);
            return opRes;
        }

        //---------------------------------------------------------------------
        public IModelObject Fetch(IDataReader reader)
        {
            Instance instance = new Instance
            {
                instanceKey = reader["InstanceKey"] as string,
                description = reader["Description"] as string,
                disabled = (bool)reader["Disabled"],
                origin = reader["Origin"] as string,
                tags = reader["Tags"] as string,
                underMaintenance = (bool)reader["UnderMaintenance"],
                pendingDate = (DateTime)reader["PendingDate"],
                verificationCode = reader["VerificationCode"] as string,
                ticks = (int)reader["Ticks"],
				securityValue = reader["SecurityValue"] as string
            };

            //verifico la pending date, se la data è manomessa rilascio eccezione
            if (!instance.VerifyPendingDate())
                throw new Exception(String.Format(Strings.BurgledInstance, instance.InstanceKey));

            return instance;
        }

        //---------------------------------------------------------------------
        public bool VerifyPendingDate()
        {
            return TicksHelper.GetDateHashing(PendingDate) == VerificationCode;
        }

        //---------------------------------------------------------------------
        public AuthorizationInfo GetAuthorizationInfo()
        {
            return new AuthorizationInfo(AuthorizationInfo.TypeAppName, instanceKey, securityValue);
        }
       
        //---------------------------------------------------------------------
        public string GetKey()
        {
            return String.Concat(" ( InstanceKey = '", this.InstanceKey, "' ) ");
        }
    }
}
