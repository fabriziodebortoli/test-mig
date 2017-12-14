#pragma once
#include "MDocument.h"

#include "MDataObj.h"
#include "MSqlRecord.h"
#include "MFormatters.h"
#include "MDocument.h"
#include "MParsedControls.h"
#include "MPanel.h"
#include "MHotLink.h"


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


	System::String^ GenerateJsonFor(IWindowWrapper^ window, bool bInCustom);
	bool SerializePublishedHotLinks(MDocument^ document, System::String^ currentApplication, System::String^ currentModule);
	void RemoveHotLink(MHotLink^ hotlink);
	bool UpdateDatabaseObjects(MDocument^ document);

private:
	bool GenerateReferenceObject(MHotLink^ hotlink, System::String^ partialPathApplicationModuleActiveNow);
	bool RefreshReferenceObject(System::String^ partialPathApplicationModuleActiveNow);
	void PrepareHotLink(MHotLink^ hotlink, HotKeyLinkObj::SelectionType nQuerySelection);
	bool RemoveHotLink(NameSpace^ hotlinkNS);
	void RemoveHotLinks(System::String^ currentApplication, System::String^ currentModule);
	void PrepareInitialTags(CXMLNode* pRoot, MHotLink^ hotlink);
	void PrepareQueries(CXMLNode* pSelectionModes, CString queryType, MHotLink^ hotlink);

};