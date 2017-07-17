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
		List<IServerURL> LoadURLs(string instanceKey);
	}

	//================================================================================
	public interface IAccountDataProvider : IDataProvider
	{
		List<IAccount> GetAccounts(string accountName);
		List<IAccount> GetAccountsBySubscription(string subscriptionKey);
	}

	//================================================================================
	public interface ISubscriptionDatabaseDataProvider : IDataProvider
	{
		List<SubscriptionDatabase> GetDatabasesBySubscription(string subscriptionKey, string name);
	}

	//================================================================================
	public interface ISubscriptionDataProvider : IDataProvider
	{
		List<ISubscription> GetSubscriptions(string instanceKey);
		List<ISubscription> GetSubscriptionsByAccount(string accountName, string instanceKey);
	}
}
