using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// LicenceInfo
	/// Informazioni sulla licenza del programma accessibile a tutti i PlugIns della Console e altrove
	/// </summary>
	// 30/10/2006 - Progetto 2772 (nome in codice: 'i' turca):
	// Specifico l'invariant culture perchè il confronto tra le stringhe deve
	// essere indipendente dalla culture della macchina su cui sta girando il prodotto.
	// ========================================================================
	public class LicenceInfo
	{
		private string edition = string.Empty;
        private string editionTextualType = string.Empty;
        private string isoState = string.Empty;
		private DBNetworkType dbNetworkType = DBNetworkType.Undefined;
		private bool isSQL2012Allowed = false;
		private bool isAzureSQLDatabase = false;

		//---------------------------------------------------------------------
		public string Edition { get { return edition; } set { edition = value; } }
        public string EditionTextualType { get { return editionTextualType; } set { editionTextualType = value; } }
        public string IsoState { get { return isoState; } set { isoState = value; }}
		public DBNetworkType DBNetworkType { get { return dbNetworkType; }	set { dbNetworkType	= value; }}
		public bool IsSQL2012Allowed { get { return isSQL2012Allowed; } set { isSQL2012Allowed = value; } }
		public bool IsAzureSQLDatabase { get { return isAzureSQLDatabase; }	set { isAzureSQLDatabase = value; } }

		//---------------------------------------------------------------------
		public LicenceInfo()
		{
		}

		///<summary> 
		/// Metodo che ritorna se e' previsto per l'isostato passato come parametro
		/// l'utilizzo dello Unicode come character set (default da impostare in fase 
		/// di creazione database)
		///</summary>
		//---------------------------------------------------------------------
		public bool UseUnicodeSet()
		{
			return IsoHelper.IsoExpectUnicodeCharSet(IsoState);
		}
	}
}