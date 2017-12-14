
#include "stdafx.h"

#include <TbNameSolver\templates.h>

#include <TbGeneric\globals.h>

#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>	

#include <TbWoormEngine\ReportLink.h>

#include <TbWoormViewer\woormdoc.h>
#include <TbWoormViewer\table.h>
#include <TbWoormViewer\cell.h>

#include <TbGes\TblRead.h>
#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>
#include <TbGes\formmng.h>

#include "WrmRdrdoc.h"
#include "WrmRdrfrm.h"
#include "WrmRdrvw.h"

// risorse
#include <TbWoormViewer\Woormdoc.hjson> //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif
///////////////////////////////////////////////////////////////////////////////
//					class CWrmRadarDoc
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWrmRadarDoc, CWoormDocMng)

BEGIN_MESSAGE_MAP(CWrmRadarDoc, CWoormDocMng)
	ON_COMMAND(ID_ALLOW_EDITING,  			OnAllowEditing)
	ON_COMMAND(ID_FILE_SAVE,  				OnFileSave)
	ON_COMMAND(ID_FILE_SAVE_AS,  			OnFileSaveAs)

	ON_COMMAND(ID_VK_UP,					OnVKUp)
	ON_COMMAND(ID_VK_DOWN,					OnVKDown)
	ON_COMMAND(ID_VK_RETURN,				OnVKReturn)
	
	ON_COMMAND(ID_ESCAPE,					OnEscape)

	ON_UPDATE_COMMAND_UI(ID_VK_UP,			OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_DOWN,		OnUpdateVKMove)

	ON_COMMAND(ID_WRM_MINUS_LINES,			OnUpdateMinusMultiLines)
	ON_COMMAND(ID_WRM_PLUS_LINES,			OnUpdatePlusMultiLines)	
END_MESSAGE_MAP()

// class CWrmRadarDoc construction/destruction
//-----------------------------------------------------------------------------
CWrmRadarDoc::CWrmRadarDoc()
	:
	m_bTemporary		(FALSE)
{
	// modifico i valori di default
	m_pDataDefaults->m_rgbColumnBorder		= AfxGetThemeManager()->GetRadarColumnBorderColor();
	m_pDataDefaults->m_rgbColumnTitleBorder	= AfxGetThemeManager()->GetRadarColumnTitleBorderColor();
	m_pDataDefaults->m_rgbPageBkgn			= AfxGetThemeManager()->GetRadarPageBkgColor();
	m_pDataDefaults->m_bUseAsRadar			= TRUE;
}

//-----------------------------------------------------------------------------
CWrmRadarDoc::~CWrmRadarDoc()
{
	// pulisco quello che c'e` da pulire prima di morire
	SAFE_DELETE(m_pDynamicSqlRecord)

	// Notifico al documento collegato l'avvenuta morte
	OnRadarDied ();
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::OnOpenDocument(LPCTSTR pObject)
{
	if (!CWoormDocMng::OnOpenDocument(pObject))
		return FALSE;
	
	if (!m_arWoormLinks.GetConnectionRadar())
	{
		AfxMessageBox(cwsprintf(_TB("The report {0-%s} is unusable as radar"), m_strTitle));
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CWrmRadarDoc::UpdateWhereClause(SqlTable* pSqlTable) 
{
	CString strAuxFilter = pSqlTable->GetNativeFilter();

	CString sFrom = pSqlTable->GetAllTableName();

	if (!strAuxFilter.IsEmpty())
	{
		strAuxFilter = _T("FROM ") + sFrom + _T(" WHERE ") + strAuxFilter;
	}
	if (!pSqlTable->m_strSort.IsEmpty() )
	{
		if(strAuxFilter.IsEmpty())
			strAuxFilter = _T("FROM ") + sFrom;

		strAuxFilter += _T(" ORDER BY ") + pSqlTable->m_strSort;
	}
	return strAuxFilter;
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::Run(const CString& strDocumentFilter)
{
	SetAuxDocumentFilter(strDocumentFilter);

	RunReport();
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::CloseRadar ()
{
	if (m_bTemporary && ExistFile(m_strPathName))
		DeleteFile (m_strPathName);
}

//-----------------------------------------------------------------------------
CWrmRadarView*	CWrmRadarDoc::GetWrmRadarView ()
{
	POSITION pos = GetFirstViewPosition();
	CWrmRadarView* pView = (CWrmRadarView*) GetNextView(pos);
	ASSERT_VALID (pView);
	return pView;
}

// Ritorna la frame del WrmRadar
//-----------------------------------------------------------------------------
CWrmRadarFrame* CWrmRadarDoc::GetWrmRadarFrame()
{
	return GetWrmRadarView()->GetWrmRadarFrame();
}


// Permette di cambiare lo stato di 'stay alive'. Se è attivo viene disattivato 
// altrimenti attivato. Il collegamento automatico tra record selezionato e sua
// visualizzazione all'interno del documento sara abilitato solo se è attivo
// lo stato di 'stay alive'.
//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::ChangeStayAlive()
{
	m_bStayAlive = !m_bStayAlive;
	return m_bStayAlive;
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::OnEscape()
{
	if (!m_bEngineRunning)
	{
		GetWrmRadarFrame()->PostMessage(WM_COMMAND, ID_FILE_CLOSE);
	}
	else
		OnStop();
}

//------------------------------------------------------------------------------
void CWrmRadarDoc::OnAllowEditing ()
{
	__super::OnAllowEditing();
	m_bStayAlive = TRUE;
}

// se e` temporaneo deve salvare con nome diverso
//------------------------------------------------------------------------------
void CWrmRadarDoc::OnFileSave()
{
	if (!m_bTemporary)	
		__super::OnFileSave();
	else				
		OnFileSaveAs();
}


// essendo temporaneo deve salvare con nome diverso
//------------------------------------------------------------------------------
void CWrmRadarDoc::OnFileSaveAs()
{
	//changes locally the value, and restores it at the end of scope
	CLocalChange<BOOL> _l(m_bStayAlive, TRUE);
	CWoormDocMng::OnFileSaveAs();

	m_bTemporary = FALSE;

	// inibisce il lancio ricorsivo del radar appena aggiunto
	if (m_pCallerDoc) 
		m_pCallerDoc->DoCustomize(FALSE, GetPathName());
}


// Connette il WrmRadar al documento passato come parametro (chiamata da DBT).
// Si assume una frame e un solo documento
//-----------------------------------------------------------------------------
void CWrmRadarDoc::Attach (CAbstractFormDoc* pCallerDoc, BOOL bTemporary)
{
	ASSERT_VALID (pCallerDoc);

	ASSERT (m_pCallerDoc		== NULL);
	ASSERT (m_pCallerSqlRecord	== NULL);
	ASSERT (m_pDynamicSqlRecord == NULL);

	m_bTemporary		= bTemporary;
	m_pCallerHotLink 	= NULL;
	m_pCallerDoc		= pCallerDoc;
	m_pCallerSqlRecord	= pCallerDoc->m_pDBTMaster->GetRecord();
	m_pDynamicSqlRecord	= (SqlRecord*)m_pCallerSqlRecord->Create();
	
	m_pDynamicSqlRecord->SetConnection(m_pCallerSqlRecord->GetConnection());

	// recupero gli eventuali valori dai campi findable 
	if( m_pCallerDoc->GetFormMode() != CBaseDocument::FIND )
		return;

	for (int i = 0; i <= m_pCallerSqlRecord->GetUpperBound(); i++)
	{
		DataObj* pMasterDataObj = m_pCallerSqlRecord->GetDataObjAt(i);

		if (!pMasterDataObj->IsFindable())
			continue;

		// Se l'utente forza il modificato viene tenuto presente anche il
		// campo vuoto (per tener conto dei valori di default per enum 
		// e i valori zero per i numeri)
		if	(!pMasterDataObj->IsEmpty() || pMasterDataObj->IsValueChanged())
		{
			DataObj* pField = pMasterDataObj->DataObjClone();
			if (pMasterDataObj->GetDataType() == DATA_STR_TYPE)
			{
				*((DataStr*)pField) += _T("%");
			}
			m_pWoormInfo->InternalAddParam(m_pCallerSqlRecord->GetColumnName(i), pField, TRUE);
			delete pField;
		}
	}
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::Attach (HotKeyLink* pKkl, SqlTable*, SqlRecord* pRec)
{
	ASSERT (m_pCallerSqlRecord	== NULL);
	ASSERT (m_pDynamicSqlRecord == NULL);

	m_bTemporary		= FALSE;
	m_pCallerDoc		= NULL;
	m_pCallerHotLink 	= pKkl;

	m_pCallerSqlRecord	= pRec;
	m_pDynamicSqlRecord	= pRec->Create();
	m_pDynamicSqlRecord->SetConnection(m_pCallerSqlRecord->GetConnection());
}

// Sconnette il WrmRadar dal document a cui era collegato.
//-----------------------------------------------------------------------------
void CWrmRadarDoc::Detach ()
{
	m_pCallerDoc = NULL;
	m_pCallerSqlRecord = NULL;
	SAFE_DELETE(m_pDynamicSqlRecord);
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::OnVKUp()
{
	if (m_bAllowEditing)
	{
		__super::OnVKUp();
		return;
	}
	VKUpDown(TRUE);
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::OnVKDown()
{
	if (m_bAllowEditing)
	{
		__super::OnVKDown();
		return;
	}
	VKUpDown(FALSE);
}

//-----------------------------------------------------------------------------
void CWrmRadarDoc::OnVKReturn()
{
	if (m_bAllowEditing)
	{
		__super::OnVKReturn();
		return;
	}
	//asincrono, altrimenti la routing frame risulta essere quella del radar e una eventuale messagebox di retry lock tenterebbe
	//di prenderla come parent anche se sta morendo!
	AfxInvokeAsyncThreadFunction<BOOL, CWrmRadarDoc, Table*, int, BOOL, BOOL>
		(
		::GetCurrentThreadId(), 
		this, 
		&CWrmRadarDoc::RecordSelected,
		GetConnectedTable(), 
		m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow, 
		TRUE, 
		TRUE
		);
}


//-----------------------------------------------------------------------------
void CWrmRadarDoc::VKUpDown(BOOL bUp)
{
	Table* pTable = GetConnectedTable();
	if (pTable == NULL) return;

	int nRow = m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow;
	if (bUp)
	{
		nRow--; 
		if (nRow < 0) 
		{ 
			if (m_pRDEmanager->CurrPageRead() == 0)
			{
				nRow = 0;
				return;
			}

			nRow = pTable->LastRow(); 
			ReadPrevPage();
		}
	}
	else 
	{
		nRow++;
		if (nRow > pTable->LastRow()) 
		{
			if (m_pRDEmanager->CurrPageRead() == m_pRDEmanager->LastPage())
			{
				nRow = pTable->LastRow();
				return;
			}
			nRow = 0; 
			ReadNextPage(); 
		}
	}

	RecordSelected(pTable, nRow, IsStayAlive(), FALSE);
}

//-----------------------------------------------------------------------------
Table* CWrmRadarDoc::GetConnectedTable ()
{
	BaseObj* pObj = NULL;
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		pObj = GetObjects()[i];

		if (pObj->GetInternalID() == m_arWoormLinks.GetConnectionRadar()->m_nAlias)
			break;
	}

	ASSERT(pObj->GetInternalID() == m_arWoormLinks.GetConnectionRadar()->m_nAlias);
	ASSERT(pObj->IsKindOf(RUNTIME_CLASS(Table)));
	return STATIC_DOWNCAST(Table, pObj);
}
//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::CanDoExit () 
{ 
	return !IsStayAlive() && 
		!m_bIsPrinting &&
		!m_bIsExporting &&
		(m_pEngine && !m_pEngine->IsRunning()) &&
		!AfxIsThreadInModalState()	&&
		!IsWaiting(); ; 
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::OnWrmRadarRecordSelected (BOOL bActivateDoc)
{
	// Selezione del record collegato con il Documento DBT se sono stati caricati
	// i dati del report e se c'e` connessione altrimenti non ritorno niente
	if ((m_pCallerDoc || m_pCallerHotLink) && m_arWoormLinks.GetConnectionRadar()->m_pLocalSymbolTable->GetSize()) 
	{
		for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
		{
			BaseObj* pObj = GetObjects()[i];
			if (!pObj->InMe(m_ptCurrPos))
				continue;

			if (pObj->GetInternalID() != m_arWoormLinks.GetConnectionRadar()->m_nAlias)
				continue;

			ASSERT(pObj->IsKindOf(RUNTIME_CLASS(Table)));
			Table* pTable = STATIC_DOWNCAST(Table, pObj);

			int nRow = -1;
			if (pTable->GetPosition(m_ptCurrPos, nRow) != Table::POS_ROW)
				continue;

			return RecordSelected(pTable, nRow, bActivateDoc || IsStayAlive(), bActivateDoc);
		}

		Invalidate		();
		UpdateWindow	();
	}
	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::GetDataOfRecordSelected(Table* pTable, int nRow)
{
	BOOL bValid = TRUE;
	//m_pDynamicSqlRecord->SetValid();
	for (int i = 1; i <= m_arWoormLinks.GetConnectionRadar()->m_pLocalSymbolTable->GetUpperBound(); i++)
	{
		WoormField* pItem = m_arWoormLinks.GetConnectionRadar()->m_pLocalSymbolTable->GetAt(i);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;
		CString sName = pItem->GetName();
		int nIdx = sName.Find('.');
		if (nIdx >= 0)
			sName = sName.Mid(nIdx + 1);
		int nPos = m_pDynamicSqlRecord->GetIndexFromColumnName(sName);
		//ASSERT(nPos >= 0);
		if (nPos < 0) 
			continue; //@@TODO dare messaggio

		DataObj* pDataObj =	m_pDynamicSqlRecord->GetDataObjAt(nPos);
		ASSERT(pDataObj);

		// si fa valorizzare il dato se esiste altrimenti non connette
		// il radar al documento
		TableCell* pCell = pTable->GetCellFromID(nRow, pItem->GetId());
		if (!pCell) 
			return FALSE;

		// se non contiene dati buoni (validati da RDE) non connette il documento
		bValid = bValid && pCell->IsEnabledRDEData();
		if (bValid) 
			pDataObj->Assign(pCell->m_Value.m_RDEdata);
	}

	CRect reportRect,rowRect;
	this->GetWrmRadarView()->GetClientRect(reportRect); 
	rowRect = pTable->RowRect(nRow);
	
	// normalize mouse position to reflect new window origin
	CClientDC dc(this->GetWrmRadarView());
	this->GetWrmRadarView()->OnPrepareDC(&dc);
	
	dc.DPtoLP(reportRect);

	if (rowRect.bottom > reportRect.bottom)
	{
		//scroll down
		GetWrmRadarView()->SetScrollPos(SB_VERT, GetWrmRadarView()->GetScrollPos(SB_VERT) + pTable->RowHeight()); 
		GetWrmRadarView()->ScrollWindow(0, -pTable->RowHeight()) ;
		InvalidateRect(reportRect);
	}
	else if (rowRect.top < reportRect.top)
	{	
		//scroll up 
		GetWrmRadarView()->SetScrollPos(SB_VERT, GetWrmRadarView()->GetScrollPos(SB_VERT) - pTable->RowHeight()); 
		GetWrmRadarView()->ScrollWindow(0, pTable->RowHeight()) ;
		InvalidateRect(reportRect);
	}
	else
	{
		// gestire il refresh delle sole righe evidenziate e deevidenziate
		InvalidateRect(pTable->RowRect(m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow), TRUE);
		InvalidateRect(pTable->RowRect(nRow), TRUE);
	}

	m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow = nRow;

	UpdateWindow();

	RecordReader rr(m_pDynamicSqlRecord, m_pCallerDoc);
	if (rr.FindRecord() == TableReader::FOUND)
		*m_pDynamicSqlRecord = *rr.GetRecord();

	return bValid;
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::RecordSelected(Table* pTable, int nRow, BOOL bLinked, BOOL bActivateDoc)
{
	if (m_arWoormLinks.GetConnectionRadar()->m_pLocalSymbolTable->GetSize() == 0 || !IsDataLoaded()) 
		return FALSE;

	if (m_pCallerDoc) 
	{
		// Abilitazione dell'applicazione e visualizzazione del cursore precedente
		if (bLinked && m_pCallerDoc->IsModified() && AfxMessageBox(_TB("The document has been changed.\r\nIgnore changes and continue?"), MB_YESNO) != IDYES)
			return FALSE;

		BOOL bValid = GetDataOfRecordSelected(pTable, nRow);

		if (bLinked && bValid && m_pCallerDoc->DispatchOnValidateRadarSelection(m_pDynamicSqlRecord))
		{
			//TODO sono valorizzati solo i campi chiave
			*m_pCallerSqlRecord = *m_pDynamicSqlRecord;
			
			m_pCallerDoc->OnRadarRecordSelected(bActivateDoc);
			return TRUE;
		}
	}
	else if (m_pCallerHotLink)
	{
		BOOL bValid = GetDataOfRecordSelected(pTable, nRow);

		if (!m_pCallerHotLink->OnValidateRadarSelection(m_pDynamicSqlRecord))
		{
			Message(m_pCallerHotLink->GetErrorString(TRUE));
			return FALSE;
		}
		else if (!m_pCallerHotLink->GetWarningString(FALSE).IsEmpty())
		{
			Message(m_pCallerHotLink->GetWarningString(TRUE), 0,0,0, CMessages::MSG_WARNING);
		}

		if (bValid)
		{
			*m_pCallerSqlRecord = *m_pDynamicSqlRecord;

			if (m_pCallerHotLink->IsKindOf(RUNTIME_CLASS(DynamicHotKeyLink)))
				((DynamicHotKeyLink*)m_pCallerHotLink)->SetRecord(m_pDynamicSqlRecord);
			
			m_pCallerHotLink->FindRecord(m_pCallerHotLink->GetDataObj(), FALSE, FALSE, TRUE);
			
			m_pCallerHotLink->OnRadarRecordAvailable();
			return TRUE;
		}
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarDoc::Customize (HotKeyLinkObj::SelectionType nQuerySelection, CString& sAuxQuery)
{
	if (m_pCallerHotLink == NULL)
		return FALSE;
	
	DataObj* pData = m_pCallerHotLink->GetAttachedData();

	//fixed parameter for hotlink (selection type, code, description), passed by woormInfo to Radar report
	
	SymField* pHkl_Selection			= m_pCallerHotLink->GetSymTable()->GetField(CHotlinkDescription::s_SelectionType_Name);
	SymField* pHkl_FilterValue			= m_pCallerHotLink->GetSymTable()->GetField(CHotlinkDescription::s_FilterValue_Name);
		
	SymField* p_HklFilterValue;
	p_HklFilterValue = pHkl_FilterValue;

	int index = m_pWoormInfo->GetParamIndex(pHkl_Selection->GetName());
	if (index < 0)
		m_pWoormInfo->AddParam(pHkl_Selection->GetName(), pHkl_Selection->GetData());
	else
		m_pWoormInfo->GetParamValue(index)->Assign(*pHkl_Selection->GetData());

	index = m_pWoormInfo->GetParamIndex(p_HklFilterValue->GetName());
	if (index < 0)
		m_pWoormInfo->AddParam(p_HklFilterValue->GetName(), p_HklFilterValue->GetData());
	else
		m_pWoormInfo->GetParamValue(index)->Assign(*p_HklFilterValue->GetData());

	for (int i = 0; i < m_pCallerHotLink->GetHotlinkDescription()->GetParameters().GetSize(); i++)
	{
		CDataObjDescription* param = m_pCallerHotLink->GetHotlinkDescription()->GetParamDescription(i);

		int idx = m_pWoormInfo->GetParamIndex(param->GetName());
		if (idx < 0)
			m_pWoormInfo->AddParam(param->GetName(), param->GetValue());
		else
			m_pWoormInfo->GetParamValue(idx)->Assign(*param->GetValue());
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CWrmRadarDoc::CanDoExportData (UINT)
{
	BOOL bOk = __super::CanDoExportData(0);
	if (!bOk)
		return FALSE;

	if (!OSL_IS_PROTECTED(GetInfoOSL()))
	{
		if (m_pCallerDoc)
		{
			bOk = OSL_CAN_DO(m_pCallerDoc->GetInfoOSL(), OSL_GRANT_EXPORT); 
		}
		else if (m_pCallerHotLink )
		{
			bOk = OSL_CAN_DO(m_pCallerHotLink->GetInfoOSL(), OSL_GRANT_EXPORT); 
		}
	}
	return bOk;
}

//------------------------------------------------------------------------------
BOOL CWrmRadarDoc::CanDoSendMail ()
{	
	BOOL bOk = __super::CanDoSendMail();
	if (!bOk)
		return FALSE;

	if (!OSL_IS_PROTECTED(GetInfoOSL()))
	{
		if (m_pCallerDoc)
		{
			bOk = OSL_CAN_DO(m_pCallerDoc->GetInfoOSL(), OSL_GRANT_EXPORT); 
		}
		else if (m_pCallerHotLink )
		{
			bOk = OSL_CAN_DO(m_pCallerHotLink->GetInfoOSL(), OSL_GRANT_EXPORT); 
		}
	}
	return bOk;
}

//------------------------------------------------------------------------------
void CWrmRadarDoc::OnUpdatePlusMultiLines()
{
	//TODO gestire solo tabella/e selezionate	
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnUpdatePlusMultiLines();
	}
}

//------------------------------------------------------------------------------
void CWrmRadarDoc::OnUpdateMinusMultiLines()
{
	//TODO gestire solo tabella/e selezionate
	for (int i = 0; i <= GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = GetObjects()[i];
		if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
			continue;

		((Table*)pObj)->OnUpdateMinusMultiLines();
	}

}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CWrmRadarDoc::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CWrmRadarDoc\n");
	CWoormDocMng::Dump(dc);
}
#endif // _DEBUG
