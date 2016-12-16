using System;
using System.Collections;

namespace TaskBuilderNetCore.Interfaces.Model
{
	public enum DataRelationType	{
										None,
										Master,
										OneToOne,
										OneToMany,
										ForeignKey
									};
	public enum DelayReadType		{ 
										Immediate,
										DalayedUntilBrowse,
										DalayedUntilEdit,
										Delayed 
									};

	public enum FormModeType	{
									None, 
									Browse, 
									New, 
									Edit, 
									Find, 
									Design 
								};
	//=============================================================================
	public interface IDocumentDataManager
	{
		INameSpace	Namespace { get; }
		string ControllerType { get; }
		bool InUnattendedMode { get; }
        bool Batch            { get; }
		bool AutoValueChanged { get; }
		IntPtr GetReadOnlySessionPtr ();
		FormModeType FormMode { get; }
        Int32 TbHandle        { get; }
        bool Modified         { get; set; }
        bool IsAlive        { get; }
        string Title        { get; }
        void Close();
    }

	//=============================================================================
	public interface IDataManager
	{
		INameSpace Namespace { get; }
		string Name { get; }
		string TableName { get; }
		IRecord Record { get; }
		DataRelationType Relation { get; }
		bool HasCodeBehind { get; }
		bool IsUpdatable { get; }
        IDocumentDataManager Document { get; }
		object BindableDataSource { get;  }
		void OnQuerying	();
		void OnQueried	();
	}

	//=============================================================================
	public interface IDocumentSlaveDataManager : IDataManager
	{
		IDocumentMasterDataManager Master { get; set; }
	}

    //=============================================================================
    public interface IDocumentSlaveBufferedDataManager : IDocumentSlaveDataManager
    {
        bool Modified { get; set; }
		IList Rows { get; }
    }

	//=============================================================================
	public interface IDocumentMasterDataManager : IDataManager
	{
		
	}

	//=============================================================================
	public interface IWoormInfo 
	{
		string[] GetInputParamNames();

		object GetInputParamValue(string name);
	}

	public delegate IWoormInfo CreateWoormInfoWrapper(IntPtr woormInfoPtr);
}
