#include "stdafx.h"

#include <TbNameSolver\FileSystemFunctions.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbClientCore\ClientObjects.h>
#include <TbXmlCore\XMLTags.h>
#include <TbGeneric\TbStrings.h>
#include <TbGeneric\GeneralFunctions.h>

#include <TbParser\TokensTable.h>
#include <TbParser\XMLBaseDescriptionParser.h>

#include "EnumsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
static const TCHAR szXmlEnums[]	= _T("Enums");

static const TCHAR szXmlTagKey[]		= _T("/Enums/Tag");
static const TCHAR szXmlItemKey[]		= _T("/Enums/Tag/Item");
static const TCHAR szXmlDefaultKey[]	= _T("/Enums/Tag/DefaultValue");

static const TCHAR szXmlName[]				= _T("name");
static const TCHAR szXmlValue[]				= _T("value");
static const TCHAR szXmlDefault[]			= _T("defaultValue");
static const TCHAR szXmlHidden[]			= _T("hidden");

#define DEFAULT_ZERO_VALUE	-1;

////////////////////////////////////////////////////////////////////////////////
//				class CXmlEnumsContent implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC (CXmlEnumsContent, CXMLSaxContent)
//-----------------------------------------------------------------------------
CXmlEnumsContent::CXmlEnumsContent()
{
	m_nDefaultsFound = 0;
}

//-----------------------------------------------------------------------------
void CXmlEnumsContent::SetCurrentModule (const CTBNamespace& aCurrentModule)
{
	m_CurrentModule = aCurrentModule;
}

//-----------------------------------------------------------------------------
void CXmlEnumsContent::AttachTable (EnumsTable* pTable)
{
	m_pEnumsTable = pTable;
}

//-----------------------------------------------------------------------------
CString CXmlEnumsContent::OnGetRootTag () const
{
	return szXmlEnums;
}

//-----------------------------------------------------------------------------
void CXmlEnumsContent::OnBindParseFunctions ()
{
	BIND_PARSE_ATTRIBUTES	(szXmlTagKey,		&CXmlEnumsContent::ParseTag);
	BIND_PARSE_ATTRIBUTES	(szXmlItemKey,		&CXmlEnumsContent::ParseItem);
    BIND_PARSE_ATTRIBUTES	(szXmlDefaultKey,	&CXmlEnumsContent::ParseDefaultValue);
    BIND_PARSE_TAG			(szXmlTagKey,		&CXmlEnumsContent::CheckTagEnd);
	
}

//-----------------------------------------------------------------------------
int CXmlEnumsContent::ParseTag (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	// leggo l'attributo di identificazione del TAG
	m_sTagName = arAttributes.GetAttributeByName (szXmlName);
	if (m_sTagName.IsEmpty())
	{
		AddError (_TB("<Tag> element without 'name' attribute! "));
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	// value e default del tag 
	CString strTemp = arAttributes.GetAttributeByName (szXmlValue);
	if (strTemp.IsEmpty())
	{
		AddError (_TB("<Tag> element without 'value' attribute! "));
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	// verifico che il valore non superi quello massimo
	m_wTagValue = _ttoi((LPCTSTR) strTemp);
	if (m_wTagValue > MAX_ENUM_TAG_VALUE)
	{
		AddError (_TB("Found enum value greater than the maximum allowed. ") + strTemp);
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}
	
	m_bTagHidden = FALSE;

	// country checking
	strTemp = arAttributes.GetAttributeByName (XML_ALLOWISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, TRUE))
		m_bTagHidden = TRUE;

	strTemp = arAttributes.GetAttributeByName (XML_DENYISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, FALSE))
		m_bTagHidden = TRUE;

	// activation checking
	strTemp = arAttributes.GetAttributeByName (XML_ACTIVATION_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation (strTemp))
		m_bTagHidden = TRUE;

	//deprecaded items
	strTemp = arAttributes.GetAttributeByName (szXmlHidden);
	if (!strTemp.IsEmpty())
	{
		DataBool db;
		db.AssignFromXMLString(strTemp);
		if (db)
			m_bTagHidden = TRUE;
	}
	
	//----
	strTemp = arAttributes.GetAttributeByName(szXmlDefault);
	
	for (int i=0; i < MAX_DEFAULT_DIM; i++)
		m_arDefaultItemValues[i] = DEFAULT_ZERO_VALUE;

	m_nDefaultsFound = 1;
    m_arDefaultItemValues[0] = strTemp.IsEmpty() ? 0 : _ttoi((LPCTSTR) strTemp);
	

	// Se il Tag esiste mi metto in automatico in INSERT, 
	// verificherò solo che non vi siano duplicati di ITEMS
	EnumTag* pTag			= m_pEnumsTable->GetEnumTags()->GetTagByName(m_sTagName);
	EnumTag* pTagByValue	= m_pEnumsTable->GetEnumTags()->GetTagByValue(m_wTagValue);
	
	// esiste con un valore numerico differente 
	if ((!pTag && pTagByValue) || (pTag && !pTagByValue))
	{
		AddError (_TB("The enum has already been defined with a different value. ") + m_sTagName);
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	return CXMLSaxContent::OK;
}

//-----------------------------------------------------------------------------
int CXmlEnumsContent::CheckTagEnd (const CString& sUri, const CString& aTagValue)
{
	// ora esiste il tag e ha degli elementi validi all'interno
	EnumTag* pTag = m_pEnumsTable->GetEnumTags()->GetTagByName(m_sTagName);

	WORD wDefaultItemValue = 0;
	if (m_nDefaultsFound > 0)
		for (int i= m_nDefaultsFound; i > 0; i--) 
		{
			EnumItem* pItem = pTag->GetEnumItems()->GetItemByValue (m_arDefaultItemValues[i-1]);

			if (pItem && !pItem->IsHidden())
			{
				wDefaultItemValue = m_arDefaultItemValues[i-1];
				break;
			}
		}

	// ho trovato un default <> dallo zero valido
	if (wDefaultItemValue > 0)
	{
		pTag->SetDefaultItemValue (wDefaultItemValue);
		return CXMLSaxContent::OK;
	}

	// l'item zero è sempre prioritario rispetto al primo dell'array
	EnumItem* pItem = pTag->GetEnumItems()->GetItemByValue (0);

	// default su elemento zero
	if (wDefaultItemValue == 0 && pItem && !pItem->IsHidden ())
	{
		pTag->SetDefaultItemValue (0);
		return CXMLSaxContent::OK;
	}

	// ricerca del primo elemento non zero
	for (int i=0;  i <= pTag->GetEnumItems()->GetUpperBound(); i++)
	{
		pItem = pTag->GetEnumItems()->GetAt(i);
		if (!pItem->IsHidden())
		{
			wDefaultItemValue = pItem->GetItemValue();
			break;
		}
	}

	if (pTag && pTag->GetEnumItems()->GetSize())
		pTag->SetDefaultItemValue
			(
				wDefaultItemValue
				? wDefaultItemValue
				: pTag->GetDefaultItemValue ()
			);

	return CXMLSaxContent::OK;
}

//-----------------------------------------------------------------------------
int  CXmlEnumsContent::ParseItem (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	WORD wItemValue = 0;

	EnumTag* pTag = m_pEnumsTable->GetEnumTags()->GetTagByName(m_sTagName);

	CString	strItemName = arAttributes.GetAttributeByName(szXmlName);

	// nome dell'item
	if (strItemName.IsEmpty ())
	{
		AddWarning(_TB("The enum has an element missing the name. ") + m_sTagName);
		return CXMLSaxContent::OK;
	}

	// valore dell'item
	CString strValue = arAttributes.GetAttributeByName(szXmlValue);
	if (strValue.IsEmpty())
	{
		AddWarning (_TB("The enum has an element missing the 'value' attribute. ") + m_sTagName);
		return CXMLSaxContent::OK;
	}

	if (!strValue.IsEmpty())
		wItemValue = _ttoi((LPCTSTR) strValue);

	BOOL bHidden = FALSE;

	// country checking
	CString strTemp;
	strTemp = arAttributes.GetAttributeByName (XML_ALLOWISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, TRUE))
		bHidden = TRUE;

	strTemp = arAttributes.GetAttributeByName (XML_DENYISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, FALSE))
		bHidden = TRUE;

	// activation checking
	strTemp = arAttributes.GetAttributeByName (XML_ACTIVATION_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation (strTemp))
		bHidden = TRUE;

	//deprecaded items
	strTemp = arAttributes.GetAttributeByName (szXmlHidden);
	if (!strTemp.IsEmpty())
	{
		DataBool db;
		db.AssignFromXMLString(strTemp);
		if (db)
			bHidden = TRUE;
	}
	//----

	if	(pTag && pTag->ExistItemName(strItemName))
	{
		AddWarning (_TB("Value of the enum item already used.") + m_sTagName);
		return CXMLSaxContent::OK;
	}

	// se non ho mai inserito il TAG lo faccio al primo item valido
	if (!pTag)
	{
		pTag = m_pEnumsTable->AddEnumTag(m_sTagName, m_wTagValue, m_sTagName, m_CurrentModule);
		pTag->SetHidden (m_bTagHidden);
	}

	// l'owner module del tag diventa 
	EnumItem* pItem;
	// se è stato creato l'enumerativo prova ad aggiungere l'item
	if (!pTag || (pItem = pTag->AddItem (strItemName, wItemValue, m_CurrentModule)) == NULL)
	{
		AddError (_TB("Value of the item already used. ") + m_sTagName + _T(" ") + strItemName);
		return CXMLSaxContent::ABORT;
	}
	pItem->SetHidden (bHidden || m_bTagHidden);

	return CXMLSaxContent::OK;
}

//-----------------------------------------------------------------------------
BOOL CXmlEnumsContent::ParseDefaultValue (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	// valore dell'item
	CString strValue = arAttributes.GetAttributeByName(szXmlValue);
	if (strValue.IsEmpty())
	{
		AddWarning (_TB("The DefaultValue of the enum has an 'value' attribute missing. ") + m_sTagName);
		return CXMLSaxContent::OK;
	}

	WORD wItemValue = 0;
	if (!strValue.IsEmpty())
		wItemValue = _ttoi((LPCTSTR) strValue);

	// country checking
	CString strTemp;
	strTemp = arAttributes.GetAttributeByName (XML_ALLOWISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, TRUE))
		return CXMLSaxContent::OK;

	strTemp = arAttributes.GetAttributeByName (XML_DENYISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry (strTemp, FALSE))
		return CXMLSaxContent::OK;

	// activation checking
	strTemp = arAttributes.GetAttributeByName (XML_ACTIVATION_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation (strTemp))
		return CXMLSaxContent::OK;

	m_arDefaultItemValues[m_nDefaultsFound] = wItemValue;
	m_nDefaultsFound++;

	return CXMLSaxContent::OK;
}
