
namespace Microarea.AdminServer
{
	// Istruzioni SQL per la gestione delle strutture dati del model
	//=========================================================================
	public class Consts
    {
		// Instance
		public const string ExistInstance = @"SELECT COUNT(*) FROM MP_Instances WHERE InstanceId = @InstanceId";
		public const string SelectInstanceByName = @"SELECT * FROM MP_Instances WHERE Name = @Name";
		public const string InsertInstance = @"INSERT INTO MP_Instances (Name, Customer, Disabled) VALUES (@Name, @Customer, @Disabled)";
		public const string UpdateInstance = @"UPDATE MP_Instances SET Name = @Name, Customer = @Customer, Disabled = @Disabled WHERE InstanceId = @InstanceId";
		public const string DeleteInstance = @"DELETE MP_Instances WHERE InstanceId = @InstanceId";
		//

		// ServerURL
		public const string ExistServerURL = @"SELECT COUNT(*) FROM MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string SelectServerURL = @"SELECT * FROM MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string InsertServerURL = @"INSERT INTO MP_ServerURLs (InstanceId, URLType, URL) VALUES (@InstanceId, @URLType, @URL)";
		public const string UpdateServerURL = @"UPDATE MP_ServerURLs SET URL = @URL WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string DeleteServerURL = @"DELETE MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		//

		// Subscription
		public const string ExistSubscription = @"SELECT COUNT(*) FROM MP_Subscriptions WHERE SubscriptionId = @SubscriptionId";
		public const string SelectSubscriptionByName = @"SELECT * FROM MP_Subscriptions WHERE Name = @Name";
		public const string InsertSubscription = @"INSERT INTO MP_Subscriptions (Name, ActivationKey, PurchaseId, InstanceId) VALUES (@Name, @ActivationKey, @PurchaseId, @InstanceId)";
		public const string UpdateSubscription = @"UPDATE MP_Subscriptions SET Name = @Name, ActivationKey = @ActivationKey, PurchaseId = @PurchaseId, InstanceId = @InstanceId 
												WHERE SubscriptionId = @SubscriptionId";
		public const string DeleteSubscription = @"DELETE MP_Subscriptions WHERE SubscriptionId = @SubscriptionId";
		//

		// Company
		public const string ExistCompany = @"SELECT COUNT(*) FROM MP_Companies WHERE CompanyId = @CompanyId";

		public const string SelectCompanyByName = @"SELECT * FROM MP_Companies WHERE Name = @Name";

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
		public const string ExistAccount = @"SELECT COUNT(*) FROM MP_Accounts WHERE AccountId = @AccountId";

		public const string SelectAccountByAccountName = @"SELECT * FROM MP_Accounts WHERE AccountName = @AccountName";

        public const string InsertAccount = @"INSERT INTO MP_Accounts (AccountName, FullName, Password, Notes, Email, PasswordNeverExpires, MustChangePassword, CannotChangePassword, 
			ExpiryDateCannotChange, ExpiryDatePassword, Disabled, Locked, ProvisioningAdmin, WindowsAuthentication, PreferredLanguage, ApplicationLanguage) 
			VALUES (@AccountName, @FullName, @Password, @Notes, @Email, @PasswordNeverExpires, @MustChangePassword, @CannotChangePassword, 
			@ExpiryDateCannotChange, @ExpiryDatePassword, @Disabled, @Locked, @ProvisioningAdmin, @WindowsAuthentication, @PreferredLanguage, @ApplicationLanguage)";

		public const string UpdateAccount = @"UPDATE MP_Accounts SET AccountName = @AccountName, FullName = @FullName, Password = @Password, Notes = @Notes, Email = @Email, 
			PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
			ExpiryDateCannotChange = @ExpiryDateCannotChange, ExpiryDatePassword = @ExpiryDatePassword, Disabled = @Disabled, Locked = @Locked, 
			ProvisioningAdmin = @ProvisioningAdmin, WindowsAuthentication = @WindowsAuthentication, PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage) 
			WHERE AccountId = @AccountId";

		public const string DeleteAccount = @"DELETE MP_Accounts WHERE AccountId = @AccountId";
		//

		// CompanyAccount
		public const string ExistCompanyAccount = @"SELECT COUNT(*) FROM MP_CompanyAccounts WHERE AccountId = @AccountId AND CompanyId = @CompanyId";
		public const string SelectCompanyAccount = @"SELECT * FROM MP_CompanyAccounts WHERE AccountId = @AccountId AND CompanyId = @CompanyId";
		public const string InsertCompanyAccount = @"INSERT INTO MP_CompanyAccounts (AccountId, CompanyId, Admin) VALUES (@AccountId, @CompanyId, @Admin)";
		public const string UpdateCompanyAccount = @"UPDATE MP_CompanyAccounts SET Admin = @Admin WHERE @AccountId AND CompanyId = @CompanyId";
		public const string DeleteCompanyAccount = @"DELETE MP_CompanyAccounts WHERE @AccountId AND CompanyId = @CompanyId";
		//
	}
}
