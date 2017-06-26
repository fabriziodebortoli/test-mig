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
		bool existsOnDB;
		string url;
		string appName;

		//---------------------------------------------------------------------
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public URLType URLType { get { return this.urlType; } set { this.urlType = value; } }
		public string URL { get { return this.url; } set { this.url = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }
		public string AppName { get { return this.appName; } set { this.appName = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public ServerURL()
		{
			this.instanceKey = String.Empty;
			this.urlType = new URLType();
			this.url = String.Empty;
			this.appName = String.Empty;
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
	}
}
