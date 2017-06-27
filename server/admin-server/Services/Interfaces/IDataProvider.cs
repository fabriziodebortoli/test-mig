using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services
{
	//================================================================================
	public interface IDataProvider
    {
        IAdminModel Load(IAdminModel iModel);
        OperationResult Save(IAdminModel iModel);
        bool Delete(IAdminModel iModel);

        // database-dependent values
        DateTime MinDateTimeValue { get; }
    }

	//================================================================================
	public interface IInstanceDataProvider
	{
		List<ServerURL> LoadURLs(string instanceKey);
	}

	//================================================================================
	public interface IAccountDataProvider
	{
		List<Account> GetAccounts(string accountName);
	}

	//================================================================================
	public interface ICompanyDataProvider
	{
		List<Company> GetCompanies(string accountName, string subscriptionKey);
	}
}
