
namespace Microarea.AdminServer
{
	//=========================================================================
	public class Consts
    {
		// Account SQL statements
		public const string InsertAccount = @"INSERT INTO MP_Accounts (Name, Password, Description, Email, PasswordNeverExpires, MustChangePassword, CannotChangePassword, 
			ExpiryDateCannotChange, ExpiryDatePassword, Disabled, Locked, ProvisioningAdmin, WindowsAuthentication, PreferredLanguage, ApplicationLanguage) 
			VALUES (@Name, @Password, @Description, @Email, @PasswordNeverExpires, @MustChangePassword, @CannotChangePassword, 
			@ExpiryDateCannotChange, @ExpiryDatePassword, @Disabled, @Locked, @ProvisioningAdmin, @WindowsAuthentication, @PreferredLanguage, @ApplicationLanguage)";

		public const string UpdateAccount = @"UPDATE MP_Accounts SET Name = @Name, Password = @Password, Description = @Description, Email = @Email, 
			PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
			ExpiryDateCannotChange = @ExpiryDateCannotChange, ExpiryDatePassword = @ExpiryDatePassword, Disabled = @Disabled, Locked = @Locked, 
			ProvisioningAdmin = @ProvisioningAdmin, WindowsAuthentication = @WindowsAuthentication, PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage) 
			WHERE AccountId = @AccountId";

		public const string DeleteAccount = @"DELETE MP_Accounts WHERE AccountId = @AccountId";
		//
	}
}
