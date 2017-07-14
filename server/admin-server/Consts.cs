
namespace Microarea.AdminServer
{
    // Istruzioni SQL per la gestione delle strutture dati del model
    //=========================================================================
    public class Consts
    {
        // Instance
        public const string ExistInstance = "SELECT COUNT(*) FROM MP_Instances ";
        public const string SelectInstance = "SELECT * FROM MP_Instances";
        public const string InsertInstance = "INSERT INTO MP_Instances (InstanceKey, Description, Customer, Disabled, Origin, Tags, UnderMaintenance) VALUES (@InstanceKey, @Description, @Customer, @Disabled, @Origin, @Tags, @UnderMaintenance)";
        public const string UpdateInstance = "UPDATE MP_Instances SET Description = @Description, Customer = @Customer, Disabled = @Disabled, Origin=@Origin, Tags=@Tags, UnderMaintenance=@UnderMaintenance WHERE InstanceKey = @InstanceKey";
        public const string DeleteInstance = "DELETE MP_Instances WHERE InstanceKey = @InstanceKey";

		// Referenced tables
		public const string SelectURlsInstance = "SELECT * FROM MP_ServerURLs WHERE InstanceKey = @InstanceKey";

		// ServerURL
		public const string ExistServerURL = "SELECT COUNT(*) FROM MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string SelectServerURL = "SELECT * FROM MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string InsertServerURL = "INSERT INTO MP_ServerURLs (InstanceKey, URLType, URL) VALUES (@InstanceKey, @URLType, @URL)";
        public const string UpdateServerURL = "UPDATE MP_ServerURLs SET URL = @URL WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string DeleteServerURL = "DELETE MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        //

        // Subscription
        public const string ExistSubscription = @"SELECT COUNT(*) FROM MP_Subscriptions WHERE SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscription = @"SELECT * FROM MP_Subscriptions WHERE SubscriptionKey = @SubscriptionKey";
        public const string InsertSubscription = @"INSERT INTO MP_Subscriptions (SubscriptionKey, Description, ActivationToken, PreferredLanguage, ApplicationLanguage, MinDBSizeToWarn, InstanceKey, UnderMaintenance) 
											 	VALUES (@SubscriptionKey, @Description, @ActivationToken, @PreferredLanguage, @ApplicationLanguage, @MinDBSizeToWarn, @InstanceKey, @UnderMaintenance)";
        public const string UpdateSubscription = @"UPDATE MP_Subscriptions SET Description = @Description, ActivationToken = @ActivationToken, PreferredLanguage = @PreferredLanguage, 
												ApplicationLanguage = @ApplicationLanguage, MinDBSizeToWarn = @MinDBSizeToWarn, InstanceKey = @InstanceKey, UnderMaintenance=@UnderMaintenance WHERE SubscriptionKey = @SubscriptionKey";
        public const string DeleteSubscription = @"DELETE MP_Subscriptions WHERE SubscriptionKey = @SubscriptionKey";
        //

        // SubscriptionSlot
        public const string DeleteSubscriptionSlot = "DELETE MP_SubscriptionsSlot WHERE SubscriptionKey = @SubscriptionKey";
        //

        // Company
        public const string ExistCompany = @"SELECT CompanyId FROM MP_Companies WHERE Name = @Name";
        public const string SelectCompany = @"SELECT * FROM MP_Companies WHERE Name = @Name";
        public const string InsertCompany = @"INSERT INTO MP_Companies (Name, Description, CompanyDBServer, CompanyDBName, CompanyDBOwner, CompanyDBPassword, Disabled,
			                                    DatabaseCulture, IsUnicode, PreferredLanguage, ApplicationLanguage, Provider, SubscriptionKey, UseDMS, DMSDBServer, DMSDBName, DMSDBOwner, DMSDBPassword) 
			                                    VALUES (@Name, @Description, @CompanyDBServer, @CompanyDBName, @CompanyDBOwner, @CompanyDBPassword, @Disabled, @DatabaseCulture, @IsUnicode, 
												@PreferredLanguage, @ApplicationLanguage, @Provider, @SubscriptionKey, @UseDMS, @DMSDBServer, @DMSDBName, @DMSDBOwner, @DMSDBPassword)";
        public const string UpdateCompany = @"UPDATE MP_Companies SET Name = @Name, Description = @Description, CompanyDBServer = @CompanyDBServer, CompanyDBName = @CompanyDBName, 
			                                    CompanyDBOwner = @CompanyDBOwner, CompanyDBPassword = @CompanyDBPassword, Disabled = @Disabled, DatabaseCulture = @DatabaseCulture, IsUnicode = @IsUnicode, 
			                                    PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage, Provider = @Provider, SubscriptionKey = @SubscriptionKey, 
												UseDMS = @UseDMS, DMSDBServer = @DMSDBServer, DMSDBName = @DMSDBName, DMSDBOwner = @DMSDBOwner, DMSDBPassword = @DMSDBPassword
			                                    WHERE CompanyId = @CompanyId";
        public const string DeleteCompany = @"DELETE MP_Companies WHERE CompanyId = @CompanyId";
        //

        // Account
        public const string ExistAccount = @"SELECT COUNT(*) FROM MP_Accounts WHERE AccountName = @AccountName";
        public const string SelectAccount = @"SELECT * FROM MP_Accounts WHERE AccountName = @AccountName";

        public const string InsertAccount = @"INSERT INTO MP_Accounts (AccountName, FullName, Password, CloudAdmin, Notes, Email, LoginFailedCount, PasswordNeverExpires, MustChangePassword, 
											CannotChangePassword, PasswordExpirationDate, PasswordDuration, Disabled, Locked, ProvisioningAdmin, WindowsAuthentication, PreferredLanguage, 
											ApplicationLanguage, Ticks, ExpirationDate, ParentAccount, Confirmed) 
		                                    VALUES (@AccountName, @FullName, @Password, @CloudAdmin, @Notes, @Email, @LoginFailedCount, @PasswordNeverExpires, @MustChangePassword, 
											@CannotChangePassword, @PasswordExpirationDate, @PasswordDuration, @Disabled, @Locked, @ProvisioningAdmin, @WindowsAuthentication, @PreferredLanguage, 
											@ApplicationLanguage, @Ticks, @ExpirationDate, @ParentAccount, @Confirmed)";

        public const string UpdateAccount = @"UPDATE MP_Accounts SET FullName = @FullName, Password = @Password, Notes = @Notes, Email = @Email, LoginFailedCount = @LoginFailedCount,
			                                PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
			                                PasswordExpirationDate = @PasswordExpirationDate, PasswordDuration = @PasswordDuration, Disabled = @Disabled, Locked = @Locked, 
											ProvisioningAdmin = @ProvisioningAdmin, CloudAdmin = @CloudAdmin, WindowsAuthentication = @WindowsAuthentication, PreferredLanguage = @PreferredLanguage, 
											ApplicationLanguage = @ApplicationLanguage, Ticks = @Ticks, ExpirationDate = @ExpirationDate, ParentAccount = @ParentAccount, Confirmed = @Confirmed
			                                WHERE AccountName = @AccountName";
        public const string DeleteAccount = @"DELETE MP_Accounts WHERE AccountName = @AccountName";
        //

        // CompanyAccount
        public const string ExistCompanyAccount = @"SELECT COUNT(*) FROM MP_CompanyAccounts WHERE AccountName = @AccountName AND CompanyId = @CompanyId";
        public const string SelectCompanyAccount = @"SELECT * FROM MP_CompanyAccounts WHERE AccountName = @AccountName AND CompanyId = @CompanyId";
        public const string InsertCompanyAccount = @"INSERT INTO MP_CompanyAccounts (AccountName, CompanyId, Admin) VALUES (@AccountName, @CompanyId, @Admin)";
        public const string UpdateCompanyAccount = @"UPDATE MP_CompanyAccounts SET Admin = @Admin WHERE @AccountName = @AccountName AND CompanyId = @CompanyId";
        public const string DeleteCompanyAccount = @"DELETE MP_CompanyAccounts WHERE @AccountName = @AccountName AND CompanyId = @CompanyId";
        //

        // InstanceAccount
        public const string ExistInstanceAccount = @"SELECT COUNT(*) FROM MP_InstanceAccounts WHERE AccountName = @AccountName AND InstanceKey = @InstanceKey";
        public const string SelectInstanceAccountByInstanceKey = @"SELECT * FROM MP_InstanceAccounts WHERE InstanceKey = @InstanceKey";
        public const string SelectInstanceAccountByAccount = @"SELECT * FROM MP_InstanceAccounts WHERE AccountName = @AccountName";
        public const string InsertInstanceAccount = @"INSERT INTO MP_InstanceAccounts (AccountName, InstanceKey) VALUES (@AccountName, @InstanceKey)";
        public const string DeleteInstanceAccount = @"DELETE MP_InstanceAccounts WHERE @AccountName = @AccountName AND InstanceKey = @InstanceKey";
        //

        // SubscriptionAccount
        public const string ExistSubscriptionAccount = @"SELECT COUNT(*) FROM MP_SubscriptionAccounts WHERE AccountName = @AccountName AND SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscriptionAccountBySubscriptionKey = @"SELECT * FROM MP_SubscriptionAccounts WHERE SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscriptionAccountByAccount = @"SELECT * FROM MP_SubscriptionAccounts WHERE AccountName = @AccountName";
        public const string InsertSubscriptionAccount = @"INSERT INTO MP_SubscriptionAccounts (AccountName, SubscriptionKey) VALUES (@AccountName, @SubscriptionKey)";
        public const string DeleteSubscriptionAccount = @"DELETE MP_SubscriptionAccounts WHERE @AccountName = @AccountName AND SubscriptionKey = @SubscriptionKey";
        //

        // securitytoken
        public const string ExistSecurityToken = "SELECT COUNT(*) FROM MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";
        public const string SelectSecurityToken = "SELECT * FROM MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";
        public const string InsertSecurityToken = "INSERT INTO MP_SecurityTokens (AccountName, TokenType, Token, ExpirationDate, Expired) VALUES (@AccountName, @TokenType, @Token, @ExpirationDate, @Expired)";
        public const string UpdateSecurityToken = "UPDATE MP_SecurityTokens SET Token=@Token, ExpirationDate=@ExpirationDate, Expired=@Expired WHERE AccountName=@AccountName AND TokenType=@TokenType";
        public const string DeleteSecurityToken = "DELETE MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";

        // roles
        public const string ExistRole = "SELECT COUNT(*) FROM MP_Roles WHERE RoleId=@RoleId";
        public const string SelectRole = "SELECT * FROM MP_Roles WHERE RoleId=@RoleId";
        public const string InsertRole = "INSERT INTO MP_Roles (RoleName, Description, Disabled) VALUES (@RoleName, @Description, @Disabled)";
        public const string UpdateRole = "UPDATE MP_Roles SET RoleName=@RoleName, Description=@Description, Disabled=@Disabled WHERE RoleId=@RoleId";
        public const string DeleteRole = "DELETE MP_Roles WHERE RoleId=@RoleId";

        // SubscriptionAccountRole
        public const string ExistSubscriptionAccountRole = "SELECT COUNT(*) FROM MP_SubscriptionAccountRoles WHERE RoleId=@RoleId AND AccountName=@AccountName AND SubscriptionKey=@SubscriptionKey";
        public const string SelectSubscriptionAccountRole = "SELECT * FROM MP_SubscriptionAccountRolesWHERE RoleId=@RoleId AND AccountName=@AccountName AND SubscriptionKey=@SubscriptionKey";
        public const string InsertSubscriptionAccountRole = "INSERT INTO MP_SubscriptionAccountRoles (RoleId, AccountName, SubscriptionKey) VALUES (@RoleId, @AccountName, @SubscriptionKey)";
        // la query di update essendo tutte chiavi non so se si fa.
        public const string UpdateSubscriptionAccountRole = "UPDATE MP_SubscriptionAccountRoles SET RoleName=@RoleName WHERE RoleId=@RoleId AND AccountName=@AccountName AND SubscriptionKey=@SubscriptionKey";
        public const string DeleteSubscriptionAccountRole = "DELETE MP_SubscriptionAccountRoles WHERE RoleId=@RoleId AND AccountName=@AccountName AND SubscriptionKey=@SubscriptionKey";

    }
}
