using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class InstanceAccount : IInstanceAccount
	{
        public int accountId;
        public int instanceId;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public int AccountId { get { return this.accountId; } set { this.accountId = value; } }
        public int InstanceId { get { return this.instanceId; } set { this.instanceId = value; } }

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
		public bool Save()
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
