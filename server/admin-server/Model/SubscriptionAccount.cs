using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionAccount : ISubscriptionAccount
	{
        public string accountName;
		public int subscriptionId;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public int SubscriptionId { get { return this.subscriptionId; } set { this.subscriptionId = value; } }

		//---------------------------------------------------------------------
		public SubscriptionAccount()
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
