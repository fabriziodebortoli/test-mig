﻿using System;

namespace TaskBuilderNetCore.Interfaces
{
	//WARNING: these values partially correspond to those defined in 
	//\Framework\TbNameSolver\Diagnostic.h (for commonly defined items)
	[Flags]
	public enum DiagnosticType 
	{
		None = 0, 
		Warning = 1, 
		Error = 2, 
		LogInfo = 4, 
		Information = 8, 
		FatalError = 16, 
		Banner = 32, 
		LogOnFile = 64,
		All = Warning | Error | LogInfo | Information | FatalError | Banner | LogOnFile
	};

	public enum LineSeparator { None, Blank, Tab, Cr, Lf, CrLf, Br }

	/// <summary>
	/// Indica lo stato attuale in cui si trova l'attivazione
	/// </summary> 
	/* Demo:			è lo stato in cui non è stato inserito nulla in attivazione e non è stata fatta 
	///					la registrazione.
	///					C'è un limite di CAL a 3 e mette lo sfondo demo
	///					Dura 15 giorni
	///	DemoWarning		quando scade il periodo demo si comunica all'utente che deve registrarsi
	///					Dura 5 giorni
	///	
	///	SilentWarning:	Quando scade il periodo attivazione si entra in questo stato dove loginmanager
	///					tenta di connettersi al server microarea senza comunicare nulla all'utente
	///					Dura 3 giorni
	///					
	///	Warning			Quando scade il periodo SilentWarning si entra in questo stato dove loginmanager
	///					tenta di connettersi al server microarea e ad ogni login viene comunicato agli utenti 
	///					che devono registrare il prodotto sul sito
	///					Dura 5 giorni
	///	
	///	Activated		E' stata fatta una registrazione sul sito microarea e tutto funziona come Io comanda
	///					Dura 12 giorni
	///					
	///	NoActivated		Non funziona niente
	///	
	///	Disabled		Microarea ha sentenziato che questa installazione non è valida. Non funziona niente
	///	
	/// Undefined		Non e' stato ancora assegnato, equivale a null*/
	//=========================================================================
	public enum ActivationState
	{
		Demo,
		DemoWarning,
		SilentWarning,
		Warning,
		Activated,
		NoActivated,
		Disabled,
		Undefined
	}
}
namespace Microarea.WebServices.LoginManager
{
	//-----------------------------------------------------------------------
	public enum SlotType
	{
		Invalid,
		Gdi,
		MagicDocument,
		EasyLook,
		ThirdPart, 
        Mobile
	}
}
namespace Microarea.TaskBuilderNet.Licence.Activation
{

	/// <summary>
	/// Esprime le possibili edition del prodotto
	/// </summary>
	//=========================================================================
	public enum Edition
	{
		Undefined,
		Standard,
		Professional,
		Enterprise, ALL
	}

	/// <summary>
	/// Esprime i possibili sistemi operativi per il prodotto
	/// </summary>
	//=========================================================================
	public enum OperatingSystem
	{
		Undefined,
		Windows
	}
}
namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	//============================================================================
	public enum DBMSType
	{
		UNKNOWN,
		SQLSERVER,
		ORACLE,
        POSTGRE
	}

	public enum KindOfDatabase
	{
		System,
		Company,
		Dms
	}

	public enum DatabaseVersion
	{
		SqlServer2000,
		MSDE,
		Oracle,
        Postgre,
		Undefined,
		All,
		Ndb
	}
}
namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Esprime i possibili tipi di rete per il prodotto
	/// </summary>
	//=========================================================================
	public enum DBNetworkType
	{
		Undefined,
		Large,
		Small
	}

	//---------------------------------------------------------------------------
	public enum TraceActionType
	{
		Login,
		Logout,
		ChangePassword,
		LoginFailed,
		ChangePasswordFailed,
		DeleteUser,
		DeleteCompany,
		DeleteCompanyUser,
		All
	}
}
namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	//----------------------------------------------------------------------------
	public enum CommandOrigin : short
	{
		Unknown = 0x0000,
		Standard = 0x0001,
		CustomAllUsers = 0x0002,
		CustomCurrentUser = 0x0003
	}

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
        SsoTokenError=59,
        SSOIDNotAssociated =60,
        ImagoCompanyNotCorresponding = 61

    }

	//----------------------------------------------------------------------------
	public enum EasyLookConnectionCodes
	{
		OK = 0,
		UserNotAuthenticated = -1,
		EasyLookSysLoginFailed = -2,
		StartTbLoaderFailed = -3,
		InitTbLoginFailed = -4,
		SetApplicationDateFailed = -5,
	}
}
namespace Microarea.TaskBuilderNet.Licence.Licence
{
	public enum DependencyEvaluationStatus { NotEvaluated, NotSatisfied, Satisfied }
}
