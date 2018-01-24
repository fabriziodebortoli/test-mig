using System.Collections.Generic;

namespace TaskBuilderNetCore.Interfaces
{ 
	///<summary>
	/// Interfaccia IDbObjectInfo
	/// Da essa derivano le generiche ViewInfo e ProcedureInfo
	///</summary>
    //=========================================================================
    public interface IDbObjectInfo
    {
        int Createstep { get; }
        string Name { get; }
        int Release { get; }
        string Namespace { get; }
    }

	///<summary>
	/// Interfaccia ITableInfo
	/// Da essa derivano le generiche TableInfo
	///</summary>
	//=========================================================================
    public interface ITableInfo : IDbObjectInfo
	{
		string Title { get; }//nome localizzato della tabella
	}

	///<summary>
	/// Interfaccia IAddOnDbObjectInfo
	/// Da essa derivano le generiche ExtraAddedColumn
	///</summary>
	//=========================================================================
	public interface IAddOnDbObjectInfo : IDbObjectInfo
	{
		string TableName { get; }
		string TableNamespace { get; }
	}

	//=========================================================================
	public interface IDBObjects
	{
		List<IDbObjectInfo> TableInfoList { get; }
		List<IDbObjectInfo> ViewInfoList { get; }
		List<IDbObjectInfo> ProcedureInfoList { get; }
		List<IAddOnDbObjectInfo> ExtraAddedColsList { get; }

//		ModuleInfo ParentModuleInfo { get; }
	}

	public interface IDatabaseCkecker
	{
		bool Check(string token);
		IDiagnostic Diagnostic { get; }
	}
}
