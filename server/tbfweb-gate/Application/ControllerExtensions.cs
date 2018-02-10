using Microarea.Common;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.TbfWebGate.Application
{
    public static class ControllerExtensions
    {
        //---------------------------------------------------------------------
        public static UserInfo GetLoginInformation(this Controller @this, string elementKey, HttpRequest request, ISession session)
        {
            //return GetDummyUserInfo();

            string sAuthT = AutorizationHeaderManager.GetAuthorizationElement(request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(sAuthT))
                return null;

            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = session;
            }
            catch (Exception)
            {
            }

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(hsession, sAuthT);

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            return ui;
        }

        //---------------------------------------------------------------------
        public static UserInfo GetDummyUserInfo()
        {
            LoginInfoMessage dummyMessage = new LoginInfoMessage();

            dummyMessage.connectionString = "";
            dummyMessage.preferredLanguage = "en-us";
            dummyMessage.applicationLanguage = "en-us";
            return new UserInfo(dummyMessage, "aa");
        }
    }
}
