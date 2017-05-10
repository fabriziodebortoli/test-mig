using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Account : IAccount
    {
        int accountId;
        string name = string.Empty;
		string password = string.Empty;
		string description = string.Empty;
		string email = string.Empty;
		bool provisioningAdmin;
		bool passwordNeverExpires = false;
		bool mustChangePassword = false;
		bool cannotChangePassword = false;
		bool expireDateCannotChange = false;
		DateTime expireDatePassword = DateTime.MinValue;
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
		public bool ExpireDateCannotChange { get { return this.expireDateCannotChange; } set { this.expireDateCannotChange = value; } }
		public DateTime ExpireDatePassword { get { return this.expireDatePassword; } set { this.expireDatePassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool Locked { get { return this.locked; } set { this.locked = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
        public bool IsWindowsAuthentication { get { return this.isWindowsAuthentication; } set { this.isWindowsAuthentication = value; } }
    }
}
