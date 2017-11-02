using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Data;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IModelObject
    {
        OperationResult Save(BurgerData burgerData);
        IModelObject Fetch(IDataReader reader);
        string GetKey();
    }

    //================================================================================
    public class TicksHelper
    {
        //---------------------------------------------------------------------
        public static int GetTicks()
        {
            return DateTime.UtcNow.GetHashCode();
        }

        //---------------------------------------------------------------------
        public static int GetDateHashing(DateTime dateTime)
        {
            return dateTime.GetHashCode();
        }
    }
}
  
