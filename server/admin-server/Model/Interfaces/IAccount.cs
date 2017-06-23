using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount : IAdminModel
    {
        string AccountName { get; }
		string Password { get; set; }
        bool CloudAdmin{ get; set; }
        string FullName { get; set; }
        string Notes { get; set; }
        string Email { get; set; }
        bool ProvisioningAdmin { get; set; }
		int LoginFailedCount { get; set; }
		bool PasswordNeverExpires { get; set; }
        bool MustChangePassword { get; set; }
        bool CannotChangePassword { get; set; }
        DateTime PasswordExpirationDate { get; set; }
		int PasswordDuration { get; set; }
		bool Disabled { get; set; }
        bool Locked { get; set; }
        string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }
        long Ticks { get; }
        bool IsPasswordExpirated();
		DateTime ExpirationDate { get; set; }
        bool IsAdmin { get; }
    }
}
