using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class ServerURL : IServerURL
	{
		int instanceId;
		URLType urlType = URLType.API;

		string url = string.Empty;

		//---------------------------------------------------------------------
		public int InstanceId { get { return this.instanceId; } set { this.instanceId = value; } }
		public URLType URLType { get { return this.urlType; } set { this.urlType = value; } }
		public string URL { get { return this.url; } set { this.url = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public ServerURL()
		{
		}

		//---------------------------------------------------------------------
		public void Load()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public bool Save()
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
