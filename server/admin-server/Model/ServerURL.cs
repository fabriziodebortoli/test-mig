using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class ServerURL : IServerURL
	{
		string instanceKey;
		URLType urlType = URLType.API;
		bool existsOnDB = false;
		string url = string.Empty;

		//---------------------------------------------------------------------
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public URLType URLType { get { return this.urlType; } set { this.urlType = value; } }
		public string URL { get { return this.url; } set { this.url = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public ServerURL()
		{
		}

		//---------------------------------------------------------------------
		public IAdminModel Load()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public OperationResult Save()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}
	}
}
