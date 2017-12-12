using System;

namespace Microarea.TaskBuilderNet.Interfaces.Model
{
	//=========================================================================
	[System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
	public interface IDataBinding : ICloneable
	{
		bool IsEmpty { get; }
		string Name { get; }
		object Data { get; }
		IDataType DataType { get; }
		bool IsDataReadOnly { get; }
        IDataManager Parent { get; }
        bool Modified { get; set; }
	}

	//=============================================================================
	public interface IDataBindingConsumer
	{
		IDataBinding DataBinding { get; set; }
		Type		 ExcludedBindParentType { get; }
        IDataManager FixedDataManager { get; }

		bool	CanAutoFillFromDataBinding { get; }
		void	AutoFillFromDataBinding(IDataBinding dataBinding, bool overrideExisting);

        IDataType CompatibleDataType { get; }
	}
}
