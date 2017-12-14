
#include "stdafx.h"

#include "wrmrdrdoc.h"
#include "wrmrdrfrm.h"
#include "wrmrdrvw.h"
#include <TbWoormViewer\table.h>
#include <TbWoormViewer\export.h>
#include <TbWoormViewer\expExter.h>
#include <TbWoormViewer\WoormDoc.hjson>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CWrmRadarView
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWrmRadarView, CWoormView)

BEGIN_MESSAGE_MAP(CWrmRadarView, CWoormView)

	ON_WM_LBUTTONDBLCLK	()
	ON_WM_LBUTTONDOWN	()
	ON_WM_MOUSEWHEEL	()
	ON_COMMAND			(ID_VK_UP, OnVKUp)
	ON_COMMAND			(ID_VK_DOWN, OnVKDown)
	ON_UPDATE_COMMAND_UI(ID_VK_UP, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_DOWN, OnUpdateVKMove)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CWrmRadarView::CWrmRadarView()
{
}

//-----------------------------------------------------------------------------
CWrmRadarView::~CWrmRadarView()
{
}

//-----------------------------------------------------------------------------
void CWrmRadarView::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	// avoid processing of incoming OnLButtonDown
	// condizioni meno restrittive per esportazione: riverifico dopo
	if	(
			GetDocument()->IsEngineRunning() || 
/*			GetDocument()->m_bAllowEditing || */
			m_pProcessingMouse->IsLocked()
		)
		return;

	// normalize mouse position to reflect new window origin
	CClientDC dc(this);

	OnPrepareDC(&dc);
	GetDocument()->m_ptCurrPos = point;
	dc.DPtoLP(&GetDocument()->m_ptCurrPos);
	UnScalePoint(GetDocument()->m_ptCurrPos, dc);

	BOOL bStayAlive = GetDocument()->IsStayAlive();
	if (bStayAlive)
		GetDocument()->SetWaiting(TRUE);

	BOOL bCanUnlock = !GetDocument()->OnWrmRadarRecordSelected() || bStayAlive;

	// il documento se non e` fissato viene chiuso dal cambio di
	// attivazione (OnMDIActivate) e pertanto non ho tempo di finire di 
	// processare il comando
	if (bCanUnlock)
		m_pProcessingMouse->Unlock();
	
	if (bStayAlive)
		GetDocument()->SetWaiting(FALSE);
}	

//------------------------------------------------------------------------------
void CWrmRadarView::OnLinkSelected()
{
	BOOL bStayAlive = GetDocument()->IsStayAlive();
	GetDocument()->OnLinkSelected();
	if (bStayAlive)
		m_pProcessingMouse->Unlock();
}

//------------------------------------------------------------------------------
void CWrmRadarView::OnLButtonDown(UINT nFlags, CPoint point)
{
	if	(
			GetDocument()->m_bAllowEditing ||
			GetDocument()->IsEngineRunning() || 
			GetDocument()->IsMouseHoverLink() ||
			m_pProcessingMouse->IsLocked() 
		)
	{
		CWoormView::OnLButtonDown(nFlags, point);
		return;
	}

	// normalize mouse position to reflect new window origin
	CClientDC dc(this);

	OnPrepareDC(&dc);
	GetDocument()->m_ptCurrPos = point;
	dc.DPtoLP(&GetDocument()->m_ptCurrPos);
	
	CDC* pDC = GetDC();
	ASSERT(pDC);
	UnScalePoint(GetDocument()->m_ptCurrPos, *pDC);
	ReleaseDC(pDC);

	// gestione della selezione degli oggetti per export data		
	BOOL bClickOnColumnTitle = FALSE, bClickAsShift = FALSE;
	for (int i = 0; i <= this->GetDocument()->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = this->GetDocument()->GetObjects()[i];
		if ( (pObj->IsKindOf(RUNTIME_CLASS(Table))) && (pObj->InMe(point)) ) //sono su tabella
		{	
			//controllo se sono su titolo, allora abilito selezione con click semplice
			int nRow = -1; int nAlias = 0;
			if ( ((Table*)pObj)->GetPosition(GetDocument()->m_ptCurrPos, nRow, &nAlias) == Table::POS_COLUMN )
			{	
				int idxCol = ((Table*)pObj)->GetIdxColFromAlias(nAlias);
				if ( 
					idxCol >= 0 &&
					GetDocument()->m_pExportData->IsTableItem()
					 &&	
					 GetDocument()->m_pExportData->IncludeColumn(idxCol)
					 )
						bClickAsShift = TRUE;
				
				bClickOnColumnTitle = TRUE;
				break;
			}
		}
	}

	if	((nFlags & MK_CONTROL) || bClickOnColumnTitle)
	{			
		GetDocument()->SelectForExportData (GetDocument()->m_ptCurrPos, bClickOnColumnTitle, ((nFlags & MK_SHIFT) == MK_SHIFT) || bClickAsShift);
		
		m_pProcessingMouse->Unlock();
		return;
	}
	
	// reenable incoming buttondown
	m_pProcessingMouse->Unlock();
	
	GetDocument()->OnWrmRadarRecordSelected(FALSE);
}

//------------------------------------------------------------------------------
/*
nFlags : Indicates whether various virtual keys are down. This parameter can be any combination of the following values: 
	MK_CONTROL   Set if the CTRL key is down. 
	MK_LBUTTON   Set if the left mouse button is down. 
	MK_MBUTTON   Set if the middle mouse button is down. 
	MK_RBUTTON   Set if the right mouse button is down. 
	MK_SHIFT	 Set if the SHIFT key is down. 
zDelta : Indicates distance rotated. 
	The zDelta value is expressed in multiples or divisions of WHEEL_DELTA, which is 120. 
	A value less than zero indicates rotating back (toward the user) 
	while a value greater than zero indicates rotating forward (away from the user). 
	The user can reverse this response by changing the Wheel setting in the mouse software. 
	See the Remarks for more information about this parameter. 
pt : Specifies the x- and y-coordinate of the cursor. These coordinates are always relative to the upper-left corner of the screen. 

*/
BOOL CWrmRadarView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	if (zDelta > 0)
	{
		GetDocument()->OnVKUp ();
	}
	else if (zDelta < 0)
	{
		GetDocument()->OnVKDown ();
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWrmRadarView::OnVKDown()
{
	CWrmRadarDoc* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKDown();
}

//-----------------------------------------------------------------------------
void CWrmRadarView::OnVKUp()
{
	CWrmRadarDoc* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKUp();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CWrmRadarView::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CWrmRadarView\n");
	CWoormView::Dump(dc);
}

#endif // _DEBUG
