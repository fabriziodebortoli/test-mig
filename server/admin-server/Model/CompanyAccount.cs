﻿using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class CompanyAccount : ICompanyAccount
	{
        string accountName;
		int companyId;
        bool admin;
        bool existsOnDB;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public int CompanyId { get { return this.companyId; } set { this.companyId = value; } }
        public bool Admin { get { return this.admin; } set { this.admin = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		//---------------------------------------------------------------------
		public CompanyAccount()
		{

		}

		//---------------------------------------------------------------------
		public IAdminModel Load()
		{
			return this.dataProvider.Load(this);
		}

		//---------------------------------------------------------------------
		public OperationResult Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			return this.dataProvider.Query(qi);
		}
	}
}
