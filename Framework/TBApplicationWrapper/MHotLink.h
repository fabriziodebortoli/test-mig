#pragma once

#include <tbges\hotlink.h>

#include "MSqlRecord.h"
#include "MDataManager.h"
#include "EasyBuilderComponents.h"

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces;

using namespace ICSharpCode::NRefactory;
using namespace ICSharpCode::NRefactory::CSharp;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	ref class MParsedControl;	
	ref class MHotLink;	
	ref class DataObjUITypeEditor;
	ref class HotLinkFieldEditor;
	ref class DocumentUITypeEditor;
	ref class HotLinkSearchesUITypeEditor;

 
	public enum class HotLinkSelectionType { None, DirectAccess, UpperButton, LowerButton, ComboAccess, CustomAccess};


	ref class SearchFieldConverter : public System::ComponentModel::StringConverter
    {
	public:
		virtual bool CanConvertFrom(System::ComponentModel::ITypeDescriptorContext^ context, System::Type^ sourceType) override;
        virtual Object^ ConvertFrom(System::ComponentModel::ITypeDescriptorContext^ context, System::Globalization::CultureInfo^ culture, System::Object^ value ) override;
    };
	/// <summary>
	/// Internal use: serializes a HotLink
	/// </summary>
	//===================================================================================
	public ref class HotLinkSerializer : GenericDataManagerSerializer
	{
	public: static System::String^ HKL = "HKL";

		
	private:
		static System::String^ ParamPropertyName			= "Param{0}";
		static System::String^ GetParamByNameMethod			= "GetParamByName";
		static System::String^ ValuePropertyName			= "Value";

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ dbt) override;

		/// <remarks />
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		static System::String^ CreateSerializedType(System::String^ hotLinkName);
	
	protected:
		void						SerializeParameters			(SyntaxTree^ syntaxTree, TypeDeclaration^ hklClass, IDataManager^ dataManager);
		IList<Statement^>^			SerializeParametersValues(IDataManager^ hotLink, System::String^ hklName);
		virtual IList<Statement^>^	SerializeConstruction(IDesignerSerializationManager^ manager, MHotLink^ hotLink, System::String^ varName, System::String^ className);
	};

	/// <summary>
	/// Internal use: serializes a HotLink in a module scenario
	/// </summary>
	//===================================================================================
	public ref class HotLinkSerializerForModuleController : HotLinkSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual ICSharpCode::NRefactory::CSharp::Expression^ GetEventHandlerOwner() override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual AssignmentExpression^ GenerateCodeAttachEventStatement(System::String^ varName, Microarea::TaskBuilderNet::Core::EasyBuilder::EventInfo^ changedEvent, ICSharpCode::NRefactory::CSharp::Expression^ handlerExpression) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::String^ HotLinkSerializerForModuleController::GetOwnerController() override;
	};

	/// <summary>
	/// Internal use: serializes a HotLink in a module scenario
	/// </summary>
	//===================================================================================
	public ref class HotLinkSerializerForSharedHotLinks : HotLinkSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object) override;

	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual IList<Statement^>^ SerializeConstruction(IDesignerSerializationManager^ manager, MHotLink^ hotLink, System::String^ varName, System::String^ className) override;

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
	};

	//===================================================================================
	public ref class HotLinkSearchSerializer : EasyBuilderSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ Serialize (IDesignerSerializationManager^ manager, Object^ current) override;
	};
		
	/// <summary>
	/// Represents a wrapper to HotLinks
	/// </summary>
	//=========================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::DefaultPropertyAttribute("Description")]
	[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MHotLinkParam : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem, IMHotLinkParam
	{
	private:
		System::String^		name;
		System::String^		description;
		MDataObj^	data;

	public:
		MHotLinkParam (System::String^ name, System::String^ description, MDataObj^ data);

	public:
		/// <summary>
		/// Gets the name of the hotlink parameter
		/// </summary>
		virtual property System::String^	Name	{ virtual System::String^ get () { return name; } }

		/// <summary>
		/// Gets the description of the hotlink parameter
		/// </summary>
		virtual property System::String^	Description	{ virtual System::String^ get () { return description; } }

		/// <summary>
		/// Gets the value of the hotlink parameter
		/// </summary>
		[System::ComponentModel::EditorAttribute(DataObjUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property MDataObj^	Data	{ MDataObj^ get () { return data; } void set (MDataObj^ v) { data = v; } } 

		/// <summary>
		/// Gets the value of the hotlink parameter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Object^	Value	{ System::Object^ get () { return data->Value; } void set (System::Object^ v) { data->Value = v; } } 

	public:
		virtual System::String^ ToString() override;
	};

	/// <summary>
	/// Represents a wrapper to HotLinks
	/// </summary>
	[System::ComponentModel::Design::Serialization::DesignerSerializer(HotLinkSearchSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	//=========================================================================
	public ref class MHotLinkSearch : public Microarea::TaskBuilderNet::Core::EasyBuilder::EasyBuilderComponent
	{
	public:
		enum class ButtonAssociation { None, UpperButton, LowerButton };

	public:
		static System::String^ SearchByKeyName			= "ByKey";
		static System::String^ SearchByDescriptionName	= "ByDescription";
	
	private:
		System::String^		name;
		System::String^		description;
		System::String^		fieldName;
		MHotLink^			hotLink;
		ButtonAssociation	associatedButton;
		bool				useInComboBox;
		bool				showInContextMenu;

	public:
		MHotLinkSearch(MHotLink^ hotLink, bool hasCodeBehind);

		/// <summary>
		/// Gets or the sets the query name 
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
		property System::String^ Name { virtual System::String^ get() override{ return name; } virtual void set(System::String^ value) override { name = value; } }

		/// <summary>
		/// Gets or the sets the query descriptin
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		[System::ComponentModel::Localizable(true), LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ Description { System::String^ get() { return description; } void set(System::String^ value) { description = value; } }
		
		/// <summary>
		/// Gets or the sets the field related with the search
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		[System::ComponentModel::EditorAttribute(HotLinkFieldEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
        [LocalizedCategory("DataCategory", EBCategories::typeid)]
		property System::String^ FieldName { System::String^ get(); void set(System::String^ value); }

		/// <summary>
		/// Gets or the sets the associated button
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property ButtonAssociation AssociatedButton { ButtonAssociation get(); void set(ButtonAssociation value); }

		/// <summary>
		/// Gets or the sets if search is used in combo Box
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool UseInComboBox { bool get(); void set(bool value); }

		/// <summary>
		/// Show the search in context menu
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool ShowInContextMenu { bool get(); void set(bool value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get () override { return GetType()->Name; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName { virtual System::String^ get () override; }

	internal:
		property MHotLink^	HotLink { MHotLink^ get() { return hotLink; } }
		property bool		IsSearchByKey { bool get() { return Name == SearchByKeyName; } }

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanChangeProperty	(System::String^ propertyName) override;

	};

	//=========================================================================
	public ref class MHotLinkSearches : public EasyBuilderComponents
	{
	private:
		MHotLinkSearches^ originalCollection;

	public:
		property MHotLinkSearch^	ByKey			{ MHotLinkSearch^ get (); } 
		property MHotLinkSearch^	ByDescription	{ MHotLinkSearch^ get (); } 

		[System::ComponentModel::Browsable(false)]
		property MHotLinkSearch^	UpperButton	{ MHotLinkSearch^ get (); } 		
		[System::ComponentModel::Browsable(false)]
		property MHotLinkSearch^	LowerButton	{ MHotLinkSearch^ get (); } 
		[System::ComponentModel::Browsable(false)]
		property MHotLinkSearch^	ComboBox	{ MHotLinkSearch^ get (); } 
		
		property MHotLinkSearch^	default[System::String^] {  virtual MHotLinkSearch^ get (System::String^ searchName); } 

		property bool				HaveSearchesInContextMenu { bool get(); }

	public:
		MHotLinkSearches (EasyBuilderComponent^ parent);

	internal:
		MHotLinkSearch^ ContextMenu				(int nMenuNr);
		void			AfterCreateComponents	();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		void			Initialize				();

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual EasyBuilderComponent^ CreateNewInstance () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void ApplyChanges () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool HasChanged	() override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsEditable () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual EasyBuilderComponents^ Clone () override;
	};

	/// <summary>
	/// Provides information about all hotlink events.
	/// </summary>
	//=============================================================================
	public ref class HotLinkEventArgs : EasyBuilderEventArgs
	{
	private:
		IRecord^	record;
		bool		cancel;
	
	public:
		/// <summary>
		/// Gets or sets the Record for the event
		/// </summary>
		property IRecord^ Record { IRecord^ get (); void set (IRecord^ record); }
		
		/// <summary>
		/// Gets or sets Cancel for the event
		/// </summary>
		property bool Cancel { bool get (); void set (bool value); }

	public:
		/// <summary>
		/// Initializes a new instance of HotLInkEventArgs
		/// </summary>
		/// <param name="record">The record containing data.</param>
		HotLinkEventArgs(IRecord^ record);
	};

	/// <summary>
	/// Provides information about hotlink events related witg Combo Box.
	/// </summary>
	//=============================================================================
	public ref class HotLinkComboEventArgs : HotLinkEventArgs
	{
	private:
		System::String^		result;
	
	public:
		/// <summary>
		/// Gets or sets the Result description for the combo box item
		/// </summary>
		property System::String^ Result { System::String^ get (); void set (System::String^ result); }
		
	public:
		/// <summary>
		/// Initializes a new instance of HotLinkComboEventArgs
		/// </summary>
		/// <param name="record">The record containing data.</param>
		HotLinkComboEventArgs(IRecord^ record);
	};

	/// <summary>
	/// Represents a wrapper to HotLinks
	/// </summary>
	//=========================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(HotLinkSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(HotLinkSerializerForModuleController::typeid, HotLinkSerializer::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(HotLinkSerializerForSharedHotLinks::typeid, HotLinkSerializerForModuleController::typeid)]
	[System::ComponentModel::TypeConverter(System::ComponentModel::TypeConverter::typeid)]
	public ref class MHotLink : public MGenericDataManager, IDataManager, IMHotLink
	{
		private:
			static System::String^ DefaultDecodeName	= "l_DefaultDecode";

		private:
			MHotLinkSearch^												customSearch;
			HotKeyLink*													m_pHotKeyLink;
			MHotLinkSearches^											searches;
			MParsedControl^												attachedControl;
			bool														isDynamic;
			NameSpace^													sourceNamespace;
			System::Collections::Generic::IList<IMHotLinkParam^>^		parameters;
			bool														readOnDataLoad;
			bool														wasDataUpperCase;
			bool														useFilterQueryInDirectAccess;
			bool														cacheData;
			MSqlTable^													lastWhereClauseTable;
			bool														m_bIsPublished;
			System::String^												m_sHotlinkDescr;
			System::String^												publicationNamespace;

			System::Delegate^											onGoodRecordCallBack;
			System::Runtime::InteropServices::GCHandle					onGoodRecordHandle;
		protected:
			IRecord^				record;

		public:
			/// <summary>
			/// Internal use
			/// </summary>
			/// <remarks>
			///
			/// </remarks>
			[System::ComponentModel::Browsable(false)]
			[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
			virtual event System::EventHandler<EasyBuilderEventArgs^>^ HotLinkDestroyed;
			/// <summary>
			/// Event raised when the user open the linked document to insert data on the fly; useful for initiating document data (document data is precompiled using the hotlink record)
			/// </summary>
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			virtual event System::EventHandler<EasyBuilderEventArgs^>^ CallLink;
		
		public:
			/// <summary>
			/// </summary>
			MHotLink(HotKeyLink* hotKeyLink);
			/// <summary>
			/// Crea un hotlink a partire da una tabella per una personalizzazione di documento
			/// </summary>
			MHotLink (System::String^ tableName, System::String^ hklName, IDocumentDataManager^ document, bool hasCodeBehind);
			/// <summary>
			/// Crea un hotlink a partire da una tabella per una personalizzazione di modulo
			/// </summary>
			MHotLink (System::String^ tableName, NameSpace^ hklNamespace);
			/// <summary>
			///Crea un hotlink a partire da un template per una personalizzazione di documento
			/// </summary>
			MHotLink (NameSpace^ hlkNamespace, System::String^ hklName, IDocumentDataManager^ document, bool hasCodeBehind);
			
			void		FireHotLinkDestroyed();
			HotKeyLink* GetHotLink() { return m_pHotKeyLink; }
			System::IntPtr GetHotLinkPtr() { return System::IntPtr(m_pHotKeyLink); }
			/// <summary>
			// Finalizer
			/// </summary>
			!MHotLink();

			/// <summary>
			// Distructor/Dispose method
			/// </summary>
			~MHotLink();

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			bool CanBeAttached(Microarea::TaskBuilderNet::Core::CoreTypes::DataType dataType, int maxLength, System::String^% error);
		
		private:
			void Init				();
			void AfterConstruction	();
			BOOL IsString			(::DataObj* pDataObj);
			void AttachDefaultEvents();
		public:
	
			/// <summary>
			/// </summary>
			[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
			property bool			Enabled { bool get (); void set (bool value); } 

			/// <summary>
			/// It says if the hotlink is published, so it can be used in reports etc..
			/// </summary>
			[LocalizedCategory("GeneralCategory", EBCategories::typeid), 
				TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)]
			property bool			Published { bool get(); void set(bool value); }

			/// <summary>
			/// Gets the name for the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
			[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
			property System::String^		Name	{ virtual System::String^ get () override; } 

			/// <summary>
			/// Gets the name for the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
			[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
			property System::String^	Description { 
				virtual System::String^ get() ;
				virtual void set(System::String^ value) ;
			}
			
			/// <summary>
			/// Gets the namespace for the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
			[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
			property INameSpace^	Namespace { virtual INameSpace^ get(); }
			
			/// <summary>
			/// Gets the namespace for the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
			[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
			property System::String^	PublicationNamespace { System::String^ get(); void set(System::String^ value);  }

			/// <summary>
			/// Gets the record associated to the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			property IRecord^		Record { virtual IRecord^ get () override; } 
			
			/// <summary>
			/// Gets the table name of the hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			virtual property System::String^	TableName { virtual System::String^ get () { return Record->Name; } }
			
			/// <summary>
			/// Gets the Data relation for the hotlink
			/// </summary>
			[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			property DataRelationType Relation { virtual DataRelationType get () { return DataRelationType::ForeignKey; } } 
		
			/// <summary>
			/// Internal Use
			/// </summary>
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
			property System::String^ SerializedType	{ virtual System::String^ get () override; }

			/// <summary>
			/// Get/sets the namespace of the related document
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
			[System::ComponentModel::EditorAttribute(DocumentUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			property System::String^	LinkedDocumentNamespace { System::String^ get (); void set (System::String^ value); }

			/// <summary>
			/// Enables/Disables the add on fly management
			/// </summary>
			[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
			property bool CanAddOnFly { bool get (); void set (bool); }

			/// <summary>
			/// Enables/Disables the related document management
			/// </summary>
			[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
			property bool CanOpenLinkedDocument { bool get (); void set (bool); }

			/// <summary>
			/// Enables/Disables the search buttons 
			/// </summary>
			[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
			property bool CanSearch { bool get (); void set (bool); }
		
			/// <summary>
			/// Internal use
			/// </summary>
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
			property bool IsUpdatable { virtual  bool get () { return false; }  }
		
			/// <summary>
			/// Internal use
			/// </summary>
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
			property System::Object^		BindableDataSource { virtual System::Object^ get () { return Record; } } 
			
			/// <summary>
			/// When enabled, controls accepts only data existing into the hotlink table, otherwise no
			/// </summary>
			[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
			property bool DataMustExist { bool get (); void set (bool); }
		
			[System::ComponentModel::Browsable(false)]
			property bool CanBeDeleted { virtual bool get () override { return true; }  }

			/// <summary>
			/// Gets or sets the control this hotlink is attached to
			/// </summary>
			[System::ComponentModel::Browsable(false)]
			property MParsedControl^ AttachedControl { MParsedControl^ get(); void set (MParsedControl^ value);	}

			[System::ComponentModel::Browsable(false)]
			virtual property System::Collections::Generic::IList<IMHotLinkParam^>^	Parameters	{ virtual System::Collections::Generic::IList<IMHotLinkParam^>^ get (); } 

			/// <summary>
			/// Enables/Disables the related document management
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
			[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
			property bool	ReadOnDataLoaded { bool get (); void set (bool value); }

			/// <summary>
			// Gets and sets search criteria associated with hotlink
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
			[System::ComponentModel::EditorAttribute(HotLinkSearchesUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
			property MHotLinkSearches^	Searches	{ virtual MHotLinkSearches^ get (); virtual void set (MHotLinkSearches^ value); }

			[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
			[System::ComponentModel::Browsable(false)]
			virtual property System::String^	DBFieldName	{ virtual System::String^ get (); }

			[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
			[System::ComponentModel::Browsable(false)]
			virtual property System::String^	ReturnType	{ virtual System::String^ get (); }

			/// <summary>
			/// FilterQuery by default is excluded by direct access query filters. 
			/// This property allows to execute filterQuery in direct access mode too
			/// </summary>
			[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), LocalizedCategory("DataCategory", EBCategories::typeid)]
			property bool UseFilterQueryInDirectAccess { bool get (); void set (bool value); }

			/// <summary>
			/// Allows to add validation code to hotlink
			/// </summary>
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			virtual event System::EventHandler<HotLinkEventArgs^>^ Validated;

			/// <summary>
			/// Allows to add customize the item descriptions displayed when hotlink is in combo box 
			/// </summary>
			[LocalizedCategory("DataCategory", EBCategories::typeid)]
			virtual event System::EventHandler<HotLinkComboEventArgs^>^ ComboItemPrepared;

			[Browsable(false)]
			property bool CacheData		 { bool get () { return cacheData; } void set(bool value) { cacheData = value; } }

			/// <summary>
			/// override, tells if msqlrecord needs serialization (it consinder also dbt )
			/// </summary>
			virtual bool IsToDelete() override;

			property bool IsDynamic { bool get() { return isDynamic; } }

		internal:
			MSqlRecord^	CreateRecord 			(SqlRecord* pRecord);
			void		OnDefineQuery			(HotKeyLink::SelectionType nQuerySelection);
			void		OnPrepareQuery			(::DataObj* aDataObj, HotKeyLink::SelectionType nQuerySelection);
			CString		FormatComboItem			(SqlRecord* pRec);

			property INameSpace^ SourceNamespace { INameSpace^ get() { return sourceNamespace; }}
			
			[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
			delegate void OnGoodRecordCallBack();
			
			[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
			void OnGoodRecord ();
		
			HotLinkSelectionType GetSelectionType(HotKeyLink::SelectionType type);

		protected:
			virtual SqlTable* GetTable() override;

		public:
			virtual	bool SearchOnLinkUpper	();	
			virtual	bool SearchOnLinkLower	();	
			virtual	void DefineQuery		(MSqlTable^ mTable, HotLinkSelectionType selectionType);
			virtual	void PrepareQuery		(MDataObj^ dataObj, MSqlTable^ mTable, HotLinkSelectionType selectionType);
					void SearchOnLink		(MHotLinkSearch^ search);
					
					void AddUpperSearch		(System::String^ searchField, System::String^ description);
					void AddLowerSearch		(System::String^ searchField, System::String^ description);
					void AddCustomSearch	(System::String^ searchField, System::String^ description); 

			virtual bool ExistData			(IDataObj^ dataObj);
		
		private: 
			MHotLinkSearch^ CreateHotLinkSearch(System::String^ searchField, System::String^ description);
		
		public:
			virtual System::String^			ToString() override;
			virtual System::Collections::Generic::List<System::String^>^	GetCompatibleFieldNames	(bool exactMatch);
			
			/// <summary>
			/// Internal Use
			/// </summary>
			void OnDefineSearchQuery (MSqlTable^ mTable, int nQuerySelection);
			
			/// <summary>
			/// Internal Use
			/// </summary>
			bool ReceiveValidate (System::IntPtr recordPtr);

			MDataObj^		GetCodeDataObj ();
			MHotLinkParam^	GetParamByName (System::String^ name);
			/// <summary>
			/// Internal Use
			/// </summary>
			bool FindRecord (MDataObj^ mDataObj);

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			System::IntPtr GetRecordPtr ();

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual void OnCallLink	();
			
			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual void Add (System::ComponentModel::IComponent^ component, System::String^ name) override;
		
			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual bool CanChangeProperty	(System::String^ propertyName) override;
		
			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual void PrepareFilterQuery (MSqlTable^ mSqlTable, int nQuerySelection);

			/// <summary>
			/// Sets an error message 
			/// </summary>
			void SetError (System::String^ message);

			/// <summary>
			/// Clear the running mode process of hotlink
			/// </summary>
			void ClearRunningMode();

			/// <summary>
			/// Sets a warning message 
			/// </summary>
			void SetWarning (System::String^ message);
			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual void CallCreateComponents () override;

			virtual void	OnRadarRecordAvailable	();

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual bool IsValid ();


		private:
			void InitializeParameters	();
			void RestoreDataUpperCase	();
			void SetDataUpperCase 		();
	};

	//=========================================================================
	public ref class MHotLinkTable : public MSqlTable
	{
	public:
		MHotLinkTable(System::IntPtr tablePtr, MSqlRecord^ templateRecord);
	};
}}}


