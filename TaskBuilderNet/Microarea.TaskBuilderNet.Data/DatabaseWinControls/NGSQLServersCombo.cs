using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	///<summary>
	///	dal 03/12/09 (Miglioria 4076) questa combobox sostituisce la vecchia SqlServersCombo (che e' stata mantenuta come storia)
	/// Combo-box derivata che si occupa di visualizzare un elenco di SQL Server
	/// serializzati in un file xml memorizzato nel file system. 
	/// Su richiesta dell'utente e' possibile broware tramite un'apposita form nei
	/// server locali e di rete.
	///</summary>
	//========================================================================
	public partial class NGSQLServersCombo : ComboBox
	{
		List<string> sqlServersList = new List<string>();

		private string initSQLServer = string.Empty;
		private string selectedSQLServer = string.Empty;
		private string serverName = string.Empty;
		private string instanceName = string.Empty;

		private string sqlServersFilePath = 
			Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SQLServers.xml");

		//---------------------------------------------------------------------
		// evento sparato dopo la OnLeave dalla combobox
		public delegate void SetSelectedServerSQL(string serverName);
		public event SetSelectedServerSQL OnSetSelectedServerSQL;

		// evento sparato sull'evento OnSelectedIndexChanged della combobox
		public delegate void ChangeServerName();
		public event ChangeServerName OnChangeServerName;

		// Properties
		//---------------------------------------------------------------------
		public string SelectedSQLServer 
		{ 
			get { return selectedSQLServer; } 
			set 
			{ 
				selectedSQLServer = value;

				if (!string.IsNullOrEmpty(selectedSQLServer))
				{
					string[] serverElements = selectedSQLServer.Split(Path.DirectorySeparatorChar);
					if (serverElements != null)
					{
						serverName = (serverElements.Length > 0) ? serverElements[0] : string.Empty;
						instanceName = (serverElements.Length > 1) ? serverElements[1] : string.Empty;
					}
				}
			} 
		}
		
		public string ServerName { get { return serverName.ToUpperInvariant(); } }
		public string InstanceName { get { return instanceName.ToUpperInvariant(); } }

		//---------------------------------------------------------------------
		public NGSQLServersCombo()
		{
			InitializeComponent();
			LoadInfoFromFile();
		}

		//---------------------------------------------------------------------
		public NGSQLServersCombo(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
			LoadInfoFromFile();
		}

		///<summary>
		/// Carico l'elenco dei server storicizzati nel file apposito (se esiste)
		///</summary>
		//---------------------------------------------------------------------
		private void LoadInfoFromFile()
		{
			if (DesignMode)
				return;

			if (File.Exists(sqlServersFilePath))
			{
				try
				{
					using (FileStream inputFile = new FileStream(sqlServersFilePath, FileMode.Open, FileAccess.Read))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(SQLServersCombo));
						XmlReader xmlReader = XmlReader.Create(inputFile);

						SQLServersCombo newList = (SQLServersCombo)serializer.Deserialize(xmlReader);
						foreach (SQLServer ss in newList.Servers)
							sqlServersList.Add(ss.Name);
					}
				}
				catch
				{ }
			}
			// in fondo aggiungo anche l'item per dare la possibilita' di browsare i server
			sqlServersList.Add(DatabaseWinControlsStrings.BrowseForMore);
			
			// riempio la combo
			this.Items.Clear();
			this.Items.AddRange(sqlServersList.ToArray());

			// aggiungo un menu di contesto alla combo che consenta di eliminare il file
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.Data.DatabaseWinControls" + ".Images.RecycleBinEmpty.png");
            Image itemImage = (imageStream != null) ? Image.FromStream(imageStream) : null;

			this.ContextMenuStrip = new ContextMenuStrip();
			this.ContextMenuStrip.Items.Add(new ToolStripMenuItem(DatabaseWinControlsStrings.ClearItems, itemImage));
			this.ContextMenuStrip.Items[0].Click += new EventHandler(SQLServersComboClearItems_Click);
		}

		///<summary>
		/// Evento intercettato sul click del menu di contesto della combo
		/// Elimina il file e aggiunge solo la voce di Browse
		///</summary>
		//---------------------------------------------------------------------
		private void SQLServersComboClearItems_Click(object sender, EventArgs e)
		{
			try
			{
				File.Delete(sqlServersFilePath);
				sqlServersList.Clear();

				if (!string.IsNullOrEmpty(initSQLServer))
					sqlServersList.Add(initSQLServer);
				sqlServersList.Add(DatabaseWinControlsStrings.BrowseForMore);

				if (sqlServersList.Count > 0)
					this.SelectedIndex = 0;
			}
			catch
			{
			}
		}

		///<summary>
		/// Metodo esposto per aggiungere un server di default alla lista, se necessario
		///</summary>
		//---------------------------------------------------------------------
		public void InitDefaultServer(string serverToAdd)
		{
			AddServer(serverToAdd, true);
			SelectedSQLServer = serverToAdd;
		}

		///<summary>
		/// Overload del metodo che si occupa di aggiungere alla combobox il server selezionato.
		/// Consente di memorizzare in una variabile globale della classe un server da aggiungere sempre alla lista
		///</summary>
		//---------------------------------------------------------------------
		private void AddServer(string serverToAdd, bool storeServerToInit)
		{
			AddServer(serverToAdd);
			if (storeServerToInit)
				initSQLServer = serverToAdd;
		}

		///<summary>
		/// Metodo che si occupa di aggiungere alla combobox il server selezionato.
		/// Se questo esiste gia' nell'elenco allora seleziono l'esistente, se non esiste lo inserisce come
		/// penultimo elemento (l'ultimo e' sempre il browse)
		///</summary>
		//---------------------------------------------------------------------
		private void AddServer(string serverToAdd)
		{
			if (string.Compare(serverToAdd, DatabaseWinControlsStrings.BrowseForMore, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.IsNullOrEmpty(serverToAdd))
				return;

			// il server memorizzato e' sempre maiuscolo
			serverToAdd = serverToAdd.ToUpperInvariant();

			try
			{
				if (sqlServersList.Contains(serverToAdd))
				{
					// se esiste gia' nell'elenco lo seleziono
					int i = sqlServersList.IndexOf(serverToAdd);
					if (this.Items.Count > i)
						this.SelectedIndex = i;
				}
				else
				{
					// se non esiste lo inserisco in fondo (prima del browse) e lo seleziono
					sqlServersList.Insert(sqlServersList.Count - 1, serverToAdd);
					this.Items.Clear();
					this.Items.AddRange(sqlServersList.ToArray());
					this.SelectedIndex = sqlServersList.Count - 2;
				}
			}
			catch
			{
			}
		}
		
		///<summary>
		/// Cliccando sulla tendina della combo travaso tutti i server che ho aggiunto nella lista di appoggio
		/// negli items della combo
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnDropDown(System.EventArgs e)
		{
			base.OnDropDown(e);

			if (DesignMode)
				return;

			this.Items.Clear();
			this.Items.AddRange(sqlServersList.ToArray());
		}

		///<summary>
		/// Sul cambio della selezione nella combo, se mi trovo sull'ultimo elemento (che e' sempre il 
		/// browse) apro la finestra per cercare il server
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnSelectedValueChanged(EventArgs e)
		{
			base.OnSelectedValueChanged(e);

			// se ho selezionato l'ultimo indice significa che voglio browsare
			if (this.SelectedIndex == sqlServersList.Count - 1)
			{
				// allora apro la finestra con l'elenco dei server locali e di rete
				SQLServerBrowser ssb = new SQLServerBrowser();
				DialogResult dr = ssb.ShowDialog();

				if (dr == DialogResult.OK)
				{
					// se ho selezionato un server lo aggiungo alla lista
					if (!string.IsNullOrEmpty(ssb.SelectedServer))
						AddServer(ssb.SelectedServer);
				}
				else // se ho scelto Cancel seleziono quello di prima
					if (this.Items.Count > 0)
						this.SelectedIndex = 0;
			}
		}

		///<summary>
		/// Nel cambio di selezione controllo se ho cambiato il nome del server precedente
		/// Se si invio un evento che potrebbe servire alla form parent per effettuare delle azioni (pulizia di campi, etc.)
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			string server = this.Text;

			try
			{
				if (InstanceName.Length == 0)
				{
					if (ServerName.Length > 0 &&
						string.Compare(server, ServerName, StringComparison.InvariantCultureIgnoreCase) != 0)
						if (OnChangeServerName != null)
							OnChangeServerName();
				}
				else
				{
					if (ServerName.Length > 0 && InstanceName.Length > 0 &&
						(string.Compare(Path.GetDirectoryName(server), ServerName, StringComparison.InvariantCultureIgnoreCase) != 0 ||
						string.Compare(Path.GetFileName(server), InstanceName, StringComparison.InvariantCultureIgnoreCase) != 0))
						if (OnChangeServerName != null)
							OnChangeServerName();
				}
			}
			catch
			{
			}
		}

		///<summary>
		/// Sulla Leave della combo inserisco nella lista di supporto quello che ho scritto
		/// Sostituisco gli eventuali "local" con il nome macchina
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			// se l'utente ha scritto local nelle sue varianti metto il nome macchina
			if (
				string.IsNullOrEmpty(this.Text) ||
				(string.Compare(this.Text, "(local)", StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(this.Text, "(localhost)", StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(this.Text, "local", StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(this.Text, "localhost", StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(this.Text, ".", StringComparison.InvariantCultureIgnoreCase) == 0)
				)
				AddServer(string.IsNullOrEmpty(initSQLServer) ? System.Net.Dns.GetHostName() : initSQLServer);
			else
				AddServer(this.Text);

			SelectedSQLServer = (this.SelectedItem != null)
				? this.SelectedItem.ToString().ToUpperInvariant()
				: System.Net.Dns.GetHostName().ToUpperInvariant();

			if (OnSetSelectedServerSQL != null)
				OnSetSelectedServerSQL(SelectedSQLServer);
		}

		///<summary>
		/// Al momento del cambio visibilita' scrivo le informazioni sul file
		/// (usavo il metodo OnHandleDestroyed, ma sul cambio di form dentro il panel della console non veniva richiamato)
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			WriteServersToFile();
		}

		//---------------------------------------------------------------------
		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			if (!this.Visible)
				WriteServersToFile();
		}

		///<summary>
		/// Metodo che serializza le info nel file xml (se non esiste lo creo)
		///</summary>
		//---------------------------------------------------------------------
		private void WriteServersToFile()
		{
			// elimino dalla lista dei server l'ultimo item (che sarebbe quello del browse)
			SQLServersCombo myList = new SQLServersCombo();
			for (int i = 0; i < sqlServersList.Count - 1; i++)
				myList.AddServer(new SQLServer(sqlServersList[i]));

			// se non ci sono item nella lista non procedo a serializzare
			if (myList.ItemsCount == 0)
				return;

			try
			{
				using (FileStream outputFile = new FileStream(sqlServersFilePath, FileMode.Create, FileAccess.Write))
				{
					XmlWriter xmlWriter = XmlWriter.Create(outputFile);
					XmlSerializer serializer = new XmlSerializer(typeof(SQLServersCombo));
					// serializzo in un xml l'elenco dei server
					serializer.Serialize(outputFile, myList);
				}
			}
			catch
			{ }
		}
	}
}

/* TEST DA FARE:
 * salvare in xml i vari server
 * caricare file xml con errori
 * caricare file xml con sintassi diversa
 * creare il file se non esiste
 * inizializzare la combo con il nome del server memorizzato nel serverconnection.config (AddServer)
 * con tasto dx sulla combo consentire di svuotare i server memorizzati sul file
 * caricare file xml vuoto
*/