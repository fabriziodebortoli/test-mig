using System;

namespace Microarea.AdminServer.Interfaces
{
    //================================================================================
    public interface IUserAccount
    {
        string Id { get; set; }
        string Name { get; set; }
        string Password { get; set; }
        bool PasswordNeverExpires { get; set; }
        bool UserMustChangePassword { get; set; }
        bool UserCannotChangePassword { get; set; }
        DateTime ExpireDatePassword { get; set; }
        bool Disabled { get; set; }
        string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; set; }
        bool IsProvisioningAdmin { get; set; }
    }
}
