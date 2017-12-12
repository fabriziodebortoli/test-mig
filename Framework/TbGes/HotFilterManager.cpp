#include "stdafx.h"

#include "BusinessServiceProvider.h"
#include "TileDialog.h"

// local declaration
#include "HotFiltermanager.h"
#include "HotFilter.h"
#include "JsonFormEngineEx.h"
#include "TbActivityDocument.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							 HotFilterManager
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(HotFilterManager, Array)

//-----------------------------------------------------------------------------
HotFilterManager::HotFilterManager(
	CAbstractFormDoc* pCallerDoc,
	BOOL			  bViewPanelUnpinnedFilters,
	UINT			  nIDCTileGroup,
	CString			  strPanelName
)
	:
	m_pCallerDoc(pCallerDoc),
	m_bViewPanelUnpinnedFilters(bViewPanelUnpinnedFilters),
	m_nIDCTileGroup(nIDCTileGroup),
	m_strPanelName(strPanelName)
{
}

//-----------------------------------------------------------------------------
HotFilterManager::~HotFilterManager()
{
}

//-----------------------------------------------------------------------------
HotFilterObj* HotFilterManager::GetHotFilter(const CString& strName, CRuntimeClass* pClass, const DataBool& bForce /*= FALSE*/)
{
	HotFilterObj* pHFL = GetExistHotFilter(strName);
	// Creo l'hotfilter solo se sono in UnattendedMode altrimenti bisogna attendere
	// che venga creato dall'interfaccia

	if (pHFL == NULL && (m_pCallerDoc->IsInUnattendedMode() || bForce))
		pHFL = Add(strName, pClass);

	return pHFL;
}

//-----------------------------------------------------------------------------
HotFilterObj* HotFilterManager::GetExistHotFilter(const CString& strName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		HotFilterObj* pHFL = (HotFilterObj*)GetAt(i);

		if (pHFL->GetName() == strName)
			return pHFL;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void HotFilterManager::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		HotFilterObj* pHFL = (HotFilterObj*)GetAt(i);
		pHFL->OnParsedControlCreated(pCtrl, m_pCallerDoc);
	}
}

//-----------------------------------------------------------------------------
HotFilterObj*  HotFilterManager::AddPickerColumn(const CString& strName, const CString& strColumnName, const CString strColumnTitle, CString strFieldName)
{
	HotFilterRange* pHF = (HotFilterRange*)GetExistHotFilter(strName);

	if (!pHF || !pHF->IsKindOf(RUNTIME_CLASS(HotFilterDateRange)))
	{
		ASSERT(FALSE);
		return NULL;
	}

	pHF->AddPickerColumn(strColumnName, strColumnTitle, strFieldName);
	return pHF;
}

//-----------------------------------------------------------------------------
HotFilterObj* HotFilterManager::Add(const CString& strName, CRuntimeClass* pHFClass, UINT nIdentificationIDC /*= 0*/)
{
	HotFilterObj* pHF = this->GetExistHotFilter(strName);
	if (pHF)
	{
		ASSERT_VALID(pHF);
		ASSERT(pHF->IsKindOf(pHFClass));
		return pHF;
	}

	pHF = dynamic_cast<HotFilterObj*>(pHFClass->CreateObject());
	if (!pHF)
	{
		ASSERT(FALSE);
		return NULL;
	}
	pHF->AttachDocument(m_pCallerDoc);
	pHF->AttachHFMngParent(this);
	pHF->SetName(strName);
	pHF->SetNotificationIDC(nIdentificationIDC);

	pHF->Customize();
	pHF->ResetCriteria();
	pHF->InitializeHotFilter();

	ASSERT(pHF->GetType() != EHotFilterType::HF_WRONG);

	Add(pHF);
	return pHF;
}

//-----------------------------------------------------------------------------
void HotFilterManager::OnBuildDataControlLinks(CTabDialog* pTabDlg)
{
	CTileGroup* pGroup = dynamic_cast<CTileGroup*>(pTabDlg->GetChildTileGroup());
	if (!pGroup)
	{
		//ASSERT(FALSE);
		return;
	}
	CTBNamespace ns = pTabDlg->GetNamespace();
	CString name = ns.ToString();
}

//-----------------------------------------------------------------------------
void HotFilterManager::OnBuildDataControlLinks(CAbstractFormView*  pView)
{
	CTileGroup* pGroup = pView->GetTileGroup(m_nIDCTileGroup);
	if (!pGroup)
	{
		//ASSERT(FALSE);
		return;
	}
	CTBNamespace ns = pView->GetNamespace();
	CString name = ns.ToString();
}

//-----------------------------------------------------------------------------
void HotFilterManager::CompleteQuery(const CString& strName, SqlTable* pTable, SqlRecord* pRec, const DataObj& aColumn)
{
	HotFilterObj* pHFL = GetExistHotFilter(strName);
	if (!pHFL)
	{
		return;
	}
	if (pHFL->IsEmptyQuery())
	{
		return;
	}

	pHFL->DefineQuery(pTable, pRec, aColumn);
	pHFL->PrepareQuery(pTable);
}

//-----------------------------------------------------------------------------
BOOL HotFilterManager::OnBeforeBatchExecute()
{
	BOOL bOK = TRUE;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		HotFilterObj* pHFL = (HotFilterObj*)GetAt(i);

		if (!pHFL->OnBeforeBatchExecute())
		{
			bOK = FALSE;
		}
	}

	if (!bOK)
		m_pCallerDoc->m_pMessages->Show();

	return bOK;
}

//-----------------------------------------------------------------------------
void HotFilterManager::OnPinUnpin(CBaseTileDialog* pTileDlg)
{
	if (pTileDlg->IsPinned())
		return;

	CString hfName = pTileDlg->GetNamespace().GetObjectName();
	HotFilterObj* pHF = this->GetExistHotFilter(hfName);
	if (pHF)
		pHF->ResetCriteria();
}

//-----------------------------------------------------------------------------

/*
//-----------------------------------------------------------------------------
void CHotFilterDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	//m_HF_Type = ((CHotFilterDescription*)pDesc)->m_HF_Type;
}
//-----------------------------------------------------------------------------
void CHotFilterDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_ENUM(m_HF_Type, szJsonHotFilterType, EHotFilterType::HF_RANGE_SIMPLE);

}
//-----------------------------------------------------------------------------
void CHotFilterDescription::ParseJson(CJsonFormParser& parser)
{
	PARSE_ENUM(m_HF_Type, szJsonHotFilterType, EHotFilterType);

}

//-----------------------------------------------------------------------------
CString CHotFilterDescription::GetEnumDescription(EHotFilterType value)
{
	switch(value)
	{
	case HF_WRONG: return _T("Wrong");
	case HF_DATE_SIMPLE: return _T("Date Simple");
	case HF_DATE_WITHSELECTION: return _T("Date With Selection");
	case HF_RANGE_SIMPLE: return _T("Range Simple");
	case HF_RANGE_WITHSELECTION: return _T("Range With Selection");
	case HF_RANGE_WITHPICKER: return _T("Range With Picker");
	case HF_LISTBOX: return _T("List Box");
	case HF_LIST_POPUP: return _T("List Popup");
	case HF_CHECKLISTBOX: return _T("Chect List Box");
	case HF_CUSTOM: return _T("Custom");
	case HF_ARRAY: return _T("Array");
	}
	return _T("");
}
*/
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterDescription, CWndTileDescription)
//-----------------------------------------------------------------------------
REGISTER_WND_OBJ_CLASS(CHotFilterDescription, HotFilter)