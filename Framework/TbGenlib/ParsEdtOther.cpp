#include "stdafx.h"

#include <float.h>
#include <ctype.h>
#include <atlimage.h>

#include <TBNameSolver\Chars.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\pictures.h>
#include <TbGeneric\linefile.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\CMapi.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbGenlib\DirTreeCtrl.h>
#include <TbGenlib\CEFClasses.h>

#include <TbGes\JsonFormEngineEx.h>

#include <TbStringLoader\Generic.h>

#include <TbParser\SymTable.h>

#include "baseapp.h"
#include "Generic.h"
#include "parsobj.h"
#include "hlinkobj.h"
#include "parsedt.h"
#include "TbExplorerInterface.h"
#include <TbFrameworkImages\CommonImages.h>
#include "TBPropertyGrid.h"
#include "BaseTileDialog.h"

// resources
#include "parsres.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
#include "ParsEdtOther.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//			Class CParsedBitmap implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedBitmap, CParsedStatic)

BEGIN_MESSAGE_MAP(CParsedBitmap, CParsedStatic)
	//{{AFX_MSG_MAP(CParsedBitmap)
		ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)
		ON_WM_PAINT()
		ON_WM_NCPAINT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedBitmap::CParsedBitmap(DataObj* pData /*= NULL*/)
	:
	CParsedStatic	(pData),
	m_pBitmap		(NULL)
{
}

//-----------------------------------------------------------------------------
void CParsedBitmap::Attach(DataObj* pDataObj)
{
	CParsedCtrl::Attach (pDataObj);	
}

//-----------------------------------------------------------------------------
LRESULT CParsedBitmap::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndImageDescription* pDesc = (CWndImageDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId);
	UpdateDescription(pDesc);

	return (LRESULT) pDesc;
}

//Metodo usato dalla cella di bodyedit che contiene un CParsedBitmap, per farsi ritornare la descrizione della bitmap.
//Differisce dal OnGetControlDescription per il calcolo dell'id. Siccome nel Bodyedit un CparsedBitmap puo' essere presente
//su piu' righe con lo stesso handle, ma deve generare descrizione diverse, come chiave si usa non l'handle della finestra ma anche la riga
//del bodyedit cui appartiene
//-----------------------------------------------------------------------------
CWndImageDescription* CParsedBitmap::GetControlStructure(CString strId, CWndObjDescriptionContainer* pContainer)
{
	CWndImageDescription* pDesc = (CWndImageDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId);
	UpdateDescription(pDesc);
	return pDesc;
}

//Metodo che aggiorna la descrizione del CParsedBitmap
//-----------------------------------------------------------------------------
void CParsedBitmap::UpdateDescription(CWndImageDescription* pDesc)
{
	pDesc->UpdateAttributes(this);
	if ((HBITMAP)*m_pBitmap)
	{
		CString sName = cwsprintf(_T("pbmp%ud.png"), (HBITMAP)*m_pBitmap);
				
		if (pDesc->m_ImageBuffer.Assign( (HBITMAP)*m_pBitmap, sName, this))
			pDesc->SetUpdated(&pDesc->m_ImageBuffer);
	}
}

//-----------------------------------------------------------------------------
void CParsedBitmap::SetValue(CBitmap* pBitmap)
{
	m_pBitmap = pBitmap;

	if (IsTBWindowVisible(this))
		CParsedBitmap::SetValue(L"");
}

//-----------------------------------------------------------------------------
void CParsedBitmap::SetValue(LPCTSTR pszValue /*= NULL*/)
{
	__super::SetValue(pszValue);
}

//-----------------------------------------------------------------------------
CSize CParsedBitmap::GetImageSize()
{
	if (m_pBitmap == NULL)
	{
		return CSize(0, 0);
	}
		

	BITMAP bm;
	m_pBitmap->GetObject(sizeof(BITMAP), (LPTSTR)&bm);
	return CSize(bm.bmWidth, bm.bmHeight);
}

//-----------------------------------------------------------------------------
void CParsedBitmap::DrawBitmap(CDC& DC, const CRect& rect)
{
	if (m_pBitmap == NULL)
		return;

	::DrawBitmap(m_pBitmap, &DC, rect, (m_dwCtrlStyle & BMP_STYLE_STRETCH) == BMP_STYLE_STRETCH);
}

//-----------------------------------------------------------------------------
void CParsedBitmap::DrawImage()
{
	CRect rect; 
	GetClientRect(rect);
	// specific paint
	CClientDC DC(this);
		
	if (HasEdgeStyle())
	{
		// Disegna i bordi uguali allo sfondo per nasconderli
		CPen pen(PS_SOLID, 0, AfxGetThemeManager()->GetBackgroundColor());
		CPen* pPenOld = DC.SelectObject(&pen);
		DC.MoveTo(rect.left - 1 , rect.top - 1); DC.LineTo(rect.left - 1, rect.bottom + 1);
		DC.MoveTo(rect.left - 1 , rect.top - 1); DC.LineTo(rect.right + 1, rect.top - 1);
		DC.MoveTo(rect.right,     rect.top - 1); DC.LineTo(rect.right, rect.bottom);
		DC.MoveTo(rect.left - 1,  rect.bottom);  DC.LineTo(rect.right + 1, rect.bottom);
		if (pPenOld)
		{
			DC.SelectObject(pPenOld);
		}
	}
		
	DrawBitmap(DC, rect);
}

//-----------------------------------------------------------------------------
void CParsedBitmap::OnPaint()
{
	__super::OnPaint();

	DrawImage();
}

// Ripristiato NcPaint nei documenti
//-----------------------------------------------------------------------------
void CParsedBitmap::OnNcPaint()
{
	__super::OnNcPaint();

	DrawImage();
}

//-----------------------------------------------------------------------------
CSize CParsedBitmap::AdaptNewSize	(UINT, UINT, BOOL)
{
	CDC* pDC = m_pOwnerWnd->GetDC();
	int nEditSize = GetEditSize(pDC, AfxGetThemeManager()->GetControlFont(), 1, 1).cy;
	ReleaseDC(pDC);

	return CSize(nEditSize, nEditSize);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CParsedBitmap::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CParsedBitmap\n");
	CParsedStatic::Dump(dc);
}
#endif // _DEBUG

//=============================================================================
//			Class CNSBitmap implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CNSBitmap, CParsedBitmap)

//-----------------------------------------------------------------------------
CNSBitmap::CNSBitmap()
:
CParsedBitmap()
{
	m_nsBitmap = _T("");
	m_pBitmap = new CBitmap();
}

//-----------------------------------------------------------------------------
CNSBitmap::CNSBitmap(DataStr* pData)
	:
	CParsedBitmap(pData)
{
	m_nsBitmap = _T("");
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	m_pBitmap = new CBitmap();
}

//-----------------------------------------------------------------------------
CNSBitmap::~CNSBitmap()
{
	SAFE_DELETE(m_pBitmap);
}

//-----------------------------------------------------------------------------
void CNSBitmap::DrawBitmap(CDC& dcDest, const CRect& rect)
{
	if (m_nsBitmap.IsEmpty())
	{
		return;
	}
	__super::DrawBitmap(dcDest, rect);
}

//-----------------------------------------------------------------------------
void CNSBitmap::SetValue(CString sValue)
{
	if (m_nsBitmap.Compare(sValue) == 0 && m_pBitmap->GetSafeHandle())
	{
		// Invalidate(); // Tolto perchè non necessario .... da vedere con test
		return;
	}
	
	if (m_pBitmap->GetSafeHandle())
	{
		HBITMAP hOldBitmap = (HBITMAP)m_pBitmap->Detach();
		if (hOldBitmap) ::DeleteObject(hOldBitmap);
	}

	m_nsBitmap = sValue;

	if (!m_nsBitmap.IsEmpty())
	{
		if (!LoadBitmapOrPng(m_pBitmap, m_nsBitmap))
			m_nsBitmap.Empty();
	}

	if (IsTBWindowVisible(this))
	{
		Invalidate();
	}	
}

//-----------------------------------------------------------------------------
void CNSBitmap::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	SetValue(((DataStr &)aValue).GetString());
}
//-----------------------------------------------------------------------------
CString CNSBitmap::GetValue()
{
	return m_nsBitmap;
}

//-----------------------------------------------------------------------------
void CNSBitmap::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	m_nsBitmap = ((DataStr &)aValue).GetString();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CNSBitmap::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	dc << " CIntBitmap\n\bID del bitmap: " << m_nsBitmap << "\n";
	CParsedBitmap::Dump(dc);
}
#endif // _DEBUG

//=============================================================================
//			Class CIntBitmap implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntBitmap, CParsedBitmap)

//-----------------------------------------------------------------------------
CIntBitmap::CIntBitmap()
	:
	CParsedBitmap (),
	m_nIDBitmap	(0)	
{
	m_pBitmap	= new CBitmap();
}

//-----------------------------------------------------------------------------
CIntBitmap::CIntBitmap(DataInt* pData)
	:
	CParsedBitmap	(pData),
	m_nIDBitmap		(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	m_pBitmap	= new CBitmap();
}

//-----------------------------------------------------------------------------
CIntBitmap::~CIntBitmap ()
{
	SAFE_DELETE(m_pBitmap);
}

//-----------------------------------------------------------------------------
void CIntBitmap::DrawBitmap (CDC& dcDest, const CRect& rect)
{
	if (m_nIDBitmap > 0 && !m_pBitmap->GetSafeHandle())
	{
		CRect r = rect;
		// visualizza il valore sbagliato
		dcDest.DrawText(cwsprintf(_T("%d"), m_nIDBitmap), r, DT_SINGLELINE);
		//CParsedStatic::SetValue(cwsprintf(_T("%d"), m_nIDBitmap));
		return;
	}

	CParsedBitmap::DrawBitmap (dcDest, rect);
}

//-----------------------------------------------------------------------------
void CIntBitmap::SetValue(int nValue)
{
	if (m_nIDBitmap == nValue)
		return;
	
	if (m_pBitmap->GetSafeHandle())
	{
		HBITMAP hOldBitmap = (HBITMAP)m_pBitmap->Detach();
		if (hOldBitmap) ::DeleteObject(hOldBitmap);
	}

	m_nIDBitmap = nValue;
	if (nValue > 0)
	{
		LoadBitmapOrPng(m_pBitmap, m_nIDBitmap);
	}
	
	if (IsTBWindowVisible(this))
		Invalidate();
}

//-----------------------------------------------------------------------------
int CIntBitmap::GetValue()
{
	return m_nIDBitmap;
}

//-----------------------------------------------------------------------------
void CIntBitmap::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((int) ((DataInt &)aValue));
}

//-----------------------------------------------------------------------------
void CIntBitmap::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataInt &)aValue).Assign(m_nIDBitmap);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CIntBitmap::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " CIntBitmap\n\bID del bitmap: " << m_nIDBitmap << "\n";
	CParsedBitmap::Dump(dc);
}
#endif // _DEBUG

//=============================================================================
//			Class CStateImage implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CParsedStateImage, CParsedBitmap)

//-----------------------------------------------------------------------------
CParsedStateImage::CParsedStateImage()
	:
	CParsedBitmap	(),
	m_nImageID		(-1)
{
}

//-----------------------------------------------------------------------------
CParsedStateImage::CParsedStateImage(DataInt* pData)
	:
	CParsedBitmap	(pData),
	m_nImageID		(-1)
{
}

//-----------------------------------------------------------------------------
CParsedStateImage::~CParsedStateImage()
{
	POSITION pos = m_mapStateImages.GetStartPosition();
	while (pos)
	{
		CStateImg *pImg(NULL);
		int key;
		m_mapStateImages.GetNextAssoc(pos, key, pImg);
		if (pImg) delete pImg;
	}

	m_mapStateImages.RemoveAll();
}

//-----------------------------------------------------------------------------
void CParsedStateImage::AddStateImg(const CString& sText, const CString& sTooltip, int nIDBitmap)
{
	CStateImg* pImg = new CStateImg(sText, sTooltip, nIDBitmap);

	if (nIDBitmap > 0)
	{
		LoadBitmapOrPng(&pImg->m_Bitmap, nIDBitmap);
	}

	m_mapStateImages.SetAt(nIDBitmap, pImg);
}

//-----------------------------------------------------------------------------
void CParsedStateImage::AddStateImg(const CString& sText, const CString& sTooltip, const CString& sNsTooltip, int nId)
{
	CStateImg* pImg = new CStateImg(sText, sTooltip, sNsTooltip);

	if (pImg->m_nsImage.IsValid() > 0)
	{
		LoadBitmapOrPng(&pImg->m_Bitmap, sNsTooltip);
	}

	m_mapStateImages.SetAt(nId, pImg);
}
//-----------------------------------------------------------------------------
CString CParsedStateImage::FormatData(const DataObj* pDataObj, BOOL) const
{
	ASSERT_VALID(pDataObj);
	ASSERT_KINDOF(DataInt, pDataObj);

	int nID = *(DataInt*)pDataObj;

	CStateImg* pImg = NULL;
	if (!m_mapStateImages.Lookup(nID, pImg))
	{
		return L"";
	}
	ASSERT(pImg);

	return pImg->m_sText;
}

//-----------------------------------------------------------------------------
CString CParsedStateImage::GetTooltip(const DataInt* pDataObj) const
{
	ASSERT_VALID(pDataObj);
	ASSERT_KINDOF(DataInt, pDataObj);

	int nID = pDataObj ? *(DataInt*)pDataObj : m_nImageID;

	CStateImg* pImg = NULL;
	if (!m_mapStateImages.Lookup(nID, pImg))
	{
		return L"";
	}
	ASSERT(pImg);

	return pImg->m_sTooltip;
}

//-----------------------------------------------------------------------------
void CParsedStateImage::DrawBitmap(CDC& dcDest, const CRect& rect)
{
	if (m_nImageID == -1)
		return;

	CStateImg* pImg = NULL;
	if (!m_mapStateImages.Lookup(m_nImageID, pImg))
	{ 
		CRect r = rect;
		// visualizza il valore sbagliato
		if (m_nImageID > 0)
			dcDest.DrawText(cwsprintf(_T("%d"), m_nImageID), r, DT_SINGLELINE);
		return;
	}
	ASSERT(pImg);

	if (!pImg->m_Bitmap.GetSafeHandle())
	{
		CRect r = rect;
		// visualizza il valore sbagliato
		if (m_nImageID > 0)
			dcDest.DrawText(cwsprintf(_T("%d"), m_nImageID), r, DT_SINGLELINE);
		return;
	}

	this->m_pBitmap = &pImg->m_Bitmap;

	CParsedBitmap::DrawBitmap(dcDest, rect);
}

//-----------------------------------------------------------------------------
void CParsedStateImage::SetValue(int nValue)
{
	if (m_nImageID == nValue)
		return;

	m_nImageID = nValue;

#ifdef _DEBUG
	CStateImg* pImg = NULL;
	if (m_nImageID > -1 && !m_mapStateImages.Lookup(m_nImageID, pImg))
	{
		// visualizza il valore sbagliato (0 potrebbe volutamente non avere immagine)
		if (m_nImageID > 0)
			CParsedStatic::SetValue((LPCTSTR)cwsprintf(_T("%d"), m_nImageID));
		return;
	}
#endif

	if (IsTBWindowVisible(this))
		Invalidate();
}

//-----------------------------------------------------------------------------
void CParsedStateImage::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	SetValue((int)((DataInt &)aValue));
}

//-----------------------------------------------------------------------------
int CParsedStateImage::GetValue()
{
	return m_nImageID;
}

//-----------------------------------------------------------------------------
void CParsedStateImage::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataInt &)aValue).Assign(m_nImageID);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CParsedStateImage::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	dc << " CParsedStateImage\nCurrent ID del bitmap: " << m_nImageID << "\n";
	CParsedBitmap::Dump(dc);
}
#endif // _DEBUG

//=============================================================================
//			Class CLinkEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLinkEdit, CStrEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLinkEdit, CStrEdit)

	ON_WM_SETCURSOR		()
	ON_WM_LBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
	ON_WM_SETFOCUS		()

	ON_COMMAND			(ID_BROWSE_LINK,	OnBrowseLink)
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)

END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CLinkEdit::CLinkEdit()
	:
	CStrEdit				(),
	m_hLinkCursor			(NULL),
	m_bEnabledLink			(TRUE)
{
	SetCtrlStyle(STR_STYLE_ALL);
}

//-----------------------------------------------------------------------------
CLinkEdit::CLinkEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CStrEdit				(nBtnIDBmp, pData),
	m_hLinkCursor			(NULL),
	m_bEnabledLink			(TRUE)
{
	SetCtrlStyle(STR_STYLE_ALL);
}

//-----------------------------------------------------------------------------
CLinkEdit::~CLinkEdit()
{
	m_fontEdit.DeleteObject();
	m_fontUnderline.DeleteObject();
}

//-----------------------------------------------------------------------------
BOOL CLinkEdit::OnInitCtrl()
{
	VERIFY(__super::OnInitCtrl());

	CFont* pFont = this->GetFont();
	LOGFONT lf;
	if (pFont)
	{
		pFont->GetObject(sizeof(lf), &lf);

		m_fontEdit.CreateFontIndirect(&lf);

		lf.lfUnderline = TRUE;
		m_fontUnderline.CreateFontIndirect(&lf);
		SetFont(&m_fontEdit);
	}	

	m_hLinkCursor = AfxGetApp()->LoadStandardCursor(IDC_HAND);
	ASSERT(m_hLinkCursor);
 
 	return TRUE;
}

//-----------------------------------------------------------------------------
void CLinkEdit::SetOwnFont (CFont* pFont, BOOL bOwns/* = FALSE*/)	
{ 
	__super::SetOwnFont (pFont, bOwns);	
	
	m_fontEdit.DeleteObject();
	m_fontUnderline.DeleteObject();

	LOGFONT lf;
	pFont->GetObject(sizeof(lf), &lf);

	m_fontEdit.CreateFontIndirect(&lf);

	lf.lfUnderline = TRUE;
	m_fontUnderline.CreateFontIndirect(&lf);

	SetFont(IsEditReadOnly() ? &m_fontUnderline : &m_fontEdit);
}

//-----------------------------------------------------------------------------
void CLinkEdit::SetEnabledLink(BOOL bEnabled)
{
	m_bEnabledLink = bEnabled;
}

//-----------------------------------------------------------------------------
BOOL CLinkEdit::IsValid()
{
	if	(
			m_pDocument && 
			m_pDocument->GetFormMode() == CBaseDocument::BROWSE &&
			IsEditReadOnly ()
		)
		return TRUE;

	return __super::IsValid();
}

//-----------------------------------------------------------------------------
BOOL CLinkEdit::UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage /* = FALSE */)
{
	if	(
			m_pDocument && 
			m_pDocument->GetFormMode() == CBaseDocument::BROWSE &&
			IsEditReadOnly ()
		)
		return TRUE;

	return __super::UpdateCtrlData(bEmitError, bSendMessage);
}

//-----------------------------------------------------------------------------
void CLinkEdit::ModifiedCtrlData()
{
	if	(
			m_pDocument && 
			m_pDocument->GetFormMode() == CBaseDocument::BROWSE &&
			IsEditReadOnly ()
		)
		return;

	__super::ModifiedCtrlData();
}

//-----------------------------------------------------------------------------
CWnd* CLinkEdit::SetCtrlFocus(BOOL bSetSel /*FALSE*/)
{
	m_bColored = !GetDocument() || (GetDocument()->GetFormMode() == CBaseDocument::BROWSE && !GetDocument()->IsABatchDocument());
	return __super::SetCtrlFocus(bSetSel);
}

//-----------------------------------------------------------------------------
void CLinkEdit::OnSetFocus (CWnd* pWnd)
{
	__super::OnSetFocus(pWnd);
	m_bColored = !GetDocument() || (GetDocument()->GetFormMode() == CBaseDocument::BROWSE && !GetDocument()->IsABatchDocument());
	SetSel(-1, FALSE);
}

//-----------------------------------------------------------------------------
void CLinkEdit::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{	
	CMenu menu;
	menu.CreatePopupMenu();

	if(!GetMenuButton(&menu))
	{
		menu.AppendMenu(MF_STRING, ID_BROWSE_LINK,	_TB("Open..."));
	}
	
	AppendEditMenu(&menu);
	menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, mousePos.x, mousePos.y, this);	
}

//-----------------------------------------------------------------------------
void CLinkEdit::SetColor()
{
	if (IsEditReadOnly())
	{
		SetTextColor(AfxGetThemeManager()->GetHyperLinkForeColor());

		SetBkgColor(AfxGetThemeManager()->GetBackgroundColor());

		SetFont(&m_fontUnderline);

		SetSel(-1, FALSE);
	}
	else
	{
		SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());

		SetBkgColor(AfxGetThemeManager()->GetEnabledControlBkgColor());

		SetFont(&m_fontEdit);
	}
}

//-----------------------------------------------------------------------------
BOOL CLinkEdit::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message) 
{
	CString str;
   	CParsedCtrl::GetValue(str);

	if (m_bEnabledLink && IsEditReadOnly() && !str.IsEmpty())
	{
		//Use the hand cursor
		::SetCursor(m_hLinkCursor);   
		return TRUE;
	}

	//Let the parent class do its thing
	return __super::OnSetCursor(pWnd, nHitTest, message);
}

//-----------------------------------------------------------------------------
void CLinkEdit::DoEnable (BOOL bEnable)
{
	if (GetCtrlData() && !GetCtrlData()->IsReadOnly())
		bEnable = TRUE;

	__super::DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
BOOL CLinkEdit::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	BOOL bOk = __super::EnableCtrl(bEnable);

	if (GetCtrlData() && GetCtrlData()->IsEmpty() && !bEnable)
	{
		SetBkgColor(AfxGetThemeManager()->GetBackgroundColor());
		return bOk;
	}

	__super::SetEditReadOnly(!bEnable);

	SetColor();

	return bOk;
}

//-----------------------------------------------------------------------------
void CLinkEdit::OnLButtonDown(UINT nFlags, CPoint point) 
{
	CString str = _T("");
   	CParsedCtrl::GetValue(str);
	if (str.IsEmpty())
	{
		__super::OnLButtonDown(nFlags, point);
		return;
	}

	if (IsEditReadOnly() && m_bEnabledLink) 
	{
		PostClickMessage();
		SetSel(-1, FALSE);
	}
	else
		__super::OnLButtonDown(nFlags, point);
}

//-----------------------------------------------------------------------------
void CLinkEdit::PostClickMessage()
{
	PostMessage(WM_COMMAND, ID_BROWSE_LINK);
}
//-----------------------------------------------------------------------------
CString	CLinkEdit::OnCustomizeLink	(CString sValue)
{
	return m_sPrefix + sValue;
}

//-----------------------------------------------------------------------------
void CLinkEdit::OnBrowseLink ()
{
	CString str;
	CParsedCtrl::GetValue(str);
	if (str.IsEmpty())
		return;

	HINSTANCE hInst = ::TBShellExecute(OnCustomizeLink(str));
	if (hInst <= (HINSTANCE)32)
		AfxMessageBox (ShellExecuteErrMsg((int)hInst));;
}



//=============================================================================
//			Class CPhoneEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPhoneEdit, CLinkEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPhoneEdit, CLinkEdit)
//	ON_COMMAND			(ID_BROWSE_LINK,		OnBrowseLink)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPhoneEdit::CPhoneEdit()
	:
	CLinkEdit (),
	m_pISOCode (NULL),
	m_nDataIdxParam (-1)

{
	SetPrefix(_T("callto:"));
}

//-----------------------------------------------------------------------------
CPhoneEdit::CPhoneEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CLinkEdit (nBtnIDBmp, pData),
	m_pISOCode (NULL),
	m_nDataIdxParam (-1)
{
	SetPrefix(_T("callto:"));
}

//-----------------------------------------------------------------------------
CString	CPhoneEdit::LookupTelephonePrefix (DataStr* pISOCode)
{
	if (!pISOCode) 
		return _T("");

	CString sNs;
	if (AfxIsActivated(MAGONET_APP, _NS_ACT("Company")))
		sNs = _NS_HKL("HotKeyLink.Erp.Company.Dbl.ISOCountryCodes");
	else if (AfxIsActivated(OFM_APP, _NS_ACT("Office")))
		sNs = _NS_HKL("HotKeyLink.OFM.Office.Dbl.ISOCountryCodes");
	else
		return _T("");

	FailedInvokeCode aFailedCode;
	CTBNamespace ns(sNs);
	HotKeyLinkObj* pHKL = AfxGetTbCmdManager()->RunHotlink (ns, &aFailedCode);
	if (pHKL)
	{
		if (pHKL->DoFindRecord(pISOCode))
		{
			DataObj* pTelephonePrefix = pHKL->GetField(_NS_FLD("TelephonePrefix"));
			if (pTelephonePrefix)
			{
				CString s = ((DataStr*)pTelephonePrefix)->GetString();
				if (!s.IsEmpty())
				{
					if (s[0] != '+' && s.Left(2) != _T("00"))
						s = '+' + s;
					return s;
				}
			}
		}
		delete pHKL;
	}
	return _T("");
}

//-----------------------------------------------------------------------------
CString	CPhoneEdit::OnCustomizeLink	(CString sValue)
{
	CString sV (sValue); sV.MakeLower();
	if (sV.Find(_T("callto:")) == 0 || sV.Find(_T("skype:")) == 0)
		return sValue;
	if (sValue[0] == '+' || _istalpha(sValue[0]) || sValue.Find(_T("00")) == 0)
		return m_sPrefix + sValue;

	if (m_pISOCode)
	{
		CString s = LookupTelephonePrefix(m_pISOCode);
		sValue = s + sValue;
	}

	return m_sPrefix + sValue;
}

//=============================================================================
//			Class CEmailAddressEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CEmailAddressEdit, CLinkEdit)



//-----------------------------------------------------------------------------
CEmailAddressEdit::CEmailAddressEdit()
	:
	CLinkEdit ()
{
	SetPrefix(_T("mailto:"));
	m_bEnabledLink = AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT);
}

//-----------------------------------------------------------------------------
CEmailAddressEdit::CEmailAddressEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CLinkEdit (nBtnIDBmp, pData)
{
	SetPrefix(_T("mailto:"));
	m_bEnabledLink = AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT);
}

//-----------------------------------------------------------------------------
void CEmailAddressEdit::Attach (UINT /*nBtnID*/)
{ 
	if (AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
		m_nButtonIDBmp = BTN_OUTLOOK_ID; 
}

//-----------------------------------------------------------------------------
CString	CEmailAddressEdit::OnCustomizeLink	(CString sValue)
{
	sValue.Replace(CMapiMessage::TAG_CERTIFIED, L"");
	return m_sPrefix + sValue;
}


//=============================================================================
//			Class CPathEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPathEdit, CLinkEdit)

BEGIN_MESSAGE_MAP(CPathEdit, CLinkEdit)
	//{{AFX_MSG_MAP(CPathEdit)
	  ON_WM_DROPFILES()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPathEdit::CPathEdit()
	:
	CLinkEdit()
{
	SetCtrlStyle(STR_STYLE_ALL);
}	
    
//-----------------------------------------------------------------------------
CPathEdit::CPathEdit(UINT nBtnIDBmp, DataStr* pData)
	:
	CLinkEdit(nBtnIDBmp, pData)
{
	SetCtrlStyle(STR_STYLE_ALL);
}	

//-----------------------------------------------------------------------------
int CPathEdit::OnInitCtrl() 
{
	DragAcceptFiles ();
	return __super::OnInitCtrl();
}
//-----------------------------------------------------------------------------
void CPathEdit::OnDropFiles(HDROP hDropInfo)
{
	UINT nFiles = ::DragQueryFile(hDropInfo, (UINT)-1, NULL, 0);
	for (UINT iFile = 0; iFile < nFiles; iFile++)
	{
		TCHAR szFileName[_MAX_PATH];
		::DragQueryFile(hDropInfo, iFile, szFileName, _MAX_PATH);
		SetValue(szFileName);
	}
	::DragFinish(hDropInfo);
}

//-----------------------------------------------------------------------------
BOOL CPathEdit::SetDefaultDir(LPCTSTR lpszDefaultDir)
{
	BOOL bCheckExist= (m_dwCtrlStyle & PATH_STYLE_NO_CHECK_EXIST) != PATH_STYLE_NO_CHECK_EXIST;
	if 
		(
			lpszDefaultDir		&& 
			lpszDefaultDir[0]	&&
			!IsPathName(lpszDefaultDir, bCheckExist)
		)
		return FALSE;
	
	m_strDefaultDir = lpszDefaultDir;
	
	return TRUE;
}
//-----------------------------------------------------------------------------
void CPathEdit::GetValue(CString& strValue)
{
	__super::GetValue(strValue);

	// Se la path è relativa la concateno con m_strDefaultDir
	if (!strValue.IsEmpty() && !m_strDefaultDir.IsEmpty() && IsRelativePath(strValue))
	{
		if (!IsDirSeparator(m_strDefaultDir[m_strDefaultDir.GetLength() - 1]))
			m_strDefaultDir += SLASH_CHAR;
		if (IsDirSeparator(strValue[0]) && strValue.GetLength() > 1)
			strValue = strValue.Mid(1);
		strValue = m_strDefaultDir + strValue;
	}
}

//-----------------------------------------------------------------------------
CString CPathEdit::GetValue()
{
	CString	strValue;
	GetValue(strValue);

	return strValue;
}

//-----------------------------------------------------------------------------
BOOL CPathEdit::IsValid()
{               
	if (!__super::IsValid())
		return FALSE;

	CString strPath = GetValue();
	strPath.TrimRight();

	BOOL bIsEmpty = strPath.IsEmpty();
    
	m_nErrorID = EMPTY_MESSAGE;

    if (!bIsEmpty)
    {
		if (strPath.IsEmpty())
			if ((m_dwCtrlStyle & PATH_STYLE_AS_PATH) == PATH_STYLE_AS_PATH)
				m_nErrorID = PATH_EDIT_BAD_PATH;
			else
				m_nErrorID = PATH_EDIT_BAD_FILE;

		if (m_nErrorID)
			return FALSE;
	}
			
	BOOL bCanBeEmpty= (m_dwCtrlStyle & PATH_STYLE_NO_EMPTY) != PATH_STYLE_NO_EMPTY;
	BOOL bCheckExist= (m_dwCtrlStyle & PATH_STYLE_NO_CHECK_EXIST) != PATH_STYLE_NO_CHECK_EXIST;
	               
	if ((m_dwCtrlStyle & PATH_STYLE_AS_PATH) == PATH_STYLE_AS_PATH)
	{
		if (bIsEmpty && !bCanBeEmpty)
			m_nErrorID = PATH_EDIT_EMPTY_PATH;
		else
		{
			// la funzione name_ok in caso di rete da errati i path che non finiscono con slash
			// per evitare di modificare la funzione di bassissimo livello preferisco aggiungere
			// lo slash per validare il path e ritoglierlo successivamente (an 17.751)
			BOOL bSlashAdded = strPath.Right(1) != SLASH_CHAR;
			if (bSlashAdded)
				strPath += SLASH_CHAR;

			if (!bIsEmpty && !::IsPathName(strPath, bCheckExist))
				if (!bCheckExist)
					m_nErrorID = PATH_EDIT_BAD_PATH;
				else
					m_nErrorID = PATH_EDIT_NO_PATH;
			if (bSlashAdded)
				strPath = strPath.Left(strPath.GetLength() - 1);

		}
	}
	else
		if (bIsEmpty && !bCanBeEmpty)
			m_nErrorID = PATH_EDIT_EMPTY_FILE;
		else
			if (!bIsEmpty && !::IsFileName(strPath, bCheckExist))
				if (!bCheckExist)
					m_nErrorID = PATH_EDIT_BAD_FILE;
				else
					m_nErrorID = PATH_EDIT_NO_FILE;

	return m_nErrorID == 0;
}

//-----------------------------------------------------------------------------
static const CString szFileSystemReservedChars	= _T("*?\"<>|");

BOOL CPathEdit::DoOnChar(UINT nChar)
{
	if (szFileSystemReservedChars.Find ((TCHAR) nChar) >= 0)
		return TRUE;	//eat the char

	return FALSE;	//char is accepted

	//accetta solo lettere e numeri: neppure il .
	//if (__super::DoOnChar(nChar))
	//	return TRUE;
	//if	(
	//		_istcntrl(nChar)	|| _istalnum(nChar)	||
	//		nChar == DOT_CHAR	|| nChar == SLASH_CHAR	||
	//		nChar == _T('-')	|| nChar == _T('_')		||
	//		nChar == _T(':')	|| nChar == SHARP_CHAR
	//	)
	//	return FALSE;

	//BadInput();

	//return TRUE;
}

//-----------------------------------------------------------------------------
void CPathEdit::Attach(DataObj* pDataObj)
{
	__super::Attach (pDataObj);	

	if (GetCtrlData())
	{
		GetCtrlData()->SetCollateCultureSensitive (FALSE);

		SetCtrlStyle(STR_STYLE_NUMBERS | STR_STYLE_LETTERS | STR_STYLE_FILESYSTEM);
	}
}

//////////////////////////////////////////////////////////////////////////////
//             class CBrowsePathEdit implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CBrowsePathEdit, CPathEdit)

// ----------------------------------------------------------------------------
CBrowsePathEdit::CBrowsePathEdit()
	:
	CPathEdit(),
	m_bValue		(TRUE),
	m_bUseFileDlg	(FALSE),
	m_lpszDefExt	(NULL),
	m_lpszFileName	(NULL),
	m_dwFlags		(OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT),
	m_pParentWnd	(NULL),
	m_dwSize		(0),
	m_bVistaStyle	(TRUE)
{
	SetCtrlStyle(STR_STYLE_ALL);
	m_lpszFilter = _T("All Files (*.*)|*.*||");
}

// ----------------------------------------------------------------------------
BOOL CBrowsePathEdit::OnInitCtrl()
{
	BOOL bResult = __super::OnInitCtrl();
	if (bResult)
	{
		CStateCtrlObj* pStateCtrl = __super::AttachStateData(&m_bValue);

		CStateCtrlState* pState = pStateCtrl->GetCtrlState(TRUE);
		if (pState)
			pState->Set(TBIcon(szIconFolderFind, CONTROL), TRUE);
		pState = pStateCtrl->GetCtrlState(FALSE);
		if (pState)
			pState->Set(TBIcon(szIconFolderFind, CONTROL), TRUE);
	}
	return bResult;
}

// ----------------------------------------------------------------------------
void CBrowsePathEdit::DoPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	if (GetCtrlData()->IsReadOnly())
		return;

	CString strFileName;
	BOOL bOk;
	
	if (GetUseFileDlg())
	{
		CString aFilePath = GetValue();
		
		if (aFilePath.IsEmpty())
			aFilePath = AfxGetPathFinder()->GetStandardPath() + SLASH_CHAR; 
		else if (PathIsDirectory(aFilePath))
			aFilePath += _T("\\");

		CFileDialog aFileDlg(TRUE, GetExtension() ,aFilePath.IsEmpty() ? GetFilter() : (LPCTSTR)aFilePath, GetFlags(), GetFilterText(), GetParentWnd());
		aFileDlg.m_ofn.lpstrTitle = GetDlgTitle().Str();

		SetUserRunning(TRUE);
		bOk = (aFileDlg.DoModal() == IDOK);
		SetUserRunning(FALSE);

		if (!bOk)
			return;

		strFileName = aFileDlg.GetPathName();
	}
	else
	{
		CString Path = GetValue();
		CChooseDirDlg dlg(Path);

		SetUserRunning(TRUE);
		bOk = (dlg.DoModal() == IDOK);
		SetUserRunning(FALSE);

		if (!bOk)
			return;

		strFileName = dlg.GetSelectedDir();
	}

	*m_pData = DataStr(strFileName);
	SetDataModified(TRUE);
	if (GetDocument())
	{
		GetDocument()->SetModifiedFlag(TRUE);
		GetDocument()->UpdateDataView();
	}
}

//=============================================================================
//			Class CNamespaceEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CNamespaceEdit, CLinkEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CNamespaceEdit, CLinkEdit)

	ON_COMMAND			(ID_PICTURE_SEARCH,		OnSearchPicture)
	ON_COMMAND			(ID_TEXT_SEARCH,		OnSearchText)
	ON_COMMAND			(ID_OTHER_SEARCH,		OnSearchObjectInOthers)
	ON_COMMAND			(ID_REPORT_SEARCH,		OnSearchReport)
	ON_COMMAND			(ID_DOCUMENT_SEARCH,	OnSearchDocument)

	ON_COMMAND			(ID_PDF_SEARCH,			OnSearchPdf)
	ON_COMMAND			(ID_RTF_SEARCH,			OnSearchRtf)
	//ON_COMMAND			(ID_ODF_SEARCH,			OnSearchOdf)

	ON_COMMAND			(ID_BROWSE_FILES,		OnBrowseFiles)
	ON_COMMAND			(ID_BROWSE_LINK,		OnBrowseLink)

END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CNamespaceEdit::CNamespaceEdit()
	:
	CLinkEdit				(),
	m_pPictureStatic		(NULL),
	m_pShowFileTextStatic	(NULL),
	m_pRichCtrl				(NULL),
	m_pWebCtrl				(NULL),
	m_NsType				(CTBNamespace::NOT_VALID),
	m_bIsRunning			(FALSE),
	m_pParam				(NULL),
	m_nDataIdxParam			(-1)
{
}

//-----------------------------------------------------------------------------
CNamespaceEdit::CNamespaceEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CLinkEdit				(nBtnIDBmp, pData),
	m_pPictureStatic		(NULL),
	m_pShowFileTextStatic	(NULL),
	m_pRichCtrl				(NULL),
	m_pWebCtrl				(NULL),
	m_NsType				(CTBNamespace::NOT_VALID),
	m_bIsRunning			(FALSE),
	m_pParam				(NULL),
	m_nDataIdxParam			(-1)
{
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::ReadStaticPropertiesFromJson()
{
	if (!m_pOwnerWndDescription)
		return;
	__super::ReadStaticPropertiesFromJson();
	CString s, sBareText;
	if (m_pOwnerWndDescription->GetValue(szJsonDefaultNamespace, s))
	{
		if (CJsonFormEngineObj::IsExpression(s, sBareText))
			CJsonContext::EvaluateExpression<CString, DataStr>((CAbstractFormDoc*) m_pDocument, sBareText, m_pOwnerWndDescription, s);
		CTBNamespace tbns(s);
		ASSERT(tbns.IsValid());
		SetNamespace(tbns);
	}
	if (m_pOwnerWndDescription->GetValue(szJsonNamespaceType, s))
	{
		if (CJsonFormEngineObj::IsExpression(s, sBareText))
			CJsonContext::EvaluateExpression<CString, DataStr>((CAbstractFormDoc*)m_pDocument, sBareText, m_pOwnerWndDescription, s);
		CTBNamespace::NSObjectType nsType = CTBNamespace::FromString(s);
		SetNamespaceType(nsType);
	}
}
//-----------------------------------------------------------------------------
CString CNamespaceEdit::GetMenuButtonImageNS()
{
	return TBIcon(szIconFolderFind, CONTROL);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::SetNamespaceType(CTBNamespace::NSObjectType NsType)
{
	if 
		(
			NsType == CTBNamespace::IMAGE ||
			NsType == CTBNamespace::TEXT ||
			NsType == CTBNamespace::PDF ||
			NsType == CTBNamespace::RTF ||
			//NsType == CTBNamespace::ODF ||
			NsType == CTBNamespace::FILE ||
			NsType == CTBNamespace::REPORT ||
			NsType == CTBNamespace::DOCUMENT 
		)
		m_NsType = NsType;
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CNamespaceEdit::GetMenuButton (CMenu* pMenu)
{
	BOOL bEmpty = GetCtrlData() == NULL;
	
	if (bEmpty || !GetCtrlData()->IsReadOnly())
	{
		if (m_NsType == CTBNamespace::IMAGE || m_NsType == CTBNamespace::FILE)
			pMenu->AppendMenu(MF_STRING, ID_PICTURE_SEARCH,	_TB("Search for Images..."));

		if (m_NsType == CTBNamespace::TEXT || m_NsType == CTBNamespace::FILE)
			pMenu->AppendMenu(MF_STRING, ID_TEXT_SEARCH,	_TB("Search for Texts..."));

		if (m_NsType == CTBNamespace::PDF || m_NsType == CTBNamespace::FILE)
			pMenu->AppendMenu(MF_STRING, ID_PDF_SEARCH,	_TB("Search for Pdf..."));

		if (m_NsType == CTBNamespace::RTF || m_NsType == CTBNamespace::FILE)
			pMenu->AppendMenu(MF_STRING, ID_RTF_SEARCH,	_TB("Search for Rtf..."));

		//if (m_NsType == CTBNamespace::ODF || m_NsType == CTBNamespace::FILE)
		//	pMenu->AppendMenu(MF_STRING, ID_ODF_SEARCH,	_TB("Search for Odf..."));

		if (
			m_NsType == CTBNamespace::IMAGE || 
			m_NsType == CTBNamespace::TEXT || 
			m_NsType == CTBNamespace::PDF || 
			m_NsType == CTBNamespace::RTF || 
			//m_NsType == CTBNamespace::ODT || 
			//m_NsType == CTBNamespace::ODS || 
			m_NsType == CTBNamespace::FILE
			)
		{	
			pMenu->AppendMenu(MF_STRING, ID_OTHER_SEARCH,	_TB("Search in Other Files..."));
			pMenu->AppendMenu(MF_STRING, ID_BROWSE_FILES,	_TB("Browse Files..."));
		}
	
		if (m_NsType == CTBNamespace::REPORT)
			pMenu->AppendMenu(MF_STRING, ID_REPORT_SEARCH,	_TB("Search Report..."));

		if (m_NsType == CTBNamespace::DOCUMENT)
			pMenu->AppendMenu(MF_STRING, ID_DOCUMENT_SEARCH,	_TB("Choose Document..."));
	}
	
	CString sName;
	CParsedCtrl::GetValue(sName);
	if (sName.IsEmpty())
		return TRUE;

	pMenu->AppendMenu(MF_SEPARATOR);

	if (
			m_NsType == CTBNamespace::FILE	|| 
			m_NsType == CTBNamespace::IMAGE || 
			m_NsType == CTBNamespace::PDF || 
			m_NsType == CTBNamespace::TEXT
		)
	{
		pMenu->AppendMenu(MF_STRING, ID_BROWSE_LINK,	_TB("&Show in registered application..."));
	}
	else if (
			m_NsType == CTBNamespace::REPORT
		)
	{
		pMenu->AppendMenu(MF_STRING, ID_BROWSE_LINK,	_TB("Open..."));
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::DoCmdMenuButton(UINT nUINT)
{
	if (nUINT == ID_PICTURE_SEARCH)		OnSearchPicture();
	else if (nUINT == ID_TEXT_SEARCH)		OnSearchText();
	else if (nUINT == ID_OTHER_SEARCH)		OnSearchObjectInOthers();
	else if (nUINT == ID_REPORT_SEARCH)		OnSearchReport();
	else if (nUINT == ID_DOCUMENT_SEARCH)	OnSearchDocument();
	else if (nUINT == ID_BROWSE_FILES)		OnBrowseFiles();
	else if (nUINT == ID_BROWSE_LINK)		OnBrowseLink();
}
//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchObjectInOthers()
{
	OnSearchObject(CTBNamespace::FILE);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchPicture()
{
	OnSearchObject(CTBNamespace::IMAGE);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchText()
{
	OnSearchObject(CTBNamespace::TEXT);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchPdf()
{
	OnSearchObject(CTBNamespace::PDF);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchRtf()
{
	OnSearchObject(CTBNamespace::RTF);
}
//-----------------------------------------------------------------------------
//void CNamespaceEdit::OnSearchOdf()
//{
//	OnSearchObject(CTBNamespace::ODF);
//}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchReport()
{
	OnSearchObject(CTBNamespace::REPORT);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchObject(CTBNamespace::NSObjectType NsType)
{
	CTBNamespace ns;
	CTBNamespace nsSelected;

	if (NsType == CTBNamespace::NOT_VALID)
	{
		ASSERT(FALSE);
		return;
	}

	ns.SetObjectName(NsType, _T(""), TRUE);

	if (!ns.GetType())
	{
		ASSERT(FALSE);	// non è stato settato il tipo!!
		return;
	}

	if (!m_Ns.IsEmpty())
	{
		ns.SetApplicationName(m_Ns.GetApplicationName());
		ns.SetObjectName(CTBNamespace::MODULE, m_Ns.GetObjectName(CTBNamespace::MODULE));
	}

	// mi tengo il dataobj prima che scatti la perdita di fuoco 
	// (nel bodyedit scatta la HideCtrl che rimette il prototipo vd. an #20.863) 
	DataObj* pBoundDataObj = m_pData;

	ITBExplorer* myTbExp = AfxCreateTBExplorer(ITBExplorer::OPEN, ns);
	if (!myTbExp)
	{
		ASSERT(FALSE);
		return;
	}

	m_bIsRunning = TRUE;

	if (!myTbExp->Open())
	{
		delete myTbExp;
		m_bIsRunning = FALSE;
		return;
	}

	CStringArray arSelectedFiles; 
	myTbExp->GetSelNameSpace(nsSelected);
	delete myTbExp;

	CString strNs = nsSelected.ToString();

	//Aggiorna il parsed control con il nuovo path
	ForceUpdateCtrlView(TRUE);
	if (pBoundDataObj)
		pBoundDataObj->Assign(strNs);
	
	SetModifyFlag(TRUE);
	if (GetDocument())
		GetDocument()->SetModifiedFlag(TRUE);

	UpdateCtrlView();

	m_bIsRunning = FALSE;

	ModifiedCtrlData();

	CWnd* pParent = GetParent();
	if (pParent && pParent->IsKindOf(RUNTIME_CLASS(CGridControl)))
		pParent->Invalidate();

	if (m_pPictureStatic)
	{
		m_pPictureStatic->ClearCtrl();
		m_pPictureStatic->SetValue(strNs);
		m_pPictureStatic->Invalidate();

		SetFocus();
	}
	
	if (m_pShowFileTextStatic)
	{
		m_pShowFileTextStatic->SetValue(strNs);
		m_pShowFileTextStatic->Invalidate();

		SetFocus();
	}
}

///<summary>
///Permette di ricercare i file del tipo associato al namespaceEdit sui drive di rete e locali.
///Nel caso si selezionino file di percorsi locali viene avvertito l'utente: i file potrebbero non essere accessibili da altri client.
///</summary>
//-----------------------------------------------------------------------------
void CNamespaceEdit::OnBrowseFiles()
{
	CString			strExt;
	CString			strFilter;
	
	switch (m_NsType)
	{
		case CTBNamespace::IMAGE:
			strExt		= FileExtension::BMP_EXT();
			strFilter	= FileExtension::BMP_FILTER();
			break;
		case CTBNamespace::TEXT:
			strExt		= FileExtension::CSV_EXT();
			strFilter	= FileExtension::TXT_FILTER();
			break;
		case CTBNamespace::PDF:
			strExt		= FileExtension::PDF_EXT();
			strFilter	= FileExtension::PDF_FILTER();
			break;
		case CTBNamespace::RTF:
			strExt		= FileExtension::RTF_EXT();
			strFilter	= FileExtension::RTF_FILTER();
			break;
		case CTBNamespace::ODT:
			strExt		= FileExtension::ODT_EXT();
			strFilter	= FileExtension::ODT_FILTER();
			break;
		case CTBNamespace::ODS:
			strExt		= FileExtension::ODS_EXT();
			strFilter	= FileExtension::ODS_FILTER();
			break;
	}

	CFileDialog dlg(TRUE, strExt, _T(""), OFN_HIDEREADONLY, strFilter);
	
	m_bIsRunning = TRUE;
	// mi tengo il dataobj prima che scatti la perdita di fuoco 
	// (nel bodyedit scatta la HideCtrl che rimette il prototipo vd. an #20.863) 
	DataObj* pBoundDataObj = m_pData;

	SetUserRunning(TRUE);
	BOOL bOk = dlg.DoModal() == IDOK;
	SetUserRunning(FALSE);

	if (bOk)
	{
		CString sPath = dlg.GetPathName();
		//Se il percroso non e' un percorso di rete, ma locale, si avverte l'utente che da altri client il file "linkato"
		//potrebbe non essere accessibile
		if (!IsServerPath(sPath) && AfxMessageBox(_TB("Selected file could not be accessed from other clients. In order to be accessed from all clients file must be located on a network folder. Do you want to continue anyway?"), MB_YESNO | MB_ICONEXCLAMATION) != IDYES)
		{
			m_bIsRunning = FALSE;
			return;
		}

		//Aggiorna il parsed control con il nuovo path
		ForceUpdateCtrlView(TRUE);
		if (pBoundDataObj) pBoundDataObj->Assign(sPath);
		SetModifyFlag(TRUE);
		if (GetDocument()) GetDocument()->SetModifiedFlag(TRUE);
		UpdateCtrlView();
	}

	m_bIsRunning = FALSE;

	CWnd* pParent = GetParent();
	if (pParent && pParent->IsKindOf(RUNTIME_CLASS(CGridControl)))
		pParent->Invalidate();
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::OnSearchDocument()
{
	CBaseDocumentExplorerDlg* pDocExplorer = AfxGetTBExplorerFactory()->CreateDocumentExplorerDlg();
	pDocExplorer->m_bFilterXmlDescrition = FALSE;

	m_bIsRunning = TRUE;
	// mi tengo il dataobj prima che scatti la perdita di fuoco 
	// (nel bodyedit scatta la HideCtrl che rimette il prototipo vd. an #20.863) 
	DataObj* pBoundDataObj = m_pData;

	SetUserRunning(TRUE);
	BOOL bOk = (pDocExplorer->DoModal() == IDOK);
	SetUserRunning(FALSE);

	if (bOk)
	{
		//Aggiorna il parsed control con il nuovo path
		ForceUpdateCtrlView(TRUE);
		pBoundDataObj->Assign(pDocExplorer->m_FullNameSpace);
		SetModifyFlag(TRUE);
		if (GetDocument())
			GetDocument()->SetModifiedFlag(TRUE);
		UpdateCtrlView();
	}

	m_bIsRunning = FALSE;

	CWnd* pParent = GetParent();
	if (pParent && pParent->IsKindOf(RUNTIME_CLASS(CGridControl)))
		pParent->Invalidate();

	SAFE_DELETE(pDocExplorer);
}

//-----------------------------------------------------------------------------
BOOL CNamespaceEdit::GetToolTipProperties	(CTooltipProperties& tp)
{
	CString strNamespace;
   	CParsedCtrl::GetValue(strNamespace);
	if (strNamespace.IsEmpty())
		return FALSE;

	if (IsDosName(strNamespace, TRUE))
	{
		tp.m_strText = strNamespace;
		return TRUE;
	}

	CTBNamespace Ns;
	Ns.SetNamespace(strNamespace);

	CString strFullPathName = (AfxGetPathFinder())->GetFileNameFromNamespace(Ns, AfxGetLoginInfos()->m_strUserName);
	if (strFullPathName.IsEmpty())
		return FALSE;

	tp.m_strTitle = strNamespace;
	tp.m_strText = strFullPathName;
	return TRUE;
}


//-----------------------------------------------------------------------------
void CNamespaceEdit::OnBrowseLink ()
{
	CString str;
	CParsedCtrl::GetValue(str);
	if (str.IsEmpty())
		return;
	CTBNamespace Ns;
	Ns.SetNamespace(str);

	if (m_NsType == CTBNamespace::REPORT || (Ns.IsValid() && Ns.GetType() == CTBNamespace::REPORT))
	{
		AfxGetTbCmdManager()->RunWoormReport(str, NULL, FALSE, FALSE);
		return;
	}
	if (m_NsType == CTBNamespace::DOCUMENT && Ns.IsValid() && Ns.GetType() == CTBNamespace::DOCUMENT)
	{
		AfxGetTbCmdManager()->RunDocument(Ns.ToString());
		return;
	}

	BOOL bOk = IsDosName(str, TRUE);
	if (!bOk)
	{
		str = (AfxGetPathFinder())->GetFileNameFromNamespace(Ns, AfxGetLoginInfos()->m_strUserName);
	}

	HINSTANCE hInst = ::TBShellExecute(str);
	if (hInst <= (HINSTANCE)32)
		AfxMessageBox (ShellExecuteErrMsg((int)hInst));
}

//-----------------------------------------------------------------------------
BOOL CNamespaceEdit::IsValid ()
{ 
	BOOL bOk = CParsedEdit::IsValid();
	if (!bOk) return FALSE;
	CString strNamespace;
	CParsedCtrl::GetValue(strNamespace);
	if (IsValidNamespace(strNamespace) || ExistFile(strNamespace))
		return TRUE;
	m_nErrorID = NAMESPACE_EDIT_BAD_NAMESPACE;
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CNamespaceEdit::IsValidNamespace(CString& strNamespace)
{
	if (strNamespace.IsEmpty())
		return	TRUE;

	CTBNamespace ns(strNamespace);
	if (!ns.IsValid())
	{
		return FALSE;
	}
	if (m_NsType == CTBNamespace::DOCUMENT && ns.GetType() == CTBNamespace::DOCUMENT)
	{
		return TRUE;
	}

	CString strFileName = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);

	// il file non esiste per questo utente
	return ExistFile(strFileName);
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::Attach(DataObj* pDataObj)
{
	__super::Attach (pDataObj);	

	if (GetCtrlData())
	{
		GetCtrlData()->SetCollateCultureSensitive (FALSE);
	}
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::AttachPicture(CPictureStatic* pPictureStatic)
{
	if (pPictureStatic == NULL || m_NsType != CTBNamespace::IMAGE)
	{
		ASSERT(FALSE);
		return;
	}

	//Per evitare ambiguità nel caso siano collegati control di tipo Picture a namespaceedit collegati allo stesso dataobj
	//sulla AttachPicture aggiungiamo postfisso al namespace del picturestatic per disambiguare il namespace del control.
	if (this->GetNamespace() == pPictureStatic->GetNamespace())
	{
		CString objName = this->GetNamespace().GetObjectName();
		objName+= _T("_Img");
		pPictureStatic->GetNamespace().SetObjectName(objName, TRUE);
	}
	m_pPictureStatic = pPictureStatic;	
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::AttachFileText(CShowFileTextStatic* pShowFileTextStatic)
{
	if (pShowFileTextStatic == NULL || m_NsType != CTBNamespace::TEXT)
	{
		ASSERT(FALSE);
		return;
	}
	m_pShowFileTextStatic = pShowFileTextStatic;	
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::AttachRichCtrl (CParsedRichCtrl* pRichCtrl)
{
	if (pRichCtrl == NULL || m_NsType != CTBNamespace::FILE)
	{
		ASSERT(FALSE);
		return;
	}
	m_pRichCtrl = pRichCtrl;	
}

//-----------------------------------------------------------------------------
void CNamespaceEdit::AttachWebCtrl (CParsedWebCtrl* pWebCtrl)
{
	if (pWebCtrl == NULL || m_NsType != CTBNamespace::FILE)
	{
		ASSERT(FALSE);
		return;
	}
	m_pWebCtrl = pWebCtrl;	
}

//=============================================================================
//			Class CPictureStatic
//=============================================================================
IMPLEMENT_DYNCREATE (CPictureStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CPictureStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CPictureStatic)
	ON_WM_CONTEXTMENU		()
	ON_WM_PAINT				()
	ON_WM_ERASEBKGND		()
	ON_WM_WINDOWPOSCHANGING	()

	ON_COMMAND				(ID_PARSEDT_CTRL_MENU_BEST,			OnCtrlStyleBest)
	ON_COMMAND				(ID_PARSEDT_CTRL_MENU_NORMAL,		OnCtrlStyleNormal)
	ON_COMMAND				(ID_PARSEDT_CTRL_MENU_HORIZONTAL,	OnCtrlStyleHorizontal)
	ON_COMMAND				(ID_PARSEDT_CTRL_MENU_VERTICAL,		OnCtrlStyleVertical)
	ON_MESSAGE				(UM_RECALC_CTRL_SIZE,				OnRecalcCtrlSize)
	
	ON_COMMAND				(ID_BROWSE_LINK,					OnShowInOtherEditor)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPictureStatic::CPictureStatic()
	:
	CParsedStatic	(),
	m_bValid		(FALSE)
{
	m_pPicture		= new CTBPicture(CTBPicture::ImageFitMode::NORMAL);
}

//-----------------------------------------------------------------------------
CPictureStatic::CPictureStatic(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_bValid		(FALSE)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	m_pPicture		= new CTBPicture(CTBPicture::ImageFitMode::NORMAL);
}

//-----------------------------------------------------------------------------
CPictureStatic::~CPictureStatic()	
{
	SAFE_DELETE(m_pPicture);
}

//-----------------------------------------------------------------------------
LRESULT CPictureStatic::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndImageDescription* pDesc = (CWndImageDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId);
	pDesc->UpdateAttributes(this);

	if (m_pPicture->ImageIsValid())
	{
		CString sName = cwsprintf(_T("pic%ud_%ud.png"), m_hWnd, GetHashCode(m_pPicture->GetFileName()));
		if (pDesc->m_ImageBuffer.Assign(m_pPicture->GetImage(), sName))
			pDesc->SetUpdated(&pDesc->m_ImageBuffer);
	}
	//else
	//{	//se l'immagine non e' piu valida, e' come se fosse stata cancellata
	//	pDesc->SetRemoved();
	//}

	return (LRESULT) pDesc;
}
//-----------------------------------------------------------------------------
BOOL CPictureStatic::OnInitCtrl()	
{
	VERIFY(CParsedCtrl::OnInitCtrl());
	if (GetCtrlData())
		SetValue(GetCtrlData()->Str());
	return TRUE;
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	__super::OnWindowPosChanging(wndPos);
}

//-----------------------------------------------------------------------------
//per gestire ownerdraw delegato al parsed control di una cella del bodyedit
BOOL CPictureStatic::OwnerDraw (CDC* pDC, CRect& rect, DataObj* /*= NULL*/)
{ 
	DrawBitmap(*pDC, rect);
	return TRUE; 
} 

//-----------------------------------------------------------------------------
void CPictureStatic::DrawBitmap (CDC& dcDest, const CRect& rect)
{
	CWnd* pParent = GetParent();
	if (pParent)
	{
		CBaseTileDialog* pTile = dynamic_cast<CBaseTileDialog*>(pParent);
		if (pTile) {
			dcDest.FillSolidRect(&rect, pTile->GetBackgroundColor());
		}
	}

	if (!m_pPicture->IsOk())
	{
		return;
	}

	dcDest.SetBkMode(TRANSPARENT);						
	CRect rectCopy(rect);
	CRect rectSrc(CPoint(0, 0), rect.Size());
	CRect rcBars;
	rcBars.SetRect
		(
			1,
			1,
			rectCopy.right  - 1,
			rectCopy.bottom - 1
		);

	if (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::NORMAL)
	{
		m_pPicture->DrawPicture(dcDest, rectCopy, rectSrc);
		return;
	}

	m_pPicture->FitImage(rectCopy, rectSrc);

	if (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::HORIZONTAL || m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::VERTICAL)
		dcDest.IntersectClipRect(rcBars);

	//m_pPicture->m_ImageFitMode == CTBPicture::BEST
	m_pPicture->DrawPicture(dcDest, rectCopy);
}

//-----------------------------------------------------------------------------
void CPictureStatic::SetValue(CTBPicture*	pPicture)
{
	m_pPicture = pPicture;

	if (IsTBWindowVisible(this) && GetCtrlData())
		CPictureStatic::SetValue(GetCtrlData()->Str());
}

//-----------------------------------------------------------------------------
void CPictureStatic::SetValue(LPCTSTR pszValue/* = NULL */)
{
	if (m_strCurrValue.CompareNoCase(pszValue) == 0)
	{
		Invalidate();
		return;
	}
	m_strCurrValue = pszValue;

	m_pPicture->Clear();

	CParsedCtrl::SetValue(pszValue); 

	BOOL bReadOk = TRUE;
	CString	strMsg;
	CString strFullPath;

	ShowWindow(SW_NORMAL);

	if (!m_strCurrValue.IsEmpty())
	{
		if (IsDosName(m_strCurrValue, TRUE))
			strFullPath = m_strCurrValue;
		else
		{
			CTBNamespace ns;
			ns.SetNamespace(m_strCurrValue);

			strFullPath = (AfxGetPathFinder())->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		}

		m_bValid =  !strFullPath.IsEmpty() && (bReadOk = m_pPicture->ReadFile(strFullPath, TRUE));		
		
		strMsg.Format(_T("%s \n%s: %s"), _TB("Preview not available."), _TB("File"), m_strCurrValue);
	}
	else
		m_bValid = TRUE;

	if (!m_bValid)
	{
		ShowError(strMsg);
		return;
	}

	Invalidate();	
}

//-----------------------------------------------------------------------------
void CPictureStatic::ShowError(const CString& strMsg)
{
	if (m_strCurrValue.IsEmpty() || strMsg.IsEmpty())
		return;

	// specific paint
	CClientDC DC(this);
	CRect rect; GetClientRect(rect);
	rect.InflateRect(+3, +1);

	// set color, mode, fonts
	int			old_mode  = DC.SetBkMode(TRANSPARENT);
	
	COLORREF	old_color = DC.SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());
	UINT		nOldStyle = DC.SetTextAlign (DT_CENTER);
	
	CSize cs = GetMultilineTextSize(&DC, AfxGetThemeManager()->GetControlFont(), strMsg);
	int y = rect.top + (rect.Height() - cs.cy / 2);
	// make Draw

	DC.DrawText (strMsg, strMsg.GetLength(), rect, 0);
	
	DC.SetTextAlign (nOldStyle);
	DC.SetBkMode	(old_mode);
	DC.SetTextColor	(old_color);
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{	
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CParsedStatic::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
BOOL CPictureStatic::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	this->GetClientRect(rclientRect);

	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());
	if (pParsedForm)
	{
		if (m_bColored)
		{
			CBrush brk(m_crBkgnd);
			pDC->FillRect(&rclientRect, &brk);
		}
		else
		{
			CBrush* pBrush = pParsedForm->GetBackgroundBrush();
			if (pBrush)
				pDC->FillRect(&rclientRect, pBrush);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	CRect rect; GetClientRect(rect);
	CDC* pDC = GetDC();
	
	if (pDC && m_bValid && HasEdgeStyle() && rect.Height() > 0 && rect.Width() > 0)
	{
		DrawBitmap (*pDC, rect);
	}
	ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnCtrlStyleNormal()
{
	m_pPicture->m_ImageFitMode = CTBPicture::ImageFitMode::NORMAL;
	Invalidate();
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnCtrlStyleBest()
{
	m_pPicture->m_ImageFitMode = CTBPicture::ImageFitMode::BEST;
	Invalidate();
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnCtrlStyleHorizontal()
{
	m_pPicture->m_ImageFitMode = CTBPicture::ImageFitMode::HORIZONTAL;
	Invalidate();
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnCtrlStyleVertical()
{
	m_pPicture->m_ImageFitMode = CTBPicture::ImageFitMode::VERTICAL;
	Invalidate();
}
	
//------------------------------------------------------------------------------
LRESULT CPictureStatic::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CPictureStatic::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName)
{
	if (!CParsedStatic::SubclassDlgItem(IDC, pParent))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);
	SetNamespace(strName);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CPictureStatic::OnShowingPopupMenu(CMenu& menu)
{
	CParsedStatic::OnShowingPopupMenu(menu);

	//TODO aggiungere controlli su validita namespace
	if (!m_bValid)
		return TRUE;

	menu.AppendMenu
		(
			MF_STRING , 
			ID_BROWSE_LINK,
			(LPCTSTR) _TB("&Show in registered application...")
		);	

	menu.AppendMenu(MF_SEPARATOR);
	
	UINT nChecked = (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::NORMAL)
		? MF_CHECKED 
		: MF_UNCHECKED;
	menu.AppendMenu
		(
			MF_STRING | nChecked, 
			ID_PARSEDT_CTRL_MENU_NORMAL,
			(LPCTSTR) _TB("&Show original dimension")
		);

	nChecked = (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::BEST)
		? MF_CHECKED 
		: MF_UNCHECKED;
	menu.AppendMenu
		(
			MF_STRING | nChecked, 
			ID_PARSEDT_CTRL_MENU_BEST,			
			(LPCTSTR) _TB("&Fit to best")
		);

	nChecked = (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::HORIZONTAL)
		? MF_CHECKED 
		: MF_UNCHECKED;
	menu.AppendMenu
		(
			MF_STRING | nChecked, 
			ID_PARSEDT_CTRL_MENU_HORIZONTAL,
			(LPCTSTR) _TB("&Horizontal fit")
		);

	nChecked = (m_pPicture->m_ImageFitMode == CTBPicture::ImageFitMode::VERTICAL)
		? MF_CHECKED 
		: MF_UNCHECKED;
	menu.AppendMenu
		(
			MF_STRING | nChecked, 
			ID_PARSEDT_CTRL_MENU_VERTICAL,
			(LPCTSTR) _TB("&Vertical fit")
		);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CPictureStatic::OnShowInOtherEditor ()
{
	if (m_strCurrValue.IsEmpty())
		return;

	CString str;
	BOOL bOk = IsDosName(str, TRUE);
	if (!bOk)
	{
		CTBNamespace Ns;
		Ns.SetNamespace(m_strCurrValue);
		str = (AfxGetPathFinder())->GetFileNameFromNamespace(Ns, AfxGetLoginInfos()->m_strUserName);
	}

	HINSTANCE hInst = ::TBShellExecute(str);
	if (hInst <= (HINSTANCE)32)
		ASSERT(FALSE);
}
	
//-----------------------------------------------------------------------------
void CPictureStatic::Attach(DataObj* pDataObj)
{
	CParsedCtrl::Attach (pDataObj);	

	if (GetCtrlData())
	{
		GetCtrlData()->SetCollateCultureSensitive (FALSE);
		SetCtrlStyle(STR_STYLE_NUMBERS | STR_STYLE_LETTERS | STR_STYLE_FILESYSTEM);
	}
}

//=============================================================================
//			Class CShowFileTextStatic
//=============================================================================
IMPLEMENT_DYNCREATE (CShowFileTextStatic, CParsedEdit)

BEGIN_MESSAGE_MAP(CShowFileTextStatic, CParsedEdit)
	//{{AFX_MSG_MAP(CShowFileTextStatic)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CShowFileTextStatic::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName)
{
	if (! __super::SubclassEdit(IDC, pParent, strName))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);
	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CShowFileTextStatic::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
CShowFileTextStatic::CShowFileTextStatic()
	:
	CParsedEdit	()
{
	SetCtrlStyle(STR_STYLE_ALL);
	CParsedCtrl::Attach(this);	
}

//-----------------------------------------------------------------------------
CShowFileTextStatic::CShowFileTextStatic(UINT nBtnIDBmp, DataStr* pData /* = NULL */ )
	:
	CParsedEdit		(nBtnIDBmp)
{
	SetCtrlStyle(STR_STYLE_ALL);
	Attach(pData);
}

//-----------------------------------------------------------------------------
void CShowFileTextStatic::Attach(DataObj* pDataObj)
{
	ASSERT(pDataObj == NULL || pDataObj->GetDataType() == GetDataType());
	CParsedCtrl::Attach(pDataObj);
	if (GetCtrlData())
	{
		GetCtrlData()->SetCollateCultureSensitive (FALSE);
		SetCtrlStyle(STR_STYLE_NUMBERS | STR_STYLE_LETTERS |STR_STYLE_FILESYSTEM);
	}
}

//-----------------------------------------------------------------------------
BOOL CShowFileTextStatic::OnInitCtrl()
{
	VERIFY(CParsedEdit::OnInitCtrl());
	return TRUE;
}

//-----------------------------------------------------------------------------
void CShowFileTextStatic::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	SetValue (((DataStr&)aValue).GetString());
}

//-----------------------------------------------------------------------------
void CShowFileTextStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataStr &)aValue).Assign(m_strFullPath);
}

//-----------------------------------------------------------------------------
void CShowFileTextStatic::SetValue(LPCTSTR strValue /* = NULL */)
{
	CTBNamespace ns;
	ns.SetNamespace(strValue);

	m_strFullPath = (AfxGetPathFinder())->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
	
	if (!m_strFullPath.IsEmpty())
	{
		if (!ExistFile(m_strFullPath))
		{
			// visualizza nello static la stringa non valida
			CParsedEdit::SetValue(*GetCtrlData());
			return;
		}
	}
	else
		m_strFullPath.Empty();

	if (IsTBWindowVisible(this))
		CParsedEdit::SetValue(*GetCtrlData());

	CRect rect; GetClientRect(rect);
	rect.InflateRect(-1, -1);

	CClientDC DC(this);
	if (!m_strFullPath.IsEmpty())
		DrawTextFile (DC, rect);
	SetReadOnly();
	return;
}

//-----------------------------------------------------------------------------
void CShowFileTextStatic::DrawTextFile (CDC& DC, CRect& inside)
{
	CString strText = LoadTextFile (m_strFullPath);
	SetWindowText(strText);
}

//------------------------------------------------------------------------------
CString CShowFileTextStatic::LoadTextFile (const CString& strFileName)
{
	CString strText;
	LoadLineTextFile (strFileName, strText);
	return strText;
}

//=============================================================================
//			Class CParsedRichCtrl
//=============================================================================
IMPLEMENT_DYNCREATE (CParsedRichCtrl, CRichEditCtrl)

BEGIN_MESSAGE_MAP(CParsedRichCtrl, CRichEditCtrl)

	ON_WM_CTLCOLOR_REFLECT	()

	ON_MESSAGE				(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)

	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,	OnPushButtonCtrl)

	ON_WM_KILLFOCUS			()
	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_ENABLE			()

	ON_WM_CHAR				()
	ON_WM_KEYUP				()
	ON_WM_KEYDOWN			()

	ON_WM_RBUTTONDOWN		()
//	ON_WM_LBUTTONUP			()
	ON_WM_LBUTTONDOWN		()
	ON_WM_NCMOUSEMOVE		()
	ON_WM_MOUSEMOVE			()

	//ON_WM_SETFOCUS		()

	ON_NOTIFY_REFLECT		(EN_LINK, OnLinkNotify)

END_MESSAGE_MAP()

#pragma warning(disable:4355) // disabilita la warning sull'uso del this del parent
//-----------------------------------------------------------------------------
CParsedRichCtrl::CParsedRichCtrl()
	:
	CColoredControl	(this),
	m_bBrowsingLink		(FALSE),
	m_bIsExternalFile	(FALSE)
{
	SetCtrlStyle(STR_STYLE_ALL);
	CParsedCtrl::Attach(this);	
}
#pragma warning(default:4355)

//-----------------------------------------------------------------------------
void CParsedRichCtrl::SetIsExternalFile(BOOL bSet/*=TRUE*/)
{
	m_bIsExternalFile = bSet;
	SetEditReadOnly (bSet);
}

//-----------------------------------------------------------------------------
BOOL CParsedRichCtrl::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk = 
		CheckControl(nID, pParentWnd, _T("RichEdit20A"))	&&
		SubclassDlgItem(nID, pParentWnd)			&&
		CParsedCtrl::CreateAssociatedButton(pParentWnd)			&& 
		OnInitCtrl();
	
	if (m_hWnd)
		ResizableCtrl::InitSizeInfo (this);

	SetNamespace(strName);
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CParsedRichCtrl::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk =
		//CheckControl(nID, pParentWnd, _T("RichEdit20A"))		&&
		CRichEditCtrl::Create(dwStyle, rect, pParentWnd, nID)	&&
		CParsedCtrl::CreateAssociatedButton(pParentWnd)			&& 
		OnInitCtrl();

	if (m_hWnd)
		ResizableCtrl::InitSizeInfo(this);

	return bOk;

}

//----------------------------------------------------------------------------
BOOL CParsedRichCtrl::CreateEx(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, const CString& strName)
{
	BOOL bOk = Create(dwStyle, rect, pParentWnd, nID);

	SetNamespace(strName);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CParsedRichCtrl::CheckControl(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName /* = NULL */)
{
	ASSERT(m_pButton == NULL);
	ASSERT(!m_bAttached);   
	ASSERT(CheckDataObjType());

	if (m_nFormatIdx == UNDEF_FORMAT)
	{
		AddOnModule* pAddOnMod = NULL;
		CWnd* pParent = pParentWnd;
		BOOL bNoOtherContext = FALSE;

		// devo controllare in quale contesto mi trovo, se in quello dell'AddOnApplication
		// oppure in quello globale di applicazione
		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))			
		{
			m_pFormatContext = &((CBaseFormView*)pParent)->GetNamespace();
			bNoOtherContext = ((CBaseFormView*)pParent)->IsDisableOtherContext();
		}
		else 
		{
			if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
			{
				m_pFormatContext = &((CParsedDialog*)pParent)->GetNamespace();
				bNoOtherContext = ((CParsedDialog*)pParent)->IsDisableOtherContext();
			}
			else
			{
				CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pParent);
				if (pGrid)
				{
					m_pFormatContext = &pGrid->GetNamespace();
					bNoOtherContext = pGrid->IsDisableOtherContext();
				}
			}
		}

		m_nFormatIdx = AfxGetFormatStyleTable()->GetFormatIdx(GetDataType());
	}

	m_bAttached = TRUE;

#ifdef _DEBUG
	CWnd* pWndCtrl = pParentWnd->GetDlgItem(nID);

	if (ctrlClassName)
	{
		ASSERT(pWndCtrl);
		
		TCHAR className[MAX_CLASS_NAME + 1];
		ASSERT(::GetClassName(pWndCtrl->m_hWnd, className, MAX_CLASS_NAME));
		/*
		if (_tcsicmp(className, ctrlClassName) != 0)
		{
			TRACE
				(
					"Tentativo di subclassing di un control %s sul control %s ID = %u\n",
					(LPCTSTR) m_pOwnerWnd->GetRuntimeClass()->m_lpszClassName,
					className, nID
				);
			ASSERT(FALSE);
			return FALSE;
		}
		*/
	}
	else
		if (pWndCtrl)
		{
			TRACE
				(
					"Tentativo di ri-creazione di un control %s ID = %u\n",
					(LPCTSTR) CParsedCtrl::m_pOwnerWnd->GetRuntimeClass()->m_lpszClassName, nID
				);
			ASSERT(FALSE);
			return FALSE;
		}
#endif
	
	return TRUE;
}
//-----------------------------------------------------------------------------
void CParsedRichCtrl::Attach(DataObj* pDataObj)
{
	ASSERT(pDataObj == NULL || pDataObj->GetDataType() == GetDataType());
	__super::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
BOOL CParsedRichCtrl::OnInitCtrl()
{
	VERIFY(__super::OnInitCtrl());

	//  abilita il notify degli url
	DWORD prevMsk = GetEventMask();
	SetEventMask(prevMsk | ENM_LINK |ENM_CHANGE); 
	//	controlla il contenuto per trovare gli url	
	LRESULT result = SendMessage(EM_AUTOURLDETECT, TRUE, 0);

	return TRUE;
}

// gestisce lo stato di ReadOnly per la parte editabile del control
//-----------------------------------------------------------------------------
void CParsedRichCtrl::SetEditReadOnly (const BOOL bValue)
{
	BOOL bIsReadOnly = IsEditReadOnly();

	// lo è già in stato corretto
	//if ((bIsReadOnly && bValue) || (!bIsReadOnly && !bValue))
	//	return;

	// la finestra non è abilitata e ma devo farlo
	if (!IsWindowEnabled ()) //simmetrica //fix ? && bValue) //originale && !bValue)
		EnableWindow(TRUE);

	__super::SetReadOnly(bValue);

	if (m_pButton)
		DoEnable(!bValue);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	if (m_bIsExternalFile)
		aValue.Assign(m_strFullPath);
	else
	{
		CString strBuffer;
		CParsedCtrl::GetValue(strBuffer);
		aValue.Assign(strBuffer);
	}
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CParsedCtrl::SetValue (aValue.Str());
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::SetValue(LPCTSTR strValue /* = NULL */)
{
	if (!m_bIsExternalFile)
	{
		CParsedCtrl::SetValue(strValue);
		return;
	}

	CTBNamespace ns;
	ns.SetNamespace(strValue);

	m_strFullPath = (AfxGetPathFinder())->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
	
	if (!m_strFullPath.IsEmpty())
	{
		if (!ExistFile(m_strFullPath))
		{
			// visualizza la stringa non valida
			__super::SetValue(strValue);
			return;
		}
	}
	else
		m_strFullPath.Empty();

	if (IsTBWindowVisible(this))
		__super::SetValue(GetCtrlData()->Str());

	CRect rect; GetClientRect(rect);
	rect.InflateRect(-1, -1);

	CClientDC DC(this);
	if (!m_strFullPath.IsEmpty())
		DrawTextFile (DC, rect);

	SetReadOnly();
	return;
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::DrawTextFile (CDC& DC, CRect& inside)
{
	CString strText = LoadTextFile (m_strFullPath);
	SetWindowText(strText);
}

//------------------------------------------------------------------------------
CString CParsedRichCtrl::LoadTextFile (const CString& strFileName)
{
	CString strText;
	LoadLineTextFile (strFileName, strText);
	return strText;
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnKillFocus (CWnd* pWnd)
{
	CParsedCtrl::DoKillFocus(pWnd);

	__super::OnKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
    // Corregge la dimensione del control per integrare nell'area, disegnata
    // con il dialog editor, anche l'eventuale bottone
    // NB. il flag SWP_NOACTIVATE viene usato in particolare nel caso di control
    // creato all'interno del BodyEdit: in tal caso no si deve operare alcuna
    // correzione in quanto il control e` gia` dimensionato correttamente
    //
    if (!m_pButton || (wndPos->flags & SWP_NOACTIVATE) == SWP_NOACTIVATE)
    	return;

	CRect rectEdit	(CPoint(wndPos->x, wndPos->y), CSize(wndPos->cx, wndPos->cy));
	
	// calculate button coordinates (one pixel between edit and button)
	CRect	rectBtn(0,0,0,0);
	int		btnWidth;

	CRect wndRect;
	GetWindowRect(wndRect);
	GetCtrlParent()->ScreenToClient(wndRect);

	if ((wndPos->flags & SWP_NOMOVE) == SWP_NOMOVE)
	{
		btnWidth = GetButtonWidth(rectEdit.Height());

		rectBtn.left	= wndRect.left + rectEdit.Width() - btnWidth;
		rectBtn.top		= wndRect.top;
		rectBtn.right	= wndRect.left + rectEdit.Width();
		rectBtn.bottom	= wndRect.bottom;
	}
	else
		if ((wndPos->flags & SWP_NOSIZE) == SWP_NOSIZE)
		{
			btnWidth		= GetButtonWidth(wndRect.Height());

			rectBtn.left	= rectEdit.left + wndRect.Width() + BTN_OFFSET;
			rectBtn.top		= rectEdit.top;
			rectBtn.right	= rectBtn.left + btnWidth;
			rectBtn.bottom	= wndRect.bottom;
		}
		else
		{
			btnWidth = GetButtonWidth(rectEdit.Height());
	
			rectBtn.left	= rectEdit.right - btnWidth;
			rectBtn.top		= rectEdit.top;
			rectBtn.right	= rectEdit.right;
			rectBtn.bottom	= rectEdit.bottom;
		}	
		
	SetButtonPos(rectBtn, wndPos->flags);

	rectEdit.right	= rectEdit.right - btnWidth - BTN_OFFSET;
	wndPos->x		= rectEdit.left;
	wndPos->y		= rectEdit.top;
	wndPos->cx		= rectEdit.Width();
	wndPos->cy		= rectEdit.Height();

	SetHyperLinkPos(rectEdit, wndPos->flags);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnEnable(BOOL bEnable)
{
	__super::OnEnable(bEnable);
	
	CParsedCtrl::DoEnable(bEnable);
}

//----------------------------------------------------------------------------
void CParsedRichCtrl::SetCtrlMaxLen(UINT nLen, BOOL bApplyNow)
{
	m_nCtrlLimit = nLen;
	
	if (bApplyNow)
		__super::LimitText(nLen);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (!DoKeyUp(nChar))
		__super::OnKeyUp(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (!DoKeyDown(nChar))
		__super::OnKeyDown(nChar, nRepCnt, nFlags);
}		

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (!DoOnChar(nChar))
		CRichEditCtrl::OnChar(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
LRESULT CParsedRichCtrl::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::SetModifyFlag(BOOL bFlag)
{ 
	CParsedCtrl::SetModifyFlag(bFlag);

	SetModify(bFlag);
}
	
//-----------------------------------------------------------------------------
BOOL CParsedRichCtrl::GetModifyFlag()			
{ 
	if (CParsedCtrl::GetModifyFlag())
		return TRUE;
		
	return GetModify();
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	if (!CParsedCtrl::DoRButtonDown(nFlag, mousePos))
		__super::OnRButtonDown(nFlag, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnLButtonDown(UINT nFlags, CPoint point) 
{
	CRichEditCtrl::OnLButtonDown(nFlags, point);

	//	controllo che abbia cliccato su un url
	if (!m_bBrowsingLink)
		return;

	CString szComando;

	//	"esegue" l'url, windows in automatico trova il programma di default e lo lancia
	HINSTANCE hInst = ::TBShellExecute(m_strLink);
	if (hInst <= (HINSTANCE)32)
		AfxMessageBox(ShellExecuteErrMsg((int)hInst));

	m_bBrowsingLink = FALSE;
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnNcMouseMove(UINT nHitTest, CPoint point) 
{
	//	quando il mouse va fuori al controllo e poi ci clicca dentro non devo fare nulla
	m_bBrowsingLink = FALSE;
	
	CRichEditCtrl::OnNcMouseMove(nHitTest, point);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnMouseMove(UINT nFlags, CPoint point) 
{
	if (m_bBrowsingLink)
		m_bBrowsingLink = FALSE;
		
	CRichEditCtrl::OnMouseMove(nFlags, point);
}

//-----------------------------------------------------------------------------
void CParsedRichCtrl::OnLinkNotify ( NMHDR* nmhdr, LRESULT* pResult)
{
	*pResult = 0L;

	//	Contiene le informazioni del messaggio ricevuto e le coordinate della stringa url
	ENLINK* lnk = (ENLINK*) nmhdr;
	if(!lnk)
		return ;

	GetWindowText(m_strLink);

	if (m_strLink.GetLength() == 0)
		return;
	//	stringa url
	m_strLink = m_strLink.Mid(lnk->chrg.cpMin, (lnk->chrg.cpMax - lnk->chrg.cpMin));
	m_bBrowsingLink = TRUE;
}

//=============================================================================
//			Class CParsedWebCtrl
//=============================================================================

BEGIN_MESSAGE_MAP(CWebBrowser, CWnd)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------
BOOL CWebBrowser::Create(const RECT& rect, CWnd* pParentWnd, UINT nID, LPCTSTR lpszUrl /*= L""*/)
{
	BOOL bOk = ::CreateChildBrowser(pParentWnd->m_hWnd, lpszUrl, TRUE, this, &rect);
	if (bOk && m_pBrowser && m_pBrowser->GetMainWnd())
		Attach(m_pBrowser->GetMainWnd());
	return bOk;
}
//-------------------------------------------------------------------------------
CWebBrowser::CWebBrowser()
{
}

//-------------------------------------------------------------------------------
CWebBrowser::~CWebBrowser()
{
}


//-------------------------------------------------------------------------------
void CWebBrowser::Navigate(LPCTSTR lpszUrl)
{
	if (m_pBrowser && lpszUrl && lpszUrl[0])
		m_pBrowser->Navigate(lpszUrl);
}
//-------------------------------------------------------------------------------
void CWebBrowser::Refresh()
{
	if (m_pBrowser)
		m_pBrowser->Reload();
}
//-------------------------------------------------------------------------------
void CWebBrowser::OnAfterCreated(CBrowserObj* pBrowser)
{
	m_pBrowser = pBrowser;

}
//-------------------------------------------------------------------------------
void CWebBrowser::OnBeforeClose(CBrowserObj* pBrowser)
{
	m_pBrowser = NULL;
}

//-------------------------------------------------------------------------------
void CWebBrowser::AdjustPosition(int cx, int cy)
{
	if (cx == 0 && cy == 0)
		return;
	HWND hwnd = m_pBrowser ? m_pBrowser->GetMainWnd() : NULL;
	if (hwnd)
	{
		::SetWindowPos(hwnd, NULL, 0, 0, cx, cy, SWP_NOACTIVATE | SWP_NOZORDER | SWP_DRAWFRAME);
		Paint();
	}
}

//-------------------------------------------------------------------------------
void CWebBrowser::Paint()
{
	HWND hwnd = GetMainWindow();
	if (hwnd)
	{
		hwnd = ::GetWindow(hwnd, GW_CHILD);
		::InvalidateRect(hwnd, NULL, TRUE);
		//::UpdateWindow(hwnd);
	}
}
//-------------------------------------------------------------------------------
HWND CWebBrowser::GetMainWindow()
{
	return m_pBrowser ? m_pBrowser->GetMainWnd() : NULL;
}

//=============================================================================
//			Class CParsedWebCtrl
//=============================================================================
IMPLEMENT_DYNCREATE (CParsedWebCtrl, CWebBrowser)

BEGIN_MESSAGE_MAP(CParsedWebCtrl, CWebBrowser)

	ON_MESSAGE (UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	ON_WM_PAINT()

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedWebCtrl::CParsedWebCtrl()
{
	CParsedCtrl::Attach(this);	
}

//-----------------------------------------------------------------------------
void CParsedWebCtrl::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	Paint();
}
//-----------------------------------------------------------------------------
BOOL CParsedWebCtrl::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk = 
		CheckControl(nID, pParentWnd)					&&
		SubclassDlgItem(nID, pParentWnd)				&&
		OnInitCtrl();
	
	if (m_hWnd)
		ResizableCtrl::InitSizeInfo (this);

	SetNamespace(strName);
#ifdef DEBUG
	TCHAR szClassName[MAX_CLASS_NAME+1];
	GetClassName(m_hWnd, szClassName, MAX_CLASS_NAME);
	ASSERT(_tcsicmp(szClassName, L"Button") == 0);
#endif // DEBUG
	ModifyStyle(0, WS_CLIPCHILDREN|BS_OWNERDRAW);
	CreateChildBrowser(m_hWnd, m_pData ? m_pData->FormatData() : _T(""), TRUE, this);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CParsedWebCtrl::Create(DWORD, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk =	
		CheckControl(nID, pParentWnd) &&
		CWebBrowser::Create(rect, pParentWnd, nID, (LPCTSTR) (m_pData ? m_pData->FormatData() : _T(""))) &&
		OnInitCtrl();
	
	if (m_hWnd)
		ResizableCtrl::InitSizeInfo(this);

	return bOk;
}

//----------------------------------------------------------------------------
BOOL CParsedWebCtrl::CreateEx(const RECT& rect, CWnd* pParentWnd, UINT nID, const CString& strName)
{
	BOOL bOk = Create(0/*dummy*/, rect, pParentWnd, nID);

	SetNamespace(strName);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CParsedWebCtrl::CheckControl(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName /* = NULL */)
{
	ASSERT(m_pButton == NULL);
	ASSERT(!m_bAttached);   
	ASSERT(CheckDataObjType());

	m_bAttached = TRUE;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedWebCtrl::Attach(DataObj* pDataObj)
{
	ASSERT(pDataObj == NULL || pDataObj->GetDataType() == GetDataType());
	CParsedCtrl::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
BOOL CParsedWebCtrl::OnInitCtrl()
{
	return __super::OnInitCtrl();
}


//-----------------------------------------------------------------------------
LRESULT	CParsedWebCtrl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize(); 

	CRect aRect;
	GetClientRect(aRect);
	AdjustPosition(aRect.right - aRect.left, aRect.bottom - aRect.top);

	return 0L; 
}

//-----------------------------------------------------------------------------
void CParsedWebCtrl::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CString strBuffer;
	CParsedCtrl::GetValue(strBuffer);
	aValue.Assign(strBuffer);
}

//-----------------------------------------------------------------------------
void CParsedWebCtrl::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	CString strNew = aValue.Str();
	SetValue((LPCTSTR)strNew);
}

//-----------------------------------------------------------------------------
void CParsedWebCtrl::SetValue(LPCTSTR strValue /* = NULL */)
{
	CString strCurrent;
	CParsedCtrl::GetValue(strCurrent);
	if (strCurrent.CompareNoCase(strValue) == 0)
		return;
	CParsedCtrl::SetValue(strValue);
	Navigate(strValue);
}

#define CPN_SELCHANGE        WM_USER + 1001        // Colour Picker Selection change
#define CPN_SELENDOK         WM_USER + 1004        // Colour Picker end OK
#define CPN_SELENDCANCEL     WM_USER + 1005        // Colour Picker end (cancelled)
//=============================================================================
//			Class CColorEdit
//=============================================================================
IMPLEMENT_DYNCREATE (CColorEdit, CLongEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CColorEdit, CLongEdit)
	//ON_MESSAGE	(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	
	//ON_CONTROL_REFLECT_EX(BN_CLICKED, OnClicked)
    ON_MESSAGE(CPN_SELENDOK,     OnSelEndOK)
    ON_MESSAGE(CPN_SELENDCANCEL, OnSelEndCancel)
    ON_MESSAGE(CPN_SELCHANGE,    OnSelChange)

END_MESSAGE_MAP()

CColorEdit::CColorEdit ()
	:
//	m_bActive (FALSE),
	m_bTrackSelection (FALSE),
	m_bIsRunning(FALSE)
{ 
	SetColored(TRUE); 
	DisableSelection();
}

//-----------------------------------------------------------------------------
void CColorEdit::Attach (UINT /*nBtnID*/)
{ 
	m_nButtonIDBmp = BTN_COLOR_ID; 
}

//-----------------------------------------------------------------------------
void CColorEdit::Attach (DataObj* pData)
{ 
	CParsedCtrl::Attach(pData);
}

//-----------------------------------------------------------------------------
void CColorEdit::SetValue (COLORREF rgb)
{
	SetBkgColor(rgb);

	if (IsWindowEnabled())
		SetTextColor(::OppositeRGB(rgb));
	else
		SetTextColor(rgb);

	m_lCurValue = (long)rgb;

	CString s; s.Format(L"%d", rgb);

	CParsedCtrl::SetValue((LPCTSTR)s);
}


//-----------------------------------------------------------------------------
void CColorEdit::SetValue (const DataObj& aValue)
{
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataLng)));
	const DataLng* pDl = (DataLng*) &aValue;
	COLORREF rgb = (long)(*pDl);
	
	SetValue(rgb);
}

//-----------------------------------------------------------------------------
BOOL CColorEdit::GetToolTipProperties (CTooltipProperties& tp)
{ 
	DataLng color;
	GetValue(color);
	COLORREF rgb = (long) color;

	tp.m_strText = cwsprintf(_T("RGB(%d, %d, %d): %d"), GetRValue(rgb), GetGValue(rgb), GetBValue(rgb), rgb);

	return TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CColorEdit::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	BOOL bOk = __super::EnableCtrl(bEnable);
	__super::SetEditReadOnly(!bEnable);

	DataLng color;
	GetValue(color);
	COLORREF rgb = (long) color;

	if (bEnable)
		SetTextColor(::OppositeRGB(rgb));
	else
		SetTextColor(rgb);

	return bOk;
}

/////////////////////////////////////////////////////////////////////////////
// CColourPicker message handlers

LONG CColorEdit::OnSelEndOK(UINT lParam, LONG /*wParam*/)
{
  // m_bActive = FALSE;
	COLORREF crNewColour = (COLORREF) lParam;
	DataLng dl = crNewColour;

	SetValue(dl);

	SetModifyFlag(TRUE);
	UpdateCtrlData(TRUE);
	SetFocus();

 /*   CWnd *pParent = GetParent();
    if (pParent) {
        pParent->SendMessage(CPN_CLOSEUP, lParam, (WPARAM) GetDlgCtrlID());
        pParent->SendMessage(CPN_SELENDOK, lParam, (WPARAM) GetDlgCtrlID());
    }

    if (crNewColour != GetBkgColor())
        if (pParent) pParent->SendMessage(CPN_SELCHANGE, lParam, (WPARAM) GetDlgCtrlID());*/

    return TRUE;
}

LONG CColorEdit::OnSelEndCancel(UINT lParam, LONG /*wParam*/)
{
   //m_bActive = FALSE;
    SetBkgColor((COLORREF) lParam);

 //   CWnd *pParent = GetParent();
 //   if (pParent)
	//{
 //       pParent->SendMessage(CPN_CLOSEUP, lParam, (WPARAM) GetDlgCtrlID());
 //       pParent->SendMessage(CPN_SELENDCANCEL, lParam, (WPARAM) GetDlgCtrlID());
 //   }

	return TRUE;
}

LONG CColorEdit::OnSelChange(UINT lParam, LONG /*wParam*/)
{
    if (m_bTrackSelection) 
		SetBkgColor((COLORREF) lParam);


    //CWnd *pParent = GetParent();
    //if (pParent) pParent->SendMessage(CPN_SELCHANGE, lParam, (WPARAM) GetDlgCtrlID());

    return TRUE;
}

