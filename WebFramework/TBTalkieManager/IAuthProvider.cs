using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.TBTalkieManager
{
    //=========================================================================
    public interface IAuthProvider
    {
        //---------------------------------------------------------------------
        bool IsTokenValid(string aAuthToken);

		//---------------------------------------------------------------------
        ICollection<string> GetAllUsers();   
    }
}
