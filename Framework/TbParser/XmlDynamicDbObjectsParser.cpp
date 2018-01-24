#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\TBResourceLocker.h>
#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\LoginContext.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>
#include <TbXmlCore\XMLGeneric.h>

#include <TBGeneric\DatabaseObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\BinaryFileReader.h>

#include "XmlDynamicDbObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlDbSignature[]			= _T("Signature");
static const TCHAR szXmlDbRelease[]				= _T("Release");
static const TCHAR szColumns[]					= _T("Columns");
static const TCHAR szSegmentKey[]				= _T("SegmentKey");


static const TCHAR szParameters[]				= _T("Parameters");
static const TCHAR szColumn[]					= _T("Column");
static const TCHAR szParameter[]				= _T("Parameter");

static const TCHAR szUsers[]					= _T("users");
static const TCHAR szCompanies[]				= _T("companies");
static const TCHAR szAllUsersCompanies[]		= _T("all");
static const TCHAR szXmlUserCompanySeparator[]	= _T(",");
static const TCHAR szXmlDynamic[]				= _T("dynamic");
static const TCHAR szTemplateNs[]				= _T("templateNamespace");

static const int   nDefaultLength				= 10;
static const int   nDatabaseObjectsBinVersion	= 1;
//----------------------------------------------------------------------------------------------
//							CXMLDatabaseObjectsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDatabaseObjectsParser, CXMLBaseDescriptionParser)

//----------------------------------------------------------------------------------------------
CXMLDatabaseObjectsParser::CXMLDatabaseObjectsParser ()
{
}

//-----------------------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::LoadDatabaseObjects (const CTBNamespace& aNamespace, DatabaseObjectsTable* pTable)
{
	CString sFileName = AfxGetPathFinder()->GetDatabaseObjectsFullName(aNamespace, CPathFinder::STANDARD);
	if (!ExistFile(sFileName))
		sFileName = AfxGetPathFinder()->GetDatabaseObjectsFullName(aNamespace, CPathFinder::CUSTOM);
	if (!ExistFile(sFileName))
	{
		DatabaseReleasesTablePtr pReleaseTablePtr = AfxGetWritableDatabaseReleasesTable();
		DatabaseReleasesTable* pReleaseTable = pReleaseTablePtr.GetPointer();
		if (pReleaseTable->GetReleaseOf(aNamespace.ToString()) <= 0)
			pReleaseTable->AddRelease(aNamespace.ToString(), aNamespace.GetObjectName(), 0);
		
		return TRUE;
	}

	CLocalizableXMLDocument aDoc(aNamespace, AfxGetPathFinder());

	TRY
	{
		if (aDoc.LoadXMLFile (sFileName) && Parse(&aDoc, pTable, aNamespace))
			return TRUE;
	}
	CATCH (CException, e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("{0-%s} file has not been loaded due to the following error {1-%s} "), sFileName, szError), CDiagnostic::Warning);
		return FALSE;
	}
	END_CATCH

	return FALSE;
}


//----------------------------------------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::Parse(
											CXMLDocumentObject*		pDoc, 
											DatabaseObjectsTable*	pTable, 
											const CTBNamespace&		aParent
										)
{
	CXMLNode* pRoot = pDoc->GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || _tcscmp(sRootName, XML_DBOBJECTS_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("The file {0-%s} for module {1-%s} has no root element. File not loaded."), 
							(LPCTSTR) pDoc->GetFileName(), 
							(LPCTSTR) aParent.ToString()));
		return FALSE;
	}

	CString sSignature;
	// signature di database
	CXMLNode* pInfoNode = pRoot->GetChildByName (szXmlDbSignature);
	if (pInfoNode)
		pInfoNode->GetText(sSignature);

	if (sSignature.IsEmpty ())
		sSignature = aParent.GetObjectName(CTBNamespace::MODULE);
	
	// release di database
	CString sNodeName;
	pInfoNode = pRoot->GetChildByName (szXmlDbRelease);
	if (pInfoNode)
		pInfoNode->GetText(sNodeName);

	int nRelease = sNodeName.IsEmpty () ? 0 : _ttoi(sNodeName);

	DatabaseReleasesTablePtr pReleaseTablePtr = AfxGetWritableDatabaseReleasesTable();
	DatabaseReleasesTable* pReleaseTable = pReleaseTablePtr.GetPointer();
	pReleaseTable->AddRelease (aParent.ToString(), sSignature, nRelease);

	// tables
	CXMLNode* pChildsNode = pRoot->GetChildByName(XML_TABLES_TAG);
	ParseGroup (pChildsNode, pTable, aParent, XML_TABLE_TAG);

	// views
    pChildsNode = pRoot->GetChildByName(XML_VIEWS_TAG);
	ParseGroup (pChildsNode, pTable, aParent, XML_VIEW_TAG);

	// procedures
    pChildsNode = pRoot->GetChildByName(XML_PROCEDURES_TAG);
	ParseGroup (pChildsNode, pTable, aParent, XML_PROCEDURE_TAG);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::ParseGroup (
												CXMLNode* pNode, 
												DatabaseObjectsTable* pTable, 
												const CTBNamespace& aParent, 
												const CString& sTag
											)
{
	if (!pNode || !pNode->GetChilds())
		return FALSE;

	CTBNamespace::NSObjectType aType = CTBNamespace::TABLE;
	if (_tcscmp(sTag, XML_VIEW_TAG) == 0)
		aType = CTBNamespace::VIEW;
	else if (_tcscmp(sTag, XML_PROCEDURE_TAG) == 0)
		aType = CTBNamespace::PROCEDURE;
	else if (_tcscmp(sTag, XML_VIRTUAL_TABLE_TAG) == 0)
		aType = CTBNamespace::VIRTUAL_TABLE;

	SetTagName(sTag);
	CXMLNode* pObjectNode;
	CString sNodeName;
	CDbObjectDescription* pNewDescription;

	for (int i=0; i <= pNode->GetChilds()->GetUpperBound(); i++)
	{
		pObjectNode = pNode->GetChilds()->GetAt(i);

		if (!pObjectNode || !pObjectNode->GetName(sNodeName) || sNodeName != sTag)
			continue;

		pNewDescription = new CDbObjectDescription (aType);

		if	(
				!ParseDbObject(pObjectNode, pNewDescription, aParent) ||
				pTable->AddObject(pNewDescription) <= 0
			)
			delete pNewDescription;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::ParseDbObject	(
														CXMLNode* pNode, 
														CDbObjectDescription* pDescri, 
														const CTBNamespace& aModuleNS
													)
{

	if (!pNode || !pDescri)
		return FALSE;

	CString sTemp;
	CTBNamespace aNs;
	pNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, sTemp);

	if (sTemp.IsEmpty())
	{
		pNode->GetAttribute(XML_NAME_ATTRIBUTE, sTemp);
	}
	aNs.AutoCompleteNamespace(pDescri->GetType(), sTemp, aModuleNS);
	if (aNs.IsValid() && !aNs.IsEmpty())
	{
		pDescri->SetNamespace(aNs);
		pDescri->SetName(pDescri->GetNamespace().GetObjectName());
	}
	else
		pDescri->SetName(sTemp);
	
	// se non ha name lo salto
	if (sTemp.IsEmpty())
	{
		CString sMessage;
		sMessage.Format 
			(
				_TB("Missing name for database object %s file: attribute <name> or <namespace> not found. Registration ignored."),
				(LPCTSTR) pNode->GetXMLDocument()->GetFileName()
			);
		AfxGetDiagnostic()->Add(sMessage, CDiagnostic::Warning);
		return FALSE;
	}
	
	//attributo mastertable
	pNode->GetAttribute(XML_MASTERTABLE_ATTRIBUTE, sTemp);
	DataBool bMaster;
	bMaster.AssignFromXMLString(sTemp);
	pDescri->SetMasterTable(bMaster);

	// localization
	CXMLNode* pDocNode;
	if (!pNode->GetAttribute(XML_LOCALIZE_ATTRIBUTE, sTemp))
		if (pDocNode = pNode->GetChildByName(XML_TITLE_TAG))
			pDocNode->GetText(sTemp);

	if (sTemp.IsEmpty())
		sTemp = pDescri->GetName();

	pDescri->SetNotLocalizedTitle(sTemp);
	pDescri->SetOwner(aModuleNS);
	
	sTemp.Empty();
	pNode->GetAttribute(szXmlDynamic, sTemp);
	if (sTemp.CompareNoCase(XML_TRUE_VALUE) == 0 || pDescri->GetNamespace().HasAFakeLibrary())
		pDescri->SetDeclarationType(CDbObjectDescription::Dynamic);
	else
		pDescri->SetDeclarationType(CDbObjectDescription::Coded);

	sTemp.Empty();
	pNode->GetAttribute(szTemplateNs, sTemp);
	CTBNamespace templateNs (sTemp);
	if (!templateNs.IsEmpty() && templateNs.IsValid())
		pDescri->SetTemplateNamespace(templateNs);

	CXMLNodeChildsList* pChildNodes = pNode->GetChilds();
	if (!pChildNodes)
		return TRUE;
	
	ParseDynamicData(pDescri, pChildNodes, aModuleNS, NULL);
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::ParseFields(
	CXMLNode* pNode,
	const CString& sType,
	CDbObjectDescription* pDescri,
	const CTBNamespace& moduleNamespace,
	CAlterTableDescription* pAlterDescri,
	BOOL bIsAdditionalColumns
	)
{
	CXMLNodeChildsList* pChilds = pNode->GetChilds();
	if (!pChilds)
		return TRUE;

	CString aType;
	CString sName;
	CString sTemp;
	CString sCorrectType;

	// type checking defiition
	CDbFieldDescription::DbColumnType eCorrectType;
	if (_tcscmp(sType, szColumns) == 0)
	{
		sCorrectType = szColumn;
		eCorrectType = CDbFieldDescription::Column;
	}
	else if (_tcscmp(sType, XML_VARIABLES_TAG) == 0)
	{
		sCorrectType = XML_VARIABLE_TAG;
		eCorrectType = CDbFieldDescription::Variable;
	}
	else if (_tcscmp(sType, szParameters) == 0)
	{
		sCorrectType = szParameter;
		eCorrectType = CDbFieldDescription::Parameter;
	}

	// child nodes
	CXMLNode* pChildNode;
	CDbFieldDescription* pFieldDescri;
	for (int i = 0; i <= pChilds->GetUpperBound(); i++)
	{
		pChildNode = pChilds->GetAt(i);

		if (!pChildNode || !pChildNode->GetName(aType) || _tcscmp(aType, sCorrectType) || !pChildNode->GetName(sName))
			continue;

		pFieldDescri = pDescri->GetDynamicFieldByName(sName);
		BOOL bNewed = pFieldDescri == NULL;

		if (bNewed)
		{
			pFieldDescri = new CDbFieldDescription(moduleNamespace);
			pFieldDescri->SetColType(eCorrectType);
			pFieldDescri->SetIsAddOn(pDescri->GetNamespace() != moduleNamespace);			
		}

		if (!pFieldDescri->Parse(pChildNode, TRUE) && bNewed)
		{
			delete pFieldDescri;
			continue;
		}
		//ho l'attributo? uso quello
		if (pChildNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp))
			pFieldDescri->SetCreationRelease(_ttoi(sTemp));
		else if (pAlterDescri) //in subordine quello dell'altertable
			pFieldDescri->SetCreationRelease(pAlterDescri->GetCreationRelease());
		else //infine quello della tabella
			pDescri->GetCreationRelease();

		if (pChildNode->GetAttribute(XML_DEFAULT_VALUE_ATTRIBUTE, sTemp))
			pFieldDescri->SetValue(sTemp);
		if (pChildNode->GetAttribute(XML_CONTEXT_NAME_ATTRIBUTE, sTemp))
			pFieldDescri->SetContextName(sTemp);

		if (
			pFieldDescri->GetColType() != CDbFieldDescription::Column &&
			pFieldDescri->GetDataType() == DataType::String &&
			pFieldDescri->GetLength() <= 0
			)
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Variable {0-%s} declared into record {1-%s}\r\n has no length declared into {2-%s}!\r\nVariable ignored!"), pFieldDescri->GetName(), pDescri->GetName(), pNode->GetXMLDocument()->GetFileName()));
			delete pFieldDescri;
			continue;
		}

		if (bNewed)
			pDescri->AddDynamicField(pFieldDescri);
	}

	return TRUE;
}

//----------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::ParseDynamicData(
	CDbObjectDescription* pDescri, 
	CXMLNodeChildsList* pChildNodes,
	const CTBNamespace& moduleNamespace,
	CAlterTableDescription* pAlterDescri)
{
	CString sTemp;
	
	CDbObjectDescription* pTableDescri = (CDbObjectDescription*) pDescri;

	CXMLNode* pChildNode;
	for (int i=0; i <= pChildNodes->GetUpperBound(); i++)
	{
		pChildNode = pChildNodes->GetAt(i);
		if (!pChildNode->GetName(sTemp))
			continue;
		if (_tcscmp(sTemp, XML_CREATE_TAG) == 0)
		{
			if (pChildNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp))
				pTableDescri->SetCreationRelease(_ttoi(sTemp));
			if (pChildNode->GetAttribute(XML_CREATESTEP_ATTRIBUTE, sTemp))
				pTableDescri->SetCreationStep(_ttoi(sTemp));
		}
		else if (	_tcscmp(sTemp, szColumns) == 0 ||
				_tcscmp(sTemp, XML_VARIABLES_TAG) == 0 ||
				(pDescri->GetType() ==  CTBNamespace::PROCEDURE && _tcscmp(sTemp, szParameters) == 0)
			)
			ParseFields (pChildNode, sTemp, pTableDescri, moduleNamespace, pAlterDescri, FALSE); 
	}
	return TRUE;
}



//----------------------------------------------------------------
BOOL CXMLDatabaseObjectsParser::SaveDatabaseObjects(DatabaseObjectsTable* pTable)
{
	if (!pTable)
		return FALSE;

	CString sFileName;
	CDbReleaseDescription* pDBRel;
	CString strModuleNs;
	POSITION pos;
	CMapStringToOb* pReleaseMap = AfxGetDatabaseReleasesTable()->GetDatabaseReleaseMap();
	for (pos = pReleaseMap->GetStartPosition(); pos != NULL;)
	{
		pReleaseMap->GetNextAssoc(pos, strModuleNs, (CObject*&)pDBRel);		
		if (pDBRel && !strModuleNs.IsEmpty())
		{
			sFileName = AfxGetPathFinder()->GetDatabaseObjectsFullName(strModuleNs, CPathFinder::STANDARD);
			if (!ExistFile(sFileName))
				sFileName = AfxGetPathFinder()->GetDatabaseObjectsFullName(strModuleNs, CPathFinder::CUSTOM);

			if (ExistFile(sFileName))
			{
				CXMLDocumentObject aXMLDatabaseObj(TRUE);
				TRY
				{
					Unparse(&aXMLDatabaseObj, pTable, pDBRel, strModuleNs);
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
	return FALSE;
}

//----------------------------------------------------------------------------------------------
void CXMLDatabaseObjectsParser::Unparse( CXMLDocumentObject* pDoc,	DatabaseObjectsTable*	pTable, CDbReleaseDescription* pDBRel, const CTBNamespace&	aParent)
{
	CXMLNode* pnRoot = pDoc->CreateRoot(XML_DBOBJECTS_TAG);
	if (!pnRoot)
		return;

	CXMLNode* pnNode = pnRoot->CreateNewChild(szXmlDbSignature);
	pnNode->SetText(pDBRel->GetSignature());

	pnNode = pnRoot->CreateNewChild(szXmlDbRelease);
	pnNode->SetText(FormatIntForXML(pDBRel->GetRelease()));

	UnparseGroup(pnRoot, pTable, aParent);	
}

//----------------------------------------------------------------------------------------------
void CXMLDatabaseObjectsParser::UnparseGroup(CXMLNode* pnRoot, DatabaseObjectsTable* pTable, const CTBNamespace& aParent)
{
	if (!pnRoot || !pTable)
		return;

	// elimina gli elementi nella mappa
	POSITION	pos;
	CString		strKey;
	CDbObjectDescription*	pDBObjectDescri;
	CXMLNode* pTablesNode = NULL;
	CXMLNode* pViewsNode = NULL;
	CXMLNode* pProcsNode = NULL;
	CXMLNode* pVirtualTablesNode = NULL;
	CXMLNode* pDBObjectNode = NULL;

	for (pos = pTable->m_DbObjects.GetStartPosition(); pos != NULL;)
	{
		pTable->m_DbObjects.GetNextAssoc(pos, strKey, (CObject*&)pDBObjectDescri);
		if (pDBObjectDescri && pDBObjectDescri->GetOwner() == aParent)
		{
			switch (pDBObjectDescri->GetType())
			{
				case CTBNamespace::TABLE:
				{
					if (!pTablesNode)
						pTablesNode = pnRoot->CreateNewChild(XML_TABLES_TAG);
					pDBObjectNode = pTablesNode->CreateNewChild(XML_TABLE_TAG);
					break;
				}
				case CTBNamespace::VIEW:
				{

					if (!pViewsNode)
						pViewsNode = pnRoot->CreateNewChild(XML_VIEWS_TAG);
					pDBObjectNode = pViewsNode->CreateNewChild(XML_VIEW_TAG);
					break;
				}
				case CTBNamespace::PROCEDURE:
				{
					if (!pProcsNode)
						pProcsNode = pnRoot->CreateNewChild(XML_PROCEDURES_TAG);
					pDBObjectNode = pProcsNode->CreateNewChild(XML_PROCEDURE_TAG);
					break;
				}				
			}
			if (pDBObjectNode)
				UnparseDbObject(pDBObjectNode, pDBObjectDescri, aParent);
		}
	}
}

//----------------------------------------------------------------------------------------------
void CXMLDatabaseObjectsParser::UnparseDbObject(
	CXMLNode* pNode,
	CDbObjectDescription* pDBObjectDescri,
	const CTBNamespace& aModuleNS
)
{

	if (!pNode || !pDBObjectDescri)
		return;

	pNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pDBObjectDescri->GetNamespace().ToUnparsedString());

	//attributo mastertable
	pNode->SetAttribute(XML_MASTERTABLE_ATTRIBUTE, FormatBoolForXML(pDBObjectDescri->IsMasterTable(), TRUE));
	
	pNode->SetAttribute(XML_LOCALIZE_ATTRIBUTE, pDBObjectDescri->GetNotLocalizedTitle());
	if (pDBObjectDescri->GetDeclarationType() == CDbObjectDescription::Dynamic || pDBObjectDescri->GetNamespace().HasAFakeLibrary())
		pNode->SetAttribute(szXmlDynamic, FormatBoolForXML(TRUE, TRUE));

	if (!pDBObjectDescri->GetTemplateNamespace().IsEmpty())
		pNode->SetAttribute(szTemplateNs, pDBObjectDescri->GetTemplateNamespace().ToUnparsedString());
	
	CXMLNode* pCreateNode = pNode->CreateNewChild(XML_CREATE_TAG);
	pCreateNode->SetAttribute(XML_RELEASE_ATTRIBUTE, FormatIntForXML(pDBObjectDescri->GetCreationRelease()));
	pCreateNode->SetAttribute(XML_CREATESTEP_ATTRIBUTE, FormatIntForXML(pDBObjectDescri->GetCreationStep()));

	CXMLNode* pFieldsNode;
	switch (pDBObjectDescri->GetType())
	{
		case CTBNamespace::TABLE:
		case CTBNamespace::VIEW:
			pFieldsNode = pNode->CreateNewChild(szColumns); break;
		case CTBNamespace::PROCEDURE:
			pFieldsNode = pNode->CreateNewChild(szParameters); break;	
	}
	if (pFieldsNode)
		UnparseFields(pFieldsNode, pDBObjectDescri, aModuleNS);
}

//----------------------------------------------------------------------------------------------
void CXMLDatabaseObjectsParser::UnparseFields(CXMLNode* pNode, CDbObjectDescription* pDBObjectDescri, const CTBNamespace& nsOwnerModule)
{
	CString aType;
	CString sName;
	CString sTemp;
	CDbFieldDescription* pFieldDescri = NULL;
	CXMLNode* pFieldNode = NULL;
	for (int i = 0; i < pDBObjectDescri->GetDynamicFields().GetCount(); i++)
	{
		// child nodes
		
		pFieldDescri = (CDbFieldDescription*)pDBObjectDescri->GetDynamicFields().GetAt(i);

		
		if (!pFieldDescri)// || pFieldDescri->GetOwnerModule() != nsOwnerModule)
			continue;

		if (
			pFieldDescri->GetColType() != CDbFieldDescription::Column &&
			pFieldDescri->GetDataType() == DataType::String &&
			pFieldDescri->GetLength() <= 0
			)
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Variable {0-%s} declared into record {1-%s}\r\n has no length declared into {2-%s}!\r\nVariable ignored!"), pFieldDescri->GetName(), pDBObjectDescri->GetName(), pNode->GetXMLDocument()->GetFileName()));
			continue;
		}		

		switch (pFieldDescri->GetColType())
		{
			case CDbFieldDescription::Column:
				pFieldNode = pNode->CreateNewChild(szColumn); break;
			case CDbFieldDescription::Parameter:
				pFieldNode = pNode->CreateNewChild(szParameter); break;
		}
		pFieldDescri->Unparse(pFieldNode, TRUE);

		/*pFieldNode->SetAttribute(XML_NAME_ATTRIBUTE, pFieldDescri->GetName());
		pFieldNode->SetAttribute(XML_TYPE_ATTRIBUTE, pFieldDescri->ToString(pFieldDescri->GetDataType()));
	*/	if (pFieldDescri->GetLength() > 0)
			pFieldNode->SetAttribute(XML_LENGTH_ATTRIBUTE, FormatIntForXML(pFieldDescri->GetLength()));
		//pFieldNode->SetAttribute(XML_DEFAULT_VALUE_ATTRIBUTE, pFieldDescri->GetValue()->FormatDataForXML());

		//release e contesto ??/ @@BAUZI è necessario
		if (pFieldDescri->IsSegmentKey())
			pFieldNode->SetAttribute(szSegmentKey, FormatBoolForXML(TRUE, TRUE));
		if (pFieldDescri->GetCreationRelease() > 1)
			pFieldNode->SetAttribute(XML_RELEASE_ATTRIBUTE, FormatIntForXML(pFieldDescri->GetCreationRelease()));
	}
}
//
////----------------------------------------------------------------
//BOOL CXMLDatabaseObjectsParser::UnparseDynamicData(
//	CDbObjectDescription* pDescri,
//	CXMLNodeChildsList* pChildNodes,
//	const CTBNamespace& moduleNamespace,
//	CAlterTableDescription* pAlterDescri)
//{
//	CString sTemp;
//
//	CDbObjectDescription* pTableDescri = (CDbObjectDescription*)pDescri;
//
//	CXMLNode* pChildNode;
//	for (int i = 0; i <= pChildNodes->GetUpperBound(); iv++)
//	{
//		pChildNode = pChildNodes->GetAt(i);
//		if (!pChildNode->GetName(sTemp))
//			continue;
//		if (_tcscmp(sTemp, XML_CREATE_TAG) == 0)
//		{
//			if (pChildNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp))
//				pTableDescri->SetCreationRelease(_ttoi(sTemp));
//		}
//		else if (_tcscmp(sTemp, szColumns) == 0 ||
//			_tcscmp(sTemp, XML_VARIABLES_TAG) == 0 ||
//			(pDescri->GetType() == CTBNamespace::PROCEDURE && _tcscmp(sTemp, szParameters) == 0)
//			)
//			ParseFields(pChildNode, sTemp, pTableDescri, moduleNamespace, pAlterDescri, FALSE);
//	}
//	return TRUE;
//}


