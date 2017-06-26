using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        IAdminModel Load(IAdminModel iModel);
        OperationResult Save(IAdminModel iModel);
        bool Delete(IAdminModel iModel);

        // database-dependent values
        DateTime MinDateTimeValue { get; }
    }

	public interface IInstanceDataProvider
	{
		List<ServerURL> LoadURLs();
	}
}
