﻿
namespace Microarea.AdminServer.Library
{
    //----------------------------------------------------------------------------
    public enum LoginReturnCodes
    {
        NoError = 0,
        SysDBConnectionFailure = 1,
        PathFinderInitializationFailure = 2,
        ActivationManagerInitializationFailure = 3,
        NoLicenseError = 4,
        ArticlesTableReadingFailure = 6,
        ActivationFilesReadingFailure = 7,
        AlreadyLoggedOnDifferentCompanyError = 8,
        UserAlreadyLoggedError = 9,
        NoCalAvailableError = 10,
        UserAssignmentToArticleFailure = 11,
        ProcessNotAuthenticatedError = 12, 
        InvalidUserError = 13,
        InvalidProcessError = 14,
        CannotChangePasswordError = 15,
        PasswordExpiredError = 16,
        PasswordTooShortError = 17,
        LockedDatabaseError = 18,
        UserMustChangePasswordError = 19,
        GenericLoginFailure = 20,
        InvalidCompanyError = 21,
        ProviderError = 22,
        ConnectionParamsError = 23,
        LoginManagerWrapperUninitializedError = 24,
        LoginManagerNotLoggedError = 25,
        Initializing = 26,
        NotInitializing = 27,
        CompanyDatabaseNotPresent = 29,
        CompanyDatabaseTablesNotPresent = 30,
        InvalidDatabaseForActivation = 31,
        WebApplicationAccessDenied = 32,
        GDIApplicationAccessDenied = 33,
        NoWebLicenseError = 34,
        LoginLocked = 35,
        PasswordAlreadyChangedToday = 36,
        AuthenticationTypeError = 37,
        InvalidDatabaseError = 38,
        UnregisteredProduct = 39,
        NoAdmittedCompany = 40,
        NoOfficeLicenseError = 41,
        WebUserAlreadyLoggedError = 42,
        TooManyAssignedCAL = 43,
        BusyResourcesError = 44,
        CalManagementError = 45,
        MissingConnectionString = 46,
        NoDatabase = 47,
        NoTables = 48,
        NoActivatedDatabase = 49,
        InvalidModule = 50,
        Error = 51,
        DBSizeError = 53,
        SsoTokenEmpty = 54,
        InvalidSSOToken = 55,
        UserNotAllowed = 56,
        MoreThanOneSSOToken = 57,
        ImagoUserAlreadyAssociated = 58,
        SsoTokenError = 59,
        SSOIDNotAssociated = 60,
        ImagoCompanyNotCorresponding = 61,
        ErrorSavingTokens=62,
        ErrorSavingAccount = 63,
		UnknownAccountName = 64,
        UserExpired = 65
    }

}

