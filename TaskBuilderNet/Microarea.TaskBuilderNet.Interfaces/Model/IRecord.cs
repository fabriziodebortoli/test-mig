using System;

using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces.Model
{
	public enum DataModelEntityType			{ Table, View, StoreProcedure, Virtual };
	public enum DataModelEntityFieldType	{ Column, Variable, Parameter};

	//=============================================================================
	public interface IRecordField
	{
		string						Name			{ get; }
		string						DefaultValue	{ get; }
		string						QualifiedName	{ get; }
		IDataObj					DataObj			{ get; }
		IDataType					DataObjType		{ get; }
		int							Length			{ get; }
		DataModelEntityFieldType	Type			{ get; }
		object						Value			{ get; set; }
		bool						IsSegmentKey	{ get; }
		int							CreationRelease { get; }
		IRecord						Record			{ get; }
		bool IsCompatibleWith (IRecordField field);
	}

	//=============================================================================
	public interface IRecord
	{
		INameSpace NameSpace				{ get; }
		
		DataModelEntityType RecordType		{ get; }
		IList				Fields			{ get; }
		bool				IsValid			{ get; }
		bool				IsRegistered	{ get; }
		string				Name			{ get; }
		IList				PrimaryKeyFields{ get; }
		int					CreationRelease { get; }
				
		IRecordField	Add		(string name, DataModelEntityFieldType type, IDataType dataType, string localizableName, int length);
		IRecordField	GetField(string name);
		IRecordField	GetField(IDataObj dataObj);
		object			GetValue(string fieldName);
		IDataObj		GetData	(string fieldName);

	}

	//=============================================================================
	public interface IEBDataReader : IDisposable
	{
		bool Read		();
		bool IsEOF		();
		bool NextResult	();

		object		GetValue(string fieldName);
		IDataObj	GetData	(string fieldName);
		void		Close	();
		object		this[string fieldName] { get; }
	}
}
