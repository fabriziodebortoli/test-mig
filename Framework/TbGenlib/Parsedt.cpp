
#include "stdafx.h"

#include <float.h>
#include <ctype.h>

//#include <BCGCBPro\BCGPLocalResource.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TBNameSolver\Chars.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\pictures.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbWoormEngine\INPUTMNG.H>

#include <TbParser\SymTable.h>

#include <TbXmlCore\XmlGeneric.h>

#include <TbGeneric\TBThemeManager.h>

#include "MicroareaVisualManager.h"
#include "BaseTileDialog.h"
#include "messages.h"
#include "Generic.h"
#include "parsobj.h"
#include "basedoc.h"
#include "hlinkobj.h"
#include "parsedt.h"
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

#define TIME_SPIN_DEFAULT_WIDTH 24

//Metodo che dice se il CParsedStatic e' stato creato con lo stile SS_SUNKEN
//(vedere il codice dentro la CParsedStatic::Create)
//-----------------------------------------------------------------------------
BOOL HasSunkenStyle(LONG lStyle, LONG lExStyle)
{
	return 
			(lExStyle & (WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE)) 
			||
			(lStyle & (SS_SUNKEN));
}

EnumParsedLabelDescriptionAssociations CParsedLabelDescription::singletonParsedLabelDescription;

//=============================================================================
//			Class CParsedLabelDescription implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CParsedLabelDescription, CLabelDescription)
REGISTER_WND_OBJ_CLASS(CParsedLabelDescription, Label)

//---------------------------------------------------------------------
void CParsedLabelDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_ENUM(m_LinePos, szJsonLinePos, CLabelStatic::LP_NONE);
	__super::SerializeJson(strJson);
}
//---------------------------------------------------------------------
void CParsedLabelDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_ENUM(m_LinePos, szJsonLinePos, CLabelStatic::ELinePos);
}
//---------------------------------------------------------------------
void CParsedLabelDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_LinePos = ((CParsedLabelDescription*)pDesc)->m_LinePos;
}


//-----------------------------------------------------------------------------
/*static*/CString CParsedLabelDescription::GetEnumDescription(CLabelStatic::ELinePos value)
{
	return singletonParsedLabelDescription.m_arELinePos.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CParsedLabelDescription::GetEnumValue(CString description, CLabelStatic::ELinePos& retVal)
{
	retVal = singletonParsedLabelDescription.m_arELinePos.GetEnum(description);
}


//-----------------------------------------------------------------------------
void EnumParsedLabelDescriptionAssociations::InitEnumParsedLabelDescriptionStructures()
{
	/*ELinePos*/
	m_arELinePos.Add(CLabelStatic::LP_TOP,		_T("Top"));
	m_arELinePos.Add(CLabelStatic::LP_VCENTER,	_T("VCenter"));
	m_arELinePos.Add(CLabelStatic::LP_BOTTOM,	_T("Bottom"));
	m_arELinePos.Add(CLabelStatic::LP_LEFT,		_T("Left"));
	m_arELinePos.Add(CLabelStatic::LP_RIGHT,	_T("Right"));
	m_arELinePos.Add(CLabelStatic::LP_HCENTER,	_T("HCenter"));
	m_arELinePos.Add(CLabelStatic::LP_NONE,		_T("None")); 
}

//=============================================================================
//			Class CAutoCompleteProvider implementation
//=============================================================================
//-----------------------------------------------------------------------------
CAutoCompleteProvider::CAutoCompleteProvider()
	:
	m_pList(NULL),
	m_pData(NULL)
{
}

//-----------------------------------------------------------------------------
void CAutoCompleteProvider::SetList(CStringArray* pList, DataObjArray* pDataArray /*NULL*/)
{
	m_pList = pList;
	m_pData = pDataArray;
}

//-----------------------------------------------------------------------------
void  CAutoCompleteProvider::OnAutoComplete	(CString strValue, CStringList* pList)
{
	if (!m_pList)
		return;

	for (int i = 0; i < m_pList->GetSize(); i++)
	{
		CString sItem = m_pList->GetAt(i);

		if (CanAutoComplete(strValue, sItem))
		{
			pList->AddTail(sItem);
		}
	}
}

//-----------------------------------------------------------------------------
DataObj* CAutoCompleteProvider::GetDataOf (CString strItem)
{
	if (!m_pData)
		return NULL;

	for (int i = 0; i < m_pList->GetSize(); i++)
	{
		CString sItem = m_pList->GetAt(i);

		if (sItem.CompareNoCase(strItem) == 0 && m_pData->GetSize() > i)
			return m_pData->GetAt(i);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CAutoCompleteProvider::CanAutoComplete (const CString& strValue, const CString& strItem)
{
	if (strValue.IsEmpty())
		return TRUE;

	CString sVal (strValue);
	CString sItem (strItem);
	sVal.MakeLower();
	sItem.MakeLower();

	return sItem.Find(sVal) >= 0;
}

//=============================================================================
//			Class CParsedEdit implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedEdit, CBCGPEdit)

BEGIN_MESSAGE_MAP(CParsedEdit, CBCGPEdit)
	//{{AFX_MSG_MAP(CParsedEdit)
	ON_WM_CTLCOLOR_REFLECT	()
	ON_WM_PAINT				()
	//ON_WM_ERASEBKGND		()
	ON_WM_KILLFOCUS			()
	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_VSCROLL			()     // for associated spin controls
	ON_WM_CHAR				()
	ON_WM_KEYUP				()
	ON_WM_LBUTTONUP			()
	ON_WM_RBUTTONDOWN		()
	ON_WM_KEYDOWN			()
	ON_WM_SETFOCUS			()
	ON_WM_ENABLE			()
    
    //An.21569:
	ON_CONTROL_REFLECT_EX(EN_CHANGE, OnChange)
	
	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,			OnPushButtonCtrl)
	
	ON_WM_CONTEXTMENU		()
	
	ON_COMMAND_RANGE		(ID_FORMAT_STYLE_MENU_CMD_0, (UINT)(ID_FORMAT_STYLE_MENU_CMD_19), OnFormatPopupMenuItemSelected)
	
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)
	ON_MESSAGE				(WM_PASTE,	OnPaste)
	
	ON_COMMAND			(WM_UNDO,	OnUndo)
	ON_COMMAND			(WM_CUT,	OnCut)
	ON_COMMAND			(WM_COPY,	OnCopy)
	ON_COMMAND			(WM_PASTE,	OnPaste)
	ON_COMMAND			(WM_CLEAR,	OnClear)
	ON_COMMAND			(WM_MENUSELECT,	OnSelectAll)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
 
//disabilito il warning sul this passato al costruttore
#pragma warning(disable: 4355) 
//-----------------------------------------------------------------------------
CParsedEdit::CParsedEdit(DataObj* pData /*=NULL*/)
	:
	CBCGPEdit			(),
	CParsedCtrl			(pData),
	CColoredControl		(this),
	CCustomFont			(this),
	IDisposingSourceImpl(this),
	
	m_bDisableSelection(FALSE),
	m_pAutoCompleteProvider	(NULL)	
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CParsedEdit::CParsedEdit(UINT nBtnIDBmp, DataObj* pData /*=NULL*/)
	:
	CBCGPEdit			(),
	CParsedCtrl			(pData),
	CColoredControl		(this),
	CCustomFont			(this),
	IDisposingSourceImpl(this),

	m_bDisableSelection		(FALSE),
	m_pAutoCompleteProvider	(NULL)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::Attach(nBtnIDBmp);
	CParsedCtrl::AttachCustomFont(this);

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}


//-----------------------------------------------------------------------------
HRESULT CParsedEdit::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}


//-----------------------------------------------------------------------------
CParsedEdit::~CParsedEdit()
{
	SAFE_DELETE(m_pAutoCompleteProvider);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

#pragma warning(default: 4355)

//-----------------------------------------------------------------------------
void CParsedEdit::AppendEditMenu(CMenu* pMenu)
{
	pMenu->AppendMenu(MF_SEPARATOR);
	pMenu->AppendMenu(MF_STRING, WM_UNDO,	_TB("Undo"));
	pMenu->AppendMenu(MF_SEPARATOR);
	pMenu->AppendMenu(MF_STRING, WM_CUT,	_TB("Cut"));
	pMenu->AppendMenu(MF_STRING, WM_COPY,	_TB("Copy"));
	pMenu->AppendMenu(MF_STRING, WM_PASTE,	_TB("Paste"));
	pMenu->AppendMenu(MF_STRING, WM_CLEAR,	_TB("Delete"));
	pMenu->AppendMenu(MF_SEPARATOR);
	//TODO verificare che sia l'ID giusto
	pMenu->AppendMenu(MF_STRING, WM_MENUSELECT,	_TB("Select All"));
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::CanEdit() const
{
	return this->IsWindowEnabled() && (this->GetStyle() & ES_READONLY) == 0;
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnUndo()
{
	if (CanUndo() && CanEdit())
		this->Undo();
}       

//-----------------------------------------------------------------------------
void CParsedEdit::OnPaste()
{
	if (CanEdit())
		this->Paste();
}       
//-----------------------------------------------------------------------------
void CParsedEdit::OnCut()
{
	if (CanEdit())
		this->Cut();
}       
//-----------------------------------------------------------------------------
void CParsedEdit::OnCopy()
{
	this->Copy();
}       
//-----------------------------------------------------------------------------
void CParsedEdit::OnClear()
{
	if (CanEdit())
		this->Clear();
}       
//-----------------------------------------------------------------------------
void CParsedEdit::OnSelectAll()
{
	this->SetSel(0,-1);
}
//-----------------------------------------------------------------------------
BOOL CParsedEdit::ForceUpdateCtrlView	(int i/*= -1*/)	
{ 
	if ((GetStyle() & ES_MULTILINE) == ES_MULTILINE)
		return TRUE;

	return FALSE; 
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedEdit::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if ((dwStyle & ES_MULTILINE) != ES_MULTILINE)
		dwStyle |= ES_AUTOHSCROLL;

/*@@TODO ES_RIGHT
	switch(GetDataType().m_wType)
	{
		case DATA_INT_TYPE:
		case DATA_LNG_TYPE:
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
			if ((m_dwCtrlStyle & NUM_STYLE_NO_RIGHT_ALIGN) != NUM_STYLE_NO_RIGHT_ALIGN)
				dwStyle |= ES_RIGHT;
			break;
	}
*/
	BOOL bOk = CheckControl(nID, pParentWnd);
	if (!bOk) return FALSE;

	if (dwStyle & WS_EX_CLIENTEDGE)
	{
	   bOk = CreateEx(WS_EX_CLIENTEDGE, _T("EDIT"), _T(""), dwStyle & ~WS_EX_CLIENTEDGE, rect, pParentWnd, nID);
	}
	else
	{
		bOk = CBCGPEdit::Create(dwStyle, rect, pParentWnd, nID);
	}

	this->m_bVisualManagerStyle = TRUE;
	
	if (!bOk) 
		return FALSE;
	
	bOk = CreateAssociatedButton(pParentWnd) && InitCtrl();

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::OnInitCtrl ()
{
	__super::OnInitCtrl();

	if (!AfxGetThemeManager()->GetControlsUseBorders())
	{
		ModifyStyleEx(WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE, 0, SWP_FRAMECHANGED);

		ModifyStyle(WS_BORDER, 0, SWP_FRAMECHANGED);
	}
	return TRUE;
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedEdit::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk = CheckControl(nID, pParentWnd, _T("EDIT"));
	bOk = bOk && SubclassDlgItem(nID, pParentWnd);
	bOk = bOk && CreateAssociatedButton(pParentWnd);
	bOk = bOk && InitCtrl();

	if (bOk)
		SetNamespace(strName);

	return bOk;
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnKillFocus (CWnd* pWnd)
{

	DoKillFocus(pWnd);

	// standard action
	CBCGPEdit::OnKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::OnChildNotify(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pLResult)
{
	if ( message == WM_COMMAND && HIWORD( wParam ) == EN_SETFOCUS )
	{
		CBCGPEdit::SetSel(0, -1);
		// Avoid message reflection 'eats' the EN_SETFOCUS notification
		return FALSE;
	}

	return __super::OnChildNotify(message, wParam, lParam, pLResult);
}

//An.21569:
//Workaround per il seguente problema: quando digito la prima lettera in una masked edit con autocompletamento
//non viene mostrata la tendina con le opzioni per l'autocompletamento.
//Dalla seconda pressione in poi funziona tutto.
//Il problema e` dovuto al metodo GetWindowText del masked edit che ritorna il valore vecchio anziche` quello attuale.
//Questa e` un'anomalia che abbiamo segnalato ai BCG, siamo in attesa la risolvano.
//Ho aggirato questo problema riscrivendo la OnChange e decidendo se chiamare quella papa` oppure un clone
//di quella papa` che pero` lavora sulla stringa aggiornata.
//**********************************************************************************************************
BOOL CParsedEdit::OnChange() 
{
	CString strText;
	GetWindowText(strText);

	if (strText.IsEmpty())
	{
		CString strActualText;
		CParsedCtrl::GetValue(strActualText);
		return OnChangeInternal(strActualText);
	}
	else
	{
		return __super::OnChange();
	}
}
//An.21569:
//Workaround per il seguente problema: quando digito la prima lettera in una masked edit con autocompletamento
//non viene mostrata la tendina con le opzioni per l'autocompletamento.
//Dalla seconda pressione in poi funziona tutto.
//Il problema e` dovuto al metodo GetWindowText del masked edit che ritorna il valore vecchio anziche` quello attuale.
//Questa e` un'anomalia che abbiamo segnalato ai BCG, siamo in attesa la risolvano.
//Ho aggirato questo problema riscrivendo la OnChange e decidendo se chiamare quella papa` oppure un clone
//di quella papa` che pero` lavora sulla stringa aggiornata.
//**********************************************************************************************************
BOOL CParsedEdit::OnChangeInternal(CString strText) 
{
	if (m_bOnGlass)
	{
		InvalidateRect (NULL, FALSE);
		UpdateWindow ();
	}

#ifndef _BCGSUITE_
	BOOL bIsAutocompleteAvailable = (m_Mode == BrowseMode_None || m_Mode == BrowseMode_Default);

	if (bIsAutocompleteAvailable && !m_bInAutoComplete)
	{
		BOOL bDestroyDropDown = TRUE;

		if (!strText.IsEmpty() && (GetStyle() & ES_MULTILINE) == 0)
		{
			CStringList	lstAutocomplete;
			if (OnGetAutoCompleteList(strText, lstAutocomplete) && !lstAutocomplete.IsEmpty())
			{
				bDestroyDropDown = FALSE;

				if (::IsWindow(m_pDropDownPopup->GetSafeHwnd()) && m_pDropDownPopup->Compare(lstAutocomplete))
				{
					// Keep existing list
				}
				else
				{
					CreateAutocompleteList(lstAutocomplete);
				}
			}
		}

		if (bDestroyDropDown)
		{
			CloseAutocompleteList();
		}
	}
#endif

	if (m_bSearchMode || m_bHasPrompt)
	{
		BOOL bTextIsEmpty = m_bTextIsEmpty;

		m_bTextIsEmpty = strText.IsEmpty();

		if (!m_strErrorMessage.IsEmpty())
		{
			SetErrorMessage(NULL, m_clrErrorText);
		}
		else if (bTextIsEmpty != m_bTextIsEmpty)
		{
			RedrawWindow (NULL, NULL, RDW_FRAME | RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW);
		}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedEdit::DestroyCalendar()
{
	// se ho la gestione del calendario e posso chiuderlo
	// allora eseguo anche la chiusura del calendario
	CButton *pButton = GetButton();
	if (pButton && pButton->IsKindOf(RUNTIME_CLASS(CCalendarButton)))
	{
		CCalendarButton* pCalendarButton = (CCalendarButton*) pButton;

		BOOL bIsEnabledOrGrid = IsWindowEnabled() || dynamic_cast<CGridControlObj*>(GetCtrlParent());

		if (bIsEnabledOrGrid && pCalendarButton->IsDestroyCalendarEnabled())
			pCalendarButton->SendMessage(UM_DESTROY_CALENDAR);//bug #14280
	}
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	GridCellPosChanging(wndPos);
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::NeedCustomPaint()
{
	return	
			!m_pHyperLink 
			&& !IsWindowEnabled() /* &&
			m_pDocument && m_pDocument->UseEasyReading()*/
			;
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnPaint()
{
	__super::OnPaint();
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnEnable(BOOL bEnable)
{
	__super::OnEnable(bEnable);
	
	DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CParsedEdit::SetModifyFlag(BOOL bFlag)
{ 
	CParsedCtrl::SetModifyFlag(bFlag);

	SetModify(bFlag);
}
	
//-----------------------------------------------------------------------------
BOOL CParsedEdit::GetModifyFlag()			
{ 
	if (CParsedCtrl::GetModifyFlag())
		return TRUE;
		
	return GetModify();
}

//-----------------------------------------------------------------------------
void CParsedEdit::SetValue(const DataObj& aValue)
{
	DataObj& pData = const_cast<DataObj&>(aValue);
	CParsedCtrl::SetValue(pData.Str());

}

//-----------------------------------------------------------------------------
void CParsedEdit::GetValue(DataObj& aValue)
{
	CString strBuffer;
	
	CParsedCtrl::GetValue(strBuffer);
	DoMaskedGetValue(strBuffer, aValue);
}

//-----------------------------------------------------------------------------
void CParsedEdit::SetCtrlSel(int nStart, int nEnd)
{
	CBCGPEdit::SetSel(nStart, nEnd);
}

//-----------------------------------------------------------------------------
void CParsedEdit::GetCtrlSel (int& nStart, int& nEnd) 
{ 
	CBCGPEdit::GetSel(nStart, nEnd);
}

//-----------------------------------------------------------------------------
void CParsedEdit::SetCtrlMaxLen(UINT nLen, BOOL bApplyNow)
{
	m_nCtrlLimit = nLen;
	
	if (bApplyNow)
		CBCGPEdit::LimitText(nLen);
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::OnCommand(WPARAM wParam, LPARAM lParam)
{
	CParsedCtrl::DoCommand(wParam, lParam);

	BOOL bDone = CBCGPEdit::OnCommand(wParam, lParam);

	//guardo se è un comando di menu o un accelleratore
	CWnd* pParent = GetCtrlParent();
	CDocument* pDoc  = GetDocument ();
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
			pDoc->GetNextView(pos)->PostMessage (WM_COMMAND, wParam, 0);
	}

	return bDone;
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (!DoOnChar(nChar))
		CBCGPEdit::OnChar(nChar, nRepCnt, nFlags);

	if (_istcntrl(nChar))
		return;

	// migl. #2772 il control non è più ES_UPPERCASE, ma si definisce
	// il tipo di UpperCase da applicare sulla base del DataObj
	if ((m_dwCtrlStyle & STR_STYLE_UPPERCASE) == STR_STYLE_UPPERCASE)
	{
		CString sText;
		GetWindowText (sText);
		DWORD nSel = GetSel();
		AfxGetCultureInfo()->MakeUpper (sText);

		SetWindowText (sText);
		SetModifyFlag (TRUE);
		SetSel(nSel);
	}

	if (nChar == VK_RETURN && nFlags == 0)
	{
		CWnd* pParent = GetCtrlParent();
		CDocument* pDoc = GetDocument();
		if (!pDoc && pParent)
		{
			CParsedForm* pForm = dynamic_cast<CParsedForm*>(pParent);
			if (pForm)
				pDoc = pForm->GetDocument();
		}
		if (pParent)
			pParent->PostMessage(WM_COMMAND, GetDlgCtrlID(), 0);
		if (pDoc)
			((CBaseDocument*)pDoc)->PostMessage(WM_COMMAND, GetDlgCtrlID(), 0);
	}
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (!DoKeyUp(nChar))
		CBCGPEdit::OnKeyUp(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedEdit::SetAutoCompleteList(CStringArray* pAutoCompleteList, DataObjArray* pDataArray /*NULL*/)
{
	if (!pAutoCompleteList)
	{
		SAFE_DELETE(m_pAutoCompleteProvider);
		return;
	}

	if (!m_pAutoCompleteProvider)
		m_pAutoCompleteProvider = new CAutoCompleteProvider();
	m_pAutoCompleteProvider->SetList(pAutoCompleteList, pDataArray);
}

//-----------------------------------------------------------------------------
BOOL CParsedEdit::OnGetAutoCompleteList	(const CString& strEditText, CStringList& lstAutocomplete)
{
	__super::OnGetAutoCompleteList(strEditText, lstAutocomplete);
	if (m_pAutoCompleteProvider)
	{
		CString actualText;
		CParsedCtrl::GetValue(actualText);
		m_pAutoCompleteProvider->OnAutoComplete(actualText, &lstAutocomplete);
	}
	return !lstAutocomplete.IsEmpty();
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (m_bInAutoComplete && m_pAutoCompleteProvider)
	{
		CString strText;
		GetWindowText(strText);
		DataObj* pData = m_pAutoCompleteProvider->GetDataOf(strText);
		if (pData)
			SetWindowText(pData->FormatData());
		SetModifyFlag(TRUE);
	}
			
	DestroyCalendar();

	if (!DoKeyDown(nChar))
	{
		__super::OnKeyDown(nChar, nRepCnt, nFlags);
}		
}		

//-----------------------------------------------------------------------------
void CParsedEdit::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	if (!CParsedCtrl::DoRButtonDown(nFlag, mousePos))
	{
		__super::OnRButtonDown(nFlag, mousePos);
	}
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CBCGPEdit::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnLButtonUp(UINT nFlag, CPoint mousePos)
{
	__super::OnLButtonUp(nFlag, mousePos);

	if (m_nButtonIDBmp != BTN_SPIN_ID)
	 	return;
	
	NotifyToParent(EN_SPIN_RELEASED);
}
	
//-----------------------------------------------------------------------------
void CParsedEdit::OnVScroll (UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{                  
	if (m_nButtonIDBmp != BTN_SPIN_ID)
	{
		CBCGPEdit::OnVScroll(nSBCode, nPos, pScrollBar);
		return;
	}                          
	
	DoSpinScroll(nSBCode);
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnSetFocus(CWnd* pOldCWnd)
{
	__super::OnSetFocus(pOldCWnd);

	if (m_bDisableSelection)
		return;

	int nStart = GetFormatMask() ? GetFormatMask()->GetEditableZoneStart() : 0;
	SetCtrlSel(nStart,-1);
	DoSetFocus(pOldCWnd);
}

//-----------------------------------------------------------------------------
LRESULT CParsedEdit::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
LRESULT CParsedEdit::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	/*Il metodo GetWindowDescription crea da zero unad	escrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CEditObjDescription* pDesc = (CEditObjDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CEditObjDescription), strId);
	pDesc->m_Type = CWndObjDescription::Edit;
	pDesc->UpdateAttributes(this);
	
	
	PopulateColorDescription(pDesc);
	PopulateFontDescription(pDesc);
	Formatter* pFormatter = (Formatter*)GetCurrentFormatter();
	if (pDesc->m_pFormatter != pFormatter)
	{
		pDesc->m_pFormatter = pFormatter;
		pDesc->SetUpdated(&pDesc->m_pFormatter);
	}
	return (LRESULT) pDesc;
}
//-----------------------------------------------------------------------------
LRESULT CParsedEdit::OnPaste(WPARAM wParam, LPARAM lParam)
{
	DefWindowProc(WM_PASTE, wParam, lParam);
	SetModifyFlag (TRUE);
	
	if (m_dwCtrlStyle & STR_STYLE_UPPERCASE)
	{
		CString sText;
		GetWindowText(sText);
		AfxGetCultureInfo()->MakeUpper (sText);
		SetWindowText(sText);
	}
	return 1L;
}

//-----------------------------------------------------------------------------
void CParsedEdit::OnFormatPopupMenuItemSelected	(UINT nID)
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
			PostMessage(UM_FORMAT_STYLE_CHANGED);
			return;
		}
	}	
}

// gestisce lo stato di ReadOnly per la parte editabile del control
//-----------------------------------------------------------------------------
void CParsedEdit::SetEditReadOnly (const BOOL bValue)
{
	BOOL bIsReadOnly = IsEditReadOnly();

	// lo è già in stato corretto
	//if ((bIsReadOnly && bValue) || (!bIsReadOnly && !bValue))
	//	return;

	// la finestra non è abilitata e ma devo farlo
	if (!IsWindowEnabled ()) //simmetrica //fix ? && bValue) //originale && !bValue)
		EnableWindow(TRUE);

	CBCGPEdit::SetReadOnly(bValue);

	if (m_pButton)
		DoEnable(!bValue);
}


//=============================================================================
//			Class CStrEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CStrEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CStrEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CStrEdit)
	ON_WM_KILLFOCUS		()
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CStrEdit::CStrEdit()
	:
	CParsedEdit ()
{
	SetCtrlStyle(STR_STYLE_ALL);
	m_nDirStrech = 0;
}

//-----------------------------------------------------------------------------
CStrEdit::CStrEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CParsedEdit (nBtnIDBmp)
{
	SetCtrlStyle(STR_STYLE_ALL);
	m_nDirStrech = 0;

	Attach(pData);
}

//-----------------------------------------------------------------------------
void CStrEdit::Attach(DataObj* pDataObj)
{
	if (!(pDataObj == NULL || pDataObj->GetDataType() == GetDataType()))
		if (
			!	(
				(pDataObj->GetDataType() == DataType::Guid || pDataObj->GetDataType() == DataType::Text) &&
				GetDataType() == DataType::String
				)
			)
			ASSERT(FALSE);
		
	CParsedCtrl::Attach(pDataObj);

	if (GetCtrlData() && ((DataStr*)GetCtrlData())->IsUpperCase())
		m_dwCtrlStyle |= STR_STYLE_UPPERCASE;
}

//-----------------------------------------------------------------------------
BOOL CStrEdit::OnInitCtrl()
{
	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CStrEdit::SetValue(const DataObj& aValue)
{
	if (( GetStyle() & ES_PASSWORD) == ES_PASSWORD)
	{
		((DataStr&) aValue).SetPrivate();
	}

	ASSERT(CheckDataObjType(&aValue));

	SetValue(((DataStr&)aValue).GetString());

}

//-----------------------------------------------------------------------------
void CStrEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	// An. 22146
	// workaround suggerito dal supporto BCG per prelevare l'efettivo valore nel campo
	CString strBuffer;
	CParsedCtrl::GetValue(strBuffer);
	DoMaskedGetValue(strBuffer, aValue);
}

//-----------------------------------------------------------------------------
CString CStrEdit::GetValue()
{
	CString str;
	GetValue(str);
	
	return str;
}

//-----------------------------------------------------------------------------
BOOL CStrEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
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

	if (IsStrChar(nChar, m_dwCtrlStyle))
		return FALSE;

	BadInput();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CStrEdit::IsValidStr(LPCTSTR pszStr)
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

//------------------------------------------------------------------------------
void CStrEdit::PositionCursor(DWORD dwCurPos)
{
	SetFocus();
	SetSel(dwCurPos);

	int nLine = LineFromChar();

	// per lasciare al meno una riga sopra a quella cui si scrolla
	if (nLine > 1)	nLine--;
	LineScroll(nLine, 0);
}

//------------------------------------------------------------------------------
void CStrEdit::PositionCursor(int nLine, int nCol)
{
	int nPos = LineIndex(nLine) + nCol;

	SetFocus();
	SetSel(nPos, nPos);

	// per lasciare al meno una riga sopra a quella cui si scrolla
	if (nLine > 1)	nLine--;
	LineScroll(nLine, 0);
}

//-----------------------------------------------------------------------------
void CStrEdit::OnKillFocus (CWnd* pWnd)
{
	Invalidate();
	CParsedEdit::OnKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
void CStrEdit::EnableFindBrowseButton(BOOL bEnable/* = TRUE*/, LPCTSTR szLabel/* = L""*/, BOOL bRedraw/* = TRUE*/)
{
	__super::EnableBrowseButton(bEnable , szLabel);
	if (!bEnable) return;

/*
	m_bDefaultImage = FALSE;
	//CBCGPLocalResource locaRes;
	VERIFY(m_ImageBrowse.Load(globalData.Is32BitIcons() ?
		IDB_BCGBARRES_SEARCH32 : IDB_BCGBARRES_SEARCH));
	m_ImageBrowse.SetSingleImage();
	m_ImageBrowse.SetTransparentColor(globalData.clrBtnFace);

	m_sizeImage = globalUtils.ScaleByDPI(m_ImageBrowse);
*/
	SetBrowseButtonImage(TBLoadPng(TBGlyph(szGlyphSearch32)));

	if (bRedraw && GetSafeHwnd() != NULL)
	{
		RedrawWindow(NULL, NULL, RDW_FRAME | RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW);
	}
}

//-----------------------------------------------------------------------------
void CStrEdit::OnBrowse()
{
	CWnd* pParent = GetParent();
	while (pParent)
	{
		if (dynamic_cast<CParsedForm*>(pParent))
		{
			pParent->SendMessage(WM_COMMAND, GetDlgCtrlID());
			return;
		}
		pParent = pParent->GetParent();
	}
}

//-----------------------------------------------------------------------------
BOOL CStrEdit::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName)
{
	if (!CParsedEdit::SubclassEdit(IDC, pParent, strName))
		return FALSE;

	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CStrEdit::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//=============================================================================
//			Class CResizableStrEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CResizableStrEdit, CStrEdit)

//-----------------------------------------------------------------------------
CResizableStrEdit::CResizableStrEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CStrEdit (nBtnIDBmp, pData)
{
	m_nDirStrech = 3;
}

//-----------------------------------------------------------------------------
CResizableStrEdit::CResizableStrEdit()
	:
	CStrEdit ()
{
	m_nDirStrech = 3;
}

//=============================================================================
//			Class CTextEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTextEdit, CResizableStrEdit)

BEGIN_MESSAGE_MAP(CTextEdit, CResizableStrEdit)
	//{{AFX_MSG_MAP(CTextEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CTextEdit::CTextEdit()
	:
	CResizableStrEdit ()
{
	SetCtrlStyle(STR_STYLE_ALL);
}

//-----------------------------------------------------------------------------
CTextEdit::CTextEdit(UINT nBtnIDBmp, DataText* pData /* = NULL */)
	:
	CResizableStrEdit (nBtnIDBmp, pData)
{
}

//=============================================================================
//			Class CIntEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CIntEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CIntEdit)
	ON_MESSAGE				(UM_FORMAT_STYLE_CHANGED,		OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CIntEdit::CIntEdit()
	:
	CParsedEdit	(),
	m_nMin		(SHRT_MIN),
	m_nMax		(SHRT_MAX),
	m_nCurValue	(0)

{}

//-----------------------------------------------------------------------------
CIntEdit::CIntEdit(UINT nBtnIDBmp, DataInt* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData),
	m_nMin		(SHRT_MIN),
	m_nMax		(SHRT_MAX),
	m_nCurValue	(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CIntEdit::SetRange	(int nMin, int nMax)
{                      
	if (nMin > nMax)
		nMax = nMin;

	m_nMin = nMin;
	m_nMax = nMax > SHRT_MAX ? SHRT_MAX : nMax;
}

//-----------------------------------------------------------------------------
void CIntEdit::SetValue(int nValue)
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
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(7);
			_stprintf_s(szBuffer, nLen,_T("%d"), nValue);
			strBuffer.ReleaseBuffer();
		}

	m_nCurValue = nValue;

	CParsedCtrl::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
int CIntEdit::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	if (m_nFormatIdx >= 0)
	{
		CIntFormatter* pFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);
	}

	return _tstoi(strValue);
}

//-----------------------------------------------------------------------------
void CIntEdit::SetValue(const DataObj& aValue)
{             
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));
	SetValue((int) ((DataInt &)pData));

}

//-----------------------------------------------------------------------------
void CIntEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataInt &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CIntEdit::IsValidInt(int nValue)
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
CString CIntEdit::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	if (nIDP == INT_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), pszBadVal, (int) m_nMin, (int) m_nMax);
	
	return CParsedCtrl::FormatErrorMessage(nIDP, pszBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CIntEdit::DoSpinScroll(UINT nSBCode)
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
BOOL CIntEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetSel();
	CString	strValue; CParsedCtrl::GetValue (strValue);

	CIntFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

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

			SetSel(nPos, nPos);

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
BOOL CIntEdit::DoKeyDown(UINT nChar)
{       
	if (CParsedEdit::DoKeyDown(nChar))
		return TRUE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		CString	strValue;
		CParsedCtrl::GetValue (strValue);

		DWORD dwPos = GetSel();
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

			SetSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
		}

		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CIntEdit::DoKillFocus (CWnd* pWnd)
{
	if (GetModifyFlag() && IsValid())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedEdit::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CIntEdit::OnInitCtrl()
{
	m_nCtrlLimit = 6;

	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CIntEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
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
//			Class CLongEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLongEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CLongEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CLongEdit)
	ON_MESSAGE				(UM_FORMAT_STYLE_CHANGED,		OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLongEdit::CLongEdit()
	:
	CParsedEdit	(),
	m_lMin		(LONG_MIN),
	m_lMax		(LONG_MAX),
	m_lCurValue	(0)
{}

//-----------------------------------------------------------------------------
CLongEdit::CLongEdit(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData),
	m_lMin		(LONG_MIN),
	m_lMax		(LONG_MAX),
	m_lCurValue	(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CLongEdit::SetRange	(int nMin, int nMax)
{
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = nMin;
	m_lMax = nMax;
}

//-----------------------------------------------------------------------------
void CLongEdit::SetValue(long nValue)
{
	CString strBuffer;
	if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) != NUM_STYLE_HEXADECIMAL_VALUE)
	{
		CLongFormatter* pFormatter = NULL;

		SetNewFormatIdx();

		if (m_nFormatIdx >= 0)
			pFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
		if (pFormatter && pFormatter->GetFormat() != CIntFormatter::LETTER && pFormatter->GetFormat() != CIntFormatter::ENCODED)
		{
			if	(
					pFormatter->IsZeroPadded() &&
					pFormatter->GetPaddedLen() > 0	&&
					pFormatter->GetPaddedLen() < 12
				)
				pFormatter->Format(&nValue, strBuffer, FALSE);
			else
				if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
					pFormatter->Format(&nValue, strBuffer, FALSE);
		}
		else
			if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
			{
				const rsize_t nLen = 12;
				TCHAR* szBuffer = strBuffer.GetBufferSetLength(12);
				_stprintf_s(szBuffer, nLen, _T("%ld"), nValue);
				strBuffer.ReleaseBuffer();
			}
	}	
	else
		strBuffer.Format(_T("0x%.8X"), (DWORD)nValue);

	m_lCurValue = nValue;

	CParsedCtrl::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
long CLongEdit::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue(strValue);

	if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) == NUM_STYLE_HEXADECIMAL_VALUE)
	{
		long nValue;
		_stscanf_s((LPCTSTR)strValue, _T("%X"), &nValue);
		return nValue;
	}

	if (m_nFormatIdx >= 0)
	{
		CLongFormatter* pFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);
	}

	return _tstol(strValue);
}

//-----------------------------------------------------------------------------
void CLongEdit::SetValue(const DataObj& aValue)
{             
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));
	SetValue((long) ((DataLng &)pData));

}

//-----------------------------------------------------------------------------
void CLongEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataLng &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CLongEdit::IsValidLong(long nValue)
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
CString CLongEdit::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	if (nIDP == LONG_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), pszBadVal, (long) m_lMin, (long) m_lMax);
	
	return CParsedCtrl::FormatErrorMessage(nIDP, pszBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CLongEdit::DoSpinScroll(UINT nSBCode)
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
BOOL CLongEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetSel();
	CString	strValue; CParsedCtrl::GetValue (strValue);

	CIntFormatter*	pFormatter = NULL;
	TCHAR			ch1000Sep = 0;
	long			nVal;	

	if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) != NUM_STYLE_HEXADECIMAL_VALUE)
	{
		if (m_nFormatIdx >= 0)
			pFormatter = (CIntFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter && !pFormatter->Get1000Separator().IsEmpty())
			ch1000Sep = pFormatter->Get1000Separator()[0];
		
		nVal = _tstol(pFormatter ? pFormatter->UnFormat(strValue) : strValue);
	}
	else
		_stscanf_s((LPCTSTR)strValue, _T("%x"), &nVal);

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

			SetSel(nPos, nPos);

			return TRUE;
		}

		// il control vuoto e` accettato
		return FALSE;
	}

	if	(!IsValidChar(nChar, strValue, dwPos, nPos))
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

	double dVal;
	if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) == NUM_STYLE_HEXADECIMAL_VALUE)
	{
		_stscanf_s((LPCTSTR)strValue, _T("%x"), &nVal);
		dVal = (double)nVal;
	}
	else
		dVal = _tstof(pFormatter ? pFormatter->UnFormat(strValue) : strValue);

	if (dVal < LONG_MIN || dVal > LONG_MAX)
	{
		BadInput();
		return TRUE;
	}

	UpdateNumericInput(dVal, strValue, dwPos, nPos, ch1000Sep, nCurNr1000Sep);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLongEdit::IsValidChar(UINT nChar, const CString& strValue, DWORD dwPos, int nPos)
{
	if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) == NUM_STYLE_HEXADECIMAL_VALUE)
		return
			(
				(nPos != 0 || nChar == _T('0')) &&
				(nPos != 1 || nChar == _T('x')|| nChar == _T('X')) &&
				(nPos < 2 || _istdigit(nChar) || nChar == VK_BACK || (nChar >= _T('a') && nChar <= _T('f')) || (nChar >= _T('A') && nChar <= _T('F')))
			);
	return
		(
			(_istdigit(nChar) || nChar == VK_BACK) &&
			(dwPos != 0 || strValue.IsEmpty() || _istdigit(strValue[0])) 
		);
}

//-----------------------------------------------------------------------------
BOOL CLongEdit::DoKeyDown(UINT nChar)
{       
	if (CParsedEdit::DoKeyDown(nChar))
		return TRUE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		if ((m_dwCtrlStyle & NUM_STYLE_HEXADECIMAL_VALUE) != NUM_STYLE_HEXADECIMAL_VALUE)
		{
			CString	strValue;
			CParsedCtrl::GetValue (strValue);

			DWORD dwPos = GetSel();
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
				SetSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
			}

		}
		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CLongEdit::DoKillFocus (CWnd* pWnd)
{
	if (GetModifyFlag()  && IsValid())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedEdit::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CLongEdit::OnInitCtrl()
{
	m_nCtrlLimit = 11;

	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}


//-----------------------------------------------------------------------------
LRESULT CLongEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
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
//			Class CDoubleEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDoubleEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CDoubleEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CDoubleEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDoubleEdit::CDoubleEdit()
	:
	CParsedEdit	()
{}

//-----------------------------------------------------------------------------
CDoubleEdit::CDoubleEdit(UINT nBtnIDBmp, DataDbl* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CDoubleEdit::SetRange	(double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;

	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}
//-----------------------------------------------------------------------------
CDblFormatter* CDoubleEdit::GetFormatter() const
{
	CDblFormatter* pFormatter = (CDblFormatter*)GetCurrentFormatter();
	return pFormatter;
}

//-----------------------------------------------------------------------------
int CDoubleEdit::GetCtrlNumDec ()
{
	if (m_pExternalNumDec)
		return (int) *m_pExternalNumDec;
	if (m_nDec >= 0)
		return m_nDec;

	CDblFormatter* pFormatter = GetFormatter();
	
	return pFormatter ? pFormatter->GetDecNumber() : m_nDec;
}

//-----------------------------------------------------------------------------
void CDoubleEdit::SetValue(double nValue)
{
	CString strBuffer;

	if (
			(fabs(nValue) >= ::GetEpsilonForDataType(GetDataType())) ||
			(m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO
		)
	{
		SetNewFormatIdx();
		
		CDblFormatter* pFormatter = GetFormatter();
		if	(
				pFormatter												&&
				pFormatter->GetFormat() != CDblFormatter::LETTER		&&
				pFormatter->GetFormat() != CDblFormatter::ENCODED		&&
				pFormatter->GetFormat() != CDblFormatter::EXPONENTIAL	&&
				pFormatter->GetFormat() != CDblFormatter::ENGINEER
			)
		{
			int nOldDN;
			if (m_pExternalNumDec || m_nDec >= 0)
				nOldDN = pFormatter->SetDecNumber(GetCtrlNumDec());

			pFormatter->Format(&nValue, strBuffer, FALSE);

			if (m_pExternalNumDec || m_nDec >= 0)
				pFormatter->SetDecNumber(nOldDN);
		}
		else
		{
			const rsize_t nLen = 512;
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(512);
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

	m_dCurValue = nValue;
		
	CParsedCtrl::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
double CDoubleEdit::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	CDblFormatter* pFormatter = GetFormatter();
	if (pFormatter)
		strValue = pFormatter->UnFormat(strValue);		

	return _tstof(strValue);
}

//-----------------------------------------------------------------------------
void CDoubleEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataDbl &)aValue).Assign(GetValue());

	if (!aValue.IsEmpty() && (m_dwCtrlStyle & NUM_STYLE_INVERT_SIGN) == NUM_STYLE_INVERT_SIGN)
		((DataDbl &)aValue).Assign(-GetValue());
}

//-----------------------------------------------------------------------------
void CDoubleEdit::SetValue(const DataObj& aValue)
{    
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));

	double aDblValue = (double) ((const DataDbl&)pData);
	if (aDblValue != 0.0 && (m_dwCtrlStyle & NUM_STYLE_INVERT_SIGN) == NUM_STYLE_INVERT_SIGN)
		aDblValue = -aDblValue;

	
	SetValue(aDblValue);


}

//-----------------------------------------------------------------------------
BOOL CDoubleEdit::IsValidDouble(double nValue)
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
CString CDoubleEdit::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	if (nIDP == DOUBLE_EDIT_OUT_RANGE)
		return cwsprintf(FormatMessage(nIDP), pszBadVal, (double) m_dMin, (double) m_dMax);
	
	return CParsedCtrl::FormatErrorMessage(nIDP, pszBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDoubleEdit::DoSpinScroll(UINT nSBCode)
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
BOOL CDoubleEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetSel();
	CString	strValue; CParsedCtrl::GetValue (strValue);

	CDblFormatter* pFormatter = GetFormatter();
	
	TCHAR chDecSep = pFormatter && !pFormatter->GetDecSeparator().IsEmpty()
					? pFormatter->GetDecSeparator()[0]
					: pFormatter ? 0 : DOT_CHAR;
	TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
					? pFormatter->Get1000Separator()[0]
					: 0;

	CDblFormatter::RoundingTag  nCurRounding = 
		pFormatter 
			?pFormatter->SetRounding(CDblFormatter::ROUND_NONE) 
			: CDblFormatter::ROUND_NONE;

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

			SetSel(nPos, nPos);

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
BOOL CDoubleEdit::DoKeyDown(UINT nChar)
{       
	if (CParsedEdit::DoKeyDown(nChar))
		return TRUE;

	// this virtual key by-pass the OnChar management
	// check if a decimal point has been deleted
	if (nChar == VK_DELETE)
	{
		CString	strValue;
		CParsedCtrl::GetValue (strValue);

		DWORD dwPos = GetSel();
		if (LOWORD(dwPos) == HIWORD(dwPos))
		{
			int nPos = (int) LOWORD(dwPos);
			if (nPos == strValue.GetLength())
				return TRUE;

			CDblFormatter* pFormatter = GetFormatter();

			TCHAR chDecSep = pFormatter && !pFormatter->GetDecSeparator().IsEmpty()
							? pFormatter->GetDecSeparator()[0]
							: pFormatter ? 0 : DOT_CHAR;

			if (strValue[nPos] == chDecSep)
			{
				SetSel(nPos + 1, nPos + 1);
				return TRUE;
			}

			TCHAR ch1000Sep = pFormatter && !pFormatter->Get1000Separator().IsEmpty()
							? pFormatter->Get1000Separator()[0]
							: 0;
			SetSel(nPos, nPos + (strValue[nPos] == ch1000Sep ? 2 : 1));
		}

		if (DoOnChar(VK_BACK))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CDoubleEdit::DoKillFocus (CWnd* pWnd)
{
	if (GetModifyFlag()  && IsValid())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedEdit::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CDoubleEdit::OnInitCtrl()
{
	m_nCtrlLimit = 2*DBL_DIG+1;

	ModifyStyle(0,ES_RIGHT, SWP_FRAMECHANGED);

	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CDoubleEdit::FormatValue(DataDbl& aValue)
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
LRESULT CDoubleEdit::OnFormatStyleChange (WPARAM  /*wParam*/ , LPARAM  /*lParam*/ )
{
	DataDbl	aValue;
	return FormatValue(aValue);	
}

//-----------------------------------------------------------------------------
CString CDoubleEdit::FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const
{
	ASSERT_VALID(pDataObj);
	CString strCell;

	// Viene formattato il dato
	CDblFormatter* pFormatter = GetFormatter();

	if (pFormatter)
	{
		//se il programmatore ha scelto un numero di decimali diverso da quello del formattatore
		int nOldDN;
		if (m_pExternalNumDec || m_nDec >= 0)
			nOldDN = pFormatter->SetDecNumber(const_cast<CDoubleEdit*>(this)->GetCtrlNumDec());

		pFormatter->FormatDataObj(*pDataObj, strCell, bEnablePadding);

		if (m_pExternalNumDec || m_nDec >= 0)
			pFormatter->SetDecNumber(nOldDN);
	}
	else
		return pDataObj->Str();

	return strCell;
}

//=============================================================================
//			Class CMoneyEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMoneyEdit, CDoubleEdit)

BEGIN_MESSAGE_MAP(CMoneyEdit, CDoubleEdit)
	//{{AFX_MSG_MAP(CMoneyEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMoneyEdit::CMoneyEdit()
	:
	CDoubleEdit()
{}

//-----------------------------------------------------------------------------
CMoneyEdit::CMoneyEdit(UINT nBtnIDBmp, DataMon* pData)
	:
	CDoubleEdit(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CMoneyEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleEdit::SetValue(aValue);
}

//-----------------------------------------------------------------------------
void CMoneyEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CDoubleEdit::GetValue(aValue);
}


//-----------------------------------------------------------------------------
LRESULT CMoneyEdit::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataMon aValue;
	return FormatValue(aValue);
}

//=============================================================================
//			Class CQuantityEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CQuantityEdit, CDoubleEdit)

BEGIN_MESSAGE_MAP(CQuantityEdit, CDoubleEdit)
	//{{AFX_MSG_MAP(CQuantityEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CQuantityEdit::CQuantityEdit()
	:
	CDoubleEdit()
{}

//-----------------------------------------------------------------------------
CQuantityEdit::CQuantityEdit(UINT nBtnIDBmp, DataQty* pData)
	:
	CDoubleEdit(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CQuantityEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleEdit::SetValue((double) ((DataQty &)aValue));
}

//-----------------------------------------------------------------------------
void CQuantityEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	((DataQty &)aValue).Assign( CDoubleEdit::GetValue() );
}

//-----------------------------------------------------------------------------
LRESULT CQuantityEdit::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataQty aValue;
	return FormatValue(aValue);
}


//=============================================================================
//			Class CPercEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPercEdit, CDoubleEdit)

BEGIN_MESSAGE_MAP(CPercEdit, CDoubleEdit)
	//{{AFX_MSG_MAP(CPercEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPercEdit::CPercEdit()
	:
	CDoubleEdit()
{
	m_dMin = -100.;
	m_dMax = 100.;
}

//-----------------------------------------------------------------------------
CPercEdit::CPercEdit(UINT nBtnIDBmp, DataPerc* pData)
	:
	CDoubleEdit(nBtnIDBmp, pData )
{
	m_dMin = -100.;
	m_dMax = 100.;

	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CPercEdit::SetRange(double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;
		
	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}

//-----------------------------------------------------------------------------
void CPercEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleEdit::SetValue((double) ((DataPerc &)aValue));
}

//-----------------------------------------------------------------------------
void CPercEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataPerc &)aValue).Assign( CDoubleEdit::GetValue() );
}

//-----------------------------------------------------------------------------
LRESULT CPercEdit::OnFormatStyleChange (WPARAM  wParam , LPARAM  lParam )
{
	DataPerc aValue;
	return 	FormatValue(aValue);
}

//=============================================================================
//			Class CMetricEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMetricEdit, CDoubleEdit)

static const TCHAR BASED_CODE szIntl 		[]=_T("intl");
static const TCHAR BASED_CODE szIMeasure	[]=_T("intl");

//-----------------------------------------------------------------------------
CMetricEdit::CMetricEdit()
	:
	CDoubleEdit		(),
	m_nScale		(DEFAULT_SCALING)
{                                          
	m_UserMeasureUnits	= (MeasureUnits) CM;
}

//-----------------------------------------------------------------------------
CMetricEdit::CMetricEdit(UINT nBtnIDBmp)
	:
	CDoubleEdit		(nBtnIDBmp),
	m_nScale		(DEFAULT_SCALING)
{                                          
	m_UserMeasureUnits	= (MeasureUnits) CM;
}

//-----------------------------------------------------------------------------
BOOL CMetricEdit::OnInitCtrl()
{
	VERIFY(CDoubleEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CMetricEdit::SetMeasureUnits(double nScale, MeasureUnits aMu)
{                          
	if (aMu != STD_MU)                       
		m_UserMeasureUnits = aMu;
		
	m_nScale = nScale;

	if (m_pCaption)	SetCaption(T_DEFAULT);
}

//-----------------------------------------------------------------------------
void CMetricEdit::SetRange(int nMin, int nMax, int nDec, double nScale)
{    
	if (nMin > nMax)
		nMax = nMin;

	m_nScale = nScale;
	CDoubleEdit::SetRange	
		(
			LPtoMU(nMin, m_UserMeasureUnits, nScale, nDec),
			LPtoMU(nMax, m_UserMeasureUnits, nScale, nDec),
			nDec
		);

	if (m_pCaption)	SetCaption(T_DEFAULT);
}

//-----------------------------------------------------------------------------
void CMetricEdit::SetValue(int nValue)
{
	CDoubleEdit::SetValue(LPtoMU(nValue, m_UserMeasureUnits, m_nScale, GetCtrlNumDec()));
}

//-----------------------------------------------------------------------------
int CMetricEdit::GetValue()
{                                                                  
	return MUtoLP(CDoubleEdit::GetValue(), m_UserMeasureUnits, m_nScale, GetCtrlNumDec());
}

//------------------------------------------------------------------------------
CString CMetricEdit::GetSpecialCaption()
{
	switch(m_UserMeasureUnits)
	{
		case CM :
		{
			if (m_nScale == 1.) return _T("(cm.)");
			else if (m_nScale == 10.) return _T("(mm.)");
				else return cwsprintf(_T("(cm./%f)"), m_nScale);
		}
		case INCH :
		{
			if (m_nScale == 1.) return _T("(\")");
			else return cwsprintf(_T("(\"/%f)"), m_nScale);
		}                                
	}

	return _T("");
}


//=============================================================================
//			Class CDateEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CDateEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CDateEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	ON_MESSAGE	(UM_GET_PARSEDCTRL_TYPE, OnGetParsedCtrlType)
	ON_MESSAGE	(UM_RANGE_SELECTOR_SELECTED, OnRangeSelectorSelected)
	ON_MESSAGE	(UM_RANGE_SELECTOR_CLOSED, OnRangeSelectorClosed)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateEdit::CDateEdit()
	:
	CParsedEdit	(),
	m_lMin		(MIN_GIULIAN_DATE),
	m_lMax		(MAX_GIULIAN_DATE),
	m_nCurDate	(0),
	m_nOriginalDate(-1)
{}

//-----------------------------------------------------------------------------
CDateEdit::CDateEdit(UINT nBtnIDBmp, DataDate* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData),
	m_lMin		(MIN_GIULIAN_DATE),
	m_lMax		(MAX_GIULIAN_DATE),
	m_nCurDate	(0),
	m_nOriginalDate(-1)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || !pData->IsATime());
}

//-----------------------------------------------------------------------------
void CDateEdit::Attach (UINT /*nBtnID*/)
{ 
	m_nButtonIDBmp = BTN_CALENDAR_ID; 
}

//-----------------------------------------------------------------------------
void CDateEdit::SetRange(int nMin, int nMax)
{                 
	if (nMin < MIN_GIULIAN_DATE) nMin = MIN_GIULIAN_DATE;
	if (nMax > MAX_GIULIAN_DATE) nMax = MAX_GIULIAN_DATE;
	
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = (long)nMin;
	m_lMax = (long)nMax;
}

//-----------------------------------------------------------------------------
void CDateEdit::SetRange(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax)
{
	long nMin = ::GetGiulianDate(dMin, mMin, yMin);
	long nMax = ::GetGiulianDate(dMax, mMax, yMax);

	SetRange(nMin, nMax);
}

//-----------------------------------------------------------------------------
void CDateEdit::SetValue(long nValue)
{
	m_nCurDate = nValue;

    WORD nDay, nMonth, nYear;          
    ::GetShortDate(nDay, nMonth, nYear, nValue);
	
	SetNewFormatIdx();

	CParsedCtrl::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
}
    
//-----------------------------------------------------------------------------
void CDateEdit::SetValue(WORD nDay, WORD nMonth, WORD nYear)
{
	m_nCurDate = ::GetGiulianDate(nDay, nMonth, nYear);

	SetNewFormatIdx();

	CParsedCtrl::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx, m_pFormatContext));
}

//-----------------------------------------------------------------------------
long CDateEdit::GetValue()
{                          
	DBTIMESTAMP aDateTime;
    CString strDate;
             
	CParsedCtrl::GetValue(strDate);
	if (::GetTimeStamp(aDateTime, strDate, m_nFormatIdx, m_pFormatContext))
		return ::GetGiulianDate(aDateTime);

	return BAD_DATE;
}

//-----------------------------------------------------------------------------
BOOL CDateEdit::OnInitCtrl()
{
	m_nCtrlLimit = GetInputCharLen();

	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDateEdit::SetValue(const DataObj& aValue)
{             
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));
	SetValue((long) ((DataDate &)pData));
	
}

//-----------------------------------------------------------------------------
void CDateEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataDate &)aValue).SetDate(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CDateEdit::IsValidDate(long nValue)
{                                                                    
    // bad value 
    if (nValue == BAD_DATE)
    {
		m_nErrorID = DATE_EDIT_BAD_FORMAT;
		return FALSE;
    }

	CDateFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	// bad range
	if (nValue != 0L && (nValue < (long)m_lMin || nValue > (long)m_lMax))
	{
		m_nErrorID = DATE_EDIT_OUT_RANGE;
		return FALSE;
	}
    
	if ((m_dwCtrlStyle & STR_STYLE_NO_EMPTY) == STR_STYLE_NO_EMPTY)
	{
		if (nValue == 0)
		{
			m_nErrorID = FIELD_EMPTY;
			return FALSE;
		}
	}
    // if good date hold new value
	m_nCurDate = nValue;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CDateEdit::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	switch (nIDP)	              
	{                                 
		case DATE_EDIT_BAD_FORMAT :
			return cwsprintf(FormatMessage(nIDP), pszBadVal, (LPCTSTR) GetDateTimeTemplate());
				
		case DATE_EDIT_OUT_RANGE :
		{
		    WORD nDay, nMonth, nYear;          
		
		    ::GetShortDate(nDay, nMonth, nYear, m_lMin);
			CString dMin(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
			
		    ::GetShortDate(nDay, nMonth, nYear, m_lMax);
			CString dMax(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
			
			return cwsprintf(FormatMessage(nIDP), pszBadVal, (LPCTSTR) dMin, (LPCTSTR) dMax);
		}
	}
	return CParsedCtrl::FormatErrorMessage(nIDP, pszBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDateEdit::DoSpinScroll(UINT nSBCode)
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
	long l = m_lMax;
	if ((nOld < (long)m_lMin && nDelta < 0) || (nOld > (long)m_lMax && nDelta > 0))
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
BOOL CDateEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
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

	CParsedCtrl::GetValue(strBuffer);
	GetSel(nStartChar, nPosChar);
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
		if (nSepIdx != FIRST_DATE_SEP && nSepIdx != SECOND_DATE_SEP && nSepIdx != DATE_TIME_SEP)
			return FALSE;

		strSeparator += (TCHAR) nChar;
	}

	CParsedCtrl::SetValue(strBuffer.Left(nPosChar) + strSeparator + strBuffer.Mid(nPosChar));
	nPosChar += strSeparator.GetLength();
	SetSel(nPosChar, nPosChar);

	SetModifyFlag(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CDateEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
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
		
	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CDateEdit::OnGetParsedCtrlType(WPARAM /*wParam*/, LPARAM /*lParam*/)
{
	return 1L;
}

//Struttura usata per leggere le date da una zona di memoria, scritta dal RangeSelector 
//TENERE ALLINEATA con \TaskBuilderNet\Microarea.TaskBuilderNet.Core\Generic\DateRangeSelector.cs	
typedef struct {
	WORD day;
	WORD month;
	WORD year;
} DateSelection;

//Metodo che sulla chiusura della finestra di selezione del range, imposta la data che gli e' stata inviata 
//con una SendMessage dalla finestra stessa
//-----------------------------------------------------------------------------
LRESULT CDateEdit::OnRangeSelectorSelected(WPARAM wParam, LPARAM lParam)
{
	//Se non l'ho gia' fatto metto da parte la data originale
	if (m_nOriginalDate == -1)
		m_nOriginalDate = GetValue();

	DateSelection* sel = (DateSelection*)wParam;	
	DataDate dd(sel->day, sel->month, sel->year);
	//libera la memoria allocata dalla finestra c# per il passaggio della data
	::CoTaskMemFree(sel);

	if (GetCtrlData() && GetCtrlData()->IsFullDate())	
		dd.SetFullDate();
	
	SetValue(dd);

	//lParam = 1 : selection
	//lParam = 0 : preview
	//In caso di selezione devo aggiornare il dataobj che sta dietro il parsed control, e dare il fuoco
	//al parsed control stesso in modo che si chiuda la finestra popup managed
	//In caso di preview solo il parsed control ()
	if (lParam == 1)
	{
		SetModifyFlag(TRUE);
		UpdateCtrlData(TRUE);
	}
	
	return 1L;
}

//Sulla chiusura della finestra di selezione del range distrugge il puntatore al wrapper del
//SelectorManager
//-----------------------------------------------------------------------------
LRESULT CDateEdit::OnRangeSelectorClosed(WPARAM wParam, LPARAM lParam)
{
	//wParam = 1 Range selector si e' chiuso per la selezione di una data
	//wParam = 0 Range selector si e' chiuso senza selezione di data, ripristino il valore originale
	//(quello prima dell'apertura del range selector)
	if (wParam == 0 && m_nOriginalDate != -1 && m_nOriginalDate != GetValue() )
	{
		SetValue(m_nOriginalDate);
	}

	//resetto il valore che mi ero tenuto da parte in fase di selzione in anteprima delle date
	m_nOriginalDate = -1;

	CCalendarButton* pCalButton = (CCalendarButton*)GetButton();
	if (pCalButton )
	{
		SAFE_DELETE(pCalButton->m_pManagedWndWrapper);
	}

	SetFocus();
	return 1L;
}


//reimplementato per evitare che perda il fuoco sull'apertura del calendario
//-----------------------------------------------------------------------------
void CDateEdit::DoKillFocus (CWnd* pWnd)
{
	CCalendarButton* pCalButton = (CCalendarButton*)GetButton();
	if (pCalButton && pCalButton->m_pManagedWndWrapper)
		return;

	__super::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CDateEdit::GetToolTipProperties (CTooltipProperties& tp)
{ 
	if (IsKindOf(RUNTIME_CLASS(CTimeEdit)))
		return FALSE;
	DataDate date;
	if (IsKindOf(RUNTIME_CLASS(CDateTimeEdit)))
		date.SetFullDate();

	GetValue(date);
	if (!date.IsEmpty())
	{
		tp.m_strText = cwsprintf(_TB("{0-%s}; Week: {1-%d}"), (LPCTSTR)date.WeekDayName(), date.WeekOfYear());
		return TRUE;
	}
	return FALSE; 
}

//=============================================================================
//			Class CDateSpinEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateSpinEdit, CDateEdit)

BEGIN_MESSAGE_MAP(CDateSpinEdit, CDateEdit)
	//{{AFX_MSG_MAP(CDateSpinEdit)
	ON_WM_ENABLE()
	ON_MESSAGE	(WM_SETTEXT, OnSetTimeText)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateSpinEdit::CDateSpinEdit()
	:
	m_pDateSpin	(NULL),
	m_nStartDate(0)
{
}

//-----------------------------------------------------------------------------
CDateSpinEdit::~CDateSpinEdit()
{
	if (m_pDateSpin)
		delete m_pDateSpin;
}

//-----------------------------------------------------------------------------
BOOL CDateSpinEdit::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	if (CParsedEdit::SubclassEdit(nID, pParentWnd, strName))
		AttachSpin();

	return (m_pDateSpin != NULL);
}

//-----------------------------------------------------------------------------
CDateSpin* CDateSpinEdit::AttachSpin(UINT nSpinID /* = IDC_STATIC*/)
{
	CRect EditRect;
	GetClientRect(EditRect);

	m_pDateSpin = new CDateSpin;
    if	(
			!m_pDateSpin->Create
			(
				this, 
				CRect
				(
					EditRect.right - TIME_SPIN_DEFAULT_WIDTH,
					EditRect.top,
					EditRect.right,
					EditRect.bottom
				),
				nSpinID
			)
		)
	{
		delete m_pDateSpin;
		m_pDateSpin = NULL;
		return NULL;
	}
	CRect SpinRect;
	m_pDateSpin->GetClientRect(SpinRect);
	m_pDateSpin->SetWindowPos
							(
								this,
								EditRect.right - SpinRect.Width(), 
								EditRect.top,
								SpinRect.Width(),
								EditRect.Height(),
								SWP_SHOWWINDOW
							);
	
	m_pDateSpin->SetBuddy(this);

	return m_pDateSpin;
}

//-----------------------------------------------------------------------------
void CDateSpinEdit::OnEnable(BOOL bEnable)
{
	if (m_pDateSpin)
		m_pDateSpin->ShowWindow(bEnable ? SW_SHOW : SW_HIDE);
	
	//CDateEdit::OnEnable(bEnable);
	CBCGPEdit::OnEnable(bEnable);
	
	DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
LRESULT CDateSpinEdit::OnSetTimeText(WPARAM wParam, LPARAM lParam)
{
	ASSERT(lParam == 0 || AfxIsValidString((LPCTSTR)lParam));
	CString strDateText = (LPCTSTR)lParam;

	CDateFormatter* pDateFormatter = (CDateFormatter*)AfxGetFormatStyleTable()->GetFormatter(DataType::Date, m_pFormatContext);
	CString strDateFirstSep = pDateFormatter->GetFirstSeparator();
	if (m_pDateSpin && !strDateFirstSep.IsEmpty())
	{
		int nSepPos = strDateText.Find(strDateFirstSep);
		if (nSepPos == -1)
		{
			int nDays = _tstoi(strDateText);
			if (nDays == UD_MINVAL)
			{
				m_nStartDate += UD_MINVAL;
				nDays = 0;
				m_pDateSpin->SetPos(0);
			}
			if (nDays == UD_MAXVAL)
			{
				m_nStartDate += UD_MAXVAL;
				nDays = 0;
				m_pDateSpin->SetPos(0);
			}


			DataDate CurrentDate;
			CurrentDate.SetDate(m_nStartDate + nDays);
			
			DataDate aDate (CurrentDate.GetDateTime());
			pDateFormatter->FormatDataObj(aDate, strDateText);
		}
	}
	
	SetModifyFlag(TRUE);
	
	return DefWindowProc(WM_SETTEXT, wParam, (LPARAM)((LPCTSTR)strDateText));
}

//-----------------------------------------------------------------------------
void CDateSpinEdit::SetValue(const DataObj& aValue)
{
	ASSERT(aValue.GetDataType() == GetDataType());
	m_nStartDate = ((DataDate &)aValue).GiulianDate();
	if(m_pDateSpin)
	{
		m_pDateSpin->SetRange(UD_MINVAL, UD_MAXVAL);
	}
	__super::SetValue(aValue);
}

//-----------------------------------------------------------------------------
void CDateSpinEdit::DoKillFocus (CWnd* pWnd)
{
	CDateEdit::DoKillFocus(pWnd);

	m_nStartDate = m_nCurDate;
	if(m_pDateSpin)
		m_pDateSpin->SetPos(0);
}

//-----------------------------------------------------------------------------
void CDateSpinEdit::UpdateStartDate()
{
	m_nStartDate = GetValue();
	if(m_pDateSpin)
		m_pDateSpin->SetPos(0);
}

//=============================================================================
//			Class CDateSpin implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateSpin, CSpinButtonCtrl)

BEGIN_MESSAGE_MAP(CDateSpin, CSpinButtonCtrl)
	//{{AFX_MSG_MAP(CDateSpin)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateSpin::CDateSpin()
	:
	m_pDateEdit	(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL CDateSpin::Create(CDateSpinEdit* pDateEdit, const RECT& rect, UINT nID)
{
	ASSERT(pDateEdit);
	m_pDateEdit = pDateEdit;
	
	DWORD dwStyle = WS_CHILD | WS_VISIBLE;
	dwStyle |= UDS_SETBUDDYINT | UDS_ALIGNRIGHT | UDS_ARROWKEYS | UDS_NOTHOUSANDS | UDS_WRAP;

	return CSpinButtonCtrl::Create(dwStyle, rect, pDateEdit->GetParent(), nID);
}

//-----------------------------------------------------------------------------
void CDateSpin::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (m_pDateEdit && m_pDateEdit->GetModifyFlag())
		m_pDateEdit->UpdateStartDate();
	CSpinButtonCtrl::OnLButtonDown(nFlags, point);
}

//=============================================================================
//			Class CDateTimeEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateTimeEdit, CDateEdit)

BEGIN_MESSAGE_MAP(CDateTimeEdit, CDateEdit)
	//{{AFX_MSG_MAP(CDateTimeEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateTimeEdit::CDateTimeEdit()
	:
	CDateEdit	(),
	m_nCurTime	(0)
{}

//-----------------------------------------------------------------------------
CDateTimeEdit::CDateTimeEdit(UINT nBtnIDBmp, DataDate* pData)
	:
	CDateEdit	(nBtnIDBmp, pData),
	m_nCurTime	(0)
{
	// NON si puo` usare il metodo CDateEdit(nBtnIDBmp, pData) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
void CDateTimeEdit::SetValue(const DataObj& aValue)
{             
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));
	SetValue(((DataDate &)pData).GetDateTime());

}

//-----------------------------------------------------------------------------
void CDateTimeEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataDate &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
void CDateTimeEdit::SetValue(const DBTIMESTAMP& aDateTime)
{
	CDateFormatter* pFormatter = NULL;

	SetNewFormatIdx();

	if (m_nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	CString str; 
	if (pFormatter)
		pFormatter->Format(&aDateTime, str, FALSE);

	m_nCurDate = ::GetGiulianDate(aDateTime);
	m_nCurTime = ::GetTotalSeconds(aDateTime);
	CParsedCtrl::SetValue(str);
}

//-----------------------------------------------------------------------------
DBTIMESTAMP CDateTimeEdit::GetValue()
{                          
	DBTIMESTAMP aDateTime;
    CString strDate;
             
	CParsedCtrl::GetValue(strDate);

	// se non e` valida GetTimeStamp valorizza aDateTime con dei valori BAD
	::GetTimeStamp(aDateTime, strDate, m_nFormatIdx, m_pFormatContext);

	return aDateTime;
}

//-----------------------------------------------------------------------------
BOOL CDateTimeEdit::OnInitCtrl()
{
	m_nCtrlLimit = GetInputCharLen();

	VERIFY(CParsedEdit::OnInitCtrl());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDateTimeEdit::IsValidDateTime(const DBTIMESTAMP& aDateTime)
{
	long nCurDate = m_nCurDate;
 	if (!IsValidDate(GetGiulianDate(aDateTime)))
		return FALSE;

	long nTime = GetTotalSeconds(aDateTime);
	if (nTime == BAD_TIME || m_nCurDate == 0 && nTime > 0)
	{
		// ripristina il valore precedente
		m_nCurDate = nCurDate;

		// bad value 
		m_nErrorID = DATE_EDIT_BAD_FORMAT;
		return FALSE;
	}

	m_nCurTime = nTime;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDateTimeEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar))
		return FALSE;

	int		nStartChar	= 0;
	int		nPosChar	= 0;
	CString strBuffer;

	CDateFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter == NULL)
		return FALSE;

	CParsedCtrl::GetValue(strBuffer);
	GetSel(nStartChar, nPosChar);
	if (nStartChar != nPosChar)
	{
		strBuffer = strBuffer.Left(nStartChar) + strBuffer.Mid(nPosChar);
		nPosChar = nStartChar;
	}

	if ((pFormatter->GetTimeFormat() & CDateFormatHelper::TIME_ONLY) != CDateFormatHelper::TIME_ONLY)
	{
		int nSepDT = FindDateSepPos(DATE_TIME_SEP, strBuffer, pFormatter);
		if (nSepDT < 0 || nPosChar <= nSepDT)
			return CDateEdit::DoOnChar(nChar);
	}

	CString strSeparator;
		
	if (!_istdigit(nChar))
	{
		if (nPosChar == 0 || !_istdigit(strBuffer[nPosChar - 1]))
		{
			BadInput();
			return TRUE;
		}

		// si verifica se ci sono gia` separtori 
		//
		int		nSepIdx = DateSepPermitted(strBuffer, pFormatter, strSeparator);
		CString	strChar((TCHAR)nChar);
		if (nSepIdx == AMPM_TIME_SEP)
			if (_tcsicmp(strChar, CString(pFormatter->GetTimeAMString()[0])) == 0)
				strSeparator += pFormatter->GetTimeAMString();
			else
				if (_tcsicmp(strChar, CString(pFormatter->GetTimePMString()[0])) == 0)
					strSeparator += pFormatter->GetTimePMString();
				else
					nSepIdx = NO_MORE_SEP;

		if (nSepIdx == NO_MORE_SEP)
		{
			BadInput();
			return TRUE;
		}
	}
	else
	{
		if (nPosChar < 2)
			return FALSE;

		// potrei mettere un separatore (ogni nOffset cifre) ?
		for (int i = nPosChar - 2; i < nPosChar; i++)
			if (!_istdigit(strBuffer[i]))
				return FALSE;

		// il carattere prima delle 2 cifre e` un separatore ?
		if (nPosChar > 2 && _istdigit(strBuffer[nPosChar - 3]))
			return FALSE;

		// servono ulteriori separtori ?
		int nSepIdx = DateSepPermitted(strBuffer, pFormatter, strSeparator);
		if (nSepIdx == NO_MORE_SEP || nSepIdx == AMPM_TIME_SEP)
		{
			BadInput();
			return TRUE;
		}

		strSeparator += (TCHAR) nChar;
	}

	CParsedCtrl::SetValue(strBuffer.Left(nPosChar) + strSeparator + strBuffer.Mid(nPosChar));
	nPosChar += strSeparator.GetLength();
	SetSel(nPosChar, nPosChar);

	SetModifyFlag(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CDateTimeEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	DataDate aDataDate;
	aDataDate.SetFullDate();

	BOOL bModify = GetModifyFlag();

	if (m_nCurDate || m_nCurTime)
		aDataDate.Assign(m_nCurDate, m_nCurTime);
	else
		aDataDate.Assign(GetValue());
	
	if (IsValid(aDataDate))
	{
		SetValue(aDataDate);
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
//			Class CTimeEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTimeEdit, CDateTimeEdit)

BEGIN_MESSAGE_MAP(CTimeEdit, CDateTimeEdit)
	//{{AFX_MSG_MAP(CTimeEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTimeEdit::CTimeEdit()
	:
	CDateTimeEdit	()
{}

//-----------------------------------------------------------------------------
CTimeEdit::CTimeEdit(UINT nBtnIDBmp, DataDate* pData)
	:
	CDateTimeEdit	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CDateEdit(nBtnIDBmp, pData) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->IsFullDate());
}

//-----------------------------------------------------------------------------
void CTimeEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	DBTIMESTAMP aDateTime = GetValue();
	((DataDate &)aValue).SetTime(aDateTime.hour, aDateTime.minute, aDateTime.second);
}

//-----------------------------------------------------------------------------
DBTIMESTAMP CTimeEdit::GetValue()
{                          
	DBTIMESTAMP aDateTime = CDateTimeEdit::GetValue();

	// se al control e` associato un DataDateTime (quindi e` un FullDate, come
	// DEVE essere il DataDate associato a questo tipo di control, ma NON gestisce
	// solamente l'Ora) valorizza la parte Data con il valore della data del DataDate
	if (GetCtrlData() && !GetCtrlData()->IsATime())
	{
		aDateTime.day	= ((DataDate*) GetCtrlData())->Day();
		aDateTime.month	= ((DataDate*) GetCtrlData())->Month();
		aDateTime.year	= ((DataDate*) GetCtrlData())->Year();
	}

	return aDateTime;
}

//-----------------------------------------------------------------------------
LRESULT CTimeEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{

	DataDate aDataDate;
	aDataDate.SetAsTime();

	BOOL bModify = GetModifyFlag();

	if (m_nCurTime)
		aDataDate.SetTime(m_nCurTime);
	else
		aDataDate.Assign(GetValue());
	
	if (IsValid(aDataDate))
	{
		SetValue(aDataDate);
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
//			Class CTimeSpinEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTimeSpinEdit, CTimeEdit)

BEGIN_MESSAGE_MAP(CTimeSpinEdit, CTimeEdit)
	//{{AFX_MSG_MAP(CTimeSpinEdit)
	ON_WM_ENABLE()
	ON_MESSAGE	(WM_SETTEXT, OnSetTimeText)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTimeSpinEdit::CTimeSpinEdit()
	:
	m_pTimeSpin	(NULL)
{
}

//-----------------------------------------------------------------------------
CTimeSpinEdit::~CTimeSpinEdit()
{
	if (m_pTimeSpin)
		delete m_pTimeSpin;
}

//-----------------------------------------------------------------------------
BOOL CTimeSpinEdit::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	if (CParsedEdit::SubclassEdit(nID, pParentWnd, strName))
		AttachSpin();

	return (m_pTimeSpin != NULL);
}

//-----------------------------------------------------------------------------
CTimeSpin* CTimeSpinEdit::AttachSpin(UINT nSpinID /* = IDC_STATIC*/)
{
	CRect EditRect;
	GetClientRect(EditRect);

	m_pTimeSpin = new CTimeSpin;
    if	(
			!m_pTimeSpin->Create
			(
				this,
				CRect
				(
					EditRect.right - TIME_SPIN_DEFAULT_WIDTH,
					EditRect.top,
					EditRect.right,
					EditRect.bottom
				),
				nSpinID
			)
		)
	{
		delete m_pTimeSpin;
		m_pTimeSpin = NULL;
		return NULL;
	}
	CRect SpinRect;
	m_pTimeSpin->GetClientRect(SpinRect);
	m_pTimeSpin->SetWindowPos
							(
								this,
								EditRect.right - SpinRect.Width(), 
								EditRect.top,
								SpinRect.Width(),
								EditRect.Height(),
								SWP_SHOWWINDOW
							);
	
	m_pTimeSpin->SetRange(0, 1440 - 1); // 1440 = numero di minuti in un giorno
	m_pTimeSpin->SetBuddy(this);

	return m_pTimeSpin;
}

//-----------------------------------------------------------------------------
void CTimeSpinEdit::OnEnable(BOOL bEnable)
{
	if(m_pTimeSpin)
		m_pTimeSpin->ShowWindow(bEnable ? SW_SHOW : SW_HIDE);
	
	//CDateTimeEdit::OnEnable(bEnable);
	CBCGPEdit::OnEnable(bEnable);
	
	DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
LRESULT CTimeSpinEdit::OnSetTimeText(WPARAM wParam, LPARAM lParam)
{
	ASSERT(lParam == 0 || AfxIsValidString((LPCTSTR)lParam));

	CString strTimeText = (LPCTSTR)lParam;
	CDateFormatter* pTimeFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(DataType::Time, m_pFormatContext);
	CString strTimeSep = pTimeFormatter->GetTimeSeparator();
	if (m_pTimeSpin && !strTimeSep.IsEmpty())
	{
		int nSepPos = strTimeText.Find(strTimeSep);
		if (nSepPos == -1)
		{
			int nMinutes = _tstoi(strTimeText);
			DataDate Time;
			Time.SetAsTime();
			Time.SetTime(nMinutes * 60);
			
			DataDate aTime(Time.GetDateTime());
			pTimeFormatter->FormatDataObj(aTime, strTimeText);
		}
	}
	return DefWindowProc(WM_SETTEXT, wParam, (LPARAM)((LPCTSTR)strTimeText));
}

//-----------------------------------------------------------------------------
void CTimeSpinEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(aValue.GetDataType() == GetDataType());
	if(m_pTimeSpin)
		m_pTimeSpin->SetPos(((DataDate &)aValue).Minute() + ((DataDate &)aValue).Hour() * 60);
	CDateTimeEdit::SetValue(aValue);
}

//-----------------------------------------------------------------------------
void CTimeSpinEdit::DoKillFocus (CWnd* pWnd)
{
	CTimeEdit::DoKillFocus(pWnd);

	if(!m_pTimeSpin)
		return;

	m_pTimeSpin->SetPos(m_nCurTime/60);
}

//=============================================================================
//			Class CTimeSpin implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTimeSpin, CSpinButtonCtrl)

BEGIN_MESSAGE_MAP(CTimeSpin, CSpinButtonCtrl)
	//{{AFX_MSG_MAP(CTimeSpin)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTimeSpin::CTimeSpin()
	:
	m_pTimeEdit	(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL CTimeSpin::Create(CTimeSpinEdit* pTimeEdit, const RECT& rect, UINT nID)
{
	ASSERT(pTimeEdit);
	m_pTimeEdit = pTimeEdit;
	
	DWORD dwStyle = WS_CHILD | WS_VISIBLE;
	dwStyle |= UDS_SETBUDDYINT | UDS_ALIGNRIGHT | UDS_ARROWKEYS | UDS_NOTHOUSANDS | UDS_WRAP;

	return CSpinButtonCtrl::Create(dwStyle, rect, pTimeEdit->GetParent(), nID);
}

//-----------------------------------------------------------------------------
void CTimeSpin::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (m_pTimeEdit && m_pTimeEdit->GetModifyFlag())
	{
		int nCurrentTime = ((DataDate)m_pTimeEdit->GetValue()).Minute() + + ((DataDate)m_pTimeEdit->GetValue()).Hour() * 60;
		if (nCurrentTime != LOWORD(GetPos()))
			SetPos(nCurrentTime);
	}	
	CSpinButtonCtrl::OnLButtonDown(nFlags, point);
}

//=============================================================================
//@@ElapsedTime			Class CElapsedTimeEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CElapsedTimeEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CElapsedTimeEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CDateEdit)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CElapsedTimeEdit::CElapsedTimeEdit()
	:
	CParsedEdit	(),
	m_lMin		(0),
	m_lMax		(LONG_MAX),
	m_nCurTime	(0)
{}

//-----------------------------------------------------------------------------
CElapsedTimeEdit::CElapsedTimeEdit(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData),
	m_lMin		(0),
	m_lMax		(LONG_MAX),
	m_nCurTime	(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
BOOL CElapsedTimeEdit::OnInitCtrl()
{
	m_nCtrlLimit = GetInputCharLen();

	VERIFY(CParsedEdit::OnInitCtrl());
	
	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter && pFormatter->GetCaptionPos() != 0)
		SetCaption((Token)pFormatter->GetCaptionPos(), _T(""), 1);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CElapsedTimeEdit::SetRange(int nMin, int nMax)
{                 
	if (nMin < 0) nMin = MIN_GIULIAN_DATE;
	if (nMax > LONG_MAX) nMax = LONG_MAX;
	
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = nMin;
	m_lMax = nMax;
}

//-----------------------------------------------------------------------------
void CElapsedTimeEdit::SetValue(long nValue)
{
	CElapsedTimeFormatter* pFormatter = NULL;

	SetNewFormatIdx();

	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	CString str; 
	if (pFormatter)
		pFormatter->Format(&nValue, str, FALSE);

	m_nCurTime = nValue;
	CParsedCtrl::SetValue(str);
}
    
//-----------------------------------------------------------------------------
long CElapsedTimeEdit::GetValue()
{                          
	DataLng aTime;
    CString strTime;
             
	CParsedCtrl::GetValue(strTime);
	if (::GetElapsedTime(aTime, strTime, m_nFormatIdx, m_pFormatContext))
		return (long)aTime;

	return BAD_TIME;
}

//-----------------------------------------------------------------------------
void CElapsedTimeEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataLng &)aValue));
}

//-----------------------------------------------------------------------------
void CElapsedTimeEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataLng &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CElapsedTimeEdit::IsValidTime(long nValue)
{                                                                    
    // bad value 
    if (nValue == BAD_TIME)
    {
		m_nErrorID = TIME_EDIT_BAD_FORMAT;
		return FALSE;
    }

	// bad range
	if (nValue != 0L && (nValue < m_lMin || nValue > m_lMax))
	{
		m_nErrorID = TIME_EDIT_OUT_RANGE;
		return FALSE;
	}
    
    // if good date hold new value
	m_nCurTime = nValue;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CElapsedTimeEdit::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	switch (nIDP)	              
	{                                 
		case TIME_EDIT_BAD_FORMAT :
			return cwsprintf(FormatMessage(nIDP), pszBadVal, (LPCTSTR) GetElapsedTimeTemplate());
				
		case TIME_EDIT_OUT_RANGE :
		{
			DataLng aVal(m_lMin); aVal.SetAsTime();
			CString dMin(FormatData(&aVal));

			aVal = m_lMax;
			CString dMax(FormatData(&aVal));

			return cwsprintf(FormatMessage(nIDP), pszBadVal, (LPCTSTR) dMin, (LPCTSTR) dMax);
		}
	}

	return CParsedCtrl::FormatErrorMessage(nIDP, pszBadVal);
}
	
// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CElapsedTimeEdit::DoSpinScroll(UINT nSBCode)
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

	if ((nOld < m_lMin && nDelta < 0) || (nOld > m_lMax && nDelta > 0))
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
BOOL CElapsedTimeEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar))
		return FALSE;

	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter == NULL)
		return FALSE;

	int		nStartChar	= 0;
	int		nPosChar	= 0;
	CString strBuffer;

	CParsedCtrl::GetValue(strBuffer);
	GetSel(nStartChar, nPosChar);
	if (nStartChar != nPosChar)
	{
		strBuffer = strBuffer.Left(nStartChar) + strBuffer.Mid(nPosChar);
		nPosChar = nStartChar;
	}

	CString strSeparator;
		
	if (!_istdigit(nChar))
	{
		if (nPosChar == 0 || !_istdigit(strBuffer[nPosChar - 1]))
		{
			BadInput();
			return TRUE;
		}

		// si verifica se ci sono gia` separtori 
		//
		if (TimeSepPermitted(strBuffer, pFormatter, strSeparator) == NO_MORE_SEP)
		{
			BadInput();
			return TRUE;
		}
	}
	else
	{
		int nLast = strBuffer.GetLength() - 1;
		int nSepIdx = TimeSepPermitted(strBuffer, pFormatter, strSeparator);
		int nStep = 2;
		int nFirstSegment = FMT_ELAPSED_TIME_D_LEN;
		switch (pFormatter->GetFormat() & CElapsedTimeFormatHelper::TIME_DHMS)
			{
				case CElapsedTimeFormatHelper::TIME_DHMS: 
				case CElapsedTimeFormatHelper::TIME_DHM: 
				case CElapsedTimeFormatHelper::TIME_DH: 
				case CElapsedTimeFormatHelper::TIME_D: 
					switch (DataLng::GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nFirstSegment = FMT_ELAPSED_TIME_D_LEN;		break;
						case PRECISON_DEC	: nFirstSegment = FMT_ELAPSED_TIME_D_LEN - 1;	break;
						case PRECISON_CENT	: nFirstSegment = FMT_ELAPSED_TIME_D_LEN - 2;	break;
						case PRECISON_MILL	: nFirstSegment = FMT_ELAPSED_TIME_D_LEN - 3;	break;
						default				: nFirstSegment = FMT_ELAPSED_TIME_D_LEN;		break;
					}
					break;

				case CElapsedTimeFormatHelper::TIME_HMS: 
				case CElapsedTimeFormatHelper::TIME_HM: 
				case CElapsedTimeFormatHelper::TIME_H: 
					switch (DataLng::GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nFirstSegment = FMT_ELAPSED_TIME_H_LEN;		break;
						case PRECISON_DEC	: nFirstSegment = FMT_ELAPSED_TIME_H_LEN - 1;	break;
						case PRECISON_CENT	: nFirstSegment = FMT_ELAPSED_TIME_H_LEN - 2;	break;
						case PRECISON_MILL	: nFirstSegment = FMT_ELAPSED_TIME_H_LEN - 3;	break;
						default				: nFirstSegment = FMT_ELAPSED_TIME_H_LEN;		break;
					}
					break;
				
				case CElapsedTimeFormatHelper::TIME_MSEC: 
				case CElapsedTimeFormatHelper::TIME_M: 
					switch (DataLng::GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nFirstSegment = FMT_ELAPSED_TIME_M_LEN;		break;
						case PRECISON_DEC	: nFirstSegment = FMT_ELAPSED_TIME_M_LEN - 1;	break;
						case PRECISON_CENT	: nFirstSegment = FMT_ELAPSED_TIME_M_LEN - 2;	break;
						case PRECISON_MILL	: nFirstSegment = FMT_ELAPSED_TIME_M_LEN - 3;	break;
						default				: nFirstSegment = FMT_ELAPSED_TIME_M_LEN;		break;
					}
					break;

				case CElapsedTimeFormatHelper::TIME_S: 
					switch (DataLng::GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nFirstSegment = FMT_ELAPSED_TIME_S_LEN;		break;
						case PRECISON_DEC	: nFirstSegment = FMT_ELAPSED_TIME_S_LEN - 1;	break;
						case PRECISON_CENT	: nFirstSegment = FMT_ELAPSED_TIME_S_LEN - 2;	break;
						case PRECISON_MILL	: nFirstSegment = FMT_ELAPSED_TIME_S_LEN - 3;	break;
						default				: nFirstSegment = FMT_ELAPSED_TIME_S_LEN;		break;
					}
					break;
			}

		if (nPosChar <= nFirstSegment && (nSepIdx == FIRST_TIME_SEP || nSepIdx == NO_MORE_SEP))
			nStep = nFirstSegment;
		else
			if	(
					(pFormatter->GetFormat() & CElapsedTimeFormatHelper::TIME_DEC) != 0 &&
					nSepIdx == NO_MORE_SEP
				)
				nStep = pFormatter->GetDecNumber();

		if (nPosChar < nStep)
			return FALSE;

		// potrei mettere un separatore (ogni nStep cifre) ?
		for (int i = nPosChar - nStep; i < nPosChar; i++)
			if (!_istdigit(strBuffer[i]))
			{
				if	(
						(
							nPosChar == nLast && _istdigit(strBuffer[nPosChar - 1]) ||
							nPosChar == nLast - 1 && _istdigit(strBuffer[nPosChar + 1])
						) &&
						_istdigit(strBuffer[nPosChar])
					)
				{
					BadInput();
					return TRUE;
				}

				return FALSE;
			}

		// il carattere prima delle nStep cifre e` un separatore ?
		if (nPosChar > nStep && _istdigit(strBuffer[nPosChar - nStep - 1]))
			return FALSE;

		if	(
				nSepIdx == NO_MORE_SEP ||
				(
					nPosChar == nLast - 1			&&
					_istdigit(strBuffer[nPosChar + 1])&&
					_istdigit(strBuffer[nPosChar])
				)
			)
		{
			BadInput();
			return TRUE;
		}

		strSeparator += (TCHAR) nChar;
	}

	CParsedCtrl::SetValue(strBuffer.Left(nPosChar) + strSeparator + strBuffer.Mid(nPosChar));
	nPosChar += strSeparator.GetLength();
	SetSel(nPosChar, nPosChar);

	SetModifyFlag(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CElapsedTimeEdit::DoKillFocus (CWnd* pWnd)
{
	if (GetModifyFlag() && IsValid())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);		
	}

	CParsedEdit::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
LRESULT CElapsedTimeEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	DataLng aTime;
	aTime.SetAsTime();

	BOOL bModify = GetModifyFlag();

	if (m_nCurTime)
		aTime.Assign(m_nCurTime);
	else
		aTime.Assign(GetValue());
	
	if (IsValid(aTime))
	{
		SetValue(aTime);
		if (bModify)
			SetModifyFlag(TRUE);
		SetCtrlMaxLen(GetInputCharLen(), TRUE);
	}
	else
	{
		ClearCtrl();
		SetModifyFlag(FALSE);
	}		
	
	if (m_pCaption)
	{
		CElapsedTimeFormatter* pFormatter = NULL;

		if (m_nFormatIdx >= 0)
			pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			SetCaption((Token)pFormatter->GetCaptionPos());
		else
			SetCaption(T_DEFAULT);
	}

	return 0L;
}

//------------------------------------------------------------------------------
CString CElapsedTimeEdit::GetSpecialCaption()
{
	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter == NULL)
		return _T("");

	return pFormatter->GetShortDescription();
}


//=============================================================================
//			Class CBoolEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CBoolEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CBoolEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CBoolEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBoolEdit::CBoolEdit()
	:
	CParsedEdit	()
{}
    
//-----------------------------------------------------------------------------
CBoolEdit::CBoolEdit(UINT nBtnIDBmp, DataBool* pData)
	:
	CParsedEdit	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
    
//-----------------------------------------------------------------------------
void CBoolEdit::SetValue(BOOL nValue)
{
	if (nValue)	
	{
		CParsedCtrl::SetValue(DataObj::Strings::YES());
		return;
	}

	CParsedCtrl::SetValue(DataObj::Strings::NO());
}

//-----------------------------------------------------------------------------
BOOL CBoolEdit::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	return AfxGetCultureInfo()->IsEqual(strValue, DataObj::Strings::YES());
}

//-----------------------------------------------------------------------------
void CBoolEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataBool)));
	SetValue((BOOL)((DataBool&)aValue));
}

//-----------------------------------------------------------------------------
void CBoolEdit::GetValue(DataObj& aValue)
{
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataBool)));
	
	((DataBool &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CBoolEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
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



//=============================================================================
//			Class CIdentifierEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIdentifierEdit, CStrEdit)

BEGIN_MESSAGE_MAP(CIdentifierEdit, CStrEdit)
	//{{AFX_MSG_MAP(CIdentifierEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CIdentifierEdit::CIdentifierEdit()
	:
	CStrEdit		(),
	m_pItemToCheck	(NULL),
	m_pSymTable		(NULL)
{
	m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
CIdentifierEdit::CIdentifierEdit
						(
							SymTable* pSymTable, CObject* pItem,
							UINT nBtnIDBmp /* = NO_BUTTON */, DataStr* pData
						)
	:
	CStrEdit		(nBtnIDBmp, pData),
	m_pItemToCheck	(pItem),
	m_pSymTable		(pSymTable)
{
	if (m_pSymTable)
		m_dwCtrlStyle	|= IDE_STYLE_MUST_NO_EXIST;
	else
		m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
CIdentifierEdit::CIdentifierEdit
						(
							SymTable* pSymTable, DataStr* pData,
							CObject* pItem, UINT nBtnIDBmp /* = NO_BUTTON */
						)
	:
	CStrEdit		(nBtnIDBmp, pData),
	m_pItemToCheck	(pItem),
	m_pSymTable		(pSymTable)
{
	if (m_pSymTable)
		m_dwCtrlStyle	|= IDE_STYLE_MUST_NO_EXIST;
	else
		m_dwCtrlStyle	|= IDE_STYLE_MUST_EXIST;
}
    
//-----------------------------------------------------------------------------
BOOL CIdentifierEdit::IsValid()
{
	if (!CParsedEdit::IsValid())
		return FALSE;

	if ((m_dwCtrlStyle & IDE_STYLE_NO_CHECK) == IDE_STYLE_NO_CHECK)
		return TRUE;
		
	CString strIdent;
	CParsedCtrl::GetValue(strIdent);                                                
		
	if ((m_dwCtrlStyle & IDE_STYLE_NO_EMPTY) == IDE_STYLE_NO_EMPTY && strIdent.IsEmpty())
	{
		m_nErrorID = FIELD_EMPTY;
		return FALSE;
	}
		
	if (m_pSymTable == NULL)
		return TRUE;

	SymField* pItem = m_pSymTable->GetField(strIdent);
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

	return m_nErrorID == 0;
}

//-----------------------------------------------------------------------------
BOOL CIdentifierEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if	(   
			(_istcntrl(nChar) || _istalnum(nChar) || (nChar == _T('_'))) &&
			(LOWORD(GetSel()) != 0 || !_istdigit(nChar))
		)
		return FALSE;

	BadInput();
	return TRUE;
}

//=============================================================================
//			Class CGuidEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CGuidEdit, CParsedEdit)

BEGIN_MESSAGE_MAP(CGuidEdit, CParsedEdit)
	//{{AFX_MSG_MAP(CGuidEdit)
	ON_MESSAGE (UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CGuidEdit::CGuidEdit()
	:
	CParsedEdit		(),
	m_guidCurValue	(GUID_NULL)
{
	SetCtrlStyle(STR_STYLE_ALL);
}

//-----------------------------------------------------------------------------
CGuidEdit::CGuidEdit(UINT nBtnIDBmp, DataGuid* pData /* = NULL */)
	:
	CParsedEdit (nBtnIDBmp),
	m_guidCurValue	(GUID_NULL)
{
	SetCtrlStyle(STR_STYLE_ALL);
	Attach(pData);
}

//-----------------------------------------------------------------------------
void CGuidEdit::Attach(DataObj* pDataObj)
{
	ASSERT(pDataObj == NULL || pDataObj->GetDataType() == GetDataType());
		
	CParsedCtrl::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
void CGuidEdit::SetValue(GUID nValue)
{
	CString strBuffer;

	CGuidFormatter* pFormatter = NULL;

	SetNewFormatIdx();

	if (m_nFormatIdx >= 0)
		pFormatter = (CGuidFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
	if (pFormatter)
		pFormatter->Format(&nValue, strBuffer, FALSE);

	m_guidCurValue = nValue;

	CParsedCtrl::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
GUID CGuidEdit::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	if (m_nFormatIdx >= 0)
	{
		CGuidFormatter* pFormatter = (CGuidFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);		
	}

	GUID guid;
	if (!IsGUIDStringValid(strValue, FALSE, &guid))
		return GUID_NULL;

	return guid;
}

//-----------------------------------------------------------------------------
void CGuidEdit::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue(((DataGuid &)aValue).GetGUID());
}

//-----------------------------------------------------------------------------
void CGuidEdit::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	((DataGuid &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
BOOL CGuidEdit::DoOnChar(UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istcntrl(nChar) && nChar != VK_BACK)
		return FALSE;

	DWORD	dwPos = GetSel();
	CString	strValue; CParsedCtrl::GetValue (strValue);

	CGuidFormatter*	pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CGuidFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	GUID guidVal;	
	IsGUIDStringValid(pFormatter ? pFormatter->UnFormat(strValue) : strValue, FALSE, &guidVal);

	int	nPos = (int) LOWORD(dwPos);

	if (guidVal != GUID_NULL && ((nChar == _T('{') && dwPos > 0) || (nChar == _T('}') &&  HIWORD(dwPos) != strValue.GetLength())))
	{
		BadInput();
		return TRUE;
	}

	if	(IsValidChar(nChar, strValue, dwPos, nPos))
		return FALSE;

	BadInput();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CGuidEdit::IsValidChar(UINT nChar, const CString& strValue, DWORD dwPos, int nPos)
{
	//@@TODO 
	// il controllo andrebbe migliorato, verificando anche se la posizione in cui
	// si trova il carattere è in accordo con il formato di un GUID
	return
		(
		(_istdigit(nChar) || (nChar >= _T('a') && nChar <= _T('f')) || (nChar >= _T('A') && nChar <= _T('F')) || nChar == _T('-') || nChar == _T('{') || nChar == _T('}') || nChar == VK_BACK) &&
			(dwPos != 0 || strValue.IsEmpty()) 
		);
}

//-----------------------------------------------------------------------------
void CGuidEdit::DoKillFocus (CWnd* pWnd)
{
	if (GetModifyFlag() && IsValid())
	{
		SetValue(GetValue());
		SetModifyFlag(TRUE);
	}

	CParsedEdit::DoKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
LRESULT CGuidEdit::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	CGuidFormatter* pFormatter = NULL;
	CGuidFormatter* pNewFormatter = NULL;

	if (m_nFormatIdx > 0)
		pFormatter = (CGuidFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
	if (m_nNewFormatIdx > 0 && m_nNewFormatIdx != m_nFormatIdx)
		pNewFormatter = (CGuidFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nNewFormatIdx, m_pFormatContext);

	DataGuid aValue;

	if (m_guidCurValue != GUID_NULL) 
		aValue.Assign(m_guidCurValue);
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

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

//=============================================================================
//			Class CParsedStatic implementation
//=============================================================================
IMPLEMENT_DYNAMIC(CParsedStatic, CBCGPStatic)

BEGIN_MESSAGE_MAP(CParsedStatic, CBCGPStatic)

	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_ENABLE			()
	ON_WM_VSCROLL			()     // for associated spin controls

	ON_WM_CTLCOLOR_REFLECT	()	

	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,		OnPushButtonCtrl)
	ON_MESSAGE				(UM_FORMAT_STYLE_CHANGED,	OnFormatStyleChange)
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,		OnGetControlDescription)
	
	ON_WM_NCPAINT			()
	ON_WM_PAINT				()
	ON_WM_ERASEBKGND		()
END_MESSAGE_MAP()
           
//disabilito il warning sul this passato al costruttore
#pragma warning(disable: 4355) 
//-----------------------------------------------------------------------------
CParsedStatic::CParsedStatic(DataObj *pData /*= NULL*/)
	:
	CBCGPStatic			(),
	CParsedCtrl			(pData),
	CColoredControl		(this),
	CCustomFont			(this),
	m_bRightAnchor		(FALSE),
	IDisposingSourceImpl(this)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);

	m_bVisualManagerStyle = TRUE;

	m_crText = AfxGetThemeManager()->GetStaticControlForeColor();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CParsedStatic::CParsedStatic(UINT nBtnIDBmp, DataObj *pData /*= NULL*/)
	:
	CBCGPStatic			(),
	CParsedCtrl			(pData),
	CColoredControl		(this),
	CCustomFont			(this),
	m_bRightAnchor		(FALSE),
	IDisposingSourceImpl(this)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);
	CParsedCtrl::Attach(nBtnIDBmp);

	m_bVisualManagerStyle = TRUE;

	m_crText = AfxGetThemeManager()->GetStaticControlForeColor();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();

}
#pragma warning(default: 4355)

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedStatic::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{                 
	switch (GetDataType().m_wType)
	{
		case DATA_INT_TYPE:
		case DATA_LNG_TYPE:
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
			if ((m_dwCtrlStyle & NUM_STYLE_NO_RIGHT_ALIGN) != NUM_STYLE_NO_RIGHT_ALIGN)
				dwStyle |= SS_RIGHT;
			break;
	}

	BOOL bOk = CheckControl(nID, pParentWnd, _T("STATIC"));
    if (!bOk) 
		return FALSE;

	if (dwStyle & (WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE))
	{
		dwStyle |= SS_SUNKEN;
	}
	
	bOk = __super::Create(_T(""), dwStyle, rect, pParentWnd, nID);
	if (!bOk) 
		return FALSE;

	this->m_bVisualManagerStyle = TRUE;

	bOk = CreateAssociatedButton(pParentWnd) && InitCtrl();
	return bOk;
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedStatic::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk =
			CheckControl(nID, pParentWnd, _T("STATIC"))	&&
			SubclassDlgItem(nID, pParentWnd)		&&
			CreateAssociatedButton(pParentWnd)		&&
			InitCtrl();

	if (bOk)
		SetNamespace(strName);
	
	return bOk;
}

//-----------------------------------------------------------------------------
 CParsedStatic::~CParsedStatic()
{
	 if (m_pProxy != NULL)
	 {
		 //force disconnect accessibility clients
		 ::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		 m_pProxy = NULL;
	 }
 }

//-----------------------------------------------------------------------------
BOOL CParsedStatic::OnInitCtrl ()
{
	__super::OnInitCtrl();
	
	if (AfxGetThemeManager()->UseFlatStyle())
	{
		if ((this->GetStyle() & SS_SUNKEN) == SS_SUNKEN)
		{
			ModifyStyle(SS_SUNKEN, WS_BORDER, SWP_FRAMECHANGED);
			ModifyStyleEx(0, WS_EX_CLIENTEDGE, SWP_FRAMECHANGED);
		}

		AfxGetThemeManager()->MakeFlat(GetCtrlCWnd());
	}

	if (!AfxGetThemeManager()->GetControlsUseBorders())
	{
		ModifyStyle(WS_BORDER, 0, SWP_FRAMECHANGED);
		ModifyStyleEx(WS_EX_CLIENTEDGE, 0, SWP_FRAMECHANGED);
	}

	if (IsKindOf(RUNTIME_CLASS(CDoubleStatic)))
		ModifyStyle(0,ES_RIGHT, SWP_FRAMECHANGED);

	SetWindowPos(NULL, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedStatic::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	GridCellPosChanging(wndPos);
}

//-----------------------------------------------------------------------------
void CParsedStatic::OnEnable(BOOL bEnable)
{
	// Per eliminare l'effetto 3D delle scitte degli statici disablitati
	// NB. NON MODIFICARE LA SEQUNZIALITA` DEGLI STATEMENTS

#define SELF_ENABLED	0x80000000
								
	if (!bEnable)
	{
		// si auto riabilita e segnala a se stesso cosa fare al rientro
		// in questa funzione a fronte della EnableWindow(TRUE)
		m_dwCtrlStyle |= SELF_ENABLED;
		EnableWindow(TRUE);
		return;
	}

	__super::OnEnable(bEnable);
	
	if ((m_dwCtrlStyle & SELF_ENABLED) == SELF_ENABLED)
	{
		// siamo in stato di auto riabilitazione e quindi bisogna
		// fare la DoEnable(FALSE)
		m_dwCtrlStyle &= ~SELF_ENABLED;
		bEnable = FALSE;
	}

	DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CParsedStatic::OnVScroll (UINT nSBCode, UINT, CScrollBar*)
{                  
	if (m_nButtonIDBmp == BTN_SPIN_ID)
		DoSpinScroll(nSBCode);
}

//------------------------------------------------------------------------------
BOOL CParsedStatic::OnEraseBkgnd(CDC* pDC)
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
			pDC->FillRect(&rclientRect, pParsedForm->GetBackgroundBrush());
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedStatic::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	COLORREF old = m_clrText;

	CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());

	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());

	int nIdx;
	BOOL bIsInStaticArea = FALSE;
	CBaseTileDialog* pTileDialog = NULL;
	if (pParsedForm &&  pParsedForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
	{
		pTileDialog = (CBaseTileDialog*)pParsedForm->GetFormCWnd();
		bIsInStaticArea = pTileDialog->IsInStaticArea(this, &nIdx);
	}

	if (m_pData && !bIsInStaticArea)
		m_clrText = pManager ? pManager->GetControlForeColor(this, FALSE) : AfxGetThemeManager()->GetStaticControlForeColor();

	if (m_bColored)
	{
		m_clrText = m_crText;
	}

	DoPaint(&dc);
	m_clrText = old;
}

//-----------------------------------------------------------------------------
void CParsedStatic::DoPaint(CDC* pDC)
{
	const DWORD dwStyle = GetStyle();
	if ((dwStyle & SS_ICON) == SS_ICON)
	{
		__super::DoPaint(pDC);
		return;
	}

	CRect rectText;
	GetClientRect(rectText);

	if ((GetStyle() & WS_BORDER) != 0 || (GetExStyle() & WS_EX_CLIENTEDGE) != 0)
	{
		// per allinearlo al paint dell'Edit
		rectText.top += 1;
		rectText.DeflateRect(5, 1);
	}

	if (m_hFont != NULL && ::GetObjectType(m_hFont) != OBJ_FONT)
	{
		m_hFont = NULL;
	}

	CFont* pOldFont = m_hFont == NULL ?
		(CFont*)pDC->SelectStockObject(DEFAULT_GUI_FONT) :
		pDC->SelectObject(CFont::FromHandle(m_hFont));

	ASSERT(pOldFont != NULL);

	UINT uiDTFlags = DT_WORDBREAK;

	if (dwStyle & SS_CENTER)
	{
		uiDTFlags |= DT_CENTER;
	}
	else if (dwStyle & SS_RIGHT)
	{
		uiDTFlags |= DT_RIGHT;
	}

	if (dwStyle & SS_NOPREFIX)
	{
		uiDTFlags |= DT_NOPREFIX;
	}

	if ((dwStyle & SS_CENTERIMAGE) == SS_CENTERIMAGE)
	{
		uiDTFlags |= DT_SINGLELINE | DT_VCENTER;
	}

	if ((dwStyle & SS_ELLIPSISMASK) == SS_ENDELLIPSIS)
	{
		uiDTFlags |= DT_END_ELLIPSIS;
	}

	if ((dwStyle & SS_ELLIPSISMASK) == SS_PATHELLIPSIS)
	{
		uiDTFlags |= DT_PATH_ELLIPSIS;
	}

	if ((dwStyle & SS_ELLIPSISMASK) == SS_WORDELLIPSIS)
	{
		uiDTFlags |= DT_WORD_ELLIPSIS;
	}

#if (!defined _BCGSUITE_)
	COLORREF clrText = m_clrText == (COLORREF)-1 ? CBCGPVisualManager::GetInstance()->GetDlgTextColor(GetParent()) : m_clrText;

#if (!defined BCGP_EXCLUDE_RIBBON)
	if (m_bBackstageMode && m_clrText == (COLORREF)-1)
	{
		clrText = CBCGPVisualManager::GetInstance()->GetRibbonBackstageTextColor();
	}
#endif

#else
	COLORREF clrText = m_clrText == (COLORREF)-1 ? globalData.clrBarText : m_clrText;
#endif

	if (!IsWindowEnabled())
	{
#ifndef _BCGSUITE_
		clrText = CBCGPVisualManager::GetInstance()->GetToolbarDisabledTextColor();
#else
		clrText = globalData.clrGrayedText;
#endif
	}

	CString strText;
	GetWindowText(strText);

	if (strText.Find(_T('\t')) >= 0)
	{
		uiDTFlags |= (DT_TABSTOP | 0x40);
	}

	if (!m_bOnGlass)
	{
		COLORREF clrTextOld = pDC->SetTextColor(clrText);
		pDC->SetBkMode(TRANSPARENT);
		pDC->DrawText(strText, rectText, uiDTFlags);
		pDC->SetTextColor(clrTextOld);
	}
	else
	{
		CBCGPVisualManager::GetInstance()->DrawTextOnGlass(pDC, strText, rectText, uiDTFlags, 6, IsWindowEnabled() ? m_clrText : globalData.clrGrayedText);
	}

	pDC->SelectObject(pOldFont);
}

//-----------------------------------------------------------------------------
void CParsedStatic::OnNcPaint()
{
	// default paint
	if (AfxGetThemeManager()->GetControlsUseBorders())
	{
		CLabelStatic* pLS = dynamic_cast<CLabelStatic*>(this);
		if (pLS)// && pLS->IsCustomDraw()) //(pLS->IsShowTextWithLine() || pLS->IsShowSeparator()))
		{
			__super::OnNcPaint();
			return;
		}

		if ((this->GetStyle() & WS_BORDER) == 0 && (this->GetExStyle() & WS_EX_CLIENTEDGE) == 0)
		{
			__super::OnNcPaint();
			return;
		}

		CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());
		if (pParsedForm && !pParsedForm->IsTransparent())
		{
			CDC* pDC = GetWindowDC();

			//work out the coordinates of the window rectangle,
			CRect rect;
			GetWindowRect(&rect);
			rect.OffsetRect(-rect.left, -rect.top);

			//Draw a single line around the outside
			pDC->FrameRect(&rect, AfxGetThemeManager()->GetStaticControlInBorderBkgBrush());

			ReleaseDC(pDC);
		}
	}
	else
	{
		__super::OnNcPaint();
	}
}

//-----------------------------------------------------------------------------
LRESULT CParsedStatic::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
LRESULT CParsedStatic::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	if (GetCtrlData())
		SetValue(*GetCtrlData());

	return 0L;
}
//-----------------------------------------------------------------------------
LRESULT CParsedStatic::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
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
	CParsedLabelDescription* pDesc = (CParsedLabelDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CParsedLabelDescription), strId);
	pDesc->UpdateAttributes(this);
	
	GetControlDescription(pDesc);

	return (LRESULT) pDesc;
}

//-----------------------------------------------------------------------------
void CParsedStatic::GetControlDescription(CParsedLabelDescription* pDesc)
{
	
	PopulateColorDescription(pDesc);
	PopulateFontDescription(pDesc);
}

//Metodo che dice se il CParsedStatic e' stato creato con lo stile SS_SUNKEN
//(vedere il codice dentro la CParsedStatic::Create)
//-----------------------------------------------------------------------------
BOOL CParsedStatic::HasSunkenStyle()
{
	return ::HasSunkenStyle(GetStyle(), GetExStyle());
}

//-----------------------------------------------------------------------------
BOOL CParsedStatic::HasEdgeStyle()
{
	DWORD dwStyle = GetStyle();
	return	(dwStyle & SS_BLACKRECT)	==	SS_BLACKRECT	||
			(dwStyle & SS_GRAYRECT)		==	SS_GRAYRECT		||
			(dwStyle & SS_WHITERECT)	==	SS_WHITERECT	||
			(dwStyle & SS_BLACKFRAME)	==	SS_BLACKFRAME	||
			(dwStyle & SS_GRAYFRAME)	==	SS_GRAYFRAME	||
			(dwStyle & SS_WHITEFRAME)	==	SS_WHITEFRAME;
}

//-----------------------------------------------------------------------------
HRESULT CParsedStatic::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}


//-----------------------------------------------------------------------------
void CParsedStatic::SetValue(const DataObj& aValue)
{
	SetValue(aValue.Str());
}

void CParsedStatic::SetValue(LPCTSTR pszValue)
{
	ASSERT(pszValue);
	CParsedCtrl::SetValue(pszValue);
}

//-----------------------------------------------------------------------------
void CParsedStatic::GetValue(DataObj& aValue)
{
	CString strBuffer;
	__super::GetValue(strBuffer);

	aValue.Assign(strBuffer);
}

void CParsedStatic::GetValue (CString& strValue)
{ 
	CParsedCtrl::GetValue(strValue);
}

//-----------------------------------------------------------------------------
BOOL CParsedStatic::ForceUpdateCtrlView	(int/* = -1*/)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedStatic::PaintStatic()
{
	CPaintDC dc(this); // device context for painting

	// Get the text
	CString text;
	this->GetWindowText(text);

	// Get the rectangle
	CRect clientRect;
	this->GetClientRect(&clientRect);

	dc.SetBkMode(TRANSPARENT);
	dc.SetTextColor(m_crText);

	CWnd* pParent = this->GetParent();
	CParsedForm* pParsedForm = ::GetParsedForm(pParent);
	CBaseTileDialog* pTileDlg = NULL;
	if (pParent->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		pTileDlg = (CBaseTileDialog*)pParent;

	COLORREF crBkgnd = m_crBkgnd;
	if (pParsedForm && !pParsedForm->IsTransparent())
	{
		CBrush* pOldBrush = NULL;
		if (m_bColored && pTileDlg == NULL)
		{
			ASSERT(m_pBrush);
			dc.SetBkColor(m_crBkgnd);
			pOldBrush = dc.SelectObject(GetBkgBrush());
		}
		else if (pParsedForm->GetBackgroundBrush())
		{
			HBRUSH hBrush = GetBkgBrushHandle();
			dc.SetBkColor(pParsedForm->GetBackgroundColor()); 
			crBkgnd = pParsedForm->GetBackgroundColor();
			pOldBrush = dc.SelectObject(pParsedForm->GetBackgroundBrush());
			hBrush = (HBRUSH)pParsedForm->GetBackgroundBrush()->GetSafeHandle();
		}
		if (pOldBrush)
		{
			dc.SelectObject(pOldBrush);
		}
	}

	// Style is set using the "Align Text" property in the dialog editor, or as a parameter to CreateWindow or CreateWindowEx
	const DWORD windowStyle(this->GetStyle());

	CFont* pOldFont = dc.SelectObject(((CParsedStatic*)this)->GetPreferredFont());
	
	if (pParsedForm && !pParsedForm->IsTransparent())
	{
	if (this->HasSunkenStyle())
	{
		clientRect.left -= 2;
		clientRect.top -= 2;
		clientRect.right += 2;
		clientRect.bottom += 2;

		dc.FillSolidRect(&clientRect, AfxGetThemeManager()->GetStaticControlOutBorderBkgColor());

		clientRect.left += 1;
		clientRect.top += 1;
		clientRect.right -= 1;
		clientRect.bottom -= 1;

		dc.FillSolidRect(&clientRect, AfxGetThemeManager()->GetStaticControlInBorderBkgColor());

		clientRect.left += 1;
		clientRect.top += 1;
		clientRect.right -= 1;
		clientRect.bottom -= 1;
	}

	dc.FillSolidRect(&clientRect, crBkgnd);

		if (pTileDlg && pTileDlg->GetPartSize())
		{
			CRect rectIntersez;
			for (int p = 0; p < pTileDlg->GetPartSize(); p++)
			{
				CRect rectStaticPart = pTileDlg->GetStaticAreaRect(p);
				pTileDlg->ClientToScreen(rectStaticPart);
				this->ScreenToClient(rectStaticPart);

				if (rectIntersez.IntersectRect(clientRect, rectStaticPart))
				{
					dc.FillSolidRect(&rectIntersez, pTileDlg->GetStaticAreaColor());
				}
			}
		}
	}

	// Draw The Line(s)
	int nFormat = DT_WORDBREAK | DT_NOCLIP | DT_NOPREFIX;

	if (SS_CENTERIMAGE & windowStyle)	// text and line are vertically centered
	{ 
		nFormat |= DT_VCENTER;
	}
	else
		nFormat |= DT_TOP;

	if (SS_RIGHT & windowStyle)		// right-aligned text
	{ 
		nFormat |= DT_RIGHT;
	}
	else if (SS_CENTER & windowStyle)	// centered text	
	{ 	
		nFormat |= DT_CENTER;
	}
	else	// left-aligned text
	{ 
		nFormat |= DT_LEFT;
	}

	// Draw the text
	COLORREF crOldTextColor = NULL;
	CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
	if (pManager)
		dc.SetTextColor(pManager->GetControlForeColor(this, IsWindowEnabled()));
	else
		dc.SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());

	/*int h = */dc.DrawText(text, clientRect, nFormat);
	
	if (crOldTextColor)
		dc.SetTextColor(crOldTextColor);

	if (pOldFont)
	{
		dc.SelectObject(pOldFont);
	}

	ReleaseDC(&dc);
}

//-----------------------------------------------------------------------------
CRuntimeClass* CParsedStatic::GetDefaultClassFor (DataObj* pDataObj)
{
	ASSERT(pDataObj);

	DataType aType = pDataObj->GetDataType();

	if (aType == DATA_INT_TYPE)	return RUNTIME_CLASS(CIntStatic);
	if (aType == DATA_LNG_TYPE)	return RUNTIME_CLASS(CLongStatic);
	if (aType == DATA_DBL_TYPE)	return RUNTIME_CLASS(CDoubleStatic);
	if (aType == DATA_MON_TYPE)	return RUNTIME_CLASS(CMoneyStatic);
	if (aType == DATA_QTA_TYPE)	return RUNTIME_CLASS(CQuantityStatic);
	if (aType == DATA_PERC_TYPE) return RUNTIME_CLASS(CPercStatic);
	if (aType == DATA_DATE_TYPE) return RUNTIME_CLASS(CDateStatic);
	if (aType == DATA_BOOL_TYPE) return RUNTIME_CLASS(CBoolStatic);
	if (aType == DATA_ENUM_TYPE) return RUNTIME_CLASS(CEnumStatic);

	return RUNTIME_CLASS(CStrStatic);
}


//=============================================================================
//			Class CStrStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CStrStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CStrStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CStrStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CStrStatic::CStrStatic()
	:
	CParsedStatic ()
{}

//-----------------------------------------------------------------------------
CStrStatic::CStrStatic(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CParsedStatic (nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
                        
//-----------------------------------------------------------------------------
void CStrStatic::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	CParsedStatic::SetValue(((DataStr&) aValue).GetString());
}

//-----------------------------------------------------------------------------
void CStrStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	__super::GetValue(aValue);
}

//=============================================================================
//			Class CResizableStrStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CResizableStrStatic, CStrStatic)

BEGIN_MESSAGE_MAP(CResizableStrStatic, CStrStatic)
	//{{AFX_MSG_MAP(CResizableStrStatic)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CResizableStrStatic::CResizableStrStatic(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CStrStatic (nBtnIDBmp, pData)
{
}

//-----------------------------------------------------------------------------
BOOL CResizableStrStatic::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName)
{
	if (!CStrStatic::SubclassEdit(IDC, pParent, strName))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CResizableStrStatic::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//=============================================================================
//			Class CTextStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTextStatic, CResizableStrStatic)

BEGIN_MESSAGE_MAP(CTextStatic, CResizableStrStatic)
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CTextStatic::CTextStatic()
	:
	CResizableStrStatic ()
{}

//-----------------------------------------------------------------------------
CTextStatic::CTextStatic(UINT nBtnIDBmp, DataText* pData /* = NULL */)
	:
	CResizableStrStatic (nBtnIDBmp, pData)
{
}

//=============================================================================
//			Class CLabelStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CLabelStatic, CResizableStrStatic)

BEGIN_MESSAGE_MAP(CLabelStatic, CResizableStrStatic)
	
	ON_WM_PAINT				()
	ON_WM_ERASEBKGND		()

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLabelStatic::CLabelStatic()
	:	
	m_bShowSeparator		(FALSE),
	m_bVerticalSeparator	(FALSE),
	m_bMiniHtml				(FALSE)
{
	m_crText = AfxGetThemeManager()->GetEnabledControlForeColor();
}

//-----------------------------------------------------------------------------
void CLabelStatic::ShowTextWithLine (COLORREF crBorder, int nSizePen/* = 1*/, /*ELinePos*/int pos/* = LP_VCENTER*/)
{ 
	m_bShowSeparator = FALSE;	
	m_LinePos = (ELinePos) pos;

	SetLineColor (crBorder, nSizePen);
}

//-----------------------------------------------------------------------------
void CLabelStatic::ShowSeparator (COLORREF crBorder, int nSizePen/* = 1*/, BOOL  bVertical/* = FALSE*/, ELinePos pos/* = LP_NONE*/)
{ 
	m_bShowSeparator = TRUE;	
	m_LinePos = (ELinePos)pos;
	m_bVerticalSeparator = bVertical;	

	SetLineColor (crBorder, nSizePen);
}

//-----------------------------------------------------------------------------
void CLabelStatic::ShowMiniHtml ()
{ 
	m_bCustomDraw = TRUE;

	m_bMiniHtml = TRUE;

	m_bShowSeparator = FALSE;	//m_bShowTextWithLine = FALSE;	
}

//-----------------------------------------------------------------------------
void CLabelStatic::SetZOrderInnerControls(CWnd* pParentWnd/*= NULL*/)
{
	CWnd* pParent = pParentWnd ? pParentWnd : GetParent();
	CParsedForm* pForm = ::GetParsedForm(pParent);

	if (pForm && pForm->SetZOrderInnerControls(this, pParent) > 0)
		m_bCustomDraw = TRUE;
}

//-----------------------------------------------------------------------------
void CLabelStatic::GetControlDescription(CParsedLabelDescription* pDesc)
{
	__super::GetControlDescription(pDesc);

	if (pDesc->m_LinePos != m_LinePos)
	{
		pDesc->m_LinePos = m_LinePos;
		pDesc->SetUpdated(&pDesc->m_LinePos);
	}
}

//------------------------------------------------------------------------------------------------------
void CLabelStatic::SetFontStyleAndColor(BOOL bBold, BOOL bItalic, BOOL bUnderline, COLORREF aColor)
{
	__super::SetOwnFont(bBold, bItalic, bUnderline);
	SetCustomDraw(FALSE);
	SetTextColor(aColor);
}

//-----------------------------------------------------------------------------
void CLabelStatic::SetOwnFont(BOOL bBold, BOOL bItalic, BOOL bUnderline, int nPointSize /*0*/, LPCTSTR lpszFaceName/*NULL*/)
{
	__super::SetOwnFont(bBold, bItalic, bUnderline, nPointSize, lpszFaceName);
}

//-----------------------------------------------------------------------------
void CLabelStatic::SetOwnFont(CFont* pFont, BOOL bOwns /*FALSE*/)
{
	ASSERT_VALID(pFont);
	if (!pFont)
		return;

	m_bCustomDraw = bOwns;
	__super::SetOwnFont(pFont, bOwns);
}

//-----------------------------------------------------------------------------
void CLabelStatic::OnPaint ()
{ 
	if (m_bCustomDraw)
	{
		 if (m_bMiniHtml)
			PaintMiniHtml();
		else if (IsShowTextWithLine())
			PaintStaticWithLine();
		else if (m_bShowSeparator)
			PaintSeparator();
		else
			__super::PaintStatic();
	}
	else
		__super::OnPaint();
}

//-----------------------------------------------------------------------------
BOOL CLabelStatic::OnEraseBkgnd (CDC* pDC)
{ 
	if (m_bCustomDraw)
		return TRUE; 
	else
		return __super::OnEraseBkgnd(pDC);
}

//-----------------------------------------------------------------------------
void CLabelStatic::PaintStaticWithLine()
{
	CPaintDC dc(this); // device context for painting

	// Get the rectangle
	CRect clientRect;
	this->GetClientRect(&clientRect);

	dc.SetBkMode(TRANSPARENT);
	HBRUSH hBrush = GetBkgBrushHandle();
	COLORREF crBkgnd = m_crBkgnd;

	CWnd* pParent = this->GetParent();
	CParsedForm* pParsedForm = ::GetParsedForm(pParent);
	CBaseTileDialog* pTileDlg = NULL;
	if (pParent->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		pTileDlg = (CBaseTileDialog*)pParent;

	if (m_bColored)
	{
		dc.SetTextColor(m_crText);

		ASSERT(m_pBrush);
		dc.SetBkColor(m_crBkgnd);
		dc.SelectObject(GetBkgBrush());
	}
	else
	{
		if (pParsedForm && pParsedForm->GetBackgroundBrush())
		{
			dc.SetBkColor(pParsedForm->GetBackgroundColor()); crBkgnd = pParsedForm->GetBackgroundColor();
			dc.SelectObject(pParsedForm->GetBackgroundBrush()); hBrush = (HBRUSH)pParsedForm->GetBackgroundBrush()->GetSafeHandle();
		}
	}

	const int nSep = 2;

	// Get the text
	CString text;
	this->GetWindowText(text);

	// Style is set using the "Align Text" property in the dialog editor, or as a parameter to CreateWindow or CreateWindowEx
	const DWORD windowStyle(this->GetStyle());

	ASSERT_KINDOF(CParsedStatic, this);

	dc.SelectObject(((CParsedStatic*)this)->GetPreferredFont());

	if (pParsedForm && !pParsedForm->IsTransparent())
	{
	dc.FillSolidRect(&clientRect, crBkgnd);

		if (pTileDlg && pTileDlg->GetPartSize())
		{
			CRect rectIntersez;
			for (int p = 0; p < pTileDlg->GetPartSize(); p++)
			{
				CRect rectStaticPart = pTileDlg->GetStaticAreaRect(p);
				pTileDlg->ClientToScreen(rectStaticPart);
				this->ScreenToClient(rectStaticPart);

				if (rectIntersez.IntersectRect(clientRect, rectStaticPart))
				{
					dc.FillSolidRect(&rectIntersez, pTileDlg->GetStaticAreaColor());
				}
			}
		}
	}

	const int offsetSize(3/*textSize.cy / 2*/); // offset the start of the line a little bit from the edge of the text
	
	CRect textRect(clientRect);

	CSize textSize(dc.GetTextExtent(text));

	const DWORD textDrawingFlags( ((m_LinePos & LP_VCENTER) ? 0 : DT_SINGLELINE) | DT_NOCLIP | DT_LEFT | DT_NOPREFIX);

	// Calculate the text size

	int nTopLineY;
	if (SS_CENTERIMAGE & windowStyle)	// text and line are vertically centered
	{ 
		textRect.top = clientRect.CenterPoint().y - (textSize.cy / 2);
		nTopLineY = clientRect.CenterPoint().y;
	}
	else	// text and line run along the top edge of the control
	{ 
		nTopLineY = textSize.cy / 2;
	}
	
	CPoint textLocation(0,0); // have to calculate, since DrawState does not have a center flag (is there a DSS_CENTER, and I just missed it ???)

	// Draw The Line(s)
	int hdir;
	if (SS_RIGHT & windowStyle)		// right-aligned text
	{ 
		hdir = 2;
	}
	else if (SS_CENTER & windowStyle)	// centered text	
	{ 	
		hdir = 1;
	}
	else	// left-aligned text
	{ 
		hdir = 0;
	}

	switch (hdir)
	{
		case 0:	//LEFT	- left-aligned text
			{
				textRect.right = textRect.left + textSize.cx;

				if (IsShowTextWithLine() && (m_LinePos & LP_VCENTER))
				{
					DrawHLine(dc, nTopLineY, textRect.right + offsetSize, clientRect.right, m_crLine, m_nSizeLinePen == 1);
				}
				break;
			}
		case 1:	//CENTER
			{
				const int eachSideWidth = (clientRect.Width() - textSize.cx) / 2;
				textRect.left = eachSideWidth;
				textRect.right = textRect.left + textSize.cx;

				if (IsShowTextWithLine() && (m_LinePos & LP_VCENTER))
				{
					DrawHLine(dc, nTopLineY, clientRect.left, textRect.left - offsetSize, m_crLine, m_nSizeLinePen == 1);
					DrawHLine(dc, nTopLineY, textRect.right + offsetSize, clientRect.right, m_crLine, m_nSizeLinePen == 1);
				}
				break;
			}
		case 2:	//RIGHT
			{
				textRect.left = textRect.right - textSize.cx;

				if (IsShowTextWithLine() && (m_LinePos & LP_VCENTER))
				{
					DrawHLine(dc, nTopLineY, clientRect.left, textRect.left - offsetSize, m_crLine, m_nSizeLinePen == 1);
				}
				break;
			}
	}

	if (IsShowTextWithLine())
	{
		if (m_LinePos & LP_TOP)
		{
			DrawHLine(dc, clientRect.top, clientRect.left, clientRect.right, m_crLine, FALSE);
		}
		if (m_LinePos & LP_BOTTOM)
		{
			DrawHLine(dc, clientRect.bottom - max(2, m_nSizeLinePen), clientRect.left, clientRect.right, m_crLine, FALSE);
		}
		if (m_LinePos & LP_LEFT)
		{
			DrawVLine(dc, clientRect.left + m_nSizeLinePen / 2, ((m_LinePos & LP_VCENTER) ? nTopLineY : clientRect.top), clientRect.bottom, m_crLine, FALSE);
		}
		if (m_LinePos & LP_RIGHT)
		{
			DrawVLine(dc, clientRect.right - m_nSizeLinePen / 2, ((m_LinePos & LP_VCENTER) ? nTopLineY : clientRect.top), clientRect.bottom, m_crLine, FALSE, FALSE);
		}
		ASSERT((m_LinePos & LP_HCENTER) == 0);
		ASSERT(!m_bVerticalSeparator);
	}

	// Draw the text
	
	//const UINT drawStateFlags(this->IsWindowEnabled() ? DSS_NORMAL : DSS_DISABLED);
	const UINT drawStateFlags(DSS_NORMAL);

	if (!text.IsEmpty())
		dc.DrawState
			(
				CPoint(textRect.left, textRect.top),
				CSize(0,0), // 0,0 says to calculate it (per DrawState SDK docs)
				text, 
				drawStateFlags,
				TRUE, 0, 
				hBrush
			);
}

//-----------------------------------------------------------------------------
void CLabelStatic::PaintSeparator()
{
	CPaintDC dc(this); // device context for painting

	CRect clientRect;
	this->GetClientRect(&clientRect);

	CPen pen(PS_SOLID, m_nSizeLinePen, m_crLine);	
	CPen* oldPen = dc.SelectObject(&pen);

	if (m_bVerticalSeparator)
	{
		int x = clientRect.left + m_nSizeLinePen / 2;
		dc.MoveTo(x, clientRect.top);
		dc.LineTo(x, clientRect.bottom);
	}
	else
	{
		int y = clientRect.top + m_nSizeLinePen / 2;
		dc.MoveTo(clientRect.left , y);
		dc.LineTo(clientRect.right, y);
	}

	dc.SelectObject(oldPen);
}

//-----------------------------------------------------------------------------
void CLabelStatic::PaintMiniHtml ()
{
	CPaintDC dc(this); // device context for painting

	COLORREF crBkgnd = m_crBkgnd;
	if (!m_bColored)
	{
		CWnd* pParent = this->GetParent();
		CParsedForm* pParsedForm = ::GetParsedForm(pParent);
		if (pParsedForm && pParsedForm->GetBackgroundBrush())
			{
			crBkgnd = pParsedForm->GetBackgroundColor();
		}
	}

	CRect clientRect;
	this->GetClientRect(&clientRect);
	
	::SetBkColor(dc.m_hDC, crBkgnd);
	{
		HBRUSH hbrush = CreateSolidBrush(crBkgnd); 
		ASSERT(hbrush);
		FillRect(dc.m_hDC, &clientRect, hbrush);
		if (hbrush)
			DeleteObject(hbrush);
	}

	CString strText;
	GetWindowText(strText);

	/*
	CXHtmlDraw::XHTMLDRAWSTRUCT ds(this->GetTextColor(), crBkgnd, GetFont(), (LPRECT)clientRect);
	CXHtmlDraw htmldraw;
	htmldraw.Draw(dc.m_hDC, strText, &ds, FALSE);
	*/
	CMiniHtmlDraw mh;
	mh.Draw(dc.m_hDC, this, this->GetTextColor(), crBkgnd);

	//----
	if (IsShowTextWithLine())
	{	
		if (m_LinePos & LP_TOP)
		{
			DrawHLine(dc, clientRect.top, clientRect.left, clientRect.right, m_crLine, FALSE);
		}
		if (m_LinePos & LP_BOTTOM)
		{
			DrawHLine(dc, clientRect.bottom - max(2, m_nSizeLinePen), clientRect.left, clientRect.right, m_crLine, FALSE);
		}
		if (m_LinePos & LP_LEFT)
		{
			DrawVLine(dc, clientRect.left + m_nSizeLinePen / 2, clientRect.top, clientRect.bottom, m_crLine, FALSE);
		}
		if (m_LinePos & LP_RIGHT)
		{
			DrawVLine(dc, clientRect.right - m_nSizeLinePen / 2, clientRect.top, clientRect.bottom, m_crLine, FALSE, FALSE);
		}
		ASSERT((m_LinePos & (LP_VCENTER|LP_HCENTER)) == 0);
		ASSERT(!m_bVerticalSeparator);
	}
}

//=============================================================================
//			Class CIntStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CIntStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CIntStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CIntStatic::CIntStatic()
	:
	CParsedStatic	(),
	m_nMin			(SHRT_MIN),
	m_nMax			(SHRT_MAX)
{}

//-----------------------------------------------------------------------------
CIntStatic::CIntStatic(UINT nBtnIDBmp, DataInt* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_nMin			(SHRT_MIN),
	m_nMax			(SHRT_MAX)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CIntStatic::SetRange(int nMin, int nMax)
{                      
	if (nMin > nMax)
		nMax = nMin;

	m_nMin = nMin;
	m_nMax = nMax;
}

//-----------------------------------------------------------------------------
void CIntStatic::SetValue(int nValue)
{
	CString strBuffer;

	CIntFormatter* pFormatter = NULL;

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
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(7);
			_stprintf_s(szBuffer, nLen, _T("%d"), nValue);
			strBuffer.ReleaseBuffer();
		}

	CParsedStatic::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
int CIntStatic::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	if (m_nFormatIdx >= 0)
	{
		CIntFormatter* pFormatter = (CIntFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);
	}

	return _tstoi(strValue);
}

//-----------------------------------------------------------------------------
void CIntStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((int) ((DataInt &)aValue));
}

//-----------------------------------------------------------------------------
void CIntStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataInt &)aValue).Assign(GetValue());
}

// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CIntStatic::DoSpinScroll(UINT nSBCode)
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
		UpdateCtrlData(TRUE);
	}
}

//=============================================================================
//			Class CLongStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLongStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CLongStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CLongStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLongStatic::CLongStatic()
	:
	CParsedStatic	(),
	m_lMin			(LONG_MIN),
	m_lMax			(LONG_MAX)
{}

//-----------------------------------------------------------------------------
CLongStatic::CLongStatic(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_lMin			(LONG_MIN),
	m_lMax			(LONG_MAX)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CLongStatic::SetRange	(int nMin, int nMax)
{
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = nMin;
	m_lMax = nMax;
}

//-----------------------------------------------------------------------------
void CLongStatic::SetValue(long nValue)
{
	CString strBuffer;

	CLongFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CLongFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
	if (pFormatter)
	{
		if	(
				pFormatter->IsZeroPadded() &&
				pFormatter->GetPaddedLen() > 0	&&
				pFormatter->GetPaddedLen() < 12
			)
			pFormatter->Format(&nValue, strBuffer, FALSE);
		else
			if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
				pFormatter->Format(&nValue, strBuffer, FALSE);
	}
	else
		if (nValue || (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO)
		{
			const rsize_t nLen = 12;
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(12);
			_stprintf_s(szBuffer, nLen, _T("%ld"), nValue);
			strBuffer.ReleaseBuffer();
		}
		
	CParsedStatic::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
long CLongStatic::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue(strValue);

	if (m_nFormatIdx >= 0)
	{
		CLongFormatter* pFormatter = (CLongFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);
	}

	return _tstol(strValue);
}

//-----------------------------------------------------------------------------
void CLongStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataLng &)aValue));
}

//-----------------------------------------------------------------------------
void CLongStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataLng &)aValue).Assign(GetValue());
}

// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CLongStatic::DoSpinScroll(UINT nSBCode)
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
		UpdateCtrlData(TRUE);
	}
}

//=============================================================================
//			Class CDoubleStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDoubleStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CDoubleStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CDoubleStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDoubleStatic::CDoubleStatic()
	:
	CParsedStatic	(),
	m_dMin			(-DBL_MAX),
	m_dMax			(DBL_MAX),
	m_nDec			(DEFAULT_N_DEC),
	m_pExternalNumDec	(NULL),
	m_dCurValue			(0)
{}

//-----------------------------------------------------------------------------
CDoubleStatic::CDoubleStatic(UINT nBtnIDBmp, DataDbl* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_dMin			(-DBL_MAX),
	m_dMax			(DBL_MAX),
	m_nDec			(DEFAULT_N_DEC),
	m_pExternalNumDec	(NULL),
	m_dCurValue			(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CDoubleStatic::SetRange (double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;

	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}

//-----------------------------------------------------------------------------
CDblFormatter* CDoubleStatic::GetFormatter() const
{
	CDblFormatter* pFormatter = (CDblFormatter*)GetCurrentFormatter();
	return pFormatter;
}

//-----------------------------------------------------------------------------
int CDoubleStatic::GetCtrlNumDec ()
{
	if (m_pExternalNumDec)
		return (int) *m_pExternalNumDec;
	if (m_nDec >= 0)
		return m_nDec;

	CDblFormatter* pFormatter = GetFormatter();
	
	return pFormatter ? pFormatter->GetDecNumber() : m_nDec;
}

//-----------------------------------------------------------------------------
void CDoubleStatic::SetValue(double nValue)
{
	CString strBuffer;

	if (
			(fabs(nValue) >= ::GetEpsilonForDataType(GetDataType())) ||
			(m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO
		)
	{
		CDblFormatter* pFormatter = GetFormatter();
		if (pFormatter)
		{
			int nOldDN;
			if (m_pExternalNumDec || m_nDec >= 0)
				nOldDN = pFormatter->SetDecNumber(GetCtrlNumDec());

			pFormatter->Format(&nValue, strBuffer, FALSE);

			if (m_pExternalNumDec || m_nDec >= 0)
				pFormatter->SetDecNumber(nOldDN);
		}
		else
		{
			const rsize_t nLen = 512;
			TCHAR* szBuffer = strBuffer.GetBufferSetLength(512);
			int nDec = GetCtrlNumDec();
			if (nDec >= 0)
				_stprintf_s(szBuffer, nLen, _T("%.*f"), nDec, nValue);
			else
				_stprintf_s(szBuffer, nLen, _T("%f"), nValue);
				
			strBuffer.ReleaseBuffer();
		}
	}
		
	CParsedStatic::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
double CDoubleStatic::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	CDblFormatter* pFormatter = GetFormatter();
	if (pFormatter)
		strValue = pFormatter->UnFormat(strValue);

	return _tstof(strValue);
}

//-----------------------------------------------------------------------------
void CDoubleStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataDbl &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
void CDoubleStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((double) ((DataDbl &)aValue));
}

// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDoubleStatic::DoSpinScroll(UINT nSBCode)
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
		UpdateCtrlData(TRUE);
	}
}

//-----------------------------------------------------------------------------
CString CDoubleStatic::FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const
{
	ASSERT_VALID(pDataObj);
	CString strCell;

	// Viene formattato il dato
	CDblFormatter* pFormatter = GetFormatter();
	if (pFormatter)
	{
		//se il programmatore ha scelto un numero di decimali diverso da quello del formattatore
		int nOldDN;
		if (m_pExternalNumDec || m_nDec >= 0)
			nOldDN = pFormatter->SetDecNumber(const_cast<CDoubleStatic*>(this)->GetCtrlNumDec());

		pFormatter->FormatDataObj(*pDataObj, strCell, bEnablePadding);

		if (m_pExternalNumDec || m_nDec >= 0)
			pFormatter->SetDecNumber(nOldDN);
	}
	else
		return pDataObj->Str();

	return strCell;
}

//=============================================================================
//			Class CMoneyStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMoneyStatic, CDoubleStatic)

BEGIN_MESSAGE_MAP(CMoneyStatic, CDoubleStatic)
	//{{AFX_MSG_MAP(CMoneyStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMoneyStatic::CMoneyStatic()
	:
	CDoubleStatic()
{}

//-----------------------------------------------------------------------------
CMoneyStatic::CMoneyStatic(UINT nBtnIDBmp, DataMon* pData)
	:
	CDoubleStatic(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CMoneyStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleStatic::SetValue((double) ((DataMon &)aValue));
}

//-----------------------------------------------------------------------------
void CMoneyStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataMon &)aValue).Assign( CDoubleStatic::GetValue() );
}

//=============================================================================
//			Class CQuantityStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CQuantityStatic, CDoubleStatic)

BEGIN_MESSAGE_MAP(CQuantityStatic, CDoubleStatic)
	//{{AFX_MSG_MAP(CQuantityStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CQuantityStatic::CQuantityStatic()
	:
	CDoubleStatic()
{}

//-----------------------------------------------------------------------------
CQuantityStatic::CQuantityStatic(UINT nBtnIDBmp, DataQty* pData)
	:
	CDoubleStatic(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CQuantityStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleStatic::SetValue((double) ((DataQty &)aValue));
}

//-----------------------------------------------------------------------------
void CQuantityStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataQty &)aValue).Assign( CDoubleStatic::GetValue() );
}

//=============================================================================
//			Class CPercStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPercStatic, CDoubleStatic)

BEGIN_MESSAGE_MAP(CPercStatic, CDoubleStatic)
	//{{AFX_MSG_MAP(CPercStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPercStatic::CPercStatic()
	:
	CDoubleStatic()
{
	m_dMin = -100.;
	m_dMax = 100.;
}

//-----------------------------------------------------------------------------
CPercStatic::CPercStatic(UINT nBtnIDBmp, DataPerc* pData)
	:
	CDoubleStatic(nBtnIDBmp, pData)
{
	m_dMin = -100.;
	m_dMax = 100.;

	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CPercStatic::SetRange(double nMin, double nMax, int numDec)
{
	if (nMin > nMax)
		nMax = nMin;
		
	m_dMin = nMin;
	m_dMax = nMax;

	m_nDec = numDec;
}

//-----------------------------------------------------------------------------
void CPercStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleStatic::SetValue((double) ((DataPerc &)aValue));
}

//-----------------------------------------------------------------------------
void CPercStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataPerc &)aValue).Assign( CDoubleStatic::GetValue() );
}

//=============================================================================
//			Class CDateStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CDateStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CDateStatic)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateStatic::CDateStatic()
	:
	CParsedStatic	(),
	m_lMin			(MIN_GIULIAN_DATE),
	m_lMax			(MAX_GIULIAN_DATE),
	m_nCurDate		(::TodayDate())
{}

//-----------------------------------------------------------------------------
CDateStatic::CDateStatic(UINT nBtnIDBmp, DataDate* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_lMin			(MIN_GIULIAN_DATE),
	m_lMax			(MAX_GIULIAN_DATE),
	m_nCurDate		(::TodayDate())
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || !pData->IsATime());
}

//-----------------------------------------------------------------------------
void CDateStatic::SetRange(int nMin, int nMax)
{                 
	if (nMin < MIN_GIULIAN_DATE) nMin = MIN_GIULIAN_DATE;
	if (nMax > MAX_GIULIAN_DATE) nMax = MAX_GIULIAN_DATE;
	
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = (long)nMin;
	m_lMax = (long)nMax;
}

//-----------------------------------------------------------------------------
void CDateStatic::SetRange(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax)
{
	long nMin = ::GetGiulianDate(dMin, mMin, yMin);
	long nMax = ::GetGiulianDate(dMax, mMax, yMax);

	SetRange(nMin, nMax);
}

//-----------------------------------------------------------------------------
void CDateStatic::SetValue(long nValue)
{
	m_nCurDate = nValue;

    WORD nDay, nMonth, nYear;          
    ::GetShortDate(nDay, nMonth, nYear, nValue);
	CParsedStatic::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
}
    
//-----------------------------------------------------------------------------
void CDateStatic::SetValue(WORD nDay, WORD nMonth, WORD nYear)
{
	m_nCurDate = ::GetGiulianDate(nDay, nMonth, nYear);
	CParsedStatic::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
}

//-----------------------------------------------------------------------------
long CDateStatic::GetValue()
{                          
	DBTIMESTAMP aDateTime;
    CString strDate;
             
	CParsedStatic::GetValue(strDate);
	if (::GetTimeStamp(aDateTime, strDate, m_nFormatIdx, m_pFormatContext))
		return ::GetGiulianDate(aDateTime);

	return BAD_DATE;
}

//-----------------------------------------------------------------------------
void CDateStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataDate &)aValue));
}

//-----------------------------------------------------------------------------
void CDateStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataDate &)aValue).SetDate(GetValue());
}

// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CDateStatic::DoSpinScroll(UINT nSBCode)
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
		UpdateCtrlData(TRUE);
	}
}

//-----------------------------------------------------------------------------
LRESULT CDateStatic::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	if (m_nErrorID == 0)
		SetValue(m_nCurDate);

	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CDateStatic::GetToolTipProperties (CTooltipProperties& tp)
{ 
	if (IsKindOf(RUNTIME_CLASS(CTimeStatic)))
		return FALSE;
	DataDate date;
	if (IsKindOf(RUNTIME_CLASS(CDateTimeStatic)))
		date.SetFullDate();

	GetValue(date);
	if (!date.IsEmpty())
	{
		tp.m_strText = cwsprintf(_TB("{0-%s}; Week: {1-%d}"), (LPCTSTR)date.WeekDayName(), date.WeekOfYear());
		return TRUE;
	}
	return FALSE; 
}

//=============================================================================
//			Class CDateTimeStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateTimeStatic, CDateStatic)

BEGIN_MESSAGE_MAP(CDateTimeStatic, CDateStatic)
	//{{AFX_MSG_MAP(CDateTimeStatic)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateTimeStatic::CDateTimeStatic()
	:
	CDateStatic	(),
	m_nCurTime (0)
{}

//-----------------------------------------------------------------------------
CDateTimeStatic::CDateTimeStatic(UINT nBtnIDBmp, DataDate* pData)
	:
	CDateStatic	(nBtnIDBmp, pData),
	m_nCurTime (0)
{
	// NON si puo` usare il metodo CDateStatic(nBtnIDBmp, pData) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
void CDateTimeStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue(((DataDate &)aValue).GetDateTime());
}

//-----------------------------------------------------------------------------
void CDateTimeStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataDate &)aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
void CDateTimeStatic::SetValue(const DBTIMESTAMP& aDateTime)
{
	CDateFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	CString str; 
	if (pFormatter)
		pFormatter->Format(&aDateTime, str, FALSE);

	m_nCurDate = ::GetGiulianDate(aDateTime);
	m_nCurTime = ::GetTotalSeconds(aDateTime);
	CParsedStatic::SetValue(str);
}
    
//-----------------------------------------------------------------------------
DBTIMESTAMP CDateTimeStatic::GetValue()
{                          
	DBTIMESTAMP aDateTime;
    CString strDate;
             
	CParsedCtrl::GetValue(strDate);

	// se non e` valida GetTimeStamp valorizza aDateTime con dei valori BAD
	::GetTimeStamp(aDateTime, strDate, m_nFormatIdx, m_pFormatContext);

	return aDateTime;
}

//-----------------------------------------------------------------------------
LRESULT CDateTimeStatic::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	if (m_nErrorID == 0)
	{
		DataDate aDataDate;
		aDataDate.SetFullDate();
		aDataDate.Assign(m_nCurDate, m_nCurTime);
		SetValue(aDataDate);
	}

	return 0L;
}

//=============================================================================
//			Class CTimeStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CTimeStatic, CDateTimeStatic)

BEGIN_MESSAGE_MAP(CTimeStatic, CDateTimeStatic)
	//{{AFX_MSG_MAP(CDateTimeStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTimeStatic::CTimeStatic()
	:
	CDateTimeStatic	()
{}

//-----------------------------------------------------------------------------
CTimeStatic::CTimeStatic(UINT nBtnIDBmp, DataDate* pData)
	:
	CDateTimeStatic	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CDateStatic(nBtnIDBmp, pData) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->IsFullDate());
}

//-----------------------------------------------------------------------------
void CTimeStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	DBTIMESTAMP aDateTime = GetValue();
	((DataDate &)aValue).SetTime(aDateTime.hour, aDateTime.minute, aDateTime.second);
}

//-----------------------------------------------------------------------------
DBTIMESTAMP CTimeStatic::GetValue()
{                          
	DBTIMESTAMP aDateTime = CDateTimeStatic::GetValue();

	// se al control e` associato un DataDateTime (quindi e` un FullDate, come
	// DEVE essere il DataDate associato a questo tipo di control, ma NON gestisce
	// solamente l'Ora) valorizza la parte Data con il valore della data del DataDate
	if (GetCtrlData() && !GetCtrlData()->IsATime())
	{
		aDateTime.day	= ((DataDate*) GetCtrlData())->Day();
		aDateTime.month	= ((DataDate*) GetCtrlData())->Month();
		aDateTime.year	= ((DataDate*) GetCtrlData())->Year();
	}

	return aDateTime;
}

//-----------------------------------------------------------------------------
LRESULT CTimeStatic::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	if (m_nErrorID == 0)
	{
		DataDate aDataDate;
		aDataDate.SetAsTime();
		aDataDate.SetTime(m_nCurTime);
		SetValue(aDataDate);
	}

	return 0L;
}

//=============================================================================
//@@ElapsedTime			Class CElapsedTimeStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CElapsedTimeStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CElapsedTimeStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CDateStatic)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CElapsedTimeStatic::CElapsedTimeStatic()
	:
	CParsedStatic	(),
	m_lMin			(0),
	m_lMax			(LONG_MAX),
	m_nCurTime		(0)
{}

//-----------------------------------------------------------------------------
CElapsedTimeStatic::CElapsedTimeStatic(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData),
	m_lMin			(0),
	m_lMax			(LONG_MAX),
	m_nCurTime		(0)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
BOOL CElapsedTimeStatic::OnInitCtrl()
{
	VERIFY(CParsedStatic::OnInitCtrl());
	
	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter && pFormatter->GetCaptionPos() != 0)
		SetCaption((Token)pFormatter->GetCaptionPos(), _T(""), 1);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CElapsedTimeStatic::SetRange(int nMin, int nMax)
{                 
	if (nMin < 0) nMin = 0;
	if (nMax > LONG_MAX) nMax = LONG_MAX;
	
	if (nMin > nMax)
		nMax = nMin;

	m_lMin = nMin;
	m_lMax = nMax;
}

//-----------------------------------------------------------------------------
void CElapsedTimeStatic::SetValue(long nValue)
{
	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	CString str; 
	if (pFormatter)
		pFormatter->Format(&nValue, str, FALSE);

	m_nCurTime = nValue;
	CParsedStatic::SetValue(str);
}
    
//-----------------------------------------------------------------------------
long CElapsedTimeStatic::GetValue()
{                          
	DataLng aTime;
    CString strTime;
             
	CParsedCtrl::GetValue(strTime);
	if (::GetElapsedTime(aTime, strTime, m_nFormatIdx, m_pFormatContext))
		return (long)aTime;

	return BAD_TIME;
}

//-----------------------------------------------------------------------------
void CElapsedTimeStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataLng &)aValue));
}

//-----------------------------------------------------------------------------
void CElapsedTimeStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataLng &)aValue).Assign(GetValue());
}

// Spin controls will send scroll messages
//
//-----------------------------------------------------------------------------
void CElapsedTimeStatic::DoSpinScroll(UINT nSBCode)	//@@ElapsedTime @@TODOTIME
{                  
	long nDelta; 
	
	switch (nSBCode)
	{
		case SB_LINEDOWN	: nDelta = -1; break;
		case SB_PAGEDOWN	: nDelta = -1; break;
		case SB_LINEUP		: nDelta = +1; break;
		case SB_PAGEUP		: nDelta = +1; break;
		default				: return;
	}	

	//Get the number in the control.
	long nOld = GetValue() + nDelta;

	if ((nOld < m_lMin && nDelta < 0) || (nOld > m_lMax && nDelta > 0))
		BadInput();
	else
	{
		SetValue(nOld);
		SetModifyFlag(TRUE);
		UpdateCtrlData(TRUE);
	}
}

//-----------------------------------------------------------------------------
LRESULT CElapsedTimeStatic::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	if (m_nErrorID == 0)
		SetValue(m_nCurTime);

	if (m_pCaption)
	{
		CElapsedTimeFormatter* pFormatter = NULL;
		if (m_nFormatIdx >= 0)
			pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			SetCaption((Token)pFormatter->GetCaptionPos());
		else
			SetCaption(T_DEFAULT);
	}

	return 0L;
}

//------------------------------------------------------------------------------
CString CElapsedTimeStatic::GetSpecialCaption()
{
	CElapsedTimeFormatter* pFormatter = NULL;
	if (m_nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	if (pFormatter == NULL)
		return _T("");

	return pFormatter->GetShortDescription();
}

//=============================================================================
//			Class CBoolStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CBoolStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CBoolStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CBoolStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBoolStatic::CBoolStatic()
	:
	CParsedStatic	()
{}
    
//-----------------------------------------------------------------------------
CBoolStatic::CBoolStatic(UINT nBtnIDBmp, DataBool* pData)
	:
	CParsedStatic	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
    
//-----------------------------------------------------------------------------
void CBoolStatic::SetValue(BOOL nValue)
{
	if (nValue)	
	{
		CParsedStatic::SetValue(DataObj::Strings::YES());
		return;
	}

	CParsedStatic::SetValue(DataObj::Strings::NO());
}

//-----------------------------------------------------------------------------
BOOL CBoolStatic::GetValue()
{
	CString	strValue;
	CParsedStatic::GetValue (strValue);

	return AfxGetCultureInfo()->IsEqual(strValue, DataObj::Strings::YES());
}

//-----------------------------------------------------------------------------
void CBoolStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataBool)));
	SetValue((BOOL)((DataBool&)aValue));
}

//-----------------------------------------------------------------------------
void CBoolStatic::GetValue(DataObj& aValue)
{
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataBool)));
	((DataBool &)aValue).Assign(GetValue());
}

//=============================================================================
//			Class CEnumStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CEnumStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CEnumStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CEnumStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CEnumStatic::CEnumStatic()
	:
	CParsedStatic	()
{
	this->m_wTag = 0;	
}
    
//-----------------------------------------------------------------------------
CEnumStatic::CEnumStatic(UINT nBtnIDBmp, DataEnum* pData)
	:
	CParsedStatic	(nBtnIDBmp)
{
	Attach(pData);
}
    
//-----------------------------------------------------------------------------
void CEnumStatic::Attach(DataObj* pDataObj)
{
	if (pDataObj)
	{
		ASSERT(pDataObj->GetDataType().m_wType == GetDataType().m_wType);
	
		SetTagValue(((DataEnum*) pDataObj)->GetTagValue());
	}
	
	CParsedCtrl::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
void CEnumStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataEnum)));

	DataEnum aEnum ((DataEnum&) aValue);
	const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(aEnum.GetTagValue());

	if (pItems == NULL)
	{
		TRACE1("ENUM %d Undefined for CEnumStatic\n", m_wTag);
		ASSERT(FALSE);
		
		return;
	}

	CParsedStatic::SetValue(pItems->GetTitle(aEnum.GetItemValue()));
}

//-----------------------------------------------------------------------------
void CEnumStatic::GetValue(DataObj& aValue)
{
	CString strText;
	CParsedStatic::GetValue(strText);
	
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataEnum)));
	((DataEnum&)aValue).Assign(strText);
}

//=============================================================================
//			Class CGuidStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CGuidStatic, CParsedStatic)

BEGIN_MESSAGE_MAP(CGuidStatic, CParsedStatic)
	//{{AFX_MSG_MAP(CGuidStatic)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CGuidStatic::CGuidStatic()
	:
	CParsedStatic ()
{}

//-----------------------------------------------------------------------------
CGuidStatic::CGuidStatic(UINT nBtnIDBmp, DataGuid* pData /* = NULL */)
	:
	CParsedStatic (nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CGuidStatic::SetValue(GUID nValue)
{
	CString strBuffer;

	CGuidFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CGuidFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
    
	if (pFormatter)
		pFormatter->Format(&nValue, strBuffer, FALSE);

	CParsedStatic::SetValue(strBuffer);
}

//-----------------------------------------------------------------------------
GUID CGuidStatic::GetValue()
{
	CString	strValue;
	CParsedCtrl::GetValue (strValue);

	if (m_nFormatIdx >= 0)
	{
		CGuidFormatter* pFormatter = (CGuidFormatter*)AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

		if (pFormatter)
			strValue = pFormatter->UnFormat(strValue);		
	}

	GUID guid;
	if (!IsGUIDStringValid(strValue, FALSE, &guid))
		return GUID_NULL;
	
	return guid;
}

//-----------------------------------------------------------------------------
void CGuidStatic::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue(((DataGuid &)aValue).GetGUID());
}

//-----------------------------------------------------------------------------
void CGuidStatic::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	((DataGuid &)aValue).Assign(GetValue());
}

