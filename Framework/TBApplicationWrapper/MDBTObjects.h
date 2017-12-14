#pragma once

#include <TbGeneric/DataObj.h>
#include <TbGes/DBT.h>
#include <TbGes/XMLGesInfo.h>

#include "Utility.h"
#include "MSqlRecord.h"
#include "MSqlTable.h"
#include "MSqlCatalog.h"
#include "MDataManager.h"


using namespace System::Collections::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace System::Runtime::InteropServices;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::ComponentModel::Design::Serialization;
using namespace Microarea::TaskBuilderNet::UI::WinControls::Others;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
class WClause;
class DBTObjectProxy;

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class MXMLSearchBookmark 
	{	
	private:
		CXMLSearchBookmark* m_pSearchBookmark;
		
	public:
		/// <summary>
		/// Constructor
		/// </summary>
		MXMLSearchBookmark(System::IntPtr dbtPtr);
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^ FieldName { System::String^  get ();}
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property bool ShowAsDescription	{ bool  get();}
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^ HKLName { System::String^  get ();}

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^ KeyCode { System::String^  get ();}

	};
	
	
	/// <summary>
	/// Internal use: serializes a Dbt
	/// </summary>
	//===================================================================================
	public ref class DBTSerializer : GenericDataManagerSerializer
	{
	private:
		static System::String^ AttachMasterMethodName		= "AttachMaster";
		static System::String^ AttachSlaveMethodName		= "AttachSlave";
		static System::String^ MasterPropertyName			= "Master";
		static System::String^ AddMasterForeignKeyMethodName= "AddMasterForeignKey";
		static System::String^ AddRecordMethodName			= "AddRecord";
		static System::String^ AddRowMethodName				= "AddRow";
		static System::String^ GetCurrentRecordMethodName	= "GetCurrentRecord";
		static System::String^ GetCurrentRowMethodName		= "GetCurrentRow";
		static System::String^ InsertRecordMethodName		= "InsertRecord";
		static System::String^ InsertRowMethodName			= "InsertRow";
		static System::String^ GetRowMethodName				= "GetRow";
		static System::String^ GetRecordMethodName			= "GetRecord";
		static System::String^ GetOldRowMethodName			= "GetOldRow";
		static System::String^ GetOldRecordMethodName		= "GetOldRecord";
		static System::String^ RowNrParamName				= "rowNumber";
		static System::String^ OldRecordPropertyName		= "OldRecord";
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ dbt) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		void SerializeOldAccessors (SyntaxTree^ syntaxTree, TypeDeclaration^ dbtClass, MSqlRecord^ record);

		/// <remarks />
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }
	
	private:
		void						SerializeRowMethods		(SyntaxTree^ syntaxTree, TypeDeclaration^ aParentClass, MSqlRecord^ record);
		IList<Statement^>^			SerializeForeignKey		(IDataManager^ dbt, System::String^ varName);
	};

	/// <summary>
	/// Provides information about all dbt events regarding the current data row.
	/// </summary>
	//=============================================================================
	public ref class RowEventArgs : EasyBuilderEventArgs
	{
	private:
		System::Int16		nRow;
		IRecord^	record;
		bool		cancel;
	
	public:
		/// <summary>
		/// Gets or sets the row number for the event
		/// </summary>
		property System::Int16 RowNumber { System::Int16 get (); void set (System::Int16 nValue); }
		
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
		/// Initializes a new instance of RowEventArgs
		/// </summary>
		/// <param name="nRow">Row number to identify data.</param>
		/// <param name="record">The record containing data.</param>
		RowEventArgs(System::Int16 nRow, IRecord^ record);
	};

	/// <summary>
	/// Provides information about the CheckPrimaryKey event. 
	/// </summary>
	//=============================================================================
	public ref class BadDataRowEventArgs : RowEventArgs
	{
	private:
		MDataObj^	badData;
	
	public:
		/// <summary>
		/// Gets or sets the Bad Data for the event
		/// </summary>
		property MDataObj^ BadData { MDataObj^ get (); void set (MDataObj^ nValue); }

	public:
		/// <summary>
		/// Initializes a new instance of BadDataRowEventArgs
		/// </summary>
		/// <param name="nRow">Row number to identify data.</param>
		/// <param name="record">The record containing data.</param>
		BadDataRowEventArgs(System::Int16 nRow, IRecord^ record);
	};

	/// <summary>
	/// Root class for all Dbt classes
	/// </summary>
	//===================================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(DBTSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MDBTObject abstract : MGenericDataManager, IDataManager
	{
	private:
		System::Delegate^			onCheckPrimaryKeyCallBack;
		System::Runtime::InteropServices::GCHandle	onCheckPrimaryKeyHandle;

	protected:
		System::Collections::IList^	records;
		TDisposablePtr<DBTObject>*m_ppDBTObject;
		IRecord^			record;
		IRecord^			oldRecord;
	
	internal:
		MSqlTable^			table;
	

	public:
		/// <summary>
		/// Get the Data relation for the dbt
		/// </summary>
		property DataRelationType	Relation { virtual DataRelationType get () abstract; } 

	public:
		/// <summary>
		/// Initializes a new instance of MDBTObject
		/// </summary>
		MDBTObject ();

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MDBTObject (System::IntPtr dbtPtr);

		/// <summary>
		/// Distructor
		/// </summary>
		~MDBTObject();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MDBTObject();

	public:
		/// <summary>
		/// Get the record associated to the dbt
		/// </summary>	
		property System::Collections::IList^ Records { virtual System::Collections::IList^ get (); } 
		
		/// <summary>
		/// Get the record associated to the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IRecord^ Record { virtual IRecord^ get () override; } 

		/// <summary>
		/// Get the old record associated to the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IRecord^ OldRecord { virtual IRecord^ get (); } 

		/// <summary>
		/// Get the namespace related to the dbt
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get (); }
		
		/// <summary>
		/// Get the name of the dbt
		/// </summary>
		property System::String^ Name { virtual System::String^ get () override; }
		
		/// <summary>
		/// Get the title of the dbt
		/// </summary>
		property System::String^ Title { virtual System::String^ get (); }
	
		/// <summary>
		/// Get the table name of the dbt
		/// </summary>
		property System::String^ TableName { virtual System::String^ get (); }
		
		/// <summary>
		/// true if is valid
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool Valid { virtual bool get (); }
				
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ SerializedType	{ virtual System::String^ get () override; }

		/// <summary>
		/// True if the dbt record can be deleted by this dbt
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool IsDeleteOwner { bool get (); void set (bool value); }

		/// <summary>
		/// True if the dbt is associated to a view
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsAssociatedToAView { bool get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void ReceivePreparePrimaryKey (int nRow, System::IntPtr pRecordPtr) {};

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property bool			IsUpdatable { virtual bool get () { return true; }  }

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual MDBTObject^ CreateAndAttach(System::IntPtr dbtPtr) { return nullptr; }
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property MSqlTable^	Table { virtual MSqlTable^ get (); }

		/// <summary>
		/// Internal use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::Object^		BindableDataSource { virtual System::Object^ get () { return Record; } } 
		
		/// <summary>
		/// Internal use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::IntPtr		DBTPtr { virtual System::IntPtr get () { return (System::IntPtr)(long)GetDBTObject(); } } 
		
	internal:
		::DBTObject*	GetDBTObject();
		virtual void AssignInternalState(MGenericDataManager^ source) override;
		void		 DefineQuery();
		void		 PrepareQuery();

	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void AttachDefaultEvents();

	public:

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetRecordPtr ();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetOldRecordPtr ();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add (System::ComponentModel::IComponent^ component, System::String^ name) override;
		
		/// <summary>
		/// true if the dbt passed as parameter is equal to the current one
		/// </summary>
		/// <param name="obj">dbt to check</param>
		virtual bool Equals (System::Object^ obj) override;
		
		/// <summary>
		/// override, tells if msqlrecord needs serialization (it consinder also dbt )
		/// </summary>
		virtual bool IsToDelete() override;

		/// <summary>
		/// Event Raised when checking for the primary key
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(CheckPrimaryKey, BadDataRowEventArgs, "DataCategory", EBCategories, "Occurs when dbt checks primary key. It is TaskBuilderNet C++ framework \"OnCheckPrimaryKey\" method");

	protected:
		virtual SqlTable* GetTable() override { return GetDBTObject()->GetTable(); }
		
		void	BindAutoincrement	(MDataObj^ data, System::String^ entity);
		void	BindAutonumber		(MDataObj^ data, System::String^ entity);
		void	BindAutonumber		(MDataObj^ data, System::String^ entity, MDataDate^ date);

	internal:
		
		virtual MSqlRecord^	GetRecord 	(SqlRecord* pRecord);
		MSqlRecord^			GetRecord 	(SqlRecord* pRecord, bool invokeLazyRecord);

		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		delegate ::DataObj*	OnCheckPrimaryKeyCallBack	(SqlRecord* pRecord, int nRow);
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void				OnUnmanagedRecordDisposing			(System::Object^ sender, System::EventArgs^ args);
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		::DataObj*			OnCheckPrimaryKey			(SqlRecord* pRecord, int nRow);	
		virtual void		Init () { }
	};

	/// <summary>
	/// Class MDBTMaster
	/// </summary>
	//===================================================================================
	[ExcludeFromIntellisense]
	public ref class MDBTMaster : public MDBTObject, IDocumentMasterDataManager
	{
	private:
		System::String^		browserQuery;
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="dbtPtr">is the c++ handle of the master which the control refers to</param>
		MDBTMaster  (System::IntPtr	dbtPtr);
		
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="tableName">the name of the table which the slave will refers to</param>
		/// <param name="dbtName">the name of the dbt</param>
		/// <param name="document">the main document manager</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an original taskbuilder object</param>
		MDBTMaster (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MDBTMaster ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MDBTMaster ();
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual MDBTObject^ CreateAndAttach(System::IntPtr dbtPtr) override;
		
	public:
		/// <summary>
		/// Get the Data relation for the dbt
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property DataRelationType	Relation	{ virtual DataRelationType get () override; } 
		
		/// <summary>
		/// Gets or the sets the browser query for the dbt
		/// </summary>
		[System::ComponentModel::EditorAttribute(QueryUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
		property System::String^		BrowserQuery		{ virtual System::String^ get (); virtual void set (System::String^ value); }
		
		/// <summary>
		/// Event raised during the preparation of the primary key
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(PrimaryKeyPrepared, EasyBuilderEventArgs, "DataCategory", EBCategories, "Occurs when dbt prepares primary key. It is TaskBuilderNet C++ framework \"OnPreparePrimaryKey\" method");

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void	ReceivePreparePrimaryKey	(int nRow, System::IntPtr pRecordPtr) override;
		/// <summary>
		/// Gets or the sets the filter query for the object
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^		FilterQuery		{ virtual System::String^ get () override { return __super::FilterQuery; } virtual void set (System::String^ value) override { __super::FilterQuery = value; }}
	
		virtual void OnPrepareFindQuery(MSqlTable^ mTable) {}
		virtual void OnPrepareForXImportExport(MSqlTable^ mTable) {}

	internal:
		bool	Open();
		virtual void AssignInternalState(MGenericDataManager^ source) override;
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void DefineQuery (MSqlTable^ mSqlTable) override;
	
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void	PrepareQuery	(MSqlTable^ mSqlTable) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void	PrepareBrowser	(MSqlTable^ mSqlTable);

		/// <summary>
		/// override, tells if msqlrecord needs serialization (it consinder also dbt )
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool IsToDelete() override;
	};

	/// <summary>
	/// Root class for all Dbt classes
	/// </summary>
	//===================================================================================
	[ExcludeFromIntellisense]
	public ref class MDBTSlave : public MDBTObject, IDocumentSlaveDataManager
	{
	private:
		System::Collections::Generic::List<MForeignKeyField^>^	masterForeignKey;
		IDocumentMasterDataManager^	master;
	internal:
		virtual void AssignInternalState(MGenericDataManager^ source) override;
	public:
		/// <summary>
		/// Get the Data relation for the dbt
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property DataRelationType Relation { virtual DataRelationType get () override; } 

		/// <summary>
		/// Gets or sets the MasterForeignKey relation for the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Collections::Generic::List<MForeignKeyField^>^ MasterForeignKey { virtual System::Collections::Generic::List<MForeignKeyField^>^ get (); void set (System::Collections::Generic::List<MForeignKeyField^>^ fields); } 
		
		/// <summary>
		/// Gets or sets the maximum visible (preloaded) number of rows of the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int PreloadStep { int get (); void set (int value); }
		
		/// <summary>
		/// True if the dbt can be empty
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool AllowEmpty { bool get (); void set (bool value); }

		/// <summary>
		/// gets or sets the dbt onlydelete attribute
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool OnlyDeleteAction { bool get (); void set (bool value); }

		/// <summary>
		/// gets or sets the dbt read behaviour
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property DelayReadType ReadBehaviour { DelayReadType get (); void set (DelayReadType value); }

		/// <summary>
		/// Gets or sets the DBT master 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDocumentMasterDataManager^ Master	{ virtual IDocumentMasterDataManager^ get (); virtual void set (IDocumentMasterDataManager^ master); } 

		/// <summary>
		/// Event raised during the preparation of the primary key
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(PrimaryKeyPrepared, EasyBuilderEventArgs, "DataCategory", EBCategories, "Occurs when dbt prepares primary key. It is TaskBuilderNet C++ framework \"OnPreparePrimaryKey\" method");
	
	public:
		/// <summary>
		/// Initializes a new instance of MDBTSlave
		/// </summary>
		MDBTSlave ();

		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="tableName">the name of the table which the slave will refers to</param>
		/// <param name="dbtName">the name of the dbt</param>
		/// <param name="document">the main document manager</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an original taskbuilder object</param>
		MDBTSlave (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind);
	
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="dbtPtr">is the c++ handle of the slave which the control refers to</param>
		MDBTSlave (System::IntPtr dbtPtr);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MDBTSlave ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MDBTSlave ();

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual MDBTObject^ CreateAndAttach(System::IntPtr dbtPtr) override;
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void DefineQuery (MSqlTable^ mSqlTable) override;
	
		/// <summary>
		/// Allows the user to override and change the PrepareQuery of the dbt
		/// </summary>
		/// <param name="mSqlTable"></param>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void PrepareQuery (MSqlTable^ mSqlTable) override;
	
		/// <summary>
		/// Allows the user to add foreing key to the dbt master 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void AddMasterForeignKey(MDataObj^ dataObj, System::String^ masterFieldName);
	
		/// <summary>
		/// Allows the user to add foreing key to the dbt master 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void AddMasterForeignKey(MDataObj^ dataObj, MDataObj^ masterdataObj);

		/// <summary>
		///
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void ReceivePreparePrimaryKey (int nRow, System::IntPtr pRecordPtr) override;

		/// <summary>
		/// returns true if the desired field name is foreign key to the dbt
		/// </summary>
		/// <param name="fieldName">the desired field to check</param>
		[System::ComponentModel::Browsable(false)]
		bool IsInForeignKey	(System::String^ fieldName);
		/// <summary>
		/// Gets or the sets the filter query for the object
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^		FilterQuery		{ virtual System::String^ get () override { return __super::FilterQuery; } virtual void set (System::String^ value) override { __super::FilterQuery = value; }}
			
		/// <summary>
		/// Gets a collection of non foreign key fields
		/// </summary>
		[ExcludeFromIntellisense]
		System::Collections::Generic::IList<IRecordField^>^ GetNonForeignKeyFields();


		virtual bool IsEmptyData() {return true;}

	internal:
		void PerformDefaultPreparePrimaryKey (MSqlRecord^ record);
		[ExcludeFromIntellisense]
		bool ContainsMasterForeignKey(System::String^ fieldName);

	protected:
		/// <summary>
		/// Allows the user to add foreing key to the dbt master 
		/// </summary>
		[ExcludeFromIntellisense]
		void AddMasterForeignKey(IRecordField^ field, IRecordField^ masterField);
		virtual void AddMasterForeignKey(CString sPrimary, CString sForeign);
	public protected:
		[ExcludeFromIntellisense]
		virtual void InitializeForeignKeys();
	};

	//================================================
	public ref class MDBTSlaveBuffered : public MDBTSlave, IDocumentMasterDataManager, IDocumentSlaveBufferedDataManager
	{
	private:
		System::Delegate^	isDuplicateKeyCallBack;
		System::Runtime::InteropServices::GCHandle	isDuplicateKeyHandle;
		System::Collections::Generic::List<IDocumentSlaveDataManager^>^ prototypes;
		System::Collections::Generic::List<MDBTSlave^>^ slaves;
	protected:
		System::Collections::IList^	rows;
	internal:
		virtual void AssignInternalState(MGenericDataManager^ source) override;
	
	public:
		/// <summary>
		/// Get the Data relation for the dbt
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property DataRelationType Relation { virtual DataRelationType get () override; } 

		/// <summary>
		/// Gets or sets the modified property of the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool Modified { virtual bool get (); virtual void set(bool value); } 

		/// <summary>
		/// Gets or sets the readonly property of the dbt
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool ReadOnly { bool get (); void set (bool value); }

		/// <summary>
		/// Gets the DBT slave (if any)
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property System::Collections::Generic::List<IDocumentSlaveDataManager^>^ SlavePrototypes { System::Collections::Generic::List<IDocumentSlaveDataManager^>^ get () { return prototypes; } }


		/// <summary>
		///  Gets or sets if the dbt is checking for duplicate keys
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool CheckDuplicateKey { bool get (); void set (bool value); }

		/// <summary>
		/// Get the row list ofthe dbt
		/// </summary>	
		property System::Collections::IList^ Rows { virtual System::Collections::IList^ get (); } 

		/// <summary>
		/// Gets the number of row of the dbt
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int RowsCount { int get (); }
		/// <summary>
		/// Gets or the sets the filter query for the object
		/// </summary>
		[System::ComponentModel::Browsable(true)]
		property System::String^		FilterQuery		{ virtual System::String^ get () override { return __super::FilterQuery; } virtual void set (System::String^ value) override { __super::FilterQuery = value; }}
		
		/// <summary>
		/// Gets the number of rows of the dbt old record array
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int OldRowsCount { int get (); }
	
		/// <summary>
		/// Gets/Sets the current row index of the DBT object+
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int CurrentRow	{ int get (); void set (int value); }

		/// <summary>
		/// Internal use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::Object^		BindableDataSource { virtual System::Object^ get () override { return Rows; } } 
			
		/// <summary>
		/// Event raised during the preparation of the primary key
		/// </summary>
		GENERIC_HANDLER_EVENT(PrimaryKeyPrepared, RowEventArgs, "DataCategory", EBCategories, "Occurs when dbt prepares primary key. It is TaskBuilderNet C++ framework \"OnPreparePrimaryKey\" method");

		/// <summary>
		/// Event raised when dbt ReadOnly property changes
		/// </summary>
		GENERIC_HANDLER_EVENT(ReadOnlyChanged, EasyBuilderEventArgs, "DataCategory", EBCategories, "Occurs when dbt readonly property value changes");

		MDBTSlave^		GetCurrentSlave();

		MDBTSlave^		GetCurrentSlave(System::String^ name);
		MDBTSlave^		GetDBTSlave(System::String^ name, int idx);
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="dbtPtr">is the c++ handle of the slavebuffered which the control refers to</param>
		MDBTSlaveBuffered	(System::IntPtr	dbtPtr);
	
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="tableName">the name of the table which the slavebuffered will refers to</param>
		/// <param name="dbtName">the name of the dbt</param>
		/// <param name="document">the main document manager</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an original taskbuilder object</param>
		MDBTSlaveBuffered (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MDBTSlaveBuffered ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MDBTSlaveBuffered ();
	
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual MDBTObject^ CreateAndAttach(System::IntPtr dbtPtr) override;
		/// <summary>
		/// Returns the current MSqlRecord record of the dbt
		/// </summary>
		[ExcludeFromIntellisense]
		MSqlRecord^ GetCurrentRecord ();
	
		// metodi override-ati dalle classi figlie: non possono 
		// essere trasformati in proprietà perchè così le classi figlie 
		// possono avere le loro proprietà ben tipizzate
		
		/// <summary>
		/// Adds a record to the dbt at the desired position
		/// </summary>
		/// <param name="nRow">desired position for the new record</param>
		MSqlRecord^ InsertRecord (int nRow);
		
		/// <summary>
		/// Returns the current record of the dbt
		/// </summary>
		/// <param name="nRow">position of the desired record</param>
		MSqlRecord^ GetRecord (int nRow);
		
		/// <summary>
		/// Returns the previous record of the dbt at the specified position
		/// </summary>
		/// <param name="nRow">position of the desired record</param>
		MSqlRecord^ GetOldRecord (int nRow);

	internal:
		virtual MSqlRecord^	GetRecord 	(SqlRecord* pRecord) override;
		MDBTObject^ GetDBT(System::String^ name);
		MDBTObject^ GetDBT(INameSpace^ nameSpace);
		MDBTObject^	GetDBT (System::IntPtr dbtPtr);
		
		void	OnRemovingRecord(SqlRecord* pRecord, int nRow);
		void	OnRecordAdded(SqlRecord* pRecord, int nRow);

	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void AttachDefaultEvents() override;

	public:

		/// <summary>
		/// Adds a record to the dbt
		/// </summary>
		MSqlRecord^ AddRecord ();
	
		/// <summary>
		/// Returns true if the current record is valid
		/// </summary>
		bool IsValid ();
		
		/// <summary>
		/// Allows the user to delete the desired row from the dbt
		/// </summary>
		/// <param name="nRow">the row to delete</param>
		bool DeleteRow (int nRow);

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceivePrepareRow	(int nRow, System::IntPtr pRecordPtr);
	
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		bool ReceiveBeforeAddRow (int nRow);

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceiveAfterAddRow	(int nRow, System::IntPtr recordPtr);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		bool ReceiveBeforeInsertRow(int nRow);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceiveAfterInsertRow	(int nRow, System::IntPtr recordPtr);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		bool ReceiveBeforeDeleteRow(int nRow, System::IntPtr recordPtr);
				
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceiveAfterDeleteRow	(int nRow);/// <summary>

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceiveSetCurrentRow	(int nRow);/// <summary>


		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void SetCurrentRowByRecord (MSqlRecord^ record);/// <summary>

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void ReceivePreparePrimaryKey (int nRow, System::IntPtr pRecordPtr) override;

		/// <summary>
		/// Remove all records in DBT
		/// </summary>
		virtual void Clear ();

		/// <summary>
		/// Gets from all DBT records the max value contained into the field related to mDataObj parameter
		/// </summary>
		Object^ GetMaxValueOf (MDataObj^ mDataObj);

		/// <summary>
		/// Gets from all DBT records the min value contained into the field related to mDataObj parameter
		/// </summary>
		Object^ GetMinValueOf (MDataObj^ mDataObj);

		/// <summary>
		/// Gets from all DBT records the total value contained into the field related to mDataObj parameter
		/// </summary>
		Object^ GetSumValueOf (MDataObj^ mDataObj);

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		void ReceivePrepareAuxColumns (int nRow, System::IntPtr pRecordPtr);

		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(RowPrepared, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt prepares row. It is TaskBuilderNet C++ framework \"OnPrepareRow\" method");
		
		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(AddingRow, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt adds a new row. It is TaskBuilderNet C++ framework \"OnBeforeAddRow\" method");

		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(RowAdded, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt adds a new row. It is TaskBuilderNet C++ framework \"OnAfterAddRow\" method");

		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(InsertingRow, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt inserts a new row. It is TaskBuilderNet C++ framework \"OnBeforeInsertRow\" method");
	
		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(RowInserted, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt inserts a new row. It is TaskBuilderNet C++ framework \"OnAfterInsertRow\" method");
		
		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(DeletingRow, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt deletes a row. It is TaskBuilderNet C++ framework \"OnBeforeDeleteRow\" method");
		
		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(RowDeleted, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt deletes a row. It is TaskBuilderNet C++ framework \"OnAfterDeleteRow\" method");

		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(CurrentRowChanged, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt changes row. It is TaskBuilderNet C++ framework \"OnSetCurrentRow\" method");

		/// <summary>
		/// Internal Use
		/// </summary>
		//[LocalizedCategory("DBTRowCategory", EBCategories::typeid)]
		GENERIC_HANDLER_EVENT(AuxColumnsPrepared, RowEventArgs, "DBTRowCategory", EBCategories, "Occurs when dbt prepares aux data for row. It is TaskBuilderNet C++ framework \"OnPrepareAuxColumns\" method");


		/// <summary>
		/// Internal Use
		/// </summary>
		void AttachSlave(IDocumentSlaveDataManager^ dbtSlave);

		virtual void SetCurrentRowForValueChanged(int nRow);

		/// <summary>
		/// Sort
		/// </summary>
		[ExcludeFromIntellisense]
		void Sort(System::String^ orderBy);

		/// <summary>
		/// Get the index of the record
		/// </summary>
		[ExcludeFromIntellisense]
		int GetRecordIndex(MSqlRecord^ record);

		/// <summary>
		/// Load next rows
		/// </summary>
		[ExcludeFromIntellisense]
		bool MDBTSlaveBuffered::LoadMoreRows(int preloadStep);

		virtual void OnPrepareOldAuxColumns(SqlRecord* pRec) {  }
		
		virtual ::DataObj* GetDuplicateKeyPos(SqlRecord* pRec) { return OnDefaultIsDuplicateKey(pRec, ((DBTSlaveBuffered*) GetDBTObject())->GetCurrentRowIdx()); }
		virtual System::String^ GetDuplicateKeyMsg(SqlRecord* pRec) { return _T(""); }

			/// <summary>
		/// Find record by column name
		/// </summary>
		MSqlRecord^ FindRecord (System::String^ columnName, MDataObj^ value, int startPos /*= 0*/);
		
		/// <summary>
		/// Find record by column names
		/// </summary>
		MSqlRecord^ FindRecord (array<System::String^>^ columnNames, array<MDataObj^>^ values, int startPos /*= 0*/);

	internal:
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		delegate ::DataObj*	IsDuplicateKeyCallBack	(SqlRecord* pRecord, int nRow);
		
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		::DataObj*			OnDefaultIsDuplicateKey	(SqlRecord* pRecord, int nRow);

	private:
		bool	IsInForeignKey(System::String^ fieldName);
		System::Object^ GetValueOf(MDataObj^ dataObj, int valueType); 
	protected:
		void AddMasterForeignKey(CString sPrimary, CString sForeign) override;
	};
}
}
}

