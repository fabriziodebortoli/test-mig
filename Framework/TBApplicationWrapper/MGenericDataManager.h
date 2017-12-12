#pragma once
#include "MEasyBuilderContainer.h"

#include "Utility.h"
#include "UITypeEditors.h"

using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
class WClause;
class SqlTableInfoArray;
class SqlTable;
class SymTable;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{	
	/// <summary>
	/// Defines arguments for the query management events
	/// </summary>
	//=========================================================================
	public ref class DataManagerEventArgs : public EasyBuilderEventArgs
	{
		MSqlTable^ table;
	public:

		DataManagerEventArgs(MSqlTable^ table) { this->table = table; }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property MSqlTable^	Table { virtual MSqlTable^ get () { return table; } virtual void set(MSqlTable^ value) { this->table = value;} } 
		
	};
	/// <summary>
	/// A generic class for reading/updating data
	/// </summary>
	//=========================================================================
	[ExcludeFromIntellisense]
	public ref class MGenericDataManager abstract: public MEasyBuilderContainer
	{
	protected:
		System::String^				filterQuery;
		WClause*			whereClause;
		SqlTableInfoArray*	sqlTableArray; 
		
	public:
		/// <summary>
		/// Event raised before execution of the query
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(Querying, EasyBuilderEventArgs, "DataCategory", EBCategories, "Occurs after DefiningQuery and before PreparingQuery.");
		/// <summary>
		/// Event raised after execution of the query
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(Queried, EasyBuilderEventArgs, "DataCategory", EBCategories, "Occurs after query has been performed.");
		
		/// <summary>
		/// Event raised when defining the query structure
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(DefiningQuery, DataManagerEventArgs, "DataCategory", EBCategories, "Allows to define query text. It is TaskBuilder C++ framework OnDefineQuery method.");
		
		/// <summary>
		/// Event raised when assigning query parameter values
		/// </summary>
		//[LocalizedCategory("DataCategory", EBCategories::typeid), ExcludeFromIntellisense]
		GENERIC_HANDLER_EVENT(PreparingQuery, DataManagerEventArgs, "DataCategory", EBCategories, "Allows to prepare query parameters. It is TaskBuilder C++ framework OnPrepareQuery method.");
		
		MGenericDataManager();
		!MGenericDataManager();
		~MGenericDataManager();

		/// <summary>
		/// Gets or the sets the filter query for the data manager
		/// </summary>
		[System::ComponentModel::EditorAttribute(QueryUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), LocalizedCategory("DataCategory", EBCategories::typeid)]
		property System::String^		FilterQuery		{ virtual System::String^ get (); virtual void set (System::String^ value); }


		/// <summary>
		/// Gets the record associated to the object
		/// </summary>
		property IRecord^		Record { virtual IRecord^ get () = 0; } 
	protected:
		virtual SqlTable* GetTable() = 0;
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		WClause* CreateValidWhereClause(SqlTable* pTable, System::String^ whereClause, CString& strError);
	internal:
		/// <summary>
		/// Serve per travasare i dati da un datamanager all'altro, creandone una copia; mi serve per la gestione del dbtslavebuffered slavable, che crea cloni del dbt prototipo per ogni riga del papà
		/// </summary>
		/// <param name="source"></param>
		/// <remarks>
		///
		/// </remarks>
		virtual void AssignInternalState(MGenericDataManager^ source);

	public:
		SymTable* GetSymTable();
		SqlTableInfoArray* GetTableInfoArray();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnQuerying	();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnQueried	();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void PrepareQuery (MSqlTable^ mSqlTable);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void DefineQuery (MSqlTable^ mSqlTable);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add (System::ComponentModel::IComponent^ component, System::String^ name) override;
	};

}}}