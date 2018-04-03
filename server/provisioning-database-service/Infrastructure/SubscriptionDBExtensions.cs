﻿using Microarea.ProvisioningDatabase.Infrastructure.Model;

namespace Microarea.ProvisioningDatabase.Infrastructure
{
	/// <summary>
	/// Classe passata dal provisioning back-end contenente le informazioni
	/// dell'oggetto SubscriptionDatabase e le credenziali di amministrazione al server
 	/// piu' le informazioni aggiuntive specificate al momento della sottoscrizione 
	/// della subscription: country, collation, tipo DB di appoggio
	/// </summary>
	//================================================================================
	public class ExtendedSubscriptionDatabase
	{
		public DatabaseCredentials AdminCredentials;
		public SubscriptionDatabase Database;
		public string Country = string.Empty;
		public string Collation = string.Empty;
		public DB DB = DB.unknown;
	}

	public enum DB { sqlazure = 0, onpremises = 1, othercloud = 2, privatecloud = 3, unknown = 4, };

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

	#region Import data
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

	/// <summary>
	/// Classe con i parametri aggiuntivi utilizzati nell'importazione dati
	/// </summary>
	//============================================================================
	public class ImportDataParameters
	{
		public bool OverwriteRecord = true;
		public bool DeleteTableContext = false;
		public bool NoOptional = true;
	}
	#endregion

	#region Delete database
	/// <summary>
	/// Classe utilizzata in fase di cancellazione di un oggetto di tipo SubscriptionDatabase
	/// Si compone di:
	/// - oggetto SubscriptionDatabase da eliminare
	/// - credenziali di amministrazione di SQL Azure (solo per questo tipo di provider e se voglio eliminare almeno un database)
	/// - parametri aggiuntivi relativi all'eliminazione dei contenitori
	/// </summary>
	//============================================================================
	public class DeleteDatabaseBodyContent
	{
		public SubscriptionDatabase Database;
		public DatabaseCredentials AdminCredentials;
		public DeleteDatabaseParameters DeleteParameters;
	}

	/// <summary>
	/// Classe con i parametri aggiuntivi utilizzati nella cancellazione dei database
	/// </summary>
	//============================================================================
	public class DeleteDatabaseParameters
	{
		public bool DeleteERPDatabase = false;
		public bool DeleteDMSDatabase = false;
	}
	#endregion
}
