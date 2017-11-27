using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class InstanceAccount : IInstanceAccount, IModelObject
	{
		string accountName;
		string instanceKey;
		int ticks;
		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public int Ticks { get { return this.ticks; } set { this.ticks = value; } }
		//---------------------------------------------------------------------
		public InstanceAccount()
		{
			this.ticks = 0;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();
			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			BurgerDataParameter keyColumnAccountName = new BurgerDataParameter("@AccountName", this.accountName);
			BurgerDataParameter keyColumnInstanceKey = new BurgerDataParameter("@InstanceKey", this.instanceKey);
			BurgerDataParameter columnTicks = new BurgerDataParameter("@Ticks", this.ticks);

			burgerDataParameters.Add(keyColumnAccountName);
			burgerDataParameters.Add(keyColumnInstanceKey);
			burgerDataParameters.Add(columnTicks);

			BurgerDataParameter[] keyColumnsParameter = new BurgerDataParameter[]
			{
				keyColumnAccountName,
				keyColumnInstanceKey
			};

			opRes.Result = burgerData.Save(ModelTables.InstanceAccounts, keyColumnsParameter, burgerDataParameters);
			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			InstanceAccount instanceAccount = new InstanceAccount
			{
				accountName = reader["AccountName"] as string,
				instanceKey = reader["InstanceKey"] as string
			};

			return instanceAccount;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new NotImplementedException();
		}
	}
}

