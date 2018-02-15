
#include "stdafx.h"

#include <float.h>
#include <ctype.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbWoormEngine\INPUTMNG.H>

#include <TbParser\SymTable.h>
#include <TbGenlib\Generic.h>

#include "messages.h"
#include "parsobj.h"
#include "hlinkobj.h"
#include "baseapp.h"
#include "basedoc.h"
#include "parslbx.h"
#include <TbGes\ItemSource.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
static CString StripCtrlChar(LPCTSTR lpszAssoc)
{
	// cerca eventuali caratteri di controllo (p.e. CR-LF nel
	// caso di stringhe su piu` righe) e ci sostituisce 3 punti di
	// sospensione
	int nLen = _tclen(lpszAssoc);
	int i = 0;
	for (i = 0; i < nLen; i++)
		if (lpszAssoc[i] < 0x0020) break;	// trovato carattere di controllo

	CString	strAssoc;
	if (i < nLen)
	{
		// per appendere "..." -----------------------v
		TCHAR* pszStr = strAssoc.GetBufferSetLength(i + 3);
		TB_TCSNCPY(pszStr, lpszAssoc, i);
		_tccpy(&pszStr[i], _T("..."));
		strAssoc.ReleaseBuffer();

		return strAssoc;
	}
	return lpszAssoc;
}

//=============================================================================
//			Class CTBListBox
//=============================================================================
IMPLEMENT_DYNAMIC (CTBListBox, CBCGPListBox)

BEGIN_MESSAGE_MAP(CTBListBox, CBCGPListBox)
	//{{AFX_MSG_MAP(CTBListBox)
	ON_WM_WINDOWPOSCHANGING	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTBListBox::CTBListBox()
	:
	CBCGPListBox()
{
	this->m_bVisualManagerStyle = TRUE;
}

//-----------------------------------------------------------------------------
void CTBListBox::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
}

//=============================================================================
//			Class CParsedListBox implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedListBox, CBCGPListBox)

BEGIN_MESSAGE_MAP(CParsedListBox, CBCGPListBox)
	//{{AFX_MSG_MAP(CParsedListBox)
	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_KILLFOCUS			()
	ON_WM_LBUTTONUP			()
	ON_WM_RBUTTONDOWN		()
	ON_WM_CONTEXTMENU		()
	ON_WM_KEYUP				()
	ON_WM_KEYDOWN			()
	ON_WM_ENABLE			()
	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,	OnPushButtonCtrl)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CParsedListBox::CParsedListBox()
	:
	CBCGPListBox					(),
	IDisposingSourceImpl			(this),

	m_nMaxItemsNo					(DEFAULT_COMBO_ITEMS),
	m_pManagedFillListBoxFuncPtr	(NULL)
{
	CParsedCtrl::Attach(this);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;
}
                        
//-----------------------------------------------------------------------------
CParsedListBox::CParsedListBox(UINT nBtnIDBmp, DataObj* pData /*= NULL*/)
	:
	CBCGPListBox					(),
	IDisposingSourceImpl			(this),
	CParsedCtrl						(pData),
	m_nMaxItemsNo					(DEFAULT_COMBO_ITEMS),
	m_pManagedFillListBoxFuncPtr	(NULL)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::Attach(nBtnIDBmp);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;
}
                        
// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedListBox::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{                 
	dwStyle |= LBS_NOTIFY | LBS_HASSTRINGS;

	BOOL bOk = CheckControl(nID, pParentWnd);
	if (!bOk) return FALSE;

	if (dwStyle & WS_EX_CLIENTEDGE)
	{
	   bOk = CreateEx(WS_EX_CLIENTEDGE, _T("ListBox"), _T(""), dwStyle,
					   rect, pParentWnd, nID); 
	}
	else
	{
		bOk = CBCGPListBox::Create(dwStyle, rect, pParentWnd, nID);
	}
	if (!bOk) return FALSE;

	bOk = CreateAssociatedButton(pParentWnd) && InitCtrl();
	return bOk;
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedListBox::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk =
			CheckControl(nID, pParentWnd, _T("LISTBOX"))	&&
			SubclassDlgItem(nID, pParentWnd)			&&
			CreateAssociatedButton(pParentWnd)			&&
			InitCtrl();
	
	if (bOk)
		SetNamespace(strName);
	
	return bOk;
}
//-----------------------------------------------------------------------------
int	 CParsedListBox::GetMaxItemsNo()	const 
{
	return m_pItemSource ? m_pItemSource->GetMaxItemsNo() : m_nMaxItemsNo; 
}

//-----------------------------------------------------------------------------
BOOL CParsedListBox::IsValidItemListBox(const DataObj& aValue)
{
	return m_pItemSource ? m_pItemSource->IsValidItem(aValue) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CParsedListBox::OnInitCtrl()
{
	VERIFY(CParsedCtrl::OnInitCtrl());
	
	if (!AfxGetThemeManager()->GetControlsUseBorders())
		GetCtrlCWnd()->ModifyStyle(WS_BORDER,0);

	CBCGPListBox::ResetContent();
	m_DataAssociations.RemoveAll();

	SetCurSel(-1);

	FillListBox();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
}

//-----------------------------------------------------------------------------
BOOL CParsedListBox::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == (UINT)GetCtrlID() && hWndCtrl != NULL)
	{
		// control notification
		ASSERT(::IsWindow(hWndCtrl));

		switch (nCode)
		{                                                  
			case LBN_SELCHANGE	:
		    {
				SetModifyFlag(TRUE);
				if	(
						(GetStyle() & LBS_EXTENDEDSEL) != LBS_EXTENDEDSEL	&&
						(GetStyle() & LBS_MULTIPLESEL) != LBS_MULTIPLESEL
					)
					UpdateCtrlData(TRUE);

				return TRUE;
			}
	
			case LBN_DBLCLK		:	return TRUE;

			case LBN_SELCANCEL	:	return TRUE;
		}
	}

	BOOL bDone = CBCGPListBox::OnCommand(wParam, lParam);

	//guardo se è un comando di menu o un accelleratore
	CWnd* pParent = GetCtrlParent();
	CBaseDocument* pDoc  = GetDocument ();
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
void CParsedListBox::OnEnable(BOOL bEnable)
{
	CBCGPListBox::OnEnable(bEnable);
	
	DoEnable(bEnable);
	
	FillListBox();
}


//-----------------------------------------------------------------------------
void CParsedListBox::OnKillFocus (CWnd* pWnd)
{
	if (!IsAssociatedButton(pWnd))
		DoKillFocus(pWnd);
                           
	// standard action
	CBCGPListBox::OnKillFocus(pWnd);
}
                                     
//-----------------------------------------------------------------------------
void CParsedListBox::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (DoKeyUp(nChar))
		return;
	
	CBCGPListBox::OnKeyUp(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{       
	if (DoKeyDown(nChar))
		return;
	
	CBCGPListBox::OnKeyDown(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	if (!CParsedCtrl::DoRButtonDown(nFlag, mousePos))
		CBCGPListBox::OnRButtonDown(nFlag, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CBCGPListBox::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnLButtonUp(UINT nFlag, CPoint mousePos)
{
	CBCGPListBox::OnLButtonUp (nFlag, mousePos);

	if (m_nButtonIDBmp != BTN_SPIN_ID)
	 	return;
	
	NotifyToParent(EN_SPIN_RELEASED);
}

//-----------------------------------------------------------------------------
LRESULT CParsedListBox::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0;
}

/*
//-----------------------------------------------------------------------------
void CParsedListBox::OnSearchOnLinkUpper()
{
	CParsedCtrl::DoPushButtonCtrl(0, 0);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnSearchOnLinkLower()
{
	CParsedCtrl::DoPushButtonCtrl(1, 0);
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnCallLink	()
{
	CParsedCtrl::DoCallLink();
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnEditAutomaticExpression	()
{
	//@@TODOAUTOEXPR
	DoEditAutomaticExpression();
}

//-----------------------------------------------------------------------------
void CParsedListBox::OnBehavior	()
{
	// se sono nel documento (sono sicuramente in stato di FIND) devo permettere l'inserimento
	// del datoobj nella WHERE CLAUSE in base al flag che ha scelto l'utente
	if (GetDocument() && GetCtrlData())
	{
		GetCtrlData()->Clear();
		GetCtrlData()->SetValueChanged(!GetCtrlData()->IsValueChanged());
	}
}
*/

//-----------------------------------------------------------------------------
void CParsedListBox::SetFillListBoxFuncPtr(FILLLISTBOX_FUNC value)
{
	m_pManagedFillListBoxFuncPtr = value;
}

//-----------------------------------------------------------------------------
void CParsedListBox::FillListBox()
{
//	if (!IsWindowEnabled())
//	{
////@@		// cleanup parziale della ListBox (vedi piu` avanti)
////@@		ResetAssociations ();
//		return;
//	}
//
	SetRedraw(FALSE);
	
	// cleanup completo della ListBox
	ResetAssociations (TRUE);
	if (m_pItemSource)
	{
		CStringArray descriptions;
		DataArray values;
		CString strData = GetCtrlData() ? GetCtrlData()->Str() : _T("");
		m_pItemSource->GetData(values, descriptions, strData);
		if (values.GetSize() == descriptions.GetSize())
		{
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
	// chiama la virtual function
 	OnFillListBox ();

//TODO Germano:	Non si capisce perche`, ma non ritorna lo stile WS_HSCROLL anche
// se impostato nella risorsa
//
//	if ((GetStyle() & WS_HSCROLL) == WS_HSCROLL)
	{
		HFONT hFont = (HFONT)SendMessage(WM_GETFONT);
		CFont *pFont = CFont::FromHandle(hFont);
		ASSERT(pFont);

		CDC* pDC = GetDC();
		CFont* pPrevFont = pDC->SelectObject(pFont);

		int nLongestExtent = 0;

		CString str;
		CSize newExtent;

		for(int i = 0; i < GetCount(); i++)
		{
			GetText(i, str);
			newExtent = pDC->GetTextExtent(str, str.GetLength());

			if (newExtent.cx > nLongestExtent)
				nLongestExtent = newExtent.cx;
		}

		pDC->SelectObject(pPrevFont);
		ReleaseDC(pDC);

		SetHorizontalExtent(nLongestExtent);
	}

	SetRedraw(TRUE);
	Invalidate(FALSE);
	
	// setta il corrente item
	if (GetCtrlData())
		DoSetCurSel(*GetCtrlData());
}

// In caso di HotKeyLink associato riempie la listbox con i dati estratti dalla
// tabella di database
//-----------------------------------------------------------------------------
void CParsedListBox::OnFillListBox()
{
	if (m_pManagedFillListBoxFuncPtr)
	{
		m_pManagedFillListBoxFuncPtr();
		return;
	}

	if (GetCtrlData() == NULL || m_pHotKeyLink == NULL || !m_pHotKeyLink->IsFillListBoxEnabled())
		return;

	// chiedo all'hotlink di eseguire la query e di restituirmi i dati per la combo
	DataObjArray aKeys; CStringArray aDescriptions;
	int nResult = m_pHotKeyLink->DoSearchComboQueryData(GetMaxItemsNo(), aKeys, aDescriptions);
	
	if	(!nResult || !aKeys.GetSize() || aKeys.GetSize() != aDescriptions.GetSize())
		return;		
	
	// aggiungo il messaggio di raggiunto nr. massimo di elementi
	if (nResult == 2)
		AddAssociation(cwsprintf(FormatMessage(MAX_ITEM_REACHED), GetMaxItemsNo()), *GetCtrlData()->DataObjClone());

	// ora riempo la combo controllando il nr. di items massimo
	for (int i=0; i < aKeys.GetSize (); i++)
		AddAssociation(aDescriptions.GetAt(i), *aKeys.GetAt(i));
}

//-----------------------------------------------------------------------------
void CParsedListBox::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	if (!DoSetCurSel(aValue))
		DoSetValue(aValue);
}

//-----------------------------------------------------------------------------
void CParsedListBox::SetValue(LPCTSTR pszValue)
{
	// si prova a cercare il dataobj nell'array
	//
	for (int idx = 0; idx < m_DataAssociations.GetSize(); idx++)
		if (_tcscmp(FormatData(m_DataAssociations[idx]), pszValue) == 0)
		{
			SetCurSel(idx);
			return;
		}

	// azione di default
	SetCurSel(FindStringExact(-1, pszValue));
}

//-----------------------------------------------------------------------------
void CParsedListBox::GetValue(CString& strBuffer)
{
	strBuffer.Empty();
	int idx = GetCurSel();
	if (idx >= 0 && idx < GetCount())
		GetText(idx, strBuffer);
}

//-----------------------------------------------------------------------------
void CParsedListBox::GetValue(DataObj& aValue)
{
	int idx = GetCurSel();
	if (m_DataAssociations.GetSize() && idx >= 0 && idx < GetCount())
	{
		DataObj* pTmpDataObj = GetDataObjFromIdx(idx);
	
		if (pTmpDataObj)
			aValue.Assign(*pTmpDataObj);
			
		return;
	}

	CString strBuffer;
	GetValue(strBuffer);
	aValue.Assign(strBuffer);
}

//-----------------------------------------------------------------------------
BOOL CParsedListBox::DoSetCurSel(const DataObj& aValue)
{
	if (m_DataAssociations.GetSize() == 0)
		return FALSE;
		
	int nIdx = GetIdxFromDataObj(aValue);
	if (nIdx < 0 && nIdx > m_DataAssociations.GetUpperBound())
		return FALSE;

	// setta il corrente item
	SetCurSel(nIdx);
	return TRUE;
}

//	Add an Association between a dataObj and a String. The user looks only
//	the Association string.
//
//-----------------------------------------------------------------------------
int CParsedListBox::AddAssociation(LPCTSTR lpszAssoc, const DataObj& dataObj)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (!IsValidItemListBox(dataObj))
		return -1;
		
	int nLbIdx = CBCGPListBox::AddString(StripCtrlChar(lpszAssoc));
	
	// add the dataObj value in an association array
	m_DataAssociations.InsertAt(nLbIdx, dataObj.DataObjClone());
	
	return nLbIdx;
}
//-----------------------------------------------------------------------------
void CParsedListBox::SetItemSource(IItemSource* pItemSource)
{
	__super::SetItemSource(pItemSource);
	if (GetDocument() && GetDocument()->GetDesignMode() != CBaseDocument::DM_STATIC)
		FillListBox();
}
//-----------------------------------------------------------------------------
void CParsedListBox::ResetAssociations(BOOL)
{
	ASSERT(m_hWnd);

	// il SetRedraw() e` gestito dal chiamante
	CBCGPListBox::ResetContent();
	m_DataAssociations.RemoveAll();
}

/*@@
//-----------------------------------------------------------------------------
void CParsedListBox::ResetAssociations(BOOL bRemoveAll)
{
	ASSERT(m_hWnd);

	int nCur = GetCurSel();
	
	if (bRemoveAll || nCur < 0 || nCur > m_DataAssociations.GetUpperBound())
	{	
		// il SetRedraw() e` gestito dal chiamante
		CBCGPListBox::ResetContent();
		m_DataAssociations.RemoveAll();
		return;
	}
	
	//
	// Serve per lasciare solo il/gli elemento/i attualmente selezionato/i:
	// tuttavia l'effetto non e` bello (ad esempio quando si browsa non vengono
	// visualizzati gli item selezionati dal nuovo dato diverso dal precedente).
	// Lasciato per futura memoria
	//

	SetRedraw(FALSE);

	if	(
			(GetStyle() & LBS_MULTIPLESEL) == LBS_MULTIPLESEL |
			(GetStyle() & LBS_EXTENDEDSEL) == LBS_EXTENDEDSEL
		)
	{
	    int nItems = GetSelCount();
		
	    if (nItems > 0)
		{
		    int* pItemVector = new int[nItems];
			GetSelItems(nItems, pItemVector);
							
			// la scansione e` a ritroso poiche` gli array vengono impaccati
			for (int i = GetCount() - 1; i >= 0; i--)
			{
				for (int j = nItems - 1; j  >= 0; j--)
					if (i == pItemVector[j]) break;

				// non e` tra quelli selezionati quindi lo si cancella
				if (j < 0)
				{
					DeleteString(i);
					m_DataAssociations.RemoveAt(i);
				}
			}

			delete []pItemVector;
		}
	}
	else
	{
		// la scansione e` a ritroso poiche` gli array vengono impaccati
		for (int i = GetCount() - 1; i >= 0; i--)
			if (i != nCur)
			{
				DeleteString(i);
				m_DataAssociations.RemoveAt(i);
			}

		// a questo punto l'unico elemento rimasto e` lo 0-esimo
		// e quindi e` necessario segnalare alla ListBox quale l'effettivo
		// item selezionato
	}
	
	SetRedraw(TRUE);
	Invalidate(FALSE);
}
*/

//-----------------------------------------------------------------------------
int CParsedListBox::GetIdxFromDataObj(const DataObj& dataObj)
{
	int numItm = m_DataAssociations.GetSize();
	for (int idx = 0; idx < numItm; idx++)
	{
		if (m_DataAssociations.GetAt(idx)->IsEqual( dataObj ))
			return idx;
	}
	
	return -1;
}

//-----------------------------------------------------------------------------
DataObj* CParsedListBox::GetDataObjFromIdx(int nIdx)
{
	if (nIdx >= 0 && nIdx < m_DataAssociations.GetSize())
		return m_DataAssociations.GetAt(nIdx);
	
	return NULL;
}

//=============================================================================
//			Class CStrListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CStrListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CStrListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CStrListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CStrListBox::CStrListBox()
	:
	CParsedListBox	()
{}
                        
//-----------------------------------------------------------------------------
CStrListBox::CStrListBox(UINT nBtnIDBmp, DataStr* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
                        
//-----------------------------------------------------------------------------
CString CStrListBox::GetValue()
{
	DataStr aValue;

	CParsedListBox::GetValue(aValue);
	return aValue.GetString();
}

//-----------------------------------------------------------------------------
void CStrListBox::DoSetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	SetValue(aValue.Str());
}

//-----------------------------------------------------------------------------
int CStrListBox::AddAssociation(LPCTSTR lpszAssoc, LPCTSTR pszValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataStr(pszValue));
}

// intercetta l'evento di drop files nel controllo
//-----------------------------------------------------------------------------
void CStrListBox::OnDropFiles(CStringArray* pDroppedFiles)
{
	if (!pDroppedFiles)
		return;

	for (int i = 0; i <= pDroppedFiles->GetUpperBound(); i++)
		AddString(pDroppedFiles->GetAt(i));
}

//=============================================================================
//			Class CIntListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CIntListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CIntListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CStrListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CIntListBox::CIntListBox()
	:
	CParsedListBox	()
{}
                        
//-----------------------------------------------------------------------------
CIntListBox::CIntListBox(UINT nBtnIDBmp, DataInt* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
                        
//-----------------------------------------------------------------------------
void CIntListBox::SetValue(int nValue)
{
	const rsize_t nLen = 15;
	TCHAR szBuffer[nLen];

	_stprintf_s(szBuffer, nLen, _T("%d"),nValue);
	CParsedListBox::SetValue(szBuffer);
}

//-----------------------------------------------------------------------------
int CIntListBox::GetValue()
{
	DataInt aValue;

	CParsedListBox::GetValue(aValue);
	return (int)aValue;
}

//-----------------------------------------------------------------------------
void CIntListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (int) (DataInt&) aValue );
}

//-----------------------------------------------------------------------------
int CIntListBox::AddAssociation(LPCTSTR lpszAssoc, int nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataInt(nValue));
}

//=============================================================================
//			Class CLongListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CLongListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CLongListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CLongListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLongListBox::CLongListBox()
	:
	CParsedListBox	()
{}

//-----------------------------------------------------------------------------
CLongListBox::CLongListBox(UINT nBtnIDBmp, DataLng* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CLongListBox::SetValue(long nValue)
{
	const rsize_t nLen = 15;
	TCHAR szBuffer[nLen];

	_stprintf_s(szBuffer, nLen, _T("%ld"),nValue);
	CParsedListBox::SetValue((LPCTSTR) szBuffer);
}

//-----------------------------------------------------------------------------
long CLongListBox::GetValue()
{
	DataLng	aValue;

	CParsedListBox::GetValue (aValue);
	return (long)aValue;
}

//-----------------------------------------------------------------------------
void CLongListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (long) (DataLng&) aValue );
}

//-----------------------------------------------------------------------------
int CLongListBox::AddAssociation(LPCTSTR lpszAssoc, long nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataLng(nValue));
}

//=============================================================================
//			Class CDoubleListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDoubleListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CDoubleListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CDoubleListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDoubleListBox::CDoubleListBox()
	:
	CParsedListBox	(),
	m_nDec			(DEFAULT_N_DEC)
{}

//-----------------------------------------------------------------------------
CDoubleListBox::CDoubleListBox(UINT nBtnIDBmp, DataDbl* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData),
	m_nDec			(DEFAULT_N_DEC)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
int CDoubleListBox::GetCtrlNumDec ()
{
	CDblFormatter* pFormatte = NULL;
	
	if (m_nDec < 0 && m_nFormatIdx >= 0)
		pFormatte = (CDblFormatter*) AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);
		
	if (pFormatte)
		return pFormatte->GetDecNumber();
	
	return m_nDec;
}

//-----------------------------------------------------------------------------
void CDoubleListBox::SetValue(double nValue)
{
	const rsize_t nLen = 35;
	TCHAR szBuffer[nLen];

	int nDec = GetCtrlNumDec();
	if (nDec >= 0)
		_stprintf_s(szBuffer, nLen, _T("%.*f"), nDec, nValue);
	else
		_stprintf_s(szBuffer, nLen, _T("%f"), nValue);

	CParsedListBox::SetValue((LPCTSTR) szBuffer);
}

//-----------------------------------------------------------------------------
void CDoubleListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (double) (DataDbl&) aValue );
}

//-----------------------------------------------------------------------------
double CDoubleListBox::GetValue()
{
	DataDbl aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CDoubleListBox::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CParsedListBox::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CDoubleListBox::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataDbl(nValue));
}

//-----------------------------------------------------------------------------
CString CDoubleListBox::FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const
{
	ASSERT_VALID(pDataObj);
	CString strCell;

	CDblFormatter* pFormatter = NULL;

	if (m_nFormatIdx >= 0)
		pFormatter = (CDblFormatter*) (AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext));

	if (pFormatter)
	{
		int nOldDN;
		//se il programmatore ha scelto un numero di decimali diverso da quello del formattatore
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
//			Class CMoneyListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMoneyListBox, CDoubleListBox)

BEGIN_MESSAGE_MAP(CMoneyListBox, CDoubleListBox)
	//{{AFX_MSG_MAP(CMoneyListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMoneyListBox::CMoneyListBox()
	:
	CDoubleListBox()
{}

//-----------------------------------------------------------------------------
CMoneyListBox::CMoneyListBox(UINT nBtnIDBmp, DataMon* pData)
	:
	CDoubleListBox(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CMoneyListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleListBox::SetValue( (double) (DataMon&) aValue );
}

//-----------------------------------------------------------------------------
double CMoneyListBox::GetValue()
{
	DataMon aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CMoneyListBox::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CParsedListBox::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CMoneyListBox::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataMon(nValue));
}

//=============================================================================
//			Class CQuantityListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CQuantityListBox, CDoubleListBox)

BEGIN_MESSAGE_MAP(CQuantityListBox, CDoubleListBox)
	//{{AFX_MSG_MAP(CQuantityListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CQuantityListBox::CQuantityListBox()
	:
	CDoubleListBox()
{}

//-----------------------------------------------------------------------------
CQuantityListBox::CQuantityListBox(UINT nBtnIDBmp, DataQty* pData)
	:
	CDoubleListBox(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CQuantityListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleListBox::SetValue( (double) (DataQty&) aValue );
}

//-----------------------------------------------------------------------------
double CQuantityListBox::GetValue()
{
	DataQty aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CQuantityListBox::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CParsedListBox::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CQuantityListBox::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataQty(nValue));
}

//=============================================================================
//			Class CPercListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPercListBox, CDoubleListBox)

BEGIN_MESSAGE_MAP(CPercListBox, CDoubleListBox)
	//{{AFX_MSG_MAP(CPercListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPercListBox::CPercListBox()
	:
	CDoubleListBox()
{}

//-----------------------------------------------------------------------------
CPercListBox::CPercListBox(UINT nBtnIDBmp, DataPerc* pData)
	:
	CDoubleListBox(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}

//-----------------------------------------------------------------------------
void CPercListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CDoubleListBox::SetValue( (double) (DataPerc&) aValue );
}

//-----------------------------------------------------------------------------
double CPercListBox::GetValue()
{
	DataPerc aValue;

	GetValue (aValue);
	
	return (double) aValue;
}

//-----------------------------------------------------------------------------
void CPercListBox::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	CParsedListBox::GetValue (aValue);
}

//-----------------------------------------------------------------------------
int CPercListBox::AddAssociation(LPCTSTR lpszAssoc, double nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataPerc(nValue));
}

//=============================================================================
//			Class CDateListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CDateListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CDateListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CDateListBox)
	ON_MESSAGE	(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDateListBox::CDateListBox()
	:
	CParsedListBox	()
{}

//-----------------------------------------------------------------------------
CDateListBox::CDateListBox(UINT nBtnIDBmp, DataDate* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
	ASSERT(pData == NULL || pData->GetDataType() == GetDataType());
}

//-----------------------------------------------------------------------------
void CDateListBox::SetValue(long nValue)
{
 	WORD nDay, nMonth, nYear;          

    ::GetShortDate(nDay, nMonth, nYear, nValue);
	CParsedListBox::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
}
    
//-----------------------------------------------------------------------------
void CDateListBox::SetValue(WORD nDay, WORD nMonth, WORD nYear)
{
	CParsedListBox::SetValue(::FormatDate(nDay, nMonth, nYear, m_nFormatIdx));
}

//-----------------------------------------------------------------------------
long CDateListBox::GetValue()
{                          
    DataDate aDate;
             
	CParsedListBox::GetValue(aDate);
	return (long)aDate;
}

//-----------------------------------------------------------------------------
void CDateListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue((long) ((DataDate &)aValue));
}

//-----------------------------------------------------------------------------
LRESULT CDateListBox::OnFormatStyleChange (WPARAM /* wParam */, LPARAM /* lParam */)
{
	FillListBox();
	return 0L;
}

//-----------------------------------------------------------------------------
int CDateListBox::AddAssociation(LPCTSTR lpszAssoc, long nValue)
{
	return CParsedListBox::AddAssociation(lpszAssoc, DataDate(nValue));
}

//=============================================================================
//			Class CBoolListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CBoolListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CBoolListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CBoolListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBoolListBox::CBoolListBox()
	:
	CParsedListBox	()
{}
    
//-----------------------------------------------------------------------------
CBoolListBox::CBoolListBox(UINT nBtnIDBmp, DataBool* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
    
//-----------------------------------------------------------------------------
void CBoolListBox::SetValue(BOOL bValue)
{
	if (bValue)	
	{
		CParsedListBox::SetValue(DataObj::Strings::YES());
		return;
	}

	CParsedListBox::SetValue(DataObj::Strings::NO());
}

//-----------------------------------------------------------------------------
BOOL CBoolListBox::GetValue()
{
	DataBool	aValue;

	CParsedListBox::GetValue (aValue);
	return (BOOL) aValue;
}

//-----------------------------------------------------------------------------
void CBoolListBox::DoSetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	SetValue( (BOOL) (DataBool&) aValue );
}

//-----------------------------------------------------------------------------
void CBoolListBox::OnFillListBox()
{
	CParsedListBox::AddAssociation(DataObj::Strings::YES(), DataBool(TRUE));
	CParsedListBox::AddAssociation(DataObj::Strings::NO(), DataBool(FALSE));
}

//=============================================================================
//			Class CEnumListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CEnumListBox, CParsedListBox)

BEGIN_MESSAGE_MAP(CEnumListBox, CParsedListBox)
	//{{AFX_MSG_MAP(CEnumListBox)
	ON_WM_CONTEXTMENU	()
	ON_WM_RBUTTONDOWN	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CEnumListBox::CEnumListBox()
	:
	CParsedListBox()
{
	m_bShowEnumValue = FALSE;
}
                        
//-----------------------------------------------------------------------------
CEnumListBox::CEnumListBox(UINT nBtnIDBmp, DataEnum* pData)
	:
	CParsedListBox	(nBtnIDBmp, pData)
{
	// NON si puo` usare il metodo CParsedCtrl::Attach(DataObj*) poiche`
	// esso controllerebbe il tipo del DataObj chiamando il metodo virtuale GetDataType()
	// cosa impossibile  trovandoci in un costruttore
	//
}
                        
//-----------------------------------------------------------------------------
void CEnumListBox::Attach(DataObj* pDataObj)
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

		SetTagValue(wTag);
	}

	CParsedCtrl::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
void CEnumListBox::OnFillListBox()
{
	const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(m_wTag);

	if (pItems == NULL)
	{
		TRACE("ENUM %d Undefined for CEnumListBox\n", m_wTag);
		ASSERT(FALSE);
		
		return;
	}

	ResetContent();
	
	m_DataAssociations.RemoveAll();

	for (int i = 0; i <= pItems->GetUpperBound(); i++)
	{
		EnumItem* pItem = pItems->GetAt(i);
		ASSERT(pItem);
		if (pItem->IsHidden())
			continue;

		if (m_bShowEnumValue)
			AddAssociation(
							cwsprintf(_T("{0-%s} --> {1-%05d} ({2-%08ld})"), pItem->GetTitle(), pItem->GetItemValue(), GET_TI_VALUE(pItem->GetItemValue(), m_wTag))/*pItem->GetTitle()*/,
							DataEnum(m_wTag, pItem->GetItemValue())
						   );
		else
			AddAssociation(pItem->GetTitle(), DataEnum(m_wTag, pItem->GetItemValue()));
	}

	Invalidate();
	GetParent()->Invalidate();
}

//-----------------------------------------------------------------------------
void CEnumListBox::DoSetValue(const DataObj&)
{
	ASSERT(FALSE);
}
//---------------------------------------------------------------------------
void CEnumListBox::OnRButtonDown(UINT nFlags, CPoint point) 
{
	OnContextMenu(this, point);
}
//-----------------------------------------------------------------------------
void CEnumListBox::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CBCGPListBox::OnContextMenu(pWnd, mousePos);
}
//-----------------------------------------------------------------------------
BOOL CEnumListBox::OnCommand(WPARAM wParam, LPARAM lParam)
{
	CParsedCtrl::DoCommand(wParam, lParam);
	return TRUE;
}
//-----------------------------------------------------------------------------
void CEnumListBox::DoShowEnumValue()
{
	if (m_bShowEnumValue)
		m_bShowEnumValue = FALSE;
	else
		m_bShowEnumValue = TRUE;
	
	OnFillListBox();
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CResizableListBox
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CResizableListBox, CBCGPListBox)

BEGIN_MESSAGE_MAP(CResizableListBox, CBCGPListBox)
	//{{AFX_MSG_MAP(CResizableListBox)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CResizableListBox::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (! CBCGPListBox::SubclassDlgItem(IDC, pParent))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CResizableListBox::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//------------------------------------------------------------------------------
void  CResizableListBox::CalcHorizontalExtent()
{
	CString     str;
	CSize		sz;
	int			dx = 0;
	TEXTMETRIC  tm;
	CDC*		pDC		= GetDC();
	CFont*		pFont	= GetFont();

	// Select the listbox font, save the old font
	CFont* pOldFont = pDC->SelectObject(pFont);
	// Get the text metrics for avg char width
	pDC->GetTextMetrics(&tm); 

	for (int n = 0; n < GetCount(); n++)
	{
		GetText(n, str);
		sz = pDC->GetTextExtent(str);

	// Add the avg width to prevent clipping
		sz.cx += tm.tmAveCharWidth;

		if (sz.cx > dx)
			dx = sz.cx;
	}
	// Select the old font back into the DC
	pDC->SelectObject(pOldFont);
	ReleaseDC(pDC);

	// Set the horizontal extent so every character of all strings 
	// can be scrolled to.
	SetHorizontalExtent(dx);
}

//===============================================================================

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CResizableMultiListBox
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CResizableMultiListBox, CMultiListBox)

BEGIN_MESSAGE_MAP(CResizableMultiListBox, CMultiListBox)
	//{{AFX_MSG_MAP(CResizableMultiListBox)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CResizableMultiListBox::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (! CMultiListBox::SubclassDlgItem(IDC, pParent))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CResizableMultiListBox::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//===============================================================================
///////////////////////////////////////////////////////////////////////////////
// Implementazione di CResizableListCtrl
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CResizableListCtrl, CBCGPListCtrl)

BEGIN_MESSAGE_MAP(CResizableListCtrl, CBCGPListCtrl)
	//{{AFX_MSG_MAP(CResizableListBox)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CResizableListCtrl::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (! __super::SubclassDlgItem(IDC, pParent))
		return FALSE;
	
	ResizableCtrl::InitSizeInfo (this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CResizableListCtrl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//=============================================================================
//			Class CParsedCheckListBox implementation
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CParsedCheckListBox, CCheckListBox)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CParsedCheckListBox, CCheckListBox)

	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)

	ON_WM_ERASEBKGND		()

	ON_WM_KILLFOCUS			()
	ON_WM_LBUTTONUP			()
	ON_WM_RBUTTONDOWN		()
	ON_WM_CONTEXTMENU		()
	ON_WM_KEYUP				()
	ON_WM_KEYDOWN			()
	ON_WM_ENABLE			()

	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,	OnPushButtonCtrl)

	ON_COMMAND(ID_SELECT_ALL,		SelectAll)
	ON_COMMAND(ID_UNSELECT_ALL,		UnSelectAll)
	ON_COMMAND(ID_INVERT_SELECTION, InvertSelected)

END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CParsedCheckListBox::CParsedCheckListBox()
	:
	CCheckListBox						(),
	m_nMaxItemsNo					(DEFAULT_COMBO_ITEMS),
	m_crBkgColor(AfxGetThemeManager()->GetBackgroundColor())
{
	CParsedCtrl::Attach(this);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo     = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;
	m_Separator		  = _T(";");
}
                        
//-----------------------------------------------------------------------------
CParsedCheckListBox::CParsedCheckListBox(UINT nBtnIDBmp, DataObj* pData /*= NULL*/)
	:
	CCheckListBox					(),
	CParsedCtrl						(pData),
	m_nMaxItemsNo					(DEFAULT_COMBO_ITEMS)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::Attach(nBtnIDBmp);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo     = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*) pSetting) : (long)*((DataLng*) pSetting)) : m_nMaxItemsNo;
	m_Separator		  = _T(";");
}
                        
// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	dwStyle |= LBS_NOTIFY;

	return
		CheckControl(nID, pParentWnd) &&
		CCheckListBox::Create(dwStyle, rect, pParentWnd, nID) &&
		CreateAssociatedButton(pParentWnd) &&
		InitCtrl();
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk =
		CheckControl(nID, pParentWnd, _T("LISTBOX")) &&
		SubclassDlgItem(nID, pParentWnd) &&
		CreateAssociatedButton(pParentWnd) &&
		InitCtrl();

	if (bOk)
		SetNamespace(strName);

	return bOk;
}

//-----------------------------------------------------------------------------
int CParsedCheckListBox::GetMaxItemsNo()	const
{
	return m_pItemSource ? m_pItemSource->GetMaxItemsNo() : m_nMaxItemsNo; 
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::OnInitCtrl()
{
	VERIFY(CParsedCtrl::OnInitCtrl());

	ResizableCtrl::InitSizeInfo(this);

	::SetDefaultFontControl(this, NULL);

	if (!AfxGetThemeManager()->GetControlsUseBorders())
		GetCtrlCWnd()->ModifyStyle(WS_BORDER,0);

	ResetContent();

	SetCurSel(-1);

	CParsedForm* pParentForm = ::GetParsedForm(GetParent());
	if (pParentForm)
	{
		m_crBkgColor = pParentForm->GetBackgroundColor();
	}

	FillListBox();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::OnEraseBkgnd(CDC* pDC)
{
	if (m_bCustomDraw)
	{ 
		//if (!m_bTrasparent)
		{
			CRect rectClient;
			GetClientRect(&rectClient);
			CBrush bkg(m_crBkgColor);
			pDC->FillRect(rectClient, &bkg);
			bkg.DeleteObject();
		}
		return TRUE;
	}
	else
		return __super::OnEraseBkgnd(pDC);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	if (!m_bCustomDraw)
	{
		__super::DrawItem(lpDrawItemStruct);
		return;
	}

	//---- MFC VS12 -----------------------------------------------------------

	ASSERT((GetStyle() & (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS)) ==
		(LBS_OWNERDRAWFIXED | LBS_HASSTRINGS));

	CDC* pDC = CDC::FromHandle(lpDrawItemStruct->hDC);
	ENSURE(pDC);

	if (((LONG)(lpDrawItemStruct->itemID) >= 0) &&
		(lpDrawItemStruct->itemAction & (ODA_DRAWENTIRE | ODA_SELECT)))
	{
		int cyItem = GetItemHeight(lpDrawItemStruct->itemID);
		BOOL fDisabled = !IsWindowEnabled() || !IsEnabled(lpDrawItemStruct->itemID);

		COLORREF newTextColor = fDisabled ?
			RGB(0x80, 0x80, 0x80) : GetSysColor(COLOR_WINDOWTEXT);  // light gray
		COLORREF oldTextColor = pDC->SetTextColor(newTextColor);

		/*TB*/	COLORREF newBkColor = m_crBkgColor; //	GetSysColor(COLOR_WINDOW);

		COLORREF oldBkColor = pDC->SetBkColor(newBkColor);

		//if (newTextColor == newBkColor)
		//	newTextColor = RGB(0xC0, 0xC0, 0xC0);   // dark gray

		//if (!fDisabled && ((lpDrawItemStruct->itemState & ODS_SELECTED) != 0))
		//{
		//	pDC->SetTextColor(GetSysColor(COLOR_HIGHLIGHTTEXT));
		//	pDC->SetBkColor(GetSysColor(COLOR_HIGHLIGHT));
		//}
//#ifdef _DEBUG
//		if (m_cyText == 0)
//		{
//			int minH = CalcMinimumItemHeight();
//			ASSERT(cyItem >= minH);
//		}
//#endif
		CString strText;
		GetText(lpDrawItemStruct->itemID, strText);

		/*TB*/	int nETO = m_bTrasparent ? 0 : ETO_OPAQUE;
		if (m_bTrasparent)
			pDC->SetBkMode(TRANSPARENT);

		pDC->ExtTextOut(lpDrawItemStruct->rcItem.left,
			lpDrawItemStruct->rcItem.top + max(0, (cyItem - m_cyText) / 2),
			/*TB ETO_OPAQUE*/ nETO,
			&(lpDrawItemStruct->rcItem), strText, (int)strText.GetLength(), NULL);

		pDC->SetTextColor(oldTextColor);
		pDC->SetBkColor(oldBkColor);
	}

	if ((lpDrawItemStruct->itemAction & ODA_FOCUS) != 0)
		pDC->DrawFocusRect(&(lpDrawItemStruct->rcItem));

	//ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::OnAuxCommand(UINT nCode)
{
	switch (nCode)
	{
		case LBN_DBLCLK:
		case LBN_SELCHANGE:
		{
			SetModifyFlag(TRUE);
			if (
				(GetStyle() & LBS_EXTENDEDSEL) != LBS_EXTENDEDSEL &&
				(GetStyle() & LBS_MULTIPLESEL) != LBS_MULTIPLESEL
				)
				UpdateCtrlData(TRUE);

			return TRUE;
		}

		//case LBN_DBLCLK:	return TRUE;

		case LBN_SELCANCEL:	return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == (UINT)GetCtrlID() && hWndCtrl != NULL)
	{
		// control notification
		ASSERT(::IsWindow(hWndCtrl));

		if (OnAuxCommand(nCode))
			return TRUE;
	}

	BOOL bDone = CCheckListBox::OnCommand(wParam, lParam);

	//guardo se è un comando di menu o un accelleratore
	CWnd* pParent = GetCtrlParent();
	CBaseDocument* pDoc  = GetDocument ();
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
void CParsedCheckListBox::OnEnable(BOOL bEnable)
{
	CCheckListBox::OnEnable(bEnable);
	
	DoEnable(bEnable);
	
	//???? FillListBox();
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnKillFocus (CWnd* pWnd)
{
	if (!IsAssociatedButton(pWnd))
		DoKillFocus(pWnd);
                           
	// standard action
	CCheckListBox::OnKillFocus(pWnd);
}
                                     
//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (DoKeyUp(nChar))
		return;
	
	CCheckListBox::OnKeyUp(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{       
	if (DoKeyDown(nChar))
		return;
	
	CCheckListBox::OnKeyDown(nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	if (!CParsedCtrl::DoRButtonDown(nFlag, mousePos))
		CCheckListBox::OnRButtonDown(nFlag, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		CCheckListBox::OnContextMenu(pWnd, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnLButtonUp(UINT nFlag, CPoint mousePos)
{
	CCheckListBox::OnLButtonUp (nFlag, mousePos);

	if (m_nButtonIDBmp != BTN_SPIN_ID)
	 	return;
	
	NotifyToParent(EN_SPIN_RELEASED);
}

//-----------------------------------------------------------------------------
LRESULT CParsedCheckListBox::OnPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	DoPushButtonCtrl(wParam, lParam);
	return (LRESULT) 0;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::SetFillListBoxFuncPtr(FILLLISTBOX_FUNC value)
{
	m_pManagedFillListBoxFuncPtr = value;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::FillListBox()
{
	if (!IsWindowEnabled())
	{
		return;
	}
	if (m_pManagedFillListBoxFuncPtr)
	{
		return;
	}
	if (m_pHotKeyLink == NULL || !m_pHotKeyLink->IsFillListBoxEnabled())
		return;

	SetRedraw(FALSE);
	
	// cleanup completo della ListBox
	ResetAssociations ();

	// chiama la virtual function
 	OnFillListBox ();

	HFONT hFont = (HFONT)SendMessage(WM_GETFONT);
	CFont *pFont = CFont::FromHandle(hFont);
	ASSERT(pFont);

	CDC* pDC = GetDC();
	CFont* pPrevFont = pDC->SelectObject(pFont);

	int nLongestExtent = 0;

	CString str;
	CSize newExtent;

	for(int i = 0; i < GetCount(); i++)
	{
		GetText(i, str);
		newExtent = pDC->GetTextExtent(str, str.GetLength());

		if (newExtent.cx > nLongestExtent)
			nLongestExtent = newExtent.cx;
	}

	pDC->SelectObject(pPrevFont);
	ReleaseDC(pDC);

	__super::SetHorizontalExtent(nLongestExtent);

	SetRedraw(TRUE);
	Invalidate(FALSE);
	
	//// setta il corrente item
	//TODO if (GetCtrlData())
	//	DoSetCurSel(*GetCtrlData());
}

// In caso di HotKeyLink associato riempie la listbox con i dati estratti dalla
// tabella di database
//-----------------------------------------------------------------------------
void CParsedCheckListBox::OnFillListBox()
{
	if (m_pManagedFillListBoxFuncPtr)
	{
		m_pManagedFillListBoxFuncPtr();
		return;
	}

	if (/*GetCtrlData() == NULL || */m_pHotKeyLink == NULL || !m_pHotKeyLink->IsFillListBoxEnabled())
		return;

	// chiedo all'hotlink di eseguire la query e di restituirmi i dati per la combo
	CStringArray aDescriptions;
	int nResult = m_pHotKeyLink->DoSearchComboQueryData(GetMaxItemsNo(), m_DataAssociations.GetData(), aDescriptions);
	
	if	(!nResult || !m_DataAssociations.GetSize() || m_DataAssociations.GetSize() != aDescriptions.GetSize())
	{
		ASSERT(m_DataAssociations.GetSize() == aDescriptions.GetSize());

		if (m_DataAssociations.GetSize() == 0)
		{
			DataBool* pDummy = new DataBool();
			m_pData->SetAlwaysReadOnly();

			m_DataAssociations.GetData().InsertAt(0, pDummy, 1);
			InsertAssociation(0, cwsprintf(FormatMessage(HOTLINK_NO_DATA_FOUND)), pDummy);
		}
		return;
	}		

	for (int i = 0; i < m_DataAssociations.GetSize(); )
	{
		if (AddAssociationUnsorted(aDescriptions.GetAt(i), m_DataAssociations[i]) < 0)
		{
			m_DataAssociations.RemoveAt(i); aDescriptions.RemoveAt(i);
		}
		else i++;
	}
	if (nResult == 2)	// aggiungo il messaggio di raggiunto nr. massimo di elementi
	{
		m_DataAssociations.GetData().InsertAt(0, m_DataAssociations[0]->DataObjClone());

		InsertAssociation(0, cwsprintf(FormatMessage(MAX_ITEM_REACHED), GetMaxItemsNo()), m_DataAssociations[0]);
	}
}

//-----------------------------------------------------------------------------
int CParsedCheckListBox::AddAssociationUnsorted(const CString& sAssoc, DataObj* dataObj)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (!IsValidItemListBox(*dataObj))
		return -1;

	//CParsedCheckListBox::AddAssociation(lpszAssoc, DataStr(pszValue))
	int nLbIdx = __super::AddString(sAssoc);

	return nLbIdx;
}

//-----------------------------------------------------------------------------
int CParsedCheckListBox::InsertAssociation(int nIdx, const CString& sAssoc, DataObj* dataObj)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (!IsValidItemListBox(*dataObj))
		return -1;

	int nLbIdx = __super::InsertString(nIdx, sAssoc);
	return nLbIdx;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::SetValue(const DataObj& aValue)
{
	if (aValue.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT(CheckDataObjType(&aValue));
		RefreshAllCheck(aValue.Str());
	}
	else if (aValue.IsKindOf(RUNTIME_CLASS(DataArray)))
	{
		SetArrayValue(((DataArray&)aValue).GetData());
	}
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::SetValue(LPCTSTR pszValue)
{
	RefreshAllCheck(pszValue);
	// azione di default
	SetCurSel(FindStringExact(-1, pszValue));
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::GetValue(CString& strBuffer)
{
	strBuffer.Empty();
	strBuffer = ComponeString();
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::GetValue(DataObj& aValue)
{
	if (aValue.IsKindOf(RUNTIME_CLASS(DataStr)))
		aValue.Assign(ComponeString());
	else if (aValue.IsKindOf(RUNTIME_CLASS(DataArray)))
	{
		GetArrayValue(((DataArray&)aValue).GetData());
	}
	else ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::GetArrayValue(DataObjArray& values) 
{
	values.RemoveAll();
	// Get the list count
	if (m_hWnd)
	{	
		for (INT i = 0; i < m_DataAssociations.GetSize(); i++)
		{
			if (__super::GetCheck(i))
				 values.Add(m_DataAssociations.GetAt(i)->DataObjClone());
		}
	}
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::SetArrayValue( const DataObjArray& values)
{
	if (!m_hWnd || GetCount() <= 0 || m_DataAssociations.GetSize() == 0)
		return;

	for (int i = 0; i < values.GetSize(); i++)
	{
		int nIdx = this->m_DataAssociations.Find(values.GetAt(i));
		if (nIdx >= 0)
			SetCheck(nIdx, TRUE);
	}
}

//-----------------------------------------------------------------------------
CString	CParsedCheckListBox::ComponeString()
{
	// Get the list count
	INT nCount  =  m_DataAssociations.GetUpperBound();
	CString ris = _T("");
	BOOL first = TRUE;
	for (INT i = 0; i <= nCount; i++)
	{
		if (CCheckListBox::GetCheck(i))
		{
			if (!first)
				ris += m_Separator;

			ris += m_DataAssociations.GetAt(i)->Str();
			first = FALSE;
		}
	}
	return ris;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::RefreshAllCheck(CString sItems)
{
	// se passo una stringa vuota metto d'ufficio tutti gli item unchecked
	if (sItems.IsEmpty())
	{
		for (int i = 0; i <= m_DataAssociations.GetUpperBound(); i++)
			CCheckListBox::SetCheck(i, FALSE);
		return;
	}

	// la variabile sItems contiene una lista di elementi intervallati da ;
	// per ognuno di questi vado a cercarlo nella lista degli item della listbox
	// e se lo trovo imposto il flag a checked
	int nTokenPos = 0;
	CString sToken = sItems.Tokenize(m_Separator, nTokenPos);
	while ((nTokenPos - 1) <= sItems.GetLength() && !sToken.Trim().IsEmpty())
	{
		for (int i = 0; i <= m_DataAssociations.GetUpperBound(); i++)
		{
			CString strItem = m_DataAssociations.GetAt(i)->Str();
			if (strItem.CompareNoCase(sToken) == 0)
			{
				CCheckListBox::SetCheck(i, TRUE);
				break;
			}
		}

		sToken = sItems.Tokenize(m_Separator, nTokenPos);
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage)
{
	BOOL bModified = GetModifyFlag();

	if (bModified)
	{
		DataArray* pData = dynamic_cast<DataArray*>(GetCtrlData());
		if (pData)
		{
			this->GetArrayValue(pData->GetData());
			return TRUE;
		}
		DataStr* pDataStr = dynamic_cast<DataStr*>(GetCtrlData());
		if (pDataStr)
		{
			this->GetValue(*pDataStr);
			return TRUE;
		}

	}

	return TRUE;
}

//	Add an Association between a dataObj and a String. The user looks only
//	the Association string.
//
//-----------------------------------------------------------------------------
int CParsedCheckListBox::AddAssociation(LPCTSTR lpszAssoc, const DataObj& dataObj)
{
	// se reimplementata permette di filtrare gli item da inserire nella listbox
	if (!IsValidItemListBox(dataObj))
		return -1;
		
	int nLbIdx = CCheckListBox::AddString(StripCtrlChar(lpszAssoc));
	
	// add the dataObj value in an association array
	m_DataAssociations.InsertAt(nLbIdx, dataObj.DataObjClone());
	
	return nLbIdx;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::ResetAssociations()
{
	// il SetRedraw() e` gestito dal chiamante
	if (m_hWnd) __super::ResetContent();
	m_DataAssociations.RemoveAll();
}

//-----------------------------------------------------------------------------
// Changes the checked status of the given 
// row (iRow) according to the given value (bChecked).
void CParsedCheckListBox::ChangeCheckStateColumnRow(int iRow, bool bChecked)
{
	if (iRow < 0 || iRow > this->GetCount())
	{
		// index out of bound, just return.
		return;
	}
	// set the checked value for the given row-
	this->SetCheck(iRow, bChecked);
}

//------------------------------------------------------------------------------
LRESULT CParsedCheckListBox::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::DoSetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	SetValue(aValue.Str());
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::OnShowingPopupMenu(CMenu& menu)
{
	if (GetCount() <= 0)
		return FALSE;

	if (menu.GetMenuItemCount() > 0)
		menu.AppendMenu(MF_SEPARATOR);

	menu.AppendMenu
	(
		MF_STRING,
		ID_SELECT_ALL,
		(LPCTSTR)_TB("&Select all")
	);

	menu.AppendMenu
	(
		MF_STRING,
		ID_UNSELECT_ALL,
		(LPCTSTR)_TB("&Unselect all")
	);

	menu.AppendMenu
	(
		MF_STRING,
		ID_INVERT_SELECTION,
		(LPCTSTR)_TB("&Invert selection")
	);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::SelectAll()
{
	for (int i = 0; i < GetCount(); i++) {
		CCheckListBox::SetCheck(i, TRUE);
	}

	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::UnSelectAll()
{
	for (int i = 0; i < GetCount(); i++) {
		CCheckListBox::SetCheck(i, FALSE);
	}

	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
void CParsedCheckListBox::InvertSelected()
{
	for (int i = 0; i < GetCount(); i++) {
		CCheckListBox::SetCheck(i, ! CCheckListBox::GetCheck(i));
	}

	// Redraw the window
	Invalidate(FALSE);
	NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
BOOL CParsedCheckListBox::IsSelectAll()
{
	if (GetCount() <= 0) return FALSE;

	for (int i = 0; i < GetCount(); i++) {
		if (!CCheckListBox::GetCheck(i))
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
DataObj* CParsedCheckListBox::GetCtrlData()
{
	return __super::GetCtrlData();
}

//-----------------------------------------------------------------------------
DataObj* CParsedCheckListBox::GetCtrlData(const CString& sName, int /*nRow = 0*/)
{
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (sName.CompareNoCase(m_DataAssociations.GetAt(i)->Str()) == 0)
			return SetSel(i), m_DataAssociations.GetAt(i);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
//ritorna solo i tag delle righe selezionate
int CParsedCheckListBox::EnumColumnName(CStringArray& arNames, BOOL bAll /*= TRUE*/)
{
	for (int i = 0; i < m_DataAssociations.GetSize(); i++)
	{
		if (GetSel(i)) 
			arNames.Add(m_DataAssociations.GetAt(i)->Str());
	}
	return arNames.GetSize();
}

//=============================================================================
//			Class CBoolCheckListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CBoolCheckListBox, CParsedCheckListBox)

//-----------------------------------------------------------------------------
CBoolCheckListBox::CBoolCheckListBox()
	:
	CParsedCheckListBox()
{
	m_DataAssociations.GetData().SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
CBoolCheckListBox::CBoolCheckListBox(UINT nBtnIDBmp)
	:
	CParsedCheckListBox(nBtnIDBmp)
{
	m_DataAssociations.GetData().SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::ResetContent()
{
	__super::ResetContent();
	m_TagDataAssociations.RemoveAll();
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetDataModified(BOOL bMod)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		m_DataAssociations[idx]->SetModified(bMod);
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetDataReadOnly(BOOL bRO)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		m_DataAssociations[idx]->SetReadOnly(bRO);
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetDataOSLReadOnly(BOOL bVal)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		m_DataAssociations[idx]->SetOSLReadOnly(bVal);
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetDataOSLHide(BOOL bVal)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		m_DataAssociations[idx]->SetOSLHide(bVal);
}

//-----------------------------------------------------------------------------
BOOL CBoolCheckListBox::IsDataModified()
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		if (m_DataAssociations[idx]->IsModified()) return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBoolCheckListBox::IsDataOSLReadOnly()
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		if (m_DataAssociations[idx]->IsOSLReadOnly()) return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBoolCheckListBox::IsDataOSLHide()
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		if (m_DataAssociations[idx]->IsOSLHide()) return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::UpdateCtrlView()
{
	if (m_nValueChanging)
		return;

	SetValue(*CParsedCtrl::GetCtrlData());
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::UpdateCtrlStatus()
{
	if (m_nValueChanging)
		return;

	OnUpdateCtrlStatus();

	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		Enable(idx, !m_DataAssociations[idx]->IsReadOnly());
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::ModifiedCtrlData()
{
	CParsedCtrl::ModifiedCtrlData();
}

//-----------------------------------------------------------------------------
BOOL CBoolCheckListBox::OnAuxCommand(UINT nCode)
{
	switch (nCode)
	{
		case LBN_SELCHANGE:
		case LBN_DBLCLK:
		{
			int idx = GetCurSel();
			if (idx >= 0 && idx < m_DataAssociations.GetSize())
			{
				DataBool* pdb = (DataBool*)m_DataAssociations[idx];
				ASSERT_VALID(pdb);
				if (pdb)
				{
					BOOL bOld = *pdb;
					*pdb = CCheckListBox::GetCheck(idx) == 1;

					if ((BOOL)*pdb != bOld)
					{
						SetModifyFlag(TRUE);
						UpdateCtrlData(TRUE);
					}
				}
			}
			return TRUE;
		}
		case LBN_SELCANCEL:	return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetValue(const DataObj&)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		CCheckListBox::SetCheck(idx, (BOOL) *((DataBool*)m_DataAssociations[idx]));
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::GetValue(DataObj&)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		((DataBool*)m_DataAssociations[idx])->Assign(CCheckListBox::GetCheck(idx));
}

//-----------------------------------------------------------------------------
BOOL CBoolCheckListBox::IsCurrentTag(DataBool& bDataObj)
{
	if (GetCurSel() < 0)
		return FALSE;
	return (m_DataAssociations[GetCurSel()] == &bDataObj);
}

//-----------------------------------------------------------------------------
//	Add an Association between a dataObj and a String. The user looks only
//	the Association string.
int CBoolCheckListBox::AddDataBool(LPCTSTR lpszAssoc, DataBool& dataObj)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		if (m_DataAssociations[idx] == &dataObj)
		{
			ASSERT(FALSE);
			return idx;
		}

	CString strAssoc = StripCtrlChar(lpszAssoc);

	int nLbIdx = CCheckListBox::AddString(strAssoc);
	// add the dataObj value in an association array
	m_DataAssociations.InsertAt(nLbIdx, &dataObj);

	CDC* pDC = GetDC();
	CSize sz = ::GetTextSize(pDC, strAssoc, GetFont());
	ReleaseDC(pDC);

	SetHorizontalExtent(sz.cx);

	return nLbIdx;
}

//-----------------------------------------------------------------------------
int CBoolCheckListBox::AddTagDataBool(CString aTag, LPCTSTR aStr, DataBool& bDataObj)
{
	int idx = AddDataBool(aStr, bDataObj);
	m_TagDataAssociations.InsertAt(idx, aTag);
	return idx;
}

//-----------------------------------------------------------------------------
void CBoolCheckListBox::SetTagDataBool(CString aTag, BOOL bSet)
{
	SetDataBoolAt(GetIdxFromTag(aTag), DataBool(bSet));
}

//-----------------------------------------------------------------------------
int	CBoolCheckListBox::GetIdxFromTag(CString aTag)
{
	for (int idx = 0; idx <= m_TagDataAssociations.GetUpperBound(); idx++)
		if (m_TagDataAssociations[idx] == aTag)
			return idx;

	ASSERT(FALSE);
	return -1;
}

//-----------------------------------------------------------------------------
int CBoolCheckListBox::SetDataBoolAt(int idx, DataBool& aDataBool, LPCTSTR lpcstr /* = NULL*/)
{
	if (idx < 0) 
	{
		ASSERT(FALSE);
		return -1;
	}

	int nLbIdx = idx;
	if (lpcstr)
	{
		CCheckListBox::DeleteString(idx);

		CString strAssoc = StripCtrlChar(lpcstr);

		nLbIdx = CCheckListBox::AddString(strAssoc);
		
		CDC* pDC = GetDC();
		CSize sz = ::GetTextSize(pDC, strAssoc, GetFont());
		ReleaseDC(pDC);

		SetHorizontalExtent(sz.cx);
	}

	if (nLbIdx != idx)
	{
		m_DataAssociations.RemoveAt(idx);
		m_DataAssociations.InsertAt(nLbIdx, &aDataBool);
	}
	else
		m_DataAssociations.SetAt(nLbIdx, &aDataBool);

	return nLbIdx;
}

//-----------------------------------------------------------------------------
DataObj* CBoolCheckListBox::GetCtrlData(const CString& sName, int /*nRow = 0*/)
{
	for (int i = 0; i < m_TagDataAssociations.GetSize(); i++)
	{
		if (sName.CompareNoCase(m_TagDataAssociations.GetAt(i)) == 0)
			return m_DataAssociations.GetAt(i);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
int CBoolCheckListBox::EnumColumnName(CStringArray& arNames, BOOL bAll /*= TRUE*/)
{
	arNames.RemoveAll();

	if (bAll)
	{
		arNames.Copy(m_TagDataAssociations);
	}
	else
	{
		for (int i = 0; i < m_TagDataAssociations.GetSize(); i++)
		{
			if (GetSel(i))
				arNames.Add(m_TagDataAssociations.GetAt(i));
		}
	}

	return arNames.GetSize();
}

//=============================================================================
//			Class CMultiSelectionListBox implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CMultiSelectionListBox, CResizableListBox)

//-----------------------------------------------------------------------------
int CMultiSelectionListBox::AddTag (CString aTag, LPCTSTR aTitle, DataObj* pData)
{
	for (int idx = 0; idx <= m_DataAssociations.GetUpperBound(); idx++)
		if (m_TagDataAssociations[idx].CompareNoCase(aTag) == 0)
		{
			ASSERT(FALSE);
			return idx;
		}

	CString strAssoc = StripCtrlChar(aTitle);
	int nLbIdx = CBCGPListBox::AddString(strAssoc);
	m_DataAssociations.InsertAt(nLbIdx, pData->DataObjClone());
	m_TagDataAssociations.InsertAt(nLbIdx, aTag);
	return nLbIdx;
}

//-----------------------------------------------------------------------------
DataObj* CMultiSelectionListBox::GetCtrlData (const CString& sName, int /*nRow = 0*/)
{
	for (int i=0; i < m_TagDataAssociations.GetSize(); i++)
	{
		if (sName.CompareNoCase(m_TagDataAssociations.GetAt(i)) == 0)
			return SetSel(i), m_DataAssociations.GetAt(i);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
//ritorna solo i tag delle righe selezionate
int CMultiSelectionListBox::EnumColumnName (CStringArray& arNames, BOOL bAll /*= TRUE*/)
{
	arNames.RemoveAll();
	if (bAll)
	{
		arNames.Copy(m_TagDataAssociations);
		return arNames.GetSize();
	}
	int nMaxSel = m_TagDataAssociations.GetSize();
	if (nMaxSel == 0) return 0;
	int*	pIdxVector = new int[nMaxSel];
	nMaxSel = GetSelItems(nMaxSel, pIdxVector);
	for (int i = 0; i < nMaxSel; i++)
	{
		arNames.Add(m_TagDataAssociations.GetAt(pIdxVector[i]));
	}
	delete [] pIdxVector;
	return arNames.GetSize();
}

//-----------------------------------------------------------------------------
int CMultiSelectionListBox::GetSelectedItemData (DataObjArray& arSelectedData)
{
	arSelectedData.RemoveAll();

	int nMaxSel = m_TagDataAssociations.GetSize();
	if (nMaxSel == 0) return 0;

	int* pIdxVector = new int[nMaxSel];
	nMaxSel = GetSelItems(nMaxSel, pIdxVector);

	for (int i = 0; i < nMaxSel; i++)
	{
		arSelectedData.Add(m_DataAssociations.GetAt(pIdxVector[i])->DataObjClone());
	}
	delete [] pIdxVector;

	return arSelectedData.GetSize();
}

//-----------------------------------------------------------------------------
void CMultiSelectionListBox::SetSelTags (CStringArray& arSelectTags)
{
	for (int i=0; i < arSelectTags.GetSize(); i++)
	{
		int nIdx = CStringArray_Find(m_TagDataAssociations, arSelectTags.GetAt(i), TRUE);
		if (nIdx >= 0)
			SetSel(nIdx);
	}
}

//-----------------------------------------------------------------------------
void CMultiSelectionListBox::SetSelTags (DataStrArray& arSelectTags)
{
	for (int i=0; i < arSelectTags.GetSize(); i++)
	{
		int nIdx = CStringArray_Find(m_TagDataAssociations, arSelectTags.GetAt(i)->GetString(), TRUE);
		if (nIdx >= 0)
			SetSel(nIdx);
	}
}

//-----------------------------------------------------------------------------
void CMultiSelectionListBox::ResetContent	()
{
	CResizableListBox::ResetContent();

	m_DataAssociations.RemoveAll();
	m_TagDataAssociations.RemoveAll();
}

