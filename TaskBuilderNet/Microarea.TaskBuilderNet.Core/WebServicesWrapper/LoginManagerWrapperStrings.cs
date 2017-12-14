using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// Summary description for LoginManagerWrapperStrings.
	/// </summary>
	//============================================================================
	public sealed class LoginManagerWrapperStrings
	{
		public const string Guest = "Guest";

		//------------------------------------------------------------------------
		private LoginManagerWrapperStrings()
		{ }

		//------------------------------------------------------------------------
		public static string GetString(int loginReturnCode)
		{
			switch (loginReturnCode)
			{
				case (int)LoginReturnCodes.SysDBConnectionFailure:
					return WebServicesWrapperStrings.ServerNotConnectedToSystemDB;
				case (int)LoginReturnCodes.MissingConnectionString:
					return WebServicesWrapperStrings.MissingConnectionString;
				case (int)LoginReturnCodes.ActivationManagerInitializationFailure:
					return WebServicesWrapperStrings.ErrActivationManager;
				case (int)LoginReturnCodes.UserAlreadyLoggedError:
					return WebServicesWrapperStrings.ErrUserAlreadyLogged;
				case (int)LoginReturnCodes.NoCalAvailableError:
					return WebServicesWrapperStrings.NoCalAvailableError;
				case (int)LoginReturnCodes.CalManagementError:
					return WebServicesWrapperStrings.CalManagementError;
				case (int)LoginReturnCodes.NoLicenseError:
					return WebServicesWrapperStrings.ErrNoArticleFunctionality;
				case (int)LoginReturnCodes.UserAssignmentToArticleFailure:
					return WebServicesWrapperStrings.ErrUserAssignmentToArticle;
                case (int)LoginReturnCodes.UserNotAllowed:
                    return WebServicesWrapperStrings.UserNotAllowed;
				case (int)LoginReturnCodes.ProcessNotAuthenticatedError:
					return WebServicesWrapperStrings.ErrProcessNotAuthenticated;
				case (int)LoginReturnCodes.InvalidUserError:
					return WebServicesWrapperStrings.ErrInvalidUser;
				case (int)LoginReturnCodes.InvalidProcessError:
					return WebServicesWrapperStrings.ErrInvalidProcess;
				case (int)LoginReturnCodes.LockedDatabaseError:
					return WebServicesWrapperStrings.ErrLockedDatabase;
				case (int)LoginReturnCodes.UserMustChangePasswordError:
					return WebServicesWrapperStrings.ErrUserCannotChangePwdButMust;
				case (int)LoginReturnCodes.InvalidCompanyError:
					return WebServicesWrapperStrings.ErrInvalidCompany;
				case (int)LoginReturnCodes.ProviderError:
					return WebServicesWrapperStrings.ErrProviderInfo;
				case (int)LoginReturnCodes.ConnectionParamsError:
					return WebServicesWrapperStrings.ErrConnectionParams;
				case (int)LoginReturnCodes.CompanyDatabaseNotPresent:
					return WebServicesWrapperStrings.CompanyDatabaseNotPresent;
				case (int)LoginReturnCodes.CompanyDatabaseTablesNotPresent:
					return WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent;
				case (int)LoginReturnCodes.InvalidDatabaseForActivation:
					return WebServicesWrapperStrings.InvalidDatabaseForActivation;
				case(int)LoginReturnCodes.WebApplicationAccessDenied:
					return WebServicesWrapperStrings.WebApplicationAccessDenied;
				case (int)LoginReturnCodes.GDIApplicationAccessDenied:
					return WebServicesWrapperStrings.GDIApplicationAccessDenied;
				case (int)LoginReturnCodes.NoWebLicenseError:
					return WebServicesWrapperStrings.NoWebLicenseError;
				case (int)LoginReturnCodes.LoginLocked:
					return WebServicesWrapperStrings.LoginLocked;
				case (int)LoginReturnCodes.PasswordAlreadyChangedToday:
					return WebServicesWrapperStrings.PasswordAlreadyChangedToday;
				case (int)LoginReturnCodes.AuthenticationTypeError:
					return WebServicesWrapperStrings.AuthenticationTypeError;
				case (int)LoginReturnCodes.InvalidDatabaseError:
					return WebServicesWrapperStrings.InvalidDatabaseError;
				case (int)LoginReturnCodes.NoAdmittedCompany:
					return WebServicesWrapperStrings.NoAdmittedCompany;
				case (int)LoginReturnCodes.NoOfficeLicenseError:
					return WebServicesWrapperStrings.NoOfficeLicenseError;
				case (int)LoginReturnCodes.TooManyAssignedCAL:
					return WebServicesWrapperStrings.TooManyAssignedCAL;
				case (int)LoginReturnCodes.InvalidModule:
					return WebServicesWrapperStrings.InvalidModuleRelease;

                case (int)LoginReturnCodes.SsoTokenError:
                    return WebServicesWrapperStrings.SsoTokenError;
                   

                case (int)LoginReturnCodes.MoreThanOneSSOToken:
                    return WebServicesWrapperStrings.MoreThanOneSSOToken;
                  

                case (int)LoginReturnCodes.InvalidSSOToken:
                    return WebServicesWrapperStrings.InvalidSSOToken;
                  
                case (int)LoginReturnCodes.ImagoUserAlreadyAssociated:
                    return WebServicesWrapperStrings.ImagoUserAlreadyAssociated;
                  

                case (int)LoginReturnCodes.SSOIDNotAssociated:
                    return WebServicesWrapperStrings.SSOIDNotAssociated;
                    
                case (int)LoginReturnCodes.ImagoCompanyNotCorresponding:
                    return WebServicesWrapperStrings.ImagoCompanyNotCorresponding;
                default:
					return WebServicesWrapperStrings.ErrLoginFailed;
			}
		}
	}
}