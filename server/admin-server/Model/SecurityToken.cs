using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using System;

namespace Microarea.AdminServer.Model
{
    public class SecurityToken : ISecurityToken
    {
        string accountName = null;
        TokenType tokenType = 0;
        string token = Guid.Empty.ToString();
        bool expired = true;
        DateTime expirationDate = DateTime.MinValue;
		bool existsOnDB = false;

        public string AccountName { get { return accountName; } set { accountName = value; } }
        public TokenType TokenType { get { return tokenType; } set { tokenType = value; } }
        public string Token { get { return token; } set { token = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

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
			t.expirationDate = DateTime.Now.AddDays(7);// default: dura una settinama
			t.TokenType = type;
			return t;
        }

        //---------------------------------------------------------------------
        public bool Expired { get { return expired; } set { expired = value; } }
        public DateTime ExpirationDate { get { return expirationDate; } set { expirationDate = value; } }
        // data provider
        IDataProvider dataProvider;

        //---------------------------------------------------------------------
        public void SetDataProvider(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            // setting database-dependent values
            this.expirationDate = this.dataProvider.MinDateTimeValue;//default value
        }

        //---------------------------------------------------------------------
        public OperationResult Save()
        {
            return this.dataProvider.Save(this);
        }

        //---------------------------------------------------------------------
        public IAdminModel Load()
        {
            return this.dataProvider.Load(this);
        }

        //---------------------------------------------------------------------
        public bool IsValid
        {
            //expired potrebbe essere impostato da fuori a true o se data scadenza passata
            get {
                if (String.IsNullOrEmpty(token) || 
                    String.IsNullOrEmpty(accountName) || 
                    tokenType == TokenType.Undefined)
                    return false;

                if (expired) return false;

                if (expirationDate < DateTime.Now)
                    expired = true;
                
                return !expired;
            }
        }
    }
}
