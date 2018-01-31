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

//////////////////////////////////////////////////////////////
SerializationAddOnService::SerializationAddOnService() {}

//-------------------------------------------------------------------------------
System::String^ SerializationAddOnService::BuildPath(/*IWindowWrapper^ window*/System::String^ nsDocument, System::String^ lastTokenNS)
{
	//CTBNamespace aNs (CString(window->Namespace->FullNameSpace));
	CString sNs = CString(nsDocument);
	CTBNamespace aNs(sNs);

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

	//manage _bak
	if (ExistPath(path))
		::RenameFilePath(path, bakPath);

	return gcnew System::String(AfxGetPathFinder()->GetJsonFormPath(aNs, CPathFinder::CUSTOM, TRUE, CString(lastTokenNS)));
}

//------------------------------------------------------------------------------------------------------------------
System::String^ SerializationAddOnService::GetJsonFileCompleteName(System::String^ path, System::String^ name)
{
	return gcnew System::String(path + CString(SLASH_CHAR) + CString(name) + CString(_T(".tbjson")));
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::GenerateJson(MView^ view, /*System::String^ nsDocument,*/ System::String^ lastTokenNS)
{
	if (view == nullptr || view->Handle == IntPtr::Zero)
		return false;

	CString aNs = view->Document->Namespace->FullNameSpace;
	System::String^ nsDocument = gcnew String(aNs);
	path = BuildPath(/*view*/nsDocument, lastTokenNS);
	System::String^ fileName = GetJsonFileCompleteName(path, view->Id);

	HWND hWnd = (HWND)(int)view->Handle.ToInt64();
	CWndObjDescription* pOriginalDescription = CWndObjDescription::GetFrom(hWnd);
	CWndObjDescription* pNewDescription = NULL;
	// serializzazione della view
	if (view->HasCodeBehind)
		pNewDescription = (CWndObjDescription*)pOriginalDescription->DeepClone();
	else
		pNewDescription = CreateFromType(view->GetType());

	UpdateAttributesFromType(pNewDescription, view);
	pNewDescription->SetRuntimeState(CWndObjDescription::STATIC);

	AddJsonDescriptionFor(view, pNewDescription);

	bool bSave = SaveJson(fileName, pNewDescription);
	SAFE_DELETE(pNewDescription);
	return bSave;
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::SaveJson(System::String^ fileName, CWndObjDescription* pDescription)
{
	CLineFile file;
	CFileException fileExc;
	if (!file.Open(CString(fileName), CFile::modeCreate | CFile::modeWrite | CFile::typeText, &fileExc))
		return false;

	CJsonSerializer ser;
	pDescription->SerializeJson(ser);

	file.WriteString(ser.GetJson());
	file.Close();
	return true;
}

//----------------------------------------------------------------------------	
CWndObjDescription*	SerializationAddOnService::GenerateDescription(IWindowWrapper^ container)
{
	if (container == nullptr || container->Handle == IntPtr::Zero)
		return false;

	HWND hWnd = (HWND)(int)container->Handle.ToInt64();
	CWndObjDescription* pOriginalDescription = CWndObjDescription::GetFrom(hWnd);
	CWndObjDescription* pNewDescription = NULL;

	WindowWrapperContainer^ pContainer = dynamic_cast<WindowWrapperContainer^>(container);
	if (!pContainer)
		return NULL;

	if (pContainer->HasCodeBehind)
		pNewDescription = (CWndObjDescription*)pOriginalDescription->DeepClone();
	else
		pNewDescription = CreateFromType(container->GetType());

	UpdateAttributesFromType(pNewDescription, pContainer);
	pNewDescription->SetRuntimeState(CWndObjDescription::STATIC);

	AddJsonDescriptionFor(pContainer, pNewDescription);
	return pNewDescription;
}

//----------------------------------------------------------------------------	
void SerializationAddOnService::AddJsonDescriptionFor(IWindowWrapperContainer^ container, CWndObjDescription* pParentDescription)
{
	for each (IWindowWrapper^ wrapper in container->Components)
	{
		WindowWrapperContainer^ childContainer = dynamic_cast<WindowWrapperContainer^>(wrapper);
		if (childContainer != nullptr && childContainer->Handle != IntPtr::Zero)
		{
			if (childContainer->GetType() == MTileDialog::typeid || childContainer->GetType()->IsSubclassOf(MTileDialog::typeid))
				GenerateJsonFor(childContainer, pParentDescription);
			else
			{
				CWndObjDescription* pNewParent = pParentDescription->AddChildWindow(childContainer->GetWnd(), CString(childContainer->Name));
				UpdateAttributesFromType(pNewParent, childContainer);
				AddJsonDescriptionFor(childContainer, pNewParent);
			}
		}
	}
}

//----------------------------------------------------------------------------	
bool SerializationAddOnService::GenerateJsonFor(WindowWrapperContainer^ childContainer, CWndObjDescription* pParentDescription)
{
	CDummyDescription* pDummyDescription = new CDummyDescription();
	pDummyDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
	pParentDescription->m_Children.Add(pDummyDescription);
	pDummyDescription->m_arHrefHierarchy.Add(childContainer->Id);

	CWndObjDescription* pTileDescription = (CWndObjDescription*)(childContainer->GetWnd())->SendMessage(UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(pParentDescription->m_Children), NULL/*(LPARAM)((LPCTSTR)(childContainer->Id))*/);
	UpdateAttributesFromType(pTileDescription, childContainer);


	for each (IWindowWrapper^ wrapper in childContainer->Components)
	{
		BaseWindowWrapper^ child = dynamic_cast<BaseWindowWrapper^>(wrapper);
		if (child != nullptr && child->Handle != IntPtr::Zero)
		{
			//skip always StaticArea
			if (child->Id->CompareTo(gcnew String(staticAreaID)) != 0 && child->Id->CompareTo(gcnew String(staticArea1ID)) != 0 && child->Id->CompareTo(gcnew String(staticArea2ID)) != 0)
			{
				CWndObjDescription* pLeafDescription = pTileDescription->AddChildWindow(child->GetWnd(), CString(child->Name));
				UpdateAttributesFromType(pLeafDescription, child);
			}
		}
	}

	//save tbjson file for entire tile
	System::String^ fileName = GetJsonFileCompleteName(path, childContainer->Id);
	bool bSave = SaveJson(fileName, pTileDescription);
	for (int i = pParentDescription->m_Children.GetUpperBound(); i >= 0; i--)
	{
		if (pParentDescription->m_Children.GetAt(i) == pTileDescription)
			pParentDescription->m_Children.RemoveAt(i);
	}
	SAFE_DELETE(pTileDescription);

	return bSave;
}

//----------------------------------------------------------------------------	
CWndObjDescription*	SerializationAddOnService::CreateFromType(System::Type^ type)
{
	CWndObjDescription* pDescri = NULL;
	if (type == MTileGroup::typeid || type->IsSubclassOf(MTileGroup::typeid))
	{
		pDescri = new CWndLayoutContainerDescription(NULL);
		pDescri->m_Type = CWndObjDescription::WndObjType::TileGroup;
	}
	else if (type == MTileManager::typeid || type->IsSubclassOf(MTileManager::typeid))
	{
		pDescri = new CTabberDescription(NULL);
		pDescri->m_Type = CWndObjDescription::WndObjType::TileManager;
	}
	else if (type == MTabber::typeid || type->IsSubclassOf(MTabber::typeid))
	{
		pDescri = new CTabberDescription(NULL);
		pDescri->m_Type = CWndObjDescription::WndObjType::Tabber;
	}
	else if (type == MTileDialog::typeid || type->IsSubclassOf(MTileDialog::typeid))
	{
		pDescri = new CWndTileDescription(NULL);
	}

	else
		pDescri = new CWndObjDescription(NULL);
	return pDescri;
}


//----------------------------------------------------------------------------	
bool SerializationAddOnService::SerializePublishedHotLinks(MDocument^ document, System::String^ currentApplication, String^ currentModule)
{
	CTBNamespace aModule(CTBNamespace::MODULE, CString(currentApplication) + CTBNamespace::GetSeparator() + CString(currentModule));
	String^ path = gcnew String(AfxGetPathFinder()->GetModulePath(aModule, CPathFinder::CUSTOM, FALSE, CPathFinder::ALL_COMPANIES));

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

	CString sPath = AfxGetPathFinder()->GetModuleReferenceObjectsPath(aNamespace, CPathFinder::CUSTOM, _T(""), FALSE, CPathFinder::ALL_COMPANIES);
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
	if (!pXmlDescri || pXmlDescri->IsMasterTable())	//Se risulta già master esci
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

//-------------------------------------------------------------------------------------------------------------------------------------------
void SerializationAddOnService::UpdateAttributesFromTileDialogType(CWndTileDescription* pTileDescription, MTileDialog^ tile)
{
	if (!pTileDescription || tile == nullptr)
		return;

	HWND hWnd = (HWND)(int)tile->Handle.ToInt64();
	pTileDescription->UpdateAttributes(CWnd::FromHandle(hWnd));

	pTileDescription->m_X = 0;
	pTileDescription->m_Y = 0;
	pTileDescription->m_bHasStaticArea = (tile->m_pTileDialog->HasStaticArea() == TRUE);
	//pTileDescription->m_arHrefHierarchy.Add(tile->Id);
	switch (tile->TileDialogType)
	{
	case ETileDialogSize::AutoFill:
		pTileDescription->m_Size = TileDialogSize::TILE_AUTOFILL;
		break;
	case ETileDialogSize::Standard:
		pTileDescription->m_Size = TileDialogSize::TILE_STANDARD;
		pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Standard).Width;
		break;
	case ETileDialogSize::Wide:
		pTileDescription->m_Size = TileDialogSize::TILE_WIDE;
		pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Wide).Width;
		break;
	case ETileDialogSize::Mini:
		pTileDescription->m_Size = TileDialogSize::TILE_MINI;
		pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Mini).Width;
		break;
	}
}

//-------------------------------------------------------------------------------------------------------------------
void SerializationAddOnService::UpdateAttributesFromTileGroupType(CWndLayoutContainerDescription* pTileGroupDescription, MTileGroup^ tileGroup)
{
	if (!pTileGroupDescription || tileGroup == nullptr)
		return;

	pTileGroupDescription->m_Type = CWndObjDescription::WndObjType::TileGroup;
	pTileGroupDescription->m_LayoutType = CLayoutContainer::COLUMN;	//its own default
	if (tileGroup->TileGroup && tileGroup->TileGroup->GetLayoutContainer())
		pTileGroupDescription->m_LayoutType = tileGroup->TileGroup->GetLayoutContainer()->GetLayoutType();
	if (tileGroup->TileGroup && tileGroup->TileGroup->GetLayoutContainer())
		pTileGroupDescription->m_LayoutAlign =
		tileGroup->TileGroup->GetLayoutContainer()->GetLayoutAlign() == CLayoutContainer::LayoutAlign::NO_ALIGN ?
		CLayoutContainer::LayoutAlign::STRETCH : //its own default
		tileGroup->TileGroup->GetLayoutContainer()->GetLayoutAlign();
}

//----------------------------------------------------------------------------------------------------------------------------------------
void SerializationAddOnService::UpdateAttributesFromParsedControl(CWndObjDescription* pWndObjDescription, MParsedControl^ parsedCtrl)
{
	if (parsedCtrl == nullptr)
		return;

	pWndObjDescription->m_Width = ((BaseWindowWrapper^)parsedCtrl)->Size.Width;
	pWndObjDescription->m_Height = ((BaseWindowWrapper^)parsedCtrl)->Size.Height;
	pWndObjDescription->m_strControlCaption = parsedCtrl->Caption;
	pWndObjDescription->m_strIds.RemoveAll();
	pWndObjDescription->m_strIds.Add(parsedCtrl->Name);

	//manage anchor
	ManageAnchor(pWndObjDescription, parsedCtrl);

	//manage DataBindings and HotLinks
	ManageDataBinding(pWndObjDescription, parsedCtrl);
}

//----------------------------------------------------------------------------------------------------------------------------------------
CWndObjDescription* SerializationAddOnService::UpdateAttributesFromType(CWndObjDescription* pWndObjDescription, BaseWindowWrapper^ wrapper)
{
	if (!pWndObjDescription || wrapper == nullptr)
		return NULL;

	//initialize common default attributes
	pWndObjDescription->m_X = NULL_COORD;
	pWndObjDescription->m_Y = NULL_COORD;
	pWndObjDescription->m_Width = NULL_COORD;
	pWndObjDescription->m_Height = NULL_COORD;
	pWndObjDescription->m_strName = wrapper->Name;
	pWndObjDescription->m_strIds.Add(wrapper->Id);
	pWndObjDescription->m_strText = wrapper->Text;

	if (wrapper->GetType() == MTileManager::typeid || wrapper->GetType()->IsSubclassOf(MTileManager::typeid))
	{
		//TODO?? - some attributes
		return pWndObjDescription;
	}

	if (wrapper->GetType() == MView::typeid || wrapper->GetType()->IsSubclassOf(MView::typeid))
	{
		MView^ view = dynamic_cast<MView^>(wrapper);
		if (view == nullptr)
			return pWndObjDescription;

		pWndObjDescription->m_bChild = true;
		pWndObjDescription->m_Type = CWndObjDescription::WndObjType::View;
		return pWndObjDescription;
	}

	if (wrapper->GetType() == MTileGroup::typeid || wrapper->GetType()->IsSubclassOf(MTileGroup::typeid))
	{
		CWndLayoutContainerDescription* pTileGroupDescription = dynamic_cast<CWndLayoutContainerDescription*>(pWndObjDescription);
		MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(wrapper);
		UpdateAttributesFromTileGroupType(pTileGroupDescription, tileGroup);
		return pTileGroupDescription ? pTileGroupDescription : pWndObjDescription;
	}

	if (wrapper->GetType() == MTileDialog::typeid || wrapper->GetType()->IsSubclassOf(MTileDialog::typeid))
	{
		CWndTileDescription* pTileDescription = dynamic_cast<CWndTileDescription*>(pWndObjDescription);
		MTileDialog^ tile = dynamic_cast<MTileDialog^>(wrapper);
		//TODO - pay attention - namespace + ID or ID?
		//pWndObjDescription->m_arHrefHierarchy.Add(wrapper->Id);
		UpdateAttributesFromTileDialogType(pTileDescription, tile);

		return pTileDescription ? pTileDescription : pWndObjDescription;
	}

	if (wrapper->GetType() == MParsedControl::typeid || wrapper->GetType()->IsSubclassOf(MParsedControl::typeid))
	{
		MParsedControl^ parsedCtrl = dynamic_cast<MParsedControl^>(wrapper);
		UpdateAttributesFromParsedControl(pWndObjDescription, parsedCtrl);
		return pWndObjDescription;
	}

	if (wrapper->GetType() == MBodyEdit::typeid || wrapper->GetType()->IsSubclassOf(MBodyEdit::typeid))
	{
		MBodyEdit^ bodyEdit = dynamic_cast<MBodyEdit^>(wrapper);
		CWndBodyDescription* pBEDescription = dynamic_cast<CWndBodyDescription*>(pWndObjDescription);

		pBEDescription->m_Width = wrapper->Size.Width;
		pBEDescription->m_Height = wrapper->Size.Height;
		//manage anchor
		ManageAnchor(pBEDescription, bodyEdit);
		//TODO: clipChildren && ownerDraw
		//pBEDescription->Owne
		pBEDescription->m_bShowColumnHeaders = (Bool3)bodyEdit->ShowColumnHeaders;
		pBEDescription->m_bShowHorizLines = (Bool3)bodyEdit->ShowHorizLines;
		pBEDescription->m_bShowVertLines = (Bool3)bodyEdit->ShowVertLines;
		pBEDescription->m_bShowHeaderToolbar = (Bool3)bodyEdit->ShowHeaderToolbar;
		pBEDescription->m_bShowStatusBar = (Bool3)bodyEdit->ShowStatusBar;
		pBEDescription->m_bAllowInsert = (Bool3)bodyEdit->AllowInsert;
		pBEDescription->m_bAllowDelete = (Bool3)bodyEdit->AllowDelete;

		//manage data binding
		if (pBEDescription->m_pBindings)
		{
			//exists => clear
			BindingInfo* pBindings = pBEDescription->m_pBindings;
			delete pBindings;
			pBEDescription->m_pBindings = nullptr;
		}
		if (bodyEdit->DataBinding != nullptr)
		{
			//update databinding
			pBEDescription->m_pBindings = new BindingInfo();
			pBEDescription->m_pBindings->m_strDataSource = (NameSpace^)bodyEdit->DataBinding->Name;//   Parent->Namespace;
		}

		//manage columns
		for (int i = 0; i < pBEDescription->m_Children.GetCount(); i++)
		{
			CWndBodyColumnDescription* pColumnDescription = dynamic_cast<CWndBodyColumnDescription*>(pBEDescription->m_Children.GetAt(i));
			if (!pColumnDescription)
				continue;

			pColumnDescription->m_X = NULL_COORD;
			pColumnDescription->m_Y = NULL_COORD;
			pColumnDescription->m_Width = NULL_COORD;
			pColumnDescription->m_Height = NULL_COORD;

			//binding
			if (pColumnDescription->m_pBindings)
			{
				//exists => clear
				if (pColumnDescription->m_pBindings->m_pHotLink)
				{
					delete pColumnDescription->m_pBindings->m_pHotLink;
					pColumnDescription->m_pBindings->m_pHotLink = NULL;
				}
				BindingInfo* pBindings = pColumnDescription->m_pBindings;
				delete pBindings;
				pColumnDescription->m_pBindings = NULL;
			}
			//manage column's binding
			for each (MBodyEditColumn^ column in bodyEdit->ColumnsCollection)
			{
				if (column->Name->CompareTo(gcnew String(pColumnDescription->m_strName)) == 0)
				{
					//manage binding
					if (column->DataBinding != nullptr)
					{
						pColumnDescription->m_pBindings = new BindingInfo();
						NameSpace^ parent = (NameSpace^)column->DataBinding->Parent->Namespace;
						pColumnDescription->m_pBindings->m_strDataSource = CString(parent->Leaf) + _T(".") + CString(column->DataBinding->Name);
						if (column->HotLink != nullptr)
						{
							pColumnDescription->m_pBindings->m_pHotLink = new HotLinkInfo();
							pColumnDescription->m_pBindings->m_pHotLink->m_strName = CString(column->HotLink->Name);
							pColumnDescription->m_pBindings->m_pHotLink->m_strNamespace = CString(column->HotLink->Namespace->FullNameSpace);
							pColumnDescription->m_pBindings->m_pHotLink->m_bEnableAddOnFly = (Bool3)column->HotLink->CanAddOnFly;
							pColumnDescription->m_pBindings->m_pHotLink->m_bMustExistData = (Bool3)column->HotLink->DataMustExist;
						}
					}
					//grayed
					pColumnDescription->m_bStatusGrayed = column->IsGrayed;
					//hidden
					pColumnDescription->m_bStatusHidden = column->IsHidden;
					break;
				}
			}
		}

		return pBEDescription;
	}

	return pWndObjDescription;
}

//--------------------------------------------------------------------------------------------------------------------------------------
void SerializationAddOnService::ManageAnchor(CWndObjDescription* pWndObjDescription, BaseWindowWrapper^ wrapper)
{
	//TODO manage: when hasCodeBehind manage control's moving
	if (!pWndObjDescription || wrapper == nullptr)
		return;

	if (wrapper->HasCodeBehind)
	{
		return;		//TODO - only if control is moved - calculate up side brother
	}

	//calculate possible left side brother (not depending on HasCodeBehind)
	WindowWrapperContainer^ container = dynamic_cast<WindowWrapperContainer^>(wrapper->Parent);
	bool bFoundBrotherAnchor = false;
	if (container != nullptr)
	{
		CWnd* pMeWnd = wrapper->GetWnd();
		CRect aMeRect, aBrotherRect;
		pMeWnd->GetWindowRect(&aMeRect);

		for each (BaseWindowWrapper^ brother in container->Components)
		{
			//skip static area
			if (brother == nullptr || brother == wrapper || brother->Id->CompareTo(gcnew String(staticAreaID)) == 0 || brother->Id->CompareTo(gcnew String(staticArea1ID)) == 0 || brother->Id->CompareTo(gcnew String(staticArea2ID)) == 0)
				continue;

			CWnd* pBrotherWnd = brother->GetWnd();
			if (!pBrotherWnd)
				continue;

			pBrotherWnd->GetWindowRect(aBrotherRect);
			if (aBrotherRect.left + aBrotherRect.Width() <= aMeRect.left && !bFoundBrotherAnchor)
			{
				pWndObjDescription->m_sAnchor = brother->Id;
				bFoundBrotherAnchor = true;
				break;
			}
		}
	}

	if (!bFoundBrotherAnchor)
		pWndObjDescription->m_sAnchor = wrapper->PartAnchor.Y == 0 ? _T("COL1") : _T("COL2");
}

//------------------------------------------------------------------------------------------------------------------------------------
void SerializationAddOnService::ManageDataBinding(CWndObjDescription* pWndObjDescription, MParsedControl^ parsedCtrl)
{
	if (!pWndObjDescription || parsedCtrl == nullptr)
		return;

	if (pWndObjDescription->m_pBindings)
	{
		//exists => clear
		BindingInfo* pBindings = pWndObjDescription->m_pBindings;
		if (pWndObjDescription->m_pBindings->m_pHotLink)
		{
			delete pWndObjDescription->m_pBindings->m_pHotLink;
			pWndObjDescription->m_pBindings->m_pHotLink = NULL;
		}
		delete pBindings;
		pWndObjDescription->m_pBindings = NULL;
	}
	if (parsedCtrl->DataBinding != nullptr)
	{
		//update databinding
		pWndObjDescription->m_pBindings = new BindingInfo();
		NameSpace^ parent = (NameSpace^)parsedCtrl->DataBinding->Parent->Namespace;
		pWndObjDescription->m_pBindings->m_strDataSource = CString(parent->Leaf) + _T(".") + CString(parsedCtrl->DataBinding->Name);
		if (parsedCtrl->HotLink != nullptr)
		{
			pWndObjDescription->m_pBindings->m_pHotLink = new HotLinkInfo();
			pWndObjDescription->m_pBindings->m_pHotLink->m_strName = CString(parsedCtrl->HotLink->Name);
			pWndObjDescription->m_pBindings->m_pHotLink->m_strNamespace = CString(parsedCtrl->HotLink->Namespace->FullNameSpace);
			pWndObjDescription->m_pBindings->m_pHotLink->m_bMustExistData = (Bool3)parsedCtrl->HotLink->DataMustExist;
			pWndObjDescription->m_pBindings->m_pHotLink->m_bEnableAddOnFly = (Bool3)parsedCtrl->HotLink->CanAddOnFly;
		}
	}
}



