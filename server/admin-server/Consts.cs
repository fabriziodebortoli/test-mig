
namespace Microarea.AdminServer
{
	// Istruzioni SQL per la gestione delle strutture dati del model
	//=========================================================================
	public class Consts
    {
		// Instance
		public const string InsertInstance = @"INSERT INTO MP_Instances (Name, Customer, Disabled) VALUES (@Name, @Customer, @Disabled)";
		public const string UpdateInstance = @"UPDATE MP_Instances SET Name = @Name, Customer = @Customer, Disabled = @Disabled WHERE InstanceId = @InstanceId";
		public const string DeleteInstance = @"DELETE MP_Instances WHERE InstanceId = @InstanceId";
		//

		// Subscription
		public const string InsertSubscription = @"INSERT INTO MP_Subscriptions (Name, ActivationKey, PurchaseId, InstanceId) VALUES (@Name, @ActivationKey, @PurchaseId, @InstanceId)";
		public const string UpdateSubscription = @"UPDATE MP_Subscriptions SET Name = @Name, ActivationKey = @ActivationKey, PurchaseId = @PurchaseId, InstanceId = @InstanceId 
												WHERE SubscriptionId = @SubscriptionId";
		public const string DeleteSubscription = @"DELETE MP_Subscriptions WHERE SubscriptionId = @SubscriptionId";
		//

		// Company
		public const string InsertCompany = @"INSERT INTO MP_Companies (Name, Description, CompanyDBServer, CompanyDBName, CompanyDBOwner, CompanyDBPassword, Disabled,
			DatabaseCulture, IsUnicode, PreferredLanguage, ApplicationLanguage, Provider, SubscriptionId, UseDMS, DMSDBServer, DMSDBName, DMSDBOwner, DMSDBPassword) 
			VALUES (@Name, @Description, @CompanyDBServer, @CompanyDBName, @CompanyDBOwner, @CompanyDBPassword, @Disabled,
			@DatabaseCulture, @IsUnicode, @PreferredLanguage, @ApplicationLanguage, @Provider, @SubscriptionId, @UseDMS, @DMSDBServer, @DMSDBName, @DMSDBOwner, @DMSDBPassword)";

		public const string UpdateCompany = @"UPDATE MP_Companies SET Name = @Name, Description = @Description, CompanyDBServer = @CompanyDBServer, CompanyDBName = @CompanyDBName, 
			CompanyDBOwner = @CompanyDBOwner, CompanyDBPassword = @CompanyDBPassword, Disabled = @Disabled, DatabaseCulture = @DatabaseCulture, IsUnicode = @IsUnicode, 
			PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage, Provider = @Provider, SubscriptionId = @SubscriptionId, UseDMS = @UseDMS, 
			DMSDBServer = @DMSDBServer, DMSDBName = @DMSDBName, DMSDBOwner = @DMSDBOwner, DMSDBPassword = @DMSDBPassword
			WHERE CompanyId = @CompanyId";
		public const string DeleteCompany = @"DELETE MP_Companies WHERE CompanyId = @CompanyId";
        //

        // Account
        public const string SelectAccountByUserName = @"SELECT * FROM MP_Accounts WHERE UserName = @UserName";

        public const string InsertAccount = @"INSERT INTO MP_Accounts (Name, Password, Description, Email, PasswordNeverExpires, MustChangePassword, CannotChangePassword, 
			ExpiryDateCannotChange, ExpiryDatePassword, Disabled, Locked, ProvisioningAdmin, WindowsAuthentication, PreferredLanguage, ApplicationLanguage) 
			VALUES (@Name, @Password, @Description, @Email, @PasswordNeverExpires, @MustChangePassword, @CannotChangePassword, 
			@ExpiryDateCannotChange, @ExpiryDatePassword, @Disabled, @Locked, @ProvisioningAdmin, @WindowsAuthentication, @PreferredLanguage, @ApplicationLanguage)";

		public const string UpdateAccount = @"UPDATE MP_Accounts SET Name = @Name, Password = @Password, Description = @Description, Email = @Email, 
			PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
			ExpiryDateCannotChange = @ExpiryDateCannotChange, ExpiryDatePassword = @ExpiryDatePassword, Disabled = @Disabled, Locked = @Locked, 
			ProvisioningAdmin = @ProvisioningAdmin, WindowsAuthentication = @WindowsAuthentication, PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage) 
			WHERE AccountId = @AccountId";

		public const string DeleteAccount = @"DELETE MP_Accounts WHERE UserName = @UserName";
		//
	}
}
