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

        public string AccountName { get { return accountName; } set { accountName = value; } }
        public TokenType TokenType { get { return tokenType; } set { tokenType = value; } }
        public string Token { get { return token; } set { token = value; } }

        internal static SecurityToken GetToken(TokenType type, string accountName)
        {
           SecurityToken t = new SecurityToken();
            t.accountName = accountName;
            t.token  = Guid.NewGuid().ToString();
            t.expired = false;
            t.expirationDate = DateTime.MaxValue;
            t.TokenType = type;

            return t;
        }

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
        public bool Save()
        {
            return this.dataProvider.Save(this);
        }

        //---------------------------------------------------------------------
        public void Load()
        {
            this.dataProvider.Load(this);
        }
        //---------------------------------------------------------------------
        public static SecurityToken Empty
        {
            get { 
            return new SecurityToken();}
        }
    }
}
