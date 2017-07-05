using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using Microarea.AdminServer.Services;
using System.Collections.Generic;

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

        //---------------------------------------------------------------------
        public UserTokens(bool isAdmin, string accountName)
        {
            if (isAdmin) apiSecurityToken = SecurityToken.GetToken(TokenType.API, accountName);
           
            authenticationToken = SecurityToken.GetToken(TokenType.Authentication, accountName);
        }

        //---------------------------------------------------------------------
        internal bool Save()
        {
            return apiSecurityToken.Save().Result && authenticationToken.Save().Result;
        }

        //---------------------------------------------------------------------
        internal void Setprovider(IDataProvider tokenSQLDataProvider)
        {
            apiSecurityToken.SetDataProvider(tokenSQLDataProvider);
            authenticationToken.SetDataProvider(tokenSQLDataProvider);
        }

		//---------------------------------------------------------------------
		internal bool IsEmpty()
        {
            throw new NotImplementedException();
        }

		//---------------------------------------------------------------------
		public List<SecurityToken> GetTokenList(bool isAdmin, string accountName)
		{
			List<SecurityToken> tokenList = new List<SecurityToken>();

			if (isAdmin)
			{
				tokenList.Add(SecurityToken.GetToken(TokenType.API, accountName));
			}

			tokenList.Add(SecurityToken.GetToken(TokenType.Authentication, accountName));
			return tokenList;
		}
	}
}

