#include "stdafx.h"

#include <TbNameSolver\ApplicationContext.h>
#include <TBNameSolver\CompanyContext.h>

#include "GeneralFunctions.h"
#include "TBStrings.h"

#include "EnumsTable.h"
#include "DataObj.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------
int CompareEnumsByTitle(CObject* pObj1, CObject* pObj2)
{
	EnumTag* pd1 = (EnumTag*)pObj1;
	EnumTag* pd2 = (EnumTag*)pObj2;

	return pd1->GetTagTitle().CompareNoCase(pd2->GetTagTitle());
}

//-----------------------------------------------------------------------------
const EnumsTable* AFXAPI AfxGetStandardEnumsTable()
{
	return AfxGetApplicationContext()->GetObject<const EnumsTable>(&CApplicationContext::GetEnumsTable);
}

//-----------------------------------------------------------------------------
EnumsTableConstPtr AFXAPI AfxGetEnumsTable()
{ 
	/*CCompanyContext* pContext = AfxGetCompanyContext();
	
	EnumsTable* pTable = NULL;
	if (pContext)
	{
		pTable = pContext->GetObject<EnumsTable>(&CCompanyContext::GetEnumsTable);
		if (!pTable)
			// it creates the pointer if not exists
			pTable = AfxGetWritableEnumsTable().GetPointer();
	}
	else*/
	EnumsTable* pTable = (EnumsTable*) AfxGetApplicationContext()->GetEnumsTable();

	return EnumsTableConstPtr(pTable, FALSE);
	
}          

//------------------------------------------------------------------------------
EnumsTablePtr AFXAPI AfxGetWritableEnumsTable () 
{
	/*CCompanyContext* pContext = AfxGetCompanyContext();
	if (pContext && !pContext->GetEnumsTable())
		pContext->AttachEnumsTable(new EnumsTable(*AfxGetStandardEnumsTable()));*/

	return EnumsTablePtr((EnumsTable*) AfxGetApplicationContext()->GetEnumsTable(), TRUE);
}

//============================================================================
//				class EnumItem implementation
//============================================================================
//
//-----------------------------------------------------------------------------
EnumItem::EnumItem()
	:
	m_strItemName	(),
	m_wItemValue	(0),
	m_pEnumTag		(NULL),
	m_bHidden		(FALSE)
{
}

//-----------------------------------------------------------------------------
EnumItem::EnumItem(const CString& strItemName, WORD wItemValue, EnumTag *pEnumTag, const CTBNamespace& OwnerModule)
	:
	m_strItemName	(strItemName),
	m_wItemValue	(wItemValue),
	m_pEnumTag		(pEnumTag),
	m_OwnerModule	(OwnerModule),
	m_bHidden		(FALSE)
{
}

//-----------------------------------------------------------------------------
const EnumItem& EnumItem::operator = (const EnumItem& aEnumItem)
{
	m_strItemName	= aEnumItem.m_strItemName;
	m_wItemValue	= aEnumItem.m_wItemValue;
	m_pEnumTag		= aEnumItem.m_pEnumTag;
	m_OwnerModule	= aEnumItem.m_OwnerModule;
	m_bHidden		= aEnumItem.m_bHidden;
	
	return *this;
}

//-----------------------------------------------------------------------------
BOOL EnumItem::operator == (const EnumItem& aEnumItem) const
{
	return
		m_strItemName	== aEnumItem.m_strItemName	&&
		m_wItemValue	== aEnumItem.m_wItemValue	&&
		m_OwnerModule	== aEnumItem.m_OwnerModule	&&
		m_bHidden		== aEnumItem.m_bHidden;
}

//-----------------------------------------------------------------------------
void EnumItem::SetItem(const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule)
{
	m_strItemName	= strItemName;
	m_wItemValue	= wItemValue;
	m_OwnerModule	= OwnerModule;
}

//-----------------------------------------------------------------------------
const BOOL&	 EnumItem::IsHidden	()
{
	return m_bHidden;
}

//-----------------------------------------------------------------------------
CString	EnumItem::GetTitle()	const
{
	return AfxLoadEnumString(m_strItemName, m_pEnumTag->GetTagName(), m_OwnerModule);
}

//-----------------------------------------------------------------------------
void EnumItem::SetHidden (const BOOL& bHidden)
{
	m_bHidden = bHidden;
}

//============================================================================
//				class EnumItemArray implementation
//============================================================================
//
//-----------------------------------------------------------------------------
EnumItem* EnumItemArray::GetItemByName (const CString& strItemName) const
{
	LPCTSTR pItemName = (LPCTSTR) strItemName;
	EnumItem* pItem;
	
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pItem = GetAt(i);
		if (_tcsicmp((LPCTSTR) pItem->GetItemName(), pItemName)  == 0)
			return pItem;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
EnumItem* EnumItemArray::GetItemByValue (WORD wItemValue) const
{
	EnumItem* pItem;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pItem = GetAt(i);
		if (pItem->GetItemValue() == wItemValue)
			return pItem;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
const EnumItemArray& EnumItemArray::operator = (const EnumItemArray& aEnumItemArray)
{
	RemoveAll();
	
	for (int i = 0; i <= aEnumItemArray.GetUpperBound(); i++)
	{
		EnumItem* pFromItem = aEnumItemArray[i];
		EnumItem* pToItem = new EnumItem;
		*pToItem = *pFromItem;
		Add(pToItem);
	}

	return *this;		
}

//-----------------------------------------------------------------------------
BOOL EnumItemArray::operator == (const EnumItemArray& aEnumItemArray) const
{
	if (GetSize() != aEnumItemArray.GetSize())
		return FALSE;
		
	EnumItem* pItem;
	for (int i = 0; i <= aEnumItemArray.GetUpperBound(); i++)
	{
		pItem = GetAt(i);
		if (*aEnumItemArray[i] != *pItem)
			return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
WORD EnumItemArray::GetEnumItemValue (const CString& strItemName) const
{
	EnumItem* pItem = GetItemByName(strItemName);
	return pItem ? pItem->GetItemValue() : ITEM_ERROR;
}

//-----------------------------------------------------------------------------
CString EnumItemArray::GetEnumItemName (WORD wItemValue) const
{
	EnumItem* pItem = GetItemByValue(wItemValue);
	if (pItem)
		return pItem->GetItemName();

	if (wItemValue == ITEM_ERROR) return _T("");
	
	return cwsprintf(_T("%d - UNKNOWN ITEM"), wItemValue);
}


//-----------------------------------------------------------------------------
CString EnumItemArray::GetTitle (WORD wItemValue) const
{
	EnumItem* pItem = GetItemByValue(wItemValue);
	if (pItem)
		return pItem->GetTitle();

	if (wItemValue == ITEM_ERROR) return _T("");
	
	return cwsprintf(_T("%d - UNKNOWN ITEM"), wItemValue);
}

//-----------------------------------------------------------------------------
BOOL EnumItemArray::ExistItemInModule (const CTBNamespace& nsModule) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumItem* pItem = GetAt(i);
		if (pItem->GetOwnerModule() == nsModule)
			return TRUE;
	}
	return FALSE;
}

//============================================================================
//				class EnumTag implementation
//============================================================================
//
//-----------------------------------------------------------------------------
EnumTag::EnumTag()
	:
	m_strTagName		(),
	m_wTagValue			(0),
	m_wDefaultItemValue	(0),
	m_pEnumItems		(new EnumItemArray),
	m_bHidden			(FALSE)
{
}

//-----------------------------------------------------------------------------
EnumTag::EnumTag
	(
		const CString&		strTagName,
		WORD				wTagValue,
		const CString&		sTitle, 
		const CTBNamespace& OwnerModule,
		WORD				wDefaultItemValue
	)
	:
	m_strTagName		(strTagName),
	m_wTagValue			(wTagValue),
	m_wDefaultItemValue	(wDefaultItemValue),
	m_pEnumItems		(new EnumItemArray),
	m_bHidden			(FALSE)
{
}

//-----------------------------------------------------------------------------
EnumTag::~EnumTag()
{
	ASSERT_TRACE(m_pEnumItems,"Datamember m_pEnumItems must be not null in this context");
	
	delete m_pEnumItems;
	m_pEnumItems = NULL;
}

//-----------------------------------------------------------------------------
const EnumTag& EnumTag::operator = (const EnumTag& aEnumTag)
{
	m_strTagName 		= aEnumTag.m_strTagName;
	m_wTagValue			= aEnumTag.m_wTagValue;
	m_wDefaultItemValue = aEnumTag.m_wDefaultItemValue;
	*m_pEnumItems		= *(aEnumTag.m_pEnumItems);
	m_bHidden			= aEnumTag.m_bHidden;

	return *this;		
}

//-----------------------------------------------------------------------------
BOOL EnumTag::operator == (const EnumTag& aEnumTag) const
{
	return
		m_strTagName 		== aEnumTag.m_strTagName		&&
		m_wTagValue			== aEnumTag.m_wTagValue			&&
		m_wDefaultItemValue == aEnumTag.m_wDefaultItemValue	&&
		*m_pEnumItems		== *(aEnumTag.m_pEnumItems)		&&
		m_bHidden			== aEnumTag.m_bHidden;
}

//-----------------------------------------------------------------------------
EnumItem* EnumTag::AddItem(const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule)
{
	if (ExistItem(strItemName, wItemValue))
		return NULL;

	EnumItem* pItem = new EnumItem (strItemName, wItemValue, this, OwnerModule);
	m_pEnumItems->Add(pItem);
	return pItem;
}

//-----------------------------------------------------------------------------
void EnumTag::DeleteItem(const CString& strItemName)
{
	LPCTSTR pItemName = (LPCTSTR) strItemName;

	for (int i = 0; i <= m_pEnumItems->GetUpperBound(); i++)
		if (_tcsicmp((LPCTSTR) m_pEnumItems->GetAt(i)->GetItemName(), pItemName)  == 0)
		{
			m_pEnumItems->RemoveAt(i);
			return;
		}
}

//-----------------------------------------------------------------------------
BOOL EnumTag::ExistItemName(const CString& strItemName)
{                       
	LPCTSTR pItemName = (LPCTSTR) strItemName;

	for (int i = 0; i <= m_pEnumItems->GetUpperBound(); i++)
		if (_tcsicmp((LPCTSTR) m_pEnumItems->GetAt(i)->GetItemName(), pItemName) == 0)
			return TRUE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL EnumTag::ExistItemValue(WORD wItemValue)
{                       
	for (int i = 0; i <= m_pEnumItems->GetUpperBound(); i++)
		if (m_pEnumItems->GetAt(i)->GetItemValue() == wItemValue)
			return TRUE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL EnumTag::ExistItem(const CString& strItemName, WORD wItemValue)
{    
	EnumItem* pItem;
	LPCTSTR pItemName = (LPCTSTR) strItemName;
	for (int i = 0; i <= m_pEnumItems->GetUpperBound(); i++)
	{
		pItem = m_pEnumItems->GetAt(i);
		if	(
				(_tcsicmp((LPCTSTR) pItem->GetItemName(), pItemName) == 0) ||
				(pItem->GetItemValue() == wItemValue)
			)
			return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void EnumTag::SetTag(const CString& strTagName, WORD wTagValue)
{
	m_strTagName	= strTagName;
	m_wTagValue		= wTagValue;
}

//-----------------------------------------------------------------------------
CString EnumTag::GetTagTitle ()	const	
{
	// il dizionario primario per il tag, viene
	// considerato quello dell'item 0 o 1

	EnumItem* pFirstItem = NULL;
	if (m_pEnumItems->GetSize())
	{
		pFirstItem = m_pEnumItems->GetItemByValue(0);
		if (!pFirstItem)
			pFirstItem = m_pEnumItems->GetItemByValue(1);
	}

	if (!pFirstItem)
		return m_strTagName;

	return AfxLoadEnumString(m_strTagName, m_strTagName, pFirstItem->GetOwnerModule()); 
}

//-----------------------------------------------------------------------------
WORD EnumTag::GetLongerItemValue() const
{
	int idx = GetLongerItemIdx();
	if (idx < 0)
		return 0xFFFF;
	return m_pEnumItems->GetAt(idx)->GetItemValue();
}

//-----------------------------------------------------------------------------
int EnumTag::GetLongerItemIdx() const
{
	int idx = -1, maxLength = 0, currLength = 0;
	for (int i = 0; i < m_pEnumItems->GetCount(); i++)
	{
		currLength = m_pEnumItems->GetAt(i)->GetTitle().GetLength();
		if (currLength > maxLength && !m_pEnumItems->GetAt(i)->IsHidden())
		{
			maxLength = currLength;
			idx = i;
		}
	}
	return idx;
}

//-----------------------------------------------------------------------------
const BOOL&	 EnumTag::IsHidden	()
{
	return m_bHidden;
}

//-----------------------------------------------------------------------------
void EnumTag::SetHidden (const BOOL& bHidden)
{
	m_bHidden = bHidden;
}

//============================================================================
//				class EnumTagArray implementation
//============================================================================
//
//-----------------------------------------------------------------------------
EnumTagArray::EnumTagArray()
{
}

//-----------------------------------------------------------------------------
const EnumTagArray& EnumTagArray::operator = (const EnumTagArray& aEnumTagArray)
{
	RemoveAll();
	
	for (int i = 0; i <= aEnumTagArray.GetUpperBound(); i++)
	{
		EnumTag* pFromTag = aEnumTagArray[i];
		EnumTag* pToTag = new EnumTag();
		*pToTag = *pFromTag;
		Add(pToTag);
	}

	return *this;		
}

//-----------------------------------------------------------------------------
BOOL EnumTagArray::operator == (const EnumTagArray& aEnumTagArray) const
{
	if (GetSize() != aEnumTagArray.GetSize())
		return FALSE;
		
	for (int i = 0; i <= aEnumTagArray.GetUpperBound(); i++)
		if (*aEnumTagArray[i] != *GetAt(i))
			return FALSE;
	
	return	TRUE;
}

//-----------------------------------------------------------------------------
EnumItemArray* EnumTagArray::GetEnumItems (const CString& strTagName) const
{
	EnumTag* pTag;
	LPCTSTR pTagName = (LPCTSTR) strTagName;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pTag = GetAt(i);
		if (_tcsicmp((LPCTSTR) pTag->GetTagName(), pTagName)  == 0)
			return pTag->GetEnumItems();
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
EnumItemArray* EnumTagArray::GetEnumItems (WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag->GetEnumItems();
	}

	return NULL;
}

//-----------------------------------------------------------------------------
EnumTag* EnumTagArray::GetEnumTag(WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL EnumTagArray::ExistEnumValue (WORD wTagValue, WORD wItemValue) const
{
	const EnumItemArray* enumItems = GetEnumItems (wTagValue);
	if (enumItems == NULL) 
		return FALSE;
	const EnumItem* pItem = enumItems->GetItemByValue(wItemValue);
	if (pItem == NULL) 
		return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
CString EnumTagArray::GetEnumItemTitle (WORD wTagValue, WORD wItemValue) const
{
	const EnumItemArray* enumItems = GetEnumItems (wTagValue);
	if (enumItems == NULL) 
		return L"";
	const EnumItem* pItem = enumItems->GetItemByValue(wItemValue);
	if (pItem == NULL) 
		return L"";
	return pItem->GetTitle();
}

//-----------------------------------------------------------------------------
WORD EnumTagArray::GetEnumLongerItemValue (WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag->GetLongerItemValue();
	}

	TRACE("Tag value not found: %d", wTagValue);
	return 0xFFFF;
}
	
//-----------------------------------------------------------------------------
WORD EnumTagArray::GetEnumTagValue (const CString&	strTagName) const
{
	LPCTSTR pTagName = (LPCTSTR) strTagName;
	EnumTag* pTag;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pTag = GetAt(i);
		
		if (_tcsicmp((LPCTSTR) pTag->GetTagName(), pTagName)  == 0)
			return pTag->GetTagValue();
	}

	return TAG_ERROR;
}

//-----------------------------------------------------------------------------
CString EnumTagArray::GetEnumTagName (WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag->GetTagName();
	}

	return cwsprintf( _T("%d - UNKNOWN TAG"), wTagValue);
}

//-----------------------------------------------------------------------------
CString EnumTagArray::GetEnumTagTitle (WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag->GetTagTitle();
	}

	return cwsprintf(_T("%d - UNKNOWN TAG"), wTagValue);
}

//-----------------------------------------------------------------------------
WORD EnumTagArray::GetEnumDefaultItemValue (WORD wTagValue) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag->GetDefaultItemValue();
	}

	return 0;
}

//-----------------------------------------------------------------------------
WORD EnumTagArray::GetEnumItemValue (const CString& strTagName, const CString& strItemName) const
{
	EnumItemArray* pEnumTags = GetEnumItems(strTagName);
	
	return (pEnumTags == NULL ? TAG_ERROR : pEnumTags->GetEnumItemValue(strItemName));
}

//-----------------------------------------------------------------------------
EnumTag* EnumTagArray::AddTag 
	(
		const	CString&			strTagName, 
				WORD				wTagValue, 
				const CString&		sTitle, 
				const CTBNamespace& OwnerModule,
				WORD				wDefaultItemValue
	)
{                       
	if (ExistTag(strTagName, wTagValue))
		return NULL;
		
	EnumTag* pEnumTag = new EnumTag (strTagName, wTagValue, sTitle, OwnerModule, wDefaultItemValue);
	Add(pEnumTag);
	
	return pEnumTag;
}

//-----------------------------------------------------------------------------
void EnumTagArray::DeleteTag(const CString& strTagName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (_tcsicmp((LPCTSTR) GetAt(i)->GetTagName(), (LPCTSTR) strTagName)  == 0)
		{
			RemoveAt(i);
			return;
		}
}

//-----------------------------------------------------------------------------
EnumTag* EnumTagArray::GetTagByName(const CString& strTagName) const
{
	EnumTag* pTag;
	LPCTSTR pTagName = (LPCTSTR) strTagName;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pTag = GetAt(i);
		if (_tcsicmp((LPCTSTR) pTag->GetTagName(), pTagName)  == 0)
			return pTag;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
EnumTag* EnumTagArray::GetTagByValue(WORD wTagValue) const
{
	EnumTag* pTag;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pTag = GetAt(i);
		if (pTag->GetTagValue() == wTagValue)
			return pTag;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL EnumTagArray::ExistTag(const CString& strTagName, WORD wTagValue)
{       
	EnumTag* pTag;
	LPCTSTR pTagName = (LPCTSTR) strTagName;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pTag = GetAt(i);
		if	(
				(_tcsicmp((LPCTSTR) pTag->GetTagName(), pTagName)  == 0) ||
				(pTag->GetTagValue() == wTagValue)
			)
			return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL EnumTagArray::ExistTagInModule (const CTBNamespace& nsModule) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		EnumTag* pTag = GetAt(i);
		if	(pTag->GetEnumItems()->ExistItemInModule(nsModule))
			return TRUE;
	}
	return FALSE;
}

//============================================================================
//				class EnumsTable implementation
//============================================================================
//
//-----------------------------------------------------------------------------
EnumsTable::EnumsTable()
{
	m_pEnumTags = new EnumTagArray();
};

//-----------------------------------------------------------------------------
EnumsTable::EnumsTable(const EnumsTable& aTable)
{
	m_pEnumTags = new EnumTagArray();
	AssignEnums (aTable);
};

//-----------------------------------------------------------------------------
EnumsTable::~EnumsTable()
{ 
	ASSERT_TRACE(m_pEnumTags,"m_pEnumTags datamember must be not null in this context");

	delete m_pEnumTags;
	m_pEnumTags	= NULL; 
};

//-----------------------------------------------------------------------------
void EnumsTable::Init()
{
	m_pEnumTags->RemoveAll();
}

//-----------------------------------------------------------------------------
void EnumsTable::AssignEnums(const EnumsTable& aEnumsTable)
{
	*m_pEnumTags= *(aEnumsTable.m_pEnumTags);
}

//-----------------------------------------------------------------------------
BOOL EnumsTable::IsEqualEnums(const EnumsTable& aEnumsTable) const
{
	return *m_pEnumTags == *aEnumsTable.m_pEnumTags;
}

// Don't do ASSERT but give diagnostic for WOORM purpose
//-----------------------------------------------------------------------------
EnumTag* EnumsTable::AddEnumTag
	(
		const CString&		strTagName,
		WORD				wTagValue,
		const CString&		sTitle, 
		const CTBNamespace& OwnerModule,
		WORD				wDefaultItemValue /*= 0*/
	)	
{
	return m_pEnumTags->AddTag(strTagName, wTagValue, sTitle, OwnerModule, wDefaultItemValue); 
}

// Don't do ASSERT but give diagnostic for WOORM purpose
//-----------------------------------------------------------------------------
BOOL EnumsTable::AddEnumValue (EnumTag* pTag, const CString& strItemName, WORD wItemValue, const CTBNamespace& OwnerModule)	
{ 
	return pTag->AddItem(strItemName, wItemValue, OwnerModule) != NULL; 
}

//------------------------------------------------------------------------------
const EnumItemArray* EnumsTable::GetEnumItems (const CString& strTagName) const
{
	return m_pEnumTags->GetEnumItems(strTagName);
}

//------------------------------------------------------------------------------
const EnumItemArray* EnumsTable::GetEnumItems (WORD wTagValue) const
{ 
	return m_pEnumTags->GetEnumItems(wTagValue);
}
	
//------------------------------------------------------------------------------
BOOL EnumsTable::ExistEnumTagName (const CString& strTagName) const
{
	return m_pEnumTags->ExistTagName(strTagName);
}
	
//------------------------------------------------------------------------------
BOOL EnumsTable::ExistEnumTagValue (WORD wTagValue) const
{
	return m_pEnumTags->ExistTagValue(wTagValue);
}
	
//------------------------------------------------------------------------------
BOOL EnumsTable::ExistEnumTag (const CString& strTagName, WORD wTagValue) const
{
	return m_pEnumTags->ExistTag(strTagName, wTagValue);
}
	
//------------------------------------------------------------------------------
CString EnumsTable::GetEnumTagName (WORD wTagValue) const
{
	return m_pEnumTags->GetEnumTagName(wTagValue);
}

//------------------------------------------------------------------------------
EnumTag* EnumsTable::GetEnumTag(WORD wTagValue) const
{
	return m_pEnumTags->GetEnumTag(wTagValue);
}

//------------------------------------------------------------------------------
CString EnumsTable::GetEnumTagTitle (WORD wTagValue) const
{
	EnumTag* pTag = m_pEnumTags->GetTagByValue(wTagValue);
	return pTag ? pTag->GetTagTitle() : _T("");
}

//------------------------------------------------------------------------------
WORD EnumsTable::GetEnumTagValue (const CString& strTagName) const
{
	return m_pEnumTags->GetEnumTagValue(strTagName);
}
	
//------------------------------------------------------------------------------
WORD EnumsTable::GetEnumItemValue (const CString&	strTagName, const CString& strItemName) const
{
	return m_pEnumTags->GetEnumItemValue(strTagName, strItemName);
}

//------------------------------------------------------------------------------
WORD EnumsTable::GetEnumDefaultItemValue (WORD wTagValue) const
{
	return m_pEnumTags->GetEnumDefaultItemValue(wTagValue);
}
	
//------------------------------------------------------------------------------
WORD EnumsTable::GetEnumLongerItemValue (WORD wTagValue) const
{
	return m_pEnumTags->GetEnumLongerItemValue(wTagValue);
}

//------------------------------------------------------------------------------
BOOL EnumsTable::ExistEnumValue (DWORD dwValue) const
{
	WORD wTagValue = GET_TAG_VALUE(dwValue);
	WORD wItemValue = GET_ITEM_VALUE(dwValue);
	return m_pEnumTags->ExistEnumValue(wTagValue, wItemValue);
}

//------------------------------------------------------------------------------
CString EnumsTable::GetEnumItemTitle (DWORD dwValue) const
{
	WORD wTagValue = GET_TAG_VALUE(dwValue);
	WORD wItemValue = GET_ITEM_VALUE(dwValue);
	return m_pEnumTags->GetEnumItemTitle(wTagValue, wItemValue);
}

//-----------------------------------------------------------------------------
CString EnumsTable::ToJson() const
{
	CJsonSerializer strJson;
	strJson.OpenObject(_T("enums"));
	strJson.OpenArray(_T("tags"));
	for (int i = 0; i < m_pEnumTags->GetSize(); i++)
	{
		strJson.OpenObject();

		EnumTag* pTag = m_pEnumTags->GetAt(i);
		strJson.WriteString(_T("name"), CString(pTag->GetTagName()));
		strJson.WriteString(_T("value"), cwsprintf(_T("%d"), pTag->GetTagValue()));

		strJson.OpenArray(_T("items"));
		for (int j = 0; j < pTag->GetEnumItems()->GetSize(); j++)
		{
			EnumItem* pItem = pTag->GetEnumItems()->GetAt(j);
			strJson.OpenObject();
			strJson.WriteString(_T("name"), CString(pItem->GetItemName()));
			strJson.WriteString(_T("value"), cwsprintf(_T("%d"), pItem->GetItemValue()));

			int enumStored = DataEnum(pTag->GetTagValue(), pItem->GetItemValue()).GetSoapValue();
			strJson.WriteString(_T("stored"), cwsprintf(_T("%d"), enumStored));
			strJson.CloseObject();
		}
		strJson.CloseArray();

		strJson.CloseObject();
	}
	strJson.CloseArray();
	strJson.CloseObject();
	

	return strJson.GetJson();
}

