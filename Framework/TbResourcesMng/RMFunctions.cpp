#include "stdafx.h"

#include <TbGes\NumbererService.h>
#include <TbClientCore\GlobalFunctions.h>
#include <TbGeneric\CMapi.h>

#include "TResources.h"
#include "RMFunctions.h"

#ifdef _DEBUG
#undef THIS_FILE
static char  THIS_FILE[] = __FILE__;
#endif

// Parametri query
static TCHAR szResourceCode		[] = _T("p01");
static TCHAR szResourceType		[] = _T("p02");
static TCHAR szParamWorkerID	[] = _T("p03");
static TCHAR szParamUserName	[] = _T("p04");
static TCHAR szParamName		[] = _T("p05");
static TCHAR szParamLastName	[] = _T("p06");
static TCHAR szParamNotes		[] = _T("p07");

///////////////////////////////////////////////////////////////////////////////
//					CCheckWorker implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CCheckWorker, CObject)

//-----------------------------------------------------------------------------
CCheckWorker::CCheckWorker()
{}

//-----------------------------------------------------------------------------
DataLng CCheckWorker::GetWorkerID()
{
	DataLng aWorkerID = 0;
	CString aUserName = AfxGetLoginInfos()->m_strUserName;
	CString aTooltip = cwsprintf(_TB("User: %s"), aUserName);

	m_WorkerDescription.Clear();
	m_WorkerLastName.Clear();

	TWorkers aRec;
	SqlTable aTbl(&aRec, AfxGetDefaultSqlConnection()->GetDefaultSqlSession());

	aTbl.Open();
	aTbl.Select(aRec.f_WorkerID);

	aTbl.AddParam(szParamUserName, aRec.f_CompanyLogin);
	aTbl.AddFilterColumn(aRec.f_CompanyLogin);
	aTbl.SetParamValue(szParamUserName, DataStr(aUserName));

	TRY
	{
		aTbl.Query();
		// se il worker non esiste lo creo al volo altrimenti prendo il primo
		// (dalla 4.0 la gestione multi-login non esiste piu', nel dubbio prendo il primo)
		aWorkerID = (aTbl.HasRows()) ? aRec.f_WorkerID : CreateWorker();
		aTbl.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTbl.IsOpen()) aTbl.Close();
		aWorkerID = 0;
		e->ShowError();
	}
	END_CATCH

	AfxSetUserPaneText(aUserName, aTooltip);

	return aWorkerID;
}

//-----------------------------------------------------------------------------
DataLng CCheckWorker::CreateWorker()
{
	CString aUserName = AfxGetLoginInfos()->m_strUserName;
	CString aUserDescription = AfxGetLoginInfos()->m_strUserDescription;
	CString aUserEmail = AfxGetLoginInfos()->m_strUserEmail;
	CString aUser = _T("");

	DataStr aName = _T("");
	DataStr aLastName = _T("");
	int aBlank = -1;

	SqlSession*	pSession = AfxGetDefaultSqlConnection()->GetNewSqlSession(AfxGetDefaultSqlConnection()->m_pContext);

	CAutoincrementService* pAutoincrement;
	pAutoincrement = new CAutoincrementService(pSession);

	int aResult = UPDATE_SUCCESS;

	aUser = aUserDescription.IsEmpty() ? aUserName : aUserDescription;
	aBlank = aUser.Find(_T(" "));

	if (aBlank == -1)
		aLastName = aUser;
	else
	{
		aName = aUser.Mid(0, aBlank).Trim();
		aLastName = aUser.Mid(aBlank).Trim();
	}

	TWorkers aRec;
	SqlTable aTbl(&aRec, pSession);

	aTbl.Open(TRUE);
	aTbl.SelectAll();

	TRY
	{
		pSession->ConnectToDatabase();
		pSession->StartTransaction();

		aTbl.Query();
		aTbl.AddNew();
		aTbl.LockCurrent();

		pAutoincrement->GetNextNumber(_T("Framework.TbResourcesMng.Workers.WorkerId"), &aRec.f_WorkerID);

		aRec.f_CompanyLogin				= aUserName;
		aRec.f_Name						= aName;
		aRec.f_LastName					= aLastName;
		aRec.f_Email					= aUserEmail;
		aRec.f_PasswordAttemptsNumber	= 5;
		aRec.f_HideOnLayout				= TRUE;
		aRec.f_PasswordNeverExpire		= TRUE;
		aRec.f_Notes					= _TB("Automatically generated");

		aResult = aTbl.Update();
	
		(aResult == UPDATE_SUCCESS) ? pSession->Commit() : pSession->Abort();
		
		aTbl.UnlockAll();
		aTbl.Close();
		delete pAutoincrement;

		pSession->DisconnectFromDatabase();
		delete pSession;
	}
	CATCH(SqlException, e)
	{
		if (pAutoincrement)
			delete pAutoincrement;
		if (aTbl.IsOpen()) 
			aTbl.Close();
		
		pSession->Abort();
		pSession->DisconnectFromDatabase();
		delete pSession;
		aResult = UPDATE_FAILED;
		e->ShowError();
	}
	END_CATCH
	
	return (aResult == UPDATE_SUCCESS) ? aRec.f_WorkerID : 0;
}

//-----------------------------------------------------------------------------
void CCheckWorker::IntegrateConvertedWorkers()
{
	CString aUserName = AfxGetLoginInfos()->m_strUserName;
	CString aUserDescription = AfxGetLoginInfos()->m_strUserDescription;
	CString aUser = _T("");

	DataStr aName = _T("");
	DataStr aLastName = _T("");
	int aBlank = -1;

	//SqlSession*	pSession = AfxGetDefaultSqlConnection()->GetNewSqlSession(AfxGetDefaultSqlConnection()->m_pContext);
	SqlSession*	pSession = AfxGetDefaultSqlSession();
	TWorkers aRec;
	SqlTable aTbl(&aRec, pSession);

	aTbl.Open(TRUE);
	aTbl.SelectAll();

	aTbl.AddParam(szParamNotes, aRec.f_Notes);
	aTbl.AddFilterColumn(aRec.f_Notes);
	aTbl.SetParamValue(szParamNotes, DataStr(_T("##TO_BE_INTEGRATED##")));

	TRY
	{		
		aTbl.Query();
	if (aTbl.HasRows())
	{
		pSession->StartTransaction();
		while (!aTbl.IsEOF())
		{
			if (aTbl.LockCurrent())
			{
				aTbl.Edit();

				aUserName = aRec.f_CompanyLogin;
				aUserDescription = AfxGetLoginManager()->GetUserDescriptionByName(aUserName);
				aUser = aUserDescription.IsEmpty() ? aUserName : aUserDescription;
				aBlank = aUser.Find(_T(" "));

				if (aBlank == -1)
				{
					aName = _T("");
					aLastName = aUser;
				}
				else
				{
					aName = aUser.Mid(0, aBlank).Trim();
					aLastName = aUser.Mid(aBlank).Trim();
				}

				aRec.f_Name = aName;
				aRec.f_LastName = aLastName;
				aRec.f_Email = AfxGetLoginManager()->GetUserEmailByName(aUserName);
				aRec.f_Notes = _TB("Automatically converted");

				aTbl.Update();
				aTbl.UnlockCurrent();
			}
			aTbl.MoveNext();
		}
		pSession->Commit();
	}

		aTbl.UnlockAll();
		aTbl.Close();
		
		/*if (pSession->CanClose())
		{
			pSession->Close();
			delete pSession;
		}*/
	}
	CATCH(SqlException, e)
	{
		pSession->Abort();
		if (aTbl.IsOpen())
			aTbl.Close();

		/*if (pSession->CanClose())
		{
			pSession->Close();
			delete pSession;
		}*/
		e->ShowError();
	}
	END_CATCH
}

///////////////////////////////////////////////////////////////////////////////
//							CWorkersTable 
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CWorkersTable, CWorkersTableObj)

//-----------------------------------------------------------------------------
void CWorkersTable::LoadWorkers()
{
	TWorkers aRec;
	SqlTable aTbl(&aRec, AfxGetDefaultSqlConnection()->GetDefaultSqlSession());

	aTbl.Open();
	aTbl.Select(aRec.f_WorkerID);
	aTbl.Select(aRec.f_Name);
	aTbl.Select(aRec.f_LastName);
	aTbl.Select(aRec.f_CompanyLogin);
	aTbl.Select(aRec.f_Disabled);
	aTbl.Select(aRec.f_ImagePath);

	TRY
	{
		aTbl.Query();
		while (!aTbl.IsEOF())
		{
			AddWorker(aRec.f_WorkerID, aRec.f_Name, aRec.f_LastName, aRec.f_CompanyLogin, aRec.f_Disabled, aRec.f_ImagePath);
			aTbl.MoveNext();
		}
		aTbl.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTbl.IsOpen())
			aTbl.Close();
		RemoveAll();
	}
	END_CATCH
}

/////////////////////////////////////////////////////////////////////////////
//				class CResourcesFunctions implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CResourcesFunctions, CObject)

//----------------------------------------------------------------------------
CResourcesFunctions::CResourcesFunctions(CAbstractFormDoc* pDoc)
:
	m_pDoc		(pDoc),
	m_TRWorkers	(pDoc)
{}

//----------------------------------------------------------------------------
BOOL CResourcesFunctions::OpenUrl(CString aUrl)
{
	if (aUrl == _T(""))
		return FALSE;

	HINSTANCE hInst = ShellExecute(NULL, NULL, aUrl, NULL, NULL, SW_SHOWDEFAULT);
	if (hInst <= (HINSTANCE)32)
	{
		AfxMessageBox(ShellExecuteErrMsg((int)hInst));
		return FALSE;
	}
	else
		return TRUE;
}

//----------------------------------------------------------------------------
BOOL CResourcesFunctions::SendEmailbyWorker(CString aToEmailAddress)
{
	if (AfxIsActivated(TBEXT_APP, _T("MailConnector"), TRUE) && m_pDoc)
	{
		m_TRWorkers.FindRecord(DataLng(AfxGetWorkerId()));
		return OpenEmailConnector(aToEmailAddress, m_TRWorkers.GetRecord()->f_Email.GetString());
	}
	else
		return OpenEmailClient(aToEmailAddress);
}

//----------------------------------------------------------------------------
BOOL CResourcesFunctions::SendEmail(CString aToEmailAddress, CString aFromEmailAddress /*= _T("")*/)
{
	if (AfxIsActivated(TBEXT_APP, _T("MailConnector"), TRUE))
		return OpenEmailConnector(aToEmailAddress, aFromEmailAddress);
	else
		return OpenEmailClient(aToEmailAddress);
}

//----------------------------------------------------------------------------
BOOL CResourcesFunctions::OpenEmailClient(CString aToEmailAddress)
{
	HINSTANCE hInst = ShellExecute(NULL, NULL, _T("mailto:") + aToEmailAddress, NULL, NULL, SW_SHOWDEFAULT); 
	if (hInst <= (HINSTANCE)32)
	{
		AfxMessageBox(ShellExecuteErrMsg((int)hInst));
		return FALSE;
	}
	else
		return TRUE;
}

//----------------------------------------------------------------------------
BOOL CResourcesFunctions::OpenEmailConnector(CString aToEmailAddress, CString aFromEmailAddress /*= _T("")*/)
{
	BOOL bOK = FALSE;

	if (!m_pDoc)
		return FALSE;

	CMapiMessage Email;
	Email.SetTo(aToEmailAddress);

	if (aFromEmailAddress != _T(""))
		Email.SetFrom(aFromEmailAddress);
	//Email.SetSubject(strOggetto);
	//Email.SetBody(strBody);

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();
	//BOOL bAttachRDE = FALSE;
	//BOOL bAttachPDF = TRUE;
	//BOOL bCompressAttach= params->GetMailCompress();
	BOOL bRequestDeliveryNotification = params->GetRequestDeliveryNotifications();
	BOOL bRequestReadNotification = params->GetRequestReadNotifications();

	CString strLocal = ::_tsetlocale (LC_ALL, NULL);

	if (AfxGetIMailConnector()->ShowEmailDlg(m_pDoc, Email, NULL, NULL, NULL, NULL, NULL, NULL, &bRequestDeliveryNotification, &bRequestReadNotification))
	{
		if (!AfxGetIMailConnector()->SendMail(Email, &bRequestReadNotification, &bRequestReadNotification, m_pDoc->m_pMessages))
		{
			if (m_pDoc->m_pMessages)
				m_pDoc->m_pMessages->Show();
			bOK = FALSE;
		}
	}
	else
		bOK = FALSE;

	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	if (strLocal != strLocal2)
	{
		::_tsetlocale (LC_ALL, strLocal);
	}
	return bOK;
} 

/////////////////////////////////////////////////////////////////////////////
//				class CElemResourceRecursion implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CElemResourceRecursion, CObject)

//----------------------------------------------------------------------------
CElemResourceRecursion::CElemResourceRecursion
	(
		const DataStr&	sResourceType,
		const DataStr& 	sResourceCode,
		int	  nLev
	)
	:	
	m_sResourceType	(sResourceType),
	m_sResourceCode	(sResourceCode),
	m_nLev			(nLev)
{}

///////////////////////////////////////////////////////////////////////////////
//					CheckResourcesRecursion implementation
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CheckResourcesRecursion, CObject)

//-----------------------------------------------------------------------------------
CheckResourcesRecursion::CheckResourcesRecursion(CAbstractFormDoc* pDoc /* = NULL */)
	:
	m_pDoc			(pDoc),
	m_TRResources	(m_pDoc),
	m_TRWorkers		(m_pDoc),
	m_ResourceLevel	(0)
{}

//----------------------------------------------------------------------------
CheckResourcesRecursion::~CheckResourcesRecursion()
{
	Clear();
}

//----------------------------------------------------------------------------
void CheckResourcesRecursion::Clear()
{
	CElemResourceRecursion* pElem;
	while (!m_ResourceList.IsEmpty())
	{
		pElem = m_ResourceList.RemoveHead();
		delete pElem;
	}

	m_CCheckRecursion.Clear();
}

//----------------------------------------------------------------------------
BOOL CheckResourcesRecursion::IsRecursive(const DataStr& aParentResourceCode, const DataStr& aParentResourceType, const DataStr& aResourceCode, const DataStr& aResourceType)
{
	BOOL	bRecursion = FALSE;
	DataStr	aEmptyStr;

	aEmptyStr.Clear();

	CElemResourceRecursion*	pParentResource;
	pParentResource = NULL;
	m_ResourceLevel = 0;

	Clear();

	// Aggiungo la risorsa padre nella lista delle risorse per ricorsione
	m_CCheckRecursion.Add(aParentResourceType, aParentResourceCode, aEmptyStr, aEmptyStr, m_ResourceLevel);
	m_ResourceLevel = m_ResourceLevel + 1;

	// Aggiungo la risorsa di partenza nell'elenco da esplodere
	m_ResourceList.AddHead(new CElemResourceRecursion(aResourceType, aResourceCode, m_ResourceLevel));

	while (!m_ResourceList.IsEmpty())
	{
		pParentResource = m_ResourceList.RemoveHead();

		// controllo che non  sia nell'elenco delle risorse per la ricorsione
		if (m_CCheckRecursion.IsRecursive(pParentResource->m_sResourceType, pParentResource->m_sResourceCode, aEmptyStr, aEmptyStr, pParentResource->m_nLev))
		{
			delete pParentResource;
			bRecursion = TRUE;
			m_pDoc->IsInUnattendedMode() ? m_pDoc->m_pMessages->Add(_TB("Unable to save. Recursiveness found")) : m_pDoc->Message(_TB("Unable to save. Recursiveness found"));
			break;
		}

		BOOL bOk = TRUE;
		if (pParentResource)
		{
			bOk = pParentResource->m_sResourceType.IsEmpty()
				? !AddChildResourcesFromWorkers(_ttol(pParentResource->m_sResourceCode.Str()))
				: !AddChildResources(pParentResource->m_sResourceCode, pParentResource->m_sResourceType);

			if (bOk)
			{
				delete pParentResource;
				bRecursion = TRUE;
				m_pDoc->IsInUnattendedMode() ? m_pDoc->m_pMessages->Add(_TB("Unable to save. Recursiveness found")) : m_pDoc->Message(_TB("Unable to save. Recursiveness found"));
				break;
			}
		}

		delete pParentResource;
	}

	Clear();

	return bRecursion;
}

//----------------------------------------------------------------------------
BOOL CheckResourcesRecursion::AddChildResources(const DataStr& aResourceCode, const DataStr& aResourceType)
{
	DataStr	aEmptyStr;
	aEmptyStr.Clear();

	// Aggiungo la risorsa che sto esaminando nella lista delle risorse per ricorsione
	m_CCheckRecursion.Add(aResourceType, aResourceCode, aEmptyStr, aEmptyStr, m_ResourceLevel);
	m_ResourceLevel = m_ResourceLevel + 1;

	DataStr	aElemRecursionStrKey;

	TResourcesDetails	aRec;
	SqlTable			aTbl(&aRec, m_pDoc->GetReadOnlySqlSession());

	aTbl.Open();
	aTbl.SelectAll();

	aTbl.AddParam(szResourceCode, aRec.f_ResourceCode);
	aTbl.AddFilterColumn(aRec.f_ResourceCode);
	aTbl.AddParam(szResourceType, aRec.f_ResourceType);
	aTbl.AddFilterColumn(aRec.f_ResourceType);

	TRY
	{
		aTbl.SetParamValue(szResourceCode, aResourceCode);
		aTbl.SetParamValue(szResourceType, aResourceType);
		aTbl.Query();

		while (!aTbl.IsEOF())
		{
			if (aResourceCode == aRec.f_ChildResourceCode && aResourceType == aRec.f_ChildResourceType)
			{
				if (aTbl.IsOpen()) aTbl.Close();
				return FALSE;
			}

			if (aRec.f_IsWorker)
			{
				if (m_TRWorkers.FindRecord(aRec.f_ChildWorkerID) == TableReader::FOUND)
					m_ResourceList.AddHead(new CElemResourceRecursion(aEmptyStr, aRec.f_ChildWorkerID.Str(), m_ResourceLevel));
			}
			else
				if (m_TRResources.FindRecord(aRec.f_ChildResourceCode, aRec.f_ChildResourceType) == TableReader::FOUND)
					m_ResourceList.AddHead(new CElemResourceRecursion(aRec.f_ChildResourceType, aRec.f_ChildResourceCode, m_ResourceLevel));

			aTbl.MoveNext();
		}
	}
	CATCH(SqlException, e)
	{
		if (e) e->m_strError;
		return FALSE;
	}
	END_CATCH

	aTbl.Close();

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CheckResourcesRecursion::AddChildResourcesFromWorkers(const DataLng& aResourceCode)
{
	DataStr	aEmptyStr;
	aEmptyStr.Clear();

	// Aggiungo la risorsa che sto esaminando nella lista delle risorse per ricorsione
	m_CCheckRecursion.Add(aEmptyStr, aResourceCode.Str(), aEmptyStr, aEmptyStr, m_ResourceLevel);
	m_ResourceLevel = m_ResourceLevel + 1;

	DataStr	aElemRecursionStrKey;

	TWorkersDetails aRec;
	SqlTable		aTbl(&aRec, m_pDoc->GetReadOnlySqlSession());

	aTbl.Open();
	aTbl.SelectAll();
	aTbl.AddParam		(szResourceCode, aRec.f_WorkerID);
	aTbl.AddFilterColumn(aRec.f_WorkerID);

	TRY
	{
		aTbl.SetParamValue(szResourceCode, aResourceCode);
		aTbl.Query();

		while (!aTbl.IsEOF())
		{
			if (aResourceCode == aRec.f_ChildWorkerID)
			{
				if (aTbl.IsOpen()) aTbl.Close();
				return FALSE;
			}

			// Aggiungo la risorsa figlia nell'elenco da esplodere
			if (aRec.f_IsWorker)
			{
				if (m_TRWorkers.FindRecord(aRec.f_ChildWorkerID) == TableReader::FOUND)
					m_ResourceList.AddHead(new CElemResourceRecursion(aEmptyStr, aRec.f_ChildWorkerID.Str(), m_ResourceLevel));
			}
			else
				if (m_TRResources.FindRecord(aRec.f_ChildResourceCode, aRec.f_ChildResourceType) == TableReader::FOUND)
					m_ResourceList.AddHead(new CElemResourceRecursion(aRec.f_ChildResourceType, aRec.f_ChildResourceCode, m_ResourceLevel));

			aTbl.MoveNext();
		}
	}
	CATCH(SqlException, e)
	{
		if (e) e->m_strError;
		return FALSE;
	}
	END_CATCH

	aTbl.Close();

	return TRUE;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(name = SetWorkerByName , woorm_method=false)]
///<summary>
///Set the worker linked to the company login
///</summary>
/// <remarks>This method set the worker linked to the company login</remarks>
/// <param name="aUserName">Company login</param>
/// <param name="aWorkerName">Worker name</param>
/// <param name="aWorkerLastName">Worker lastname</param>
/// <returns>true if successful otherwise false and the current worker id is set to 0</returns>
//-----------------------------------------------------------------------------
DataBool SetWorkerByName(DataStr aUserName, DataStr aWorkerName, DataStr aWorkerLastName)
{   
	BOOL bOK = TRUE;

	TWorkers aRec;
	SqlTable aTbl(&aRec, AfxGetDefaultSqlConnection()->GetDefaultSqlSession());

	aTbl.Open();
	aTbl.Select(aRec.f_WorkerID);

	aTbl.AddParam		(szParamUserName,	aRec.f_CompanyLogin);
	aTbl.AddParam		(szParamName,		aRec.f_Name);
	aTbl.AddParam		(szParamLastName,	aRec.f_LastName);

	aTbl.AddFilterColumn(aRec.f_CompanyLogin);
	aTbl.AddFilterColumn(aRec.f_Name);
	aTbl.AddFilterColumn(aRec.f_LastName);

	aTbl.SetParamValue	(szParamUserName,	aUserName);
	aTbl.SetParamValue	(szParamName,		aWorkerName);
	aTbl.SetParamValue	(szParamLastName,	aWorkerLastName);

	TRY
	{
		aTbl.Query();

		bOK = !aTbl.IsEmpty();
		if (bOK) AfxSetWorkerId(aRec.f_WorkerID);
		aTbl.Close();
	}
	CATCH(SqlException, e)	
	{
		if (aTbl.IsOpen()) aTbl.Close();
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	return bOK;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(name = SetWorkerByID , woorm_method=false)]
///<summary>
///Set the worker linked to the company login
///</summary>
/// <remarks>This method set the worker linked to the company login</remarks>
/// <param name="aUserName">Company login</param>
/// <param name="aWorkerName">Worker id</param>
/// <returns>true if successful otherwise false and the current worker id is set to 0</returns>
//-----------------------------------------------------------------------------
DataBool SetWorkerByID(DataStr aUserName, DataLng aWorkerID)
{   
	BOOL bOK = TRUE;

	TWorkers aRec;
	SqlTable aTbl(&aRec, AfxGetDefaultSqlConnection()->GetDefaultSqlSession());

	aTbl.Open();
	aTbl.Select(aRec.f_WorkerID);

	aTbl.AddParam		(szParamUserName,	aRec.f_CompanyLogin);
	aTbl.AddParam		(szParamWorkerID,	aRec.f_WorkerID);

	int* i = 0;
	*i = 1;

	aTbl.AddFilterColumn(aRec.f_CompanyLogin);
	aTbl.AddFilterColumn(aRec.f_WorkerID);

	aTbl.SetParamValue	(szParamUserName,	aUserName);
	aTbl.SetParamValue	(szParamWorkerID,	aWorkerID);

	TRY
	{
		aTbl.Query();

		bOK = !aTbl.IsEmpty();
		if (bOK) AfxSetWorkerId(aRec.f_WorkerID);
		aTbl.Close();
	}
	CATCH(SqlException, e)	
	{
		if (aTbl.IsOpen()) aTbl.Close();
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	return bOK;
}
