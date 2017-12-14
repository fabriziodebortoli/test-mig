#include "stdafx.h"
#include "MPropertyGrid.h"
#include <TbGes\ExtDocView.h>
using namespace Microarea::Framework::TBApplicationWrapper;

using namespace System;
using namespace System::Collections::Generic;

//----------------------------------------------------------------------------
class CMyTBPropertyGrid : public CTBPropertyGrid
{
public:
	void ClearProperties() { m_lstProps.RemoveAll(); }//la delete è fatta nei distruttori
};
//----------------------------------------------------------------------------
class CMyTBProperty : public CTBProperty
{
public:
	void ClearItems() { m_lstSubItems.RemoveAll(); }
};

//----------------------------------------------------------------------------
MPropertyGrid::MPropertyGrid(System::IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr)
{
	ASSERT(GetWnd() && GetWnd()->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)));
	m_pGrid = (CTBPropertyGrid*)GetWnd();
	for (int i = 0; i < m_pGrid->GetPropertyCount(); i++)
	{
		CTBProperty* pProp = (CTBProperty*)m_pGrid->GetProperty(i);
		MPropertyItem^ item = gcnew MPropertyItem(pProp, m_pGrid);
		item->ParentComponent = this;
		items->Add(item);
	}
	m_pGrid->ExpandAll(true);
	items->CollectionChanged += gcnew NotifyCollectionChangedEventHandler(this, &MPropertyGrid::OnCollectionChanged);

}

//----------------------------------------------------------------------------
MPropertyGrid::MPropertyGrid(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, controlClass, location, hasCodeBehind)
{
	items->CollectionChanged += gcnew NotifyCollectionChangedEventHandler(this, &MPropertyGrid::OnCollectionChanged);
}

//----------------------------------------------------------------------------
MPropertyGrid::~MPropertyGrid()
{
	this->!MPropertyGrid();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MPropertyGrid::!MPropertyGrid()
{
	if (!m_pGrid)
		return;
	
	for (int i = Items->Count - 1; i >= 0; i--)
	{
		MPropertyItem^ item = Items[i];
		Items->Remove(item);
		delete item;
	}
	if (!HasCodeBehind)
	{
		CWnd* pParentWnd = m_pGrid->GetParent();
		if (pParentWnd)
		{
			CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
			if (pParsedForm)
				pParsedForm->GetControlLinks()->Remove(m_pGrid);
		}
		m_pGrid->DestroyWindow();
		delete m_pGrid;
		m_pGrid = NULL;
	}

	m_pGrid = NULL;
}

//----------------------------------------------------------------------------
void MPropertyGrid::OnCollectionChanged(Object^ sender, NotifyCollectionChangedEventArgs^ args)
{
	if (args->Action == NotifyCollectionChangedAction::Reset)
	{
		((CMyTBPropertyGrid*) m_pGrid)->ClearProperties();
	}
	else if (args->Action == NotifyCollectionChangedAction::Add)
	{
		for each (MPropertyItem^ item in args->NewItems)
		{
			if (item->m_pProperty)
			{
				((CBCGPPropList*)m_pGrid)->AddProperty((CBCGPProp*)item->m_pProperty);
			}
			else
			{
				item->CreateItem(this);
			}
		}
	}

}

//----------------------------------------------------------------------------
[ExcludeFromIntellisense]
bool MPropertyGrid::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	m_pGrid = new CTBPropertyGrid();
	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	if (!pParentWnd)
	{
		ASSERT(FALSE);
		return false;
	}
	CParsedForm* pForm = GetParsedForm(pParentWnd);
	if (!pForm)
	{
		ASSERT(FALSE);
		return false;
	}
	CreateNamespaceFromParent(parentWindow);
	CRect r(location.X, location.Y, 200, 200);
	::AddLinkPropertyGrid(m_pGrid,
		pForm,
		pParentWnd,
		pForm->GetControlLinks(),
		AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls),
		Name
		);

	Handle = (IntPtr)(int)m_pGrid->m_hWnd;
	return true;
}

//----------------------------------------------------------------------------
ObservableCollection<MPropertyItem^>^  MPropertyGrid::Items::get()
{
	return items;
}

//----------------------------------------------------------------------------
String^ MPropertyGrid::Name::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return  gcnew String(pDescri->m_strName);
	return m_pGrid ? gcnew String(m_pGrid->GetName()) : "";
}

//----------------------------------------------------------------------------
void MPropertyGrid::Name::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strName = value;
	if (!m_pGrid)
	{
		ASSERT(FALSE);
		return;
	}
	m_pGrid->SetName(value);
}

//--------------------------------------------------------------------------------
void MPropertyGrid::UpdateAllPropertyGrid()
{
	CWndObjDescription* pGridDescr = GetWndObjDescription();
	if (!pGridDescr)
		return;
	itemsId->Clear();
	UpdateAllItems(pGridDescr);
	UpdateItemsOrder(pGridDescr);
}

//--------------------------------------------------------------------------------
void MPropertyGrid::CalcItemsIds(ObservableCollection<MPropertyItem^>^ propItems)
{
	for (int i = 0; i < propItems->Count; i++)
	{
		MPropertyItem^ thisItem = propItems[i];
		itemsId->Add(thisItem->Id);
		if (thisItem->Items && thisItem->Items->Count > 0)
			CalcItemsIds(thisItem->Items);
	}
}

//--------------------------------------------------------------------------------
void MPropertyGrid::UpdateAllItems(CWndObjDescription* pParentDescr)
{
	CalcItemsIds(Items);
	int countItems = pParentDescr->m_Children.GetCount();
	for (int i = countItems - 1; i >= 0; i--)
	{
		CWndObjDescription* item = pParentDescr->m_Children[i];
		if (!itemsId->Contains(gcnew String(item->GetID())))
			pParentDescr->m_Children.RemoveItem(item);
		else
			UpdateAllItems(item);
	}
}

//--------------------------------------------------------------------------------
void MPropertyGrid::UpdateItemsOrder(CWndObjDescription* pParentDescr)
{
	for (int i = 0; i < Items->Count; i++)
	{
		MPropertyItem^ thisItem = Items[i];
		int idx = pParentDescr->m_Children.IndexOf(thisItem->Id);
		if (idx != i)
		{
			CWndObjDescription* found = pParentDescr->m_Children[idx];
			pParentDescr->m_Children.RemoveAt(idx);
			pParentDescr->m_Children.InsertAt(i, found);
		}
	}
}

//================================================================================

//----------------------------------------------------------------------------
MPropertyItem::MPropertyItem()
{
	//necessaria per rendere subito operativa l'aggiunta di SubItem dell'item appena creato
	Items->CollectionChanged += gcnew NotifyCollectionChangedEventHandler(this, &MPropertyItem::OnCollectionChanged);
}
//----------------------------------------------------------------------------
MPropertyItem::MPropertyItem(CTBProperty* pProperty, CTBPropertyGrid* pGrid)
	: m_pProperty(pProperty), m_pGrid(pGrid)
{
	Id = gcnew String(AfxGetTBResourcesMap()->DecodeID(TbControls, pProperty->GetID()).m_strName);
	Name = gcnew String(m_pProperty->GetName());
	Text = gcnew String(m_pProperty->GetText());
	Hint = gcnew String(m_pProperty->GetDescription());
	for (int i = 0; i < m_pProperty->GetSubItemsCount(); i++)
	{
		CTBProperty* pProp = (CTBProperty*)m_pProperty->GetSubItem(i);
		MPropertyItem^ item = gcnew MPropertyItem(pProp, m_pGrid);
		item->ParentComponent = this;
		Items->Add(item);
	}
	Items->CollectionChanged += gcnew NotifyCollectionChangedEventHandler(this, &MPropertyItem::OnCollectionChanged);
}

//-----------------------------------------------------------------------------
MPropertyItem::~MPropertyItem()
{
	this->!MPropertyItem();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MPropertyItem::!MPropertyItem()
{
	for (int i = Items->Count - 1; i >= 0; i--)
	{
		MPropertyItem^ item = Items[i];
		Items->Remove(item);
		delete item;
	}
	if (!m_pProperty)
		return;
	CTBProperty* p = m_pProperty;
	if (MPropertyGrid::typeid->IsInstanceOfType(ParentComponent))
	{
		//se non riesce a distruggerlo il contenitore, perché nel frattempo è stato rimosso dalla lista (es ClearProperties)
		//lo distruggo io
		if (!m_pGrid->DeleteProperty(p))
			delete m_pProperty;
	}
	else
	{
		//se non riesce a distruggerlo il contenitore, perché nel frattempo è stato rimosso dalla lista (es ClearProperties)
		//lo distruggo io
		if (!((MPropertyItem^)ParentComponent)->m_pProperty->RemoveSubItem(p))
			delete m_pProperty;
	}
	m_pProperty = NULL;
}

//----------------------------------------------------------------------------
ObservableCollection<MPropertyItem^>^ MPropertyItem::Items::get()
{
	return items;
}

//----------------------------------------------------------------------------
void MPropertyItem::CreateItem(EasyBuilderComponent^ parent)
{
	ParentComponent = parent;
	ObservableCollection<MPropertyItem^>^ parentItems;
	CString sName, sText;
	if (MPropertyGrid::typeid->IsInstanceOfType(parent))
	{
		Id = ((MPropertyGrid^)ParentComponent)->Id;
		sName = ((MPropertyGrid^)ParentComponent)->Name;
		parentItems = ((MPropertyGrid^)ParentComponent)->Items;
	}
	else
	{
		Id = ((MPropertyItem^)ParentComponent)->Id;
		sName = ((MPropertyItem^)ParentComponent)->Name;
		parentItems = ((MPropertyItem^)ParentComponent)->Items;
	}

	int start = 0;
	bool conflict = true;
	do
	{
		start++;
		String^ candidateId = Id + "_" + start;
		conflict = false;
		for each (MPropertyItem^ item in parentItems)
			if (item->Id == candidateId)
			{
				conflict = true;
				break;
			}
	} while (conflict);

	String^ numbers = "_" + start;
	Id = Id + numbers;
	sName = sName + numbers;
	sText = "Text" + numbers;

	DataObj* pDataObj = NULL;
	UINT nId = AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls);
	DWORD dwNeededStyle = 0;
	CRuntimeClass* pClass = NULL;
	HotKeyLink* pHotLink = NULL;
	UINT nButtonID = BTN_DEFAULT;
	SqlRecord* pRec = NULL;
	if (MPropertyGrid::typeid->IsInstanceOfType(parent))
	{
		m_pGrid = (CTBPropertyGrid*) ((MPropertyGrid^)parent)->GetWnd();
		m_pProperty = m_pGrid->AddProperty(sName, sText, Hint, pDataObj, nId, dwNeededStyle, pClass, pHotLink, nButtonID, pRec);
	}
	else
	{
		m_pGrid = ((MPropertyItem^)parent)->m_pGrid;
		m_pProperty = m_pGrid->AddSubItem(((MPropertyItem^)parent)->m_pProperty, sName, sText, Hint, pDataObj, nId, dwNeededStyle, pClass, pHotLink, nButtonID, pRec);
	}	
}

//----------------------------------------------------------------------------
void MPropertyItem::OnCollectionChanged(Object^ sender, NotifyCollectionChangedEventArgs^ args)
{
	if (args->Action == NotifyCollectionChangedAction::Reset)
	{
		((CMyTBProperty*) m_pProperty)->ClearItems();
	}
	else if (args->Action == NotifyCollectionChangedAction::Add)
	{
		for each (MPropertyItem^ item in args->NewItems)
		{
			if (item->m_pProperty)
			{
				((CBCGPProp*)m_pProperty)->AddSubItem((CBCGPProp*)item->m_pProperty);
			}
			else
			{
				item->CreateItem(this);
			}
		}
	}
}

//----------------------------------------------------------------------------
CWndPropertyGridItemDescription* MPropertyItem::GetWndObjDescription()
{
	EasyBuilderComponent^ cmp = ParentComponent;
	if (!cmp)
		return NULL;
	CWndObjDescription* pParentDesc = MPropertyGrid::typeid->IsInstanceOfType(cmp)
		? ((MPropertyGrid^)cmp)->GetWndObjDescription()
		: ((MPropertyItem^)cmp)->GetWndObjDescription();

	if (!pParentDesc)
		return NULL;
	return (CWndPropertyGridItemDescription*)pParentDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndPropertyGridItemDescription), id);
}

//----------------------------------------------------------------------------
System::String^ MPropertyItem::Text::get() 
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	CString textAutoGenerated = m_pProperty ? m_pProperty->GetText() : CString("");
	if (!pDescri)
		return gcnew String(textAutoGenerated);
	if(pDescri->m_strText.IsEmpty() && !textAutoGenerated.IsEmpty())
		pDescri->m_strText = textAutoGenerated;
	return gcnew String(pDescri->m_strText);
}

//----------------------------------------------------------------------------
void MPropertyItem::Text::set(System::String^ value)
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strText = value;

	if (m_pProperty)
	{
		m_pProperty->SetText(value);
		m_pProperty->Redraw();
	}
}

//----------------------------------------------------------------------------
System::String^ MPropertyItem::Name::get()
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return gcnew String(pDescri->m_strName);
	return m_pProperty ? gcnew String(m_pProperty->GetName()) : "";
}

//----------------------------------------------------------------------------
void MPropertyItem::Name::set(System::String^ value)
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strName = value;
	if (m_pProperty)
	{
		m_pProperty->SetName(value);
		m_pProperty->Redraw();
	}
}

//----------------------------------------------------------------------------
System::String^ MPropertyItem::Hint::get()
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		return gcnew String(pDescri->m_strHint);
	return m_pProperty ? gcnew String(m_pProperty->GetDescription()) : "";
}

//----------------------------------------------------------------------------
void MPropertyItem::Hint::set(System::String^ value)
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strHint = value;
	if (m_pProperty)
	{
		m_pProperty->SetDescription(value);
		m_pProperty->Redraw();
	}
}

//----------------------------------------------------------------------------
void MPropertyItem::ItemSourceNs::set(String^ value)
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;

	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceNamespace = value;
}

//----------------------------------------------------------------------------
String^ MPropertyItem::ItemSourceNs::get()
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pItemSourceDescri)
		return gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceNamespace);
	return "";
}

//----------------------------------------------------------------------------
String^ MPropertyItem::ItemSourceName::get()
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pItemSourceDescri)
		return gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceName);
	return "";
}

//----------------------------------------------------------------------------
void MPropertyItem::ItemSourceName::set(String^ value)
{
	CWndPropertyGridItemDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceName = value;
}

//----------------------------------------------------------------------------
IControlClass^ MPropertyItem::ClassType::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	CRegisteredParsedCtrl*	pRegInfo = pDescri ? 
		AfxGetParsedControlsRegistry()->GetRegisteredControl(pDescri->m_strControlClass) : NULL;
	return pRegInfo ? gcnew ControlClass(pRegInfo) : nullptr;
}

//----------------------------------------------------------------------------
void MPropertyItem::ClassType::set(IControlClass^ controlClass)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if(pDescri)
		pDescri->m_strControlClass = controlClass->ClassName;
}

//----------------------------------------------------------------------------
bool MPropertyItem::AutoVScroll::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && MPropertyItem::ClassType && MPropertyItem::ClassType->FamilyName == "MParsedCombo")
		return pDesc->m_bVScroll;
	return	false;
}

//----------------------------------------------------------------------------
void MPropertyItem::AutoVScroll::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_bVScroll = (MPropertyItem::ClassType->FamilyName != "MParsedCombo") ? false : value;
}

//----------------------------------------------------------------------------
String^ MPropertyItem::HotLinkName::get()
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
void MPropertyItem::HotLinkName::set(String^ value)
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
String^ MPropertyItem::HotLinkNs::get()
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

//----------------------------------------------------------------------------
void MPropertyItem::HotLinkNs::set(String^ value)
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
String^ MPropertyItem::DataSource::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);

	return "";
}

//----------------------------------------------------------------------------
void MPropertyItem::DataSource::set(String^ dataSource)
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
String^ MPropertyItem::Activation::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? gcnew String(pDescri->m_strActivation) : "";
}

//----------------------------------------------------------------------------
void MPropertyItem::Activation::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strActivation = value;
}

//-----------------------------------------------------------------------------
bool MPropertyItem::Collapsed::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndPropertyGridItemDescription)) ? 
		((CWndPropertyGridItemDescription*)pDesc)->m_bCollapsed : false;
}

//-----------------------------------------------------------------------------
void MPropertyItem::Collapsed::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndPropertyGridItemDescription)))
		((CWndPropertyGridItemDescription*)pDesc)->m_bCollapsed = value;
}

//----------------------------------------------------------------------------
bool MPropertyItem::Sort::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndPropertyGridItemDescription)))
		return ((CWndPropertyGridItemDescription*)pDesc)->m_bSort;
	return	false;
}

//----------------------------------------------------------------------------
void MPropertyItem::Sort::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndPropertyGridItemDescription)))
		((CWndPropertyGridItemDescription*)pDesc)->m_bSort = value;
}

//----------------------------------------------------------------------------
int MPropertyItem::Chars::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_nChars;
	return -1;
}

//----------------------------------------------------------------------------
void MPropertyItem::Chars::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_nChars = value;
}

//----------------------------------------------------------------------------
bool MPropertyItem::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "AutoVScroll")
		return (MPropertyItem::ClassType 
			&&  MPropertyItem::ClassType->FamilyName 
			&& (MPropertyItem::ClassType->FamilyName == "MParsedCombo"));
	
	//ESD: posso assegnare l'hotlinkNs solo se ho prima impostato l'hotlinkName perchè è chiave
	if (propertyName == MParsedControlSerializer::HotLinkNsPropName)
		return !System::String::IsNullOrEmpty(HotLinkName);

	return __super::CanChangeProperty(propertyName);
}