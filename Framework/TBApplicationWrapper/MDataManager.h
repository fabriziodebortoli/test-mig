#pragma once

#include <TbOleDb\SqlRec.h>

#include "MSqlRecord.h"
#include "MSqlTable.h"

#include "MGenericDataManager.h"

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace ICSharpCode::NRefactory;
using namespace ICSharpCode::NRefactory::CSharp;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	ref class MDataManager;	

	/// <summary>
	/// Internal use: serializes a generic DataManager
	/// </summary>
	//===================================================================================
	public ref class GenericDataManagerSerializer : EasyBuilderSerializer
	{
	public:
		static System::String^ GetRecordPtrMethodName		= "GetRecordPtr";
		static System::String^ RecordParamName				= "record";
		static System::String^ CastToMyRecordMethodName		= "CastToMyRecord";
		static System::String^ CloneMethodName				= "Clone";
		static System::String^ AddMethodName				= "Add";
		
		void						SerializeRecord				(SyntaxTree^ syntaxTree, TypeDeclaration^ aParentClass, MSqlRecord^ record);
		void						SerializeRecordAccessor		(SyntaxTree^ syntaxTree, TypeDeclaration^ dbtClass, MSqlRecord^ record);
		PropertyDeclaration^		GenerateRecordAccessor		(MSqlRecord^ record, System::String^ invokeMethod, System::String^ propertyName);
		PropertyDeclaration^		GenerateTypedRecordAccessor	(MSqlRecord^ record, System::String^ propertyName);
	};

	/// <summary>
	/// Internal use: serializes a DataManager
	/// </summary>
	//===================================================================================
	public ref class DataManagerSerializer : public GenericDataManagerSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, Object^ current) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ dbt) override;
		/// <remarks />
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }

	protected:
		virtual IList<Statement^>^ SerializeConstruction(IDesignerSerializationManager^ manager, MDataManager^ dataManager, System::String^ varName, System::String^ className);
	};
	
	/// <summary>
	/// Internal use: serializes a HotLink in a module scenario
	/// </summary>
	//===================================================================================
	public ref class DataManagerSerializerForModuleController : DataManagerSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, Object^ current) override;
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
		virtual System::String^ GetOwnerController() override;
	};

	/// <summary>
	/// Internal use: serializes a HotLink in a module scenario
	/// </summary>
	//===================================================================================
	public ref class DataManagerSerializerForSharedDataManagers : DataManagerSerializer
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
		virtual IList<Statement^>^ SerializeConstruction(IDesignerSerializationManager^ manager, MDataManager^ dataManager, System::String^ varName, System::String^ className) override;
				
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;

	};
	
	/// <summary>
	/// A class for reading/updating data
	/// </summary>
	//=========================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(DataManagerSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(DataManagerSerializerForModuleController::typeid, DataManagerSerializer::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(DataManagerSerializerForSharedDataManagers::typeid, DataManagerSerializerForModuleController::typeid)]
	[System::ComponentModel::TypeConverter(System::ComponentModel::TypeConverter::typeid)]
	public ref class MDataManager : public MGenericDataManager, IDataManager
	{
		public:
			enum class ReadResult { None, Found, NotFound, Locked };
		private:
			MSqlTable^					table;
			System::String^				name;
			ReadResult					status;
			SqlRecord*					m_pOldRecord;
			bool						isUpdatable;
		protected:
			virtual SqlTable* GetTable() override { return table->GetSqlTable(); }
		public:
			
			/// <summary>
			/// Gets the record associated to the object
			/// </summary>
			property IRecord^		Record { virtual IRecord^ get () override; } 
			
		public:
			/// <summary>
			/// Initializes a new instance of MDataManager 
			/// </summary>
			MDataManager (System::String^ tableName, System::String^ dataManagerName, EasyBuilderComponent^ document);
			!MDataManager();
			~MDataManager();
			
		public:
		
			/// <summary>
			/// Gets the name for the object
			/// </summary>
			property System::String^		Name	{ virtual System::String^ get () override; } 
			
			/// <summary>
			/// Gets the namespace for the object
			/// </summary>
			property INameSpace^	Namespace	{ virtual INameSpace^ get (); } 
			
			/// <summary>
			/// Gets the table name of the object
			/// </summary>
			property System::String^		TableName { virtual System::String^ get(); }
			/// <summary>
			/// Gets the Data relation for the object
			/// </summary>
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),  ExcludeFromIntellisense]
			property DataRelationType Relation { virtual DataRelationType get () { return DataRelationType::None; } } 
			/// <summary>
			/// Internal Use
			/// </summary>
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
			property System::String^ SerializedType	{ virtual System::String^ get () override; }

			/// <summary>
			/// true if this data manager can update data
			/// </summary>
			property bool			IsUpdatable { virtual  bool get (); virtual void set(bool value); }
			
			/// <summary>
			/// true if this data manager has its own transaction context and immediately commits changes to database when updating
			/// </summary>
			[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
			property bool			AutoCommit { virtual  bool get (); virtual void set(bool value); }
			
			[System::ComponentModel::Browsable(false)]
			property MSqlTable^ Table	{ virtual MSqlTable^ get () { return table; } }
			/// <summary>
			/// Internal use
			/// </summary>
			[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
			property System::Object^		BindableDataSource { virtual System::Object^ get () { return Record; } } 
		
		public:
			virtual System::String^ ToString() override;
			
			/// <summary>
			/// Locks the current record
			/// </summary>
			bool						LockCurrent();
			
			/// <summary>
			/// Unlocks the current record
			/// </summary>
			void						UnlockCurrent	()	{ GetTable()->UnlockCurrent (); }
			
			/// <summary>
			/// Unlocks all records of DataManager
			/// </summary>
			void						UnlockAll		()	{ GetTable()->UnlockAll (); }
			
			/// <summary>
			/// Allow to perform data extraction from table associate to DataManager without locking the record
			/// </summary>
			MDataManager::ReadResult	Read();
			
			/// <summary>
			///  Allow to perform data extraction from table associate to DataManager
			/// </summary>
			/// <param name="lock">Allow to lock the record</param>
			MDataManager::ReadResult	Read(bool lock);
			
			/// <summary>
			/// Allow the insert of a new record
			/// </summary>
			void AddNew();
			
			/// <summary>
			/// Update current record
			/// </summary>
			bool						UpdateCurrent();
			
			/// <summary>
			/// Delete current record
			/// </summary>
			bool						DeleteCurrent();
			
			/// <summary>
			/// Allow to re-execute the query changing parameter values
			/// </summary>
			void						ReExecuteQuery();
			
			/// <summary>
			/// Perform close operation on DataManager (unlock all records, clean the record)
			/// </summary>
			void						Close();

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			System::IntPtr GetRecordPtr ();

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual bool CanChangeProperty	(System::String^ propertyName) override;
			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			virtual void DefineQuery (MSqlTable^ mSqlTable) override;
	};

}}}


