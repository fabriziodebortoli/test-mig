
namespace Microarea.AdminServer
{
    // Istruzioni SQL per la gestione delle strutture dati del model
    //=========================================================================
    public class Queries
    {
        // Instance
        public const string ExistInstance = @"SELECT COUNT(*) FROM MP_Instances WHERE InstanceKey = @InstanceKey ";
        public const string SelectInstance = @"SELECT * FROM MP_Instances WHERE InstanceKey = '{0}'";
        public const string SelectInstanceAll = @"SELECT * FROM MP_Instances";
        public const string InsertInstance = @"INSERT INTO MP_Instances (InstanceKey, Description, Disabled, Origin, Tags, UnderMaintenance, PendingDate) VALUES (@InstanceKey, @Description, @Disabled, @Origin, @Tags, @UnderMaintenance, @PendingDate)";
        public const string UpdateInstance = @"UPDATE MP_Instances SET Description = @Description, Disabled = @Disabled, Origin=@Origin, Tags=@Tags, UnderMaintenance=@UnderMaintenance, PendingDate=@PendingDate WHERE InstanceKey = @InstanceKey";
        public const string DeleteInstance = @"DELETE MP_Instances WHERE InstanceKey = @InstanceKey";

        // Instance specialized (BurgerData)
        public const string SelectInstanceForAccount = @"SELECT * FROM MP_Instances WHERE InstanceKey IN
														(SELECT DISTINCT InstanceKey FROM MP_Subscriptions sub 
															JOIN MP_SubscriptionAccounts subAcc ON sub.SubscriptionKey = subAcc.SubscriptionKey
															JOIN MP_SubscriptionInstances subIns ON subAcc.SubscriptionKey = subIns.SubscriptionKey
															WHERE subAcc.AccountName = '{0}')";

        // Referenced table MP_ServerURLs
        public const string SelectURlsInstance = @"SELECT * FROM MP_ServerURLs WHERE InstanceKey = {0}";

        // ServerURL
        public const string ExistServerURL = @"SELECT COUNT(*) FROM MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string SelectServerURL = @"SELECT * FROM MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string InsertServerURL = @"INSERT INTO MP_ServerURLs (InstanceKey, URLType, URL) VALUES (@InstanceKey, @URLType, @URL)";
        public const string UpdateServerURL = @"UPDATE MP_ServerURLs SET URL = @URL WHERE InstanceKey = @InstanceKey AND URLType = @URLType";
        public const string DeleteServerURL = @"DELETE MP_ServerURLs WHERE InstanceKey = @InstanceKey AND URLType = @URLType";

        // Subscription (BurgerData)
        public const string SelectSubscriptionsByAccount = "SELECT * FROM MP_Subscriptions sub JOIN MP_SubscriptionAccounts subacc ON sub.SubscriptionKey = subacc.SubscriptionKey WHERE subacc.AccountName = '{0}'";
        public const string SelectSubscriptionsByAccountInstance = @"SELECT * FROM MP_Subscriptions sub 
																		JOIN MP_SubscriptionAccounts subacc ON sub.SubscriptionKey = subacc.SubscriptionKey 
																		JOIN MP_SubscriptionInstances subins ON subacc.SubscriptionKey = subins.SubscriptionKey
																		WHERE subacc.AccountName = '{0}'";
        // Subscription
        public const string ExistSubscription = @"SELECT COUNT(*) FROM MP_Subscriptions WHERE SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscriptionsAll = @"SELECT * FROM MP_Subscriptions";
        public const string SelectSubscription = @"SELECT * FROM MP_Subscriptions WHERE SubscriptionKey = '{0}'";
        public const string InsertSubscription = @"INSERT INTO MP_Subscriptions (SubscriptionKey, Description, ActivationToken, Language, RegionalSettings, MinDBSizeToWarn, UnderMaintenance) 
											 	VALUES (@SubscriptionKey, @Description, @ActivationToken, @Language, @RegionalSettings, @MinDBSizeToWarn, @UnderMaintenance)";
        public const string UpdateSubscription = @"UPDATE MP_Subscriptions SET Description = @Description, ActivationToken = @ActivationToken, Language = @Language, 
												RegionalSettings = @RegionalSettings, MinDBSizeToWarn = @MinDBSizeToWarn, UnderMaintenance=@UnderMaintenance WHERE SubscriptionKey = @SubscriptionKey";
        public const string DeleteSubscription = @"DELETE MP_Subscriptions WHERE SubscriptionKey = @SubscriptionKey";

        // SubscriptionSlot
        public const string DeleteSubscriptionSlot = @"DELETE MP_SubscriptionsSlots WHERE SubscriptionKey = @SubscriptionKey";

        // SubscriptionDatabases
        public const string ExistSubscriptionDatabase = @"SELECT COUNT(*) FROM MP_SubscriptionDatabases WHERE InstanceKey = @InstanceKey AND SubscriptionKey = @SubscriptionKey AND Name = @Name";
		public const string SelectSubscriptionDatabase = @"SELECT * FROM MP_SubscriptionDatabases WHERE InstanceKey = @InstanceKey AND SubscriptionKey = @SubscriptionKey AND Name = @Name";
		public const string InsertSubscriptionDatabase = @"INSERT INTO MP_SubscriptionDatabases (InstanceKey, SubscriptionKey, Name, Description, DBServer, DBName, DBOwner, DBPassword, Disabled, DatabaseCulture, 
														IsUnicode, Language, RegionalSettings, Provider, UseDMS, DMSDBServer, DMSDBName, DMSDBOwner, DMSDBPassword, Test, UnderMaintenance)  
														VALUES (@InstanceKey, @SubscriptionKey, @Name, @Description, @DBServer, @DBName, @DBOwner, @DBPassword, @Disabled, @DatabaseCulture, 
														@IsUnicode, @Language, @RegionalSettings, @Provider, @UseDMS, @DMSDBServer, @DMSDBName, @DMSDBOwner, @DMSDBPassword, @Test, @UnderMaintenance)";
		public const string UpdateSubscriptionDatabase = @"UPDATE MP_SubscriptionDatabases SET Description = @Description, DBServer = @DBServer, DBName = @DBName, DBOwner = @DBOwner, 
														DBPassword = @DBPassword, Disabled = @Disabled, DatabaseCulture = @DatabaseCulture, IsUnicode = @IsUnicode, 
														Language = @Language, RegionalSettings = @RegionalSettings, Provider = @Provider, UseDMS = @UseDMS, 
														DMSDBServer = @DMSDBServer, DMSDBName = @DMSDBName, DMSDBOwner = @DMSDBOwner, DMSDBPassword = @DMSDBPassword, Test = @Test, UnderMaintenance = @UnderMaintenance
														WHERE InstanceKey = @InstanceKey AND SubscriptionKey = @SubscriptionKey AND Name = @Name";
		public const string DeleteSubscriptionDatabase = @"DELETE MP_SubscriptionDatabases WHERE InstanceKey = @InstanceKey AND SubscriptionKey = @SubscriptionKey AND Name = @Name";

		// SubscriptionDatabases (BurgerData)
		public const string SelectDatabases = @"SELECT * FROM MP_SubscriptionDatabases WHERE InstanceKey = '{0}' AND SubscriptionKey = '{1}'";
		public const string SelectDatabaseByName = @"SELECT * FROM MP_SubscriptionDatabases WHERE InstanceKey = '{0}' AND SubscriptionKey = '{1}' AND Name = '{2}'";

		// Account
		public const string ExistAccount = @"SELECT COUNT(*) FROM MP_Accounts WHERE AccountName = @AccountName";
        public const string SelectAccount = @"SELECT * FROM MP_Accounts WHERE AccountName = @AccountName";

        public const string InsertAccount = @"INSERT INTO MP_Accounts (AccountName, FullName, Password, Salt, Notes, Email, LoginFailedCount, PasswordNeverExpires, MustChangePassword, 
												CannotChangePassword, PasswordExpirationDate, PasswordDuration, Disabled, Locked, WindowsAuthentication, Language, 
												RegionalSettings, Ticks, ExpirationDate, ParentAccount, Confirmed) 
												VALUES (@AccountName, @FullName, @Password, @Salt, @Notes, @Email, @LoginFailedCount, @PasswordNeverExpires, @MustChangePassword, 
												@CannotChangePassword, @PasswordExpirationDate, @PasswordDuration, @Disabled, @Locked, @WindowsAuthentication, @Language, 
												@RegionalSettings, @Ticks, @ExpirationDate, @ParentAccount, @Confirmed)";

        public const string UpdateAccount = @"UPDATE MP_Accounts SET FullName = @FullName, Password = @Password, Salt = @Salt, Notes = @Notes, Email = @Email, LoginFailedCount = @LoginFailedCount,
												PasswordNeverExpires = @PasswordNeverExpires, MustChangePassword = @MustChangePassword, CannotChangePassword = @CannotChangePassword, 
												PasswordExpirationDate = @PasswordExpirationDate, PasswordDuration = @PasswordDuration, Disabled = @Disabled, Locked = @Locked, 
												WindowsAuthentication = @WindowsAuthentication, Language = @Language, 
												RegionalSettings = @RegionalSettings, Ticks = @Ticks, ExpirationDate = @ExpirationDate, ParentAccount = @ParentAccount, Confirmed = @Confirmed
											WHERE AccountName = @AccountName";

        public const string DeleteAccount = @"DELETE MP_Accounts WHERE AccountName = @AccountName";

        // SubscriptionAccounts
        public const string ExistSubscriptionAccount = @"SELECT COUNT(*) FROM MP_SubscriptionAccounts WHERE AccountName = @AccountName AND SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscriptionAccountBySubscriptionKey = @"SELECT * FROM MP_SubscriptionAccounts WHERE SubscriptionKey = @SubscriptionKey";
        public const string SelectSubscriptionAccountByAccount = @"SELECT * FROM MP_SubscriptionAccounts WHERE AccountName = @AccountName";
        public const string InsertSubscriptionAccount = @"INSERT INTO MP_SubscriptionAccounts (AccountName, SubscriptionKey) VALUES (@AccountName, @SubscriptionKey)";
        public const string DeleteSubscriptionAccount = @"DELETE MP_SubscriptionAccounts WHERE @AccountName = @AccountName AND SubscriptionKey = @SubscriptionKey";

        // SecurityToken
        public const string ExistSecurityToken = @"SELECT COUNT(*) FROM MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";
        public const string SelectSecurityToken = @"SELECT * FROM MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";
        public const string InsertSecurityToken = @"INSERT INTO MP_SecurityTokens (AccountName, TokenType, Token, ExpirationDate, Expired) VALUES (@AccountName, @TokenType, @Token, @ExpirationDate, @Expired)";
        public const string UpdateSecurityToken = @"UPDATE MP_SecurityTokens SET Token=@Token, ExpirationDate=@ExpirationDate, Expired=@Expired WHERE AccountName=@AccountName AND TokenType=@TokenType";
        public const string DeleteSecurityToken = @"DELETE MP_SecurityTokens WHERE AccountName = @AccountName AND TokenType=@TokenType";

        // Roles
        public const string ExistRole = @"SELECT COUNT(*) FROM MP_Roles WHERE RoleName = @RoleName";
        public const string SelectRole = @"SELECT * FROM MP_Roles WHERE RoleName = @RoleName";
        public const string InsertRole = @"INSERT INTO MP_Roles (RoleName, Description, ParentRoleName, Disabled) VALUES (@RoleName, @Description, @ParentRoleName, @Disabled)";
        public const string UpdateRole = @"UPDATE MP_Roles SET Description = @Description, ParentRoleName = @ParentRoleName, Disabled = @Disabled WHERE RoleName = @RoleName";
        public const string DeleteRole = @"DELETE MP_Roles WHERE RoleName = @RoleName";

        // AccountRoles
        public const string ExistAccountRoles = @"SELECT COUNT(*) FROM MP_AccountRoles WHERE RoleName = @RoleName AND AccountName = @AccountName AND EntityKey = @EntityKey";
        public const string SelectAccountRoles = @"SELECT * FROM MP_AccountRoles WHERE AccountName = '{0}'";
        public const string InsertAccountRoles = @"INSERT INTO MP_AccountRoles (RoleName, AccountName, EntityKey, Level) VALUES (@RoleName, @AccountName, @EntityKey, @Level)";
        public const string UpdateAccountRoles = @"UPDATE MP_AccountRoles SET Level = @Level WHERE RoleId = @RoleId AND AccountName = @AccountName AND EntityKey = @EntityKey";
        public const string DeleteAccountRole = @"DELETE MP_AccountRoles WHERE RoleName = @RoleName AND AccountName = @AccountName AND EntityKey = @EntityKey";
        // AccountRole (BurgerData)
        public const string SelectRolesByAccountName = @"SELECT * FROM MP_AccountRoles acc INNER JOIN 
															MP_Roles rol on acc.RoleName = rol.RoleName 
															WHERE AccountName = '{0}' AND rol.Disabled = 0";

        // RegisteredApp
        public const string SelectAppById = @"SELECT * FROM MP_RegisteredApps WHERE AppId = @AppId";
        public const string ExistRegisteredApp = @"SELECT COUNT(*) FROM MP_RegisteredApps WHERE AppId = @AppId";
        public const string InsertRegisteredApp = @"INSERT INTO MP_RegisteredApps (AppId, Name, Description, URL, SecurityValue) VALUES (@AppId, @Name, @Description, @URL, @SecurityValue)";
        public const string UpdateRegisteredApp = @"UPDATE MP_RegisteredApps SET (Name = @Name, Description = @Description, URL = @URL, SecurityValue = @SecurityValue) WHERE AppId = @AppId";

        // RecoveryCodes
        public const string SelectRecoveryCode = @"SELECT * FROM MP_RecoveryCodes WHERE AccountName = @AccountName";
        public const string DeleteRecoveryCode = @"DELETE MP_RecoveryCodes WHERE AccountName = @AccountName";
        public const string ExistRecoveryCode = @"SELECT COUNT(*) FROM MP_RecoveryCodes  WHERE AccountName = @AccountName";
        public const string InsertRecoveryCode = @"INSERT INTO MP_RecoveryCodes (AccountName, Code, ExpirationDate) VALUES (@AccountName, @Code, @ExpirationDate)";
        public const string UpdateRecoveryCode = @"UPDATE MP_RecoveryCodes SET Code = @Code, ExpirationDate = @ExpirationDate WHERE AccountName = @AccountName";
    }
}
