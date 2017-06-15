using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount : IAdminModel
    {
        string AccountName { get; }
        string FullName { get; set; }
        string Password { get; set; }
        int LoginFailedCount { get; set; }
        string Notes { get; set; }
        string Email { get; set; }
        bool ProvisioningAdmin { get; set; }
        bool PasswordNeverExpires { get; }
        bool MustChangePassword { get; }
        bool CannotChangePassword { get; }
        bool PasswordExpirationDateCannotChange { get; }
        DateTime PasswordExpirationDate { get; set; }
        bool Disabled { get; set; }
        bool Locked { get; set; }
        string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }

        bool IsPasswordExpirated();
    }

}
