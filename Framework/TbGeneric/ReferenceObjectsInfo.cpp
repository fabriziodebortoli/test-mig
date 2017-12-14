
#include "stdafx.h"

#include <io.h>

#include "ReferenceObjectsInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//					class QueryObjectBase implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(QueryObjectBase, CObject)

//----------------------------------------------------------------------------------------------
//					class CComboColumnDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CComboColumnDescription, CObject)

//----------------------------------------------------------------------------------------------
CComboColumnDescription::CComboColumnDescription ()
	:
	m_nLength (-1)
{
}

//----------------------------------------------------------------------------------------------
CComboColumnDescription::CComboColumnDescription (const CComboColumnDescription& cd)
	:
	m_nLength (-1)
{
	Assign(cd);
}

//----------------------------------------------------------------------------------------------
const int& CComboColumnDescription::GetLength ()
{
	return m_nLength;
}

//----------------------------------------------------------------------------------------------
const CString& CComboColumnDescription::GetSource ()
{
	return m_sSource;
}

//----------------------------------------------------------------------------------------------
const CString& CComboColumnDescription::GetLabel ()
{
	return m_sLabel;
}

//----------------------------------------------------------------------------------------------
const CString& CComboColumnDescription::GetNotLocalizedLabel ()
{
	return m_sNotLocalizedLabel;
}

//----------------------------------------------------------------------------------------------
const CString& CComboColumnDescription::GetWhen	()
{
	return m_sWhen;
}

//----------------------------------------------------------------------------------------------
const CString& CComboColumnDescription::GetFormatter ()
{
	return m_sFormatter;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetLength	(const int& nLength)
{
	m_nLength = nLength;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetSource	(const CString& sSource)
{
	m_sSource = sSource;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetLabel(const CString& sLabel)
{
	m_sLabel = sLabel;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetNotLocalizedLabel (const CString& sLabel)
{
	m_sNotLocalizedLabel = sLabel;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetWhen (const CString& sWhen)
{
	m_sWhen = sWhen;
}

//----------------------------------------------------------------------------------------------
void CComboColumnDescription::SetFormatter (const CString& sFormatter)
{
	m_sFormatter = sFormatter;
}

//----------------------------------------------------------------------------------------------
BOOL CComboColumnDescription::IsValid () const
{
	return !m_sLabel.IsEmpty() || m_sSource.IsEmpty();
}

//--------------------------------------------------------------------
CComboColumnDescription& CComboColumnDescription::operator= (const CComboColumnDescription& cd) 
{ 
	Assign(cd); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CComboColumnDescription::operator== (const CComboColumnDescription& cd) 
{ 
	return IsEqual (cd);
}

//--------------------------------------------------------------------
BOOL CComboColumnDescription::operator!= (const CComboColumnDescription& cd) 
{ 
	return !IsEqual (cd);
}

//--------------------------------------------------------------------
BOOL CComboColumnDescription::IsEqual (const CComboColumnDescription& cd)
{
	return 	m_nLength			== cd.m_nLength &&
			m_sSource			== cd.m_sSource &&
			m_sLabel			== cd.m_sLabel &&
			m_sNotLocalizedLabel== cd.m_sNotLocalizedLabel &&
			m_sWhen				== cd.m_sWhen &&
			m_sFormatter		== cd.m_sFormatter;
}

//----------------------------------------------------------------------------
void CComboColumnDescription::Assign (const CComboColumnDescription& cd)
{
	m_nLength			= cd.m_nLength;
	m_sSource			= cd.m_sSource;
	m_sLabel			= cd.m_sLabel;
	m_sNotLocalizedLabel= cd.m_sNotLocalizedLabel;
	m_sWhen				= cd.m_sWhen;
	m_sFormatter		= cd.m_sFormatter;
}

//----------------------------------------------------------------------------------------------
//					class CHotlinkDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotlinkDescription, CFunctionDescription)

//----------------------------------------------------------------------------------------------
CString	CHotlinkDescription::s_SelectionType_Name		= _T("selection_type");
CString	CHotlinkDescription::s_FilterValue_Name			= _T("filter_value");
CString	CHotlinkDescription::s_SelectionType_Direct		= _T("Direct");
CString	CHotlinkDescription::s_SelectionType_Combo		= _T("Combo");
CString	CHotlinkDescription::s_SelectionType_Code		= _T("Code");
CString	CHotlinkDescription::s_SelectionType_Description = _T("Description");

CString	CHotlinkDescription::s_SelectionType_Custom	= _T("Custom");	

CString	CHotlinkDescription::s_ModeType_Query		= _T("Query");	
CString	CHotlinkDescription::s_ModeType_Report		= _T("Report");	
CString	CHotlinkDescription::s_ModeType_Script		= _T("Script");	

//----------------------------------------------------------------------------------------------
CHotlinkDescription::CHotlinkDescription ()
	:
	CFunctionDescription(CTBNamespace::HOTLINK),
	m_bHasComboBox		(FALSE),
	m_bAddOnFlyEnabled	(TRUE),
	m_bMustExistData	(FALSE),
	m_bSearchOnLinkEnabled (TRUE),
	m_bLoadFullRecord		(TRUE)
{
	m_arComboBox.RemoveAll();
}

//----------------------------------------------------------------------------------------------
CHotlinkDescription::CHotlinkDescription (const CHotlinkDescription& hd)
{
	Assign(hd);
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::Clear ()
{
	m_Namespace.Clear();
	m_sName.Empty();

	m_sDbField.Empty();
	m_sDbTable.Empty();
	m_sDbFieldDescription.Empty();
	m_sCallLink.Empty();
	m_sAskDialogs.Empty();
	m_bLoadFullRecord = TRUE;

	m_bHasComboBox	= FALSE;
	m_arComboBox.RemoveAll();

	m_arSelectionTypes.RemoveAll();
	m_arSelectionModes.RemoveAll();
}

//----------------------------------------------------------------------------------------------
const CString& CHotlinkDescription::GetDbField () const
{
	return m_sDbField;
}

//----------------------------------------------------------------------------------------------
const CString CHotlinkDescription::GetTitle	() const
{
	return AfxLoadXMLString 
						(
							m_sNotLocalizedTitle, 
							m_Namespace.GetObjectName(), 
							AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE)
						);
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::SetDbField (const CString& sField)
{
	m_sDbField = sField;
}

//----------------------------------------------------------------------------------------------
const CString& CHotlinkDescription::GetDbFieldDescription () const
{
	return m_sDbFieldDescription;
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::SetDbFieldDescription (const CString& sFieldDescription)
{
	m_sDbFieldDescription = sFieldDescription;
}

//----------------------------------------------------------------------------------------------
const CString& CHotlinkDescription::GetDbTable () const
{
	return m_sDbTable;
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::SetDbTable (const CString& sTable)
{
	m_sDbTable = sTable;
}

//----------------------------------------------------------------------------------------------
CHotlinkDescription::CSelectionMode* CHotlinkDescription::GetSelectionMode (CHotlinkDescription::ESelectionType eType, int nSelectionCode)
{
	CString sName;
	int i = 0;
	int nCustomIndex = 1;
	for (; i < m_arSelectionTypes.GetSize(); i++)
	{
		CSelectionType* pst = (CSelectionType*)(m_arSelectionTypes.GetAt(i));
		if (eType == pst->m_eType)
		{		
			
			if (pst->m_eType != ESelectionType::CUSTOM || (pst->m_bVisible && (nSelectionCode < 0 || nCustomIndex == nSelectionCode)))
			{
				sName = pst->m_sName;
				break;
			}
			if (pst->m_eType == ESelectionType::CUSTOM && pst->m_bVisible)
				nCustomIndex++;
		}
	}
	if (sName.IsEmpty())
	{
		sName = _T("Default");
		//ASSERT_TRACE(!sName.IsEmpty(),"No selections found of the requested type");
		//return NULL;
	}
	for (i = 0; i < m_arSelectionModes.GetSize(); i++)
	{
		CSelectionMode* psm = (CSelectionMode*)(m_arSelectionModes.GetAt(i));
		if (sName.CompareNoCase(psm->m_sName) == 0)
			return psm;
	}

	ASSERT_TRACE2(FALSE,"CHotlinkDescription - Selection Mode not found: %s (%d)", sName, eType);
	return NULL;
}

//----------------------------------------------------------------------------------------------
const CString& CHotlinkDescription::GetCallLink () const
{
	return m_sCallLink;
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::SetCallLink (const CString& sCallLink)
{
	m_sCallLink = sCallLink;
}

//----------------------------------------------------------------------------------------------
Array& CHotlinkDescription::GetComboBox ()
{
	return m_arComboBox;
}

//----------------------------------------------------------------------------------------------
BOOL CHotlinkDescription::HasComboBox () const
{
	return m_bHasComboBox;
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::SetHasComboBox (BOOL bValue)
{
	m_bHasComboBox = bValue;
}

//----------------------------------------------------------------------------------------------
void CHotlinkDescription::AddComboColumn (CComboColumnDescription* pDescri)
{
	m_arComboBox.Add (pDescri);
}

//--------------------------------------------------------------------
CHotlinkDescription& CHotlinkDescription::operator= (const CHotlinkDescription& hd) 
{ 
	Assign(hd); 
	return *this; 
}

//--------------------------------------------------------------------
BOOL CHotlinkDescription::operator== (const CHotlinkDescription& hd) 
{ 
	return IsEqual (hd);
}

//--------------------------------------------------------------------
BOOL CHotlinkDescription::operator!= (const CHotlinkDescription& hd) 
{ 
	return !IsEqual (hd);
}

//--------------------------------------------------------------------
BOOL CHotlinkDescription::IsEqual (const CHotlinkDescription& hd)
{
	if (!CFunctionDescription::IsEqual(hd))
		return FALSE;

	if	(
			m_sDbField.				CompareNoCase(hd.m_sDbField) || 
			m_bHasComboBox			!= hd.m_bHasComboBox || 
			m_arComboBox.GetSize()	!= hd.m_arComboBox.GetSize() ||
			m_sDbTable.				CompareNoCase(hd.m_sDbTable) ||
			m_sDbFieldDescription.	CompareNoCase(hd.m_sDbFieldDescription) ||
			m_sCallLink.			CompareNoCase(hd.m_sCallLink) || 
			m_bAddOnFlyEnabled		!= hd.m_bAddOnFlyEnabled || 
			m_bMustExistData		!= hd.m_bMustExistData || 
			m_bSearchOnLinkEnabled	!= hd.m_bSearchOnLinkEnabled ||

			m_sAskDialogs.			CompareNoCase (hd.m_sAskDialogs) ||
			m_sDatafile.			CompareNoCase (hd.m_sDatafile) ||

			m_bLoadFullRecord		!= hd.m_bLoadFullRecord || 

			m_arSelectionTypes.			GetSize() != hd.m_arSelectionTypes.GetSize() ||
			m_arSelectionModes.			GetSize() != hd.m_arSelectionModes.GetSize() ||
			m_EventsInfo.GetFunctions().GetSize() != hd.m_EventsInfo.GetFunctions().GetSize()
		)
		return FALSE;

	//TODO compare array m_arSelectionTypes and m_arSelectionModes and m_EventsInfo

	for (int i=0; i <= hd.m_arComboBox.GetUpperBound(); i++)
		if (*((CComboColumnDescription*) m_arComboBox.GetAt(i)) != *((CComboColumnDescription*) hd.m_arComboBox.GetAt(i)))
			return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void CHotlinkDescription::Assign (const CHotlinkDescription& hd)
{
	// base description
	CFunctionDescription::Assign(hd);

	m_sDbField				= hd.m_sDbField;
	m_sDbTable				= hd.m_sDbTable;
	m_sDbFieldDescription	= hd.m_sDbFieldDescription;

	m_bHasComboBox			= hd.m_bHasComboBox;

	m_sCallLink				= hd.m_sCallLink;
	m_bAddOnFlyEnabled		= hd.m_bAddOnFlyEnabled;
	m_bMustExistData		= hd.m_bMustExistData;
	m_bSearchOnLinkEnabled	= hd.m_bSearchOnLinkEnabled;

	m_sAskDialogs			= hd.m_sAskDialogs;
	m_sDatafile				= hd.m_sDatafile;

	m_bLoadFullRecord		= hd.m_bLoadFullRecord;

	m_arComboBox.RemoveAll();
	m_arSelectionTypes.RemoveAll();
	m_arSelectionModes.RemoveAll();
	m_EventsInfo.m_arFunctions.RemoveAll();

	for (int i=0; i <= hd.m_arComboBox.GetUpperBound(); i++)
	{
		CComboColumnDescription* pDescri = new CComboColumnDescription();
		*pDescri = *((CComboColumnDescription*) hd.m_arComboBox.GetAt(i));
		m_arComboBox.Add(pDescri);
	}

	for (int i=0; i <= hd.m_arSelectionTypes.GetUpperBound(); i++)
	{
		CSelectionType* pst = new CSelectionType(*(CSelectionType*)(hd.m_arSelectionTypes.GetAt(i)));
		m_arSelectionTypes.Add(pst);
	}

	for (int i=0; i <= hd.m_arSelectionModes.GetUpperBound(); i++)
	{
		CSelectionMode* psm = new CSelectionMode(*(CSelectionMode*)(hd.m_arSelectionModes.GetAt(i)));
		m_arSelectionModes.Add(psm);
	}

	for (int i=0; i <= hd.m_EventsInfo.GetFunctions() .GetUpperBound(); i++)
	{
		CFunctionDescription* psm = new CFunctionDescription(*(CFunctionDescription*)(hd.m_EventsInfo.GetFunctions().GetAt(i)));
		m_EventsInfo.AddFunction(psm);
	}

	m_EventsInfo.SetLoaded(hd.m_EventsInfo.GetFunctions().GetSize() > 0);
}

//----------------------------------------------------------------------------------------------
//					class CReferenceObjectsDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CReferenceObjectsDescription, CObject)

//----------------------------------------------------------------------------------------------
CReferenceObjectsDescription::CReferenceObjectsDescription ()
	:
	m_bLoaded	 (false)
{
}

//----------------------------------------------------------------------------------------------
CHotlinkDescription* CReferenceObjectsDescription::GetHotlinkInfo (const CTBNamespace& aNs) const
{
	return (CHotlinkDescription*) m_arHotLinks.GetInfo(aNs);
}

//----------------------------------------------------------------------------------------------
const CBaseDescriptionArray& CReferenceObjectsDescription::GetHotLinks () const
{
	return m_arHotLinks;
}

//----------------------------------------------------------------------------------------------
void CReferenceObjectsDescription::AddHotlink	(CHotlinkDescription* pDescri)
{
	m_arHotLinks.Add (pDescri);
}

//----------------------------------------------------------------------------------------------
void CReferenceObjectsDescription::RemoveHotlink(CString nameSpace)
{
	CBaseDescriptionArray*	pHotLinks = (CBaseDescriptionArray*)&GetHotLinks();
	for (int i = pHotLinks->GetUpperBound(); i >=0 ; i--)
	{
		CBaseDescription* var  = (CBaseDescription*)m_arHotLinks[i];
		CHotlinkDescription* c = dynamic_cast<CHotlinkDescription*>(var);
		if (c && c->GetNamespace().ToString().CompareNoCase(nameSpace) == 0)
		{
			m_arHotLinks.RemoveAt(i);
			break;
		}
		
	}
}

