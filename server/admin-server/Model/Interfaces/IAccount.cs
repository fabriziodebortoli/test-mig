using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount : IAdminModel
    {
		int AccountId { get; }
<<<<<<< HEAD
		string AccountName { get; set; }
		string FullName { get; }
		string Password { get; set; }
		string Notes { get; }
=======
		string AccountName { get; }
		string FullName { get; set; }
		string Password { get; set; }
		string Notes { get; set; }
>>>>>>> master
		string Email { get; set; }
		bool ProvisioningAdmin { get; set; }
		bool PasswordNeverExpires { get; }
        bool MustChangePassword { get; }
        bool CannotChangePassword { get; }
		bool ExpiryDateCannotChange { get; }
		DateTime ExpiryDatePassword { get; set; }
        bool Disabled { get; set; }
		bool Locked { get; set; }
		string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }
    }
}
