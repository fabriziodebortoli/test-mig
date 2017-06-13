using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Library
{
    public class UserTokens
    {
        SecurityToken apiSecurityToken = SecurityToken.Empty;
        SecurityToken authenticationToken = SecurityToken.Empty;

        public string ApiSecurityToken { get; set; }
        public string AuthenticationToken { get; set; }

        //---------------------------------------------------------------------
        public UserTokens(bool isAdmin, int accountid)
        {
            if (isAdmin) apiSecurityToken = SecurityToken.GetToken(TokenType.API, accountid);
            authenticationToken = SecurityToken.GetToken(TokenType.Authentication, accountid);
        }

        internal bool  Save()
        {
            return apiSecurityToken.Save() && authenticationToken.Save();
        }
    }
}
