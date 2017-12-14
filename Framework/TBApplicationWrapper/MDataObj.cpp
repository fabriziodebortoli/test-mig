#include "StdAfx.h"

#include "float.h"
#include <TbGeneric/DataObj.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGes/eventmng.h>
#include <TbGes/extdoc.h>

#include "MDataObj.h"

using namespace System;
using namespace System::Reflection;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::ComponentModel;

//=============================================================================
//			Class MDataObjEvents
//=============================================================================


//-----------------------------------------------------------------------------
MDataObjEvents::~MDataObjEvents()
{
}

//-----------------------------------------------------------------------------
void MDataObjEvents::Signal(CObservable* pSender, EventType eType)
{
	if (DataObj->IsDisposed)
		return;
	
	switch (eType)
	{
	case ON_CHANGING:
		DataObj->DoValueChanging();
		break;
	case ON_CHANGED:
		DataObj->DoValueChanged();
		if (Document && Document->AutoValueChanged)
			DataObj->FireValueChanged();
		break;
	}
}

//-----------------------------------------------------------------------------
void MDataObjEvents::Fire(CObservable* pSender, EventType eType)
{
	if (!DataObj->IsDisposed && eType == ON_CHANGED && Document && Document->InUnattendedMode && DataObj->ParentComponent)
		FireUnattendedValueChanged();
}

//----------------------------------------------------------------------------
void MDataObjEvents::FireUnattendedValueChanged ()
{
	IDocumentDataManager^ doc = Document;
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*) (long) ((EasyBuilderComponent^) doc)->TbHandle;
	if (!pDoc || !pDoc->m_pEventManager || !pDoc->m_pEventManager->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
		return;

	if (!IRecord::typeid->IsInstanceOfType(DataObj->ParentComponent))
		return;

	IRecordField^ field = ((IRecord^) DataObj->ParentComponent)->GetField(DataObj);
	if (field == nullptr)
		return;

	
	CXMLEventManager* pEventMng = (CXMLEventManager*) pDoc->m_pEventManager;
	CFieldEvent* pFieldEvent = pEventMng->GetFieldEvents(DataObj->ParentComponent->Name, field->Name);
	
	if (pFieldEvent)
		for (int f=0; f <= pFieldEvent->m_arFunctions.GetUpperBound(); f++)
			pEventMng->FireAction(((CFieldFunction*) pFieldEvent->m_arFunctions.GetAt(f))->m_strFunction, DataObj->GetDataObj());
}

//-----------------------------------------------------------------------------
CObserverContext* MDataObjEvents::GetContext() const
{
	//if (m_pControl->GetDocument() && m_pControl->GetDocument()->m_ObserverContext.IsObserving(ON_CHANGED)) 
	//	return &m_pControl->GetDocument()->m_ObserverContext;
	return NULL;
}

//-----------------------------------------------------------------------------
MDataObjEvents* MDataObjEvents::Clone()
{
	MDataObjEvents* pEvents = new MDataObjEvents(this->DataObj);
	pEvents->SetParent (this->GetParent());
	pEvents->m_bOwned = this->m_bOwned;

	return pEvents;
}

//-----------------------------------------------------------------------------
MDataObj^ MDataObjEvents::GetDataObj ()
{
	return this->DataObj;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MDataObj Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataObj::MDataObj (System::IntPtr pDataObjPtr)
{
	m_pDataObj = (::DataObj*) pDataObjPtr.ToInt64();
	HasCodeBehind = true;
	if (m_pDataObj)
		m_pDataObj->AttachEvents(new MDataObjEvents(this));
}

//-----------------------------------------------------------------------------
MDataObj::~MDataObj()
{
	this->!MDataObj();
}

//-----------------------------------------------------------------------------
MDataObj::!MDataObj()
{	
	if (!HasCodeBehind && m_pDataObj)
	{
		delete m_pDataObj;
		m_pDataObj = NULL;
	}

	OnDisposed();
}

//-----------------------------------------------------------------------------
void MDataObj::OnPropertyChanging(System::String^ propertyName)
{
	PropertyChanging(this, gcnew PropertyChangingEventArgs(propertyName));
}

//-----------------------------------------------------------------------------
void MDataObj::OnPropertyChanged(System::String^ propertyName)
{
	PropertyChanged(this, gcnew PropertyChangedEventArgs(propertyName));
}

//-----------------------------------------------------------------------------
bool MDataObj::Equals(Object^ obj)
{
	MDataObj^ mdataObj = dynamic_cast<MDataObj^>(obj);

	return (mdataObj != nullptr && m_pDataObj == mdataObj->m_pDataObj);
}

//-----------------------------------------------------------------------------
int MDataObj::GetHashCode()
{
	return (int) DataObjPtr;
}

//-----------------------------------------------------------------------------
void MDataObj::DoValueChanging()
{
	OnValueChanging();
}

//-----------------------------------------------------------------------------
void MDataObj::DoValueChanged()
{
	OnValueChanged();
}

//-----------------------------------------------------------------------------
System::Object^ MDataObj::Clone()
{
	MDataObj^ dataObj = MDataObj::Create(DataType); 
	dataObj->Value = this->Value;

	return dataObj;
}

//-----------------------------------------------------------------------------
void MDataObj::ReplaceDataObj(System::IntPtr pDataObj, bool deletePrev)
{
	if (HasCodeBehind)
	{
		ASSERT(FALSE);
		return;
	}
	
	if (m_pDataObj && deletePrev) delete m_pDataObj;

	m_pDataObj = (::DataObj*) pDataObj.ToInt64();
}

//-----------------------------------------------------------------------------
MDataObj^ MDataObj::Create (IDataType^ dataType)
{
	::DataType aDataType(dataType->Type, dataType->Tag);
	DataObj* pData = DataObj::DataObjCreate(aDataType);
	if (!pData)
		return nullptr;
	MDataObj^ ret = Create(pData);
	ret->HasCodeBehind = false;//per cancellare il dataobj
	return ret;
}

//-----------------------------------------------------------------------------
MDataObj^ MDataObj::Create (IntPtr dataObjPtr)
{
	::DataObj* pDataObj = (::DataObj*) dataObjPtr.ToInt64();
	return Create(pDataObj);
}

//-----------------------------------------------------------------------------
MDataObj^ MDataObj::Create (DataObj* pDataObj)
{
	if (!pDataObj)
		return nullptr;

	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataInt))
	{
		return gcnew MDataInt((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataLng))
	{
		return gcnew MDataLng((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataDbl))
	{
		return gcnew MDataDbl((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataBool))
	{
		return gcnew MDataBool((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataEnum))
	{
		return gcnew MDataEnum((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataPerc))
	{
		return gcnew MDataPerc((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataQty))
	{
		return gcnew MDataQty((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataMon))
	{
		return gcnew MDataMon((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataStr))
	{
		return gcnew MDataStr((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataGuid))
	{
		return gcnew MDataGuid((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataDate))
	{
		return gcnew MDataDate((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataText))
	{
		return gcnew MDataText((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataBlob))
	{
		return gcnew MDataBlob((System::IntPtr)pDataObj);
	}
	if (pDataObj->GetRuntimeClass() == RUNTIME_CLASS(DataArray))
	{
		return gcnew MDataArray((System::IntPtr)pDataObj);
	}
	
	ASSERT(FALSE);
	return nullptr; 
}

//-----------------------------------------------------------------------------
 void MDataObj::OnValueChanged()
{
	this->OnPropertyChanged("Value");
	this->ValueChanged(this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
 void MDataObj::OnValueChanging()
{
	this->OnPropertyChanging("Value");
	this->ValueChanging(this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
 void MDataObj::OnReadOnlyChanged()
{
	this->ReadOnlyChanged(this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
void MDataObj::OnVisibleChanged()
{
	this->VisibleChanged(this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
Object^ MDataObj::Value::get () 
{ 
	return GetValue();
}

//-----------------------------------------------------------------------------
void MDataObj::Value::set (Object^ value)
{
	SetValue(value);
}

//-----------------------------------------------------------------------------
void MDataObj::SetValueFromString (System::String^ value)
{
	m_pDataObj->Assign(CString(value));
}

//-----------------------------------------------------------------------------
System::Object^ MDataObj::LowerValue::get ()
{
	return nullptr;
}

//-----------------------------------------------------------------------------
System::Object^ MDataObj::UpperValue::get ()
{
	return nullptr;
}

//-----------------------------------------------------------------------------
bool MDataObj::Modified::get () 
{ 
	return m_pDataObj != nullptr && m_pDataObj->IsModified();
}

//-----------------------------------------------------------------------------
void MDataObj::Modified::set (bool value)
{
	if (m_pDataObj != nullptr)
		m_pDataObj->SetModified(value);
}

//-----------------------------------------------------------------------------
bool MDataObj::ReadOnly::get ()
{ 
	return m_pDataObj ? m_pDataObj->IsReadOnly() == TRUE : true;
}

//-----------------------------------------------------------------------------
void MDataObj::ReadOnly::set (bool value)
{ 
	if (m_pDataObj)
	{
		m_pDataObj->SetReadOnly(value == true);
		PropertiesManager->AddReadOnlyToProperty(ValuePropertyName, value);
		OnReadOnlyChanged();
	}
}

//-----------------------------------------------------------------------------
bool MDataObj::Visible::get ()
{ 
	return m_pDataObj ? !m_pDataObj->IsHide() == TRUE : true;
}

//-----------------------------------------------------------------------------
void MDataObj::Visible::set (bool value)
{ 
	if (m_pDataObj)
	{
		OnPropertyChanging("Visible");
		m_pDataObj->SetHide(value != true);
		OnPropertyChanged("Visible");
		OnVisibleChanged();
	}
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::StringValue::get () 
{ 
	return gcnew System::String(m_pDataObj->FormatDataForXML());
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::StringNoSoapValue::get () 
{ 
	return gcnew System::String(m_pDataObj->FormatDataForXML(FALSE));
}


//-----------------------------------------------------------------------------
BOOL IsValidOldCtrlData()
{
	if (!AfxGetApp())//se non ho una app, provo a mettere a posto le cose con la AFX_MANAGE_STATE
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		return AfxGetApp() ? AfxGetBaseApp()->IsValidOldCtrlData() : FALSE;
	}
	else
	{
		return AfxGetBaseApp()->IsValidOldCtrlData();
	}
}

//-----------------------------------------------------------------------------
void MDataObj::FireValueChanging()
{
	if (IsValidOldCtrlData())//caso in cui sono già impegnato in un change
	{
		FireEvent^ evt = gcnew FireEvent(this, &MDataObj::FireValueChanging);
		System::Windows::Threading::Dispatcher::CurrentDispatcher->BeginInvoke(evt);
		return;
	}
	if (m_pDataObj)
	{
		m_pDataObj->Fire(ON_CHANGING);
	}
}

//-----------------------------------------------------------------------------
void MDataObj::FireValueChanged()
{
	if (IsValidOldCtrlData())//caso in cui sono già impegnato in un change
	{
		FireEvent^ evt = gcnew FireEvent(this, &MDataObj::FireValueChanged);
		System::Windows::Threading::Dispatcher::CurrentDispatcher->BeginInvoke(evt);
		return;
	}
	if (m_pDataObj)
	{
		m_pDataObj->Fire(ON_CHANGED);
	}
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::Name::get () 
{ 
	if (this->ParentComponent != nullptr)
	{
		IRecord^ record = dynamic_cast<IRecord^>(this->ParentComponent);
		if (record != nullptr)
		{
			IRecordField^ field = record->GetField(this);
			if (field != nullptr)
				return field->Name;
		}
	}

	return __super::Name;
}

//-----------------------------------------------------------------------------
void MDataObj::StringValue::set (System::String^ value)
{
	m_pDataObj->AssignFromXMLString(CString(value));
}

//-----------------------------------------------------------------------------
void MDataObj::StringNoSoapValue::set (String^ value)
{
	m_pDataObj->AssignFromXMLString(CString(value));
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::ToString ()
{
	if (!m_pDataObj)
		return System::String::Empty;

	return gcnew System::String(m_pDataObj->Str());
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::ToString(System::String^ format, IFormatProvider^ formatProvider)
{
/*    if (formatProvider != nullptr)
    {
        ICustomFormatter^ fmt = dynamic_cast<ICustomFormatter^>(formatProvider->GetFormat(this->GetType()));

        if (fmt != nullptr)
		{
			return fmt->Format(format, this, formatProvider);
		}
    }*/


	return ToString();
}

//-----------------------------------------------------------------------------
System::String^ MDataObj::FormatData()
{
	if (!m_pDataObj)
		return System::String::Empty;

	return gcnew System::String(m_pDataObj->FormatData());
}

//-----------------------------------------------------------------------------
IDataType^ MDataObj::DataTypeIDataObj::get()
{
	return DataType;
}
//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Core::CoreTypes::DataType MDataObj::DataType::get()
{
	if (!m_pDataObj)
		return Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Null;

	return Microarea::TaskBuilderNet::Core::CoreTypes::DataType(
			m_pDataObj->GetDataType().m_wType,
			m_pDataObj->GetDataType().m_wTag
			);

}

//-----------------------------------------------------------------------------
bool MDataObj::Empty::get()
{
	return (m_pDataObj) ? (m_pDataObj->IsEmpty()==TRUE) : false;
}

//-----------------------------------------------------------------------------
bool MDataObj::BelongsToPrototypeRecord::get()
{
	EasyBuilderComponent^ rec = dynamic_cast<EasyBuilderComponent^>(ParentComponent);
	if (rec == nullptr)
		return false;

	IDocumentSlaveBufferedDataManager^ parentDbt =  dynamic_cast<IDocumentSlaveBufferedDataManager^>(rec->ParentComponent);
	
	return parentDbt != nullptr && parentDbt->Record == dynamic_cast<IRecord^>(rec);
}


//-----------------------------------------------------------------------------
bool MDataObj::ShowOnlyValueInPropertyGrid::get ()
{
	return showOnlyValueInPropertyGrid;
}

//-----------------------------------------------------------------------------
void MDataObj::ShowOnlyValueInPropertyGrid::set (bool value)
{
	showOnlyValueInPropertyGrid = value;
	for each (PropertyInfo^ propInfo in GetType()->GetProperties())
	{
		if (propInfo->Name == ShowOnlyValueInPropertyGridName) 
			continue;

		if (propInfo->Name == ValuePropertyName) 
			PropertiesManager->AddReadOnlyToProperty (propInfo->Name, !value);
		else
			PropertiesManager->RemoveBrowsableToProperty (propInfo->Name, !value);
	}
}

//-----------------------------------------------------------------------------
void MDataObj::ParentComponent::set(EasyBuilderComponent^ value) 
{
	__super::ParentComponent = value;
	
	if (m_pDataObj)
	{
		CDataEventsObj *pEvents = (CDataEventsObj *)m_pDataObj->GetDataEvents();

		while (pEvents) 
		{
			MDataObjEvents* pDataEvents = dynamic_cast<MDataObjEvents*>(pEvents);
			if (pDataEvents)
				pDataEvents->Document = ParentComponent == nullptr ? nullptr : ParentComponent->Document;
			pEvents = pEvents->GetParent();
		}
	}
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataInt Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataInt::MDataInt (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataInt::MDataInt ()
	:
	MDataObj((System::IntPtr) new DataInt())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
MDataInt::MDataInt (System::Int16 value)
	:
	MDataObj((System::IntPtr) new DataInt())
{
	HasCodeBehind = false;
	Value = value;
}

//-----------------------------------------------------------------------------
System::Object^ MDataInt::LowerValue::get ()
{
	return (System::Int16)DATA_INT_MINVALUE;
}

//-----------------------------------------------------------------------------
System::Object^ MDataInt::UpperValue::get ()
{
	return (System::Int16)DATA_INT_MAXVALUE;
}

//-----------------------------------------------------------------------------
Int16 MDataInt::Value::get ()
{
	return Int16(*((::DataInt*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataInt::Value::set (Int16 value)
{
	((::DataInt*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataInt::SetValue (Object^ value)
{
	Value = Convert::ToInt16(value);
}

//-----------------------------------------------------------------------------
Object^ MDataInt::GetValue()
{
	return Value;
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataLng Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataLng::MDataLng (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataLng::MDataLng ()
	:
	MDataObj((System::IntPtr) new DataLng())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
MDataLng::MDataLng (System::Int32 value)
	:
	MDataObj((System::IntPtr) new DataLng())
{
	HasCodeBehind = false;
	Value = value;
}

//-----------------------------------------------------------------------------
Int32 MDataLng::Value::get ()
{
	return Int32(*((::DataLng*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataLng::Value::set (Int32 value)
{
	((::DataLng*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataLng::SetValue (Object^ value)
{
	Value = Convert::ToInt32(value);
}

//-----------------------------------------------------------------------------
Object^ MDataLng::GetValue()
{
	return Value;
}

//-----------------------------------------------------------------------------
System::Object^ MDataLng::LowerValue::get ()
{
	return DATA_LNG_MINVALUE;
}

//-----------------------------------------------------------------------------
System::Object^ MDataLng::UpperValue::get ()
{
	return DATA_LNG_MAXVALUE;
}


//-----------------------------------------------------------------------------
System::String^ MDataLng::ScriptingFormat::get ()
{
	DataLng* pLng = (::DataLng*) m_pDataObj;

	return pLng->GetDataType().IsATime()
		? System::String::Concat("{et'", gcnew System::String(pLng->Str()), "'}")
		: ((long)*pLng).ToString();
}
//////////////////////////////////////////////////////////////////////////////
// 				class MDataDbl Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataDbl::MDataDbl (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataDbl::MDataDbl ()
	:
	MDataObj((System::IntPtr) new DataDbl())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Double MDataDbl::Value::get ()
{
	return Double(*((::DataDbl*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataDbl::Value::set (Double value)
{
	((::DataDbl*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataDbl::SetValue (Object^ value)
{
	Value = Convert::ToDouble(value);
}

//-----------------------------------------------------------------------------
Object^ MDataDbl::GetValue()
{
	return Value;
}

//-----------------------------------------------------------------------------
System::Object^ MDataDbl::LowerValue::get ()
{
	return DATA_DBL_MINVALUE;
}

//-----------------------------------------------------------------------------
System::Object^ MDataDbl::UpperValue::get ()
{
	return DATA_DBL_MAXVALUE;
}


//////////////////////////////////////////////////////////////////////////////
// 				class MDataBool Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataBool::MDataBool (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{
}

//-----------------------------------------------------------------------------
MDataBool::MDataBool ()
	:
	MDataObj((System::IntPtr) new DataBool())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Boolean MDataBool::Value::get ()
{
	return (*((::DataBool*) m_pDataObj)) ? true : false;
}

//-----------------------------------------------------------------------------
void MDataBool::Value::set (Boolean value)
{
	((::DataBool*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataBool::SetValue (Object^ value)
{
	Value = Convert::ToBoolean(value);
}

//-----------------------------------------------------------------------------
Object^ MDataBool::GetValue()
{
	return Value;
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataEnum Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataEnum::MDataEnum (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataEnum::MDataEnum (Int32 value)
	:
	MDataObj((System::IntPtr) new DataEnum(value))
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Int32 MDataEnum::Value::get ()
{
	return Int32(((::DataEnum*) m_pDataObj)->GetValue());
}

//-----------------------------------------------------------------------------
void MDataEnum::Value::set (Int32 value)
{
	((::DataEnum*) m_pDataObj)->Assign((DWORD)value);
}

//-----------------------------------------------------------------------------
void MDataEnum::SetValue (Object^ value)
{
	Value = Convert::ToUInt32(value);
}

//-----------------------------------------------------------------------------
Object^ MDataEnum::GetValue()
{
	return Value;
}

//-----------------------------------------------------------------------------
System::String^ MDataEnum::ScriptingFormat::get ()
{
	DataEnum* pEnum = (::DataEnum*) m_pDataObj;

	return System::String::Concat("{", pEnum->GetTagValue(), ":", pEnum->GetItemValue(), "}");
}

//-----------------------------------------------------------------------------
void MDataEnum::SetValueFromString (System::String^ value)
{
	DataEnum* pEnum = (::DataEnum*) m_pDataObj;
	const EnumItemArray* pItems = AfxGetEnumsTable()->GetEnumItems(pEnum->GetBirthTagValue());
	for (int i=0; i <= pItems->GetUpperBound(); i++)
	{
		EnumItem* pItem = pItems->GetAt(i);
		if (pItem->GetTitle().CompareNoCase(CString(value)) == 0)
		{
			pEnum->Assign(pEnum->GetBirthTagValue(), pItem->GetItemValue());
			break;
		}
	}

}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataPerc Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataPerc::MDataPerc (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataPerc::MDataPerc ()
	:
	MDataObj((System::IntPtr) new DataPerc())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Double MDataPerc::Value::get ()
{
	return Double(*((::DataPerc*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataPerc::Value::set (Double value)
{
	((::DataPerc*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataPerc::SetValue (Object^ value)
{
	Value = Convert::ToDouble(value);
}

//-----------------------------------------------------------------------------
Object^ MDataPerc::GetValue()
{
	return Value;
}


//////////////////////////////////////////////////////////////////////////////
// 				class MDataQty Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataQty::MDataQty (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataQty::MDataQty ()
	:
	MDataObj((System::IntPtr) new DataQta())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Double MDataQty::Value::get ()
{
	return Double(*((::DataQty*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataQty::Value::set (Double value)
{
	((::DataQty*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataQty::SetValue (Object^ value)
{
	Value = Convert::ToDouble(value);
}

//-----------------------------------------------------------------------------
Object^ MDataQty::GetValue()
{
	return Value;
}


//////////////////////////////////////////////////////////////////////////////
// 				class MDataMon Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataMon::MDataMon (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataMon::MDataMon ()
	:
	MDataObj((System::IntPtr) new DataMon())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
MDataMon::MDataMon (System::Double value)
	:
	MDataObj((System::IntPtr) new DataMon())
{
	HasCodeBehind = false;
	Value = value;
}

	//-----------------------------------------------------------------------------
Double MDataMon::Value::get ()
{
	return Double(*((::DataMon*) m_pDataObj));
}

//-----------------------------------------------------------------------------
void MDataMon::Value::set (Double value)
{
	((::DataMon*) m_pDataObj)->Assign(value);
}

//-----------------------------------------------------------------------------
void MDataMon::SetValue (Object^ value)
{
	Value = Convert::ToDouble(value);
}

//-----------------------------------------------------------------------------
Object^ MDataMon::GetValue()
{
	return Value;
}


//////////////////////////////////////////////////////////////////////////////
// 				class MDataStr Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataStr::MDataStr (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataStr::MDataStr ()
	:
	MDataObj((System::IntPtr) new DataStr())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
MDataStr::MDataStr (System::String^ value)
	:
	MDataObj((System::IntPtr) new DataStr())
{
	HasCodeBehind = false;
	Value = value;
}

//-----------------------------------------------------------------------------
System::String^ MDataStr::Value::get ()
{
	return gcnew System::String(((::DataStr*) m_pDataObj)->GetString());
}

//-----------------------------------------------------------------------------
void MDataStr::Value::set (System::String^ value)
{

	((::DataStr*) m_pDataObj)->Assign(CString(value));
}

//-----------------------------------------------------------------------------
void MDataStr::SetValue (Object^ value)
{
	Value = Convert::ToString(value);
}

//-----------------------------------------------------------------------------
Object^ MDataStr::GetValue()
{
	return Value;
}

//-----------------------------------------------------------------------------
int MDataStr::MaxLength::get()
{
	//usare GetColumnLen invece di GetAllocSize perché quest'ultima restituisce la dimensione in byte e non in caratteri!
	return m_pDataObj->GetColumnLen();
}
//-----------------------------------------------------------------------------
void MDataStr::MaxLength::set(int value)
{
	m_pDataObj->SetAllocSize(value);
}

//-----------------------------------------------------------------------------
System::String^ MDataStr::FormatterName::get()
{
	return System::String::Empty;
}

//-----------------------------------------------------------------------------
void MDataStr::FormatterName::set(System::String^ value)
{
	
}

//-----------------------------------------------------------------------------
bool MDataStr::UpperCase::get()
{
	return ((::DataStr*) m_pDataObj)->IsUpperCase() == TRUE;
}
//-----------------------------------------------------------------------------
void MDataStr::UpperCase::set(bool value)
{
	((::DataStr*) m_pDataObj)->SetUpperCase(value);
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataGuid Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataGuid::MDataGuid (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataGuid::MDataGuid ()
	:
	MDataObj((System::IntPtr) new DataGuid())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
Guid MDataGuid::Value::get ()
{
	return Guid(gcnew System::String(((::DataGuid*) m_pDataObj)->Str()));
}

//-----------------------------------------------------------------------------
void MDataGuid::Value::set (Guid value)
{
	((::DataGuid*) m_pDataObj)->Assign(CString(value.ToString()));
}

//-----------------------------------------------------------------------------
void MDataGuid::SetValue (Object^ value)
{
	Value = Convert::ToString(value);
}

//-----------------------------------------------------------------------------
Object^ MDataGuid::GetValue()
{
	return Value;
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataDate Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataDate::MDataDate (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataDate::MDataDate ()
	:
	MDataObj((System::IntPtr) new DataDate())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
DateTime MDataDate::Value::get ()
{
	DataDate* pDate = (::DataDate*) m_pDataObj;
	return DateTime(pDate->Year(), pDate->Month(), pDate->Day(), pDate->Hour(), pDate->Minute(), pDate->Second());
}

//-----------------------------------------------------------------------------
void MDataDate::Value::set (DateTime value)
{
	((::DataDate*) m_pDataObj)->Assign(DataDate(value.Day, value.Month, value.Year, value.Hour, value.Minute, value.Second));
}

//-----------------------------------------------------------------------------
void MDataDate::SetValue (Object^ value)
{
	Value = Convert::ToDateTime(value);
}

//-----------------------------------------------------------------------------
Object^ MDataDate::GetValue()
{
	return Value;
}
//-----------------------------------------------------------------------------
System::String^ MDataDate::ScriptingFormat::get ()
{
	DataDate* pDate = (::DataDate*) m_pDataObj;
	if (pDate->IsFullDate())
	{
		if (pDate->IsATime())
			return System::String::Concat("{t'", gcnew System::String(pDate->Str(-1, 0)), "'}");
		else
			return System::String::Concat("{dt'", gcnew System::String(pDate->Str(-1, 0)), "'}");
	}
	return System::String::Concat("{d'", gcnew System::String(pDate->Str(-1, 0)), "'}");
}

//-----------------------------------------------------------------------------
System::DateTime  MDataDate::NullDate::get ()
{
	return System::DateTime(MIN_YEAR, MIN_MONTH, MIN_DAY, 0, 0, 0);
}

//Vedere TBGeneric\DateFuncionts.h
//#define	MIN_GIULIAN_DATE	1L			// corrispondente a 01/01/1800
//-----------------------------------------------------------------------------
System::DateTime  MDataDate::MinDate::get ()
{
	return System::DateTime(1800, 1, 1, 0, 0, 0);
}

//-----------------------------------------------------------------------------
System::DateTime  MDataDate::MaxDate::get ()
{
	return System::DateTime(MAX_YEAR, MAX_MONTH, MAX_DAY, 0, 0, 0);
}	


//////////////////////////////////////////////////////////////////////////////
// 				class MDataText Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataText::MDataText (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataText::MDataText ()
	:
	MDataObj((System::IntPtr) new DataText())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
System::String^ MDataText::Value::get ()
{
	return gcnew System::String(((::DataText*) m_pDataObj)->Str());
}

//-----------------------------------------------------------------------------
void MDataText::Value::set (System::String^ value)
{
	((::DataText*) m_pDataObj)->Assign(CString(value));
}

//-----------------------------------------------------------------------------
void MDataText::SetValue (Object^ value)
{
	Value = Convert::ToString(value);
}

//-----------------------------------------------------------------------------
Object^ MDataText::GetValue()
{
	return Value;
}

//////////////////////////////////////////////////////////////////////////////
// 				class MDataBlob Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataBlob::MDataBlob (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataBlob::MDataBlob()
	:
	MDataObj((System::IntPtr) new DataBlob())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
array<Byte>^ MDataBlob::Value::get ()
{
	DataBlob* pBlob = (::DataBlob*) m_pDataObj;
	BYTE* pBuffer = (BYTE*) pBlob->GetRawData();

	int count = pBlob->GetLen();
	array<Byte>^ bytes = gcnew array<Byte>(count);
	for (int i = 0; i < count; i++)
		bytes[i] = pBuffer[i];
	return bytes;
}

//-----------------------------------------------------------------------------
void MDataBlob::Value::set (array<Byte>^ value)
{
	BYTE* pBuffer = new BYTE[value->Length];
	for (int i = 0; i < value->Length; i++)
		pBuffer[i] = value[i];
	((::DataBlob*) m_pDataObj)->Assign(pBuffer, value->Length);
	delete pBuffer;
}

//-----------------------------------------------------------------------------
void MDataBlob::SetValue (Object^ value)
{
	Value = (array<Byte>^)value;
}

//-----------------------------------------------------------------------------
Object^ MDataBlob::GetValue()
{
	return Value;
}
//////////////////////////////////////////////////////////////////////////////
// 				class MDataArray Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataArray::MDataArray (System::IntPtr pDataObjPtr)
	: MDataObj(pDataObjPtr)
{}

//-----------------------------------------------------------------------------
MDataArray::MDataArray()
	:
	MDataObj((System::IntPtr) new DataArray())
{
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
array<MDataObj^>^ MDataArray::Value::get ()
{
	DataArray* pArray = (DataArray*) m_pDataObj;
	
	array<MDataObj^>^ ret = gcnew array<MDataObj^>(pArray->GetSize());
	for (int i = 0; i < pArray->GetSize(); i++)
		ret[i] = Create(&pArray[i]);
	return ret;
}

//-----------------------------------------------------------------------------
void MDataArray::Value::set (array<MDataObj^>^ value)
{
	DataArray* pArray = (DataArray*) m_pDataObj;
	pArray->Clear();

	for (int i = 0; i < value->Length; i++)
		pArray->Add(value[i]->GetDataObj());
}

//-----------------------------------------------------------------------------
void MDataArray::SetValue (Object^ value)
{
	Value = (array<MDataObj^>^)value;
}

//-----------------------------------------------------------------------------
Object^ MDataArray::GetValue()
{
	return Value;
}

