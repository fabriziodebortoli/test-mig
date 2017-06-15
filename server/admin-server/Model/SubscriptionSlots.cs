using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionSlots : IAdminModel
    {
        int subscriptionId;
        string value = string.Empty;
        

        //---------------------------------------------------------------------
        public int SubscriptionId { get { return this.subscriptionId; } set { this.subscriptionId = value; } }
        public string Value { get { return this.value; } set { this.value = value; } }
       
        // data provider
        IDataProvider dataProvider;

        //---------------------------------------------------------------------
        public SubscriptionSlots()
        {
        }

        //---------------------------------------------------------------------
        public SubscriptionSlots(string subscriptionSlotsValue)
        {
            this.value = subscriptionSlotsValue;
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
