#include "stdafx.h"

#include <TbGeneric\DataObj.h>

#include "PARSOBJ.H"
#include "PARScbx.H"
#include "hlinkobj.h"
#include "basedoc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//						Class HotKeyLinkObj implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (HotKeyLinkObj, CObject)

//-----------------------------------------------------------------------------
HotKeyLinkObj::HotKeyLinkObj()
	:
	m_pDocument				(NULL),
	m_bHotLinkEnabled		(TRUE),
	m_bAddOnFlyEnabled		(TRUE),
	m_bIsAddOnFlyRunning	(FALSE),
	m_bSearchOnLinkEnabled	(TRUE),
	m_bMustExistData		(FALSE),
	m_wRunningMode			(0),
	m_bEnableFillListBox	(TRUE),
	m_bLikeOnDropDownEnabled (FALSE),
	m_bAutoFindable			(TRUE),
	m_bHyperLinkEnabled		(TRUE)
{
}

//----------------------------------------------------------------------------
HotKeyLinkObj::~HotKeyLinkObj()
{
	for (int i = 0; i < m_OwnerCtrls.GetSize(); i++)
	{
		CParsedCtrl* pOwnerCtrl = m_OwnerCtrls[i];
		if (pOwnerCtrl)
			pOwnerCtrl->m_pHotKeyLink = NULL;
	}
}

//----------------------------------------------------------------------------
void HotKeyLinkObj::DoOnCreatedOwnerCtrl	()	
{ 
	for (int i = 0; i < m_OwnerCtrls.GetSize(); i++)
	{
		CParsedCtrl* pOwnerCtrl = m_OwnerCtrls[i];
		if (pOwnerCtrl && pOwnerCtrl->GetCtrlCWnd() && pOwnerCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CMSStrCombo)))
		{
			m_bHotLinkEnabled = m_bAddOnFlyEnabled = m_bSearchOnLinkEnabled = m_bHyperLinkEnabled = FALSE;
			break;
		}
	}
	OnCreatedOwnerCtrl (); 
}

//----------------------------------------------------------------------------
DataObj* HotKeyLinkObj::GetAttachedData()
{
	return m_OwnerCtrls.GetCount() 
		? m_OwnerCtrls.GetAt(m_OwnerCtrls.GetUpperBound())->GetCtrlData() 
		: NULL;
}

//----------------------------------------------------------------------------
SqlRecord* HotKeyLinkObj::GetMasterRecord()
{
	CParsedCtrl* pControl = GetOwnerCtrl();
	return pControl ? pControl->m_pSqlRecord : NULL;
}
//----------------------------------------------------------------------------
CParsedCtrl* HotKeyLinkObj::GetOwnerCtrl() const
{
	return m_OwnerCtrls.GetCount()
		? m_OwnerCtrls.GetAt(m_OwnerCtrls.GetUpperBound())
		: NULL;
}
//----------------------------------------------------------------------------
void HotKeyLinkObj::SetActiveOwnerCtrl(CParsedCtrl*	pCtrl)
{
	bool bFound = false;
	for (int i = 0; i < m_OwnerCtrls.GetSize(); i++)
	{
		CParsedCtrl* pOwnerCtrl = m_OwnerCtrls[i];
		if (pOwnerCtrl == pCtrl)
		{
			bFound = true;
			if (i != m_OwnerCtrls.GetUpperBound())
			{
				m_OwnerCtrls.RemoveAt(i);
				m_OwnerCtrls.Add(pCtrl);
			}
			break;
		}
	}
	ASSERT(bFound);
}

//----------------------------------------------------------------------------
void HotKeyLinkObj::SetOwnerCtrl(CParsedCtrl* pCtrl)
{ 
	for (int i = 0; i < m_OwnerCtrls.GetSize(); i++)
	{
		if (m_OwnerCtrls[i] == pCtrl)
			return;
	}
	m_OwnerCtrls.Add(pCtrl);
}
//----------------------------------------------------------------------------
void HotKeyLinkObj::RemoveOwnerCtrl(CParsedCtrl* pCtrl)
{ 
	for (int i = 0; i < m_OwnerCtrls.GetSize(); i++)
	{
		CParsedCtrl* pOwnerCtrl = m_OwnerCtrls[i];
		if (pOwnerCtrl == pCtrl)
		{
			m_OwnerCtrls.RemoveAt(i);
			break;
		}
	}
}

//----------------------------------------------------------------------------
void HotKeyLinkObj::OnCustomizeRadarToolbar	(CTBTabbedToolbar* pTabbedToolbar)
{
	if (m_pDocument)
		m_pDocument->OnCustomizeRadarToolbar(pTabbedToolbar);
}

//----------------------------------------------------------------------------
BOOL HotKeyLinkObj::GetToolTipProperties(CTooltipProperties* pTp)
{
	return FALSE;
}

//----------------------------------------------------------------------------
void HotKeyLinkObj::OnPrepareForFind(SqlRecord* pRec) 
{
	if (m_pDocument)
		m_pDocument->OnPrepareForFind(this, pRec);
}

///////////////////////////////////////////////////////////////////////////////