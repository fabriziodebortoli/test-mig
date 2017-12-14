
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <tbgeneric\functioncall.h>
#include <TbGenlibManaged\Main.h>

#include "LoginManagerInterface.h"
#include "LockManagerInterface.h"

//----------------------------------------------------------------------------
CLockManagerInterface::CLockManagerInterface(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort)
:
	m_strService(strService),
	m_strServiceNamespace(strServiceNamespace),
	m_strServer(strServer),
	m_nWebServicesPort(nWebServicesPort)
{
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
BOOL CLockManagerInterface::Init (const CString& strCompanyDBName)
{
	CFunctionDescription aFunctionDescription	(_T("InitLock"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
		
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
		{
		ASSERT(FALSE);
		return FALSE;
}

	InitializeLockSessionID();

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockAllForCurrentConnection(const CString& strCompanyDBName)
{
	CFunctionDescription aFunctionDescription(_T("UnlockAllForCurrentConnection"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

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
BOOL CLockManagerInterface::LockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress, CString& strLockMsg, CString strLockKeyDescription)
{
	CFunctionDescription aFunctionDescription(_T("LockRecordEx"));
	InitFunction(aFunctionDescription);

	DataStr aLockUser;
	DataStr aLockApp;
	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("userName"),			AfxGetLoginInfos()->m_strUserName);
	aFunctionDescription.AddStrParam(_T("tableName"),			strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),				strLockKey);	
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);
	aFunctionDescription.AddStrParam(_T("processName"),			AfxGetLoginInfos()->m_strProcessName);
	aFunctionDescription.AddOutParam(_T("lockUser"),			&aLockUser);
	aFunctionDescription.AddOutParam(_T("lockApp"),				&aLockApp);


	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		strLockMsg = cwsprintf(_TB("The following error occurred calling LockManager::LockRecordEx function for data {0-%s} of {1-%s} table of {2-%s} database:\n{3-%s}\n\r"),
			(LPCTSTR)strLockKeyDescription,
			(LPCTSTR)strTableName,
			(LPCTSTR)strCompanyDBName,
						(LPCTSTR) aFunctionDescription.m_strError);
		return FALSE;
	}
	if (strLockKeyDescription.IsEmpty())
		strLockKeyDescription = strLockKey;

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aLockUser.IsEmpty() && !aLockApp.IsEmpty())
		strLockMsg = cwsprintf
		(
			_TB("Data {0-%s} of {1-%s} table of {2-%s} database has been locked by {3-%s} throught {4-%s}"),
			(LPCTSTR)strLockKeyDescription,
			(LPCTSTR)strTableName,
			(LPCTSTR)strCompanyDBName,
			(LPCTSTR)aLockUser.GetString(),
			(LPCTSTR)aLockApp.GetString()
			);

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress)
{
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
BOOL CLockManagerInterface::IsCurrentLocked(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strContextAddress)
{
	CFunctionDescription aFunctionDescription(_T("IsCurrentLocked"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),	strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("tableName"),		strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),			strLockKey);
	aFunctionDescription.AddStrParam(_T("address"),			strContextAddress);

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
BOOL CLockManagerInterface::IsMyLock(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strContextAddress)
{
	CFunctionDescription aFunctionDescription(_T("IsMyLock"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.AddStrParam(_T("companyDBName"),	strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("tableName"),		strTableName);
	aFunctionDescription.AddStrParam(_T("lockKey"),			strLockKey);
	aFunctionDescription.AddStrParam(_T("address"),			strContextAddress);

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
		return DataDate::MAXVALUE;
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
	return m_sLockSessionID != GetLockSessionID();
}

//----------------------------------------------------------------------------
const CString CLockManagerInterface::GetLockSessionID () const
{
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

	CFunctionDescription aFunctionDescription(_T("LockRecordEx"));
	InitFunction(aFunctionDescription);

	DataStr aLockUser;
	DataStr aLockApp;
	aFunctionDescription.AddStrParam(_T("companyDBName"),		strCompanyDBName);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("userName"),			AfxGetLoginInfos()->m_strUserName);
	aFunctionDescription.AddStrParam(_T("tableName"),			strDocumentNamespace);
	aFunctionDescription.AddStrParam(_T("lockKey"),				_T("LOCK"));	
	aFunctionDescription.AddStrParam(_T("address"),				strAddress);
	aFunctionDescription.AddStrParam(_T("processName"),			AfxGetLoginInfos()->m_strProcessName);
	aFunctionDescription.AddOutParam(_T("lockUser"),			&aLockUser);
	aFunctionDescription.AddOutParam(_T("lockApp"),				&aLockApp);


	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aLockUser.IsEmpty() && !aLockApp.IsEmpty())
		strLockMsg = cwsprintf
		(
			_TB("This procedure can be run in Single User Mode only. You can not open it now, because in use by {0-%s}."),
			(LPCTSTR)aLockUser.GetString()
			);

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CLockManagerInterface::UnlockDocument(const CString& strCompanyDBName, const CString& strDocumentNamespace, const CString& strAddress)
{

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
