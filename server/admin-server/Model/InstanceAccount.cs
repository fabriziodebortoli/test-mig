using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class InstanceAccount : IInstanceAccount
	{
        public string accountName;
		public string instanceKey;

		bool existsOnDB = false;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		//---------------------------------------------------------------------
		public InstanceAccount()
		{

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
