using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using System.Collections.Generic;
using System.Data;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders
{
	///<summary>
	/// Classi per la memorizzazione dei dati estratti da database a fronte di una notifica del CDNotification
	///</summary>
	//================================================================================
	internal class ActionToSynch : BaseObjectToSynch
	{
		public List<ActionToExport> ActionsToExecute = new List<ActionToExport>(); // lista di azioni in xml da eseguire

		//--------------------------------------------------------------------------------
		public ActionToSynch(BaseObjectToSynch baseObject)
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
	/// Classe per memorizzare la sequenza delle actions e relativi xml da inviare al CRM
	///</summary>
	//================================================================================
	internal class ActionToExport
	{
		public string Name { get; set; }		// nome azione
		public string XmlToImport { get; set; }	// stringa cumulativa del file xml

		public List<string> Keys { get; set; }	// lista di chiavi per la transcodifica
		public List<string> TBGuid { get; set; }	// lista dei TBGuid
		public List<string> Image { get; set; }	// lista dei path assoluto immagine articolo
		public List<bool> IsSucceeded { get; set; }// lista di valori di ritorno di executeSyncro
        public List<string> ErrorMessages { get; set; }

        //--------------------------------------------------------------------------------
        public ActionToExport()
		{ }

		//--------------------------------------------------------------------------------
		public ActionToExport(string name)
		{
			Name = name;
			XmlToImport = string.Empty;
			Keys = new List<string>();
			TBGuid = new List<string>();
			Image = new List<string>();
			IsSucceeded = new List<bool>();
            ErrorMessages = new List<string>();
		}
		//--------------------------------------------------------------------------------
		public ActionToExport(string name, string xmlToImport) : this(name)
		{
			XmlToImport = xmlToImport;
		}
	}

	///<summary>
	/// Classe che identifica la struttura dei DataTable per importare un documento nel CRM
	/// MasterDt: DataTable identifica il master (che esiste sempre)
	/// SlavesDtList: la lista di DataTable identifica gli slave
	/// AppendDtList: la lista di DataTable identifica le personalizzazioni
	///</summary>
	//================================================================================
	internal class DTValuesToImport
	{
		public DataTable MasterDt { get; set; }
		public List<DataTable> SlavesDtList { get; set; }
		public List<DTValuesToImport> AppendDtList { get; set; }

		//--------------------------------------------------------------------------------
		public DTValuesToImport(DataTable masterDt)
		{
			MasterDt = masterDt; // il master c'e' sempre
			SlavesDtList = new List<DataTable>();
			AppendDtList = new List<DTValuesToImport>();
		}
	}

	//================================================================================
	internal class ActionToImport
	{
		public string ActionName { get; set; }
		public string ProccessId { get; set; }
		public List<string> InfinityKeys { get; set; }
		public string MagoXml { get; set; }
		public string MagoKey { get; set; }
		public bool DoRollback { get; set; }
		public string TBGuid { get; set; }
		public string Message { get; set; }

		//--------------------------------------------------------------------------------
		public ActionToImport()
		{
			DoRollback = false;
			InfinityKeys = new List<string>();
			Message = "";
		}
	}
}