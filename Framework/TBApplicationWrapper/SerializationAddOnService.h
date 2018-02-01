#pragma once
#include "MDocument.h"

#include "MDataObj.h"
#include "MSqlRecord.h"
#include "MFormatters.h"
#include "MDocument.h"
#include "MParsedControls.h"
#include "MPanel.h"
#include "MHotLink.h"
#include "MView.h"
#include "MBodyEdit.h"

using namespace Microarea::TaskBuilderNet::Interfaces::View;

//=============================================================================
public ref class SerializationAddOnService
{
public:
	SerializationAddOnService();
	List<NameSpace^>^ hotLinksDeleted;
	System::String^	strFilter;
	System::String^	strOrderBY;
	System::String^ pathStr;

	static System::String^	staticAreaID = "IDC_STATIC_AREA";
	static System::String^	staticArea1ID = "IDC_STATIC_AREA_1";
	static System::String^	staticArea2ID = "IDC_STATIC_AREA_2";
	System::String^ path;

	bool GenerateJson(MView^ view, System::String^ nameSpace);
	bool SerializePublishedHotLinks(MDocument^ document, System::String^ currentApplication, System::String^ currentModule);
	void RemoveHotLink(MHotLink^ hotlink);
	bool UpdateDatabaseObjects(MDocument^ document);

private:
	// hotlink serialialization
	bool GenerateReferenceObject(MHotLink^ hotlink, System::String^ partialPathApplicationModuleActiveNow);
	bool RefreshReferenceObject(System::String^ partialPathApplicationModuleActiveNow);
	void PrepareHotLink(MHotLink^ hotlink, HotKeyLinkObj::SelectionType nQuerySelection);
	bool RemoveHotLink(NameSpace^ hotlinkNS);
	void RemoveHotLinks(System::String^ currentApplication, System::String^ currentModule);
	void PrepareInitialTags(CXMLNode* pRoot, MHotLink^ hotlink);
	void PrepareQueries(CXMLNode* pSelectionModes, CString queryType, MHotLink^ hotlink);

	/// json serialization
	bool SaveJson(System::String^ fileName, CWndObjDescription* pDescription);
	System::String^ BuildPath(/*IWindowWrapper^ window*/System::String^ nsDocument, System::String^ lastTokenNS);
	System::String^ GetJsonFileCompleteName(System::String^ path, System::String^ name);
	bool GenerateJsonFor(WindowWrapperContainer^ childContainer, CWndObjDescription* pParentDescription);
	CWndObjDescription*	GenerateDescription(IWindowWrapper^ container);
	static CWndObjDescription* UpdateAttributesFromType(CWndObjDescription* pWndObjDescription, BaseWindowWrapper^ wrapper);
	static void UpdateAttributesFromTileDialogType(CWndTileDescription*, MTileDialog^);
	static void UpdateAttributesFromTileGroupType(CWndLayoutContainerDescription*, MTileGroup^);
	static void UpdateAttributesFromParsedControl(CWndObjDescription*, MParsedControl^);
	void AddJsonDescriptionFor(IWindowWrapperContainer^ container, CWndObjDescription* pParentDescription);
	static CWndObjDescription*	CreateFromType(System::Type^ type);
	static void ManageDataBinding(CWndObjDescription* pWndObjDescription, MParsedControl^ parsedCtrl);
	static void ManageAnchor(CWndObjDescription*, BaseWindowWrapper^ wrapper);
};