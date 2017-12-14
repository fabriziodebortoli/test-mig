using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDiagnosticItems : IEnumerable
	{
		bool AreItemsOfTypePresent(DiagnosticType diagnosticType);
		void Clear(DiagnosticType diagnosticType);
		int CountOfType(DiagnosticType diagnosticType);
		IDiagnosticItems GetItems(DiagnosticType diagnosticType);
		string OwnerName { get; }
		IDiagnosticItem this[int index] { get; set; }
		int Count { get; }
	}
}
