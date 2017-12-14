
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface INameSpaceType
	{
		string PublicName { get; }
		int TokenNumber { get; }
		NameSpaceObjectType Type { get; }
	}

	/// <summary>
	/// tipo di name space
	/// </summary>
	//=========================================================================
	[System.Serializable]
	public enum NameSpaceObjectType
	{
		Application,
		Module,
		Library,
		Document,
		Dbt,
		Hotlink,
		Function,
		Report,
		Image,
		Text,
		File,
		Table,
		View,
		Procedure,
		ExcelDocument,
		ExcelTemplate,
		WordDocument,
		WordTemplate,
		ExcelDocument2007,
		ExcelTemplate2007,
		WordDocument2007,
		WordTemplate2007,
		ReportSchema,
		DocumentSchema,
		Setting,
		NotValid,
		Control,
		Form,
		Customization,
		Leaf,
		HotKeyLink,
		Standardization,
        ToolbarButton,
        Toolbar,
        Tabber,
        TabDialog,
        Grid,
        GridColumn
		//Sono solo nella parte C++: DATAFILE, EVENTHANDLER, PROFILE, ALIAS
	}
}
