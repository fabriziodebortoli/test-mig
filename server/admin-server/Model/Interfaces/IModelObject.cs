using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Data;
using System.Text;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IModelObject
    {
        OperationResult Save(BurgerData burgerData);
		OperationResult Delete(BurgerData burgerData);
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
        public static string GetDateHashing(DateTime dateTime)
        {
            string val = dateTime.Ticks.ToString();
            val += val[0]; //per confondere
            byte[] encodedBytes = Encoding.UTF8.GetBytes(val);
            return Convert.ToBase64String(encodedBytes);
        }
        
    }
}
  
