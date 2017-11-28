using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
    //=========================================================================
    public class SecurityToken : ISecurityToken, IModelObject
	{
        string accountName = String.Empty;
        TokenType tokenType = 0;
        string token = Guid.Empty.ToString();
        bool expired = true;
        DateTime expirationDate = DateTime.MinValue;

        public string AccountName { get { return accountName; } set { accountName = value; } }
        public TokenType TokenType { get { return tokenType; } set { tokenType = value; } }
        public string Token { get { return token; } set { token = value; } }
		public bool Expired { get { return expired; } set { expired = value; } }
		public DateTime ExpirationDate { get { return expirationDate; } set { expirationDate = value; } }

		//---------------------------------------------------------------------
		public static SecurityToken Empty { get { return new SecurityToken(); } }

		//---------------------------------------------------------------------
		public bool IsValid
		{
			//expired potrebbe essere impostato da fuori a true o se data scadenza passata
			get
			{
				if (String.IsNullOrEmpty(token) ||
					String.IsNullOrEmpty(accountName) ||
					tokenType == TokenType.Undefined)
					return false;

				if (expired)
					return false;

				if (expirationDate < DateTime.Now)
					expired = true;

				return !expired;
			}
		}

		//---------------------------------------------------------------------
		public SecurityToken()
		{
			this.token = String.Empty;
		}

        //---------------------------------------------------------------------
        internal static SecurityToken GetToken(TokenType type, string accountName)
        {
			SecurityToken t = new SecurityToken();
			t.accountName = accountName;
			t.token  = Guid.NewGuid().ToString();
			t.expired = false;
			t.expirationDate = DateTime.Now.AddDays(7); // default: dura una settinama
			t.TokenType = type;
			return t;
        }

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.accountName));
			burgerDataParameters.Add(new BurgerDataParameter("@TokenType", this.tokenType));
			burgerDataParameters.Add(new BurgerDataParameter("@Token", this.token));
			burgerDataParameters.Add(new BurgerDataParameter("@Expired", this.expired));
			burgerDataParameters.Add(new BurgerDataParameter("@ExpirationDate", this.expirationDate));

			BurgerDataParameter accountNameKeyColumnParameter = new BurgerDataParameter("@AccountName", this.accountName);
			BurgerDataParameter tokenTypeKeyColumnParameter = new BurgerDataParameter("@TokenType", this.tokenType);

			opRes.Result = burgerData.Save
				(
				ModelTables.SecurityTokens,
				new BurgerDataParameter[] { accountNameKeyColumnParameter, tokenTypeKeyColumnParameter },
				burgerDataParameters
				);
			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			SecurityToken securityToken = new SecurityToken();
			securityToken.AccountName = reader["AccountName"] as string;
			securityToken.TokenType = (TokenType)reader["TokenType"];
			securityToken.Token = reader["Token"] as string;
			securityToken.Expired = (bool)reader["Expired"];
			securityToken.ExpirationDate = (DateTime)reader["ExpirationDate"];
			return securityToken;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new NotImplementedException();
		}

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			throw new NotImplementedException();
		}
	}
}
