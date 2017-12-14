
namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	public interface ITBWebControl
	{
		int ProxyObjectId { get; }
		string WindowId { get; }
		ITBWebControl ParentTBWebControl { get; }
        System.Drawing.Point ChildsOffset { get; set; }
        System.Drawing.Size InflateSize { get; set; }		
        int X { get; }
		int Y { get; }
		int ZIndex { get;  }
		TBForm OwnerForm { get; }
		WndObjDescription ControlDescription { get; set; }
    }

	public interface ICommand
	{
		ITBWebControl CommandObject { get; }
		int CommandId { get; }
	}
}
