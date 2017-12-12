#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\ApplicationContext.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TBGeneric\DatabaseObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>

#include "XmlAddOnDatabaseObjectsParser.h"
#include "XmlDynamicDbObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlDbSignature[]	= _T("Signature");
static const TCHAR szXmlDbRelease[]		= _T("Release");
//----------------------------------------------------------------------------------------------
//	class CXMLAlterTableDescriptionParser implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLAlterTableDescriptionParser, CXMLBaseDescriptionParser)

//----------------------------------------------------------------------------------------------
CXMLAlterTableDescriptionParser::CXMLAlterTableDescriptionParser ()
	:
	CXMLBaseDescriptionParser(XML_ALTERTABLE_TAG)
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLAlterTableDescriptionParser::Parse(CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aModuleNS, const CString &strTableToAlter)
{
	if (!pNode)
		return FALSE;

	if (!CXMLBaseDescriptionParser::Parse(pNode, pDescri, aModuleNS))
		return FALSE;

	CAlterTableDescription* pAlterDescri = (CAlterTableDescription*) pDescri;

	CString sTemp;
	pNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
		pAlterDescri->SetCreationRelease(_ttoi((LPCTSTR) sTemp));

	// fields
	CXMLNodeChildsList* pChildNodes = pNode->GetChilds();
	if (!pChildNodes)
		return TRUE;
	
	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(strTableToAlter);
	if (!pDescription)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CXMLDatabaseObjectsParser::ParseDynamicData(pDescription, pChildNodes, aModuleNS, pAlterDescri);
	return TRUE;
}

//----------------------------------------------------------------------------------------------
//	class CXMLAddColsTableDescriptionParser implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLAddColsTableDescriptionParser, CXMLBaseDescriptionParser)

//----------------------------------------------------------------------------------------------
CXMLAddColsTableDescriptionParser::CXMLAddColsTableDescriptionParser ()
	:
	CXMLBaseDescriptionParser(XML_TABLE_TAG)
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLAddColsTableDescriptionParser::Parse(CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aModuleNS)
{
	if (!pNode)
		return FALSE;

	if (!CXMLBaseDescriptionParser::Parse (pNode, pDescri, aModuleNS))
		return FALSE;

	CAddColsTableDescription* pAddColsDescri = (CAddColsTableDescription*) pDescri;

	// tables
	if (!pNode->GetChilds())
		return TRUE;

	CXMLNode* pAlterNode;
	CString sNodeName;
	CAlterTableDescription* pNewDescription;

	for (int i=0; i <= pNode->GetChilds()->GetUpperBound(); i++)
	{
		pAlterNode = pNode->GetChilds()->GetAt(i);

		if (!pAlterNode || !pAlterNode->GetName(sNodeName) || sNodeName != m_AlterTableParser.GetTagName())
			continue;

		pNewDescription = new CAlterTableDescription ();

		if (m_AlterTableParser.Parse(pAlterNode, pNewDescription, aModuleNS, pAddColsDescri->GetName()))
			pAddColsDescri->m_arAlterTables.Add(pNewDescription);
		else
			delete pNewDescription;
	}

	return TRUE;
}

//----------------------------------------------------------------------------------------------
//							CXMLAddOnDatabaseObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLAddOnDatabaseObjectsParser, CObject)

//----------------------------------------------------------------------------------------------
CXMLAddOnDatabaseObjectsParser::CXMLAddOnDatabaseObjectsParser ()
{
}

//----------------------------------------------------------------------------------------------
BOOL CXMLAddOnDatabaseObjectsParser::Parse (CXMLDocumentObject* pDoc, const CTBNamespace& aParent)
{
	CString sNodeName;

	CXMLNode* pRoot = pDoc->GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || _tcscmp(sRootName, XML_ADDONDBASEOBJECTS_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("The file AddOnDatabaseObjects.xml for module {0-%s} has no root element. File not loaded."), 
							(LPCTSTR) aParent.ToString()));
		return FALSE;
	}

	CXMLNode* pColsNode = pRoot->GetChildByName(XML_OLDADDCOLS_TAG);
	if (!pColsNode || !pColsNode->GetChilds())
		return TRUE;

	CXMLNode* pColNode;
	CAddColsTableDescription* pNewDescription;

	for (int i=0; i <= pColsNode->GetChilds()->GetUpperBound(); i++)
	{
		pColNode = pColsNode->GetChilds()->GetAt(i);

		if (!pColNode || !pColNode->GetName(sNodeName) || sNodeName != m_AddColsParser.GetTagName())
			continue;

		pNewDescription = new CAddColsTableDescription();

		if	(m_AddColsParser.Parse(pColNode, pNewDescription, aParent))
			AfxGetApplicationContext()->GetObject<CAlterTableDescriptionArray>(&CApplicationContext::GetAddOnFieldsTable)->AddAddOnFieldOnTable(pNewDescription);
		else
			delete pNewDescription;
	}

	return TRUE;
}
