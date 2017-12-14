#include "stdafx.h"

//#include "extdoc.h"
#include "tabber.h"

#include "TbExtBEOleDataSource.h"
#include "TbExtBEDefaultCodec.h"

///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//-----------------------------------------------------------------------------

static const TCHAR szCFSampleCoDec[] = _T("Taskbuilder::DefaultCoDec");

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDefaultCoDec, CTBEDataCoDecASCII)
//-----------------------------------------------------------------------------
CTBEDefaultCoDec::CTBEDefaultCoDec(CRuntimeClass* pDBTRrc)
	:
	CTBEDataCoDecASCII()
{
	PrepareCodec(szCFSampleCoDec);
}

//-----------------------------------------------------------------------------
void CTBEDefaultCoDec::PrepareCodec(LPCTSTR strCFFormat)
{
	m_cfCoDec = ::RegisterClipboardFormat(strCFFormat);

	// Aggiungo i codec che eseguono il paste da formati differenti
	AddPasteCodecFormat(GetClipFormat(), this, FALSE);

	// Eseguo le associazioni tra source e target del paste
	// Ammesso copia/incolla nello stesso bodyedit.
	SetCodecAssociation(GetClipFormat(), GetClipFormat());

	// Drop da me stesso
	AddDropFormat	(GetClipFormat());
}

//-----------------------------------------------------------------------------
CTBEDefaultCoDec::~CTBEDefaultCoDec()
{
}

//-----------------------------------------------------------------------------
void CTBEDefaultCoDec::LoadRows(CAbstractFormDoc* pDocument, DBTSlaveBuffered* pDBT, CBodyEdit* pBody, int nRecIdx)
{
	ASSERT(m_pParsedDoc);
	if (!m_pParsedDoc)	return;

	m_pDocument	= pDocument;
	BOOL bSetModify = FALSE;

	CRuntimeClass*		classDBT = pDBT->GetRuntimeClass();
	CTBEDataCoDecDBT*	pClip_DBT = m_pParsedDoc->GetDBTSlave(classDBT);
	if (!pClip_DBT)
	{
		pClip_DBT = m_pParsedDoc->GetDBTSlaveSqlRec(pDBT->GetRecord()->GetRuntimeClass());
		if (!pClip_DBT)
		{
			pClip_DBT = m_pParsedDoc->GetDBTSlaveBuffered();
		}
	}
	ASSERT(pClip_DBT);

	RecordArray arRows;
	if (pClip_DBT)
	{
		//PASTE STEP
		int r = 0;
		for (r = 0; r <= pClip_DBT->GetUpperBound(); r++)
		{
			SqlRecord* pDropRow = pDBT->GetRecord()->Create();

			CTBEDataCoDecPastedRecord pr(pDBT, pBody, pClip_DBT, r, pDropRow);
			if (!m_pDocument->DispatchOnPasteDBTRows(pr))
			{
				return;
			}

			arRows.Add (pDropRow); 
		}

		//VALIDATION STEP
		CTBEDataCoDecRecordToValidate vr (pDBT, pBody, pClip_DBT);
		if (!m_pDocument->DispatchOnValidatePasteDBTRows(arRows, vr))
		{
			return;
		}

		//DBT INSERT STEP
		for (r = 0; r < arRows.GetSize(); r++)
		{
			SqlRecord* pDropRow = arRows[r];

			SqlRecord* pTargetRow = NULL;
			if (nRecIdx == -1)
				pTargetRow = pDBT->AddRecord();
			else
			{
				pTargetRow = pDBT->InsertRecord(nRecIdx);
				nRecIdx++;
			}

			pTargetRow->CopyRecord (pDropRow);

			pTargetRow->SetStorable();
			bSetModify = TRUE;
		}
	}
	pDocument->SetModifiedFlag(bSetModify);
}

///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBEDefaultDropTargetCoDec, CTBExtBEDropTarget)

//-----------------------------------------------------------------------------
void CTBEDefaultDropTargetCoDec::OnDropCoDec	(CTBEDataCoDec*	pClpBrdDataCodec)
{
	int	nIdxRec = m_nDropRecordIdx;
	if (nIdxRec != -1 && m_dstWhere == CBodyEdit::DST_BOTTOM)
		nIdxRec ++;

	CWaitCursor		wait;
	ASSERT(pClpBrdDataCodec);
	if (pClpBrdDataCodec && m_pTarget->CanDoPaste(pClpBrdDataCodec,TRUE))
		m_pTarget->DoPasteForDrop(pClpBrdDataCodec, nIdxRec, m_dstWhere);

	GetDocument()->UpdateDataView();
}

///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
//			class CTBEDefaultBaseDropTarget
//-----------------------------------------------------------------------------
DROPEFFECT CTBEDefaultBaseDropTarget::OnDragEnter(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	return DROPEFFECT_COPY;
}

//-----------------------------------------------------------------------------
DROPEFFECT CTBEDefaultBaseDropTarget::OnDragOver(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	if (pWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
	{
		//CAbstractFormView*	pView	= (CAbstractFormView*) pWnd;
		TRACE2("View (%d, %d)\n", point.x, point.y);

		ActivateMainFrame(pWnd);
	}
/* TODO da TabPro a TabCtrl
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTabManager)))
	{
		CTabManager*	pTabManager = (CTabManager*) pWnd;
		TRACE2("Tab Manager (%d, %d)\n", point.x, point.y);

		ActivateMainFrame(pWnd);
		
		int nPos = pTabManager->GetMouseOverTab();
		if (nPos < 0 || nPos > pTabManager->GetDlgInfoArray()->GetUpperBound())
			 return DROPEFFECT_NONE;

		// @@TODO: cambio tab solo se sono su un documento diverso dal source
		// altrimenti rischio di non avere più disponibili i dati al momento del drop
		BOOL bCambiaTab = TRUE;
		
		// controllo se è disponibile un cliformat TBECF per bodyedit
		if (pDataObject->IsDataAvailable(::GetTBExtBodyCF()))
		{
			// controllo se l'applicazione che ha registrato il clipformat 
			// è la stessa di quella su cui sto facendo il drop

			// prendo il puntatore al bodyedit dal clipformat
			CBodyEdit* pBody = GetSourceBody(pDataObject);

			// ciclo sui parent del bodyedit per vedere se
			// il tabmanager pWnd è nella gerarchia
			CWnd* parentWnd = pBody->GetParent();
			while (!parentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			{
				if (pWnd == parentWnd)
				{
					bCambiaTab = FALSE;
					break;
				}
				parentWnd = parentWnd->GetParent();
			}
		}

		if (bCambiaTab)
		{		
			CBaseTabDialog*	pCurrent = pTabManager->GetActiveDlg();
			if (pCurrent && pCurrent->CheckForm(TRUE))
			{
				DlgInfoItem* pDlgInfoItem = pTabManager->GetDlgInfoArray()->GetAt(nPos);

				pTabManager->SetTab(nPos);
				
				// controllo se la tab è abilitata
				int nState = pTabManager->GetTabState();
				if (nState != TAB_STATE_DISABLED_TEXT && pTabManager->TabDialogActivate(pTabManager->GetDlgCtrlID(), pDlgInfoItem->GetDialogID()) == 1)
				{
					// tab attivata
				}
			}
		}
	}
*/
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CFrameWnd)))
	{
		CFrameWnd*	pFrame= (CFrameWnd*) pWnd;
		TRACE2("Frame (%d, %d)\n", point.x, point.y);

		CWnd*		pCurrentWnd = pWnd->GetFocus();
		
		if (pCurrentWnd)
		{
			CFrameWnd*	pActiveFrame = pCurrentWnd->GetParentFrame();

			// attivo la frame solo se non è già attiva			
			if (pActiveFrame != pFrame)
			{
				pFrame->ActivateFrame	(SW_NORMAL);
				pFrame->SetFocus		();
			}
		}
	}

	return DROPEFFECT_NONE;
}

//-----------------------------------------------------------------------------
void CTBEDefaultBaseDropTarget::OnDragLeave(CWnd* pWnd)
{
}


//-----------------------------------------------------------------------------
//			class CTBEDefaultBaseDropTargetArray
//-----------------------------------------------------------------------------
void CTBEDefaultBaseDropTargetArray::AutoRegister(CAbstractFormView* pView)
{
	Register(pView);	
	if (pView->GetFrame())
		Register(pView->GetFrame());	
	
	TabManagers*		pTabManagers = pView->m_pTabManagers;		// contiene i tab dialog manager
	if (pTabManagers)
	{
		for (int c = 0; c <= pTabManagers->GetUpperBound(); c++)
		{
			CWnd*	pWnd = pTabManagers->GetAt(c);
			if (pWnd && ::IsWindow(pWnd->m_hWnd))
			{
				Register(pWnd);	
			}
		}
	}
}


//-----------------------------------------------------------------------------
void CTBEDefaultBaseDropTargetArray::Register(CWnd* pWnd)
{
	CTBEDefaultBaseDropTarget*	pDT = new CTBEDefaultBaseDropTarget();
	pDT->Register(pWnd);	
	Add(pDT);
}


//-----------------------------------------------------------------------------
//	class CTBEDefaultBaseDropTargetPlugIn
//-----------------------------------------------------------------------------
CTBEDefaultBaseDropTargetPlugIn::CTBEDefaultBaseDropTargetPlugIn()
						:
						m_bDropTargetEnabled (FALSE)	
{}


