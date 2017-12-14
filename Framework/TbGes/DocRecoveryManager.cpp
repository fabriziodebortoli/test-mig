#include "stdafx.h"

#include <afxtempl.h>
#include <atldbcli.h>
#include <atldbsch.h>
#include <atlconv.h>

#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\WebServiceStateObjects.h>

#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\BaseApp.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\ExternalControllerInfo.h>
#include <TbGenlib\DiagnosticManager.h>

#include <TbOleDb\SqlRecoveryManager.h>
#include <TbOleDb\SqlObject.h>
#include <TbOleDb\SqlConnect.h>
#include <TbOleDb\oledbmng.h>

#include <TbWoormEngine\RepEngin.h>
#include <TbWoormEngine\Report.h>
#include <TbWoormViewer\WoormDoc.hjson> //JSON AUTOMATIC UPDATE

#include <TbGes\Dbt.h>
#include <TbGes\ExtDoc.h>
#include <TbGes\Browser.h>

#include "DocRecoveryManager.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const int nFurtherSteps	= 6;
static const int nPlayDocRetry	= 3;

//=============================================================================
class DocRecoveryManagerUM
{
public:
	static CString AskStartExplanation			();
	static CString AskStartQuestion				();
	static CString SaveSnapshotsStart			(const int& nPhase);
	static CString SaveSnapshotsSuccess			(const int& nPhase);
	static CString SaveSnapshotsFailed			(const int& nPhase);
	static CString CloseAllStart				(const int& nPhase);
	static CString CloseAllSuccess				(const int& nPhase);
	static CString CloseAllFailed				(const int& nPhase);
	static CString CloseAllActivity				(const int& nPhase, const CString& sActivity);
	static CString PlaySnapshotsStart			(const int& nPhase);
	static CString PlaySnapshotsSuccess			(const int& nPhase);
	static CString PlaySnapshotsFailed			(const int& nPhase);
	static CString PlaySnapshotsActivityFailed	(const int& nPhase, const CString& sActivity);
	static CString PlaySnapshotsActivityNoDoc	(const int& nPhase, const CString& sActivity);
	static CString PlaySnapshotsActivityNoRep	(const int& nPhase, const CString& sActivity);
	static CString PlaySnapshotsActivitySuccess	(const int& nPhase, const CString& sActivity);
};

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::AskStartExplanation ()
{
	return cwsprintf(
						_TB("Application %s loose database connection!\nRecovery System has started and catched the error.\nConfirming automatic Recovery Activity, application performs the\nfollowing operations:\n\n - Phase 1: saves snapshots of your current opened documents\n - Phase 2: closes all your opened documents\n - Phase 3: tries to reconnect the lost databases\n - Phase 4: opens all documents and applies the saved snapshots\n\nDuring Recovery Activity application is freezed,\nso please wait the end of the task."), 
						AfxGetLoginManager()->GetMasterProductBrandedName()
					);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::AskStartQuestion ()
{
	return _TB("Do you want to reconnect the lost database connections ?\nNO answer will show original error and it is suggested to logoff application.");
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::SaveSnapshotsStart (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System is saving all opened activities Snapshots! Please wait..."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::SaveSnapshotsSuccess (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System has saved snapshots of all opened activities successfully!"), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::SaveSnapshotsFailed (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System cannot save snapshots of all opened activities Snapshots! Snapshots will not be replayed!"), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::CloseAllStart (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System will close automatically all opened activities! Please wait..."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::CloseAllSuccess (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System has closed successfully all opened activities!"), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::CloseAllFailed	(const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System has was not able to close all opened activities! "), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::CloseAllActivity (const int& nPhase, const CString& sActivity)
{
	return cwsprintf(_TB("Phase %d: Recovery System is closing the %s document"), nPhase, sActivity);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsStart (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System will try to restore all previous activities. Please wait..."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsSuccess (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System has performed snapshots successfully ! "), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsFailed	(const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to perform all snapshots! The warning list about failed activities is logged above."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsActivitySuccess (const int& nPhase, const CString& sActivity)
{
	return cwsprintf(_TB("Phase %d: Recovery System has played successfully snapshot of %s document! "), nPhase, sActivity);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsActivityFailed (const int& nPhase, const CString& sActivity)
{
	return cwsprintf(_TB("Phase %d: Recovery System has failed to play snapshot of %s document! Document has to be opened and reposition manually!"), nPhase, sActivity);
}

//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsActivityNoDoc (const int& nPhase, const CString& sActivity)
{
	return cwsprintf(_TB("Phase %d: Snapshot cannot be played as cannot re-open %s document. You will have to reopen manually the document."), nPhase, sActivity);
}
	
//-----------------------------------------------------------------------------
/*static*/ CString DocRecoveryManagerUM::PlaySnapshotsActivityNoRep (const int& nPhase, const CString& sActivity)
{
	return cwsprintf(_TB("Phase %d: Snapshot cannot be played as cannot re-open %s Woorm Report. You will have to reopen manually the Woorm Report."), nPhase, sActivity);
}

//////////////////////////////////////////////////////////////////////////////
//							SnapShots Management
// snapshot made before recovery operation and to restore after recovery
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT BaseDocumentSnapshot : public CObject
{
protected:
	CTBNamespace				m_DocNamespace;
	CString						m_sViewMode;
	ViewModeType				m_DocType;
	CBaseDocument::FormMode		m_FormMode;

	SqlRecoveryManagerDiagnostic*	m_pDiagnostic;

public:
	BaseDocumentSnapshot (SqlRecoveryManagerDiagnostic*	pDiagnostic);

public:
	CString GetNamespace		() const;
	CString GetFullNamespace	() const;
	CString GetViewMode			() const;

public:
	virtual BOOL					SaveSnapshot(CBaseDocument* pDocument);
	virtual BOOL					PlaySnapshot(BOOL bDoDiagnostic) = 0;
	virtual BaseDocumentSnapshot*	Clone		() = 0;

};

//-----------------------------------------------------------------------------
CString BaseDocumentSnapshot::GetFullNamespace () const
{
	return m_DocNamespace.ToString();
}

//-----------------------------------------------------------------------------
CString BaseDocumentSnapshot::GetNamespace () const
{
	return m_DocNamespace.ToUnparsedString();
}

//-----------------------------------------------------------------------------
CString BaseDocumentSnapshot::GetViewMode () const
{
	return m_sViewMode;
}

//-----------------------------------------------------------------------------
BaseDocumentSnapshot::BaseDocumentSnapshot (SqlRecoveryManagerDiagnostic* pDiagnostic)
	:
	m_DocType		(VMT_DATAENTRY),
	m_FormMode		(CBaseDocument::BROWSE),
	m_pDiagnostic	(pDiagnostic)
{
	ASSERT(m_pDiagnostic);
}

//-----------------------------------------------------------------------------
BOOL BaseDocumentSnapshot::SaveSnapshot (CBaseDocument* pDocument)
{
	if (!pDocument)
	{
		ASSERT(FALSE);
		TRACE ("BaseDocumentSnapshot::SaveSnapshot cannot save snapshot! NULL pDocument!");
		return FALSE;
	}

	m_DocType		= pDocument->GetType ();
	m_FormMode		= pDocument->GetFormMode();
	m_DocNamespace	= pDocument->GetNamespace();

	if (pDocument->m_pTemplate)
		m_sViewMode = pDocument->m_pTemplate->m_sViewMode;

	return TRUE;
}

// CAbstractFormDoc snapshots properties
//=============================================================================
class TB_EXPORT AbstractDocSnapshot : public BaseDocumentSnapshot
{
private:
	CString		m_strQueryName;
	BOOL		m_bBatchRunning;
	SqlRecord*	m_pMasterRec;

public:
	AbstractDocSnapshot		(SqlRecoveryManagerDiagnostic* pDiagnostic);
	~AbstractDocSnapshot	();

public:
	virtual BOOL					SaveSnapshot(CBaseDocument* pDocument);
	virtual BOOL					PlaySnapshot(BOOL bDoDiagnostic);
	virtual BaseDocumentSnapshot*	Clone		();
};

// construct a snapshot information starting from a document instance
//-----------------------------------------------------------------------------
AbstractDocSnapshot::AbstractDocSnapshot (SqlRecoveryManagerDiagnostic*	pDiagnostic)
	:
	BaseDocumentSnapshot(pDiagnostic),
	m_pMasterRec		(NULL)
{
}


//-----------------------------------------------------------------------------
AbstractDocSnapshot::~AbstractDocSnapshot ()
{
	SAFE_DELETE (m_pMasterRec);
}

//-----------------------------------------------------------------------------
BOOL AbstractDocSnapshot::SaveSnapshot (CBaseDocument* pDocument)
{
	if (!pDocument || !pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		return FALSE;
	
	if (!BaseDocumentSnapshot::SaveSnapshot (pDocument))
		return FALSE;

	CAbstractFormDoc* pAbstractDoc = (CAbstractFormDoc*) pDocument;

	m_strQueryName	= pAbstractDoc->GetCurrentQueryName();
	m_bBatchRunning = pAbstractDoc->m_bBatchRunning;

	DBTMaster* pDBTMaster = pAbstractDoc->m_pDBTMaster;
	if (pAbstractDoc->m_pBrowser && pDBTMaster && !pAbstractDoc->m_pBrowser->NoCurrent() && pDBTMaster->GetRecord())
		m_pMasterRec = pDBTMaster->GetRecord()->Clone ();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL AbstractDocSnapshot::PlaySnapshot (BOOL bDoDiagnostic)
{
	// not valid snapshot, no warning
	if (m_DocNamespace.IsEmpty() || m_sViewMode.IsEmpty())
		return FALSE;
	
	const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(m_DocNamespace);
	CString sDocTitle = pDocDescri && !pDocDescri->GetTitle().IsEmpty() ?
											pDocDescri->GetTitle() :
											m_DocNamespace.ToUnparsedString();

	CBaseDocument* pDoc = NULL;
	// I have to aviod diagnostic messages during document running
	AfxGetDiagnostic ()->StartSession();
	
	// the first run after reconnection often fails for user grants
	pDoc = AfxGetTbCmdManager()->RunDocument(GetNamespace(), m_sViewMode);
	AfxGetDiagnostic ()->EndSession();
	if (!pDoc)
	{
		if (bDoDiagnostic)
		{
			Array arExtInfo;
			for (int i=0; i <= AfxGetDiagnostic ()->GetUpperBound(); i++)
				arExtInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("open error"), AfxGetDiagnostic()->GetMessageLine(i)));
			m_pDiagnostic->AddMessage	(DocRecoveryManagerUM::PlaySnapshotsActivityNoDoc(m_pDiagnostic->GetPhase(), pDocDescri->GetTitle()), CDiagnosticManagerWriter::Warning, &arExtInfo);
		}
		return FALSE;
	}

	AfxGetDiagnostic ()->ClearMessages();

	// batch documents stops snapshot after document re-opening
	if (m_DocType == VMT_BATCH || m_bBatchRunning)
		return TRUE;
	
	CAbstractFormDoc* pAbsDoc = (CAbstractFormDoc*) pDoc;

	// query selection
	pAbsDoc->SetCurrentQueryName(this->m_strQueryName);

	// new record
	if (m_FormMode == CBaseDocument::NEW)
	{
		pAbsDoc->SendMessage(WM_COMMAND, (WPARAM)ID_EXTDOC_NEW, (LPARAM)NULL);
		return TRUE;
	}

	pAbsDoc->SendMessage(WM_COMMAND, (WPARAM)ID_EXTDOC_EXEC_QUERY, (LPARAM)NULL);
	if (m_pMasterRec)
	{
		// browsing operations and record positioning
		pAbsDoc->SendMessage(UM_EXTDOC_FETCH, (WPARAM) m_pMasterRec, (LPARAM)NULL);

		// editing
		if (m_FormMode == CBaseDocument::EDIT)
			pAbsDoc->SendMessage(WM_COMMAND, (WPARAM)ID_EXTDOC_EDIT, (LPARAM)NULL);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BaseDocumentSnapshot* AbstractDocSnapshot::Clone ()
{
	AbstractDocSnapshot* pClone = new AbstractDocSnapshot(m_pDiagnostic);
	
	pClone->m_DocType		= m_DocType;
	pClone->m_FormMode		= m_FormMode;
	pClone->m_DocNamespace	= m_DocNamespace;
	pClone->m_sViewMode		= m_sViewMode;
	pClone->m_strQueryName	= m_strQueryName;
	pClone->m_bBatchRunning = m_bBatchRunning;

	if (m_pMasterRec)
	{
		pClone->m_pMasterRec = m_pMasterRec->Create ();
		*pClone->m_pMasterRec	= *m_pMasterRec;
	}

	return pClone;
}


// CWoormDoc snapshots properties
//=============================================================================
class TB_EXPORT AskRuleField : public CObject
{
private:
	CString		m_sName;
	DataObj*	m_pValue;

public:
	AskRuleField (const CString& sName, DataObj* pValue);

public:
	const CString&	GetName () const { return m_sName; }
	DataObj*		GetValue() { return m_pValue; }
	AskRuleField*	Clone	();
};

//-----------------------------------------------------------------------------
AskRuleField::AskRuleField (const CString& sName, DataObj* pValue)
	:
	m_sName		(sName),
	m_pValue	(pValue)
{
}

//-----------------------------------------------------------------------------
AskRuleField* AskRuleField::Clone ()
{
	return new AskRuleField(m_sName, m_pValue->Clone());
}


// CWoormDoc snapshots properties
//=============================================================================
class TB_EXPORT WoormDocSnapshot : public BaseDocumentSnapshot
{
private:
	enum Activity { IsOpened, IsEditing, IsRunning, IsPrinting };

	Activity	m_eActivity;
	CWoormInfo*	m_pWoormInfo;
	Array		m_AskRuleFields;
	int			m_nReportPage;

public:
	WoormDocSnapshot	(SqlRecoveryManagerDiagnostic*	pDiagnostic);
	~WoormDocSnapshot	();

public:
	virtual BOOL					SaveSnapshot(CBaseDocument* pDocument);
	virtual BOOL					PlaySnapshot(BOOL bDoDiagnostic);
	virtual BaseDocumentSnapshot*	Clone		();
};

// construct a snapshot information starting from a document instance
//-----------------------------------------------------------------------------
WoormDocSnapshot::WoormDocSnapshot (SqlRecoveryManagerDiagnostic* pDiagnostic)
	:
	BaseDocumentSnapshot(pDiagnostic),
	m_pWoormInfo		(NULL),
	m_eActivity			(IsOpened)
{
}

//-----------------------------------------------------------------------------
WoormDocSnapshot::~WoormDocSnapshot ()
{
	SAFE_DELETE (m_pWoormInfo);
}

//-----------------------------------------------------------------------------
BOOL WoormDocSnapshot::SaveSnapshot (CBaseDocument* pDocument)
{
	if (!pDocument || !pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		ASSERT(FALSE);
		TRACE ("WoormDocSnapshot::SaveSnapshot cannot save snapshot! NULL pDocument!");
		return FALSE;
	}

	if (!BaseDocumentSnapshot::SaveSnapshot (pDocument))
		return FALSE;

	CWoormDoc* pWoormDoc = (CWoormDoc*) pDocument;

	if (pWoormDoc->m_bAllowEditing)
	{
		// I try to save the document
		pWoormDoc->SendMessage(ID_FILE_SAVE, NULL, NULL);
		m_eActivity = IsEditing;
	}
	else if (pWoormDoc->IsReportRunning())
		m_eActivity = IsRunning;

	if (pWoormDoc->m_pWoormInfo && pWoormDoc->m_pWoormInfo->m_bOwnedByReport)
		m_pWoormInfo = new CWoormInfo(*pWoormDoc->m_pWoormInfo);

	m_nReportPage = pWoormDoc->GetCurrentPage(); 

	// current ask rules values 
	SymTable* pRepTable = pWoormDoc->GetEngineSymTable();

	WoormField*	pRepField;
	SymField*	pItem;
	m_AskRuleFields.RemoveAll();
	if (pRepTable)
		for (int i=0; i <= pRepTable->GetUpperBound(); i++)
		{
			pItem = pRepTable->GetAt(i);
			if (!pItem->IsKindOf(RUNTIME_CLASS(WoormField)))
				continue;

			pRepField = (WoormField*) pItem;
			if (pRepField && pRepField->IsInput())
				m_AskRuleFields.Add (new AskRuleField(pRepField->GetPhysicalName(), pRepField->GetRepData()->Clone()));
		}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WoormDocSnapshot::PlaySnapshot (BOOL bDoDiagnostic)
{
	BOOL bReportRunning = m_eActivity == WoormDocSnapshot::IsRunning;

	// to run automatically ask rules I have to use external controller interface
	CExternalControllerInfo extController;
	extController.m_ControllingMode = CExternalControllerInfo::RUNNING;

	// current ask rules values 
	AskRuleField* pAskRule;
	for (int i=0; i <= m_AskRuleFields.GetUpperBound(); i++)
	{
		pAskRule = (AskRuleField*) m_AskRuleFields.GetAt(i);
		if (pAskRule)
			extController.m_Data.AddParam(pAskRule->GetName(), pAskRule->GetValue()->Clone());
	}

	CWoormDoc* pWoormDoc = NULL;
	CWoormInfo* pWoormInfoToUse;

	// I have to clone CWoorminfo as this instance will die at the end of the recovery action
	if (m_pWoormInfo)
		pWoormInfoToUse = new CWoormInfo(*m_pWoormInfo);
	else
		pWoormInfoToUse = new CWoormInfo(DataStr(GetNamespace()));

	AfxGetDiagnostic ()->StartSession();
	pWoormDoc = (CWoormDoc*)  AfxGetTbCmdManager()->RunWoormReport
		(
			pWoormInfoToUse, 
			NULL, 
			&extController, 
			FALSE, 
			// I always run report as I don't know if has extracted data before
			TRUE
		);
	AfxGetDiagnostic ()->EndSession();

	if (!pWoormDoc)
	{
		if (bDoDiagnostic)
		{
			Array arExtInfo;
			for (int i=0; i <= AfxGetDiagnostic ()->GetUpperBound(); i++)
				arExtInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("open error"), AfxGetDiagnostic()->GetMessageLine(i)));
			m_pDiagnostic->AddMessage (DocRecoveryManagerUM::PlaySnapshotsActivityNoRep(m_pDiagnostic->GetPhase(), GetNamespace()), CDiagnosticManagerWriter::Warning);
		}
		delete pWoormInfoToUse;
		return FALSE;
	}

	extController.WaitUntilFinished();

	// Edit mode
	if (m_eActivity == WoormDocSnapshot::IsEditing)
		pWoormDoc->PostMessage(WM_COMMAND, (WPARAM) ID_ALLOW_EDITING, (LPARAM) NULL);

	// if I was located in a page I reposition
	else if (m_eActivity == WoormDocSnapshot::IsRunning && m_nReportPage)
	{
		for (int i=1; i < m_nReportPage; i++)
			pWoormDoc->SendMessage(WM_COMMAND, (WPARAM) ID_GOTO_PAGE, (LPARAM) NULL);
	}

	pWoormDoc->m_pExternalControllerInfo = NULL;
	return TRUE;
}

//-----------------------------------------------------------------------------
BaseDocumentSnapshot* WoormDocSnapshot::Clone ()
{
	WoormDocSnapshot* pClone = new WoormDocSnapshot(m_pDiagnostic);
	
	pClone->m_DocType		= m_DocType;
	pClone->m_FormMode		= m_FormMode;
	pClone->m_DocNamespace	= m_DocNamespace;
	pClone->m_sViewMode		= m_sViewMode;
	pClone->m_eActivity		= m_eActivity;
	pClone->m_pWoormInfo	= new CWoormInfo(*m_pWoormInfo);
	pClone->m_nReportPage	= m_nReportPage;

	AskRuleField* pAskRule;
	for (int i=0; i <= m_AskRuleFields.GetUpperBound(); i++)
	{
		pAskRule = (AskRuleField*) m_AskRuleFields.GetAt(i);
		if (pAskRule)
			pClone->m_AskRuleFields.Add (pAskRule->Clone());
	}

	return pClone;
}
//////////////////////////////////////////////////////////////////////////////
//					DocRecoveryManager
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
DocRecoveryManager::DocRecoveryManager ()
{
}

//-----------------------------------------------------------------------------
BOOL DocRecoveryManager::OnPerformRecoveryActivity ()
{
	if (!m_Settings.IsEnabled() || AfxIsInUnattendedMode())
	{
		AfxTBMessageBox(m_CurrState.m_Diagnostic.m_sOriginalError);
		return FALSE;
	}

	InitializeRecoveryState ();

	// locks of all involved object and UI freezing
	FreezeApplication ();

	// ask to the user what he wants to do only once for a thread exception
	if (m_CurrState.GetState() == SqlRecoveryManagerState::OnIdle)
	{
		m_CurrState.SetState(SqlRecoveryManagerState::Alerted);
	
		// user feedback and choice about connection lost is shown only once
		if (AskToUser	(
							DocRecoveryManagerUM::AskStartExplanation(),  
							DocRecoveryManagerUM::AskStartQuestion()
						) == IDCANCEL)
		{
			m_CurrState.SetState(SqlRecoveryManagerState::Aborted);
			ShowResultsToUser();
			
			// I close all and I try to logoff
			CloseAllActivities ();
			m_CurrState.SetState (SqlRecoveryManagerState::OnIdle);
			return FALSE;
		}
	}

	// user feedback
	m_CurrState.StartPhase	(1);

	int nBaseBarSteps = (m_Settings.GetRetries() * nProgressBarRetriesMult) * AfxGetOleDbMng()->m_aConnectionPool.GetSize();
	ShowRecoveryUI		();
	SetMaxProgressBarUI	(nBaseBarSteps + nFurtherSteps);

	m_CurrState.SetState(SqlRecoveryManagerState::Recovering);

	// it not mandatory
	m_CurrState.m_Diagnostic.m_bSnapShotSaved = SaveSnapshots ();

	m_CurrState.StartPhase (2);

	// it close all opened tasks 
	if (!CloseAllActivities ())
	{
		m_CurrState.SetState(SqlRecoveryManagerState::NotRecovered);
		ShowResultsToUser();
		m_CurrState.SetState (SqlRecoveryManagerState::OnIdle);

		return FALSE;
	}

	m_CurrState.StartPhase (3);

	// it try to restore connections (mandatory)
	if (RecoveryConnections ())
		m_CurrState.SetState(SqlRecoveryManagerState::Recovered);
	else	
		m_CurrState.SetState(SqlRecoveryManagerState::NotRecovered);
	
	BOOL bRecovered = m_CurrState.GetState() == SqlRecoveryManagerState::Recovered;

	// snapshot must be replayed only with unlocked context
	if (bRecovered)
		UnFreezeApplication();

	// play snapshot management in case of success
	if	(bRecovered && m_CurrState.m_Diagnostic.IsSnapshotsSaved())
	{
		m_CurrState.StartPhase (4);
		m_CurrState.m_Diagnostic.m_bSnapShotPlayed = PlaySnapshots ();
	}
	
	DestroyRecoveryUI();
	
	ShowResultsToUser();

	m_CurrState.SetState (SqlRecoveryManagerState::OnIdle);

	return bRecovered;
}

//-----------------------------------------------------------------------------
BOOL DocRecoveryManager::SaveSnapshots ()
{
	UpdateRecoveryUI (DocRecoveryManagerUM::SaveSnapshotsStart(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);

	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));
	
	m_SnapShots.RemoveAll ();
	BOOL bOk = TRUE;
	
	CBaseDocument* pOpenedDoc;
	for (int i = 0; i < arDocs.GetCount(); i++)
	{
		pOpenedDoc = (CBaseDocument*) arDocs[i];
		if (!pOpenedDoc)
			continue;
		
		DocInvocationInfo* pDocInfo = pOpenedDoc->GetDocInvocationInfo();
		
		// if I have an ancestor or a caller document I cannot reopen the document
		if (pOpenedDoc->GetCallerDocument() || (pDocInfo && pDocInfo->m_pAncestorDoc))
			continue;

		// status of the unplayable of the document
		if (pOpenedDoc->IsInUnattendedMode() || pOpenedDoc->IsExporting() || pOpenedDoc->IsImporting())
			continue;

		// snapshot saving operations
		BaseDocumentSnapshot* pSnapshot = NULL;
		
		if (pOpenedDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) && (!pDocInfo || !pDocInfo->m_pAuxInfo))
			pSnapshot = new AbstractDocSnapshot(&m_CurrState.m_Diagnostic);
		else if (pOpenedDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
			pSnapshot = new WoormDocSnapshot(&m_CurrState.m_Diagnostic);
		if (pSnapshot && pSnapshot->SaveSnapshot(pOpenedDoc)) 
			m_SnapShots.Add (pSnapshot);
	}

	if (bOk)
		UpdateRecoveryUI (DocRecoveryManagerUM::SaveSnapshotsSuccess(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);
	else
		UpdateRecoveryUI (DocRecoveryManagerUM::SaveSnapshotsFailed(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Warning);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL DocRecoveryManager::CloseAllActivities ()
{
	UpdateRecoveryUI (DocRecoveryManagerUM::CloseAllStart(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);

	BOOL bAllClosed = TRUE;

	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	const CBaseDocument* pDocToClose;
	
	// Diagnostic produced by document close is not meaningful, could be exceptions errors!
	AfxGetDiagnostic()->StartSession();

	for (int i = 0; i < arDocs.GetCount(); i++)
	{
		pDocToClose = (CBaseDocument*) arDocs[i];

		if (pDocToClose)
		{
			UpdateRecoveryUI (DocRecoveryManagerUM::CloseAllActivity(m_CurrState.GetPhase(), pDocToClose->GetTitle()), CDiagnosticManagerWriter::Information);
			try
			{
				bAllClosed = bAllClosed && AfxGetTbCmdManager()->DestroyDocument(pDocToClose);
			}
			catch (...)
			{
				continue;
			}
		}
	}

	AfxGetDiagnostic()->EndSession();

	if (bAllClosed)
		UpdateRecoveryUI (DocRecoveryManagerUM::CloseAllSuccess(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);
	else
		UpdateRecoveryUI (DocRecoveryManagerUM::CloseAllFailed(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Warning);

	return bAllClosed;
}

// restore all previous tasks saved into snapshots
//-----------------------------------------------------------------------------
BOOL DocRecoveryManager::PlaySnapshots ()
{
	UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsStart(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);

	if (m_SnapShots.GetSize() == 0)
	{
		UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsSuccess(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);
		return TRUE;
	}

	BOOL bOk = FALSE;
	BaseDocumentSnapshot* pSnapshot;
	for (int i=0; i <= m_SnapShots.GetUpperBound() ; i++)
	{
		pSnapshot = (BaseDocumentSnapshot*) m_SnapShots.GetAt(i);
		if (!pSnapshot)
			continue;

		// first runs usually fail for communication with web services; 
		// I postpone it, I avoid to log diagnostic on the first run
		for (int nRetries = 0; nRetries < nPlayDocRetry; nRetries++)
		{
			if (pSnapshot->PlaySnapshot (nRetries == (nRetries -1 )))
			{
				UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsActivitySuccess(m_CurrState.GetPhase(), pSnapshot->GetNamespace()), CDiagnosticManagerWriter::Information);
				bOk = TRUE;
				break;
			}
			else
				UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsActivityFailed(m_CurrState.GetPhase(), pSnapshot->GetNamespace()), CDiagnosticManagerWriter::Warning);
		}
	}

	if (bOk || !m_SnapShots.GetSize())
		UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsSuccess(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);
	else
		UpdateRecoveryUI (DocRecoveryManagerUM::PlaySnapshotsFailed(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Warning);

	return bOk;
}

// it freeze application and UI
//-----------------------------------------------------------------------------
void DocRecoveryManager::FreezeApplication ()
{
	m_CurrState.FreezeApplication();
	AfxGetLoginContext()->Lock();
}

// it unfreeze application and UI
//-----------------------------------------------------------------------------
void DocRecoveryManager::UnFreezeApplication ()
{
	m_CurrState.UnFreezeApplication();
	AfxGetLoginContext()->UnLock();
}
