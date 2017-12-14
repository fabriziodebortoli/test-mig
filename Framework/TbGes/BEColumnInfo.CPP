
#include "stdafx.h"

#include <atlimage.h>

#include <TbGeneric\globals.h>  
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\spin.h>
#include <TBGeneric\WndObjDescription.h>
#include <TBGeneric\visualstylesxp.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\HyperLink.h>
#include <TbGenlib\ParsObj.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include "formmng.h"

#include "bodyedit.h"

#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include"begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define	ISO_COUNTRY_CODE_BRAZIL	_T("BR")

class CFooterCalculator: public CDataEventsObj
{
	CBodyEdit* m_pEdit;
	ColumnInfo* m_pInfo;
	DataObj* m_pDataObj;

	friend class ColumnInfo;

public:
	CFooterCalculator(ColumnInfo* pInfo, CBodyEdit* pEdit) 
		:
	m_pEdit(pEdit), 
	m_pInfo(pInfo)
	{
		m_pDataObj = m_pInfo->m_pBaseDataObj->Clone();
		m_bOwned = TRUE;
	}
	~CFooterCalculator() { delete m_pDataObj; }

	virtual void Signal(CObservable* pSender, EventType eType)
	{
		ASSERT_VALID(m_pEdit);
		ASSERT_VALID(m_pInfo);
		ASSERT_VALID(m_pDataObj);

		if (eType == ON_CHANGED)
		{
			m_pEdit->OnCalcFooter(m_pInfo, m_pDataObj);
		}
	}
	virtual void Fire(CObservable* pSender, EventType eType)
	{
	}
	//virtual void Signal(CObservable* pSender, EventType eType);
	virtual CObserverContext* GetContext() const { return NULL; }

};

////////////////////////////////////////////////////////////////////////////////
//				class ColumnsInfoArray implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
int	ColumnsInfoArray::GetColumnIdx(ColumnInfo* pInfo)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) == pInfo)
			return i;
	return -1;
}

//-----------------------------------------------------------------------------
void ColumnsInfoArray::Add (ColumnInfo* pInfo)
{
	int nIdx = GetColumnIdx(pInfo);
	if (nIdx < 0)
		__super::Add (pInfo);
}

//-----------------------------------------------------------------------------
void ColumnsInfoArray::Remove (ColumnInfo* pInfo)
{
	int nIdx = GetColumnIdx(pInfo);
	if (nIdx >= 0)
		__super::RemoveAt (nIdx);
}

//-----------------------------------------------------------------------------
void ColumnsInfoArray::MoveTo (ColumnInfo* pInfo, const int& nIdx)
{
	int nPrevIdx = GetColumnIdx(pInfo);
	if (nPrevIdx < 0)
		return;
	
	BOOL bOldOwns = m_bOwnsElements;
	SetOwns (FALSE);
	RemoveAt(nPrevIdx);
	if (nIdx > GetUpperBound())
		Add(pInfo); 
	else
		InsertAt(nIdx, pInfo);
	
	SetOwns (bOldOwns);
}

////////////////////////////////////////////////////////////////////////////////
//				class CBodyBitmapButton implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBodyBitmapButton, CBitmapButton)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBodyBitmapButton, CBitmapButton)
	//{{AFX_MSG_MAP(CBodyBitmapButton)
	ON_WM_LBUTTONDOWN()
	ON_WM_MOUSEMOVE()
	ON_WM_LBUTTONUP()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBodyBitmapButton::CBodyBitmapButton(UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip)
	:
	CBitmapButton			(),
	m_nButtonIDC			(nIDC),
	m_nButtonIDBU			(nIDBU),
	m_nButtonIDBD			(nIDBD),
	m_nButtonIDBDI			(nIDBDI),
	m_bButtonEnabled		(FALSE),
	m_bDisableOnReadOnly	(FALSE),
	m_strToolTip			(szToolTip)
{}

//-----------------------------------------------------------------------------
CBodyBitmapButton::CBodyBitmapButton(UINT nIDC, const CString& sIDBU, const CString& sIDBD, const CString& sIDBDI, LPCTSTR szToolTip)
	:
	CBitmapButton			(),
	m_nButtonIDC			(nIDC),
	m_sButtonIDBUName		(sIDBU),
	m_sButtonIDBDName		(sIDBD),
	m_sButtonIDBDIName		(sIDBDI),
	m_bButtonEnabled		(FALSE),
	m_bDisableOnReadOnly	(FALSE),
	m_strToolTip			(szToolTip)
{
}
//-----------------------------------------------------------------------------
void CBodyBitmapButton::EnableButton(BOOL bBodyState)
{
	EnableWindow(m_bButtonEnabled && !(!bBodyState && m_bDisableOnReadOnly));
}

//-----------------------------------------------------------------------------
void CBodyBitmapButton::SetEnableState(BOOL bEnable, BOOL bDisableOnReadOnly)
{
	m_bButtonEnabled = bEnable;
	m_bDisableOnReadOnly = bDisableOnReadOnly;
}

//-----------------------------------------------------------------------------
BOOL CBodyBitmapButton::Create(LPCTSTR psz, DWORD style, const RECT& rect, CWnd* pParent)
{
	if (!CBitmapButton::Create(psz, style, rect, pParent, m_nButtonIDC))
	{
		TRACE1("CBodyBitmapButton::Create: failed to create USER button %d\n", m_nButtonIDC);
		ASSERT(FALSE);
		return FALSE;
	}

	ASSERT(m_nButtonIDBU && m_nButtonIDBD && m_nButtonIDBDI && m_sButtonIDBDIName.IsEmpty() && m_sButtonIDBDName.IsEmpty() && m_sButtonIDBUName.IsEmpty());

	if (!m_sButtonIDBDIName.IsEmpty())
		LoadBitmaps (m_sButtonIDBUName, m_sButtonIDBDName, 0, m_sButtonIDBDIName);
	else
		LoadBitmaps (m_nButtonIDBU, m_nButtonIDBD, 0, m_nButtonIDBDI);

	return TRUE;
}
				
//-----------------------------------------------------------------------------
void CBodyBitmapButton::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{       
	ASSERT(lpDIS != NULL);
	// must have at least the first bitmap loaded before calling DrawItem
	ASSERT(m_bitmap.m_hObject != NULL);     // required

	// use the main bitmap for up, the selected bitmap for down
	CBitmap* pBitmap = &m_bitmap;

	if (GetParent()->IsWindowEnabled())
	{
		UINT state = lpDIS->itemState;
		if ((state & ODS_SELECTED) && m_bitmapSel.m_hObject != NULL)
			pBitmap = &m_bitmapSel;
		else if ((state & ODS_FOCUS) && m_bitmapFocus.m_hObject != NULL)
			pBitmap = &m_bitmapFocus;   // third image for focused
		else if ((state & ODS_DISABLED) && m_bitmapDisabled.m_hObject != NULL)
			pBitmap = &m_bitmapDisabled;   // last image for disabled
	}
	else
	{
		if (m_bitmapDisabled.m_hObject != NULL)
			pBitmap = &m_bitmapDisabled;   
	}

	BITMAP bmInfo;
	if (pBitmap->GetObject(sizeof(bmInfo), &bmInfo) != sizeof(bmInfo))
		return;

	// draw the whole button
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	CDC memDC;
	memDC.CreateCompatibleDC(pDC);
	CBitmap* pOld = memDC.SelectObject(pBitmap);
	if (pOld == NULL)
		return;     // destructors will clean up

	CRect rect(lpDIS->rcItem);
	
	// draw the icon contents
	pDC->StretchBlt
		(	
			rect.left, rect.top, rect.Width(), rect.Height(),
			&memDC, 0, 0, bmInfo.bmWidth, bmInfo.bmHeight,
			SRCCOPY
		);

	memDC.SelectObject(pOld);
	memDC.DeleteDC();
}

//-----------------------------------------------------------------------------
void CBodyBitmapButton::OnLButtonDown(UINT, CPoint)
{
	SetCapture();
	SetState(TRUE);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//-----------------------------------------------------------------------------
void CBodyBitmapButton::OnMouseMove(UINT, CPoint point)
{
	if (GetCapture() != this)
		return;     // not captured                      
		
	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (((GetState() & 0x0004) != NULL) == bInside)
		return;
		
	SetState(bInside);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//-----------------------------------------------------------------------------
void CBodyBitmapButton::OnLButtonUp(UINT, CPoint point)
{
	if (GetCapture() != this)
		return;     // not captured                      
		
	ReleaseCapture();
	SetState(FALSE);
	Invalidate();
	UpdateWindow(); // immediate feedback

	CRect rect;
	GetClientRect(rect);
	
	if (!rect.PtInRect(point))
		return;

	if (CBodyEdit::IsFocusCommand(m_nButtonIDC))
		GetParent()->SetFocus();

	GetParent()->SEND_WM_COMMAND(GetDlgCtrlID(), 0, m_hWnd);
}

//-----------------------------------------------------------------------------
BOOL CBodyBitmapButton::LoadWalkBitmap
		(
			CBitmap&	bitmapResource,
			UINT		nBitmapResource
		)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nBitmapResource), RT_BITMAP);
	return bitmapResource.Attach(::LoadBitmap(hInst, MAKEINTRESOURCE(nBitmapResource)));
#else
	return bitmapResource.LoadBitmap(MAKEINTRESOURCE(nBitmapResource));
#endif
}

//-----------------------------------------------------------------------------
BOOL CBodyBitmapButton::LoadWalkBitmap
		(
			CBitmap&		bitmapResource,
			const CString&	sBitmapResource
		)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(sBitmapResource, RT_BITMAP);
	return bitmapResource.Attach(::LoadBitmap(hInst, sBitmapResource));
#else
	return bitmapResource.LoadBitmap(sBitmapResource);
#endif
}

//-----------------------------------------------------------------------------
BOOL CBodyBitmapButton::LoadBitmaps
		(
			UINT nBitmapResource,
			UINT nBitmapResourceSel,
			UINT nBitmapResourceFocus,		/* 0 */
			UINT nBitmapResourceDisabled	/* 0 */
		)
{
	// delete old bitmaps (if present)
	m_bitmap.DeleteObject();
	m_bitmapSel.DeleteObject();
	m_bitmapFocus.DeleteObject();
	m_bitmapDisabled.DeleteObject();

	if (!LoadWalkBitmap(m_bitmap, nBitmapResource))
	{
		TRACE0("CBodyBitmapButton::LoadBitmaps: failed to load bitmap for normal image\n");
		return FALSE;   // need this one image
	}
	BOOL bAllLoaded = TRUE;
	if (nBitmapResourceSel != 0)
	{
		if (!LoadWalkBitmap(m_bitmapSel, nBitmapResourceSel))
		{
			TRACE0("CBodyBitmapButton::LoadBitmaps: failed to load bitmap for selected image\n");
			bAllLoaded = FALSE;
		}
	}
	if (nBitmapResourceFocus != 0)
	{
		if (!LoadWalkBitmap(m_bitmapFocus, nBitmapResourceFocus))
			bAllLoaded = FALSE;
	}
	if (nBitmapResourceDisabled != 0)
	{
		if (!LoadWalkBitmap(m_bitmapDisabled, nBitmapResourceDisabled))
			bAllLoaded = FALSE;
	}

	Invalidate();
	return bAllLoaded;
}

//-----------------------------------------------------------------------------
BOOL CBodyBitmapButton::LoadBitmaps
		(
			const CString& sBitmapResource,
			const CString& sBitmapResourceSel,
			const CString& sBitmapResourceFocus,	/* "" */
			const CString& sBitmapResourceDisabled	/* "" */
		)
{
	// delete old bitmaps (if present)
	m_bitmap.DeleteObject();
	m_bitmapSel.DeleteObject();
	m_bitmapFocus.DeleteObject();
	m_bitmapDisabled.DeleteObject();

	if (!LoadWalkBitmap(m_bitmap, sBitmapResource))
	{
		TRACE0("CBodyBitmapButton::LoadBitmaps: failed to load bitmap for normal image\n");
		return FALSE;   // need this one image
	}
	BOOL bAllLoaded = TRUE;
	if (!sBitmapResourceSel.IsEmpty())
	{
		if (!LoadWalkBitmap(m_bitmapSel, sBitmapResourceSel))
		{
			TRACE0("CBodyBitmapButton::LoadBitmaps: failed to load bitmap for selected image\n");
			bAllLoaded = FALSE;
		}
	}
	if (!sBitmapResourceFocus.IsEmpty())
	{
		if (!LoadWalkBitmap(m_bitmapFocus, sBitmapResourceFocus))
			bAllLoaded = FALSE;
	}
	if (!sBitmapResourceDisabled.IsEmpty())
	{
		if (!LoadWalkBitmap(m_bitmapDisabled, sBitmapResourceDisabled))
			bAllLoaded = FALSE;
	}

	Invalidate();
	return bAllLoaded;
}

// Diagnostics

#ifdef _DEBUG
void CBodyBitmapButton::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCBodyBitmapButton");
}

void CBodyBitmapButton::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//-----------------------------------------------------------------------------
ColumnInfoStateCtrlData::ColumnInfoStateCtrlData (int nDataInfo, BOOL bInvertStates)
	:
	m_nDataInfoIdx			(nDataInfo),
	m_bInvertDefaultState	(bInvertStates),

	m_pDataObj				(NULL),

	m_bIsCommandBtn			(FALSE),
	m_bSetColorInEditableState (FALSE),
	m_crTextColor			(0),
	m_crBkgColor			(AfxGetThemeManager()->GetBackgroundColor())
{
}

////////////////////////////////////////////////////////////////////////////////
//				class ColumnInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
ColumnInfo::ColumnInfo ()
	:
	IOSLObjectManager		(OSLType_BodyEditColumn),
	m_pBody					(NULL),
	m_nDataInfoIdx			(-1),
	m_pBaseDataObj			(NULL),
	m_nColPos				(-1),
	m_pParsedCtrl			(NULL),
	m_dwCtrlStyle			(0),
	m_nCtrlIDC				(0),
	m_nCtrlLen				(-1),
	m_nCtrlRows				(1),
	m_nColScrnWidth			(0),
	m_nDefaultColScreenWidth(0),
	m_nDefaultCtrlLen		(0),
	m_nDefaultCtrlRows		(0),
	m_nColCharWidth			(0),
	m_wColumnStatus			(0),
	m_nAlignFlags			(TA_LEFT),
	m_pCheckBitmap			(NULL),
	m_nTitleExtraRSpace		(0),
	m_bShowExtraRSpace		(FALSE),
	m_Ordering				(NONE),
	m_nTitleIDB				(0),
	m_crBkgColor			(0xFF << 24),
	m_crTextColor			(0xFF << 24),
	m_bVisible				(TRUE),
	m_nTitleWidth			(0),
	m_nDataScrnWidth		(0),
	m_nCustomScrnWidth		(0),
	m_eWidthType			(WT_DATA),
	m_nHorzColOffset		(0),
	m_nHorzCtrlOffset		(0),
	m_pFooterCalculator		(NULL),
	m_pDynamicDecFmt		(NULL),
	m_pAddressComp			(NULL),
	m_pComponent			(NULL),
	m_bUseOtherColumnCtrl	(FALSE),
	m_bAllowEnlarge			(FALSE)
{
}

//-----------------------------------------------------------------------------
ColumnInfo::ColumnInfo
	(
		CBodyEdit*		pBody, 
		LPCTSTR			lpszTitle, 
		int				nDataInfoIdx,
		CParsedCtrl*	pCtrl, 
		DWORD			dwStyle, 
		UINT			nIDC
	)
	:
	IOSLObjectManager	(OSLType_BodyEditColumn),
	m_pBody				(pBody),
	m_strTitle			(lpszTitle),
	m_nDataInfoIdx		(nDataInfoIdx),
	m_pParsedCtrl		(pCtrl),
	m_dwCtrlStyle		(dwStyle),
	m_nCtrlIDC			(nIDC),
	m_nCtrlLen			(-1),
	m_nCtrlRows			(1),
	m_nColScrnWidth		(0),
	m_nDefaultColScreenWidth	(0),
	m_nDefaultCtrlLen			(0),	//@@GERMANO
	m_nDefaultCtrlRows			(0),	//@@GERMANO
	m_nColCharWidth		(0),
	m_wColumnStatus		(0),
	m_nAlignFlags		(TA_LEFT),
	m_pCheckBitmap		(NULL),
	m_nTitleExtraRSpace	(0),
	m_bShowExtraRSpace	(FALSE),
	m_Ordering			(NONE),
	m_nTitleIDB			(0),
	m_crBkgColor		(0xFF << 24),
	m_crTextColor		(0xFF << 24),
	m_bVisible			(TRUE),
	m_nTitleWidth		(0),
	m_nDataScrnWidth	(0),
	m_nCustomScrnWidth	(0),
	m_eWidthType		(WT_DATA),
	m_nColPos			(-1),
	m_nHorzColOffset	(0),
	m_nHorzCtrlOffset	(0),
	m_pFooterCalculator	(NULL),
	m_pComponent		(NULL),
	m_pAddressComp		(NULL),
	m_pDynamicDecFmt	(NULL),
	m_bUseOtherColumnCtrl(FALSE),
	m_bAllowEnlarge(FALSE)
{
	m_strTitle.Remove(CR_CHAR);

	ASSERT(pCtrl);                            
	m_pBaseDataObj	= pCtrl->GetCtrlData();

	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		m_pCheckBitmap = new CCheckBitmap(TBGlyph(szIconCheck));
}

//-----------------------------------------------------------------------------
ColumnInfo::~ColumnInfo	()
{
	if (m_pFooterCalculator)
	{
		m_pBaseDataObj->DetachEvents(m_pFooterCalculator);//farà lui la delete
		SAFE_DELETE(m_pFooterCalculator);
	}
	SAFE_DELETE(m_pParsedCtrl);
	SAFE_DELETE(m_pCheckBitmap);
	SAFE_DELETE(m_pDynamicDecFmt);
	SAFE_DELETE(m_pAddressComp);
	SAFE_DELETE(m_pComponent);
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetTitleIDB (UINT nTitleIDB)
{ 
	m_nTitleIDB = nTitleIDB; 
	
	if (m_nTitleIDB == 0 || (m_pBody && m_pBody->m_pImageList))
		return;

	BITMAP bmIconInfo;

	if 
		( 
			! m_bmpTitle.LoadBitmap(m_nTitleIDB) 
		||
			! (m_bmpTitle.GetObject(sizeof(bmIconInfo), &bmIconInfo) == sizeof(bmIconInfo))
		)
	{
		m_nTitleIDB = 0;
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetStatus(WORD nStatus)
{
	m_wColumnStatus = nStatus;

	if(m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CStatic)))
		m_wColumnStatus	|= STATUS_NOCHANGE_GRAYED | STATUS_GRAYED;
}

//-----------------------------------------------------------------------------
CString	ColumnInfo::GetToolTip ()
{ 
	CString str;
	
	if (m_strToolTip.IsEmpty())
	{
		if (
				(m_pBody->m_DataTipStyle & CBodyEdit::BE_DTSTYLE_HEADER) &&
				m_nTitleWidth > m_nColScrnWidth
			)
			str = m_strTitle;
	}
	else if (m_pBody->m_DataTipStyle & (CBodyEdit::BE_DTSTYLE_HEADER | CBodyEdit::BE_DTSTYLE_CUSTOM))
		str = m_strToolTip;

	str.Replace(_T("\r\n"), _T(" "));
	str.Replace(_T("\n"), _T(" "));
	return str;
}

// Il parametro nTextLimit serve per limitare il no. di caratteri inputabili
// nell'edit, e viene memorizzato in un data-member di classe poiche`, dato che questa
// funzione deve essere chiamata subito dopo l'AddColumn (vedi ASSERT su m_hWnd),
// l'oggetto windows associato al ParsedCtrl non e` ancora nato (nasce nella
// CBodyEdit::OnCreateClient() chiamata da CBodyEdit::SubclassEdit()/CBodyEdit::OnCreate())
// e quindi non puo` essere fatta ancora una ParsedCtrl::SetCtrlMaxLen(nLen, TRUE) che ad
// esempio per il CParsedEdit esegue la CEdit::LimitText()
//-----------------------------------------------------------------------------
void ColumnInfo::SetCtrlSize(int nChars, int nRows, int nTextLimit)
{           
	ASSERT(nChars > 0);
	ASSERT(nRows > 0);

	if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
	{
		m_nCtrlLen = 0;
		m_nCtrlRows = 1;
		return;
	}

	// deve essere fatta prima della create della window poiche` il ParsedCombo possa
	// dimensionare la tendina in maniera opportuna (la SetCtrlSize che verra fatta nella
	// ResizeColumns interviene quando ormai window ha, per esempio, gia` deciso se dopo
	// quanti items far apparire la scrollbar vericale della tendina
	if (nRows > 1 && m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		((CParsedCombo*) m_pParsedCtrl)->SetMaxRows(nRows);

	// Assegna il default della limitazione di input (verra` resa operativa dalla
	// specifica OnInitCtrl() chiamata dalla Create)
	//
	if (nRows > 1 && m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
		m_dwCtrlStyle |= ES_WANTRETURN | ES_MULTILINE | ES_AUTOVSCROLL;

	if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CTextEdit)))
		m_dwCtrlStyle |= WS_VSCROLL;	
	
	if (nTextLimit <= 0 && m_nCtrlLen < 0)	nTextLimit = nChars;

	m_nDefaultCtrlLen = m_nCtrlLen = nChars;	//@@GERMANO
	m_nDefaultCtrlRows	= m_nCtrlRows	= nRows;	//@@GERMANO

	if (nTextLimit > 0)
		m_pParsedCtrl->SetCtrlMaxLen(nTextLimit, FALSE);
}                          

//-----------------------------------------------------------------------------
void ColumnInfo::SetColWidth(int nChars)
{               
	m_nColCharWidth	= nChars;
}                          

//-----------------------------------------------------------------------------
BOOL ColumnInfo::CreateCtrl(CBodyEdit* pWndOwner)
{
	ASSERT(pWndOwner->m_hWnd);
	ASSERT(m_pParsedCtrl);
	
	ASSERT(pWndOwner->GetParent()->GetDlgItem(m_nCtrlIDC) == NULL);

	if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		m_dwCtrlStyle &= ~WS_BORDER;

	DWORD style = m_dwCtrlStyle;
	if (m_pParsedCtrl->GetHotLink())
		m_pParsedCtrl->GetHotLink()->PreCreateOwnerCtrl(m_pParsedCtrl, style);

	VERIFY(m_pParsedCtrl->Create(style, CRect(0,0,0,0), pWndOwner, m_nCtrlIDC));

	if	(
			m_pBaseDataObj &&
			m_pBaseDataObj->GetDataType() != DATA_STR_TYPE &&
			m_nDefaultCtrlLen == MIN_CTRL_SIZE
		)
		// trucco per ricordarmi che l'ho cambiato (vedi sotto)
		m_nDefaultCtrlLen = -(m_pParsedCtrl->GetInputCharLen());

	CFont* pFont = m_pParsedCtrl->GetCustomOwnFont();
	if (!pFont)
		pFont = pWndOwner->GetWndPreferredFont();
	m_pParsedCtrl->GetCtrlCWnd()->SetFont(pFont);

	if (IsDynamicDecimalFormatter())
		VERIFY(PrepareDynamicDecimalFormatter());

	if (m_pParsedCtrl->GetHotLink())
	{
		m_pParsedCtrl->GetHotLink()->DoOnCreatedOwnerCtrl();
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
int	ColumnInfo::ResizeCtrlWidth (const int& nHeight)
{
	int wCtrlSize = 0;
	int hCtrlSize = nHeight;
	if (m_bVisible)
	{
		// button has not to resize, but must return to it's original dimension + 2 for border
		if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		{
			CDC* pDC = m_pParsedCtrl->GetCtrlCWnd()->GetDC();
			CSize cs = GetEditSize(pDC, m_pParsedCtrl->GetCtrlCWnd()->GetFont(), 1, 1);
			m_pParsedCtrl->GetCtrlCWnd()->ReleaseDC(pDC);
			wCtrlSize = cs.cx + 2;
			hCtrlSize = cs.cx + 2;
		}
		else
			wCtrlSize = GetScreenWidth() - m_pParsedCtrl->GetAllButtonsWitdh();
	}
	m_pParsedCtrl->GetCtrlCWnd()->SetWindowPos (NULL, 0,0, wCtrlSize, hCtrlSize, SWP_NOMOVE | SWP_NOZORDER);

	return wCtrlSize;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetTitle (const CString& strT)
{ 
	m_strTitle = strT;
	
	CFont* pFont = AfxGetThemeManager()->GetFormFont();

	CDC* pDC = NULL;

	if (m_pBody->m_hWnd)
	{
		pDC = m_pBody->GetDC();
	}
	else
	{
		if (m_pBody->GetDocument() && m_pBody->GetDocument()->GetFirstView())
			pDC = m_pBody->GetDocument()->GetFirstView()->GetDC();
	}

	CalcTitleWidth (pDC, pFont);

	if (m_pBody->m_hWnd) 
		m_pBody->ReleaseDC(pDC);
	else if (m_pBody->GetDocument() && m_pBody->GetDocument()->GetFirstView())
		m_pBody->GetDocument()->GetFirstView()->ReleaseDC(pDC);

}

//-----------------------------------------------------------------------------
void ColumnInfo::CalcTitleWidth (CDC* pDC, CFont* pFont)
{
	if (!m_pBody->BEGetExStyle(BE_STYLE_SHOW_COLUMN_HEADERS))
	{
		m_nTitleWidth = 1;
		return;
	}

	//nel caso di ElapsedTime inserisco se richiesta la caption del titolo
	if	(
			m_pParsedCtrl->GetCtrlData() &&
			m_pParsedCtrl->GetDataType().m_wType == DATA_LNG_TYPE && 
			(DataLng*)(m_pParsedCtrl->GetCtrlData())->IsATime() &&
			m_pParsedCtrl->GetFormatIdx() >= 0
		)
	{
		CElapsedTimeFormatter* pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_pParsedCtrl->GetFormatIdx(), m_pParsedCtrl ? m_pParsedCtrl->m_pFormatContext : NULL);
		if (pFormatter && pFormatter->GetCaptionPos() != 0)
		{
			CString strSD = pFormatter->GetShortDescription();
			if( m_strTitle.Right(strSD.GetLength()) != strSD )
				m_strTitle += _T(" ") + pFormatter->GetShortDescription();
		}
	}

	if (m_pBody->BEGetExStyle(BE_STYLE_UPPERCASE_COLUMN_TITLES))
	{
		CString upperTitle(m_strTitle);
		upperTitle.MakeUpper();
		m_nTitleWidth = ::GetMultilineTextSize(pDC, pFont, upperTitle).cx + 2 * DEFAULT_COLUMN_OFFSET;
	}
	else
		m_nTitleWidth = ::GetMultilineTextSize(pDC, pFont, m_strTitle).cx + 2 * DEFAULT_COLUMN_OFFSET;

	if (m_pBody->m_bAddColumnHeaderExtraSpace)
	{
		if (m_nAlignFlags == TA_CENTER)
			m_nTitleWidth += GetTitleExtraRSpace() * 2 ;
		else
			m_nTitleWidth += GetTitleExtraRSpace();
	}
}

//-----------------------------------------------------------------------------
CSize ColumnInfo::GetCtrlSize()
{
	if (m_pParsedCtrl == NULL)
		return CSize(0, 0);

	// set control size : il 3^ parametro indica se si voglio includere gli
	// eventuali bottoni oppure no. Devo agganciare i controls altrimenti non
	// sono capaci a dimensionarsi corretti
	return m_pParsedCtrl->GetCtrlData() ? m_pParsedCtrl->SetCtrlSize(m_nCtrlLen, m_nCtrlRows, TRUE) : 10;
}

//-----------------------------------------------------------------------------
void ColumnInfo::ResizeColumn(CFont* pFont, int& xOffset, int nMaxColWidth, BOOL bButtonsToRemoveInRightAlign)
{
	ASSERT(m_pParsedCtrl);

	CSize aCtrlSize = GetCtrlSize();
	int nCtrlWidth = aCtrlSize.cx;

	CDC* pDC = m_pBody->GetDC();
	CalcTitleWidth (pDC, pFont);
	m_pBody->ReleaseDC(pDC);
	
	// set default column width
	if (m_nColScrnWidth <= 0)
	{
		if (m_nColCharWidth > 0)
		{
			m_nDataScrnWidth = m_pParsedCtrl->GetDataSize(m_nColCharWidth) - CTRL_NOT_INSIDE; 
		}
		else
		{
			m_nDataScrnWidth = nCtrlWidth - CTRL_NOT_INSIDE;
		}
		m_pBody->AddCtrlOffset (this, m_nDataScrnWidth);
		//----

		if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CIntBitmap)) || m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedStateImage)))
		{
			m_nColScrnWidth = max(m_nDataScrnWidth, m_nTitleWidth);
			m_nDataScrnWidth = m_nTitleWidth; 
		}
		else if (m_pBody->m_bAllowColumnHeaderSmallFont)
		{
			m_nColScrnWidth = max(m_nDataScrnWidth, 1);

			if (m_nTitleWidth > m_nDataScrnWidth)
				m_pBody->m_bUseColumnHeaderSmallFont = TRUE;
		}
		else
		{
			m_nColScrnWidth = max(m_nDataScrnWidth, m_nTitleWidth);
			m_eWidthType = m_nDataScrnWidth > m_nTitleWidth ? WT_DATA : WT_TITLE;
		}

		if (m_nColScrnWidth + 1 - CTRL_NOT_INSIDE > nMaxColWidth)
		{
			// colonna piu` larga del body : non accettata
			TRACE1("ColumnInfo::ResizeColumn: the column %s is too wide\n", (LPCTSTR) m_strTitle); 
			//ASSERT(FALSE);

			m_nColScrnWidth = nMaxColWidth - (1 - CTRL_NOT_INSIDE);	// to exclude column separator (one pixel)
		}

		m_nDefaultColScreenWidth = m_nColScrnWidth + 1 - CTRL_NOT_INSIDE;	// to include column separator (one pixel)
	}

	//--------------------------------- 
	nCtrlWidth = ResizeCtrlWidth(aCtrlSize.cy);

	// determina dove posizonare il control nell'ambito della colonna
	switch (m_nAlignFlags)
	{
		case TA_CENTER :
			m_nHorzCtrlOffset = (GetScreenWidth() - nCtrlWidth) / 2;
			break;

		case TA_LEFT :
			m_nHorzCtrlOffset = 0;
			break;

		case TA_RIGHT :
			m_nHorzCtrlOffset = GetScreenWidth() - nCtrlWidth + CTRL_NOT_INSIDE;
			if (bButtonsToRemoveInRightAlign)
			{
				m_nHorzCtrlOffset -= m_pParsedCtrl->GetAllButtonsWitdh();
				if (m_nHorzCtrlOffset < 0)
					m_nHorzCtrlOffset = CTRL_NOT_INSIDE;
			}
			break;

		default :
			TRACE1("ColumnInfo::ResizeColumn: the column %s has an illegal TextAlign\n", (LPCTSTR) m_strTitle); 
			ASSERT(FALSE);

			m_nHorzCtrlOffset = 0;
			break;
	}

	m_nColScrnWidth += 1 - CTRL_NOT_INSIDE;	// to include column separator (one pixel)

	// absolute column offset
	m_nHorzColOffset = xOffset;
		
	// addjust next column x offset based on current column width	
    xOffset += GetScreenWidth();
}

//-----------------------------------------------------------------------------
void ColumnInfo::ShowCtrl(SqlRecord* pRec, int xPos, int yPos)
{
	if (m_bUseOtherColumnCtrl)
	{
		ColumnInfo* pCol = m_pBody->GetAlternativeColumnForEditing(this, pRec);
		if (pCol)
		{
			pCol->ShowCtrl(pRec, xPos, yPos);
			return;
		}
	}

	if (IsDynamicDecimalFormatter())
		SetDynamicDecimalFormatter(pRec);
	else if (IsAddress())
		SetAddressComponents(pRec);
	else if (HasSingleComponent())
		SetComponent(pRec);

	//----
	m_pParsedCtrl->Attach(pRec->GetDataObjAt(m_nDataInfoIdx));
	m_pParsedCtrl->m_pSqlRecord = pRec;
	AttachCtrlStateData (pRec, NULL);

	if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
	{
		yPos += (m_pBody->m_nRowsHeight - 14) / 2; //14 is the checkbox square height
	}

	m_pParsedCtrl->SetCtrlPos
		(
			NULL, 
			xPos + m_nHorzCtrlOffset, 
			yPos,
			0, 0, 
			SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOZORDER
		);

	// Resize of control
	ResizeCtrlWidth(GetCtrlSize().cy);

	CRect rect;
	m_pParsedCtrl->GetCtrlCWnd()->GetWindowRect(&rect);
	m_pParsedCtrl->GetCtrlCWnd()->GetParent()->ScreenToClient(&rect);
	m_pParsedCtrl->GetCtrlCWnd()->GetParent()->InvalidateRect(&rect);
	
	// in this sequence for correct window update						
	m_pParsedCtrl->UpdateCtrlStatus();
	m_pParsedCtrl->UpdateCtrlView();
	m_pParsedCtrl->SetCtrlFocus(TRUE);
	m_pParsedCtrl->GetCtrlCWnd()->Invalidate();

	m_pBody->OnShowCtrl(pRec, this);
	if (m_pBody->m_pDBT && m_pBody->m_pDBT->GetDocument())
		m_pBody->m_pDBT->GetDocument()->DispatchOnBEShowCtrl(m_pBody, m_pBody->GetDBT()->GetCurrentRow(), this);
}

//-----------------------------------------------------------------------------
void ColumnInfo::ShowCtrl(SqlRecord* pRec, const CRect& rect)
{
	if (m_bUseOtherColumnCtrl)
	{
		ColumnInfo* pCol = m_pBody->GetAlternativeColumnForEditing(this, pRec);
		if (pCol)
		{
			pCol->ShowCtrl(pRec, rect);
			return;
		}
	}

	if (IsDynamicDecimalFormatter())
		SetDynamicDecimalFormatter(pRec);
	else if (IsAddress())
		SetAddressComponents(pRec);
	else if (HasSingleComponent())
		SetComponent(pRec);

	m_pParsedCtrl->Attach(pRec->GetDataObjAt(m_nDataInfoIdx));
	
	AttachCtrlStateData (pRec, NULL);

	m_pParsedCtrl->SetCtrlPos
		(
			NULL, 
			rect.left, rect.top, rect.Width(), rect.Height(), 
			SWP_SHOWWINDOW | SWP_NOZORDER
		);

	m_pParsedCtrl->GetCtrlCWnd()->GetParent()->InvalidateRect(&rect);
	
	// in this sequence for correct window update						
	m_pParsedCtrl->UpdateCtrlStatus();
	m_pParsedCtrl->UpdateCtrlView();
	m_pParsedCtrl->SetCtrlFocus(TRUE);
	m_pParsedCtrl->GetCtrlCWnd()->Invalidate();

	m_pBody->OnShowCtrl(pRec, this);
	if (m_pBody->m_pDBT && m_pBody->m_pDBT->GetDocument())
		m_pBody->m_pDBT->GetDocument()->DispatchOnBEShowCtrl(m_pBody, m_pBody->GetDBT()->GetCurrentRow(), this);
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::HideCtrl()
{
	if (m_bUseOtherColumnCtrl && m_pBody->GetDBT() && m_pBody->GetDBT()->GetCurrentRow())
	{
		ColumnInfo* pCol = m_pBody->GetAlternativeColumnForEditing(this, m_pBody->GetDBT()->GetCurrentRow());
		if (pCol)
		{
			pCol->HideCtrl();
			return TRUE;
		}
	}

#ifdef _DEBUG
	if (m_pParsedCtrl->GetModifyFlag())
	{
		TRACE0("BodyEdit: HideCtrl called with ModifyFlag = TRUE\n");
	}
#endif

	m_pParsedCtrl->ShowCtrl(SW_HIDE);
	if (m_pBody->GetDBT())
	{
		m_pBody->OnHideCtrl(m_pBody->GetDBT()->GetCurrentRow(), this);
		if (m_pBody->m_pDBT->GetDocument())
			m_pBody->m_pDBT->GetDocument()->DispatchOnBEHideCtrl(m_pBody, m_pBody->GetDBT()->GetCurrentRow(), this);
	}
	BOOL bDeAttachDataObj = TRUE;

	// si tiene conto del fatto che nel caso di hotlink che e` stato
	// attivato per un inserimento al volo o per una chiamata al radar
	// ed in questo caso NON bisogna cambiare il dataobj
	// a cui e` correntemente attaccato il control poiche` l'hotlink
	// fa riferimento proprio ad esso
	HotKeyLink* pHotKeyLink = (HotKeyLink*)m_pParsedCtrl->GetHotLink();

	if (pHotKeyLink)
		bDeAttachDataObj = ! pHotKeyLink->IsHotLinkRunning();
	else if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CNamespaceEdit)))
	{
		bDeAttachDataObj = ! ((CNamespaceEdit*)m_pParsedCtrl->GetCtrlCWnd())->IsRunning();
	}
	else if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CColorEdit)))
	{
		bDeAttachDataObj = !((CColorEdit*)m_pParsedCtrl->GetCtrlCWnd())->IsRunning();
	}

	if (bDeAttachDataObj)
	{
		m_pParsedCtrl->Attach(m_pBaseDataObj);
		m_pParsedCtrl->DetachAllStateData ();
		m_pParsedCtrl->EnableCtrl(FALSE);
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void ColumnInfo::ReattachBaseData()
{
	m_pParsedCtrl->Attach(m_pBaseDataObj);
	m_pParsedCtrl->UpdateCtrlView();
	m_pParsedCtrl->SetErrorID(CParsedCtrl::EMPTY_MESSAGE);
	m_pParsedCtrl->SetWarningID(CParsedCtrl::EMPTY_MESSAGE);
	m_pParsedCtrl->SetModifyFlag(FALSE);
	m_pParsedCtrl->EnableCtrl(FALSE);
	m_pParsedCtrl->ShowCtrl(SW_HIDE);	
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::UpdateData(BOOL bSignalError, BOOL bSendMessage)
{
	return m_pParsedCtrl->UpdateCtrlData(bSignalError, bSendMessage);
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::IsReadOnly(SqlRecord* pRec/*= NULL*/) const
{
	ASSERT(m_pParsedCtrl && m_pParsedCtrl->GetCtrlCWnd() && m_pBaseDataObj);

	if (
			(m_wColumnStatus & STATUS_GRAYED) == STATUS_GRAYED 
			||
			m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedStatic)) 
		)
		return TRUE;

	if (m_pBaseDataObj->IsReadOnly()) 
		return TRUE;

	DataObj* pObj;
	if (pRec && (pObj = pRec->GetDataObjAt(m_nDataInfoIdx)) && pObj->IsReadOnly())
	{
		if (
				pObj->IsOSLReadOnly() &&
				m_pBody->m_pDBT &&
				m_pBody->m_pDBT->GetDocument() &&
				m_pBody->m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::EDIT &&
				OSL_CAN_DO(m_pBody->GetInfoOSL(), OSL_GRANT_BE_ADDROW) &&
				OSL_CAN_DO((const_cast<ColumnInfo*>(this))->GetInfoOSL(), OSL_GRANT_NEW) &&
				pRec->IsInsertedByUI()
			)
		{
			pObj->SetOSLReadOnly(FALSE);
			return FALSE;
		}

		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::IsStatic() const
{
	CWnd* pCtrl = NULL;
	BOOL bReadOnly =
			(
				GetParsedCtrl() &&
				(pCtrl = GetParsedCtrl()->GetCtrlCWnd()) &&
				(
					pCtrl->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CStatic))
				||
					pCtrl->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CStatic))
				)
			);
	return bReadOnly;
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::ToggleColumnOrdering()
{
	switch  (m_Ordering)
	{
		case NONE:
		{m_Ordering = ASC; return TRUE;}
	
		case ASC:
		{m_Ordering = DESC; return TRUE;}
		
		case DESC:
		{m_Ordering = NONE; return TRUE;}	
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void ColumnInfo::AttachCtrlStateData (SqlRecord* pRecord, CParsedCtrl* pRWControl)
{
	ASSERT_VALID(pRecord);

	// Gestione degli stati
	m_pParsedCtrl->DetachAllStateData ();	

	if (pRWControl && pRWControl->GetCtrlCWnd() && pRWControl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
		pRWControl = NULL;

	if (pRWControl)
	{
		pRWControl->DetachAllStateData ();
		pRWControl->SetButtonsXOffset(GetParsedCtrl()->GetButtonsXOffset());
	}

	for (int i=0; i <= m_arStateDataInfoIdx.GetUpperBound(); i++)
	{
		ColumnInfoStateCtrlData* pData = (ColumnInfoStateCtrlData*) m_arStateDataInfoIdx.GetAt(i);

		AttachCtrlStateData(pData, pRecord, pRWControl);
	}
	//GetParsedCtrl()->UpdateStateButtons ();	//NO, non è ancora correttamente posizionato, ci pensa la ShowCtrl
	if (pRWControl)
	{
		pRWControl->UpdateStateButtons ();
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::AttachCtrlStateData (ColumnInfoStateCtrlData* pData, SqlRecord* pRec, CParsedCtrl* pRWControl /*= NULL*/)
{
	ASSERT_VALID(pData);
	ASSERT_VALID(pRec);

	DataObj* pObj = pData->m_nDataInfoIdx >= 0 ? pRec->GetDataObjAt(pData->m_nDataInfoIdx) : pData->m_pDataObj;
	ASSERT_VALID(pObj);

	CStateCtrlObj* pSC = NULL;
	CStateCtrlObj* pSC_RW = NULL;

	pSC = m_pParsedCtrl->AttachStateData(pObj, pData->m_bInvertDefaultState);
	//ASSERT_VALID(pSC);	
	if (pRWControl)
	{
		pSC_RW = pRWControl->AttachStateData(pObj, pData->m_bInvertDefaultState);
		ASSERT_VALID(pSC_RW);	
	}
	
	if (pData->m_bIsCommandBtn)
	{
		if (pSC) 
			pSC->SetCommandBtn();
		if (pSC_RW)
			pSC_RW->SetCommandBtn();
	}

	for (int i = 0; i < pData->m_idbNormal.GetCount(); i++)
	{
		if (pSC)
			pSC->SetCtrlState(i, pData->m_idbNormal[i], pData->m_idbDisabled[i]);
		if (pSC_RW)
			pSC_RW->SetCtrlState(i, pData->m_idbNormal[i], pData->m_idbDisabled[i]);	
	}

	if (pData->m_bSetColorInEditableState)
	{
		if (pSC)
			pSC->SetColorInEditableState(pData->m_crBkgColor, pData->m_crTextColor);
		if (pSC_RW)
			pSC_RW->SetColorInEditableState(pData->m_crBkgColor, pData->m_crTextColor);
	}
}

//-----------------------------------------------------------------------------
ColumnInfoStateCtrlData* ColumnInfo::AttachStateData (int nDataObjIdx, BOOL bInvertDefaultStates /*= FALSE*/)
{
	ASSERT(nDataObjIdx > -1);
	ColumnInfoStateCtrlData* pData = NULL;
#ifdef _DEBUG
	for (int i=0; i <= m_arStateDataInfoIdx.GetUpperBound(); i++)
	{
		pData = (ColumnInfoStateCtrlData*) m_arStateDataInfoIdx.GetAt(i);
		if (!pData)
			continue;

		if (nDataObjIdx == pData->m_nDataInfoIdx && pData->m_nDataInfoIdx > -1)
		{
			ASSERT(FALSE);
			TRACE(_T("ColumnInfo::AttachStateData: the DataObj has been already linked as state data to the control"));
		}
	}
#endif
	pData = new ColumnInfoStateCtrlData(nDataObjIdx, bInvertDefaultStates);
	m_arStateDataInfoIdx.Add (pData);
	return pData;
}

//-----------------------------------------------------------------------------
ColumnInfoStateCtrlData* ColumnInfo::AttachStateData (DataObj* pDataObj, BOOL bInvertDefaultStates /*= FALSE*/)
{

	ASSERT(pDataObj);
	ASSERT(m_pBody);
	ASSERT(m_pBody->GetDBT());

	SqlRecord* pRec = m_pBody->GetDBT()->GetRecord();

	int nIdx = -1;
	// se appartiene al SqlRecord me ne tengo l'idx
	// altrimenti lo tratto come campo unico sempre fisso
	if (pRec && pRec->IsPresent(pDataObj, nIdx))
		return AttachStateData(nIdx, bInvertDefaultStates);

	GetParsedCtrl()->OnAttachStateData(pDataObj, bInvertDefaultStates);
	ColumnInfoStateCtrlData* pExistingData = NULL;

	for (int i = 0; i <= m_arStateDataInfoIdx.GetUpperBound(); i++)
	{
		ColumnInfoStateCtrlData* pData = (ColumnInfoStateCtrlData*)m_arStateDataInfoIdx.GetAt(i);
		if (!pData)
			continue;

		if (pDataObj == pData->m_pDataObj && pData->m_pDataObj)
			pExistingData = pData;
	}

	if (!pExistingData)
	{
		pExistingData = new ColumnInfoStateCtrlData(-1, bInvertDefaultStates);
		m_arStateDataInfoIdx.Add(pExistingData);
		pExistingData->m_pDataObj = pDataObj;
	}
	return pExistingData;
}

//=============================================================================
BOOL ColumnInfo::EnableDynamicDecimalFormatterByRecord	(SqlRecord* pRec, DataObj* pDecField, int nMaxDec/* = -1*/)
{ 
	ASSERT(pRec && pDecField);
	
	return EnableDynamicDecimalFormatter(pRec->Lookup(pDecField), NULL, nMaxDec); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::EnableDynamicDecimalFormatterByColumn	(ColumnInfo* pColDec, int nMaxDec/* = -1*/)
{ 
	if (!pColDec || !pColDec->m_pBaseDataObj->IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return EnableDynamicDecimalFormatter(pColDec->m_nDataInfoIdx, NULL, nMaxDec); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::EnableDynamicDecimalFormatterByExternal (DataInt* pNumDec, int nMaxDec/* = -1*/)
{ 
	if (!pNumDec || !pNumDec->IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return EnableDynamicDecimalFormatter(-1, pNumDec, nMaxDec); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::EnableDynamicDecimalFormatter(int nDecDataInfoIdx, DataInt* pNumDec, int nMaxDec/* = -1*/)
{
	if (!m_pBaseDataObj->IsKindOf(RUNTIME_CLASS(DataDbl)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	ASSERT((nDecDataInfoIdx != -1 && pNumDec == NULL) || (nDecDataInfoIdx == -1 && pNumDec != NULL));
	ASSERT(nMaxDec != 0 && nMaxDec >= -1);

	m_pDynamicDecFmt = new ColumnInfoDynamicDecimalFormatter(this, pNumDec ? -1 : nDecDataInfoIdx, pNumDec, nMaxDec);

	return TRUE;
}

//=============================================================================
ColumnInfoComponents* ColumnInfo::AddComponentByRecord	(SqlRecord* pRec, DataObj* pComp)
{ 
	ASSERT(pRec && pComp);
	
	return AddComponent(pRec->GetIndexFromDataObj(pComp), NULL); 
}

//-----------------------------------------------------------------------------
ColumnInfoComponents* ColumnInfo::AddComponentByColumn	(ColumnInfo* pColComp)
{ 
	if (!pColComp)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return AddComponent(pColComp->m_nDataInfoIdx, NULL); 
}

//-----------------------------------------------------------------------------
ColumnInfoComponents* ColumnInfo::AddComponentByExternal (DataObj* pComp)
{ 
	if (!pComp)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return AddComponent(-1, pComp); 
}

//-----------------------------------------------------------------------------
ColumnInfoComponents* ColumnInfo::AddComponent(int nDataInfoIdx, DataObj* pComp)
{
	ASSERT((nDataInfoIdx != -1 && pComp == NULL) || (nDataInfoIdx == -1 && pComp != NULL));

	m_pComponent = new ColumnInfoComponents(this, pComp ? -1 : nDataInfoIdx, pComp);

	return m_pComponent;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetComponent (SqlRecord* pCurrentRow)
{
	if (!m_pComponent)
	{
		ASSERT(FALSE);
		return;
	}
	
	DataObj* pD = pCurrentRow->GetDataObjAt(m_nDataInfoIdx);
	if (!pD /*|| pD->IsEmpty()*/)
		return;

	CParsedCtrl* pCtrl = GetParsedCtrl ();
	if (pCtrl && pCtrl->GetCtrlCWnd() && ::IsWindow(pCtrl->GetCtrlCWnd()->m_hWnd))
	{
		pCtrl->SetValue(*pD);

		pCtrl->BindParam(m_pComponent->GetComponent(pCurrentRow));
	}
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::PrepareDynamicDecimalFormatter()
{
	if (!m_pDynamicDecFmt)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CDblFormatter* pDblFormatter = (CDblFormatter*) GetParsedCtrl ()->GetCurrentFormatter();
	
	if (!pDblFormatter)
	{
		DataType dt = m_pBaseDataObj->GetDataType();
		if (m_pBody && m_pBody->GetDocument())
			pDblFormatter = (CDblFormatter*) AfxGetFormatStyleTable()->GetFormatter(dt, &(m_pBody->GetDocument()->GetNamespace()));
	}
	if (!pDblFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
	{
		ASSERT(FALSE);
		SAFE_DELETE(m_pDynamicDecFmt);
		return FALSE;
	}
	
	//uso 2 istanze differenti del formattatore perche durante l'editing eventuali paint mi rimodificherebbero i decimali del control
	GetParsedCtrl ()->AttachFormatter(pDblFormatter->Clone());

	if (m_pDynamicDecFmt->m_pFormatter != NULL)
		delete m_pDynamicDecFmt->m_pFormatter;

	m_pDynamicDecFmt->m_pFormatter = (CDblFormatter*) pDblFormatter->Clone();

	m_pDynamicDecFmt->m_nMaxDec = m_pDynamicDecFmt->m_nMaxDec == -1 ? pDblFormatter->GetDecNumber() : m_pDynamicDecFmt->m_nMaxDec;

	CFont* pFont = GetParsedCtrl ()->GetCtrlCWnd()->GetFont();

	CDC* pDC = m_pBody->GetDC();
	CSize csDec = ::GetTextSize(pDC, CString(_T("0")), pFont);
	m_pDynamicDecFmt->m_nDecXPad = csDec.cx;

	csDec = ::GetTextSize(pDC, pDblFormatter->GetDecSeparator(), pFont);
	m_pDynamicDecFmt->m_nSepXPad = csDec.cx;

	m_pBody->ReleaseDC(pDC);

	return TRUE;
}

//-----------------------------------------------------------------------------
int ColumnInfo::GetDynamicDecimalFormatter(SqlRecord* pCurrentRow)
{
	DataInt* pDec = 
		m_pDynamicDecFmt->m_pNumDec ?
			m_pDynamicDecFmt->m_pNumDec : 
			(DataInt*) pCurrentRow->GetDataObjAt(m_pDynamicDecFmt->m_nDecDataIdx);

	return (int) * pDec;
}

//-----------------------------------------------------------------------------
int ColumnInfo::SetDynamicDecimalFormatter(SqlRecord* pCurrentRow)
{
	int nDec = GetDynamicDecimalFormatter(pCurrentRow);

	GetParsedCtrl()->SetCtrlNumDec(nDec);

	return nDec;
}

//-----------------------------------------------------------------------------
CString ColumnInfo::OnFormatDynamicDecimal(DataObj* pCellData, SqlRecord* pCurrentRow, int& nXOffset)
{
	int nDec = GetDynamicDecimalFormatter(pCurrentRow);

	nXOffset = -(m_pDynamicDecFmt->m_nDecXPad * max(0, m_pDynamicDecFmt->m_nMaxDec - nDec));
	if (nDec == 0) 
		nXOffset -= m_pDynamicDecFmt->m_nSepXPad;
	
	/*int nOldDN = */m_pDynamicDecFmt->m_pFormatter->SetDecNumber(nDec);
	CString sCell;
	m_pDynamicDecFmt->m_pFormatter->FormatDataObj(*pCellData, sCell);
	// Ripristino il vecchio numero di decimali
	//NON serve il formattatore è clonato! m_pDynamicDecFmt->m_pFormatter->SetDecNumber(nOldDN);

	return sCell;
}

//=============================================================================
void ColumnInfo::SetScreenWidth (int nWidth, BOOL bSave)
{ 
	nWidth = ScalePix(nWidth);
	m_nColScrnWidth = nWidth; 
	m_eWidthType = WT_CUSTOM;
	if (bSave)
	{
		m_nCustomScrnWidth = nWidth;
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetScreenWidth (ColumnWidthType wt)
{ 
	m_nColScrnWidth = GetAlternativeWidth(wt); 
	m_eWidthType = wt;
}

//-----------------------------------------------------------------------------
//void ColumnInfo::SetAlternativeWidth()
//{
//	SetScreenWidth(GetNextWidthType());
//}

//-----------------------------------------------------------------------------
ColumnInfo::ColumnWidthType	ColumnInfo::GetNextWidthType(BOOL bFixed/* = TRUE*/) const
{
	switch (m_eWidthType)
	{
	case WT_DATA:
		if (bFixed)
			return WT_TITLE;
		else
			return m_nDataScrnWidth != m_nTitleWidth ? WT_TITLE : WT_CUSTOM;

	case WT_TITLE:
		if (bFixed)
			return WT_CUSTOM;
		else
			return m_nTitleWidth != m_nCustomScrnWidth ? WT_CUSTOM : WT_DATA;

	case WT_CUSTOM:
		if (bFixed)
			return WT_DATA;
		else
			return m_nCustomScrnWidth != m_nDataScrnWidth ? WT_DATA : WT_TITLE;
	}
	ASSERT(FALSE);
	return WT_CUSTOM;
}

//-----------------------------------------------------------------------------
int	ColumnInfo::GetAlternativeWidth (ColumnWidthType wt) const
{
	switch (wt)
	{
	case WT_TITLE:
		return m_nTitleWidth;
	case WT_DATA:
		return m_nDataScrnWidth;
	case WT_CUSTOM:
		return m_nCustomScrnWidth;
	}
	return m_nColScrnWidth;
}

//-----------------------------------------------------------------------------
ColumnInfo::ColumnWidthType ColumnInfo::GetCurrentWidthType() const
{
	//if (m_nColScrnWidth == m_nTitleWidth)
	//{
	//	return WT_TITLE;
	//}
	//else if (m_nColScrnWidth == m_nDataScrnWidth)
	//{
	//	return WT_DATA;
	//}
	////else if (m_nColScrnWidth == m_nCustomScrnWidth)
	////{
	////	return WT_CUSTOM;
	////}
	//return WT_CUSTOM;
	return m_eWidthType;
}

//=============================================================================
// Diagnostics

#ifdef _DEBUG
void ColumnInfo::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nColumnInfo :");
	AFX_DUMP1(dc, "\n\tTitle = ", m_strTitle);
}

void ColumnInfo::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//------------------------------------------------------------------------------

////////////////////////////////////////////////////////////////////////////////
//				class ColumnsInfoArray implementation
////////////////////////////////////////////////////////////////////////////////

BOOL ColumnsInfoArray::LessThen( CObject* pO1, CObject* pO2) const
{
	return ((ColumnInfo*)pO1)->m_nColPos < ((ColumnInfo*)pO2)->m_nColPos;
}

int	ColumnsInfoArray::Compare(CObject* pO1, CObject* pO2) const
{
	return ((ColumnInfo*)pO1)->m_nColPos < ((ColumnInfo*)pO2)->m_nColPos ? 
			-1 :
			(((ColumnInfo*)pO1)->m_nColPos > ((ColumnInfo*)pO2)->m_nColPos ?
			 1 :
			 0);
}

////////////////////////////////////////////////////////////////////////////////
//				class ColumnInfoComponentsBase implementation
////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(ColumnInfoComponentsBase, CObject)


////////////////////////////////////////////////////////////////////////////////
//				class ColumnInfoComponents implementation
////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(ColumnInfoComponents, ColumnInfoComponentsBase)

//------------------------------------------------------------------------------
ColumnInfoComponents::ColumnInfoComponents (ColumnInfo* pColumn, int nCompDataIdx, DataObj* pComp /*= NULL*/)
	:
	ColumnInfoComponentsBase(pColumn),

	m_nCompDataIdx	(nCompDataIdx),
	m_pComp			(pComp),
	m_bUseComponentToFormat (FALSE)
{
}

//------------------------------------------------------------------------------
DataObj* ColumnInfoComponents::GetComponent(SqlRecord* pCurrentRow) const
{
	if (m_pComp)
		return m_pComp;
	if (m_nCompDataIdx >= 0)
		return pCurrentRow->GetDataObjAt(m_nCompDataIdx);
	return NULL;
}

//------------------------------------------------------------------------------
void ColumnInfoComponents::UseComponentToFormat(BOOL b)
{ 
	m_bUseComponentToFormat = b; 

	if (m_nCompDataIdx >= 0)
	{ 
		ASSERT_VALID(m_pColumn);
		ASSERT(m_pColumn->GetParsedCtrl());
		m_pColumn->GetParsedCtrl()->m_nUseComponentToFormat = m_nCompDataIdx;
	}
}


////////////////////////////////////////////////////////////////////////////////
//				class ColumnInfoDynamicDecimalFormatter implementation
////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(ColumnInfoDynamicDecimalFormatter, ColumnInfoComponentsBase)

//------------------------------------------------------------------------------
ColumnInfoDynamicDecimalFormatter::ColumnInfoDynamicDecimalFormatter 
										(
											ColumnInfo* pColumn, 
											int nDecDataIdx, DataInt* pNumDec /*= NULL*/, int nMaxDec /*= -1*/
										)
	:
	ColumnInfoComponentsBase(pColumn),

	m_nDecDataIdx	(nDecDataIdx),
	m_pNumDec		(pNumDec),
	m_nMaxDec		(nMaxDec),
	m_nDecXPad		(0),
	m_nSepXPad		(0),
	m_pFormatter	(NULL)
{
}

//------------------------------------------------------------------------------
ColumnInfoDynamicDecimalFormatter::~ColumnInfoDynamicDecimalFormatter ()
{
	SAFE_DELETE(m_pFormatter);
}

////////////////////////////////////////////////////////////////////////////////
//				class ColumnInfoAddressComponents implementation
////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(ColumnInfoAddressComponents, ColumnInfoComponentsBase)

//------------------------------------------------------------------------------
ColumnInfoAddressComponents::ColumnInfoAddressComponents(ColumnInfo* pColumn)
	:
	ColumnInfoComponentsBase(pColumn),

	m_pCity			(NULL),
	m_pCounty		(NULL),
	m_pCountry		(NULL),
	m_pZipCode		(NULL),
	m_pRegion		(NULL),
	m_pLatitude		(NULL),
	m_pLongitude	(NULL),
	//m_pStreetNumber (NULL),
	m_pAddress		(NULL),
	m_pDistrict		(NULL),
	m_pFederalState	(NULL),
	m_pISOCode		(NULL),

	m_nDataIdxCity		(-1),
	m_nDataIdxCounty	(-1),
	m_nDataIdxCountry	(-1),
	m_nDataIdxZipCode	(-1),
	m_nDataIdxRegion	(-1),
	m_nDataIdxLatitude	(-1),
	m_nDataIdxLongitude	(-1),
	//m_nDataIdxStreetNumber (-1),
	m_nDataIdxAddress	(-1),
	m_nDataIdxDistrict	(-1),
	m_nDataIdxFederalState(-1),
	m_nDataIdxISOCode	(-1)
{
}

//------------------------------------------------------------------------------
void ColumnInfoAddressComponents::AddComponent(CBaseAddressEdit::FieldType eFieldType, int nDataIdx, DataStr* pComp)
{
	switch (eFieldType)
	{
		case CBaseAddressEdit::CITY:
			if (pComp)
				m_pCity = pComp;
			else
				m_nDataIdxCity = nDataIdx;
			break;

		case CBaseAddressEdit::COUNTY:
			if (pComp)
				m_pCounty = pComp;
			else
				m_nDataIdxCounty = nDataIdx;
			break;

		case CBaseAddressEdit::COUNTRY:
			if (pComp)
				m_pCountry = pComp;
			else
				m_nDataIdxCountry = nDataIdx;
			break;

		case CBaseAddressEdit::ZIP:
			if (pComp)
				m_pZipCode = pComp;
			else
				m_nDataIdxZipCode = nDataIdx;
			break;

		case CBaseAddressEdit::REGION:
			if (pComp)
				m_pRegion = pComp;
			else
				m_nDataIdxRegion = nDataIdx;
			break;

		case CBaseAddressEdit::LATITUDE:
			if (pComp)
				m_pLatitude = pComp;
			else
				m_nDataIdxLatitude = nDataIdx;
			break;

		case CBaseAddressEdit::LONGITUDE:
			if (pComp)
				m_pLongitude = pComp;
			else
				m_nDataIdxLongitude = nDataIdx;
			break;

		case CBaseAddressEdit::ADDRESS:
			if (pComp)
				m_pAddress = pComp;
			else
				m_nDataIdxAddress = nDataIdx;
			break;

		case CBaseAddressEdit::DISTRICT:
			if (pComp)
				m_pDistrict = pComp;
			else
				m_nDataIdxDistrict = nDataIdx;
			break;

		case CBaseAddressEdit::FEDERAL_STATE:
			if (pComp)
				m_pFederalState = pComp;
			else
				m_nDataIdxFederalState = nDataIdx;
			break;

		case CBaseAddressEdit::ISO_CODE:
			if (pComp)
				m_pISOCode = pComp;
			else
				m_nDataIdxISOCode = nDataIdx;
			break;
	}
}

//------------------------------------------------------------------------------
DataStr* ColumnInfoAddressComponents::GetComponent(CBaseAddressEdit::FieldType eFieldType, SqlRecord* pCurrentRow) const
{
	switch (eFieldType)
	{
		case CBaseAddressEdit::CITY:
			if (m_pCity)
				return m_pCity;
			if (m_nDataIdxCity >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxCity);
			break;
		case CBaseAddressEdit::COUNTY:
			if (m_pCounty)
				return m_pCounty;
			if (m_nDataIdxCounty >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxCounty);
			break;
		case CBaseAddressEdit::COUNTRY:
			if (m_pCountry)
				return m_pCountry;
			if (m_nDataIdxCountry >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxCountry);
			break;
		case CBaseAddressEdit::ZIP:
			if (m_pZipCode)
				return m_pZipCode;
			if (m_nDataIdxZipCode >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxZipCode);
			break;
		case CBaseAddressEdit::REGION:
			if (m_pRegion)
				return m_pRegion;
			if (m_nDataIdxRegion >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxRegion);
			break;
		case CBaseAddressEdit::LATITUDE:
			if (m_pLatitude)
				return m_pLatitude;
			if (m_nDataIdxLatitude >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxLatitude);
			break;
		case CBaseAddressEdit::LONGITUDE:
			if (m_pLongitude)
				return m_pLongitude;
			if (m_nDataIdxLongitude >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxLongitude);
			break;
		case CBaseAddressEdit::ADDRESS:
			if (m_pAddress)
				return m_pAddress;
			if (m_nDataIdxAddress >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxAddress);
			break;
		case CBaseAddressEdit::DISTRICT:
			if (m_pDistrict)
				return m_pDistrict;
			if (m_nDataIdxDistrict >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxDistrict);
			break;
		case CBaseAddressEdit::FEDERAL_STATE:
			if (m_pFederalState)
				return m_pFederalState;
			if (m_nDataIdxFederalState >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxFederalState);
			break;
		case CBaseAddressEdit::ISO_CODE:
			if (m_pISOCode)
				return m_pISOCode;
			if (m_nDataIdxISOCode >= 0)
				return (DataStr*) pCurrentRow->GetDataObjAt(m_nDataIdxISOCode);
			break;
	}
	return NULL;
}

//------------------------------------------------------------------------------
CString ColumnInfoAddressComponents::GetAddressText(SqlRecord* pCurrentRow) const
{
	CString sA;
	DataStr* pComp;
	DataStr* pISOCode = GetComponent(CBaseAddressEdit::ISO_CODE, pCurrentRow);
	if (pISOCode && *pISOCode == ISO_COUNTRY_CODE_BRAZIL)
	{
		// per Brasile visualizza in ordine ISO, CAP, Stato, Indirizzo, Distretto, Città, Paese, ecc.
		if ((pComp = GetComponent(CBaseAddressEdit::ISO_CODE, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("ISO Code"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::ZIP, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("ZipCode"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::FEDERAL_STATE, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Federal State"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::ADDRESS, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Address"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::DISTRICT, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("District"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::CITY, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("City"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::COUNTRY, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Country"), (LPCTSTR)pComp->GetString());
		// campi non Brasile
		if ((pComp =GetComponent(CBaseAddressEdit::COUNTY, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("County"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::REGION, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Region"), (LPCTSTR)pComp->GetString());
	}
	else
	{
		// per non Brasile visualizza in ordine Indirizzo, Città, CAP, Provincia, Paese, ISO, ecc.
		if ((pComp = GetComponent(CBaseAddressEdit::ADDRESS, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Address"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::CITY, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("City"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::ZIP, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("ZipCode"), (LPCTSTR)pComp->GetString());
		if ((pComp =GetComponent(CBaseAddressEdit::COUNTY, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("County"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::REGION, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Region"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::COUNTRY, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Country"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::ISO_CODE, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("ISO Code"), (LPCTSTR)pComp->GetString());
		// Brasile (non dovrebbero apparire)
		if ((pComp = GetComponent(CBaseAddressEdit::DISTRICT, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("District"), (LPCTSTR)pComp->GetString());
		if ((pComp = GetComponent(CBaseAddressEdit::FEDERAL_STATE, pCurrentRow)) && !pComp->GetString().IsEmpty())
			sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Federal State"), (LPCTSTR)pComp->GetString());
	}

	// posizione geografica per ultima 
	if ((pComp = GetComponent(CBaseAddressEdit::LATITUDE, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Latitude"), (LPCTSTR)pComp->GetString());
	if ((pComp = GetComponent(CBaseAddressEdit::LONGITUDE, pCurrentRow)) && !pComp->GetString().IsEmpty())
		sA += cwsprintf(_T("{0-%s}:{1-%s}\n"), _TB("Longitude"), (LPCTSTR)pComp->GetString());

	if (!sA.IsEmpty())
		sA.Truncate(sA.GetLength() - 1);
	return sA;
}

//=============================================================================
BOOL ColumnInfo::AddAddressComponentByRecord	(CBaseAddressEdit::FieldType ft, SqlRecord* pRec, DataStr* pDecField)
{ 
	ASSERT(pRec && pDecField);
	
	return AddAddressComponent(ft, pRec->Lookup(pDecField), NULL); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::AddAddressComponentByColumn	(CBaseAddressEdit::FieldType ft, ColumnInfo* pCol)
{ 
	if (!pCol || !pCol->m_pBaseDataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return AddAddressComponent(ft, pCol->m_nDataInfoIdx, NULL); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::AddAddressComponentByExternal (CBaseAddressEdit::FieldType ft, DataStr* pComp)
{ 
	if (!pComp || !pComp->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return AddAddressComponent(ft, -1, pComp); 
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::AddAddressComponent(CBaseAddressEdit::FieldType eFieldType, int nDataInfoIdx, DataStr* pComp)
{
	if (!m_pBaseDataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	ASSERT((nDataInfoIdx != -1 && pComp == NULL) || (nDataInfoIdx == -1 && pComp != NULL));

	if (m_pAddressComp == NULL)
		m_pAddressComp = new ColumnInfoAddressComponents(this);

	m_pAddressComp->AddComponent(eFieldType, nDataInfoIdx, pComp);

	return TRUE;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetAddressComponents(SqlRecord* pCurrentRow)
{
	if (!m_pAddressComp)
	{
		ASSERT(FALSE);
		return;
	}
	
	DataStr* pD = (DataStr*) pCurrentRow->GetDataObjAt(m_nDataInfoIdx);
	if (!pD /*|| pD->IsEmpty()*/)
		return;

	CWnd* pW = GetParsedCtrl ()->GetCtrlCWnd();
	ASSERT(pW->IsKindOf(RUNTIME_CLASS(CBaseAddressEdit)));

	CBaseAddressEdit* pAddrCtrl = (CBaseAddressEdit*) pW;

	if (!pD->IsEmpty())
		pAddrCtrl->SetValue(pD->GetString());

	DataStr* pComp;
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::CITY, pCurrentRow))
		pAddrCtrl->BindCity(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::COUNTY, pCurrentRow))
		pAddrCtrl->BindCounty(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::COUNTRY, pCurrentRow))
		pAddrCtrl->BindCountry(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::ZIP, pCurrentRow))
		pAddrCtrl->BindZip(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::REGION, pCurrentRow))
		pAddrCtrl->BindRegion(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::LATITUDE, pCurrentRow))
		pAddrCtrl->BindLatitude(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::LONGITUDE, pCurrentRow))
		pAddrCtrl->BindLongitude(pComp);

	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::ADDRESS, pCurrentRow))
		pAddrCtrl->BindAddress(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::DISTRICT, pCurrentRow))
		pAddrCtrl->BindDistrict(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::FEDERAL_STATE, pCurrentRow))
		pAddrCtrl->BindFederalState(pComp);
	if (pComp = m_pAddressComp->GetComponent(CBaseAddressEdit::ISO_CODE, pCurrentRow))
		pAddrCtrl->BindISOCode(pComp);
}

//=============================================================================
BOOL ColumnInfo::HasHyperLink() const
{
	CParsedCtrl* pCtrl = GetParsedCtrl();
	if (pCtrl == NULL)
		return FALSE;

	if (pCtrl->GetHyperLink() && pCtrl->GetHotLink())
	{
		if (!pCtrl->GetHotLink()->IsHyperLinkEnabled())
			return FALSE;
		if (!pCtrl->GetHotLink()->GetAddOnFlyNamespace().IsEmpty())
			return TRUE;
	}

	CWnd* pW = pCtrl->GetCtrlCWnd();
	if (pW == NULL)
		return FALSE;

	if (pW->IsKindOf(RUNTIME_CLASS(CLinkEdit)))
	{
		CLinkEdit* pL = (CLinkEdit*) pW;

		return pL->IsEnabledLink();
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void ColumnInfo::BrowseHyperLink(SqlRecord* pRec)
{
	CParsedCtrl* pCtrl = GetParsedCtrl();

	if (pCtrl->GetHyperLink())
	{
		DataObj* pD = pRec->GetDataObjAt(m_nDataInfoIdx);

		if (pD && !pD->IsEmpty())
			pCtrl->GetHyperLink()->DoFollowHyperlink(pD);
		return;
	}
	ASSERT(pCtrl->GetCtrlCWnd());
	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CAddressEdit)))
	{
		SetAddressComponents(pRec);

		CAddressEdit* pAddrCtrl = (CAddressEdit*) pCtrl->GetCtrlCWnd();
		pAddrCtrl->ShowMap();
		return;
	}

	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CLinkEdit)))
	{
		DataStr* pD = (DataStr*) pRec->GetDataObjAt(m_nDataInfoIdx);
		if (pD && !pD->IsEmpty())
		{
			CLinkEdit* pNsCtrl = (CLinkEdit*) pCtrl->GetCtrlCWnd();
			pNsCtrl->SetValue(pD->GetString());

			if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CPhoneEdit)))
				SetComponent(pRec);

			pNsCtrl->OnBrowseLink();
		}
		return;
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::RefreshHyperLink(SqlRecord* pRec)
{
	CParsedCtrl* pCtrl = GetParsedCtrl();

	if (pCtrl->GetHyperLink())
	{
		DataObj* pD = pRec->GetDataObjAt(m_nDataInfoIdx);

		if (pD && !pD->IsEmpty())
			pCtrl->GetHyperLink()->RefreshHyperlink(pD);
		return;
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::UpdateFooter()
{
	if (!m_pFooterCalculator)
		return;
	
	m_pFooterCalculator->Signal(NULL, /*EventType::*/ON_CHANGED);
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetHasFooter(BOOL bSet)
{
	if (!m_pBaseDataObj)
		return;
	m_pFooterCalculator = new CFooterCalculator(this, m_pBody);
	m_pBaseDataObj->AttachEvents(m_pFooterCalculator);//farà lui la delete
}

//-----------------------------------------------------------------------------
DataObj* ColumnInfo::GetFooterDataObj()
{
	return m_pFooterCalculator ? m_pFooterCalculator->m_pDataObj : NULL;
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::IsHidden() const
{
	return (m_wColumnStatus & STATUS_HIDDEN) == STATUS_HIDDEN;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetVisible (BOOL bVisible, BOOL bApply /*TRUE*/)
{
	if ((m_wColumnStatus & STATUS_NOCHANGE_HIDDEN) == STATUS_NOCHANGE_HIDDEN)
	{
		return;
	}
	if (bVisible && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE))
	{
		return;
	}

	BOOL bActualVisible = !((m_wColumnStatus & STATUS_HIDDEN) == STATUS_HIDDEN);
	
	if (bVisible && !bActualVisible)
	{
		m_wColumnStatus	&= ~STATUS_HIDDEN;
		if (bApply)
		{
			// provo a risistemarla nella sua posizione se riesco
			if (m_nColPos < 0 || m_nColPos > m_pBody->m_ColumnsInfo.GetUpperBound())
				m_pBody->m_ColumnsInfo.Add(this);
			else
				m_pBody->m_ColumnsInfo.InsertAt(m_nColPos, this);
			m_pBody->RecalcColumnsLayout();
		}
	}
	else if (!bVisible && bActualVisible)
	{
		m_wColumnStatus	|= STATUS_HIDDEN;
		if (bApply)
		{
			m_pBody->m_ColumnsInfo.Remove(this);
			m_pBody->RecalcColumnsLayout();
		}
	}

	if (m_pFooterCalculator)
	{
		UpdateFooter();
	}
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetBaseDataObj (DataObj* pDataObj)
{
	BOOL bChangeDataObj = m_pBaseDataObj != pDataObj;
	// Se cambia devo detachare prima le strutture correlate
	if (bChangeDataObj)
	{
		if (IsAddress())
			SAFE_DELETE(m_pAddressComp);

		if (HasSingleComponent())
			SAFE_DELETE(m_pComponent);
	}

	m_pBaseDataObj = pDataObj;
	m_nDataInfoIdx = m_pBaseDataObj == NULL ? -1 : m_pBody->GetDBT()->GetRecord()->Lookup(m_pBaseDataObj); ;

	if (bChangeDataObj && m_pBaseDataObj && m_pBody && m_pBody->GetDBT())
		InitializeLengthsFromBaseDataObj();
}

//-----------------------------------------------------------------------------
void ColumnInfo::ChangeParsedControlTo	(CRuntimeClass* pNewClass, DWORD dwOldStyles, DWORD dwStyles, CString sName)
{
	CParsedCtrl* pOldCtrl = m_pParsedCtrl;
	HotKeyLinkObj* pHotLink = NULL;
	UINT nBtnID = NO_BUTTON;
	
	// La struttura ColumnInfoAddressComponents è legata solo al databinding
	ColumnInfoDynamicDecimalFormatter* pOldDynamicDecFmt =	NULL; /*m_pDynamicDecFmt ? 
															m_pDynamicDecFmt->Clone() :
															NULL;*/
	
	HWND hOldHandle = NULL;
	if (pOldCtrl != NULL)
	{
		hOldHandle = pOldCtrl->GetCtrlCWnd ()->m_hWnd;
		pHotLink = pOldCtrl->GetHotLink(); 
		nBtnID = pOldCtrl->GetButtonIDBmp();
		HideCtrl ();
		pOldCtrl->GetCtrlCWnd ()->SetParent(NULL);
		pOldCtrl->GetCtrlCWnd ()->UnsubclassWindow();
		m_pParsedCtrl = NULL;
	}

	if (m_nCtrlIDC < 0)
		m_nCtrlIDC = AfxGetTBResourcesMap()->GetTbResourceID(GetNamespace().ToString(), TbControls);
	
	m_pParsedCtrl = m_pBody->CreateParsedCtrl
		(
			sName,
			m_nCtrlIDC,
			m_pBaseDataObj,
			pNewClass,
			(HotKeyLink*) pHotLink,
			nBtnID
		);

	m_dwCtrlStyle &= ~dwOldStyles;
	m_dwCtrlStyle |= dwStyles;

	if (!CreateCtrl(m_pBody))
	{
		m_pParsedCtrl = pOldCtrl;
		m_pParsedCtrl->GetCtrlCWnd()->SubclassWindow(hOldHandle);
		m_pParsedCtrl->GetCtrlCWnd()->SetParent(m_pBody);
		m_pDynamicDecFmt = pOldDynamicDecFmt;
		return;
	}

	if (m_pBaseDataObj)
		InitializeLengthsFromBaseDataObj();

	if (pOldCtrl != NULL)
		delete pOldCtrl;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetColPos (const int& nPos)
{
	m_nColPos = nPos;
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetCtrlRows (int nCtrlRows)
{ 
	m_nCtrlRows = nCtrlRows; 
	m_pBody->ResizeColumn(this); 
}

// adeguamento di tutte le lunghezze coinvolte. Queste chiamate vanno tutte
// eseguite rigorosamente tutte e in quest'ordine, altrimenti non funzionano
//-----------------------------------------------------------------------------
void ColumnInfo::InitializeLengthsFromBaseDataObj ()
{
	int nLen = 0;
		
	if (m_nDataInfoIdx >= 0 && m_pBody->GetDBT())
	{
		SqlRecordItem* pRecItem = m_pBody->GetDBT()->GetRecord()->GetAt(m_nDataInfoIdx);
		ASSERT_VALID(pRecItem);
		nLen = pRecItem->GetColumnLength();
	}

	m_pBody->RecalcCtrlLen(m_pBaseDataObj, nLen, this);

	SetColWidth (m_nCtrlLen);
	SetScreenWidth (0, TRUE);
	// avvisa il parsed control perchè la SetCtrlSize cabla FALSE in applyNow
	if (m_pParsedCtrl && m_pParsedCtrl->GetCtrlData() && m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_STR_TYPE)
		m_pParsedCtrl->SetCtrlMaxLen(m_nCtrlLen, TRUE);

	m_pBody->ResizeColumn(this);
}

//-----------------------------------------------------------------------------
void ColumnInfo::SetDefaultScreenWidth (const int& nWidth)
{
	m_nDefaultColScreenWidth = nWidth;
}

//-----------------------------------------------------------------------------
BOOL ColumnInfo::IsSortedByDescription() const
{
	CWnd* pCtrl = GetControl();
	return pCtrl &&
		(
			pCtrl->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)) ||
			pCtrl->IsKindOf(RUNTIME_CLASS(CBoolButton)) ||
			pCtrl->IsKindOf(RUNTIME_CLASS(CEnumCombo)) ||
			pCtrl->IsKindOf(RUNTIME_CLASS(CEnumStatic)) ||
			pCtrl->IsKindOf(RUNTIME_CLASS(CParsedStateImage))
		);
}

