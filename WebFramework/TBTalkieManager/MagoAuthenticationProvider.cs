using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;


using Microarea.Library;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
 
namespace Microarea.TBTalkieManager
{
    //=========================================================================
    class MagoAuthenticationProvider : IAuthProvider 
    {
        LoginManager loginManagerWrapper = null;

        //Provvisoria per IDE
        bool IsIDE = false;

        //---------------------------------------------------------------------
        public MagoAuthenticationProvider(string aLoginManagerURL)
        {
            if (aLoginManagerURL != null )
                loginManagerWrapper = new LoginManager(aLoginManagerURL, 10000);
            else
                IsIDE = true;
        }

        #region IAuthProvider Members
        
        //---------------------------------------------------------------------
        public bool IsTokenValid(string aAuthToken)
        {
            if(IsIDE)
                return true;

            return loginManagerWrapper.IsValidToken(aAuthToken);
        }

        //---------------------------------------------------------------------
        public ICollection<string> GetAllUsers()
        {
            if (IsIDE)
            {
                List<string> usersIDE = new List<string>();
                usersIDE.Add("Merlo");
                usersIDE.Add("grillo");
                usersIDE.Add("Manzoni");
                usersIDE.Add("Patrucco");
                usersIDE.Add("Canessa");

                return usersIDE;
            }

            string[] users = null;
            List<string> userList = new List<string>();

            if (loginManagerWrapper == null)
                return users;

            users = loginManagerWrapper.EnumAllUsers();

            foreach (string str in users)
                userList.Add(str);            

            return userList;
        }

        #endregion
    }
}