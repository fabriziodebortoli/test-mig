
namespace Microarea.Console.Plugin.SysAdmin.Form
{
	///<summary>
	/// Form che visualizza un elenco di parametri da proporre per la cancellazione
	/// dell'azienda:
	/// - l'anagrafica dell'azienda (e tutte le tabelle correlate) viene SEMPRE eliminata
	/// - su richiesta l'utente puo' decidere di eliminare anche il database:
	///		1) aziendale
	///		2) documentale (solo se associato uno slave all'azienda oggetto di cancellazione)
	///</summary>
	//================================================================================
	public partial class DeleteCompany : System.Windows.Forms.Form
	{
        private bool isDMSActivated = false;
		private bool useDbSlave = false;
		private bool isLite = false;

		public bool IsDMSActivated { get { return isDMSActivated; } set { isDMSActivated = value; } }

		// Properties
		//--------------------------------------------------------------------------------
		public bool DeleteCompanyDB { get { return DeleteCompanyDBCheckBox.Checked; } }
		public bool DeleteDMSDB { get { return DeleteDMSDBCheckBox.Checked; } }

		//--------------------------------------------------------------------------------
		public DeleteCompany(bool useDbSlave, bool isDMSActivated, bool isLite = false)
		{
			InitializeComponent();

			this.useDbSlave = useDbSlave;
			this.isDMSActivated = isDMSActivated;
		}

		//--------------------------------------------------------------------------------
		private void DeleteCompany_Load(object sender, System.EventArgs e)
		{
			// se la company ha uno slave associato e il modulo dms e' attivato allora visualizzo la checkbox
			DeleteDMSDBCheckBox.Visible = useDbSlave && isDMSActivated;

			// se non e' visibile la checkbox imposto d'ufficio il suo valore a false
			if (!useDbSlave)
				DeleteDMSDBCheckBox.Checked = false;

			DeleteCompanyDBCheckBox.Visible = !isLite;

			if (isLite)
			{
				DeleteCompanyDBCheckBox.Checked = DeleteDMSDBCheckBox.Checked = false;
				DeleteDMSDBCheckBox.Visible = false;
			}
		}
	}
}
