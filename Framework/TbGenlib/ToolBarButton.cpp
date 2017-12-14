
#include "stdafx.h"

#include <TbGeneric\Globals.h>
#include <TbGeneric\JsonFormEngine.h>
#include "BASEAPP.H"
#include "toolbarbutton.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

////////////////////////////////////////////////////////////////////////////////
//				class CButtonInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CButtonInfo, CObject);

//-----------------------------------------------------------------------------
CButtonInfo::CButtonInfo
(
	UINT nCommandID,
	const CString& sName,
	const CString& sText,
	const CString& sImageNameSpace,
	const CString& sToolBarName,
	const CString& sToolTip,
	BOOL	bDropdown
)
	:
	m_nCommandID(nCommandID),
	m_sImageNameSpace(sImageNameSpace),
	m_sText(sText),
	m_sName(sName),
	m_sToolbarName(sToolBarName),
	m_sToolTip(sToolTip),
	m_bDropdown(bDropdown)
{

}

//-----------------------------------------------------------------------------
CButtonInfo::~CButtonInfo()
{
}

////////////////////////////////////////////////////////////////////////////////
//				class CComboInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CComboInfo, CObject);

//-----------------------------------------------------------------------------
CComboInfo::CComboInfo
(
	UINT nID,
	const CString& aLibNamespace,
	const CString& sName,
	int nWidth,
	DWORD dwStyle,
	const CString& sToolBarName
)
	:
	m_nID(nID),
	m_libNamespace(aLibNamespace),
	m_sName(sName),
	m_nWidth(nWidth),
	m_dwStyle(dwStyle),
	m_sToolBarName(sToolBarName)
{
}

CComboInfo::~CComboInfo()
{
}

////////////////////////////////////////////////////////////////////////////////
//				class CComboInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CEditInfo, CObject);

//-----------------------------------------------------------------------------
CEditInfo::CEditInfo
(
	UINT nID,
	const CString& aLibNamespace,
	const CString& sName,
	int nWidth,
	DWORD dwStyle,
	const CString& sToolBarName
)
	:
	m_nID(nID),
	m_libNamespace(aLibNamespace),
	m_sName(sName),
	m_nWidth(nWidth),
	m_dwStyle(dwStyle),
	m_sToolBarName(sToolBarName)
{
}

CEditInfo::~CEditInfo()
{
}

////////////////////////////////////////////////////////////////////////////////
//				class CLabelInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CLabelInfo, CObject);

//-----------------------------------------------------------------------------
CLabelInfo::CLabelInfo
(
	UINT nID,
	CString sText,
	CString sToolBarName
) :
	m_nID(nID),
	m_sText(sText),
	m_sToolBarName(sToolBarName)
{
}

//-----------------------------------------------------------------------------
CLabelInfo::~CLabelInfo()
{
}

////////////////////////////////////////////////////////////////////////////////
//				class CMenuItemInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDropdownMenuItemInfo, CObject);

//-----------------------------------------------------------------------------
CDropdownMenuItemInfo::CDropdownMenuItemInfo(UINT nCommandID, UINT_PTR nIDNewItem, CString sNewItem, CString sToolBarName)
	:
	m_nCommandID(nCommandID),
	m_nIDNewItem(nIDNewItem),
	m_sNewItem(sNewItem),
	m_sToolBarName(sToolBarName)
{
}

//-----------------------------------------------------------------------------
CDropdownMenuItemInfo::~CDropdownMenuItemInfo()
{
}
////////////////////////////////////////////////////////////////////////////////
//				class CSeparatorInfo implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CSeparatorInfo, CObject);

CSeparatorInfo::CSeparatorInfo(CString sToolBarName)
	:
	m_sToolBarName(sToolBarName)
{
}

CSeparatorInfo::~CSeparatorInfo()
{
}

////////////////////////////////////////////////////////////////////////////////
//				class CToolBarButtons implementation
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CToolBarButtons::CToolBarButtons()
{
}

// il contenuto dei due array sarà deletato dal frame proprietario della toolbar a cui
// sono inseriti i nuovi bottoni
//-----------------------------------------------------------------------------
CToolBarButtons::~CToolBarButtons()
{
}

//-----------------------------------------------------------------------------
void CToolBarButtons::AddButton
(
	UINT nCommandID,
	const CString& sName,
	const CString& sText,
	const CString& sImageNameSpace,
	const CString& sToolBarName,
	const CString& sToolTip,
	BOOL	bDropdown
)
{
	m_ButtonsInfo.Add(new CButtonInfo(
		nCommandID,
		sName,
		sText,
		sImageNameSpace,
		sToolBarName,
		sToolTip,
		bDropdown
	));
}

//-----------------------------------------------------------------------------
void CToolBarButtons::AddComboBox
(
	UINT nID,
	const CString& aLibNamespace,
	const CString& sName,
	int nWidth,
	DWORD dwStyle,
	const CString& sToolBarName
)
{
	m_ButtonsInfo.Add(new CComboInfo(nID, aLibNamespace, sName, nWidth, dwStyle, sToolBarName));
}

//-----------------------------------------------------------------------------
void CToolBarButtons::AddEdit
(
	UINT	nID,
	const CString& aLibNamespace,
	const CString& sName,
	int nWidth,
	DWORD dwStyle,
	const CString& sToolBarName
)
{
	m_ButtonsInfo.Add(new CEditInfo(nID, aLibNamespace, sName, nWidth, dwStyle, sToolBarName));
}


//-----------------------------------------------------------------------------
void CToolBarButtons::AddLabel
(
	UINT nID, const CString& sText, const CString& sToolBarName
)
{
	m_ButtonsInfo.Add(new CLabelInfo(nID, sText, sToolBarName));
}

//-----------------------------------------------------------------------------
void CToolBarButtons::AddSeparator(const CString& sToolBarName)
{
	m_ButtonsInfo.Add(new CSeparatorInfo(sToolBarName));
}
//-----------------------------------------------------------------------------
void CToolBarButtons::AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName)
{
	m_ButtonsInfo.Add(new CDropdownMenuItemInfo(nCommandID, nIDNewItem, sNewItem, sToolBarName));
}



// nel caso di utilizzo di un'istanza di questa classe da parte di un ClientDoc, la toolbar
// a cui i bottoni sono aggiunti, viene conosciuta solo nel momento in cui viene chiamato il
// metodo CreateNewButtons. Negli altri casi viene passata come parametro nel costruttore
//-----------------------------------------------------------------------------
BOOL CToolBarButtons::CreateNewButtons(CTBTabbedToolbar* pTabbedBar)
{
	BOOL bRet = TRUE;
	for (int i = 0; i <= m_ButtonsInfo.GetUpperBound(); i++)
	{
		CObject* pObj = m_ButtonsInfo.GetAt(i);
		ASSERT(pObj);

		// append new button in toolbar
		if (pObj->IsKindOf(RUNTIME_CLASS(CButtonInfo)))
		{
			CButtonInfo* pButtonInfo = (CButtonInfo*)pObj;
			if (pButtonInfo)
			{
				CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pButtonInfo->m_sToolbarName);
				if (!pToolBar->ExButton(pButtonInfo->m_nCommandID))
				{
					bRet = pToolBar->AddButton
					(
						pButtonInfo->m_nCommandID,
						pButtonInfo->m_sName,
						pButtonInfo->m_sImageNameSpace,
						pButtonInfo->m_sText,
						pButtonInfo->m_sToolTip
					) && bRet;
					
					if (pButtonInfo->m_bDropdown)
						pToolBar->SetDropdown(pButtonInfo->m_nCommandID);
				}
			}

		}
		// append new combo in toolbar
		else if (pObj->IsKindOf(RUNTIME_CLASS(CComboInfo)))
		{
			CComboInfo* pComboInfo = (CComboInfo*)pObj;
			CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pComboInfo->m_sToolBarName);
			bRet = pToolBar && pToolBar->AddComboBox(pComboInfo->m_nID, pComboInfo->m_libNamespace, pComboInfo->m_sName, pComboInfo->m_nWidth, pComboInfo->m_dwStyle) && bRet;
		}
		// append new edit in toolbar
		else if (pObj->IsKindOf(RUNTIME_CLASS(CEditInfo)))
		{
			CEditInfo* pEditInfo = (CEditInfo*)pObj;
			CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pEditInfo->m_sToolBarName);
			bRet = pToolBar && pToolBar->AddEdit(pEditInfo->m_nID, pEditInfo->m_libNamespace, pEditInfo->m_sName, pEditInfo->m_nWidth, pEditInfo->m_dwStyle) && bRet;
		}
		// append new label in toolbar
		else if (pObj->IsKindOf(RUNTIME_CLASS(CLabelInfo)))
		{
			CLabelInfo* pLabelInfo = (CLabelInfo*)pObj;
			CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pLabelInfo->m_sToolBarName);
			bRet = pToolBar && pToolBar->AddLabel(pLabelInfo->m_nID, pLabelInfo->m_sText) && bRet;
		} 
		// append new menu item in toolbar
		else if (pObj->IsKindOf(RUNTIME_CLASS(CDropdownMenuItemInfo)))
		{
			CDropdownMenuItemInfo* pItemInfo = (CDropdownMenuItemInfo*)pObj;
			CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pItemInfo->m_sToolBarName);
			bRet = pToolBar && pToolBar->AddDropdownMenuItem(pItemInfo->m_nCommandID, MF_STRING, pItemInfo->m_nIDNewItem, pItemInfo->m_sNewItem) && bRet;
		}
		// append separetor
		else if (pObj->IsKindOf(RUNTIME_CLASS(CSeparatorInfo)))
		{
			CSeparatorInfo* pSeparatorInfo = (CSeparatorInfo*)pObj;
			CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(pTabbedBar, pSeparatorInfo->m_sToolBarName);
			pToolBar->AddSeparator();
		}
		else
		{
			ASSERT(FALSE);
			bRet = FALSE;
		}

	}
	return bRet;
}

