using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Instance : IInstance
    {
        int instanceId;
        string name = string.Empty;
		string customer = string.Empty;
		bool disabled = false;

		//---------------------------------------------------------------------
		public int InstanceId { get { return this.instanceId; } set { this.instanceId = value; } }
        public string Name { get { return this.name; } set { this.name = value; } }
		public string Customer { get { return this.customer; } set { this.customer = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Instance()
		{
		}

		//---------------------------------------------------------------------
		public Instance(string instanceName)
		{
			this.name = instanceName;
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public void Load()
		{
			this.dataProvider.Load(this);
		}
	}
}
