#pragma once

class SqlRecord;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	/// <summary>
	/// Class that contains information on all register tables
	/// </summary>
	//================================================
	public ref class MSqlCatalog
	{	
	private:
		SqlConnection* m_pConnection;

	public:
		/// <summary>
		/// Returns a list of all items contained in the sql catalog
		/// </summary>
		property System::Collections::Generic::IList<IRecord^>^ Items { virtual System::Collections::Generic::IList<IRecord^>^ get (); }
		
		/// <summary>
		/// Returns a list of all tables contained in the sql catalog
		/// </summary>
		property System::Collections::Generic::IList<IRecord^>^ Tables { virtual System::Collections::Generic::IList<IRecord^>^ get (); }
		
		/// <summary>
		/// Returns a list of all views contained in the sql catalog
		/// </summary>
		property System::Collections::Generic::IList<IRecord^>^ Views { virtual System::Collections::Generic::IList<IRecord^>^ get (); }
		
		/// <summary>
		/// Returns a list of all stored procedures contained in the sql catalog
		/// </summary>
		property System::Collections::Generic::IList<IRecord^>^ StoreProcedures { virtual System::Collections::Generic::IList<IRecord^>^ get (); }
		
		/// <summary>
		/// Returns a list of all tables and views contained in the sql catalog
		/// </summary>
		property System::Collections::Generic::IList<IRecord^>^ TablesAndViews { virtual System::Collections::Generic::IList<IRecord^>^ get (); }
		
		/// <summary>
		/// Returns the currentdatabase type (SQLSERVER, ORACLE, UNKNOWN)
		/// </summary>
		property Microarea::TaskBuilderNet::Interfaces::DBMSType DbmsType { Microarea::TaskBuilderNet::Interfaces::DBMSType get (); }
			
	public:
		/// <summary>
		/// Initializes a new instance of MSqlCatalog
		/// </summary>
		MSqlCatalog ();
		
		/// <summary>
		/// Internal Use: Initializes a new instance of MSqlCatalog
		/// </summary>
		MSqlCatalog (System::IntPtr connectionPtr);

	public:
		/// <summary>
		/// Gets the record associated to the given table name
		/// </summary>
		/// <param name="tableName">the table name to get</param>
		IRecord^ GetTable (System::String^ tableName);
		
		/// <summary>
		/// Adds a table to the catalog
		/// </summary>
		/// <param name="table">the table to add to the catalog</param>
		/// <param name="bVirtual">true if the table is not really in the database but only in memory</param>
		void AddTable (IRecord^ table, bool bVirtual, bool masterTable);
		
		/// <summary>
		/// Adds a field to the desired table
		/// </summary>
		void AddField (IRecord^ table, IRecordField^ field, INameSpace^ ownerModule, bool bVirtual);
		
		/// <summary>
		/// Removes all object created into the catalog associated to the specified release number
		/// </summary>
		/// <param name="nRelease">the release number to delete</param>
		/// <param name="moduleNamespace">the module owner of the changes</param>
		void RemoveObjectsOfRelease(int nRelease, INameSpace^ moduleNamespace);
		
		/// <summary>
		/// Returns true if master and slave are compatible for a 1 - n or 1 - 1 relation
		/// </summary>
		bool HaveMasterSlaveRelationship (System::Collections::IList^ masterFields, System::Collections::IList^ slaveFields);

	private:
		System::Collections::Generic::IList<IRecord^>^ GetItems (bool tables, bool views, bool storeProcedures);
	};

	/// <summary>
	/// Class that describe the foreign key field segment
	/// </summary>
	//================================================
	public ref class MForeignKeyField
	{	
	private:
		System::String^ pkFieldName;
		System::String^ fkFieldName;
		System::String^ pkTableName;
		System::String^ fkTableName;
	public:
		MForeignKeyField (System::String^ pkFieldName, System::String^ fkFieldName, System::String^ pkTableName, System::String^ fkTableName);
	
	public:
		/// <summary>
		/// Gets or Sets the primary key for the field
		/// </summary>
		property System::String^ PrimaryKey	{ System::String^ get (); void set (System::String^ field); }
		
		/// <summary>
		/// Gets or Sets the foreign key for the field
		/// </summary>
		property System::String^ ForeignKey	{ System::String^ get (); void set (System::String^ field); }
		
		/// <summary>
		/// Gets the fully qualified primary key string
		/// </summary>
		property System::String^ QualifiedPrimaryKey { System::String^ get () {return pkTableName + "." + pkFieldName;} }
		
		/// <summary>
		/// Gets the fully qualified foreign key string
		/// </summary>
		property System::String^ QualifiedForeignKey { System::String^ get () {return fkTableName + "." + fkFieldName;} }
	public:
		/// <summary>
		/// Returns true if the specified field name is a foreign key
		/// </summary>
		/// <param name="fieldName">the fieldName to check</param>
		bool IsInForeignKey (System::String^ fieldName);
	};
}
}
}
