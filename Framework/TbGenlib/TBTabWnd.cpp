// TabSelector.cpp : implementation file
//

#include "stdafx.h"
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGenlib\BASEDOC.H>
#include <TbGenlib\TabSelector.h>

#include "TBTabWnd.h"
#include "reswalk.h"

/////////////////////////////////////////////////////////////////////////////
//			CTaskBuilderTabWndInfo
/////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTaskBuilderTabWndInfo, CBCGPTabInfo)

//------------------------------------------------------------------------------
CTaskBuilderTabWndInfo::CTaskBuilderTabWndInfo(const CString& strText, UINT nTabID)
	:
	CBCGPTabInfo(strText, 0, NULL, nTabID, FALSE)
{
	m_bAlwaysShowToolTip = TRUE;
}

//------------------------------------------------------------------------------
CTaskBuilderTabWndInfo::~CTaskBuilderTabWndInfo()
{

}

//------------------------------------------------------------------------------
void CTaskBuilderTabWndInfo::SetVisible(BOOL bVisible)
{
	m_bVisible = bVisible;
}

//------------------------------------------------------------------------------
void CTaskBuilderTabWndInfo::Enable(BOOL bEnabled)
{
	if (m_pWnd)
		m_pWnd->EnableWindow(bEnabled);
}

//------------------------------------------------------------------------------
void CTaskBuilderTabWndInfo::SetForeColor(COLORREF color)
{
	m_clrText = color;
}

/////////////////////////////////////////////////////////////////////////////
//			CTaskBuilderTabWnd
/////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTaskBuilderTabWnd, CBCGPTabWnd)

//------------------------------------------------------------------------------
CTaskBuilderTabWnd::CTaskBuilderTabWnd()
	:
	m_bDeleteTabInfo(FALSE),
	m_HasTabTopBorder(TRUE),
	m_pDocument(NULL)
{
	m_HoveringBkgColor =  AfxGetThemeManager()->GetTabberTabHoveringBkgColor();
	m_HoveringForeColor = AfxGetThemeManager()->GetTabberTabHoveringForeColor();
	EnableTabSwap(FALSE);
}

//-----------------------------------------------------------------------------
CTaskBuilderTabWnd::~CTaskBuilderTabWnd()
{
	if (m_bDeleteTabInfo)
		for (int i = m_arTabs.GetUpperBound(); i >= 0; i--)
		{
			CObject* pObject = (CObject*) m_arTabs.GetAt(i);
			CTaskBuilderTabWndInfo* pInfo = dynamic_cast<CTaskBuilderTabWndInfo*>(pObject);
			if (pInfo)
				delete pInfo;
		}
}

//-----------------------------------------------------------------------------
/*static*/ BOOL CTaskBuilderTabWnd::PreProcessSysKeyMessage(MSG* pMsg, CBaseDocument* pDoc, CWnd* pWnd)
{
	// gestisco solo acceleratori di tab & selettori
	if (!pMsg || pMsg->message != WM_SYSKEYDOWN || (pMsg->wParam != VK_PRIOR && pMsg->wParam != VK_NEXT && pMsg->wParam != VK_F11 && (pMsg->wParam < 0x30 || pMsg->wParam > 0x5A)))
		return FALSE;

	BOOL bHoldForwardingSysKeydownToParent = FALSE;
	
	if (pDoc)
	{
		bHoldForwardingSysKeydownToParent = pDoc->m_bForwardingSysKeydownToParent;
		pDoc->m_bForwardingSysKeydownToParent = TRUE;
	}

	// serve per segnalare al parent che, nel caso debba passare in modalita "discendente" (per esempio io sono un "fratello di un tabber") mi salti nel
	// broadcasting
	LPARAM bHoldLPARM = pMsg->lParam;
	pMsg->lParam = (LPARAM) pWnd->GetSafeHwnd();
	BOOL bRet = !pWnd->IsKindOf(RUNTIME_CLASS(CTabSelectorTabWnd)) && pWnd->GetParent()->PreTranslateMessage(pMsg);
	pMsg->lParam = bHoldLPARM;

	if (pDoc)
		pDoc->m_bForwardingSysKeydownToParent = bHoldForwardingSysKeydownToParent;

	return bRet;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderTabWnd::Create(int tbStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, Location location /* LOCATION_BOTTOM*/, BOOL bCloseBtn /*FALSE*/)
{
	SetTbStyle((TBStyle) tbStyle);
	
	Style bcgpStyle = STYLE_3D;
	if (tbStyle >= TB_STRIP)
	{
		SetHasTopTabBorder(FALSE);
	}
	else
		bcgpStyle = (Style) tbStyle;

	return __super::Create(bcgpStyle, rect, pParentWnd, nID, location, bCloseBtn);
}

//--------------------------------------------------------------------------
BOOL CTaskBuilderTabWnd::ProcessSysKeyMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);
		
	// gestisco solo acceleratori di tab & selettori
	if (!pMsg || pMsg->message != WM_SYSKEYDOWN || (pMsg->wParam != VK_PRIOR && pMsg->wParam != VK_NEXT && pMsg->wParam != VK_F11 && (pMsg->wParam < 0x30 || pMsg->wParam > 0x5A)))
		return FALSE;

	
	// Attenzione LPARAM è inutilizzabile in questo contesto (vedi CTaskBuilderTabWnd::PreProcessSysKeyMessage)
	int nActiveTab = -1;
	switch ((UINT) pMsg->wParam)
	{
		case VK_PRIOR:
		{
			nActiveTab = GetActiveTab();
			do
			{
				if (nActiveTab == 0)
					return FALSE;

				nActiveTab--;
			}	// se si passa per una dialog disabilitata si skippa
			while (/*!pTab-> m_bEnabled || */ !IsTabVisible(nActiveTab));

			break;
		}
		case VK_NEXT:
		{
			nActiveTab = GetActiveTab();
			do
			{
				if (nActiveTab == GetTabsNum() - 1)
				 	return FALSE;
			
			 	nActiveTab++;
	
			}	// se si passa per una dialog disabilitata si skippa
			while (/*!pTab-> m_bEnabled || */ !IsTabVisible(nActiveTab));

			break;
		}
		case VK_F11:
		{
			CWnd* pForm = GetParent();
			if (!pForm)
				return FALSE;
	
			CWnd* pWnd = pForm->GetNextDlgTabItem(this, FALSE);
	
			while (pWnd && pWnd->m_hWnd != m_hWnd)
			{
				if (pWnd->IsWindowEnabled()	&& !pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
				{
					pWnd->SetFocus();
					return TRUE;
				}
	
				pWnd = pForm->GetNextDlgTabItem(pWnd, FALSE);
			}
					
			return FALSE;
		}
		default:
		{
			CString strPattern(_T("&"));
			strPattern += (TCHAR) pMsg->wParam;
						
			for (int i = 0; i < GetTabsNum(); i++)
			{
				CString strCaption;
				if (!GetTabLabel (i, strCaption) || !IsTabVisible(i))
					continue;

				strCaption.MakeUpper();
				if (strCaption.Find(strPattern) != -1)
				{
					if (i == GetActiveTab())
						return TRUE;
				
					// se si richiama una dialog disabilitata si da errore
					//if (!pTab-> m_bEnabled)
					//{
					//	MessageBeep(MB_ICONHAND);
					//	return TRUE;
					//}

					nActiveTab = i;
					break;
				}
			}
		}
	}

	if (nActiveTab < 0)
		return FALSE;

	m_bUserSelectedTab = TRUE;
	BOOL bOk = SetActiveTab(nActiveTab);
	m_bUserSelectedTab = FALSE;

	return bOk;
}

//--------------------------------------------------------------------------
BOOL CTaskBuilderTabWnd::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!m_pDocument)
		return __super::PreTranslateMessage(pMsg);

	if (ProcessSysKeyMessage(pMsg))
		return TRUE;

	if (m_pDocument->m_bForwardingSysKeydownToChild)
		return FALSE;

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, m_pDocument, this) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//-----------------------------------------------------------------------------
int CTaskBuilderTabWnd::GetTabIndexOf(CWnd* pWnd)
{
	for (int i = 0; i < m_arTabs.GetSize(); i++)
		if (GetTabWnd(i) == pWnd)
			return i;

	return -1;
}
//-----------------------------------------------------------------------------
void CTaskBuilderTabWnd::SetTabImageOf(CWnd* pWnd, UINT nID)
{
	int idx = GetTabIndexOf(pWnd);
	if (idx < 0)
	{
		ASSERT_TRACE(FALSE, "CTaskBuilderDockPaneTabs::SetTabImageOf tab not found!");
		return;
	}

	HICON hIcon = ::LoadWalkIcon(nID);
	SetTabHicon(idx, hIcon);
	AdjustTabs();
}

//-----------------------------------------------------------------------------
CTaskBuilderTabWnd::TBStyle	CTaskBuilderTabWnd::GetTbStyle() const
{
	return m_TbStyle;
}

//-----------------------------------------------------------------------------
void CTaskBuilderTabWnd::SetTbStyle(TBStyle style)
{
	m_TbStyle = style;
}

//-----------------------------------------------------------------------------
void CTaskBuilderTabWnd::ModifyTabberStyle(int tbStyle)
{
	SetTbStyle((TBStyle) tbStyle);
	SetHasTopTabBorder(tbStyle < TB_STRIP);

	__super::ModifyTabStyle((Style) tbStyle);
}

//-----------------------------------------------------------------------------
int CTaskBuilderTabWnd::GetDefaultHeight() const
{
	return AfxGetThemeManager()->GetTabberTabHeight();
}

//------------------------------------------------------------------------------
CFont* CTaskBuilderTabWnd::GetTabFont()
{
	return AfxGetThemeManager()->GetTabberTabFont();
}

// poiche' potrei non avere ancora la CWnd disponibile, non posso usare la
// AddTab standard, ma devo creare le strutture BCG di base che rappresentano 
// le linguette (diciamo i loro DlgInfoItem)
//------------------------------------------------------------------------------
void CTaskBuilderTabWnd::AddTabByInfo(CTaskBuilderTabWndInfo* pInfo, int nPos /*-1*/)
{
	m_bDeleteTabInfo = TRUE;
	if (nPos >= 0)
		m_arTabs.InsertAt(nPos, pInfo);
	else
		m_arTabs.Add(pInfo);

	// devo aggiornare l'int che tiene quante tab ci sono
	m_iTabsNum++;

}

//------------------------------------------------------------------------------
COLORREF CTaskBuilderTabWnd::GetHoveringBkgColor() const
{
	return m_HoveringBkgColor;
}

//------------------------------------------------------------------------------
COLORREF CTaskBuilderTabWnd::GetHoveringForeColor() const
{
	return m_HoveringForeColor;
}

//---------------------------------------------------------------------- -
void CTaskBuilderTabWnd::SetHoveringBkgColor(COLORREF color)
{
	m_HoveringBkgColor = color;
}

//---------------------------------------------------------------------- -
void CTaskBuilderTabWnd::SetHoveringForeColor(COLORREF color)
{
	m_HoveringForeColor = color;
}

//------------------------------------------------------------------------------
COLORREF CTaskBuilderTabWnd::GetBestTabBkgColor(int nTab, BOOL isActive /*FALSE*/) const
{
	if (nTab == GetHighlightedTab())
		return GetHoveringBkgColor();
	
	return isActive ? GetActiveTabColor() : GetTabBkColor(GetActiveTab());
}

//------------------------------------------------------------------------------
COLORREF CTaskBuilderTabWnd::GetBestTabForeColor(int nTab, BOOL isActive /*FALSE*/) const
{
	if (nTab == GetHighlightedTab())
		return GetHoveringForeColor();

	return isActive ? GetActiveTabTextColor() : GetTabTextColor(nTab);
}

//------------------------------------------------------------------------------
BOOL CTaskBuilderTabWnd::HasTopTabBorder() const
{
	return m_HasTabTopBorder;
}

//------------------------------------------------------------------------------
void CTaskBuilderTabWnd::SetHasTopTabBorder(BOOL bValue)
{
	m_HasTabTopBorder = bValue;
}

//------------------------------------------------------------------------------
void CTaskBuilderTabWnd::AdjustTabTopBorder(CRect& aRect) const
{
	if (!HasTopTabBorder())
		aRect.top -= 2;	// ahimè il 2 e' cablato nella AdjustTabs BCG
}

//------------------------------------------------------------------------------
CBCGPTabInfo* CTaskBuilderTabWnd::FindTabInfo(int nTab)
{
	if (nTab < 0 || nTab >= m_arTabs.GetSize())
		return NULL;
	return (CBCGPTabInfo*)m_arTabs.GetAt(nTab);
}



