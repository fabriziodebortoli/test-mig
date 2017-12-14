using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// DataBaseItem
	/// Classe astratta da cui derivano tutte le classi che gestiscono oggetti di database
	/// </summary>
	//=========================================================================
	public abstract class DataBaseItem
	{
		private string connectionString = string.Empty;
		private SqlConnection currentSqlConnection = null;
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
		private Diagnostic diagnostic = new Diagnostic("Microarea.TaskBuilderNet.Data");

		// Properties
		//---------------------------------------------------------------------
		public string ConnectionString { get { return connectionString; } set { connectionString = value; } }
		public SqlConnection CurrentSqlConnection { get { return currentSqlConnection; } set { currentSqlConnection = value; } }
		public DiagnosticViewer DiagnosticViewer { get { return diagnosticViewer; } set { diagnosticViewer = value; } }
		public Diagnostic Diagnostic { get { return diagnostic; } set { diagnostic = value; } }
	}
}
