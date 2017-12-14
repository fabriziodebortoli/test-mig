using System.Collections.Generic;
using System.Data;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM
{
	///<summary>
	/// Classi per la memorizzazione dei dati estratti da database a fronte di una notifica del CDNotification
	///</summary>
	//================================================================================
	internal class EntityToSynch : BaseObjectToSynch
	{
		// memorizzazione del testo xml + result inviato a Pat
		public string SynchXMLData = string.Empty;
		public string ResultString = string.Empty;

		//--------------------------------------------------------------------------------
		public EntityToSynch(BaseObjectToSynch baseObject)
		{
			this.LogID = baseObject.LogID;
			this.DocNamespace = baseObject.DocNamespace;
			this.DocTBGuid = baseObject.DocTBGuid;
			this.ActionType = baseObject.ActionType;
			this.SynchStatus = baseObject.SynchStatus;
			this.ActionData = baseObject.ActionData;
		}
	}

    ///<summary>
    /// Memorizzazione dei dati per la SetData in Mago (chiamate outbound)
    ///</summary>
    //================================================================================
    internal class SetDataInfo
    {
        public string PatID { get; set; }
        public string MagoID { get; set; } // codice assegnato da Mago a passare poi come codice esterno a Pat
        public string MagoXml { get; set; } // xml per la SetData di MagicLink
        public string PatXml { get; set; }
        public string Namespace { get; set; }
        public string MagoTableName { get; set; } // per la transcodifica
        public string EntityName { get; set; } // per la transcodifica
		public string TBGuid { get; set; }
        public bool Inserted = true;
        public List<SetDataInfo> PatRows = new List<SetDataInfo>(); // per gestire gli ID delle righe delle subentities di Pat
	}

	///<summary>
	/// Classe per memorizzare la sequenza delle entita' e relativi xml da inviare al CRM
	///</summary>
	//================================================================================
	internal class EntityToImport
	{
		public string Name { get; set; }
		public string TableName { get; set; }
		public string XmlToImport { get; set; }
        
		public int LogId = -1;
        public int Status = 0;

		//--------------------------------------------------------------------------------
        public EntityToImport()
        { }
	}

	///<summary>
	/// Classe che identifica la struttura dei DataTable per importare un documento nel CRM
	/// Il membro DataTable identifica il master (che esiste sempre), la lista di DataTable identifica gli slave
	///</summary>
	//================================================================================
	internal class DTPatValuesToImport
	{
		public DataTable MasterDt { get; set; }
        public string PatID { get; set; }     

		//--------------------------------------------------------------------------------
		public DTPatValuesToImport(DataTable masterDt)
		{
			MasterDt = masterDt; // il master c'e' sempre
		}
	}
}
