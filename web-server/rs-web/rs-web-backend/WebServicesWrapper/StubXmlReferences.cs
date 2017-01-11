using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microarea.TaskBuilderNet.Woorm.Generic;
using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.Woorm.WebServicesWrapper.LoginMngXml
{

    public class MicroareaLoginManager //: System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        private XDocument xdoc;
        private System.Data.SqlClient.SqlConnectionStringBuilder ccstr = new System.Data.SqlClient.SqlConnectionStringBuilder();
        // private System.Threading.SendOrPostCallback InitOperationCompleted;

        //  private System.Threading.SendOrPostCallback IsAliveOperationCompleted;

        //  private System.Threading.SendOrPostCallback IsCalAvailableOperationCompleted;

        //  private System.Threading.SendOrPostCallback PingNeededOperationCompleted;

        //   private System.Threading.SendOrPostCallback SetCompanyInfoOperationCompleted;

        //  private System.Threading.SendOrPostCallback IsActivatedOperationCompleted;

        //  private System.Threading.SendOrPostCallback IsSynchActivationOperationCompleted;

        //  private System.Threading.SendOrPostCallback GetModulesOperationCompleted;

        //   private System.Threading.SendOrPostCallback GetCompanyUsersOperationCompleted;

        //  private System.Threading.SendOrPostCallback GetNonNTCompanyUsersOperationCompleted;

        //  private System.Threading.SendOrPostCallback GetCompanyRolesOperationCompleted;

        //  private System.Threading.SendOrPostCallback GetUserRolesOperationCompleted;

        //  private System.Threading.SendOrPostCallback EnumAllUsersOperationCompleted;

        // private System.Threading.SendOrPostCallback EnumAllCompanyUsersOperationCompleted;

        // private System.Threading.SendOrPostCallback GetRoleUsersOperationCompleted;

        // private System.Threading.SendOrPostCallback EnumCompaniesOperationCompleted;

        // private System.Threading.SendOrPostCallback IsIntegratedSecurityUserOperationCompleted;

        // private System.Threading.SendOrPostCallback GetLoggedUsersNumberOperationCompleted;

        // private System.Threading.SendOrPostCallback GetCompanyLoggedUsersNumberOperationCompleted;

        // private System.Threading.SendOrPostCallback GetLoggedUsersOperationCompleted;

        // private System.Threading.SendOrPostCallback GetLoggedUsersAdvancedOperationCompleted;

        // private System.Threading.SendOrPostCallback GetCalNumberOperationCompleted;

        // private System.Threading.SendOrPostCallback GetCalNumber2OperationCompleted;

        // private System.Threading.SendOrPostCallback GetTokenProcessTypeOperationCompleted;

        // private System.Threading.SendOrPostCallback ReloadConfigurationOperationCompleted;

        // private System.Threading.SendOrPostCallback ValidateUserOperationCompleted;

        //private System.Threading.SendOrPostCallback ChangePasswordOperationCompleted;

        //private System.Threading.SendOrPostCallback LoginCompactOperationCompleted;

        //private System.Threading.SendOrPostCallback SsoLoginOperationCompleted;

        //private System.Threading.SendOrPostCallback SsoLoggedUserOperationCompleted;

        //private System.Threading.SendOrPostCallback LoginOperationCompleted;

        //private System.Threading.SendOrPostCallback Login2OperationCompleted;

        //private System.Threading.SendOrPostCallback GetLoginInformationOperationCompleted;

        //private System.Threading.SendOrPostCallback LogOffOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserNameOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserDescriptionByIdOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserDescriptionByNameOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserEMailByNameOperationCompleted;

        //private System.Threading.SendOrPostCallback IsFloatingUserOperationCompleted;

        //private System.Threading.SendOrPostCallback IsWebUserOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDbOwnerOperationCompleted;

        //private System.Threading.SendOrPostCallback IsCompanySecuredOperationCompleted;

        //private System.Threading.SendOrPostCallback GetAuthenticationInformationsOperationCompleted;

        //private System.Threading.SendOrPostCallback GetAuthenticationNamesOperationCompleted;

        //private System.Threading.SendOrPostCallback DeleteAssociationOperationCompleted;

        //private System.Threading.SendOrPostCallback DeleteUserOperationCompleted;

        //private System.Threading.SendOrPostCallback DeleteCompanyOperationCompleted;

        //private System.Threading.SendOrPostCallback GetSystemDBConnectionStringOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDMSConnectionStringOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDMSDatabasesInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDataSynchroDatabasesInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback GetCompanyDatabasesInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback GetEditionOperationCompleted;

        //private System.Threading.SendOrPostCallback GetConfigurationStreamOperationCompleted;

        //private System.Threading.SendOrPostCallback GetCountryOperationCompleted;

        //private System.Threading.SendOrPostCallback GetProviderNameFromCompanyIdOperationCompleted;

        //private System.Threading.SendOrPostCallback GetInstallationVersionOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback GetUserInfoIDOperationCompleted;

        //private System.Threading.SendOrPostCallback TraceActionOperationCompleted;

        //private System.Threading.SendOrPostCallback HasUserAlreadyChangedPasswordTodayOperationCompleted;

        //private System.Threading.SendOrPostCallback GetBrandedApplicationTitleOperationCompleted;

        //private System.Threading.SendOrPostCallback GetMasterProductBrandedNameOperationCompleted;

        //private System.Threading.SendOrPostCallback GetMasterSolutionBrandedNameOperationCompleted;

        //private System.Threading.SendOrPostCallback GetBrandedProducerNameOperationCompleted;

        //private System.Threading.SendOrPostCallback GetBrandedProductTitleOperationCompleted;

        //private System.Threading.SendOrPostCallback GetBrandedKeyOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDBNetworkTypeOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDatabaseTypeOperationCompleted;

        //private System.Threading.SendOrPostCallback CanUseNamespaceOperationCompleted;

        //private System.Threading.SendOrPostCallback CacheCounterOperationCompleted;

        //private System.Threading.SendOrPostCallback CacheCounterGTGOperationCompleted;

        //private System.Threading.SendOrPostCallback SetCurrentComponentsOperationCompleted;

        //private System.Threading.SendOrPostCallback IsVirginActivationOperationCompleted;

        //private System.Threading.SendOrPostCallback HDOperationCompleted;

        //private System.Threading.SendOrPostCallback StoreMLUChoiceOperationCompleted;

        //private System.Threading.SendOrPostCallback SaveLicensedOperationCompleted;

        //private System.Threading.SendOrPostCallback SaveUserInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback DeleteUserInfoOperationCompleted;

        //private System.Threading.SendOrPostCallback DeleteLicensedOperationCompleted;

        //private System.Threading.SendOrPostCallback PrePingOperationCompleted;

        //private System.Threading.SendOrPostCallback PingOperationCompleted;

        //private System.Threading.SendOrPostCallback GetArticlesWithNamedCalOperationCompleted;

        //private System.Threading.SendOrPostCallback RefreshSecurityStatusOperationCompleted;

        //private System.Threading.SendOrPostCallback GetProxySupportVersionOperationCompleted;

        //private System.Threading.SendOrPostCallback GetProxySettingsOperationCompleted;

        //private System.Threading.SendOrPostCallback SetProxySettingsOperationCompleted;

        //private System.Threading.SendOrPostCallback GetCompanyLanguageOperationCompleted;

        //private System.Threading.SendOrPostCallback IsValidTokenOperationCompleted;

        //private System.Threading.SendOrPostCallback ReloadUserArticleBindingsOperationCompleted;

        //private System.Threading.SendOrPostCallback SbrillOperationCompleted;

        //private System.Threading.SendOrPostCallback GetCalTypeOperationCompleted;

        //private System.Threading.SendOrPostCallback IsUserLoggedOperationCompleted;

        //private System.Threading.SendOrPostCallback IsSecurityLightEnabledOperationCompleted;

        //private System.Threading.SendOrPostCallback IsSecurityLightAccessAllowedOperationCompleted;

        //private System.Threading.SendOrPostCallback GetDBCultureLCIDOperationCompleted;

        //private System.Threading.SendOrPostCallback SetMessageReadOperationCompleted;

        //private System.Threading.SendOrPostCallback GetMessagesQueueOperationCompleted;

        //private System.Threading.SendOrPostCallback GetOldMessagesOperationCompleted;

        //private System.Threading.SendOrPostCallback SendAccessMailOperationCompleted;

        //private System.Threading.SendOrPostCallback GetAspNetUserOperationCompleted;

        //private System.Threading.SendOrPostCallback GetConfigurationHashOperationCompleted;

        //private System.Threading.SendOrPostCallback UserCanAccessWebSitePrivateAreaOperationCompleted;

        //private System.Threading.SendOrPostCallback IsEasyBuilderDeveloperOperationCompleted;

        //private System.Threading.SendOrPostCallback SendErrorFileOperationCompleted;

        //private System.Threading.SendOrPostCallback DownloadPdbOperationCompleted;

        //private System.Threading.SendOrPostCallback GetMainSerialNumberOperationCompleted;

        //private System.Threading.SendOrPostCallback GetMLUExpiryDateOperationCompleted;

        //private System.Threading.SendOrPostCallback SendBalloonOperationCompleted;

        //private bool useDefaultCredentialsSetExplicitly;
        //private string ConnectionStringBuilder;

        /// <remarks/>
        public MicroareaLoginManager()
        {
            //TODO RSWEB this.Url = global::Microarea.TaskBuilderNet.Core.Properties.Settings.Default.Microarea_TaskBuilderNet_Core_loginMng_MicroareaLoginManager;

            this.UseDefaultCredentials = true;
           // this.useDefaultCredentialsSetExplicitly = false;
            xdoc = XDocument.Load("TempXmlParameters/LoginMngParams.xml");

        }

        public  string Url
        {
            get
            {
                return string.Empty;
            }
            set { }
        }

        public bool UseDefaultCredentials
        {
            get;
            set;
        }

        /// <remarks/>
       // public event InitCompletedEventHandler InitCompleted;

        /// <remarks/>
       // public event IsAliveCompletedEventHandler IsAliveCompleted;

        /// <remarks/>
       // public event IsCalAvailableCompletedEventHandler IsCalAvailableCompleted;

        /// <remarks/>
        //public event PingNeededCompletedEventHandler PingNeededCompleted;

        /// <remarks/>
       // public event SetCompanyInfoCompletedEventHandler SetCompanyInfoCompleted;

        /// <remarks/>
        //public event IsActivatedCompletedEventHandler IsActivatedCompleted;

        /// <remarks/>
       // public event IsSynchActivationCompletedEventHandler IsSynchActivationCompleted;

        /// <remarks/>
       // public event GetModulesCompletedEventHandler GetModulesCompleted;

        /// <remarks/>
        //public event GetCompanyUsersCompletedEventHandler GetCompanyUsersCompleted;

        /// <remarks/>
       // public event GetNonNTCompanyUsersCompletedEventHandler GetNonNTCompanyUsersCompleted;

        /// <remarks/>
       // public event GetCompanyRolesCompletedEventHandler GetCompanyRolesCompleted;

        /// <remarks/>
       // public event GetUserRolesCompletedEventHandler GetUserRolesCompleted;

        /// <remarks/>
       // public event EnumAllUsersCompletedEventHandler EnumAllUsersCompleted;

        /// <remarks/>
       // public event EnumAllCompanyUsersCompletedEventHandler EnumAllCompanyUsersCompleted;

        /// <remarks/>
        //public event GetRoleUsersCompletedEventHandler GetRoleUsersCompleted;

        /// <remarks/>
       /// public event EnumCompaniesCompletedEventHandler EnumCompaniesCompleted;

        /// <remarks/>
       // public event IsIntegratedSecurityUserCompletedEventHandler IsIntegratedSecurityUserCompleted;

        /// <remarks/>
       // public event GetLoggedUsersNumberCompletedEventHandler GetLoggedUsersNumberCompleted;

        /// <remarks/>
      //  public event GetCompanyLoggedUsersNumberCompletedEventHandler GetCompanyLoggedUsersNumberCompleted;

       // /// <remarks/>
        //public event GetLoggedUsersCompletedEventHandler GetLoggedUsersCompleted;

        /// <remarks/>
        // public event GetLoggedUsersAdvancedCompletedEventHandler GetLoggedUsersAdvancedCompleted;

        /// <remarks/>
        // public event GetCalNumberCompletedEventHandler GetCalNumberCompleted;

        /// <remarks/>
        // public event GetCalNumber2CompletedEventHandler GetCalNumber2Completed;

        /// <remarks/>
        // public event GetTokenProcessTypeCompletedEventHandler GetTokenProcessTypeCompleted;

        /// <remarks/>
        // public event ReloadConfigurationCompletedEventHandler ReloadConfigurationCompleted;

        /// <remarks/>
        // public event ValidateUserCompletedEventHandler ValidateUserCompleted;

        /// <remarks/>
        // public event ChangePasswordCompletedEventHandler ChangePasswordCompleted;

        // /// <remarks/>
        //  public event LoginCompactCompletedEventHandler LoginCompactCompleted;

        /// <remarks/>
        // public event SsoLoginCompletedEventHandler SsoLoginCompleted;

        /// <remarks/>
        // public event SsoLoggedUserCompletedEventHandler SsoLoggedUserCompleted;

        /// <remarks/>
        // public event LoginCompletedEventHandler LoginCompleted;

        /// <remarks/>
        //public event Login2CompletedEventHandler Login2Completed;

        /// <remarks/>
        // public event GetLoginInformationCompletedEventHandler GetLoginInformationCompleted;

        /// <remarks/>
        //  public event LogOffCompletedEventHandler LogOffCompleted;

        /// <remarks/>
        // public event GetUserNameCompletedEventHandler GetUserNameCompleted;

        /// <remarks/>
        // public event GetUserDescriptionByIdCompletedEventHandler GetUserDescriptionByIdCompleted;

        /// <remarks/>
        // public event GetUserDescriptionByNameCompletedEventHandler GetUserDescriptionByNameCompleted;

        /// <remarks/>
        // public event GetUserEMailByNameCompletedEventHandler GetUserEMailByNameCompleted;

        /// <remarks/>
        //  public event IsFloatingUserCompletedEventHandler IsFloatingUserCompleted;

        /// <remarks/>
        //  public event IsWebUserCompletedEventHandler IsWebUserCompleted;

        /// <remarks/>
        // public event GetDbOwnerCompletedEventHandler GetDbOwnerCompleted;

        /// <remarks/>
        //   public event IsCompanySecuredCompletedEventHandler IsCompanySecuredCompleted;

        /// <remarks/>
        //  public event GetAuthenticationInformationsCompletedEventHandler GetAuthenticationInformationsCompleted;

        /// <remarks/>
        //  public event GetAuthenticationNamesCompletedEventHandler GetAuthenticationNamesCompleted;

        /// <remarks/>
        //  public event DeleteAssociationCompletedEventHandler DeleteAssociationCompleted;

        /// <remarks/>
        //   public event DeleteUserCompletedEventHandler DeleteUserCompleted;

        /// <remarks/>
        //  public event DeleteCompanyCompletedEventHandler DeleteCompanyCompleted;

        /// <remarks/>
        //   public event GetSystemDBConnectionStringCompletedEventHandler GetSystemDBConnectionStringCompleted;

        /// <remarks/>
        //  public event GetDMSConnectionStringCompletedEventHandler GetDMSConnectionStringCompleted;

        /// <remarks/>
        // public event GetDMSDatabasesInfoCompletedEventHandler GetDMSDatabasesInfoCompleted;

        /// <remarks/>
        // public event GetDataSynchroDatabasesInfoCompletedEventHandler GetDataSynchroDatabasesInfoCompleted;

        /// <remarks/>
        //   public event GetCompanyDatabasesInfoCompletedEventHandler GetCompanyDatabasesInfoCompleted;

        /// <remarks/>
        // public event GetEditionCompletedEventHandler GetEditionCompleted;

        /// <remarks/>
       // public event GetConfigurationStreamCompletedEventHandler GetConfigurationStreamCompleted;

        /// <remarks/>
       // public event GetCountryCompletedEventHandler GetCountryCompleted;

        /// <remarks/>
      //  public event GetProviderNameFromCompanyIdCompletedEventHandler GetProviderNameFromCompanyIdCompleted;

        /// <remarks/>
       // public event GetInstallationVersionCompletedEventHandler GetInstallationVersionCompleted;

        /// <remarks/>
       // public event GetUserInfoCompletedEventHandler GetUserInfoCompleted;

        /// <remarks/>
       // public event GetUserInfoIDCompletedEventHandler GetUserInfoIDCompleted;

        /// <remarks/>
       // public event TraceActionCompletedEventHandler TraceActionCompleted;

        /// <remarks/>
      //  public event HasUserAlreadyChangedPasswordTodayCompletedEventHandler HasUserAlreadyChangedPasswordTodayCompleted;

        /// <remarks/>
      //  public event GetBrandedApplicationTitleCompletedEventHandler GetBrandedApplicationTitleCompleted;

        /// <remarks/>
       // public event GetMasterProductBrandedNameCompletedEventHandler GetMasterProductBrandedNameCompleted;

        /// <remarks/>
      //  public event GetMasterSolutionBrandedNameCompletedEventHandler GetMasterSolutionBrandedNameCompleted;

        /// <remarks/>
      //  public event GetBrandedProducerNameCompletedEventHandler GetBrandedProducerNameCompleted;

        /// <remarks/>
      //  public event GetBrandedProductTitleCompletedEventHandler GetBrandedProductTitleCompleted;

        /// <remarks/>
       // public event GetBrandedKeyCompletedEventHandler GetBrandedKeyCompleted;

        /// <remarks/>
       // public event GetDBNetworkTypeCompletedEventHandler GetDBNetworkTypeCompleted;

        /// <remarks/>
      //  public event GetDatabaseTypeCompletedEventHandler GetDatabaseTypeCompleted;

        /// <remarks/>
      //  public event CanUseNamespaceCompletedEventHandler CanUseNamespaceCompleted;

        /// <remarks/>
     //   public event CacheCounterCompletedEventHandler CacheCounterCompleted;

        /// <remarks/>
       // public event CacheCounterGTGCompletedEventHandler CacheCounterGTGCompleted;

        /// <remarks/>
      //  public event SetCurrentComponentsCompletedEventHandler SetCurrentComponentsCompleted;

        /// <remarks/>
      //  public event IsVirginActivationCompletedEventHandler IsVirginActivationCompleted;

        /// <remarks/>
      //  public event HDCompletedEventHandler HDCompleted;

        /// <remarks/>
      //  public event StoreMLUChoiceCompletedEventHandler StoreMLUChoiceCompleted;

        /// <remarks/>
       // public event SaveLicensedCompletedEventHandler SaveLicensedCompleted;

        /// <remarks/>
      //  public event SaveUserInfoCompletedEventHandler SaveUserInfoCompleted;

        /// <remarks/>
      //  public event DeleteUserInfoCompletedEventHandler DeleteUserInfoCompleted;

        /// <remarks/>
       // public event DeleteLicensedCompletedEventHandler DeleteLicensedCompleted;

        /// <remarks/>
     //   public event PrePingCompletedEventHandler PrePingCompleted;

        /// <remarks/>
      //  public event PingCompletedEventHandler PingCompleted;

        /// <remarks/>
      //  public event GetArticlesWithNamedCalCompletedEventHandler GetArticlesWithNamedCalCompleted;

        /// <remarks/>
       // public event RefreshSecurityStatusCompletedEventHandler RefreshSecurityStatusCompleted;

        /// <remarks/>
      //  public event GetProxySupportVersionCompletedEventHandler GetProxySupportVersionCompleted;

        /// <remarks/>
      //  public event GetProxySettingsCompletedEventHandler GetProxySettingsCompleted;

        /// <remarks/>
      //  public event SetProxySettingsCompletedEventHandler SetProxySettingsCompleted;

        /// <remarks/>
      //  public event GetCompanyLanguageCompletedEventHandler GetCompanyLanguageCompleted;

        /// <remarks/>
      //  public event IsValidTokenCompletedEventHandler IsValidTokenCompleted;

        /// <remarks/>
       // public event ReloadUserArticleBindingsCompletedEventHandler ReloadUserArticleBindingsCompleted;

        /// <remarks/>
      //  public event SbrillCompletedEventHandler SbrillCompleted;

        /// <remarks/>
      //  public event GetCalTypeCompletedEventHandler GetCalTypeCompleted;

        /// <remarks/>
      //  public event IsUserLoggedCompletedEventHandler IsUserLoggedCompleted;

        /// <remarks/>
       // public event IsSecurityLightEnabledCompletedEventHandler IsSecurityLightEnabledCompleted;

        /// <remarks/>
       // public event IsSecurityLightAccessAllowedCompletedEventHandler IsSecurityLightAccessAllowedCompleted;

        /// <remarks/>
      //  public event GetDBCultureLCIDCompletedEventHandler GetDBCultureLCIDCompleted;

        /// <remarks/>
      //  public event SetMessageReadCompletedEventHandler SetMessageReadCompleted;

        /// <remarks/>
       // public event GetMessagesQueueCompletedEventHandler GetMessagesQueueCompleted;

        /// <remarks/>
     //   public event GetOldMessagesCompletedEventHandler GetOldMessagesCompleted;

        /// <remarks/>
       // public event SendAccessMailCompletedEventHandler SendAccessMailCompleted;

        /// <remarks/>
     //   public event GetAspNetUserCompletedEventHandler GetAspNetUserCompleted;

        /// <remarks/>
      //  public event GetConfigurationHashCompletedEventHandler GetConfigurationHashCompleted;

        /// <remarks/>
      //  public event UserCanAccessWebSitePrivateAreaCompletedEventHandler UserCanAccessWebSitePrivateAreaCompleted;

        /// <remarks/>
      //  public event IsEasyBuilderDeveloperCompletedEventHandler IsEasyBuilderDeveloperCompleted;

        /// <remarks/>
      //  public event SendErrorFileCompletedEventHandler SendErrorFileCompleted;

        /// <remarks/>
      //  public event DownloadPdbCompletedEventHandler DownloadPdbCompleted;

        /// <remarks/>
        //  public event GetMainSerialNumberCompletedEventHandler GetMainSerialNumberCompleted;

        /// <remarks/>
      //  public event GetMLUExpiryDateCompletedEventHandler GetMLUExpiryDateCompleted;

        /// <remarks/>
       // public event SendBalloonCompletedEventHandler SendBalloonCompleted;

        /// <remarks/>

        public int Init(bool reboot, string authenticationToken)
        {
            return 0;
        }

        /// <remarks/>
        public void InitAsync(bool reboot, string authenticationToken)
        {
            //this.InitAsync(reboot, authenticationToken, null);
        }

        /// <remarks/>
        public void InitAsync(bool reboot, string authenticationToken, object userState)
        {

        }

        private void OnInitOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool IsAlive()
        {

            return true;
        }

        /// <remarks/>
        public void IsAliveAsync()
        {

        }

        /// <remarks/>
        public void IsAliveAsync(object userState)
        {

        }

        private void OnIsAliveOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool IsCalAvailable(string authenticationToken, string application, string functionality)
        {
            return true;
        }

        /// <remarks/>
        public void IsCalAvailableAsync(string authenticationToken, string application, string functionality)
        {

        }

        /// <remarks/>
        public void IsCalAvailableAsync(string authenticationToken, string application, string functionality, object userState)
        {

        }

        private void OnIsCalAvailableOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool PingNeeded(bool force)
        {

            return false;
        }

        /// <remarks/>
        public void PingNeededAsync(bool force)
        {

        }

        /// <remarks/>
        public void PingNeededAsync(bool force, object userState)
        {

        }

        private void OnPingNeededOperationCompleted(object arg)
        {

        }


        /// <remarks/>

        public void SetClientData(ClientData cd)
        {


        }

        /// <remarks/>

        public bool SetCompanyInfo(string authToken, string aName, string aValue)
        {
            return true;
        }

        /// <remarks/>
        public void SetCompanyInfoAsync(string authToken, string aName, string aValue)
        {

        }

        /// <remarks/>
        public void SetCompanyInfoAsync(string authToken, string aName, string aValue, object userState)
        {

        }

        private void OnSetCompanyInfoOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool IsActivated(string application, string functionality)
        {
            return true;
        }

        /// <remarks/>
        public void IsActivatedAsync(string application, string functionality)
        {

        }

        /// <remarks/>
        public void IsActivatedAsync(string application, string functionality, object userState)
        {

        }

        private void OnIsActivatedOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool IsSynchActivation()
        {
            return false;
        }

        /// <remarks/>
        public void IsSynchActivationAsync()
        {

        }

        /// <remarks/>
        public void IsSynchActivationAsync(object userState)
        {

        }

        private void OnIsSynchActivationOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetModules()
        {
            return new string[1];
        }

        /// <remarks/>
        public void GetModulesAsync()
        {

        }

        /// <remarks/>
        public void GetModulesAsync(object userState)
        {

        }

        private void OnGetModulesOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetCompanyUsers(string companyName)
        {

            return new string[1];
        }

        /// <remarks/>
        public void GetCompanyUsersAsync(string companyName)
        {

        }

        /// <remarks/>
        public void GetCompanyUsersAsync(string companyName, object userState)
        {

        }

        private void OnGetCompanyUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetNonNTCompanyUsers(string companyName)
        {

            return new string[1];
        }

        /// <remarks/>
        public void GetNonNTCompanyUsersAsync(string companyName)
        {

        }

        /// <remarks/>
        public void GetNonNTCompanyUsersAsync(string companyName, object userState)
        {

        }

        private void OnGetNonNTCompanyUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetCompanyRoles(string companyName)
        {
            return new string[1];
        }

        /// <remarks/>
        public void GetCompanyRolesAsync(string companyName)
        {

        }

        /// <remarks/>
        public void GetCompanyRolesAsync(string companyName, object userState)
        {

        }

        private void OnGetCompanyRolesOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetUserRoles(string companyName, string userName)
        {

            return new string[1];
        }

        /// <remarks/>
        public void GetUserRolesAsync(string companyName, string userName)
        {

        }

        /// <remarks/>
        public void GetUserRolesAsync(string companyName, string userName, object userState)
        {

        }

        private void OnGetUserRolesOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] EnumAllUsers()
        {

            return new string[1];
        }

        /// <remarks/>
        public void EnumAllUsersAsync()
        {

        }

        /// <remarks/>
        public void EnumAllUsersAsync(object userState)
        {

        }

        private void OnEnumAllUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers)
        {

            return new string[1];
        }

        /// <remarks/>
        public void EnumAllCompanyUsersAsync(int companyId, bool onlyNonNTUsers)
        {

        }

        /// <remarks/>
        public void EnumAllCompanyUsersAsync(int companyId, bool onlyNonNTUsers, object userState)
        {

        }

        private void OnEnumAllCompanyUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] GetRoleUsers(string companyName, string roleName)
        {

            return new string[1];
        }

        /// <remarks/>
        public void GetRoleUsersAsync(string companyName, string roleName)
        {

        }

        /// <remarks/>
        public void GetRoleUsersAsync(string companyName, string roleName, object userState)
        {

        }

        private void OnGetRoleUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string[] EnumCompanies(string userName)
        {

            return new string[1];
        }

        /// <remarks/>
        public void EnumCompaniesAsync(string userName)
        {

        }

        /// <remarks/>
        public void EnumCompaniesAsync(string userName, object userState)
        {

        }

        private void OnEnumCompaniesOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public bool IsIntegratedSecurityUser(string userName)
        {
            return false;
        }

        /// <remarks/>
        public void IsIntegratedSecurityUserAsync(string userName)
        {

        }

        /// <remarks/>
        public void IsIntegratedSecurityUserAsync(string userName, object userState)
        {

        }

        private void OnIsIntegratedSecurityUserOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int GetLoggedUsersNumber()
        {
            return 1;
        }

        /// <remarks/>
        public void GetLoggedUsersNumberAsync()
        {

        }

        /// <remarks/>
        public void GetLoggedUsersNumberAsync(object userState)
        {

        }

        private void OnGetLoggedUsersNumberOperationCompleted(object arg)
        {
        }

        /// <remarks/>

        public int GetCompanyLoggedUsersNumber(int companyId)
        {
            return 1;
        }

        /// <remarks/>
        public void GetCompanyLoggedUsersNumberAsync(int companyId)
        {

        }

        /// <remarks/>
        public void GetCompanyLoggedUsersNumberAsync(int companyId, object userState)
        {

        }

        private void OnGetCompanyLoggedUsersNumberOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string GetLoggedUsers()
        {
            return "sa";
        }

        /// <remarks/>
        public void GetLoggedUsersAsync()
        {

        }

        /// <remarks/>
        public void GetLoggedUsersAsync(object userState)
        {

        }

        private void OnGetLoggedUsersOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public string GetMobileToken(string token, int loginType)
        {
            return string.Empty;
        }



        /// <remarks/>

        public string GetIToken(string token)
        {
            return string.Empty;
        }


        /// <remarks/>

        public string GetLoggedUsersAdvanced(string token)
        {
            return string.Empty;
        }

        /// <remarks/>
        public void GetLoggedUsersAdvancedAsync(string token)
        {

        }

        /// <remarks/>
        public void GetLoggedUsersAdvancedAsync(string token, object userState)
        {

        }

        private void OnGetLoggedUsersAdvancedOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int GetCalNumber(out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal)
        {
            object[] results = new object[5];
            gdiConcurrent = ((int)(results[1]));
            unnamedCal = ((int)(results[2]));
            officeCal = ((int)(results[3]));
            tpCal = ((int)(results[4]));
            return 1;
        }

        /// <remarks/>
        public void GetCalNumberAsync()
        {

        }

        /// <remarks/>
        public void GetCalNumberAsync(object userState)
        {

        }

        private void OnGetCalNumberOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int GetCalNumber2(out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal, out int wmsCal, out int manufacturingCal)
        {
            object[] results = new object[7];
            gdiConcurrent = ((int)(results[1]));
            unnamedCal = ((int)(results[2]));
            officeCal = ((int)(results[3]));
            tpCal = ((int)(results[4]));
            wmsCal = ((int)(results[5]));
            manufacturingCal = ((int)(results[6]));
            return 1;
        }

        /// <remarks/>
        public void GetCalNumber2Async()
        {

        }

        /// <remarks/>
        public void GetCalNumber2Async(object userState)
        {

        }

        private void OnGetCalNumber2OperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int GetTokenProcessType(string token)
        {

            return 1;
        }

        /// <remarks/>
        public void GetTokenProcessTypeAsync(string token)
        {

        }

        /// <remarks/>
        public void GetTokenProcessTypeAsync(string token, object userState)
        {

        }

        private void OnGetTokenProcessTypeOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public void ReloadConfiguration()
        {

        }

        /// <remarks/>
        public void ReloadConfigurationAsync()
        {

        }

        /// <remarks/>
        public void ReloadConfigurationAsync(object userState)
        {

        }

        private void OnReloadConfigurationOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int ValidateUser(string userName, string password, bool winNtAuthentication, out string[] userCompanies, out int loginId, out bool userCannotChangePassword, out bool userMustChangePassword, out System.DateTime expiredDatePassword, out bool passwordNeverExpired, out bool expiredDateCannotChange)
        {
            object[] results = new object[8];
            userCompanies = ((string[])(results[1]));
            loginId = ((int)(results[2]));
            userCannotChangePassword = ((bool)(results[3]));
            userMustChangePassword = ((bool)(results[4]));
            expiredDatePassword = ((System.DateTime)(results[5]));
            passwordNeverExpired = ((bool)(results[6]));
            expiredDateCannotChange = ((bool)(results[7]));
            return 0;
        }

        /// <remarks/>
        public void ValidateUserAsync(string userName, string password, bool winNtAuthentication)
        {

        }

        /// <remarks/>
        public void ValidateUserAsync(string userName, string password, bool winNtAuthentication, object userState)
        {
        }

        private void OnValidateUserOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {

            return 0;
        }

        /// <remarks/>
        public void ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {

        }

        /// <remarks/>
        public void ChangePasswordAsync(string userName, string oldPassword, string newPassword, object userState)
        {

        }

        private void OnChangePasswordOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int LoginCompact(ref string userName, ref string companyName, string password, string askingProcess, bool overWriteLogin, out string authenticationToken)
        {


            userName = xdoc.Element("LoginInfo").Element("userName").Value;
            companyName = xdoc.Element("LoginInfo").Element("companyName").Value;
            authenticationToken = string.Empty;
            return 0;
        }

        /// <remarks/>
        public void LoginCompactAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin)
        {

        }

        /// <remarks/>
        public void LoginCompactAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin, object userState)
        {

        }

        private void OnLoginCompactOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int SsoLogin(ref LoginProperties loginProperties)
        {

            return 1;
        }

        /// <remarks/>
        public void SsoLoginAsync(LoginProperties loginProperties)
        {

        }

        /// <remarks/>
        public void SsoLoginAsync(LoginProperties loginProperties, object userState)
        {

        }

        private void OnSsoLoginOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int SsoLoggedUser(ref LoginProperties loginProperties)
        {

            return 1;
        }

        /// <remarks/>
        public void SsoLoggedUserAsync(LoginProperties loginProperties)
        {

        }

        /// <remarks/>
        public void SsoLoggedUserAsync(LoginProperties loginProperties, object userState)
        {

        }

        private void OnSsoLoggedUserOperationCompleted(object arg)
        {

        }

        /// <remarks/>

        public int Login(
                    ref string userName,
                    ref string companyName,
                    string password,
                    string askingProcess,
                    bool overWriteLogin,
                    out bool admin,
                    out string authenticationToken,
                    out int companyId,
                    out string dbName,
                    out string dbServer,
                    out int providerId,
                    out bool security,
                    out bool auditing,
                    out bool useKeyedUpdate,
                    out bool transactionUse,
                    out string preferredLanguage,
                    out string applicationLanguage,
                    out string providerName,
                    out string providerDescription,
                    out bool useConstParameter,
                    out bool stripTrailingSpaces,
                    out string providerCompanyConnectionString,
                    out string nonProviderCompanyConnectionString,
                    out string dbUser,
                    out string activationDB)
        {

            userName = xdoc.Element("LoginInfo").Element("userName").Value;
            companyName = xdoc.Element("LoginInfo").Element("companyName").Value;
            admin = true;
            authenticationToken = string.Empty;
            companyId = 1;
            dbName = xdoc.Element("LoginInfo").Element("dbName").Value;
            dbServer = xdoc.Element("LoginInfo").Element("dbServer").Value;
            providerId = 1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = true;
            preferredLanguage = xdoc.Element("LoginInfo").Element("preferredLanguage").Value;
            applicationLanguage = xdoc.Element("LoginInfo").Element("applicationLanguage").Value;
            providerName = xdoc.Element("LoginInfo").Element("providerName").Value;
            providerDescription = string.Empty;
            useConstParameter = true;
            stripTrailingSpaces = true;
            dbUser = xdoc.Element("LoginInfo").Element("providerName").Value;
            //System.Data.SqlClient.SqlConnectionStringBuilder ccstr = new System.Data.SqlClient.SqlConnectionStringBuilder();
            ccstr.UserID = userName;
            ccstr.Password = password;
            ccstr.InitialCatalog = dbName;
            ccstr.DataSource = dbServer;
            providerCompanyConnectionString = ccstr.ConnectionString;
            nonProviderCompanyConnectionString = ccstr.ConnectionString;
            activationDB = string.Empty;
            return 0;
        }

        /// <remarks/>
        public void LoginAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void LoginAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnLoginOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        public int Login2(
                    ref string userName,
                    ref string companyName,
                    string password,
                    string askingProcess,
                    string macIp,
                    bool overWriteLogin,
                    out bool admin,
                    out string authenticationToken,
                    out int companyId,
                    out string dbName,
                    out string dbServer,
                    out int providerId,
                    out bool security,
                    out bool auditing,
                    out bool useKeyedUpdate,
                    out bool transactionUse,
                    out string preferredLanguage,
                    out string applicationLanguage,
                    out string providerName,
                    out string providerDescription,
                    out bool useConstParameter,
                    out bool stripTrailingSpaces,
                    out string providerCompanyConnectionString,
                    out string nonProviderCompanyConnectionString,
                    out string dbUser,
                    out string activationDB)
        {
            userName = xdoc.Element("LoginInfo").Element("userName").Value;
            companyName = xdoc.Element("LoginInfo").Element("companyName").Value;
            admin = true;
            authenticationToken = string.Empty;
            companyId = 1;
            dbName = xdoc.Element("LoginInfo").Element("dbName").Value;
            dbServer = xdoc.Element("LoginInfo").Element("dbServer").Value;
            providerId = 1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = true;
            preferredLanguage = xdoc.Element("LoginInfo").Element("preferredLanguage").Value;
            applicationLanguage = xdoc.Element("LoginInfo").Element("applicationLanguage").Value;
            providerName = xdoc.Element("LoginInfo").Element("providerName").Value;
            providerDescription = string.Empty;
            useConstParameter = true;
            stripTrailingSpaces = true;
            dbUser = xdoc.Element("LoginInfo").Element("dbUser").Value;
            ccstr.UserID = userName;
            ccstr.Password = password;
            ccstr.InitialCatalog = dbName;
            ccstr.DataSource = dbServer;
            providerCompanyConnectionString = ccstr.ConnectionString;
            nonProviderCompanyConnectionString = ccstr.ConnectionString;
            activationDB = string.Empty;
            return 0;
        }

        /// <remarks/>
        public void Login2Async(string userName, string companyName, string password, string askingProcess, string macIp, bool overWriteLogin)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void Login2Async(string userName, string companyName, string password, string askingProcess, string macIp, bool overWriteLogin, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnLogin2OperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public int GetUsagePercentageOnDBSize()
        {

            return 80;
        }


        public bool ConfirmToken(
                    string authenticationToken, string procType)
        {
            return true;
        }


        /// <remarks/>

        public bool GetLoginInformation(
                    string authenticationToken,
                    out string userName,
                    out int loginId,
                    out string companyName,
                    out int companyId,
                    out bool admin,
                    out string dbName,
                    out string dbServer,
                    out int providerId,
                    out bool security,
                    out bool auditing,
                    out bool useKeyedUpdate,
                    out bool transactionUse,
                    out bool useUnicode,
                    out string preferredLanguage,
                    out string applicationLanguage,
                    out string providerName,
                    out string providerDescription,
                    out bool useConstParameter,
                    out bool stripTrailingSpaces,
                    out string providerCompanyConnectionString,
                    out string nonProviderCompanyConnectionString,
                    out string dbUser,
                    out string processName,
                    out string userDescription,
                    out string email,
                    out bool easyBuilderDeveloper,
                    out bool rowSecurity
            )
        {

            userName = xdoc.Element("LoginInfo").Element("userName").Value;
            loginId = 1;
            companyName = xdoc.Element("LoginInfo").Element("userName").Value;
            companyId = 1;
            admin = true;
            dbName = xdoc.Element("LoginInfo").Element("userName").Value;
            dbServer = xdoc.Element("LoginInfo").Element("userName").Value;
            providerId = 1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = true;
            useUnicode = true;
            preferredLanguage = xdoc.Element("LoginInfo").Element("userName").Value;
            applicationLanguage = xdoc.Element("LoginInfo").Element("userName").Value;
            providerName = xdoc.Element("LoginInfo").Element("userName").Value;
            providerDescription = string.Empty;
            useConstParameter = true;
            stripTrailingSpaces = true;

            providerCompanyConnectionString = ccstr.ConnectionString;
            nonProviderCompanyConnectionString = ccstr.ConnectionString;
            dbUser = xdoc.Element("LoginInfo").Element("dbUser").Value;
            processName = string.Empty;
            userDescription = string.Empty;
            email = string.Empty;
            easyBuilderDeveloper = false;
            rowSecurity = false;
            return true;
        }

        /// <remarks/>
        public void GetLoginInformationAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetLoginInformationAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetLoginInformationOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public int LoginViaInfinityToken2(string cryptedToken, string username, string password, string company, out string authenticationToken)
        {
            throw new NotImplementedException();



        }
        /// <remarks/>

        public void SSOLogOff(string cryptedToken)
        {
            throw new NotImplementedException();
        }


        /// <remarks/>

        public void LogOff(string authenticationToken)
        {
            ccstr.Clear();
        }

        /// <remarks/>
        public void LogOffAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void LogOffAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnLogOffOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserName(int loginId)
        {

            return xdoc.Element("LoginInfo").Element("userName").Value; ;
        }

        /// <remarks/>
        public void GetUserNameAsync(int loginId)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserNameAsync(int loginId, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserDescriptionById(int loginId)
        {

            return string.Empty;
        }

        /// <remarks/>
        public void GetUserDescriptionByIdAsync(int loginId)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserDescriptionByIdAsync(int loginId, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserDescriptionByIdOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserDescriptionByName(string login)
        {

            return string.Empty; ;
        }

        /// <remarks/>
        public void GetUserDescriptionByNameAsync(string login)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserDescriptionByNameAsync(string login, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserDescriptionByNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserEMailByName(string login)
        {

            return xdoc.Element("LoginInfo").Element("userName").Value;
        }

        /// <remarks/>
        public void GetUserEMailByNameAsync(string login)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserEMailByNameAsync(string login, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserEMailByNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsFloatingUser(string loginName, out bool floating)
        {
            floating = false;
            return false;
        }

        /// <remarks/>
        public void IsFloatingUserAsync(string loginName)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsFloatingUserAsync(string loginName, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsFloatingUserOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsWinNT(int loginId)
        {

            return false;
        }


        /// <remarks/>

        public bool IsWebUser(string loginName, out bool web)
        {
            web = true;
            return true;
        }

        /// <remarks/>
        public void IsWebUserAsync(string loginName)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsWebUserAsync(string loginName, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsWebUserOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetDbOwner(int companyId)
        {

            return "sa";
        }

        /// <remarks/>
        public void GetDbOwnerAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDbOwnerAsync(int companyId, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDbOwnerOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsCompanySecured(int companyId)
        {

            return false;
        }

        /// <remarks/>
        public void IsCompanySecuredAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsCompanySecuredAsync(int companyId, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsCompanySecuredOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool webLogin)
        {

            loginId = 1;
            companyId = 1;
            webLogin = true;
            return true;
        }

        /// <remarks/>
        public void GetAuthenticationInformationsAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetAuthenticationInformationsAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetAuthenticationInformationsOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName)
        {

            userName = xdoc.Element("LoginInfo").Element("userName").Value;
            companyName = xdoc.Element("LoginInfo").Element("companyName").Value;
            return true;
        }

        /// <remarks/>
        public void GetAuthenticationNamesAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetAuthenticationNamesAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetAuthenticationNamesOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool DeleteAssociation(int loginId, int companyId, string authenticationToken)
        {
            return true;
        }

        /// <remarks/>
        public void DeleteAssociationAsync(int loginId, int companyId, string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DeleteAssociationAsync(int loginId, int companyId, string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteAssociationOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool DeleteUser(int loginId, string authenticationToken)
        {
            return true;
        }

        /// <remarks/>
        public void DeleteUserAsync(int loginId, string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DeleteUserAsync(int loginId, string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteUserOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool DeleteCompany(int companyId, string authenticationToken)
        {
            return true;
        }

        /// <remarks/>
        public void DeleteCompanyAsync(int companyId, string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DeleteCompanyAsync(int companyId, string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteCompanyOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetSystemDBConnectionString(string authenticationToken)
        {
            return ccstr.ConnectionString;
        }

        /// <remarks/>
        public void GetSystemDBConnectionStringAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetSystemDBConnectionStringAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetSystemDBConnectionStringOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetDMSConnectionString(string authenticationToken)
        {
            return string.Empty;
        }

        /// <remarks/>
        public void GetDMSConnectionStringAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDMSConnectionStringAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException(); ;
        }

        private void OnGetDMSConnectionStringOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        //public DmsDatabaseInfo[] GetDMSDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetDMSDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((DmsDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetDMSDatabasesInfoAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDMSDatabasesInfoAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDMSDatabasesInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }


        /// <remarks/>

        //public DataSynchroDatabaseInfo[] GetDataSynchroDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetDataSynchroDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((DataSynchroDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetDataSynchroDatabasesInfoAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDataSynchroDatabasesInfoAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDataSynchroDatabasesInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }


        /// <remarks/>

        //public TbSenderDatabaseInfo[] GetCompanyDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetCompanyDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((TbSenderDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetCompanyDatabasesInfoAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetCompanyDatabasesInfoAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetCompanyDatabasesInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetEdition()
        {

            return "1.0";
        }

        /// <remarks/>
        public void GetEditionAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetEditionAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetEditionOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public byte[] GetConfigurationStream()
        {

            return new byte[1];
        }

        /// <remarks/>
        public void GetConfigurationStreamAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetConfigurationStreamAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetConfigurationStreamOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetCountry()
        {

            return "IT";
        }

        /// <remarks/>
        public void GetCountryAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetCountryAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetCountryOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetProviderNameFromCompanyId(int companyId)
        {

            return "SQL";
        }

        /// <remarks/>
        public void GetProviderNameFromCompanyIdAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetProviderNameFromCompanyIdAsync(int companyId, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetProviderNameFromCompanyIdOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetInstallationVersion(out string productName, out System.DateTime buildDate, out System.DateTime instDate, out int build)
        {

            productName = "RSWEB";
            buildDate = System.DateTime.Today;
            instDate = System.DateTime.Today;
            build = 1;
            return "1.0";
        }

        /// <remarks/>
        public void GetInstallationVersionAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetInstallationVersionAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetInstallationVersionOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserInfo()
        {

            return xdoc.Element("LoginInfo").Element("userName").Value;

        }

        /// <remarks/>
        public void GetUserInfoAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserInfoAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetUserInfoID()
        {

            return xdoc.Element("LoginInfo").Element("userName").Value;
        }

        /// <remarks/>
        public void GetUserInfoIDAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetUserInfoIDAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetUserInfoIDOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void TraceAction(string company, string login, int type, string processName, string winUser, string location)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void TraceActionAsync(string company, string login, int type, string processName, string winUser, string location)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void TraceActionAsync(string company, string login, int type, string processName, string winUser, string location, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnTraceActionOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool Sql2012Allowed(string authToken)
        {
            return true;
        }


        /// <remarks/>

        public bool HasUserAlreadyChangedPasswordToday(string user)
        {
            return true;
        }

        /// <remarks/>
        public void HasUserAlreadyChangedPasswordTodayAsync(string user)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void HasUserAlreadyChangedPasswordTodayAsync(string user, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnHasUserAlreadyChangedPasswordTodayOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetBrandedApplicationTitle(string application)
        {

            return "RSWeb";
        }

        /// <remarks/>
        public void GetBrandedApplicationTitleAsync(string application)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetBrandedApplicationTitleAsync(string application, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetBrandedApplicationTitleOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetMasterProductBrandedName()
        {
            return "RSWeb";
        }

        /// <remarks/>
        public void GetMasterProductBrandedNameAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMasterProductBrandedNameAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetMasterProductBrandedNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetMasterSolution()
        {
            return string.Empty;
        }

        /// <remarks/>

        public string GetMasterSolutionBrandedName()
        {
            return "RSWeb";
        }

        /// <remarks/>
        public void GetMasterSolutionBrandedNameAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMasterSolutionBrandedNameAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetMasterSolutionBrandedNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetBrandedProducerName()
        {
            return string.Empty;
        }

        /// <remarks/>
        public void GetBrandedProducerNameAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetBrandedProducerNameAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetBrandedProducerNameOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetBrandedProductTitle()
        {
            return "RSWeb";
        }

        /// <remarks/>
        public void GetBrandedProductTitleAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetBrandedProductTitleAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetBrandedProductTitleOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetBrandedKey(string source)
        {
            return "RSWeb";
        }

        /// <remarks/>
        public void GetBrandedKeyAsync(string source)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetBrandedKeyAsync(string source, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetBrandedKeyOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetEditionType()
        {

            return "1.0";
        }

        /// <remarks/>

        public Object GetDBNetworkType()
        {

            throw new NotImplementedException(); ;
        }

        /// <remarks/>
        public void GetDBNetworkTypeAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDBNetworkTypeAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDBNetworkTypeOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetDatabaseType(string providerName)
        {

            return "SQL";
        }

        /// <remarks/>
        public void GetDatabaseTypeAsync(string providerName)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDatabaseTypeAsync(string providerName, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDatabaseTypeOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool CanUseNamespace(string nameSpace, string authenticationToken, GrantType grantType)
        {
            return true;
        }

        /// <remarks/>
        public void CanUseNamespaceAsync(string nameSpace, string authenticationToken, GrantType grantType)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void CanUseNamespaceAsync(string nameSpace, string authenticationToken, GrantType grantType, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnCanUseNamespaceOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool CacheCounter()
        {
            return true;
        }

        /// <remarks/>
        public void CacheCounterAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void CacheCounterAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnCacheCounterOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object CacheCounterGTG()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void CacheCounterGTGAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void CacheCounterGTGAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnCacheCounterGTGOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object SetCurrentComponents(out int dte)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SetCurrentComponentsAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SetCurrentComponentsAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSetCurrentComponentsOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsVirginActivation()
        {
            return false;
        }

        /// <remarks/>
        public void IsVirginActivationAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsVirginActivationAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsVirginActivationOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public int HD()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void HDAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void HDAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnHDOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void StoreMLUChoice(bool userChoseMluInChargeToMicroarea)
        {

        }

        /// <remarks/>
        public void StoreMLUChoiceAsync(bool userChoseMluInChargeToMicroarea)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void StoreMLUChoiceAsync(bool userChoseMluInChargeToMicroarea, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnStoreMLUChoiceOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool SaveLicensed(string xml, string name)
        {
            return true;
        }

        /// <remarks/>
        public void SaveLicensedAsync(string xml, string name)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SaveLicensedAsync(string xml, string name, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSaveLicensedOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string ValidateIToken(string itoken, string authenticationToken)
        {
            return string.Empty;
        }

        /// <remarks/>

        public bool SaveUserInfo(string xml)
        {
            return true;
        }

        /// <remarks/>
        public void SaveUserInfoAsync(string xml)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SaveUserInfoAsync(string xml, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSaveUserInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void DeleteUserInfo()
        {

        }

        /// <remarks/>
        public void DeleteUserInfoAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DeleteUserInfoAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteUserInfoOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void DeleteLicensed(string name)
        {

        }

        /// <remarks/>
        public void DeleteLicensedAsync(string name)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DeleteLicensedAsync(string name, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteLicensedOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string PrePing()
        {
            return string.Empty;
        }

        /// <remarks/>
        public void PrePingAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void PrePingAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnPrePingOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string Ping()
        {
            return string.Empty;
        }

        /// <remarks/>
        public void PingAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void PingAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnPingOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }


        /// <remarks/>

        public ModuleNameInfo[] GetArticlesWithFloatingCal()
        {

            throw new NotImplementedException();
        }

        /// <remarks/>

        public void RefreshFloatingMark()
        {


        }

        /// <remarks/>

        public ModuleNameInfo[] GetArticlesWithNamedCal()
        {

            return new ModuleNameInfo[1];
        }

        /// <remarks/>
        public void GetArticlesWithNamedCalAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetArticlesWithNamedCalAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetArticlesWithNamedCalOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void RefreshSecurityStatus()
        {

        }

        /// <remarks/>
        public void RefreshSecurityStatusAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void RefreshSecurityStatusAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnRefreshSecurityStatusOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public int GetProxySupportVersion()
        {

            return 1;
        }

        /// <remarks/>
        public void GetProxySupportVersionAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetProxySupportVersionAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetProxySupportVersionOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }



        public ProxySettings GetProxySettings()
        {

            return new ProxySettings();
        }

        /// <remarks/>
        public void GetProxySettingsAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetProxySettingsAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetProxySettingsOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void SetProxySettings(Object proxySettings)
        {

        }

        /// <remarks/>
        public void SetProxySettingsAsync(Object proxySettings)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SetProxySettingsAsync(Object proxySettings, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSetProxySettingsOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool GetCompanyLanguage(int companyID, out string cultureUI, out string culture)
        {

            cultureUI = "it";
            culture = "it";
            return true;
        }

        /// <remarks/>
        public void GetCompanyLanguageAsync(int companyID)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetCompanyLanguageAsync(int companyID, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetCompanyLanguageOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }


        /// <remarks/>

        public bool VerifyDBSize()
        {
            return true;
        }

        /// <remarks/>

        public bool IsValidToken(string authenticationToken)
        {
            return true;
        }

        /// <remarks/>
        public void IsValidTokenAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsValidTokenAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsValidTokenOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void ReloadUserArticleBindings(string authenticationToken)
        {

        }

        /// <remarks/>
        public void ReloadUserArticleBindingsAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void ReloadUserArticleBindingsAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnReloadUserArticleBindingsOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool Sbrill(string token)
        {
            return true;
        }

        /// <remarks/>
        public void SbrillAsync(string token)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SbrillAsync(string token, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSbrillOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object GetCalType(string token)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetCalTypeAsync(string token)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetCalTypeAsync(string token, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetCalTypeOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsUserLogged(int loginID)
        {
            return true;
        }

        /// <remarks/>
        public void IsUserLoggedAsync(int loginID)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsUserLoggedAsync(int loginID, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsUserLoggedOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsSecurityLightEnabled()
        {
            return false;
        }

        /// <remarks/>
        public void IsSecurityLightEnabledAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsSecurityLightEnabledAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsSecurityLightEnabledOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsSecurityLightAccessAllowed(string nameSpace, string authenticationToken, bool unattended)
        {
            return false;
        }

        /// <remarks/>
        public void IsSecurityLightAccessAllowedAsync(string nameSpace, string authenticationToken, bool unattended)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsSecurityLightAccessAllowedAsync(string nameSpace, string authenticationToken, bool unattended, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnIsSecurityLightAccessAllowedOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public int GetDBCultureLCID(int companyID)
        {
            return 1;
        }

        /// <remarks/>
        public void GetDBCultureLCIDAsync(int companyID)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetDBCultureLCIDAsync(int companyID, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetDBCultureLCIDOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void SetMessageRead(string userName, string messageID)
        {

        }

        /// <remarks/>
        public void SetMessageReadAsync(string userName, string messageID)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SetMessageReadAsync(string userName, string messageID, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSetMessageReadOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object[] GetImmediateMessagesQueue(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object[] GetMessagesQueue(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMessagesQueueAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMessagesQueueAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetMessagesQueueOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public Object[] GetOldMessages(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetOldMessagesAsync(string authenticationToken)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetOldMessagesAsync(string authenticationToken, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetOldMessagesOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool SendAccessMail()
        {
            return true;
        }

        /// <remarks/>
        public void SendAccessMailAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SendAccessMailAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSendAccessMailOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetAspNetUser()
        {
            return xdoc.Element("LoginInfo").Element("userName").Value;
        }

        /// <remarks/>
        public void GetAspNetUserAsync()
        {

            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetAspNetUserAsync(object userState)
        {

            throw new NotImplementedException();
        }

        private void OnGetAspNetUserOperationCompleted(object arg)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetConfigurationHash()
        {

            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetConfigurationHashAsync()
        {

            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetConfigurationHashAsync(object userState)
        {

            throw new NotImplementedException();
        }

        private void OnGetConfigurationHashOperationCompleted(object arg)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool UserCanAccessWebSitePrivateArea(int loginId)
        {
            return true;
        }

        /// <remarks/>
        public void UserCanAccessWebSitePrivateAreaAsync(int loginId)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>
        public void UserCanAccessWebSitePrivateAreaAsync(int loginId, object userState)
        {

            throw new NotImplementedException();
        }

        private void OnUserCanAccessWebSitePrivateAreaOperationCompleted(object arg)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool IsEasyBuilderDeveloper(string authenticationToken)
        {
            return false;
        }

        /// <remarks/>
        public void IsEasyBuilderDeveloperAsync(string authenticationToken)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>
        public void IsEasyBuilderDeveloperAsync(string authenticationToken, object userState)
        {

            throw new NotImplementedException();
        }

        private void OnIsEasyBuilderDeveloperOperationCompleted(object arg)
        {

            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool SendErrorFile(string LogFile, out string ErrorMessage)
        {
            ErrorMessage = string.Empty;
            return true;
        }

        /// <remarks/>
        public void SendErrorFileAsync(string LogFile)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SendErrorFileAsync(string LogFile, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSendErrorFileOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public bool DownloadPdb(string PdbFile, out string ErrorMessage)
        {

            ErrorMessage = string.Empty;
            return true;
        }

        /// <remarks/>
        public void DownloadPdbAsync(string PdbFile)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void DownloadPdbAsync(string PdbFile, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnDownloadPdbOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetMainSerialNumber()
        {
            return string.Empty;
        }

        /// <remarks/>
        public void GetMainSerialNumberAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMainSerialNumberAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetMainSerialNumberOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public string GetMLUExpiryDate()
        {
            return string.Empty; ;
        }

        /// <remarks/>
        public void GetMLUExpiryDateAsync()
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void GetMLUExpiryDateAsync(object userState)
        {
            throw new NotImplementedException();
        }

        private void OnGetMLUExpiryDateOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>

        public void AdvancedSendTaggedBalloon(string authenticationToken,
                string bodyMessage,
                DateTime expiryDate,
                int messageType = 0,
                string[] recipients = null,
                int sensation = 0,
                bool historicize = true,
                bool immediate = false,
                int timer = 0,
                string tag = null)
        {
            throw new NotImplementedException();
        }
        /// <remarks/>

        public void AdvancedSendBalloon(string authenticationToken,
                string bodyMessage,
                DateTime expiryDate,
                int messageType = 0,
                string[] recipients = null,
                int sensation = 0,
                bool historicize = true,
                bool immediate = false,
                int timer = 0)
        {
            throw new NotImplementedException();
        }
        /// <remarks/>

        public void DeleteMessageFromQueue(string messageID)
        {

        }

        /// <remarks/>

        public void PurgeMessageByTag(string tag, string user)
        {

        }

        /// <remarks/>

        public void SendBalloon(string authenticationToken, string bodyMessage, Object messageType, string[] recipients)
        {

        }

        /// <remarks/>
        public void SendBalloonAsync(string authenticationToken, string bodyMessage, Object messageType, string[] recipients)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public void SendBalloonAsync(string authenticationToken, string bodyMessage, Object messageType, string[] recipients, object userState)
        {
            throw new NotImplementedException();
        }

        private void OnSendBalloonOperationCompleted(object arg)
        {
            throw new NotImplementedException();
        }

        /// <remarks/>
        public  void CancelAsync(object userState)
        {

        }

        private bool IsLocalFileSystemWebService(string url)
        {
            return true;
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://microarea.it/LoginManager/")]
        public partial class FirewallCredentialsSettings
        {

            private bool needsCredentialsField;

            private string domainField;

            private string nameField;

            private string passwordField;

            /// <remarks/>
            public bool NeedsCredentials
            {
                get
                {
                    return this.needsCredentialsField;
                }
                set
                {
                    this.needsCredentialsField = value;
                }
            }

            /// <remarks/>
            public string Domain
            {
                get
                {
                    return this.domainField;
                }
                set
                {
                    this.domainField = value;
                }
            }

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public string Password
            {
                get
                {
                    return this.passwordField;
                }
                set
                {
                    this.passwordField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void InitCompletedEventHandler(object sender, InitCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class InitCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal InitCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsAliveCompletedEventHandler(object sender, IsAliveCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsAliveCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsAliveCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsCalAvailableCompletedEventHandler(object sender, IsCalAvailableCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsCalAvailableCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsCalAvailableCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void PingNeededCompletedEventHandler(object sender, PingNeededCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class PingNeededCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal PingNeededCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SetCompanyInfoCompletedEventHandler(object sender, SetCompanyInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SetCompanyInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SetCompanyInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsActivatedCompletedEventHandler(object sender, IsActivatedCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsActivatedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsActivatedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsSynchActivationCompletedEventHandler(object sender, IsSynchActivationCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsSynchActivationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsSynchActivationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetModulesCompletedEventHandler(object sender, GetModulesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetModulesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetModulesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCompanyUsersCompletedEventHandler(object sender, GetCompanyUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCompanyUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCompanyUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetNonNTCompanyUsersCompletedEventHandler(object sender, GetNonNTCompanyUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetNonNTCompanyUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetNonNTCompanyUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCompanyRolesCompletedEventHandler(object sender, GetCompanyRolesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCompanyRolesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCompanyRolesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserRolesCompletedEventHandler(object sender, GetUserRolesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserRolesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserRolesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void EnumAllUsersCompletedEventHandler(object sender, EnumAllUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class EnumAllUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal EnumAllUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void EnumAllCompanyUsersCompletedEventHandler(object sender, EnumAllCompanyUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class EnumAllCompanyUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal EnumAllCompanyUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetRoleUsersCompletedEventHandler(object sender, GetRoleUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetRoleUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetRoleUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void EnumCompaniesCompletedEventHandler(object sender, EnumCompaniesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class EnumCompaniesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal EnumCompaniesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsIntegratedSecurityUserCompletedEventHandler(object sender, IsIntegratedSecurityUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsIntegratedSecurityUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsIntegratedSecurityUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetLoggedUsersNumberCompletedEventHandler(object sender, GetLoggedUsersNumberCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetLoggedUsersNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetLoggedUsersNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCompanyLoggedUsersNumberCompletedEventHandler(object sender, GetCompanyLoggedUsersNumberCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCompanyLoggedUsersNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCompanyLoggedUsersNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetLoggedUsersCompletedEventHandler(object sender, GetLoggedUsersCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetLoggedUsersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetLoggedUsersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetLoggedUsersAdvancedCompletedEventHandler(object sender, GetLoggedUsersAdvancedCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetLoggedUsersAdvancedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetLoggedUsersAdvancedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCalNumberCompletedEventHandler(object sender, GetCalNumberCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCalNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCalNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public int gdiConcurrent
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[1]));
                }
            }

            /// <remarks/>
            public int unnamedCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[2]));
                }
            }

            /// <remarks/>
            public int officeCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[3]));
                }
            }

            /// <remarks/>
            public int tpCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[4]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCalNumber2CompletedEventHandler(object sender, GetCalNumber2CompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCalNumber2CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCalNumber2CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public int gdiConcurrent
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[1]));
                }
            }

            /// <remarks/>
            public int unnamedCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[2]));
                }
            }

            /// <remarks/>
            public int officeCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[3]));
                }
            }

            /// <remarks/>
            public int tpCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[4]));
                }
            }

            /// <remarks/>
            public int wmsCal
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[5]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetTokenProcessTypeCompletedEventHandler(object sender, GetTokenProcessTypeCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetTokenProcessTypeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetTokenProcessTypeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void ReloadConfigurationCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void ValidateUserCompletedEventHandler(object sender, ValidateUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class ValidateUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal ValidateUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public string[] userCompanies
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string[])(this.results[1]));
                }
            }

            /// <remarks/>
            public int loginId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[2]));
                }
            }

            /// <remarks/>
            public bool userCannotChangePassword
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[3]));
                }
            }

            /// <remarks/>
            public bool userMustChangePassword
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[4]));
                }
            }

            /// <remarks/>
            public System.DateTime expiredDatePassword
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((System.DateTime)(this.results[5]));
                }
            }

            /// <remarks/>
            public bool passwordNeverExpired
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[6]));
                }
            }

            /// <remarks/>
            public bool expiredDateCannotChange
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[7]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void ChangePasswordCompletedEventHandler(object sender, ChangePasswordCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class ChangePasswordCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal ChangePasswordCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void LoginCompactCompletedEventHandler(object sender, LoginCompactCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class LoginCompactCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal LoginCompactCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public string userName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public string companyName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[2]));
                }
            }

            /// <remarks/>
            public string authenticationToken
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[3]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SsoLoginCompletedEventHandler(object sender, SsoLoginCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SsoLoginCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SsoLoginCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public LoginProperties loginProperties
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((LoginProperties)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SsoLoggedUserCompletedEventHandler(object sender, SsoLoggedUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SsoLoggedUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SsoLoggedUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public LoginProperties loginProperties
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((LoginProperties)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void LoginCompletedEventHandler(object sender, LoginCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class LoginCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal LoginCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public string userName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public string companyName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[2]));
                }
            }

            /// <remarks/>
            public bool admin
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[3]));
                }
            }

            /// <remarks/>
            public string authenticationToken
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[4]));
                }
            }

            /// <remarks/>
            public int companyId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[5]));
                }
            }

            /// <remarks/>
            public string dbName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[6]));
                }
            }

            /// <remarks/>
            public string dbServer
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[7]));
                }
            }

            /// <remarks/>
            public int providerId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[8]));
                }
            }

            /// <remarks/>
            public bool security
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[9]));
                }
            }

            /// <remarks/>
            public bool auditing
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[10]));
                }
            }

            /// <remarks/>
            public bool useKeyedUpdate
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[11]));
                }
            }

            /// <remarks/>
            public bool transactionUse
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[12]));
                }
            }

            /// <remarks/>
            public string preferredLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[13]));
                }
            }

            /// <remarks/>
            public string applicationLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[14]));
                }
            }

            /// <remarks/>
            public string providerName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[15]));
                }
            }

            /// <remarks/>
            public string providerDescription
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[16]));
                }
            }

            /// <remarks/>
            public bool useConstParameter
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[17]));
                }
            }

            /// <remarks/>
            public bool stripTrailingSpaces
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[18]));
                }
            }

            /// <remarks/>
            public string providerCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[19]));
                }
            }

            /// <remarks/>
            public string nonProviderCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[20]));
                }
            }

            /// <remarks/>
            public string dbUser
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[21]));
                }
            }

            /// <remarks/>
            public string activationDB
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[22]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void Login2CompletedEventHandler(object sender, Login2CompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class Login2CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal Login2CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }

            /// <remarks/>
            public string userName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public string companyName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[2]));
                }
            }

            /// <remarks/>
            public bool admin
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[3]));
                }
            }

            /// <remarks/>
            public string authenticationToken
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[4]));
                }
            }

            /// <remarks/>
            public int companyId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[5]));
                }
            }

            /// <remarks/>
            public string dbName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[6]));
                }
            }

            /// <remarks/>
            public string dbServer
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[7]));
                }
            }

            /// <remarks/>
            public int providerId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[8]));
                }
            }

            /// <remarks/>
            public bool security
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[9]));
                }
            }

            /// <remarks/>
            public bool auditing
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[10]));
                }
            }

            /// <remarks/>
            public bool useKeyedUpdate
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[11]));
                }
            }

            /// <remarks/>
            public bool transactionUse
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[12]));
                }
            }

            /// <remarks/>
            public string preferredLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[13]));
                }
            }

            /// <remarks/>
            public string applicationLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[14]));
                }
            }

            /// <remarks/>
            public string providerName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[15]));
                }
            }

            /// <remarks/>
            public string providerDescription
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[16]));
                }
            }

            /// <remarks/>
            public bool useConstParameter
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[17]));
                }
            }

            /// <remarks/>
            public bool stripTrailingSpaces
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[18]));
                }
            }

            /// <remarks/>
            public string providerCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[19]));
                }
            }

            /// <remarks/>
            public string nonProviderCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[20]));
                }
            }

            /// <remarks/>
            public string dbUser
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[21]));
                }
            }

            /// <remarks/>
            public string activationDB
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[22]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetLoginInformationCompletedEventHandler(object sender, GetLoginInformationCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetLoginInformationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetLoginInformationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public string userName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public int loginId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[2]));
                }
            }

            /// <remarks/>
            public string companyName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[3]));
                }
            }

            /// <remarks/>
            public int companyId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[4]));
                }
            }

            /// <remarks/>
            public bool admin
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[5]));
                }
            }

            /// <remarks/>
            public string dbName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[6]));
                }
            }

            /// <remarks/>
            public string dbServer
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[7]));
                }
            }

            /// <remarks/>
            public int providerId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[8]));
                }
            }

            /// <remarks/>
            public bool security
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[9]));
                }
            }

            /// <remarks/>
            public bool auditing
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[10]));
                }
            }

            /// <remarks/>
            public bool useKeyedUpdate
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[11]));
                }
            }

            /// <remarks/>
            public bool transactionUse
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[12]));
                }
            }

            /// <remarks/>
            public bool useUnicode
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[13]));
                }
            }

            /// <remarks/>
            public string preferredLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[14]));
                }
            }

            /// <remarks/>
            public string applicationLanguage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[15]));
                }
            }

            /// <remarks/>
            public string providerName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[16]));
                }
            }

            /// <remarks/>
            public string providerDescription
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[17]));
                }
            }

            /// <remarks/>
            public bool useConstParameter
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[18]));
                }
            }

            /// <remarks/>
            public bool stripTrailingSpaces
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[19]));
                }
            }

            /// <remarks/>
            public string providerCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[20]));
                }
            }

            /// <remarks/>
            public string nonProviderCompanyConnectionString
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[21]));
                }
            }

            /// <remarks/>
            public string dbUser
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[22]));
                }
            }

            /// <remarks/>
            public string processName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[23]));
                }
            }

            /// <remarks/>
            public string userDescription
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[24]));
                }
            }

            /// <remarks/>
            public string email
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[25]));
                }
            }

            /// <remarks/>
            public bool easyBuilderDeveloper
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[26]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void LogOffCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserNameCompletedEventHandler(object sender, GetUserNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserDescriptionByIdCompletedEventHandler(object sender, GetUserDescriptionByIdCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserDescriptionByIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserDescriptionByIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserDescriptionByNameCompletedEventHandler(object sender, GetUserDescriptionByNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserDescriptionByNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserDescriptionByNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserEMailByNameCompletedEventHandler(object sender, GetUserEMailByNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserEMailByNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserEMailByNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsFloatingUserCompletedEventHandler(object sender, IsFloatingUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsFloatingUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsFloatingUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public bool floating
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[1]));
                }
            }
        }
    }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsWebUserCompletedEventHandler(object sender, IsWebUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsWebUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsWebUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public bool web
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDbOwnerCompletedEventHandler(object sender, GetDbOwnerCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDbOwnerCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDbOwnerCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsCompanySecuredCompletedEventHandler(object sender, IsCompanySecuredCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsCompanySecuredCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsCompanySecuredCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetAuthenticationInformationsCompletedEventHandler(object sender, GetAuthenticationInformationsCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetAuthenticationInformationsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetAuthenticationInformationsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public int loginId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[1]));
                }
            }

            /// <remarks/>
            public int companyId
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[2]));
                }
            }

            /// <remarks/>
            public bool webLogin
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[3]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetAuthenticationNamesCompletedEventHandler(object sender, GetAuthenticationNamesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetAuthenticationNamesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetAuthenticationNamesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public string userName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public string companyName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[2]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DeleteAssociationCompletedEventHandler(object sender, DeleteAssociationCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class DeleteAssociationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal DeleteAssociationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DeleteUserCompletedEventHandler(object sender, DeleteUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class DeleteUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal DeleteUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DeleteCompanyCompletedEventHandler(object sender, DeleteCompanyCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class DeleteCompanyCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal DeleteCompanyCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetSystemDBConnectionStringCompletedEventHandler(object sender, GetSystemDBConnectionStringCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetSystemDBConnectionStringCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetSystemDBConnectionStringCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDMSConnectionStringCompletedEventHandler(object sender, GetDMSConnectionStringCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDMSConnectionStringCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDMSConnectionStringCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDMSDatabasesInfoCompletedEventHandler(object sender, GetDMSDatabasesInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDMSDatabasesInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDMSDatabasesInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
        public delegate void GetDataSynchroDatabasesInfoCompletedEventHandler(object sender, GetDataSynchroDatabasesInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDataSynchroDatabasesInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDataSynchroDatabasesInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCompanyDatabasesInfoCompletedEventHandler(object sender, GetCompanyDatabasesInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCompanyDatabasesInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCompanyDatabasesInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetEditionCompletedEventHandler(object sender, GetEditionCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetEditionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetEditionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetConfigurationStreamCompletedEventHandler(object sender, GetConfigurationStreamCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetConfigurationStreamCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetConfigurationStreamCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public byte[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((byte[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCountryCompletedEventHandler(object sender, GetCountryCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCountryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCountryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetProviderNameFromCompanyIdCompletedEventHandler(object sender, GetProviderNameFromCompanyIdCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetProviderNameFromCompanyIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetProviderNameFromCompanyIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetInstallationVersionCompletedEventHandler(object sender, GetInstallationVersionCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetInstallationVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetInstallationVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }

            /// <remarks/>
            public string productName
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public System.DateTime buildDate
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((System.DateTime)(this.results[2]));
                }
            }

            /// <remarks/>
            public System.DateTime instDate
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((System.DateTime)(this.results[3]));
                }
            }

            /// <remarks/>
            public int build
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[4]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserInfoCompletedEventHandler(object sender, GetUserInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetUserInfoIDCompletedEventHandler(object sender, GetUserInfoIDCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetUserInfoIDCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetUserInfoIDCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void TraceActionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void HasUserAlreadyChangedPasswordTodayCompletedEventHandler(object sender, HasUserAlreadyChangedPasswordTodayCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class HasUserAlreadyChangedPasswordTodayCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal HasUserAlreadyChangedPasswordTodayCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetBrandedApplicationTitleCompletedEventHandler(object sender, GetBrandedApplicationTitleCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetBrandedApplicationTitleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetBrandedApplicationTitleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetMasterProductBrandedNameCompletedEventHandler(object sender, GetMasterProductBrandedNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetMasterProductBrandedNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetMasterProductBrandedNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetMasterSolutionBrandedNameCompletedEventHandler(object sender, GetMasterSolutionBrandedNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetMasterSolutionBrandedNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetMasterSolutionBrandedNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetBrandedProducerNameCompletedEventHandler(object sender, GetBrandedProducerNameCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetBrandedProducerNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetBrandedProducerNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetBrandedProductTitleCompletedEventHandler(object sender, GetBrandedProductTitleCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetBrandedProductTitleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetBrandedProductTitleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetBrandedKeyCompletedEventHandler(object sender, GetBrandedKeyCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetBrandedKeyCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetBrandedKeyCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDBNetworkTypeCompletedEventHandler(object sender, GetDBNetworkTypeCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDBNetworkTypeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDBNetworkTypeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDatabaseTypeCompletedEventHandler(object sender, GetDatabaseTypeCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDatabaseTypeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDatabaseTypeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void CanUseNamespaceCompletedEventHandler(object sender, CanUseNamespaceCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class CanUseNamespaceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal CanUseNamespaceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void CacheCounterCompletedEventHandler(object sender, CacheCounterCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class CacheCounterCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal CacheCounterCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void CacheCounterGTGCompletedEventHandler(object sender, CacheCounterGTGCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class CacheCounterGTGCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal CacheCounterGTGCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SetCurrentComponentsCompletedEventHandler(object sender, SetCurrentComponentsCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SetCurrentComponentsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SetCurrentComponentsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object)(this.results[0]));
                }
            }

            /// <remarks/>
            public int dte
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsVirginActivationCompletedEventHandler(object sender, IsVirginActivationCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsVirginActivationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsVirginActivationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void HDCompletedEventHandler(object sender, HDCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class HDCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal HDCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void StoreMLUChoiceCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SaveLicensedCompletedEventHandler(object sender, SaveLicensedCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SaveLicensedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SaveLicensedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SaveUserInfoCompletedEventHandler(object sender, SaveUserInfoCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SaveUserInfoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SaveUserInfoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DeleteUserInfoCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DeleteLicensedCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void PrePingCompletedEventHandler(object sender, PrePingCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class PrePingCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal PrePingCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void PingCompletedEventHandler(object sender, PingCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class PingCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal PingCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetArticlesWithNamedCalCompletedEventHandler(object sender, GetArticlesWithNamedCalCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetArticlesWithNamedCalCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetArticlesWithNamedCalCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public ModuleNameInfo[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((ModuleNameInfo[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void RefreshSecurityStatusCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetProxySupportVersionCompletedEventHandler(object sender, GetProxySupportVersionCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetProxySupportVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetProxySupportVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetProxySettingsCompletedEventHandler(object sender, GetProxySettingsCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetProxySettingsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetProxySettingsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SetProxySettingsCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCompanyLanguageCompletedEventHandler(object sender, GetCompanyLanguageCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCompanyLanguageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCompanyLanguageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public string cultureUI
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }

            /// <remarks/>
            public string culture
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[2]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsValidTokenCompletedEventHandler(object sender, IsValidTokenCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsValidTokenCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsValidTokenCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void ReloadUserArticleBindingsCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SbrillCompletedEventHandler(object sender, SbrillCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SbrillCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SbrillCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetCalTypeCompletedEventHandler(object sender, GetCalTypeCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetCalTypeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetCalTypeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsUserLoggedCompletedEventHandler(object sender, IsUserLoggedCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsUserLoggedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsUserLoggedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsSecurityLightEnabledCompletedEventHandler(object sender, IsSecurityLightEnabledCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsSecurityLightEnabledCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsSecurityLightEnabledCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsSecurityLightAccessAllowedCompletedEventHandler(object sender, IsSecurityLightAccessAllowedCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsSecurityLightAccessAllowedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsSecurityLightAccessAllowedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetDBCultureLCIDCompletedEventHandler(object sender, GetDBCultureLCIDCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetDBCultureLCIDCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetDBCultureLCIDCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public int Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((int)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SetMessageReadCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetMessagesQueueCompletedEventHandler(object sender, GetMessagesQueueCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetMessagesQueueCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetMessagesQueueCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetOldMessagesCompletedEventHandler(object sender, GetOldMessagesCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetOldMessagesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetOldMessagesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public Object[] Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((Object[])(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SendAccessMailCompletedEventHandler(object sender, SendAccessMailCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SendAccessMailCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SendAccessMailCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetAspNetUserCompletedEventHandler(object sender, GetAspNetUserCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetAspNetUserCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetAspNetUserCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetConfigurationHashCompletedEventHandler(object sender, GetConfigurationHashCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetConfigurationHashCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetConfigurationHashCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void UserCanAccessWebSitePrivateAreaCompletedEventHandler(object sender, UserCanAccessWebSitePrivateAreaCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class UserCanAccessWebSitePrivateAreaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal UserCanAccessWebSitePrivateAreaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void IsEasyBuilderDeveloperCompletedEventHandler(object sender, IsEasyBuilderDeveloperCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class IsEasyBuilderDeveloperCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal IsEasyBuilderDeveloperCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SendErrorFileCompletedEventHandler(object sender, SendErrorFileCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class SendErrorFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal SendErrorFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public string ErrorMessage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void DownloadPdbCompletedEventHandler(object sender, DownloadPdbCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class DownloadPdbCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal DownloadPdbCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public bool Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((bool)(this.results[0]));
                }
            }

            /// <remarks/>
            public string ErrorMessage
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[1]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetMainSerialNumberCompletedEventHandler(object sender, GetMainSerialNumberCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetMainSerialNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetMainSerialNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void GetMLUExpiryDateCompletedEventHandler(object sender, GetMLUExpiryDateCompletedEventArgs e);

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        public partial class GetMLUExpiryDateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
        {

            private object[] results;

            internal GetMLUExpiryDateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                    base(exception, cancelled, userState)
            {
                this.results = results;
            }

            /// <remarks/>
            public string Result
            {
                get
                {
                    this.RaiseExceptionIfNecessary();
                    return ((string)(this.results[0]));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
        public delegate void SendBalloonCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        public class WebServicesWrapperStrings
        {

            private static global::System.Resources.ResourceManager resourceMan;

            private static global::System.Globalization.CultureInfo resourceCulture;

            [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal WebServicesWrapperStrings()
            {
            }

            /// <summary>
            ///   Returns the cached ResourceManager instance used by this class.
            /// </summary>
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
            public static global::System.Resources.ResourceManager ResourceManager
            {
                get
                {
                    if (object.ReferenceEquals(resourceMan, null))
                    {
                        global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.TaskBuilderNet.Woorm.WebServicesWrapper.WebServicesWrapperStrings", typeof(WebServicesWrapperStrings).Assembly);
                        resourceMan = temp;
                    }
                    return resourceMan;
                }
            }

            /// <summary>
            ///   Overrides the current thread's CurrentUICulture property for all
            ///   resource lookups using this strongly typed resource class.
            /// </summary>
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
            public static global::System.Globalization.CultureInfo Culture
            {
                get
                {
                    return resourceCulture;
                }
                set
                {
                    resourceCulture = value;
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The activation expression contains invalid characters..
            /// </summary>
            public static string ActivationExpressionContainsInvalidCharErrMsg
            {
                get
                {
                    return ResourceManager.GetString("ActivationExpressionContainsInvalidCharErrMsg", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Probably parenthesis mismatch in activation expression..
            /// </summary>
            public static string ActivationExpressionParenthesisMismatchErrMsg
            {
                get
                {
                    return ResourceManager.GetString("ActivationExpressionParenthesisMismatchErrMsg", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Syntax error in activation expression (Error in &apos;{0}&apos;)..
            /// </summary>
            public static string ActivationExpressionSyntaxErrMsg
            {
                get
                {
                    return ResourceManager.GetString("ActivationExpressionSyntaxErrMsg", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product is running in Demo mode with limited functionalities. It has to be registered within {0}, or the program will stop working and you will have to register it in order to continue using it..
            /// </summary>
            public static string ActivationStateDemo
            {
                get
                {
                    return ResourceManager.GetString("ActivationStateDemo", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product is running in Demo mode..
            /// </summary>
            public static string ActivationStateDemoNew
            {
                get
                {
                    return ResourceManager.GetString("ActivationStateDemoNew", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product has been disabled. Please contact the producer..
            /// </summary>
            public static string ActivationStateDisabled
            {
                get
                {
                    return ResourceManager.GetString("ActivationStateDisabled", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product is not activated and has to be registered. After registration you will be able to continue using it..
            /// </summary>
            public static string ActivationStateNoActivated
            {
                get
                {
                    return ResourceManager.GetString("ActivationStateNoActivated", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product has to be registered within {0} or it will stop working. After registration you will be able to continue using it..
            /// </summary>
            public static string ActivationStateWarning
            {
                get
                {
                    return ResourceManager.GetString("ActivationStateWarning", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user is already connected to the system using a company different from the specified one..
            /// </summary>
            public static string AlreadyLoggedOnDifferentCompanyError
            {
                get
                {
                    return ResourceManager.GetString("AlreadyLoggedOnDifferentCompanyError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Integrated security users can not use this functionality.
            /// </summary>
            public static string AuthenticationTypeError
            {
                get
                {
                    return ResourceManager.GetString("AuthenticationTypeError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The beginning of the execution of the command in silente mode is failed..
            /// </summary>
            public static string BeginRunBatchInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("BeginRunBatchInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The begin of function call {0} has failed.
            /// </summary>
            public static string BeginRunFunctionFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("BeginRunFunctionFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The beginning of the execution of the report in silente mode is failed..
            /// </summary>
            public static string BeginRunReportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("BeginRunReportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The beginning of the execution of export document in silente mode is failed..
            /// </summary>
            public static string BeginRunXmlExportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("BeginRunXmlExportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The beginning of the execution of import document in silente mode is failed..
            /// </summary>
            public static string BeginRunXmlImportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("BeginRunXmlImportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error in CAL management. Impossible to assign a CAL..
            /// </summary>
            public static string CalManagementError
            {
                get
                {
                    return ResourceManager.GetString("CalManagementError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Cannot obtain diagnostic from TBServices.
            /// </summary>
            public static string CannotObtainDiagnostic
            {
                get
                {
                    return ResourceManager.GetString("CannotObtainDiagnostic", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to You have to change your password.
            /// </summary>
            public static string ChangePasswordNeeded
            {
                get
                {
                    return ResourceManager.GetString("ChangePasswordNeeded", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Activation check failed (syntax error in expression &quot;{0}&quot;)..
            /// </summary>
            public static string CheckActivationExpressionErrFmtMsg
            {
                get
                {
                    return ResourceManager.GetString("CheckActivationExpressionErrFmtMsg", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Document close failed.
            /// </summary>
            public static string CloseDocumentFailed
            {
                get
                {
                    return ResourceManager.GetString("CloseDocumentFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Failed to close active login.
            /// </summary>
            public static string CloseLoginFailed
            {
                get
                {
                    return ResourceManager.GetString("CloseLoginFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TaskBuilder close failed..
            /// </summary>
            public static string CloseTBFailed
            {
                get
                {
                    return ResourceManager.GetString("CloseTBFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to A communication error occurred..
            /// </summary>
            public static string CommunicationErrorExceptionMessage
            {
                get
                {
                    return ResourceManager.GetString("CommunicationErrorExceptionMessage", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The company database does not exists or it is not possible connecting to it. It must be created with the Administration Console..
            /// </summary>
            public static string CompanyDatabaseNotPresent
            {
                get
                {
                    return ResourceManager.GetString("CompanyDatabaseNotPresent", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The company database table does not exists. They must be created with the Administration Console..
            /// </summary>
            public static string CompanyDatabaseTablesNotPresent
            {
                get
                {
                    return ResourceManager.GetString("CompanyDatabaseTablesNotPresent", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TaskBuilder connection failed.
            /// </summary>
            public static string ConnectionToTbFailed
            {
                get
                {
                    return ResourceManager.GetString("ConnectionToTbFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to {0} day(s).
            /// </summary>
            public static string DayToExpire
            {
                get
                {
                    return ResourceManager.GetString("DayToExpire", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The size of the database you are using exceed the maximum allowed size (2GB). To continue you have to upgrade your server to the newest version SQLServer..
            /// </summary>
            public static string DBSizeError
            {
                get
                {
                    return ResourceManager.GetString("DBSizeError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Description.
            /// </summary>
            public static string Description
            {
                get
                {
                    return ResourceManager.GetString("Description", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Login failed for EasyLookSystem user..
            /// </summary>
            public static string EasyLookSysLoginFailed
            {
                get
                {
                    return ResourceManager.GetString("EasyLookSysLoginFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The ending of the execution of the command in silente mode is failed..
            /// </summary>
            public static string EndRunBatchInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("EndRunBatchInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The end of the function call has failed..
            /// </summary>
            public static string EndRunFunctionFailed
            {
                get
                {
                    return ResourceManager.GetString("EndRunFunctionFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The ending of the execution of the report in silente mode is failed..
            /// </summary>
            public static string EndRunReportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("EndRunReportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The ending of the execution of export document in silente mode is failed..
            /// </summary>
            public static string EndRunXmlExportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("EndRunXmlExportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The ending of the execution of import document in silente mode is failed..
            /// </summary>
            public static string EndRunXmlImportInUnattendedModeFailed
            {
                get
                {
                    return ResourceManager.GetString("EndRunXmlImportInUnattendedModeFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Login Manager has not ben able to initialize Activation Manager..
            /// </summary>
            public static string ErrActivationManager
            {
                get
                {
                    return ResourceManager.GetString("ErrActivationManager", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Password change failed..
            /// </summary>
            public static string ErrChangePassword
            {
                get
                {
                    return ResourceManager.GetString("ErrChangePassword", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Please select both user and company..
            /// </summary>
            public static string ErrChooseUserAndCompany
            {
                get
                {
                    return ResourceManager.GetString("ErrChooseUserAndCompany", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: impossible to read from system database the connection parameters for the company.
            /// </summary>
            public static string ErrConnectionParams
            {
                get
                {
                    return ResourceManager.GetString("ErrConnectionParams", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to File {0} does not exist..
            /// </summary>
            public static string ErrFileNotExists
            {
                get
                {
                    return ResourceManager.GetString("ErrFileNotExists", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: selected company not exist in system database.
            /// </summary>
            public static string ErrInvalidCompany
            {
                get
                {
                    return ResourceManager.GetString("ErrInvalidCompany", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: the process not allows a simultaneous login.
            /// </summary>
            public static string ErrInvalidProcess
            {
                get
                {
                    return ResourceManager.GetString("ErrInvalidProcess", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: user not exist in system database, please make sure you entered the password correctly..
            /// </summary>
            public static string ErrInvalidUser
            {
                get
                {
                    return ResourceManager.GetString("ErrInvalidUser", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: company database is in use by another user..
            /// </summary>
            public static string ErrLockedDatabase
            {
                get
                {
                    return ResourceManager.GetString("ErrLockedDatabase", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Login failed. Please make sure you entered the password correctly..
            /// </summary>
            public static string ErrLoginFailed
            {
                get
                {
                    return ResourceManager.GetString("ErrLoginFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: no article exists with the specified feature.
            /// </summary>
            public static string ErrNoArticleFunctionality
            {
                get
                {
                    return ResourceManager.GetString("ErrNoArticleFunctionality", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error invoking remote function &apos;{0}&apos;: &apos;{1}&apos;.
            /// </summary>
            public static string ErrorInvokingRemoteFunction
            {
                get
                {
                    return ResourceManager.GetString("ErrorInvokingRemoteFunction", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: authentication token is empty.
            /// </summary>
            public static string ErrProcessNotAuthenticated
            {
                get
                {
                    return ResourceManager.GetString("ErrProcessNotAuthenticated", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: system database not contains information about the provider specified for the company.
            /// </summary>
            public static string ErrProviderInfo
            {
                get
                {
                    return ResourceManager.GetString("ErrProviderInfo", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The new password does not match the confirmed password..
            /// </summary>
            public static string ErrPwdDifferent
            {
                get
                {
                    return ResourceManager.GetString("ErrPwdDifferent", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The password must be at least of {0} characters. Check if you are using the strong password management..
            /// </summary>
            public static string ErrPwdLength
            {
                get
                {
                    return ResourceManager.GetString("ErrPwdLength", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The connection string for system database specified in ServerConnection.config is empty, so you must use Administration Console..
            /// </summary>
            public static string ErrSysDBConnectionString
            {
                get
                {
                    return ResourceManager.GetString("ErrSysDBConnectionString", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user is already connected to the system. Do you want to close previous working session and open new one?.
            /// </summary>
            public static string ErrUserAlreadyLogged
            {
                get
                {
                    return ResourceManager.GetString("ErrUserAlreadyLogged", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user is already connected to the system..
            /// </summary>
            public static string ErrUserAlreadyLoggedNoForce
            {
                get
                {
                    return ResourceManager.GetString("ErrUserAlreadyLoggedNoForce", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: impossible to associate the user to the licence.
            /// </summary>
            public static string ErrUserAssignmentToArticle
            {
                get
                {
                    return ResourceManager.GetString("ErrUserAssignmentToArticle", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user has to change the password, but his configuration does not allow it. Please contact the security manager..
            /// </summary>
            public static string ErrUserCannotChangePwdButMust
            {
                get
                {
                    return ResourceManager.GetString("ErrUserCannotChangePwdButMust", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user is already connected to the system with another application..
            /// </summary>
            public static string ErrWebUserAlreadyLogged
            {
                get
                {
                    return ResourceManager.GetString("ErrWebUserAlreadyLogged", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Function.
            /// </summary>
            public static string Function
            {
                get
                {
                    return ResourceManager.GetString("Function", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to User can not access to this application.
            /// </summary>
            public static string GDIApplicationAccessDenied
            {
                get
                {
                    return ResourceManager.GetString("GDIApplicationAccessDenied", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to A generic error occurred..
            /// </summary>
            public static string GenericErrorExceptionMessage
            {
                get
                {
                    return ResourceManager.GetString("GenericErrorExceptionMessage", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to An unexpected error occurred. Error code is {0}..
            /// </summary>
            public static string GenericExceptionError
            {
                get
                {
                    return ResourceManager.GetString("GenericExceptionError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The application date cannot be obtained..
            /// </summary>
            public static string GetApplicationDateFailed
            {
                get
                {
                    return ResourceManager.GetString("GetApplicationDateFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TaskBuilder can&apos;t supply his connection data..
            /// </summary>
            public static string GetCurrentUserFailed
            {
                get
                {
                    return ResourceManager.GetString("GetCurrentUserFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The parameters assignment of the command {0} has failed.
            /// </summary>
            public static string GetDocumentParametersFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("GetDocumentParametersFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error getting extra filters for row security layer.
            /// </summary>
            public static string GetGetExtraFilteringFailed
            {
                get
                {
                    return ResourceManager.GetString("GetGetExtraFilteringFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The HotLink interrogation cannot be deduced..
            /// </summary>
            public static string GetHotlinkQueryFailed
            {
                get
                {
                    return ResourceManager.GetString("GetHotlinkQueryFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The application global messages cannot be found..
            /// </summary>
            public static string GetMessagesFailed
            {
                get
                {
                    return ResourceManager.GetString("GetMessagesFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Request of number of open documents in application failed..
            /// </summary>
            public static string GetNrOpenDocumentsFailed
            {
                get
                {
                    return ResourceManager.GetString("GetNrOpenDocumentsFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error, it is impossible to obtain the TbLoader process id.
            /// </summary>
            public static string GetProcessIDFailed
            {
                get
                {
                    return ResourceManager.GetString("GetProcessIDFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Demand for valorization of the parameters of report {0} failed..
            /// </summary>
            public static string GetReportParametersFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("GetReportParametersFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to GetWindowInfosFromPoint failed.
            /// </summary>
            public static string GetWindowInfosFromPointFailed
            {
                get
                {
                    return ResourceManager.GetString("GetWindowInfosFromPointFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Demand for valorization of the execution parameters of document {0} failed..
            /// </summary>
            public static string GetXMLExportParametersFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("GetXMLExportParametersFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Company not corresponding to the company previously specified in configuration wizard..
            /// </summary>
            public static string ImagoCompanyNotCorresponding
            {
                get
                {
                    return ResourceManager.GetString("ImagoCompanyNotCorresponding", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This user is already associated to another Infinity user..
            /// </summary>
            public static string ImagoUserAlreadyAssociated
            {
                get
                {
                    return ResourceManager.GetString("ImagoUserAlreadyAssociated", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TbLoader initialization failed..
            /// </summary>
            public static string InitTbLoginFailed
            {
                get
                {
                    return ResourceManager.GetString("InitTbLoginFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The version of company database is not compatible with program in use. Please check its correctness with Administration Console (Check and upgrade Company database)..
            /// </summary>
            public static string InvalidDatabaseError
            {
                get
                {
                    return ResourceManager.GetString("InvalidDatabaseError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The company database is not supported by application configuration..
            /// </summary>
            public static string InvalidDatabaseForActivation
            {
                get
                {
                    return ResourceManager.GetString("InvalidDatabaseForActivation", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The database you are connecting to is not valid for current product release; please upgrade it using the Administration Console.
            /// </summary>
            public static string InvalidModuleRelease
            {
                get
                {
                    return ResourceManager.GetString("InvalidModuleRelease", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Invalid service URL: &apos;{0}&apos;.
            /// </summary>
            public static string InvalidServiceUrl
            {
                get
                {
                    return ResourceManager.GetString("InvalidServiceUrl", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Infinity token contains invalid data..
            /// </summary>
            public static string InvalidSSOToken
            {
                get
                {
                    return ResourceManager.GetString("InvalidSSOToken", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to An error occurred invoking &apos;{0}&apos; web method..
            /// </summary>
            public static string InvokeWebMethodFailed
            {
                get
                {
                    return ResourceManager.GetString("InvokeWebMethodFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Library.
            /// </summary>
            public static string Library
            {
                get
                {
                    return ResourceManager.GetString("Library", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The user is locked, you need to unlock it with Administration Console..
            /// </summary>
            public static string LoginLocked
            {
                get
                {
                    return ResourceManager.GetString("LoginLocked", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to You must manually start TBLoader.exe, or modify the &apos;singletonTBLoader&apos; attribute in the TBServices web configuration file setting it to &apos;false&apos;.
            /// </summary>
            public static string ManualStartTBLoader
            {
                get
                {
                    return ResourceManager.GetString("ManualStartTBLoader", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Impossible to retrieve connection string to connect to the system database..
            /// </summary>
            public static string MissingConnectionString
            {
                get
                {
                    return ResourceManager.GetString("MissingConnectionString", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The same SSOID  Infinity is associated to more than one user..
            /// </summary>
            public static string MoreThanOneSSOToken
            {
                get
                {
                    return ResourceManager.GetString("MoreThanOneSSOToken", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The selected company is not available because it violate the Standard Edition boundaries..
            /// </summary>
            public static string NoAdmittedCompany
            {
                get
                {
                    return ResourceManager.GetString("NoAdmittedCompany", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Available user licences are sold out.
            /// </summary>
            public static string NoCalAvailableError
            {
                get
                {
                    return ResourceManager.GetString("NoCalAvailableError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to MagicDocuments module is not usable, it lacks CAL..
            /// </summary>
            public static string NoOfficeLicenseError
            {
                get
                {
                    return ResourceManager.GetString("NoOfficeLicenseError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to No TCP port available in range [{0}-{1}].
            /// </summary>
            public static string NoPortAvailable
            {
                get
                {
                    return ResourceManager.GetString("NoPortAvailable", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to A user is not logged in..
            /// </summary>
            public static string NotLoggedExceptionMessage
            {
                get
                {
                    return ResourceManager.GetString("NotLoggedExceptionMessage", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to No valid TBLoader process listening at port {0}!.
            /// </summary>
            public static string NoValidTbLoaderListening
            {
                get
                {
                    return ResourceManager.GetString("NoValidTbLoaderListening", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to EasyLook article is not activated, is not usabled.
            /// </summary>
            public static string NoWebLicenseError
            {
                get
                {
                    return ResourceManager.GetString("NoWebLicenseError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TaskBuilder authentication token change failed..
            /// </summary>
            public static string OverwriteAuthenticationFailed
            {
                get
                {
                    return ResourceManager.GetString("OverwriteAuthenticationFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Unable to change password many times during the same day..
            /// </summary>
            public static string PasswordAlreadyChangedToday
            {
                get
                {
                    return ResourceManager.GetString("PasswordAlreadyChangedToday", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to PathFinder initialize failed..
            /// </summary>
            public static string PathFinderInitializationFailed
            {
                get
                {
                    return ResourceManager.GetString("PathFinderInitializationFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to This product is properly registered..
            /// </summary>
            public static string ProductIsRegistered
            {
                get
                {
                    return ResourceManager.GetString("ProductIsRegistered", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The execution of the command {0} in silente mode is failed..
            /// </summary>
            public static string RunBatchInUnattendedModeFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunBatchInUnattendedModeFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Before using the application you first have to execute Administration Console and create a company..
            /// </summary>
            public static string RunConsoleFirst
            {
                get
                {
                    return ResourceManager.GetString("RunConsoleFirst", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Failed running document {0}..
            /// </summary>
            public static string RunDocumentFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunDocumentFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Failed to invoke function &apos;{0}&apos;.
            /// </summary>
            public static string RunFailed
            {
                get
                {
                    return ResourceManager.GetString("RunFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The run function {0} failed..
            /// </summary>
            public static string RunFunctionFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunFunctionFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to RunIconizedDocument failed.
            /// </summary>
            public static string RunIconizedDocumentFailed
            {
                get
                {
                    return ResourceManager.GetString("RunIconizedDocumentFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The run report {0} failed..
            /// </summary>
            public static string RunReportFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunReportFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The run report {0} in silent mode is failed..
            /// </summary>
            public static string RunReportInUnattendedModeFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunReportInUnattendedModeFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The execution of the export of the document {0} in silente mode is failed..
            /// </summary>
            public static string RunXmlExportInUnattendedModeFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunXmlExportInUnattendedModeFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The execution of the import of the document {0} in silente mode is failed..
            /// </summary>
            public static string RunXmlImportInUnattendedModeFailedFmt
            {
                get
                {
                    return ResourceManager.GetString("RunXmlImportInUnattendedModeFailedFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error: the communication channel with Login Manager is down, please verify the Asp.Net process (aspnet_wp or w3wp) and IIS are running properly. {0}.
            /// </summary>
            public static string ServerDown
            {
                get
                {
                    return ResourceManager.GetString("ServerDown", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Login Manager has failed the connection to system database..
            /// </summary>
            public static string ServerNotConnectedToSystemDB
            {
                get
                {
                    return ResourceManager.GetString("ServerNotConnectedToSystemDB", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Request of change of application date failed.
            /// </summary>
            public static string SetApplicationDateFailed
            {
                get
                {
                    return ResourceManager.GetString("SetApplicationDateFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error, it is impossible to communicate the menu identifier to TaskBuilder application.
            /// </summary>
            public static string SetMenuHandleFailed
            {
                get
                {
                    return ResourceManager.GetString("SetMenuHandleFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to An unexpected error occurred. Error code is {0}. This might be caused by a failed product update. Please try reinstalling the product..
            /// </summary>
            public static string SoapExceptionError
            {
                get
                {
                    return ResourceManager.GetString("SoapExceptionError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Source.
            /// </summary>
            public static string Source
            {
                get
                {
                    return ResourceManager.GetString("Source", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Unable to connect to selected company, because your serial numbers do not allow you to connect to a company database running on SQL Server 2012 or later versions.
            ///  .
            /// </summary>
            public static string Sql2012NotAllowedForCompany
            {
                get
                {
                    return ResourceManager.GetString("Sql2012NotAllowedForCompany", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Unable to connect to selected company, because your serial numbers do not allow you to connect to a DMS database running on SQL Server 2012 or later versions.
            ///	.
            /// </summary>
            public static string Sql2012NotAllowedForDMS
            {
                get
                {
                    return ResourceManager.GetString("Sql2012NotAllowedForDMS", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to An Infinity functionality was requested but the currently logged user is not the same connected with Infinity and this SSOID is not associated to any user. Please close this instance and retry..
            /// </summary>
            public static string SSOIDNotAssociated
            {
                get
                {
                    return ResourceManager.GetString("SSOIDNotAssociated", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Infinity token is empty..
            /// </summary>
            public static string SsoTokenEmpty
            {
                get
                {
                    return ResourceManager.GetString("SsoTokenEmpty", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Exception during login from Infinity..
            /// </summary>
            public static string SsoTokenError
            {
                get
                {
                    return ResourceManager.GetString("SsoTokenError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error while starting a new tbLoader.
            /// </summary>
            public static string StartTbLoaderFailed
            {
                get
                {
                    return ResourceManager.GetString("StartTbLoaderFailed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Task Builder Framework libraries.
            /// </summary>
            public static string TBComponents
            {
                get
                {
                    return ResourceManager.GetString("TBComponents", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The file {0} is missing..
            /// </summary>
            public static string TBLoaderExeNotFoundFmt
            {
                get
                {
                    return ResourceManager.GetString("TBLoaderExeNotFoundFmt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The communication channel is unusable.
            /// </summary>
            public static string TbLoaderSoapPortInvalid
            {
                get
                {
                    return ResourceManager.GetString("TbLoaderSoapPortInvalid", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to TaskBuilder connection hasn&apos;t been executed.
            /// </summary>
            public static string TbNotConnected
            {
                get
                {
                    return ResourceManager.GetString("TbNotConnected", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Waiting for TBLoader process initialization has timed-out after {0} seconds.
            /// </summary>
            public static string TimeoutStartingTbloader
            {
                get
                {
                    return ResourceManager.GetString("TimeoutStartingTbloader", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to today.
            /// </summary>
            public static string ToDayExpire
            {
                get
                {
                    return ResourceManager.GetString("ToDayExpire", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The total of CAL associated to the users (in the system database) is greater than the CAL number..
            /// </summary>
            public static string TooManyAssignedCAL
            {
                get
                {
                    return ResourceManager.GetString("TooManyAssignedCAL", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Unable to retrieve a valid SOS ticket. Please check your SOS credentials..
            /// </summary>
            public static string UnableToRetrieveSOSTkt
            {
                get
                {
                    return ResourceManager.GetString("UnableToRetrieveSOSTkt", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to The LoginManager has not been initialized..
            /// </summary>
            public static string UnInitializedExceptionMessage
            {
                get
                {
                    return ResourceManager.GetString("UnInitializedExceptionMessage", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to User is not allowed to use the requested functionality..
            /// </summary>
            public static string UserNotAllowed
            {
                get
                {
                    return ResourceManager.GetString("UserNotAllowed", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to User connected to Easylook is not authenticated.
            /// </summary>
            public static string UserNotAuthenticated
            {
                get
                {
                    return ResourceManager.GetString("UserNotAuthenticated", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to User can not access to Web applications.
            /// </summary>
            public static string WebApplicationAccessDenied
            {
                get
                {
                    return ResourceManager.GetString("WebApplicationAccessDenied", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error code: {0}. LoginManager does not respond. Please verify that LoginManager is up and running..
            /// </summary>
            public static string WebExceptionError
            {
                get
                {
                    return ResourceManager.GetString("WebExceptionError", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to Error in program object management which contains information about run report..
            /// </summary>
            public static string WoormInfoError
            {
                get
                {
                    return ResourceManager.GetString("WoormInfoError", resourceCulture);
                }
            }
        }


        #region ModuleNameInfo
        //=========================================================================
        [Serializable]
        public class ModuleNameInfo
        {
            public string Name;
            public string LocalizedName;
            public int CAL;
            public ModuleNameInfo()
            {

            }
            public ModuleNameInfo(string name, string localizedName, int cal)
            {
                Name = name;
                LocalizedName = localizedName;
                CAL = cal;
            }
        }
        #endregion

        //=========================================================================
        [Serializable]
        public class LoginProperties : ISerializable
        {
            string ssoToken;
            string authenticationToken;

            //---------------------------------------------------------------------
            public string AuthenticationToken
            {
                get { return authenticationToken; }
                set { authenticationToken = value; }
            }

            //---------------------------------------------------------------------
            public string SsoToken
            {
                get { return ssoToken; }
                set { ssoToken = value; }
            }

            //---------------------------------------------------------------------
            public LoginProperties()
            {

            }

            //---------------------------------------------------------------------
            protected LoginProperties(SerializationInfo info, StreamingContext context)
            {
                try
                {
                    ssoToken = info.GetString("ssoToken");
                }
                catch
                {
                    ssoToken = String.Empty;
                }

                try
                {
                    authenticationToken = info.GetString("authenticationToken");
                }
                catch
                {
                    authenticationToken = String.Empty;
                }
            }

            #region ISerializable Members

            //---------------------------------------------------------------------
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ssoToken", ssoToken);
                info.AddValue("authenticationToken", authenticationToken);
            }

            #endregion
        }

        public enum GrantType
        {
            Execute = 1,
            Edit = 2,
            New = 4,
            Delete = 8,
            Browse = 16,
            CustomizeForm = 32,
            EditQuery = 64,
            Import = 128,
            Export = 256,
            SilentMode = 512
        }
}
