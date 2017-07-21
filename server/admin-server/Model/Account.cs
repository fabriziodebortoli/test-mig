using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Services.Providers;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Account : IAccount
    {
		// model attributes
        string accountName;
        string fullName = string.Empty;
		string password = string.Empty;
        int loginFailedCount = 0;
        string notes = string.Empty;
		string email = string.Empty;
		bool passwordNeverExpires = false;
		bool mustChangePassword = false;
		bool cannotChangePassword = false;
        DateTime passwordExpirationDate;
		int passwordDuration;
		bool disabled = false;
		bool locked = false;
		string regionalSettings = string.Empty;
        string language = string.Empty;
        bool isWindowsAuthentication = false;
        DateTime expirationDate = DateTime.Now.AddDays(3);// todo per ora scadenza 3 giorni per esempio
		string parentAccount = string.Empty;
		bool confirmed = false;
		bool existsOnDB = false;
        long ticks;

        //---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
		public string FullName { get { return this.fullName; } set { this.fullName = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
        public int LoginFailedCount { get { return this.loginFailedCount; } set { this.loginFailedCount = value; } }
        public string Notes { get { return this.notes; } set { this.notes = value; } }
		public string Email { get { return this.email; } set { this.email = value; } }
		public bool PasswordNeverExpires { get { return this.passwordNeverExpires; } set { this.passwordNeverExpires = value; } }
		public bool MustChangePassword { get { return this.mustChangePassword; } set { this.mustChangePassword = value; } }
		public bool CannotChangePassword { get { return this.cannotChangePassword; } set { this.cannotChangePassword = value; } }
		public DateTime PasswordExpirationDate { get { return this.passwordExpirationDate; } set { this.passwordExpirationDate = value; } }
		public int PasswordDuration { get { return this.passwordDuration; } set { this.passwordDuration = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool Locked { get { return this.locked; } set { this.locked = value; } }
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
        public bool IsWindowsAuthentication { get { return this.isWindowsAuthentication; } set { this.isWindowsAuthentication = value; } }
        public DateTime ExpirationDate { get { return this.expirationDate; } set { this.expirationDate = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }
        public long Ticks { get { return this.ticks; } set { this.ticks = value; } }
		public string ParentAccount { get { return this.parentAccount; } set { this.parentAccount = value; } }
		public bool Confirmed { get { return this.confirmed; } set { this.confirmed = value; } }
		// data provider
		IDataProvider dataProvider;

        //---------------------------------------------------------------------
        public Account()
        {
		}

		//---------------------------------------------------------------------
		public Account(string accountName)
        {
            this.accountName = accountName;
        }

        //---------------------------------------------------------------------
        public void SetDataProvider(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;

            // setting database-dependent values
            this.passwordExpirationDate = this.dataProvider.MinDateTimeValue;//default value
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
        public bool IsPasswordExpirated()
		{
			// La data è inferiore ad adesso, ma comunque non è il min value che è il default
            return passwordExpirationDate < DateTime.Now && 
                passwordExpirationDate > this.dataProvider.MinDateTimeValue;
        }

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			return this.dataProvider.Query(qi);
		}

        //---------------------------------------------------------------------
        public void ResetPasswordExpirationDate()
        {
            passwordExpirationDate = DateTime.Now.AddDays(passwordDuration);
        }

        //---------------------------------------------------------------------
        public List<IAccountRoles> GetRoles(string entityKey = null)
        {
            string query = String.IsNullOrEmpty(entityKey) ?
                    String.Format(
                        "SELECT * FROM mp_accountroles WHERE AccountName = '{0}' ",
                        accountName)
                        :
                    String.Format(
                        "SELECT * FROM mp_accountroles WHERE AccountName = '{0}'  AND entityKey = '{1}'",
                        accountName,
                        entityKey);
           BurgerData burgerData = new BurgerData(((AccountSQLDataProvider)dataProvider).connectionString);
           return   burgerData.GetList<AccountRoles, IAccountRoles>(//todo manca ancora fetch dell'oggetto!!
                query,
               ModelTables.Roles);

        }

        //---------------------------------------------------------------------
        public bool IsAdmin()
        {
            List<IAccountRoles>  list = GetRoles();
            foreach (AccountRoles ar in list)
                if (ar.EntityKey == RolesStrings.CloudAdmin) return true;
            return false;

        }

    }
}