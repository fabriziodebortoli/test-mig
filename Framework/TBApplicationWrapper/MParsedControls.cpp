// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"

#include <TbNameSolver\TBResourcesMap.h>
#include <TbOledb\SqlRec.h>
#include <TBGenlib\Hyperlink.h>
#include <TBGenlib\TileButtons.h>
#include <TBGenlib\TileDialogPart.h>
#include <TbGes\dbt.h>
#include <TbGes\Tabber.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>

#include "MBodyedit.h"
#include "MView.h"
#include "GenericControls.h"
#include "MParsedControlsExtenders.h"
#include "MDocument.h"
#include "MPanel.h"
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder::Refactor;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System::Drawing::Drawing2D;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System;
using namespace System::Text::RegularExpressions;
using namespace System::Globalization;
using namespace System::CodeDom;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design::Serialization;

typedef AstExpression ICSharpCode::NRefactory::CSharp::Expression;

//----------------------------------------------------------------------------
void ModifyStyle(HWND hWnd, int nStyleOffset, DWORD dwRemove, DWORD dwAdd)
{
	ASSERT(hWnd != NULL);
	DWORD dwStyle = ::GetWindowLong(hWnd, nStyleOffset);
	DWORD dwNewStyle = (dwStyle & ~dwRemove) | dwAdd;
	if (dwStyle == dwNewStyle)
		return;

	::SetWindowLong(hWnd, nStyleOffset, dwNewStyle);
}
//----------------------------------------------------------------------------
HWND GetChildWindow(HWND hParent, Point clientPosition)
{
	CWnd* pWnd = CWnd::FromHandle(hParent);
	if (!pWnd)
		return NULL;

	CWnd* pwndChild = pWnd->GetWindow(GW_CHILD);
	CRect r;
	while (pwndChild)
	{
		pwndChild->GetWindowRect(r);
		pWnd->ScreenToClient(r);
		if (r.left == clientPosition.X && r.top == clientPosition.Y)
			return pwndChild->m_hWnd;
		pwndChild = pwndChild->GetNextWindow();
	}
	return NULL;
}

//----------------------------------------------------------------------------
HWND GetChildWindow(HWND hParent, CString& strControlClass, Point clientPosition, CMap<CString, LPCTSTR, HWND, HWND>& hWNDPositionsMap)
{
	CString sKey;
	sKey.Format(_T("%d.%d.%s"), clientPosition.X, clientPosition.Y, strControlClass);
	HWND hwnd;
	if (!hWNDPositionsMap.Lookup(sKey, hwnd) || !IsWindow(hwnd))
	{
		hwnd = GetChildWindow(hParent, clientPosition);
		if (hwnd)
			hWNDPositionsMap[sKey] = hwnd;
	}
	return hwnd;
}

//----------------------------------------------------------------------------
void SaveChildWindowPos(HWND hwndChild, Point clientPosition, CMap<CString, LPCTSTR, HWND, HWND>& hWNDPositionsMap)
{
	if (!hwndChild)
		return;

	HWND hwnd;
	CString sKey;
	POSITION pos = hWNDPositionsMap.GetStartPosition();
	while (pos)
	{
		hWNDPositionsMap.GetNextAssoc(pos, sKey, hwnd);
		if (hwnd == hwndChild)
			return;
	}
	TCHAR szClassName[MAX_CLASS_NAME + 1];
	::GetClassName(hwndChild, szClassName, MAX_CLASS_NAME);
	sKey.Format(_T("%d.%d.%s"), clientPosition.X, clientPosition.Y, szClassName);
	hWNDPositionsMap[sKey] = hwnd;
}

/////////////////////////////////////////////////////////////////////////////
// 				class WindowWrapperNativeWindow Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
ref class WindowWrapperNativeWindow : NativeWindow
{
	BaseWindowWrapper^ m_Owner;
public:
	WindowWrapperNativeWindow(BaseWindowWrapper^ owner)
		: m_Owner(owner)
	{}

protected:

	virtual	void WndProc(Message% m) override
	{
		if (!m_Owner->WndProc(m))
		{
			__super::WndProc(m);
			m_Owner->AfterWndProc(m);
		}
	}
};

/////////////////////////////////////////////////////////////////////////////
// 				class ControlClass Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
ControlClass::ControlClass(BaseWindowWrapper^ control)
	:
	m_pRegInfo(NULL)
{
	this->control = control;
}

//----------------------------------------------------------------------------
ControlClass::ControlClass(CRegisteredParsedCtrl* pRegInfo)
	:
	m_pRegInfo(pRegInfo)
{
	this->control = nullptr;
}

//----------------------------------------------------------------------------
ControlClass::~ControlClass()
{
	this->!ControlClass();
}

//----------------------------------------------------------------------------
ControlClass::!ControlClass()
{
	m_pRegInfo = NULL;
}

//----------------------------------------------------------------------------
CRegisteredParsedCtrl* ControlClass::GetRegInfoPtr()
{
	return m_pRegInfo;
}
//----------------------------------------------------------------------------
void ControlClass::SetRegInfoPtr(CRegisteredParsedCtrl* ptr)
{
	m_pRegInfo = ptr;
}

//----------------------------------------------------------------------------
System::String^ ControlClass::ClassName::get()
{
	if (!m_pRegInfo)
	{
		if (control == nullptr || !control->GetWnd())
		{
			ASSERT(FALSE);
			return System::String::Empty;
		}

		m_pRegInfo = AfxGetParsedControlsRegistry()->GetRegisteredControl(control->GetWnd());
	}

	return gcnew System::String(m_pRegInfo ? m_pRegInfo->GetName() : _TB("Unregistered Window"));
}

//----------------------------------------------------------------------------
System::String^ ControlClass::ClassDescription::get()
{
	if (!m_pRegInfo)
	{
		if (control == nullptr || !control->GetWnd())
		{
			ASSERT(FALSE);
			return System::String::Empty;
		}

		m_pRegInfo = AfxGetParsedControlsRegistry()->GetRegisteredControl(control->GetWnd());
	}

	return gcnew System::String(m_pRegInfo ? m_pRegInfo->GetLocalizedText() : _TB("Unregistered Window"));
}

//----------------------------------------------------------------------------
System::String^ ControlClass::CompatibleTypeName::get()
{
	IDataType^ type = CompatibleType;
	return type == nullptr ? "-" : type->ToString();
}

//----------------------------------------------------------------------------
IDataType^ ControlClass::CompatibleType::get()
{
	if (control == nullptr && !m_pRegInfo)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	if (control != nullptr &&
		(control->GetType() == MBodyEdit::typeid || control->GetType()->IsSubclassOf(MBodyEdit::typeid)))
		return Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Null;

	CParsedCtrl* pCtrl = control != nullptr ? ::GetParsedCtrl(control->GetWnd()) : NULL;
	if (pCtrl)
		return gcnew Microarea::TaskBuilderNet::Core::CoreTypes::DataType
		(
			pCtrl->GetDataType().m_wType,
			pCtrl->GetDataType().m_wTag
		);
	else if (m_pRegInfo)
		return gcnew Microarea::TaskBuilderNet::Core::CoreTypes::DataType
		(
			m_pRegInfo->GetDataType().m_wType,
			m_pRegInfo->GetDataType().m_wTag
		);

	return Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Null;
}

//----------------------------------------------------------------------------
System::String^ ControlClass::FamilyName::get()
{
	if (control == nullptr && !m_pRegInfo)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	if (m_pRegInfo)
		return gcnew System::String(m_pRegInfo->GetFamily()->GetName());

	return control->GetType()->Name;
}

//----------------------------------------------------------------------------
System::String^ ControlClass::ToString()
{
	return ClassDescription;
}

/////////////////////////////////////////////////////////////////////////////
// 				class EasyBuilderControlSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ EasyBuilderControlSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	System::Collections::Generic::IList<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	EasyBuilderControl^ ebControl = (EasyBuilderControl^)current;

	if (!IsSerializable(ebControl))
		return newCollection;

	////Aggiunge le righe di commento per la sepazione tra un oggetto e l'altro
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", ebControl->SerializedName)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));

	// constructor
	newCollection->Add(GetConstructor(manager, ebControl));

	// dataBinding x primo
	System::Collections::Generic::IList<Statement^>^ dbCollection = SerializeDataBinding(ebControl, ebControl->SerializedName);
	if (dbCollection != nullptr)
	{
		for each (Statement^ item in dbCollection)
			newCollection->Add(item);
	}

	// properties
	IdentifierExpression^ expr = gcnew IdentifierExpression(ebControl->SerializedName);
	SetExpression(manager, ebControl, expr, true);

	//lascio scegliere alla classe derivata se serializzare prima l'istruzione di Add o prima l'assegnazione delle proprietà
	//ad es. il tabmanager deve prima aggiungere la tab, poi impostarne le proprietà altrimenti non imposta correttamente la Enabled
	SerializePropertiesAndAddMethod(manager, ebControl, newCollection);

	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, ebControl, ebControl->SerializedName);

	//Serializzazione registrazione event handlers.
	if (events != nullptr)
	{
		for each (Statement^ item in events)
			newCollection->Add(item);
	}

	System::Collections::Generic::IList<Statement^>^ references = SerializeReferences(ebControl, ebControl->SerializedName);

	//Serializzazione registrazione event handlers.
	if (references != nullptr)
	{
		for each (Statement^ item in references)
			newCollection->Add(item);
	}

	return newCollection;
}

//----------------------------------------------------------------------------------------------------------------
void EasyBuilderControl::GenerateJsonForEvents(List<System::Tuple<System::String^, System::String^>^>^ evSerialization)
{
	if (evSerialization == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	EasyBuilderControl^ ebControl = dynamic_cast<EasyBuilderControl^>(this);
	if (ebControl == nullptr)
		return;

	CJsonSerializer jsonSer;
	BOOL bFoundEvents = FALSE;
	int i = 0;
	System::Collections::Generic::IEnumerator<Microarea::TaskBuilderNet::Core::EasyBuilder::EventInfo^>^ evEnumerator = ebControl->ChangedEvents->GetEnumerator();
	while (evEnumerator->MoveNext())
	{
		if (!bFoundEvents)
		{
			jsonSer.OpenArray(CString(contentTag));
			bFoundEvents = TRUE;
		}
		jsonSer.OpenObject(i);
		Microarea::TaskBuilderNet::Core::EasyBuilder::EventInfo^ ev = evEnumerator->Current;
		jsonSer.WriteString(CString(namespaceTag), CString(this->Namespace->ToString()));
		jsonSer.WriteString(CString(eventTag), CString(ev->EventName));
		jsonSer.CloseObject();
		i++;
	}

	if (bFoundEvents)
	{
		jsonSer.CloseArray();
		evSerialization->Add
		(
			gcnew Tuple<System::String^, System::String^>
			(
				String::Concat(prefixEvent, this->Namespace),	
				gcnew String(jsonSer.GetJson())
			)
		);
	}
}

//----------------------------------------------------------------------------
INameSpace^ EasyBuilderControl::Namespace::get()
{
	return nullptr;
}

/// <summary>
/// Internal Use
/// </summary>
void EasyBuilderControlSerializer::SerializePropertiesAndAddMethod(
	IDesignerSerializationManager^ manager,
	EasyBuilderControl^ ebControl,
	System::Collections::Generic::IList<Statement^>^ collection
)
{
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, ebControl, ebControl->SerializedName);
	if (props != nullptr)
	{
		for each (Statement^ item in props)
			collection->Add(item);
	}

	collection->Add
	(
		AstFacilities::GetInvocationStatement
		(
			gcnew ThisReferenceExpression(),
			AddMethodName,
			gcnew IdentifierExpression(ebControl->SerializedName),
			gcnew PrimitiveExpression(ebControl->IsChanged)
		)
	);
}

//----------------------------------------------------------------------------	
bool EasyBuilderControlSerializer::IsSerializable(EasyBuilderComponent^ ebComponent)
{
	return __super::IsSerializable(ebComponent) || !ebComponent->HasCodeBehind;
}

//----------------------------------------------------------------------------	
Statement^ EasyBuilderControlSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	Point currentLocation = GetLocationToSerialize(ebControl);

	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	ObjectCreateExpression^ creationExpression =
		AstFacilities::GetObjectCreationExpression
		(
			gcnew SimpleType(ebControl->GetType()->ToString()),
			GetParentWindowReference(),
			gcnew PrimitiveExpression(ebControl->FullId),
			gcnew PrimitiveExpression(ebControl->ClassName),
			AstFacilities::GetObjectCreationExpression
			(
				System::Drawing::Point::typeid->FullName,
				gcnew PrimitiveExpression(currentLocation.X),
				gcnew PrimitiveExpression(currentLocation.Y)
			),
			gcnew PrimitiveExpression(ebControl->HasCodeBehind)
		);

	SetExpression(manager, ebControl, variableDeclExpression, true);

	return AstFacilities::GetAssignmentStatement
	(
		variableDeclExpression,
		creationExpression
	);
}

//----------------------------------------------------------------------------	
Point EasyBuilderControlSerializer::GetLocationToSerialize(EasyBuilderControl^ ebControl)
{
	// prima di tutto questa funzione elimina la eventuale parte di scrolling 
	Point currentLocation = ((BaseWindowWrapper^)ebControl)->Location;
	Point scrollOffset = ebControl->Parent == nullptr
		? Point::Empty
		: ((IWindowWrapperContainer^)ebControl->Parent)->GetScrollPosition();

	//depuro lo scrolling eventuale
	currentLocation.Offset(scrollOffset);
	return currentLocation;
}

//----------------------------------------------------------------------------	
AstExpression^	EasyBuilderControlSerializer::GetParentWindowReference()
{
	return gcnew ThisReferenceExpression();
}

/////////////////////////////////////////////////////////////////////////////
// 				class EasyBuilderControl Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
EasyBuilderControl::EasyBuilderControl()
	: visible(true)
{
}

//----------------------------------------------------------------------------
IDataType^ EasyBuilderControl::CompatibleType::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
bool EasyBuilderControl::Visible::get()
{
	return visible;
}

//----------------------------------------------------------------------------
void EasyBuilderControl::Visible::set(bool visible)
{
	this->visible = visible;
}


//----------------------------------------------------------------------------
System::String^ EasyBuilderControl::ClassName::get()
{
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^	EasyBuilderControl::SerializedName::get()
{
	return System::String::Concat("ctrl_", EasyBuilderSerializer::Escape(Name));
}
//----------------------------------------------------------------------------
System::String^	EasyBuilderControl::SerializedType::get()
{
	return GetType()->Name;
}

//---------------------------------------------------------------serialize-----------------
bool EasyBuilderControl::IsInListContainer(Object^ obj)
{
	if (obj == nullptr)
		return false;

	if (!IComponent::typeid->IsInstanceOfType(obj))
		return false;

	IComponent^ cmp = (IComponent^)obj;
	if (cmp->Site == nullptr)
		return false;

	IContainer^ cnt = cmp->Site->Container;
	//se appartengo ad un dbt slave buffered non posso essere droppato, a meno che non sia un sub dbt 
	if (MDBTSlaveBuffered::typeid->IsInstanceOfType(cnt) && !MDBTSlave::typeid->IsInstanceOfType(obj))
		return true;

	return IsInListContainer(cnt);
}
//----------------------------------------------------------------------------
bool EasyBuilderControl::CanDropData(IDataBinding^ dataBinding)
{
	return dataBinding != nullptr &&
		dataBinding->Data != nullptr &&
		!IsInListContainer(dataBinding->Data);
}

//----------------------------------------------------------------------------
bool EasyBuilderControl::TabStop::get()
{
	return false;
}

//----------------------------------------------------------------------------
void EasyBuilderControl::TabStop::set(bool value)
{
}

//----------------------------------------------------------------------------
void EasyBuilderControl::TabOrder::set(int value)
{
}

//----------------------------------------------------------------------------
int EasyBuilderControl::TabOrder::get()
{
	return -1;
}

////////////////////////////////////////////////////////////sca/////////////////
// 				class BaseWindowWrapper Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
BaseWindowWrapper::BaseWindowWrapper(System::IntPtr handleWndPtr)
{
	partAnchor = Point::Empty;
	nativeWindow = gcnew WindowWrapperNativeWindow(this);
	borderColor = Color::Empty;
	originalLocation = Location;
	Initialize();
	Handle = handleWndPtr;
	extensions = gcnew EasyBuilderComponentExtenders(this);
	extensions->Service->Refresh();
	this->IsStretchable = false;
}

//----------------------------------------------------------------------------
CString BaseWindowWrapper::FullIdToResourceKey(System::String^ fullId)
{
	System::String^ id = fullId->Replace(":", "");
	return CString(id);
}

//----------------------------------------------------------------------------
CBaseTileDialog* BaseWindowWrapper::GetParentTileDialog()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd || !pWnd->GetParent())
		return NULL;

	return dynamic_cast<CBaseTileDialog*>(pWnd->GetParent());
}

//----------------------------------------------------------------------------
BaseWindowWrapper::BaseWindowWrapper(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
{
	nativeWindow = gcnew WindowWrapperNativeWindow(this);
	borderColor = Color::Empty;
	this->parent = parentWindow;
	originalLocation = location;
	Initialize();

	this->name = name;
	this->HasCodeBehind = hasCodeBehind;

	this->IsStretchable = false;

	extensions = gcnew EasyBuilderComponentExtenders(this);
	extensions->Service->Refresh();

	if (parentWindow == nullptr)
		return;

	// lo cerco per namespace e ne estraggo l'handle
	CTBNamespace aFullNs(CString(parentWindow->Namespace->ToString()));
	aFullNs.SetChildNamespace((CTBNamespace::NSObjectType) GetNamespaceType(), CString(name), aFullNs);

	EasyBuilderComponent^ parentComponent = (EasyBuilderComponent^)parentWindow;

	if (parentComponent->DesignModeType != EDesignMode::Static)
	{
		RenameChangeRequest^ request = gcnew RenameChangeRequest(Refactor::ChangeSubject::Class, parentWindow->Namespace, parentComponent->Document->Namespace, String::Empty, gcnew String(aFullNs.ToString()), this->Version);
		String^ newName = BaseCustomizationContext::ApplicationChanges->GetNewNameOf(request);
		delete request;
		aFullNs.SetNamespace((CString)newName);
	}

	HWND handle = ((WindowWrapperContainer^)parentWindow)->GetControlHandle(aFullNs);
	if (!handle)
	{
		UINT nID = AfxGetTBResourcesMap()->GetExistingTbResourceID(FullIdToResourceKey(name), GetTbResourceType());
		if (nID != 0 && parentWindow->GetType()->IsSubclassOf(WindowWrapperContainer::typeid))
			handle = (HWND)(int)((WindowWrapperContainer^)parentWindow)->GetChildFromCtrlID(nID);
	}

	//Se è un control personalizzato lo creo sulla fiducia
	if (!this->HasCodeBehind)
	{
		if (handle != nullptr)
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Trying to create duplicate control! Control {0-%s} already exists !"), CString(name))));
		Create(parentWindow, location, className);
	}
	else
	{
		if (parentWindow->GetType() == MTabber::typeid || parentWindow->GetType()->IsSubclassOf(MTabber::typeid))
			return;

		ASSERT(handle);
		Handle = (System::IntPtr)(int)handle;
		this->Location = location;
	}

	// la tab dialog iniziale potrebbe non avere ancora l'handle 
	if (Handle == IntPtr::Zero && parentComponent != nullptr && parentWindow != nullptr && Version != nullptr && Version->Major < 2)
	{ 
		System::String^ message = gcnew System::String(
			"This customization is not compatibile with new document view model due to:" + System::Environment::NewLine +
			gcnew System::String(aFullNs.ToString()) + System::Environment::NewLine +
			"Parent Object Type : " + parentComponent->GetType()->ToString() + System::Environment::NewLine +
			"Parent Object Name : " + parentWindow->Namespace->ToString() + System::Environment::NewLine +
			"Document : " + parentComponent->Document->Namespace->ToString() + System::Environment::NewLine
		);
		throw gcnew ApplicationException(message);
	}
}

//-----------------------------------------------------------------------------
BaseWindowWrapper::~BaseWindowWrapper()
{
	this->!BaseWindowWrapper();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
BaseWindowWrapper::!BaseWindowWrapper()
{
	if (nativeWindow != nullptr)
		nativeWindow->ReleaseHandle();
	if (extensions)
	{
		delete extensions;
		extensions = nullptr;
	}

}


//----------------------------------------------------------------------------
void BaseWindowWrapper::UpdateViewOutlineOrder(BaseWindowWrapper^ targetNode)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}

	CWnd* pWndTarget = targetNode->GetWnd();
	if (!pWndTarget)
	{
		ASSERT(FALSE);
		return;
	}

	if (pWndTarget->m_hWnd != pWnd->m_hWnd)
	{
		pWnd->SetWindowPos(pWndTarget, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		return;
	}
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Activate()
{
	WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
	if (container != nullptr)
		container->Activate();
}

//-----------------------------------------------------------------------------
int	BaseWindowWrapper::GetNamespaceType()
{
	return CTBNamespace::CONTROL;
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::Site::set(ISite^ site)
{
	__super::Site = site;
	Extensions->Service->AdjustSites();
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::OnDesignerControlCreated()
{
	SizeLU = AdjustMinSizeOnParent(this, Parent);
}

//-------------------------------------------------------------------------------------
CString BaseWindowWrapper::GetHorizontalIdAnchor()
{
	CString myAnchor = _T("");

	if (this->HasCodeBehind)
	{
		//TODO - ancora niente
	}

	//calculate possible left side brother (not depending on HasCodeBehind)
	WindowWrapperContainer^ container = dynamic_cast<WindowWrapperContainer^>(this->Parent);
	bool bFoundBrotherAnchor = false;
	if (container != nullptr)
	{
		CWnd* pMeWnd = this->GetWnd();
		CRect aMeRect, aBrotherRect;
		pMeWnd->GetWindowRect(&aMeRect);

		for each (BaseWindowWrapper^ brother in container->Components)
		{
			//skip static area
			if (brother == nullptr || brother == this || brother->Id->CompareTo(gcnew String(staticAreaID)) == 0 || brother->Id->CompareTo(gcnew String(staticArea1ID)) == 0 || brother->Id->CompareTo(gcnew String(staticArea2ID)) == 0)
				continue;

			CWnd* pBrotherWnd = brother->GetWnd();
			if (pBrotherWnd == nullptr)
				continue;

			pBrotherWnd->GetWindowRect(aBrotherRect);
			if 
				(
					aBrotherRect.left + aBrotherRect.Width() <= aMeRect.left &&
					(
						aMeRect.top >= aBrotherRect.top && aMeRect.top <= aBrotherRect.top + aBrotherRect.Height() || 
						aMeRect.bottom >= aBrotherRect.top && aMeRect.bottom <= aBrotherRect.top + aBrotherRect.Height()
					) && 
					!bFoundBrotherAnchor
				)
			{
				myAnchor = brother->Id;
				bFoundBrotherAnchor = true;
				break;
			}
		}
	}

	if (!bFoundBrotherAnchor)
		myAnchor = this->PartAnchor.Y == 0 ? _T("COL1") : _T("COL2");

	return myAnchor;
}

//-------------------------------------------------------------------------------------------------
CString BaseWindowWrapper::GetSerialization(CWndObjDescription* pWndObjDescription)
{
	CJsonSerializer ser;
	pWndObjDescription->SerializeJson(ser);

	return ser.GetJson();
}

//----------------------------------------------------------------------------------
void BaseWindowWrapper::GenerateJson(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	UpdateAttributesForJson(pParentDescription);
	GenerateJsonForChildren(jsonDescription, serialization);
	GenerateSerialization(pParentDescription, serialization);
}

//------------------------------------------------------------------------------
void BaseWindowWrapper::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	//base implementation
	if (!jsonDescription)
		return;

	//initialize common default attributes
	jsonDescription->m_X = NULL_COORD;
	jsonDescription->m_Y = NULL_COORD;
	jsonDescription->m_Width = NULL_COORD;
	jsonDescription->m_Height = NULL_COORD;
	jsonDescription->m_strName = this->Name;
	jsonDescription->m_strIds.Add(this->Id);
	jsonDescription->m_strText = this->Text;
}

//-----------------------------------------------------------------------------------------
void BaseWindowWrapper::GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	//base class implementation empty
}

//---------------------------------------------------------------------------------------------
void BaseWindowWrapper::GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	//base class implementation
	//serialize anyway and always the events for this class
	GenerateJsonForEvents(serialization);
}

//-----------------------------------------------------------------------------
System::Drawing::Size	BaseWindowWrapper::AdjustMinSizeOnParent(BaseWindowWrapper^ control, IWindowWrapperContainer^ parentWindow)
{
	System::Drawing::Size parentSize = ((BaseWindowWrapper^)parentWindow)->SizeLU;
	System::Drawing::Size size = minSize;
	if (size.Width > parentSize.Width)
	{
		size.Width = parentSize.Width - control->Location.X;
	}
	if (size.Height > parentSize.Height)
	{
		size.Height = parentSize.Height - control->Location.Y;
	}

	int diffHeight = control->Rectangle.Bottom - parentWindow->Rectangle.Bottom;
	int diffWidth = control->Rectangle.Right - parentWindow->Rectangle.Right;

	if (diffHeight > 0 || diffWidth > 0)
	{
		diffHeight = diffHeight > 0 ? diffHeight : 0;
		diffWidth = diffWidth > 0 ? diffWidth : 0;
		CSize sz(control->Size.Width - diffWidth, control->Size.Height - diffHeight);
		ReverseMapDialog((HWND)parentWindow->Handle.ToInt64(), sz);
		size = System::Drawing::Size(sz.cx, sz.cy);
	}

	return size;
}

//-----------------------------------------------------------------------------
IWindowWrapper^ BaseWindowWrapper::Create(System::IntPtr handle)
{
	if (handle == System::IntPtr::Zero)
		return nullptr;

	HWND hWnd = (HWND)(int)handle;
	CWnd* pWnd = CWnd::FromHandle(hWnd);
	if (!pWnd)
	{
		ASSERT(FALSE);
		return nullptr;
	}
	LPCSTR szRuntimeClass = pWnd->GetRuntimeClass()->m_lpszClassName;
	if (strcmp(szRuntimeClass, "CEasyStudioDesignerDialog") == 0)//designer dialog
		return gcnew MEasyStudioPanel((System::IntPtr) handle);

	DWORD dwStyle = pWnd->GetStyle();
	// views are not registered
	if (pWnd->IsKindOf(RUNTIME_CLASS(CView)))
		return gcnew MView(handle);
	if (pWnd->GetControlSite())
		return gcnew GenericActiveX((System::IntPtr) handle);
	//HyperLink, StateButton e LinkButton non devono avere un wrapper agganciato, sono inclusi nel parsed
	//control che li ha creati
	if (
		pWnd->IsKindOf(RUNTIME_CLASS(CStateButton)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CLinkButton)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CHyperLink)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CControlLabel)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CCollapseButton))
		)
		return nullptr;

	TCHAR szClassName[MAX_CLASS_NAME + 1];
	VERIFY(::GetClassName(pWnd->m_hWnd, szClassName, MAX_CLASS_NAME));


	BaseWindowWrapper^ newWindow = nullptr;

	//non deve essere né una tab dialog, né un TabManager, infatti
	//questi oggetti sono creati in fase di generazione delle wrapper tipizzate
	//con l'editor json, questo non è più valido
	//ASSERT(!pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabManager)));
	//ASSERT(!pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)));

	const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetRegisteredControlFamily(pWnd);
	if (pFamily)
	{
		Type^ familyType = Type::GetType(gcnew System::String(pFamily->GetQualifiedTypeName()));

		if (familyType != nullptr)
			newWindow = (BaseWindowWrapper^)System::Activator::CreateInstance(familyType, handle);
	}

	if (newWindow != nullptr)
		return newWindow;

	if ((_tcsicmp(szClassName, _T("#32770")) == 0))//dialog
		return gcnew MPanel((System::IntPtr) handle);


	if ((_tcsicmp(szClassName, _T("Static")) == 0))
		return gcnew MLabel((System::IntPtr) handle);

	if (_tcsicmp(szClassName, _T("Button")) == 0)
	{
		UINT typeStyle = BS_TYPEMASK & dwStyle;
		if (typeStyle == BS_CHECKBOX
			|| typeStyle == BS_3STATE
			|| typeStyle == BS_AUTO3STATE
			|| typeStyle == BS_AUTOCHECKBOX)
			return gcnew GenericCheckBox((System::IntPtr) handle);

		if (typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON)
			return gcnew GenericRadioButton((System::IntPtr) handle);
		if (typeStyle == BS_GROUPBOX)
		{
			return gcnew GenericGroupBox((System::IntPtr) handle);
		}

		return gcnew GenericPushButton((System::IntPtr) handle);
	}

	if (_tcsicmp(szClassName, _T("ListBox")) == 0)
		return gcnew GenericListBox((System::IntPtr) handle);

	if (_tcsicmp(szClassName, _T("ComboBox")) == 0)
		return gcnew GenericComboBox((System::IntPtr) handle);

	if (_tcsicmp(szClassName, _T("Edit")) == 0)
		return gcnew GenericEdit((System::IntPtr) handle);

	if (_tcsicmp(szClassName, _T("SYSTREEVIEW32")) == 0)
		return gcnew GenericTreeView((System::IntPtr) handle);

	if (_tcsicmp(szClassName, _T("SYSLISTVIEW32")) == 0)
		return gcnew GenericListCtrl((System::IntPtr) handle);

	//OKKIO!!!! se aggiungi una nuova tipologia di controlli, modifica anche il metodo CanBeWrapped!!!!!!!
	return nullptr;
}

//-----------------------------------------------------------------------------
bool BaseWindowWrapper::CanBeWrapped(System::IntPtr handle)
{
	if (handle == System::IntPtr::Zero)
		return false;

	HWND hWnd = (HWND)(int)handle;
	CWnd* pWnd = CWnd::FromHandle(hWnd);
	if (!pWnd)
	{
		ASSERT(FALSE);
		return false;
	}

	// views are not registered
	if (pWnd->IsKindOf(RUNTIME_CLASS(CView)))
		return true;

	//HyperLink, StateButton e LinkButton non devono avere un wrapper agganciato, sono inclusi nel parsed
	//control che li ha creati
	if (
		pWnd->IsKindOf(RUNTIME_CLASS(CStateButton)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CLinkButton)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CHyperLink)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CControlLabel)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CCollapseButton))
		)
		return false;


	TCHAR szClassName[MAX_CLASS_NAME + 1];
	VERIFY(::GetClassName(pWnd->m_hWnd, szClassName, MAX_CLASS_NAME));

	//ignoro tutte le groupbox dentro le tilemanager (dovrebbe esserci solo quella della Static_area
	if ((_tcsicmp(szClassName, _T("Button")) == 0 && (pWnd->GetStyle() & BS_GROUPBOX) == BS_GROUPBOX))
	{
		CWnd* pParent = pWnd->GetParent();
		if (pParent && pParent->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CBaseTileDialog)))
			return false;
	}

	if (pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabManager)) || pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
		return true;

	const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetRegisteredControlFamily(pWnd);
	if (pFamily)
	{
		Type^ familyType = Type::GetType(gcnew System::String(pFamily->GetQualifiedTypeName()));

		if (familyType != nullptr)
			return true;
	}

	if (_tcsicmp(szClassName, _T("Static")) == 0 ||
		_tcsicmp(szClassName, _T("Button")) == 0 ||
		_tcsicmp(szClassName, _T("ListBox")) == 0 ||
		_tcsicmp(szClassName, _T("CheckBox")) == 0 ||
		_tcsicmp(szClassName, _T("RadioButton")) == 0 ||
		_tcsicmp(szClassName, _T("ComboBox")) == 0 ||
		_tcsicmp(szClassName, _T("Edit")) == 0 ||
		_tcsicmp(szClassName, _T("#32770")) == 0)
		return true;

	return false;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Initialize()
{
	HasCodeBehind = true;
	minSize = CUtility::GetIdealBaseWindowWrapperSize();
}

//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::CalcIdFromName(System::String^ name)
{
	if (name->ToUpper()->StartsWith(idPrefix) || String::IsNullOrEmpty(name))
		return name->ToUpper();

	return idPrefix + name->ToUpper();
}

//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::CalcNameFromId(System::String^ id)
{
	if (String::IsNullOrEmpty(id) || !id->ToUpper()->StartsWith(idPrefix))
		return id;
	if (id == staticAreaID)
		return staticAreaName;
	if (id == staticArea2ID)
		return staticArea2Name;

	id = id->Replace(idPrefix, "")->ToLower();
	array<System::String^>^ pieces = id->Split({ '_' });
	System::String^ concat = "";
	for each (System::String^ word in pieces)
	{
		concat = concat + word + " ";
	}

	return concat;

}

//----------------------------------------------------------------------------
Point BaseWindowWrapper::PartAnchor::get()
{
	return partAnchor;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::PartAnchor::set(Point value)
{
	partAnchor = value;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::CalculatePartAnchor(CPoint pt)
{
	CWnd* pWnd = GetWnd();
	CBaseTileDialog* pTileDialog = GetParentTileDialog();
	// devo essere in runtime e devo avere delle parts
	if	(
			!pWnd || !pTileDialog || !pTileDialog->GetDocument() ||
			!pTileDialog->HasParts() || 
			pTileDialog->GetDocument()->GetDesignMode() != CBaseDocument::DM_RUNTIME
		) 
		return;

	int nPart = pTileDialog->GetOwnerPart(pWnd);
	// per ora mi limito alla seconda part, funzionerebbe su tutto
	if (nPart <= 0)
		return;

	CTileDialogPart* pPart = pTileDialog->GetPart(nPart);
	partAnchor = Point(pt.x - pPart->GetStaticAreaRect().right, nPart);

	// se c'è un valore lo serializzo
	if (partAnchor.X != 0)
		AddChangedProperty("PartAnchor");
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::DelayedPartsAnchor()
{
	CWnd* pWnd = GetWnd();
	CBaseTileDialog* pTileDialog = GetParentTileDialog();
	if (!pWnd || !pTileDialog || !pTileDialog->HasParts() || PartAnchor == Point::Empty)
		return;

	CTileDialogPart* pPart = pTileDialog->GetPart(PartAnchor.Y);
	if (!pPart || PartAnchor.Y != 1)
		return;
	
	int nPartX = pPart->GetStaticAreaRect().right; 
	CRect actualRect;
	pPart->GetUsedRect(actualRect);
	int nPartY = actualRect.top == 0 ? Location.Y : (actualRect.top - pTileDialog->GetTitleHeight()) + originalLocation.Y;
	Location = Point(nPartX + PartAnchor.X, nPartY);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::CreateUniqueName(IEasyBuilderContainer^ parentContainer)
{
	System::String^ sToken = GetInternalName();
	if (System::String::IsNullOrEmpty(sToken))
	{
		sToken = GetType()->Name;
	}
	int nrTempName = 1;
	name = sToken;
	EasyBuilderComponent^ cmp = dynamic_cast<EasyBuilderComponent^>(parentContainer);
	while (cmp)
	{
		parentContainer = (IEasyBuilderContainer^)cmp;
		cmp = cmp->ParentComponent;
	}
	while (EasyBuilderComponent::HasComponent(parentContainer->Components, name, true))
	{
		name = System::String::Concat(sToken, (nrTempName++).ToString());
	}
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::CreateUniqueNameAndId(WindowWrapperContainer^ parentContainer, String^ sCandidateNameToken, String^ sCandidateIdToken)
{
	if (String::IsNullOrEmpty(sCandidateNameToken))
	{
		sCandidateNameToken = GetInternalName();
		if (String::IsNullOrEmpty(sCandidateNameToken))
		{
			sCandidateNameToken = GetType()->Name;
		}
	}
	if (String::IsNullOrEmpty(sCandidateIdToken))
	{
		if (idPrefix == "IDC_STATIC")
			sCandidateIdToken = idPrefix;  //per un generic label si preferisce IDC_STATIC al IDC_STATIC_MLABEL
		else
			sCandidateIdToken = CalcIdFromName(sCandidateNameToken);
	}

	String^ sCandidateId = sCandidateIdToken;
	String^ sCandidateName = sCandidateNameToken;

	// quando incontro le group box delle aree statiche non le posso rinominare
	// altrimenti smettono di essere le aree statiche.
	String^ sStaticArea = gcnew String(AfxGetTBResourcesMap()->DecodeID(TbControls, IDC_STATIC_AREA).m_strName);
	if (sCandidateId->StartsWith(sStaticArea))
	{
		name = CalcNameFromId(sCandidateId);
		Id = sCandidateId;
		return;
	}

	if (DesignModeType == EDesignMode::Runtime && HasCodeBehind)
	{
		name = sCandidateName;
		Id = sCandidateId;
		return;
	}

	//risalgo alla root
	while (parentContainer->Parent)
	{
		parentContainer = (WindowWrapperContainer^)parentContainer->Parent;
	}
	int nrTempId = 0;

	bool conflict = true;
	while (conflict)
	{
		conflict = false;
		List<BaseWindowWrapper^>^ list = parentContainer->GetChildrenByIdOrName(sCandidateId, sCandidateName);
		for each (BaseWindowWrapper^ w in list)
		{
			if (w == this)
				continue;
			nrTempId++;
			sCandidateId = String::Concat(sCandidateIdToken, "_", (nrTempId).ToString());
			sCandidateName = String::Concat(sCandidateNameToken, (nrTempId).ToString());
			conflict = true;
			break;
		}
	}
	name = sCandidateName;
	Id = sCandidateId;
}
//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::CreateNamespaceFromParent(IEasyBuilderContainer^ parentContainer)
{
	bool bInDesignMode = false;
	CTBNamespace::NSObjectType aType = (CTBNamespace::NSObjectType)GetNamespaceType();
	CTBNamespace aParent;
	if (MDocument::typeid->IsInstanceOfType(parentContainer))
	{
		aParent = CString(((MDocument^)parentContainer)->Namespace->ToString());
		bInDesignMode = ((MDocument^)parentContainer)->GetDocument()->IsInDesignMode();
	}
	else
	{
		CWnd* pParentWnd = ((BaseWindowWrapper^)parentContainer)->GetWnd();
		IOSLObjectManager* pParentInfo = dynamic_cast<IOSLObjectManager*>(pParentWnd);
		if (pParentInfo && pParentInfo->GetInfoOSL())
			aParent = pParentInfo->GetInfoOSL()->m_Namespace;
		bInDesignMode = ((BaseWindowWrapper^)parentContainer)->DesignModeType != EDesignMode::None;
	}

	// le customizzazioni che arrivano dal codice c# devono usare 
	// il nome che gli arriva da fuori
	CTBNamespace aNamespace(aType);

	if (bInDesignMode && (HasCodeBehind || System::String::IsNullOrEmpty(name)))
	{
		int nrTempName = 0;
		if (
			System::String::IsNullOrEmpty(name) || System::String::IsNullOrEmpty(Id))
		{
			if (WindowWrapperContainer::typeid->IsInstanceOfType(parentContainer))
			{
				CreateUniqueNameAndId((WindowWrapperContainer^)parentContainer, name, Id);
			}
			else
			{
				CreateUniqueName(parentContainer);
			}
		}
	}

	aNamespace.SetChildNamespace(aType, CString(name), aParent);
	if (String::IsNullOrEmpty(id))
		Id = CalcIdFromName(name);

	return gcnew System::String(aNamespace.ToString());
}
//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::GetInternalName()
{
	System::String^ id = Id;
	return System::String::IsNullOrEmpty(id)
		? GetType()->Name
		: id;
}

//----------------------------------------------------------------------------
System::String^	BaseWindowWrapper::Name::get()
{
	if (!System::String::IsNullOrEmpty(name))
		return name;

	if (DesignModeType == EDesignMode::Static)
	{
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri)
			return gcnew System::String(pDescri->m_strName);

		return String::Empty;
	}
	else
	{
		if (Namespace == nullptr)
			return gcnew System::String("");

		int nPos = Namespace->ToString()->LastIndexOf(".");
		if (nPos <= 0)
			return Namespace->ToString();

		return Namespace->ToString()->Substring(nPos + 1);
	}
}

//----------------------------------------------------------------------------
TbResourceType BaseWindowWrapper::GetTbResourceType()
{
	return TbControls;
}

/// <summary>
/// Internal Use
/// </summary>
//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::GetIdFormID(UINT nID, TbResourceType aType, BOOL full)
{
	CJsonResource resource = AfxGetTBResourcesMap()->DecodeID(aType, nID);
	CString sName = resource.m_strName;
	if (full && !resource.m_strContext.IsEmpty())
		sName = sName + _T(":") + resource.m_strContext;
	return  gcnew System::String(sName);
}

//----------------------------------------------------------------------------
System::String^	BaseWindowWrapper::FullId::get()
{
	if (!String::IsNullOrEmpty(Name))
		return Name;

	CWnd* pWnd = GetWnd();
	if (pWnd)
	{
		CJsonResource resource = AfxGetTBResourcesMap()->DecodeID(GetTbResourceType(), pWnd->GetDlgCtrlID());
		CString sName = resource.m_strName;
		if (DesignModeType != EDesignMode::Static && !resource.m_strContext.IsEmpty())
			sName = sName + _T(":") + resource.m_strContext;
		return  gcnew System::String(sName);
	}

	return id;
}

//----------------------------------------------------------------------------
System::String^	BaseWindowWrapper::Id::get()
{
	CWnd* pWnd = GetWnd();
	return pWnd && pWnd->m_hWnd
		? gcnew System::String(AfxGetTBResourcesMap()->DecodeID(GetTbResourceType(), pWnd->GetDlgCtrlID()).m_strName)
		: id;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Id::set(System::String^ value)
{
	CWnd* pWnd = GetWnd();
	if (pWnd)
		pWnd->SetDlgCtrlID(AfxGetTBResourcesMap()->GetTbResourceID(CString(value), GetTbResourceType()));
	id = value;
}

//----------------------------------------------------------------------------
System::String^	BaseWindowWrapper::Activation::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return "";

	return gcnew String(pDescri->m_strActivation);
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::Activation::set(System::String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	pDescri->m_strActivation = value;
}
//----------------------------------------------------------------------------
INameSpace^ BaseWindowWrapper::Namespace::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::HasStyle(DWORD s)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return false;

	return pWnd && ((pWnd->GetStyle() & s) == s);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::SetStyle(DWORD dwRemove, DWORD dwAdd)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}

	pWnd->ModifyStyle(dwRemove, dwAdd, SWP_FRAMECHANGED);
}
//----------------------------------------------------------------------------
bool BaseWindowWrapper::HasExStyle(DWORD s)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return false;

	return pWnd && ((pWnd->GetExStyle() & s) == s);
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::SetExStyle(DWORD dwRemove, DWORD dwAdd)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}

	pWnd->ModifyStyleEx(dwRemove, dwAdd, SWP_FRAMECHANGED);

}
//----------------------------------------------------------------------------
#pragma push_macro("SendMessage")
#undef SendMessage
System::IntPtr BaseWindowWrapper::SendMessage(int msg, System::IntPtr wParam, System::IntPtr lParam)
#pragma pop_macro("SendMessage")
{
	return (System::IntPtr)::SendMessage((HWND)(int)Handle, msg, (WPARAM)(int)wParam, (LPARAM)(int)lParam);
}
//----------------------------------------------------------------------------
#pragma push_macro("PostMessage")
#undef PostMessage
bool BaseWindowWrapper::PostMessage(int msg, System::IntPtr wParam, System::IntPtr lParam)
#pragma pop_macro("PostMessage")
{
	return ::PostMessage((HWND)(int)Handle, msg, (WPARAM)(int)wParam, (LPARAM)(int)lParam) == TRUE;
}

//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::Text::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_strText && !pDescri->m_strText.IsEmpty())
		return gcnew System::String(pDescri->m_strText);

	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return gcnew System::String("");

	CString str;
	pWnd->GetWindowText(str);
	return gcnew System::String(str);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Text::set(System::String^ value)
{
	CWnd* pWnd = GetWnd();
	if (pWnd)
		pWnd->SetWindowText(CString(value));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strText = value;
}

//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::ControlLabel::get()
{
	String^ nodeLabel = Text;

	if (System::String::IsNullOrEmpty(nodeLabel))
		nodeLabel = Name;

	return nodeLabel;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::TabOrder::set(int value)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}
	if (!TabStop)
		return;

	CWnd* pParent = pWnd->GetParent();
	if (!pParent)
		return;

	int index = -1;
	CWnd* pChild = pParent->GetWindow(GW_CHILD);
	CWnd* pWndBefore = const_cast<CWnd*>(&CWnd::wndTop);
	while (pChild)
	{
		if ((pChild->GetStyle() & WS_TABSTOP) != WS_TABSTOP || !CanBeWrapped((System::IntPtr)(int)pChild->m_hWnd))
		{
			pWndBefore = pChild;
			pChild = pChild->GetNextWindow();
			continue;
		}
		//se sono io, allora il mio taborder si sposta in avanti
		//e se mi sposto in avanti, non devo tenere conto del mio numero
		if (pChild->m_hWnd != pWnd->m_hWnd)
			index++;
		if (index == value)
		{
			if (pWndBefore->m_hWnd != pWnd->m_hWnd)
				pWnd->SetWindowPos(pWndBefore, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
			return;
		}
		if (index > value)
		{
			ASSERT(FALSE);
			return;
		}
		pWndBefore = pChild;
		pChild = pChild->GetNextWindow();
	}
}
//----------------------------------------------------------------------------
int BaseWindowWrapper::TabOrder::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return -1;

	if (!TabStop)
		return -1;

	CWnd* pParent = pWnd->GetParent();
	if (!pParent)
		return -1;
	int index = -1;
	CWnd* pChild = pParent->GetWindow(GW_CHILD);
	while (pChild)
	{
		if ((pChild->GetStyle() & WS_TABSTOP) != WS_TABSTOP || !CanBeWrapped((System::IntPtr)(int)pChild->m_hWnd))
		{
			pChild = pChild->GetNextWindow();
			continue;
		}

		index++;
		if (pChild->m_hWnd == pWnd->m_hWnd)
			return index;
		pChild = pChild->GetNextWindow();
	}
	return -1;
}

#pragma region Styles Properties
//----------------------------------------------------------------------------
bool BaseWindowWrapper::VScroll::get()
{
	if (HasStyle(WS_VSCROLL))
		return true;

	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bVScroll : false;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::VScroll::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_VSCROLL));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bVScroll = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::HScroll::get()
{
	if (HasStyle(WS_HSCROLL))
		return true;

	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bHScroll : false;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::HScroll::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_HSCROLL));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bHScroll = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::Border::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bBorder : HasExStyle(WS_EX_CLIENTEDGE);

}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Border::set(bool value)
{
	SetExStyle(SET_STYLE_PARAMS(WS_EX_CLIENTEDGE));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bBorder = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::Transparent::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bTransparent : HasExStyle(WS_EX_ACCEPTFILES);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Transparent::set(bool value)
{
	SetExStyle(SET_STYLE_PARAMS(WS_EX_ACCEPTFILES));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bTransparent = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::AcceptFiles::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bAcceptFiles : HasExStyle(WS_EX_ACCEPTFILES);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::AcceptFiles::set(bool value)
{
	SetExStyle(SET_STYLE_PARAMS(WS_EX_ACCEPTFILES));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bAcceptFiles = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::TabStop::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bTabStop : HasStyle(WS_TABSTOP);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::TabStop::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_TABSTOP));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bTabStop = value;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::Group::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bGroup : HasStyle(WS_GROUP);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Group::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_GROUP));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bGroup = value;
}

//----------------------------------------------------------------------------
bool MCheckBox::Group::get()
{
	if (HasStyle(WS_GROUP))
		return true;
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bGroup : false;
}

//----------------------------------------------------------------------------
void MCheckBox::Group::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_GROUP));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bGroup = value;
}

//----------------------------------------------------------------------------
bool MRadioButton::Group::get()
{
	if (HasStyle(WS_GROUP))
		return true;
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bGroup : false;
}

//----------------------------------------------------------------------------
void MRadioButton::Group::set(bool value)
{
	SetStyle(SET_STYLE_PARAMS(WS_GROUP));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bGroup = value;
}

#pragma endregion

//---------------------------------------------------------------------------------
void BaseWindowWrapper::DoSetLocation(System::Drawing::Point point)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;
	CWndObjDescription* pDescri = GetWndObjDescription();

	CPoint pt(point.X, point.Y);

	int idcCtrl = pWnd->GetDlgCtrlID();

	BOOL bIsStaticArea = CUtility::IsStaticArea(idcCtrl);

	CWnd* pParent = pWnd->GetParent();
	CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pParent);
	if (pTileDialog && !pTileDialog->IsLayoutIntialized() && DesignerMovable != EditingMode::Moving && !bIsStaticArea)
		pt.y -= pTileDialog->GetTitleHeight();

	WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
	if (container)
	{
		int CurrentDPi = container->GetCurrentLogPixels();
		if (CurrentDPi != container->LastEditDPI)
		{
			DpiConvertPoint(pt, container->LastEditDPI, CurrentDPi);
		}
	}

	//aggiorno le coordinate fisiche della finestra
	pWnd->SetWindowPos(NULL, pt.x, pt.y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
	CalculatePartAnchor(pt);

	if (pParent)
	{
		//quindi assegno le LU

		if (pDescri)
		{
			pDescri->m_sAnchor = _T("");
			//trasformo in coordinate logiche
			ReverseMapDialog(pParent->m_hWnd, pt);
			pDescri->m_X = pt.x;
			pDescri->m_Y = pt.y;
		}
	}
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Location::set(Point point)
{
	if (!IsStretchable)
	{
		DoSetLocation(point);
		return;
	}

	//is stretchable
	if (Site != nullptr && DesignMode)
	{
		bool oldValue = AutoFill;
		AutoFill = false;
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "AutoFill", oldValue, false);
		DoSetLocation(point);
	}
	else
		DoSetLocation(point);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::LocationLU::set(Point pointLU)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;
	CWndObjDescription* pDescri = GetWndObjDescription();
	//assegno le coordinate logiche
	if (pDescri)
	{
		pDescri->m_sAnchor = _T("");
		pDescri->m_X = pointLU.X;
		pDescri->m_Y = pointLU.Y;
	}
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
	{
		//trasformo in coordinate fisiche
		CPoint pt(pointLU.X, pointLU.Y);
		SafeMapDialog(pParent->m_hWnd, pt);

		WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
		if (container)
		{
			int CurrentDPi = container->GetCurrentLogPixels();
			if (CurrentDPi != container->LastEditDPI)
			{
				DpiConvertPoint(pt, container->LastEditDPI, CurrentDPi);
			}
		}

		//aggiorno le coordinate fisiche della finestra
		pWnd->SetWindowPos(NULL, pt.x, pt.y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
	}
}
//----------------------------------------------------------------------------
Point BaseWindowWrapper::Location::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return Point::Empty;

	System::Drawing::Point point = Rectangle.Location;
	CPoint aPt(point.X, point.Y);
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
		pParent->ScreenToClient(&aPt);

	return System::Drawing::Point(aPt.x, aPt.y);
}
//----------------------------------------------------------------------------
Point BaseWindowWrapper::LocationLU::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return Point::Empty;
	CWndObjDescription* pDescri = GetWndObjDescription();

	if (pDescri && pDescri->m_sAnchor.IsEmpty())
		return System::Drawing::Point(pDescri->m_X, pDescri->m_Y);
	System::Drawing::Point point = Location;
	CPoint pt(point.X, point.Y);
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
		ReverseMapDialog(pParent->m_hWnd, pt);
	point.X = pt.x;
	point.Y = pt.y;
	return point;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::DoSetSize(System::Drawing::Size size)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;

	CSize sz(size.Width, size.Height);
	WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
	if (container)
	{
		int CurrentDPi = container->GetCurrentLogPixels();
		if (CurrentDPi != container->LastEditDPI)
		{
			DpiConvertSize(sz, container->LastEditDPI, CurrentDPi);
		}
	}

	//assegno le coordinate fisiche
	pWnd->SetWindowPos(NULL, 0, 0, sz.cx, sz.cy, SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);

	//poi quello logiche
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
	{
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri)
		{
			ReverseMapDialog(pParent->m_hWnd, sz);
			pDescri->m_Width = sz.cx;
			pDescri->m_Height = sz.cy;
		}
	}
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Size::set(System::Drawing::Size size)
{
	if (!IsStretchable)
	{
		DoSetSize(size);
		return;
	}

	//is stretchable
	if (Site != nullptr && DesignMode)
	{
		if (EndCreation)
		{
			bool oldValue = AutoFill;
			AutoFill = false;
			PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "AutoFill", oldValue, false);
			DoSetSize(size);
			return;
		}
		else
		{
			MTileDialog^ parentTile = dynamic_cast<MTileDialog^>(Parent);
			if (parentTile != nullptr && parentTile->TileDialogType == ETileDialogSize::AutoFill)
			{
				bool bHasBrothers = parentTile->HasBrothers(this);

				bool bOldFillParent = AutoFill;
				bool bOldBottomStretch = BottomStretch;
				bool bOldRightStretch = RightStretch;

				AutoFill = !bHasBrothers;
				BottomStretch = bHasBrothers;
				RightStretch = bHasBrothers;

				PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "AutoFill", bOldFillParent, !bHasBrothers);
				PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "BottomStretch", bOldBottomStretch, bHasBrothers);
				PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "RightStretch", bOldRightStretch, bHasBrothers);
				return;
			}
		}

	}

	DoSetSize(size);
}

//----------------------------------------------------------------------------
System::Drawing::Size BaseWindowWrapper::Size::get()
{
	return Rectangle.Size;
}

//-----------------------------------------------------------------------------
void BaseWindowWrapper::AutoStretch::set(bool value)
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());

	if (!IsStretchable || !pResizableCtrl || AutoFill)
		return;

	autoSizeCtrl = -1;
	if (value)
	{
		autoSizeCtrl = 3;
		BottomStretch = true;
		RightStretch = true;
	}
	else
	{
		autoSizeCtrl = 0;
		BottomStretch = false;
		RightStretch = false;
	}

	pResizableCtrl->SetAutoSizeCtrl(autoSizeCtrl);
	pResizableCtrl->SetResizableCurSize(0, 0);
	pResizableCtrl->DoRecalcCtrlSize();
}

//-----------------------------------------------------------------------------------
bool BaseWindowWrapper::BottomStretch::get()
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());
	
	if (!IsStretchable || !pResizableCtrl)
		return false;

	autoSizeCtrl = pResizableCtrl->GetAutoSizeCtrl();
	return autoSizeCtrl == 1 || autoSizeCtrl == 3;
}

//-------------------------------------------------------------------------------------
void BaseWindowWrapper::BottomStretch::set(bool value)
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());
	
	if (!IsStretchable || !pResizableCtrl || AutoFill)
		return;

	autoSizeCtrl = -1;
	if (value)
	{
		if (RightStretch)
			autoSizeCtrl = 3;
		else
			autoSizeCtrl = 1;
	}
	else
	{
		if (RightStretch)
			autoSizeCtrl = 2;
		else
			autoSizeCtrl = 0;
	}

	pResizableCtrl->SetAutoSizeCtrl(autoSizeCtrl);
	pResizableCtrl->SetResizableCurSize(0, 0);
	pResizableCtrl->DoRecalcCtrlSize();
}

//-----------------------------------------------------------------------------
bool BaseWindowWrapper::RightStretch::get()
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());

	if (!IsStretchable || !pResizableCtrl)
		return false;

	autoSizeCtrl = pResizableCtrl->GetAutoSizeCtrl();
	return autoSizeCtrl == 2 || autoSizeCtrl == 3;
}

//-----------------------------------------------------------------------------
void BaseWindowWrapper::RightStretch::set(bool value)
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());

	if (!IsStretchable || !pResizableCtrl || AutoFill)
		return;

	autoSizeCtrl = -1;
	if (value)
	{
		if (BottomStretch)
			autoSizeCtrl = 3;
		else
			autoSizeCtrl = 2;
	}
	else
	{
		if (BottomStretch)
			autoSizeCtrl = 1;
		else
			autoSizeCtrl = 0;
	}

	pResizableCtrl->SetAutoSizeCtrl(autoSizeCtrl);
	pResizableCtrl->SetResizableCurSize(0, 0);
	pResizableCtrl->DoRecalcCtrlSize();
}

//-----------------------------------------------------------------------------
bool BaseWindowWrapper::AutoFill::get()
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());

	if (!IsStretchable || !pResizableCtrl)
		return false;

	autoSizeCtrl = pResizableCtrl->GetAutoSizeCtrl();
	return autoSizeCtrl == 7;
}

//-----------------------------------------------------------------------------
void BaseWindowWrapper::AutoFill::set(bool value)
{
	ResizableCtrl* pResizableCtrl = dynamic_cast<ResizableCtrl*>(GetWnd());

	if (!IsStretchable || !pResizableCtrl)
		return;

	autoSizeCtrl = value ? 7 : 0;

	pResizableCtrl->SetAutoSizeCtrl(autoSizeCtrl);
	pResizableCtrl->SetResizableCurSize(0, 0);
	pResizableCtrl->DoRecalcCtrlSize();
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::SizeLU::set(System::Drawing::Size sizeLU)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		pDescri->m_Width = sizeLU.Width;
		pDescri->m_Height = sizeLU.Height;
	}
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
	{
		CSize sz(sizeLU.Width, sizeLU.Height);
		SafeMapDialog(pParent->m_hWnd, sz);
		WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
		if (container)
		{
			int CurrentDPi = container->GetCurrentLogPixels();
			if (CurrentDPi != container->LastEditDPI)
			{
				DpiConvertSize(sz, container->LastEditDPI, CurrentDPi);
			}
		}
		//assegno le coordinate fisiche
		pWnd->SetWindowPos(NULL, 0, 0, sz.cx, sz.cy, SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);
	}
}

//----------------------------------------------------------------------------
System::Drawing::Size BaseWindowWrapper::SizeLU::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return System::Drawing::Size(0, 0);

	/*CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && (pDescri->m_Rect.Width() != 0 || pDescri->m_Rect.Height() != 0))
		return System::Drawing::Size(pDescri->m_Rect.Width(), pDescri->m_Rect.Height());*/

	System::Drawing::Size size = Size;
	CSize sz(size.Width, size.Height);

	CWnd* pParent = pWnd->GetParent();
	if (pParent)
		ReverseMapDialog(pParent->m_hWnd, sz);
	size.Width = sz.cx;
	size.Height = sz.cy;
	return size;
}

//----------------------------------------------------------------------------
System::String^ BaseWindowWrapper::Anchor::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? gcnew String(pDescri->m_sAnchor) : "";
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Anchor::set(System::String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		List<System::String^>^ list = gcnew List<System::String^>();
		CWndObjDescriptionContainer& children = pDescri->GetParent()->m_Children;
		for (int i = children.GetUpperBound(); i >= 0; i--)
		{
			CWndObjDescription* pChild = children.GetAt(i);
			if (pChild && pChild != pDescri)
				list->Add(gcnew System::String(pChild->GetJsonID()));
		}
		list->Add(gcnew System::String("COL1"));
		list->Add(gcnew System::String("COL2"));

		//la metto da parte prima di cambiare ancoraggio: contiene x e y della finestra effettiva, e non quelli
		//della wnddescription perché non avrebbero senso se sono ancorato (
		Point pt = LocationLU;
		pDescri->m_sAnchor = list->Contains(value) ? value : _T("");

		if (pDescri->m_sAnchor.IsEmpty())
		{
			pDescri->m_X = pt.X;
			pDescri->m_Y = pt.Y;
		}
		else
		{
			pDescri->m_X = NULL_COORD;
			pDescri->m_Y = NULL_COORD;
		}

	}
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::DesignMode::get()
{
	return Parent == nullptr ? false : Parent->DesignMode;
}

//----------------------------------------------------------------------------
EditingMode BaseWindowWrapper::DesignerMovable::get()
{
	return Anchor == "" || DesignModeType == EDesignMode::Runtime ?
		EditingMode::All : EditingMode::OnlyResizing;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::DesignerVisible::get()
{
	return Parent == nullptr || !BaseWindowWrapper::typeid->IsInstanceOfType(Parent)
		? false
		: ((BaseWindowWrapper^)Parent)->DesignerVisible;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::Visible::get()
{
	CWnd* pWnd = GetWnd();
	if (pWnd)
	{
		CWndObjDescription* pDesc = GetWndObjDescription();
		if (pDesc)
			return pDesc->m_bVisible;
	}

	//se devo visualizzare i campi nascosti, non posso usare lo style, sarà sempre visibile
	if (DesignerVisible)
		return __super::Visible;

	return HasStyle(WS_VISIBLE);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Visible::set(bool value)
{
	__super::Visible = value;

	CWnd* pWnd = GetWnd();
	if (pWnd)
	{
		CWndObjDescription* pDesc = GetWndObjDescription();
		if (pDesc)
			pDesc->m_bVisible = value;
	}
}

//----------------------------------------------------------------------------
Color BaseWindowWrapper::BorderColor::get()
{
	return borderColor;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::BorderColor::set(Color value)
{
	borderColor = value;
	Invalidate();
}
//----------------------------------------------------------------------------
bool BaseWindowWrapper::Enabled::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return false;
	}
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_bEnabled;
	DWORD currentStyle = pWnd->GetStyle();
	return (currentStyle & WS_DISABLED) == 0;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Enabled::set(bool value)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_bEnabled != value)
	{
		pDescri->m_bEnabled = value;
		pDescri->SetUpdated(&(pDescri->m_bEnabled));
	}
	pWnd->EnableWindow(value);
}

//----------------------------------------------------------------------------
int BaseWindowWrapper::MarginLeft::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_MarginLeft;
	return NULL_COORD;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::MarginLeft::set(int value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	else pDescri->m_MarginLeft = value;
}

//----------------------------------------------------------------------------
int BaseWindowWrapper::MarginTop::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_MarginTop;
	return NULL_COORD;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::MarginTop::set(int value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	else pDescri->m_MarginTop = value;
}

//----------------------------------------------------------------------------
int BaseWindowWrapper::MarginBottom::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_MarginBottom;
	return NULL_COORD;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::MarginBottom::set(int value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	else pDescri->m_MarginBottom = value;
}
//----------------------------------------------------------------------------
int BaseWindowWrapper::CaptionWidth::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_CaptionWidth;
	return NULL_COORD;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::CaptionWidth::set(int value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	else pDescri->m_CaptionWidth = value;
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::UpdateWindow()
{
	::UpdateWindow(GetHandle());
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::InvalidateInternal(CWnd* pWnd, const CRect& screenRect)
{
	if (!pWnd)
	{
		return;
	}
	CPoint p = screenRect.TopLeft();
	pWnd->ScreenToClient(&p);
	CRect r(p, screenRect.Size());
	pWnd->InvalidateRect(&r, TRUE);

	CWnd* pWndChild = pWnd->GetWindow(GW_CHILD);
	while (pWndChild)
	{
		InvalidateInternal(pWndChild, screenRect);
		pWndChild = pWndChild->GetWindow(GW_HWNDNEXT);
	}
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::Invalidate(System::Drawing::Rectangle screenCoordRect)
{
	CWnd* pWnd = GetWnd();
	if (pWnd)
	{
		InvalidateInternal(
			pWnd,
			CRect(screenCoordRect.Left, screenCoordRect.Top, screenCoordRect.Right, screenCoordRect.Bottom)
		);
	}
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Invalidate()
{
	//this->Rectangle=rettangolo in coordinate screen.
	this->Invalidate(this->Rectangle);
}

//----------------------------------------------------------------------------
System::Drawing::Rectangle BaseWindowWrapper::Rectangle::get()
{
	CRect r;
	::GetWindowRect(GetHandle(), &r);
	return System::Drawing::Rectangle(r.left, r.top, r.Width(), r.Height());
}

//----------------------------------------------------------------------------
System::Drawing::Rectangle BaseWindowWrapper::ClientRectangle::get()
{
	CRect r;
	::GetClientRect(GetHandle(), &r);
	return System::Drawing::Rectangle(r.left, r.top, r.Width(), r.Height());
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::ScreenToClient(System::Drawing::Rectangle% rect)
{
	CPoint p(rect.X, rect.Y);
	::ScreenToClient(GetHandle(), &p);
	rect.X = p.x;
	rect.Y = p.y;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::ClientToScreen(System::Drawing::Rectangle% rect)
{
	CPoint p(rect.X, rect.Y);
	::ClientToScreen(GetHandle(), &p);
	rect.X = p.x;
	rect.Y = p.y;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::ScreenToClient(Point% point)
{
	CPoint p(point.X, point.Y);
	::ScreenToClient(GetHandle(), &p);

	point.X = p.x;
	point.Y = p.y;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::ClientToScreen(Point% point)
{
	CPoint p(point.X, point.Y);
	::ClientToScreen(GetHandle(), &p);

	point.X = p.x;
	point.Y = p.y;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	ASSERT(FALSE);
	return false;
}

//----------------------------------------------------------------------------
CWnd* BaseWindowWrapper::GetWnd()
{
	return CWnd::FromHandle(GetHandle());
}
//----------------------------------------------------------------------------
CWndObjDescription* BaseWindowWrapper::GetWndObjDescription()
{
	return CWndObjDescription::GetFrom(GetHandle());
}

//----------------------------------------------------------------------------
System::IntPtr BaseWindowWrapper::GetWndPtr()
{
	return (System::IntPtr) GetWnd();
}

//----------------------------------------------------------------------------
HWND BaseWindowWrapper::GetHandle()
{
	return (HWND)(int)Handle;
}

//----------------------------------------------------------------------------
System::IntPtr BaseWindowWrapper::Handle::get()
{
	return nativeWindow == nullptr ? System::IntPtr::Zero : nativeWindow->Handle;
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::Handle::set(System::IntPtr value)
{
	if (nativeWindow->Handle == value)
		return;
	nativeWindow->ReleaseHandle();
	nativeWindow->AssignHandle(value);
}

//----------------------------------------------------------------------------
void BaseWindowWrapper::SwitchVisibility(bool bVisible)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;

	if (bVisible)
	{
		if ((pWnd->GetStyle() & WS_VISIBLE) != WS_VISIBLE)
		{
			pWnd->ShowWindow(SW_SHOW);
			visible = false;
		}
	}
	else
	{
		if (!visible && (pWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE)
		{
			pWnd->ShowWindow(SW_HIDE);
		}
	}
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::IsWindowVisible()
{
	CWnd* pWnd = GetWnd();
	return pWnd ? pWnd->IsWindowVisible() == TRUE : false;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::CanCreate()
{
	return false;
}

//----------------------------------------------------------------------------
bool BaseWindowWrapper::CanChangeProperty(System::String^ propertyName)
{
	bool canChange = true;

	if (propertyName == "PartAnchor")
		return false;

	if (!IsStretchable)
		return __super::CanChangeProperty(propertyName);

	//is stretchable
	MTileDialog^ parentTile = dynamic_cast<MTileDialog^>(Parent);

	if (parentTile == nullptr)
		return __super::CanChangeProperty(propertyName);

	if (parentTile->TileDialogType == ETileDialogSize::AutoFill && (AutoFill || (BottomStretch && RightStretch)))
		canChange = false;

	if (propertyName == "AutoFill" || propertyName == "BottomStretch" || propertyName == "RightStretch")
		return canChange;

	return __super::CanChangeProperty(propertyName);
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::Focus()
{
	CWnd* pWnd = GetWnd();
	ASSERT(pWnd);
	if (pWnd)
		pWnd->SetFocus();
}
//----------------------------------------------------------------------------
bool BaseWindowWrapper::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case WM_PAINT:
	{
		Paint(this, EasyBuilderEventArgs::Empty);
		break;
	}

	case WM_LBUTTONDOWN:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseDown(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Left, 1, x, y, 0));
		break;
	}

	case WM_RBUTTONDOWN:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseDown(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Right, 1, x, y, 0));
		break;
	}
	case WM_MBUTTONDOWN:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseDown(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Middle, 1, x, y, 0));
		break;
	}
	case WM_LBUTTONUP:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseUp(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Left, 1, x, y, 0));
		break;
	}

	case WM_RBUTTONUP:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseUp(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Right, 1, x, y, 0));
		break;
	}
	case WM_MBUTTONUP:
	{
		int x = GET_X_LPARAM((LPARAM)(int)m.LParam);
		int y = GET_Y_LPARAM((LPARAM)(int)m.LParam);
		Point aPt(x, y);
		ClientToScreen(aPt);
		if (this->Rectangle.Contains(aPt))
			MouseUp(this, gcnew MouseEventArgs(System::Windows::Forms::MouseButtons::Middle, 1, x, y, 0));
		break;
	}
	case WM_SIZE:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		SizeChanged(this, args);
		break;
	}
	case WM_MOVE:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		LocationChanged(this, args);
		break;
	}
	case WM_SETFOCUS:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		SetFocus(this, args);
		break;
	}
	}
	return false;
}
//----------------------------------------------------------------------------
void BaseWindowWrapper::AfterWndProc(Message% m)
{
	if (m.Msg == WM_PAINT)
	{
		Graphics^ g = nullptr;
		try
		{
			if (borderColor != Color::Empty)
			{
				if (g == nullptr)
					g = Graphics::FromHwnd(Handle);
				Pen^ pen = gcnew Pen(borderColor);
				g->DrawRectangle(pen, Drawing::Rectangle(0, 0, Size.Width - 1, Size.Height - 1));
				delete pen;
			}

			if (!visible)
			{
				if (g == nullptr)
					g = Graphics::FromHwnd(Handle);
				int size = 16;
				System::IO::Stream^ controlClassImage = BaseWindowWrapper::typeid->Assembly->GetManifestResourceStream("HiddenField.png");

				HatchBrush^ aHatchBrush = gcnew HatchBrush(HatchStyle::DottedDiamond, Color::Gray, Color::Transparent);
				g->FillRectangle(aHatchBrush, Drawing::Rectangle(1, 1, Size.Width - 1, Size.Height - 1));
				delete aHatchBrush;

				Bitmap^ bmp = ((Bitmap^)Bitmap::FromStream(controlClassImage));
				g->DrawImage(bmp, 5, 0, size, size);
				delete bmp;
			}
		}
		finally
		{
			delete g;
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedControl Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedControl::MParsedControl(System::IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr),
	m_pControl(NULL),
	dataBinding(nullptr),
	m_pFont(NULL)
{
	m_pControl = GetParsedCtrl(GetWnd());

	controlClass = gcnew ControlClass(this);
	components = gcnew List<IComponent^>();
	showHotLinkButton = true;
}


//----------------------------------------------------------------------------
MParsedControl::MParsedControl(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, controlClass, location, hasCodeBehind),
	m_pControl(NULL),
	dataBinding(nullptr),
	m_pFont(NULL)
{
	m_pControl = ::GetParsedCtrl(GetWnd());
	this->controlClass = gcnew ControlClass(this);
	components = gcnew List<IComponent^>();
	//la devo fare anche se l'ha già fatta il papà perché deve spostare anche le finestre accessorie di m_pControl, 
	//che finora non avevo ancora valorizzato

	Location = location;
	showHotLinkButton = true;

}

//----------------------------------------------------------------------------
MParsedControl::~MParsedControl()
{
	this->!MParsedControl();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MParsedControl::!MParsedControl()
{
	CWnd* pWnd = GetWnd();
	if (pWnd && !m_pControl)
		pWnd->DestroyWindow();

	if (HasCodeBehind)
	{
		m_pControl = NULL;
		return;
	}

	DataBinding = nullptr;//pulisce eventuali reference al databinding

	//prima provo a vedere se ho un handle buono
	pWnd = GetWnd();
	//in subordine, vedo se ho un parsed control che è rimasto orfano della sua finestra
	//(che è stata distrutta) ma va cancellato
	if (!pWnd)
		pWnd = m_pControl ? m_pControl->GetCtrlCWnd() : NULL;

	//se non ho nessun oggetto da cancellare, esco
	if (!pWnd)
	{
		m_pControl = NULL;
		return;
	}

	//controllo se ho un parent da cui togliere il link
	CWnd* pParentWnd = pWnd->m_hWnd ? pWnd->GetParent() : NULL;
	if (pParentWnd)
	{
		CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
		if (pParsedForm)
			pParsedForm->GetControlLinks()->Remove(pWnd);
	}
	//distruggo la finestra se ancora attiva
	if (pWnd->m_hWnd)
		pWnd->DestroyWindow();
	//distruggo l'oggetto
	delete pWnd;
	m_pControl = NULL;
	//se ho un font custom, lo distruggo
	if (m_pFont)
	{
		m_pFont->DeleteObject();
		delete m_pFont;
	}

	if (hotLink != nullptr)
		hotLink->AttachedControl = nullptr;
}

//------------------------------------------------------------------------------------
void MParsedControl::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	ASSERT(pParentDescription);
	if (!pParentDescription)
		return;

	if (!this->HasCodeBehind)
	{
		jsonDescription = pParentDescription->AddChildWindow(this->GetWnd(), this->Name);

		ASSERT(jsonDescription);
		if (!jsonDescription)
			return;

		__super::UpdateAttributesForJson(pParentDescription);

		jsonDescription->m_Width = ((BaseWindowWrapper^)this)->Size.Width;
		jsonDescription->m_Height = ((BaseWindowWrapper^)this)->Size.Height;
		jsonDescription->m_strControlCaption = this->Caption;
		jsonDescription->m_strIds.RemoveAll();
		jsonDescription->m_strIds.Add(this->Name);

		//manage anchor
		jsonDescription->m_sAnchor = GetHorizontalIdAnchor();

		//manage data binding
		if (jsonDescription->m_pBindings)
		{
			//exists => clear
			BindingInfo* pBindings = jsonDescription->m_pBindings;
			if (pBindings->m_pHotLink)
			{
				delete pBindings->m_pHotLink;
				pBindings->m_pHotLink = NULL;
			}
			delete pBindings;
			jsonDescription->m_pBindings = NULL;
		}
		if (this->DataBinding != nullptr)
		{
			//update databinding
			jsonDescription->m_pBindings = new BindingInfo();
			NameSpace^ parent = (NameSpace^)this->DataBinding->Parent->Namespace;
			jsonDescription->m_pBindings->m_strDataSource = CString(parent->Leaf) + _T(".") + CString(this->DataBinding->Name);
			if (this->HotLink != nullptr)
			{
				jsonDescription->m_pBindings->m_pHotLink = new HotLinkInfo();
				jsonDescription->m_pBindings->m_pHotLink->m_strName = CString(this->HotLink->Name);
				jsonDescription->m_pBindings->m_pHotLink->m_strNamespace = CString(this->HotLink->Namespace->FullNameSpace);
				jsonDescription->m_pBindings->m_pHotLink->m_bMustExistData = (Bool3)this->HotLink->DataMustExist;
				jsonDescription->m_pBindings->m_pHotLink->m_bEnableAddOnFly = (Bool3)this->HotLink->CanAddOnFly;
			}
		}
	}
	else
	{
		//TODO: serializze differences
	}
}

//-----------------------------------------------------------------------------------------------------
void MParsedControl::GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	//no children
}

//----------------------------------------------------------------------------
void MParsedControl::Font::set(System::Drawing::Font^ font)
{
	if (!m_pControl)
		return;

	if (m_pFont)
	{
		m_pFont->DeleteObject();
		delete m_pFont;
	}

	ExternalAPI::LOGFONT^ lf = gcnew ExternalAPI::LOGFONT();
	font->ToLogFont(lf);
	System::IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(lf));
	Marshal::StructureToPtr(lf, ptr, false);
	m_pFont = new CFont();
	m_pFont->CreateFontIndirect((LOGFONT*)(int)ptr);
	m_pControl->SetCtrlFont(m_pFont);
	Marshal::FreeHGlobal(ptr);
}
//----------------------------------------------------------------------------
INameSpace^	MParsedControl::Namespace::get()
{
	return gcnew NameSpace(m_pControl ? gcnew System::String(m_pControl->GetNamespace().ToString()) : System::String::Empty);
}

//----------------------------------------------------------------------------
ComponentCollection^ MParsedControl::Components::get()
{
	return gcnew ComponentCollection(components->ToArray());
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::Name::get()
{
	if (DesignModeType == EDesignMode::Static)
	{
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri)
			return gcnew System::String(pDescri->m_strName);
	}
	else
	{
		if (m_pControl)
			return gcnew System::String(m_pControl->GetNamespace().GetObjectName());
	}

	return __super::Name;
}
//----------------------------------------------------------------------------
void MParsedControl::Name::set(System::String^ name)
{
	if (!m_pControl)
		return;

	//la descrizione va sempre aggiornata
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strName = name;
	//per motivi di funzionamento interno, aggiorno il namespace solo se non vuoto
	if (!String::IsNullOrEmpty(name))
		m_pControl->GetNamespace().SetObjectName(name, TRUE);
}

//----------------------------------------------------------------------------
void MParsedControl::Visible::set(bool value)
{
	__super::Visible = value;

	if (!this->DesignMode)
	{
		this->DataBinding->DataVisible = value;
	}
}

//----------------------------------------------------------------------------
void MParsedControl::Location::set(Point point)
{
	//non posso modificare la size finché non ho il parsed control! altrimenti sposto solo un pezzo...
	if (!m_pControl)
		return;

	CWnd* pWnd = GetWnd();
	CSize offset;
	//Sposto la CWnd primaria (il parsed control intero o la porzione editabile se c'è un linkbutton o statebutton)
	if (pWnd)
	{
		//posizione del control prima dello spostamento
		CRect originalRect;
		pWnd->GetWindowRect(&originalRect);
		CWnd* pParent = pWnd->GetParent();
		if (pParent)
			pParent->ScreenToClient(originalRect);
		__super::Location = point;
		offset = CSize(__super::Location.X - originalRect.left, __super::Location.Y - originalRect.top);
	}

	//Controllo se c'è uno StateButton  o LinkButton attaccato al parsedControl
	CWnd* pButton = m_pControl->GetButton();
	if (pButton)
		MoveAuxControl(pButton, offset);

	//Controllo se c'è uno CHyperLink attacato al parsedControl
	CHyperLink* pHyperLink = m_pControl->GetHyperLink();
	if (pHyperLink)
		MoveAuxControl(pHyperLink, offset);

	CControlLabel* pLabel = m_pControl->GetControlLabel();
	if (pLabel)
		MoveAuxControl(pLabel, offset);

	//Infine cerco nell'array degli state control per vedere se ci sono altri bottoni addizionali da spostare
	for (int i = 0; i <= m_pControl->GetStateCtrlsArray().GetUpperBound(); i++)
	{
		CStateCtrlObj* pState = (CStateCtrlObj*)m_pControl->GetStateCtrlsArray().GetAt(i);
		if (!pState)
			continue;

		CWnd* pStateWnd = pState->GetButton();
		if (!pStateWnd)
			continue;

		MoveAuxControl(pStateWnd, offset);
	}
}

//----------------------------------------------------------------------------
bool MParsedControl::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MParsedControl::typeid) || MParsedControl::typeid->IsInstanceOfType(obj))
		)
		return false;

	MParsedControl^ aControl = (MParsedControl^)obj;
	return this->FullId == aControl->FullId;
}

//----------------------------------------------------------------------------
void MParsedControl::ReSetCtrlCaption(CString caption)
{
	// SetCtrlCaption è virtualizzata, per i Button non fa nulla
	if (this->DesignModeType == EDesignMode::Static)
		m_pControl->SetCtrlCaption(caption, TARIGHT, (::VerticalAlignment)CaptionVerticalAlign, CParsedCtrl::Left, NULL_COORD);
	else
		m_pControl->SetCtrlCaption(caption);
}

//----------------------------------------------------------------------------
void MParsedControl::Size::set(System::Drawing::Size sz)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return;

	CRect originalRect;
	int originalWidth = 0;

	//posizione del control prima dello spostamento
	pWnd->GetWindowRect(&originalRect);

	__super::Size = sz;

	//recupero la nuova posizione del parsedControl
	CRect newParsedControlRect;
	pWnd->GetWindowRect(&newParsedControlRect);

	if (!m_pControl)
		return;

	//Controllo se c'è uno StateButton  o LinkButton attaccato al parsedControl
	CWnd* pButton = m_pControl->GetButton();
	if (pButton && pButton->GetParent())
		ResizeAuxButton(pButton, originalRect, newParsedControlRect);

	ReSetCtrlCaption(CString(Caption));

	//Controllo se c'è uno CHyperLink attacato al parsedControl
	CHyperLink* pHyperLink = m_pControl->GetHyperLink();
	if (pHyperLink)
		ResizeHyperLink(pHyperLink, sz);


	//Infine cerco nell'array degli state control per vedere se ci sono altri bottoni addizionali da spostare
	for (int i = 0; i <= m_pControl->GetStateCtrlsArray().GetUpperBound(); i++)
	{
		CStateCtrlObj* pState = (CStateCtrlObj*)m_pControl->GetStateCtrlsArray().GetAt(i);
		if (!pState)
			continue;

		CWnd* pStateWnd = pState->GetButton();
		if (!pStateWnd || !pStateWnd->GetParent())
			continue;

		ResizeAuxButton(pStateWnd, originalRect, newParsedControlRect);
	}
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::Caption::get()
{
	if (!m_pControl)
		return System::String::Empty;
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return gcnew System::String(pDescri->m_strControlCaption);
	CString str = m_pControl->GetCtrlCaption();
	return gcnew System::String(str);
}

//----------------------------------------------------------------------------
void MParsedControl::Caption::set(System::String^ value)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	//la descrizione va sempre aggiornata
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strControlCaption = CString(value);

	ReSetCtrlCaption(CString(value));
}

//----------------------------------------------------------------------------
System::Drawing::Font^ MParsedControl::CaptionFont::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (!pDesc)
		return nullptr;
	if (!m_pControl || !m_pControl->GetControlLabel())
		return nullptr;

	System::Drawing::FontStyle fontStyle = System::Drawing::FontStyle::Regular;
	CFont* pFont = m_pControl->GetControlLabel()->GetFont(); //default per TB quanfo il font name corrisponde a questo non va serializzato
	if (pFont == NULL)
		return nullptr;
	int hToPoint = 0;
	LOGFONT lf;
	if (pDesc->m_pCaptionFontDescription)
	{
		if (pDesc->m_pCaptionFontDescription->m_bIsBold)		fontStyle = fontStyle | System::Drawing::FontStyle::Bold;
		if (pDesc->m_pCaptionFontDescription->m_bIsItalic)		fontStyle = fontStyle | System::Drawing::FontStyle::Italic;
		if (pDesc->m_pCaptionFontDescription->m_bIsUnderline)	fontStyle = fontStyle | System::Drawing::FontStyle::Underline;

		AfxGetControlFont()->GetLogFont(&lf);
		if (pDesc->m_pCaptionFontDescription->m_nFontSize > 0)
			hToPoint = (int)pDesc->m_pCaptionFontDescription->m_nFontSize;
		else
			hToPoint = GetDisplayFontPointSize(lf.lfHeight);

		System::String^ faceName;
		if (pDesc->m_pCaptionFontDescription->m_strFaceName.IsEmpty())
		{
			faceName = gcnew System::String(lf.lfFaceName);
		}
		else
		{
			faceName = gcnew System::String(pDesc->m_pCaptionFontDescription->m_strFaceName);
		}

		return gcnew System::Drawing::Font(
			faceName,
			(float)hToPoint,
			fontStyle);
	}
	pFont->GetLogFont(&lf);

	System::String^ faceName = gcnew System::String(lf.lfFaceName);
	if (lf.lfItalic) fontStyle = fontStyle | System::Drawing::FontStyle::Italic;
	if (lf.lfUnderline) fontStyle = fontStyle | System::Drawing::FontStyle::Underline;
	if (lf.lfWeight >= 700) fontStyle = fontStyle | System::Drawing::FontStyle::Bold;
	hToPoint = GetDisplayFontPointSize(lf.lfHeight);
	return gcnew System::Drawing::Font(faceName, (float)hToPoint, fontStyle);

}
//----------------------------------------------------------------------------
void MParsedControl::CaptionFont::set(System::Drawing::Font^ value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (!pDesc)
		return;
	if (!m_pControl || !m_pControl->GetControlLabel())
		return;

	int pointToH = abs(GetDisplayFontHeight((int)(value->SizeInPoints)));

	if (value == nullptr)
	{
		if (pDesc->m_pCaptionFontDescription)
		{
			delete(pDesc->m_pCaptionFontDescription);
			pDesc->m_pCaptionFontDescription = NULL;
		}
		return;
	}
	if (!pDesc->m_pCaptionFontDescription)
		pDesc->m_pCaptionFontDescription = new CFontDescription();

	pDesc->m_pCaptionFontDescription->m_bIsBold = value->Bold;
	pDesc->m_pCaptionFontDescription->m_bIsItalic = value->Italic;
	pDesc->m_pCaptionFontDescription->m_bIsUnderline = value->Underline;

	LOGFONT lfDef;
	AfxGetControlFont()->GetLogFont(&lfDef);

	pDesc->m_pCaptionFontDescription->m_strFaceName = value->Name;
	if (pDesc->m_pCaptionFontDescription->m_strFaceName == lfDef.lfFaceName)
		pDesc->m_pCaptionFontDescription->m_strFaceName = _T("");

	if (GetDisplayFontPointSize(lfDef.lfHeight) == value->SizeInPoints)
		pDesc->m_pCaptionFontDescription->m_nFontSize = 0;
	else
		pDesc->m_pCaptionFontDescription->m_nFontSize = value->SizeInPoints;

	pDesc->SetUpdated(&pDesc->m_pCaptionFontDescription);
	CCustomFont* pCustomFont = dynamic_cast<CCustomFont*>(GetWnd());
	if (pCustomFont) {
		pCustomFont->SetOwnFont(value->Bold, value->Italic, value->Underline, (int)value->SizeInPoints, CString(value->Name));
		return;
	}
}


//----------------------------------------------------------------------------
EVerticalAlign MParsedControl::CaptionVerticalAlign::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return (EVerticalAlign)pDescri->m_CaptionVerticalAlign;

	return EVerticalAlign::Top;
}
//----------------------------------------------------------------------------
void MParsedControl::CaptionVerticalAlign::set(EVerticalAlign value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_CaptionVerticalAlign != (::VerticalAlignment)value)
	{
		pDescri->m_CaptionVerticalAlign = (::VerticalAlignment)value;
		pDescri->SetUpdated(&(pDescri->m_CaptionVerticalAlign));
	}
	if (!m_pControl)
		return;
	int nCaptionW = pDescri->m_CaptionWidth;
	if (nCaptionW != NULL_COORD)
	{
		CSize aSize(nCaptionW, 0);
		CWnd* pParent = GetWnd()->GetParent();
		SafeMapDialog(pParent->m_hWnd, aSize);//trasformo in pixel
		nCaptionW = aSize.cx;
	}
	m_pControl->SetCtrlLabelDefaultPosition(
		pDescri->m_CaptionHorizontalAlign,
		pDescri->m_CaptionVerticalAlign,
		CParsedCtrl::Left,
		nCaptionW);
};

//----------------------------------------------------------------------------
ETextAlign MParsedControl::CaptionAlign::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return (ETextAlign)pDescri->m_CaptionHorizontalAlign;

	return ETextAlign::Right;
}
//----------------------------------------------------------------------------
void MParsedControl::CaptionAlign::set(ETextAlign value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_CaptionHorizontalAlign != (::TextAlignment)value)
	{
		pDescri->m_CaptionHorizontalAlign = (::TextAlignment)value;
		pDescri->SetUpdated(&(pDescri->m_CaptionHorizontalAlign));
	}
	if (!m_pControl)
		return;
	int nCaptionW = pDescri->m_CaptionWidth;
	if (nCaptionW != NULL_COORD)
	{
		CSize aSize(nCaptionW, 0);
		CWnd* pParent = GetWnd()->GetParent();
		SafeMapDialog(pParent->m_hWnd, aSize);//trasformo in pixel
		nCaptionW = aSize.cx;
	}
	m_pControl->SetCtrlLabelDefaultPosition(
		pDescri->m_CaptionHorizontalAlign,
		pDescri->m_CaptionVerticalAlign,
		CParsedCtrl::Left,
		nCaptionW);
};
//----------------------------------------------------------------------------
FormatterStyle^ MParsedControl::Formatter::get()
{
	if (!m_pControl)
		return nullptr;

	const ::Formatter* pCurrent = m_pControl->GetCurrentFormatter();
	if (pCurrent)
		return gcnew FormatterStyle(AfxGetFormatStyleTable()->GetFormatter(pCurrent->GetName(), NULL));

	return nullptr;
}

//----------------------------------------------------------------------------
void MParsedControl::Formatter::set(FormatterStyle^ value)
{
	if (value == nullptr)
		return;

	::FormatIdx aIdx = AfxGetFormatStyleTable()->GetFormatIdx(CString(value->StyleName));
	if (aIdx >= 0 && m_pControl)
		m_pControl->AttachFormatter(aIdx);
	else
		ASSERT(FALSE);
}

//----------------------------------------------------------------------------
void MParsedControl::DataSource::set(String^ dataSource)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		pDescri->m_pBindings->m_strDataSource = dataSource;
	}

}
//----------------------------------------------------------------------------
String^ MParsedControl::DataSource::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);

	return "";
}



//----------------------------------------------------------------------------
IDataBinding^ MParsedControl::DataBinding::get()
{
	if (dataBinding != nullptr || !m_pControl || !m_pControl->m_pSqlRecord || Parent == nullptr)
		return dataBinding;

	::DataObj* pDataObj = m_pControl->GetCtrlData();
	if (!pDataObj)
		return dataBinding;

	WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
	if (container->Document == nullptr)
		return dataBinding;

	MDocument^ document = (MDocument^)container->Document;
	IDataManager^ dataManager = document->GetDataManager(m_pControl->m_pSqlRecord);
	MSqlRecord^ mSqlRecord = dataManager == nullptr ? nullptr : (MSqlRecord^)dataManager->Record;

	if (mSqlRecord == nullptr)
		mSqlRecord = gcnew MSqlRecord(m_pControl->m_pSqlRecord);

	System::String^ colName = gcnew System::String(m_pControl->m_pSqlRecord->GetColumnName(pDataObj));
	IRecordField^ recField = mSqlRecord->GetField(colName);
	MDataObj^ mDataObj = recField == nullptr ? nullptr : (MDataObj^)recField->DataObj;

	if (mDataObj == nullptr)
		mDataObj = MDataObj::Create(pDataObj);

	dataBinding = gcnew FieldDataBinding(mDataObj, dataManager);
	if (mDataObj != nullptr)
		((EasyBuilderComponent^)mDataObj)->AddReferencedBy(this->SerializedName);

	return dataBinding;
}

//----------------------------------------------------------------------------
void MParsedControl::DataBinding::set(IDataBinding^ dataBinding)
{
	if (this->dataBinding != nullptr)
	{
		MDataObj^ data = (MDataObj^)this->dataBinding->Data;
		if (data != nullptr)
			data->RemoveReferencedBy(this->SerializedName);

		EasyBuilderComponent^ parent = (EasyBuilderComponent^)this->dataBinding->Parent;
		if (parent != nullptr)
			parent->RemoveReferencedBy(this->SerializedName);

	}

	if (dataBinding == nullptr || dataBinding->Data == nullptr)
	{
		this->dataBinding = nullptr;
		if (m_pControl && !HasCodeBehind)
			m_pControl->Attach((::DataObj*)NULL);
		return;
	}

	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	FieldDataBinding^ fds = (FieldDataBinding^)dataBinding;
	ASSERT(fds);
	MDataObj^ data = (MDataObj^)fds->Data;
	::DataObj* pDataObj = data == nullptr ? NULL : data->GetDataObj();

	if (data != nullptr)
		data->AddReferencedBy(this->SerializedName);
	EasyBuilderComponent^ parent = (EasyBuilderComponent^)fds->Parent;
	if (parent != nullptr)
		parent->AddReferencedBy(this->SerializedName);

	if (pDataObj == m_pControl->GetCtrlData())
	{
		this->dataBinding = fds;
		return;
	}

	if (
		!pDataObj ||
		m_pControl->CheckDataObjType(pDataObj) ||
		(
			pDataObj->GetDataType().m_wType == DATA_ENUM_TYPE &&
			m_pControl->GetDataType().m_wType == DATA_ENUM_TYPE &&
			m_pControl->GetDataType().m_wTag == 0
			)
		)
	{
		m_pControl->Attach(data->GetDataObj());
		m_pControl->m_pSqlRecord = fds->Record == nullptr ? NULL : ((MSqlRecord^)fds->Record)->GetSqlRecord();
		m_pControl->UpdateCtrlView();
		this->dataBinding = fds;

		// invalido l'eventuale lista di oggetti che dipendono dal data type
		if (IItemsSourceConsumer::typeid->IsInstanceOfType(this))
			((IItemsSourceConsumer^)this)->RefreshContentByDataType();

		//Imposto la maxlenght in base alla dimensione del campo di database 
		// la lunghezza è significativa solo x i campi stringa per gli altri rischia
		// invece di impostare lunghezze errate rispetto alla rappresentazione
		if (m_pControl->m_pSqlRecord && pDataObj->GetDataType() == DATA_STR_TYPE)
			MaxLength = m_pControl->m_pSqlRecord->GetColumnLength(pDataObj);
	}
	else
		ASSERT(FALSE);
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::Text::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && !pDescri->m_strText.IsEmpty())
		return gcnew System::String(pDescri->m_strText);

	if (!m_pControl)
		return gcnew System::String("");

	CString str;
	m_pControl->GetValue(str);
	return gcnew System::String(str);
}

//----------------------------------------------------------------------------
void MParsedControl::Text::set(System::String^ value)
{
	if (m_pControl)
		m_pControl->SetValue(CString(value));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strText = value;
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::ControlLabel::get()
{
	if (!m_pControl)
		return __super::ControlLabel;

	System::String^  name = gcnew System::String(m_pControl->GetCtrlName());
	array<System::String^>^ pieces = name->Split({ '_' });
	int size = pieces->Length;
	if (size <= 0)
		return __super::ControlLabel;
	if (size == 1)
		return  pieces[0];
	String^ nodeLabel = pieces[size - 1] + " (Of ";
	for (int i = 0; i < size - 1; i++)
	{
		if (i != 0)
			nodeLabel += "_";
		nodeLabel += pieces[i];
	}

	return nodeLabel += ")";
}

//----------------------------------------------------------------------------
CWnd* MParsedControl::GetWnd()
{
	if (m_pControl)
		return m_pControl->GetCtrlCWnd();

	return __super::GetWnd();
}

//----------------------------------------------------------------------------
bool MParsedControl::CanCreate()
{
	return !m_pControl && !GetHandle();
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::ClassName::get()
{
	return ClassType->ClassName;
}

//----------------------------------------------------------------------------
void MParsedControl::ClassType::set(IControlClass^ controlClass)
{
	ChangeClassType(controlClass->ClassName);
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strControlClass = controlClass->ClassName;
}
//----------------------------------------------------------------------------
System::String^ MParsedControl::GetInternalName()
{
	return m_pControl
		? gcnew System::String(m_pControl->GetCtrlName())
		: __super::GetInternalName();
}


//----------------------------------------------------------------------------
void MParsedControl::SetStyle(DWORD dwRemove, DWORD dwAdd)
{
	if (controlClass != nullptr)
	{
		CRegisteredParsedCtrl* pCtrl = controlClass->GetRegInfoPtr();
		if (pCtrl)
		{
			DWORD dwStyle = ::GetWindowLong(GetHandle(), GWL_STYLE);
			DWORD dwNewStyle = (dwStyle & ~dwRemove) | dwAdd;
			if (dwStyle != dwNewStyle)
			{
				DWORD wNeededStyle = pCtrl->GetNeededStyle();
				DWORD wNotWantedStyle = pCtrl->GetNotWantedStyle();
				if (
					(wNeededStyle && ((dwNewStyle & wNeededStyle) != wNeededStyle))//è richiesto uno stile
					||
					(wNotWantedStyle && ((dwNewStyle & wNotWantedStyle) == wNotWantedStyle))//non voglio uno stile
					)
				{
					ASSERT(FALSE);
					return;
					//throw gcnew Exception(gcnew String(_TB("This style is incompatible with current control class")));
				}
			}
		}
	}
	__super::SetStyle(dwRemove, dwAdd);
}


//----------------------------------------------------------------------------
void MParsedControl::SetExStyle(DWORD dwRemove, DWORD dwAdd)
{
	if (controlClass != nullptr)
	{
		CRegisteredParsedCtrl* pCtrl = controlClass->GetRegInfoPtr();
		if (pCtrl)
		{
			DWORD dwExStyle = ::GetWindowLong(GetHandle(), GWL_EXSTYLE);
			DWORD dwNewExStyle = (dwExStyle & ~dwRemove) | dwAdd;
			if (dwExStyle != dwNewExStyle)
			{
				DWORD wNeededExStyle = pCtrl->GetNeededExStyle();
				DWORD wNotWantedExStyle = pCtrl->GetNotWantedExStyle();
				if (
					(wNeededExStyle && ((dwNewExStyle & wNeededExStyle) != wNeededExStyle))//è richiesto uno stile
					||
					(wNotWantedExStyle && ((dwNewExStyle & wNotWantedExStyle) == wNotWantedExStyle))//non voglio uno stile
					)
				{
					throw gcnew Exception(gcnew String(_TB("This style is incompatible with current control class")));
				}
			}
		}
	}
	__super::SetExStyle(dwRemove, dwAdd);
}

//----------------------------------------------------------------------------
void MParsedControl::ChangeClassType(System::String^ name)
{
	CWnd* pOldWnd = GetWnd();

	// same class check
	CString sName(name);
	CString sOldName(ClassType->ClassName);
	CString sFamilyName(this->GetType()->Name);

	CRegisteredParsedCtrl* pOldCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sOldName);
	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sName);

	if (!pCtrl || !pCtrl->GetClass() || pOldCtrl == pCtrl)
		return;

	DWORD oldStyle = pOldCtrl->GetNeededStyle();
	DWORD oldExStyle = pOldCtrl->GetNeededExStyle();
	DWORD newStyle = pCtrl->GetNeededStyle();
	DWORD newExStyle = pCtrl->GetNeededExStyle();

	//Mi tengo da parte il namespace
	CTBNamespace ns = m_pControl->GetNamespace();
	//preservo la caption originaria
	System::String^ oldCaption = Caption;

	CWnd* pParentWnd = pOldWnd->GetParent();
	UINT nID = pOldWnd->GetDlgCtrlID();
	HWND hwnd = m_pControl->UnSubclassEdit();
	CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
	int nOldPos = -1;
	if (pParsedForm)
	{
		nOldPos = pParsedForm->GetControlLinks()->Find(pOldWnd);
	}
	delete pOldWnd;
	CWnd* pNewWnd = (CWnd*)pCtrl->GetClass()->CreateObject();
	m_pControl = GetParsedCtrl(pNewWnd);

	ModifyStyle(hwnd, GWL_STYLE, oldStyle, newStyle);
	ModifyStyle(hwnd, GWL_EXSTYLE, oldExStyle, newExStyle);
	m_pControl->SubclassEdit(nID, pParentWnd);
	if (nOldPos >= 0)
		pParsedForm->GetControlLinks()->SetAt(nOldPos, pNewWnd);

	//Riassegno il namespace del control a quello appena ricreato
	m_pControl->SetNamespace(ns.ToString());
	Caption = oldCaption;//ripristino la caption
	controlClass->SetRegInfoPtr(pCtrl);
	Extensions->Service->Refresh();
}


//----------------------------------------------------------------------------
void MParsedControl::HotLink::set(MHotLink^ value)
{
	System::String^ error = nullptr;
	if (value != nullptr && !value->CanBeAttached((Microarea::TaskBuilderNet::Core::CoreTypes::DataType)CompatibleType, MaxLength, error))
	{
		Diagnostic->SetError(error);
		return;
	}

	if (hotLink != nullptr)
		hotLink->AttachedControl = nullptr;

	hotLink = value;

	if (hotLink != nullptr)
		hotLink->AttachedControl = this;

	if (m_pControl)
	{
		m_pControl->ReattachHotKeyLink(hotLink == nullptr ? NULL : hotLink->GetHotLink());
		m_pControl->ReAttachButton(ShowHotLinkButton ? BTN_DOUBLE_ID : NO_BUTTON);

		CWnd* pWnd = GetWnd();

		if (pWnd)
		{
			CRect originalRect;
			pWnd->GetWindowRect(&originalRect);
			CWnd* pParent = pWnd->GetParent();
			if (pParent)
				pParent->ScreenToClient(originalRect);


			m_pControl->DoCellPosChanging(originalRect, SWP_NOZORDER | SWP_NOMOVE);
		}
	}
}

//----------------------------------------------------------------------------
String^ MParsedControl::HotLinkName::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri || !pDescri->m_pBindings)
		return "";

	HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
	if (!pInfo)
		return "";
	return gcnew String(pInfo->m_strName);
}

//----------------------------------------------------------------------------
void MParsedControl::HotLinkName::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	if (!pDescri->m_pBindings)
		pDescri->m_pBindings = new BindingInfo();
	HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
	if (!pInfo)
	{
		pInfo = new HotLinkInfo;
		pDescri->m_pBindings->m_pHotLink = pInfo;
	}

	pInfo->m_strName = value;
	if (System::String::IsNullOrEmpty(value))
		HotLinkNs = value;
}

//----------------------------------------------------------------------------
void MParsedControl::HotLinkNs::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
		if (!pInfo)
		{
			pInfo = new HotLinkInfo;
			pDescri->m_pBindings->m_pHotLink = pInfo;
		}

		pInfo->m_strNamespace = value;
	}

}
//----------------------------------------------------------------------------
String^ MParsedControl::HotLinkNs::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
	{
		HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
		if (!pInfo)
			return "";
		return gcnew String(pInfo->m_strNamespace);
	}

	return "";
}
/*
//----------------------------------------------------------------------------
EControlButton MParsedControl::Button::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return EControlButton::Default;
	}

	return (EControlButton)m_pControl->GetButtonIDBmp();
}

//----------------------------------------------------------------------------
void MParsedControl::Button::set(EControlButton value)
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
	{
		ASSERT(FALSE);
		return;
	}
	if (!pDescri->m_pBindings)
		pDescri->m_pBindings = new BindingInfo();
	m_pControl->ReAttachButton((int)value);
	pDescri->m_pBindings->m_nButtonId = (int) value;
}
*/
//----------------------------------------------------------------------------
bool MParsedControl::Create(Control^ parent, System::Drawing::Rectangle rect, System::String^ controlName, System::String^ className)
{
	CString sFamilyName(this->GetType()->Name);
	CString sControlClassName(className);

	if (sControlClassName.IsEmpty())
		sControlClassName = AfxGetParsedControlsRegistry()->GetFamilyDefaultControl(sFamilyName);

	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sControlClassName);
	if (!pCtrl)
		return false;

	DWORD style = WS_CHILD | WS_VISIBLE;
	DWORD exStyle = pCtrl->GetNeededExStyle();
	CRuntimeClass* pCtrlClass = pCtrl->GetClass();
	OnCreateStyles(style, exStyle);
	style |= pCtrl->GetNeededStyle();

	// recuper il control
	m_pControl = GetParsedCtrl(pCtrlClass->CreateObject());
	ASSERT(m_pControl);

	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls);
	CWnd* pWnd = CWnd::FromHandle((HWND)(int)parent->Handle);
	if (!m_pControl->Create(style, CRect(rect.Left, rect.Top, rect.Width, rect.Height), pWnd, nID))
	{
		delete m_pControl;
		m_pControl = NULL;
		TRACE("fail to create control %d\n", nID);
		ASSERT(FALSE);
		return false;
	}
	if (exStyle)
		m_pControl->GetCtrlCWnd()->ModifyStyleEx(0, exStyle);

	Handle = (System::IntPtr) m_pControl->GetCtrlCWnd()->m_hWnd;
	HasCodeBehind = false;
	return true;
}

//----------------------------------------------------------------------------
bool MParsedControl::Create(IWindowWrapperContainer^ parentWindow, Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	if (!pParentWnd)
		return false;

	CPoint aPt(location.X, location.Y);
	CFont* pObjectFont = AfxGetThemeManager()->GetControlFont();

	CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
	if (!pParsedForm)
		return false;

	// main control data
	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	CString idOrNamespace;
	EasyBuilderComponent^ parent = dynamic_cast<EasyBuilderComponent^>(parentWindow);
	if (parent && parent->DesignModeType == EDesignMode::Static)
		idOrNamespace = CString(Id);
	else
		idOrNamespace = aNamespace.ToString();

	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(idOrNamespace, TbControls);

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	CString sFamilyName(this->GetType()->Name);
	CString sControlClassName(className);

	if (sControlClassName.IsEmpty())
		sControlClassName = AfxGetParsedControlsRegistry()->GetFamilyDefaultControl(sFamilyName);

	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sControlClassName);
	if (!pCtrl)
		return false;

	DWORD style = WS_CHILD | WS_VISIBLE;
	CRuntimeClass* pCtrlClass = pCtrl->GetClass();
	DWORD exStyle = pCtrl->GetNeededExStyle();
	OnCreateStyles(style, exStyle);
	style |= pCtrl->GetNeededStyle();

	FieldDataBinding^ fds = (FieldDataBinding^)DataBinding;
	::SqlRecord* pSqlRecord = fds == nullptr || fds->Record == nullptr ? NULL : ((MSqlRecord^)fds->Record)->GetSqlRecord();
	::DataObj*  pDataObj = fds == nullptr || fds->Data == nullptr ? NULL : ((MDataObj^)fds->Data)->GetDataObj();

	m_pControl = ::AddLinkAndCreateControl
	(
		aNamespace.GetObjectName(),
		style,
		CRect(aPt, aSize),
		pParentWnd,
		pParsedForm->GetControlLinks(),
		nID,
		pSqlRecord,
		pDataObj,
		pCtrlClass,
		NULL
	);
	//assegno il tema bcg
	//m_pControl->GetCtrlCWnd()->SendMessage(BCGM_CHANGEVISUALMANAGER);
	if (!m_pControl)
		return false;

	if (exStyle)
		m_pControl->GetCtrlCWnd()->ModifyStyleEx(0, exStyle);

	Handle = (System::IntPtr) m_pControl->GetCtrlCWnd()->m_hWnd;
	HasCodeBehind = false;
	return m_pControl != NULL;
}

//----------------------------------------------------------------------------
bool MParsedControl::CanDropTarget(Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return CanChangeProperty(MParsedControlSerializer::HotLinkPropertyName);

	return CanUpdateTarget(droppedObject);
}

//----------------------------------------------------------------------------
IControlClass^ MParsedControl::ClassType::get()
{
	return controlClass;
}

//----------------------------------------------------------------------------
bool IsSameOrSubclass(Type^ potentialDescendant, Type^ potentialBase)
{
	return potentialDescendant->IsSubclassOf(potentialBase)
		|| potentialDescendant == potentialBase;
}

//----------------------------------------------------------------------------
Object^ MParsedControl::MinValue::get()
{
	if (!m_pControl)
		return nullptr;

	MDataObj^ dataObj = MDataObj::Create(m_pControl->GetMinValue());
	return dataObj ? dataObj->Value : nullptr;
}

//----------------------------------------------------------------------------
void MParsedControl::MinValue::set(Object^ minValue)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	Type^ type = minValue->GetType();

	if (IsSameOrSubclass(type, Int32::typeid))
		m_pControl->SetMinValue(::DataInt((int)minValue));
	else if (IsSameOrSubclass(type, String::typeid))
		m_pControl->SetMinValue(::DataStr(CString(minValue->ToString())));
	else if (IsSameOrSubclass(type, Double::typeid))
		m_pControl->SetMinValue(::DataDbl((double)minValue));
	else if (IsSameOrSubclass(type, DateTime::typeid))
	{
		DateTime^ dateTime = (DateTime^)minValue;
		::DataDate dataDate = ::DataDate(dateTime->Day, dateTime->Month, dateTime->Year, dateTime->Hour, dateTime->Minute, dateTime->Second);
		dataDate.SetFullDate();
		m_pControl->SetMinValue(dataDate);
	}
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::MinValueStr::get()
{
	CWndObjDescription* descr = GetWndObjDescription();
	if (descr)
		return gcnew String(descr->m_sMinValue);
	if (MinValue)
		return MinValue->ToString();
	return "";
}

//----------------------------------------------------------------------------
void MParsedControl::MinValueStr::set(System::String^ minValue)
{
	SetMinMaxStr(minValue, true);
}

//----------------------------------------------------------------------------
Object^ MParsedControl::MaxValue::get()
{
	if (!m_pControl)
		return nullptr;

	MDataObj^ dataObj = MDataObj::Create(m_pControl->GetMaxValue());
	return dataObj ? dataObj->Value : nullptr;
}

//----------------------------------------------------------------------------
void MParsedControl::MaxValue::set(Object^ maxValue)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	Type^ type = maxValue->GetType();

	if (IsSameOrSubclass(type, Int32::typeid))
		m_pControl->SetMaxValue(::DataInt((int)maxValue));
	else if (IsSameOrSubclass(type, String::typeid))
		m_pControl->SetMaxValue(::DataStr(CString(maxValue->ToString())));
	else if (IsSameOrSubclass(type, Double::typeid))
		m_pControl->SetMaxValue(::DataDbl((double)maxValue));
	else if (IsSameOrSubclass(type, DateTime::typeid))
	{
		DateTime^ dateTime = (DateTime^)maxValue;
		::DataDate dataDate = ::DataDate(dateTime->Day, dateTime->Month, dateTime->Year, dateTime->Hour, dateTime->Minute, dateTime->Second);
		dataDate.SetFullDate();
		m_pControl->SetMaxValue(dataDate);
	}
}

//----------------------------------------------------------------------------
System::String^ MParsedControl::MaxValueStr::get()
{
	//return MaxValue == nullptr ? "" : MaxValue->ToString();
	CWndObjDescription* descr = GetWndObjDescription();
	if (descr)
		return gcnew String(descr->m_sMaxValue);
	if (MaxValue)
		return MaxValue->ToString();
	return "";
}

//----------------------------------------------------------------------------
void MParsedControl::MaxValueStr::set(System::String^ maxValue)
{
	SetMinMaxStr(maxValue, false);
}

//----------------------------------------------------------------------------
int MParsedControl::MaxLength::get()
{
	return m_pControl ? m_pControl->GetCtrlMaxLen() : 0;
}

//----------------------------------------------------------------------------
void MParsedControl::MaxLength::set(int length)
{
	if (m_pControl)
		m_pControl->SetCtrlMaxLen((int)length);
	else
		ASSERT(FALSE);
	return;
}

void MParsedControl::SetMinMaxStr(System::String^ value, bool isMixValue)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	DataType valueType = m_pControl->GetDataType();
	CWndObjDescription* descr = GetWndObjDescription();
	::DataObj* pValue = ::DataObj::DataObjCreate(m_pControl->GetDataType());//creo il dataobj a partire dal tipo di dato
	double doubleParsed = 0.0;
	int intParse, compare = 0;
	DateTime dateParsed;
	System::String^ valueCopy = nullptr;
	System::String^  doubleDot = nullptr;
	CultureInfo^ provider = CultureInfo::InvariantCulture;
	NumberStyles style = NumberStyles::Number;



	if (value != nullptr && value != "")
	{

		switch (valueType) {
		case(DATA_INT_TYPE):
			Int32::TryParse(value, intParse);
			if (intParse.ToString() != value)
				throw gcnew Exception(gcnew String(_TB("This value is incompatible with current control class. Expected integer")));
			pValue->Assign(CString(value));		//assegno il tipo di dato in formato stringa, il dataobj sa convertire nel proprio tipo

			break;
		case(DATA_DBL_TYPE):
			Double::TryParse(value, style, provider, doubleParsed);
			doubleDot = doubleParsed.ToString(provider);
			if (doubleDot != value)
				throw gcnew Exception(gcnew String(_TB("This value is incompatible with current control class.\nFormat expected: integers dot decimals (nnn.dd) ")));
			pValue->Assign(CString(value));		//assegno il tipo di dato in formato stringa, il dataobj sa convertire nel proprio tipo

			break;
		case(DATA_DATE_TYPE):
		{
			BOOL bValid = FALSE;
			CString s(value);
			pValue->Assign(s, UNDEF_FORMAT);		//assegno il tipo di dato in formato stringa, il dataobj sa convertire nel proprio tipo
			CString s1 = pValue->Str(1);
			bValid = s1 == s;
			if (!bValid)
			{
				if (s[0] != _T('D') && s[0] != _T('T') && s[0] != _T('F'))
				{
					s.Insert(0, _T('D'));
					pValue->Assign(s);
					bValid = pValue->Str() == s;

				}
			}
			if (!bValid)
				throw gcnew Exception(gcnew String(_TB("This value is incompatible with current control class.\nFormat expected: 'dd/mm/yyyy' or 'dd/mm/yyyy hh::mm:ss' ")));

			break;
		}
		}
	}

	if (isMixValue)
	{
		m_pControl->SetMinValue(*pValue);
		if (descr)
			descr->m_sMinValue = valueCopy ? valueCopy : value;
	}
	else
	{
		m_pControl->SetMaxValue(*pValue);
		if (descr)
			descr->m_sMaxValue = valueCopy ? valueCopy : value;
	}
	delete pValue;

}
//----------------------------------------------------------------------------
EControlStyle MParsedControl::ControlStyle::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return (EControlStyle)(pDesc ? pDesc->m_ControlStyle : CS_NONE);
}

//----------------------------------------------------------------------------
void MParsedControl::ControlStyle::set(EControlStyle value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_ControlStyle = (::ControlStyle)value;
}
//----------------------------------------------------------------------------
int MParsedControl::NumberOfDecimal::get()
{
	return m_pControl ? m_pControl->GetCtrlNumDec() : 0;
}

//----------------------------------------------------------------------------
void MParsedControl::NumberOfDecimal::set(int nDec)
{
	if (!m_pControl)
		return;
	CWndObjDescription* descr = GetWndObjDescription();
	if (descr)
		descr->m_nNumberDecimal = nDec;

	if (m_pControl)
		m_pControl->SetCtrlNumDec((int)nDec);
	else
		ASSERT(FALSE);
	return;
}

////----------------------------------------------------------------------------
//void MParsedControl::MoveHyperLink(CWnd* pWndToMove, Point positionToMove, CRect parsedControlOriginalRect)
//{
//	CRect hyperLinkOriginalRect;
//	pWndToMove->GetWindowRect(&hyperLinkOriginalRect);
//
//	int originalWidth = hyperLinkOriginalRect.right - hyperLinkOriginalRect.left;
//	int originalEight = hyperLinkOriginalRect.bottom - hyperLinkOriginalRect.top;
//
//	CRect stateCtrlRect;
//	//posizione dello statectrl addizionale se presente
//	pWndToMove->GetWindowRect(&stateCtrlRect);
//
//	//questa è la distanza dello statectrl dal topleft del parsed control
//	int xOffset = stateCtrlRect.left - parsedControlOriginalRect.left;
//	int yOffset = parsedControlOriginalRect.bottom - stateCtrlRect.bottom;
//	System::Drawing::Size sz = System::Drawing::Size(stateCtrlRect.right - stateCtrlRect.left, stateCtrlRect.bottom - stateCtrlRect.top);
//
//	//Se ha uno statectrl addizionale, sposto anche lui
//	pWndToMove->SetWindowPos(NULL, positionToMove.X + xOffset, positionToMove.Y + yOffset, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
//}

//----------------------------------------------------------------------------
void MParsedControl::MoveAuxControl(CWnd* pWndToMove, const CSize& offset)
{
	CRect ctrlRect;
	//posizione dello statectrl addizionale se presente
	pWndToMove->GetWindowRect(&ctrlRect);
	CWnd* pParent = pWndToMove->GetParent();
	if (pParent)
		pParent->ScreenToClient(ctrlRect);
	ctrlRect.OffsetRect(offset);
	//Se ha uno statectrl addizionale, sposto anche lui
	pWndToMove->SetWindowPos(NULL, ctrlRect.left, ctrlRect.top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
}

//----------------------------------------------------------------------------
bool MParsedControl::CanChangeProperty(System::String^ propertyName)
{
	//ES: posso assegnare l'hotlink solo se ho prima impostato il databinding
	if (propertyName == MParsedControlSerializer::HotLinkPropertyName)
		return DataBinding != nullptr;

	//ESD: posso assegnare l'hotlinkNs solo se ho prima impostato l'hotlinkName perchè è chiave
	if (propertyName == MParsedControlSerializer::HotLinkNsPropName)
		return !System::String::IsNullOrEmpty(HotLinkName);

	//posso modificare il databinding solo se non ho l'hotlink agganciato
	if (propertyName == EasyBuilderSerializer::DataBindingPropertyName)
		return HotLink == nullptr;

	if (propertyName == "MinValueStr" || propertyName == "MaxValueStr")
	{
		System::String^ type = this->controlClass->CompatibleTypeName;
		return (type == "integer" || type == "double" ||
			type->Contains("date") || (Regex::IsMatch(type, "time", RegexOptions::IgnoreCase)));
	}

	if (propertyName == "PartAnchor")
		return false;

	if (DesignModeType == EDesignMode::Runtime)
	{
		if (propertyName == MParsedControlSerializer::ShowHotLinkButtonPropertyName)
			return HotLink != nullptr;
	}
	if (propertyName == "ControlLabel")
		return false;
	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
void MParsedControl::ResizeAuxButton(CWnd* pWndToMove, CRect parsedControlOriginalRect, CRect newParsedControlRect)
{
	//posizione di un evenutale button addizionale (statebutton o linkbutton) se presente, recupero 
	//la distanza dal parsedcontrol
	CRect buttonRect;
	pWndToMove->GetWindowRect(&buttonRect);

	int distance = buttonRect.left - parsedControlOriginalRect.right;

	//Calcolo la nuova posizione del button o link addizionale
	CPoint pt(newParsedControlRect.right + distance, newParsedControlRect.top);

	pWndToMove->GetParent()->ScreenToClient(&pt);

	//Se ha un button addizionale (ad esempio un calendar), sposto anche lui
	pWndToMove->SetWindowPos(NULL, pt.x, pt.y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
}

//----------------------------------------------------------------------------
void MParsedControl::ResizeHyperLink(CWnd* pWndToMove, System::Drawing::Size sz)
{
	pWndToMove->SetWindowPos(NULL, 1, 1, sz.Width - 5, sz.Height - 5, SWP_NOMOVE | SWP_NOZORDER);
}

//----------------------------------------------------------------------------
Type^ MParsedControl::GetDefaultControlType(IDataType^ dataType, bool readOnly, System::String^% controlClass)
{
	::DataType aDataType(dataType->Type, dataType->Tag);

	if (readOnly)
	{
		CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(MParsedStatic::typeid->FullName, aDataType);
		// se ne trovo uno della famiglia lo uso, altrimenti uso l'unico tipo che ho
		if (pCtrl)
		{
			controlClass = gcnew System::String(pCtrl->GetName());
			return MParsedStatic::typeid;
		}
	}

	const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(aDataType);
	if (!pFamily)
		return nullptr;

	CString sControlClass = pFamily->GetDefaultControl(aDataType);
	controlClass = gcnew System::String(sControlClass);
	return Type::GetType(gcnew System::String(pFamily->GetQualifiedTypeName()));
}

//----------------------------------------------------------------------------
bool MParsedControl::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case WM_DESTROY:
	{
		//se il wrapper ha creato il controllo, lo deve anche distruggere
		//non metto a null il puntatore così poi verrà distrutto nella dispose
		if (HasCodeBehind)
			m_pControl = NULL;
		break;
	}
	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::ValueChanged)
		{
			if (IsFromMyParentView(m))
			{
				EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
				ValueChanged(this, args);
				return true;//mangio il messaggio (che è mio non deve essere ruotato a MFC)
			}
		}
		break;
	}
	}
	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
bool MParsedControl::IsFromMyParentView(Message% m)
{
	if (this->parent == nullptr)
	{
		return false;
	}

	int hashCode = (int)m.LParam;

	return IsFromMyParentViewRecursive(this->parent, hashCode);
}

//----------------------------------------------------------------------------
bool MParsedControl::IsFromMyParentViewRecursive(IWindowWrapperContainer^ windowWrapperContainer, int hashCode)
{
	if (windowWrapperContainer == nullptr)
	{
		return false;
	}

	if (windowWrapperContainer->GetHashCode() == hashCode)
	{
		return true;
	}

	EasyBuilderControl^ easyBuilderControl = dynamic_cast<EasyBuilderControl^> (windowWrapperContainer);
	if (easyBuilderControl == nullptr)
	{
		return false;
	}

	IWindowWrapperContainer^ parentWrapperContainer = easyBuilderControl->Parent;
	if (parentWrapperContainer == nullptr)
	{
		return false;
	}

	return IsFromMyParentViewRecursive(parentWrapperContainer, hashCode);
}

//----------------------------------------------------------------------------
IDataType^ MParsedControl::CompatibleType::get()
{
	return ClassType->CompatibleType;
}

//----------------------------------------------------------------------------
Type^ MParsedControl::ExcludedBindParentType::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
bool MParsedControl::ShowHotLinkButton::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return pDescri->m_pBindings->m_nButtonId != NO_BUTTON;
	return showHotLinkButton;
}

//----------------------------------------------------------------------------
void MParsedControl::ShowHotLinkButton::set(bool value)
{
	int btn = value ? BTN_DOUBLE_ID : NO_BUTTON;
	showHotLinkButton = value;
	if (m_pControl)
		m_pControl->ReAttachButton(btn);
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo();
		pDescri->m_pBindings->m_nButtonId = btn;
	}
}

//-----------------------------------------------------------------------------
void MParsedControl::Add(IComponent^ component)
{
	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void MParsedControl::Add(IComponent^ component, System::String^ name)
{
	if (name != nullptr && component->Site != nullptr)
		component->Site->Name = name;

	components->Add(component);
	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentAdded(this, component);
}

//-----------------------------------------------------------------------------
void MParsedControl::Remove(IComponent^ component)
{
	//Rimuove il component attuale dalla lista dei components del container
	components->Remove(component);

	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentRemoved(this, component);
}

//-----------------------------------------------------------------------------
void MParsedControl::SetParent(IWindowWrapperContainer^	parent)
{
	this->parent = parent;
}

//----------------------------------------------------------------------------
void MParsedControl::DelayedPartsAnchor()
{
	__super::DelayedPartsAnchor();
	if (m_pControl && m_pControl->GetControlLabel())
	{
		CString strText;
		m_pControl->GetControlLabel()->GetWindowText(strText);
		m_pControl->SetCtrlCaption(strText);
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedEdit Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedEdit::MParsedEdit(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MParsedEdit::MParsedEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void MParsedEdit::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	// l'autoscroll e' solo per gli edit di default, se uno ha degli
	// stili specifici va definito in quelli specifici
	if (styles == 0)
		styles |= ES_AUTOHSCROLL;
	styles |= WS_TABSTOP;
	exStyles |= WS_EX_CLIENTEDGE;
}

//----------------------------------------------------------------------------
int MParsedEdit::Chars::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_nChars;
	return -1;
}

//----------------------------------------------------------------------------
void MParsedEdit::Chars::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_nChars = value;
}

//----------------------------------------------------------------------------
Color MParsedEdit::BackColor::get()
{
	if (!m_pControl)
		return Color::Empty;

	return System::Drawing::ColorTranslator::FromWin32(((CParsedEdit*)m_pControl)->GetBkgColor());
}

//----------------------------------------------------------------------------
void MParsedEdit::BackColor::set(Color color)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	CParsedEdit* pCtrl = (CParsedEdit*)m_pControl;
	if (color == Color::Empty)
	{
		pCtrl->SetColored(FALSE);
	}
	else
	{
		COLORREF ref = System::Drawing::ColorTranslator::ToWin32(color);
		pCtrl->SetColored(TRUE);
		pCtrl->SetBkgColor(ref);
	}
	pCtrl->Invalidate();
}

//----------------------------------------------------------------------------
Color MParsedEdit::ForeColor::get()
{
	if (!m_pControl)
		return Color::Empty;

	return System::Drawing::ColorTranslator::FromWin32(((CParsedEdit*)m_pControl)->GetTextColor());
}

//----------------------------------------------------------------------------
void MParsedEdit::ForeColor::set(Color color)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}

	COLORREF ref = System::Drawing::ColorTranslator::ToWin32(color);

	CParsedEdit* pCtrl = (CParsedEdit*)m_pControl;

	pCtrl->SetColored(TRUE);
	pCtrl->SetTextColor(ref);
	pCtrl->Invalidate();
}

//----------------------------------------------------------------------------
EResizableControl MParsedEdit::Resizable::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		return (EResizableControl)((CEditObjDescription*)pDescri)->m_Resizable;

	ResizableCtrl* pControl = dynamic_cast<ResizableCtrl*>(GetWnd());
	return pControl ? (EResizableControl)pControl->GetAutoSizeCtrl() : EResizableControl::None;
}

//----------------------------------------------------------------------------
void MParsedEdit::Resizable::set(EResizableControl value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) && ((CEditObjDescription*)pDescri)->m_Resizable != (::ResizableControl)value)
	{
		((CEditObjDescription*)pDescri)->m_Resizable = (::ResizableControl)value;
		((CEditObjDescription*)pDescri)->SetUpdated(&(((CEditObjDescription*)pDescri)->m_Resizable));
	}

	ResizableCtrl* pControl = dynamic_cast<ResizableCtrl*>(GetWnd());
	if (!pControl)
		return;
	pControl->SetAutoSizeCtrl((int)value);
	pControl->InitSizeInfo(GetWnd());
}

//----------------------------------------------------------------------------
bool MParsedEdit::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::StateButtonClicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			MStateObject^ mStateObject = (MStateObject^)Extensions["StateButton"];
			if (mStateObject != nullptr && !mStateObject->EmptyComponent)
			{
				mStateObject->StateButtonClicked
				(
					this,
					gcnew MStateObjectEventArgs(mStateObject->DataObj)
				);
			}
			return true;//mangio il messaggio (che è mio non deve essere ruotato a MFC)
		}
		break;
	}
	}
	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
bool MParsedEdit::CanUpdateTarget(Type^ droppedType)
{
	return !HasCodeBehind && droppedType == MPushButton::typeid && ((MStateObject^)Extensions["StateButton"])->Style == MStateObject::ButtonStyle::NoButton;
}

//----------------------------------------------------------------------------
void MParsedEdit::UpdateTargetFromDrop(Type^ droppedType)
{
	MStateObject^ stateButton = (MStateObject^)Extensions["StateButton"];

	if (CanUpdateTarget(droppedType) && stateButton != nullptr)
	{
		stateButton->Style = MStateObject::ButtonStyle::AutoManual;
		PropertyChangingNotifier::OnComponentPropertyChanged(stateButton->Site, stateButton, "Style", MStateObject::ButtonStyle::NoButton, stateButton->Style);
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, stateButton->Name, nullptr, stateButton);
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedStatic Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedStatic::MParsedStatic(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MParsedStatic::MParsedStatic(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void MParsedStatic::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= SS_LEFT;
}

//----------------------------------------------------------------------------
ELinePos MParsedStatic::LinePosition::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CParsedLabelDescription)))
		return (ELinePos)((CParsedLabelDescription*)pDescri)->m_LinePos;

	CLabelStatic* pStatic = dynamic_cast<CLabelStatic*>(m_pControl);
	if (pStatic)
		return (ELinePos)pStatic->GetLinePosition();
	return ELinePos::NONE;
}
//----------------------------------------------------------------------------
void MParsedStatic::LinePosition::set(ELinePos value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CParsedLabelDescription)))
		((CParsedLabelDescription*)pDescri)->m_LinePos = (CLabelStatic::ELinePos)value;

	CLabelStatic* pStatic = dynamic_cast<CLabelStatic*>(m_pControl);
	if (pStatic)
		pStatic->SetLinePosition((CLabelStatic::ELinePos)value);
}

//----------------------------------------------------------------------------
bool MParsedStatic::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "LinePosition")
		return dynamic_cast<CLabelStatic*>(m_pControl) != NULL;

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
Color MParsedStatic::BackColor::get()
{
	if (!m_pControl)
		return Color::Empty;

	return System::Drawing::ColorTranslator::FromWin32(((CParsedStatic*)m_pControl)->GetBkgColor());
}

//----------------------------------------------------------------------------
void MParsedStatic::BackColor::set(Color color)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}
	CParsedStatic* pCtrl = (CParsedStatic*)m_pControl;
	if (color == Color::Empty)
	{
		pCtrl->SetColored(FALSE);
	}
	else
	{
		COLORREF ref = System::Drawing::ColorTranslator::ToWin32(color);
		pCtrl->SetColored(TRUE);
		pCtrl->SetBkgColor(ref);
	}
	pCtrl->Invalidate();
}

//----------------------------------------------------------------------------
Color MParsedStatic::ForeColor::get()
{
	if (!m_pControl)
		return Color::Empty;

	return System::Drawing::ColorTranslator::FromWin32(((CParsedStatic*)m_pControl)->GetTextColor());
}

//----------------------------------------------------------------------------
void MParsedStatic::ForeColor::set(Color color)
{
	if (!m_pControl)
	{
		ASSERT(FALSE);
		return;
	}

	COLORREF ref = System::Drawing::ColorTranslator::ToWin32(color);

	CParsedStatic* pCtrl = (CParsedStatic*)m_pControl;

	pCtrl->SetColored(TRUE);
	pCtrl->SetTextColor(ref);
	pCtrl->Invalidate();
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedCombo Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedCombo::MParsedCombo(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
	itemsSource = gcnew List<MDataObj^>();
	AttachCallBackFunction();
}

//----------------------------------------------------------------------------
MParsedCombo::MParsedCombo(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{
	itemsSource = gcnew List<MDataObj^>();

	AttachCallBackFunction();
}

//----------------------------------------------------------------------------
MParsedCombo::~MParsedCombo()
{
	this->!MParsedCombo();
	GC::SuppressFinalize(this);
}
//----------------------------------------------------------------------------
MParsedCombo::!MParsedCombo()
{
	delete itemsSource;
	if (fillComboHandle.IsAllocated)
		fillComboHandle.Free();
	fillComboCallBack = nullptr;
}

//----------------------------------------------------------------------------
void MParsedCombo::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP | WS_VSCROLL | CBS_DROPDOWN | CBS_SORT;
}

//----------------------------------------------------------------------------
int MParsedCombo::MaxRowsNumber::get()
{
	return m_pControl ? ((CParsedCombo*)m_pControl)->GetMaxRows() : 0;
}

//----------------------------------------------------------------------------
void MParsedCombo::MaxRowsNumber::set(int nRows)
{
	if (m_pControl)
		((CParsedCombo*)m_pControl)->SetMaxRows(nRows);
}

//----------------------------------------------------------------------------
int MParsedCombo::MaxItemsNumber::get()
{
	return m_pControl ? ((CParsedCombo*)m_pControl)->GetMaxItemsNo() : 0;
}

//----------------------------------------------------------------------------
void MParsedCombo::MaxItemsNumber::set(int nRows)
{
	if (m_pControl)
		((CParsedCombo*)m_pControl)->SetMaxItemsNo(nRows);
}

//----------------------------------------------------------------------------
void MParsedCombo::HotLink::set(MHotLink^ value)
{
	__super::HotLink = value;

	if (value == nullptr)
	{
		System::IntPtr funPtr = Marshal::GetFunctionPointerForDelegate(fillComboCallBack);
		((CParsedCombo*)m_pControl)->SetFillComboFuncPtr(static_cast<FILLCOMBO_FUNC>(funPtr.ToPointer()));
	}
	else
		((CParsedCombo*)m_pControl)->SetFillComboFuncPtr(NULL);
}


//----------------------------------------------------------------------------
String^ MParsedCombo::ItemSourceName::get()
{
	CComboDescription* pDescri = dynamic_cast<CComboDescription*>(GetWndObjDescription());
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceName) : "";
}

//----------------------------------------------------------------------------
void MParsedCombo::ItemSourceName::set(String^ value)
{
	CComboDescription* pDescri = dynamic_cast<CComboDescription*>(GetWndObjDescription());
	if (!pDescri)
		return;
	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceName = value;
}

//----------------------------------------------------------------------------
void MParsedCombo::ItemSourceNs::set(String^ value)
{
	CComboDescription* pDescri = dynamic_cast<CComboDescription*>(GetWndObjDescription());
	if (!pDescri)
		return;

	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceNamespace = value;

}
//----------------------------------------------------------------------------
String^ MParsedCombo::ItemSourceNs::get()
{
	CComboDescription* pDescri = dynamic_cast<CComboDescription*>(GetWndObjDescription());
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceNamespace) : "";
}
//----------------------------------------------------------------------------
System::Collections::IList^ MParsedCombo::ItemsSource::get()
{
	return itemsSource;
}

//----------------------------------------------------------------------------
void MParsedCombo::ItemsSource::set(System::Collections::IList^ value)
{
	itemsSource = value;
}

//----------------------------------------------------------------------------
bool MParsedCombo::IsItemsSourceEditable::get()
{
	// la proprietà hotlink non sarebbe sufficiente
	return m_pControl ? !m_pControl->GetHotLink() : false;
}

//----------------------------------------------------------------------------
void MParsedCombo::ResizeHyperLink(CWnd* pWndToMove, System::Drawing::Size sz)
{
	pWndToMove->SetWindowPos(NULL, 1, 1, sz.Width - 24, sz.Height - 5, SWP_NOMOVE | SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
void MParsedCombo::AttachCallBackFunction()
{
	if (!m_pControl || !((CParsedCombo*)m_pControl))
		return;

	if (fillComboHandle.IsAllocated)
		fillComboHandle.Free();

	fillComboCallBack = gcnew FillComboCallBack(this, &MParsedCombo::OnFillCombo);
	fillComboHandle = GCHandle::Alloc(fillComboCallBack);
	System::IntPtr funPtr = Marshal::GetFunctionPointerForDelegate(fillComboCallBack);
	((CParsedCombo*)m_pControl)->SetFillComboFuncPtr(static_cast<FILLCOMBO_FUNC>(funPtr.ToPointer()));
}

//-----------------------------------------------------------------------------
bool MParsedCombo::OnFillCombo()
{
	if (itemsSource == nullptr || !m_pControl || !((CParsedCombo*)m_pControl))
		return false;

	for each (MDataObj^ current in itemsSource)
		((CParsedCombo*)m_pControl)->AddAssociation(current->GetDataObj()->Str(), *current->GetDataObj());

	return true;
}

//----------------------------------------------------------------------------
bool MParsedCombo::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case WM_COMMAND:
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);
		if (nCode == CBN_SELCHANGE)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			SelectedIndexChanged(this, args);
			if (args->Handled)
				return true;//mangio il messaggio
		}
	}
	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::StateButtonClicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			MStateObject^ mStateObject = (MStateObject^)Extensions["StateButton"];

			if (mStateObject != nullptr && !mStateObject->EmptyComponent)
			{
				mStateObject->StateButtonClicked
				(
					this,
					gcnew MStateObjectEventArgs(mStateObject->DataObj)
				);
			}
			return true;//mangio il messaggio (che è mio non deve essere ruotato a MFC)
		}
		break;
	}
	}

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void MParsedCombo::RefreshContentByDataType()
{
	if (ItemsSource->Count == 0)
		return;

	List<MDataObj^>^ newItems = gcnew List<MDataObj^>();
	for (int i = ItemsSource->Count - 1; i >= 0; i--)
	{
		MDataObj^ mOldData = (MDataObj^)ItemsSource[i];
		MDataObj^ mNewData = MDataObj::Create((Microarea::TaskBuilderNet::Core::CoreTypes::DataType^) CompatibleType);

		mNewData->StringValue = mOldData->StringValue;

		newItems->Add(mNewData);
		ItemsSource->Remove(mOldData);
	}

	ItemsSource = newItems;
}

//----------------------------------------------------------------------------
bool MParsedCombo::CanUpdateTarget(Type^ droppedType)
{
	return !HasCodeBehind && droppedType == MPushButton::typeid && ((MStateObject^)Extensions["StateButton"])->Style == MStateObject::ButtonStyle::NoButton;
}

//----------------------------------------------------------------------------
void MParsedCombo::UpdateTargetFromDrop(Type^ droppedType)
{
	MStateObject^ stateButton = (MStateObject^)Extensions["StateButton"];

	if (CanUpdateTarget(droppedType) && stateButton != nullptr)
	{
		stateButton->Style = MStateObject::ButtonStyle::AutoManual;
		PropertyChangingNotifier::OnComponentPropertyChanged(stateButton->Site, stateButton, "Style", MStateObject::ButtonStyle::NoButton, stateButton->Style);
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, stateButton->Name, nullptr, stateButton);
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedListBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedListBox::MParsedListBox(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
	itemsSource = gcnew List<MDataObj^>();
	AttachCallBackFunction();
}

//----------------------------------------------------------------------------
MParsedListBox::MParsedListBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{
	itemsSource = gcnew List<MDataObj^>();
	AttachCallBackFunction();
}

//----------------------------------------------------------------------------
MParsedListBox::~MParsedListBox()
{
	delete itemsSource;
}

//----------------------------------------------------------------------------
void MParsedListBox::Initialize()
{
	__super::Initialize();
	minSize = CUtility::Get100x100Size();
}

//----------------------------------------------------------------------------
void MParsedListBox::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP;
	exStyles |= WS_EX_CLIENTEDGE;
}

//----------------------------------------------------------------------------
System::Collections::IList^ MParsedListBox::ItemsSource::get()
{
	return itemsSource;
}

//----------------------------------------------------------------------------
void MParsedListBox::Parent::set(IWindowWrapperContainer^ value)
{
	if (Parent != nullptr && Parent->GetType()->IsSubclassOf(WindowWrapperContainer::typeid))
		((WindowWrapperContainer^)Parent)->ResumeLayout -= gcnew EventHandler<EasyBuilderEventArgs^>(this, &MParsedListBox::ResumeLayout);

	__super::Parent = value;

	if (Parent != nullptr)
		((WindowWrapperContainer^)Parent)->ResumeLayout += gcnew EventHandler<EasyBuilderEventArgs^>(this, &MParsedListBox::ResumeLayout);
}
//----------------------------------------------------------------------------
String^ MParsedListBox::ItemSourceName::get()
{
	CListDescription* pDescri = dynamic_cast<CListDescription*>(GetWndObjDescription());
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceName) : "";
}

//----------------------------------------------------------------------------
void MParsedListBox::ItemSourceName::set(String^ value)
{
	CListDescription* pDescri = dynamic_cast<CListDescription*>(GetWndObjDescription());
	if (!pDescri)
		return;
	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceName = value;
}

//----------------------------------------------------------------------------
void MParsedListBox::ItemSourceNs::set(String^ value)
{
	CListDescription* pDescri = dynamic_cast<CListDescription*>(GetWndObjDescription());
	if (!pDescri)
		return;

	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceNamespace = value;

}
//----------------------------------------------------------------------------
String^ MParsedListBox::ItemSourceNs::get()
{
	CListDescription* pDescri = dynamic_cast<CListDescription*>(GetWndObjDescription());
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceNamespace) : "";
}
//----------------------------------------------------------------------------
void MParsedListBox::ResumeLayout(Object^ sender, EasyBuilderEventArgs^ e)
{
	Refresh();
}

//----------------------------------------------------------------------------
void MParsedListBox::ItemsSource::set(System::Collections::IList^ value)
{
	itemsSource = value;
}

//----------------------------------------------------------------------------
bool MParsedListBox::IsItemsSourceEditable::get()
{
	// la proprietà hotlink non sarebbe sufficiente
	return m_pControl ? !m_pControl->GetHotLink() : false;
}

//-----------------------------------------------------------------------------
void MParsedListBox::AttachCallBackFunction()
{
	if (!m_pControl)
		return;

	if (fillListBoxHandle.IsAllocated)
		fillListBoxHandle.Free();

	fillListBoxCallBack = gcnew FillListBoxCallBack(this, &MParsedListBox::OnFillListBox);
	fillListBoxHandle = GCHandle::Alloc(fillListBoxCallBack);
	System::IntPtr funPtr = Marshal::GetFunctionPointerForDelegate(fillListBoxCallBack);
	((CParsedListBox*)m_pControl)->SetFillListBoxFuncPtr(static_cast<FILLLISTBOX_FUNC>(funPtr.ToPointer()));
}

//-----------------------------------------------------------------------------
bool MParsedListBox::OnFillListBox()
{
	if (ItemsSource == nullptr || !m_pControl)
		return false;

	for each (MDataObj^ current in ItemsSource)
		((CParsedListBox*)m_pControl)->AddAssociation(current->GetDataObj()->Str(), *current->GetDataObj());

	return true;
}

//----------------------------------------------------------------------------
bool MParsedListBox::WndProc(Message% m)
{
	if (m.Msg == WM_COMMAND)
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);
		if (nCode == LBN_SELCHANGE)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			SelectedIndexChanged(this, args);
			if (args->Handled)
				return true; //mangio il messaggio
		}
	}

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void MParsedListBox::Refresh()
{
	if (m_pControl)
		((CParsedListBox*)m_pControl)->FillListBox();
}

//----------------------------------------------------------------------------
void MParsedListBox::RefreshContentByDataType()
{
	if (ItemsSource->Count == 0)
		return;

	List<MDataObj^>^ newItems = gcnew List<MDataObj^>();
	for (int i = ItemsSource->Count - 1; i >= 0; i--)
	{
		MDataObj^ mOldData = (MDataObj^)ItemsSource[i];
		MDataObj^ mNewData = MDataObj::Create((Microarea::TaskBuilderNet::Core::CoreTypes::DataType^) CompatibleType);

		mNewData->StringValue = mOldData->StringValue;

		newItems->Add(mNewData);
		ItemsSource->Remove(mOldData);
	}

	ItemsSource = newItems;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MParsedButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MParsedButton::MParsedButton(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MParsedButton::MParsedButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{

}

//----------------------------------------------------------------------------
void MParsedButton::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP | BS_MULTILINE | BS_VCENTER | BS_CENTER;
}

//----------------------------------------------------------------------------
bool MParsedButton::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "Text")
		return true;

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
System::String^ MParsedButton::Caption::get()
{
	return Text;
}

//----------------------------------------------------------------------------
void MParsedButton::Caption::set(System::String^ value)
{
	Text = value;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MCheckBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MCheckBox::MCheckBox(System::IntPtr handleWndPtr)
	:
	MParsedButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MCheckBox::MCheckBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedButton(parentWindow, name, controlClass, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
bool MCheckBox::WndProc(Message% m)
{
	if (m.Msg == BM_SETCHECK)
		Checked = (((int)m.WParam) == 1);

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
bool MCheckBox::Checked::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		return ((CWndCheckRadioDescription*)pDescri)->m_bChecked;

	return checkedCheck;
}

//----------------------------------------------------------------------------
void MCheckBox::Checked::set(bool value)
{
	if (value != checkedCheck)
	{
		checkedCheck = value;
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
			((CWndCheckRadioDescription*)pDescri)->m_bChecked = value;
		CWnd* pWnd = GetWnd();
		if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CButton)))
			((CButton*)pWnd)->SetCheck(value ? BST_CHECKED : BST_UNCHECKED);

		CheckedChanged(this, EasyBuilderEventArgs::Empty);
	}
}
//----------------------------------------------------------------------------
void MCheckBox::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= BS_AUTOCHECKBOX | WS_TABSTOP | BS_LEFTTEXT | BS_VCENTER | BS_RIGHT;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MRadioButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MRadioButton::MRadioButton(System::IntPtr handleWndPtr)
	:
	MParsedButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MRadioButton::MRadioButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedButton(parentWindow, name, controlClass, location, hasCodeBehind)
{
}
//----------------------------------------------------------------------------
void MRadioButton::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= BS_AUTORADIOBUTTON | WS_TABSTOP | BS_RIGHT | BS_VCENTER | BS_LEFTTEXT;
}

//----------------------------------------------------------------------------
bool MRadioButton::WndProc(Message% m)
{
	if (m.Msg == BM_SETCHECK)
		Checked = (((int)m.WParam) == 1);

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
bool MRadioButton::Checked::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		return ((CWndCheckRadioDescription*)pDescri)->m_bChecked;

	return checkedRadio;
}

//----------------------------------------------------------------------------
void MRadioButton::Checked::set(bool value)
{
	if (value != checkedRadio)
	{
		checkedRadio = value;
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
			((CWndCheckRadioDescription*)pDescri)->m_bChecked = value;
		CWnd* pWnd = GetWnd();
		if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CButton)))
			((CButton*)pWnd)->SetCheck(value ? BST_CHECKED : BST_UNCHECKED);

		CheckedChanged(this, EasyBuilderEventArgs::Empty);
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MPushButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MPushButton::MPushButton(System::IntPtr handleWndPtr)
	:
	MParsedButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MPushButton::MPushButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedButton(parentWindow, name, controlClass, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
bool MPushButton::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case BM_CLICK:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		Click(this, args);
		if (args->Handled)
			return true; //mangio il messaggio
		break;
	}
	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::Clicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			Click(this, args);

			return true;//mangio il messaggio
		}
		break;
	}
	}

	return __super::WndProc(m);
}
//----------------------------------------------------------------------------
MParsedGroupBox::MParsedGroupBox(System::IntPtr handleWndPtr)
	:
	MParsedButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MParsedGroupBox::MParsedGroupBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedButton(parentWindow, name, controlClass, location, hasCodeBehind)
{
}
//----------------------------------------------------------------------------
void MParsedGroupBox::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= BS_GROUPBOX | BS_VCENTER | BS_CENTER;
}

/////////////////////////////////////////////////////////////////////////////
// 				class LocalHost Implementation
/////////////////////////////////////////////////////////////////////////////

//--------------------------------------------------------------------------------
LocalHost::LocalHost(TextBox^ textBox)
{
	this->textBox = textBox;
	SetPanelSize(textBox);
}

//--------------------------------------------------------------------------------
LocalHost::~LocalHost()
{
	if (wrapper != nullptr)
	{
		delete wrapper;
		textBox->SizeChanged -= gcnew EventHandler(this, &LocalHost::TextBox_SizeChanged);
		wrapper = nullptr;
	}
}

//--------------------------------------------------------------------------------
LocalHost::!LocalHost()
{
}

//--------------------------------------------------------------------------------
MParsedControl^ LocalHost::Wrapper::get()
{
	return wrapper;
}

//--------------------------------------------------------------------------------
void LocalHost::Wrapper::set(MParsedControl^ value)
{
	wrapper = value;
}

//--------------------------------------------------------------------------------
void LocalHost::SetPanelSize(TextBox^ textBox)
{
	Location = textBox->Location - System::Drawing::Size(2, 2);
	Size = textBox->Size + System::Drawing::Size(2, 10);
}

//--------------------------------------------------------------------------------
void LocalHost::TextBox_SizeChanged(Object^ sender, EventArgs^ e)
{
	SetPanelSize((TextBox^)sender);
	wrapper->Size = Size;
}

