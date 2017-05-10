using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount
    {
        int AccountId { get; }
        string Name { get; }
		string Password { get; }
		string Description { get; }
		string Email { get; }
		bool ProvisioningAdmin { get; }
        bool PasswordNeverExpires { get; }
        bool MustChangePassword { get; }
        bool CannotChangePassword { get; }
		bool ExpireDateCannotChange { get; }
		DateTime ExpireDatePassword { get; set; }
        bool Disabled { get; }
		bool Locked { get; }
		string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }
    }
}
