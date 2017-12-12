#include "StdAfx.h"
#include <TbOleDb\sqlrec.h>
#include <TbOleDb\sqltable.h>
#include <TbGes\extdoc.h>

#include "EBHotKeyLink.h"
#include "mhotlink.h"

using namespace Microarea::Framework::TBApplicationWrapper;
#include "QueryController.h"

IMPLEMENT_DYNAMIC (EBHotKeyLink, HotKeyLink)

//----------------------------------------------------------------------------
EBHotKeyLink::EBHotKeyLink(System::String^ tableName, MHotLink^ mHotLink)
	:
	HotKeyLink (CString(tableName))
{
	m_pMHotLink = mHotLink;
	m_XmlDescription.SetHasComboBox(TRUE);
}

	
//----------------------------------------------------------------------------
EBHotKeyLink::~EBHotKeyLink()
{
	if (m_pMHotLink)
		m_pMHotLink->FireHotLinkDestroyed();
}

//-------------------------------------------------------------------------------
void EBHotKeyLink::OnCallLink()
{
	m_pMHotLink->OnCallLink();
}

// m_pMHotLink->OnDefineQuery(nQuerySelection) fa la preparazione
// delle query di ricerca automatiche quando l'hotlink e' dinamico
//-------------------------------------------------------------------------------
void EBHotKeyLink::OnDefineQuery (SelectionType nQuerySelection)
{
	m_pMHotLink->OnDefineQuery(nQuerySelection);
}

// m_pMHotLink->OnPrepareQuery(aDataObj, nQuerySelection) fa la preparazione
// delle query di ricerca automatiche quando l'hotlink e' dinamico
//-------------------------------------------------------------------------------
void EBHotKeyLink::OnPrepareQuery (DataObj* aDataObj, SelectionType nQuerySelection)
{
	m_pMHotLink->OnPrepareQuery(aDataObj, nQuerySelection);
}

//-------------------------------------------------------------------------------
::DataObj* EBHotKeyLink::GetDataObj () const
{
	MDataObj^ mDataObj = m_pMHotLink->GetCodeDataObj();
	return mDataObj == nullptr ? NULL : mDataObj->GetDataObj();
}

//-----------------------------------------------------------------------------
BOOL EBHotKeyLink::OnValidateRadarSelection (SqlRecord* pRec)
{ 
	return m_pDocument ? ((CAbstractFormDoc*) m_pDocument)->DispatchOnValidateRadarSelection(pRec, this) : TRUE; 
} 

//-----------------------------------------------------------------------------
void EBHotKeyLink::InitializeXmlDescription ()
{ 
	MHotLinkSearch^ src = m_pMHotLink->Searches->ComboBox;
	if (src == nullptr)
		src = m_pMHotLink->Searches->ByKey;
	if (src != nullptr)
	{
		m_XmlDescription.SetDbField(CString (src->FieldName));
		src = m_pMHotLink->Searches->ByDescription;
		if (src != nullptr)
			m_XmlDescription.SetDbFieldDescription (CString(src->FieldName));
		CheckXmlDescription();
		LoadSymbolTable();
	}
} 

//-----------------------------------------------------------------------------
CString	 EBHotKeyLink::FormatComboItem	(SqlRecord* pRec)
{
	CString str = m_pMHotLink->FormatComboItem(pRec);
	return str.IsEmpty() ? __super::FormatComboItem(pRec) : str;
} 

//-----------------------------------------------------------------------------
bool EBHotKeyLink::SearchOnLinkUpperProxy()
{
	return SearchOnLinkUpper() == TRUE;
}

//-----------------------------------------------------------------------------
bool EBHotKeyLink::SearchOnLinkLowerProxy()
{
	return SearchOnLinkLower() == TRUE;
}

//-----------------------------------------------------------------------------
void EBHotKeyLink::OnRadarRecordAvailable ()
{
	__super::OnRadarRecordAvailable();
	m_pMHotLink->OnRadarRecordAvailable();
}

//-----------------------------------------------------------------------------
BOOL EBHotKeyLink::IsValid ()
{
	return __super::IsValid();
}
		