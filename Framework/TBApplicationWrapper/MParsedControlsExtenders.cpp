#include "StdAfx.h"

#include <TbGenlib\ParsRes.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\AddressEdit.hjson> //JSON AUTOMATIC UPDATE
#include <TbGeneric\TBThemeManager.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbGes\DBT.h>
#include <TbGes\EXTDOC.H>
#include <TbGes\BEColumnInfo.H>
#include <TbGes\CAddressDlg.hjson> //JSON AUTOMATIC UPDATE


#include "MBodyEdit.h"
#include "MParsedControlsExtenders.h"
#include "GenericControls.h"
#include <TbGenLibManaged\\UserControlHandlersMixed.h>
#include "MDocument.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Core;

using namespace System;
using namespace System::CodeDom;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design::Serialization;

//=============================================================================
//						MManagedControl
//=============================================================================

//----------------------------------------------------------------------------
MManagedControl::MManagedControl(System::IntPtr handleWndPtr)
	:
	MParsedControl(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MManagedControl::MManagedControl(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MParsedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void MManagedControl::Site::set(ISite^ site)
{
	__super::Site = site;
	Ctrl->Site = (ITBSite^)((ITBSite^)site)->Clone();
	Ctrl->Site->Name = System::String::Concat(Ctrl->Site->Name, ".", TheControlPropertyName);
}

//----------------------------------------------------------------------------
Control^ MManagedControl::Ctrl::get()
{
	CManagedCtrl* pManCtrl = (CManagedCtrl*)GetWnd();
	CUserControlHandlerMixed* pHandler = (CUserControlHandlerMixed*)pManCtrl->GetHandler();
	return pHandler->GetWinControl();
}

//=============================================================================
//						MTextBox
//=============================================================================

//----------------------------------------------------------------------------
MTextBox::MTextBox(System::IntPtr handleWndPtr)
	:
	MManagedControl(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MTextBox::MTextBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	MManagedControl(parentWindow, name, controlClass, location, hasCodeBehind)
{

}

//----------------------------------------------------------------------------
TextBox^ MTextBox::TheControl::get()
{
	return (TextBox^)Ctrl;
}

//=============================================================================
//						MStateObject
//=============================================================================

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MStateObjectEventArgs::MStateObjectEventArgs(MDataObj^ dataObj)
{
	this->dataObj = dataObj;
}

//----------------------------------------------------------------------------
MDataBool^ MStateObjectEventArgs::StateData::get()
{
	return dataObj->DataType == CoreTypes::DataType::Bool ? ((MDataBool^)dataObj) : nullptr;
}

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MStateObject::MStateObject(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	EasyBuilderComponentExtender(owner, parentComponent, name),
	m_pStateCtrl(NULL)
{

	privateDataObj = gcnew MDataBool();

	// se mi arriva un owner, mi aggancio all'esistente 
	AttachExistingStateCtrl();
}

//----------------------------------------------------------------------------
MStateObject::~MStateObject()
{
	this->!MStateObject();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MStateObject::!MStateObject()
{
	if (Control != nullptr && Control->Handle != IntPtr::Zero)
	{
		CWnd* pWnd = Control->GetWnd();
		if (m_pStateCtrl && pWnd && IsWindow(pWnd->m_hWnd))
		{
			CParsedCtrl* pCtrl = ::GetParsedCtrl(Control->GetWnd());
			if (pCtrl && m_pStateCtrl->GetDataObj() && !HasCodeBehind)
			{
				// in questo modo il control si ridimensioni normale mentre fa il detaching
				Style = ButtonStyle::NoButton;
			}
		}
		m_pStateCtrl = NULL;
	}
	delete privateDataObj;
}

//----------------------------------------------------------------------------
bool MStateObject::ControlAlwaysDisabled::get()
{
	return m_pStateCtrl ? m_pStateCtrl->IsManualReadOnly() == TRUE : false;
}

//----------------------------------------------------------------------------
void MStateObject::ControlAlwaysDisabled::set(bool value)
{
	if (m_pStateCtrl)
	{
		m_pStateCtrl->SetManualReadOnly(value);
		NotifyParentChanged();
	}
}

//----------------------------------------------------------------------------
bool MStateObject::ButtonAlwaysDisabled::get()
{
	return m_pStateCtrl ? m_pStateCtrl->IsManuallyDisabled() == TRUE : false;
}

//----------------------------------------------------------------------------
void MStateObject::ButtonAlwaysDisabled::set(bool value)
{
	if (m_pStateCtrl)
	{
		m_pStateCtrl->DisableButton(value);
		NotifyParentChanged();
	}
}

//----------------------------------------------------------------------------
bool MStateObject::ControlEnabledInEditMode::get()
{
	return m_pStateCtrl ? m_pStateCtrl->IsCtrlEnabledInEditMode() == TRUE : false;
}

//----------------------------------------------------------------------------
void MStateObject::ControlEnabledInEditMode::set(bool value)
{
	if (m_pStateCtrl)
	{
		m_pStateCtrl->EnableCtrlInEditMode(value);
		NotifyParentChanged();
	}
}

//----------------------------------------------------------------------------
bool MStateObject::ButtonEnabledInEditMode::get()
{
	return m_pStateCtrl ? m_pStateCtrl->IsStateEnabledInEditMode() == TRUE : false;
}

//----------------------------------------------------------------------------
void MStateObject::ButtonEnabledInEditMode::set(bool value)
{
	if (m_pStateCtrl)
	{
		m_pStateCtrl->EnableStateInEditMode(value);
		NotifyParentChanged();
	}
}

//----------------------------------------------------------------------------
IDataBinding^ MStateObject::DataBinding::get()
{
	if (dataBinding != nullptr || HasCodeBehind)
		return dataBinding;

	// attaching del databinding privato se ci vuole
	::DataObj* pDataObj = m_pStateCtrl ? m_pStateCtrl->GetDataObj() : NULL;
	if (!pDataObj || pDataObj == privateDataObj->GetDataObj())
	{
		if (!pDataObj && Style != ButtonStyle::NoButton && Control != nullptr)
			AttachNewStateCtrl(::GetParsedCtrl(Control->GetWnd()), privateDataObj->GetDataObj());

		return nullptr;
	}

	// creazione del databinding automatico sull'esistente
	CParsedCtrl* pCtrl = ::GetParsedCtrl(Control->GetWnd());
	if (!pDataObj || !pCtrl || !pCtrl->m_pSqlRecord)
		return dataBinding;

	WindowWrapperContainer^ container = (WindowWrapperContainer^)Control->Parent;
	if (container->Document == nullptr)
		return dataBinding;

	MDocument^ document = (MDocument^)container->Document;
	IDataManager^ dataManager = document->GetDataManager(pCtrl->m_pSqlRecord);
	MSqlRecord^ mSqlRecord = dataManager == nullptr ? nullptr : (MSqlRecord^)dataManager->Record;

	if (mSqlRecord == nullptr)
		mSqlRecord = gcnew MSqlRecord(pCtrl->m_pSqlRecord);

	System::String^ colName = gcnew System::String(pCtrl->m_pSqlRecord->GetColumnName(pDataObj));
	IRecordField^ recField = mSqlRecord->GetField(colName);
	MDataObj^ mDataObj = recField == nullptr ? nullptr : (MDataObj^)recField->DataObj;

	if (mDataObj == nullptr)
		mDataObj = MDataObj::Create(pDataObj);

	dataBinding = gcnew FieldDataBinding(mDataObj, dataManager);
	if (dataManager != nullptr)
		((EasyBuilderComponent^)dataManager)->AddReferencedBy(this->SerializedName);

	return dataBinding;
}

//----------------------------------------------------------------------------
void MStateObject::DataBinding::set(IDataBinding^ value)
{
	if (!Control)
		return;

	CParsedCtrl* pCtrl = Control != nullptr ? ::GetParsedCtrl(Control->GetWnd()) : NULL;

	// detaching
	if (dataBinding)
	{
		EasyBuilderComponent^ parent = (EasyBuilderComponent^) this->dataBinding->Parent;
		if (parent != nullptr)
			parent->RemoveReferencedBy(this->SerializedName);

		pCtrl->DetachStateData(((MDataObj^)(dataBinding->Data))->GetDataObj());
		m_pStateCtrl = NULL;
	}
	// detaching del databinding privato
	else if (Style == ButtonStyle::NoButton)
	{
		pCtrl->DetachStateData(privateDataObj->GetDataObj());
		m_pStateCtrl = NULL;
	}

	dataBinding = (FieldDataBinding^)value;
	// gestione del databinding normale
	if (dataBinding)
	{
		::DataObj* pStateDataObj = ((MDataObj^)(dataBinding->Data))->GetDataObj();
		AttachNewStateCtrl(pCtrl, pStateDataObj);

		// di default TaskBuilder genera lo stile autonum
		if (Style == ButtonStyle::NoButton)
			style = ButtonStyle::AutoManual;

		EasyBuilderComponent^ parent = (EasyBuilderComponent^)dataBinding->Parent;
		if (parent != nullptr)
			parent->AddReferencedBy(this->SerializedName);
	}
	// gestione del databinding privato
	else if (Style != ButtonStyle::NoButton)
		AttachNewStateCtrl(pCtrl, privateDataObj->GetDataObj());

	// aggiorno l'interfaccia e notifico l'evento se non sono nel bodyedit,
	// mentre il bodyedit lo farà quando il control sarà acceso
	if (ParentComponent->GetType() != MBodyEditColumn::typeid)
		pCtrl->UpdateCtrlView();
	UpdateUIWrapping();
	NotifyParentChanged();
}

// nelle colonne di bodyedit lo state control è totalmente dinamico
// e viene attachato e reattachato in ShowCtrl e HideCtro, quindi devo
// predispormi ad attacharlo anche in modo ritardato rispetto alla
// costruzione dello state button stesso
//----------------------------------------------------------------------------
void MStateObject::AttachExistingStateCtrl()
{
	if (Control == nullptr)
	{
		m_pStateCtrl = NULL;
		return;
	}

	// se l'ho già attachato sono a posto
	if (m_pStateCtrl)
		return;

	CParsedCtrl* pCtrl = ::GetParsedCtrl(Control->GetWnd());
	// (nonostante sia un array non è mai stato aggiunto più di uno state button)
	if (pCtrl && pCtrl->GetStateCtrlsArray().GetSize() > 0)
	{
		m_pStateCtrl = (CStateCtrlObj*)pCtrl->GetStateCtrlsArray().GetAt(0);
		HasCodeBehind = true;
		InitExistingStyle();
		UpdateUIWrapping();
	}
}

//----------------------------------------------------------------------------
void MStateObject::AttachNewStateCtrl(CParsedCtrl* pCtrl, ::DataObj* pDataObj)
{
	// purtroppo sulla colonna devo chiamare un metodo differente
	if (ParentComponent->GetType() == MBodyEditColumn::typeid)
		((MBodyEditColumn^)ParentComponent)->GetColumnInfo()->AttachStateData(pDataObj, FALSE);
	else
		pCtrl->AttachStateData(pDataObj, FALSE);

	m_pStateCtrl = pCtrl->GetStateCtrl(pDataObj);
}

//----------------------------------------------------------------------------
void MStateObject::UpdateUIWrapping()
{
	if (ui != nullptr)
	{
		delete ui;
		ui = nullptr;
	}

	if (m_pStateCtrl && m_pStateCtrl->GetButton())
	{
		ui = gcnew BaseWindowWrapper((System::IntPtr) m_pStateCtrl->GetButton());
		ui->HasCodeBehind = true;
	}
}

//----------------------------------------------------------------------------
IDataManager^ MStateObject::FixedDataManager::get()
{
	if (ParentComponent == nullptr)
		return nullptr;

	// per gli SlaveBuffered fisso il DBT perchè il C++ attualmente 
	// lavora solo con i dataObj dei record di prototipo
	if (ParentComponent->GetType() == MBodyEditColumn::typeid)
	{
		MBodyEditColumn^ column = (MBodyEditColumn^)ParentComponent;
		if (column->DataBinding != nullptr && column->DataBinding->Parent != nullptr)
			return column->DataBinding->Parent;
	}

	return nullptr;
}

//----------------------------------------------------------------------------
Type^ MStateObject::ExcludedBindParentType::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^ MStateObject::ToString()
{
	return Style.ToString();
}

//----------------------------------------------------------------------------
void MStateObject::Style::set(ButtonStyle value)
{
	if (HasCodeBehind)
		return;

	bool dataBindingInvolved = value == ButtonStyle::NoButton || style == ButtonStyle::NoButton;
	if (value != ButtonStyle::AutoManual)
		ClearStateDescription();
	style = value;

	// sembra inutile ma ha lo scopo di far preparare
	// il databinding quando c'è di mezzo il NoButton
	if (dataBindingInvolved)
		DataBinding = nullptr;

	// quindi sistemo le immagini
	CString sFirstState, sSecondState;
	switch (style)
	{
	case ButtonStyle::AutoManual:
		sFirstState = ControlEditableStyle == EditableStyle::WhenTrue || ControlEditableStyle == EditableStyle::Default ? GetAutoImage() : GetManualImage();
		sSecondState = ControlEditableStyle == EditableStyle::WhenTrue || ControlEditableStyle == EditableStyle::Default ? GetManualImage() : GetAutoImage();
		break;
		case ButtonStyle::Arrow:
			sFirstState = sSecondState = TBIcon(szIconFolderFind, CONTROL);
			break;
		case ButtonStyle::Calendar:
			sFirstState = sSecondState = TBIcon(szIconCalendar, CONTROL);
			break;
		case ButtonStyle::Outlook:
			sFirstState = sSecondState = TBIcon(szIconAddressBook, CONTROL);
			break;
		case ButtonStyle::Color:
			sFirstState = sSecondState = TBIcon(szIconColors, CONTROL);
			break;
		case ButtonStyle::Internet:
			sFirstState = sSecondState = TBIcon(szIconAddress, CONTROL);
			break;
	}

	ControlEditableStyle = EditableStyle::Default;

	if (!m_pStateCtrl)
		return;

	m_pStateCtrl->SetCtrlStateSingle(0, sFirstState);
	m_pStateCtrl->SetCtrlStateSingle(1, sSecondState);

	CStateButton* pButton = (CStateButton*)m_pStateCtrl->GetButton();

	if (pButton)
	{
		// il disabled è sempre 3 id dopo
		pButton->SetStateBitmaps(0, sFirstState);
		pButton->SetStateBitmaps(1, sSecondState);
		if (pButton->IsWindowVisible())
		{
			pButton->Invalidate();
			pButton->UpdateWindow();
		}

		// devo reimpostare la size completa perchè attachando il bottone il control si è rimpicciolito 
		// automaticamente e se non notifico la size giusta si rimpicciolirà ad ogni salvataggio di EB
		if (ParentComponent->GetType() != MBodyEditColumn::typeid && dataBindingInvolved && Control != nullptr && Control->GetWnd())
			Control->Size = Size(Control->Size.Width + ::GetParsedCtrl(Control->GetWnd())->GetAllButtonsWitdh(), Control->Size.Height);
	}

	NotifyParentChanged();
}
//----------------------------------------------------------------------------
void MStateObject::DataSource::set(String^ dataSource)
{
	StateData* pDescri = GetStateDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		pDescri->m_pBindings->m_strDataSource = dataSource;
		if (!String::IsNullOrEmpty(dataSource))
			Style = ButtonStyle::AutoManual;
	}

}
//----------------------------------------------------------------------------
String^ MStateObject::DataSource::get()
{
	StateData* pDescri = GetStateDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);

	return "";
}

//----------------------------------------------------------------------------
void MStateObject::InvertState::set(bool invert)
{
	StateData* pDescri = GetStateDescription();
	if (pDescri)
	{
		pDescri->m_bInvertDefaultStates = invert ? B_TRUE : B_FALSE;
		if (invert)
			Style = ButtonStyle::AutoManual;
	}

}
//----------------------------------------------------------------------------
bool MStateObject::InvertState::get()
{
	StateData* pDescri = GetStateDescription();
	if (pDescri)
		return pDescri->m_bInvertDefaultStates == B_TRUE;

	return false;
}
//----------------------------------------------------------------------------
bool MStateObject::EmptyComponent::get()
{
	return Style == ButtonStyle::NoButton;
}

//----------------------------------------------------------------------------
MStateObject::ButtonStyle MStateObject::Style::get()
{
	return style;
}

//----------------------------------------------------------------------------
IDataType^ MStateObject::CompatibleDataType::get()
{
	return Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Bool;
}

// non posso usare quello del databinding perchè sulle colonnedel bodyedit 
// viene sempre riattachato il dataObj della riga corrente, mentre il 
// databinding lavora sul prototipo. Comunque questa è usata solo x lo StateButtonClicked
//----------------------------------------------------------------------------
MDataObj^ MStateObject::DataObj::get()
{
	// attach ritardato x bodyedit
	if (!m_pStateCtrl && Control != nullptr)
		AttachExistingStateCtrl();

	if (m_pStateCtrl && m_pStateCtrl->GetDataObj())
		return MDataObj::Create(m_pStateCtrl->GetDataObj());

	return Style == ButtonStyle::NoButton ? nullptr : privateDataObj;
}

//----------------------------------------------------------------------------
MStateObject::EditableStyle MStateObject::ControlEditableStyle::get()
{
	if (!m_pStateCtrl || !m_pStateCtrl->GetButton())
		return EditableStyle::Default;

	CStateButton* pButton = (CStateButton*)m_pStateCtrl->GetButton();

	CStateCtrlState* pFalseState = m_pStateCtrl->GetCtrlState(0);
	CStateCtrlState* pTrueState = m_pStateCtrl->GetCtrlState(1);

	if (pFalseState && pFalseState->IsToEnableCtrl() && pTrueState && pTrueState->IsToEnableCtrl())
		return EditableStyle::Always;

	if (pFalseState && pFalseState->IsToEnableCtrl() && pTrueState && !pTrueState->IsToEnableCtrl())
		return EditableStyle::WhenFalse;

	if (pFalseState && !pFalseState->IsToEnableCtrl() && pTrueState && pTrueState->IsToEnableCtrl())
		return EditableStyle::WhenTrue;

	return EditableStyle::Default;
}

//----------------------------------------------------------------------------
void MStateObject::ControlEditableStyle::set(MStateObject::EditableStyle style)
{
	if (!m_pStateCtrl || !m_pStateCtrl->GetButton())
		return;

	CStateButton* pButton = (CStateButton*)m_pStateCtrl->GetButton();

	CStateCtrlState* pFalseState = m_pStateCtrl->GetCtrlState(0);
	CStateCtrlState* pTrueState = m_pStateCtrl->GetCtrlState(1);

	bool falseStateValue = false;
	bool trueStateValue = false;
	// quindi sistemo le logiche di abilitazione/disabilitazione
	switch (style)
	{
	case EditableStyle::Default:
		falseStateValue = Style != ButtonStyle::AutoManual;
		trueStateValue = true;
		break;
	case EditableStyle::Always:
		falseStateValue = trueStateValue = true;
		break;
	case EditableStyle::WhenFalse:
		falseStateValue = true;
		trueStateValue = false;
		break;
	case EditableStyle::WhenTrue:
		falseStateValue = false;
		trueStateValue = true;
		break;
	}

	// calcolo dello stato corretto
	AdjustState(0, pFalseState, falseStateValue);
	AdjustState(1, pTrueState, trueStateValue);

	if (pButton->IsWindowVisible())
	{
		pButton->Invalidate();
		pButton->UpdateWindow();
	}

	NotifyParentChanged();
}

//----------------------------------------------------------------------------
CString MStateObject::GetAutoImage()
{
	return TBIcon(szIconExecute, CONTROL);

}

//----------------------------------------------------------------------------
CString MStateObject::GetManualImage()
{
	return TBIcon(szIconEdit, CONTROL);
}

//----------------------------------------------------------------------------
void MStateObject::AdjustState(int nState, CStateCtrlState* pState, bool isControlToEnable)
{
	if (!pState)
		return;

	UINT nNormalImage = pState->GetBmpNormalId();
	UINT nDisabledImage = pState->GetBmpNormalId();
	CString sImgNs = pState->GetBmpNS();
	UINT nOldImage = nNormalImage;

	if (nNormalImage > 0)
	{
		if (Style == ButtonStyle::AutoManual)
		{
			// l'autonumerazione deve ricalcolare bene anche il verso delle immagini
			//nNormalImage = isControlToEnable ? GetManualImage() : GetAutoImage();
			//nNormalImage  = ControlEditableStyle == EditableStyle::WhenTrue || ControlEditableStyle == EditableStyle::Default ? GetAutoImage() : GetManualImage();
			nDisabledImage = nNormalImage + 3;
		}

		pState->Set(nNormalImage, nDisabledImage, isControlToEnable);
		if (m_pStateCtrl && m_pStateCtrl->GetButton() && nOldImage != nNormalImage)
			((CStateButton*)m_pStateCtrl->GetButton())->SetStateBitmaps(nState, nNormalImage, nDisabledImage);
	}
	else
	{
		pState->Set(sImgNs, isControlToEnable);
	}
}

//----------------------------------------------------------------------------
void MStateObject::InitExistingStyle()
{
	if (!m_pStateCtrl)
		return;

	if (!m_pStateCtrl->GetButton())
	{
		style = ButtonStyle::NoButton;
		return;
	}

	CStateCtrlState* pState = m_pStateCtrl->GetCtrlState(0);

	if (!pState)
		style = ButtonStyle::NoButton;
	else
		style = ButtonStyle::AutoManual;
}

//----------------------------------------------------------------------------
bool MStateObject::CanExtendObject(IEasyBuilderComponentExtendable^ e)
{
	Object^ theObject = e->GetType() == MBodyEditColumn::typeid ? ((MBodyEditColumn^)e)->Control : e;
	return	theObject &&
		(
			theObject->GetType() == MParsedCombo::typeid ||
			theObject->GetType() == MParsedEdit::typeid
			);
}

//----------------------------------------------------------------------------
bool MStateObject::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "DataBinding")
		return Style != ButtonStyle::NoButton;

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
StateData* MStateObject::GetStateDescription()
{
	CWndObjDescription* pDesc = Control->GetWndObjDescription();
	if (!pDesc)
		return NULL;
	if (!pDesc->m_pStateData)
		pDesc->m_pStateData = new StateData();
	return pDesc->m_pStateData;
}
//----------------------------------------------------------------------------
void MStateObject::ClearStateDescription()
{
	CWndObjDescription* pDesc = Control->GetWndObjDescription();
	if (!pDesc)
		return;
	delete pDesc->m_pStateData;
	pDesc->m_pStateData = NULL;
}
/////////////////////////////////////////////////////////////////////////////
// 				class MFileItemsSource Implementation
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
Object^ MFileItemsSourceFieldSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	EasyBuilderComponent^ ebComponent = (EasyBuilderComponent^)current;

	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	System::String^ uniqueVarName = System::String::Format("{0}_{1}", ebComponent->ParentComponent->Name, ebComponent->SerializedName)->Replace(".", "_");

	VariableDeclarationStatement^ varDeclStatement = gcnew VariableDeclarationStatement
		(
			gcnew SimpleType(ebComponent->GetType()->Name), 
			uniqueVarName, 
				AstFacilities::GetObjectCreationExpression
				(
					gcnew SimpleType(ebComponent->GetType()->ToString()),
					gcnew IdentifierExpression(ebComponent->ParentComponent->SerializedName),
					gcnew PrimitiveExpression(ebComponent->SerializedName),
					gcnew PrimitiveExpression(ebComponent->HasCodeBehind)
				)
			);

	newCollection->Add(varDeclStatement);
	IdentifierExpression^ createExpr = gcnew IdentifierExpression(uniqueVarName);

	SetExpression(manager, ebComponent, createExpr, true);

	System::Collections::Generic::IList<Statement^>^ dbCollection = SerializeDataBinding(ebComponent, uniqueVarName);
	if (dbCollection != nullptr)
		newCollection->AddRange(dbCollection);

	return newCollection;
}

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MFileItemsSourceField::MFileItemsSourceField(EasyBuilderComponent^ parent, System::String^ name, bool hasCodeBehind)
{
	ParentComponent = parent;
	Name = name;
	HasCodeBehind = hasCodeBehind;

	AdjustSite();
}

//----------------------------------------------------------------------------
MFileItemsSourceField::~MFileItemsSourceField()
{
	this->!MFileItemsSourceField();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MFileItemsSourceField::!MFileItemsSourceField()
{
}

//----------------------------------------------------------------------------
System::String^ MFileItemsSourceField::Name::get()
{
	return name;
}

//----------------------------------------------------------------------------
void MFileItemsSourceField::Name::set(System::String^ value)
{
	name = value;
}

//----------------------------------------------------------------------------
IDataBinding^ MFileItemsSourceField::DataBinding::get()
{
	return dataBinding;
}

//----------------------------------------------------------------------------
void MFileItemsSourceField::DataBinding::set(IDataBinding^ value)
{
	dataBinding = value;

	MFileItemsSource^ fileSource = (MFileItemsSource^)ParentComponent;
	::DataObj* pDataObj = dataBinding == nullptr ? NULL : ((MDataObj^)dataBinding->Data)->GetDataObj();
	if (pDataObj && fileSource->GetXmlCombo())
	{
		// riallineo il dataType se non l'ho ancora fatto
		if (CompatibleDataType == nullptr)
			CompatibleDataType = ((MDataObj^)dataBinding->Data)->DataType;

		if (!HasCodeBehind)
			fileSource->GetXmlCombo()->Attach(pDataObj, CString(Name));
	}
}

//----------------------------------------------------------------------------
IDataType^ MFileItemsSourceField::CompatibleDataType::get()
{
	return dataType;
}

//----------------------------------------------------------------------------
void MFileItemsSourceField::CompatibleDataType::set(IDataType^ dataType)
{
	this->dataType = dataType;
}

//----------------------------------------------------------------------------
Type^ MFileItemsSourceField::ExcludedBindParentType::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
void MFileItemsSourceField::AdjustSite()
{
	if (ParentComponent->Site && Site == nullptr)
	{
		Site = ((ITBSite^)ParentComponent->Site)->CloneChild(this, Name);
		Site->Name = Name;
	}
}

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MFileItemsSourceFields::MFileItemsSourceFields(EasyBuilderComponent^ parent)
	:
	EasyBuilderComponents(parent)
{
}

//----------------------------------------------------------------------------
void MFileItemsSourceFields::InitializeForUI()
{
	if (!Parent || this->Count > 0)
		return;

	CXmlCombo* pComboBox = ((MFileItemsSource^)Parent)->GetXmlCombo();
	if (!pComboBox || !pComboBox->GetDataFileInfo())
		return;

	CDataFileInfo* pDataFile = pComboBox->GetDataFileInfo();

	// inizializzo i campi da bindare
	for (int i = 0; i <= pDataFile->m_arElementTypes.GetUpperBound(); i++)
	{
		CDataFileElementFieldType* pFieldType = (CDataFileElementFieldType*)pDataFile->m_arElementTypes.GetAt(i);
		if (pFieldType->m_bKey)
			continue;

		MFileItemsSourceField^ field = gcnew MFileItemsSourceField(Parent, gcnew System::String(pFieldType->m_sName), Parent->HasCodeBehind);
		field->CompatibleDataType = gcnew Microarea::TaskBuilderNet::Core::CoreTypes::DataType(pFieldType->m_Type.m_wType, pFieldType->m_Type.m_wTag);

		// viene inizializzato il databinding se esiste
		::DataObj* pDataObj = pComboBox->GetAttachedDataByName(pFieldType->m_sName);

		if (field->HasCodeBehind && pDataObj)
		{
			MFileItemsSource^ fileSource = (MFileItemsSource^)Parent;
			MParsedControl^ control = (MParsedControl^)fileSource->ParentComponent;
			if (control != nullptr && control->DataBinding != nullptr)
			{
				IDataManager^ dataManager = control->DataBinding->Parent;
				if (dataManager->Record != nullptr)
				{
					MSqlRecord^ rec = (MSqlRecord^)dataManager->Record;
					System::String^ colName = gcnew System::String(rec->GetSqlRecord()->GetColumnName(pDataObj));
					IRecordField^ recField = rec->GetField(colName);
					MDataObj^ mDataObj = recField == nullptr ? nullptr : (MDataObj^)recField->DataObj;

					if (mDataObj == nullptr)
						mDataObj = MDataObj::Create(pDataObj);

					field->DataBinding = gcnew FieldDataBinding(mDataObj, dataManager);
					((EasyBuilderComponent^)dataManager)->AddReferencedBy(field->SerializedName);
				}
			}
		}

		Add(field);
	}
}

//----------------------------------------------------------------------------
void MFileItemsSourceFields::AdjustSites()
{
	for each (MFileItemsSourceField^ field in this)
		field->AdjustSite();
}

//----------------------------------------------------------------------------
bool MFileItemsSourceFields::HasChanged()
{
	for each (EasyBuilderComponent^ ebComponent in this)
		if (ebComponent->ChangedPropertiesCount > 0)
			return true;

	return false;
}

/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MFileItemsSource::MFileItemsSource(MParsedCombo^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	EasyBuilderComponentExtender(owner, parentComponent, name)
{
	components = gcnew MFileItemsSourceFields(this);
	components->CanBeExtendedByUI = false;
	components->CanBeReducedByUI = false;
	components->CanBeModifiedByUI = true;
	components->AdjustSites();

	PropertiesManager->OrderBy = EasyBuilderPropertiesOrderBy::AlphabeticalOrder;
}

//----------------------------------------------------------------------------
MFileItemsSource::~MFileItemsSource()
{
	this->!MFileItemsSource();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MFileItemsSource::!MFileItemsSource()
{
}

//----------------------------------------------------------------------------
System::String^ MFileItemsSource::FileNamespace::get()
{
	CXmlCombo* pComboBox = GetXmlCombo();
	if (!pComboBox)
		return System::String::Empty;

	CDataFileInfo* pDataFile = pComboBox->GetDataFileInfo();

	return pDataFile ? gcnew System::String(pDataFile->m_Namespace.ToString()) : System::String::Empty;
}

//----------------------------------------------------------------------------
void MFileItemsSource::FileNamespace::set(System::String^ nameSpace)
{
	CXmlCombo* pComboBox = GetXmlCombo();
	if (!pComboBox)
		return;

	// prima ripulisco lo stato precedente 
	OtherBindings->Clear();
	CDataFileInfo* pInfo = pComboBox->GetDataFileInfo();
	if (pInfo)
		pComboBox->DetachDataObjs();

	// quindi setto il nuovo file
	pComboBox->UseProductLanguage(useCountry);
	pComboBox->SetNameSpace(CString(nameSpace));

	NotifyParentChanged();
}

//----------------------------------------------------------------------------
bool MFileItemsSource::UseCountry::get()
{
	if (!GetXmlCombo())
		return false;

	CDataFileInfo* pDataFile = GetXmlCombo()->GetDataFileInfo();
	if (pDataFile)
		useCountry = pDataFile->m_bUseProductLanguage == TRUE;

	return useCountry;
}

//----------------------------------------------------------------------------
void MFileItemsSource::UseCountry::set(bool value)
{
	if (!GetXmlCombo())
		return;

	CDataFileInfo* pDataFile = GetXmlCombo()->GetDataFileInfo();
	if (pDataFile)
		pDataFile->m_bUseProductLanguage = value;
	else
		// appoggio in una variabile locale
		useCountry = value;

	NotifyParentChanged();
}

//-----------------------------------------------------------------------------
void MFileItemsSource::Add(IComponent^ component)
{
	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void MFileItemsSource::Add(IComponent^ component, System::String^ name)
{
	if (name != nullptr && component->Site != nullptr)
		component->Site->Name = name;

	components->Add((EasyBuilderComponent^)component);
	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentAdded(this, component);
}

//-----------------------------------------------------------------------------
void MFileItemsSource::Remove(IComponent^ component)
{
	// Rimuove il component attuale dalla lista dei components del container
	components->Remove((EasyBuilderComponent^)component);

	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentRemoved(this, component);
}

//----------------------------------------------------------------------------
System::String^ MFileItemsSource::ToString()
{
	return FileNamespace;
}

//----------------------------------------------------------------------------
bool MFileItemsSource::CanExtendObject(IEasyBuilderComponentExtendable^ e)
{
	Object^ theObject = e->GetType() == MBodyEditColumn::typeid ? ((MBodyEditColumn^)e)->Control : e;

	if (theObject && theObject->GetType() == MParsedCombo::typeid)
		return ((MParsedCombo^)theObject)->GetWnd() && ((MParsedCombo^)theObject)->GetWnd()->IsKindOf(RUNTIME_CLASS(CXmlCombo));

	return false;
}

//----------------------------------------------------------------------------
void MFileItemsSource::AdjustSite()
{
	__super::AdjustSite();
	if (components != nullptr)
		components->AdjustSites();
}

//----------------------------------------------------------------------------
bool MFileItemsSource::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "OtherBindings")
		return FileNamespace != nullptr;

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
CXmlCombo* MFileItemsSource::GetXmlCombo()
{
	return ((MParsedCombo^)ExtendedObject) == nullptr ? NULL : (CXmlCombo*)((MParsedCombo^)ExtendedObject)->GetWnd();
}
