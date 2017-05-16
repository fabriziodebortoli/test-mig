using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount : IAdminModel
    {
		int AccountId { get; }
		string AccountName { get; set; }
		string FullName { get; }
		string Password { get; set; }
		string Notes { get; }
		string Email { get; set; }
		bool ProvisioningAdmin { get; }
        bool PasswordNeverExpires { get; }
        bool MustChangePassword { get; }
        bool CannotChangePassword { get; }
		bool ExpiryDateCannotChange { get; }
		DateTime ExpiryDatePassword { get; set; }
        bool Disabled { get; }
		bool Locked { get; }
		string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }
    }
}
