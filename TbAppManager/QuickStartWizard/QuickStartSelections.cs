
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.MenuManager.QuickStartWizard
{
	///<summary>
	/// Classe che memorizza le selezioni effettuate dall'utente nelle pagine del wizard
	///</summary>
	//================================================================================
	public class QuickStartSelections
	{
		// informazioni per la creazione del database di sistema/aziendale
		public string Server = string.Empty;
		public string Login = string.Empty;
		public string Password = string.Empty;

        // di default metto i nomi da noi stabiliti, poi l'utente li puo' cambiare
        public string SystemDBName ;//= DatabaseLayerConsts.MagoNetSystemDBName;
        public string CompanyDBName ;//= DatabaseLayerConsts.MagoNetCompanyDBName;
        public string CompanyName ;//= DatabaseLayerConsts.MagoNetCompanyName;

		public bool LoadDefaultData = true;

		public DBNetworkType DBNetworkType { get; private set; }
		public string Country { get; private set; }
		public string Edition { get; private set; }
		//

		public bool SkipBaseConfiguration = false;

		//--------------------------------------------------------------------------------
		public QuickStartSelections()
		{
		}
        //--------------------------------------------------------------------------------
        public QuickStartSelections(string prefix)
        {
        SystemDBName = DatabaseLayerConsts.MagoNetSystemDBName(prefix);
        CompanyDBName = DatabaseLayerConsts.MagoNetCompanyDBName(prefix);
        CompanyName = DatabaseLayerConsts.MagoNetCompanyName(prefix);
        }
	}
}
