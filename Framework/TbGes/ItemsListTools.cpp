//=======================================================================
// Module name  : ItemsListTools.CPP
// Author		:
// Description	: Dialog per la selezione degli Items
// CopyRight (c) MicroArea S.p.A. All rights reserved
//=======================================================================

#include "stdafx.h"

// Library declarations
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGenlib\ParsCtrl.h>
#include <TbOleDB\OleDbMng.h>
#include <TbGes\Tabber.h>
#include <TbGes\TileDialog.h>

#include "ItemsListTools.h"
#include "ItemsListTools.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Parametri per le query
static TCHAR szParam1[]			= _T("p1");
static TCHAR szParamAmbiente[]	= _T("p2");
static TCHAR szParamDis[]		= _T("p3");
static TCHAR szTokenSeps[]		= _T("; ");

////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
BOOL ParseMultipleItems(const DataStr& Items, CStringArray& arrayItems, BOOL bNoRemove /*=FALSE*/)
{
	if (!bNoRemove)
		arrayItems.RemoveAll();

	if (Items.IsEmpty())
		return FALSE;

	CString	strItems = Items.GetString();
	strItems.Replace(_T("'"), _T("''")); // sostituisco ' con '' per le query SQL
	TCHAR* pItem = strItems.GetBuffer(strItems.GetLength());
	while (pItem)
	{
		TCHAR* pItemSep = _tcschr(pItem, _T(';'));
		if (pItemSep)
		{
			if (*(pItemSep - 1) == _T(' '))
				*(pItemSep - 1) = _T('\0');

			*pItemSep++ = _T('\0');
			if (*pItemSep == _T(' '))
				*pItemSep++ = _T('\0');
		}

		if (!bNoRemove)
			arrayItems.Add(pItem);
		else
		{
			BOOL bTrovato = FALSE;
			for (int i = 0; i <= arrayItems.GetUpperBound(); i++)
			{
				if (pItem == arrayItems[i])
				{
					bTrovato = TRUE;
					break;
				}
			}
			if (!bTrovato)
				arrayItems.Add(pItem);
		}

		pItem = pItemSep;
	}

	strItems.ReleaseBuffer();

	return TRUE;
}

//------------------------------------------------------------------------------
void GetMultipleItemsORClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter,
		const DataType& aDT /*= DataType::String*/
	)
{
	CStringArray arrayItems;
	if (!ParseMultipleItems(strItems, arrayItems))
		return;

	DataObj* pData = DataObj::DataObjCreate(aDT);

	CString strTemp;
	for (int i = 0; i <= arrayItems.GetUpperBound(); i++)
	{
		if (arrayItems.GetAt(i).GetAt(0) == _T('-') || arrayItems.GetAt(i).GetAt(0) == _T('!'))
			continue;

		if (!strTemp.IsEmpty())
			strTemp += _T(" OR ");

		pData->Assign(arrayItems.GetAt(i).Mid(0).Trim());
		CString strData = AfxGetDefaultSqlConnection()->NativeConvert(pData);
		if (strData.Left(2) == _T("N'"))
			strData = strData.Mid(1);
		strTemp += strColName + _T(" = ") + strData;
	}

	if (!strTemp.IsEmpty())
	{
		if(!strFilter.IsEmpty())
			strFilter = _T("(") + strFilter + _T(")") + _T(" AND ");

		strFilter += _T("(") + strTemp + _T(")");
		strTemp.Empty();
	}

	for (int i = 0; i <= arrayItems.GetUpperBound(); i++)
	{
		if (arrayItems.GetAt(i).GetAt(0) != _T('-') && arrayItems.GetAt(i).GetAt(0) != _T('!'))
			continue;

		if (!strTemp.IsEmpty())
			strTemp += _T(" AND ");

		pData->Assign(arrayItems.GetAt(i).Mid(1).Trim());
		CString strData = AfxGetDefaultSqlConnection()->NativeConvert(pData);
		if (strData.Left(2) == _T("N'"))
			strData = strData.Mid(1);
		strTemp += strColName + _T(" <> ") + strData;
	}

	if (!strTemp.IsEmpty())
	{
		if(!strFilter.IsEmpty())
			strFilter = _T("(") + strFilter + _T(")") + _T(" AND ");

		strFilter += _T("(") + strTemp + _T(")");
		strTemp.Empty();
	}

	delete pData;
}

//------------------------------------------------------------------------------
void GetMultipleItemsLIKEClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter
	)
{
	CStringArray arrayItems;
	if (!ParseMultipleItems(strItems, arrayItems))
		return;

	if(!strFilter.IsEmpty())
		strFilter = _T("(") + strFilter + _T(")") + _T(" AND ");

	for (int i = 0; i <= arrayItems.GetUpperBound(); i++)
	{
		if (i == 0)
			strFilter += _T("(");
		else
			strFilter += _T(" OR ");

		strFilter += strColName + _T(" LIKE '%") + arrayItems.GetAt(i) + _T("%'");
	}

	strFilter += _T(")");
}

//-----------------------------------------------------------------------------
void GetMultipleArrayItemsLIKEClause
	(
		const CStringArray& arrayItems,
		const CString& strColName,
		CString& strFilter
	)
{
	if(!strFilter.IsEmpty())
		strFilter += _T(" AND ");

	for (int i = 0; i <= arrayItems.GetUpperBound(); i++)
	{
		if (i == 0)
			strFilter += _T("(");
		else
			strFilter += _T(" OR ");

		strFilter += strColName + _T(" LIKE '%") + arrayItems.GetAt(i) + _T("%'");
	}

	strFilter += _T(")");
}

//------------------------------------------------------------------------------
void GetCheckItemClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter
	)
{
	TCHAR N = AfxGetDefaultSqlConnection()->UseUnicode() ? 'N' : ' ';

	// AND (Countries = '' OR Countries IS NULL OR Countries LIKE '%ALL%'
	// OR (Countries LIKE '%nazione%' AND Countries NOT LIKE '%-nazione%') OR (Countries NOT LIKE '%nazione%' AND Countries LIKE '%-%')
	if (!strItems.IsEmpty())
		strFilter += cwsprintf
			(
				_T(" AND (%s = %s OR %s IS NULL OR %s LIKE %s OR \
				   (%s LIKE %c'%%%s%%' AND %s NOT LIKE %c'%%-%s%%') OR (%s NOT LIKE %c'%%%s%%' AND %s LIKE %c'%%-%%'))"),
				(LPCTSTR) strColName,
				(LPCTSTR) AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(_T(""))),
				(LPCTSTR) strColName,
				(LPCTSTR) strColName,
				(LPCTSTR) AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(_T("%%ALL%%"))),
				(LPCTSTR) strColName,
				N,
				(LPCTSTR) strItems.Str(),
				(LPCTSTR) strColName,
				N,
				(LPCTSTR) strItems.Str(),
				(LPCTSTR) strColName,
				N,
				(LPCTSTR) strItems.Str(),
				(LPCTSTR) strColName,
				N
			);
}

//-----------------------------------------------------------------------------
void ClearListboxItems(CListBox& lb)
{
	for (int i = 0; i < lb.GetCount(); i++)
		if (lb.GetItemDataPtr(i) != NULL)
		{
			DataObj* pData = (DataObj*)lb.GetItemDataPtr(i);
			delete pData;
		}

	lb.ResetContent();
}

//////////////////////////////////////////////////////////////////////////////
// 				class CItemsListDlg Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CItemsListDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CItemsListDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CItemsListDlg)
	ON_BN_CLICKED (IDC_ITEMSLISTDLG_CARICAITEM,			OnLoadItem)
	ON_BN_CLICKED (IDC_ITEMSLISTDLG_CANCELLAITEM,		OnDeleteItem)
	ON_LBN_DBLCLK (IDC_ITEMSLISTDLG_LBITEMSLIST_FROM,	OnLoadItem)
	ON_LBN_DBLCLK (IDC_ITEMSLISTDLG_LBITEMSLIST_TO,		OnDeleteItem)
	ON_EN_VALUE_CHANGED (IDC_ITEMSLISTDLG_RADIO1,		OnQueryChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CItemsListDlg::CItemsListDlg(HotKeyLink* pHKL, DataObjArray* pItemsList, CBaseDocument* pDocument)
	:
	CParsedDialog	(IDD_ITEMSLISTDLG),
	m_pHKL			(pHKL),
	m_pItemsList	(pItemsList),
	m_pDocument		(pDocument),
	m_bEnumMode		(FALSE),
	m_pRadio1		(NULL),
	m_pRadio2		(NULL)
{
}

//-----------------------------------------------------------------------------
CItemsListDlg::CItemsListDlg(HotKeyLink* pHKL, DataObjArray* pItemsList, CBaseDocument* pDocument, BOOL bEnumMode, DWORD dwValue)
	:
	CParsedDialog	(IDD_ITEMSLISTDLG),
	m_pHKL			(pHKL),
	m_pItemsList	(pItemsList),
	m_pDocument		(pDocument),
	m_bEnumMode		(bEnumMode),
	m_dwValue		(dwValue),
	m_pRadio1		(NULL),
	m_pRadio2		(NULL)
{
}

//-----------------------------------------------------------------------------
CItemsListDlg::~CItemsListDlg()
{
}

//-----------------------------------------------------------------------------
BOOL CItemsListDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	if (m_pRadio1)
	{
		::GetParsedCtrl(&m_Radio1)->Attach(m_pRadio1);
		m_Radio1.SubclassEdit(IDC_ITEMSLISTDLG_RADIO1, this);
		m_Radio1.ShowWindow(SW_SHOW);
		m_Radio1.SetWindowTextW(m_RadioLabel1);
		::GetParsedCtrl(&m_Radio1)->UpdateCtrlView();
	}

	if (m_pRadio2)
	{
		m_Radio2.SubclassDlgItem(IDC_ITEMSLISTDLG_RADIO2, this);
		m_Radio2.ShowWindow(SW_SHOW);
		m_Radio2.SetWindowTextW(m_RadioLabel2);
	}

	m_LBItems.SubclassDlgItem(IDC_ITEMSLISTDLG_LBITEMSLIST_FROM, this);
	m_LBItemsSelected.SubclassDlgItem(IDC_ITEMSLISTDLG_LBITEMSLIST_TO, this);

	ClearListboxItems(m_LBItemsSelected);

	// Carico la ListBox degli item già scelti.
	for (int i = 0; i <= m_pItemsList->GetUpperBound(); i++)
	{
		// populate the listbox with the all the pairs description - DataObj* corresponding to the
		// already selected items
		if (m_bEnumMode)
		{
			int nPos = m_LBItemsSelected.AddString(m_pItemsList->GetAt(i)->FormatData());
			m_LBItemsSelected.SetItemDataPtr(nPos, m_pItemsList->GetAt(i)->Clone());
		}
		else
		{
			CString str;
			DataObj* pDataObj = m_pItemsList->GetAt(i);
			if (!m_pHKL->IsHotLinkRunning() && m_pHKL->DoFindRecord(const_cast<DataObj*>(pDataObj)))
				str = m_pHKL->FormatComboItem(m_pHKL->GetAttachedRecord());
			int nPos = m_LBItemsSelected.AddString(str);
			m_LBItemsSelected.SetItemDataPtr(nPos, pDataObj->Clone());
		}
	}

	QueryLBItems();

	CenterWindow();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CItemsListDlg::QueryLBItems()
{
	BeginWaitCursor();

	for (int i = 0; i < m_LBItems.GetCount(); i++)
		if (m_LBItems.GetItemDataPtr(i) != NULL)
			delete m_LBItems.GetItemDataPtr(i);
	m_LBItems.ResetContent();

	if (m_bEnumMode)
	{
		WORD wTag = GET_TAG_VALUE(m_dwValue);
        const EnumItemArray* pItems = AfxGetEnumsTable()->GetEnumItems(wTag);

		if (pItems == NULL)
			return FALSE;

		for (int i = 0; i <= pItems->GetUpperBound(); i++)
		{
			EnumItem* pItem = pItems->GetAt(i);
			if (pItem->IsHidden())
				continue;

			CString strItem = pItem->GetTitle();

			// populate the listbox with the all the pairs description - DataObj* corresponding to the
			// items not already selected 
			BOOL bFound = FALSE;
			for (int j =  0; j < m_LBItemsSelected.GetCount(); j++)
				if (((DataEnum*)m_LBItemsSelected.GetItemDataPtr(j))->GetItemValue() == pItem->GetItemValue())
				{
					bFound = TRUE;
					break;
				}

			if (!bFound)
			{
				int nPos = m_LBItems.AddString(pItem->GetTitle());
				m_LBItems.SetItemDataPtr(nPos, new DataEnum(wTag, pItem->GetItemValue()));
			}
		}
	}
	else
	{
        int nMaxItems = -1;
		DataObjArray arKeyData;
		CStringArray arDescriptions;

		m_pHKL->CloseTable();
		m_pHKL->SearchComboQueryData(nMaxItems, arKeyData, arDescriptions);

		for (int i = 0; i < arKeyData.GetSize(); i++)
		{
			// populate the listbox with the all the pairs description - DataObj* corresponding to the
			// items not already selected 
			BOOL bFound = FALSE;
			for (int j =  0; j < m_LBItemsSelected.GetCount(); j++)
				if (((DataObj*)m_LBItemsSelected.GetItemDataPtr(j))->IsEqual(*arKeyData[i]))
				{
					bFound = TRUE;
					break;
				}

			if (!bFound)
			{
				int nPos = m_LBItems.AddString(arDescriptions[i]);
				m_LBItems.SetItemDataPtr(nPos, arKeyData[i]->Clone());
			}
		}
	}

	m_LBItems.CalcHorizontalExtent();
	m_LBItemsSelected.CalcHorizontalExtent();

	EndWaitCursor();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CItemsListDlg::OnQueryChanged()
{
	if (m_pRadio1 && m_pRadio2)
		*m_pRadio2 = !*m_pRadio1;

	QueryLBItems();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CItemsListDlg::OnLoadItem()
{
	CString str;
	for (int i = m_LBItems.GetCount() - 1; i >= 0; i--)
	{
		if (m_LBItems.GetSel(i) == 0)
			continue;

		m_LBItems.GetText(i, str);
		void* pItem = m_LBItems.GetItemDataPtr(i);

		int nPos = m_LBItemsSelected.AddString(str);
		m_LBItemsSelected.SetItemDataPtr(nPos, pItem);

		m_LBItems.DeleteString(i);
	}
	m_LBItems.CalcHorizontalExtent();
	m_LBItemsSelected.CalcHorizontalExtent();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CItemsListDlg::OnDeleteItem()
{
	CString str;
	for (int i = m_LBItemsSelected.GetCount() - 1; i >= 0; i--)
	{
		if (m_LBItemsSelected.GetSel(i) == 0)
			continue;

		m_LBItemsSelected.GetText(i, str);
		void* pItem = m_LBItemsSelected.GetItemDataPtr(i);

		int nPos = m_LBItems.AddString(str);
		m_LBItems.SetItemDataPtr(nPos, pItem);

		m_LBItemsSelected.DeleteString(i);
	}
	m_LBItems.CalcHorizontalExtent();
	m_LBItemsSelected.CalcHorizontalExtent();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CItemsListDlg::OnOK()
{
	m_pItemsList->RemoveAll();

	for (int i = 0; i < m_LBItemsSelected.GetCount(); i++)
		m_pItemsList->Add(((DataObj*)m_LBItemsSelected.GetItemDataPtr(i))->Clone());

	m_pDocument->SetModifiedFlag(TRUE);

	ClearListboxItems(m_LBItems);
	ClearListboxItems(m_LBItemsSelected);

	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CItemsListDlg::OnCancel()
{
	ClearListboxItems(m_LBItems);
	ClearListboxItems(m_LBItemsSelected);

	EndDialog(IDCANCEL);
}

//////////////////////////////////////////////////////////////////////////////
// 				class CItemsStringDlg Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CItemsStringDlg::CItemsStringDlg(HotKeyLink* pHKL, CBaseDocument* pDocument)
	:
	CItemsListDlg(pHKL, &m_StringArray, pDocument)
{
}

//-----------------------------------------------------------------------------
CItemsStringDlg::CItemsStringDlg(HotKeyLink* pHKL, CBaseDocument* pDocument, BOOL bEnumMode, DWORD dwValue)
	:
	CItemsListDlg(pHKL, &m_StringArray, pDocument, bEnumMode, dwValue)
{
}

//-----------------------------------------------------------------------------
void CItemsStringDlg::SetCommaSeparetdString (const CString& aString)
{
	m_strCommaSeparated = aString;

	m_StringArray.RemoveAll();

	CStringArray arItemList;
	ParseMultipleItems(m_strCommaSeparated, arItemList);
	for (int i = 0; i <= arItemList.GetUpperBound(); i++)
		m_StringArray.Add(new DataStr(arItemList[i]));
}

//-----------------------------------------------------------------------------
void CItemsStringDlg::OnOK()
{
	CItemsListDlg::OnOK();

	m_strCommaSeparated.Empty();

	for (int i = 0; i <= m_StringArray.GetUpperBound(); i++)
	{
		if (!m_strCommaSeparated.IsEmpty())
			m_strCommaSeparated += _T("; ");

		m_strCommaSeparated += m_StringArray[i]->GetString();
	}
}

////////////////////////////////////////////////////////////////////////////
//           class CItemsMSCombo implementation
////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CItemsMS, CMSStrButton)

BEGIN_MESSAGE_MAP(CItemsMS, CMSStrButton)
	//{{AFX_MSG_MAP(CItemsMSCombo)
	ON_WM_KEYDOWN	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CItemsMS::CItemsMS()
	:
	CMSStrButton		(),
	m_pCItemsListEdit	(NULL)
{}

//-----------------------------------------------------------------------------
CItemsMS::~CItemsMS()
{
	m_pCItemsListEdit = NULL;
}

//-----------------------------------------------------------------------------
void CItemsMS::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (nChar == VK_LEFT || nChar == VK_RIGHT)
		return;

	__super::OnKeyDown(nChar, nRepCnt, nFlags);
}		

//-----------------------------------------------------------------------------
BOOL CItemsMS::UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage)
{
	BOOL bModified = GetModifyFlag();

	if (bModified && m_pCItemsListEdit)
		m_pCItemsListEdit->UpdateItemsList();

	SetCurSel(-1);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CItemsMS::OnUpdateCtrlStatus(int)
{
	if (GetDocument()->GetFormMode() == CBaseDocument::FIND)
		GetCtrlData()->SetReadOnly(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CItemsMS::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);
	if (nID == (UINT)GetCtrlID() && hWndCtrl != NULL)
	{
		// control notification
		ASSERT(::IsWindow(hWndCtrl));
		if (nCode == CBN_CLOSEUP)
		{
			UpdateCtrlData(FALSE, FALSE);
		}
		else if (nCode == CBN_DROPDOWN && m_pCItemsListEdit)
		{
			m_pCItemsListEdit->UpdateItemsToOpen();
		}
	}

	return __super::OnCommand(wParam, lParam);
}


////////////////////////////////////////////////////////////////////////////
//           class CItemsEdit implementation
////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CItemsEdit, CStrEdit)

BEGIN_MESSAGE_MAP(CItemsEdit, CStrEdit)
	//{{AFX_MSG_MAP(CItemsEdit)
	ON_WM_KEYDOWN	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CItemsEdit::CItemsEdit()
	:
	CStrEdit			(),
	m_pCItemsListEdit	(NULL)
{}

//-----------------------------------------------------------------------------
CItemsEdit::~CItemsEdit()
{
	m_pCItemsListEdit = NULL;
}	

//-----------------------------------------------------------------------------
void CItemsEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (nChar == VK_LEFT || nChar == VK_RIGHT)
		return;

	CStrEdit::OnKeyDown(nChar, nRepCnt, nFlags);
}	

//-----------------------------------------------------------------------------
BOOL CItemsEdit::UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage)
{
	DataStr* pData = (DataStr*)m_pData;
	if(pData->GetString() == L"")
		SetModifyFlag(FALSE);

	BOOL bModified = GetModifyFlag();
	BOOL bOk = CStrEdit::UpdateCtrlData(bEmitError, bSendMessage);

	if (bModified && bOk && m_pCItemsListEdit)
		m_pCItemsListEdit->UpdateItemsList();

	return bOk;
}

//-----------------------------------------------------------------------------
void CItemsEdit::ModifiedCtrlData	()
{	
	__super::ModifiedCtrlData();	
	
	// TODO: check data != ""
	DataStr* pData = (DataStr*)m_pData;
	
	if(pData->GetString() != L"")
	{
		m_pCItemsListEdit->UpdateItemsList();	
		// clean up the control content.
		ClearCtrl();	
	}
}

//-----------------------------------------------------------------------------
void CItemsEdit::OnUpdateCtrlStatus(int)
{
	if (GetDocument()->GetFormMode() == CBaseDocument::FIND)
		GetCtrlData()->SetReadOnly(FALSE);
}

////////////////////////////////////////////////////////////////////////////
//           class CItemsListEdit implementation
////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CItemsListEdit, CStrEdit)

BEGIN_MESSAGE_MAP(CItemsListEdit, CStrEdit)
	//{{AFX_MSG_MAP(CParsedEdit)
	ON_COMMAND		(ID_EDIT_CUT, OnDeleteItem)
	ON_WM_KEYDOWN	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CItemsListEdit::CItemsListEdit()
	:
	CStrEdit			(),
	m_pCItemsMS			(NULL),
	m_pItemsList		(NULL),
	m_pCItemsEdit		(NULL),
	m_pHKL				(NULL),
	m_bOwnCombo			(FALSE),
	m_bOwnList			(FALSE),
	m_bShowDescriptions	(TRUE)
{
	// Il separatore di default è il ";".
	m_Separator = ';';
	m_bDisableSelection = TRUE;
}

//-----------------------------------------------------------------------------
CItemsListEdit::CItemsListEdit(DataStr* pData)
	:
	CStrEdit			(NO_BUTTON, pData),
	m_pCItemsMS			(NULL),
	m_pItemsList		(NULL),
	m_pCItemsEdit		(NULL),
	m_pHKL				(NULL),
	m_bOwnCombo			(FALSE),
	m_bOwnList			(FALSE),
	m_bShowDescriptions	(TRUE)
{
	// Il separatore di default è il ";".
	m_Separator = ';';
	m_bDisableSelection = TRUE;
}

//-----------------------------------------------------------------------------
CItemsListEdit::~CItemsListEdit()
{
	if (m_bOwnCombo)
	{
		SAFE_DELETE(m_pCItemsMS);
	}

	if (m_bOwnList)
	{
		m_pItemsList->RemoveAll();
		SAFE_DELETE(m_pItemsList);
	}
}

//-----------------------------------------------------------------------------
BOOL CItemsListEdit::OnShowingPopupMenu	(CMenu& menu)
{
	int	nStartChar	= 0;
	int	nPosChar	= 0;

	DataStr& itemsList = *((DataStr*) GetCtrlData());
	UINT nEnabled = (itemsList.IsEmpty()) ? MF_GRAYED : 0;

	menu.AppendMenu(MF_STRING | nEnabled, ID_EDIT_CUT, _TB("Cancel"));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CItemsListEdit::PreCreateWindow(CREATESTRUCT& cs) 
{
	//cs.style |=  ES_AUTOHSCROLL; 
	cs.style |=  WS_HSCROLL; 
	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
void CItemsListEdit::OnDeleteItem()
{
	int	nStartChar	= 0;
	int	nPosChar	= 0;
	GetSel(nStartChar, nPosChar);

	BOOL bSel = nStartChar != nPosChar;

	char aSeparator = m_bShowDescriptions ? '\n' : m_Separator;

	CString strItemsList;
	GetValue(strItemsList);

	// provo ad eliminare i separatori che ci sono prima della selezione
	if	(
			nStartChar > 0 &&
			(nStartChar == strItemsList.GetLength() || strItemsList[nStartChar] != aSeparator)
		)
		nStartChar = max(strItemsList.Left(nStartChar).ReverseFind(aSeparator), 0);

	if (nPosChar < strItemsList.GetLength())
		if (nStartChar > 0)
		{
			if (bSel && strItemsList[nPosChar-1] == _T(' '))
				nPosChar--; // selezionati "); "

			if (bSel && strItemsList[nPosChar-1] == aSeparator)
				nPosChar--; // selezionati ");" o "); "
			else
			{
				int i = strItemsList.Mid(nPosChar).Find(aSeparator);
				if (i < 0)
					nPosChar = strItemsList.GetLength();
				else
					nPosChar += i;
			}
		}
		else
		{
			if (!bSel || (strItemsList[nPosChar-1] != aSeparator && strItemsList[nPosChar-1] != _T(' ')))
			{
				int i = strItemsList.Mid(nPosChar).Find(aSeparator);
				if (i < 0)
					nPosChar = strItemsList.GetLength();
				else
					nPosChar += i;
			}

			// essendo a inzio stringa devo eliminare i separatori che ci sono dopo
			if (nPosChar < strItemsList.GetLength() && strItemsList[nPosChar] == aSeparator)
				nPosChar++;
			if (nPosChar < strItemsList.GetLength() && strItemsList[nPosChar] == _T(' '))
				nPosChar++;
		}

	if (nStartChar == nPosChar)
		return;

	INT j = 0;
	CString strItemsListToDelete = strItemsList.Mid(nStartChar, nPosChar - nStartChar);
	for (CString sItemComboStr = strItemsListToDelete.Tokenize(CString(aSeparator), j); j >= 0; sItemComboStr = strItemsListToDelete.Tokenize(CString(aSeparator), j))
	{
		sItemComboStr.Replace(_T("\r\n"), _T(""));
		sItemComboStr.Replace(_T("-"), _T(""));
		sItemComboStr.Trim();
		for (int i = 0; i <= m_pItemsList->GetUpperBound(); i++)
		{
			CString str = m_pItemsList->GetAt(i)->FormatData();
			if (str.Compare(sItemComboStr) == 0)
			{
				m_pItemsList->RemoveAt(i);
				break;
			}
		}

	}
	
	if (m_pCItemsMS)
		m_pCItemsMS->SetArrayValue(*m_pItemsList);
		
	*((DataStr*) GetCtrlData()) = strItemsList.Left(nStartChar) + strItemsList.Mid(nPosChar);

	ModifiedCtrlData();

	SetSel(nStartChar, nStartChar);
}

//-----------------------------------------------------------------------------
BOOL CItemsListEdit::DoOnChar(UINT nChar)
{
	switch (nChar)
	{
	case VK_UP:
	case VK_DOWN:
		return m_pCItemsMS != NULL;
	case VK_DELETE:
	case VK_BACK:
	case VK_OEM_MINUS:
	case VK_SUBTRACT:
	case (UINT) '-':
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CItemsListEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if ((nChar == VK_UP || nChar == VK_DOWN))
	{
		if (m_pCItemsMS)
		{
			m_pCItemsMS->SetFocus();
			m_pCItemsMS->ShowDropDown(TRUE);
		}
		return;
	}

	if (nChar == VK_DELETE || nChar == VK_BACK)
	{
		OnDeleteItem();
		return;
	}

	if (nChar == VK_OEM_MINUS || nChar == VK_SUBTRACT)
	{
		int	nStartChar = 0;
		int	nPosChar = 0;
		GetSel(nStartChar, nPosChar);
		CString strItemsList;
		GetValue(strItemsList);

		if (strItemsList.GetLength() == 0)
			return;

		if (nPosChar > 0)
		{
			if (strItemsList[nPosChar - 1] != ' ')
				return;
		}
		if (nPosChar < strItemsList.GetLength())
		{
			if (strItemsList[nPosChar] == '-')
				return;
		}

		strItemsList.Insert(nPosChar, '-');

		*((DataStr*)GetCtrlData()) = strItemsList;

		ModifiedCtrlData();
		return;
	}

	CStrEdit::OnKeyDown(nChar, nRepCnt, nFlags);
}		

// Update check value of item in the open combo
//-----------------------------------------------------------------------------
void CItemsListEdit::UpdateItemsToOpen()
{
	if (!m_pCItemsMS) return;

	m_pCItemsMS->UnSelectAll();
	DataStr& inputString = *((DataStr*)GetCtrlData());
	CString str = inputString.GetString();
	str.Replace(_T("-"), _T(""));
	if (str.IsEmpty()) return;
	int nPos = str.Find(m_Separator);
	int nStart = 0;
	if (nPos < 0)
		m_pCItemsMS->SetCheck(str, TRUE);
	else
	{
		CString strCode;
		while (nPos > 0)
		{
			strCode = str.Mid(nStart, nPos - nStart);
			m_pCItemsMS->SetCheck(strCode.Trim(), TRUE);
			nStart = nPos + 1;
			nPos = str.Find(m_Separator, nStart);
		}
		strCode = str.Mid(nStart);
		m_pCItemsMS->SetCheck(strCode.Trim(), TRUE);
	}
}

//-----------------------------------------------------------------------------
void CItemsListEdit::UpdateItemsList()
{
	ASSERT(m_pCItemsMS || m_pCItemsEdit);
	ASSERT(m_pItemsList);
	if (!(m_pCItemsMS || m_pCItemsEdit) || !m_pItemsList)
		return;

	if (m_pCItemsMS)
	{
		m_pItemsList->RemoveAll();
		m_pCItemsMS->GetArrayValue(*m_pItemsList);
	}

	if (m_pCItemsEdit)
	{
		DataStr* pStrCopy = new DataStr(m_Item);
		m_pItemsList->Add(pStrCopy);
	}

	UpdateInputString();
	ModifiedCtrlData();
}

//-----------------------------------------------------------------------------
void CItemsListEdit::RestoreSubtract(CString pStrOld, CString pStrToInsert, DataStr* pDataStr)
{
	if (!pDataStr || pStrOld.IsEmpty() || pStrToInsert.IsEmpty()) 
		return;
	DataStr& inputString = *(pDataStr);
	pStrOld.Replace(_T("\r\n"), _T(""));
	int nPosChar = pStrOld.Find(pStrToInsert.MakeUpper());
	if ((nPosChar >= 1 && pStrOld[nPosChar - 1] == '-'))
	{
		inputString += _T('-');
	}
}

//-----------------------------------------------------------------------------
void CItemsListEdit::UpdateInputString()
{
	DataStr& inputString = *((DataStr*)GetCtrlData());
	CString strItemsListOld = inputString.GetString();
	// restore value in m_pItemsList
	if (!inputString.IsEmpty() && m_pItemsList->GetSize() <= 0)
	{
		INT j = 0;
		CString strToken = CString(m_Separator);
		strItemsListOld.Replace(_T("\r\n"), _T(""));
		strItemsListOld.Replace(_T("-"), _T(""));

		for (CString sItemComboStr = strItemsListOld.Tokenize(strToken, j); j >= 0; sItemComboStr = strItemsListOld.Tokenize(strToken, j))
		{
			DataStr* pStrCopy = new DataStr(sItemComboStr);
			m_pItemsList->Add(pStrCopy);
		}
	}
	
	inputString.Clear();
	
	ASSERT_TRACE(!m_bShowDescriptions || m_pHKL, "If the CItemsListEdit show the description, the HotKeyLink must be attached");
	if (m_bShowDescriptions && m_pHKL)
	{
		for (int i = 0; i <= m_pItemsList->GetUpperBound(); i++)
		{
			DataObj* pDataObj = m_pItemsList->GetAt(i);
			CString str;
			if (!m_pHKL->IsHotLinkRunning() && m_pHKL->DoFindRecord(const_cast<DataObj*>(pDataObj)))
				str = m_pHKL->FormatComboItem(m_pHKL->GetAttachedRecord());
			inputString += (i == 0 ? _T("") : CString(m_Separator) + _T(" "));
			// Restore the Subtract
			RestoreSubtract(strItemsListOld, str, &inputString);

			inputString += (i == 0 ? _T("") : _T("\r\n")) + str;
		}
	}
	else
		for (int i = 0; i <= m_pItemsList->GetUpperBound(); i++)
		{
			CString str = m_pItemsList->GetAt(i)->FormatData();
			inputString += (i == 0 ? _T("") : CString(m_Separator) + _T(" "));
			// Restore the Subtract
			RestoreSubtract(strItemsListOld, str, &inputString);
			inputString += str;
		}
}

//-----------------------------------------------------------------------------
void CItemsListEdit::Attach
	(
		DataObjArray*	pItemsList,
		HotKeyLink*		pHKL
	)
{
	m_pItemsList = pItemsList; 
	m_pHKL = pHKL;
	UpdateInputString();
}

//-----------------------------------------------------------------------------
void CItemsListEdit::CreateItemsMSButton
	(
		const	CString&		sName,
		CWnd*			pView,
		UINT			nIDC,
		CString			sNSImage /* = _T("") */
	)
{
	CreateItemsMSCombo(sName, pView, nIDC);
	if (sNSImage.IsEmpty())
		m_pCItemsMS->SetImageNS(TBIcon(szIconDown, IconSize::TOOLBAR));
	else
		m_pCItemsMS->SetImageNS(sNSImage);
}

//-----------------------------------------------------------------------------
HotKeyLinkObj* CItemsListEdit::CreateItemsCombo(const CString& sName, CWnd* pView, UINT nIDC, HotKeyLink* pHKL)
{
	m_bOwnList = TRUE;
	Attach(new DataObjArray, pHKL);

	CreateItemsMSCombo(sName, pView, nIDC);

	return m_pCItemsMS ? m_pCItemsMS->GetHotLink() : NULL;
}

//-----------------------------------------------------------------------------
void CItemsListEdit::CreateItemsMSCombo
	(
		const	CString&		sName, 
				CWnd*			pView, 
				UINT			nIDC
	)
{
	ASSERT(m_pCItemsMS == NULL);
	ASSERT_TRACE(m_pItemsList && m_pHKL, "Attach must be called before CreateItemsMSCombo");

	m_bOwnCombo = FALSE;

	if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		m_pCItemsMS = (CItemsMS*)((CAbstractFormView*)pView)->AddLink
			(
				nIDC,
				sName,
				NULL,
				&m_InputString,
				RUNTIME_CLASS(CItemsMS),
				m_pHKL,
				NO_BUTTON
			);
	
	if (pView->IsKindOf(RUNTIME_CLASS(CTabDialog)))
		m_pCItemsMS = (CItemsMS*)((CTabDialog*)pView)->AddLink
			(
				nIDC,
				sName,
				NULL,
				&m_InputString,
				RUNTIME_CLASS(CItemsMS),
				m_pHKL,
				NO_BUTTON
			);
	else if (pView->IsKindOf(RUNTIME_CLASS(CTileDialog)))
		m_pCItemsMS = (CItemsMS*)((CTileDialog*)pView)->AddLink
		(
			nIDC,
			sName,
			NULL,
			&m_InputString,
			RUNTIME_CLASS(CItemsMS),
			m_pHKL,
			NO_BUTTON
			);
	else if (pView->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
	{
		m_pCItemsMS = new CItemsMS;
		m_bOwnCombo = TRUE;

		CParsedCtrl* pControl = ::GetParsedCtrl(m_pCItemsMS);
		pControl->Attach(&m_InputString);

		if (m_pHKL)
			pControl->AttachHotKeyLink(m_pHKL);

		if (!pControl->SubclassEdit(nIDC, pView))
		{
			delete m_pCItemsMS;

			ASSERT(FALSE);
			return;
		}
	}

	ASSERT(m_pCItemsMS);

	if (m_pCItemsMS)
	{
		m_pCItemsMS->m_pCItemsListEdit = this;
		m_pCItemsMS->ShowDescription();
		m_pCItemsMS->LoadUnattending();
		m_pCItemsMS->SetArrayValue(*m_pItemsList);
	}
}

// porting da 3_x (futura 3_11?), da testare, visto che i meccanismi interni 
// sono cambiati.
//-----------------------------------------------------------------------------
HotKeyLinkObj* CItemsListEdit::CreateItemsEdit(const CString& sName, CWnd* pView, UINT nIDC, HotKeyLink* pHKL)
{
	ASSERT(m_pCItemsEdit == NULL);
	m_bOwnEdit = FALSE;

	m_bShowDescriptions = FALSE;
	m_bOwnList = TRUE;
	Attach(new DataObjArray, pHKL);

	if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		m_pCItemsEdit = (CItemsEdit*)	((CAbstractFormView*)pView)->AddLink
			(
				nIDC,
				sName,
				NULL,
				&m_Item,
				RUNTIME_CLASS(CItemsEdit),
				pHKL
			);
	
	if (pView->IsKindOf(RUNTIME_CLASS(CTabDialog)))
		m_pCItemsEdit = (CItemsEdit*)	((CTabDialog*)pView)->AddLink
			(
				nIDC,
				sName,
				NULL,
				&m_Item,
				RUNTIME_CLASS(CItemsEdit),
				pHKL
			);
	else if (pView->IsKindOf(RUNTIME_CLASS(CTileDialog)))
		m_pCItemsEdit = (CItemsEdit*)((CTileDialog*)pView)->AddLink
		(
			nIDC,
			sName,
			NULL,
			&m_Item,
			RUNTIME_CLASS(CItemsEdit),
			pHKL
			);
	else if (pView->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
	{
		m_pCItemsEdit = new CItemsEdit;
		m_bOwnEdit = TRUE;

		CParsedCtrl* pControl = ::GetParsedCtrl(m_pCItemsEdit);
		pControl->Attach(&m_Item);

		if (pHKL)
			pControl->AttachHotKeyLink(pHKL);

		if (!pControl->SubclassEdit(nIDC, pView))
		{
			delete m_pCItemsEdit;

			ASSERT(FALSE);
			return NULL;
		}
	}

	ASSERT(m_pCItemsEdit);
	m_pCItemsEdit->ShowWindow(SW_SHOW);

	if (m_pCItemsEdit)
		m_pCItemsEdit->m_pCItemsListEdit = this;

	return m_pCItemsEdit ? m_pCItemsEdit->GetHotLink() : NULL;
}
//-----------------------------------------------------------------------------
int CItemsListEdit::DoModal()
{
	CItemsListDlg dlg(m_pHKL, m_pItemsList, GetDocument());

	int result = dlg.DoModal();
	if (result == IDOK)
		UpdateInputString();

	return result;
}

//-----------------------------------------------------------------------------
void CItemsListEdit::OnUpdateCtrlStatus(int)
{
	if (GetDocument()->GetFormMode() == CBaseDocument::FIND)
		GetCtrlData()->SetReadOnly(FALSE);

	m_Item.SetReadOnly(GetCtrlData()->IsReadOnly());

	// @@TODO come gestire il readonly della combo senza fare comparire l-hyperlink?
	if (m_pCItemsMS)
		m_InputString.SetAlwaysReadOnly(GetCtrlData()->IsReadOnly());
}

//-----------------------------------------------------------------------------
void CItemsListEdit::SetSeparator(char separator)
{
	m_Separator = separator;
}
