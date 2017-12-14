#pragma once

class SqlRecord;
#include <TbOleDb\Sqltable.h>
#include "MDataObj.h"
#include "MSqlRecord.h"

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	/// <summary>
	/// Represents type of Top keyword in Select clause
	/// </summary>
	public enum class	SelectTopType {Top = SqlTable::TOP, TopPercent = SqlTable::TOP_PERCENT, TopWithTies = SqlTable::TOP_WITH_TIES};
	/// <summary>
	/// Represents the type of Database
	/// </summary>
	public enum class	DBType { SqlServer = DBMS_SQLSERVER, Oracle = DBMS_ORACLE, Unknown = DBMS_UNKNOWN};
	
	//================================================
	class CRecordDataFinder
	{
	public:
		CRecordDataFinder ();

		virtual SqlRecordItem* GetRecordItem (SqlRecord* pRecord, ::DataObj* dataObj);
	};	

	/// <summary>
	/// Select object of MSqlTable class
	/// </summary>
	//================================================
	public ref class SelectStatement
	{
	private:
		CRecordDataFinder* dataFinder;

	protected:
		SqlTable*	m_pSqlTable;
	
	internal:
		SelectStatement (SqlTable* pSqlTable){ m_pSqlTable = pSqlTable; }
	
	public:
		/// <summary>
		/// Represents the SELECT clause string
		/// </summary>
		property System::String^	PlainSelect		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strSelect); } void set (System::String^ selectSql) {m_pSqlTable->m_strSelect = CString(selectSql);} } 

	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ CRecordDataFinder* get () { return dataFinder; } void set (CRecordDataFinder* value) {  dataFinder = value; } } 

	public:
		/// <summary>
		/// Selects the specified columns in the MSqlRecord
		/// </summary>
		void	AddColumn(MDataObj^ dataObj);
		/// <summary>
		/// Selects the specified columns in the MSqlRecord
		/// </summary>
		void	AddColumn(MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Selects the specified columns in the MSqlRecord
		/// </summary>
		void	AddColumn(System::String^ columnName, MDataObj^ dataObj, int allocSize, bool autoIncrement);
		/// <summary>
		/// Selects the specified columns in the MSqlRecord
		/// </summary>
		void	AddColumn(System::String^ columnName, MDataObj^ dataObj, int allocSize);
		/// <summary>
		/// Selects the specified columns in the MSqlRecord
		/// </summary>
		void	AddColumn(System::String^ columnName, MDataObj^ dataObj);
		/// <summary>
		/// Allows to write select query with functions (E.g. SELECT COUNT(*) or SELECT a+b))
		/// </summary>
		void	AddFunction (System::String^ function, MDataObj^ resDataObj);
		/// <summary>
		/// Allows to write select query with functions (E.g. SELECT COUNT(*) or SELECT a+b))
		/// </summary>
		void	AddFunction (System::String^ function, MDataObj^ resDataObj, int allocSize);
		/// <summary>
		/// Allows to write select query with functions (E.g. SELECT SUM(a))
		/// </summary>
		void	AddFunction (System::String^ function, MDataObj^ paramDataObj, MDataObj^ resDataObj);
		/// <summary>
		/// Allows to write select query with functions (E.g. SELECT SUM(a))
		/// </summary>
		void	AddFunction (System::String^ function, MDataObj^ ParamDataObj, MDataObj^ resDataObj, int allocSize);
		/// <summary>
		/// Allows to write select query with functions
		/// </summary>
		void	AddFunction (System::String^ function, MDataObj^ ParamDataObj, MDataObj^ resDataObj, int allocSize, MSqlRecord^ record);
		/// <summary>
		/// Selects all columns in the MSqlRecord
		/// </summary>
		void	All();
		/// <summary>
		/// Selects all columns in the specified MSqlRecord
		/// </summary>
		void	All(MSqlRecord^ record);
		/// <summary>
		/// Selects all columns from all tables
		/// </summary>
		void	FromAllTables();
		/// <summary>
		/// Add TOP, TOP PERCENT or TOP WITH TIES keyword in the SELECT clause
		/// </summary>
		void	Top(SelectTopType aType, int topValue);
		/// <summary>
		/// Add DISTINCT keyword in the SELECT clause
		/// </summary>
		void	Distinct();
	};
	

	/// <summary>
	/// From object of MSqlTable class
	/// </summary>
	//================================================
	public ref class FromStatement
	{
	private:
		SqlTable*				m_pSqlTable;

	internal:
		FromStatement (SqlTable*	pSqlTable){ m_pSqlTable = pSqlTable; }

	public:
		/// <summary>
		/// Represents the FROM clause string
		/// </summary>
		property System::String^	PlainFrom		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strFrom); } void set (System::String^ fromText) {m_pSqlTable->m_strFrom = CString(fromText);} } 

	public:
		/// <summary>
		/// Adds other tables to the FROM clause string, when more tables are involved
		/// </summary>
		void	AddTable			(MSqlRecord^ record);
		/// <summary>
		/// Adds other tables to the FROM clause string, when more tables are involved
		/// </summary>
		void	AddTable			(MSqlRecord^ record, System::String^ alias);
		/// <summary>
		/// Adds other tables to the FROM clause string, when more tables are involved
		/// </summary>
		void	AddTable			(System::String^ tableName, System::String^ alias);
		/// <summary>
		/// Adds other tables to the FROM clause string, when more tables are involved
		/// </summary>
		void	AddTable			(System::String^ tableName, System::String^ alias, MSqlRecord^ record);
	};

	/// <summary>
	/// Where object of MSqlTable class
	/// </summary>
	//================================================
	public ref class WhereStatement
	{
	private:
		SqlTable*				m_pSqlTable;
		CRecordDataFinder*		dataFinder;

	internal:
		WhereStatement (SqlTable*	pSqlTable){ m_pSqlTable = pSqlTable; }

	public:
		/// <summary>
		/// Represents the WHERE clause string
		/// </summary>
		property System::String^	PlainWhere		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strFilter); } void set (System::String^ whereText) {m_pSqlTable->SetFilter(CString(whereText));} } 
	
	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ CRecordDataFinder* get () { return dataFinder; } void set (CRecordDataFinder* value) {  dataFinder = value; } } 

	public:
		/// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
		void   AddColumn     (MDataObj^ dataObj);
		/// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
	    void   AddColumn     (MDataObj^ dataObj, System::String^ strOperator);
		/// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
		void   AddColumn     (MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
		void   AddColumn     (MSqlRecord^ record, MDataObj^ dataObj, System::String^ strOperator);
        /// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
		void   AddColumn     (System::String^ columnName);
		/// <summary>
		/// Adds a filter column in the WHERE clause of query string
		/// </summary>
		void   AddColumn     (System::String^ columnName, System::String^ strOperator);
		/// <summary>
		/// Adds a parameter in the WHERE clause of query string
		/// </summary>
		void   AddParameter		(System::String^ paramName, MDataObj^ dataObj);
		/// <summary>
		/// Adds a parameter in the WHERE clause of query string
		/// </summary>
		void   AddParameter		(System::String^ paramName, MDataObj^ dataObj, System::String^ columnName);	
		/// <summary>
		/// Adds to query string the comparison between two columns from two different tables
		/// </summary>
		void   AddCompareColumn		(MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj);
		/// <summary>
		/// Adds to query string the comparison between two columns from two different tables
		/// </summary>
		void   AddCompareColumn		(MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj, System::String^ strOperator);
		/// <summary>
		/// Adds to query string the comparison between two columns from two different tables
		/// </summary>
		void   AddCompareColumn		(MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2);
		/// <summary>
		/// Adds to query string the comparison between two columns from two different tables
		/// </summary>
		void   AddCompareColumn		(MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2, System::String^ strOperator);
		/// <summary>
		/// Adds to query string the comparison of a column between two values
		/// </summary>
		void   AddBetweenColumn		(MDataObj^ dataObj);
		/// <summary>
		/// Adds to query string the comparison of a column between two values
		/// </summary>
		void   AddBetweenColumn		(MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Adds to query string the comparison of a column between two values
		/// </summary>
		void   AddBetweenColumn		(System::String^ columnName);
		/// <summary>
		/// Set value of parameter in the WHERE clause of query string
		/// </summary>
		void   Parameter  (System::String^ paramName, MDataObj^ value);
		/// <summary>
		/// Set value of like parameter in the WHERE clause of query string
		/// </summary>
		void	ParameterLike (System::String^ paramName, MDataObj^ dataObj);
		/// <summary>
		/// Adds a "filter like" column in the WHERE clause of query string
		/// </summary>
		void	AddColumnLike (MDataObj^ dataObj);
		/// <summary>
		/// Adds a "filter like" column in the WHERE clause of query string
		/// </summary>
		void	AddColumnLike (System::String^ columnName);
	};
	
	/// <summary>
	/// OrderBy object of MSqlTable class
	/// </summary>
	//================================================
	public ref class OrderByStatement
	{
	private:
		SqlTable*			m_pSqlTable;
		CRecordDataFinder*	dataFinder;

	internal:
		OrderByStatement (SqlTable*	pSqlTable){ m_pSqlTable = pSqlTable; }

	public:
		/// <summary>
		/// Represents the ORDER BY clause string
		/// </summary>
		property System::String^	PlainOrderBy		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strSort); } void set (System::String^ orderByText) {m_pSqlTable->m_strSort = CString(orderByText);} } 

	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ CRecordDataFinder* get () { return dataFinder; } void set (CRecordDataFinder* value) {  dataFinder = value; } } 

	public:
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
		void   AddColumn (MDataObj^ dataObj);
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
	    void   AddColumn ( MDataObj^ dataObj, bool descending);
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
		void   AddColumn (System::String^ columnName);
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
		void   AddColumn (System::String^ columnName, bool descending);
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
		void   AddColumn (MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Adds a  column in the ORDER BY clause of query string
		/// </summary>
		void   AddColumn (MSqlRecord^ record, MDataObj^ dataObj, bool descending);		
	};

	/// <summary>
	/// GroupBy object of MSqlTable class
	/// </summary>
	//================================================
	public ref class GroupByStatement
	{
	private:
		SqlTable*			m_pSqlTable;
		CRecordDataFinder*	dataFinder;
	
	internal:
		GroupByStatement (SqlTable*	pSqlTable){ m_pSqlTable = pSqlTable; }

	public:
		/// <summary>
		/// Represents the GROUP BY clause string
		/// </summary>
		property System::String^	PlainGroupBy		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strGroupBy); } void set (System::String^ groupByText) {m_pSqlTable->m_strGroupBy = CString(groupByText);} } 

	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ CRecordDataFinder* get () { return dataFinder; } void set (CRecordDataFinder* value) {  dataFinder = value; } } 

	public:
		/// <summary>
		/// Adds a  column in the GROUP BY clause of query string
		/// </summary>
		void   AddColumn    (MDataObj^ dataObj);
		/// <summary>
		/// Adds a  column in the GROUP BY clause of query string
		/// </summary>
		void   AddColumn    (System::String^ columnName);
		/// <summary>
		/// Adds a  column in the GROUP BY clause of query string
		/// </summary>
		void   AddColumn    (MSqlRecord^ record, MDataObj^ dataObj);	
	};
	

	/// <summary>
	/// Having object of MSqlTable class
	/// </summary>
	//================================================
	public ref class HavingStatement
	{
	private:
		SqlTable*			m_pSqlTable;
		CRecordDataFinder*	dataFinder;

	internal:
		HavingStatement (SqlTable*	pSqlTable){ m_pSqlTable = pSqlTable; }

	public:
		/// <summary>
		/// Represents the HAVING clause string
		/// </summary>
		property System::String^	PlainHaving		{ System::String^ get () { return gcnew System::String(m_pSqlTable->m_strHaving); } void set (System::String^ havingText) { m_pSqlTable->SetHavingFilter(CString(havingText));} } 
	
	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ CRecordDataFinder* get () { return dataFinder; } void set (CRecordDataFinder* value) {  dataFinder = value; } } 

	public:
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(MDataObj^ dataObj);
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(MDataObj^ dataObj, System::String^ strOperator);
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(MSqlRecord^ record, MDataObj^ dataObj, System::String^ strOperator);
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(System::String^ columnName);
		/// <summary>
		/// Adds a column in the HAVING clause of query string
		/// </summary>
		void	AddColumn				(System::String^ columnName, System::String^ strOperator);
		/// <summary>
		/// Adds to Having clause string the comparison between two values
		/// </summary>
		void	AddBetweenColumn		(MDataObj^ dataObj);
		/// <summary>
		/// Adds to Having clause string the comparison between two values
		/// </summary>
		void	AddBetweenColumn		(MSqlRecord^ record, MDataObj^ dataObj);
		/// <summary>
		/// Adds to Having clause string the comparison between two values
		/// </summary>
		void	AddBetweenColumn		(System::String^ columnName);
		/// <summary>
		/// Adds to HAVING clause string the LIKE operator
		/// </summary>
		void	AddColumnLike			(MDataObj^ dataObj);
		/// <summary>
		/// Adds to HAVING clause string the LIKE operator
		/// </summary>
		void	AddColumnLike			(System::String^ columnName);
		/// <summary>
		/// Adds to HAVING clause string the comparison between two columns from two different tables
		/// </summary>
		void	AddCompareColumn		(MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj);
		/// <summary>
		/// Adds to HAVING clause string the comparison between two columns from two different tables
		/// </summary>
		void	AddCompareColumn		(MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj, System::String^ strOperator);
		/// <summary>
		/// Adds to HAVING clause string the comparison between two columns from two different tables
		/// </summary>
		void	AddCompareColumn		(MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2);
		/// <summary>
		/// Adds to HAVING clause string the comparison between two columns from two different tables
		/// </summary>
		void	AddCompareColumn		(MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2, System::String^ strOperator);
	};
	/// <summary>
	/// Wrapper class to taskbuilder sql table class
	/// </summary>
	//================================================
	public ref class MSqlTable
	{
	private:
		TDisposablePtr<SqlTable>* m_ppSqlTable;
		IRecord^	record;
		bool		hasCodeBehind;
	
		FromStatement^			from;
		OrderByStatement^		orderBy;
		GroupByStatement^		groupBy;
		HavingStatement^		having;
		SelectStatement^		select;
		WhereStatement^			where;

	public:
		enum class	CursorType {None = E_NO_CURSOR, ForwardOnly = E_FORWARD_ONLY, FastForwardOnly =  E_FAST_FORWARD_ONLY, Keyset = E_KEYSET_CURSOR, Dynamic = E_DYNAMIC_CURSOR, Static = E_STATIC_CURSOR };			
	
		property SelectStatement^	Select		{ SelectStatement^	get () { return select;		} }
		property FromStatement^		From		{ FromStatement^	get () { return from;		} }
		property WhereStatement^	Where		{ WhereStatement^	get () { return where;		} }
		property OrderByStatement^	OrderBy		{ OrderByStatement^ get () { return orderBy;	} } 
		property GroupByStatement^	GroupBy		{ GroupByStatement^ get () { return groupBy;	} }
		property HavingStatement^	Having		{ HavingStatement^	get () { return having;		} }
		/// <summary>
		/// Represents the complete SQL query string
		/// </summary>
		property System::String^			PlainQuery	{ System::String^ get () { return gcnew System::String(GetSqlTable()->m_strSQL); } void set (System::String^ queryText) { GetSqlTable()->m_strSQL = queryText;} } 
		/// <summary>
		/// Gets or sets the IRecord related to the SqlTable
		/// </summary>
		property IRecord^		Record			{ virtual IRecord^ get (); virtual void set (IRecord^ record); } 
		/// <summary>
		/// Gets the type of underlying database
		/// </summary>
		property DBType			DatabaseType 	{ virtual DBType get (){ return (DBType)GetSqlTable()->GetDBMSType();} } 
		property CursorType		Cursor			{ virtual CursorType get (){ return (CursorType)GetSqlTable()->GetCursorType();} } 
		property System::String^		Alias	{ System::String^ get () { return gcnew System::String(GetSqlTable()->GetAlias()); } void set (System::String^ alias) { if (GetSqlTable()) GetSqlTable()->SetAliasName(alias); } } 
	

	public:
		/// <summary>
		/// Initializes a new instance of MSqlTable
		/// </summary>
		MSqlTable (MSqlRecord^ record);

		/// <summary>
		/// Initializes a new instance of MSqlTable
		/// </summary>
		MSqlTable (System::IntPtr sqlTablePtr, IRecord^ record);

		/// <summary>
		/// Initializes a new instance of MSqlTable
		/// </summary>
		MSqlTable (System::IntPtr sqlTablePtr);

		/// <summary>
		/// Initializes a new instance of MSqlTable
		/// </summary>
		MSqlTable (MSqlRecord^ record, System::IntPtr sqlSessionPtr);
		
		/// <summary>
		/// Destructor
		/// </summary>
		~MSqlTable();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MSqlTable();
	
	private:
		void InitStatement ();
	internal:
		/// <summary>
		/// Internal Use
		/// </summary>
		SqlTable* GetSqlTable() { return m_ppSqlTable ? m_ppSqlTable->operator->() : NULL; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property CRecordDataFinder*	DataFinder 	{ void set (CRecordDataFinder* value); } 

	public:
		void Open ( 
					bool updatable,    // é un rowset su cui verranno effettuate operazioni di insert/update/delete
                    bool scrollable,   // é un rowset con un cursore di tipo forward-only
                    bool sensitivity   // é un rowset aggiornato dinamicamente con le modifiche effettuate sul database (in base al tipo poi di cursore richiesto)                                      
                  );
		
		/// <summary>
		/// Open the MSqlTable 
		/// </summary>
		void	Open ();
		/// <summary>
		/// Open the MSqlTable 
		/// </summary>
		void	Open (bool updatable);
		/// <summary>
		/// Open the MSqlTable 
		/// </summary>
		void	Open (bool updatable, bool scrollable);
		/// <summary>
		/// Open the MSqlTable 
		/// </summary>
		void	Open (bool updatable, CursorType cursorType);
        
		/// <summary>
		/// Close the MSqlTable 
		/// </summary>
		void	Close();

		/// <summary>
		/// Moves the cursor to next record
		/// </summary>
		void	NextResult  ();
		/// <summary>
		/// Moves the cursor to previous record
		/// </summary>
		void	PrevResult  ();
		/// <summary>
		/// Moves the cursor to the first record
		/// </summary>
		void	FirstResult ();
		/// <summary>
		/// Moves the cursor to the last record
		/// </summary>
		void	LastResult  ();
		/// <summary>
		/// Executes the query and the first fetch
		/// </summary>
		void	ExecuteQuery();
		/// <summary>
		/// Returns true if the cursor position is before the first record
		/// </summary>
		bool	IsEOF		();
		/// <summary>
		/// Returns true if the cursor position is after the last record
		/// </summary>
		bool	IsBOF		();

		
		void	AddNew	();
		void	Edit	();
		int		Update	();
		int		Update	(MSqlRecord^ oldRecord);
		void	Delete	();
		void	Delete	(MSqlRecord^ oldRecord);

		/// <summary>
		/// Lock the current record
		/// </summary>
		bool	LockCurrent		();
		/// <summary>
		/// Lock the current record
		/// </summary>
		bool	LockCurrent		(bool useMessageBox);
		/// <summary>
		/// Unlock the current record
		/// </summary>
		bool	UnlockCurrent	();
		/// <summary>
		/// Unlock all records
		/// </summary>
		bool	UnlockAll		();

		bool	IsEmpty	();
	};
}
}
}
