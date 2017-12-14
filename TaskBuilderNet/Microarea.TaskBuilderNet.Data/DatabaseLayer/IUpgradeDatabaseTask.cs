
using Microarea.TaskBuilderNet.Core.DiagnosticManager;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	///<summary>
	/// Interfaccia da reimplementare nelle dll da eseguire durante
	/// l'aggiornamento database di livello 3
	/// 
	/// E' necessario aggiungere come reference al progetto le seguenti dll:
	/// Microarea.TaskBuilderNet.Data.dll
	/// Microarea.TaskBuilderNet.Interfaces.dll
	/// Microarea.TaskBuilderNet.Core.dll
	/// cercando nel folder ..\Apps\TBApps\release\
	///</summary>
	//=========================================================================
	public interface IUpgradeDatabaseTask
	{
		bool Run(TBConnection connection);
		Diagnostic GetDiagnostic();
	}
}
	