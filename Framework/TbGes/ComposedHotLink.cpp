#include "stdafx.h"
#include "EXTDOC.H"
#include "ComposedHotLink.h"


//=============================================================================
//							ComposedHotLink
//=============================================================================

IMPLEMENT_DYNCREATE(ComposedHotLink, HotKeyLink)

//-----------------------------------------------------------------------------
ComposedHotLink::ComposedHotLink()
{
	m_pRecord = new SqlVirtualRecord(_T("LocalRecord"));
}
//-----------------------------------------------------------------------------
ComposedHotLink::~ComposedHotLink()
{
	if (m_pSelector)
		m_pSelector->DetachEvents(this);

	POSITION pos = m_Mappings.GetStartPosition();
	while (pos)
	{
		CString sKey;
		ASSOCIATIONS* pValue;
		m_Mappings.GetNextAssoc(pos, sKey, pValue);
		delete pValue;
	}
	pos = m_HotLinks.GetStartPosition();
	while (pos)
	{
		CString sKey;
		HotKeyLink* pValue;
		m_HotLinks.GetNextAssoc(pos, sKey, pValue);
		delete pValue;
	}

}
//-----------------------------------------------------------------------------
void ComposedHotLink::Signal(CObservable* pSender, EventType eType)
{
	if (pSender == m_pSelector && eType == EventType::ON_CHANGED)
		SelectHotLink();
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnDefineQuery(SelectionType nQuerySelection /*= DIRECT_ACCESS*/)
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnDefineQuery(nQuerySelection);
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection /*= DIRECT_ACCESS*/)
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnPrepareQuery(pDataObj, nQuerySelection);
}

//-----------------------------------------------------------------------------
BOOL ComposedHotLink::IsValid()
{
	if (!m_pCurrentHKL)
		return FALSE;

	return m_pCurrentHKL->IsValid();
}
//-----------------------------------------------------------------------------
BOOL ComposedHotLink::OnValidateRadarSelection(SqlRecord* pRec)
{
	if (!m_pCurrentHKL)
		return FALSE;

	return m_pCurrentHKL->OnValidateRadarSelection(pRec);
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnRecordAvailable()
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnRecordAvailable();
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnCallLink()
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnCallLink();
}

//-----------------------------------------------------------------------------
HotKeyLink::FindResult ComposedHotLink::FindRecord(DataObj* pDataObj, BOOL bCallLink /*= FALSE*/, BOOL bFromControl /*= FALSE*/, BOOL bAllowRunningModeForInternalUse /*= FALSE*/)
{
	if (!m_pCurrentHKL)
		return HotKeyLink::FindResult::EMPTY;
	HotKeyLink::FindResult res = m_pCurrentHKL->FindRecord(pDataObj, bCallLink, bFromControl, bAllowRunningModeForInternalUse);
	OnPrepareAuxData();
	return res;
}
//-----------------------------------------------------------------------------
bool ComposedHotLink::FindNeeded(DataObj* pDataObj, SqlRecord* pMasterRec)
{
	return m_pCurrentHKL
		? m_pCurrentHKL->FindNeeded(pDataObj, pMasterRec)
		: __super::FindNeeded(pDataObj, pMasterRec);
}

//-----------------------------------------------------------------------------
BOOL ComposedHotLink::SearchOnLink(DataObj* pData /*= NULL*/, SelectionType nQuerySelection /*= NO_SEL*/)
{
	if (!m_pCurrentHKL)
		return FALSE;

	return m_pCurrentHKL->SearchOnLink(pData, nQuerySelection);
}
//-----------------------------------------------------------------------------
int ComposedHotLink::SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions)
{
	if (!m_pCurrentHKL)
		return -1;

	return m_pCurrentHKL->SearchComboQueryData(nMaxItems, pKeyData, arDescriptions);
}
//-----------------------------------------------------------------------------
SqlParamArray* ComposedHotLink::GetQuery(SelectionType nQuerySelection, CString& sQuery, CString sFilter /*= _T("")*/)
{
	if (!m_pCurrentHKL)
		return NULL;

	return m_pCurrentHKL->GetQuery(nQuerySelection, sQuery, sFilter);
}

//-----------------------------------------------------------------------------
const CString ComposedHotLink::GetAddOnFlyNamespace()
{
	if (!m_pCurrentHKL)
		return __super::GetAddOnFlyNamespace();

	return m_pCurrentHKL->GetAddOnFlyNamespace();
}

//-----------------------------------------------------------------------------
void ComposedHotLink::GetAuxInfoForHklBrowse(LPAUXINFO& pInfo)
{
	if (!m_pCurrentHKL)
		return __super::GetAuxInfoForHklBrowse(pInfo);

	return m_pCurrentHKL->GetAuxInfoForHklBrowse(pInfo);
}
//-----------------------------------------------------------------------------
void ComposedHotLink::DoCallLink(BOOL bAskForCallLink /*= FALSE*/)
{
	if (!m_pCurrentHKL)
		return __super::DoCallLink(bAskForCallLink);

	return m_pCurrentHKL->DoCallLink(bAskForCallLink);
}

//-----------------------------------------------------------------------------
CDocument* ComposedHotLink::BrowserLink
(
	DataObj*		pData,
	CDocument*		pFormDoc, /*= NULL*/
	const	CRuntimeClass*	pViewClass, /*= NULL*/
	BOOL			bActivate   /*= TRUE*/
)
{
	if (!m_pCurrentHKL)
		return __super::BrowserLink(pData, pFormDoc, pViewClass, bActivate);

	return m_pCurrentHKL->BrowserLink(pData, pFormDoc, pViewClass, bActivate);
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnRadarRecordAvailable()
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnRadarRecordAvailable();
}

//-----------------------------------------------------------------------------
void ComposedHotLink::CloseTable()
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->CloseTable();
}

//-----------------------------------------------------------------------------
BOOL ComposedHotLink::ExistData(DataObj* pData)
{
	if (!m_pCurrentHKL)
		return FALSE;
	OnPrepareForFind(GetMasterRecord());
	BOOL b = m_pCurrentHKL->ExistData(pData);
	OnPrepareAuxData();
	return b;
}

//-----------------------------------------------------------------------------
BOOL ComposedHotLink::SearchOnLinkUpper()
{
	if (!m_pCurrentHKL)
		return FALSE;
	OnPrepareForFind(GetMasterRecord());
	return m_pCurrentHKL->SearchOnLinkUpper();
}
//-----------------------------------------------------------------------------
BOOL ComposedHotLink::SearchOnLinkLower()
{
	if (!m_pCurrentHKL)
		return FALSE;
	OnPrepareForFind(GetMasterRecord());
	return m_pCurrentHKL->SearchOnLinkLower();
}
//-----------------------------------------------------------------------------
HotKeyLink::FindResult ComposedHotLink::OnRecordNotFound()
{
	if (!m_pCurrentHKL)
		return HotKeyLink::FindResult::EMPTY;

	return m_pCurrentHKL->OnRecordNotFound();
}
//-----------------------------------------------------------------------------
void ComposedHotLink::OnPrepareForFind(SqlRecord* pRec)
{
	if (m_pCurrentHKL)
		m_pCurrentHKL->OnPrepareForFind(pRec);
}

//-----------------------------------------------------------------------------
void ComposedHotLink::OnPrepareAuxData()
{
	if (m_pSelector)
	{
		CString sName = m_pSelector->Str();
		ASSOCIATIONS* pList = NULL;
		if (m_Mappings.Lookup(sName, pList))
		{
			for (int i = 0; i < pList->GetCount(); i++)
			{
				DataObjAssociation* pAsso = pList->GetAt(i);
				pAsso->m_pCompositeHKL->Assign(*pAsso->m_pHKL);
			}
		}
	}
	__super::OnPrepareAuxData();

}

//-----------------------------------------------------------------------------
void ComposedHotLink::SelectHotLink()
{
	if (!m_pSelector)
		return;
	CString sNamespace, sName = m_pSelector->Str();
	HotKeyLink* pNewHKL = NULL;
	if (sName.IsEmpty())
		pNewHKL = NULL;
	else if (!m_HotLinks.Lookup(sName, pNewHKL))
	{
		ASSERT(FALSE);
		TRACE1("Invalid HotLink name: %s", (LPCTSTR)sName);
	}
	bool buttonChanged = false;
	if (pNewHKL != m_pCurrentHKL)
	{
		buttonChanged = !m_pCurrentHKL || !pNewHKL;
		if (m_pCurrentHKL)
		{
			m_pCurrentHKL->m_OwnerCtrls.RemoveAll();
		}
		if (pNewHKL)
		{
			pNewHKL->m_OwnerCtrls.Append(m_OwnerCtrls);
		}

		m_pCurrentHKL = pNewHKL;
	}
	if (buttonChanged)
	{
		int nButtonId = m_pCurrentHKL
			? (m_nButtonId == BTN_DEFAULT ? BTN_DOUBLE_ID : m_nButtonId)
			: NO_BUTTON;
		for (int i = 0; i < m_OwnerCtrls.GetCount(); i++)
		{
			m_OwnerCtrls[i]->ReAttachButton(nButtonId);
		}
	}

}

//-----------------------------------------------------------------------------
void ComposedHotLink::AddHotLink(const CString& sName, HotKeyLink* pHotLink)
{
	m_HotLinks[sName] = pHotLink;
	pHotLink->m_pOwner = this;
	pHotLink->AttachDocument(m_pDocument);
}

//-----------------------------------------------------------------------------
void ComposedHotLink::MapField(const CString& sHKLName, const CString& sName, DataObj& sourceDataObj)
{
	DataObj* pLocal = m_pRecord->GetDataObjFromColumnName(sName);
	if (pLocal)
	{
		if (pLocal->GetDataType() != sourceDataObj.GetDataType())
		{
			TRACE2("Type mismatch for field %s in hotlink %s", (LPCTSTR)sName, (LPCTSTR)sHKLName);
			ASSERT(FALSE);
			return;
		}
	}
	else
	{
		pLocal = sourceDataObj.Clone();
		m_pRecord->BindDynamicDataObj(sName, *pLocal, sourceDataObj.GetColumnLen());
	}
	ASSOCIATIONS* pList = NULL;
	if (!m_Mappings.Lookup(sHKLName, pList))
	{
		pList = new ASSOCIATIONS;
		m_Mappings[sHKLName] = pList;
	}

	pList->Add(new DataObjAssociation(pLocal, &sourceDataObj));
}
//----------------------------------------------------------------------------
void ComposedHotLink::RemoveOwnerCtrl(CParsedCtrl* pCtrl)
{
	__super::RemoveOwnerCtrl(pCtrl);
	if (m_pCurrentHKL)
		m_pCurrentHKL->RemoveOwnerCtrl(pCtrl);
}
//----------------------------------------------------------------------------
void ComposedHotLink::SetActiveOwnerCtrl(CParsedCtrl* pCtrl)
{
	__super::SetActiveOwnerCtrl(pCtrl);
	if (m_pCurrentHKL)
		m_pCurrentHKL->SetActiveOwnerCtrl(pCtrl);
}
//----------------------------------------------------------------------------
void ComposedHotLink::SetOwnerCtrl(CParsedCtrl* pCtrl)
{
	__super::SetOwnerCtrl(pCtrl);
	if (m_pCurrentHKL)
		m_pCurrentHKL->SetOwnerCtrl(pCtrl);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableFillListBox(BOOL bEnable)
{
	m_bEnableFillListBox = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableFillListBox(bEnable);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableAddOnFly(BOOL bEnable)
{
	m_bAddOnFlyEnabled = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableAddOnFly(bEnable);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableHotLink(BOOL bEnable)
{
	m_bHotLinkEnabled = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableHotLink(bEnable);
}
//----------------------------------------------------------------------------
void ComposedHotLink::MustExistData(BOOL bExist)
{
	m_bMustExistData = bExist;
	if (m_pCurrentHKL)
		m_pCurrentHKL->MustExistData(bExist);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableSearchOnLink(BOOL bEnable)
{
	m_bSearchOnLinkEnabled = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableSearchOnLink(bEnable);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableLikeOnDropDown(BOOL bEnable)
{
	m_bLikeOnDropDownEnabled = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableLikeOnDropDown(bEnable);
}

//----------------------------------------------------------------------------
void ComposedHotLink::SetAddOnFlyRunning(const BOOL& bSet)
{
	m_bIsAddOnFlyRunning = bSet;
	if (m_pCurrentHKL)
		m_pCurrentHKL->SetAddOnFlyRunning(bSet);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableHyperLink(BOOL bEnable)
{
	m_bHyperLinkEnabled = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableHyperLink(bEnable);
}
//----------------------------------------------------------------------------
void ComposedHotLink::EnableAutoFind(BOOL bEnable)
{
	m_bAutoFindable = bEnable;
	if (m_pCurrentHKL)
		m_pCurrentHKL->EnableAutoFind(bEnable);
}


//----------------------------------------------------------------------------
void ComposedHotLink::SetRunningMode(WORD runningMode)
{
	m_wRunningMode = runningMode;
	if (m_pCurrentHKL)
		m_pCurrentHKL->SetRunningMode(runningMode);
}
//----------------------------------------------------------------------------
WORD ComposedHotLink::GetRunningMode()
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->GetRunningMode();
	return m_wRunningMode;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsFillListBoxEnabled() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsFillListBoxEnabled();
	return m_bEnableFillListBox;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsMustExistData()	const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsMustExistData();
	return m_bMustExistData;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsHotLinkEnabled() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsHotLinkEnabled();
	return m_bHotLinkEnabled;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsHotLinkRunning() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsHotLinkRunning();
	return m_wRunningMode != 0;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsSearchOnLinkEnabled() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsSearchOnLinkEnabled();
	return m_bSearchOnLinkEnabled;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsLikeOnDropDownEnabled()	const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsLikeOnDropDownEnabled();
	return m_bLikeOnDropDownEnabled;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsHyperLinkEnabled() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsHyperLinkEnabled();
	return m_bHyperLinkEnabled;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsAutoFindable() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsAutoFindable();
	return m_bAutoFindable;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsEnabledAddOnFly() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsEnabledAddOnFly();
	return m_bAddOnFlyEnabled;
}
//----------------------------------------------------------------------------
BOOL ComposedHotLink::IsAddOnFlyRunning() const
{
	if (m_pCurrentHKL)
		return m_pCurrentHKL->IsAddOnFlyRunning();
	return m_bIsAddOnFlyRunning;
}
//=============================================================================
//							ComposedJsonHotLink
//=============================================================================

IMPLEMENT_DYNCREATE(ComposedJsonHotLink, ComposedHotLink)

void ComposedJsonHotLink::Parameterize(HotLinkInfo* pInfo, int buttonId)
{
	if (m_bParameterized)
		return;
	__super::Parameterize(pInfo, buttonId);
	ASSERT(!m_pSelector);
	if (!m_sSelectorVar.IsEmpty() && m_pDocument)
	{
		m_pSelector = ((CAbstractFormDoc*)m_pDocument)->GetVariableValue(m_sSelectorVar);
		if (!m_pSelector)
		{
			//se non trovo la variabile, forse è perché non ho ancora agganciato i client doc, lo farò dopo e riparameterizzerò l'hotlink
			ASSERT(((CAbstractFormDoc*)m_pDocument)->m_pClientDocs->GetCount() == 0);
			return;
		}
		m_pSelector->AttachEvents(this);
	}
	for (int i = 0; i < pInfo->m_arHotlinks.GetSize(); i++)
	{
		HotLinkInfo* pChild = pInfo->m_arHotlinks[i];
		HotKeyLink* pHKL = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(CTBNamespace(CTBNamespace::HOTLINK, pChild->m_strNamespace));
		if (!pHKL)
		{
			TRACE1("Invalid hotlink: %s\n", (LPCTSTR)pChild->m_strNamespace);
			ASSERT(FALSE);
			continue;
		}
		pHKL->Parameterize(pInfo, buttonId);//prima applico quelle del composito, comuni
		pHKL->Parameterize(pChild, buttonId);//poi quelle specifiche
		AddHotLink(pChild->m_strName, pHKL);
		if (pInfo->m_arDescriptionFields.GetCount() == pChild->m_arDescriptionFields.GetCount())
		{
			for (int i = 0; i < pInfo->m_arDescriptionFields.GetCount(); i++)
			{
				DataObj* pPhysicalField = pHKL->GetAttachedRecord()->GetDataObjFromName(pChild->m_arDescriptionFields[i]);
				if (!pPhysicalField)
				{
					TRACE2("Field not found: %s in hotlink %s\n", (LPCTSTR)pChild->m_arDescriptionFields[i], (LPCTSTR)pChild->m_strName);
					ASSERT(FALSE);
					continue;
				}
				//mappo il campo locale sul corrispondente campo 'fisico' dell'hotlink
				MapField(pChild->m_strName, pInfo->m_arDescriptionFields[i], *pPhysicalField);
			}
		}
		else
		{
			TRACE2("Field number mismatch for hotlinks: %s - %s\n", (LPCTSTR)pInfo->m_strName, (LPCTSTR)pChild->m_strName);
			ASSERT(FALSE);
		}

	}
	SelectHotLink();
	m_bParameterized = true;
}