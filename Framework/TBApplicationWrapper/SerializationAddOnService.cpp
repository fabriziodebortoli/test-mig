#include "stdafx.h"
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include <TbGes\BODYEDIT.H>
#include <TbGes\XmlReferenceObjectsParserEx.h>
#include "SerializationAddOnService.h"
#include "EasyStudioTemplate.h"
#include "MDocument.h"


using namespace System;
using namespace System::IO;
using namespace System::Windows::Forms;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;

static const TCHAR szBak[] = _T("_bak");

//////////////////////////////////////////////////////////////
SerializationAddOnService::SerializationAddOnService() {}

//-------------------------------------------------------------------------------
System::String^ SerializationAddOnService::BuildPath(System::String^ nsDocument, System::String^ lastTokenNS)
{
	CTBNamespace aNs((CString)nsDocument);

	CString path = AfxGetPathFinder()->GetJsonFormPath(aNs, CPathFinder::CUSTOM, FALSE, lastTokenNS);
	CString bakPath = path + _T("_bak");

	//delete bakPath if exists
	if (ExistPath(bakPath))
	{
		CStringArray allFiles;
		GetFiles(bakPath, _T("*.*"), &allFiles);
		for (int i = allFiles.GetUpperBound(); i >= 0; i--)
			::RemoveFile(allFiles[i]);
		
		::RemoveDirectory(bakPath);
	}

	if (ExistPath(path))
		::RenameFilePath(path, bakPath);

	return gcnew System::String(AfxGetPathFinder()->GetJsonFormPath(aNs, CPathFinder::CUSTOM, TRUE, CString(lastTokenNS)));
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::GenerateJson(MView^ view, System::String^ nameSpace)
{
	if (view == nullptr || view->Handle == IntPtr::Zero)
		return false;

	CTBNamespace aNs(nameSpace);
	aNs.SetType(CTBNamespace::FORM);

	CTBNamespace aDocumentNs(CTBNamespace::DOCUMENT,
		aNs.GetApplicationName() +
		CTBNamespace::GetSeparator() +
		aNs.GetModuleName() +
		CTBNamespace::GetSeparator() +
		aNs.GetObjectName(CTBNamespace::LIBRARY) +
		CTBNamespace::GetSeparator() +
		aNs.GetObjectName(CTBNamespace::DOCUMENT)
	);

	System::String^ lastTokenNS = gcnew String(aNs.GetObjectName());
	System::String^ path = BuildPath(gcnew String(aDocumentNs.ToString()), lastTokenNS);
	view->SetPathToSerialize(path);
	view->GenerateJson(NULL, nullptr);

	return true;
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::SerializePublishedHotLinks(MDocument^ document, System::String^ currentApplication, String^ currentModule)
{
	CTBNamespace aModule(CTBNamespace::MODULE, CString(currentApplication) + CTBNamespace::GetSeparator() + CString(currentModule));
	String^ path = gcnew String(AfxGetPathFinder()->GetModulePath(aModule, CPathFinder::CUSTOM, FALSE, CPathFinder::EASYSTUDIO));

	// prima rimuove quelli da eliminare
	RemoveHotLinks(currentApplication, currentModule);

	bool result = false;
	for each (IComponent^ var in document->Components)
	{
		MHotLink^ hotlink = dynamic_cast<MHotLink^>(var);
		if (hotlink && hotlink->Published)
		{
			result = GenerateReferenceObject(hotlink, path);
			if (result)
				result = RefreshReferenceObject(path);
		}

	}
	return result;
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::GenerateReferenceObject(MHotLink^ hotlink, System::String^ partialPathApplicationModuleActiveNow)
{
	const int queryTypesNum = 4;
	HotKeyLinkObj::SelectionType selections[queryTypesNum] = { HotKeyLinkObj::SelectionType::DIRECT_ACCESS, HotKeyLinkObj::SelectionType::UPPER_BUTTON, HotKeyLinkObj::SelectionType::LOWER_BUTTON, HotKeyLinkObj::SelectionType::COMBO_ACCESS };
	CString queries[queryTypesNum] = { CString("Direct"),							  CString("Upper"),							  CString("Lower"),							 CString("Combo") };

	System::String^ hotlinkName = hotlink->Name;
	System::String^ tableName = hotlink->TableName;
	System::String^ hotlinkLocalizeByDesc = hotlink->Searches->ByDescription->Description;
	System::String^ hotlinkLocalizeByKey = hotlink->Searches->ByKey->Description;

	CXMLNode* pRoot;
	CXMLDocumentObject aDoc(TRUE);
	pRoot = aDoc.CreateRoot(XML_HOTKEYLINK_TAG);

	//aDoc.SetIndent(FALSE);
	PrepareInitialTags(pRoot, hotlink);
	CXMLNode* pSelectionModes = pRoot->CreateNewChild(XML_SELECTIONMODES_TAG);

	for (int i = 0; i < queryTypesNum; i++)
	{
		PrepareHotLink(hotlink, selections[i]);
		PrepareQueries(pSelectionModes, queries[i], hotlink);
	}


	CXMLNode* pSelectionTypes = pRoot->CreateNewChild(XML_SELECTIONTYPES_TAG);
	for (int i = 0; i < queryTypesNum; i++)
	{
		CXMLNode* pSelection = pSelectionTypes->CreateNewChild(XML_SELECTION_TAG);
		pSelection->SetAttribute(XML_NAME_ATTRIBUTE, queries[i]);
		pSelection->SetAttribute(XML_TYPE_ATTRIBUTE, queries[i]);
		if (queries[i] != _T("Lower"))
			pSelection->SetAttribute(XML_LOCALIZE_ATTRIBUTE, CString(hotlinkLocalizeByKey));
		else
			pSelection->SetAttribute(XML_LOCALIZE_ATTRIBUTE, CString(hotlinkLocalizeByDesc));
	}
	System::String^ temp = "";
	System::String^ path = Path::Combine(partialPathApplicationModuleActiveNow, "ReferenceObjects");
	if (!Directory::Exists(path))
		Directory::CreateDirectory(path);
	path = Path::Combine(path, hotlinkName + NameSolverStrings::XmlExtension);
	//if(Directory::Exists(temp))
	//TODOROBY  message exist already
	pathStr = path;
	return aDoc.SaveXMLFile(path, TRUE) == TRUE;

}
//----------------------------------------------------------------------------
void SerializationAddOnService::PrepareQueries(CXMLNode* pSelectionModes, CString queryType, MHotLink^ hotlink)
{

	array<System::String^>^ strFilterSplitted = strFilter->Split(' ');
	int strFilterSplittedCount = strFilterSplitted->Length;
	if (strFilterSplittedCount == 1) //ho una stringa vuota
		return;

	System::String^ tableName = hotlink->TableName;

	CXMLNode* pSelectionModesMode = pSelectionModes->CreateNewChild(XML_MODE_TAG);
	pSelectionModesMode->SetAttribute(XML_NAME_ATTRIBUTE, queryType);
	pSelectionModesMode->SetAttribute(XML_TYPE_ATTRIBUTE, _T("query"));

	int count = strFilter->Split('?')->Length - 1;

	int j = 0;
	for (int k = 0; k < count; k++)
	{
		System::String^ actual = strFilterSplitted[k];
		strFilterSplitted[k + 1] = _T(" LIKE { IN ");
		if (actual == hotlink->DBFieldName && queryType != _T("Lower"))
			strFilterSplitted[k + 2] = _T("hkl_Value }");
		else if (queryType == _T("Lower") && actual == hotlink->Searches->ByDescription->FieldName)
			strFilterSplitted[k + 2] = _T("hkl_Description }");
		else if (j < hotlink->Parameters->Count) {
			MHotLinkParam^ param = dynamic_cast<MHotLinkParam^>(hotlink->Parameters[j]);
			if (param)
				strFilterSplitted[k + 2] = param->Name->ToString() + "}";
			j++;
		}
		k = k + 3;
	}

	System::String^ prepare2 = "";
	for each (System::String^ var in strFilterSplitted)
		prepare2 += var + " ";

	System::String^ query = "SELECT { Columns " + tableName + "} FROM " + tableName;
	if (!String::IsNullOrEmpty(prepare2))
		query += " WHERE " + prepare2;

	if (!String::IsNullOrEmpty(strOrderBY))
		query += _T(" ORDER BY ") + strOrderBY;

	pSelectionModesMode->CreateCDATASection((LPCTSTR)CString(query));
}

//----------------------------------------------------------------------------
void SerializationAddOnService::PrepareInitialTags(CXMLNode* pRoot, MHotLink^ hotlink)
{
	System::String^ tableName = hotlink->TableName;
	System::String^ hotlinkDescription = hotlink->Description;
	System::String^ hotlinkNS = hotlink->PublicationNamespace;
	System::String^ hotlinkLinkedDocNS = hotlink->LinkedDocumentNamespace;
	System::String^ hotlinkFieldName = tableName + "." + hotlink->DBFieldName;
	System::String^ hotlinkFieldDescr = tableName + "." + hotlink->Searches->ByDescription->FieldName;
	LPCTSTR hotlinkAddOnFlyEnabled = hotlink->CanAddOnFly == true ? XML_TRUE_VALUE : XML_FALSE_VALUE;
	LPCTSTR hotlinkMustExistData = hotlink->DataMustExist == true ? XML_TRUE_VALUE : XML_FALSE_VALUE;
	LPCTSTR hotlinkBrowseEnabled = hotlink->CanOpenLinkedDocument == true ? XML_TRUE_VALUE : XML_FALSE_VALUE;

	CXMLNode* pFunction = pRoot->CreateNewChild(XML_FUNCTION_TAG);
	pFunction->SetAttribute(XML_NAMESPACE_ATTRIBUTE, (LPCTSTR)CString(hotlinkNS));
	pFunction->SetAttribute(XML_PUBLISHED_ATTRIBUTE, XML_TRUE_VALUE);
	pFunction->SetAttribute(XML_LOCALIZE_ATTRIBUTE, (LPCTSTR)CString(hotlinkDescription));
	pFunction->SetAttribute(XML_TYPE_ATTRIBUTE, CString(hotlink->ReturnType));

	for each (MHotLinkParam^ param in hotlink->Parameters)
	{
		CXMLNode* pParam = pFunction->CreateNewChild(XML_PARAMETER_TAG);
		pParam->SetAttribute(XML_NAME_ATTRIBUTE, (LPCTSTR)CString(param->Name));
		pParam->SetAttribute(XML_TYPE_ATTRIBUTE, (LPCTSTR)CString(param->Data->DataType.DataTypeToString()));
		pParam->SetAttribute(XML_LOCALIZE_ATTRIBUTE, (LPCTSTR)CString(param->Description));
		pParam->SetAttribute(XML_CAPTION_VALUE_ATTRIBUTE, (LPCTSTR)CString(param->Value->ToString()));
	}

	CXMLNode* pDbTableNode = pRoot->CreateNewChild(XML_DBTABLE_TAG);
	pDbTableNode->SetAttribute(XML_NAME_ATTRIBUTE, (LPCTSTR)CString(tableName));

	CXMLNode* pDbFieldNode = pRoot->CreateNewChild(XML_DBFIELD_TAG);
	pDbFieldNode->SetAttribute(XML_NAME_ATTRIBUTE, (LPCTSTR)CString(hotlinkFieldName));

	CXMLNode* pDbFieldDescr = pRoot->CreateNewChild(XML_DBFIELDDESCRIPTION_TAG);
	pDbFieldDescr->SetAttribute(XML_NAME_ATTRIBUTE, (LPCTSTR)CString(hotlinkFieldDescr));

	CXMLNode* pCallLink = pRoot->CreateNewChild(XML_CALLLINK_TAG);
	pCallLink->SetAttribute(XML_NAMESPACE_ATTRIBUTE, (LPCTSTR)CString(hotlinkLinkedDocNS));
	pCallLink->SetAttribute(XML_AddOnFlyEnabled_ATTRIBUTE, hotlinkAddOnFlyEnabled);
	pCallLink->SetAttribute(XML_MustExistData_ATTRIBUTE, hotlinkMustExistData);
	pCallLink->SetAttribute(XML_BrowseEnabled_ATTRIBUTE, hotlinkBrowseEnabled);
	return;
}

//----------------------------------------------------------------------------
void SerializationAddOnService::PrepareHotLink(MHotLink^ hotlink, HotKeyLinkObj::SelectionType nQuerySelection)
{
	DynamicHotKeyLink* dynHotlink = (DynamicHotKeyLink*)(hotlink->GetHotLink());
	SqlTable* table = dynHotlink->GetSqlTable();

	dynHotlink->DoDefineQuery(nQuerySelection);
	dynHotlink->DoPrepareQuery(dynHotlink->GetDataObj(), nQuerySelection);
	table->BuildSelect();
	strFilter = gcnew System::String(table->m_strFilter);	//where
	strOrderBY = gcnew System::String(table->m_strSort);	//order by
	table->Close();
	hotlink->GetHotLink()->CloseTable();
	return;
}

//----------------------------------------------------------------------------
bool SerializationAddOnService::RefreshReferenceObject(System::String^ partialPathApplicationModuleActiveNow)
{
	System::String^ dirApplic = Path::GetFileName(Path::GetDirectoryName(partialPathApplicationModuleActiveNow));
	System::String^ dirModule = Path::GetFileName(partialPathApplicationModuleActiveNow);
	CTBNamespace aModuleNS(CTBNamespace::MODULE, dirApplic + CTBNamespace::GetSeparator() + dirModule);
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aModuleNS);
	if (!pAddOnMod)
		return false;

	CXMLDocumentObject XMLDocument;
	XMLDocument.LoadXMLFile(CString(pathStr));
	CXMLReferenceObjectsParser parser;
	parser.Parse(&XMLDocument, (CReferenceObjectsDescription*)& pAddOnMod->m_XmlDescription.GetReferencesInfo(), pAddOnMod->GetNamespace());
	return true;
}

//----------------------------------------------------------------------------------------------
void SerializationAddOnService::RemoveHotLink(MHotLink^ hotlink)
{
	if (!hotlink->IsDynamic)
		return;
	if (hotLinksDeleted == nullptr)
		hotLinksDeleted = gcnew List<NameSpace^>();

	hotLinksDeleted->Add(gcnew NameSpace(hotlink->PublicationNamespace));
}

//----------------------------------------------------------------------------------------------
void SerializationAddOnService::RemoveHotLinks(System::String^ currentApplication, System::String^ currentModule)
{
	if (hotLinksDeleted == nullptr)
		return;

	for each (NameSpace^ ns in hotLinksDeleted)
		if (ns->Application == currentApplication && ns->Module == currentModule)
			RemoveHotLink(ns);

	hotLinksDeleted->Clear();
}

//----------------------------------------------------------------------------------------------
bool SerializationAddOnService::RemoveHotLink(NameSpace^ hotlinkNS)
{
	CTBNamespace aNamespace(hotlinkNS->FullNameSpace);
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNamespace);
	if (!pAddOnMod)
		return false;

	CString sPath = AfxGetPathFinder()->GetModuleReferenceObjectsPath(aNamespace, CPathFinder::CUSTOM, _T(""), FALSE, CPathFinder::EASYSTUDIO);
	String^ fileName = gcnew String(sPath + SLASH_CHAR + aNamespace.GetObjectName() + _T(".xml"));
	try
	{
		if (System::IO::File::Exists(fileName))
			System::IO::File::Delete(fileName);
	}
	catch (Exception^) {}
	finally
	{
		CReferenceObjectsDescription* pDescri = (CReferenceObjectsDescription*)& pAddOnMod->m_XmlDescription.GetReferencesInfo();
		pDescri->RemoveHotlink(aNamespace.ToString());
	}
	return true;
}


//----------------------------------------------------------------------------------------------
bool SerializationAddOnService::UpdateDatabaseObjects(MDocument^ document)
{
	if (!document || !document->Master)
		return false;
	MDBTMaster^ master = dynamic_cast<MDBTMaster^>(document->Master);
	if (!master)
		return false;
	const CDbObjectDescription* pXmlDescri = AfxGetDbObjectDescription(master->TableName);
	if (!pXmlDescri || pXmlDescri->IsMasterTable())	//Se risulta gi� master esci
		return true;

	const CTBNamespace& masterTableNamespace = pXmlDescri->GetNamespace();
	CString DBOpath = AfxGetPathFinder()->GetDatabaseObjectsFullName(masterTableNamespace, CPathFinder::PosType::STANDARD);

	CXMLDocumentObject XMLDocument;
	if (!ExistFile(DBOpath))
		return false;


	(const_cast<CDbObjectDescription*>(pXmlDescri))->SetMasterTable(TRUE);

	XMLDocument.LoadXMLFile(DBOpath);
	CXMLNodeChildsList* list = XMLDocument.GetNodeListByTagName(XML_TABLE_TAG);

	for (int i = 0; i < list->GetCount(); i++)
	{
		CXMLNode* oneNode = list->GetAt(i);
		if (oneNode)
		{
			CString ns;
			System::String^ masterTableNS = gcnew System::String(masterTableNamespace.ToString());
			oneNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, ns);
			if (masterTableNS->Contains(gcnew System::String(ns)))  //(table.erp.pippo) contains (erp.pippo)
				oneNode->SetAttribute(XML_MASTERTABLE_ATTRIBUTE, XML_TRUE_VALUE);
		}
	}

	FileInfo^ fInfo = gcnew FileInfo(gcnew System::String(DBOpath));
	bool isreadOnly = fInfo->IsReadOnly;
	fInfo->IsReadOnly = false;			// IsReadOnly false temporaneamente.

	if (!XMLDocument.SaveXMLFile(DBOpath))
		ASSERT(FALSE);

	fInfo->IsReadOnly = isreadOnly;		// IsReadOnly torna all'originale valore
	SAFE_DELETE(list);

	return true;
}


