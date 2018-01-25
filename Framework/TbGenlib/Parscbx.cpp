#include "stdafx.h"

#include <float.h>
#include <ctype.h>

#include <TBNameSolver\Chars.h>

#include <TbParser\SymTable.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbWoormEngine\INPUTMNG.H>

#include "MicroareaVisualManager.h"
#include "messages.h"
#include "Baseapp.h"
#include "BaseTileDialog.h"
#include "parsobj.h"
#include "HyperLink.h"
#include "basedoc.h"
#include "hlinkobj.h"
#include "parscbx.h"
#include "parsedt.h"
#include "TBToolBar.h"
#include "TBPropertyGrid.h"

// resources
#include "parsres.hjson" //JSON AUTOMATIC UPDATE
#include "commands.hrc"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//*****************************************************************************
#define DEFAULT_COMBO_ROWS	30
#define	HIT_SELENDOK	0x0001
#define	FORCING_CLOSEUP	0x0002
#define	FILLING_LISTBOX	0x0004

//-----------------------------------------------------------------------------
static int nIsServer2008 = 0;

//-----------------------------------------------------------------------------
BOOL IsServer2008()
{
	if (nIsServer2008 == 0)
	{
		// Is windows server 2008 ?
		OSVERSIONINFOEXW osvi = { sizeof(osvi), 0, 0, 0, 0,{ 0 }, 0, 0 };
		DWORDLONG        const dwlConditionMask = VerSetConditionMask(
			VerSetConditionMask(
				VerSetConditionMask(
					0, VER_MAJORVERSION, VER_GREATER_EQUAL),
				VER_MINORVERSION, VER_GREATER_EQUAL),
			VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);

		osvi.dwMajorVersion = 6;
		osvi.dwMinorVersion = 0;
		osvi.wServicePackMajor = 0;

		nIsServer2008 = VerifyVersionInfoW(&osvi, VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR, dwlConditionMask) != FALSE ? 1 : 2;
	}

	return nIsServer2008 == 1;
}

//=============================================================================
//			Class CParsedCombo implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedCombo, CBCGPComboBox)

BEGIN_MESSAGE_MAP(CParsedCombo, CBCGPComboBox)
	ON_WM_KILLFOCUS			()
	ON_WM_SETFOCUS			()
	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_VSCROLL			()     // for associated spin controls
	ON_WM_LBUTTONUP			()
	ON_WM_RBUTTONDOWN		()
	ON_WM_CONTEXTMENU		()
	ON_WM_CHAR				()
	ON_WM_KEYUP				()
	ON_WM_KEYDOWN			()
	ON_WM_ENABLE			()

	ON_COMMAND_RANGE		(ID_FORMAT_STYLE_MENU_CMD_0, (UINT)(ID_FORMAT_STYLE_MENU_CMD_19), OnFormatPopupMenuItemSelected)

	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,			OnPushButtonCtrl)
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
	ON_MESSAGE				(UM_CHECK_DROPDOWN,				OnCheckDropDown)
	ON_MESSAGE				(CB_GETCURSEL,					OnGetCurSel)

	ON_WM_CTLCOLOR			()
	ON_WM_CTLCOLOR_REFLECT	()
	ON_WM_PAINT				()

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedCombo::CParsedCombo(DataObj* pData /*= NULL*/)
	:
	CBCGPComboBox				(),
	CParsedCtrl					(pData),
	CColoredControl				(this),
	CCustomFont					(this),
	IDisposingSourceImpl		(this),
	m_pOldChildListBoxWndProc	(NULL),
	m_pOldChildEditWndProc		(NULL),
	m_wComboStatus				(0),
	m_sizeListBox				(0, 0),
	m_nMaxRowsNo				(DEFAULT_COMBO_ROWS),
	m_nMaxItemsNo				(DEFAULT_COMBO_ITEMS),
	m_nMaxItemsLen				(-1),
	m_pReadOnlyEdit				(NULL),
	m_bFillListBoxEmpty			(FALSE),
	m_pManagedFillComboFuncPtr	(NULL),
	m_bSorted					(FALSE),
	m_bShowHKLDescription		(FALSE),
	m_bNoResetAssociations		(FALSE)
{
	CParsedCtrl::Attach (this);	
	CParsedCtrl::AttachCustomFont (this);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CParsedCombo::CParsedCombo(UINT nBtnIDBmp, DataObj* pData /*=NULL*/)
	:
	CBCGPComboBox				(),
	CParsedCtrl					(pData),
	CColoredControl				(this),
	CCustomFont					(this),
	IDisposingSourceImpl		(this),
	m_pOldChildListBoxWndProc	(NULL),
	m_pOldChildEditWndProc		(NULL),
	m_wComboStatus				(0),
	m_sizeListBox				(0, 0),
	m_nMaxRowsNo				(DEFAULT_COMBO_ROWS),
	m_nMaxItemsNo				(DEFAULT_COMBO_ITEMS),
	m_nMaxItemsLen				(-1),
	m_pReadOnlyEdit				(NULL),
	m_bFillListBoxEmpty			(FALSE),
	m_pManagedFillComboFuncPtr	(NULL),
	m_bSorted					(FALSE),
	m_bShowHKLDescription		(FALSE),
	m_bNoResetAssociations		(FALSE)
{
	CParsedCtrl::Attach (this);
	CParsedCtrl::Attach (nBtnIDBmp);
	CParsedCtrl::AttachCustomFont (this);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}
                        
//-----------------------------------------------------------------------------
CParsedCombo::~CParsedCombo()
{
	SAFE_DELETE(m_pReadOnlyEdit);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
LRESULT CALLBACK InternalChildListBoxWndProc(HWND hWnd, UINT nMsg, WPARAM wParam, LPARAM lParam)
{
	// special message which identifies the window as using AfxWndProc
	if (nMsg == WM_QUERYAFXWNDPROC)
		return 1;

	// all other messages route through message map
	CWnd* pWnd = CWnd::FromHandlePermanent(hWnd);
	CBCGPDropDownListBox* pLBChild = dynamic_cast<CBCGPDropDownListBox*>(pWnd);
	CParsedCombo* pCombo = NULL;

	if (pLBChild)
		pCombo = dynamic_cast<CParsedCombo*>(pLBChild->m_pWndCombo);

	if (pCombo)
	{
		LRESULT res = -1;
		switch (nMsg)
		{
			case LB_FINDSTRING			: res = pCombo->OnLBFindString(wParam, lParam); break;
			case LB_FINDSTRINGEXACT		: res = pCombo->OnLBFindStringExact(wParam, lParam); break;
			case LB_GETTEXT				: res = pCombo->OnLBGetText(wParam, lParam); break;
			case LB_GETTEXTLEN			: res = pCombo->OnLBGetTextLen(wParam, lParam); break;
			case WM_WINDOWPOSCHANGING	: res = pCombo->OnLBWindowPosChanging((LPWINDOWPOS) lParam); break;
		}

		if (res >= 0)
			return res;
	}

	if (pCombo && pCombo->m_pOldChildListBoxWndProc)
	{
		WNDPROC lpfnproc = pCombo->m_pOldChildListBoxWndProc;

		if (nMsg == WM_NCDESTROY)
			pCombo->UnSubclassChildListBox();

		return CallWindowProc(lpfnproc, hWnd, nMsg, wParam, lParam);
	}

	return 0;
}
//-----------------------------------------------------------------------------
long CParsedCombo::GetMaxItemsNo() 
{
	return m_pItemSource ? m_pItemSource->GetMaxItemsNo() : m_nMaxItemsNo; 
}
//-----------------------------------------------------------------------------
BOOL CParsedCombo::SubclassChildListBox()
{
	// now hook into the AFX WndProc
	m_pOldChildListBoxWndProc = reinterpret_cast<WNDPROC>(::SetWindowLongPtr(m_wndList.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&InternalChildListBoxWndProc)));
	return (m_pOldChildListBoxWndProc && m_pOldChildListBoxWndProc != &InternalChildListBoxWndProc);
}

//-----------------------------------------------------------------------------
void CParsedCombo::UnSubclassChildListBox()
{
	ASSERT(::IsWindow(m_wndList.m_hWnd));
	ASSERT(::IsWindow(m_hWnd));

	// set WNDPROC back to original value
	SetWindowLongPtr(m_wndList.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(m_pOldChildListBoxWndProc));
	m_pOldChildListBoxWndProc = NULL;
}

//-----------------------------------------------------------------------------
LRESULT CALLBACK InternalChildEditWndProc(HWND hWnd, UINT nMsg, WPARAM wParam, LPARAM lParam)
{
	// special message which identifies the window as using AfxWndProc
	if (nMsg == WM_QUERYAFXWNDPROC)
		return 1;

	// all other messages route through message map
	CWnd* pWnd = CWnd::FromHandlePermanent(hWnd);
	CBCGPEdit* pEditChild = dynamic_cast<CBCGPEdit*>(pWnd);
	CParsedCombo* pCombo = NULL;

	if (pEditChild)
		pCombo = dynamic_cast<CParsedCombo*>(pEditChild->m_pAutoCompleteCombo);

	if (pCombo)
	{
		LRESULT res = -1;
		switch (nMsg)
		{
			case WM_KEYUP		:	res = pCombo->OnEditKeyUp((UINT)(wParam), (int)(short)LOWORD(lParam), (UINT)HIWORD(lParam)); break;
			case WM_KEYDOWN		:	res = pCombo->OnEditKeyDown((UINT)(wParam), (int)(short)LOWORD(lParam), (UINT)HIWORD(lParam)); break;
			case WM_CHAR		:	res = pCombo->OnEditChar((UINT)(wParam), (int)(short)LOWORD(lParam), (UINT)HIWORD(lParam)); break;
			case WM_KILLFOCUS	:	res = pCombo->OnEditKillFocus(CWnd::FromHandle((HWND)(wParam))); break;
			case WM_SETFOCUS	:	res = pCombo->OnEditSetFocus(CWnd::FromHandle((HWND)(wParam))); break;
			case WM_RBUTTONDOWN	:	res = pCombo->OnEditRButtonDown((UINT)(wParam), CPoint((int)(short)LOWORD(lParam), (int)(short)HIWORD(lParam))); break;
			case WM_CONTEXTMENU	:	res = pCombo->OnEditContextMenu(CWnd::FromHandlePermanent((HWND)(wParam)), CPoint((int)(short)LOWORD(lParam), (int)(short)HIWORD(lParam))); break;
			case WM_PAINT		:	res = pCombo->OnEditPaint(); break;
		}

		if (res >= 0)
			return res;
	}

	if (pCombo && pCombo->m_pOldChildEditWndProc)
	{
		WNDPROC lpfnproc = pCombo->m_pOldChildEditWndProc;

		if (nMsg == WM_NCDESTROY)
			pCombo->UnSubclassChildEdit();

		return CallWindowProc(lpfnproc, hWnd, nMsg, wParam, lParam);
	}

	return 0;
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::SubclassChildEdit()
{
	// Viene utilizzata il comportamento standard delle combo dei BCG legano Edit della combo con ComboBox stessa
	// per fare questo è stato reso pubblico il data member m_pAutoCompleteCombo dei BCG per potervi accedere dalla InternalChildEditWndProc di cui sopra

	// si backuppano i valori attuali che servono per forzare alla CBCGPComboBox::SubclassEditBox() il link tra i due oggetti
	BOOL bVisualManagerStyle = m_bVisualManagerStyle;
	BOOL bEditVisualManagerStyle = m_wndEdit.m_bVisualManagerStyle;
	BOOL bAutoComplete = m_bAutoComplete;

	// Link tra i due oggetti
	m_bVisualManagerStyle = TRUE;
	m_bAutoComplete = TRUE;
	CBCGPComboBox::SubclassEditBox();
	
	// ripristino valore precedenti
	m_bVisualManagerStyle = bVisualManagerStyle;
	m_wndEdit.m_bVisualManagerStyle = bEditVisualManagerStyle;
	m_bAutoComplete = bAutoComplete;

	if (GetChildEdit() == NULL)
		return FALSE;

	// now hook into the AFX WndProc
	m_pOldChildEditWndProc = reinterpret_cast<WNDPROC>(::SetWindowLongPtr(m_wndEdit.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&InternalChildEditWndProc)));
	return (m_pOldChildEditWndProc && m_pOldChildEditWndProc != &InternalChildEditWndProc);
}

//-----------------------------------------------------------------------------
void CParsedCombo::UnSubclassChildEdit()
{
	ASSERT(::IsWindow(m_wndEdit.m_hWnd));
	ASSERT(::IsWindow(m_hWnd));

	// set WNDPROC back to original value
	SetWindowLongPtr(m_wndEdit.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(m_pOldChildEditWndProc));
	m_pOldChildEditWndProc = NULL;
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedCombo::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	dwStyle |= CBS_AUTOHSCROLL | WS_VSCROLL;
	
	CRect wndRect(rect);

	BOOL bEmptyRect = wndRect.IsRectNull();
	CDC* pDC = pParentWnd->GetDC();
	int h = GetEditSize(pDC, pParentWnd->GetFont(), 1, 1).cy;

	if (bEmptyRect)
	{
		ASSERT(m_nMaxRowsNo > 1);
		wndRect.right = 20;
		m_sizeListBox.cy = GetEditSize(pDC, pParentWnd->GetFont(), 1, m_nMaxRowsNo).cy;
		wndRect.bottom = m_sizeListBox.cy + h;
	}
	else
	{
		m_sizeListBox.cy = wndRect.Height() - h;
		int nRows = (m_sizeListBox.cy - V_EXTRA_EDIT_PIXEL) / (h - V_EXTRA_EDIT_PIXEL);
		if (nRows <= 1)
		{
			ASSERT(m_nMaxRowsNo > 1);
			m_sizeListBox.cy = GetEditSize(pDC, pParentWnd->GetFont(), 1, m_nMaxRowsNo).cy;
			wndRect.bottom = m_sizeListBox.cy + h;
		}
		else
			m_nMaxRowsNo = nRows;
	}
	pParentWnd->ReleaseDC(pDC);

	if (!CheckControl(nID, pParentWnd))
		return FALSE;

	BOOL bOk = CBCGPComboBox::Create(dwStyle, wndRect, pParentWnd, nID);

	this->m_bVisualManagerStyle = TRUE;

	return	bOk				&&
		SubclassChilds()	&&
		CreateAssociatedButton(pParentWnd) &&
		InitCtrl();
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedCombo::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	if (
		!CheckControl(nID, pParentWnd, _T("COMBOBOX")) ||
		!SubclassDlgItem(nID, pParentWnd) ||
		!SubclassChilds() ||
		!CreateAssociatedButton(pParentWnd) ||
		!InitCtrl()
		)
		return FALSE;

	CDC* pDC = pParentWnd->GetDC();
	if ((GetStyle() & CBS_STYLES) != CBS_SIMPLE)
	{
		CRect wndRect;
		GetDroppedControlRect(wndRect);
		CRect wndRectEdit;
		GetWindowRect(wndRectEdit);
		wndRect.bottom -= wndRectEdit.Height();
		pParentWnd->ScreenToClient(wndRect);

		m_sizeListBox.cy = wndRect.Height();

		int h = GetEditSize(pDC, pParentWnd->GetFont(), 1, 1).cy - V_EXTRA_EDIT_PIXEL;
		int nRows = (int)(((float)(m_sizeListBox.cy - V_EXTRA_EDIT_PIXEL) / h) + 0.5);
		if (nRows <= 1)
		{
			m_sizeListBox.cy = GetEditSize(pDC, pParentWnd->GetFont(), 1, m_nMaxRowsNo).cy;

			// segnalo che apparira` senz'altro una scrollbar verticale
			// (vedi OnLBWindowPosChanging(...))
			m_nMaxRowsNo = 0;
		}
		else
			m_nMaxRowsNo = nRows;
	}

	pParentWnd->ReleaseDC(pDC);

	// di default setto il delete in caso di dropdownlist
	if (!IsKindOf(RUNTIME_CLASS(CEnumCombo)) && (GetStyle() & CBS_STYLES) == CBS_DROPDOWNLIST)
		SetCtrlStyle(GetCtrlStyle() | COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL);

	SetNamespace(strName);
	return TRUE;
}

// Subclassa, se presenti l'Edit Control la ComboListBox della ComboBox.
// Questa gestione è necessaria per i due seguenti problemi:
// - resizing della tendina sulla base di un nr. di item dinamici
// - la selezione di un item usa l'intero testo contenuto nell'item, mentre
//   normalmente facciamo ritornare solo il valore del codice m_Associations
//-----------------------------------------------------------------------------
BOOL CParsedCombo::SubclassChilds()
{
	ASSERT(m_hWnd);

	SubclassChildEdit();
	SubclassChildListBox();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCombo::UnSubclassChilds()
{
	if (m_pOldChildEditWndProc)
		UnSubclassChildEdit();

	if (m_pOldChildListBoxWndProc)
		UnSubclassChildListBox();
}

//-----------------------------------------------------------------------------
HWND CParsedCombo::UnSubclassEdit()
{
	UnSubclassChilds();
	return __super::UnSubclassEdit();
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	return DoKeyUp(nChar) ? 0 : -1;
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	return DoKeyDown(nChar) ? 0 : -1;
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// per eliminare il beep dopo la selezione a tendina aperta
	if (nChar == VK_RETURN)
		return 0;

	if (!DoOnChar(nChar))
	{
		CallWindowProc(m_pOldChildEditWndProc, m_wndEdit.m_hWnd, WM_CHAR, MAKEWPARAM(nChar,0), MAKELPARAM(nRepCnt, nFlags));
		if (_istcntrl(nChar))
		{
			UINT key = nChar | 0x40;
			// Solo CTRL-H, CTRL-V, CTRL-X, CTRL-Z provocano il value changed 
			if (key != 0x48 && key != 0x56 && key != 0x58 && key != 0x5A)
				return 0;
		}

		SetModifyFlag(TRUE);
	}

	if (_istcntrl(nChar))
		return 0;

	// migl. #2772 il control non è più ES_UPPERCASE, ma si definisce
	// il tipo di UpperCase da applicare sulla base del DataObj
	if ((m_dwCtrlStyle & STR_STYLE_UPPERCASE) == STR_STYLE_UPPERCASE)
	{
		CString sText;
		m_wndEdit.GetWindowText(sText);
		DWORD nSel = m_wndEdit.GetSel();
		AfxGetCultureInfo()->MakeUpper(sText);

		m_wndEdit.SetWindowText(sText);
		SetModifyFlag(TRUE);
		m_wndEdit.SetSel(nSel);
	}

	return 0;
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditKillFocus(CWnd* pWnd)
{
	DoKillFocus(pWnd);

	return -1;
}
//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditSetFocus(CWnd* pWnd)
{
	DoSetFocus(pWnd);

	return -1;
}
//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditRButtonDown(UINT nFlag, CPoint mousePos)
{
	return DoRButtonDown(nFlag, mousePos) ? 0 : -1;
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditContextMenu(CWnd*, CPoint mousePos)
{
	return DoContextMenu(this, mousePos) ? 0 : -1;
}

//-----------------------------------------------------------------------------
HBRUSH CParsedCombo::OnEditCtlColor(CDC* pDC, UINT nCtlColor)
{
	if (m_wndEdit.IsWindowEnabled() && !(GetStyle() & ES_READONLY))
	{
		pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
		if (AfxGetThemeManager()->IsFocusedControlBkgColorEnabled() && m_wndEdit.m_hWnd == ::GetFocus())
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetFocusedControlBkgColor());
			return (HBRUSH)AfxGetThemeManager()->GetFocusedControlBkgColorBrush()->GetSafeHandle();
		}
		else
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
			return (HBRUSH)AfxGetThemeManager()->GetEnabledControlBkgColorBrush()->GetSafeHandle();
		}
	}

	CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
	if (pManager)
		pDC->SetTextColor(pManager->GetControlForeColor(this, IsWindowEnabled()));
	else 
		pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());

	pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
	return (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnEditPaint()
{
	if (m_wndEdit.IsWindowEnabled() || m_pHyperLink || !m_pDocument || !m_pDocument->UseEasyReading())
		return -1;

	CDC* pDC = m_wndEdit.GetDC();
	if (pDC)
	{
		if (m_wndEdit.IsWindowEnabled())
		{
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
			pDC->SetBkColor(HasFocus() ? AfxGetThemeManager()->GetFocusedControlBkgColor() : AfxGetThemeManager()->GetEnabledControlBkgColor());
		}
		else
		{
			CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
			if (pManager)
				pDC->SetTextColor(pManager->GetControlForeColor(this, IsWindowEnabled()));
			else
				pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());
			pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
		}

	    HGDIOBJ old = pDC->SelectObject(AfxGetThemeManager()->GetControlFont());
		CString strText;
		m_wndEdit.GetWindowText(strText);

		RECT aRect;
		m_wndEdit.GetClientRect(&aRect);
		RECT invRect = aRect;

		if (m_wndEdit.IsWindowEnabled())
			pDC->FillRect(&aRect, const_cast<CBrush*>(HasFocus() ? AfxGetThemeManager()->GetFocusedControlBkgColorBrush() : AfxGetThemeManager()->GetEnabledControlBkgColorBrush()));
		else
			pDC->FillRect(&aRect, const_cast<CBrush*>(AfxGetThemeManager()->GetBackgroundColorBrush()));

		DWORD	dwTextStyle;
		LONG	lStyles = ::GetWindowLong(m_wndEdit.m_hWnd, GWL_STYLE);
		// if the text is multiline, set automatic word break. 
		if ((lStyles & ES_MULTILINE) == ES_MULTILINE)
			dwTextStyle = DT_WORDBREAK;
		else
			dwTextStyle = 0;

		aRect.left += 2;
		pDC->DrawText(strText, &aRect, dwTextStyle | DT_NOPREFIX);
		pDC->SelectObject(old);
		m_wndEdit.ReleaseDC(pDC);
		m_wndEdit.ValidateRect(&invRect);
	}
	
	return -1;
}

//-----------------------------------------------------------------------------
int CParsedCombo::DichotomousSearch(const CString& str, int nStart, int nEnd)
{
	int med;
	if (nStart > nEnd)
		return -1;

	med = (nStart + nEnd) / 2;

	GetAssociatedText(med, m_pszStr);
	int res = _tcsicmp(m_pszStr, str);
	if (res == 0)
		return med;

	if (res > 0)
		return DichotomousSearch(str, med + 1, nEnd);

	return DichotomousSearch(str, nStart, med - 1);
}

// per gestire il corretto posizionamento della selezione nella tendina
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::InternalFindString(WPARAM wParam, LPARAM lParam)
{
	CString lp((LPCTSTR)lParam);
	if (IsShowDescription())
	{
		int idx = ::CStringArray_Find(m_DescriptionsBuffer, lp);
		if (idx >= 0)
			return idx;
	}

	if (m_nMaxItemsLen <= 0)
		return -1;

	m_pszStr = new TCHAR[m_nMaxItemsLen + 1];

	int nIdx = -1;

	// se è sorted allora una stringa vuota o di soli blanck è la prima
	if (m_bSorted)
		if (lp.Trim() == _T(""))
		{
			GetAssociatedText(0, m_pszStr);
			lp = m_pszStr;
			nIdx = lp.Trim() == _T("") ? 0 : -1;
		}
		else
			nIdx = DichotomousSearch(lp, 0, m_DataAssociations.GetUpperBound());
	else
		for (int i = 0; i <= m_DataAssociations.GetUpperBound(); i++)
		{
			GetAssociatedText(i, m_pszStr);
			if (_tcsicmp(m_pszStr, lp) == 0)
				nIdx = i;
		}

	if (m_pszStr)
		delete[]m_pszStr;

	m_pszStr = NULL;
	return nIdx;
}

// per ottenere la stringa completa visualizzata nella tendina della combo (per descrizione web delle finestre)
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::GetLBTextMFC(int index, CString& strText)
{
	int len = GetLBTextLen(index);
	if (len < 0)
		return -1;

	LPTSTR buffer = strText.GetBufferSetLength(len + 1);
	LPARAM lParam = (LPARAM)(LPTSTR)buffer;

	LRESULT	res = -1;
	if (m_pOldChildListBoxWndProc)
		res = m_pOldChildListBoxWndProc(m_wndList, LB_GETTEXT, (WPARAM)index, lParam);

	strText.ReleaseBuffer();
	return res;
}

// per gestire il corretto posizionamento della selezione nella tendina
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnLBFindString(WPARAM wParam, LPARAM lParam)
{
	return InternalFindString(wParam, lParam);
}

// per gestire il corretto posizionamento della selezione nella tendina
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnLBFindStringExact(WPARAM wParam, LPARAM lParam)
{
	return InternalFindString(wParam, lParam);
}

// per gestire la corretta visualizzazione del dato nell'edit associato
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnLBGetText(WPARAM wParam, LPARAM lParam)
{
	int idx = (int)wParam;
	LPTSTR pszStr = (LPTSTR)lParam;
	if (m_pItemSource && m_pItemSource->GetShowDescription())
	{
		if (idx < 0 || idx > m_DescriptionsBuffer.GetUpperBound())
			return -1;
		CString s = m_DescriptionsBuffer.GetAt(idx);
		TB_TCSCPY(pszStr, (LPCTSTR)s);
		return s.GetLength();
	}
	return GetAssociatedText(idx, pszStr);
}

// per gestire la corretta visualizzazione del dato nell'edit associato
//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnLBGetTextLen(WPARAM wParam, LPARAM lParam)
{
	int idx = (int)wParam;
	if (m_pItemSource && m_pItemSource->GetShowDescription())
	{
		if (idx < 0 || idx > m_DescriptionsBuffer.GetUpperBound())
			return -1;
		CString s = m_DescriptionsBuffer.GetAt(idx);
		return s.GetLength();
	}
	return GetAssociatedTextLen(idx);
}

//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnLBWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	if (wndPos == NULL)
		return -1;

	if ((wndPos->flags & SWP_HIDEWINDOW) == SWP_HIDEWINDOW)
		return -1;

	if((GetStyle() & CBS_STYLES) == CBS_SIMPLE)
		return -1;

	// se la combo è ReadOnly non si deve mai aprire!
	if (IsEditReadOnly())
	{
		wndPos->flags &= ~SWP_SHOWWINDOW;
		wndPos->flags |= SWP_HIDEWINDOW;
		return -1;
	}

	wndPos->flags &= ~SWP_NOSIZE;

	CRect comboRect;
	GetWindowRect(comboRect);

	wndPos->cx = m_sizeListBox.cx;

	// se il numero di elementi e superiore a la dimensione massima ammessa
	// allora Windows fara` apparire una scroll-bar verticale e quindi la larghezza
	// deve essere incrementata della larghezza della scroll-bar
	//
	if (m_DataAssociations.GetSize() >= m_nMaxRowsNo)
		wndPos->cx += GetSystemMetrics(SM_CXVSCROLL);

	wndPos->cx = max(wndPos->cx, comboRect.Width());

	// Get screen size
	CRect rcMonitorArea, rcWorkArea;
	GetMonitorRect(this, rcMonitorArea, rcWorkArea);

	if (comboRect.left + wndPos->cx > rcMonitorArea.right)
	{
		wndPos->flags &= ~SWP_NOMOVE;
		wndPos->x = rcMonitorArea.right - wndPos->cx;
		if(comboRect.bottom + wndPos->cy > rcMonitorArea.bottom)
			wndPos->y = comboRect.top - wndPos->cy;
		else
			wndPos->y = comboRect.bottom;
	}

	return -1;
}

//-----------------------------------------------------------------------------
LRESULT	CParsedCombo::OnGetCurSel(WPARAM, LPARAM)
{
	LRESULT nIdx = DefWindowProc(CB_GETCURSEL, 0, 0L);

	if (nIdx != CB_ERR && nIdx >= (LRESULT)GetCount())
		nIdx = CB_ERR;

	return nIdx;
}

//-----------------------------------------------------------------------------
HRESULT CParsedCombo::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

//-----------------------------------------------------------------------------
HBRUSH CParsedCombo::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	if (GetChildEdit() == NULL || !pWnd || m_wndEdit.m_hWnd != pWnd->m_hWnd)
		return __super::OnCtlColor(pDC, pWnd, nCtlColor);

	return OnEditCtlColor(pDC, nCtlColor);
}

//-----------------------------------------------------------------------------
HBRUSH CParsedCombo::CtlColor(CDC* pDC, UINT nCtlColor) 
{
	if (IsWindowEnabled() && !IsEditReadOnly())
	{
		if (AfxGetThemeManager()->IsFocusedControlBkgColorEnabled() && this == GetFocus())
		{
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
			pDC->SetBkColor(AfxGetThemeManager()->GetFocusedControlBkgColor()); 
			return (HBRUSH)AfxGetThemeManager()->GetFocusedControlBkgColorBrush()->GetSafeHandle();
		}
		else
		{
			if (m_bColored)
			{
				pDC->SetTextColor(this->m_crText);
				pDC->SetBkColor(this->m_crBkgnd);
				return GetBkgBrushHandle();
			}
			else
			{
				pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
				pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
				return (HBRUSH) AfxGetThemeManager()->GetEnabledControlBkgColorBrush()->GetSafeHandle();
			}
		}
	}
	else
	{
		if (m_bColored)
		{
			pDC->SetTextColor(this->m_crText);
			pDC->SetBkColor(this->m_crBkgnd);
			return GetBkgBrushHandle();
		}
		else
		{
			CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
			if (pManager)
				pDC->SetTextColor(pManager->GetControlForeColor(this, IsWindowEnabled()));
			else
				pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());

			pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
			return (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::NeedCustomPaint()
{
	return 
		(m_bColored && !IsWindowEnabled())
		||
		(IsShowDescription	() && !IsWindowEnabled())
		||
		(
			!IsWindowEnabled() &&
			GetChildEdit() == NULL &&
			(m_pHotKeyLink ? !m_pHotKeyLink->IsHyperLinkEnabled() : TRUE) /*&&
			m_pDocument && m_pDocument->UseEasyReading()*/
		);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnPaint( )
{
	if (!NeedCustomPaint() && ((GetStyle() & CBS_STYLES) != CBS_DROPDOWNLIST))
	{
		__super::OnPaint();
		return;
	}

	COMBOBOXINFO cbi;
	cbi.cbSize = sizeof(cbi);
	if (!GetComboBoxInfo(&cbi))
	{
		CPaintDC dc(this);
		return;
	}

	// Tapullo per evitare il fillrect casuale (vedi anomalia in test) di un area al di fuori del clientrect

	CRect rect;
	GetClientRect(rect);
	if	(
			cbi.rcItem.left - 3 < rect.left || cbi.rcItem.top - 3 < rect.top ||
			cbi.rcItem.right + (IsServer2008() ? 19 : 20) > rect.right || cbi.rcItem.bottom + 3 > rect.bottom
		)
	{
		CPaintDC dc(this);
		return;
	}

	CDC* pDC = GetDC();
	if (pDC)
	{
		HGDIOBJ old = pDC->SelectObject(GetPreferredFont());

		CString strText;
		if (IsShowDescription())
		{
			ASSERT_KINDOF(CDescriptionCombo, this);
			strText = m_sReadOnlyText;
		}
		else
			GetValue(strText);

		RECT aRect = cbi.rcItem;
		aRect.left -= 2;
		if (aRect.left < 0)
			aRect.left = 1;
//		else
//			aRect.right += 0;

		aRect.top -= 2;
		if(aRect.top < 0)
			aRect.top = 1;
		else
			aRect.bottom += 2;

		RECT invRect = aRect;

		if (IsWindowEnabled())
		{
			CBrush* pB1 = HasFocus() ? AfxGetThemeManager()->GetFocusedControlBkgColorBrush() : AfxGetThemeManager()->GetEnabledControlBkgColorBrush();
			pDC->FillRect(&aRect, pB1);
		}
		else
		{
			pDC->FillRect(&aRect, GetBkgBrush());
			pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
		}

		aRect.left += 3;
		aRect.top += 2;

		if (m_bColored)
		{
			pDC->SetTextColor(this->m_crText);
			pDC->SetBkColor(this->m_crBkgnd);

			pDC->DrawText(strText, &aRect, DT_NOPREFIX);
		}
		else
		{
			if (IsWindowEnabled())
			{
				pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
				pDC->SetBkColor(HasFocus() ? AfxGetThemeManager()->GetFocusedControlBkgColor() : AfxGetThemeManager()->GetEnabledControlBkgColor());
			}
			else
			{
				CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
				if (pManager)
					pDC->SetTextColor(pManager->GetControlForeColor(this, IsWindowEnabled()));
				else
					pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());
			}

			pDC->ExtTextOut	
							(
								aRect.left, 
								aRect.top,
								ETO_CLIPPED, 
								&aRect,
								strText, 
								strText.GetLength(),
								NULL
							);
		}
		pDC->SelectObject(old);
		ReleaseDC(pDC);
		ValidateRect(&invRect);
	}

	//serve a disegnare frame e dropdown
	__super::OnPaint();
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
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
	CComboDescription* pDesc = (CComboDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CComboDescription), strId);	
	pDesc->UpdateAttributes(this);
	
	PopulateColorDescription(pDesc);
	PopulateFontDescription(pDesc);

	return (LRESULT) pDesc;
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::HasFocus()
{
	CWnd* pFocusWnd = GetFocus();
	if (!pFocusWnd || !IsWindow(pFocusWnd->m_hWnd))
		return FALSE;

	return (pFocusWnd == m_pOwnerWnd || pFocusWnd->GetParent() == m_pOwnerWnd);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::IsThemedDropDownList() const
{ 
	BOOL bRet = TRUE;
	return bRet; 
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::DoKeyDown(UINT nChar)
{
	if (CParsedCtrl::DoKeyDown(nChar))
		return TRUE;

	BOOL bIsDropped = GetDroppedState();

	if	(
			(nChar == VK_DELETE	|| nChar == VK_BACK)		&&
			(GetStyle() & CBS_STYLES) == CBS_DROPDOWNLIST	&&
			(GetCtrlStyle() & COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL) == COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL
		)
	{
		if (bIsDropped)
		{
			// semaforizza le azioni svolte su CBN_SELENDOK/CBN_SELCHANGE
			m_wComboStatus |= FORCING_CLOSEUP;
			ShowDropDown(FALSE);
			m_wComboStatus &= ~FORCING_CLOSEUP;
		}

//		m_wComboStatus |= HIT_DEL;
//		ResetAssociations(TRUE);
		SetCurSel(-1);
		m_sReadOnlyText.Empty();
		SetModifyFlag(TRUE);

		// Anomalia 22061
		if ((nChar == VK_DELETE || nChar == VK_BACK) && this->m_pData)
		{
			this->m_pData->Clear();
			SetValue(*this->m_pData);
			if (this->m_pOldData)
				this->m_pOldData->Clear();
		}

		return TRUE;
	}

	if (!bIsDropped || (GetStyle() & CBS_STYLES) == CBS_SIMPLE)
		return FALSE;

	switch (nChar)
	{
		case VK_ESCAPE :
		{
			BOOL bOk = m_nErrorID == 0;

			// semaforizza le azioni svolte su CBN_SELENDOK/CBN_SELCHANGE
			m_wComboStatus |= FORCING_CLOSEUP;
			ShowDropDown(FALSE);
			m_wComboStatus &= ~FORCING_CLOSEUP;

			ResetAssociations(TRUE);

			if (IsTBWindowVisible(this))
				UpdateCtrlView();

			// se si e` in stato di errore si rimane con il flag di modifica a TRUE
			SetModifyFlag(!bOk);

			return TRUE;
		}
		case VK_TAB :
		case VK_RETURN :
		{
			if (GetCurSel() < 0)
			{
				if (GetChildEdit() != NULL)
				{
					// serve per gestire, secondo lo stile WIN16, il
					// caso in cui venga digitato qualcosa nell'edit con la
					// aperta e poi si confermi mediante TAB
					if (GetCtrlData())
					{
						DataObj* pHoldData = GetCtrlData()->DataObjClone();

						GetValue(*pHoldData);

						// semaforizza le azioni svolte su CBN_SELENDOK/CBN_SELCHANGE
						m_wComboStatus |= FORCING_CLOSEUP;
						ShowDropDown(FALSE);
						m_wComboStatus &= ~FORCING_CLOSEUP;

						ResetAssociations(TRUE);
						SetValue(*pHoldData);
						delete pHoldData;
					}
					else
					{
						CString strCur;
						GetValue(strCur);

						// semaforizza le azioni svolte su CBN_SELENDOK/CBN_SELCHANGE
						m_wComboStatus |= FORCING_CLOSEUP;
						ShowDropDown(FALSE);
						m_wComboStatus &= ~FORCING_CLOSEUP;

						ResetAssociations(TRUE);
						SetValue(strCur);
					}
				}
				else
				{
					m_wComboStatus |= FORCING_CLOSEUP;
					ShowDropDown(FALSE);
					m_wComboStatus &= ~FORCING_CLOSEUP;
				}

				SetModifyFlag(TRUE);
			}
			else
			{
				NotifyToParent(CBN_SELENDOK);
				NotifyToParent(CBN_SELCHANGE);

				// semaforizza le azioni svolte su CBN_SELENDOK/CBN_SELCHANGE
				m_wComboStatus |= FORCING_CLOSEUP;
				ShowDropDown(FALSE);
				m_wComboStatus &= ~FORCING_CLOSEUP;
			}

			return nChar == VK_RETURN;
		}
	}
	
	if (GetDroppedState() && m_pHotKeyLink && m_pHotKeyLink->IsLikeOnDropDownEnabled() && m_sPrefixBeforeDrop.GetLength())
		PostMessage(UM_CHECK_DROPDOWN);

	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedCombo::DoKillFocus (CWnd* pWnd)
{
	// if the focus is given to associate button or comboBox window
	// then the kill focus message must not go forward.
	//
	if	(
			pWnd &&
			(
			 	IsAssociatedButton(pWnd)	||
			 	(GetChildEdit() != NULL && pWnd->m_hWnd == m_wndEdit.m_hWnd) ||
				(pWnd->m_hWnd == m_wndList.m_hWnd)
			)
		)
		return;
		
	CParsedCtrl::DoKillFocus(pWnd);

	// lascia solo l'elemento attualmente selezionato
	if ((GetStyle() & CBS_STYLES) != CBS_SIMPLE && m_nErrorID == 0)
		ResetAssociations(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::OnInitCtrl()
{
	VERIFY(CParsedCtrl::OnInitCtrl());

	m_bSorted = ((GetStyle() & CBS_SORT) == CBS_SORT);

	m_sizeListBox.cx = 0;
	m_nMaxItemsLen = -1;

	CBCGPComboBox::ResetContent();
	m_DataAssociations.RemoveAll();	m_DescriptionsBuffer.RemoveAll();

	SetCurSel(-1);

	// per aprire  la tendina su VK_DOWN
	if ((GetStyle() & CBS_STYLES) == CBS_DROPDOWN)
		SetExtendedUI();
	
	if ((GetStyle() & CBS_STYLES) == CBS_SIMPLE)
		FillListBox();
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::IsEditReadOnly () const
{
	 return (const_cast<CParsedCombo*>(this))->GetChildEdit() != NULL && (m_wndEdit.GetStyle() & ES_READONLY) != 0;
}

//-----------------------------------------------------------------------------
CRect CParsedCombo::GetEditReadOnlyRect()
{
	// mi faccio dare le dimensioni della combo
	COMBOBOXINFO cbi;
	cbi.cbSize = sizeof(cbi);
	GetComboBoxInfo(&cbi);

	CRect aRect(cbi.rcItem);
	aRect.left -= 3;
	aRect.right = cbi.rcButton.right + 1;
	aRect.top -= 3;
	aRect.bottom += 3;
	return aRect;
}

// Si occupa di rendere ReadOnly una ComboBox facendola diventare un edit in
// stato di ReadOnly=TRUE. L'edit viene tenuto nascosto sotto la combo e usato
// solo quando richiesto
//-----------------------------------------------------------------------------
void CParsedCombo::SetEditReadOnly (const BOOL bValue)
{
	// se non esiste di Edit lavoro solo sulla combo
	if (GetChildEdit() == NULL)
	{
		__super::SetEditReadOnly(bValue);
		return;
	}

	ASSERT(m_pOwnerWnd->m_hWnd);

	if (!m_pOwnerWnd->IsWindowEnabled() && !bValue)
		m_pOwnerWnd->EnableWindow(TRUE);

	m_wndEdit.SetReadOnly(bValue);

	if (m_pDocument &&  m_pDocument->GetFormMode() != CBaseDocument::BROWSE)
	{
		CRect aRect = GetEditReadOnlyRect();
		// se non esiste la creo, altrimenti la sistemo per grandezza
		if (!m_pReadOnlyEdit)
		{
			m_pReadOnlyEdit = new CStrEdit();
			VERIFY(m_pReadOnlyEdit->Create(m_wndEdit.GetStyle(), aRect, m_pOwnerWnd, 0));
			m_pReadOnlyEdit->ModifyStyle(WS_BORDER, 0);
			m_pReadOnlyEdit->SetCtrlFont(AfxGetThemeManager()->GetHyperlinkFont());
			m_pReadOnlyEdit->SetBkgColor(this->GetBkgColor()/*AfxGetThemeManager()->GetBackgroundColor()*/);
		}

		m_pReadOnlyEdit->SetWindowPos(NULL, 0, 0, aRect.Width(), aRect.Height(), SWP_NOMOVE | SWP_NOZORDER);

		m_pReadOnlyEdit->InvalidateRect(aRect);

		// travaso il contenuto del testo dalla combo all'edit
		m_wndEdit.GetWindowText(m_sReadOnlyText);
		m_pReadOnlyEdit->SetWindowText((LPCTSTR)m_sReadOnlyText);
		m_pOwnerWnd->SetWindowPos(m_pReadOnlyEdit, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		m_pReadOnlyEdit->SetReadOnly(bValue);

		m_wndEdit.ShowWindow(bValue ? SW_HIDE : SW_SHOW);
		m_pReadOnlyEdit->ShowWindow(bValue ? SW_SHOW : SW_HIDE);
	}
	else
	{
		// nascondo l'edit per rivisualizzare la combo originaria
		if (m_pReadOnlyEdit)
			m_pReadOnlyEdit->ShowWindow(SW_HIDE);

		m_wndEdit.ShowWindow(SW_SHOW);
		if (m_pReadOnlyEdit)
			m_pReadOnlyEdit->SetWindowPos(m_pOwnerWnd, 0, 0, 0, 0, SWP_NOMOVE|SWP_NOSIZE);
	}

	if (m_pButton)
		DoEnable(!bValue);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnEnable(BOOL bEnable)
{
	CBCGPComboBox::OnEnable(bEnable);
	
	DoEnable(bEnable);

	if ((GetStyle() & CBS_STYLES) == CBS_SIMPLE)
		if (bEnable)
			FillListBox();
		else
			ResetAssociations(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if ((nID == (UINT)GetCtrlID() && hWndCtrl != NULL))
	{
		// control notification
		ASSERT(::IsWindow(hWndCtrl));

		switch (nCode)
		{
			case CBN_SELCHANGE:
			{
				if ((m_wComboStatus & HIT_SELENDOK) == HIT_SELENDOK)
					DoSelection();

				return TRUE;
			}
			case CBN_DBLCLK:		return TRUE;
			case CBN_EDITCHANGE:	return TRUE;
			case CBN_EDITUPDATE:	return TRUE;
			case CBN_DROPDOWN:
			{
				if (!IsEditReadOnly())
				{
					m_wComboStatus &= ~HIT_SELENDOK;

					FillListBox();

					m_bFillListBoxEmpty = m_DataAssociations.GetSize() == 0 && GetCount() == 0;

					if (m_bFillListBoxEmpty)
						AddString(_T(""));
				}

				return TRUE;
			}
			case CBN_CLOSEUP:
			{
				if (m_bFillListBoxEmpty)
				{
					DeleteString(0);
					m_bFillListBoxEmpty = FALSE;
				}

				GetParent()->UpdateWindow();
				return TRUE;
			}
			case CBN_SELENDOK:
			{
				// NB. Se dopo questo messaggio arriva il CBN_SELCHANGE allora verra`
				// effettuata la selezione
				if	(
						IsTBWindowVisible(this) &&
//						m_nErrorID == 0 &&
						(m_wComboStatus & FORCING_CLOSEUP) == 0
					)
					m_wComboStatus |= HIT_SELENDOK;

				return TRUE;
			}
			case CBN_SELENDCANCEL:
			{
				if (IsTBWindowVisible(this) && m_nErrorID == 0)
					UpdateCtrlView();

				// se si e` in stato di errore si rimane con il flag di modifica a TRUE
				if (m_nErrorID)
					SetModifyFlag(TRUE);

				return TRUE;
			}
		}
	}

	CParsedCtrl::DoCommand(wParam, lParam);

	BOOL bDone = CBCGPComboBox::OnCommand(wParam, lParam);

	//guardo se è un comando di menu o un accelleratore
	CWnd* pParent = GetCtrlParent();
	CBaseDocument* pDoc = GetDocument();
	if (!pDoc && pParent)
	{
		CParsedForm* pForm = dynamic_cast<CParsedForm*>(pParent);
		if (pForm)
			pDoc = pForm->GetDocument();
	}

	if (!bDone && lParam == 0 && ((wParam & 0xFFFE0000) == 0) && pDoc)
	{
		//lo ruoto al documento
		POSITION pos = pDoc->GetFirstViewPosition();
		if (pos != NULL)
			pDoc->GetNextView(pos)->PostMessage(WM_COMMAND, wParam, 0);
	}
	return bDone;
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	if (m_pReadOnlyEdit && (::GetWindowLong(m_pReadOnlyEdit->m_hWnd, GWL_STYLE) & WS_VISIBLE) )
	{
		CRect aRect = GetEditReadOnlyRect();
		m_pReadOnlyEdit->SetWindowPos(NULL, 0, 0, aRect.Width(), aRect.Height(), SWP_NOMOVE | SWP_NOZORDER);
		m_pReadOnlyEdit->InvalidateRect(aRect);
	}

	GridCellPosChanging(wndPos);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnSetFocus(CWnd* pWnd)
{
	DoSetFocus(pWnd);
	__super::OnSetFocus(pWnd);
}
//-----------------------------------------------------------------------------
void CParsedCombo::OnKillFocus (CWnd* pWnd)
{
	// nel caso ci sia l'edit l'azione e` svolta dalla sua OnKillFocus
	if (GetChildEdit() == NULL)
		DoKillFocus(pWnd);

	// se ho la gestione del calendario e posso chiuderlo
	// allora eseguo anche la chiusura del calendario
	CCalendarButton* pCalendar = NULL;
	if (GetButton() && GetButton()->IsKindOf(RUNTIME_CLASS(CCalendarButton)))
		pCalendar = (CCalendarButton*) GetButton();

	if (pCalendar)
	{
		BOOL bIsEnabledOrGrid = IsWindowEnabled() || dynamic_cast<CGridControlObj*>(GetCtrlParent());

		if (bIsEnabledOrGrid && pCalendar->IsDestroyCalendarEnabled())
			GetButton()->PostMessage(UM_DESTROY_CALENDAR);
	}

	// standard action
	CBCGPComboBox::OnKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (GetChildEdit() != NULL && DoOnChar(nChar))
		return;

	CBCGPComboBox::OnChar(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnCheckDropDown(WPARAM , LPARAM )
{
	if (GetDroppedState() && m_pHotKeyLink && m_pHotKeyLink->IsLikeOnDropDownEnabled() && m_sPrefixBeforeDrop.GetLength())
	{
		CString s;	GetValue(s);
		TRACE(s +'\n' + m_sPrefixBeforeDrop+'\n');
		if (m_sPrefixBeforeDrop.CompareNoCase(s.Left(m_sPrefixBeforeDrop.GetLength())))
		{
			m_wComboStatus |= FORCING_CLOSEUP;
			ShowDropDown(FALSE);
			m_wComboStatus &= ~FORCING_CLOSEUP;

			ResetAssociations(TRUE);
			SetModifyFlag(TRUE);

			SetWindowText(s);
			this->GetCtrlData()->Assign(s);

			if (GetChildEdit() != NULL)
			{
				m_wndEdit.SetWindowText(s);
				m_wndEdit.SetSel(s.GetLength(), s.GetLength());
			}
		}
	}

	return 0;
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (DoKeyUp(nChar))
		return;
	
	CBCGPComboBox::OnKeyUp(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{       
	if (DoKeyDown(nChar))
		return;
	
	// Per simulare il CComboBox::SetExtendedUI() in quanto se la combo e` vuota,
	// se si preme la freccia Up/Down, Windows non apre la tendina se lo stile e`
	// CBS_DROPDOWNLIST
	//
	if	(
			(GetStyle() & CBS_STYLES) == CBS_DROPDOWNLIST	&&
			!GetDroppedState()								&&
			(nChar == VK_UP || nChar == VK_DOWN)			&&
			(CWnd::GetFocus() == CWnd::FromHandle(m_hWnd))
		)
	{ 
		ShowDropDown(TRUE);
		return;
	}

	CBCGPComboBox::OnKeyDown(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	if (!CParsedCtrl::DoRButtonDown(nFlag, mousePos))
		CBCGPComboBox::OnRButtonDown(nFlag, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CBCGPComboBox::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnLButtonUp(UINT nFlag, CPoint mousePos)
{
	CBCGPComboBox::OnLButtonUp (nFlag, mousePos);

	if (m_nButtonIDBmp != BTN_SPIN_ID)
	 	return;
	
	NotifyToParent(EN_SPIN_RELEASED);
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnVScroll (UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{                  
	if (m_nButtonIDBmp != BTN_SPIN_ID)
	{
		CBCGPComboBox::OnVScroll(nSBCode, nPos, pScrollBar);
		return;
	}                          
	
	DoSpinScroll(nSBCode);
}

//-----------------------------------------------------------------------------
LRESULT CParsedCombo::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::DoSelection()
{
	m_wComboStatus &= ~HIT_SELENDOK;

	SetModifyFlag(TRUE);

	UpdateCtrlData(TRUE); 

	if (IsShowDescription())
	{
		GetDescription(GetCtrlData(), m_sReadOnlyText);
	}
	if (m_pControlBehaviour)
	{
		int nIndex = GetCurSel();
		m_pControlBehaviour->OnSelect(GetCtrlData(), nIndex);
	}
	
	//Anomalia 14854: forza il ridisegno se la tendina copre il control
	this->SetFocus();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCombo::FillListBox()
{
	m_sPrefixBeforeDrop.Empty();

	SetRedraw(FALSE);
	DataObj* pHoldData = NULL;
	CString strCur;
	CRect cRectCombo;
	CRect cRectWindows;

	// Mi salvo il corrente valore visualizzato, per permettere al programmatore,
	// nella reimplementazione della OnFillListBox(), di utilizzare tale valore che
	// altrimente verrebbe perso a causa della chiamata alla ResetAssociations (TRUE)
	// (vedi sotto)
	// Un esempio di questo utilizzo e` nella gestione della combo degli articoli 
	// equivalenti dei "Movimenti articoli" di MxW
	if (GetCtrlData())
	{
		pHoldData = GetCtrlData()->DataObjClone();
		GetValue(*pHoldData);
	}
	else
		GetValue(strCur);

	// cleanup completo della combo
	ResetAssociations (TRUE);

	if (GetChildEdit() != NULL)
	{
		if (GetCtrlData())
			SetValue(*pHoldData);
		else
			SetValue(strCur);
	}
	
	bool itemSourcePopulated = false;
	
	if (m_pItemSource)
	{
		CStringArray descriptions;
		DataArray values;
		m_pItemSource->m_bAllowEmptyData = false;
		m_pItemSource->GetData(values, descriptions, GetCtrlData()->Str()); //TODOLUCA
		if (values.GetSize() == descriptions.GetSize())
		{
			itemSourcePopulated = descriptions.GetSize() > 0 || m_pItemSource->m_bAllowEmptyData;
			for (int i = 0; i < values.GetSize(); i++)
			{
				CString sDesc = descriptions[i];
				DataObj* pData = values.GetAt(i);
				if (pData->GetDataType() != GetDataType())
				{
					ASSERT(FALSE);
					break;
				}
				AddAssociation(sDesc, *pData);
			}
		}
		else
		{
			ASSERT(FALSE);
		}
	}

	//se non è stato popolato dall'itemsource, chiamo la OnFillListBox
	if (!itemSourcePopulated)
	{
		//chiama la virtual function
		OnFillListBox();
	}

	SetRedraw(TRUE);
	Invalidate(FALSE);

	m_wComboStatus |= FILLING_LISTBOX;

	// setta il corrente item
	if (GetCtrlData())
		SetValue(*pHoldData);
	else
		SetValue(strCur);

	SetModifyFlag(TRUE);

	if (pHoldData)
		delete pHoldData;

	SetCtrlSel(0,-1);

	m_wComboStatus &= ~FILLING_LISTBOX;

	if ((GetStyle() & CBS_STYLES) == CBS_SIMPLE)
		return;

	// dimensione standard ? (E` azzerato dalla ResetAssociations)
	if (m_DataAssociations.GetSize() == 0)
	{
		// Show one row in Combo list box
		return;
	}

	CRect rect;
	GetWindowRect(rect);

	int cx = m_sizeListBox.cx;

	// se il numero di elementi e superiore a la dimensione massima ammessa
	// allora Windows fara` apparire una scroll-bar verticale e quindi la larghezza
	// deve essere incrementata della larghezza della scroll-bar
	//
	if (m_DataAssociations.GetSize() >= m_nMaxRowsNo)
		cx += GetSystemMetrics(SM_CXVSCROLL);

	GetCtrlParent()->ScreenToClient(rect);
	SetDroppedWidth(max(cx, rect.Width()));
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetFillComboFuncPtr(FILLCOMBO_FUNC value)
{
	m_pManagedFillComboFuncPtr = value;
}

// In caso di hotlink con combo riempie la listbox con i dati estratti dalla
// tabella di database
//-----------------------------------------------------------------------------
void CParsedCombo::OnFillListBox()
{
	if (m_pManagedFillComboFuncPtr && (!m_pHotKeyLink || !m_pHotKeyLink->IsFillListBoxEnabled()))
	{
		m_pManagedFillComboFuncPtr();
		return;
	}

	if (GetCtrlData() == NULL || m_pHotKeyLink == NULL || !m_pHotKeyLink->IsFillListBoxEnabled())
		return;
	
	GetValue(m_sPrefixBeforeDrop);

	// ---- richiesta all'hotlink di eseguire la query e di popolare la combo
	
	if (m_bSorted)
	{
		DataObjArray aKeys; CStringArray aDescriptions;

		int nResult = m_pHotKeyLink->DoSearchComboQueryData(GetMaxItemsNo(), aKeys, aDescriptions);
	
		if (!nResult || !aKeys.GetSize() || aKeys.GetSize() != aDescriptions.GetSize())
			return;	

		CDC*	pDC		= m_wndList.GetDC();
		CFont*	pFont	= m_wndList.GetFont();

		// riempie la combo controllando il nr. di items massimo
		aKeys.SetOwns(FALSE);
		for (int i = 0; i < aKeys.GetSize (); i++)
			AddAssociationSorted(aDescriptions.GetAt(i), aKeys.GetAt(i), pDC, pFont);

		// aggiungo il messaggio di raggiunto nr. massimo di elementi
		if (nResult == 2)
			AddAssociation(CString(_T(" ")) + cwsprintf(FormatMessage(MAX_ITEM_REACHED), GetMaxItemsNo()), *GetCtrlData(), FALSE);

		if (pDC)
			m_wndList.ReleaseDC(pDC);
	}
	else	//versione ottimizzata
	{
		m_DescriptionsBuffer.RemoveAll(); m_DataAssociations.RemoveAll();

		int nResult = m_pHotKeyLink->DoSearchComboQueryData(GetMaxItemsNo(), m_DataAssociations, m_DescriptionsBuffer);
	
		if (!nResult || !m_DataAssociations.GetSize() || m_DataAssociations.GetSize() != m_DescriptionsBuffer.GetSize())
			return;		
	
		CDC*	pDC = m_wndList.GetDC();
		CFont*	pFont = m_wndList.GetFont();

		for (int i = 0; i < m_DataAssociations.GetSize (); )
		{
			if (AddAssociationUnsorted(m_DescriptionsBuffer.GetAt(i), m_DataAssociations.GetAt(i), pDC, pFont) < 0)
			{
				m_DataAssociations.RemoveAt(i); m_DescriptionsBuffer.RemoveAt(i); 
			}
			else i++;
		}
		if (nResult == 2)	// aggiungo il messaggio di raggiunto nr. massimo di elementi
		{
			m_DataAssociations	.InsertAt(0, GetCtrlData()->DataObjClone());
			m_DescriptionsBuffer.InsertAt(0, cwsprintf(FormatMessage(MAX_ITEM_REACHED), GetMaxItemsNo()));

			InsertAssociation(0, m_DescriptionsBuffer[0], m_DataAssociations[0], pDC, pFont, FALSE);
		}

		if (pDC)
			m_wndList.ReleaseDC(pDC);
	}

}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::IsValid()
{
	if (GetCtrlData() && GetCtrlData()->IsOSLHide()) 
		return TRUE; 

	GetValue(m_strBadVal);

	return CParsedCtrl::IsValid();
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::IsValid(const DataObj& aValue)
{
	m_strBadVal = aValue.FormatData();

	return CParsedCtrl::IsValid(aValue);
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetValue(const DataObj& aValue)
{
	if (aValue.IsOSLHide()) 
		return; 

	ASSERT(CheckDataObjType(&aValue));
	
	if (!DoSetCurSel(aValue))
		DoSetValue(aValue);
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetValue(LPCTSTR pszValue)
{
	if (GetChildEdit() != NULL)
	{
		SetWindowText(pszValue);
		return;
	}
    
	int nIdx = FindStringExact(-1, pszValue);
		
	if (nIdx == -1 && pszValue && *pszValue)
	{
		// non e` stato trovata la stringa quindi svuoto d'ufficio la
		// la combo e aggiungo la stringa da visualizzare poiche` questo
		// e` il solo modo per visualizzare qualcosa in uan combo DROPDOWNLIST
		//
		if (!GetDroppedState() && (m_wComboStatus & FILLING_LISTBOX) == 0)
			ResetAssociations(TRUE);

		nIdx = AddString(pszValue);

		if (IsShowDescription())
		{
			m_sReadOnlyText = pszValue;
			SetWindowText(pszValue);
		}
		
		// e` necessario fare anche l'aggiunta nel vettore ausiliario per
		// non incorrere nello spiacevole effetto di vedere nella listbox
		// l'elemento aggiunto dalla AddString di cui sopra, ma di non avere
		// l'indice di selezione sincrono (provare in "Test Parsed Control" di Tim
		// ad aggiungere in una delle combo editabili un valore non esistente
		// ed abassare la tendina nella combo NON editabile corrispondente
		//@@TODOLUCA, il controllo di attivazione del test manager è un modo per evitare una regressione dovuta ad una correzione
		//su un tipo particolare di combo, è un tappullo, ma per il momento va bene così
		if (AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")))
		{
			if (GetCtrlData() && (m_wComboStatus & FILLING_LISTBOX) == FILLING_LISTBOX)
			{
				DataObj* pDummy = GetCtrlData()->DataObjClone();
				m_DataAssociations.InsertAt(nIdx, pDummy);	
				m_DescriptionsBuffer.InsertAt(nIdx, m_pItemSource ? m_pItemSource->GetDescription(GetCtrlData()) : pszValue);
			}
		}
		else
		{
			if (GetCtrlData())
			{
				DataObj* pDummy = GetCtrlData()->DataObjClone();
				m_DataAssociations.InsertAt(nIdx, pDummy);	
				m_DescriptionsBuffer.InsertAt(nIdx, m_pItemSource ? m_pItemSource->GetDescription(GetCtrlData()) : pszValue);
			}
		}
	}

	SetCurSel(nIdx);
}

//-----------------------------------------------------------------------------
void CParsedCombo::GetValue(CString& strBuffer)
{
   	if (GetChildEdit() != NULL)
   	{
		m_wndEdit.GetWindowText(strBuffer);
		return;
	}
	
	strBuffer.Empty();
	int idx = GetCurSel();
	int n = GetCount();
	int m = m_DataAssociations.GetSize();
	if (idx >= 0 && idx < n)
		GetLBText(idx, strBuffer);
}

//-----------------------------------------------------------------------------
void CParsedCombo::GetValue(DataObj& aValue)
{
	if (aValue.IsOSLHide()) 
		return;

	
	int idx = GetCurSel();
	if (m_DataAssociations.GetSize() && idx >= 0 && idx < GetCount())
	{
		DataObj* pTmpDataObj = GetDataObjFromIdx(idx);
	
		if (pTmpDataObj)
		{
			ASSERT(aValue.GetDataType() == pTmpDataObj->GetDataType());
			aValue.Assign(*pTmpDataObj);
		}
			
		return;
	}

	CString strBuffer;
	GetValue(strBuffer);

	if (m_nFormatIdx >= 0)
	{
		Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
		if (pFormatter)
			strBuffer = pFormatter->UnFormat(strBuffer);
	}

	if (strBuffer.IsEmpty() && aValue.GetDataType() == DATA_ENUM_TYPE)
		return; 
	
	aValue.Assign(strBuffer, m_nFormatIdx);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::DoSetCurSel(const DataObj& aValue)
{
	int nIdx = -1;
	
	if (m_DataAssociations.GetSize() && !IsKindOf(RUNTIME_CLASS(CMSStrCombo)))
	{
		nIdx = GetIdxFromDataObj(aValue, TRUE);
		if (nIdx < 0 || nIdx > m_DataAssociations.GetUpperBound())
			nIdx = -1;
	}

	// corretta an# 7084 (anche se c'è finito per errore
	// un valore invalido, faccio caricare alla  tendina 
	// i valori consentiti, per permetterne la modifica)
	if ((m_wComboStatus & FILLING_LISTBOX) == FILLING_LISTBOX && m_DataAssociations.GetSize () && nIdx == -1 && !IsValidItemListBox(aValue))
		nIdx = 0;

	// setta il corrente item
	if (m_hWnd)
		SetCurSel(nIdx);

	return nIdx >= 0;
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::IsValidItemListBox (const DataObj& aValue)
{ 
	return m_pItemSource ? m_pItemSource->IsValidItem (aValue) : TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetCtrlSel(int nStart, int nEnd)
{
	if (GetChildEdit() != NULL)
		m_wndEdit.SetSel(nStart, nEnd);
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetCtrlMaxLen(UINT nLen, BOOL bApplyNow)
{
	m_nCtrlLimit = nLen;
	
	if (GetChildEdit() != NULL && bApplyNow)
		CBCGPComboBox::LimitText(nLen);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::SetCtrlPos(const CWnd* pWndInsertAfter, int x, int y, int cx, int cy, UINT nFlags)
{
	// e` necessario chiudere la tendina se si sta muovendo il control
	// (per es. nel bodyedit)
	//
	if ((GetStyle() & CBS_STYLES) != CBS_SIMPLE && GetDroppedState())
	{
		// semaforizza le azioni svolre su CBN_SELENDOK/CBN_SELCHANGE
		m_wComboStatus |= FORCING_CLOSEUP;
		ShowDropDown(FALSE);
		m_wComboStatus &= ~FORCING_CLOSEUP;

		// se il bottone va a finire dove prima cera la tendina e`
		// necessario forzare il repaint dello stesso
		if (m_pButton) m_pButton->Invalidate();
	}
		
	return CParsedCtrl::SetCtrlPos(pWndInsertAfter, x, y, cx, cy, nFlags);
}

//-----------------------------------------------------------------------------
CSize CParsedCombo::AdaptNewSize(UINT nCols, UINT nRows, BOOL bButtonsIncluded)
{
	ASSERT(nRows <= 1 || (GetStyle() & CBS_STYLES) == CBS_SIMPLE || nRows == (UINT)m_nMaxRowsNo);

	CSize cs = CParsedCtrl::AdaptNewSize(nCols, 1, bButtonsIncluded);

	if (!bButtonsIncluded)
		return cs;

	// se lo stile e` diverso da CBS_SIMPLE si aggiunge la larghezza del
	// bitmap della freccia
	//
	if ((GetStyle() & CBS_STYLES) != CBS_SIMPLE)
		cs.cx += GetSystemMetrics(SM_CXVSCROLL);
		
	// se lo stile e` CBS_DROPDOWN si aggiunge lo spazio di separazione
	// tra l'edit la freccia
	//
	if ((GetStyle() & CBS_STYLES) == CBS_DROPDOWN)
		cs.cx += 8;

	return cs;
}

//-----------------------------------------------------------------------------
//	Add an Association between a dataObj and a String. 
int CParsedCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& dataObj, BOOL bCallFilter /* = TRUE */)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (bCallFilter && !IsValidItemListBox(dataObj))
		return -1;

	// cerca eventuali caratteri di controllo (p.e. CR-LF nel
	// caso di stringhe su piu` righe) e ci sostituisce 3 punti di sospensione
	int nLen = _tcslen(lpszAssoc);
	int i;
	for (i = 0; i < nLen; i++)
		if ((UINT)lpszAssoc[i] < 0x0020) break;	// trovato carattere di controllo

	int nLbIdx = 0;
	int nStrSize;
	
	CDC*	pDC = m_wndList.GetDC();
	CFont*	pFont = m_wndList.GetFont();

	if (i < nLen)
	{
		CString	strAssoc;
		
		// per appendere "..." -----------------------v
		TCHAR* pszStr = strAssoc.GetBufferSetLength(i + 3);
		TB_TCSNCPY(pszStr, lpszAssoc, i);
		TB_TCSCPY(&pszStr[i], _T("..."));
		strAssoc.ReleaseBuffer();

		m_nMaxItemsLen = max(m_nMaxItemsLen, i + 3);

		if (m_hWnd)
			nLbIdx = CBCGPComboBox::AddString(strAssoc);

		nStrSize = GetEditSize(pDC, pFont, strAssoc).cx;

		m_DescriptionsBuffer.InsertAt(nLbIdx, strAssoc);
	}
	else
	{
		m_nMaxItemsLen = max(m_nMaxItemsLen, nLen);

		if (m_hWnd)
			nLbIdx = CBCGPComboBox::AddString(lpszAssoc);

		nStrSize = GetEditSize(pDC, pFont, lpszAssoc).cx;

		m_DescriptionsBuffer.InsertAt(nLbIdx, lpszAssoc);
	}

	if (dataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
		m_nMaxItemsLen = max(m_nMaxItemsLen, ((const DataStr&)dataObj).GetLen());

	if (pDC)
		m_wndList.ReleaseDC(pDC);

	m_sizeListBox.cx = max(m_sizeListBox.cx, nStrSize);
	
	// add the dataObj value in an association array
	m_DataAssociations.InsertAt(nLbIdx, dataObj.DataObjClone());

	return nLbIdx;
}

//-----------------------------------------------------------------------------
int CParsedCombo::AddAssociationSorted(const CString& sAssoc, DataObj* dataObj, CDC* pDC, CFont* pFont, BOOL bCallFilter /* = TRUE */)
{
	ASSERT(m_bSorted);
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (bCallFilter && !IsValidItemListBox(*dataObj))
	{
		delete dataObj;
		return -1;
	}

	// cerca eventuali caratteri di controllo (p.e. CR-LF nel
	// caso di stringhe su piu` righe) e ci sostituisce 3 punti di sospensione
	int nLen = sAssoc.GetLength();
	int i;
	for (i = 0; i < nLen; i++)
		if (sAssoc[i] < 0x0020) 
			break;	// trovato carattere di controllo

	int nLbIdx;
	int nStrSize;
	
	if (i < nLen)
	{
		CString	strAssoc(sAssoc.Left(i)); strAssoc.Append( _T("..."));

		m_nMaxItemsLen = max(m_nMaxItemsLen, strAssoc.GetLength());
		nLbIdx = CBCGPComboBox::AddString(strAssoc);

		nStrSize = GetEditSize(pDC, pFont, strAssoc).cx;

		m_DescriptionsBuffer.InsertAt(nLbIdx, strAssoc);
	}
	else
	{
		m_nMaxItemsLen = max(m_nMaxItemsLen, nLen);
		nLbIdx = CBCGPComboBox::AddString(sAssoc);

		nStrSize = GetEditSize(pDC, pFont, sAssoc).cx;

		m_DescriptionsBuffer.InsertAt(nLbIdx, sAssoc);
	}

	if (dataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
		m_nMaxItemsLen = max(m_nMaxItemsLen, ((DataStr*)dataObj)->GetLen());

	m_sizeListBox.cx = max(m_sizeListBox.cx, nStrSize);
	
	// add the dataObj value in an association array
	m_DataAssociations.InsertAt(nLbIdx, dataObj);

	return nLbIdx;
}

//-----------------------------------------------------------------------------
int CParsedCombo::AddAssociationUnsorted(const CString& sAssoc, DataObj* dataObj, CDC*	pDC, CFont*	pFont, BOOL bCallFilter /* = TRUE */)
{
	ASSERT(!m_bSorted);
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (bCallFilter && !IsValidItemListBox(*dataObj))
	{
		return -1;
	}

	// cerca eventuali caratteri di controllo (p.e. CR-LF nel
	// caso di stringhe su piu` righe) e ci sostituisce 3 punti di sospensione
	int nLen = sAssoc.GetLength();
	int i;
	for (i = 0; i < nLen; i++)
		if (sAssoc[i] < 0x0020) 
			break;	// trovato carattere di controllo

	int nLbIdx;
	int nStrSize;
	
	if (i < nLen)
	{
		CString	strAssoc(sAssoc.Left(i)); strAssoc.Append( _T("..."));

		m_nMaxItemsLen = max(m_nMaxItemsLen, strAssoc.GetLength());
		nLbIdx = CBCGPComboBox::AddString(strAssoc);

		nStrSize = GetEditSize(pDC, pFont, strAssoc).cx;
	}
	else
	{
		m_nMaxItemsLen = max(m_nMaxItemsLen, nLen);
		nLbIdx = CBCGPComboBox::AddString(sAssoc);

		nStrSize = GetEditSize(pDC, pFont, sAssoc).cx;
	}

	if (dataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
		m_nMaxItemsLen = max(m_nMaxItemsLen, ((DataStr*)dataObj)->GetLen());

	m_sizeListBox.cx = max(m_sizeListBox.cx, nStrSize);
	
	return nLbIdx;
}

//-----------------------------------------------------------------------------
int CParsedCombo::InsertAssociation(int nIdx, const CString& sAssoc, DataObj* dataObj, CDC*	pDC, CFont*	pFont, BOOL bCallFilter /* = TRUE */)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (bCallFilter && !IsValidItemListBox(*dataObj))
		return -1;

	// cerca eventuali caratteri di controllo (p.e. CR-LF nel
	// caso di stringhe su piu` righe) e ci sostituisce 3 punti di sospensione
	int nLen = sAssoc.GetLength();
	int i;
	for (i = 0; i < nLen; i++)
		if (sAssoc[i] < 0x0020) 
			break;	// trovato carattere di controllo

	int nLbIdx;
	int nStrSize;
	
	if (i < nLen)
	{
		CString	strAssoc(sAssoc.Left(i));	
		strAssoc.Append( _T("..."));

		nLbIdx = CBCGPComboBox::InsertString(nIdx, strAssoc);

		nStrSize = GetEditSize(pDC, pFont, strAssoc).cx;
	}
	else
	{
		nLbIdx = CBCGPComboBox::InsertString(nIdx, sAssoc);

		nStrSize = GetEditSize(pDC, pFont, sAssoc).cx;
	}

	m_sizeListBox.cx = max(m_sizeListBox.cx, nStrSize);
	
	return nLbIdx;
}

//-----------------------------------------------------------------------------
void CParsedCombo::ResetAssociations(BOOL bRemoveAll /* = FALSE */)
{
	ASSERT(m_hWnd);

	int nCur = m_hWnd ? GetCurSel() : -1;
	
	if (bRemoveAll || nCur < 0 || nCur > m_DataAssociations.GetUpperBound())
	{
		// in questo modo la prossima che verra` aperta la tendina verra` ricalcolata
		// la larghezza giusta dalla AddAssociation()
		m_sizeListBox.cx = 0;
		m_nMaxItemsLen = -1;
		
		if (bRemoveAll)
			CBCGPComboBox::ResetContent();
		else
		{
			// non viene usata la CBCGPComboBox::ResetContent() per evitare la
			// pulizia dell'edit
			m_wndList.SendMessage(LB_RESETCONTENT, 0, 0);
		}

		if (bRemoveAll || nCur > m_DataAssociations.GetUpperBound())
			SetCurSel(-1);
		
		m_DataAssociations.RemoveAll();	m_DescriptionsBuffer.RemoveAll();
		return;
	}
	
	//----
	if (m_bNoResetAssociations)
		return;

	// lascia solo l'elemento attualmente selezionato
	// la scansione e` a ritroso poiche` gli array vengono impaccati
	SetRedraw(FALSE);

	int nLastItem = GetCount() - 1;
	CString strBuffer;
	
	for (int i = nLastItem; i >= 0; i--)
	{
		if (i == nCur) continue;

		DeleteString(i);

		//@@ paramento di c... (non dovrebbe accadere : vedi piu` sopra SetValue(LPCTSTR) - Germano)
		if (i < 0 || i > m_DataAssociations.GetUpperBound())
		{
			TRACE0("CParsedCombo::ResetAssociation : (i < 0 || i > m_DataAssociations.GetUpperBound()) has happened...\n");
			continue;
		}

		ASSERT(i < m_DescriptionsBuffer.GetCount());
		m_DataAssociations.RemoveAt(i);	m_DescriptionsBuffer.RemoveAt(i);
	}

	// a questo punto l'unico elemento rimasto e` lo 0-esimo
	// e quindi e` necessario segnalare alla combo quale l'effettivo
	// item selezionato
	SetRedraw(TRUE);
	SetCurSel(0);
}

//-----------------------------------------------------------------------------
void CParsedCombo::SetModifyFlag(BOOL bFlag)
{ 
	CParsedCtrl::SetModifyFlag(bFlag);

	if (GetChildEdit() != NULL)
		m_wndEdit.SetModify(bFlag);
}
	
//-----------------------------------------------------------------------------
BOOL CParsedCombo::GetModifyFlag()			
{ 
	if (CParsedCtrl::GetModifyFlag())
		return TRUE;
		
	if (GetChildEdit() != NULL)
		return m_wndEdit.GetModify();
		
	return FALSE;
}

//-----------------------------------------------------------------------------
int CParsedCombo::GetIdxFromDataObj (const DataObj& dataObj, BOOL bNoCase /*= FALSE*/) const
{
	return m_DataAssociations.Find (&dataObj, 0, bNoCase);
}

//-----------------------------------------------------------------------------
DataObj* CParsedCombo::GetDataObjFromIdx(int nIdx) const
{
	if (nIdx >= 0 && nIdx < m_DataAssociations.GetSize())
		return m_DataAssociations[nIdx];
	
	return NULL;
}

//-----------------------------------------------------------------------------
void CParsedCombo::OnFormatPopupMenuItemSelected	(UINT nID)
{
	//per come è stato costruito il menù popup del tipo di formattazione da scegliere ho
	//sempre che il primo comando (con indice di IDFormatTable = 0) rappresenta il formattatore
	//già associata al control. 

	for (int i = 0; IDFormatTable[i].nIndex != -1; i++)
	{		
		if (IDFormatTable[i].uiID == nID)
		{
			int nFmtIdx = AfxGetFormatStyleTable()->GetFormatIdx(m_FmtCmdArray.GetAt(i));
			//non devo fare nulla, ho selezionato quello già associato al control.
			if (nFmtIdx == m_nFormatIdx) return;
			//gli assegno il nuovo formattatore e sfrutto il metodo OnFormatStyleChange per la 
			//nuova formattazione del valore associato al control
			//devo conservare il vecchio indice del formattatore per effettuare la GetValue
			//in maniera corretta.
			m_nNewFormatIdx = nFmtIdx;
			FromHandle(m_hWnd)->PostMessage(UM_FORMAT_STYLE_CHANGED);
			return;
		}
	}	
}
//-----------------------------------------------------------------------------
void CParsedCombo::SetNewFormatIdx ()
{
	if (m_nNewFormatIdx >= 0 && m_nFormatIdx != m_nNewFormatIdx)
		AttachFormatter(m_nNewFormatIdx);
}

//-----------------------------------------------------------------------------
BOOL CParsedCombo::GetToolTipProperties(CTooltipProperties& tp) 
{ 
	if (__super::GetToolTipProperties(tp))
	{
		return TRUE;
	}
	
	int nIdx = this->GetCurSel();
	if (nIdx < 0)
		return FALSE;

	TCHAR szStr[1000];
	if (GetAssociatedText(nIdx, szStr) > 0)
	{
		CDC* pDC = GetDC();
		CSize sz = ::GetEditSize(pDC, GetFont(), szStr);
		ReleaseDC(pDC);

		CRect rect;
		GetClientRect(rect);

		if (sz.cx > rect.Width())
		{
			tp.m_strText = szStr;
			return TRUE;
		}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
int CParsedCombo::GetAssociatedTextLen(int nIdx)
{
	TCHAR szStr[1000];
	return GetAssociatedText(nIdx, szStr);
}	

//=============================================================================
//			Class CStrCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CStrCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CStrCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CStrCombo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CStrCombo::CStrCombo()
	:
	CParsedCombo	()
{
	SetCtrlStyle(STR_STYLE_ALL);
}
                        
//-----------------------------------------------------------------------------
CStrCombo::CStrCombo(UINT nBtnIDBmp, DataStr* pData)
	:
	CParsedCombo	(nBtnIDBmp)
{
	SetCtrlStyle(STR_STYLE_ALL);

	Attach(pData);
}
                             
//-----------------------------------------------------------------------------
void CStrCombo::Attach(DataObj* pDataObj)
{
	ASSERT(pDataObj == NULL || pDataObj->GetDataType() == GetDataType());

	CParsedCtrl::Attach(pDataObj);

	if (GetCtrlData() && GetCtrlData()->IsKindOf(RUNTIME_CLASS(DataStr)) && ((DataStr*)GetCtrlData())->IsUpperCase())
		m_dwCtrlStyle |= STR_STYLE_UPPERCASE;
}

//-----------------------------------------------------------------------------
BOOL CStrCombo::OnInitCtrl()
{
	VERIFY(CParsedCombo::OnInitCtrl());
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CStrCombo::GetValue()
{
	switch (GetDataType().m_wType)
	{
		case DATA_STR_TYPE:
			{
				DataStr aValue;
				CParsedCombo::GetValue(aValue);
				return aValue.GetString();
			}
		case DATA_LNG_TYPE:
			{
				DataLng aValue;
				CParsedCombo::GetValue(aValue);
				return aValue.Str(0,0);
			}
		default:
			ASSERT(FALSE);
	}

	return _T("");
}

//-----------------------------------------------------------------------------
void CStrCombo::GetValue(DataObj& aValue)
{
	__super::GetValue(aValue);

	if (aValue.IsKindOf(RUNTIME_CLASS(DataStr)))
		DoMaskedGetValue(aValue.Str(), aValue);
}

//-----------------------------------------------------------------------------
void CStrCombo::SetValue(const DataObj& aValue)
{
	__super::SetValue(aValue);
	
	m_sReadOnlyText = FormatData(GetCtrlData());

	Invalidate();
}

//-----------------------------------------------------------------------------
void CStrCombo::DoSetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	__super::SetValue( GetString(&aValue) );

	m_sReadOnlyText = FormatData(GetCtrlData());

	if (m_pReadOnlyEdit)
	{
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}

	Invalidate();
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CStrCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataObj* pData = GetDataObjFromIdx(nIdx);
	if (pData == NULL)
		return -1;

	TB_TCSCPY(pszStr, GetString(pData));
	return _tcslen(pszStr);
}

// Usata dalla OnGetTextLen()
//-----------------------------------------------------------------------------
int CStrCombo::GetAssociatedTextLen(int nIdx)
{
	DataObj* pData = GetDataObjFromIdx(nIdx);
	if (pData == NULL)
		return -1;

	return GetString(pData).GetLength();
}	

//-----------------------------------------------------------------------------
BOOL CStrCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	for (int i=0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		CStateCtrlObj* pStateCtrl = (CStateCtrlObj*) m_StateCtrls.GetAt(i);
		if (pStateCtrl && !pStateCtrl->DoChar(nChar))
		{
			BadInput();
			return TRUE;
		}
	}

	if (!IsStrChar(nChar, m_dwCtrlStyle))
	{
		BadInput();
		return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CStrCombo::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR)
{
	CParsedCombo::SetValue((LPCTSTR)m_strBadVal);

	return CParsedCtrl::FormatErrorMessage(nIDP, m_strBadVal);
}
	
//-----------------------------------------------------------------------------
int CStrCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
int CStrCombo::AddAssociation(LPCTSTR lpszAssoc, LPCTSTR pszValue)
{
	return AddAssociation(lpszAssoc, DataStr(pszValue), TRUE);
}

//-----------------------------------------------------------------------------
BOOL CStrCombo::IsValidStr(LPCTSTR pszStr)
{
	if ((m_dwCtrlStyle & STR_STYLE_NO_EMPTY) == STR_STYLE_NO_EMPTY)
	{
		CString str(pszStr);
		str.TrimRight();
		
		if (str.IsEmpty())
		{
			m_nErrorID = STR_EDIT_EMPTY;
			return FALSE;
		}
	}
	return TRUE;
}

//=============================================================================
//			Class CDescriptionCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDescriptionCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CDescriptionCombo, CParsedCombo)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BOOL CDescriptionCombo::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName/* = _T("")*/)
{
	if (! __super::SubclassEdit(IDC, pParent, strName))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CDescriptionCombo::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	ResizableCtrl::DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
CDescriptionCombo::CDescriptionCombo()
	:
	CParsedCombo	()
{
	m_bNoResetAssociations = TRUE;
}
                        
//-----------------------------------------------------------------------------
CDescriptionCombo::CDescriptionCombo(UINT nBtnIDBmp, DataObj* pData)
	:
	CParsedCombo	(nBtnIDBmp, pData)
{
	m_bNoResetAssociations = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDescriptionCombo::PreCreateWindow(CREATESTRUCT& cs) 
{
	cs.style |= (CBS_DROPDOWNLIST); 
	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
BOOL CDescriptionCombo::OnInitCtrl()
{
	VERIFY(__super::OnInitCtrl());

	if (GetCtrlData())
	{
		ASSERT(GetDataType() == GetCtrlData()->GetDataType());
	}

	ShowDescription();

	return TRUE;
}

//-----------------------------------------------------------------------------
int CDescriptionCombo::GetInputWidth() const
{ 
	if (m_nMaxItemsLen < 1)
		return -1;

	CDC* pDC = const_cast<CDescriptionCombo*>(this)->GetDC();

	CSize cs = ::GetEditSize(pDC, GetPreferredFont(), m_nMaxItemsLen, 1);

	const_cast<CDescriptionCombo*>(this)->ReleaseDC(pDC);

	return cs.cx; 
}

//-----------------------------------------------------------------------------
void CDescriptionCombo::ShowDescription (BOOL bShowDescription/* = TRUE*/) 
{ 
	m_bShowHKLDescription = bShowDescription;

	if (m_bShowHKLDescription)
	{
		ASSERT_TRACE(m_pHotKeyLink, _T("\nCDescriptionCombo::ShowDescription was called on control without HotLink\n"));
		if (m_hWnd)
		{
			ASSERT_TRACE(((GetStyle() & CBS_DROPDOWNLIST) == CBS_DROPDOWNLIST), _T("\nCDescriptionCombo::ShowDescription was called on control without CBS_DROPDOWNLIST style\n"));
		}
		if (m_pHotKeyLink)
		{
			ASSERT_TRACE(m_pHotKeyLink->IsFillListBoxEnabled (), _T("\nCDescriptionCombo::ShowDescription was called on control binded to HotLink lacking in combo access\n"));
		}
	}
}

//-----------------------------------------------------------------------------
void CDescriptionCombo::AddDescription (const DataObj* pDataObj, const CString& str) 
{
	int nIdx = -1;
	if (m_bSorted && m_hWnd)
	{
		nIdx = AddString(str);
		m_DataAssociations.InsertAt(nIdx, pDataObj->DataObjClone());
		m_DescriptionsBuffer.InsertAt(nIdx, CString(str));
	}
	else
	{
		if (m_hWnd) nIdx = AddString(str);

		m_DataAssociations.Add(pDataObj->DataObjClone());
		m_DescriptionsBuffer.Add(CString(str));
	}
	if (nIdx > -1)
		SetCurSel(nIdx);

}

//-----------------------------------------------------------------------------
BOOL CDescriptionCombo::GetDescription (const DataObj* pDataObj, CString& str) const
{
	ASSERT_VALID(pDataObj);
	if (IsShowDescription())
	{
		if (pDataObj->IsEmpty())
		{
			str.Empty();
			return TRUE;
		}

		if (m_pHotKeyLink)
		{
			int nIdx = GetIdxFromDataObj(*pDataObj);
			if (nIdx >= 0 && nIdx < m_DescriptionsBuffer.GetCount() && m_DescriptionsBuffer.GetCount() > 0)
			{
				str = this->m_DescriptionsBuffer[nIdx];
				return TRUE;
			}
			
			if (m_pHotKeyLink->IsHKLSimulated())
			{
				if (m_pHotKeyLink->SearchComboKeyDescription(pDataObj, str))
				{
					const_cast<CDescriptionCombo*>(this)->AddDescription (pDataObj, str);

					return TRUE;
				}
			}
			else if (!m_pHotKeyLink->IsHotLinkRunning() && m_pHotKeyLink->DoFindRecord(const_cast<DataObj*>(pDataObj)))
			{
				str = m_pHotKeyLink->GetHKLDescription();

				const_cast<CDescriptionCombo*>(this)->AddDescription (pDataObj, str);

				return TRUE;
			}
		}
	}
	str = pDataObj->Str(0,0);
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CDescriptionCombo::FormatData (const DataObj* pDataObj, BOOL bEnablePadding/* = FALSE*/) const
{
	CString str;
	if (GetDescription(pDataObj, str))
		return str;

	return __super::FormatData (pDataObj, bEnablePadding);
}

//-----------------------------------------------------------------------------
// Usata dalla OnGetText()
int CDescriptionCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataObj* pData = GetDataObjFromIdx(nIdx);
	if (pData == NULL)
		return -1;

	CString str;
	if (GetDescription(pData, str))
	{
		TB_TCSCPY(pszStr, (LPCTSTR) str);
		return str.GetLength();
	}
	
	TB_TCSCPY(pszStr, pData->Str(0,0));
	return _tcslen(pszStr);
}

//-----------------------------------------------------------------------------
int CDescriptionCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
void CDescriptionCombo::DoSetValue(const DataObj& aValue)
{             
	CString str;
	if (!GetDescription(&aValue, str))
	{
		AddAssociation(aValue.Str(0,0), aValue, FALSE);
	}

	__super::SetValue((LPCTSTR)str);
	
	m_sReadOnlyText = str;
	if (m_pReadOnlyEdit)
	{
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

//-----------------------------------------------------------------------------
void CDescriptionCombo::SetValue(const DataObj& aValue)
{
	__super::SetValue(aValue);
	
	m_sReadOnlyText = FormatData(GetCtrlData());
	SetWindowText(m_sReadOnlyText);

	Invalidate();
}

//=============================================================================
//			Class CIntCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CIntCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CStrCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CIntCombo::CIntCombo()
	:
	CParsedCombo	(),
	m_nMin			(SHRT_MIN),
	m_nMax			(SHRT_MAX),
	m_nCurValue		(0)
{}
                        
//-----------------------------------------------------------------------------
CIntCombo::CIntCombo(UINT nBtnIDBmp, DataInt* pData)
	:
	CParsedCombo	(nBtnIDBmp, pData),
	m_nMin			(SHRT_MIN),
	m_nMax			(SHRT_MAX),
	m_nCurValue		(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//

}
                        
//-----------------------------------------------------------------------------
void CIntCombo::SetRange(int nMin, int nMax)
{                      
	if (nMin > nMax)
		nMax = nMin;

	m_nMin = nMin;
	m_nMax = nMax > SHRT_MAX ? SHRT_MAX : nMax;
}

//-----------------------------------------------------------------------------
void CIntCombo::SetValue(int nValue)
{
	m_nCurValue = nValue;
	TCHAR szBuffer[256];
	Format(nValue, szBuffer);
	
	CParsedCombo::SetValue(szBuffer);
}

//-----------------------------------------------------------------------------
int CIntCombo::GetValue()
{
	DataInt aValue;

	CParsedCombo::GetValue(aValue);
	return (int)aValue;
}

//-----------------------------------------------------------------------------
void CIntCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (int) (DataInt&) aValue );

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

//-----------------------------------------------------------------------------
int CIntCombo::Format(int nValue, LPTSTR pszStr)
{
	CString strBuffer;

	CIntFormatter* pFormatter = NULL;
	
	SetNewFormatIdx();
	
	if (m_nFormatIdx >= 0)
		pFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
	if (pFormatter && pFormatter->GetFormat() != CIntFormatter::LETTER && pFormatter->GetFormat() != CIntFormatter::ENCODED)
	{
		if	(
				pFormatter->IsZeroPadded() &&
				pFormatter->GetPaddedLen() > 0	&&
				pFormatter->GetPaddedLen() < 7
			)
			pFormatter->Format(&nValue, strBuffer, FALSE);
		else
			if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
				pFormatter->Format(&nValue, strBuffer, FALSE);
	}
	else
		if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
		{
			const rsize_t nLen = 7;
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(nLen);
			_stprintf_s(szBuffer, nLen, _T("%d"), nValue);
			strBuffer.ReleaseBuffer();
		}
		
	TB_TCSCPY(pszStr, strBuffer);
	return _tcslen(pszStr);
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CIntCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataInt* pData = (DataInt*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	return Format((int) *pData, pszStr);
}

//-----------------------------------------------------------------------------
BOOL CIntCombo::IsValid()
{
	if (!CParsedCombo::IsValid())
		return FALSE;
	
	if (GetChildEdit() == NULL)
		return TRUE;

	return IsValidInt(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CIntCombo::IsValidInt(int nValue)
{
	// bad nValue or bad range
	if ((nValue >= m_nMin) && (nValue <= m_nMax))
	{
		m_nCurValue = nValue;
		return TRUE;
	}

	m_nErrorID = INT_EDIT_OUT_RANGE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CIntCombo::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR)
{
	CParsedCombo::SetValue((LPCTSTR)m_strBadVal);

	if (nIDP == INT_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), (LPCTSTR) m_strBadVal, (int) m_nMin, (int )m_nMax);

	return CParsedCtrl::FormatErrorMessage(nIDP, m_strBadVal);
}
	
// Spin controls will send scroll messages
//-----------------------------------------------------------------------------
void CIntCombo::DoSpinScroll(UINT nSBCode)
{                  
	int nDelta;
	
	switch (nSBCode)
	{
		case SB_LINEDOWN	: 
		case SB_PAGEDOWN	: nDelta = -1; break;
		case SB_LINEUP		: 
		case SB_PAGEUP		: nDelta = +1; break;
		default				: return;
	}	

	//Get the number in the control.
	int nOld = GetValue();

	if ((nOld == m_nMin && nDelta < 0) || (nOld == m_nMax && nDelta > 0))
		BadInput();
	else
    {
		SetValue(nOld + nDelta);
		SetModifyFlag(TRUE);

		// set the focus to this edit item and select it all
		SetCtrlFocus(TRUE);
		UpdateCtrlData(TRUE);
		return;
	}

	// set the focus to this edit item and select it all
	SetCtrlFocus(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CIntCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetEditSel();
	CString	strValue; CParsedCombo::GetValue (strValue);

	CIntFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CIntFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
					? pFormatter->Get1000Separator()[0]
					: 0;
	int nVal = _tstoi(pFormatter ? pFormatter->UnFormat(strValue) : strValue);
	int	nPos = (int) LOWORD(dwPos);

	if (nChar == _T('-'))
	{	
		if (m_nMin >= 0 || (nVal == 0 && dwPos > 0))
		{
			BadInput();
			return TRUE;
		}
		
		if (nVal)
		{
			nVal = -nVal;
			SetValue(nVal);
			CString	strTmp; CParsedCtrl::GetValue (strTmp);

			SetModifyFlag(TRUE);

			if	(
					strTmp[0] != strValue[0] &&
					_istdigit(strTmp[0]) != _istdigit(strValue[0])
				)
				if (_istdigit(strTmp[0]))
					nPos--;
				else
					nPos++;

			SetCtrlSel(nPos, nPos);

			return TRUE;
		}

		// il control vuoto e` accettato
		return FALSE;
	}

	if	(
			(!_istdigit(nChar) && nChar != VK_BACK) ||
			(dwPos == 0 && !strValue.IsEmpty() && !_istdigit(strValue[0]))
		)
	{
		BadInput();
		return TRUE;
	}

	if (nChar == VK_BACK)
		switch (ManageNumericBackKey(strValue, dwPos, nPos, ch1000Sep))
		{
			case -1 : return TRUE;
			case 0	: return FALSE;
		}

	int nCurNr1000Sep = UpdateNumericString(nVal, strValue, dwPos, nPos, nChar, ch1000Sep);
	if (nCurNr1000Sep < 0)
	{
		BadInput();
		return TRUE;
	}

	double dVal = _tstof(pFormatter ? pFormatter->UnFormat(strValue) : strValue);

	if (dVal < SHRT_MIN || dVal > SHRT_MAX)
	{
		BadInput();
		return TRUE;
	}

	UpdateNumericInput(dVal, strValue, dwPos, nPos, ch1000Sep, nCurNr1000Sep);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CIntCombo::DoKeyDown(UINT nChar)
{
	if (CParsedCombo::DoKeyDown(nChar))
		return TRUE;

	if (GetChildEdit() == NULL)
		return FALSE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		CString	strValue;
		CParsedCombo::GetValue (strValue);

		DWORD dwPos = GetEditSel();
		if (LOWORD(dwPos) == HIWORD(dwPos))
		{
			int nPos = (int) LOWORD(dwPos);
			if (nPos == strValue.GetLength())
				return TRUE;

			CIntFormatter* pFormatter = NULL;

			if (m_nFormatIdx >= 0)
				pFormatter = (CIntFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

			TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
							? pFormatter->Get1000Separator()[0]
							: 0;

			SetCtrlSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
		}

		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CIntCombo::DoKillFocus (CWnd* pWnd)
{
	if (GetChildEdit() != NULL && m_wndEdit.GetModify())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedCombo::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
int CIntCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
int CIntCombo::AddAssociation(LPCTSTR lpszAssoc, int nValue)
{
	return AddAssociation(lpszAssoc, DataInt(nValue), TRUE);
}

//-----------------------------------------------------------------------------
BOOL CIntCombo::OnInitCtrl()
{
	m_nCtrlLimit = 6;

	VERIFY(CParsedCombo::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CIntCombo::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	CIntFormatter* pFormatter = NULL;
	CIntFormatter* pNewFormatter = NULL;

	if (m_nFormatIdx > 0)
		pFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
	if (m_nNewFormatIdx > 0 && m_nNewFormatIdx != m_nFormatIdx)
		pNewFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nNewFormatIdx, m_pFormatContext);


	if (pFormatter)
		switch (pFormatter->GetFormat())
		{
			case CIntFormatter::LETTER:	
			case CIntFormatter::ENCODED:			
								return 0L;
			default:
								break; 
		}


	if (pNewFormatter)
		switch (pNewFormatter->GetFormat())
		{
			case CIntFormatter::LETTER:	
			case CIntFormatter::ENCODED:			
								return 0L;
			default:
								break; 
		}
	
	DataInt aValue;

	if (m_nCurValue) 
		aValue.Assign(m_nCurValue);
	else
		aValue.Assign(GetValue());
		
	if (IsValid(aValue))
	{
		BOOL bModify = GetModifyFlag();
		SetValue(aValue);
		if (bModify)
			SetModifyFlag(TRUE);
		SetCtrlMaxLen(GetInputCharLen(), TRUE);
	}
	else
	{
		ClearCtrl();
		SetModifyFlag(FALSE);
	}		

	return 0L;
}

//=============================================================================
//			Class CLongIDCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLongIDCombo, CDescriptionCombo)
     
//-----------------------------------------------------------------------------
CLongIDCombo::CLongIDCombo()
	:
	CDescriptionCombo	()
{
}
                        
//-----------------------------------------------------------------------------
CLongIDCombo::CLongIDCombo(UINT nBtnIDBmp, DataLng* pData)
	:
	CDescriptionCombo ()
{
	CParsedCtrl::Attach(nBtnIDBmp);
	CParsedCtrl::Attach(pData);
}
                             
//-----------------------------------------------------------------------------
int CLongIDCombo::AddAssociation(LPCTSTR lpszAssoc, long nValue)
{
	return __super::AddAssociation(lpszAssoc, DataLng(nValue));
}

//-----------------------------------------------------------------------------
// Anche la - GetDescription - concorre alla costruzione della combo
void CLongIDCombo::DoSetValue(const DataObj& aValue)
{
	if (m_DescriptionsBuffer.GetCount() > 0)
	{
		int nIdx = GetIdxFromDataObj(aValue);
		if (nIdx >= 0)
			SetCurSel(nIdx);
	}
}

//=============================================================================
//			Class CLongCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLongCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CLongCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CLongCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLongCombo::CLongCombo(DataLng* pData/*= NULL*/)
	:
	CParsedCombo			(pData),
	m_lMin					(LONG_MIN),
	m_lMax					(LONG_MAX),
	m_lCurValue				(0)
{}

//-----------------------------------------------------------------------------
CLongCombo::CLongCombo(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedCombo			(nBtnIDBmp, pData),
	m_lMin					(LONG_MIN),
	m_lMax					(LONG_MAX),
	m_lCurValue				(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CLongCombo::SetRange(int nMin, int nMax)
{
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = nMin;
	m_lMax = nMax;
}

//-----------------------------------------------------------------------------
void CLongCombo::SetValue(long nValue)
{
	m_lCurValue = nValue;
	TCHAR szBuffer[256];
	Format(nValue, szBuffer);

	CParsedCombo::SetValue(szBuffer);
}

//-----------------------------------------------------------------------------
void CLongCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));

	SetValue( (long) (DataLng&) aValue );

	m_sReadOnlyText = FormatData(GetCtrlData());

	if (m_pReadOnlyEdit)
	{
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

//-----------------------------------------------------------------------------
int CLongCombo::Format (long nValue, LPTSTR pszStr)
{	
	CString strBuffer;
	CLongFormatter* pFormatter = NULL;

	SetNewFormatIdx();

	if (m_nFormatIdx >= 0)
		pFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
	if (pFormatter && pFormatter->GetFormat() != CLongFormatter::LETTER && pFormatter->GetFormat() != CLongFormatter::ENCODED)
	{
		if	(
				pFormatter->IsZeroPadded() &&
				pFormatter->GetPaddedLen() > 0	&&
				pFormatter->GetPaddedLen() < 12
			)
				pFormatter->Format(&nValue, strBuffer, FALSE);
		else if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
				pFormatter->Format(&nValue, strBuffer, FALSE);
	}
	else if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
	{
		const rsize_t nLen = 12;
		TCHAR* szBuffer = strBuffer.GetBufferSetLength(nLen);
		_stprintf_s(szBuffer, nLen, _T("%ld"), nValue);
		strBuffer.ReleaseBuffer();
	}
		
	TB_TCSCPY(pszStr, strBuffer);
	return _tcslen(pszStr);
}

//-----------------------------------------------------------------------------
long CLongCombo::GetValue()
{
	DataLng	aValue;
	CParsedCombo::GetValue (aValue);
	return (long)aValue;
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CLongCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataLng* pData = (DataLng*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;
		
	return Format((long) *pData, pszStr);
}

//-----------------------------------------------------------------------------
BOOL CLongCombo::IsValid()
{
	if (!CParsedCombo::IsValid())
		return FALSE;
	
	if (GetChildEdit() != NULL)
		return TRUE;

	return IsValidLong(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CLongCombo::IsValidLong(long nValue)
{
	// bad nValue or bad range
	if ((nValue >= m_lMin) && (nValue <= m_lMax))
	{
		m_lCurValue = nValue;
		return TRUE;
	}

	m_nErrorID = LONG_EDIT_OUT_RANGE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CLongCombo::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR)
{
	CParsedCombo::SetValue((LPCTSTR)m_strBadVal);

	if (nIDP == LONG_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), (LPCTSTR) m_strBadVal, (long) m_lMin, (long) m_lMax);

	return CParsedCtrl::FormatErrorMessage(nIDP, m_strBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CLongCombo::DoSpinScroll(UINT nSBCode)
{                  
	long nDelta;
	
	switch (nSBCode)
	{
		case SB_LINEDOWN	: 
		case SB_PAGEDOWN	: nDelta = -1; break;
		case SB_LINEUP		: 
		case SB_PAGEUP		: nDelta = +1; break;
		default				: return;
	}	

	//Get the number in the control.
	long nOld = GetValue();

	if ((nOld == m_lMin && nDelta < 0) || (nOld == m_lMax && nDelta > 0))
		BadInput();
	else
	{
		SetValue(nOld + nDelta);
		SetModifyFlag(TRUE);

		// set the focus to this edit item and select it all
		SetCtrlFocus(TRUE);
		UpdateCtrlData(TRUE);
		return;
	}

	// set the focus to this edit item and select it all
	SetCtrlFocus(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CLongCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetEditSel();
	CString	strValue; CParsedCombo::GetValue (strValue);

	CLongFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CLongFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
					? pFormatter->Get1000Separator()[0]
					: 0;
	long nVal = _tstol(pFormatter ? pFormatter->UnFormat(strValue) : strValue);
	int	nPos = (int) LOWORD(dwPos);

	if (nChar == _T('-'))
	{	
		if (m_lMin >= 0 || (nVal == 0 && dwPos > 0))
		{
			BadInput();
			return TRUE;
		}
		
		if (nVal)
		{
			nVal = -nVal;
			SetValue(nVal);
			CString	strTmp; CParsedCtrl::GetValue (strTmp);

			SetModifyFlag(TRUE);

			if	(
					strTmp[0] != strValue[0] &&
					_istdigit(strTmp[0]) != _istdigit(strValue[0])
				)
				if (_istdigit(strTmp[0]))
					nPos--;
				else
					nPos++;

			SetCtrlSel(nPos, nPos);

			return TRUE;
		}

		// il control vuoto e` accettato
		return FALSE;
	}

	if	(
			(!_istdigit(nChar) && nChar != VK_BACK) ||
			(dwPos == 0 && !strValue.IsEmpty() && !_istdigit(strValue[0]))
		)
	{
		BadInput();
		return TRUE;
	}

	if (nChar == VK_BACK)
		switch (ManageNumericBackKey(strValue, dwPos, nPos, ch1000Sep))
		{
			case -1 : return TRUE;
			case 0	: return FALSE;
		}

	int nCurNr1000Sep = UpdateNumericString(nVal, strValue, dwPos, nPos, nChar, ch1000Sep);
	if (nCurNr1000Sep < 0)
	{
		BadInput();
		return TRUE;
	}

	double dVal = _tstof(pFormatter ? pFormatter->UnFormat(strValue) : strValue);

	if (dVal < LONG_MIN || dVal > LONG_MAX)
	{
		BadInput();
		return TRUE;
	}

	UpdateNumericInput(dVal, strValue, dwPos, nPos, ch1000Sep, nCurNr1000Sep);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLongCombo::DoKeyDown(UINT nChar)
{
	if (CParsedCombo::DoKeyDown(nChar))
		return TRUE;

	if (GetChildEdit() == NULL)
		return FALSE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		CString	strValue;
		CParsedCombo::GetValue (strValue);

		DWORD dwPos = GetEditSel();
		if (LOWORD(dwPos) == HIWORD(dwPos))
		{
			int nPos = (int) LOWORD(dwPos);
			if (nPos == strValue.GetLength())
				return TRUE;

			CLongFormatter* pFormatter = NULL;

			if (m_nFormatIdx >= 0)
				pFormatter = (CLongFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

			TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
							? pFormatter->Get1000Separator()[0]
							: 0;

			SetCtrlSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
		}

		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CLongCombo::DoKillFocus (CWnd* pWnd)
{
	if (GetChildEdit() != NULL && m_wndEdit.GetModify())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedCombo::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
int CLongCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
int CLongCombo::AddAssociation(LPCTSTR lpszAssoc, long nValue)
{
	return AddAssociation(lpszAssoc, DataLng(nValue));
}

//-----------------------------------------------------------------------------
BOOL CLongCombo::OnInitCtrl()
{
	m_nCtrlLimit = 11;

	VERIFY(CParsedCombo::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CLongCombo::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	CLongFormatter* pFormatter = NULL;
	CLongFormatter* pNewFormatter = NULL;

	if (m_nFormatIdx > 0)
		pFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
	if (m_nNewFormatIdx > 0 && m_nNewFormatIdx != m_nFormatIdx)
		pNewFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nNewFormatIdx, m_pFormatContext);

	if (pFormatter)
		switch (pFormatter->GetFormat())
		{
			case CLongFormatter::LETTER:	
			case CLongFormatter::ENCODED:			
								return 0L;
			default:
								break; 
		}

	 if (pNewFormatter)
		switch (pNewFormatter->GetFormat())
		{
			case CLongFormatter::LETTER:	
			case CLongFormatter::ENCODED:			
								return 0L;
			default:
								break; 
		}
		
	DataLng aValue;

	if (m_lCurValue) 
		aValue.Assign(m_lCurValue);
	else
		aValue.Assign(GetValue());
		
	if (IsValid(aValue))
	{
		BOOL bModify = GetModifyFlag();
		SetValue(aValue);
		if (bModify)
			SetModifyFlag(TRUE);
		SetCtrlMaxLen(GetInputCharLen(), TRUE);
	}
	else
	{
		ClearCtrl();
		SetModifyFlag(FALSE);
	}

	return 0L;
}
//=============================================================================
//			Class CDataObjTypesCombo implementation
//
// Questa combo box contiene tutti i DataType di MicroArea tennendo conto
// che per i DataEnum vengono mostrate all'utente TUTTE i possibili
// Enumerativi caricati correntemente nell'applicazione
//
// Per gestire cio` si sfrutta come tipo base di combo la CLongCombo associando
// ogni tipo di dato, da visualizzare nella tendina, ad una lista di DataLong
// valorizzati con un MAKELONG(DataTye.m_wType, DataTye.m_wTag)
//
//=============================================================================
IMPLEMENT_DYNCREATE (CDataObjTypesCombo, CLongCombo)

BEGIN_MESSAGE_MAP(CDataObjTypesCombo, CLongCombo)
	//{{AFX_MSG_MAP(CDataObjTypesCombo)
	ON_WM_CONTEXTMENU	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CDataObjTypesCombo::CDataObjTypesCombo()
	:
	CLongCombo(new DataLng(DATA_STR_TYPE))
{
	// Inibisce la formattazione di default dello CLongEdit
	m_nFormatIdx = NO_FORMAT;
}

//-----------------------------------------------------------------------------
CDataObjTypesCombo::~CDataObjTypesCombo()
{
	delete GetCtrlData();
}

//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::OnInitCtrl()
{
	VERIFY(CParsedCombo::OnInitCtrl());

	for (WORD i = DATA_NULL_TYPE + 1; i <= LAST_USED_DATA_TYPE; i++)
		m_DataTypes.Add(i);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CDataObjTypesCombo::OnFillListBox()
{
	for (int i = 0; i <= m_DataTypes.GetUpperBound(); i++)
	{
		switch (m_DataTypes[i])
		{
			case DATA_ENUM_TYPE	:
			{
				const EnumTagArray* pTags = AfxGetEnumsTable()->GetEnumTags();
					
				for (int j = 0; j < pTags->GetSize(); j++)
				{
					EnumTag* pTag = pTags->GetAt(j); 
					AddAssociation(pTag->GetTagTitle(), MAKELONG(m_DataTypes[i], pTag->GetTagValue()));
				}

				break;
			}

			case DATA_DATE_TYPE	:
			{
				AddAssociation(_T(" ") + FromDataTypeToDescr(DataType(DATA_DATE_TYPE, 0)), MAKELONG(DATA_DATE_TYPE, 0));

				// Viene aggiunto il tipo predefinito DataOra
				AddAssociation
				(
					_T(" ")+FromDataTypeToDescr(DataType::DateTime),
					MAKELONG(DATA_DATE_TYPE, DataType::DateTime.m_wTag)
				);

				// Viene aggiunto il tipo predefinito Ora
				AddAssociation
				(
					_T(" ")+FromDataTypeToDescr(DataType::Time),
					MAKELONG(DATA_DATE_TYPE, DataType::Time.m_wTag)
				);

				break;
			}

			case DATA_LNG_TYPE	:	//@@ElapsedTime
			{
				AddAssociation(_T(" ") + FromDataTypeToDescr(DataType(DATA_LNG_TYPE, 0)), MAKELONG(DATA_LNG_TYPE, 0));

				// Viene aggiunto il tipo predefinito Tempo
				AddAssociation
				(
					_T(" ") + FromDataTypeToDescr(DataType::ElapsedTime),
					MAKELONG(DATA_LNG_TYPE, DataType::ElapsedTime.m_wTag)
				);

				break;
			}
			
			// tipi di dato non pubblici
			case DATA_NULL_TYPE	:	
			case DATA_BLOB_TYPE	:	
			case DATA_ARRAY_TYPE :	
			case DATA_RECORD_TYPE :	
			case DATA_SQLRECORD_TYPE:
			case DATA_VARIANT_TYPE :
				continue;

			default	:
			{
				CString sDescri = FromDataTypeToDescr(DataType(m_DataTypes[i], 0));
				if (sDescri.IsEmpty())
					ASSERT(FALSE);
				else
					AddAssociation(_T(" ") + sDescri, MAKELONG(m_DataTypes[i], 0));
				
				break;
			}
		}
	}
}

// Usata dalla OnGetTextLen e OnGetText
//-----------------------------------------------------------------------------
int CDataObjTypesCombo::GetAssociatedText(int, LPTSTR)
{
	// si lascia chiamare la DefWindowProc poiche` quello che si vuole
	// visualizzare e` proprio cio` che compare nella tendina
	return -1;
}

// Usata dalla OnGetTextLen e OnGetTextLen
//-----------------------------------------------------------------------------
int CDataObjTypesCombo::GetAssociatedTextLen(int)
{
	// si lascia chiamare la DefWindowProc poiche` quello che si vuole
	// visualizzare e` proprio cio` che compare nella tendina
	return -1;
}

//-----------------------------------------------------------------------------
void CDataObjTypesCombo::DoSetValue(const DataObj& aData)
{
	ASSERT(aData.GetDataType() == DATA_LNG_TYPE);

	long aValue = (long) (DataLng&) aData;
	CParsedCombo::SetValue(FromDataTypeToDescr(DataType(LOWORD(aValue), HIWORD(aValue))));
}

//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::DoOnChar(UINT nChar)
{
	return CParsedCombo::DoOnChar(nChar);
}

//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::DoKeyUp(UINT nChar)
{
	return CParsedCombo::DoKeyUp(nChar);
}

//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::DoKeyDown(UINT nChar)
{
	if	(
			(nChar == VK_DELETE	|| nChar == VK_BACK)		&&
			(GetStyle() & CBS_STYLES) == CBS_DROPDOWNLIST	&&
			(GetCtrlStyle() & COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL) == COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL
		)
	{
	    return TRUE;
	}
	else
		return CParsedCombo::DoKeyDown(nChar);
}

//-----------------------------------------------------------------------------
void CDataObjTypesCombo::DoKillFocus (CWnd* pWnd)
{
	CParsedCombo::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
void CDataObjTypesCombo::SetTypeValue(const DataType& dataType)
{
	((DataLng*) GetCtrlData())->Assign(MAKELONG(dataType.m_wType, dataType.m_wTag));
	UpdateCtrlView();
}

//-----------------------------------------------------------------------------
DataType CDataObjTypesCombo::GetTypeValue()
{
	DWORD dwDataType = (long) *((DataLng*) GetCtrlData());
	return DataType(LOWORD(dwDataType), HIWORD(dwDataType));
}

//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::IsPrecisionEnabled()
{
	switch (GetTypeValue().m_wType)
	{
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE: return TRUE;
	};
	
	return FALSE;
}

// disabilito il Pulisci nella combo dei tipi che non ha senso!
//-----------------------------------------------------------------------------
BOOL CDataObjTypesCombo::DoContextMenu (CWnd* pWnd, CPoint ptPoint)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDataObjTypesCombo::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
}

//=============================================================================
//			Class CDoubleCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDoubleCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CDoubleCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CDoubleCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDoubleCombo::CDoubleCombo()
	:
	CParsedCombo	(),
	m_dMin			(-DBL_MAX),
	m_dMax			(DBL_MAX),
	m_nDec			(DEFAULT_N_DEC),
	m_dCurValue		(0.)
{}

//-----------------------------------------------------------------------------
CDoubleCombo::CDoubleCombo(UINT nBtnIDBmp, DataDbl* pData)
	:
	CParsedCombo	(nBtnIDBmp, pData),
	m_dMin			(-DBL_MAX),
	m_dMax			(DBL_MAX),
	m_nDec			(DEFAULT_N_DEC),
	m_dCurValue		(0.)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//

}

//-----------------------------------------------------------------------------
void CDoubleCombo::SetRange(double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;

	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}

//-----------------------------------------------------------------------------
int CDoubleCombo::GetCtrlNumDec ()
{
	CDblFormatter* pFormatte = NULL;
	
	if (m_nDec < 0 && m_nFormatIdx >= 0)
		pFormatte = (CDblFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
		
	if (pFormatte)
		return pFormatte->GetDecNumber();

	return m_nDec;
}

	
//-----------------------------------------------------------------------------
void CDoubleCombo::SetValue(double nValue)
{
	m_dCurValue = nValue;
	TCHAR szBuffer[256];
	Format(nValue, szBuffer);
	
	CParsedCombo::SetValue(szBuffer);
}

//-----------------------------------------------------------------------------
int CDoubleCombo::Format(double nValue, LPTSTR pszStr)
{
	CString strBuffer;

	if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
	{
		CDblFormatter* pFormatter = NULL;
		SetNewFormatIdx();

		if (m_nFormatIdx >= 0)
			pFormatter = (CDblFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
			
		if	(
				pFormatter												&&
				pFormatter->GetFormat() != CDblFormatter::LETTER		&&
				pFormatter->GetFormat() != CDblFormatter::ENCODED		&&
				pFormatter->GetFormat() != CDblFormatter::EXPONENTIAL	&&
				pFormatter->GetFormat() != CDblFormatter::ENGINEER
			)
		{
			int nOldDN;
			if (m_nDec >= 0)
				nOldDN = pFormatter->SetDecNumber(m_nDec);

			pFormatter->Format(&nValue, strBuffer, FALSE);

			if (m_nDec >= 0)
				pFormatter->SetDecNumber(nOldDN);
		}
		else
		{
			const rsize_t nLen = 512;
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(nLen);
			int nDec = GetCtrlNumDec();
			if (nDec >= 0)
				_stprintf_s(szBuffer, nLen, _T("%.*f"), nDec, nValue);
			else
				_stprintf_s(szBuffer, nLen, _T("%f"), nValue);

			if (pFormatter && !pFormatter->GetDecSeparator().IsEmpty())
			{
				TCHAR* pDP = _tcschr(szBuffer, DOT_CHAR);
				if (pDP) *pDP = pFormatter->GetDecSeparator()[0];
			}

			strBuffer.ReleaseBuffer();
		}
	}
		
	TB_TCSCPY(pszStr, strBuffer);
	return _tcslen(pszStr);
}
	
//-----------------------------------------------------------------------------
void CDoubleCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (double) ((const DataDbl&) aValue));

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CDoubleCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataDbl* pData = (DataDbl*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	return Format((double) *pData, pszStr);
}

//-----------------------------------------------------------------------------
double CDoubleCombo::GetValue()
{
	DataDbl aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CDoubleCombo::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	__super::GetValue (aValue);

	if ((m_dwCtrlStyle & NUM_STYLE_INVERT_SIGN) == NUM_STYLE_INVERT_SIGN)
		aValue = -((DataDbl&) aValue);
}

//-----------------------------------------------------------------------------
void CDoubleCombo::SetValue(const DataObj& aValue)
{
	if ((m_dwCtrlStyle & NUM_STYLE_INVERT_SIGN) == NUM_STYLE_INVERT_SIGN)
	{
		DataDbl& aDblValue = (DataDbl&) aValue;
		aDblValue = -aDblValue;
		__super::SetValue(aDblValue);
	}
	else
		__super::SetValue(aValue);
}

//-----------------------------------------------------------------------------
BOOL CDoubleCombo::IsValid()
{
	if (!CParsedCombo::IsValid())
		return FALSE;
	
	if (GetChildEdit() == NULL)
		return TRUE;

	return IsValidDouble(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CDoubleCombo::IsValidDouble(double nValue)
{
	// bad value or bad range
	if ((nValue >= m_dMin) && (nValue <= m_dMax))
	{
		m_dCurValue = nValue;
		return TRUE;
	}

	m_nErrorID = DOUBLE_EDIT_OUT_RANGE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CDoubleCombo::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR)
{
	CParsedCombo::SetValue((LPCTSTR)m_strBadVal);

	if (nIDP == DOUBLE_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), (LPCTSTR) m_strBadVal, (double) m_dMin, (double) m_dMax);

	return CParsedCtrl::FormatErrorMessage(nIDP, m_strBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDoubleCombo::DoSpinScroll(UINT nSBCode)
{                  
	double nDelta;
		
	switch (nSBCode)
	{
		case SB_LINEDOWN	: 
		case SB_PAGEDOWN	: nDelta = -1; break;
		case SB_LINEUP		: 
		case SB_PAGEUP		: nDelta = +1; break;
		default				: return;
	}	
	
	if (nSBCode == SB_LINEDOWN || nSBCode == SB_LINEUP)
		for (int i = GetCtrlNumDec(); i > 0; i--) nDelta /= 10.;
	
	//Get the number in the control.
	double nOld = GetValue() + nDelta;
	
	if ((nDelta < 0 && nOld < m_dMin) || (nDelta > 0 && nOld > m_dMax))
		BadInput();
	else                
	{
		SetValue(nOld);
		SetModifyFlag(TRUE);

		// set the focus to this edit item and select it all
		SetCtrlFocus(TRUE);
		UpdateCtrlData(TRUE);
		return;
	}
	
	// set the focus to this edit item and select it all
	SetCtrlFocus(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDoubleCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetEditSel();
	CString	strValue; CParsedCombo::GetValue (strValue);

	CDblFormatter* pFormatter = NULL;
	
	if (m_nFormatIdx >= 0)
		pFormatter = (CDblFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	TCHAR chDecSep = pFormatter && !pFormatter->GetDecSeparator().IsEmpty()
					? pFormatter->GetDecSeparator()[0]
					: pFormatter ? 0 : DOT_CHAR;
	TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
					? pFormatter->Get1000Separator()[0]
					: 0;

	CDblFormatter::RoundingTag  nCurRounding = 
				pFormatter 
				?pFormatter->SetRounding(CDblFormatter::ROUND_NONE) 
					:CDblFormatter::ROUND_NONE;

	double nVal = _tstof(pFormatter ? pFormatter->UnFormat(strValue) : strValue);

	int	nPos = (int) LOWORD(dwPos);

	BOOL bEaten = TRUE;

	int nPointPos;
	int nCurNr1000Sep;

	if (nChar == _T('-'))
	{	
		if (m_dMin >= 0 || (nVal == 0 && dwPos > 0))
			goto bad_input;
		
		if (nVal)
		{
			nVal = -nVal;
			SetValue(nVal);
			CString	strTmp; CParsedCtrl::GetValue (strTmp);

			SetModifyFlag(TRUE);

			if	(
					strTmp[0] != strValue[0] &&
					_istdigit(strTmp[0]) != _istdigit(strValue[0])
				)
				if (_istdigit(strTmp[0]))
					nPos--;
				else
					nPos++;

			SetCtrlSel(nPos, nPos);

			goto exit;
		}

		// il control vuoto e` accettato
		bEaten = FALSE;
		goto exit;
	}

	if (nChar == DOT_CHAR) nChar = chDecSep;

	if	(
			(!_istdigit(nChar) && nChar != VK_BACK && (TCHAR)nChar != chDecSep) ||
			(
				dwPos == 0 && !strValue.IsEmpty() && !_istdigit(strValue[0]) &&
				strValue[0] != chDecSep && strValue[0] != ch1000Sep
			)
		)
		goto bad_input;

	nPointPos = strValue.Find(chDecSep);

	if	(
			(TCHAR)nChar != chDecSep &&
			LOWORD(dwPos) != HIWORD(dwPos) &&
			nPointPos >= LOWORD(dwPos) && nPointPos < HIWORD(dwPos) &&
			HIWORD(dwPos) != strValue.GetLength()
		)
		goto bad_input;

	if (nChar == VK_BACK)
		switch (ManageNumericBackKey(strValue, dwPos, nPos, ch1000Sep, chDecSep))
		{
			case -1 : goto exit;
			case 0	: bEaten = FALSE; goto exit;
		}

	if ((TCHAR)nChar == chDecSep)
	{
		if (nPointPos >= 0)
		{
			strValue = strValue.Left(nPointPos) +  strValue.Mid(nPointPos + 1);

			if (nPointPos < nPos)
			{
				nPos--;
				dwPos = MAKELONG(LOWORD(dwPos) - 1, HIWORD(dwPos) - 1);
			}
		}

		if	(
				GetCtrlNumDec() == 0 ||
				(
					nPointPos > HIWORD(dwPos) &&
					_tstof(pFormatter
							? pFormatter->UnFormat(strValue.Mid(HIWORD(dwPos))).Mid(GetCtrlNumDec())
							: strValue.Mid(HIWORD(dwPos) + GetCtrlNumDec())
						) != 0.0
				)
			)
			goto bad_input;

		nPointPos = nPos;
	}

	if (_istdigit(nChar) && nPointPos >= 0 && nPos > nPointPos)
	{
		if (nPos - nPointPos > GetCtrlNumDec())
			goto bad_input;

		if (LOWORD(dwPos) == HIWORD(dwPos))	// tento di INSERIRE un numero dopo la virgola
			for (int i = strValue.GetLength() - 1; i >= nPointPos + GetCtrlNumDec(); i--)
				if (strValue[i] != _T('0'))
					goto bad_input;
	}

	nCurNr1000Sep = UpdateNumericString(nVal, strValue, dwPos, nPos, nChar, ch1000Sep);
	if (nCurNr1000Sep < 0)
		goto bad_input;

	nVal = _tstof(pFormatter ? pFormatter->UnFormat(strValue) : strValue);

	if (nVal < -DBL_MAX || nVal > DBL_MAX)
		goto bad_input;

	UpdateNumericInput(nVal, strValue, dwPos, nPos, ch1000Sep, nCurNr1000Sep, pFormatter);

	goto exit;

bad_input:
	BadInput();

exit:
	if (pFormatter) pFormatter->SetRounding(nCurRounding);
	return bEaten;
}

//-----------------------------------------------------------------------------
BOOL CDoubleCombo::DoKeyDown(UINT nChar)
{
	if (CParsedCombo::DoKeyDown(nChar))
		return TRUE;

	if (GetChildEdit() == NULL)
		return FALSE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		CString	strValue;
		CParsedCombo::GetValue (strValue);

		DWORD dwPos = GetEditSel();
		if (LOWORD(dwPos) == HIWORD(dwPos))
		{
			int nPos = (int) LOWORD(dwPos);
			if (nPos == strValue.GetLength())
				return TRUE;

			CDblFormatter* pFormatter = NULL;
			
			if (m_nFormatIdx >= 0)
				pFormatter = (CDblFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

			TCHAR chDecSep = pFormatter && !pFormatter->GetDecSeparator().IsEmpty()
							? pFormatter->GetDecSeparator()[0]
							: pFormatter ? 0 : DOT_CHAR;

			if (strValue[nPos] == chDecSep)
			{
				SetCtrlSel(nPos + 1, nPos + 1);
				return TRUE;
			}

			TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
							? pFormatter->Get1000Separator()[0]
							: 0;
			SetCtrlSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
		}

		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CDoubleCombo::DoKillFocus (CWnd* pWnd)
{
	if (GetChildEdit() != NULL && m_wndEdit.GetModify())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedCombo::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
int CDoubleCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
int CDoubleCombo::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return AddAssociation(lpszAssoc, DataDbl(nValue));
}

//-----------------------------------------------------------------------------
BOOL CDoubleCombo::OnInitCtrl()
{
	m_nCtrlLimit = 2*DBL_DIG+1;

	ModifyStyle(0,ES_RIGHT);

	VERIFY(CParsedCombo::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CDoubleCombo::FormatValue(DataDbl& aValue)
{
	if (m_dCurValue) 
		aValue.Assign(m_dCurValue);
	else
		aValue.Assign(GetValue());
		
	if (IsValid(aValue))
	{
		BOOL bModify = GetModifyFlag();
		SetValue(aValue);
		if (bModify)
			SetModifyFlag(TRUE);
		SetCtrlMaxLen(GetInputCharLen(), TRUE);
	}
	else
	{
		ClearCtrl();
		SetModifyFlag(FALSE);
	}		

	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CDoubleCombo::OnFormatStyleChange (WPARAM  /*wParam*/ , LPARAM  /*lParam*/ )
{
	DataDbl	aValue;
	return FormatValue(aValue);	
}


//-----------------------------------------------------------------------------
CString CDoubleCombo::FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const
{
	ASSERT_VALID(pDataObj);
	CString strCell;

	// Viene formattato il dato
	CDblFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CDblFormatter*) (AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext));

	if (pFormatter)
	{
		//se il programmatore ha scelto un numero di decimali diverso da quello del formattatore
		int nOldDN;
		if (m_nDec >= 0)
			nOldDN = pFormatter->SetDecNumber(m_nDec);

		pFormatter->FormatDataObj(*pDataObj, strCell, bEnablePadding);
		if (m_nDec >= 0)
			pFormatter->SetDecNumber(nOldDN);
	}
	else
		return pDataObj->Str();

	return strCell;
}

//=============================================================================
//			Class CMoneyCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMoneyCombo, CDoubleCombo)

BEGIN_MESSAGE_MAP(CMoneyCombo, CDoubleCombo)
	//{{AFX_MSG_MAP(CMoneyCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMoneyCombo::CMoneyCombo()
	:
	CDoubleCombo()
{}

//-----------------------------------------------------------------------------
CMoneyCombo::CMoneyCombo(UINT nBtnIDBmp, DataMon* pData)
	:
	CDoubleCombo(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CMoneyCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleCombo::SetValue( (double) (DataMon&) aValue );

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CMoneyCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataMon* pData = (DataMon*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	return Format((double) *pData, pszStr);
}

//-----------------------------------------------------------------------------
double CMoneyCombo::GetValue()
{
	DataMon aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CMoneyCombo::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	__super::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CMoneyCombo::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return __super::AddAssociation(lpszAssoc, DataMon(nValue));
}


//-----------------------------------------------------------------------------
LRESULT CMoneyCombo::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataMon aValue;
	return FormatValue(aValue);
}

//=============================================================================
//			Class CQuantityCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CQuantityCombo, CDoubleCombo)

BEGIN_MESSAGE_MAP(CQuantityCombo, CDoubleCombo)
	//{{AFX_MSG_MAP(CQuantityCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CQuantityCombo::CQuantityCombo()
	:
	CDoubleCombo()
{}

//-----------------------------------------------------------------------------
CQuantityCombo::CQuantityCombo(UINT nBtnIDBmp, DataQty* pData)
	:
	CDoubleCombo(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CQuantityCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleCombo::SetValue( (double) (DataQty&) aValue );

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CQuantityCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataQty* pData = (DataQty*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	return Format((double) *pData, pszStr);
}

//-----------------------------------------------------------------------------
double CQuantityCombo::GetValue()
{
	DataQty aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CQuantityCombo::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	__super::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CQuantityCombo::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return __super::AddAssociation(lpszAssoc, DataQty(nValue));
}

//-----------------------------------------------------------------------------
LRESULT CQuantityCombo::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataQty aValue;
	return FormatValue(aValue);
}

//=============================================================================
//			Class CPercCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPercCombo, CDoubleCombo)

BEGIN_MESSAGE_MAP(CPercCombo, CDoubleCombo)
	//{{AFX_MSG_MAP(CPercCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPercCombo::CPercCombo()
	:
	CDoubleCombo()
{
	m_dMin = -100.;
	m_dMax = 100.;
}

//-----------------------------------------------------------------------------
CPercCombo::CPercCombo(UINT nBtnIDBmp, DataPerc* pData)
	:
	CDoubleCombo(nBtnIDBmp, pData)
{
	m_dMin = -100.;
	m_dMax = 100.;
	
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CPercCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleCombo::SetValue( (double) (DataPerc&) aValue );

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CPercCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataPerc* pData = (DataPerc*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;
		
	return Format((double) *pData, pszStr);
}

//-----------------------------------------------------------------------------
double CPercCombo::GetValue()
{
	DataPerc aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CPercCombo::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	__super::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CPercCombo::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return __super::AddAssociation(lpszAssoc, DataPerc(nValue));
}

//-----------------------------------------------------------------------------
void CPercCombo::SetRange(double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;
		
	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}

//-----------------------------------------------------------------------------
LRESULT CPercCombo::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataPerc aValue;
	return FormatValue(aValue);
}


//=============================================================================
//			Class CDateCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CDateCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CDateCombo)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateCombo::CDateCombo()
	:
	CParsedCombo	(),
	m_lMin			(MIN_GIULIAN_DATE),
	m_lMax			(MAX_GIULIAN_DATE),
	m_nCurDate		(0)
{}

//-----------------------------------------------------------------------------
CDateCombo::CDateCombo(UINT nBtnIDBmp, DataDate* pData)
	:
	CParsedCombo	(nBtnIDBmp, pData),
	m_lMin			(MIN_GIULIAN_DATE),
	m_lMax			(MAX_GIULIAN_DATE),
	m_nCurDate		(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
void CDateCombo::SetRange(int nMin, int nMax)
{                 
	if (nMin < MIN_GIULIAN_DATE) nMin = MIN_GIULIAN_DATE;
	if (nMax > MAX_GIULIAN_DATE) nMax = MAX_GIULIAN_DATE;
	
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = (long)nMin;
	m_lMax = (long)nMax;
}

//-----------------------------------------------------------------------------
void CDateCombo::SetRange(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax)
{
	long nMin = ::GetGiulianDate(dMin, mMin, yMin);
	long nMax = ::GetGiulianDate(dMax, mMax, yMax);

	SetRange(nMin, nMax);
}

//-----------------------------------------------------------------------------
void CDateCombo::SetValue(long nValue)
{
	m_nCurDate = nValue;

	SetNewFormatIdx();

	// Correzione anomalia su visualizzazione FullDates
	// per evitare regressioni si verifica che il valore passato sia uguale a quello del DataDate associato
	// (Ge & Bru)
	if (GetCtrlData() && ((long) *((DataDate*)GetCtrlData())) == nValue)
	{
		CParsedCombo::SetValue(FormatDate(((DataDate*)GetCtrlData())->GetDateTime(), m_nFormatIdx, m_pFormatContext));
		return;
	}

 	WORD nDay, nMonth, nYear;          
    ::GetShortDate(nDay, nMonth, nYear, nValue);

	CParsedCombo::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
}
    
//-----------------------------------------------------------------------------
void CDateCombo::SetValue(WORD nDay, WORD nMonth, WORD nYear)
{
	m_nCurDate = ::GetGiulianDate(nDay, nMonth, nYear);
	SetNewFormatIdx();

	CParsedCombo::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
}

//-----------------------------------------------------------------------------
long CDateCombo::GetValue()
{                          
	DBTIMESTAMP aDateTime;
    CString strDate;
             
	CParsedCombo::GetValue(strDate);
	if (::GetTimeStamp(aDateTime, strDate, m_nFormatIdx, m_pFormatContext))
		return ::GetGiulianDate(aDateTime);

	return BAD_DATE;
}

//-----------------------------------------------------------------------------
BOOL CDateCombo::OnInitCtrl()
{
	m_nCtrlLimit = GetInputCharLen();

	VERIFY(CParsedCombo::OnInitCtrl());
		
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDateCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataDate &)aValue));

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CDateCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataDate* pData = (DataDate*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	SetNewFormatIdx();

	TB_TCSCPY(pszStr, ::FormatDate(((DataDate*)GetCtrlData())->GetDateTime(), m_nFormatIdx, m_pFormatContext));
	return _tcslen(pszStr);
}

//-----------------------------------------------------------------------------
int CDateCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
int CDateCombo::AddAssociation(LPCTSTR lpszAssoc, long nValue)
{
	return AddAssociation(lpszAssoc, DataDate(nValue));
}

//-----------------------------------------------------------------------------
BOOL CDateCombo::IsValid()
{
	if (!CParsedCombo::IsValid())
		return FALSE;
	
	if (GetChildEdit() == NULL)
		return TRUE;

	return IsValidDate(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CDateCombo::IsValidDate(long nValue)
{                                                                    
    // bad value 
    if (nValue == BAD_DATE)
    {
		m_nErrorID = DATE_EDIT_BAD_FORMAT;
		return FALSE;
    }

	// bad range
	if (nValue != 0L && (nValue < (long) m_lMin || nValue > (long) m_lMax))
	{
		m_nErrorID = DATE_EDIT_OUT_RANGE;
	
		return FALSE;
	}
    
    // if good date hold new value
	m_nCurDate = nValue;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString	CDateCombo::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR)
{
	CParsedCombo::SetValue((LPCTSTR)m_strBadVal);

	switch (nIDP)
	{                                 
		case DATE_EDIT_BAD_FORMAT :
			return cwsprintf(FormatMessage(nIDP), (LPCTSTR) m_strBadVal, (LPCTSTR) GetDateTimeTemplate());
				
		case DATE_EDIT_OUT_RANGE :
		{
		    WORD nDay, nMonth, nYear;          
		
		    ::GetShortDate(nDay, nMonth, nYear, m_lMin);
			CString dMin(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
			
		    ::GetShortDate(nDay, nMonth, nYear, m_lMax);
			CString dMax(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
			
			return cwsprintf(FormatMessage(nIDP), (LPCTSTR) m_strBadVal, (LPCTSTR) dMin, (LPCTSTR) dMax);
		}
	}

	return CParsedCtrl::FormatErrorMessage(nIDP, m_strBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDateCombo::DoSpinScroll(UINT nSBCode)
{                  
	long nDelta; 
	
	switch (nSBCode)
	{
		case SB_LINEDOWN	: nDelta = -1; break;
		case SB_PAGEDOWN	: nDelta = -30; break;
		case SB_LINEUP		: nDelta = +1; break;
		case SB_PAGEUP		: nDelta = +30; break;
		default				: return;
	}	

	//Get the number in the control.
	long nOld = GetValue() + nDelta;

	if ((nOld < (long) m_lMin && nDelta < 0) || (nOld > (long) m_lMax && nDelta > 0))
		BadInput();
	else
	{
		SetValue(nOld);
		SetModifyFlag(TRUE);

		// set the focus to this edit item and select it all
		SetCtrlFocus(TRUE);
		UpdateCtrlData(TRUE);
		return;
	}

	// set the focus to this edit item and select it all
	SetCtrlFocus(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDateCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	if (_istcntrl(nChar))
		return FALSE;

	CDateFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter == NULL)
		return FALSE;

	int		nStartChar	= 0;
	int		nPosChar	= 0;
	CString strBuffer;
	CString strSeparator;

	CParsedCombo::GetValue(strBuffer);
	m_wndEdit.GetSel(nStartChar, nPosChar);
	if (nStartChar != nPosChar)
	{
		strBuffer = strBuffer.Left(nStartChar) + strBuffer.Mid(nPosChar);
		nPosChar = nStartChar;
	}

	if (!_istdigit(nChar))
	{
		// si verifica se ci sono gia` separatori 
		//
		if	(
				nPosChar == 0 ||
				!_istdigit(strBuffer[nPosChar - 1]) ||
				DateSepPermitted(strBuffer, pFormatter, strSeparator) == NO_MORE_SEP
			)
		{
			BadInput();
			return TRUE;
		}
	}
	else
	{
		int nOffset = 2;

		// per l'anno si lasciano digitare sempre 4 cifre consecutive
		// prima di decidere che serve un separatore
		if (pFormatter->GetFormat() == CDateFormatHelper::DATE_YMD)
		{
			if (nPosChar <= 4)
				nOffset = 4;
		}
		else
		{
			if (nPosChar > pFormatter->GetInputDateLen() - 4)
				nOffset = 4;
		}

		if (nPosChar < nOffset)
			return FALSE;

		// potrei mettere un separatore (ogni nOffset cifre) ?
		for (int i = nPosChar - nOffset; i < nPosChar; i++)
			if (!_istdigit(strBuffer[i]))
				return FALSE;

		// il carattere prima delle nOffset cifre e` un separatore ?
		if (nPosChar > nOffset && _istdigit(strBuffer[nPosChar - nOffset - 1]))
			return FALSE;

		// servono ulteriori separtori ?
		int nSepIdx = DateSepPermitted(strBuffer, pFormatter, strSeparator);
		if (nSepIdx != FIRST_DATE_SEP && nSepIdx != SECOND_DATE_SEP && nSepIdx)
			return FALSE;

		strSeparator += (TCHAR) nChar;
	}

	CParsedCombo::SetValue(strBuffer.Left(nPosChar) + strSeparator + strBuffer.Mid(nPosChar));
	nPosChar += strSeparator.GetLength();
	SetCtrlSel(nPosChar, nPosChar);

	SetModifyFlag(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CDateCombo::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{	
	DataDate aData;
	if (m_nCurDate)
		aData.SetDate(m_nCurDate);
	else
		aData.Assign(GetValue());

	if (IsValid(aData))
	{
		BOOL bModify = GetModifyFlag();
		SetValue(aData);
		if (bModify)
			SetModifyFlag(TRUE);
		SetCtrlMaxLen(GetInputCharLen(), TRUE);
	}
	else
	{
		ClearCtrl();
		SetModifyFlag(FALSE);
	}
		
	/*if (m_nErrorID == 0)
	{
		if (m_nCurDate)
			SetValue(m_nCurDate);
		else
			SetValue(GetValue());
	}

	SetCtrlMaxLen(GetInputCharLen(), TRUE);*/
	
	return 0L;
}

//=============================================================================
//			Class CBoolCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CBoolCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CBoolCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CBoolCombo)
	ON_WM_CHAR		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBoolCombo::CBoolCombo()
	:
	CParsedCombo	()
{}
    
//-----------------------------------------------------------------------------
CBoolCombo::CBoolCombo(UINT nBtnIDBmp, DataBool* pData)
	:
	CParsedCombo	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
    
//-----------------------------------------------------------------------------
void CBoolCombo::SetValue(BOOL bValue)
{
	if (bValue)	
	{
		CParsedCombo::SetValue(DataObj::Strings::YES());
		return;
	}

	CParsedCombo::SetValue(DataObj::Strings::NO());
}

//-----------------------------------------------------------------------------
BOOL CBoolCombo::GetValue()
{
	DataBool	aValue;

	CParsedCombo::GetValue (aValue);
	return (BOOL) aValue;
}

//-----------------------------------------------------------------------------
void CBoolCombo::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (BOOL) (DataBool&) aValue );

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = FormatData(GetCtrlData());
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CBoolCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataBool* pData = (DataBool*) GetDataObjFromIdx(nIdx);
	
	if (pData == NULL)
		return -1;

	if ((BOOL) *pData)
		TB_TCSCPY(pszStr, DataObj::Strings::YES());
	else
	    TB_TCSCPY(pszStr, DataObj::Strings::NO());
	    
	return _tcslen(pszStr);
}

//-----------------------------------------------------------------------------
BOOL CBoolCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	CString	strYes	= DataObj::Strings::YES();
	CString	strNo	= DataObj::Strings::NO();
    
    if (_istcntrl(nChar))
		return FALSE;
    
	if	(nChar == BLANK_CHAR)
	{
		SetValue( !GetValue() );
		SetModifyFlag(TRUE);
		return TRUE;
	}
	
	if	(
			toupper((int) nChar) == toupper(strYes[0]) ||
			toupper((int) nChar) == toupper(strNo[0])
		)
	{
		SetValue(toupper((int) nChar) == toupper(strYes[0]));
		SetModifyFlag(TRUE);
		return TRUE;
	}

	BadInput();
	return TRUE;
}

//-----------------------------------------------------------------------------
int CBoolCombo::AddAssociation(LPCTSTR lpszAssoc, const DataObj& aDataObj, BOOL bCallFilter)
{
	return __super::AddAssociation(lpszAssoc, aDataObj, bCallFilter);
}

//-----------------------------------------------------------------------------
void CBoolCombo::OnFillListBox()
{
	AddAssociation(DataObj::Strings::YES(), DataBool(TRUE));
	AddAssociation(DataObj::Strings::NO(), DataBool(FALSE));
}

//=============================================================================
//			Class CIdentifierCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIdentifierCombo, CStrCombo)

BEGIN_MESSAGE_MAP(CIdentifierCombo, CStrCombo)
	//{{AFX_MSG_MAP(CIdentifierCombo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CIdentifierCombo::CIdentifierCombo()
	:
	CStrCombo		(),
	m_pItemToCheck	(NULL),
	m_pSymTable		(NULL)
{
	m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
CIdentifierCombo::CIdentifierCombo
					(
						SymTable* pSymTable, CObject* pItem,
						UINT nBtnIDBmp /*= NO_BUTTON */, DataStr* pData
					)
	:
	CStrCombo		(nBtnIDBmp, pData),
	m_pItemToCheck	(pItem),
	m_pSymTable		(pSymTable)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//

	if (m_pSymTable)
		m_dwCtrlStyle	|= IDE_STYLE_MUST_NO_EXIST;
	else
		m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
CIdentifierCombo::CIdentifierCombo
					(
						SymTable* pSymTable, DataStr* pData,
						CObject* pItem, UINT nBtnIDBmp /* = NO_BUTTON */
					)
	:
	CStrCombo		(nBtnIDBmp, pData),
	m_pItemToCheck	(pItem),
	m_pSymTable		(pSymTable)
{
	if (m_pSymTable)
		m_dwCtrlStyle	|= IDE_STYLE_MUST_NO_EXIST;
	else
		m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
void CIdentifierCombo::OnFillListBox()
{
	if (m_pSymTable)
	{
		for (int i = 0; i <= m_pSymTable->GetUpperBound(); i++)
		{
			SymField* pField = m_pSymTable->GetAt(i);
			AddAssociation(pField->GetDescription(), pField->GetName());
		}
	}
	
	CParsedCombo::OnFillListBox();
}

//-----------------------------------------------------------------------------
BOOL CIdentifierCombo::IsValid()
{   
	if (!CParsedCombo::IsValid())
		return FALSE;
	
	if (GetChildEdit() == NULL)
		return TRUE;

	if ((m_dwCtrlStyle & IDE_STYLE_NO_CHECK) == IDE_STYLE_NO_CHECK)
		return TRUE;
		
	DataStr aValue;
	CParsedCombo::GetValue(aValue);
		
	if ((m_dwCtrlStyle & IDE_STYLE_NO_EMPTY) == IDE_STYLE_NO_EMPTY && aValue.IsEmpty())
	{
		m_nErrorID = FIELD_EMPTY;
		return FALSE;
	}

	if (aValue.IsEmpty())
		return TRUE;
		
	if (m_pSymTable)
	{
		SymField* pItem = m_pSymTable->GetField(aValue.GetString());
		if (pItem)
			if (m_pItemToCheck == NULL)
			{
				if ((m_dwCtrlStyle & IDE_STYLE_MUST_NO_EXIST) == IDE_STYLE_MUST_NO_EXIST)
					m_nErrorID = FIELD_REDEFINED;
			}
			else
			{
				if (m_pItemToCheck != pItem)
					if ((m_dwCtrlStyle & IDE_STYLE_MUST_NO_EXIST) == IDE_STYLE_MUST_NO_EXIST)
						m_nErrorID = FIELD_REDEFINED;
					else
						if ((m_dwCtrlStyle & IDE_STYLE_MUST_EXIST) == IDE_STYLE_MUST_EXIST)
							m_nErrorID = FIELD_NOT_FOUND;
			}
		else
			if ((m_dwCtrlStyle & IDE_STYLE_MUST_EXIST) == IDE_STYLE_MUST_EXIST)
				m_nErrorID = FIELD_NOT_FOUND;
	}
	else
	{
		int nIdx;
		if (m_DataAssociations.GetSize())
			nIdx = GetIdxFromDataObj(aValue);
		else
			nIdx = FindStringExact(-1, aValue.GetString());
			
		if (nIdx >= 0)
		{
			if ((m_dwCtrlStyle & IDE_STYLE_MUST_NO_EXIST) == IDE_STYLE_MUST_NO_EXIST)
				m_nErrorID = FIELD_REDEFINED;
		}
		else
			if ((m_dwCtrlStyle & IDE_STYLE_MUST_EXIST) == IDE_STYLE_MUST_EXIST)
				m_nErrorID = FIELD_NOT_FOUND;
	}

	return m_nErrorID == 0;
}

//-----------------------------------------------------------------------------
BOOL CIdentifierCombo::DoOnChar(UINT nChar)
{
	if (CParsedCombo::DoOnChar(nChar))
		return TRUE;

	if	(   
			(_istcntrl(nChar) || _istalnum(nChar) || (nChar == _T('_'))) &&
			(LOWORD(GetEditSel()) != 0 || !_istdigit(nChar))
		)
		return CStrCombo::DoOnChar(nChar);

	BadInput();
	return TRUE;
}

//=============================================================================
//			Class CEnumCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CEnumCombo, CParsedCombo)

BEGIN_MESSAGE_MAP(CEnumCombo, CParsedCombo)
	//{{AFX_MSG_MAP(CEnumCombo)
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CEnumCombo::CEnumCombo()
	:
	CParsedCombo	()
{
	m_wTag = 0;
	m_bShowEnumValue = FALSE;
}
                        
//-----------------------------------------------------------------------------
CEnumCombo::CEnumCombo(UINT nBtnIDBmp, DataEnum* pData)
	:
	CParsedCombo(nBtnIDBmp)
{
	m_bShowEnumValue = FALSE;

	Attach(pData);
}
                        
//-----------------------------------------------------------------------------
void CEnumCombo::Attach(DataObj* pDataObj)
{
	if (pDataObj)
	{
		ASSERT(pDataObj->GetDataType().m_wType == GetDataType().m_wType);

		WORD wTag = ((DataEnum*)pDataObj)->GetTagValue();
	
		if (AfxGetEnumsTable()->GetEnumItems(wTag) == NULL)
		{
			TRACE1("ENUM %d Undefined for CEnumCombo\n", wTag);
			ASSERT(FALSE);
			
			return;
		}
	
		// local document enums are not initialized, 
		// so birthTag could be not initialized.
		WORD wBirthTag = ((DataEnum*) pDataObj)->GetBirthTagValue();
		SetTagValue(wBirthTag > 0 ? wBirthTag : wTag);
	}

	CParsedCtrl::Attach(pDataObj);
}


//-----------------------------------------------------------------------------
BOOL CEnumCombo::OnInitCtrl()
{
	VERIFY(CParsedCombo::OnInitCtrl());
	
	if ((GetStyle() & CBS_STYLES) != CBS_DROPDOWNLIST)
	{
		TRACE0("CEnumCombo MUST have CBS_DROPDOWNLIST style\n");
		ASSERT(FALSE);
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CEnumCombo::SetItemDescription(EnumItem* pItem)
{
	return pItem->GetTitle();
}

//-----------------------------------------------------------------------------
void CEnumCombo::OnFillListBox()
{
	const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(m_wTag);

	if (pItems == NULL)
	{
		TRACE1("ENUM %d Undefined for CEnumCombo\n", m_wTag);
		ASSERT(FALSE);
		
		return;
	}
	
	//ResetContent();
	m_DataAssociations.RemoveAll(); m_DescriptionsBuffer.RemoveAll();

	for (int i = 0; i <= pItems->GetUpperBound(); i++)
	{
		EnumItem* pItem = pItems->GetAt(i);
		if (pItem->IsHidden ())
			continue;

		AddAssociation(m_bShowEnumValue	? 
			cwsprintf(_T("{0-%s} --> {1-%05d} ({2-%08ld})"), pItem->GetTitle(), pItem->GetItemValue(), GET_TI_VALUE(pItem->GetItemValue(), m_wTag)) : pItem->GetTitle(), 
			DataEnum(m_wTag, pItem->GetItemValue()));
	}

}

//-----------------------------------------------------------------------------
void CEnumCombo::DoSetValue(const DataObj& aValue)
{             
	BOOL bGoodValue = CheckDataObjType(&aValue);
	ASSERT(bGoodValue);

	const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(m_wTag);

	if (pItems == NULL)
	{
		TRACE1("ENUM %d Undefined for CEnumCombo\n", m_wTag);
		ASSERT(FALSE);
		
		return;
	}

	EnumItem* pItem = pItems->GetItemByValue(((const DataEnum&) aValue).GetItemValue());

	CString strItem = pItem ? SetItemDescription(pItem) : pItems->GetTitle(((const DataEnum&)aValue).GetItemValue());

	if (bGoodValue && pItem && !pItem->IsHidden())		
		ResetAssociations(FALSE);

	AddAssociation(strItem, aValue, FALSE);

	SetValue(strItem);

	if (m_pReadOnlyEdit)
	{
		m_sReadOnlyText = strItem;
		m_pReadOnlyEdit->SetValue(m_sReadOnlyText);
	}
}

// Usata dalla OnGetText()
//-----------------------------------------------------------------------------
int CEnumCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	DataEnum* pData = (DataEnum*) GetDataObjFromIdx(nIdx);
	if (pData == NULL)
		return -1;

	const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(m_wTag);
	if (pItems == NULL)
	{
		TRACE1("ENUM %d Undefined for CEnumCombo\n", m_wTag);
		ASSERT(FALSE);
		return -1;
	}

	EnumItem* pItem = pItems->GetItemByValue(pData->GetItemValue());
	CString strItem = pItem ? SetItemDescription(pItem) : pItems->GetTitle(pData->GetItemValue());

	TB_TCSCPY(pszStr, strItem);
	return _tcslen(pszStr);
}

//-----------------------------------------------------------------------------
BOOL CEnumCombo::DoOnChar(UINT nChar)
{
	if (nChar == VK_DELETE)
		return FALSE;

	return CParsedCombo::DoOnChar(nChar);
}

//-----------------------------------------------------------------------------
BOOL CEnumCombo::DoKeyUp(UINT nChar)
{
	if (nChar == VK_DELETE)
		return FALSE;

	return CParsedCombo::DoKeyUp(nChar);
}

//-----------------------------------------------------------------------------
BOOL CEnumCombo::DoKeyDown(UINT nChar)
{
	if (nChar == VK_DELETE)
		return FALSE;

	return CParsedCombo::DoKeyDown(nChar);
}

//---------------------------------------------------------------------------
void CEnumCombo::OnRButtonDown(UINT nFlags, CPoint point) 
{
	OnContextMenu(this, point);
}

//-----------------------------------------------------------------------------
void CEnumCombo::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CParsedCombo::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
void CEnumCombo::DoShowEnumValue()
{
	if (m_bShowEnumValue)
		m_bShowEnumValue = FALSE;
	else
		m_bShowEnumValue = TRUE;
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CBCGPComboBoxEx
///////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP(CComboBoxExt, CBCGPComboBox)
	ON_CONTROL_REFLECT(CBN_DROPDOWN, OnDropdown)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CComboBoxExt::OnDropdown() 
{
    RecalcDropWidth();
}

//-----------------------------------------------------------------------------
void CComboBoxExt::RecalcDropWidth()
{
    // Reset the dropped width
    int nNumEntries = GetCount();
    int nWidth = 0;
    CString str;

    CClientDC dc(this);
    int nSave = dc.SaveDC();
    dc.SelectObject(GetFont());

    int nScrollWidth = ::GetSystemMetrics(SM_CXVSCROLL);
    for (int i = 0; i < nNumEntries; i++)
    {
        GetLBText(i, str);
        int nLength = dc.GetTextExtent(str).cx + nScrollWidth;
        nWidth = max(nWidth, nLength);
    }
    
    // Add margin space to the calculations
    nWidth += dc.GetTextExtent(_T("0")).cx;

    dc.RestoreDC(nSave);
    SetDroppedWidth(nWidth);
}
//-----------------------------------------------------------------------------

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CResizableComboBox
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CResizableComboBox, CComboBoxEx)

BEGIN_MESSAGE_MAP(CResizableComboBox, CComboBoxEx)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CResizableComboBox::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (! __super::SubclassDlgItem(IDC, pParent))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CResizableComboBox::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//=============================================================================
//			Class CXmlCombo implementation
//=============================================================================
//
IMPLEMENT_DYNCREATE (CXmlCombo, CStrCombo)

BEGIN_MESSAGE_MAP(CXmlCombo, CStrCombo)
	//{{AFX_MSG_MAP(CXmlCombo)
	ON_WM_CONTEXTMENU		()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXmlCombo::CXmlCombo()
	:
	CStrCombo				(),
	m_pDfi					(NULL),
	m_bUseProductLanguage	(FALSE),
	m_bEnableAddElements	(FALSE)
{
}

//-----------------------------------------------------------------------------
CXmlCombo::~CXmlCombo()
{
	DetachDataObjs();

	if (m_pDfi && m_pDfi->m_bAllowChanges)
		delete m_pDfi;
}

//-----------------------------------------------------------------------------
void CXmlCombo::SetNameSpace(LPCTSTR lpcszNamespace, BOOL bAllowChanges)
{
	m_pDfi = AfxGetTbCmdManager()->GetDataFileInfo(lpcszNamespace, bAllowChanges, m_bUseProductLanguage);
}

////-----------------------------------------------------------------------------
void CXmlCombo::EnableAddElements()
{
	 m_bEnableAddElements = TRUE; 
}


//-----------------------------------------------------------------------------
void CXmlCombo::AllowChanges()
{
	if (!m_pDfi || m_pDfi->m_bAllowChanges)
		return;

	m_pDfi = AfxGetTbCmdManager()->GetDataFileInfo(m_pDfi->m_Namespace.ToString(), TRUE);
}
//-----------------------------------------------------------------------------
void CXmlCombo::SetKey(const CString& strFieldName)
{
	// controllo l'esistenza del DataFileInfo
	if (!m_pDfi)
		return;
	m_pDfi->ChangeKey(strFieldName);
}

//-----------------------------------------------------------------------------
void CXmlCombo::SetHidden(const CString& strFieldName, BOOL bHidden /*= TRUE*/)
{
	// controllo l'esistenza del DataFileInfo
	if (!m_pDfi)
		return;

	m_pDfi->SetHidden(strFieldName, bHidden);
}

//-----------------------------------------------------------------------------
void CXmlCombo::OnFillListBox()
{
	if (!m_pDfi || m_pDfi->m_arElements.IsEmpty())
		return;
	
	m_DataAssociations.RemoveAll(); m_DescriptionsBuffer.RemoveAll();

	CString sPrefix;
	if (m_pDfi->m_bFilterLike) 
		sPrefix = CStrCombo::GetValue();

	long idxCount = 0;
	for (long i = 0; i <= m_pDfi->m_arElements.GetUpperBound(); i++)
	{
		if (idxCount > GetMaxItemsNo())
		{
			AddAssociation(cwsprintf(FormatMessage(MAX_ITEM_REACHED), GetMaxItemsNo()), _T(""));
			break;
		}

		CString sVal = m_pDfi->GetValue(i);

		if (!sPrefix.IsEmpty())
		{
			//TRACE(m_pDfi->GetValue(i));TRACE(_T("\n"));
			
			if (::FindNoCase(sVal, sPrefix) != 0)
				continue;;
		}

		AddAssociation(m_pDfi->GetDescription(i), sVal);
		idxCount++;
	}
}

//-----------------------------------------------------------------------------
void CXmlCombo::Attach(DataObj* pDataObj, CString pName)
{
	// file existance check
	if (!m_pDfi)
		return;

	// if attached data type exists and it matches the
	// requested data, I add it to the DataObjs list
	if (m_pDfi->GetFieldType(pName) && 
		m_pDfi->GetFieldType(pName)->m_Type == pDataObj->GetDataType())
	{
		pAttachedDataObj.Add(pDataObj);
		pAttachedDataObjNames.Add(pName);
		return;
	}
	TRACE(_T("CXmlCombo::Attach : the attached data type is missing or not corrisponding to the requested data type.\n"));
	return;
}

//-----------------------------------------------------------------------------
void CXmlCombo::DetachDataObjs ()
{
	// assegno a NULL tutti i DataObj Attachati
	for (int i = 0; i <= pAttachedDataObj.GetUpperBound(); i++)
	{
		pAttachedDataObj[i] = NULL;
	}

	pAttachedDataObj.RemoveAll();
	pAttachedDataObjNames.RemoveAll();
	pAttachedDataObj.RemoveAll();
	pAttachedDataObjNames.RemoveAll();
}

//-----------------------------------------------------------------------------
CString CXmlCombo::GetValue()
{
	CString aValue = CStrCombo::GetValue();

	// bug #13.390 and #15.556 (I cannot use m_bModifiedFlag)
	CString aOldValue (GetCtrlData() ? GetCtrlData()->Str() : _T(""));
	if (
			m_pDfi && 
			_tcsicmp((LPCTSTR) aOldValue, (LPCTSTR) aValue) != 0 && 
			(!m_pDocument || m_pDocument->GetFormMode() != CBaseDocument::FIND)
		)
	{
		// I have to updated Attached dataobjs
		for (int i = 0; i <= pAttachedDataObj.GetUpperBound(); i++)
		{
			DataObj* pValue = m_pDfi->GetElement(aValue, pAttachedDataObjNames[i]);
			if (pValue)
				pAttachedDataObj[i]->Assign(*pValue);
		}
	}

	return aValue;
}

//-----------------------------------------------------------------------------
DataObj* CXmlCombo::GetAttachedDataByName (const CString& sNomeCampo)
{
	for (int i = 0; i <= pAttachedDataObjNames.GetUpperBound(); i++)
	{
		if (sNomeCampo.CompareNoCase(pAttachedDataObjNames[i]) == 0 && pAttachedDataObj[i] != NULL)
			return pAttachedDataObj[i];
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CXmlCombo::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	CStrCombo::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
BOOL CXmlCombo::OnShowingPopupMenu(CMenu& menu)
{
	if (!m_pDfi || !m_bEnableAddElements) //!m_pDfi->m_bEnableAddElements)
		return TRUE;

	CParsedCombo::OnShowingPopupMenu(menu);
	
	CString aValue = CStrCombo::GetValue();
	//do la possibilità di inserire o cancellare un elemento inserito in precedenza solo
	//per gli xml che contengono un solo field type (che è quello visualizzato) 
	if (m_pDfi->m_arElementTypes.GetCount() > 1 || aValue.IsEmpty())
		return TRUE;

	if (menu.GetMenuItemCount() > 0)
		menu.AppendMenu(MF_SEPARATOR);

	CDataFileElement* pElement = m_pDfi->GetDataFileElement(aValue);
	if (!pElement)
		menu.AppendMenu(MF_STRING, ID_MENU_ADD_ELEMENT, _TB("Add element"));
	else
		if (pElement->m_bFromCustom)
			menu.AppendMenu(MF_STRING, ID_MENU_REMOVE_ELEMENT, _TB("Remove element"));

	//devo inserire anche la parte dell'edit
	if (menu.GetMenuItemCount() > 0)
	{
		if (GetChildEdit() != NULL)
			m_wndEdit.SetSel(0,-1);

		menu.AppendMenu(MF_SEPARATOR);
		menu.AppendMenu(MF_STRING, WM_UNDO,	_TB("Undo"));
		menu.AppendMenu(MF_SEPARATOR);
		menu.AppendMenu(MF_STRING, WM_CUT,	 _TB("Cut"));
		menu.AppendMenu(MF_STRING, WM_COPY,	 _TB("Copy"));
		menu.AppendMenu(MF_STRING, WM_PASTE, _TB("Paste"));
		menu.AppendMenu(MF_STRING, WM_CLEAR, _TB("Delete"));
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlCombo::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);
	CString aValue = CStrCombo::GetValue();
	if (nCode == 0)
	{
		if (nID == ID_MENU_ADD_ELEMENT)
		{
			m_pDfi->AddElement(aValue);
			AfxGetTbCmdManager()->SaveDataFileInfo(m_pDfi);
		}
		else if (nID == ID_MENU_REMOVE_ELEMENT)
		{
			int nSel = GetCurSel();
			m_pData->Clear();
			Clear();
			m_pDfi->RemoveElement(aValue);
			m_DataAssociations.RemoveAt(nSel);	m_DescriptionsBuffer.RemoveAt(nSel);
			AfxGetTbCmdManager()->SaveDataFileInfo(m_pDfi);
		}
		else 
			if (GetChildEdit() != NULL)
			{
				if (nID == WM_UNDO)
					m_wndEdit.Undo();
				else if (nID == WM_CUT)
					m_wndEdit.Cut();
				else if (nID == WM_COPY)
					m_wndEdit.Copy();
				else if (nID == WM_PASTE)
					m_wndEdit.Paste();
				else if (nID == WM_CLEAR)
					m_wndEdit.Clear();
			}
	}

	return CStrCombo::OnCommand(wParam, lParam);

}
      
 
//=============================================================================
//			Class CIntMonthNameCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntMonthNameCombo, CIntCombo)
//-----------------------------------------------------------------------------
CIntMonthNameCombo::CIntMonthNameCombo()
	:
	CIntCombo	()
{
	m_nMin = 1;
	m_nMax = 12;
}

//-----------------------------------------------------------------------------
BOOL CIntMonthNameCombo::OnInitCtrl()
{
	VERIFY(CParsedCombo::OnInitCtrl());
	
	if ((GetStyle() & CBS_STYLES) != CBS_DROPDOWNLIST)
	{
		TRACE0("CIntMonthNameCombo MUST have CBS_DROPDOWNLIST style\n");
		ASSERT(FALSE);
	}

	int nIdx = AfxGetFormatStyleTable()->GetFormatIdx(_NS_FMT("MonthName")) ;
	if (nIdx >= 0 )	AttachFormatter(nIdx);	

	FillListBox();
	
	return TRUE;
}
//-----------------------------------------------------------------------------
void CIntMonthNameCombo::SetRange(int nMin, int nMax)
{
	if (nMin > nMax)
	{
		TRACE0("MinValue for CIntMontNameCombo greather than MaxValue");
		ASSERT(FALSE);
	}

	if (nMin > 12 || nMin < 1)
	{
		TRACE0("MinValue for CIntMontNameCombo must be between 1 and 12");
		ASSERT(FALSE);
	}

	if (nMax > 12 || nMax < 1)
	{
		TRACE0("MaxValue for CIntMontNameCombo must be between 1 and 12");
		ASSERT(FALSE);
	}

	m_nMin = nMin;
	m_nMax = nMax;
}
//-----------------------------------------------------------------------------
void CIntMonthNameCombo::OnFillListBox()
{
	//CIntCombo::OnFillListBox();

	for (int t=m_nMin; t<=m_nMax; t++)
		__super::AddAssociation(GetTextValue(t), t); 
}
//-----------------------------------------------------------------------------
CString CIntMonthNameCombo::GetTextValue(int index)
{
	return MonthName(index);
}

//=============================================================================
//			Class CIntWeekDaysCombo implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntWeekDaysCombo, CIntCombo)
//-----------------------------------------------------------------------------
CIntWeekDaysCombo::CIntWeekDaysCombo()
	:
	CIntCombo	()
{
	m_nMin = 0;
	m_nMax = 6;
}

//-----------------------------------------------------------------------------
BOOL CIntWeekDaysCombo::OnInitCtrl()
{
	VERIFY(CParsedCombo::OnInitCtrl());
	
	if ((GetStyle() & CBS_STYLES) != CBS_DROPDOWNLIST)
	{
		TRACE0("CIntWeekDaysCombo MUST have CBS_DROPDOWNLIST style\n");
		ASSERT(FALSE);
	}

	int nIdx = AfxGetFormatStyleTable()->GetFormatIdx(_NS_FMT("WeekDayName")) ;
	if (nIdx >= 0 )	AttachFormatter(nIdx);	

	FillListBox();
	
	return TRUE;
}
//-----------------------------------------------------------------------------
void CIntWeekDaysCombo::SetRange(int nMin, int nMax)
{
	if (nMin > nMax)
	{
		TRACE0("MinValue for CIntWeekDaysCombo greather than MaxValue");
		ASSERT(FALSE);
	}

	if (nMin > 6 || nMin < 0)
	{
		TRACE0("MinValue for CIntWeekDaysCombo must be between 0 and 6");
		ASSERT(FALSE);
	}

	if (nMax > 6 || nMax < 0)
	{
		TRACE0("MaxValue for CIntWeekDaysCombo must be between 0 and 6");
		ASSERT(FALSE);
	}

	m_nMin = nMin;
	m_nMax = nMax;
}

//-----------------------------------------------------------------------------
void CIntWeekDaysCombo::OnFillListBox()
{
	for (int t=m_nMin; t<=m_nMax; t++)
		__super::AddAssociation(GetTextValue(t), t); 
}
//-----------------------------------------------------------------------------
CString CIntWeekDaysCombo::GetTextValue(int index)
{
	CString strTmp;
                           
	switch 	(index)
	{
		case 0 : strTmp = _TB("Monday");	break;
		case 1 : strTmp = _TB("Tuesday");	break;
		case 2 : strTmp = _TB("Wednesday");	break;
		case 3 : strTmp = _TB("Thursday");	break;
		case 4 : strTmp = _TB("Friday");	break;
		case 5 : strTmp = _TB("Saturday");	break;
		case 6 : strTmp = _TB("Sunday");	break;
	}              
	
	return strTmp;
}



//=============================================================================
//			Class CMSStrCombo implementation
//=============================================================================

IMPLEMENT_DYNCREATE (CMSStrCombo, CDescriptionCombo)

BEGIN_MESSAGE_MAP(CMSStrCombo, CDescriptionCombo)
	ON_CONTROL_REFLECT_EX (CBN_DROPDOWN, OnDropDown)
	ON_MESSAGE            (UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
	ON_COMMAND			  (ID_SELECT_ALL       , SelectAll)	
	ON_COMMAND			  (ID_UNSELECT_ALL     , UnSelectAll)	
	ON_COMMAND			  (ID_INVERT_SELECTION , InvertSelected)
	ON_MESSAGE			  (UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_WM_LBUTTONDOWN	  ()
	ON_WM_LBUTTONDBLCLK	  ()
	ON_WM_PAINT			  ()
	ON_WM_COMPAREITEM	  ()
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
//
//  CMSStrComboProc  (Global window procedure)
//

static bool bMenu = false;

//-----------------------------------------------------------------------------
extern "C" LRESULT FAR PASCAL CMSStrComboProc(HWND hWnd, UINT nMsg, WPARAM wParam, LPARAM lParam)
{
	// special message which identifies the window as using AfxWndProc
	if (nMsg == WM_QUERYAFXWNDPROC)
		return 1;

	// all other messages route through message map
	CWnd* pWnd = CWnd::FromHandlePermanent(hWnd);
	CBCGPDropDownListBox* pLBChild = dynamic_cast<CBCGPDropDownListBox*>(pWnd);
	CMSStrCombo* pCMSStrCombo = NULL;

	if (pLBChild)
		pCMSStrCombo = dynamic_cast<CMSStrCombo*>(pLBChild->m_pWndCombo);

	switch (nMsg)
    {
		case LB_GETCURSEL: {
			return -1;
		}

		case WM_WINDOWPOSCHANGING: 
		{
			return 0;
		}

		case WM_CHAR:
        {
            if (wParam == VK_SPACE && (lParam & 1 << 30) == 0)
            {
                // Get the current selection
				int nIndex = CallWindowProc(pCMSStrCombo->m_pOldMSStrComboWndProc, hWnd, LB_GETCURSEL, wParam, lParam);

				if (nIndex < 0) 
					break;

				CRect rcItem;
                ::SendMessage(hWnd, LB_GETITEMRECT, nIndex, (LONG)(VOID *)&rcItem);
                InvalidateRect(hWnd, rcItem, FALSE);

                // Invert the check mark
                pCMSStrCombo->SetCheck(nIndex, !pCMSStrCombo->GetCheck(nIndex));

                // Notify that check state has changed
                pCMSStrCombo->GetParent()->SendMessage(WM_COMMAND, MAKELONG(GetWindowLong(pCMSStrCombo->m_hWnd, GWL_ID), BN_CLICKED), (LPARAM)pCMSStrCombo->m_hWnd);
                return 0;
            }
            break;
        }
		
		case WM_LBUTTONDBLCLK:
        case WM_LBUTTONDOWN:
        {
			CRect rc;
			pWnd->GetClientRect(rc);

            CPoint pt;
            pt.x = LOWORD(lParam);
            pt.y = HIWORD(lParam);


			if (PtInRect(rc, pt))
            {
                int nItemHeight = ::SendMessage(hWnd, LB_GETITEMHEIGHT, 0, 0);
                int nTopIndex   = ::SendMessage(hWnd, LB_GETTOPINDEX, 0, 0);

                // Compute which index to check/uncheck
                int nIndex = nTopIndex + pt.y / nItemHeight;

                ::SendMessage(hWnd, LB_GETITEMRECT, nIndex, (LONG)(VOID *)&rc);

                if (PtInRect(rc, pt))
                {
                    // Invalidate this window
                    InvalidateRect(hWnd, rc, FALSE);

                    // Notify that selection has changed
					pCMSStrCombo->GetParent()->SendMessage(WM_COMMAND, MAKELONG(GetWindowLong(pCMSStrCombo->m_hWnd, GWL_ID), CBN_SELCHANGE), (LPARAM)pCMSStrCombo->m_hWnd);

                    // Was the click on the check box?
                    CDC* hdc = pWnd->GetDC();
                    TEXTMETRIC metrics;
                    hdc->GetTextMetrics(&metrics);
                    pWnd->ReleaseDC(hdc);
                    rc.right = rc.left + metrics.tmHeight + metrics.tmExternalLeading + 6;

					// remove the comment for sensible only check Box
                    // Toggle the check state
                    pCMSStrCombo->SetCheck(nIndex, !pCMSStrCombo->GetCheck(nIndex));

                    // Notify that check state has changed
                    pCMSStrCombo->GetParent()->SendMessage(WM_COMMAND, MAKELONG(GetWindowLong(pCMSStrCombo->m_hWnd, GWL_ID), BN_CLICKED), (LPARAM)pCMSStrCombo->m_hWnd);
                }
            }

			// Do the default handling now
            break;
		}

		case WM_RBUTTONDOWN:
		{
            CPoint pt;
			GetCursorPos(&pt);

			CMenu   menu;	
			menu.CreatePopupMenu();
			pCMSStrCombo->OnShowingPopupMenu(menu);
			bMenu = true;
			menu.TrackPopupMenu (TPM_LEFTALIGN |TPM_RIGHTBUTTON | TPM_NONOTIFY , pt.x, pt.y, pCMSStrCombo);
			bMenu = false;
			return 0;
		}

		case WM_RBUTTONUP:
		case WM_LBUTTONUP:
		{
			return 0;
		}
	}

	if (bMenu) return 0;

	if (pCMSStrCombo && pCMSStrCombo->m_pOldMSStrComboWndProc)
	{
		WNDPROC lpfnproc = pCMSStrCombo->m_pOldMSStrComboWndProc;

		if (nMsg == WM_NCDESTROY)
		{
			SetWindowLongPtr(hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(pCMSStrCombo->m_pOldMSStrComboWndProc));
			pCMSStrCombo->m_pOldMSStrComboWndProc = NULL;
		}

		return CallWindowProc(lpfnproc, hWnd, nMsg, wParam, lParam);
	}

	return 0;
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::SubclassChildListBox()
{
	// now hook into the AFX WndProc
	if (!__super::SubclassChildListBox())
		return FALSE;

	m_pOldMSStrComboWndProc = reinterpret_cast<WNDPROC>(::SetWindowLongPtr(m_wndList.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&CMSStrComboProc)));
	return (m_pOldMSStrComboWndProc && m_pOldMSStrComboWndProc != &CMSStrComboProc);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::UnSubclassChildListBox()
{
	ASSERT(::IsWindow(m_wndList.m_hWnd));
	ASSERT(::IsWindow(m_hWnd));

	// set WNDPROC back to original value
	SetWindowLongPtr(m_wndList.m_hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(m_pOldMSStrComboWndProc));
	m_pOldMSStrComboWndProc = NULL;

	__super::UnSubclassChildListBox();
}

//-----------------------------------------------------------------------------
CMSStrCombo::CMSStrCombo()
{
	m_pOldMSStrComboWndProc = NULL;
	m_bItemHeightSet = FALSE;
	m_bTextUpdated   = FALSE;

	m_Separator		 = _T(";");
	m_DataTypeKey	 = DataType::String;	//---- per Woorm
	m_bDecodeListKey = FALSE;				//---- per Woorm
	m_RectBitmap.SetRectEmpty();
}

//-----------------------------------------------------------------------------
CMSStrCombo::CMSStrCombo(UINT nBtnIDBmp, DataStr* pData)
	:
	CDescriptionCombo (nBtnIDBmp, pData)
{
	CMSStrCombo();
}

//-----------------------------------------------------------------------------
// check the style
BOOL CMSStrCombo::OnInitCtrl()
{
	BOOL bOnInitRet = __super::OnInitCtrl();

	//ASSERT_TRACE(((GetStyle() & CBS_SORT) == 0), _T("\nCMSStrCombo was called on control with CBS_SORT style\n"));
	ASSERT_TRACE(((GetStyle() & CBS_DROPDOWNLIST) == CBS_DROPDOWNLIST), _T("\nCMSStrCombo was called on control without CBS_DROPDOWNLIST style\n"));
	ASSERT_TRACE(((GetStyle() & CBS_OWNERDRAWFIXED) == CBS_OWNERDRAWFIXED), _T("\nCMSStrCombo was called on control without CBS_OWNERDRAWFIXED style\n"));
	return bOnInitRet;
}

//-----------------------------------------------------------------------------
int CMSStrCombo::OnCompareItem(int /*nIDCtl*/, LPCOMPAREITEMSTRUCT lpCompareItemStruct)
{
	return CompareItem(lpCompareItemStruct);
}

//-----------------------------------------------------------------------------
int CMSStrCombo::CompareItem(LPCOMPAREITEMSTRUCT lpCompareItemStruct)
{
	if	(
			GetSizeDescriptionsBuffer() == 0 ||
			(int)lpCompareItemStruct->itemID1 < 0 ||
			(int)lpCompareItemStruct->itemID1 >= GetSizeDescriptionsBuffer() ||
			(int)lpCompareItemStruct->itemID2 < 0 ||
			(int)lpCompareItemStruct->itemID2 >= GetSizeDescriptionsBuffer()
		)
		return 0;

	ASSERT(lpCompareItemStruct->CtlType == ODT_COMBOBOX || lpCompareItemStruct->CtlType == ODT_LISTBOX);

	int iComp = 0;
	CString strText1 = GetDescriptionsBuffer((int)lpCompareItemStruct->itemID1);
	CString strText2 = GetDescriptionsBuffer((int)lpCompareItemStruct->itemID2);

	iComp = strText1.Compare(strText2);

	return iComp;
}

//-----------------------------------------------------------------------------
void CMSStrCombo::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	HDC dc = lpDrawItemStruct->hDC;
	m_RectBitmap = lpDrawItemStruct->rcItem;
    CRect rcText   = lpDrawItemStruct->rcItem;
	CString strText;

	int nItems =  GetSizeDescriptionsBuffer();

	// 0 - No check, 1 - Empty check, 2 - Checked
	int nCheck = 0;

	// Check if we are drawing the static portion of the combobox
	if ((LONG)lpDrawItemStruct->itemID < 0 || m_DescriptionsBuffer.GetCount() == 0)
	{
		RecalcText();

		// Get the text
		strText = GetCtrlData()->Str();
		m_sReadOnlyText = strText;
		// Don't draw any boxes on this item
		nCheck = 0;
	}
	else
	{	// Otherwise it is one of the items
		strText =  GetDescriptionsBuffer((int) lpDrawItemStruct->itemID);

		nCheck = 1 + m_ItemsCheckState.GetAt(lpDrawItemStruct->itemID); /*(GetItemData(lpDrawItemStruct->itemID) != 0);*/

		TEXTMETRIC metrics;
		GetTextMetrics(dc, &metrics);

		m_RectBitmap.left = 0;
		m_RectBitmap.right = m_RectBitmap.left + metrics.tmHeight + metrics.tmExternalLeading + 6;
		m_RectBitmap.top += 1;
		m_RectBitmap.bottom -= 1;

		rcText.left = m_RectBitmap.right;
	}
	
	if (nCheck > 0) 
	{
		UINT nState = DFCS_BUTTONCHECK | DFCS_FLAT;

		if (nCheck > 1)
			nState |= DFCS_CHECKED;

		// Draw the checkmark using DrawFrameControl
		DrawFrameControl(dc, m_RectBitmap, DFC_BUTTON, nState);
	}
	else
	{
		CDC * pDC = CDC::FromHandle(dc);
		if (IsWindowEnabled())
		{
			pDC->SetBkColor(HasFocus() ? AfxGetThemeManager()->GetFocusedControlBkgColor() : AfxGetThemeManager()->GetEnabledControlBkgColor());
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
		}
		else
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
		}
	}

	// Erase and draw
	ExtTextOut(dc, 0, 0, ETO_OPAQUE, &rcText, 0, 0, 0);
	DrawText(dc, ' ' + strText, strText.GetLength() + 1, &rcText, DT_SINGLELINE|DT_VCENTER|DT_END_ELLIPSIS);
	if ((nCheck > 0) && (lpDrawItemStruct->itemState & (ODS_FOCUS | ODS_SELECTED)) == (ODS_FOCUS | ODS_SELECTED))
        DrawFocusRect(dc, &rcText);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::CalculateSize()
{
	// Calculate the maximum line visible in ComboBox 
	CFrameWnd* pFrame = GetParentFrame();
	if (pFrame)
	{
		// Dropped lenght calcolate

		// Get Compo position
		CRect cRectCombo;
		GetWindowRect(cRectCombo);
		// Get the Frame windows size
		CRect cRectWindows;
		pFrame->GetWindowRect(cRectWindows);
		int nItems = 5;
		int height = m_DataAssociations.GetSize() > 0 ? GetItemHeight(0) : GetItemHeight(-1);
		int h1 = cRectWindows.bottom - cRectCombo.bottom;
		if (h1 > 0)
		{
			nItems = (int)(h1 / height);
			if (nItems > 0)
			{
				nItems = nItems - (int)(nItems * 0.20);
				if (nItems <= 0) nItems = 5;
			}
		}
		SetMinVisibleItems(nItems);
	}

	// Dropped Width calcolate
	int nCount = GetSizeDescriptionsBuffer();
	int nWidth = 0;
	CClientDC dc(this);
	int nSave = dc.SaveDC();
	CFont* pFont = GetFont();
	if (pFont)
	{
		dc.SelectObject(pFont);
		int nScrollWidth = ::GetSystemMetrics(SM_CXVSCROLL);
		// score the list items
		for (int i = 0; i < nCount; i++)
		{
			CString strItem = GetDescriptionsBuffer(i);
			int nLength = dc.GetTextExtent(strItem).cx + nScrollWidth;
			nWidth = max(nWidth, nLength);
		}

		// Add margin space to the calculations
		nWidth += dc.GetTextExtent(_T("X")).cx + m_RectBitmap.Width();
		dc.RestoreDC(nSave);
		SetDroppedWidth(nWidth);
	}
	else { ASSERT(FALSE); }
	// End calcoli
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::OnDropDown()
{
	CalculateSize();
	RefreshAllCheck(GetCtrlData()->Str());
	return FALSE; // Parent window will also receive this notification
}

//-----------------------------------------------------------------------------
// string containing the checked items
void CMSStrCombo::RecalcText()
{
	if (!m_bTextUpdated) {
		CString strText = _T("");
		// Get the list count
		for (int i = 0; i < GetSizeDescriptionsBuffer(); i++) {
			if (m_ItemsCheckState.GetAt(i)) {
				// get string for compone the combo
				CString strItem = m_DataAssociations.GetAt(i)->Str();
				if (m_bShowHKLDescription)
					strItem = GetDescriptionsBuffer(i);
				// add separator
				if (!strText.IsEmpty())
					strText += m_Separator;
				OnRecalcText(strItem);
					// append string
				strText += strItem;
			}
		}
		// Set the text
		GetCtrlData()->Assign(strText);
		m_bTextUpdated = TRUE;
	}
}

//-----------------------------------------------------------------------------
void CMSStrCombo::OnRecalcText(CString& strText)
{
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);
	if (nID == (UINT)GetCtrlID() && hWndCtrl != NULL)
	{
		// control notification
		ASSERT(::IsWindow(hWndCtrl));

		switch (nCode)
		{          
			case CBN_CLOSEUP:
			{
				NotifyToParent(EN_VALUE_CHANGED);
				break;
			}

			case  CBN_SELCHANGE :
			{
				if ((m_wComboStatus & HIT_SELENDOK) == HIT_SELENDOK)
				{
					int idx = GetCurSel();
					if (idx < 0) return true;
					SetCheck(idx, !m_ItemsCheckState.GetAt(idx));
				}
				return TRUE;
			}
		}
	}

	return __super::OnCommand(wParam, lParam);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::GetArrayValue(DataObjArray& values) const 
{
	ASSERT_VALID(this);
	ASSERT(m_DataAssociations.GetSize() == m_ItemsCheckState.GetSize());
	
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (m_ItemsCheckState.GetAt(i) != FALSE)
		{
			DataObj* pObj = GetDataObjFromIdx(i);
			ASSERT_VALID(pObj);
			if (pObj)
				values.Add(pObj->DataObjClone());
		}
	}
}

//-----------------------------------------------------------------------------
CString CMSStrCombo::GetSqlNativeValues	() const
{
	ASSERT_VALID(this);
	ASSERT(m_DataAssociations.GetSize() == m_ItemsCheckState.GetSize());
	
	CString s;
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (m_ItemsCheckState.GetAt(i) != FALSE)
		{
			DataObj* pObj = GetDataObjFromIdx(i);
			ASSERT_VALID(pObj);
			if (pObj)
			{
				if (!s.IsEmpty())
					s += ',';
				s += AfxGetTbCmdManager()->NativeConvert(pObj);
			}
		}
	}
	return (s.IsEmpty() ? s : '(' + s + ')' );
}

//-----------------------------------------------------------------------------
void CMSStrCombo::LoadUnattending()
{
	int nResult = m_pHotKeyLink->DoSearchComboQueryData(GetMaxItemsNo(),  this->m_DataAssociations, this->m_DescriptionsBuffer);
	for (int k = 0; k < this->m_DescriptionsBuffer.GetSize(); k++)
	{
		m_nMaxItemsLen = max(m_nMaxItemsLen, this->m_DescriptionsBuffer[k].GetLength());
		CBCGPComboBox::AddString(this->m_DescriptionsBuffer[k]);

		if (k > m_ItemsCheckState.GetSize()-1)
		{
			m_ItemsCheckState.Add((BYTE) 0);
		}
	}
}

//-----------------------------------------------------------------------------
void CMSStrCombo::SetArrayValue(const DataObjArray& values)
{
	if (!m_hWnd)
		return;

	if (GetCount() <= 0)
	{	
		LoadUnattending();
	}

	if (GetCount() <= 0)
		return;

	for (int i = 0; i < GetCount(); i++)
		SetCheck(i, FALSE);

	for (int i = 0; i < values.GetSize(); i++)
	{
		int nIdx =  this->m_DataAssociations.Find(values.GetAt(i));
		if (nIdx >= 0)
			SetCheck(nIdx, TRUE);
	}

	// Signal that the text need updating
	m_bTextUpdated = FALSE;

	// Redraw the window
	Invalidate(FALSE);

	RecalcText();
}

//-----------------------------------------------------------------------------
CString CMSStrCombo::GetValue()
{
	return GetCtrlData()->Str();
}

//-----------------------------------------------------------------------------
void CMSStrCombo::GetValue(DataObj& aValue)
{
	DoMaskedGetValue(GetCtrlData()->Str(), aValue);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::SetValue(const DataObj& /*aValue*/)
{
	CString strText = GetCtrlData()->Str();	
	
	// TODO: problem in BodyEdit the [GetCtrlData()->Str()] return "" 
	if (strText != GetCtrlData()->Str())
		GetCtrlData()->Assign(strText);
	//----
	
	if (m_bDecodeListKey)	//---- per Woorm
	{
		DataObjArray arObj;
		strText.Replace(_T("'"), _T(""));
		arObj.Assign(strText, m_DataTypeKey);
		if (arObj.GetCount())
		{
			SetArrayValue(arObj);
		}
	}
}

//-----------------------------------------------------------------------------
void CMSStrCombo::SetValue(LPCTSTR pszValue)
{	
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
int CMSStrCombo::GetAssociatedText(int nIdx, LPTSTR pszStr)
{
	return -1;
}

//-----------------------------------------------------------------------------
int CMSStrCombo::GetAssociatedTextLen(int nIdx)
{
	return -1;
}

//-----------------------------------------------------------------------------
void CMSStrCombo::SetComboValue(CString pszValue)
{	
	GetCtrlData()->Assign(pszValue);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::DoSetValue(const DataObj& aValue)
{
	// not call the father this append the list in comboBox
	ASSERT(CheckDataObjType(&aValue));
	
	m_sReadOnlyText = FormatData(GetCtrlData());

	Invalidate();
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::DoSetCurSel (const DataObj&)
{ 
	return FALSE; 
}

//-----------------------------------------------------------------------------
void CMSStrCombo::RefreshAllCheck(CString comboStr)
{
	if (m_hWnd)
	{
		// Get the list count
		int nCount  =  GetSizeDescriptionsBuffer();
		BOOL bSep = (comboStr.Find(m_Separator) > 0);
		// score the list items
		for (int i = 0; i < nCount; i++)
		{
			CString strItem = m_DataAssociations.GetAt(i)->Str();
			if (m_bShowHKLDescription)
				strItem = GetDescriptionsBuffer(i);

			BOOL vCheck = FALSE;
			if (bSep)
			{
				INT j = 0;
				for (CString sItemComboStr = comboStr.Tokenize(m_Separator, j); j >= 0; sItemComboStr = comboStr.Tokenize(m_Separator, j))
				{
					vCheck = (strItem.Compare(sItemComboStr) == 0);
					if (vCheck) break;
				}
			}
			else
			{
				if (!comboStr.Trim().IsEmpty())
					vCheck = (strItem.Compare(comboStr) == 0);
			}

			if (i >= m_ItemsCheckState.GetSize())
			{	
				m_ItemsCheckState.Add((BYTE) vCheck);
			}
			else
			{
				m_ItemsCheckState.SetAt(i, (BYTE) vCheck);
			}
		}

		// Signal that the text need updating
		m_bTextUpdated = FALSE;

		// Redraw the window
		Invalidate(FALSE);
	}
}

//-----------------------------------------------------------------------------
int CMSStrCombo::SetCheck(CString strText, BOOL bFlag)
{
	if (!m_hWnd || strText.IsEmpty()) return -1;
	// Get the list count
	int nCount = GetSizeDescriptionsBuffer();

	for (int i = 0; i < nCount; i++)
	{
		CString strItem = m_DataAssociations.GetAt(i)->Str();
		if (m_bShowHKLDescription)
			strItem = GetDescriptionsBuffer(i);

		if (strItem.Find(strText) == 0)
		{
			int iRet = SetCheck(i, bFlag);
			RecalcText();
			return iRet;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CMSStrCombo::SetCheck(int nIndex, BOOL bFlag)
{
	if (nIndex < 0 || nIndex > m_ItemsCheckState.GetUpperBound())
		return -1;

	int nResult = SetItemData(nIndex, bFlag);

	if (nResult < 0)
		return nResult;

	m_ItemsCheckState.SetAt(nIndex, (BYTE) bFlag);

	// Signal that the text need updating
	m_bTextUpdated = FALSE;

	// Redraw the window
	Invalidate(FALSE);
	
	return nResult;
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::GetCheck(int nIndex)
{
	if (m_ItemsCheckState.GetCount() <= nIndex)
		return FALSE;

	return m_ItemsCheckState.GetAt(nIndex); /*GetItemData(nIndex)*/;
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::PreCreateWindow(CREATESTRUCT& cs) 
{
	cs.style |= (CBS_OWNERDRAWFIXED | CBS_DROPDOWNLIST | CBS_HASSTRINGS | WS_VSCROLL); 
	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::GetToolTipProperties (CTooltipProperties& tp)
{ 
	DataStr val;
	GetValue(val);

	if (!val.IsEmpty())
	{
		tp.m_strText = val;
		return TRUE;
	}
	return FALSE; 
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::OnShowingPopupMenu (CMenu& menu)
{
	if (m_ItemsCheckState.GetSize() <= 0)
		return FALSE;

	if (menu.GetMenuItemCount() > 0)
		menu.AppendMenu(MF_SEPARATOR);

	menu.AppendMenu
		(
			MF_STRING , 
			ID_SELECT_ALL,
			(LPCTSTR) _TB("&Select all")
		);	

	menu.AppendMenu
		(
			MF_STRING , 
			ID_UNSELECT_ALL,
			(LPCTSTR) _TB("&Unselect all")
		);

	menu.AppendMenu
		(
			MF_STRING , 
			ID_INVERT_SELECTION,
			(LPCTSTR) _TB("&Invert selection")
		);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CMSStrCombo::SelectAll()
{
	for (int i = 0; i < m_ItemsCheckState.GetSize(); i++) {
		m_ItemsCheckState.SetAt(i,TRUE);
	}
	m_bTextUpdated = FALSE;
	RecalcText();
	ShowDropDown(FALSE);

	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::UnSelectAll()
{
	for (int i = 0; i < m_ItemsCheckState.GetSize(); i++) {
		m_ItemsCheckState.SetAt(i,FALSE);
	}
	m_bTextUpdated = FALSE;
	RecalcText();
	ShowDropDown(FALSE);
	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::InvertSelected()
{
	for (int i = 0; i < m_ItemsCheckState.GetSize(); i++) {
		m_ItemsCheckState.SetAt(i, !m_ItemsCheckState.GetAt(i));
	}
	m_bTextUpdated = FALSE;
	RecalcText();
	ShowDropDown(FALSE);
	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
BOOL CMSStrCombo::IsSelectAll()
{
	if (m_ItemsCheckState.GetSize() <= 0) return FALSE;
	
	for (int i = 0; i < m_ItemsCheckState.GetSize(); i++) {
		if (!m_ItemsCheckState.GetAt(i))
			return FALSE;
	}
	return TRUE;
}


//-----------------------------------------------------------------------------
void CMSStrCombo::ResetAssociations(BOOL bRemoveAll /* = FALSE */)
{
	ASSERT(m_hWnd);
	
	if (bRemoveAll)
	{
		// in questo modo la prossima che verra` aperta la tendina verra` ricalcolata
		// la larghezza giusta dalla AddAssociation()
		m_sizeListBox.cx = 0;
		m_nMaxItemsLen = -1;

		CBCGPComboBox::ResetContent();
		
		SetCurSel(-1);
		
		m_DataAssociations.RemoveAll();	m_DescriptionsBuffer.RemoveAll(); m_ItemsCheckState.RemoveAll();
		return;
	}
}

//-----------------------------------------------------------------------------
LRESULT CMSStrCombo::OnRecalcCtrlSize(WPARAM wp, LPARAM lp)
{
	return 0L;
}

//-----------------------------------------------------------------------------
void CMSStrCombo::OnDraw(CDC* pDC, BOOL bDrawPrompt)
{
	__super::OnDraw(pDC, bDrawPrompt);
	
	CRect rectClient;
	GetClientRect(rectClient);
	CBCGPVisualManager::GetInstance()->OnDrawControlBorder(pDC, rectClient, this, m_bOnGlass);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::OnPaint()
{
	CBCGPComboBox::OnPaint();
}

//-----------------------------------------------------------------------------
void CMSStrCombo::OnLButtonDblClk(UINT nFlags,	CPoint point)
{
	if (!OnOpenDropDown(point))
		return;

	__super::OnLButtonDblClk(nFlags, point);
}

//-----------------------------------------------------------------------------
void CMSStrCombo::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (!OnOpenDropDown(point)) 
		return;

	__super::OnLButtonDown(nFlags, point);
}

//-----------------------------------------------------------------------------
DataObj* CMSStrCombo::GetCtrlData(const CString& sName, int /*nRow = 0*/)
{
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (sName.CompareNoCase(m_DataAssociations.GetAt(i)->Str()) == 0)
			return SetCheck(i, TRUE), m_DataAssociations.GetAt(i);	//SetCheck per analogia a 
	}
	return NULL;
}

//-----------------------------------------------------------------------------
DataObj* CMSStrCombo::GetCtrlData()
{
	return __super::GetCtrlData();
}

//-----------------------------------------------------------------------------
//ritorna solo i tag delle righe selezionate
int CMSStrCombo::EnumColumnName(CStringArray& arNames, BOOL bAll /*= TRUE*/)
{
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (GetCheck(i))
			arNames.Add(m_DataAssociations.GetAt(i)->Str());
	}
	return arNames.GetSize();
}

//=============================================================================
//			Class CMSStrCombo implementation
//=============================================================================

IMPLEMENT_DYNCREATE(CMSStrButton, CMSStrCombo)

BEGIN_MESSAGE_MAP(CMSStrButton, CMSStrCombo)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMSStrButton::CMSStrButton() :
	CMSStrCombo	(),
	m_bIamge	(FALSE)
{
	SetImageNS(TBIcon(_T("Down"), MINI));
}

//-----------------------------------------------------------------------------
CMSStrButton::~CMSStrButton()
{
	m_CBmp.Detach();
}

//-----------------------------------------------------------------------------
void CMSStrButton::SetImageNS(CString strImageNS)
{ 
	LoadBitmapOrPng(&m_CBmp, strImageNS);
	m_bIamge = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CMSStrButton::OnOpenDropDown(CPoint point)
{
	if (!m_bIamge) return TRUE;
	BITMAP bmpSouce;
	m_CBmp.GetBitmap(&bmpSouce);

	if (point.x > bmpSouce.bmWidth || 
		point.y > bmpSouce.bmHeight)
	{
		return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CMSStrButton::OnDraw(CDC* pDC, BOOL bDrawPrompt)
{
	if (m_bIamge)
	{
		CDC SrcDC;
		BITMAP bmpSouce;
		SrcDC.CreateCompatibleDC(pDC);

		m_CBmp.GetBitmap(&bmpSouce);
		CBitmap* pOld = SrcDC.SelectObject(&m_CBmp);
		pDC->BitBlt(0, 0, bmpSouce.bmWidth, bmpSouce.bmHeight, &SrcDC, 0, 0, SRCCOPY);
		if (pOld)
			pDC->SelectObject(pOld);
		SrcDC.DeleteDC();
		return;
	}
	__super::OnDraw(pDC, bDrawPrompt);
}

//==============================================================================
//		class CRadioCombo
//------------------------------------------------------------------------------
// Gestione di più DataBool in un unico combo box con funzionalità tipo
// radio button, ossia assegnazione a solo un DataBool del valore TRUE
//==============================================================================
IMPLEMENT_DYNCREATE (CRadioCombo, CStrCombo)	

//-----------------------------------------------------------------------------
BOOL CRadioCombo::OnInitCtrl()
{
	m_arDataStr.SetOwns(TRUE);	//oggetti da rimuovere in uscita
	m_arDataBool.SetOwns(FALSE);//oggetti da NON rimuovere in uscita

	if ((GetStyle() & CBS_SORT) == CBS_SORT)
	{
		TRACE0("CRadioCombo MUST NOT have CBS_SORT style\n");
		ASSERT(FALSE);
	}

	if ((GetStyle() & CBS_STYLES) != CBS_DROPDOWNLIST)
	{
		TRACE0("CRadioCombo MUST have CBS_DROPDOWNLIST style\n");
		ASSERT(FALSE);
	}

	return __super::OnInitCtrl();
}

//-----------------------------------------------------------------------------
void CRadioCombo::OnFillListBox()
{
	__super::OnFillListBox(); 

	ASSERT(m_arDataStr.GetUpperBound() >= 0);
	CString s;
	DataBool* pBool;

	for (int i = 0; i <= m_arDataBool.GetUpperBound(); i++)
	{
		pBool = (DataBool*)m_arDataBool.GetAt(i);
		if (pBool && !pBool->IsReadOnly())//esclude i bool read-only
		{
			s = *(DataStr*)m_arDataStr.GetAt(i);
			__super::AddAssociation(s, s);
		}
	}
}

//-----------------------------------------------------------------------------
void CRadioCombo::ResetAssociations(BOOL bRemoveAll /* = FALSE */)
{
	__super::ResetAssociations(bRemoveAll);
}

//-----------------------------------------------------------------------------
void CRadioCombo::AddAssociation(CString sDescription, DataBool* pBool)
{ 
	// non permetto l'inserimento di descrizione vuota o puntatore NULL al DataBool 
	if (sDescription.IsEmpty() || !pBool)
	{
		ASSERT(FALSE);
		return;
	}

	m_arDataStr.Add(new DataStr(sDescription)); 
	m_arDataBool.Add(pBool); 

	// la tendina NON viene mai svuotata
	__super::AddAssociation(sDescription, sDescription);
}

//-----------------------------------------------------------------------------
void CRadioCombo::GetValue(DataObj& aValue)
{
	ASSERT_KINDOF(DataStr, &aValue);
	((DataStr&)aValue) = GetSelectedString();
}

//-----------------------------------------------------------------------------
void CRadioCombo::SetValue(LPCTSTR szStr)
{
	DataStr* pData = (DataStr* )GetCtrlData();
	if (!pData)
	{
		__super::SetValue(szStr);
		return;
	}
	
	DataStr& aData = *pData;
	aData = GetSelectedString();

	__super::SetValue(aData.GetString());
}

//-----------------------------------------------------------------------------
void CRadioCombo::SetValue(const DataObj&)
{
	DataStr* pData = (DataStr*)GetCtrlData();
	if (!pData)
	{
		__super::SetValue(_T(""));
		return;
	}

	DataStr& aData = *pData;
	aData = GetSelectedString();

	__super::SetValue(aData);
}

//-----------------------------------------------------------------------------
void CRadioCombo::DoSetValue(const DataObj&)
{
	DataStr& aData = *((DataStr*) GetCtrlData());
	aData = GetSelectedString();

	__super::DoSetValue(aData);
}

//-----------------------------------------------------------------------------
BOOL CRadioCombo::DoSetCurSel(const DataObj&)
{
	DataStr& aData = *((DataStr*) GetCtrlData());
	aData = GetSelectedString();

	return __super::DoSetCurSel(aData);
}

// la selezione provoca la valorizzazione del corretto DataBool nelle mappe 
//-----------------------------------------------------------------------------
BOOL CRadioCombo::DoSelection()
{
	DataStr aStrValue;
	__super::GetValue(aStrValue);

	int idx;
	for (idx = 0; idx <= m_arDataStr.GetUpperBound(); idx++)
	{
		DataStr* pStr = (DataStr*)m_arDataStr.GetAt(idx);
		if (pStr && *pStr == aStrValue)
		{
			SetDataObjFromSelection(idx);
			return __super::DoSelection();
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadioCombo::SetDataObjFromSelection(int idx)
{
	DataBool* pBool;

	for (int i = 0; i <= m_arDataBool.GetUpperBound(); i++)
	{
		pBool = (DataBool*)m_arDataBool.GetAt(i);
		if (!pBool)
			continue;

		if (i == idx)
			*pBool = TRUE;//pongo a TRUE il DataBool relativo all'elemento selezionato
		else
			pBool->Clear();//pongo a FALSE i DataBool relativi agli elementi non selezionati
	}
}

//-----------------------------------------------------------------------------
CString	CRadioCombo::FormatData(const DataObj* pDataObj, BOOL bEnablePadding /*= FALSE*/) const
{
	CString s = GetSelectedString();
	// se stringa vuota vuol dire che nessun booleano è a TRUE e quindi ritorna il default
	return s.IsEmpty() ? __super::FormatData(pDataObj, bEnablePadding) : s;
}

//-----------------------------------------------------------------------------
BOOL CRadioCombo::ForceUpdateCtrlView(int i/*= -1*/)	
{ 
	if (__super::ForceUpdateCtrlView(i))
		return TRUE;

	// Dato che il DataObj associato al controllo in realtà non ha alcun valore
	// verifico che la stringa associata al booleano selezionato sia uguale
	// alla stringa selezionata. Se non lo è allora forzo l'update. Questo si 
	// verifica quando vengono modificati manualmente i booleani associati al combo
	BOOL bIsEqual = !GetSelectedString().CompareNoCase(m_sReadOnlyText);

	return !bIsEqual; 
}

//-----------------------------------------------------------------------------
DataStr CRadioCombo::GetSelectedString() const
{
	DataStr aStrValue;

	int nIdx = GetSelectedIndex();
	if (nIdx != -1)
		aStrValue = *(DataStr*)m_arDataStr.GetAt(nIdx);

	return aStrValue;
}

//-----------------------------------------------------------------------------
int CRadioCombo::GetSelectedIndex() const
{
	DataBool* pBool;
	for (int i = 0; i <= m_arDataBool.GetUpperBound(); i++)
	{
		pBool = (DataBool*)m_arDataBool.GetAt(i);
		if (pBool && *pBool)
			return i;
	}
	return -1;
}

// Usata dal FieldInspector
//-----------------------------------------------------------------------------
DataBool* CRadioCombo::GetSelectedData()	const
{
	int idx = GetSelectedIndex();
	if (idx < 0)
		return NULL;

	return (DataBool*)m_arDataBool.GetAt(idx);
}

// Usata dalla OnGetTextLen e OnGetText
//-----------------------------------------------------------------------------
int CRadioCombo::GetAssociatedText(int i, LPTSTR)
{
	// si lascia chiamare la DefWindowProc poiche` quello che si vuole
	// visualizzare e` proprio cio` che compare nella tendina
	return -1;
}

// Usata dalla OnGetTextLen e OnGetTextLen
//-----------------------------------------------------------------------------
int CRadioCombo::GetAssociatedTextLen(int i)
{
	// si lascia chiamare la DefWindowProc poiche` quello che si vuole
	// visualizzare e` proprio cio` che compare nella tendina
	return -1;
}
