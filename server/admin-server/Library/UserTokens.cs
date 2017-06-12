using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Library
{
    public class UserTokens : ITokens
    {
        string apiSecurityToken;
        string authenticationToken;

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
            return new Guid().ToString();
        }
    }
}