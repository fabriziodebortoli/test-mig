
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbNameSolver\InterfaceClasses.h>
#include <TBNameSolver\LoginContext.h>
#include <TBNameSolver\ThreadContext.h>
#include <TbNameSolver\MacroToRedifine.h>
#include <TbNameSolver\TBResourceLocker.h>

#include "Beginh.dex"

typedef CMap<CString, LPCTSTR, BOOL, BOOL> MapStringToBOOL;
class CFunctionDescription;
//----------------------------------------------------------------------------
class TB_EXPORT CLoginManagerInterface : public CObject
{
	friend class CClientObjects;
	friend class CTaskBuilderApp;

public:
	/// <summary>
	/// Indica lo stato attuale in cui si trova l'attivazione
	/// Demo:			è lo stato in cui non è stato inserito nulla in attivazione e non è stata fatta 
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
	/// Disable			Definito da Microarea quando vuole disattivare un'installazione e come NoActivated
	/// </summary> 
	enum ActivationState 
	{
		Demo,
		DemoWarning,
		SilentWarning,
		Warning,
		Activated,
		NoActivated,
		Disabled
	};

	enum SerialNumberType 
	{
		Normal,
		Development,
		Reseller,
		Distributor,
		Undefined = 99
	};

	//vedi Microarea.TaskBuilderNet.Interfaces\IAdvertisement.cs 
		//[Flags]
		enum BalloonMessageType		//Microarea.TaskBuilderNet.Interfaces.MessageType
		{
			bt_None = 0x0,
			bt_Contract = 0x1,
			bt_Advrtsm = 0x2,
			bt_Updates = 0x4,
			bt_PostaLite = 0x8
		};

		enum BalloonMessageSensation	//Microarea.TaskBuilderNet.Interfaces.MessageSensation 
		{
			 bs_Information, 
			 bs_ResultGreen, 
			 bs_Warning, 
			 bs_Error, 
			 bs_AccessDenied, 
			 bs_Help
		};
	//----

private:
	const CString	m_strService;			// nome del WEB service (se esterno)
	const CString	m_strServiceNamespace;	// namespace del WEB service (se esterno)
	const CString	m_strServer;			// nome del server del WEB service (se esterno)
	const int		m_nWebServicesPort;		// numero di porta di IIS

	CString	m_strSysDbConnectionString;
	CString	m_strProductLanguage;
	CString	m_strEdition; 
	CString	m_strUserInfoLicensee; 
	CString	m_strUserInfoName; 
	CString	m_strUserInfoId; 
	CString	m_strUserInfoCode; 

	CString	m_strBrandedProducerName; //Microarea S.p.A. from installation.ver
	
	struct _InstallationVer	// versione dell'installazione
	{
		DataStr		Version;
		DataStr		ProductName;
		DataDate	BuildDate;
		DataDate	InstallationDate;
		DataLng		Build;
	}					m_InstallationVer;

	// activation data caching
	ActivationState		m_ActivationState;
	SerialNumberType	m_SerialNumberType;
	BOOL				m_bSecurityLightEnabled;

public:
	CLoginManagerInterface	(
								const CString& strService, 
								const CString& strServer, 
								int nWebServicesPort
							);

public:
	BOOL InitLogin					(const CString&	sAuthenticationToken);

	CString	GetService										() { return m_strService; }
	CString	GetServiceNamespace								() { return m_strServiceNamespace; }
	CString	GetServer										() { return m_strServer; }
	int		GetWebServicesPort								() { return m_nWebServicesPort;	}

	CString			GetUserDescriptionById					(int loginID);
	CString			GetUserDescriptionByName				(CString login);
	CString			GetUserEmailByName						(CString login);
	void			EnumCompanies							(CString userName, CStringArray& ar);
	BOOL			IsUserLoggedByName						(CString userName);
	BOOL			ValidateUser							(CString userName, CString password, BOOL ntAuth);


	void			GetCompanyUsersWithoutEasyLookSystem	(CStringArray& ar);
	BOOL			GetUserRoles							(const CString strUser, const CString strCompanyName, CStringArray& arUserRoles);
	BOOL			GetRoleUsers							(const CString& strCompany, const CString& strRole, CStringArray&);
	BOOL			IsSecurityLightEnabled()				{return m_bSecurityLightEnabled;}

public:
	void				InitActivationStateInfo		();
	ActivationState		GetActivationState			();
	const BOOL			IsActivationForDevelopment	();	
	const BOOL			IsADemo						() const;
	//infinity
	CString GetIToken();
	//
	CString GetDbOwner						(int companyId);
	BOOL	GetAuthenticationInformations	(const CString& strAuthenticationToken, DataStr& strLoginName, DataStr& strCompName);
	int		GetTokenProcessType				();
	BOOL	IsCalAvailable					(const CString& strApplication, const CString& strModuleOrFunctionality);
	BOOL	FeUsed							();
	BOOL	IsActivated						(const CString& strApplication, const CString& strModuleOrFunctionality);
	BOOL	IsValidDate						(const DataDate operationDate, DataDate& maxDate);
	BOOL	GetActivatedModules				(CStringArray& arModules);
	int		LogIn							(const CString& sCompany, const CString& sUser, const CString& sPassword, BOOL overWriteLogin, CString& authToken);
	void	LogOff							();
	void	LogOff							(CString token);
	CString	GetLoggedUsersAdvanced			();
	BOOL	IsWinNTUser						(const CString& loginName);
	BOOL	SetCompanyInfo					(const CString& strName, const CString& strValue);
	CString GetMobileToken					(CString token, int loginType);
	void	PurgeMessageByTag				(CString tag, CString user); 

	CString GetSystemDBConnectionString		();
	CString GetUserInfo						();
	CString GetUserInfoName					();
	CString GetUserInfoId					();
	CString GetUserInfoCode					();
	CString	GetProductLanguage				();
	CString	GetEdition						();
	CString	GetEditionType();
	BOOL	UserCanAccessWebSitePrivateArea ();

	//Branding
	CString GetBrandedApplicationTitle		(const CString& strApplicationName);
	CString GetMasterProductBrandedName		();
	CString GetBrandedKey					(const CString& source);
	CString GetBrandedProducerName			(); //call web method


	BOOL	GetLoginInformation				(const CString&	authenticationToken, CLoginInfos* pLoginInfos);
	
	int		GetCompanyLoggedUsersNumber		(int companyId);

	CString GetAspNetUser() const;
	BOOL	IsValidToken					(const CString& strToken, bool& isValid);
	BOOL	SendErrorFile					(const CString& strLogFilePath, CString& strErrors);
	BOOL	DownloadPdb						(const CString& strPdb, CString& strErrors);

	//Easy Attachment 
	CString GetDMSConnectionString();
	
private:
	void			InitFunction (CFunctionDescription& aFunctionDescription) const;
	BOOL			CanTokenRunTB					();
	BOOL			FillCompanyUsers				(CLoginInfos* pLoginInfos);
	BOOL			FillCompanyRoles				(CLoginInfos* pLoginInfos);
	BOOL			FillUserRoles					(CLoginInfos* pLoginInfos);
	BOOL			FillDatabaseType				(CLoginInfos* pLoginInfos);

	BOOL			FillCompanyLanguages			(CLoginInfos* pLoginInfos);
	BOOL			FillCompanyDatabaseCulture		(CLoginInfos* pLoginInfos);

	void			FillUserInfoName				();
	void			FillUserInfo					();
	void			FillSystemDBConnectionString	();
	void			FillProductLanguage				();
	void			FillEdition						();
	void			FillSerialNumberType			();
	void			FillActivationInfoState			();

	ActivationState	GetActivationStrateFromString	(CString value);

public:
	SerialNumberType	GetSerialNumberType		();
	BOOL				CanUseNamespace			(const CString&	nameSpace, int grantType);

	CString				GetInstallationVersion	();
	DataDate			GetBuildDate			();
	CString				GetProducerName			() { return m_strBrandedProducerName; }

	void				AdvancedSendBalloon	
							(
								const CString&	sBodyMsg,
								const DataDate& dtExpireDate,
								BalloonMessageType mt,
								const CStringArray& recipients,
								BalloonMessageSensation ms,
								BOOL historicize,
								BOOL immediate,
								int closingTimer	//milliseconds
							);
	void				AdvancedSendTaggedBalloon
		(
			const CString&	sBodyMsg,
			const DataDate& dtExpireDate,
			BalloonMessageType mt,
			const CStringArray& recipients,
			BalloonMessageSensation ms,
			BOOL historicize,
			BOOL immediate,
			int closingTimer,	//milliseconds
			const CString&	strTag
			);

//deprecated methods:: you should use the code in the body of each method
TB_OLD_METHOD	CString	GetUserName										() { return AfxGetLoginInfos()->m_strUserName; }
TB_OLD_METHOD	CString	GetUserDescription								() { return AfxGetLoginInfos()->m_strUserDescription; }
TB_OLD_METHOD	int		GetLoginId										() { return AfxGetLoginInfos()->m_nLoginId; }
TB_OLD_METHOD	CString	GetCompanyName									() { return AfxGetLoginInfos()->m_strCompanyName; }
TB_OLD_METHOD	int		GetCompanyId									() { return AfxGetLoginInfos()->m_nCompanyId; }
TB_OLD_METHOD	BOOL	Admin											() { return AfxGetLoginInfos()->m_bAdmin;  }
TB_OLD_METHOD	CString	GetDBName										() { return AfxGetLoginInfos()->m_strDBName; }
TB_OLD_METHOD	CString	GetDBServer										() { return AfxGetLoginInfos()->m_strDBServer; }
TB_OLD_METHOD	int		GetProviderId									() { return AfxGetLoginInfos()->m_nProviderId; }
TB_OLD_METHOD	BOOL	Security										() { return AfxGetLoginInfos()->m_bSecurity;  }
TB_OLD_METHOD	BOOL	Auditing										() { return AfxGetLoginInfos()->m_bAuditing; }
TB_OLD_METHOD	BOOL	TransactionUse									() { return AfxGetLoginInfos()->m_bTransactionUse; }
TB_OLD_METHOD	BOOL	UseUnicode										() { return AfxGetLoginInfos()->m_bUseUnicode; }
TB_OLD_METHOD	CString	GetPreferredLanguage							() { return AfxGetLoginInfos()->m_strPreferredLanguage;  }
TB_OLD_METHOD	CString	GetApplicationLanguage							() { return AfxGetLoginInfos()->m_strApplicationLanguage; }
TB_OLD_METHOD	CString	GetProviderName									() { return AfxGetLoginInfos()->m_strProviderName;  }
TB_OLD_METHOD	CString	GetProviderDescription							() { return AfxGetLoginInfos()->m_strProviderDescription;  }
TB_OLD_METHOD	BOOL	UseConstParameter								() { return AfxGetLoginInfos()->m_bUseConstParameter;  }
TB_OLD_METHOD	BOOL	StripTrailingSpaces								() { return AfxGetLoginInfos()->m_bStripTrailingSpaces; }
TB_OLD_METHOD	CString	GetProviderCompanyConnectionString				() { return AfxGetLoginInfos()->m_strProviderCompanyConnectionString; }
TB_OLD_METHOD	CString	GetNonProviderCompanyConnectionString			() { return AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString; }
TB_OLD_METHOD	CString	GetDBUser										() { return AfxGetLoginInfos()->m_strDBUser; }
TB_OLD_METHOD	CString	GetProcessName									() { return AfxGetLoginInfos()->m_strProcessName; }
	
TB_OLD_METHOD	CString	GetCompanyLanguage								() { return AfxGetLoginInfos()->m_strCompanyLanguage; }
TB_OLD_METHOD	const CStringArray&	GetCompanyUsers						() { return AfxGetLoginInfos()->m_CompanyUsers; }
TB_OLD_METHOD	const CStringArray&	GetCompanyRoles						() { return AfxGetLoginInfos()->m_CompanyRoles; }
TB_OLD_METHOD	const CStringArray&	GetCurrentUserRoles					() { return AfxGetLoginInfos()->m_UserRoles; }
TB_OLD_METHOD	CString	GetDatabaseType									() { return AfxGetLoginInfos()->m_strDatabaseType; };
TB_OLD_METHOD	LCID	GetDataBaseCultureLCID							() const { return AfxGetLoginInfos()->m_wDataBaseCultureLCID; }
TB_OLD_METHOD	CString	GetAuthenticationToken							() { return AfxGetAuthenticationToken(); }

};



#include "Endh.dex"
