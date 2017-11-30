using Microarea.AdminServer.Model;
using Microarea.AdminServer.Libraries.DataManagerEngine;

namespace Microarea.AdminServer.Controllers.Helpers.Database
{
	/// <summary>
	/// Classe utilizzata per passare al back-end l'oggetto SubscriptionDatabase e 
	/// le credenziali di amministrazione al server
	/// </summary>
	//================================================================================
	public class ExtendedSubscriptionDatabase
	{
		public DatabaseCredentials AdminCredentials;
		public SubscriptionDatabase Database;
	}

	/// <summary>
	/// Classe con le informazioni lette dalla tabella TB_DBMark, per controllare che 
	/// i dati siano coerenti
	/// </summary>
	//================================================================================
	public class DBInfo
	{
		public string Name = string.Empty;
		public bool ExistDBMark = false;
		public bool UseUnicode = false;
		public string Collation = string.Empty;
		public string Error = string.Empty;

		public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
	}

	/// <summary>
	/// Classe con i parametri aggiuntivi utilizzati nell'importazione dati
	/// </summary>
	//============================================================================
	public class ImportDataParameters
	{
		public bool OverwriteRecord = true;
		public bool DeleteTableContext = false;
	}

	/// <summary>
	/// Classe utilizzata in fase di importazione dati e che viene passata al back-end 
	/// Si compone della classe SubscriptionDatabase e degli ulteriori parametri aggiuntivi
	/// </summary>
	//============================================================================
	public class ImportDataBodyContent
    {
		public SubscriptionDatabase Database;
		public ImportDataParameters ImportParameters;
	}
}
