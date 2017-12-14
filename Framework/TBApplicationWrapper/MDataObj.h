#pragma once

#include "Attributes.h"
#include <TbGeneric\DataObj.h>
#include "EBEventArgs.h"

using namespace Microarea::TaskBuilderNet::Interfaces;

using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::ComponentModel::Design::Serialization;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper 
{
	/// <summary>
	/// Root Class for all dataobjs
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(EasyBuilderSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::TypeConverter(System::ComponentModel::TypeConverter::typeid)]
	public ref class MDataObj abstract : EasyBuilderComponent, IDataObj, System::ComponentModel::INotifyPropertyChanging, System::ComponentModel::INotifyPropertyChanged, System::IFormattable
	{
	private:
		delegate void  FireEvent();
		static System::String^ ValuePropertyName				= "Value";
		static System::String^ ShowOnlyValueInPropertyGridName	= "ShowOnlyValueInPropertyGrid";
		bool		   showOnlyValueInPropertyGrid;

	protected:
		::DataObj*		m_pDataObj;
		
	
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataObj (System::IntPtr pDataObjPtr);

		/// <summary>
		/// Destructor 
		/// </summary>
		~MDataObj ();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MDataObj ();

	internal:
		void DoValueChanging	();
		void DoValueChanged		();

	public:
		/// <summary>
		/// Event Raised after a change has occurred to the value of the dataobj
		/// </summary>
		virtual event System::EventHandler<EasyBuilderEventArgs^>^ ValueChanged;
		
		/// <summary>
		/// Event Raised before a change has occurred to the value of the dataobj
		/// </summary>
		virtual event System::EventHandler<EasyBuilderEventArgs^>^ ValueChanging;

		/// <summary>
		/// Event Raised after a change in readonly status
		/// </summary>
		virtual event System::EventHandler<EasyBuilderEventArgs^>^ ReadOnlyChanged;
		
		/// <summary>
		/// Event Raised after a change in visible status
		/// </summary>
		virtual event System::EventHandler<EasyBuilderEventArgs^>^ VisibleChanged;

	public:
		/// <summary>
		/// PropertyChanging event
		/// </summary>
		virtual event System::ComponentModel::PropertyChangingEventHandler^ PropertyChanging;
		/// <summary>
		/// PropertyChanged event
		/// </summary>
		virtual event System::ComponentModel::PropertyChangedEventHandler^ PropertyChanged;
		/// <summary>
		/// Get the type of the current data obj
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property Microarea::TaskBuilderNet::Core::CoreTypes::DataType	DataType	
		{
			virtual Microarea::TaskBuilderNet::Core::CoreTypes::DataType get (); 
		}
		/// <summary>
		/// Get the type of the current data obj
		/// </summary>
		property IDataType^	DataTypeIDataObj { virtual IDataType^ get () =  IDataObj::DataType::get; }

		/// <summary>
		/// Get the name of the current data obj
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Name { virtual System::String^ get () override; }

		/// <summary>
		/// Gets or sets the value of the current data obj (as an object)
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
		property System::Object^ Value { virtual System::Object^ get (); virtual void set (System::Object^ value); }

		/// <summary>
		/// Gets or sets if the current data obj is modified
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool Modified { virtual bool get (); virtual void set (bool value); }
		
		/// <summary>
		/// Gets or sets the value of the current data obj (as a string)
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ StringValue { System::String^	get ();	void set (System::String^ value); }
		

		/// <summary>
		/// Gets or sets the value of the current data obj (as a NOT SOAP XML String)
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ StringNoSoapValue { System::String^ get (); void set (System::String^ value); }

		/// <summary>
		/// true if the dataobj is empty
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool Empty { bool get(); }
		
		/// <summary>
		/// true if the dataobj belongs to prototype of a DBTSlaveBuffered
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool BelongsToPrototypeRecord { virtual bool get(); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::IntPtr DataObjPtr {System::IntPtr get() { return (System::IntPtr) m_pDataObj; }}
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property bool ShowOnlyValueInPropertyGrid { bool get (); void set (bool value); }

		/// <summary>
		/// Gets or sets the readonly property of the dataobj
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool ReadOnly { bool get(); void set(bool value); }

		/// <summary>
		/// Gets or sets the visible property of the dataobj
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool Visible { bool get(); void set(bool value); }


		[System::ComponentModel::Browsable(false)]
		property EasyBuilderComponent^ ParentComponent { virtual void set(EasyBuilderComponent^ value) override; }
	
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual System::Object^ Clone ();

		/// <summary>
		/// Internal and Forbidden Use
		/// </summary>
		[ExcludeFromIntellisense]
		void ReplaceDataObj(System::IntPtr pDataObj, bool deletePrev);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		static MDataObj^ Create (DataObj* pDataObj);

		/// <summary>
		/// Crete a dataobj of the specified type
		/// </summary>
		/// <param name="dataType">the type of the new dataobj</param>
		static MDataObj^ Create (IDataType^ dataType);

		/// <summary>
		/// Crete a wrap of the specified dataObj
		/// </summary>
		/// <param name="dataObjPtr">the dataobj of the new dataobj</param>
		static MDataObj^ Create (System::IntPtr dataObjPtr);

		/// <summary>
		/// Overrides the default equals implementation
		/// </summary>
		virtual bool Equals(Object^ obj) override;

		/// <summary>
		/// Overrides the default GetHashCode implementation
		/// </summary>
		virtual int GetHashCode() override;

		/// <summary>
		/// Set the Value of the dataObj
		/// </summary>
		virtual	void SetValueFromString (System::String^ value);


		/// <summary>
		/// Gets the minimun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ LowerValue { virtual System::Object^ get ();}

		/// <summary>
		/// Gets the maximun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ UpperValue { virtual System::Object^ get ();}

	internal:
		virtual void OnPropertyChanging(System::String^ propertyName);
		virtual void OnPropertyChanged(System::String^ propertyName);
	
	protected:
		/// <summary>
		/// abstract method, sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) = 0;

		/// <summary>
		/// abstract method, gets the Value of the dataObj
		/// </summary>
		virtual	System::Object^ GetValue () = 0;

		/// <summary>
		/// OnValueChanged
		/// </summary>
		virtual void OnValueChanged ();
		
		/// <summary>
		/// OnValueChanging
		/// </summary>
		virtual void OnValueChanging ();

		/// <summary>
		/// OnReadOnlyChanged
		/// </summary>
		virtual void OnReadOnlyChanged ();
		
		/// <summary>
		/// OnVisibleChanged
		/// </summary>
		virtual void OnVisibleChanged ();

	public:
		virtual void Clear() { m_pDataObj->Clear(); }
		/// <summary>
		/// Convert the value of the dataobh from object to string
		/// </summary>
		virtual System::String^ ToString() override;

		virtual System::String^ ToString(System::String^ format, System::IFormatProvider^ formatProvider);
		
		/// <summary>
		/// Fires changing event related to the dataobj
		/// </summary>
		void FireValueChanging	();

		/// <summary>
		/// Fires changed event related to the dataobj
		/// </summary>
		void FireValueChanged	();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		::DataObj* GetDataObj() { return m_pDataObj; }
		
		/// <summary>
		/// Format the dataobj value with the default formatter
		/// </summary>
		System::String^ FormatData(); 

		/// <summary>
		/// Format the dataobj value with the scripting formatter
		/// </summary>
		property System::String^ ScriptingFormat { virtual System::String^ get() { return this->ToString(); } }
	};

#undef new
	/// <summary>
	/// Class that wraps the c++ tasbuilder DataInt object
	/// </summary>
	//=========================================================================
	public ref class MDataInt : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataInt (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataInt ();

		/// <summary>
		/// Constructor
		/// </summary>
		MDataInt (System::Int16  value);

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void	SetValue	(System::Object^ value) override;

		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue	() override;

			/// <summary>
		/// Gets the minimun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ LowerValue { virtual System::Object^ get () override;}

		/// <summary>
		/// Gets the maximun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ UpperValue { virtual System::Object^ get () override;}

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Int16)
		/// </summary>
		property System::Int16 Value { virtual System::Int16 get() new; virtual void set(System::Int16 value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataLng object
	/// </summary>
	//=========================================================================
	public ref class MDataLng : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataLng (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataLng ();

		/// <summary>
		/// Constructor
		/// </summary>
		MDataLng (System::Int32 value);

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue		() override;

		/// <summary>
		/// Gets the minimun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ LowerValue { virtual System::Object^ get () override;}

		/// <summary>
		/// Gets the maximun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ UpperValue { virtual System::Object^ get () override;}

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Int32
		/// </summary>
		property System::Int32 Value { virtual System::Int32 get() new; virtual void set(System::Int32 value); }
		/// <summary>
		/// Gets the scripting format of the long object (significant only for elapsed time)
		/// </summary>
		property System::String^ ScriptingFormat { virtual System::String^ get() override; }

		void SetAsTime			(bool IsTime		){ m_pDataObj->SetAsTime(IsTime);}
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataDbl object
	/// </summary>
	//=========================================================================
	public ref class MDataDbl : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataDbl (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataDbl ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

		/// <summary>
		/// Gets the minimun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ LowerValue { virtual System::Object^ get () override;}

		/// <summary>
		/// Gets the maximun allowed value for the dataObj
		/// </summary>
		[System::ComponentModel::Browsable(true), TBBindable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Object^ UpperValue { virtual System::Object^ get () override;}

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a double)
		/// </summary>
		property System::Double Value { virtual System::Double get() new; virtual void set(System::Double value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataBool object
	/// </summary>
	//=========================================================================
	public ref class MDataBool : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataBool (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataBool ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Boolean)
		/// </summary>
		property System::Boolean Value { virtual System::Boolean get() new; virtual void set(System::Boolean value); }
		
		/// <summary>
		/// Convert the databool value to Boolean
		/// </summary>
		static operator System::Boolean ( MDataBool^ b ) { return b->Value;}

	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataEnum object
	/// </summary>
	//=========================================================================
	public ref class MDataEnum : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataEnum (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataEnum (System::Int32);
		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Int32)
		/// </summary>
		property System::Int32 Value { virtual System::Int32 get() new; virtual void set(System::Int32 value); }

		/// <summary>
		/// Gets the scripting format of the enum object
		/// </summary>
		property System::String^ ScriptingFormat { virtual System::String^ get() override; }

		virtual	void SetValueFromString (System::String^ value) override;
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataPerc object
	/// </summary>
	//=========================================================================
	public ref class MDataPerc : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataPerc (System::IntPtr pDataObjPtr);
	
		/// <summary>
		/// Constructor
		/// </summary>
		MDataPerc ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Double)
		/// </summary>
		property System::Double Value { virtual System::Double get() new; virtual void set(System::Double value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataQty object
	/// </summary>
	//=========================================================================
	public ref class MDataQty : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataQty (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataQty ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a double)
		/// </summary>
		property System::Double Value { virtual System::Double get() new; virtual void set(System::Double value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataMon object
	/// </summary>
	//=========================================================================
	public ref class MDataMon : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataMon (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataMon ();

		/// <summary>
		/// Constructor
		/// </summary>
		MDataMon (System::Double value);

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a double)
		/// </summary>
		property System::Double Value { virtual System::Double get() new; virtual void set(System::Double value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataStr object
	/// </summary>
	//=========================================================================
	public ref class MDataStr : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataStr (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataStr ();

		/// <summary>
		/// Constructor
		/// </summary>
		MDataStr (System::String^ value );

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the value of the Dataobj (as a string)
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
 		property System::String^ Value { virtual System::String^ get() new; virtual void set(System::String^ value); }

		/// <summary>
		/// Gets or sets the max length for the string
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
 		property int MaxLength { virtual int get(); virtual void set(int value); }

		/// <summary>
		/// Gets or sets the max length for the string
		/// </summary>
		[System::ComponentModel::Browsable(false)]
 		property System::String^ FormatterName { virtual System::String^ get(); virtual void set(System::String^ value); }

		/// <summary>
		/// Gets or sets the uppercase status of the string
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
 		property bool UpperCase { bool get(); void set(bool value); }

	};


	/// <summary>
	/// Class that wraps the c++ tasbuilder DataGuid object
	/// </summary>
	//=========================================================================
	public ref class MDataGuid : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataGuid (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataGuid ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a Guid)
		/// </summary>
		property System::Guid Value { virtual System::Guid get() new; virtual void set(System::Guid value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataDate object
	/// </summary>
	//=========================================================================
	public ref class MDataDate : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataDate (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataDate ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a DateTime)
		/// </summary>
		property System::DateTime Value { virtual System::DateTime get() new; virtual void set(System::DateTime value); }

		/// <summary>
		/// Gets the scripting format of the Date object
		/// </summary>
		property System::String^ ScriptingFormat { virtual System::String^ get() override; }

		void SetFullDate		(bool FullDate		){ m_pDataObj->SetFullDate(FullDate); }
		void SetAsTime			(bool IsTime		){ m_pDataObj->SetAsTime(IsTime);}

		//date constants
		static property System::DateTime NullDate {  System::DateTime get(); }
		static property System::DateTime MaxDate  {  System::DateTime get(); }
		static property System::DateTime MinDate  {  System::DateTime get(); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataText object
	/// </summary>
	//=========================================================================
	public ref class MDataText : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataText (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataText ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as a System::String)
		/// </summary>
		property System::String^ Value { virtual System::String^ get() new; virtual void set(System::String^ value); }
	};
	
	/// <summary>
	/// Class that wraps the c++ tasbuilder DataBlob object
	/// </summary>
	//=========================================================================
	public ref class MDataBlob : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataBlob (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataBlob ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as an array of bytes)
		/// </summary>
		property array<System::Byte>^ Value { virtual array<System::Byte>^ get() new; virtual void set(array<System::Byte>^ value); }
	};

	/// <summary>
	/// Class that wraps the c++ tasbuilder DataObjArray object
	/// </summary>
	//=========================================================================
	public ref class MDataArray : MDataObj
	{
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="pDataObjPtr">is the c++ pointer of the dataobj</param>
		MDataArray (System::IntPtr pDataObjPtr);
		
		/// <summary>
		/// Constructor
		/// </summary>
		MDataArray ();

		/// <summary>
		/// sets the value of the dataobj to the specified object
		/// </summary>
		/// <param name="value">the value to set</param>
		virtual void SetValue (System::Object^ value) override;
		
		/// <summary>
		/// abstract method, gets the Value of the databj
		/// </summary>
		virtual	System::Object^ GetValue () override;

	public:
		/// <summary>
		/// Gets or sets the Value of the Dataobj (as an array of MDataObj)
		/// </summary>
		property array<MDataObj^>^ Value { virtual array<MDataObj^>^ get() new; virtual void set(array<MDataObj^>^ value); }
	};

	/// <summary>
	/// Intenral Use
	/// </summary>
	//=========================================================================
	class MDataObjEvents : public CDataEventsObj
	{
	public:
		gcroot<MDataObj^> DataObj;
		gcroot<IDocumentDataManager^> Document;
	
		MDataObjEvents(MDataObj^ dataObj) : DataObj(dataObj) { m_bOwned = true; } 
		~MDataObjEvents();
		virtual CObserverContext* GetContext() const;
	
		//metodo per segnalare che l'evento può essere scatenato
		virtual void Signal(CObservable* pSender, EventType eType);
		virtual void Fire(CObservable* pSender, EventType eType);
		MDataObjEvents* Clone();
		MDataObj^	GetDataObj ();

	private:
		void FireUnattendedValueChanged	();

	};
}
}
}
