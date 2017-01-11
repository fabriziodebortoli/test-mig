using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microarea.RSWeb.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.RSWeb.WebServicesWrapper.LoginMg
{
    
    public partial class MicroareaLoginManager : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private System.Threading.SendOrPostCallback InitOperationCompleted;

        private System.Threading.SendOrPostCallback IsAliveOperationCompleted;

        private System.Threading.SendOrPostCallback IsCalAvailableOperationCompleted;

        private System.Threading.SendOrPostCallback PingNeededOperationCompleted;

        private System.Threading.SendOrPostCallback SetCompanyInfoOperationCompleted;

        private System.Threading.SendOrPostCallback IsActivatedOperationCompleted;

        private System.Threading.SendOrPostCallback IsSynchActivationOperationCompleted;

        private System.Threading.SendOrPostCallback GetModulesOperationCompleted;

        private System.Threading.SendOrPostCallback GetCompanyUsersOperationCompleted;

        private System.Threading.SendOrPostCallback GetNonNTCompanyUsersOperationCompleted;

        private System.Threading.SendOrPostCallback GetCompanyRolesOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserRolesOperationCompleted;

        private System.Threading.SendOrPostCallback EnumAllUsersOperationCompleted;

        private System.Threading.SendOrPostCallback EnumAllCompanyUsersOperationCompleted;

        private System.Threading.SendOrPostCallback GetRoleUsersOperationCompleted;

        private System.Threading.SendOrPostCallback EnumCompaniesOperationCompleted;

        private System.Threading.SendOrPostCallback IsIntegratedSecurityUserOperationCompleted;

        private System.Threading.SendOrPostCallback GetLoggedUsersNumberOperationCompleted;

        private System.Threading.SendOrPostCallback GetCompanyLoggedUsersNumberOperationCompleted;

        private System.Threading.SendOrPostCallback GetLoggedUsersOperationCompleted;

        private System.Threading.SendOrPostCallback GetLoggedUsersAdvancedOperationCompleted;

        private System.Threading.SendOrPostCallback GetCalNumberOperationCompleted;

        private System.Threading.SendOrPostCallback GetCalNumber2OperationCompleted;

        private System.Threading.SendOrPostCallback GetTokenProcessTypeOperationCompleted;

        private System.Threading.SendOrPostCallback ReloadConfigurationOperationCompleted;

        private System.Threading.SendOrPostCallback ValidateUserOperationCompleted;

        private System.Threading.SendOrPostCallback ChangePasswordOperationCompleted;

        private System.Threading.SendOrPostCallback LoginCompactOperationCompleted;

        private System.Threading.SendOrPostCallback SsoLoginOperationCompleted;

        private System.Threading.SendOrPostCallback SsoLoggedUserOperationCompleted;

        private System.Threading.SendOrPostCallback LoginOperationCompleted;

        private System.Threading.SendOrPostCallback Login2OperationCompleted;

        private System.Threading.SendOrPostCallback GetLoginInformationOperationCompleted;

        private System.Threading.SendOrPostCallback LogOffOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserNameOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserDescriptionByIdOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserDescriptionByNameOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserEMailByNameOperationCompleted;

        private System.Threading.SendOrPostCallback IsFloatingUserOperationCompleted;

        private System.Threading.SendOrPostCallback IsWebUserOperationCompleted;

        private System.Threading.SendOrPostCallback GetDbOwnerOperationCompleted;

        private System.Threading.SendOrPostCallback IsCompanySecuredOperationCompleted;

        private System.Threading.SendOrPostCallback GetAuthenticationInformationsOperationCompleted;

        private System.Threading.SendOrPostCallback GetAuthenticationNamesOperationCompleted;

        private System.Threading.SendOrPostCallback DeleteAssociationOperationCompleted;

        private System.Threading.SendOrPostCallback DeleteUserOperationCompleted;

        private System.Threading.SendOrPostCallback DeleteCompanyOperationCompleted;

        private System.Threading.SendOrPostCallback GetSystemDBConnectionStringOperationCompleted;

        private System.Threading.SendOrPostCallback GetDMSConnectionStringOperationCompleted;

        private System.Threading.SendOrPostCallback GetDMSDatabasesInfoOperationCompleted;

        private System.Threading.SendOrPostCallback GetDataSynchroDatabasesInfoOperationCompleted;

        private System.Threading.SendOrPostCallback GetCompanyDatabasesInfoOperationCompleted;

        private System.Threading.SendOrPostCallback GetEditionOperationCompleted;

        private System.Threading.SendOrPostCallback GetConfigurationStreamOperationCompleted;

        private System.Threading.SendOrPostCallback GetCountryOperationCompleted;

        private System.Threading.SendOrPostCallback GetProviderNameFromCompanyIdOperationCompleted;

        private System.Threading.SendOrPostCallback GetInstallationVersionOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserInfoOperationCompleted;

        private System.Threading.SendOrPostCallback GetUserInfoIDOperationCompleted;

        private System.Threading.SendOrPostCallback TraceActionOperationCompleted;

        private System.Threading.SendOrPostCallback HasUserAlreadyChangedPasswordTodayOperationCompleted;

        private System.Threading.SendOrPostCallback GetBrandedApplicationTitleOperationCompleted;

        private System.Threading.SendOrPostCallback GetMasterProductBrandedNameOperationCompleted;

        private System.Threading.SendOrPostCallback GetMasterSolutionBrandedNameOperationCompleted;

        private System.Threading.SendOrPostCallback GetBrandedProducerNameOperationCompleted;

        private System.Threading.SendOrPostCallback GetBrandedProductTitleOperationCompleted;

        private System.Threading.SendOrPostCallback GetBrandedKeyOperationCompleted;

        private System.Threading.SendOrPostCallback GetDBNetworkTypeOperationCompleted;

        private System.Threading.SendOrPostCallback GetDatabaseTypeOperationCompleted;

        private System.Threading.SendOrPostCallback CanUseNamespaceOperationCompleted;

        private System.Threading.SendOrPostCallback CacheCounterOperationCompleted;

        private System.Threading.SendOrPostCallback CacheCounterGTGOperationCompleted;

        private System.Threading.SendOrPostCallback SetCurrentComponentsOperationCompleted;

        private System.Threading.SendOrPostCallback IsVirginActivationOperationCompleted;

        private System.Threading.SendOrPostCallback HDOperationCompleted;

        private System.Threading.SendOrPostCallback StoreMLUChoiceOperationCompleted;

        private System.Threading.SendOrPostCallback SaveLicensedOperationCompleted;

        private System.Threading.SendOrPostCallback SaveUserInfoOperationCompleted;

        private System.Threading.SendOrPostCallback DeleteUserInfoOperationCompleted;

        private System.Threading.SendOrPostCallback DeleteLicensedOperationCompleted;

        private System.Threading.SendOrPostCallback PrePingOperationCompleted;

        private System.Threading.SendOrPostCallback PingOperationCompleted;

        private System.Threading.SendOrPostCallback GetArticlesWithNamedCalOperationCompleted;

        private System.Threading.SendOrPostCallback RefreshSecurityStatusOperationCompleted;

        private System.Threading.SendOrPostCallback GetProxySupportVersionOperationCompleted;

        private System.Threading.SendOrPostCallback GetProxySettingsOperationCompleted;

        private System.Threading.SendOrPostCallback SetProxySettingsOperationCompleted;

        private System.Threading.SendOrPostCallback GetCompanyLanguageOperationCompleted;

        private System.Threading.SendOrPostCallback IsValidTokenOperationCompleted;

        private System.Threading.SendOrPostCallback ReloadUserArticleBindingsOperationCompleted;

        private System.Threading.SendOrPostCallback SbrillOperationCompleted;

        private System.Threading.SendOrPostCallback GetCalTypeOperationCompleted;

        private System.Threading.SendOrPostCallback IsUserLoggedOperationCompleted;

        private System.Threading.SendOrPostCallback IsSecurityLightEnabledOperationCompleted;

        private System.Threading.SendOrPostCallback IsSecurityLightAccessAllowedOperationCompleted;

        private System.Threading.SendOrPostCallback GetDBCultureLCIDOperationCompleted;

        private System.Threading.SendOrPostCallback SetMessageReadOperationCompleted;

        private System.Threading.SendOrPostCallback GetMessagesQueueOperationCompleted;

        private System.Threading.SendOrPostCallback GetOldMessagesOperationCompleted;

        private System.Threading.SendOrPostCallback SendAccessMailOperationCompleted;

        private System.Threading.SendOrPostCallback GetAspNetUserOperationCompleted;

        private System.Threading.SendOrPostCallback GetConfigurationHashOperationCompleted;

        private System.Threading.SendOrPostCallback UserCanAccessWebSitePrivateAreaOperationCompleted;

        private System.Threading.SendOrPostCallback IsEasyBuilderDeveloperOperationCompleted;

        private System.Threading.SendOrPostCallback SendErrorFileOperationCompleted;

        private System.Threading.SendOrPostCallback DownloadPdbOperationCompleted;

        private System.Threading.SendOrPostCallback GetMainSerialNumberOperationCompleted;

        private System.Threading.SendOrPostCallback GetMLUExpiryDateOperationCompleted;

        private System.Threading.SendOrPostCallback SendBalloonOperationCompleted;

        private bool useDefaultCredentialsSetExplicitly;

        /// <remarks/>
        public MicroareaLoginManager()
        {
            //TODO RSWEB this.Url = global::Microarea.TaskBuilderNet.Core.Properties.Settings.Default.Microarea_TaskBuilderNet_Core_loginMng_MicroareaLoginManager;
            if ((this.IsLocalFileSystemWebService(this.Url) == true))
            {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true)
                            && (this.useDefaultCredentialsSetExplicitly == false))
                            && (this.IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        /// <remarks/>
        public event InitCompletedEventHandler InitCompleted;

        /// <remarks/>
        public event IsAliveCompletedEventHandler IsAliveCompleted;

        /// <remarks/>
        public event IsCalAvailableCompletedEventHandler IsCalAvailableCompleted;

        /// <remarks/>
        public event PingNeededCompletedEventHandler PingNeededCompleted;

        /// <remarks/>
        public event SetCompanyInfoCompletedEventHandler SetCompanyInfoCompleted;

        /// <remarks/>
        public event IsActivatedCompletedEventHandler IsActivatedCompleted;

        /// <remarks/>
        public event IsSynchActivationCompletedEventHandler IsSynchActivationCompleted;

        /// <remarks/>
        public event GetModulesCompletedEventHandler GetModulesCompleted;

        /// <remarks/>
        public event GetCompanyUsersCompletedEventHandler GetCompanyUsersCompleted;

        /// <remarks/>
        public event GetNonNTCompanyUsersCompletedEventHandler GetNonNTCompanyUsersCompleted;

        /// <remarks/>
        public event GetCompanyRolesCompletedEventHandler GetCompanyRolesCompleted;

        /// <remarks/>
        public event GetUserRolesCompletedEventHandler GetUserRolesCompleted;

        /// <remarks/>
        public event EnumAllUsersCompletedEventHandler EnumAllUsersCompleted;

        /// <remarks/>
        public event EnumAllCompanyUsersCompletedEventHandler EnumAllCompanyUsersCompleted;

        /// <remarks/>
        public event GetRoleUsersCompletedEventHandler GetRoleUsersCompleted;

        /// <remarks/>
        public event EnumCompaniesCompletedEventHandler EnumCompaniesCompleted;

        /// <remarks/>
        public event IsIntegratedSecurityUserCompletedEventHandler IsIntegratedSecurityUserCompleted;

        /// <remarks/>
        public event GetLoggedUsersNumberCompletedEventHandler GetLoggedUsersNumberCompleted;

        /// <remarks/>
        public event GetCompanyLoggedUsersNumberCompletedEventHandler GetCompanyLoggedUsersNumberCompleted;

        /// <remarks/>
        public event GetLoggedUsersCompletedEventHandler GetLoggedUsersCompleted;

        /// <remarks/>
        public event GetLoggedUsersAdvancedCompletedEventHandler GetLoggedUsersAdvancedCompleted;

        /// <remarks/>
        public event GetCalNumberCompletedEventHandler GetCalNumberCompleted;

        /// <remarks/>
        public event GetCalNumber2CompletedEventHandler GetCalNumber2Completed;

        /// <remarks/>
        public event GetTokenProcessTypeCompletedEventHandler GetTokenProcessTypeCompleted;

        /// <remarks/>
        public event ReloadConfigurationCompletedEventHandler ReloadConfigurationCompleted;

        /// <remarks/>
        public event ValidateUserCompletedEventHandler ValidateUserCompleted;

        /// <remarks/>
        public event ChangePasswordCompletedEventHandler ChangePasswordCompleted;

        /// <remarks/>
        public event LoginCompactCompletedEventHandler LoginCompactCompleted;

        /// <remarks/>
        public event SsoLoginCompletedEventHandler SsoLoginCompleted;

        /// <remarks/>
        public event SsoLoggedUserCompletedEventHandler SsoLoggedUserCompleted;

        /// <remarks/>
        public event LoginCompletedEventHandler LoginCompleted;

        /// <remarks/>
        public event Login2CompletedEventHandler Login2Completed;

        /// <remarks/>
        public event GetLoginInformationCompletedEventHandler GetLoginInformationCompleted;

        /// <remarks/>
        public event LogOffCompletedEventHandler LogOffCompleted;

        /// <remarks/>
        public event GetUserNameCompletedEventHandler GetUserNameCompleted;

        /// <remarks/>
        public event GetUserDescriptionByIdCompletedEventHandler GetUserDescriptionByIdCompleted;

        /// <remarks/>
        public event GetUserDescriptionByNameCompletedEventHandler GetUserDescriptionByNameCompleted;

        /// <remarks/>
        public event GetUserEMailByNameCompletedEventHandler GetUserEMailByNameCompleted;

        /// <remarks/>
        public event IsFloatingUserCompletedEventHandler IsFloatingUserCompleted;

        /// <remarks/>
        public event IsWebUserCompletedEventHandler IsWebUserCompleted;

        /// <remarks/>
        public event GetDbOwnerCompletedEventHandler GetDbOwnerCompleted;

        /// <remarks/>
        public event IsCompanySecuredCompletedEventHandler IsCompanySecuredCompleted;

        /// <remarks/>
        public event GetAuthenticationInformationsCompletedEventHandler GetAuthenticationInformationsCompleted;

        /// <remarks/>
        public event GetAuthenticationNamesCompletedEventHandler GetAuthenticationNamesCompleted;

        /// <remarks/>
        public event DeleteAssociationCompletedEventHandler DeleteAssociationCompleted;

        /// <remarks/>
        public event DeleteUserCompletedEventHandler DeleteUserCompleted;

        /// <remarks/>
        public event DeleteCompanyCompletedEventHandler DeleteCompanyCompleted;

        /// <remarks/>
        public event GetSystemDBConnectionStringCompletedEventHandler GetSystemDBConnectionStringCompleted;

        /// <remarks/>
        public event GetDMSConnectionStringCompletedEventHandler GetDMSConnectionStringCompleted;

        /// <remarks/>
        public event GetDMSDatabasesInfoCompletedEventHandler GetDMSDatabasesInfoCompleted;

        /// <remarks/>
        public event GetDataSynchroDatabasesInfoCompletedEventHandler GetDataSynchroDatabasesInfoCompleted;

        /// <remarks/>
        public event GetCompanyDatabasesInfoCompletedEventHandler GetCompanyDatabasesInfoCompleted;

        /// <remarks/>
        public event GetEditionCompletedEventHandler GetEditionCompleted;

        /// <remarks/>
        public event GetConfigurationStreamCompletedEventHandler GetConfigurationStreamCompleted;

        /// <remarks/>
        public event GetCountryCompletedEventHandler GetCountryCompleted;

        /// <remarks/>
        public event GetProviderNameFromCompanyIdCompletedEventHandler GetProviderNameFromCompanyIdCompleted;

        /// <remarks/>
        public event GetInstallationVersionCompletedEventHandler GetInstallationVersionCompleted;

        /// <remarks/>
        public event GetUserInfoCompletedEventHandler GetUserInfoCompleted;

        /// <remarks/>
        public event GetUserInfoIDCompletedEventHandler GetUserInfoIDCompleted;

        /// <remarks/>
        public event TraceActionCompletedEventHandler TraceActionCompleted;

        /// <remarks/>
        public event HasUserAlreadyChangedPasswordTodayCompletedEventHandler HasUserAlreadyChangedPasswordTodayCompleted;

        /// <remarks/>
        public event GetBrandedApplicationTitleCompletedEventHandler GetBrandedApplicationTitleCompleted;

        /// <remarks/>
        public event GetMasterProductBrandedNameCompletedEventHandler GetMasterProductBrandedNameCompleted;

        /// <remarks/>
        public event GetMasterSolutionBrandedNameCompletedEventHandler GetMasterSolutionBrandedNameCompleted;

        /// <remarks/>
        public event GetBrandedProducerNameCompletedEventHandler GetBrandedProducerNameCompleted;

        /// <remarks/>
        public event GetBrandedProductTitleCompletedEventHandler GetBrandedProductTitleCompleted;

        /// <remarks/>
        public event GetBrandedKeyCompletedEventHandler GetBrandedKeyCompleted;

        /// <remarks/>
        public event GetDBNetworkTypeCompletedEventHandler GetDBNetworkTypeCompleted;

        /// <remarks/>
        public event GetDatabaseTypeCompletedEventHandler GetDatabaseTypeCompleted;

        /// <remarks/>
        public event CanUseNamespaceCompletedEventHandler CanUseNamespaceCompleted;

        /// <remarks/>
        public event CacheCounterCompletedEventHandler CacheCounterCompleted;

        /// <remarks/>
        public event CacheCounterGTGCompletedEventHandler CacheCounterGTGCompleted;

        /// <remarks/>
        public event SetCurrentComponentsCompletedEventHandler SetCurrentComponentsCompleted;

        /// <remarks/>
        public event IsVirginActivationCompletedEventHandler IsVirginActivationCompleted;

        /// <remarks/>
        public event HDCompletedEventHandler HDCompleted;

        /// <remarks/>
        public event StoreMLUChoiceCompletedEventHandler StoreMLUChoiceCompleted;

        /// <remarks/>
        public event SaveLicensedCompletedEventHandler SaveLicensedCompleted;

        /// <remarks/>
        public event SaveUserInfoCompletedEventHandler SaveUserInfoCompleted;

        /// <remarks/>
        public event DeleteUserInfoCompletedEventHandler DeleteUserInfoCompleted;

        /// <remarks/>
        public event DeleteLicensedCompletedEventHandler DeleteLicensedCompleted;

        /// <remarks/>
        public event PrePingCompletedEventHandler PrePingCompleted;

        /// <remarks/>
        public event PingCompletedEventHandler PingCompleted;

        /// <remarks/>
        public event GetArticlesWithNamedCalCompletedEventHandler GetArticlesWithNamedCalCompleted;

        /// <remarks/>
        public event RefreshSecurityStatusCompletedEventHandler RefreshSecurityStatusCompleted;

        /// <remarks/>
        public event GetProxySupportVersionCompletedEventHandler GetProxySupportVersionCompleted;

        /// <remarks/>
        public event GetProxySettingsCompletedEventHandler GetProxySettingsCompleted;

        /// <remarks/>
        public event SetProxySettingsCompletedEventHandler SetProxySettingsCompleted;

        /// <remarks/>
        public event GetCompanyLanguageCompletedEventHandler GetCompanyLanguageCompleted;

        /// <remarks/>
        public event IsValidTokenCompletedEventHandler IsValidTokenCompleted;

        /// <remarks/>
        public event ReloadUserArticleBindingsCompletedEventHandler ReloadUserArticleBindingsCompleted;

        /// <remarks/>
        public event SbrillCompletedEventHandler SbrillCompleted;

        /// <remarks/>
        public event GetCalTypeCompletedEventHandler GetCalTypeCompleted;

        /// <remarks/>
        public event IsUserLoggedCompletedEventHandler IsUserLoggedCompleted;

        /// <remarks/>
        public event IsSecurityLightEnabledCompletedEventHandler IsSecurityLightEnabledCompleted;

        /// <remarks/>
        public event IsSecurityLightAccessAllowedCompletedEventHandler IsSecurityLightAccessAllowedCompleted;

        /// <remarks/>
        public event GetDBCultureLCIDCompletedEventHandler GetDBCultureLCIDCompleted;

        /// <remarks/>
        public event SetMessageReadCompletedEventHandler SetMessageReadCompleted;

        /// <remarks/>
        public event GetMessagesQueueCompletedEventHandler GetMessagesQueueCompleted;

        /// <remarks/>
        public event GetOldMessagesCompletedEventHandler GetOldMessagesCompleted;

        /// <remarks/>
        public event SendAccessMailCompletedEventHandler SendAccessMailCompleted;

        /// <remarks/>
        public event GetAspNetUserCompletedEventHandler GetAspNetUserCompleted;

        /// <remarks/>
        public event GetConfigurationHashCompletedEventHandler GetConfigurationHashCompleted;

        /// <remarks/>
        public event UserCanAccessWebSitePrivateAreaCompletedEventHandler UserCanAccessWebSitePrivateAreaCompleted;

        /// <remarks/>
        public event IsEasyBuilderDeveloperCompletedEventHandler IsEasyBuilderDeveloperCompleted;

        /// <remarks/>
        public event SendErrorFileCompletedEventHandler SendErrorFileCompleted;

        /// <remarks/>
        public event DownloadPdbCompletedEventHandler DownloadPdbCompleted;

        /// <remarks/>
        public event GetMainSerialNumberCompletedEventHandler GetMainSerialNumberCompleted;

        /// <remarks/>
        public event GetMLUExpiryDateCompletedEventHandler GetMLUExpiryDateCompleted;

        /// <remarks/>
        public event SendBalloonCompletedEventHandler SendBalloonCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Init", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int Init(bool reboot, string authenticationToken)
        {
            object[] results = this.Invoke("Init", new object[] {
                        reboot,
                        authenticationToken});
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void InitAsync(bool reboot, string authenticationToken)
        {
            this.InitAsync(reboot, authenticationToken, null);
        }

        /// <remarks/>
        public void InitAsync(bool reboot, string authenticationToken, object userState)
        {
            if ((this.InitOperationCompleted == null))
            {
                this.InitOperationCompleted = new System.Threading.SendOrPostCallback(this.OnInitOperationCompleted);
            }
            this.InvokeAsync("Init", new object[] {
                        reboot,
                        authenticationToken}, this.InitOperationCompleted, userState);
        }

        private void OnInitOperationCompleted(object arg)
        {
            if ((this.InitCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.InitCompleted(this, new InitCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsAlive", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsAlive()
        {
            object[] results = this.Invoke("IsAlive", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsAliveAsync()
        {
            this.IsAliveAsync(null);
        }

        /// <remarks/>
        public void IsAliveAsync(object userState)
        {
            if ((this.IsAliveOperationCompleted == null))
            {
                this.IsAliveOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsAliveOperationCompleted);
            }
            this.InvokeAsync("IsAlive", new object[0], this.IsAliveOperationCompleted, userState);
        }

        private void OnIsAliveOperationCompleted(object arg)
        {
            if ((this.IsAliveCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsAliveCompleted(this, new IsAliveCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsCalAvailable", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsCalAvailable(string authenticationToken, string application, string functionality)
        {
            object[] results = this.Invoke("IsCalAvailable", new object[] {
                        authenticationToken,
                        application,
                        functionality});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsCalAvailableAsync(string authenticationToken, string application, string functionality)
        {
            this.IsCalAvailableAsync(authenticationToken, application, functionality, null);
        }

        /// <remarks/>
        public void IsCalAvailableAsync(string authenticationToken, string application, string functionality, object userState)
        {
            if ((this.IsCalAvailableOperationCompleted == null))
            {
                this.IsCalAvailableOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsCalAvailableOperationCompleted);
            }
            this.InvokeAsync("IsCalAvailable", new object[] {
                        authenticationToken,
                        application,
                        functionality}, this.IsCalAvailableOperationCompleted, userState);
        }

        private void OnIsCalAvailableOperationCompleted(object arg)
        {
            if ((this.IsCalAvailableCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsCalAvailableCompleted(this, new IsCalAvailableCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/PingNeeded", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool PingNeeded(bool force)
        {
            object[] results = this.Invoke("PingNeeded", new object[] {
                        force});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void PingNeededAsync(bool force)
        {
            this.PingNeededAsync(force, null);
        }

        /// <remarks/>
        public void PingNeededAsync(bool force, object userState)
        {
            if ((this.PingNeededOperationCompleted == null))
            {
                this.PingNeededOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPingNeededOperationCompleted);
            }
            this.InvokeAsync("PingNeeded", new object[] {
                        force}, this.PingNeededOperationCompleted, userState);
        }

        private void OnPingNeededOperationCompleted(object arg)
        {
            if ((this.PingNeededCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PingNeededCompleted(this, new PingNeededCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SetClientData", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SetClientData(ClientData cd)
        {
            object[] results = this.Invoke("SetClientData", new object[] {

                        cd});

        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SetCompanyInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SetCompanyInfo(string authToken, string aName, string aValue)
        {
            object[] results = this.Invoke("SetCompanyInfo", new object[] {
                        authToken,
                        aName,
                        aValue});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SetCompanyInfoAsync(string authToken, string aName, string aValue)
        {
            this.SetCompanyInfoAsync(authToken, aName, aValue, null);
        }

        /// <remarks/>
        public void SetCompanyInfoAsync(string authToken, string aName, string aValue, object userState)
        {
            if ((this.SetCompanyInfoOperationCompleted == null))
            {
                this.SetCompanyInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetCompanyInfoOperationCompleted);
            }
            this.InvokeAsync("SetCompanyInfo", new object[] {
                        authToken,
                        aName,
                        aValue}, this.SetCompanyInfoOperationCompleted, userState);
        }

        private void OnSetCompanyInfoOperationCompleted(object arg)
        {
            if ((this.SetCompanyInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetCompanyInfoCompleted(this, new SetCompanyInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsActivated", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsActivated(string application, string functionality)
        {
            object[] results = this.Invoke("IsActivated", new object[] {
                        application,
                        functionality});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsActivatedAsync(string application, string functionality)
        {
            this.IsActivatedAsync(application, functionality, null);
        }

        /// <remarks/>
        public void IsActivatedAsync(string application, string functionality, object userState)
        {
            if ((this.IsActivatedOperationCompleted == null))
            {
                this.IsActivatedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsActivatedOperationCompleted);
            }
            this.InvokeAsync("IsActivated", new object[] {
                        application,
                        functionality}, this.IsActivatedOperationCompleted, userState);
        }

        private void OnIsActivatedOperationCompleted(object arg)
        {
            if ((this.IsActivatedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsActivatedCompleted(this, new IsActivatedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsSynchActivation", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsSynchActivation()
        {
            object[] results = this.Invoke("IsSynchActivation", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsSynchActivationAsync()
        {
            this.IsSynchActivationAsync(null);
        }

        /// <remarks/>
        public void IsSynchActivationAsync(object userState)
        {
            if ((this.IsSynchActivationOperationCompleted == null))
            {
                this.IsSynchActivationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsSynchActivationOperationCompleted);
            }
            this.InvokeAsync("IsSynchActivation", new object[0], this.IsSynchActivationOperationCompleted, userState);
        }

        private void OnIsSynchActivationOperationCompleted(object arg)
        {
            if ((this.IsSynchActivationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsSynchActivationCompleted(this, new IsSynchActivationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetModules", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetModules()
        {
            object[] results = this.Invoke("GetModules", new object[0]);
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetModulesAsync()
        {
            this.GetModulesAsync(null);
        }

        /// <remarks/>
        public void GetModulesAsync(object userState)
        {
            if ((this.GetModulesOperationCompleted == null))
            {
                this.GetModulesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetModulesOperationCompleted);
            }
            this.InvokeAsync("GetModules", new object[0], this.GetModulesOperationCompleted, userState);
        }

        private void OnGetModulesOperationCompleted(object arg)
        {
            if ((this.GetModulesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetModulesCompleted(this, new GetModulesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCompanyUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetCompanyUsers(string companyName)
        {
            object[] results = this.Invoke("GetCompanyUsers", new object[] {
                        companyName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetCompanyUsersAsync(string companyName)
        {
            this.GetCompanyUsersAsync(companyName, null);
        }

        /// <remarks/>
        public void GetCompanyUsersAsync(string companyName, object userState)
        {
            if ((this.GetCompanyUsersOperationCompleted == null))
            {
                this.GetCompanyUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCompanyUsersOperationCompleted);
            }
            this.InvokeAsync("GetCompanyUsers", new object[] {
                        companyName}, this.GetCompanyUsersOperationCompleted, userState);
        }

        private void OnGetCompanyUsersOperationCompleted(object arg)
        {
            if ((this.GetCompanyUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCompanyUsersCompleted(this, new GetCompanyUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetNonNTCompanyUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetNonNTCompanyUsers(string companyName)
        {
            object[] results = this.Invoke("GetNonNTCompanyUsers", new object[] {
                        companyName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetNonNTCompanyUsersAsync(string companyName)
        {
            this.GetNonNTCompanyUsersAsync(companyName, null);
        }

        /// <remarks/>
        public void GetNonNTCompanyUsersAsync(string companyName, object userState)
        {
            if ((this.GetNonNTCompanyUsersOperationCompleted == null))
            {
                this.GetNonNTCompanyUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetNonNTCompanyUsersOperationCompleted);
            }
            this.InvokeAsync("GetNonNTCompanyUsers", new object[] {
                        companyName}, this.GetNonNTCompanyUsersOperationCompleted, userState);
        }

        private void OnGetNonNTCompanyUsersOperationCompleted(object arg)
        {
            if ((this.GetNonNTCompanyUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetNonNTCompanyUsersCompleted(this, new GetNonNTCompanyUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCompanyRoles", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetCompanyRoles(string companyName)
        {
            object[] results = this.Invoke("GetCompanyRoles", new object[] {
                        companyName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetCompanyRolesAsync(string companyName)
        {
            this.GetCompanyRolesAsync(companyName, null);
        }

        /// <remarks/>
        public void GetCompanyRolesAsync(string companyName, object userState)
        {
            if ((this.GetCompanyRolesOperationCompleted == null))
            {
                this.GetCompanyRolesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCompanyRolesOperationCompleted);
            }
            this.InvokeAsync("GetCompanyRoles", new object[] {
                        companyName}, this.GetCompanyRolesOperationCompleted, userState);
        }

        private void OnGetCompanyRolesOperationCompleted(object arg)
        {
            if ((this.GetCompanyRolesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCompanyRolesCompleted(this, new GetCompanyRolesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserRoles", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetUserRoles(string companyName, string userName)
        {
            object[] results = this.Invoke("GetUserRoles", new object[] {
                        companyName,
                        userName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetUserRolesAsync(string companyName, string userName)
        {
            this.GetUserRolesAsync(companyName, userName, null);
        }

        /// <remarks/>
        public void GetUserRolesAsync(string companyName, string userName, object userState)
        {
            if ((this.GetUserRolesOperationCompleted == null))
            {
                this.GetUserRolesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserRolesOperationCompleted);
            }
            this.InvokeAsync("GetUserRoles", new object[] {
                        companyName,
                        userName}, this.GetUserRolesOperationCompleted, userState);
        }

        private void OnGetUserRolesOperationCompleted(object arg)
        {
            if ((this.GetUserRolesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserRolesCompleted(this, new GetUserRolesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/EnumAllUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] EnumAllUsers()
        {
            object[] results = this.Invoke("EnumAllUsers", new object[0]);
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void EnumAllUsersAsync()
        {
            this.EnumAllUsersAsync(null);
        }

        /// <remarks/>
        public void EnumAllUsersAsync(object userState)
        {
            if ((this.EnumAllUsersOperationCompleted == null))
            {
                this.EnumAllUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEnumAllUsersOperationCompleted);
            }
            this.InvokeAsync("EnumAllUsers", new object[0], this.EnumAllUsersOperationCompleted, userState);
        }

        private void OnEnumAllUsersOperationCompleted(object arg)
        {
            if ((this.EnumAllUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.EnumAllUsersCompleted(this, new EnumAllUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/EnumAllCompanyUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers)
        {
            object[] results = this.Invoke("EnumAllCompanyUsers", new object[] {
                        companyId,
                        onlyNonNTUsers});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void EnumAllCompanyUsersAsync(int companyId, bool onlyNonNTUsers)
        {
            this.EnumAllCompanyUsersAsync(companyId, onlyNonNTUsers, null);
        }

        /// <remarks/>
        public void EnumAllCompanyUsersAsync(int companyId, bool onlyNonNTUsers, object userState)
        {
            if ((this.EnumAllCompanyUsersOperationCompleted == null))
            {
                this.EnumAllCompanyUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEnumAllCompanyUsersOperationCompleted);
            }
            this.InvokeAsync("EnumAllCompanyUsers", new object[] {
                        companyId,
                        onlyNonNTUsers}, this.EnumAllCompanyUsersOperationCompleted, userState);
        }

        private void OnEnumAllCompanyUsersOperationCompleted(object arg)
        {
            if ((this.EnumAllCompanyUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.EnumAllCompanyUsersCompleted(this, new EnumAllCompanyUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetRoleUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetRoleUsers(string companyName, string roleName)
        {
            object[] results = this.Invoke("GetRoleUsers", new object[] {
                        companyName,
                        roleName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetRoleUsersAsync(string companyName, string roleName)
        {
            this.GetRoleUsersAsync(companyName, roleName, null);
        }

        /// <remarks/>
        public void GetRoleUsersAsync(string companyName, string roleName, object userState)
        {
            if ((this.GetRoleUsersOperationCompleted == null))
            {
                this.GetRoleUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRoleUsersOperationCompleted);
            }
            this.InvokeAsync("GetRoleUsers", new object[] {
                        companyName,
                        roleName}, this.GetRoleUsersOperationCompleted, userState);
        }

        private void OnGetRoleUsersOperationCompleted(object arg)
        {
            if ((this.GetRoleUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRoleUsersCompleted(this, new GetRoleUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/EnumCompanies", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] EnumCompanies(string userName)
        {
            object[] results = this.Invoke("EnumCompanies", new object[] {
                        userName});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void EnumCompaniesAsync(string userName)
        {
            this.EnumCompaniesAsync(userName, null);
        }

        /// <remarks/>
        public void EnumCompaniesAsync(string userName, object userState)
        {
            if ((this.EnumCompaniesOperationCompleted == null))
            {
                this.EnumCompaniesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEnumCompaniesOperationCompleted);
            }
            this.InvokeAsync("EnumCompanies", new object[] {
                        userName}, this.EnumCompaniesOperationCompleted, userState);
        }

        private void OnEnumCompaniesOperationCompleted(object arg)
        {
            if ((this.EnumCompaniesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.EnumCompaniesCompleted(this, new EnumCompaniesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsIntegratedSecurityUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsIntegratedSecurityUser(string userName)
        {
            object[] results = this.Invoke("IsIntegratedSecurityUser", new object[] {
                        userName});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsIntegratedSecurityUserAsync(string userName)
        {
            this.IsIntegratedSecurityUserAsync(userName, null);
        }

        /// <remarks/>
        public void IsIntegratedSecurityUserAsync(string userName, object userState)
        {
            if ((this.IsIntegratedSecurityUserOperationCompleted == null))
            {
                this.IsIntegratedSecurityUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsIntegratedSecurityUserOperationCompleted);
            }
            this.InvokeAsync("IsIntegratedSecurityUser", new object[] {
                        userName}, this.IsIntegratedSecurityUserOperationCompleted, userState);
        }

        private void OnIsIntegratedSecurityUserOperationCompleted(object arg)
        {
            if ((this.IsIntegratedSecurityUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsIntegratedSecurityUserCompleted(this, new IsIntegratedSecurityUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetLoggedUsersNumber", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetLoggedUsersNumber()
        {
            object[] results = this.Invoke("GetLoggedUsersNumber", new object[0]);
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetLoggedUsersNumberAsync()
        {
            this.GetLoggedUsersNumberAsync(null);
        }

        /// <remarks/>
        public void GetLoggedUsersNumberAsync(object userState)
        {
            if ((this.GetLoggedUsersNumberOperationCompleted == null))
            {
                this.GetLoggedUsersNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLoggedUsersNumberOperationCompleted);
            }
            this.InvokeAsync("GetLoggedUsersNumber", new object[0], this.GetLoggedUsersNumberOperationCompleted, userState);
        }

        private void OnGetLoggedUsersNumberOperationCompleted(object arg)
        {
            if ((this.GetLoggedUsersNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetLoggedUsersNumberCompleted(this, new GetLoggedUsersNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCompanyLoggedUsersNumber", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetCompanyLoggedUsersNumber(int companyId)
        {
            object[] results = this.Invoke("GetCompanyLoggedUsersNumber", new object[] {
                        companyId});
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetCompanyLoggedUsersNumberAsync(int companyId)
        {
            this.GetCompanyLoggedUsersNumberAsync(companyId, null);
        }

        /// <remarks/>
        public void GetCompanyLoggedUsersNumberAsync(int companyId, object userState)
        {
            if ((this.GetCompanyLoggedUsersNumberOperationCompleted == null))
            {
                this.GetCompanyLoggedUsersNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCompanyLoggedUsersNumberOperationCompleted);
            }
            this.InvokeAsync("GetCompanyLoggedUsersNumber", new object[] {
                        companyId}, this.GetCompanyLoggedUsersNumberOperationCompleted, userState);
        }

        private void OnGetCompanyLoggedUsersNumberOperationCompleted(object arg)
        {
            if ((this.GetCompanyLoggedUsersNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCompanyLoggedUsersNumberCompleted(this, new GetCompanyLoggedUsersNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetLoggedUsers", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetLoggedUsers()
        {
            object[] results = this.Invoke("GetLoggedUsers", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetLoggedUsersAsync()
        {
            this.GetLoggedUsersAsync(null);
        }

        /// <remarks/>
        public void GetLoggedUsersAsync(object userState)
        {
            if ((this.GetLoggedUsersOperationCompleted == null))
            {
                this.GetLoggedUsersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLoggedUsersOperationCompleted);
            }
            this.InvokeAsync("GetLoggedUsers", new object[0], this.GetLoggedUsersOperationCompleted, userState);
        }

        private void OnGetLoggedUsersOperationCompleted(object arg)
        {
            if ((this.GetLoggedUsersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetLoggedUsersCompleted(this, new GetLoggedUsersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMobileToken", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMobileToken(string token, int loginType)
        {
            object[] results = this.Invoke("GetMobileToken", new object[] {
                        token,loginType});
            return ((string)(results[0]));
        }



        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetIToken", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetIToken(string token)
        {
            object[] results = this.Invoke("GetIToken", new object[] {
                        token});
            return ((string)(results[0]));
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetLoggedUsersAdvanced", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetLoggedUsersAdvanced(string token)
        {
            object[] results = this.Invoke("GetLoggedUsersAdvanced", new object[] {
                        token});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetLoggedUsersAdvancedAsync(string token)
        {
            this.GetLoggedUsersAdvancedAsync(token, null);
        }

        /// <remarks/>
        public void GetLoggedUsersAdvancedAsync(string token, object userState)
        {
            if ((this.GetLoggedUsersAdvancedOperationCompleted == null))
            {
                this.GetLoggedUsersAdvancedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLoggedUsersAdvancedOperationCompleted);
            }
            this.InvokeAsync("GetLoggedUsersAdvanced", new object[] {
                        token}, this.GetLoggedUsersAdvancedOperationCompleted, userState);
        }

        private void OnGetLoggedUsersAdvancedOperationCompleted(object arg)
        {
            if ((this.GetLoggedUsersAdvancedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetLoggedUsersAdvancedCompleted(this, new GetLoggedUsersAdvancedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCalNumber", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("namedCal")]
        public int GetCalNumber(out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal)
        {
            object[] results = this.Invoke("GetCalNumber", new object[0]);
            gdiConcurrent = ((int)(results[1]));
            unnamedCal = ((int)(results[2]));
            officeCal = ((int)(results[3]));
            tpCal = ((int)(results[4]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetCalNumberAsync()
        {
            this.GetCalNumberAsync(null);
        }

        /// <remarks/>
        public void GetCalNumberAsync(object userState)
        {
            if ((this.GetCalNumberOperationCompleted == null))
            {
                this.GetCalNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCalNumberOperationCompleted);
            }
            this.InvokeAsync("GetCalNumber", new object[0], this.GetCalNumberOperationCompleted, userState);
        }

        private void OnGetCalNumberOperationCompleted(object arg)
        {
            if ((this.GetCalNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCalNumberCompleted(this, new GetCalNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCalNumber2", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("namedCal")]
        public int GetCalNumber2(out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal, out int wmsCal, out int manufacturingCal)
        {
            object[] results = this.Invoke("GetCalNumber2", new object[0]);
            gdiConcurrent = ((int)(results[1]));
            unnamedCal = ((int)(results[2]));
            officeCal = ((int)(results[3]));
            tpCal = ((int)(results[4]));
            wmsCal = ((int)(results[5]));
            manufacturingCal = ((int)(results[6]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetCalNumber2Async()
        {
            this.GetCalNumber2Async(null);
        }

        /// <remarks/>
        public void GetCalNumber2Async(object userState)
        {
            if ((this.GetCalNumber2OperationCompleted == null))
            {
                this.GetCalNumber2OperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCalNumber2OperationCompleted);
            }
            this.InvokeAsync("GetCalNumber2", new object[0], this.GetCalNumber2OperationCompleted, userState);
        }

        private void OnGetCalNumber2OperationCompleted(object arg)
        {
            if ((this.GetCalNumber2Completed != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCalNumber2Completed(this, new GetCalNumber2CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetTokenProcessType", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetTokenProcessType(string token)
        {
            object[] results = this.Invoke("GetTokenProcessType", new object[] {
                        token});
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetTokenProcessTypeAsync(string token)
        {
            this.GetTokenProcessTypeAsync(token, null);
        }

        /// <remarks/>
        public void GetTokenProcessTypeAsync(string token, object userState)
        {
            if ((this.GetTokenProcessTypeOperationCompleted == null))
            {
                this.GetTokenProcessTypeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTokenProcessTypeOperationCompleted);
            }
            this.InvokeAsync("GetTokenProcessType", new object[] {
                        token}, this.GetTokenProcessTypeOperationCompleted, userState);
        }

        private void OnGetTokenProcessTypeOperationCompleted(object arg)
        {
            if ((this.GetTokenProcessTypeCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTokenProcessTypeCompleted(this, new GetTokenProcessTypeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ReloadConfiguration", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void ReloadConfiguration()
        {
            this.Invoke("ReloadConfiguration", new object[0]);
        }

        /// <remarks/>
        public void ReloadConfigurationAsync()
        {
            this.ReloadConfigurationAsync(null);
        }

        /// <remarks/>
        public void ReloadConfigurationAsync(object userState)
        {
            if ((this.ReloadConfigurationOperationCompleted == null))
            {
                this.ReloadConfigurationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnReloadConfigurationOperationCompleted);
            }
            this.InvokeAsync("ReloadConfiguration", new object[0], this.ReloadConfigurationOperationCompleted, userState);
        }

        private void OnReloadConfigurationOperationCompleted(object arg)
        {
            if ((this.ReloadConfigurationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ReloadConfigurationCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ValidateUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int ValidateUser(string userName, string password, bool winNtAuthentication, out string[] userCompanies, out int loginId, out bool userCannotChangePassword, out bool userMustChangePassword, out System.DateTime expiredDatePassword, out bool passwordNeverExpired, out bool expiredDateCannotChange)
        {
            object[] results = this.Invoke("ValidateUser", new object[] {
                        userName,
                        password,
                        winNtAuthentication});
            userCompanies = ((string[])(results[1]));
            loginId = ((int)(results[2]));
            userCannotChangePassword = ((bool)(results[3]));
            userMustChangePassword = ((bool)(results[4]));
            expiredDatePassword = ((System.DateTime)(results[5]));
            passwordNeverExpired = ((bool)(results[6]));
            expiredDateCannotChange = ((bool)(results[7]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void ValidateUserAsync(string userName, string password, bool winNtAuthentication)
        {
            this.ValidateUserAsync(userName, password, winNtAuthentication, null);
        }

        /// <remarks/>
        public void ValidateUserAsync(string userName, string password, bool winNtAuthentication, object userState)
        {
            if ((this.ValidateUserOperationCompleted == null))
            {
                this.ValidateUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnValidateUserOperationCompleted);
            }
            this.InvokeAsync("ValidateUser", new object[] {
                        userName,
                        password,
                        winNtAuthentication}, this.ValidateUserOperationCompleted, userState);
        }

        private void OnValidateUserOperationCompleted(object arg)
        {
            if ((this.ValidateUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ValidateUserCompleted(this, new ValidateUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ChangePassword", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {
            object[] results = this.Invoke("ChangePassword", new object[] {
                        userName,
                        oldPassword,
                        newPassword});
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            this.ChangePasswordAsync(userName, oldPassword, newPassword, null);
        }

        /// <remarks/>
        public void ChangePasswordAsync(string userName, string oldPassword, string newPassword, object userState)
        {
            if ((this.ChangePasswordOperationCompleted == null))
            {
                this.ChangePasswordOperationCompleted = new System.Threading.SendOrPostCallback(this.OnChangePasswordOperationCompleted);
            }
            this.InvokeAsync("ChangePassword", new object[] {
                        userName,
                        oldPassword,
                        newPassword}, this.ChangePasswordOperationCompleted, userState);
        }

        private void OnChangePasswordOperationCompleted(object arg)
        {
            if ((this.ChangePasswordCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ChangePasswordCompleted(this, new ChangePasswordCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/LoginCompact", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int LoginCompact(ref string userName, ref string companyName, string password, string askingProcess, bool overWriteLogin, out string authenticationToken)
        {
            object[] results = this.Invoke("LoginCompact", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        overWriteLogin});
            userName = ((string)(results[1]));
            companyName = ((string)(results[2]));
            authenticationToken = ((string)(results[3]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void LoginCompactAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin)
        {
            this.LoginCompactAsync(userName, companyName, password, askingProcess, overWriteLogin, null);
        }

        /// <remarks/>
        public void LoginCompactAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin, object userState)
        {
            if ((this.LoginCompactOperationCompleted == null))
            {
                this.LoginCompactOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginCompactOperationCompleted);
            }
            this.InvokeAsync("LoginCompact", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        overWriteLogin}, this.LoginCompactOperationCompleted, userState);
        }

        private void OnLoginCompactOperationCompleted(object arg)
        {
            if ((this.LoginCompactCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginCompactCompleted(this, new LoginCompactCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SsoLogin", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int SsoLogin(ref LoginProperties loginProperties)
        {
            object[] results = this.Invoke("SsoLogin", new object[] {
                        loginProperties});
            loginProperties = ((LoginProperties)(results[1]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void SsoLoginAsync(LoginProperties loginProperties)
        {
            this.SsoLoginAsync(loginProperties, null);
        }

        /// <remarks/>
        public void SsoLoginAsync(LoginProperties loginProperties, object userState)
        {
            if ((this.SsoLoginOperationCompleted == null))
            {
                this.SsoLoginOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSsoLoginOperationCompleted);
            }
            this.InvokeAsync("SsoLogin", new object[] {
                        loginProperties}, this.SsoLoginOperationCompleted, userState);
        }

        private void OnSsoLoginOperationCompleted(object arg)
        {
            if ((this.SsoLoginCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SsoLoginCompleted(this, new SsoLoginCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SsoLoggedUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int SsoLoggedUser(ref LoginProperties loginProperties)
        {
            object[] results = this.Invoke("SsoLoggedUser", new object[] {
                        loginProperties});
            loginProperties = ((LoginProperties)(results[1]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void SsoLoggedUserAsync(LoginProperties loginProperties)
        {
            this.SsoLoggedUserAsync(loginProperties, null);
        }

        /// <remarks/>
        public void SsoLoggedUserAsync(LoginProperties loginProperties, object userState)
        {
            if ((this.SsoLoggedUserOperationCompleted == null))
            {
                this.SsoLoggedUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSsoLoggedUserOperationCompleted);
            }
            this.InvokeAsync("SsoLoggedUser", new object[] {
                        loginProperties}, this.SsoLoggedUserOperationCompleted, userState);
        }

        private void OnSsoLoggedUserOperationCompleted(object arg)
        {
            if ((this.SsoLoggedUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SsoLoggedUserCompleted(this, new SsoLoggedUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Login", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
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
            object[] results = this.Invoke("Login", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        overWriteLogin});
            userName = ((string)(results[1]));
            companyName = ((string)(results[2]));
            admin = ((bool)(results[3]));
            authenticationToken = ((string)(results[4]));
            companyId = ((int)(results[5]));
            dbName = ((string)(results[6]));
            dbServer = ((string)(results[7]));
            providerId = ((int)(results[8]));
            security = ((bool)(results[9]));
            auditing = ((bool)(results[10]));
            useKeyedUpdate = ((bool)(results[11]));
            transactionUse = ((bool)(results[12]));
            preferredLanguage = ((string)(results[13]));
            applicationLanguage = ((string)(results[14]));
            providerName = ((string)(results[15]));
            providerDescription = ((string)(results[16]));
            useConstParameter = ((bool)(results[17]));
            stripTrailingSpaces = ((bool)(results[18]));
            providerCompanyConnectionString = ((string)(results[19]));
            nonProviderCompanyConnectionString = ((string)(results[20]));
            dbUser = ((string)(results[21]));
            activationDB = ((string)(results[22]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void LoginAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin)
        {
            this.LoginAsync(userName, companyName, password, askingProcess, overWriteLogin, null);
        }

        /// <remarks/>
        public void LoginAsync(string userName, string companyName, string password, string askingProcess, bool overWriteLogin, object userState)
        {
            if ((this.LoginOperationCompleted == null))
            {
                this.LoginOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginOperationCompleted);
            }
            this.InvokeAsync("Login", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        overWriteLogin}, this.LoginOperationCompleted, userState);
        }

        private void OnLoginOperationCompleted(object arg)
        {
            if ((this.LoginCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginCompleted(this, new LoginCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Login2", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
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
            object[] results = this.Invoke("Login2", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        macIp,
                        overWriteLogin});
            userName = ((string)(results[1]));
            companyName = ((string)(results[2]));
            admin = ((bool)(results[3]));
            authenticationToken = ((string)(results[4]));
            companyId = ((int)(results[5]));
            dbName = ((string)(results[6]));
            dbServer = ((string)(results[7]));
            providerId = ((int)(results[8]));
            security = ((bool)(results[9]));
            auditing = ((bool)(results[10]));
            useKeyedUpdate = ((bool)(results[11]));
            transactionUse = ((bool)(results[12]));
            preferredLanguage = ((string)(results[13]));
            applicationLanguage = ((string)(results[14]));
            providerName = ((string)(results[15]));
            providerDescription = ((string)(results[16]));
            useConstParameter = ((bool)(results[17]));
            stripTrailingSpaces = ((bool)(results[18]));
            providerCompanyConnectionString = ((string)(results[19]));
            nonProviderCompanyConnectionString = ((string)(results[20]));
            dbUser = ((string)(results[21]));
            activationDB = ((string)(results[22]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void Login2Async(string userName, string companyName, string password, string askingProcess, string macIp, bool overWriteLogin)
        {
            this.Login2Async(userName, companyName, password, askingProcess, macIp, overWriteLogin, null);
        }

        /// <remarks/>
        public void Login2Async(string userName, string companyName, string password, string askingProcess, string macIp, bool overWriteLogin, object userState)
        {
            if ((this.Login2OperationCompleted == null))
            {
                this.Login2OperationCompleted = new System.Threading.SendOrPostCallback(this.OnLogin2OperationCompleted);
            }
            this.InvokeAsync("Login2", new object[] {
                        userName,
                        companyName,
                        password,
                        askingProcess,
                        macIp,
                        overWriteLogin}, this.Login2OperationCompleted, userState);
        }

        private void OnLogin2OperationCompleted(object arg)
        {
            if ((this.Login2Completed != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.Login2Completed(this, new Login2CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUsagePercentageOnDBSize", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetUsagePercentageOnDBSize()
        {
            object[] results = this.Invoke("GetUsagePercentageOnDBSize", new object[0]);
            return ((int)(results[0]));
        }

        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ConfirmToken", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool ConfirmToken(
                    string authenticationToken, string procType)
        {
            object[] results = this.Invoke("ConfirmToken", new object[] {
                        authenticationToken, procType});
            return ((bool)(results[0]));
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetLoginInformation", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
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
            object[] results = this.Invoke("GetLoginInformation", new object[] {
                        authenticationToken});
            userName = ((string)(results[1]));
            loginId = ((int)(results[2]));
            companyName = ((string)(results[3]));
            companyId = ((int)(results[4]));
            admin = ((bool)(results[5]));
            dbName = ((string)(results[6]));
            dbServer = ((string)(results[7]));
            providerId = ((int)(results[8]));
            security = ((bool)(results[9]));
            auditing = ((bool)(results[10]));
            useKeyedUpdate = ((bool)(results[11]));
            transactionUse = ((bool)(results[12]));
            useUnicode = ((bool)(results[13]));
            preferredLanguage = ((string)(results[14]));
            applicationLanguage = ((string)(results[15]));
            providerName = ((string)(results[16]));
            providerDescription = ((string)(results[17]));
            useConstParameter = ((bool)(results[18]));
            stripTrailingSpaces = ((bool)(results[19]));
            providerCompanyConnectionString = ((string)(results[20]));
            nonProviderCompanyConnectionString = ((string)(results[21]));
            dbUser = ((string)(results[22]));
            processName = ((string)(results[23]));
            userDescription = ((string)(results[24]));
            email = ((string)(results[25]));
            easyBuilderDeveloper = ((bool)(results[26]));
            rowSecurity = ((bool)(results[27]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void GetLoginInformationAsync(string authenticationToken)
        {
            this.GetLoginInformationAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetLoginInformationAsync(string authenticationToken, object userState)
        {
            if ((this.GetLoginInformationOperationCompleted == null))
            {
                this.GetLoginInformationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLoginInformationOperationCompleted);
            }
            this.InvokeAsync("GetLoginInformation", new object[] {
                        authenticationToken}, this.GetLoginInformationOperationCompleted, userState);
        }

        private void OnGetLoginInformationOperationCompleted(object arg)
        {
            if ((this.GetLoginInformationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetLoginInformationCompleted(this, new GetLoginInformationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/LoginViaInfinityToken2", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int LoginViaInfinityToken2(string cryptedToken, string username, string password, string company, out string authenticationToken)
        {
            object[] results = this.Invoke("LoginViaInfinityToken2", new object[] {
                        cryptedToken, username, password, company});
            authenticationToken = ((string)(results[1]));
            return ((int)(results[0]));



        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SSOLogOff", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SSOLogOff(string cryptedToken)
        {
            this.Invoke("SSOLogOff", new object[] {
                        cryptedToken});
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/LogOff", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void LogOff(string authenticationToken)
        {
            this.Invoke("LogOff", new object[] {
                        authenticationToken});
        }

        /// <remarks/>
        public void LogOffAsync(string authenticationToken)
        {
            this.LogOffAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void LogOffAsync(string authenticationToken, object userState)
        {
            if ((this.LogOffOperationCompleted == null))
            {
                this.LogOffOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLogOffOperationCompleted);
            }
            this.InvokeAsync("LogOff", new object[] {
                        authenticationToken}, this.LogOffOperationCompleted, userState);
        }

        private void OnLogOffOperationCompleted(object arg)
        {
            if ((this.LogOffCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LogOffCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserName(int loginId)
        {
            object[] results = this.Invoke("GetUserName", new object[] {
                        loginId});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserNameAsync(int loginId)
        {
            this.GetUserNameAsync(loginId, null);
        }

        /// <remarks/>
        public void GetUserNameAsync(int loginId, object userState)
        {
            if ((this.GetUserNameOperationCompleted == null))
            {
                this.GetUserNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserNameOperationCompleted);
            }
            this.InvokeAsync("GetUserName", new object[] {
                        loginId}, this.GetUserNameOperationCompleted, userState);
        }

        private void OnGetUserNameOperationCompleted(object arg)
        {
            if ((this.GetUserNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserNameCompleted(this, new GetUserNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserDescriptionById", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserDescriptionById(int loginId)
        {
            object[] results = this.Invoke("GetUserDescriptionById", new object[] {
                        loginId});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserDescriptionByIdAsync(int loginId)
        {
            this.GetUserDescriptionByIdAsync(loginId, null);
        }

        /// <remarks/>
        public void GetUserDescriptionByIdAsync(int loginId, object userState)
        {
            if ((this.GetUserDescriptionByIdOperationCompleted == null))
            {
                this.GetUserDescriptionByIdOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserDescriptionByIdOperationCompleted);
            }
            this.InvokeAsync("GetUserDescriptionById", new object[] {
                        loginId}, this.GetUserDescriptionByIdOperationCompleted, userState);
        }

        private void OnGetUserDescriptionByIdOperationCompleted(object arg)
        {
            if ((this.GetUserDescriptionByIdCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserDescriptionByIdCompleted(this, new GetUserDescriptionByIdCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserDescriptionByName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserDescriptionByName(string login)
        {
            object[] results = this.Invoke("GetUserDescriptionByName", new object[] {
                        login});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserDescriptionByNameAsync(string login)
        {
            this.GetUserDescriptionByNameAsync(login, null);
        }

        /// <remarks/>
        public void GetUserDescriptionByNameAsync(string login, object userState)
        {
            if ((this.GetUserDescriptionByNameOperationCompleted == null))
            {
                this.GetUserDescriptionByNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserDescriptionByNameOperationCompleted);
            }
            this.InvokeAsync("GetUserDescriptionByName", new object[] {
                        login}, this.GetUserDescriptionByNameOperationCompleted, userState);
        }

        private void OnGetUserDescriptionByNameOperationCompleted(object arg)
        {
            if ((this.GetUserDescriptionByNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserDescriptionByNameCompleted(this, new GetUserDescriptionByNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserEMailByName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserEMailByName(string login)
        {
            object[] results = this.Invoke("GetUserEMailByName", new object[] {
                        login});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserEMailByNameAsync(string login)
        {
            this.GetUserEMailByNameAsync(login, null);
        }

        /// <remarks/>
        public void GetUserEMailByNameAsync(string login, object userState)
        {
            if ((this.GetUserEMailByNameOperationCompleted == null))
            {
                this.GetUserEMailByNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserEMailByNameOperationCompleted);
            }
            this.InvokeAsync("GetUserEMailByName", new object[] {
                        login}, this.GetUserEMailByNameOperationCompleted, userState);
        }

        private void OnGetUserEMailByNameOperationCompleted(object arg)
        {
            if ((this.GetUserEMailByNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserEMailByNameCompleted(this, new GetUserEMailByNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsFloatingUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsFloatingUser(string loginName, out bool floating)
        {
            object[] results = this.Invoke("IsFloatingUser", new object[] {
                        loginName});
            floating = ((bool)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsFloatingUserAsync(string loginName)
        {
            this.IsFloatingUserAsync(loginName, null);
        }

        /// <remarks/>
        public void IsFloatingUserAsync(string loginName, object userState)
        {
            if ((this.IsFloatingUserOperationCompleted == null))
            {
                this.IsFloatingUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsFloatingUserOperationCompleted);
            }
            this.InvokeAsync("IsFloatingUser", new object[] {
                        loginName}, this.IsFloatingUserOperationCompleted, userState);
        }

        private void OnIsFloatingUserOperationCompleted(object arg)
        {
            if ((this.IsFloatingUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsFloatingUserCompleted(this, new IsFloatingUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsWinNT", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsWinNT(int loginId)
        {
            object[] results = this.Invoke("IsWinNT", new object[] {
                        loginId});
            return ((bool)(results[0]));
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsWebUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsWebUser(string loginName, out bool web)
        {
            object[] results = this.Invoke("IsWebUser", new object[] {
                        loginName});
            web = ((bool)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsWebUserAsync(string loginName)
        {
            this.IsWebUserAsync(loginName, null);
        }

        /// <remarks/>
        public void IsWebUserAsync(string loginName, object userState)
        {
            if ((this.IsWebUserOperationCompleted == null))
            {
                this.IsWebUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsWebUserOperationCompleted);
            }
            this.InvokeAsync("IsWebUser", new object[] {
                        loginName}, this.IsWebUserOperationCompleted, userState);
        }

        private void OnIsWebUserOperationCompleted(object arg)
        {
            if ((this.IsWebUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsWebUserCompleted(this, new IsWebUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDbOwner", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetDbOwner(int companyId)
        {
            object[] results = this.Invoke("GetDbOwner", new object[] {
                        companyId});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetDbOwnerAsync(int companyId)
        {
            this.GetDbOwnerAsync(companyId, null);
        }

        /// <remarks/>
        public void GetDbOwnerAsync(int companyId, object userState)
        {
            if ((this.GetDbOwnerOperationCompleted == null))
            {
                this.GetDbOwnerOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDbOwnerOperationCompleted);
            }
            this.InvokeAsync("GetDbOwner", new object[] {
                        companyId}, this.GetDbOwnerOperationCompleted, userState);
        }

        private void OnGetDbOwnerOperationCompleted(object arg)
        {
            if ((this.GetDbOwnerCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDbOwnerCompleted(this, new GetDbOwnerCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsCompanySecured", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsCompanySecured(int companyId)
        {
            object[] results = this.Invoke("IsCompanySecured", new object[] {
                        companyId});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsCompanySecuredAsync(int companyId)
        {
            this.IsCompanySecuredAsync(companyId, null);
        }

        /// <remarks/>
        public void IsCompanySecuredAsync(int companyId, object userState)
        {
            if ((this.IsCompanySecuredOperationCompleted == null))
            {
                this.IsCompanySecuredOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsCompanySecuredOperationCompleted);
            }
            this.InvokeAsync("IsCompanySecured", new object[] {
                        companyId}, this.IsCompanySecuredOperationCompleted, userState);
        }

        private void OnIsCompanySecuredOperationCompleted(object arg)
        {
            if ((this.IsCompanySecuredCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsCompanySecuredCompleted(this, new IsCompanySecuredCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetAuthenticationInformations", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool webLogin)
        {
            object[] results = this.Invoke("GetAuthenticationInformations", new object[] {
                        authenticationToken});
            loginId = ((int)(results[1]));
            companyId = ((int)(results[2]));
            webLogin = ((bool)(results[3]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void GetAuthenticationInformationsAsync(string authenticationToken)
        {
            this.GetAuthenticationInformationsAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetAuthenticationInformationsAsync(string authenticationToken, object userState)
        {
            if ((this.GetAuthenticationInformationsOperationCompleted == null))
            {
                this.GetAuthenticationInformationsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAuthenticationInformationsOperationCompleted);
            }
            this.InvokeAsync("GetAuthenticationInformations", new object[] {
                        authenticationToken}, this.GetAuthenticationInformationsOperationCompleted, userState);
        }

        private void OnGetAuthenticationInformationsOperationCompleted(object arg)
        {
            if ((this.GetAuthenticationInformationsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetAuthenticationInformationsCompleted(this, new GetAuthenticationInformationsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetAuthenticationNames", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName)
        {
            object[] results = this.Invoke("GetAuthenticationNames", new object[] {
                        authenticationToken});
            userName = ((string)(results[1]));
            companyName = ((string)(results[2]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void GetAuthenticationNamesAsync(string authenticationToken)
        {
            this.GetAuthenticationNamesAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetAuthenticationNamesAsync(string authenticationToken, object userState)
        {
            if ((this.GetAuthenticationNamesOperationCompleted == null))
            {
                this.GetAuthenticationNamesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAuthenticationNamesOperationCompleted);
            }
            this.InvokeAsync("GetAuthenticationNames", new object[] {
                        authenticationToken}, this.GetAuthenticationNamesOperationCompleted, userState);
        }

        private void OnGetAuthenticationNamesOperationCompleted(object arg)
        {
            if ((this.GetAuthenticationNamesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetAuthenticationNamesCompleted(this, new GetAuthenticationNamesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteAssociation", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool DeleteAssociation(int loginId, int companyId, string authenticationToken)
        {
            object[] results = this.Invoke("DeleteAssociation", new object[] {
                        loginId,
                        companyId,
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void DeleteAssociationAsync(int loginId, int companyId, string authenticationToken)
        {
            this.DeleteAssociationAsync(loginId, companyId, authenticationToken, null);
        }

        /// <remarks/>
        public void DeleteAssociationAsync(int loginId, int companyId, string authenticationToken, object userState)
        {
            if ((this.DeleteAssociationOperationCompleted == null))
            {
                this.DeleteAssociationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteAssociationOperationCompleted);
            }
            this.InvokeAsync("DeleteAssociation", new object[] {
                        loginId,
                        companyId,
                        authenticationToken}, this.DeleteAssociationOperationCompleted, userState);
        }

        private void OnDeleteAssociationOperationCompleted(object arg)
        {
            if ((this.DeleteAssociationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteAssociationCompleted(this, new DeleteAssociationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool DeleteUser(int loginId, string authenticationToken)
        {
            object[] results = this.Invoke("DeleteUser", new object[] {
                        loginId,
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void DeleteUserAsync(int loginId, string authenticationToken)
        {
            this.DeleteUserAsync(loginId, authenticationToken, null);
        }

        /// <remarks/>
        public void DeleteUserAsync(int loginId, string authenticationToken, object userState)
        {
            if ((this.DeleteUserOperationCompleted == null))
            {
                this.DeleteUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteUserOperationCompleted);
            }
            this.InvokeAsync("DeleteUser", new object[] {
                        loginId,
                        authenticationToken}, this.DeleteUserOperationCompleted, userState);
        }

        private void OnDeleteUserOperationCompleted(object arg)
        {
            if ((this.DeleteUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteUserCompleted(this, new DeleteUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteCompany", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool DeleteCompany(int companyId, string authenticationToken)
        {
            object[] results = this.Invoke("DeleteCompany", new object[] {
                        companyId,
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void DeleteCompanyAsync(int companyId, string authenticationToken)
        {
            this.DeleteCompanyAsync(companyId, authenticationToken, null);
        }

        /// <remarks/>
        public void DeleteCompanyAsync(int companyId, string authenticationToken, object userState)
        {
            if ((this.DeleteCompanyOperationCompleted == null))
            {
                this.DeleteCompanyOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteCompanyOperationCompleted);
            }
            this.InvokeAsync("DeleteCompany", new object[] {
                        companyId,
                        authenticationToken}, this.DeleteCompanyOperationCompleted, userState);
        }

        private void OnDeleteCompanyOperationCompleted(object arg)
        {
            if ((this.DeleteCompanyCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteCompanyCompleted(this, new DeleteCompanyCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetSystemDBConnectionString", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetSystemDBConnectionString(string authenticationToken)
        {
            object[] results = this.Invoke("GetSystemDBConnectionString", new object[] {
                        authenticationToken});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetSystemDBConnectionStringAsync(string authenticationToken)
        {
            this.GetSystemDBConnectionStringAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetSystemDBConnectionStringAsync(string authenticationToken, object userState)
        {
            if ((this.GetSystemDBConnectionStringOperationCompleted == null))
            {
                this.GetSystemDBConnectionStringOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetSystemDBConnectionStringOperationCompleted);
            }
            this.InvokeAsync("GetSystemDBConnectionString", new object[] {
                        authenticationToken}, this.GetSystemDBConnectionStringOperationCompleted, userState);
        }

        private void OnGetSystemDBConnectionStringOperationCompleted(object arg)
        {
            if ((this.GetSystemDBConnectionStringCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetSystemDBConnectionStringCompleted(this, new GetSystemDBConnectionStringCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDMSConnectionString", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetDMSConnectionString(string authenticationToken)
        {
            object[] results = this.Invoke("GetDMSConnectionString", new object[] {
                        authenticationToken});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetDMSConnectionStringAsync(string authenticationToken)
        {
            this.GetDMSConnectionStringAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetDMSConnectionStringAsync(string authenticationToken, object userState)
        {
            if ((this.GetDMSConnectionStringOperationCompleted == null))
            {
                this.GetDMSConnectionStringOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDMSConnectionStringOperationCompleted);
            }
            this.InvokeAsync("GetDMSConnectionString", new object[] {
                        authenticationToken}, this.GetDMSConnectionStringOperationCompleted, userState);
        }

        private void OnGetDMSConnectionStringOperationCompleted(object arg)
        {
            if ((this.GetDMSConnectionStringCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDMSConnectionStringCompleted(this, new GetDMSConnectionStringCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDMSDatabasesInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        //public DmsDatabaseInfo[] GetDMSDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetDMSDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((DmsDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetDMSDatabasesInfoAsync(string authenticationToken)
        {
            this.GetDMSDatabasesInfoAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetDMSDatabasesInfoAsync(string authenticationToken, object userState)
        {
            if ((this.GetDMSDatabasesInfoOperationCompleted == null))
            {
                this.GetDMSDatabasesInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDMSDatabasesInfoOperationCompleted);
            }
            this.InvokeAsync("GetDMSDatabasesInfo", new object[] {
                        authenticationToken}, this.GetDMSDatabasesInfoOperationCompleted, userState);
        }

        private void OnGetDMSDatabasesInfoOperationCompleted(object arg)
        {
            if ((this.GetDMSDatabasesInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDMSDatabasesInfoCompleted(this, new GetDMSDatabasesInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDataSynchroDatabasesInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        //public DataSynchroDatabaseInfo[] GetDataSynchroDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetDataSynchroDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((DataSynchroDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetDataSynchroDatabasesInfoAsync(string authenticationToken)
        {
            this.GetDataSynchroDatabasesInfoAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetDataSynchroDatabasesInfoAsync(string authenticationToken, object userState)
        {
            if ((this.GetDataSynchroDatabasesInfoOperationCompleted == null))
            {
                this.GetDataSynchroDatabasesInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDataSynchroDatabasesInfoOperationCompleted);
            }
            this.InvokeAsync("GetDataSynchroDatabasesInfo", new object[] {
                        authenticationToken}, this.GetDataSynchroDatabasesInfoOperationCompleted, userState);
        }

        private void OnGetDataSynchroDatabasesInfoOperationCompleted(object arg)
        {
            if ((this.GetDataSynchroDatabasesInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDataSynchroDatabasesInfoCompleted(this, new GetDataSynchroDatabasesInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCompanyDatabasesInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        //public TbSenderDatabaseInfo[] GetCompanyDatabasesInfo(string authenticationToken)
        //{
        //    object[] results = this.Invoke("GetCompanyDatabasesInfo", new object[] {
        //                authenticationToken});
        //    return ((TbSenderDatabaseInfo[])(results[0]));
        //}

        /// <remarks/>
        public void GetCompanyDatabasesInfoAsync(string authenticationToken)
        {
            this.GetCompanyDatabasesInfoAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetCompanyDatabasesInfoAsync(string authenticationToken, object userState)
        {
            if ((this.GetCompanyDatabasesInfoOperationCompleted == null))
            {
                this.GetCompanyDatabasesInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCompanyDatabasesInfoOperationCompleted);
            }
            this.InvokeAsync("GetCompanyDatabasesInfo", new object[] {
                        authenticationToken}, this.GetCompanyDatabasesInfoOperationCompleted, userState);
        }

        private void OnGetCompanyDatabasesInfoOperationCompleted(object arg)
        {
            if ((this.GetCompanyDatabasesInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCompanyDatabasesInfoCompleted(this, new GetCompanyDatabasesInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetEdition", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetEdition()
        {
            object[] results = this.Invoke("GetEdition", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetEditionAsync()
        {
            this.GetEditionAsync(null);
        }

        /// <remarks/>
        public void GetEditionAsync(object userState)
        {
            if ((this.GetEditionOperationCompleted == null))
            {
                this.GetEditionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetEditionOperationCompleted);
            }
            this.InvokeAsync("GetEdition", new object[0], this.GetEditionOperationCompleted, userState);
        }

        private void OnGetEditionOperationCompleted(object arg)
        {
            if ((this.GetEditionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetEditionCompleted(this, new GetEditionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetConfigurationStream", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] GetConfigurationStream()
        {
            object[] results = this.Invoke("GetConfigurationStream", new object[0]);
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public void GetConfigurationStreamAsync()
        {
            this.GetConfigurationStreamAsync(null);
        }

        /// <remarks/>
        public void GetConfigurationStreamAsync(object userState)
        {
            if ((this.GetConfigurationStreamOperationCompleted == null))
            {
                this.GetConfigurationStreamOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetConfigurationStreamOperationCompleted);
            }
            this.InvokeAsync("GetConfigurationStream", new object[0], this.GetConfigurationStreamOperationCompleted, userState);
        }

        private void OnGetConfigurationStreamOperationCompleted(object arg)
        {
            if ((this.GetConfigurationStreamCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetConfigurationStreamCompleted(this, new GetConfigurationStreamCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCountry", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetCountry()
        {
            object[] results = this.Invoke("GetCountry", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetCountryAsync()
        {
            this.GetCountryAsync(null);
        }

        /// <remarks/>
        public void GetCountryAsync(object userState)
        {
            if ((this.GetCountryOperationCompleted == null))
            {
                this.GetCountryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCountryOperationCompleted);
            }
            this.InvokeAsync("GetCountry", new object[0], this.GetCountryOperationCompleted, userState);
        }

        private void OnGetCountryOperationCompleted(object arg)
        {
            if ((this.GetCountryCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCountryCompleted(this, new GetCountryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetProviderNameFromCompanyId", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetProviderNameFromCompanyId(int companyId)
        {
            object[] results = this.Invoke("GetProviderNameFromCompanyId", new object[] {
                        companyId});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetProviderNameFromCompanyIdAsync(int companyId)
        {
            this.GetProviderNameFromCompanyIdAsync(companyId, null);
        }

        /// <remarks/>
        public void GetProviderNameFromCompanyIdAsync(int companyId, object userState)
        {
            if ((this.GetProviderNameFromCompanyIdOperationCompleted == null))
            {
                this.GetProviderNameFromCompanyIdOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetProviderNameFromCompanyIdOperationCompleted);
            }
            this.InvokeAsync("GetProviderNameFromCompanyId", new object[] {
                        companyId}, this.GetProviderNameFromCompanyIdOperationCompleted, userState);
        }

        private void OnGetProviderNameFromCompanyIdOperationCompleted(object arg)
        {
            if ((this.GetProviderNameFromCompanyIdCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetProviderNameFromCompanyIdCompleted(this, new GetProviderNameFromCompanyIdCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetInstallationVersion", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetInstallationVersion(out string productName, out System.DateTime buildDate, out System.DateTime instDate, out int build)
        {
            object[] results = this.Invoke("GetInstallationVersion", new object[0]);
            productName = ((string)(results[1]));
            buildDate = ((System.DateTime)(results[2]));
            instDate = ((System.DateTime)(results[3]));
            build = ((int)(results[4]));
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetInstallationVersionAsync()
        {
            this.GetInstallationVersionAsync(null);
        }

        /// <remarks/>
        public void GetInstallationVersionAsync(object userState)
        {
            if ((this.GetInstallationVersionOperationCompleted == null))
            {
                this.GetInstallationVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetInstallationVersionOperationCompleted);
            }
            this.InvokeAsync("GetInstallationVersion", new object[0], this.GetInstallationVersionOperationCompleted, userState);
        }

        private void OnGetInstallationVersionOperationCompleted(object arg)
        {
            if ((this.GetInstallationVersionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetInstallationVersionCompleted(this, new GetInstallationVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserInfo()
        {
            object[] results = this.Invoke("GetUserInfo", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserInfoAsync()
        {
            this.GetUserInfoAsync(null);
        }

        /// <remarks/>
        public void GetUserInfoAsync(object userState)
        {
            if ((this.GetUserInfoOperationCompleted == null))
            {
                this.GetUserInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserInfoOperationCompleted);
            }
            this.InvokeAsync("GetUserInfo", new object[0], this.GetUserInfoOperationCompleted, userState);
        }

        private void OnGetUserInfoOperationCompleted(object arg)
        {
            if ((this.GetUserInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserInfoCompleted(this, new GetUserInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetUserInfoID", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserInfoID()
        {
            object[] results = this.Invoke("GetUserInfoID", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetUserInfoIDAsync()
        {
            this.GetUserInfoIDAsync(null);
        }

        /// <remarks/>
        public void GetUserInfoIDAsync(object userState)
        {
            if ((this.GetUserInfoIDOperationCompleted == null))
            {
                this.GetUserInfoIDOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserInfoIDOperationCompleted);
            }
            this.InvokeAsync("GetUserInfoID", new object[0], this.GetUserInfoIDOperationCompleted, userState);
        }

        private void OnGetUserInfoIDOperationCompleted(object arg)
        {
            if ((this.GetUserInfoIDCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserInfoIDCompleted(this, new GetUserInfoIDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/TraceAction", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void TraceAction(string company, string login, int type, string processName, string winUser, string location)
        {
            this.Invoke("TraceAction", new object[] {
                        company,
                        login,
                        type,
                        processName,
                        winUser,
                        location});
        }

        /// <remarks/>
        public void TraceActionAsync(string company, string login, int type, string processName, string winUser, string location)
        {
            this.TraceActionAsync(company, login, type, processName, winUser, location, null);
        }

        /// <remarks/>
        public void TraceActionAsync(string company, string login, int type, string processName, string winUser, string location, object userState)
        {
            if ((this.TraceActionOperationCompleted == null))
            {
                this.TraceActionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTraceActionOperationCompleted);
            }
            this.InvokeAsync("TraceAction", new object[] {
                        company,
                        login,
                        type,
                        processName,
                        winUser,
                        location}, this.TraceActionOperationCompleted, userState);
        }

        private void OnTraceActionOperationCompleted(object arg)
        {
            if ((this.TraceActionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.TraceActionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Sql2012Allowed", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool Sql2012Allowed(string authToken)
        {
            object[] results = this.Invoke("Sql2012Allowed", new object[] {
                        authToken});
            return ((bool)(results[0]));
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/HasUserAlreadyChangedPasswordToday", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool HasUserAlreadyChangedPasswordToday(string user)
        {
            object[] results = this.Invoke("HasUserAlreadyChangedPasswordToday", new object[] {
                        user});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void HasUserAlreadyChangedPasswordTodayAsync(string user)
        {
            this.HasUserAlreadyChangedPasswordTodayAsync(user, null);
        }

        /// <remarks/>
        public void HasUserAlreadyChangedPasswordTodayAsync(string user, object userState)
        {
            if ((this.HasUserAlreadyChangedPasswordTodayOperationCompleted == null))
            {
                this.HasUserAlreadyChangedPasswordTodayOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHasUserAlreadyChangedPasswordTodayOperationCompleted);
            }
            this.InvokeAsync("HasUserAlreadyChangedPasswordToday", new object[] {
                        user}, this.HasUserAlreadyChangedPasswordTodayOperationCompleted, userState);
        }

        private void OnHasUserAlreadyChangedPasswordTodayOperationCompleted(object arg)
        {
            if ((this.HasUserAlreadyChangedPasswordTodayCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.HasUserAlreadyChangedPasswordTodayCompleted(this, new HasUserAlreadyChangedPasswordTodayCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetBrandedApplicationTitle", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetBrandedApplicationTitle(string application)
        {
            object[] results = this.Invoke("GetBrandedApplicationTitle", new object[] {
                        application});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetBrandedApplicationTitleAsync(string application)
        {
            this.GetBrandedApplicationTitleAsync(application, null);
        }

        /// <remarks/>
        public void GetBrandedApplicationTitleAsync(string application, object userState)
        {
            if ((this.GetBrandedApplicationTitleOperationCompleted == null))
            {
                this.GetBrandedApplicationTitleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetBrandedApplicationTitleOperationCompleted);
            }
            this.InvokeAsync("GetBrandedApplicationTitle", new object[] {
                        application}, this.GetBrandedApplicationTitleOperationCompleted, userState);
        }

        private void OnGetBrandedApplicationTitleOperationCompleted(object arg)
        {
            if ((this.GetBrandedApplicationTitleCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetBrandedApplicationTitleCompleted(this, new GetBrandedApplicationTitleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMasterProductBrandedName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMasterProductBrandedName()
        {
            object[] results = this.Invoke("GetMasterProductBrandedName", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetMasterProductBrandedNameAsync()
        {
            this.GetMasterProductBrandedNameAsync(null);
        }

        /// <remarks/>
        public void GetMasterProductBrandedNameAsync(object userState)
        {
            if ((this.GetMasterProductBrandedNameOperationCompleted == null))
            {
                this.GetMasterProductBrandedNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMasterProductBrandedNameOperationCompleted);
            }
            this.InvokeAsync("GetMasterProductBrandedName", new object[0], this.GetMasterProductBrandedNameOperationCompleted, userState);
        }

        private void OnGetMasterProductBrandedNameOperationCompleted(object arg)
        {
            if ((this.GetMasterProductBrandedNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMasterProductBrandedNameCompleted(this, new GetMasterProductBrandedNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMasterSolution", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMasterSolution()
        {
            object[] results = this.Invoke("GetMasterSolution", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMasterSolutionBrandedName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMasterSolutionBrandedName()
        {
            object[] results = this.Invoke("GetMasterSolutionBrandedName", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetMasterSolutionBrandedNameAsync()
        {
            this.GetMasterSolutionBrandedNameAsync(null);
        }

        /// <remarks/>
        public void GetMasterSolutionBrandedNameAsync(object userState)
        {
            if ((this.GetMasterSolutionBrandedNameOperationCompleted == null))
            {
                this.GetMasterSolutionBrandedNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMasterSolutionBrandedNameOperationCompleted);
            }
            this.InvokeAsync("GetMasterSolutionBrandedName", new object[0], this.GetMasterSolutionBrandedNameOperationCompleted, userState);
        }

        private void OnGetMasterSolutionBrandedNameOperationCompleted(object arg)
        {
            if ((this.GetMasterSolutionBrandedNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMasterSolutionBrandedNameCompleted(this, new GetMasterSolutionBrandedNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetBrandedProducerName", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetBrandedProducerName()
        {
            object[] results = this.Invoke("GetBrandedProducerName", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetBrandedProducerNameAsync()
        {
            this.GetBrandedProducerNameAsync(null);
        }

        /// <remarks/>
        public void GetBrandedProducerNameAsync(object userState)
        {
            if ((this.GetBrandedProducerNameOperationCompleted == null))
            {
                this.GetBrandedProducerNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetBrandedProducerNameOperationCompleted);
            }
            this.InvokeAsync("GetBrandedProducerName", new object[0], this.GetBrandedProducerNameOperationCompleted, userState);
        }

        private void OnGetBrandedProducerNameOperationCompleted(object arg)
        {
            if ((this.GetBrandedProducerNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetBrandedProducerNameCompleted(this, new GetBrandedProducerNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetBrandedProductTitle", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetBrandedProductTitle()
        {
            object[] results = this.Invoke("GetBrandedProductTitle", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetBrandedProductTitleAsync()
        {
            this.GetBrandedProductTitleAsync(null);
        }

        /// <remarks/>
        public void GetBrandedProductTitleAsync(object userState)
        {
            if ((this.GetBrandedProductTitleOperationCompleted == null))
            {
                this.GetBrandedProductTitleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetBrandedProductTitleOperationCompleted);
            }
            this.InvokeAsync("GetBrandedProductTitle", new object[0], this.GetBrandedProductTitleOperationCompleted, userState);
        }

        private void OnGetBrandedProductTitleOperationCompleted(object arg)
        {
            if ((this.GetBrandedProductTitleCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetBrandedProductTitleCompleted(this, new GetBrandedProductTitleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetBrandedKey", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetBrandedKey(string source)
        {
            object[] results = this.Invoke("GetBrandedKey", new object[] {
                        source});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetBrandedKeyAsync(string source)
        {
            this.GetBrandedKeyAsync(source, null);
        }

        /// <remarks/>
        public void GetBrandedKeyAsync(string source, object userState)
        {
            if ((this.GetBrandedKeyOperationCompleted == null))
            {
                this.GetBrandedKeyOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetBrandedKeyOperationCompleted);
            }
            this.InvokeAsync("GetBrandedKey", new object[] {
                        source}, this.GetBrandedKeyOperationCompleted, userState);
        }

        private void OnGetBrandedKeyOperationCompleted(object arg)
        {
            if ((this.GetBrandedKeyCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetBrandedKeyCompleted(this, new GetBrandedKeyCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetEditionType", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetEditionType()
        {
            object[] results = this.Invoke("GetEditionType", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDBNetworkType", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object GetDBNetworkType()
        {
            object[] results = this.Invoke("GetDBNetworkType", new object[0]);
            return ((Object)(results[0]));
        }

        /// <remarks/>
        public void GetDBNetworkTypeAsync()
        {
            this.GetDBNetworkTypeAsync(null);
        }

        /// <remarks/>
        public void GetDBNetworkTypeAsync(object userState)
        {
            if ((this.GetDBNetworkTypeOperationCompleted == null))
            {
                this.GetDBNetworkTypeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDBNetworkTypeOperationCompleted);
            }
            this.InvokeAsync("GetDBNetworkType", new object[0], this.GetDBNetworkTypeOperationCompleted, userState);
        }

        private void OnGetDBNetworkTypeOperationCompleted(object arg)
        {
            if ((this.GetDBNetworkTypeCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDBNetworkTypeCompleted(this, new GetDBNetworkTypeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDatabaseType", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetDatabaseType(string providerName)
        {
            object[] results = this.Invoke("GetDatabaseType", new object[] {
                        providerName});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetDatabaseTypeAsync(string providerName)
        {
            this.GetDatabaseTypeAsync(providerName, null);
        }

        /// <remarks/>
        public void GetDatabaseTypeAsync(string providerName, object userState)
        {
            if ((this.GetDatabaseTypeOperationCompleted == null))
            {
                this.GetDatabaseTypeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDatabaseTypeOperationCompleted);
            }
            this.InvokeAsync("GetDatabaseType", new object[] {
                        providerName}, this.GetDatabaseTypeOperationCompleted, userState);
        }

        private void OnGetDatabaseTypeOperationCompleted(object arg)
        {
            if ((this.GetDatabaseTypeCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDatabaseTypeCompleted(this, new GetDatabaseTypeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/CanUseNamespace", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool CanUseNamespace(string nameSpace, string authenticationToken, GrantType grantType)
        {
            object[] results = this.Invoke("CanUseNamespace", new object[] {
                        nameSpace,
                        authenticationToken,
                        grantType});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void CanUseNamespaceAsync(string nameSpace, string authenticationToken, GrantType grantType)
        {
            this.CanUseNamespaceAsync(nameSpace, authenticationToken, grantType, null);
        }

        /// <remarks/>
        public void CanUseNamespaceAsync(string nameSpace, string authenticationToken, GrantType grantType, object userState)
        {
            if ((this.CanUseNamespaceOperationCompleted == null))
            {
                this.CanUseNamespaceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCanUseNamespaceOperationCompleted);
            }
            this.InvokeAsync("CanUseNamespace", new object[] {
                        nameSpace,
                        authenticationToken,
                        grantType}, this.CanUseNamespaceOperationCompleted, userState);
        }

        private void OnCanUseNamespaceOperationCompleted(object arg)
        {
            if ((this.CanUseNamespaceCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CanUseNamespaceCompleted(this, new CanUseNamespaceCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/CacheCounter", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool CacheCounter()
        {
            object[] results = this.Invoke("CacheCounter", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void CacheCounterAsync()
        {
            this.CacheCounterAsync(null);
        }

        /// <remarks/>
        public void CacheCounterAsync(object userState)
        {
            if ((this.CacheCounterOperationCompleted == null))
            {
                this.CacheCounterOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCacheCounterOperationCompleted);
            }
            this.InvokeAsync("CacheCounter", new object[0], this.CacheCounterOperationCompleted, userState);
        }

        private void OnCacheCounterOperationCompleted(object arg)
        {
            if ((this.CacheCounterCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CacheCounterCompleted(this, new CacheCounterCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/CacheCounterGTG", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object CacheCounterGTG()
        {
            object[] results = this.Invoke("CacheCounterGTG", new object[0]);
            return ((Object)(results[0]));
        }

        /// <remarks/>
        public void CacheCounterGTGAsync()
        {
            this.CacheCounterGTGAsync(null);
        }

        /// <remarks/>
        public void CacheCounterGTGAsync(object userState)
        {
            if ((this.CacheCounterGTGOperationCompleted == null))
            {
                this.CacheCounterGTGOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCacheCounterGTGOperationCompleted);
            }
            this.InvokeAsync("CacheCounterGTG", new object[0], this.CacheCounterGTGOperationCompleted, userState);
        }

        private void OnCacheCounterGTGOperationCompleted(object arg)
        {
            if ((this.CacheCounterGTGCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CacheCounterGTGCompleted(this, new CacheCounterGTGCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SetCurrentComponents", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object SetCurrentComponents(out int dte)
        {
            object[] results = this.Invoke("SetCurrentComponents", new object[0]);
            dte = ((int)(results[1]));
            return ((Object)(results[0]));
        }

        /// <remarks/>
        public void SetCurrentComponentsAsync()
        {
            this.SetCurrentComponentsAsync(null);
        }

        /// <remarks/>
        public void SetCurrentComponentsAsync(object userState)
        {
            if ((this.SetCurrentComponentsOperationCompleted == null))
            {
                this.SetCurrentComponentsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetCurrentComponentsOperationCompleted);
            }
            this.InvokeAsync("SetCurrentComponents", new object[0], this.SetCurrentComponentsOperationCompleted, userState);
        }

        private void OnSetCurrentComponentsOperationCompleted(object arg)
        {
            if ((this.SetCurrentComponentsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetCurrentComponentsCompleted(this, new SetCurrentComponentsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsVirginActivation", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsVirginActivation()
        {
            object[] results = this.Invoke("IsVirginActivation", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsVirginActivationAsync()
        {
            this.IsVirginActivationAsync(null);
        }

        /// <remarks/>
        public void IsVirginActivationAsync(object userState)
        {
            if ((this.IsVirginActivationOperationCompleted == null))
            {
                this.IsVirginActivationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsVirginActivationOperationCompleted);
            }
            this.InvokeAsync("IsVirginActivation", new object[0], this.IsVirginActivationOperationCompleted, userState);
        }

        private void OnIsVirginActivationOperationCompleted(object arg)
        {
            if ((this.IsVirginActivationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsVirginActivationCompleted(this, new IsVirginActivationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/HD", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int HD()
        {
            object[] results = this.Invoke("HD", new object[0]);
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void HDAsync()
        {
            this.HDAsync(null);
        }

        /// <remarks/>
        public void HDAsync(object userState)
        {
            if ((this.HDOperationCompleted == null))
            {
                this.HDOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHDOperationCompleted);
            }
            this.InvokeAsync("HD", new object[0], this.HDOperationCompleted, userState);
        }

        private void OnHDOperationCompleted(object arg)
        {
            if ((this.HDCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.HDCompleted(this, new HDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/StoreMLUChoice", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void StoreMLUChoice(bool userChoseMluInChargeToMicroarea)
        {
            this.Invoke("StoreMLUChoice", new object[] {
                        userChoseMluInChargeToMicroarea});
        }

        /// <remarks/>
        public void StoreMLUChoiceAsync(bool userChoseMluInChargeToMicroarea)
        {
            this.StoreMLUChoiceAsync(userChoseMluInChargeToMicroarea, null);
        }

        /// <remarks/>
        public void StoreMLUChoiceAsync(bool userChoseMluInChargeToMicroarea, object userState)
        {
            if ((this.StoreMLUChoiceOperationCompleted == null))
            {
                this.StoreMLUChoiceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStoreMLUChoiceOperationCompleted);
            }
            this.InvokeAsync("StoreMLUChoice", new object[] {
                        userChoseMluInChargeToMicroarea}, this.StoreMLUChoiceOperationCompleted, userState);
        }

        private void OnStoreMLUChoiceOperationCompleted(object arg)
        {
            if ((this.StoreMLUChoiceCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StoreMLUChoiceCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SaveLicensed", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SaveLicensed(string xml, string name)
        {
            object[] results = this.Invoke("SaveLicensed", new object[] {
                        xml,
                        name});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SaveLicensedAsync(string xml, string name)
        {
            this.SaveLicensedAsync(xml, name, null);
        }

        /// <remarks/>
        public void SaveLicensedAsync(string xml, string name, object userState)
        {
            if ((this.SaveLicensedOperationCompleted == null))
            {
                this.SaveLicensedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSaveLicensedOperationCompleted);
            }
            this.InvokeAsync("SaveLicensed", new object[] {
                        xml,
                        name}, this.SaveLicensedOperationCompleted, userState);
        }

        private void OnSaveLicensedOperationCompleted(object arg)
        {
            if ((this.SaveLicensedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SaveLicensedCompleted(this, new SaveLicensedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ValidateIToken", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string ValidateIToken(string itoken, string authenticationToken)
        {
            object[] results = this.Invoke("ValidateIToken", new object[] { itoken, authenticationToken });
            return ((string)(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SaveUserInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SaveUserInfo(string xml)
        {
            object[] results = this.Invoke("SaveUserInfo", new object[] {
                        xml});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SaveUserInfoAsync(string xml)
        {
            this.SaveUserInfoAsync(xml, null);
        }

        /// <remarks/>
        public void SaveUserInfoAsync(string xml, object userState)
        {
            if ((this.SaveUserInfoOperationCompleted == null))
            {
                this.SaveUserInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSaveUserInfoOperationCompleted);
            }
            this.InvokeAsync("SaveUserInfo", new object[] {
                        xml}, this.SaveUserInfoOperationCompleted, userState);
        }

        private void OnSaveUserInfoOperationCompleted(object arg)
        {
            if ((this.SaveUserInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SaveUserInfoCompleted(this, new SaveUserInfoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteUserInfo", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void DeleteUserInfo()
        {
            this.Invoke("DeleteUserInfo", new object[0]);
        }

        /// <remarks/>
        public void DeleteUserInfoAsync()
        {
            this.DeleteUserInfoAsync(null);
        }

        /// <remarks/>
        public void DeleteUserInfoAsync(object userState)
        {
            if ((this.DeleteUserInfoOperationCompleted == null))
            {
                this.DeleteUserInfoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteUserInfoOperationCompleted);
            }
            this.InvokeAsync("DeleteUserInfo", new object[0], this.DeleteUserInfoOperationCompleted, userState);
        }

        private void OnDeleteUserInfoOperationCompleted(object arg)
        {
            if ((this.DeleteUserInfoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteUserInfoCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteLicensed", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void DeleteLicensed(string name)
        {
            this.Invoke("DeleteLicensed", new object[] {
                        name});
        }

        /// <remarks/>
        public void DeleteLicensedAsync(string name)
        {
            this.DeleteLicensedAsync(name, null);
        }

        /// <remarks/>
        public void DeleteLicensedAsync(string name, object userState)
        {
            if ((this.DeleteLicensedOperationCompleted == null))
            {
                this.DeleteLicensedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteLicensedOperationCompleted);
            }
            this.InvokeAsync("DeleteLicensed", new object[] {
                        name}, this.DeleteLicensedOperationCompleted, userState);
        }

        private void OnDeleteLicensedOperationCompleted(object arg)
        {
            if ((this.DeleteLicensedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteLicensedCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/PrePing", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string PrePing()
        {
            object[] results = this.Invoke("PrePing", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void PrePingAsync()
        {
            this.PrePingAsync(null);
        }

        /// <remarks/>
        public void PrePingAsync(object userState)
        {
            if ((this.PrePingOperationCompleted == null))
            {
                this.PrePingOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPrePingOperationCompleted);
            }
            this.InvokeAsync("PrePing", new object[0], this.PrePingOperationCompleted, userState);
        }

        private void OnPrePingOperationCompleted(object arg)
        {
            if ((this.PrePingCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PrePingCompleted(this, new PrePingCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Ping", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string Ping()
        {
            object[] results = this.Invoke("Ping", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void PingAsync()
        {
            this.PingAsync(null);
        }

        /// <remarks/>
        public void PingAsync(object userState)
        {
            if ((this.PingOperationCompleted == null))
            {
                this.PingOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPingOperationCompleted);
            }
            this.InvokeAsync("Ping", new object[0], this.PingOperationCompleted, userState);
        }

        private void OnPingOperationCompleted(object arg)
        {
            if ((this.PingCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PingCompleted(this, new PingCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetArticlesWithFloatingCal", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ModuleNameInfo[] GetArticlesWithFloatingCal()
        {
            object[] results = this.Invoke("GetArticlesWithFloatingCal", new object[0]);
            return ((ModuleNameInfo[])(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/RefreshFloatingMark", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void RefreshFloatingMark()
        {
            this.Invoke("RefreshFloatingMark", new object[0]);

        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetArticlesWithNamedCal", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ModuleNameInfo[] GetArticlesWithNamedCal()
        {
            object[] results = this.Invoke("GetArticlesWithNamedCal", new object[0]);
            return ((ModuleNameInfo[])(results[0]));
        }

        /// <remarks/>
        public void GetArticlesWithNamedCalAsync()
        {
            this.GetArticlesWithNamedCalAsync(null);
        }

        /// <remarks/>
        public void GetArticlesWithNamedCalAsync(object userState)
        {
            if ((this.GetArticlesWithNamedCalOperationCompleted == null))
            {
                this.GetArticlesWithNamedCalOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetArticlesWithNamedCalOperationCompleted);
            }
            this.InvokeAsync("GetArticlesWithNamedCal", new object[0], this.GetArticlesWithNamedCalOperationCompleted, userState);
        }

        private void OnGetArticlesWithNamedCalOperationCompleted(object arg)
        {
            if ((this.GetArticlesWithNamedCalCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetArticlesWithNamedCalCompleted(this, new GetArticlesWithNamedCalCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/RefreshSecurityStatus", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void RefreshSecurityStatus()
        {
            this.Invoke("RefreshSecurityStatus", new object[0]);
        }

        /// <remarks/>
        public void RefreshSecurityStatusAsync()
        {
            this.RefreshSecurityStatusAsync(null);
        }

        /// <remarks/>
        public void RefreshSecurityStatusAsync(object userState)
        {
            if ((this.RefreshSecurityStatusOperationCompleted == null))
            {
                this.RefreshSecurityStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRefreshSecurityStatusOperationCompleted);
            }
            this.InvokeAsync("RefreshSecurityStatus", new object[0], this.RefreshSecurityStatusOperationCompleted, userState);
        }

        private void OnRefreshSecurityStatusOperationCompleted(object arg)
        {
            if ((this.RefreshSecurityStatusCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RefreshSecurityStatusCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetProxySupportVersion", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetProxySupportVersion()
        {
            object[] results = this.Invoke("GetProxySupportVersion", new object[0]);
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetProxySupportVersionAsync()
        {
            this.GetProxySupportVersionAsync(null);
        }

        /// <remarks/>
        public void GetProxySupportVersionAsync(object userState)
        {
            if ((this.GetProxySupportVersionOperationCompleted == null))
            {
                this.GetProxySupportVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetProxySupportVersionOperationCompleted);
            }
            this.InvokeAsync("GetProxySupportVersion", new object[0], this.GetProxySupportVersionOperationCompleted, userState);
        }

        private void OnGetProxySupportVersionOperationCompleted(object arg)
        {
            if ((this.GetProxySupportVersionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetProxySupportVersionCompleted(this, new GetProxySupportVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetProxySettings", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ProxySettings GetProxySettings()
        {
            object[] results = this.Invoke("GetProxySettings", new object[0]);
            return ((ProxySettings)(results[0]));
        }

        /// <remarks/>
        public void GetProxySettingsAsync()
        {
            this.GetProxySettingsAsync(null);
        }

        /// <remarks/>
        public void GetProxySettingsAsync(object userState)
        {
            if ((this.GetProxySettingsOperationCompleted == null))
            {
                this.GetProxySettingsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetProxySettingsOperationCompleted);
            }
            this.InvokeAsync("GetProxySettings", new object[0], this.GetProxySettingsOperationCompleted, userState);
        }

        private void OnGetProxySettingsOperationCompleted(object arg)
        {
            if ((this.GetProxySettingsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetProxySettingsCompleted(this, new GetProxySettingsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SetProxySettings", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SetProxySettings(Object proxySettings)
        {
            this.Invoke("SetProxySettings", new object[] {
                        proxySettings});
        }

        /// <remarks/>
        public void SetProxySettingsAsync(Object proxySettings)
        {
            this.SetProxySettingsAsync(proxySettings, null);
        }

        /// <remarks/>
        public void SetProxySettingsAsync(Object proxySettings, object userState)
        {
            if ((this.SetProxySettingsOperationCompleted == null))
            {
                this.SetProxySettingsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetProxySettingsOperationCompleted);
            }
            this.InvokeAsync("SetProxySettings", new object[] {
                        proxySettings}, this.SetProxySettingsOperationCompleted, userState);
        }

        private void OnSetProxySettingsOperationCompleted(object arg)
        {
            if ((this.SetProxySettingsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetProxySettingsCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCompanyLanguage", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool GetCompanyLanguage(int companyID, out string cultureUI, out string culture)
        {
            object[] results = this.Invoke("GetCompanyLanguage", new object[] {
                        companyID});
            cultureUI = ((string)(results[1]));
            culture = ((string)(results[2]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void GetCompanyLanguageAsync(int companyID)
        {
            this.GetCompanyLanguageAsync(companyID, null);
        }

        /// <remarks/>
        public void GetCompanyLanguageAsync(int companyID, object userState)
        {
            if ((this.GetCompanyLanguageOperationCompleted == null))
            {
                this.GetCompanyLanguageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCompanyLanguageOperationCompleted);
            }
            this.InvokeAsync("GetCompanyLanguage", new object[] {
                        companyID}, this.GetCompanyLanguageOperationCompleted, userState);
        }

        private void OnGetCompanyLanguageOperationCompleted(object arg)
        {
            if ((this.GetCompanyLanguageCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCompanyLanguageCompleted(this, new GetCompanyLanguageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }


        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/VerifyDBSize", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool VerifyDBSize()
        {
            object[] results = this.Invoke("VerifyDBSize", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsValidToken", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsValidToken(string authenticationToken)
        {
            object[] results = this.Invoke("IsValidToken", new object[] {
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsValidTokenAsync(string authenticationToken)
        {
            this.IsValidTokenAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void IsValidTokenAsync(string authenticationToken, object userState)
        {
            if ((this.IsValidTokenOperationCompleted == null))
            {
                this.IsValidTokenOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsValidTokenOperationCompleted);
            }
            this.InvokeAsync("IsValidToken", new object[] {
                        authenticationToken}, this.IsValidTokenOperationCompleted, userState);
        }

        private void OnIsValidTokenOperationCompleted(object arg)
        {
            if ((this.IsValidTokenCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsValidTokenCompleted(this, new IsValidTokenCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/ReloadUserArticleBindings", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void ReloadUserArticleBindings(string authenticationToken)
        {
            this.Invoke("ReloadUserArticleBindings", new object[] {
                        authenticationToken});
        }

        /// <remarks/>
        public void ReloadUserArticleBindingsAsync(string authenticationToken)
        {
            this.ReloadUserArticleBindingsAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void ReloadUserArticleBindingsAsync(string authenticationToken, object userState)
        {
            if ((this.ReloadUserArticleBindingsOperationCompleted == null))
            {
                this.ReloadUserArticleBindingsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnReloadUserArticleBindingsOperationCompleted);
            }
            this.InvokeAsync("ReloadUserArticleBindings", new object[] {
                        authenticationToken}, this.ReloadUserArticleBindingsOperationCompleted, userState);
        }

        private void OnReloadUserArticleBindingsOperationCompleted(object arg)
        {
            if ((this.ReloadUserArticleBindingsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ReloadUserArticleBindingsCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/Sbrill", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool Sbrill(string token)
        {
            object[] results = this.Invoke("Sbrill", new object[] {
                        token});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SbrillAsync(string token)
        {
            this.SbrillAsync(token, null);
        }

        /// <remarks/>
        public void SbrillAsync(string token, object userState)
        {
            if ((this.SbrillOperationCompleted == null))
            {
                this.SbrillOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSbrillOperationCompleted);
            }
            this.InvokeAsync("Sbrill", new object[] {
                        token}, this.SbrillOperationCompleted, userState);
        }

        private void OnSbrillOperationCompleted(object arg)
        {
            if ((this.SbrillCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SbrillCompleted(this, new SbrillCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetCalType", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object GetCalType(string token)
        {
            object[] results = this.Invoke("GetCalType", new object[] {
                        token});
            return ((Object)(results[0]));
        }

        /// <remarks/>
        public void GetCalTypeAsync(string token)
        {
            this.GetCalTypeAsync(token, null);
        }

        /// <remarks/>
        public void GetCalTypeAsync(string token, object userState)
        {
            if ((this.GetCalTypeOperationCompleted == null))
            {
                this.GetCalTypeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCalTypeOperationCompleted);
            }
            this.InvokeAsync("GetCalType", new object[] {
                        token}, this.GetCalTypeOperationCompleted, userState);
        }

        private void OnGetCalTypeOperationCompleted(object arg)
        {
            if ((this.GetCalTypeCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCalTypeCompleted(this, new GetCalTypeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsUserLogged", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsUserLogged(int loginID)
        {
            object[] results = this.Invoke("IsUserLogged", new object[] {
                        loginID});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsUserLoggedAsync(int loginID)
        {
            this.IsUserLoggedAsync(loginID, null);
        }

        /// <remarks/>
        public void IsUserLoggedAsync(int loginID, object userState)
        {
            if ((this.IsUserLoggedOperationCompleted == null))
            {
                this.IsUserLoggedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsUserLoggedOperationCompleted);
            }
            this.InvokeAsync("IsUserLogged", new object[] {
                        loginID}, this.IsUserLoggedOperationCompleted, userState);
        }

        private void OnIsUserLoggedOperationCompleted(object arg)
        {
            if ((this.IsUserLoggedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsUserLoggedCompleted(this, new IsUserLoggedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsSecurityLightEnabled", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsSecurityLightEnabled()
        {
            object[] results = this.Invoke("IsSecurityLightEnabled", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsSecurityLightEnabledAsync()
        {
            this.IsSecurityLightEnabledAsync(null);
        }

        /// <remarks/>
        public void IsSecurityLightEnabledAsync(object userState)
        {
            if ((this.IsSecurityLightEnabledOperationCompleted == null))
            {
                this.IsSecurityLightEnabledOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsSecurityLightEnabledOperationCompleted);
            }
            this.InvokeAsync("IsSecurityLightEnabled", new object[0], this.IsSecurityLightEnabledOperationCompleted, userState);
        }

        private void OnIsSecurityLightEnabledOperationCompleted(object arg)
        {
            if ((this.IsSecurityLightEnabledCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsSecurityLightEnabledCompleted(this, new IsSecurityLightEnabledCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsSecurityLightAccessAllowed", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsSecurityLightAccessAllowed(string nameSpace, string authenticationToken, bool unattended)
        {
            object[] results = this.Invoke("IsSecurityLightAccessAllowed", new object[] {
                        nameSpace,
                        authenticationToken,
                        unattended});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsSecurityLightAccessAllowedAsync(string nameSpace, string authenticationToken, bool unattended)
        {
            this.IsSecurityLightAccessAllowedAsync(nameSpace, authenticationToken, unattended, null);
        }

        /// <remarks/>
        public void IsSecurityLightAccessAllowedAsync(string nameSpace, string authenticationToken, bool unattended, object userState)
        {
            if ((this.IsSecurityLightAccessAllowedOperationCompleted == null))
            {
                this.IsSecurityLightAccessAllowedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsSecurityLightAccessAllowedOperationCompleted);
            }
            this.InvokeAsync("IsSecurityLightAccessAllowed", new object[] {
                        nameSpace,
                        authenticationToken,
                        unattended}, this.IsSecurityLightAccessAllowedOperationCompleted, userState);
        }

        private void OnIsSecurityLightAccessAllowedOperationCompleted(object arg)
        {
            if ((this.IsSecurityLightAccessAllowedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsSecurityLightAccessAllowedCompleted(this, new IsSecurityLightAccessAllowedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetDBCultureLCID", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetDBCultureLCID(int companyID)
        {
            object[] results = this.Invoke("GetDBCultureLCID", new object[] {
                        companyID});
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void GetDBCultureLCIDAsync(int companyID)
        {
            this.GetDBCultureLCIDAsync(companyID, null);
        }

        /// <remarks/>
        public void GetDBCultureLCIDAsync(int companyID, object userState)
        {
            if ((this.GetDBCultureLCIDOperationCompleted == null))
            {
                this.GetDBCultureLCIDOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDBCultureLCIDOperationCompleted);
            }
            this.InvokeAsync("GetDBCultureLCID", new object[] {
                        companyID}, this.GetDBCultureLCIDOperationCompleted, userState);
        }

        private void OnGetDBCultureLCIDOperationCompleted(object arg)
        {
            if ((this.GetDBCultureLCIDCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDBCultureLCIDCompleted(this, new GetDBCultureLCIDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SetMessageRead", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SetMessageRead(string userName, string messageID)
        {
            this.Invoke("SetMessageRead", new object[] {
                        userName,
                        messageID});
        }

        /// <remarks/>
        public void SetMessageReadAsync(string userName, string messageID)
        {
            this.SetMessageReadAsync(userName, messageID, null);
        }

        /// <remarks/>
        public void SetMessageReadAsync(string userName, string messageID, object userState)
        {
            if ((this.SetMessageReadOperationCompleted == null))
            {
                this.SetMessageReadOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetMessageReadOperationCompleted);
            }
            this.InvokeAsync("SetMessageRead", new object[] {
                        userName,
                        messageID}, this.SetMessageReadOperationCompleted, userState);
        }

        private void OnSetMessageReadOperationCompleted(object arg)
        {
            if ((this.SetMessageReadCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetMessageReadCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetImmediateMessagesQueue", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object[] GetImmediateMessagesQueue(string authenticationToken)
        {
            object[] results = this.Invoke("GetImmediateMessagesQueue", new object[] {
                        authenticationToken});
            return ((Object[])(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMessagesQueue", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object[] GetMessagesQueue(string authenticationToken)
        {
            object[] results = this.Invoke("GetMessagesQueue", new object[] {
                        authenticationToken});
            return ((Object[])(results[0]));
        }

        /// <remarks/>
        public void GetMessagesQueueAsync(string authenticationToken)
        {
            this.GetMessagesQueueAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetMessagesQueueAsync(string authenticationToken, object userState)
        {
            if ((this.GetMessagesQueueOperationCompleted == null))
            {
                this.GetMessagesQueueOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMessagesQueueOperationCompleted);
            }
            this.InvokeAsync("GetMessagesQueue", new object[] {
                        authenticationToken}, this.GetMessagesQueueOperationCompleted, userState);
        }

        private void OnGetMessagesQueueOperationCompleted(object arg)
        {
            if ((this.GetMessagesQueueCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMessagesQueueCompleted(this, new GetMessagesQueueCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetOldMessages", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Object[] GetOldMessages(string authenticationToken)
        {
            object[] results = this.Invoke("GetOldMessages", new object[] {
                        authenticationToken});
            return ((Object[])(results[0]));
        }

        /// <remarks/>
        public void GetOldMessagesAsync(string authenticationToken)
        {
            this.GetOldMessagesAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetOldMessagesAsync(string authenticationToken, object userState)
        {
            if ((this.GetOldMessagesOperationCompleted == null))
            {
                this.GetOldMessagesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetOldMessagesOperationCompleted);
            }
            this.InvokeAsync("GetOldMessages", new object[] {
                        authenticationToken}, this.GetOldMessagesOperationCompleted, userState);
        }

        private void OnGetOldMessagesOperationCompleted(object arg)
        {
            if ((this.GetOldMessagesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetOldMessagesCompleted(this, new GetOldMessagesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SendAccessMail", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SendAccessMail()
        {
            object[] results = this.Invoke("SendAccessMail", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SendAccessMailAsync()
        {
            this.SendAccessMailAsync(null);
        }

        /// <remarks/>
        public void SendAccessMailAsync(object userState)
        {
            if ((this.SendAccessMailOperationCompleted == null))
            {
                this.SendAccessMailOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendAccessMailOperationCompleted);
            }
            this.InvokeAsync("SendAccessMail", new object[0], this.SendAccessMailOperationCompleted, userState);
        }

        private void OnSendAccessMailOperationCompleted(object arg)
        {
            if ((this.SendAccessMailCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendAccessMailCompleted(this, new SendAccessMailCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetAspNetUser", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetAspNetUser()
        {
            object[] results = this.Invoke("GetAspNetUser", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetAspNetUserAsync()
        {
            this.GetAspNetUserAsync(null);
        }

        /// <remarks/>
        public void GetAspNetUserAsync(object userState)
        {
            if ((this.GetAspNetUserOperationCompleted == null))
            {
                this.GetAspNetUserOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAspNetUserOperationCompleted);
            }
            this.InvokeAsync("GetAspNetUser", new object[0], this.GetAspNetUserOperationCompleted, userState);
        }

        private void OnGetAspNetUserOperationCompleted(object arg)
        {
            if ((this.GetAspNetUserCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetAspNetUserCompleted(this, new GetAspNetUserCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetConfigurationHash", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetConfigurationHash()
        {
            object[] results = this.Invoke("GetConfigurationHash", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetConfigurationHashAsync()
        {
            this.GetConfigurationHashAsync(null);
        }

        /// <remarks/>
        public void GetConfigurationHashAsync(object userState)
        {
            if ((this.GetConfigurationHashOperationCompleted == null))
            {
                this.GetConfigurationHashOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetConfigurationHashOperationCompleted);
            }
            this.InvokeAsync("GetConfigurationHash", new object[0], this.GetConfigurationHashOperationCompleted, userState);
        }

        private void OnGetConfigurationHashOperationCompleted(object arg)
        {
            if ((this.GetConfigurationHashCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetConfigurationHashCompleted(this, new GetConfigurationHashCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/UserCanAccessWebSitePrivateArea", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool UserCanAccessWebSitePrivateArea(int loginId)
        {
            object[] results = this.Invoke("UserCanAccessWebSitePrivateArea", new object[] {
                        loginId});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void UserCanAccessWebSitePrivateAreaAsync(int loginId)
        {
            this.UserCanAccessWebSitePrivateAreaAsync(loginId, null);
        }

        /// <remarks/>
        public void UserCanAccessWebSitePrivateAreaAsync(int loginId, object userState)
        {
            if ((this.UserCanAccessWebSitePrivateAreaOperationCompleted == null))
            {
                this.UserCanAccessWebSitePrivateAreaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUserCanAccessWebSitePrivateAreaOperationCompleted);
            }
            this.InvokeAsync("UserCanAccessWebSitePrivateArea", new object[] {
                        loginId}, this.UserCanAccessWebSitePrivateAreaOperationCompleted, userState);
        }

        private void OnUserCanAccessWebSitePrivateAreaOperationCompleted(object arg)
        {
            if ((this.UserCanAccessWebSitePrivateAreaCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UserCanAccessWebSitePrivateAreaCompleted(this, new UserCanAccessWebSitePrivateAreaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/IsEasyBuilderDeveloper", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsEasyBuilderDeveloper(string authenticationToken)
        {
            object[] results = this.Invoke("IsEasyBuilderDeveloper", new object[] {
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsEasyBuilderDeveloperAsync(string authenticationToken)
        {
            this.IsEasyBuilderDeveloperAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void IsEasyBuilderDeveloperAsync(string authenticationToken, object userState)
        {
            if ((this.IsEasyBuilderDeveloperOperationCompleted == null))
            {
                this.IsEasyBuilderDeveloperOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsEasyBuilderDeveloperOperationCompleted);
            }
            this.InvokeAsync("IsEasyBuilderDeveloper", new object[] {
                        authenticationToken}, this.IsEasyBuilderDeveloperOperationCompleted, userState);
        }

        private void OnIsEasyBuilderDeveloperOperationCompleted(object arg)
        {
            if ((this.IsEasyBuilderDeveloperCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsEasyBuilderDeveloperCompleted(this, new IsEasyBuilderDeveloperCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SendErrorFile", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SendErrorFile(string LogFile, out string ErrorMessage)
        {
            object[] results = this.Invoke("SendErrorFile", new object[] {
                        LogFile});
            ErrorMessage = ((string)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SendErrorFileAsync(string LogFile)
        {
            this.SendErrorFileAsync(LogFile, null);
        }

        /// <remarks/>
        public void SendErrorFileAsync(string LogFile, object userState)
        {
            if ((this.SendErrorFileOperationCompleted == null))
            {
                this.SendErrorFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendErrorFileOperationCompleted);
            }
            this.InvokeAsync("SendErrorFile", new object[] {
                        LogFile}, this.SendErrorFileOperationCompleted, userState);
        }

        private void OnSendErrorFileOperationCompleted(object arg)
        {
            if ((this.SendErrorFileCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendErrorFileCompleted(this, new SendErrorFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DownloadPdb", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool DownloadPdb(string PdbFile, out string ErrorMessage)
        {
            object[] results = this.Invoke("DownloadPdb", new object[] {
                        PdbFile});
            ErrorMessage = ((string)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void DownloadPdbAsync(string PdbFile)
        {
            this.DownloadPdbAsync(PdbFile, null);
        }

        /// <remarks/>
        public void DownloadPdbAsync(string PdbFile, object userState)
        {
            if ((this.DownloadPdbOperationCompleted == null))
            {
                this.DownloadPdbOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDownloadPdbOperationCompleted);
            }
            this.InvokeAsync("DownloadPdb", new object[] {
                        PdbFile}, this.DownloadPdbOperationCompleted, userState);
        }

        private void OnDownloadPdbOperationCompleted(object arg)
        {
            if ((this.DownloadPdbCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DownloadPdbCompleted(this, new DownloadPdbCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMainSerialNumber", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMainSerialNumber()
        {
            object[] results = this.Invoke("GetMainSerialNumber", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetMainSerialNumberAsync()
        {
            this.GetMainSerialNumberAsync(null);
        }

        /// <remarks/>
        public void GetMainSerialNumberAsync(object userState)
        {
            if ((this.GetMainSerialNumberOperationCompleted == null))
            {
                this.GetMainSerialNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMainSerialNumberOperationCompleted);
            }
            this.InvokeAsync("GetMainSerialNumber", new object[0], this.GetMainSerialNumberOperationCompleted, userState);
        }

        private void OnGetMainSerialNumberOperationCompleted(object arg)
        {
            if ((this.GetMainSerialNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMainSerialNumberCompleted(this, new GetMainSerialNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/GetMLUExpiryDate", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetMLUExpiryDate()
        {
            object[] results = this.Invoke("GetMLUExpiryDate", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetMLUExpiryDateAsync()
        {
            this.GetMLUExpiryDateAsync(null);
        }

        /// <remarks/>
        public void GetMLUExpiryDateAsync(object userState)
        {
            if ((this.GetMLUExpiryDateOperationCompleted == null))
            {
                this.GetMLUExpiryDateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMLUExpiryDateOperationCompleted);
            }
            this.InvokeAsync("GetMLUExpiryDate", new object[0], this.GetMLUExpiryDateOperationCompleted, userState);
        }

        private void OnGetMLUExpiryDateOperationCompleted(object arg)
        {
            if ((this.GetMLUExpiryDateCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMLUExpiryDateCompleted(this, new GetMLUExpiryDateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/AdvancedSendTaggedBalloon", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
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
            this.Invoke("AdvancedSendTaggedBalloon", new object[] {
                 authenticationToken,
                 bodyMessage,
                 expiryDate ,
                 messageType ,
                 recipients ,
                 sensation ,
                 historicize ,
                 immediate ,
                 timer, tag });
        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/AdvancedSendBalloon", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
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
            this.Invoke("AdvancedSendBalloon", new object[] {
                 authenticationToken,
                 bodyMessage,
                 expiryDate ,
                 messageType ,
                 recipients ,
                 sensation ,
                 historicize ,
                 immediate ,
                 timer });
        }
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/DeleteMessageFromQueue", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void DeleteMessageFromQueue(string messageID)
        {
            this.Invoke("DeleteMessageFromQueue", new object[] { messageID });
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/PurgeMessageByTag", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void PurgeMessageByTag(string tag, string user)
        {
            this.Invoke("PurgeMessageByTag", new object[] { tag, user });
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/LoginManager/SendBalloon", RequestNamespace = "http://microarea.it/LoginManager/", ResponseNamespace = "http://microarea.it/LoginManager/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SendBalloon(string authenticationToken, string bodyMessage, Object messageType, string[] recipients)
        {
            this.Invoke("SendBalloon", new object[] {
                        authenticationToken,
                        bodyMessage,
                        messageType,
                        recipients});
        }

        /// <remarks/>
        public void SendBalloonAsync(string authenticationToken, string bodyMessage, Object messageType, string[] recipients)
        {
            this.SendBalloonAsync(authenticationToken, bodyMessage, messageType, recipients, null);
        }

        /// <remarks/>
        public void SendBalloonAsync(string authenticationToken, string bodyMessage, Object messageType, string[] recipients, object userState)
        {
            if ((this.SendBalloonOperationCompleted == null))
            {
                this.SendBalloonOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendBalloonOperationCompleted);
            }
            this.InvokeAsync("SendBalloon", new object[] {
                        authenticationToken,
                        bodyMessage,
                        messageType,
                        recipients}, this.SendBalloonOperationCompleted, userState);
        }

        private void OnSendBalloonOperationCompleted(object arg)
        {
            if ((this.SendBalloonCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendBalloonCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        private bool IsLocalFileSystemWebService(string url)
        {
            if (((url == null)
                        || (url == string.Empty)))
            {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024)
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microarea.RSWeb.WebServicesWrapper.WebServicesWrapperStrings", typeof(WebServicesWrapperStrings).Assembly);
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


namespace Microarea.RSWeb.WebServicesWrapper.TbSrv
{


   
    public partial class TbServices : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private System.Threading.SendOrPostCallback InitOperationCompleted;

        private System.Threading.SendOrPostCallback CloseTBOperationCompleted;

        private System.Threading.SendOrPostCallback CreateTBOperationCompleted;

        private System.Threading.SendOrPostCallback CreateTBExOperationCompleted;

        private System.Threading.SendOrPostCallback GetWCFBindingOperationCompleted;

        private System.Threading.SendOrPostCallback ReleaseTBOperationCompleted;

        private System.Threading.SendOrPostCallback GetTbLoaderInstantiatedListXMLOperationCompleted;

        private System.Threading.SendOrPostCallback IsTbLoaderInstantiatedOperationCompleted;

        private System.Threading.SendOrPostCallback KillThreadOperationCompleted;

        private System.Threading.SendOrPostCallback StopThreadOperationCompleted;

        private System.Threading.SendOrPostCallback KillProcessOperationCompleted;

        private System.Threading.SendOrPostCallback StopProcessOperationCompleted;

        private System.Threading.SendOrPostCallback SetForceApplicationDateOperationCompleted;

        private System.Threading.SendOrPostCallback SetDataOperationCompleted;

        private System.Threading.SendOrPostCallback GetDataOperationCompleted;

        private System.Threading.SendOrPostCallback XmlGetParametersOperationCompleted;

        private System.Threading.SendOrPostCallback GetXMLHotLinkOperationCompleted;

        private System.Threading.SendOrPostCallback GetDocumentSchemaOperationCompleted;

        private System.Threading.SendOrPostCallback GetReportSchemaOperationCompleted;

        private System.Threading.SendOrPostCallback GetXMLEnumOperationCompleted;

        private System.Threading.SendOrPostCallback GetEnumsXmlOperationCompleted;

        private System.Threading.SendOrPostCallback GetXMLHotLinkDefOperationCompleted;

        private System.Threading.SendOrPostCallback RunFunctionOperationCompleted;

        private System.Threading.SendOrPostCallback GetCachedFileOperationCompleted;

        private System.Threading.SendOrPostCallback GetFileStreamOperationCompleted;

        private System.Threading.SendOrPostCallback IsAliveOperationCompleted;

        private System.Threading.SendOrPostCallback GetDiagnosticItemsOperationCompleted;

        private System.Threading.SendOrPostCallback GetServerPrinterNamesOperationCompleted;

        private bool useDefaultCredentialsSetExplicitly;

        /// <remarks/>
        public TbServices()
        {
            //TODO RSWEB  this.Url = global::Microarea.TaskBuilderNet.Core.Properties.Settings.Default.Microarea_TaskBuilderNet_Core_tbService_TbServices;
            if ((this.IsLocalFileSystemWebService(this.Url) == true))
            {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true)
                            && (this.useDefaultCredentialsSetExplicitly == false))
                            && (this.IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        /// <remarks/>
        public event InitCompletedEventHandler InitCompleted;

        /// <remarks/>
        public event CloseTBCompletedEventHandler CloseTBCompleted;

        /// <remarks/>
        public event CreateTBCompletedEventHandler CreateTBCompleted;

        /// <remarks/>
        public event CreateTBExCompletedEventHandler CreateTBExCompleted;

        /// <remarks/>
        public event GetWCFBindingCompletedEventHandler GetWCFBindingCompleted;

        /// <remarks/>
        public event ReleaseTBCompletedEventHandler ReleaseTBCompleted;

        /// <remarks/>
        public event GetTbLoaderInstantiatedListXMLCompletedEventHandler GetTbLoaderInstantiatedListXMLCompleted;

        /// <remarks/>
        public event IsTbLoaderInstantiatedCompletedEventHandler IsTbLoaderInstantiatedCompleted;

        /// <remarks/>
        public event KillThreadCompletedEventHandler KillThreadCompleted;

        /// <remarks/>
        public event StopThreadCompletedEventHandler StopThreadCompleted;

        /// <remarks/>
        public event KillProcessCompletedEventHandler KillProcessCompleted;

        /// <remarks/>
        public event StopProcessCompletedEventHandler StopProcessCompleted;

        /// <remarks/>
        public event SetForceApplicationDateCompletedEventHandler SetForceApplicationDateCompleted;

        /// <remarks/>
        public event SetDataCompletedEventHandler SetDataCompleted;

        /// <remarks/>
        public event GetDataCompletedEventHandler GetDataCompleted;

        /// <remarks/>
        public event XmlGetParametersCompletedEventHandler XmlGetParametersCompleted;

        /// <remarks/>
        public event GetXMLHotLinkCompletedEventHandler GetXMLHotLinkCompleted;

        /// <remarks/>
        public event GetDocumentSchemaCompletedEventHandler GetDocumentSchemaCompleted;

        /// <remarks/>
        public event GetReportSchemaCompletedEventHandler GetReportSchemaCompleted;

        /// <remarks/>
        public event GetXMLEnumCompletedEventHandler GetXMLEnumCompleted;

        /// <remarks/>
        public event GetEnumsXmlCompletedEventHandler GetEnumsXmlCompleted;

        /// <remarks/>
        public event GetXMLHotLinkDefCompletedEventHandler GetXMLHotLinkDefCompleted;

        /// <remarks/>
        public event RunFunctionCompletedEventHandler RunFunctionCompleted;

        /// <remarks/>
        public event GetCachedFileCompletedEventHandler GetCachedFileCompleted;

        /// <remarks/>
        public event GetFileStreamCompletedEventHandler GetFileStreamCompleted;

        /// <remarks/>
        public event IsAliveCompletedEventHandler IsAliveCompleted;

        /// <remarks/>
        public event GetDiagnosticItemsCompletedEventHandler GetDiagnosticItemsCompleted;

        /// <remarks/>
        public event GetServerPrinterNamesCompletedEventHandler GetServerPrinterNamesCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/Init", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void Init()
        {
            this.Invoke("Init", new object[0]);
        }

        /// <remarks/>
        public void InitAsync()
        {
            this.InitAsync(null);
        }

        /// <remarks/>
        public void InitAsync(object userState)
        {
            if ((this.InitOperationCompleted == null))
            {
                this.InitOperationCompleted = new System.Threading.SendOrPostCallback(this.OnInitOperationCompleted);
            }
            this.InvokeAsync("Init", new object[0], this.InitOperationCompleted, userState);
        }

        private void OnInitOperationCompleted(object arg)
        {
            if ((this.InitCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.InitCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/CloseTB", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void CloseTB(string authenticationToken)
        {
            this.Invoke("CloseTB", new object[] {
                        authenticationToken});
        }

        /// <remarks/>
        public void CloseTBAsync(string authenticationToken)
        {
            this.CloseTBAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void CloseTBAsync(string authenticationToken, object userState)
        {
            if ((this.CloseTBOperationCompleted == null))
            {
                this.CloseTBOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCloseTBOperationCompleted);
            }
            this.InvokeAsync("CloseTB", new object[] {
                        authenticationToken}, this.CloseTBOperationCompleted, userState);
        }

        private void OnCloseTBOperationCompleted(object arg)
        {
            if ((this.CloseTBCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CloseTBCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/CreateTB", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int CreateTB(string authenticationToken, System.DateTime applicationDate, bool checkDate, out string easyToken)
        {
            object[] results = this.Invoke("CreateTB", new object[] {
                        authenticationToken,
                        applicationDate,
                        checkDate});
            easyToken = ((string)(results[1]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void CreateTBAsync(string authenticationToken, System.DateTime applicationDate, bool checkDate)
        {
            this.CreateTBAsync(authenticationToken, applicationDate, checkDate, null);
        }

        /// <remarks/>
        public void CreateTBAsync(string authenticationToken, System.DateTime applicationDate, bool checkDate, object userState)
        {
            if ((this.CreateTBOperationCompleted == null))
            {
                this.CreateTBOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateTBOperationCompleted);
            }
            this.InvokeAsync("CreateTB", new object[] {
                        authenticationToken,
                        applicationDate,
                        checkDate}, this.CreateTBOperationCompleted, userState);
        }

        private void OnCreateTBOperationCompleted(object arg)
        {
            if ((this.CreateTBCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CreateTBCompleted(this, new CreateTBCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/CreateTBEx", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int CreateTBEx(string authenticationToken, System.DateTime applicationDate, bool checkDate, out string easyToken, out string server)
        {
            object[] results = this.Invoke("CreateTBEx", new object[] {
                        authenticationToken,
                        applicationDate,
                        checkDate});
            easyToken = ((string)(results[1]));
            server = ((string)(results[2]));
            return ((int)(results[0]));
        }

        /// <remarks/>
        public void CreateTBExAsync(string authenticationToken, System.DateTime applicationDate, bool checkDate)
        {
            this.CreateTBExAsync(authenticationToken, applicationDate, checkDate, null);
        }

        /// <remarks/>
        public void CreateTBExAsync(string authenticationToken, System.DateTime applicationDate, bool checkDate, object userState)
        {
            if ((this.CreateTBExOperationCompleted == null))
            {
                this.CreateTBExOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateTBExOperationCompleted);
            }
            this.InvokeAsync("CreateTBEx", new object[] {
                        authenticationToken,
                        applicationDate,
                        checkDate}, this.CreateTBExOperationCompleted, userState);
        }

        private void OnCreateTBExOperationCompleted(object arg)
        {
            if ((this.CreateTBExCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CreateTBExCompleted(this, new CreateTBExCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetWCFBinding", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public WCFBinding GetWCFBinding()
        {
            object[] results = this.Invoke("GetWCFBinding", new object[0]);
            return ((WCFBinding)(results[0]));
        }

        /// <remarks/>
        public void GetWCFBindingAsync()
        {
            this.GetWCFBindingAsync(null);
        }

        /// <remarks/>
        public void GetWCFBindingAsync(object userState)
        {
            if ((this.GetWCFBindingOperationCompleted == null))
            {
                this.GetWCFBindingOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetWCFBindingOperationCompleted);
            }
            this.InvokeAsync("GetWCFBinding", new object[0], this.GetWCFBindingOperationCompleted, userState);
        }

        private void OnGetWCFBindingOperationCompleted(object arg)
        {
            if ((this.GetWCFBindingCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetWCFBindingCompleted(this, new GetWCFBindingCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/ReleaseTB", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void ReleaseTB(string easyToken)
        {
            this.Invoke("ReleaseTB", new object[] {
                        easyToken});
        }

        /// <remarks/>
        public void ReleaseTBAsync(string easyToken)
        {
            this.ReleaseTBAsync(easyToken, null);
        }

        /// <remarks/>
        public void ReleaseTBAsync(string easyToken, object userState)
        {
            if ((this.ReleaseTBOperationCompleted == null))
            {
                this.ReleaseTBOperationCompleted = new System.Threading.SendOrPostCallback(this.OnReleaseTBOperationCompleted);
            }
            this.InvokeAsync("ReleaseTB", new object[] {
                        easyToken}, this.ReleaseTBOperationCompleted, userState);
        }

        private void OnReleaseTBOperationCompleted(object arg)
        {
            if ((this.ReleaseTBCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ReleaseTBCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetTbLoaderInstantiatedListXML", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTbLoaderInstantiatedListXML(string authenticationToken)
        {
            object[] results = this.Invoke("GetTbLoaderInstantiatedListXML", new object[] {
                        authenticationToken});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetTbLoaderInstantiatedListXMLAsync(string authenticationToken)
        {
            this.GetTbLoaderInstantiatedListXMLAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void GetTbLoaderInstantiatedListXMLAsync(string authenticationToken, object userState)
        {
            if ((this.GetTbLoaderInstantiatedListXMLOperationCompleted == null))
            {
                this.GetTbLoaderInstantiatedListXMLOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTbLoaderInstantiatedListXMLOperationCompleted);
            }
            this.InvokeAsync("GetTbLoaderInstantiatedListXML", new object[] {
                        authenticationToken}, this.GetTbLoaderInstantiatedListXMLOperationCompleted, userState);
        }

        private void OnGetTbLoaderInstantiatedListXMLOperationCompleted(object arg)
        {
            if ((this.GetTbLoaderInstantiatedListXMLCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTbLoaderInstantiatedListXMLCompleted(this, new GetTbLoaderInstantiatedListXMLCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/IsTbLoaderInstantiated", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsTbLoaderInstantiated(string authenticationToken)
        {
            object[] results = this.Invoke("IsTbLoaderInstantiated", new object[] {
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsTbLoaderInstantiatedAsync(string authenticationToken)
        {
            this.IsTbLoaderInstantiatedAsync(authenticationToken, null);
        }

        /// <remarks/>
        public void IsTbLoaderInstantiatedAsync(string authenticationToken, object userState)
        {
            if ((this.IsTbLoaderInstantiatedOperationCompleted == null))
            {
                this.IsTbLoaderInstantiatedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsTbLoaderInstantiatedOperationCompleted);
            }
            this.InvokeAsync("IsTbLoaderInstantiated", new object[] {
                        authenticationToken}, this.IsTbLoaderInstantiatedOperationCompleted, userState);
        }

        private void OnIsTbLoaderInstantiatedOperationCompleted(object arg)
        {
            if ((this.IsTbLoaderInstantiatedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsTbLoaderInstantiatedCompleted(this, new IsTbLoaderInstantiatedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/KillThread", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void KillThread(int threadID, int processId, string authenticationToken)
        {
            this.Invoke("KillThread", new object[] {
                        threadID,
                        processId,
                        authenticationToken});
        }

        /// <remarks/>
        public void KillThreadAsync(int threadID, int processId, string authenticationToken)
        {
            this.KillThreadAsync(threadID, processId, authenticationToken, null);
        }

        /// <remarks/>
        public void KillThreadAsync(int threadID, int processId, string authenticationToken, object userState)
        {
            if ((this.KillThreadOperationCompleted == null))
            {
                this.KillThreadOperationCompleted = new System.Threading.SendOrPostCallback(this.OnKillThreadOperationCompleted);
            }
            this.InvokeAsync("KillThread", new object[] {
                        threadID,
                        processId,
                        authenticationToken}, this.KillThreadOperationCompleted, userState);
        }

        private void OnKillThreadOperationCompleted(object arg)
        {
            if ((this.KillThreadCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.KillThreadCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/StopThread", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool StopThread(int threadID, int processId, string authenticationToken)
        {
            object[] results = this.Invoke("StopThread", new object[] {
                        threadID,
                        processId,
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void StopThreadAsync(int threadID, int processId, string authenticationToken)
        {
            this.StopThreadAsync(threadID, processId, authenticationToken, null);
        }

        /// <remarks/>
        public void StopThreadAsync(int threadID, int processId, string authenticationToken, object userState)
        {
            if ((this.StopThreadOperationCompleted == null))
            {
                this.StopThreadOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStopThreadOperationCompleted);
            }
            this.InvokeAsync("StopThread", new object[] {
                        threadID,
                        processId,
                        authenticationToken}, this.StopThreadOperationCompleted, userState);
        }

        private void OnStopThreadOperationCompleted(object arg)
        {
            if ((this.StopThreadCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StopThreadCompleted(this, new StopThreadCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/KillProcess", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void KillProcess(int processId, string authenticationToken)
        {
            this.Invoke("KillProcess", new object[] {
                        processId,
                        authenticationToken});
        }

        /// <remarks/>
        public void KillProcessAsync(int processId, string authenticationToken)
        {
            this.KillProcessAsync(processId, authenticationToken, null);
        }

        /// <remarks/>
        public void KillProcessAsync(int processId, string authenticationToken, object userState)
        {
            if ((this.KillProcessOperationCompleted == null))
            {
                this.KillProcessOperationCompleted = new System.Threading.SendOrPostCallback(this.OnKillProcessOperationCompleted);
            }
            this.InvokeAsync("KillProcess", new object[] {
                        processId,
                        authenticationToken}, this.KillProcessOperationCompleted, userState);
        }

        private void OnKillProcessOperationCompleted(object arg)
        {
            if ((this.KillProcessCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.KillProcessCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/StopProcess", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool StopProcess(int processId, string authenticationToken)
        {
            object[] results = this.Invoke("StopProcess", new object[] {
                        processId,
                        authenticationToken});
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void StopProcessAsync(int processId, string authenticationToken)
        {
            this.StopProcessAsync(processId, authenticationToken, null);
        }

        /// <remarks/>
        public void StopProcessAsync(int processId, string authenticationToken, object userState)
        {
            if ((this.StopProcessOperationCompleted == null))
            {
                this.StopProcessOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStopProcessOperationCompleted);
            }
            this.InvokeAsync("StopProcess", new object[] {
                        processId,
                        authenticationToken}, this.StopProcessOperationCompleted, userState);
        }

        private void OnStopProcessOperationCompleted(object arg)
        {
            if ((this.StopProcessCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StopProcessCompleted(this, new StopProcessCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/SetForceApplicationDate", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SetForceApplicationDate(bool force)
        {
            this.Invoke("SetForceApplicationDate", new object[] {
                        force});
        }

        /// <remarks/>
        public void SetForceApplicationDateAsync(bool force)
        {
            this.SetForceApplicationDateAsync(force, null);
        }

        /// <remarks/>
        public void SetForceApplicationDateAsync(bool force, object userState)
        {
            if ((this.SetForceApplicationDateOperationCompleted == null))
            {
                this.SetForceApplicationDateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetForceApplicationDateOperationCompleted);
            }
            this.InvokeAsync("SetForceApplicationDate", new object[] {
                        force}, this.SetForceApplicationDateOperationCompleted, userState);
        }

        private void OnSetForceApplicationDateOperationCompleted(object arg)
        {
            if ((this.SetForceApplicationDateCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetForceApplicationDateCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/SetData", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SetData(string authenticationToken, string data, System.DateTime applicationDate, int postingAction, bool useApproximation, out string result)
        {
            object[] results = this.Invoke("SetData", new object[] {
                        authenticationToken,
                        data,
                        applicationDate,
                        postingAction,
                        useApproximation});
            result = ((string)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void SetDataAsync(string authenticationToken, string data, System.DateTime applicationDate, int postingAction, bool useApproximation)
        {
            this.SetDataAsync(authenticationToken, data, applicationDate, postingAction, useApproximation, null);
        }

        /// <remarks/>
        public void SetDataAsync(string authenticationToken, string data, System.DateTime applicationDate, int postingAction, bool useApproximation, object userState)
        {
            if ((this.SetDataOperationCompleted == null))
            {
                this.SetDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetDataOperationCompleted);
            }
            this.InvokeAsync("SetData", new object[] {
                        authenticationToken,
                        data,
                        applicationDate,
                        postingAction,
                        useApproximation}, this.SetDataOperationCompleted, userState);
        }

        private void OnSetDataOperationCompleted(object arg)
        {
            if ((this.SetDataCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetDataCompleted(this, new SetDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetData", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetData(string authenticationToken, string parameters, System.DateTime applicationDate, int loadAction, int resultType, int formatType, bool useApproximation)
        {
            object[] results = this.Invoke("GetData", new object[] {
                        authenticationToken,
                        parameters,
                        applicationDate,
                        loadAction,
                        resultType,
                        formatType,
                        useApproximation});
            return ((string[])(results[0]));
        }

        /// <remarks/>
        public void GetDataAsync(string authenticationToken, string parameters, System.DateTime applicationDate, int loadAction, int resultType, int formatType, bool useApproximation)
        {
            this.GetDataAsync(authenticationToken, parameters, applicationDate, loadAction, resultType, formatType, useApproximation, null);
        }

        /// <remarks/>
        public void GetDataAsync(string authenticationToken, string parameters, System.DateTime applicationDate, int loadAction, int resultType, int formatType, bool useApproximation, object userState)
        {
            if ((this.GetDataOperationCompleted == null))
            {
                this.GetDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDataOperationCompleted);
            }
            this.InvokeAsync("GetData", new object[] {
                        authenticationToken,
                        parameters,
                        applicationDate,
                        loadAction,
                        resultType,
                        formatType,
                        useApproximation}, this.GetDataOperationCompleted, userState);
        }

        private void OnGetDataOperationCompleted(object arg)
        {
            if ((this.GetDataCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDataCompleted(this, new GetDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/XmlGetParameters", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string XmlGetParameters(string authenticationToken, string parameters, System.DateTime applicationDate, bool useApproximation)
        {
            object[] results = this.Invoke("XmlGetParameters", new object[] {
                        authenticationToken,
                        parameters,
                        applicationDate,
                        useApproximation});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void XmlGetParametersAsync(string authenticationToken, string parameters, System.DateTime applicationDate, bool useApproximation)
        {
            this.XmlGetParametersAsync(authenticationToken, parameters, applicationDate, useApproximation, null);
        }

        /// <remarks/>
        public void XmlGetParametersAsync(string authenticationToken, string parameters, System.DateTime applicationDate, bool useApproximation, object userState)
        {
            if ((this.XmlGetParametersOperationCompleted == null))
            {
                this.XmlGetParametersOperationCompleted = new System.Threading.SendOrPostCallback(this.OnXmlGetParametersOperationCompleted);
            }
            this.InvokeAsync("XmlGetParameters", new object[] {
                        authenticationToken,
                        parameters,
                        applicationDate,
                        useApproximation}, this.XmlGetParametersOperationCompleted, userState);
        }

        private void OnXmlGetParametersOperationCompleted(object arg)
        {
            if ((this.XmlGetParametersCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.XmlGetParametersCompleted(this, new XmlGetParametersCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetXMLHotLink", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetXMLHotLink(string authenticationToken, string docNamespace, string nsUri, string fieldXPath)
        {
            object[] results = this.Invoke("GetXMLHotLink", new object[] {
                        authenticationToken,
                        docNamespace,
                        nsUri,
                        fieldXPath});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetXMLHotLinkAsync(string authenticationToken, string docNamespace, string nsUri, string fieldXPath)
        {
            this.GetXMLHotLinkAsync(authenticationToken, docNamespace, nsUri, fieldXPath, null);
        }

        /// <remarks/>
        public void GetXMLHotLinkAsync(string authenticationToken, string docNamespace, string nsUri, string fieldXPath, object userState)
        {
            if ((this.GetXMLHotLinkOperationCompleted == null))
            {
                this.GetXMLHotLinkOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetXMLHotLinkOperationCompleted);
            }
            this.InvokeAsync("GetXMLHotLink", new object[] {
                        authenticationToken,
                        docNamespace,
                        nsUri,
                        fieldXPath}, this.GetXMLHotLinkOperationCompleted, userState);
        }

        private void OnGetXMLHotLinkOperationCompleted(object arg)
        {
            if ((this.GetXMLHotLinkCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetXMLHotLinkCompleted(this, new GetXMLHotLinkCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetDocumentSchema", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetDocumentSchema(string authenticationToken, string documentNamespace, string profileName, string forUser)
        {
            object[] results = this.Invoke("GetDocumentSchema", new object[] {
                        authenticationToken,
                        documentNamespace,
                        profileName,
                        forUser});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetDocumentSchemaAsync(string authenticationToken, string documentNamespace, string profileName, string forUser)
        {
            this.GetDocumentSchemaAsync(authenticationToken, documentNamespace, profileName, forUser, null);
        }

        /// <remarks/>
        public void GetDocumentSchemaAsync(string authenticationToken, string documentNamespace, string profileName, string forUser, object userState)
        {
            if ((this.GetDocumentSchemaOperationCompleted == null))
            {
                this.GetDocumentSchemaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDocumentSchemaOperationCompleted);
            }
            this.InvokeAsync("GetDocumentSchema", new object[] {
                        authenticationToken,
                        documentNamespace,
                        profileName,
                        forUser}, this.GetDocumentSchemaOperationCompleted, userState);
        }

        private void OnGetDocumentSchemaOperationCompleted(object arg)
        {
            if ((this.GetDocumentSchemaCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDocumentSchemaCompleted(this, new GetDocumentSchemaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetReportSchema", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetReportSchema(string authenticationToken, string reportNamespace, string forUser)
        {
            object[] results = this.Invoke("GetReportSchema", new object[] {
                        authenticationToken,
                        reportNamespace,
                        forUser});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetReportSchemaAsync(string authenticationToken, string reportNamespace, string forUser)
        {
            this.GetReportSchemaAsync(authenticationToken, reportNamespace, forUser, null);
        }

        /// <remarks/>
        public void GetReportSchemaAsync(string authenticationToken, string reportNamespace, string forUser, object userState)
        {
            if ((this.GetReportSchemaOperationCompleted == null))
            {
                this.GetReportSchemaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetReportSchemaOperationCompleted);
            }
            this.InvokeAsync("GetReportSchema", new object[] {
                        authenticationToken,
                        reportNamespace,
                        forUser}, this.GetReportSchemaOperationCompleted, userState);
        }

        private void OnGetReportSchemaOperationCompleted(object arg)
        {
            if ((this.GetReportSchemaCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetReportSchemaCompleted(this, new GetReportSchemaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetXMLEnum", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetXMLEnum(string authenticationToken, int enumID, string userLanguage)
        {
            object[] results = this.Invoke("GetXMLEnum", new object[] {
                        authenticationToken,
                        enumID,
                        userLanguage});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetXMLEnumAsync(string authenticationToken, int enumID, string userLanguage)
        {
            this.GetXMLEnumAsync(authenticationToken, enumID, userLanguage, null);
        }

        /// <remarks/>
        public void GetXMLEnumAsync(string authenticationToken, int enumID, string userLanguage, object userState)
        {
            if ((this.GetXMLEnumOperationCompleted == null))
            {
                this.GetXMLEnumOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetXMLEnumOperationCompleted);
            }
            this.InvokeAsync("GetXMLEnum", new object[] {
                        authenticationToken,
                        enumID,
                        userLanguage}, this.GetXMLEnumOperationCompleted, userState);
        }

        private void OnGetXMLEnumOperationCompleted(object arg)
        {
            if ((this.GetXMLEnumCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetXMLEnumCompleted(this, new GetXMLEnumCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetEnumsXml", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetEnumsXml(string userLanguage)
        {
            object[] results = this.Invoke("GetEnumsXml", new object[] {
                        userLanguage});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetEnumsXmlAsync(string userLanguage)
        {
            this.GetEnumsXmlAsync(userLanguage, null);
        }

        /// <remarks/>
        public void GetEnumsXmlAsync(string userLanguage, object userState)
        {
            if ((this.GetEnumsXmlOperationCompleted == null))
            {
                this.GetEnumsXmlOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetEnumsXmlOperationCompleted);
            }
            this.InvokeAsync("GetEnumsXml", new object[] {
                        userLanguage}, this.GetEnumsXmlOperationCompleted, userState);
        }

        private void OnGetEnumsXmlOperationCompleted(object arg)
        {
            if ((this.GetEnumsXmlCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetEnumsXmlCompleted(this, new GetEnumsXmlCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetXMLHotLinkDef", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetXMLHotLinkDef(string authenticationToken, string docNameSpace, string nsUri, string fieldXPath, string companyName)
        {
            object[] results = this.Invoke("GetXMLHotLinkDef", new object[] {
                        authenticationToken,
                        docNameSpace,
                        nsUri,
                        fieldXPath,
                        companyName});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetXMLHotLinkDefAsync(string authenticationToken, string docNameSpace, string nsUri, string fieldXPath, string companyName)
        {
            this.GetXMLHotLinkDefAsync(authenticationToken, docNameSpace, nsUri, fieldXPath, companyName, null);
        }

        /// <remarks/>
        public void GetXMLHotLinkDefAsync(string authenticationToken, string docNameSpace, string nsUri, string fieldXPath, string companyName, object userState)
        {
            if ((this.GetXMLHotLinkDefOperationCompleted == null))
            {
                this.GetXMLHotLinkDefOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetXMLHotLinkDefOperationCompleted);
            }
            this.InvokeAsync("GetXMLHotLinkDef", new object[] {
                        authenticationToken,
                        docNameSpace,
                        nsUri,
                        fieldXPath,
                        companyName}, this.GetXMLHotLinkDefOperationCompleted, userState);
        }

        private void OnGetXMLHotLinkDefOperationCompleted(object arg)
        {
            if ((this.GetXMLHotLinkDefCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetXMLHotLinkDefCompleted(this, new GetXMLHotLinkDefCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/RunFunction", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string RunFunction(string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg)
        {
            object[] results = this.Invoke("RunFunction", new object[] {
                        authenticationToken,
                        request,
                        nameSpace,
                        functionName});
            errorMsg = ((string)(results[1]));
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void RunFunctionAsync(string authenticationToken, string request, string nameSpace, string functionName)
        {
            this.RunFunctionAsync(authenticationToken, request, nameSpace, functionName, null);
        }

        /// <remarks/>
        public void RunFunctionAsync(string authenticationToken, string request, string nameSpace, string functionName, object userState)
        {
            if ((this.RunFunctionOperationCompleted == null))
            {
                this.RunFunctionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRunFunctionOperationCompleted);
            }
            this.InvokeAsync("RunFunction", new object[] {
                        authenticationToken,
                        request,
                        nameSpace,
                        functionName}, this.RunFunctionOperationCompleted, userState);
        }

        private void OnRunFunctionOperationCompleted(object arg)
        {
            if ((this.RunFunctionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RunFunctionCompleted(this, new RunFunctionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetCachedFile", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetCachedFile(string authenticationToken, string nameSpace, string user, string company)
        {
            object[] results = this.Invoke("GetCachedFile", new object[] {
                        authenticationToken,
                        nameSpace,
                        user,
                        company});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void GetCachedFileAsync(string authenticationToken, string nameSpace, string user, string company)
        {
            this.GetCachedFileAsync(authenticationToken, nameSpace, user, company, null);
        }

        /// <remarks/>
        public void GetCachedFileAsync(string authenticationToken, string nameSpace, string user, string company, object userState)
        {
            if ((this.GetCachedFileOperationCompleted == null))
            {
                this.GetCachedFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCachedFileOperationCompleted);
            }
            this.InvokeAsync("GetCachedFile", new object[] {
                        authenticationToken,
                        nameSpace,
                        user,
                        company}, this.GetCachedFileOperationCompleted, userState);
        }

        private void OnGetCachedFileOperationCompleted(object arg)
        {
            if ((this.GetCachedFileCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCachedFileCompleted(this, new GetCachedFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetFileStream", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] GetFileStream(string authenticationToken, string nameSpace, string user, string company)
        {
            object[] results = this.Invoke("GetFileStream", new object[] {
                        authenticationToken,
                        nameSpace,
                        user,
                        company});
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public void GetFileStreamAsync(string authenticationToken, string nameSpace, string user, string company)
        {
            this.GetFileStreamAsync(authenticationToken, nameSpace, user, company, null);
        }

        /// <remarks/>
        public void GetFileStreamAsync(string authenticationToken, string nameSpace, string user, string company, object userState)
        {
            if ((this.GetFileStreamOperationCompleted == null))
            {
                this.GetFileStreamOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileStreamOperationCompleted);
            }
            this.InvokeAsync("GetFileStream", new object[] {
                        authenticationToken,
                        nameSpace,
                        user,
                        company}, this.GetFileStreamOperationCompleted, userState);
        }

        private void OnGetFileStreamOperationCompleted(object arg)
        {
            if ((this.GetFileStreamCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetFileStreamCompleted(this, new GetFileStreamCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/IsAlive", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsAlive()
        {
            object[] results = this.Invoke("IsAlive", new object[0]);
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void IsAliveAsync()
        {
            this.IsAliveAsync(null);
        }

        /// <remarks/>
        public void IsAliveAsync(object userState)
        {
            if ((this.IsAliveOperationCompleted == null))
            {
                this.IsAliveOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsAliveOperationCompleted);
            }
            this.InvokeAsync("IsAlive", new object[0], this.IsAliveOperationCompleted, userState);
        }

        private void OnIsAliveOperationCompleted(object arg)
        {
            if ((this.IsAliveCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsAliveCompleted(this, new IsAliveCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetDiagnosticItems", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public Microarea.TaskBuilderNet.Interfaces.DiagnosticSimpleItem[] GetDiagnosticItems(string authenticationToken, bool clear)
        {
            object[] results = this.Invoke("GetDiagnosticItems", new object[] {
                        authenticationToken,
                        clear});
            return ((Microarea.TaskBuilderNet.Interfaces.DiagnosticSimpleItem[])(results[0]));
        }

        /// <remarks/>
        public void GetDiagnosticItemsAsync(string authenticationToken, bool clear)
        {
            this.GetDiagnosticItemsAsync(authenticationToken, clear, null);
        }

        /// <remarks/>
        public void GetDiagnosticItemsAsync(string authenticationToken, bool clear, object userState)
        {
            if ((this.GetDiagnosticItemsOperationCompleted == null))
            {
                this.GetDiagnosticItemsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDiagnosticItemsOperationCompleted);
            }
            this.InvokeAsync("GetDiagnosticItems", new object[] {
                        authenticationToken,
                        clear}, this.GetDiagnosticItemsOperationCompleted, userState);
        }

        private void OnGetDiagnosticItemsOperationCompleted(object arg)
        {
            if ((this.GetDiagnosticItemsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDiagnosticItemsCompleted(this, new GetDiagnosticItemsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://microarea.it/TbServices/GetServerPrinterNames", RequestNamespace = "http://microarea.it/TbServices/", ResponseNamespace = "http://microarea.it/TbServices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool GetServerPrinterNames(out string[] printers)
        {
            object[] results = this.Invoke("GetServerPrinterNames", new object[0]);
            printers = ((string[])(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void GetServerPrinterNamesAsync()
        {
            this.GetServerPrinterNamesAsync(null);
        }

        /// <remarks/>
        public void GetServerPrinterNamesAsync(object userState)
        {
            if ((this.GetServerPrinterNamesOperationCompleted == null))
            {
                this.GetServerPrinterNamesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetServerPrinterNamesOperationCompleted);
            }
            this.InvokeAsync("GetServerPrinterNames", new object[0], this.GetServerPrinterNamesOperationCompleted, userState);
        }

        private void OnGetServerPrinterNamesOperationCompleted(object arg)
        {
            if ((this.GetServerPrinterNamesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetServerPrinterNamesCompleted(this, new GetServerPrinterNamesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        private bool IsLocalFileSystemWebService(string url)
        {
            if (((url == null)
                        || (url == string.Empty)))
            {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024)
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18408")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://microarea.it/TbServices/")]
    public enum WCFBinding
    {

        /// <remarks/>
        None,

        /// <remarks/>
        BasicHttp,

        /// <remarks/>
        NetTcp,
    }



    /// <remarks/>
    [System.FlagsAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18408")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://microarea.it/TbServices/")]
    public enum DiagnosticType
    {

        /// <remarks/>
        None = 1,

        /// <remarks/>
        Warning = 2,

        /// <remarks/>
        Error = 4,

        /// <remarks/>
        LogInfo = 8,

        /// <remarks/>
        Information = 16,

        /// <remarks/>
        FatalError = 32,

        /// <remarks/>
        Banner = 64,

        /// <remarks/>
        LogOnFile = 128,

        /// <remarks/>
        All = 256,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void InitCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void CloseTBCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void CreateTBCompletedEventHandler(object sender, CreateTBCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CreateTBCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal CreateTBCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
        public string easyToken
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void CreateTBExCompletedEventHandler(object sender, CreateTBExCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CreateTBExCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal CreateTBExCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
        public string easyToken
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }

        /// <remarks/>
        public string server
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[2]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetWCFBindingCompletedEventHandler(object sender, GetWCFBindingCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetWCFBindingCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetWCFBindingCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public WCFBinding Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((WCFBinding)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void ReleaseTBCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetTbLoaderInstantiatedListXMLCompletedEventHandler(object sender, GetTbLoaderInstantiatedListXMLCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTbLoaderInstantiatedListXMLCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetTbLoaderInstantiatedListXMLCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void IsTbLoaderInstantiatedCompletedEventHandler(object sender, IsTbLoaderInstantiatedCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class IsTbLoaderInstantiatedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal IsTbLoaderInstantiatedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void KillThreadCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void StopThreadCompletedEventHandler(object sender, StopThreadCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StopThreadCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal StopThreadCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void KillProcessCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void StopProcessCompletedEventHandler(object sender, StopProcessCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StopProcessCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal StopProcessCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void SetForceApplicationDateCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void SetDataCompletedEventHandler(object sender, SetDataCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SetDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal SetDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
        public string result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetDataCompletedEventHandler(object sender, GetDataCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void XmlGetParametersCompletedEventHandler(object sender, XmlGetParametersCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class XmlGetParametersCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal XmlGetParametersCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetXMLHotLinkCompletedEventHandler(object sender, GetXMLHotLinkCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetXMLHotLinkCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetXMLHotLinkCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetDocumentSchemaCompletedEventHandler(object sender, GetDocumentSchemaCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetDocumentSchemaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetDocumentSchemaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetReportSchemaCompletedEventHandler(object sender, GetReportSchemaCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetReportSchemaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetReportSchemaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetXMLEnumCompletedEventHandler(object sender, GetXMLEnumCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetXMLEnumCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetXMLEnumCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetEnumsXmlCompletedEventHandler(object sender, GetEnumsXmlCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetEnumsXmlCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetEnumsXmlCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetXMLHotLinkDefCompletedEventHandler(object sender, GetXMLHotLinkDefCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetXMLHotLinkDefCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetXMLHotLinkDefCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void RunFunctionCompletedEventHandler(object sender, RunFunctionCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RunFunctionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal RunFunctionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
        public string errorMsg
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetCachedFileCompletedEventHandler(object sender, GetCachedFileCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetCachedFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetCachedFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetFileStreamCompletedEventHandler(object sender, GetFileStreamCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetFileStreamCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetFileStreamCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void IsAliveCompletedEventHandler(object sender, IsAliveCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetDiagnosticItemsCompletedEventHandler(object sender, GetDiagnosticItemsCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetDiagnosticItemsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetDiagnosticItemsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Microarea.TaskBuilderNet.Interfaces.DiagnosticSimpleItem[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Microarea.TaskBuilderNet.Interfaces.DiagnosticSimpleItem[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetServerPrinterNamesCompletedEventHandler(object sender, GetServerPrinterNamesCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetServerPrinterNamesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal GetServerPrinterNamesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
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
        public string[] printers
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string[])(this.results[1]));
            }
        }
    }
}
