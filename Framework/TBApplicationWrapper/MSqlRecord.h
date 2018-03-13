#pragma once


#include "MDataObj.h"
#include "MEasyBuilderContainer.h"
#include <TBNameSolver\CallbackHandler.h>	

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::ComponentModel::Design::Serialization;

class SqlRecordLocals;
class SqlRecord; 
class SqlRecordItem;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	ref class MSqlRecordItem;
	ref class MLocalSqlRecordItem;
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class RecordSerializer : EasyBuilderSerializer
	{
	public:
		static System::String^ GetFieldMethodName			= "GetField";
		static System::String^ GetRecordPtrMethodName		= "GetRecordPtr";
		static System::String^ GetOldRecordPtrMethodName	= "GetOldRecordPtr";
		static System::String^ RecordPropertyName			= "Record";
		
	private:
		static System::String^ GetFieldPtrMethodName	= "GetFieldPtr";
		
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass (SyntaxTree^ syntaxTree, IComponent^ record) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual PropertyDeclaration^	GenerateFieldAccessor (MSqlRecordItem^ recordField);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }
	};
	ref class MSqlRecord;
	public ref class MSqlRecordTypeDescriptionProvider : public EBTypeDescriptionProvider<MSqlRecord^>
	{
	
	};
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(RecordSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::TypeDescriptionProvider(MSqlRecordTypeDescriptionProvider::typeid)]
	public ref class MSqlRecord : public MEasyBuilderContainer, IRecord, System::IDisposable, System::ComponentModel::INotifyPropertyChanged, System::ComponentModel::INotifyPropertyChanging
	{
	private:
		TDisposablePtr<SqlRecord>*		m_ppRecord;
		SqlRecordLocals*				m_pLocals;
		bool							m_bComponentsCreated;
		System::String^					instanceName;

	protected:	
		property INameSpace^ NameSpaceIRecord { virtual INameSpace^ get () =  IRecord::NameSpace::get { return NameSpace;} 	}

	public:
		/// <summary>
		/// Fires when a property value is changed
		/// </summary>
		virtual event System::ComponentModel::PropertyChangedEventHandler^ PropertyChanged;
		/// <summary>
		/// Fires when a property value is changing
		/// </summary>
		virtual event System::ComponentModel::PropertyChangingEventHandler^ PropertyChanging;
		
		/// <summary>
		/// Gets or Sets the IsStorable attribute of the sql record
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsStorable { bool get(); void set (bool value); }

		/// <summary>
		/// Gets the Namespace of che current SqlRecord
		/// </summary>
		property Microarea::TaskBuilderNet::Core::Generic::NameSpace^ NameSpace	 { virtual Microarea::TaskBuilderNet::Core::Generic::NameSpace^ get(); }
		
		/// <summary>
		/// Gets the release number associated to the creation of the record itself
		/// </summary>
		property int CreationRelease { virtual int get(); }
		
		/// <summary>
		/// Gets the release number associated to the creation of the record itself
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool IsAddonInstanceOfSharedSerializedClass { virtual bool get() override; }

		/// <summary>
		/// Gets or the sets the browser query for the dbt
		/// </summary>
		[Browsable(false)]
		property bool		IsMasterTable { virtual bool get(); }

		/// <summary>
		/// Gets the record description
		/// </summary>
		property virtual System::String^ RecordDescription { virtual System::String^ get(); }

		/// <summary>
		/// Returns true if the SqlRecord is valid
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsValid { virtual bool get(); }
		
		/// <summary>
		/// Returns true if the SqlRecord is registered
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsRegistered { virtual bool get(); }
	
		/// <summary>
		/// Returns the type of the Record (Table, StoredProcedure, View, Virtual)
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property DataModelEntityType RecordType { virtual DataModelEntityType get(); }
		
		/// <summary>
		/// Gets the colleciton of Fields related to the sql record
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Collections::IList^ Fields { virtual System::Collections::IList^ get(); }
		
		/// <summary>
		/// Gets the name of the sql record
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Name { virtual System::String^ get() override; }
		
		/// <summary>
		/// Returns a collection of IRecordFiles, one for each field that is related to the primary key
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Collections::IList^ PrimaryKeyFields { virtual System::Collections::IList^ get(); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType	{ virtual System::String^ get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName	{ virtual System::String^ get () override; }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool			Qualified		{ bool get (); void set (bool value); } 	

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^		Qualifier		{ System::String^ get (); void set (System::String^ value); } 

		/// <summary>
		/// Gets the name of the sql record
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ InstanceName { System::String^ get(); void set(System::String^ value); }

	public:
		/// <summary>
		/// Initializes a new instance of MSqlRecord
		/// </summary>
		MSqlRecord ();
		
		/// <summary>
		/// Initializes a new instance of MSqlRecord with the given table name
		/// </summary>
		MSqlRecord (System::String^ tableName);
		
		/// <summary>
		/// Internal Use: Initializes a new instance of MSqlRecord
		/// </summary>
		MSqlRecord (System::IntPtr sqlRecPtr);

		/// <summary>
		/// Distructor
		/// </summary>
		~MSqlRecord();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MSqlRecord();
		
	internal:
		MSqlRecord (SqlRecord*	pRecord);
		virtual event System::EventHandler<System::EventArgs^>^ UnmanagedObjectDisposing;
		::SqlRecord*	GetSqlRecord ();
		void			AttachSqlRecord (::SqlRecord* pRecord, bool hasCodeBehind);
		IDataObj^		GetData (::DataObj* pDataObj);
		void FireUnmanagedObjectDisposing(){ UnmanagedObjectDisposing(this, System::EventArgs::Empty); }
	
	private:
		void AddRecItems(const ::SqlRecord* pRec);

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void			FirePropertyChanged(System::String^ propertyName);
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void			FirePropertyChanging(System::String^ propertyName);
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		static System::Collections::IList^ GetCompatibleFieldsWith (IRecordField^ masterField, System::Collections::IList^ fields);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void RefreshFields();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::Collections::IList^ GetFieldsNoExtensions();
	
	public:
		void	Init ();

		virtual MLocalSqlRecordItem^ AddLocalField(System::String^ name, System::String^ dataType, int length);
		virtual MLocalSqlRecordItem^ AddLocalField(System::String^ name, Microarea::TaskBuilderNet::Core::CoreTypes::DataType^ dataType, int length);
		/// <summary>
		/// Returns the primary key of the sql record as a string
		/// </summary>
		void GetKeyInXMLFormat (System::String^% key, bool bEnumAsString);
		
		/// <summary>
		/// Returns a string containing the primary key name of the sql record
		/// </summary>
		System::String^ GetPrimaryKeyNameValue ();

		/// <summary>
		/// Set the primary key value
		/// </summary>
		void SetPrimaryKeyNameValue(System::String^ strKey);

		/// <summary>
		/// Returns the IRecordField related to the provided dataobj
		/// </summary>
		IRecordField^ GetField (MDataObj^ dataObj);

		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		virtual IRecordField^ Add (System::String^ name, DataModelEntityFieldType type, IDataType^ dataType, System::String^ localizableName, int length);
		
		/// <summary>
		/// Returns the IRecordField related to the provided field name
		/// </summary>
		virtual IRecordField^ GetField(System::String^ name);
		
		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		virtual System::IntPtr GetFieldPtr(System::String^ name);
			
		/// <summary>
		/// Sets the value of the specified field with the desired value
		/// </summary>
		virtual void SetValue(System::String^ fieldName, Object^ value);

		/// <summary>
		/// override, tells if msqlrecord needs serialization (it consinder also dbt )
		/// </summary>
		virtual bool IsToDelete() override;
		
		/// <summary>
		/// Gets the value of the field related to the provided fieldName
		/// </summary>
		virtual Object^ GetValue(System::String^ fieldName);
		
		/// <summary>
		/// Gets the dataobj related to the provided fieldName
		/// </summary>
		virtual IDataObj^ GetData (System::String^ fieldName);
	
		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add(System::ComponentModel::IComponent^ component, System::String^ name) override;
		
		/// <summary>
		/// Gets the IRecordField related to the provided dataObj
		/// </summary>
		virtual	IRecordField^ GetField(IDataObj^ dataObj);

		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;
		
		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		void GetCompatibleDataTypes(System::String^ columnName, System::Collections::Generic::List<Microarea::TaskBuilderNet::Core::CoreTypes::DataType>^ dataTypes);

		/// <summary>
		/// Internal Use 
		/// </summary>
		//virtual bool Equals(System::Object^ obj) override;
		
		/// <summary>
		/// Internal Use 
		/// </summary>
		virtual int GetHashCode()override;

		/// <summary>
		/// Assign the value of this record
		/// </summary>
		void Assign(MSqlRecord^ record);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class RecordFieldSerializer : EasyBuilderSerializer
	{
	public:
		static System::String^ AddLocalFieldMethodName = "AddLocalField";
		static System::String^ GetFieldPtrMethodName = "GetFieldPtr";

	public:
		/// <summary>
		/// Internal Use 
		/// </summary>
		virtual Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, Object^ current) override;
		
		/// <summary>
		/// Internal Use 
		/// </summary>
		virtual TypeDeclaration^ SerializeClass (SyntaxTree^ syntaxTree, IComponent^ record) override;

		/// <summary>
		/// Internal Use 
		/// </summary>
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }
	};

	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(RecordFieldSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MSqlRecordItem : EasyBuilderComponent, IRecordField, IChangedEventsSource
	{
	protected:
		MSqlRecord^					m_Owner;
		SqlRecordItem*				m_pItem;
		DataModelEntityFieldType	m_type;
		IDataType^					m_DataObjType;
	private:
		IDataObj^					m_DataObj;

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
		/// Internal Use: Initializes a new instance of MDBTObject
		/// </summary>
		MSqlRecordItem 
			(
				MSqlRecord^				owner,
				SqlRecordItem*			pItem, 
				DataModelEntityFieldType type, 		
				bool					bOwnsSqlRecordItem
			); 
		
		/// <summary>
		/// Internal Use: Initializes a new instance of MDBTObject
		/// </summary>
		MSqlRecordItem 
			(
				MSqlRecord^				owner,
				SqlRecordItem*			pItem, 
				DataModelEntityFieldType type, 
				bool					bOwnsSqlRecordItem,
				IDataType^				dataObjType
			);
		virtual ~MSqlRecordItem();
		!MSqlRecordItem();

	public:
		/// <summary>
		/// Gets the name of the current sql record item
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::ReadOnly(true)]
		property System::String^ Name { virtual System::String^ get () override; }

		/// <summary>
		/// Gets the name of the current sql record item
		/// </summary>
		[System::ComponentModel::Browsable(false), LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::ReadOnly(true)]
		property System::ComponentModel::IComponent^ EventSourceComponent { virtual System::ComponentModel::IComponent^ get () override;  }

		/// <summary>
		/// Gets the localizable name of the sql record item
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ LocalizableName { virtual System::String^ get (); }
		
		/// <summary>
		/// Gets the predefined value for the sql record item
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ DefaultValue { virtual System::String^ get (); }
		
		/// <summary>
		/// Gets the length of the sql record item
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property int Length { virtual int get ();  }
		
		/// <summary>
		/// Gets the release number associated to the creation of the record itself
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property int CreationRelease { virtual int get(); }

		/// <summary>
		/// Gets the predefined value for the sql record item
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ ContextName { virtual System::String^ get (); }

	protected:	
		property IDataType^ DataObjTypeIRecordField { virtual IDataType^ get () =  IRecordField::DataObjType::get; }
	public:
		/// <summary>
		/// Gets the namespace of the module that owns the sql record item
		/// </summary>
		property Microarea::TaskBuilderNet::Core::Generic::NameSpace^ OwnerModule { virtual Microarea::TaskBuilderNet::Core::Generic::NameSpace^ get ();	}
		
		/// <summary>
		/// Gets the type of the dataobj related to the sql record item
		/// </summary>
		property Microarea::TaskBuilderNet::Core::CoreTypes::DataType DataObjType {	virtual Microarea::TaskBuilderNet::Core::CoreTypes::DataType get (); }
		
		/// <summary>
		/// Gets the DataModelEntityFieldType of the sql record item
		/// </summary>
		property DataModelEntityFieldType Type { virtual DataModelEntityFieldType get (); }
			
		/// <summary>
		/// Gets the dataobj related to the sql record item
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDataObj^ DataObj { virtual IDataObj^ get (); internal: virtual void set(IDataObj^ value);}
		
		/// <summary>
		/// Gets the record that owns the sql record item
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IRecord^ Record { virtual IRecord^ get (); }
		
		/// <summary>
		/// Gets or sets the value of the sql record item
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property Object^ Value { virtual Object^ get (); virtual void set (Object^ value); }
		
		/// <summary>
		/// Returns true if the sql record items is part of the primary key
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool IsSegmentKey { virtual bool get (); }
		
		/// <summary>
		/// Gets the qualified name of the sql record item
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ QualifiedName { virtual System::String^ get (); }

		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName	{ virtual System::String^ get () override; }
		
		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType	{ virtual System::String^ get () override; }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDocumentDataManager^ Document { virtual IDocumentDataManager^ get() override; }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of MSqlRecordItem
		/// </summary>
		void SetColumnInfo (System::IntPtr pSqlColumnInfo);
		
		/// <summary>
		/// Internal Use: Do not use it (RR)
		/// </summary>
		[ExcludeFromIntellisense]
		void ReplaceDataObj (System::IntPtr pDataObj, bool deletePrev);
	
	public:
		/// <summary>
		/// Return true if the desired record field is compatible with the record
		/// </summary>
		/// <param name="field">the record field to test</param>
		virtual bool IsCompatibleWith (IRecordField^ field);

	protected:
		void OnValueChanged(Object^ sender, EasyBuilderEventArgs^ args);
		void OnValueChanging(Object^ sender, EasyBuilderEventArgs^ args);
	private:
		bool NeedsQualification();
	};

	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(RecordFieldSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MLocalSqlRecordItem : public MSqlRecordItem
	{
		public:
		/// <summary>
		/// Internal Use: Initializes a new instance of MDBTObject
		/// </summary>
		MLocalSqlRecordItem 
			(
				MSqlRecord^				owner,
				SqlRecordItem*			pItem, 
				bool					bOwnsSqlRecordItem
				) 
				: MSqlRecordItem (owner, pItem, DataModelEntityFieldType::Variable, bOwnsSqlRecordItem)
		{
			HasCodeBehind = false;
		} 
		
		/// <summary>
		/// Internal Use: Initializes a new instance of MDBTObject
		/// </summary>
		MLocalSqlRecordItem 
			(
				MSqlRecord^				owner,
				SqlRecordItem*			pItem, 
				bool					bOwnsSqlRecordItem,
				IDataType^				dataObjType
			) : MSqlRecordItem (owner, pItem, DataModelEntityFieldType::Variable, bOwnsSqlRecordItem, dataObjType) 
		{
			HasCodeBehind = false;
		} 

		property bool CanBeDeleted { virtual bool get () override;  }
		/// <summary>
		/// Gets or sets the name of the field
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::ComponentState)]
		[System::ComponentModel::Browsable(true), System::ComponentModel::ReadOnly(false), LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set(System::String^ value) override; }
		/// <summary>
		/// Gets or sets the data type of the field
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::ComponentState)]
		[System::ComponentModel::Browsable(true), 
			LocalizedCategory("GeneralCategory",
			EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property Microarea::TaskBuilderNet::Core::CoreTypes::DataType DataObjType 
		{ 
			virtual Microarea::TaskBuilderNet::Core::CoreTypes::DataType get () override; 
			void set(Microarea::TaskBuilderNet::Core::CoreTypes::DataType value);
		}
		/// <summary>
		/// Gets or sets the data lenght of the field
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::ComponentState)]
		[System::ComponentModel::Browsable(true), LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int Length { virtual int get () override; void set(int value); }
		
		/// <summary>
		/// Gets or sets the value of the current data obj (as an object)
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::ReadOnly(false), LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
		property Object^ Value { virtual Object^ get () override { return __super::Value; } virtual void set (Object^ value) override { __super::Value = value; } }
		
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DefaultValue { virtual System::String^ get ()override { return __super::DefaultValue; } }
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int CreationRelease { virtual int get()override { return __super::CreationRelease; } }
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsSegmentKey { virtual bool get () override { return __super::IsSegmentKey; }  }
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName	{ virtual System::String^ get () override { return __super::SerializedName; } }
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property Microarea::TaskBuilderNet::Core::Generic::NameSpace^ OwnerModule { virtual Microarea::TaskBuilderNet::Core::Generic::NameSpace^ get ()override { return __super::OwnerModule; }	}
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ QualifiedName { virtual System::String^ get () override { return __super::QualifiedName; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanChangeProperty	(System::String^ propertyName) override;
	};
	/// <summary>
	/// Contains data binding information to use a DataObj as a data source for a parsedcontrol.
	/// </summary>
	//===================================================================================
	[System::ComponentModel::DefaultPropertyAttribute("FullName")]
	[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
	public ref class FieldDataBinding : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem, IDataBinding
	{	
	private:
		MDataObj^		mDataObj;
		IDataManager^	parent;


	public:
		/// <summary>
		/// Initializes a new instance of DBTDataBinding
		/// </summary>
		/// <param name="dataObj">The dataObj to use as data source</param>
		FieldDataBinding (MDataObj^ dataObj);
		/// <summary>
		/// Initializes a new instance of DBTDataBinding
		/// </summary>
		/// <param name="dataObj">The dataObj to use as data source</param>
		FieldDataBinding (MDataObj^ dataObj, IDataManager^ parent);
		
		/// <summary>
		/// Returns true if the databinding is empty
		/// </summary>
		property bool IsEmpty { virtual bool get (); }
		
		/// <summary>
		/// Gets the data of the databinding (as object)
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property Object^ Data { virtual Object^ get (); }

		/// <summary>
		/// Gets the name of the dbt databinding
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Name { virtual System::String^ get (); }

		/// <summary>
		/// Gets the document namespace
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDocumentDataManager^ Document { virtual IDocumentDataManager^ get (); }

		/// <summary>
		/// Get the type of the data related to the databinding
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDataType^	DataType{ virtual IDataType^ get ();}

		/// <summary>
		/// Gets the IRecord that owns the databinding
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IRecord^ Record { virtual IRecord^ get (); }

		/// <summary>
		/// Gets the full name of the databinding (as a full namespace)
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ FullName { virtual System::String^ get () { return ToString(); } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property IDataManager^ Parent { virtual IDataManager^ get () { return parent; } }

		/// <summary>
		/// Returns true if the data contained in data binding is readonly
		/// </summary>
		property bool IsDataReadOnly { virtual bool get (); }

		/// <summary>
		/// Returns true if data bound to this control should be visible
		/// </summary>
		property bool DataVisible { virtual bool get(); virtual void set(bool value); }

		/// <summary>
		/// Overrides the ToString method for the databiding class
		/// </summary>
		virtual System::String^ ToString() override;

		/// <summary>
		/// Overrides the Clone method for the databiding class
		/// </summary>
		virtual Object^ Clone();

		/// <summary>
		/// Modified flag
		/// </summary>
		property bool Modified { virtual bool get(); virtual void set(bool value); }

	private:
		void InitializeParent();
	};
	
	ref class MDBTObject;
	
	/// <summary>
	/// Contains data binding information to use a Dbt as a data source for a parsedcontrol.
	/// </summary>
	//===================================================================================
	[System::ComponentModel::DefaultPropertyAttribute("FullName")]
	[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
	public ref class DBTDataBinding : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem, IDataBinding
	{	
	private:
		MDBTObject^ mDbt;
	
	public:
		/// <summary>
		/// Initializes a new instance of DBTDataBinding
		/// </summary>
		/// <param name="dbt">The dbt to use as data source</param>
		DBTDataBinding (MDBTObject^ dbt);
		
		/// <summary>
		/// Returns true if the dbt databinding is empty
		/// </summary>
		property bool IsEmpty { virtual bool get (); }
		
		/// <summary>
		/// Gets the data of the dbt databinding (as object)
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property Object^ Data { virtual Object^ get (); virtual void set (Object^ data); }
		
		/// <summary>
		/// Gets the name of the dbt databinding
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Name { virtual System::String^ get (); }

		
		/// <summary>
		/// Gets the document namespace
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDocumentDataManager^ Document { virtual IDocumentDataManager^ get (); }
		/// <summary>
		/// Gets the type of the dbt databinding 
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDataType^ DataType{ virtual IDataType^ get ();}
		
		/// <summary>
		/// Returns true if the data contained in data binding is readonly
		/// </summary>
		property bool IsDataReadOnly { virtual bool get (); }

		/// <summary>
		/// Returns true if data bound to this control should be visible
		/// </summary>
		property bool DataVisible { virtual bool get(); virtual void set(bool value); }

		/// <summary>
		/// Gets the full name of the dbt databinding (as a full namespace)
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ FullName { virtual System::String^ get () { return ToString(); } }

		/// <summary>
		/// Overrides the ToString method for the databiding class
		/// </summary>
		virtual System::String^ ToString() override;

		/// <summary>
		/// Overrides the Clone method for the databiding class
		/// </summary>
		virtual Object^ Clone();

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property IDataManager^ Parent { virtual IDataManager^ get () { return nullptr; } }

		/// <summary>
		/// Modified flag
		/// </summary>
		property bool Modified { virtual bool get(); virtual void set(bool value); }

	};
}
}
}
