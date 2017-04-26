using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IUserAccount
    {
        string Id { get; }
        string Name { get; }
        bool IsAdmin { get; }
        string Password { get; }
        bool PasswordNeverExpires { get; }
        bool UserMustChangePassword { get; }
        bool UserCannotChangePassword { get; }
        DateTime ExpireDatePassword { get; set; }
        bool Disabled { get; }
        string PreferredLanguage { get; set; }
        string ApplicationLanguage { get; set; }
        bool IsWindowsAuthentication { get; }
        bool IsProvisioningAdmin { get; set; }
    }
}
