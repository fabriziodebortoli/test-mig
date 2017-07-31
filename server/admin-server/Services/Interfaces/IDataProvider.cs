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
	public interface ISubscriptionDatabaseDataProvider : IDataProvider
	{
		List<SubscriptionDatabase> GetDatabasesBySubscription(string subscriptionKey, string name);
	}


}
