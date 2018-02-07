//*******************************************************************************
// COPYRIGHT NOTES
// ---------------
// This is a sample for BCGControlBar Library Professional Edition
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//
// CustomEditCtrl.cpp : implementation file
//

#include "stdafx.h"

#include <regex>
#include <string>
using namespace std;

#include <TbGenlib/TBPropertyGrid.h>
#include <TbGenlib/BaseApp.h>

#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/edtmng.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include "WoormDoc.h"
#include "WoormFrm.h"

#include "RSEditView.h"
#include "RSEditorUI.h"

#include "CustomEditCtrl.h"

#include "RSEditorUI.hjson" //JSON AUTOMATIC UPDATE


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif



static const CString g_strEOL = _T("\n");
static const TCHAR g_chEOL = _T('\n');
static const CString g_strEOLExport = _T("\r\n");



struct StringIndex
{
	LPCTSTR lpszName;
	int		nIndex =1;
};





/////////////////////////////////////////////////////////////////////////////
// IntellisenseWndExtended


CStringList CCustomEditCtrl::m_lstFind;

CCustomEditCtrl::CCustomEditCtrl()
{
	m_bReadOnly = FALSE;
	m_bCheckColorTags = FALSE;
	m_bCopyRTFToClipboard = TRUE;
	m_bReplaceTabsAndEOLOnCopy = TRUE;
	m_bEnableWholeTextCopy = TRUE;
	m_bEnableCurrentLineCopy = TRUE;
	m_bColorHyperlink = TRUE;
	m_bBlockSelectionMode = TRUE;
	EnableGradientMarkers (TRUE);
	m_bDragTextMode = TRUE;
	m_bEnableToolTips = TRUE;
	m_nUndoBufferSize = 200;
	m_bUndoCharMode = FALSE;
	m_nScrollMouseWheelSpeed = 1;
	m_bScrollVertEmptyPage = TRUE;
	EnableIntelliSense();
	m_bIntelliSenseMode = TRUE;
	m_bIsModified = FALSE;
	m_DropTarget.Register(this);
	m_pIntelliSenseWnd = NULL;
	m_bEnableBreakpoints = FALSE;
}

//------------------------------------------------------------------
void CCustomEditCtrl::EmptyIntellisense() 
{
	if (!IsIntelliSenseEnabled())
		return;

	if (!m_mIntelliString.empty())
	{
		
		for (std::multimap<CString, IntellisenseData*>::iterator it = m_mIntelliString.begin(); it != m_mIntelliString.end(); ++it)
		{
			SAFE_DELETE(it->second);
		}

		m_mIntelliString.clear();
	}
}

//------------------------------------------------------------------
void CCustomEditCtrl::AddIntellisenseWord(CString key, CString intelliItem, CString intelliValue,CString additionalInfo,CString help)
{
	if (key.IsEmpty() || intelliItem.IsEmpty())
		return;
	if (!IsIntelliSenseEnabled())
		return;
	pair <std::multimap<CString, IntellisenseData*>::const_iterator, std::multimap<CString, IntellisenseData*>::const_iterator> range;
	range = m_mIntelliString.equal_range(key);
	typedef multimap<CString, IntellisenseData*>::const_iterator it;

	for (it p = range.first; p != range.second; ++p)
		if (p->second->m_strItemName.Compare(intelliItem)==0)
			return;
	IntellisenseData* data=new IntellisenseData();
	data->m_strItemName = intelliItem;
	data->m_strItemValue = intelliValue;
	data->m_strAdditionalInfo = additionalInfo;
	data->m_strItemHelp = help;
	//m_arGarbage.Add(data);

	m_mIntelliString.insert(pair<CString, IntellisenseData*>(key, data));
}

//------------------------------------------------------------------
CString CCustomEditCtrl::GetKeyFromWordForIntellisense(CString word)
{
	if (word.IsEmpty() || word.GetLength() < 2)
		return L"";

	word.Left(2).MakeUpper();

	return word.Left(2).MakeUpper();
}

void CCustomEditCtrl::AddToolTipItem(LPCTSTR word, LPCTSTR toolTip)
{
	if (word == NULL || toolTip == NULL)
		return;
	m_mTipString.SetAt(word, toolTip);
}

//Color variables
void CCustomEditCtrl::ColorVariables(CWoormDocMng* doc, BOOL viewMode)
{
	if (!doc || !doc->m_pEditorManager || !doc->m_pEditorManager->GetPrgData())
		return;
	WoormTable*	pSymTable = viewMode ?
							&doc->m_ViewSymbolTable
							:
							doc->m_pEditorManager->GetPrgData()->GetSymTable();
	ASSERT(pSymTable);


	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		SymField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);

		CString str = pF->GetName();
		if (!str)
			continue;

		SetWordColor(str, RS_COLOR_VARIABLE, -1, FALSE);
	}
}

CCustomEditCtrl::~CCustomEditCtrl()
{
	EmptyIntellisense();
}

/////////////////////////////////////////////////////////////////////////////
// CCustomEditCtrl message handlers

BEGIN_MESSAGE_MAP(CCustomEditCtrl, CBCGPEditCtrl)

	ON_WM_CREATE()
	//ON_WM_CHAR()
	ON_MESSAGE(UM_GET_DOCUMENT_TITLE_INFO, PostInvokeIntelliSense)

END_MESSAGE_MAP()

BOOL CCustomEditCtrl::FindText(LPCTSTR lpszFind, BOOL bNext /* = TRUE */, BOOL bCase /* = TRUE */, BOOL bWholeWord /* = FALSE */)
{
	POSITION pos = m_lstFind.Find(lpszFind);
	if (pos != NULL)
	{
		m_lstFind.RemoveAt (pos);
	}

	m_lstFind.AddHead(lpszFind);

	return CBCGPEditCtrl::FindText(lpszFind, bNext, bCase, bWholeWord);
}

void CCustomEditCtrl::OnDrawMarker (CDC* pDC, CRect rectMarker, const CBCGPEditMarker* pMarker)
{
	if (pMarker->m_dwMarkerType & g_dwBookmarkPointType)
	{

		rectMarker.left = rectMarker.left+ 3;
		if (IsEnableBreakpoints()  )
			pDC->DrawState(rectMarker.TopLeft(), CSize(14, 14), TBLoadPng(TBGlyph(szGlyphBreakpoint)), DST_ICON | DSS_NORMAL,(CBrush*)0);
		else
			pDC->DrawState(rectMarker.TopLeft(), CSize(14, 14), TBLoadPng(TBGlyph(szGlyphBookmark)), DST_ICON | DSS_NORMAL, (CBrush*)0);

	}
	
	else
	{
		CBCGPEditCtrl::OnDrawMarker (pDC, rectMarker, pMarker);
	}
}

int CCustomEditCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CBCGPEditCtrl::OnCreate(lpCreateStruct) == -1)
		return -1;
	
	if (m_lstFind.IsEmpty ())
	{
		CBCGPToolbarComboBoxButton* pSrcCombo = NULL;
		CObList listButtons;

		if (CBCGPToolBar::GetCommandButtons (ID_EDIT_FIND_COMBO, listButtons) > 0)
		{
			for (POSITION posCombo = listButtons.GetHeadPosition (); 
				pSrcCombo == NULL && posCombo != NULL;)
			{
				CBCGPToolbarComboBoxButton* pCombo = 
					DYNAMIC_DOWNCAST (CBCGPToolbarComboBoxButton, listButtons.GetNext (posCombo));

				if (pCombo != NULL)
				{
					pSrcCombo = pCombo;
				}
			}
		}

		if (pSrcCombo != NULL)
		{
			if (const int nMax = (int) pSrcCombo->GetCount())
			{
				CString sText;
				CComboBox* pCombo = pSrcCombo->GetComboBox();
				
				for (int i = 0; i < nMax; i++)
				{
					pCombo->GetLBText(i,sText);
					m_lstFind.AddTail (sText);
				}

				pCombo->SetCurSel(0);
			}
		}
	}

	return 0;
}

BOOL CCustomEditCtrl::CheckIntelliMark(const CString& strBuffer, int& nOffset, CString& strWordSuffix) const
{
	BOOL bIntelliMark = (strBuffer.GetAt(nOffset) == _T('.')) ;

	if (bIntelliMark)
	{
		strWordSuffix = _T('.');
	}
	/*else if (strBuffer.GetAt(nOffset) == _T('_'))
	{
		strWordSuffix = _T('_');
		bIntelliMark = TRUE;
	}*/

	if (bIntelliMark)
	{
		nOffset--;  
	}

	return TRUE;// bIntelliMark;
}

BOOL CCustomEditCtrl::OnBeforeInvokeIntelliSense (const CString& strBuffer, int& nCurrOffset, CString& strIntelliSence) const
{		
	
	if (!IsIntelliSenseEnabled())
	{
		return FALSE;
	}
	
	strIntelliSence.Empty();

	int nOffset = nCurrOffset;

	if (nOffset > strBuffer.GetLength())
	{
		nOffset = strBuffer.GetLength();
	}


	if (nOffset >= 0)
	{
		
		for (--nOffset;
			 nOffset >= 0 &&
			 m_strIntelliSenseChars.Find(strBuffer.GetAt(nOffset)) == -1 &&
			 (m_strNonSelectableChars.Find(strBuffer.GetAt(nOffset)) >= 0 ||
			  m_strWordDelimeters.Find(strBuffer.GetAt(nOffset)) == -1);
			 nOffset--);

		if (nOffset >= 0 &&
			FillIntelliSenceWord(strBuffer,nOffset,strIntelliSence) &&
			IsIntelliSenceWord(strIntelliSence))
		{
			nCurrOffset = nOffset + 1;
			return TRUE;
		}
	}
	else
	{
		ASSERT(FALSE);
	}

	return FALSE;
}

BOOL CCustomEditCtrl::FillIntelliSenseList (CObList& lstIntelliSenseData,
											LPCTSTR lpszIntelliSense /* = NULL */) const
{
	if (lpszIntelliSense == NULL)
	{
		BOOL bRet;
		CString strIntelliSence;
		int nCurrOffset = m_nCurrOffset;
		
		if (!OnBeforeInvokeIntelliSense(m_strBuffer, nCurrOffset, strIntelliSence))
		{
			return FALSE;
		}

		bRet = FillIntelliSenseList(lstIntelliSenseData, strIntelliSence.GetBuffer(0));
		ASSERT(bRet);

		return bRet;
	}

	if (!IsIntelliSenseEnabled() ||
		!IsIntelliSenceWord(lpszIntelliSense))
	{
		return FALSE;
	}	

	// here substitute with trie
	IntellisenseData* pData;
	ReleaseIntelliSenseList(lstIntelliSenseData);	//lstIntelliSenseData.RemoveAll();
	
	pair <std::multimap<CString, IntellisenseData*>::const_iterator, std::multimap<CString, IntellisenseData*>::const_iterator> range;
	range = m_mIntelliString.equal_range(((CString)lpszIntelliSense).MakeUpper());


	for (std::multimap<CString, IntellisenseData*>::const_iterator it = range.first; it != range.second; ++it)
	{
		pData = new IntellisenseData;
		pData->m_strItemName = it->second->m_strItemName;
		pData->m_strItemValue = it->second->m_strItemValue;
		pData->m_strItemHelp = it->second->m_strItemHelp;
		pData->m_strAdditionalInfo = it->second->m_strAdditionalInfo;
		//const_cast<CCustomEditCtrl*>(this)->m_arGarbage.Add(pData);

		lstIntelliSenseData.AddTail (pData);
	}

	return TRUE;
}

BOOL CCustomEditCtrl::OnIntelliSenseComplete(int nIdx, CBCGPIntelliSenseData* pData, CString& strText)
{	
	SetFocus();
	HideCaret();

	int offset = GetCurOffset();
	int i = 0;
	for ( i = offset;i >= 0;i--)
	{				
		if (m_strIntelliSenseChars.Find(GetCharAt(i),0)==-1)	
			break;
	}

	if (i!=offset)
		SetSel(i+1, offset);
	ReplaceSel(((IntellisenseData*)pData)->m_strItemValue);

	m_nCurrOffset = m_nSavedOffset = GetCurOffset();
	
	ShowCaret();
	return FALSE;

}

// here i need to search for the word in trie 
BOOL CCustomEditCtrl::IsIntelliSenceWord(CString strWord) const
{
	if (!IsIntelliSenseEnabled())
	{
		return FALSE;
	}
	
	if (m_mIntelliString.count(strWord.MakeUpper()) > 0)
	{
		return TRUE;
	}

	return FALSE;
}

BOOL CCustomEditCtrl::OnGetTipText (CString& strTipString)
{
	CPoint point;
	::GetCursorPos (&point);
	ScreenToClient (&point);

	CString strText;
	BOOL bIsHyperlink = m_bEnableHyperlinkSupport && GetHyperlinkToolTip (strText);
	BOOL bIsHiddenTextFromPoint = !bIsHyperlink && m_bEnableOutlining && GetHiddenTextFromPoint (point, strText);
	BOOL bIsWordFromPoint = !bIsHyperlink && !bIsHiddenTextFromPoint && GetWordFromPoint (point, strText);

	if ((bIsHiddenTextFromPoint || bIsHyperlink) && strText == strTipString)
	{
		return TRUE;
	}
	else if (m_mTipString.Lookup(strTipString,strTipString))
	{
		return TRUE; 
	}
	else if (IsIntelliSenseEnabled() && !bIsWordFromPoint)
	{
		return TRUE;
	}

	return FALSE;
}

BOOL CCustomEditCtrl::IntelliSenseCharUpdate(const CString& strBuffer, int nCurrOffset, TCHAR nChar, CString& strIntelliSense)
{
	if (!IsIntelliSenseEnabled())
	{
		return FALSE;
	}
	
	m_strIntelliSenseChars = L"._abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	m_strNonSelectableChars = L"";

	

	return FALSE;
}


int CCustomEditCtrl::GetNextPos(const CString& strBuffer, const CString& strSkipChars, int& nPos, BOOL bForward)
{
	if (bForward)
	{
		for (int nLen = strBuffer.GetLength();
			 nPos < nLen &&
			 strSkipChars.Find(strBuffer.GetAt(nPos)) >= 0;
			 nPos++);
	}
	else
	{
		for (--nPos;
			 nPos >= 0 &&
			 strSkipChars.Find(strBuffer.GetAt(nPos)) >= 0;
			 nPos--);
	}

	return nPos;
}

BOOL CCustomEditCtrl::FillIntelliSenceWord(const CString& strBuffer, int nOffset, CString& strIntelliSence) const
{
	CString strISWord, strWordSuffix;

	if (!CheckIntelliMark(strBuffer,nOffset,strWordSuffix) || nOffset >= 0)
	{
		do
		{
			int nStartOffset = -1,
				nEndOffset = -1;
			
			FindWordStartFinish (nOffset, strBuffer, nStartOffset, nEndOffset);
			
			if ((nStartOffset < nEndOffset) && (nStartOffset >= 0))
			{
				const CString& strWord = strBuffer.Mid(nOffset = nStartOffset, nEndOffset - nStartOffset);
				if (strWordSuffix == L"_")
					strISWord = (strWord + strISWord).MakeUpper();
				else
					strISWord = strWord + strWordSuffix + strISWord;
			}
			else
			{
				strISWord = strWordSuffix + strISWord;
				break;
			}

			strWordSuffix.Empty();
		}
		while (GetNextPos(strBuffer, m_strNonSelectableChars, nOffset, FALSE) >= 0 &&
			   CheckIntelliMark(strBuffer,nOffset,strWordSuffix) &&
			   nOffset >= 0);
	}

	if ((strISWord = strWordSuffix + strISWord).IsEmpty())
	{
		return FALSE;
	}

	strIntelliSence = strISWord;
	return TRUE;
}

void CCustomEditCtrl::ReleaseIntelliSenseList(CObList& lstIntelliSenseData) const
{
	for (POSITION pos = lstIntelliSenseData.GetHeadPosition();
		 pos != NULL;
		 delete lstIntelliSenseData.GetNext(pos));

	lstIntelliSenseData.RemoveAll();
}

void CCustomEditCtrl::OnGetCharColor (TCHAR ch, int nOffset, COLORREF& clrText, COLORREF& clrBk)
{
	if (m_bCheckColorTags)
	{
		
		TCHAR chOpen = _T ('<');
		TCHAR chClose = _T ('>');
		
		if (ch == chOpen || ch == chClose || ch == _T ('/'))
		{
			clrText = RGB (0, 0, 255);
		}
		else 
		{
			COLORREF clrDefaultBack = GetDefaultBackColor ();
			COLORREF clrDefaultTxt = GetDefaultTextColor ();
			int nBlockStart, nBlockEnd;
			if (!IsInBlock (nOffset, chOpen, chClose, nBlockStart, nBlockEnd))
			{
				clrText = clrDefaultTxt;
				clrBk = clrDefaultBack;
			}
			else if (GetCharAt (nBlockStart + 1) == _T ('%') && 
					 GetCharAt (nBlockEnd - 1) == _T ('%'))
			{

			}
			else if (clrText == clrDefaultTxt)
			{
				if (ch == _T ('='))
				{
					clrText = RGB (0, 0, 255);
				}
				else
				{
					clrText = RGB (255, 0, 0);
				}
			}
		}
	}
}



BOOL CCustomEditCtrl::ToggleCurrentLine (int nCurrRow/* = -1*/)
{
	if (nCurrRow == -1)
	{
		nCurrRow = GetCurRow();
	}

	return ToggleMarker (nCurrRow, g_dwCurrLineType, NULL, FALSE);
}



void CCustomEditCtrl::OnOutlineChanges (BCGP_EDIT_OUTLINE_CHANGES& changes, BOOL bRedraw)
{
	POSITION posInserted;
	POSITION posRemoved;

	// Get list of blocks (recursive)
	CObList lstInsertedBlocks;
	for (posInserted = changes.m_lstInserted.GetHeadPosition (); posInserted != NULL; )
	{
		CBCGPOutlineNode* pNodeInserted = (CBCGPOutlineNode*) changes.m_lstInserted.GetNext (posInserted);
		ASSERT_VALID (pNodeInserted);

		lstInsertedBlocks.AddTail (pNodeInserted);
		
		pNodeInserted->GetAllBlocks (lstInsertedBlocks, TRUE);
	}
	
	CObList lstRemovedBlocks;
	for (posRemoved = changes.m_lstRemoved.GetHeadPosition (); posRemoved != NULL; )
	{
		CBCGPOutlineNode* pNodeRemoved = (CBCGPOutlineNode*) changes.m_lstRemoved.GetNext (posRemoved);
		ASSERT_VALID (pNodeRemoved);

		lstRemovedBlocks.AddTail (pNodeRemoved);
		
		pNodeRemoved->GetAllBlocks (lstRemovedBlocks, TRUE);
	}
	
	// Find the same blocks and save them
	CObList lstSaveCollapsedBlocks;
	for (posInserted = lstInsertedBlocks.GetHeadPosition (); posInserted != NULL; )
	{
		CBCGPOutlineNode* pNodeInserted = (CBCGPOutlineNode*) lstInsertedBlocks.GetNext (posInserted);
		ASSERT_VALID (pNodeInserted);
		
		for (posRemoved = lstRemovedBlocks.GetHeadPosition (); posRemoved != NULL; )
		{
			CBCGPOutlineNode* pNodeRemoved = (CBCGPOutlineNode*) lstRemovedBlocks.GetNext (posRemoved);
			ASSERT_VALID (pNodeRemoved);
			
			if (pNodeRemoved->m_nStart == pNodeInserted->m_nStart &&
				pNodeRemoved->m_nEnd == pNodeInserted->m_nEnd &&
				pNodeRemoved->m_nBlockType == pNodeInserted->m_nBlockType &&
				pNodeRemoved->m_bCollapsed)
			{
				lstSaveCollapsedBlocks.AddTail(pNodeInserted);
			}
		}
		
	}
	
	// call the base implementation
	CBCGPEditCtrl::OnOutlineChanges (changes, bRedraw);
	
	// Restore the collapsed state
	for (POSITION posCollapsed = lstSaveCollapsedBlocks.GetHeadPosition(); posCollapsed != NULL; )
	{
		CBCGPOutlineNode* pNode = (CBCGPOutlineNode*) lstSaveCollapsedBlocks.GetNext (posCollapsed);
		ASSERT_VALID (pNode);
		
		pNode->Collapse(TRUE);
	}

}

BOOL CCustomEditCtrl::OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point)
{
	__super::OnDrop(pDataObject, dropEffect, point);
	//FindAndReplaceEnums();
	return TRUE;
}

////ENUMS

//-----------------------------------------------------------------------------
// replaces all the spaces occurencies in wrm enums 
// {     36:        0  }==>{36:0}

void CCustomEditCtrl::FindAndReplaceEnums()
{
	//gettext
	//parse matching with regular expression
	//get enum and remove spaces
	//replace
	//add enum to tooltip list
	CString sText = GetText();
	sText.Trim();
	if (sText.IsEmpty())
		return;

	string insertedText = CT2CA(sText);
	if (insertedText.empty())
		return;

	smatch match;
	regex wrmEnum("\\{\\s*\\d+\\s*:\\s*\\d+\\s*\\}"); //

	//high exception possibility
	try{
		while (regex_search(insertedText, match, wrmEnum)) {
			for (auto x : match){

				//replacing all the white spaces in enum
				string enumWithoutSpaces = regex_replace((string)x, regex("\\s+"), "");
				if (enumWithoutSpaces.empty() || ((std::string)x).empty())
					continue;
				std::string str = (std::string)x;
				CString first(str.c_str());
				CString second(enumWithoutSpaces.c_str()); //enum without spaces

				if (first!=second)
					//replace white-spaced enum with clean enum
					ReplaceText(first, second);

				//extracting two nubers from the enum
				//error possibility in conversion
				//if error occurs skip this section and pass to the the next iteration
				try{
					std::regex number("\\d+");
					std::smatch digitMatch;
					std::sregex_iterator next(enumWithoutSpaces.begin(), enumWithoutSpaces.end(), number);
					digitMatch = *next;
					short wTag = std::stoi(digitMatch.str()); //first occurance
					next++;
					digitMatch = *next;
					short wItem = std::stoi(digitMatch.str()); //second occurance

					//Get tooltip and insert in tooltip array
					CString toolTip = FormatEnum(wTag, wItem);
					AddToolTipItem(second, toolTip);
				}
				catch (...){}
			}

			insertedText = match.suffix().str();
		}
	}
	catch (...){
		return;
	}
}

//---------------------------------------------------------------------------- 
void CCustomEditCtrl::SetWindowText(CString text)
{
	text.Trim();
	__super::SetWindowText(text);

	FindAndReplaceEnums();
}


//-----------------------------------------------------------------------------
// Replaces strings in editor
void CCustomEditCtrl::ReplaceText(CString stringToReplace, CString newString){
 	FindText(stringToReplace, TRUE, TRUE, TRUE);
	ReplaceSel(newString, TRUE);
}

//-----------------------------------------------------------------------------
CString CCustomEditCtrl::FormatEnum(WORD wTag, WORD wItem)
{
	const EnumTagArray* pTags = AfxGetEnumsTable()->GetEnumTags();

	EnumTag* pTag = pTags->GetTagByValue(wTag);
	if(!pTag)
	{
		return _TB("Wrong Enum Tag value");
	}
	EnumItem* pItem = pTag->GetEnumItems()->GetItemByValue(wItem);
	if (!pItem)
	{
		return _TB("Wrong Enum Item value");
	}

	DataEnum de(wTag, wItem);

	CString a = pTag->GetTagTitle();
	CString b = pItem->GetTitle();
	CString sTooltip = pTag->GetTagTitle/*GetTagName*/() + L" : " + pItem->GetTitle/*GetItemName*/() /*+ cwsprintf(L" (%d)", de.GetValue())*/;

	return sTooltip;
}

//-----------------------------------------------------------------------------
void CCustomEditCtrl::ChangeSelectedText(CString str)
{	
 	ReplaceSel(str, TRUE);
	FindAndReplaceEnums();

	CDC*		hDC;		// handle to device context
	TEXTMETRIC	textMetric;	// text metric information
	HFONT		hFont;
	CFont* hOldFont;

	hDC = GetDC();
	// get a 10-point font and select it into the DC
	int points = MulDiv(10, GetDeviceCaps(hDC->m_hDC, LOGPIXELSY), 72);
	hFont = CreateFont(-points, 0, 0, 0, FW_NORMAL, 0, 0, 0, 0, 0, 0, 0, 0, L"Courier New");
	hOldFont = SelectFont(hDC);

	GetTextMetrics(hDC->m_hDC, &textMetric);
	ReleaseDC(hDC);

	int currentX = textMetric.tmAveCharWidth* str.GetLength() + textMetric.tmOverhang*str.GetLength();
	
	POINT point = GetCaretPos();
	point.x += currentX;
	SetCaretPos(point);
}

//-----------------------------------------------------------------------------
BOOL CCustomEditCtrl::PreTranslateMessage(MSG* pMsg)
{
	if ( IsIntelliSenseEnabled())
	{
		if (pMsg->message == WM_MOUSEMOVE)
			SetIntellisenseMode(FALSE);
		else
			SetIntellisenseMode(TRUE);
	}

	BOOL ctrlPressed = GetKeyState(VK_CONTROL) & 0x8000;
	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000;

	if (pMsg->message == WM_LBUTTONUP)
	{
		//CPoint point(GET_X_LPARAM(pMsg->lParam), GET_Y_LPARAM(pMsg->lParam));
		if (IsEnableBreakpoints() && GET_X_LPARAM(pMsg->lParam) < m_nLeftMarginWidth)
			ToggleBreakpoint();
	}

	if (ctrlPressed && (pMsg->message == WM_KEYDOWN)) {

		if (pMsg->wParam == 'C')
		{
			Copy();
			return TRUE;
		}

		if (pMsg->wParam == 'V')
		{
			Paste();
			FindAndReplaceEnums();
			return TRUE;
		}

		if (pMsg->wParam == 'X')
		{
			if (GetSelText().IsEmpty())
				return TRUE;
			Cut();
			return TRUE;
		}

		if (pMsg->wParam == 'Z')
		{
			OnUndo();
			return TRUE;
		}

		if (pMsg->wParam == 'Y')
		{
			OnRedo();
			return TRUE;
		}

		if (pMsg->wParam == 'A')
		{
			MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_ALL_TEXT);
			return TRUE;
		}

		if (pMsg->wParam == 'D')
		{
			DeleteSelectedText();
			return TRUE;
		}
		
	}

	if (shiftPressed && (pMsg->message == WM_KEYDOWN))
	{
		if (pMsg->wParam == VK_END)
		{
			MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_END);
			return TRUE;
		}

		if (pMsg->wParam == VK_HOME)
		{
			MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_HOME);
			return TRUE;
		}
																												   
		if (m_bForceIntellisense)
		{
			if (::MapVirtualKey(pMsg->wParam, MAPVK_VK_TO_CHAR) != 0)
			{
				ForceIntellisense(); 
				m_bForceIntellisense = FALSE;
			
				/*PBYTE kbs= new BYTE();
				LPWORD ch= new WORD();

				::GetKeyboardState(kbs);
				::ToAscii(pMsg->wParam, ::MapVirtualKey(pMsg->wParam, MAPVK_VK_TO_VSC), kbs, ch, 0);
			
				char* k= reinterpret_cast<char*>(ch);	

				this->InsertChar(*k,TRUE);
				return TRUE;*/
 
			}
			m_bForceIntellisense = FALSE;
		}					
	}
	
	//Opens intellisense
	if (ctrlPressed && pMsg->wParam == VK_SPACE && pMsg->message == WM_KEYDOWN)
	{
		if (!IsIntelliSenseEnabled())
			return TRUE;

		ForceIntellisense();
		
		return TRUE;
	}
	 return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void CCustomEditCtrl::SetIntellisenseMode(BOOL mode)
{
	m_bIntelliSenseMode = mode;
}

//-----------------------------------------------------------------------------
void CCustomEditCtrl::ForceIntellisense()
{
	CObList lstIntelliSenseData;
	CString strIntelliSense;
	int nCurrOffset = m_nCurrOffset;

	if (
		(nCurrOffset > 0) &&
		(nCurrOffset <= m_strBuffer.GetLength()) &&
			m_strIntelliSenseChars.Find(m_strBuffer.GetAt(nCurrOffset - 1)) != -1
		)
	{
		CString strIntelliSence;

		for (--nCurrOffset;
		nCurrOffset >= 0 &&
			m_strIntelliSenseChars.Find(m_strBuffer.GetAt(nCurrOffset)) == -1 &&
			(m_strNonSelectableChars.Find(m_strBuffer.GetAt(nCurrOffset)) >= 0);
			nCurrOffset--);

		if (nCurrOffset < 0 || !FillIntelliSenceWord(m_strBuffer, nCurrOffset, strIntelliSence))
				return ;

		//Find Point

		CString tableName = strIntelliSence.Left(strIntelliSence.Find(L"."));
		if (!tableName.IsEmpty())
			strIntelliSence = tableName + L".";
		else if (strIntelliSence.GetLength() >= 2)

			strIntelliSence = strIntelliSence.Left(2);

		else  if (strIntelliSence.GetLength()< 2)
			return ;

		if (IsIntelliSenceWord(strIntelliSence))
			nCurrOffset = nCurrOffset + 1;
		else
		{
			strIntelliSence = strIntelliSence + L"_";
			if (!IsIntelliSenceWord(strIntelliSence))
				return ;

			nCurrOffset = nCurrOffset + 1;
		}

		FillIntelliSenseList(lstIntelliSenseData, strIntelliSence.GetBuffer(0));

		CPoint pt(0, 0);
		OffsetToPoint(m_nCurrOffset = nCurrOffset, pt);
		ClientToScreen(&pt);

		InvokeIntelliSense(lstIntelliSenseData, pt);
	}
}

//-----------------------------------------------------------------------------
BOOL CCustomEditCtrl::InvokeIntelliSense()
{
	return __super::InvokeIntelliSense();
}

//-----------------------------------------------------------------------------
BOOL CCustomEditCtrl::InvokeIntelliSense(CObList& lstIntelliSenseData, CPoint ptTopLeft)
{
	BOOL intelliCreated = TRUE;
	if (lstIntelliSenseData.IsEmpty())
	{
		return FALSE;
	}

	if (m_pIntelliSenseWnd && ::IsWindow(m_pIntelliSenseWnd->m_hWnd))
	{
		ReleaseIntelliSenseList(lstIntelliSenseData); //lstIntelliSenseData.RemoveAll();

		m_pIntelliSenseWnd->PostMessage(WM_CLOSE, 0, 0);

		PostMessage(UM_GET_DOCUMENT_TITLE_INFO, 0, 0);
		return FALSE;
	}

	IntellisenseWndExtended* pIntelliSenseWnd = new IntellisenseWndExtended;
	intelliCreated = pIntelliSenseWnd->Create(lstIntelliSenseData,
		WS_POPUP | WS_VISIBLE | MFS_SYNCACTIVE | WS_BORDER ,
		ptTopLeft, this, m_pIntelliSenseLBFont, m_pIntelliSenseImgList);
	
	
	SetIntelliSenseWnd(pIntelliSenseWnd);
	return intelliCreated;
}

//-----------------------------------------------------------------------------
LRESULT CCustomEditCtrl::PostInvokeIntelliSense(WPARAM, LPARAM)
{
	CObList lstIntelliSenseData;
	CString strIntelliSense;
	int nCurrOffset = m_nCurrOffset;

	if (IsIntelliSenseEnabled() &&
		(nCurrOffset > 0) &&
		(nCurrOffset <= m_strBuffer.GetLength()) &&
		m_strIntelliSenseChars.Find(m_strBuffer.GetAt(nCurrOffset - 1)) != -1)
	{
		if (OnFillIntelliSenseList(nCurrOffset, lstIntelliSenseData))
		{
			SetFocus(); 
			HideCaret(); 
			ShowCaret();

			CPoint pt(0, 0);
			OffsetToPoint(m_nCurrOffset = nCurrOffset, pt);
			ClientToScreen(&pt);

			InvokeIntelliSense(lstIntelliSenseData, pt);
		}
	}

	return 0;
}



//-----------------------------------------------------------------------------
BOOL CCustomEditCtrl::EnableBreakpoints(BOOL bFl /* = TRUE */)
{
	const BOOL bEnableBreakpoints = m_bEnableBreakpoints;
	m_bEnableBreakpoints = bFl;

	return bEnableBreakpoints;
}

//-----------------------------------------------------------------------------
BOOL CCustomEditCtrl::PointOutBreakpointMarker(int nCurrRow)
{
	SetLineColorMarker(nCurrRow,  RGB(0, 255, 0), RGB(180, 80, 80), TRUE, 0, g_dwColorBreakPointType, 2);
	
	return TRUE;
}

BOOL CCustomEditCtrl::SetMarker(int nCurrRow, BOOL bCurrent/* = FALSE*/)
{
	CBCGPEditMarker* pMarker = NULL;
	if (!GetMarker(nCurrRow, &pMarker, g_dwBreakPointType))
		ToggleMarker(nCurrRow, g_dwBreakPointType, NULL, FALSE, 2);
	
	//when bCurrent it PointOut the Breakpoint Marker
	SetLineColorMarker(nCurrRow, (bCurrent ? RGB(255, 255, 255) : RGB(0, 0, 0)), RGB(255, 127, 127), TRUE, 0, g_dwColorBreakPointType, 2);

	return TRUE;
}

BOOL CCustomEditCtrl::ToggleBreakpoint()
{
	int nCurrRow = GetCurRow();

	BOOL bMarkerSet = ToggleMarker(nCurrRow, g_dwBreakPointType, NULL, FALSE, 2);

	if (bMarkerSet)
	{
		SetLineColorMarker(nCurrRow,RGB(0, 0, 0), RGB(255, 127, 127), TRUE, 0, g_dwColorBreakPointType, 2);
	}
	else
	{
		DeleteMarker(nCurrRow, g_dwColorBreakPointType);
	}

	CFrameWnd* pFr = this->GetParentFrame();
	CRSEditorDebugFrame* pRSFr = dynamic_cast<CRSEditorDebugFrame*>(pFr);
	if (pRSFr && pRSFr->m_pToolTreeView)
	{
		((CRSEditorToolDebugView*)pRSFr->m_pToolTreeView)->ToggleBreakpoint(nCurrRow, bMarkerSet);
	}

	return bMarkerSet;
}

//-----------------------------------------------------------------------------
void CCustomEditCtrl::RemoveAllBreakpoints()
{
	DeleteAllMarkers(g_dwBreakPointType | g_dwColorBreakPointType);
}

 /////////////////////////////////////////////////////////////
//////////////////IntellisenseWndExtended////////////////////
/////////////////////////////////////////////////////////////

BEGIN_MESSAGE_MAP(IntellisenseWndExtended, CBCGPIntelliSenseWnd)
	
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------

IntellisenseWndExtended::~IntellisenseWndExtended()
{
	
}


//-----------------------------------------------------------------------------

BOOL IntellisenseWndExtended::DestroyWindow()
{
	/*m_pLstBoxData->DestroyWindow();
	m_pLstBoxData = NULL;*/
	return __super::DestroyWindow();
}



/////////////////////////////////////////////////////////////
//////////////////IntellisenseMap////////////////////
/////////////////////////////////////////////////////////////


IntellisenseMap::IntellisenseMap() {
	root = this->getNode();
}

IntellisenseMap::IntellisenseNode* IntellisenseMap::getNode() {

 IntellisenseNode *pNode = new IntellisenseNode;

	pNode->isEndOfWord = false;

	for (int i = 0; i < this->alphaberSizeExtended; i++)
		pNode->children[i] = NULL;

	return pNode;
}

void IntellisenseMap::insert(CString key, IntellisenseData* data) {
	 IntellisenseNode *pCrawl = root;

	for (int i = 0; i < key.GetLength(); i++)
	{
		int index = key[i] - (char)33;
		if (!pCrawl->children[index])
			pCrawl->children[index] = getNode();

		pCrawl = pCrawl->children[index];
	}

	// mark last node as leaf
	pCrawl->isEndOfWord = true;
	pCrawl->data = data;
}

IntellisenseData* IntellisenseMap::search(CString key) {
	IntellisenseNode *pCrawl = root;

	for (int i = 0; i < key.GetLength(); i++)
	{
		int index = key[i] - (char)33;
		if (!pCrawl->children[index])
			return NULL;

		pCrawl = pCrawl->children[index];
	}

	if (pCrawl != NULL && pCrawl->isEndOfWord && pCrawl->data)
		return pCrawl->data;
	else return NULL;
}