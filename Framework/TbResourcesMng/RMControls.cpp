#include "stdafx.h"

#include <TbGenlib\PARSOBJ.H>

// resources
#include <TbGenlib\parsres.hjson> //JSON AUTOMATIC UPDATE

#include "ADMResourcesMng.h"
#include "RMControls.h"
#include "RMControls.hjson"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//             class CWorkerStatic implementation							//
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CWorkerStatic, CStrStatic)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWorkerStatic, CStrStatic)
	//{{AFX_MSG_MAP(CWorkerStatic)
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CWorkerStatic::CWorkerStatic()
	:
	CStrStatic(),
	m_pWorker(NULL)
{
	m_bMouseInside = FALSE;

	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_TB_HAND), RT_GROUP_CURSOR);
	m_hHandCursor = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TB_HAND));

	SetColored(TRUE);
	SetTextColor(RGB(0, 0, 255));
	SetBkgColor(AfxGetBkgColor());
}

//-----------------------------------------------------------------------------
BOOL CWorkerStatic::OnInitCtrl()
{
	CFont* pObjectFont = AfxGetHyperlinkFont();
	if (m_pOwnerWnd)
		m_pOwnerWnd->SetFont(pObjectFont);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWorkerStatic::OnLButtonUp(UINT nFlags, CPoint point)
{
	::SetCursor(m_hOldCursor);
	
	ReleaseCapture();
	m_bMouseInside = FALSE;
	// attiva l'apertura del documento autoinviandosi un messaggio, in modo da fare finire le attività
	// legate alla visualizzazione (cambio cursore, ecc.) per non rischiare di inceppare la perdita del fuoco 
	CStrStatic::OnLButtonUp(nFlags, point);
	OpenWorker();
}

//-----------------------------------------------------------------------------
void CWorkerStatic::OnMouseMove(UINT nFlags, CPoint point)
{
	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (m_bMouseInside == bInside)
		return;

	m_bMouseInside = bInside;

	if (m_bMouseInside)
	{
		m_hOldCursor = ::SetCursor(m_hHandCursor);
		SetCapture();
	}
	else
	{
		::SetCursor(m_hOldCursor);
		ReleaseCapture();
	}
	CStrStatic::OnMouseMove(nFlags, point);
}

//-----------------------------------------------------------------------------
void CWorkerStatic::OpenWorker()
{
	if (!m_pWorker || (*m_pWorker).IsEmpty())
		return;

	ADMWorkersObj* pDoc = (ADMWorkersObj*)AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMWorkersObj), szDefaultViewMode, NULL);
	if (pDoc)
		pDoc->SetWorker(*m_pWorker);
}

//////////////////////////////////////////////////////////////////////////////
//             class CResourcesPictureStatic implementation			 		//
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CResourcesPictureStatic, CPictureStatic)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CResourcesPictureStatic, CPictureStatic)
	//{{AFX_MSG_MAP(CResourcesPictureStatic)
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CResourcesPictureStatic::CResourcesPictureStatic()
	:
	CPictureStatic()
{
	m_bMouseInside = FALSE;
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_TB_HAND), RT_GROUP_CURSOR);
	m_hHandCursor = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TB_HAND));
	SetRoot();
}

//-----------------------------------------------------------------------------
void CResourcesPictureStatic::OnLButtonUp(UINT nFlags, CPoint point)
{
	::SetCursor(m_hOldCursor);
	
	ReleaseCapture();
	m_bMouseInside = FALSE;

	// attiva l'apertura del documento autoinviandosi un messaggio, in modo da fare finire le attività
	// legate alla visualizzazione (cambio cursore, ecc.) per non rischiare di inceppare la perdita del fuoco 
	CPictureStatic::OnLButtonUp(nFlags, point);
	OpenResource();
}

//-----------------------------------------------------------------------------
void CResourcesPictureStatic::OnMouseMove(UINT nFlags, CPoint point)
{
	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (m_bMouseInside == bInside)
		return;

	m_bMouseInside = bInside;

	if (m_bMouseInside)
	{
		m_hOldCursor = ::SetCursor(m_hHandCursor);
		SetCapture();
	}
	else
	{
		::SetCursor(m_hOldCursor);
		ReleaseCapture();
	}
	CPictureStatic::OnMouseMove(nFlags, point);
}

//-----------------------------------------------------------------------------
void CResourcesPictureStatic::OpenResource()
{
	if (!m_ResourceType.IsEmpty())
	{
		if (!m_Resource.IsEmpty())
		{
			ADMResourcesObj* pDoc = (ADMResourcesObj*)AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMResourcesObj), szDefaultViewMode, NULL);
			if (pDoc) 
				pDoc->SetResource(m_ResourceType, m_Resource);
		}
	}
	else
	{
		if (!m_Worker.IsEmpty())
		{
			//open worker
			ADMWorkersObj* pDoc = (ADMWorkersObj*)AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMWorkersObj), szDefaultViewMode, NULL);
			if (pDoc) 
				pDoc->SetWorker(m_Worker);
		}
		else
		{
			//open company
			GetDocument()->GetFrame()->SendMessage(WM_COMMAND, ID_RESOURCES_PICTURE_OTHER);
		}

	}

}
