#include "stdafx.h"
#include <TbNameSolver\LoginContext.h>
#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include "MTileGroup.h"
#include "MTileManager.h"
#include "MHeaderStrip.h"
#include "EasyStudioTemplate.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace System::IO;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;

#define szJsonTemplate	_T("template")
#define szJsonViewModel	_T("viewModel")
#define szJsonDataModel	_T("dataModel")

//////////////////////////////////////////////////////////////
//=============================================================================
class CTemplateDescription : public CObject, public IJsonObject
{
	friend ref class EasyStudioTemplate;

	CString	m_Description;
	// view model
	CWndObjDescription*	m_pWndDescription;
public:
	CTemplateDescription();
	~CTemplateDescription();

public:
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//----------------------------------------------------------------------------	
CTemplateDescription::CTemplateDescription()
	:
	m_pWndDescription(NULL)
{
}

//----------------------------------------------------------------------------	
CTemplateDescription::~CTemplateDescription()
{
	SAFE_DELETE(m_pWndDescription);
}

//----------------------------------------------------------------------------	
void CTemplateDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenObject(szJsonTemplate);
		strJson.WriteString(szJsonText, m_Description);
		strJson.OpenObject(szJsonDataModel);
		strJson.CloseObject();
		
		strJson.OpenObject(szJsonViewModel);
			if (m_pWndDescription)
				m_pWndDescription->SerializeJson(strJson);
		strJson.CloseObject();
	strJson.CloseObject();
}

//----------------------------------------------------------------------------	
void CTemplateDescription::ParseJson(CJsonFormParser& parser)
{
	parser.BeginReadObject(szJsonTemplate);
	m_Description = parser.ReadString(szJsonText);
	
	parser.BeginReadObject(szJsonDataModel);
	parser.EndReadObject();

	parser.BeginReadObject(szJsonViewModel);
	if (m_pWndDescription)
		SAFE_DELETE(m_pWndDescription);
	m_pWndDescription = CWndObjDescription::ParseJsonObject(parser);
	parser.EndReadObject();
	parser.EndReadObject();
}


//////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
EasyStudioTemplateWndCreateInfo::EasyStudioTemplateWndCreateInfo()
{

}

//----------------------------------------------------------------------------	
IWindowWrapperContainer^ EasyStudioTemplateWndCreateInfo::Parent::get() 
{
	return parent;
} 

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateInfo::Parent::set(IWindowWrapperContainer^ value)
{
	parent = value;
} 

//----------------------------------------------------------------------------	
Point EasyStudioTemplateWndCreateInfo::ScreenLocation::get()
{
	return screenLocation;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateInfo::ScreenLocation::set(Point value)
{
	screenLocation = value;
}

//----------------------------------------------------------------------------	
System::Type^ EasyStudioTemplateWndCreateInfo::ControlType::get()
{
	return controlType;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateInfo::ControlType::set(System::Type^ value)
{
	controlType = value;
}

//----------------------------------------------------------------------------	
System::String^ EasyStudioTemplateWndCreateInfo::ControlClass::get()
{
	return controlClass;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateInfo::ControlClass::set(System::String^ value)
{
	controlClass = value;
}

//----------------------------------------------------------------------------	
System::String^ EasyStudioTemplateWndCreateInfo::Caption::get()
{
	return caption;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateInfo::Caption::set(System::String^ value)
{
	caption = value;
}

//////////////////////////////////////////////////////////////
EasyStudioTemplateWndCreateEventArgs::EasyStudioTemplateWndCreateEventArgs(EasyStudioTemplateWndCreateInfo^ info)
{
	this->info = info;
}

//----------------------------------------------------------------------------	
EasyStudioTemplateWndCreateInfo^ EasyStudioTemplateWndCreateEventArgs::Info::get()
{ 
	return info;
}

//----------------------------------------------------------------------------	
IWindowWrapper^	EasyStudioTemplateWndCreateEventArgs::Wrapper::get()
{
	return wrapper;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplateWndCreateEventArgs::Wrapper::set(IWindowWrapper^ value)
{
	wrapper = value;
}

//////////////////////////////////////////////////////////////
EasyStudioTemplate::EasyStudioTemplate(IWindowWrapperContainer^ container, System::String^ name)
	:
	m_pDescription(NULL)
{
	LoadFrom(container);
	this->name = name;
}

//----------------------------------------------------------------------------	
EasyStudioTemplate::EasyStudioTemplate()
	:
	m_pDescription(NULL)
{
}

//----------------------------------------------------------------------------	
EasyStudioTemplate::~EasyStudioTemplate()
{
	this->!EasyStudioTemplate();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------	
EasyStudioTemplate::!EasyStudioTemplate()
{
	SAFE_DELETE(m_pDescription);
}

//----------------------------------------------------------------------------	
System::String^	EasyStudioTemplate::Name::get() 
{
	return name;
}

//----------------------------------------------------------------------------	
System::String^	EasyStudioTemplate::FileName::get() 
{
	return fileName;
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::IsFromFile::get()
{
	return !String::IsNullOrEmpty(FileName);
}

//----------------------------------------------------------------------------	
System::Type^ EasyStudioTemplate::ViewModelRootType::get()
{
	return m_pDescription && m_pDescription->m_pWndDescription ?
		GetTypeFromWndType(m_pDescription->m_pWndDescription->m_Type) : 
		nullptr;
}


//----------------------------------------------------------------------------	
bool EasyStudioTemplate::IsValid::get()
{
	return m_pDescription != nullptr;
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::LoadFrom(IWindowWrapperContainer^ container)
{
	if (!m_pDescription)
		m_pDescription = new CTemplateDescription();

	CreateDescription(container);
	return IsValid;
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::LoadFrom(String^ fileName)
{
	if (m_pDescription)
		SAFE_DELETE(m_pDescription);

	CString sFileName(fileName);
	CLineFile file;

	try
	{
		if (!file.Open(CString(sFileName), CFile::modeRead | CFile::typeText))
			return false;

		CString jsonBuffer = file.ReadToEnd();

		CJsonFormParser parser;
		parser.ReadJsonFromString(jsonBuffer);
		m_pDescription = new CTemplateDescription();
		m_pDescription->ParseJson(parser);
		file.Close();
		this->name = System::IO::Path::GetFileNameWithoutExtension(fileName);
		this->fileName = fileName;
	}
	catch (Exception^)
	{
		return false;
	}
	return IsValid;
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::Save(System::String^ path)
{
	this->fileName = System::IO::Path::Combine(path, name + NameSolverStrings::JsonExtension);
	return Save(m_pDescription, CString(fileName));
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::Save(IJsonObject* pDescri, CString sfileName)
{
	CLineFile file;
	if (!file.Open(sfileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText))
		return false;
	CJsonSerializer ser;
	pDescri->SerializeJson(ser);
	file.WriteString(ser.GetJson());
	file.Close();
	return true;
}

//----------------------------------------------------------------------------	
bool EasyStudioTemplate::Rename (System::String^ oldName, System::String^ newName)
{
	try
	{
		String^ newFileName = Path::Combine(Path::GetDirectoryName(FileName), newName);
		newFileName = String::Concat(newFileName, Path::GetExtension(FileName));
		File::Copy(FileName, newFileName);
		if (File::Exists(newFileName))
			File::Delete(FileName);
		fileName = newFileName;
		name = newName;
	}
	catch (Exception^ ex)
	{
		MessageBox::Show(ex->Message);
		return false;
	}

	return true;
}

//----------------------------------------------------------------------------	
IWindowWrapper^ EasyStudioTemplate::ApplyViewModel(IWindowWrapperContainer^ container, System::Drawing::Point screenLocation)
{
	if (!m_pDescription || !m_pDescription->m_pWndDescription)
		return nullptr;

	return ApplyViewModel(m_pDescription->m_pWndDescription, container, screenLocation);
}

//----------------------------------------------------------------------------	
IWindowWrapper^ EasyStudioTemplate::ApplyViewModel(CWndObjDescription* pDescription, IWindowWrapperContainer^ container, System::Drawing::Point screenLocation)
{
	if (!pDescription)
		return nullptr;

	EasyStudioTemplateWndCreateInfo^ info = gcnew EasyStudioTemplateWndCreateInfo();
	info->Parent		= container;
	info->ControlType	= GetTypeFromWndType(pDescription->m_Type);
	info->ScreenLocation = screenLocation;
	if (!pDescription->m_strControlClass.IsEmpty())
		info->ControlClass = gcnew String(pDescription->m_strControlClass);
	if (!pDescription->m_strControlCaption.IsEmpty())
		info->Caption = gcnew String(pDescription->m_strControlCaption);

	EasyStudioTemplateWndCreateEventArgs^ eventArgs = gcnew EasyStudioTemplateWndCreateEventArgs(info);
	NeedCreateWindow(this, eventArgs);
	if (eventArgs->Wrapper == nullptr)
		return nullptr;

	ApplyAttributesTo(eventArgs->Wrapper, pDescription);
	
	WindowWrapperContainer^ meAsContainer = dynamic_cast<WindowWrapperContainer^>(eventArgs->Wrapper);
	if (meAsContainer != nullptr &&  meAsContainer->GetWnd() && pDescription->m_Children.GetCount() > 0)
	{
		for (int i = 0; i < pDescription->m_Children.GetSize(); i++)
		{
			CWndObjDescription* pChildDescription = pDescription->m_Children.GetAt(i);
			screenLocation = Point(meAsContainer->Rectangle.Left, meAsContainer->Rectangle.Top);
			ApplyViewModel(pChildDescription, meAsContainer, screenLocation);
		}
	}
	return eventArgs->Wrapper;
}

//----------------------------------------------------------------------------	
CWndObjDescription*	EasyStudioTemplate::CreateWndDescriptionFrom(IWindowWrapperContainer^ container)
{
	if (container == nullptr || container->Handle == IntPtr::Zero)
		return false;

	HWND hWnd = (HWND)(int)container->Handle.ToInt64();
	CWndObjDescription* pOriginalDescription = CWndObjDescription::GetFrom(hWnd);
	CWndObjDescription* pNewDescription = NULL;
	if (pOriginalDescription)
		pNewDescription = (CWndObjDescription*)pOriginalDescription->DeepClone();
	else
		pNewDescription = CreateFromType(container->GetType());

	pNewDescription->UpdateAttributes(CWnd::FromHandle(hWnd));
	pNewDescription->SetRuntimeState(CWndObjDescription::STATIC);
	AddJsonDescriptionFor(container, pNewDescription);
	return pNewDescription;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplate::CreateDescription(IWindowWrapperContainer^ container)
{
	if (!m_pDescription)
		return;

	if (m_pDescription->m_pWndDescription)
		SAFE_DELETE(m_pDescription->m_pWndDescription);

	m_pDescription->m_pWndDescription = CreateWndDescriptionFrom(container);
}

//----------------------------------------------------------------------------	
CWndObjDescription*	EasyStudioTemplate::CreateFromType(Type^ type)
{
	CWndObjDescription* pDescri = NULL;
	if (type == MTileGroup::typeid || type->IsSubclassOf(MTileGroup::typeid))
	{
		pDescri = new CGroupBoxDescription(NULL);
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
void EasyStudioTemplate::AddJsonDescriptionFor(IWindowWrapperContainer^ container, CWndObjDescription* pParentDescription)
{
	for each (IWindowWrapper^ wrapper in container->Components)
	{
		WindowWrapperContainer^ childContainer = dynamic_cast<WindowWrapperContainer^>(wrapper);
		if (childContainer != nullptr && childContainer->Handle != IntPtr::Zero)
		{
			CWndObjDescription* pNewParent = pParentDescription->AddChildWindow(childContainer->GetWnd(), CString(childContainer->Name));
			AddJsonDescriptionFor(childContainer, pNewParent);
		}
	}
}

//----------------------------------------------------------------------------	
System::Type^ EasyStudioTemplate::GetTypeFromWndType(int nWndType)
{
	switch (nWndType)
	{
	case CWndObjDescription::WndObjType::TileGroup:		return MTileGroup::typeid;
	case CWndObjDescription::WndObjType::Tabber:		return MTabber::typeid;
	case CWndObjDescription::WndObjType::Tab:			return MTab::typeid;
	case CWndObjDescription::WndObjType::TileManager:	return MTileManager::typeid;
	case CWndObjDescription::WndObjType::Tile:			return MTileDialog::typeid;
	case CWndObjDescription::WndObjType::HeaderStrip:	return MHeaderStrip::typeid;
	}
	return nullptr;
}

//----------------------------------------------------------------------------	
void EasyStudioTemplate::ApplyAttributesTo(IWindowWrapper^ wrapper, CWndObjDescription* pDescription)
{
	BaseWindowWrapper^ window = dynamic_cast<BaseWindowWrapper^>(wrapper);
	if (window == nullptr)
		return;
	window->Text = gcnew String(pDescription->m_strText);
	PropertyChangingNotifier::OnComponentPropertyChanged(window->Site, window, "Text", nullptr, window->Text);

	window->LocationLU = Point(pDescription->m_X, pDescription->m_Y);
	window->SizeLU = Size(pDescription->m_Width, pDescription->m_Height);
	window->TabStop = pDescription->m_bTabStop;
	
	MTileDialog^ tile = dynamic_cast<MTileDialog^>(window);
	if (tile != nullptr && pDescription->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		CWndTileDescription* pTileDescription = (CWndTileDescription*)pDescription;
		tile->Collapsible = pTileDescription->m_bIsCollapsible == TRUE;
		PropertyChangingNotifier::OnComponentPropertyChanged(tile->Site, tile, "Collapsible", nullptr, tile->Collapsible);
		switch (pTileDescription->m_nStyle)
		{
		case TILE_WIDE:
			tile->TileDialogType = ETileDialogSize::Wide;
			PropertyChangingNotifier::OnComponentPropertyChanged(tile->Site, tile, "Collapsible", nullptr, tile->Collapsible);
			break;
		case TILE_MINI:
			tile->TileDialogType = ETileDialogSize::Mini;
			break;
		case TILE_AUTOFILL:
			tile->TileDialogType = ETileDialogSize::AutoFill;
			break;
		default:
			break; // standard
		}
	}
}