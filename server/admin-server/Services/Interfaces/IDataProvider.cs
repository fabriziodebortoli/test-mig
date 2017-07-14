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
		OperationResult Query(QueryInfo qi);

		// database-dependent values
		DateTime MinDateTimeValue { get; }
    }

	//================================================================================
	public interface IInstanceDataProvider : IDataProvider
	{
		List<ServerURL> LoadURLs(string instanceKey);
	}

	//================================================================================
	public interface IAccountDataProvider : IDataProvider
	{
		List<Account> GetAccounts(string accountName);
		List<Account> GetAccountsBySubscription(string subscriptionKey);
	}

	//================================================================================
	public interface ICompanyDataProvider : IDataProvider
	{
		List<Company> GetCompanies(string accountName, string subscriptionKey);
		List<Company> GetCompaniesBySubscription(string subscriptionKey);
	}

	//================================================================================
	public interface ISubscriptionDataProvider : IDataProvider
	{
		List<Subscription> GetSubscriptions(string instanceKey);
		List<Subscription> GetSubscriptionsByAccount(string accountName, string instanceKey);
	}
}
