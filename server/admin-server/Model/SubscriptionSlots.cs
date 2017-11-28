using System;
using System.Data;

using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionSlots : IModelObject
    {
        string subscriptionKey;
        string value = string.Empty;

        //---------------------------------------------------------------------
        public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
        public string Value { get { return this.value; } set { this.value = value; } }

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
        public OperationResult Save(BurgerData burgerData)
        {
            throw new NotImplementedException();
        }


        //---------------------------------------------------------------------
        public IModelObject Fetch(IDataReader reader)
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        public string GetKey()
        {
            throw new NotImplementedException();
        }

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			throw new NotImplementedException();
		}
	}
}
