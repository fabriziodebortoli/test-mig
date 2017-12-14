using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Interfaces.View
{
	[System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
	//================================================================================
	public interface IControlClass
	{
		IDataType	CompatibleType				{ get; }
		string		ClassName					{ get; }
		string		FamilyName					{ get; }
	}

	//================================================================================
	public interface IControlClassConsumer
	{
		IControlClass ClassType			{ get; set;  }
		bool HasFamilyClassChangeable	{ get; }
	}
}
