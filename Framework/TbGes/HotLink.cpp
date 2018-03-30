
#include "stdafx.h"

#include <TBNameSolver\ApplicationContext.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\parsobj.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\Command.h>
#include <TbGenlib\HotlinkController.h>
#include <TbGenlib\Barcode.h>
#include <TbGenlib\SettingsTableManager.h>

#include <TbGenlibManaged\GlobalFunctions.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlaccessor.h>				
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>	

#include <TbWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\askdata.h>
#include <TBWoormEngine\inputmng.h>
#include <TBWoormEngine\prgdata.h>
#include <TBWoormEngine\askdlg.h>
#include <TBWoormEngine\reptable.h>

#include "TbRadarInterface.h"
#include "hotlink.h"
#include "ComposedHotLink.h"
#include "extdoc.h"
#include <TbGes\DocumentSession.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static const TCHAR szDbFieldSeparator[] = _T(",");
static const TCHAR szDbFieldId[] = _T("@dbfield");
static const TCHAR szWhere[] = _T("WHERE");
static const TCHAR szLike[] = _T(" LIKE ?");
static const TCHAR szAnd[] = _T(" AND ");
static const TCHAR szOr[] = _T(" OR ");
static const TCHAR szQualificator[] = _T(".");

//=============================================================================
//					Class HotKeyLink implementation
//=============================================================================
IMPLEMENT_DYNAMIC(HotKeyLink, HotKeyLinkObj)

//@@OLEDB gestire la connessione di lavoro
//CREATO PER LA CLASSE DERIVATA SimulateHotKeyLink da non usare
//-----------------------------------------------------------------------------
HotKeyLink::HotKeyLink()
	:
	HotKeyLinkObj(),
	IOSLObjectManager(OSLType_HotLink),
	m_bRecordAvailable(FALSE),
	m_pRadarDoc(NULL),
	m_pCallLinkDoc(NULL),
	m_pSearchTable(NULL),
	m_PrevResult(NONE),
	m_bDisableDoCallLink(FALSE),
	m_pSymTable(NULL),
	m_pRecord(NULL),
	m_pTable(NULL),
	m_bForceQuery(FALSE),
	m_bSetParamValueByName(FALSE),
	m_nCustomSearch(-1),
	m_pOnGoodRecord(NULL),
	m_bSkipRowSecurity(FALSE),
	m_pRSWorkerID(NULL),
	m_pCatalogEntry(NULL),
	m_nDbFieldRecIndex(-1),
	m_bSkipEmptyDataObj(FALSE)
{
}

//-----------------------------------------------------------------------------
HotKeyLink::HotKeyLink(CRuntimeClass* pClass, CString sAddOnFlyNamespace /*_T("")*/, SqlConnection* pSqlConnection /*= NULL*/)
	:
	HotKeyLinkObj(),
	IOSLObjectManager(OSLType_HotLink),
	m_bRecordAvailable(FALSE),
	m_pRadarDoc(NULL),
	m_pCallLinkDoc(NULL),
	m_pSearchTable(NULL),
	m_PrevResult(NONE),
	m_bDisableDoCallLink(FALSE),
	m_pSymTable(NULL),
	m_bForceQuery(FALSE),
	m_bSetParamValueByName(FALSE),
	m_nCustomSearch(-1),
	m_pOnGoodRecord(NULL),
	m_bSkipRowSecurity(FALSE),
	m_pRSWorkerID(NULL),
	m_pCatalogEntry(NULL),
	m_nDbFieldRecIndex(-1),
	m_bSkipEmptyDataObj(FALSE),
	m_pSqlSession(NULL)
{
	ASSERT(pClass);

	m_pRecord = (SqlRecord*)pClass->CreateObject();
	if (pSqlConnection)
		m_pRecord->SetConnection(pSqlConnection);

	m_sAddOnFlyNamespace = sAddOnFlyNamespace;

	//m_pSqlSession = (pSqlConnection) ? pSqlConnection->GetNewSqlSession() : AfxGetDefaultSqlConnection()->GetNewSqlSession();

	m_pSqlSession = (pSqlConnection) ? pSqlConnection->GetDefaultSqlSession() : AfxGetDefaultSqlSession();

	m_pTable = new SqlTable(m_pRecord, m_pSqlSession);
	m_pTable->SetOnlyOneRecordExpected();

	m_pSymTable = new SymTable();

	SetProtectedState();
}

//-----------------------------------------------------------------------------
HotKeyLink::HotKeyLink(const CString& sTableName, CString sAddOnFlyNamespace /*_T("")*/, SqlConnection* pSqlConnection /*= NULL*/)
	:
	HotKeyLinkObj(),
	IOSLObjectManager(OSLType_HotLink),
	m_bRecordAvailable(FALSE),
	m_pRadarDoc(NULL),
	m_pCallLinkDoc(NULL),
	m_pSearchTable(NULL),
	m_PrevResult(NONE),
	m_bDisableDoCallLink(FALSE),
	m_pSymTable(NULL),
	m_bForceQuery(FALSE),
	m_bSetParamValueByName(FALSE),
	m_nCustomSearch(-1),
	m_pOnGoodRecord(NULL),
	m_bSkipRowSecurity(FALSE),
	m_pRSWorkerID(NULL),
	m_pCatalogEntry(NULL),
	m_nDbFieldRecIndex(-1),
	m_bSkipEmptyDataObj(FALSE),
	m_pSqlSession(NULL)
{
	ASSERT(!sTableName.IsEmpty());

	const CDbObjectDescription* pDescri = AfxGetDbObjectDescription(sTableName);

	m_sAddOnFlyNamespace = sAddOnFlyNamespace;

	m_pRecord = AfxCreateRecord(sTableName);
	//	new SqlRecord(sTableName, NULL, pDescri ? pDescri->GetSqlRecType() : TABLE_TYPE);
	if (pSqlConnection)
		m_pRecord->SetConnection(pSqlConnection);

	//m_pSqlSession = (pSqlConnection) ? pSqlConnection->GetNewSqlSession() : AfxGetDefaultSqlConnection()->GetNewSqlSession();
	m_pSqlSession = (pSqlConnection) ? pSqlConnection->GetDefaultSqlSession() : AfxGetDefaultSqlSession();

	m_pTable = new SqlTable(m_pRecord, m_pSqlSession);
	m_pTable->SetOnlyOneRecordExpected();

	m_pSymTable = new SymTable();

	SetProtectedState();
}

//-----------------------------------------------------------------------------
HotKeyLink::~HotKeyLink()
{
	Free();

	// MUST detach before close radar to avoid ACK-NACK comunication
	if (m_pRadarDoc)
	{
		m_pRadarDoc->Detach();
		m_pRadarDoc->CloseRadar(); //GetDocument()->OnCloseDocument();
	}

	if (m_pCallLinkDoc)
		m_pCallLinkDoc->OnHotLinkDied(this);
}


//-----------------------------------------------------------------------------	
void HotKeyLink::GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	if (!m_pRecord)
		return;
	jsonSerializer.OpenObject(GetName());

	m_pRecord->GetJson(jsonSerializer, bOnlyWebBound);

	jsonSerializer.CloseObject(TRUE);
}
//-----------------------------------------------------------------------------	
void HotKeyLink::GetJsonPatch(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	if (!m_pRecord)
		return;
	jsonSerializer.OpenObject(GetName());
	m_pRecord->GetJsonPatch(jsonSerializer, NULL, bOnlyWebBound);
	jsonSerializer.CloseObject(TRUE);
}
//-----------------------------------------------------------------------------
void HotKeyLink::CloseTable()
{
	// potrebbe non essere mai stata aperta, oppure chiusa d'ufficio
	// dalla tabella LRU
	if (m_pTable)
	{
		if (m_pTable->IsOpen())
			m_pTable->Close();
	}
}

//-----------------------------------------------------------------------------
void HotKeyLink::SetNamespace(const	CTBNamespace& aNamespace)
{
	GetInfoOSL()->m_Namespace = aNamespace;
}

//-----------------------------------------------------------------------------
void HotKeyLink::SetSkipEmptyDataObj(BOOL bValue /*TRUE*/)
{
	m_bSkipEmptyDataObj = bValue;
}


//-----------------------------------------------------------------------------
void HotKeyLink::SetAddOnFlyNamespace(const CString& sNamespace)
{
	if (sNamespace.IsEmpty())
	{
		return;
	}

	// corretto an. su SetAddOnFlyNamespace usata durante le Reattach
	CTBNamespace ns(CTBNamespace::DOCUMENT, sNamespace);

	// corretta an. 11.027 se il namepsace del parent non è ancora disponibile
	// non tento nemmeno di comporre un namespace che sarebbe comunque sbagliato
	if (!ns.IsValid() && GetNamespace().IsEmpty())
	{
		m_sAddOnFlyNamespace = ns.ToString();
		return;
	}


	// Dato per assunto che il tipo lo gestisco io, ammetto che il namespace possa arrivarmi parziale:
	// 1. Solo il nome del documento perchè si trova nello stesso modulo
	// 2. Modulo.Documento perchè si trova nello stessa applicazione
	// 3. App.Modulo.Documento 
	if (!CTBNamespace(sNamespace).IsValid())
		m_AddOnFlyNamespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, sNamespace, GetNamespace());
	else
		m_AddOnFlyNamespace = sNamespace;

	// riallineo la stringa, così se l'InitNamespace viene
	// chiamata dopo le modifiche vengono propagate
	if (!m_sAddOnFlyNamespace.IsEmpty())
		m_sAddOnFlyNamespace = m_AddOnFlyNamespace.ToString();
}

//-----------------------------------------------------------------------------
void HotKeyLink::ClearAddOnFlyNamespace()
{
	m_AddOnFlyNamespace.Clear();
	m_sAddOnFlyNamespace.Empty();
}

//-----------------------------------------------------------------------------
void HotKeyLink::Free()
{
	CloseTable();

	SAFE_DELETE(m_pTable);
	SAFE_DELETE(m_pRecord);
	SAFE_DELETE(m_pSymTable);
}

//-----------------------------------------------------------------------------
void HotKeyLink::GetInfoOsl()
{
	if (GetInfoOSL()->m_Namespace.IsValid())
		return;

	CRuntimeClass* pClass = GetRuntimeClass();
	AddOnModule*  pAddOnMod = AfxGetOwnerAddOnModule(pClass);
	AddOnLibrary* pAddOnLib = pAddOnMod ? pAddOnMod->GetOwnerLibrary(pClass) : NULL;
	FunctionDataInterface* pQCE = pAddOnLib ? (FunctionDataInterface*)pAddOnLib->GetRegisteredObjectInfo(pClass) : NULL;

	for
		(
			CRuntimeClass* rtc = GetRuntimeClass()->m_pfnGetBaseClass();
			pQCE == NULL && rtc != NULL && rtc != RUNTIME_CLASS(HotKeyLink);
			rtc = rtc->m_pfnGetBaseClass == NULL ? NULL : rtc->m_pfnGetBaseClass()
			// nel passaggio da 6.0 a 7.0, m_pfnGetBaseClass di CObject diventa NULL
			// (e non più un puntatore valido a funzione che restituisce NULL)
			)
	{
		pAddOnMod = AfxGetOwnerAddOnModule(rtc);
		pAddOnLib = pAddOnMod ? pAddOnMod->GetOwnerLibrary(rtc) : NULL;
		pQCE = pAddOnLib ? (FunctionDataInterface*)pAddOnLib->GetRegisteredObjectInfo(rtc) : NULL;
		if (pQCE)
			break;
	}

	if (pQCE)
	{
		if (!pQCE->GetInfoOSL()->m_Namespace.IsValid())
		{
			pQCE->GetInfoOSL()->m_Namespace = pQCE->GetNamespace();
		}

		*GetInfoOSL() = *pQCE->GetInfoOSL();
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
	}
}

//-----------------------------------------------------------------------------
void HotKeyLink::InitNamespace()
{
	if (GetInfoOSL()->m_Namespace.IsValid())
		return;

	m_XmlDescription.Clear();

	CRuntimeClass* aParent = this->GetRuntimeClass();
	CString aClass(aParent->m_lpszClassName);

	AddOnLibrary* pAddOnLib = NULL;
	BOOL bNsAssigned = FALSE;
	do
	{
		pAddOnLib = AfxGetOwnerAddOnLibrary(CTBNamespace::HOTLINK, aClass);

		// ne ho trovato la registrazione
		if (pAddOnLib)
		{
			CBaseDescription* pDescri = pAddOnLib->GetRegisteredObjectInfo(CTBNamespace::HOTLINK, aClass);
			if (pDescri)
			{
				ASSERT(pDescri->IsKindOf(RUNTIME_CLASS(FunctionDataInterface)));
				if (!bNsAssigned)
				{
					SetNamespace(pDescri->GetNamespace());
					bNsAssigned = TRUE;
				}

				// compongo la descrizione Xml dell' oggetto
				DefineXmlDescription(pDescri->GetNamespace());

				OnInit((FunctionDataInterface*)pDescri);
			}
		}

		// cerco il padre
		aParent = aParent->m_pfnGetBaseClass();
		if (aParent && aParent != RUNTIME_CLASS(HotKeyLink))
			aClass = aParent->m_lpszClassName;
		else
			aClass.Empty();
	} while (!aClass.IsEmpty());

	if (
		!m_sAddOnFlyNamespace.IsEmpty() &&
		(
			GetNamespace().IsValid() ||
			CTBNamespace(m_sAddOnFlyNamespace).IsValid()
			)
		)
		SetAddOnFlyNamespace(m_sAddOnFlyNamespace);

	CheckXmlDescription();
}

// aggancio il documento all'hotlink 
//-----------------------------------------------------------------------------
void HotKeyLink::AttachDocument(CBaseDocument* pDocument)
{
	//se ho già fatto l'aggancio al documento è inutile ripeterlo
	//(vedi AddLink dell'hotlink. Per gli hotlink creati al volo mediante namespace ha senso mentre è inutile per gli hotlink già agganciati al documento) 
	if (pDocument == m_pDocument)
		return;

	HotKeyLinkObj::AttachDocument(pDocument);
	InitNamespace();
	GetInfoOsl();

	if (pDocument && m_pRecord)
	{
		if (m_pTable)
		{
			CloseTable();
			delete m_pTable;
		}
		
		m_pSqlSession = (m_pDocument && m_pDocument->GetSqlConnection() == m_pRecord->GetConnection()) ? m_pDocument->GetReadOnlySqlSession() : m_pRecord->GetConnection()->GetDefaultSqlSession();		
		
		m_pTable = new SqlTable(m_pRecord, m_pSqlSession, pDocument);
		m_pTable->SetOnlyOneRecordExpected();

		SetProtectedState();
	}
}

// Chiamata dal control su perdita di fuoco (ParsedCtrl::UpdateCtrlData)
//-----------------------------------------------------------------------------
BOOL HotKeyLink::ExistData(DataObj* pData)
{
	if (GetRunningMode() != 0)
		return TRUE;
	OnPrepareForFind(GetMasterRecord());

	FindResult res = FindRecord(pData, IsEnabledAddOnFly(), TRUE);

	return res == FOUND || res == EMPTY;
}

// Chiamata dal control per fermare l'hotlink se e` running
//-----------------------------------------------------------------------------
void HotKeyLink::StopRunning()
{
	if (m_pCallLinkDoc && (GetRunningMode() & (CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB)) != 0)
		OnFormDied();

	if (m_pRadarDoc && (GetRunningMode() & RADAR_FROM_CTRL) == RADAR_FROM_CTRL)
		OnRadarDied(m_pRadarDoc);
}

// Chiamata dal control su richiesta di CallLink diretta da parte dell'utente
// o per dato non trovato dalla FindRecord
//-----------------------------------------------------------------------------
void HotKeyLink::DoCallLink(BOOL bAskForCallLink)
{
	if (m_bDisableDoCallLink)
		return;
	// L'hotlink non e` abilitato a chiamare l'inserimento al volo
	if (!IsEnabledAddOnFly())
		return;

	// Nel caso in cui venga chiamata a seguito di record non trovato
	// (vedi OnFindRecord piu` sotto) e` stato segnalato che l'HotLink
	// e` running, ma in questo caso il control chiama il metodo con
	// bAskForCallLink = TRUE
	//
	if (!bAskForCallLink && GetRunningMode() != 0)
		return;

	// la CallLink poi aggiungera` (o riaggiungera` nella condizione di cui
	// sopra) lo specifico flag di running
	SetRunningMode(CALL_LINK_FROM_CTRL);
	CallLink(GetAttachedData(), bAskForCallLink);
}

//----------------------------------------------------------------------------
void HotKeyLink::DoPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection/* = DIRECT_ACCESS*/)
{
	if (m_bSetParamValueByName)
	{
		DataObjArray arParam;
		PrepareCustomize(arParam);
		Customize(arParam);
	}

	OnPrepareQuery(pDataObj, nQuerySelection);

	///posso far intervenire il documento ed i suoi clientdoc nella modifica della query
	if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		((CAbstractFormDoc*)m_pDocument)->DispatchOnModifyHKLPrepareQuery(this, m_pTable, pDataObj, nQuerySelection);
}

//------------------------------------------------------------------------------
HotKeyLink::FindResult HotKeyLink::FindRecord
(
	DataObj*	pDataObj,
	BOOL		bCallLink,		/* = FALSE */
	BOOL		bFromControl	/* = FALSE */,
	BOOL		bAllowRunningModeForInternalUse/* = FALSE*/
)
{
	ASSERT_VALID(pDataObj);

	if (!m_pTable || !m_pRecord)
	{
		ASSERT_TRACE1(FALSE, "HotKeyLink::FindRecord: %s isn't linked to a SqlRecord!\n", GetRuntimeClass()->m_lpszClassName);
		m_PrevResult = EMPTY;
		return m_PrevResult;
	}

	// Paramento per evitare la ricorsivita' della FindRecord durante Radarate o CallLink
	if (!bAllowRunningModeForInternalUse && GetRunningMode() != 0)
	{
		TRACE("HotKeyLink::FindRecord: %s is in active status!\n", GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		m_PrevResult = EMPTY;
		return m_PrevResult;
	}

	// aggiunto parametro per gestione valori empty in caso di hkl non stringa
	// senza causare regressioni al pregresso esistente
	if (m_bSkipEmptyDataObj && pDataObj->IsEmpty())
	{
		m_PrevResult = EMPTY;
		OnPrepareAuxData();
		return m_PrevResult;
	}

	FindResult eResult = NONE;
	TRY
	{
		// Permette di intervenire sul DataObj associato al control un attimo prima
		// di fare la FindRecord, permettendo cosi' eventuali accessi indiretti o altre
		// possibilita di modifica del DataObj
		//
		OnPreprocessDataObj(pDataObj, bFromControl);

	// In caso di dato di find senza valore non effettua la find
		if (pDataObj->GetDataType() == DATA_STR_TYPE && pDataObj->IsEmpty())
		{
			// devo rifare il settaggio dei parametri per gestire il confronto di query
			if (m_pTable->IsOpen())
			{
				DoPrepareQuery(pDataObj);
			}

			m_pRecord->Init();
			m_PrevResult = EMPTY;
			OnPrepareAuxData();
			return m_PrevResult;
		}

		//ROWSECURITY righe spostate nel costruttore (Riccardo x ottimizzazione find ripetute)
		//const SqlCatalogEntry* pConstCatalogEntry = m_pTable->m_pSqlConnection->GetCatalogEntry(m_pTable->GetRecord()->GetTableName());					
		//BOOL bIsProtected = (pConstCatalogEntry && pConstCatalogEntry->IsProtected());

			if (!m_pTable->IsOpen())
			{
				//@@FORWARD
				//m_pTable->Open(FALSE, E_FAST_FORWARD_ONLY);
				m_pTable->Open();

				//devo controllare se il record richiesto sia sotto protezione e se l'utente abbia o meno i grant
				//pr cui devo temporaneamente togliere la query di filtro 
				//il parametro lo devo aggiungere per primo altrimenti va in coda a tutti gli altri parametri della where
				if (IsProtected())
				{
					m_pTable->SetSkipRowSecurity();
					if (m_pRSWorkerID)
						m_pTable->SetRowSecurityFilterWorker(m_pRSWorkerID);
					m_pTable->SetSelectGrantInformation(m_pRSWorkerID);
				}

				DoDefineQuery();
				DoPrepareQuery(pDataObj);
			}
			else
			{
				DoPrepareQuery(pDataObj);
				if (
					!bFromControl			  &&
					m_PrevResult != NOT_FOUND &&
					m_PrevResult != NONE &&
					m_pTable->SameQuery() &&
					!m_bForceQuery
					)
				{
					OnPrepareAuxData();
					return m_PrevResult;
				}
			}

			// esegue la query scelta
			m_pTable->Query();

			eResult = m_pTable->IsEmpty() ? NOT_FOUND : FOUND;

			//se provengo dal control devo controllare se il record è protetto o meno
			if (m_pCatalogEntry && m_pCatalogEntry->IsProtected() && eResult == FOUND)
				eResult = (m_pCatalogEntry->CanCurrentWorkerUsesRecord(m_pTable->GetRecord(), m_pTable)) ? FOUND : PROTECTED;

			// Effettua il controllo di query
			m_PrevResult = eResult = OnFindRecord
				(
					eResult,
					pDataObj,
					bCallLink,
					bFromControl
				);
			CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
			if (bFromControl && pOwnerCtrl && pOwnerCtrl->GetHotLinkController())
				pOwnerCtrl->GetHotLinkController()->OnAfterFindRecord();			
	}
	CATCH(SqlException, e)
	{
		if (m_pTable && m_pTable->m_pSqlSession)
			m_pTable->m_pSqlSession->ShowMessage(e->m_strError + L" - " + m_pTable->m_strTableName);
		TRACE(e->m_strError);
		Free();
		return NONE;
	}
	END_CATCH

	OnPrepareAuxData();
	return eResult;
}

//------------------------------------------------------------------------------
HotKeyLink::FindResult HotKeyLink::OnFindRecord
(
	HotKeyLink::FindResult	eResult,
	DataObj*				pDataObj,
	BOOL					bCallLink,
	BOOL					bFromControl
)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (eResult == NOT_FOUND)
	{
		// Paramento di culo per evitare la chiamata ricorsiva indiretta
		if (GetRunningMode() != 0)
			return NOT_FOUND;

		if (bCallLink)
		{
			// si pone l'hotlink in running mode
			SetRunningMode(CALL_LINK_MODE);

			if (!AfxIsRemoteInterface() && GetAttachedData() && bFromControl)
			{
				// si aggiunge la segnalazione che e` stato lanciato dal control
				//
				SetRunningMode(GetRunningMode() | CALL_LINK_FROM_CTRL);
				// per evitare il problema di messaggistica mentre si e` in perdita
				// di fuoco del control viene postata la richiesta al control di attivare
				// un inserimento al volo con richiesta di conferma, cioe` il control
				// quando gestira` il messaggio UM_BAD_VALUE chiamera` il metodo DoCallLink
				// della presente classe HotKeyLink
				//
				SetErrorID(CParsedCtrl::HOTLINK_DATA_NOT_FOUND);
			}
			else
			{
				if (AfxIsRemoteInterface())
					SetRunningMode(GetRunningMode() | CALL_LINK_FROM_CTRL_WEB);

				// non si sta effettuando la richiesta tramite il control, ma direttamente
				// da programma e quindi si puo` chiamare la CallLink direttamente
				//
				SetAddOnFlyRunning(TRUE);
				CallLink(pDataObj, TRUE);
				SetAddOnFlyRunning(FALSE);
			}
		}
		else
			if (pOwnerCtrl && bFromControl)
				eResult = OnRecordNotFound();

		// il record viene inizializzato in ogni caso, in quanto, se e` stata
		// chiamata la CallLink verra eventualmente rivalorizzato nella OnRecordAvailable()
		m_pRecord->Init();
		return eResult;
	}

	//Impr. 5185: RowSecurity. Se l'utente ha scelto un record protetto
	if (eResult == PROTECTED)
	{
		// Paramento di culo per evitare la chiamata ricorsiva indiretta
		if (GetRunningMode() != 0)
			return PROTECTED;

		if (GetAttachedData() && bFromControl)
		{
			// per evitare il problema di messaggistica mentre si e` in perdita
			// di fuoco del control viene postata la richiesta al control di attivare
			// un inserimento al volo con richiesta di conferma, cioe` il control
			// quando gestira` il messaggio UM_BAD_VALUE chiamera` il metodo DoCallLink
			// della presente classe HotKeyLink
			//
			if (!AfxIsRemoteInterface())
				SetErrorID(CParsedCtrl::HOTLINK_DATA_PROTECTED);

			m_pRecord->Init();
			return PROTECTED;
		}
	}

	// controllo di congruenza deciso dal programmatore ad alto livello	
	if (!IsValid())
	{
		if (!AfxIsRemoteInterface() && pOwnerCtrl && bFromControl)
			pOwnerCtrl->SetError(m_strError);

		return NOT_VALID;
	}

	if (pOwnerCtrl && bFromControl && !m_strWarning.IsEmpty())
	{
		if (GetAttachedDocument() && GetAttachedDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			((CAbstractFormDoc*)GetAttachedDocument())->SetWarning(m_strWarning);
		}
	}
	m_strWarning.Empty();

	if (m_pOnGoodRecord)
		m_pOnGoodRecord();

	return FOUND;
}

//-----------------------------------------------------------------------------
CString HotKeyLink::GetErrorString(BOOL bClear)
{
	CString s(m_strError);
	if (bClear) m_strError.Empty();
	return s;
}

//-----------------------------------------------------------------------------
CString HotKeyLink::GetWarningString(BOOL bClear)
{
	CString s(m_strWarning);
	if (bClear) m_strWarning.Empty();
	return s;
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::SetErrorString(const CString& strError)
{
	return (m_strError = strError).IsEmpty();
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::SetWarningString(const CString& strWarning)
{
	m_strWarning = strWarning;

	return TRUE;
}

//-----------------------------------------------------------------------------
void HotKeyLink::SetErrorID(CParsedCtrl::MessageID nErrID)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl)
		pOwnerCtrl->SetErrorID(nErrID);
	// TODOBRUNA bada value sul dataobj
}

//-----------------------------------------------------------------------------
HotKeyLink::FindResult HotKeyLink::OnRecordNotFound()
{
	if (m_bMustExistData)
	{
		SetErrorID(CParsedCtrl::HOTLINK_RECORD_NOT_FOUND);
		return NOT_FOUND;
	}

	return FOUND;
}

//-----------------------------------------------------------------------------
void HotKeyLink::EnableCtrl(BOOL bEnable)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (!pOwnerCtrl)
		return;

	/*@@TODO
		DataObj* pDataObj = m_pDataObj;
		ASSERT(pDataObj);

		if (pDataObj && m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			pDataObj->SetReadOnly(!bEnable);
			((CAbstractFormDoc*) m_pDocument)->UpdateDataView();
		}

		// serve perche` il control potrebbe non rispondere alla variazione
		// di stato (autoprotezione)
		m_pOwnerCtrl->EnableCtrl(bEnable);
	*/

	if (bEnable)
	{
		pOwnerCtrl->SetCtrlFocus(TRUE);
		pOwnerCtrl->SetModifyFlag(!m_bRecordAvailable && m_bMustExistData);

		// segnala al control che l'hotlink ha terminato l'attivita`
		pOwnerCtrl->OnHotLinkClosed();

		// dopo questa chiamata l'hotlink potrebbe aver perso il control
		// a causa di ReattachHotLink 
	}
}

//----------------------------------------------------------------------------
bool HotKeyLink::FindNeeded(DataObj* pDataObj, SqlRecord* pMasterRec)
{
	bool b = pDataObj && pDataObj->AlignHKL(this) && !IsHotLinkRunning();
	for (int i = 0; i < m_arAdditionalSensitiveFields.GetCount(); i++)
	{
		DataObj* pAuxDataObj = pMasterRec->GetDataObjFromColumnName(m_arAdditionalSensitiveFields[i]);
		if (pAuxDataObj)
		{
			b = b || pAuxDataObj->AlignHKL(this);
		}
	}
	return b;
}

//----------------------------------------------------------------------------
void HotKeyLink::Parameterize(HotLinkInfo* pInfo, int buttonId)
{
	Bool3 bMustExistData = pInfo->m_bMustExistData;
	Bool3 bEnableAddOnFly = pInfo->m_bEnableAddOnFly;
	Bool3 bEnableLink = pInfo->m_bEnableHyperLink;
	Bool3 bEnableHotLink = pInfo->m_bEnableHotLink;

	if (bEnableAddOnFly != B_UNDEFINED)
	{
		EnableAddOnFly(bEnableAddOnFly == B_TRUE ? TRUE : FALSE);
	}
	if (bMustExistData != B_UNDEFINED)
	{
		MustExistData(bMustExistData == B_TRUE ? TRUE : FALSE);
	}

	if (bEnableLink != B_UNDEFINED)
	{
		EnableHyperLink(bEnableLink == B_TRUE ? TRUE : FALSE);
	}

	if (bEnableHotLink != B_UNDEFINED)
	{
		EnableHotLink(bEnableLink == B_TRUE ? TRUE : FALSE);
	}

	if (!pInfo->m_strAddOnFlyNs.IsEmpty())
	{
		SetAddOnFlyNamespace(pInfo->m_strAddOnFlyNs);
	}
	//il default è true
	if (!pInfo->m_bAutoFind)
		EnableAutoFind(FALSE);

	m_arAdditionalSensitiveFields.Append(pInfo->m_arAdditionalSensitiveFields);
}
// E` chiamata delle SearchOnLinkUpper()/SearchOnLinkLower() inline nel hotlink.h
//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetUpdatedDataObj()
{
	ASSERT(GetRunningMode() == 0);

	// la SearchOnLink() aggiungera` poi lo specifico flag di running
	SetRunningMode(RADAR_FROM_CTRL);
	DataObj* pData = GetAttachedData();
	// ritorna il DataObj del parsed control associato
	ASSERT(pData);

	return pData;
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::IsHotLinkFromControl()
{
	return ((GetRunningMode() & RADAR_FROM_CTRL) == RADAR_FROM_CTRL);
}

// Dal Radar o dalla Form e` stato aggiornato il record dello HotLink pertanto e` necessario
// aggiornare il DataObj del control associato con un preciso dataobj del record 
// di hotlink e per farlo si da` la canches al programmatore di determinarlo (attraverso
// la virtualizzazione della GetDataObj
//-----------------------------------------------------------------------------
void HotKeyLink::RecordAvailable()
{
	// Il record referenziato da m_pRecord e` stato valorizzato dal documento chiamato
	//
	m_bRecordAvailable = TRUE;

	// Si forza lo stato di NOT_FOUND per obbligare una eventuale
	// FindRecord successiva a non chiamare le 	m_pTable->SameQuery() e
	// m_pTable->SameParamValues() (vedi sopra)
	//
	m_PrevResult = NOT_FOUND;

	// potrebbe essere stato cambiato lo stato mentre era running
	if (!IsHotLinkEnabled())
		return;

	// da la chance al programmatore di recuperare i valori dal record appena
	// valorizzato
	OnRecordAvailable();
	OnPrepareAuxData();
	if ((GetRunningMode() & (RADAR_FROM_CTRL | CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB)) == 0)
		return;

	// La chiamata e` avvenuta tramite il control attaccato

	DataObj* pHotLinkRecordData = GetDataObj();
	if (!pHotLinkRecordData)
	{
		ASSERT_TRACE(FALSE, _T("Key data is missing, do you add a reference object xml file ?\n"));
		return;
	}
	DataObj* pData = GetAttachedData();
	ASSERT(pData);
	if (pData)
	{
		ASSERT(pHotLinkRecordData->GetRuntimeClass() == pData->GetRuntimeClass());
		OnAssignSelectedValue(pData, pHotLinkRecordData);
	}

	if (AfxIsRemoteInterface() && (GetRunningMode() & CALL_LINK_FROM_CTRL_WEB) != 0)
	{
		m_pDocument->UpdateDataView();
		return;
	}

	if (m_pRadarDoc)
		m_pRadarDoc->SetCanBeParentWindow(FALSE);//se la ModifiedCtrlData scatenasse una messagebox, non deve prendere il radar come parent

	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl)
	{
		pOwnerCtrl->ModifiedCtrlData();

		pOwnerCtrl->GetCtrlCWnd()->PostMessage
		(pOwnerCtrl->IsFindMode() ? EN_AFTER_VALUE_CHANGED_FOR_FIND_BY_HKL : EN_AFTER_VALUE_CHANGED_BY_HKL, pOwnerCtrl->GetCtrlID(), (LPARAM)0);
		pOwnerCtrl->NotifyToParent
		(pOwnerCtrl->IsFindMode() ? EN_AFTER_VALUE_CHANGED_FOR_FIND_BY_HKL : EN_AFTER_VALUE_CHANGED_BY_HKL, FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::OnValidateRadarSelection(SqlRecord* pRec)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void HotKeyLink::GetFieldsForDBTJoinQuery(DataObjArray* pFieldsForJoin)
{
	if (!m_pRecord)
		return;

	DataObj* pDataObj = m_pRecord->GetDataObjFromColumnName(_T("Description"));
	if (pDataObj)
		pFieldsForJoin->Add(pDataObj);
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnAssignSelectedValue(DataObj* pCtrlData, DataObj* pHKLData)
{
	if (GetRunningMode() & (CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB))
	{
		if (!OnValidateRadarSelection(m_pRecord))
		{
			if (GetAttachedDocument())
				GetAttachedDocument()->Message(GetErrorString(TRUE));

			pCtrlData->Clear();
			return;
		}
		else if (!GetWarningString(FALSE).IsEmpty())
		{
			if (GetAttachedDocument())
				GetAttachedDocument()->Message(GetWarningString(TRUE), 0, 0, 0, CMessages::MSG_WARNING);
		}
	}

	pCtrlData->Assign(*pHKLData);
}

//-----------------------------------------------------------------------------
void HotKeyLink::ActivateWindow(BOOL bPosted)
{
	CWnd*	pParentWnd = NULL;
	BOOL	bFound = FALSE;
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl)
	{
		pParentWnd = pOwnerCtrl->GetCtrlCWnd();

		// risale la catena dei parent cercando una CFormView
		while (!bFound && (pParentWnd = pParentWnd->GetParent()) != NULL)
			if (
				pParentWnd->IsKindOf(RUNTIME_CLASS(CFormView)) ||
				pParentWnd->IsKindOf(RUNTIME_CLASS(CFrameWnd))
				)
			{
				bFound = TRUE;
				break;
			}
			else if (
				/*pParentWnd->IsKindOf(RUNTIME_CLASS(CDynamicDlg)) ||*/
				pParentWnd->IsKindOf(RUNTIME_CLASS(CDynamicContainerTileDlg))
				)
			{
				pParentWnd->BringWindowToTop();
				return;
			}
	}

	if (!bFound)
	{
		if (m_pDocument == NULL)
		{
			if (!pOwnerCtrl || !pOwnerCtrl->GetCtrlCWnd()->GetParent()->IsKindOf(RUNTIME_CLASS(CDialog)))
			{
				TRACE("HotKeyLink::ActivateWindow: HotLink %s is not useful for the Radar or CallLink functionality.\n", GetRuntimeClass()->m_lpszClassName);
				ASSERT(FALSE);
			}

			return;
		}

		POSITION pos = m_pDocument->GetFirstViewPosition();
		ASSERT(pos);

		pParentWnd = m_pDocument->GetNextView(pos);
	}

	CView* pView = dynamic_cast<CView*>(pParentWnd);
	CFrameWnd* pFrame = NULL;
	if (pView)
		pFrame = pView->GetParentFrame();
	else
		pFrame = dynamic_cast<CFrameWnd*>(pParentWnd);

	if (!pFrame)
		return;

	//if (bPosted)
	//{
	//	// non e` possibile usare direttamnente la "CFrameWnd::ActivateFrame()"
	//	// poiche` l'interazione con le azioni che sta conducendo il documento richiamato
	//	// per l'inserimento al volo provoca un incorretto posizionamento del fuoco
	//	//
	//	pFrame->PostMessage(WM_ACTIVATE, (WPARAM)WA_ACTIVE, (LPARAM)pFrame->m_hWnd);
	//}
	//else
	//{
		//Usato invece di ActivateFrame perche la BringToTop chiamata da CFrameWnd::ActivateFrame fallisce.
		//l'attivazione del frame di documento non va a buon fine
		//e di conseguenza il radar non venendo disattivato non muore
	pFrame->SendMessage(WM_ACTIVATE, WA_ACTIVE, (LPARAM)pFrame->m_hWnd);
	pFrame->SetActiveView(pView);
	//}
}

// Viene chiamata la FormView per l'inserimento di un record nel file collegato
// all'HotLink
//-----------------------------------------------------------------------------
void HotKeyLink::CallLink(DataObj* pData /* = NULL */, BOOL bAskForCallLink /* = FALSE */)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// La CallLink puo` essere chiamata dal radar lanciato dal un hotlink agganciato
	// ad un control
	BOOL bFromRadar = pData == NULL &&
		(GetRunningMode() & (RADAR_MODE | RADAR_FROM_CTRL)) == (RADAR_MODE | RADAR_FROM_CTRL) &&
		pOwnerCtrl;

	// Si prosegue solo se l'hotlink ha abilitato l'inserimento al volo
	if (bFromRadar && !IsEnabledAddOnFly())
		return;

	// Ci si protegge da una eventuale doppia chiamata da parte del Radar
	// (vedi sotto)
	//
	if ((GetRunningMode() & (CALL_LINK_MODE | RADAR_MODE)) == (CALL_LINK_MODE | RADAR_MODE))
		return;

	// Si segnala che l'hotlink e` running : DEVE essere fatta PRIMA di fare
	// la domanda di conferma di inserimento poiche` la apertura della nuova finestra
	// fa perdere il fuoco al control e quindi puo` essere utile conoscere che si e`
	// in questa situazione (vedi per esempio BodyEdit)
	//
	// Viene messo in OR poiche` potrebbe essere chiamata dalla DoCallLink che ha segnalato
	// che l'azione proviene da un control, oppure perche` si sta chiedendo l'inserimento/modifica
	// al volo da parte del radar 
	//
	SetRunningMode(GetRunningMode() | CALL_LINK_MODE);
	// nello stato iniziale il dato non e` disponibile
	m_bRecordAvailable = FALSE;

	// Se non e` stato agganciato il documento per l'inserimento al volo o
	// non e` presente un Application manager non e' possibile dirottare
	// la richiesta di inserimento al volo oppure si richiede una conferma
	// all'utente e questi la nega allora il corrente dato e` accettato e viene
	// inviata la notifica al document tramite la ModifiedCtrlData()
	CString sNamespace = GetAddOnFlyNamespace();

	BOOL bCanRun = !sNamespace.IsEmpty() && CTBNamespace(sNamespace).IsValid();
	if (bCanRun)
	{
		//Comunica al clientdoc che è partito un documento al volo
		if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			((CAbstractFormDoc*)m_pDocument)->DispatchBeforeCallLink(pOwnerCtrl);

		if (bAskForCallLink)
		{
			bCanRun = AfxMessageBox
			(
				cwsprintf(_TB("Data {0-%s} not found.\r\n Do you want to enter it now?"), pData ? (LPCTSTR)pData->FormatData() : _T("")),
				MB_YESNO
			) == IDYES;
		}
	}

	WORD wRunningMask = CALL_LINK_MODE | (AfxIsRemoteInterface() ? CALL_LINK_FROM_CTRL_WEB : CALL_LINK_FROM_CTRL);

	// NB: dal radar potrebbe esser stata attivata la CallLink, in tal caso
	// l'HotLink e` anche in stato di running da radar e quindi non deve essere
	// fatta alcuna gestione del control associato
	//
	BOOL bFromCtrl = pOwnerCtrl && GetRunningMode() == wRunningMask;

	if (!bCanRun)
	{
		// si segnala che la CallLink non e` piu` attiva
		SetRunningMode(GetRunningMode() & ~wRunningMask);

		// Non si effettua l'inserimento al volo quindi il dato corrente
		// e` valido e si fa procedere la segnalazione di "VALUE_CHANGED"
		// che era stata sospesa per permettere l'eventuale inserimento
		// (vedi ParsedCtrl::UpdateCtrlData(): ExistData ....)
		//
		if (bFromCtrl && pOwnerCtrl)
		{
			if (m_bMustExistData)
				pOwnerCtrl->RestoreOldCtrlData(TRUE);
			else
				pOwnerCtrl->ModifiedCtrlData();

			// Si segnala anche che la CallLink si sta chiudendo
			EnableCtrl(TRUE);
		}

		return;
	}

	if (bFromCtrl && bAskForCallLink && pOwnerCtrl)
		pOwnerCtrl->SetCtrlFocus();

	// Se la CallLink e` stata chiamata dal radar, chiamato dal control, allora
	// viene utilizzato il contenuto del DataObj associato al control per proporre
	// il valore in input
	//
	if (bFromRadar)
	{
		pData = GetAttachedData();
		ASSERT(pData);

		// e` come se la CallLink fosse stata chiamata direttamente dal control:
		// l'attivazione del documento fa si` che il radar si "suicidi" chiamando la
		// OnRadarDied() che rimette a posto i flag relativi al radar
		//
		SetRunningMode(GetRunningMode() | CALL_LINK_FROM_CTRL);
	}

	if (sNamespace.IsEmpty() || !CTBNamespace(sNamespace).IsValid())
	{
		ASSERT(FALSE);
		return;
	}

	CDocument* pFormDoc = NULL;
	// se si tratta di una funzione, mi faccio ritornare l'handle del documento 
	// aperto dalla funzione
	if (CTBNamespace(sNamespace).GetType() == CTBNamespace::FUNCTION)
	{
		CFunctionDescription aFunction;
		AfxGetTbCmdManager()->GetFunctionDescription(CTBNamespace(sNamespace), aFunction);

		AfxGetTbCmdManager()->RunFunction(&aFunction);

		DataObj* pHandle = aFunction.GetReturnValue();
		if (pHandle && pHandle->GetDataType() == DataType::Long && !((DataLng*)pHandle)->IsEmpty())
			pFormDoc = (CDocument*)*((long*)(DataLng*)pHandle);
	}
	else
	{
		LPAUXINFO pAuxInfo = NULL;
		GetAuxInfoForHklBrowse(pAuxInfo);
		pFormDoc = AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, pAuxInfo, NULL, NULL, NULL, FALSE, NULL, NULL, (m_pDocument) ? m_pDocument->m_pContextBag : NULL);
	}

	if (!pFormDoc || !pFormDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		AfxMessageBox(_TB("Unable to call the insert document!"), MB_ICONSTOP);

		// dal radar potrebbe esser stata attivata la CallLink, in tal caso
		// l'HotLink e` anche in stato di running da radar e quindi non deve essere
		// fatta alcuna gestione del control associato
		//
		bFromCtrl = pOwnerCtrl && GetRunningMode() == wRunningMask;

		// si segnala che la CallLink non e` piu` attiva
		SetRunningMode(GetRunningMode() & ~wRunningMask);

		if (bFromCtrl && !bFromRadar && pOwnerCtrl)
		{
			if (m_bMustExistData)
				pOwnerCtrl->RestoreOldCtrlData(TRUE);
			else
				pOwnerCtrl->ModifiedCtrlData();

			// Si segnala anche che la CallLink si sta chiudendo
			EnableCtrl(TRUE);
		}

		return;
	}

	m_pCallLinkDoc = (CAbstractFormDoc*)pFormDoc;

	DataObj* pHotLinkRecordData = GetDataObj();

	ASSERT(pHotLinkRecordData);

	// prepara il record pulendolo
	m_pRecord->Init();

	if (pData)
	{
		ASSERT(pHotLinkRecordData->GetRuntimeClass() == pData->GetRuntimeClass());

		// valorizza il dataobj per l'inserimento
		pHotLinkRecordData->Assign(*pData);
	}

	// da la chance al programmatore di valorizzare altri campi del record
	// con cui deve aprirsi per default il documneto chiamato
	OnCallLink();

	// adesso la Form attaccata puo' restituire l'intero record con il 
	// Dataobj valorizzato. L'handle serve per sapere se l'HotLink e` ancora vivo
	((CAbstractFormDoc*)pFormDoc)->ConnectForm(this, m_pRecord);

	// la disabilitazione invece DEVE essere fatta DOPO (vedi BodyEdit: il
	// comando SHOW_HIDE non viene gestito se il control e` disabilitato)
	if (pOwnerCtrl && (GetRunningMode() & (CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB)))
	{
		if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			((CAbstractFormDoc*)m_pDocument)->DispatchOnHotLinkRun();

		EnableCtrl(FALSE);
	}

	((CAbstractFormDoc*)pFormDoc)->OnDisableControlsForCallLink();
}

//-----------------------------------------------------------------------------
CDocument* HotKeyLink::BrowserLink
(
	DataObj*		pData,
	CDocument*		pFormDoc, /*= NULL*/
	const	CRuntimeClass*	pViewClass, /*= NULL*/
	BOOL			bActivate   /*= TRUE*/
)
{
	if (pData == NULL || pData->IsEmpty())
		return pFormDoc;
	OnPrepareForFind(GetMasterRecord());
	//ATTENZIONE: 
	//Devo chiamare la DoCallLink virtuale per allineare eventuali ridefinizioni 
	//dei data entry richiamati per il browse dedotti dagli IDHL degli inserimenti al volo
	//il flag m_bDisableDoCallLink rende nulla la chiamata alla HotKeyLink::DoCallLink
	m_bDisableDoCallLink = TRUE;
	m_bIsAddOnFlyRunning = FALSE;
	DoCallLink(FALSE);
	m_bDisableDoCallLink = FALSE;
	//----
	FindResult eResult = FindRecord(pData, FALSE, TRUE);
	if (eResult != FOUND)
	{
		if (eResult == PROTECTED)
			AfxMessageBox(_TB("The data is protected. In order to see it you must have grants "), MB_ICONSTOP);

		return pFormDoc;
	}

	BOOL bCanRun = !GetAddOnFlyNamespace().IsEmpty();
	if (!bCanRun)
	{
		return NULL;
	}

	if (!pFormDoc)
	{
		CString sNamespace = GetAddOnFlyNamespace();
		if (sNamespace.IsEmpty() || !CTBNamespace(sNamespace).IsValid())
		{
			ASSERT(FALSE);
			return NULL;
		}
		// se si tratta di una funzione, mi faccio ritornare l'handle del documento 
		// aperto dalla funzione
		if (CTBNamespace(sNamespace).GetType() == CTBNamespace::FUNCTION)
		{
			CFunctionDescription aFunction;
			AfxGetTbCmdManager()->GetFunctionDescription(CTBNamespace(sNamespace), aFunction);

			AfxGetTbCmdManager()->RunFunction(&aFunction);

			DataObj* pHandle = aFunction.GetReturnValue();
			if (pHandle && pHandle->GetDataType() == DataType::Long && !((DataLng*)pHandle)->IsEmpty())
				pFormDoc = (CDocument*)*((long*)(DataLng*)pHandle);
		}
		else
		{
			LPAUXINFO pAuxInfo = NULL;
			GetAuxInfoForHklBrowse(pAuxInfo);
			pFormDoc = AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, pAuxInfo, NULL, NULL, NULL, FALSE, NULL, NULL, (m_pDocument) ? m_pDocument->m_pContextBag : NULL);
		}

		// Potrebbe fallire la creazione del documento associato all'inserimento al volo	
		if (!pFormDoc || !pFormDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			AfxMessageBox(_TB("Unable to call the insert document!"), MB_ICONSTOP);
			return NULL;
		}
	}

	if (pFormDoc && pFormDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pFormDoc;
		if (pDoc->GetFormMode() != CBaseDocument::BROWSE)
		{
			pDoc->SaveModified();
			pDoc->GoInBrowseMode();
		}

		if (bActivate)
			pDoc->Activate();

		pDoc->GoInBrowserMode(m_pRecord);
	}

	return pFormDoc;
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnFormRecordAvailable()
{
	// parte comune all form linkata o al radar
	RecordAvailable();
	// need to activate hotlink window, in order supply a valid main window
	// for potential message boxes
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnFormDied()
{
	SetRunningMode(GetRunningMode() & ~CALL_LINK_MODE);
	m_pCallLinkDoc = NULL;
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if ((GetRunningMode() & (CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB)) != 0)
	{
		BOOL bFromWeb = (GetRunningMode() & CALL_LINK_FROM_CTRL_WEB) != 0;
		SetRunningMode(GetRunningMode() & ~(CALL_LINK_FROM_CTRL | CALL_LINK_FROM_CTRL_WEB));

		if (!bFromWeb && !m_bRecordAvailable && pOwnerCtrl)
			pOwnerCtrl->RestoreOldCtrlData(TRUE);

		if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			((CAbstractFormDoc*)m_pDocument)->DispatchOnHotLinkStop();

		if (pOwnerCtrl)
		{
			ActivateWindow(TRUE);
			// Si segnala anche che la CallLink si sta chiudendo
			EnableCtrl(TRUE);
		}
	}
}

// Chiama il radar dopo aver correttamente preparato la opportuna query
// si ricorda che la tabella e` di proprieta` dello HotLink.
// Il radar restituische il record (se piu` record soddisfano la query, li presenta
// a video e ritorna quello scelto dall'utente). E` compito del radar verificare
// l'esistenmza della tabella, aprirla, fare la query effettiva, chiudere la tabella.
//
// nQuerySelection serve per poter far definire all'utente diversi criteri di query
// default 0 e` per chiave primaria. Il programmatore se desidera solo una queri puo`
// ignorare il parametro
//-----------------------------------------------------------------------------
BOOL HotKeyLink::SearchOnLink(DataObj* pData, SelectionType nQuerySelection)
{
	OnPrepareForFind(GetMasterRecord());
	// Se e` stata chiamata dalle SearchOnLinkUpper/SearchOnLinkLower
	// la GetUpdatedDataObj() ha gia` messo il flag di running da control,
	// solo in questo caso si accetta un RunningMode != 0
	//
	if ((GetRunningMode() & ~RADAR_FROM_CTRL) != 0)
		return FALSE;

	// NON deve esserci un radar attaccato
	ASSERT(m_pRadarDoc == NULL);

	GetInfoOsl();

	if (!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE))
	{
		EnableCtrl(FALSE);
		AfxMessageBox(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString()));
		EnableCtrl(TRUE);
		SetRunningMode(GetRunningMode() & ~RADAR_FROM_CTRL);
		return FALSE;
	}

	// Si segnala che l'hotlink e` running : DEVE essere fatta PRIMA di fare
	// la chiamata al Radar poiche` la apertura della nuova finestra fa perdere
	// il fuoco al control e quindi puo` essere utile conoscere che si e` in questa
	// situazione (vedi per esempio BodyEdit)
	//
	SetRunningMode(GetRunningMode() | RADAR_MODE);

	// Alloca la Table localmente perche' viene manipolata dal Radar
	ASSERT(m_pSearchTable == NULL);

	// é una tabella con cursore scrollable
	//m_pSearchTable = new SqlTable(m_pRecord, (m_pDocument) ? m_pDocument->GetReadOnlySqlSession() : AfxGetDefaultSqlSession()); 

	// é una tabella con cursore scrollable
	// se non viene passato il puntatore al documento, utilizzo come connessione quella su cui è istanziato il SqlRecord
	//vedi miglioria #4732
	//m_pSearchTable = new SqlTable(m_pRecord, (m_pDocument && m_pDocument->GetSqlConnection() == m_pRecord->GetConnection()) ? m_pDocument->GetReadOnlySqlSession() :  m_pRecord->GetConnection()->GetDefaultSqlSession());
	m_pSearchTable = new SqlTable(m_pRecord, m_pSqlSession);

	const SqlCatalogEntry* pConstCatalogEntry = m_pTable->m_pSqlConnection->GetCatalogEntry(m_pTable->GetRecord()->GetTableName());
	BOOL bIsProtected = (pConstCatalogEntry && pConstCatalogEntry->IsProtected());
	//Copy the RowSecurity properties
	if (bIsProtected)
	{
		if (m_bSkipRowSecurity)
			m_pSearchTable->SetSkipRowSecurity();
		if (m_pRSWorkerID)
			m_pSearchTable->SetRowSecurityFilterWorker(m_pRSWorkerID);
		m_pSearchTable->SetSelectGrantInformation(NULL);
	}


	// swappa la tabella perche' le varie On...Query usano direttamente m_pTable
	// che e' diverso a seconda che si faccia delle search o delle direct access    

	SqlTable* pTable = m_pTable;
	m_pTable = m_pSearchTable;


	// definisce la condizione di ORDER BY e di WHERE
	DoDefineQuery(nQuerySelection);

	//if (bIsProtected)
	//	pConstCatalogEntry->SelectGrantInformation(m_pTable); //mi serve poi per oscurare i dati sensibili		


	// valorizza i parametri per la query
	m_pRecord->Init();

	// nello stato iniziale il dato non e` disponibile
	m_bRecordAvailable = FALSE;

	DoPrepareQuery(pData, nQuerySelection);

	// terminata la preparazione della query riaggiusto il puntatore m_pTable
	m_pTable = pTable;

	m_pRadarDoc = AfxCreateTBRadar(this, m_pSearchTable, m_pRecord, nQuerySelection);

	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// la disabilitazione invece DEVE essere fatta DOPO (vedi BodyEdit: il
	// comando SHOW_HIDE non viene gestito se il control e` disabilitato)
	if (pOwnerCtrl && (GetRunningMode() & RADAR_FROM_CTRL) == RADAR_FROM_CTRL)
		EnableCtrl(FALSE);

	return TRUE;
};

//-----------------------------------------------------------------------------
void HotKeyLink::OnRadarDied(ITBRadar* pRadarDoc)
{
	//ASSERT(m_pSearchTable);

	// rimuove lo statement
	if (m_pSearchTable)
	{
		if (m_pSearchTable->IsOpen())
			m_pSearchTable->Close();
		delete m_pSearchTable;
		m_pSearchTable = NULL;
	}
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// caso mai arrivasse un radar del belino (si puo` dire perche` e` genovese)
	ASSERT(pRadarDoc == m_pRadarDoc);
	m_pRadarDoc = NULL;

	WORD wRunningMask = RADAR_MODE | RADAR_FROM_CTRL;

	// dal radar potrebbe esser stata attivata la CallLink, in tal caso
	// l'HotLink e` anche in stato di running da CallLink e quindi non deve essere
	// fatta alcuna gestione del control associato
	//
	BOOL bFromCtrl = pOwnerCtrl && GetRunningMode() == wRunningMask;

	// si segnala che il radar non e` piu` attivo
	SetRunningMode(GetRunningMode() & ~wRunningMask);
	if (bFromCtrl)
	{
		if (!m_bRecordAvailable)
			pOwnerCtrl->RestoreOldCtrlData(TRUE);

		// Non si chiama la HotLinkStop poiche` per il radar non viene chiamata
		// la OnHotlinkRun

		// Se e` stato ritornato un record questa azione e` gia` stata fatta
		// dalla OnRadarRecordAvailable
		if (!m_bRecordAvailable)
		{
			// faccio rifindare l'hotlink per pulire le descrizioni
			// nel caso si siano sporcate
			FindRecord(GetAttachedData());
			ActivateWindow(FALSE);
		}

		// Si segnala anche che il radar si sta chiudendo
		EnableCtrl(TRUE);
	}
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnRadarRecordAvailable()
{
	// parte comune all form linkata o al radar
	RecordAvailable();

	if (m_pOnGoodRecord)
		m_pOnGoodRecord();

	// NB. L'attivazione della finestra che contiene il control possessore dell'HotLink
	// va fatta qui poiche` e` a seguito di questa che il radar, sentendosi disattivato
	// si "suicida" chiamando la OnRadarDied
	//
// 04/06/2010 Germano & Marco :
// Per gestire correttamente la messaggistica di errore dopo avere selezionato il record
// dal Radar, si è deciso di "uccidere" direttamente il radar in modo da far chiamare
// la HotKeyLink::OnRadarDied prima che il control perda il fuoco dando Assert di "HotLink Runnig"
// La CloseRadar DEVE essere chiamata con bSend = FALSE (che è il default) in quanto una SendMessage
// del WM_CLOSE comunque provocava il problema summenzionato di "HotLink Runnig"
//
//	if (m_pOwnerCtrl &&	(m_wRunningMode & RADAR_FROM_CTRL) == RADAR_FROM_CTRL)
//		ActivateWindow(FALSE);
//	else	
//	{
		// Non essendo stato chiamto dal control il radar viene ucciso esplicitamente
	if (m_pRadarDoc)
		m_pRadarDoc->CloseRadar();
	//	}
}

//-----------------------------------------------------------------------------
int HotKeyLink::DoSearchComboQueryData(const int& nMaxItems, DataObjArray& arKeyData, CStringArray& arDescriptions)
{
	OnPrepareForFind(GetMasterRecord());
	int nret = SearchComboQueryData(nMaxItems, arKeyData, arDescriptions);

	///posso far intervenire il documento ed i suoi clientdoc nella modifica della query
	if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		nret = max(nret, ((CAbstractFormDoc*)m_pDocument)->DispatchOnModifyHKLSearchComboQueryData(this, nMaxItems, arKeyData, arDescriptions));
	return nret;
}

// ritorna i dati selezionati dalla Combo Query
//-----------------------------------------------------------------------------
int HotKeyLink::SearchComboQueryData(const int& nMaxItems, DataObjArray& arKeyData, CStringArray& arDescriptions)
{
	SqlRecord*	pOriginalRecord = NULL;
	SqlTable*	pOriginalTable = NULL;

	SqlRecord* pRecord = m_pRecord->Create();
	ASSERT_TRACE(GetDbFieldValue(pRecord), "DbField was not set");

	// se non viene passato il puntatore al documento, utilizzo come connessione quella su cui è istanziato il SqlRecord
	//vedi miglioria #4732
	//SqlTable* pTable = new SqlTable(pRecord, (m_pDocument && m_pDocument->GetSqlConnection() == pRecord->GetConnection()) ? m_pDocument->GetReadOnlySqlSession() :  pRecord->GetConnection()->GetDefaultSqlSession());
	SqlTable* pTable = new SqlTable(pRecord, m_pSqlSession);
	int nResult = 1;
	TRY
	{
		pTable->Open(FALSE, E_FAST_FORWARD_ONLY);
	//Copy the RowSecurity properties
	pTable->SetRowSecurityFilterWorker(m_pTable->GetRowSecurityFilterWorker());

	// scambio i puntatori originali per Define e Prepare
	pOriginalRecord = m_pRecord;
	m_pRecord = pRecord;
	pOriginalTable = m_pTable;
	m_pTable = pTable;

	HotKeyLink::SelectionType nSelection = HotKeyLink::COMBO_ACCESS;

	DoDefineQuery(nSelection);

	// gestione del default: HotKeyLink::UPPER_BUTTON)
	if (pTable->m_strSelect.IsEmpty() && pTable->m_strFilter.IsEmpty() && pTable->m_strSort.IsEmpty())
	{
		pTable->ClearQuery();

		nSelection = HotKeyLink::UPPER_BUTTON;
		DoDefineQuery(nSelection);
	}

	DataObj* pDataObj = GetDataObj();
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// evita problemi di refresh su mancato
	// DoKillFocus quando si riapre la tendina
	if (pOwnerCtrl)
	{
		DataObj* pData = GetAttachedData();
		if (pData)
			pDataObj = pData->DataObjClone();

		// se il ctrl gestisce primo e ultimo, devo ricordarmi di eliminarli
		pOwnerCtrl->GetValue(*pDataObj);
		if ((pOwnerCtrl->GetCtrlStyle() & CTRL_STYLE_SHOW_FIRST) == CTRL_STYLE_SHOW_FIRST)
		{
			if (AfxGetCultureInfo()->IsEqual(pDataObj->Str(), CParsedCtrl::Strings::FIRST()))
				pDataObj->Clear();
		}
		else if ((pOwnerCtrl->GetCtrlStyle() & CTRL_STYLE_SHOW_LAST) == CTRL_STYLE_SHOW_LAST)
		{
			if (AfxGetCultureInfo()->IsEqual(pDataObj->Str(), CParsedCtrl::Strings::LAST()))
				pDataObj->Clear();
		}
	}

	ASSERT(pDataObj);
	if (pDataObj->IsKindOf(RUNTIME_CLASS(DataArray)))
	{
		DataStr dsDummy;
		DoPrepareQuery(&dsDummy, nSelection);
	}
	else
		DoPrepareQuery(pDataObj, nSelection);

	// in questa tipologia di query non ammetto filtri in like su chiave 
	if (!m_bLikeOnDropDownEnabled && !pTable->m_strFilter.IsEmpty())
		RemovePrimaryKeyLike(pTable, pRecord);

	// se sono arrivata a scambiare i 
	// puntatori ripristino gli originali
	if (pOriginalRecord)
	{
		m_pRecord = pOriginalRecord;
		pOriginalRecord = NULL;
	}

		if (pOriginalTable)
		{
			m_pTable = pOriginalTable;
			pOriginalTable = NULL;
		}
	
		// Oracle doesn't support TOP sintax
		if (nMaxItems > 0 && m_pRecord->GetConnection()->GetDBMSType() != DBMS_ORACLE)
		{
			pTable->AddSelectKeyword(SqlTable::TOP, nMaxItems + 1);
		}

		pTable->Query();
		PrepareFormatComboItem(m_pTable->GetRecord());

	int idxDbField = GetDbFieldRecIndex(pRecord);
	if (idxDbField < 0)
	{
		//ASSERT(FALSE);
		nResult = 0;
		if (pOwnerCtrl)
			delete pDataObj;
		goto l_end;
	}

	for (int nCount = 1; !pTable->IsEOF() && (nMaxItems < 0 || nCount <= nMaxItems); pTable->MoveNext(), nCount++)
	{
		DataObj* pData = GetKeyData(pRecord, idxDbField);
		ASSERT_VALID(pData);

		arKeyData.Add(pData->DataObjClone());

		arDescriptions.Add(FormatComboItem(pRecord));
	}

		if (pOwnerCtrl)
			delete pDataObj;
		
		// è nr. massimo di elementi richiesti
		if (!pTable->IsEOF())
			nResult = 2;
		
	
	}

		CATCH(SqlException, e)
	{
		if (pTable)
		{
			if (pTable->m_pSqlSession)
				pTable->m_pSqlSession->ShowMessage(e->m_strError);

			if (pTable->IsOpen())
				pTable->Close();

			delete pTable;
		}
		delete pRecord;

		//m_pSqlSession->Close();

		// se sono arrivata a scambiare i 
		// puntatori ripristino gli originali
		if (pOriginalRecord)
			m_pRecord = pOriginalRecord;
		if (pOriginalTable)
			m_pTable = pOriginalTable;

		nResult = 0;
		THROW_LAST();
	}
	END_CATCH
		l_end :
	if (pTable->IsOpen())
		pTable->Close();

	delete pTable;
	delete pRecord;

	//m_pSqlSession->Close();

	return nResult;
}

//-----------------------------------------------------------------------------
// Si occupa di eliminare la LIKE per chiave primaria dalla stringa SQL. Questa
// funzione si basa sui seguenti assiomi:
//	1)	di LIKE ce ne può essere solo una per filtro
//	2)	possono essere presenti nella stringa più filtri dati da SubQueries
//	3)	le LIKE della SubQueries sono accopiate ad una WHERE che le precede
//	4)	non sono in grado di riconoscere il parametro nell'array di Param dal
//		DataObj associato (viene fatto un clone)
//	5)	L'array di Param è ordinato per AddParam, quindi per ? inseriti dalla
//		AddFilter (unico vincolo attuale nella preparazione della query).
//		Conoscendo il numero associato al ? della LIKE che tolgo, conosco
//		anche l'elemento dell'Array di cui devo fare la Remove
void HotKeyLink::RemovePrimaryKeyLike(SqlTable* pTable, SqlRecord* pRec)
{
	// chiedo il DataObj legato al control
	DataObj* pKeyField = GetDbFieldValue(pRec);
	if (!pKeyField)
		return;

	// nome del campo di chiave primaria
	CString sKeyField = pRec->GetColumnName(pKeyField);
	if (sKeyField.IsEmpty())
		return;

	RemovePrimaryKeyLike(pTable->m_strFilter, sKeyField, pTable->GetTableName(), pTable);
}

BOOL HotKeyLink::RemovePrimaryKeyLike(CString& sFilter, CString sKeyField, CString sTable, SqlTable* pTable/* = NULL*/)
{
	sKeyField.MakeUpper();
	sFilter.MakeUpper();
	// cerco direttamente con la chiave
	CString sToSearch = sKeyField + szLike;

	int nLikePos = sFilter.Find(sToSearch);

	// se non c'è salto subito
	if (nLikePos < 0)
		return TRUE;

	// ne ho trovato almeno una ma potrebbe essercene di più !
	// Mi preoccupo di escludere quelle delle SubQueries e
	// di scegliere quella vera.
	int nStart = nLikePos;
	int nWherePos = sFilter.Find(szWhere, 0);

	//Devo escludere i nomi campi che includono la Where
	TCHAR c1 = ' ';
	if (nWherePos > 0)
	{
		c1 = sFilter.GetAt(nWherePos - 1);
	}
	TCHAR c2 = ' ';
	if (nWherePos >= 0 && (nWherePos + 5) < sFilter.GetLength())
	{
		c2 = sFilter.GetAt(nWherePos + 5);
	}
	if (c1 != ' ' || c2 != ' ')
		nWherePos = -1;

	// se ho la prima Like prima delle Where è quella primaria,
	// altrimenti vado a cercarmela nella stringa sql per coppie
	if (nLikePos > nWherePos)	//FIX c'era <
		do
		{
			//FIX if (nLikePos > nWherePos)
			//	continue;	//LOOP infinito break o return ?
			if (nLikePos < nWherePos)
				break;
			//----

			nWherePos = sFilter.Find(szWhere, nStart + 1);
			//Devo escludere i nomi campi che includono la Where
			TCHAR c1 = ' ';
			if (nWherePos > 0)
			{
				c1 = sFilter.GetAt(nWherePos - 1);
			}
			TCHAR c2 = ' ';
			if (nWherePos >= 0 && (nWherePos + 5) < sFilter.GetLength())
			{
				c2 = sFilter.GetAt(nWherePos + 5);
			}
			if (c1 != ' ' || c2 != ' ')
				nWherePos = -1;

			//se trova una WHERE LOOP infinito 
			if (nWherePos < 0)
			{
				nStart = sFilter.Find(szLike, nStart + 1);
				if (nStart > 0)
				{
					nLikePos = nStart;
					break;
				}

				// se non ho trovato la LIKE priva di Where 
				// allora la LIKE primaria non esiste
				return TRUE;
			}
		} while (true);

		// ora determino il numero del parametro che dovrò eliminare.
		// Conto i precedenti fino alla LIKE che andrò ad Eliminare
		int nrOfParam = 0;
		nStart = 0;
		do
		{
			nStart = sFilter.Find(_T("?"), nStart + 1);
			if (nStart > 0 && nStart < nLikePos)
				nrOfParam++;
		} while (nStart < nLikePos);

		//non mi corrisponde il nr. o il tipo di parametri, lascio tutto come è
		if (
			pTable &&
			(nrOfParam < 0 || nrOfParam > pTable->m_pParamArray->GetUpperBound() ||
				pTable->m_pParamArray->GetAt(nrOfParam) == NULL ||
				pTable->m_pParamArray->GetAt(nrOfParam)->GetDataType() != DataType::String)
			)
			return FALSE;

		//FIX
		CString s = sFilter.Mid(nLikePos, sToSearch.GetLength());
		if (s != sToSearch && (nLikePos - sKeyField.GetLength()) >= 0)
		{
			nLikePos -= sKeyField.GetLength();
			s = sFilter.Mid(nLikePos, sToSearch.GetLength());;
		}

		// controllo se devo eliminare anche un AND dopo
		if (sFilter.Mid(nLikePos + sToSearch.GetLength(), 5).CompareNoCase(szAnd) == 0)
			sToSearch += szAnd;

		// controllo se devo eliminare anche la qualificazione della tabella
		if (nLikePos > 0 && sFilter[nLikePos - 1] == '.')
		{
			CString sTableName = sTable + szQualificator;
			// se la qualificazione non appartiene alla stessa tabella, vuol dire che sto sbagliando 
			// e quindi preferisco lasciare stare tutto.
			if (sFilter.Mid(nLikePos - sTableName.GetLength(), sTableName.GetLength()).CompareNoCase(sTableName) != 0)
			{
				ASSERT(FALSE);
				return FALSE;
			}

			nLikePos -= sTableName.GetLength();
			sToSearch = sTableName + sToSearch;
		}

		// elimino dalla filter la stringa e il param corrispondente
		sFilter = sFilter.Left(nLikePos) + sFilter.Mid(nLikePos + sToSearch.GetLength());

		sFilter.Trim();

		CString sToDelete;
		sToDelete = szAnd;
		sToDelete.Trim();

		// verifico che non finisca in AND o OR
		if (sFilter.Right(sToDelete.GetLength()).CompareNoCase(sToDelete) == 0)
			sFilter = sFilter.Left(sFilter.GetLength() - sToDelete.GetLength());

		sToDelete = szOr;
		sToDelete.Trim();

		if (sFilter.Right(sToDelete.GetLength()).CompareNoCase(sToDelete) == 0)
			sFilter = sFilter.Left(sFilter.GetLength() - sToDelete.GetLength());

		if (pTable) pTable->m_pParamArray->RemoveAt(nrOfParam);
		return TRUE;
}

/* ---- Test effettuati per A. 17913 - S. 212884
{
	CString sFilter;

sFilter.Empty();
sFilter += L"Storage LIKE ? AND Disabled = ? AND";
sFilter += L" (  EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage AND DG_FilterStorages.BusinessUnit = 'TV' )";
sFilter += L"  OR  NOT EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage  )  ) AND ";
	RemovePrimaryKeyLike (sFilter, _T("Storage"), _T("MA_Storages"));

	sFilter.Empty();
sFilter += L"Disabled = ? AND Storage LIKE ? AND";
sFilter += L" (  EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage AND DG_FilterStorages.BusinessUnit = 'TV' )";
sFilter += L"  OR  NOT EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage  )  ) AND ";
	RemovePrimaryKeyLike (sFilter, _T("Storage"), _T("MA_Storages"));

sFilter.Empty();
sFilter += L" (  EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage AND DG_FilterStorages.BusinessUnit = 'TV' )";
sFilter += L"  OR  NOT EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage  )  ) AND ";
sFilter += L"Disabled = ? AND Storage LIKE ?";
	RemovePrimaryKeyLike (sFilter, _T("Storage"), _T("MA_Storages"));

sFilter.Empty();
sFilter += L" (  EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage AND DG_FilterStorages.BusinessUnit = 'TV' )";
sFilter += L"  OR  NOT EXISTS ( SELECT Storage FROM DG_FilterStorages ";
sFilter += L"WHERE MA_Storages.Storage = DG_FilterStorages.Storage  )  ) AND ";
sFilter += L"Storage LIKE ? AND Disabled = ?";
	RemovePrimaryKeyLike (sFilter, _T("Storage"), _T("MA_Storages"));

}
*/

//-----------------------------------------------------------------------------
SqlParamArray* HotKeyLink::GetQuery(SelectionType nQuerySelection, CString& sQuery, CString sFilter /*= _T("")*/)
{
	// sono allocati nel costruttore
	if (!m_pTable || !m_pRecord)
		return NULL;

	// se non è ma stata aperta la chiudo, potrei dover cambiare di define
	if (m_pTable->IsOpen())
		m_pTable->Close();

	TRY
	{
		m_pTable->Open(FALSE, E_FAST_FORWARD_ONLY);
		DoDefineQuery(nQuerySelection);
		DataObj* pData = GetDataObj();
		pData->AssignFromXMLString(sFilter);
		DoPrepareQuery(pData, nQuerySelection);
		m_pTable->BuildSelect();
	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH

	sQuery = m_pTable->ToString(FALSE, FALSE, TRUE);

	// per avere i parametri devo tenere la tabella ancora
	// aperta. Chi usa questa funzione dovrà ricordarsi di
	// chiamare la CloseTable
	return m_pTable->m_pParamArray;
}

// ritorna il dataobj che sarebbe da collegare al dato del control
//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetDbFieldValue(SqlRecord* pRec) const
{
	ASSERT_VALID(pRec);

	CString sDBNames = m_XmlDescription.GetDbField();
	sDBNames.Trim();
	if (sDBNames.IsEmpty())
	{
		//ASSERT(FALSE);
		return NULL;
	}

	CStringArray ar;
	CStringArray_Split(ar, sDBNames, L",");

	for (int i = 0; i < ar.GetSize(); i++)
	{
		CString sName = ar[i];
		int pos = sName.Find('.');
		if (pos > 0)
			sName = sName.Mid(pos + 1);

		int idx = pRec->GetIndexFromColumnName(sName);
		if (idx < 0 && pos > 0)
			idx = pRec->GetIndexFromColumnName(ar[i]);
		if (idx < 0)
			continue;

		DataObj* pDataObj = pRec->GetDataObjAt(idx);

		const_cast<HotKeyLink*>(this)->m_nDbFieldRecIndex = idx;

		return pDataObj;
	}
	TRACE2("HotKeyLink::GetDbFieldValue: column (%s) shows in tag <DbField> of ReferenceObject\n(%s) not exist in the table.\n",
		m_XmlDescription.GetDbField(), m_XmlDescription.GetNamespace().ToString());
	return NULL;
}

//-----------------------------------------------------------------------------
int HotKeyLink::GetDbFieldRecIndex(SqlRecord* pRec) const
{
	ASSERT_VALID(pRec);

	if (m_nDbFieldRecIndex < 0)
	{
		DataObj* pField = GetDbFieldValue(pRec);
		if (pField)
		{
			const_cast<HotKeyLink*>(this)->m_nDbFieldRecIndex = pRec->GetIndexFromDataObj(pField);
		}
	}
	return m_nDbFieldRecIndex;
}

//-----------------------------------------------------------------------------
void HotKeyLink::SplitFieldName(const CString& sName, CString& sTblName, CString& sColName)
{
	sTblName.Empty();

	if (sName.IsEmpty())
		return;

	// cerco la qualifica
	int nPos = sName.Find('.');

	if (nPos >= 0)
	{
		sTblName = sName.Left(nPos);
		sColName = sName.Mid(nPos + 1);
	}
	else
		sColName = sName;
}

//-----------------------------------------------------------------------------
// prepara la formattazione del contenuto della combo sulla base della descrizione statica
BOOL HotKeyLink::PrepareFormatComboItem(SqlRecord* pRec)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	m_arColumnAuxDescr.RemoveAll();

	int nColumns = m_XmlDescription.GetComboBox().GetSize();
	for (int i = 0; i < nColumns; i++)
	{
		CComboColumnDescription* pColDescri = (CComboColumnDescription*)m_XmlDescription.GetComboBox().GetAt(i);
		ASSERT_VALID(pColDescri);

		ColumnAuxDescr* pColAux = new ColumnAuxDescr();
		m_arColumnAuxDescr.Add(pColAux);
		//----

		if (pColDescri->GetSource().IsEmpty())
			continue;

		pColAux->m_nRecIndexDbField = -1;
		CStringArray ar;
		CStringArray_Split(ar, pColDescri->GetSource(), L",");
		for (int i = 0; i < ar.GetSize(); i++)
		{
			CString sName = ar[i];

			pColAux->m_nRecIndexDbField = pRec->GetIndexFromColumnName(sName);
			if (pColAux->m_nRecIndexDbField >= 0)
				break;
		}

		DataObj* pDataObj = NULL;
		if (pColAux->m_nRecIndexDbField >= 0)
		{
			pDataObj = pRec->GetDataObjAt(pColAux->m_nRecIndexDbField);
		}

		if (pDataObj)
		{
			LPCTSTR lpsFormatter = NULL;

			if (!pColDescri->GetFormatter().IsEmpty())
			{
				lpsFormatter = pColDescri->GetFormatter();
			}
			else if (
				pOwnerCtrl &&
				pColDescri->GetSource().CompareNoCase(m_XmlDescription.GetDbField()) == 0 &&
				pDataObj->GetDataType() == GetAttachedData()->GetDataType()

				)	// control formatter is to apply only to the same hotlink control data
			{
				pColAux->m_pFormatter = AfxGetFormatStyleTable()->GetFormatter(pOwnerCtrl->GetFormatIdx(), &(GetInfoOSL()->m_Namespace));
			}

			if (lpsFormatter)
			{
				pColAux->m_pFormatter = AfxGetFormatStyleTable()->GetFormatter(lpsFormatter, NULL);
				ASSERT_TRACE1(pColAux->m_pFormatter, "Formatter not found %s", lpsFormatter);
			}

			if (pColAux->m_pFormatter == NULL)
			{
				pColAux->m_pFormatter = AfxGetFormatStyleTable()->GetFormatter(pDataObj->GetDataType(), NULL);
				ASSERT_TRACE(pColAux->m_pFormatter, "Base formatter for the type not found");
			}
		}
	}

	return m_arColumnAuxDescr.GetSize() > 0;
}

//-----------------------------------------------------------------------------
// formatta il contenuto della combo sulla base della descrizione statica
CString	HotKeyLink::FormatComboItem(SqlRecord* pRec)
{
	if (m_arColumnAuxDescr.GetCount() == 0)
		return SlowFormatComboItem(pRec);

	UpdateSymbolTable(pRec);

	CComboColumnDescription* pColDescri;
	CString sComboItem, sColumnName;
	DataObj* pDataObj;

	for (int i = 0; i <= m_XmlDescription.GetComboBox().GetUpperBound(); i++)
	{
		pColDescri = (CComboColumnDescription*)m_XmlDescription.GetComboBox().GetAt(i);
		ASSERT_VALID(pColDescri);

		// valutazione della when
		if (!IsColumnToDisplay(pColDescri->GetWhen()))
			continue;

		// label
		if (!pColDescri->GetLabel().IsEmpty())
			sComboItem += pColDescri->GetLabel() + ' ';

		ColumnAuxDescr* pColAux = (ColumnAuxDescr*)m_arColumnAuxDescr.GetAt(i);

		if (pColAux->m_nRecIndexDbField >= 0)
		{
			pDataObj = pRec->GetDataObjAt(pColAux->m_nRecIndexDbField);

			sColumnName.Empty();
			if (pColAux->m_pFormatter)
				pColAux->m_pFormatter->FormatDataObj(*pDataObj, sColumnName);
			else
				sColumnName = pDataObj->Str();

			// lunghezza indicata nella descrizione
			if (pColDescri->GetLength() > 0/* && sColumnName.GetLength() > pColDescri->GetLength()*/)
				sColumnName = sColumnName.Left(pColDescri->GetLength());

			sComboItem += sColumnName + ' ';
		}
	}

	// se c' è ancora uno spazio in fondo lo rimuovo
	sComboItem.TrimRight();

	return sComboItem;
}

//-----------------------------------------------------------------------------
CString	HotKeyLink::SlowFormatComboItem(SqlRecord* pRec)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// se non ho la descrizione Xml formatto con il dato principale
	DataObj* pDataObj = NULL;
	if (m_XmlDescription.GetDbField().IsEmpty())
	{
		pDataObj = GetDbFieldValue(pRec);
		return pDataObj ? pDataObj->FormatData() : _T("");
	}

	UpdateSymbolTable(pRec);

	CComboColumnDescription* pColDescri;
	CString sComboItem;

	for (int i = 0; i <= m_XmlDescription.GetComboBox().GetUpperBound(); i++)
	{
		pColDescri = (CComboColumnDescription*)m_XmlDescription.GetComboBox().GetAt(i);
		if (!pColDescri)
			continue;

		// valutazione della when
		if (!IsColumnToDisplay(pColDescri->GetWhen()))
			continue;

		// label
		if (!pColDescri->GetLabel().IsEmpty())
			sComboItem += pColDescri->GetLabel() + _T(" ");

		// se ho il nome del campo
		CString sColumnName;
		CString sTableName;
		if (!pColDescri->GetSource().IsEmpty())
			SplitFieldName(pColDescri->GetSource(), sTableName, sColumnName);

		// cerco la colonna nel SqlRecord
		if (sColumnName.IsEmpty())
			pDataObj = NULL;
		else
		{
			pDataObj = pRec->GetDataObjFromColumnName(sColumnName);
		}
		if (pDataObj)
		{
			LPCTSTR lpsFormatter = NULL;

			if (!pColDescri->GetFormatter().IsEmpty())
				lpsFormatter = pColDescri->GetFormatter();
			// control formatter is to apply only to the same hotlink control data
			else if (
				pOwnerCtrl &&
				pRec->GetTableName().CompareNoCase(m_pRecord->GetTableName()) == 0 &&
				pColDescri->GetSource().CompareNoCase(m_XmlDescription.GetDbField()) == 0
				)
			{
				Formatter* pCtrlFormatter = AfxGetFormatStyleTable()->GetFormatter(pOwnerCtrl->GetFormatIdx(), &(GetInfoOSL()->m_Namespace));
				lpsFormatter = pCtrlFormatter ? pCtrlFormatter->GetName() : NULL;
			}

			sColumnName = pDataObj->FormatData(lpsFormatter);

			// lunghezza indicata nella descrizione
			if (pColDescri->GetLength() > 0 && sColumnName.GetLength() > pColDescri->GetLength())
				sColumnName = sColumnName.Left(pColDescri->GetLength());

			sComboItem += sColumnName + _T(" ");
		}
	}
	// se c' è ancora uno spazio in fondo lo rimuovo
	sComboItem.TrimRight();

	return sComboItem;
}

// valuta l'espressione che indica se la colonna va visualizzata
//-----------------------------------------------------------------------------
BOOL HotKeyLink::IsColumnToDisplay(const CString& sWhenExpr)
{
	if (sWhenExpr.IsEmpty() || !m_pSymTable)
		return TRUE;

	Expression	tmpExpr(m_pSymTable);
	Parser		lex(sWhenExpr);

	if (!tmpExpr.Parse(lex, DataType::Bool, FALSE))
		return FALSE;

	// valuto l'espressione
	DataBool bValue;
	tmpExpr.Eval(bValue);

	return bValue;
}

//----------------------------------------------------------------------------
void HotKeyLink::LoadSymbolTable()
{
	//Per evitare schiantamento nel caso un campo non esista nella tabella
	if (!m_pRecord || !m_pRecord->IsValid() || !m_pSymTable)
	{
		//ASSERT(FALSE);
		return;
	}

	m_pSymTable->RemoveAll(); m_arSymFieldsMappedToRecFieldIndex.RemoveAll();

	for (int i = 0; i <= m_XmlDescription.GetParameters().GetUpperBound(); i++)
	{
		CDataObjDescription* pParam = (CDataObjDescription*)m_XmlDescription.GetParameters().GetAt(i);
		if (!pParam || pParam->GetName().IsEmpty())
			continue;

		SymField* pField = new SymField
		(
			pParam->GetName(),
			pParam->GetValue()->GetDataType(),
			0,
			pParam->GetValue(),
			/*pParam->GetPassedMode() == CDataObjDescription::_IN ? TRUE : */FALSE	//bind diretto cosi' usufruisce della SetParamValue
		);

		pField->GetData()->SetValid(TRUE);	// not valid dataobjs cannot be evaluated by expression

		if (pParam->GetValue()->GetDataType() == DataType::Object)
		{
			pField->AddMethods(pParam->GetClassType(), AfxGetAddOnAppsTable()->GetMapWebClass());
		}

		pField->SetTitle(pParam->GetTitle());

		m_pSymTable->Add(pField);
	}

	DataObj* pDataObj; SymField* pField;
	int sz = m_pRecord->GetSizeEx();
	m_arSymFieldsMappedToRecFieldIndex.SetSize(sz);
	for (int i = 0; i < sz; i++)
	{
		pDataObj = m_pRecord->GetDataObjAt(i);
		ASSERT_VALID(pDataObj);

		pField = new SymField
		(
			m_pRecord->GetQualifiedColumnName(pDataObj, TRUE),
			pDataObj->GetDataType(),
			0,
			pDataObj,
			FALSE
		);
		m_pSymTable->Add(pField);

		m_arSymFieldsMappedToRecFieldIndex[i] = pField;
	}
}

//----------------------------------------------------------------------------
void HotKeyLink::UpdateSymbolTable(SqlRecord* pRecord)
{
	if (!pRecord || !m_pSymTable)
	{
		ASSERT(FALSE);
		return;
	}

	DataObj* pRecDataObj(NULL);
	DataObj* pSymDataObj(NULL);
	SymField* pF(NULL);
	int n = m_arSymFieldsMappedToRecFieldIndex.GetSize();
	ASSERT(n == pRecord->GetSizeEx() || !this->m_XmlDescription.IsLoadFullRecord());
	for (int i = 0; i < n; i++)
	{
		pF = m_arSymFieldsMappedToRecFieldIndex.GetAt(i);
		ASSERT_VALID(pF);

		if (pF->GetRefCount() == 0)
			continue;

		pRecDataObj = pRecord->GetDataObjAt(i);
		ASSERT_VALID(pRecDataObj);

		pSymDataObj = pF->GetData();
		ASSERT_VALID(pSymDataObj);
		pSymDataObj->Assign(*pRecDataObj);
		pSymDataObj->SetValid(TRUE);
	}
}

// Si occupa di definire la descrizione Xml completa dell'hotlink tenendo
// conto anche dell'ereditarietà tra i parents. Questo metodo viene chiamato 
// a ritroso sul ramo dei parent, quindi integra la descrizione Xml partendo 
// dalle informazioni del figlio e salendo nella parentela.
//-----------------------------------------------------------------------------
void HotKeyLink::DefineXmlDescription(const CTBNamespace& aNamespace)
{
	// se ho un namespace valido provo a cercare la mia descrizione Xml
	if (!aNamespace.IsValid())
		return;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNamespace);
	if (!pAddOnMod)
		return;

	// leggo la descrizione corrispondente al namespace
	CHotlinkDescription aDescription = *pAddOnMod->m_XmlDescription.GetReferencesInfo().GetHotlinkInfo(aNamespace);

	// la prima volta assegno tutto
	if (m_XmlDescription.GetNamespace().GetType() == CTBNamespace::NOT_VALID)
		m_XmlDescription = aDescription;
	else
	{
		// prima il field di codice
		if (m_XmlDescription.GetDbField().IsEmpty())
			m_XmlDescription.SetDbField(aDescription.GetDbField());

		if (m_XmlDescription.GetDbTable().IsEmpty())
			m_XmlDescription.SetDbTable(aDescription.GetDbTable());

		if (m_XmlDescription.GetDbFieldDescription().IsEmpty())
			m_XmlDescription.SetDbFieldDescription(aDescription.GetDbFieldDescription());

		// il primo che ha la combo la usa
		if (aDescription.HasComboBox())
			m_XmlDescription.SetHasComboBox(TRUE);

		// colonne della combo
		CComboColumnDescription* pDescri;
		if (aDescription.GetComboBox().GetSize() && !m_XmlDescription.GetComboBox().GetSize())
			for (int i = 0; i <= aDescription.GetComboBox().GetUpperBound(); i++)
			{
				pDescri = (CComboColumnDescription*)aDescription.GetComboBox().GetAt(i);
				m_XmlDescription.AddComboColumn(new CComboColumnDescription(*pDescri));
			}
	}
}

// si occupa di verificare la descrizione Xml completa dell'hotlink ed 
// eventualmente la integra con i defaults
//-----------------------------------------------------------------------------
void HotKeyLink::CheckXmlDescription()
{
	if (!m_XmlDescription.HasComboBox())
	{
		m_bEnableFillListBox = FALSE;
		return;
	}

	// le colonne sono state definite. Ok, le uso
	if (m_XmlDescription.GetComboBox().GetSize())
		return;

	DataObj* pStrFields = AfxGetSettingValue(snsTbGenlib, szFormsSection, szHotlinkComboDefaultFields, DataStr(), szTbDefaultSettingFileName);

	// non ho nemmeno i defaults, allora disabilito la funzione
	if (!pStrFields || pStrFields->Str().IsEmpty())
	{
		m_bEnableFillListBox = FALSE;
		return;
	}

	// gestione dei defaults
	CString sFields = pStrFields->Str();

	int nCurrPos = -1;
	int nNexSepPos = 0;
	CString sField;

	do
	{
		CComboColumnDescription* pNewCol = new CComboColumnDescription();

		nNexSepPos = sFields.Find(szDbFieldSeparator, nCurrPos + 1);
		sField = sFields.Mid(nCurrPos + 1, nNexSepPos > 0 ? nNexSepPos - nCurrPos - 1 : sFields.GetLength());

		// verifico se è il dbfield
		pNewCol->SetSource(sField.CompareNoCase(szDbFieldId) == 0 ? m_XmlDescription.GetDbField() : sField);
		m_XmlDescription.GetComboBox().Add(pNewCol);

		nCurrPos = nNexSepPos;
	} while (nCurrPos >= 0 && nCurrPos <= sFields.GetLength());

	// se non ho la definizione base disabilito la funzionalità
	if (m_bEnableFillListBox)
		m_bEnableFillListBox = !m_XmlDescription.GetDbField().IsEmpty();
}

//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetDataObj() const
{
	return GetDbFieldValue(m_pRecord);
}

//-----------------------------------------------------------------------------
//BOOL HotKeyLink::DispatchCustomize (const DataObjArray& params)
//{
//	m_arCustomizeParameters.RemoveAll();
//	m_arCustomizeParameters = params;
//
//	return Customize (params);
//}

//-----------------------------------------------------------------------------
void HotKeyLink::PrepareCustomize(DataObjArray& params)
{
	params.RemoveAll();
	for (int i = 0; i < m_XmlDescription.GetParameters().GetSize(); i++)
	{
		CDataObjDescription* pDescrPar = (CDataObjDescription*)m_XmlDescription.GetParameters().GetAt(i);
		DataObj* var = pDescrPar->GetValue();
		ASSERT(var);
		DataObj* par = var->Clone();
		params.Add(par);
	}
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnPrepareAuxData()
{
	__super::OnPrepareAuxData();
	if (m_pOwner)
		m_pOwner->OnPrepareAuxData();
	if (m_pDocument)
		m_pDocument->OnPrepareAuxData(this);
}

//-----------------------------------------------------------------------------
void HotKeyLink::SelectColumns(SqlTable* pTable, SelectionType nQuerySelection/* = NO_SEL*/)
{
	pTable->SelectAll();
}

//-----------------------------------------------------------------------------
///<summary>
///Set reference object param value
///</summary>
//[TBWebMethod(securityhidden=true, thiscall_method=true)]
void HotKeyLink::SetParamValue(DataStr name, DataObj* value)
{
	for (int i = 0; i < m_XmlDescription.GetParameters().GetSize(); i++)
	{
		CDataObjDescription* pDescrPar = (CDataObjDescription*)m_XmlDescription.GetParamDescription(i);
		if (name.GetString().CompareNoCase(pDescrPar->GetName()) == 0)
		{
			if (DataType::IsCompatible(value->GetDataType(), pDescrPar->GetValue()->GetDataType()))
			{
				pDescrPar->GetValue()->Assign(*value);
				pDescrPar->GetValue()->SetValid(TRUE);

				return;
			}
			else
			{
				break;
			}
		}
	}
	ASSERT(FALSE);
	return;
}

//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetField(LPCTSTR sName) const
{
	SqlRecord* pRec = GetAttachedRecord();
	if (!pRec)
		return NULL;
	return pRec->GetDataObjFromColumnName(sName);
}

//-----------------------------------------------------------------------------
CString HotKeyLink::GetHKLDescription() const
{
	CString sName = GetDescriptionField();
	if (sName.IsEmpty())
	{
		return L"";
	}

	DataObj* pObj = GetField(sName);
	if (!pObj)
	{
		return L"";
	}

	if (!pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		return L"";
	}

	return *(DataStr*)pObj;
}

//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetDescriptionDataObj()
{
	CString sFieldName = GetDescriptionField();
	if (sFieldName.IsEmpty())
		return NULL;

	int nPos = sFieldName.FindOneOf(_T("."));
	if (nPos > 0)
		sFieldName = sFieldName.Mid(nPos + 1);

	return m_pRecord->GetDataObjFromColumnName(sFieldName);
}

//-----------------------------------------------------------------------------
CString HotKeyLink::GetDescriptionField() const
{
	if (!m_XmlDescription.GetDbFieldDescription().IsEmpty())
		return m_XmlDescription.GetDbFieldDescription();

	DataObj* pStrFields = AfxGetSettingValue(snsTbGenlib, szFormsSection, szHotlinkComboDefaultFields, DataStr(), szTbDefaultSettingFileName);

	// non ho nemmeno i defaults, allora disabilito la funzione
	if (!pStrFields || pStrFields->Str().IsEmpty() || !m_pRecord)
		return _T("");

	CString sFields = pStrFields->Str();

	int nCurrPos = -1;
	int nNexSepPos = 0;
	CString sField;

	do
	{
		nNexSepPos = sFields.Find(szDbFieldSeparator, nCurrPos + 1);
		sField = sFields.Mid(nCurrPos + 1, nNexSepPos > 0 ? nNexSepPos - nCurrPos - 1 : sFields.GetLength());

		if (sField.CompareNoCase(szDbFieldId) == 0)
		{
			nCurrPos = nNexSepPos;
			continue;
		}

		if (m_pRecord->GetIndexFromColumnName(sField) >= 0)
			return sField;

		nCurrPos = nNexSepPos;
	} while (nCurrPos >= 0 && nCurrPos <= sFields.GetLength());

	return _T("");
}

// ritorna il dataobj che rappresenta la chiave per il combo
//-----------------------------------------------------------------------------
DataObj* HotKeyLink::GetKeyData(SqlRecord* pRec, int idxDbField)
{
	return pRec->GetDataObjAt(idxDbField);
}

//-----------------------------------------------------------------------------
void HotKeyLink::SetName(const CString& sName)
{
	m_sName = sName;
	m_sName.Replace(_T("m_pHKL"), _T(""));
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::IsValid()
{
	return m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ? ((CAbstractFormDoc*)m_pDocument)->DispatchOnHKLIsValid(this) : TRUE;
}

//-----------------------------------------------------------------------------
void HotKeyLink::OnExtendContextMenu(CMenu& menu)
{
	if (!m_arContextMenuSearches.GetSize())
		return;

	menu.AppendMenu(MF_SEPARATOR);
	UINT nID = ID_CTRL_HKLEXTMENU_START;
	for (int i = 0; i <= m_arContextMenuSearches.GetUpperBound(); i++)
		menu.AppendMenu(MF_STRING, nID + i + 1, m_arContextMenuSearches.GetAt(i));
	menu.AppendMenu(MF_SEPARATOR);
}

//-----------------------------------------------------------------------------
void HotKeyLink::DoContextMenuAction(UINT nCode)
{
	if (nCode < 0 || (int)nCode > m_arContextMenuSearches.GetSize())
		return;

	// devo far scatenare il refresh del dato
	DataObj* pDataObj = GetAttachedData();
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl)
		pOwnerCtrl->GetValue(*pDataObj);

	// avviso che sono il control
	SetRunningMode(GetRunningMode() | RADAR_FROM_CTRL);

	m_nCustomSearch = nCode;
	SearchOnLink(pDataObj, HotKeyLinkObj::CUSTOM_ACCESS);
	m_nCustomSearch = -1;
}

//-----------------------------------------------------------------------------	
GOOD_REC_FUNC HotKeyLink::GetOnGoodRecordFunPtr()
{
	return m_pOnGoodRecord;
}

//-----------------------------------------------------------------------------	
void HotKeyLink::SetOnGoodRecordFunPtr(GOOD_REC_FUNC funPtr)
{
	m_pOnGoodRecord = funPtr;
}

//-------------------------------------------------------------------------------
void HotKeyLink::DoDefineQuery(SelectionType nQuerySelection)
{
	if (m_bSetParamValueByName)
	{
		DataObjArray arParam;
		PrepareCustomize(arParam);
		Customize(arParam);
	}

	OnDefineQuery(nQuerySelection);

	//posso far intervenire il documento ed i suoi clientdoc nella modifica della query
	if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		((CAbstractFormDoc*)m_pDocument)->DispatchOnModifyHKLDefineQuery(this, m_pTable, nQuerySelection);

	// dovrebbe essere completamente scollegata dalla define
	LoadSymbolTable();
}


//TBRowSecurityLayer
//TODO forse si può spostare l'implementazione direttamente nella SqlTable
//-----------------------------------------------------------------------------
void HotKeyLink::SetProtectedState()
{
	if (!m_pTable)
		return;
	//if (!AfxisActivated("Extensions", "RowSecurity"))
	m_pCatalogEntry = m_pTable->m_pSqlConnection->GetCatalogEntry(m_pTable->GetRecord()->GetTableName());
}

//-----------------------------------------------------------------------------
BOOL HotKeyLink::IsProtected()
{
	return m_pCatalogEntry && m_pCatalogEntry->IsProtected();
}

//-------------------------------------------------------------------------------
// allow the developer to disable query restrictions based on RowSecurityLayer
void HotKeyLink::SetSkipRowSecurity(BOOL bSet)
{
	m_bSkipRowSecurity = bSet;
}
//-------------------------------------------------------------------------------
BOOL HotKeyLink::IsSkipRowSecurity()
{
	return m_bSkipRowSecurity;
}

//-------------------------------------------------------------------------------
void HotKeyLink::SetRowSecurityWorker(DataLng* pRSWorkerID)
{
	m_pRSWorkerID = pRSWorkerID;
}

//-------------------------------------------------------------------------------
DataLng* HotKeyLink::GetRowSecurityWorker()
{
	return m_pRSWorkerID;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void HotKeyLink::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	dc << _T("\nHotKeyLink = ") << this->GetRuntimeClass()->m_lpszClassName;

}

void HotKeyLink::AssertValid() const
{
	HotKeyLinkObj::AssertValid();
}
#endif //_DEBUG

//=============================================================================
//					Class SimulatedHotKeyLink implementation
//=============================================================================
IMPLEMENT_DYNAMIC(SimulatedHotKeyLink, HotKeyLink)

//-----------------------------------------------------------------------------
SimulatedHotKeyLink::SimulatedHotKeyLink()
{
	EnableAddOnFly(FALSE);
	EnableHotLink(FALSE);
	EnableSearchOnLink(FALSE);
	EnableAutoFind(FALSE);
}

//-----------------------------------------------------------------------------
void SimulatedHotKeyLink::AttachDocument(CBaseDocument* pDocument)
{
	HotKeyLinkObj::AttachDocument(pDocument);
	InitNamespace();
	GetInfoOsl();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void SimulatedHotKeyLink::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nSimulatedHotKeyLink:");
}

void SimulatedHotKeyLink::AssertValid() const
{
	HotKeyLink::AssertValid();
}
#endif//_DEBUG

//////////////////////////////////////////////////////////////////////////////
//             					SimHKLUser
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SimHKLUser, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int SimHKLUser::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	CString strUser;

	for (int i = 0; i < AfxGetLoginInfos()->m_CompanyUsers.GetSize(); i++)
	{
		strUser = AfxGetLoginInfos()->m_CompanyUsers.GetAt(i);

		pDataObj = new DataStr(strUser);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(strUser);
	}
	return 1; //tutto ok
}

//////////////////////////////////////////////////////////////////////////////
//             					SimHKLRole
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SimHKLRole, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int SimHKLRole::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	CString strRole;

	for (int i = 0; i < AfxGetLoginInfos()->m_CompanyRoles.GetSize(); i++)
	{
		strRole = AfxGetLoginInfos()->m_CompanyRoles.GetAt(i);

		pDataObj = new DataStr(strRole);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(strRole);
	}
	return 1; //tutto ok
}

//////////////////////////////////////////////////////////////////////////////
//             					SimHKLUserRole
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SimHKLCurrentUserRoles, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int SimHKLCurrentUserRoles::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	CString strRole;

	for (int i = 0; i < AfxGetLoginInfos()->m_UserRoles.GetSize(); i++)
	{
		strRole = AfxGetLoginInfos()->m_UserRoles.GetAt(i);

		pDataObj = new DataStr(strRole);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(strRole);
	}
	return 1; //tutto ok
}

//////////////////////////////////////////////////////////////////////////////
//             					SimHKLCultureUI
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SimHKLCultureUI, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int SimHKLCultureUI::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	CString str;

	CStringArray ar, arUICultureDescriptions;
	LoadInstalledLanguages(ar, arUICultureDescriptions);

	for (int i = 0; i < ar.GetSize(); i++)
	{
		pDataObj = new DataStr(ar.GetAt(i));
		pKeyData.Add(pDataObj);
		arDescriptions.Add(arUICultureDescriptions.GetAt(i));
	}
	return 1; //tutto ok
}

//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLBarCodes
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SimHKLBarCode, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int SimHKLBarCode::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	CString str;

	for (int i = 0; i < CBarCodeTypes::BARCODE_TYPES_NUM; i++)
	{
		str = CBarCodeTypes::s_arBarCodeTypes[i].m_sName;

		pDataObj = new DataStr(str);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(str);
	}
	return 1; //tutto ok
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLBehaviours
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLBehaviours, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int HKLBehaviours::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;

	for (int i = 0; i < AfxGetBehavioursRegistry()->GetBehavioursCount(); i++)
	{
		CBehavioursRegistryService* pService = AfxGetBehavioursRegistry()->GetBehaviour(i);

		pDataObj = new DataStr(pService->GetNamespace());
		pKeyData.Add(pDataObj);
		CTBNamespace aNs(pService->GetNamespace());
		CString sTitle = AfxLoadXMLString
		(
			pService->GetTitle(),
			GetNameWithExtension(AfxGetPathFinder()->GetBehaviourObjectsFullName(aNs, CPathFinder::STANDARD)),
			AfxGetDictionaryPathFromNamespace(aNs, TRUE)
		);

		arDescriptions.Add(sTitle);
	}
	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
BOOL HKLBehaviours::SearchComboKeyDescription(const DataObj* pKey, CString& sDescription)
{
	for (int i = 0; i < AfxGetBehavioursRegistry()->GetBehavioursCount(); i++)
	{
		CBehavioursRegistryService* pService = AfxGetBehavioursRegistry()->GetBehaviour(i);

		if (pKey->IsEqual(DataStr(pService->GetNamespace())))
		{
			CString sTitle = AfxLoadXMLString
			(
				pService->GetTitle(),
				GetNameWithExtension(AfxGetPathFinder()->GetBehaviourObjectsFullName(pService->GetNamespace(), CPathFinder::STANDARD)),
				AfxGetDictionaryPathFromNamespace(pService->GetNamespace(), TRUE)
			);
			sDescription = sTitle;
			return TRUE;
		}
	}
	return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLEntities
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLEntities, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
HKLEntities::HKLEntities()
	:
	m_pBehaviourArray(NULL)
{
	m_pBehaviourArray = new CTBNamespaceArray();
}

//-----------------------------------------------------------------------------
HKLEntities::~HKLEntities()
{
	SAFE_DELETE(m_pBehaviourArray);
}

//-----------------------------------------------------------------------------
void HKLEntities::AddFilterByBehaviour(const CString& aValue)
{
	CTBNamespace aElem;
	aElem.AutoCompleteNamespace(CTBNamespace::BEHAVIOUR, aValue, aElem);
	m_pBehaviourArray->Add(new CTBNamespace(aElem));
}

//-----------------------------------------------------------------------------
void HKLEntities::BindParam(DataObj* pObj, int /*n = -1*/)
{
	ASSERT_VALID(pObj);
	ASSERT_KINDOF(DataStr, pObj);
	CString sBehaviour = pObj->Str();

	AddFilterByBehaviour(sBehaviour);
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl && pOwnerCtrl->GetCtrlCWnd())
	{
		if (pOwnerCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		{
			((CParsedCombo*)(pOwnerCtrl->GetCtrlCWnd()))->FillListBox();
		}
	}
}

//-----------------------------------------------------------------------------
int HKLEntities::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;

	for (int i = 0; i < AfxGetBehavioursRegistry()->GetEntitiesCount(); i++)
	{
		CBehavioursRegistryEntity* pEntity = AfxGetBehavioursRegistry()->GetEntity(i);

		if (pEntity->GetService().IsEmpty())
			continue;

		CBehavioursRegistryService* pService = AfxGetTbCmdManager()->GetBehaviourService(pEntity->GetNamespace());

		CTBNamespace behaviour;

		for (int r = 0; r < m_pBehaviourArray->GetCount(); r++)
		{
			behaviour = *m_pBehaviourArray->GetAt(r);

			if (behaviour.IsEmpty() || (pService && pService->GetNamespace().CompareNoCase(behaviour.ToString()) == 0))
			{
				pDataObj = new DataStr(pEntity->GetNamespace());
				pKeyData.Add(pDataObj);
				CTBNamespace aNs(pEntity->GetNamespace());
				CString sTitle = AfxLoadXMLString
				(
					pEntity->GetTitle(),
					GetNameWithExtension(AfxGetPathFinder()->GetBehaviourObjectsFullName(aNs, CPathFinder::STANDARD)),
					AfxGetDictionaryPathFromNamespace(aNs, TRUE)
				);

				arDescriptions.Add(sTitle);
			}
		}
	}
	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
BOOL HKLEntities::SearchComboKeyDescription(const DataObj* pKey, CString& sDescription)
{
	for (int i = 0; i < AfxGetBehavioursRegistry()->GetEntitiesCount(); i++)
	{
		CBehavioursRegistryEntity* pEntity = AfxGetBehavioursRegistry()->GetEntity(i);

		if (pEntity->GetService().IsEmpty())
			continue;

		CBehavioursRegistryService* pService = AfxGetTbCmdManager()->GetBehaviourService(pEntity->GetNamespace());

		CTBNamespace behaviour;

		for (int r = 0; r < m_pBehaviourArray->GetCount(); r++)
		{
			behaviour = *m_pBehaviourArray->GetAt(r);

			if (behaviour.IsEmpty() || (pService && pService->GetNamespace().CompareNoCase(behaviour.ToString()) == 0))
			{
				if (pKey->IsEqual(DataStr(pEntity->GetNamespace())))
				{
					CTBNamespace aNs(pEntity->GetNamespace());
					CString sTitle = AfxLoadXMLString
					(
						pEntity->GetTitle(),
						GetNameWithExtension(AfxGetPathFinder()->GetBehaviourObjectsFullName(aNs, CPathFinder::STANDARD)),
						AfxGetDictionaryPathFromNamespace(aNs, TRUE)
					);

					sDescription = sTitle;
					return TRUE;
				}
			}
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL HKLEntities::Customize(const DataObjArray& params)
{
	if (params.GetSize() > m_XmlDescription.GetParameters().GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (params.GetSize() == 0)
		return TRUE;

	DataObj* par = params.GetAt(0);
	if (!par->IsKindOf(RUNTIME_CLASS(DataStr)))
		return FALSE;

	AddFilterByBehaviour(par->Str());

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLTables
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLTables, SimulatedHotKeyLink)

HKLTables::HKLTables()
{
	MustExistData(TRUE);
}
//------------------------------------------------------------------------------
void HKLTables::SetPrefixName(const CString& sPrefix)
{
	m_sPrefixName = sPrefix;
}

//-----------------------------------------------------------------------------
int HKLTables::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	SqlCatalogConstPtr pCatalog = AfxGetDefaultSqlConnection()->GetCatalog();

	int nPrefix = m_sPrefixName.GetLength();

	POSITION	pos;
	CString		key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = pCatalog->GetStartPosition(); pos != NULL;)
	{
		pCatalog->GetNextAssoc(pos, key, (CObject*&)pCatalogEntry);

		if (pCatalogEntry)
		{
			CString sName(pCatalogEntry->m_strTableName);

			if (nPrefix && _tcsnicmp((LPCTSTR)sName, (LPCTSTR)m_sPrefixName, nPrefix))
				continue;

			DataStr* pDataObj = new DataStr(sName);
			pKeyData.Add(pDataObj);
			arDescriptions.Add(sName);
		}
	}
	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
void HKLTables::BindParam(DataObj* pObj, int /*n = -1*/)
{
	ASSERT_VALID(pObj);
	ASSERT_KINDOF(DataStr, pObj);
	CString sPrefix = pObj->Str();

	SetPrefixName(sPrefix);
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl && pOwnerCtrl->GetCtrlCWnd())
	{
		if (pOwnerCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		{
			((CParsedCombo*)(pOwnerCtrl->GetCtrlCWnd()))->FillListBox();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL HKLTables::Customize(const DataObjArray& params)
{
	if (params.GetSize() > m_XmlDescription.GetParameters().GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (params.GetSize() == 0)
		return TRUE;

	DataObj* par = params.GetAt(0);
	if (!par->IsKindOf(RUNTIME_CLASS(DataStr)))
		return FALSE;

	SetPrefixName(par->Str());

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLTableColumns
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLTableColumns, SimulatedHotKeyLink)

HKLTableColumns::HKLTableColumns()
{
	MustExistData(TRUE);
}

//------------------------------------------------------------------------------
void HKLTableColumns::SetTableName(const CString& sTable)
{
	m_sTableName = sTable;
}

//------------------------------------------------------------------------------
BOOL HKLTableColumns::SetTableNameFromDocNs(const CString& sDocNs)
{
	m_sTableName.Empty();

	if (sDocNs.IsEmpty())
		return FALSE;

	CTBNamespace ns(CTBNamespace::DOCUMENT, sDocNs);
	if (!ns.IsValid())
		return FALSE;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(ns);
	if (!pAddOnMod)
		return FALSE;

	const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(ns);
	if (!pDocInfo)
		return FALSE;

	CLocalizableXMLDocument aXMLDBTDoc(ns, AfxGetPathFinder());
	aXMLDBTDoc.EnableMsgMode(FALSE);

	if (aXMLDBTDoc.LoadXMLFile(AfxGetPathFinder()->GetDocumentDbtsFullName(ns)))
	{
		CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(_T("Master")); //XML_DBT_TYPE_MASTER_TAG
		if (pDBTMasterNode == NULL)
			return FALSE;

		CXMLNode* pChildNode = pDBTMasterNode->GetChildByName(_T("Table"));//XML_TABLE_TAG
		if (pChildNode == NULL)
			return FALSE;

		pChildNode->GetText(m_sTableName);
		if (!m_sTableName.IsEmpty())
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
int HKLTableColumns::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	if (m_sTableName.IsEmpty())
		return FALSE;

	const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(m_sTableName);
	if (pEntry == NULL)
		return FALSE;
	SqlRecord* pRec = pEntry->CreateRecord();
	if (pRec == NULL)
		return FALSE;

	DataStr* pDataObj = NULL;

	for (int i = 0; i < pRec->GetSizeEx(); i++)
	{
		SqlRecordItem* pField = pRec->GetAt(i);

		CString sName = pField->GetColumnName();

		pDataObj = new DataStr(sName);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(sName);
	}
	delete pRec;
	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
void HKLTableColumns::BindParam(DataObj* pObj, int /*n = -1*/)
{
	ASSERT_VALID(pObj);
	ASSERT_KINDOF(DataStr, pObj);
	CString sTable = pObj->Str();

	if (sTable.Find('.') > 0)
		SetTableNameFromDocNs(sTable);
	else
		SetTableName(sTable);
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl && pOwnerCtrl->GetCtrlCWnd())
	{
		if (pOwnerCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		{
			((CParsedCombo*)(pOwnerCtrl->GetCtrlCWnd()))->FillListBox();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL HKLTableColumns::Customize(const DataObjArray& params)
{
	if (params.GetSize() > m_XmlDescription.GetParameters().GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (params.GetSize() == 0)
		return FALSE;

	DataObj* par = params.GetAt(0);
	if (!par->IsKindOf(RUNTIME_CLASS(DataStr)))
		return FALSE;

	CString sTable = par->Str();

	if (sTable.Find('.') > 0)
		SetTableNameFromDocNs(sTable);
	else
		SetTableName(sTable);

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLApplications
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLApplications, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int HKLApplications::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	DataStr* pDataObj = NULL;
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!AfxIsAppActivated(pAddOnApp->m_strAddOnAppName))
			continue;

		pDataObj = new DataStr(pAddOnApp->m_strAddOnAppName);
		pKeyData.Add(pDataObj);
		arDescriptions.Add(pAddOnApp->GetTitle());
	}
	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
BOOL HKLApplications::SearchComboKeyDescription(const DataObj* pKey, CString& sDescription)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(pKey->Str());
	if (!pAddOnApp)
		sDescription = pKey->Str();
	else
		sDescription = pAddOnApp->GetTitle();
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//             					HKLModules
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLModules, SimulatedHotKeyLink)

//-----------------------------------------------------------------------------
int HKLModules::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_sApplication);
	if (!pAddOnApp)
		return FALSE;

	for (int i = 0; i <= pAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnModule = pAddOnApp->m_pAddOnModules->GetAt(i);
		if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
			continue;

		DataStr* pDataObj = new DataStr(pAddOnModule->GetModuleName());
		pKeyData.Add(pDataObj);
		arDescriptions.Add(pAddOnModule->GetModuleTitle());
	}

	return 1; //tutto ok
}

//-----------------------------------------------------------------------------
BOOL HKLModules::SearchComboKeyDescription(const DataObj* pKey, CString& sDescription)
{
	sDescription = pKey->Str();

	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_sApplication);
	if (pAddOnApp)
	{
		for (int i = 0; i <= pAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
		{
			AddOnModule* pAddOnModule = pAddOnApp->m_pAddOnModules->GetAt(i);
			if (pAddOnModule->GetModuleName().CompareNoCase(pKey->Str()) == 0)
			{
				sDescription = pAddOnModule->GetModuleTitle();
				break;
			}
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void HKLModules::SetApplication(const CString& sApplication)
{
	m_sApplication = sApplication;
}

//-----------------------------------------------------------------------------
void HKLModules::BindParam(DataObj* pObj, int /*n = -1*/)
{
	ASSERT_VALID(pObj);
	ASSERT_KINDOF(DataStr, pObj);
	CString sApplication = pObj->Str();

	SetApplication(sApplication);

	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	if (pOwnerCtrl && pOwnerCtrl->GetCtrlCWnd())
	{
		if (pOwnerCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		{
			((CParsedCombo*)(pOwnerCtrl->GetCtrlCWnd()))->FillListBox();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL HKLModules::Customize(const DataObjArray& params)
{
	if (params.GetSize() > m_XmlDescription.GetParameters().GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (params.GetSize() == 0)
		return FALSE;

	DataObj* par = params.GetAt(0);
	if (!par->IsKindOf(RUNTIME_CLASS(DataStr)))
		return FALSE;

	CString sApplication = par->Str();

	SetApplication(sApplication);

	return TRUE;
}
//////////////////////////////////////////////////////////////////////////////
//             					XmlHotKeyLink
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(XmlHotKeyLink, SimulatedHotKeyLink)

XmlHotKeyLink::XmlHotKeyLink()
	:
	m_pDfi(NULL),
	m_bUseProductLanguage(FALSE),
	m_bInitialized(FALSE)
{
	this->m_bEnableFillListBox = TRUE;
}

XmlHotKeyLink::XmlHotKeyLink(LPCTSTR sNs)
	:
	m_pDfi(NULL),
	m_bUseProductLanguage(FALSE),
	m_bInitialized(FALSE)
{
	this->m_bEnableFillListBox = TRUE;

	SetHotKeyLinkNamespace(sNs);
}

//-----------------------------------------------------------------------------
void XmlHotKeyLink::SetDFNamespace(LPCTSTR lpcszNamespace)
{
	m_pDfi = AfxGetTbCmdManager()->GetDataFileInfo(lpcszNamespace, FALSE, m_bUseProductLanguage);
	if (m_pDfi)
		this->m_bLikeOnDropDownEnabled = m_pDfi->m_bFilterLike;
}

//-----------------------------------------------------------------------------
BOOL XmlHotKeyLink::SetHotKeyLinkNamespace(LPCTSTR sNs) //sostituisce registrazione nell'interface di library, effettua bind con ReferenceObject xml
{
	CTBNamespace ns(CTBNamespace::HOTLINK, sNs);
	if (!ns.IsValid())
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
		return FALSE;
	}

	AddOnModule* pAddOnMod = AfxGetAddOnModule(ns);
	if (!pAddOnMod)
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
		return FALSE;
	}

	AddOnLibrary* pLib = AfxGetAddOnLibrary(ns);
	if (!pLib)
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
		return FALSE;
	}
	pLib->m_pAddOn->DeclareFunction(new FunctionDataInterface(ns, RUNTIME_CLASS(DynamicHotKeyLink), NULL));

	CFunctionDescription* pXmlDescri = pAddOnMod->m_XmlDescription.GetParamObjectInfo(ns);
	if (!pXmlDescri)
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid xml description."), sNs));
		return FALSE;
	}

	ASSERT(pXmlDescri->IsKindOf(RUNTIME_CLASS(CHotlinkDescription)));
	m_XmlDescription = *((CHotlinkDescription*)pXmlDescri);

	SetDFNamespace(m_XmlDescription.GetDatafile());

	m_bInitialized = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
void XmlHotKeyLink::AttachDocument(CBaseDocument* pDocument)
{
	HotKeyLinkObj::AttachDocument(pDocument);

	XmlHotKeyLink::InitNamespace();
	GetInfoOsl();
}

//-----------------------------------------------------------------------------
void XmlHotKeyLink::InitNamespace()
{
	CheckXmlDescription();
}

//-----------------------------------------------------------------------------
int XmlHotKeyLink::SearchComboQueryData(const int&, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	if (!m_pDfi || m_pDfi->m_arElements.IsEmpty())
		return 0;

	CString sPrefixBeforeDrop;
	if (m_pDfi->m_bFilterLike && GetOwnerCtrl())
		GetOwnerCtrl()->GetValue(sPrefixBeforeDrop);

	CString str;
	BOOL bOk = TRUE;
	int idxCount = 0;

	for (long i = 0; i <= m_pDfi->m_arElements.GetUpperBound(); i++)
	{
		CString sVal = m_pDfi->GetValue(i);

		if (!sPrefixBeforeDrop.IsEmpty())
		{
			if (::FindNoCase(sVal, sPrefixBeforeDrop) != 0)
				continue;
		}

		pKeyData.Add(new DataStr(sVal));
		arDescriptions.Add(m_pDfi->GetDescription(i));
		idxCount++;
	}
	return 1; //tutto ok
}

//=============================================================================
//					Class DynamicHotKeyLink implementation
//=============================================================================
IMPLEMENT_DYNCREATE(DynamicHotKeyLink, HotKeyLink)

//-----------------------------------------------------------------------------
DynamicHotKeyLink::DynamicHotKeyLink()
	:
	m_bInitialized(FALSE),
	m_pQuery(NULL),
	m_pAsk(NULL)
{
	SAFE_DELETE(m_pTable);	//ereditato della classe HotKeyLink
}

//-----------------------------------------------------------------------------
DynamicHotKeyLink::DynamicHotKeyLink(LPCTSTR sNs)
	:
	m_bInitialized(FALSE),
	m_pQuery(NULL),
	m_pAsk(NULL)
{
	SetHotKeyLinkNamespace(sNs);

	LoadCustomSelectionTypes();

	SAFE_DELETE(m_pTable);	//ereditato della classe HotKeyLink
}

//-----------------------------------------------------------------------------
DynamicHotKeyLink::~DynamicHotKeyLink()
{
	m_pTable = NULL;
	SAFE_DELETE(m_pAsk);
}

DynamicHotKeyLink::CAsk::~CAsk()
{
	SAFE_DELETE(m_pAskRule);
	SAFE_DELETE(m_pSymTable);
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::AttachDocument(CBaseDocument* pDocument)
{
	HotKeyLinkObj::AttachDocument(pDocument);

	DynamicHotKeyLink::InitNamespace();
	GetInfoOsl();
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::InitNamespace()
{
	CheckXmlDescription();
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::SetHotKeyLinkNamespace(LPCTSTR sNs) //sostituisce registrazione nell'interface di library, effettua bind con ReferenceObject xml
{
	CTBNamespace ns(CTBNamespace::HOTLINK, sNs);
	if (!ns.IsValid())
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
		return FALSE;
	}

	AddOnModule* pAddOnMod = AfxGetAddOnModule(ns);
	if (!pAddOnMod)
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
		return FALSE;
	}

	if (!ns.HasAFakeLibrary())
	{
		AddOnLibrary* pLib = AfxGetAddOnLibrary(ns);
		if (!pLib)
		{
			ASSERT(FALSE);
			TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid namespace."), sNs));
			return FALSE;
		}
		pLib->m_pAddOn->DeclareFunction(new FunctionDataInterface(ns, RUNTIME_CLASS(DynamicHotKeyLink), NULL));
	}
	CFunctionDescription* pXmlDescri = pAddOnMod->m_XmlDescription.GetParamObjectInfo(ns);
	if (!pXmlDescri)
	{
		ASSERT(FALSE);
		TRACE(cwsprintf(_TB("{0-%s} report-hotlink has invalid xml description."), sNs));
		return FALSE;
	}

	ASSERT(pXmlDescri->IsKindOf(RUNTIME_CLASS(CHotlinkDescription)));
	m_XmlDescription = *((CHotlinkDescription*)pXmlDescri);

	//-----
	m_pRecord = AfxCreateRecord(m_XmlDescription.GetDbTable());
	ASSERT(m_pRecord);

	SetNamespace(ns);

	SetAddOnFlyNamespace(m_XmlDescription.GetCallLink());
	EnableAddOnFly(m_XmlDescription.IsAddOnFlyEnabled());
	MustExistData(m_XmlDescription.IsMustExistData());
	EnableSearchOnLink(m_XmlDescription.IsSearchOnLinkEnabled());

	m_pSymTable = new SymTable();
	LoadSymbolTable();

	//----
	m_bInitialized = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::LoadCustomSelectionTypes()
{
	CHotlinkDescription::CSelectionType* selType;
	for (int i = 0; i < m_XmlDescription.m_arSelectionTypes.GetCount(); i++)
	{
		selType = (CHotlinkDescription::CSelectionType*)m_XmlDescription.m_arSelectionTypes.GetAt(i);
		if (selType->m_eType == CHotlinkDescription::CUSTOM && selType->m_bVisible)
			m_arContextMenuSearches.Add(selType->m_sTitle);
	}
}

//-----------------------------------------------------------------------------
DataObj* DynamicHotKeyLink::GetDataObj() const
{
	//dalla descrizione ho il nome delle colonna dbfield del record corrente
	return GetDataObj(m_XmlDescription.GetDbField());
}

//-----------------------------------------------------------------------------
DataObj* DynamicHotKeyLink::GetDescriptionDataObj()
{
	//dalla descrizione ho il nome delle colonna dbfield del record corrente
	return GetDataObj(m_XmlDescription.GetDbFieldDescription());
}

//-----------------------------------------------------------------------------
DataObj* DynamicHotKeyLink::GetDataObj(CString sDBField) const
{
	SqlRecord* pRec = GetRecord();
	ASSERT_VALID(pRec);

	CStringArray ar;
	CStringArray_Split(ar, sDBField, L",");

	for (int i = 0; i < ar.GetSize(); i++)
	{
		CString sName = ar[i];

		int pos = sName.Find('.');
		if (pos > 0)
			sName = sName.Mid(pos + 1);

		DataObj* pData = pRec->GetDataObjFromColumnName(sName);
		if (!pData && pos > 0)
			pData = pRec->GetDataObjFromColumnName(ar[i]);

		if (pData)
			return pData;
	}
	TRACE2("DynamicHotKeyLink::GetDataObj: column (%s) shows in tag <DbField or DbFieldDescription> of ReferenceObject\n(%s) not exist in the table.\n",
		sDBField, m_XmlDescription.GetNamespace().ToString());
	return NULL;
}

//-----------------------------------------------------------------------------
SqlRecord* DynamicHotKeyLink::GetRecord() const
{
	ASSERT(m_pRecord);
	return m_pRecord;
}

SqlTable*	DynamicHotKeyLink::GetSqlTable()
{
	if (m_pQuery && m_pQuery->GetSqlTable())
		return m_pQuery->GetSqlTable();
	return m_pTable;
}

//----------------------------------------------------------------------------
void DynamicHotKeyLink::LoadSymbolTable()
{
	__super::LoadSymbolTable();
	//----
	DataObj* pObj = GetDataObj(); pObj->SetValid(TRUE);
	SymField* pField = new SymField
	(
		CHotlinkDescription::s_FilterValue_Name,
		pObj->GetDataType(),
		0,
		pObj,
		FALSE
	);
	m_pSymTable->Add(pField);

	pObj = &m_dsSelectionType; pObj->SetValid(TRUE);
	m_pSymTable->Add(
		new SymField
		(
			CHotlinkDescription::s_SelectionType_Name,
			pObj->GetDataType(),
			0,
			pObj,
			FALSE
		)
	);

	//------------------------------------------------------------
	CString sAskDialogs = this->m_XmlDescription.GetAskDialogs();
	if (!sAskDialogs.IsEmpty())
	{
		Parser lex(sAskDialogs);
		m_pAsk = new CAsk;

		m_pAsk->m_pSymTable = new WoormTable(WoormTable::ReportSymTable_EDITOR, NULL);
		m_pAsk->m_pSymTable->SetParent(m_pSymTable);

		BOOL bOk = m_pAsk->m_pSymTable->Parse(lex);
		if (!bOk)
		{
			ASSERT_TRACE2(FALSE,
				"Error on parsing ask dialog variables of dynamic Reference Object %s\n%s)\n",
				this->GetNamespace().ToString(), lex.GetError());
			TRACE(sAskDialogs + '\n');

			SAFE_DELETE(m_pAsk);
			return;
		}

		m_pAsk->m_pAskRule = new AskRuleData(*m_pAsk->m_pSymTable);
		bOk = m_pAsk->m_pAskRule->Parse(lex, NULL);
		if (!bOk)
		{
			ASSERT_TRACE2(FALSE,
				"Error on parsing ask dialogs of dynamic Reference Object %s\n%s\n",
				this->GetNamespace().ToString(), lex.GetError());
			TRACE(sAskDialogs + '\n');
			SAFE_DELETE(m_pAsk);
			return;
		}
	}
}

//----------------------------------------------------------------------------
void DynamicHotKeyLink::UpdateSymbolTable(SqlRecord* pRecord)
{
	__super::UpdateSymbolTable(pRecord);
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::InternalOnVoidEvent(LPCTSTR pszEventName)
{
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::OnValidateRadarSelection(SqlRecord* pRec)
{
	ValorizeSelectedRecord(pRec);

	return ExecuteEventIsValid();
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::IsValid()
{
	ValorizeSelectedRecord(m_pTable ? m_pTable->GetRecord() : m_pRecord);

	return ExecuteEventIsValid();
}
//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::ExecuteEventIsValid()
{
	CFunctionDescription* pE = m_XmlDescription.m_EventsInfo.GetFunctionInfo(_T("IsValid"));
	if (pE == NULL)
		return TRUE;

	CString sTbScript = pE->GetTBScript();
	if (sTbScript.IsEmpty())
		return TRUE;

	Parser lex(sTbScript);

	CFunctionDescription aFunction(*pE);

	TBScript* script = AfxGetTbCmdManager()->CreateTbScript(&aFunction, m_pSymTable);
	if (!script)
		return FALSE;

	if (!script->Parse(lex))
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on parsing script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
		delete script;
		return FALSE;
	}
	if (!script->Exec())
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on execute script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
		delete script;
		return FALSE;
	}

	DataObj* pRetValue = NULL;
	if (aFunction.GetReturnValueDataType() != DataType::Void)
		pRetValue = aFunction.GetReturnValue();

	BOOL bRet = pRetValue ? *(DataBool*)pRetValue : FALSE;
	delete script;
	return bRet;
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::OnCallLink()
{
	CFunctionDescription* pE = m_XmlDescription.m_EventsInfo.GetFunctionInfo(_T("OnCallLink"));
	if (pE == NULL)
		return;

	CString sTbScript = pE->GetTBScript();
	if (sTbScript.IsEmpty())
		return;

	CFunctionDescription aFunction(*pE);

	TBScript* script = AfxGetTbCmdManager()->CreateTbScript(&aFunction, m_pSymTable);
	if (!script)
		return;

	SymField* pField = script->GetSymTable()->GetField(_T("calledDocument"));
	if (!pField || pField->GetDataType() != DataType::Object)
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on parsing script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
		delete script;
		return;
	}
	DataLng dl((long)m_pCallLinkDoc); dl.SetAsHandle();
	pField->GetData()->Assign(dl); pField->GetData()->SetValid(TRUE);
	pField->AddMethods(RUNTIME_CLASS(CBaseDocument), AfxGetAddOnAppsTable()->GetMapWebClass());

	Parser lex(sTbScript);
	if (!script->Parse(lex))
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on parsing script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
		delete script;
		return;
	}
	if (!script->Exec())
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on execute script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
		delete script;
		return;
	}
	delete script;
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::OnRecordAvailable()
{
	CFunctionDescription* pE = m_XmlDescription.m_EventsInfo.GetFunctionInfo(_T("OnRecordAvailable"));
	if (pE == NULL)
		return;

	CString sTbScript = pE->GetTBScript();
	if (sTbScript.IsEmpty())
		return;

	Parser lex(sTbScript);

	CFunctionDescription aFunction(*pE);

	TBScript* script = AfxGetTbCmdManager()->CreateTbScript(&aFunction, m_pSymTable);
	if (!script)
		return;

	if (!script->Parse(lex))
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on parsing script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
	}
	else if (!script->Exec())
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on execute script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);
	}
	delete script;
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::Customize(const DataObjArray& params)
{
	if (params.GetSize() > m_XmlDescription.GetParameters().GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	for (int i = 0; i < params.GetSize(); i++)
	{
		DataObj* par = params.GetAt(i);

		CDataObjDescription* pDescrPar = (CDataObjDescription*)m_XmlDescription.GetParameters().GetAt(i);
		DataObj* var = pDescrPar->GetValue();
		ASSERT(var);

		if (DataType::IsCompatible(par->GetDataType(), var->GetDataType()))
		{
			var->Assign(*par);
		}
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}
	return TRUE;
}

//-------------------------------------------------------------------------------
void DynamicHotKeyLink::SetRecord(const SqlRecord* pRec)
{
	*m_pRecord = *pRec;
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::SelectColumns(SqlTable* pTable, SelectionType /*nQuerySelection = NO_SEL*/)
{
	//OK
}

//-------------------------------------------------------------------------------
void DynamicHotKeyLink::OnDefineQuery(SelectionType nQuerySelection)
{
	//OK
}

//-------------------------------------------------------------------------------
void DynamicHotKeyLink::OnPrepareQuery(DataObj* pDataObj, SelectionType nSelection)
{
	CHotlinkDescription::CSelectionMode* pMode = m_XmlDescription.GetSelectionMode((CHotlinkDescription::ESelectionType) nSelection, GetCustomSearch());
	if (!pMode)
	{
		if (nSelection == HotKeyLinkObj::COMBO_ACCESS)
		{
			//POSSIBILE TAPPULLO da eliminare
			pMode = m_XmlDescription.GetSelectionMode((CHotlinkDescription::ESelectionType) HotKeyLinkObj::DIRECT_ACCESS);
		}

		if (!pMode)
		{
			ASSERT(FALSE);
			return;
		}
	}

	switch (nSelection)
	{
		case HotKeyLinkObj::DIRECT_ACCESS:
		{
			m_dsSelectionType = CHotlinkDescription::s_SelectionType_Direct;
			break;
		}
		case HotKeyLinkObj::UPPER_BUTTON:
		{
			if (pDataObj)
			{
				ASSERT_VALID(pDataObj);
				DataStr* pDS = dynamic_cast<DataStr*>(pDataObj);
				if (pDS) pDS->Append(L'%');
			}
			m_dsSelectionType = CHotlinkDescription::s_SelectionType_Code;
			break;
		}
		case HotKeyLinkObj::LOWER_BUTTON:
		{
			ASSERT_VALID(pDataObj);
			DataStr* pDS = dynamic_cast<DataStr*>(pDataObj);
			if (pDS) pDS->Append(L'%');

			m_dsSelectionType = CHotlinkDescription::s_SelectionType_Description;
			break;
		}
		case HotKeyLinkObj::COMBO_ACCESS:
		{
			if (pDataObj)
			{
				ASSERT_VALID(pDataObj);
				DataStr* pDS = dynamic_cast<DataStr*>(pDataObj);
				if (pDS) pDS->Append(L'%');

			}
			m_dsSelectionType = CHotlinkDescription::s_SelectionType_Combo;
			break;
		}
		case HotKeyLinkObj::CUSTOM_ACCESS:
		{
			if (pDataObj)
			{
				ASSERT_VALID(pDataObj);
				DataStr* pDS = dynamic_cast<DataStr*>(pDataObj);
				if (pDS) pDS->Append(L'%');

			}
			m_dsSelectionType = CHotlinkDescription::s_SelectionType_Custom;
			break;
		}
		default:
		{
			ASSERT(FALSE);
			m_dsSelectionType = _T("");
		}
	}
	if(m_dsSelectionType != _T(""))
	{ 
		SymField* pField = m_pSymTable->GetField(CHotlinkDescription::s_FilterValue_Name);
		ASSERT_VALID(pField);
		pField->GetData()->Assign(*pDataObj);
	}

	if (pMode->m_eMode == CHotlinkDescription::QUERY)
	{
		if (pMode->m_pQuery == NULL)
		{
			pMode->m_pQuery = new QueryObject(m_pSymTable, m_pDocument ? m_pDocument->GetReadOnlySqlSession() : AfxGetDefaultSqlSession());

			if (!pMode->m_pQuery->Define(DataStr(pMode->m_sBody)))
			{
				ASSERT(FALSE);
				return;
			}
		}

		m_pQuery = (QueryObject*)(pMode->m_pQuery);
	}
	else if (pMode->m_eMode == CHotlinkDescription::SCRIPT)
	{
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::ValorizeSelectedRecord(SqlRecord* pRec)
{
	//Il radar ha valorizzato m_pTable->GetRecord(), di tipo SqlRecordDynamic
	// occorre valorizzare l'intero record m_pRecord
	BOOL bAllColumnsValorized = TRUE;
	BOOL bThereIsKeys = TRUE;
	int sz = m_pRecord->GetSizeEx();
	for (int i = 0; i < sz; i++)
	{
		CString sQName;
		SqlRecordItem* pItemDst = m_pRecord->GetAt(i);
		TRACE(_T("%s\n"), pItemDst->GetColumnName());

		DataObj* pSrc = pRec->GetDataObjFromColumnName(pItemDst->GetColumnName());
		if (pSrc == NULL)
		{
			sQName = m_pRecord->GetTableName() + '.' + pItemDst->GetColumnName();

			if (sQName.CompareNoCase(pItemDst->GetColumnName()))
				pSrc = pRec->GetDataObjFromColumnName(sQName);

			if (pSrc == NULL)
			{
				SymField* pF = m_pSymTable->GetField(pItemDst->GetColumnName());
				if (pF == NULL && sQName.CompareNoCase(pItemDst->GetColumnName()))
					pF = m_pSymTable->GetField(sQName);
				if (pF)
					pSrc = pF->GetData();
			}
		}

		if (pSrc == NULL)
		{
			TRACE(_T("missing %s\n"), pItemDst->GetColumnName());
			pItemDst->GetDataObj()->SetValid(FALSE);
			bAllColumnsValorized = FALSE;
			if (pItemDst->IsSpecial())
				bThereIsKeys = FALSE;
			continue;
		}

		TRACE(_T("%s=%s\n"), pItemDst->GetColumnName(), pSrc->Str());
		pItemDst->GetDataObj()->Assign(*pSrc);
		pItemDst->GetDataObj()->SetValid(TRUE);
	}
	//----
	if (!bAllColumnsValorized && bThereIsKeys && m_XmlDescription.IsLoadFullRecord())
	{
		SqlTable aTable(m_pRecord, m_pDocument ? m_pDocument->GetReadOnlySqlSession() : AfxGetDefaultSqlSession());
		aTable.SelectAll();
		for (int i = 0; i < m_pRecord->GetNumberSpecialColumns(); i++)
		{
			SqlRecordItem* pItem = m_pRecord->GetSpecialColumn(i);
			ASSERT(pItem->GetDataObj()->IsValid());

			aTable.AddFilterColumn(pItem->GetColumnName());
			CString par; par.Format(_T("p%d"), i);
			aTable.AddParam(par, *pItem->GetDataObj());
			aTable.SetParamValue(par, *pItem->GetDataObj());
		}
		aTable.Open();
		aTable.Query();
		ASSERT(!aTable.IsEOF());
		aTable.Close();
	}
	//----
	UpdateSymbolTable(m_pRecord);
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::OnRadarRecordAvailable()
{
	__super::OnRadarRecordAvailable();
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::OnAssignSelectedValue(DataObj* pCtrlData, DataObj* pHKLData)
{
	CFunctionDescription* pE = m_XmlDescription.m_EventsInfo.GetFunctionInfo(_T("OnAssignSelectedValue"));
	if (pE == NULL)
	{
		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		return;
	}

	CString sTbScript = pE->GetTBScript();
	if (sTbScript.IsEmpty())
	{
		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		return;
	}

	Parser lex(sTbScript);

	CFunctionDescription aFunction(*pE);

	TBScript* script = AfxGetTbCmdManager()->CreateTbScript(&aFunction, m_pSymTable);
	if (!script)
	{
		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		delete script;
		return;
	}

	SymField* pFCtrlVal = script->GetSymTable()->GetField(_T("p_CtrlValue"));
	if (!pFCtrlVal)
	{
		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		delete script;
		return;
	}

	SymField* pFHklVal = script->GetSymTable()->GetField(_T("p_HklValue"));
	if (!pFCtrlVal)
	{
		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		delete script;
		return;
	}

	pFCtrlVal->AssignData(*pCtrlData);
	pFHklVal->AssignData(*pHKLData);

	if (!script->Parse(lex))
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on parsing script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);

		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		delete script;
		return;
	}
	if (!script->Exec())
	{
		AfxGetDiagnostic()->Add(
			cwsprintf(_TB("{0-%s}: error on execute script event \"{1-%s}\""), GetNamespace().ToString(), aFunction.GetName()),
			CDiagnostic::Error);

		__super::OnAssignSelectedValue(pCtrlData, pHKLData);
		delete script;
		return;
	}
	pCtrlData->Assign(*pFCtrlVal->GetData());
	delete script;
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::SearchOnLink(DataObj* pData, SelectionType nQuerySelection)
{
	// Se e` stata chiamata dalle SearchOnLinkUpper/SearchOnLinkLower
	// la GetUpdatedDataObj() ha gia` messo il flag di running da control,
	// solo in questo caso si accetta un RunningMode != 0
	//
	if ((GetRunningMode() & ~RADAR_FROM_CTRL) != 0)
		return FALSE;

	// NON deve esserci un radar attaccato
	ASSERT(m_pRadarDoc == NULL);

	GetInfoOsl();

	if (!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE))
	{
		EnableCtrl(FALSE);
		AfxMessageBox(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString()));
		EnableCtrl(TRUE);
		SetRunningMode(GetRunningMode() & ~RADAR_FROM_CTRL);
		return FALSE;
	}

	if (m_pAsk)
	{
		AskDialogInputMng aAskDialogInputMng(m_pAsk->m_pAskRule, m_pAsk->m_pSymTable, NULL);
		if (!aAskDialogInputMng.ExecAskRules(NULL, FALSE))
		{
			SetRunningMode(GetRunningMode() & ~RADAR_FROM_CTRL);
			return FALSE;
		}

		//----
		m_pSymTable->Append(*m_pAsk->m_pSymTable);
	}

	// Si segnala che l'hotlink e` running : DEVE essere fatta PRIMA di fare
	// la chiamata al Radar poiche` la apertura della nuova finestra fa perdere
	// il fuoco al control e quindi puo` essere utile conoscere che si e` in questa
	// situazione (vedi per esempio BodyEdit)
	//
	SetRunningMode(GetRunningMode() | RADAR_MODE);
	// valorizza i parametri per la query
	m_pRecord->Init();

	// nello stato iniziale il dato non e` disponibile
	m_bRecordAvailable = FALSE;

	OnPrepareQuery(pData, nQuerySelection);

	if (m_pQuery)
	{
		m_pQuery->SetCursorType(E_KEYSET_CURSOR, FALSE, FALSE);

		BOOL bOk = m_pQuery->Open();
		if (!bOk)
		{
			ASSERT(FALSE);
			SetRunningMode(GetRunningMode() & ~(RADAR_FROM_CTRL | RADAR_MODE));
			return FALSE;
		}

		m_pTable = m_pQuery->GetSqlTable();
		m_pTable->GetRecord()->SetTableName(this->m_XmlDescription.GetDbTable());
	}
	m_pRadarDoc = AfxCreateTBRadar(this, m_pTable, m_pTable ? m_pTable->GetRecord()/*m_pRecord*/ : m_pRecord, nQuerySelection);
	if (!m_pRadarDoc)
	{
		SetRunningMode(GetRunningMode() & ~(RADAR_FROM_CTRL | RADAR_MODE));
		return FALSE;
	}
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	// la disabilitazione invece DEVE essere fatta DOPO (vedi BodyEdit: il
	// comando SHOW_HIDE non viene gestito se il control e` disabilitato)
	if (pOwnerCtrl && (GetRunningMode() & RADAR_FROM_CTRL) == RADAR_FROM_CTRL)
		EnableCtrl(FALSE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DynamicHotKeyLink::ExistData(DataObj* pData)
{
	CHotlinkDescription::CSelectionMode* pMode = m_XmlDescription.GetSelectionMode((CHotlinkDescription::ESelectionType) HotKeyLinkObj::DIRECT_ACCESS);
	//TODO RICCARDO - rivedere logica
	if (!pMode || pMode->m_sName.CompareNoCase(L"Default") == 0)
	{
		return TRUE;
	}
	return __super::ExistData(pData);
}

//-----------------------------------------------------------------------------
HotKeyLink::FindResult DynamicHotKeyLink::FindRecord
(
	DataObj*	pDataObj,
	BOOL		bCallLink,		/* = FALSE */
	BOOL		bFromControl	/* = FALSE */,
	BOOL		bAllowRunningModeForInternalUse/* = FALSE*/
)
{
	ASSERT(pDataObj);

	if (!m_pRecord)
	{
		TRACE("HotKeyLink::FindRecord: %s isn't linked to a SqlRecord!\n", GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		m_PrevResult = EMPTY;
		return m_PrevResult;
	}

	// Paramento per evitare la ricorsivita' della FindRecord durante Radarate o CallLink
	if (!bAllowRunningModeForInternalUse && GetRunningMode() != 0)
	{
		TRACE("HotKeyLink::FindRecord: %s is in active status!\n", GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		m_PrevResult = EMPTY;
		return m_PrevResult;
	}

	FindResult eResult = NONE;
	TRY
	{
		// Permette di intervenire sul DataObj associato al control un attimo prima
		// di fare la FindRecord, permettendo cosi' eventuali accessi indiretti o altre
		// possibilita di modifica del DataObj
		//
		OnPreprocessDataObj(pDataObj, bFromControl);

	// In caso di dato di find senza valore non effettua la find
	if (pDataObj->GetDataType() == DATA_STR_TYPE && pDataObj->IsEmpty())
	{
		m_pRecord->Init();
		m_PrevResult = EMPTY;
		return m_PrevResult;
	}

	OnPrepareQuery(pDataObj);

	if (!m_pQuery)
	{
		m_pRecord->Init();
		m_PrevResult = EMPTY;
		return NOT_VALID;
	}

	// esegue la query scelta
	BOOL bOk = m_pQuery->Open();
	if (bOk)
		bOk = m_pQuery->Read();

	m_pTable = m_pQuery->GetSqlTable();

	// Effettua il controllo di query
	m_PrevResult = eResult = OnFindRecord
		(
			!bOk /*m_pTable->IsEmpty()*/ ? NOT_FOUND : FOUND,
			pDataObj,
			bCallLink,
			bFromControl
		);

	m_pQuery->Close();
	}
		CATCH(SqlException, e)
	{
		if (m_pTable && m_pTable->m_pSqlSession)
			m_pTable->m_pSqlSession->ShowMessage(e->m_strError);
		TRACE(e->m_strError);
		Free();
		return NONE;
	}
	END_CATCH

		return eResult;
}

//-----------------------------------------------------------------------------
int DynamicHotKeyLink::SearchComboQueryData(const int& nMaxItems, DataObjArray& arKeyData, CStringArray& arDescriptions)
{
	CParsedCtrl* pOwnerCtrl = GetOwnerCtrl();
	int nResult = 1;
	TRY
	{
		HotKeyLink::SelectionType nSelection = HotKeyLink::COMBO_ACCESS;
	// evita problemi di refresh su mancato
	// DoKillFocus quando si riapre la tendina
	DataObj* pDBFieldDataObj = GetDataObj();
	DataObj* pCurrDataObj = pDBFieldDataObj;

	if (pOwnerCtrl)
	{
		DataObj* pData = GetAttachedData();
		if (pData)
			pCurrDataObj = pData->DataObjClone();

		// se il ctrl gestisce primo e ultimo, devo ricordarmi di eliminarli
		pOwnerCtrl->GetValue(*pCurrDataObj);
		if (
				(pOwnerCtrl->GetCtrlStyle() & CTRL_STYLE_SHOW_FIRST) == CTRL_STYLE_SHOW_FIRST &&
				AfxGetCultureInfo()->IsEqual(pCurrDataObj->Str(), CParsedCtrl::Strings::FIRST())
			)
		{
			pCurrDataObj->Clear();
			pCurrDataObj->SetLowerValue(pOwnerCtrl->GetCtrlMaxLen());
		}
		else if (
					(pOwnerCtrl->GetCtrlStyle() & CTRL_STYLE_SHOW_LAST) == CTRL_STYLE_SHOW_LAST &&
					AfxGetCultureInfo()->IsEqual(pCurrDataObj->Str(), CParsedCtrl::Strings::LAST()) == 0
				)
				pCurrDataObj->SetUpperValue(pOwnerCtrl->GetCtrlMaxLen());
	}

	OnPrepareQuery(pCurrDataObj, nSelection);

	if (pOwnerCtrl)
		delete pCurrDataObj;

	m_pQuery->SetCursorType(E_FAST_FORWARD_ONLY);

	// esegue la query scelta
	BOOL bOk = m_pQuery->Open();
	if (!bOk)
		return 0;

	m_pTable = m_pQuery->GetSqlTable();
	m_pTable->GetRecord()->SetTableName(this->m_XmlDescription.GetDbTable());

	if (!PrepareFormatComboItem(m_pQuery->GetSqlTable()->GetRecord()))
	{
		nResult = 0;
		m_pQuery->Close();
		return 0;
	}

	SqlRecord* pRec = m_pQuery->GetSqlTable()->GetRecord();
	ASSERT_VALID(pRec);
	int idxDbField = GetDbFieldRecIndex(pRec);
	if (idxDbField == -1)
	{
		ASSERT(FALSE);
		m_pQuery->Close();
		return 0;
	}

	for (int nCount = 1; m_pQuery->Read() && (nMaxItems < 0 || nCount <= nMaxItems); nCount++)
	{
		SqlRecord* pRecord = m_pQuery->GetSqlTable()->GetRecord();

		DataObj* pData = GetKeyData(pRecord, idxDbField);
		ASSERT_VALID(pData);
		if (!pData) continue;

		arKeyData.Add(pData->DataObjClone());

		arDescriptions.Add(FormatComboItem(pRecord));
	}

	// è nr. massimo di elementi richiesti
	if (!m_pQuery->IsEof())
		nResult = 2;
	}

		CATCH(SqlException, e)
	{
		nResult = 0;
		m_pQuery->Close();
		THROW_LAST();
	}
	END_CATCH

		m_pQuery->Close();
	return nResult;
}

//-----------------------------------------------------------------------------
SqlParamArray* DynamicHotKeyLink::GetQuery(SelectionType nQuerySelection, CString& sQuery, CString sFilter /*= _T("")*/)
{
	TRY
	{
		OnPrepareQuery(GetDataObj(), nQuerySelection);

		BOOL bOk = m_pQuery->Open();
		if (!bOk)
			return NULL;
		m_pTable = m_pQuery->GetSqlTable();
	}
		CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH

		sQuery = m_pQuery->GetSql();

	// per avere i parametri devo tenere la tabella ancora
	// aperta. Chi usa questa funzione dovrà ricordarsi di
	// chiamare la CloseTable
	return m_pTable->m_pParamArray;
}

//-----------------------------------------------------------------------------
void DynamicHotKeyLink::CloseTable()
{
	if (m_pQuery)
	{
		m_pQuery->Close();
	}
}

//=============================================================================
