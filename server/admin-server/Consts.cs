﻿
namespace Microarea.AdminServer
{
	// Istruzioni SQL per la gestione delle strutture dati del model
	//=========================================================================
	public class Consts
    {
		// Instance
		public const string ExistInstance =             @"SELECT COUNT(*) FROM MP_Instances WHERE InstanceId = @InstanceId";
		public const string SelectInstanceByName =      @"SELECT * FROM MP_Instances WHERE Name = @Name";
		public const string InsertInstance =            @"INSERT INTO MP_Instances (Name, Customer, Disabled) VALUES (@Name, @Customer, @Disabled)";
		public const string UpdateInstance =            @"UPDATE MP_Instances SET Name = @Name, Customer = @Customer, Disabled = @Disabled WHERE InstanceId = @InstanceId";
		public const string DeleteInstance =            @"DELETE MP_Instances WHERE InstanceId = @InstanceId";
		//

		// ServerURL
		public const string ExistServerURL = @"SELECT COUNT(*) FROM MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string SelectServerURL = @"SELECT * FROM MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string InsertServerURL = @"INSERT INTO MP_ServerURLs (InstanceId, URLType, URL) VALUES (@InstanceId, @URLType, @URL)";
		public const string UpdateServerURL = @"UPDATE MP_ServerURLs SET URL = @URL WHERE InstanceId = @InstanceId AND URLType = @URLType";
		public const string DeleteServerURL = @"DELETE MP_ServerURLs WHERE InstanceId = @InstanceId AND URLType = @URLType";
		//

		// Subscription
		public const string ExistSubscription =         @"SELECT COUNT(*) FROM MP_Subscriptions WHERE SubscriptionId = @SubscriptionId";
		public const string SelectSubscriptionByName =  @"SELECT * FROM MP_Subscriptions WHERE Name = @Name";
		public const string InsertSubscription =        @"INSERT INTO MP_Subscriptions (Name, ActivationKey, PurchaseId, InstanceId) VALUES (@Name, @ActivationKey, @PurchaseId, @InstanceId)";
		public const string UpdateSubscription =        @"UPDATE MP_Subscriptions SET Name = @Name, ActivationKey = @ActivationKey, PurchaseId = @PurchaseId, InstanceId = @InstanceId 
												        WHERE SubscriptionId = @SubscriptionId";
		public const string DeleteSubscription =        @"DELETE MP_Subscriptions WHERE SubscriptionId = @SubscriptionId";
        //

        // SubscriptionSlot
        public const string DeleteSubscriptionSlot =    @"DELETE MP_SubscriptionsSlot WHERE InstanceId = @InstanceId";
        //

        // Company
        public const string ExistCompany =              @"SELECT COUNT(*) FROM MP_Companies WHERE CompanyId = @CompanyId";
		public const string SelectCompanyByName =       @"SELECT * FROM MP_Companies WHERE Name = @Name";
		public const string InsertCompany =             @"INSERT INTO MP_Companies (Name, Description, CompanyDBServer, CompanyDBName, CompanyDBOwner, CompanyDBPassword, Disabled,
			                                            DatabaseCulture, IsUnicode, PreferredLanguage, ApplicationLanguage, Provider, SubscriptionId, UseDMS, DMSDBServer, DMSDBName, DMSDBOwner, DMSDBPassword) 
			                                            VALUES (@Name, @Description, @CompanyDBServer, @CompanyDBName, @CompanyDBOwner, @CompanyDBPassword, @Disabled,
			                                            @DatabaseCulture, @IsUnicode, @PreferredLanguage, @ApplicationLanguage, @Provider, @SubscriptionId, @UseDMS, @DMSDBServer, @DMSDBName, @DMSDBOwner, @DMSDBPassword)";
		public const string UpdateCompany =             @"UPDATE MP_Companies SET Name = @Name, Description = @Description, CompanyDBServer = @CompanyDBServer, CompanyDBName = @CompanyDBName, 
			                                            CompanyDBOwner = @CompanyDBOwner, CompanyDBPassword = @CompanyDBPassword, Disabled = @Disabled, DatabaseCulture = @DatabaseCulture, IsUnicode = @IsUnicode, 
			                                            PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage, Provider = @Provider, SubscriptionId = @SubscriptionId, UseDMS = @UseDMS, 
			                                            DMSDBServer = @DMSDBServer, DMSDBName = @DMSDBName, DMSDBOwner = @DMSDBOwner, DMSDBPassword = @DMSDBPassword
			                                            WHERE CompanyId = @CompanyId";
		public const string DeleteCompany=              @"DELETE MP_Companies WHERE CompanyId = @CompanyId";
		//

		// Account
		public const string ExistAccount                = @"SELECT COUNT(*) FROM MP_Accounts WHERE AccountName = @AccountName";
		public const string SelectAccountByAccountName= @"SELECT * FROM MP_Accounts WHERE AccountName = @AccountName";
        public const string InsertAccount =				@"INSERT INTO MP_Accounts (AccountName, FullName, Password, Notes, Email, PasswordNeverExpires, MustChangePassword, CannotChangePassword, 
		                                            	PasswordExpirationDateCannotChange, PasswordExpirationDate, Disabled, Locked, ProvisioningAdmin, WindowsAuthentication, PreferredLanguage, ApplicationLanguage, LoginFailedCount) 
		                                            	VALUES (@AccountName, @FullName, @Password, @Notes, @Email, @PasswordNeverExpires, @MustChangePassword, @CannotChangePassword, 
		                                            	@PasswordExpirationDateCannotChange, @PasswordExpirationDate, @Disabled, @Locked, @ProvisioningAdmin, @WindowsAuthentication, @PreferredLanguage, @ApplicationLanguage, @LoginFailedCount)";
		public const string UpdateAccount				= @"UPDATE MP_Accounts SET AccountName = @AccountName, FullName = @FullName, Password = @Password, Notes = @Notes, Email = @Email, 
			                                            PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
			                                            PasswordExpirationDateCannotChange = @PasswordExpirationDateCannotChange, PasswordExpirationDate = @PasswordExpirationDate, Disabled = @Disabled, Locked = @Locked, 
			                                            ProvisioningAdmin = @ProvisioningAdmin, WindowsAuthentication = @WindowsAuthentication, PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage, LoginFailedCount = @LoginFailedCount) 
			                                            WHERE AccountId = @AccountId";
		public const string DeleteAccount =             @"DELETE MP_Accounts WHERE AccountId = @AccountId";
		//

		// CompanyAccount
		public const string ExistCompanyAccount	 = @"SELECT COUNT(*) FROM MP_CompanyAccounts WHERE AccountId = @AccountId AND CompanyId = @CompanyId";
		public const string SelectCompanyAccount = @"SELECT * FROM MP_CompanyAccounts WHERE AccountId = @AccountId AND CompanyId = @CompanyId";
		public const string InsertCompanyAccount = @"INSERT INTO MP_CompanyAccounts (AccountId, CompanyId, Admin) VALUES (@AccountId, @CompanyId, @Admin)";
		public const string UpdateCompanyAccount = @"UPDATE MP_CompanyAccounts SET Admin = @Admin WHERE @AccountId = @AccountId AND CompanyId = @CompanyId";
		public const string DeleteCompanyAccount = @"DELETE MP_CompanyAccounts WHERE @AccountId = @AccountId AND CompanyId = @CompanyId";
		//

		// InstanceAccount
		public const string ExistInstanceAccount = @"SELECT COUNT(*) FROM MP_InstanceAccounts WHERE AccountId = @AccountId AND InstanceId = @InstanceId";
		public const string SelectInstanceAccountByInstanceId = @"SELECT * FROM MP_InstanceAccounts WHERE InstanceId = @InstanceId";
		public const string SelectInstanceAccountByAccountId = @"SELECT * FROM MP_InstanceAccounts WHERE AccountId = @AccountId";
		public const string InsertInstanceAccount = @"INSERT INTO MP_InstanceAccounts (AccountId, InstanceId) VALUES (@AccountId, @InstanceId)";
		public const string DeleteInstanceAccount = @"DELETE MP_InstanceAccounts WHERE @AccountId = @AccountId AND InstanceId = @InstanceId";
		//

		// SubscriptionAccount
		public const string ExistSubscriptionAccount = @"SELECT COUNT(*) FROM MP_SubscriptionAccounts WHERE AccountId = @AccountId AND SubscriptionId = @SubscriptionId";
		public const string SelectSubscriptionAccountBySubscriptionId = @"SELECT * FROM MP_SubscriptionAccounts WHERE SubscriptionId = @SubscriptionId";
		public const string SelectSubscriptionAccountByAccountId = @"SELECT * FROM MP_SubscriptionAccounts WHERE AccountId = @AccountId";
		public const string InsertSubscriptionAccount = @"INSERT INTO MP_SubscriptionAccounts (AccountId, SubscriptionId) VALUES (@AccountId, @SubscriptionId)";
		public const string DeleteSubscriptionAccount = @"DELETE MP_SubscriptionAccounts WHERE @AccountId = @AccountId AND SubscriptionId = @SubscriptionId";
		//
	}
}
