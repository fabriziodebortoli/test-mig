#include "StdAfx.h"
#include <TbNameSolver/TBResourcesMap.h>
#include <TbGeneric/DataObj.h>
#include <TbGeneric/FormatsTable.h>
#include <TbParser\SymTable.h>

#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\ParsEdt.h>
#include <TbGenlib\ParsCbx.h>
#include <TbGenlib\ParsBtn.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\TABCORE.H>
#include <TbGenlib\hlinkobj.h>
#include <TbGenlib\HyperLink.h>
#include <TbGenlib\BaseApp.h>

#include <TbGenlibUI\TBExplorer.h>

#include <TbOledb\SqlRec.h>

#include <TbGes\dbt.h>
#include <TbGes\Tabber.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\BodyEdit.h>
#include <TbGes\FormMng.h>

#include <TbWoormEngine\EQNEDIT.H>

#include "MBodyEdit.h"
#include "MParsedControlsExtenders.h"
#include "QueryController.h"
#include "Utility.h"
#include "MFormatters.h"
#include "MDocument.h"
#include "UITypeEditors.h"
#include "MPanel.h"

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::ComponentModel::Design;
using namespace System::Drawing;
using namespace System::IO;
using namespace Microarea::TaskBuilderNet::UI::MenuManagerWindowsControls;
using namespace Microarea::TaskBuilderNet::Core::MenuManagerLoader;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;
using namespace Microarea::TaskBuilderNet::Core::CoreTypes;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms::Design;
using namespace System::Windows::Forms;
using namespace System::Resources;
using namespace System::ComponentModel;
using namespace System::Drawing::Design;


/////////////////////////////////////////////////////////////////////////////
// 				class ControlClassUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
void PropertyChangingNotifier::OnComponentPropertyChanged
		(
			System::IServiceProvider^ provider,
			IComponent^ component,
			String^ changingPropertyName,
			Object^ oldValue,
			Object^ newValue
		)
{
	ITBComponentChangeService^ ccs = (ITBComponentChangeService^) provider->GetService(IComponentChangeService::typeid);
	
	if (ccs == nullptr)
		return;

	PropertyDescriptorCollection^ propDescs = TypeDescriptor::GetProperties(component->GetType());
	if (propDescs == nullptr || propDescs->Count == 0)
		return;

	PropertyDescriptor^ pDesc = propDescs[changingPropertyName];
	if (pDesc == nullptr)
		return;

	ccs->OnComponentChanged(component, pDesc, oldValue, newValue);

	EasyBuilderComponent^ ebComponent = dynamic_cast<EasyBuilderComponent^>(component);
	if (ebComponent == nullptr)
		return;
	
	ebComponent->IsChanged = true;

	while (ebComponent->ParentComponent != nullptr)
	{
		EasyBuilderComponent^ temp = ebComponent->ParentComponent;
		temp->IsChanged = true;
		ebComponent = ebComponent->ParentComponent;
	}
}

//-----------------------------------------------------------------------------
void PropertyChangingNotifier::OnComponentAdded(IComponent^ parentComponent, IComponent^ component, bool updateSources)
{
	ITBComponentChangeService^ svc = nullptr;
	
	if (parentComponent->Site != nullptr)
	{
		svc = (ITBComponentChangeService^) parentComponent->Site->GetService(ITBComponentChangeService::typeid);
	}
	
	if (svc != nullptr)
	{
		svc->OnComponentAdded(parentComponent, component);
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class ControlClassUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
UITypeEditorEditStyle ControlClassUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ ControlClassUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);
	IControlClassConsumer^ consumer = (IControlClassConsumer^) context->Instance;

	if (consumer == nullptr)
		return UITypeEditor::EditValue(context, provider, value);

	// old value
	IControlClass^ oldValue = nullptr;
	CRegisteredParsedCtrl* pOldValueCtrl = NULL;
	if (value != nullptr)
	{
		oldValue = (IControlClass^) value;
		if (value->GetType() == ControlClass::typeid)
			pOldValueCtrl = ((ControlClass^) oldValue)->GetRegInfoPtr();
	}

	// user control
	TreeView^ treeView = BuildControlTree(pOldValueCtrl, oldValue, consumer);
	treeView->Tag = service;
	if (Control::typeid->IsInstanceOfType(service))
	{
		treeView->Width = ((Control^)service)->Width;
		treeView->Height = ((Control^)service)->Height * 2/3;
	}
	service->DropDownControl(treeView);

	// selection
	if (treeView->SelectedNode != nullptr && treeView->SelectedNode->Tag != nullptr)
	{
		CRegisteredParsedCtrl* pSelectedCtrl = (CRegisteredParsedCtrl*) ((IntPtr) treeView->SelectedNode->Tag).ToInt64(); 

		if	(oldValue == nullptr || pOldValueCtrl != pSelectedCtrl)
		{
			consumer->ClassType = gcnew ControlClass(pSelectedCtrl);
			PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) consumer, MParsedControlSerializer::ClassTypePropertyName, oldValue, consumer->ClassType);
		}
	}

	treeView->MouseDoubleClick -= gcnew MouseEventHandler(this, &ControlClassUITypeEditor::listBox_MouseClick);

	return UITypeEditor::EditValue(context, provider, value);
}

//-----------------------------------------------------------------------------
void ControlClassUITypeEditor::listBox_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	TreeView^ treeView = (TreeView^) sender;
			
	if (treeView == nullptr || treeView->SelectedNode == nullptr || treeView->SelectedNode->Tag == nullptr)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) treeView->Tag;
	service->CloseDropDown();
}

//-----------------------------------------------------------------------------
TreeNode^ ControlClassUITypeEditor::GetFamilyNode(TreeNode^ rootNode, String^ name)
{
	for each (TreeNode^ node in rootNode->Nodes)
		if (String::Compare (node->Text, name, true) == 0)
			return node;
	
	return nullptr;
}

//-----------------------------------------------------------------------------
TreeView^ ControlClassUITypeEditor::BuildControlTree (CRegisteredParsedCtrl* pOldValueCtrl, IControlClass^ oldValue, IControlClassConsumer^ consumer)
{
	// compatibilità di tipo
	IDataBindingConsumer^ dataBindingConsumer = nullptr;
	TaskBuilderNet::Core::CoreTypes::DataType^ compatibilityType = nullptr;

	if (IDataBindingConsumer::typeid->IsInstanceOfType(consumer))
	{
		dataBindingConsumer = (IDataBindingConsumer^) consumer;
		if (dataBindingConsumer->DataBinding != nullptr)
			compatibilityType = gcnew TaskBuilderNet::Core::CoreTypes::DataType
					(
						dataBindingConsumer->DataBinding->DataType->Type, 
						dataBindingConsumer->DataBinding->DataType->Tag
					);
	}

	// user control
	TreeView^ treeView = gcnew TreeView();
	treeView->SuspendLayout();
	treeView->AllowDrop = false;
	treeView->ImageList = gcnew ImageList(); 

	for each (Image^ image in EditorImages::ViewModelImages->Images)
		treeView->ImageList->Images->Add (image);
	
	Stream^ controlClassImage = this->GetType()->Assembly->GetManifestResourceStream("ControlClass.png");
	int controlClassImageIdx = 0;
	if (controlClassImage != nullptr)
	{
		controlClassImageIdx = treeView->ImageList->Images->Count;
		treeView->ImageList->Images->Add ((Bitmap^) Bitmap::FromStream(controlClassImage));
	}

	treeView->MouseDoubleClick += gcnew MouseEventHandler(this, &ControlClassUITypeEditor::listBox_MouseClick);

	CRegisteredParsedCtrl* pCtrl = NULL;
	CObArray arControls;
	AfxGetParsedControlsRegistry()->GetAllRegisteredControls (arControls);
	
	TreeNode^ rootNode = gcnew TreeNode(gcnew String(_TB("Control Families")));
	treeView->Nodes->Add(rootNode);

	for (int i=0; i <= arControls.GetUpperBound(); i++)
	{
		pCtrl = (CRegisteredParsedCtrl*) arControls.GetAt(i);
		const CParsedCtrlFamily* pFamily = pCtrl->GetFamily();

		if (!consumer->HasFamilyClassChangeable && pFamily != pOldValueCtrl->GetFamily())
			continue;

		if	(	pCtrl->GetDataType() == DATA_NULL_TYPE ||
				pCtrl->GetDataType() == DATA_ARRAY_TYPE ||
				pCtrl->GetDataType() == DATA_RECORD_TYPE || pCtrl->GetDataType() == DATA_SQLRECORD_TYPE ||
				pCtrl->GetDataType() == ::DataType(DATA_NULL_TYPE, ::DataObj::TB_VOID)
			) 
				continue;

		if (compatibilityType != nullptr)
		{
			if (pCtrl->GetDataType() == DATA_ENUM_TYPE)
			{
				if (compatibilityType->Type != pCtrl->GetDataType().m_wType)
					continue;
			}
			else if (compatibilityType->Type != pCtrl->GetDataType().m_wType ||
					compatibilityType->Tag != pCtrl->GetDataType().m_wTag)
				continue;
		}

		String^ familyName = gcnew String(pFamily->GetCaption());

		TreeNode^ familyNode = GetFamilyNode(rootNode, familyName);
		if (familyNode == nullptr)
		{
			Type^ familyType = Type::GetType(gcnew String(pFamily->GetQualifiedTypeName()));
			if (familyType == nullptr)
				continue;

			int imageIdx = 0;
			//----------------------------------------------------------------------------
			ResourceManager^ resources = gcnew ResourceManager(familyType);
			try
			{
				// cerco una risorsa embedded il cui namespace è lo stesso del controllo, con estensione png oppure jpg
				Stream^ stream = familyType->Assembly->GetManifestResourceStream(familyType->FullName + ".png");
				if (stream == nullptr)
					stream = familyType->Assembly->GetManifestResourceStream(familyType->FullName + ".jpg");
				if (stream != nullptr)
				{
					treeView->ImageList->Images->Add((Bitmap^) Bitmap::FromStream(stream));
					imageIdx = treeView->ImageList->Images->Count - 1;
				}
			}
			catch(Exception ^ e)
			{
				System::Diagnostics::Debug::Fail(e->ToString());
			}

			delete resources;
			familyNode = gcnew TreeNode(familyName, imageIdx, imageIdx);
			rootNode->Nodes->Add(familyNode);
		}
		String^ controlName = gcnew String(pCtrl->GetLocalizedText());

		TreeNode^ controlNode = gcnew TreeNode(controlName, controlClassImageIdx,controlClassImageIdx);
		controlNode->Tag = (IntPtr) (int) pCtrl;
		familyNode->Nodes->Add(controlNode);
		
		if	(oldValue != nullptr && pOldValueCtrl == pCtrl)
			treeView->SelectedNode = controlNode;
	}
	treeView->Sort();

	treeView->ResumeLayout();
	return treeView;
}

/////////////////////////////////////////////////////////////////////////////
// 				class FormatterUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
UITypeEditorEditStyle FormatterUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ FormatterUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);
	
	EasyBuilderControl^ control = 
		EasyBuilderControl::typeid->IsInstanceOfType(context->Instance) 
		? (EasyBuilderControl^) context->Instance
		: nullptr;

	if (control == nullptr)
		return UITypeEditor::EditValue(context, provider, value);
	
	IControlClassConsumer^ controlConsumer = nullptr;
	::DataType aCompatibleType = DATA_NULL_TYPE;

	if (IControlClassConsumer::typeid->IsInstanceOfType(control))
	{
		controlConsumer = (IControlClassConsumer^) control;
		if (controlConsumer->ClassType != nullptr && controlConsumer->ClassType->CompatibleType != nullptr)
			aCompatibleType = ::DataType ( controlConsumer->ClassType->CompatibleType->Type, controlConsumer->ClassType->CompatibleType->Tag);
	}

	CStringArray arNames;
	AfxGetFormatStyleTable()->GetCompatibleFormatterNames (aCompatibleType, arNames);
	
	// user control
	ListView^ listBox = gcnew ListView();
	listBox->AllowColumnReorder = false;
	listBox->AllowDrop = false;
	listBox->MultiSelect = false;
	listBox->View = System::Windows::Forms::View::SmallIcon;
	listBox->Tag = service;
	listBox->SmallImageList = EditorImages::ViewModelImages;
	listBox->MouseDoubleClick += gcnew MouseEventHandler(this, &FormatterUITypeEditor::listBox_MouseClick);

	String^ oldValue = value != nullptr ? ((FormatterStyle^) value)->StyleName : nullptr;

	for (int i=0; i <= arNames.GetUpperBound(); i++)
	{
		String^ text = gcnew String(arNames.GetAt(i));
		ListViewItem^ item = gcnew ListViewItem(text, (int)EditorImages::ViewModelImageIndex::Formatter);
		listBox->Items->Add(item);
		if (oldValue && String::Compare(oldValue, text, true) == 0)
			item->Selected = true;
	}

	service->DropDownControl(listBox);

	// no selection
	if (listBox->SelectedItems->Count > 0)
	{
		String^ newValue = listBox->SelectedItems[0]->Text;
		::Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter (CString(newValue), NULL);

		value = gcnew FormatterStyle (pFormatter);
		PropertyChangingNotifier::OnComponentPropertyChanged (provider, control, MParsedControlSerializer::FormatterPropertyName, oldValue, value);
	}

	listBox->MouseDoubleClick -= gcnew MouseEventHandler(this, &FormatterUITypeEditor::listBox_MouseClick);

	return UITypeEditor::EditValue(context, provider, value);
}

//-----------------------------------------------------------------------------
void FormatterUITypeEditor::listBox_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	ListView^ listView = (ListView^) sender;
			
	if (listView == nullptr || listView->SelectedItems->Count == 0)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) listView->Tag;
	service->CloseDropDown();
}

/////////////////////////////////////////////////////////////////////////////
// 				class DBTOneToManyUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
DBTOneToManyUITypeEditor::DBTOneToManyUITypeEditor ()
	:
	UITypeEditor()
{
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle DBTOneToManyUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ DBTOneToManyUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);

	IDataBindingConsumer^ dataBindingConsumer = (IDataBindingConsumer^) context->Instance;
	IComponent^ component = (IComponent^) context->Instance;

	if (dataBindingConsumer == nullptr)
		return UITypeEditor::EditValue(context, provider, value);

	// user control
	ListView^ listBox = gcnew ListView();
	listBox->AllowColumnReorder = false;
	listBox->AllowDrop = false;
	listBox->MultiSelect = false;
	listBox->View = System::Windows::Forms::View::SmallIcon;
	listBox->Tag = service;
	listBox->Width	= listBox->Width;
	listBox->Height	= listBox->Height;
	listBox->SmallImageList = EditorImages::DataModelImages;
	listBox->MouseDoubleClick += gcnew MouseEventHandler(this, &DBTOneToManyUITypeEditor::listBox_MouseClick);
	
	MDocument^ document = (MDocument^) component->Site->GetService(MDocument::typeid);

	if (document->Components->Count == 0)
		return UITypeEditor::EditValue(context, provider, value);

	ListViewItem^ item = listBox->Items->Add(gcnew String(_TB("None")), (int)EditorImages::ObjectModelImageIndex::None);

	for each (IComponent^ component in document->Components)
	{
		IDocumentSlaveDataManager^ dbt = dynamic_cast<IDocumentSlaveDataManager^>(component);
		if (dbt == nullptr)
			continue;

		if (dbt->Relation != DataRelationType::OneToMany)
			continue;
			
		ListViewItem^ item = gcnew ListViewItem(dbt->GetType()->Name, (int)EditorImages::ObjectModelImageIndex::Master);
		item->Tag = dbt;
		DBTDataBinding^ dds = (DBTDataBinding^)dataBindingConsumer->DataBinding;
		if (dds != nullptr && dds->Data != nullptr && dds->Data->Equals(dbt))
			item->Selected = true;
		listBox->Items->Add(item);
	}
	
	service->DropDownControl(listBox);

	if (listBox->SelectedItems->Count > 0)
	{
		dataBindingConsumer->DataBinding = listBox->SelectedItems[0]->Tag != nullptr ?  gcnew DBTDataBinding((MDBTSlaveBuffered^) listBox->SelectedItems[0]->Tag) : nullptr;
		if (dataBindingConsumer->DataBinding != nullptr && dataBindingConsumer->CanAutoFillFromDataBinding)
			dataBindingConsumer->AutoFillFromDataBinding(dataBindingConsumer->DataBinding, true);

		PropertyChangingNotifier::OnComponentPropertyChanged (provider, component, MParsedControlSerializer::DataBindingPropertyName, value, dataBindingConsumer->DataBinding);
	}
	
	listBox->MouseDoubleClick -= gcnew MouseEventHandler(this, &DBTOneToManyUITypeEditor::listBox_MouseClick);

	return UITypeEditor::EditValue(context, provider, dataBindingConsumer->DataBinding);
}

//-----------------------------------------------------------------------------
void DBTOneToManyUITypeEditor::listBox_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	ListView^ listView = (ListView^) sender;
			
	if (listView == nullptr || listView->SelectedItems->Count == 0)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) listView->Tag;
	service->CloseDropDown();
}


/////////////////////////////////////////////////////////////////////////////
// 				class QueryUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////

///----------------------------------------------------------------------------
UITypeEditorEditStyle QueryUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::Modal;
}

class RuleDataArray {};
class TblRuleData {};
//----------------------------------------------------------------------------
Object^ QueryUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);
	MGenericDataManager^ gdt = (MGenericDataManager^) context->Instance;

	if (gdt == nullptr)
		return UITypeEditor::EditValue(context, provider, value);

	String^ oldValue = (String^) value;
	CString strOutExpression = oldValue;
	SafeThreadCallContext^ sc = gcnew SafeThreadCallContext();
	try
	{
		SqlTable table;
		SymTable* pSymTable = gdt->GetSymTable();
		CQueryController checker(&table, pSymTable, gdt->GetTableInfoArray());
		//nella symbol table dell'editor metto solo la variabili non deprecate,
		//in quella del motore e del checker le lascio tutte altrimenti non compilano le vecchie personalizzazioni
		SymbolTable^ symTable = gcnew SymbolTable(nullptr);
		for (int i = 0; pSymTable && i < pSymTable->GetSize(); i++)
		{
			SymField* pF = pSymTable->GetAt(i);
			//if (pF->IsKindOf(RUNTIME_CLASS(DeprecatedSymField)))
			//	continue;
			symTable->Add(gcnew ExtendedVariable(gcnew String(pF->GetName())));
		}
		if (DoExpressionEditor(gdt->GetTableInfoArray(), symTable, strOutExpression, true, &checker, (CHECK_EXPRESSION) &CQueryController::OnCheckEdtWhereClause))
		{
			if (oldValue != value)
				PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) context->Instance, context->PropertyDescriptor->Name, oldValue, value);
			return gcnew String(strOutExpression);
		}
	}
	finally
	{
		delete sc;
	}
	return value;
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkFieldEditor Implementation
/////////////////////////////////////////////////////////////////////////////

///----------------------------------------------------------------------------
UITypeEditorEditStyle HotLinkFieldEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ HotLinkFieldEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);
	MHotLinkSearch^ search = (MHotLinkSearch^) context->Instance;
	IComponent^ component = (IComponent^) context->Instance;

	if (search == nullptr)
		return nullptr;

	// user control
	ListView^ listView = gcnew ListView();
	
	List<String^>^ fields = search->HotLink->GetCompatibleFieldNames(search->IsSearchByKey);

	for each (String^ fieldName in fields)
	{
		ListViewItem^ item = gcnew ListViewItem(fieldName, (int)EditorImages::ObjectModelImageIndex::DatabaseItem);
		item->Tag = fieldName;
		listView->Items->Add(item);
		if (fieldName->Equals(value))
			item->Selected = true;
	}
	listView->Sort();
	listView->AllowColumnReorder = false;
	listView->AllowDrop = false;
	listView->MultiSelect = false;
	listView->View = System::Windows::Forms::View::List;
	listView->Tag = service;
	listView->SmallImageList = EditorImages::DataModelImages;
	listView->Width = 100;
	listView->Height = 200;
	
	listView->MouseDoubleClick += gcnew MouseEventHandler(this, &HotLinkFieldEditor::listView_MouseClick);

	service->DropDownControl(listView);
	listView->MouseDoubleClick -= gcnew MouseEventHandler(this, &HotLinkFieldEditor::listView_MouseClick);
	
	// no selection
	if (listView->SelectedItems->Count > 0)
		return listView->SelectedItems[0]->Tag;

	return nullptr;
}

//-----------------------------------------------------------------------------
void HotLinkFieldEditor::listView_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	ListView^ listView = (ListView^) sender;
			
	if (listView == nullptr || listView->SelectedItems->Count == 0)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) listView->Tag;
	service->CloseDropDown();
}

/////////////////////////////////////////////////////////////////////////////
// 				class ItemSourcesUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
ItemSourcesUITypeEditor::ItemSourcesUITypeEditor(Type^ type)
	:
	CollectionEditor	(type)
{
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle ItemSourcesUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::Modal;
}

//----------------------------------------------------------------------------
Object^ ItemSourcesUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	EasyBuilderControl^ control = (EasyBuilderControl^) context->Instance;

	// non editable
	if (control == nullptr || !IItemsSourceConsumer::typeid->IsInstanceOfType(control) || !((IItemsSourceConsumer^) control)->IsItemsSourceEditable)
		return value;

	Object^ object = __super::EditValue(context, provider, value);
	PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) control, context->PropertyDescriptor->DisplayName, value, object);
	
	System::Collections::IList^ list = (System::Collections::IList^) value;
	if (list != nullptr)
		for each (IComponent^ cmp in list)
			PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) cmp, "ShowOnlyValueInPropertyGrid", false, true);

	return object;
}

///----------------------------------------------------------------------------
Object^ ItemSourcesUITypeEditor::CreateInstance (Type^ itemType)
{
	EasyBuilderControl^ control = (EasyBuilderControl^) Context->Instance;
	if (control == nullptr || control->CompatibleType == nullptr)
		return nullptr;
	
	IItemsSourceConsumer^ consumer = nullptr;
	if (IItemsSourceConsumer::typeid->IsInstanceOfType(control))
	{
		consumer = (IItemsSourceConsumer^) control;
		if (consumer->ItemsSource == nullptr)
			return nullptr;
	}

	MDataObj^ dataObj = MDataObj::Create((Microarea::TaskBuilderNet::Core::CoreTypes::DataType^) control->CompatibleType);
	dataObj->ShowOnlyValueInPropertyGrid = true;
	return dataObj;
}

///----------------------------------------------------------------------------
Type^ ItemSourcesUITypeEditor::CreateCollectionItemType ()
{
	return MDataObj::typeid;
}

/////////////////////////////////////////////////////////////////////////////
// 				class EasyBuilderComponentsEditor Implementation
/////////////////////////////////////////////////////////////////////////////
EasyBuilderComponentsEditor::EasyBuilderComponentsEditor (Type^ type)
	:
	CollectionEditor (type),
	changesCancelled (false)
{
}

///----------------------------------------------------------------------------
EasyBuilderComponentsEditor::~EasyBuilderComponentsEditor()
{
	this->!EasyBuilderComponentsEditor();
	GC::SuppressFinalize(this);
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::!EasyBuilderComponentsEditor()
{
	propertyGrid = nullptr;
	addButton = nullptr;
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle EasyBuilderComponentsEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::Modal;
}

///----------------------------------------------------------------------------
Object^ EasyBuilderComponentsEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	changesCancelled = false;

	if (value != nullptr && !value->GetType()->IsSubclassOf(EasyBuilderComponents::typeid))
		return __super::EditValue(context, provider, value);

	currentCollection = (EasyBuilderComponents^) value;
	if (currentCollection == nullptr)
		return value;

	currentCollection->InitializeForUI ();

	/// totalmente inutile editare 
	if (!currentCollection->CanBeModifiedByUI && !currentCollection->CanBeExtendedByUI && !currentCollection->CanBeReducedByUI)
		return value;

	EasyBuilderComponent^ parentObject = (EasyBuilderComponent^) context->Instance;

	EasyBuilderComponents^ oldCollection = currentCollection->Clone();

	// non c'è il contesto per l'editing
	if (currentCollection != nullptr && !currentCollection->IsEditable())
	{
		if (!String::IsNullOrEmpty(currentCollection->GetEditableError()))
			throw gcnew ApplicationException(currentCollection->GetEditableError());
		return value;
	}

	Object^ returnedObject = __super::EditValue(context, provider, value);

	if (returnedObject != nullptr) 
	{
		EasyBuilderComponents^ returnedCollection = (EasyBuilderComponents^) returnedObject;
		if (
				!changesCancelled &&
				(
					returnedCollection != currentCollection || returnedCollection->HasChanged()
				)
			)
		{
			returnedCollection->ApplyChanges();
			PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) parentObject, context->PropertyDescriptor->DisplayName, oldCollection, returnedCollection->OriginalCollection);
		}
	}

	return returnedObject;
}

///----------------------------------------------------------------------------
CollectionEditor::CollectionForm^ EasyBuilderComponentsEditor::CreateCollectionForm()
{
	propertyGrid = nullptr;

	CollectionEditor::CollectionForm^ form = __super::CreateCollectionForm();
	
	bool found = false;
	for each (Control^ control in form->Controls)
	{
		for each (Control^ childControl in control->Controls)
		{
			for each (Control^ subChildControl in childControl->Controls)
			{
				if (subChildControl->GetType() == SplitButton::typeid && subChildControl->Name == "addButton")
				{
					addButton = (ButtonBase^) subChildControl;
					addButton->EnabledChanged += gcnew EventHandler  (this, &EasyBuilderComponentsEditor::AddButtonEnabledChanged);
					break;
				}
			}
			if (childControl->GetType()->IsSubclassOf(PropertyGrid::typeid))
			{
				found = true;
				propertyGrid = ((PropertyGrid^) childControl);
				propertyGrid->SelectedObjectsChanged += gcnew EventHandler(this, &EasyBuilderComponentsEditor::SelectedObjectChanged);
				break;
			}

			if (found)
				break;
		}	
	}

	form->Closing += gcnew CancelEventHandler(this, &EasyBuilderComponentsEditor::OnCollectionFormClosing);

	if (form->AcceptButton != nullptr)
		((Button^) form->AcceptButton)->Click += gcnew EventHandler(this, &EasyBuilderComponentsEditor::OnOkCancelClicked);

	if (form->CancelButton != nullptr)
		((Button^) form->CancelButton)->Click += gcnew EventHandler(this, &EasyBuilderComponentsEditor::OnOkCancelClicked);
	
	return form;
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::OnCollectionFormClosing(Object^ sender, CancelEventArgs^ e)
{
	CollectionEditor::CollectionForm^ form = (CollectionEditor::CollectionForm^) sender;
	
	if (addButton != nullptr)
		addButton->EnabledChanged -= gcnew EventHandler  (this, &EasyBuilderComponentsEditor::AddButtonEnabledChanged);
	if (propertyGrid)
		propertyGrid->SelectedObjectsChanged -= gcnew EventHandler(this, &EasyBuilderComponentsEditor::SelectedObjectChanged);
	
	if (form->AcceptButton != nullptr)
		((Button^) form->AcceptButton)->Click -= gcnew EventHandler(this, &EasyBuilderComponentsEditor::OnOkCancelClicked);

	if (form->CancelButton != nullptr)
		((Button^) form->CancelButton)->Click -= gcnew EventHandler(this, &EasyBuilderComponentsEditor::OnOkCancelClicked);

	form->Closing -= gcnew CancelEventHandler (this, &EasyBuilderComponentsEditor::OnCollectionFormClosing);
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::SelectedObjectChanged(Object^ sender, EventArgs^ e)
{
	if (
		propertyGrid->SelectedObjects != nullptr &&
		propertyGrid->SelectedObjects->Length > 0
		)
	{
		propertyGrid->Site = ((IComponent^)propertyGrid->SelectedObject)->Site;
	}

	AddButtonEnabledChanged(sender, e);
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::OnOkCancelClicked(Object^ sender, EventArgs^ e)
{
	if (propertyGrid != nullptr)
		propertyGrid->Site = nullptr;
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::AddButtonEnabledChanged (Object^ sender, EventArgs^ e)
{
	if (addButton && addButton->Enabled && currentCollection != nullptr && !currentCollection->CanBeExtendedByUI)
		addButton->Enabled = false;
}

///----------------------------------------------------------------------------
bool EasyBuilderComponentsEditor::CanRemoveInstance (Object^ value)
{
	return currentCollection != nullptr && currentCollection->CanBeReducedByUI && !((EasyBuilderComponent^) value)->HasCodeBehind;;
}

///----------------------------------------------------------------------------
void EasyBuilderComponentsEditor::CancelChanges()
{
	changesCancelled = true;
}

///----------------------------------------------------------------------------
Object^ EasyBuilderComponentsEditor::CreateInstance (Type^ itemType)
{
	if (currentCollection == nullptr)
		return __super::CreateInstance(itemType);

	// la collection non può essere estesa
	if (!currentCollection->CanBeExtendedByUI)
		return nullptr;

	EasyBuilderComponent^ newInstance = currentCollection->CreateNewInstance();
	if (newInstance != nullptr)
	{
		EasyBuilderComponent^ parentObject = (EasyBuilderComponent^) Context->Instance;
		PropertyChangingNotifier::OnComponentAdded (parentObject, newInstance, false);
		return newInstance;	
	}

	return __super::CreateInstance(itemType);
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkSearchesUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
HotLinkSearchesUITypeEditor::HotLinkSearchesUITypeEditor(Type^ type)
	:
	EasyBuilderComponentsEditor	(type)
{
}

//----------------------------------------------------------------------------
Object^ HotLinkSearchesUITypeEditor::EditValue (ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) 
{
	return __super::EditValue(context, provider, ((EasyBuilderComponents^) value)->Clone());
}

//----------------------------------------------------------------------------
bool HotLinkSearchesUITypeEditor::CanRemoveInstance	(Object^ value) 
{
	return __super::CanRemoveInstance(value) && ((EasyBuilderComponent^) value)->Name != MHotLinkSearch::SearchByKeyName;
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
HotLinkUITypeEditor::HotLinkUITypeEditor ()
	:
	UITypeEditor()
{
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle HotLinkUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ HotLinkUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);

	IComponent^ component = (IComponent^) context->Instance;

	// user control
	ListView^ listBox = gcnew ListView();
	listBox->AllowColumnReorder = false;
	listBox->AllowDrop = false;
	listBox->MultiSelect = false;
	listBox->View = System::Windows::Forms::View::SmallIcon;
	listBox->Tag = service;
	listBox->Width	= listBox->Width;
	listBox->Height	= listBox->Height;
	listBox->SmallImageList = EditorImages::DataModelImages;
	listBox->MouseDoubleClick += gcnew MouseEventHandler(this, &HotLinkUITypeEditor::listBox_MouseClick);
	
	MDocument^ document = (MDocument^) component->Site->GetService(MDocument::typeid);
	
	ListViewItem^ item = listBox->Items->Add(gcnew String(_TB("None")), (int)EditorImages::ObjectModelImageIndex::None);
	for each (IComponent^ cmp in document->Components)
	{
		if (!MHotLink::typeid->IsInstanceOfType(cmp))
			continue;
		MHotLink^ hkl = (MHotLink^) cmp;
		ListViewItem^ item = listBox->Items->Add(hkl->Name, (int)EditorImages::ObjectModelImageIndex::HotLink);
		item->Tag = hkl;
	}
	
	service->DropDownControl(listBox);

	listBox->MouseDoubleClick -= gcnew MouseEventHandler(this, &HotLinkUITypeEditor::listBox_MouseClick);
	if (listBox->SelectedItems->Count > 0)
		return listBox->SelectedItems[0]->Tag;
	
	return value;
}

//-----------------------------------------------------------------------------
void HotLinkUITypeEditor::listBox_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	ListView^ listView = (ListView^) sender;
			
	if (listView == nullptr || listView->SelectedItems->Count == 0)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) listView->Tag;
	service->CloseDropDown();
}

/////////////////////////////////////////////////////////////////////////////
// 				class MenuCommandSelector Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
ref class MenuCommandSelector : public MenuManagerDialog
{
	String^ selectedCommandItemObject;
	String^ initialSelection;
	static String^ prefix = NameSpaceObjectType::Document.ToString() + ".";
public:
	//---------------------------------------------------------------------------------------------
	MenuCommandSelector(Microarea::TaskBuilderNet::Core::MenuManagerLoader::MenuLoader^ loader, String^ initialSelection) 
		: MenuManagerDialog (loader)
	{
		this->initialSelection = initialSelection;
		if (!String::IsNullOrEmpty(initialSelection) && initialSelection->StartsWith(prefix))
			this->initialSelection = initialSelection->Substring(prefix->Length);
	}

	//---------------------------------------------------------------------------------------------
	property String^ SelectedCommand 
	{
		String^ get() { return prefix + selectedCommandItemObject; } 
	}
protected:
	//---------------------------------------------------------------------------------------------
	virtual void OnSelectedCommandChanged(MenuMngCtrlEventArgs^ e) override
	{
		selectedCommandItemObject = (e != nullptr) ? e->ItemObject : String::Empty;
	}
	//--------------------------------------------------------------------------------------------------------------------------------
	virtual void OnMenuShowed() override 
	{
		// Invoke base class implementation
		__super::OnMenuShowed();

		if (!String::IsNullOrEmpty(initialSelection))
			MenuManagerWinCtrl->SelectDocumentNodeFromItemObject(initialSelection);
	}

};

/////////////////////////////////////////////////////////////////////////////
// 				class DocumentUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
DocumentUITypeEditor::DocumentUITypeEditor ()
	:
	UITypeEditor()
{
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle DocumentUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::Modal;
}

//----------------------------------------------------------------------------
Object^ DocumentUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	if (context->PropertyDescriptor->IsReadOnly)
		return value;

	IUIService^ service = (IUIService^) provider->GetService(IUIService::typeid);
	IComponent^ component = (IComponent^) context->Instance;
	PathFinder^ pf = gcnew PathFinder(CUtility::GetCompany(), CUtility::GetUser());
	MenuLoader^ loader = gcnew MenuLoader(pf, false);
	loader->LoadAllMenus(MenuLoader::CommandsTypeToLoad::Form, true, false);
	MenuCommandSelector^ selAppCmdDlg = gcnew MenuCommandSelector(loader, (String^)value);
	if (service->ShowDialog(selAppCmdDlg) == DialogResult::OK && !String::IsNullOrEmpty(selAppCmdDlg->SelectedCommand))
		return selAppCmdDlg->SelectedCommand;
	
	return value;
}

/////////////////////////////////////////////////////////////////////////////
// 				class DataObjUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
void DataObjUITypeEditor::OnAttachPropertyEditor(TextBox^ textBox, MDataObj^ dataObj)
{
	String^ ctrlClass = "";
	String^ name = "tmp";
	Type^ ctrlType = MParsedControl::GetDefaultControlType(dataObj->DataType, false, ctrlClass);
	if (ctrlType == nullptr)
		return;

	//Creo l'istanza dell'oggetto via reflection specificando il type
	panel = gcnew LocalHost(textBox);
	textBox->Parent->Controls->Add(panel);
	textBox->SizeChanged += gcnew EventHandler(this, &DataObjUITypeEditor::TextBox_SizeChanged);
	panel->BringToFront();
	panel->Wrapper = (MParsedControl^ )System::Activator::CreateInstance(ctrlType, gcnew array<Object^>(1) { nullptr });
	UINT id = AfxGetTBResourcesMap()->GetTbResourceID(CString(name), TbControls);
	panel->Wrapper->Id = name;
	panel->Wrapper->Create(panel, System::Drawing::Rectangle(0, 0, panel->Width, panel->Height), name, ctrlClass);
	panel->Wrapper->DataBinding = gcnew FieldDataBinding(dataObj);
	panel->Wrapper->Font = textBox->Font;
	panel->Wrapper->Focus();
}

//--------------------------------------------------------------------------------
void DataObjUITypeEditor::OnDetachPropertyEditor(TextBox^ textBox, MDataObj^ mDataObj)
{
	Clear();
}

//--------------------------------------------------------------------------------
void DataObjUITypeEditor::Clear()
{
	if (panel == nullptr)
		return;
			
	if (panel->Parent != nullptr)
		panel->Parent->Controls->Remove(panel);
	delete panel;
	panel = nullptr;
}

//--------------------------------------------------------------------------------
void DataObjUITypeEditor::TextBox_SizeChanged(Object^ sender, EventArgs^ e)
{
	if (panel == nullptr)
		return;

	panel->TextBox_SizeChanged(sender, e);
}

/////////////////////////////////////////////////////////////////////////////
// 				class DataFileUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
DataFileUITypeEditor::DataFileUITypeEditor ()
	:
	UITypeEditor()
{
}

///----------------------------------------------------------------------------
UITypeEditorEditStyle DataFileUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context) 
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ DataFileUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	if (context->PropertyDescriptor->IsReadOnly)
		return value;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) provider->GetService(IWindowsFormsEditorService::typeid);

	TreeView^ treeView = gcnew TreeView();
	
	treeView->MouseDoubleClick += gcnew MouseEventHandler(this, &DataFileUITypeEditor::treeView_MouseClick);

	MFileItemsSource^ fileSources = (MFileItemsSource^) context->Instance;
	if (fileSources == nullptr)
		return nullptr;

	treeView->Tag = service;
	BuildTree(treeView, value, fileSources->UseCountry);

	service->DropDownControl(treeView);

	// selection
	if (treeView->SelectedNode != nullptr && treeView->SelectedNode->Tag != nullptr)
	{
		String^ oldValue = fileSources->FileNamespace;
		fileSources->FileNamespace = gcnew String((String^) treeView->SelectedNode->Tag);
		PropertyChangingNotifier::OnComponentPropertyChanged (provider, (IComponent^) fileSources, "FileNamespace", oldValue, value);
	}

	treeView->MouseDoubleClick -= gcnew MouseEventHandler(this, &DataFileUITypeEditor::treeView_MouseClick);
	return UITypeEditor::EditValue(context, provider, value);
}

//----------------------------------------------------------------------------
void DataFileUITypeEditor::BuildTree(TreeView^ treeView, Object^ oldValue, bool useCountry)
{
	treeView->Width	= treeView->Width * 2;
	treeView->Height= treeView->Height * 2; // TODOBRUNA
	
	treeView->Nodes->Clear();

	List<String^>^ appTitles	= gcnew List<String^>();
	List<String^>^ modTitles	= gcnew List<String^>();
	List<String^>^ namespaces	= gcnew List<String^>();
	CUtility::GetDataFiles(appTitles, modTitles, namespaces, useCountry);
	if (namespaces->Count == 0)
		return;

	String^ oldNs = (String^) oldValue;

	// carico le imamgini dall'assembli locale
	treeView->ImageList = gcnew ImageList(); 
	Stream^ appStream = this->GetType()->Assembly->GetManifestResourceStream("Application.png");
	if (appStream != nullptr) treeView->ImageList->Images->Add ((Bitmap^) Bitmap::FromStream(appStream));
	Stream^ modStream = this->GetType()->Assembly->GetManifestResourceStream("Module.png");
	if (modStream != nullptr) treeView->ImageList->Images->Add ((Bitmap^) Bitmap::FromStream(modStream));
	Stream^ docStream = this->GetType()->Assembly->GetManifestResourceStream("Document.png");
	if (docStream != nullptr)  treeView->ImageList->Images->Add ((Bitmap^) Bitmap::FromStream(docStream));

	for (int i = 0; i < namespaces->Count; i++)
	{
		String^ app = appTitles[i];
		String^ mod = modTitles[i];
		// il namespace C# non gestisce i DataFile
		CTBNamespace aNs (namespaces[i]->ToString());
		
		TreeNode^ appNode = GetNode(treeView->Nodes, app, 0);
		TreeNode^ modNode = GetNode(appNode->Nodes, mod,  1);
		TreeNode^ fileNode = GetNode(modNode->Nodes, gcnew String(aNs.GetObjectName()), 2);
		fileNode->Tag = namespaces[i];
		if (oldNs != nullptr && String::Compare(oldNs, namespaces[i] , true) == 0)
			treeView->SelectedNode = fileNode;
	}
	treeView->ExpandAll();
}

//-----------------------------------------------------------------------------
TreeNode^ DataFileUITypeEditor::GetNode(TreeNodeCollection^ nodes, String^ text, int imageIndex)
{
	for each (TreeNode^ node in nodes)
		if (node->Text == text)
			return node;

	TreeNode^ n = gcnew TreeNode(text, imageIndex, imageIndex);
	nodes->Add(n);
	
	return n;
}

//-----------------------------------------------------------------------------
void DataFileUITypeEditor::treeView_MouseClick(Object^ sender, MouseEventArgs^ e)
{
	TreeView^ treeView = (TreeView^) sender;
			
	if (treeView == nullptr || treeView->SelectedNode == nullptr ||  treeView->SelectedNode->Tag == nullptr)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^) treeView->Tag;
	service->CloseDropDown();
}

/////////////////////////////////////////////////////////////////////////////
// 				class NamespaceUITypeEditor Implementation
/////////////////////////////////////////////////////////////////////////////
///----------------------------------------------------------------------------
UITypeEditorEditStyle ModuleNamespaceUITypeEditor::GetEditStyle(ITypeDescriptorContext^ context)
{
	return UITypeEditorEditStyle::DropDown;
}

//----------------------------------------------------------------------------
Object^ ModuleNamespaceUITypeEditor::EditValue(ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value)
{
	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^)provider->GetService(IWindowsFormsEditorService::typeid);

	BaseWindowWrapper ^ control =
		BaseWindowWrapper::typeid->IsInstanceOfType(context->Instance)
		? (BaseWindowWrapper^)context->Instance
		: nullptr;
	if (control == nullptr)
		return UITypeEditor::EditValue(context, provider, value);
	bool isEasyStudioPanel = MEasyStudioPanel::typeid->IsInstanceOfType(control);

	// user control
	TreeView^ treeView = gcnew TreeView();
	treeView->AllowDrop = false;
	treeView->Width = treeView->Width * 2;
	treeView->Height = treeView->Height * 2; // TODOBRUNA
	treeView->ImageList = EditorImages::DataModelImages;
	treeView->Tag = service;

	treeView->MouseDoubleClick += gcnew MouseEventHandler(this, &ModuleNamespaceUITypeEditor::MouseClick);


	//non posso non avere un context sulla root dell'oggetto, non saprei dove salvare
	if (!isEasyStudioPanel)
	{
		TreeNode^ nullNode = gcnew TreeNode();
		nullNode->Text = gcnew String(_TB("None"));
		nullNode->ImageIndex = (int)EditorImages::ObjectModelImageIndex::None;
		nullNode->ImageIndex = 0;
		nullNode->Tag = gcnew NameSpace("");
		if (nullNode->Tag->Equals(value))
			treeView->SelectedNode = nullNode;
		treeView->Nodes->Add(nullNode);
	}
	for each(BaseApplicationInfo^ ai in BasePathFinder::BasePathFinderInstance->ApplicationInfos)
	{
		TreeNode^ appNode = gcnew TreeNode();
		appNode->Text = ai->Name;
		appNode->ImageIndex = 0;
		treeView->Nodes->Add(appNode);
		for each(BaseModuleInfo^ mi in ai->Modules)
		{
			TreeNode^ modNode = gcnew TreeNode();
			modNode->Text = mi->Name;
			modNode->ImageIndex = 1;
			modNode->Tag = mi->NameSpace;
			if (modNode->Tag->Equals(value))
				treeView->SelectedNode = modNode;
			appNode->Nodes->Add(modNode);
			//un oggetto root può avere come contesto un documento,
			//gli altri (client forms) possono essere associati solo ad un module
			if (isEasyStudioPanel)
			{
				for each(DocumentInfo^ di in mi->Documents)
				{
					TreeNode^ docNode = gcnew TreeNode();
					docNode->Text = di->Name;
					docNode->ImageIndex = 1;
					docNode->Tag = di->NameSpace;
					modNode->Nodes->Add(docNode);
					if (docNode->Tag->Equals(value))
						treeView->SelectedNode = docNode;
				}
			}
		}
	}
	service->DropDownControl(treeView);

	treeView->MouseDoubleClick -= gcnew MouseEventHandler(this, &ModuleNamespaceUITypeEditor::MouseClick);

	return UITypeEditor::EditValue(context, provider, value);
}

//-----------------------------------------------------------------------------
void ModuleNamespaceUITypeEditor::MouseClick(Object^ sender, MouseEventArgs^ e)
{
	TreeView^ treeView = (TreeView^)sender;

	if (treeView == nullptr || treeView->SelectedNode == nullptr || treeView->SelectedNode->Tag == nullptr)
		return;

	IWindowsFormsEditorService^ service = (IWindowsFormsEditorService^)treeView->Tag;
	service->CloseDropDown();
}

