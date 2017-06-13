using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Library
{
    public class UserTokens
    {
        string apiSecurityToken = string.Empty;
        string authenticationToken = string.Empty;

        public string ApiSecurityToken { get; set; }
        public string AuthenticationToken { get; set; }

        //---------------------------------------------------------------------
        public UserTokens(bool isAdmin)
        {
            if (isAdmin) apiSecurityToken = GetToken();
            authenticationToken = GetToken();
        }

        //---------------------------------------------------------------------
        private string GetToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
