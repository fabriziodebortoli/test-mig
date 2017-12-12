#include "stdafx.h"

#include <io.h>

#include "ReportObjectsInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//							CReportMenuNode 
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CReportMenuNode, CObject)

//----------------------------------------------------------------------------------------------
CReportMenuNode::CReportMenuNode()
	:
	m_pSons			(NULL),
	m_sNodeTag		(_T("")),
	m_bVisible		(TRUE),
	m_pFather		(NULL)
{}

//----------------------------------------------------------------------------------------------
CReportMenuNode::~CReportMenuNode()
{
	if (m_pSons)
	{
		m_pSons->RemoveAll();
		SAFE_DELETE(m_pSons);
	}
}

//----------------------------------------------------------------------------------------------
CString	CReportMenuNode::GetNodeTag()
{
	return m_sNodeTag;
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::SetNodeTag(CString sNodeTag)
{
	m_sNodeTag = sNodeTag;
}

//----------------------------------------------------------------------------------------------
BOOL CReportMenuNode::IsVisible()
{
	return m_bVisible;
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::SetVisible(BOOL bVisible /*= TRUE*/)
{
	m_bVisible = bVisible;
}

//----------------------------------------------------------------------------------------------
BOOL CReportMenuNode::GetUseSubMenu()
{
	return m_bUseSubMenu;
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::SetUseSubMenu(BOOL bUseSubMenu)
{
	m_bUseSubMenu = bUseSubMenu;
}

//----------------------------------------------------------------------------------------------
BOOL CReportMenuNode::IsLeaf()
{
	return !m_pSons || (m_pSons->GetUpperBound() < 0);
}

//----------------------------------------------------------------------------------------------
BOOL CReportMenuNode::IsRoot()
{
	return !m_pFather;
}

//----------------------------------------------------------------------------------------------
int CReportMenuNode::GetMaxDepth()
{
	if (IsLeaf())
		return 1;

	CReportMenuNode* pCurrentNode(NULL);
	int currSubTreeMaxDepth(0);
	int maxSubTreeMaxDepth(0);
	for (int i = 0; i <= GetSonsUpperBound(); i++)
	{
		pCurrentNode = dynamic_cast<CReportMenuNode*>(GetSonAt(i));
		currSubTreeMaxDepth = pCurrentNode->GetMaxDepth();
		if (currSubTreeMaxDepth > maxSubTreeMaxDepth)
			maxSubTreeMaxDepth = currSubTreeMaxDepth;
	}
	return 1 + maxSubTreeMaxDepth;
}

//----------------------------------------------------------------------------------------------
int CReportMenuNode::GetSonsUpperBound()
{
	if (!m_pSons)
		return -1;

	return m_pSons->GetUpperBound();
}

//----------------------------------------------------------------------------------------------
CReportMenuNode* CReportMenuNode::GetSonAt(int nSonIdx)
{
	if (nSonIdx < 0 || nSonIdx > GetSonsUpperBound())
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	CReportMenuNode* pSon = dynamic_cast<CReportMenuNode*>(m_pSons->GetAt(nSonIdx));

	if (!pSon)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return pSon;
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::AddSon(CReportMenuNode* pSonToAdd)
{
	if (!m_pSons)
		m_pSons = new Array;

	m_pSons->Add(pSonToAdd);
}

//----------------------------------------------------------------------------------------------
BOOL CReportMenuNode::RemoveSonAt(int nSonIdx)
{
	if (nSonIdx < 0 || nSonIdx > GetSonsUpperBound())
		return FALSE;

	m_pSons->RemoveAt(nSonIdx);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::RemoveAllSons()
{
	if (m_pSons)
	{
		for (int i = GetSonsUpperBound(); i >= 0; i--)
		{
			if (!GetSonAt(i)->IsLeaf())
				GetSonAt(i)->RemoveAllSons();

			RemoveSonAt(i);
		}
	}
}

//----------------------------------------------------------------------------------------------
void CReportMenuNode::InsertSonAt(int nSonIdx, CReportMenuNode* pSonToAdd)
{
	if (!m_pSons)
		m_pSons = new Array;

	m_pSons->InsertAt(nSonIdx, pSonToAdd);
}

//----------------------------------------------------------------------------------------------
//							CDocumentReportGroupDescription 
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDocumentReportGroupDescription, CObject)

//----------------------------------------------------------------------------------------------
CDocumentReportGroupDescription::CDocumentReportGroupDescription()
	:
	m_nId(0),
	m_sLocalize(_T("")),
	m_bUseSubMenu(FALSE)
{ }

//----------------------------------------------------------------------------------------------
CDocumentReportGroupDescription::CDocumentReportGroupDescription(int nId, CString sLocalize, BOOL bUseSubmenu)
	:
	m_nId(nId),
	m_sLocalize(sLocalize),
	m_bUseSubMenu(bUseSubmenu)

{ }

//----------------------------------------------------------------------------------------------
CDocumentReportGroupDescription::~CDocumentReportGroupDescription()	
{ }

//----------------------------------------------------------------------------------------------
//							CDocumentReportDescription 
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDocumentReportDescription, CFunctionDescription)

//----------------------------------------------------------------------------------------------
CDocumentReportDescription::CDocumentReportDescription ()
	:
	CFunctionDescription(CTBNamespace::REPORT)
{
	m_bIsDefault = FALSE;
	m_GroupDescription.SetId(0);
	m_GroupDescription.SetLocalize(_T(""));
	m_GroupDescription.SetUseSubMenu(FALSE);
}

//----------------------------------------------------------------------------------------------
CDocumentReportDescription&	CDocumentReportDescription::Assign(const CDocumentReportDescription& fd)
{
	m_bIsDefault = fd.m_bIsDefault;
	SetGroupDescription(fd.GetGroupDescription());
	CFunctionDescription::Assign(fd);
	return *this;
}

//----------------------------------------------------------------------------------------------
void CDocumentReportDescription::SetGroupDescription(const CDocumentReportGroupDescription& nGroupDescription)
{
	m_GroupDescription.SetId		(nGroupDescription.GetId());
	m_GroupDescription.SetLocalize	(nGroupDescription.GetLocalize());
	m_GroupDescription.SetUseSubMenu(nGroupDescription.GetUseSubMenu());
}

//----------------------------------------------------------------------------------------------
void CDocumentReportDescription ::SetDefault (const BOOL& bValue)
{
	m_bIsDefault = bValue;
}

//----------------------------------------------------------------------------------------------
//	class CBaseReportDescriptionArray implementation
//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CBaseReportDescriptionArray::GetDefault()
{
	CDocumentReportDescription* pDef;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pDef = GetAt(i);
		if (pDef->IsDefault())
			return pDef;
	}

	return NULL;
}
//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CBaseReportDescriptionArray::GetInfo (const CTBNamespace& aNS) const
{
	CDocumentReportDescription* pInfo;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pInfo = GetAt(i);
		if (aNS == pInfo->GetNamespace())
			return pInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CBaseReportDescriptionArray::GetInfo (const CString& sName) const
{
	CDocumentReportDescription* pInfo;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pInfo = GetAt(i);
		if (sName.CompareNoCase(pInfo->GetName()) == 0)
			return pInfo;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
void CBaseReportDescriptionArray::Assign (const CBaseReportDescriptionArray& ar)
{
	RemoveAll ();

	CDocumentReportDescription* pDescri;
	for (int i=0; i <= ar.GetUpperBound(); i++)
	{
		pDescri = ar.GetAt(i);
		//pNewDescri = new CDocumentReportDescription(*pDescri);
		//VERIFY(pNewDescri = (CDocumentReportDescription*) pDescri->GetRuntimeClass()->CreateObject());
		//*pNewDescri = *pDescri;
		CDocumentReportDescription* pNewDescri = new CDocumentReportDescription(*pDescri);
		Add (pNewDescri);		
	}
}

//----------------------------------------------------------------------------------------------
BOOL CBaseReportDescriptionArray::IsEqual (const CBaseReportDescriptionArray& ar)
{
	BOOL bEqual = GetSize () == ar.GetSize();

	if (bEqual)
		for (int i=0; i <= ar.GetUpperBound(); i++)
			if (GetAt(i) != ar.GetAt(i))
				return FALSE;

	return  bEqual;
}

//--------------------------------------------------------------------
CBaseReportDescriptionArray& CBaseReportDescriptionArray::operator= (const CBaseReportDescriptionArray& ar) 
{ 
	Assign(ar); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CBaseReportDescriptionArray::operator== (const CBaseReportDescriptionArray& ar) 
{ 
	return IsEqual (ar);
}

//--------------------------------------------------------------------
BOOL CBaseReportDescriptionArray::operator!= (const CBaseReportDescriptionArray& ar) 
{ 
	return !IsEqual (ar);
}

//----------------------------------------------------------------------------------------------
//							CReportObjectsDescription
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CReportObjectsDescription, CObject)

//----------------------------------------------------------------------------------------------
CReportObjectsDescription::CReportObjectsDescription ()
{
}

//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CReportObjectsDescription::GetReportInfo (const CTBNamespace& aNamespace) const
{
	return (CDocumentReportDescription*) m_arReports.GetInfo(aNamespace);
}

//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CReportObjectsDescription::GetReportInfo (const int& nIndex) const
{
	return (CDocumentReportDescription*) m_arReports.GetAt(nIndex);
}

//----------------------------------------------------------------------------------------------
void CReportObjectsDescription::Clear ()
{
	m_arReports.RemoveAll ();
}

//----------------------------------------------------------------------------------------------
CBaseReportDescriptionArray& CReportObjectsDescription::GetReports() 
{
	return m_arReports;
}

//----------------------------------------------------------------------------------------------
CDocumentReportDescription* CReportObjectsDescription::GetDefault() 
{
	return GetReports().GetDefault();
}
//----------------------------------------------------------------------------------------------
void CReportObjectsDescription::AddReport (CDocumentReportDescription* pDescri)
{
	m_arReports.Add (pDescri);
}

//----------------------------------------------------------------------------------------------
void CReportObjectsDescription::RemoveReportAt (const int& nIndex) 
{
	m_arReports.RemoveAt(nIndex);
}

//----------------------------------------------------------------------------------------------
BOOL CReportObjectsDescription::RemoveReport (CDocumentReportDescription* pDescri) 
{
	for (int i = 0; i <= m_arReports.GetUpperBound(); i++)
	{
		CDocumentReportDescription* pRep = (CDocumentReportDescription*) m_arReports.GetAt(i);
		if (pRep->GetNamespace() == pDescri->GetNamespace())
		{
			m_arReports.RemoveAt(i);
			return TRUE; 
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------------------------
void CReportObjectsDescription::RemoveAll()
{
	m_arReports.RemoveAll();
}

