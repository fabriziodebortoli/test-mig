#include "stdafx.h"

#include <stdlib.h>
#include <winspool.h>
// Extern library declarations
#include <bclw/bclw.h>

#include "BASEOBJ.h"
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric/SettingsTable.h>
#include <TbGeneric/ParametersSections.h>
#include <TbGeneric/DataObj.h>
#include <TbGenlib/TBPropertyGrid.h>
#include <TbGenlib/BaseApp.h>
#include <TbGenlib/TBExplorerInterface.h>
#include <TbGenlibUI/SettingsTableManager.h>
#include <TbGenlibUI/FontsDialog.h>
#include <TbGenlibUI/TBExplorer.h>
#include <TbGenlib/BarCode.h>

#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/QueryObject.h>
#include <TbWoormEngine/rpsymtbl.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/events.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/repdata.h>
#include <TbWoormEngine/qrydata.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/edtmng.h>
#include <TbWoormEngine/edtcmm.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include "ListDlg.h"
#include "WoormFrm.h"

#include "WoormVw.h"
#include "Column.h"
#include "Table.h"
#include "Repeater.h"
#include "mulselob.h"
#include "DocProperties.h"
#include "baseobj.h"
#include "mclrdlg.h"
#include "chart.h"

#include "RSEditorUI.h"
#include "RSEditView.h"
#include "CustomEditCtrl.h"
#include "RSEditor_Property.h"

#include "RSEditorUI.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//#ifdef _DEBUG
//#undef THIS_FILE
//static const char THIS_FILE[] = __FILE__;
//#endif
//
#define PROP_HAS_LIST 0x0001

void GetPrinters(CStringArray& szaPrinterArray){
	DWORD dwNeeded = 0, dwItems = 0;
	LPBYTE lpPrinterInfo = NULL;
	
	EnumPrinters(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS, NULL, 2, NULL, 0, &dwNeeded, &dwItems);
	//Enumerating all the printers available in the system

	lpPrinterInfo = new BYTE[dwNeeded];

	if (lpPrinterInfo)
	{
		//Get the data in the BYTE pointer
		if (EnumPrinters(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS, NULL, 2, lpPrinterInfo, dwNeeded, &dwNeeded, &dwItems))
		{
			for (int i = 0; i< (int)dwItems; i++)
				//Loop through them and add them to the String Array
				szaPrinterArray.Add((((LPPRINTER_INFO_2)lpPrinterInfo) + i)->pPrinterName);
		}
	}

	delete lpPrinterInfo;
}

CString GetControlStyleString(AskFieldData::CtrlStyle style)
//TODO stringhe da localizzare ?
{
	switch (style){
	case AskFieldData::CtrlStyle::CHECK_BOX:
		return L"CheckBox";
	case AskFieldData::CtrlStyle::RADIO_BUTTON:
		return L"Radio";
	case AskFieldData::CtrlStyle::COMBO_BOX:
		return L"Combo";
	case AskFieldData::CtrlStyle::EDIT:
		return L"Edit (Default)";
	case AskFieldData::CtrlStyle::EDIT_HKL:
		return L"Edit (Forced)";
	default:
		return L"None";
	}
}

CString MakeStringFromBool(BOOL val)
//TODO stringhe da localizzare ?
{
	if (val == TRUE)
		return L"True";
	return L"False";
}

BOOL MakeBoolFromString(CString str)
//TODO stringhe da localizzare ?
{
	if (str.Compare(L"True") == 0)
		return TRUE;
	return FALSE;
}

CString GetTokenString(Token tk)
//TODO stringhe da localizzare ?
{
//TODO return ::cwsprintf(tk);
	switch (tk)
	{
	case Token::T_NOT:
		return L"None";
	case Token::T_UPPER_LIMIT:
		return L"Upper";
	case Token::T_LOWER_LIMIT:
		return L"Lower";
	case Token::T_DEFAULT:
		return L"Default";
	case Token::T_LEFT:
		return L"Left";
	case Token::T_TOP:
		return L"Top";
	default:
		return L"None";
	}
}

CString GetLinkTypeString(WoormLink::WoormLinkType str)
//TODO stringhe da localizzare ?
{
	switch (str){
	case WoormLink::WoormLinkType::ConnectionForm:
		return L"Link to Form";
	case WoormLink::WoormLinkType::ConnectionReport:
		return L"Link to Report";		
	case WoormLink::WoormLinkType::ConnectionFunction:
		return L"Link to Function";
	case WoormLink::WoormLinkType::ConnectionURL:
		return L"Link to URL";
	default:
		return L"None";
	}
}

CString GetLinkSubTypeString(WoormLink::WoormLinkSubType str)
//TODO stringhe da localizzare ?
{
	switch (str){
	case WoormLink::WoormLinkSubType::File:
		return L"File";
	case WoormLink::WoormLinkSubType::Url:
		return L"URL";
	case WoormLink::WoormLinkSubType::MailTo:
		return L"Mail to";
	case WoormLink::WoormLinkSubType::CallTo:
		return L"Call to:";
	case WoormLink::WoormLinkSubType::GoogleMap:
		return L"Google map";
	default:
		return L"None";
	}
}

CString GetWoormIniType(WoormIni::ShowType type){
	switch (type){
	case WoormIni::ShowType::ID:
		return _TB("Internal name");
	case WoormIni::ShowType::NAME:
		return _TB("Public name");
	case WoormIni::ShowType::VALUE:
		return _TB("Value");
	default: 
		return L"None";
	}
}

CString FormatExprAndDescription(CString description, CString expr)
{
	//return (description + _TB("\n\nExpression associated:\r\n") + expr.TrimLeft(_T("\r\n")));				// description and expression
	return (_TB("Expression associated:\r\n") + expr.TrimLeft(_T("\r\n")) + _T("\r\n\r\n") + description);	// expression and description
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRS_PropertyGrid, CBCGPPropList)

BEGIN_MESSAGE_MAP(CRS_PropertyGrid, CBCGPPropList)

	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//ON_WM_ERASEBKGND()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRS_PropertyGrid::CRS_PropertyGrid()
{
	InitializeImgList();
}

CRS_PropertyGrid::~CRS_PropertyGrid()
{
}

// NO REFACTORING DI QUESTO METODO	   !!!!!!!
// ----------------------------------------------------------------------------
BOOL CRS_PropertyGrid::PreTranslateMessage(MSG* pMsg)
{
	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000;
	BOOL ctrlPressed = GetKeyState(VK_CONTROL) & 0x8000;

	if (pMsg->message == WM_RBUTTONUP)
		return TRUE;
	if (pMsg->message == WM_CONTEXTMENU)
		return TRUE;

	if (pMsg->wParam == VK_TAB && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CBCGPProp* prop = GetCurSel();
		CBCGPProp* nextProp = NULL;
		if (prop)
			nextProp = GetNextVisibleInFilterProperty(prop);//GetNextProperty(prop);
		if (nextProp)
		{
			SetCurSel(nextProp,TRUE);
			EnsureVisible(nextProp);

			if (nextProp->IsAllowEdit())
			{
			   nextProp->DoEdit();
			   CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, ((CrsProp*)nextProp)->m_pWndInPlace);
			   if (::IsWindow(pEdit->GetSafeHwnd()))
			   {
				   CString str=nextProp->GetValue();
				   pEdit->SetSel(str.GetLength(), str.GetLength(), FALSE);
			   }
			}

			return TRUE;
		}				
	}

	if (pMsg->wParam == VK_TAB && pMsg->message == WM_KEYDOWN && shiftPressed)
	{
		CBCGPProp* prop = GetCurSel();
		CBCGPProp* nextProp = NULL;
		if (prop)
			nextProp = GetPrevVisibleInFilterProperty(prop);//GetPrevProperty(prop);
		if (nextProp)
		{
			SetCurSel(nextProp, TRUE);
			EnsureVisible(nextProp);

			if (nextProp->IsAllowEdit())
			{
				nextProp->DoEdit();
				CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, ((CrsProp*)nextProp)->m_pWndInPlace);
				if (::IsWindow(pEdit->GetSafeHwnd()))
				{
					CString str = nextProp->GetValue();
					pEdit->SetSel(str.GetLength(), str.GetLength(), FALSE);
				}
			}

			return TRUE;
		}
	}

	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop && prop->IsAllowEdit())
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);

				int charsStart = 0, charsEnd = 0;
				pEdit->GetSel(charsStart, charsEnd);
				if (charsStart < charsEnd)
				{
					pEdit->Clear();
					return TRUE;
				}
				
				CString subStrStart = str.Mid(0, charsStart);
				CString subStrEnd;
				if (str.GetLength()== charsStart)
					subStrEnd = L"";
				else
					subStrEnd= str.Mid(charsStart +1);
				CString finaStr = subStrStart + subStrEnd;

				prop->SetValue((LPCTSTR)finaStr);
				prop->DoEdit();
				pEdit->SetSel(charsStart, charsStart, FALSE);
			
				return TRUE;
			}
		}	
	}

	if (pMsg->wParam == 0x5A && pMsg->message == WM_KEYDOWN && ctrlPressed)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop && prop->IsAllowEdit())
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (pEdit && ::IsWindow(pEdit->GetSafeHwnd()))
			{
				pEdit->Undo();
				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int chars = pEdit->CharFromPos(pEdit->GetCaretPos());
				if (chars != 0)
					pEdit->SetSel(chars - 1, chars - 1);
				else
					pEdit->SetSel(0, 0);

			 return TRUE;
			}
		}		
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN && shiftPressed)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int cPos = pEdit->CharFromPos(pEdit->GetCaretPos());
				int charsStart=0, charsEnd=0;
				pEdit->GetSel(charsStart, charsEnd);
				int pos = 0;

				if (cPos == charsStart)
				{
					pos = charsStart != 0 ? charsStart - 1 : 0;
					pEdit->SetSel(pos, charsEnd);
				}

				else
				{
					pos = charsEnd > charsStart ? charsEnd - 1 : charsStart;
					pEdit->SetSel(charsStart, pos);
				}
					
				pEdit->SetCaretPos(pEdit->PosFromChar(pos));

				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{															  
		CrsProp* prop = dynamic_cast<CrsProp*>(GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int chars = pEdit->CharFromPos(pEdit->GetCaretPos());
				if (chars < str.GetLength())
					pEdit->SetSel(chars +1, chars +1);
				else
					pEdit->SetSel(str.GetLength(), str.GetLength()); 
			
				return TRUE;
			}		
		}
	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN && shiftPressed)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int cPos = pEdit->CharFromPos(pEdit->GetCaretPos());
				int charsStart = 0, charsEnd = 0;
				pEdit->GetSel(charsStart, charsEnd);
				int pos = 0;
				if (cPos == charsEnd || cPos==-1)
				{
					pos = charsEnd < str.GetLength() ? charsEnd + 1 : str.GetLength();
					pEdit->SetSel(charsStart, pos);					
				}
					
				else
				{ 	
					pos = charsStart < charsEnd ? charsStart + 1 : charsEnd;
					pEdit->SetSel(pos, charsEnd);					
				}

				pEdit->SetCaretPos(pEdit->PosFromChar(pos));

				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_HOME && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CrsProp* prop =dynamic_cast<CrsProp*>(GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);			
				pEdit->SetSel(0,0);
				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_END && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CrsProp* prop = dynamic_cast<CrsProp*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->m_pWndInPlace);
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);			
				pEdit->SetSel(str.GetLength(), str.GetLength());
				return TRUE;
			}
		}
	}

	if ((pMsg->wParam == VK_ESCAPE && pMsg->message == WM_KEYDOWN))
	{
		CrsProp* propCur = dynamic_cast<CrsProp*>(GetCurSel());
		if (propCur)
		{

			if (propCur->GetOptionCount() > 0)
				propCur->DoEdit();
			if (propCur->m_pWndCombo && propCur->m_pWndCombo->GetDroppedState())
				propCur->m_pWndCombo->ShowDropDown(FALSE);
		}

		return TRUE;
	}

	// NO REFACTORING
	if ((pMsg->wParam == VK_DOWN && pMsg->message == WM_KEYDOWN) || ((short)HIWORD(pMsg->wParam)< 0 && pMsg->message == WM_MOUSEWHEEL))
	{
		CrsProp* propCur = dynamic_cast<CrsProp*>(GetCurSel());
		if (propCur)
		{
			if (propCur->GetOptionCount()>0 && !propCur->m_pWndCombo)
				propCur->DoEdit();

			if (propCur->m_pWndCombo && propCur->IsEnabled())
			{
				{
					// gestione filtraggio
					CRSCommonProp* commonProp = dynamic_cast<CRSCommonProp*>(propCur);
					if (commonProp)
					{
						if (!commonProp->m_pWndCombo->GetDroppedState() &&
							 (commonProp->m_eType == CRSCommonProp::NEW_RULE_TABLES ||
							 commonProp->m_eType == CRSCommonProp::NEW_RULE_MODULE  ||
							commonProp->m_eType == CRSCommonProp::SELECT_TABLE_NAME	||
								 commonProp->m_eType == CRSCommonProp::NEW_ENUMTYPE	||
								 commonProp->m_eType ==CRSCommonProp::PropType::NEW_COLUMN_ENUM
								 ) )
						{
							commonProp->OnUpdateValue();	

							if (!propCur->m_pWndCombo)
								return TRUE;
						}
					}
				}

				ASSERT_VALID(propCur);
				ASSERT_VALID(propCur->m_pWndCombo);

				if (!propCur->m_pWndCombo->GetDroppedState())
					propCur->m_pWndCombo->ShowDropDown();
				int curSel = propCur->m_pWndCombo->GetCurSel();
				if (curSel < propCur->m_pWndCombo->GetCount() - 1)
					curSel++;
				else
					curSel = 0;
				propCur->m_pWndCombo->SetCurSel(curSel);
				return TRUE;
			}
		}
	}

	//NO REFACTORING
	if ((pMsg->wParam == VK_UP && pMsg->message == WM_KEYDOWN) || ((short)HIWORD(pMsg->wParam)>0 && pMsg->message == WM_MOUSEWHEEL))
	{
		CrsProp* propCur = dynamic_cast<CrsProp*>(GetCurSel());
		if (propCur)
		{
			
			if (propCur->GetOptionCount()>0 && !propCur->m_pWndCombo)
				propCur->DoEdit();

			if (propCur->m_pWndCombo && propCur->IsEnabled())
			{
					CRSCommonProp* commonProp = dynamic_cast<CRSCommonProp*>(propCur);
					if (commonProp)
					{
						if (!commonProp->m_pWndCombo->GetDroppedState() &&
							(commonProp->m_eType == CRSCommonProp::NEW_RULE_TABLES ||
								commonProp->m_eType == CRSCommonProp::NEW_RULE_MODULE ||
								commonProp->m_eType == CRSCommonProp::SELECT_TABLE_NAME ||
								commonProp->m_eType == CRSCommonProp::NEW_ENUMTYPE ||
								commonProp->m_eType == CRSCommonProp::PropType::NEW_COLUMN_ENUM
								))
						{
							commonProp->OnUpdateValue();
							
							if (!propCur->m_pWndCombo)
								return TRUE;
						}

					}
				
				if (!propCur->m_pWndCombo->GetDroppedState())
					propCur->m_pWndCombo->ShowDropDown();
				int curSel = propCur->m_pWndCombo->GetCurSel();
				if (curSel > 0)
					curSel--;
				else
					curSel = propCur->m_pWndCombo->GetCount() - 1;
				propCur->m_pWndCombo->SetCurSel(curSel);
				return TRUE;	
			}
		}
	}

	if (pMsg->wParam == VK_RETURN && pMsg->message == WM_KEYDOWN)
	{
		CRSCheckBoxProp* propBox = dynamic_cast<CRSCheckBoxProp*>(GetCurSel());
		if (propBox && propBox->m_rectCheck)
		{	
			propBox->OnDblClick(CPoint(-1, -1));
			return TRUE;
		}
	}

	if (pMsg->message == WM_KEYDOWN && (pMsg->wParam >= 0x30 && pMsg->wParam<=VK_DIVIDE))
	{
		CrsProp* propCur = dynamic_cast<CrsProp*>(GetCurSel());

		if (propCur && propCur->m_pWndCombo && propCur->m_pWndCombo->GetDroppedState())
			propCur->m_pWndCombo->ShowDropDown(FALSE);
	}

	if ((pMsg->wParam == VK_ADD && pMsg->message == WM_KEYDOWN) || (pMsg->wParam == VK_SUBTRACT && pMsg->message == WM_KEYDOWN))
	{
		//gestione resa necessaria perch� i caratteri "+" e "-" sono catturati dalla property 
		//list per il collapse ed expand dei gruppi
		CrsProp* propCur = dynamic_cast<CrsProp*>(GetCurSel());
		if (propCur && !propCur->IsGroup() && propCur->IsAllowEdit())
		{
			_variant_t varValue = propCur->GetValue();
			if (varValue.vt == VT_BSTR) //solo properties di tipo testo
			{
				CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, propCur->m_pWndInPlace);
				if (::IsWindow(pEdit->GetSafeHwnd()))
				{
					CString str;
					pEdit->GetWindowText(str);
					int start, end;
					pEdit->GetSel(start, end);
					if ((end - start) > 0)
						str.Delete(start, end - start);

					str.Insert(start, pMsg->wParam == VK_ADD ? L"+" : L"-");

					pEdit->SetWindowTextW(str);
					pEdit->SetSel(start + 1, start + 1);
				}	

				return TRUE;
			}
		}
	}

	return __super ::PreTranslateMessage(pMsg);
}

// ----------------------------------------------------------------------------
CBCGPProp* CRS_PropertyGrid::GetNextVisibleInFilterProperty(CBCGPProp* prop, BOOL cycleFinished /*FALSE*/)
{
	CBCGPProp* pNextProp = GetNextProperty(prop, cycleFinished);
	while (prop != pNextProp && !pNextProp->IsVisibleInFilter())
		pNextProp = GetNextProperty(pNextProp, cycleFinished);
	return pNextProp;
}

// ----------------------------------------------------------------------------
CBCGPProp* CRS_PropertyGrid::GetNextProperty(CBCGPProp* prop, BOOL cycleFinished /*FALSE*/)
{
	if (!prop->IsExpanded() && !cycleFinished)
		return GetNextProperty(prop, TRUE);

	CBCGPProp* parent = prop->GetParent();

	// if prop has suitems it will return the first subitem
	if (!cycleFinished && prop->IsExpanded())
	{
		for (int i = 0;i < prop->GetSubItemsCount();i++)
		{
			if (prop->GetSubItem(i)->IsVisible() )
				return prop->GetSubItem(i);
		}
	}
	
	// if parent is not property grid
	if (parent) {
		//if it doesnt have subitems it seraches the current property
		for (int i = 0;i < parent->GetSubItemsCount();i++)
		{
			if (parent->GetSubItem(i) == prop)
			{
				//if the property is  the last subitem of parent
				if (i == parent->GetSubItemsCount() - 1)
					return 	GetNextProperty(parent, TRUE);
				else // visit next subitem
					return parent->GetSubItem(i + 1);
			}
		}
	}

	else
	{
		for (int i = 0;i < GetPropertyCount();i++)
		{
			if (GetProperty(i) == prop)
			{
				//if the property is  the last subitem of parent
				if (i == GetPropertyCount() - 1)
						return GetProperty(0);

				else // return next subitem
					return GetProperty(i + 1);
			}
		}
	}

	return prop;
}

// ----------------------------------------------------------------------------
CBCGPProp* CRS_PropertyGrid::GetPrevVisibleInFilterProperty(CBCGPProp* prop, BOOL justStarted /*FALSE*/)
{
	CBCGPProp* pPrevProp = GetPrevProperty(prop, justStarted);
	while (prop != pPrevProp && !pPrevProp->IsVisibleInFilter())
		pPrevProp = GetPrevProperty(pPrevProp, justStarted);
	return pPrevProp;
}

// ----------------------------------------------------------------------------
CBCGPProp* CRS_PropertyGrid::GetPrevProperty(CBCGPProp* prop, BOOL justStarted /*FALSE*/)
{
	
	if ((prop->GetSubItemsCount() == 0 || !prop->IsExpanded()) && !justStarted)
		return prop;

	CBCGPProp* parent = prop->GetParent();

	// if prop has suitems it will return the first subitem
	if (!justStarted && prop->IsExpanded())
	{
		for (int i = prop->GetSubItemsCount()-1 ;i >= 0;i--)
		{
			if (prop->GetSubItem(i)->IsVisible())
				return GetPrevProperty(prop->GetSubItem(i),FALSE);
		}
	}

	// if parent is not property grid
	if (parent) {
		//if it doesnt have subitems it seraches the current property
		for (int i = parent->GetSubItemsCount()-1;i >= 0;i--)
		{
			if (parent->GetSubItem(i) == prop)
			{
				//if the property is  the first subitem of parent
				if (i == 0)
					return parent;
				else // visit prev subitem
					return GetPrevProperty(parent->GetSubItem(i - 1),FALSE);
			}
		}
	}

	else
	{
		for (int i = GetPropertyCount()-1;i >=0;i--)
		{
			if (GetProperty(i) == prop)
			{
				//if the property is  the first subitem of parent
				if (i == 0)
					return GetPrevProperty(GetProperty(GetPropertyCount()-1),FALSE);

				else // return prev subitem
					return GetPrevProperty(GetProperty(i-1)  ,FALSE);
			}
		}
	}

	return prop;
}

//-----------------------------------------------------------------------------
void CRS_PropertyGrid::EnsureParentPropertiesVisible(CBCGPProp* pProp)
{
	CrackProp* parentProp = (CrackProp*)pProp->GetParent();
	while (parentProp)
	{
		parentProp->m_bInFilter = TRUE;
		parentProp = (CrackProp*)parentProp->GetParent();
	}
}

//-----------------------------------------------------------------------------
void CRS_PropertyGrid::ExpandCurrentProperty(BOOL bExpand, BOOL bDeep)
{
	if (m_pSel && m_pSel->IsGroup())
	{
		if (bDeep)
		{
			((CrackProp*)m_pSel)->ExpandDeep(bExpand);
			AdjustLayout();
		}

		else
			m_pSel->Expand(bExpand);
	}
}

//-----------------------------------------------------------------------------
void CRS_PropertyGrid::SetPropertiesFilter(LPCTSTR lpszFilter)
{
	if (GetPropertyCount() == 0)
		return;
	//fix filter string
	m_strFilter = lpszFilter == NULL ? _T("") : lpszFilter;
	m_strFilter.MakeUpper();
	//remove selected property
	m_pSel = NULL;
	//expand all nodes
	ExpandAll();
	//retrieve first property
	CBCGPProp* firstProp = GetProperty(0);
	//start from the first
	CBCGPProp* currProp = firstProp;
	do
	{
		if (currProp && currProp->IsKindOf(RUNTIME_CLASS(CrsProp)))
		{
			CrsProp*currCrsProp = (CrsProp*)(currProp);
			currCrsProp->SetFilter(m_strFilter);
			if (currCrsProp->m_bInFilter)
				EnsureParentPropertiesVisible(currCrsProp);
		}

		else if (dynamic_cast<CBCGPProp*>(currProp))
		{
			CrackProp* pCP = (CrackProp*)currProp;
			pCP->SetFilter(m_strFilter);
			if (pCP->m_bInFilter)
				EnsureParentPropertiesVisible(pCP);
		}

		currProp = GetNextProperty(currProp);
	} while (currProp != firstProp);

	//riporto la scrollbar in alto
	m_nVertScrollOffset = 0;
	m_wndScrollVert.SetScrollPos(0);
	//applico il filtro
	AdjustLayout();
}

//-----------------------------------------------------------------------------
void CRS_PropertyGrid::ClearSearchBox()
{
	m_wndFilter.SetWindowText(_T(""));
	SetPropertiesFilter(NULL);
}

//-----------------------------------------------------------------------------
void CRS_PropertyGrid::InitializeImgList()
{
	m_imageList.SetImageSize(CSize(14, 14));
	m_imageList.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageList);

	HICON	hIcon[Img::Max];

	hIcon[Img::QuestionMark]	= TBLoadPng(TBGlyph(szGlyphQuestionMark));
	hIcon[Img::OpenFolder]		= TBLoadPng(TBGlyph(szGlyphOpenFolder14));
	hIcon[Img::CenterPoint]		= TBLoadPng(TBGlyph(szGlyphCenterPoint));
	hIcon[Img::Barcode]			= TBLoadPng(TBGlyph(szGlyphRSBarcode));
	hIcon[Img::Image]			= TBLoadPng(TBGlyph(szGlyphImageFuchsiaBkg));
	hIcon[Img::Textfile]		= TBLoadPng(TBGlyph(szGlyphTextFileFuchsiaBkg));
	hIcon[Img::PrimaryKey]		= TBLoadPng(TBGlyph(szGlyphPrimaryKeyFuchsiaBkg));
	hIcon[Img::ForeignKey]		= TBLoadPng(TBGlyph(szGlyphForeignKeyFuchsiaBkg));
	hIcon[Img::DBVar]			= TBLoadPng(TBGlyph(szGlyphData2));
	hIcon[Img::FuncVar]			= TBLoadPng(TBGlyph(szGlyphFunction));
	hIcon[Img::ExprVar]			= TBLoadPng(TBGlyph(szGlyphExpression));
	hIcon[Img::InputVar]		= TBLoadPng(TBGlyph(szGlyphInputVar));
	hIcon[Img::Rotate]			= TBLoadPng(TBGlyph(szGlyphRotateFuchsiaBkg));

	hIcon[Img::BreakPoint] = TBLoadPng(TBIcon(szIconBreakpoint, CONTROL));
	hIcon[Img::BreakPointCondition] = TBLoadPng(TBIcon(szIconBreakpointCondition, CONTROL));
	hIcon[Img::BreakPointAction] = TBLoadPng(TBIcon(szIconBreakpointAction, CONTROL));
	hIcon[Img::BreakPointConditionAction] = TBLoadPng(TBIcon(szIconBreakpointConditionAction, CONTROL));
	hIcon[Img::BreakPointCurrent] = TBLoadPng(TBIcon(szIconBreakpointCurrent, CONTROL));
	hIcon[Img::BreakPointDisabled] = TBLoadPng(TBIcon(szIconBreakpointDisabled, CONTROL));

	for (int n = 0; n < CRSTreeCtrlImgIdx::MAXGlyph; n++)
	{
		int index = m_imageList.AddIcon(hIcon[n]);
		ASSERT(index == n);
		::DestroyIcon(hIcon[n]);
	}

	EnableSearchBox(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CRS_PropertyGrid::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (!__super::SubclassDlgItem(IDC, pParent))
		return FALSE;

	m_wndScrollVert.SetVisualStyle(CBCGPScrollBar::BCGP_SBSTYLE_VISUAL_MANAGER);

	// Obbligatorio per impedire di far sparire la toolbar (search, order..) quando viene effettuata la ReLayout. 
	// Se non dovesse bastare, copiare OnEraseBkgnd(CDC* pDC) da TBPropertyGrid.cpp e inserirlo nella message map
	ModifyStyle(WS_CLIPCHILDREN, 0);

	InitSizeInfo(this);

	m_bVisualManagerStyle = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
CRS_ObjectPropertyView* CRS_PropertyGrid::GetPropertyView()
{
	CWnd* parentOwner = GetParent();

	if (!parentOwner)
		ASSERT(FALSE);

	CRS_ObjectPropertyView* pPropView = dynamic_cast<CRS_ObjectPropertyView*>(parentOwner);
	ASSERT_VALID(pPropView);

	return pPropView;
}

//-----------------------------------------------------------------------------
//CRSReportTreeView* CRS_PropertyGrid::GetReportTreeView()
//{
//	CRS_ObjectPropertyView* pPropView = GetPropertyView();
//
//	ASSERT_VALID(pPropView->GetDocument()->GetWoormFrame()->m_pReportTreeView);
//	return pPropView->GetDocument()->GetWoormFrame()->m_pReportTreeView;
//}

//-----------------------------------------------------------------------------
//BOOL CRS_PropertyGrid::RemovePropertiesAfter(CBCGPProp* pProp, BOOL bRemoveItAlso)
//{
//	BOOL bFound = FALSE;
//	int i;
//	for (i = GetPropertyCount() - 1; i >= 0; i--)
//	{
//		CBCGPProp* prop = GetProperty(i);
//		if (prop == pProp)
//		{
//			bFound = TRUE;
//			if (bRemoveItAlso)
//				i--;
//			break;
//		}
//	}

//	if (bFound)
//	{
//		for (int j = GetPropertyCount() - 1; j > i && j >= 0; j--)
//		{
//			CBCGPProp* prop = GetProperty(i);
//			DeleteProperty(prop, TRUE, TRUE);
//		}
//	}

//	return bFound;
//}

//=============================================================================
IMPLEMENT_DYNCREATE(CRS_ObjectPropertyView, CAbstractFormView);

BEGIN_MESSAGE_MAP(CRS_ObjectPropertyView, CAbstractFormView)

	ON_REGISTERED_MESSAGE	(BCGM_PROPERTY_COMMAND_CLICKED, OnCommandClicked)

	ON_UPDATE_COMMAND_UI	(ID_RS_REFRESH,		OnUpdateRefresh)

	ON_UPDATE_COMMAND_UI	(ID_RS_COLLAPSEALLTREE, OnUpdateCollapseAll)
	ON_UPDATE_COMMAND_UI	(ID_RS_EXPANDALLTREE,	OnUpdateExpandAll)

	ON_UPDATE_COMMAND_UI	(ID_RS_SAVE,		OnUpdateApply)
	//ON_UPDATE_COMMAND_UI	(ID_RS_DISCARD,		OnUpdateApply)

	ON_UPDATE_COMMAND_UI	(ID_RS_LAYOUT,		OnUpdateLayout)
	ON_UPDATE_COMMAND_UI	(ID_RS_VARIABLE,	OnUpdateVariable)
	ON_UPDATE_COMMAND_UI	(ID_RS_FINDRULE,	OnUpdateFindRule)
	ON_UPDATE_COMMAND_UI	(ID_RS_REQUESTFIELD, OnUpdateRequestField)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRS_ObjectPropertyView::CRS_ObjectPropertyView()
	:
	CAbstractFormView(_T("ObjectProperty"), IDD_RS_ObjectPropertyView),
	m_pPropGrid(NULL),
	//m_pHelpCtrl(NULL),
	//m_sUrlObject(L"www.microarea.it")
	m_pTreeNode(NULL),
	m_pMulSel(NULL),
	m_pMulCol(NULL),
	m_pSelectedTableCell(NULL),
	m_bNeedsApply(FALSE),
	m_bIsHidden(FALSE),
	m_bIsArray(FALSE),
	m_bCreatingNewTable(FALSE),
	m_bFromDragnDrop(FALSE),
	m_bAllFieldsAreHidden(FALSE),
	m_fieldType(WoormField::RepFieldType::FIELD_NORMAL),
	m_bShowLayoutBtn(FALSE),
	m_bCheckLayoutBtn(FALSE),
	m_bCheckLayoutBtnChanged(FALSE),
	m_eShowVariableTypeBtn(WoormField::SourceFieldType::NONE),
	m_bCheckVariableBtn(FALSE),
	m_bCheckVariableBtnChanged(FALSE),
	m_bShowRequestFieldBtn(FALSE),
	m_bCheckRequestFieldBtn(FALSE),
	m_bCheckRequestFieldBtnChanged(FALSE),
	m_bShowFindRuleBtn(FALSE),
	m_bAddNewReportLinkParams(FALSE)
{
}

CRS_ObjectPropertyView::~CRS_ObjectPropertyView()
{
	GetDocument()->GetWoormFrame()->m_pObjectPropertyView = NULL;

	if (m_pPropGrid)
	{
		ASSERT_VALID(m_pPropGrid);
		//m_pPropGrid->RemoveAll();
		//m_pPropGrid->ClearSearchBox();
		//m_pPropGrid->ClearCommands();
		//m_pPropGrid->AdjustLayout();

		//spostata in ClosePanels m_pPropGrid->RemoveAll();

		SAFE_DELETE(m_pPropGrid);
		//viene deletato dal treectrl da cui proviene SAFE_DELETE(m_pTreeNode);
	}

	//SAFE_DELETE(m_pHelpCtrl);
}

// -----------------------------------------------------------------------------
CWoormDocMng* CRS_ObjectPropertyView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// -----------------------------------------------------------------------------
CRSEditView* CRS_ObjectPropertyView::CreateEditView()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument())
		return NULL;
	if (!GetDocument()->GetWoormFrame())
		return NULL;

	CRSEditView* pEdtView = GetDocument()->GetWoormFrame()->CreateEditView();
	return pEdtView;

}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	SetScrollSizes(MM_TEXT, CSize(0, 0));
}

//-----------------------------------------------------------------------------
//Gestione click sui commands collocati soprail footer della propertyGrid
LRESULT CRS_ObjectPropertyView::OnCommandClicked(WPARAM wp, LPARAM lp)
{
	if (m_pTreeNode)
	{
		ASSERT_VALID(m_pTreeNode);
	
		if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			FieldRect* pField = (FieldRect*)m_pTreeNode->m_pItemData;
			switch (pField->m_ShowAs)
			{
			case EFieldShowAs::FT_IMAGE:
			{
				switch (lp)
				{
				case 0: // Copy Attributes From
				{
					pField->OnCopyAttributes();
					pField->UpdateDocument(TRUE);
					break;
				}

				case 1: // Resize keeping image aspect ratio
				{
					pField->OnResizeProportional();
					
					break;
				}

				case 2: // Cut/Undo
				{
					if (!pField->m_bIsCutted)
						pField->OnCutBitmap();
					else
						pField->OnCancelCut();
					break;
				}

				case 3: // SetOriginalSize
				{
					pField->RefreshStandardSize();
					break;
				}
				}

				//ricarico la property grid per aggiornare alcune properties che potrebbero essere cambiate
				CNodeTree* pTmpNode = m_pTreeNode;
				ClearPropertyGrid();
				m_pTreeNode = pTmpNode;
				LoadLayoutObjectPropertyGrid(pTmpNode);
			}
			}
		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(GraphRect)))
		{
			GraphRect* pGraph = dynamic_cast<GraphRect*>(m_pTreeNode->m_pItemData);
			ASSERT_VALID(pGraph);
			switch (lp)
			{
			case 0: // Copy Attributes From
			{
				pGraph->OnCopyAttributes();
				pGraph->UpdateDocument(TRUE);
				break;
			}

			case 1: // Resize keeping image aspect ratio
			{
				pGraph->OnResizeProportional();
				break;
			}

			case 2: // Crop/Show all
			{
				if (!pGraph->m_bIsCutted)
					pGraph->OnCutBitmap();
				else
					pGraph->OnCancelCut();
				break;
			}

			case 3: // SetOriginalSize
			{
				pGraph->RefreshStandardSize();
				break;
			}

			case 4: // Refresh Image
			{
				pGraph->RefreshCurrentSize();
				break;
			}
			}

			//ricarico la property grid per aggiornare alcune properties che potrebbero essere cambiate
			CNodeTree* pTmpNode = m_pTreeNode;
			ClearPropertyGrid();
			m_pTreeNode = pTmpNode;
			LoadLayoutObjectPropertyGrid(pTmpNode);
		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = (Repeater*)m_pTreeNode->m_pItemData;
			switch (lp)
			{
			case 0: //Rebuild
			{
				pRep->OnRebuild();
				break;
			}

			case 1: //Fit the content
			{
				pRep->OnFitTheContent();
				break;
			}

			case 2: // Clear custom styles
			{
				pRep->OnClearCustomStyle();
				LoadPropertyGrid(m_pTreeNode);
				break;
			}

			case 3: // Copy attributes from
			{
				pRep->OnCopyAttributes();
				pRep->UpdateDocument(TRUE);
				break;
			}
			}
		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(Chart)))
		{
			Chart* pChart = (Chart*)m_pTreeNode->m_pItemData;
			switch (lp)
			{
			case 0: // Refresh chart
			{
				pChart->OnCreate();
				break;
			}
			}
		}
		//should be after subclasses
		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBaseRect = (BaseRect*)m_pTreeNode->m_pItemData;
			switch (lp)
			{
				case 0: // Clear custom styles
				{
					pBaseRect->OnClearCustomStyle();
					LoadPropertyGrid(m_pTreeNode);
					break;
				}

				case 1: // Copy attributes from
				{
					pBaseRect->OnCopyAttributes();
					pBaseRect->UpdateDocument(TRUE);
					break;
				}
			}
		}
	
		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = dynamic_cast<Table*>(m_pTreeNode->m_pItemData);
			ASSERT_VALID(pTable);
			switch (lp)
			{
				case 0: // Clear custom styles
				{
					pTable->OnClearAllCustomStyles();

					LoadPropertyGrid(m_pTreeNode);
					break;
				}

				case 1: // Clear custom styles
				{
					pTable->OnClearTableCustomStyles();

					LoadPropertyGrid(m_pTreeNode);
					break;
				}
			}
		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(TableColumn)))
		{
			TableColumn* pCol = dynamic_cast<TableColumn*>(m_pTreeNode->m_pItemData);
			Table* pTable = pCol->GetTable();
			ASSERT_VALID(pTable);
			switch (lp)
			{
			case 0: // Clear custom styles
			{
				pCol->ClearStyle();

				pTable->m_pDocument->InvalidateRect(pCol->GetColumnRect(), TRUE);
				pTable->m_pDocument->UpdateWindow();
				pTable->m_pDocument->SetModifiedFlag();

				LoadPropertyGrid(m_pTreeNode);
				break;
			}

			case 1: // Copy attributes to current colum
			{
				pTable->OnCopyActiveColumnAttributes();

				LoadPropertyGrid(m_pTreeNode);
				break;
			}

			case 2: // Copy header attributes to current colum
			{
				pTable->OnCopyActiveColumnTitleAttributes();

				LoadPropertyGrid(m_pTreeNode);
				break;
		}

			//case : // Delete column
			//{
			//	int index = pTable->GetColumnIndexByPtr(pCol);

			//	if (pTable->DeleteColumn(index))
			//	{
			//		GetDocument()->GetWoormFrame()->m_pReportTreeView->RemoveNode(m_pTreeNode);
			//		pTable->m_pDocument->Invalidate();
			//		pTable->m_pDocument->SetModifiedFlag();
			//		pTable->m_nActiveColumn = Table::NO_ACTIVE_COLUMN;
			//		pTable->m_pDocument->m_pActiveRect->SetActive(pTable->m_TitleRect);
			//		GetDocument()->GetWoormFrame()->m_pReportTreeView->SelectLayoutObject(pTable);
			//	}

			//	break;
			//}
			}
		}

	}

	else if (m_pSelectedTableCell)
	{
		switch (lp)
		{
		case 0: //Apply to all row
		{
			CopyTableCellPropertiesToAllRow();
			break;
		}
		}
	}

	else if (m_pMulSel)
	{
		switch (lp)
		{
		case 0: //Align on the left of the last selected
		{
			m_pMulSel->AlignHLeft();
			m_pMulSel->BuildBaseRect();
			m_pMulSel->Redraw();
			break;
		}

		case 1: //Align on the Top of the last selected
		{
			m_pMulSel->AlignVTop();
			m_pMulSel->BuildBaseRect();
			m_pMulSel->Redraw();
			break;
		}

		case 2: //Same Width of the last selected
		{
			m_pMulSel->SizeLargeAsLast();
			m_pMulSel->BuildBaseRect();
			m_pMulSel->Redraw();
			break;
		}

		case 3: //Same Height of the last selected
		{
			m_pMulSel->SizeHighAsLast();
			m_pMulSel->BuildBaseRect();
			m_pMulSel->Redraw();
			break;
		}
		}
	}

	//BCGPMessageBox(str);
	return 0;
}

// -----------------------------------------------------------------------------
void CRS_ObjectPropertyView::AddBasicCommands()
{
	CStringList lstCommands;
	lstCommands.AddTail(_TB("Clear custom styles"));
	lstCommands.AddTail(_TB("Copy attributes from"));
	m_pPropGrid->SetCommands(lstCommands);
	m_pPropGrid->SendMessage(WM_NCPAINT, DCX_WINDOW, 0L); //TAPULLO by silvano :-P e Andrea per risolvere il problema del mancato disegno dei comandi sotto la property grid
}

// -----------------------------------------------------------------------------
void  CRS_ObjectPropertyView::SetPanelTitle(const CString& sTitle)
{
	GetDocument()->GetWoormFrame()->m_pPropertyPane->SetWindowTextW(_TB("Property") + (!sTitle.IsEmpty() ? L" - " : L"") + sTitle);
}

// -----------------------------------------------------------------------------
void CRS_ObjectPropertyView::SetFocusOnFirstProperty()
{
	if (!m_pPropGrid || m_pPropGrid->GetPropertyCount() == 0) return;
	
	m_pPropGrid->SetFocus();
	CBCGPProp* firstProp = m_pPropGrid->GetFirstNonFilteredProperty();
	CBCGPProp* lastProp = m_pPropGrid->GetLastNonFilteredProperty();

	if (!firstProp) return;

	while (firstProp !=lastProp)
		if (firstProp->IsGroup() || (!firstProp->IsEnabled() && !firstProp->IsAllowEdit()))
			firstProp = m_pPropGrid->GetNextProperty(firstProp);
		else
		{
			m_pPropGrid->SetCurSel(firstProp,TRUE);
			return;
		}
}

// -----------------------------------------------------------------------------
void CRS_ObjectPropertyView::BuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->m_pObjectPropertyView = this;

	/*m_pHelpCtrl = (CParsedWebCtrl*) AddLink(IDC_RS_HelpPropertyGrid, L"ObjectHelp", NULL, &m_sUrlObject, RUNTIME_CLASS(CParsedWebCtrl));
	ASSERT_VALID(m_pHelpCtrl);*/

	//m_pPropGrid = AddLinkPropertyGrid(IDC_RS_PropertyGrid, L"ObjectProperty");
	m_pPropGrid = new CRS_PropertyGrid();
	
	InitCRS_PropertyGrid();

	//m_pPropGrid->SubclassDlgItem(IDC_RS_PropertyGrid, this);
	ASSERT_VALID(m_pPropGrid);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnRefresh()
{
	//TODO
}

void CRS_ObjectPropertyView::OnUpdateRefresh(CCmdUI* pCmdUI)
{
	//TODO
	pCmdUI->Enable(FALSE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnDiscard()
{
	m_bNeedsApply = FALSE;
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnApply()	//OnSave
{
	if (m_pTreeNode)
	{
		switch (m_pTreeNode->m_NodeType)
		{
		case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
		{
			CreateNewLayout();
			return;
		}

		case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
		{
			CreateNewProcedure();
			return;
		}

		case CNodeTree::ENodeType::NT_ROOT_QUERIES:
		{
			CreateNewNamedQuery();
			return;
		}

		case CNodeTree::ENodeType::NT_ASKGROUP:
		{
			CreateNewAskField();
			return;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM:
		{
			CreateNewJoinTable();
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
		{
			CreateNewCalcColumn();
			return;
		}

		case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
		case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:
		{
			CreateNewBreakingEvent(TRUE);
			return;
		}

		case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
		case CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS:
		{
			CreateNewBreakingEvent();
			return;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			ChangeVariableType();
			return;
		}

		case CNodeTree::ENodeType::NT_LINK_PARAMETERS:
		{
			if (m_bAddNewReportLinkParams)
			{
				AddReportLinkParameters();

				m_bAddNewReportLinkParams = FALSE;
			}
			return;
		}

		/*default:
			ASSERT(FALSE);*/
		}
	}

	switch (m_pPropGrid->m_NewElement_Type)
	{
	case CRS_PropertyGrid::NewElementType::NEW_ELEMENT:
	{
		CreateNewElement();
		break;
	}

	case CRS_PropertyGrid::NewElementType::NEW_DB_ELEMENT:
	{
		CreateNewDBElement();
		break;
	}

	case CRS_PropertyGrid::NewElementType::NEW_FUNCTION_LINK:
	{
		CreateNewFunctionLink();
		break;
	}

	case CRS_PropertyGrid::NewElementType::NEW_URL_LINK:
	{
		CreateNewUrlLink();
		break;
	}

	case CRS_PropertyGrid::NewElementType::NEW_FORM_LINK:
	{
		CreateNewFormLink();
		break;
	}

	case CRS_PropertyGrid::NewElementType::NEW_REPORT_LINK:
	{
		//if (m_bAddNewReportLinkParams)
		//{
		//	AddReportLinkParameters();
		//	m_bAddNewReportLinkParams = FALSE;
		//}
		//else
			CreateNewReportLink();

		break;
	}
	}

	//update repeater
	GetDocument()->ApplyRepeater(GetDocument()->m_Objects);
}

void CRS_ObjectPropertyView::OnUpdateApply(CCmdUI* pCmdUI)
{
	//TODO																
	BOOL bEnable = m_bNeedsApply;

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnCollapseAll()
{
	if (!m_pPropGrid || m_pPropGrid->GetPropertyCount() < 1)
		return;
	m_pPropGrid->ExpandAll(FALSE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateCollapseAll(CCmdUI* pCmdUI)
{
	BOOL bEnable = FALSE;

	if (m_pPropGrid && m_pPropGrid->GetPropertyCount() > 0)
		bEnable = TRUE;

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnExpand()
{
	if (!m_pPropGrid || m_pPropGrid->GetPropertyCount() < 1)
		return;
	m_pPropGrid->ExpandCurrentProperty(TRUE, TRUE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateExpandAll(CCmdUI* pCmdUI)
{
	BOOL bEnable = FALSE;
	
	if (m_pPropGrid && m_pPropGrid->GetPropertyCount() > 0)
		bEnable = TRUE;
	
	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnFindRuleBtn()
{
	ASSERT_VALID(m_pTreeNode);
	ASSERT_VALID(m_pTreeNode->m_pItemData);

	WoormField*		wrmField = NULL;
	CWoormDocMng*	wrmDocMng = this->GetDocument();

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng) return;

	if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pField = dynamic_cast<FieldRect*>(m_pTreeNode->m_pItemData);
		if (!pField)
		{
			ASSERT(FALSE);
			return;
		}

		wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pField->GetInternalID());
	}

	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(m_pTreeNode->m_pItemData);
		if (!pCol)
		{
			ASSERT(FALSE);
			return;
		}

		wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pCol->GetInternalID());
	}

	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(WoormField)))
	{
		wrmField = dynamic_cast<WoormField*>(m_pTreeNode->m_pItemData);
	}

	ASSERT_VALID(wrmField);
	if (wrmField && (wrmField->m_bIsTableField || wrmField->IsExprRuleField()))
	{
		//deseleziono il nodo precedentemente selezionato
		GetDocument()->ClearSelectionFromAllTrees(NULL);
		//cerco il wrmfield per selezionarlo sul tree
		wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Rules, wrmField);
		//mostro il tree dell'engine
		if(GetDocument()->GetWoormFrame()->m_pEnginePane)
			GetDocument()->GetWoormFrame()->m_pEnginePane->ShowControlBar(TRUE, FALSE, TRUE);
	}
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateFindRule(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bShowFindRuleBtn);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnLayoutBtn()
{
	ASSERT_VALID(m_pTreeNode);

	CWoormDocMng*	wrmDocMng = this->GetDocument();
	WoormField*		pField = NULL;

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng) return;

	if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(WoormField)))
		pField = dynamic_cast<WoormField*>(m_pTreeNode->m_pItemData);
	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(AskFieldData)))
	{
		AskFieldData* askField = dynamic_cast<AskFieldData*>(m_pTreeNode->m_pItemData);
		
		ASSERT_VALID(askField);
		if (!askField)
			return;

		pField = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable()->GetField(askField->GetPublicName());
	}

	if (!pField)
		return;
	ASSERT_VALID(pField);

	int nID = pField->GetId();
	//cerco l'oggetto grafico associato al wrmfield
	CObject* pObj = wrmDocMng->m_Objects->FindObjectByID(nID);

	if (!pObj) //altrimenti potrebbe essere un totale
		pObj = wrmDocMng->GetTotalCellById(nID);

	//controllo il puntatore
	if (pObj)
	{
		//deseleziono il nodo precedentemente selezionato
		GetDocument()->ClearSelectionFromAllTrees(NULL);

		wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Layouts, pObj);
		//mostro il tree del layout
		if(GetDocument()->GetWoormFrame()->m_pLayoutPane)
			GetDocument()->GetWoormFrame()->m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);
	}
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateLayout(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bShowLayoutBtn);

	//controllo se � cambiato lo stato del pulsante
	if (!m_bCheckLayoutBtnChanged) return;

	//se deve essere checckato
	if (m_bCheckLayoutBtn)
	{
		//lo faccio diventare una checkbox selezionata
		if (GetDocument() && GetDocument()->GetWoormFrame())
		{
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_LAYOUT, TBBS_CHECKBOX);
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_LAYOUT, TBBS_CHECKED);
		}	
	}

	//altrimenti
	else
	{
		//lo faccio tornare bottone
		if (GetDocument() && GetDocument()->GetWoormFrame())
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_LAYOUT, TBBS_BUTTON);
		pCmdUI->Enable(m_bShowLayoutBtn);
	}

	//rimetto il flag del cambiamento a false
	m_bCheckLayoutBtnChanged = FALSE;

	//in teoria dovevo usare questa sola riga di codice ma non funziona
	//pCmdUI->SetCheck(m_bCheckLayoutBtn);
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnVariableBtn()
{
	WoormField*		wrmField = NULL;
	CWoormDocMng*	wrmDocMng = this->GetDocument();

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng) return;

	if (m_pTreeNode)
	{
		if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			FieldRect* pField = dynamic_cast<FieldRect*>(m_pTreeNode->m_pItemData);
			if (!pField)
			{
				ASSERT(FALSE);
				return;
			}

			wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pField->GetInternalID());
			ASSERT_VALID(wrmField);
			if (wrmField)
			{
				wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Layouts, wrmField);
				//mostro il tree del layout
				if (GetDocument()->GetWoormFrame()->m_pLayoutPane)
					GetDocument()->GetWoormFrame()->m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);
			}

		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(TableColumn)))
		{
			TableColumn* pCol = dynamic_cast<TableColumn*>(m_pTreeNode->m_pItemData);
			if (!pCol)
			{
				ASSERT(FALSE);
				return;
			}

			wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pCol->GetInternalID());
			ASSERT_VALID(wrmField);
			if (wrmField)
			{
				wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Layouts, wrmField);
				//mostro il tree del layout
				if (GetDocument()->GetWoormFrame()->m_pLayoutPane)
					GetDocument()->GetWoormFrame()->m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);
			}
		}

		else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(AskFieldData)))
		{
			AskFieldData* askField = dynamic_cast<AskFieldData*>(m_pTreeNode->m_pItemData);
			ASSERT_VALID(askField);
			if (!askField)
				return;

			wrmField = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable()->GetField(askField->GetPublicName());
			ASSERT_VALID(wrmField);
			if (wrmField)
			{
				//deseleziono il nodo precedentemente selezionato
				GetDocument()->ClearSelectionFromAllTrees(NULL);
				//lo seleziono nel tree
				if (wrmField->IsAsk())
					wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Dialogs, wrmField);
				//mostro il tree dell'engine
				if (GetDocument()->GetWoormFrame()->m_pEnginePane)
					GetDocument()->GetWoormFrame()->m_pEnginePane->ShowControlBar(TRUE, FALSE, TRUE);
			}
		}
	}

	else if (m_pSelectedTableCell)
	{
		TotalCell* pTotalCell = dynamic_cast<TotalCell*>(m_pSelectedTableCell);
		if (!pTotalCell)
		{
			ASSERT(FALSE);
			return;
		}

		WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();
		CWordArray	idsColTotal;
		WORD		idColTotal;
		pSymTable->GetTotalOf(pTotalCell->m_pColumn->GetInternalID(), idsColTotal, WoormField::FIELD_COLTOTAL);

		if (idsColTotal.GetSize() > 0)
		{
			idColTotal = idsColTotal[0];
			WoormField* pTotalField = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable()->GetFieldByID(idColTotal);

			ASSERT_VALID(pTotalField);
			if (pTotalField)
			{
				wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Layouts, pTotalField);
				//mostro il tree del layout
				if (GetDocument()->GetWoormFrame()->m_pLayoutPane)
					GetDocument()->GetWoormFrame()->m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);
			}

		}
	}
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateVariable(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_eShowVariableTypeBtn != WoormField::SourceFieldType::NONE );

	//controllo se � cambiato lo stato del pulsante
	if (!m_bCheckVariableBtnChanged) return;

	if (m_eShowVariableTypeBtn != WoormField::SourceFieldType::NONE)
	{
		switch (m_eShowVariableTypeBtn)
		{
		case (WoormField::SourceFieldType::DB_FIELD):
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconVarDb, TOOLBAR));
			break;
		case (WoormField::SourceFieldType::EXPRESSION_FIELD) :
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconVarExpr, TOOLBAR));
			break;
		case (WoormField::SourceFieldType::FUNCTION_FIELD) :
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconVarFunc, TOOLBAR));
			break;
		case (WoormField::SourceFieldType::ASK_FIELD) :
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconVarInput, TOOLBAR));
			break;
		case (WoormField::SourceFieldType::INPUT_FIELD) :
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconVarInput, TOOLBAR));
			break;
		case (WoormField::SourceFieldType::TOTAL_FIELD) :
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->ChangeImage(ID_RS_VARIABLE, TBIcon(szIconColumnTotal, TOOLBAR));
			break;
		}
	}

	//se deve essere checckato
	if (m_bCheckVariableBtn)
	{
		//lo faccio diventare una checkbox selezionata
		if (GetDocument() && GetDocument()->GetWoormFrame())
		{
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_VARIABLE, TBBS_CHECKBOX);
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_VARIABLE, TBBS_CHECKED);
		}
	}

	//altrimenti
	else
	{
		//lo faccio tornare bottone
		if (GetDocument() && GetDocument()->GetWoormFrame())
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_VARIABLE, TBBS_BUTTON);
		pCmdUI->Enable(m_eShowVariableTypeBtn != WoormField::SourceFieldType::NONE);
	}

	//rimetto il flag del cambiamento a false
	m_bCheckVariableBtnChanged = FALSE;

	//in teoria dovevo usare questa sola riga di codice ma non funziona
	//pCmdUI->SetCheck(m_bCheckVariableBtn);
	GetDocument()->SetModifiedFlag();
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnRequestFieldBtn()
{
	ASSERT_VALID(m_pTreeNode);
	ASSERT_VALID(m_pTreeNode->m_pItemData);

	WoormField*		wrmField = NULL;
	CWoormDocMng*	wrmDocMng = this->GetDocument();

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng) return;

	if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pField = dynamic_cast<FieldRect*>(m_pTreeNode->m_pItemData);
		if (!pField)
		{
			ASSERT(FALSE);
			return;
		}

		wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pField->GetInternalID());
	}

	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(m_pTreeNode->m_pItemData);
		if (!pCol)
		{
			ASSERT(FALSE);
			return;
		}

		wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pCol->GetInternalID());
	}

	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(WoormField)))
	{
		wrmField = dynamic_cast<WoormField*>(m_pTreeNode->m_pItemData);
	}

	else if (m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(AskFieldData))) return;

	ASSERT_VALID(wrmField);
	if (wrmField && wrmField->IsAsk())
	{
		AskRuleData* pAskDialogs = wrmDocMng->m_pEditorManager->GetPrgData()->GetAskRuleData();
		ASSERT_VALID(pAskDialogs);

		AskFieldData* pAskField =  pAskDialogs->GetAskField(wrmField->GetName());
		ASSERT_VALID(pAskField);
		//deseleziono il nodo precedentemente selezionato
		GetDocument()->ClearSelectionFromAllTrees(NULL);
		//lo seleziono nel tree
		wrmDocMng->SelectRSTreeItemData(ERefreshEditor::Dialogs, pAskField);
		//mostro il tree dell'engine
		if (GetDocument()->GetWoormFrame()->m_pEnginePane)
			GetDocument()->GetWoormFrame()->m_pEnginePane->ShowControlBar(TRUE, FALSE, TRUE);
	}
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::OnUpdateRequestField(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bShowRequestFieldBtn);

	//controllo se � cambiato lo stato del pulsante
	if (!m_bCheckRequestFieldBtnChanged) return;

	//se deve essere checckato
	if (m_bCheckRequestFieldBtn)
	{
		//lo faccio diventare una checkbox selezionata
		if (GetDocument() && GetDocument()->GetWoormFrame())
		{
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_REQUESTFIELD, TBBS_CHECKBOX);
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_REQUESTFIELD, TBBS_CHECKED);
		}
	}

	//altrimenti
	else
	{
		//lo faccio tornare bottone
		if (GetDocument() && GetDocument()->GetWoormFrame())
			GetDocument()->GetWoormFrame()->m_pPropertyPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_REQUESTFIELD, TBBS_BUTTON);
		pCmdUI->Enable(m_bShowRequestFieldBtn);
	}

	//rimetto il flag del cambiamento a false
	m_bCheckRequestFieldBtnChanged = FALSE;

	//in teoria dovevo usare questa sola riga di codice ma non funziona
	//pCmdUI->SetCheck(m_bCheckRequestFieldBtn);
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::SetCheckLayoutBtn(BOOL checked)
{
	//Layout
	m_bCheckLayoutBtn = checked;
	m_bCheckLayoutBtnChanged = TRUE;
	//Variable
	m_bCheckVariableBtn = !checked;
	m_bCheckVariableBtnChanged = TRUE;
	//Request Field
	m_bCheckRequestFieldBtn = !checked;
	m_bCheckRequestFieldBtnChanged = TRUE;
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::SetCheckVariableBtn(BOOL checked)
{
	//Variable
	m_bCheckVariableBtn = checked;
	m_bCheckVariableBtnChanged = TRUE;
	//Layout
	m_bCheckLayoutBtn = !checked;
	m_bCheckLayoutBtnChanged = TRUE;
	//Request Field
	m_bCheckRequestFieldBtn = !checked;
	m_bCheckRequestFieldBtnChanged = TRUE;
}

//----------------------------------------------------------------------------
void CRS_ObjectPropertyView::SetCheckRequestFieldBtn(BOOL checked)
{
	//Request Field
	m_bCheckRequestFieldBtn = checked;
	m_bCheckRequestFieldBtnChanged = TRUE;
	//Variable
	m_bCheckVariableBtn = !checked;
	m_bCheckVariableBtnChanged = TRUE;
	//Layout
	m_bCheckLayoutBtn = !checked;
	m_bCheckLayoutBtnChanged = TRUE;
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewBreakingEvent(CNodeTree* pNode, BOOL addNewFields, BOOL addNewSubtotal, BOOL addNew)
{
	m_arNewBreakingFields.RemoveAll();
	m_pBreakingFieldListGroup = NULL;
	m_arNewSubTotalFields.RemoveAll();
	m_pSubTotalGroup = NULL; 
	if (!ClearPropertyGrid())
	{
		m_pTreeNode = pNode;
		return;
	}

	// NO REFACTORING
	m_pTreeNode = pNode;

	//----
	EventsData* pEventsData = GetDocument()->m_pEditorManager->GetPrgData()->GetEventsData();
	ASSERT_VALID(pEventsData);

	TriggEventData* pEvent;
	if (addNew)
		pEvent = new TriggEventData(*GetDocument()->GetEditorSymTable());
	else
		pEvent = (TriggEventData*) pNode->m_pItemData;

	ASSERT_VALID(pEvent);

	if (addNewFields)
		SetPanelTitle(_TB("Breaking Event") + L" " + pEvent->GetEventName() + L" - "+ _TB("Add fields"));
	else
		SetPanelTitle(_TB("Breaking Event") + L" " + pEvent->GetEventName());

	m_bNeedsApply = addNewFields || addNewSubtotal;

	CrsProp* pPropGeneral = new CrsProp(_TB("Breaking Event"));

	//----
	CrsProp* pProp = new CRSTriggerEventProp(pEvent, _TB("Name"), pEvent->GetEventName(), CRSTriggerEventProp::PropType::EVENT_NAME, this);
	pProp->SetData((DWORD_PTR)pEvent);
	if (addNew)
		pProp->SetColoredState(CrsProp::State::Mandatory);
	pPropGeneral->AddSubItem(pProp);
	pProp->AllowEdit(addNew);
	//---- 
	CStringArray arFields;

	m_pBreakingFieldListGroup = new CrsProp(_TB("Fields list"));
	pPropGeneral->AddSubItem(m_pBreakingFieldListGroup);
	m_pBreakingFieldListGroup->Show(addNewFields, TRUE);

	pEvent->GetBreakingFields(arFields);
	pProp = new CRSTriggerEventProp(pEvent, _TB("new breaking field"), L"", CRSTriggerEventProp::PropType::NewBreakingField, this);
	pProp->Show(addNewFields, TRUE);
	pProp->SetColoredState(CrsProp::State::Mandatory);
	m_pBreakingFieldListGroup->AddSubItem(pProp);

	//----
	pProp = new CRSTriggerEventProp(pEvent, _TB("AND/OR"), L"", CRSTriggerEventProp::PropType::MustTrueTogether, this);
	pProp->SetDescription(_TB("It is used to interrelate breaking field list to breaking expression where them are not empty both.\n"
		"When the property is TRUE the condition have to be TRUE both to raise the breaking event (it is a logic AND clause);\n"
		"otherwise the event is raised just one of them is TRUE (it is a logic OR clause)."));
	pProp->AddOption(L"False", TRUE, FALSE);
	pProp->AddOption(L"True", TRUE, TRUE);
	pProp->SelectOption(pProp->GetOptionDataIndex(FALSE));
	pPropGeneral->AddSubItem(pProp);
	//pProp->SetVisible(addNewFields && addNewSubtotal);

	//----
	
	CRSExpressionProp* pe = new CRSExpressionProp(_TB("Breaking expression"), &pEvent->m_pWhenExpr, DataType::Bool, &pEvent->m_SymTable, this);
	pPropGeneral->AddSubItem(pe);
	pe->SetDescription(_TB("Breaking event will be raised when the expression evaluation is TRUE"));
	pe->m_bViewMode = FALSE;
	//pe->SetVisible(addNewFields || addNewSubtotal);

	//----
	m_pSubTotalGroup = new CrsProp(_TB("Sub totals"));
	pPropGeneral->AddSubItem(m_pSubTotalGroup);
	m_pSubTotalGroup->Show(addNewSubtotal, TRUE);
	arFields.RemoveAll();
	pEvent->GetSubtotalFields(arFields);
	pProp = new CRSTriggerEventProp(pEvent, _TB("new sub total field"), L"", CRSTriggerEventProp::PropType::NewSubTotalField, this);
	pProp->Show(addNewSubtotal, TRUE);
	pProp->SetColoredState(CrsProp::State::Mandatory);
	m_pSubTotalGroup->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewBreakingEvent(BOOL onlyColumns)
{
	CString name = m_pPropGrid->GetProperty(0)->GetSubItem(0)->GetValue();
	name.Trim();

	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->CheckPropValue(FALSE, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}
	else
	{
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::Normal);
	}

	EventsData* pEventsData = GetDocument()->m_pEditorManager->GetPrgData()->GetEventsData();
	ASSERT_VALID(pEventsData);

	//if name already exists this block highligts the property 
	if (!onlyColumns && pEventsData->GetEventIdx(name) >= 0)
	{
		m_pPropGrid->GetProperty(0)->GetSubItem(0)->SetDescription(_TB("Event name already exists."));
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::State::Mandatory);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::State::Normal);

	TriggEventData*	pEvent = (TriggEventData*)m_pPropGrid->GetProperty(0)->GetSubItem(0)->GetData();
	ASSERT_VALID(pEvent);
	
	if (m_arNewBreakingFields.GetCount() == 0 && m_arNewSubTotalFields.GetCount() == 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(1)->GetSubItem(0))->SetColoredState(CrsProp::State::Mandatory);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(1)->GetSubItem(0))->SetColoredState(CrsProp::State::Normal);

	pEvent->SetEventName(name);
	if (!onlyColumns)
		pEventsData->m_TriggEvents.Add(pEvent);

	while (m_arNewBreakingFields.GetCount())
	{
		CString sName = m_arNewBreakingFields[0];
		if (sName.IsEmpty())
			continue;

		pEvent->m_BreakList.Add(sName);

		CStringArray_Remove(m_arNewBreakingFields, sName, TRUE, TRUE);
	}

	while (m_arNewSubTotalFields.GetCount())
	{
		CString sName = m_arNewSubTotalFields[0];
		if (sName.IsEmpty())
			continue;

		VERIFY(pEvent->CreateSubTotalField(sName, pEventsData));

		CStringArray_Remove(m_arNewSubTotalFields, sName, TRUE, TRUE);
	}

	m_bNeedsApply = FALSE;

	//Sync tree
	if (!onlyColumns)
	{
		GetDocument()->GetRSTree(ERefreshEditor::Events)->FillTriggerEvent(pEventsData, pEvent, TRUE);
	}

	else
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Events, pEvent);
	}

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::ChangeVariableType()
 {
	ASSERT(m_pTreeNode->m_pItemData);

	CBCGPProp *selectedType = m_pPropGrid->GetProperty(0)->GetSubItem(1);

	int index = selectedType->GetSelectedOption();
	if (index < 0) 
		return;
	DWORD_PTR option = selectedType->GetOptionData(index);

	WoormField* field = dynamic_cast<WoormField*> (m_pTreeNode->m_pItemData);
	WoormTable*	pSymTable = field->GetSymTable();

	//if the field type (expression/function) has changed
	if ((field->IsExprRuleField() && option == 0) || (!field->IsExprRuleField() && option == 1))
	{

		//function
		if (option == 0) {
			GetDocument()->m_pEditorManager->GetRuleData()->DeleteField(field->GetName());
			field->m_pEventFunction = new EventFunction(pSymTable, *field);
			field->SetFieldType(WoormField::RepFieldType::FIELD_NORMAL);
			field->SetExprRuleField(FALSE);
		}

		//expression
		if (option == 1) 
		{
			field->m_pEventFunction = NULL;
			if (field->GetFieldType() == WoormField::RepFieldType::FIELD_INPUT)
				field->SetFieldType(WoormField::RepFieldType::FIELD_NORMAL);
			field->SetExprRuleField(TRUE);
			ExpRuleData*	pExpRuleData = new ExpRuleData(*pSymTable);
			pExpRuleData->SetPublicName(field->GetName());
			DataObj* p = DataObj::DataObjCreate(field->GetDataType());
			CString data = p->FormatData();
			SAFE_DELETE(p);
			if (data.IsEmpty())
				data = L"\" \"";
			pExpRuleData->m_ThenExpr.AssignStr(data);
			GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData()->Add(pExpRuleData);
		}

		GetDocument()->SyncronizeViewSymbolTable(pSymTable);
	}

	// if the field type changed (normal to input and vice versa)
	else  if (!field->IsExprRuleField() && option == 0)
	{

		CBCGPProp *isInput = m_pPropGrid->GetProperty(0)->GetSubItem(2);

		int indexInput = isInput->GetSelectedOption();
		WoormField::RepFieldType optionInput =(WoormField::RepFieldType) isInput->GetOptionData(indexInput);
		field->SetFieldType(optionInput);

		if (field->IsHidden() && field->IsInput())
		{
			int arrIndex = isArray->GetSelectedOption();
			BOOL option = (BOOL)isArray->GetOptionData(arrIndex);
			field->SetDataType(field->GetDataType(), option);
		}
	}

	m_bNeedsApply = FALSE;

	ASSERT_VALID(field);
	if (!field->IsHidden())
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, field);
	}

	else
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Variables, field);
	}

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewHyperLink()
{
	// add name, link type, flag by variable (?)
	m_pPropGrid->RemoveAll();

	SetPanelTitle(_TB("New Link"));

	int internalId = -1;
	BaseObj* obj=GetDocument()->m_pCurrentObj;
	if (GetDocument()->CurrentIsTable())
	{
		TableColumn* pCol = ((Table*)obj)->m_Columns[((Table*)obj)->m_nActiveColumn];
		internalId = pCol->GetInternalID();
	}

	else
		internalId = obj->GetInternalID();

	WoormField* field = GetDocument()->GetEditorSymTable()->GetFieldByID(internalId);
	ASSERT(field);
	if (!field)
	{
		ClearPropertyGrid();
		return;
	}

	CrsProp* nameProp = new CrsProp(_TB("Owner"), (LPCTSTR)field->GetName());
	nameProp->SetData(field->GetId());
	nameProp->SetDescription(_TB("The field/column that owns the hyperlink"));
	nameProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(nameProp);

	CRSCommonProp* newLinkType = new CRSCommonProp(_TB("Link type"), L"", CRSCommonProp::NEW_HYPERLINK, this);
	newLinkType->AllowEdit(FALSE);
	newLinkType->SetColoredState(CrsProp::State::Mandatory);
	newLinkType->AddOption(_TB("Link to Form"), TRUE, WoormLink::WoormLinkType::ConnectionForm);
	newLinkType->AddOption(_TB("Link to Function"), TRUE, WoormLink::WoormLinkType::ConnectionFunction);
	newLinkType->AddOption(_TB("Link to Report"), TRUE, WoormLink::WoormLinkType::ConnectionReport);
	newLinkType->AddOption(_TB("Link to URL"), TRUE, WoormLink::WoormLinkType::ConnectionURL);
	newLinkType->SelectOption(/*2*/newLinkType->GetOptionDataIndex(WoormLink::WoormLinkType::ConnectionReport));
	m_pPropGrid->AddProperty(newLinkType);

	CRSCommonProp* hFlag = new  CRSCommonProp(_TB("Link object by variable"), L"", CRSCommonProp::PropType::NEW_HYPERLINK_VAR_FLAG, this);
	hFlag->AllowEdit(FALSE);
	hFlag->SetDescription(_TB("The link target contains a variable value"));
	hFlag->AddOption(L"False", TRUE, 0);
	hFlag->AddOption(L"True", TRUE, 1);
	hFlag->SelectOption(hFlag->GetOptionDataIndex(0));
	m_pPropGrid->AddProperty(hFlag, TRUE, TRUE);

	NewReportLink();

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
//void CRS_ObjectPropertyView::UpdateReportTree()
//{
//	ASSERT_VALID(this->m_pTreeNode);
//
//	if (GetDocument()->GetWoormFrame()->m_pReportTreeView)
//	{
//		ASSERT_VALID(GetDocument()->GetWoormFrame()->m_pReportTreeView);
//		GetDocument()->GetWoormFrame()->m_pReportTreeView->UpdateRSTreeNode(m_pTreeNode);
//	}

//	//ASSERT(FALSE);
//}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::InitCRS_PropertyGrid()
{
	CRect rect;
	CWnd* pPlaceHolder = this->GetDlgItem(IDC_RS_PropertyGrid);
	if (pPlaceHolder)
	{
		pPlaceHolder->GetWindowRect(&rect);
		pPlaceHolder->UnsubclassWindow();
		pPlaceHolder->Detach();
		pPlaceHolder->DestroyWindow();
	}	

	else
		GetWindowRect(&rect);

	HWND wnd = m_hWnd;
	
	ScreenToClient(&rect);

	if (!m_pPropGrid->Create(WS_CHILD | WS_VISIBLE | BS_DEFPUSHBUTTON | WS_BORDER, rect, this, IDC_RS_PropertyGrid))
	{
		ASSERT(FALSE);
		delete m_pPropGrid;
		return;
	}

	// Obbligatorio per impedire di far sparire la toolbar (search, order..) quando viene effettuata la ReLayout. 
	// Se non dovesse bastare, copiare OnEraseBkgnd(CDC* pDC) da TBPropertyGrid.cpp e inserirlo nella message map
	m_pPropGrid->ModifyStyle(WS_CLIPCHILDREN, 0);

	m_pPropGrid->SetCommandsVisible(TRUE);
	m_pPropGrid->EnableSearchBox(TRUE, _TB("Search"));
	m_pPropGrid->EnableToolBar();		  // abilita la toolbar per ordine alfabetico delle properties oppure categorizzato a tree
	m_pPropGrid->EnableHeaderCtrl(FALSE); // Property e Value in testa alla property Grid (eliminati perch� mi sembrano inutili)
	m_pPropGrid->EnableDescriptionArea(); // area sottostante la property grid per la descrizione delle properties
	m_pPropGrid->SetVSDotNetLook();
	m_pPropGrid->MarkModifiedProperties(TRUE);
	m_pPropGrid->SetNameAlign(DT_LEFT);
	m_pPropGrid->SetFont(AfxGetThemeManager()->GetFormFont());
	m_pPropGrid->InitSizeInfo(m_pPropGrid);

	// Obbligatorio per impedire di far sparire la toolbar (search, order..) quando viene effettuata la ReLayout. 
	// Se non dovesse bastare, copiare OnEraseBkgnd(CDC* pDC) da TBPropertyGrid.cpp e inserirlo nella message map
	//aux->ModifyStyle(WS_CLIPCHILDREN, 0);

	//aux->SetCommandsVisible(TRUE);
	//aux->EnableSearchBox(TRUE, _TB("Search"));
	//aux->EnableToolBar();		  // abilita la toolbar per ordine alfabetico delle properties oppure categorizzato a tree
	//aux->EnableHeaderCtrl(FALSE); // Property e Value in testa alla property Grid (eliminati perch� mi sembrano inutili)
	//aux->EnableDescriptionArea(); // area sottostante la property grid per la descrizione delle properties
	//aux->SetVSDotNetLook();
	//aux->MarkModifiedProperties(TRUE);
	//aux->SetNameAlign(DT_LEFT);
	//aux->SetFont(AfxGetThemeManager()->GetFormFont());
	//aux->InitSizeInfo(aux);
}

// ----------------------------------------------------------------------------
BOOL CRS_ObjectPropertyView::ClearPropertyGrid()
{
	if (m_bNeedsApply && !m_bFromDragnDrop)
	{
		if (AfxTBMessageBox(_TB("vuoi abbandonare le modifiche nella property list ?"), MB_ICONWARNING | MB_YESNO) == IDNO)
		{
			GetPropertyGrid()->SetFocusOnSearch();		
			
			return FALSE;
		} 

		GetDocument()->RefreshRSTree(ERefreshEditor::ToolBox);
	}

	m_pTreeNode = NULL;
	m_pMulSel = NULL;
	m_pMulCol = NULL;
	m_pSelectedTableCell = NULL;

	m_bNeedsApply				= FALSE;
	m_bShowLayoutBtn				= FALSE;
	m_bCheckLayoutBtn				= FALSE;
	m_bCheckLayoutBtnChanged		= FALSE;
	m_eShowVariableTypeBtn			= WoormField::SourceFieldType::NONE;
	m_bCheckVariableBtn				= FALSE;
	m_bCheckVariableBtnChanged		= FALSE;
	m_bShowRequestFieldBtn			= FALSE;
	m_bCheckRequestFieldBtn			= FALSE;
	m_bCheckRequestFieldBtnChanged	= FALSE;
	m_bShowFindRuleBtn				= FALSE;

	m_NewName.Empty();
	m_NewType = DataType::String;

	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();
	m_pPropGrid->ClearSearchBox();
	m_pPropGrid->ClearCommands();
	m_pPropGrid->AdjustLayout();
	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NET_NONE;

	SetPanelTitle(_TB("None object selected"));

	return TRUE;
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadPropertyGrid(CNodeTree* pNode)
{
	if (!ClearPropertyGrid()) return;

	ASSERT_VALID(pNode);
	m_pTreeNode = pNode;

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_ROOT_LAYOUTS: // pnode non valorizzato
		{
			LoadLayoutsPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_PARAMETERS:
		{
			LoadWoormParametersPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_PAGE:
		{
			LoadPagePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_PROPERTIES:
		{
			LoadPropertiesPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_SETTINGS:
		{
			LoadRSSettingsPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_PROCEDURE:
		{
			LoadProcedurePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_NAMED_QUERY:
		{
			LoadNamedQueryPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_LINK:
		{
			LoadLinkPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_LINK_PARAM:
		{
			LoadLinkParamPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			LoadVariablePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_ASKDIALOG:
		{
			LoadAskDialogPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_ASKGROUP:
		{
			LoadAskGroupPropertyGrid(pNode);
			break;
		} 
		
		case CNodeTree::ENodeType::NT_ASKFIELD:
		{
			LoadAskFieldPropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_TEXTRECT:
		case CNodeTree::ENodeType::NT_OBJ_FIELDRECT:
		case CNodeTree::ENodeType::NT_OBJ_FILERECT:
		case CNodeTree::ENodeType::NT_OBJ_GRAPHRECT:
		case CNodeTree::ENodeType::NT_OBJ_SQRRECT:
		case CNodeTree::ENodeType::NT_OBJ_TABLE:
		case CNodeTree::ENodeType::NT_OBJ_COLUMN:
		case CNodeTree::ENodeType::NT_OBJ_REPEATER:
		case CNodeTree::ENodeType::NT_OBJ_TOTAL:
		case CNodeTree::ENodeType::NT_OBJ_CHART:
		{
			LoadLayoutObjectPropertyGrid(pNode);
			break;
		}
		case CNodeTree::ENodeType::NT_OBJ_CATEGORY:
		{
			LoadChartCategoryPropertyGrid(pNode);
			break;
		}
		case CNodeTree::ENodeType::NT_OBJ_SERIES:
		{
			LoadChartSeriesPropertyGrid(pNode);
			break;
		}
		case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
		{
			NewBreakingEvent(pNode, FALSE);
			break;
		}

		case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
		case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:
		{
			//NewBreakingEvent(pNode, FALSE);
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
		{
			LoadNamedQueryRulePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
		{
			LoadTableRulePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO:
		{
			LoadJoinTablePropertyGrid(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_LAYOUT:
		{
			LoadSingleLayoutPropertyGrid(pNode);
			break;
		}

		//case CNodeTree::ENodeType::NT_RULE_QUERY_COLUMN:
		//{
		//	LoadColumnPropertyGrid(pNode);
		//	break;
		//}

		//case CNodeTree::ENodeType::NT_RULE_QUERY_CALC_COLUMN:
		//{
		//	LoadCalcColumnPropertyGrid(pNode);
		//	break;
		//}
	}

	GetDocument()->GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::ReLoadPropertyGrid()
{
	if (m_pMulSel)
	{
		LoadMultipleSelectionProperties(m_pMulSel);
	}
	else if (m_pTreeNode)
	{
		ASSERT_VALID(m_pTreeNode);
		LoadPropertyGrid(m_pTreeNode);
	}
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewObjectPropertyGrid(CNodeTree* pTreeNode)
{
	ASSERT_VALID(m_pPropGrid);
	if (!ClearPropertyGrid()) return;
	m_bNeedsApply = TRUE;

	ASSERT_VALID(pTreeNode);
	m_pTreeNode = pTreeNode;

	switch (pTreeNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
			NewLayoutPropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
			NewProcedurePropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;
		case CNodeTree::ENodeType::NT_ROOT_QUERIES:
			NewNamedQueryPropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_ASKGROUP:
			NewAskFieldPropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM:
			NewJoinTablePropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
			NewCalcColumnPropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
			NewColumnsPropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;

		case CNodeTree::ENodeType::NT_ROOT_RULES:
		{
			m_bNeedsApply = FALSE;
			NewRulesPropertyGrid(pTreeNode);
			break;
		}
			
		case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
		{
			m_bNeedsApply = FALSE;
			NewVariablePropertyGrid(pTreeNode);
			SetFocusOnFirstProperty();
			break;
		}

		case CNodeTree::ENodeType::NT_LINK_PARAMETERS:
		{
			AddLinkParameters(pTreeNode);
			//SetFocusOnFirstProperty();
			break;
		}

		default:
			ASSERT(FALSE);
	}

	GetDocument()->GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);
}

//-----------------------------------------------------------------------------
// prop grid for new expression or function column or field 
void CRS_ObjectPropertyView::CreateNewElement()
{
	int index = m_pPropGrid->GetProperty(0)->GetSubItem(0)->GetSelectedOption();
	if (index < 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::Mandatory);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::Normal);

	DWORD_PTR option = m_pPropGrid->GetProperty(0)->GetSubItem(0)->GetOptionData(index);

	BOOL bEmptyAllowed = option == 2;
	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(1))->CheckPropValue(bEmptyAllowed, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}		
	else
	{
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(1))->SetColoredState(CrsProp::Normal);
	}

	WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();

	if (m_NewType == DataType::Null || m_NewType == DataType::Enum /*generic enum type*/)
	{
		AfxMessageBox(_TB("Field name has wrong data type"));
		return;
	}

	Table* currentTable = NULL;
	GetDocument()->m_wCreatingColumnIds.RemoveAll();
	if (GetDocument()->CurrentIsTable() && !m_bCreatingNewTable) {

		ASSERT_VALID(GetDocument()->m_pCurrentObj);
		ASSERT_KINDOF(Table, GetDocument()->m_pCurrentObj);

		currentTable = (Table*)GetDocument()->m_pCurrentObj;
	}

	WoormField* pRepField = NULL;

	if (option == 1)	//Expression
	{
		pRepField = new WoormField(m_NewName, m_fieldType);
			pRepField->SetDataType(m_NewType);
			pRepField->SetHidden(m_bIsHidden);
			pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(m_NewType, &GetDocument()->GetNamespace()));

		if (currentTable && !m_bIsHidden)
		{
			ASSERT(!m_bIsArray);

			pRepField->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);
			pRepField->SetDispTable(currentTable->GetName());
		}

		else 
			pRepField->SetFieldType(WoormField::RepFieldType::FIELD_NORMAL);

		pRepField->SetExprRuleField(TRUE);

		ExpRuleData* pExpRuleData = new ExpRuleData(*pSymTable);
		pExpRuleData->SetPublicName(m_NewName);
		pExpRuleData->m_ThenExpr.AssignStr(m_NewType.FormatDefaultValue());

		pSymTable->Add(pRepField);

		GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData()->Add(pExpRuleData);

		GetDocument()->SyncronizeViewSymbolTable(pRepField);
	} 

	else if (option == 0) 	//Function
	{
		pRepField = new WoormField(m_NewName, m_fieldType);
			pRepField->SetDataType(m_NewType, m_bIsArray);
			pRepField->SetHidden(m_bIsHidden);
			pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(m_NewType, &GetDocument()->GetNamespace()));
			pRepField->m_pEventFunction = new EventFunction(pSymTable, *pRepField);

		if (currentTable && !m_bIsHidden)
		{
			ASSERT(!m_bIsArray);

			pRepField->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);
			pRepField->SetDispTable(currentTable->GetName());
		}

		else if (m_fieldType!= WoormField::RepFieldType::FIELD_INPUT)
			pRepField->SetFieldType(WoormField::RepFieldType::FIELD_NORMAL);

		pSymTable->Add(pRepField);
		GetDocument()->SyncronizeViewSymbolTable(pRepField);
	}

	else if (option == 2)	//From hidden field
	{
		for (int i = 0;i < m_pPropGrid->GetProperty(0)->GetSubItem(2)->GetSubItemsCount();i++) {

			CRSCheckBoxProp* prop =(CRSCheckBoxProp*) m_pPropGrid->GetProperty(0)->GetSubItem(2)->GetSubItem(i);
			BOOL checked = prop->GetValue();
			if (!checked)
				continue;

			pRepField = pSymTable->GetField(m_pPropGrid->GetProperty(0)->GetSubItem(2)->GetSubItem(i)->GetName());
			ASSERT_VALID(pRepField);
			pRepField->SetHidden(FALSE);
			pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(pRepField->GetDataType(), &GetDocument()->GetNamespace()));

			if (currentTable)
			{
				pRepField->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);
				pRepField->SetDispTable(currentTable->GetName());				
			}

			GetDocument()->m_wCreatingColumnIds.Add(pRepField->GetId());
		}
	}

	if (!pRepField->IsHidden()) 
	{
		if (currentTable)
		{
			if (option == 2)
				currentTable->AddColumn(GetDocument()->m_wCreatingColumnIds);
			else
			{
				CWordArray ids; ids.Add(pRepField->GetId());
				currentTable->AddColumn(ids);
			}

		}

		else if (m_bCreatingNewTable)
		{
			if (option!=2)
				GetDocument()->m_wCreatingColumnIds.Add(pRepField->GetId());
			
			if (m_bFromDragnDrop)
			{
				GetDocument()->m_pEditorManager->AddTableDataField(GetDocument()->m_wCreatingId, GetDocument()->m_wCreatingColumnIds, TRUE);
				GetDocument()->m_Creating =  CWoormDocMng::TABLE;
				CClientDC dc(this);
				GetDocument()->CreateListObj(dc, GetDocument()->GetWoormView(), TRUE);
				GetDocument()->m_Creating = CWoormDocMng::NONE;
			}

			else
				GetDocument()->OnAddTable();
		}

		else
		{
			GetDocument()->m_wCreatingId = pRepField->GetId();
			GetDocument()->m_Creating = CWoormDocMng::ObjectType::FIELDRECT;
			if (m_bFromDragnDrop)
			{
				CClientDC dc(this);
				GetDocument()->CreateListObj(dc, GetDocument()->GetWoormView(), TRUE);
				GetDocument()->m_Creating = CWoormDocMng::NONE;
			}

			else
				GetDocument()->ClipCursorToActiveView();
		}
	}

	//----------------------
	m_bNeedsApply = FALSE;
	m_bCreatingNewTable = FALSE;
	m_bFromDragnDrop = FALSE;
	m_bIsArray = FALSE;
	ClearPropertyGrid();

	ASSERT_VALID(pRepField);
	if (pRepField->IsExprRuleField() && option == 1)
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pRepField);
	}

	if  (pRepField->IsHidden())
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);
		GetDocument()->RefreshRSTree(ERefreshEditor::Variables, pRepField);
	}

	else
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pRepField);
	}

	//GetDocument()->SetModifiedFlag();
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadPagePropertyGrid(CNodeTree* pNode){

	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	PageInfo* pgInfo = dynamic_cast<PageInfo*>(pNode->m_pItemData);
	ASSERT_VALID(pgInfo);

	SetPanelTitle(_TB("Page Info"));

	LoadPageInfoSettings(pgInfo);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadPageInfoSettings(PageInfo* pgInfo)
{
	PrinterInfo* printerInfo;
	if (pgInfo->GetPreferredPrinter().IsEmpty())
		printerInfo = new PrinterInfo();
	else
		printerInfo = new PrinterInfo(pgInfo->GetPreferredPrinter());

	//Preferred printer
	CString printer = printerInfo->m_strPrinterName;
	CrsProp* printerGroup = new CrsProp(_TB("Preferred Printer"));
	CRSPageProp* prefPrinter = new CRSPageProp(pgInfo, _TB("Printer"), (LPCTSTR)(printer.IsEmpty() ? _TB("Default printer") : printer), CRSPageProp::PageInfoType::PRINTER, this);
	CStringArray printerArray;
	GetPrinters(printerArray);
	for (int i = 0; i < printerArray.GetSize(); i++)
		prefPrinter->AddOption(printerArray[i]);
	prefPrinter->AllowEdit(FALSE);
	printerGroup->AddSubItem(prefPrinter);
	
	//Preffered printer flags
	Options* printOptions = GetDocument()->m_pOptions;
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Without Background"), &printOptions->m_bDefaultPrnNoBitmap.m_bValue, this));
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Without Border"), &printOptions->m_bDefaultPrnNoBorders.m_bValue, this));
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Show Description"), &printOptions->m_bDefaultPrnShowLabels.m_bValue, this));
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Show Titles"), &printOptions->m_bDefaultPrnShowTitles.m_bValue, this));
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Only Fonts for Quick Print"), &printOptions->m_bDefaultPrnUseDraftFont.m_bValue, this));
	printerGroup->AddSubItem(new CRSPageBoolProp(pgInfo, _TB("Print on Letterhead"), &(GetDocument()->m_bPrintOnLetterhead.m_bValue), this));
	m_pPropGrid->AddProperty(printerGroup);

	//WOORM PAGE SIZE BLOCk & PRINTER PAGE SIZE BLOCK
	CrsProp* wrmPageSize = new CrsProp(_TB("Report page size"));
	CrsProp* printerPageSize = new CrsProp(_TB("Printer page size"));
	
	PrinterInfoItem* wrmInfoItem = GetDefaultPaperSize(printerInfo, pgInfo->dmPaperWidth, pgInfo->dmPaperLength);
	PrinterInfoItem* printerInfoItem = GetDefaultPaperSize(printerInfo, pgInfo->m_PrinterPageInfo.dmPaperWidth, pgInfo->m_PrinterPageInfo.dmPaperLength);
	
	wrmStyle = new CRSPageProp(pgInfo, _TB("Styles"), (LPCTSTR)wrmInfoItem == NULL ? L"" : wrmInfoItem->m_strPaperSize, CRSPageProp::PageInfoType::REPORT_STYLES, this);
	wrmStyle->AllowEdit(FALSE);
	wrmStyle->AddOption(L"");

	printerStyle = new CRSPageProp(pgInfo, _TB("Styles"), (LPCTSTR)printerInfoItem == NULL ? L"" : printerInfoItem->m_strPaperSize, CRSPageProp::PageInfoType::PRINTER_STYLES, this);
	printerStyle->AllowEdit(FALSE);	
	printerStyle->AddOption(L"");

	for (int i = 0; i < printerInfo->m_PrinterPaperInfo.GetCount(); i++)
	{
		wrmStyle->AddOption(((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_strPaperSize, TRUE, ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_wPaperType);
		printerStyle->AddOption(((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_strPaperSize, TRUE, ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_wPaperType);
	}

	wrmPageSize->AddSubItem(wrmStyle);
	printerPageSize->AddSubItem(printerStyle);

	//Woorm page width
	CString width; width.Format(L"%d", pgInfo->dmPaperWidth / 10);
	wrmWidth = new CRSPageProp(pgInfo, _TB("Width (mm)"), (LPCTSTR)width, CRSPageProp::PageInfoType::REPORT_WIDTH, this);
	wrmPageSize->AddSubItem(wrmWidth);
	//Printer page width
	width.Format(L"%d", pgInfo->m_PrinterPageInfo.dmPaperWidth/ 10);
	printerWidth = new CRSPageProp(pgInfo, _TB("Width (mm)"), (LPCTSTR)width, CRSPageProp::PageInfoType::PRINTER_WIDTH, this);
	printerPageSize->AddSubItem(printerWidth);

	//Woorm page length
	CString length; length.Format(L"%d", pgInfo->dmPaperLength / 10);
	wrmLength = new CRSPageProp(pgInfo, _TB("Length (mm)"), length, CRSPageProp::PageInfoType::REPORT_LENGTH, this);
	wrmPageSize->AddSubItem(wrmLength);
	//Printer page length
	length.Format(L"%d", pgInfo->m_PrinterPageInfo.dmPaperLength / 10);
	printerLength = new CRSPageProp(pgInfo, _TB("Length (mm)"), length, CRSPageProp::PageInfoType::PRINTER_LENGTH, this);
	printerPageSize->AddSubItem(printerLength);

	m_pPropGrid->AddProperty(wrmPageSize);
	m_pPropGrid->AddProperty(printerPageSize);

	//ORIENTATION
	CString orientation;
	if (pgInfo->dmOrientation == DMORIENT_PORTRAIT)
		orientation = _TB("Portrait");
	else
		orientation = _TB("Landscape");

	CRSPageProp* orientProp = new CRSPageProp(pgInfo,_TB("Orientation"), (LPCTSTR)orientation,CRSPageProp::PageInfoType::PAGE_ORIENT,this);
	orientProp->AllowEdit(FALSE);
	orientProp->AddOption(_TB("Portrait"), TRUE, DMORIENT_PORTRAIT);
	orientProp->AddOption(_TB("Landscape"), TRUE, DMORIENT_LANDSCAPE);
	printerGroup->AddSubItem(orientProp);

	//Number of copies
	CString copies; copies.Format(L"%d", pgInfo->GetCopies());
	CRSPageProp* copiesNum = new CRSPageProp(pgInfo, _TB("Number of Copies"), (LPCTSTR)copies, CRSPageProp::PageInfoType::NUMBER_COPIES, this);
	printerGroup->AddSubItem(copiesNum);

	//Collate copies
	CRSPageProp* collateCopies = new CRSPageProp(pgInfo, _TB("Collate copies"), (LPCTSTR)MakeStringFromBool(pgInfo->dmCollate), CRSPageProp::PageInfoType::COLLATE_COPIES, this);
	collateCopies->AllowEdit(FALSE);
	collateCopies->AddOption(L"True");
	collateCopies->AddOption(L"False");
	printerGroup->AddSubItem(collateCopies);

	//Use printable area
	CRSPageProp* printableArea = new CRSPageProp(pgInfo, _TB("Use printable area"), (LPCTSTR)MakeStringFromBool(pgInfo->m_bUsePrintableArea), CRSPageProp::PageInfoType::PRINTABLE_AREA, this);
	printableArea->AllowEdit(FALSE);
	printableArea->AddOption(L"True");
	printableArea->AddOption(L"False");
	printerGroup->AddSubItem(printableArea);

	//MARGINS
	if (pgInfo->m_bUsePrintableArea)
		pgInfo->CalculateMargins();

	CrsProp* margins = new CrsProp(_TB("Margins (mm)"));

	//LEFT	
	CString margin; margin.Format(L"%.2lf", abs(LPtoMU((pgInfo->m_rectMargins.left), CM, 10., 3)));
	marginLeft = new CRSPageProp(pgInfo, _TB("Left"), (LPCTSTR)margin, CRSPageProp::PageInfoType::MARGIN_LEFT, this);
	margins->AddSubItem(marginLeft);
			//RIGHT
	margin.Format(L"%.2lf", abs(LPtoMU((pgInfo->m_rectMargins.right), CM, 10., 3)));
	marginRight = new CRSPageProp(pgInfo, _TB("Right"), (LPCTSTR)margin, CRSPageProp::PageInfoType::MARGIN_RIGHT, this);
	margins->AddSubItem(marginRight);
			//TOP
	margin.Format(L"%.2lf", abs(LPtoMU((pgInfo->m_rectMargins.top), CM, 10., 3)));
	marginTop = new CRSPageProp(pgInfo, _TB("Top"), (LPCTSTR)margin, CRSPageProp::PageInfoType::MARGIN_TOP, this);
	margins->AddSubItem(marginTop);
			//BOTTOM
	margin.Format(L"%.2lf", abs(LPtoMU((pgInfo->m_rectMargins.bottom), CM, 10., 3)));
	marginBottom = new CRSPageProp(pgInfo, _TB("Bottom"), (LPCTSTR)margin, CRSPageProp::PageInfoType::MARGIN_BOTTOM, this);
	margins->AddSubItem(marginBottom);

	if (pgInfo->m_bUsePrintableArea == TRUE)
	{
		marginLeft->AllowEdit(FALSE);
		marginRight->AllowEdit(FALSE);
		marginTop->AllowEdit(FALSE);
		marginBottom->AllowEdit(FALSE);
	}

	m_pPropGrid->AddProperty(margins);

	//Show on printer flags
	CrsProp* showOnPrinter = new CrsProp(_TB("Show on Printer"));
	showOnPrinter->AddSubItem(new CRSBoolProp(pgInfo, _TB("Without Background"), &printOptions->m_bPrnNoBitmap, L""));
	showOnPrinter->AddSubItem(new CRSBoolProp(pgInfo, _TB("Without Border"), &printOptions->m_bPrnNoBorders, L""));
	showOnPrinter->AddSubItem(new CRSBoolProp(pgInfo, _TB("Show Description"), &printOptions->m_bPrnShowLabels, L""));
	showOnPrinter->AddSubItem(new CRSBoolProp(pgInfo, _TB("Show Titles"), &printOptions->m_bPrnShowTitles, L""));
	showOnPrinter->AddSubItem(new CRSBoolProp(pgInfo, _TB("Only Fonts for Quick Print"), &printOptions->m_bPrnUseDraftFont, L""));	
	showOnPrinter->Expand(FALSE);
	m_pPropGrid->AddProperty(showOnPrinter);

	////Show on screen flags
	CrsProp* showOnScreen = new CrsProp(_TB("Show on Screen"));
	showOnScreen->AddSubItem(new CRSBoolProp(pgInfo, _TB("Without Background"), &printOptions->m_bConNoBitmap, L""));
	showOnScreen->AddSubItem(new CRSBoolProp(pgInfo, _TB("Without Border"), &printOptions->m_bConNoBorders, L""));
	showOnScreen->AddSubItem(new CRSBoolProp(pgInfo, _TB("Show Description"), &printOptions->m_bConShowLabels, L""));
	showOnScreen->AddSubItem(new CRSBoolProp(pgInfo, _TB("Show Titles"), &printOptions->m_bConShowTitles, L""));
	showOnScreen->Expand(FALSE);
	m_pPropGrid->AddProperty(showOnScreen);
}

// ----------------------------------------------------------------------------
PrinterInfoItem* CRS_ObjectPropertyView::GetDefaultPaperSize(PrinterInfo* printerInfo, int paperWidth, int paperLength){

	for (int i = 0; i < printerInfo->m_PrinterPaperInfo.GetCount(); i++)
	{
		if (paperWidth == ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_sPaperSize.cy && paperLength == ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_sPaperSize.cx)
			return (PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]);
	}

	return NULL;
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadRSSettingsPropertyGrid(CNodeTree* pNode){
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	WoormIni* wrmIni = dynamic_cast<WoormIni*>(pNode->m_pItemData);
	ASSERT(wrmIni);
	SetPanelTitle(_TB("Reporting Studio Settings"));
	LoadRSSettingsInfo(wrmIni);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadRSSettingsInfo(WoormIni* wrmIni)
{
	CrsProp* general = new CrsProp(_TB("General"));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Sort items by position"), &wrmIni->m_bSortObjects.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Align single data at the bottom"), &wrmIni->m_bBottomAlign.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Save schemes creating .BAK"), &wrmIni->m_bMakeBackupFile.m_bValue, L""));
	//SEMPRE VERO general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Save deleted fields"), &wrmIni->m_bAlwaysHidden.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Add before the column"), &wrmIni->m_bAddColumnBefore.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Column attributes also on total"), &wrmIni->m_bIncludeTotal.m_bValue, L""));
	
	// Auto save time
	general->AddSubItem(new CRSIntProp(wrmIni, _TB("Time auto save"), &wrmIni->m_iTimeAutoSave, _TB("Auto Save timer in minutes"), 0, 60, CRSIntProp::CooType::DEFAULT));
	
	//Grid group
	CrsProp* GridGroup = new CrsProp(_TB("Background Grid"));
		//enable
		GridGroup->AddSubItem(new CRSBoolPropWithLayoutRefresh(wrmIni, _TB("Show grid"), &wrmIni->m_bShowGrid.m_bValue, L""));
		//line/point
		GridGroup->AddSubItem(new CrsGridStyleProp(wrmIni));
		//color
		GridGroup->AddSubItem(new CRSColorProp(wrmIni, _TB("Color"), &wrmIni->m_rgb_GridColor, L""));
		//x
		CString pitchx; pitchx.Format(L"%.1lf", LPtoMU((wrmIni->m_nGridX), CM, 10., 3));
		GridGroup->AddSubItem(new CRSIniProp(wrmIni, _TB("Grid pitch X (mm)"), (LPCTSTR)pitchx, CRSIniProp::IniLineType::PITCH_X));
		//y
		CString pitchy; pitchy.Format(L"%.1lf", LPtoMU((wrmIni->m_nGridY), CM, 10., 3));
		GridGroup->AddSubItem(new CRSIniProp(wrmIni, _TB("Grid pitch Y (mm)"), (LPCTSTR)pitchy, CRSIniProp::IniLineType::PITCH_Y));
		//alignment
		GridGroup->AddSubItem(new CRSBoolProp(wrmIni, _TB("Align to grid"), &wrmIni->m_bSnapToGrid.m_bValue, L""));
	//adding
	general->AddSubItem(GridGroup);

	//tracking draw group
	CrsProp* TrackingGroup= new CrsProp(_TB("Tracking Line"));
		//enabled
		TrackingGroup->AddSubItem(new CRSBoolProp(wrmIni, _TB("Enable"), &wrmIni->m_bEnableTrackCross.m_bValue, _TB("Deciding whether to show the alignment guides while moving an object")));
		//color
		TrackingGroup->AddSubItem(new CRSColorProp(wrmIni, _TB("Color"), &wrmIni->m_rgb_TrackLineColor, _TB("The color of alignment guides while moving an object")));
		//size
		TrackingGroup->AddSubItem(new CRSIntProp(wrmIni, _TB("Size"), &wrmIni->m_nTrackLineSize, _TB("The size of alignment guides while moving an object"), 1, 10, CRSIntProp::CooType::DEFAULT));
		//pen
		TrackingGroup->AddSubItem(new CRSLineStyleProp(wrmIni, _TB("Line Style"), &wrmIni->m_eTrackLineStyle, this, _TB("The style of alignment guides while moving an object")));
	//adding
	general->AddSubItem(TrackingGroup);

	//Object Selection Group
	CrsProp* SelectionGroup = new CrsProp(_TB("Object Selection Line"));
		//inside/outside
		SelectionGroup->AddSubItem(new CRSBoolPropWithLayoutRefresh(wrmIni, _TB("Show selection inside"), &wrmIni->m_bTrackInside.m_bValue, _TB("Decides whether to draw the selection of an object internally or externally")));
		//theme
		SelectionGroup->AddSubItem(new CRSObjectSelectionStyleProp(wrmIni, this));
	//adding
	general->AddSubItem(SelectionGroup);

	//tracking draw group
	CrsProp* HiddenBorderGroup = new CrsProp(_TB("Hidden Border"));
		//color
		HiddenBorderGroup->AddSubItem(new CRSColorProp(wrmIni, _TB("Color"), &wrmIni->m_rgb_HiddenBorderColor, _TB("The color of the borders drawn in editing for objects without user defined borders")));
		//pen
		HiddenBorderGroup->AddSubItem(new CRSLineStyleProp(wrmIni, _TB("Line Style"), &wrmIni->m_eHiddenBorderStyle, this, _TB("The style of the borders drawn in editing for objects without user defined borders")));
	//adding
	general->AddSubItem(HiddenBorderGroup);

	//unit
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Show sizes in unit"), &wrmIni->m_bSizeInGridUnits.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Show printing margins"), &wrmIni->m_bShowMargins.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Show printable area"), &wrmIni->m_bShowPrintableArea.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Create transparents"), &wrmIni->m_bTransparentCreate.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Create without borders"), &wrmIni->m_bNoBorderCreate.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Create without description"), &wrmIni->m_bNoLabelCreate.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Enable optimized line break"), &wrmIni->m_bOptimizedLineBreak.m_bValue, L""));

	CString percentrage; percentrage.Format(L"%.lf", wrmIni->m_ColumnWidthPercentage.m_nValue);
	general->AddSubItem(new CRSIniProp(wrmIni, _TB("Percentage of used column width (%)"), (LPCTSTR)percentrage, CRSIniProp::IniLineType::PERCENTAGE));

	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Check barcode size"), &wrmIni->m_bCheckBarcodeSize.m_bValue, L""));
	general->AddSubItem(new CRSBoolProp(wrmIni, _TB("Force vertical alignment label relative"), &wrmIni->m_bForceVerticalAlignLabelRelative.m_bValue, L""));

	CRSBoolProp* bp = new CRSBoolProp(wrmIni, _TB("Unifies report structure's trees"), &wrmIni->m_bShowReportTree.m_bValue, L"");
	bp->SetDescription(_TB("It joins Layout and Engine Tree panels"));
	general->AddSubItem(bp);

	CRSBoolProp* bt = new CRSBoolProp(wrmIni, _TB("Show All Toolbars"), &wrmIni->m_bShowAllToolbars.m_bValue, L"");
	bt->SetDescription(_TB("It enables old Object, Alignment and Borders toolbars"));
	general->AddSubItem(bt);

	CString tolerance; tolerance.Format(L"%.2lf", abs(LPtoMU((wrmIni->m_nSortGap), CM, 10., 3)));
	general->AddSubItem(new CRSIniProp(wrmIni, _TB("Sorting tolerance (mm)"), (LPCTSTR)tolerance, CRSIniProp::IniLineType::TOLERANCE));
	general->AddSubItem(new CRSShortProp(wrmIni, _TB("Mouse sensibility"), &wrmIni->m_nMouseSensibility.m_nValue, L""));
	general->AddSubItem(new CRSShortProp(wrmIni, _TB("Table lines"), &wrmIni->m_nTableRows.m_nValue, L""));

	//Show on field value
	CRSIniProp* showOnField = new CRSIniProp(wrmIni, _TB("Show on field value"), GetWoormIniType(wrmIni->m_Show), CRSIniProp::IniLineType::SHOW_ON_FIELD);
	showOnField->AllowEdit(FALSE);
	showOnField->AddOption(GetWoormIniType(WoormIni::ShowType::ID), TRUE, WoormIni::ShowType::ID);
	showOnField->AddOption(GetWoormIniType(WoormIni::ShowType::NAME), TRUE, WoormIni::ShowType::NAME);
	showOnField->AddOption(GetWoormIniType(WoormIni::ShowType::VALUE), TRUE, WoormIni::ShowType::VALUE);
	general->AddSubItem(showOnField);

	m_pPropGrid->AddProperty(general);

	//Column total
	CrsProp* columnTotal = new CrsProp(_TB("Column total"));
	columnTotal->AddSubItem(new CRSBoolProp(wrmIni, _TB("Atomatic total"), &wrmIni->m_bAutoTotal.m_bValue, L""));
	columnTotal->AddSubItem(new CRSBoolProp(wrmIni, _TB("Total on new page"), &wrmIni->m_bTotalOnNewPage.m_bValue, L""));
	columnTotal->AddSubItem(new CRSBoolProp(wrmIni, _TB("Reset on new page"), &wrmIni->m_bResetOnNewPage.m_bValue, L""));
	m_pPropGrid->AddProperty(columnTotal);
	
	//Eccentricity
	CrsProp* ecc = new CrsProp(_TB("Eccentricity"));
	CString land; land.Format(L"%.2lf", abs(LPtoMU((wrmIni->m_nHorzRatio.m_nValue), CM, 10., 3)));
	ecc->AddSubItem(new CRSIniProp(wrmIni, _TB("Landscape"), (LPCTSTR)land, CRSIniProp::IniLineType::LANDSCAPE));
	land.Format(L"%.2lf", abs(LPtoMU((wrmIni->m_nVertRatio.m_nValue), CM, 10., 3)));
	ecc->AddSubItem(new CRSIniProp(wrmIni, _TB("Portrait"), (LPCTSTR)land, CRSIniProp::IniLineType::PORTRAIT));
	m_pPropGrid->AddProperty(ecc);
}

// ----------------------------------------------------------------------------
void  CRS_ObjectPropertyView::LoadWoormParametersPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	//CDocProperties* properties = GetDocument()->m_pDocProperties;
	CWoormInfo* info = dynamic_cast<CWoormInfo*>(pNode->m_pItemData);// GetDocument()->m_pDocProperties
	ASSERT(info);
	SetPanelTitle(_TB("Report parameters"));

	LoadWoormParameters(info);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadWoormParameters(CWoormInfo* m_pWoormInfo)
{
	for (int i = 0; i <= m_pWoormInfo->GetParameters().GetUpperBound(); i++)
	{
		
		CDataObjDescription* parName = (CDataObjDescription*)m_pWoormInfo->GetParameters().GetAt(i);
	
		CrsProp* parameterName = new CrsProp(parName->GetName());

		CrsProp* type = new CrsProp(L"Type", (LPCTSTR)parName->ToString(parName->GetDataType()));
		type->AllowEdit(FALSE);
		parameterName->AddSubItem(type);

		CrsProp* value = new CrsProp(L"Value", (LPCTSTR)parName->GetValue()->Str());
		value->AllowEdit(FALSE);
		parameterName->AddSubItem(value);

		CString strDir;
		switch (parName->GetPassedMode())
		{
		case (CDataObjDescription::_IN):
			strDir = _T("In");
			break;
		case (CDataObjDescription::_INOUT):
			strDir = _T("In/Out");
			break;
		case (CDataObjDescription::_OUT):
			strDir = _T("Out");
			break;
		}

		CrsProp* direction = new CrsProp(L"Direction", (LPCTSTR)strDir);
		direction->AllowEdit(FALSE);
		parameterName->AddSubItem(direction);

		CString strBinded;
		if (GetDocument()->GetEditorSymTable())
		{
			SymField* pF = GetDocument()->GetEditorSymTable()->GetField(parName->GetName());

			if (pF)
			{
				strBinded = _TB("True");

				if (!DataType::IsCompatible(parName->GetDataType(), pF->GetDataType()))
					strBinded = _TB("ERROR:it has incompatible data type");
			}

			if (strBinded.IsEmpty())
				strBinded = _TB("False");

			CrsProp* binded = new CrsProp(L"Binded", (LPCTSTR)strBinded);
			binded->AllowEdit(FALSE);
			parameterName->AddSubItem(binded);

			parameterName->Expand(FALSE);
		}

		m_pPropGrid->AddProperty(parameterName, TRUE, TRUE);
	}	
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadPropertiesPropertyGrid(CNodeTree* pNode){

	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	//CDocProperties* properties = GetDocument()->m_pDocProperties;
	CDocProperties* properties = dynamic_cast<CDocProperties*>(pNode->m_pItemData);// GetDocument()->m_pDocProperties
	ASSERT(properties);
	SetPanelTitle(GetName(properties->m_pWoormDoc->m_pEngine->GetReportPath()) + GetExtension(properties->m_pWoormDoc->m_pEngine->GetReportPath()));

	LoadPropertiesInfo(properties, properties->m_pWoormDoc->m_bAllowEditing);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadPropertiesInfo(CDocProperties* properties, BOOL allowEdit)
{
	CString strPathName;

	if (properties->m_pWoormDoc->m_pEngine->GetReportPath().IsEmpty())
		strPathName = properties->m_pWoormDoc->GetFilePath();
	else
		strPathName = properties->m_pWoormDoc->m_pEngine->GetReportPath();

	//GENERAL group
	CrsProp* generalProp= new CrsProp(_TB("General"));

	CrsProp* type = new CrsProp(_TB("Type"),(LPCTSTR)L"Report");
	type->AllowEdit(FALSE);
	generalProp->AddSubItem(type);

	generalProp->AddSubItem(new CRSFileExplorerProp(_TB("Position"), GetPath(strPathName), _TB("Opens file location")));

	CrsProp* nameSpace = new CrsProp(_TB("Namespace"), (LPCTSTR)properties->m_pWoormDoc->GetReportNamespace());
	nameSpace->AllowEdit(FALSE);
	generalProp->AddSubItem(nameSpace);

	CString sz;
	LONG long_sz = GetFileSize((LPCTSTR)strPathName);
	sz.Format(L"%.2lf KB (%d bytes)",abs( (double)long_sz/1000), long_sz);
	CrsProp* size = new CrsProp(_TB("Size"), (LPCTSTR)sz);
	size->AllowEdit(FALSE);
	generalProp->AddSubItem(size);

	CFileStatus	fileStatus;
	CTime		ctime(1980, 1, 1, 0, 0, 0);  //The date and time the file was created.
	CTime		mtime(1980, 1, 1, 0, 0, 0);  //The date and time the file was last modified.
	CTime		atime(1980, 1, 1, 0, 0, 0);  // The date and time the file was last accessed for reading.

	BOOL bStatus = CLineFile::GetStatus(strPathName, fileStatus);

	if (bStatus)
	{
		ctime = fileStatus.m_ctime;
		mtime = fileStatus.m_mtime;
		atime = fileStatus.m_atime;
	} 

	CrsProp* creation = new CrsProp(_TB("Creation date"), (LPCTSTR)ctime.Format("%A, %d/%b/%Y, %H:%M:%S"));
	creation->AllowEdit(FALSE);
	generalProp->AddSubItem(creation);

	CrsProp* changed = new CrsProp(_TB("Last change"), (LPCTSTR)mtime.Format("%A, %d/%b/%Y, %H:%M:%S"));
	changed->AllowEdit(FALSE);
	generalProp->AddSubItem(changed);

	CrsProp* access = new CrsProp(_TB("Last access"), (LPCTSTR)atime.Format("%A, %d/%b/%Y, %H:%M:%S"));
	access->AllowEdit(FALSE);
	generalProp->AddSubItem(access);

	CrsProp* attr = new CrsProp(_TB("Read-only"), (LPCTSTR)MakeStringFromBool(fileStatus.m_attribute == 33));
	attr->AllowEdit(FALSE);
	generalProp->AddSubItem(attr);

	m_pPropGrid->AddProperty(generalProp);

	//summary group
	CrsProp* summaryProp = new CrsProp(_TB("Summary"));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Title"), &properties->m_strTitle,L""));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Subject"), &properties->m_strSubject,L""));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Author"), &properties->m_strAuthor, L""));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Company"), &properties->m_strCompany, L""));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Comments"), &properties->m_strComments, L""));
	summaryProp->AddSubItem(new CRSStringProp(properties, _TB("Default security roles"), &properties->m_strDefaultSecurityRoles, L""));

	if (!allowEdit)
	{ 
		for (int i = 0;i < summaryProp->GetSubItemsCount();i++)
			summaryProp->GetSubItem(i)->AllowEdit(FALSE);
	}

	m_pPropGrid->AddProperty(summaryProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadLinkPropertyGrid(CNodeTree* pNode, BOOL addParams)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	WoormLink* wrmLink = dynamic_cast<WoormLink*>(pNode->m_pItemData);	
	ASSERT_VALID(wrmLink);

	SetPanelTitle(wrmLink->m_strLinkOwner);

	m_bAddNewReportLinkParams = addParams;

	LoadLinkSettings(wrmLink);
}

// ----------------------------------------------------------------------------
//load base link properties into property grid
void CRS_ObjectPropertyView::LoadLinkSettings(WoormLink* wrmLink){

	CrsProp* linkSettings = new CrsProp(_TB("Link management - "+wrmLink->m_strLinkOwner));

	CrsProp* linkType = new CrsProp(_TB("Type"),(LPCTSTR)GetLinkTypeString(wrmLink->m_LinkType));
	linkType->AllowEdit(FALSE);
	linkType->SetOwnerList(m_pPropGrid);											
	linkSettings->AddSubItem(linkType);

	if (wrmLink->m_LinkType == WoormLink::WoormLinkType::ConnectionURL){
		CrsProp* linkSubType = new CrsProp(_TB("Subtype"), (LPCTSTR)GetLinkSubTypeString(wrmLink->m_SubType));
		linkSubType->AllowEdit(FALSE);
		linkSubType->SetOwnerList(m_pPropGrid);
		linkSettings->AddSubItem(linkSubType);
	}

	CrsProp* linkOn = new CrsProp(_TB("On"), (LPCTSTR)wrmLink->m_strLinkOwner);
	linkOn->AllowEdit(FALSE);
	linkSettings->AddSubItem(linkOn);

	//Namespace || variable contains link
	CrsProp* addrOrNamesp = new CrsProp(_TB("Namespace"), (LPCTSTR)wrmLink->m_strTarget);
	if (wrmLink->m_LinkType == WoormLink::WoormLinkType::ConnectionURL || wrmLink->m_LinkType == WoormLink::WoormLinkType::ConnectionReport)
		addrOrNamesp->SetName(_TB("Variable contains link:"));
	addrOrNamesp->AllowEdit(FALSE);
	linkSettings->AddSubItem(addrOrNamesp);

	CrsProp* parameters = new CrsProp(_TB("Link parameters"));

	if (m_bAddNewReportLinkParams)
	{
		InsertReportLinkParameterBlock(wrmLink, wrmLink->m_strTarget, parameters);
	}
	else
	{
		for (int i = 0; i < wrmLink->m_pLocalSymbolTable->GetCount(); i++)
		{
			WoormField* pF = wrmLink->m_pLocalSymbolTable->GetAt(i);
			if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
				continue;

			CRSExpressionProp* expr = new CRSExpressionProp(pF->GetName(), &pF->m_pInitExpression, pF->GetDataType(), pF->GetSymTable(), this);
			parameters->AddSubItem(expr);
		}
	}

	linkSettings->AddSubItem(parameters);

	m_pPropGrid->AddProperty(linkSettings);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadLinkParamPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	m_pPropGrid->RemoveAll();

	WoormField* wrmField;
	try{
		wrmField = dynamic_cast<WoormField*>(pNode->m_pItemData);
	}

	catch (...){
		return;
	}

	if (wrmField)	{
		try{
			SetPanelTitle(wrmField->GetName());
		}
		catch (...){}
	}

	LoadLinkParamSettings(wrmField);
}

//load link param properties into property grid
void CRS_ObjectPropertyView::LoadLinkParamSettings(WoormField* wrmField){

	CrsProp* paramLinkSettings = new CrsProp(_TB("Parameter management - ") + wrmField->GetName());
	
	//Name
	CrsProp* paramName = new CrsProp(_TB("Name"), (LPCTSTR)wrmField->GetName());
	paramName->AllowEdit(FALSE);
	paramLinkSettings->AddSubItem(paramName);

	//Type
	CrsProp* paramType = new CrsProp( _TB("Type"), (LPCTSTR)wrmField->GetDataType().ToString());
	paramType->AllowEdit(FALSE);
	paramLinkSettings->AddSubItem(paramType);

	//Param expression
	CRSExpressionProp* paramExpression = new CRSExpressionProp(_TB("Parameter value"), &wrmField->GetInitExpression(), 
										wrmField->GetDataType(), wrmField->GetSymTable(), this,
										_TB("Parameter will be valorized with the result value of this expression"), FALSE);
	paramLinkSettings->AddSubItem(paramExpression);

	m_pPropGrid->AddProperty(paramLinkSettings);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadLayoutsPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);
		
	// Add Appearence properties----------------------------------------------
	m_pAppearenceGroup = new CrsProp(_TB("Appearence"));

	SetPanelTitle(L"Layout");

	CWoormDocMng* pWrmDoc = GetDocument();
	ASSERT_VALID(pWrmDoc);

	//import static objects
	CRSImportStaticObjects* importStaticObjects = new CRSImportStaticObjects(this);

	//templates
	m_pAppearenceGroup->AddSubItem(new CRSLayoutTemplateProp(pWrmDoc, &GetDocument()->m_Template.m_sNsTemplate, this, importStaticObjects));
	m_pAppearenceGroup->AddSubItem(importStaticObjects);
	//img name
	m_pAppearenceGroup->AddSubItem(new CRSSearchTbDialogProp(pWrmDoc, &(pWrmDoc->m_pOptions->m_strBkgnBitmap), CRSSearchTbDialogProp::PropertyType::GenericImgName, CRSFileNameProp::Filter::Img, this));

	//---Img Location
	CrsProp* pLocationGroup = new CrsProp(_TB("Img Location (px)"), 0, FALSE);
		//location x
		// prop 0
		pLocationGroup->AddSubItem(new CRSImgOriginProp(pWrmDoc, CRSImgOriginProp::PropertyType::XP));
		//location y
		// prop 1
		pLocationGroup->AddSubItem(new CRSImgOriginProp(pWrmDoc, CRSImgOriginProp::PropertyType::YP));
		//location x
		// prop 2
		pLocationGroup->AddSubItem(new CRSImgOriginProp(pWrmDoc, CRSImgOriginProp::PropertyType::XM));
		//location y
		// prop 3
		pLocationGroup->AddSubItem(new CRSImgOriginProp(pWrmDoc, CRSImgOriginProp::PropertyType::YM));
	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pLocationGroup);

	m_pPropGrid->AddProperty(m_pAppearenceGroup);
	//--------------------------------------------------

	//layer - for Z order filter in design mode
	CRSIntProp* pLayerProp = new CRSIntProp(pWrmDoc, _TB("Layer"), &(pWrmDoc->m_nCurrentLayer),
		_TB("It is a filter of visibility  to use during design mode to show different group of overlapped objects, only one group at a time"));
	m_pPropGrid->AddProperty(pLayerProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadSingleLayoutPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	CLayout* pLayout = dynamic_cast<CLayout*>(pNode->m_pItemData);
	if (!pLayout)
		return;
	ASSERT_VALID(pLayout);

	SetPanelTitle(pLayout->m_strLayoutName);

	CRSBoolPropWithLayoutRefresh* InvertProp = new CRSBoolPropWithLayoutRefresh(pLayout, _TB("Invert page orientation"), &pLayout->m_bInvertOrientation);
	InvertProp->SetStateImg(CRS_PropertyGrid::Img::Rotate);
	m_pPropGrid->AddProperty(InvertProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadLayoutObjectPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	// Add Appearence properties----------------------------------------------
	m_pAppearenceGroup = new CrsProp(_TB("Appearence"));
	// Add Behavior properties------------------------------------------------
	m_pBehaviorGroup = new CrsProp(_TB("Behavior"));
	// Add Layout properties--------------------------------------------------
	m_pLayoutGroup = new CrsProp(_TB("Layout"));

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_OBJ_TEXTRECT:
		{
			LoadTextProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_FIELDRECT:
		{
			LoadFieldProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_FILERECT:
		{
			LoadFileProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_GRAPHRECT:
		{
			LoadGraphProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_SQRRECT:
		{
			LoadSqrProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_CHART:
		{
			LoadChartProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_TABLE:
		{
			LoadTableProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_COLUMN:
		{
			LoadColumnProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_REPEATER:
		{
			LoadRepeaterProperties(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_OBJ_TOTAL:
		{
			LoadTableCellProperties(pNode);
			return;
		}

		default:
			return;
	}

	//------------------------------------------------------TODO spostare altrove?
	//aggiungo le properties alla propertyGrid dopo aver inserito tutti i subitem
	//per non avere problemi di refresh
	//--------------------------------------------------
	m_pPropGrid->AddProperty(m_pAppearenceGroup);
	//--------------------------------------------------
	m_pPropGrid->AddProperty(m_pBehaviorGroup);
	//--------------------------------------------------
	m_pPropGrid->AddProperty(m_pLayoutGroup);

	m_bShowLayoutBtn = TRUE;
	SetCheckLayoutBtn(TRUE);
}

// ----------------------------------------------------------------------------
// Load variable properties into property grid
void CRS_ObjectPropertyView::LoadVariablePropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	//pulisco la property grid
	if (m_pPropGrid->GetPropertyCount()>0)
		m_pPropGrid->RemoveAll();

	WoormField* field = dynamic_cast<WoormField*>(pNode->m_pItemData);
	if (!field)	
		return;
	ASSERT_VALID(field);
 	/*if (!field->IsHidden() && !GetDocument()->ExistsFieldID(field->GetId(), FALSE))
		field->SetHidden(TRUE);*/

	CPoint pt;
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return;
	}

	GetDocument()->SetCurrentObj(field->GetId(), pt);

	SetPanelTitle(field->GetName());

	LoadVariableGeneralSettings(field);
	LoadAttributesForExporting(field);

	m_bShowLayoutBtn		= !field->IsHidden();

	m_eShowVariableTypeBtn	= field->GetSourceEnum();
	m_bShowRequestFieldBtn	= field->IsAsk();
	m_bShowFindRuleBtn		= (field->m_bIsTableField || field->IsExprRuleField())  && !GetDocument()->RSTreeIsSelectedItemData(ERefreshEditor::Rules, pNode->m_pItemData);
	
	SetCheckVariableBtn(TRUE);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadVariableGeneralSettings(WoormField* wrmField)
{
	// General settings
	CrsProp* m_pGeneralSettings = new CrsProp(_TB("General Settings"));
	//Add general settings property to a property grid

	CString sName = wrmField->GetName();

	CString sLog; BOOL onlyUI = FALSE;
	BOOL d = GetDocument()->GetEditorManager()->GetPrgData()->CanDeleteField(sName, sLog, &onlyUI);
	BOOL allowRename = d || onlyUI;

	if (allowRename)
	{
		CRSVariableProp* varName = new CRSVariableProp(wrmField, _TB("Name"), 
											(LPCTSTR)wrmField->GetName(),
											CRSVariableProp::VariableType::FIELD_NAME, 0, _TB("The field can be renamed"));
		varName->SetColoredState(CrsProp::State::Mandatory);
		m_pGeneralSettings->AddSubItem(varName);
	}
	else
	{
		CrsProp* varName = new CrsProp(_TB("Name"), (LPCTSTR) sName);
		varName->AllowEdit(FALSE); 
		varName->SetColoredState(CrsProp::State::Important);
		m_pGeneralSettings->AddSubItem(varName);
	}

	// Field Type 
	CRSVariableProp* fTypeProp = new CRSVariableProp(wrmField, _TB("Field Type"), 
			(LPCTSTR)wrmField->GetSourceDescription(),
			CRSVariableProp::VariableType::FIELD_TYPE);	// dynamic second property instead of "All"
		fTypeProp->AllowEdit(FALSE);
		fTypeProp->SetColoredState(CrsProp::State::Important);
		if (!wrmField->IsTableRuleField() && !wrmField->IsAsk() 
			&& 
			(
				wrmField->GetFieldType() != WoormField::RepFieldType::FIELD_COLTOTAL &&
				wrmField->GetFieldType() != WoormField::RepFieldType::FIELD_SUBTOTAL
			)
			)
		{
			fTypeProp->AddOption(L"Function", TRUE, 0);
			fTypeProp->AddOption(L"Expression", TRUE, 1);
			fTypeProp->SelectOption(fTypeProp->GetOptionDataIndex(wrmField->IsExprRuleField() ? 1 : 0));
		}

	m_pGeneralSettings->AddSubItem(fTypeProp);

	if (wrmField->IsTableRuleField())
	{
		CrsProp* varName = new CrsProp(_TB("DB Name"), (LPCTSTR)wrmField->GetPhysicalName());
		varName->AllowEdit(FALSE);
		m_pGeneralSettings->AddSubItem(varName);
	}

	if (!wrmField->IsTableRuleField() && !wrmField->IsAsk())
	{
		CRSVariableProp* fInput	= new CRSVariableProp(wrmField, _TB("Input field"),(LPCTSTR)MakeStringFromBool(wrmField->IsInput()) , CRSVariableProp::VariableType::FIELD_IS_INPUT);
		fInput->AllowEdit(FALSE);		
		CString sLog;
		//convert function variable from input to normal and vice versa
		if (!wrmField->IsExprRuleField() && !wrmField->IsAsk() && GetDocument()->m_pEditorManager->GetPrgData()->CanConvertFieldToInput(wrmField->GetName(), sLog))
		{
			fInput->AddOption(L"False", TRUE, WoormField::RepFieldType::FIELD_NORMAL);
			fInput->AddOption(L"True", TRUE, WoormField::RepFieldType::FIELD_INPUT);
			fInput->SelectOption(fInput->GetOptionDataIndex( wrmField->IsInput() ? 1 : 0));
		}		

		m_pGeneralSettings->AddSubItem(fInput);
	}	
	
	CrsProp* varId = new CrsProp(_TB("Id"), (LPCTSTR) ::cwsprintf(L"%d", wrmField->GetId()));
	varId->SetDescription(L"Alias");
	varId->AllowEdit(FALSE);
	m_pGeneralSettings->AddSubItem(varId);

	// Hidden ?
	CrsProp* varProp = new CrsProp(_TB("Visible"),(LPCTSTR)MakeStringFromBool(!wrmField->IsHidden()));
	varProp->AllowEdit(FALSE);
	m_pGeneralSettings->AddSubItem(varProp);

	// DataType
	if (!wrmField->IsTableRuleField() && !wrmField->IsAsk())
	{
		//Array
		if (wrmField->IsHidden())
		{
			isArray = new CRSVariableProp(wrmField, _TB("Array"), (LPCTSTR)MakeStringFromBool(wrmField->GetDataType() == DataType::Array), CRSVariableProp::VariableType::FIELD_ISARRAY);
			isArray->AddOption(L"False", TRUE, FALSE);
			isArray->AddOption(L"True", TRUE, TRUE);
			isArray->SelectOption(isArray->GetOptionDataIndex(wrmField->GetDataType() == DataType::Array));
			if (!wrmField->IsInput())
				isArray->SetVisible(FALSE);
			m_pGeneralSettings->AddSubItem(isArray);
		}
	}

	CrsProp* typeProp = new CrsProp(_TB("Data type"), (LPCTSTR)wrmField->GetDataType().ToString());
	typeProp->AllowEdit(FALSE);
	m_pGeneralSettings->AddSubItem(typeProp);

	//Length and precision
	CrsProp* lpProp = NULL;
	if (wrmField->GetDataType() == DataType::String)
	{
		if (wrmField->IsAsk())
		{
			lpProp = new CrsProp(_TB("Length and row number"));
				lpProp->AddSubItem(new CRSVariableProp(wrmField, _TB("Length"), (_variant_t)wrmField->GetLen(), CRSVariableProp::VariableType::LENGTH));// dynamic second property instead of "15l"
				lpProp->AddSubItem(new CRSVariableProp(wrmField, _TB("Rows"), (_variant_t)wrmField->GetNumDec(), CRSVariableProp::VariableType::PRECISION));// dynamic second property instead of "5l"
		}
		else
		{
			lpProp = new CRSVariableProp(wrmField, _TB("Length"), (_variant_t)wrmField->GetLen(), CRSVariableProp::VariableType::LENGTH);
		}
	}

	else if (wrmField->GetData()->IsKindOf(RUNTIME_CLASS(DataDbl)))
	{
		lpProp = new CrsProp(_TB("Length and Precision"));
			lpProp->AddSubItem(new CRSVariableProp(wrmField, _TB("Length"), (_variant_t)wrmField->GetLen(), CRSVariableProp::VariableType::LENGTH));// dynamic second property instead of "15l"
			lpProp->AddSubItem(new CRSVariableProp(wrmField, _TB("Precision"), (_variant_t)wrmField->GetNumDec(), CRSVariableProp::VariableType::PRECISION));// dynamic second property instead of "5l"
	}
	else
	{
		lpProp = new CRSVariableProp(wrmField, _TB("Length"), (_variant_t)wrmField->GetLen(), CRSVariableProp::VariableType::LENGTH);
	}

	m_pGeneralSettings->AddSubItem(lpProp);

	//Initial value
	if (!wrmField->m_bIsTableField)
		m_pGeneralSettings->AddSubItem(new CRSExpressionProp(_TB("Initial expression"), &wrmField->GetInitExpression(), wrmField->GetDataType(), wrmField->GetSymTable(), this));

	if (wrmField->IsExprRuleField() && !wrmField->m_bIsTableField)
	{
		//RuleObj* pRule = wrmField->GetOwnerRule(); //NO � la Rule dell'engine e non dell'editor
		RuleDataObj* pRule = GetDocument()->m_pEditorManager->GetRuleData()->GetRuleData(wrmField->GetName());
		ASSERT_VALID(pRule);
		ExpRuleData* pExpRule = dynamic_cast<ExpRuleData*>(pRule);
		ASSERT_VALID(pExpRule);

		if (pExpRule)
		{
			CrsProp* pProp = NULL;
			m_pGeneralSettings->AddSubItem(pProp = new CRSRuleExpressionProp(_TB("Data Rule expression"), pExpRule, wrmField->GetDataType()));
			pProp->SetColoredState(CrsProp::State::Mandatory);
			m_pGeneralSettings->AddSubItem(new CRSExpressionProp(_TB("Validate expression"), &(pExpRule->m_pValidateWhereExpr), DataType::Bool, pExpRule->GetSymTable(), this));
		}
	}

	//Evaluation Expression must open text editor
	else if (!wrmField->IsExprRuleField() && (wrmField->GetDataType() != DataType::Array) && !wrmField->IsInput() && !wrmField->m_bIsTableField)
		m_pGeneralSettings->AddSubItem(new CRSExpressionProp(_TB("Evaluation expression"), (Expression**)&wrmField->GetEventFunction(), wrmField->GetDataType(), wrmField->GetSymTable(), this));

	//expression calculated column
	else if (m_pTreeNode->m_NodeType == CNodeTree::ENodeType::NT_VARIABLE && wrmField->IsTableRuleField() && wrmField->IsNativeColumnExpr())
	{
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pAncestorItemData);
		if (!pTblRule)
		{
			//sto selezionando la variabile passando dall'oggetto di layout e allora recupero il parent nelle rules dell'engine
			HTREEITEM engineHt = GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, wrmField);//GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetParentItem(GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, wrmField));
			CNodeTree*pEngineNode = (CNodeTree*)GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetItemData(engineHt);
			pTblRule = dynamic_cast<TblRuleData*>(pEngineNode->m_pAncestorItemData);
		}

		if (pTblRule)
		{
			DataFieldLink*  pObjLink = pTblRule->GetCalcColumn(wrmField->GetName());
			ASSERT_VALID(pObjLink);
			if (pObjLink)
			{
				m_pGeneralSettings->AddSubItem(new CRSSqlExpressionProp(wrmField, pTblRule, pObjLink, this, m_pTreeNode));
			}
		}
	}

	m_pPropGrid->AddProperty(m_pGeneralSettings);
}

// ----------------------------------------------------------------------------
// XML exporting
void CRS_ObjectPropertyView::LoadAttributesForExporting(WoormField* wrmField){

	// xml exporting
	CrsProp* m_pAttributesForXMLExporting = new CrsProp(_TB("Attributes for exporting"));

	// Do not export flag
	CrsProp* prop = new CRSVariableProp(wrmField, _TB("Do not export"), (LPCTSTR)MakeStringFromBool(wrmField->GetSkipXml()), CRSVariableProp::VariableType::DO_NOT_EXPORT);
	prop->AllowEdit(FALSE);
	prop->AddOption(_T("True"));
	prop->AddOption(_T("False"));
	m_pAttributesForXMLExporting->AddSubItem(prop);

	//alis name
	m_pAttributesForXMLExporting->AddSubItem(new CRSVariableProp(wrmField, _TB("Export with the following alias"), (LPCTSTR)wrmField->GetXmlName(), CRSVariableProp::VariableType::EXPORT_ALIAS));

	//Add xml exporing property to a property grid
	m_pPropGrid->AddProperty(m_pAttributesForXMLExporting);
}

// ----------------------------------------------------------------------------
// Load ask dialog properties into property grid
void CRS_ObjectPropertyView::LoadAskDialogPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	AskDialogData* dlg = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
	if (dlg)
		SetPanelTitle(dlg->GetName() + L" - "+_TB("Ask dialog settings"));

	LoadAskDialogSettings(dlg);
}

// ----------------------------------------------------------------------------
// Ask dialog settings
void CRS_ObjectPropertyView::LoadAskDialogSettings(AskDialogData* askDialogField)
{
	CrsProp* m_pAskDialogSettings = new CrsProp(_TB("Dialog Settings"));

	// Add dialog name
	m_pAskDialogSettings->AddSubItem(new CRSAskDialogProp(askDialogField, _TB("Name"), (LPCTSTR)askDialogField->GetName(), CRSAskDialogProp::DialogType::NAME, this));

	// Add dialog title
	CRSAskDialogProp* pTitle = new CRSAskDialogProp(askDialogField, _TB("Title"), (LPCTSTR)askDialogField->GetTitle(), CRSAskDialogProp::DialogType::TITLE,this);
	pTitle->SetColoredState(CrsProp::State::Important);
	m_pAskDialogSettings->AddSubItem(pTitle);

	//Position of fields combo
	
	CrsProp* posProp = new CRSAskDialogProp(askDialogField, _TB("Position of field titles"), (LPCTSTR)GetTokenString(askDialogField->m_nFieldsCaptionPos), CRSAskDialogProp::DialogType::POSITION_OF_FIELDS,this);
	posProp->AllowEdit(FALSE);
	posProp->AddOption(_TB("Default"), TRUE, Token::T_DEFAULT);
	posProp->AddOption(_TB("Left"), TRUE, Token::T_LEFT);
	posProp->AddOption(_TB("Top"), TRUE, Token::T_TOP);
	m_pAskDialogSettings->AddSubItem(posProp);

	//Dialog box only on event
	CrsProp* askProp = new CRSAskDialogProp(askDialogField, _TB("Dialog box only on event"), (LPCTSTR)MakeStringFromBool(askDialogField->IsOnAsk()), CRSAskDialogProp::DialogType::ONLY_ON_EVENT,this);
	askProp->AllowEdit(FALSE);
	askProp->AddOption(_T("True"));
	askProp->AddOption(_T("False"));
	askProp->SetDescription(_TB("Dialog only on event has not Before/After events and check expressions"));
	m_pAskDialogSettings->AddSubItem(askProp);

	// Initialization instruction
	m_pAskDialogSettings->AddSubItem(new CRSBlockExpressionProp(_TB("Initialization instruction"), &askDialogField->m_pBeforeBlock, askDialogField->GetSymTable(), this));

	// Initialization instruction
	m_pAskDialogSettings->AddSubItem(new CRSBlockExpressionProp(_TB("Validation instruction"), &askDialogField->m_pAfterBlock, askDialogField->GetSymTable(), this));

	//Displays if
	m_pAskDialogSettings->AddSubItem(new CRSExpressionProp(_TB("Visible if:"), &askDialogField->m_pWhenExpr, DataType::Bool, askDialogField->GetSymTable(), this));

	//Data acceptable if:
	m_pAskDialogSettings->AddSubItem(new CRSExpressionProp(_TB("Abort if:"), &askDialogField->m_pOnExpr, DataType::Bool, askDialogField->GetSymTable(), this));

	//Data acceptable if:
	m_pAskDialogSettings->AddSubItem(new CRSExpressionProp(_TB("Message on error"), &askDialogField->m_pAbortExpr, DataType::String, askDialogField->GetSymTable(), this));

	// Add ask dialog properties to property grid
	m_pPropGrid->AddProperty(m_pAskDialogSettings);

	/*CStringList lstCommands;
	lstCommands.AddTail(_TB("Preview"));
	m_pPropGrid->SetCommands(lstCommands);*/
	m_pPropGrid->AdjustLayout(); //---> verificare se serve
}

// ----------------------------------------------------------------------------
// Load ask group properties into property grid
void CRS_ObjectPropertyView::LoadAskGroupPropertyGrid(CNodeTree* pNode){

	ASSERT_VALID(pNode);

	ASSERT_VALID(m_pPropGrid);

	//pulisco la property grid
	m_pPropGrid->RemoveAll();

	AskGroupData* group = dynamic_cast<AskGroupData*>(pNode->m_pItemData);

	//Set Panel Title
	if (group)	{
		try{
			SetPanelTitle(group->GetTitle() + L" - " + _TB("Ask group settings"));
		}
		catch (...){}
	}

	LoadAskGroupGeneralSettings(group);
}

// ----------------------------------------------------------------------------
// Load ask field properties into property grid
void CRS_ObjectPropertyView::LoadAskGroupGeneralSettings(AskGroupData* group){

	CrsProp* m_pAskGroupGeneral = new CrsProp(_TB("Group Information"));

	//Hidden title
	m_pAskGroupGeneral->AddSubItem(new CRSBoolProp(group, _TB("Hide Group frame"), &(group->m_bHiddenTitle), L""));

	//Caption
	CRSGroupFieldProp* caption = new CRSGroupFieldProp(group, _TB("Caption"), group->GetTitle(), CRSGroupFieldProp::AskGroupType::CAPTION);
	caption->SetColoredState(CrsProp::State::Important);
	m_pAskGroupGeneral->AddSubItem(caption);

	//Caption position
	CRSGroupFieldProp* captionPos = new CRSGroupFieldProp(group, _TB("Caption position"), GetTokenString(group->m_nFieldsCaptionPos), CRSGroupFieldProp::AskGroupType::CAPTION_POSITION);
	captionPos->AllowEdit(FALSE);
	captionPos->AddOption(AskDlgStrings::DEFAULT(), TRUE, T_DEFAULT);
	captionPos->AddOption(AskDlgStrings::LEFT(), TRUE, T_LEFT);
	captionPos->AddOption(AskDlgStrings::UP(), TRUE, T_TOP);
	m_pAskGroupGeneral->AddSubItem(captionPos);

	CRSExpressionProp* pe = NULL;
	m_pAskGroupGeneral->AddSubItem(pe = new CRSExpressionProp(_TB("Visible if True"), &group->m_pWhenExpr, DataType::Bool, &group->m_SymTable, this));
	pe->SetDescription(_TB("If the expression is not empty, the full group will be visible only when its result is TRUE"));
	pe->m_bViewMode = FALSE;

	m_pPropGrid->AddProperty(m_pAskGroupGeneral);
}

//-----------------------------------------------------------------------------
// Create new field or column from database
void CRS_ObjectPropertyView::NewDBElement(BOOL newTable, BOOL isAllFieldsAreHidden, TblRuleData* pTblRule/* = NULL*/)
{
	if (!ClearPropertyGrid()) return;
	
	m_bCreatingNewTable = newTable;
	m_bAllFieldsAreHidden = isAllFieldsAreHidden;  //hidden rule
	m_bNeedsApply = TRUE;

	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_DB_ELEMENT;

	if (newTable)
	{
		if (isAllFieldsAreHidden)
			SetPanelTitle(_TB("Create new Rule"));
		else
			SetPanelTitle(_TB("Create new table with DB columns"));
	}

	else if (GetDocument()->CurrentIsTable())
		SetPanelTitle(_TB("Create new column from DB"));
	else
		SetPanelTitle(_TB("Create new field from DB"));
	
	CRSCommonProp* newRule = new CRSCommonProp(_TB("Choose rule..."), L"", CRSCommonProp::PropType::EXISTING_RULE_OR_NEW_RULE, this);
	newRule->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(newRule);

	WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();

	RuleDataArray* pRules = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
	int firstOccurance = -1;
	if (!pTblRule)
	{
		for (int r = 0; r < pRules->GetSize(); r++)
		{
			TblRuleData* pR = dynamic_cast<TblRuleData*>(pRules->GetAt(r));
			if (!pR)
				continue;

			ASSERT_VALID(pR);

			//NO REFACTORING//
			if (firstOccurance == -1)
				firstOccurance = r;
			/////////////////

			CString sDescr = pR->GetRuleDescription();
			newRule->AddOption(sDescr, FALSE, (DWORD_PTR)pR);
		}

		newRule->AddOption(_TB("New rule..."),TRUE, 0);
		newRule->SelectOption(/*newRule->GetOptionCount() - 1*/newRule->GetOptionDataIndex(0));
	}

	else
	{
		CString sDescr = pTblRule->GetRuleDescription();
		newRule->AddOption(sDescr, FALSE, (DWORD_PTR)pTblRule);
		newRule->SelectOption(/*newRule->GetOptionCount()-1*/newRule->GetOptionDataIndex((DWORD_PTR)pTblRule));
		firstOccurance = 0;
	}

	// if new table or there are no rules
	if ((newTable && !pTblRule) || firstOccurance==-1)
	{ 	
		newRule->SelectOption(newRule->GetOptionCount()-1);

		CRSCommonProp* module = new CRSCommonProp(_TB("Module"), L"", CRSCommonProp::PropType::NEW_RULE_MODULE, this);
		LoadTableModules(module);

		CRSCommonProp* table = new CRSCommonProp(_TB("Table"), L"", CRSCommonProp::PropType::NEW_RULE_TABLES, this);
		//table->AllowEdit(FALSE);
		LoadTables(table, NULL);

		m_pPropGrid->AddProperty(module, TRUE, FALSE);
		m_pPropGrid->AddProperty(table, TRUE, TRUE);
		m_pPropGrid->SetCurSel(newRule);
		m_pPropGrid->SetCurSel(module,TRUE);
	}

	// if there are rules 
	else
	{
		CRSCommonProp* tables = new CRSCommonProp(_TB("Table"), L"", CRSCommonProp::EXISTING_RULE_TABLES, this);
		//tables->AllowEdit(FALSE);

		TblRuleData* pR = pTblRule ? pTblRule : dynamic_cast<TblRuleData*>(pRules->GetAt(firstOccurance));

		if (!newTable)
		{
			newRule->SelectOption(0);

			for (int i = 0; i < pR->m_arSqlTableJoinInfoArray.GetSize(); i++)
			{
				tables->AddOption(((SqlTableInfo*)pR->m_arSqlTableJoinInfoArray[i])->GetTableName(), TRUE, (DWORD_PTR)pR);

				if (i == 0)
				{
					tables->SetValue((LPCTSTR)pR->m_arSqlTableJoinInfoArray[i]->GetTableName());
					tables->SelectOption(0);
				}
			}
		}

		else
		{
			CString name = (LPCTSTR)pR->m_arSqlTableJoinInfoArray[0]->GetTableName();
			tables->AddOption((LPCTSTR)pR->m_arSqlTableJoinInfoArray[0]->GetTableName(), TRUE, (DWORD_PTR)pR);
			tables->SelectOption(0);
			tables->AllowEdit(FALSE);
		}

		m_pPropGrid->AddProperty(tables, TRUE, TRUE);

		CrsProp* columnBlock = new CrsProp(_TB("Columns"));
		m_pPropGrid->AddProperty(columnBlock, TRUE, TRUE);
		InsertColumnBlock(columnBlock, NULL, pR->m_arSqlTableJoinInfoArray[0]->GetTableName(), pR);
		m_pPropGrid->SetCurSel(tables,TRUE);
	}

	m_pPropGrid->SetFocus();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::InsertColumnBlock(CBCGPProp* father, CBCGPProp* caller, CString tblName, TblRuleData* pTblRule)
{
	ASSERT_VALID(pTblRule);
	
	const SqlTableInfo* pT = pTblRule->m_arSqlTableJoinInfoArray.GetTableInfo(tblName);
	ASSERT_VALID(pT);
	if (!pT) 
		return;

	BOOL bQualified = pTblRule->m_arSqlTableJoinInfoArray.GetSize() > 1;

	CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);
	CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pT->GetSqlCatalogEntry());
	ASSERT_VALID(pTC);
	if (!pTC)
		return;

	//--------------------
	CString varPrefix = L"w_";

	for (int i = 0; i < pTC->m_arSortedColumns.GetSize(); i++)
	{
		const SqlColumnInfo* pSqlColumnInfo =  (SqlColumnInfo*)(pTC->m_arSortedColumns.GetAt(i));
		ASSERT_VALID(pSqlColumnInfo);
		if (!pSqlColumnInfo)
			continue;

		if (i == 0)
		{
			CString sTableName = pSqlColumnInfo->m_strTableName;
			//int idx = sTableName.Find('_');
			//if (idx < 4)
			//	varPrefix = sTableName.Mid(idx + 1);
			//else 
				varPrefix = sTableName + '_';
		}

		if (pSqlColumnInfo->m_bVirtual)
			continue;

		if (
			pTblRule->ExistPhysicalName(pSqlColumnInfo->GetColumnName()) ||
			pTblRule->ExistPhysicalName(pSqlColumnInfo->GetQualifiedColumnName())
			)
			continue;

		CRSChkDBCol* column = new CRSChkDBCol(pSqlColumnInfo->GetColumnName(), FALSE,L"",(DWORD_PTR)pTblRule,this);

		CString sDescr = pSqlColumnInfo->GetColumnTitle();
		if (pSqlColumnInfo->m_bSpecial)
		{
			column->SetStateImg(CRS_PropertyGrid::Img::PrimaryKey);
			sDescr = _TB("It is a primary key segment. ") + sDescr;
		}

		column->SetDescription(sDescr);
		//column->SetGroup(TRUE);
		father->AddSubItem(column);

		//Name
		CString sVarName = GetDocument()->GetEditorSymTable()->GetAdviseName(varPrefix + pSqlColumnInfo->GetColumnName());

		CRSCommonProp* nameCol = new CRSCommonProp(_TB("Name"), (LPCTSTR)sVarName,CRSCommonProp::PropType::COLUMN_BLOCK_NAME,this, _TB("Variable name"));
		nameCol->SetVisible(FALSE);
		nameCol->SetData((DWORD)pSqlColumnInfo);
		column->AddSubItem(nameCol);
		//nameCol->SetOwnerList(m_pPropGrid);

		//Type
		CRSCommonProp* typeCol = new CRSCommonProp(_TB("Data type"), pSqlColumnInfo->GetDataObjType().ToString(), CRSCommonProp::PropType::NEW_COLUMN_TYPE, this);
		typeCol->AllowEdit(FALSE);
		typeCol->SetVisible(FALSE);
		column->AddSubItem(typeCol);
		//typeCol->SetOwnerList(m_pPropGrid);

		CWordArray typeArray;
		pSqlColumnInfo->GetDataObjTypes(typeArray);
		LoadTypeProp(typeCol, typeArray);
		{
			DataType dtCol = pSqlColumnInfo->GetDataObjType();
			if (dtCol.m_wType == DATA_ENUM_TYPE) 
				dtCol.m_wTag = 0;
			for (int i = 0; i < typeCol->GetOptionCount(); i++)
			{
				DataType dt = *(DataType*) typeCol->GetOptionData(i);
				if (dt == dtCol)
				{
					typeCol->SelectOption(i);
				}
			}

			if (typeCol->GetOptionCount() && typeCol->GetSelectedOption() == -1)
				typeCol->SelectOption(0);
		}

		//typeCol->SetGroup(TRUE);
		
		//SubType - Enums
		CRSCommonProp* subTypeCol = new CRSCommonProp(_TB("Enumerative subtype"), L"", CRSCommonProp::PropType::NEW_COLUMN_ENUM, this);
		//subTypeCol->AllowEdit(FALSE);
		subTypeCol->SetVisible(FALSE);

		typeCol->AddSubItem(subTypeCol);
		//subTypeCol->SetOwnerList(m_pPropGrid);
		//typeCol->SetGroup(FALSE);

		if (pSqlColumnInfo->GetDataObjType().m_wType == DATA_ENUM_TYPE && pSqlColumnInfo->GetDataObjType().m_wTag)
		{
			if (subTypeCol->GetOptionCount() == 0)
				this->LoadEnumTypeProp(subTypeCol);

			subTypeCol->SetValue((LPCTSTR)AfxGetEnumsTable()->GetEnumTagTitle(pSqlColumnInfo->GetDataObjType().m_wTag));
			
			int pos = subTypeCol->GetOptionDataIndex(pSqlColumnInfo->GetDataObjType().m_wTag);
			if (pos > -1)
				subTypeCol->SelectOption(pos);
		}

		//Hidden
		CrsProp* isHidden = new CrsProp(_TB("Hidden"), (LPCTSTR)L"False");
		isHidden->AllowEdit(FALSE);
		isHidden->SetVisible(FALSE);
		isHidden->AddOption(L"False", TRUE, FALSE);
		isHidden->AddOption(L"True", TRUE, TRUE);
		isHidden->SelectOption(isHidden->GetOptionDataIndex(m_bAllFieldsAreHidden));
		//if (m_bAllFieldsAreHidden) // create new hidden rule
		//	isHidden->SelectOption(1);
		//else
		//	isHidden->SelectOption(0);
		column->AddSubItem(isHidden);
		//isHidden->SetOwnerList(m_pPropGrid);

		//----
		//column->SetGroup(FALSE);
	}

	m_pPropGrid->AdjustLayout();
}

// ----------------------------------------------------------------------------
//prop grid for new column or field from db
void CRS_ObjectPropertyView::CreateNewDBElement()
{
	//GetDocument()->m_pCurrentObj = NULL;
	//GetDocument()->DeleteDragRect();
	//GetDocument()->OnDeselectAll();

	TblRuleData* pTblRule;
	int index = m_pPropGrid->GetProperty(0)->GetSelectedOption();
	if (index < 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(0))->SetColoredState(CrsProp::Mandatory);
		 return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(0))->SetColoredState(CrsProp::Normal);
		
	DWORD_PTR option = m_pPropGrid->GetProperty(0)->GetOptionData(index);  //wether i create a new rule or choose an existing one

	//Property "Columns" witn various "column"-subProperties 
	int numProp = m_pPropGrid->GetPropertyCount();
	CBCGPProp* columnsProp = option == 0 ? (numProp > 3 ? m_pPropGrid->GetProperty(3) : NULL ) : ( numProp > 2 ? m_pPropGrid->GetProperty(2) : NULL) ;
	if (!columnsProp)
		return;

	pTblRule = (TblRuleData*)columnsProp->GetSubItem(0)->GetData();
	ASSERT_VALID(pTblRule);
	if (!pTblRule)
		return;
	BOOL bQualified = pTblRule->m_arSqlTableJoinInfoArray.GetSize() > 1;

	Table* currentTable = NULL;
	if (GetDocument()->CurrentIsTable() && !m_bCreatingNewTable)
	{
		ASSERT_VALID(GetDocument()->m_pCurrentObj);
		ASSERT_KINDOF(Table, GetDocument()->m_pCurrentObj);

		currentTable = (Table*)GetDocument()->m_pCurrentObj;
	}

	GetDocument()->m_wCreatingColumnIds.RemoveAll();

	BOOL bCallClipCursorToActiveView = FALSE;

	// if New rule then first add TableRuleData to rules
	if (option == 0/* && pTblRule) || (pTblRule && !GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData()->GetRuleData(pTblRule->GetRuleDescription()))*/)
	{
		RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
		pRuleData->Add(pTblRule);
	}

	int lastVisibleFieldID = 0;
	for (int i = 0; i < columnsProp->GetSubItemsCount(); i++)
	{
		CRSChkDBCol* pColProp = dynamic_cast<CRSChkDBCol*>(columnsProp->GetSubItem(i));
		ASSERT_VALID(pColProp);

		if (!pColProp->IsVisible())
			continue;

		BOOL ifChecked = pColProp->GetValue();
		if (!ifChecked)
			continue;

		
		//Variable name
		CrsProp* propVarName = dynamic_cast<CrsProp*>(pColProp->GetSubItem(0));
		ASSERT_VALID(propVarName);

		CString errMsg = _T("");
		if (!propVarName->CheckPropValue(FALSE, errMsg))
		{
			if (!errMsg.IsEmpty())
				AfxMessageBox(errMsg);
			return;
		}	

		const SqlColumnInfo* currentColumn = (SqlColumnInfo*)((CObject*)propVarName->GetData());
		ASSERT_VALID(currentColumn);

		// new DataType
		CRSCommonProp* propType = dynamic_cast<CRSCommonProp*>(pColProp->GetSubItem(1));
		ASSERT_VALID(propType);
		int index = propType->GetSelectedOption();
		if (index < 0)
		{
			propType->SetColoredState(CrsProp::Error);
			return;
		}

		else
			propType->SetColoredState(CrsProp::Normal);

		DataType dType = *(DataType*)propType->GetOptionData(index);
		if (dType == DATA_ENUM_TYPE)
		{
			// new DataType
			CRSCommonProp* propSubType = dynamic_cast<CRSCommonProp*>(propType->GetSubItem(0));
			ASSERT_VALID(propSubType);

			index = propSubType->GetSelectedOption();
			if (index < 0)
			{
				propSubType->SetColoredState(CrsProp::Error);
				return;
			}

			else
				propSubType->SetColoredState(CrsProp::Normal);

			WORD enumType = (WORD)propSubType->GetOptionData(index);
			dType = DataType(DATA_ENUM_TYPE, enumType);
		}

		//hidden property
		CrsProp* propHidden = dynamic_cast<CrsProp*>(pColProp->GetSubItem(2));
		ASSERT_VALID(propHidden);
		BOOL hidden = FALSE;
		int hiddenIndex = propHidden->GetSelectedOption();
		if (hiddenIndex < 0)
		{
			propHidden->SetColoredState(CrsProp::Error);
			return;
		}

		else
			propHidden->SetColoredState(CrsProp::Normal);

		hidden = (BOOL)propHidden->GetOptionData(hiddenIndex);
	
		CString name = propVarName->GetValue();
		name = name.Trim();

		CString phName = bQualified ? currentColumn->GetQualifiedColumnName() : currentColumn->GetColumnName();

		/*DataFieldLink* pFL = */pTblRule->AddLink(phName, name, FALSE, dType, pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetUpperBound());

		WoormTable*	pSymTable = pTblRule->GetSymTable();		
		WoormField* pRepField = new WoormField(name, (currentTable || m_bCreatingNewTable) && !hidden ? WoormField::RepFieldType::FIELD_COLUMN : WoormField::RepFieldType::FIELD_NORMAL, dType);
		pRepField->SetHidden(hidden);
		pRepField->SetPrecision(
			AfxGetFormatStyleTable()->GetInputCharLen(dType, &GetDocument()->GetNamespace()),
			currentColumn->GetColumnDecimal());
		pRepField->SetTableRuleField(TRUE);
		pRepField->SetSpecialField(currentColumn->m_bSpecial);
		pRepField->SetPhysicalName(phName);
		pSymTable->Add(pRepField);

		//if I add columns to an existing table and variables are not hidden
		if (currentTable && !hidden)
			pRepField->SetDispTable(currentTable->GetName(TRUE));

		if (!hidden)
		{
			GetDocument()->SyncronizeViewSymbolTable(pRepField);

			if (currentTable)
			{
				CWordArray ids; ids.Add(pRepField->GetId());
				currentTable->AddColumn(ids);
			}

			else
			{
				if (!m_bCreatingNewTable)
				{
					bCallClipCursorToActiveView = TRUE;
					lastVisibleFieldID = pRepField->GetId();
				}

				GetDocument()->m_wCreatingId = pRepField->GetId();
				GetDocument()->m_wCreatingColumnIds.Add(pRepField->GetId());
			}
		}

		//m_pPropGrid->DeleteProperty(pColProp, TRUE, TRUE); i--;
		pColProp->SetVisible(FALSE);
		m_pPropGrid->AdjustLayout();
	}
	
	if (bCallClipCursorToActiveView)
	{
		GetDocument()->m_Creating = CWoormDocMng::ObjectType::FIELDRECT;
		GetDocument()->ClipCursorToActiveView();
	}

	// Add new Table
	GetDocument()->OnAddTable();

	m_bNeedsApply = FALSE;

	ASSERT_VALID(pTblRule);

	GetDocument()->RefreshRSTree(ERefreshEditor::Rules);
	GetDocument()->RefreshRSTree(ERefreshEditor::Variables);

	if (currentTable)
	{
		CObject* pObj = currentTable->GetActiveColumn() ? (CObject*)currentTable->GetActiveColumn() : (CObject*)currentTable;

		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pObj);
	}

	else
	{
		if (lastVisibleFieldID)
		{
			BaseObj* pFieldRect = GetDocument()->GetObjects().FindByID(lastVisibleFieldID);
			if (pFieldRect)
			{
				GetDocument()->SelectRSTreeItemData(ERefreshEditor::Layouts, pFieldRect);
			}
		}

		else
		{
			GetDocument()->SelectRSTreeItemData(ERefreshEditor::Rules, pTblRule);
		}
	}

	m_bCreatingNewTable = FALSE;
	m_bAllFieldsAreHidden = FALSE;

	GetDocument()->SetModifiedFlag();
}

// ----------------------------------------------------------------------------
// Create a new expression or function field or column
void CRS_ObjectPropertyView::NewElement(BOOL newTable, BOOL FromHiddenVariable, BOOL fromDragnDrop, BOOL forceHidden)
{
	if (!ClearPropertyGrid()) return;

	m_bCreatingNewTable = newTable;
	m_bFromDragnDrop = fromDragnDrop;

	m_bNeedsApply = TRUE;
	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_ELEMENT;

	if (newTable)
		SetPanelTitle(_TB("Create new table"));
	else if (GetDocument()->CurrentIsTable())
		SetPanelTitle(_TB("Create new column"));
	else
		SetPanelTitle(_TB("Create new field"));

	CrsProp* pNewVariable = new CrsProp(_TB("Add variable"));

	// count the hidden fields and adds (or not) the option FROM HIDDEN FIELD  // 
	CStringArray orderedArray;

	if (FromHiddenVariable)
		for (int i = 0; i < GetDocument()->GetEditorSymTable()->GetSize(); i++)
		{
			WoormField* pF = (WoormField*)GetDocument()->GetEditorSymTable()->GetAt(i);
			ASSERT_VALID(pF);

			if (pF->IsColTotal() || pF->IsSubTotal() || /*pF->IsInput() ||*/ pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
				continue;
			if (pF->IsHidden())
				orderedArray.Add(pF->GetName());
		}

	//SubItem 0

	CRSCommonProp* typeProp = new CRSCommonProp(_TB("Variable type"), _TB("Expression"), CRSCommonProp::PropType::NEW_VAR_TYPE, this);
	typeProp->AllowEdit(FALSE);
	if (!FromHiddenVariable)
	{
		typeProp->AddOption(_TB("Function"), TRUE, 0);
		typeProp->AddOption(_TB("Expression"), TRUE, 1);
		typeProp->SelectOption(1);
		typeProp->SetColoredState(CrsProp::State::Important);
	}

	else
	{
		typeProp->AddOption(_TB("From hidden variable"), TRUE, 2);
		typeProp->SetValue((LPCTSTR)_TB("From hidden variable"));
		typeProp->SelectOption(0);
	}

	pNewVariable->AddSubItem(typeProp);

	//SubItem 1
	CRSCommonProp* nameProp = new CRSCommonProp(_TB("Name"), (LPCTSTR)L"", CRSCommonProp::PropType::NEW_NAME, this);
	nameProp->SetVisible(!FromHiddenVariable);
	nameProp->SetColoredState(CrsProp::State::Mandatory);
	pNewVariable->AddSubItem(nameProp);

	//SubItem 2
	CrsProp* fromHiddenField = new CrsProp(_TB("Choose variables"));
	fromHiddenField->AllowEdit(FALSE);
	fromHiddenField->SetOwnerList(m_pPropGrid);
	if (!FromHiddenVariable)
		fromHiddenField->SetVisible(FALSE);

	if (FromHiddenVariable) 
	{
		CStringArray_Sort(orderedArray);

		for (int i = 0; i < orderedArray.GetSize(); i++)
		{
			WoormField* pF = (WoormField*)GetDocument()->GetEditorSymTable()->GetField(orderedArray[i]);
			ASSERT_VALID(pF);
			if (!pF)
				continue;
			CRSCheckBoxProp* chkProp = new CRSCheckBoxProp(orderedArray[i], FALSE, L"", (DWORD_PTR)pF, this);
			chkProp->SetOwnerList(m_pPropGrid);
			fromHiddenField->AddSubItem(chkProp);
		}
	}

	pNewVariable->AddSubItem(fromHiddenField);

/*
	CRSBoolProp* isHidden = new CRSBoolProp(this, _TB("Hidden"), &m_bIsHidden);
	isHidden->SetVisible(!FromHiddenVariable);
	m_fieldType = WoormField::RepFieldType::FIELD_COLUMN;
	m_bIsHidden = FALSE;
*/	 
	//SubItem 3
	CRSCommonProp* isHidden = new CRSCommonProp(_TB("Hidden"), MakeStringFromBool(FALSE), CRSCommonProp::PropType::NEW_FIELD_ISHIDDEN, this);
	if (!forceHidden)
		isHidden->AddOption((LPCTSTR)MakeStringFromBool(FALSE), TRUE, FALSE);
	isHidden->AddOption((LPCTSTR)MakeStringFromBool(TRUE), TRUE, TRUE);
	isHidden->SelectOption(0);
	isHidden->SetVisible(!FromHiddenVariable && !forceHidden);
	m_fieldType = WoormField::RepFieldType::FIELD_NORMAL;
	m_bIsHidden = forceHidden;
	pNewVariable->AddSubItem(isHidden);

	//SubItem 4
	CRSCommonProp* typeField = new CRSCommonProp(_TB("Field type"), _TB("Normal"), CRSCommonProp::PropType::NEW_FIELD_TYPE, this);
	typeField->SetVisible(FALSE);
	typeField->AllowEdit(FALSE);
	typeField->AddOption(_TB("Normal"), TRUE, WoormField::RepFieldType::FIELD_NORMAL);
	typeField->SelectOption(0);

	pNewVariable->AddSubItem(typeField);

	//SubItem 5
	CRSBoolProp* isArray = new CRSBoolProp(this, _TB("Array"), &m_bIsArray);
	
	isArray->SetVisible(FALSE);
	pNewVariable->AddSubItem(isArray);

	//NEW_VAR_TYPE
	m_NewType = DataType::String;
	//SubItem 6
	CRSCommonProp* dTypeProp = new CRSCommonProp(_TB("Data type"), DataType::String.ToString(), CRSCommonProp::PropType::NEW_TYPE, this);
	dTypeProp->AllowEdit(FALSE);
	dTypeProp->SetVisible(!FromHiddenVariable);
	pNewVariable->AddSubItem(dTypeProp);

	//SubItem 7
	CRSCommonProp* propEnumsType = new CRSCommonProp(_TB("Enumerative subtype"), L"", CRSCommonProp::PropType::NEW_ENUMTYPE, this);
	//propEnumsType->AllowEdit(FALSE);
	pNewVariable->AddSubItem(propEnumsType);
	propEnumsType->SetVisible(FALSE);

	dTypeProp->m_pChildProp = propEnumsType;

	m_pPropGrid->AddProperty(pNewVariable);
	m_pPropGrid->SetCurSel(nameProp,TRUE);
	m_pPropGrid->SetFocus();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewAskField()
{
	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->CheckPropValue(FALSE, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}
	else
	{
		((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->SetColoredState(CrsProp::Normal);
	}

	m_NewName.Trim();
	WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();

	WoormField* pRepField = new WoormField(m_NewName, WoormField::FIELD_INPUT);
	pRepField->SetDataType(m_NewType);
	pRepField->SetHidden(TRUE);
	pRepField->SetAsk(TRUE);
	pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(m_NewType, &GetDocument()->GetNamespace()));
	pSymTable->Add(pRepField);

	GetDocument()->SyncronizeViewSymbolTable(pRepField);

	AskGroupData* pAskGroup = dynamic_cast<AskGroupData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pAskGroup);

	AskFieldData* pNewAskField = new AskFieldData(pAskGroup);
	pNewAskField->m_CtrlStyle = m_NewType == DataType::Bool ? AskFieldData::CtrlStyle::CHECK_BOX : AskFieldData::CtrlStyle::EDIT;
	pNewAskField->m_strPublicName = m_NewName;
	pAskGroup->AddAskField(pNewAskField);

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
	GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pNewAskField);

	GetDocument()->SetModifiedFlag();
}

// ----------------------------------------------------------------------------
// Create a new ask field into property grid
void CRS_ObjectPropertyView::NewAskFieldPropertyGrid(CNodeTree* pNode)
{
	SetPanelTitle(_TB("Add new ask field"));

	CrsProp* pNewAskFieldGeneral = new CrsProp(_TB("General Settings"));

	m_NewName = GetDocument()->GetEditorSymTable()->GetAdviseName(_TB("w_AskField"));
	CRSCommonProp* pPropName = new CRSCommonProp(_TB("Name"), (LPCTSTR)m_NewName, CRSCommonProp::PropType::NEW_NAME, this);
	pPropName->SetColoredState(CrsProp::Mandatory);
	pNewAskFieldGeneral->AddSubItem(pPropName);

	m_NewType = DataType::String;
	CRSCommonProp* propType = new CRSCommonProp(_TB("Data type"), DataType::String.ToString(), CRSCommonProp::PropType::NEW_TYPE, this);
	pNewAskFieldGeneral->AddSubItem(propType);

	CRSCommonProp* propEnumsType = new CRSCommonProp(_TB("Enumerative subtype"), L"", CRSCommonProp::PropType::NEW_ENUMTYPE, this);
	//propEnumsType->AllowEdit(FALSE);
	pNewAskFieldGeneral->AddSubItem(propEnumsType);
	propEnumsType->SetVisible(FALSE);

	propType->m_pChildProp = propEnumsType;

	m_pPropGrid->AddProperty(pNewAskFieldGeneral);
}

// ----------------------------------------------------------------------------
// Load ask field properties into property grid
void CRS_ObjectPropertyView::LoadAskFieldPropertyGrid(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);

	WoormField* pField = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable()->GetField(pAskField->GetPublicName());
	ASSERT_VALID(pField);
	if (!pField->IsHidden() && !GetDocument()->ExistsFieldID(pField->GetId(), FALSE))
		pField->SetHidden(TRUE);

	SetPanelTitle(pAskField->GetPublicName() + L" - " + _TB("Ask field settings"));

	CRSAskFieldProp* pControlType = NULL;
	LoadAskFieldGeneralSettings(pAskField);
	LoadAskFieldCaption(pAskField);
	LoadAskFieldBehavior(pAskField, pControlType);
	LoadAskFieldHotLinkSettings(pAskField, pControlType);

	m_bShowLayoutBtn		= !pField->IsHidden(); 
	m_eShowVariableTypeBtn	= pField->GetSourceEnum();
	m_bShowRequestFieldBtn	= TRUE;

	SetCheckRequestFieldBtn(TRUE);
}

// ----------------------------------------------------------------------------
// Load ask field general properties into property grid
void CRS_ObjectPropertyView::LoadAskFieldGeneralSettings(AskFieldData* field)
{
	WoormField* pF = dynamic_cast<WoormField*>(GetDocument()->GetEditorSymTable()->GetField(field->GetPublicName()));
	ASSERT_VALID(pF);

	CrsProp* pAskFieldGeneral = new CrsProp(_TB("General settings"));

	CrsProp* p;
	pAskFieldGeneral->AddSubItem(p = new CRSAskFieldProp(field, _TB("Name"), (LPCTSTR)field->GetPublicName(), CRSAskFieldProp::AskFieldName::NAME,this));
	p->AllowEdit(FALSE);

	m_pPropGrid->AddProperty(pAskFieldGeneral);
}

// ----------------------------------------------------------------------------
// Load caption properties into property grid
void CRS_ObjectPropertyView::LoadAskFieldCaption(AskFieldData* field)
{
	CrsProp* pAskFieldCaption = new CrsProp(_TB("Caption"));

	CRSStringWithExprProp* prop = new CRSStringWithExprProp(
		field,											/*owner della property*/
		_TB("Caption"),									/*name della property*/
		&(field->m_strCaption),							/*puntatore al value della property*/
		&(field->m_pCaptionExpr),						/*puntatore all'exp della property*/
		(GetDocument()->GetSymTable()),					/*symtable per editview*/
		_TB("Request field caption"),					/*description della property*/
		this, 											/*propertyview (this)*/
		TRUE);
	prop->SetColoredState(CrsProp::State::Important);
	pAskFieldCaption->AddSubItem(prop);

	if (field->m_CtrlStyle != AskFieldData::CHECK_BOX && field->m_CtrlStyle != AskFieldData::RADIO_BUTTON)
	{
		CRSAskFieldProp* captionPos = new CRSAskFieldProp(field, _TB("Position"), (LPCTSTR)GetTokenString(field->m_nCaptionPos), CRSAskFieldProp::AskFieldName::CAPTION_POSITION, this);
			captionPos->AllowEdit(FALSE);
			captionPos->AddOption(AskDlgStrings::DEFAULT(), TRUE, T_DEFAULT);
			captionPos->AddOption(AskDlgStrings::LEFT(), TRUE, T_LEFT);
			captionPos->AddOption(AskDlgStrings::UP(), TRUE, T_TOP);

		pAskFieldCaption->AddSubItem(captionPos);
	}

	else
	{
		CRSAskFieldProp* nearFieldFlag = new CRSAskFieldProp(field, _TB("Near the border"), (LPCTSTR)L"False", CRSAskFieldProp::AskFieldName::NEAR_THE_BORDER, this);
		nearFieldFlag->AllowEdit(FALSE);
		nearFieldFlag->AddOption(L"True", TRUE, TRUE);
		nearFieldFlag->AddOption(L"False", TRUE, FALSE);
		nearFieldFlag->SelectOption(nearFieldFlag->GetOptionDataIndex(field->m_bLeftBoolAlign) /*field->m_bLeftBoolAlign ? 0 : 1*/);

		pAskFieldCaption->AddSubItem(nearFieldFlag);

		CRSAskFieldProp* leftText = new CRSAskFieldProp(field, _TB("Show text to left"), (LPCTSTR)L"False", CRSAskFieldProp::AskFieldName::BOOL_LEFT_TEXT, this);
		leftText->AllowEdit(FALSE);
		leftText->AddOption(L"True", TRUE, TRUE);
		leftText->AddOption(L"False", TRUE, FALSE);
		leftText->SelectOption(leftText->GetOptionDataIndex(field->m_bLeftTextBool)/*field->m_bLeftTextBool ? 0 : 1*/);

		pAskFieldCaption->AddSubItem(leftText);
	}

	m_pPropGrid->AddProperty(pAskFieldCaption);
}

// ----------------------------------------------------------------------------
// Loads limit/style section of askfiled
void CRS_ObjectPropertyView::LoadAskFieldBehavior(AskFieldData* field, CRSAskFieldProp*& pControlType) 
{
	WoormField* pF = dynamic_cast<WoormField*>(GetDocument()->GetEditorSymTable()->GetField(field->GetPublicName()));

	CrsProp* pBehavior = new CrsProp(_TB("Behavior"));

	pControlType = new CRSAskFieldProp(field, _TB("Control Type"), "", CRSAskFieldProp::AskFieldName::CONTROL_STYLE, this, pF);
	pControlType->AllowEdit(FALSE);
	pControlType->SetValue((LPCTSTR)GetControlStyleString(field->m_CtrlStyle));

	pBehavior->AddSubItem(pControlType);

	CRSAskFieldProp* m_pRangeLimit = NULL;
	if (
			pF->GetDataType().m_wType != DataType::Enum.m_wType &&
			pF->GetDataType() != DataType::Bool &&
			pF->GetDataType() != DataType::Guid
		)
	{
		m_pRangeLimit = new CRSAskFieldProp(field, _TB("Range Limit"), (LPCTSTR)GetTokenString(field->m_nInputLimit), CRSAskFieldProp::AskFieldName::RANGE_LIMIT, this);
			m_pRangeLimit->AllowEdit(FALSE);
			m_pRangeLimit->AddOption((LPCTSTR)GetTokenString(Token::T_NOTOKEN), TRUE, Token::T_NOTOKEN);
			m_pRangeLimit->AddOption((LPCTSTR)GetTokenString(Token::T_LOWER_LIMIT), TRUE, Token::T_LOWER_LIMIT);
			m_pRangeLimit->AddOption((LPCTSTR)GetTokenString(Token::T_UPPER_LIMIT), TRUE, Token::T_UPPER_LIMIT);
		pBehavior->AddSubItem(m_pRangeLimit);
	}

	if (pF->GetDataType() != DataType::Bool) 
	{	
		
		pControlType->AddOption(GetControlStyleString(AskFieldData::CtrlStyle::EDIT),			TRUE, AskFieldData::CtrlStyle::EDIT);

		if (pF->GetDataType() != DataType::Date && pF->GetDataType() != DataType::DateTime)
		{
			pControlType->AddOption(GetControlStyleString(AskFieldData::CtrlStyle::EDIT_HKL),		TRUE, AskFieldData::CtrlStyle::EDIT_HKL);
			pControlType->AddOption(GetControlStyleString(AskFieldData::CtrlStyle::COMBO_BOX),		TRUE, AskFieldData::CtrlStyle::COMBO_BOX);			
		}
				
		pControlType->SelectOption(pControlType->GetOptionDataIndex(AskFieldData::CtrlStyle::EDIT));

		//controllo se m_pControlType e' abilitato. Se abilitato aggiungo le option
		AddOnApplication* pAddOnApp = AfxGetAddOnApp(field->m_nsHotLink.GetApplicationName());
		AddOnModule* pAddOnMod = AfxGetAddOnModule(field->m_nsHotLink);
		if (pAddOnMod) 
		{
			CHotlinkDescription* pComp = (CHotlinkDescription*)pAddOnMod->m_XmlDescription.GetParamObjectInfo(field->m_nsHotLink);
			BOOL bCanBeCombo = pComp->HasComboBox();

			HotKeyLinkObj* pHL = AfxGetTbCmdManager()->RunHotlink(field->m_nsHotLink, NULL, NULL);
			if (pHL)
			{
				pHL->InitNamespace();
				bCanBeCombo = pHL->IsFillListBoxEnabled();
				SAFE_DELETE(pHL);
			}

			pControlType->SetEnable(bCanBeCombo);
			int k = pControlType->GetOptionDataIndex(AskFieldData::CtrlStyle::COMBO_BOX);
			if (bCanBeCombo)
			{
				switch(field->m_CtrlStyle)
				{
					case AskFieldData::CtrlStyle::COMBO_BOX:
						pControlType->SelectOption(pControlType->GetOptionDataIndex(AskFieldData::CtrlStyle::COMBO_BOX));
						break;
					case AskFieldData::CtrlStyle::EDIT_HKL:
						pControlType->SelectOption(pControlType->GetOptionDataIndex(AskFieldData::CtrlStyle::EDIT_HKL));
						break;
					case AskFieldData::CtrlStyle::EDIT:
					default:
						pControlType->SelectOption(pControlType->GetOptionDataIndex(AskFieldData::CtrlStyle::EDIT));
						break;
				}
			}
		}
	}

	else 
	{
		pControlType->AddOption(GetControlStyleString(AskFieldData::CtrlStyle::CHECK_BOX), TRUE, AskFieldData::CtrlStyle::CHECK_BOX);
		pControlType->AddOption(GetControlStyleString(AskFieldData::CtrlStyle::RADIO_BUTTON), TRUE, AskFieldData::CtrlStyle::RADIO_BUTTON);
		pControlType->SelectOption(pControlType->GetOptionDataIndex(field->m_CtrlStyle ));
	}

	CRSVariableProp* reinitProp = new CRSVariableProp(pF, _TB("Reinitialization rule"), pF->IsReInit() ? _TB("Always") : (pF->IsStatic() ? _TB("Never") : _TB("Normal")), CRSVariableProp::VariableType::REINIT_INPUT, TRUE);
	reinitProp->AllowEdit(FALSE);
	reinitProp->SetDescription(_TB("It drive the reinizialization rule on re-running the report"));
	reinitProp->AddOption(_TB("Always"), TRUE, CRSVariableProp::ReinitType::REINIT_ALWAYS);
	reinitProp->AddOption(_TB("Never"), TRUE, CRSVariableProp::ReinitType::REINIT_NEVER);
	reinitProp->AddOption(_TB("Normal"), TRUE, CRSVariableProp::ReinitType::REINIT_NORMAL);
	if (pF->IsReInit())
		reinitProp->SelectOption(0);
	else if (pF->IsStatic())
		reinitProp->SelectOption(1);
	else
		reinitProp->SelectOption(2);
	pBehavior->AddSubItem(reinitProp);

	CRSExpressionProp* pe = new CRSExpressionProp(_TB("Disable if True"), &field->m_pReadOnlyExpr, DataType::Bool, pF->GetSymTable(), this);
	pBehavior->AddSubItem(pe);
	pe->SetDescription(_TB("If the expression is not empty, the entry will be readonly when its result is TRUE"));
	pe->m_bViewMode = FALSE;

	pe = new CRSExpressionProp(_TB("Visible if True"), &field->m_pWhenExpr, DataType::Bool, pF->GetSymTable(), this);
	pBehavior->AddSubItem(pe);
	pe->SetDescription(_TB("If the expression is not empty, the entry will be visible only when its result is TRUE"));
	pe->m_bViewMode = FALSE;

	m_pPropGrid->AddProperty(pBehavior);
}

// ----------------------------------------------------------------------------
// Load hotlink properties into property grid
void CRS_ObjectPropertyView::LoadAskFieldHotLinkSettings(AskFieldData* field, CRSAskFieldProp* pControlType)
{
	ASSERT_VALID(pControlType);

	hotLinkGroup = NULL;
	hotLinkName = NULL;
	hotlinkNamespace = NULL;
	hotlinkCustParameters = NULL;
	showHotlinkDescription = NULL;
	hotlinkDescriptionCombo = NULL;
	hotlinkMultiselectionCombo = NULL;

	WoormField* pF = dynamic_cast<WoormField*>(GetDocument()->GetEditorSymTable()->GetField(field->GetPublicName()));
	ASSERT_VALID(pF);

	CHotlinkDescription* pHKLDescription = NULL;

	CrsProp* pAskFieldHotLink = new CrsProp(_TB("Hotlink"));
	
	//Load hotlink group
	hotLinkGroup = new CRSAskFieldProp(field, _TB("Module"), "<None>", CRSAskFieldProp::AskFieldName::HOTLINK_GROUP,this);
		hotLinkGroup->AllowEdit(FALSE);
		hotLinkGroup->AddOption(L"<None>");
		LoadHotlinkModules(hotLinkGroup, pF->GetDataType());	
	
	//show hot link description combo
	showHotlinkDescription = new CRSAskFieldProp(field, _TB("Show Hotlink Description"), (LPCTSTR)MakeStringFromBool(field->m_bShowHotLinkDescription), CRSAskFieldProp::AskFieldName::SHOW_HOTLINK_DESCRIPTION, this);
		showHotlinkDescription->AllowEdit(FALSE);
		showHotlinkDescription->AddOption(L"True");
		showHotlinkDescription->AddOption(L"False");
		showHotlinkDescription->SetVisible(FALSE);	//TODO

	//get current group
	// set value of current group e name
	CString currentName = L"<None>";
	hotLinkName = new CRSAskFieldProp(field, _TB("Name"), currentName, CRSAskFieldProp::AskFieldName::HOTLINK_NAME, this);
	hotLinkName->AllowEdit(FALSE);
	hotLinkName->AddOption(currentName);
	hotLinkName->m_pChildProp = pControlType;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(field->m_nsHotLink);
	if (field->m_nsHotLink.IsValid())
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnApp(field->m_nsHotLink.GetApplicationName());
		if (pAddOnMod && pAddOnApp)
		{
			CString currentModule = cwsprintf(_T("%s - %s"), pAddOnApp->GetTitle(), pAddOnMod->GetModuleTitle());
			hotLinkGroup->SetValue((LPCTSTR)currentModule);
			hotLinkGroup->m_pOldValue = currentModule;

			//Load hotlink names
			this->LoadHotlinks(hotLinkName, pAddOnMod, pF->GetDataType());
			const CBaseDescriptionArray& aObjects = pAddOnMod->m_XmlDescription.GetReferencesInfo().GetHotLinks();

			//Get Current Name
			CBaseDescription* pInfo = pAddOnMod->m_XmlDescription.GetParamObjectInfo(field->m_nsHotLink);
			pHKLDescription = dynamic_cast<CHotlinkDescription*>(pInfo);

			CString currentName = L"<None>";
			if (pInfo)
			{
				if (pInfo->GetTitle().IsEmpty())
					currentName = pInfo->GetNamespace().GetObjectName();
				else
					currentName = pInfo->GetTitle();
			}

			hotLinkName->SetValue((LPCTSTR)currentName);
			hotLinkName->m_pOldValue = currentName;
			if (currentName.Compare(L"<None>") == 0)
			{
				showHotlinkDescription->RemoveAllOptions();
				showHotlinkDescription->AllowEdit(FALSE);
			}
		}
	}

	//HotLinkNamespace
	hotlinkNamespace = new CRSAskFieldProp(field, _TB("Namespace"), (LPCTSTR)(field->m_nsHotLink.ToString().IsEmpty() ? L"" : field->m_nsHotLink.ToString()), CRSAskFieldProp::AskFieldName::HOTLINK_NAMESPACE,this);
	hotlinkNamespace->AllowEdit(FALSE);	

	//Customization parameters
	hotlinkCustParameters = new CrsProp(_TB("Parameters"));
	WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();

	if (pHKLDescription && pHKLDescription->GetParameters().GetSize() > 0)
	{
		for (int p = 0; p < pHKLDescription->GetParameters().GetSize(); p++)
		{
			CDataObjDescription* pDOD = pHKLDescription->GetParamDescription(p);

			if (p > field->m_HotLinkParamsExpr.GetUpperBound())
			{
				field->m_HotLinkParamsExpr.Add(new Expression(pSymTable));
			}

			if (field->m_HotLinkParamsExpr[p])
			{
				Expression** exp = (Expression**)&field->m_HotLinkParamsExpr[p];
			
				CRSExpressionProp* pP = new CRSExpressionProp(pDOD->GetName(), exp, pDOD->GetDataType(), pSymTable, this);
				pP->SetDescription(pDOD->GetTitle());
				hotlinkCustParameters->AddSubItem(pP);
			}
		}

		hotlinkCustParameters->SetVisible(TRUE);
	}

	else
		hotlinkCustParameters->SetVisible(FALSE);
	
	//Description Combo + Multiselection Combo
	
	hotlinkDescriptionCombo = new CRSAskFieldProp(field, _TB("Description combo"), (LPCTSTR)MakeStringFromBool(field->m_bDescriptionCombo), CRSAskFieldProp::AskFieldName::DESCRIPTION_COMBO, this);
	hotlinkDescriptionCombo->AllowEdit(FALSE);
	hotlinkDescriptionCombo->AddOption(L"True");
	hotlinkDescriptionCombo->AddOption(L"False");
	hotlinkDescriptionCombo->SetVisible(FALSE);	

	hotlinkMultiselectionCombo = new CRSAskFieldProp(field, _TB("Multi selection combo"), (LPCTSTR)MakeStringFromBool(field->m_bMultiSelectionCombo ), CRSAskFieldProp::AskFieldName::MULTI_SELECTION_COMBO, this);
	hotlinkMultiselectionCombo->AllowEdit(FALSE);
	hotlinkMultiselectionCombo->AddOption(L"True");
	hotlinkMultiselectionCombo->AddOption(L"False");
	
	if (pHKLDescription)
	{
		BOOL bCanBeCombo = pHKLDescription->HasComboBox();
		HotKeyLinkObj* pHL = AfxGetTbCmdManager()->RunHotlink(field->m_nsHotLink, NULL, NULL);
		if (pHL)
		{
			pHL->InitNamespace();
			bCanBeCombo = pHL->IsFillListBoxEnabled();
			SAFE_DELETE(pHL);
		}
	
		pControlType->SetEnable(bCanBeCombo); 

		if ( (field->m_CtrlStyle == AskFieldData::CtrlStyle::EDIT_HKL && !bCanBeCombo) || (field->m_nInputLimit==Token::T_LOWER_LIMIT || field->m_nInputLimit == Token::T_UPPER_LIMIT))
		{
			hotlinkDescriptionCombo->RemoveAllOptions();
			hotlinkDescriptionCombo->AllowEdit(FALSE);

			hotlinkMultiselectionCombo->RemoveAllOptions();
			hotlinkMultiselectionCombo->AllowEdit(FALSE);
			hotlinkMultiselectionCombo->SetValue(L"False");
		}
	}

	// add to property grid
	pAskFieldHotLink->AddSubItem(hotLinkGroup);
	pAskFieldHotLink->AddSubItem(hotLinkName);
	pAskFieldHotLink->AddSubItem(hotlinkNamespace);
	pAskFieldHotLink->AddSubItem(showHotlinkDescription);
	pAskFieldHotLink->AddSubItem(hotlinkDescriptionCombo);
	pAskFieldHotLink->AddSubItem(hotlinkMultiselectionCombo);
	pAskFieldHotLink->AddSubItem(hotlinkCustParameters);
	
	if (hotLinkGroup->GetOptionCount() == 1)	 // It contains only the <None> option
		pAskFieldHotLink->SetVisible(FALSE);

	m_pPropGrid->AddProperty(pAskFieldHotLink);
}

///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------

CRSAskFieldProp::CRSAskFieldProp(AskFieldData* askField, CString name, LPCTSTR value, AskFieldName dFieldType, 
								CRS_ObjectPropertyView* propertyView,  WoormField* woormField) : CrsProp(name, value)
{
	this->m_askField = askField;
	this->m_dFieldType = dFieldType;
	this->m_dWoormField = woormField;
	this->m_pPropertyView = propertyView;

	ASSERT_VALID(askField);
}

//-----------------------------------------------------------------------------
CRSAskFieldProp::CRSAskFieldProp(AskFieldData* askField, CString name, variant_t value, AskFieldName dFieldType, 
								CRS_ObjectPropertyView* propertyView, WoormField* woormField) : CrsProp(name, value)
{
	this->m_askField = askField;
	this->m_dFieldType = dFieldType;
	this->m_dWoormField = woormField;
	this->m_pPropertyView = propertyView;

	ASSERT_VALID(askField);
}

//-----------------------------------------------------------------------------
BOOL CRSAskFieldProp::OnUpdateValue(){

	CString oldValue = GetValue();
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	CString value = this->GetValue();
	if (value.CompareNoCase(oldValue) == 0)
		return TRUE;

	switch (m_dFieldType)
	{
	case AskFieldName::NAME:{
			m_askField->m_strPublicName = value;
			break;
		}

		case AskFieldName::TYPE:{
			if (m_dWoormField == NULL)
				break;
			m_dWoormField->SetDataType(DataType(value));
			break;
		}

		case AskFieldName::HOTLINK_GROUP:
		{
			int sel = this->GetSelectedOption();
			AddOnModule* pMod = NULL;
			if (sel >= 0)
				pMod = dynamic_cast<AddOnModule*> ((CObject*)this->GetOptionData(sel));
			HotLinkGroupChanged(value, pMod);
			break;
		}

		case AskFieldName::HOTLINK_NAME:
		{
			int sel = this->GetSelectedOption();
			CHotlinkDescription* pHklDescr = NULL;
			if (sel >= 0)
				pHklDescr = dynamic_cast<CHotlinkDescription*> ((CObject*)this->GetOptionData(sel));
			HotLinkNameChanged(value, pHklDescr);
				
			break;
		}

		case AskFieldName::SHOW_HOTLINK_DESCRIPTION:{
			//TODO			
			m_askField->m_bShowHotLinkDescription = MakeBoolFromString(value);
			break;
		}

		case AskFieldName::DESCRIPTION_COMBO:{
			//TODO	
			m_askField->m_bDescriptionCombo = MakeBoolFromString(value);
			break;
		}

		case AskFieldName::MULTI_SELECTION_COMBO:
		{
			m_askField->m_bMultiSelectionCombo = MakeBoolFromString(value);
			//if (m_askField->m_bMultiSelectionCombo)
			//{
				//m_askField->m_CtrlStyle = AskFieldData::CtrlStyle::COMBO_BOX;
			//}

			break;
		}

		case AskFieldName::CAPTION_POSITION:
		{
			int index = GetSelectedOption();
			if (index < 0)
				break;
			DWORD_PTR option = GetOptionData(index);
			m_askField->m_nCaptionPos = (Token)option;
			break;
		}

		case AskFieldName::CONTROL_STYLE:
		{
			int index = GetSelectedOption();
			if (index < 0)
				break;
			DWORD_PTR option = GetOptionData(index);
			m_askField->m_CtrlStyle = (AskFieldData::CtrlStyle)option;

			//if (m_askField->m_CtrlStyle == AskFieldData::CtrlStyle::EDIT_HKL)
			//{
				//m_askField->m_bDescriptionCombo = m_askField ->m_bMultiSelectionCombo = FALSE;
			//}

			break;
		}

		case AskFieldName::NEAR_THE_BORDER:
		{
			m_askField->m_bLeftBoolAlign = MakeBoolFromString(value);
			break;
		}

		case AskFieldName::BOOL_LEFT_TEXT:
		{
			m_askField->m_bLeftTextBool = MakeBoolFromString(value);
			break;
		}

		case AskFieldName::RANGE_LIMIT:
		{
			int index = GetSelectedOption();
			if (index < 0)
				break;
			DWORD_PTR option = GetOptionData(index);
			m_askField->m_nInputLimit = (Token)option;
			if (option == T_UPPER_LIMIT || option == T_LOWER_LIMIT)
			{
				m_pPropertyView->hotlinkMultiselectionCombo->RemoveAllOptions();
				m_pPropertyView->hotlinkMultiselectionCombo->AllowEdit(FALSE);
				m_pPropertyView->hotlinkMultiselectionCombo->SetValue(L"False");
				m_askField->m_bMultiSelectionCombo = FALSE;
			}

			else
			{
				m_pPropertyView->hotlinkMultiselectionCombo->AllowEdit(FALSE);
				m_pPropertyView->hotlinkMultiselectionCombo->AddOption(L"True",TRUE);
				m_pPropertyView->hotlinkMultiselectionCombo->AddOption(L"False",FALSE);
			}
				
			m_pPropertyView->m_pPropGrid->AdjustLayout();
			break;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
// Loads Hotlink names associated with the new group name
void CRSAskFieldProp::HotLinkGroupChanged(CString value, AddOnModule* pAddOnMod)
{
	if (m_pOldValue.Compare(value) == 0)
		return;
	
	ASSERT_VALID(m_pPropertyView->hotLinkName);
	m_pPropertyView->hotLinkName->RemoveAllOptions();		
	m_pPropertyView->hotLinkName->SetValue("<None>");

	ASSERT_VALID(m_pPropertyView->hotlinkDescriptionCombo);
	m_pPropertyView->hotlinkDescriptionCombo->RemoveAllOptions();
	m_pPropertyView->hotlinkDescriptionCombo->AllowEdit(FALSE);

	if (value.Compare(L"<None>") == 0 || !pAddOnMod)
	{
		HotLinkNameChanged(L"<None>", NULL);
		return;
	}

	ASSERT_VALID(pAddOnMod);

	m_pOldValue = value;

	WoormField* pF = dynamic_cast<WoormField*>(GetDocument()->GetEditorSymTable()->GetField(m_askField->GetPublicName()));
	ASSERT_VALID(pF);

	this->GetPropertyView()->LoadHotlinks(m_pPropertyView->hotLinkName, pAddOnMod, pF->GetDataType());
}

//-----------------------------------------------------------------------------
// Loads Namespace associated with the new hotlink name
void CRSAskFieldProp::HotLinkNameChanged(CString value, CHotlinkDescription* pHklDescr)
{
	if (m_pOldValue.Compare(value) == 0)
		return;

	m_pOldValue = value;

	ASSERT_VALID(m_askField);
	m_askField->m_HotLinkParamsExpr.RemoveAll();

	m_pPropertyView->hotlinkCustParameters->SetVisible(FALSE);
	m_pPropertyView->hotlinkCustParameters->RemoveAllSubItems();

	if (m_pChildProp)
	{
		m_pChildProp->SelectOption(m_pChildProp->GetOptionDataIndex(AskFieldData::CtrlStyle::EDIT));
		m_pChildProp->SetEnable(FALSE);
	}

	m_pPropertyView->m_pPropGrid->AdjustLayout();

	if (value.Compare(L"<None>") == 0 || !pHklDescr)
	{
		m_pPropertyView->hotlinkNamespace->SetValue(L"");
		return;
	}

	ASSERT_VALID(pHklDescr);

	m_askField->m_nsHotLink.SetNamespace(pHklDescr->GetNamespace());

	m_pPropertyView->hotlinkNamespace->SetValue((LPCTSTR)m_askField->m_nsHotLink.ToString());

	m_pPropertyView->showHotlinkDescription->AddOption(L"True");
	m_pPropertyView->showHotlinkDescription->AddOption(L"False");
	m_pPropertyView->showHotlinkDescription->AllowEdit(FALSE);

	BOOL bCanBeCombo = pHklDescr->HasComboBox();
	HotKeyLinkObj* pHL = AfxGetTbCmdManager()->RunHotlink(m_askField->m_nsHotLink, NULL, NULL);
	if (pHL)
	{
		pHL->InitNamespace();
		bCanBeCombo = pHL->IsFillListBoxEnabled();
		SAFE_DELETE(pHL);
	}

	if (!bCanBeCombo)
	{
		m_pPropertyView->hotlinkDescriptionCombo->RemoveAllOptions();
		m_pPropertyView->hotlinkDescriptionCombo->AllowEdit(FALSE);

		m_pPropertyView->hotlinkMultiselectionCombo->RemoveAllOptions();
		m_pPropertyView->hotlinkMultiselectionCombo->AllowEdit(FALSE);
	}

	else
	{
		m_pPropertyView->hotlinkDescriptionCombo->AddOption(L"True");
		m_pPropertyView->hotlinkDescriptionCombo->AddOption(L"False");
		m_pPropertyView->hotlinkDescriptionCombo->AllowEdit(FALSE);

		m_pPropertyView->hotlinkMultiselectionCombo->AddOption(L"True");
		m_pPropertyView->hotlinkMultiselectionCombo->AddOption(L"False");
		m_pPropertyView->hotlinkMultiselectionCombo->AllowEdit(FALSE);

		if (m_pChildProp)
		{
			m_pChildProp->SelectOption(m_pChildProp->GetOptionDataIndex(AskFieldData::CtrlStyle::COMBO_BOX));	//AskFieldData::CtrlStyle::COMBO_BOX
			m_pChildProp->SetEnable(TRUE);
		}
	}

	SymTable*	pSymTable = m_pPropertyView->GetDocument()->GetEditorSymTable();
	for (int i = 0; i <= pHklDescr->GetParameters().GetUpperBound(); i++)
	{
		CDataObjDescription* pDOD = pHklDescr->GetParamDescription(i);
		Expression* exp = new Expression(pSymTable);
		exp->AssignStr(pDOD->GetDataType().FormatDefaultValue());
		int p = m_askField->m_HotLinkParamsExpr.Add(exp);
		CRSExpressionProp* pP = new CRSExpressionProp(pDOD->GetName(), (Expression**)&m_askField->m_HotLinkParamsExpr[i], pDOD->GetDataType(), pSymTable, m_pPropertyView);
		pP->SetDescription(pDOD->GetTitle());
		m_pPropertyView->hotlinkCustParameters->AddSubItem(pP);
	}

	/////////////////////////////////////////////////////////////////////////////
	// DO NOT DELETE //	DO NOT REFACTOR // DO NOT MOVE //
	// if you refactor nothing will work
	if (m_pPropertyView->hotlinkCustParameters->GetSubItemsCount() > 0)
		m_pPropertyView->hotlinkCustParameters->SetVisible(TRUE);
	
	for (int i = 0; i < m_askField->m_HotLinkParamsExpr.GetCount();i++)
		((CRSExpressionProp*)m_pPropertyView->hotlinkCustParameters->GetSubItem(i))->m_ppExp =(Expression**)(&m_askField->m_HotLinkParamsExpr[i]);
	/////////////////////////////////////////////////////////////////////////////

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadBaseProperties(CNodeTree* pNode)
{
	BaseRect* pBase = dynamic_cast<BaseRect*>(pNode->m_pItemData);
	if (!pBase)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pBase);

	LoadBaseAppearenceProperties(pBase);

	LoadBaseBehaviorProperties(pBase);

	LoadBaseLayoutProperties(pBase);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadBaseAppearenceProperties(BaseRect* pBase)
{
	CList<CBCGPProp*>* depList = new CList<CBCGPProp*>();

	//Style
	m_pAppearenceGroup->AddSubItem(new CRSStyleProp(pBase, this));

	//BackGroundColor 
	Expression** bkgExpr = pBase->GetBkgColorExpr();
	CRSColorProp* bkgColorProp = NULL;

	if (pBase->GetBkgColor())
	{
		if (bkgExpr)
			bkgColorProp = new CRSColorWithExprProp(pBase, _TB("Background Color"), pBase->GetBkgColor(), pBase->GetBkgColorExpr(), _TB("The background color of this component"), this);
		else
			bkgColorProp = new CRSColorProp(pBase, _TB("Background Color"), pBase->GetBkgColor(), _TB("The background color of this component"));
	
		depList->AddTail(bkgColorProp);
	}

	//transparent std
	m_pAppearenceGroup->AddSubItem(new CRSBoolPropWithDepListToDisable(pBase, _TB("Transparent"), &(pBase->m_bTransparent), depList, _TB("Determines whether the control background must be transparent"), TRUE));

	//add BackGroundColor
	if (bkgColorProp)
		m_pAppearenceGroup->AddSubItem(bkgColorProp);

	//---shadow group
	CrsProp* pShadowGroup = new CrsProp(_TB("Shadow"), 0, FALSE);
		//color
		pShadowGroup->AddSubItem(new CRSColorProp(pBase, _TB("ShadowColor"), &(pBase->m_crDropShadowColor), _TB("This property specifies the color of the shadow of this control")));
		//size
		pShadowGroup->AddSubItem(new CRSIntProp(pBase, _TB("ShadowSize"), &(pBase->m_nDropShadowHeight), _TB("The size in pixels of the shadow around the control")));
	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pShadowGroup);

	//---border group
	CrsProp* pBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
	BorderPen borderPen = pBase->m_BorderPen;
		//color
		pBorderGroup->AddSubItem(new CRSColorProp(pBase, _TB("Color"), &(pBase->m_BorderPen.m_rgbColor), _TB("This property specifies the color of the border around the control")));
		//size
		pBorderGroup->AddSubItem(new CRSIntProp(pBase, _TB("Size"), &(pBase->m_BorderPen.m_nWidth), _TB("The size in pixels of the border around the control")));
		//Ratio group
		CrsProp* pRatioGroup = new CrsProp(_TB("Ratio"), 0, TRUE);
			//H
			pRatioGroup->AddSubItem(new CRSIntProp(pBase, _TB("H"), &(pBase->m_nHRatio), _TB("H Ratio")));
			//V
			pRatioGroup->AddSubItem(new CRSIntProp(pBase, _TB("V"), &(pBase->m_nVRatio), _TB("V Ratio")));
		//add to the parent group
		pBorderGroup->AddSubItem(pRatioGroup);

		//---Side
		CrsProp* pSideGroup = new CrsProp(_TB("Sides"), 0, TRUE);
		pSideGroup->AllowEdit(FALSE);
			//left
			pSideGroup->AddSubItem(new CRSBoolProp(pBase, _TB("Left"), &(pBase->m_Borders.left), _TB("Left border")));
			//top
			pSideGroup->AddSubItem(new CRSBoolProp(pBase, _TB("Top"), &(pBase->m_Borders.top), _TB("Top border")));
			//right
			pSideGroup->AddSubItem(new CRSBoolProp(pBase, _TB("Right"), &(pBase->m_Borders.right), _TB("Right border")));
			//bottom
			pSideGroup->AddSubItem(new CRSBoolProp(pBase, _TB("Bottom"), &(pBase->m_Borders.bottom), _TB("Bottom border")));
		//add to the parent group
		pBorderGroup->AddSubItem(pSideGroup);

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pBorderGroup);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadBaseBehaviorProperties(BaseRect* pBase)
{
	//Tooltip expression
	CWoormDocMng* wrmDocMng = this->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	//Hidden Prop
	m_pBehaviorGroup->AddSubItem(new CRSHiddenProp(pBase, _TB("Hidden"), &(pBase->m_pHideExpr), DataType::Bool, wrmDocMng->GetSymTable(), this, _TB("The expression associated to this property")));
	//Tooltip
	m_pBehaviorGroup->AddSubItem(new CRSExpressionProp(_TB("Tooltip"), &(pBase->m_pTooltipExpr), DataType::String, wrmDocMng->GetSymTable(), this, _TB("The expression associated to this property"), TRUE, TRUE));
	//Borders
	m_pBehaviorGroup->AddSubItem(new CRSExpressionProp(_TB("Borders side"), &(pBase->m_pBordersExpr), DataType::String, wrmDocMng->GetSymTable(), this, _TB("The expression associated to this property, should return a string like \"left,-bottom\""), TRUE, TRUE));
	//persistent
	CrsProp* pProp = new CrsProp(_T("Is Persistent"),
		pBase->m_bPersistent == 1 ? (_variant_t)_TB("True") : (_variant_t)_TB("False")
		);
	pProp->Enable(FALSE);
	m_pBehaviorGroup->AddSubItem(pProp);

	AddBasicCommands();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadBaseLayoutProperties(BaseRect* pBase)
{
	//Anchor Left
	m_pLayoutGroup->AddSubItem(new CRSBoolProp(pBase, _TB("Anchorleft"), &(pBase->m_bAnchorPageLeft), _TB("Determines whether the control is repated in each page of the report at the position")));

	//Anchor to Column
	m_pLayoutGroup->AddSubItem(new CRSAnchorToProp(pBase, m_pPropGrid));

	//---location group
	CrsProp* pLocationProp = new CrsProp(_TB("Location"), 0, FALSE);
		//x	 px	  prop 0
		pLocationProp->AddSubItem(new CRSRectProp(pBase, _TB("X px"),  _TB("X"), CRSRectProp::PropertyType::LocationXP));
		//y	 px	  prop 1
		pLocationProp->AddSubItem(new CRSRectProp(pBase, _TB("Y px"),  _TB("Y"), CRSRectProp::PropertyType::LocationYP));
		//x	 mm	  prop 2
		pLocationProp->AddSubItem(new CRSRectProp(pBase, _TB("X mm"),  _TB("X"), CRSRectProp::PropertyType::LocationXM));
		//y	 mm	   prop 3
		pLocationProp->AddSubItem(new CRSRectProp(pBase, _TB("Y mm"),  _TB("Y"), CRSRectProp::PropertyType::LocationYM));
	//add to the parent group
	m_pLayoutGroup->AddSubItem(pLocationProp);
	pLocationProp->Expand(FALSE);

	//---size group
	CrsProp* pSizeProp = new CrsProp(_TB("Size"), 0, FALSE);
		//width	  px prop 0
		pSizeProp->AddSubItem(new CRSRectProp(pBase, _TB("Width px"),  _TB("Width"), CRSRectProp::PropertyType::WidthP));
		//height  px prop 1
		pSizeProp->AddSubItem(new CRSRectProp(pBase, _TB("Height px"),  _TB("Height"), CRSRectProp::PropertyType::HeightP));
		//width	  mm prop 2
		pSizeProp->AddSubItem(new CRSRectProp(pBase, _TB("Width mm"),  _TB("Width"), CRSRectProp::PropertyType::WidthM));
		//height  mm prop 3
		pSizeProp->AddSubItem(new CRSRectProp(pBase, _TB("Height mm"),  _TB("Height"), CRSRectProp::PropertyType::HeightM));
	//add to the parent group
	m_pLayoutGroup->AddSubItem(pSizeProp);
	pSizeProp->Expand(FALSE);

	//layer - for Z order filter in design mode
	CRSIntProp* pLayerProp = new CRSIntProp(pBase, _TB("Layer"), &(pBase->m_nLayer), 
_TB("It is a filter of visibility  to use during design mode to show different group of overlapped objects, only one group at a time"));
	m_pLayoutGroup->AddSubItem(pLayerProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFieldProperties(CNodeTree* pNode)
{
	FieldRect* pFieldR = dynamic_cast<FieldRect*>(pNode->m_pItemData);
	if (!pFieldR)
	{
		ASSERT(FALSE);
		return;
	}

	CString title = /*wrmField->GetSourceDescription() + L" - " +*/ pFieldR->GetFieldName();
	SetPanelTitle(title);

	CWoormDocMng* wrmDocMng = this->GetDocument();
	WoormField* wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pFieldR->GetInternalID());
	if (wrmField)
	{
		//Report Template
		ASSERT_VALID(wrmField);

		//Fieldname
		CrsProp* nameProp = new CrsProp(_TB("Name"), (_variant_t)wrmField->GetName(), _TB("Variable Name"));
		nameProp->AllowEdit(FALSE); /*nameProp->Enable(FALSE);*/ nameProp->SetColoredState(CrsProp::State::Important);
		m_pPropGrid->AddProperty(nameProp, FALSE, FALSE);

		//Field type description
		CrsProp* typeProp = new CrsProp(_TB("Type"), (_variant_t)wrmField->GetSourceDescription(), _TB("Variable source type"));
		typeProp->AllowEdit(FALSE); /*typeProp->Enable(FALSE);*/ typeProp->SetColoredState(CrsProp::State::Important);
		//SetImageVarType(wrmField, FALSE, typeProp);
		m_pPropGrid->AddProperty(typeProp, FALSE, FALSE);

		m_eShowVariableTypeBtn = wrmField->GetSourceEnum();
		m_bShowFindRuleBtn = wrmField->m_bIsTableField || wrmField->IsExprRuleField();
		m_bShowRequestFieldBtn = wrmField->IsAsk();
	}

	//Id
	CrsProp* IdProp = new CrsProp(_T("Id"), pFieldR->GetInternalID(), _TB("The internal Id of the current object"));
	IdProp->AllowEdit(FALSE); //IdProp->Enable(FALSE);
	m_pPropGrid->AddProperty(IdProp,FALSE, FALSE);

	LoadBaseProperties(pNode);

	LoadFieldAppearenceProperties(pFieldR);

	LoadFieldBehaviorProperties(pFieldR);

	//show layout � gi� visibile di default per tutti gli oggetti di layout e checckato
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFieldAppearenceProperties(FieldRect* pField)
{
	CList<CrsProp*>* depList = new CList<CrsProp*>();

	//Value Group
	CrsProp* pValueGroup = new CrsProp(_TB("Value"), 0, FALSE);

		//Value
		CRSValueProp* pValue = new CRSValueProp(pField, _TB("Preview Value"), &(pField->m_Value), _TB("!Only for Preview! The value of this property is temporary and will not be saved."));
		pValueGroup->AddSubItem(pValue);
		depList->AddTail(pValue);
		//TextColor
		pValueGroup->AddSubItem(new CRSColorWithExprProp(pField, _TB("Text Color"), &(pField->m_Value.m_rgbTextColor), &(pField->m_pTextColorExpr), _TB("Value Text Color"), this));
		//Alignment type
		//pValueGroup->AddSubItem(new CRSDialogProp(pField, _TB("Alignment"), 0, CRSDialogProp::PropertyType::FielRectValueAlign, _TB("Open the dialog associated to this property")));
		//Simil BitWise Alignment prop
		pValueGroup->AddSubItem(new CRSAlignBitwiseProp(this, pField, _TB("Alignment"), &(pField->m_Value.m_nAlign), TRUE, TRUE, FALSE));
		//FontStyle
		pValueGroup->AddSubItem(new  CRSSetFontDlgProp(pField, _TB("FontStyle"), CRSSetFontDlgProp::PropertyType::FieldRectValue));
		//Format
		CWoormDocMng* wrmDocMng = this->GetDocument();
		ASSERT_VALID(wrmDocMng);
		if (wrmDocMng)
			pValueGroup->AddSubItem(new CRSDialogWithExprProp(pField, _TB("FormatStyle"), &(pField->m_pFormatExpr), DataType::String, &(wrmDocMng->m_ViewSymbolTable), CRSDialogWithExprProp::PropertyType::FieldValueFormat, _TB("Left button: Open Dialog \nRight button: Open Expression Editor"), this));

		DataType colType = pField->GetDataType();
		if (colType == DataType::String || colType == DataType::Text)
		{
			pValueGroup->AddSubItem(new CRSBoolProp(pField, _TB("Mini html"), &(pField->m_bMiniHtml), _TB("Mini html ...")));
		}

	//Label Group
	CrsProp* pLabelGroup = new CrsProp(_TB("Label"), 0, FALSE);
	depList->AddTail(pLabelGroup);

	//Description *with expression*
	CRSStringWithExprProp* descr = new CRSStringWithExprProp(pField, _TB("Description"), &(pField->m_Label.m_strText), &(pField->m_pLabelTextExpr), &(this->GetDocument()->m_ViewSymbolTable), _TB("Label Description"), this, TRUE);
	pLabelGroup->AddSubItem(descr);
	descr->SetColoredState(CrsProp::State::Important);
	//TextColor *with expression*
	pLabelGroup->AddSubItem(new CRSColorWithExprProp(pField, _TB("Text Color"), &(pField->m_Label.m_rgbTextColor), &(pField->m_pLabelTextColorExpr), _TB("Label Text Color"), this));
	//Alignment type
	//pLabelGroup->AddSubItem(new CRSDialogProp(pField, _TB("Alignment"), 0, CRSDialogProp::PropertyType::FieldRectLabelAlign, _TB("Open the dialog associated to this property")));
	//Simil BitWise Alignment prop
	pLabelGroup->AddSubItem(new CRSAlignBitwiseProp(this, pField, _TB("Alignment"), &(pField->m_Label.m_nAlign), TRUE, FALSE, TRUE));
	//FontStyle
	pLabelGroup->AddSubItem(new CRSSetFontDlgProp(pField, _TB("FontStyle"), CRSSetFontDlgProp::PropertyType::FieldRectLabel));

	//Type
	CRSFieldTypeProp* pFieldTypeProp = new CRSFieldTypeProp(pField, _TB("Show As"), _TB("Type of the field rect"), this, m_pAppearenceGroup, depList);
	m_pAppearenceGroup->AddSubItem(pFieldTypeProp);

	pFieldTypeProp->DrawProperties();

	//aggiungo i gruppi label e Value dopo la combo del tipo perch� sta meglio, in quanto, in base al tipo rimuovo le properties
	m_pAppearenceGroup->AddSubItem(pValueGroup);
	//le salvo in una lista per poterle ridisegnare al posto giusto ad ogni modifica del 'showAs'
	pFieldTypeProp->m_pPropToRedraw->AddTail(pValueGroup);
	m_pAppearenceGroup->AddSubItem(pLabelGroup);
	pFieldTypeProp->m_pPropToRedraw->AddTail(pLabelGroup);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFieldBehaviorProperties(FieldRect* pField)
{
	ASSERT_VALID(pField->m_pDocument);
	if (pField->m_pDocument->m_pFormatStyles->GetDataType(pField->m_nFormatIdx) == DATA_STR_TYPE)
		//Email Parameter
		m_pBehaviorGroup->AddSubItem(new CRSEmailParameterProp(_TB("EmailParameter"), pField, _TB("Email Parameter Description"), m_pPropGrid));
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTextProperties(CNodeTree* pNode)
{
	TextRect* pText = dynamic_cast<TextRect*>(pNode->m_pItemData);
	if (!pText)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Text");

	LoadBaseProperties(pNode);

	LoadTextAppearenceProperties(pText);

	LoadTextBehaviorProperties(pText);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTextAppearenceProperties(TextRect* pText)
{
	//Text
	CRSEditDescriptionText* pEditText = new CRSEditDescriptionText(_TB("Text"), pText, this);
	pEditText->SetColoredState(CrsProp::State::Important);
	m_pAppearenceGroup->AddSubItem(pEditText);

	//Text Color property
	m_pAppearenceGroup->AddSubItem(new CRSColorWithExprProp(pText, _TB("Text Color"), &(pText->m_StaticText.m_rgbTextColor), &(pText->m_pTextColorExpr), _TB("The text color of this component"), this));

	//FontStyle
	m_pAppearenceGroup->AddSubItem(new CRSSetFontDlgProp(pText, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::TextRectDescription));

	//Simil BitWise Alignment prop
	m_pAppearenceGroup->AddSubItem(new CRSAlignBitwiseProp(this, pText, _TB("Alignment"), &(pText->m_StaticText.m_nAlign), TRUE, FALSE, TRUE));

	m_pAppearenceGroup->AddSubItem(new CRSBoolProp(pText, _TB("Mini html"), &(pText->m_bMiniHtml), _TB("Mini html ...")));
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTextBehaviorProperties(TextRect* pText)
{
	CrsProp* pProp = new CrsProp(_T("Is SpecialField"),
		pText->m_bSpecialField == 1 ? (_variant_t)_TB("True") : (_variant_t)_TB("False")
		);
	pProp->Enable(FALSE);
	m_pBehaviorGroup->AddSubItem(pProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFileProperties(CNodeTree* pNode)
{
	FileRect* pFile = dynamic_cast<FileRect*>(pNode->m_pItemData);
	if (!pFile)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Text File");

	LoadBaseProperties(pNode);

	LoadFileAppearenceProperties(pFile);

	LoadFileBehaviorProperties(pFile);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFileAppearenceProperties(FileRect* pFile)
{
	//Text Color property
	m_pAppearenceGroup->AddSubItem(new CRSColorWithExprProp(pFile, _TB("Text Color"), &(pFile->m_StaticText.m_rgbTextColor), &(pFile->m_pTextColorExpr), _TB("The text color of this component"), this));

	//FontStyle
	m_pAppearenceGroup->AddSubItem(new CRSSetFontDlgProp(pFile, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::FileRectText));

	//Alignment
	//m_pAppearenceGroup->AddSubItem(new CRSDialogProp(pFile, _TB("Alignment"), (int)pFile->m_StaticText.GetAlign(),  CRSDialogProp::PropertyType::TextRectAlign));

	//Simil BitWise Alignment prop
	m_pAppearenceGroup->AddSubItem(new CRSAlignBitwiseProp(this, pFile, _TB("Alignment"), &(pFile->m_StaticText.m_nAlign), TRUE, FALSE, FALSE));

	//FileName
	m_pAppearenceGroup->AddSubItem(new CRSSearchTbDialogProp(pFile, &(pFile->m_strFileName), CRSSearchTbDialogProp::PropertyType::FileRectFileName, CRSFileNameProp::Filter::Txt,this));
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadFileBehaviorProperties(FileRect* pFile)
{
	CrsProp* pProp = new CrsProp(_TB("IsRtf"),
		(pFile->m_bIsRtf == 1 ? (_variant_t)_TB("True") : (_variant_t)_TB("False"))
		);
	pProp->Enable(FALSE);
	m_pBehaviorGroup->AddSubItem(pProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadGraphProperties(CNodeTree* pNode)
{
	GraphRect* pGraph = dynamic_cast<GraphRect*>(pNode->m_pItemData);
	if (!pGraph)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Image");

	LoadBaseProperties(pNode);

	LoadGraphAppearenceProperties(pGraph);

	LoadGraphBehaviorProperties(pGraph);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadGraphAppearenceProperties(GraphRect* pGraph)
{
	//tb search file dialog
	m_pAppearenceGroup->AddSubItem(new CRSSearchTbDialogProp(pGraph, &(pGraph->m_sImage), CRSSearchTbDialogProp::PropertyType::GraphRectImgName, CRSFileNameProp::Filter::Img, this));

	//ImageFitMode
	m_pAppearenceGroup->AddSubItem(new CRSImageFitProp(pGraph, _TB("Image Fit Mode"), &(pGraph->m_Bitmap.m_ImageFitMode), _TB("Set image fit mode for this image")));

	//Simil BitWise Alignment prop
	m_pAppearenceGroup->AddSubItem(new CRSAlignBitwiseProp(this, pGraph, _TB("Alignment"), &(pGraph->m_nAlign), FALSE, FALSE, FALSE, 0, NULL, FALSE, FALSE, FALSE, FALSE));

	if (pGraph->m_bIsCutted)
	{//---cutted location group
		CrsProp* pLocationProp = new CrsProp(_TB("Cropped Location"), 0, TRUE);
			CPoint coordinatePoint = pGraph->m_rectCutted.TopLeft();
			//x	px
			pLocationProp->AddSubItem(new CRSRectProp(pGraph, _TB("X px"),  _TB("X"), CRSRectProp::PropertyType::LocationXP));
			//y	px
			pLocationProp->AddSubItem(new CRSRectProp(pGraph, _TB("Y px"),  _TB("Y"), CRSRectProp::PropertyType::LocationYP));
			//x	mm
			pLocationProp->AddSubItem(new CRSRectProp(pGraph, _TB("X mm"),  _TB("X"), CRSRectProp::PropertyType::LocationXM));
			//y	mm
			pLocationProp->AddSubItem(new CRSRectProp(pGraph, _TB("Y mm"), _TB("Y"), CRSRectProp::PropertyType::LocationYM));
		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pLocationProp);

		//---cutted size group
		CrsProp* pSizeProp = new CrsProp(_TB("Cropped Size"), 0, TRUE);
			//width	px
			pSizeProp->AddSubItem(new CRSRectProp(pGraph, _TB("Width px"), _TB("Width"), CRSRectProp::PropertyType::WidthP));
			//height px
			pSizeProp->AddSubItem(new CRSRectProp(pGraph, _TB("Height px"), _TB("Height"), CRSRectProp::PropertyType::HeightP));
			//width	mm
			pSizeProp->AddSubItem(new CRSRectProp(pGraph, _TB("Width mm"), _TB("Width"), CRSRectProp::PropertyType::WidthM));
			//height mm
			pSizeProp->AddSubItem(new CRSRectProp(pGraph, _TB("Height mm"), _TB("Height"), CRSRectProp::PropertyType::HeightM));
		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pSizeProp);
	}
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadGraphBehaviorProperties(GraphRect* pGraph)
{
	CStringList lstCommands;

	//subtotal definition
	lstCommands.AddTail(_TB("Copy attributes from"));

	//Resize to mantain original Aspeect Ratio
	lstCommands.AddTail(_TB("Resize Keeping original Aspect Ratio"));

	//Crop Image/Show all - Cut Image/Undo
	lstCommands.AddTail(_TB("Crop Image/Show all"));

	//Original Size
	lstCommands.AddTail(_TB("Original Size"));

	//Refresh Image
	lstCommands.AddTail(_TB("Refresh Image"));

	m_pPropGrid->SetCommands(lstCommands);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadSqrProperties(CNodeTree* pNode)
{
	SqrRect* pSqr = dynamic_cast<SqrRect*>(pNode->m_pItemData);
	if (!pSqr)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Rectangle");

	LoadBaseProperties(pNode);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartProperties(CNodeTree* pNode)
{
	Chart* pChart = dynamic_cast<Chart*>(pNode->m_pItemData);
	if (!pChart)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pChart);

	SetPanelTitle(L"Chart");

	LoadChartAppearenceProperties(pChart);
	LoadChartBehaviorProperties(pChart);
	LoadChartLayoutProperties(pChart);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartAppearenceProperties(Chart* pChart)
{
	//--Chart properties
	CRSChartStringProp* pPropName = new CRSChartStringProp(pChart, _TB("Name"), &(pChart->m_sName), _TB("Chart name"), EnumChartObject::CHART);
	pPropName->AllowEdit(FALSE);
	m_pAppearenceGroup->AddSubItem(pPropName);
	m_pAppearenceGroup->AddSubItem(new CRSChartStringProp(pChart, _TB("Title"), &(pChart->m_sTitle), _TB("Chart title"), EnumChartObject::CHART));
	CRSChartTypeComboProp* typeProp = new CRSChartTypeComboProp(pChart, this, m_pAppearenceGroup);
	m_pAppearenceGroup->AddSubItem(typeProp);
	typeProp->DrawProperties();
	typeProp->SetColoredState(CrsProp::State::Mandatory);

	//Style
	m_pAppearenceGroup->AddSubItem(new CRSStyleProp(pChart, this));

	//BackGroundColor 
	m_pAppearenceGroup->AddSubItem(new CRSChartColorProp(pChart, m_pPropGrid, _TB("Background Color"), pChart->GetBkgColor(), _TB("The background color of chart"), EnumChartObject::CHART));
	//hidden legend
	m_pAppearenceGroup->AddSubItem(new CRSChartBoolProp(pChart, _TB("Legend"), &pChart->m_pLegend->m_bEnabled, _TB("Enable chart legend"), this, GetDocument()->GetSymTable(), EnumChartObject::LEGEND));
	
	//---shadow group
	CrsProp* pShadowGroup = new CrsProp(_TB("Shadow"), 0, FALSE);
	//color
	pShadowGroup->AddSubItem(new CRSColorProp(pChart, _TB("ShadowColor"), &(pChart->m_crDropShadowColor), _TB("This property specifies the color of the shadow of this control")));
	//size
	pShadowGroup->AddSubItem(new CRSIntProp(pChart, _TB("ShadowSize"), &(pChart->m_nDropShadowHeight), _TB("The size in pixels of the shadow around the control")));
	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pShadowGroup);

	//---border group
	CrsProp* pBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
	BorderPen borderPen = pChart->m_BorderPen;
	//color
	pBorderGroup->AddSubItem(new CRSColorProp(pChart, _TB("Color"), &(pChart->m_BorderPen.m_rgbColor), _TB("This property specifies the color of the border around the control")));
	//size
	pBorderGroup->AddSubItem(new CRSIntProp(pChart, _TB("Size"), &(pChart->m_BorderPen.m_nWidth), _TB("The size in pixels of the border around the control")));

	//---Side
	CrsProp* pSideGroup = new CrsProp(_TB("Sides"), 0, TRUE);
	pSideGroup->AllowEdit(FALSE);
	//left
	pSideGroup->AddSubItem(new CRSBoolProp(pChart, _TB("Left"), &(pChart->m_Borders.left), _TB("Left border")));
	//top
	pSideGroup->AddSubItem(new CRSBoolProp(pChart, _TB("Top"), &(pChart->m_Borders.top), _TB("Top border")));
	//right
	pSideGroup->AddSubItem(new CRSBoolProp(pChart, _TB("Right"), &(pChart->m_Borders.right), _TB("Right border")));
	//bottom
	pSideGroup->AddSubItem(new CRSBoolProp(pChart, _TB("Bottom"), &(pChart->m_Borders.bottom), _TB("Bottom border")));

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pBorderGroup);	
	m_pAppearenceGroup->AddSubItem(pSideGroup);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartBehaviorProperties(Chart* pChart)
{
	//Tooltip expression
	CWoormDocMng* wrmDocMng = this->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	//Hidden Prop
	m_pBehaviorGroup->AddSubItem(new CRSHiddenProp(pChart, _TB("Hidden"), &(pChart->m_pHideExpr), DataType::Bool, wrmDocMng->GetSymTable(), this, _TB("The expression associated to this property")));

	CStringList lstCommands;
	lstCommands.AddTail(_TB("Refresh Chart"));	
	m_pPropGrid->SetCommands(lstCommands);
	m_pPropGrid->SendMessage(WM_NCPAINT, DCX_WINDOW, 0L);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartLayoutProperties(Chart* pChart)
{	
	//---location group
	CrsProp* pLocationProp = new CrsProp(_TB("Location"), 0, FALSE);
	//x	 px	  prop 0
	pLocationProp->AddSubItem(new CRSRectProp(pChart, _TB("X px"), _TB("X"), CRSRectProp::PropertyType::LocationXP));
	//y	 px	  prop 1
	pLocationProp->AddSubItem(new CRSRectProp(pChart, _TB("Y px"), _TB("Y"), CRSRectProp::PropertyType::LocationYP));
	//x	 mm	  prop 2
	pLocationProp->AddSubItem(new CRSRectProp(pChart, _TB("X mm"), _TB("X"), CRSRectProp::PropertyType::LocationXM));
	//y	 mm	   prop 3
	pLocationProp->AddSubItem(new CRSRectProp(pChart, _TB("Y mm"), _TB("Y"), CRSRectProp::PropertyType::LocationYM));
	//add to the parent group
	m_pLayoutGroup->AddSubItem(pLocationProp);
	pLocationProp->Expand(FALSE);

	//---size group
	CrsProp* pSizeProp = new CrsProp(_TB("Size"), 0, FALSE);
	//width	  px prop 0
	pSizeProp->AddSubItem(new CRSRectProp(pChart, _TB("Width px"), _TB("Width"), CRSRectProp::PropertyType::WidthP));
	//height  px prop 1
	pSizeProp->AddSubItem(new CRSRectProp(pChart, _TB("Height px"), _TB("Height"), CRSRectProp::PropertyType::HeightP));
	//width	  mm prop 2
	pSizeProp->AddSubItem(new CRSRectProp(pChart, _TB("Width mm"), _TB("Width"), CRSRectProp::PropertyType::WidthM));
	//height  mm prop 3
	pSizeProp->AddSubItem(new CRSRectProp(pChart, _TB("Height mm"), _TB("Height"), CRSRectProp::PropertyType::HeightM));
	//add to the parent group
	m_pLayoutGroup->AddSubItem(pSizeProp);
	pSizeProp->Expand(FALSE);

	//layer - for Z order filter in design mode
	CRSIntProp* pLayerProp = new CRSIntProp(pChart, _TB("Layer"), &(pChart->m_nLayer),
		_TB("It is a filter of visibility  to use during design mode to show different group of overlapped objects, only one group at a time"));
	m_pLayoutGroup->AddSubItem(pLayerProp);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartCategoryPropertyGrid(CNodeTree* pNode)
{
	Chart::CCategories* pCategory = dynamic_cast<Chart::CCategories*>(pNode->m_pItemData);
	if (!pCategory)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pCategory);

	CrsProp* pPropGeneral = new CrsProp(_TB("Chart category"));

	SetPanelTitle(L"Chart category");

	pPropGeneral->AddSubItem(new CRSChartStringProp(pCategory, _TB("Title"), &(pCategory->m_sTitle), _TB("Category title"), EnumChartObject::CATEGORY));

	WoormField* wrmField = (WoormField*)pCategory->m_pBindedField;

	CRSChartFieldComboProp* pComboFiledProp = new CRSChartFieldComboProp(pCategory, pCategory->GetParent(), this,  GetDocument()->GetSymTable(), EnumChartObject::CATEGORY);
	pComboFiledProp->SetOwnerList(m_pPropGrid);
	pPropGeneral->AddSubItem(pComboFiledProp);
	m_pPropGrid->AddProperty(pPropGeneral);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadChartSeriesPropertyGrid(CNodeTree* pNode)
{
	Chart::CSeries* pSeries = dynamic_cast<Chart::CSeries*>(pNode->m_pItemData);
	if (!pSeries)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pSeries);

	SetPanelTitle(L"Chart Series");

	ASSERT_VALID(pNode);
	ASSERT_VALID(m_pPropGrid);

	// Add Appearence properties----------------------------------------------
	m_pAppearenceGroup = new CrsProp(_TB("Appearence"));

	CrsProp* pDSGroupProp = new CrsProp(_TB("Series datasources"));
	
	m_pAppearenceGroup->AddSubItem(new CRSChartStringProp(pSeries, _TB("Title"), &(pSeries->m_sTitle), _TB("Series title"), EnumChartObject::SERIES));
	CRSChartTypeComboProp* typeProp = new CRSChartTypeComboProp(pSeries, this, GetDocument()->GetSymTable(), pDSGroupProp);
	m_pAppearenceGroup->AddSubItem(typeProp);
	typeProp->Enable(pSeries->m_pParent && pSeries->m_pParent->AllowSeriesWithDiffType());
	//typeProp->DrawProperties(); per le serie viene chiamato nel costruttore
	m_pAppearenceGroup->AddSubItem(pDSGroupProp);
 
	pSeries->m_sTrasparency.Format(L"%.1lf", pSeries->m_dTrasparency);

	m_pAppearenceGroup->AddSubItem(new CRSChartStringProp(pSeries, _TB("Trasparency"), &pSeries->m_sTrasparency, _TB("Trasparency of series"), EnumChartObject::TRASPARENCY));
	m_pAppearenceGroup->AddSubItem(new CRSChartBoolProp(pSeries, _TB("Labels"), &pSeries->m_bShowLabels, _TB("Show labels"), this, GetDocument()->GetSymTable(), EnumChartObject::LABEL));
	m_pPropGrid->AddProperty(m_pAppearenceGroup);


	// Add Behavior properties------------------------------------------------
	m_pBehaviorGroup = new CrsProp(_TB("Behavior"));
	m_pBehaviorGroup->AddSubItem(new CRSBoolProp(this, _TB("Hidden"), &m_bIsHidden));
	m_pPropGrid->AddProperty(m_pBehaviorGroup);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTableProperties(CNodeTree* pNode)
{
	Table* pTable = dynamic_cast<Table*>(pNode->m_pItemData);
	if (!pTable)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Table");

	//Id
	CrsProp* IdProp = new CrsProp(_T("Id"), pTable->GetInternalID(), _TB("The internal Id of the current object"));
	IdProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(IdProp, FALSE, FALSE);

	//Name
	CrsProp* NameProp = new CrsProp(_T("Name"), (_variant_t)pTable->GetName(), _TB("The name of the current object"));
	NameProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(NameProp, FALSE, FALSE);

	LoadTableAppearenceProperties(pTable);

	LoadTableBehaviorProperties(pTable);

	LoadTableLayoutProperties(pTable);

	CStringList lstCommands;
	lstCommands.AddTail(_TB("Clear all custom styles"));
	
	//� stato sce3gliere di non mostrare il command seguente perch�, nonostante fosse funzionante, era poco chiaro il significato di questo comando e sembrava pi� superfluo che utile
	//lstCommands.AddTail(_TB("Clear table custom styles"));
	
	m_pPropGrid->SetCommands(lstCommands);
	//TAPULLO by silvano :-P e Andrea per risolvere il problema del mancato disegno dei comandi sotto la property grid
	m_pPropGrid->SendMessage(WM_NCPAINT, DCX_WINDOW, 0L); 
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTableAppearenceProperties(Table* pTable)
{
	// GENERAL---------------------------------------------------------------------------------------------------------GENERAL
	{
		//Style
		m_pAppearenceGroup->AddSubItem(new CRSStyleProp(pTable, this));

		//transparent
		m_pAppearenceGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Transparent BackGround"), &(pTable->m_bTransparent), _TB("Determines whether the table background must be transparent")));

		//alternate mode
		m_pAppearenceGroup->AddSubItem(new CRSAlternateColorProp(pTable));

		//---shadow group
		CrsProp* pShadowProp = new CrsProp(_TB("Shadow"), 0, FALSE);

			//color
			pShadowProp->AddSubItem(new CRSColorProp(pTable, _TB("Color"), &(pTable->m_crDropShadowColor), _TB("This property specifies the color of the shadow of this table")));
			//size
			pShadowProp->AddSubItem(new CRSTableShadowProp(pTable));

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pShadowProp);
		pShadowProp->Expand(FALSE);
	}
	// TITLE-----------------------------------------------------------------------------------------------------------TITLE
	{
			CrsProp* pTitleGroup = new CrsProp(_TB("Title"), 0, FALSE);

			//hide table title
			pTitleGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Hide title"), &(pTable->m_bHideTableTitle), _TB("Determines whether the table's title should be visible")));
		
			//title text
			pTitleGroup->AddSubItem(new CRSTableTitlTextProp(pTable));

			//BackGround Color 
			pTitleGroup->AddSubItem(new CRSColorProp(pTable, _TB("Background Color"), &(pTable->m_Title.m_rgbBkgColor), _TB("The backGround color of the table title")));

			//Text Color
			pTitleGroup->AddSubItem(new CRSColorProp(pTable, _TB("Text Color"), &(pTable->m_Title.m_rgbTextColor), _TB("The text Color of the table title")));

			//alignment bitwise
			pTitleGroup->AddSubItem(new CRSAlignBitwiseProp(this, pTable, _TB("Alignment"), &pTable->m_Title.m_nAlign, FALSE, FALSE, FALSE));

			//FontStyle
			pTitleGroup->AddSubItem(new CRSSetFontDlgProp(pTable, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::TableTitle));

			//table title height
			CrsProp* heightProp = new CrsProp(_TB("Height"));
			heightProp->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::TableTitle));
			heightProp->AddSubItem(new CRSTableHeightsPropMM(pTable, CRSTableHeightsPropMM::PropertyType::TableTitle));
			pTitleGroup->AddSubItem(heightProp);

			//---Table Titles border
			CrsProp* pTableTitlesBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
				//color
				pTableTitlesBorderGroup->AddSubItem(new CRSColorProp(pTable, _TB("Color"), &(pTable->m_TitlePen.m_rgbColor), _TB("Border color table title")));

				//size
				pTableTitlesBorderGroup->AddSubItem(new CRSIntProp(pTable, _TB("Size"), &(pTable->m_TitlePen.m_nWidth), _TB("Border size table title"), 0, 12));

				//Sides
				CrsProp* pTitleBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
				pTitleBorderSidesGroup->AllowEdit(FALSE);
					//left
					pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bTableTitleLeft), _TB("Left border of Table Titles")));
					//top
					pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bTableTitleTop), _TB("Top border of Table Titles")));
					//right
					pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bTableTitleRight), _TB("Right border of Table Titles")));
					//bottom
					pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bTableTitleBottom), _TB("Bottom border of Table Titles")));
				//add to the parent group
				pTableTitlesBorderGroup->AddSubItem(pTitleBorderSidesGroup);
				pTitleBorderSidesGroup->Expand(FALSE);

			//add to the parent group
			pTitleGroup->AddSubItem(pTableTitlesBorderGroup);
			pTableTitlesBorderGroup->Expand(FALSE);

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pTitleGroup);
		pTitleGroup->Expand(FALSE);
	}
	// ALL COLUMN TITLE----------------------------------------------------------------------------------------------------------ALL COLUMNS TITLE
	{
		CrsProp* pColumnGroup = new CrsProp(_TB("All Columns title"), 0, FALSE);

		//BackColor
		pColumnGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::ColumnTitleBackColor, this));

		//Text Color
		pColumnGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::ColumnTitleForeColor, this));

		//hide column title
		pColumnGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Hide title"), &(pTable->m_bHideColumnsTitle), _TB("Determines whether the columns title should be visible")));

		//columns height
		CrsProp* heightPropColGroup = new CrsProp(_TB("Height"));
		heightPropColGroup->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::ColumnTitle));
		heightPropColGroup->AddSubItem(new CRSTableHeightsPropMM(pTable, CRSTableHeightsPropMM::PropertyType::ColumnTitle));
		pColumnGroup->AddSubItem(heightPropColGroup);

		//separator
		pColumnGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Titles Separator"), &(pTable->m_Borders.m_bColumnTitleSeparator), _TB("Separator border of Column Titles")));
		
		//alignment
		//pColumnGroup->AddSubItem(new CRSDialogProp(pTable, _TB("Alignment Type"), 0, CRSDialogProp::PropertyType::AllColumnTitlesAlign, _TB("Set all columns alignment")));

		//alignment bitwise
		pColumnGroup->AddSubItem(new CRSAllColumnsAlignmentStyleBitWiseProp(this, pTable, CRSAllColumnsAlignmentStyleBitWiseProp::PropertyType::ColumnTitles));

		//FontStyle
		pColumnGroup->AddSubItem(new CRSSetFontDlgProp(pTable, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::AllColumnTitles));

		//---Column Titles
		CrsProp* pColumnTitlesBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
			
			//Border color
			pColumnTitlesBorderGroup->AddSubItem(new CRSTableAllColumnsColorProp(pTable, CRSTableAllColumnsColorProp::ColumnTitleBorderColor));

			//Size
			pColumnTitlesBorderGroup->AddSubItem(new CRSTableAllColumnsBorderSizeProp(pTable, CRSTableAllColumnsBorderSizeProp::ColumnTitle));

			//Sides
			CrsProp* pColumnBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
			pColumnBorderSidesGroup->AllowEdit(FALSE);
				//left
				pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bColumnTitleLeft), _TB("Left border of Column Titles")));
				//top
				pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bColumnTitleTop), _TB("Top border of Column Titles")));
				//right
				pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bColumnTitleRight), _TB("Right border of Column Titles")));
				//bottom
				pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bColumnTitleBottom), _TB("Bottom border of Column Titles")));
			//add to the parent group
			pColumnTitlesBorderGroup->AddSubItem(pColumnBorderSidesGroup);
			pColumnBorderSidesGroup->Expand(FALSE);

		//add to the parent group
		pColumnGroup->AddSubItem(pColumnTitlesBorderGroup);
		pColumnTitlesBorderGroup->Expand(FALSE);

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pColumnGroup);
		pColumnGroup->Expand(FALSE);
	}
	// ALL BODY------------------------------------------------------------------------------------------------------------ALL BODY
	{
		CrsProp* pBodyGroup = new CrsProp(_TB("All Body"), 0, FALSE);

		//Back Color
		pBodyGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::BodyBackColor, this));

		//Text Color
		pBodyGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::BodyForeColor, this));

		//rows height
		CrsProp* heightPropRow = new CrsProp(_TB("Height"));
		heightPropRow->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::Row));
		heightPropRow->AddSubItem(new CRSTableHeightsPropMM(pTable, CRSTableHeightsPropMM::PropertyType::Row));
		pBodyGroup->AddSubItem(heightPropRow);

		//separator
		pBodyGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Column Separator"), &(pTable->m_Borders.m_bColumnSeparator), _TB("Separator border of Body")));

		//separator mode (propriet� ternaria)
		pBodyGroup->AddSubItem(new CRSTableRowSeparatorProp(pTable));

		//row separator color (easyview)
		pBodyGroup->AddSubItem(new CRSColorProp(pTable, _TB("Easy view color"), &(pTable->m_crAlternateBkgColorOnRow), _TB("Alternate BackGround color")));

		//alignment bitwise
		pBodyGroup->AddSubItem(new CRSAllColumnsAlignmentStyleBitWiseProp(this, pTable, CRSAllColumnsAlignmentStyleBitWiseProp::PropertyType::Body));

		//FontStyle
		pBodyGroup->AddSubItem(new CRSSetFontDlgProp(pTable, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::AllColumnsBody));

		CrsProp* pBodyBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);

			//if (pTable->m_Borders.m_pRowSepPen)
			//{
			//	//Row Separator color
			//	pBodyBorderGroup->AddSubItem(new CRSColorProp(pTable, _TB("Row Separator Color"), &(pTable->m_Borders.m_pRowSepPen->m_rgbColor), _TB("Border Color of row separators")));
			//	//Row Separator width
			//	pBodyBorderGroup->AddSubItem(new CRSIntProp(pTable, _TB("Row Separator Size"), &(pTable->m_Borders.m_pRowSepPen->m_nWidth), _TB("Border Size of row separators")));
			//}

			//Color
			pBodyBorderGroup->AddSubItem(new CRSTableAllColumnsColorProp(pTable, CRSTableAllColumnsColorProp::PropertyType::BodyBorderColor));

			//Size
			pBodyBorderGroup->AddSubItem(new CRSTableAllColumnsBorderSizeProp(pTable, CRSTableAllColumnsBorderSizeProp::Body));

			//Sides
			CrsProp* pBodyBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
			pBodyBorderSidesGroup->AllowEdit(FALSE);
				//left
				pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bBodyLeft), _TB("Left border of Body")));
				//top
				pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bBodyTop), _TB("Top border of Body")));
				//right
				pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bBodyRight), _TB("Right border of Body")));
				//bottom
				pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bBodyBottom), _TB("Bottom border of Body")));
			//add to the parent group
			pBodyBorderGroup->AddSubItem(pBodyBorderSidesGroup);
			pBodyBorderSidesGroup->Expand(FALSE);

		//add to the parent group
		pBodyGroup->AddSubItem(pBodyBorderGroup);
		pBodyBorderGroup->Expand(FALSE);

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pBodyGroup);
		pBodyGroup->Expand(FALSE);
	}
	// SUBTITLE-----------------------------------------------------------------------------------------------------------SUBTITLE
	{
		CrsProp* pSubTitleGroup = new CrsProp(_TB("Sub Title"), 0, FALSE);

		//BackGround Color 
		pSubTitleGroup->AddSubItem(new CRSColorProp(pTable, _TB("Background Color"), &(pTable->m_SubTitle.m_rgbBkgColor), _TB("The backGround color of the table sub title")));

		//Text Color
		pSubTitleGroup->AddSubItem(new CRSColorProp(pTable, _TB("Text Color"), &(pTable->m_SubTitle.m_rgbTextColor), _TB("The text Color of the table sub title")));

		//alignment bitwise
		pSubTitleGroup->AddSubItem(new CRSAlignBitwiseProp(this, pTable, _TB("Alignment"), &pTable->m_SubTitle.m_nAlign, FALSE, FALSE, FALSE));

		//pSubTitleGroup
		pSubTitleGroup->AddSubItem(new CRSSetFontDlgProp(pTable, _TB("Font Style"), CRSSetFontDlgProp::PropertyType::TableSubTitle));

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pSubTitleGroup);
		pSubTitleGroup->Expand(FALSE);
	}
	// SUBTOTAL-----------------------------------------------------------------------------------------------------------SUBTOTAL
	{
		CrsProp* pSubTotalGroup = new CrsProp(_TB("All SubTotal"), 0, FALSE);
		//Back Color
		pSubTotalGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::SubTotalBackColor, this));
		//Text Color
		pSubTotalGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::SubTotalForeColor, this));

		//font style
		pSubTotalGroup->AddSubItem(new CRSSetFontDlgProp(pTable, _TB("Font style"), CRSSetFontDlgProp::PropertyType::AllSubTotals));

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pSubTotalGroup);
		pSubTotalGroup->Expand(FALSE);
	}
	// ALL TOTAL-----------------------------------------------------------------------------------------------------------ALL TOTAL
	{
		CrsProp* pTotalGroup = new CrsProp(_TB("All Total"), 0, FALSE);

		//BackColor
		pTotalGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::TotalBackColor, this));
		//Text Color
		pTotalGroup->AddSubItem(new CRSTableAllColumnsColorWithExprProp(pTable, CRSTableAllColumnsColorWithExprProp::TotalForeColor, this));

		//total height
		CrsProp* heightPropTotal = new CrsProp(_TB("Height"));
		heightPropTotal->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::Total));
		heightPropTotal->AddSubItem(new CRSTableHeightsPropMM(pTable, CRSTableHeightsPropMM::PropertyType::Total));
		pTotalGroup->AddSubItem(heightPropTotal);

		//alignment bitwise
		pTotalGroup->AddSubItem(new CRSAllColumnsAlignmentStyleBitWiseProp(this, pTable, CRSAllColumnsAlignmentStyleBitWiseProp::PropertyType::Totals));

			//---Total
		CrsProp* pTotalBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);

		//Border color
		pTotalBorderGroup->AddSubItem(new CRSTableAllColumnsColorProp(pTable, CRSTableAllColumnsColorProp::TotalBorderColor));

		//Size
		pTotalBorderGroup->AddSubItem(new CRSTableAllColumnsBorderSizeProp(pTable, CRSTableAllColumnsBorderSizeProp::Total));

		//---Sides
		CrsProp* pTotalTitlesBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
		pTotalTitlesBorderSidesGroup->AllowEdit(FALSE);
			//left
			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bTotalLeft), _TB("Left border of Total")));
			//top
			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bTotalTop), _TB("Top border of Total")));
			//right
			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bTotalRight), _TB("Right border of Total")));
			//bottom
			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bTotalBottom), _TB("Bottom border of Total")));
		//add to the parent group
		pTotalBorderGroup->AddSubItem(pTotalTitlesBorderSidesGroup);
		pTotalTitlesBorderSidesGroup->Expand(FALSE);	

		//add to the parent group
		pTotalGroup->AddSubItem(pTotalBorderGroup);
		pTotalBorderGroup->Expand(FALSE);

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pTotalGroup);
		pTotalGroup->Expand(FALSE);
	}
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTableBehaviorProperties(Table* pTable)
{
	//hidden
	CWoormDocMng* wrmDocMng = this->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (wrmDocMng)
		m_pBehaviorGroup->AddSubItem(new CRSExpressionProp(_TB("Hidden"), &(pTable->m_pHideExpr), DataType::Bool, &(wrmDocMng->m_ViewSymbolTable), this, _TB("The expression associated to this property")));

	//end of fiscal year
	m_pBehaviorGroup->AddSubItem(new CRSBoolProp(pTable, _TB("End of fiscal year"), &(pTable->m_bFiscalEnd), _TB("Enables painting of a z at the end of the table")));
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTableLayoutProperties(Table* pTable)
{
	//size
	m_pLayoutGroup->AddSubItem(new CRSRowCountProp(pTable));
	//---location group
	CrsProp* pLocationProp = new CrsProp(_TB("Location"), 0, TRUE);
		//x	px
		pLocationProp->AddSubItem(new CRSRectProp(pTable, _TB("X px"),  _TB("X"), CRSRectProp::PropertyType::LocationXP));
		//y	px
		pLocationProp->AddSubItem(new CRSRectProp(pTable, _TB("Y px"),  _TB("Y"), CRSRectProp::PropertyType::LocationYP));
		//x	mm
		pLocationProp->AddSubItem(new CRSRectProp(pTable, _TB("X mm"),  _TB("X"), CRSRectProp::PropertyType::LocationXM));
		//y	mm
		pLocationProp->AddSubItem(new CRSRectProp(pTable, _TB("Y mm"), _TB("Y"), CRSRectProp::PropertyType::LocationYM));
	//add to the parent group
	m_pLayoutGroup->AddSubItem(pLocationProp);

	//layer - for Z order filter in design mode
	CRSIntProp* pLayerProp = new CRSIntProp(pTable, _TB("Layer"), &(pTable->m_nLayer),
		_TB("It is a filter of visibility  to use during design mode to show different group of overlapped objects, only one group at a time"));
	m_pLayoutGroup->AddSubItem(pLayerProp);
}

// ----------------------------------------------------------------------------
CString	CRS_ObjectPropertyView::SetImageVarType(WoormField* pF, BOOL bIsColumn, CrsProp* prop)
{
	if (pF->IsTableRuleField())
	{
		prop->SetStateImg(CRS_PropertyGrid::Img::DBVar);
		return bIsColumn ? _TB("DB column") : _TB("DB field");
	}

	else if (pF->IsAsk())
	{
		prop->SetStateImg(CRS_PropertyGrid::Img::InputVar);
		return bIsColumn ? _TB("Request column") : _TB("Request field");
	}

	else if (pF->IsInput())
	{
		prop->SetStateImg(CRS_PropertyGrid::Img::DBVar);
		return  bIsColumn ? _TB("Input column") : _TB("Input field");
	}

	else if (pF->IsExprRuleField())
	{
		prop->SetStateImg(CRS_PropertyGrid::Img::ExprVar);
		return bIsColumn ? _TB("Expression column") : _TB("Expression field");
	}

	else
	{
		prop->SetStateImg(CRS_PropertyGrid::Img::FuncVar);
		return bIsColumn ? _TB("Function column") : _TB("Function field");
	}
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadColumnProperties(CNodeTree* pNode)
{
	TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
	if (!pCol)
	{
		ASSERT(FALSE);
		return;
	}

	CString title = /*wrmField->GetSourceDescription(TRUE) + L" - " +*/ pCol->GetFieldName();
	SetPanelTitle(title);

	CWoormDocMng* wrmDocMng = this->GetDocument();
	WoormField* wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(pCol->GetInternalID());
	if (wrmField)
	{
		ASSERT_VALID(wrmField);

		//Fieldname
		CrsProp* nameProp = new CrsProp(_TB("Name"), (_variant_t)wrmField->GetName(), _TB("Variable Name"));
		nameProp->AllowEdit(FALSE); /*nameProp->Enable(FALSE);*/ nameProp->SetColoredState(CrsProp::State::Important);
		m_pPropGrid->AddProperty(nameProp, FALSE, FALSE);

		//Field type description
		CrsProp* typeProp = new CrsProp(_TB("Type"), (_variant_t)wrmField->GetSourceDescription(), _TB("Variable source type"));
		typeProp->AllowEdit(FALSE); /*typeProp->Enable(FALSE);*/ typeProp->SetColoredState(CrsProp::State::Important);
		//SetImageVarType(wrmField, TRUE, typeProp);
		m_pPropGrid->AddProperty(typeProp, FALSE, FALSE);

		m_eShowVariableTypeBtn = wrmField->GetSourceEnum();
		m_bShowFindRuleBtn = wrmField->m_bIsTableField || wrmField->IsExprRuleField();
	}

	//Id
	CrsProp* IdProp = new CrsProp(_T("Id"), pCol->GetInternalID(), _TB("The internal Id of the current object"));
	IdProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(IdProp, FALSE, FALSE);

	LoadColumnAppearenceProperties(pCol);

	LoadColumnBehaviorProperties(pCol);

	LoadColumnLayoutProperties(pCol);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadColumnAppearenceProperties(TableColumn* pCol)
{
	CWoormDocMng* wrmDocMng = this->GetDocument();
	ASSERT_VALID(wrmDocMng);

	//Style
	m_pAppearenceGroup->AddSubItem(new CRSStyleProp(pCol, this));

	// TITLE-----------------------------------------------------------------------------------------------------------TITLE
	CrsProp* pTitleGroup = new CrsProp(_TB("Title"), 0, FALSE);
		//title text
		CRSStringWithExprProp* title = new CRSStringWithExprProp(pCol, _TB("Text"), &(pCol->m_Title.m_strText), &(pCol->m_pTitleExpr), &(this->GetDocument()->m_ViewSymbolTable), _TB("Column Title title"), this,TRUE);
		pTitleGroup->AddSubItem(title);
		title->SetColoredState(CrsProp::State::Important);
		//back Color
		pTitleGroup->AddSubItem(new CRSColorWithExprProp(pCol, _TB("Background Color"), &(pCol->m_Title.m_rgbBkgColor), &(pCol->m_pTitleBkgColorExpr), _TB("Background Color of column Title"), this));
		//Text Color
		pTitleGroup->AddSubItem(new CRSColorWithExprProp(pCol, _TB("Text Color"), &(pCol->m_Title.m_rgbTextColor), &(pCol->m_pTitleTextColorExpr), _TB("Text Color of column Title"), this));
		//alignment
		//pTitleGroup->AddSubItem(new CRSDialogProp(pCol, _TB("AlignmentType"), 0, CRSDialogProp::PropertyType::ColumnTitleAlign, _TB("Open the dialog associated to this property")));
		//Simil BitWise Alignment prop
		pTitleGroup->AddSubItem(new CRSAlignBitwiseProp(this, pCol, _TB("Alignment"), &(pCol->m_Title.m_nAlign), TRUE, FALSE, FALSE));
		//font style
		pTitleGroup->AddSubItem(new CRSSetFontDlgProp(pCol, _TB("Font style"), CRSSetFontDlgProp::PropertyType::ColumnTitle));
		//Tooltip
		pTitleGroup->AddSubItem(new CRSExpressionProp(_TB("Tooltip"), &(pCol->m_pTitleTooltipExpr), DataType::String, &(this->GetDocument()->m_ViewSymbolTable), this, _TB("The expression associated to the tooltip")));

		//Border
		CrsProp* pTitleBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
			//color
			pTitleBorderGroup->AddSubItem(new CRSColorProp(pCol, _TB("Color"), &(pCol->m_ColumnTitlePen.m_rgbColor), _TB("Border Color of Column Title")));
			//size
			pTitleBorderGroup->AddSubItem(new CRSIntProp(pCol, _TB("Size"), &(pCol->m_ColumnTitlePen.m_nWidth), _TB("Border size of Column Title"), 0, 12));
			////---Sides
			//CrsProp* pTitleBorderSidesGroup = new CrsProp(_TB("Sides"), 0, FALSE);
			//pTitleBorderSidesGroup->AllowEdit(FALSE);
			//	//left
			//	pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Left"), &(pCol->GetColumnTitlePen), _TB("Left border of Total")));
			//	//top
			//	pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Top"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalTop), _TB("Top border of Total")));
			//	//right
			//	pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Right"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalRight), _TB("Right border of Total")));
			//	//bottom
			//	pTitleBorderSidesGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Bottom"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalBottom), _TB("Bottom border of Total")));
			////add to the parent group
			//pTitleBorderGroup->AddSubItem(pTitleBorderSidesGroup);
		//add to the parent group
		pTitleGroup->AddSubItem(pTitleBorderGroup);
		pTitleBorderGroup->Expand(FALSE);

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pTitleGroup);
	pTitleGroup->Expand(TRUE);

	// BODY-----------------------------------------------------------------------------------------------------------BODY
	CrsProp* pBodyGroup = new CrsProp(_TB("Body"), 0, FALSE);
		//Back Color
		pBodyGroup->AddSubItem(new CRSTableColumnColorWithExprProp(pCol, CRSTableColumnColorWithExprProp::PropertyType::BackColor, &(pCol->m_pBkgColorExpr), this));

		//Text Color
		pBodyGroup->AddSubItem(new CRSTableColumnColorWithExprProp(pCol, CRSTableColumnColorWithExprProp::PropertyType::ForeColor, &(pCol->m_pTextColorExpr), this));

		//pBodyGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Preserve mixed cell aligments"), &(pCol->m_bMixedColumnAlign), _TB("Preserve mixed cell aligments")));
		//Simil BitWise Alignment prop
		CRSColumnAlignBitwiseProp* columnBodyAlignment = new CRSColumnAlignBitwiseProp(this, pCol, _TB("Alignment"), &(pCol->m_nAlignType), CRSColumnAlignBitwiseProp::PropertyType::Body);
		pBodyGroup->AddSubItem(columnBodyAlignment);

		//Type
		CRSColumnTypeProp* TypeProp = new CRSColumnTypeProp(pCol, this);

		//can also use ColumnCanBeMultiRow() on table
		DataType colType = pCol->GetDataType();
		if (colType == DataType::String || colType == DataType::Text)
		{
			//distribute over multiline if too long
			pBodyGroup->AddSubItem(new CRSMultilineColumnProp(pCol, pCol->IsMultiRow(), columnBodyAlignment, TypeProp));//passa anche il puntatore del tipo
		}

		if (wrmDocMng)
		{
			//font style
			pBodyGroup->AddSubItem(new CRSDialogWithExprProp(pCol, _TB("Font style"), &(pCol->m_pTextFontExpr), DataType::String, &(wrmDocMng->m_ViewSymbolTable), CRSDialogWithExprProp::PropertyType::TableColumnBodyFont, _TB("Open font style dialog or editor"), this));
			//format style
			pBodyGroup->AddSubItem(new CRSDialogWithExprProp(pCol, _TB("Format style"), &(pCol->m_pCellFormatterExpr), DataType::String, &(wrmDocMng->m_ViewSymbolTable), CRSDialogWithExprProp::PropertyType::TableColumnBodyFormat, _TB("Open format style dialog or editor"), this));
		}

		if (colType == DataType::String || colType == DataType::Text)
		{
			pBodyGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Mini html"), &(pCol->m_bMiniHtml), _TB("Mini html ...")));
		}

		if (wrmDocMng)
		{
			//tooltip expression
			pBodyGroup->AddSubItem(new CRSExpressionExtendedProp(pCol, _TB("Tooltip Expression"), &(pCol->m_pCellTooltipExpr), DataType::String, &(wrmDocMng->m_ViewSymbolTable), this, _TB("the expression associated to this property")));
		}

		//Border
		CrsProp* pBodyBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
			//side expression
			if (wrmDocMng)
				pBodyBorderGroup->AddSubItem(new CRSExpressionExtendedProp(pCol,_TB("Borders side"), &(pCol->m_pCellBordersExpr), 
						DataType::String, &(wrmDocMng->m_ViewSymbolTable), this, _TB("the expression associated to this property, should return a string like \"left,-bottom\"")));
			//color
			pBodyBorderGroup->AddSubItem(new CRSColorProp(pCol, _TB("Color"), &(pCol->m_ColumnPen.m_rgbColor), _TB("Border Color of Column Body")));
			//width
			pBodyBorderGroup->AddSubItem(new CRSIntProp(pCol, _TB("Size"), &(pCol->m_ColumnPen.m_nWidth), _TB("Border size of Column Body"), 0, 12));
		//add to the parent group
		pBodyGroup->AddSubItem(pBodyBorderGroup);
		pBodyBorderGroup->Expand(FALSE);

		//Type
		pBodyGroup->AddSubItem(TypeProp);
		// disegno le 'subproperties' qui e non nel costruttore perch� sono  allo stesso livello di profondit�
		// e in questo modo rimangono dopo e non prima la property 'Typeprop'
		TypeProp->DrawProperties();

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pBodyGroup);
	pBodyGroup->Expand(TRUE);

	// SUBTOTAL-----------------------------------------------------------------------------------------------------------SUBTOTAL
	CrsProp* pSubTotalGroup = new CrsProp(_TB("SubTotal"), 0, FALSE);
		//Back Color
		pSubTotalGroup->AddSubItem(new CRSColorWithExprProp(pCol, _TB("Background Color"), &(pCol->m_SubTotal.m_rgbBkgColor), &(pCol->m_SubTotal.m_pBkgColorExpr), _TB("Background Color of subtotals"), this));
		//Text Color
		pSubTotalGroup->AddSubItem(new CRSColorWithExprProp(pCol, _TB("Text Color"), &(pCol->m_SubTotal.m_rgbTextColor), &(pCol->m_SubTotal.m_pTextColorExpr), _TB("Text Color of subtotals"), this));
		
		//Alignment
		////TODO NON viene salvato nel wrm nella sezione Table-SubTotal (vedi riga 200 CELL.CPP)
		//pSubTotalGroup->AddSubItem(new CRSDialogProp(pCol, _TB("AlignmentType"), 0, CRSDialogProp::PropertyType::SubtotalAlign, _TB("Open the dialog associated to this property")));
		
		//font style
		pSubTotalGroup->AddSubItem(new CRSSetFontDlgProp(pCol, _TB("Font style"), CRSSetFontDlgProp::PropertyType::ColumnSubTotal));

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pSubTotalGroup);
	pSubTotalGroup->Expand(FALSE);

	// TOTAL-----------------------------------------------------------------------------------------------------------TOTAL
	CrsProp* pTotalGroup = new CrsProp(_TB("Total"), 0, FALSE);
		//show total
		pTotalGroup->AddSubItem(new CRSShowColumnTotalProp(pCol, this));

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pTotalGroup);
	pTotalGroup->Expand(FALSE);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadColumnBehaviorProperties(TableColumn* pCol)
{
	//AddBasicCommands();

	CStringList lstCommands;

	lstCommands.AddTail(_TB("Clear custom styles"));
	lstCommands.AddTail(_TB("Copy body attributes..."));
	lstCommands.AddTail(_TB("Copy title attributes..."));

	m_pPropGrid->SetCommands(lstCommands);
	
	//Hidden Prop
	m_pBehaviorGroup->AddSubItem(new CRSHiddenProp(pCol, _TB("Hidden"), &(pCol->m_pHideExpr), DataType::Bool, this->GetDocument()->GetSymTable(), this, _TB("The expression associated to this property")));

	//Merge Empty cells
	m_pBehaviorGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Merge empty cells"), &(pCol->m_bVMergeEmptyCell), _TB("Merge empty vertical cells")));

	//Merge Equal cells
	m_pBehaviorGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Merge equal cells"), &(pCol->m_bVMergeEqualCell), _TB("Merge equal vertical cells")));

	//Merge Breaked cells
	m_pBehaviorGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Merge breaked cells"), &(pCol->m_bVMergeTailCell), _TB("Merge breaked vertical cells")));

	if (pCol->m_bPinnedColumn || pCol->GetTable()->m_pDocument->GetTableCount() < 2)
	{
		// PRINT ATTRIBUTES-------------------------------------------------------------- PRINT ATTRIBUTES
		CrsProp* pPrintGroup = new CrsProp(_TB("Print Attributes"), 0, FALSE);

			//fixed column
			pPrintGroup->AddSubItem(new CRSBoolProp(pCol, _TB("Fixed Column"), &(pCol->m_bPinnedColumn), _TB("fixed column")));

			//splitter column
			pPrintGroup->AddSubItem(new CRSSplitterColumnProp(pCol));
		
		//add to the parent group
		m_pBehaviorGroup->AddSubItem(pPrintGroup);
		pPrintGroup->Expand(FALSE);
	}

	m_pBehaviorGroup->Expand(TRUE);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadColumnLayoutProperties(TableColumn* pCol)
{
	//width
	CRSColumnWidthWithExprProp* pPropColWidth = new CRSColumnWidthWithExprProp(pCol, _TB("Width px"), pCol->m_nWidth, &(pCol->m_pDynamicWidthExpr), this->GetDocument()->GetSymTable(), this, _TB("static or conditional width of this columns"), CRSExpressionProp::IntValue, CRSColumnWidthWithExprProp::WidthCoor::WidthP);
	m_pLayoutGroup->AddSubItem(pPropColWidth);

	//pPropColWidth->AddSubItem(
	//	new CRSIntProp(pCol, _TB("Static width"), &(pCol->m_nSavedWidth), _TB("Saved column width for hidden column")),
	//	m_pPropGrid);

	LONG colWidthM = (LONG)LPtoMU(pCol->m_nWidth, CM, 10., 3);
	m_pLayoutGroup->AddSubItem(new CRSColumnWidthWithExprProp(pCol, _TB("Width mm"), colWidthM, &(pCol->m_pDynamicWidthExpr), this->GetDocument()->GetSymTable(), this, _TB("static or conditional width of this columns"), CRSExpressionProp::IntValue, CRSColumnWidthWithExprProp::WidthCoor::WidthM,FALSE));
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadRepeaterProperties(CNodeTree* pNode)
{
	Repeater* pRep = dynamic_cast<Repeater*>(pNode->m_pItemData);
	if (!pRep)
	{
		ASSERT(FALSE);
		return;
	}

	SetPanelTitle(L"Repeater");

	CrsProp* NameProp = new CrsProp(_T("Name"), (_variant_t)pRep->GetName(), _TB("The name of the current object"));
	NameProp->AllowEdit(FALSE);
	m_pAppearenceGroup->AddSubItem(NameProp);

	//Id
	CrsProp* IdProp = new CrsProp(_T("Id"), pRep->GetInternalID(), _TB("The internal Id of the current object"));
	IdProp->AllowEdit(FALSE);
	m_pAppearenceGroup->AddSubItem(IdProp);

	LoadRepeaterLayoutProperties(pRep);

	LoadBaseProperties(pNode);

	//repeater commands
	m_pPropGrid->ClearCommands();
	CStringList lstCommands;
	lstCommands.AddTail(_TB("Rebuild"));
	lstCommands.AddTail(_TB("Fit The Content"));
	lstCommands.AddTail(_TB("Clear custom styles"));
	lstCommands.AddTail(_TB("Copy attributes from"));
	m_pPropGrid->SetCommands(lstCommands);
	m_pPropGrid->SendMessage(WM_NCPAINT, DCX_WINDOW, 0L); //TAPULLO by silvano :-P e Andrea per risolvere il problema del mancato disegno dei comandi sotto la property grid
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadRepeaterLayoutProperties(Repeater* pRep)
{
	//Rows
	//m_pLayoutGroup->AddSubItem(new CRSIntProp(pRep, _TB("Rows number"), &(pRep->m_nRows), _TB("Set rows number"), 1, 100));
	m_pLayoutGroup->AddSubItem(new CRSRepeaterCount(pRep, &(pRep->m_nRows), CRSRepeaterCount::PropertyType::Row));
	//Columns
	//m_pLayoutGroup->AddSubItem(new CRSIntProp(pRep, _TB("Columns number"), &(pRep->m_nColumns), _TB("Set columns number"), 1, 100));
	m_pLayoutGroup->AddSubItem(new CRSRepeaterCount(pRep, &(pRep->m_nColumns),CRSRepeaterCount::PropertyType::Col));
	//Offset Group
	CrsProp* offsetGroup = new CrsProp(_TB("Interspace"), 0, FALSE);
		//X	px
		offsetGroup->AddSubItem(new CRSIntProp(pRep, _TB("X px"), &(pRep->m_nXOffset), _TB("Set x Offset in pixel"), 0, 100, CRSIntProp::CooType::XP));
		//Y	px
		offsetGroup->AddSubItem(new CRSIntProp(pRep, _TB("Y px"), &(pRep->m_nYOffset), _TB("Set y Offset in pixel"), 0, 100, CRSIntProp::CooType::YP));

		//X	mm
		offsetGroup->AddSubItem(new CRSIntProp(pRep, _TB("X mm"), &(pRep->m_nXOffset), _TB("Set x Offset in mm"), 0, 100, CRSIntProp::CooType::XM));
		//Y	mm
		offsetGroup->AddSubItem(new CRSIntProp(pRep, _TB("Y mm"), &(pRep->m_nYOffset), _TB("Set y Offset in mm"), 0, 100, CRSIntProp::CooType::YM));

	//Add to parent property
	m_pLayoutGroup->AddSubItem(offsetGroup);
	offsetGroup->Expand(FALSE);
}

// ----------------------------------------------------------------------------
//Si occuper� di caricare la property grid con le properties comuni(vedi loadbaseproperties) 
//e cicler� sugli elementi dell'array per i vari aggiornamenti. 
//RETURN IF HAVE TO EXIT FROM PARENT METHOD
BOOL CRS_ObjectPropertyView::LoadMultipleSelectionProperties(SelectionRect* pMulSel)
{
	//controllo se la multiselezione � valida 
	if (!pMulSel || pMulSel->IsEmpty())
	{
		ClearPropertyGrid();
		return TRUE;
	}
		
	else if (pMulSel->GetSize() == 1)
	{
		//ho una multiselezione con un solo elemento, quindi la faccio diventare una selezione singola, svuotando la multiselezione
		//e simulando il click sull'oggetto rimasto (nel caso venissi da 2 elementi e ne rimuovessi uno premendo shift + click)
		BaseObj* obj = GetDocument()->m_pMultipleSelObj->GetObjAt(0);
		ASSERT_VALID(obj);
		if (!obj)
			return FALSE;
		CPoint centerPoint = obj->GetBaseRect().CenterPoint();

		GetDocument()->OnDeselectAll();
		GetDocument()->GetRSTree(ERefreshEditor::Layouts)->ClearSelection();

		if (GetDocument()->GetWoormView() && GetDocument()->GetWoormView()->m_pProcessingMouse && GetDocument()->GetWoormView()->m_pProcessingMouse->IsLocked())
			GetDocument()->GetWoormView()->m_pProcessingMouse->Unlock();

		GetDocument()->SetCurrentObj(obj, centerPoint);
		return FALSE;
	}

	//TODO ANDREA: ORA RICARICO LA PROPERTY GRID PER RIPASSARE DAL COSTRUTTORE DI OGNI PROPERTY
	//IN QUESTO MODO POSSO VALORIZZARLE CON IL VALORE COMUNE, SE C'�

	//UNA SOLUZIONE ALTERNATIVA SAREBBE INSERIRE UN METODO VIRTUALE IN CRSPROP (AD ESEMPIO UPDATEPROPERTYVALUE)
	//E CHIAMARE SOLO QUELLO

	//ho esattamente 2 elementi nell'array e devo capire se venivo da 1 o da 3. infatti se prima ne avevo
	// 1 ->devo pulire la griglia e caricare la multiselezione
	// 3 ->ho gi� la griglia caricata e non devo fare niente
	//if (pMulSel->GetSize() == 2 && pMulSel->GetPreviousSize() == 1)
	//{
		ClearPropertyGrid();
		//cambio nome alla property grid
		SetPanelTitle(L"MultiSelection");

		// Add Appearence properties--------------------------------------------------
		m_pAppearenceGroup = new CrsProp(_TB("Appearence"));
		// Add Behavior properties--------------------------------------------------
		m_pBehaviorGroup = new CrsProp(_TB("Behavior"));
		// Add Layout properties--------------------------------------------------
		m_pLayoutGroup = new CrsProp(_TB("Layout"));

		//caricamento propriet� comuni
		LoadMultipleSelectionAllProperties(pMulSel);

		m_pPropGrid->AddProperty(m_pAppearenceGroup);
		m_pPropGrid->AddProperty(m_pBehaviorGroup);
		m_pPropGrid->AddProperty(m_pLayoutGroup);
	//}

	////se ho pi� di 2 elementi nell'array, non devo fare niente tranne rivalutare le property
	////per vedere se ce ne sono alcune comuni da valorizzare
	//else
		//m_pPropGrid->AdjustLayout();

	GetDocument()->GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);

	return TRUE;
}

// ----------------------------------------------------------------------------
//Si occuper� di caricare la property grid con le properties comuni(vedi loadbaseproperties) 
void CRS_ObjectPropertyView::LoadMultiColumnProperties(MultiColumnSelection* pMulCol)
{
	ClearPropertyGrid();
	//cambio nome alla property grid
	SetPanelTitle(L"Selected Column");

	CrsProp* countProp = new CrsProp(_T("Count"), (_variant_t)pMulCol->GetSize(), _T("Number of selected columns"));
	countProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(countProp);

	CrsProp* titlesProp = new CrsProp(_T("Titles"), (_variant_t)pMulCol->GetAllTitles(), _T("Titles of all column in selection"));
	titlesProp->AllowEdit(FALSE);
	m_pPropGrid->AddProperty(titlesProp);

	// Add Appearence properties--------------------------------------------------
	m_pAppearenceGroup = new CrsProp(_TB("Appearence"));
	// Add Behavior properties--------------------------------------------------
	m_pBehaviorGroup = new CrsProp(_TB("Behavior"));
	// Add Layout properties--------------------------------------------------
	m_pLayoutGroup = new CrsProp(_TB("Layout"));

	LoadMultiColumnAppearenceProperties(pMulCol);
	LoadMultiColumnBehaviorProperties(pMulCol);
	LoadMultiColumnLayoutProperties(pMulCol);
	
	m_pPropGrid->AddProperty(m_pAppearenceGroup);
	m_pPropGrid->AddProperty(m_pBehaviorGroup);
	m_pPropGrid->AddProperty(m_pLayoutGroup);

	GetDocument()->GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� comuni a tutti le colonne presenti nella multiselezione
void CRS_ObjectPropertyView::LoadMultiColumnAppearenceProperties(MultiColumnSelection* pMulCol)
{
	// ALL COLUMN TITLE----------------------------------------------------------------------------------------------------------ALL COLUMN TITLE
	CrsProp* pTitleGroup = new CrsProp(_TB("Title"), 0, FALSE);

		//BackColor
		pTitleGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::TitleBackColor, this));
		
		//Text Color
		pTitleGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::TitleForeColor, this));

		////columns height
		//pColumnGroup->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::ColumnTitle));

		////separator
		//pColumnGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Titles Separator"), &(pTable->m_Borders.m_bColumnTitleSeparator), _TB("Separator border of Column Titles")));

		//alignment bitwise
		pTitleGroup->AddSubItem(new CRSMultiColumnsAlignmentStyleBitWiseProp(this, pMulCol, CRSMultiColumnsAlignmentStyleBitWiseProp::PropertyType::ColumnTitles));

		//FontStyle
		pTitleGroup->AddSubItem(new CRSMultiColumnFontStyleProp(pMulCol, CRSMultiColumnFontStyleProp::PropertyType::Title));

		//---Column Titles
		CrsProp* pColumnTitlesBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
			
			//Border color
			pColumnTitlesBorderGroup->AddSubItem(new CRSTableMultiColumnsColorProp(pMulCol, CRSTableMultiColumnsColorProp::TitleBorderColor));

			//Size
			pColumnTitlesBorderGroup->AddSubItem(new CRSTableMultiColumnsBorderSizeProp(pMulCol, CRSTableMultiColumnsBorderSizeProp::Title));

		//	//Sides
		//	CrsProp* pColumnBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
		//	pColumnBorderSidesGroup->AllowEdit(FALSE);
		//		//left
		//		pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bColumnTitleLeft), _TB("Left border of Column Titles")));
		//		//top
		//		pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bColumnTitleTop), _TB("Top border of Column Titles")));
		//		//right
		//		pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bColumnTitleRight), _TB("Right border of Column Titles")));
		//		//bottom
		//		pColumnBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bColumnTitleBottom), _TB("Bottom border of Column Titles")));
		//	//add to the parent group
		//	pColumnTitlesBorderGroup->AddSubItem(pColumnBorderSidesGroup);
		//	pColumnBorderSidesGroup->Expand(FALSE);

		//add to the parent group
			pTitleGroup->AddSubItem(pColumnTitlesBorderGroup);
		pColumnTitlesBorderGroup->Expand(FALSE);

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pTitleGroup);

	// ALL BODY------------------------------------------------------------------------------------------------------------ALL BODY
	CrsProp* pBodyGroup = new CrsProp(_TB("Body"), 0, FALSE);

		//Back color
		pBodyGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::BodyBackColor, this));

		//Text Color
		pBodyGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::BodyForeColor, this));

		////rows height
		//pBodyGroup->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::Row));

		////separator
		//pBodyGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Column Separator"), &(pTable->m_Borders.m_bColumnSeparator), _TB("Separator border of Body")));

		////separator mode (propriet� ternaria)
		//pBodyGroup->AddSubItem(new CRSTableRowSeparatorProp(pTable));

		//alignment bitwise
		CRSMultiColumnsAlignmentStyleBitWiseProp* mulAlignmentProp = new CRSMultiColumnsAlignmentStyleBitWiseProp(this, pMulCol, CRSMultiColumnsAlignmentStyleBitWiseProp::PropertyType::Body);
		pBodyGroup->AddSubItem(mulAlignmentProp);

		//Distribute over multiline if too long
		pBodyGroup->AddSubItem(new CRSMultilineMultiColumnsProp(pMulCol, mulAlignmentProp));

		//FontStyle
		pBodyGroup->AddSubItem(new CRSMultiColumnFontStyleProp(pMulCol, CRSMultiColumnFontStyleProp::PropertyType::Body));

		CrsProp* pBodyBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);

		//Border color
		pBodyBorderGroup->AddSubItem(new CRSTableMultiColumnsColorProp(pMulCol, CRSTableMultiColumnsColorProp::BodyBorderColor));

		//Size
		pBodyBorderGroup->AddSubItem(new CRSTableMultiColumnsBorderSizeProp(pMulCol, CRSTableMultiColumnsBorderSizeProp::Body));

		//	//Sides
		//	CrsProp* pBodyBorderSidesGroup = new CrsProp(_TB("Sides"), 0, TRUE);
		//	pBodyBorderSidesGroup->AllowEdit(FALSE);
		//		//left
		//		pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bBodyLeft), _TB("Left border of Body")));
		//		//top
		//		pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bBodyTop), _TB("Top border of Body")));
		//		//right
		//		pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bBodyRight), _TB("Right border of Body")));
		//		//bottom
		//		pBodyBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bBodyBottom), _TB("Bottom border of Body")));
		//	//add to the parent group
		//	pBodyBorderGroup->AddSubItem(pBodyBorderSidesGroup);
		//	pBodyBorderSidesGroup->Expand(FALSE);

		//add to the parent group
		pBodyGroup->AddSubItem(pBodyBorderGroup);
		pBodyBorderGroup->Expand(FALSE);

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pBodyGroup);

	// SUBTOTAL-----------------------------------------------------------------------------------------------------------SUBTOTAL
	CrsProp* pSubTotalGroup = new CrsProp(_TB("SubTotal"), 0, FALSE);
		//Back Color
		pSubTotalGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::SubTotalBackColor, this));

		//Text Color
		pSubTotalGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::SubTotalForeColor, this));

		//font style
		pSubTotalGroup->AddSubItem(new CRSMultiColumnFontStyleProp(pMulCol, CRSMultiColumnFontStyleProp::PropertyType::Subtotal));

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pSubTotalGroup);

	// ALL TOTAL-----------------------------------------------------------------------------------------------------------ALL TOTAL
	CrsProp* pTotalGroup = new CrsProp(_TB("Total"), 0, FALSE);

		////total height
		//pTotalGroup->AddSubItem(new CRSTableHeightsProp(pTable, CRSTableHeightsProp::PropertyType::Total));

		//BackColor
		pTotalGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::TotalBackColor, this));

		//Text Color
		pTotalGroup->AddSubItem(new CRSTableMultiColumnsColorWithExprProp(pMulCol, CRSTableMultiColumnsColorWithExprProp::TotalForeColor, this));

		//alignment bitwise
		pTotalGroup->AddSubItem(new CRSMultiColumnsAlignmentStyleBitWiseProp(this, pMulCol, CRSMultiColumnsAlignmentStyleBitWiseProp::PropertyType::Totals));

		//FontStyle
		pTotalGroup->AddSubItem(new CRSMultiColumnFontStyleProp(pMulCol, CRSMultiColumnFontStyleProp::PropertyType::Total));

			//---Total
		CrsProp* pTotalBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);

				//Border color
				pTotalBorderGroup->AddSubItem(new CRSTableMultiColumnsColorProp(pMulCol, CRSTableMultiColumnsColorProp::TotalBorderColor));

				//Size
				pTotalBorderGroup->AddSubItem(new CRSTableMultiColumnsBorderSizeProp(pMulCol, CRSTableMultiColumnsBorderSizeProp::Total));

		//		//---Sides
		//		CrsProp* pTotalTitlesBorderSidesGroup = new CrsProp(_TB("Sides"), 0, FALSE);
		//		pTotalTitlesBorderSidesGroup->AllowEdit(FALSE);
		//			//left
		//			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Left"), &(pTable->m_Borders.m_bTotalLeft), _TB("Left border of Total")));
		//			//top
		//			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Top"), &(pTable->m_Borders.m_bTotalTop), _TB("Top border of Total")));
		//			//right
		//			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Right"), &(pTable->m_Borders.m_bTotalRight), _TB("Right border of Total")));
		//			//bottom
		//			pTotalTitlesBorderSidesGroup->AddSubItem(new CRSBoolProp(pTable, _TB("Bottom"), &(pTable->m_Borders.m_bTotalBottom), _TB("Bottom border of Total")));
		//		//add to the parent group
		//		pTotalBorderGroup->AddSubItem(pTotalTitlesBorderSidesGroup);
		//		pTotalTitlesBorderSidesGroup->Expand(FALSE);	

			//add to the parent group
			pTotalGroup->AddSubItem(pTotalBorderGroup);
			pTotalBorderGroup->Expand(FALSE);

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pTotalGroup);
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� comuni a tutti le colonne presenti nella multiselezione
void CRS_ObjectPropertyView::LoadMultiColumnBehaviorProperties(MultiColumnSelection* pMulCol)
{
	//Hidden
	//m_pBehaviorGroup->AddSubItem(new CRSTableMultiColumnsBoolProp(pMulCol, CRSTableMultiColumnsBoolProp::PropertyType::Hidden));
	m_pBehaviorGroup->AddSubItem(new CRSMultiColumnsHiddenProp(pMulCol, this));
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� comuni a tutti le colonne presenti nella multiselezione
void CRS_ObjectPropertyView::LoadMultiColumnLayoutProperties(MultiColumnSelection* pMulCol)
{
	//Width
	m_pLayoutGroup->AddSubItem(new CRSTableMultiColumnsSizeProp(pMulCol, CRSTableMultiColumnsSizeProp::PropertyType::Width, 5, 500));
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� comuni a tutti gli oggetti presenti nella multiselezione
void CRS_ObjectPropertyView::LoadMultipleSelectionAllProperties(SelectionRect* pMulSel)
{
	LoadMultipleSelectionAppearenceProperties(pMulSel);
	LoadMultipleSelectionBehaviorProperties(pMulSel);
	LoadMultipleSelectionLayoutProperties(pMulSel);

	m_pMulSel = pMulSel;
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadMultipleSelectionAppearenceProperties(SelectionRect* pMulSel)
{
	//Style
	m_pAppearenceGroup->AddSubItem(new CRSStyleProp(pMulSel, this));

	//transparent
	m_pAppearenceGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Transparent"),CRSMulBoolProp::PropertyType::Transparent, _TB("Determines whether the control background must be transparent")));

	//Backcolor
	m_pAppearenceGroup->AddSubItem(new CRSMulColorWithExprProp(pMulSel, _TB("Background Color"), CRSMulColorWithExprProp::PropertyType::ValueBackGroundColor, _TB("Background Color of selected objects"), this));

	//---shadow group
	CrsProp* pShadowGroup = new CrsProp(_TB("Shadow"), 0, FALSE);
		//color
		pShadowGroup->AddSubItem(new CRSMulColorProp(pMulSel, _TB("ShadowColor"),CRSMulColorProp::PropertyType::Shadow, _TB("This property specifies the color of the common shadow of these controls")));
		//size
		pShadowGroup->AddSubItem(new CRSMulIntProp(pMulSel, _TB("ShadowSize"), CRSMulIntProp::PropertyType::ShadowSize, _TB("Common size of shadow")));
	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pShadowGroup);

	//---border group
	CrsProp* pBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
		//color
		pBorderGroup->AddSubItem(new CRSMulColorProp(pMulSel, _TB("Color"), CRSMulColorProp::PropertyType::Border , _TB("This property specifies the color of the border around the control")));
		//size
		pBorderGroup->AddSubItem(new CRSMulIntProp(pMulSel, _TB("Size"), CRSMulIntProp::PropertyType::BorderSize,  _TB("The size in pixels of the border around the control")));
		//Ratio group
		CrsProp* pRatioGroup = new CrsProp(_TB("Ratio"), 0, TRUE);
			//H
			pRatioGroup->AddSubItem(new CRSMulIntProp(pMulSel, _TB("H"), CRSMulIntProp::PropertyType::HRatio, _TB("H Ratio")));
			//V
			pRatioGroup->AddSubItem(new CRSMulIntProp(pMulSel, _TB("V"), CRSMulIntProp::PropertyType::VRatio, _TB("V Ratio")));
		//add to the parent group
		pBorderGroup->AddSubItem(pRatioGroup);
		//---Side
		CrsProp* pSideGroup = new CrsProp(_TB("Sides"), 0, TRUE);
		pSideGroup->AllowEdit(FALSE);
			//left
			pSideGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Left"), CRSMulBoolProp::PropertyType::BorderSideLeft, _TB("Left border")));
			//top
			pSideGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Top"), CRSMulBoolProp::PropertyType::BorderSideTop, _TB("Top border")));
			//right
			pSideGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Right"), CRSMulBoolProp::PropertyType::BorderSideRight, _TB("Right border")));
			//bottom
			pSideGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Bottom"), CRSMulBoolProp::PropertyType::BorderSideBottom, _TB("Bottom border")));
		//add to the parent group
		pBorderGroup->AddSubItem(pSideGroup);
	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pBorderGroup);

	if (pMulSel->ContainsAtLeastOneFieldRect())
	{
		//Label Group
		CrsProp* pLabelGroup = new CrsProp(_TB("Label"), 0, FALSE);
			//Text Color
			pLabelGroup->AddSubItem(new CRSMulColorWithExprProp(pMulSel, _TB("Text Color"), CRSMulColorWithExprProp::PropertyType::LabelForeColor, _TB("Text Color of selected objects"), this));
			//Label FontStyle
			pLabelGroup->AddSubItem(new CRSMulFontStyleProp(pMulSel, CRSMulFontStyleProp::PropertyType::Label));
			//Bitwise alignment prop
			pLabelGroup->AddSubItem(new CRSMulAlignmentStyleBitWiseProp(this, pMulSel, CRSMulAlignmentStyleBitWiseProp::PropertyType::Label));

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pLabelGroup);
	}

	//Value Group
	CrsProp* pValueGroup = new CrsProp(_TB("Value"), 0, FALSE);
		//Text Color
		pValueGroup->AddSubItem(new CRSMulColorWithExprProp(pMulSel, _TB("Text Color"), CRSMulColorWithExprProp::PropertyType::ValueForeColor, _TB("Text Color of selected objects"), this));
		//Value FontStyle
		pValueGroup->AddSubItem(new CRSMulFontStyleProp(pMulSel, CRSMulFontStyleProp::PropertyType::Value));
		//Value Alignment
		//pValueGroup->AddSubItem(new CRSMulAlignmentStyleProp(pMulSel, CRSMulAlignmentStyleProp::PropertyType::Value));
		//Bitwise alignment prop
		pValueGroup->AddSubItem(new CRSMulAlignmentStyleBitWiseProp(this, pMulSel, CRSMulAlignmentStyleBitWiseProp::PropertyType::Value));

	//add to the parent group
	m_pAppearenceGroup->AddSubItem(pValueGroup);
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadMultipleSelectionBehaviorProperties(SelectionRect* pMulSel)
{
	CStringList lstCommands;

	//Align Left
	lstCommands.AddTail(_TB("Align Left"));
	//Align Top
	lstCommands.AddTail(_TB("Align Top"));
	//Same Width
	lstCommands.AddTail(_TB("Same Width"));
	//Same Height
	lstCommands.AddTail(_TB("Same Heigth"));

	m_pPropGrid->SetCommands(lstCommands);

	//Hidden
	//m_pBehaviorGroup->AddSubItem(new CRSMulBoolProp(pMulSel, _TB("Hidden"), CRSMulBoolProp::PropertyType::Hidden, _TB("Set if selected objects must be visible or not")));

	//Hidden with expr
	m_pBehaviorGroup->AddSubItem(new CRSMulHiddenProp(pMulSel, _TB("Hidden"), _TB("Set if selected objects must be visible or not"), this));
}

// ----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadMultipleSelectionLayoutProperties(SelectionRect* pMulSel)
{
	//AnchorTo
	m_pLayoutGroup->AddSubItem(new CRSAnchorToProp(pMulSel, m_pPropGrid));

	//---Alignment group
	CrsProp* pAlignmentProp = new CrsProp(_TB("Align To"), 0, TRUE);
		//x	pixel		  prop 0
		pAlignmentProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("X px"), CRSMulRectProp::PropertyType::AlignToXP, _TB("Align the objects to the value")));
		//y	  pixel		prop 1
		pAlignmentProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Y px"), CRSMulRectProp::PropertyType::AlignToYP, _TB("Align the objects to the value")));
		//x	mm			prop 2
		pAlignmentProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("X mm"), CRSMulRectProp::PropertyType::AlignToXM, _TB("Align the objects to the value")));
		//y	  mm		 prop 3
		pAlignmentProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Y mm"), CRSMulRectProp::PropertyType::AlignToYM, _TB("Align the objects to the value")));

	//add to the parent group
		m_pLayoutGroup->AddSubItem(pAlignmentProp);

	//---location group
		CrsProp* pLocationProp = new CrsProp(_TB("Location"), 0, TRUE);
		//x	  pix		prop 0
		pLocationProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("X px"), CRSMulRectProp::PropertyType::LocationXP, _TB("common X coordinates of selected objects")));
		//y	  pix		prop 1
		pLocationProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Y px"), CRSMulRectProp::PropertyType::LocationYP, _TB("common Y coordinates of selected objects")));
		//x	  mm		prop 2
		pLocationProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("X mm"), CRSMulRectProp::PropertyType::LocationXM, _TB("common X coordinates of selected objects")));
		//y	  mm		prop 3
		pLocationProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Y mm"), CRSMulRectProp::PropertyType::LocationYM, _TB("common Y coordinates of selected objects")));

	//add to the parent group
	m_pLayoutGroup->AddSubItem(pLocationProp);

	//---size group
	CrsProp* pSizeProp = new CrsProp(_TB("Size"), 0, TRUE);
		//width	px	prop 0
		pSizeProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Width px"), CRSMulRectProp::PropertyType::WidthP, _TB("common Width of selected objects")));
		//height px	  prop 1
		pSizeProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Height px"), CRSMulRectProp::PropertyType::HeightP, _TB("common Height of selected objects")));

		//width	mm	 prop 2
		pSizeProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Width mm"), CRSMulRectProp::PropertyType::WidthM, _TB("common Width of selected objects")));
		//height mm  prop 3
		pSizeProp->AddSubItem(new CRSMulRectProp(pMulSel, _TB("Height mm"), CRSMulRectProp::PropertyType::HeightM, _TB("common Height of selected objects")));

	//add to the parent group
	m_pLayoutGroup->AddSubItem(pSizeProp);

	//layer - for Z order filter in design mode
	m_pLayoutGroup->AddSubItem(new CRSMulIntProp(pMulSel, _TB("Layer"), CRSMulIntProp::Layer,
		_TB("It is a filter of visibility  to use during design mode to show different group of overlapped objects, only one group at a time")));
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� relative alla cella di una tabella
void CRS_ObjectPropertyView::LoadTableCellProperties(CNodeTree* pNode)
{
	TableCell* pCell = dynamic_cast<TableCell*>(pNode->m_pItemData);
	if (!pCell)
	{
		ASSERT(FALSE);
		return;
	}

	LoadTableCellProperties(pCell);
}

// ----------------------------------------------------------------------------
//caricamento delle propriet� relative alla cella di una tabella
void CRS_ObjectPropertyView::LoadTableCellProperties(TableCell* pCell)
{
	ASSERT_VALID(pCell);
	ASSERT_VALID(m_pPropGrid);

	if (!ClearPropertyGrid()) return;

	m_pSelectedTableCell = pCell;

	BOOL isSubTotal = pCell->IsSubTotal();
	BOOL isTotal = dynamic_cast<TotalCell*>(pCell) == false ? FALSE : TRUE;
	BOOL isNormalCell = !isSubTotal && !isTotal;

	COLORREF* bkgColor;
	COLORREF* foreColor;

	// Add Appearence properties----------------------------------------------
	m_pAppearenceGroup = new CrsProp(_TB("Appearence"));
	
	if (isSubTotal)
	{
		SetPanelTitle(_TB("Subtotal Cell"));

		bkgColor = &(pCell->m_pColumn->m_SubTotal.m_rgbBkgColor);
		foreColor = &(pCell->m_pColumn->m_SubTotal.m_rgbTextColor);
	}

	else
	{
		if (isNormalCell)
			SetPanelTitle(_TB("Table Cell"));
	
		bkgColor = pCell->GetBkgColor();
		foreColor = pCell->GetValueForeColor();
	}
	
	//Back color
	m_pAppearenceGroup->AddSubItem(new CRSColorProp(pCell, _TB("Background Color"), bkgColor, _TB("Background Color of selected cell")));
	//Text Color
	m_pAppearenceGroup->AddSubItem(new CRSColorProp(pCell, _TB("Text Color"), foreColor, _TB("Text Color of selected cell")));

	//Alignment
	if (!isSubTotal)
	{
		//Simil BitWise Alignment prop
		m_pAppearenceGroup->AddSubItem(new CRSAlignBitwiseProp(this, pCell, _TB("Alignment"), &(pCell->m_Value.m_nAlign), TRUE, FALSE, FALSE));
	}

	//Font Style
	m_pAppearenceGroup->AddSubItem(new CRSSetFontDlgProp(pCell, _TB("Fontstyle"), CRSSetFontDlgProp::PropertyType::TableCellValue));

	//Apply to all rows
	CStringList lstCommands;
	lstCommands.AddTail(_TB("Apply to all row"));
	m_pPropGrid->SetCommands(lstCommands);

	//for total cell only
	if (isTotal)
	{
		TotalCell* pTotalCell = (TotalCell*)pCell;

		CWoormDocMng* wrmDocMng = this->GetDocument();
		CWordArray		idsDummy;
		
		wrmDocMng->GetEditorSymTable()->GetTotalOf(pTotalCell->m_pColumn->GetInternalID(), idsDummy, WoormField::FIELD_COLTOTAL);
		if (idsDummy.GetSize() > 0)
		{
			WORD id = idsDummy.GetAt(0);
			WoormField* wrmField = wrmDocMng->GetEditorSymTable()->GetFieldByID(id);

			CString title = _TB("Total Cell") + _T(" - ") + wrmField->GetName();
			SetPanelTitle(title);

			//Id
			CrsProp* IdProp = new CrsProp(_T("Id"), id, _TB("The internal Id of the current object"));
			IdProp->AllowEdit(FALSE);
			m_pPropGrid->AddProperty(IdProp, FALSE, FALSE);
			//Fieldname
			CrsProp* nameProp = new CrsProp(_TB("Field Name"), (_variant_t)wrmField->GetName(), _TB("Variable Name"));
			nameProp->AllowEdit(FALSE);
			m_pPropGrid->AddProperty(nameProp, FALSE, FALSE);
			//pulsanti nella toolbar
			m_eShowVariableTypeBtn = wrmField->GetSourceEnum();
			m_bShowLayoutBtn = TRUE;
			SetCheckLayoutBtn(TRUE);
		}

		CrsProp* pBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);

			//color
			pBorderGroup->AddSubItem(new CRSColorProp(pTotalCell, _TB("Color"), &(pTotalCell->m_TotalPen.m_rgbColor), _TB("Border Color of this total cell")));
			//width
			pBorderGroup->AddSubItem(new CRSIntProp(pTotalCell, _TB("Size"), &(pTotalCell->m_TotalPen.m_nWidth), _TB("Border Size of this total cell")));

			//---Sides
			CrsProp* pTotalBorderSidesGroup = new CrsProp(_TB("Sides"), 0, FALSE);
			pTotalBorderSidesGroup->AllowEdit(FALSE);
				//left
				pTotalBorderSidesGroup->AddSubItem(new CRSBoolProp(pTotalCell, _TB("Left"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalLeft), _TB("Left border of Total")));
				//top
				pTotalBorderSidesGroup->AddSubItem(new CRSBoolProp(pTotalCell, _TB("Top"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalTop), _TB("Top border of Total")));
				//right
				pTotalBorderSidesGroup->AddSubItem(new CRSBoolProp(pTotalCell, _TB("Right"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalRight), _TB("Right border of Total")));
				//bottom
				pTotalBorderSidesGroup->AddSubItem(new CRSBoolProp(pTotalCell, _TB("Bottom"), &(pTotalCell->m_pColumn->GetTable()->m_Borders.m_bTotalBottom), _TB("Bottom border of Total")));
			//add to the parent group
			pBorderGroup->AddSubItem(pTotalBorderSidesGroup);

		//add to the parent group
		m_pAppearenceGroup->AddSubItem(pBorderGroup);
	}

	//--------------------------------------------------
	m_pPropGrid->AddProperty(m_pAppearenceGroup);
}

// ----------------------------------------------------------------------------
//Copio le properties di una cella in tutte le altre celle della riga
void CRS_ObjectPropertyView::CopyTableCellPropertiesToAllRow()
{
	ASSERT_VALID(m_pSelectedTableCell);
	ASSERT_VALID(m_pSelectedTableCell->m_pColumn);

	COLORREF colors[CCellColorsDlg::MAX];
	colors[CCellColorsDlg::BACKGROUND]	= *m_pSelectedTableCell->GetBkgColor();
	colors[CCellColorsDlg::VALUE]		= *m_pSelectedTableCell->GetValueForeColor();

	Table* pTable = m_pSelectedTableCell->m_pColumn->GetTable();

	if (dynamic_cast<TotalCell*>(m_pSelectedTableCell))
	{
		colors[CCellColorsDlg::BORDER] = ((TotalCell*)m_pSelectedTableCell)->m_TotalPen.m_rgbColor;

		pTable->SetAllTotalsColor	(colors);
		pTable->SetAllTotalsAlign	(m_pSelectedTableCell->GetCellAlign());
		pTable->SetAllTotalsFontIdx	(m_pSelectedTableCell->GetCellFontIdx());

		BorderPen pen = ((TotalCell*)m_pSelectedTableCell)->GetTotalPen();
		pTable->SetAllTotalsPen		(pen);
	}

	else
	{
		pTable->SetRowColor			(colors, m_pSelectedTableCell->m_nCurrRow);
		pTable->SetRowAlign			(m_pSelectedTableCell->GetCellAlign(), m_pSelectedTableCell->m_nCurrRow);
		pTable->SetRowFontIdx		(m_pSelectedTableCell->GetCellFontIdx(), m_pSelectedTableCell->m_nCurrRow);
	}
	
	pTable->Redraw();
}

//================================CRSetFontDlgProp=============================
//-----------------------------------------------------------------------------
CRSSetFontDlgProp::CRSSetFontDlgProp(CObject* pOwner, const CString& strName, PropertyType propType)
	:
	CrsProp(strName, L""),
	m_pOwner(pOwner),
	m_pPropertyType(propType)
{
	AllowEdit(FALSE);
	switch (m_pPropertyType)
	{
	case(PropertyType::TextRectDescription) :
	{
		SetDescription(_TB("Text description FontStyle"));
		break;
	}

	case(PropertyType::FieldRectValue) :
	{
		SetDescription(_TB("Value FontStyle"));
		break;
	}

	case(PropertyType::FieldRectLabel) :
	{
		SetDescription(_TB("Label FontStyle"));
		break;
	}

	case(PropertyType::TableTitle) :
	{
		SetDescription(_TB("Table title FontStyle"));
		break;
	}

	case(PropertyType::ColumnTitle) :
	{
		SetDescription(_TB("Column title FontStyle"));
		break;
	}

	case(PropertyType::TableCellValue) :
	{
		SetDescription(_TB("Table cell FontStyle"));
		break;
	}

	case(PropertyType::AllColumnTitles) :
	{
		SetDescription(_TB("All column titles FontStyle"));
		break;
	}

	case(PropertyType::AllSubTotals) :
	{
		SetDescription(_TB("All subtotals FontStyle"));
		break;
	}

	case(PropertyType::AllColumnsBody) :
	{
		SetDescription(_TB("Body Font Style"));
		break;
	}

	default:
		SetDescription(_TB("Font Style Dialog Prop"));
		break;
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//implementazione metodo virtuale classe BCG chiamato nella on-click del pulsante
void CRSSetFontDlgProp::OnClickButton(CPoint point)
{
	ASSERT_VALID(m_pOwner);

	switch (m_pPropertyType)
	{
	case(PropertyType::TextRectDescription) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TextRect)))
		{
			ASSERT(FALSE);
			break;
		}

		TextRect* pText = (TextRect*)m_pOwner;

		pText->SetFontStyle();
		break;
	}

	case(PropertyType::FieldRectValue) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			ASSERT(FALSE);
			break;
		}

		FieldRect* pField = (FieldRect*)m_pOwner;
		
		pField->SetFontStyle();
		break;
	}

	case(PropertyType::FieldRectLabel) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			ASSERT(FALSE);
			break;
		}

		FieldRect* pField = (FieldRect*)m_pOwner;
		
		pField->SetLabelFontStyle();
		break;
	}

	case (PropertyType::FileRectText) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FileRect)))
		{
			ASSERT(FALSE);
			break;
		}

		FileRect* pFileObj = (FileRect*)m_pOwner;

		pFileObj->SetFontStyle();
		break;
	}

	case(PropertyType::TableTitle) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
		{
			ASSERT(FALSE);
			break;
		}

		Table* pTabObj = (Table*)m_pOwner;
		
		pTabObj->OnTableTitleFontStyle();
		break;
	}
	case(PropertyType::TableSubTitle):
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
		{
			ASSERT(FALSE);
			break;
		}

		Table* pTabObj = (Table*)m_pOwner;

		pTabObj->OnTableSubTitleFontStyle();
		break;
	}

	case (PropertyType::ColumnTitle) : 
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
		{
			ASSERT(FALSE);
			break;
		}

		TableColumn* pColObj = (TableColumn*)m_pOwner;

		pColObj->GetTable()->OnColumnTitleFontStyle();
		break;
	}

	case (PropertyType::ColumnSubTotal) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
		{
			ASSERT(FALSE);
			break;
		}

		TableColumn* pColObj = (TableColumn*)m_pOwner;

		pColObj->GetTable()->OnSubTotalFontStyle();
		break;
	}

	case(PropertyType::TableCellValue) :
	{
		if (dynamic_cast<TotalCell*>(m_pOwner))
		{
			((TotalCell*)m_pOwner)->m_pColumn->GetTable()->OnTotalFontStyle();
		}

		else if (m_pOwner->GetRuntimeClass() == RUNTIME_CLASS(TableCell))
		{
			((TableCell*)m_pOwner)->m_pColumn->GetTable()->OnCellFontStyle();
		}

		break;
	}

	case(PropertyType::AllColumnTitles) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;

		FontIdx commonFont = GetFontIdx(GetValue());
		// construct font dialogs
		CFontStylesDlg dialog(*(pTabObj->m_pDocument->m_pFontStyles), commonFont, FALSE, NULL, pTabObj->m_pDocument->GetNamespace(), FALSE, pTabObj->m_pDocument->m_Template.m_bIsTemplate);

		if (dialog.DoModal() != IDOK)
		{
			if (!pTabObj->m_pDocument->IsModified())
				pTabObj->m_pDocument->SetModifiedFlag(pTabObj->m_pDocument->m_pFontStyles->IsModified());
			return;
		}

		pTabObj->SetAllColumnsTitleFontIdx(commonFont);

		pTabObj->Redraw();
		break;
	}

	case(PropertyType::AllSubTotals) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;

		FontIdx commonFont = GetFontIdx(GetValue());
		// construct font dialogs
		CFontStylesDlg dialog(*(pTabObj->m_pDocument->m_pFontStyles), commonFont, FALSE, NULL, pTabObj->m_pDocument->GetNamespace(), FALSE, pTabObj->m_pDocument->m_Template.m_bIsTemplate);

		if (dialog.DoModal() != IDOK)
		{
			if (!pTabObj->m_pDocument->IsModified())
				pTabObj->m_pDocument->SetModifiedFlag(pTabObj->m_pDocument->m_pFontStyles->IsModified());
			return;
		}

		pTabObj->SetAllSubTotalsFontIdx(commonFont);

		pTabObj->Redraw();
		break;
	}

	case(PropertyType::AllColumnsBody) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;

		FontIdx commonFont = GetFontIdx(GetValue());
		// construct font dialogs
		CFontStylesDlg dialog(*(pTabObj->m_pDocument->m_pFontStyles), commonFont, FALSE, NULL, pTabObj->m_pDocument->GetNamespace(), FALSE, pTabObj->m_pDocument->m_Template.m_bIsTemplate);

		if (dialog.DoModal() != IDOK)
		{
			if (!pTabObj->m_pDocument->IsModified())
				pTabObj->m_pDocument->SetModifiedFlag(pTabObj->m_pDocument->m_pFontStyles->IsModified());
			return;
		}

		pTabObj->SetAllColumnsFontIdx(commonFont);

		pTabObj->Redraw();
		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSSetFontDlgProp::UpdatePropertyValue()
{
	switch (m_pPropertyType)
	{
		case(PropertyType::TextRectDescription) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				ASSERT(FALSE);
				break;
			}

			TextRect* pText = (TextRect*)m_pOwner;

			SetValue((_variant_t)GetFontName(pText->m_StaticText.GetFontIdx()));
			break;
		}

		case(PropertyType::FieldRectValue) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				ASSERT(FALSE);
				break;
			}

			FieldRect* pField = (FieldRect*)m_pOwner;

			SetValue((_variant_t)GetFontName(pField->m_Value.GetFontIdx()));
			break;
		}

		case(PropertyType::FieldRectLabel) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				ASSERT(FALSE);
				break;
			}

			FieldRect* pField = (FieldRect*)m_pOwner;

			SetValue((_variant_t)GetFontName(pField->m_Label.GetFontIdx()));
			break;
		}

		case (PropertyType::FileRectText) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FileRect)))
			{
				ASSERT(FALSE);
				break;
			}

			FileRect* pFileObj = (FileRect*)m_pOwner;

			SetValue((_variant_t)GetFontName(pFileObj->m_StaticText.GetFontIdx()));
			break;
		}

		case(PropertyType::TableTitle) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			{
				ASSERT(FALSE);
				break;
			}

			Table* pTabObj = (Table*)m_pOwner;

			SetValue((_variant_t)GetFontName(pTabObj->GetTableTitleFontIdx()));
			break;
		}
		case(PropertyType::TableSubTitle):
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			{
				ASSERT(FALSE);
				break;
			}

			Table* pTabObj = (Table*)m_pOwner;

			SetValue((_variant_t)GetFontName(pTabObj->m_SubTitle.GetFontIdx()));
			break;
		}


		case (PropertyType::ColumnTitle) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			{
				ASSERT(FALSE);
				break;
			}

			TableColumn* pColObj = (TableColumn*)m_pOwner;

			SetValue((_variant_t)GetFontName(pColObj->GetColumnTitleFontIdx()));
			break;
		}

		case (PropertyType::ColumnSubTotal) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			{
				ASSERT(FALSE);
				break;
			}

			TableColumn* pColObj = (TableColumn*)m_pOwner;

			SetValue((_variant_t)GetFontName(pColObj->GetSubTotalFontIdx()));
			break;
		}

		case(PropertyType::TableCellValue) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableCell)))
			{
				ASSERT(FALSE);
				break;
			}

			TableCell* pCell = (TableCell*)m_pOwner;
			SetValue((_variant_t)GetFontName(pCell->GetCellFontIdx()));
			break;
		}

		case (PropertyType::AllColumnTitles) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			{
				ASSERT(FALSE);
				break;
			}

			Table* pTabObj = (Table*)m_pOwner;

			FontIdx commonFont=0;
			int i = 0;

			for (i = 0; i < pTabObj->m_Columns.GetSize(); i++)
			{
				TableColumn* pCol = (TableColumn*)pTabObj->m_Columns[i];
				if (!pCol)
				{
					ASSERT(FALSE);
					break;
				}

				if (i == 0 || commonFont == 0)
					commonFont = pCol->GetColumnTitleFontIdx();

				else if (commonFont != pCol->GetColumnTitleFontIdx())
					break;
			}

			//todo andrea: se non c'e font comune, metterlo default
			SetValue((_variant_t)GetFontName(commonFont));
			
			break;
		}

		case (PropertyType::AllSubTotals) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			{
				ASSERT(FALSE);
				break;
			}

			Table* pTabObj = (Table*)m_pOwner;

			FontIdx commonFont = 0;
			int i = 0;

			for (i = 0; i < pTabObj->m_Columns.GetSize(); i++)
			{
				TableColumn* pCol = (TableColumn*)pTabObj->m_Columns[i];
				if (!pCol)
				{
					ASSERT(FALSE);
					break;
				}

				if (i == 0 || commonFont == 0)
					commonFont = pCol->GetSubTotalFontIdx();

				else if (commonFont != pCol->GetSubTotalFontIdx())
					break;
			}

			//todo andrea: se non c'e font comune, metterlo default
			SetValue((_variant_t)GetFontName(commonFont));

			break;
		}

		case (PropertyType::AllColumnsBody) :
		{
			if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			{
				ASSERT(FALSE);
				break;
			}

			Table* pTabObj = (Table*)m_pOwner;

			FontIdx commonFont = 0;
			int i = 0;

			for (i = 0; i < pTabObj->m_Columns.GetSize(); i++)
			{
				TableColumn* pCol = (TableColumn*)pTabObj->m_Columns[i];
				if (!pCol)
				{
					ASSERT(FALSE);
					break;
				}

				if (i == 0 || commonFont == 0)
					commonFont = pCol->GetColumnFontIdx();

				else if (commonFont != pCol->GetColumnFontIdx())
					break;
			}

			//todo andrea: se non c'e font comune, metterlo default
			SetValue((_variant_t)GetFontName(commonFont));

			break;
		}
	}
}

//-----------------------------------------------------------------------------
//metodo per aggiornare il font name della property
CString CRSSetFontDlgProp::GetFontName(FontIdx fontIdx)
{
	CString fontName = _TB("Error retrieving font name");

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pBase = (BaseRect*)m_pOwner;
		fontName = pBase->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTabObj = (Table*)m_pOwner;
		fontName = pTabObj->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		fontName = pColObj->GetTable()->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	if (m_pOwner->GetRuntimeClass() == RUNTIME_CLASS(TableCell))
	{
		TableCell* pCell = (TableCell*)m_pOwner;
		fontName = pCell->m_pColumn->GetTable()->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	return fontName;
}

//-----------------------------------------------------------------------------
//metodo per aggiornare il font name della property
FontIdx CRSSetFontDlgProp::GetFontIdx(CString fontName)
{
	FontIdx fontIdx;
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTabObj = (Table*)m_pOwner;
		fontIdx = pTabObj->m_pDocument->m_pFontStyles->GetFontIdx(fontName);
	}

	return fontIdx;
}

//================================CRSSearchTbDialogProp=============================
//-----------------------------------------------------------------------------
CRSSearchTbDialogProp::CRSSearchTbDialogProp(CObject* pObj, CString* pStrFileName, PropertyType propType, CRSFileNameProp::Filter filter, CRS_ObjectPropertyView* propertyView)
	:
	CRSFileNameProp(pObj, L"", pStrFileName, L"", filter),
	m_propType(propType),
	m_pPropertyView(propertyView), 
	m_eFilter(filter)
{

	InitProperty(m_propType);
}

//-----------------------------------------------------------------------------
CRSSearchTbDialogProp::CRSSearchTbDialogProp(CObject* pObj, Value* pValue, PropertyType propType, CRSFileNameProp::Filter filter, CRS_ObjectPropertyView* propertyView)
	:
	CRSFileNameProp(pObj, L"", (_variant_t)pValue->GetText(), L"", filter),
	m_propType(propType),
	m_pPropertyView(propertyView),
	m_eFilter(filter)
{
	InitProperty(m_propType);
}

//-----------------------------------------------------------------------------
void CRSSearchTbDialogProp::InitProperty(PropertyType propType)
{
	ASSERT_VALID(m_pOwner);
	m_dwFlags = 1;//todo andrea: rivedere
	SetOwnerList(m_pPropertyView->m_pPropGrid);

	switch (propType)
	{
	case PropertyType::GraphRectImgName:
	case PropertyType::GenericImgName:
	{
		SetName(_TB("Image Filename"));
		break;
	}

	case PropertyType::FileRectFileName:
	case PropertyType::GenericTextFileName:
	{
		SetName(_TB("Filename"));
		break;
	}

	case PropertyType::FieldValueFileName:
	case PropertyType::FieldValueImgName:
	{
		SetName(_TB("Preview filename"));
		SetDescription(_TB("!Only for Preview! The value of this property is temporary and will not be saved."));
		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//implementazione metodo virtuale classe BCG chiamato nella on-click del pulsante
void CRSSearchTbDialogProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - tb explorer
		OnLeftButtonClick();
		UpdatePropertyValue();
	}

	else
	{
		// right - file system browser using parent property handling
		CRSFileNameProp::OnClickButton(point);
	}
}

// Click on Left button: open Tb Explorer
//-----------------------------------------------------------------------------
void CRSSearchTbDialogProp::OnLeftButtonClick()
{
	switch (m_propType)
	{
	case PropertyType::FileRectFileName:
	{
		FileRect* pFile = dynamic_cast<FileRect*>(m_pOwner);
		if (!pFile)
		{
			ASSERT(FALSE);
			return;
		}

		pFile->LoadFromFile();
		break;
	}

	case PropertyType::GraphRectImgName:
	{
		GraphRect* pGraph = dynamic_cast<GraphRect*>(m_pOwner);
		if (!pGraph)
		{
			ASSERT(FALSE);
			return;
		}

		pGraph->LoadFromFile();
		break;
	}

	case PropertyType::GenericImgName:
	{
		CString strName(*m_pStrFileName);
		CTBNamespace aNamespace(CTBNamespace::IMAGE);
		aNamespace.SetNamespace(L"Image." + m_pPropertyView->GetDocument()->GetNamespace().GetApplicationName() + '.' + m_pPropertyView->GetDocument()->GetNamespace().GetModuleName());

		ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNamespace);
		pExplorer->SetCanLink();

		if (!pExplorer->Open())
			return;

		CString strPath;
		pExplorer->GetSelPathElement(strPath);

		if (!strPath.IsEmpty())
			*m_pStrFileName = strPath;

		if (*m_pStrFileName != strName)
			m_pPropertyView->GetDocument()->UpdateLayout();

		break;
	}

	case PropertyType::GenericTextFileName:
	{
		CString strName(*m_pStrFileName);
		CTBNamespace aNamespace(CTBNamespace::TEXT);
		aNamespace.SetNamespace(L"Text." + m_pPropertyView->GetDocument()->GetNamespace().GetApplicationName() + '.' + m_pPropertyView->GetDocument()->GetNamespace().GetModuleName());

		ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNamespace);
		pExplorer->SetCanLink();

		if (!pExplorer->Open())
			return;

		CString strPath;
		pExplorer->GetSelPathElement(strPath);

		if (!strPath.IsEmpty())
			*m_pStrFileName = strPath;

		if (*m_pStrFileName != strName)
			m_pPropertyView->GetDocument()->UpdateLayout();

		break;
	}

	case PropertyType::FieldValueFileName:
	case PropertyType::FieldValueImgName:
	{
		FieldRect* pField = dynamic_cast<FieldRect*>(m_pOwner);
		if (!pField)
		{
			ASSERT(FALSE);
			return;
		}

		CString strName(this->GetValue());
		CTBNamespace ns = CTBNamespace::IMAGE;

		switch(m_eFilter)
		{
		case CRSFileNameProp::Filter::Img:
			ns= CTBNamespace::IMAGE;
			ns.SetNamespace(L"Image." + m_pPropertyView->GetDocument()->GetNamespace().GetApplicationName() + '.' + m_pPropertyView->GetDocument()->GetNamespace().GetModuleName());
			break;
		case CRSFileNameProp::Filter::Txt:
			ns = CTBNamespace::TEXT;
			ns.SetNamespace(L"Text." + m_pPropertyView->GetDocument()->GetNamespace().GetApplicationName() + '.' + m_pPropertyView->GetDocument()->GetNamespace().GetModuleName());
			break;
		}

		ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, ns);
		pExplorer->SetCanLink();

		if (!pExplorer->Open())
			return;

		CString strPath;
		pExplorer->GetSelPathElement(strPath);

		if (!strPath.IsEmpty())
			SetValue((_variant_t)strPath);

		break;
	}
	}
}

//aggiorno il campo testuale della property
//-----------------------------------------------------------------------------
void CRSSearchTbDialogProp::UpdatePropertyValue()
{
	switch (m_propType)
	{
	case PropertyType::FileRectFileName:
	{
		FileRect* pFile = dynamic_cast<FileRect*>(m_pOwner);
		if (!pFile)
		{
			ASSERT(FALSE);
			return;
		}

		SetValue((_variant_t)pFile->m_strFileName);

		//pFile->Reload(TRUE);
		break;
	}

	case PropertyType::GraphRectImgName:
	{
		GraphRect* pGraph = dynamic_cast<GraphRect*>(m_pOwner);
		if (!pGraph)
		{
			ASSERT(FALSE);
			return;
		}

		SetValue((_variant_t)pGraph->m_sImage);

		//pGraph->Reload();
		break;
	}

	case PropertyType::GenericImgName:
	case PropertyType::GenericTextFileName:
	{
		SetValue((_variant_t)*m_pStrFileName);
		break;
	}

	case PropertyType::FieldValueFileName:
	case PropertyType::FieldValueImgName:
	{
		FieldRect* pField = dynamic_cast<FieldRect*>(m_pOwner);
		if (!pField)
		{
			ASSERT(FALSE);
			return;
		}

		CString actualValue = GetValue();

		pField->m_Value.SetText(actualValue);

		pField->Redraw();
		break;
	}
	}
}

//disegna i due pulsanti
//-----------------------------------------------------------------------------
void CRSSearchTbDialogProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
				rect.right = rect.left + rect.Width() / 2;

				// Draw normal button for tb explorer
				COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

				CString str = m_strButtonText;
				if (!str.IsEmpty())
				{
					COLORREF clrTextOld = pDC->SetTextColor(clrText);
					rect.DeflateRect(2, 2);
					rect.bottom--;
					pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
					pDC->SetTextColor(clrTextOld);
				}
		}

		else
		{
			rect.left = rect.right - rect.Width() / 2;

			//Draw custom folder find button
			CBCGPToolbarButton button;
			CBCGPVisualManager::BCGBUTTON_STATE state = FALSE ? CBCGPVisualManager::ButtonsIsHighlighted : CBCGPVisualManager::ButtonsIsRegular; //todo andrea: rivedere
			CBCGPVisualManager::GetInstance()->OnFillButtonInterior(pDC, &button, rect, state);
			
			GetPropertyGrid()->m_imageList.DrawEx(pDC, rect, CRS_PropertyGrid::Img::OpenFolder, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
			//m_image.DrawEx(pDC, rect, 0, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
			CBCGPVisualManager::GetInstance()->OnDrawButtonBorder(pDC, &button, rect, state);
		}
	}
}

//-----------------------------------------------------------------------------
//per disegnare due pulsanti
void CRSSearchTbDialogProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//================================CRSLayoutTemplateProp=============================
//-----------------------------------------------------------------------------
CRSLayoutTemplateProp::CRSLayoutTemplateProp(CObject* pObj, CString* strFileName, CRS_ObjectPropertyView* propertyView, CRSImportStaticObjects* pImportStaticObjectsProp)
	:
	CRSFileNameProp(pObj,_T("Template"), strFileName, L"", CRSFileNameProp::Filter::WoormTemplate),
	m_pPropertyView(propertyView),
	m_pImportStaticObjects(pImportStaticObjectsProp)
{
	ASSERT_VALID(pObj);
	ASSERT_VALID(propertyView);
	m_dwFlags = 1;//todo andrea: rivedere

	m_image.SetImageSize(CSize(14, 14));
	m_image.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_image);
	HICON icon = TBLoadPng(TBGlyph(szGlyphOpenFolder14));
	m_image.AddIcon(icon);
	::DestroyIcon(icon);

	AddTemplates();

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
CRSLayoutTemplateProp::~CRSLayoutTemplateProp()
{
	m_arStrFullNameTmpl.RemoveAll();
}

//Fill Dropdown
//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::AddTemplates()
{
	if (AfxIsActivated(MAGONET_APP, _NS_ACT("Core")))
		AddTemplatesFromModule(_NS_MOD("Module.Erp.Core"));
	else if (AfxIsActivated(OFM_APP, _NS_ACT("Core")))
		AddTemplatesFromModule(_NS_MOD("Module.Ofm.Core"));
	
	//Unload template
	AddOption(_TB("Unload Template"), TRUE, -1);
}

//Fill Dropdown
//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::AddTemplatesFromModule(LPCTSTR sNs)
{
	CTBNamespace aNameSpace(sNs);
	aNameSpace.SetType(CTBNamespace::REPORT);

	CStringArray aUserSel; aUserSel.Add(AfxGetLoginInfos()->m_strUserName);
	CStringArray aAllModulesPath;
	CStringArray aAllObjPaths;
	AfxGetPathFinder()->GetAllModulePath(aAllModulesPath, aNameSpace, aUserSel, TRUE, TRUE, TRUE, TRUE, aNameSpace.GetObjectName(CTBNamespace::MODULE));
	AfxGetPathFinder()->GetAllObjInFolder(aAllObjPaths, aNameSpace, aAllModulesPath, _T("*"), _T("*.wrmt"));

	for (int s = 0; s <= aAllObjPaths.GetUpperBound(); s++)
	{
		CString strFullName = aAllObjPaths.GetAt(s);
		CString sSubObjName = ::GetName(strFullName);

		if (!::IsValidObjName(sSubObjName))
			continue;// invalid character into object name filter

		int index = m_arStrFullNameTmpl.Add(strFullName);

		AddOption(sSubObjName, TRUE, index);
	}
}

//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::OnSelectCombo()
{
	CBCGPProp::OnSelectCombo();

	if (m_pPropertyView->GetDocument()->m_bPlayback || m_pPropertyView->GetDocument() -> m_bEngineRunning) return;

	if (this->IsValueChanged() || ((CString)GetValue()).IsEmpty())
	{
		int index = GetSelectedOption();
		if (index >= 0)
		{
			int listIndex = (int)GetOptionData(index);

			if (listIndex == -1) //Unload template
			{
				m_pPropertyView->GetDocument()->GetWoormFrame()->PostMessage(WM_COMMAND, ID_UNLOAD_TEMPLATE);
				return;
			}

			CString strPathName = m_arStrFullNameTmpl.GetAt(listIndex);

			SetTemplate(strPathName);
		}

	}
}

//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::SetTemplate(CString strPathName)
{
	m_pImportStaticObjects->SetAsFalse();

	CWoormTemplate* m_pTemplate = &(m_pPropertyView->GetDocument()->m_Template);

	m_pPropertyView->GetDocument()->UnloadTemplate();

	m_pTemplate->m_sNsTemplate = AfxGetPathFinder()->GetNamespaceFromPath(strPathName).ToString();

	m_pPropertyView->GetDocument()->LoadTemplate(TRUE, FALSE, FALSE);

	ASSERT(m_pTemplate && m_pTemplate->m_pWoormTpl != NULL);
}

//-----------------------------------------------------------------------------
//implementazione metodo virtuale classe BCG chiamato nella on-click del pulsante
void CRSLayoutTemplateProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - dropdown
		CBCGPProp::OnClickButton(point);
	}

	else
	{
		// right - file system browser using parent property handling
		CRSFileNameProp::OnClickButton(point);

		SetTemplate(GetValue());
	}
}

//aggiorno il campo testuale della property
//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::UpdatePropertyValue()
{
	CString sCurrent, sPath;
	if (m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl)
	{
		sPath = AfxGetPathFinder()->GetFileNameFromNamespace(m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl->GetNamespace(), AfxGetLoginInfos()->m_strUserName);
		sCurrent = ::GetName(sPath);
	}

	SetValue((_variant_t)sCurrent);
}

//aggiorno il campo testuale della property
//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::UpdateValue()
{
	//non faccio niente
}

//disegna i due pulsanti
//-----------------------------------------------------------------------------
void CRSLayoutTemplateProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			rect.right = rect.left + rect.Width() / 2;

			// Draw combobox button at left
			CBCGPProp::OnDrawButton(pDC, rect);
		}

		else
		{
			rect.left = rect.right - rect.Width() / 2;

			//Draw custom folder find button
			CBCGPToolbarButton button;
			CBCGPVisualManager::BCGBUTTON_STATE state = FALSE ? CBCGPVisualManager::ButtonsIsHighlighted : CBCGPVisualManager::ButtonsIsRegular; //todo andrea: rivedere
			CBCGPVisualManager::GetInstance()->OnFillButtonInterior(pDC, &button, rect, state);
			m_image.DrawEx(pDC, rect, 0, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
			CBCGPVisualManager::GetInstance()->OnDrawButtonBorder(pDC, &button, rect, state);
		}
	}
}

//-----------------------------------------------------------------------------
//per disegnare due pulsanti
void CRSLayoutTemplateProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//================================CRSImportStaticObjects===================================
//-----------------------------------------------------------------------------
CRSImportStaticObjects::CRSImportStaticObjects(CRS_ObjectPropertyView* propertyView)
	:
	CrsProp(_TB("Import static objects"), (_variant_t)false, _TB("It allows you decide whether to import static objects of a template")),
	m_pPropertyView(propertyView)
{
	ASSERT_VALID(m_pPropertyView);
}

//-----------------------------------------------------------------------------
void CRSImportStaticObjects::SetAsFalse()
{
	SetValue((_variant_t)false);
}

//-----------------------------------------------------------------------------
void CRSImportStaticObjects::OnSelectCombo()
{
	CBCGPProp::OnSelectCombo();

	CString sPath;

	if (m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl)
		sPath = AfxGetPathFinder()->GetFileNameFromNamespace(m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl->GetNamespace(), AfxGetLoginInfos()->m_strUserName);

	m_pPropertyView->GetDocument()->UnloadTemplate();

	m_pPropertyView->GetDocument()->m_Template.m_sNsTemplate = AfxGetPathFinder()->GetNamespaceFromPath(sPath).ToString();

	BOOL import = (BOOL) this->GetValue() == 0 ? FALSE : TRUE;

	m_pPropertyView->GetDocument()->LoadTemplate(TRUE, FALSE, import);

	return;
}

//================================CREditDescriptionText========================
//-----------------------------------------------------------------------------
CRSEditDescriptionText::CRSEditDescriptionText(const CString& strName, TextRect* textRect, CRS_ObjectPropertyView* propertyView) : CrsProp(strName, (_variant_t)"")
{
	ASSERT_VALID(textRect);

	this->m_textRect = textRect;
	this->m_pPropertyView = propertyView;

	SetValue((_variant_t)m_textRect->GetText());
}

CRSEditDescriptionText::CRSEditDescriptionText()
	: CrsProp(L"empty", (_variant_t)"")
{ 
	ASSERT(FALSE);
}

CRSEditDescriptionText::~CRSEditDescriptionText() 
{
}

//-----------------------------------------------------------------------------
//metodo chiamato quando tolgo il focus dal valore della property che � stato modificato, 
//prima chiamo il base per aggiornare il valore, quindi aggiorno l'oggetto e il documento di woorm
BOOL CRSEditDescriptionText::OnUpdateValue()
{
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	CString value = this->GetValue();

	UpdateText(value);

	return baseUpdate;
}

//-----------------------------------------------------------------------------
//Aggiorno il valore della property text dell'oggeto textRect puntato
void CRSEditDescriptionText::UpdateText(CString newValue)
{
	if (m_textRect != NULL)
	{
		m_textRect->SetText(newValue);
		m_textRect->UpdateDocument();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, this->GetCurrentNode(), TRUE);
	}
}

//-----------------------------------------------------------------------------
//implementazione metodo virtuale classe BCG chiamato nella on-click del pulsante modifica testo
void CRSEditDescriptionText::OnClickButton(CPoint point)
{
	ASSERT_VALID(m_pPropertyView);
	if (!m_pPropertyView)
		return;

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	CString sText = this->GetValue();
	pEditView->EnableStringPreview();
	pEditView->LoadElement(&sText);

	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetValue((_variant_t)sText);

	UpdateText(sText);
}

//================================CRSHiddenProp========================
//-----------------------------------------------------------------------------
CRSHiddenProp::CRSHiddenProp(CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description)
	:
	CRSExpressionExtendedProp(pOwner, strName, ppExp, dataType, pSymTable,  pPropertyView, description, FALSE)
{
	m_bAllowEdit = FALSE;
	AddOption(_T("False"),TRUE, FALSE);
	AddOption(_T("True"),TRUE, TRUE);

	UpdatePropertyValue();
	SetStateImg(CRS_PropertyGrid::Img::QuestionMark);

	m_pWndList = new CBCGPPropList();

	BOOL shouldBeHidden =GetOptionData(GetSelectedOption());
	SAFE_DELETE(m_pWndList);

	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);
}

//-----------------------------------------------------------------------------
void CRSHiddenProp::UpdatePropertyValue()
{
	/*if (*m_ppExp && !(*m_ppExp)->IsEmpty())
	{
		CString expr = currentValue.IsEmpty()?(*m_ppExp)->ToString() : currentValue;
		SetValue((_variant_t)expr);
	}

	else*/ if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pBase = (BaseRect*)m_pOwner;
		SelectOption(GetOptionDataIndex(pBase->m_bHidden), FALSE);
	}

	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SelectOption(GetOptionDataIndex(pColObj->m_bIsHidden),FALSE);
		//SetValue(pColObj->m_bIsHidden == 0 ? (_variant_t)_TB("False") : (_variant_t)_TB("True"));
	}

	else
		SelectOption(0);
}

//-----------------------------------------------------------------------------
void CRSHiddenProp::UpdateDocument()
{
	BOOL shouldBeHidden = GetOptionData(GetSelectedOption()); //sthis->GetValue() == _TB("True") ? TRUE : FALSE;

	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);
	
	if (this->IsValueChanged())
	{
		if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBase = (BaseRect*)m_pOwner;
			pBase->SetHidden(shouldBeHidden);
			pBase->Redraw();
		}

		else if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
		{
			TableColumn* pColObj = (TableColumn*)m_pOwner;

			int index = pColObj->GetTable()->GetColumnIndexByPtr(pColObj);

			if (!*m_ppExp && pColObj->IsHidden() != shouldBeHidden)
				pColObj->GetTable()->SetColumnHiddenStatus(index, shouldBeHidden);

			if (!*m_ppExp && pColObj->IsHidden())
				pColObj->RemoveSplitter();

			if(shouldBeHidden && (!*m_ppExp || (*m_ppExp)->IsEmpty())) 
				pColObj->GetTable()->m_pDocument->m_pActiveRect->SetActive(pColObj->GetTable()->m_BaseRect);
			
			pColObj->GetTable()->m_pDocument->Invalidate();
			pColObj->GetTable()->m_pDocument->SetModifiedFlag();
			pColObj->GetTable()->Redraw();
			
			if (!shouldBeHidden) 
				pColObj->Redraw();
		}

		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, this->GetCurrentNode(), FALSE);
	}
}

//-----------------------------------------------------------------------------
//implementazione metodo virtuale classe BCG chiamato nella on-click del pulsante modifica espressione
void CRSHiddenProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - combobox
		CBCGPProp::OnClickButton(point);
	}

	else
	{
		// right - edit view
		CRSExpressionProp::OnClickButton(point);
		//aggiorna il valore della property con il valore dell'espressione
		UpdatePropertyValue();
	}
}

//-----------------------------------------------------------------------------
void CRSHiddenProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
void CRSHiddenProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw combobox button at left
				CBCGPProp::OnDrawButton(pDC, rect);
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			// Draw push button at right
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSHiddenProp::OnUpdateValue()
{
	//ottimizzazione:
	CString strOldValue = this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	CString strNewValue = this->GetValue();

	if (strOldValue != strNewValue) //solo se il valore della property � effettivamente cambiato
	{
		if (*m_ppExp && !(*m_ppExp)->IsEmpty())//era presente un espressione, mi devo sincerare l'utente voglia cancellarla per settare una visibilit� non dinamica
		{
			if (AfxTBMessageBox(_TB("This change will erase the dynamic expression currently set. Are you sure you want to proceed?"), MB_ICONWARNING | MB_YESNO) == IDNO)
			{
				//risetto il precedente valore(l'espressione) 
				SetValue((_variant_t)strOldValue);
				return baseUpdate;
			}

			//se l'utente � sicuro, cancello l'espressione settata precedentemente e procedo 
			SAFE_DELETE(*m_ppExp);
			*m_ppExp = NULL;
		}	

		UpdateDocument();
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSHiddenProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (*m_ppExp && !(*m_ppExp)->IsEmpty())
	{
		CrsProp::OnDrawStateIndicator(pDC, rect);	//draw question mark

		CString expr = (*m_ppExp)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	else
		SetOriginalDescription();
}

//================================CRSBoolProp==================================
//-----------------------------------------------------------------------------
CRSBoolProp::CRSBoolProp(CObject* pOwner, const CString& strName, BOOL* pValue, const CString& description)
	:
	CrsProp(strName, *pValue == 1 ? (_variant_t)true : (_variant_t)false, description),
	m_pOwner(pOwner),
	m_pBValue(pValue),
	m_pColumns(NULL) 
{
		
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
CRSBoolProp::CRSBoolProp(MultiColumnSelection* pColumns, const CString& strName, BOOL* pValue, const CString& description)
	:
	CrsProp(strName, *pValue == 1 ? (_variant_t)true : (_variant_t)false, description),
	m_pOwner(NULL),
	m_pBValue(pValue),
	m_pColumns(pColumns)
{
}

//-----------------------------------------------------------------------------
BOOL CRSBoolProp::OnUpdateValue()
{
	BOOL previousValue = this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	BOOL value = this->GetValue();

	*m_pBValue = value == 0 ? FALSE : TRUE;
	if (m_pOwner)
	{
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
		{
			pGenObj->Redraw();
			return baseUpdate;
		}

		WoormIni* pWoormIni = dynamic_cast<WoormIni*>(m_pOwner);
		if (pWoormIni)
			pWoormIni->WriteWoormSettings();

	}

	else if (m_pColumns)
		m_pColumns->Redraw();
		
	return baseUpdate;
}

//================================CRSBoolPropWithDepListToDisable==================================
//-----------------------------------------------------------------------------
CRSBoolPropWithDepListToDisable::CRSBoolPropWithDepListToDisable(CObject* pOwner, const CString& strName, BOOL* pValue, CList<CBCGPProp*>* pDepList, const CString& description, BOOL bInvertBehaviour)
	:
	CRSBoolProp(pOwner, strName, pValue, description),
	m_pDepList(pDepList),
	m_bInvertBehaviour(bInvertBehaviour)
{
	SetDepPropsVisibile();
}

//-----------------------------------------------------------------------------
CRSBoolPropWithDepListToDisable::~CRSBoolPropWithDepListToDisable()
{
	SAFE_DELETE(m_pDepList);
}

//-----------------------------------------------------------------------------
BOOL CRSBoolPropWithDepListToDisable::OnUpdateValue()
{
	BOOL baseUpdate = CRSBoolProp::OnUpdateValue();

	SetDepPropsVisibile();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSBoolPropWithDepListToDisable::SetDepPropsVisibile()
{
	BOOL bEnable = (BOOL)GetValue() ? TRUE : FALSE;
	if (m_bInvertBehaviour)
		bEnable = !bEnable;
	POSITION pos;

	pos = m_pDepList->GetHeadPosition();
	while (pos)
	{
		CBCGPProp* prop = m_lstSubItems.GetNext(pos);

		CrsProp* crsProp = dynamic_cast<CrsProp*>(prop);
		CRSColorProp* colorProp = dynamic_cast<CRSColorProp*>(prop);
		if (crsProp)
			crsProp->SetEnable(bEnable);
		else if (colorProp)
			colorProp->SetEnable(bEnable);
	}
}

//================================CRSBoolPropWithRefresh==================================
//-----------------------------------------------------------------------------
CRSBoolPropWithLayoutRefresh::CRSBoolPropWithLayoutRefresh(CObject* pOwner, const CString& strName, BOOL* pValue, const CString& description)
	:
	CRSBoolProp(pOwner, strName, pValue, description)
{
}

//-----------------------------------------------------------------------------
CRSBoolPropWithLayoutRefresh::CRSBoolPropWithLayoutRefresh(MultiColumnSelection* pColumns, const CString& strName, BOOL* pValue, const CString& description)
	:
	CRSBoolProp(pColumns, strName, pValue, description)
{
}

//-----------------------------------------------------------------------------
BOOL CRSBoolPropWithLayoutRefresh::OnUpdateValue()
{
	BOOL baseUpdate = CRSBoolProp::OnUpdateValue();

	CRS_ObjectPropertyView* propView = GetPropertyView();
	if (propView)
	{
		CWoormDocMng* doc = propView->GetDocument();
		if (doc)
			doc->Invalidate(TRUE);
	}

	return baseUpdate;
}

//================================CRSMultilineColumnProp==================================
//-----------------------------------------------------------------------------
CRSMultilineColumnProp::CRSMultilineColumnProp(TableColumn* pCol, BOOL bValue, CRSColumnAlignBitwiseProp* pAlignment, CRSColumnTypeProp* pType)
	:
	CrsProp(_TB("Distribute over multiline if too long"), bValue == 1 ? (_variant_t)true : (_variant_t)false, _TB("If text does not fit one line, it divides the text and generates successive lines. It force aligment 'singleline' flag")),
	m_pAlignment(pAlignment),
	m_pType(pType),
	m_pCol(pCol) 
{
	ASSERT_VALID(m_pCol);
}

//-----------------------------------------------------------------------------
BOOL CRSMultilineColumnProp::OnUpdateValue()
{
	int nPrevValue = GetValue();
	BOOL baseUpdate = __super::OnUpdateValue();
	int nValue = GetValue();

	
	BOOL prevValue = nPrevValue == -1;
	BOOL value = nValue == -1;

	//ottimizzazione
	if (value == prevValue)
	return baseUpdate;

	m_pCol->m_bMultipleRow = value;
	// forza stili di allignement congruenti con l'attributo MultipleRows
	//m_pCol->m_bMixedColumnAlign = TRUE;

	m_pCol->SetFieldWidth();
	m_pCol->GetDocument()->InvalidateRect(m_pCol->GetColumnRect(), m_pCol->GetTable()->m_bTransparent);
	m_pCol->GetDocument()->UpdateWindow();

	//-----------------------------------------------------------

	if (m_pAlignment)
		m_pAlignment->RedrawSingleLineProp();

	if (m_pType)
	{
		if (m_pCol->IsMultiRow() && m_pCol->m_ShowAs == EFieldShowAs::FT_IMAGE)
		{
			m_pType->SetColoredState(CrsProp::State::Error);
			m_pType->SetNewDescription(_TB("This setting is not compatible with the property \"Distribute over multiline if too long\""));
		}

		else
		{
			m_pType->SetColoredState(Normal);
			m_pType->SetOriginalDescription();
		}
	}

	return baseUpdate;
}

//================================CRSMultilineMultiColumnsProp==================================
//-----------------------------------------------------------------------------
CRSMultilineMultiColumnsProp::CRSMultilineMultiColumnsProp(MultiColumnSelection* pMulCols, CRSMultiColumnsAlignmentStyleBitWiseProp* pAlignment)
	:
	CrsProp(_TB("Distribute over multiline if too long"), L"", _TB("If text does not fit one line, it divides the text and generates successive lines")),
	m_pMulCols(pMulCols),
	m_pMulAlignment(pAlignment)
{
	ASSERT_VALID(m_pMulCols);

	AddOption(_T("True"));
	AddOption(_T("False"));

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSMultilineMultiColumnsProp::UpdatePropertyValue()
{
	int i;
	BOOL bCommonValue = FALSE;
	BOOL bSingleValue = FALSE;
	TableColumn* pCol;
	for (i = 0; i < m_pMulCols->GetSize(); i++)
	{
		pCol = m_pMulCols->GetAt(i);

		bSingleValue = pCol->IsMultiRow();

		if (i == 0)
			bCommonValue = bSingleValue;
		else if (bCommonValue != bSingleValue) break;
	}

	if (i == m_pMulCols->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetValue(bCommonValue ? _T("True") : _T("False"));

	//altrimenti lascio vuoto
	else SetValue(L"");
}

//-----------------------------------------------------------------------------
BOOL CRSMultilineMultiColumnsProp::OnUpdateValue()
{
	CString prevValue = GetValue();
	BOOL baseUpdate = __super::OnUpdateValue();
	CString value = GetValue();

	//ottimizzazione
	if (value == prevValue)
		return baseUpdate;

	TableColumn* pCol;
	for (int i = 0; i < m_pMulCols->GetSize(); i++)
	{
		pCol = m_pMulCols->GetAt(i);
		if (!pCol->CanBeMultiRow())
			continue;

		pCol->m_bMultipleRow = value == _T("True") ? TRUE: FALSE;

		//pCol->m_bMixedColumnAlign = true;

		pCol->SetFieldWidth();
	}

	if (m_pMulAlignment)
		m_pMulAlignment->RedrawSingleLineProp();

	m_pMulCols->Redraw();

	return baseUpdate;
}

//================================CRSShowColumnTotalProp===================================
//-----------------------------------------------------------------------------
CRSShowColumnTotalProp::CRSShowColumnTotalProp(TableColumn* pCol, CRS_ObjectPropertyView* propertyView)
	:
	CRSBoolProp(pCol, _TB("Show total"), &(pCol->m_bShowTotal), _TB("Flag to set the visibility of the total of this column")),
	m_pCol(pCol),
	m_pPropertyView(propertyView)
{
	ASSERT_VALID(m_pPropertyView);
	ASSERT_VALID(m_pPropertyView->m_pPropGrid);

	DrawProperties(*m_pBValue);//pCol->m_bShowTotal
}

//-----------------------------------------------------------------------------
BOOL CRSShowColumnTotalProp::OnUpdateValue()
{
	BOOL previousValue = this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	BOOL value = this->GetValue();

	if (previousValue == value) //se il valore non � cambiato esco
		return baseUpdate;

	if (m_pWndList != NULL)
		value = GetSelectedOption();

	DrawProperties(value);

	//aggiorno il tree per aggiungere o rimuovere il totale dai figli della colonna
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, m_pCol);

	return baseUpdate;
	/*return TRUE;*/
}

//----------------------------------------------------------------------------- todo andrea: rivedere posizionamento tendina della combo
void CRSShowColumnTotalProp::DrawProperties(BOOL showTotal)
{
	this->m_bGroup = TRUE;

	if (this->GetSubItemsCount() > 0)
		this->RemoveAllSubItems();

	if (showTotal)
	{
		//show Total
		m_pCol->AddColumnTotal();
		//m_pCol->GetTable()->m_pDocument->InvalidateRect(m_pCol->GetTable()->AllTotalsRect(), TRUE);
		//m_pCol->GetTable()->m_pDocument->UpdateWindow();

		//bkg color
		AddSubItem(new CRSColorWithExprProp(m_pCol, _TB("Background Color"), &(m_pCol->m_pTotalCell->m_Value.m_rgbBkgColor), &(m_pCol->m_pTotalBkgColorExpr), _TB("Background Color of total cell"), m_pPropertyView));
		//Text Color
		AddSubItem(new CRSColorWithExprProp(m_pCol, _TB("Text Color"), &(m_pCol->m_pTotalCell->m_Value.m_rgbTextColor), &(m_pCol->m_pTotalTextColorExpr), _TB("Text Color of total cell"), m_pPropertyView));
		//TotalCell borderpen
		CrsProp* pBorderGroup = new CrsProp(_TB("Border"), 0, FALSE);
		//color
		CRSColorProp* borderColor = new CRSColorProp(m_pCol, _TB("Color"), &(m_pCol->m_pTotalCell->m_TotalPen.m_rgbColor), _TB("Border Color of the total cell"));
		pBorderGroup->AddSubItem(borderColor);
		borderColor->SetOwnerList(m_pPropertyView->m_pPropGrid);

		//size
		CRSIntProp* sizeColor = new CRSIntProp(m_pCol, _TB("Size"), &(m_pCol->m_pTotalCell->m_TotalPen.m_nWidth), _TB("Border size of the total cell"));
		pBorderGroup->AddSubItem(sizeColor);
		sizeColor->SetOwnerList(m_pPropertyView->m_pPropGrid);

		//add to parent property
		AddSubItem(pBorderGroup);
	}

	m_pPropertyView->m_pPropGrid->AdjustLayout();

	//in ogni caso devo riselezionare la colonna per includere/escludere il rettangolo di selezione
	if (!m_pCol->IsHidden())
	{
		m_pCol->GetTable()->m_pDocument->m_pActiveRect->SetActive(m_pCol->GetTable()->GetActiveRect());
		m_pCol->GetTable()->m_pDocument->m_pActiveRect->Redraw();
	}

	this->m_bGroup = FALSE;
}

//================================CRSIntProp===================================
//-----------------------------------------------------------------------------
CRSIntProp::CRSIntProp(CObject* pOwner, const CString& strName, int* pValue, const CString& description, int fromValue, int toValue, CooType type)
	:
	CrsProp(strName, *pValue, description),
	m_pOwner(pOwner),
	m_pValue(pValue) ,
	type(type)
{
	ASSERT_VALID(m_pOwner);
	
	if (toValue > 0)
		EnableSpinControl(TRUE, fromValue, toValue);
}

//-----------------------------------------------------------------------------
BOOL CRSIntProp::OnUpdateValue()
{
	int prevValue = *m_pValue;
	BOOL baseUpdate = __super::OnUpdateValue();
	//limite minvalue < x < maxvalue anche sull'input manuale con tastiera-----------
	int value = GetValue();
	if (value > m_nMaxValue && m_nMaxValue)
	{
		SetValue(m_nMaxValue);
		value = m_nMaxValue;
	}	
	else if (value < m_nMinValue)
	{
		SetValue(m_nMinValue);
		value = m_nMinValue;
	}

	//-------------------------------------------------------------------------------
	//ottimizzazione per evitare di ridisegnare l'oggetto se il valore in realt� non � cambiato
	if (value == prevValue)
		return baseUpdate;

	*m_pValue = value;

	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj) 
		pGenObj->Redraw();

	CBCGPProp* parent = GetParent();

	switch (type)
	{
	case CooType::XP:
	{
		if (parent)
		{
			int val =(int)floor(LPtoMU((LONG)*m_pValue, CM, 10., 3));
			parent->GetSubItem(2)->SetValue(val);
			*((CRSIntProp*)(parent->GetSubItem(2)))->m_pValue = val;
		}

		break;
	}

	case CooType::YP:
	{
		if (parent)
		{
			int val = (int)floor(LPtoMU((LONG)*m_pValue, CM, 10., 3));
			parent->GetSubItem(3)->SetValue(val);
			*((CRSIntProp*)(parent->GetSubItem(3)))->m_pValue = val;
		}

		break;
	}

	case CooType::XM:
	{
		if (parent)
		{
			int val = MUtoLP((LONG)*m_pValue*10, CM, MU_SCALE, MU_DECIMAL);
			parent->GetSubItem(0)->SetValue(val);
			*((CRSIntProp*)(parent->GetSubItem(2)))->m_pValue = val;
		}

		break;
	}

	case CooType::YM:
	{
		if (parent)
		{
			int val = MUtoLP((LONG)*m_pValue * 10, CM, MU_SCALE, MU_DECIMAL);
			parent->GetSubItem(1)->SetValue(val);
			*((CRSIntProp*)(parent->GetSubItem(1)))->m_pValue = val;
		}

		break;
	}

	case CooType::DEFAULT:
	{
		//TODO aggiungere nuovi valori CooType per identificare differenti campi

		WoormIni* pWoormIni = dynamic_cast<WoormIni*>(m_pOwner);
		if (pWoormIni)
		{
			pWoormIni->WriteWoormSettings();
			CWoormDocMng* wrmDocMng = GetDocument();
			if (wrmDocMng)
				wrmDocMng->SetAutoSaveTimer();
			break;
		}

		CWoormDocMng* pWoorm = dynamic_cast<CWoormDocMng*>(m_pOwner);
		if (pWoorm)
		{
			pWoorm->Invalidate();
			break;
		}

		break;
	}
	}

	return baseUpdate;
}

//================================CRSRepeaterCount===================================
//-----------------------------------------------------------------------------
CRSRepeaterCount::CRSRepeaterCount(Repeater* pRepeater, int* pCount, PropertyType ePropType)
	:
	CRSIntProp( pRepeater, L"", pCount, L"", 1, 100),
	m_pRepeater(pRepeater),
	m_ePropType(ePropType)
{
	switch (m_ePropType)
	{
	case CRSRepeaterCount::Row:
		SetName(_TB("Row Count"));
		break;
	case CRSRepeaterCount::Col:
		SetName(_TB("Columns Count"));
		break;
	}
}

//-----------------------------------------------------------------------------
BOOL CRSRepeaterCount::OnUpdateValue()
{
	CRSIntProp::OnUpdateValue();
	int value = (int)GetValue();
	//a questo punto il numero delle righe o delle colonne potrebbe essere stato adattato alla dimensione della pagina
	switch (m_ePropType)
	{
	case CRSRepeaterCount::Row:
	{
		int rows = m_pRepeater->m_nRows;
		if (value > rows)
		{
			SetValue(rows);
			*m_pValue = rows;
		}

		break;
	}

	case CRSRepeaterCount::Col:
	{
		int cols = m_pRepeater->m_nColumns;
		if (value > cols)
		{
			SetValue(cols);
			*m_pValue = cols;
		}

		break;
	}
	}

	return TRUE;
}

//================================CRSShortProp===================================
//-----------------------------------------------------------------------------
CRSShortProp::CRSShortProp(CObject* pOwner, const CString& strName, short* pValue, const CString& description, int fromValue, int toValue)
	:
	CrsProp(strName, *pValue, description),
	m_pOwner(pOwner),
	m_pValue(pValue)
{
	ASSERT_VALID(m_pOwner);

	EnableSpinControl(TRUE, fromValue, toValue);
}

//-----------------------------------------------------------------------------
BOOL CRSShortProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();
	short prevShadow = *m_pValue;
	*m_pValue = this->GetValue();

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(WoormIni)))
		((WoormIni*)(m_pOwner))->WriteWoormSettings();

	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	return baseUpdate;
}

//================================CRSDoubleProp===================================
//-----------------------------------------------------------------------------
CRSDoubleProp::CRSDoubleProp(CObject* pOwner, const CString& strName, double* pValue, const CString& description, int fromValue, int toValue)
	:
	CrsProp(strName, *pValue, description),
	m_pOwner(pOwner),
	m_pValue(pValue)
{
	ASSERT_VALID(m_pOwner);

	//EnableSpinControl(TRUE, fromValue, toValue);
}

//-----------------------------------------------------------------------------
BOOL CRSDoubleProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();
	double prevShadow = *m_pValue;
	*m_pValue = this->GetValue();

	WoormIni* pWI = dynamic_cast<WoormIni*>(m_pOwner);
	if (pWI)
		pWI->WriteWoormSettings();
	
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	return baseUpdate;
}

//================================CRSTableShadowProp===================================
//-----------------------------------------------------------------------------
CRSTableShadowProp::CRSTableShadowProp(Table* pTable)
	:
	CrsProp(_TB("Size"), pTable->m_nDropShadowHeight, _TB("The size in pixels of the shadow around the table")),
	m_pTable(pTable),
	m_pValue(&(pTable->m_nDropShadowHeight))
{
	ASSERT_VALID(m_pTable);
	EnableSpinControl(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CRSTableShadowProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int prevShadow = *m_pValue;

	*m_pValue = this->GetValue();

	m_pTable->Redraw(TRUE, prevShadow);

	return baseUpdate;
}

//================================CRSStringProp===================================
//-----------------------------------------------------------------------------
CRSStringProp::CRSStringProp(CObject* pOwner, const CString& strName, CString* pValue, const CString& description)
	:
	CrsProp(strName, (_variant_t)*pValue, description),
	m_pOwner(pOwner),
	m_pValue(pValue)
{
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
BOOL CRSStringProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	*m_pValue = this->GetValue();

	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	return baseUpdate;
}

//================================CRSColorProp=================================
//-----------------------------------------------------------------------------
CRSColorProp::CRSColorProp(CObject* pOwner, const CString& strName, COLORREF* pValue, const CString& description)
	:
	CBCGPColorProp(strName, *pValue, NULL, description),
	m_pOwner(pOwner),
	m_pValue(pValue),
	m_pColumns(NULL),
	m_strOriginalDescr(description)
{
	ASSERT_VALID(m_pOwner);
	EnableOtherButton(_TB("Other..."));
	//EnableAutomaticButton(_TB("Default"), ::GetSysColor(COLOR_3DFACE)); //disabilitato il bottone "Default"
	//nella popup della palette dei colori, perch� settava il colore nero anche se era stato spcificato un colore di default diverso
}

//-----------------------------------------------------------------------------
CRSColorProp::CRSColorProp(CObject* pOwner, const CString& strName, COLORREF value, const CString& description)
	:
	CBCGPColorProp(strName, value, NULL, description),
	m_pOwner(pOwner),
	m_pValue(NULL),
	m_pColumns(NULL),
	m_strOriginalDescr(description)
{
	ASSERT_VALID(m_pOwner);
	EnableOtherButton(_TB("Other..."));
	//EnableAutomaticButton(_TB("Default"), ::GetSysColor(COLOR_3DFACE)); //disabilitato il bottone "Default"
	//nella popup della palette dei colori, perch� settava il colore nero anche se era stato spcificato un colore di default diverso
}

//-----------------------------------------------------------------------------
CRSColorProp::CRSColorProp(MultiColumnSelection* p_Columns, const CString& strName, COLORREF value, const CString& description)
	:
	CBCGPColorProp(strName, value, NULL, description),
	m_pOwner(NULL),
	m_pValue(NULL),
	m_pColumns(p_Columns),
	m_strOriginalDescr(description)
{
	ASSERT(!p_Columns==NULL);
	EnableOtherButton(_TB("Other..."));
	//EnableAutomaticButton(_TB("Default"), ::GetSysColor(COLOR_3DFACE)); //disabilitato il bottone "Default"
	//nella popup della palette dei colori, perch� settava il colore nero anche se era stato spcificato un colore di default diverso
}

//-----------------------------------------------------------------------------
BOOL CRSColorProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	if (m_pValue)
	{
		//gli ho passato il puntatore e aggiorno io l'oggetto grafico, altrimenti sono in casi specifici e potrei voler aggiornare l'oggetto altrove (vedi multiselezione)
		*m_pValue = this->GetValue();

		if (m_pOwner)
		{
			TableCell* pCell = dynamic_cast<TableCell*>(m_pOwner);
			GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
			if (pCell && pCell->IsSubTotal())
				pCell->m_pColumn->Redraw();
			else if (pGenObj)
				pGenObj->Redraw();
		}

		else if (m_pColumns)
		{
			m_pColumns->Redraw();
		}
	}

	WoormIni* pWoormIni = dynamic_cast<WoormIni*>(m_pOwner);
	if (pWoormIni)
	{
		pWoormIni->WriteWoormSettings();
		if (GetPropertyView() && GetPropertyView()->GetDocument() && GetPropertyView()->GetDocument()->m_pActiveRect)
			GetPropertyView()->GetDocument()->m_pActiveRect->UpdateColor();
		GetPropertyView()->GetDocument()->Invalidate(TRUE);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSColorProp::SetEnable(BOOL bEnable, BOOL bIncludeChildren)
{
	Enable(bEnable);
	ASSERT_VALID(this);

	if (!bIncludeChildren) return;

	for (int i = 0; i < this->GetSubItemsCount(); i++)
	{
		CrsProp* prop = dynamic_cast<CrsProp*>(this->GetSubItem(i));
		CRSColorProp* colorProp = dynamic_cast<CRSColorProp*>(this->GetSubItem(i));
		if (prop)
			prop->SetEnable(bEnable);
		else if (colorProp)
			colorProp->SetEnable(bEnable);
	}
}

//-----------------------------------------------------------------------------
CRS_PropertyGrid* CRSColorProp::GetPropertyGrid()
{
	ASSERT_VALID(this->m_pWndList);

	CRS_PropertyGrid* pPropGrid = dynamic_cast<CRS_PropertyGrid*>(this->m_pWndList);
	if (!pPropGrid)
		ASSERT(FALSE);

	return pPropGrid;
}

//-----------------------------------------------------------------------------
CRS_ObjectPropertyView*	CRSColorProp::GetPropertyView()
{
	return GetPropertyGrid()->GetPropertyView();
}

//-----------------------------------------------------------------------------
void CRSColorProp::SetOriginalDescription()
{
	if (!m_strOriginalDescr)
		ASSERT(FALSE);
	SetDescription(m_strOriginalDescr);
}

//================================CRSColorWithExprProp==================================
//-----------------------------------------------------------------------------
CRSColorWithExprProp::CRSColorWithExprProp(CObject* pOwner, const CString& strName, COLORREF* value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorProp(pOwner, strName, value, description),
	m_ppExp(ppExp),
	m_pPropertyView(propertyView),
	m_strDescr(description)
{
	InitProp();
}

//-----------------------------------------------------------------------------
CRSColorWithExprProp::CRSColorWithExprProp(CObject* pOwner, const CString& strName, COLORREF value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorProp(pOwner, strName, value, description),
	m_ppExp(ppExp),
	m_pPropertyView(propertyView),
	m_strDescr(description)
{
	InitProp();
}

//-----------------------------------------------------------------------------
CRSColorWithExprProp::CRSColorWithExprProp(MultiColumnSelection* pMulCol, const CString& strName, COLORREF value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorProp(pMulCol, strName, value, description),
	m_ppExp(ppExp),
	m_pPropertyView(propertyView),
	m_strDescr(description)
{
	InitProp();
}

CRSColorWithExprProp::~CRSColorWithExprProp()
{}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::InitProp()
{
	AllowEdit(FALSE);
	m_bHasState = TRUE;

	m_imageExpr.SetImageSize(CSize(14, 14));
	m_imageExpr.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageExpr);
	HICON icon = TBLoadPng(TBGlyph(szGlyphQuestionMark));
	m_imageExpr.AddIcon(icon);
	::DestroyIcon(icon);
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::OnClickButton(CPoint point)
{
	m_ColorOrig = m_Color;

	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - combobox
		CBCGPColorProp::OnClickButton(point);
	}

	else
	{
		OnRightButtonClick();
	}
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::OnRightButtonClick()
{
	// right - edit view
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		m_ppExp,
		DataType::Long,
		TRUE
		);
	pEditView->DoEvent();

	ASSERT_VALID(this);
	if (!this) return;

	if (m_ppExp && *m_ppExp && (*m_ppExp)->IsEmpty())
		SAFE_DELETE(*m_ppExp);

	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw combobox button at left
				CBCGPProp::OnDrawButton(pDC, rect);
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			// Draw push button at right
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
		CString expr = (*m_ppExp)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	else
		SetOriginalDescription();
}

//todo andrea: vedere se � meglio rimuovere il metodo seguente e inserirne il corpo nell'if sopra
//-----------------------------------------------------------------------------
BOOL CRSColorWithExprProp::HasExpression()
{
	return *m_ppExp && !(*m_ppExp)->IsEmpty();
}

//-----------------------------------------------------------------------------
void CRSColorWithExprProp::SetVisible(BOOL visible)
{
	m_bIsVisible = visible;

	ASSERT_VALID(this);
	for (int i = 0;i < this->GetSubItemsCount();i++)
	{
		CrsProp* prop = dynamic_cast<CrsProp*>(this->GetSubItem(i));
		CRSColorWithExprProp* colorProp = dynamic_cast<CRSColorWithExprProp*>(this->GetSubItem(i));
		if (prop)
			prop->SetVisible(visible);
		else if (colorProp)
			colorProp->SetVisible(visible);
	}
}

//================================CRSTableColumnColorWithExprProp==================================
//-----------------------------------------------------------------------------
CRSTableColumnColorWithExprProp::CRSTableColumnColorWithExprProp(TableColumn* pCol, PropertyType propType, Expression** ppExp, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorWithExprProp(pCol, L"", RGB(0, 0, 0), ppExp, L"", propertyView),
	m_pCol(pCol),
	m_propType(propType)
{
	ASSERT_VALID(m_pCol);

	switch (m_propType)
	{
	case(PropertyType::BackColor) :
	{
		m_pCol->GetAllCellsBkgColor(m_Color);

		SetName(_TB("Background Color"), FALSE);
		SetDescription(_TB("Set the background Color of this column"));

		break;
	}

	case(PropertyType::ForeColor) :
	{
		m_pCol->GetAllCellsTextColor(m_Color);

		SetName(_TB("Text Color"), FALSE);
		SetDescription(_TB("Set the Text Color of this column"));

		break;
	}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableColumnColorWithExprProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	if (m_Color)
	{
		switch (m_propType)
		{
		case(PropertyType::BackColor) :
		{
			m_pCol->SetAllCellsBkgColor(m_Color);
			break;
		}

		case(PropertyType::ForeColor) :
		{
			m_pCol->SetAllCellsTextColor(m_Color);
			break;
		}
		}
	}

	//aggiorno il report
	m_pCol->UpdateDocument();

	return baseUpdate;
}

//================================CRSStringWithExprProp==================================
//-----------------------------------------------------------------------------
CRSStringWithExprProp::CRSStringWithExprProp(CObject* pOwner, const CString& strName, CString* value, Expression** ppExp, SymTable* pSymTable, const CString& description, CRS_ObjectPropertyView* propertyView, BOOL bUpdateNodeTree)
	:
	CRSStringProp(pOwner, strName, value, description),
	m_pPropertyView(propertyView),
	m_ppExp(ppExp),
	m_psymTable(pSymTable),
	m_bUpdateNodeTree(bUpdateNodeTree)
{
	AllowEdit(TRUE);
	m_bHasState = TRUE;

	m_bImgVisible = *m_ppExp && !(*m_ppExp)->IsEmpty();

	m_imageExpr.SetImageSize(CSize(14, 14));
	m_imageExpr.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageExpr);
	HICON icon = TBLoadPng(TBGlyph(szGlyphQuestionMark));
	m_imageExpr.AddIcon(icon);
	::DestroyIcon(icon);
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::OnClickButton(CPoint point)
{
		// right - edit view

		ASSERT_VALID(m_pPropertyView);
		if (!m_pPropertyView) return;

		CRSEditView* pEditView = m_pPropertyView->CreateEditView();
		if (!pEditView)
			return;
		pEditView->EnableStringPreview();
		pEditView->LoadElement(
			m_psymTable,
			m_ppExp,
			DataType::String,
			TRUE
			);
		pEditView->DoEvent();
		ASSERT_VALID(this);
		if (m_ppExp && *m_ppExp)
			m_bImgVisible = *m_ppExp && !(*m_ppExp)->IsEmpty();
		//aggiorno il report
		UpdateRect();
		//ridisegno lo state (quindi l'immagine se ci deve essere)
		RedrawState();
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::UpdateRect()
{
	//aggiorno il report
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (!pGenObj)
		m_pPropertyView->m_pPropGrid->AdjustLayout();

	if (m_bUpdateNodeTree)
	{
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, GetCurrentNode(), FALSE);
	}
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::ValorizePropertyIfNecessary()
{
	CString val = GetValue();

	//aggiorno il report
	TableColumn* pCol = dynamic_cast<TableColumn*>(m_pOwner);
	if (pCol)
	{
		if (val == (_variant_t)pCol->GetFieldName())
		{
			this->SetColoredState(CrsProp::State::Important);
			CBCGPProp* parentProp = this->GetParent();
			if (parentProp->IsGroup())
				parentProp->Expand(TRUE);
		}
	}

	FieldRect* pField = dynamic_cast<FieldRect*>(m_pOwner);
	if (pField)
	{
		if (val == (_variant_t)pField->GetFieldName())
		{
			this->SetColoredState(CrsProp::State::Important);
			CBCGPProp* parentProp = this->GetParent();
			if (parentProp->IsGroup())
				parentProp->Expand(TRUE);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSStringWithExprProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	if (m_bUpdateNodeTree)
	{
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, GetCurrentNode(), FALSE);
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Dialogs, GetCurrentNode(), FALSE);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw combobox button at left
				CBCGPProp::OnDrawButton(pDC, rect);
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			// Draw push button at right
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CRSStringWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (m_bImgVisible)
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
		if (*m_ppExp && !(*m_ppExp)->IsEmpty())
		{
			CString expr = (*m_ppExp)->ToString();
			SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
		}

		else
			SetOriginalDescription();
	}

	else
		SetOriginalDescription();
}

//================================CRSDialogWithExprProp==================================
//-----------------------------------------------------------------------------
CRSDialogWithExprProp::CRSDialogWithExprProp(CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, PropertyType propType, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CrsProp(strName, (_variant_t)"", description),
	m_pOwner(pOwner),
	m_ppExp(ppExp),
	m_dataType(dataType),
	m_psymTable(pSymTable),
	m_propType(propType),
	m_pPropertyView(propertyView)
{
	AllowEdit(FALSE);
	m_bHasState = TRUE;
	//needs this setting to draw the left button
	m_dwFlags = TRUE;

	//set property value
	UpdatePropertyValue();

	m_imageExpr.SetImageSize(CSize(14, 14));
	m_imageExpr.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageExpr);
	HICON icon = TBLoadPng(TBGlyph(szGlyphQuestionMark));
	m_imageExpr.AddIcon(icon);
	::DestroyIcon(icon);

	m_strDescr = description;
}

//-----------------------------------------------------------------------------
void CRSDialogWithExprProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
void CRSDialogWithExprProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		OnLeftClick();
	}

	else
	{
		OnRightClick();
	}
}

//-----------------------------------------------------------------------------
//Open dialog 
void CRSDialogWithExprProp::OnLeftClick()
{
	switch (m_propType)
	{
	case(PropertyType::FieldValueFormat) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			return;
		FieldRect* pField = (FieldRect*)m_pOwner;
		pField->SetFormatStyle();
		break;
	}

	case(PropertyType::TableColumnBodyFormat) : {
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			return;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		pColObj->GetTable()->OnColumnFormatStyle();
		break;
	}

	case(PropertyType::TableColumnBodyFont) : {
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			return;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		pColObj->GetTable()->OnColumnFontStyle();
		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//Open expression editor 
//todo andrea: refactoring per pulire un p� il metodo? 
void CRSDialogWithExprProp::OnRightClick()
{
	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;
	if (m_dataType == DataType::String || m_dataType == DataType::Text)
		pEditView->EnableStringPreview();
	pEditView->LoadElement(
		m_psymTable,
		m_ppExp,
		m_dataType,
		TRUE
		);

	pEditView->DoEvent();
	ASSERT_VALID(this);
	if (!this) return;
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

//-----------------------------------------------------------------------------
void CRSDialogWithExprProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw left Button 
				COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

				CString str = m_strButtonText;
				if (!str.IsEmpty())
				{
					COLORREF clrTextOld = pDC->SetTextColor(clrText);
					rect.DeflateRect(2, 2);
					rect.bottom--;
					pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
					pDC->SetTextColor(clrTextOld);
				}
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			//Draw Right Button 
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//-----------------------------------------------------------------------------
//todo unificare il codice dei due if
void CRSDialogWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (*m_ppExp && !(*m_ppExp)->IsEmpty())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);

		CString expr = (*m_ppExp)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
//aggiorna il valore associato alla property, ovvero la descrizione del formato
void CRSDialogWithExprProp::UpdatePropertyValue()
{
	switch (m_propType)
	{
	case(PropertyType::FieldValueFormat) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			return;
		FieldRect* pField = (FieldRect*)m_pOwner;
		SetValue((_variant_t)GetFormatName(pField->m_nFormatIdx));
		break;
	}

	case(PropertyType::TableColumnBodyFormat) : {
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			return;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SetValue((_variant_t)GetFormatName(pColObj->m_nFormatIdx));
		break;
	}

	case(PropertyType::TableColumnBodyFont) : {
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			return;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SetValue((_variant_t)GetFontName(pColObj->m_nFontIdx));
		break;
	}

	}
}

//-----------------------------------------------------------------------------
//metodo per aggiornare il format name della property
CString CRSDialogWithExprProp::GetFormatName(FormatIdx formatIdx)
{
	CString formatName = _TB("Error retrieving format name");

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pBase = (BaseRect*)m_pOwner;
		formatName = pBase->m_pDocument->m_pFormatStyles->GetStyleName(formatIdx);
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		formatName = pColObj->GetTable()->m_pDocument->m_pFormatStyles->GetStyleName(formatIdx);
	}

	return formatName;
}

//-----------------------------------------------------------------------------
//metodo per aggiornare il font name della property
CString CRSDialogWithExprProp::GetFontName(FontIdx fontIdx)
{
	CString fontName = _TB("Error retrieving font name");

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pBase = (BaseRect*)m_pOwner;
		fontName = pBase->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTabObj = (Table*)m_pOwner;
		fontName = pTabObj->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		fontName = pColObj->GetTable()->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
	}

	return fontName;
}

//================================CRSRectProp==================================
//-----------------------------------------------------------------------------
CRSRectProp::CRSRectProp(CObject* pOwner, const CString& strName, const CString& description, CRSRectProp::PropertyType propType, int fromValue, int toValue)
	:
	CrsProp(strName, (LONG)0, description),
	m_pOwner(pOwner),
	m_propType(propType)
{
	ASSERT_VALID(m_pOwner);

	EnableSpinControl(TRUE, fromValue, toValue);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSRectProp::UpdatePropertyValue() {

	CRect * m_pRect = &((BaseObj*)m_pOwner)->GetBaseRect();
	switch (m_propType)
	{
	case CRSRectProp::PropertyType::LocationXP:
	{
		SetValue(m_pRect->left);
		break;
	}

	case CRSRectProp::PropertyType::LocationYP:
	{
		SetValue(m_pRect->top);
		break;
	}

	case CRSRectProp::PropertyType::LocationXM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pRect->left, CM, 10., 3));
		SetValue(val);
		break;
	}

	case CRSRectProp::PropertyType::LocationYM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pRect->top, CM, 10., 3));
		SetValue(val);
		break;
	}

	case CRSRectProp::PropertyType::WidthP:
	{
		SetValue(m_pRect->right - m_pRect->left);
		break;
	}

	case CRSRectProp::PropertyType::HeightP:
	{
		SetValue(m_pRect->bottom - m_pRect->top);
		break;
	}

	case CRSRectProp::PropertyType::WidthM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pRect->right - m_pRect->left, CM, 10., 3));
		SetValue(val);
		break;
	}

	case CRSRectProp::PropertyType::HeightM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pRect->bottom - m_pRect->top, CM, 10., 3));
		SetValue(val);
		break;
	}

	default:
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void CRSRectProp::UpdateLocationX(LONG previousValue, LONG currValue)
{
	//se � un repeater
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		Repeater* pRepObj = (Repeater*)m_pOwner;
		LONG diff = currValue - previousValue;
		//pRepObj->MoveBaseRect(diff, 0, TRUE); // todo andrea, capire perch� con questa chiamata gli oggetti interni si muovono pi� dei contenitori (questa chiamata prevede di mettere in else l'if successivo)
		pRepObj->MoveObjects(diff, 0, TRUE); //muove solo gli oggetti interni, poi nel secondo if muovo l'oggetto ma mi perso l'ombra dei repeaters
	}

	//se � un baseRect, aggiorno direttamente il rect
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		CRect rect = ((BaseRect*)m_pOwner)->GetBaseRect();

		int width = rect.Width();

		rect.TopLeft().x = currValue;

		rect.BottomRight().x = rect.TopLeft().x + width;

		((BaseRect*)m_pOwner)->SetBaseRect(rect);
	}

	//altrimenti, se � una tabella, chiamo direttamente il metodo moveObject, che si occupa di tutto
	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTabObj = (Table*)m_pOwner;
		LONG diff = currValue - previousValue;
		CSize offset(diff, 0);
		pTabObj->MoveObject(offset);
		// refresh active status if this are current
		if (pTabObj->m_pDocument->m_pCurrentObj == pTabObj)
			pTabObj->m_pDocument->m_pActiveRect->SetActive(pTabObj->GetActiveRect());

		pTabObj->Redraw(TRUE, 0, diff);
	}
}

//-----------------------------------------------------------------------------
void CRSRectProp::UpdateLocationY(LONG previousValue, LONG currValue)
{
	//se � un repeater
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		Repeater* pRepObj = (Repeater*)m_pOwner;
		LONG diff = (LONG)currValue - previousValue;
		//pRepObj->MoveBaseRect(0, diff, TRUE); // todo andrea, capire perch� con questa chiamata gli oggetti interni si muovono pi� dei contenitori (questa chiamata prevede di mettere in else l'if successivo)
		pRepObj->MoveObjects(0, diff, TRUE);//muove solo gli oggetti interni, poi nel secondo if muovo l'oggetto ma mi perso l'ombra dei repeaters
	}

	//se � un baseRect, aggiorno direttamente il rect
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		CRect rect = ((BaseRect*)m_pOwner)->GetBaseRect();

		int height = rect.Height();

		rect.TopLeft().y = currValue;

		rect.BottomRight().y = rect.TopLeft().y + height;

		((BaseRect*)m_pOwner)->SetBaseRect(rect);
	}

	//altrimenti, se � una tabella, chiamo direttamente il metodo moveObject, che si occupa di tutto
	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTabObj = (Table*)m_pOwner;
		LONG diff = currValue - previousValue;
		CSize offset(0, diff);
		pTabObj->MoveObject(offset);
		pTabObj->Redraw(TRUE, 0, diff);
	}
}

//-----------------------------------------------------------------------------
void CRSRectProp::UpdateWidth(LONG currValue)
{
	
	CRect rect = ((BaseObj*)m_pOwner)->GetBaseRect();

	int pLeftX = rect.TopLeft().x;
	rect.BottomRight().x = pLeftX + currValue;
	((BaseObj*)m_pOwner)->SetBaseRect(rect);
	//se � un repeater aggiorno anche gli oggetti contenuti e ripetuti
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		Repeater* pRepObj = (Repeater*)m_pOwner;
		pRepObj->ChangedAction();
	}
}

//-----------------------------------------------------------------------------
void CRSRectProp::UpdateHeight(LONG currValue)
{
	CRect rect = ((BaseObj*)m_pOwner)->GetBaseRect();
	int pTopY = rect.TopLeft().y;
	rect.BottomRight().y = pTopY + currValue;
	((BaseObj*)m_pOwner)->SetBaseRect(rect);

	//se � un repeater aggiorno anche gli oggetti contenuti e ripetuti
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		Repeater* pRepObj = (Repeater*)m_pOwner;
		pRepObj->ChangedAction();
	}
}

//-----------------------------------------------------------------------------
//Updates the object values associated to this property
void CRSRectProp::UpdateObjectValue(LONG previousValue/* = 0*/)
{
	CBCGPProp* parent = GetParent();
	switch (m_propType)
	{
	case CRSRectProp::PropertyType::LocationXP:
	{
		UpdateLocationX(previousValue, (LONG)GetValue());
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU((LONG)GetValue(), CM, 10., 3));
			parent->GetSubItem(2)->SetValue(val);
		}

		break;
	}

	case CRSRectProp::PropertyType::LocationYP:
	{
		UpdateLocationY(previousValue, (LONG)GetValue());
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU((LONG)GetValue(), CM, 10., 3));
			parent->GetSubItem(3)->SetValue(val);
		}

		break;
	}

	case CRSRectProp::PropertyType::LocationXM:
	{
		LONG val = (LONG)MUtoLP(previousValue*10, CM, MU_SCALE, MU_DECIMAL);
		LONG currValue = (LONG)MUtoLP((LONG)GetValue()*10, CM, MU_SCALE, MU_DECIMAL);
		if (parent)
			parent->GetSubItem(0)->SetValue(currValue);
		UpdateLocationX(val, currValue);
		break;
	}

	case CRSRectProp::PropertyType::LocationYM:
	{
		LONG val = (LONG)MUtoLP(previousValue*10, CM, MU_SCALE, MU_DECIMAL);
		LONG currValue = (LONG)MUtoLP((LONG)GetValue()*10, CM, MU_SCALE, MU_DECIMAL);
		if (parent)
			parent->GetSubItem(1)->SetValue(currValue);
		UpdateLocationY(val, currValue);
		break;
	}

	case CRSRectProp::PropertyType::WidthP:
	{
		UpdateWidth((LONG)GetValue());
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU((LONG)GetValue(), CM, 10., 3));
			parent->GetSubItem(2)->SetValue(val);
		}

		break;
	}

	case CRSRectProp::PropertyType::HeightP:
	{
		UpdateHeight((LONG)GetValue());
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU((LONG)GetValue(), CM, 10., 3));
			parent->GetSubItem(3)->SetValue(val);
		}

		break;
	}

	case CRSRectProp::PropertyType::WidthM:
	{
		LONG val = (LONG)MUtoLP((LONG)GetValue()*10, CM, MU_SCALE, MU_DECIMAL);
		UpdateWidth(val);
		if (parent)
			parent->GetSubItem(0)->SetValue(val);
		break;
	}

	case CRSRectProp::PropertyType::HeightM:
	{
		LONG val = (LONG)MUtoLP((LONG)GetValue()*10, CM, MU_SCALE, MU_DECIMAL);
		UpdateHeight(val);
		if (parent)
			parent->GetSubItem(1)->SetValue(val);
		break;
	}

	default:
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSRectProp::OnUpdateValue()
{
	LONG previousValue = (LONG)this->GetValue();

	//Se � un baseRect provo a cancellare il rect precedente compreso di shadow (senn� rimane sporcizia quando lo sposto o cambio dimensione
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pBaseObj = (BaseRect*)m_pOwner;
		int shadowSize = *(pBaseObj->GetShadowSize());
		CRect rectWithShadow(((BaseObj*)m_pOwner)->GetBaseRect());
		rectWithShadow.bottom += shadowSize;
		rectWithShadow.right += shadowSize;
		pBaseObj->m_pDocument->InvalidateRect(rectWithShadow, FALSE);
	}

	BOOL baseUpdate = __super::OnUpdateValue();
	LONG currentValue= (LONG)this->GetValue();
	if (previousValue == currentValue)
		return TRUE;

	UpdateObjectValue(previousValue);

	SqrRect* pSqr = dynamic_cast<SqrRect*>(m_pOwner);

	//Se � un sqr, aggiorno la sua descrizione nel tree control, essendo di fatto rappresentata dalle sue coordinate
	if (pSqr)
	{
		SqrRect* pSqr = (SqrRect*)m_pOwner;

		CNodeTree* node = GetCurrentNode();
		ASSERT(node->m_pItemData == pSqr);

		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, node, FALSE);

		pSqr->UpdateDocument();
	}

	//Se � un baseRect, aggiorno semplicemente il report
	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pRectObj = (BaseRect*)m_pOwner;
		pRectObj->UpdateDocument();
	}

	return baseUpdate;
}

//================================CRSIniProp========================
//-----------------------------------------------------------------------------

CRSIniProp::CRSIniProp(WoormIni* wrmIni, CString name, LPCTSTR value, IniLineType iniLine) 
	:
	CrsProp(name, value)
{
	m_wrmIni = wrmIni;
	m_iniLine = iniLine;
}

//-----------------------------------------------------------------------------
CRSIniProp::CRSIniProp(WoormIni* wrmIni, CString name, variant_t value, IniLineType iniLine) 
	: 
	CrsProp(name, value)
{
	m_wrmIni = wrmIni;
	m_iniLine = iniLine;
}

//-----------------------------------------------------------------------------
BOOL CRSIniProp::OnUpdateValue(){

	BOOL baseUpdate = CBCGPProp::OnUpdateValue();
	CString value = GetValue();
	switch (m_iniLine){
	case IniLineType::TOLERANCE:{
		int tolerance;
		try{
			tolerance = (int)MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
		}

		catch(...){
			break;
		}

		m_wrmIni->m_nSortGap = tolerance;
		break;
	}

	case IniLineType::PITCH_X:{
		int pitchx;
		try{
			pitchx = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
		}

		catch (...){
			break;
		}

		m_wrmIni->m_nGridX = pitchx;

		m_wrmIni->WriteWoormSettings();
		
		CRS_ObjectPropertyView* objPropView = GetPropertyView();
		if (objPropView)
		{
			CWoormDocMng* doc = GetPropertyView()->GetDocument();
			if(doc)
				GetPropertyView()->GetDocument()->Invalidate(TRUE);
		}

		break;
	}

	case IniLineType::PITCH_Y:{
		int pitchy;
		try{
			pitchy = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
		}

		catch (...){
			break;
		}

		m_wrmIni->m_nGridY = pitchy;

		m_wrmIni->WriteWoormSettings();
		CRS_ObjectPropertyView* objPropView = GetPropertyView();
		if (objPropView)
		{
			CWoormDocMng* doc = GetPropertyView()->GetDocument();
			if (doc)
				GetPropertyView()->GetDocument()->Invalidate(TRUE);
		}

		break;
	}

	case IniLineType::PERCENTAGE:{
		double perc;
		try{
			perc = _wtof(value);
		}

		catch (...){
			break;
		}

		m_wrmIni->m_ColumnWidthPercentage.m_nValue = perc;
		break;
	}

	case IniLineType::SHOW_ON_FIELD:{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);
		m_wrmIni->m_Show = (WoormIni::ShowType)option;
		break;
	}

	case IniLineType::LANDSCAPE:{
		int land;
		try{
			land = (int)MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
		}

		catch (...){
			break;
		}

		m_wrmIni->m_nHorzRatio =  DataInt(land);
		break;
	}

	case IniLineType::PORTRAIT:{
		int perc;
		try{
			perc =(int)MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
		}

		catch (...){
			break;
		}

		m_wrmIni->m_nHorzRatio = DataInt(perc); 
		break;
	}
	}

	m_wrmIni->WriteWoormSettings();
	return TRUE; 
}

//================================CRSVariableProp========================
// Property class for setting property of variables

//-----------------------------------------------------------------------------
CRSVariableProp::CRSVariableProp(WoormField* wrmField, CString name, LPCTSTR value, VariableType varType, BOOL isFieldProp , LPCTSTR originDescr /*= 0*/)
: CrsProp(name, value, originDescr)
{
	this->m_pWrmField = wrmField;
	this->m_pVarType = varType;
	this->m_pIsFieldProp = isFieldProp;
}

//-----------------------------------------------------------------------------
CRSVariableProp::CRSVariableProp(WoormField* wrmField, CString name, variant_t value, VariableType varType, BOOL isFieldProp, LPCTSTR originDescr /*= 0*/)
: CrsProp(name, value, originDescr)
{
	this->m_pWrmField = wrmField;
	this->m_pVarType = varType;
	this->m_pIsFieldProp = isFieldProp;
}

//-----------------------------------------------------------------------------
BOOL CRSVariableProp::OnUpdateValue()
{
	CString old = GetValue();
	BOOL baseUpdate = __super::OnUpdateValue();

	CString value = GetValue();
	if (old.CompareNoCase(value) == 0)
		return TRUE;

	if (m_pWrmField != NULL)
	{
		switch (m_pVarType)
		{
			case VariableType::FIELD_NAME:
			{
				CString errMsg = _T("");

				if(this->CheckPropValue(FALSE, errMsg))
				{
					GetDocument()->GetEditorManager()->GetPrgData()->RenameField(m_pWrmField->GetName(), value);

					GetDocument()->GetRSTree(ERefreshEditor::Layouts)->RenameVariableNode(m_pWrmField);
					GetDocument()->GetRSTree(ERefreshEditor::Rules)->RenameVariableNode(m_pWrmField);

					SetColoredState(CrsProp::Mandatory);
				}
				break;
			}

			case VariableType::LENGTH:
			{
				//Gets an old version
				int newValue;
				int oldValue = m_pWrmField->GetLen();
				try
				{	// if conversion fails i'll set an old version 
					newValue = _ttoi(value);
				}
				catch (...)
				{
					newValue = oldValue;
				}

				m_pWrmField->SetLen(newValue);
				break;
			}

			case VariableType::PRECISION:
			{
				int newValue;
				try
				{	// if conversion fails i'll set 0 to the decimal part of precision
					newValue = _ttoi(value);
				}
				catch (...)
				{
					newValue = 0;
				}

				m_pWrmField->SetNumDec(newValue);
				break;
			}

			case VariableType::REINIT_INPUT:
			{
				int index = GetSelectedOption();
				if (index < 0)
					break;
				DWORD_PTR option = GetOptionData(index);
				switch (option) 
				{
					case ReinitType::REINIT_ALWAYS:
					{
						m_pWrmField->SetReInit(TRUE);				
						break;
					}
					case ReinitType::REINIT_NEVER:
					{
						m_pWrmField->SetStatic(TRUE);
						break;
					}
					case ReinitType::REINIT_NORMAL:
					{
						m_pWrmField->SetReInit(FALSE);
						m_pWrmField->SetStatic(FALSE);		
						break;
					}
				}
				break;
			}

			case VariableType::FIELD_ISARRAY :
			{
				if (GetPropertyView()->m_bNeedsApply)
					break;

				int index = GetSelectedOption();
				if (index < 0)
					break;

				BOOL option = GetOptionData(index);

				if (m_pWrmField->IsInput() && m_pWrmField->IsHidden())
					m_pWrmField->SetDataType(m_pWrmField->GetDataType(), option);
				break;
			}

			case VariableType::FIELD_IS_INPUT: 
			{
				int index = GetSelectedOption();
				if (index < 0)
					break;

				DWORD_PTR option = GetOptionData(index);
				if ((m_pWrmField->IsInput() && option == 0) || (!m_pWrmField->IsInput() && option == 1))
					GetPropertyView()->m_bNeedsApply = TRUE;
				else
				{ 
					WoormField::RepFieldType a = (WoormField::RepFieldType)option;
					m_pWrmField->SetFieldType(a);
				}

				if (GetPropertyView()->isArray)
				{
					ASSERT_VALID(GetPropertyView()->isArray);
					if (option == WoormField::RepFieldType::FIELD_INPUT && m_pWrmField->IsHidden())
						GetPropertyView()->isArray->SetVisible(TRUE);
					else
					{
						GetPropertyView()->isArray->SetVisible(FALSE);
						GetPropertyView()->isArray->SelectOption(/*0*/	GetPropertyView()->isArray->GetOptionDataIndex(FALSE));
					}			
				
					GetPropertyView()->m_pPropGrid->AdjustLayout();
				}			
				break;
			}

			case VariableType::FIELD_TYPE: 
			{
				int index = GetSelectedOption();
				if (index < 0)
					break;
				DWORD_PTR option = GetOptionData(index);
				if ((m_pWrmField->IsExprRuleField() && option == 0) || (!m_pWrmField->IsExprRuleField() && option == 1))
					GetPropertyView()->m_bNeedsApply = TRUE;
				break;
			}

			case VariableType::DO_NOT_EXPORT:
			{	
				m_pWrmField->SetSkipXml(MakeBoolFromString(value));
				break;
			}

			case VariableType::EXPORT_ALIAS:
			{
				CString oldValue = m_pWrmField->GetXmlName();
				if (value.IsEmpty())
				{
					this->SetValue((variant_t)oldValue);
					m_pWrmField->SetXmlName(oldValue);
					m_pWrmField->SetName(oldValue);
				}
				else
				{
					m_pWrmField->SetXmlName(value);
					m_pWrmField->SetName(value);
				}

				/*CRSTreeCtrl* pTreeCtrl = &GetDocument()->GetWoormFrame()->m_pReportTreeView->m_TreeCtrl;
				ASSERT(pTreeCtrl);
				HTREEITEM ht = NULL;

				if (m_pWrmField->IsHidden())
				{ 
					pTreeCtrl->FillVariables(TRUE,FALSE);
					ht = pTreeCtrl->FindItemData((DWORD)m_pWrmField, pTreeCtrl->m_htVariables);
				}

				else
				{
					HTREEITEM item = pTreeCtrl->GetSelectedItem();
					HTREEITEM firstLevel;
					while (item = pTreeCtrl->GetParentItem(item)) 
					{
						firstLevel = item;
					}

					CNodeTree* node=pTreeCtrl->GetNode(firstLevel);
					CNodeTree::ENodeType nodeT = node->m_NodeType;
			
					if (nodeT== CNodeTree::ENodeType::NT_ROOT_RULES)
						ht =pTreeCtrl->FindItemData((DWORD)m_pWrmField,pTreeCtrl->m_htRules);
					else if (nodeT == CNodeTree::ENodeType::NT_ROOT_LAYOUTS)
						ht = pTreeCtrl->FindItemData((DWORD)m_pWrmField, pTreeCtrl->m_htLayouts);
				}

				if (ht)
					pTreeCtrl->SelectItem(ht);*/
				break;
			}
			default:
				break;
		}
	}
	return TRUE;
}

//================================CRSFieldTypeProp==================================
//-----------------------------------------------------------------------------
CRSFieldTypeProp::CRSFieldTypeProp(FieldRect* pFieldRect, const CString& strName, const CString& description, CRS_ObjectPropertyView* PropertyView, CBCGPProp* parentGroup, CList<CrsProp*>* dependentPropList)
	:
	CrsProp(strName, L"", description),
	m_pPropertyView(PropertyView),
	m_pFieldRect(pFieldRect),
	m_pParentProp(parentGroup),
	m_lstDependentProp(dependentPropList),
	m_pShowAsProp(NULL)

{
	ASSERT_VALID(pFieldRect);

	AllowEdit(FALSE);

	AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_NORMAL),	TRUE, ::EFieldShowAs::FT_NORMAL);
	AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_IMAGE),		TRUE, ::EFieldShowAs::FT_IMAGE);
	AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_BARCODE),	TRUE, ::EFieldShowAs::FT_BARCODE);
	AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_TEXTFILE),	TRUE, ::EFieldShowAs::FT_TEXTFILE);
	//AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_URL),		TRUE, ::EFieldShowAs::FT_URL);

	//this->SelectOption(m_pFieldRect->m_ShowAs); -> commentato perch�, selezionandolo, viene mostrato in bold (come se fosse stato editato)
	SetValue((_variant_t)::EFieldShowAsToString(m_pFieldRect->m_ShowAs));

	//aggiunge properties al proprio livello: le disegna dopo essere stata aggiunta lei stessa
	//-> ora ancora presto per il DrawProperties
	//DrawProperties(m_pFieldRect->m_ShowAs);
	m_pPropToRedraw = new CList<CrsProp*>();
}

//-----------------------------------------------------------------------------
CRSFieldTypeProp::~CRSFieldTypeProp()
{
	delete m_lstDependentProp;
	delete m_pPropToRedraw;
}

//-----------------------------------------------------------------------------
void CRSFieldTypeProp::DrawProperties()
{
	DrawProperties(m_pFieldRect->m_ShowAs);
}

//-----------------------------------------------------------------------------
BOOL CRSFieldTypeProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	DrawProperties(index);

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSFieldTypeProp::DrawProperties(int index)
{
	m_pPropertyView->m_pPropGrid->ClearCommands();
	m_pPropertyView->AddBasicCommands();

	DWORD_PTR option = GetOptionData(index);

	if(m_pShowAsProp)
	{
		CBCGPProp* prop = (CBCGPProp*)m_pShowAsProp;
		m_pPropertyView->m_pAppearenceGroup->RemoveSubItem(prop);
		m_pShowAsProp = NULL;
	}

	POSITION pos;
	pos = m_pPropToRedraw->GetHeadPosition();
	while (pos)
	{
		CBCGPProp* prop;
		prop = m_pPropToRedraw->GetNext(pos);

		ASSERT_VALID(prop);
		m_pPropertyView->m_pAppearenceGroup->RemoveSubItem(prop, FALSE);
	}

	this->m_bGroup = TRUE;

	if ((::EFieldShowAs)option == ::EFieldShowAs::FT_IMAGE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Image);

		//image group
		CrsProp* pImageGroup = new CrsProp(_TB("Image settings"), TRUE);
		m_pPropertyView->m_pAppearenceGroup->AddSubItem(pImageGroup, m_pPropertyView->GetPropertyGrid());
		m_pShowAsProp = pImageGroup;

		//set as Bitmap if is'nt yet setted
		if (m_pFieldRect->m_ShowAs != EFieldShowAs::FT_IMAGE)
			m_pFieldRect->ToggleBitmap();

		//filename
		pImageGroup->AddSubItem(new CRSSearchTbDialogProp(m_pFieldRect, &(m_pFieldRect->m_Value), CRSSearchTbDialogProp::PropertyType::FieldValueImgName, CRSFileNameProp::Filter::Img, m_pPropertyView), m_pPropertyView->GetPropertyGrid());
		
		//ImageFitMode
		pImageGroup->AddSubItem(new CRSImageFitProp(m_pFieldRect, _TB("Image Fit Mode"), &(m_pFieldRect->m_pBitmap->m_ImageFitMode), _TB("Set image fit mode for this image")), m_pPropertyView->GetPropertyGrid());

		CStringList lstCommands;
	
		//subtotal definition
		lstCommands.AddTail(_TB("Copy attributes from"));

		//Resize to mantain original Aspeect Ratio
		lstCommands.AddTail(_TB("Resize Keeping original Aspect Ratio"));

		//Crop Image/Show all - Cut Image/Undo
		lstCommands.AddTail(_TB("Crop Image/Show all"));

		//Original Size
		lstCommands.AddTail(_TB("Original Size"));
		if (m_pFieldRect->m_bIsCutted)
		{//---cutted location group
			CrsProp* pLocationProp = new CrsProp(_TB("Cutted Location"), 0, TRUE);
			CPoint coordinatePoint = m_pFieldRect->m_rectCutted.TopLeft();
				//x	 px
				CRSRectProp* pRectXpx = new CRSRectProp(m_pFieldRect, _TB("X px"), _TB("X"), CRSRectProp::PropertyType::LocationXP);
				pLocationProp->AddSubItem(pRectXpx);
				pRectXpx->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//y	 px
				CRSRectProp* pRectYpy = new CRSRectProp(m_pFieldRect, _TB("Y px"), _TB("Y"), CRSRectProp::PropertyType::LocationYP);
				pLocationProp->AddSubItem(pRectYpy);
				pRectYpy->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//x	 mm
				CRSRectProp* pRectXmm = new CRSRectProp(m_pFieldRect, _TB("X mm"), _TB("X"), CRSRectProp::PropertyType::LocationXM);
				pLocationProp->AddSubItem(pRectXmm);
				pRectXmm->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//y	 mm
				CRSRectProp* pRectYmm = new CRSRectProp(m_pFieldRect, _TB("Y mm"), _TB("Y"), CRSRectProp::PropertyType::LocationYM);
				pLocationProp->AddSubItem(pRectYmm);
				pRectYmm->SetOwnerList(m_pPropertyView->GetPropertyGrid());
			//add to the parent group
			pImageGroup->AddSubItem(pLocationProp, m_pPropertyView->GetPropertyGrid());

			//---cutted size group
			CrsProp* pSizeProp = new CrsProp(_TB("Cutted Size"), 0, TRUE);
				//width	 px
			CRSRectProp* pRecWpx = new CRSRectProp(m_pFieldRect, _TB("Width px"), _TB("Width"), CRSRectProp::PropertyType::WidthP);
				pSizeProp->AddSubItem(pRecWpx);
				pRecWpx->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//height px
				CRSRectProp* pRecHpx = new CRSRectProp(m_pFieldRect, _TB("Height px"), _TB("Height"), CRSRectProp::PropertyType::HeightP);
				pSizeProp->AddSubItem(pRecHpx);
				pRecHpx->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//width	 mm
				CRSRectProp* pRecWmm = new CRSRectProp(m_pFieldRect, _TB("Width mm"), _TB("Width"), CRSRectProp::PropertyType::WidthM);
				pSizeProp->AddSubItem(pRecWmm);
				pRecWmm->SetOwnerList(m_pPropertyView->GetPropertyGrid());
				//height mm
				CRSRectProp* pRecHmm = new CRSRectProp(m_pFieldRect, _TB("Height mm"), _TB("Height"), CRSRectProp::PropertyType::HeightM);
				pSizeProp->AddSubItem(pRecHmm);
				pRecHmm->SetOwnerList(m_pPropertyView->GetPropertyGrid());
			//add to the parent group
			pImageGroup->AddSubItem(pSizeProp, m_pPropertyView->GetPropertyGrid());
		}

		m_pPropertyView->m_pPropGrid->SetCommands(lstCommands);

		//nascondo le propriet� dipendenti
		SetDepPropsVisibile(FALSE);
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_BARCODE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Barcode);
		//set as BarCode if is'nt yet setted
		if (m_pFieldRect->m_ShowAs != EFieldShowAs::FT_BARCODE)
			m_pFieldRect->ToggleBarCode();
		 
		//barcode group
		CRSBarCodeGroupProp* pBarcodeGroup = new CRSBarCodeGroupProp();
		m_pPropertyView->m_pAppearenceGroup->AddSubItem(pBarcodeGroup, m_pPropertyView->GetPropertyGrid());
		m_pShowAsProp = pBarcodeGroup;
		pBarcodeGroup->m_lstDependentProp = new CList<CRSBarCodeProp*>();

		//Barcode Type From ComboBox
		CRSBarCodeComboProp* barcodeTypeFromComboProp = new CRSBarCodeComboProp(m_pFieldRect, _TB("Barcode Type From"), m_pFieldRect->m_pBarCode, _T("Select source of Barcode Type: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::TypeFromField);
		
		pBarcodeGroup->AddSubItem(barcodeTypeFromComboProp);
		barcodeTypeFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeTypeFromComboProp);

		//Barcode Encoding From CheckBox
		CRSBarCodeComboProp* barcodeEncFromComboProp = new CRSBarCodeComboProp(m_pFieldRect, _TB("Checksum Digit Module From"), m_pFieldRect->m_pBarCode, _TB(""), m_pPropertyView, CRSBarCodeComboProp::PropertyType::EncodingTypeFromField);

		pBarcodeGroup->AddSubItem(barcodeEncFromComboProp);
		barcodeEncFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeEncFromComboProp);

		//Barcode Version From CheckBox
		CRSBarCodeComboProp* barcodeVersionFromComboProp = new CRSBarCodeComboProp(m_pFieldRect, _TB("Barcode Version From"), m_pFieldRect->m_pBarCode, _TB("Select source of Barcode Version: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::VersionFromField);

		pBarcodeGroup->AddSubItem(barcodeVersionFromComboProp);
		barcodeVersionFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeVersionFromComboProp);

		//Barcode Error Correction Level From CheckBox
		CRSBarCodeComboProp* barcodeErrCorrLevelFromComboProp = new CRSBarCodeComboProp(m_pFieldRect, _TB("Error Correction Level From"), m_pFieldRect->m_pBarCode, _TB("Select source of Error Correction Level: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::ErrorCorrectionLevelFromField);

		pBarcodeGroup->AddSubItem(barcodeErrCorrLevelFromComboProp);
		barcodeErrCorrLevelFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeErrCorrLevelFromComboProp);

		//Show Label
		CRSBarCodeShowTextProp* pShowTextProp = new CRSBarCodeShowTextProp(m_pFieldRect, _TB("Show text"), &(m_pFieldRect->m_pBarCode->m_bShowLabel), m_pFieldRect->m_pBarCode, m_pPropertyView, _TB("If label should be visible"));
		pBarcodeGroup->AddSubItem(pShowTextProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pShowTextProp);

		//Size Module/Bar
		CRSBarCodeSizeProp* pModuleBarSizeProp = new CRSBarCodeSizeProp(m_pFieldRect, m_pFieldRect->m_pBarCode, m_pPropertyView, CRSBarCodeComboProp::PropertySizeType::BarWidth_ModuleSize, FALSE);
		pBarcodeGroup->AddSubItem(pModuleBarSizeProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pModuleBarSizeProp);

		//Hight bar
		CRSBarCodeSizeProp* pBarHeightProp = new CRSBarCodeSizeProp(m_pFieldRect, m_pFieldRect->m_pBarCode, m_pPropertyView, CRSBarCodeComboProp::PropertySizeType::BarHeight, FALSE);
		pBarcodeGroup->AddSubItem(pBarHeightProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pBarHeightProp);

		//Vertical
		pBarcodeGroup->AddSubItem(new CRSBoolProp(m_pFieldRect, _TB("Vertical orientation"), &(m_pFieldRect->m_pBarCode->m_bVertical), _TB("TRUE: the barcode is shown with vertical orientation; FALSE: the barcode is shown with horizontal orientation")));

		//default Barcode
		pBarcodeGroup->AddSubItem(new CRSValueProp(m_pFieldRect, _TB("Preview Barcode"), &(m_pFieldRect->m_Value), _TB("!Only for Preview! The value of this property is temporary and will not be saved.")));

		//nascondo le propriet� dipendenti
		SetDepPropsVisibile(FALSE);
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_TEXTFILE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Textfile);

		//textFile group
		CrsProp* pFileGroup = new CrsProp(_TB("File settings"), TRUE);
		m_pPropertyView->m_pAppearenceGroup->AddSubItem(pFileGroup, m_pPropertyView->GetPropertyGrid());
		m_pShowAsProp = pFileGroup;

		//set as TextFile if is'nt yet setted
		if (m_pFieldRect->m_ShowAs != EFieldShowAs::FT_TEXTFILE)
			m_pFieldRect->ToggleTextFile();

		//filename
		pFileGroup->AddSubItem(new CRSSearchTbDialogProp(m_pFieldRect, &(m_pFieldRect->m_Value), CRSSearchTbDialogProp::PropertyType::FieldValueFileName, CRSFileNameProp::Filter::Txt, m_pPropertyView), m_pPropertyView->GetPropertyGrid());
		
		//mostro le propriet� dipendenti
		SetDepPropsVisibile(FALSE);
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_URL)
	{
		//TODO ANDREA
		//mostro le propriet� dipendenti
		SetDepPropsVisibile(TRUE);
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_NORMAL)
	{
		RemoveStateImg();

		//set as Normal if is'nt yet setted
		if (m_pFieldRect->m_ShowAs == EFieldShowAs::FT_IMAGE)
			m_pFieldRect->ToggleBitmap();
		else if (m_pFieldRect->m_ShowAs == EFieldShowAs::FT_BARCODE)
			m_pFieldRect->ToggleBarCode();
		else if (m_pFieldRect->m_ShowAs == EFieldShowAs::FT_TEXTFILE)
			m_pFieldRect->ToggleTextFile();

		//mostro le propriet� dipendenti
		SetDepPropsVisibile(TRUE);
	}

	this->m_bGroup = FALSE;

	pos = m_pPropToRedraw->GetHeadPosition();
	while (pos)
	{
		CBCGPProp* prop;
		prop = m_pPropToRedraw->GetNext(pos);

		ASSERT_VALID(prop);
		m_pPropertyView->m_pAppearenceGroup->AddSubItem(prop);
	}

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

//-----------------------------------------------------------------------------
//questo metodo serve a settare a visible False alcune properties "registrate" a questa come sue dipendenti, 
//ovvero che cambiano visibilit� in base al suo valore. 
void CRSFieldTypeProp::SetDepPropsVisibile(BOOL visible)
{
	POSITION pos;

	pos = m_lstDependentProp->GetHeadPosition();
	while (pos)
	{
		CBCGPProp* prop;
		prop = m_lstSubItems.GetNext(pos);

		ASSERT_VALID(prop);

		if (prop->IsKindOf(RUNTIME_CLASS(CrsProp)))
		{
			CrsProp* propEx = (CrsProp*)prop;
			propEx->SetVisible(visible);
		}
	}
}

//================================CRSColumnTypeProp==================================
//-----------------------------------------------------------------------------
CRSColumnTypeProp::CRSColumnTypeProp(TableColumn* pCol, CRS_ObjectPropertyView* PropertyView)
	:
	CrsProp(_TB("Show As"), L"", _TB("The type of the column")),
	m_pPropertyView(PropertyView),
	m_pCol(pCol),
	m_pShowAsProp(NULL)
{
	ASSERT_VALID(m_pCol);

	AllowEdit(FALSE);

	//normal
	AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_NORMAL), TRUE, ::EFieldShowAs::FT_NORMAL);
	//image
	if (m_pCol->CanBeBitmap())
		AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_IMAGE), TRUE, ::EFieldShowAs::FT_IMAGE);
	//barcode
	if (m_pCol->CanBeBarCode())
		AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_BARCODE), TRUE, ::EFieldShowAs::FT_BARCODE);
	//textfile
	if (m_pCol->CanBeTextFile())
		AddOption(::EFieldShowAsToString(::EFieldShowAs::FT_TEXTFILE), TRUE, ::EFieldShowAs::FT_TEXTFILE);

	SetValue((_variant_t)::EFieldShowAsToString(m_pCol->m_ShowAs));

	
	//aggiunge properties al proprio livello: le disegna dopo essere stata aggiunta lei stessa
	//-> ora ancora presto per il DrawProperties
	//DrawProperties(m_pCol->m_ShowAs);
}

//-----------------------------------------------------------------------------
CRSColumnTypeProp::~CRSColumnTypeProp()
{
}

//-----------------------------------------------------------------------------
BOOL CRSColumnTypeProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	DrawProperties(index);

	return baseUpdate;
}
//-----------------------------------------------------------------------------
void CRSColumnTypeProp::DrawProperties()
{
	DrawProperties(m_pCol->m_ShowAs);
}

//-----------------------------------------------------------------------------
void CRSColumnTypeProp::DrawProperties(int index)
{
	m_pPropertyView->m_pPropGrid->ClearCommands();
	m_pPropertyView->AddBasicCommands();

	DWORD_PTR option = GetOptionData(index);

	if (m_pShowAsProp)
	{
		CBCGPProp* prop = (CBCGPProp*)m_pShowAsProp;
		m_pParent->RemoveSubItem(prop);
		m_pShowAsProp = NULL;
	}

	this->m_bGroup = TRUE;

	this->SetColoredState(CrsProp::State::Normal);
	this->SetOriginalDescription();

	if ((::EFieldShowAs)option == ::EFieldShowAs::FT_IMAGE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Image);

		//image group
		CrsProp* pImageGroup = new CrsProp(_TB("Image settings"), TRUE);
		m_pParent->AddSubItem(pImageGroup);
		m_pShowAsProp = pImageGroup;

		//set as Bitmap if is'nt yet setted
		if (m_pCol->m_ShowAs != EFieldShowAs::FT_IMAGE)
			m_pCol->ToggleBitmap();

		//ImageFitMode
		pImageGroup->AddSubItem(new CRSImageFitProp(m_pCol, _TB("Image Fit Mode"), &(m_pCol->m_pBitmap->m_ImageFitMode), _TB("Set image fit mode for this column")));

		if (m_pCol->m_bMultipleRow)
		{
			this->SetColoredState(CrsProp::State::Error);
			this->SetNewDescription(_TB("This setting is not compatible with the property \"Distribute over multiline if too long\""));
		}

	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_BARCODE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Barcode);
		//set as BarCode if is'nt yet setted
		if (m_pCol->m_ShowAs != EFieldShowAs::FT_BARCODE)
			m_pCol->ToggleBarCode();

		//barcode group
		CRSBarCodeGroupProp* pBarcodeGroup = new CRSBarCodeGroupProp();
		m_pParent->AddSubItem(pBarcodeGroup);
		m_pShowAsProp = pBarcodeGroup;
		pBarcodeGroup->m_lstDependentProp = new CList<CRSBarCodeProp*>();

		//Barcode Type From ComboBox
		CRSBarCodeComboProp* barcodeTypeFromComboProp = new CRSBarCodeComboProp(m_pCol, _TB("Barcode Type From"), m_pCol->m_pBarCode, _TB("Select source of Barcode Type: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::TypeFromField);

		pBarcodeGroup->AddSubItem(barcodeTypeFromComboProp);
		barcodeTypeFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeTypeFromComboProp);

		//Barcode Encoding
		CRSBarCodeComboProp* barcodeEncFromComboProp = new CRSBarCodeComboProp(m_pCol, _TB("Checksum Digit Module From"), m_pCol->m_pBarCode, _TB(""), m_pPropertyView, CRSBarCodeComboProp::PropertyType::EncodingTypeFromField);

		pBarcodeGroup->AddSubItem(barcodeEncFromComboProp);
		barcodeEncFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeEncFromComboProp);

		//Barcode Version
		CRSBarCodeComboProp* barcodeVersionFromComboProp = new CRSBarCodeComboProp(m_pCol, _TB("Barcode Version From"), m_pCol->m_pBarCode, _TB("Select source of Barcode Version: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::VersionFromField);

		pBarcodeGroup->AddSubItem(barcodeVersionFromComboProp);
		barcodeVersionFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeVersionFromComboProp);

		//Barcode Error Correction Level From CheckBox
		CRSBarCodeComboProp* barcodeErrCorrLevelFromComboProp = new CRSBarCodeComboProp(m_pCol, _TB("Error Correction Level From"), m_pCol->m_pBarCode, _TB("Select source of Error Correction Level: from a selected value or an other field"), m_pPropertyView, CRSBarCodeComboProp::PropertyType::ErrorCorrectionLevelFromField);

		pBarcodeGroup->AddSubItem(barcodeErrCorrLevelFromComboProp);
		barcodeErrCorrLevelFromComboProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		pBarcodeGroup->m_lstDependentProp->AddTail(barcodeErrCorrLevelFromComboProp);

		//Show Label
		CRSBarCodeShowTextProp* pShowTextProp = new CRSBarCodeShowTextProp(m_pCol, _TB("Show text"), &(m_pCol->m_pBarCode->m_bShowLabel), m_pCol->m_pBarCode, m_pPropertyView, _TB("If label should be visible"));
		pBarcodeGroup->AddSubItem(pShowTextProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pShowTextProp);

		//Size Module/Bar
		CRSBarCodeSizeProp* pModuleBarSizeProp = new CRSBarCodeSizeProp(m_pCol, m_pCol->m_pBarCode, m_pPropertyView, CRSBarCodeComboProp::PropertySizeType::BarWidth_ModuleSize, FALSE);
		pBarcodeGroup->AddSubItem(pModuleBarSizeProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pModuleBarSizeProp);

		//Hight bar
		CRSBarCodeSizeProp* pBarHeightProp = new CRSBarCodeSizeProp(m_pCol, m_pCol->m_pBarCode, m_pPropertyView, CRSBarCodeComboProp::PropertySizeType::BarHeight, FALSE);
		pBarcodeGroup->AddSubItem(pBarHeightProp);
		pBarcodeGroup->m_lstDependentProp->AddTail(pBarHeightProp);

		//Vertical
		pBarcodeGroup->AddSubItem(new CRSBoolProp(m_pCol, _TB("Vertical orientation"), &(m_pCol->m_pBarCode->m_bVertical), _TB("If barcode should be show vertical or horizontal.")));
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_TEXTFILE)
	{
		SetStateImg(CRS_PropertyGrid::Img::Textfile);
		//set as TextFile if is'nt yet setted
		if (m_pCol->m_ShowAs != EFieldShowAs::FT_TEXTFILE)
			m_pCol->ToggleTextFile();
	}

	else if ((::EFieldShowAs)option == ::EFieldShowAs::FT_NORMAL)
	{
		SetStateImg(CRS_PropertyGrid::Img::NoImg);
		//set as Normal if is'nt yet setted
		if (m_pCol->m_ShowAs == EFieldShowAs::FT_IMAGE)
			m_pCol->ToggleBitmap();
		else if (m_pCol->m_ShowAs == EFieldShowAs::FT_BARCODE)
			m_pCol->ToggleBarCode();
		else if (m_pCol->m_ShowAs == EFieldShowAs::FT_TEXTFILE)
			m_pCol->ToggleTextFile();
	}

	this->m_bGroup = FALSE;

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

//================================CRSImageFitProp=================================== todo andrea->da rimuovere perch� non gestita in woorm
//-----------------------------------------------------------------------------
CRSImageFitProp::CRSImageFitProp(CObject* pOwner, const CString& strName, CTBPicture::ImageFitMode* pValue, const CString& description)
	:
	CrsProp(strName, (_variant_t)CTBPicture::ImageFitModeToString(*pValue), description),
	m_pOwner(pOwner),
	m_pValue(pValue)
{
	ASSERT_VALID(pOwner);

	AllowEdit(FALSE);

	AddOption(CTBPicture::ImageFitModeToString(CTBPicture::ImageFitMode::NORMAL),		TRUE, CTBPicture::ImageFitMode::NORMAL);
	AddOption(CTBPicture::ImageFitModeToString(CTBPicture::ImageFitMode::BEST),			TRUE, CTBPicture::ImageFitMode::BEST);
	/*AddOption(CTBPicture::ImageFitModeToString(CTBPicture::ImageFitMode::HORIZONTAL),	TRUE, CTBPicture::ImageFitMode::HORIZONTAL);
	AddOption(CTBPicture::ImageFitModeToString(CTBPicture::ImageFitMode::VERTICAL),		TRUE, CTBPicture::ImageFitMode::VERTICAL);*/
	AddOption(CTBPicture::ImageFitModeToString(CTBPicture::ImageFitMode::STRETCH),		TRUE, CTBPicture::ImageFitMode::STRETCH);
	
	//avendo commmentato horizontal e vertical, non posso usare il value (imagefitmode) come indice, ma devo cercare l'indice del value e settare quello
	
	this->SelectOption(this->GetOptionDataIndex(*pValue));
}

//-----------------------------------------------------------------------------
BOOL CRSImageFitProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	DWORD_PTR option = GetOptionData(index);

	*m_pValue = (CTBPicture::ImageFitMode)option;

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pFieldObj = (FieldRect*)m_pOwner;
		pFieldObj->Invalidate(TRUE);
	}

	UpdatePropertyValue();

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pRectObj = (BaseRect*)m_pOwner;
		pRectObj->UpdateDocument();
	}

	if (m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pCol = (TableColumn*)m_pOwner;
		pCol->Redraw();
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSImageFitProp::UpdatePropertyValue()
{
	SetValue((_variant_t)CTBPicture::ImageFitModeToString(*m_pValue));
}

//================================CRSStyleProp=================================== 
//-----------------------------------------------------------------------------
CRSStyleProp::CRSStyleProp(CObject* pOwner, CRS_ObjectPropertyView* propertyView)
	:
	CrsProp(_TB("Class Style"), L"", L""),
	m_pOwner(pOwner),
	m_pPropertyView(propertyView)
{
	ASSERT_VALID(pOwner);
	AllowEdit(FALSE);

	AddOptions();

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSStyleProp::OnSelectCombo()
{
	CBCGPProp::OnSelectCombo();
	
	if (this->IsValueChanged() || ((CString)GetValue()).IsEmpty())
	{
		int index = GetSelectedOption();
		if (index >= 0)
		{
			CObject* pStyleObj = (CObject*)GetOptionData(index);//pu� essere null perch� per custom e default ho puntatore a null
			
			GenericDrawObj* pCurrObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
			if (pCurrObj)
			{
				if (((CString)GetValue()).CompareNoCase(CWoormTemplate::s_sCustom_StyleName) == 0) //default
				{
					ASSERT(pStyleObj == NULL);
					pCurrObj->SetWrmStyleClass(CWoormTemplate::s_sCustom_StyleName);
				}

				else	
				{ 
					ASSERT_VALID(pStyleObj);
					pCurrObj->SetWrmStyleClass(dynamic_cast<CObject*>(pStyleObj));
				}

				pCurrObj->Redraw();

				m_pPropertyView->ReLoadPropertyGrid();
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CRSStyleProp::UpdatePropertyValue()
{
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
	{
		CString wrmStyle = pGenObj->GetWrmStyleClass();
		//se ho un oggetto singolo e wrmStyle � vuoto, allora lo valorizzo a "Default"
		if(m_pOwner->GetRuntimeClass() != RUNTIME_CLASS(SelectionRect))
			if (wrmStyle &&  wrmStyle.IsEmpty())
				wrmStyle = L"<Default>";
		//in caso contrario, il metodo GetWrmStyleClass() della multiselezione lo fa gi� al suo interno e in caso
		//restituisca stringa vuota vuol dire che non � uno stile comune e lascio stringa vuota

		SetValue((_variant_t)wrmStyle);
	}
}

//-----------------------------------------------------------------------------
void CRSStyleProp::AddOptions()
{
	ASSERT_VALID(m_pOwner);
	CRuntimeClass* objRuntimeClass = NULL;

	//multi selezione
	if (m_pOwner->GetRuntimeClass() == RUNTIME_CLASS(SelectionRect))
	{
		SelectionRect* pMulSel = (SelectionRect*)m_pOwner;
		ASSERT_VALID(pMulSel);

		BaseObj* pObj = pMulSel->GetObjAt(0);
		ASSERT_VALID(pObj);
		if (!pObj)
		{
			this->SetVisible(FALSE);
			return;
		}

		//should be the first row of every case statement
		CRuntimeClass* commonClass = pObj->GetRuntimeClass();

		int i;
		for (i = 0; i < pMulSel->GetSize(); i++)
			if (commonClass != pMulSel->GetObjAt(i)->GetRuntimeClass()) break;

		if (i == pMulSel->GetSize())
		{
			//li ho scorsi tutti, quindi sono tutti dello stesso tipo e valorizzo la runtime class a quella comune, altrimenti rimarr� vuota
			objRuntimeClass = commonClass;
		}

		else
		{
			//non sono tutti dello stesso tipo, quindi nascondo la property
			this->SetVisible(FALSE);
			return;
		}
	}

	//selezione singola
	else
		objRuntimeClass = m_pOwner->GetRuntimeClass();

	if (m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl)
	{
		CString strName; CObject* pAr = NULL;
		for (POSITION pos = m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl->m_Layouts.GetStartPosition(); pos != NULL; pAr = NULL, strName.Empty())
		{
			m_pPropertyView->GetDocument()->m_Template.m_pWoormTpl->m_Layouts.GetNextAssoc(pos, strName, pAr);
			CLayout* pObjects = (CLayout*)pAr;

			for (int i = 0; i <= pObjects->GetUpperBound(); i++)
			{
				BaseObj* pObj = (*pObjects)[i];
				CRuntimeClass*iterationObjclass = pObj->GetRuntimeClass();
				
				//stessa classe -> aggiungo l'option con il puntatore all'oggetto di riferimento, dentro alla dropdown
				if (objRuntimeClass == iterationObjclass)
				{ 
					CString sStyleName = pObj->GetWrmStyleClass();
					if (sStyleName.IsEmpty())
						sStyleName = L"<Default>";

					AddOption(sStyleName, TRUE, (DWORD_PTR)pObj);
				}

				//pi� complicato: ho selezionato una colonna e sto cercando un oggetto di tipo table per frugare fra le sue colonne
				else if (objRuntimeClass == RUNTIME_CLASS(TableColumn) && iterationObjclass == RUNTIME_CLASS(Table))
				{
					Table* pT = (Table*)pObj;
					for (int nC = 0; nC <= pT->GetColumns().GetUpperBound(); nC++)
					{
						TableColumn* pCol = pT->GetColumns()[nC];

						CString sStyleName = pCol->GetWrmStyleClass();
						if (sStyleName.IsEmpty())
							sStyleName = L"<Default>";

						AddOption(sStyleName, TRUE, (DWORD_PTR)pCol);
					}
				}
			}
		}
	}

	AddOption(CWoormTemplate::s_sCustom_StyleName, TRUE, NULL);
}

//================================CRSColumnPrintAttributesProp==================================
//-----------------------------------------------------------------------------
CRSColumnPrintAttributesProp::CRSColumnPrintAttributesProp(TableColumn* pCol)
	:
	CrsProp(_TB("Type"), L"", _TB("Print")),
	m_pCol(pCol)
{
	ASSERT_VALID(m_pCol);
	AllowEdit(FALSE);
	pCol->ToggleFixed();
}

//-----------------------------------------------------------------------------
BOOL CRSColumnPrintAttributesProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	return baseUpdate;
}

//================================CRSValueProp===================================
//-----------------------------------------------------------------------------
CRSValueProp::CRSValueProp(CObject* pOwner, const CString& strName, Value* pValue, const CString& description)
	:
	CrsProp(strName, (_variant_t)pValue->GetText(), description),
	m_pOwner(pOwner),
	m_pValue(pValue)
{
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
BOOL CRSValueProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	m_pValue->SetText(this->GetValue());

	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	return baseUpdate;
}

//================================CRSFileNameProp===================================
//-----------------------------------------------------------------------------
CRSFileNameProp::CRSFileNameProp(CObject* pObj, const CString& strName, CString* strFileName, const CString& description, Filter filter)
	:
	CBCGPFileProp(strName, TRUE, *strFileName),
	m_pOwner(pObj),
	m_pStrFileName(strFileName)
{
	ASSERT_VALID(m_pOwner);

	InitProperty(filter);
}

//-----------------------------------------------------------------------------
CRSFileNameProp::CRSFileNameProp(CObject* pObj, const CString& strName, const CString& strFileName, const CString& description, Filter filter)
	:
	CBCGPFileProp(strName, TRUE, strFileName),
	m_pOwner(pObj),
	m_pStrFileName(NULL)
{
	ASSERT_VALID(m_pOwner);

	InitProperty(filter);
}

//-----------------------------------------------------------------------------
void CRSFileNameProp::InitProperty(Filter filter)
{
	m_image.SetImageSize(CSize(14, 14));
	m_image.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_image);
	HICON icon = TBLoadPng(TBGlyph(szGlyphOpenFolder14));
	m_image.AddIcon(icon);
	::DestroyIcon(icon);

	switch (filter)
	{
	case Filter::None:
		break;
	case Filter::Img:
	{
		m_strFilter = _T("Image Files (*.png, *.bmp, *.jpg)|*.png; *.bmp; *.jpg|All Files (*.*)|*.*||");
		m_strDefExt = _T("png");
		break;
	}

	case Filter::Txt:
	{
		m_strFilter = _T("Text Files (*.txt)|*.txt|All Files (*.*)|*.*||");
		m_strDefExt = _T("txt");
		break;
	}

	case Filter::WoormTemplate:
	{
		m_strFilter = _T("Woorm templates (*.wrmt)|*.wrmt||");
		m_strDefExt = _T("wrmt");
		break;
	}
	}

	CheckIfIsAccessibleNameSpace();
}

//-----------------------------------------------------------------------------
void CRSFileNameProp::OnClickButton(CPoint point)
{
	__super::OnClickButton(point);

	UpdateValue();
}

//per quando l'utente digita il path a mano o facendo copia e incolla
//-----------------------------------------------------------------------------
BOOL CRSFileNameProp::OnEndEdit()
{
	if (IsValueChanged())
	{
		UpdateValue();
		CheckIfIsAccessibleNameSpace();
	}

	return CBCGPFileProp::OnEndEdit();
}

//-----------------------------------------------------------------------------
void CRSFileNameProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	CBCGPToolbarButton button;

	CBCGPVisualManager::BCGBUTTON_STATE state = FALSE ? CBCGPVisualManager::ButtonsIsHighlighted : CBCGPVisualManager::ButtonsIsRegular;

	CBCGPVisualManager::GetInstance()->OnFillButtonInterior(pDC, &button, rectButton, state);
	m_image.DrawEx(pDC, rectButton, 0, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	CBCGPVisualManager::GetInstance()->OnDrawButtonBorder(pDC, &button, rectButton, state);
}

//-----------------------------------------------------------------------------
CRS_PropertyGrid* CRSFileNameProp::GetPropertyGrid()
{
	ASSERT_VALID(this->m_pWndList);

	CRS_PropertyGrid* pPropGrid = dynamic_cast<CRS_PropertyGrid*>(this->m_pWndList);
	if (!pPropGrid)
		ASSERT(FALSE);

	return pPropGrid;
}

//-----------------------------------------------------------------------------
void CRSFileNameProp::UpdateValue()
{
	if (m_pStrFileName)
		*m_pStrFileName = this->GetValue();

	//read from file
	if (m_pOwner->IsKindOf(RUNTIME_CLASS(FileRect)))
	{
		FileRect* pFile = (FileRect*)m_pOwner;
		pFile->ReadTextFromFile(*m_pStrFileName);
	}

	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(GraphRect)))
	{
		GraphRect* pGraph = (GraphRect*)m_pOwner;
		pGraph->m_Bitmap.ReadFile(*m_pStrFileName, TRUE);
	}

	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pField = (FieldRect*)m_pOwner;
		pField->m_Value.SetText(this->GetValue());
	}

	//redraw
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();
	else if (m_pOwner->IsKindOf(RUNTIME_CLASS(CWoormDocMng)))
	{
		CWoormDocMng* pWrmDoc = (CWoormDocMng*)m_pOwner;
		pWrmDoc->UpdateLayout();
	}
}

//-----------------------------------------------------------------------------
void CRSFileNameProp::CheckIfIsAccessibleNameSpace()
{
	if (!IsFileName(GetValue(), TRUE, TRUE))
		SetDescription(_TB("! Attention !\nSelected file could not be accessed from other clients. In order to be accessed from all clients file must be located on a network folder."));
	else 
		SetDescription(_T(""));
}

//================================CRSFileExplorerProp===================================
//-----------------------------------------------------------------------------
CRSFileExplorerProp::CRSFileExplorerProp(const CString& strName, CString& strDirName, const CString& description)
	:
	CrsProp(strName, (_variant_t)strDirName, description)
{
	AllowEdit(FALSE);

	m_image.SetImageSize(CSize(14, 14));
	m_image.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_image);
	HICON icon = TBLoadPng(TBGlyph(szGlyphOpenFolder14));
	m_image.AddIcon(icon);
	::DestroyIcon(icon);
}

//-----------------------------------------------------------------------------
void CRSFileExplorerProp::OnClickButton(CPoint point)
{
	CString dirName = GetValue();

	::TBShellExecute(dirName);
}

//-----------------------------------------------------------------------------
void CRSFileExplorerProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	CBCGPToolbarButton button;

	CBCGPVisualManager::BCGBUTTON_STATE state = FALSE ? CBCGPVisualManager::ButtonsIsHighlighted : CBCGPVisualManager::ButtonsIsRegular;

	CBCGPVisualManager::GetInstance()->OnFillButtonInterior(pDC, &button, rectButton, state);
	m_image.DrawEx(pDC, rectButton, 0, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	CBCGPVisualManager::GetInstance()->OnDrawButtonBorder(pDC, &button, rectButton, state);
}

//================================CRSImgOriginProp===================================
//-----------------------------------------------------------------------------
CRSImgOriginProp::CRSImgOriginProp(CWoormDocMng* pWrmDocMng, PropertyType propType)
	:
	CrsProp(L"", (_variant_t)(LONG)0, L""),
	m_pWrmDocMng(pWrmDocMng),
	m_propType(propType)
{
	ASSERT_VALID(m_pWrmDocMng);

	switch (m_propType)
	{
	case PropertyType::XP:
	{
		EnableSpinControl(TRUE, 0, m_pWrmDocMng->m_PageInfo.GetPageSize_LP().cx);
		m_strName = _T("X px");
		break;
	}

	case PropertyType::YP:
	{
		EnableSpinControl(TRUE, 0, m_pWrmDocMng->m_PageInfo.GetPageSize_LP().cy);
		m_strName = _T("Y px");
		break;
	}

	case PropertyType::XM:
	{
		EnableSpinControl(TRUE, 0, m_pWrmDocMng->m_PageInfo.GetPageSize_LP().cx);
		m_strName = _T("X mm");
		break;
	}

	case PropertyType::YM:
	{
		EnableSpinControl(TRUE, 0, m_pWrmDocMng->m_PageInfo.GetPageSize_LP().cy);
		m_strName = _T("Y mm");
		break;
	}
	}

	m_strButtonText = _TB("Center");

	UpdatePropertyValue();

	m_image.SetImageSize(CSize(14, 14));
	m_image.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_image);
	HICON icon = TBLoadPng(TBGlyph(szGlyphCenterPoint));
	m_image.AddIcon(icon);
	::DestroyIcon(icon);
}

//-----------------------------------------------------------------------------
void CRSImgOriginProp::OnClickButton(CPoint point)
{
	__super::OnClickButton(point);
	CString strName(m_pWrmDocMng->m_pOptions->m_strBkgnBitmap);
	strName = AfxGetPathFinder()->FromNs2Path(strName, CTBNamespace::IMAGE, CTBNamespace::FILE);

	if (strName.IsEmpty() || !ExistFile(strName))
		return;

	CTBPicture	bitmap;
	VERIFY(bitmap.ReadFile(strName, TRUE));

	CSize sizePage = m_pWrmDocMng->m_PageInfo.GetPageSize_LP();
	CSize sizeBitmap(bitmap.GetWidth(), bitmap.GetHeight());

	switch (m_propType)
	{
	case PropertyType::XP:
	case PropertyType::XM:
	{
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.SetPoint((sizePage.cx - sizeBitmap.cx) / 2, m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y);
		break;
	}

	case PropertyType::YP:
	case PropertyType::YM:
	{
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.SetPoint(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x, (sizePage.cy - sizeBitmap.cy) / 2);
		break;
	}
	}

	UpdatePropertyValue();
	
	m_pWrmDocMng->UpdateLayout();
}

//-----------------------------------------------------------------------------
BOOL CRSImgOriginProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	if (((CString)GetValue()).IsEmpty())
		return TRUE;

	switch (m_propType)
	{
	case PropertyType::XP:
	{
		// convert from pix to mm		
		LONG propinmm = (LONG)floor(LPtoMU((int)GetValue(), CM, 10., 3));

		// set value in mm to another X property
		GetParent()->GetSubItem(2)->SetValue(propinmm);
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x = GetValue();
		
		break;
	}

	case PropertyType::YP:
	{
		// convert from pix to mm		
		LONG propinmm = (LONG)floor(LPtoMU((int)GetValue(), CM, 10., 3));

		// set value in mm to another Y property
		GetParent()->GetSubItem(3)->SetValue(propinmm);
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y = GetValue();

		break;
	}

	case PropertyType::XM:
	{
		// convert from mm to pix
		LONG propinpix  = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);

		// set value in pix to another X property
		GetParent()->GetSubItem(0)->SetValue(propinpix);
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x = propinpix;

		break;
	}

	case PropertyType::YM:
	{
		// convert from mm to pix
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);

		// set value in pix to another Y property
		GetParent()->GetSubItem(1)->SetValue(propinpix);
		m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y = propinpix;

		break;
	}
	}

	m_pWrmDocMng->UpdateLayout();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSImgOriginProp::UpdatePropertyValue()
{
	CBCGPProp* parent=GetParent();

	switch (m_propType)
	{
	case PropertyType::XP:
	{
		LONG val = (LONG)floor(LPtoMU(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x, CM, 10., 3));
		SetValue(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x);
		if (parent)
			parent->GetSubItem(2)->SetValue(val);
		break;
	}

	case PropertyType::YP:
	{
		LONG val = (LONG)floor(LPtoMU(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y, CM, 10., 3));
		SetValue(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y);
		if (parent)
			parent->GetSubItem(3)->SetValue(val);
		break;
	}

	case PropertyType::XM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x, CM, 10., 0));
		SetValue(val);
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.x);
		break;
	}

	case PropertyType::YM:
	{
		LONG val = (LONG)floor(LPtoMU(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y, CM, 10., 0));
		SetValue(val);
		if (parent)
			parent->GetSubItem(1)->SetValue(m_pWrmDocMng->m_pOptions->m_BitmapOrigin.y);
		break;
	}
	}
}

//*******************************************************************************
void CRSImgOriginProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	CBCGPToolbarButton button;
	
	CBCGPVisualManager::BCGBUTTON_STATE state = FALSE ? CBCGPVisualManager::ButtonsIsHighlighted : CBCGPVisualManager::ButtonsIsRegular;

	CBCGPVisualManager::GetInstance()->OnFillButtonInterior(pDC, &button, rectButton, state);
	m_image.DrawEx(pDC, rectButton, 0, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	CBCGPVisualManager::GetInstance()->OnDrawButtonBorder(pDC, &button, rectButton, state);
}

//================================CRSEmailParameterProp==================================
//-----------------------------------------------------------------------------
CRSEmailParameterProp::CRSEmailParameterProp(const CString& strName, FieldRect* pFieldRect, const CString& description, CRS_PropertyGrid* pPropGrid)
	:
	CrsProp(strName, L"", description),
	m_pFieldRect(pFieldRect)
{
	AllowEdit(FALSE);

	SetOwnerList(pPropGrid);
	UpdatePropertyValue();

	for (int i = FieldRect::EmailParameter::EP_None; i <= FieldRect::EmailParameter::EP_Last; i++)
	{
		//if (i == 7 && !AfxGetBaseApp()->IsDevelopment())
		//	continue;
		if (i > 10 && i < 21 && !AfxGetIMailConnector()->IsPostaLiteEnabled())
			continue;

		FieldRect::EmailParameter emailEnum = static_cast<FieldRect::EmailParameter>(i);
		CString emailString = FieldRect::EmailParamToString(emailEnum);

		AddOption(emailString, FALSE, emailEnum);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSEmailParameterProp::OnUpdateValue()
{
	int prevIndex = GetSelectedOption();

	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	DWORD_PTR option = GetOptionData(index);

	m_pFieldRect->m_eEmailParameter = (FieldRect::EmailParameter)option;

	if(prevIndex!=index)
		UpdatePropertyValue();

	m_pFieldRect->UpdateDocument();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSEmailParameterProp::UpdatePropertyValue()
{
	SetValue((_variant_t)FieldRect::EmailParamToString(m_pFieldRect->m_eEmailParameter));
	int prevSubItemsCount = this->GetSubItemsCount();
	//remove subproperty
	if (prevSubItemsCount > 0)
		this->RemoveAllSubItems();

	//add subproperty
	if (m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_Body ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_Subject ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_To ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_Cc ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_Bcc ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_Attachment ||
		m_pFieldRect->m_eEmailParameter == FieldRect::EmailParameter::EP_To_by_Certified)

		AddSubItem(new CRSBoolProp(this->m_pFieldRect, _TB("Append this mail part"), &(m_pFieldRect->m_bAppendMailPart), _TB("Append this mail part")));

	if(this->GetSubItemsCount() != prevSubItemsCount || prevSubItemsCount==1)
		GetPropertyGrid()->AdjustLayout();
}

//================================CRSAnchorToProp==================================
//-----------------------------------------------------------------------------
CRSAnchorToProp::CRSAnchorToProp(BaseRect* pBaseRect, CRS_PropertyGrid* propGrid)
	:
	CrsProp(_TB("Anchor To Table"), L"", _TB("Select the table on which to anchor the selected object")),
	m_pBaseRect(pBaseRect),
	m_pMulSel		(NULL),
	m_pLeftColumn	(NULL),
	m_pRightColumn	(NULL),
	m_pTables(NULL)
{
	SetOwnerList(propGrid);
	Initialize();
}

//-----------------------------------------------------------------------------
CRSAnchorToProp::CRSAnchorToProp(SelectionRect* pMulSel, CRS_PropertyGrid* propGrid)
	:
	CrsProp(_TB("Anchor To Table"), L"", _TB("Select the table on which to anchor selected objects")),
	m_pBaseRect		(NULL),
	m_pMulSel		(pMulSel),
	m_pLeftColumn	(NULL),
	m_pRightColumn	(NULL),
	m_pTables		(NULL)
{
	SetOwnerList(propGrid);
	Initialize();
}

//-----------------------------------------------------------------------------
CRSAnchorToProp::~CRSAnchorToProp()
{
	delete m_pTables;
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::Initialize()
{
	AllowEdit(FALSE);

	InitializeSubItem();
	AddTables();
	SelectCurrentTable();
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::InitializeSubItem()
{
	SetGroup(TRUE);
	
	//left
	m_pLeftColumn = new CRSAnchorToColumnProp(this, CRSAnchorToColumnProp::Side::Left);
	m_pLeftColumn->SetVisible(FALSE);
	AddSubItem(m_pLeftColumn);
	//right
	m_pRightColumn = new CRSAnchorToColumnProp(this, CRSAnchorToColumnProp::Side::Right);
	m_pRightColumn->SetVisible(FALSE);
	AddSubItem(m_pRightColumn);

	SetGroup(FALSE);
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::AddTables()
{
	if (this->GetOptionCount() > 0)
	{
		ASSERT(FALSE);
		RemoveAllOptions();
	}

	if (m_pBaseRect)
		m_pTables = m_pBaseRect->m_pDocument->GetTables();
	else if (m_pMulSel)
		m_pTables = m_pMulSel->m_pDocument->GetTables();

	if (m_pTables->GetCount() == 0)
		AddOption(_TB("No table present to anchor the field"), TRUE, -1);
	else
		AddOption(_TB("None"), TRUE, -1);

	if (!m_pTables || m_pTables->GetCount() == 0)
		return;
	
	for (POSITION pos = m_pTables->GetHeadPosition(); pos != NULL;)
	{
		Table* pTable = m_pTables->GetNext(pos);
		ASSERT_VALID(pTable);
		AddOption(pTable->GetName(TRUE), TRUE, (DWORD_PTR)pTable);
	}

}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::SelectCurrentTable()
{
	int index;
	//ho gi� caricato le tabelle nella combo quindi ora cerco quella gi� settata se � presente
	if (m_pBaseRect)
	{
		if (m_pBaseRect->m_AnchorLeftColumnID == 0)
		{
			index = this->GetOptionDataIndex(-1);
			ASSERT(index >= 0);
			SelectOption(index, FALSE);//None or No table present
			//vedere se devo uscire
		}

		else
		{
			Table* pTable = m_pBaseRect->m_pDocument->GetTableFromColumnId(m_pBaseRect->m_AnchorLeftColumnID);
			if (!pTable)
				ASSERT(FALSE);
			else
			{
				index = this->GetOptionDataIndex((DWORD_PTR)pTable);
				ASSERT(index >= 0);
				SelectOption(index, FALSE);
				AddColumns(pTable);
			}
		}
	}

	else if (m_pMulSel)
	{
		//int i;
		//Table* pCommonTable = NULL;
		//Table* pTable = NULL;

		//for (i = 0; i < m_pMulSel->GetSize(); i++)
		//{
		//	BaseObj* obj = m_pMulSel->GetObjAt(i);

		//	BaseRect* pBase = dynamic_cast<BaseRect*>(obj);
		//	if (!pBase)								//no base rect
		//		continue;

		//	if (pBase->m_AnchorLeftColumnID == 0)	//no anchor
		//		break; 

		//	pTable = pBase->m_pDocument->GetTableFromColumnId(pBase->m_AnchorLeftColumnID);

		//	if (pTable != NULL)
		//	{
		//		if (i == 0 || pCommonTable == NULL)
		//			pCommonTable = pTable;
		//		else if (pTable != pCommonTable) break;
		//	}

		//	else
		//		ASSERT(FALSE);
		//}	

		//if (i == m_pMulSel->GetSize() && pCommonTable)
		//{
		//	//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		//	index = this->GetOptionDataIndex((DWORD_PTR)pTable);
		//	ASSERT(index >= 0);
		//	SelectOption(index, FALSE);
		//	AddColumns(pCommonTable);
		//}

		//else 
		//{
			index = this->GetOptionDataIndex(-1);
			ASSERT(index >= 0);
			SelectOption(index, FALSE);//None or No table present
		//}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSAnchorToProp::OnUpdateValue()
{
	int prevIndex = GetSelectedOption();

	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0 )
		return FALSE;
	if (index == prevIndex)
		return TRUE;
	//rimuovo l'ancoraggio precedente
	if (m_pBaseRect)
	{
		m_pBaseRect->FreeFieldFromColumn(&m_pBaseRect->m_pDocument->GetObjects());
		m_pBaseRect->Redraw();
	}

	else if (m_pMulSel)
	{
		int i;

		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			BaseRect* pBase = dynamic_cast<BaseRect*>(obj);
			if (!pBase)			//no baserect					
				continue;

			pBase->FreeFieldFromColumn(&pBase->m_pDocument->GetObjects());
		}

		m_pMulSel->Redraw();
	}

	m_pLeftColumn->RemoveAllOptions();
	m_pLeftColumn->SetVisible(false);
	m_pRightColumn->RemoveAllOptions();
	m_pRightColumn->SetVisible(false);
	GetPropertyGrid()->AdjustLayout();
	//se � stata selezionata una tabella carico le colonne relative
	if (index != 0)
	{
		Table* pTable = (Table*)GetOptionData(index);
		ASSERT_VALID(pTable);
		AddColumns(pTable);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::AddColumns(Table* pTable)
{
	//default
	WORD leftColumnId = 0;
	WORD rightColumnId = 0;

	if (m_pBaseRect)
	{
		leftColumnId = m_pBaseRect->m_AnchorLeftColumnID;
		rightColumnId = m_pBaseRect->m_AnchorRightColumnID;
	}

	else if (m_pMulSel)
	{
		int i;
		WORD commonLeftId = NULL;
		WORD commonRightId = NULL;

		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			BaseRect* pBase = dynamic_cast<BaseRect*>(obj);
			if (!pBase)								//no base rect
				continue;

			leftColumnId = pBase->m_AnchorLeftColumnID;
			rightColumnId = pBase->m_AnchorRightColumnID;

			if (i == 0 || commonLeftId == NULL || commonRightId == NULL)
			{
				commonLeftId = leftColumnId;
				commonRightId = rightColumnId;
			}

			else if (commonLeftId != leftColumnId || commonRightId != rightColumnId) 
				break;
		}

		//fine ciclo
		if (i == m_pMulSel->GetSize())
		{
			//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property al valore comune
			leftColumnId = commonLeftId;
			rightColumnId = commonRightId;
		}

		else
		{
			//altrimenti non sono tutti uguali e setto la prima colonna
			leftColumnId = 0;
			rightColumnId = 0;
		}
	}

	//add Columns to subitems
	AddColumnsToSubItem(pTable, m_pLeftColumn, 0);				//Aggiungo alla tendina tutte le colonne
	AddColumnsToSubItem(pTable, m_pRightColumn, leftColumnId);	//Aggiungo solo le colonne con id superiore a quello di sx
	
	GetPropertyGrid()->AdjustLayout();

	m_leftId = leftColumnId;
	m_rightId = rightColumnId;

	//select Columns
	if (!leftColumnId)
	{
		int idLeft = m_pLeftColumn->GetOptionData(0);

		CPoint p = m_pBaseRect->m_BaseRect.TopLeft();
		p.y = pTable->m_BaseRect.CenterPoint().y;
		// correggo perche in posizione 'ancorata' il border del campo risulta in realt� nello spazio della colonna a fianco
		p.x = p.x + m_pBaseRect->GetBorderPen().GetWidth();
		int idxColLeft = pTable->GetColumnIdxByPoint(p);
		if (idxColLeft != -1 && idxColLeft < pTable->m_Columns.GetSize())
				idLeft = pTable->m_Columns[idxColLeft]->GetInternalID();
		
		int idRight = idLeft;
		p.x = m_pBaseRect->m_BaseRect.BottomRight().x -1; 
		// correggo di un pixel perch� un punto con coordinate BR, rispetto ad un rect, altrimenti,
		// risulterebbe fuori dallo spazio rect della colonna a cui invece appartiene
		int idxColRight = pTable->GetColumnIdxByPoint(p);
		if (idxColRight > idxColLeft)
			idRight = pTable->m_Columns[idxColRight]->GetInternalID();

		SetLeftColumn(idLeft);
		SetRightColumn(idRight);
	}

	else
	{
		SetLeftColumn(m_leftId);
		SetRightColumn(m_rightId);
	}
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::AddColumnsToSubItem(Table* pTable, CRSAnchorToColumnProp* pSubItem, WORD fromID)
{
	pSubItem->RemoveAllOptions();
	pSubItem->SetVisible(TRUE);

	int index = 0;
	//cerco l'indice della colonna della tabella che ha quell'id
	for (int i = 0; i < pTable->m_Columns.GetSize(); i++)
	{
		TableColumn* pCol = pTable->m_Columns[i];
		if (pCol->GetInternalID() == fromID)
		{ 
			index = i;
			break;
		}			
	}

	for (int i = index; i < pTable->m_Columns.GetSize(); i++)
	{
		TableColumn* pCol = pTable->m_Columns[i];	
		pSubItem->AddOption(pCol->GetFieldName(), TRUE, pCol->GetInternalID());
	}
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::SetLeftColumn(WORD leftId)
{
	int index = m_pLeftColumn->GetOptionDataIndex(leftId);
	if (index < 0)
	{
		index = 0;
		m_leftId = 0;
	}
	else 
		m_leftId = leftId;

	m_pLeftColumn->SelectOption(index , FALSE);	
	AddColumnsToSubItem(GetPropertyView()->GetDocument()->GetTableFromColumnId(leftId), m_pRightColumn, leftId);

	Anchor();
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::SetRightColumn(WORD rightId)
{
	int index = m_pRightColumn->GetOptionDataIndex(rightId);
	if (index < 0)
		m_rightId = 0;
	else
		m_rightId = rightId;

	m_pRightColumn->SelectOption(index > 0 ? index : 0, FALSE);

	Anchor();
}

//-----------------------------------------------------------------------------
void CRSAnchorToProp::Anchor()
{
	if (m_leftId == 0)
	{
		m_pBaseRect->FreeFieldFromColumn(&m_pBaseRect->m_pDocument->GetObjects());
		return;
	}

	int index = m_pRightColumn->GetOptionDataIndex(m_rightId);

	if (index < 0 || m_rightId == m_leftId)
	{
		m_rightId = 0;
		m_pRightColumn->SelectOption(m_rightId, FALSE);
	}

	if (m_pBaseRect)
	{
		m_pBaseRect->FreeFieldFromColumn(&m_pBaseRect->m_pDocument->GetObjects());
		m_pBaseRect->AnchorFieldToColumn(&m_pBaseRect->m_pDocument->GetObjects(),m_leftId, m_rightId);
		m_pBaseRect->Redraw();
		//m_pBaseRect->m_pDocument->Invalidate(TRUE);
		//m_pBaseRect->m_pDocument->UpdateWindow();
	}

	else if (m_pMulSel)
	{
		m_pMulSel->m_pDocument->m_pActiveRect->Clear();
		m_pMulSel->m_pDocument->Invalidate(FALSE);
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			BaseRect* pBase = dynamic_cast<BaseRect*>(obj);
			if (!pBase)//no base rect							
				continue;
			pBase->FreeFieldFromColumn(&pBase->m_pDocument->GetObjects());
			pBase->AnchorFieldToColumn(&pBase->m_pDocument->GetObjects(), m_leftId, m_rightId);
		}

		m_pMulSel->m_pDocument->Invalidate(TRUE);
		//m_pMulSel->Redraw();
		//m_pMulSel->m_pDocument->m_pActiveRect->Redraw();
		//m_pMulSel->m_pDocument->UpdateWindow();
		//m_pMulSel->m_pDocument->m_pActiveRect->Redraw();
	}
}

//================================CRSAnchorToColumnProp==================================
//-----------------------------------------------------------------------------
CRSAnchorToColumnProp::CRSAnchorToColumnProp(CRSAnchorToProp* pParent, Side eSide)
	:
	CrsProp(L"", L"", L""),
	m_pParent(pParent),
	m_eSide(eSide)
{
	AllowEdit(FALSE);

	if (m_eSide == Side::Left)
	{
		SetName(_TB("Left Column"));
		SetDescription(_TB("Set left column to anchor the selected objects"));
	}

	else
	{
		SetName(_TB("Right Column"));
		SetDescription(_TB("Set right column to anchor the selected objects"));
	}

}

//-----------------------------------------------------------------------------
BOOL CRSAnchorToColumnProp::OnUpdateValue()
{
	int prevIndex = GetSelectedOption();

	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return FALSE;
	if (index == prevIndex)
		return TRUE;

	if (m_eSide == Side::Left)
		m_pParent->SetLeftColumn((WORD)GetOptionData(index));
	else
		m_pParent->SetRightColumn((WORD)GetOptionData(index));

	return baseUpdate;
}

//================================CRSExpressionProp==================================
//-----------------------------------------------------------------------------
CRSExpressionProp::CRSExpressionProp (
	const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, 
	CRS_ObjectPropertyView* pPropertyView, const CString& description, BOOL bAllowEmpty/* = TRUE*/, BOOL bEditInPlace /*= FALSE*/
	)
	:
	CrsProp(strName, (*ppExp) == NULL ? (LPCTSTR)L"" : (LPCTSTR)(*ppExp)->ToString(), description),
	m_ppExp(ppExp),
	m_dataType(dataType),
	m_psymTable(pSymTable),
	m_pPropertyView(pPropertyView),
	m_bAllowEmpty(bAllowEmpty),
	m_bViewMode(TRUE),
	m_bEditInPlace(bEditInPlace)
{
	ASSERT_VALID(m_psymTable);
	ASSERT_VALID(m_pPropertyView);

	AllowEdit(m_bEditInPlace);
}

CRSExpressionProp::CRSExpressionProp(
	const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable,
	CWoormDocMng* pWDoc, const CString& description, BOOL bAllowEmpty/* = TRUE*/, BOOL bEditInPlace /*= FALSE*/
)
	:
	CrsProp(strName, (*ppExp) == NULL ? (LPCTSTR)L"" : (LPCTSTR)(*ppExp)->ToString(), description),

	m_ppExp(ppExp),
	m_dataType(dataType),
	m_psymTable(pSymTable),
	m_pWDoc(pWDoc),
	m_bAllowEmpty(bAllowEmpty),
	m_bViewMode(TRUE),
	m_bEditInPlace(bEditInPlace)
{
	ASSERT_VALID(m_psymTable);
	ASSERT_VALID(pWDoc);

	AllowEdit(m_bEditInPlace);
}

//-----------------------------------------------------------------------------
CRSExpressionProp::CRSExpressionProp (
	InitialValue eInitialValue, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable,
	CRS_ObjectPropertyView* pPropertyView, const CString& description, BOOL bEditInPlace /*= FALSE*/
	)
	:
	CrsProp(strName, eInitialValue==InitialValue::IntValue ? (_variant_t)0 : (LPCTSTR)L"", description),
	m_ppExp(ppExp),
	m_dataType(dataType),
	m_psymTable(pSymTable),
	m_pPropertyView(pPropertyView),
	m_bAllowEmpty(TRUE),
	m_bViewMode(TRUE),
	m_bEditInPlace(bEditInPlace)
{
	ASSERT_VALID(m_psymTable);
	ASSERT_VALID(m_pPropertyView);

	AllowEdit(m_bEditInPlace);
}

//-----------------------------------------------------------------------------
void CRSExpressionProp::OnClickButton(CPoint point)
{
	CRSEditView* pEditView = NULL;
	if (GetPropertyView()) 
	{
		ASSERT_VALID(GetPropertyView());
		pEditView = GetPropertyView()->CreateEditView();
	}

	else
	{
		ASSERT_VALID(m_pWDoc);
		if (!m_pWDoc)
			return;

		pEditView = dynamic_cast<CRSEditView*>(m_pWDoc->CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
		ASSERT_VALID(pEditView);

		pEditView->GetEditCtrl()->SelectLine(1);
	}

	if (!pEditView)
		return;
	if (m_dataType == DataType::String || m_dataType == DataType::Text)
		pEditView->EnableStringPreview();
	pEditView->LoadElement(
		m_psymTable,
		m_ppExp,
		m_dataType,
		m_bViewMode,
		NULL,
		m_bAllowEmpty,
		this->GetDesciption()
		);
	pEditView->DoEvent();

	ASSERT_VALID(this);
	UpdatePropertyValue();
	
	UpdateDocument();

	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();

	if (GetPropertyView() && GetPropertyView()->m_pTreeNode->m_pItemData->IsKindOf(RUNTIME_CLASS(Table)))
	{
		//aggiornamento icona visibilit�
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, GetPropertyView()->m_pTreeNode);
	}
}

//-----------------------------------------------------------------------------
void CRSExpressionProp::UpdatePropertyValue()
{
	SetColoredState(CrsProp::State::Normal);
	//aggiorniamo il valore della property
	//NO REFACTORING
	if (!(*m_ppExp) || (*m_ppExp)->IsEmpty())
		SetValue((LPCTSTR)L"");
	else
		SetValue((LPCTSTR)(*m_ppExp)->ToString());
}

//-----------------------------------------------------------------------------
BOOL CRSExpressionProp::OnEndEdit()
{
	BOOL bBaseEndEdit = __super::OnEndEdit();

	if (m_bEditInPlace)
	{
		CString sExpr = GetValue();

		if (sExpr.IsEmpty() && !m_bAllowEmpty)
		{
			SetColoredState(CrsProp::State::Error);
			SetNewDescription(_TB("Error, empty expression is not allowed"), FALSE);
		}

		Expression* pTempExpr = new Expression(m_psymTable);
		Parser lex(sExpr);

		if (pTempExpr->Parse(lex, m_dataType, TRUE))
		{
			//ok
			SetColoredState(CrsProp::State::Normal);
			SetOriginalDescription();

			if (*m_ppExp)
			{
				ASSERT_VALID(*m_ppExp);
				(*m_ppExp)->Empty();
				VERIFY((*m_ppExp)->Parse(sExpr, m_dataType, TRUE));
			}

			else
				*m_ppExp = pTempExpr;
		}

		else
		{
			//errore
			SetColoredState(CrsProp::State::Error);

			CString sError = lex.BuildErrMsg(TRUE);
			lex.ClearError();

			CString sAux = pTempExpr->GetErrDescription(FALSE);

			if (!sAux.IsEmpty())
				sError = sAux + L"\r\n" + sError;

			SetNewDescription(sError, FALSE);

			SAFE_DELETE(pTempExpr);
		}

		GetPropertyGrid()->AdjustLayout();
	}
	
	return bBaseEndEdit;
}

//================================CRSExpressionExtendedProp==================================
//-----------------------------------------------------------------------------
CRSExpressionExtendedProp::CRSExpressionExtendedProp(CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description, BOOL bEditInPlace)
	:
	CRSExpressionProp(strName, ppExp, dataType, pSymTable, pPropertyView, description, TRUE, bEditInPlace),
	m_pOwner(pOwner)
{
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
CRSExpressionExtendedProp::CRSExpressionExtendedProp(InitialValue eInitialValue, CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description, BOOL bEditInPlace)
	:
	CRSExpressionProp(eInitialValue, strName, ppExp, dataType, pSymTable, pPropertyView, description, bEditInPlace),
	m_pOwner(pOwner)
{
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
CRSExpressionExtendedProp::~CRSExpressionExtendedProp()
{}

//-----------------------------------------------------------------------------
void CRSExpressionExtendedProp::UpdateDocument()
{
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();
}

//================================CRSSqlExpressionProp==================================
//-----------------------------------------------------------------------------
CRSSqlExpressionProp::CRSSqlExpressionProp(WoormField* pWoormField, TblRuleData* pTblRule, DataFieldLink* pObjLink, CRS_ObjectPropertyView* pPropertyView, CNodeTree* pNode)
	:
	CrsProp(_TB("Expression calculated column"), (_variant_t)pObjLink->m_strPhysicalName, _TB("Opens the edit view")),
	m_pWoormField(pWoormField),
	m_pTblRule(pTblRule),
	m_pObjLink(pObjLink),
	m_pPropertyView(pPropertyView),
	m_pTreeNode(pNode)
{
	ASSERT_VALID(pWoormField);
	AllowEdit(FALSE);
	SetColoredState(CrsProp::State::Mandatory);
}

//-----------------------------------------------------------------------------
void CRSSqlExpressionProp::OnClickButton(CPoint point)
{
	CRSReportTreeView* pReportTreeView = m_pPropertyView->GetDocument()->GetWoormFrame()->GetEngineTreeView();

	if (!pReportTreeView->CanOpenEditor(m_pTreeNode))
		return;

	CRSEditView* pEditView = pReportTreeView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->m_Context.SetNodeTree(m_pTreeNode);

	if (pEditView->GetEditorFrame(FALSE))
	{
		pEditView->ShowDiagnostic(L"");
		pEditView->GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(m_pTblRule, NULL);
		pEditView->SetLanguage(L"SQL", FALSE);
	}

	pEditView->SetText(m_pObjLink->m_strPhysicalName);

	pEditView->DoEvent();
	
	if (this)
		UpdatePropertyValue();
	else
		m_pPropertyView->ReLoadPropertyGrid();
}

//-----------------------------------------------------------------------------
void CRSSqlExpressionProp::UpdatePropertyValue()
{
	SetValue((LPCTSTR)m_pObjLink->m_strPhysicalName);
}

//================================CRSColumnWidthWithExprProp==================================
//-----------------------------------------------------------------------------
CRSColumnWidthWithExprProp::CRSColumnWidthWithExprProp(TableColumn* pCol, const CString& strName, int width, Expression** ppExp, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description, InitialValue eInitialValue, WidthCoor type, BOOL hasButton)
	:
	CRSExpressionExtendedProp(eInitialValue, pCol, strName, ppExp, DataType::Integer , pSymTable, pPropertyView, description),
	m_pCol(pCol),
	m_pTable(pCol->GetTable()) ,
	type(type) ,
	hasButton(hasButton)
{
	ASSERT_VALID(m_pOwner);
	EnableSpinControl(TRUE, 0, 1000);
	AllowEdit(TRUE);
	SetValue(width);
		
	SetStateImg(CRS_PropertyGrid::Img::QuestionMark);
}

//-----------------------------------------------------------------------------
void CRSColumnWidthWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (*m_ppExp && !(*m_ppExp)->IsEmpty())
	{
		CrsProp::OnDrawStateIndicator(pDC, rect);	//draw question mark

		CString expr = (*m_ppExp)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
BOOL CRSColumnWidthWithExprProp::OnUpdateValue()
{
	int previousValue = (int)this->GetValue();
	int value=0;
	CBCGPProp* parent = GetParent();
	BOOL baseUpdate = __super::OnUpdateValue();
	if (previousValue != (int)this->GetValue())
	{
		switch (type)
		{
		case WidthCoor::WidthP:
		{
			value = (int)GetValue();
			if (parent)
			{
				int widthInMm = (int)floor(LPtoMU(value, CM, 10., 3));
				parent->GetSubItem(1)->SetValue(widthInMm);
			}

			break;
		}

		case WidthCoor::WidthM:
		{
			if (parent)
			{
				value = MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
				parent->GetSubItem(0)->SetValue(value);
			}

			break;
		}
		}

		//solo se il valore statico della larghezza � cambiato
		ASSERT_VALID(m_pTable);
		ASSERT_VALID(m_pCol);
		if (!m_pCol->IsHidden())
		m_pTable->OnColumnSetWidth(m_pCol, value, TRUE);
		m_pCol->m_nWidth = m_pCol->m_nSavedWidth = value;
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSColumnWidthWithExprProp::UpdateDocument()
{
	if ((!*m_ppExp || (*m_ppExp)->IsEmpty())) //se ho espressione nulla o vuota ridisegno la colonna della larghezza statica
	{
		int value = 0;
		switch (type)
		{
		case WidthCoor::WidthP:
		{
			value = (int)GetValue();
			break;
		}

		case WidthCoor::WidthM:
		{	
				value =MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
			break;
		}
		}

		if (!m_pCol->IsHidden())
		m_pTable->OnColumnSetWidth(m_pCol, value, TRUE);
	}

	else
	{
		m_pTable->m_pDocument->InvalidateRect(m_pTable->GetRectToInvalidate(), TRUE);

		DataInt width;
		if (!m_pCol->m_pDynamicWidthExpr->Eval(width))
			return;
	
		m_pTable->Redraw(TRUE, 0, width);

		CRect oldRect = m_pCol->GetColumnTitleRect();
		CRect newRect(oldRect.TopLeft().x, oldRect.TopLeft().y, oldRect.TopLeft().x + (LONG)width, oldRect.BottomRight().y);
		
		m_pTable->m_pDocument->m_pActiveRect->SetActive(newRect);
	}
}

/*
//================================CRSDialogProp==================================
//-----------------------------------------------------------------------------
CRSDialogProp::CRSDialogProp(CObject* pOwner, const CString& strName, const _variant_t defValue, PropertyType propType, const CString& description)
	:
	CrsProp(strName, defValue, description),
	m_pOwner(pOwner),
	m_pPropertyType(propType)
{
	AllowEdit(FALSE);
	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSDialogProp::OnClickButton(CPoint point)
{
	ASSERT_VALID(m_pOwner);

	switch (m_pPropertyType)
	{
	case(PropertyType::FielRectValueAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pField = (FieldRect*)m_pOwner;
		pField->SetAlign();
		break;
	}

	case(PropertyType::FieldRectLabelAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pField = (FieldRect*)m_pOwner;
		pField->SetLabelAlign();
		break;
	}

	case(PropertyType::BaseRectAnchorToColumn) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
			break;
		BaseRect* pBase = (BaseRect*)m_pOwner;
		pBase->SetAnchorColumn();
		break;
	}

	case(PropertyType::ResizeKeepingAspectRatio) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pFieldObj = (FieldRect*)m_pOwner;
		pFieldObj->OnResizeProportional();
		break;
	}

	case(PropertyType::CutImage) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pFieldObj = (FieldRect*)m_pOwner;
		if (!pFieldObj->m_bIsCutted)
			pFieldObj->OnCutBitmap();
		else
			pFieldObj->OnCancelCut();
		break;
	}

	case(PropertyType::SetOriginalSize) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pFieldObj = (FieldRect*)m_pOwner;
		pFieldObj->RefreshStandardSize();
		break;
	}

	case(PropertyType::TableTitleAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;
		pTabObj->OnTableTitleAlign();
		break;
	}

	case(PropertyType::AllColumnTitlesAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;

		AlignType commonAlign = (AlignType)GetValue();

		CAlignDlg   dialog(commonAlign);
		dialog.SetAllowFieldSet(TRUE);

		if (dialog.DoModal() != IDOK)
		{
			if (!pTabObj->m_pDocument->IsModified())
				pTabObj->m_pDocument->SetModifiedFlag(pTabObj->m_pDocument->m_pFontStyles->IsModified());
			return;
		}

		pTabObj->SetAllColumnsTitleAlign(commonAlign);

		pTabObj->Redraw();
		break;
	}

	case(PropertyType::ColumnTitleAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		//pTabObj->Redraw(TRUE);
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		pColObj->GetTable()->OnColumnTitleAlign();
		break;
	}

	case(PropertyType::ColumnBodyAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		//pTabObj->Redraw(TRUE);
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		pColObj->GetTable()->OnColumnAlign();
		break;
	}

	case(PropertyType::SubtotalAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		
		CAlignDlg   dialog(pColObj->m_SubTotal.m_nAlign);
		dialog.SetAllowFieldSet(TRUE);

		if (dialog.DoModal() != IDOK)
		{
			if (!pColObj->GetTable()->m_pDocument->IsModified())
				pColObj->GetTable()->m_pDocument->SetModifiedFlag(pColObj->GetTable()->m_pDocument->m_pFontStyles->IsModified());
			return;
		}

		pColObj->Redraw();
		break;
	}

	case(PropertyType::TextRectAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TextRect)))
			break;
		TextRect* pText = (TextRect*)m_pOwner;
		pText->SetAlign();
		break;
	}

	case(PropertyType::TableCellValueAlign) :
	{
		if (dynamic_cast<TotalCell*>(m_pOwner))
		{
			((TotalCell*)m_pOwner)->m_pColumn->GetTable()->OnTotalAlign();
		}

		else if (m_pOwner->GetRuntimeClass() == RUNTIME_CLASS(TableCell))
		{
			((TableCell*)m_pOwner)->m_pColumn->GetTable()->OnCellAlign();
		}

		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSDialogProp::UpdatePropertyValue()
{
	switch (m_pPropertyType)
	{
	case(PropertyType::FielRectValueAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pField = (FieldRect*)m_pOwner;

		SetValue((int)pField->m_Value.GetAlign());
		break;
	}

	case(PropertyType::FieldRectLabelAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(FieldRect)))
			break;
		FieldRect* pField = (FieldRect*)m_pOwner;

		SetValue((int)pField->m_Label.GetAlign());
		break;
	}

	case(PropertyType::BaseRectAnchorToColumn) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(BaseRect)))
			break;
		BaseRect* pBase = (BaseRect*)m_pOwner;

		WORD leftColumnId = pBase->m_AnchorLeftColumnID;
		WORD rightColumnId = pBase->m_AnchorRightColumnID;

		if (leftColumnId == 0)//not anchored
		{
			SetValue(_T(""));
			break;
		}

		//left column
		const TableColumn* pLeftCol = pBase->m_pDocument->m_Layouts.FindColumnByID(leftColumnId, pBase->m_pDocument->m_dsCurrentLayoutEngine);

		if (rightColumnId == 0 || leftColumnId == rightColumnId) //anchored to left Column only
		{
			SetValue((_variant_t)pLeftCol->GetDescription());
			break;
		}

		//right column
		const TableColumn* pRightCol = pBase->m_pDocument->m_Layouts.FindColumnByID(rightColumnId, pBase->m_pDocument->m_dsCurrentLayoutEngine);

		//anchored to left and right
		SetValue((_variant_t)(
			_TB("From\n") +
			pLeftCol->GetDescription() +
			_TB("\nTo\n") +
			pRightCol->GetDescription()));
		break;
	}

	case(PropertyType::TableTitleAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;
		SetValue((int)pTabObj->m_Title.GetAlign());
		break;
	}

	case(PropertyType::AllColumnTitlesAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(Table)))
			break;
		Table* pTabObj = (Table*)m_pOwner;

		AlignType commonAlign = DEFAULT_ALIGN;
		int i = 0;

		for (i = 0; i < pTabObj->m_Columns.GetSize(); i++)
		{
			TableColumn* pCol = (TableColumn*)pTabObj->m_Columns[i];
			if (!pCol)
			{
				ASSERT(FALSE);
				break;
			}	
			
			if (i == 0 || commonAlign == 0)
				commonAlign = pCol->GetColumnTitleAlign();

			else if (commonAlign != pCol->GetColumnTitleAlign())
				break;
		}

		if (i == pTabObj->m_Columns.GetUpperBound() && commonAlign != 0)
			SetValue((int)commonAlign);
		else
			SetValue(0);

		break;
	}

	case(PropertyType::ColumnTitleAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SetValue((int)pColObj->GetColumnTitleAlign());
		break;
	}

	case(PropertyType::ColumnBodyAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SetValue((int)pColObj->GetColumnAlign());
		break;
	}

	case(PropertyType::SubtotalAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableColumn)))
			break;
		TableColumn* pColObj = (TableColumn*)m_pOwner;
		SetValue((int)pColObj->m_SubTotal.GetAlign());
		break;
	}

	case(PropertyType::TextRectAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TextRect)))
			break;
		TextRect* pText = (TextRect*)m_pOwner;
		SetValue((int)pText->m_StaticText.GetAlign());
		break;
	}

	case(PropertyType::TableCellValueAlign) :
	{
		if (!m_pOwner->IsKindOf(RUNTIME_CLASS(TableCell)))
			break;
		TableCell* pCell = (TableCell*)m_pOwner;
		SetValue((int)pCell->GetCellAlign());
		break;
	}
	}
} 

*/

//================================CRSBarCodeGroupProp==================================
// Classe per la gestione del gruppo di propriet� relative al Barcode
//-----------------------------------------------------------------------------
CRSBarCodeGroupProp::CRSBarCodeGroupProp()
	:
	CrsProp(_TB("Barcode settings"), TRUE)
{}

//-----------------------------------------------------------------------------
CRSBarCodeGroupProp::~CRSBarCodeGroupProp()
{
	delete m_lstDependentProp;
}

//-----------------------------------------------------------------------------
void CRSBarCodeGroupProp::UpdateDependantProp(CRSBarCodeProp* fromProp)
{
	POSITION pos = m_lstDependentProp->GetHeadPosition();
	BOOL foundStartPoint = (fromProp == NULL);
	while (pos)
	{		
		CRSBarCodeProp* prop;
		prop = m_lstDependentProp->GetNext(pos);
		ASSERT_VALID(prop);

		if (!foundStartPoint)
		{
			if (prop == fromProp)
				foundStartPoint = TRUE;
			//la property 'from' � sempre esclusa
			continue;
		}

		if (prop->IsKindOf(RUNTIME_CLASS(CRSBarCodeProp)))
		{
			CRSBarCodeProp* propEx = (CRSBarCodeProp*)prop;
			propEx->UpdatePropertyLayout();
		}
	}

}


//================================CRSBarCodeProp==================================
// Classe da cui derivano le propriet� per i barcode
//-----------------------------------------------------------------------------
CRSBarCodeProp::CRSBarCodeProp(CObject* pOwner, const CString& strName, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description)
	:
	CrsProp(strName, L"", description),
	m_pOwner(pOwner),
	m_pBarCode(pBarCode),
	m_pPropertyView(propertyView)
{
}

//-----------------------------------------------------------------------------
CRSBarCodeProp::CRSBarCodeProp(CObject* pOwner, const CString& strName, BOOL* pValue, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description)
	:
	CrsProp(strName, *pValue == 1 ? (_variant_t)true : (_variant_t)false, description),
	m_pOwner(pOwner),
	m_pBarCode(pBarCode),
	m_pPropertyView(propertyView)
{

	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	return baseUpdate;
}


//================================CRSBarCodeComboProp==================================
// classe per le propriet� di tipo comboBox dei barcode  che permettono la scelta fra 'prendi da variabile/prendi da valore')
//-----------------------------------------------------------------------------
CRSBarCodeComboProp::CRSBarCodeComboProp(CObject* pOwner, const CString& strName, CBarCode* pBarCode, const CString& description, CRS_ObjectPropertyView* propertyView, CRSBarCodeProp::PropertyType propType)
	: CRSBarCodeProp(pOwner, strName, pBarCode, propertyView, description),
	m_propType(propType),
	m_nCurrOption(0)
{
	AllowEdit(FALSE);
	UpdatePropertyLayout(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;

	BOOL valueChanged = m_nCurrOption != index;

	if(valueChanged)
	{
		if(m_propType == CRSBarCodeProp::PropertyType::TypeFromField)
		{
			//unico valore da aggiornare se sono in update
			// gli altri li aggiorno nella DrawProperties per aggiornarli sempre quando cambia tipo di barcode
			if (index == 0)
			{
				m_pBarCode->m_nBarCodeTypeAlias = 0;
				m_pBarCode->m_nBarCodeType = CBarCodeTypes::BC_DEFAULT;
			}

			((CRSBarCodeGroupProp*)m_pParent)->UpdateDependantProp(this);
		}	

		m_nCurrOption = index;
		DrawProperties();

		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw(); 
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSBarCodeComboProp::UpdatePropertyLayout(BOOL defaultValue)
{
	BOOL bTypeFromField;

	CRSBarCodeGroupProp* bcPropGroup = (CRSBarCodeGroupProp*)GetParent();
	if (m_propType == PropertyType::TypeFromField || bcPropGroup == NULL)
	{
		bTypeFromField = m_pBarCode->m_nBarCodeTypeAlias > 0;
	}
	else
	{
		//il refresh � guidato da 'barcode type da campo/valore' 
		//barcode type da field
		CRSBarCodeComboProp* typeFrom = (CRSBarCodeComboProp*)bcPropGroup->m_lstDependentProp->GetHead();
		if (typeFrom == NULL) return;
		bTypeFromField = typeFrom->GetSelectedOption() == 1;
	}

	BOOL b2DBarcode = m_pBarCode->Is2DBarcode();
	int barcodeType = m_pBarCode->m_nBarCodeType != 0 ? m_pBarCode->m_nBarCodeType : m_pBarCode->m_nBCDefaultType;

	//from type
	switch (m_propType)
	{
		case PropertyType::EncodingTypeFromField:
		{		
			if (bTypeFromField)
			{
				SetName(_TB("Check Digit/Encoding Mode From"));
				SetVisible(TRUE);
			}
			else
			{
				//barcode type da valore
				SetName(b2DBarcode ? _TB("Encoding Mode from") : _TB("Checksum Digit Module From"));
				SetDescription(b2DBarcode ? _TB("Select source of Encoding Mode: from a selected value or an other field") : _TB("Select source of Checksum Digit Module: from a selected value or an other field"));
				SetVisible(m_pBarCode->IsCheckEncodigEnabled());
			}
			break;
		}
		case PropertyType::VersionFromField:
		{
			//SetVisible(b2DBarcode || bTypeFromField); //per ora la version non funziona per QR e Micro-QR
			SetVisible(barcodeType == BC_PDF417 || barcodeType == BC_DATAMATRIX || bTypeFromField);
			break;
		}
		case PropertyType::ErrorCorrectionLevelFromField:
		{
			SetVisible( b2DBarcode && barcodeType != BC_DATAMATRIX || bTypeFromField);
			break;
		}
	}

	if (IsVisible())
	{
		if (m_propType != PropertyType::TypeFromField)
			//SetEnable(!bTypeFromField, FALSE);
			SetVisible(!bTypeFromField);
		AddOption(_TB("Value"), TRUE, 0);
		AddOption(_TB("Field"), TRUE, 1);

		if (defaultValue)
			SelectOption(bTypeFromField ? 1 : 0, FALSE);
		else if (!bTypeFromField
				&& (m_propType == PropertyType::TypeFromField
					|| m_propType == PropertyType::EncodingTypeFromField && m_pBarCode->m_sCheckEncodeFieldName.IsEmpty()
					|| m_propType == PropertyType::VersionFromField && m_pBarCode->m_s2DVersionFieldName.IsEmpty()
					|| m_propType == PropertyType::ErrorCorrectionLevelFromField && m_pBarCode->m_sErrCorrLevelFieldName.IsEmpty()
					)
				)
			
		{
			//from value
			SelectOption(0, FALSE);
			m_nCurrOption = 0;
		}	
		else
		{
			//from field
			SelectOption(1, FALSE);
			m_nCurrOption = 1;
		}

		DrawProperties(defaultValue);
	}
}

//-----------------------------------------------------------------------------
void CRSBarCodeComboProp::DrawProperties(BOOL bDefaultValue)
{
	int selectedValue = -1;
	int bcType = m_pBarCode->m_nBarCodeType == 0 ? m_pBarCode->m_nBCDefaultType : m_pBarCode->m_nBarCodeType;
	if (m_pWndList != NULL)
	{ 
		int index = GetSelectedOption();
		if (index < 0)
			return;
		DWORD_PTR option = GetOptionData(index);
		selectedValue = (int)option;
	}
	else
	{	//recupero i valori dal barcode (sto disegnando la property view)
		if (m_pBarCode->m_nBarCodeTypeAlias <= 0
			&& (m_propType == PropertyType::TypeFromField
				|| m_propType == PropertyType::EncodingTypeFromField && m_pBarCode->m_sCheckEncodeFieldName.IsEmpty()
				|| m_propType == PropertyType::VersionFromField && m_pBarCode->m_s2DVersionFieldName.IsEmpty()
				|| m_propType == PropertyType::ErrorCorrectionLevelFromField && m_pBarCode->m_sErrCorrLevelFieldName.IsEmpty()
				)
			)
			selectedValue = 0;
		else
			selectedValue = 1;
	}

	this->m_bGroup = TRUE;

	if (this->GetSubItemsCount() > 0)
		this->RemoveAllSubItems();

	BOOL bCheckEncondingEnabled = m_pBarCode->IsCheckEncodigEnabled();
	if (selectedValue == 0)
	{
		//from type
		switch (m_propType)
		{
			case PropertyType::TypeFromField:
			{
				AddSubItem(new CRSBarCodeTypeComboProp(m_pOwner, m_pBarCode));
				break;
			}
			case PropertyType::EncodingTypeFromField:
			{
				if(IsVisible())
				{ 
					CRSBarCodeEncodingComboProp* encodingMode = new CRSBarCodeEncodingComboProp(m_pOwner, m_pBarCode, bDefaultValue);
					AddSubItem(encodingMode);
					if (bDefaultValue)
					{ 
						m_pBarCode->m_sCheckEncodeFieldName = _T("");
						m_pBarCode->m_nCheckSumType = m_pBarCode->Is2DBarcode() ? -2 : -1;
					}
				}
				break;
			}
			case PropertyType::VersionFromField:
			{
				if (IsVisible())
				{
					if(bcType == BC_PDF417)
					{
						CRSBarCodeSizeProp* nRows = new CRSBarCodeSizeProp(m_pOwner, m_pBarCode, m_pPropertyView, PropertySizeType::RowsNo, bDefaultValue);
						CRSBarCodeSizeProp* nColumns = new CRSBarCodeSizeProp(m_pOwner, m_pBarCode, m_pPropertyView, PropertySizeType::ColumnsNo, bDefaultValue);
						AddSubItem(nRows);
						AddSubItem(nColumns);						
					}
					else
					{
						CRSBarCodeVersionComboProp* version = new CRSBarCodeVersionComboProp(m_pOwner, m_pBarCode, bDefaultValue);
						AddSubItem(version);
					}
					if (bDefaultValue)
					{ 
						m_pBarCode->m_s2DVersionFieldName = _T("");
						m_pBarCode->m_n2DVersion = -1;
					}
				}

				break;
			}
			case PropertyType::ErrorCorrectionLevelFromField:
			{
				if (IsVisible())
				{
					CRSBarCodeErrCorrLevelComboProp* errCorrLevel = new CRSBarCodeErrCorrLevelComboProp(m_pOwner, m_pBarCode, bDefaultValue);
					AddSubItem(errCorrLevel);
					if (bDefaultValue)
					{
						m_pBarCode->m_sErrCorrLevelFieldName = _T("");
						int bcType = m_pBarCode->m_nBarCodeType == CBarCodeTypes::BC_DEFAULT ? m_pBarCode->m_nBCDefaultType : m_pBarCode->m_nBarCodeType;
						m_pBarCode->m_nErrCorrLevel = bcType == BC_PDF417 ? -2 : -1;
					}					
				}
				break;
			}
		}
	}
	else
	{
		//from field
		if(IsVisible())
		{ 
			if(m_propType == PropertyType::TypeFromField)
			{
				CRSBarCodeFieldComboProp* pFieldTypeProp = new CRSBarCodeFieldComboProp(m_pOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), m_propType, bDefaultValue);
				AddSubItem(pFieldTypeProp);
				pFieldTypeProp->SetName(_TB("Type source"));
				CRSBarCodeFieldComboProp* pFieldEncProp = new CRSBarCodeFieldComboProp(m_pOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), PropertyType::EncodingTypeFromField, bDefaultValue);
				AddSubItem(pFieldEncProp); 
				pFieldEncProp->SetName(_TB("Encoding/Checksum source"));
				pFieldEncProp->SetDescription(_TB("Select field of integer type that is source of Encoding Mode/Checksum Digit Module."));
				CRSBarCodeFieldComboProp* pFieldVersionProp = new CRSBarCodeFieldComboProp(m_pOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), PropertyType::VersionFromField, bDefaultValue);
				AddSubItem(pFieldVersionProp);
				pFieldVersionProp->SetName(_TB("Version source"));
				CRSBarCodeFieldComboProp* pFieldErrCorrLevelProp = new CRSBarCodeFieldComboProp(m_pOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), PropertyType::ErrorCorrectionLevelFromField, bDefaultValue);
				AddSubItem(pFieldErrCorrLevelProp);
				pFieldErrCorrLevelProp->SetName(_TB("Error Correction Level source"));
			}
			else
			{
				CRSBarCodeFieldComboProp* pFieldTypeProp = new CRSBarCodeFieldComboProp(m_pOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), m_propType, bDefaultValue);
				AddSubItem(pFieldTypeProp);
			}
		}
	}
	this->m_bGroup = FALSE;

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}


//================================CRSBarCodeTypeComboProp==================================
// Combobox per tipi barcode
//-----------------------------------------------------------------------------
CRSBarCodeTypeComboProp::CRSBarCodeTypeComboProp(CObject* pOwner, CBarCode* pBarCode)
	:
	CRSBarCodeProp(pOwner, _TB("Type"), pBarCode, NULL, _TB("Barcode types"))
{
	AllowEdit(FALSE);

	m_pBarCode = pBarCode;

	CString strTemp;
	CString strCurrentBarCodeType;

	// Caricamento della combo dei tipi di codice a barre supportati
	for (int nBCTypeIdx = 0; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		if (CBarCodeTypes::BarCodeEnum(CBarCodeTypes::s_bcTypes[nBCTypeIdx]) == E_BARCODE_TYPE_EXTENDED_CODE_93 
			|| 
			CBarCodeTypes::BarCodeEnum(CBarCodeTypes::s_bcTypes[nBCTypeIdx]) == E_BARCODE_TYPE_HIBC)
			continue;

		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		AddOption(strTemp, TRUE, nBCTypeIdx);
	}

	SetValue((_variant_t)CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[m_pBarCode->m_nBarCodeType]));
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeTypeComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();
	
	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;

	DWORD_PTR option = GetOptionData(index);//CBarCodeTypes::s_bcTypes[nBCTypeIdx]
	
	if(m_pBarCode->m_nBarCodeType != option)
	{ 
		m_pBarCode->m_nBarCodeTypeAlias = 0;
		m_pBarCode->m_nBarCodeType = option;

		((CRSBarCodeGroupProp*)GetParent()->GetParent())->UpdateDependantProp((CRSBarCodeProp*)GetParent());

		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw();
	}
	return baseUpdate;
}


//================================CRSBarCodeEncodingComboProp==================================
// Combobox per Encoding Mode Bracode 2D e Checksum Module per EAN128
//-----------------------------------------------------------------------------
CRSBarCodeEncodingComboProp::CRSBarCodeEncodingComboProp(CObject* pOwner, CBarCode* pBarCode, BOOL bDefaultValue)
	:
	CrsProp(pBarCode->Is2DBarcode() ? _TB("Encoding Mode") : _TB("Checksum Digit Module"), L"",
		pBarCode->Is2DBarcode() ? _TB("Barcode Encoding Modes") : _TB("Checksum Digit Module")),
	m_pOwner(pOwner)
{
	AllowEdit(FALSE);

	m_pBarCode = pBarCode;

	if (pBarCode->Is2DBarcode())
	{
		AddOption(_TB("Administration Console Default"), TRUE, -2);
		AddOption(_T("Undefined"), TRUE, -1);
		int bcType = pBarCode->m_nBarCodeType != 0 ? pBarCode->m_nBarCodeType : pBarCode->m_nBCDefaultType;
		if (bcType == BC_DATAMATRIX)
		{
			AddOption(sz2DBCDataMatrixEncodingMode_ASCII, TRUE, 0);
			AddOption(sz2DBCDataMatrixEncodingMode_C40, TRUE, 1);
			AddOption(sz2DBCDataMatrixEncodingMode_Text, TRUE, 2);
			AddOption(sz2DBCDataMatrixEncodingMode_X12, TRUE, 3);
			AddOption(sz2DBCDataMatrixEncodingMode_EDIFACT, TRUE, 4);
			AddOption(sz2DBCDataMatrixEncodingMode_Base256, TRUE, 5);
			SetDescription(_TB("DataMatrix Encoding mode"));
		}
		else if (bcType == BC_QR || bcType == BC_MicroQR)
		{
			AddOption(sz2DBCQREncodingMode_Numeric, TRUE, 0);
			AddOption(sz2DBCQREncodingMode_Alphanumeric, TRUE, 1);
			AddOption(sz2DBCQREncodingMode_Byte, TRUE, 2);
			AddOption(sz2DBCQREncodingMode_Kanji, TRUE, 3);
			SetDescription((bcType == BC_MicroQR ? _T("Micro ") : _T("") ) + _TB("QR-Code Encoding mode"));

		}
		else if (bcType == BC_PDF417)
		{
			AddOption(sz2DBCPDF417EncodingMode_Text, TRUE, 0);
			AddOption(sz2DBCPDF417EncodingMode_Byte, TRUE, 1);
			AddOption(sz2DBCPDF417EncodingMode_Numeric, TRUE, 2);
			SetDescription(_TB("PDF417 Encoding mode"));

		}
		if (!bDefaultValue && m_pBarCode->m_nCheckSumType < GetOptionCount() - 2 && m_pBarCode->m_nCheckSumType >= -2)
			SetValue((_variant_t)GetOption(m_pBarCode->m_nCheckSumType + 2));
		else
		{
			if (GetOptionCount() > 0)
				SetValue((_variant_t)GetOption(0));
			m_pBarCode->m_nCheckSumType = -2;//default value
		}
	}
	else
	{
		AddOption(_T(""), TRUE, -1);
		AddOption(szChecksumModule10103_Default, TRUE, 0);
		AddOption(szChecksumModule103_Optional, TRUE, 1);

		if(!bDefaultValue && m_pBarCode->m_nCheckSumType < GetOptionCount() - 1  && m_pBarCode->m_nCheckSumType >= -1)
			SetValue((_variant_t)GetOption(m_pBarCode->m_nCheckSumType + 1));
		else
		{
			if (GetOptionCount() > 0)
				SetValue((_variant_t)GetOption(0));
			m_pBarCode->m_nCheckSumType = -1;//default value
		}
	}	
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeEncodingComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;
	DWORD_PTR option = GetOptionData(index);
	if(m_pBarCode->m_nCheckSumType != option)
	{ 
		m_pBarCode->m_sCheckEncodeFieldName = _T("");
		m_pBarCode->m_nCheckSumType = option;

		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw();
	}

	return baseUpdate;
}


//================================CRSBarCodeVersionComboProp==================================
// Combobox per Version per Barcode 2D
//-----------------------------------------------------------------------------
CRSBarCodeVersionComboProp::CRSBarCodeVersionComboProp(CObject* pOwner, CBarCode* pBarCode, BOOL bDefaultValue)
	:
	CrsProp( _T(""), L"", _T("")),
	m_pOwner(pOwner)
{
	AllowEdit(FALSE);

	m_pBarCode = pBarCode;

	int nBarcodeType = pBarCode->m_nBarCodeType == 0 ? pBarCode->m_nBCDefaultType : pBarCode->m_nBarCodeType;

	SetName(nBarcodeType == BC_PDF417 ? _TB("Error Correction Level") : _TB("Version"));
	SetDescription(nBarcodeType == BC_PDF417 ? _TB("Error Correction Level") : _TB("Barcode 2D versions"));

	AddOption(_TB("Administration Console Default"), TRUE, -1);
	if (nBarcodeType == BC_DATAMATRIX)
	{	
		for (int nBCVersionIdx = 0; nBCVersionIdx < CBarCodeTypes::BARCODE_DM_VERSIONS_NUM; nBCVersionIdx++)
		{
			CString strTemp = CBarCodeTypes::BarCodeDMVersionDescription(nBCVersionIdx);
			AddOption(strTemp, TRUE, nBCVersionIdx);
		}
		SetDescription(_TB("The version of the DataMatrix barcode"));
	}
	else if (nBarcodeType == BC_QR)
	{
		for(int i = 0; i <= 40; i++)
			AddOption( DataInt(i).ToString() , TRUE, i);
		SetDescription(_TB("The version of the QrCode. Specifies the overall dimensions of the symbol. Use 0 for the minimum version required to encode all data"));
	}
	else if (nBarcodeType == BC_MicroQR)
	{
		for (int i = 0; i <= 4; i++)
			AddOption(DataInt(i).ToString(), TRUE, i);
		SetDescription(_TB("The version of the Micro QrCode. Specifies the overall dimensions of the symbol. Use 0 for the minimum version required to encode all data"));
	}
	
	if (!bDefaultValue)
	{ 
		int max = GetOptionCount() - 1;
		int currSelection = m_pBarCode->m_n2DVersion + 1;

		if (m_pBarCode->m_n2DVersion < max &&  currSelection >= 0)
		{ 
			SetValue((_variant_t)GetOption(currSelection));
			return;
		}
	}

	if (GetOptionCount() > 0)
		SetValue((_variant_t)GetOption(0));
	m_pBarCode->m_n2DVersion = -1; // default value
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeVersionComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;
	DWORD_PTR option = GetOptionData(index);
	if(m_pBarCode->m_n2DVersion != option)
	{
		m_pBarCode->m_s2DVersionFieldName = _T("");
		m_pBarCode->m_n2DVersion = option; 
	

		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw();
	}

	return baseUpdate;
}


//================================CRSBarCodeEncodingComboProp==================================
// Combobox per Error Correction Level per Barcode 2D
//-----------------------------------------------------------------------------
CRSBarCodeErrCorrLevelComboProp::CRSBarCodeErrCorrLevelComboProp(CObject* pOwner, CBarCode* pBarCode, BOOL bDefaultValue)
	:
	CrsProp(_TB("Error Correction Level"), L"", _TB("Error Correction Level")),
	m_pOwner(pOwner)
{
	AllowEdit(FALSE);

	m_pBarCode = pBarCode;
	int bcType = pBarCode->m_nBarCodeType != 0 ? pBarCode->m_nBarCodeType : pBarCode->m_nBCDefaultType;

	switch (bcType)
	{
		case BC_QR:
		{
			AddOption(_T("Administration Console Default"), TRUE, -1);
			AddOption(_T("L"), TRUE, 0);
			AddOption(_T("M"), TRUE, 1);
			AddOption(_T("Q"), TRUE, 2);
			AddOption(_T("H"), TRUE, 3);
			SetDescription(_TB("QR-Code error correction: L = Level Low � up to 7% damage can be restored; M = Level Medium � up to 15% damage can be restored , Q = Level Quartile � up to 25% damage can be restored; H = Level High � up to 30% damage can be restored. The higher the error correction level, the less storage capacity"));
			break;
		}
		case BC_MicroQR:
		{
			AddOption(_T("Administration Console Default"), TRUE, -1);
			AddOption(_T("L"), TRUE, 0);
			AddOption(_T("M"), TRUE, 1);
			AddOption(_T("Q"), TRUE, 2);
			SetDescription(_TB("Micro QR-Code error correction: L = Level Low � up to 7% damage can be restored; M = Level Medium � up to 15% damage can be restored , Q = Level Quartile � up to 25% damage can be restored. The higher the error correction level, the less storage capacity"));
			break;
		}
		case BC_PDF417:
		{
			AddOption(_T("Administration Console Default"), TRUE, -2);
			AddOption(_T("Auto"), TRUE, -1);
			for (int i = 0; i <= 8; i++)
				AddOption(DataInt(i).ToString(), TRUE, i);
			SetDescription(_TB("PDF417 error correction level (Reed Solomon). The higher the error correction level, the less storage capacity"));
			break;
		}
		default:
		{
			SetVisible(FALSE);
		}
	}

	if (!bDefaultValue)
	{
		int max = GetOptionCount() - (bcType == BC_PDF417 ? 2 : 1);
		int currSelection = m_pBarCode->m_nErrCorrLevel + (bcType == BC_PDF417 ? 2 : 1);

		if(m_pBarCode->m_nErrCorrLevel < max &&  currSelection >= 0)
			SetValue((_variant_t)GetOption(currSelection));
	}
	else
	{
		if (GetOptionCount() > 0)
			SetValue((_variant_t)GetOption(0));
		m_pBarCode->m_nErrCorrLevel = bcType == BC_PDF417 ? -2 : -1; //default value
	}
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeErrCorrLevelComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;
	DWORD_PTR option = GetOptionData(index);
	if(m_pBarCode->m_nErrCorrLevel != option)
	{
		m_pBarCode->m_sErrCorrLevelFieldName = _T("");
		m_pBarCode->m_nErrCorrLevel = option;

		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw(); 
	}

	return baseUpdate;
}


//================================CRSBarCodeFieldComboProp==================================
//-----------------------------------------------------------------------------
CRSBarCodeFieldComboProp::CRSBarCodeFieldComboProp(CObject* pOwner, CBarCode* pBarCode, SymTable* pSymTable, CRSBarCodeProp::PropertyType propType, BOOL bDefaultValue)
	:
	CrsProp(_T("title"), L"", _T("description")),
	m_pSymTable(pSymTable),
	m_pOwner(pOwner),
	m_pBarCode(pBarCode),
	m_propType(propType)
{
	AllowEdit(FALSE);

	if (!m_pSymTable)
		return;
	
	//riga vuota
	AddOption(L"", TRUE, 0);

	for (int i = 0; i <= m_pSymTable->GetUpperBound(); i++)
	{
		SymField* pField = m_pSymTable->GetAt(i);

		if (!pField)
			continue;

		if (
			pField->GetDataType() != DataType::String &&
			pField->GetDataType() != DataType::Integer &&
			pField->GetDataType().m_wTag != E_BARCODE_TYPE 	
			)
			continue;

		if (pField->GetDataType().m_wTag == E_BARCODE_TYPE && m_propType != CRSBarCodeProp::PropertyType::TypeFromField
			|| pField->GetDataType() != DataType::Integer && m_propType == CRSBarCodeProp::PropertyType::EncodingTypeFromField
			|| pField->GetDataType() != DataType::String && (m_propType == CRSBarCodeProp::PropertyType::VersionFromField || m_propType == CRSBarCodeProp::PropertyType::TextFromField))
			continue;

		CString sName = pField->GetName();
		int nAlias = pField->GetId();
		int idx = AddOption(sName,TRUE, nAlias);

	}

	SetName(_TB("Field"));

	switch (m_propType)
	{
		case CRSBarCodeProp::PropertyType::TypeFromField :
		{
			if (!bDefaultValue && m_pBarCode->m_nBarCodeTypeAlias > 0)
				SetValue((_variant_t)m_pSymTable->GetFieldByID(m_pBarCode->m_nBarCodeTypeAlias)->GetName());
			else
				SetValue(L"");
			SetDescription(_TB("Select field of string, integer or enumerative type that is source of Barcode Type."));

			break;
		}

		case CRSBarCodeProp::PropertyType::TextFromField :
		{
			if (!bDefaultValue && m_pBarCode->m_nHumanTextAlias > 0 && m_pSymTable->GetFieldByID(m_pBarCode->m_nHumanTextAlias))
				SetValue((_variant_t)m_pSymTable->GetFieldByID(m_pBarCode->m_nHumanTextAlias)->GetName());		
			else
				SetValue(L"");

			SetName(_TB("Text evaluated from field"));
			SetDescription(_TB("Select field of string type that is source of text."));

			break;
		}

		case CRSBarCodeProp::PropertyType::EncodingTypeFromField:
		{

			if (!bDefaultValue && !m_pBarCode->m_sCheckEncodeFieldName.IsEmpty())
				SetValue((_variant_t)m_pBarCode->m_sCheckEncodeFieldName);
			else
				SetValue(L"");

			SetDescription(m_pBarCode->Is2DBarcode() ? _TB("Select field of integer type that is source of Encoding Mode.") : _TB("Select field of integer type that is source of Checksum Digit Module."));
			break;
		}
		case CRSBarCodeProp::PropertyType::VersionFromField:
		{
			if (!bDefaultValue && !m_pBarCode->m_s2DVersionFieldName.IsEmpty())
				SetValue((_variant_t)m_pBarCode->m_s2DVersionFieldName);
			else
				SetValue(L"");

			SetDescription(_TB("Select field of string type that is source of Barcode Version."));

			break;
		}
		case CRSBarCodeProp::PropertyType::ErrorCorrectionLevelFromField:
		{
			if (!bDefaultValue && !m_pBarCode->m_sErrCorrLevelFieldName.IsEmpty())
				SetValue((_variant_t)m_pBarCode->m_sErrCorrLevelFieldName);
			else
				SetValue(L"");

			SetDescription(_TB("Select field of string or integer type that is source of Error Correction Level."));
			break;
		}
	}
}


//-----------------------------------------------------------------------------
BOOL CRSBarCodeFieldComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();
	
	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;
	DWORD_PTR option = GetOptionData(index);//CBarCodeTypes::s_bcTypes[nBCTypeIdx]
	BOOL valueChanged = FALSE;
	switch (m_propType)
	{
		case  CRSBarCodeProp::PropertyType::TypeFromField:
		{
			if(m_pBarCode->m_nBarCodeTypeAlias != option)
			{
				m_pBarCode->m_nBarCodeTypeAlias = option;
				valueChanged = TRUE;
			}		
			break;
		}

		case CRSBarCodeProp::PropertyType::TextFromField:
		{		
			if (m_pBarCode->m_nHumanTextAlias != option)
			{
				m_pBarCode->m_nHumanTextAlias = option;
				valueChanged = TRUE;
			}
			break;
		}
		case CRSBarCodeProp::PropertyType::EncodingTypeFromField:
		{
			if (m_pBarCode->m_sCheckEncodeFieldName != GetOption(index))
			{
				m_pBarCode->m_sCheckEncodeFieldName = GetOption(index);
				valueChanged = TRUE;
			}
			break;
		}
		case CRSBarCodeProp::PropertyType::VersionFromField:
		{			
			if (m_pBarCode->m_s2DVersionFieldName != GetOption(index))
			{
				m_pBarCode->m_s2DVersionFieldName = GetOption(index);
				valueChanged = TRUE;
			}
			break;
		}
		case CRSBarCodeProp::PropertyType::ErrorCorrectionLevelFromField:
		{

			if (m_pBarCode->m_sErrCorrLevelFieldName != GetOption(index))
			{
				m_pBarCode->m_sErrCorrLevelFieldName = GetOption(index);
				valueChanged = TRUE;
			}
			break;
		}
	}

	if(valueChanged)
	{ 
		//refresh layout object with the new value
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw();
	}

	return baseUpdate;
}


//================================CRSBarCodeShowTextProp==================================
//-----------------------------------------------------------------------------
CRSBarCodeShowTextProp::CRSBarCodeShowTextProp(CObject* pOwner, const CString& strName, BOOL* pValue, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description)
	:
	CRSBarCodeProp(pOwner, strName, pValue, pBarCode, propertyView, description),
	m_pBValue(pValue)
{
	ASSERT_VALID(m_pOwner);
	UpdatePropertyLayout(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeShowTextProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	BOOL value = this->GetValue();
	*m_pBValue = value == 0 ? FALSE : TRUE;

	m_pBarCode->m_bShowLabel = *m_pBValue;

	DrawProperties();

	if (m_pOwner)
	{
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
		{
			pGenObj->Redraw();
			return baseUpdate;
		}

		WoormIni* pWoormIni = dynamic_cast<WoormIni*>(m_pOwner);
		if (pWoormIni)
			pWoormIni->WriteWoormSettings();
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSBarCodeShowTextProp::DrawProperties(BOOL bDefaultValue)
{
	BOOL showtext;

	if (m_pWndList != NULL)
	{
		showtext = GetValue();
	}
	else
	{
		showtext = m_pBarCode->m_bShowLabel;
	}

	this->m_bGroup = TRUE;

	if (this->GetSubItemsCount() > 0)
		this->RemoveAllSubItems();

	if (IsVisible() && showtext)
	{
		//Text from field
		CRSBarCodeFieldComboProp* pTextfromFieldProp = new CRSBarCodeFieldComboProp(m_pObjOwner, m_pBarCode, m_pPropertyView->GetDocument()->GetSymTable(), CRSBarCodeProp::PropertyType::TextFromField,  bDefaultValue);
		AddSubItem(pTextfromFieldProp);
		pTextfromFieldProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
	}

	this->m_bGroup = FALSE;

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

//-----------------------------------------------------------------------------
void CRSBarCodeShowTextProp::UpdatePropertyLayout(BOOL defaultValue)
{
	BOOL typeFromField;

	CRSBarCodeGroupProp* bcPropGroup = (CRSBarCodeGroupProp*)GetParent();
	if (bcPropGroup == NULL)
	{
		typeFromField = m_pBarCode->m_nBarCodeTypeAlias > 0;
	}
	else
	{ 
		//il refresh � guidato da 'barcode type da campo/valore' 
		//barcode type da field
		CRSBarCodeComboProp* typeFrom = (CRSBarCodeComboProp*)bcPropGroup->m_lstDependentProp->GetHead();
		if (typeFrom == NULL) return;
		typeFromField = typeFrom->GetSelectedOption() == 1;
	}	

	BOOL b2DBarcode = m_pBarCode->Is2DBarcode();
	SetVisible(!b2DBarcode || typeFromField);
	if (defaultValue)
		SetValue(!b2DBarcode);

	//aggiorno la comboBox figlia
	DrawProperties(defaultValue);
}


//================================CRSBarCodeSizeProp==================================
//-----------------------------------------------------------------------------
CRSBarCodeSizeProp::CRSBarCodeSizeProp(CObject* pOwner, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, PropertySizeType eSizeType, BOOL bDefaultValue)
	: CRSBarCodeProp(pOwner, _T(""), pBarCode, propertyView, _T("")),
	m_eSizeType(eSizeType)
{
	AllowEdit(FALSE);
	UpdatePropertyLayout(bDefaultValue);
}

//-----------------------------------------------------------------------------
BOOL CRSBarCodeSizeProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();
	if (index < 0)
		return baseUpdate;
	DWORD_PTR option = GetOptionData(index);
	switch (m_eSizeType)
	{
		case PropertySizeType::BarHeight:
		{
			m_pBarCode->m_nCustomBarHeight = option;
			break;
		}
		case PropertySizeType::BarWidth_ModuleSize:
		{
			m_pBarCode->m_nNarrowBar = option;
			break;
		}
		case PropertySizeType::RowsNo:
		{
			m_pBarCode->m_nRowsNo = option;
			break;
		}
		case PropertySizeType::ColumnsNo:
		{
			m_pBarCode->m_nColumnsNo = option;
			break;
		}
	}
		
	//refresh layout object with the new value
	GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
	if (pGenObj)
		pGenObj->Redraw();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSBarCodeSizeProp::UpdatePropertyLayout(BOOL defaultValue)
{
	BOOL bTypefromField;
	CRSBarCodeGroupProp* bcPropGroup = (CRSBarCodeGroupProp*)GetParent();
	if (bcPropGroup == NULL)
	{
		//sono in creazione
		bTypefromField = (m_pBarCode->m_nBarCodeTypeAlias > 0);
	}
	else
	{ 
		//il refresh � guidato da 'barcode type da campo/valore' 
		//barcode type da field
		CRSBarCodeComboProp* typeFrom = (CRSBarCodeComboProp*)bcPropGroup->m_lstDependentProp->GetHead();
		if (typeFrom == NULL) return;

		bTypefromField = typeFrom->GetSelectedOption() == 1;
	}

	AddOption(_T("Auto"), TRUE, -1);
	BOOL b2DBarcode = m_pBarCode->Is2DBarcode();
	int bcType = m_pBarCode->m_nBarCodeType == CBarCodeTypes::BC_DEFAULT ? m_pBarCode->m_nBCDefaultType : m_pBarCode->m_nBarCodeType;

	switch (m_eSizeType)
	{
		case PropertySizeType::BarHeight:
		{
			SetVisible(!b2DBarcode || bcType == BC_PDF417 || bTypefromField);
			SetName(bTypefromField ? _TB("Bars / Rows Height") : bcType == BC_PDF417 ? _TB("Rows Height") : _TB("Bars Height"));
			SetDescription(bTypefromField ? _TB("The height of each bar/row, in pixel. A value superior or equal to 10 is recommended") : bcType == BC_PDF417 ? _TB("The height of each row, in pixel. A value superior or equal to 10 is recommended") : _TB("The height of each bar, in pixel. A value superior or equal to 10 is recommended"));
			for (int i = 1; i <= 50; i++)
				AddOption(DataInt(i).ToString(), TRUE, i);
			if (!defaultValue)
				SelectOption(GetOptionDataIndex(m_pBarCode->m_nCustomBarHeight), FALSE);
			else m_pBarCode->m_nCustomBarHeight = -1; //default value

			m_pPropertyView->m_pPropGrid->AdjustLayout();

			break;

		}
		case PropertySizeType::BarWidth_ModuleSize:
		{
			if (bTypefromField || b2DBarcode)
			{
				SetName(bTypefromField ? _TB("Bars Thickness / Module Size") : _TB("Module Size"));
				SetDescription(bTypefromField ? _TB("The thickness of a single bar or the size of each module, in pixel") : _TB("The size of each module, in pixel. A value superior or equal to 4 is recommended"));
				for (int i = 1; i <= 20; i++)
					AddOption(DataInt(i).ToString(), TRUE, i);
			}
			else
			{
				SetName(_TB("Bars Thickness"));
				SetDescription( _TB("The thickness of a single bar, in pixel"));
				for (int i = 1; i <= 10; i++)
					AddOption(DataInt(i).ToString(), TRUE, i);
			}
			if (!defaultValue)
				SelectOption(GetOptionDataIndex(m_pBarCode->m_nNarrowBar), FALSE);
			else
				m_pBarCode->m_nNarrowBar = -1; //default value
			break;
		}
		case PropertySizeType::RowsNo:
		{
			SetName( _TB("Rows Number") );
			SetDescription(_TB("The number of rows constituting the barcode. 'Auto' is recommended."));
			for (int i = 3; i <= 90; i++)
				AddOption(DataInt(i).ToString(), TRUE, i);
			if (!defaultValue)
				SelectOption(GetOptionDataIndex(m_pBarCode->m_nRowsNo), FALSE);
			else
				m_pBarCode->m_nRowsNo = -1;//default value

			break;
		}
		case PropertySizeType::ColumnsNo:
		{
			SetName(_TB("Columns Number"));
			SetDescription(_TB("The number of columns constituting the barcode. 'Auto' is recommended."));

			for (int i = 3; i <= 30; i++)
				AddOption(DataInt(i).ToString(), TRUE, i);
			if (!defaultValue)
				SelectOption(GetOptionDataIndex(m_pBarCode->m_nColumnsNo), FALSE);
			else
				m_pBarCode->m_nColumnsNo = -1;//default value
			break;
		}
	}

	if (defaultValue)
		SelectOption(0, FALSE);
}

//================================CRSChartProp==================================
//-----------------------------------------------------------------------------
CRSChartProp::CRSChartProp(CObject* pOwner, Chart* pChart)
	:
	CrsProp(L"", L"", L""),
	m_pOwner(pOwner),
	m_pChart(pChart)
{
}

//-----------------------------------------------------------------------------
BOOL CRSChartProp::OnUpdateValue() 
{ 	
	if (!__super::OnUpdateValue())
		return FALSE;

	m_pChart->SyncChart(); 
	m_pChart->Redraw();	
	return TRUE;
}

//================================CRSChartStringProp==================================
//-----------------------------------------------------------------------------
CRSChartStringProp::CRSChartStringProp(CObject* pOwnerObj, const CString& strName, CString* pValue, const CString& description, EnumChartObject eChartObjType)
	:
	CRSStringProp(pOwnerObj, strName, pValue, description),
	m_eCharObjType(eChartObjType)
{}

//-----------------------------------------------------------------------------
BOOL CRSChartStringProp::OnUpdateValue()
{
	CString oldValue = this->GetValue();
	BOOL base = __super::OnUpdateValue();

	CString value = this->GetValue();

	if (oldValue.CompareNoCase(value) == 0)
		return TRUE;

	Chart* pChart;

	switch(m_eCharObjType)		
	{
		case EnumChartObject::CHART:
		{
			pChart = (Chart*)m_pOwner;
			break;
		}
		case EnumChartObject::CATEGORY:
		{
			pChart = ((Chart::CCategories*)m_pOwner)->GetParent();
			break;
		}
		case EnumChartObject::SERIES:
		{
			pChart = ((Chart::CSeries*)m_pOwner)->GetParent();
			break;
		}
		case EnumChartObject::TRASPARENCY:
		{
			Chart::CSeries* pSeries = (Chart::CSeries*)m_pOwner;
			pChart = pSeries->GetParent();
			
			double dTrasparency = _wtof(value);
			if (dTrasparency < 0 || dTrasparency > 1)
				SetValue((_variant_t)oldValue);
			else
				pSeries->m_dTrasparency = dTrasparency;
			break;
		}
	}

	if (pChart)
	{
		pChart->SyncChart();
		pChart->Redraw();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)m_pOwner, TRUE);
	}
	return base;
}


//================================CRSChartDoubleProp==================================
//-----------------------------------------------------------------------------
CRSChartDoubleProp::CRSChartDoubleProp(CObject* pOwnerObj, const CString& strName, double* value, const CString& description, EnumChartObject eChartObjectType, int fromValue/* = 0*/, int toValue/* = 1000*/)
	:
	CRSDoubleProp(pOwnerObj, strName, value, description, fromValue, toValue),
	m_eCharObjType(eChartObjectType)
{}

//-----------------------------------------------------------------------------
BOOL CRSChartDoubleProp::OnUpdateValue()
{
	BOOL base = __super::OnUpdateValue();
	Chart* pChart;

	switch (m_eCharObjType)
	{
	case EnumChartObject::CHART:
	{
		pChart = (Chart*)m_pOwner;
		break;
	}
	case EnumChartObject::CATEGORY:
	{
		pChart = ((Chart::CCategories*)m_pOwner)->GetParent();
		break;
	}
	case EnumChartObject::SERIES:
	{
		pChart = ((Chart::CSeries*)m_pOwner)->GetParent();
		break;
	}
	}

	if (pChart)
	{
		pChart->SyncChart();
		pChart->Redraw();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)m_pOwner, TRUE);
	}
	return base;
}


//================================CRSChartColorProp==================================
//-----------------------------------------------------------------------------
CRSChartColorProp::CRSChartColorProp(CObject* pOwnerObj, CRS_PropertyGrid* pPropertyGrid, const CString& strName, COLORREF* pValue, const CString& description, EnumChartObject eChartObjType)
	:
	CRSColorProp(pOwnerObj, strName, pValue, description),
	m_eCharObjType(eChartObjType),
	m_pPropertyGrid(pPropertyGrid)
{}

//-----------------------------------------------------------------------------
CWoormDocMng*	CRSChartColorProp::GetDocument()
{
	return m_pPropertyGrid->GetPropertyView()->GetDocument();
}

//-----------------------------------------------------------------------------
BOOL CRSChartColorProp::OnUpdateValue()
{
	BOOL base = __super::OnUpdateValue();
	Chart* pChart;

	switch (m_eCharObjType)
	{
	case EnumChartObject::CHART:
	{
		pChart = (Chart*)m_pOwner;
		break;
	}
	case EnumChartObject::CATEGORY:
	{
		pChart = ((Chart::CCategories*)m_pOwner)->GetParent();
		break;
	}
	case EnumChartObject::SERIES:
	{
		pChart = ((Chart::CSeries*)m_pOwner)->GetParent();
		break;
	}
	}

	if (pChart)
	{
		pChart->SyncChart();
		pChart->Redraw();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)m_pOwner, TRUE);
	}
	return base;
}



//================================CRSChartBoolProp==================================
//-----------------------------------------------------------------------------
CRSChartBoolProp::CRSChartBoolProp(CObject* pOwnerObj, const CString& strName, BOOL* value, const CString& description, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, EnumChartObject eChartObjectType)
	:
	CRSBoolProp(pOwnerObj, strName, value, description),
	m_eCharObjType(eChartObjectType),
	m_pSymTable(pSymTable),
	m_pPropertyView(propertyView)
{
	DrawProperties();
}

//-----------------------------------------------------------------------------
BOOL CRSChartBoolProp::OnUpdateValue()
{
	BOOL base = __super::OnUpdateValue();
	Chart* pChart;

	switch (m_eCharObjType)
	{
		case EnumChartObject::CHART:
		case EnumChartObject::LEGEND:
		{
			pChart = (Chart*)m_pOwner;
			break;
		}
		case EnumChartObject::CATEGORY:
		{
			pChart = ((Chart::CCategories*)m_pOwner)->GetParent();
			break;
		}
		case EnumChartObject::SERIES:
		case EnumChartObject::LABEL:
		{
			pChart = ((Chart::CSeries*)m_pOwner)->GetParent();
			break;
		}
	}

	if (pChart && pChart->IsValidChart())
	{
		pChart->SyncChart();
		pChart->Redraw();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)m_pOwner, TRUE);
		DrawProperties();
	}
	
	return base;
}

//------------------------------------------------------------------------------------
void CRSChartBoolProp::DrawProperties()
{
	if (GetSubItemsCount() > 0)
		RemoveAllSubItems();

	if ((BOOL)GetValue())
	{

		if (m_eCharObjType == EnumChartObject::SERIES)
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CSeries)))
			{
				Chart::CSeries* pOwnerSeries = (Chart::CSeries*)m_pOwner;
				if (pOwnerSeries)
				{
					if (Chart::HasSeriesMultiColor(pOwnerSeries->GetSeriesType()))
					{
						CRSChartFieldComboProp* pComboFieldColorProp = new CRSChartFieldComboProp(pOwnerSeries, pOwnerSeries->GetParent(), m_pPropertyView, m_pSymTable, EnumChartObject::COLOR);
						AddSubItem(pComboFieldColorProp, m_pPropertyView->GetPropertyGrid());
					}
					else
					{
						CRSChartColorProp* bkgColorProp = new CRSChartColorProp(pOwnerSeries, m_pPropertyView->GetPropertyGrid(), _TB("Custom Color"), &pOwnerSeries->m_rgbColor,
							_TB("The background color of chart"), EnumChartObject::SERIES);
						AddSubItem(bkgColorProp, m_pPropertyView->GetPropertyGrid());
					}
				}
			}
		}
		else if (m_eCharObjType == EnumChartObject::CHART)
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart)))
			{
				Chart* pOwnerChart = (Chart*)m_pOwner;
				if (pOwnerChart)
				{
					CRSChartColorProp* bkgColorProp = new CRSChartColorProp(pOwnerChart, m_pPropertyView->GetPropertyGrid(), _TB("Custom Color"), &pOwnerChart->m_rgbColor,
						_TB("The background color of chart"), EnumChartObject::CHART);
					AddSubItem(bkgColorProp, m_pPropertyView->GetPropertyGrid());
				}
			}
		}
		else if (m_eCharObjType == EnumChartObject::LEGEND)
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart)))
			{
				Chart* pOwnerChart = (Chart*)m_pOwner;
				if (pOwnerChart)
				{
					//hidden legend
					AddSubItem(new CRSChartLegendPosComboProp(pOwnerChart->m_pLegend), m_pPropertyView->GetPropertyGrid());
				}
			}
		}
		else return;
	}
	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

//================================CRSChartTypeComboProp==================================
//-----------------------------------------------------------------------------
CRSChartTypeComboProp::CRSChartTypeComboProp(Chart* pChart, CRS_ObjectPropertyView* propertyView, CrsProp* pDSProp)
	:
	CRSChartProp(pChart, pChart),
	m_pPropertyView(propertyView),
	m_pDSProp(pDSProp),
	m_pColorChartProp(NULL)
{
	m_strName = _TB("Type");
	m_strDescr = _TB("Chart type");
	AllowEdit(FALSE);

	// Caricamento della combo dei tipi di grafico supportati
	for (int nChartTypeIdx = 0; nChartTypeIdx < EnumChartType::Chart_Wrong; nChartTypeIdx++)
	{
		if (m_pChart->m_eChartType != EnumChartType::Chart_None)
		{
			//non permetto di deassegnare il tipo ad un chart gi� valorizzato
			if (nChartTypeIdx == 0)
				continue;

			//non permetto di cambiare 'famiglia' ad un chart se contiene delle serie			
			if (m_pChart->GetSeries()->GetSize() > 0 && nChartTypeIdx > 0 && 
				!m_pChart->IsCompatibleChartType((EnumChartType)nChartTypeIdx))
				continue;
		}
		CString strTemp = Chart::ChartTypeDescription((EnumChartType)nChartTypeIdx);
		AddOption(strTemp, TRUE, nChartTypeIdx);
	}

	SetValue((_variant_t)Chart::ChartTypeDescription(pChart->m_eChartType));
}

//-----------------------------------------------------------------------------
CRSChartTypeComboProp::CRSChartTypeComboProp(Chart::CSeries* pSeries, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, CrsProp* pDSProp)
	:
	CRSChartProp(pSeries->GetParent(), pSeries->GetParent()),
	m_bIsSeries(TRUE),
	m_pSeries(pSeries),
	m_pPropertyView(propertyView),
	m_pSymTable(pSymTable),
	m_pDSProp(pDSProp),
	m_pColorChartProp(NULL)
{
	m_strName = _TB("Type");
	m_strDescr = _TB("Type of series");
	AllowEdit(FALSE);
 
	// Caricamento della combo dei tipi di grafico supportati
	for (int nChartTypeIdx = 0; nChartTypeIdx < EnumChartType::Chart_Wrong; nChartTypeIdx++)
	{
		if (!m_pChart->IsCompatibleChartType((EnumChartType)nChartTypeIdx))
			continue;
		
		CString strTemp = Chart::ChartTypeDescription((EnumChartType)nChartTypeIdx);
		AddOption(strTemp, TRUE, nChartTypeIdx);
	}

	SetValue((_variant_t)Chart::ChartTypeDescription(m_pSeries->m_eSeriesType));

	DrawProperties();
}

//-----------------------------------------------------------------------------
BOOL CRSChartTypeComboProp::OnUpdateValue()
{
	CString oldValue = this->GetValue();
	BOOL baseUpdate = __super::OnUpdateValue();
	if (!baseUpdate)
		return FALSE;

	CString value = this->GetValue();

	if (oldValue.CompareNoCase(value) == 0)
		return TRUE;

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;
	
	DWORD_PTR option = GetOptionData(index);
	if (m_bIsSeries)
	{
		m_pSeries->m_eSeriesType = (EnumChartType)option;
		DrawProperties();
	}
	else
	{
		m_pChart->m_eChartType = (EnumChartType)option;
		if (Chart::HasCategory(m_pChart->m_eChartType) && !m_pChart->m_pCategory)
			m_pChart->m_pCategory = new Chart::CCategories(m_pChart);
		if (!m_pChart->AllowSeriesWithDiffType() &&  m_pChart->GetSeries())
		{
			for (int s = 0; s < m_pChart->GetSeries()->GetSize(); s++)
			{
				Chart::CSeries* pSeries = m_pChart->GetSeries()->GetSeriesAt(s);
				if (pSeries->m_eSeriesType != EnumChartType::Chart_None)
					pSeries->m_eSeriesType = EnumChartType::Chart_None;
			}
		}

		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)m_pChart, TRUE);
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, (BaseObj*)m_pChart);
	}
	
	return TRUE;	
}

//------------------------------------------------------------------------------------
void CRSChartTypeComboProp::DrawProperties()
{
	if (GetSubItemsCount() > 0)
		RemoveAllSubItems();
	if (m_bIsSeries)
	{
		if (Chart::HasGroups(m_pSeries->GetSeriesType()))
		{
			CRSIntProp* pGroupProp = new CRSIntProp(m_pChart, _TB("Group"), &(m_pSeries->m_nGroup), _TB("Stacked group"));
			AddSubItem(pGroupProp, m_pPropertyView->GetPropertyGrid());
		}
		if (Chart::AllowLineStyle(m_pSeries->GetSeriesType()))
		{
			CRSChartLineStyleComboProp* pStyleProp = new CRSChartLineStyleComboProp(m_pSeries);
			AddSubItem(pStyleProp, m_pPropertyView->GetPropertyGrid());
		}

		if (!m_pDSProp) return;
		if (m_pDSProp->GetSubItemsCount() > 0)
			m_pDSProp->RemoveAllSubItems();

		int nNumDataSources = Chart::GetDSNumber(m_pSeries->GetSeriesType());
		for (int index = 0; index < nNumDataSources; index++)
		{
			CRSChartFieldComboProp* pComboFieldProp = new CRSChartFieldComboProp(m_pSeries, m_pSeries->GetParent(), m_pPropertyView, m_pSymTable, EnumChartObject::SERIES, index);
			m_pDSProp->AddSubItem(pComboFieldProp, m_pPropertyView->GetPropertyGrid());
		}
		CRSChartBoolProp* pBColored = new CRSChartBoolProp(m_pSeries, _TB("Custom Color"), &m_pSeries->m_bColored, _TB("Enable custom color of series"), m_pPropertyView, m_pSymTable, EnumChartObject::SERIES);
		m_pDSProp->AddSubItem(pBColored, m_pPropertyView->GetPropertyGrid());
	}
	else
	{ 
		//aggiunge la propriet� colored all'intero chart
		/*if (!m_pDSProp) return;
		if (m_pColorChartProp && m_pDSProp->FindSubItemByID(m_pColorChartProp->GetID()))
		{			
			m_pDSProp->RemoveSubItem(m_pColorChartProp); 
			m_pColorChartProp = NULL;
		}
					
		CRSChartBoolProp* pBColored = new CRSChartBoolProp(m_pChart, _TB("Custom Color"), &m_pChart->m_bColored, _TB("Enable custom color of series"), m_pPropertyView, m_pSymTable, EnumChartObject::CHART);
		m_pDSProp->AddSubItem(pBColored, m_pPropertyView->GetPropertyGrid());
		m_pColorChartProp = pBColored;*/
	}

	m_pPropertyView->m_pPropGrid->AdjustLayout();
}

//================================CRSChartFieldComboProp==================================
//-----------------------------------------------------------------------------
CRSChartFieldComboProp::CRSChartFieldComboProp(CObject* pOwner, Chart* pChart, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, EnumChartObject eObjType /* = CATEGORY */, int nBindedFieldIndex /* = -1 */)
	:
	CRSChartProp(pOwner, pChart),
	m_pSymTable(pSymTable),
	m_nBindedFieldIndex(nBindedFieldIndex),
	m_pPropertyView(propertyView),
	m_eObjType(eObjType)
{
	m_strName = eObjType != EnumChartObject::COLOR ? _TB("Field for data") : _TB("Field for color");
	m_strDescr = eObjType != EnumChartObject::COLOR ?  _TB("Field (array or column) witch contains data to be drawn in the chart") : _TB("Field (array or column) witch contains color to be used for series");
	AllowEdit(FALSE);

	if (!m_pSymTable)
		return;

	//fill property values
	//riga vuota
	AddOption(L"", TRUE, 0);
	BOOL bStringOk = m_eObjType == EnumChartObject::CATEGORY;
	for (int i = 0; i <= m_pSymTable->GetUpperBound(); i++)
	{
		SymField* pField = m_pSymTable->GetAt(i);

		if (!pField)
			continue;

		WoormField* currWrmField = (WoormField*)pField;

		if (
			pField->GetDataType() != DataType::Array &&
			(!currWrmField || currWrmField->GetFieldType() != WoormField::RepFieldType::FIELD_COLUMN)
			)
			continue;

		if (currWrmField->GetFieldType() == WoormField::RepFieldType::FIELD_COLUMN)
		{
			const TableColumn* pCol = propertyView->GetDocument()->GetObjects().FindColumnByID(currWrmField->GetId());
			if (!pCol)
				continue;
		}	

		if (pField->GetDataType() == DataType::Array)
		{
			DataArray* pDA = dynamic_cast<DataArray*>(pField->GetData());
			if (bStringOk && pDA->GetBaseDataType() != DataType::String ||
				!bStringOk && !pDA->GetBaseDataType().IsNumeric())
				continue;
		}
		else if (bStringOk && pField->GetDataType() != DataType::String ||
			!bStringOk && !pField->GetDataType().IsNumeric())
			continue;

		CString sName = pField->GetName();
		int nAlias = pField->GetId();
		int idx = AddOption(sName, TRUE, nAlias);
	}

	//set property value 
	WoormField* wrmField = NULL;
	switch (m_eObjType)
	{
		case EnumChartObject::CATEGORY:
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CCategories)))
			{
				Chart::CCategories* pOwnerCat = (Chart::CCategories*)m_pOwner;
				if (pOwnerCat && pOwnerCat->m_pBindedField)
				{
					SetValue((_variant_t)pOwnerCat->m_pBindedField->GetName());
					wrmField = (WoormField*)pOwnerCat->m_pBindedField;
				}
				else
				{
					SetValue(L"");
					wrmField = NULL;
				}
			}
			break;
		}
		case EnumChartObject::SERIES:	
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CSeries)))
			{
				Chart::CSeries* pOwnerSeries = (Chart::CSeries*)m_pOwner;
				if (pOwnerSeries && m_nBindedFieldIndex > -1 && m_nBindedFieldIndex < pOwnerSeries->m_arBindedField.GetSize())
				{
					SetValue((_variant_t)pOwnerSeries->m_arBindedField[m_nBindedFieldIndex]->GetName());
					wrmField = (WoormField*)pOwnerSeries->m_arBindedField[m_nBindedFieldIndex];
				}
				else
				{
					SetValue(L"");
					wrmField = NULL;
				}
			}
			break;		
		}
		case EnumChartObject::COLOR:
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CSeries)))
			{
				Chart::CSeries* pOwnerSeries = (Chart::CSeries*)m_pOwner;
				if (pOwnerSeries && pOwnerSeries->m_pFieldColor)
				{
					SetValue((_variant_t)pOwnerSeries->m_pFieldColor->GetName());
					wrmField = (WoormField*)pOwnerSeries->m_pFieldColor;
				}
				else
				{
					SetValue(L"");
					wrmField = NULL;
				}
			}
			break;
		}
	}
	
	DrawProperties(wrmField);
}

//------------------------------------------------------------------------------------
BOOL CRSChartFieldComboProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;
	
	DWORD_PTR option = GetOptionData(index);
	BOOL valueChanged = FALSE;

	SymField* pSelectedField = m_pSymTable->GetFieldByID((WORD)option);

	switch (m_eObjType)
	{
		case EnumChartObject::CATEGORY:
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CCategories)))
			{ 
				Chart::CCategories* pOwnerCat = (Chart::CCategories*)m_pOwner;
				if (pSelectedField && !(pOwnerCat->m_pBindedField && pSelectedField->GetId() == pOwnerCat->m_pBindedField->GetId()))
				{
					pOwnerCat->m_pBindedField = pSelectedField;
					valueChanged = TRUE;
				}
				if (valueChanged && pOwnerCat->GetTitle().IsEmpty())
					GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)pOwnerCat, TRUE);
			}
			break;
		}
		case EnumChartObject::SERIES:
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CSeries)))
			{
				Chart::CSeries* pOwnerSeries = (Chart::CSeries*)m_pOwner;
				if (m_nBindedFieldIndex >= 0)
				{
					if (pSelectedField)
					{
						if (m_nBindedFieldIndex >= pOwnerSeries->m_arBindedField.GetSize() ||
							pSelectedField->GetId() != pOwnerSeries->m_arBindedField[m_nBindedFieldIndex]->GetId())
						{
							if (m_nBindedFieldIndex >= pOwnerSeries->m_arBindedField.GetSize())
								pOwnerSeries->m_arBindedField.Add(pSelectedField);
							else
								pOwnerSeries->m_arBindedField[m_nBindedFieldIndex] = pSelectedField;
							valueChanged = TRUE;
						}
					}
					else if (m_nBindedFieldIndex < pOwnerSeries->m_arBindedField.GetSize())
					{
						valueChanged = TRUE;
						pOwnerSeries->m_arBindedField.RemoveAt(m_nBindedFieldIndex);
					}
					if (valueChanged && pOwnerSeries->GetTitle().IsEmpty())
						GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, (BaseObj*)pOwnerSeries, TRUE);
				}
			}
			break;
		}
		case EnumChartObject::COLOR:
		{
			if (m_pOwner->IsKindOf(RUNTIME_CLASS(Chart::CSeries)))
			{
				Chart::CSeries* pOwnerSeries = (Chart::CSeries*)m_pOwner;
				if (pSelectedField && !(pOwnerSeries->m_pFieldColor && pSelectedField->GetId() == pOwnerSeries->m_pFieldColor->GetId()))
				{
					pOwnerSeries->m_pFieldColor = pSelectedField;
					pOwnerSeries->m_bColored = pSelectedField != NULL;
					valueChanged = TRUE;
				}
			}
			break;
		}
	}
		
	if (valueChanged)
		DrawProperties((WoormField*)pSelectedField);
	
		return  TRUE;
}

//------------------------------------------------------------------------------------
void CRSChartFieldComboProp::DrawProperties(WoormField* wrmField)
{
	if (this->GetSubItemsCount() > 0)
		this->RemoveAllSubItems();
	
	if (!wrmField)
	{
		m_pPropertyView->m_pPropGrid->AdjustLayout();
		return;
	}

	//Field type description
	CrsProp* typeProp = new CrsProp(_TB("Variable Type"), (_variant_t)wrmField->GetSourceDescription(), _TB("Variable source type"));
	typeProp->AllowEdit(FALSE);
	AddSubItem(typeProp, m_pPropertyView->m_pPropGrid);
	m_pPropertyView->m_pPropGrid->AdjustLayout();
	
}


//================================CRSChartLegendPosComboProp==================================
//-----------------------------------------------------------------------------
CRSChartLegendPosComboProp::CRSChartLegendPosComboProp(CObject* pOwner)
	:
	CRSChartProp(pOwner, ((Chart::CLegend*)pOwner ? ((Chart::CLegend*)pOwner)->m_pParent : NULL))
{
	m_strName = _TB("Legend position") ;
	m_strDescr = _TB("Where and if the legend is shown.") ;
	AllowEdit(FALSE);

	//fill property values
	//AddOption(L"Hidden", TRUE, BCGPChartLayout::LP_NONE);
	AddOption(L"Top", TRUE);
	AddOption(L"Bottom", TRUE);
	AddOption(L"Left", TRUE);
	AddOption(L"Right", TRUE);

	//set property value 	
	Chart::CLegend* pLegend = (Chart::CLegend*)m_pOwner;
	if (pLegend)
		SelectOption(pLegend->m_Align - 1, FALSE);
	}

//------------------------------------------------------------------------------------
BOOL CRSChartLegendPosComboProp::OnUpdateValue()
{
	Chart::CLegend* pLegend = (Chart::CLegend*)m_pOwner;
	if (!pLegend) return FALSE;

	BOOL baseUpdate = __super::OnUpdateValue();
	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	AlignType oldAlignType = pLegend->m_Align;
	AlignType newAlignType = index + 1;
	if (oldAlignType != newAlignType)
		pLegend->m_Align = newAlignType;

	return  TRUE;
}


//================================CRSChartLineStyleComboProp==================================
//-----------------------------------------------------------------------------
CRSChartLineStyleComboProp::CRSChartLineStyleComboProp(CObject* pOwner)
	:
	CRSChartProp(pOwner, ((Chart::CLegend*)pOwner ? ((Chart::CSeries*)pOwner)->m_pParent : NULL))
{
	m_strName = _TB("Line Style");
	m_strDescr = _TB("Style of line.");
	AllowEdit(FALSE);

	//fill property values
	AddOption(L"None", TRUE, EnumChartStyle::ChartStyle_None);
	AddOption(L"Line Smooth", TRUE, EnumChartStyle::ChartStyle_LineSmooth);
	AddOption(L"Line Step", TRUE, EnumChartStyle::ChartStyle_LineStep);

	//set property value 	
	Chart::CSeries* pSeries = (Chart::CSeries*)m_pOwner;
	if (pSeries)
		SelectOption(pSeries->m_eStyle, FALSE);
}

//------------------------------------------------------------------------------------
BOOL CRSChartLineStyleComboProp::OnUpdateValue()
{
	Chart::CSeries* pSeries = (Chart::CSeries*)m_pOwner;
	if (!pSeries) return FALSE;

	BOOL baseUpdate = __super::OnUpdateValue();
	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	EnumChartStyle oldStyle = pSeries->m_eStyle;
	EnumChartStyle newStyle = (EnumChartStyle)index;
	if (oldStyle != newStyle)
		pSeries->m_eStyle = newStyle;

	return  TRUE;
}

//================================CRSTableRowSeparatorProp==================================
//-----------------------------------------------------------------------------
CRSTableRowSeparatorProp::CRSTableRowSeparatorProp(Table* pTable)
	:
	CrsProp(_TB("Row Separator"), L"", _TB("Row Separator property")),
	m_pTable(pTable)
{
	ASSERT_VALID(pTable);

	AllowEdit(FALSE);

	AddOption(_TB("No"), TRUE, 0);
	AddOption(_TB("Simple"), TRUE, 1);
	AddOption(_TB("MultiLine"), TRUE, 2);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableRowSeparatorProp::UpdatePropertyValue()
{
	if (m_pTable->m_Borders.m_bRowSeparatorDynamic)
		SetValue(GetOption(2));
	else
		SetValue(GetOption(m_pTable->m_Borders.m_bRowSeparator));
}

//-----------------------------------------------------------------------------
BOOL CRSTableRowSeparatorProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index >= 0)
	{
		//dynamic - meaning  both True
		if (index == 2)
		{
			m_pTable->m_Borders.m_bRowSeparator = TRUE;
			m_pTable->m_Borders.m_bRowSeparatorDynamic = TRUE;
		}

		//not dynamic and m_bRowSeparator = FALSE if zero option selected, TRUE otherwise
		else
		{
			m_pTable->m_Borders.m_bRowSeparator = index;
			m_pTable->m_Borders.m_bRowSeparatorDynamic = FALSE;
		}
	}

	m_pTable->Redraw();

	return baseUpdate;
}

//================================CRSAlternateColorProp==================================
//-----------------------------------------------------------------------------
CRSAlternateColorProp::CRSAlternateColorProp(Table* pTable)
	:
	CrsProp(_TB("Easy reading"), L"", _TB("Easy reading property")),
	m_pTable(pTable)
{
	ASSERT_VALID(pTable);

	AllowEdit(FALSE);

	AddOption(_TB("No"), TRUE, 0);
	AddOption(_TB("Simple"), TRUE, 1);
	AddOption(_TB("MultiLine"), TRUE, 2);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSAlternateColorProp::UpdatePropertyValue()
{
	if (m_pTable->m_bAlternateBkgColorOnMultiLineRow)
		SetValue(GetOption(2));
	else
		SetValue(GetOption(m_pTable->m_bAlternateBkgColorOnRow));
}

//-----------------------------------------------------------------------------
BOOL CRSAlternateColorProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index >= 0)
	{
		//dynamic - meaning  both True
		if (index == 2)
		{
			m_pTable->m_bAlternateBkgColorOnRow = TRUE;
			m_pTable->m_bAlternateBkgColorOnMultiLineRow = TRUE;
		}

		//not dynamic and m_bRowSeparator = FALSE if zero option selected, TRUE otherwise
		else
		{
			m_pTable->m_bAlternateBkgColorOnRow = index;
			m_pTable->m_bAlternateBkgColorOnMultiLineRow = FALSE;
		}
	}
	
	//ridisegno tutto
	m_pTable->Redraw(TRUE);

	return baseUpdate;
}

//================================CrsGridStyleProp==================================
//-----------------------------------------------------------------------------
CrsGridStyleProp::CrsGridStyleProp(WoormIni* pWrmIni)
	:
	CrsProp(_TB("Grid style"), L"", _TB("Background grid style property")),
	m_pWrmIni(pWrmIni)
{
	ASSERT_VALID(pWrmIni);

	AllowEdit(FALSE);

	AddOption(_TB("Points")	, TRUE, 0);
	AddOption(_TB("Lines")	, TRUE, 1);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CrsGridStyleProp::UpdatePropertyValue()
{
	if (m_pWrmIni->m_bLineGrid)
		SetValue(GetOption(1));
	else
		SetValue(GetOption(0));
}

//-----------------------------------------------------------------------------
BOOL CrsGridStyleProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index >= 0)
	{
		m_pWrmIni->m_bLineGrid = (index ? TRUE : FALSE);
	}

	//salvo
	m_pWrmIni->WriteWoormSettings();
	//ridisegno tutto
	GetPropertyView()->GetDocument()->Invalidate(TRUE);

	return baseUpdate;
}

//================================CRSSplitterColumnProp==================================
//-----------------------------------------------------------------------------
CRSSplitterColumnProp::CRSSplitterColumnProp(TableColumn* pCol)
	:
	CrsProp(_TB("Splitter Column"), L"", _TB("Splitter column property")),
	m_pCol(pCol)
{
	ASSERT_VALID(m_pCol);

	AllowEdit(FALSE);

	AddOption(_TB("True"), TRUE, 0);
	AddOption(_TB("False"), TRUE, 1);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSSplitterColumnProp::UpdatePropertyValue()
{
	if (m_pCol->m_bSplitterColumn)
		SetValue(GetOption(0));
	else
		SetValue(GetOption(1));
}

//-----------------------------------------------------------------------------
BOOL CRSSplitterColumnProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index >= 0)
	{
		//True
		if (index == 0)
		{
			m_pCol->m_bSplitterColumn = TRUE;
			m_pCol->GetDocument()->m_PageInfo.m_arHPageSplitter.AddSortedUnique(m_pCol->GetColumnTitleRect().right);
		}

		//False
		else if (index == 1)
		{
			m_pCol->m_bSplitterColumn = FALSE;
			m_pCol->GetDocument()->m_PageInfo.m_arHPageSplitter.Remove(m_pCol->GetColumnTitleRect().right);
		}
	}

	//ridisegno tutto
	m_pCol->GetDocument()->Invalidate(TRUE);

	return baseUpdate;
}

//================================CRSTableAllColumnsColorProp==================================
//-----------------------------------------------------------------------------
CRSTableAllColumnsColorProp::CRSTableAllColumnsColorProp(Table* pTable, PropertyType ePropType)
	:
	CRSColorProp(pTable, L"", RGB(0,0,0), L""),
	m_pTable(pTable),
	m_ePropType(ePropType)
{
	ASSERT_VALID(pTable);

	AllowEdit(FALSE);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsColorProp::UpdatePropertyValue()
{
	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of all columns titles"));
		SetColor(m_pTable->m_Columns[0]->m_ColumnTitlePen.m_rgbColor);
		break;
	case PropertyType::BodyBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of body borders"));
		SetColor(m_pTable->m_Columns[0]->m_ColumnPen.m_rgbColor);
		break;
	case PropertyType::TotalBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of all columns totals"));
		SetColor(m_pTable->m_Columns[0]->m_pTotalCell->m_TotalPen.GetColor());
		break;
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableAllColumnsColorProp::OnUpdateValue()
{
	return CBCGPColorProp::OnUpdateValue(); // = do nothing more than super 
}

//-----------------------------------------------------------------------------
BOOL CRSTableAllColumnsColorProp::OnEndEdit() 
{
	if (!IsValueChanged())
		return __super::OnEndEdit();

	BOOL baseOnEndEdit = __super::OnEndEdit();

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBorderColor:
		m_pTable->SetAllColumnTitleBorderColor(this->m_Color);
		break;
	case PropertyType::BodyBorderColor:
	{
		TableColumn* pCol;
		for (int i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			pCol->m_ColumnPen.SetColor(this->GetColor());
		}

		break;
	}

	case PropertyType::TotalBorderColor:
		m_pTable->SetAllTotalsBorderColor(this->m_Color);
		break;
	}

	m_pTable->Redraw();

	return baseOnEndEdit;
}

//================================CRSTableAllColumnsColorWithExprProp==================================
//-----------------------------------------------------------------------------
CRSTableAllColumnsColorWithExprProp::CRSTableAllColumnsColorWithExprProp(Table* pTable, PropertyType ePropType, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorWithExprProp(pTable, L"", RGB(0, 0, 0), NULL, L"", propertyView),
	m_pTable(pTable),
	m_ePropType(ePropType)
{
	ASSERT_VALID(pTable);

	switch (m_ePropType)
	{
		//titles
	case PropertyType::ColumnTitleBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns titles"));
		break;
	case PropertyType::ColumnTitleForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns titles"));
		break;
		//body
	case PropertyType::BodyBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns bodies"));
		break;
	case PropertyType::BodyForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns bodies"));
		break;
		//subtotals
	case PropertyType::SubTotalBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns subtotals"));
		break;
	case PropertyType::SubTotalForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns subtotals"));
		break;
		//totals
	case PropertyType::TotalBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns totals"));
		break;
	case PropertyType::TotalForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns totals"));
		break;
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsColorWithExprProp::UpdatePropertyValue()
{
	int i;
	COLORREF cCommonColor = NULL;
	COLORREF cSingleColor = NULL;
	TableColumn* pCol;

	for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
	{
		pCol = m_pTable->m_Columns[i];

		switch (m_ePropType)
		{
			//column title
		case PropertyType::ColumnTitleBackColor:
			cSingleColor = pCol->m_Title.GetBkgColor();
			break;
		case PropertyType::ColumnTitleForeColor:
			cSingleColor = pCol->m_Title.GetTextColor();
			break;
			//body
		case PropertyType::BodyBackColor:
			cSingleColor = pCol->GetAllCellsBkgColor();
			break;
		case PropertyType::BodyForeColor:
			cSingleColor = pCol->GetAllCellsTextColor();
			break;
			//subtotals
		case PropertyType::SubTotalBackColor:
			cSingleColor = pCol->m_SubTotal.m_rgbBkgColor;
			break;
		case PropertyType::SubTotalForeColor:
			cSingleColor = pCol->m_SubTotal.m_rgbTextColor;
			break;
			//column totals
		case PropertyType::TotalBackColor:
			cSingleColor = *(pCol->m_pTotalCell->GetBkgColor());
			break;
		case PropertyType::TotalForeColor:
			cSingleColor = pCol->m_pTotalCell->GetValueTextColor();
			break;
		}

		if (cSingleColor != NULL)
		{
			if (i == 0 || cCommonColor == NULL)
				cCommonColor = cSingleColor;
			else if (cCommonColor != cSingleColor) break;
		}
	}

	if (i == m_pTable->m_Columns.GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetColor(cCommonColor);

	//altrimenti lascio il colore di default
	else SetColor(m_ColorAutomatic);
}

//-----------------------------------------------------------------------------
BOOL CRSTableAllColumnsColorWithExprProp::OnEndEdit()
{
	if (!IsValueChanged())
		return __super::OnEndEdit();

	BOOL baseOnEndEdit = __super::OnEndEdit();

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBackColor:
		m_pTable->SetAllColumnTitleBkgColor(this->m_Color);
		break;
	case PropertyType::ColumnTitleForeColor:
		m_pTable->SetAllColumnTitleTextColor(this->m_Color);
		break;
	case PropertyType::BodyBackColor:
		m_pTable->SetAllBodyBkgColor(this->m_Color);
		break;
	case PropertyType::BodyForeColor:
		m_pTable->SetAllBodyTextColor(this->m_Color);
		break;
	case PropertyType::SubTotalBackColor:
		m_pTable->SetAllSubTotalsBkgColor(this->m_Color);
		break;
	case PropertyType::SubTotalForeColor:
		m_pTable->SetAllSubTotalsTextColor(this->m_Color);
		break;
	case PropertyType::TotalBackColor:
		m_pTable->SetAllTotalsBkgColor(this->m_Color);
		break;
	case PropertyType::TotalForeColor:
		m_pTable->SetAllTotalsTextColor(this->m_Color);
		break;
	}

	m_pTable->Redraw();

	return baseOnEndEdit;
}

//-----------------------------------------------------------------------------
//return TRUE if at least one element of the multi selection have an expression associated to itself
BOOL CRSTableAllColumnsColorWithExprProp::HasExpression()
{
	int i;
	//todo andrea: vedere come sistemare la symtable
	// right - edit view
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		FALSE;

	TableColumn* pCol;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pTitleBkgColorExpr && !pCol->m_pTitleBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::ColumnTitleForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pTitleTextColorExpr && !pCol->m_pTitleTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pBkgColorExpr && !pCol->m_pBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pTextColorExpr && !pCol->m_pTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_SubTotal.m_pBkgColorExpr && !pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_SubTotal.m_pTextColorExpr && !pCol->m_SubTotal.m_pTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pTotalBkgColorExpr && !pCol->m_pTotalBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (pCol->m_pTotalTextColorExpr && !pCol->m_pTotalTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsColorWithExprProp::OnRightButtonClick()
{
	// open edit view dialog
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	//se ve ne � una presente, recupero l'espressione comune a tutti gli oggetti selezionati
	Expression* commonExpr = GetCommonExpression();
	//altrimenti non ne creo una nuova perch�  se ne preoccupa l'editor che cos� capisce di essere l'owner dell'espression
	/*if (commonExpr == NULL)
		commonExpr = new Expression(m_psymTable);*/

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		&commonExpr,
		DataType::Long,
		TRUE
		);
	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetCommonExpression(commonExpr);

	//delete temp expression
	delete commonExpr;

	//update the multiselection
	m_pTable->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito
								//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsColorWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	//almeno un elemento della selezione ha un espressione associata e lo segnalo come un'immagine
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	}

	//se ho un'espressione associata comune a tutti gli oggetti, la scrivo nel footer
	Expression* commonExpr = GetCommonExpression();
	if (commonExpr != NULL)
	{
		CString expr = (commonExpr)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	//altrimenti lascio il footer originale
	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
//return the commmon expression if there is, NULL otherwise
Expression* CRSTableAllColumnsColorWithExprProp::GetCommonExpression()
{
	int i;
	Expression* commonExp = NULL;
	TableColumn* pCol;

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pTitleBkgColorExpr &&
				!pCol->m_pTitleBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTitleBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTitleBkgColorExpr ||
					pCol->m_pTitleBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTitleBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::ColumnTitleForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pTitleTextColorExpr &&
				!pCol->m_pTitleTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTitleTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTitleTextColorExpr ||
					pCol->m_pTitleTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTitleTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pBkgColorExpr &&
				!pCol->m_pBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pBkgColorExpr ||
					pCol->m_pBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pTextColorExpr &&
				!pCol->m_pTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTextColorExpr ||
					pCol->m_pTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_SubTotal.m_pBkgColorExpr &&
				!pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_SubTotal.m_pBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_SubTotal.m_pBkgColorExpr ||
					pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_SubTotal.m_pBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_SubTotal.m_pTextColorExpr &&
				!pCol->m_SubTotal.m_pTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_SubTotal.m_pTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_SubTotal.m_pTextColorExpr ||
					pCol->m_SubTotal.m_pTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_SubTotal.m_pTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pTotalBkgColorExpr &&
				!pCol->m_pTotalBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTotalBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTotalBkgColorExpr ||
					pCol->m_pTotalBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTotalBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			if (
				i == 0 &&
				pCol->m_pTotalTextColorExpr &&
				!pCol->m_pTotalTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTotalTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTotalTextColorExpr ||
					pCol->m_pTotalTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTotalTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}
	}

	if (!commonExp || commonExp->IsEmpty())
	{
		delete commonExp;
		return NULL;
	}

	else return commonExp;
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsColorWithExprProp::SetCommonExpression(Expression * expr)
{
	int i;
	TableColumn* pCol;
	if (!expr) return;
	switch (m_ePropType)
	{
	case PropertyType::ColumnTitleBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pTitleBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTitleBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::ColumnTitleForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pTitleTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTitleTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_SubTotal.m_pBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_SubTotal.m_pBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_SubTotal.m_pTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_SubTotal.m_pTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pTotalBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTotalBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			pCol = m_pTable->m_Columns[i];
			SAFE_DELETE(pCol->m_pTotalTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTotalTextColorExpr = new Expression(*expr);
		}

		break;
	}
	}
}

//================================CRSTableMultiColumnsColorProp==================================
//-----------------------------------------------------------------------------
CRSTableMultiColumnsColorProp::CRSTableMultiColumnsColorProp(MultiColumnSelection* pColumns, PropertyType ePropType)
	:
	CRSColorProp(pColumns, L"", RGB(0, 0, 0), _TB("Easy reading property")),
	m_pColumns(pColumns),
	m_ePropType(ePropType)
{
	AllowEdit(FALSE);

	switch (m_ePropType)
	{
	case PropertyType::TitleBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of all columns titles"));
		break;
	case PropertyType::BodyBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of all columns bodies"));
		break;
case PropertyType::TotalBorderColor:
		SetName(_TB("Color"));
		SetDescription(_TB("Border color of all columns totals"));
		break;
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsColorProp::UpdatePropertyValue()
{
	int i;
	COLORREF cCommonColor = NULL;
	COLORREF cSingleColor = NULL;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_ePropType)
		{
		case PropertyType::TitleBorderColor:
			cSingleColor = pCol->m_ColumnTitlePen.m_rgbColor;
			break;
		case PropertyType::BodyBorderColor:
			cSingleColor =pCol->m_ColumnPen.m_rgbColor;
			break;
		case PropertyType::TotalBorderColor:
			cSingleColor = pCol->m_pTotalCell->m_TotalPen.m_rgbColor;
			break;
		}

		if (cSingleColor != NULL)
		{
			if (i == 0 || cCommonColor == NULL)
				cCommonColor = cSingleColor;
			else if (cCommonColor != cSingleColor) break;
		}
	}

	if (i == m_pColumns->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetColor(cCommonColor);

	//altrimenti lascio il colore di default
	else SetColor(m_ColorAutomatic);
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsColorProp::OnUpdateValue()
{
	return CBCGPColorProp::OnUpdateValue(); // = do nothing more than super 
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsColorProp::OnEndEdit()
{
	BOOL baseOnEndEdit = __super::OnEndEdit();

	int i;
	COLORREF* pSingleColor;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);
		switch (m_ePropType)
		{
		case PropertyType::TitleBorderColor:
			pSingleColor = &(pCol->m_ColumnTitlePen.m_rgbColor);
			break;
		case PropertyType::BodyBorderColor:
			pCol->m_ColumnPen.SetColor(this->GetColor());
			continue; //go to next column
		case PropertyType::TotalBorderColor:
			pSingleColor = &(pCol->m_pTotalCell->m_TotalPen.m_rgbColor);
			break;
		}
	
		if (pSingleColor != NULL) *pSingleColor = this->GetColor();
	}
	
	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito

	return baseOnEndEdit;
}

//================================CRSTableMultiColumnsColorWithExprProp==================================
//-----------------------------------------------------------------------------
CRSTableMultiColumnsColorWithExprProp::CRSTableMultiColumnsColorWithExprProp(MultiColumnSelection* pColumns, PropertyType ePropType, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorWithExprProp(pColumns, L"", RGB(0, 0, 0), NULL, L"", propertyView),
	m_pColumns(pColumns),
	m_ePropType(ePropType)
{
	AllowEdit(FALSE);

	switch (m_ePropType)
	{
	case PropertyType::TitleBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns titles"));
		break;
	case PropertyType::TitleForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns titles"));
		break;
	case PropertyType::BodyBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns bodies"));
		break;
	case PropertyType::BodyForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns bodies"));
		break;
	case PropertyType::SubTotalBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns subtotals"));
		break;
	case PropertyType::SubTotalForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns subtotals"));
		break;
	case PropertyType::TotalBackColor:
		SetName(_TB("Background Color"));
		SetDescription(_TB("Background color of all columns totals"));
		break;
	case PropertyType::TotalForeColor:
		SetName(_TB("Text Color"));
		SetDescription(_TB("Text Color of all columns totals"));
		break;
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsColorWithExprProp::UpdatePropertyValue()
{
	int i;
	COLORREF cCommonColor = NULL;
	COLORREF cSingleColor = NULL;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_ePropType)
		{
		case PropertyType::TitleBackColor:
			cSingleColor = pCol->m_Title.m_rgbBkgColor;
			break;
		case PropertyType::TitleForeColor:
			cSingleColor = pCol->m_Title.m_rgbTextColor;
			break;
		case PropertyType::BodyBackColor:
			COLORREF bkgColor;
			pCol->GetAllCellsBkgColor(bkgColor);
			cSingleColor = bkgColor;
			break;
		case PropertyType::BodyForeColor:
			COLORREF textColor;
			pCol->GetAllCellsTextColor(textColor);
			cSingleColor = textColor;
			break;
		case PropertyType::SubTotalBackColor:
			cSingleColor = pCol->m_SubTotal.m_rgbBkgColor;
			break;
		case PropertyType::SubTotalForeColor:
			cSingleColor = pCol->m_SubTotal.m_rgbTextColor;
			break;
		case PropertyType::TotalBackColor:
			cSingleColor = *(pCol->m_pTotalCell->GetBkgColor());
			break;
		case PropertyType::TotalForeColor:
			cSingleColor = pCol->m_pTotalCell->GetValueTextColor();
			break;
		}

		if (cSingleColor != NULL)
		{
			if (i == 0 || cCommonColor == NULL)
				cCommonColor = cSingleColor;
			else if (cCommonColor != cSingleColor) break;
		}
	}

	if (i == m_pColumns->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetColor(cCommonColor);

	//altrimenti lascio il colore di default
	else SetColor(m_ColorAutomatic);
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsColorWithExprProp::OnEndEdit()
{
	BOOL baseOnEndEdit = __super::OnEndEdit();

	int i;
	COLORREF* pSingleColor;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);
		switch (m_ePropType)
		{
		case PropertyType::TitleBackColor:
			pSingleColor = &(pCol->m_Title.m_rgbBkgColor);
			break;
		case PropertyType::TitleForeColor:
			pSingleColor = &(pCol->m_Title.m_rgbTextColor);
			break;
		case PropertyType::BodyBackColor:
			pCol->SetAllCellsBkgColor(this->GetColor());
			continue; //go to next column
		case PropertyType::BodyForeColor:
			pCol->SetAllCellsTextColor(this->GetColor());
			continue; //go to next column
		case PropertyType::SubTotalBackColor:
			pCol->m_SubTotal.m_rgbBkgColor = this->GetColor();
			continue;
		case PropertyType::SubTotalForeColor:
			pCol->m_SubTotal.m_rgbTextColor = this->GetColor();
			continue;
		case PropertyType::TotalBackColor:
			pSingleColor = pCol->m_pTotalCell->GetBkgColor();
			break;
		case PropertyType::TotalForeColor:
			pSingleColor = &(pCol->m_pTotalCell->m_Value.m_rgbTextColor);
			break;
		}

		if (pSingleColor != NULL) *pSingleColor = this->GetColor();
	}

	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito

	return baseOnEndEdit;
}

//-----------------------------------------------------------------------------
//return TRUE if at least one element of the multi selection have an expression associated to itself
BOOL CRSTableMultiColumnsColorWithExprProp::HasExpression()
{
	int i;
	//todo andrea: vedere come sistemare la symtable
	// right - edit view
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		FALSE;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	switch (m_ePropType)
	{
	case PropertyType::TitleBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
				if (pCol->m_pTitleBkgColorExpr && !pCol->m_pTitleBkgColorExpr->IsEmpty())
					return TRUE;
		}

		break;
	}

	case PropertyType::TitleForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_pTitleTextColorExpr && !pCol->m_pTitleTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_pBkgColorExpr && !pCol->m_pBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_pTextColorExpr && !pCol->m_pTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_SubTotal.m_pBkgColorExpr && !pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_SubTotal.m_pTextColorExpr && !pCol->m_SubTotal.m_pTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_pTotalBkgColorExpr && !pCol->m_pTotalBkgColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (pCol->m_pTotalTextColorExpr && !pCol->m_pTotalTextColorExpr->IsEmpty())
				return TRUE;
		}

		break;
	}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsColorWithExprProp::OnRightButtonClick()
{
	// open edit view dialog
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	//se ve ne � una presente, recupero l'espressione comune a tutti gli oggetti selezionati
	Expression* commonExpr = GetCommonExpression();

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		&commonExpr,
		DataType::Long,
		TRUE
		);
	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetCommonExpression(commonExpr);

	//delete temp expression
	delete commonExpr;

	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito
								//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsColorWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	//almeno un elemento della selezione ha un espressione associata e lo segnalo come un'immagine
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	}

	//se ho un'espressione associata comune a tutti gli oggetti, la scrivo nel footer
	Expression* commonExpr = GetCommonExpression();
	if (commonExpr != NULL)
	{
		CString expr = (commonExpr)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	//altrimenti lascio il footer originale
	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
//return the commmon expression if there is, NULL otherwise
Expression* CRSTableMultiColumnsColorWithExprProp::GetCommonExpression()
{
	int i;
	Expression* commonExp = NULL;

	switch (m_ePropType)
	{
	case PropertyType::TitleBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
					i == 0 &&
					pCol->m_pTitleBkgColorExpr &&
					!pCol->m_pTitleBkgColorExpr->IsEmpty()
					)
					commonExp = new Expression(*(pCol->m_pTitleBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTitleBkgColorExpr ||
					pCol->m_pTitleBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTitleBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::TitleForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_pTitleTextColorExpr &&
				!pCol->m_pTitleTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTitleTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTitleTextColorExpr ||
					pCol->m_pTitleTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTitleTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_pBkgColorExpr &&
				!pCol->m_pBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pBkgColorExpr ||
					pCol->m_pBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_pTextColorExpr &&
				!pCol->m_pTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTextColorExpr ||
					pCol->m_pTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_SubTotal.m_pBkgColorExpr &&
				!pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_SubTotal.m_pBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_SubTotal.m_pBkgColorExpr ||
					pCol->m_SubTotal.m_pBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_SubTotal.m_pBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_SubTotal.m_pTextColorExpr &&
				!pCol->m_SubTotal.m_pTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_SubTotal.m_pTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_SubTotal.m_pTextColorExpr ||
					pCol->m_SubTotal.m_pTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_SubTotal.m_pTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_pTotalBkgColorExpr &&
				!pCol->m_pTotalBkgColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTotalBkgColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTotalBkgColorExpr ||
					pCol->m_pTotalBkgColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTotalBkgColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			if (
				i == 0 &&
				pCol->m_pTotalTextColorExpr &&
				!pCol->m_pTotalTextColorExpr->IsEmpty()
				)
				commonExp = new Expression(*(pCol->m_pTotalTextColorExpr));
			else
				if (
					!commonExp ||
					commonExp->IsEmpty() ||
					!pCol->m_pTotalTextColorExpr ||
					pCol->m_pTotalTextColorExpr->IsEmpty() ||
					commonExp->ToString() != pCol->m_pTotalTextColorExpr->ToString()
					)
					return NULL;
		}

		break;
	}
	}

	if (!commonExp || commonExp->IsEmpty())
	{
		delete commonExp;
		return NULL;
	}

	else return commonExp;
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsColorWithExprProp::SetCommonExpression(Expression * expr)
{
	if (!expr) return;
	int i;

	switch (m_ePropType)
	{
	case PropertyType::TitleBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pTitleBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTitleBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::TitleForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pTitleTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTitleTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::BodyBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::BodyForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::SubTotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_SubTotal.m_pBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_SubTotal.m_pBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::SubTotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_SubTotal.m_pTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_SubTotal.m_pTextColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::TotalBackColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pTotalBkgColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTotalBkgColorExpr = new Expression(*expr);
		}

		break;
	}

	case PropertyType::TotalForeColor:
	{
		for (i = 0; i < m_pColumns->GetSize(); i++)
		{
			TableColumn* pCol = m_pColumns->GetAt(i);
			SAFE_DELETE(pCol->m_pTotalTextColorExpr);
			if (!expr->IsEmpty())
				pCol->m_pTotalTextColorExpr = new Expression(*expr);
		}

		break;
	}
	}
}

//================================CRSTableMultiColumnsBoolProp==================================
//-----------------------------------------------------------------------------
CRSTableMultiColumnsBoolProp::CRSTableMultiColumnsBoolProp(MultiColumnSelection* pColumns, PropertyType ePropType)
	:
	CrsProp(L"", L"", L""),
	m_pColumns(pColumns),
	m_ePropType(ePropType)
{
	AllowEdit(FALSE);

	AddOption(_T("True"));
	AddOption(_T("False"));

	switch (m_ePropType)
	{
	case PropertyType::Hidden:
		SetName(_TB("Hidden"));
		SetDescription(_TB("NO EXPRESSION"));
		break;
	}

	UpdatePropertyValue();

	BOOL shouldBeHidden = this->GetValue() == _TB("True") ? TRUE : FALSE;
	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsBoolProp::UpdatePropertyValue()
{
	int i;
	BOOL bCommonValue = FALSE;
	BOOL bSingleValue = FALSE;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_ePropType)
		{
		case PropertyType::Hidden :
			bSingleValue = pCol->m_bIsHidden;
			break;
		}

		if (i == 0 )
			bCommonValue = bSingleValue;
		else if (bCommonValue != bSingleValue) break;
	}

	if (i == m_pColumns->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetValue(bCommonValue?_T("True"): _T("False"));

	//altrimenti lascio vuoto
	else SetValue(L"");
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsBoolProp::UpdateValue()
{
	BOOL shouldBeHidden = this->GetValue() == _TB("True") ? TRUE : FALSE;
	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);

	TableColumn* pCol;
	for (int i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);
		switch (m_ePropType)
		{
		case PropertyType::Hidden:
		{
			int index = pCol->GetTable()->GetColumnIndexByPtr(pCol);

			if (!pCol->m_pHideExpr && pCol->IsHidden() != shouldBeHidden)
				pCol->GetTable()->SetColumnHiddenStatus(index, shouldBeHidden);

			if (!pCol->m_pHideExpr && pCol->IsHidden())
				pCol->RemoveSplitter();

			pCol->GetTable()->m_pDocument->Invalidate();
			pCol->GetTable()->m_pDocument->SetModifiedFlag();
			pCol->GetTable()->Redraw();

			if (!shouldBeHidden)
				pCol->Redraw();

			break;
		}
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsBoolProp::OnUpdateValue()
{
	BOOL baseOnUpdate = __super::OnUpdateValue();

	UpdateValue();
	
	GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pColumns);

	//update the multiselection
	m_pColumns->Redraw();		//una volta sola per tutti gli switch, quindi codice pi� efficente

	return baseOnUpdate;
}

//==================================================================
//-----------------------------------------------------------------------------
CRSMultiColumnsHiddenProp::CRSMultiColumnsHiddenProp(MultiColumnSelection* pColumns, CRS_ObjectPropertyView* pPropertyView)
	:
	CRSTableMultiColumnsBoolProp(pColumns, CRSTableMultiColumnsBoolProp::PropertyType::Hidden),
	m_pPropertyView(pPropertyView)
{
	ASSERT_VALID(m_pColumns);

	m_bHasState = TRUE;

	m_imageExpr.SetImageSize(CSize(14, 14));
	m_imageExpr.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageExpr);
	HICON icon = TBLoadPng(TBGlyph(szGlyphQuestionMark));
	m_imageExpr.AddIcon(icon);
	::DestroyIcon(icon);

	UpdatePropertyValue();

	SetState();
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::UpdatePropertyValue()
{
	CRSTableMultiColumnsBoolProp::UpdatePropertyValue();
	
	Expression* pExp = GetCommonExpression();
	if (pExp && !pExp->IsEmpty())
	{
		CString expr = pExp->ToString();
		SetValue((_variant_t)expr);
	}

	else if(HasExpression())
		//altrimenti lascio la property vuota
		SetValue((_variant_t)_T(""));
}

//-----------------------------------------------------------------------------
//return the commmon expression if there is, NULL otherwise
Expression* CRSMultiColumnsHiddenProp::GetCommonExpression()
{
	int i;
	Expression* commonExp = NULL;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		TableColumn* pCol = m_pColumns->GetAt(i);
		if (
			i == 0 &&
			pCol->m_pHideExpr &&
			!pCol->m_pHideExpr->IsEmpty()
			)
			commonExp = new Expression(*(pCol->m_pHideExpr));
		else
			if (
				!commonExp ||
				commonExp->IsEmpty() ||
				!pCol->m_pHideExpr ||
				pCol->m_pHideExpr->IsEmpty() ||
				commonExp->ToString() != pCol->m_pHideExpr->ToString()
				)
				return NULL;
	}

	if (!commonExp || commonExp->IsEmpty())
	{
		delete commonExp;
		return NULL;
	}

	else return commonExp;
}

//-----------------------------------------------------------------------------
//return TRUE if at least one element of the multi selection have an expression associated to itself
BOOL CRSMultiColumnsHiddenProp::HasExpression()
{
	int i;
	//todo andrea: vedere come sistemare la symtable
	// right - edit view
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		FALSE;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		TableColumn* pCol = m_pColumns->GetAt(i);
		if (pCol->m_pHideExpr && !pCol->m_pHideExpr->IsEmpty())
			return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::SetCommonExpression(Expression * expr)
{
	if (!expr) return;
	int i;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		TableColumn* pCol = m_pColumns->GetAt(i);

		//cancello la vecchia espressione
		SAFE_DELETE(pCol->m_pHideExpr);
		//setto la nuova uguale a quella comnue
		if (!expr->IsEmpty())
			pCol->m_pHideExpr = new Expression(*expr);
	}	
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::SetState()
{
	BOOL shouldBeHidden = this->GetValue() == _TB("True") ? TRUE : FALSE;
	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	//almeno un elemento della selezione ha un espressione associata e lo segnalo come un'immagine
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	}

	//se ho un'espressione associata comune a tutti gli oggetti, la scrivo nel footer
	Expression* commonExpr = GetCommonExpression();
	if (commonExpr != NULL)
	{
		CString expr = (commonExpr)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	//altrimenti lascio il footer originale
	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - combobox
		CRSTableMultiColumnsBoolProp::OnClickButton(point);
	}

	else
	{
		OnRightButtonClick();
		UpdatePropertyValue();
	}
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::OnRightButtonClick()
{
	// open edit view dialog
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	//se ve ne � una presente, recupero l'espressione comune a tutti gli oggetti selezionati
	Expression* commonExpr = GetCommonExpression();

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		&commonExpr,
		DataType::Bool,
		TRUE
		);
	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetCommonExpression(commonExpr);

	//delete temp expression
	delete commonExpr;

	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito
								//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();

	GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pColumns);
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
BOOL CRSMultiColumnsHiddenProp::OnUpdateValue()
{
	//ottimizzazione:
	CString strOldValue = this->GetValue();

	BOOL baseUpdate = CrsProp::OnUpdateValue();

	CString strNewValue = this->GetValue();

	if (strOldValue != strNewValue) //solo se il valore della property � effettivamente cambiato
	{
		if (HasExpression())//era presente almeno un espressione, mi devo sincerare l'utente voglia cancellarla per settare una visibilit� non dinamica
		{
			if (AfxTBMessageBox(_TB("This change will erase the dynamic expression currently set. Are you sure you want to proceed?"), MB_ICONWARNING | MB_YESNO) == IDNO)
			{
				//risetto il precedente valore(l'espressione) 
				SetValue((_variant_t)strOldValue);
				return baseUpdate;
			}

			//se l'utente � sicuro, cancello tutte le eventuali espressioni settate
			for (int i = 0; i < m_pColumns->GetSize(); i++)
			{
				TableColumn* pCol = m_pColumns->GetAt(i);
				if (pCol->m_pHideExpr && !pCol->m_pHideExpr->IsEmpty())
				{
					SAFE_DELETE(pCol->m_pHideExpr);
					pCol->m_pHideExpr = NULL;
				}
			}
		}

		UpdateValue();
		UpdatePropertyValue();
		m_pColumns->Redraw();
		RedrawState();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pColumns);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsHiddenProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw combobox button at left
				CBCGPProp::OnDrawButton(pDC, rect);
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			// Draw push button at right
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//================================CRSTableAllColumnsBorderSizeProp==================================
//-----------------------------------------------------------------------------
CRSTableAllColumnsBorderSizeProp::CRSTableAllColumnsBorderSizeProp(Table* pTable, PropertyType ePropType)
	:
	CrsProp(L"", 0 , _TB("Border Size property")),
	m_pTable(pTable),
	m_ePropType(ePropType)
{
	ASSERT_VALID(pTable);

	AllowEdit(TRUE);
	EnableSpinControl(TRUE, 0, 100);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableAllColumnsBorderSizeProp::UpdatePropertyValue()
{
	SetName(_TB("Size"));

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitle:
		SetDescription(_TB("Size of all columns title borders"));
		SetValue(m_pTable->m_Columns[0]->m_ColumnTitlePen.m_nWidth);
		break;
	case PropertyType::Total:
		SetDescription(_TB("Size of all columns total borders"));
		SetValue(m_pTable->m_Columns[0]->m_pTotalCell->m_TotalPen.GetWidth());
		break;
	case PropertyType::Body:
		SetDescription(_TB("Size of all body borders"));
		SetValue(m_pTable->m_Columns[0]->m_ColumnPen.GetWidth());
		break;
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableAllColumnsBorderSizeProp::OnUpdateValue()
{
	BOOL baseUpdate =  CBCGPProp::OnUpdateValue(); // = do nothing more than super

	switch (m_ePropType)
	{
	case PropertyType::ColumnTitle:
		m_pTable->SetAllColumnTitleBorderWidth(this->GetValue());
		break;
	case PropertyType::Total:
		m_pTable->SetAllTotalsBorderWidth(this->GetValue());
		break;
	case PropertyType::Body:
		m_pTable->SetBodyBorderWidth(this->GetValue());
		break;
	}

	m_pTable->Redraw();

	return baseUpdate;
}

//================================CRSTableMultiColumnsSizeProp==================================
//-----------------------------------------------------------------------------
CRSTableMultiColumnsSizeProp::CRSTableMultiColumnsSizeProp(MultiColumnSelection* pColumns, PropertyType ePropType, int minValue, int maxValue)
	:
	CrsProp(_TB("Size in pixel"), 0, _TB("Border Size property")),
	m_pColumns(pColumns),
	m_ePropType(ePropType),
	m_nMinValue(minValue),
	m_nMaxValue(maxValue)
{
	AllowEdit(TRUE);
	EnableSpinControl(TRUE, m_nMinValue, m_nMaxValue);

	UpdatePropertyValue();
	AddMMProp(minValue, maxValue);
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsSizeProp::UpdatePropertyValue()
{
	int i;
	int commonWidth;
	int singlewidth;
	TableColumn* pCol;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_ePropType)
		{
		case PropertyType::Width:
			singlewidth = pCol->m_nWidth;
			break;
		}

		if (i == 0)
			commonWidth = singlewidth;
		else if (commonWidth != singlewidth) break;
	}

	if (i == m_pColumns->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetValue(commonWidth);

	//altrimenti lascio la size di default
	else SetValue(m_nDefaultSize);
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsSizeProp::OnUpdateValue()
{
	int prevValue = this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();
	
	UpdateIntValue(prevValue);

	UpdateMmProp();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsSizeProp::UpdateIntValue(int prevValue)
{
	//limite minvalue < x < maxvalue anche sull'input manuale con tastiera-----------
	int value = GetValue();
	if (value > m_nMaxValue)
	{
		SetValue(m_nMaxValue);
		value = m_nMaxValue;
	}

	else if (value < m_nMinValue)
	{
		SetValue(m_nMinValue);
		value = m_nMinValue;
	}

	//-------------------------------------------------------------------------------
	//ottimizzazione per evitare di ridisegnare l'oggetto se il valore in realt� non � cambiato
	if (value == prevValue && value != m_nDefaultSize)
		return;

	int i;
	TableColumn* pCol;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);
		if (pCol->IsHidden())
			continue;
		switch (m_ePropType)
		{
		case PropertyType::Width:
			Table* pTable = pCol->GetTable();
			pTable->OnColumnSetWidth(pCol, value, TRUE, FALSE);
			break;
		}
	}

	//update the multiselection
	m_pColumns->BuildBaseRect();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito
}

//================================CRSMMProp==================================
//-----------------------------------------------------------------------------
CRSMMProp::CRSMMProp(CrsProp* pParentProp, CString strName, int initialValueInPixel, LPCTSTR lpszDescr, int minValue, int maxValue)
	:
	CrsProp(strName, (_variant_t)initialValueInPixel,lpszDescr),
	m_pParentProp(pParentProp)
{
	AllowEdit(TRUE);
	EnableSpinControl(TRUE, minValue, maxValue);

	UpdateMM(initialValueInPixel);
}

//-----------------------------------------------------------------------------
int CRSMMProp::PixelInMM(int pixel)
{
	return (int)floor(LPtoMU(pixel, CM, 10., 3));
}

//-----------------------------------------------------------------------------
void CRSMMProp::UpdateMM(int pixel)
{
	int widthInMm = PixelInMM(pixel);
	if(widthInMm!= (int)GetValue())
		SetValue(widthInMm);
}

//-----------------------------------------------------------------------------
int CRSMMProp::MMinPixel()
{
	int pixel = MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
	return pixel;
}

//-----------------------------------------------------------------------------
BOOL CRSMMProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();
	//----------------------------------------------------------controllo sui valori inseriti dall'utente
	int value = GetValue();
	if (value > m_nMaxValue)
	{
		SetValue(m_nMaxValue);
		value = m_nMaxValue;
	}

	else if (value < m_nMinValue)
	{
		SetValue(m_nMinValue);
		value = m_nMinValue;
	}

	//----------------------------------------------------------update of parent value
	int prevValue = m_pParentProp->GetValue();
	m_pParentProp->SetValue(MMinPixel());
	m_pParentProp->UpdateIntValue(prevValue);
	//----------------------------------------------------------
	return baseUpdate;
}

//================================CRSTableMultiColumnsBorderSizeProp==================================
//-----------------------------------------------------------------------------
CRSTableMultiColumnsBorderSizeProp::CRSTableMultiColumnsBorderSizeProp(MultiColumnSelection* pColumns, PropertyType ePropType)
	:
	CrsProp(_TB("Size"), 0, _TB("Border Size property")),
	m_pColumns(pColumns),
	m_ePropType(ePropType)
{
	AllowEdit(TRUE);
	EnableSpinControl(TRUE, 0, 100);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableMultiColumnsBorderSizeProp::UpdatePropertyValue()
{
	int i;
	int commonWidth;
	int singlewidth;
	TableColumn* pCol;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_ePropType)
		{
		case PropertyType::Title:
			singlewidth = pCol->GetColumnTitlePen().m_nWidth;
			break;
		case PropertyType::Body:
			singlewidth = pCol->GetColumnPen().m_nWidth;
			break;
		case PropertyType::Total:
			singlewidth = pCol->GetTotalPen().m_nWidth;
			break;
		}

		if (i == 0)
			commonWidth = singlewidth;
		else if (commonWidth != singlewidth) break;
	}

	//TODO ANDREA: SE NON SONO TUTTI UGUALI ->IMPOSTARE IL DEFAULT
}

//-----------------------------------------------------------------------------
BOOL CRSTableMultiColumnsBorderSizeProp::OnUpdateValue()
{
	BOOL baseUpdate = CBCGPProp::OnUpdateValue(); // = do nothing more than super
	int i;
	TableColumn* pCol;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);
		switch (m_ePropType)
		{
		case PropertyType::Title:
			pCol->SetColumnTitleBorderWidth(this->GetValue());
			break;
		case PropertyType::Body:
			pCol->m_ColumnPen.SetWidth(this->GetValue());
			break;
		case PropertyType::Total:
			pCol->SetTotalBorderWidth(this->GetValue());
			break;
		}
	}

	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito

	return baseUpdate;
}

//================================CRSRowCountProp==================================
//-----------------------------------------------------------------------------
CRSRowCountProp::CRSRowCountProp(Table* pTable)
	:
	CrsProp(_TB("Row Count"), 0, _TB("Row Count property")),
	m_pTable(pTable)
{
	ASSERT_VALID(pTable);

	EnableSpinControl(TRUE,1,32767);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSRowCountProp::UpdatePropertyValue()
{
	SetValue(m_pTable->RowsNumber());
}

//-----------------------------------------------------------------------------
BOOL CRSRowCountProp::OnUpdateValue()
{
	int previousCount = (int)this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	//pulisco
	m_pTable->Redraw(FALSE);

	int actualValue = (int)this->GetValue();

	//se l'utente digita un numero fuori dal range consentito, lo risetto ai valori minimo o massimo
	if (actualValue < m_nMinValue || actualValue>m_nMaxValue)
	{
		actualValue = (actualValue < 1) ? m_nMinValue : m_nMaxValue;
		SetValue(actualValue);
	}

	int diffCount = actualValue - previousCount;

	ASSERT((previousCount + diffCount)>0 && (previousCount + diffCount) < 32767);

	m_pTable->ModifyRowsNumber(diffCount);

	//ridisegno tutto
	m_pTable->Redraw(TRUE);

	return baseUpdate;
}

//================================CRSTableHeightsProp==================================
//-----------------------------------------------------------------------------
CRSTableHeightsProp::CRSTableHeightsProp(Table* pTable, PropertyType propType)
	:
	CrsProp(_T("px"), (_variant_t) 0, _T("description")),
	m_pTable(pTable),
	m_propType(propType)
{
	ASSERT_VALID(pTable);
	EnableSpinControl(TRUE, 1, 32767);

	switch (m_propType)
	{
		case PropertyType::TableTitle:
		{
			SetDescription(_TB("The height of the table title"));
			break;
		}

		case PropertyType::ColumnTitle:
		{
			SetDescription(_TB("The height of columns title"));
			break;
		}

		case PropertyType::Row:
		{
			SetDescription(_TB("The height of rows"));
			break;
		}

		case PropertyType::Total:
		{
			SetDescription(_TB("The height of the totals"));
			break;
		}

	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableHeightsProp::UpdatePropertyValue()
{
	CBCGPProp* parent = this->GetParent();

	switch (m_propType)
	{
		case PropertyType::TableTitle:
		{
			SetValue(m_pTable->m_TitleRect.Height());
			if (parent)
				parent->GetSubItem(1)->SetValue((int)floor(LPtoMU(m_pTable->m_TitleRect.Height(), CM, 10., 3)));
			break;
		}

		case PropertyType::ColumnTitle:
		{
			SetValue(m_pTable->ColumnTitleRect(0).Height()); 
			if (parent)
				parent->GetSubItem(1)->SetValue((int)floor(LPtoMU(m_pTable->ColumnTitleRect(0).Height(), CM, 10., 3)));
			break;
		}

		case PropertyType::Row:
		{
			SetValue(m_pTable->CellRect(0,0).Height());
			if (parent)
				parent->GetSubItem(1)->SetValue((int)floor(LPtoMU(m_pTable->CellRect(0, 0).Height(), CM, 10., 3)));
			break;
		}

		case PropertyType::Total:
		{
			SetValue(m_pTable->TotalRect(0).Height());
			if (parent)
				parent->GetSubItem(1)->SetValue((int)floor(LPtoMU(m_pTable->TotalRect(0).Height(), CM, 10., 3)));
			break;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableHeightsProp::OnUpdateValue()
{
	//int previousCount = (int)this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	//pulisco
	m_pTable->Redraw(FALSE);

	int actualValue = (int)this->GetValue();

	if (this->GetParent())
		GetParent()->GetSubItem(1)->SetValue((int)floor(LPtoMU((int)this->GetValue(), CM, 10., 3)));

	//se l'utente digita un numero fuori dal range consentito, lo risetto ai valori minimo o massimo
	if (actualValue < m_nMinValue || actualValue > m_nMaxValue)
	{
		actualValue = (actualValue < 1) ? m_nMinValue : m_nMaxValue;
		SetValue(actualValue);
	}

	switch (m_propType)
	{
		case PropertyType::TableTitle:
		{
			m_pTable->RebuildTableSizes
				(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				actualValue,
				m_pTable->ColumnTitleRect(0).Height(),
				m_pTable->CellRect(0, 0).Height(),
				m_pTable->TotalRect(0).Height()
				);
			break;
		}

		case PropertyType::ColumnTitle:
		{
			m_pTable->RebuildTableSizes
				(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				actualValue,
				m_pTable->CellRect(0, 0).Height(),
				m_pTable->TotalRect(0).Height()
				);
			break;
		}

		case PropertyType::Row:
		{
			m_pTable->RebuildTableSizes
				(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				m_pTable->ColumnTitleRect(0).Height(),
				actualValue,
				m_pTable->TotalRect(0).Height()
				);
			break;
		}

		case PropertyType::Total:
		{
			m_pTable->RebuildTableSizes
				(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				m_pTable->ColumnTitleRect(0).Height(),
				m_pTable->CellRect(0, 0).Height(),
				actualValue
				);
			break;
		}

	}

	//ridisegno tutto
	m_pTable->Redraw(TRUE);

	return baseUpdate;
}

//================================CRSTableHeightsPropMM==================================
//-----------------------------------------------------------------------------
CRSTableHeightsPropMM::CRSTableHeightsPropMM(Table* pTable, PropertyType propType)
	:
	CrsProp(_T("mm"), (_variant_t)0, _T("description")),
	m_pTable(pTable),
	m_propType(propType)
{
	EnableSpinControl(TRUE, 0, 8669);

	switch (m_propType)
	{
	case PropertyType::TableTitle:
	{
		SetDescription(_TB("The height of the table title"));
		break;
	}

	case PropertyType::ColumnTitle:
	{
		SetDescription(_TB("The height of columns title"));
		break;
	}

	case PropertyType::Row:
	{
		SetDescription(_TB("The height of rows"));
		break;
	}

	case PropertyType::Total:
	{
		SetDescription(_TB("The height of the totals"));
		break;
	}

	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSTableHeightsPropMM::UpdatePropertyValue()
{
	CBCGPProp* parent = this->GetParent();

	switch (m_propType)
	{
	case PropertyType::TableTitle:
	{
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pTable->m_TitleRect.Height());

		SetValue((int)floor(LPtoMU(m_pTable->m_TitleRect.Height(), CM, 10., 3)));
		break;
	}

	case PropertyType::ColumnTitle:
	{
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pTable->ColumnTitleRect(0).Height());

		SetValue((int)floor(LPtoMU(m_pTable->ColumnTitleRect(0).Height(), CM, 10., 3)));		
		break;
	}

	case PropertyType::Row:
	{
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pTable->CellRect(0, 0).Height());

		SetValue((int)floor(LPtoMU(m_pTable->CellRect(0, 0).Height(), CM, 10., 3)));
		break;
	}

	case PropertyType::Total:
	{
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pTable->TotalRect(0).Height());

		SetValue((int)floor(LPtoMU(m_pTable->TotalRect(0).Height(), CM, 10., 3)));
		break;
	}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTableHeightsPropMM::OnUpdateValue()
{
	//int previousCount = (int)this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	//pulisco
	m_pTable->Redraw(FALSE);

	int actualValue = MUtoLP((int)this->GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);

	if (this->GetParent())
		GetParent()->GetSubItem(0)->SetValue(actualValue);

	//se l'utente digita un numero fuori dal range consentito, lo risetto ai valori minimo o massimo
	if (actualValue < m_nMinValue || actualValue > m_nMaxValue)
	{
		actualValue = (actualValue < 1) ? m_nMinValue : m_nMaxValue;
		SetValue(actualValue);
	}

	switch (m_propType)
	{
	case PropertyType::TableTitle:
	{
		m_pTable->RebuildTableSizes
			(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				actualValue,
				m_pTable->ColumnTitleRect(0).Height(),
				m_pTable->CellRect(0, 0).Height(),
				m_pTable->TotalRect(0).Height()
				);
		break;
	}

	case PropertyType::ColumnTitle:
	{
		m_pTable->RebuildTableSizes
			(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				actualValue,
				m_pTable->CellRect(0, 0).Height(),
				m_pTable->TotalRect(0).Height()
				);
		break;
	}

	case PropertyType::Row:
	{
		m_pTable->RebuildTableSizes
			(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				m_pTable->ColumnTitleRect(0).Height(),
				actualValue,
				m_pTable->TotalRect(0).Height()
				);
		break;
	}

	case PropertyType::Total:
	{
		m_pTable->RebuildTableSizes
			(
				CPoint(m_pTable->m_BaseRect.left, m_pTable->m_BaseRect.top),
				m_pTable->m_TitleRect.Height(),
				m_pTable->ColumnTitleRect(0).Height(),
				m_pTable->CellRect(0, 0).Height(),
				actualValue
				);
		break;
	}

	}

	//ridisegno tutto
	m_pTable->Redraw(TRUE);

	return baseUpdate;
}

//================================CRSTableTitlTextProp==================================
//-----------------------------------------------------------------------------
CRSTableTitlTextProp::CRSTableTitlTextProp(Table* pTable)
	:
	CRSStringProp(pTable, _T("Text"), &(pTable->m_Title.m_strTitle), _T("Text of the title")),
	m_pTable(pTable)
{
}

//-----------------------------------------------------------------------------
BOOL CRSTableTitlTextProp::OnUpdateValue()
{
	//chiamo il super che chiama a sua volta il super. Ovvero aggiorna il valore e poi ridisegna la tabella
	BOOL baseUpdate = __super::OnUpdateValue();

	m_pTable->m_Title.SetText(GetValue());

	//todo vedere se serve richiamare la update per formattare. infatti la formattazione viene fatta nella settext, chiamata dopo il primo super
	__super::OnUpdateValue();

	return baseUpdate;
}

//================================CRSMulBoolProp==================================
//-----------------------------------------------------------------------------
CRSMulBoolProp::CRSMulBoolProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description)
	:
	CrsProp(strName, L"", description),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(m_pMulSel);
	
	AllowEdit(FALSE);

	//ordinamento comodo per settare i valori con l'indice-> cambiare prestando attenzione
	AddOption(_TB("False"), TRUE, 0);
	AddOption(_TB("True"), TRUE, 1);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
void CRSMulBoolProp::UpdateValue()
{
	int index = -1;
	if (m_pWndList != NULL)
		index = GetSelectedOption();

	if (index <0 || index > 1)
		return;

	switch (m_propType)
	{
	case PropertyType::Transparent:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			obj->m_bTransparent = index;
			//obj->Redraw();	//---------------------------------> pi� efficiente perch� ciclo una volta sola
		}

		//m_pMulSel->GetObjAt(0)->UpdateWindow(); ----------------->capire se serviva
		break;
	}

	case PropertyType::BorderSideLeft:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_Borders.left = index;
		}

		break;
	}

	case PropertyType::BorderSideTop:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_Borders.top = index;
		}

		break;
	}

	case PropertyType::BorderSideRight:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_Borders.right = index;
		}

		break;
	}

	case PropertyType::BorderSideBottom:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_Borders.bottom = index;
		}

		break;
	}

	case PropertyType::Hidden:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_bHidden = index;
		}

		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pMulSel);

		if (index)
			SetColoredState(CrsProp::State::Important);
		else
			SetColoredState(CrsProp::State::Normal);

		break;
	}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSMulBoolProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	UpdateValue();

	//update the multiselection
	m_pMulSel->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi pi� pulito

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSMulBoolProp::UpdatePropertyValue()
{
	int i = 0;
	BOOL bCommonValue = FALSE;
	BOOL commonValueSetted = FALSE;

	switch (m_propType)
	{
	case PropertyType::Transparent:
	{
		//should be the first row of every case statement
		bCommonValue = m_pMulSel->GetObjAt(0)->m_bTransparent;
		
		for (i = 0; i < m_pMulSel->GetSize(); i++)
			if (bCommonValue != m_pMulSel->GetObjAt(i)->m_bTransparent) break;
		break;
	}

	case PropertyType::BorderSideLeft:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || commonValueSetted == FALSE)
			{
				bCommonValue = pbase->m_Borders.left;
				commonValueSetted = TRUE;
			}

			else if (bCommonValue != pbase->m_Borders.left) break;
		}

		break;
	}

	case PropertyType::BorderSideTop:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || commonValueSetted == FALSE)
			{
				bCommonValue = pbase->m_Borders.top;
				commonValueSetted = TRUE;
			}

			else if (bCommonValue != pbase->m_Borders.top) break;
		}

		break;
	}

	case PropertyType::BorderSideRight:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || commonValueSetted == FALSE)
			{
				bCommonValue = pbase->m_Borders.right;
				commonValueSetted = TRUE;
			}

			else if (bCommonValue != pbase->m_Borders.right) break;
		}

		break;
	}

	case PropertyType::BorderSideBottom:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || commonValueSetted == FALSE)
			{
				bCommonValue = pbase->m_Borders.bottom;
				commonValueSetted = TRUE;
			}

			else if (bCommonValue != pbase->m_Borders.bottom) break;
		}

		break;
	}

	case PropertyType::Hidden:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || commonValueSetted == FALSE)
			{
				bCommonValue = pbase->m_bHidden;
				commonValueSetted = TRUE;
			}

			else if (bCommonValue != pbase->m_bHidden) break;
		}

		if (bCommonValue)
			SetColoredState(CrsProp::State::Important);
		else
			SetColoredState(CrsProp::State::Normal);

		break;
	}

	}

	if (i == m_pMulSel->GetSize())
	{
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore

		if (bCommonValue)
			SetValue((_variant_t)_TB("True"));
		else
			SetValue((_variant_t)_TB("False"));
	}

	//altrimenti lascio la property vuota
	else SetValue((_variant_t)_T(""));
}

//================================CRSMulIntProp==================================
//-----------------------------------------------------------------------------
CRSMulIntProp::CRSMulIntProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description)
	:
	CrsProp(strName, 0, description),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(m_pMulSel);

	EnableSpinControl(TRUE, 0, 100);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
BOOL CRSMulIntProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int value = GetValue();
	ASSERT(value >= 0);

	switch (m_propType)
	{
	case PropertyType::ShadowSize:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			int* shadowSize = obj->GetShadowSize();
			if (shadowSize != NULL) *shadowSize = this->GetValue();
			//obj->Redraw();	//---------------------------------> pi� efficiente perch� ciclo una volta sola
		}

		break;
	}

	case PropertyType::BorderSize:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			int* borderSize = obj->GetBorderSize();
			if (borderSize != NULL) *borderSize = this->GetValue();
			//obj->Redraw();	//---------------------------------> pi� efficiente perch� ciclo una volta sola
		}

		break;
	}

	case PropertyType::HRatio:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_nHRatio = this->GetValue();
		}

		break;
	}

	case PropertyType::VRatio:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_nVRatio = this->GetValue();
		}

		break;
	}

	case PropertyType::Layer:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;
			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);
			pbase->m_nLayer = this->GetValue();
		}

		break;
	}
	}

	//update the multiselection
	m_pMulSel->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi pi� pulito

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSMulIntProp::UpdatePropertyValue()
{
	int i;
	int iCommonValue = -1;
	BOOL commonValueSetted = FALSE;

	switch (m_propType)
	{
	case PropertyType::ShadowSize:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			int* shadowSize = obj->GetShadowSize();

			if (shadowSize != NULL)
			{
				if (i == 0 || iCommonValue <0 )
					iCommonValue = *shadowSize;
				else if (iCommonValue != *shadowSize) break;
			}
		}

		break;
	}

	case PropertyType::BorderSize:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			int* borderSize = obj->GetBorderSize();

			if (borderSize != NULL)
			{
				if (i == 0 || iCommonValue <0)
					iCommonValue = *borderSize;
				else if (iCommonValue != *borderSize) break;
			}
		}

		break;
	}

	case PropertyType::HRatio:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || iCommonValue == FALSE)
			{
				iCommonValue = pbase->m_nHRatio;
				commonValueSetted = TRUE;
			}

			else if (iCommonValue != pbase->m_nHRatio) break;
		}

		break;
	}

	case PropertyType::VRatio:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || iCommonValue == FALSE)
			{
				iCommonValue = pbase->m_nVRatio;
				commonValueSetted = TRUE;
			}

			else if (iCommonValue != pbase->m_nVRatio) break;
		}

		break;
	}

	case PropertyType::Layer:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			//no base rect
			if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) continue;

			BaseRect* pbase = (BaseRect*)m_pMulSel->GetObjAt(i);

			if (i == 0 || iCommonValue == FALSE)
			{
				iCommonValue = pbase->m_nLayer;
				commonValueSetted = TRUE;
			}

			else if (iCommonValue != pbase->m_nLayer) break;
		}

		break;
	}
	}

	if (i == m_pMulSel->GetSize())
		SetValue(iCommonValue);
	
	//altrimenti lascio la property vuota
	else SetValue(-1);
}

//================================CRSColorProp=================================
//-----------------------------------------------------------------------------
CRSMulColorProp::CRSMulColorProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description)
	:
	CBCGPColorProp(strName, RGB(0,0,0), NULL, description),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(m_pMulSel);
	EnableOtherButton(_TB("Other..."));
	//EnableAutomaticButton(_TB("Default"), ::GetSysColor(COLOR_3DFACE)); //disabilitato il bottone "Default"
	//nella popup della palette dei colori, perch� settava il colore nero anche se era stato spcificato un colore di default diverso

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
BOOL CRSMulColorProp::OnEndEdit()
{
	/*if (!IsValueChanged())
		return __super::OnEndEdit();*/ //ottimizzazione scartata perch� non setta il colore nero

	BOOL baseOnEndEdit = __super::OnEndEdit();

	int i;

	switch (m_propType)
	{
	case PropertyType::Shadow:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* shadowColor = obj->GetShadowColor();
			if (shadowColor != NULL) *shadowColor = this->GetColor();
		}

		break;
	}

	case PropertyType::Border:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* borderColor = obj->GetBorderColor();
			if (borderColor != NULL) *borderColor = this->GetColor();
		}

		break;
	}

	}

	//update the multiselection
	m_pMulSel->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito

	return baseOnEndEdit;
}

//-----------------------------------------------------------------------------
void CRSMulColorProp::UpdatePropertyValue()
{
	int i;
	COLORREF cCommonColor = NULL;

	switch (m_propType)
	{
	case PropertyType::Shadow:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* shadowColor = obj->GetShadowColor();

			if (shadowColor != NULL)
			{
				if (i == 0 || cCommonColor == NULL)
					cCommonColor = *shadowColor;
				else if (cCommonColor != *shadowColor) break;
			}
		}

		break;
	}

	case PropertyType::Border:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* borderColor = obj->GetBorderColor();

			if (borderColor != NULL)
			{
				if (i == 0 || cCommonColor == NULL)
					cCommonColor = *borderColor;
				else if (cCommonColor != *borderColor) break;
			}
		}

		break;
	}
	}

	if (i == m_pMulSel->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetColor(cCommonColor);

	//altrimenti lascio il colore di default
	else SetColor(m_ColorAutomatic);
}

//================================CRSMulColorWithExprProp==================================
//-----------------------------------------------------------------------------
CRSMulColorWithExprProp::CRSMulColorWithExprProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CRSColorWithExprProp(pMulSel, strName, RGB(0, 0, 0), NULL, description, propertyView),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(m_pMulSel);

	UpdatePropertyValue();
}

CRSMulColorWithExprProp::~CRSMulColorWithExprProp()
{}

//-----------------------------------------------------------------------------
void CRSMulColorWithExprProp::UpdatePropertyValue()
{
	int i;
	COLORREF cCommonColor=NULL;
	ASSERT_VALID(m_pMulSel);
	switch (m_propType)
	{
	case PropertyType::ValueBackGroundColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* bkgColor = obj->GetBkgColor();
			
			if(bkgColor != NULL)
			{
				if (i == 0 || cCommonColor == NULL)
					cCommonColor = *bkgColor;
				else if (cCommonColor != *bkgColor) break;
			}
		}

		break;
	}

	case PropertyType::ValueForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* foreColor = obj->GetValueForeColor();
			if (foreColor != NULL)
			{
				if (i == 0 || cCommonColor == NULL)
					cCommonColor = *foreColor;
				else if (cCommonColor != *foreColor) break;
			}
		}

		break;
	}

	case PropertyType::LabelForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* foreColor = obj->GetLabelForeColor();
			if (foreColor != NULL)
			{
				if (i == 0 || cCommonColor == NULL)
					cCommonColor = *foreColor;
				else if (cCommonColor != *foreColor) break;
			}
		}

		break;
	}
	}

	if (i == m_pMulSel->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		SetColor(cCommonColor);

	//altrimenti lascio il colore di default
	else SetColor(m_ColorAutomatic);
}

//-----------------------------------------------------------------------------
BOOL CRSMulColorWithExprProp::OnEndEdit()
{
	if (!IsValueChanged())
		return __super::OnEndEdit();

	BOOL baseOnEndEdit = __super::OnEndEdit();
	if (!m_pMulSel)
		return baseOnEndEdit;

	int i;
	switch (m_propType)
	{
	case PropertyType::ValueBackGroundColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* bkgColor = obj->GetBkgColor();
			if (bkgColor!=NULL) *bkgColor = this->GetColor();
		}

		break;
	}

	case PropertyType::ValueForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* bkgColor = obj->GetValueForeColor();
			if (bkgColor != NULL) *bkgColor = this->GetColor();
		}

		break;
	}

	case PropertyType::LabelForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			COLORREF* bkgColor = obj->GetLabelForeColor();
			if (bkgColor != NULL) *bkgColor = this->GetColor();
		}

		break;
	}
	}

	//update the multiselection
	m_pMulSel->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito

	return baseOnEndEdit;
}

//-----------------------------------------------------------------------------
//return the commmon expression if there is, NULL otherwise
Expression* CRSMulColorWithExprProp::GetCommonExpression()
{
	if (!m_pMulSel)
		return NULL;

	int i;
	Expression* commonExp = NULL;
	switch (m_propType)
	{
	case PropertyType::ValueBackGroundColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				//retrieve the first object bkgcolor
				if (
					i == 0 && 
					pBase->m_pBkgColorExpr && 
					!pBase->m_pBkgColorExpr->IsEmpty()
					)
					commonExp = new Expression(*(pBase->m_pBkgColorExpr));
				else
					if (
						!commonExp || 
						commonExp->IsEmpty() || 
						!pBase->m_pBkgColorExpr || 
						pBase->m_pBkgColorExpr->IsEmpty() || 
						commonExp->ToString() != pBase->m_pBkgColorExpr->ToString()
						)
						return NULL;
			}
		}

		break;
	}

	case PropertyType::ValueForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				//retrieve the first object bkgcolor
				if (
					i == 0 && 
					pBase->m_pTextColorExpr && 
					!pBase->m_pTextColorExpr->IsEmpty()
					)
					commonExp = new Expression(*(pBase->m_pTextColorExpr));
				else
					if (
						!commonExp || 
						commonExp->IsEmpty() || 
						!pBase->m_pTextColorExpr || 
						pBase->m_pTextColorExpr->IsEmpty() || 
						commonExp->ToString() != pBase->m_pTextColorExpr->ToString()
						)
						return NULL;
			}
		}

		break;
	}

	case PropertyType::LabelForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				//retrieve the first object bkgcolor
				if (
					i == 0 &&
					pField-> m_pLabelTextColorExpr &&
					!pField->m_pLabelTextColorExpr->IsEmpty()
					)
					commonExp = new Expression(*(pField->m_pLabelTextColorExpr));
				else
					if (
						!commonExp ||
						commonExp->IsEmpty() ||
						!pField->m_pLabelTextColorExpr ||
						pField->m_pLabelTextColorExpr->IsEmpty() ||
						commonExp->ToString() != pField->m_pLabelTextColorExpr->ToString()
						)
						return NULL;
			}
		}

		break;
	}
	}

	if (!commonExp || commonExp->IsEmpty())
	{
		SAFE_DELETE(commonExp);
		return NULL;
	}

	return commonExp;
}

//-----------------------------------------------------------------------------
//return TRUE if at least one element of the multi selection have an expression associated to itself
BOOL CRSMulColorWithExprProp::HasExpression()
{
	int i;
	//todo andrea: vedere come sistemare la symtable
	// right - edit view
	ASSERT_VALID(m_pPropertyView);
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		FALSE;
	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	if (!m_pMulSel)
	{
		return FALSE;
	}
	ASSERT_VALID(m_pMulSel);
	if (!::IsWindow(m_pMulSel->m_hWnd))
		return FALSE;

	switch (m_propType)
	{
	case PropertyType::ValueBackGroundColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				
				if (pBase->m_pBkgColorExpr && !pBase->m_pBkgColorExpr->IsEmpty())
					return TRUE;
			}
		}

		break;
	}

	case PropertyType::ValueForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				
				if (pBase->m_pTextColorExpr && !pBase->m_pTextColorExpr->IsEmpty())
					return TRUE;
			}
		}

		break;
	}

	case PropertyType::LabelForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;

				if (pField->m_pLabelTextColorExpr && !pField->m_pLabelTextColorExpr->IsEmpty())
					return TRUE;
			}
		}

		break;
	}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSMulColorWithExprProp::SetCommonExpression(Expression * expr)
{
	if (!m_pMulSel)
		return;

	if (!expr)
		return;
	ASSERT_VALID(expr);

	int i;
	switch (m_propType)
	{
	case PropertyType::ValueBackGroundColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				//cancello la vecchia espressione
				SAFE_DELETE(pBase->m_pBkgColorExpr);
				//setto la nuova uguale a quella comnue
				if (!expr->IsEmpty())
					pBase->m_pBkgColorExpr = new Expression(*expr);
			}
		}
		break;
	}

	case PropertyType::ValueForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(BaseRect)))
			{
				BaseRect* pBase = (BaseRect*)obj;
				//cancello la vecchia espressione
				SAFE_DELETE(pBase->m_pTextColorExpr);
				//setto la nuova uguale a quella comnue
				if (!expr->IsEmpty())
					pBase->m_pTextColorExpr = new Expression(*expr);
			}
		}
		break;
	}

	case PropertyType::LabelForeColor:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				//cancello la vecchia espressione
				SAFE_DELETE(pField->m_pLabelTextColorExpr);
				//setto la nuova uguale a quella comnue
				if (!expr->IsEmpty())
					pField->m_pLabelTextColorExpr = new Expression(*expr);
			}
		}
		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSMulColorWithExprProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	ASSERT_VALID(this);

	//almeno un elemento della selezione ha un espressione associata e lo segnalo come un'immagine
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	}

	//se ho un'espressione associata comune a tutti gli oggetti, la scrivo nel footer
	Expression* commonExpr = GetCommonExpression();
	if (commonExpr != NULL)
	{
		ASSERT_VALID(commonExpr);
		CString expr = (commonExpr)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}

	//altrimenti lascio il footer originale
	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
void CRSMulColorWithExprProp::OnRightButtonClick()
{
	// open edit view dialog
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	//se ve ne � una presente, recupero l'espressione comune a tutti gli oggetti selezionati
	Expression* commonExpr = GetCommonExpression();

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		&commonExpr,
		DataType::Long,
		TRUE
		);
	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetCommonExpression(commonExpr);

	//delete temp expression
	delete commonExpr;

	//update the multiselection
	m_pMulSel->Redraw();
	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

//==================================================================
//-----------------------------------------------------------------------------
CRSMulHiddenProp::CRSMulHiddenProp(SelectionRect* pMulSel, const CString& strName, const CString& description, CRS_ObjectPropertyView* propertyView)
	:
	CRSMulBoolProp(pMulSel, _TB("Hidden"), CRSMulBoolProp::PropertyType::Hidden, _TB("Set if selected objects must be visible or not")),
	m_pPropertyView(propertyView)
{
	ASSERT_VALID(m_pMulSel);

	AllowEdit(FALSE);
	m_bHasState = TRUE;

	m_imageExpr.SetImageSize(CSize(14, 14));
	m_imageExpr.SetTransparentColor(RGB(255, 0, 255));
	globalUtils.ScaleByDPI(m_imageExpr);
	HICON icon = TBLoadPng(TBGlyph(szGlyphQuestionMark));
	m_imageExpr.AddIcon(icon);
	::DestroyIcon(icon);

	UpdatePropertyValue();

	SetState();
}

//-----------------------------------------------------------------------------
//return the commmon expression if there is, NULL otherwise
Expression* CRSMulHiddenProp::GetCommonExpression()
{
	if (!m_pMulSel)
		return NULL;

	int i;
	Expression* commonExp = NULL;

	for (i = 0; i < m_pMulSel->GetSize(); i++)
	{
		//no base rect
		if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect))) 
			continue;
		BaseRect* pBase = (BaseRect*)m_pMulSel->GetObjAt(i);
		//retrieve the first object hidden expression
		if (
			i == 0 &&
			pBase->m_pHideExpr &&
			!pBase->m_pHideExpr->IsEmpty()
			)
			commonExp = new Expression(*(pBase->m_pHideExpr));
		else
			if (
				!commonExp ||
				commonExp->IsEmpty() ||
				!pBase->m_pHideExpr ||
				pBase->m_pHideExpr->IsEmpty() ||
				commonExp->ToString() != pBase->m_pHideExpr->ToString()
				)
				return NULL;
	}

	if (!commonExp || commonExp->IsEmpty())
	{
		delete commonExp;
		return NULL;
	}

	else return commonExp;
}

//-----------------------------------------------------------------------------
//return TRUE if at least one element of the multi selection have an expression associated to itself
BOOL CRSMulHiddenProp::HasExpression()
{
	if (!m_pMulSel)
		return FALSE;

	int i;
	// right - edit view
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		FALSE;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	for (i = 0; i < m_pMulSel->GetSize(); i++)
	{
		//no base rect
		if (!m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect)))
			continue;
		BaseRect* pBase = (BaseRect*)m_pMulSel->GetObjAt(i);

		if (pBase->m_pHideExpr && !pBase->m_pHideExpr->IsEmpty())
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::SetCommonExpression(Expression * expr)
{
	if (!expr) return;
	int i;

	for (i = 0; i < m_pMulSel->GetSize(); i++)
	{
		if (m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBase = (BaseRect*)m_pMulSel->GetObjAt(i);
			//cancello la vecchia espressione
			SAFE_DELETE(pBase->m_pHideExpr);
			//setto la nuova uguale a quella comnue
			if (!expr->IsEmpty())
				pBase->m_pHideExpr = new Expression(*expr);
		}

		else if (m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = (Table*)m_pMulSel->GetObjAt(i);
			//cancello la vecchia espressione
			SAFE_DELETE(pTable->m_pHideExpr);
			//setto la nuova uguale a quella comnue
			if (!expr->IsEmpty())
				pTable->m_pHideExpr = new Expression(*expr);
		}
	}
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::SetState()
{
	BOOL shouldBeHidden = this->GetValue() == _TB("True") ? TRUE : FALSE;
	if (shouldBeHidden)
		SetColoredState(CrsProp::State::Important);
	else
		SetColoredState(CrsProp::State::Normal);
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	//almeno un elemento della selezione ha un espressione associata e lo segnalo come un'immagine
	if (HasExpression())
	{
		m_imageExpr.DrawEx(pDC, rect, 0,
			CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
	}

	//se ho un'espressione associata comune a tutti gli oggetti, la scrivo nel footer
	Expression* commonExpr = GetCommonExpression();
	if (commonExpr != NULL)
	{
		CString expr = (commonExpr)->ToString();
		SetDescription(FormatExprAndDescription(GetOriginalDescription(), expr));
	}
	//altrimenti lascio il footer originale
	else
		SetOriginalDescription();
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::OnClickButton(CPoint point)
{
	BOOL bIsLeft = point.x < m_rectButton.CenterPoint().x;

	if (bIsLeft)
	{
		// left - combobox
		CRSMulBoolProp::OnClickButton(point);
	}
	else
	{
		OnRightButtonClick();
		UpdatePropertyValue();
	}
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::OnRightButtonClick()
{
	// open edit view dialog
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();
	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	SymTable* m_psymTable = wrmDocMng->GetSymTable();

	//se ve ne � una presente, recupero l'espressione comune a tutti gli oggetti selezionati
	Expression* commonExpr = GetCommonExpression();

	CRSEditView* pEditView = m_pPropertyView->CreateEditView();
	if (!pEditView)
		return;

	pEditView->LoadElement(
		m_psymTable,
		&commonExpr,
		DataType::Bool,
		TRUE
		);
	pEditView->DoEvent();
	ASSERT_VALID(this);
	SetCommonExpression(commonExpr);

	//delete temp expression
	delete commonExpr;

	//update the multiselection
	m_pMulSel->Redraw();
	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();

	//aggiorno icona per la visibilit� sul tree
	GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pMulSel);
}

//-----------------------------------------------------------------------------
BOOL CRSMulHiddenProp::OnUpdateValue()
{
	//ottimizzazione:
	CString strOldValue = this->GetValue();

	BOOL baseUpdate = CrsProp::OnUpdateValue();

	CString strNewValue = this->GetValue();

	int index = -1;
	if (m_pWndList != NULL)
		index = GetSelectedOption();

	if (index <0 || index > 1)
		return baseUpdate;

	if (strOldValue != strNewValue) //solo se il valore della property � effettivamente cambiato
	{
		if (HasExpression())//era presente almeno un espressione, mi devo sincerare l'utente voglia cancellarla per settare una visibilit� non dinamica
		{
			if (AfxTBMessageBox(_TB("This change will erase the dynamic expression currently set. Are you sure you want to proceed?"), MB_ICONWARNING | MB_YESNO) == IDNO)
			{
				//risetto il precedente valore(l'espressione) 
				SetValue((_variant_t)strOldValue);
				return baseUpdate;
			}

			for (int i = 0; i < m_pMulSel->GetSize(); i++)
			{
				if (m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(BaseRect)))
				{
					BaseRect* pBase = (BaseRect*)m_pMulSel->GetObjAt(i);
					//cancello la vecchia espressione
					SAFE_DELETE(pBase->m_pHideExpr);
					pBase->m_pHideExpr = NULL;
				}

				else if (m_pMulSel->GetObjAt(i)->IsKindOf(RUNTIME_CLASS(Table)))
				{
					Table* pTable = (Table*)m_pMulSel->GetObjAt(i);
					//cancello la vecchia espressione
					SAFE_DELETE(pTable->m_pHideExpr);
					pTable->m_pHideExpr = NULL;
				}
			}
		}

		UpdateValue();

		UpdatePropertyValue();
		m_pMulSel->Redraw();
		RedrawState();
		GetDocument()->UpdateRSTreeNode(ERefreshEditor::Layouts, m_pMulSel);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::UpdatePropertyValue()
{
	CRSMulBoolProp::UpdatePropertyValue();

	Expression* pExp = GetCommonExpression();
	if (pExp && !pExp->IsEmpty())
	{
		CString expr = pExp->ToString();
		SetValue((_variant_t)expr);
	}

	else if(HasExpression())
		//altrimenti lascio la property vuota
		SetValue((_variant_t)_T(""));
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::AdjustButtonRect()
{
	CBCGPProp::AdjustButtonRect();

	if (m_dwFlags & PROP_HAS_LIST)
	{
		m_rectButton.left -= m_rectButton.Width();
	}
}

//-----------------------------------------------------------------------------
void CRSMulHiddenProp::OnDrawButton(CDC* pDC, CRect rectButton)
{
	for (int i = 0; i < 2; i++)
	{
		CRect rect = rectButton;

		if (i == 0)
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.right = rect.left + rect.Width() / 2;

				// Draw combobox button at left
				CBCGPProp::OnDrawButton(pDC, rect);
			}
		}

		else
		{
			if (m_dwFlags & PROP_HAS_LIST)
			{
				rect.left = rect.right - rect.Width() / 2;
			}

			// Draw push button at right
			COLORREF clrText = CBCGPVisualManager::GetInstance()->OnDrawPropListPushButton(pDC, rect, this, m_pWndList->DrawControlBarColors(), m_bButtonIsFocused, m_bEnabled, m_bButtonIsDown, m_bButtonIsHighlighted);

			CString str = m_strButtonText;
			if (!str.IsEmpty())
			{
				COLORREF clrTextOld = pDC->SetTextColor(clrText);
				rect.DeflateRect(2, 2);
				rect.bottom--;
				pDC->DrawText(str, rect, DT_SINGLELINE | DT_CENTER | DT_VCENTER);
				pDC->SetTextColor(clrTextOld);
			}
		}
	}
}

//================================CRSMulRectProp==================================
//-----------------------------------------------------------------------------
CRSMulRectProp::CRSMulRectProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description, int fromValue, int toValue)
	:
	CrsProp(strName, (LONG)0, description),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(pMulSel);
	
	EnableSpinControl(TRUE, fromValue, toValue);

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulRectProp::UpdatePropertyValue()
{
	int i;
	//LONG iCommonValue	= -1;	
	LONG minValue		= -1;
	CBCGPProp*  parent = GetParent();
	switch (m_propType)
	{
	case PropertyType::AlignToXP:
	case PropertyType::LocationXP:
	{
		
		SetValue((LONG)m_pMulSel->Left());
		if (parent)
		{
		   LONG val = (LONG)floor(LPtoMU(m_pMulSel->Left(), CM, 10., 3)); 
		   parent->GetSubItem(2)->SetValue(val);
		}
			
		break;
	}

	case PropertyType::AlignToYP:
	case PropertyType::LocationYP:
	{
			
		SetValue((LONG)m_pMulSel->Top());
		if (parent)
		{
		   LONG val = (LONG)floor(LPtoMU(m_pMulSel->Top(), CM, 10., 3));
		   parent->GetSubItem(3)->SetValue(val);
		}
			
		break;
	}

	case PropertyType::AlignToXM:
	case PropertyType::LocationXM:
	{
		LONG val = (LONG)MUtoLP(m_pMulSel->Left(), CM, 10., 0);
		SetValue(val);
		if (parent)
			parent->GetSubItem(0)->SetValue(m_pMulSel->Left());
		break;
	}

	case PropertyType::AlignToYM:
	case PropertyType::LocationYM:
	{
		LONG val = (LONG)MUtoLP(m_pMulSel->Top(), CM, 10., 0);
		SetValue(val);
		if (parent)
			parent->GetSubItem(1)->SetValue(m_pMulSel->Top());
		break;
	}

	case PropertyType::WidthP:
	{
		//SetValue((LONG)m_pMulSel->Width());
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			LONG width = obj->GetBaseRect().right - obj->GetBaseRect().left;

			if (i == 0 || minValue <0)
				minValue = width;
			else if (width < minValue) minValue = width;
		}
		
		SetValue(minValue);
	
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU(minValue, CM, 10., 3));
			parent->GetSubItem(2)->SetValue(val);
		}

		break;
	}

	case PropertyType::HeightP:
	{
		//SetValue((LONG)m_pMulSel->Height());
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			LONG height = obj->GetBaseRect().bottom - obj->GetBaseRect().top;

			if (i == 0 || minValue <0)
				minValue = height;
			else if (height < minValue) minValue = height;
		}

		SetValue(minValue);
		
		if (parent)
		{
			LONG val = (LONG)floor(LPtoMU(minValue, CM, 10., 3));
			parent->GetSubItem(3)->SetValue(val);
		}
			
		break;
	}

	case PropertyType::WidthM:
	{
		//SetValue((LONG)m_pMulSel->Width());
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			LONG width = obj->GetBaseRect().right - obj->GetBaseRect().left;

			if (i == 0 || minValue <0)
				minValue = width;
			else if (width < minValue) minValue = width;
		}

		LONG val = (LONG)MUtoLP(minValue, CM, 10., 0);
		SetValue(val);

		if (parent)
			parent->GetSubItem(0)->SetValue(minValue);
		break;
	}

	case PropertyType::HeightM:
	{
		//SetValue((LONG)m_pMulSel->Height());
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);

			LONG height = obj->GetBaseRect().bottom - obj->GetBaseRect().top;

			if (i == 0 || minValue <0)
				minValue = height;
			else if (height < minValue) minValue = height;
		}

		LONG val = (LONG)MUtoLP(minValue, CM, 10., 0);
		SetValue(val);

		if (parent)
			parent->GetSubItem(1)->SetValue(minValue);
		break;
	}
	}
}

//-----------------------------------------------------------------------------
//Updates the object values associated to this property
void CRSMulRectProp::UpdateObjectValue(LONG previousValue/* = 0*/)
{
	//se l'utente non ha modificato la property, esco, altrimenti sull'alignment, comunque viene settata
	if ((LONG)this->GetValue() == previousValue)
		return;
	CBCGPProp* father = GetParent();
	switch (m_propType)
	{
	case CRSMulRectProp::PropertyType::AlignToXP:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			CSize offset((LONG)this->GetValue() - obj->Left(), 0);
			obj->MoveObject(offset);
		}

		// reconstruct baseRect for multiple selected object and active it                                    
		m_pMulSel->BuildBaseRect();
		m_pMulSel->GetActiveWindow()->UpdateWindow();
		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(2)->SetValue(propinmm);
		break;
	}

	case CRSMulRectProp::PropertyType::AlignToXM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		father->GetSubItem(0)->SetValue(propinpix);

		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			CSize offset(propinpix - obj->Left(), 0);
			obj->MoveObject(offset);
		}

		// reconstruct baseRect for multiple selected object and active it                                    
		m_pMulSel->BuildBaseRect();
		m_pMulSel->GetActiveWindow()->UpdateWindow();
	
		break;
	}

	case CRSMulRectProp::PropertyType::AlignToYP:
	{
		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			CSize offset(0, (LONG)this->GetValue() - obj->Top());
			obj->MoveObject(offset);
		}

		// reconstruct baseRect for multiple selected object and active it                                    
		m_pMulSel->BuildBaseRect();
		m_pMulSel->GetActiveWindow()->UpdateWindow();
		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(3)->SetValue(propinmm);
		break;
	}

	case CRSMulRectProp::PropertyType::AlignToYM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		father->GetSubItem(1)->SetValue(propinpix);

		for (int i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			CSize offset(0, propinpix - obj->Top());
			obj->MoveObject(offset);
		}

		// reconstruct baseRect for multiple selected object and active it                                    
		m_pMulSel->BuildBaseRect();
		m_pMulSel->GetActiveWindow()->UpdateWindow();
		break;
	}

	case CRSMulRectProp::PropertyType::LocationXP:
	{
		CSize offset((LONG)this->GetValue() - m_pMulSel->Left(), 0);
		m_pMulSel->MoveMultipleSelObjects(offset);

		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(2)->SetValue(propinmm);
		break;
	}

	case CRSMulRectProp::PropertyType::LocationYP:
	{
		CSize offset(0, (LONG)this->GetValue() - m_pMulSel->Top());
		m_pMulSel->MoveMultipleSelObjects(offset);

		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(3)->SetValue(propinmm);
		break;
	}

	case CRSMulRectProp::PropertyType::LocationXM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		if (father)
			father->GetSubItem(0)->SetValue(propinpix);

		CSize offset(propinpix - m_pMulSel->Left(), 0);
		m_pMulSel->MoveMultipleSelObjects(offset);
		
		break;
	}

	case CRSMulRectProp::PropertyType::LocationYM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		if (father)
			father->GetSubItem(1)->SetValue(propinpix);

		CSize offset(0, (LONG)propinpix - m_pMulSel->Top());
		m_pMulSel->MoveMultipleSelObjects(offset);
		
		break;
	}

	case CRSMulRectProp::PropertyType::WidthP:
	{
		m_pMulSel->SizeHValue((LONG)this->GetValue());
		m_pMulSel->BuildBaseRect();

		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(2)->SetValue(propinmm);

		break;
	}

	case CRSMulRectProp::PropertyType::HeightP:
	{
		m_pMulSel->SizeVValue((LONG)this->GetValue());
		m_pMulSel->BuildBaseRect();

		LONG propinmm = (LONG)MUtoLP((int)GetValue(), CM, 10., 3);
		if (father)
			father->GetSubItem(3)->SetValue(propinmm);

		break;
	}

	case CRSMulRectProp::PropertyType::WidthM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		if (father)
			father->GetSubItem(0)->SetValue(propinpix);

		m_pMulSel->SizeHValue(propinpix);
		m_pMulSel->BuildBaseRect();

		break;
	}

	case CRSMulRectProp::PropertyType::HeightM:
	{
		LONG propinpix = (LONG)MUtoLP((int)GetValue() * 10, CM, MU_SCALE, MU_DECIMAL);
		if (father)
			father->GetSubItem(1)->SetValue(propinpix);

		m_pMulSel->SizeVValue(propinpix);
		m_pMulSel->BuildBaseRect();

		break;
	}

	default:
		ASSERT(FALSE);
	}

	//update the multiselection
	m_pMulSel->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi pi� pulito
}

//-----------------------------------------------------------------------------
BOOL CRSMulRectProp::OnUpdateValue()
{
	LONG previousValue = (LONG)this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	UpdateObjectValue(previousValue);

	return baseUpdate;
}

//================================CRSMulFontStyleProp==================================
//-----------------------------------------------------------------------------
CRSMulFontStyleProp::CRSMulFontStyleProp(SelectionRect* pMulSel, PropertyType propType)
	:
	CrsProp(L"", L"", L""),
	m_pMulSel(pMulSel),
	m_propType(propType)
{
	ASSERT_VALID(pMulSel);
	m_commonFont = NULL;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		SetName(_TB("Label FontStyle"));
		SetDescription(_TB("It opens the dialog and allows you to set the font of the label to all objects in the multi-selection that allow it"));
		break;
	}

	case PropertyType::Value:
	{
		SetName(_TB("Value FontStyle"));
		SetDescription(_TB("It opens the dialog and allows you to set the font of the Value to all FieldRects in the multi-selection"));
		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//metodo per recuperare il font name dello style
CString CRSMulFontStyleProp::GetFontName(FontIdx fontIdx)
{
	if (!fontIdx)
	{
		ASSERT(FALSE);
		return NULL;
	}

	else return m_pMulSel->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulFontStyleProp::UpdatePropertyValue()
{
	int i=0;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			if (i == 0 || !m_commonFont)
				m_commonFont = obj->GetCaptionFontIdx();
			else if (m_commonFont != obj->GetCaptionFontIdx())
				break;
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			if (i == 0 || !m_commonFont)
				m_commonFont = obj->GetValueFontIdx();
			else if (m_commonFont != obj->GetValueFontIdx())
				break;
		}

		break;
	}
	}

	if (i == m_pMulSel->GetSize() && m_commonFont)
		SetValue((_variant_t)GetFontName(m_commonFont));
	else
		SetValue(_T(""));
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulFontStyleProp::OnClickButton(CPoint point)
{
	// construct font dialogs
	CFontStylesDlg dialog(*(m_pMulSel->m_pDocument->m_pFontStyles), m_commonFont, FALSE, NULL, m_pMulSel->m_pDocument->GetNamespace(), FALSE, m_pMulSel->m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pMulSel->m_pDocument->IsModified())
			m_pMulSel->m_pDocument->SetModifiedFlag(m_pMulSel->m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	int i;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			obj->SetCaptionFontIdx(m_commonFont);
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			obj->SetValueFontIdx(m_commonFont);
		}

		break;
	}
	}

	m_pMulSel->Redraw();
	UpdatePropertyValue();//settare direttamente al valore appena impostato?-> sarebbe pi� efficiente perch� non deve ciclare nuovamente
}

//================================CRSMultiColumnFontStyleProp==================================
//-----------------------------------------------------------------------------
CRSMultiColumnFontStyleProp::CRSMultiColumnFontStyleProp(MultiColumnSelection* pColumns, PropertyType propType)
	:
	CrsProp(L"", L"", L""),
	m_pColumns(pColumns),
	m_propType(propType)
{
	m_commonFont = NULL;

	SetName(_TB("FontStyle"));

	switch (m_propType)
	{
	case PropertyType::Title:
		SetDescription(_TB("It opens the dialog and allows you to set the font of titles of selected columns"));
		break;
	case PropertyType::Body:
		SetDescription(_TB("It opens the dialog and allows you to set the font of bodies of selected columns"));
		break;
	case PropertyType::Total:
		SetDescription(_TB("It opens the dialog and allows you to set the font of totals of selected columns"));
		break;
	case PropertyType::Subtotal:
		SetDescription(_TB("It opens the dialog and allows you to set the font of subtotals of selected columns"));
		break;
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//metodo per recuperare il font name dello style
CString CRSMultiColumnFontStyleProp::GetFontName(FontIdx fontIdx)
{
	if (!fontIdx)
	{
		ASSERT(FALSE);
		return NULL;
	}

	else return m_pColumns->m_pDocument->m_pFontStyles->GetStyleName(fontIdx);
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMultiColumnFontStyleProp::UpdatePropertyValue()
{
	int i;
	FontIdx	singleFont = NULL;
	TableColumn* pCol;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_propType)
		{
		case PropertyType::Title:
			singleFont =pCol->GetColumnTitleFontIdx();
			break;
		case PropertyType::Body:
			singleFont = pCol->GetColumnFontIdx();
			break;
		case PropertyType::Total:
			singleFont = pCol->GetTotalFontIdx();
			break;
		case PropertyType::Subtotal:
			singleFont = pCol->GetSubTotalFontIdx();
			break;
		}

		if (singleFont||NULL && ( i == 0 || m_commonFont||NULL) )
			m_commonFont = singleFont;
		else if (m_commonFont != singleFont) break;
	}
	
	if (i == m_pColumns->GetSize() && m_commonFont)
		SetValue((_variant_t)GetFontName(m_commonFont));
	else
		SetValue(_T(""));
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMultiColumnFontStyleProp::OnClickButton(CPoint point)
{
	// construct font dialogs
	CFontStylesDlg dialog(*(m_pColumns->m_pDocument->m_pFontStyles), m_commonFont, FALSE, NULL, m_pColumns->m_pDocument->GetNamespace(), FALSE, m_pColumns->m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pColumns->m_pDocument->IsModified())
			m_pColumns->m_pDocument->SetModifiedFlag(m_pColumns->m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	int i;
	TableColumn* pCol;

	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_propType)
		{
		case PropertyType::Title:
			pCol->SetColumnTitleFontIdx(m_commonFont);
			break;
		case PropertyType::Body:
			pCol->SetColumnFontIdx(m_commonFont);
			break;
		case PropertyType::Total:
			pCol->SetTotalFontIdx(m_commonFont);
			break;
		case PropertyType::Subtotal:
			pCol->SetSubTotalFontIdx(m_commonFont);
			break;
		}
	}

	m_pColumns->Redraw();
	UpdatePropertyValue();//settare direttamente al valore appena impostato?-> sarebbe pi� efficiente perch� non deve ciclare nuovamente
}

//================================CRSMulAlignmentStyleProp==================================
//-----------------------------------------------------------------------------
CRSMulAlignmentStyleProp::CRSMulAlignmentStyleProp(SelectionRect* pMulSel, PropertyType propType)
	:
	CrsProp(L"", (_variant_t)0, L""),
	m_pMulSel(pMulSel),
	m_propType(propType),
	m_commonAlignType(DEFAULT_ALIGN)
{
	ASSERT_VALID(pMulSel);
	m_commonAlignType = 0;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		SetName(_TB("Label Alignment"));
		SetDescription(_TB("It opens the dialog and allows you to set the alignment of the label to all objects in the multi-selection that allow it"));
		break;
	}

	case PropertyType::Value:
	{
		SetName(_TB("Value Alignment"));
		SetDescription(_TB("It opens the dialog and allows you to set the alignment of the Value to all FieldRects in the multi-selection"));
		break;
	}
	}

	UpdatePropertyValue();
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulAlignmentStyleProp::UpdatePropertyValue()
{
	int i;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FielRect Label
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pField->m_Label.GetAlign();
				else if (m_commonAlignType != pField->m_Label.GetAlign())
					break;
			}
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FieldRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pField->m_Value.GetAlign();
				else if (m_commonAlignType != pField->m_Value.GetAlign())
					break;
			}

			//TextRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				TextRect* pText = (TextRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pText->m_StaticText.GetAlign();
				else if (m_commonAlignType != pText->m_StaticText.GetAlign())
					break;
			}
		}

		break;
	}
	}

	if (i == m_pMulSel->GetSize() && m_commonAlignType != 0)
		SetValue((int)m_commonAlignType);
	else
		SetValue(0);
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulAlignmentStyleProp::OnClickButton(CPoint point)
{
	CAlignDlg   dialog(m_commonAlignType);
	dialog.SetAllowFieldSet(TRUE);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pMulSel->m_pDocument->IsModified())
			m_pMulSel->m_pDocument->SetModifiedFlag(m_pMulSel->m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	int i;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FielRect Label
			if (!obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
				continue;
			FieldRect* pField = (FieldRect*)obj;
			pField->m_Label.SetAlign(m_commonAlignType);
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FieldRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				pField->m_Value.SetAlign(m_commonAlignType);
			}

			//TextRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				TextRect* pText = (TextRect*)obj;
				pText->m_StaticText.SetAlign(m_commonAlignType);
			}
		}

		break;
	}
	}

	m_pMulSel->Redraw();

	UpdatePropertyValue();//settare direttamente al valore appena impostato?-> sarebbe pi� efficiente perch� non deve ciclare nuovamente
}

//================================CRSMulAlignmentStyleBitWiseProp==================================
//-----------------------------------------------------------------------------
CRSMulAlignmentStyleBitWiseProp::CRSMulAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, SelectionRect* pMulSel, PropertyType propType)
	:
	CRSAlignBitwiseProp(PropertyView, pMulSel),
	m_pMulSel(pMulSel),
	m_propType(propType),
	m_commonAlignType(DEFAULT_ALIGN)
{
	ASSERT_VALID(pMulSel);
	//m_commonAlignType = 0;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		SetName(_TB("Label Alignment"));
		SetDescription(_TB("Label's alignment prop"));
		m_bAllowFieldSet = TRUE;
		break;
	}

	case PropertyType::Value:
	{
		SetName(_TB("Value Alignment"));
		SetDescription(_TB("Value's alignment prop"));
		m_bAllowCenterBottom = TRUE;
		break;
	}
	}

	UpdatePropertyValue();				//seach common alignment type

	SetAlignType(&m_commonAlignType);	//passo il puntatore al common alignment type alla property base da cui derivo

	AlignTypeToProps();					//aggiorno il valore dei data member usati per le property figlie

	Rebuild();							//vado a creare le property figlie
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMulAlignmentStyleBitWiseProp::UpdatePropertyValue()
{
	int i;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FielRect Label
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pField->m_Label.GetAlign();
				else if (m_commonAlignType != pField->m_Label.GetAlign())//todo andrea: vedere se ha senso
					break;
			}
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FieldRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pField->m_Value.GetAlign();
				else if (m_commonAlignType != pField->m_Value.GetAlign())//todo andrea: vedere se ha senso
					break;
			}

			//TextRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				TextRect* pText = (TextRect*)obj;
				if (i == 0 || m_commonAlignType == 0)
					m_commonAlignType = pText->m_StaticText.GetAlign();
				else if (m_commonAlignType != pText->m_StaticText.GetAlign())//todo andrea: vedere se ha senso
					break;
			}
		}

		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSMulAlignmentStyleBitWiseProp::UpdateSelectedObject()
{
	int i;

	switch (m_propType)
	{
	case PropertyType::Label:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FielRect Label
			if (!obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
				continue;
			FieldRect* pField = (FieldRect*)obj;
			pField->m_Label.SetAlign(m_commonAlignType);
		}

		break;
	}

	case PropertyType::Value:
	{
		for (i = 0; i < m_pMulSel->GetSize(); i++)
		{
			BaseObj* obj = m_pMulSel->GetObjAt(i);
			//FieldRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRect* pField = (FieldRect*)obj;
				pField->m_Value.SetAlign(m_commonAlignType);
			}

			//TextRect Value
			if (obj->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				TextRect* pText = (TextRect*)obj;
				pText->m_StaticText.SetAlign(m_commonAlignType);
			}
		}

		break;
	}
	}

	m_pMulSel->Redraw();
}

//================================CRSAllColumnsAlignmentStyleBitWiseProp==================================
//-----------------------------------------------------------------------------
CRSAllColumnsAlignmentStyleBitWiseProp::CRSAllColumnsAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, Table* pTable, PropertyType propType)
	:
	CRSAlignBitwiseProp(PropertyView, pTable),
	m_pTable(pTable),
	m_propType(propType),
	m_allColumnsAlignType(DEFAULT_ALIGN)
{
	ASSERT_VALID(m_pTable);

	SetName(_TB("Alignment"));
	m_bAllowFieldSet = FALSE;
	m_bAllowCenterBottom = FALSE;

	switch (m_propType)
	{
	case PropertyType::ColumnTitles:
	{
		SetDescription(_TB("All title's alignment prop"));
		break;
	}

	case PropertyType::Body:
	{
		SetDescription(_TB("All body's alignment prop"));
		break;
	}

	case PropertyType::Totals:
	{
		SetDescription(_TB("All totals's alignment prop"));
		break;
	}
	}

	UpdatePropertyValue();					//seach common alignment type

	SetAlignType(&m_allColumnsAlignType);	//passo il puntatore al common alignment type alla property base da cui derivo

	AlignTypeToProps();						//aggiorno il valore dei data member usati per le property figlie

	Rebuild();								//vado a creare le property figlie
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSAllColumnsAlignmentStyleBitWiseProp::UpdatePropertyValue()
{
	int i;

	switch (m_propType)
	{
	case PropertyType::ColumnTitles:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			if (i == 0 || m_allColumnsAlignType == 0)
				m_allColumnsAlignType = m_pTable->GetColumnTitleAlign(i);
			else if (m_allColumnsAlignType != m_pTable->GetColumnTitleAlign(i))//todo andrea: vedere se ha senso
				break;
		}

		break;
	}

	case PropertyType::Body:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			if (i == 0 || m_allColumnsAlignType == 0)
				m_allColumnsAlignType = m_pTable->GetColumnAlign(i);
			else if (m_allColumnsAlignType != m_pTable->GetColumnAlign(i))		//todo andrea: vedere se ha senso
				break;
		}

		break;
	}

	case PropertyType::Totals:
	{
		for (i = 0; i < m_pTable->m_Columns.GetSize(); i++)
		{
			if (i == 0 || m_allColumnsAlignType == 0)
				m_allColumnsAlignType = m_pTable->GetTotalAlign(i);
			else if (m_allColumnsAlignType != m_pTable->GetTotalAlign(i))		//todo andrea: vedere se ha senso
				break;
		}

		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSAllColumnsAlignmentStyleBitWiseProp::UpdateSelectedObject()
{
	switch (m_propType)
	{
	case PropertyType::ColumnTitles:
		m_pTable->SetAllColumnsTitleAlign(m_allColumnsAlignType);
		break;
	case PropertyType::Body:
		m_pTable->SetAllColumnsAlign(m_allColumnsAlignType);
		break;
	case PropertyType::Totals:
		m_pTable->SetAllTotalsAlign(m_allColumnsAlignType);
		break;
	}

	m_pTable->Redraw();
}

//================================CRSMultiColumnsAlignmentStyleBitWiseProp==================================
//-----------------------------------------------------------------------------
CRSMultiColumnsAlignmentStyleBitWiseProp::CRSMultiColumnsAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, MultiColumnSelection* pMulCol, PropertyType propType)
	:
	CRSAlignBitwiseProp(PropertyView, pMulCol),
	m_pColumns(pMulCol),
	m_propType(propType),
	m_allColumnsAlignType(DEFAULT_ALIGN)
{
	SetName(_TB("Alignment"));
	m_bAllowFieldSet = FALSE;
	m_bAllowCenterBottom = FALSE;

	switch (m_propType)
	{
	case PropertyType::ColumnTitles:
	{
		SetDescription(_TB("All title's alignment prop"));
		break;
	}

	case PropertyType::Body:
	{
		SetDescription(_TB("All body's alignment prop"));
		break;
	}

	case PropertyType::Totals:
	{
		SetDescription(_TB("All totals's alignment prop"));
		break;
	}
	}

	UpdatePropertyValue();					//seach common alignment type

	SetAlignType(&m_allColumnsAlignType);	//passo il puntatore al common alignment type alla property base da cui derivo

	AlignTypeToProps();						//aggiorno il valore dei data member usati per le property figlie

	Rebuild();								//vado a creare le property figlie
}

//-----------------------------------------------------------------------------
//Updates the property grid value associated with the current property
void CRSMultiColumnsAlignmentStyleBitWiseProp::UpdatePropertyValue()
{
	int i;
	TableColumn* pCol;
	AlignType alignType = NULL;
	for (i = 0; i < m_pColumns->GetSize(); i++)
	{
		pCol = m_pColumns->GetAt(i);

		switch (m_propType)
		{
		case PropertyType::ColumnTitles:
			alignType = pCol->GetColumnTitleAlign();
			break;
		case PropertyType::Body:
			alignType = pCol->GetColumnAlign();
			break;
		case PropertyType::Totals:
			alignType = pCol->GetTotalAlign();
			break;
		}

		if (alignType != NULL)
		{
			if (i == 0 || m_allColumnsAlignType == DEFAULT_ALIGN)
				m_allColumnsAlignType = alignType;
			else if (m_allColumnsAlignType != alignType) break;
		}
	}

	if (i == m_pColumns->GetSize())
		//li ho scorsi tutti, quindi sono tutti uguali e posso settare la property ad un valore
		m_allColumnsAlignType = alignType;

	//altrimenti lascio il colore di default
	else m_allColumnsAlignType = DEFAULT_ALIGN;
}

//-----------------------------------------------------------------------------
void CRSMultiColumnsAlignmentStyleBitWiseProp::UpdateSelectedObject()
{
	ASSERT_VALID(m_pColumns);
	for (int i = 0; i < m_pColumns->GetSize(); i++)
	{
		TableColumn* pCol = m_pColumns->GetAt(i);
		ASSERT_VALID(pCol);

		switch (m_propType)
		{
		case PropertyType::ColumnTitles:
		{
			pCol->SetColumnTitleAlign(m_allColumnsAlignType);
			break;
		}

		case PropertyType::Body:
		{
			pCol->SetColumnAlign(m_allColumnsAlignType);
			break;
		}

		case PropertyType::Totals:
		{
			pCol->SetTotalAlign(m_allColumnsAlignType);
			break;
		}
		}
	}

	//update the multiselection
	m_pColumns->Redraw();		//---------------------------------> una volta sola per tutti gli switch, quindi codice pi� pulito
}

//================================CRSAlignBitwiseProp==================================
//-----------------------------------------------------------------------------
CRSAlignBitwiseProp::CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, CObject* pOwner, const CString& strName, AlignType* pAlignType, 
	BOOL bAllowVertical, BOOL bAllowCenterBottom, BOOL bAllowFieldSet,
	DWORD_PTR dwData, LPCTSTR lpszDescr,
	BOOL bAllowWordBreak, BOOL bAllowLineProp, BOOL bAllowPrefixSelectionProp, BOOL bAllowExpandTabProp)
	:
	CrsProp(strName, _variant_t(0L), lpszDescr, dwData),
	m_pPropertyView(PropertyView),
	m_pOwner(pOwner),
	m_pAlignType(pAlignType),

	m_bAllowVertical(bAllowVertical),
	m_bAllowCenterBottom(bAllowCenterBottom),
	m_bAllowFieldSet(bAllowFieldSet),

	m_bAllowWordBreak(bAllowWordBreak),
	m_bAllowLineProp(bAllowLineProp),
	m_bAllowPrefixSelectionProp(bAllowPrefixSelectionProp),
	m_bAllowExpandTabProp(bAllowExpandTabProp),

	m_pColumns(NULL)
{
	AlignTypeToProps();

	Rebuild();
}

//-----------------------------------------------------------------------------
CRSAlignBitwiseProp::CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, CObject* pOwner)
	:
	CrsProp(L"", _variant_t(0L), L"", 0),
	m_pPropertyView(PropertyView),
	m_pOwner(pOwner),

	m_bAllowVertical(TRUE),
	m_bAllowCenterBottom(FALSE),
	m_bAllowFieldSet(FALSE),

	m_bAllowWordBreak(TRUE),
	m_bAllowLineProp(TRUE),
	m_bAllowPrefixSelectionProp(TRUE),
	m_bAllowExpandTabProp(TRUE),

	m_pColumns(NULL)
{
}

//-----------------------------------------------------------------------------
CRSAlignBitwiseProp::CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, MultiColumnSelection* pMulCol)
	:
	CrsProp(L"", _variant_t(0L), L"", 0),
	m_pPropertyView(PropertyView),
	m_pOwner(NULL),
	m_bAllowVertical(TRUE),
	m_bAllowCenterBottom(FALSE),
	m_bAllowFieldSet(FALSE),

	m_bAllowWordBreak(TRUE),
	m_bAllowLineProp(TRUE),
	m_bAllowPrefixSelectionProp(TRUE),
	m_bAllowExpandTabProp(TRUE),

	m_pColumns(pMulCol)
{
}

//-----------------------------------------------------------------------------
CRSAlignBitwiseProp::~CRSAlignBitwiseProp()
{
}

//-----------------------------------------------------------------------------
void CRSAlignBitwiseProp::Rebuild()
{
	m_bGroup = TRUE;
	m_bGroupHasValue = TRUE;
	m_bIsValueList = TRUE;
	m_bAllowEdit = FALSE;

	RemoveAllSubItems();

	//Horizontal align
	m_HorizontalAlignProp = new CRSBitProp(this, _TB("Horizontal Align"), (_variant_t)GetHorizontalAlignString());
	m_HorizontalAlignProp->AddOption(_TB("Left"), TRUE, 0);										//default 0
	m_HorizontalAlignProp->AddOption(_TB("Centered"), TRUE, DT_CENTER);							//DT_CENTER
	m_HorizontalAlignProp->AddOption(_TB("Right"), TRUE, DT_RIGHT);								//DT_RIGHT
	AddSubItem(m_HorizontalAlignProp);
	
	//Vertical align
	m_VerticalAlignProp = new CRSBitProp(this, _TB("Vertical Align"), (_variant_t)GetVerticalAlignString());
		m_VerticalAlignProp->AddOption(_TB("Top"), TRUE, 0);										//default 0
		
		m_VerticalAlignProp->AddOption(_TB("Centered"), TRUE, DT_VCENTER);							//DT_VCENTER				ma devo controllare anche m_bAllowCenterBottom
		
		if (m_bAllowCenterBottom &&
			m_pPropertyView->m_pDocument &&
			((CWoormDocMng*)m_pPropertyView->m_pDocument)->m_pWoormIni &&
			!((CWoormDocMng*)m_pPropertyView->m_pDocument)->m_pWoormIni->m_bForceVerticalAlignLabelRelative)
			m_VerticalAlignProp->AddOption(_TB("Centered relative to Label"), TRUE, DT_EX_VCENTER_LABEL);		

		m_VerticalAlignProp->AddOption(_TB("Bottom"), TRUE, DT_BOTTOM);								//DT_BOTTOM					ma devo controllare anche m_bAllowCenterBottom
		if (m_bAllowFieldSet)
			m_VerticalAlignProp->AddOption(_TB("Field Set"), TRUE, DT_EX_FIELD_SET);				//DT_EX_FIELD_SET
	AddSubItem(m_VerticalAlignProp);

	//line
	if (m_bAllowLineProp)
	{
	//wordBreak
		if (m_bAllowWordBreak)
		{																							//DT_WORDBREAK
			if (m_pOwner)
				m_WordBreakProp = new CRSBitBoolProp(this, m_pOwner, _TB("Word Break"), &m_bWordBreak);
			else if (m_pColumns)
				m_WordBreakProp = new CRSBitBoolProp(this, m_pColumns, _TB("Word Break"), &m_bWordBreak);	
		}
	
		m_SingleLineProp = new CRSBitSingleLineProp(this, m_WordBreakProp, _TB("Line"), (_variant_t)GetSingleLineString());
		m_SingleLineProp->AddOption(_TB("Single Line"), TRUE, DT_SINGLELINE);						//DT_SINGLELINE
		m_SingleLineProp->AddOption(_TB("Multi Line"), TRUE, 0);									//default 0
		AddSubItem(m_SingleLineProp);
		m_SingleLineProp->SetOwnerList(m_pPropertyView->m_pPropGrid);
		m_SingleLineProp->DrawProperties();
	}

	//withoutPrefix
	if (m_bAllowPrefixSelectionProp)
	{
		if (m_pOwner)
			m_WithoutPrefixProp = new CRSBitBoolProp(this, m_pOwner, _TB("Without Prefix"), &m_bWithoutPrefix);
		else if (m_pColumns)
			m_WithoutPrefixProp = new CRSBitBoolProp(this, m_pColumns, _TB("Without Prefix"), &m_bWithoutPrefix);
		AddSubItem(m_WithoutPrefixProp);															//DT_WITHOUTPREFIX
	}

	//expand tab
	if (m_bAllowExpandTabProp)
	{
		if (m_pOwner)
			m_ExpandTabProp = new CRSBitBoolProp(this, m_pOwner, _TB("Expand Tab"), &m_bExpandTab);
		else if (m_pColumns)
			m_ExpandTabProp = new CRSBitBoolProp(this, m_pColumns, _TB("Expand Tab"), &m_bExpandTab);
		AddSubItem(m_ExpandTabProp);																//DT_EXPANDTABS
	}

	//rotation
	if (m_bAllowVertical)
	{
		m_OrientationProp = new CRSBitProp(this, _TB("Rotate by"), (_variant_t)GetOrientationString());
			m_OrientationProp->AddOption(_TB("0�"), TRUE, 0);									//deafult 0
			m_OrientationProp->AddOption(_TB("90�"), TRUE, DT_EX_ORIENTATION_90);				//DT_EX_ORIENTATION_90		ma devo impostare m_nAlign = (m_nAlign | DT_EX_ORIENTATION_90) &  ~DT_EX_VCENTER_LABEL
			m_OrientationProp->AddOption(_TB("270�"), TRUE, DT_EX_ORIENTATION_270);				//DT_EX_ORIENTATION_270		ma devo impostare m_nAlign = (m_nAlign | DT_EX_ORIENTATION_270) &  ~DT_EX_VCENTER_LABEL
		AddSubItem(m_OrientationProp);
	}

	//manca ask per all columns
	Expand(FALSE);
}

//-----------------------------------------------------------------------------
CString CRSAlignBitwiseProp::GetSingleLineString()
{
	switch (m_eSingleLine)
	{
	case BitSingleLine::SingleLine:
		return _TB("Single Line");
	case BitSingleLine::MultiLine:
		return _TB("Multi Line");
	default:
		ASSERT(FALSE);
		return _TB("Error retrieving Single or Multi Line alignment");
	}
}

//-----------------------------------------------------------------------------
CString CRSAlignBitwiseProp::GetHorizontalAlignString()
{
	switch (m_eHorizontalALign)
	{
	case BitHorizontalALign::Left:
		return _TB("Left");
	case BitHorizontalALign::CenteredHorizontal:
		return _TB("Centered");
	case BitHorizontalALign::Right:
		return _TB("Right");
	default:
		ASSERT(FALSE);
		return _TB("Error retrieving Horizontal Align");
	}
}

//-----------------------------------------------------------------------------
CString CRSAlignBitwiseProp::GetVerticalAlignString()
{
	switch (m_eVerticalAlign)
	{
	case BitVerticalALign::Top:
		return _TB("Top");
	case BitVerticalALign::CenteredVertical:
		return _TB("Centered");
	case BitVerticalALign::CenteredVerticalRel:
		return _TB("Centered relative to Label");
	case BitVerticalALign::Bottom:
		return _TB("Bottom");
	case BitVerticalALign::FieldSet:
		return _TB("Field Set");
	default:
		ASSERT(FALSE);
		return _TB("Error retrieving Vertical Align");
	}
}

//-----------------------------------------------------------------------------
CString CRSAlignBitwiseProp::GetOrientationString()
{
	switch (m_eOrientation)
	{
	case BitOrientation::Orientation_0:
		return _TB("0�");
	case BitOrientation::Orientation_90:
		return _TB("90�");
	case BitOrientation::Orientation_270:
		return _TB("270�");
	default:
		ASSERT(FALSE);
		return _TB("Error retrieving Orientation");
	}
}

//-----------------------------------------------------------------------------
void CRSAlignBitwiseProp::AlignTypeToProps()
{
	//update props from align type value

	if ((*m_pAlignType & DT_SINGLELINE) == DT_SINGLELINE)
		m_eSingleLine = BitSingleLine::SingleLine;
	else 
		m_eSingleLine = BitSingleLine::MultiLine;

	m_bWordBreak = !((*m_pAlignType & DT_SINGLELINE) == DT_SINGLELINE) && ((*m_pAlignType & DT_WORDBREAK) == DT_WORDBREAK);

	m_bExpandTab = (*m_pAlignType & DT_EXPANDTABS) == DT_EXPANDTABS;

	m_bWithoutPrefix = (*m_pAlignType & DT_NOPREFIX) == DT_NOPREFIX;

	if ((*m_pAlignType & DT_CENTER) == DT_CENTER)
		m_eHorizontalALign = BitHorizontalALign::CenteredHorizontal;
	else if ((*m_pAlignType & DT_RIGHT) == DT_RIGHT)
		m_eHorizontalALign = BitHorizontalALign::Right;
	else
		m_eHorizontalALign = BitHorizontalALign::Left;

	if ((*m_pAlignType & DT_VCENTER) == DT_VCENTER)
		m_eVerticalAlign = BitVerticalALign::CenteredVertical;
	else if ((*m_pAlignType & DT_BOTTOM) == DT_BOTTOM)
		m_eVerticalAlign = BitVerticalALign::Bottom;
	else if (m_bAllowFieldSet && (*m_pAlignType & DT_EX_FIELD_SET) == DT_EX_FIELD_SET)
		m_eVerticalAlign = BitVerticalALign::FieldSet;
	else if (*m_pAlignType & DT_EX_VCENTER_LABEL)
		m_eVerticalAlign = BitVerticalALign::CenteredVerticalRel;//todo da verificare
	else 
		m_eVerticalAlign = BitVerticalALign::Top;

	if (m_bAllowVertical)
	{
		if ((*m_pAlignType & DT_EX_ORIENTATION_90) == DT_EX_ORIENTATION_90)
			m_eOrientation = BitOrientation::Orientation_90;
		else if ((*m_pAlignType & DT_EX_ORIENTATION_270) == DT_EX_ORIENTATION_270)
			m_eOrientation = BitOrientation::Orientation_270;
		else
			m_eOrientation = BitOrientation::Orientation_0;
	}
}

//-----------------------------------------------------------------------------
void CRSAlignBitwiseProp::UpdateAlignType()
{
	//update alignType pointer to from Props values
	m_nAlignTypeOld = *m_pAlignType;
	*m_pAlignType = 0;
	int index = 0;

	if (m_bAllowLineProp)
	{
		index = m_SingleLineProp->GetSelectedOption();
		if (index >= 0)
		{
			DWORD_PTR option = m_SingleLineProp->GetOptionData(index);
			BitSingleLine singleLine = (BitSingleLine)option;

			m_eSingleLine = singleLine;
			if (singleLine != 0) 
				*m_pAlignType |= singleLine; //DT_SINGLELINE
		}
	}

	if(m_bAllowWordBreak && m_bWordBreak && m_eSingleLine != BitSingleLine::SingleLine)
		*m_pAlignType |= DT_WORDBREAK;
	if (m_bAllowExpandTabProp && m_bExpandTab)
		*m_pAlignType |= DT_EXPANDTABS;
	if(m_bAllowPrefixSelectionProp && m_bWithoutPrefix)
		*m_pAlignType |= DT_NOPREFIX;

	index = m_HorizontalAlignProp->GetSelectedOption();
	if (index >= 0)
	{
		DWORD_PTR option = m_HorizontalAlignProp->GetOptionData(index);
		BitHorizontalALign horizontalAlign = (BitHorizontalALign)option;

		m_eHorizontalALign = horizontalAlign;
		if (horizontalAlign != 0) 
			*m_pAlignType |= horizontalAlign;
	}

	*m_pAlignType &= ~DT_EX_VCENTER_LABEL;

	index = m_VerticalAlignProp->GetSelectedOption();
	if (index >= 0)
	{
		DWORD_PTR option = m_VerticalAlignProp->GetOptionData(index);
		BitVerticalALign verticalAlign = (BitVerticalALign)option;

		m_eVerticalAlign = verticalAlign;
		//TODO ANDREA: vedere se si pu� semplificare il codice come negli altri casi mettendo  
		//if (horizontalAlign != 0) *m_pAlignType |= verticalAlign; 
		//e poi vedere se fare *m_pAlignType |= CenteredVerticalRel;
		if (verticalAlign == BitVerticalALign::CenteredVertical)
		{
			*m_pAlignType |= CenteredVertical;

			if (m_bAllowCenterBottom &&
				this->GetPropertyGrid()->GetPropertyView()->m_pDocument &&
				((CWoormDocMng*)this->GetPropertyGrid()->GetPropertyView()->m_pDocument)->m_pWoormIni &&
				!((CWoormDocMng*)this->GetPropertyGrid()->GetPropertyView()->m_pDocument)->m_pWoormIni->m_bForceVerticalAlignLabelRelative)
				
				*m_pAlignType |= CenteredVerticalRel;
		}

		else if (m_bAllowFieldSet && verticalAlign == BitVerticalALign::FieldSet)
			*m_pAlignType |= DT_EX_FIELD_SET;

		else if (verticalAlign == BitVerticalALign::CenteredVerticalRel )
			*m_pAlignType |= CenteredVerticalRel;
		
		else if(verticalAlign == BitVerticalALign::Bottom)
			*m_pAlignType |= Bottom;
	}

	if (m_bAllowVertical && m_OrientationProp && m_eVerticalAlign != BitVerticalALign::FieldSet)
	{
		index = m_OrientationProp->GetSelectedOption();
		if (index >= 0)
		{
			DWORD_PTR option = m_OrientationProp->GetOptionData(index);
			BitOrientation orientation = (BitOrientation)option;
			m_eOrientation = (BitOrientation)orientation;
			if (orientation != 0) *m_pAlignType = (*m_pAlignType |= orientation) &  ~DT_EX_VCENTER_LABEL;
		}
	}

	UpdateSelectedObject();
}

//-----------------------------------------------------------------------------
void CRSAlignBitwiseProp::UpdateSelectedObject()
{
	//refresh layout object with the new value
	if (m_pOwner)
	{
		GenericDrawObj* pGenObj = dynamic_cast<GenericDrawObj*>(m_pOwner);
		if (pGenObj)
			pGenObj->Redraw();
	}

	else if (m_pColumns)
		m_pColumns->Redraw();
}

//-----------------------------------------------------------------------------
//metodo richiamato ad ogni modifica sulle propriet� figlie (tante volte, quindi,
//in modo da aggiornare il valore della property parent
CString CRSAlignBitwiseProp::FormatProperty()
{
	CString strVal;

	ULONG ulFlags = 0;

	BOOL bIsFirst = TRUE;

	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		CBCGPProp* pProp = m_lstSubItems.GetNext(pos);
		ASSERT_VALID(pProp);

		CRSBoolProp* boolProp = dynamic_cast<CRSBoolProp*>(pProp);
		if (boolProp && !(bool)pProp->GetValue())
			continue;

		if (bIsFirst) strVal = _T("(");

		else
		{
			strVal += m_pWndList->GetListDelimiter();
			strVal += _T(' ');
		}

		if (boolProp)
			//property booleana
			strVal += pProp->GetName();

		else if (pProp-> GetOptionCount() >0)
		{
			//property con options (dropdown)
			int selOption = pProp->GetSelectedOption();
			if (selOption < 0) continue;
			CString optionDescription = pProp->GetOption(selOption);
			strVal += optionDescription;
		}

		bIsFirst = FALSE;
	}

	if (!bIsFirst) strVal += _T(")");

	return strVal;
}

//-----------------------------------------------------------------------------
void CRSAlignBitwiseProp::RedrawSingleLineProp()
{
	if (m_SingleLineProp)
		m_SingleLineProp->DrawProperties();
}

//================================CRSColumnAlignBitwiseProp==================================
//-----------------------------------------------------------------------------
CRSColumnAlignBitwiseProp::CRSColumnAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, TableColumn* pCol, const CString& strName, AlignType* pAlignType, PropertyType ePropertyType)
	:
	CRSAlignBitwiseProp(PropertyView, pCol, strName, pAlignType, FALSE, TRUE, FALSE, 0, L""),
	m_ePropertyType(ePropertyType),
	m_pCol(pCol)
{
	switch (m_ePropertyType)
	{
	case PropertyType::Body:
		SetDescription(_TB("Body Alignment property"));
		break;
	}

	AlignTypeToProps();

	Rebuild();
}

//-----------------------------------------------------------------------------
void CRSColumnAlignBitwiseProp::UpdateSelectedObject()
{
	switch (m_ePropertyType)
	{
	case PropertyType::Body:
		m_pCol->SetColumnAlign(GetAlignType());
		break;
	}

	m_pCol->Redraw();
}

//================================CRSBitBoolProp==================================
//-----------------------------------------------------------------------------
CRSBitBoolProp::CRSBitBoolProp(CRSAlignBitwiseProp* pParent, CObject* pOwner, const CString& strName, BOOL* value, const CString& description)
	: CRSBoolProp(pOwner, strName, value, description),
	m_pParent(pParent)
{
}

//-----------------------------------------------------------------------------
CRSBitBoolProp::CRSBitBoolProp(CRSAlignBitwiseProp* pParent, MultiColumnSelection* pColumns, const CString& strName, BOOL* value, const CString& description)
	: CRSBoolProp(pColumns, strName, value, description),
	m_pParent(pParent)
{
}

//-----------------------------------------------------------------------------
BOOL CRSBitBoolProp::OnUpdateValue()
{
	BOOL baseUpdate = CRSBoolProp::OnUpdateValue(); //controlla aggiornamento grafico che potrebbe dare problemi
	m_pParent->UpdateAlignType();
	return baseUpdate;
}

//================================CRSBitProp==================================
//-----------------------------------------------------------------------------
CRSBitProp::CRSBitProp(CRSAlignBitwiseProp* pParent, const CString& strName, const _variant_t& varValue)
	: CrsProp(strName, varValue),
	m_pParent(pParent)
{
}

//-----------------------------------------------------------------------------
BOOL CRSBitProp::OnUpdateValue()
{
	BOOL baseUpdate = CrsProp::OnUpdateValue();
	m_pParent->UpdateAlignType();
	return baseUpdate;
}

//================================CRSBitSingleLineProp==================================
//-----------------------------------------------------------------------------
CRSBitSingleLineProp::CRSBitSingleLineProp(CRSAlignBitwiseProp* pParent, CRSBitBoolProp* pChild, const CString& strName, const _variant_t& varValue)
	: CRSBitProp(pParent, strName, varValue),
	m_pParent(pParent),
	m_pChild(pChild)
{
	DrawProperties();
}

//-----------------------------------------------------------------------------
BOOL CRSBitSingleLineProp::OnUpdateValue()
{
	BOOL baseUpdate = CRSBitProp::OnUpdateValue();
	DrawProperties();

	return baseUpdate;
}

//-----------------------------------------------------------------------------
//serve per decidere se disegnare o meno la subitemProperty
void CRSBitSingleLineProp::DrawProperties()
{
	int index = -1;
	if (m_pWndList != NULL)
		index = GetSelectedOption();

	if (index < 0)
		return;

	DWORD_PTR option = GetOptionData(index);

	this->m_bGroup = TRUE;

	if (this->GetSubItemsCount() > 0 && m_pChild)
		this->RemoveSubItem((CBCGPProp*&)m_pChild, FALSE);

	if ((int)option == 0 && m_pChild)
	{
		AddSubItem(m_pChild);
	}

	BOOL bError = FALSE;

	//"messaggio di errore se la riga � gi� "Distribute over multiline if too long" e voglio settare il multiline
	CRSColumnAlignBitwiseProp* pColumnParent = dynamic_cast<CRSColumnAlignBitwiseProp*>(m_pParent);
	if (pColumnParent)
	{
		TableColumn* pCol = pColumnParent->GetTableColumn();

		if (pCol->IsMultiRow() && (int)option == 0)
			bError = TRUE;
	}

	//"messaggio di errore se la riga � gi� "Distribute over multiline if too long" e voglio settare il multiline
	CRSMultiColumnsAlignmentStyleBitWiseProp* pMulColumnParent = dynamic_cast<CRSMultiColumnsAlignmentStyleBitWiseProp*>(m_pParent);
	if (pMulColumnParent)
	{
		MultiColumnSelection* pColumns = pMulColumnParent->GetColumns();

		if (pColumns->AreMultiRows() && (int)option == 0)
			bError = TRUE;
	}

	if (bError)
	{
		this->SetColoredState(CrsProp::State::Error);
		this->SetNewDescription(_TB("This setting is not compatible with the property \"Distribute over multiline if too long\""));
	}

	else
	{
		this->SetColoredState(CrsProp::State::Normal);
		this->SetOriginalDescription();
	}

	this->m_bGroup = FALSE;

	this->GetPropertyGrid()->AdjustLayout();
}

//================================CrsProp==================================
CrsProp::CrsProp(const CString& strGroupName, DWORD_PTR dwData, BOOL bIsValueList)
	: 
	CBCGPProp(strGroupName, dwData, bIsValueList),
	IDisposingSourceImpl(this),
	m_eImg(CRS_PropertyGrid::Img::NoImg), 
	m_pChildProp(NULL),
	m_pMmProp(NULL)
{
}

//-----------------------------------------------------------------------------
CrsProp::CrsProp (const CString& strName, const _variant_t& varValue,
					LPCTSTR lpszDescr, DWORD_PTR dwData,
					LPCTSTR lpszEditMask, LPCTSTR lpszEditTemplate,
					LPCTSTR lpszValidChars)
	: 
	CBCGPProp(strName, varValue, lpszDescr, dwData, lpszEditMask, lpszEditTemplate, lpszValidChars),
	IDisposingSourceImpl(this),
	m_strOriginalDescr(lpszDescr),
	m_eImg(CRS_PropertyGrid::Img::NoImg), 
	m_pChildProp(NULL),
	m_pMmProp(NULL)
{
}

//-----------------------------------------------------------------------------
CrsProp::CrsProp (const CString& strName, UINT nID, const _variant_t& varValue,
					LPCTSTR lpszDescr, DWORD_PTR dwData,
					LPCTSTR lpszEditMask, LPCTSTR lpszEditTemplate,
					LPCTSTR lpszValidChars)
	: 
	CBCGPProp(strName, nID, varValue, lpszDescr, dwData, lpszEditMask, lpszEditTemplate, lpszValidChars),
	IDisposingSourceImpl(this),
	m_strOriginalDescr(lpszDescr),
	m_eImg(CRS_PropertyGrid::Img::NoImg), 
	m_pChildProp(NULL),
	m_pMmProp(NULL)
{
}

//-----------------------------------------------------------------------------
void CrsProp::SetGroup(BOOL isGroup)
{
	this->m_bGroup = isGroup;
}

//-----------------------------------------------------------------------------
BOOL CrsProp::AddSubItem(CBCGPProp* pSubProp, CBCGPPropList* pWndList)
{
	if (this->m_bGroup)
		return __super::AddSubItem(pSubProp);

	if (pWndList)
	{
		this->SetOwnerList(pWndList);
		((CrsProp*)pSubProp)->SetOwnerList(pWndList);
	}

	else
		((CrsProp*)pSubProp)->SetOwnerList(GetPropertyGrid());

	SetGroup(TRUE);

	BOOL bRet = __super::AddSubItem(pSubProp);

	SetGroup(FALSE);

	return bRet;
}

//-----------------------------------------------------------------------------
void CrsProp::SetVisible(BOOL visible)
{
	m_bIsVisible = visible;

	ASSERT_VALID(this);
	for (int i = 0;i < this->GetSubItemsCount();i++)
	{
		CrsProp* prop = dynamic_cast<CrsProp*>(this->GetSubItem(i));
		CRSColorWithExprProp* colorProp = dynamic_cast<CRSColorWithExprProp*>(this->GetSubItem(i));
		if (prop)
			prop->SetVisible(visible);
		else if (colorProp)
			colorProp->SetVisible(visible);
	}
}

//-----------------------------------------------------------------------------
void CrsProp::SetEnable(BOOL bEnable, BOOL bIncludeChildren)
{
	Enable(bEnable);
	ASSERT_VALID(this);

	if (!bIncludeChildren) return;

	for (int i = 0; i < this->GetSubItemsCount(); i++)
	{
		CrsProp* prop = dynamic_cast<CrsProp*>(this->GetSubItem(i));
		CRSColorProp* colorProp = dynamic_cast<CRSColorProp*>(this->GetSubItem(i));
		if (prop)
			prop->SetEnable(bEnable);
		else if (colorProp)
			colorProp->SetEnable(bEnable);
	}
}

//-----------------------------------------------------------------------------
void CrsProp::SetColoredState(CrsProp::State eState)
{
	switch (eState)
	{
	case CrsProp::State::Important:
		SetState(_TB("Important"), _T('-'), RS_COLOR_PROP_IMPORTANT);	
		break;
	case CrsProp::State::Mandatory:
		SetState(_TB("Mandatory!"), _T('!'), RS_COLOR_PROP_MANDATORY);
		break;
	case CrsProp::State::Error:
		SetState(_TB("Error!"), _T('!'), RGB(255,0,0));
		break;
	case CrsProp::State::Normal:
	default:
		SetState(NULL, _T(' '), RGB(0, 0, 0), FALSE);
		break;
	}
}

//*****************************************************************************************
//metodo implementato per gestire se settare o meno il modified flag
BOOL CrsProp::SelectOption(int nIndex, BOOL bSetModifiedFlag)
{
	ASSERT_VALID(this);

	if (m_varValue.vt == VT_BOOL)
	{
		SetValue(nIndex != 0);
	}

	else
	{
		LPCTSTR lpszOption = nIndex < 0 ? _T("") : GetOption(nIndex);
		if (lpszOption == NULL)
		{
			return FALSE;
		}

		SetValue(lpszOption);
	}

	if(bSetModifiedFlag)
		SetModifiedFlag();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CrsProp::SetOriginalDescription()
{
	if (!m_strOriginalDescr)
		ASSERT(FALSE);
	SetDescription(m_strOriginalDescr);
}

//-----------------------------------------------------------------------------
void CrsProp::SetNewDescription(CString strNewDescription, BOOL bAppendOriginalDescr)
{
	if (!bAppendOriginalDescr)
		SetDescription(strNewDescription);
	else
		SetDescription(strNewDescription + _T("\r\n\r\n") + m_strOriginalDescr);
}

//-----------------------------------------------------------------------------
CRS_PropertyGrid* CrsProp::GetPropertyGrid()
{
	ASSERT_VALID(this->m_pWndList);

	CRS_PropertyGrid* pPropGrid = dynamic_cast<CRS_PropertyGrid*>(this->m_pWndList);
	if (!pPropGrid)
		ASSERT(FALSE);

	return pPropGrid;
}

//-----------------------------------------------------------------------------
CRS_ObjectPropertyView*	CrsProp::GetPropertyView()
{
	return GetPropertyGrid()->GetPropertyView();
}

//-----------------------------------------------------------------------------
CWoormDocMng*	CrsProp::GetDocument()
{
	return GetPropertyGrid()->GetPropertyView()->GetDocument();
}

//-----------------------------------------------------------------------------
CNodeTree* CrsProp::GetCurrentNode()
{
	CNodeTree* currentNode = GetPropertyView()->m_pTreeNode;
	
	ASSERT_VALID(currentNode);

	return currentNode;
}

//-----------------------------------------------------------------------------
//void CrsProp::UpdateCurrentNode()
//{
//	this->GetPropertyGrid()->GetReportTreeView()->UpdateRSTreeNode(GetCurrentNode());
//}

//-----------------------------------------------------------------------------
void CrsProp::OnDrawStateIndicator(CDC* pDC, CRect rect)
{
	if (m_eImg > CRS_PropertyGrid::Img::NoImg)
		GetPropertyGrid()->m_imageList.DrawEx(pDC, rect, m_eImg, CBCGPToolBarImages::ImageAlignHorzCenter, CBCGPToolBarImages::ImageAlignVertCenter);
}

//-----------------------------------------------------------------------------
int CrsProp::GetOptionDataIndex(DWORD_PTR d) const
{
	for (int i = 0; i < this->GetOptionCount(); i++)
	{
		if (d == this->GetOptionData(i))
		{
			return i;
		}
	}

	return -1;
}

//-----------------------------------------------------------------------------
BOOL CrsProp::InsertOption(int index, LPCTSTR lpszOption, BOOL bInsertUnique/* = TRUE*/, DWORD_PTR dwData/* = 0*/)
{
	ASSERT_VALID(this);
	ASSERT(lpszOption != NULL);

	if (index < 0 || index >= m_lstOptions.GetCount())
		return AddOption(lpszOption, bInsertUnique, dwData);

	if (bInsertUnique)
	{
		if (m_lstOptions.Find(lpszOption) != NULL)
		{
			return FALSE;
		}
	}

	POSITION pos = m_lstOptions.FindIndex(index);
	m_lstOptions.InsertBefore(pos, lpszOption);
	pos = m_lstOptionsData.FindIndex(index);
	m_lstOptionsData.InsertBefore(pos, dwData);

	m_dwFlags = PROP_HAS_LIST;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CrsProp::CheckPropValue(BOOL bAllowEmpty, CString &errMsg)
{
	CString name = GetValue();

	CString emptyErr = _TB("Empty name is not allowed");
	CString invalidErr = _TB("Name is invalid! The name can contain only letters, numbers and a '_'. The name cannot start with a number");
	CString reservedErr = _TB("The name collides with a reserved word of TaskBuilder");
	CString existErr = _TB("This name already exists");

	errMsg = _T("");

	if (!bAllowEmpty && name.IsEmpty())
	{
		errMsg = emptyErr;
		SetColoredState(CrsProp::Mandatory);		
	}

	else if (!IsValidName(name))
	{
		errMsg = invalidErr;
		SetColoredState(CrsProp::Error);
	}

	else if (AfxGetTokensTable()->IsInLanguage(name))
	{
		errMsg = reservedErr;
		SetColoredState(CrsProp::Error);
	}

	else if (GetDocument()->GetEditorSymTable()->GetField(name))
	{
		errMsg = existErr;
		SetColoredState(CrsProp::Error);
	}

	if (!errMsg.IsEmpty() )
	{ 
		SetNewDescription(errMsg, TRUE);
		GetPropertyGrid()->AdjustLayout();
		return false;
	}		
	else
	{ 
		SetOriginalDescription();
		GetPropertyGrid()->AdjustLayout();
		SetColoredState(CrsProp::Normal);
		return true;
	}
}

//*******************************************************************************************
void CrsProp::SetFilter(const CString& strFilter)
{
	ASSERT_VALID(this);
	ASSERT_VALID(m_pWndList);

	m_bInFilter = m_pWndList->IsPropertyMatchedToFilter(this, strFilter);
}

//*******************************************************************************************
void CrsProp::AllowEdit(BOOL bAllow/* = TRUE*/)
{
	if (!bAllow && m_pWndList && GetOptionCount() && GetSelectedOption() == -1)
	{
		ASSERT_VALID(m_pWndList);
		ASSERT(FALSE);
		SelectOption(0);
	}

	__super::AllowEdit(bAllow);
}

//*******************************************************************************************
BOOL CrsProp::AddMMProp(int minValueInPixel, int maxValueInPixel)
{
	ASSERT_VALID(this);

	switch (m_varValue.vt)
	{
	case VT_INT:
	case VT_UINT:
	case VT_I2:
	case VT_I4:
	case VT_UI2:
	case VT_UI4:
		break;

	default:
		ASSERT(FALSE);
		return FALSE;
	}

	m_pMmProp = new CRSMMProp(this, _T("in mm"), (int)GetValue(), _T("mm"), CRSMMProp::PixelInMM(minValueInPixel), CRSMMProp::PixelInMM(maxValueInPixel));

	SetGroup(TRUE);

	AddSubItem(m_pMmProp);

	SetGroup(FALSE);

	return TRUE;
}

//*******************************************************************************************
void CrsProp::UpdateMmProp()
{
	ASSERT_VALID(this);
	if (m_pMmProp)
		m_pMmProp->UpdateMM((int)GetValue());
}

//================================CRSLineStyleProp==================================
//-----------------------------------------------------------------------------
CRSLineStyleProp::CRSLineStyleProp(CObject* pOwner, const CString& strName, int* pStyle, CRS_ObjectPropertyView* pPropertyView,
	LPCTSTR lpszDescr, DWORD_PTR dwData)
	:
	CBCGPLineStyleProp(strName, (CBCGPStrokeStyle::BCGP_DASH_STYLE)*pStyle, lpszDescr, dwData),
	m_pOwner(pOwner),
	m_pStyle(pStyle),
	m_pPropertyView(pPropertyView)
{
	ASSERT_VALID(m_pOwner);
}

//-----------------------------------------------------------------------------
BOOL CRSLineStyleProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	*m_pStyle = (int)GetValue();

	WoormIni* pWoormIni = dynamic_cast<WoormIni*>(m_pOwner);
	if (pWoormIni)
	{
		pWoormIni->WriteWoormSettings();
		//ridisegno tutto
		m_pPropertyView->GetDocument()->Invalidate(TRUE);
	}

	return baseUpdate;
}

//-----------------------------------------------------------------------------
CString CRSLineStyleProp::FormatProperty()
{
	m_varValue = (long)m_varValue < 0 ? 0 : (long)m_varValue;

	return GetOption((long)m_varValue);
}

//================================CRSObjectSelectionStyleProp==================================
//-----------------------------------------------------------------------------
CRSObjectSelectionStyleProp::CRSObjectSelectionStyleProp(WoormIni* pWrmIni, CRS_ObjectPropertyView* pPropertyView)
	:
	CrsProp(_TB("Theme"), L"", _TB("Change style of the layout object selection")),
	m_pWrmIni(pWrmIni),
	m_pPropertyView(pPropertyView),
	m_bNewObjectSelectionStyle(&pWrmIni->m_bEnableNewObjectSelection)
{
	ASSERT_VALID(pWrmIni);
	AllowEdit(FALSE);

	AddOption(_TB("Mago3"), TRUE, 0);
	AddOption(_TB("M4go"), TRUE, 1);

	//todo scrivere meglio?
	if (*m_bNewObjectSelectionStyle)
		SetValue(GetOption(1));
	else
		SetValue(GetOption(0));

	DrawProperties(m_pWrmIni->m_bEnableNewObjectSelection);
}

//-----------------------------------------------------------------------------
void CRSObjectSelectionStyleProp::DrawProperties(BOOL bShowSubItems)
{
	if(!bShowSubItems)
		RemoveAllSubItems();
	else
	{ 
		if (this->GetSubItemsCount() == 0)
		{
			SetGroup(TRUE);
			//color
			AddSubItem(new CRSColorProp(m_pWrmIni, _TB("Color"), &m_pWrmIni->m_rgb_ObjectSelectionColor, L""));
			//size
			AddSubItem(new CRSIntProp(m_pWrmIni, _TB("Size"), &m_pWrmIni->m_nObjectSelectionSize, L"", 1, 10, CRSIntProp::CooType::DEFAULT));
			//pen
			AddSubItem(new CRSLineStyleProp(m_pWrmIni, _TB("Line Style"), &m_pWrmIni->m_eObjectSelectionLineStyle, m_pPropertyView, L""));

			SetGroup(FALSE);
		}
	}

	m_pPropertyView->GetPropertyGrid()->AdjustLayout();
}

//-----------------------------------------------------------------------------
BOOL CRSObjectSelectionStyleProp::OnUpdateValue()
{
	BOOL baseUpdate = __super::OnUpdateValue();

	int index = GetSelectedOption();

	if (index < 0)
		return baseUpdate;

	*m_bNewObjectSelectionStyle = index;

	DrawProperties(index);

	//aggiorno il file di configurazioni 	//todo scrivere meglio?
	m_pWrmIni->WriteWoormSettings();
	//deleto il vecchio activerect
	delete (m_pPropertyView->GetDocument()->m_pActiveRect);
	//inizializzo un nuovo active rect
	if(*m_bNewObjectSelectionStyle)
		m_pPropertyView->GetDocument()->m_pActiveRect = new NewActiveRect(m_pWrmIni, TRUE);
	else
		m_pPropertyView->GetDocument()->m_pActiveRect = new OldActiveRect();

	//TODO spostare nel costruttore
	m_pPropertyView->GetDocument()->m_pActiveRect->Attach(m_pPropertyView->GetDocument());

	//ridisegno tutto
	//m_pPropertyView->GetDocument()->Invalidate(TRUE);
	//CPoint pt;
	//m_pPropertyView->GetDocument()->SetCurrentObj(m_pPropertyView->GetDocument()->m_pCurrentObj, pt);

	return baseUpdate;
}

//================================CRSRuleExpressionProp==================================
//-----------------------------------------------------------------------------
CRSRuleExpressionProp::CRSRuleExpressionProp(const CString& strName, ExpRuleData* expRuleData, DataType dataType, const CString& description)
	:
	CrsProp(strName, (LPCTSTR)expRuleData->Unparse(FALSE, FALSE), description)
{
	m_expRuleData = expRuleData;
	m_dataType = dataType;
	AllowEdit(FALSE);
}

//-----------------------------------------------------------------------------
void CRSRuleExpressionProp::OnClickButton(CPoint point)
{
	CWoormDocMng* wrmDocMng = this->GetPropertyView()->GetDocument();

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	CRSEditView* pEditView = dynamic_cast<CRSEditView*>(wrmDocMng->CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
	ASSERT_VALID(pEditView);
	if (!pEditView)
		return;

	if (m_dataType == DataType::String || m_dataType == DataType::Text)
		pEditView->EnableStringPreview();

	ASSERT_VALID(m_expRuleData);
	pEditView->LoadElement(m_expRuleData,NULL);

	//modal view
	pEditView->DoEvent();
	ASSERT_VALID(this);
	//aggiorniamo il valore della property
	CString sVal;
	if (m_expRuleData)
	{
		ASSERT_VALID(m_expRuleData);
		sVal = m_expRuleData->Unparse(FALSE, FALSE);
	}

	SetValue((LPCTSTR)sVal);
}

//================================CRSBlockExpressionProp==================================
//-----------------------------------------------------------------------------
CRSBlockExpressionProp::CRSBlockExpressionProp(const CString& strName, Block** block, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description)
	:CrsProp(strName, (*block) == NULL ? (LPCTSTR)L"" : (LPCTSTR)(*block)->Unparse(), description)
{

	m_block = block;
	m_pSymTable = pSymTable;
	m_pPropertyView = pPropertyView;
	AllowEdit(FALSE);
}

//-----------------------------------------------------------------------------
void CRSBlockExpressionProp::OnClickButton(CPoint point)
{
	CWoormDocMng* wrmDocMng = m_pPropertyView->GetDocument();

	ASSERT_VALID(wrmDocMng);
	if (!wrmDocMng)
		return;

	CRSEditView* pEditView = dynamic_cast<CRSEditView*>(wrmDocMng->CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
	ASSERT_VALID(pEditView);
	if (!pEditView)
		return;

	pEditView->LoadElement(m_pSymTable, m_block, FALSE);

	//modal view
	pEditView->DoEvent();
	ASSERT_VALID(this);
	//aggiorniamo il valore della propertyy
	SetValue((*m_block) == NULL ? (LPCTSTR)L"" : (LPCTSTR)(*m_block)->Unparse());
}

//================================CRSAskDialogProp========================
// Property class for setting property of ask dialog
//-----------------------------------------------------------------------------
CRSAskDialogProp::CRSAskDialogProp(AskDialogData* askDialog, CString name, LPCTSTR value, DialogType dType, CRS_ObjectPropertyView* m_pView) :CrsProp(name, value){
	this->m_pAskDialog = askDialog;
	this->m_pDType = dType;
	this->m_pView = m_pView;
}

//-----------------------------------------------------------------------------
CRSAskDialogProp::CRSAskDialogProp(AskDialogData* askDialog, CString name, variant_t value, DialogType dType, CRS_ObjectPropertyView* m_pView) : CrsProp(name, value){
	this->m_pAskDialog = askDialog;
	this->m_pDType = dType;
	this->m_pView = m_pView;
}

//-----------------------------------------------------------------------------
BOOL CRSAskDialogProp::OnUpdateValue(){
	CString prevValue = GetValue();
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();
	CString value = GetValue();
	//-------------------------------------------------------------------------------
	//ottimizzazione
	if (value == prevValue)
		return baseUpdate;

	switch (m_pDType)
	{
	case DialogType::NAME:
	{
		m_pAskDialog->SetName(value);

		m_pView->GetDocument()->UpdateRSTreeNode(ERefreshEditor::Dialogs, m_pView->m_pTreeNode);
		break;
	}

	case DialogType::TITLE:
	{
		m_pAskDialog->SetTitle(value);

		m_pView->GetDocument()->UpdateRSTreeNode(ERefreshEditor::Dialogs, m_pView->m_pTreeNode);
		break;
	}

	case DialogType::ONLY_ON_EVENT:
	{
		m_pAskDialog->SetOnAsk(MakeBoolFromString(value));
		m_pView->GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, m_pAskDialog);
		break;
	}

	case DialogType::POSITION_OF_FIELDS:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		Token option = (Token)GetOptionData(index);
		m_pAskDialog->SetCaptionPos(option);
		break;
	}
	}

	return baseUpdate;
}

//================================CRSGroupFieldProp========================
//Class for group field
//-----------------------------------------------------------------------------
CRSGroupFieldProp::CRSGroupFieldProp(AskGroupData* groupField, CString name, LPCTSTR value, AskGroupType dFieldType) : CrsProp(name, value){
	this->m_askGroupfield = groupField;
	this->m_pGroupType = dFieldType;
}

//-----------------------------------------------------------------------------
CRSGroupFieldProp::CRSGroupFieldProp(AskGroupData* groupField, CString name, variant_t value, AskGroupType dFieldType) : CrsProp(name, value){
	this->m_askGroupfield = groupField;
	this->m_pGroupType = dFieldType;
}

//-----------------------------------------------------------------------------
BOOL CRSGroupFieldProp::OnUpdateValue(){

	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	CString value = this->GetValue();

	switch (m_pGroupType){
		/*case AskGroupType::HIDDEN_TITLE:{
			m_askGroupfield->m_bHiddenTitle = MakeBoolFromString(value);
			break;
		}*/

		case AskGroupType::CAPTION:{
			//if (!value.IsEmpty())
			m_askGroupfield->m_strTitle = value;
			GetDocument()->UpdateRSTreeNode(ERefreshEditor::Dialogs, GetCurrentNode());
			break;
		}

		case AskGroupType::CAPTION_POSITION:{
			int index = GetSelectedOption();
			if (index < 0)
				break;
			Token option = (Token)GetOptionData(index);
			m_askGroupfield->m_nFieldsCaptionPos = option;
			break;
		}

	}

	return TRUE;
}

//================================CRSPageProp========================
//Class for page property
//-----------------------------------------------------------------------------
CRSPageProp::CRSPageProp(PageInfo* pageInfo, CString name, LPCTSTR value, PageInfoType pageInfoType, CRS_ObjectPropertyView* propertyView) : CrsProp(name, value){
	this->m_pageInfo = pageInfo;
	this->m_pageInfoType = pageInfoType;
	this->m_propertyView = propertyView;
}

CRSPageProp::CRSPageProp(PageInfo* pageInfo, CString name, variant_t value, PageInfoType pageInfoType, CRS_ObjectPropertyView* propertyView) : CrsProp(name, value){
	this->m_pageInfo = pageInfo;
	this->m_pageInfoType = pageInfoType;
	this->m_propertyView = propertyView;
}

//-----------------------------------------------------------------------------
BOOL CRSPageProp::OnUpdateValue(){
	
	CString oldValue =this->GetValue();
	
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	CString value = this->GetValue();

	if (oldValue.CompareNoCase(value) == 0)
		return TRUE;

	CWoormDocMng* doc = m_propertyView->GetDocument();
	switch (m_pageInfoType){
	case PageInfoType::REPORT_STYLES:{
		if (value.IsEmpty())
			break;
		int index = GetSelectedOption();
		if (index < 0)
			break;

		DWORD_PTR option = GetOptionData(index);
		PrinterInfo* prInfo = new PrinterInfo();

		PrinterInfoItem* infoItem = prInfo->GetPrinterInfoItemObject((WORD)option);

		if (!infoItem)
			break;

		if (m_pageInfo->dmOrientation == DMORIENT_LANDSCAPE)
		{
			m_pageInfo->dmPaperLength = (short)infoItem->m_sPaperSize.cx;
			m_pageInfo->dmPaperWidth = (short)infoItem->m_sPaperSize.cy;
		}

		else
		{
			m_pageInfo->dmPaperLength = (short)infoItem->m_sPaperSize.cy;
			m_pageInfo->dmPaperWidth = (short)infoItem->m_sPaperSize.cx;
		}
		
		m_pageInfo->dmPaperSize = (short)infoItem->m_wPaperType;

		CString width, length;
		if (m_pageInfo->dmOrientation == DMORIENT_LANDSCAPE)	// portrait is default
		{
			// if landscape invert values
			width.Format(L"%d", infoItem->m_sPaperSize.cy / 10);
			length.Format(L"%d", infoItem->m_sPaperSize.cx / 10);
		}

		else
		{
			width.Format(L"%d", infoItem->m_sPaperSize.cx / 10);
			length.Format(L"%d", infoItem->m_sPaperSize.cy / 10);
		}
		
		m_propertyView->wrmWidth->SetValue((LPCTSTR)width);
		m_propertyView->wrmLength->SetValue((LPCTSTR)length); 
		//devo ridisegnare tutto perch� abbia effetto lo swap
		CWoormDocMng* doc = m_propertyView->GetDocument();
		if (doc)
			doc->Invalidate(TRUE);
		break;
	}

	case PageInfoType::PRINTER_STYLES:{
		if (value.IsEmpty())
			break;
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);
		PrinterInfo* prInfo = new PrinterInfo(m_pageInfo->GetPreferredPrinter());
		PrinterInfoItem* infoItem = prInfo->GetPrinterInfoItemObject((WORD)option);

		if (!infoItem)
			break;

		if (m_pageInfo->dmOrientation == DMORIENT_LANDSCAPE)
		{
			m_pageInfo->m_PrinterPageInfo.dmPaperLength = (short)infoItem->m_sPaperSize.cx;
			m_pageInfo->m_PrinterPageInfo.dmPaperWidth = (short)infoItem->m_sPaperSize.cy;
		}

		else
		{
			m_pageInfo->m_PrinterPageInfo.dmPaperLength = (short)infoItem->m_sPaperSize.cy;
			m_pageInfo->m_PrinterPageInfo.dmPaperWidth = (short)infoItem->m_sPaperSize.cx;
		}
		
		m_pageInfo->m_PrinterPageInfo.dmPaperSize = (short)infoItem->m_wPaperType;

		CString width, length;
		if (m_pageInfo->dmOrientation == DMORIENT_LANDSCAPE)	// portrait is default
		{
			// if landscape invert values
			width.Format(L"%d", infoItem->m_sPaperSize.cy / 10);
			length.Format(L"%d", infoItem->m_sPaperSize.cx / 10);
		}

		else
		{
			width.Format(L"%d", infoItem->m_sPaperSize.cx / 10);
			length.Format(L"%d", infoItem->m_sPaperSize.cy / 10);
		}

		m_propertyView->printerWidth->SetValue((LPCTSTR)width);
		m_propertyView->printerLength->SetValue((LPCTSTR)length);

		break;
	}

	case PageInfoType::REPORT_WIDTH:{
		try{
			short width = (short)_wtoi(value)*10;
			if (m_pageInfo->dmPaperWidth == width)
				break; 
			m_pageInfo->dmPaperWidth = width;
		}

		catch (...){
			m_pageInfo->dmPaperWidth = 0;
		}
		
		m_propertyView->wrmStyle->SetValue((LPCTSTR)L"");
		break;
	}

	case PageInfoType::REPORT_LENGTH:{
		try{
			short length = (short)_wtoi(value)*10;
			if (m_pageInfo->dmPaperLength == length)
				break;
			m_pageInfo->dmPaperLength = length;			
		}

		catch (...){
			m_pageInfo->dmPaperLength = 0;
		}

		m_propertyView->wrmStyle->SetValue((LPCTSTR)L"");
		break;
	}

	case PageInfoType::PRINTER_WIDTH:{		
		try{
			short width = (short)_wtoi(value)*10;
			if (m_pageInfo->m_PrinterPageInfo.dmPaperWidth == width)
				break;
			m_pageInfo->m_PrinterPageInfo.dmPaperWidth =width;
		}

		catch (...){
			m_pageInfo->m_PrinterPageInfo.dmPaperWidth = 0;
		}

		m_propertyView->printerStyle->SetValue((LPCTSTR)L"");
		break;
	}

	case PageInfoType::PRINTER_LENGTH:{		
		try{
			short length = (short)_wtoi(value)*10;
			if (m_pageInfo->m_PrinterPageInfo.dmPaperLength == length)
				break;
			m_pageInfo->m_PrinterPageInfo.dmPaperLength = length;
		}

		catch (...){
			m_pageInfo->m_PrinterPageInfo.dmPaperLength = 0;
		}

		m_propertyView->printerStyle->SetValue((LPCTSTR)L"");
		break;
	}

	case PageInfoType::PAGE_ORIENT:{
		if (value.IsEmpty())
			break;
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);
		if (m_pageInfo->dmOrientation == option)
			break;
		m_pageInfo->dmOrientation = (short)option;
		SwapOrientation();
		//devo ridisegnare tutto perch� abbia effetto lo swap
		CWoormDocMng* doc = m_propertyView->GetDocument();
		if (doc)
			doc->Invalidate(TRUE);
		break;
	}

	case PageInfoType::NUMBER_COPIES:{
		try{
			m_pageInfo->SetCopies(_wtoi(value));
		}

		catch (...){
			m_pageInfo->SetCopies(1);
			break;
		}
	}

	case PageInfoType::COLLATE_COPIES:{

		m_pageInfo->dmCollate = MakeBoolFromString(value);
		break;
	}

	case PageInfoType::PRINTABLE_AREA:{
		if (value.Compare(L"True") == 0){
			m_pageInfo->m_bUsePrintableArea = TRUE;			
			m_propertyView->marginLeft->AllowEdit(FALSE);
			m_propertyView->marginRight->AllowEdit(FALSE);
			m_propertyView->marginTop->AllowEdit(FALSE);
			m_propertyView->marginBottom->AllowEdit(FALSE);

			//MARGINS
			m_pageInfo->CalculateMargins();
			//LEFT
			CString margin; margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.left), CM, 10., 3)));
			m_propertyView->marginLeft->SetValue((LPCTSTR)margin);

			//RIGHT			
			margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.right), CM, 10., 3)));
			m_propertyView->marginRight->SetValue((LPCTSTR)margin);
			//TOP
			margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.top), CM, 10., 3)));
			m_propertyView->marginTop->SetValue((LPCTSTR)margin);
			//BOTTOM
			margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.bottom), CM, 10., 3)));
			m_propertyView->marginBottom->SetValue((LPCTSTR)margin);
		}

		else{
			m_pageInfo->m_bUsePrintableArea = FALSE;
			m_propertyView->marginLeft->AllowEdit(TRUE);
			m_propertyView->marginRight->AllowEdit(TRUE);
			m_propertyView->marginTop->AllowEdit(TRUE);
			m_propertyView->marginBottom->AllowEdit(TRUE);
		}

		break;
	}

	case PageInfoType::MARGIN_LEFT:{
		try{
			int margin = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
			m_pageInfo->m_rectMargins.left = margin;
		}

		catch (...){
			break;
		}

		break;
	}

	case PageInfoType::MARGIN_RIGHT:{
		try{
			int margin = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
			m_pageInfo->m_rectMargins.right = margin;
		}

		catch (...){
			break;
		}

		break;
	}

	case PageInfoType::MARGIN_TOP:{
		try{
			int margin = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
			m_pageInfo->m_rectMargins.top = margin;
		}

		catch (...){
			break;
		}

		break;
	}

	case PageInfoType::MARGIN_BOTTOM:{
		try{
			int margin = MUtoLP(_wtof(value) * 10, CM, MU_SCALE, MU_DECIMAL);
			m_pageInfo->m_rectMargins.bottom = margin;
		}

		catch (...){
			break;
		}

		break;
	}

	case PageInfoType::PRINTER:{

		CString printer = m_pageInfo->GetPreferredPrinter();
		if (value.Compare(m_pageInfo->GetPreferredPrinter()) == 0)
			break;
		m_pageInfo->SetPreferredPrinter(value);
		m_propertyView->GetDocument()->m_pOptions->m_strDefaultPrinter = DataStr(value);
		m_propertyView->wrmStyle->RemoveAllOptions();
		m_propertyView->printerStyle->RemoveAllOptions();

		PrinterInfo* printerInfo = new PrinterInfo(value);
		PrinterInfoItem* wrmInfoItem = m_propertyView->GetDefaultPaperSize(printerInfo, m_pageInfo->dmPaperWidth, m_pageInfo->dmPaperLength);
		PrinterInfoItem* printerInfoItem = m_propertyView->GetDefaultPaperSize(printerInfo, m_pageInfo->m_PrinterPageInfo.dmPaperWidth, m_pageInfo->m_PrinterPageInfo.dmPaperLength);
		if (wrmInfoItem)
		{
		m_propertyView->wrmStyle->SetValue((LPCTSTR)wrmInfoItem->m_strPaperSize);
		m_propertyView->wrmStyle->AddOption(L"");
		}

		if (printerInfoItem)
		{
		m_propertyView->printerStyle->SetValue((LPCTSTR)printerInfoItem->m_strPaperSize);
		m_propertyView->printerStyle->AddOption(L"");
		}

		for (int i = 0; i < printerInfo->m_PrinterPaperInfo.GetCount(); i++)
		{
			m_propertyView->wrmStyle->AddOption(((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_strPaperSize, TRUE, ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_wPaperType);
			m_propertyView->printerStyle->AddOption(((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_strPaperSize, TRUE, ((PrinterInfoItem*)(printerInfo->m_PrinterPaperInfo[i]))->m_wPaperType);
		}

		//MARGINS
		m_pageInfo->CalculateMargins();
		//LEFT
		CString margin; margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.left), CM, 10., 3)));
		m_propertyView->marginLeft->SetValue((LPCTSTR)margin);
	
		//RIGHT
		margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.right), CM, 10., 3)));
		m_propertyView->marginRight->SetValue((LPCTSTR)margin);
		//TOP
		margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.top), CM, 10., 3)));
		m_propertyView->marginTop->SetValue((LPCTSTR)margin);
		//BOTTOM
		margin.Format(L"%.2lf", abs(LPtoMU((m_pageInfo->m_rectMargins.bottom), CM, 10., 3)));
		m_propertyView->marginBottom->SetValue((LPCTSTR)margin);

		if (doc)
			// salva le nuove impostazioni del Page Info
			doc->m_pOptions->SaveDefault(doc->GetNamespace().ToString());

		break;
	}
	}

	if (doc)
		doc->Invalidate(TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSPageProp::SwapOrientation(){
	short aux = 0;
	aux = m_pageInfo->dmPaperWidth;
	m_pageInfo->dmPaperWidth = m_pageInfo->dmPaperLength;
	m_pageInfo->dmPaperLength = aux;

	CString width; width.Format(L"%d", m_pageInfo->dmPaperWidth / 10);
	CString length; length.Format(L"%d", m_pageInfo->dmPaperLength / 10);
	m_propertyView->wrmLength->SetValue((LPCTSTR)length);
	m_propertyView->wrmWidth->SetValue((LPCTSTR)width);

	aux = m_pageInfo->m_PrinterPageInfo.dmPaperWidth;
	m_pageInfo->m_PrinterPageInfo.dmPaperWidth = m_pageInfo->m_PrinterPageInfo.dmPaperLength;
	m_pageInfo->m_PrinterPageInfo.dmPaperLength = aux;

	width; width.Format(L"%d", m_pageInfo->m_PrinterPageInfo.dmPaperWidth / 10);
	length; length.Format(L"%d", m_pageInfo->m_PrinterPageInfo.dmPaperLength / 10);

	m_propertyView->printerLength->SetValue((LPCTSTR)length);
	m_propertyView->printerWidth->SetValue((LPCTSTR)width);
}

//================================CRSPageBoolProp========================
//Class for page property
//-----------------------------------------------------------------------------
CRSPageBoolProp::CRSPageBoolProp(PageInfo* pageInfo, CString name, BOOL* pValue, CRS_ObjectPropertyView* propertyView)
	: CrsProp(name, *pValue == 1 ? (_variant_t)true : (_variant_t)false),
	m_pBValue(pValue)
{
	this->m_pageInfo = pageInfo;
	this->m_propertyView = propertyView;
}

//-----------------------------------------------------------------------------
BOOL CRSPageBoolProp::OnUpdateValue()
{
	BOOL previousValue = this->GetValue();

	BOOL baseUpdate = __super::OnUpdateValue();

	BOOL value = this->GetValue();

	*m_pBValue = value == 0 ? FALSE : TRUE;

	CWoormDocMng* doc = m_propertyView->GetDocument();

	if (doc)
		// salva le nuove impostazioni del Page Info
		doc->m_pOptions->SaveDefault(doc->GetNamespace().ToString());

	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////

CRSCommonProp::CRSCommonProp(CString name, LPCTSTR value, CRSCommonProp::PropType eType, CRS_ObjectPropertyView* propertyView, LPCTSTR originDescr /* = 0*/)
	: 
	CrsProp				(name, value, originDescr),
	m_pPropertyView		(propertyView),
	m_eType				(eType)
{
	if (eType == CRSCommonProp::PropType::NEW_TYPE)
		m_pPropertyView->LoadTypeProp(this);
	else if (eType == CRSCommonProp::PropType::NEW_ENUMTYPE)
		m_pPropertyView->LoadEnumTypeProp(this);
	else if (eType == CRSCommonProp::PropType::SELECT_TABLE_MODULE_NAME)
		m_pPropertyView->LoadTableModules(this);
	else if (eType == CRSCommonProp::PropType::NEW_HOTLINK_MODULE_NAME)
		m_pPropertyView->LoadHotlinkModules(this);
}

//-----------------------------------------------------------------------------
CRSCommonProp::CRSCommonProp(CString name, variant_t value, CRSCommonProp::PropType eType, CRS_ObjectPropertyView* propertyView, LPCTSTR originDescr /* = 0*/)
	: 
	CrsProp				(name, value, originDescr),
	m_pPropertyView		(propertyView),
	m_eType				(eType)
{
	if (eType == CRSCommonProp::PropType::NEW_TYPE)
		m_pPropertyView->LoadTypeProp(this);
	else if (eType == CRSCommonProp::PropType::NEW_ENUMTYPE)
		m_pPropertyView->LoadEnumTypeProp(this);
	else if (eType == CRSCommonProp::PropType::SELECT_TABLE_MODULE_NAME)
		m_pPropertyView->LoadTableModules(this);
	else if (eType == CRSCommonProp::PropType::NEW_HOTLINK_MODULE_NAME)
		m_pPropertyView->LoadHotlinkModules(this);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTypeProp(CBCGPProp* prop)
{
	prop->AddOption(DataType::String.ToString(),		TRUE, (DWORD)&DataType::String);
	prop->AddOption(DataType::Money.ToString(),			TRUE, (DWORD)&DataType::Money);
	prop->AddOption(DataType::Quantity.ToString(),		TRUE, (DWORD)&DataType::Quantity);
	prop->AddOption(DataType::Percent.ToString(),		TRUE, (DWORD)&DataType::Percent);
	prop->AddOption(DataType::Double.ToString(),		TRUE, (DWORD)&DataType::Double);
	prop->AddOption(DataType::Bool.ToString(),			TRUE, (DWORD)&DataType::Bool);
	prop->AddOption(DataType::Date.ToString(),			TRUE, (DWORD)&DataType::Date);
	prop->AddOption(DataType::DateTime.ToString(),		TRUE, (DWORD)&DataType::DateTime);
	prop->AddOption(DataType::Time.ToString(),			TRUE, (DWORD)&DataType::Time);
	prop->AddOption(DataType::ElapsedTime.ToString(),	TRUE, (DWORD)&DataType::ElapsedTime);
	prop->AddOption(DataType::Integer.ToString(),		TRUE, (DWORD)&DataType::Integer);
	prop->AddOption(DataType::Long.ToString(),			TRUE, (DWORD)&DataType::Long);
	prop->AddOption(DataType::Text.ToString(),			TRUE, (DWORD)&DataType::Text);
	prop->AddOption(DataType::Guid.ToString(),			TRUE, (DWORD)&DataType::Guid);
	prop->AddOption(_TB("Enumerative..."),				TRUE, (DWORD)&DataType::Enum);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTypeProp(CBCGPProp* prop, CWordArray& typeArray)
{
	for (int i = 0; i < typeArray.GetSize(); i++)
	{
		WORD w = typeArray[i];
		switch(w)
		{
			case DATA_STR_TYPE:
				prop->AddOption(DataType::String.ToString(), TRUE, (DWORD)&DataType::String);
				break;
			case DATA_TXT_TYPE:
				prop->AddOption(DataType::Text.ToString(), TRUE, (DWORD)&DataType::Text);
				break;
			case DATA_INT_TYPE:
				prop->AddOption(DataType::Integer.ToString(), TRUE, (DWORD)&DataType::Integer);
				break;
			case DATA_LNG_TYPE:
				prop->AddOption(DataType::Long.ToString(), TRUE, (DWORD)&DataType::Long);
				prop->AddOption(DataType::ElapsedTime.ToString(), TRUE, (DWORD)&DataType::ElapsedTime);
				prop->AddOption(_TB("Enumerative..."), TRUE, (DWORD)&DataType::Enum);
				break;
			case DATA_ENUM_TYPE:
				prop->AddOption(_TB("Enumerative..."), TRUE, (DWORD)&DataType::Enum);
				break;
			case DATA_GUID_TYPE:
				prop->AddOption(DataType::Guid.ToString(), TRUE, (DWORD)&DataType::Guid);
				break;
			case DATA_MON_TYPE:
				prop->AddOption(DataType::Money.ToString(), TRUE, (DWORD)&DataType::Money);
				break;
			case DATA_QTA_TYPE:
				prop->AddOption(DataType::Quantity.ToString(), TRUE, (DWORD)&DataType::Quantity);
				break;
			case DATA_PERC_TYPE:
				prop->AddOption(DataType::Percent.ToString(), TRUE, (DWORD)&DataType::Percent);
				break;
			case DATA_DBL_TYPE:
				prop->AddOption(DataType::Double.ToString(), TRUE, (DWORD)&DataType::Double);
				break;
			case DATA_BOOL_TYPE:
				prop->AddOption(DataType::Bool.ToString(), TRUE, (DWORD)&DataType::Bool);
				break;
			case DATA_DATE_TYPE:
				prop->AddOption(DataType::Date.ToString(), TRUE, (DWORD)&DataType::Date);
				prop->AddOption(DataType::DateTime.ToString(), TRUE, (DWORD)&DataType::DateTime);
				prop->AddOption(DataType::Time.ToString(), TRUE, (DWORD)&DataType::Time);
				break;
			default:
				ASSERT(FALSE);
		}
	}
}

//--------------------------------------------------------------------------------------
int CompareEnumTag(CObject* po1, CObject* po2)
{
	EnumTag* p1 = (EnumTag*)po1;
	EnumTag* p2 = (EnumTag*)po2;

	return p1->GetTagTitle().CompareNoCase(p2->GetTagTitle());
}

void CRS_ObjectPropertyView::LoadEnumTypeProp(CBCGPProp* prop, CString filter)
{
	prop->RemoveAllOptions();

	filter.Trim();
	if (!filter.IsEmpty())
	{
		filter.Trim('*');
		if (!filter.IsEmpty())
			filter = '*' + filter + '*';
	}

	const EnumTagArray* pTags = GetDocument()->m_pEditorManager->GetEnumsArray();

	for (int i = 0; i < pTags->GetSize(); i++)
	{
		EnumTag* pTag = (*pTags)[i];

		if (!filter.IsEmpty() && !::WildcardMatch(pTag->GetTagTitle(), filter))
			continue;

		prop->AddOption(pTag->GetTagTitle(), TRUE, pTag->GetTagValue());
	}

	if (prop->GetOptionCount()==0)
		prop->AddOption(L"", TRUE, NULL);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTables(CBCGPProp* prop, AddOnModule* pModule, BOOL bExternalTable, CString filter)
{
	prop->RemoveAllOptions();

	filter.Trim();
	if (!filter.IsEmpty())
	{
		filter.Trim('*');
		if (!filter.IsEmpty())
			filter = '*' + filter + '*';
	}

	CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	Array* pTables = NULL;
	if (pModule)
	{	
		ASSERT_VALID(pModule);
		CHelperSqlCatalog::CModuleTables* pMT = pHelperSqlCatalog->FindModuleByTitle(pModule);
		ASSERT_VALID(pMT);

		pTables = &pMT->m_arModTables;
	}

	else if (bExternalTable)
	{
		pTables = &pHelperSqlCatalog->m_arExternalTables;
	}

	else //all
	{
		pTables = &pHelperSqlCatalog->m_arAllTables;
	}

	for (int i = 0; i < pTables->GetSize(); i++)
	{
		CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>((*pTables)[i]);

		if (!filter.IsEmpty() && !::WildcardMatch(pTC->m_pCatalogEntry->m_strTableName, filter))
			continue;

		prop->AddOption(pTC->m_pCatalogEntry->m_strTableName, TRUE, (DWORD_PTR)pTC->m_pCatalogEntry);
	}

	if (prop->GetOptionCount() == 0)
		prop->AddOption(L" ");
}

//-----------------------------------------------------------------------------
int CompareDataStr(CObject* po1, CObject* po2)
{
	DataStr* p1 = (DataStr*)po1;
	DataStr* p2 = (DataStr*)po2;

	return p1->CompareNoCase(*p2);
}

void CRS_ObjectPropertyView::LoadTableModules(CBCGPProp* prop, CString filter)
{
	prop->RemoveAllOptions();
	Array arModules, arTitles;
	arModules.SetOwns(FALSE);
	arTitles.SetCompareFunction(CompareDataStr);
	arTitles.AddAlignArray(&arModules);	//ordiner� anche questo array

	filter.Trim();
	if (!filter.IsEmpty())
	{
		filter.Trim('*');
		if (!filter.IsEmpty())
			filter = '*' + filter + '*';
	}

	for (int a = 0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		for (int m = 0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
			if (!pAddOnMod)
				continue;

			if (pAddOnMod->GetDatabaseRelease() == -1)
				continue;

			if (!filter.IsEmpty() && !WildcardMatch(pAddOnApp->GetTitle() + L" - " + pAddOnMod->GetModuleTitle(), filter))
				continue;

			arTitles.Add( new DataStr(pAddOnApp->GetTitle() + L" - " + pAddOnMod->GetModuleTitle()) );

			arModules.Add(pAddOnMod);
		}
	}

	arTitles.QuickSort();

	if ((!filter.IsEmpty() && WildcardMatch(L"All", filter)) || filter.IsEmpty())
	{
		prop->AddOption(_TB("All"), TRUE, 0);
		//prop->SetValue((LPCTSTR)_TB("All"));
		prop->SelectOption(prop->GetOptionCount() - 1);
	}

	for (int i = 0; i <= arModules.GetUpperBound(); i++)
	{
		DataStr* pds = dynamic_cast<DataStr*>(arTitles[i]);
		ASSERT_VALID(pds);
		prop->AddOption(pds->GetString(), TRUE, (DWORD_PTR)arModules[i]);
	}

	if (prop->GetOptionCount()==0)
		prop->AddOption(L"");

	//Load Tables
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadHotlinkModules(CBCGPProp* prop, DataType retVal/* = DataType::Variant*/)
{
	Array arModules, arTitles;
	arModules.SetOwns(FALSE);
	arTitles.SetCompareFunction(CompareDataStr);
	arTitles.AddAlignArray(&arModules);	//ordiner� anche questo array

	for (int a = 0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		for (int m = 0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
			if (!pAddOnMod)
				continue;

			if (
				!pAddOnMod->m_bIsValid || 
				!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()) ||
				!pAddOnMod->HasHotlinks(retVal) 
				)
				continue;

			arTitles.Add(new DataStr(pAddOnApp->GetTitle() + L" - " + pAddOnMod->GetModuleTitle()));
			arModules.Add(pAddOnMod);
		}
	}

	arTitles.QuickSort();

	for (int i = 0; i <= arModules.GetUpperBound(); i++)
	{
		DataStr* pds = dynamic_cast<DataStr*>(arTitles[i]);
		ASSERT_VALID(pds);
		prop->AddOption(pds->GetString(), TRUE, (DWORD_PTR)arModules[i]);
	}
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadHotlinks(CBCGPProp* prop, AddOnModule* pAddOnMod, DataType retVal /*= DataType::Variant*/)
{
	Array arHotlinks, arTitles;
	arHotlinks.SetOwns(FALSE);
	arTitles.SetCompareFunction(CompareDataStr);
	arTitles.AddAlignArray(&arHotlinks);	//ordiner� anche questo array
										
	//Load hotlink names
	const CBaseDescriptionArray& aObjects = pAddOnMod->m_XmlDescription.GetReferencesInfo().GetHotLinks();
	for (int o = 0; o <= aObjects.GetUpperBound(); o++)
	{
		CHotlinkDescription* pFunct = dynamic_cast<CHotlinkDescription*>(aObjects.GetAt(o));
		ASSERT_VALID(pFunct);
		if (!pFunct->IsPublished()) 
			continue;
		if (retVal != DataType::Variant && retVal != pFunct->GetReturnValueDataType())
			continue;
						
		arTitles.Add(new DataStr(pFunct->GetTitle().IsEmpty() ? pFunct->GetNamespace().GetObjectName() : pFunct->GetTitle()));
		arHotlinks.Add(pFunct);
	}

	arTitles.QuickSort();

	for (int i = 0; i <= arHotlinks.GetUpperBound(); i++)
	{
		DataStr* pds = dynamic_cast<DataStr*>(arTitles[i]);
		ASSERT_VALID(pds);
		prop->AddOption(pds->GetString(), TRUE, (DWORD_PTR)arHotlinks[i]);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSCommonProp::OnUpdateValue()
{
	CString previousValue = GetValue();

	BOOL baseUpdate = CBCGPProp::OnUpdateValue();
	
	//optimization
	CString value = GetValue();

	if (value.Compare(previousValue) == 0)
		return TRUE;

	previousValue = value;

	switch (m_eType)
	{
	case CRSCommonProp::PropType::NEW_NAME:
	{
		CString errMsg = _T("");

		if (CheckPropValue(false, errMsg))
		{
			CString name = this->GetValue();
			m_pPropertyView->m_NewName = name;
		}
		break;
	}

	case CRSCommonProp::PropType::COLUMN_BLOCK_NAME:
	{
		CString errMsg = _T("");
		CheckPropValue(FALSE, errMsg);
		break;
	}

	case CRSCommonProp::PropType::NEW_TYPE:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		m_pPropertyView->m_NewType = *(DataType*)option;
		ASSERT(m_pPropertyView->m_NewType != DataType::Null);

		if (this->m_pChildProp)
		{
			ASSERT_VALID (this->m_pChildProp);
			if (m_pPropertyView->m_NewType == DataType::Enum)
				m_pChildProp->SetVisible(TRUE);
			else
				m_pChildProp->SetVisible(FALSE);
		}

		GetPropertyGrid()->AdjustLayout();
		break;
	}

	case CRSCommonProp::PropType::NEW_ENUMTYPE:
	{	
		int index = GetSelectedOption();
		if (index < 0)
		{	
			m_pPropertyView->LoadEnumTypeProp(this, value);
			m_pPropertyView->m_pPropGrid->SetCurSel(m_pPropertyView->m_pPropGrid->GetProperty(0));
			m_pPropertyView->m_pPropGrid->SetCurSel(this, TRUE);
			if (!this->m_pWndCombo)
				this->DoEdit();
			if (this->m_pWndCombo)
				this->m_pWndCombo->ShowDropDown();
			index = GetSelectedOption();
			if (index < 0)
				break;
		}

		DWORD_PTR option = GetOptionData(index);

		m_pPropertyView->m_NewType = DataType(DATA_ENUM_TYPE, (WORD)option);
		break;
	}

	case CRSCommonProp::PropType::NEW_COLUMN_ENUM:
	{
		int index = GetSelectedOption();
		if (index < 0)
		{
			m_pPropertyView->LoadEnumTypeProp(this, value);
			m_pPropertyView->m_pPropGrid->SetCurSel(m_pPropertyView->m_pPropGrid->GetProperty(0));
			m_pPropertyView->m_pPropGrid->SetCurSel(this, TRUE);
			if (!this->m_pWndCombo)
				this->DoEdit();
			if (this->m_pWndCombo)
				this->m_pWndCombo->ShowDropDown();
		}

		break;
	}

	case CRSCommonProp::PropType::NEW_COLUMN_TYPE:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		DataType type = *(DataType*)option;
		
		if (this->GetSubItemsCount())
		{
			CRSCommonProp* prop = (CRSCommonProp*)this->GetSubItem(0);
		
			if (type == DataType::Enum) 
			{
				if (prop->GetOptionCount() == 0)
					m_pPropertyView->LoadEnumTypeProp(prop);

				prop->SetVisible(TRUE);
			}

			else 
			{
				prop->SetVisible(FALSE);
			}
		}

		m_pPropertyView->m_pPropGrid->AdjustLayout();
		break;
	}

	case CRSCommonProp::PropType::SELECT_TABLE_MODULE_NAME:
	case CRSCommonProp::PropType::NEW_HOTLINK_MODULE_NAME:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		AddOnModule* pModule = dynamic_cast<AddOnModule*>((CObject*)option);
		if (option!=0)
			ASSERT_VALID(pModule);

		ASSERT_VALID(m_pChildProp);
		m_pChildProp->RemoveAllOptions();
		m_pChildProp->SetValue(L"");
		
		if (m_eType == CRSCommonProp::PropType::SELECT_TABLE_MODULE_NAME)
			m_pPropertyView->LoadTables(this->m_pChildProp, pModule);
		else if (m_eType == CRSCommonProp::PropType::NEW_HOTLINK_MODULE_NAME)
			m_pPropertyView->LoadHotlinks(this->m_pChildProp, pModule);

		m_pChildProp->Show();

		m_pPropertyView->m_NewName.Empty();
		break;
	}

	case CRSCommonProp::PropType::SELECT_TABLE_NAME:
	{
		SetColoredState(CrsProp::Normal);

		if (m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItemsCount() > 2)
		{
			CBCGPProp* propColumns = m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(2);

			// clean all properties under the table property
			for (int i = propColumns->GetSubItemsCount() - 1; i > 0; i--)
			{
				CBCGPProp* prop = propColumns->GetSubItem(i);
				propColumns->RemoveSubItem(prop, TRUE);
			}
			m_pPropertyView->m_pPropGrid->GetProperty(0)->RemoveSubItem(propColumns, TRUE);
			m_pPropertyView->m_pPropGrid->AdjustLayout();
		}

		int index = GetSelectedOption();
		CString table = GetValue();
		if (index < 0)
		{
			/*
			//Commentato: Il check del nome non serve: non sto creando la tabella ma sto filtrando fra le esistenti
			CString errMsg = _T("");
			if(!CheckPropValue(FALSE, errMsg))
				break;

			errMsg = _TB("Table name already exists");
			CString description = GetDescription();

			if (GetDocument()->GetEditorManager()->GetDispTable()->Find(table, GetDocument()->m_Objects->m_strLayoutName) >-1)
			{
				SetColoredState(CrsProp::State::Error);
				if (description.Find(errMsg) < 0)
					SetDescription(description + (!description.IsEmpty() ? _T("\n") : _T("")) + errMsg);
				break;
			}
			else if (description.Find(errMsg) < 0)
			{
				description.Replace(errMsg, _T(""));

				if (description.ReverseFind('\n') == description.GetLength() - 1)
					description.Delete(description.GetLength() - 1, 1);
			}*/
		
			//GetModule
			CRSCommonProp* prop = (CRSCommonProp*)GetParent()->GetSubItem(0);
			int indexAddOn = prop->GetSelectedOption();
			AddOnModule* pModule = NULL;
			if (indexAddOn >= 0)
			{
				DWORD_PTR option = prop->GetOptionData(indexAddOn);

				pModule = dynamic_cast<AddOnModule*>((CObject*)option);
			}

			m_pPropertyView->LoadTables(this, pModule, FALSE, table);
			m_pPropertyView->m_pPropGrid->SetCurSel(m_pPropertyView->m_pPropGrid->GetProperty(0));

			if (!this->m_pWndCombo)
				this->DoEdit();
			if (this->m_pWndCombo)
				this->m_pWndCombo->ShowDropDown();

			break;
		}

		// ANASTASIA
		TblRuleData* pTblRuleData = new TblRuleData(*m_pPropertyView->GetDocument()->GetEditorSymTable(), AfxGetDefaultSqlConnection(), table);
		if (!pTblRuleData)
			break;
		CrsProp* columnBlock = new CrsProp(_TB("Columns"));
		m_pPropertyView->m_pPropGrid->GetProperty(0)->AddSubItem(columnBlock);
		m_pPropertyView->m_bAllFieldsAreHidden = TRUE;
		m_pPropertyView->InsertColumnBlock(columnBlock, NULL, table, pTblRuleData);
		// END ANASTASIA

		break;
	}

	case CRSCommonProp::PropType::NEW_VAR_TYPE:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		BOOL res = m_pPropertyView->GetDocument()->CurrentIsTable();
		if (option == 0 && !res) {
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(1))->SetVisible(TRUE);
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(2))->SetVisible(FALSE);
			if (!m_pPropertyView->GetDocument()->CurrentIsTable() && !m_pPropertyView->m_bCreatingNewTable) {

				((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(3))->SetVisible(TRUE);
				((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4))->SetVisible(TRUE);
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->RemoveAllOptions();
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->SetValue((LPCTSTR)L"Normal");
				
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->AddOption(_T("Normal"), TRUE, WoormField::RepFieldType::FIELD_NORMAL);
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->AddOption(_T("Input"), TRUE, WoormField::RepFieldType::FIELD_INPUT);
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->SelectOption(0);
			}

			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(FALSE);
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(6))->SetVisible(TRUE);
		}

		else if (option == 1) {
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(1))->SetVisible(TRUE);
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(2))->SetVisible(FALSE);
			if (!m_pPropertyView->GetDocument()->CurrentIsTable() && !m_pPropertyView->m_bCreatingNewTable) {
				((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(3))->SetVisible(TRUE);
				((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4))->SetVisible(FALSE);
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->RemoveAllOptions();
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->SetValue((LPCTSTR)L"Normal");
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->AddOption(_T("Normal"), TRUE, WoormField::RepFieldType::FIELD_NORMAL);
				m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(4)->SelectOption(0);
			}

			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(FALSE);
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(6))->SetVisible(TRUE);
		}

		m_pPropertyView->m_pPropGrid->AdjustLayout();
		break;
	}

	case CRSCommonProp::PropType::NEW_FIELD_ISHIDDEN :
	{	
		int index = GetSelectedOption();
		if (index < 0)
		break;
		BOOL hidden = (BOOL)GetOptionData(index);
		m_pPropertyView->m_bIsHidden = hidden;

		if (m_pPropertyView->m_fieldType == WoormField::RepFieldType::FIELD_INPUT && hidden)
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(TRUE);
		else
		{
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(FALSE);		
			m_pPropertyView->m_bIsArray = FALSE;
		}

		m_pPropertyView->m_pPropGrid->AdjustLayout();
		break;
	}

	case CRSCommonProp::PropType::NEW_FIELD_TYPE:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		m_pPropertyView->m_fieldType = (WoormField::RepFieldType)option;
		BOOL hidden = m_pPropertyView->m_bIsHidden;
		if (m_pPropertyView->m_fieldType == WoormField::RepFieldType::FIELD_INPUT && hidden)
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(TRUE);
		else
		{  
			((CRSCommonProp*)m_pPropertyView->m_pPropGrid->GetProperty(0)->GetSubItem(5))->SetVisible(FALSE);
			m_pPropertyView->m_bIsArray = FALSE;
		}

		m_pPropertyView->m_pPropGrid->AdjustLayout();

		break;
	}

	/*case CRSCommonProp::PropType::FROM_HIDDEN_FIELD: {
		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);
		WoormField* fld = (WoormField*)option;
		ASSERT(fld);
		m_pPropertyView->m_NewName = fld->GetName();
		break;
	}*/

	case CRSCommonProp::PropType::EXISTING_RULE_OR_NEW_RULE: {

		if (value.IsEmpty())
			break;

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 1) 
		{
			// clean all properties under the table property
			//GetPropertyGrid()->RemovePropertiesAfter(this, FALSE);
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 0; i--) 
			{
				//int count = m_pPropertyView->m_pPropGrid->GetPropertyCount();
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() != 1)
			break;

		int index = GetSelectedOption();
		if (index < 0)
			break;
		DWORD_PTR option = GetOptionData(index);

		if (option == 0) {

			// NEW RULE
			CRSCommonProp* module = new CRSCommonProp(_TB("Module"), L"", PropType::NEW_RULE_MODULE, m_pPropertyView);
			//module->AllowEdit(FALSE);
			m_pPropertyView->LoadTableModules(module);

			CRSCommonProp* table = new CRSCommonProp(_TB("Table"), L"", PropType::NEW_RULE_TABLES, m_pPropertyView);
			//table->AllowEdit(FALSE);
			m_pPropertyView->LoadTables(table, NULL);

			m_pPropertyView->m_pPropGrid->AddProperty(module, TRUE, FALSE);
			m_pPropertyView->m_pPropGrid->AddProperty(table, TRUE, TRUE);
		}

		else {

			CRSCommonProp* tables = new CRSCommonProp(_TB("Table"), L"", PropType::EXISTING_RULE_TABLES, m_pPropertyView);
			//tables->AllowEdit(FALSE);

			TblRuleData* selectedRule = (TblRuleData*)option;
			for (int i = 0; i < selectedRule->m_arSqlTableJoinInfoArray.GetSize(); i++)
			{
				tables->AddOption(((SqlTableInfo*)selectedRule->m_arSqlTableJoinInfoArray[i])->GetTableName(), TRUE, (DWORD_PTR)selectedRule);
			}

			m_pPropertyView->m_pPropGrid->AddProperty(tables, TRUE, TRUE);
		}

		break;
	}

	case CRSCommonProp::PropType::NEW_RULE_MODULE: {

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 2) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 1; i--) {
				int count = m_pPropertyView->m_pPropGrid->GetPropertyCount();
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() != 2)
			break;

		int index = GetSelectedOption();
		
		 //if search
		if (index < 0)
		{	
			m_pPropertyView->LoadTableModules(this, this->GetValue());
			m_pPropertyView->m_pPropGrid->SetCurSel(m_pPropertyView->m_pPropGrid->GetProperty(0));
			m_pPropertyView->m_pPropGrid->SetCurSel(this, TRUE); 
			if (!this->m_pWndCombo)
				this->DoEdit();
			if (this->m_pWndCombo)
				this->m_pWndCombo->ShowDropDown();
			index = GetSelectedOption();
			if (index < 0)
				break;
		}
		
		DWORD_PTR option = GetOptionData(index);

		AddOnModule* module = NULL;
		if (option)
			module = (AddOnModule*)option;

		CRSCommonProp* table = new CRSCommonProp(_TB("Table"), L"", PropType::NEW_RULE_TABLES, m_pPropertyView);
		//table->AllowEdit(FALSE);

		m_pPropertyView->LoadTables(table, module);

		m_pPropertyView->m_pPropGrid->AddProperty(table, TRUE, TRUE);

		break;
	}

	case CRSCommonProp::PropType::NEW_RULE_TABLES: {

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 3) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 2; i--) {
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		int k = GetSelectedOption();

		if (k < 0)
		{
			//get property MODULE
			CRSCommonProp* module = (CRSCommonProp*)GetPropertyGrid()->GetProperty(1);
			int count=module->GetSelectedOption();
			AddOnModule* modData =(AddOnModule*)module->GetOptionData(count);
			m_pPropertyView->LoadTables(this, modData, FALSE, value);
			m_pPropertyView->m_pPropGrid->SetCurSel(m_pPropertyView->m_pPropGrid->GetProperty(0));
			m_pPropertyView->m_pPropGrid->SetCurSel(this,TRUE);
			if (!this->m_pWndCombo)
				this->DoEdit();
			if (this->m_pWndCombo)
			   this->m_pWndCombo->ShowDropDown();
				
			break;
		}

		TblRuleData* pTblRuleData = new TblRuleData(*m_pPropertyView->GetDocument()->GetEditorSymTable(), AfxGetDefaultSqlConnection(), value);
		CrsProp* columnBlock = new CrsProp(_TB("Columns"));
		m_pPropertyView->m_pPropGrid->AddProperty(columnBlock, TRUE, TRUE);

		m_pPropertyView->InsertColumnBlock(columnBlock, NULL, value, pTblRuleData);

		break;
	}

	case CRSCommonProp::PropType::EXISTING_RULE_TABLES: {

		if (value.IsEmpty())
			break;
		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 2) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 1; i--) {
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() != 2)
			break;

		int index = GetSelectedOption();
		if (index < 0)
			break;

		DWORD_PTR option = GetOptionData(index);
		TblRuleData* tblRuleData = (TblRuleData*)option;
		CrsProp* columnBlock = new CrsProp(_TB("Columns"));
		m_pPropertyView->m_pPropGrid->AddProperty(columnBlock, TRUE, TRUE);
		m_pPropertyView->InsertColumnBlock(columnBlock, NULL, value, tblRuleData);

		break;
	}

	case CRSCommonProp::PropType::NEW_HYPERLINK: {

		if (value.IsEmpty())
			break;

		int index = GetSelectedOption();
		if (index < 0)
			break;
		// check if the extra property for url was added (subtype)
		WoormLink::WoormLinkType linkType = (WoormLink::WoormLinkType)GetOptionData(index);

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 2) {
			// clean all properties under the type property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 1; i--) {
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		// if type is url i need to add an extra property for sub type
		if (linkType == WoormLink::WoormLinkType::ConnectionURL) {
			CrsProp* subType = new CrsProp(_TB("Subtype"), (LPCTSTR)GetLinkSubTypeString(WoormLink::WoormLinkSubType::File),L"",WoormLink::WoormLinkSubType::File);
			subType->AllowEdit(FALSE);
			subType->AddOption(GetLinkSubTypeString(WoormLink::WoormLinkSubType::File), TRUE, WoormLink::WoormLinkSubType::File);
			subType->AddOption(GetLinkSubTypeString(WoormLink::WoormLinkSubType::Url), TRUE, WoormLink::WoormLinkSubType::Url);
			subType->AddOption(GetLinkSubTypeString(WoormLink::WoormLinkSubType::MailTo), TRUE, WoormLink::WoormLinkSubType::MailTo);
			subType->AddOption(GetLinkSubTypeString(WoormLink::WoormLinkSubType::CallTo), TRUE, WoormLink::WoormLinkSubType::CallTo);
			subType->AddOption(GetLinkSubTypeString(WoormLink::WoormLinkSubType::GoogleMap), TRUE, WoormLink::WoormLinkSubType::GoogleMap);
			m_pPropertyView->m_pPropGrid->AddProperty(subType, TRUE, TRUE);
		}

		CRSCommonProp* hFlag = new  CRSCommonProp(_TB("Link object by variable"), L"", CRSCommonProp::PropType::NEW_HYPERLINK_VAR_FLAG, m_pPropertyView);
		hFlag->AllowEdit(FALSE);
		hFlag->SetDescription(_TB("The link target contains a variable value"));
		hFlag->AddOption(L"False", TRUE, 0);
		hFlag->AddOption(L"True", TRUE, 1);
		hFlag->SelectOption(hFlag->GetOptionDataIndex(0));
		m_pPropertyView->m_pPropGrid->AddProperty(hFlag, TRUE, TRUE);

		if (linkType == WoormLink::WoormLinkType::ConnectionFunction)
			m_pPropertyView->NewFunctionLink();
			
		if (linkType == WoormLink::WoormLinkType::ConnectionForm)
			m_pPropertyView->NewFormLink();
			
		if (linkType == WoormLink::WoormLinkType::ConnectionReport)
			m_pPropertyView->NewReportLink();
		
		if (linkType == WoormLink::WoormLinkType::ConnectionURL)
			m_pPropertyView->NewURLLink();

		break;
	}
	
	case CRSCommonProp::PropType::NEW_HYPERLINK_VAR_FLAG: 
	{
		if (value.IsEmpty())
			break;

		int index = m_pPropertyView->m_pPropGrid->GetProperty(1)->GetSelectedOption();
		if (index < 0)
			break;
		// check if the extra property for url was added (subtype)
		WoormLink::WoormLinkType linkType = (WoormLink::WoormLinkType)m_pPropertyView->m_pPropGrid->GetProperty(1)->GetOptionData(index);
		int count = linkType == WoormLink::WoormLinkType::ConnectionURL ? 4 : 3;

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > count) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > count-1; i--) {
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}
		
		if (linkType == WoormLink::WoormLinkType::ConnectionFunction)
		{ 
			m_pPropertyView->NewFunctionLink();
			break;
		}
		
		if (linkType == WoormLink::WoormLinkType::ConnectionForm)
		{
			m_pPropertyView->NewFormLink();
			break;
		}

		if (linkType == WoormLink::WoormLinkType::ConnectionReport)
		{
			m_pPropertyView->NewReportLink();
			break;
		}

		if (linkType == WoormLink::WoormLinkType::ConnectionURL)
		{
			m_pPropertyView->NewURLLink();
			break;
		}

		break;
	}

	/*case CRSCommonProp::LINK_REPORT_PARAM	:
	{
		if (value.IsEmpty())
			break;
		CString nameSpace = m_pPropertyView->m_pPropGrid->GetProperty(3)->GetValue();
		if (nameSpace.IsEmpty())
			break;
		m_pPropertyView->InsertReportLinkParameterBlock(nameSpace, this);
		break;
	}*/
	}

	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////

CRSTblRuleProp::CRSTblRuleProp(TblRuleData* pRule, CString name, LPCTSTR value, PropType eType, CRS_ObjectPropertyView* propertyView)
: CrsProp(name, value)
{
	this->m_eType = eType;
	this->m_pPropertyView = propertyView;

	this->m_pTblRule = pRule;
	ASSERT_VALID(pRule);
}

//-----------------------------------------------------------------------------
CRSTblRuleProp::CRSTblRuleProp(TblRuleData* pRule, CString name, variant_t value, PropType eType, CRS_ObjectPropertyView* propertyView)
: CrsProp(name, value)
{
	this->m_eType = eType;
	this->m_pPropertyView = propertyView;

	this->m_pTblRule = pRule;
	ASSERT_VALID(pRule);
}

//-----------------------------------------------------------------------------
BOOL CRSTblRuleProp::OnUpdateValue()
{
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	ASSERT_VALID(m_pTblRule);

	switch (m_eType)
	{
	case CRSTblRuleProp::PropType::JOIN_TYPE:
		{
			int index = GetSelectedOption();
			if (index < 0)
				return FALSE;
			DWORD_PTR option = GetOptionData(index);
			
			const SqlTableInfo* pTI = dynamic_cast<const SqlTableInfo*>(this->m_pPropertyView->m_pTreeNode->m_pItemData);
			ASSERT_VALID(pTI);

			int pos = m_pTblRule->m_arSqlTableJoinInfoArray.Find(pTI);
			ASSERT(pos >= 0);

			if (option == SqlTableJoinInfoArray::EJoinType::INNER)
			{
				m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::INNER;
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::LEFT_OUTER)
			{
				m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::LEFT_OUTER;
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER)
			{
				m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER;
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::FULL_OUTER)
			{
				m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::FULL_OUTER;
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::CROSS;
			}

			else
			{
				ASSERT(FALSE);
				return FALSE;
			}

			if (
					option == SqlTableJoinInfoArray::EJoinType::INNER
					||
					option == SqlTableJoinInfoArray::EJoinType::LEFT_OUTER
					||
					option == SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER
					||
					option == SqlTableJoinInfoArray::EJoinType::FULL_OUTER
				)
			{
				if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos] == NULL)
				{
					WClause* pJoinOn = new WClause(m_pTblRule->GetConnection(), m_pTblRule->GetSymTable(), &m_pTblRule->m_arSqlTableJoinInfoArray);
					pJoinOn->SetJoinOnClause();
					m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos] = pJoinOn;
				}

				CRSTreeCtrl* pTreeCtrl = GetDocument()->GetRSTree(ERefreshEditor::Rules);
				if (pTreeCtrl && !pTreeCtrl->ItemHasChildren(this->m_pPropertyView->m_pTreeNode->m_ht))
				{
					pTreeCtrl->AddNode(L"On", CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON, this->m_pPropertyView->m_pTreeNode->m_ht, m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos], m_pTblRule);
				}
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				//TODO
				// if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos])  delete and set NULL
				//remove tree child
			}

			else if (option == SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				//TODO
				// if (m_pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos])  delete and set NULL
				//remove tree child
			}

			break;
		}

	case CRSTblRuleProp::PropType::SELECT_CONSTRAINT:
		{
			int index = GetSelectedOption();
			if (index < 0)
				return FALSE;
			DWORD_PTR option = GetOptionData(index);

			if (option == RuleSelectionMode::SEL_ALL)
			{
				m_pTblRule->m_ConstraintMode = RuleSelectionMode::SEL_ALL;
			}

			else if (option == RuleSelectionMode::SEL_NULL)
			{
				m_pTblRule->m_ConstraintMode = RuleSelectionMode::SEL_NULL;
			}

			else if (option == RuleSelectionMode::SEL_NOT_NULL)
			{
				m_pTblRule->m_ConstraintMode = RuleSelectionMode::SEL_NOT_NULL;
			}

			else
			{
				ASSERT(FALSE);
				return FALSE;
			}

			break;
		}

	case CRSTblRuleProp::PropType::DISTINCT:
		{
			ASSERT_VALID(m_pTblRule);

			bool b = this->GetValue();

			this->m_pTblRule->SetDistinct(b);
			break;
		}

	case CRSTblRuleProp::PropType::TOP:
		{
			ASSERT_VALID(m_pTblRule);
			
			int nTop = this->GetValue();

			this->m_pTblRule->m_nTop = nTop;
			break;
		}
	}

	GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_pPropertyView->m_pTreeNode);

	return TRUE;
}
///////////////////////////////////////////////////////////////////////////////

CRSNamedQueryRuleProp::CRSNamedQueryRuleProp(QueryRuleData* pRule, CString name, LPCTSTR value, CRS_ObjectPropertyView* propertyView)
	: CrsProp(name, value)
{
	this->m_pPropertyView = propertyView;

	this->m_pRule = pRule;
	ASSERT_VALID(pRule);
}

//-----------------------------------------------------------------------------
CRSNamedQueryRuleProp::CRSNamedQueryRuleProp(QueryRuleData* pRule, CString name, variant_t value, CRS_ObjectPropertyView* propertyView)
	: CrsProp(name, value)
{
	this->m_pPropertyView = propertyView;

	this->m_pRule = pRule;
	ASSERT_VALID(pRule);
}

//-----------------------------------------------------------------------------
BOOL CRSNamedQueryRuleProp::OnUpdateValue()
{
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	ASSERT_VALID(m_pRule);

	int index = GetSelectedOption();
	if (index < 0)
		return FALSE;
	DWORD_PTR option = GetOptionData(index);

	if (option == RuleSelectionMode::SEL_ALL)
	{
		m_pRule->m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	}
	else if (option == RuleSelectionMode::SEL_NULL)
	{
		m_pRule->m_ConstraintMode = RuleSelectionMode::SEL_NULL;
	}
	else if (option == RuleSelectionMode::SEL_NOT_NULL)
	{
		m_pRule->m_ConstraintMode = RuleSelectionMode::SEL_NOT_NULL;
	}
	else
	{
		m_pRule->m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	}

	GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_pPropertyView->m_pTreeNode);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewJoinTablePropertyGrid(CNodeTree* pNode)
{
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	CrsProp* pPropGeneral = new CrsProp(_TB("Add join table/view"));

	CRSCommonProp* propModules = new CRSCommonProp(_TB("Module"), L"", CRSCommonProp::PropType::SELECT_TABLE_MODULE_NAME, this, _TB("Select the Module that the table belong to"));
	//propModules->AllowEdit(FALSE);

	CRSCommonProp* propTables = new CRSCommonProp(_TB("Table"), L"", CRSCommonProp::PropType::SELECT_TABLE_NAME, this, _TB("Select the name of table or start to write it."));
	LoadTables(propTables, NULL);

	propModules->m_pChildProp = propTables;

	pPropGeneral->AddSubItem(propModules);
	pPropGeneral->AddSubItem(propTables);

	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewJoinTable()
{
	CrsProp* tableProp = (CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(1);

	if (!tableProp)
	{
		tableProp->SetColoredState(CrsProp::State::Error);
		return;
	}

	int index = tableProp->GetSelectedOption();
	if (index < 0)
	{
		tableProp->SetColoredState(CrsProp::State::Error);
		return;
	}

	DWORD_PTR option = tableProp->GetOptionData(index);
	SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>((CObject*)option);
	if (!pCatEntry)
	{
		tableProp->SetColoredState(CrsProp::State::Error);
		return;
	}

	CString tableName = pCatEntry->m_strTableName;

	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	if (!pCatEntry->m_pTableInfo)
	{
		ASSERT(FALSE);
		return;
	}

	int size = pTblRule->m_arSqlTableJoinInfoArray.GetSize();
	CString sTargetTableName;
	
	if (!m_pTreeNode)
		return;

	for (int i = 0;i < size;i++)
	{
		if (m_pTreeNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
		else if (m_pTreeNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM)
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
		else if (m_pTreeNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO)
		{
			SqlTableInfo* pTI = dynamic_cast<SqlTableInfo*>(m_pTreeNode->m_pItemData);
			ASSERT_VALID(pTI);
			if (!pTI)
				sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
			else
				sTargetTableName = pTI->GetTableName();
		}

		const  SqlCatalogEntry* entry = pCatEntry;

		CHelperSqlCatalog::CTableColumns* pTargetTC = NULL;
		CHelperSqlCatalog::CTableForeignTablesKeys* pFTK = NULL;
		CHelperExternalReferences::CTableSingleExtRef* pSER = NULL;

		GetDocument()->GetWoormFrame()->GetEngineTreeView()->FindJoinReferences
			(
				tableName,
				sTargetTableName,
				pTargetTC,
				pFTK,																		
				pSER
				);

		GetDocument()->GetWoormFrame()->GetEngineTreeView()->AddJoin(pTblRule, pCatEntry->m_pTableInfo, tableName, sTargetTableName, pFTK, pSER, FALSE);
	}

	// ANASTASIA 
	CBCGPProp* columnsProp = m_pPropGrid->GetProperty(0)->GetSubItem(2);
	BOOL bQualified = pTblRule->m_arSqlTableJoinInfoArray.GetSize() > 1;

	for (int i = 0; i < columnsProp->GetSubItemsCount(); i++)
	{
		CRSChkDBCol* pColProp = dynamic_cast<CRSChkDBCol*>(columnsProp->GetSubItem(i));
		ASSERT_VALID(pColProp);

		if (!pColProp->IsVisible())
			continue;

		BOOL ifChecked = pColProp->GetValue();
		if (!ifChecked)
			continue;

		//Variable name
		CrsProp* propVarName = dynamic_cast<CrsProp*>(pColProp->GetSubItem(0));
		ASSERT_VALID(propVarName);

		CString errMsg = _T("");
		if (!propVarName->CheckPropValue(FALSE, errMsg))
		{
			if (!errMsg.IsEmpty())
				AfxMessageBox(errMsg);
			return;
		}

		const SqlColumnInfo* currentColumn = (SqlColumnInfo*)((CObject*)propVarName->GetData());
		ASSERT_VALID(currentColumn);

		// new DataType
		CRSCommonProp* propType = dynamic_cast<CRSCommonProp*>(pColProp->GetSubItem(1));
		ASSERT_VALID(propType);
		int index = propType->GetSelectedOption();
		if (index < 0)
		{
			propType->SetColoredState(CrsProp::Error);
			return;
		}

		else
			propType->SetColoredState(CrsProp::Normal);

		DataType dType = *(DataType*)propType->GetOptionData(index);
		if (dType == DATA_ENUM_TYPE)
		{
			// new DataType
			CRSCommonProp* propSubType = dynamic_cast<CRSCommonProp*>(propType->GetSubItem(0));
			ASSERT_VALID(propSubType);

			index = propSubType->GetSelectedOption();
			if (index < 0)
			{
				propSubType->SetColoredState(CrsProp::Error);
				return;
			}

			else
				propSubType->SetColoredState(CrsProp::Normal);

			WORD enumType = (WORD)propSubType->GetOptionData(index);
			dType = DataType(DATA_ENUM_TYPE, enumType);
		}

		CString name = propVarName->GetValue();
		CString phName = bQualified ? currentColumn->GetQualifiedColumnName() : currentColumn->GetColumnName();

		/*DataFieldLink* pFL = */pTblRule->AddLink(phName, name, FALSE, dType, pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetUpperBound());

		WoormTable*	pSymTable = pTblRule->GetSymTable();
		WoormField* pRepField = new WoormField(name, WoormField::RepFieldType::FIELD_NORMAL, dType);
		pRepField->SetHidden(TRUE);
		pRepField->SetPrecision(
			AfxGetFormatStyleTable()->GetInputCharLen(dType, &GetDocument()->GetNamespace()),
			currentColumn->GetColumnDecimal());
		pRepField->SetTableRuleField(TRUE);
		pRepField->SetSpecialField(currentColumn->m_bSpecial);
		pRepField->SetPhysicalName(phName);
		pSymTable->Add(pRepField);
		GetDocument()->SyncronizeViewSymbolTable(pRepField);
		
		//m_pPropGrid->DeleteProperty(pColProp, TRUE, TRUE); i--;
		//pColProp->SetVisible(FALSE);
		m_pPropGrid->AdjustLayout();
	}

	// END ANASTASIA

	m_bNeedsApply = FALSE;
	m_bAllFieldsAreHidden = FALSE;

	HTREEITEM ht = GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pTblRule);
	if (ht)
	{
		CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
		ht = pTree->SelectRSTreeItemData(pCatEntry->m_pTableInfo, ht);
		if (ht)
			ht = pTree->FindItemText(_TB("Unselected Columns"), ht);
		if (ht)
		{
			pTree->SelectItem(ht);
			pTree->Expand(ht, TVE_EXPAND);
			pTree->EnsureVisible(ht);
		}
	}

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewCalcColumnPropertyGrid(CNodeTree* pNode)
{
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	CrsProp* pPropGeneral = new CrsProp(_TB("Add calculated column"));

	CRSCommonProp* propName = new CRSCommonProp(_TB("Name"), (LPCTSTR)L"", CRSCommonProp::PropType::NEW_NAME, this);
	propName->SetColoredState(CrsProp::Mandatory);
	pPropGeneral->AddSubItem(propName);

	m_NewType = DataType::String;
	CRSCommonProp* prop = new CRSCommonProp( _TB("Data type"), DataType::String.ToString(), CRSCommonProp::PropType::NEW_TYPE, this);
	pPropGeneral->AddSubItem(prop);

	CRSCommonProp* propEnumsType = new CRSCommonProp(_TB("Enumerative subtype"), L"", CRSCommonProp::PropType::NEW_ENUMTYPE, this);
	pPropGeneral->AddSubItem(propEnumsType);
	propEnumsType->SetVisible(FALSE);

	prop->m_pChildProp = propEnumsType;
	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewCalcColumn()
{
	ASSERT_VALID(m_pTreeNode);
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->CheckPropValue(FALSE, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}

	m_NewName.Trim();
	
	WoormTable*	pSymTable = GetDocument()->GetEditorSymTable();

	if (m_NewType == DataType::Array || m_NewType == DataType::Null)
	{
		AfxMessageBox(_TB("Field name has wrong data type"));
		return;
	}

	WoormField* pRepField = new WoormField(m_NewName);
		pRepField->SetDataType(m_NewType);
		pRepField->SetHidden(TRUE);	//per ora � hidden
		pRepField->SetNativeColumnExpr(TRUE);
		pRepField->SetTableRuleField(TRUE);
		pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(m_NewType, &GetDocument()->GetNamespace()));
	pSymTable->Add(pRepField);
	//per ora � hidden quindi non serve GetDocument()->SyncronizeViewSymbolTable(pSymTable);

	//creo una espressione SQL minimale: sar� poi modificata tramite il tree
	CString sVal = m_NewType.FormatDefaultValue();
	ASSERT(!sVal.IsEmpty());
	if (sVal[0] == '"') 
	{ 
		sVal = sVal.Mid(1); 
		sVal = sVal.Left(sVal.GetLength()-1); 
	}

	DataObj* pObj = DataObj::DataObjCreate(m_NewType);
	ASSERT_VALID(pObj);
	pObj->Assign(sVal);
	sVal = pTblRule->GetConnection()->NativeConvert(pObj);
	SAFE_DELETE(pObj);
	//----

	DataFieldLink* pFL = pTblRule->AddLink(sVal, m_NewName, TRUE, m_NewType);

	m_bNeedsApply = FALSE;

	HTREEITEM htGroupCalcCol = m_pTreeNode->m_ht;
	GetDocument()->RefreshRSTree(ERefreshEditor::Variables);

	CRSTreeCtrl* pTreeCtrl = GetDocument()->GetRSTree(ERefreshEditor::Rules);
	
	CNodeTree& nt = pTreeCtrl->AddNode(pRepField->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htGroupCalcCol, pRepField, pSymTable, pTblRule);
	VERIFY(pTreeCtrl->SelectItem(nt));

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadNamedQueryRulePropertyGrid(CNodeTree* pNode)
{
	QueryRuleData* pRule = dynamic_cast<QueryRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pRule);
	if (!pRule) return;

	CrsProp* pPropGeneral = new CrsProp(_TB("Named Query Rule"));

	SetPanelTitle(_TB("Rule - ") + (LPCTSTR)pRule->GetQueryName());

	CrsProp* prop = new CrsProp(_TB("Name"), (LPCTSTR)pRule->GetQueryName());
	prop->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(prop);

	CRSNamedQueryRuleProp* propTables = new CRSNamedQueryRuleProp(pRule, _TB("Select constraint"), pRule->GetSelectionModeDescr(), this);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_ALL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_ALL);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_NOT_NULL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_NOT_NULL);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_NULL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_NULL);
		propTables->SelectOption(pRule->m_ConstraintMode);
		propTables->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(propTables);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}
//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadTableRulePropertyGrid(CNodeTree* pNode)
{
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	CrsProp* pPropGeneral = new CrsProp(_TB("Query Rule"));

	SetPanelTitle(_TB("Rule - ") + (LPCTSTR)pTblRule->m_arSqlTableJoinInfoArray.Unparse());

	CrsProp* prop = new CrsProp(_TB("Name"), (LPCTSTR)pTblRule->m_arSqlTableJoinInfoArray.Unparse());
	prop->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(prop);

	CRSTblRuleProp* propTables = new CRSTblRuleProp(pTblRule, _TB("Select constraint"), pTblRule->GetSelectionModeDescr(), CRSTblRuleProp::SELECT_CONSTRAINT, this);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_ALL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_ALL);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_NOT_NULL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_NOT_NULL);
		propTables->AddOption(TblRuleData::GetSelectionModeDescr(RuleSelectionMode::SEL_NULL), TRUE, (DWORD_PTR)RuleSelectionMode::SEL_NULL);
		propTables->SelectOption(pTblRule->m_ConstraintMode);
		propTables->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(propTables);

	propTables = new CRSTblRuleProp(pTblRule, _T("DISTINCT"), (bool)(pTblRule->m_bDistinct ? True : False), CRSTblRuleProp::PropType::DISTINCT, this);
	pPropGeneral->AddSubItem(propTables);
	propTables = new CRSTblRuleProp(pTblRule, _T("TOP"), pTblRule->m_nTop, CRSTblRuleProp::PropType::TOP, this);
	pPropGeneral->AddSubItem(propTables);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadJoinTablePropertyGrid(CNodeTree* pNode)
{
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_pTreeNode->m_pParentItemData);
	ASSERT_VALID(pTblRule);

	const SqlTableInfo* pTI = dynamic_cast<const SqlTableInfo*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pTI);

	int pos = pTblRule->m_arSqlTableJoinInfoArray.Find(pTI);
	ASSERT(pos >= 0);

	if (pos == 0)
		return; //La prima tabella non pu� avere join

	SqlTableJoinInfoArray::EJoinType jt = pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos];

	CString sJT; 
	switch (jt)
	{
		case SqlTableJoinInfoArray::EJoinType::INNER:
			sJT = L"Inner Join";
			break;
		case SqlTableJoinInfoArray::EJoinType::LEFT_OUTER:
			sJT = L"Left Outer Join";
			break;
		case SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER:
			sJT = L"Right Outer Join";
			break;
		case SqlTableJoinInfoArray::EJoinType::FULL_OUTER:
			sJT = L"Full Outer Join";
			break;
		case SqlTableJoinInfoArray::EJoinType::CROSS:
			sJT = L"Cross Join";
			break;
		default:
			ASSERT(FALSE);
	}
	
	CrsProp* pPropGeneral = new CrsProp(_TB("Joined Table"));

	CRSTblRuleProp* propTables = new CRSTblRuleProp(pTblRule, _TB("Join"), sJT, CRSTblRuleProp::JOIN_TYPE, this);
		propTables->AddOption(L"Inner Join", TRUE, (DWORD_PTR)SqlTableJoinInfoArray::EJoinType::INNER);
		propTables->AddOption(L"Left Outer Join", TRUE, (DWORD_PTR)SqlTableJoinInfoArray::EJoinType::LEFT_OUTER);
		propTables->AddOption(L"Right Outer Join", TRUE, (DWORD_PTR)SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER);
		propTables->AddOption(L"Full Outer Join", TRUE, (DWORD_PTR)SqlTableJoinInfoArray::EJoinType::FULL_OUTER);
		propTables->AddOption(L"Cross Join", TRUE, (DWORD_PTR)SqlTableJoinInfoArray::EJoinType::CROSS);
		propTables->AllowEdit(FALSE);
		pPropGeneral->AddSubItem(propTables);
	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadColumnPropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("TODO column"));

	//TODO datatype

	DataFieldLink*	pDatalink = dynamic_cast<DataFieldLink*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pDatalink);

	CrsProp* pProp = new CrsProp(_TB("Name"), (LPCTSTR)pDatalink->m_strPhysicalName);
	pProp->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadCalcColumnPropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Calculated column"));

	DataFieldLink*	pDatalink = dynamic_cast<DataFieldLink*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pDatalink);

	//TODO datatype, expression

	CrsProp* pProp = new CrsProp(_TB("Name"), (LPCTSTR)pDatalink->m_strPhysicalName);
	pProp->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadProcedurePropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Add Procedure"));

	ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pProc);

	//TODO rename return-type e parameters

	CrsProp* pProp = new CrsProp(_TB("Name"), (LPCTSTR)pProc->GetName());
	pProp->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(pProp);

	if (pProc->m_pProcedure->GetFun())
	{
		pProp = new CrsProp(_TB("Return Type"), (LPCTSTR)pProc->m_pProcedure->GetFun()->GetReturnValueDataType().ToString());
		pProp->AllowEdit(FALSE);
		pPropGeneral->AddSubItem(pProp);
	}

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::LoadNamedQueryPropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Named Query"));

	QueryObjItem* pQuery = dynamic_cast<QueryObjItem*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pQuery);
	if (!pQuery) return;
	
	//TODO rename 

	CrsProp* pProp = new CrsProp(_TB("Name"), (LPCTSTR)pQuery->GetName());
	pProp->AllowEdit(FALSE);
	pPropGeneral->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewProcedurePropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Add Procedure"));

	CRSCommonProp* pProp = new CRSCommonProp(_TB("Name"), (LPCTSTR)L"", CRSCommonProp::PropType::NEW_NAME, this);
	pProp->SetColoredState(CrsProp::State::Mandatory);
	pPropGeneral->AddSubItem(pProp);
/* TODO mancano parametri
	//----
	m_NewType = DataType::Void;
	CRSCommonProp* propT = new CRSCommonProp(_TB("Return data type"), DataType::String.ToString(), CRSCommonProp::PropType::NEW_TYPE, this);
		propT->InsertOption(0, _TB("<none>"), TRUE, (DWORD_PTR)&DataType::Void);
		propT->SelectOption(0);
	pPropGeneral->AddSubItem(propT);

	CRSCommonProp* propEnumsType = new CRSCommonProp(_TB("Enumerative subtype"), L"", CRSCommonProp::PropType::NEW_ENUMTYPE, this);
	propEnumsType->SetVisible(FALSE);
	pPropGeneral->AddSubItem(propEnumsType);

	propT->m_pChildProp = propEnumsType;
	//----
*/
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewProcedure()
{
	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->CheckPropValue(FALSE, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}

	m_NewName.Trim();

	ProcedureData*	pProcedures = dynamic_cast<ProcedureData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pProcedures);

	if (pProcedures->Get(m_NewName))
	{
		AfxMessageBox(_TB("A Procedure with the same name already exists"));
		return;
	}

	ProcedureObjItem* pNewProcObj = new ProcedureObjItem(pProcedures->GetSymTable(), m_NewName);

	pProcedures->AddNew(pNewProcObj);

//TODO valore di ritorno e parametri

	m_bNeedsApply = FALSE;

	CRSTreeCtrl* pTreeCtrl = GetDocument()->GetRSTree(ERefreshEditor::Procedures);
	HTREEITEM htProc = pTreeCtrl->AddNode(m_NewName/* + L"..."*/, CNodeTree::ENodeType::NT_PROCEDURE, m_pTreeNode->m_ht, pNewProcObj, pProcedures);
	VERIFY(pTreeCtrl->SelectItem(htProc));

	GetDocument()->GetWoormFrame()->GetEngineTreeView()->OnOpenEditor();

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewNamedQueryPropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Add named Query"));
	//----

	CRSCommonProp* pProp = new CRSCommonProp(_TB("Name"), (LPCTSTR)L"", CRSCommonProp::PropType::NEW_NAME, this);
	pProp->SetColoredState(CrsProp::State::Mandatory);
	pPropGeneral->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewNamedQuery()
{
	CString errMsg = _T("");
	if (!((CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(0))->CheckPropValue(FALSE, errMsg))
	{
		if (!errMsg.IsEmpty())
			AfxMessageBox(errMsg);
		return;
	}

	m_NewName.Trim();
	
	QueryObjectData* pQueries = dynamic_cast<QueryObjectData*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pQueries);

	if (pQueries->Get(m_NewName))
	{
		AfxMessageBox(_TB("A Query with the same name already exists"));
		return;
	}

	QueryObjItem* pNewQryObj = new QueryObjItem(pQueries->GetSymTable(), m_NewName);

	pQueries->AddNew(pNewQryObj);

	m_bNeedsApply = FALSE;

	CRSTreeCtrl* pTreeCtrl = GetDocument()->GetRSTree(ERefreshEditor::Queries);
	HTREEITEM htQuery = pTreeCtrl->AddNode(m_NewName /*+ L"..."*/, CNodeTree::ENodeType::NT_NAMED_QUERY, m_pTreeNode->m_ht, pNewQryObj, pQueries);
	VERIFY(pTreeCtrl->SelectItem(htQuery));

	GetDocument()->GetWoormFrame()->GetEngineTreeView()->OnOpenEditor();
	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewLayoutPropertyGrid(CNodeTree* pNode)
{
	CrsProp* pPropGeneral = new CrsProp(_TB("Add Layout"));
	//----

	CRSCommonProp* pProp = new CRSCommonProp(_TB("Name"), (LPCTSTR)L"", CRSCommonProp::PropType::NEW_NAME, this);
	pProp->SetColoredState(CrsProp::State::Mandatory);
	pPropGeneral->AddSubItem(pProp);

	//----
	m_pPropGrid->AddProperty(pPropGeneral);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewLayout()
{
	m_NewName.Trim();
	if (m_NewName.IsEmpty())
		return;

	if (!IsValidName(m_NewName))
	{
		AfxMessageBox(_TB("Name is invalid! The name can contain only letters, numbers and a '_'. The name cannot start with a number"));
		return;
	}

	CMultiLayouts*	pLayouts = dynamic_cast<CMultiLayouts*>(m_pTreeNode->m_pItemData);
	ASSERT_VALID(pLayouts);

	if (pLayouts->Lookup(m_NewName))
	{
		AfxMessageBox(_TB("A Layout with the same name already exists"));
		return;
	}

	CLayout* pNewLayout = pLayouts->Add(m_NewName);

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pNewLayout);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewFunctionLink()
{
	int index = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (index < 0)
		return;
	m_bNeedsApply = TRUE;
	BOOL ifLObVChecked = m_pPropGrid->GetProperty(2)->GetOptionData(index);
	CrsProp* modules;
	
	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_FUNCTION_LINK;

	if (!ifLObVChecked)
	{
		CString sTitle;
		CStringArray orderedArray;
		modules = new  CRSNewLinkFunction(_TB("Module"), L"", this, CRSNewLinkFunction::FieldType::MODULE);
		modules->AllowEdit(FALSE);

		for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
		{
			AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
			ASSERT(pAddOnApp);

			if (!pAddOnApp)
				continue;

			for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
			{
				AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
				ASSERT(pAddOnMod);

				if (
					!pAddOnMod || !pAddOnMod->m_bIsValid ||
					!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName())
					)
					continue;
				orderedArray.Add(pAddOnMod->GetModuleTitle());
			}

			CStringArray_Sort(orderedArray);
			BOOL found = FALSE;
			for (int k = 0;k < orderedArray.GetCount();k++)
			{
				for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
				{
					AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
					ASSERT(pAddOnMod);

					if (pAddOnMod->GetModuleTitle().CompareNoCase(orderedArray[k]) == 0)
					{
						sTitle = cwsprintf(_T("%s - %s"), pAddOnApp->GetTitle(), pAddOnMod->GetModuleTitle());
						modules->AddOption(sTitle, TRUE, (DWORD_PTR)pAddOnMod);
						break;
					}
				}
			}

			orderedArray.RemoveAll();
		}

		m_pPropGrid->AddProperty(modules, TRUE, TRUE);
	}

	else
	{
		modules = new  CrsProp(_TB("Variable contains link value"), L"");
		modules->AllowEdit(FALSE);
		LoadVariablesIntoProperty(modules);
		m_pPropGrid->AddProperty(modules, TRUE, TRUE);
		CrsProp* paramGroup = new CrsProp(_TB("Add parameters"));
		m_pPropGrid->AddProperty(paramGroup);
		InsertLinkParameterBlock(NULL, FALSE);
	}
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewFunctionLink()
{
	// var name associated with field
	CString owner = m_pPropGrid->GetProperty(0)->GetValue();

	//alias
	DWORD alias = m_pPropGrid->GetProperty(0)->GetData();

	//flag Linked By Variable
	int flagIndex = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (flagIndex < 0) {
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Important);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Normal);

	BOOL linkedByVar = m_pPropGrid->GetProperty(2)->GetOptionData(flagIndex);

	// variable name or namespace
	CFunctionDescription* functionDescr;
	CString target=L"";
	WoormLink* jj = new WoormLink(&(GetDocument()->m_ViewSymbolTable));
	
	if (!linkedByVar) {
		//function description
		int functionIndex = m_pPropGrid->GetProperty(4)->GetSelectedOption();
		if (functionIndex < 0)
		{
			((CrsProp*)m_pPropGrid->GetProperty(4))->SetColoredState(CrsProp::Important);
			return;
		}

		else
			((CrsProp*)m_pPropGrid->GetProperty(4))->SetColoredState(CrsProp::Normal);
			
		functionDescr = (CFunctionDescription*)m_pPropGrid->GetProperty(4)->GetOptionData(functionIndex);
		WoormField* pField = jj->m_pLocalSymbolTable->GetFieldByID(SpecialReportField::ID.FUNCTION_RETURN_VALUE);
		if (pField)
		{
			pField->SetDataType(functionDescr->GetReturnValueDataType());
			target = functionDescr->GetNamespace().ToString();
		}
	}

	else
		target = m_pPropGrid->GetProperty(3)->GetValue();

	WoormLink* newLink = CreateNewLinkObject(WoormLink::WoormLinkType::ConnectionFunction, WoormLink::WoormLinkSubType::File, owner, target, alias, linkedByVar);
	//adding function parameters 
	if (functionDescr)
	{
		for (int i = 0; i < functionDescr->GetParameters().GetSize(); i++)
		{
			CDataObjDescription* pDescr = functionDescr->GetParamDescription(i);
			CString sName = pDescr->GetName();
			
			DataType dt = pDescr->GetDataType();
		
			newLink->AddLinkParam(sName, dt, dt.FormatDefaultValue());
		}
	}

	else if (linkedByVar)
	{
		CrsProp* addParamProp = (CrsProp*)m_pPropGrid->GetProperty(4);
		ReadUserParametersIntoLink(addParamProp, newLink);
	}

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Links);
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, newLink);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewReportLink()
{
	//linked by variable flag
	int index = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (index < 0)
		return;
	BOOL ifLObVChecked = m_pPropGrid->GetProperty(2)->GetOptionData(index);

	m_bNeedsApply = TRUE;

	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_REPORT_LINK;

	CrsProp* path;

	if (ifLObVChecked)
	{
		path = new CrsProp(_TB("Variable contains link value"), L"");
		path->AllowEdit(FALSE);
		LoadVariablesIntoProperty(path);
		m_pPropGrid->AddProperty(path);
		CrsProp* paramGroup = new CrsProp(_TB("Add parameters"));
		m_pPropGrid->AddProperty(paramGroup);

		InsertLinkParameterBlock(NULL, FALSE);
	}
	else
	{
		path = new CRSNewLinkExplorer(L"Namespace", this, CRSNewLinkExplorer::REPORT);
		m_pPropGrid->AddProperty(path);
	}
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewReportLink()
{
	//flag Linked By Variable
	int flagIndex = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (flagIndex < 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Mandatory);
		return;
	}
	else
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Normal);

	BOOL linkedByVar = m_pPropGrid->GetProperty(2)->GetOptionData(flagIndex);

	CString target = m_pPropGrid->GetProperty(3)->GetValue();
	if (target.IsEmpty())
	{
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Mandatory);
		return;
	}
	else
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Normal);

	//alias
	DWORD alias = m_pPropGrid->GetProperty(0)->GetData();

	// var name associated with field
	CString owner = m_pPropGrid->GetProperty(0)->GetValue();
	WoormLink* newLink = CreateNewLinkObject(WoormLink::WoormLinkType::ConnectionReport, WoormLink::WoormLinkSubType::File, owner, target, alias, linkedByVar);

	CrsProp* addParamProp = (CrsProp*)m_pPropGrid->GetProperty(4);
	ASSERT(addParamProp);

	//load parameters from the Form with the Namespace specified by  a user
	if (!linkedByVar)
	{
		for (int i = 0;i < addParamProp->GetSubItemsCount();i++)
		{
			BOOL ifChecked = addParamProp->GetSubItem(i)->GetValue();
			if (!ifChecked)
				continue;
			//parameter name
			CString paramName = addParamProp->GetSubItem(i)->GetName();
	
			WoormField* pF = dynamic_cast<WoormField*>((CObject*)addParamProp->GetSubItem(i)->GetData());
			ASSERT_VALID(pF);
			if (!pF)
				continue;

			newLink->AddLinkParam(paramName, pF->GetDataType(), pF->GetDataType().FormatDefaultValue());
		}
	}
	else //insert user parameters 
	{
		if (!ReadUserParametersIntoLink(addParamProp, newLink))
			return;
	}

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Links);
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, newLink);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::AddReportLinkParameters()
{
	ASSERT(m_pTreeNode);

	//extract existing object
	WoormLink* link = (WoormLink*)m_pTreeNode->m_pItemData;

	//parameters array
	CrsProp* addParamProp = (CrsProp*)m_pPropGrid->GetProperty(0)->GetSubItem(3);

	//control if parameter is checked and not present in the parameters array
	// if parameter has been unchecked then i'll remove it
	for (int i = 0; i < addParamProp->GetSubItemsCount();i++)
	{
		CRSCheckBoxProp* prop = (CRSCheckBoxProp*)addParamProp->GetSubItem(i);
		WoormField* pF = dynamic_cast<WoormField*>((CObject*)prop->GetData());
		ASSERT_VALID(pF);
		if (!pF)
			continue;

		BOOL checked = prop->GetValue();
		if (checked)
		{
			if (link->m_pLocalSymbolTable->GetField(pF->GetName(), FALSE))
				continue;

			link->AddLinkParam(prop->GetName(), pF->GetDataType(), pF->GetDataType().FormatDefaultValue());
		}
		else
		{
			if (!link->m_pLocalSymbolTable->GetField(pF->GetName(), FALSE))
				continue;

			link->m_pLocalSymbolTable->DelField(pF->GetName());
		}
	}
	
	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Links);
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, link);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::AddLinkParameters(CNodeTree* pNode)
{
	WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pItemData);
	ASSERT_VALID(pLink);
	ASSERT(pLink->m_LinkType == WoormLink::ConnectionReport);
	
	LoadLinkPropertyGrid(pNode, TRUE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewURLLink()
{
	//linked by variable flag
	int index = m_pPropGrid->GetProperty(3)->GetSelectedOption();
	if (index < 0)
		return;
	BOOL ifLObVChecked = m_pPropGrid->GetProperty(3)->GetOptionData(index);

	//url subtype
	int subTIndex = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (subTIndex < 0)
		return;

	m_bNeedsApply = TRUE;

	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_URL_LINK;

	WoormLink::WoormLinkSubType subType = (WoormLink::WoormLinkSubType)m_pPropGrid->GetProperty(2)->GetOptionData(subTIndex);

	CrsProp* path = new CrsProp(_TB("Variable contains link value"), L"");
	m_pPropGrid->AddProperty(path);
	if (ifLObVChecked)
	{
		path->AllowEdit(FALSE);
		LoadVariablesIntoProperty(path);
	}

	else
	{
		if (subType == WoormLink::File)
			path->SetName(_TB("Path name:"));
		else if (subType == WoormLink::Url)
			path->SetName(_TB("Url:"));
		else if (subType == WoormLink::CallTo)
			path->SetName(_TB("Telephone number"));
		else if (subType == WoormLink::GoogleMap)
		{
			path->SetName(_TB("Street address"));
			path->SetValue(_T("http://maps.google.it/maps"));
		}
	}

	CrsProp* paramGroup =  new CrsProp(_TB("Add parameters"));
	m_pPropGrid->AddProperty(paramGroup);
	InsertLinkParameterBlock(NULL, TRUE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewUrlLink()
{

	//flag Linked By Variable
	int index = m_pPropGrid->GetProperty(3)->GetSelectedOption();
	if (index < 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Important);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Normal);

	BOOL linkedByVar = m_pPropGrid->GetProperty(3)->GetOptionData(index);

	// Control if the path property is not empty
	CString target = m_pPropGrid->GetProperty(4)->GetValue();
	if (target.IsEmpty())
	{
		((CrsProp*)m_pPropGrid->GetProperty(4))->SetColoredState(CrsProp::State::Mandatory);
		return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(4))->SetColoredState(CrsProp::State::Normal);

	// var name associated with field
	CString owner = m_pPropGrid->GetProperty(0)->GetValue();

	//alias
	DWORD alias = m_pPropGrid->GetProperty(0)->GetData();

	 // get the woorm link subtype
	index = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (index < 0)
	{
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::State::Mandatory);
		 return;
	}

	else
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::State::Normal);
		
	WoormLink::WoormLinkSubType subType =(WoormLink::WoormLinkSubType)m_pPropGrid->GetProperty(2)->GetOptionData(index);
	
	WoormLink* newLink = CreateNewLinkObject(WoormLink::WoormLinkType::ConnectionURL, subType, owner, target, alias, linkedByVar);
	ASSERT(newLink);

	//read params for file and url
	if (subType == WoormLink::WoormLinkSubType::File || subType == WoormLink::WoormLinkSubType::Url)
	{
		CrsProp* addParamProp =(CrsProp*) m_pPropGrid->GetProperty(5);
		if (!ReadUserParametersIntoLink(addParamProp, newLink) )
			return;
	}

	else if (subType == WoormLink::WoormLinkSubType::CallTo)
	{
		CString empty = DataType::String.FormatDefaultValue();
		newLink->AddLinkParam(_NS_WRMVAR("TelephonePrefix"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("ISOCountryCode"), DataType::String, empty);
	}

	else if (subType == WoormLink::WoormLinkSubType::GoogleMap)
	{	
		CString empty = DataType::String.FormatDefaultValue();
		newLink->AddLinkParam(_NS_WRMVAR("AddressType"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("Address"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("StreetNumber"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("City"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("ZipCode"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("County"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("Country"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("FederalState"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("Latitude"), DataType::String, empty);
		newLink->AddLinkParam(_NS_WRMVAR("Longitude"), DataType::String, empty);
	}

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Links);
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, newLink);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewFormLink()
{
	//linked by variable flag
	int index = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (index < 0)
		return;
	BOOL ifLObVChecked = m_pPropGrid->GetProperty(2)->GetOptionData(index);

	m_bNeedsApply = TRUE;
	
	m_pPropGrid->m_NewElement_Type = CRS_PropertyGrid::NewElementType::NEW_FORM_LINK;

	CrsProp* path;
	
	if (ifLObVChecked)
	{
		path = new CrsProp(_TB("Variable contains link value"), L"");
		path->AllowEdit(FALSE);
		LoadVariablesIntoProperty(path);
		m_pPropGrid->AddProperty(path);
		CrsProp* paramGroup = new CrsProp(_TB("Add parameters"));
		m_pPropGrid->AddProperty(paramGroup);
		InsertLinkParameterBlock(NULL,FALSE);
	}
	else
	{
		path = new CRSNewLinkExplorer(L"Namespace", this, CRSNewLinkExplorer::FORM);
		m_pPropGrid->AddProperty(path);
	}												
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::CreateNewFormLink()
{
	//flag Linked By Variable
	int flagIndex = m_pPropGrid->GetProperty(2)->GetSelectedOption();
	if (flagIndex < 0) {
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Mandatory);
		return;
	}
	else
		((CrsProp*)m_pPropGrid->GetProperty(2))->SetColoredState(CrsProp::Normal);

	BOOL linkedByVar = m_pPropGrid->GetProperty(2)->GetOptionData(flagIndex);

	CString target = m_pPropGrid->GetProperty(3)->GetValue();
	if (target.IsEmpty())
	{
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Mandatory);
		return;
	}
	else
		((CrsProp*)m_pPropGrid->GetProperty(3))->SetColoredState(CrsProp::State::Normal);

	//alias
	DWORD alias = m_pPropGrid->GetProperty(0)->GetData();

	// var name associated with field
	CString owner = m_pPropGrid->GetProperty(0)->GetValue();
	WoormLink* newLink = CreateNewLinkObject(WoormLink::WoormLinkType::ConnectionForm, WoormLink::WoormLinkSubType::File, owner, target, alias, linkedByVar);

	//load parameters from the Form with the Namespace specified by  a user
	if (!linkedByVar)
	{
		// copied from old dialog code
		CTBNamespace ns(CTBNamespace::DOCUMENT, target);
		
		CLocalizableXMLDocument aXMLDBTDoc(ns, AfxGetPathFinder());
		aXMLDBTDoc.EnableMsgMode(FALSE);

		if (aXMLDBTDoc.LoadXMLFile(AfxGetPathFinder()->GetDocumentDbtsFullName(ns)))
		{
			CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(_T("Master")); //XML_DBT_TYPE_MASTER_TAG
			if (pDBTMasterNode == NULL)
				return;

			CXMLNode* pChildNode = pDBTMasterNode->GetChildByName(_T("Table"));//XML_TABLE_TAG
			if (pChildNode == NULL)
				return;

			CString strTableName;
			pChildNode->GetText(strTableName);
			if (strTableName.IsEmpty())
				return;

			const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(strTableName);
			if (pEntry == NULL)
				return;

			SqlRecord* pRec = pEntry->CreateRecord();
			if (pRec == NULL)
				return;

			for (int i = 0; i < pRec->GetNumberSpecialColumns(); i++)
			{
				SqlRecordItem* pField = pRec->GetSpecialColumn(i);

				CString sName = pField->GetColumnName();
				DataType dt = pField->GetDataObj()->GetDataType();
				newLink->AddLinkParam(sName, dt, dt.FormatDefaultValue());
			}

			delete pRec;
		}

	}

	//insert user parameters 
	else
	{
		CrsProp* addParamProp = (CrsProp*)m_pPropGrid->GetProperty(4);
		if (!ReadUserParametersIntoLink(addParamProp, newLink))
			return;
	}

	m_bNeedsApply = FALSE;

	GetDocument()->RefreshRSTree(ERefreshEditor::Links);
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, newLink);

	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
// Loads existing variables from Document into the Property specified *prop*
void CRS_ObjectPropertyView::LoadVariablesIntoProperty(CBCGPProp* prop)
{
	if (!prop)
		return;

	for (int i = 0; i < GetDocument()->GetEditorSymTable()->GetSize(); i++)
	{
		WoormField* pF = GetDocument()->GetEditorSymTable()->GetAt(i);
		if (pF->GetDataType() == DataType::String)
			prop->AddOption(pF->GetName());
	}
}

// Reads parameters inserted by user with function InsertLinkParametersBlock  and adds them to newLink
//-----------------------------------------------------------------------------
BOOL CRS_ObjectPropertyView::ReadUserParametersIntoLink(CBCGPProp* addParamProp, WoormLink* newLink)
{
	ASSERT(addParamProp);
	ASSERT(newLink);

	if (!addParamProp || !newLink)
		return FALSE;

	CStringArray paramsAdded;

	for (int i = 0;i < addParamProp->GetSubItemsCount();i++)
	{
		CrsProp* param = (CrsProp*)addParamProp->GetSubItem(i);

		//parameter name
		CString paramName = param->GetSubItem(0)->GetValue();
		if (paramName.IsEmpty())
			continue;

		if (CStringArray_Find(paramsAdded, paramName) >= 0)
			continue;
		
		paramsAdded.Add(paramName);
		
		//Parameter type
		if (((CString)param->GetSubItem(1)->GetValue()).IsEmpty())
		{
			((CrsProp*)param->GetSubItem(1))->SetColoredState(CrsProp::State::Important);
			return FALSE;
		}

		else
			((CrsProp*)param->GetSubItem(1))->SetColoredState(CrsProp::State::Normal);

		// get param type
		int typeIndex = param->GetSubItem(1)->GetSelectedOption();
		if (typeIndex < 0)
			continue;

		DataType dt = *(DataType*)param->GetSubItem(1)->GetOptionData(typeIndex);

		if (dt == DataType::Enum)
		{
			typeIndex = param->GetSubItem(2)->GetSelectedOption();
			if (typeIndex < 0)
			{
				((CrsProp*)param->GetSubItem(2))->SetColoredState(CrsProp::State::Important);
				return FALSE;
			}

			else
				((CrsProp*)param->GetSubItem(2))->SetColoredState(CrsProp::State::Normal);

			WORD wTag = (WORD)param->GetSubItem(2)->GetOptionData(typeIndex);
			dt = DataType(dt, wTag);
		}

		newLink->AddLinkParam(paramName, dt, dt.FormatDefaultValue());
	}
																									
	return TRUE;
}

//-----------------------------------------------------------------------------

WoormLink* CRS_ObjectPropertyView::CreateNewLinkObject(WoormLink::WoormLinkType type, WoormLink::WoormLinkSubType subType, CString owner, CString target, int alias, BOOL linkedByVar)
{
	WoormLink* newLink = new WoormLink(&(GetDocument()->m_ViewSymbolTable));
	ASSERT(newLink && newLink->m_pLocalSymbolTable);

	newLink->m_strLinkOwner = owner;
	newLink->m_strTarget = target;
	newLink->m_nAlias = alias;
	newLink->m_LinkType = type;
	newLink->m_SubType = subType;
	newLink->m_bLinkTargetByField = linkedByVar;
	GetDocument()->m_arWoormLinks.Add(newLink);

	//deleting old saved connection parameters and load the new from de list control
	//preserve first two item because they are LinkedDocumentID and _ReturnValue special predef local ident
	if (newLink->m_pLocalSymbolTable->GetUpperBound() > 1)
		newLink->m_pLocalSymbolTable->RemoveAt(2, newLink->m_pLocalSymbolTable->GetUpperBound() - 1);
	else
		newLink->m_pLocalSymbolTable->RemoveAt(1, newLink->m_pLocalSymbolTable->GetUpperBound());

	return newLink;
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::InsertReportLinkParameterBlock(WoormLink* pLink, CString nameSpace, CBCGPProp* caller)
{
	CrsProp* mainGroup = (CrsProp*)(caller!=NULL? caller : m_pPropGrid->GetProperty(4));
	mainGroup->SetOwnerList(m_pPropGrid);

	WoormTable* wrmTargetReportSymTable = new WoormTable(WoormTable::SymTable_RUNREPORT, GetDocument());
	m_arGarbage.Add(wrmTargetReportSymTable);

	CTBNamespace ns(CTBNamespace::REPORT, nameSpace);
	CString	strReportPath = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);

	CString sError; int nLine = -1;
	wrmTargetReportSymTable->ParseVariables(strReportPath, sError, nLine);

	Array paramArray; paramArray.SetOwns(false);

	for (int i = 0; i < wrmTargetReportSymTable->GetSize(); i++)
	{
		WoormField* pF = wrmTargetReportSymTable->GetAt(i);

		if (!pF->IsInput()) continue;

		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID && pF->GetId() != SpecialReportField::ID.HIDE_ALL_ASK_DIALOGS) continue;

		paramArray.Add(pF);
	}

	paramArray.SetCompareFunction(::CompareFieldByName);
	paramArray.QuickSort();

	for (int k = 0;k < paramArray.GetCount();k++)
	{ 
		WoormField* pF = (WoormField*) paramArray.GetAt(k);

		BOOL bCheck = FALSE;
		if (pLink && pLink->m_pLocalSymbolTable && pLink->m_pLocalSymbolTable->GetField(pF->GetName(), FALSE))
			bCheck = TRUE;

		CRSCheckBoxProp* newParam = new  CRSCheckBoxProp(pF->GetName(), bCheck, (LPCTSTR)L"", (DWORD_PTR)pF, this);

		//if (pF->GetId() < SpecialReportField::REPORT_LOWER_SPECIAL_ID)
		//	newParam->m_varValue = bCheck;

		newParam->SetOwnerList(m_pPropGrid);
		mainGroup->AddSubItem(newParam);
	}

	m_pPropGrid->AdjustLayout();
}																							 

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::InsertLinkParameterBlock(CBCGPProp* caller, BOOL ifUrl)
{
	CrsProp* mainGroup = ifUrl?(CrsProp*) m_pPropGrid->GetProperty(5) : (CrsProp*)m_pPropGrid->GetProperty(4);
	int count = mainGroup->GetSubItemsCount();

	if (!caller && count > 0)
		return;

	CString colCount;
	if (caller)
	{
		CString callerName = caller->GetName();
		colCount.Format(L"%d", count);
		if (callerName.Compare(_TB("Parameter") + colCount) != 0)
			return;
		count++;
	}
	else
		count = 1;
	colCount.Format(L"%d", count);

	CrsProp* paramGroup = new	CrsProp(L"Parameter" + colCount);

	CrsProp* name = new CrsProp(_TB("Name"),L"");
	//name->SetValue((LPCTSTR)(L"Parameter " + colCount));
	//name->SetColoredState(CrsProp::State::Important);
	name->SetOwnerList(m_pPropGrid);
	paramGroup->AddSubItem(name);

	//Load types  and add types
	CRSNewLinkParam* types = new  CRSNewLinkParam(_TB("Data type"), L"", this, CRSNewLinkParam::TYPE,ifUrl);
	types->AllowEdit(FALSE);
	LoadTypeProp(types);
	
	types->SetOwnerList(m_pPropGrid);
	paramGroup->AddSubItem(types);

	//Load enum subtypes  and add types
	CRSNewLinkParam* subTypes = new  CRSNewLinkParam(_TB("Enumerative subtype"), L"", this, CRSNewLinkParam::ENUM_SUBTYPE,ifUrl);
	subTypes->AllowEdit(FALSE);
	subTypes->SetVisible(FALSE);

	LoadEnumTypeProp(subTypes);

	subTypes->SetOwnerList(m_pPropGrid);
	paramGroup->AddSubItem(subTypes);

	// add group
	paramGroup->SetOwnerList(m_pPropGrid);
	mainGroup->AddSubItem(paramGroup);
	m_pPropGrid->AdjustLayout();
}

///////////////////////////////////////////////////////////////////////////////

CRSTriggerEventProp::CRSTriggerEventProp(TriggEventData* pEvent, CString name, LPCTSTR value, CRSTriggerEventProp::PropType eType, CRS_ObjectPropertyView* pPropView)
	: CrsProp(name, value)
{
	ASSERT_VALID(pEvent);
	m_pEvent = pEvent;
	m_eType = eType;
	m_sOldValue = value;

	if (eType == CRSTriggerEventProp::PropType::NewBreakingField)
		LoadUsableBreakingFields(pPropView);
	else if (eType == CRSTriggerEventProp::PropType::NewSubTotalField)
		LoadUsableSubTotalFields(pPropView);
}

//-----------------------------------------------------------------------------
CRSTriggerEventProp::CRSTriggerEventProp(TriggEventData* pEvent, CString name, variant_t value, PropType eType, CRS_ObjectPropertyView* pPropView)
	: CrsProp(name, value)
{
	ASSERT_VALID(pEvent);
	m_pEvent = pEvent;
	m_eType = eType;
	m_sOldValue = value;

	if (eType == CRSTriggerEventProp::PropType::NewBreakingField)
		LoadUsableBreakingFields(pPropView);
	else if (eType == CRSTriggerEventProp::PropType::NewSubTotalField)
		LoadUsableSubTotalFields(pPropView);
}

//-----------------------------------------------------------------------------
void CRSTriggerEventProp::LoadUsableBreakingFields(CRS_ObjectPropertyView* pPropView)
{
	CStringArray arrayBeakingFields;
	m_pEvent->GetBreakingFields(arrayBeakingFields);
	AddOption(L"", TRUE, NULL);
	for (int i = 0; i < m_pEvent->m_SymTable.GetSize(); i++)
	{
		WoormField* pRepField = m_pEvent->m_SymTable.GetAt(i);

		if (pRepField->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		if (
			pRepField->GetFieldType() != WoormField::FIELD_COLUMN &&
			pRepField->GetFieldType() != WoormField::FIELD_NORMAL
			)
			continue;

		if (!pRepField->IsExprRuleField() && !pRepField->IsTableRuleField() /*if not function*/)
			continue;

		//vedi EVNDLG.cpp - CBrkCondDlg::ShowFieldList()
		if (
			CStringArray_Find(arrayBeakingFields, pRepField->GetName()) < 0 &&
			CStringArray_Find(pPropView->m_arNewBreakingFields, pRepField->GetName()) < 0)
				AddOption(pRepField->GetName(), TRUE, (DWORD)pRepField);
	}

}

//-----------------------------------------------------------------------------
void CRSTriggerEventProp::LoadUsableSubTotalFields(CRS_ObjectPropertyView* pPropView)
{
	CStringArray arrayBeakingFields;
	m_pEvent->GetBreakingFields(arrayBeakingFields);
	CStringArray arraySubtotals;
	m_pEvent->GetSubtotalFields(arraySubtotals);
	AddOption(L"", TRUE, NULL);
	for (int i = 0; i < m_pEvent->m_SymTable.GetSize(); i++)
	{
		WoormField* pRepField = m_pEvent->m_SymTable.GetAt(i);

		if (pRepField->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		//vedi EVNDLG.cpp - void CSubTotalDlg::ShowColumnField()
		if (!pRepField->IsColumn() || pRepField->IsHidden() || pRepField->IsInput())
			continue;

		if (!pRepField->IsExprRuleField() && !pRepField->IsTableRuleField() /*if not function*/)
			continue;

		if (
				CStringArray_Find(arrayBeakingFields, pRepField->GetName()) < 0 &&
				CStringArray_Find(arraySubtotals, pRepField->GetName()) < 0 &&
				CStringArray_Find(pPropView->m_arNewBreakingFields, pRepField->GetName()) < 0 &&
				CStringArray_Find(pPropView->m_arNewSubTotalFields, pRepField->GetName()) < 0 
			) 
			AddOption(pRepField->GetName(), TRUE, (DWORD)pRepField);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTriggerEventProp::OnUpdateValue()
{
	BOOL baseUpdate = CBCGPProp::OnUpdateValue();

	ASSERT_VALID(m_pEvent);

	switch (m_eType)
	{
		case CRSTriggerEventProp::PropType::EVENT_NAME:
		{
			CString errMsg = _T("");

			if (CheckPropValue(false, errMsg))
			{
				CString name = this->GetValue();
				name.Trim();
				m_pEvent->m_strEventName = name;
				GetDocument()->UpdateRSTreeNode(ERefreshEditor::Events, GetCurrentNode(), FALSE);
			}
			break;
		}

		case CRSTriggerEventProp::PropType::MustTrueTogether:
		{
			int index = GetSelectedOption();
			if (index < 0)
				break;

			m_pEvent->m_bMustTrueTogether = (BOOL)this->GetOptionData(index);

			break;
		}

		case CRSTriggerEventProp::PropType::NewBreakingField:
		{
			CString name = this->GetValue();
			if (name.CompareNoCase(m_sOldValue) == 0)
				break;
			if (!name.IsEmpty())
			{
				if (!m_sOldValue.IsEmpty())
					CStringArray_Remove(GetPropertyView()->m_arNewBreakingFields, m_sOldValue);

				m_sOldValue = name;
				GetPropertyView()->m_arNewBreakingFields.Add(name);

				//GetPropertyView()->m_bNeedsApply = TRUE;

				ASSERT_VALID(GetPropertyView()->m_pBreakingFieldListGroup);
				if (GetParent()->GetSubItem(GetParent()->GetSubItemsCount() - 1) == this)
				{
					GetPropertyView()->m_pBreakingFieldListGroup->AddSubItem(
						new CRSTriggerEventProp(this->m_pEvent, _TB("new breaking field"), L"",
							CRSTriggerEventProp::PropType::NewBreakingField, GetPropertyView()));
					GetPropertyGrid()->AdjustLayout();
				}

			}
				
			break;
		}

		case CRSTriggerEventProp::PropType::NewSubTotalField:
		{
			CString name = this->GetValue();
			if (name.CompareNoCase(m_sOldValue) == 0)
				break;

			if (!m_sOldValue.IsEmpty())
			{
				CStringArray_Remove(GetPropertyView()->m_arNewSubTotalFields, m_sOldValue);

				/*GetPropertyView()->m_bNeedsApply =
					(GetPropertyView()->m_arNewBreakingFields.GetCount() +
						GetPropertyView()->m_arNewSubTotalFields.GetCount()) > 0;*/
			}

			if (!name.IsEmpty() && name.CompareNoCase(m_sOldValue))
			{
				m_sOldValue = name;
				GetPropertyView()->m_arNewSubTotalFields.Add(name);

				//GetPropertyView()->m_bNeedsApply = TRUE;

				ASSERT_VALID(GetPropertyView()->m_pBreakingFieldListGroup);
				GetPropertyView()->m_pSubTotalGroup->AddSubItem(
					new CRSTriggerEventProp(this->m_pEvent, _TB("new subtotal field"), L"",
						 CRSTriggerEventProp::PropType::NewSubTotalField, GetPropertyView()));
				GetPropertyGrid()->AdjustLayout();
			}

			break;
		}

	}

	return TRUE;
}

///////////////////	CRSNewLinkFunction ///////////////////////

//-----------------------------------------------------------------------------
CRSNewLinkFunction::CRSNewLinkFunction(CString name, LPCTSTR value, CRS_ObjectPropertyView* propView, CRSNewLinkFunction::FieldType fType, DWORD_PTR data) :CrsProp(name,value,L"",data) {
	this->m_pPropertyView = propView;
	this->m_fType = fType;
}

//-----------------------------------------------------------------------------
CRSNewLinkFunction::CRSNewLinkFunction(CString name, variant_t value, CRS_ObjectPropertyView* propView, CRSNewLinkFunction::FieldType fType, DWORD_PTR data) : CrsProp(name, value, L"", data) {
	this->m_pPropertyView = propView;
	this->m_fType = fType;
}

//-----------------------------------------------------------------------------
BOOL CRSNewLinkFunction::OnUpdateValue() 
{
	__super::OnUpdateValue();

	switch (m_fType)
	{

	case FieldType::MODULE:
	{
		CString value = GetValue();
		if (value.IsEmpty())
			break;

		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 4) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 3; i--) {
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
			}
		}

		int index =GetSelectedOption();
		if (index < 0)
			break;
		// check if the extra property for url was added (subtype)
		AddOnModule* pAddOnMod = (AddOnModule*)GetOptionData(index);
		ASSERT(pAddOnMod);
		CRSNewLinkFunction* funcs = new CRSNewLinkFunction(_TB("Function"),L"", m_pPropertyView, FieldType::FUNCTION);
		funcs->AllowEdit(FALSE);

		CStringArray orderedArray;
		const CBaseDescriptionArray &arFunctions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().GetFunctions();
		
		for (int k = 0; k <= arFunctions.GetUpperBound(); k++)
		{
			CFunctionDescription* pFun = (CFunctionDescription*)arFunctions.GetAt(k);
			if (!pFun->IsPublished())
				continue;
			if (AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
				orderedArray.Add(pFun->GetName());
		}

		if (orderedArray.GetSize() == 0)
			funcs->SetValue((LPCTSTR)_TB("No function found for this module"));
		else
			CStringArray_Sort(orderedArray);

		for (int i = 0;i < orderedArray.GetSize();i++)
		{
			for (int k = 0; k <= arFunctions.GetUpperBound(); k++)
			{
				CFunctionDescription* pFun = (CFunctionDescription*)arFunctions.GetAt(k);
				if (!pFun->IsPublished())
					continue;
				if (pFun->GetName().CompareNoCase(orderedArray[i]) == 0)
				{
					 funcs->AddOption(pFun->GetName(), TRUE, (DWORD_PTR)pFun);
					 break;
				}
			}
		}
		
		m_pPropertyView->m_pPropGrid->AddProperty(funcs,TRUE,TRUE);
		break;
	}
	}

	return TRUE;
}

///////////////////	CRSNewLinkURL ///////////////////////

//-----------------------------------------------------------------------------
CRSNewLinkParam::CRSNewLinkParam(CString name, LPCTSTR value, CRS_ObjectPropertyView* propView, CRSNewLinkParam::FieldType fType, BOOL ifUrl, DWORD_PTR data) :CrsProp(name, value, L"", data) {
	this->m_pPropertyView = propView;
	this->m_fType = fType;
	this->m_ifUrl = ifUrl;
}

//-----------------------------------------------------------------------------
CRSNewLinkParam::CRSNewLinkParam(CString name, variant_t value, CRS_ObjectPropertyView* propView, CRSNewLinkParam::FieldType fType, BOOL ifUrl, DWORD_PTR data) : CrsProp(name, value, L"", data) {
	this->m_pPropertyView = propView;
	this->m_ifUrl = ifUrl;
}

//-----------------------------------------------------------------------------
BOOL CRSNewLinkParam::OnUpdateValue()
{
	CString oldValue = GetValue();

	__super::OnUpdateValue();

	CString value = GetValue();
	if (value.IsEmpty() || value.Compare(oldValue)==0)
		return TRUE;

	switch (m_fType)
	{

	case CRSNewLinkParam::TYPE:
	{
		int index = GetSelectedOption();
		if (index < 0)
			break;

		DataType* data =(DataType*)GetOptionData(index);
		if (data == NULL)
			break;

		CRSNewLinkParam* subType = (CRSNewLinkParam*)this->GetParent()->GetSubItem(2);

		if (data==&DataType::Enum)
		{
			subType->SetVisible(TRUE);
			m_pPropertyView->m_pPropGrid->AdjustLayout();
		}

		else {

			subType->SetVisible(FALSE);
			m_pPropertyView->m_pPropGrid->AdjustLayout();
			m_pPropertyView->InsertLinkParameterBlock(this->GetParent(),m_ifUrl);
		}

		break;
	}

	case CRSNewLinkParam::ENUM_SUBTYPE:
	{
		m_pPropertyView->InsertLinkParameterBlock(this->GetParent(), m_ifUrl);
		break;
	}
	}

	return TRUE;
} 

///////////////////	CRSNewLinkExplorer ///////////////////////
CRSNewLinkExplorer::CRSNewLinkExplorer(const CString& strName, CRS_ObjectPropertyView* pPropertyView, NewLinkType nLType,  const CString& description) :CrsProp(strName, (LPCTSTR)L"", description) {
	this->m_pPropertyView = pPropertyView;
	this->m_type = nLType;
}

//-----------------------------------------------------------------------------
void CRSNewLinkExplorer::OnClickButton(CPoint point)
{
	ASSERT_VALID(m_pPropertyView);
	if (!m_pPropertyView) return;

	CString sNs;

	if (m_type == NewLinkType::FORM)
	{
	CBaseDocumentExplorerDlg* pDocExplorer = AfxGetTBExplorerFactory()->CreateDocumentExplorerDlg();
	if (pDocExplorer->DoModal() == IDOK)
	{
		if (!pDocExplorer->m_FullNameSpace.IsEmpty())
				sNs = pDocExplorer->m_FullNameSpace;
		}

		SAFE_DELETE(pDocExplorer);
	}

	else if (m_type == NewLinkType::REPORT)
	{
		//load report
		CTBNamespace aNamespace(m_pPropertyView->GetDocument()->GetNamespace());
		aNamespace.SetType(CTBNamespace::REPORT);
		aNamespace.SetObjectName(_T(""));

		CTBExplorer* aExplorer=new CTBExplorer(CTBExplorer::OPEN, aNamespace);
		aExplorer->SetCanLink();
		if (!aExplorer->Open())
			return;

		CString strPath;
		aExplorer->GetSelPathElement(strPath);

		if (!strPath.IsEmpty())
		{
			CTBNamespace selNamespace;
			aExplorer->GetSelNameSpace(selNamespace);
			sNs = selNamespace.ToString();
		}

		SAFE_DELETE(aExplorer);

		bool found=false;
		for (int i=0; i< GetDocument()->m_arWoormLinks.GetCount();i++)
		{
			WoormLink* k = GetDocument()->m_arWoormLinks.GetAt(i);

		    if(k->m_strTarget.Compare(sNs)==0)
			{ 
				AfxMessageBox(_TB("Attention! A link to this report already exists"));
				this->SetColoredState(CrsProp::State::Important);
				found = true;
			}			
		}

		if (!found)
				this->SetColoredState(CrsProp::State::Normal);
			
		if (m_pPropertyView->m_pPropGrid->GetPropertyCount() > 4) {
			// clean all properties under the table property
			for (int i = m_pPropertyView->m_pPropGrid->GetPropertyCount() - 1; i > 3; i--) {
				int count = m_pPropertyView->m_pPropGrid->GetPropertyCount();
				CBCGPProp* prop = m_pPropertyView->m_pPropGrid->GetProperty(i);
				m_pPropertyView->m_pPropGrid->DeleteProperty(prop, TRUE, TRUE);
		}
	}

		CrsProp* chooseParam = new CrsProp(L"Choose report parameters");
		m_pPropertyView->m_pPropGrid->AddProperty(chooseParam);
		m_pPropertyView->InsertReportLinkParameterBlock(NULL, sNs, NULL);
	}

	this->SetValue((LPCTSTR)sNs);

	UpdateDocument();

	//ridisegno lo state (quindi l'immagine se ci deve essere)
	RedrawState();
}

////////////////////////////////////////////////////////////////////////////////
// CRSCheckBoxProp class

CRSCheckBoxProp::CRSCheckBoxProp(const CString& strName, BOOL bCheck, LPCTSTR lpszDescr, DWORD_PTR dwData, CRS_ObjectPropertyView *propView, BOOL bHideChildren)
	: CrsProp		(strName, _variant_t(bCheck), lpszDescr, dwData),
	m_propView		(propView),
	m_bHideChildren (bHideChildren)
{
	m_rectCheck.SetRectEmpty();
}

CRSCheckBoxProp::CRSCheckBoxProp(const CString& strName, BOOL* pbCheck, LPCTSTR lpszDescr, DWORD_PTR dwData, CRS_ObjectPropertyView *propView, BOOL bHideChildren)
	: CrsProp		(strName, _variant_t(*pbCheck), lpszDescr, dwData),
	m_propView		(propView),
	m_bHideChildren	(bHideChildren),
	m_pBoolValue	(pbCheck)
{
	m_rectCheck.SetRectEmpty();
}

//-----------------------------------------------------------------------------
void CRSCheckBoxProp::OnDrawName(CDC* pDC, CRect rect)
{
	const CSize sizeCheckBox = CBCGPVisualManager::GetInstance()->GetCheckRadioDefaultSize();
	const int dx = globalUtils.ScaleByDPI(2);
	const int dy = max(0, (int)(.5 + .5 * (rect.Height() - sizeCheckBox.cy)));

	m_rectCheck = CRect(rect.left + dx, rect.top + dy,
		rect.left + dx + sizeCheckBox.cx, rect.top + dy + sizeCheckBox.cy);

	rect.left = m_rectCheck.right + dx;

	CBCGPProp::OnDrawName(pDC, rect);

	if (!m_rectMenuButton.IsRectEmpty() && m_rectCheck.right > m_rectMenuButton.left)
	{
		m_rectCheck.SetRectEmpty();
		return;
	}

	OnDrawCheckBox(pDC, m_rectCheck, BOOL(m_varValue));
}

//-----------------------------------------------------------------------------
void CRSCheckBoxProp::OnClickName(CPoint point)
{
	if (m_bEnabled && m_rectCheck.PtInRect(point))
	{
		m_varValue = (BOOL(m_varValue))? FALSE : TRUE;
		if (this->m_pBoolValue) *this->m_pBoolValue = BOOL(m_varValue);

		m_pWndList->InvalidateRect(m_rectCheck);

		m_pWndList->OnPropertyChanged(this);

		SetVisibleSubItems();
	}
}

//-----------------------------------------------------------------------------
BOOL CRSCheckBoxProp::OnDblClick(CPoint point)
{
	if (m_bEnabled && m_rectCheck.PtInRect(point))
	{
		return TRUE;
	}

	m_varValue = (BOOL(m_varValue)) == TRUE ? FALSE : TRUE;
	if (this->m_pBoolValue) *this->m_pBoolValue = BOOL(m_varValue);

	m_pWndList->InvalidateRect(m_rectCheck);

	m_pWndList->OnPropertyChanged(this);

	SetVisibleSubItems();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSCheckBoxProp::OnDrawCheckBox(CDC * pDC, CRect rect, BOOL bChecked)
{
	COLORREF clrTextOld = pDC->GetTextColor();

	CBCGPVisualManager::GetInstance()->OnDrawCheckBox(pDC, rect,
		FALSE, bChecked, m_bEnabled);

	pDC->SetTextColor(clrTextOld);
}

//-----------------------------------------------------------------------------
BOOL CRSCheckBoxProp::PushChar(UINT nChar)
{
	if (nChar == VK_SPACE)
	{
		OnDblClick(CPoint(-1, -1));
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSCheckBoxProp::SetVisibleSubItems() 
{	
	if (!m_bHideChildren) return;
	BOOL bVisible = m_varValue;
	for (int i = 0;i < this->GetSubItemsCount();i++)
	{
		CrsProp* prop = dynamic_cast<CrsProp*>(this->GetSubItem(i));
		ASSERT_VALID(prop);
		if (!prop) 
			break;

		if (i == 2 && (m_propView && m_propView->m_bAllFieldsAreHidden))
			prop->SetVisible(FALSE);
		else
		{
			if (bVisible)
				bVisible = OnShowChild(prop);
			prop->SetVisible(bVisible);

		}

		for (int i = 0; i < prop->GetSubItemsCount(); i++)
		{
			CrsProp* subProp = dynamic_cast<CrsProp*>(prop->GetSubItem(i));
			if (!subProp) continue;

			BOOL b = bVisible;
			if (b)
				b = OnShowGrandChild(prop, subProp);
				subProp->SetVisible(b);
		}
	}

	GetPropertyGrid()->AdjustLayout();
}

//-----------------------------------------------------------------------------
BOOL CRSChkDBCol::OnShowGrandChild(CrsProp* prop, CrsProp* subProp)
{
	CString value = prop->GetName();
	if (value.CompareNoCase(_TB("Data type")))
		return TRUE;
	
	int o = prop->GetSelectedOption();
	if (o < 0)
		return FALSE;

	DataType dt = *(DataType*) prop->GetOptionData(o);
	if (dt.m_wType == DATA_ENUM_TYPE)
		return TRUE;
	return FALSE;
}

//=============================================================================

void CRS_ObjectPropertyView::NewColumnsPropertyGrid(CNodeTree* pNode)
{
	ASSERT(pNode == m_pTreeNode);
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pParentItemData);
	ASSERT_VALID(pTblRule);

	NewDBElement(FALSE, TRUE, pTblRule);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewRulesPropertyGrid(CNodeTree*)
{
	NewDBElement(TRUE,TRUE);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewRulesFromDropTablePropertyGrid(CNodeTree* pSourceNode)
{
	ASSERT_VALID(pSourceNode);
	if (pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBTABLE && pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBVIEW)
	{
		ASSERT(FALSE);
		return;
	}

	SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pSourceNode->m_pItemData);
	ASSERT_VALID(pCatEntry);
	if (!pCatEntry)
	{
		return;
	}

	//-----
	ASSERT_VALID(m_pPropGrid);
	if (!ClearPropertyGrid()) return;
	m_bNeedsApply = TRUE;
	m_pTreeNode = pSourceNode;

	TblRuleData* pTblRule = new TblRuleData(*GetDocument()->GetEditorSymTable(), AfxGetDefaultSqlConnection(), pCatEntry->m_strTableName);

	RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
	pRuleData->Add(pTblRule);

	//-----
	//TODO Anastasia
	NewDBElement(TRUE, TRUE, pTblRule);
}

//-----------------------------------------------------------------------------
void CRS_ObjectPropertyView::NewVariablePropertyGrid(CNodeTree* pNode)
{

	NewElement(FALSE, FALSE, FALSE, TRUE);
}