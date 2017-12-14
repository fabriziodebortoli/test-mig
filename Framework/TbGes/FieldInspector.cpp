
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\ParsCtrl.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGenlib\HyperLink.h>
#include <TbGes\BodyEdit.h>
#include <TBOleDB\SqlTable.h>
#include <TbGes\Tabber.h>
#include <TbGes\DBT.h>
#include <TbGes\ParsedPanel.h>
#include <TbGes\TileDialog.h>
#include <TbGenlib\TilePanel.h>
#include <TbGes\TileManager.h>

#include "FieldInspector.h"
#include "FieldInspector.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static char  THIS_FILE[] = __FILE__;
#endif

//==============================================================================
class CInspectorDialog : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CInspectorDialog)

public:
	CInspectorDialog 
		(
			CInspectorDialog*	&pParentRef,
			CWnd*				pWndParent
		);


private:
	CWnd*				m_pWndToTrack;
	CRect				m_rectToTrack;
	CInspectorDialog*	&m_pParentRef;
	CRect				m_rectParent;
	CExtButton			m_btnInspectStartStop;
	BOOL				m_bIsInspecting;
	CExtButton			m_btnCopyNamespace;
	CExtButton			m_btnCopyNameOnDB;
	CExtButton			m_btnCopyInfo;
	CExtButton			m_btnOpenEditor;
	CBCGPEdit			m_edtNamespace;
	CBCGPEdit			m_edtNameOnDB;
	CBCGPEdit			m_edtInfo;
	HCURSOR				m_hViewFinderCursor;
	CJsonResource		m_CurrentResource;

private:
	void			SetFieldInfo		(const CString& aNamespace, const CString& aNameOnDB, const CString& aInfo);
	void			InvertTracker		(CWnd* pWndToTrack, const CRect& rectToTrack, BOOL bInspecting);
	BOOL			IsToBeTracked		(CWnd* pWnd, CPoint ptScreenPoint);
	CParsedCtrl*	CastToParsCtrl		(CWnd* pWnd);
	ColumnInfo*		CastToColumnInfo	(CWnd* pWnd, CPoint ptScreenPoint);
	CTBProperty*	CastToPropertyInfo	(CWnd* pWnd, CPoint ptScreenPoint);
	void			GetTrackInfo		(CWnd* pWndTarget, CPoint ptScreenPoint, CWnd* &pWndToTrack, CRect &rectToTrack);
	ColumnInfo*		GetTrackedColumnInfo(CWnd* pWndTarget, CPoint ptScreenPoint);
	CTBProperty*	GetTrackedPropertyInfo(CWnd* pWndTarget, CPoint ptScreenPoint);

	void	Inspect				(UINT nFlags, CPoint point);
	void	StartInspecting		();
	void	StopInspecting		();

	void	TraceWnd(CString fmt, CWnd* pWnd);
	void	TraceBE(CWnd* pWnd, CPoint ptScreenPoint);

	 int	GetCtrlDataIdx(CParsedCtrl*	pCtrl);
	 void	InspectorGetWndInfo(CWnd* pWnd, CString& sNameSpace, CString& sInfo);
protected:
	virtual BOOL OnInitDialog	();
	virtual void OnCancel		();
	virtual void PostNcDestroy	();

	afx_msg void OnMouseMove			(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp			(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDown			(UINT nFlags, CPoint point);

	afx_msg void OnStartStopClicked		();

	afx_msg void OnCopyNamespaceClicked	();
	afx_msg void OnCopyNameOnDBClicked	();
	afx_msg void OnCopyInfoClicked	();
	afx_msg void OnOpenEditor();

	DECLARE_MESSAGE_MAP ();
};

///////////////////////////////////////////////////////////////////////////////
//						class CInspectorDialog
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC (CInspectorDialog, CLocalizableDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CInspectorDialog, CLocalizableDialog)

	ON_WM_MOUSEMOVE()
	ON_WM_LBUTTONUP()
	//ON_WM_LBUTTONDOWN()

	ON_BN_CLICKED	(IDC_STARTSTOP,				OnStartStopClicked)

	ON_BN_CLICKED	(IDC_COPY_NAMESPACE,		OnCopyNamespaceClicked)
	ON_BN_CLICKED	(IDC_COPY_NAME_ON_DB,		OnCopyNameOnDBClicked)
	ON_BN_CLICKED	(IDC_COPY_INSPECTOR_INFO,	OnCopyInfoClicked)
	ON_BN_CLICKED	(IDC_OPEN_EDITOR,			OnOpenEditor)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CInspectorDialog::CInspectorDialog
	(
		CInspectorDialog* &pParentRef,
		CWnd*		pWndParent
	)
	:
	CLocalizableDialog			(IDD_FIELDINSPECTOR, NULL),
	m_pWndToTrack	(NULL),
	m_pParentRef	(pParentRef),
	m_bIsInspecting	(FALSE)
{
	pWndParent->GetWindowRect(&m_rectParent);
	m_rectToTrack.SetRectEmpty();
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnCancel() 
{
	StopInspecting();
	__super::OnCancel();
}

//-----------------------------------------------------------------------------
BOOL CInspectorDialog::OnInitDialog()
{
	__super::OnInitDialog();

	CRect aRect;
	GetWindowRect(&aRect);
	SetWindowPos(&wndTopMost, m_rectParent.right - aRect.Width(), m_rectParent.bottom - aRect.Height(),0,0,SWP_NOSIZE);
	m_btnInspectStartStop.SubclassDlgItem(IDC_STARTSTOP,this);
	m_btnInspectStartStop.SetPngImages(TBGlyph(szIconStop), TBGlyph(szIconRestart));
	
	m_edtNamespace.SubclassDlgItem(IDC_NAMESPACE,this);
	m_btnCopyNamespace.SubclassDlgItem(IDC_COPY_NAMESPACE,this);
	m_btnCopyNamespace.SetPngImages(TBGlyph(szIconCopy));

	m_edtNameOnDB.SubclassDlgItem(IDC_NAME_ON_DB,this);
	m_btnCopyNameOnDB.SubclassDlgItem(IDC_COPY_NAME_ON_DB,this);
	m_btnCopyNameOnDB.SetPngImages(TBGlyph(szIconCopy));

	m_edtInfo.SubclassDlgItem(IDC_INSPECTOR_INFO,this);
	m_btnCopyInfo.SubclassDlgItem(IDC_COPY_INSPECTOR_INFO,this);
	m_btnCopyInfo.SetPngImages(TBGlyph(szIconCopy));
	m_btnOpenEditor.SubclassDlgItem(IDC_OPEN_EDITOR,this);
	m_btnOpenEditor.SetPngImages(TBGlyph(szIconEdit));

	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_VIEWFINDER), RT_GROUP_CURSOR);
	m_hViewFinderCursor = ::LoadCursor(hInst,MAKEINTRESOURCE(IDC_VIEWFINDER));

	StartInspecting();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CInspectorDialog::PostNcDestroy() 
{
	m_pParentRef = NULL;
	delete this;
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnStartStopClicked()
{
	if (m_bIsInspecting)
		StopInspecting();
	else
		StartInspecting();
}

//-----------------------------------------------------------------------------
void CInspectorDialog::StartInspecting()
{
	SetCapture();
	m_bIsInspecting = TRUE;
	m_btnInspectStartStop.ShowAltImage(FALSE);
	::SetCursor(m_hViewFinderCursor);
}


//-----------------------------------------------------------------------------
void CInspectorDialog::StopInspecting()
{
	if (m_pWndToTrack)
		InvertTracker(m_pWndToTrack, m_rectToTrack, FALSE);	
	m_pWndToTrack = NULL;
	m_rectToTrack.SetRectEmpty();
	ReleaseCapture();
	m_bIsInspecting = FALSE;
	m_btnInspectStartStop.ShowAltImage(TRUE);
	::SetCursor(LoadCursor(NULL,IDC_ARROW));
}

//-----------------------------------------------------------------------------
ColumnInfo* CInspectorDialog::GetTrackedColumnInfo(CWnd* pWndTarget, CPoint ptScreenPoint)
{
	ASSERT(pWndTarget->IsKindOf(RUNTIME_CLASS(CBodyEdit)));
	if (pWndTarget == NULL)
		return NULL;

	CBodyEdit* pBE = (CBodyEdit*)pWndTarget;
	int nCol;
	int xColOffs;
	int nRow;
	int nNewCurrRec;

	CPoint ptClientPoint(ptScreenPoint);
	pWndTarget->ScreenToClient(&ptClientPoint);

	CBodyEdit::CursorPosArea cpa = pBE->GetCursorBodyPos(ptClientPoint, nCol, xColOffs, nRow, nNewCurrRec);

	if (cpa == CBodyEdit::CursorPosArea::IN_BODY || cpa == CBodyEdit::CursorPosArea::IN_COLUMN_TITLE)
		return pBE->GetVisibleColumnFromIdx(nCol);

	return NULL;
}

//-----------------------------------------------------------------------------
CTBProperty* CInspectorDialog::GetTrackedPropertyInfo(CWnd* pWndTarget, CPoint ptScreenPoint)
{
	ASSERT(pWndTarget->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)));
	if (pWndTarget == NULL)
		return NULL;

	CTBPropertyGrid* pPG = (CTBPropertyGrid*)pWndTarget;
	CPoint ptCursor;// = ptScreenPoint;
	::GetCursorPos(&ptCursor);
	pPG->ScreenToClient(&ptCursor);

	return DYNAMIC_DOWNCAST(CTBProperty, pPG->HitTest(ptCursor));
}

//-----------------------------------------------------------------------------
BOOL CInspectorDialog::IsToBeTracked(CWnd* pWnd, CPoint ptScreenPoint)
{
	if (!pWnd)
		return FALSE;

	if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		return GetTrackedColumnInfo(pWnd,ptScreenPoint) != NULL;
	
	if (pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
	{
		CTBProperty* pProp = GetTrackedPropertyInfo(pWnd, ptScreenPoint);
		return pProp != NULL && pProp->GetControl() != NULL;
	}

	return	pWnd->IsKindOf(RUNTIME_CLASS(CParsedEdit)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CParsedStatic)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CParsedButton)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CHyperLink)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CEnumButton)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CChildButton));
}

//-----------------------------------------------------------------------------
CParsedCtrl* CInspectorDialog::CastToParsCtrl(CWnd* pWnd)
{
	if (!pWnd)
		return NULL;

	//if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
	//	return (CParsedCtrl*)((CParsedEdit*)pWnd);
	//if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
	//	return (CParsedCtrl*)((CParsedStatic*)pWnd);
	//if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
	//	return (CParsedCtrl*)((CParsedCombo*)pWnd);
	//if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedButton)))
	//	return (CParsedCtrl*)((CParsedButton*)pWnd);
	//if (pWnd->IsKindOf(RUNTIME_CLASS(CEnumButton)))
	//	return (CParsedCtrl*)((CEnumButton*)pWnd);

	return dynamic_cast<CParsedCtrl*>(pWnd);
}

//-----------------------------------------------------------------------------
ColumnInfo* CInspectorDialog::CastToColumnInfo(CWnd* pWnd, CPoint ptScreenPoint)
{
	if (!pWnd)
		return NULL;

	if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		return GetTrackedColumnInfo(pWnd,ptScreenPoint);

	return NULL;
}

//-----------------------------------------------------------------------------
CTBProperty* CInspectorDialog::CastToPropertyInfo(CWnd* pWnd, CPoint ptScreenPoint)
{
	if (!pWnd)
		return NULL;

	if (pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
		return GetTrackedPropertyInfo(pWnd,ptScreenPoint);

	return NULL;
}

//-----------------------------------------------------------------------------
void CInspectorDialog::GetTrackInfo(CWnd* pWndTarget, CPoint ptScreenPoint, CWnd* &pWndToTrack, CRect &rectToTrack)
{
	if (pWndTarget->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
	{
		CBodyEdit* pBody = (CBodyEdit*)pWndTarget;
		ColumnInfo* pColInfo = GetTrackedColumnInfo(pWndTarget,ptScreenPoint);
		pWndToTrack = pWndTarget;
		pWndToTrack->GetWindowRect(&rectToTrack);
		if (pColInfo)
		{
			int nDummy1, nDummy2, nDummy3;
			int xColOffs;
			CPoint clientPoint(ptScreenPoint);
			pWndTarget->ScreenToClient(&clientPoint);

			CBodyEdit::CursorPosArea cpa = pBody->GetCursorBodyPos(clientPoint,nDummy1,xColOffs,nDummy2,nDummy3);
			
			rectToTrack.top += pBody->m_HeaderToolBar.GetToolBarRect().Height();
			rectToTrack.left += xColOffs;
			rectToTrack.right = min(rectToTrack.right - GetSystemMetrics(SM_CXVSCROLL),rectToTrack.left + pColInfo->GetScreenWidth());
		}
		rectToTrack.bottom -= max(GetSystemMetrics(SM_CYHSCROLL), pBody->m_FooterToolBar.GetToolBarRect().Height());
	}
	else if (pWndTarget->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
	{
		pWndToTrack = pWndTarget;
		pWndToTrack->GetWindowRect(&rectToTrack);

		CTBProperty* pProp = GetTrackedPropertyInfo(pWndTarget,ptScreenPoint);
		if (pProp)
		{
			rectToTrack = pProp->GetRect();
			pWndToTrack->ClientToScreen(rectToTrack);
		}
	}
	else if (pWndTarget->IsKindOf(RUNTIME_CLASS(CHyperLink)))
	{
		pWndToTrack = ((CHyperLink*)pWndTarget)->GetOwnerCtrl()->GetCtrlCWnd();
		pWndToTrack->GetWindowRect(&rectToTrack);
	}
	else if (pWndTarget->IsKindOf(RUNTIME_CLASS(CChildButton)))
	{
		pWndToTrack = ((CChildButton*)pWndTarget)->GetOwnerGroupBtn()->GetCtrlCWnd();
		pWndToTrack->GetWindowRect(&rectToTrack);
	}
	else if (pWndTarget->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
	{
		pWndToTrack = pWndTarget;
		pWndToTrack->GetWindowRect(&rectToTrack);

		CLabelStatic* pLabel = (CLabelStatic*) pWndTarget;
		if (pLabel->m_arVirtualChilds.GetSize())
		{
			for (int i = 0; i < pLabel->m_arVirtualChilds.GetSize(); i++)
			{
				HWND hwChild = pLabel->m_arVirtualChilds.GetAt(i);

				if (!::IsWindow(hwChild))
					continue;

				CRect rectChild;
				CWnd::FromHandle(hwChild)->GetWindowRect(&rectChild);
				if (rectChild.PtInRect(ptScreenPoint))
				{
					CWnd* pChild = CWnd::FromHandle (hwChild);
					if (IsToBeTracked(pChild, ptScreenPoint))
					{
						pWndToTrack = pChild;
						rectToTrack = rectChild;
						break;
					}
				}
			}
		}
	}
	else
	{
		pWndToTrack = pWndTarget;
		pWndToTrack->GetWindowRect(&rectToTrack);
	}
}

//-----------------------------------------------------------------------------
void CInspectorDialog::TraceWnd(CString fmt, CWnd* pWnd)
{
	CString name = _T("<none>");
	if (pWnd)
	{
		CRuntimeClass* pClass = pWnd->GetRuntimeClass();
		if (pClass)
			name = pClass->m_lpszClassName;
	}
	CString strTrace;
	wsprintf(strTrace.GetBufferSetLength(256),fmt,name);
	TRACE1("%s",strTrace);
}

//-----------------------------------------------------------------------------
void CInspectorDialog::TraceBE(CWnd* pWnd, CPoint ptScreenPoint)
{
	if (!pWnd || !pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		return;

	int nCol;
	int xColOffs;
	int nRow;
	int nNewCurrRec;
	CBodyEdit::CursorPosArea posArea;

	CPoint clientPoint(ptScreenPoint);
	pWnd->ScreenToClient(&clientPoint);
	posArea = ((CBodyEdit*)pWnd)->GetCursorBodyPos(clientPoint,nCol,xColOffs,nRow,nNewCurrRec);
	switch (posArea)
	{
		case CBodyEdit::OUT_OF_BODY				: TRACE("[OUT_OF_BODY "); break;
		case CBodyEdit::IN_COLUMN_TITLE			: TRACE("[IN_COLUMN_TITLE "); break;
		case CBodyEdit::IN_BODY					: TRACE("[IN_BODY "); break;
		case CBodyEdit::IN_RESIZE_GRIP			: TRACE("[IN_RESIZE_GRIP "); break;
		case CBodyEdit::IN_NEW_READ_ONLY_ROW	: TRACE("[IN_NEW_READ_ONLY_ROW "); break;
		default: TRACE("[UNKNOWN "); break;
	}
	TRACE2("%d %d ",nCol,xColOffs);
	TRACE2("%d %d]",nRow,nNewCurrRec);
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnLButtonUp(UINT nFlags, CPoint point)
{
	StopInspecting();
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnLButtonDown(UINT nFlags, CPoint point)
{
	ASSERT_TRACE(FALSE, "Il metodo è solo per debugging, non deve essere chiamato");
	if (m_bIsInspecting)
		Inspect(nFlags, point);
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnMouseMove(UINT nFlags, CPoint point)
{
	if (m_bIsInspecting)
		Inspect(nFlags, point);
}

//-----------------------------------------------------------------------------
int CInspectorDialog::GetCtrlDataIdx(CParsedCtrl*	pCtrl)
{
	// try to see if we are inside a row form view
	// in this case lookup on the attached record doesn't work
	CWnd* pRFWAncestor = m_pWndToTrack;
	do
	{ 
		pRFWAncestor = pRFWAncestor->GetParent();
	}
	while	(
				pRFWAncestor != NULL &&		
				!pRFWAncestor->IsKindOf(RUNTIME_CLASS(CRowFormView))		
			);
	SqlRecord* pRec = pCtrl->m_pSqlRecord;
	// found a row form view in the hierarchy
	if (pRFWAncestor != NULL)
	{
		CRowFormView* pRowFW = (CRowFormView*)pRFWAncestor;
		if	(
				pRowFW->GetDBT() &&
				pRowFW->GetDBT()->GetCurrentRow()
			)
			pRec = pRowFW->GetDBT()->GetCurrentRow();
	}
	TRY
	{
		CRadioCombo* pRC = dynamic_cast<CRadioCombo*>(pCtrl);
		DataObj* pData = pCtrl->GetCtrlData();
		if (pRC)
			pData = pRC->GetSelectedData();

		if (!pData || pRec->GetIndexFromDataObj (pData) < 0)
			return -1;
		
		// if lookup fail, it throws an exception
		return pRec->Lookup(pData);
	}
	CATCH (SqlException, e)
	{
			return -1;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void CInspectorDialog::InspectorGetWndInfo(CWnd* pWnd, CString& sNameSpace, CString& sInfo)
{
	if (!pWnd)
		return;

	if (pWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
	{
		CAbstractFormView* pView = (CAbstractFormView*)pWnd;

		sNameSpace = pView->GetNamespace().ToUnparsedString();

		sInfo += L" [wnd=" + CString(pView->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pView->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CRowTabDialog)))
	{
		CRowTabDialog* pTab = (CRowTabDialog*)pWnd;

		sNameSpace = pTab->GetNamespace().ToUnparsedString();

		sInfo += L" [RowTabDlg=" + CString(pTab->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pTab->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTabDialog)))
	{
		CTabDialog* pTab = (CTabDialog*)pWnd;

		sNameSpace = pTab->GetNamespace().ToUnparsedString();

		sInfo += L" [TabDlg=" + CString(pTab->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pTab->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedPanel)))
	{
		CParsedPanel* pPanel = (CParsedPanel*)pWnd;

		sNameSpace = pPanel->GetNamespace().ToUnparsedString();

		sInfo += L"name=" + pPanel->GetFormName();
		sInfo += L" [Panel=" + CString(pPanel->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pPanel->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTileDialog)))
	{
		CTileDialog* pTile = (CTileDialog*)pWnd;

		sNameSpace = pTile->GetNamespace().ToUnparsedString();

		sInfo += L" [TileDlg=" + CString(pTile->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pTile->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTilePanel)))
	{
		CTilePanel* pTile = (CTilePanel*)pWnd;

		//sNameSpace = pTile->GetNamespace().ToUnparsedString();

		sInfo += L" [TilePanel=" + CString(pTile->GetRuntimeClass()->m_lpszClassName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
	{
		CTilePanelTab* pTile = (CTilePanelTab*)pWnd;

		//sNameSpace = pTile->GetNamespace().ToUnparsedString();

		sInfo += L" [TilePanelTab=" + CString(pTile->GetRuntimeClass()->m_lpszClassName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTileManager)))
	{
		CTileManager* pTabber = (CTileManager*)pWnd;

		sNameSpace = pTabber->GetNamespace().ToUnparsedString();

		sInfo += L" [TileManager=" + CString(pTabber->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, pTabber->GetDlgCtrlID());
		sInfo += cwsprintf(L", IDC=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTabManager)))
	{
		CTabManager* pTabber = (CTabManager*)pWnd;

		sNameSpace = pTabber->GetNamespace().ToUnparsedString();

		sInfo += L" [TabManager=" + CString(pTabber->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, pTabber->GetDlgCtrlID());
		sInfo += cwsprintf(L", IDC=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
	{
		CBodyEdit* pBody = (CBodyEdit*)pWnd;

		sNameSpace = pBody->GetNamespace().ToUnparsedString();

		sInfo += L" [wnd=" + CString(pBody->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, pBody->GetDlgCtrlID());
		sInfo += cwsprintf(L", IDC=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
	{
		CTBPropertyGrid* pPG = (CTBPropertyGrid*)pWnd;

		sNameSpace = pPG->GetNamespace().ToUnparsedString();

		sInfo += L" [wnd=" + CString(pPG->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, pPG->GetDlgCtrlID());
		sInfo += cwsprintf(L", IDC=%s", m_CurrentResource.m_strName) + ']';
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
	{
		CParsedDialog* pPanel = (CParsedDialog*)pWnd;

		sNameSpace = pPanel->GetNamespace().ToUnparsedString();

		sInfo += L"name=" + pPanel->GetFormName();
		sInfo += L" [Dialog=" + CString(pPanel->GetRuntimeClass()->m_lpszClassName);
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbResources, pPanel->GetDialogID());
		sInfo += cwsprintf(L", IDD=%s", m_CurrentResource.m_strName) + ']';
	}
	else
		return;

	if (pWnd->GetParent())
	{
		sNameSpace += L"; ";
		sInfo += L"; ";
		InspectorGetWndInfo(pWnd->GetParent(), sNameSpace, sInfo);
	}
}
//-----------------------------------------------------------------------------
void CInspectorDialog::Inspect(UINT nFlags, CPoint point)
{
	CPoint screenPoint(point);
	ClientToScreen(&screenPoint);

	CWnd* pWnd = WindowFromPoint(screenPoint);
	TraceWnd(_T("%s"),pWnd);

	while (pWnd && !IsToBeTracked(pWnd,screenPoint))
	{
		CPoint clientPoint(screenPoint);
		pWnd->ScreenToClient(&clientPoint);
		
		//anomalia 18760, Luca, sostituita la ChildWindowFromPoint con RealChildWindowFromPoint in quanto 
		//in caso il tab index di una groupbox sia più alto di un control contenuto nella groupbox la ChildWindowFromPoint
		//ritorna il controllo topmost; la RealChildWindowFromPoint nel caso delle groupbox invece cerca anche "sotto" 
		//la groupbox
		//CWnd* pChildWnd = pWnd->ChildWindowFromPoint(clientPoint, CWP_ALL | CWP_SKIPINVISIBLE);
		HWND hChild  = ::RealChildWindowFromPoint(pWnd->m_hWnd, clientPoint);
		CWnd* pChildWnd = CWnd::FromHandle (hChild);
		if (pChildWnd == pWnd)
		{
			//if (nFlags & )
			break;
		}
		pWnd = pChildWnd;
		TraceWnd(_T("->%s"),pWnd);
	}
	TraceBE(pWnd,screenPoint);

	TRACE("\n");
	if (!IsToBeTracked(pWnd,screenPoint))
	{
		if (m_pWndToTrack)
			InvertTracker(m_pWndToTrack, m_rectToTrack, FALSE);
		m_pWndToTrack = NULL;
		m_rectToTrack.SetRectEmpty();
		::SetCursor(m_hViewFinderCursor);
	}
	if (IsToBeTracked(pWnd,screenPoint))
	{
		CWnd* pNewWnd;
		CRect rectNewRect;
		GetTrackInfo(pWnd,screenPoint,pNewWnd,rectNewRect);
		if (pNewWnd && (!m_pWndToTrack || pNewWnd->m_hWnd != m_pWndToTrack->m_hWnd || rectNewRect != m_rectToTrack))
		{
			if (m_pWndToTrack)
				InvertTracker(m_pWndToTrack, m_rectToTrack, FALSE);
			m_pWndToTrack = pNewWnd;
			m_rectToTrack = rectNewRect;
			InvertTracker(m_pWndToTrack, m_rectToTrack, TRUE);
			::SetCursor(LoadCursor(NULL,IDC_HAND));
		}
	}
	
	CParsedCtrl*	pCtrl = NULL;
	ColumnInfo*		pColInfo;
	CTBProperty*	pProp;
	CString			sNameSpace;
	CString			sNameOnDB;
	CString			sInfo;
	CAbstractFormDoc* pDoc = NULL;

	if (pProp = CastToPropertyInfo(m_pWndToTrack, screenPoint))
	{
		ASSERT_VALID(pProp);
		CTBPropertyGrid* pPG = (CTBPropertyGrid*)m_pWndToTrack;
		ASSERT_VALID(pPG);

		if (pPG->GetDocument() && pPG->GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			pDoc = (CAbstractFormDoc*) pPG->GetDocument();
			ASSERT_VALID(pDoc);
		}

		pCtrl = pProp->GetControl();	// lo usa il flusso normale su pCtrl
	}

	if (pCtrl != NULL || (pCtrl = CastToParsCtrl(m_pWndToTrack)) != NULL)
	{
		sNameSpace = pCtrl->GetNamespace().ToUnparsedString();

		if (pCtrl->GetDocument() && pCtrl->GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			pDoc = (CAbstractFormDoc*) pCtrl->GetDocument();
		}

		if (pCtrl->m_pSqlRecord)
		{
			int nIdx = GetCtrlDataIdx(pCtrl);
			TRY
			{
				if	(
						nIdx != -1 &&
						!pCtrl->m_pSqlRecord->GetColumnInfo(nIdx)->m_bVirtual
					)
				{
					sNameOnDB = pCtrl->m_pSqlRecord->GetTableName() + '.' + pCtrl->m_pSqlRecord->GetColumnName(nIdx);

					if (pDoc)
					{
						DBTObject* pDBT = pDoc->GetDBTObject(pCtrl->m_pSqlRecord);
						if (pDBT)
						{
							ASSERT_VALID(pDBT);
							sNameOnDB = pDBT->GetNamespace().GetObjectName() + '.' + sNameOnDB;
							if (pDoc->GetSymTable())
							{
								SymField* pF = pDoc->GetSymTable()->GetField(pDBT->GetNamespace().GetObjectName() + '.' + pCtrl->m_pSqlRecord->GetColumnName(nIdx));
								if (pF)
									sInfo = pF->GetTag();
							}
						}
					}
				}
			}
			CATCH (SqlException, e)
			{
			}
			END_CATCH
		}
		else
		{
			if	(pDoc && pDoc->m_pHotKeyLinks)	
			{
				for (int i = 0; i < pDoc->m_pHotKeyLinks->GetUpperBound(); i++)
				{
					int nIdx;
					HotKeyLink* pHKL = (HotKeyLink*)pDoc->m_pHotKeyLinks->GetAt(i);
					if (pHKL && pHKL->GetAttachedRecord() && pHKL->GetAttachedRecord()->IsPresent(pCtrl->GetCtrlData(),nIdx))
					{
						sNameOnDB = pHKL->GetAttachedRecord()->GetTableName() +'.' + pHKL->GetAttachedRecord()->GetColumnName(nIdx);
						break;
					}
				}
			}
		}
		if (sNameOnDB.IsEmpty())
			sNameOnDB = _TB("(local only)");
		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, m_pWndToTrack->GetDlgCtrlID());
		sInfo += cwsprintf(L" [IDC=%s", m_CurrentResource.m_strName);
		if (pCtrl->GetCtrlData())
			sInfo += L", type=" + pCtrl->GetCtrlData()->GetDataType().ToString();
		sInfo += L", ctrl=" + CString(m_pWndToTrack->GetRuntimeClass()->m_lpszClassName);
		if (pCtrl->GetHotLink())
			sInfo += L", hkl=" + pCtrl->GetHotLink()->GetNamespace().ToUnparsedString();
		sInfo += ']';
	}
	else if (pColInfo = CastToColumnInfo(m_pWndToTrack, screenPoint))
	{
		ASSERT_VALID(pColInfo);
		CBodyEdit* pBE = (CBodyEdit*)m_pWndToTrack;
		ASSERT_VALID(pBE);

		if (pBE->GetDocument() && pBE->GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			pDoc = (CAbstractFormDoc*) pBE->GetDocument();
			ASSERT_VALID(pDoc);
		}

		sNameSpace = pColInfo->GetNamespace().ToUnparsedString();
		int nIdx = pColInfo->GetDataInfoIdx();
		if (nIdx != -1)
		{
			if (
					pBE->GetDBT() && pBE->GetDBT()->GetRecord() && pBE->GetDBT()->GetRecord()->GetColumnInfo(nIdx) &&
					! pBE->GetDBT()->GetRecord()->GetColumnInfo(nIdx)->m_bVirtual
				)
			{
				sNameOnDB = pBE->GetDBT()->GetNamespace().GetObjectName() + '.' +
							pBE->GetDBT()->GetTable()->GetTableName() + '.' + 
							pBE->GetDBT()->GetRecord()->GetColumnName(nIdx);

				if (pDoc && pDoc->GetSymTable())
				{
					SymField* pF = pDoc->GetSymTable()->GetField(pBE->GetDBT()->GetNamespace().GetObjectName() + '.' + pBE->GetDBT()->GetRecord()->GetColumnName(nIdx));
					if (pF)
						sInfo = pF->GetTag();
				}
			}
			else
				sNameOnDB = _TB("(local only)");
		}
		else
			sNameOnDB = _TB("(unknown)");

		m_CurrentResource = AfxGetTBResourcesMap()->DecodeID(TbControls, pColInfo->GetCtrlID());
		sInfo += cwsprintf(L" [IDC=%s", m_CurrentResource.m_strName);
		if (pColInfo->GetBaseDataObj())
			sInfo += L", type=" + pColInfo->GetBaseDataObj()->GetDataType().ToString();
		sInfo += L", ctrl=" + CString(pColInfo->GetControl()->GetRuntimeClass()->m_lpszClassName);
		if (pColInfo->GetParsedCtrl() && pColInfo->GetParsedCtrl()->GetHotLink())
			sInfo += L", hkl=" + pColInfo->GetParsedCtrl()->GetHotLink()->GetNamespace().ToUnparsedString();
		sInfo += ']';
	}
	else if (pWnd)
	{
		InspectorGetWndInfo(pWnd, sNameSpace, sInfo);
	}

	SetFieldInfo(sNameSpace,sNameOnDB, sInfo);
}

//-----------------------------------------------------------------------------
void CInspectorDialog::InvertTracker(CWnd* pWndToTrack, const CRect& rectToTrack, BOOL bInspecting)
{
	int cxBorder = GetSystemMetrics(SM_CXBORDER);
	CDC*	pDC;
	CRect	rectWnd;
	pDC = pWndToTrack->GetWindowDC();
	pWndToTrack->GetWindowRect(&rectWnd);
	pDC->SetROP2(R2_NOT);
	COLORREF cr = AfxGetThemeManager()->GetFieldInspectorHighlightForeColor();
	CPen* pPen = new CPen(PS_INSIDEFRAME,3 * cxBorder, cr);
	CPen* pOldPen =	pDC->SelectObject(pPen);
	CGdiObject* pOldBrush = pDC->SelectStockObject(NULL_BRUSH); 
	pDC->Rectangle
		(
			rectToTrack.left - rectWnd.left,
			rectToTrack.top - rectWnd.top,
			rectToTrack.right - rectWnd.left, 
			rectToTrack.bottom - rectWnd.top
		);

	pDC->SelectObject(pOldPen);
	pDC->SelectObject(pOldBrush);
	pWndToTrack->ReleaseDC(pDC);
	delete pPen;

	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWndToTrack);
	if (pGrid)
		pGrid->SetFieldInspecting(bInspecting);
}

//-----------------------------------------------------------------------------
void CInspectorDialog::SetFieldInfo (const CString& aNamespace, const CString& aNameOnDB, const CString& aInfo)
{
	m_edtNamespace.SetWindowText(aNamespace);
	m_edtNameOnDB.SetWindowText(aNameOnDB);
	m_edtInfo.SetWindowText(aInfo);
	m_btnOpenEditor.EnableWindow(!m_CurrentResource.m_strContext.IsEmpty());
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnCopyNamespaceClicked()
{
	m_edtNameOnDB.SetSel(0,0);
	m_edtInfo.SetSel(0,0);

	m_edtNamespace.SetSel(0,-1);
	m_edtNamespace.Copy();
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnCopyNameOnDBClicked()
{
	m_edtNamespace.SetSel(0,0);
	m_edtInfo.SetSel(0,0);

	m_edtNameOnDB.SetSel(0,-1);
	m_edtNameOnDB.Copy();
}

//-----------------------------------------------------------------------------
void CInspectorDialog::OnCopyInfoClicked()
{
	m_edtNamespace.SetSel(0,0);
	m_edtNameOnDB.SetSel(0,0);

	m_edtInfo.SetSel(0,-1);
	m_edtInfo.Copy();
}
//-----------------------------------------------------------------------------
void CInspectorDialog::OnOpenEditor()
{
	CString sFile;
	CTBNamespace ns;
	m_CurrentResource.GetInfo(sFile, ns);
	if (ExistFile(sFile))
		AfxGetTbCmdManager()->RunDocument(_T("Document.Extensions.EasyBuilder.TbEasyBuilder.EasyStudioDesigner"), szDefaultViewMode, FALSE, NULL, (LPAUXINFO)(LPCTSTR)sFile);
}
//////////////////////////////////////////////////////////////////////////////
//				CDFieldInspector
//////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CDFieldInspector, CClientDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDFieldInspector, CClientDoc)
	//{{AFX_MSG_MAP(CDPublishingTaxSaleDoc)

	ON_BN_CLICKED			(ID_INSPECT,		OnInspect)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDFieldInspector::CDFieldInspector()
	: 
	m_pInspectorDialog(NULL)
{
}

//-----------------------------------------------------------------------------
CDFieldInspector::~CDFieldInspector()
{
	if (m_pInspectorDialog)
	{
		m_pInspectorDialog->DestroyWindow();
		m_pInspectorDialog = NULL;
	}
}

//-----------------------------------------------------------------------------
void CDFieldInspector::OnInspect()
{
	if (!GetMasterDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return;
	}

	if (!m_pInspectorDialog || !::IsWindow(m_pInspectorDialog->m_hWnd))
	{
		m_pInspectorDialog = new CInspectorDialog(m_pInspectorDialog,((CAbstractFormDoc*)GetMasterDocument())->GetMasterFrame());
		if (m_pInspectorDialog->Create(IDD_FIELDINSPECTOR))
			m_pInspectorDialog->ShowWindow (SW_SHOW);
		else
		{
			m_pInspectorDialog->DestroyWindow();
			delete m_pInspectorDialog;
			m_pInspectorDialog = NULL;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CDFieldInspector::OnAttachData()
{
	const TCHAR* szFieldInspector = _T("FieldInspector");
	if (AfxGetLoginInfos()->m_bAdmin)
	{
		m_pServerDocument->DeclareVariable(szFieldInspector, new DataBool(TRUE), TRUE, TRUE);
	}
	else if (AfxGetSecurityInterface()->IsSecurityEnabled())
	{
		CTBNamespace ns(CTBNamespace::FUNCTION, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.IsUserReportsDeveloper"));
		CInfoOSL infoOSL(ns, OSLType_Function);
		AfxGetSecurityInterface()->GetObjectGrant(&infoOSL);

		if (OSL_IS_PROTECTED(&infoOSL) && OSL_CAN_DO(&infoOSL, OSL_GRANT_EXECUTE))
		{
			m_pServerDocument->DeclareVariable(szFieldInspector, new DataBool(TRUE), TRUE, TRUE);
		}
	}
	return __super::OnAttachData();
}

//------------------------------------------------------------------------------
WebCommandType CDFieldInspector::OnGetWebCommandType(UINT commandID)
{
	if (commandID == ID_INSPECT)
		return WEB_UNSUPPORTED;
	return __super::OnGetWebCommandType(commandID);
}

