
#include "stdafx.h"

#include <io.h>


#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbClientCore\ClientObjects.h>

#include <TbGeneric\OutDateObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\DataFileInfo.h>

#include "XmlDataFileParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
static TCHAR szXmlAuxData[]		= _T("Auxdata");
static TCHAR szXmlHeader[]		= _T("Header");
static TCHAR szXmlFieldType[]	= _T("Fieldtype");
static TCHAR szXmlElements[]	= _T("Elements");
static TCHAR szXmlElement[]		= _T("Element");
static TCHAR szXmlField[]		= _T("Field");
static TCHAR szXmlParameters[]	= _T("Parameters");
static TCHAR szXmlFilterLike[]	= _T("FilterLike");

static const TCHAR szXmlAttrName[]	= _T("name");
static const TCHAR szXmlAttrKey[]	= _T("key");
static const TCHAR szXmlAttrHidden[]= _T("hidden");
static const TCHAR szXmlAttrType[]	= _T("type");
//============================================================================

IMPLEMENT_DYNAMIC(CXMLDataFileParser, CObject);

//----------------------------------------------------------------------------
// LoadDataFile
//   Carica il file identificato da pDataFileInfo->m_Namespace
//     Chiama la CheckDataFile
//     Chiama la LoadHeader
//     Chiama la LoadElements
//     Chiama la LoadParameters
//


//Vengono prima caricate le informazioni presenti nella standard aggiornate poi con le informazioni presenti nella custom allusers

//
//Vengono prima caricate le informazioni presenti nella standard aggiornate poi con le informazioni presenti nella custom allusers
//--------------------------------------------------------------------------------------------------------------------------
BOOL CXMLDataFileParser::LoadDataFile (CDataFileInfo* pDataFileInfo)
{
	if (!pDataFileInfo)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	p_mDFI = pDataFileInfo;

	CString strCulture;

	if (p_mDFI->m_bUseProductLanguage)
		strCulture = AfxGetLoginManager()->GetProductLanguage();
	else
		strCulture = AfxGetCulture();
	
	if (strCulture.IsEmpty()) strCulture = _T("en");


	//prima carico il file prensente nella standard se esiste (posso avere uno solo nella custom????)
	CString strFileName = AfxGetPathFinder()->GetModuleDataFilePath (p_mDFI->m_Namespace, CPathFinder::STANDARD) + SLASH_CHAR + strCulture + SLASH_CHAR + p_mDFI->m_strFileName;
	BOOL bResult = ParseDataFile(strFileName, FALSE); 
	strFileName = AfxGetPathFinder()->GetModuleDataFilePath (p_mDFI->m_Namespace, CPathFinder::ALL_USERS) + SLASH_CHAR + strCulture + SLASH_CHAR + p_mDFI->m_strFileName;
	return bResult && ParseDataFile(strFileName, TRUE);	
}



//--------------------------------------------------------------------------------------------------------------------------
BOOL CXMLDataFileParser::ParseDataFile(const CString& strFileName, BOOL bCustom)
{
	// Controllo l'esistenza dei files
	if (!ExistFile(strFileName))
	{
		if (!bCustom)
			TRACE(_T("Missing file: ") + strFileName + _T(".\r\n"));
		return bCustom;
	}

	CLocalizableXMLDocument aDocument(p_mDFI->m_Namespace, AfxGetPathFinder());

	// Controllo se l'xml ha un formato valido
	if (!aDocument.LoadXMLFile(strFileName))
	{
		AfxGetDiagnostic()->Add (_TB("The xml file has a format error.") + p_mDFI->m_Namespace.ToString());
		return FALSE;
	}
	
	// Controllo se l'xml ha un tag radice "AuxData"
	CXMLNode* pRoot = aDocument.GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || sRootName.CompareNoCase (szXmlAuxData) != 0) 
	{
		AfxGetDiagnostic()->Add(_TB("The root node of the xml file is missing.") + (LPCTSTR) p_mDFI->m_Namespace.ToString());
		return FALSE;
	}

	//la parte di header file la leggo solo per il file standard poichò il custom apporta solo gli elementi modificati o aggiunti
	BOOL bOk = (!bCustom) ? LoadHeader(aDocument) : TRUE;
	bOk = bOk && LoadElements(aDocument, bCustom); //gli elementi li carico sia se file standard che file custom
	bOk = bOk && (!bCustom) ? LoadParameters(aDocument) : TRUE; //carico i parametro solo se file standard
	// C gli elementi del file xml comprendendo un controllo sulla validità dei campi
	return bOk;
}

//----------------------------------------------------------------------------
// LoadHeader
//   Carica e controlla l'header del file xml contenuto nel tag <Header>
//     Chiama la ParseTagFieldType
//
BOOL CXMLDataFileParser::LoadHeader(CLocalizableXMLDocument& aDocument)
{
	BOOL bOk = TRUE;

	// Mi leggo la lista di FieldType
	CXMLNodeChildsList* pFieldTypeNodes = aDocument.SelectNodes (_T("/") + CString(szXmlAuxData) + _T("/") + szXmlHeader + _T("/") + szXmlFieldType);

	// Se non ci sono FieldType non ne carico ma non ritorno errori
	if (!pFieldTypeNodes || !pFieldTypeNodes->GetSize())
		return TRUE;

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pFieldTypeNodes->GetUpperBound(); i++)
		if (pFieldTypeNodes->GetAt(i) && !ParseTagFieldType(pFieldTypeNodes->GetAt(i)))
			bOk = FALSE;

	delete pFieldTypeNodes;

	return bOk;
}

//----------------------------------------------------------------------------
// LoadParameters
//   Carica e controlla ii Parametri del file xml contenuti nel tag <Parameters>
//     Chiama la ParseTagFilterLike
//
BOOL CXMLDataFileParser::LoadParameters(CLocalizableXMLDocument& aDocument)
{
	BOOL bOk = TRUE;

	// Cerco il nodo FilterLike
	CString sXQ = _T("/") + CString(szXmlAuxData) + _T("/") + szXmlParameters + _T("/") + szXmlFilterLike;
	CXMLNode* pFilterLikeNode = aDocument.SelectSingleNode (sXQ);

	// Se non c'è FilterLike non lo carico ma non ritorno errori
	if (!pFilterLikeNode)
		return TRUE;

	// anche se il TAG FilterLike è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	if (!ParseTagFilterLike(pFilterLikeNode))
		bOk = FALSE;

	delete pFilterLikeNode;

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagFilterLike
//   Carica e controlla il parametro contenuto nel TAG <FilterLike>
//
BOOL CXMLDataFileParser::ParseTagFilterLike(CXMLNode* pTagNode)
{
	BOOL bOk  = TRUE;

	CString	strTmpValue;

	// leggo il valore del tag <FilterLike>
	if (pTagNode->GetText(strTmpValue) && !strTmpValue.IsEmpty() && strTmpValue.CompareNoCase(_T("true")) == 0)
		p_mDFI->m_bFilterLike = TRUE;
	else
		p_mDFI->m_bFilterLike = FALSE;

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagFieldType
//   Carica e controlla la definizione dei filed del file xml contenuti nei tag <Fieldtype>
//
BOOL CXMLDataFileParser::ParseTagFieldType(CXMLNode* pTagNode)
{
	BOOL bOk  = TRUE;

	// leggo l'attributo name del TAG
	CString	strTagName;
	if (!pTagNode->GetAttribute(szXmlAttrName, strTagName) || strTagName.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> element without attribute name!"));
		return FALSE;
	}

	// leggo l'attributo key
	CString	strTagKey;
	DataBool bKey = FALSE;

	if (pTagNode->GetAttribute(szXmlAttrKey, strTagKey) && !strTagKey.IsEmpty())
	{
		bKey.AssignFromXMLString(strTagKey);
	}

	// leggo l'attributo hidden
	CString	strTagHidden;
	DataBool bHidden = FALSE;
	
	if (pTagNode->GetAttribute(szXmlAttrHidden, strTagHidden) && !strTagHidden.IsEmpty())
		bHidden.AssignFromXMLString(strTagHidden);

	// leggo l'attributo type
	CString	strTagType;
	DataType tType = DataType::String; // Se non lo trovo di default vale string

	if (pTagNode->GetAttribute(szXmlAttrType, strTagType) && !strTagType.IsEmpty())
		tType = CDataObjDescription::ToDataType(strTagType);

	// eseguo i controllo sulla chiave: deve essere unica e di tipo string
	if (bKey)
	{
		if (p_mDFI->m_arElementTypes.GetKey() >= 0)
		{
			AfxGetDiagnostic()->Add (_TB("Duplicate key definition!"));
			bOk = FALSE;
		}
		else
			if (tType != DataType::String)
			{
				AfxGetDiagnostic()->Add (_TB("The key has an incorrect data type. Only string keys are supported!"));
				bOk = FALSE;
			}
			else
				p_mDFI->m_arElementTypes.SetKey(p_mDFI->m_arElementTypes.GetSize());
	}
	
	// aggiungo l'elemento fieldtype al CDataFileInfo
	p_mDFI->m_arElementTypes.Add(new CDataFileElementFieldType(strTagName, tType, bHidden, bKey));
	return bOk;
}

//----------------------------------------------------------------------------
// LoadElements
//   Carica e controlla gli elementi del file xml contenuti nel tag <Elements>
//     Chiama la ParseTagElement
//
BOOL CXMLDataFileParser::LoadElements(CLocalizableXMLDocument& aDocument, BOOL bFromCustom)
{
	BOOL bOk = TRUE;

	CXMLNodeChildsList* pFieldElementsNodes = aDocument.SelectNodes (_T("/") + CString(szXmlAuxData) + _T("/") + szXmlElements + _T("/") + szXmlElement);

	// Se non ci sono FieldType non ne carico ma non ritorno errori
	if (!pFieldElementsNodes || !pFieldElementsNodes->GetSize())
		return TRUE;

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pFieldElementsNodes->GetUpperBound(); i++)
		if (pFieldElementsNodes->GetAt(i) && !ParseTagElement(pFieldElementsNodes->GetAt(i), bFromCustom))
			bOk = FALSE;

	delete pFieldElementsNodes;

	// eseguo i controllo sull'esistenza della chiave
	if (p_mDFI->m_arElementTypes.GetKey() < 0)
	{
		AfxGetDiagnostic()->Add (_TB("Key definition is missing!"));
		bOk = FALSE;
	}

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagElement
//   Carica e controlla un singolo elemento del file xml contenuto nel tag <Element>
//     Chiama la ParseTagField
//
BOOL CXMLDataFileParser::ParseTagElement(CXMLNode* pTagNode, BOOL bFromCustom)
{
	BOOL bOk = TRUE;

	CXMLNodeChildsList* pFieldElementNodes = pTagNode->SelectNodes (szXmlField);

	// Se non ci sono FieldType non ne carico ma non ritorno errori
	if (!pFieldElementNodes || !pFieldElementNodes->GetSize())
		return TRUE;

	// creo l'elemento element
	CDataFileElement* pElement = new CDataFileElement();
	pElement->m_bFromCustom = bFromCustom;
	// aggiungo l'elemento field al CDataFileInfo
	p_mDFI->m_arElements.Add(pElement);

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pFieldElementNodes->GetUpperBound(); i++)
		if (pFieldElementNodes->GetAt(i) && !ParseTagField(pFieldElementNodes->GetAt(i), pElement))
			bOk = FALSE;

	// controllo se nell'elemento è presente il campo di chiave
	if (pElement->GetElementValue(p_mDFI->m_arElementTypes.GetKeyName()) == NULL)
			{
				AfxGetDiagnostic()->Add (_TB("Key field is missing!"));
				bOk = FALSE;
			}

	// controllo se nell'elemento sono presenti tutti i campi da visualizzare (hidden = 0)
	BOOL bFound = FALSE;
	for (int i=0; i <= p_mDFI->m_arElementTypes.GetUpperBound(); i++)
	{
		if (!p_mDFI->m_arElementTypes[i]->m_bHidden)
		{
			bFound = TRUE;
			if (pElement->GetElementValue(p_mDFI->m_arElementTypes[i]->m_sName) == NULL)
			{
				AfxGetDiagnostic()->Add (_TB("Description field is missing!"));
				bOk = FALSE;
			}
		}
	}

	// controllo se nell'elemento è presente almeno un campo da visualizzare
	if (!bFound)
	{
		AfxGetDiagnostic()->Add (_TB("No description fields in file!"));
		bOk = FALSE;
	}

	delete pFieldElementNodes;

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagField
//   Carica e controlla un singolo field del file xml contenuto nel tag <Field>
//
BOOL CXMLDataFileParser::ParseTagField(CXMLNode* pTagNode, CDataFileElement* pElement)
{
	BOOL bOk = TRUE;

	// leggo l'attributo name del TAG
	CString	strTagName;
	if (!pTagNode->GetAttribute(szXmlAttrName, strTagName) || strTagName.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> element without attribute name!"));
		return FALSE;
	}

	// leggo il valore del tag
	CString	strTmpValue;

	DataObj* dataValue = NULL;
	if (pTagNode->GetText(strTmpValue) && !strTmpValue.IsEmpty())
	{
		DataType tTipo = p_mDFI->GetFieldType(strTagName)->m_Type;
		dataValue = DataObj::DataObjCreate(tTipo);
		dataValue->Assign(strTmpValue);
	}

	// creo l'elemento field
	CDataFileElementField* pField = new CDataFileElementField(strTagName, dataValue);

	// aggiungo l'elemento field al CDataFileInfo
	pElement->Add(pField);

	return bOk;
}



//--------------------------------------------------------------------------------------------------------------------------
void  CXMLDataFileParser::SaveDataFile (CDataFileInfo* pDataFileInfo)
{
	if (!pDataFileInfo)
		ASSERT(FALSE);

	p_mDFI = pDataFileInfo;

	CString strCulture;

	if (p_mDFI->m_bUseProductLanguage)
		strCulture = AfxGetLoginManager()->GetProductLanguage();
	else
		strCulture = AfxGetCulture();
	
	if (strCulture.IsEmpty()) strCulture = _T("en");

	//Le modifiche le salvo solo sul file custom
	UnparseDataFile(AfxGetPathFinder()->GetModuleDataFilePath (p_mDFI->m_Namespace, CPathFinder::ALL_USERS) + SLASH_CHAR + strCulture + SLASH_CHAR + p_mDFI->m_strFileName);	
}
//--------------------------------------------------------------------------------------------------------------------------
void  CXMLDataFileParser::UnparseDataFile(const CString& strFileName)
{
	CLocalizableXMLDocument aDocument(p_mDFI->m_Namespace, AfxGetPathFinder());
	CXMLNode* pnRoot = aDocument.CreateRoot(szXmlAuxData);	
	UnparseElements(pnRoot);	
	aDocument.SaveXMLFile(strFileName, TRUE);
}

//--------------------------------------------------------------------------------------------------------------------------
void CXMLDataFileParser::UnparseHeader(CXMLNode* pnRoot)
{
	CXMLNode* pnHeader = pnRoot->CreateNewChild(szXmlHeader);
	CXMLNode* pnFieldType = NULL;
	CDataFileElementFieldType* pType = NULL;
	for (int i = 0; i <= p_mDFI->m_arElementTypes.GetUpperBound(); i++)
	{
		pnFieldType = pnHeader->CreateNewChild(szXmlFieldType);
		pType = p_mDFI->m_arElementTypes.GetAt(i);
		pnFieldType->SetAttribute(szXmlAttrName, pType->m_sName);
		pnFieldType->SetAttribute(szXmlAttrKey, pType->m_bKey.FormatDataForXML());
		pnFieldType->SetAttribute(szXmlAttrHidden, pType->m_bHidden.FormatDataForXML());
		pnFieldType->SetAttribute(szXmlAttrType, CDataObjDescription::ToString(pType->m_Type));	
	}
	if (p_mDFI->m_bFilterLike)
	{
		CXMLNode* pnParameeters = pnRoot->CreateNewChild(szXmlParameters);
		pnParameeters->CreateNewChild(szXmlFilterLike)->SetText( _T("true"));
	}	
}

//--------------------------------------------------------------------------------------------------------------------------
void CXMLDataFileParser::UnparseElements(CXMLNode* pnRoot)
{
	//non ci sono elementi
	if (p_mDFI->m_arElements.GetCount() == 0)
		return;

	CXMLNode* pnElements = pnRoot->CreateNewChild(szXmlElements);
	CXMLNode* pnElement = NULL;
	CXMLNode* pnField = NULL;

	CDataFileElement* pDataElement = NULL;
	CDataFileElementField* pField = NULL;
	for (int i = 0; i < p_mDFI->m_arElements.GetCount(); i++)
	{
		pDataElement = p_mDFI->m_arElements.GetAt(i);
		if (pDataElement &&  pDataElement->m_bFromCustom)
		{
			pnElement = pnElements->CreateNewChild(szXmlElement);
			for (int j = 0; j < pDataElement->GetSize(); j++)
			{
				pField = pDataElement->GetAt(j);
				pnField = pnElement->CreateNewChild(szXmlField);
				pnField->SetAttribute(szXmlAttrName, pField->m_sName);
				pnField->SetText(pField->m_pValue->FormatDataForXML());
			}
		}
	}
}

