using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Services.BurgerData;
using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IAccount
    {
        string AccountName { get; set; }
		string Password { get; set; }
		byte[] Salt { get; set; }
		string FullName { get; set; }
        string Notes { get; set; }
        string Email { get; set; }
		int LoginFailedCount { get; set; }
		bool PasswordNeverExpires { get; set; }
        bool MustChangePassword { get; set; }
        bool CannotChangePassword { get; set; }
        DateTime PasswordExpirationDate { get; set; }
		int PasswordDuration { get; set; }
		bool Disabled { get; set; }
        bool Locked { get; set; }
        string Language { get; set; }
        string RegionalSettings { get; set; }
        bool IsWindowsAuthentication { get; set; }
        int Ticks { get; set; }
        bool IsPasswordExpirated();
		DateTime ExpirationDate { get; set; }
		string ParentAccount { get; set; }
		bool Confirmed { get; set; }
		void ResetPasswordExpirationDate();
        LoginReturnCodes VerifyCredential(string password, BurgerData burgerdata);
    }
}
