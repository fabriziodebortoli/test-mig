using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Instance : IInstance
    {
        string instanceKey;
        string description = string.Empty;
        string origin = string.Empty;
        string tags = string.Empty;
        bool disabled = false;
		bool existsOnDB = false;

		//---------------------------------------------------------------------
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
        public string Description { get { return this.description; } set { this.description = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }
        public string Origin { get => origin; set => origin = value; }
        public string Tags { get => tags; set => tags = value; }

        // data provider
        IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Instance()
		{
		}

		//---------------------------------------------------------------------
		public Instance(string instanceKey)
		{
			this.instanceKey = instanceKey;
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public OperationResult Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public IAdminModel Load()
		{
			return this.dataProvider.Load(this);
		}

		//---------------------------------------------------------------------
		public List<ServerURL> LoadURLs()
		{

			return ((IInstanceDataProvider)this.dataProvider).LoadURLs(this.instanceKey);
		}
	}
}
