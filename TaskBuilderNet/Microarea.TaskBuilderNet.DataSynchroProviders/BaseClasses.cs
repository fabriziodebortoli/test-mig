using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
	///<summary>
	/// Parser dei file dei profili di sincronizzazione
	///</summary>
	//================================================================================
	internal abstract class BaseSynchroProfileParser
	{
		//---------------------------------------------------------------------
		public abstract bool ParseFile(string path, string addOnAppName);
		//--------------------------------------------------------------------------------
		public abstract SynchroProfileInfo SynchroProfileInfo { get; }
	}

	/// <summary>
	/// Classe memorizzare i dati per l'esportazione massiva
	/// Esempio di query per spezzare in pagine di 200 righe alla volta:
	/// SELECT * FROM MA_Items where Item between 'QW000049' and 'QW000297' ORDER BY Item OFFSET 0 ROWS FETCH NEXT 200 ROWS ONLY;
	/// SELECT * FROM MA_Items where Item between 'QW000049' and 'QW000297' ORDER BY Item OFFSET 200 ROWS FETCH NEXT 200 ROWS ONLY;
	/// </summary>
	//================================================================================
	internal class MassiveExportData
	{
		public int LogId { get; set; } // logid
		public string EntityName { get; set; } // namespace documento
		public string Filters { get; set; } // filtri da applicare alla WHERE scelti nella batch
		public int OffSet { get; set; } // numero OFFSET per la query spezzata
		public int TBCreatedID { get; set; }
		public int TBModifiedID { get; set; }
	}


	//================================================================================
	internal class BaseObjectToSynch
	{
		// informazioni dell'azione corrente da eseguire, di cui e' arrivata notifica
		public int LogID { get; set; }
		public string DocNamespace { get; set; }
		public string DocTBGuid { get; set; }
		public SynchroActionType ActionType { get; set; }
		public SynchroStatusType SynchStatus { get; set; }
		public string ActionData { get; set; }
	}

}
