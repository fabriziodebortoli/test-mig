using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Microarea.TBTalkieManager
{
    //=========================================================================
    class DummyAuthenticationProvider : IAuthProvider
    {
        #region IAuthProvider Members

        //---------------------------------------------------------------------
        public bool IsTokenValid(string aAuthToken)
        {
            return true;
        }

		//---------------------------------------------------------------------
        public ICollection<string> GetAllUsers()
		{
            List<string> users = new List<string>();

            users.Add("Manzoni");
            users.Add("Calandrini");
            users.Add("Canessa");

			return users;
		}

		#endregion
	}
}
