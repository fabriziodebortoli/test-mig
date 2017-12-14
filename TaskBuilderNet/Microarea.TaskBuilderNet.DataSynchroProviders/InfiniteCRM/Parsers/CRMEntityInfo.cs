using System;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers
{
	///<summary>
	/// Nodo di tipo Field
	///</summary>
	//================================================================================
	internal class CRMField
	{
		public string Target { get; set; }
		public bool Key { get; set; }
		public bool Mandatory { get; set; }
		public string Entity { get; set; }
		public string ParentField { get; set; }
		public bool InternalUse { get; set; }
	}

	///<summary>
	/// Nodo di tipo Entity
	///</summary>
	//================================================================================
	internal class CRMEntity
	{
		public string Name { get; private set; }

		public string TranscodingTable { get; set; } // nome tbl PK di Mago
		public string TranscodingField { get; set; } // nome colonna PK di Mago
		public string TranscodingExternalField { get; set; } // nome colonna di riferimento in Pat

		public List<CRMField> Fields = new List<CRMField>();
		public List<CRMField> FKFields = new List<CRMField>(); // per tenere traccia delle FK

		public string Select { get; set; }
		public string From { get; set; }
		public string Where { get; set; }

		public string Query { get; set; }
		public string WhereClause { get; set; }
		public string MassiveWhereClause { get; set; } //  porzione di WHERE da applicare SOLO in esportazione massiva (caso del Disabled)

		public string ExplicitQuery = string.Empty;

		public string DeleteTarget { get; set; }
		public string DeleteValue { get; set; }

		// GESTIONE SUBENTITIES (lista di oggetti che contengono il nome e la lista di PKvalues (utili per la massiva))
		public List<string> SubEntities = new List<string>();
		public List<string> PKValues = new List<string>(); // utilizzata nelle subentities per sapere il valore della PK del master e fare la query nello slave

		public List<string> TBGuids = new List<string>(); // elenco dei TBGuid di Mago
		//

		// se l'entita' e' da considerarsi primaria
		//--------------------------------------------------------------------------------
		public bool IsPrimary { get { return (!string.IsNullOrWhiteSpace(TranscodingTable) && !string.IsNullOrWhiteSpace(TranscodingField) && !string.IsNullOrWhiteSpace(TranscodingExternalField)); } }

		// se l'entita' prevede il nodo particolare BankAccount
		//--------------------------------------------------------------------------------
		public bool HasBankAccountNode { get { return (string.Compare(Name, "Account", StringComparison.InvariantCultureIgnoreCase) == 0) || (string.Compare(Name, "Order", StringComparison.InvariantCultureIgnoreCase) == 0); } }

		//--------------------------------------------------------------------------------
		public CRMEntity(string name, string table)
		{
			Name = name;
			TranscodingTable = table;
		}

		///<summary>
		/// Ritorna la query componendo i nodi letti dall'xml (sintassi SQL)
		///</summary>
		//--------------------------------------------------------------------------------
		public string GetSQLFormattedQuery()
		{
			string query = string.Format("SELECT {0} FROM {1}", Select, From);

			if (!string.IsNullOrWhiteSpace(Where))
				query += string.Format(" WHERE {0}", Where);

			return query;
		}
	}

	///<summary>
	/// Classi per il caricamento in memoria delle informazioni lette dai file che 
	/// descrivono le varie Entity del CRM
	///</summary>
	//================================================================================
	internal class CRMEntityInfo // TODO FARE SPARIRE
	{
		public CRMEntity Entity;

		//--------------------------------------------------------------------------------
		public CRMEntityInfo()
		{
		}
	}
}
