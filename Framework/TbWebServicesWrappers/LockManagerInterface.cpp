#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <tbgeneric\functioncall.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbOleDB\SqlLockManager.h>
#include <TbOleDB\SqlConnect.h>

#include <TbGenlibManaged\Main.h>

#include "LoginManagerInterface.h"
#include "LockManagerInterface.h"




//===========================================================================
//	CLockManagerInterface
//===========================================================================
//
//----------------------------------------------------------------------------
CLockManagerInterface::CLockManagerInterface(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort)
	:
	m_strService(strService),
	m_strServiceNamespace(strServiceNamespace),
	m_strServer(strServer),
	m_nWebServicesPort(nWebServicesPort),
	m_pTBLockManagerObj(NULL)
{
	InitializeLockSessionID();
}

//----------------------------------------------------------------------------
CLockManagerInterface::CLockManagerInterface(CLockManagerObject* pLockManagerObject)
	:
	m_nWebServicesPort(0),
	m_pTBLockManagerObj(pLockManagerObject)
{
	InitializeLockSessionID();
}


//----------------------------------------------------------------------------
CLockManagerInterface::~CLockManagerInterface()
{
	SAFE_DELETE(m_pTBLockManagerObj);
}

//----------------------------------------------------------------------------
void CLockManagerInterface::InitFunction(CFunctionDescription& aFunctionDescription) const
{
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::Init (const CString& strDBName)
{
	if (m_pTBLockManagerObj)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CFunctionDescription aFunctionDescription(_T("InitLock"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("companyDBName"), strDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//InitializeLockSessionID();

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
	
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::Init()
{
	if (m_pTBLockManagerObj)
		return m_pTBLockManagerObj->Init(AfxGetLoginInfos()->m_strUserName, AfxGetLoginInfos()->m_strProcessName, AfxGetAuthenticationToken(), _T(""));
	
	ASSERT(FALSE);
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockAllForCurrentConnection(const CString& strCompanyDBName)
{
	BOOL bResult = TRUE;
	if (m_pTBLockManagerObj)
	{
		bResult = m_pTBLockManagerObj->UnlockAllForCurrentConnection();
		//if (!bResult)
			//@@TODO gestione errore
			//m_pTBLockManagerObj->mLockManager->ErrorMessage
		return bResult;
	}
	else
	{
		CFunctionDescription aFunctionDescription(_T("UnlockAllForCurrentConnection"));
		InitFunction(aFunctionDescription);

		aFunctionDescription.AddStrParam(_T("companyDBName"), strCompanyDBName);
		aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

		aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

		if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
		return (pdbVal ? *pdbVal : FALSE);
	}
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::LockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress, CString& strLockMsg, CString strLockKeyDescription)
{
	BOOL bResult = FALSE;
	CString strLockUser, strLockApp;
	if (m_pTBLockManagerObj)
	{
		DataDate aLockerDate;
		bResult = m_pTBLockManagerObj->LockCurrent(strTableName, strLockKey, strAddress, strLockUser, strLockApp, aLockerDate);
	}
	else
	{
		CFunctionDescription aFunctionDescription(_T("LockRecordEx"));
		InitFunction(aFunctionDescription);

		DataStr aLockUser;
		DataStr aLockApp;
		aFunctionDescription.AddStrParam(_T("companyDBName"), strCompanyDBName);
		aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
		aFunctionDescription.AddStrParam(_T("userName"), AfxGetLoginInfos()->m_strUserName);
		aFunctionDescription.AddStrParam(_T("tableName"), strTableName);
		aFunctionDescription.AddStrParam(_T("lockKey"), strLockKey);
		aFunctionDescription.AddStrParam(_T("address"), strAddress);
		aFunctionDescription.AddStrParam(_T("processName"), AfxGetLoginInfos()->m_strProcessName);
		aFunctionDescription.AddOutParam(_T("lockUser"), &aLockUser);
		aFunctionDescription.AddOutParam(_T("lockApp"), &aLockApp);


		aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

		if (strLockKeyDescription.IsEmpty())
			strLockKeyDescription = strLockKey;
		if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
		{
			ASSERT(FALSE);
			strLockMsg = cwsprintf(_TB("The following error occurred calling LockManager::LockRecordEx function for data {0-%s} of {1-%s} table of {2-%s} database:\n{3-%s}\n\r"),
				(LPCTSTR)strLockKeyDescription,
				(LPCTSTR)strTableName,
				(LPCTSTR)strCompanyDBName,
				(LPCTSTR)aFunctionDescription.m_strError);
			return FALSE;
		}

		DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
		bResult = (pdbVal ? *pdbVal : FALSE);
		strLockUser = aLockUser.GetString();
		strLockApp = aLockApp.GetString();
	}

	if (!bResult && !strLockUser.IsEmpty() && !strLockApp.IsEmpty())
		strLockMsg = cwsprintf
		(
			_TB("Data {0-%s} of {1-%s} table of {2-%s} database has been locked by {3-%s} throught {4-%s}"),
			(LPCTSTR)strLockKeyDescription,
			(LPCTSTR)strTableName,
			(LPCTSTR)strCompanyDBName,
			(LPCTSTR)strLockUser,
			(LPCTSTR)strLockApp
			);

	return bResult;
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress)
{
	if (m_pTBLockManagerObj)	
		return m_pTBLockManagerObj->UnlockCurrent(strTableName, strLockKey, strAddress);
	
	CFunctionDescription aFunctionDescription(_T("UnlockRecord"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("tableName"),			strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),				strLockKey);
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::IsCurrentLocked(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress)
{
	if (m_pTBLockManagerObj)
		return m_pTBLockManagerObj->IsCurrentLocked(strTableName, strLockKey, strAddress);

	CFunctionDescription aFunctionDescription(_T("IsCurrentLocked"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),	strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("tableName"),		strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),			strLockKey);
	aFunctionDescription.AddStrParam(_T("address"),			strAddress);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::IsMyLock(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress)
{
	if (m_pTBLockManagerObj)
		return m_pTBLockManagerObj->IsMyLock(strTableName, strLockKey, strAddress);
	
	CFunctionDescription aFunctionDescription(_T("IsMyLock"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),	strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("tableName"),		strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),			strLockKey);
	aFunctionDescription.AddStrParam(_T("address"),			strAddress);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockAllContext(const CString& strCompanyDBName, const CString& strAddress)
{
	if (m_pTBLockManagerObj)
		return m_pTBLockManagerObj->UnlockAllContext(strAddress);

	CFunctionDescription aFunctionDescription(_T("UnlockAllContext"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"),	AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);


	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockAll(const CString& strCompanyDBName, const CString& strAddress, const CString& strTableName)
{
	if (m_pTBLockManagerObj)
	 return m_pTBLockManagerObj->UnlockAllTableContext(strTableName, strAddress);

	CFunctionDescription aFunctionDescription(_T("UnlockAll"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"),	AfxGetAuthenticationToken());	
	aFunctionDescription.AddStrParam(_T("tableName"),			strTableName);
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::GetLockInfo(const CString& strCompanyDBName, const CString& strLockKey, const CString& strTableName, DataStr& lockerUser, DataDate& lockTime, DataStr& processName)
{
	if (m_pTBLockManagerObj)
	{
		CString strLockerUser, lockerApp;	
		BOOL bResult = (m_pTBLockManagerObj->GetLockInfo(strTableName, strLockKey, strLockerUser, lockerApp, lockTime));
		if (bResult)
		{
			lockerUser.Assign(strLockerUser);
			processName.Assign(lockerApp);
		}
		return bResult;
	}


	CFunctionDescription aFunctionDescription(_T("GetLockInfo"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),	strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("lockKey"),			strLockKey);
	aFunctionDescription.AddStrParam(_T("tableName"),		strTableName);
	aFunctionDescription.AddOutParam(_T("user"),			&lockerUser);
	aFunctionDescription.AddOutParam(_T("lockTime"),		&lockTime);
	aFunctionDescription.AddOutParam(_T("processName"),		&processName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
void CLockManagerInterface::InitializeLockSessionID()
{
	m_sLockSessionID = GetLockSessionID();
}


//----------------------------------------------------------------------------
BOOL CLockManagerInterface::HasRestarted ()
{
	if (m_pTBLockManagerObj)
		return FALSE;

	return m_sLockSessionID != GetLockSessionID();
}

//----------------------------------------------------------------------------
const CString CLockManagerInterface::GetLockSessionID () const
{
	if (m_pTBLockManagerObj)
		return m_sLockSessionID;

	CFunctionDescription aFunctionDescription(_T("GetLockSessionID"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr* pGuid = (DataStr*) aFunctionDescription.GetReturnValue();
	return (pGuid ? *pGuid : _T(""));
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::LockDocument(const CString& strCompanyDBName, const CString& strDocumentNamespace, const CString& strAddress, CString& strLockMsg)
{
	BOOL bResult = FALSE;
	CString strLockUser, strLockApp;
	if (m_pTBLockManagerObj)
	{
		DataDate aLockerDate;
		bResult = m_pTBLockManagerObj->LockCurrent(strDocumentNamespace, _T("LOCK"), strAddress, strLockUser, strLockApp, aLockerDate);
	}
	else
	{
		CFunctionDescription aFunctionDescription(_T("LockRecordEx"));
		InitFunction(aFunctionDescription);

		DataStr aLockUser;
		DataStr aLockApp;
		aFunctionDescription.AddStrParam(_T("companyDBName"), strCompanyDBName);
		aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
		aFunctionDescription.AddStrParam(_T("userName"), AfxGetLoginInfos()->m_strUserName);
		aFunctionDescription.AddStrParam(_T("tableName"), strDocumentNamespace);
		aFunctionDescription.AddStrParam(_T("lockKey"), _T("LOCK"));
		aFunctionDescription.AddStrParam(_T("address"), strAddress);
		aFunctionDescription.AddStrParam(_T("processName"), AfxGetLoginInfos()->m_strProcessName);
		aFunctionDescription.AddOutParam(_T("lockUser"), &aLockUser);
		aFunctionDescription.AddOutParam(_T("lockApp"), &aLockApp);


		aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
		bResult = (pdbVal ? *pdbVal : FALSE);
		strLockUser = aLockUser.GetString();
		strLockApp = aLockApp.GetString();
	}

	if (!bResult && !strLockUser.IsEmpty() && !strLockApp.IsEmpty())
		strLockMsg = cwsprintf
		(
			_TB("This procedure can be run in Single User Mode only. You can not open it now, because in use by {0-%s}."),
			(LPCTSTR)strLockUser.GetString()
			);

	return bResult;
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockDocument(const CString& strCompanyDBName, const CString& strDocumentNamespace, const CString& strAddress)
{
	if (m_pTBLockManagerObj)
		return m_pTBLockManagerObj->UnlockCurrent(strCompanyDBName, _T("LOCK"), strAddress);

	CFunctionDescription aFunctionDescription(_T("UnlockRecord"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("tableName"),			strDocumentNamespace);
	aFunctionDescription.AddStrParam(_T("lockKey"),				_T("LOCK"));
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}
