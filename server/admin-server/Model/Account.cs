﻿using System;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Account : IAccount
    {
        // model attributes

        int accountId;
        string name = string.Empty;
		string password = string.Empty;
		string description = string.Empty;
		string email = string.Empty;
		bool provisioningAdmin;
		bool passwordNeverExpires = false;
		bool mustChangePassword = false;
		bool cannotChangePassword = false;
		bool expiryDateCannotChange = false;
		DateTime expiryDatePassword = (DateTime)SqlDateTime.MinValue;
		bool disabled = false;
		bool locked = false;
		string applicationLanguage = string.Empty;
        string preferredLanguage = string.Empty;
        bool isWindowsAuthentication = false;

		//---------------------------------------------------------------------
		public int AccountId { get { return this.accountId; } set { this.accountId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string Email { get { return this.email; } set { this.email = value; } }
		public bool ProvisioningAdmin { get { return this.provisioningAdmin; } set { this.provisioningAdmin = value; } }
		public bool PasswordNeverExpires { get { return this.passwordNeverExpires; } set { this.passwordNeverExpires = value; } }
		public bool MustChangePassword { get { return this.mustChangePassword; } set { this.mustChangePassword = value; } }
		public bool CannotChangePassword { get { return this.cannotChangePassword; } set { this.cannotChangePassword = value; } }
		public bool ExpiryDateCannotChange { get { return this.expiryDateCannotChange; } set { this.expiryDateCannotChange = value; } }
		public DateTime ExpiryDatePassword { get { return this.expiryDatePassword; } set { this.expiryDatePassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool Locked { get { return this.locked; } set { this.locked = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
        public bool IsWindowsAuthentication { get { return this.isWindowsAuthentication; } set { this.isWindowsAuthentication = value; } }

        // data provider
        IDataProvider dataProvider;

        public IDataProvider DataProvider { get { return dataProvider; } set { dataProvider = value; } }

        //---------------------------------------------------------------------
        public bool Save(string connectionString)
		{
            return this.dataProvider.Save(this, connectionString);
		}
	}
}
