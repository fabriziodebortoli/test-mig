using System;
using System.Collections.Generic;

namespace TaskBuilderNetCore.Interfaces
{
    public interface ILoginManager
    {
        bool Admin { get; }
        bool AdminWebSitePrivateArea { get; }
        string ApplicationCompanyLanguage { get; }
        string ApplicationLanguage { get; }
        bool Auditing { get; }
        string AuthenticationToken { get; }
        int CompanyId { get; }
        string CompanyName { get; }
        string DbName { get; }
        string DbServer { get; }
        string DbUser { get; }
        string Email { get; }
        bool ExpiredDateCannotChange { get; }
        DateTime ExpiredDatePassword { get; }
        int LoginId { get; }
        LoginManagerState LoginManagerState { get; }
        string NonProviderCompanyConnectionString { get; }
        string Password { get; set; }
        bool PasswordNeverExpired { get; }
        string PreferredCompanyLanguage { get; }
        string PreferredLanguage { get; }
        string ProviderCompanyConnectionString { get; }
        string ProviderDescription { get; }
        int ProviderId { get; }
        string ProviderName { get; }
        bool RowSecurity { get; }
        bool Security { get; }
        bool StripTrailingSpaces { get; }
        bool TransactionUse { get; }
        bool UseConstParameter { get; }
        bool UseKeyedUpdate { get; }
        bool UserCannotChangePassword { get; }
        string[] UserCompanies { get; }
        string UserDescription { get; }
        string UserInfo { get; }
        string UserInfoLicensee { get; }
        bool UserMustChangePassword { get; }
        string UserName { get; }
        bool UseUnicode { get; }
        bool WinNTAuthentication { get; }
        void AdvancedSendBalloon(string authenticationToken, string bodyMessage, DateTime expiryDate, MessageType messageType = MessageType.Updates, string[] recipients = null, MessageSensation sensation = MessageSensation.Information, bool historicize = true, bool immediate = false, int timer = 0, string tag = null);
        bool CanMigrate();
        bool CanUseNamespace(string nameSpace, GrantType grantType);
        int ChangePassword(string newPassword);
        int ChangePassword(string userName, string oldPassword, string newPassword);
        bool CheckActivationExpression(string currentApplicationName, string activationExpression);
        bool ConfirmToken(string authenticationToken, string procType);
        bool DeleteAssociation(int loginId, int companyId, string authenticationToken);
        bool DeleteCompany(int companyId, string authenticationToken);
        void DeleteLicensed(string name);
        void DeleteMessageFromQueue(string messageID);
        bool DeleteUser(int loginId, string authenticationToken);
        void DeleteUserInfo();
        string[] EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers);
        string[] EnumAllUsers();
        string[] EnumCompanies(string userName);
        ActivationState GetActivationState(out int daysToExpiration);
        // ModuleNameInfo[] GetArticlesWithFloatingCal();
        //ModuleNameInfo[] GetArticlesWithNamedCal();
        string GetAspNetUser();
        bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool webLogin);
        bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName);
        string GetBrandedKey(string source);
        string GetBrandedProducerName();
        string GetBrandedProductTitle();
        int GetCalNumber(out int unNamedCal, out int gdiConcurrent, out int officeCal, out int tpCal, out int wmsCal, out int manufacturingCal);
        LoginSlotType GetCalType(string authenticationToken);
        //List<TbSenderDatabaseInfo> GetCompanyDatabasesInfo(string authenticationToken);
        int GetCompanyLoggedUsersNumber(int companyId);
        string[] GetCompanyUsers(string companyName);
        string GetConfigurationHash();
        byte[] GetConfigurationStream();
        string GetCountry();
        string GetDatabaseType();
        //List<DataSynchroDatabaseInfo> GetDataSynchroDatabasesInfo(string authenticationToken);
        int GetDBCultureLCID();
        int GetDBCultureLCID(int companyID);
        DBNetworkType GetDBNetworkType();
        string GetDMSConnectionString(string authenticationToken);
        //List<DmsDatabaseInfo> GetDMSDatabasesInfo(string authenticationToken);
        string GetEdition();
        string GetEditionType();
        IAdvertisement[] GetImmediateMessages(string authenticationToken);
        string GetInstallationVersion();
        string GetInstallationVersion(out string productName, out DateTime buildDate, out DateTime instDate, out int build);
        string GetLoggedUsers();
        string GetLoggedUsersAdvanced(string token);
        int GetLoggedUsersNumber();
        bool GetLoginInformation(string authenticationToken);
        string GetMainSerialNumber();
        string GetMasterProductBrandedName();
        string GetMasterSolutionBrandedName();
        IAdvertisement[] GetMessages(string authenticationToken);
        string GetMLUExpiryDate();
        string[] GetModules();
        string[] GetNonNTCompanyUsers(string companyName);
        IAdvertisement[] GetOldMessages(string authenticationToken);
        string GetProviderNameFromCompanyId(int companyId);
        //ProxySettings GetProxySettings();
        bool GetProxySettings(out string server, out int port);
        int GetProxySupportVersion();
        string[] GetRoleUsers(string companyName, string roleName);
        SerialNumberType GetSerialNumberType();
        string GetSystemDBConnectionString(string authenticationToken);
        int GetUsagePercentageOnDBSize();
        string GetUserDescriptionById(int loginId);
        string GetUserDescriptionByName(string login);
        string GetUserInfo();
        string GetUserInfoID();
        bool HasUserAlreadyChangedPasswordToday(string user);
        int Init(bool reboot, string authenticationToken);
        bool IsActivated(string application, string functionality);
        bool IsAlive();
        bool IsCalAvailable(string authenticationToken, string application, string functionality);
        bool IsCompanySecured(int companyId);
        bool IsDemo();
        bool IsDeveloperActivation();
        bool IsDistributor();
        bool IsEasyBuilderDeveloper(string authenticationToken);
        bool IsFloatingUser(string userName, out bool floating);
        bool IsIntegrateSecurityUser(string userName);
        bool IsRegistered(out string message, out ActivationState actState);
        bool IsRegisteredTrapped(out string message, out ActivationState actState);
        bool IsReseller();
        bool IsSynchActivation();
        bool IsUserLogged();
        bool IsUserLogged(int loginID);
        bool IsValidToken(string authenticationToken);
        bool IsVirginActivation();
        bool IsWebUser(string userName, out bool web);
        bool IsWinNT(int loginId);
        int Login(string companyName, string askingProcess, bool overWriteLogin, string macIp = null);
        int Login(string userName, string password, bool winNTAuthentication, string companyName, string askingProcess, bool overWriteLogin);
        int LoginViaInfinityToken(string cryptedToken, string username, string password, string company);
        void LogOff();
        void LogOff(string authenticationToken);
        ISecurity NewSecurity(string company, string user, bool applySecurityFilter);
        string Ping();
        bool PingNeeded(bool force);
        string PrePing();
        void PurgeMessageByTag(string tag, string user = null);
        void RefreshFloatingMark();
        void RefreshSecurityStatus();
        void ReloadConfiguration();
        void ReloadUserArticleBindings(string authenticationToken);
        bool SaveLicensed(string xml, string name);
        bool SaveUserInfo(string xml);
        void SendBalloon(string authenticationToken, string bodyMessage, MessageType messageType = MessageType.Updates, List<string> recipients = null);
        //void SetClientData(ClientData cd);
        void SetMessageRead(string authenticationToken, string messageID);
        //void SetProxySettings(ProxySettings proxySettings);
        bool Sql2012Allowed(string authToken);
        string SsoLoggedUser(string ssoToken);
        int SsoLogin(string ssoToken);
        void SSOLogOff(string cryptedToken);
        void StoreMLUChoice(bool userChoseMluInChargeToMicroarea);
        void TraceAction(string company, string login, TraceActionType type, string processName, string winUser, string location);
        bool UserCanAccessWebSitePrivateArea();
        string ValidateItoken(string itoken);
        int ValidateUser(string userName, string password, bool winNTAuthentication);
        bool VerifyDBSize();
    }
}