using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Library
{
    public class UserTokens
    {
        SecurityToken apiSecurityToken = SecurityToken.Empty;
        SecurityToken authenticationToken = SecurityToken.Empty;

        public string ApiSecurityToken { get { return this.apiSecurityToken.Token; } }
        public string AuthenticationToken { get { return this.authenticationToken.Token; } }

		//---------------------------------------------------------------------
		public UserTokens()
		{
			apiSecurityToken = new SecurityToken();
			authenticationToken = new SecurityToken();
		}

		public UserTokens(bool isAdmin, string accountName)
        {
            if (isAdmin) apiSecurityToken = SecurityToken.GetToken(TokenType.API, accountName);
            authenticationToken = SecurityToken.GetToken(TokenType.Authentication, accountName);
        }

        internal bool  Save()
        {
            return apiSecurityToken.Save() && authenticationToken.Save();
        }

        internal void Setprovider(IDataProvider tokenSQLDataProvider)
        {
             apiSecurityToken.SetDataProvider(tokenSQLDataProvider);
            authenticationToken.SetDataProvider(tokenSQLDataProvider);
        }
    }
}
