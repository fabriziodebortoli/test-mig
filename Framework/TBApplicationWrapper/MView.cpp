#include "StdAfx.h"
#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGes\ExtDocView.h>
#include <TbGes\Tabber.h>
#include <TbGeneric\DocumentObjectsInfo.h>
#include <TbGes\JsonFrame.h>

#include "MDocument.h"
#include "MView.h"
#include "MToolbar.h"
#include "MBodyEdit.h"
#include "MTileManager.h"
#include "TbGes\TileManager.h"


using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Collections::Generic;

using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System::ComponentModel;

//----------------------------------------------------------------------------
MFrame::MFrame(IntPtr framePtr, WindowWrapperContainer^ view)
	:
	WindowWrapperContainer(framePtr)
{
	this->view = view;
}

//----------------------------------------------------------------------------
bool MFrame::WndProc(Message% m)
{
	((MView^) view)->FrameWndProc(m);
	
	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
MView::MView(IntPtr handleViewPtr)
	: WindowWrapperContainer(handleViewPtr)
{ 
	Handle = handleViewPtr;
	m_pView = (CAbstractFormView*) GetWnd();	
	frame = gcnew MFrame((IntPtr) m_pView->GetFrame()->m_hWnd, this);
	NextTBPos = Point(0,0);
	toolBarLevel = 0;
	Visible = false;
	suspendLayout = false;
	pathToSerialize = gcnew String(_T(""));
}

//----------------------------------------------------------------------------
MView::~MView(void)
{
	if (m_pView)
	{
		CAbstractFormDoc* pDoc = m_pView->GetDocument();
		if (pDoc)
			m_pView->SyncExternalControllerInfo(TRUE);
	}
	this->!MView();
}

//-----------------------------------------------------------------------------
MView::!MView()
{
	delete frame;
	m_pView = NULL;
}

//-------------------------------------------------------------------------------------
void MView::SetPathToSerialize(System::String^ path)
{
	pathToSerialize = path;
}

//----------------------------------------------------------------------------------
bool MView::IsDynamicDocument()
{
	if (!this->m_pView || !this->m_pView->GetDocument())
		return false;

	return this->m_pView->GetDocument()->IsKindOf(RUNTIME_CLASS(CDynamicFormDoc)) == TRUE;
}

//------------------------------------------------------------------------------
void MView::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	if (this->IsDynamicDocument()) //created by ES
	{
		__super::UpdateAttributesForJson(NULL);
		jsonDescription->m_Type = CWndObjDescription::WndObjType::View;
		jsonDescription->m_bChild = true;
	}
	else
	{
		jsonDescription->m_strIds.Add(this->Id);
		jsonDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
	}
}

//-------------------------------------------------------------------------------------
void MView::GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	CString jsonViewId = this->Id + _T("_") + _T("VIEW_ES");
	__super::GenerateSerialization(pParentDescription, serialization);

	//manage saving
	if (!jsonDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
	{
		serialization->Add
		(
			gcnew Tuple<System::String^, System::String^, System::Boolean>
			(
				gcnew String(this->Id),
				gcnew String(GetSerialization(jsonDescription)),
				false
			)
		);
	}
	else if (jsonDescription->m_Children.GetCount() > 0)
	{
		//ClientForms
		serialization->Add
		(
			gcnew Tuple<System::String^, System::String^, System::Boolean>
			(
				gcnew String(jsonViewId),
				gcnew String(GetSerialization(jsonDescription)),
				true
			)
		);
	}

	ManageSerializations(serialization);

	ManageDocumentOutline();

	ManageClientForms(serialization);

	for (int i = jsonDescription->m_Children.GetUpperBound(); i >= 0; i--)
	{
		SAFE_DELETE(jsonDescription->m_Children.GetAt(i));
		jsonDescription->m_Children.RemoveAt(i);
	}

}

//-------------------------------------------------------------------------------
bool MView::ManageDocumentOutline()
{
	CString jsonViewId = this->Id;

	if (!this->IsDynamicDocument())
		return true;

	CString jsonBaseFrameId = _T("");
	if (this->m_pView && this->m_pView->GetDocument())
	{
		if (this->m_pView->GetDocument()->IsABatchDocument())
			jsonBaseFrameId = _T("M.Framework.TbGes.TbGes.IDD_BATCH_FRAME");
		else
			jsonBaseFrameId = _T("M.Framework.TbGes.TbGes.IDD_MASTER_FRAME");
	}

	CDummyDescription* pDocumentOutlineDescription = new CDummyDescription();
	CString jsonFrameId = _T("IDD_EMPTY_FRAME");	
	pDocumentOutlineDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
	pDocumentOutlineDescription->m_strIds.Add(jsonFrameId);
	pDocumentOutlineDescription->m_strName = this->Name + _T("Frame");
	pDocumentOutlineDescription->m_arHrefHierarchy.Add(jsonBaseFrameId);
	pDocumentOutlineDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
	CDummyDescription* pChildView = new CDummyDescription();
	pDocumentOutlineDescription->m_Children.Add(pChildView);
	pChildView->m_arHrefHierarchy.Add(jsonViewId);
	pChildView->m_Type = CWndObjDescription::WndObjType::Undefined;

	CJsonSerializer ser;
	pDocumentOutlineDescription->SerializeJson(ser);
	this->SaveSerialization(CString(String::Concat(pathToSerialize, backSlash, gcnew String(jsonFrameId), tbjsonExtension)), ser.GetJson());

	for (int i = pDocumentOutlineDescription->m_Children.GetUpperBound(); i >= 0; i--)
	{
		SAFE_DELETE(pDocumentOutlineDescription->m_Children.GetAt(i));
		pDocumentOutlineDescription->m_Children.RemoveAt(i);
	}
	SAFE_DELETE(pDocumentOutlineDescription);

	return true;
}

//----------------------------------------------------------------------------------
bool MView::ManageClientForms(List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	if (this->IsDynamicDocument() || !this->m_pView || !this->m_pView->GetDocument())
		return true;

	CTBNamespace aNs = this->m_pView->GetDocument()->GetNamespace();
	if (aNs.IsEmpty() || !aNs.IsValid())
		return false;

	//calculate l'IDD del server frame
	CJsonFrame* pFrame = dynamic_cast<CJsonFrame*>(this->m_pView->GetFrame());
	if (!pFrame)
		return false;

	CWndFrameDescription* pFrameDesc = pFrame->GetFrameDescription();
	if (!pFrameDesc)
		return false;

	CString sServerId = pFrameDesc->GetID();

	//manage ClientForms
	CString sFileNameCompletePath = _T("C:\\DevWeb_Next\\Custom\\Subscription\\ESHome\\Applications\\App1\\Mod1\\ModuleObjects\\ClientDocumentObjects.xml");
	//= AfxGetPathFinder()->GetClientDocumentObjectsFullName(this->m_pView->GetDocument()->GetNamespace(), CPathFinder::CUSTOM, CPathFinder::EASYSTUDIO);

	//manage clientforms tag for me
	
	CXMLDocumentObject aDoc;
	CXMLNode* pClientForms;
	CXMLNode* pClientForm;

	aDoc.CreateRoot(XML_CDDOCUMENTOBJECTS_TAG);
	pClientForms = aDoc.CreateRootChild(XML_CLIENTFORMS_TAG);

	for each (Tuple<System::String^, System::String^, System::Boolean>^ element in serialization)
	{
		if (element->Item3 == false)
			continue;
		
		pClientForm = pClientForms->CreateNewChild(XML_CLIENTFORM_TAG);
		pClientForm->SetAttribute(XML_NAME_ATTRIBUTE, CString(element->Item1));
		pClientForm->SetAttribute(XML_SERVER_ATTRIBUTE, sServerId);
	}

	CString aXML = _T("");
	aDoc.GetXML(aXML);
	this->SaveSerialization(sFileNameCompletePath, aXML);
	

	//TODO SAFE_DELETE

	//if (!ExistFile(sFileNameCompletePath))
	//{
	//	//TODO - save this
	//}
	//else
	//{
	//	//TODO - rewrite ClientForms for me
	//}

	return true;
}

//-------------------------------------------------------------------------------
void MView::GenerateJson(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	if (System::String::IsNullOrEmpty(pathToSerialize))
		return;
	
	if (this->IsDynamicDocument())	//creato da ES
		jsonDescription = new CWndObjDescription(NULL);
	else
		jsonDescription = new CDummyDescription();

	if (serialization != nullptr)
		delete serialization;

	serialization = gcnew List<System::Tuple<System::String^, System::String^, System::Boolean>^>();
	
	__super::GenerateJson(NULL, serialization);
	
	SAFE_DELETE(jsonDescription);
	delete serialization;
}

//------------------------------------------------------------------------------------------------------------
bool MView::ManageSerializations(List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	CJsonSerializer jsonSerEv;

	jsonSerEv.OpenArray(_T("items"));
	int n = 0;
	for each (Tuple<System::String^, System::String^, System::Boolean>^ element in serialization)
	{
		if (element->Item1->IndexOf(prefixEvent) < 0)
			this->SaveSerialization(CString(String::Concat(pathToSerialize, backSlash, element->Item1, tbjsonExtension)), CString(element->Item2));
		else
			EventsJsonStringDeserialize(CString(element->Item2), jsonSerEv, n);
	}
	jsonSerEv.CloseArray();

	if (n > 0)
		this->SaveSerialization(CString(String::Concat(pathToSerialize, backSlash, userMethods)), jsonSerEv.GetJson());

	return true;
}

//----------------------------------------------------------------------------------
void MView::EventsJsonStringDeserialize(const CString& strEvents, CJsonSerializer& jsonSer, int& idx)
{
	CJsonParser parser;
	parser.ReadJsonFromString(strEvents);
	if (parser.BeginReadArray(CString(contentTag)))
		for (int i = 0; i < parser.GetCount(); i++)
		{
			if (parser.BeginReadObject(i))
			{
				CString sNs = parser.ReadString(CString(namespaceTag));
				CString sEvent = parser.ReadString(CString(eventTag));

				jsonSer.OpenObject(idx);
				jsonSer.WriteString(CString(namespaceTag), sNs);
				jsonSer.WriteString(CString(eventTag), sEvent);
				jsonSer.CloseObject();
				idx++;
			}
			parser.EndReadObject();
		}
	parser.EndReadArray();
}

//----------------------------------------------------------------------------------------------------
bool MView::SaveSerialization(const CString& fileName, const CString& sSerialization)
{
	CLineFile file;
	if (!file.Open(CString(fileName), CFile::modeCreate | CFile::modeWrite | CFile::typeText))
		return false;

	file.WriteString(sSerialization);
	file.Close();

	return true;
}

//----------------------------------------------------------------------------
INameSpace^ MView::Namespace::get ()
{
	return gcnew NameSpace(gcnew System::String(m_pView->GetNamespace().ToString()));
}

//----------------------------------------------------------------------------
IntPtr MView::DocumentPtr::get ()
{
	if (m_pView)
		return (IntPtr) m_pView->GetDocument();

	return IntPtr::Zero;
}

//----------------------------------------------------------------------------
System::String^ MView::ControlLabel::get()
{
	String^ nodeLabel = Text;
	nodeLabel = nodeLabel->Trim();
	if (System::String::IsNullOrEmpty(nodeLabel))
	{
		nodeLabel = ViewName;
		nodeLabel = nodeLabel->Trim();
		if (nodeLabel == "Dynamic")
			nodeLabel = String::Empty;
	}

	if (System::String::IsNullOrEmpty(nodeLabel))
		nodeLabel = "View";

	return nodeLabel;
}
//----------------------------------------------------------------------------
EDesignMode MView::DesignModeType::get()
{
	CBaseDocument* pDoc = m_pView ? m_pView->GetDocument() : NULL;

	if (!pDoc)
		return EDesignMode::None;

	switch (pDoc->GetDesignMode())
	{
	case CBaseDocument::DM_RUNTIME:
		return EDesignMode::Runtime;

	case CBaseDocument::DM_STATIC:
		return EDesignMode::Static;
	}

	return EDesignMode::None;
}
//----------------------------------------------------------------------------
bool MView::DesignMode::get()
{
	return DesignModeType != EDesignMode::None;
}
//----------------------------------------------------------------------------
IntPtr MView::GetTabberHandle (System::String^ name)
{
	for each (IComponent^ component in Components)
	{
		MTabber^ tabber = dynamic_cast<MTabber^>(component);
		if (tabber != nullptr && tabber->Name == name)
			return tabber->Handle;
	}
	return IntPtr::Zero;
}

//----------------------------------------------------------------------------
void MView::PreTranslateMsgKey	(UINT Msg, WPARAM wParam, LPARAM lParam)
{
	if (ToolBar != nullptr)
		((MToolbar^) ToolBar)->PreTranslateMsgKey	(Msg, wParam, lParam);
}

//----------------------------------------------------------------------------
MTabber^ MView::GetTabberByName (System::String^ name)
{
	if (Handle == NoView)
		return nullptr;

	for each (IComponent^ component in Components)
	{
		MTabber^ tabber = dynamic_cast<MTabber^>(component);
		if (tabber != nullptr && tabber->Name == name)
			return tabber;
	}

	return nullptr;
}

//----------------------------------------------------------------------------
void MView::UpdateWindow()
{
	if (SuspendLayout)
		return;

	__super::UpdateWindow();
}

//----------------------------------------------------------------------------
void MView::Invalidate(System::Drawing::Rectangle screenCoordRect)
{
	if (SuspendLayout)
		return;

	__super::Invalidate(screenCoordRect);
}

//----------------------------------------------------------------------------
void MView::Invalidate()
{
	if (SuspendLayout)
		return;

	__super::Invalidate();
}

//----------------------------------------------------------------------------
void MView::AfterWndProc(Message% m)
{
	__super::AfterWndProc(m);

	if (m.Msg == WM_COMMAND)
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);

		if (nCode ==  EN_VALUE_CHANGED)
		{
			int senderHashCode = this->GetHashCode();
			::SendMessage(hwnd, UM_EASYBUILDER_ACTION, ValueChanged, (LPARAM)senderHashCode);
		}
		else if (nCode ==  EN_CTRL_STATE_CHANGED)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			::SendMessage(hwnd, UM_EASYBUILDER_ACTION, StateButtonClicked, (LPARAM)&args);
		}
		return;
	}

	if (m.Msg == UM_LAYOUT_CHANGED && LayoutObject != nullptr)
		LayoutObject->CallCreateComponents();

}

//----------------------------------------------------------------------------
void MView::ScrollToPosition (Point pos)
{
	if (m_pView)
		m_pView->ScrollToPosition(CPoint(pos.X, pos.Y));
}
//----------------------------------------------------------------------------
System::String^ MView::ViewName::get()
{
	if (Namespace == nullptr)
		return gcnew System::String ("");
	
	int nPos = Namespace->ToString()->LastIndexOf(".");
	if (nPos <= 0)
		return Namespace->ToString();

	return Namespace->ToString()->Substring(nPos + 1);
}

//----------------------------------------------------------------------------
void MView::OnAfterCreateComponents()
{ 
	if (!m_pView)
		return;

	for (int nIdx = 0; nIdx < m_pView->m_pTabManagers->GetSize(); nIdx++)
	{
		CTabManager* pTabber = m_pView->m_pTabManagers->GetAt(nIdx);

		CTileManager* tileManager = dynamic_cast<CTileManager*>(pTabber);
		if (!tileManager)
			continue;

		MTileManager^ tabber = gcnew MTileManager((IntPtr)pTabber->m_hWnd);
		tabber->Parent = this;
		Add(tabber);
	}

	for (int nIdx = 0; nIdx < m_pView->m_pTileGroups->GetSize(); nIdx++)
	{
		CBaseTileGroup* pTileGroup = (CBaseTileGroup*)m_pView->m_pTileGroups->GetAt(nIdx);
		if (!pTileGroup)
			continue;
		IWindowWrapper^ window = BaseWindowWrapper::Create((IntPtr)pTileGroup->m_hWnd);
		MTileGroup^ mTileGroup = dynamic_cast<MTileGroup^>(window);
		if (mTileGroup != nullptr)
		{
			mTileGroup->Parent = this;
			Add(mTileGroup);
		}
		continue;
	}
	
}

//----------------------------------------------------------------------------
void MView::CallCreateComponents()
{
	// TODOBRUNA la CallCreateComponents della DocumentView e' tutta
	// riscritta (quando unifichiamo le call createComponent come si deve sistemiamo)
	__super::CallCreateComponents();
	if (LayoutObject != nullptr)
		LayoutObject->CallCreateComponents();
}

//----------------------------------------------------------------------------
void MView::AfterTargetDrop(System::Type^ droppedType)
{
	if (m_pView)
		m_pView->RequestRelayout();
}

//----------------------------------------------------------------------------
IWindowWrapper^	MView::GetControl (INameSpace^ nameSpace)
{
	if (System::String::Compare(Namespace->ToString(), nameSpace->ToString(), true) == 0)
		return this;

	IWindowWrapper^ control = nullptr;
	for each (IComponent^ component in Components)
	{
		MTabber^ tabber = dynamic_cast<MTabber^>(component);
		if (tabber == nullptr)
			continue;

		if (System::String::Compare(tabber->Namespace->ToString(), nameSpace->ToString(), true) == 0)
			return tabber;
		
		control = tabber->GetControl(nameSpace);
		if (control != nullptr)
			return control;
	}

	return WindowWrapperContainer::GetControl(nameSpace);
}

//----------------------------------------------------------------------------
HWND MView::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pView
		? m_pView->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
IWindowWrapper^	MView::GetParsedCtrlLink(INameSpace^ nameSpace)
{
	CWnd* pCtrl = m_pView->GetWndLinkedCtrl(CTBNamespace(CString(nameSpace->ToString())));
	if (pCtrl != nullptr)
		return BaseWindowWrapper::Create((IntPtr)pCtrl->m_hWnd);
	return nullptr;
}

//----------------------------------------------------------------------------
bool MView::CanDropTarget (Type^ droppedObject)
{
	if (DesignModeType == EDesignMode::Static)
		return false;

	if (
		MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid) ||
		MTilePanel::typeid == droppedObject || droppedObject->IsSubclassOf(MTilePanel::typeid) ||
		MTab::typeid == droppedObject || droppedObject->IsSubclassOf(MTab::typeid)
		)
		return false;

	return Enabled;
}

//----------------------------------------------------------------------------
System::Drawing::Size MView::TotalSize::get ()
{ 
	if (!m_pView)
		return System::Drawing::Size::Empty;

	CSize totalSize =  m_pView->GetTotalSize ();
	return System::Drawing::Size (totalSize.cx, totalSize.cy);
}

//-----------------------------------------------------------------------------
System::String^ MView::SerializedName::get ()
{ 
	return Name;
}

//-----------------------------------------------------------------------------
System::String^ MView::SerializedType::get ()
{ 
	return EasyBuilderControlSerializer::ViewClassName;
}

//----------------------------------------------------------------------------
void MView::Location::set (Point p) 
{ 
	__super::set(p);
}

//----------------------------------------------------------------------------
System::String^ MView::BackgroundImage::get ()
{ 
	if (!m_pView)
		return "";
	return gcnew System::String(m_pView->GetBackgroundImage());
}

//----------------------------------------------------------------------------
void MView::BackgroundImage::set (System::String^ image) 
{ 
	if (!m_pView)
		return;
	m_pView->SetBackgroundImage(image);
}

//----------------------------------------------------------------------------
Point MView::Location::get ()
{ 
	return __super::get();
}

//----------------------------------------------------------------------------
void MView::Size::set(System::Drawing::Size sz)
{
	__super::Size = sz;
}

//----------------------------------------------------------------------------
System::Drawing::Size MView::Size::get ()
{ 
	return  __super::Size;
}

//----------------------------------------------------------------------------
IntPtr MView::GetChildFromOriginalPos (Point clientPosition, System::String^ controlClass)
{
	return m_pView 
		? (IntPtr)(int)GetChildWindow(m_pView->m_hWnd, CString(controlClass), clientPosition, m_pView->m_HWNDPositionsMap)
		: IntPtr::Zero;
}

//----------------------------------------------------------------------------
void MView::SaveChildOriginalPos (IntPtr hwndChild, Point clientPosition)
{
	if  (m_pView)
		SaveChildWindowPos((HWND)hwndChild.ToInt64(), clientPosition, m_pView->m_HWNDPositionsMap);
}

//----------------------------------------------------------------------------
System::Drawing::Rectangle MView::FrameArea::get ()
{
	return frame->Rectangle;
}

//----------------------------------------------------------------------------
IntPtr MView::FrameHandle::get ()
{
	return frame->Handle;
}

//----------------------------------------------------------------------------
void MView::GetChildrenFromPos (System::Drawing::Point p, IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren)
{
	__super::GetChildrenFromPos(p, handleToSkip, foundChildren);
	GetFrameChildrenFromPos(p, handleToSkip, foundChildren);
}

//----------------------------------------------------------------------------
void MView::GetFrameChildrenFromPos( System::Drawing::Point screenPoint, IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren)
{
	HWND hEditingArea = (HWND)(int) frame->Handle;
	HWND hHandleToSkip = (HWND)(int)handleToSkip;
	CPoint aPt(screenPoint.X, screenPoint.Y);

	HWND  hChild = GetWindow(hEditingArea, GW_CHILD);
	while (hChild)
	{
		if (hChild != hHandleToSkip) 
		{
			CRect aRect;
			::GetWindowRect(hChild, &aRect);
			
			if (aRect.PtInRect(aPt))
			{
				CWnd* pWnd = CWnd::FromHandle(hChild);
				if (pWnd->IsKindOf(RUNTIME_CLASS(CView)) || pWnd->IsKindOf(RUNTIME_CLASS(CFrameWnd)))
				{
					hChild = GetNextWindow(hChild, GW_HWNDNEXT);
					continue;
				}

				// customized controls
				IWindowWrapper^ wrapper = GetControl((IntPtr) hChild);
			
				if (wrapper != nullptr)
				{
					if (IWindowWrapperContainer::typeid->IsInstanceOfType(wrapper))
						((WindowWrapperContainer^) wrapper)->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);

					foundChildren->Add(wrapper);
				}
			}
		}

		hChild = GetNextWindow(hChild, GW_HWNDNEXT);
	}

}

//----------------------------------------------------------------------------
IWindowWrapper^ MView::ToolBar::get ()
{
	return GetControl(defaultToolbarName);
}

//----------------------------------------------------------------------------
bool MView::FrameWndProc(Message% m)
{
	switch (m.Msg)
	{
		case WM_SIZE:
		case WM_MOVE:
		{
			ResizeFrame ();
			break;
		}

		case WM_COMMAND:
		{
			WPARAM wParam = (WPARAM)(int)m.WParam;
			LPARAM lParam = (LPARAM)(int)m.LParam;
			DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);
			if (ToolBar != nullptr)
			{
				if (hwnd == NULL && lParam == NULL && wParam != NULL)
					((MToolbar^) ToolBar)->CallEvent(hwnd, (int) m.WParam, 0);
				else if (hwnd != NULL)
					((MToolbar^) ToolBar)->CallEvent(hwnd, nID, nCode);
			}
			break;
		}
		
		case WM_NOTIFY:
			{
				WPARAM wParam = (WPARAM)(int)m.WParam;
				LPARAM lParam = (LPARAM)(int)m.LParam;
				// notifica da parte di EasyLook dell'evento di DropDown della tendina
				if (((UINT) wParam) == AFX_IDW_TOOLBAR && ToolBar != nullptr)
				{
					NMTOOLBAR* pNmhdr = (NMTOOLBAR*) lParam;
					if (pNmhdr->hdr.code == TBN_DROPDOWN)
						((MToolbar^) ToolBar)->CallEvent(pNmhdr->hdr.hwndFrom, pNmhdr->iItem, pNmhdr->hdr.code);
				}
				break;
			}
	}

	return false;
}

//----------------------------------------------------------------------------
void MView::ResizeFrame	()
{
	
	if (!m_pView || !m_pView->GetFrame() || components == nullptr)
		return;

	Size = FrameArea.Size;
}

//----------------------------------------------------------------------------
System::String^	MView::Id::get()
{
	if (m_pView && m_pView->GetJsonContext() && m_pView->GetJsonContext()->m_pDescription && m_pView->GetJsonContext()->m_pDescription->m_strIds.GetCount() > 0)
		return gcnew System::String(m_pView->GetJsonContext()->m_pDescription->m_strIds.GetAt(0));
	
	return __super::Id;
}

//----------------------------------------------------------------------------
bool MView::CreateWrappers (array<IntPtr>^ handlesToSkip)
{
	bool ok = __super::CreateWrappers(handlesToSkip);

	if (!m_pView || !m_pView->GetFrame() || !m_pView->GetFrame()->HasToolbar())
		return ok;

	ResizeFrame();
	return ok;
}


//----------------------------------------------------------------------------
void MView::RequestRelayout()
{
	if (m_pView)
		m_pView->RequestRelayout();
}

//----------------------------------------------------------------------------
MLayoutObject^ MView::LayoutObject::get()
{
	if (layoutObject == nullptr && m_pView &&  m_pView->GetLayoutContainer())
	{
		layoutObject = gcnew MLayoutObject(gcnew MLayoutContainer((IntPtr)m_pView->GetLayoutContainer()));
		layoutObject->LinkedComponent = this;
	}

	return layoutObject;
}

//----------------------------------------------------------------------------
void MView::LayoutChangedFor(INameSpace^ ns)
{
	return 	LayoutObject == nullptr ? nullptr : LayoutObject->LayoutChangedFor(ns);
}

//----------------------------------------------------------------------------
void MView::RemoveLayoutObjectOn(INameSpace^ ns)
{
	if (LayoutObject != nullptr)
		LayoutObject->RemoveLayoutObjectOn(ns);
}

//----------------------------------------------------------------------------
void MView::MoveTileGroup(MTileGroup^ tileGroup, int newIndex)
{
	if (components && components->Contains(tileGroup))
	{
		int oldIndex = components->IndexOf(tileGroup);

		if (oldIndex < 0)
			return;

		components->RemoveAt(oldIndex);
		components->Insert(newIndex, tileGroup);
		LayoutChangedFor(Namespace);
	}
}