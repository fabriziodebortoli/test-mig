using Microarea.Common;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;

namespace Microarea.TbfWebGate.Authorization
{
    public class TbfAuthorizationRequirement : IAuthorizationRequirement
    {
        public UserInfo GetLoginInformation(HttpRequest request, ISession session)
        {
            string authenticationtoken = AutorizationHeaderManager.GetAuthorizationElement(request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrWhiteSpace(authenticationtoken))
            {
                return null;
            }

            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = session;
            }
            catch (Exception)
            {
            }

            var loginInfo = LoginInfoMessage.GetLoginInformation(hsession, authenticationtoken);

            var ui = new UserInfo(loginInfo, authenticationtoken);

            return ui;
        }
    }
}