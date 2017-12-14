#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Diagnostic.h>

#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\ReferenceObjectsInfo.h>
#include <TBGeneric\FormatsTable.h>

#include <TBParser\XmlFunctionObjectsParser.h>

#include <TBGenlib\DataObjDescriptionEx.h>
#include <TBWoormEngine\QueryObject.h>

#include "XMLReferenceObjectsParserEx.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 				class CXMLReferenceObjectsParser Implementation
/////////////////////////////////////////////////////////////////////////////
// Class moved from TbParser to TbGenlib to avoid circular dependency (because this class needs 
// CDataObjDescriptionExpr, which is in TbGenlib)
// factorized in TbGes to allow parse of QueryObjects
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLReferenceObjectsParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLReferenceObjectsParser::CXMLReferenceObjectsParser ()
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLReferenceObjectsParser::Parse (CXMLDocumentObject* pDoc, CReferenceObjectsDescription* pDescri, const CTBNamespace& aParent)
{
	CString sNodeName; 

	// HotKeyLinks
	CXMLNodeChildsList* pChildsNode = pDoc->GetNodeListByTagName(XML_HOTKEYLINK_TAG);
	
	// il file è vuoto
	if (!pChildsNode || !pChildsNode->GetSize())
		return TRUE;

	CXMLNode* pRefNode;
	CHotlinkDescription* pNewDescri;

	for (int i=0; i <= pChildsNode->GetUpperBound(); i++)
	{
		pRefNode = pChildsNode->GetAt(i);

		if (!pRefNode || !pRefNode->GetName(sNodeName) || sNodeName != XML_HOTKEYLINK_TAG)
			continue;

		pNewDescri = new CHotlinkDescription();

		if (ParseHotLink (pRefNode, pNewDescri, aParent))
			pDescri->AddHotlink(pNewDescri);
		else
			delete pNewDescri;
	}

	delete pChildsNode;

	return TRUE;
}

// la funzione non è ancora in grado di validare correttamente le espressioni che ha trovato 
// negli attributi, in quando potranno essere verificate solo in runtime, anche il formattatore.
//----------------------------------------------------------------------------------------------
BOOL CXMLReferenceObjectsParser::ParseComboColumn(
											CXMLNode*					pNode, 
											CComboColumnDescription*	pDescri, 
											const CTBNamespace&			aParent										)
{
	CString sTemp;
	pNode->GetAttribute (XML_LENGTH_ATTRIBUTE, sTemp);
	
	if (!sTemp.IsEmpty())
	{
		int nLength = _ttoi((LPCTSTR) sTemp);
		if (nLength > 0)
			pDescri->SetLength(nLength);
	}

	sTemp.Empty();
	pNode->GetAttribute (XML_LOCALIZE_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
	{
		pDescri->SetNotLocalizedLabel(sTemp);
		if (aParent.IsValid())
		{
			CXMLDocumentObject* pDoc = pNode->GetXMLDocument();

			pDescri->SetLabel(AfxLoadXMLString
			(
				sTemp, 
				GetNameWithExtension(pDoc->GetFileName()), 
				AfxGetDictionaryPathFromNamespace(aParent, TRUE)
			));
		}
		else
			pDescri->SetLabel(sTemp);
	}

	sTemp.Empty();
	pNode->GetAttribute (XML_SOURCE_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
		pDescri->SetSource(sTemp);

	sTemp.Empty();
	pNode->GetAttribute (XML_WHEN_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
		pDescri->SetWhen(sTemp);

	sTemp.Empty();
	pNode->GetAttribute (XML_FORMATTER_ATTRIBUTE, sTemp);

	// adesso non posso controllarlo, perchè potrei non avere
	// ancora caricato in memoria i formattatori programmativi
	if (!sTemp.IsEmpty())
		pDescri->SetFormatter(sTemp);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLReferenceObjectsParser::ParseHotLink (CXMLNode* pNode, CHotlinkDescription* pDescri, const CTBNamespace& aParent)
{
	if (!pDescri->ParsePrototype(pNode, aParent))
		return FALSE;

	CXMLNode* pFunNode = pNode->GetChildByName (XML_FUNCTION_TAG);
	if (!pFunNode)
		return FALSE;

	CString sTitle, sValue;

	// il localize è sul nodo di funzione
	if (pFunNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sTitle) && !sTitle.IsEmpty())
		pDescri->SetNotLocalizedTitle(sTitle);

	// verifico la pubblicazione dell'oggetto
	if (pFunNode->GetAttribute(XML_PUBLISHED_ATTRIBUTE, sValue))
		pDescri->SetPublished (sValue.CompareNoCase(XML_FALSE_VALUE));
	else if (pFunNode->GetAttribute(XML_REPORT_ATTRIBUTE, sValue))
		pDescri->SetPublished (sValue.CompareNoCase(XML_FALSE_VALUE));

	CString sTemp;

	//---- DbField
	CXMLNode* pDbField = pNode->GetChildByName (XML_DBFIELD_TAG);
	if (pDbField)
	{
		if (!pDbField->GetAttribute(XML_NAME_ATTRIBUTE, sTemp) || sTemp.IsEmpty())
			return FALSE;
		pDescri->SetDbField(sTemp);
	}
	//---- DbTable
	CXMLNode* pDbTable = pNode->GetChildByName (XML_DBTABLE_TAG);
	if (pDbTable)
	{
		sTemp.Empty();
		if (!pDbTable->GetAttribute(XML_NAME_ATTRIBUTE, sTemp) || sTemp.IsEmpty())
			return FALSE;

		pDescri->SetDbTable(sTemp);

		sTemp.Empty();
		if (pDbTable->GetAttribute(XML_LoadFullRecord_ATTRIBUTE, sTemp))
			pDescri->SetLoadFullRecord (sTemp.CompareNoCase(XML_FALSE_VALUE));
	}

	//---- DbFieldDescription
	CXMLNode* pDbFieldDescription = pNode->GetChildByName (XML_DBFIELDDESCRIPTION_TAG);
	if (pDbFieldDescription)
	{
		sTemp.Empty();
		if (!pDbFieldDescription->GetAttribute(XML_NAME_ATTRIBUTE, sTemp) || sTemp.IsEmpty())
			return FALSE;

		pDescri->SetDbFieldDescription(sTemp);
	}
	
	//---- CallLink (namespace document)
	CXMLNode* pCallLink = pNode->GetChildByName (XML_CALLLINK_TAG);
	if (pCallLink)
	{
		sTemp.Empty();
		if (pCallLink->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sTemp) && !sTemp.IsEmpty())
			pDescri->SetCallLink(sTemp);

		sTemp.Empty();
		if (pCallLink->GetAttribute(XML_AddOnFlyEnabled_ATTRIBUTE, sTemp) && !sTemp.IsEmpty())
		{
			pDescri->SetAddOnFlyEnabled (sTemp.CompareNoCase(XML_FALSE_VALUE));
		}
		sTemp.Empty();
		if (pCallLink->GetAttribute(XML_MustExistData_ATTRIBUTE, sTemp) && !sTemp.IsEmpty())
		{
			pDescri->SetMustExistData (sTemp.CompareNoCase(XML_TRUE_VALUE) == 0);
		}
		sTemp.Empty();
		if (pCallLink->GetAttribute(XML_BrowseEnabled_ATTRIBUTE, sTemp) && !sTemp.IsEmpty())
		{
			pDescri->SetSearchOnLinkEnabled (sTemp.CompareNoCase(XML_FALSE_VALUE));
		}
	}

	CXMLNode* pEvents = pNode->GetChildByName (XML_EVENTS_TAG);
	if (pEvents)
	{
		CXMLFunctionObjectsParser parser;
		if (!parser.Parse1(pEvents, &(pDescri->m_EventsInfo), pDescri->GetNamespace(), FALSE))
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	//---- Ask Dialogs
	CString sAskDialogs;
	CXMLNode* pAskRules = pNode->GetChildByName (XML_ASKRULES_TAG);
	if (pAskRules)
	{
		CXMLNodeChildsList* pCDATANodes = pAskRules->GetChildsByType(NODE_CDATA_SECTION);
		if (pCDATANodes && pCDATANodes->GetSize() > 0)
		{
			CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
			if (!pCDATANode || !pCDATANode->GetNodeValue(sAskDialogs) || sAskDialogs.IsEmpty())
			{
				//no ask dialogs
			}
		}
		pDescri->SetAskDialogs(sAskDialogs);
		SAFE_DELETE(pCDATANodes);
	}

	//---- SelectionTypes
	CXMLNode* pSelTypeRoot = pNode->GetChildByName (XML_SELECTIONTYPES_TAG);
	if (pSelTypeRoot)
	{	
 		if (!pSelTypeRoot->GetChilds())
			return FALSE;

		CXMLNode* pSelTypeNode;
		CString sNodeName;
		for (int i = 0; i <= pSelTypeRoot->GetChilds()->GetUpperBound(); i++)
		{
			pSelTypeNode = pSelTypeRoot->GetChilds()->GetAt(i);

			if (!pSelTypeNode || !pSelTypeNode->GetName(sNodeName) || sNodeName != XML_SELECTION_TAG)
				continue;

			CString sName;
			if (!pSelTypeNode->GetAttribute(XML_NAME_ATTRIBUTE, sName) || sName.IsEmpty())
				return FALSE;

			//per compatibilità nomi precedenti selection_types e parametri
			if (sName.CompareNoCase(_T("Upper")) == 0)
				sName = _T("Code");
			if (sName.CompareNoCase(_T("Lower")) == 0)
				sName = _T("Description");

			CString sType;
			if (!pSelTypeNode->GetAttribute(XML_TYPE_ATTRIBUTE, sType) || sType.IsEmpty())
			{
				return FALSE;
			}

			CString sVisible;
			BOOL bVisible = TRUE;
			if (pSelTypeNode->GetAttribute(XML_VISIBLE_ATTRIBUTE, sVisible) || sType.IsEmpty())
			{
				bVisible = sVisible.CompareNoCase(_T("true")) == 0;
			}
		
			//per compatibilità nomi precedenti selection_types e parametri
			if (sType.CompareNoCase(_T("Upper")) == 0)
				sType = _T("Code");
			if (sType.CompareNoCase(_T("Lower")) == 0)
				sType = _T("Description");

			CHotlinkDescription::ESelectionType	eType;
			if (sType.CompareNoCase(CHotlinkDescription::s_SelectionType_Direct) == 0)
			{
				eType = CHotlinkDescription::DIRECT;
				sTitle = _TB("Direct access");
			}
			else if (sType.CompareNoCase(CHotlinkDescription::s_SelectionType_Code) == 0)
			{
				eType = CHotlinkDescription::CODE;
				sTitle = _TB("Search on code");
			}
			else if (sType.CompareNoCase(CHotlinkDescription::s_SelectionType_Description) == 0)
			{
				eType = CHotlinkDescription::DESCRIPTION;
				sTitle = _TB("Search on description");
			}
			else if (sType.CompareNoCase(CHotlinkDescription::s_SelectionType_Combo) == 0)
			{
				eType = CHotlinkDescription::COMBO;
				sTitle = _TB("Drop-down list");
			}
			else 
			{
				eType = CHotlinkDescription::CUSTOM;
				sTitle = _TB("Custom search");
			}

			CString sTitle;
			if (!pSelTypeNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sTitle))
			{
				//Added default description return FALSE;
			}
			ASSERT(sTitle);

			CHotlinkDescription::CSelectionType* pAT = new CHotlinkDescription::CSelectionType(sName, sType, eType, sTitle, bVisible);
			pDescri->m_arSelectionTypes.Add(pAT);
		}
	}
	//---- SelectionModes
	CXMLNode* pModeRoot = pNode->GetChildByName (XML_SELECTIONMODES_TAG);
	if (pModeRoot)
	{	
 		if (!pModeRoot->GetChilds())
			return FALSE;

		CXMLNode* pMode;
		CString sNodeName;
		for (int i = 0; i <= pModeRoot->GetChilds()->GetUpperBound(); i++)
		{
			pMode = pModeRoot->GetChilds()->GetAt(i);

			if (!pMode || !pMode->GetName(sNodeName) || sNodeName != XML_MODE_TAG)
				continue;

			CString sName;
			if (!pMode->GetAttribute(XML_NAME_ATTRIBUTE, sName) || sName.IsEmpty())
				return FALSE;

			//per compatibilità nomi precedenti selection_mode e parametri
			if (sName.CompareNoCase(_T("Upper")) == 0)
				sName = _T("Code");
			if (sName.CompareNoCase(_T("Lower")) == 0)
				sName = _T("Description");

			CString sType;
			if (!pMode->GetAttribute(XML_TYPE_ATTRIBUTE, sType) || sType.IsEmpty())
			{
				return FALSE;
			}

			CString sMode;
			CHotlinkDescription::ESelectionMode	eMode;
			if (sType.CompareNoCase(CHotlinkDescription::s_ModeType_Query) == 0)
			{
				eMode = CHotlinkDescription::QUERY;

				CXMLNodeChildsList* pCDATANodes = pMode->GetChildsByType(NODE_CDATA_SECTION);
				if (pCDATANodes && pCDATANodes->GetSize() > 0)
				{
					CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
					if (!pCDATANode || !pCDATANode->GetNodeValue(sMode) || sMode.IsEmpty())
					{
						ASSERT_TRACE2(FALSE, "Wrong dynamic hotlink mode script named %s (%s)", sName, sType);
						return FALSE;
					}
					//per compatibilità nomi precedenti selection_mode e parametri
					//::ReplaceNoCase(sMode, ...) se servisse
					sMode.Replace(_T("hkl_SelectionType"),	_T("selection_type"));
					sMode.Replace(_T("hkl_Description"),	_T("filter_value"));
					sMode.Replace(_T("hkl_Value"),			_T("filter_value"));
				}
				SAFE_DELETE(pCDATANodes);
			} 
			else if (sType.CompareNoCase(CHotlinkDescription::s_ModeType_Report) == 0)
			{
				eMode = CHotlinkDescription::REPORT;

				if (!pMode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sMode) || sMode.IsEmpty())
				{
					ASSERT_TRACE2(FALSE, "Wrong dynamic hotlink mode script named %s (%s)", sName, sType);
					return FALSE;
				}

			}
			else if (sType.CompareNoCase(CHotlinkDescription::s_ModeType_Script) == 0)
			{
				eMode = CHotlinkDescription::SCRIPT;

				CXMLNodeChildsList* pCDATANodes = pMode->GetChildsByType(NODE_CDATA_SECTION);
				if (pCDATANodes && pCDATANodes->GetSize() > 0)
				{
					CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
					if (!pCDATANode || !pCDATANode->GetNodeValue(sMode) || sMode.IsEmpty())
					{
						ASSERT_TRACE2(FALSE, "Wrong dynamic hotlink mode script named %s (%s)", sName, sType);
						return FALSE;
					}
				}
			} 
			else
			{
				return FALSE;
			}

			CHotlinkDescription::CSelectionMode* pSelMode = new CHotlinkDescription::CSelectionMode(sName, eMode, sMode);
			pDescri->m_arSelectionModes.Add(pSelMode);
		}
	}

	CXMLNode* pClassNode = pNode->GetChildByName (XML_CLASSNAME_TAG);
	if (pClassNode)
	{
		CString s;
		pClassNode->GetText(s);
		pDescri->SetClassName(s);
	}

	//---- ComboNode
	CXMLNode* pComboNode = pNode->GetChildByName (XML_COMBOBOX_TAG);
	if (pComboNode)
	{
		// è stato dichiarato che la combo esiste, quindi
		// potrebbe anche necessario gestire i defaults
		pDescri->SetHasComboBox(TRUE);

		CString sDF;
		if (pComboNode->GetAttribute(XML_Datafile_ATTRIBUTE, sDF))
			pDescri->SetDatafile(sDF);

		if (pComboNode->GetChilds())
		{
			// Colonne di Combo
			CComboColumnDescription* pColDescri;
			CXMLNode* pColNode;
			for (int i = 0; i <= pComboNode->GetChilds()->GetUpperBound(); i++)
			{
				pColNode = pComboNode->GetChilds()->GetAt(i);
				if (!pColNode || !pColNode->GetName(sTemp) || sTemp.CompareNoCase(XML_COLUMN_TAG))
					continue;

				pColDescri = new CComboColumnDescription();
				if (ParseComboColumn(pColNode, pColDescri, aParent))
					pDescri->AddComboColumn(pColDescri);
				else
					delete pColDescri;
			}
		}
	}
	
	return TRUE;
}
