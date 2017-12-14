
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDiagnosticSimpleItem
	{
		string Message { get; set; }
		string ToString();
		DiagnosticType Type { get; set; }
		string TypeDesc { get; }
	}
}
