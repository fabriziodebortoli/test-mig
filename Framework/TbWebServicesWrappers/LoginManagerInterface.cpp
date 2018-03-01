
#include "stdafx.h"

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\LoginContext.h>

#include <TBGeneric\FunctionCall.h>
#include <TbGenlibManaged\Main.h>

#include <TbXmlCore\XmlDocObj.h>

#include "LoginManagerInterface.h"

//----------------------------------------------------------------------------
CLoginManagerInterface::CLoginManagerInterface (const CString& strService, const CString& strServer, int nWebServicesPort)
:
	m_SerialNumberType          (CLoginManagerInterface::Undefined),
	m_ActivationState			(CLoginManagerInterface::NoActivated),
	m_bSecurityLightEnabled		(FALSE),
	m_strService				(strService),
	m_strServiceNamespace		(_T("http://microarea.it/LoginManager/")),
	m_strServer					(strServer),
	m_nWebServicesPort			(nWebServicesPort)

{
	m_bSecurityLightEnabled = IsActivated(_T("MicroareaConsole"), _T("SecurityLight"));
	
	//----
	CFunctionDescription aFunctionDescription(_T("GetInstallationVersion"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));
	
	aFunctionDescription.AddOutParam(_T("productName"), &m_InstallationVer.ProductName);
	aFunctionDescription.AddOutParam(_T("buildDate"),	&m_InstallationVer.BuildDate);
	aFunctionDescription.AddOutParam(_T("instDate"),	&m_InstallationVer.InstallationDate);
	aFunctionDescription.AddOutParam(_T("build"),		&m_InstallationVer.Build);
	
	VERIFY(InvokeWCFFunction(&aFunctionDescription, FALSE));
	
	m_InstallationVer.Version = *aFunctionDescription.GetReturnValue(); 
	//----
	
	m_strBrandedProducerName = GetBrandedProducerName ();
}

//----------------------------------------------------------------------------
void CLoginManagerInterface::InitFunction(CFunctionDescription& aFunctionDescription) const
{
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
}
//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::InitLogin (const CString&	sAuthenticationToken)
{
	if (sAuthenticationToken.IsEmpty())
		return FALSE;

	CLoginInfos& infos = AfxGetLoginContext()->m_LoginInfos;
	if	(!GetLoginInformation(sAuthenticationToken, &infos))
	{
		AfxGetDiagnostic()->Add(_TB("Error getting login information from LoginManager"));
		return FALSE;
	}

	if	(!FillCompanyDatabaseCulture (&infos))
	{
		AfxGetDiagnostic()->Add(_TB("FillCompanyDatabaseCulture: error getting Company Database Culture from LoginManager"));
		return FALSE;
	}

	if	(!FillDatabaseType (&infos))
	{
		AfxGetDiagnostic()->Add(_TB("FillDatabaseType: error getting Company Database Type from LoginManager"));
		return FALSE;
	}

	if	(!CanTokenRunTB())
	{
		AfxGetDiagnostic()->Add(_TB("Error : the current user is not allowed to execute the application"));
		return FALSE;
	}

	FillUserInfo();
	FillSystemDBConnectionString();

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetIToken()
{
	CFunctionDescription aFunctionDescription(_T("GetIToken"));

	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr* dsOut = (DataStr*)aFunctionDescription.GetReturnValue();

	return dsOut ? dsOut->Str() : _T("");
}

//----------------------------------------------------------------------------
CString	CLoginManagerInterface::GetUserDescriptionById(int loginId)
{
	CFunctionDescription aFunctionDescription(_T("GetUserDescriptionById"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddIntParam(_T("loginId"), loginId);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsDescription = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsDescription ? dsDescription->Str() : _T("");	
}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsUserLoggedByName(CString user)
{
	CFunctionDescription aFunctionDescription(_T("IsUserLoggedByName"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("userName"), user);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* dsDescription = (DataBool*)aFunctionDescription.GetReturnValue();
	
	return *dsDescription;
}


//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::ValidateUser(CString user, CString password, BOOL ntAuth)
{
	CFunctionDescription aFunctionDescription(_T("ValidateUser"));
	InitFunction(aFunctionDescription);
	 
	CStringArray& Companies = CStringArray();
	DataDate expiredDatePassword;
	DataInt loginId;
	DataBool userCannotChangePassword;
	DataBool userMustChangePassword;
	DataBool passwordNeverExpired;
	DataBool expiredDateCannotChange;

	DataArray ar(DataType::String);
	ar.Append(Companies);

	aFunctionDescription.AddStrParam(_T("userName"), user);
	aFunctionDescription.AddStrParam(_T("password"), password);
	aFunctionDescription.AddParam(_T("winNTAuthentication"), &DataBool(ntAuth));
	aFunctionDescription.AddOutParam(_T("userCompanies"), &ar);
	aFunctionDescription.AddOutParam(_T("loginId"), &loginId);
	aFunctionDescription.AddOutParam(_T("userCannotChangePassword"), &userCannotChangePassword);
	aFunctionDescription.AddOutParam(_T("userMustChangePassword"), &userMustChangePassword);
	aFunctionDescription.AddOutParam(_T("expiredDatePassword"), &expiredDatePassword);
	aFunctionDescription.AddOutParam(_T("passwordNeverExpired"), &passwordNeverExpired);
	aFunctionDescription.AddOutParam(_T("expiredDateCannotChange"), &expiredDateCannotChange);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Integer, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataInt* pdbVal = (DataInt*)aFunctionDescription.GetReturnValue();

	if (!pdbVal)
	{
		ASSERT(FALSE);
		return  FALSE;
	}
	return *pdbVal == 0;
}

//----------------------------------------------------------------------------
void CLoginManagerInterface::EnumCompanies(CString userName, CStringArray& ar)
{
	CFunctionDescription aFunctionDescription(_T("EnumCompanies"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("userName"), userName);

	DataArray companiesResult (DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(companiesResult);
	aFunctionDescription.SetReturnValueDescription(dod);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
	
	DataArray* companies = (DataArray*)aFunctionDescription.GetReturnValue();
	
	companies->ToStringArray(ar);
}



//----------------------------------------------------------------------------
CString	CLoginManagerInterface::GetUserDescriptionByName(CString login)
{
	CFunctionDescription aFunctionDescription(_T("GetUserDescriptionByName"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("login"), login);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsOut = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsOut ? dsOut->Str() : _T("");	
}
//----------------------------------------------------------------------------
CString	CLoginManagerInterface::GetUserEmailByName(CString login)
{
	CFunctionDescription aFunctionDescription(_T("GetUserEMailByName"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("login"), login);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsOut = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsOut ? dsOut->Str() : _T("");	
}
//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsWinNTUser(const CString& loginName)
{
	if (loginName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription(_T("IsIntegratedSecurityUser"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("userName"),	loginName);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	if (!pdbVal)
	{
		ASSERT(FALSE);
		return  FALSE;
	}

	return *pdbVal;
}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::GetLoginInformation(const CString&	authenticationToken, CLoginInfos* pLoginInfos)
{
	CFunctionDescription aFunctionDescription(_T("GetLoginInformation"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	DataStr 
		dsUserName, 
		dsUserDescription,
		dsCompanyName, 
		dsDBName,
		dsDBServer,
		dsPreferredLanguage, 
		dsApplicationLanguage,
		dsProviderName, 
		dsProviderDescription, 
		dsProviderCompanyConnectionString,
		dsNonProviderCompanyConnectionString,
		dsDBUser,
		dsProcessName,
		dsEmail;

	DataInt diLoginId, diCompanyId, diProviderId;
	
	DataBool
		dbAdmin, 
		ebDeveloper,
		dbSecurity, 
		dbAuditing,
		dbRowSecurity,
		dbUseKeyedUpdate, 
		dbTransactionUse,
		dbUseUnicode,
		dbUseConstParameter, 
		dbStripTrailingSpaces,
		dbDataSynchro;
				
	aFunctionDescription.AddOutParam(_T("userName"),							&dsUserName);
	aFunctionDescription.AddOutParam(_T("userDescription"),						&dsUserDescription);
	aFunctionDescription.AddOutParam(_T("loginId"),								&diLoginId);
	aFunctionDescription.AddOutParam(_T("companyName"),							&dsCompanyName);
	aFunctionDescription.AddOutParam(_T("companyId"),							&diCompanyId);
	aFunctionDescription.AddOutParam(_T("admin"),								&dbAdmin);
	aFunctionDescription.AddOutParam(_T("dbName"),								&dsDBName);
	aFunctionDescription.AddOutParam(_T("dbServer"),							&dsDBServer);
	aFunctionDescription.AddOutParam(_T("providerId"),							&diProviderId);
	aFunctionDescription.AddOutParam(_T("security"),							&dbSecurity);
	aFunctionDescription.AddOutParam(_T("auditing"),							&dbAuditing);
	aFunctionDescription.AddOutParam(_T("useKeyedUpdate"),						&dbUseKeyedUpdate); 
	aFunctionDescription.AddOutParam(_T("transactionUse"),						&dbTransactionUse);
	aFunctionDescription.AddOutParam(_T("useUnicode"),							&dbUseUnicode);
	aFunctionDescription.AddOutParam(_T("preferredLanguage"),					&dsPreferredLanguage); 
	aFunctionDescription.AddOutParam(_T("applicationLanguage"),					&dsApplicationLanguage);
	aFunctionDescription.AddOutParam(_T("providerName"),						&dsProviderName); 
	aFunctionDescription.AddOutParam(_T("providerDescription"),					&dsProviderDescription); 
	aFunctionDescription.AddOutParam(_T("useConstParameter"),					&dbUseConstParameter); 
	aFunctionDescription.AddOutParam(_T("stripTrailingSpaces"),					&dbStripTrailingSpaces);
	aFunctionDescription.AddOutParam(_T("providerCompanyConnectionString"),		&dsProviderCompanyConnectionString);
	aFunctionDescription.AddOutParam(_T("nonProviderCompanyConnectionString"),	&dsNonProviderCompanyConnectionString);
	aFunctionDescription.AddOutParam(_T("dbUser"),								&dsDBUser);
	aFunctionDescription.AddOutParam(_T("processName"),							&dsProcessName);
	aFunctionDescription.AddOutParam(_T("email"),								&dsEmail);
	aFunctionDescription.AddOutParam(_T("easyBuilderDeveloper"),				&ebDeveloper);
	aFunctionDescription.AddOutParam(_T("rowSecurity"),							&dbRowSecurity); 
	aFunctionDescription.AddOutParam(_T("dataSynchro"),							&dbDataSynchro);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	if (!pdbVal || !(*pdbVal))
	{
		ASSERT(FALSE);
		return  FALSE;
	}

	pLoginInfos->m_strUserName								=	dsUserName;
	pLoginInfos->m_strUserDescription						=	dsUserDescription;
	pLoginInfos->m_nLoginId									=	diLoginId;			
	pLoginInfos->m_strCompanyName							=	dsCompanyName;			
	pLoginInfos->m_nCompanyId								=	diCompanyId;			
	pLoginInfos->m_bAdmin									=	dbAdmin;
	pLoginInfos->m_bEasyBuilderDeveloper					=	ebDeveloper;
	pLoginInfos->m_strDBName								=	dsDBName;
	pLoginInfos->m_strDBServer								=	dsDBServer;
	pLoginInfos->m_nProviderId								=	diProviderId;
	pLoginInfos->m_bSecurity								=	dbSecurity;
	pLoginInfos->m_bAuditing								=	dbAuditing;
	pLoginInfos->m_bRowSecurity								=	dbRowSecurity;
	//pLoginInfos->m_bUseKeyedUpdate						=	dbUseKeyedUpdate; 
	pLoginInfos->m_bTransactionUse							=	dbTransactionUse;
	pLoginInfos->m_bUseUnicode								=	dbUseUnicode;
	pLoginInfos->m_strPreferredLanguage						=	dsPreferredLanguage; 
	pLoginInfos->m_strApplicationLanguage					=	dsApplicationLanguage;
	pLoginInfos->m_strProviderName							=	dsProviderName; 
	pLoginInfos->m_strProviderDescription					=	dsProviderDescription; 
	pLoginInfos->m_bUseConstParameter						=	dbUseConstParameter; 
	pLoginInfos->m_bStripTrailingSpaces						=	dbStripTrailingSpaces;
	pLoginInfos->m_strProviderCompanyConnectionString		=	dsProviderCompanyConnectionString;
	pLoginInfos->m_strNonProviderCompanyConnectionString	=	dsNonProviderCompanyConnectionString;
	pLoginInfos->m_strDBUser								=	dsDBUser;
	pLoginInfos->m_strProcessName							=	dsProcessName;
	pLoginInfos->m_strUserEmail								=	dsEmail;
	pLoginInfos->m_bDataSynchro								=	dbDataSynchro;

	pLoginInfos->m_strDatabaseType.Empty();
	pLoginInfos->m_bCanAccessWebSitePrivateArea				= UserCanAccessWebSitePrivateArea();
	
	BOOL bOk = FillCompanyUsers(pLoginInfos);
	bOk = FillCompanyRoles(pLoginInfos) && bOk;
	bOk = FillUserRoles(pLoginInfos) && bOk;	
	bOk = FillCompanyLanguages(pLoginInfos) && bOk;
	
	return bOk;
}

//----------------------------------------------------------------------------
int	CLoginManagerInterface::GetCompanyLoggedUsersNumber(int companyId)
{
	CFunctionDescription aFunctionDescription(_T("GetCompanyLoggedUsersNumber"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddIntParam(_T("companyId"), companyId);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Integer, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return 0;
	}
	
	DataInt* nUsers = (DataInt*)aFunctionDescription.GetReturnValue();
	
	return nUsers ? *nUsers : 0;

}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::CanTokenRunTB()
{
	CFunctionDescription aFunctionDescription(_T("Sbrill"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("token"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);

		m_SerialNumberType = CLoginManagerInterface::Undefined;
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::DownloadPdb (const CString& strPdb, CString& strErrors)
{
	CFunctionDescription aFunctionDescription(_T("DownloadPdb"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("PdbFile"), strPdb);

	DataStr sError;
	aFunctionDescription.AddOutParam(_T("ErrorMessage"), &sError);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrors = aFunctionDescription.GetError();
		ASSERT(FALSE);
		return FALSE;
	}

	strErrors = sError;
		
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::SendErrorFile (const CString& strLogFilePath, CString& strErrors)
{
	CFunctionDescription aFunctionDescription(_T("SendErrorFile"));
	
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("LogFile"), strLogFilePath);
	
	DataStr sError;
	aFunctionDescription.AddOutParam(_T("ErrorMessage"), &sError);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrors = aFunctionDescription.GetError();
		ASSERT(FALSE);
		return FALSE;
	}

	strErrors = sError;
		
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	return (pdbVal ? *pdbVal : FALSE);
}


//----------------------------------------------------------------------------
CString CLoginManagerInterface::GetLoggedUsersAdvanced()
{

	CFunctionDescription aFunctionDescription(_T("GetLoggedUsersAdvanced"));

	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("token"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		return FALSE;
	}

	DataStr* dsOut = (DataStr*)aFunctionDescription.GetReturnValue();


	return dsOut ? dsOut->Str() : _T("");
}

//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsValidToken(const CString& strToken, bool& isValid)
{ 
	isValid = true;
	CFunctionDescription aFunctionDescription(_T("IsValidToken"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), strToken);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	if (pdbVal)
		isValid = (*pdbVal) ? true : false;
	return pdbVal != NULL;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetDMSConnectionString()
{
	CFunctionDescription aFunctionDescription(_T("GetDMSConnectionString"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsOut = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsOut ? dsOut->Str() : _T("");	
}

//-----------------------------------------------------------------------------
CLoginManagerInterface::SerialNumberType CLoginManagerInterface::GetSerialNumberType ()
{
	return m_SerialNumberType;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetInstallationVersion()
{
	return m_InstallationVer.Version.Str();
}

//-----------------------------------------------------------------------------
DataDate CLoginManagerInterface::GetBuildDate()
{
	return m_InstallationVer.BuildDate;
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillSerialNumberType()
{
	// to get info from LoginManager
	CFunctionDescription aFunctionDescription(_T("CacheCounterGTG"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));
		
	if (InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		DataStr* pdbVal	= (DataStr*) aFunctionDescription.GetReturnValue();
		if (pdbVal)
		{

			if (_tcsicmp(pdbVal->GetString (), _T("Normal")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Normal;
			else if (_tcsicmp(pdbVal->GetString (), _T("Development")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString (), _T("DevelopmentIU")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString (), _T("DevelopmentPlus")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString (), _T("PersonalPlusUser")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString (), _T("DevelopmentPlusUser")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString(), _T("DevelopmentPlusK")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString(), _T("PersonalPlusK")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Development;
			else if (_tcsicmp(pdbVal->GetString (), _T("Distributor")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Distributor;
			else if (_tcsicmp(pdbVal->GetString (), _T("Reseller")) == 0)
				m_SerialNumberType = CLoginManagerInterface::Reseller;
			else 
				m_SerialNumberType = CLoginManagerInterface::Undefined;
		}
	}
	else
	{
		ASSERT(FALSE);
		m_SerialNumberType = CLoginManagerInterface::Undefined;
	}
}

//----------------------------------------------------------------------------
CLoginManagerInterface::ActivationState	CLoginManagerInterface::GetActivationState ()
{
	return m_ActivationState;
}

//----------------------------------------------------------------------------
const BOOL	CLoginManagerInterface::IsADemo () const
{
	return m_ActivationState == CLoginManagerInterface::Demo || m_ActivationState == CLoginManagerInterface::DemoWarning;
}

//----------------------------------------------------------------------------
const BOOL CLoginManagerInterface::IsActivationForDevelopment ()
{
	return m_SerialNumberType == CLoginManagerInterface::Development;

}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::InitActivationStateInfo ()
{
	FillActivationInfoState();
	FillSerialNumberType();
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillActivationInfoState()
{
	m_ActivationState = CLoginManagerInterface::NoActivated;

	CFunctionDescription aFunctionDescription(_T("SetCurrentComponents"));
	InitFunction(aFunctionDescription);

	DataInt diDaysToExpiration;
	aFunctionDescription.AddOutParam(_T("dte"),	&diDaysToExpiration);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}

	DataStr* pDS = (DataStr*)aFunctionDescription.GetReturnValue();
	if (pDS == NULL)
	{
		m_ActivationState = CLoginManagerInterface::NoActivated;
		return;
	}

	m_ActivationState = GetActivationStrateFromString(*pDS);
}

//-----------------------------------------------------------------------------
CLoginManagerInterface::ActivationState	CLoginManagerInterface::GetActivationStrateFromString(CString value)
{
	if (_tcsicmp(value, _T("Demo")) == 0)
		return CLoginManagerInterface::Demo;

	if (_tcsicmp(value, _T("DemoWarning")) == 0)
		return CLoginManagerInterface::DemoWarning;

	if (_tcsicmp(value, _T("SilentWarning")) == 0)
		return CLoginManagerInterface::SilentWarning;
	
	if (_tcsicmp(value, _T("Warning")) == 0)
		return CLoginManagerInterface::Warning;

	if (_tcsicmp(value, _T("Activated")) == 0)
		return CLoginManagerInterface::Activated;

	if (_tcsicmp(value, _T("NoActivated")) == 0)
		return CLoginManagerInterface::NoActivated;

	if (_tcsicmp(value, _T("Disabled")) == 0)
		return CLoginManagerInterface::Disabled;

	return CLoginManagerInterface::NoActivated;
}


//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetMobileToken(CString token, int loginType)
{
	CFunctionDescription aFunctionDescription(_T("GetMobileToken"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("token"), token);
	aFunctionDescription.AddParam   (_T("loginType"), &DataInt((int)loginType));

	InvokeWCFFunction(&aFunctionDescription, FALSE);

	return ((DataStr*)aFunctionDescription.GetReturnValue())->Str();
}


//-----------------------------------------------------------------------------
void CLoginManagerInterface::PurgeMessageByTag(CString tag, CString user)
{
	CFunctionDescription aFunctionDescription(_T("PurgeMessageByTag"));

	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("tag"), tag);
	aFunctionDescription.AddStrParam(_T("user"), user);

	InvokeWCFFunction(&aFunctionDescription, FALSE);

}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::LogOff()
{
	CFunctionDescription aFunctionDescription(_T("LogOff"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	InvokeWCFFunction(&aFunctionDescription, FALSE);
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::LogOff(CString token)
{
	CFunctionDescription aFunctionDescription(_T("LogOff"));

	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), token);

	InvokeWCFFunction(&aFunctionDescription, FALSE);
}


//-----------------------------------------------------------------------------
int CLoginManagerInterface::LogIn(const CString& sCompany, const CString& sUser, const CString& sPassword, BOOL overWriteLogin, CString& authToken)
{
	CFunctionDescription aFunctionDescription(_T("LoginCompact"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("userName"), sUser);
	aFunctionDescription.AddStrParam(_T("companyName"), sCompany);
	aFunctionDescription.AddStrParam(_T("password"), sPassword);
	aFunctionDescription.AddStrParam(_T("askingProcess"), _T("MenuManager"));
	aFunctionDescription.AddParam(_T("overWriteLogin"), &DataBool(overWriteLogin));
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Integer, CDataObjDescription::_OUT));
	DataStr t;
	aFunctionDescription.AddOutParam(_T("authenticationToken"), &t);
	InvokeWCFFunction(&aFunctionDescription, FALSE);

	 authToken = t;
	 DataInt* pdbVal = (DataInt*)aFunctionDescription.GetReturnValue();
	 return (int)(*pdbVal);

}

//-----------------------------------------------------------------------------
int CLoginManagerInterface::GetTokenProcessType()
{
	CFunctionDescription aFunctionDescription(_T("GetTokenProcessType"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("token"), AfxGetAuthenticationToken());
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Integer, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);

		return TRUE;//TODO RICCARDO
	}

	DataInt* pdbVal = (DataInt*)aFunctionDescription.GetReturnValue();
	return (int) (*pdbVal);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FeUsed()
{

	CFunctionDescription aFunctionDescription(_T("FeUsed"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return TRUE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsCalAvailable(const CString& strApplication, const CString& strModuleOrFunctionality)
{
	if(_tcsicmp(strModuleOrFunctionality, _T("Free")) == 0)
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("IsCalAvailable"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("application"),			strApplication);
	aFunctionDescription.AddStrParam(_T("functionality"),		strModuleOrFunctionality);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);

		return TRUE;//TODO RICCARDO
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsValidDate(const DataDate operationDate, DataDate& maxDate)
{
	
	CFunctionDescription aFunctionDescription(_T("IsValidDate"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("operationDate"), (LPCTSTR)	operationDate.FormatDataForXML());

	DataStr maxDateString = (LPCTSTR)maxDate.FormatDataForXML();
	aFunctionDescription.AddOutParam(_T("maxDate"), &maxDateString);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return TRUE;//?
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();


	maxDate.AssignFromXMLString( (LPCTSTR)maxDateString.Str());
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::SetCompanyInfo (const CString& strName, const CString& strValue)
{
	CFunctionDescription aFunctionDescription(_T("SetCompanyInfo"));
	
	InitFunction(aFunctionDescription);
		
	aFunctionDescription.AddStrParam(_T("authToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("aName"), strName);
	aFunctionDescription.AddStrParam(_T("aValue"), strValue);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::IsActivated(const CString& strApplication, const CString& strModuleOrFunctionality)
{
	if (_tcsicmp(strModuleOrFunctionality, _T("Free")) == 0)
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("IsActivated"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("application"),			strApplication);
	aFunctionDescription.AddStrParam(_T("functionality"),		strModuleOrFunctionality);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::GetActivatedModules (CStringArray& arModules)
{
	CFunctionDescription aFunctionDescription(_T("GetModules"));
	
	InitFunction(aFunctionDescription);

	DataArray darModules(DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(darModules);
	aFunctionDescription.SetReturnValueDescription(dod);

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataArray * pAr = (DataArray *) aFunctionDescription.GetReturnValueDescription().GetValue();
	if (pAr->GetSize() == 0)
		arModules.RemoveAll ();
	else
		pAr->ToStringArray (arModules);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::GetAuthenticationInformations(const CString& strAuthenticationToken, DataStr& strLoginName, DataStr& strCompName)
{
	if (strAuthenticationToken.IsEmpty())
		return FALSE;
	
	CFunctionDescription aFunctionDescription(_T("GetLoginCompanyName"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), strAuthenticationToken);

	aFunctionDescription.AddOutParam(_T("loginName"),	&strLoginName);
	aFunctionDescription.AddOutParam(_T("companyName"),	&strCompName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillCompanyUsers(CLoginInfos* pLoginInfos)
{
	if (pLoginInfos->m_strCompanyName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription	(_T("GetCompanyUsers"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyName"),	pLoginInfos->m_strCompanyName);
	
	DataArray ar (DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(ar);
	aFunctionDescription.SetReturnValueDescription(dod);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	pLoginInfos->m_CompanyUsers.RemoveAll();
	
	DataArray* pdoaUsers = (DataArray*) aFunctionDescription.GetReturnValueDescription().GetValue();
	if(!pdoaUsers)
	{
		ASSERT(FALSE);	
		return FALSE;
	}

	pdoaUsers->Sort();
	pdoaUsers->ToStringArray(pLoginInfos->m_CompanyUsers);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillCompanyRoles(CLoginInfos* pLoginInfos)
{
	if (pLoginInfos->m_strCompanyName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription	(_T("GetCompanyRoles"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyName"),	pLoginInfos->m_strCompanyName);
	
	DataArray ar (DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(ar);
	aFunctionDescription.SetReturnValueDescription(dod);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	pLoginInfos->m_CompanyRoles.RemoveAll();
	
	DataArray* pdoaUsers = (DataArray*) aFunctionDescription.GetReturnValueDescription().GetValue();
	if(!pdoaUsers)
	{
		ASSERT(FALSE);	
		return FALSE;
	}

	pdoaUsers->ToStringArray(pLoginInfos->m_CompanyRoles);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillUserRoles(CLoginInfos* pLoginInfos)
{
	return GetUserRoles(pLoginInfos->m_strUserName, pLoginInfos->m_strCompanyName, pLoginInfos->m_UserRoles);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillCompanyLanguages (CLoginInfos* pLoginInfos)
{
	CFunctionDescription aFunctionDescription	(_T("GetCompanyLanguage"));
	InitFunction(aFunctionDescription);
	
	DataStr sLanguage, sCulture;
	aFunctionDescription.AddIntParam(_T("companyID"),	pLoginInfos->m_nCompanyId);
	aFunctionDescription.AddOutParam(_T("cultureUI"),	&sLanguage);
	aFunctionDescription.AddOutParam(_T("culture"),		&sCulture);
	
	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	pLoginInfos->m_strCompanyLanguage = sLanguage;
	pLoginInfos->m_strCompanyApplicationLanguage = sCulture;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::GetCompanyUsersWithoutEasyLookSystem (CStringArray& ar)
{
	CString strEasyLookSystemUser = _T("EasyLookSystem");
	for (int i = 0; i < ar.GetSize(); i++)
	{
		if (_tcsicmp(ar.GetAt(i), strEasyLookSystemUser) == 0)
		{
			ar.RemoveAt(i);		
			return;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::GetUserRoles(const CString strUser, const CString strCompanyName, CStringArray& arUserRoles)
{
	if (strCompanyName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription	(_T("GetUserRoles"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyName"),	strCompanyName);
	aFunctionDescription.AddStrParam(_T("userName"),	strUser);
	
	DataArray ar (DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(ar);
	aFunctionDescription.SetReturnValueDescription(dod);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	arUserRoles.RemoveAll();
	
	DataArray* pdoaUsers = (DataArray*) aFunctionDescription.GetReturnValueDescription().GetValue();
	if(!pdoaUsers)
	{
		ASSERT(FALSE);	
		return FALSE;
	}

	pdoaUsers->ToStringArray(arUserRoles);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::GetRoleUsers(const CString& strCompany, const CString& strRole, CStringArray& roleUsers)
{
	roleUsers.RemoveAll();

	if (strCompany.IsEmpty() ||strRole.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription	(_T("GetRoleUsers"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyName"),	strCompany);
	aFunctionDescription.AddStrParam(_T("roleName"),	strRole);
	
	DataArray ar (DataType::String);
	CDataObjDescription dod(_T(""), DataType::String, CDataObjDescription::_OUT);
	dod.SetValue(ar);
	aFunctionDescription.SetReturnValueDescription(dod);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
		
	DataArray* pdoaUsers = (DataArray*) aFunctionDescription.GetReturnValueDescription().GetValue();
	if(!pdoaUsers)
	{
		ASSERT(FALSE);	
		return FALSE;
	}

	pdoaUsers->ToStringArray(roleUsers);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillSystemDBConnectionString()
{
	CFunctionDescription aFunctionDescription(_T("GetSystemDBConnectionString"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
	DataStr* pStr = (DataStr*)aFunctionDescription.GetReturnValue();
	if (pStr && !pStr->IsEmpty())
	{
		SetAdminAuthenticationToken(*pStr);
		m_strSysDbConnectionString = *pStr;
	}
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetSystemDBConnectionString()
{
	return m_strSysDbConnectionString;
	
	
}

// dato l'id della company mi restituisce il dbowner (non l'utente applicativo ma l'utente di database)
//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetDbOwner(int companyId)
{
	CFunctionDescription aFunctionDescription(_T("GetDbOwner"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddIntParam(_T("companyId"), companyId);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsDbOnwer = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsDbOnwer ? dsDbOnwer->Str() : _T("");
}
//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetProductLanguage()
{
	if (m_strProductLanguage.IsEmpty())
		FillProductLanguage();
	
	return m_strProductLanguage; 
}
//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillProductLanguage()
{
	CFunctionDescription aFunctionDescription(_T("GetCountry"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
	
	DataStr* dsCountry = (DataStr*)aFunctionDescription.GetReturnValue();
	
	if (dsCountry)
		m_strProductLanguage = *dsCountry;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetEdition()
{
	if (m_strEdition.IsEmpty ())
		FillEdition();
	
	return m_strEdition;
}
//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetEditionType()
{
	CFunctionDescription aFunctionDescription(_T("GetEditionType"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr* edition = (DataStr*)aFunctionDescription.GetReturnValue();

	return edition->Str();
}
//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillEdition()
{ 
	CFunctionDescription aFunctionDescription(_T("GetEdition"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
	}
	
	DataStr* dsEdition = (DataStr*)aFunctionDescription.GetReturnValue();
	if (dsEdition)
		m_strEdition = *dsEdition;
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillUserInfo()
{
	CFunctionDescription aFunctionDescription(_T("GetUserInfo"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
	
	DataStr* dsUserInfoString = (DataStr*)aFunctionDescription.GetReturnValue();
	if (dsUserInfoString)
		m_strUserInfoLicensee = *dsUserInfoString;

	FillUserInfoName();
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetUserInfo()
{
	return m_strUserInfoLicensee;
}

//-----------------------------------------------------------------------------
void CLoginManagerInterface::FillUserInfoName()
{
	CXMLDocumentObject doc;
	doc.LoadXML(m_strUserInfoLicensee);
	CXMLNode* pUserInfoNameNode = doc.SelectSingleNode(_T("/UserInfo/Name"));
	if (!pUserInfoNameNode)
		return;

	pUserInfoNameNode->GetText(m_strUserInfoName);
	delete pUserInfoNameNode;

	CXMLNode* pNode = doc.SelectSingleNode(_T("/UserInfo/UserIdInfos/UserId"));
	if (!pNode)
		return;

	pNode->GetAttribute(_T("internalcode"), m_strUserInfoId);
	delete pNode;
}
//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetUserInfoName()
{
	return m_strUserInfoName;
}
//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetUserInfoId()
{
	return m_strUserInfoId;
}
//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillDatabaseType(CLoginInfos* pLoginInfos)
{ 
	CFunctionDescription aFunctionDescription(_T("GetDatabaseType"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("providerName"), pLoginInfos->m_strProviderName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataStr* dsDBType = (DataStr*)aFunctionDescription.GetReturnValue();


	pLoginInfos->m_strDatabaseType = dsDBType ? dsDBType->Str() : _T("");

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetBrandedApplicationTitle(const CString& strApplicationName)
{
	CFunctionDescription aFunctionDescription(_T("GetBrandedApplicationTitle"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("application"), strApplicationName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsBrandedApplicationName = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsBrandedApplicationName ? dsBrandedApplicationName->Str() : strApplicationName;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetBrandedProducerName()
{
	CFunctionDescription aFunctionDescription(_T("GetBrandedProducerName"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* pdsBrandedApplicationName = (DataStr*)aFunctionDescription.GetReturnValue();
	ASSERT(pdsBrandedApplicationName);
	return pdsBrandedApplicationName->Str();
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetMasterProductBrandedName ()
{
	CFunctionDescription aFunctionDescription(_T("GetMasterProductBrandedName"));
	InitFunction(aFunctionDescription);

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		//ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsBrandedApplicationName = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsBrandedApplicationName ? dsBrandedApplicationName->Str() : _T("");
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetBrandedKey(const CString& source)
{
	CFunctionDescription aFunctionDescription(_T("GetBrandedKey"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("source"), source);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}
	
	DataStr* dsBrandedApplicationName = (DataStr*)aFunctionDescription.GetReturnValue();
	
	return dsBrandedApplicationName ? dsBrandedApplicationName->Str() : source;
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::CanUseNamespace(const CString&	nameSpace, int grantType)
{
	CFunctionDescription aFunctionDescription(_T("CanUseNamespace"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("nameSpace"), nameSpace);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	// OSL_GRANT_SILENTEXECUTE definition is not available 
	aFunctionDescription.AddStrParam(_T("grantType"), (grantType == 512) ? _T("SilentMode") : _T("Execute"));

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//-----------------------------------------------------------------------------
BOOL CLoginManagerInterface::FillCompanyDatabaseCulture(CLoginInfos* pLoginInfos)
{
	CFunctionDescription aFunctionDescription(_T("GetDBCultureLCID"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddIntParam(_T("companyID"), pLoginInfos->m_nCompanyId);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Long, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	long  lLCID = *((DataLng*) aFunctionDescription.GetReturnValue());
	pLoginInfos->m_wDataBaseCultureLCID = lLCID < 0 ? 0 : lLCID;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CLoginManagerInterface::GetAspNetUser() const
{
	CFunctionDescription aFunctionDescription(_T("GetAspNetUser"));
	
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	VERIFY(InvokeWCFFunction(&aFunctionDescription, FALSE));
	return aFunctionDescription.GetReturnValue()->Str();
}

// Dice se un utente è amministatore dell'Area Riservata su sito web
//----------------------------------------------------------------------------
BOOL CLoginManagerInterface::UserCanAccessWebSitePrivateArea()
{
	CFunctionDescription aFunctionDescription(_T("UserCanAccessWebSitePrivateArea"));
	InitFunction(aFunctionDescription);

	CLoginInfos& infos = AfxGetLoginContext()->m_LoginInfos;
	aFunctionDescription.AddIntParam(_T("loginID"), infos.m_nLoginId);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	return *((DataBool*) aFunctionDescription.GetReturnValue());
}




//----------------------------------------------------------------------------
void CLoginManagerInterface::AdvancedSendTaggedBalloon
(
	const CString&	sBodyMsg,
	const DataDate& dtExpireDate,
	BalloonMessageType mt,
	const CStringArray& recipients,
	BalloonMessageSensation ms,
	BOOL historicize,
	BOOL immediate,
	int closingTimer,	//milliseconds
	const CString& strTag)
{
	CFunctionDescription aFunctionDescription(_T("AdvancedSendTaggedBalloon"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.AddStrParam(_T("bodyMessage"), sBodyMsg);
	aFunctionDescription.AddParam(_T("expiryDate"), const_cast<DataDate*>(&dtExpireDate));
	aFunctionDescription.AddParam(_T("messageType"), &DataLng((int)mt));

	DataArray ar(DataType::String);
	ar.Append(recipients);
	aFunctionDescription.AddParam(_T("recipients"), &ar);

	aFunctionDescription.AddParam(_T("sensation"), &DataLng((int)ms));
	aFunctionDescription.AddParam(_T("historicize"), &DataBool(historicize));
	aFunctionDescription.AddParam(_T("immediate"), &DataBool(immediate));
	aFunctionDescription.AddParam(_T("timer"), &DataLng((int)closingTimer));
	aFunctionDescription.AddStrParam(_T("tag"), strTag);


	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
}

//----------------------------------------------------------------------------
void CLoginManagerInterface::AdvancedSendBalloon	
								(
									const CString&	sBodyMsg,
									const DataDate& dtExpireDate,
									BalloonMessageType mt,
									const CStringArray& recipients,
									BalloonMessageSensation ms,
									BOOL historicize,
									BOOL immediate,
									int closingTimer	//milliseconds
								)
{
	CFunctionDescription aFunctionDescription(_T("AdvancedSendBalloon"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.AddStrParam	(_T("bodyMessage"), sBodyMsg);
	aFunctionDescription.AddParam		(_T("expiryDate"),	const_cast<DataDate*>(&dtExpireDate));
	aFunctionDescription.AddParam		(_T("messageType"), &DataLng((int)mt));

	DataArray ar (DataType::String);
	ar.Append(recipients);
	aFunctionDescription.AddParam		(_T("recipients"),	&ar);

	aFunctionDescription.AddParam		(_T("sensation"),	&DataLng((int)ms));
	aFunctionDescription.AddParam		(_T("historicize"),	&DataBool(historicize));
	aFunctionDescription.AddParam		(_T("immediate"),	&DataBool(immediate));
	aFunctionDescription.AddParam		(_T("timer"),		&DataLng((int)closingTimer));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
}

//=============================================================================
