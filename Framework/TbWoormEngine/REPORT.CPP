
#include "stdafx.h"

#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\dib.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\SettingsTable.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\const.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\TbGenlibSettings.h>
#include <TbGenlib\TBToolBar.h>

#include <TbOleDb\sqlconnect.h>
#include <TbOleDb\oledbmng.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbWoormViewer\commands.hrc>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGenlib\BASEFRM.hjson>
#include <TbGes\extdoc.hjson>

#include "rpsymtbl.h"
#include "repengin.h"
#include "repfield.h"
#include "reptable.h"
#include "report.h"
#include "MultiLayout.h"
#include "procdata.h"

#include <TbWoormViewer\WoormDoc.hjson> //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR BASED_CODE szOnPreparePrinting[] = _T("OnPreparePrinting");
static const TCHAR BASED_CODE szOnBeginPrinting[] = _T("OnBeginPrinting");
static const TCHAR BASED_CODE szOnEndPrinting[] = _T("OnEndPrinting");
static const TCHAR BASED_CODE szOnAbortPrinting[] = _T("OnAbortPrinting");
static const TCHAR BASED_CODE szOnCloseReport[] = _T("OnCloseReport");

IMPLEMENT_DYNAMIC(IBaseObj, CWnd)

// Array of structs associated to buttons of Reporting Studio toolbar 
// let manage icons of toolbar buttons and hide/show them
static CArray<RSToolbarIcons>* pRSToolbarIcons;

// Returns RSToolbarIcons struct  of a button in Reporting Studio toolbar
// commandID: button ID
//-----------------------------------------------------------------------------
RSToolbarIcons* GetRSIcon(UINT commandID)
{
	if (!pRSToolbarIcons)
		pRSToolbarIcons = new CArray<RSToolbarIcons>;

	for (int i = 0; i <= pRSToolbarIcons->GetUpperBound(); i++)
	{
		if (pRSToolbarIcons->GetAt(i).m_nCmd == commandID)
			return &pRSToolbarIcons->GetAt(i);
	}

	RSToolbarIcons myBtnIcon;
	if (commandID == ID_WRMRADAR_FIXED)
		myBtnIcon = RSToolbarIcons{ ID_WRMRADAR_FIXED, TBIcon(szIconPin, TOOLBAR), TRUE };
	else if (commandID == ID_PG_UP)
		myBtnIcon = RSToolbarIcons{ ID_PG_UP, TBIcon(szIconTop, TOOLBAR), TRUE };
	else if (commandID == ID_PG_DN)
		myBtnIcon = RSToolbarIcons{ ID_PG_DN, TBIcon(szIconBottom, TOOLBAR), TRUE };
	else if (commandID == ID_PG_HOME)
		myBtnIcon = RSToolbarIcons{ ID_PG_HOME, TBIcon(szIconFirst, TOOLBAR), TRUE };
	else if (commandID == ID_PG_END)
		myBtnIcon = RSToolbarIcons{ ID_PG_END, TBIcon(szIconLast, TOOLBAR), TRUE };
	else if (commandID == ID_BTN_FILE_PRINT)
		myBtnIcon = RSToolbarIcons{ ID_BTN_FILE_PRINT, TBIcon(szIconPrint, TOOLBAR), TRUE };
	else if (commandID == ID_FILE_PRINT_PREVIEW)
		myBtnIcon = RSToolbarIcons{ ID_FILE_PRINT_PREVIEW, TBIcon(szIconPrintPreview, TOOLBAR), TRUE };
	else if (commandID == ID_FILE_SAVE_2)
		myBtnIcon = RSToolbarIcons{ ID_FILE_SAVE_2, TBIcon(szIconSave2, TOOLBAR), TRUE };
	else if (commandID == ID_RUN_STOP)
		myBtnIcon = RSToolbarIcons{ ID_RUN_STOP, TBIcon(szIconStart, TOOLBAR), TRUE };
	else if (commandID == ID_RUN_STOP_STOP)
		myBtnIcon = RSToolbarIcons{ ID_RUN_STOP_STOP, TBIcon(szIconStop, TOOLBAR), TRUE };
	else if (commandID == ID_RUN_STOP_2)
		myBtnIcon = RSToolbarIcons{ ID_RUN_STOP_2, TBIcon(szIconStart, TOOLBAR), TRUE };
	else if (commandID == ID_RUN_STOP_2_STOP)
		myBtnIcon = RSToolbarIcons{ ID_RUN_STOP_2_STOP, TBIcon(szIconStop, TOOLBAR), TRUE };
	else if (commandID == ID_PAUSE_RESUME)
		myBtnIcon = RSToolbarIcons{ ID_PAUSE_RESUME, TBIcon(szIconPause, TOOLBAR), TRUE };
	else if (commandID == ID_PAUSE_RESUME_RESUME)
		myBtnIcon = RSToolbarIcons{ ID_PAUSE_RESUME_RESUME, TBIcon(szIconResume, TOOLBAR), TRUE };
	else if (commandID == ID_USE_PREV_ASK_INPUT)
		myBtnIcon = RSToolbarIcons{ ID_USE_PREV_ASK_INPUT, TBIcon(szIconClassicPin, TOOLBAR), TRUE };
	else if (commandID == ID_TOOLBAR_FIND_PREV)
		myBtnIcon = RSToolbarIcons{ ID_TOOLBAR_FIND_PREV, TBIcon(szIconPrev, TOOLBAR), TRUE };
	else if (commandID == ID_TOOLBAR_FIND_NEXT)
		myBtnIcon = RSToolbarIcons{ ID_TOOLBAR_FIND_NEXT, TBIcon(szIconNext, TOOLBAR), TRUE };
	else if (commandID == ID_OPEN_AS_TEXT)
		myBtnIcon = RSToolbarIcons{ ID_OPEN_AS_TEXT, TBIcon(szIconEditView, TOOLBAR), TRUE };
	else if (commandID == ID_TOOLS_2)
		myBtnIcon = RSToolbarIcons{ ID_TOOLS_2, TBIcon(szIconCustomize, TOOLBAR), TRUE };
	else if (commandID == ID_FILE_CLOSE)
		myBtnIcon = RSToolbarIcons{ ID_FILE_CLOSE, TBIcon(szIconExit, TOOLBAR), TRUE };
	else if (commandID == ID_ALLOW_EDITING)
		myBtnIcon = RSToolbarIcons{ ID_ALLOW_EDITING, TBIcon(szIconEdit, TOOLBAR), TRUE };
	else if (commandID == ID_FILE_SAVE)
		myBtnIcon = RSToolbarIcons{ ID_FILE_SAVE, TBIcon(szIconSave2, TOOLBAR), TRUE };
	else if (commandID == ID_PAUSE_RESUME)
		myBtnIcon = RSToolbarIcons{ ID_PAUSE_RESUME, TBIcon(szIconPause, TOOLBAR), TRUE };
	else if (commandID == ID_EXPORT_DATA)
		myBtnIcon = RSToolbarIcons{ ID_EXPORT_DATA, TBIcon(szIconExtract, TOOLBAR), TRUE };
	else if (commandID == ID_ARCHIVE_PDF_FORMAT)
		myBtnIcon = RSToolbarIcons{ ID_ARCHIVE_PDF_FORMAT, TBIcon(szIconAttach, TOOLBAR), TRUE };
	else if (commandID == ID_WRM_EASY_READING)
		myBtnIcon = RSToolbarIcons{ ID_WRM_EASY_READING, TBIcon(szIconEasyReadingOff, TOOLBAR), TRUE };
	else if (commandID == ID_WRM_MINUS_LINES)
		myBtnIcon = RSToolbarIcons{ ID_WRM_MINUS_LINES, TBIcon(szIconMinus, TOOLBAR), TRUE };
	else if (commandID == ID_WRM_PLUS_LINES)
		myBtnIcon = RSToolbarIcons{ ID_WRM_PLUS_LINES, TBIcon(szIconAdd, TOOLBAR), TRUE };
	else if (commandID == ID_WMR_RERUN_STARTSTOP)
		myBtnIcon = RSToolbarIcons{ ID_WMR_RERUN_STARTSTOP, TBIcon(szIconReRun, TOOLBAR), TRUE };
	else if (commandID == ID_WMR_RERUN_STARTSTOP_START)
		myBtnIcon = RSToolbarIcons{ ID_WMR_RERUN_STARTSTOP_START, TBIcon(szIconStart, TOOLBAR), TRUE };
	else if (commandID == ID_WMR_RERUN_STARTSTOP_STOP)
		myBtnIcon = RSToolbarIcons{ ID_WMR_RERUN_STARTSTOP_STOP, TBIcon(szIconStop, TOOLBAR), TRUE };
	else if (commandID == ID_ACTIONS_REP_COPY)
		myBtnIcon = RSToolbarIcons{ ID_ACTIONS_REP_COPY, TBIcon(szCopyDocumentLink, TOOLBAR), TRUE };
	else if (commandID == ID_TOOLS)
		myBtnIcon = RSToolbarIcons{ ID_TOOLS, TBIcon(szIconCustomize, TOOLBAR), TRUE };
	//statusbar button
	else if (commandID == ID_EXTDOC_BACKTOMENU)
		myBtnIcon = RSToolbarIcons{ ID_EXTDOC_BACKTOMENU, TBIcon(szIconBack, TOOLBAR), TRUE };
	else if (commandID == ID_EXTDOC_SWITCHTO)
		myBtnIcon = RSToolbarIcons{ ID_EXTDOC_SWITCHTO, TBIcon(szIconSwitchTo, TOOLBAR), TRUE };
	else if (commandID == ID_WOORM_TAB_SWITCH)
		myBtnIcon = RSToolbarIcons{ ID_WOORM_TAB_SWITCH, TBIcon(szIconTabSwitch, TOOLBAR), TRUE };
	else if (commandID == ID_WOORM_TOOLBAR_COLLAPSED)
		myBtnIcon = RSToolbarIcons{ ID_WOORM_TOOLBAR_COLLAPSED, TBIcon(szIconCollapsed, TOOLBAR), TRUE };
	else
	{
		//si ricerca un button inesistente
		ASSERT(FALSE);
		myBtnIcon = RSToolbarIcons{ commandID, _T(""), FALSE };
	}

	pRSToolbarIcons->Add(myBtnIcon);
	return &pRSToolbarIcons->GetAt(pRSToolbarIcons->GetUpperBound());

}

// TRUE if button of Reporting Studio toolbar is visible, FALSE otherwise
// commandID: button ID
//-----------------------------------------------------------------------------
TB_EXPORT BOOL AFXAPI AfxIsRSIconVisible(UINT commandID)
{
	return GetRSIcon(commandID)->m_bVisible;
}

// Return icon of a button in Reporting Studio toolbar
// commandID: button ID
//-----------------------------------------------------------------------------
TB_EXPORT CString AFXAPI AfxGetRSIcon(UINT commandID)
{
	return GetRSIcon(commandID)->m_sIcon;
}

// Set icon of a button in Reporting Studio toolbar
// sIcon: namespace of image file
// commandID: button ID
//-----------------------------------------------------------------------------
TB_EXPORT BOOL AFXAPI AfxSetRSIcon(UINT commandID, CString sIcon)
{
	CString s = GetValidImagePath(sIcon);
	if (s.IsEmpty())
		return FALSE;

	GetRSIcon(commandID)->m_sIcon = sIcon;
	return TRUE;
}

// Set visibility of a button in Reporting Studio toolbar
// commandID: button ID
// isVisible: TRUE visible, FALSE not visible
//-----------------------------------------------------------------------------
TB_EXPORT void AFXAPI AfxSetRSIconVisible(UINT commandID, BOOL isVisible /*= TRUE*/)
{
	GetRSIcon(commandID)->m_bVisible = isVisible;
}

// Delete Array of structs associated to buttons of Reporting Studio toolbar 
//-----------------------------------------------------------------------------
TB_EXPORT void AFXAPI AfxDeleteRSIconArray()
{
	SAFE_DELETE(pRSToolbarIcons);
}

//==============================================================================
//			Class CWoormDoc implementation
//==============================================================================

IMPLEMENT_DYNCREATE(CWoormDoc, CBaseDocument)

//-----------------------------------------------------------------------------
CWoormDoc::CWoormDoc()
	:
	m_deReportStatus(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS),
	m_ViewSymbolTable(WoormTable::SimpleTable_RDEVIEW, NULL)
{
	m_ViewSymbolTable.SetDocument(this);
}

//-----------------------------------------------------------------------------
BOOL CWoormDoc::UseOptimizedLineBreak()
{
	DataObj* pSetting = AfxGetSettingValue(CTBNamespace(CTBNamespace::MODULE, _NS_MOD("Framework.TbWoormViewer")), szWoormGeneralOptions, szOptimizedLineBreak, DataBool(FALSE));
	return pSetting ? *((DataBool*)pSetting) : FALSE;
}

//-----------------------------------------------------------------------------
void CWoormDoc::SyncronizeViewSymbolTable(WoormField* pRepFieldData)
{
	if (!pRepFieldData) return;
	ASSERT_VALID(pRepFieldData);

	if (pRepFieldData->IsSubTotal()) return;;

	WoormField* pField = m_ViewSymbolTable.GetField(pRepFieldData->GetName());
	if (pField == NULL)
	{
		pField = new WoormField(pRepFieldData->GetName(), pRepFieldData->GetFieldType(), pRepFieldData->GetDataType(), pRepFieldData->GetId(), pRepFieldData->GetData());

		pField->SetHidden(pRepFieldData->IsHidden());
		pField->SetAsk(pRepFieldData->IsAsk());
		pField->SetReInit(pRepFieldData->IsReInit());
		pField->SetStatic(pRepFieldData->IsStatic());
		pField->SetLowerLimit(pRepFieldData->IsLowerLimit());
		pField->SetUpperLimit(pRepFieldData->IsUpperLimit());
		pField->SetTableRuleField(pRepFieldData->IsTableRuleField());
		pField->SetSpecialField(pRepFieldData->IsSpecialField());

		m_ViewSymbolTable.Add(pField);

		WoormField* test = m_ViewSymbolTable.GetField(pRepFieldData->GetName());
		int i = 0;
	}
	else
	{
		//TODO ASSERT(pField->GetId() == pRepFieldData->GetId());
		BOOL bIsArray = pRepFieldData->GetDataType() == DataType::Array;
		pField->SetFieldType(pRepFieldData->GetFieldType());
		if (bIsArray && pRepFieldData->GetData())
			pField->SetDataType(((DataArray*)pRepFieldData->GetData())->GetBaseDataType(), bIsArray);
		else
			pField->SetDataType(pRepFieldData->GetDataType());
		pField->SetHidden(pRepFieldData->IsHidden());
		pField->SetAsk(pRepFieldData->IsAsk());
		pField->SetReInit(pRepFieldData->IsReInit());
		pField->SetStatic(pRepFieldData->IsStatic());
		pField->SetLowerLimit(pRepFieldData->IsLowerLimit());
		pField->SetUpperLimit(pRepFieldData->IsUpperLimit());
		pField->SetTableRuleField(pRepFieldData->IsTableRuleField());
		pField->SetSpecialField(pRepFieldData->IsSpecialField());
	}
}

//-----------------------------------------------------------------------------
void CWoormDoc::SyncronizeViewSymbolTable(WoormTable* pSymTable)
{
	ASSERT_VALID(pSymTable);
	for (int i = 0; i < pSymTable->GetCount(); i++)
	{
		WoormField* pRepFieldData = pSymTable->GetAt(i);
		ASSERT_VALID(pRepFieldData);

		SyncronizeViewSymbolTable(pRepFieldData);
	}
}
//-----------------------------------------------------------------------------
void CWoormDoc::SetJsonResult(CJsonParser& json)
{
	m_bEngineRunning = FALSE;
	if (!m_pWoormInfo)
	{
		ASSERT(FALSE);
		return;
	}
	for (int i = 0; i < m_pWoormInfo->GetParamCount(); i++)
	{
		CDataObjDescription* pParam = m_pWoormInfo->GetParamDescription(i);
		if (pParam->IsPassedModeOut() || pParam->IsPassedModeInOut())
		{
			if (json.BeginReadObject(pParam->GetName()))
			{
				pParam->GetValue()->AssignJsonValue(json);
				json.EndReadObject();
			}
		}
	}
}
//-----------------------------------------------------------------------------
BOOL CWoormDoc::IsHideFrame() const
{
	return m_pWoormInfo ? m_pWoormInfo->m_bHideFrame : FALSE;
}

// Open document in lite mode, only to perform proxy communication with rs web
//
//-----------------------------------------------------------------------------
BOOL CWoormDoc::OnOpenDocument(LPCTSTR pObject)
{
	m_bEngineRunning = TRUE;
	m_pDocInvocationInfo = (DocInvocationInfo*)pObject;
	AttachWoormInfoFromDocInvocationInfo();
	return TRUE;
}


//-----------------------------------------------------------------------------
void CWoormDoc::AttachWoormInfoFromDocInvocationInfo()
{
	if (m_pDocInvocationInfo && m_pDocInvocationInfo->m_pAuxInfo)
	{
		CObject* pO = (CObject*)m_pDocInvocationInfo->m_pAuxInfo;
		//il controllo non è SAFE, ... meglio di nulla
		if (pO->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
			AttachWoormInfo((CWoormInfo*)(m_pDocInvocationInfo->m_pAuxInfo));
		else
		{
			ASSERT(FALSE);
		}
	}
}

//------------------------------------------------------------------------------
void CWoormDoc::AttachWoormInfo(CWoormInfo* pWoormInfo)
{
	m_pWoormInfo = pWoormInfo;
	if (pWoormInfo)
	{
		GetInfoOSL()->m_Namespace = pWoormInfo->GetNamespace();
		GetInfoOSL()->SetType(OSLType_Report);
	}
}



//==============================================================================
//			Class EngineScheduler implementation
//==============================================================================

//-----------------------------------------------------------------------------
UINT __cdecl RunInternalCommand(LPVOID pParam)
{
	EngineScheduler* pEngineSched = (EngineScheduler*)pParam;

	TRY
	{
		if (pEngineSched->m_BoolCommand)
		{
			pEngineSched->m_CommandResult = (pEngineSched->m_pObject->*pEngineSched->m_BoolCommand)();
			pEngineSched->m_bExecuteCommandWorking = false;
			return 0;
		}

		if (pEngineSched->m_VoidCommand)
		{
			(pEngineSched->m_pObject->*pEngineSched->m_VoidCommand)();
			pEngineSched->m_bExecuteCommandWorking = false;
			return 0;
		}
	}
		CATCH_ALL(e)
	{
	}
	END_CATCH_ALL

		pEngineSched->m_bExecuteCommandWorking = false;
	return 1;
}

//-----------------------------------------------------------------------------
EngineScheduler::EngineScheduler(ReportEngine* pReportEngine)
	:
	m_pObject(NULL),
	m_BoolCommand(NULL),
	m_VoidCommand(NULL),
	m_CommandResult(NULL),
	m_bExecuteCommandWorking(false),
	m_pReportEngine(pReportEngine),
	m_bHoldingMsg(FALSE)
{
	ResetRTError();

	DataObj* pS = AfxGetSettingValue(snsTbWoormViewer, szWoormRunningOptions, szUseMultithreading, DataBool(FALSE), szTbWoormViewerSettingFileName);
	m_bUseMultithreading = pS ? *((DataBool*)pS) : FALSE;
}

//------------------------------------------------------------------------------
EngineScheduler::~EngineScheduler()
{
}

//------------------------------------------------------------------------------
void EngineScheduler::RunSQLCommand()
{
	m_bExecuteCommandWorking = true;

	// questa funzione potrebbe essere potenzialmente eseguita in un sotto-workerthread
	RunInternalCommand(this);

	m_pObject = NULL;
	m_BoolCommand = NULL;
	m_VoidCommand = NULL;
}

//------------------------------------------------------------------------------
BOOL EngineScheduler::ExecuteSQLCommandRetBool(CObject* pObject, BOOL_FUNC aCommand)
{
	m_pObject = pObject;
	m_VoidCommand = NULL;
	m_BoolCommand = aCommand;
	m_CommandResult = FALSE;

	RunSQLCommand();
	return m_CommandResult;
}

//------------------------------------------------------------------------------
void EngineScheduler::ExecuteSQLCommandRetVoid(CObject* pObject, VOID_FUNC aCommand)
{
	m_pObject = pObject;
	m_VoidCommand = aCommand;
	m_BoolCommand = NULL;
	m_CommandResult = FALSE;

	RunSQLCommand();
}

//-----------------------------------------------------------------------------
void EngineScheduler::LoopUntil(volatile const short& nSemaphore, BOOL bFromRunEngine, CTBToolBar* pTBToolBar)
{
	// for tracking the idle time state
	_AFX_THREAD_STATE* pState = AfxGetThreadState();

	while (nSemaphore < 0)
	{
		// phase1: check to see if we can do idle work
		while (m_bIdle &&
			!::PeekMessage(&(pState->m_msgCur), NULL, NULL, NULL, PM_NOREMOVE))
		{
			// call OnIdle while in bIdle state
			if (!AfxGetThread()->OnIdle(m_nCount++))
				m_bIdle = FALSE; // assume "no idle" state
			else
				if (pTBToolBar)
					pTBToolBar->OnUpdateCmdUI((CFrameWnd*)pTBToolBar->GetOwner(), FALSE);
		}

		// phase2: pump messages while available
		do
		{
			if (bFromRunEngine)
			{
				if (m_bExecuteCommandWorking)
					if (::PeekMessage(&(pState->m_msgCur), NULL, NULL, NULL, PM_NOREMOVE))
					{
						if (
							pState->m_msgCur.hwnd == m_hWndComunication &&
							(
								pState->m_msgCur.message == WM_QUIT ||
								pState->m_msgCur.message == WM_COMMAND &&
								(
									pState->m_msgCur.wParam == ID_STOP ||
									pState->m_msgCur.wParam == ID_ESCAPE ||
									pState->m_msgCur.wParam == ID_RUN_STOP
									)
								)
							)
						{
							m_HoldMsg = pState->m_msgCur;
							m_bHoldingMsg = TRUE;
							::GetMessage(&(pState->m_msgCur), NULL, NULL, NULL);
							AfxMessageBox(_TB("Report busy due to time consuming query: please wait for termination."));
							continue;
						}
					}

				if (!m_bExecuteCommandWorking && m_bHoldingMsg)
				{
					m_bHoldingMsg = FALSE;
					::PostMessage(m_HoldMsg.hwnd, m_HoldMsg.message, m_HoldMsg.wParam, m_HoldMsg.lParam);
				}
			}

			// pump message, but quit on QUIT or ID_STOP
			if (AfxGetThread()->PumpMessage() || m_bAborted)
			{
				BOOL bQuerySQLInterrompibile = FALSE;		// TODO Germano
				if (bFromRunEngine && m_bExecuteCommandWorking && bQuerySQLInterrompibile)
				{
					// COME KILLARE LA QUERY ?
					m_bExecuteCommandWorking = false;
				}

				m_bIdle = TRUE;
				m_nCount = 0;
				break;
			}

			// reset "no idle" state after pumping "normal" message
			//if (IsIdleMessage(&m_msgCur))
			if (AfxGetThread()->IsIdleMessage(&(pState->m_msgCur)))
			{
				m_bIdle = TRUE;
				m_nCount = 0;
			}

		} while (::PeekMessage(&(pState->m_msgCur), NULL, NULL, NULL, PM_NOREMOVE));
	}

}

//-----------------------------------------------------------------------------
void EngineScheduler::SetRTError(MessageID nErrCod, const CString & strErrExpl, LPCTSTR str)
{
	if (!m_nErrorCode)
	{
		m_nErrorCode = nErrCod;
		m_strErrorExpl = strErrExpl;
		m_strFailedName = str;
	}
	else if (nErrCod)
		m_strErrorExpl += '\n' + strErrExpl;
}

//-----------------------------------------------------------------------------
void EngineScheduler::ResetRTError()
{
	m_nErrorCode = EMPTY_MESSAGE;
	m_strErrorExpl.Empty();
	m_strFailedName.Empty();
}

//-----------------------------------------------------------------------------
CString EngineScheduler::FormatRTError()
{
	CString nl("\n");
	CString strError = EngineScheduler::FormatMessage(GetRTErrorCode());

	CString	strerrexpl = GetRTErrorExpl();
	if (!strerrexpl.IsEmpty())
	{
		if (!strError.IsEmpty())
			strError += nl;

		strError += strerrexpl;
	}

	if (!strError.IsEmpty())
		strError += nl;

	CString sErrFailedName = GetRTErrorFailedName();
	if (!sErrFailedName.IsEmpty())
		strError += sErrFailedName;

	return strError;
}

//-----------------------------------------------------------------------------
BOOL EngineScheduler::UserBreak()
{
	// nel caso di multithreaded l'engine thread nel caso di pausa si deve 
	// mettere in loop finche non viene fatto ripartire o viene abortito
	// altrimenti la coda deve essere interrogata tramite la CheckMessage()
	do
		if (
			m_bUseMultithreading && m_bAborted ||
			!m_bUseMultithreading && CheckMessage()
			)
			Terminate();
	while (m_bPaused);

	if (m_bAborted) SetRTError(QUERY_RULES_USER_BREAK);

	return m_bAborted;
}

//-----------------------------------------------------------------------------
CString EngineScheduler::FormatMessage(MessageID ID)
{
	switch (ID)
	{
	case EMPTY_MESSAGE: return _T("");
	case QUERY_RULES_USER_BREAK: return _TB("Report interrupted by user");
	case QUERY_RULES_UNK_FIELD_LEN: return _TB("\r\nLength unknown for the %s field");
	case QUERY_RULES_TEMPORARY_CREATE: return _TB("Error creating temporary archive:");
	case QUERY_RULES_TEMPORARY_ORINDEX: return _TB("Error creating sorting index");
	case QUERY_RULES_TEMPORARY_GRINDEX: return _TB("Error creating grouping index");
	case QUERY_EXP_RULE_BACKTRACK: return _TB("Condition never satisfied in evaluation \r\n of field expression rule");
	case QUERY_EXP_RULE_EVAL_EXP_ERR: return _TB("Evaluation error in the expression rule");
	case QUERY_EXP_RULE_EVAL_FILT_ERR: return _TB("Filter evaluation error in the expression rule");
	case QUERY_EXP_RULE_UNK_FIELD: return _TB("Unknown field in the expression rule");
	case QUERY_COND_RULE_BACKTRACK: return _TB("Condition never satisifed in the evaluation of the conditional rule of the field:");
	case QUERY_COND_RULE_EVAL_TEST_ERR: return _TB("IF evaluation error in the conditional rule");
	case QUERY_COND_RULE_EVAL_THEN_ERR: return _TB("THEN evaluation error in the conditional rule");
	case QUERY_COND_RULE_EVAL_ELSE_ERR: return _TB("ELSE evaluation error in the conditional rule");
	case QUERY_COND_RULE_EVAL_FILT_ERR: return _TB("Evaluation error of the filter in the conditional rule");
	case QUERY_COND_RULE_UNK_FIELD: return _TB("Unknown field in the conditional rule");

	case QUERY_DATTBL_RULE_BACKTRACK: return _TB("No data extracted from:");
	case QUERY_DATTBL_RULE_READ_ERR: return _TB("Reading error from archive");
	case QUERY_DATTBL_RULE_WRITE_ERR: return _TB("Error entering a record in the temporary archive");
	case QUERY_DATTBL_RULE_UPDATE_ERR: return _TB("Error updating a record in the temporary archive");
	case QUERY_DATTBL_RULE_EVAL_WHERE_ERR: return _TB("WHERE clause evaluation error");
	case QUERY_DATTBL_RULE_EVAL_HAVING_ERR: return _TB("HAVING clause evaluation error");
	case QUERY_DATTBL_RULE_EVAL_JOINON_ERR: return _TB("JOIN ON clause evaluation error");
	case QUERY_DATTBL_RULE_UNK_FIELD: return _TB("Unknown field in the archive extraction rule");
	case QUERY_DATTBL_RULE_OPEN_ERR: return _TB("Archive opening error");

	case QUERY_EVAL_QUERY_FILTER: return _TB("Filter evaluation error");
	case QUERY_EVAL_GROUP_EXPR: return _TB("Grouping expressing evaluation error");
	case QUERY_EVAL_GROUP_FUNC: return _TB("Grouping function evaluation error");

	case REPORT_EVAL_EVENT_EXPR: return _TB("Expression evaluation error in event");
	case REPORT_EVAL_EVENT_FUNC: return _TB("Evaluation error of the function of the event");
	case REPORT_EVAL_COND_EXPR: return _TB("Conditioned expression evaluation error in event");
	case REPORT_DISP_FIELD: return _TB("Error writing the field value");
	case REPORT_RDE_WRITE_NEWPAGE: return _TB("Error writing NEWPAGE");
	case REPORT_WRITE_TBL_CMD: return _TB("Error writing table command");
	case REPORT_EVAL_INIT_EXPR: return _TB("Evaluation error in initializing field expression");
	case REPORT_ENGINE_TERMINATED: return _TB("Finished");
	case REPORT_INVALID_HELP_FILE: return _TB("Invalid help file");
	case REPORT_EXEC_TABLE_ACTIONS: return _TB("Display actions have been inserted In the BEFORE section of the table that cannot be executed in full table status.\r\nTable:");
	case REPORT_INTERLINE_WHILE_RASTER_FIELDS: return _TB("The InterLine action cannot be run when displaying multi-line fields.\r\nMove this action after the NextLine operation.\r\nTable:");
	case REPORT_DISPLAY_WHILE_RASTER_FIELDS: return _TB("Unable to carry out the Display action for the field while displaying multi-line fields.");

	case REPORT_UNKNOWN_ACTION: return _TB("Unknown action");

	case REPORT_FIELD_NOT_COLUMN: return  _TB("The associated field is not a column");
	case REPORT_ILLEGAL_PRECISION: return  _TB("Illegal number of decimals");
	case REPORT_ILLEGAL_ALIAS: return  _TB("Illegal internal identifier");
	case REPORT_UNKNOWN_DISPLAY_TABLE: return  _TB("Unknown table");
	case REPORT_FIELD_EXIST: return   _TB("Field already defined");
	case REPORT_DISPLAY_TABLE_ALREADY_DEFINED: return  _TB("Table already defined");
	case REPORT_BAD_MESSAGE: return  _TB("Illegal value for the MessageBox");
	case REPORT_PROGRAM_NOT_LOADED: return _TB("The report is not loaded");
	case REPORT_ENGINE_ABORTED: return _TB("Execution not completed");
	case REPORT_PROGRAM_RUNNING: return _TB("Report already running");
	case REPORT_PROGRAM_COMPILED: return _TB("Report already compiled");
	case REPORT_RDE_MANAGER_NOTOPEN: return _TB("RDE channel closed");

	case REPORT_EVAL_WHEN_EXP: return _TB("WHEN evaluation expression error");
	case REPORT_EVAL_ON_EXP: return _TB("Error evaluating ON expression");
	case REPORT_EVAL_ONABORTMESSAGE_EXP: return _TB("Error evaluating ON-ABORT expression");
	case REPORT_DIALOG_ONABORT_MESSAGE: return _TB("Values inserted are not allowable");
	case REPORT_ABORT_MESSAGE: return _TB("No possible to continue");

	case REPORT_AUTO_DISPLAY: return _TB("Automatic display not possible");
	case REPORT_EMPTY_PROC: return _TB("Procedure call does not exist");
	case REPORT_EMPTY_DIALOG: return _TB("Dialog box call does not exist");

	case QUERY_NOT_SAME_FIELD: return _TB("Different fields, illegal conditional rule");
	case QUERY_NOT_YET_IMPLEMENTED: return _TB("Not yet implemented");
	case QUERY_UNKNOWN_FUNCTION: return _TB("Unknown function");
	case QUERY_INCOMPATIBLE_FUNCTION_OPERAND: return _TB("Incompatible function operand");

	case QUERY_RULES_RECURSIVE_ERR: return _TB("Recursive rules");
	case QUERY_RULES_TUPLE_PROC_ERR: return _TB("Error processing record");
	case QUERY_RULES_NORULE_ERR: return _TB("The rule is missing");

	case OSL_MISSING_GRANT: return OSLErrors::MISSING_GRANT();

	case UNKNOWN_FIELD: return _TB("Identifier unknown");
	case UNKNOWN_DB_COL_FIELD: return _TB("Database column unknown");
	case UNKNOWN_PROCEDURE: return _TB("Procedure unknown");

	case REPORT_QUIT: return _T("");	//volutamente stringa vuota: NON modificare 
	case REPORT_UNATTENDED: return _TB("Unatteded Report cannot shows AskDialog");
	case REPORT_ACTION_ERR: return _TB("An action raise an error");

	case DIALOG_MISSING: return _TB("Report does not contains AskDialogs");
	case DIALOG_UNKNOWN: return _TB("Report does not contains this AskDialog ");
	case DIALOG_NOT_ON_ASK: return _TB("This AskDialog is not a request dialog on demand");

	}

	ASSERT(FALSE);
	return _T("");
}

//==============================================================================
//
//			Class ReportEngine implementation
//
//==============================================================================

//---------------------------------------------------------------------------
ReportEngine::ReportEngine(CWoormDoc* pDocument)
	:
	m_strReportPath(""),
	m_hComunicationWnd(NULL),
	m_pCallerDoc(pDocument),
	m_pMainEngine(NULL),
	m_pSymTable(NULL),
	m_bRunning(FALSE),
	m_bCompiled(FALSE),
	m_bKilled(FALSE)
{
	m_pScheduler = new EngineScheduler(this);
}

//---------------------------------------------------------------------------
ReportEngine::~ReportEngine()
{
	SAFE_DELETE(m_pScheduler);
	SAFE_DELETE(m_pSymTable);
}

//---------------------------------------------------------------------------
void ReportEngine::AttachFrame(CWnd* pWnd)
{
	m_hComunicationWnd = pWnd ? pWnd->m_hWnd : NULL;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::RunningMessage()
{
	if (m_bRunning)
		m_pCallerDoc->Message(EngineScheduler::FormatMessage(EngineScheduler::REPORT_PROGRAM_RUNNING), MB_OK | MB_ICONINFORMATION);

	return m_bRunning;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::Load(const CString& strSourcePath)
{
	if (RunningMessage()) return FALSE;

	if (m_bCompiled) FreeMemory();

	m_strReportPath = strSourcePath;
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::Run(
	RDEManager* pOutChannel,
	Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*=0*/,
	BOOL bUsePrevAskInput/*=FALSE*/,
	CFunctionDescription* pWoormInfo/*= NULL*/,
	WoormTable* pSymTable/*= NULL*/)
{
	if (RunningMessage()) return FALSE;

	if (m_pMainEngine == NULL) return TRUE;

	if (pOutChannel->IsClosed())
	{
		m_pCallerDoc->Message(EngineScheduler::FormatMessage(EngineScheduler::REPORT_RDE_MANAGER_NOTOPEN), MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	CString error; long line = 0;
	if (!m_bCompiled && !Compile(error, line, FALSE))
		return FALSE;

	m_bRunning = TRUE;
	m_bKilled = FALSE;

	m_pScheduler->Detach();
	m_pScheduler->Attach(m_hComunicationWnd);
	m_pScheduler->Start();

	m_pSymTable->SetDocument(m_pCallerDoc);

	//-----
	SymField* pLastPageField = NULL;
	if (pSymTable)
	{
		pLastPageField = pSymTable->GetField(SpecialReportField::NAME.LAST_PAGE);
		if (pLastPageField && pLastPageField->GetRefCount())
		{
			m_pCallerDoc->m_bCanPreviewPagesOnPrinting = FALSE;
		}
	}
	//-----
	if (m_pCallerDoc->CanShowEABarCode())
	{
		m_pCallerDoc->m_bCanPreviewPagesOnPrinting = FALSE;
	}

	m_pScheduler->SendMessage(WM_COMMAND, ID_REPORT_BEGIN);

	BOOL bOk = m_pMainEngine->ExecReport
	(
		pOutChannel,
		arOrderOnThisColumns,
		pstrAuxWhereClause,
		&m_arStaticField,
		bUsePrevAskInput,
		pWoormInfo,
		pSymTable
	);

	pOutChannel->Write(0, RDEManager::END_OF_REPORT, 0);

	//-----
	if (pLastPageField)
	{
		ASSERT_VALID(pLastPageField);
		pLastPageField->AssignData(DataLng(pOutChannel->LastPage() + 1));
	}
	//-----

	m_bRunning = FALSE;
	m_pScheduler->SendMessage(WM_COMMAND, ID_REPORT_END);
	m_pScheduler->Terminate();
	m_pScheduler->Detach();

	// ---- 
	//Ad oggi sembrerebbe non servire in quanto gli oggetti TotPagine 
	//hanno problemi solo in stampa(SendMessage ID_FILE_PRINT)/esp.pdf che avviene solo ad engine concluso
		//Ripristina sync fra engine e visualizzatore 
		//if (
		//		pOutChannel->m_pWndComunication
		//	&&
		//		(
		//			!m_pCallerDoc->m_bCanPreviewPagesOnPrinting
		//			&&
		//			m_pCallerDoc->m_bIsPrinting
		//		)
		//	)
		//{
		//	pOutChannel->m_pWndComunication->SendMessage(WM_COMMAND, ID_PG_DN);
		//}
	//----
	return bOk;
}

//---------------------------------------------------------------------------
void ReportEngine::ForceDetach()
{
	m_bRunning = FALSE;

	m_pScheduler->SendMessage(WM_COMMAND, ID_REPORT_END);
	m_pScheduler->Terminate();
	m_pScheduler->Detach();
}

//---------------------------------------------------------------------------
BOOL ReportEngine::SkipGraphSection(Parser& parser)
{
	return parser.SkipToToken(T_REPORT);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::Compile(CString &errors, long &line, BOOL bMustFree, BOOL bFullText, CString sReportText)
{
	if (RunningMessage()) return FALSE;

	if (m_bCompiled && !bFullText)
	{
		m_pCallerDoc->Message(EngineScheduler::FormatMessage(EngineScheduler::REPORT_PROGRAM_COMPILED), MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	// time consuming funtion. Use waiting cursor
	m_bRunning = TRUE;

	CString sReport;
	if (m_pCallerDoc->IsReportString())
		sReport = m_pCallerDoc->GetReportString();

	if (bFullText)
		sReport = sReportText;

	Parser parser(sReport, NULL, -1, TRUE);

	parser.Attach(m_pScheduler);
	m_pScheduler->Start();

	BOOL bMayBeCrypted = AfxGetSecurityInterface()->IsSecurityEnabled();
	if (!m_pCallerDoc->IsReportString())
	{
		if (m_strReportPath.IsEmpty() || !parser.Open(m_strReportPath, TRUE))
		{
			m_pCallerDoc->Message(EngineScheduler::FormatMessage(EngineScheduler::REPORT_PROGRAM_NOT_LOADED), MB_OK | MB_ICONSTOP);
			m_bRunning = FALSE;
			return FALSE;
		}
	}

	m_pSymTable = new WoormTable(WoormTable::RepTable_ENGINE, m_pCallerDoc);

	//COMPILA
	BOOL bOk = SkipGraphSection(parser);
	if (bOk)
		bOk = ParseAndBuild(parser);

	ASSERT_VALID(this);
	ASSERT(m_pScheduler);
	if (m_pScheduler->IsAborted())
	{
		bOk = FALSE;
		parser.ClearError();
	}

	if (!bOk || bMustFree)
		FreeMemory();
	else
		m_bCompiled = TRUE;

	m_bRunning = FALSE;
	m_pScheduler->Terminate();

	errors = parser.BuildErrMsg();
	line = parser.GetCurrentLine();

	if (!bOk)
	{
		parser.ClearError();

		if (m_pCallerDoc->IsInUnattendedMode())
		{
			m_pCallerDoc->Message(errors + L" - " + _TB("Line") + ' ' + cwsprintf(L"%d", line));
		}
		else
		{
			parser.Abort();

			m_pCallerDoc->OpenEditViewFullText(errors, line);
		}
	}
	return bOk;
}

//---------------------------------------------------------------------------
void ReportEngine::FreeMemory()
{
	if (m_pSymTable)
	{
		m_pSymTable->CopyStaticField(m_arStaticField);
		delete m_pSymTable;
	}
	m_pSymTable = NULL;
	m_pMainEngine = NULL;

	m_strReportPath.Empty();
	m_strHelpFile.Empty();

	m_bCompiled = FALSE;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::ParseAndBuild(Parser& parser)
{
	m_pMainEngine = NULL;

	if (parser.Matched(T_REPORT))
	{
		while (parser.LookAhead() != T_PROPERTIES && parser.LookAhead() != T_EOF)
		{
			if (parser.Matched(T_HELP))
			{
				if (!parser.ParseString(m_strHelpFile))
					return FALSE;

				CTBNamespace aReportNS = AfxGetPathFinder()->GetNamespaceFromPath(m_strReportPath);
				m_strHelpFile = AfxGetPathFinder()->GetModuleHelpPath(aReportNS, CPathFinder::STANDARD) + SLASH_CHAR + m_strHelpFile;
				if (m_strHelpFile.IsEmpty())
					return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_INVALID_HELP_FILE));
			}
			parser.ParseBegin();

			RepEngine* pEngine = new RepEngine(this);
			if (m_pMainEngine == NULL) m_pMainEngine = pEngine;

			//@@@ traverso - per gestire piu` engines @@Riccardo ... interessante ...
			m_pSymTable->AddRepEngine(pEngine);

			if (!pEngine->Parse(parser, m_pCallerDoc->m_pTbContext))
				return FALSE;
		}
	}

	if (parser.ErrorFound())
		return FALSE;

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::ExecSpecialProc(LPCTSTR pszProcName)
{
	if (!m_pSymTable)
		return FALSE;
	ASSERT_VALID(m_pSymTable);

	ProcedureObjItem* pProcObj = m_pSymTable->FindProcedure(pszProcName);

	if (pProcObj)
	{
		ASSERT_VALID(pProcObj);
		ASSERT_VALID(pProcObj->m_pProcedure);

		return pProcObj->m_pProcedure->Exec();
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL ReportEngine::OnPreparePrinting()
{
	return ExecSpecialProc(szOnPreparePrinting);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::OnBeginPrinting(BOOL bPreview)
{
	if (!m_pSymTable)
		return FALSE;
	ASSERT_VALID(m_pSymTable);

	SymField* pField = m_pSymTable->GetField(SpecialReportField::NAME.STATUS);

	DataEnum* pRepStatus = (DataEnum*)(pField ? pField->GetData() : NULL);
	if (pRepStatus)
	{
		ASSERT_VALID(pRepStatus);

		if (bPreview)
			pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_PRINTING_PREVIEW);
		else if (*pRepStatus == DataEnum(TAG_REPORT_STATUS, TAG_REPORT_STATUS_PRINTING_PREVIEW))
			pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS);
	}

	return ExecSpecialProc(szOnBeginPrinting);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::OnEndPrinting()
{
	return ExecSpecialProc(szOnEndPrinting);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::OnAbortPrinting()
{
	return ExecSpecialProc(szOnAbortPrinting);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::OnCloseReport()
{
	return ExecSpecialProc(szOnCloseReport);
}

//---------------------------------------------------------------------------
BOOL ReportEngine::IsRunningFromExternalController() const
{
	return m_pCallerDoc && m_pCallerDoc->IsRunningFromExternalController();
}

//---------------------------------------------------------------------------
BOOL ReportEngine::IsEditingParamsFromExternalController() const
{
	return m_pCallerDoc && m_pCallerDoc->IsEditingParamsFromExternalController();
}

//==============================================================================
//          Class CExportInfo implementation
//==============================================================================
//------------------------------------------------------------------------------
CExportInfo::CExportInfo()
	:
	m_bMultiRows(FALSE),
	m_strCSVSep(","),
	m_strDelimStr("\"")
{
	// 19522
	TbGenlibSettings params;
	m_sDateTimeTypeFormat = params.GetExcelDateTimeFormat();
	m_sDateTypeFormat = params.GetExcelDateFormat();
	m_sTimeTypeFormat = params.GetExcelTimeFormat();
	m_FileFormat = CLineFile::UTF8;
	//----
}

CExportInfo::~CExportInfo()
{
	for (int i = 0; i < m_arPlaceHolders.GetSize(); i++)
	{
		CPlaceHolderExportInfo* ph = m_arPlaceHolders[i];
		ASSERT_VALID(ph);
		delete ph;
	}

}

IMPLEMENT_DYNAMIC(CExportInfo, CObject);

CExportInfo& CExportInfo::operator= (CExportInfo& pExp)
{
	if (this == &pExp)
		return *this;

	m_bSync = pExp.m_bSync;
	m_bCurrentPage = pExp.m_bCurrentPage;
	m_bAllPages = pExp.m_bAllPages;
	m_nPagesNum = pExp.m_nPagesNum;
	m_nFromPage = pExp.m_nFromPage;
	m_nToPage = pExp.m_nToPage;
	m_nCurrPage = pExp.m_nCurrPage;

	m_nExportType = pExp.m_nExportType;
	m_bMultiRows = pExp.m_bMultiRows;
	m_strCSVSep = pExp.m_strCSVSep;
	m_strDelimStr = pExp.m_strDelimStr;
	m_strFileName = pExp.m_strFileName;
	m_strSheetName = pExp.m_strSheetName;
	m_nOffsetRow = pExp.m_nOffsetRow;
	m_nOffsetCol = pExp.m_nOffsetCol;
	m_bAutoSave = pExp.m_bAutoSave;

	m_bRepeatColumnTitles = pExp.m_bRepeatColumnTitles;
	m_bColumnTitles = pExp.m_bColumnTitles;
	m_bHiddenColumns = pExp.m_bHiddenColumns;
	m_bEncodeCSV = pExp.m_bEncodeCSV;

	m_sDateTimeTypeFormat = pExp.m_sDateTimeTypeFormat;
	m_sDateTypeFormat = pExp.m_sDateTypeFormat;
	m_sTimeTypeFormat = pExp.m_sTimeTypeFormat;
	m_bisClipboard = pExp.m_bisClipboard;

	for (int i = 0; i < pExp.m_arPlaceHolders.GetSize(); i++)
	{
		m_arPlaceHolders.Add(new CPlaceHolderExportInfo(*(pExp.m_arPlaceHolders[i])));
	}
	return *this;
}

//------------------------------------------------------------------------------
CString	CExportInfo::GetExportFileExtension()
{
	switch (m_nExportType)
	{
	case EXPORT_OPENOFFICE_ODT_TYPE:	return _T("odt");
	case EXPORT_OPENOFFICE_ODS_TYPE:	return _T("ods");

	case EXPORT_EXCELNET_TYPE:		
	case EXPORT_OPENXML_EXCEL_TYPE:		return _T("xlsx");

	case EXPORT_WORDNET_TYPE:		return _T("docx");

	case EXPORT_CSV_TYPE:			return _T("csv");
	case EXPORT_HTML_TYPE:			return _T("html");

	case EXPORT_XML_FULL_TYPE:
	case EXPORT_XML_TYPE:			return _T("xml");

	case EXPORT_JSON_TYPE:			return _T("json");

	case EXPORT_TXTCLIPBOARD_TYPE:
	default:						return _T("");
	}
}
/////////////////////////////////////////////////////////////////////////////
// 							CWoormInfo
/////////////////////////////////////////////////////////////////////////////
//
#undef new
#undef delete

#ifdef _DEBUG
void* CWoormInfo::operator new (size_t size, LPCSTR /*pszFile = NULL*/, int /*nLine = 0*/)
{
	void* p = _malloc_dbg(size, _NORMAL_BLOCK, NULL, 0);

	CThreadContext *pThread = AfxGetThreadContext();
	ASSERT(pThread);
	if (pThread)
	{
		pThread->CollectVoid(p);
	}
	return p;
}

void CWoormInfo::operator delete (void * p, LPCSTR /*pszFile = NULL*/, int /*nLine = 0*/)
{
}

#else

void* CWoormInfo::operator new (size_t size)
{
	void* p = malloc(size);

	CThreadContext *pThread = AfxGetThreadContext();
	ASSERT(pThread);
	if (pThread)
	{
		pThread->CollectVoid(p);
	}
	return p;
}

#endif

void CWoormInfo::operator delete (void *)
{
}

//-----------------------------------------------------------------------------                      
IMPLEMENT_DYNAMIC(CWoormInfo, CFunctionDescription)


//-----------------------------------------------------------------------------                      
CWoormInfo::CWoormInfo()
	:
	CFunctionDescription(CTBNamespace::REPORT)
{
	Init();
}

//-----------------------------------------------------------------------------
CWoormInfo::CWoormInfo(const CFunctionDescription& fd)
	:
	CFunctionDescription(fd)
{
	Init();
}

//-----------------------------------------------------------------------------
CWoormInfo::CWoormInfo(const CWoormInfo& wi)
	:
	CFunctionDescription((const CFunctionDescription&)wi)
{
	Init();

	m_nCopies = wi.m_nCopies;
	m_bLockedCopies = wi.m_bLockedCopies;
	m_bNoPrintDialog = wi.m_bNoPrintDialog;
	m_bPrintAborted = wi.m_bPrintAborted;
	m_bPrinted = wi.m_bPrinted;

	m_bCloseOnEndPrint = wi.m_bCloseOnEndPrint;
	m_bAutoPrint = wi.m_bAutoPrint;
	m_bIconized = wi.m_bIconized;
	m_bOwnedByReport = wi.m_bOwnedByReport;
	m_bIsChildReport = wi.m_bIsChildReport;
	m_bHideFrame = wi.m_bHideFrame;

	m_bSendEmail = wi.m_bSendEmail;
	m_Email = wi.m_Email;
	m_bErrorOnSendEmail = wi.m_bErrorOnSendEmail;

	m_bAttachRDE = wi.m_bAttachRDE;
	m_bAttachOther = wi.m_bAttachOther;
	m_bAttachPDF = wi.m_bAttachPDF;
	m_bCompressAttach = wi.m_bCompressAttach;

	m_bOnePrintDialog = wi.m_bOnePrintDialog;

	m_bMultiCopies = wi.m_bMultiCopies;
	m_bShowPrintDialogBeforeRunning = wi.m_bShowPrintDialogBeforeRunning;
	m_strPrinterName = wi.m_strPrinterName;
	m_bMassiveOperation = wi.m_bMassiveOperation;
	m_bPrintOnLetterHead = wi.m_bPrintOnLetterHead;

	m_bUniqueMail = wi.m_bUniqueMail;
	m_bConcatPDF = wi.m_bConcatPDF;

	m_bPDFOutput = wi.m_bPDFOutput;
	m_bPDFOutputPreview = wi.m_bPDFOutputPreview;
	m_bRDEOutput = wi.m_bRDEOutput;
	m_bShowUI2SendMail = wi.m_bShowUI2SendMail;

	m_arExportInfo.Copy(wi.m_arExportInfo);

	for (int i = 0; i <= wi.m_ReportNames.GetUpperBound(); i++)
		m_ReportNames.Add(wi.m_ReportNames.GetAt(i));

	for (int i = 0; i <= wi.m_arstrOutputFileNames.GetUpperBound(); i++)
		m_arstrOutputFileNames.Add(wi.m_arstrOutputFileNames.GetAt(i));

	m_bIsReportString = wi.m_bIsReportString;
	m_sUICulture = wi.m_sUICulture;

	//DMS
	m_bArchivePdfFormat = wi.m_bArchivePdfFormat;
	m_bArchived = wi.m_bArchived;
	m_pDocumentAttachTo = wi.m_pDocumentAttachTo;
	m_bUseOutputFileName = wi.m_bUseOutputFileName;

	//Errors managment
	m_arErrors.Copy(wi.m_arErrors);
	m_nLine = wi.m_nLine;
	m_nCol = wi.m_nCol;
}

///<summary>
///Object with information about execution of one or more report
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
CWoormInfo::CWoormInfo(DataStr reportNamespace)
	:
	CFunctionDescription(CTBNamespace::REPORT)
{
	Init();

	m_bOwnedByReport = FALSE; //creato tramite web service, non deve essere cancellato dal report

	AddReport(reportNamespace);
}

//-----------------------------------------------------------------------------                      
CWoormInfo::~CWoormInfo()
{
	FreePrinterDeviceMode();
	FreePrinterDeviceNames();
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::SetDevMode(HANDLE hDevMode)
{
	FreePrinterDeviceMode();
	m_hDevMode = CopyHandle(hDevMode);
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::SetDevNames(HANDLE hDevNames)
{
	FreePrinterDeviceNames();
	m_hDevNames = CopyHandle(hDevNames);
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::FreePrinterDeviceMode()
{
	if (m_hDevMode)
	{
		GlobalFree(m_hDevMode);
		m_hDevMode = NULL;
	}
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::FreePrinterDeviceNames()
{
	if (m_hDevNames)
	{
		GlobalFree(m_hDevNames);
		m_hDevNames = NULL;
	}
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::Init()
{
	m_bNoPrintDialog = TRUE;
	m_bOnePrintDialog = FALSE;
	m_bShowPrintDialogBeforeRunning = FALSE;
	m_bPrintAborted = FALSE;
	m_bPrinted = FALSE;
	m_bCloseOnEndPrint = FALSE;
	m_bAutoPrint = FALSE;
	m_bMultiCopies = FALSE;
	m_bMassiveOperation = FALSE;
	m_bPrintOnLetterHead = FALSE;
	m_nNextReport = 0;
	m_nCopies = 1;
	m_bLockedCopies = FALSE;
	m_bOwnedByReport = TRUE;
	m_bIconized = FALSE;
	m_bIsChildReport = FALSE;
	m_bHideFrame = FALSE;
	m_bSendEmail = FALSE;
	m_bAttachRDE = FALSE;
	m_bAttachOther = FALSE;
	m_bAttachPDF = FALSE;
	m_bCompressAttach = FALSE;
	m_bErrorOnSendEmail = FALSE;
	m_bConcatPDF = FALSE;
	m_bRDEOutput = FALSE;
	m_bPDFOutput = FALSE;
	m_bPDFOutputPreview = FALSE;
	m_bUniqueMail = FALSE;
	m_bShowUI2SendMail = FALSE;
	m_bRunReport = TRUE;
	m_hDevMode = NULL;
	m_hDevNames = NULL;
	m_bBlockReRun = FALSE;

	ASSERT(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_pAddOnModules);
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	m_bCompressAttach = params->GetMailCompress();

	m_bIsReportString = FALSE;
	m_sUICulture = AfxGetCulture();

	//EasyAttachment
	m_bArchivePdfFormat = FALSE;
	m_bArchived = FALSE;
	m_bUseOutputFileName = FALSE;
	m_pDocumentAttachTo = NULL;

	//Errors managment
	m_arErrors.RemoveAll();
	m_nLine = -1;
	m_nCol = -1;
}

///<summary>
///Add an element to list report to run
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::AddReport(DataStr reportNamespace)
{
	m_ReportNames.Add(reportNamespace.GetString());
}

///<summary>
///Set automatic print report
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetAutoPrint(DataBool autoPrint)
{
	m_bAutoPrint = autoPrint;
}

///<summary>
///Disable visualisation of print window
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetNoPrintDialog(DataBool noPrintDialog)
{
	m_bNoPrintDialog = noPrintDialog;
}

///<summary>
///Close automatically report when print is finished
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetCloseOnEndPrint(DataBool closeOnEndPrint)
{
	m_bCloseOnEndPrint = closeOnEndPrint;
}

///<summary>
///Run report in backgruond
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetIconized(DataBool iconized)
{
	m_bIconized = iconized;
}

///<summary>
///Hide report window
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetHideFrame(DataBool hideFrame)
{
	m_bHideFrame = hideFrame;
}

///<summary>
///Set unattended report mode
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetSilentMode(DataBool silent)
{
	SetHideFrame(silent);
}

///<summary>
///Send report by e-mail
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetSendEmail(DataBool sendEmail)
{
	m_bSendEmail = sendEmail;
}

///<summary>
///Enclose to e-mail report in RDE format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetAttachRDE(DataBool attachRDE)
{
	m_bAttachRDE = attachRDE;
}

///<summary>
///Enclose to e-mail report in PDF format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetAttachPDF(DataBool attachPDF)
{
	m_bAttachPDF = attachPDF;
}

///<summary>
///Enclose to e-mail report in specified format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetAttachOther(DataBool attachOther, DataStr expType)
{
	m_bAttachOther = attachOther;
	if (attachOther)
		SetExportOutputType(expType);
}

///<summary>
///Archive in PDF format using Easy Attachment
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetArchivePdfFormat(DataBool archive)
{
	m_bArchivePdfFormat = archive;
}

///<summary>
///Compress enclosures send by e-mail
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetCompressAttach(DataBool compressAttach)
{
	m_bCompressAttach = compressAttach;
}

///<summary>
///Show only one print window in case of multiple reports
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetOnePrintDialog(DataBool onePrintDialog)
{
	m_bOnePrintDialog = onePrintDialog;
}

///<summary>
///Send more reports using one e-mail
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetUniqueMail(DataBool uniqueMail)
{
	m_bUniqueMail = uniqueMail;
}

///<summary>
///In case of print of more reports in PDF format, this will be queueing in a single file
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetConcatPDF(DataBool concatPDF)
{
	m_bConcatPDF = concatPDF;
}

///<summary>
///Print report on PDF file
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetPDFOutput(DataBool PDFOutput)
{
	m_bPDFOutput = PDFOutput;
}

///<summary>
///Show report preview in PDF format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetPDFOutputPreview(DataBool PDFOutputPreview)
{
	m_bPDFOutputPreview = PDFOutputPreview;
}

///<summary>
///Allow to set file name on which report will be printed
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::AddOutputFileName(DataStr fileName)
{
	m_arstrOutputFileNames.Add(fileName.GetString());
}

///<summary>
///Save the report in RDE file format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetRDEOutput(DataBool RDEOutput)
{
	m_bRDEOutput = RDEOutput;
}

///<summary>
///Save the report in Ms. Excel file format

///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetExcelOutput(DataBool ExcelOutput)
{
	if (ExcelOutput)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else
	{
		for (int i = 0; i < m_arExportInfo.GetCount(); i++)
		{
			CExportInfo* pEInfo = (CExportInfo*)m_arExportInfo.GetAt(i);
			if (pEInfo->GetExportType() == TypeOfExport::EXPORT_EXCELNET_TYPE)
			{
				m_arExportInfo.RemoveAt(i);
				i--;
			}
		}
	}
}

///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetExportType(DataInt exportType)
{
	if (exportType != 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType((TypeOfExport)(int)exportType);
		m_arExportInfo.Add(pEInfo);
	}
	else
	{
		for (int i = m_arExportInfo.GetCount() - 1; i >= 0; i--)
		{
			CExportInfo* pEInfo = (CExportInfo*)m_arExportInfo.GetAt(i);
			if (pEInfo->GetExportType() == exportType)
			{
				m_arExportInfo.RemoveAt(i);
			}
		}
	}
}

///<summary>
///Manage e-mail \"To\" list
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailTo(DataStr to)
{
	m_Email.SetTo(to.GetString());
}

///<summary>
///Manage e-mail \"Cc\" list
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailCc(DataStr cc)
{
	m_Email.SetCc(cc.GetString());
}

///<summary>
///Manage e-mail \"Bcc\" list
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailBcc(DataStr bcc)
{
	m_Email.SetBcc(bcc.GetString());
}

///<summary>
///Manage e-mail attachments
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailAttach(DataStr file, DataStr title)
{
	m_Email.SetAttachment(file.GetString(), title.GetString(), TRUE);
}

///<summary>
///Manage e-mail \"Subject\" 
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailSubject(DataStr subject)
{
	m_Email.SetSubject(subject.GetString());
}

///<summary>
///Manage e-mail \"Body\" 
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::MailBody(DataStr body, DataBool isHtml)
{
	if (isHtml)
		m_Email.SetHtml(body.GetString(), TRUE, FALSE);
	else
		m_Email.SetBody(body.GetString());
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::SetCopies(int nCopies)
{
	m_nCopies = nCopies;
}

//-----------------------------------------------------------------------------                      
BOOL CWoormInfo::AreCopiesLocked() const
{
	return m_bLockedCopies;
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::LockCopies(BOOL bLocked)
{
	m_bLockedCopies = bLocked;
}

//-----------------------------------------------------------------------------                      
void CWoormInfo::SetMassiveOperation(BOOL bMassiveOperation)
{
	m_bMassiveOperation = bMassiveOperation;
}

//-----------------------------------------------------------------------------                      
BOOL CWoormInfo::GetMassiveOperation()
{
	return m_bMassiveOperation;
}
//-----------------------------------------------------------------------------                      
///<summary>
///Add report parameter by ref(in-out) call mode
///</summary>
//[TBWebMethod(securityhidden=true, report=false)]
void CWoormInfo::AddParameter(DataStr paramName, DataObj* value)
{
	__super::AddInOutParam(paramName, value);
}

//-----------------------------------------------------------------------------                      
///<summary>
///Set report parameter value
///</summary>
//[TBWebMethod(securityhidden=true, report=false)]
void CWoormInfo::SetParamValue(DataStr paramName, DataObj* value)
{
	__super::SetParamValue(paramName, *value);
}

///<summary>
///Allow to set User Interface Culture which report will be printed
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetUICulture(DataStr culture)
{
	m_sUICulture = culture.GetString();
}

///<summary>
///Set the Printer will be used by report
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetPrinterName(DataStr name)
{
	m_strPrinterName = name.GetString();
}

///<summary>
///Set the export type ("PDF", "RDE", "XLS", "DOC", "CSV", "HTML")
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//-----------------------------------------------------------------------------                      
void CWoormInfo::SetExportOutputType(DataStr expType)
{
	if (expType.GetString().CompareNoCase(_T("PDF")) == 0)
	{
		m_bPDFOutput = TRUE;
	}
	else if (expType.GetString().CompareNoCase(_T("RDE")) == 0)
	{
		m_bRDEOutput = TRUE;
	}
	else if (expType.GetString().CompareNoCase(_T("XLS")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("XLSX")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("OXLSX")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_OPENXML_EXCEL_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("ODS")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("ODT")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODT_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("DOC")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("DOCX")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_WORDNET_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("CSV")) == 0 || expType.GetString().CompareNoCase(_T("TXT")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_CSV_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("HTML")) == 0 || expType.GetString().CompareNoCase(_T("HTM")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_HTML_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("XML")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_XML_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
	else if (expType.GetString().CompareNoCase(_T("FXML")) == 0)
	{
		CExportInfo* pEInfo = new CExportInfo();
		pEInfo->SetExportType(TypeOfExport::EXPORT_XML_FULL_TYPE);
		m_arExportInfo.Add(pEInfo);
	}
}

///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CAbstractWoormView, CBCGPScrollView)

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAbstractWoormFrame, CBaseFrame)

BEGIN_MESSAGE_MAP(CAbstractWoormFrame, CBaseFrame)

	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
LRESULT CAbstractWoormFrame::OnGetComponent(WPARAM wParam, LPARAM lParam)
{
	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	int* pIndex = (int*)lParam;
	pResp->OpenObject(_T("component"));
	TCHAR buff[32];
	_itot_s((int)m_hWnd, buff, 10);
	pResp->WriteString(_T("id"), buff);

	CString sName = GetDocument()->GetNamespace().ToUnparsedString();
	sName = sName.TrimRight(_T(".wrm"));

	pResp->WriteString(_T("name"), sName);
	pResp->WriteString(_T("app"), _T("rs"));
	pResp->WriteString(_T("mod"), _T("reportingstudio"));

	pResp->OpenObject(_T("args"));
	pResp->WriteBool(_T("runAtTbLoader"), true);
	pResp->WriteInt(_T("ownerId"), (INT_PTR)GetDocument()->GetCallerDocument());

	CWoormInfo* pInfo = GetDocument()->m_pWoormInfo;
	if (pInfo)
	{
		if (
			GetDocument()->GetCallerDocument() &&
			!GetDocument()->m_pWoormInfo->GetParamDescription(L"ownerId")
			)
			GetDocument()->m_pWoormInfo->AddParam(L"ownerId", &DataLng((long)GetDocument()->GetCallerDocument()));

		DataStr ds;
		if (GetDocument()->m_pWoormInfo->UnparseArguments(ds, L"Arguments"))
		{
			CString js = ds.FormatDataForXML();
			/*
							js.Replace(L"\\", L"\\\\");
							js.Replace(L"/", L"\\/");
							js.Replace(L"\t", L"\\t");
							js.Replace(L"\r", L"\\r");
							js.Replace(L"\n", L"\\n");
							js.Replace(L"\"", L"\\\"");
			*/

			pResp->WriteString(_T("xargs"), ::HTMLEncode(js));
			//pResp->OpenObject(L"xargs");
			//ds.SerializeJsonValue(*pResp);
			//pResp->CloseObject();
		}
		else
			pResp->WriteString(_T("xargs"), L"");

		/*
		//TODO mettere i parametri in un array, altrimenti è un casino parsarli sul server
		// [ {"name":"ownerid", "value":"nnnnn", "type":"long", mode:"in"}, ... ]
					for (int i = 0; i < pInfo->GetParamCount(); i++)
					{
						CDataObjDescription* pParam = pInfo->GetParamDescription(i);
						if (pParam->IsPassedModeIn() || pParam->IsPassedModeInOut())
						{
							pResp->OpenObject(pParam->GetName());
							pParam->GetValue()->SerializeJsonValue(*pResp);
							pResp->CloseObject();
						}
					}
		*/
	}
	pResp->CloseObject();

	pResp->CloseObject();
	return 1L;
}


//-----------------------------------------------------------------------------
void CAbstractWoormFrame::AssignWoormParameters(CString reportOutParameters)
{
	CWoormDoc* pDoc = GetDocument();
	pDoc->m_pWoormInfo->ParseArguments(reportOutParameters);
	pDoc->m_bEngineRunning = FALSE;
	
}
///////////////////////////////////////////////////////////////////////////////
