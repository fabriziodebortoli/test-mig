#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\ApplicationContext.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>
#include <TbXmlCore\XMLGeneric.h>

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

	pNode->GetAttribute(XML_CREATESTEP_ATTRIBUTE, sTemp);
	if (!sTemp.IsEmpty())
		pAlterDescri->SetCreationStep(_ttoi((LPCTSTR)sTemp));

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
void CXMLAlterTableDescriptionParser::Unparse(CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aModuleNS, const CString &strTableToAlter)
{
	if (!pNode)
		return;

	CXMLBaseDescriptionParser::Unparse(pNode, pDescri);

	CAlterTableDescription* pAlterDescri = (CAlterTableDescription*)pDescri;

	CString sTemp;
	pNode->SetAttribute(XML_RELEASE_ATTRIBUTE, FormatIntForXML(pAlterDescri->GetCreationRelease()));	
	pNode->SetAttribute(XML_CREATESTEP_ATTRIBUTE, FormatIntForXML(pAlterDescri->GetCreationStep()));

	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(strTableToAlter);
	if (!pDescription)
	{
		ASSERT(FALSE);
		return;
	}
	CXMLNode* pFieldsNode = pNode->CreateNewChild(_T("Columns")); 	

	CXMLDatabaseObjectsParser::UnparseFields(pFieldsNode, pDescription, aModuleNS);
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
void CXMLAddColsTableDescriptionParser::Unparse(CXMLNode* pNode, CBaseDescription* pDescri, const CTBNamespace& aParent)
{
	if (!pNode)
		return;

	CXMLBaseDescriptionParser::Unparse(pNode, pDescri);
	CAddColsTableDescription* pAddColsDescri = (CAddColsTableDescription*)pDescri;
	
	for (int i = 0; i < pAddColsDescri->m_arAlterTables.GetSize(); i++)
		m_AlterTableParser.Unparse(pNode, pAddColsDescri->m_arAlterTables.GetAt(i), aParent, pAddColsDescri->GetName());
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
BOOL CXMLAddOnDatabaseObjectsParser::LoadAdddOnDatabaseObjects(const CTBNamespace& aModuleNS)
{
	// addon ai database objects
	CString sFileName = AfxGetPathFinder()->GetAddOnDbObjectsFullName(aModuleNS, CPathFinder::STANDARD);
	if (!ExistFile(sFileName))
		sFileName = AfxGetPathFinder()->GetAddOnDbObjectsFullName(aModuleNS, CPathFinder::CUSTOM);

	if (!ExistFile(sFileName))
		return TRUE;

	CLocalizableXMLDocument aDoc(aModuleNS, AfxGetPathFinder());
	aDoc.EnableMsgMode(FALSE);
	TRY
	{
		return aDoc.LoadXMLFile(sFileName) && Parse(&aDoc, aModuleNS);
	}
	CATCH(CException, e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} file has not been loaded due to the following error {1-%s} "), sFileName, szError), CDiagnostic::Warning);
		return FALSE;
	}
	END_CATCH	
		
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
//----------------------------------------------------------------------------------------------
BOOL CXMLAddOnDatabaseObjectsParser::SaveAdddOnDatabaseObjects(CAlterTableDescriptionArray* pAlterTableDescription)
{
	if (!pAlterTableDescription || pAlterTableDescription->GetSize() == 0)
		return TRUE;

	CAddColsTableDescription* pAddOnCols = NULL;
	CString sFileName;

	for (int i = 0; i < pAlterTableDescription->GetSize(); i++)
	{
		pAddOnCols = (CAddColsTableDescription*)pAlterTableDescription->GetAt(i);
		if (pAddOnCols)
		{
			sFileName = AfxGetPathFinder()->GetAddOnDbObjectsFullName(pAddOnCols->GetNamespace(), CPathFinder::STANDARD);
			if (!ExistFile(sFileName))
				sFileName = AfxGetPathFinder()->GetAddOnDbObjectsFullName(pAddOnCols->GetNamespace(), CPathFinder::CUSTOM);
			if (ExistFile(sFileName))
			{
				CXMLDocumentObject aXMLDatabaseObj(TRUE);
				TRY
				{
					Unparse(&aXMLDatabaseObj, pAddOnCols, pAddOnCols->GetNamespace());
					aXMLDatabaseObj.SaveXMLFile(sFileName);
				}
					CATCH(CException, e)
				{
					TCHAR szError[1024];
					e->GetErrorMessage(szError, 1024);
					AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} file has not been saved due to the following error {1-%s} "), sFileName, szError), CDiagnostic::Warning);
					return FALSE;
				}
				END_CATCH
			}
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CXMLAddOnDatabaseObjectsParser::Unparse(CXMLDocumentObject* pDoc, CAddColsTableDescription* pAddOnCols, const CTBNamespace& aParent)
{
	if (!pDoc || !pAddOnCols)
		return;

	CXMLNode* pnRoot = pDoc->CreateRoot(XML_ADDONDBASEOBJECTS_TAG);
	if (!pnRoot)
		return;
	CXMLNode* pnNode = pnRoot->CreateNewChild(XML_OLDADDCOLS_TAG);
	
	CAddColsTableDescription* pAlterTable = NULL;
	for (int i = 0; i < pAddOnCols->m_arAlterTables.GetCount(); i++)
	{
		pAlterTable = (CAddColsTableDescription*)pAddOnCols->m_arAlterTables.GetAt(i);
		m_AddColsParser.Unparse(pnNode, pAlterTable, aParent);
	}
}