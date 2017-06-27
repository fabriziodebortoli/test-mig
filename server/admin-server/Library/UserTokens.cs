using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Library
{
    public class UserTokens
    {
		bool isAdmin;
        SecurityToken apiSecurityToken;
        SecurityToken authenticationToken;

        public string ApiSecurityToken { get { return this.apiSecurityToken.Token; } }
        public string AuthenticationToken { get { return this.authenticationToken.Token; } }

		//---------------------------------------------------------------------
		public UserTokens()
		{
			apiSecurityToken = new SecurityToken();
			authenticationToken = new SecurityToken();
		}

        //---------------------------------------------------------------------
        public UserTokens(bool isAdmin, string accountName) : this()
        {
			if (isAdmin)
			{
				apiSecurityToken = SecurityToken.GetToken(TokenType.API, accountName);
			}

            authenticationToken = SecurityToken.GetToken(TokenType.Authentication, accountName);
        }

        //---------------------------------------------------------------------
        public bool Save()
        {
			if (this.isAdmin)
			{
				return apiSecurityToken.Save().Result && authenticationToken.Save().Result;
			}

			return authenticationToken.Save().Result;
		}

        //---------------------------------------------------------------------
        public void Setprovider(IDataProvider tokenSQLDataProvider)
        {
            apiSecurityToken.SetDataProvider(tokenSQLDataProvider);
            authenticationToken.SetDataProvider(tokenSQLDataProvider);
        }
    }
}
