using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	///<summary>
	/// SqlServersCombo 
	/// (attualmente questa combobox non viene piu' utilizzata - in 03/12/09 (Miglioria 4076) con la NGSQLServersCombo)
	/// ComboBox derivata per visualizzare le istanze di SQL Server disponibili in rete
	/// Non utilizza componenti SQLDMO
	///</summary>
	//=========================================================================
	public partial class SqlServersCombo : ComboBox
	{
		#region Variabili private
		//---------------------------------------------------------------------
		private Diagnostic diagnostic = new Diagnostic("SqlServersCombo");
		private string	selectedIstanceSQLServer = string.Empty;
		private string	serverName               = string.Empty;
		private string	istanceName              = string.Empty;
		private bool	isPrimaryIstance         = false;
		
		private ArrayList allSqlServers	= null;
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } set { diagnostic = value; } }
		public int IstanceItems { get { return this.Items.Count; } }
		public string SelectedIstanceSQLServer { get { return selectedIstanceSQLServer; } set { selectedIstanceSQLServer = value; } }
		public string ServerName { get { return serverName.ToUpperInvariant(); } set { serverName = value.ToUpperInvariant(); } }
		public string IstanceName { get { return istanceName.ToUpperInvariant(); } set { istanceName = value.ToUpperInvariant(); } }
		public bool IsPrimaryIstance { get { return isPrimaryIstance; } set { isPrimaryIstance = value; } }
		#endregion

		#region Eventi e delegati
		//---------------------------------------------------------------------
		public delegate void SelectedServerSQL(object sender, string serverName);
		public event SelectedServerSQL OnSelectedServerSQL;

		public delegate void DropDownServerSQL(object sender);
		public event DropDownServerSQL OnDropDownServerSQL;

		public delegate void ClearOwnerDb(object sender);
		public event ClearOwnerDb OnClearOwnerDb;
		#endregion

		# region Costruttori
		//---------------------------------------------------------------------
		public SqlServersCombo()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public SqlServersCombo(IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}
		# endregion

		#region Metodi pubblici richiamabili al di fuori dello UserControl

		#region ClearListOfSqlServer - Svuota la combo box dei server sql
		/// <summary>
		/// ClearListOfSqlServer - pulisce la combo dei server sql
		/// </summary>
		//---------------------------------------------------------------------
		public void ClearListOfSqlServer()
		{
			this.Items.Clear();
		}
		#endregion

		#region IstanceItem - Ritorna il server sql di posizione pos all'interno della Combo
		/// <summary>
		/// IstanceItem
		/// Ritorna il server sql in posizione pos nella combo
		/// </summary>
		//---------------------------------------------------------------------
		public string IstanceItem(int pos)
		{
			string server = string.Empty;
			
			try
			{
				this.SelectedIndex = pos;
				server = (string)this.SelectedItem;
			}
			catch(System.ArgumentOutOfRangeException argumentExc)
			{
				Diagnostic.Set(DiagnosticType.Error, argumentExc.Message);
			}
			
			return server;
		}
		#endregion

		#region SelectIndex - Seleziona il server di posizione pos all'interno della Combo
		/// <summary>
		/// SelectIndex
		/// Seleziona il server in posizione pos all'interno della combo
		/// </summary>
		//---------------------------------------------------------------------
		public void SelectIndex(int pos)
		{
			try
			{
				this.SelectedIndex = pos;

				if (this.SelectedItem != null)
				{
					string serverSelected = this.SelectedItem.ToString();
					string [] serverElements = serverSelected.Split(Path.DirectorySeparatorChar);

					ServerName = serverElements[0];
					IstanceName = (serverElements.Length > 1) ? serverElements[1] : string.Empty;
				}
			}
			catch(System.ArgumentOutOfRangeException)
			{
				ServerName = string.Empty;
				IstanceName = string.Empty;
			}
		}
		#endregion

		#region Add - Aggiunge un server SQL alla combo
		/// <summary>
		/// Add
		/// Aggiunge un server SQL alla lista
		/// </summary>
		//---------------------------------------------------------------------
		public void Add( string itemToAdd)
		{
			this.Items.Clear();
			this.Items.Add(itemToAdd);
			this.SelectedItem = itemToAdd;
		}
		#endregion

		#region LoadIstanceSQL - Trova tutti i server di SQL disponibili in rete SENZA SQLDMO!
		/// <summary>
		/// LoadIstanceSQL
		/// Utilizza la nuova classe introdotta da VS2005 (SqlDataSourceEnumerator)
		/// per trovare i server di SQL disponibili in rete.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadIstanceSQL()
		{
			this.Items.Clear();

			try
			{
				allSqlServers = new ArrayList();

				SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
				DataTable table = instance.GetDataSources();

				string serverName, instanceName;

				foreach (DataRow row in table.Rows)
				{
					serverName = string.Empty;
					instanceName = string.Empty;

					foreach (DataColumn col in table.Columns)
					{
						if (string.Compare(col.ColumnName, "ServerName", true, CultureInfo.InvariantCulture) == 0)
							serverName = row[col].ToString();

						if (string.Compare(col.ColumnName, "InstanceName", true, CultureInfo.InvariantCulture) == 0)
							instanceName = row[col].ToString();
					}

					if (serverName.Length > 0)
					{
						if (instanceName.Length > 0)
							allSqlServers.Add(string.Concat(serverName, Path.DirectorySeparatorChar, instanceName));
						else
							allSqlServers.Add(serverName);
					}
				}

				// ordine alfabetico
				allSqlServers.Sort();

				// Carico la combo
				for (int i = 0; i < allSqlServers.Count; i++)
					this.Items.Add(allSqlServers[i].ToString());

				// Sposto le istanze primarie e secondarie della macchina corrente in cima alla lista
				MoveOnTopLocalServer();

				this.ForeColor = Color.Black;

				// se non ho nessun serverName impostato, mi posiziono per default sul computer name corrente
				if (ServerName.Length == 0)
				{
					// poi provare a usare SelectedIstanceSQLServer
					this.SelectedText = System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture);
					if (IstanceName.Length > 0)
						this.SelectedText += Path.DirectorySeparatorChar + IstanceName.ToUpper(CultureInfo.InvariantCulture);
				}
				else
				{
					// cerco il ServerName e se lo trovo mi posiziono lì
					int position = (IstanceName.Length == 0)  
									? this.Items.IndexOf(ServerName)
									: this.Items.IndexOf(ServerName + Path.DirectorySeparatorChar + IstanceName);
					this.SelectedIndex = (position != -1) ? position : 0;
				}
			}
			catch(Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}
			
			// si è verificato un errore
			if (this.Items.Count == 0)
				Diagnostic.Set(DiagnosticType.Error, DatabaseWinControlsStrings.ErrorLoadingSQLServers);
		}
		#endregion

		#endregion
		
		#region MoveOnTopLocalServer - Sposta le istanze del server locale in cima alla lista
		/// <summary>
		/// MoveOnTopLocalServer
		/// </summary>
		//---------------------------------------------------------------------
		private void MoveOnTopLocalServer()
		{
			string localServer = System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture);
			
			int pos = this.FindString(localServer, -1);
			int startIndex = -1;
			int elementCounter = 0;
			while (pos >= 0) 
			{
				//mi ritorna una posizione che ho già valutato e non ce ne sono altre quindi esco
				if (pos < startIndex)
					break;
				//Identifico l'oggetto da muovere e lo sposto
				object objectToMove = this.Items[pos];
				this.Items.RemoveAt(pos);
				this.Items.Insert(elementCounter, objectToMove);
				//incremento la posizione per la prossima ricerca
				startIndex = elementCounter + 1;
				//incremento la posizione per il prossimo inserimento
				elementCounter += 1;
				pos = this.FindString(localServer, startIndex);
			}
		}
		#endregion

		#region SetServerName - Imposta il nome del server sql selezionato
		/// <summary>
		/// SetServerName
		/// Imposta il nome del server
		/// </summary>
		//---------------------------------------------------------------------
		private void SetServerName(string server)
		{
			if (server.Length != 0)
			{
				try
				{
					if (Path.GetDirectoryName(server).Length > 0)
					{
						ServerName = Path.GetDirectoryName(server);
						if (ServerName == null)
							ServerName = string.Empty;
					}
					else
						ServerName = server;
				}
				catch (System.NullReferenceException)
				{ }
			}
			else
				ServerName = string.Empty;
	
/*			if (server.Length != 0)
			{
				try
				{
					ServerName = Path.GetDirectoryName(server);
				}
				catch (System.NullReferenceException)
				{
					ServerName = string.Empty;
				}

				if (ServerName == null || ServerName.Length == 0)
					ServerName = string.Empty;
			}
			else
				ServerName = string.Empty;
*/		}
		#endregion

		#region SetIstanceName - Imposta il nome dell'istanza di Sql selezionata
		/// <summary>
		/// SetIstanceName
		/// </summary>
		//---------------------------------------------------------------------
		private void SetIstanceName(string server)
		{
			IstanceName = Path.GetFileName(server);
		}
		#endregion

		#region SetIsPrimaryIstance - Se "nomeServer\nomeIstanza" ritorna false, altrimenti true
		/// <summary>
		/// SetIsPrimaryIstance
		/// Se SqlServer è del tipo "nomeServer\nomeIstanza" non è una istanza primaria
		/// </summary>
		//---------------------------------------------------------------------
		private void SetIsPrimaryIstance(string server)
		{
			if (server.IndexOf(Path.DirectorySeparatorChar) != -1)
				IsPrimaryIstance = false;
			else
				IsPrimaryIstance = true;
		}

		#endregion

		#region IsNotGroup - true se è un gruppo, false altrimenti
		/// <summary>
		/// IsNotGroup
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsNotGroup(string server)
		{
			if (server.IndexOf("Group", 0) != -1)
				return false;
			else
				return true;
		}
		#endregion

		# region Eventi intercettati sul control

		# region OnLeave event
		///<summary>
		/// OnLeave
		///</summary>
		//---------------------------------------------------------------------
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			if (this.Text.Length == 0)
			{
				// se il nome del server è vuoto faccio la clear di tutti gli items
				if (OnClearOwnerDb != null)
					OnClearOwnerDb(this);
				return;
			}

			string server = this.Text;

			string[] serverElements = this.Text.Split(Path.DirectorySeparatorChar);
			if (string.Compare(ServerName, serverElements[0], true, CultureInfo.InvariantCulture) != 0 ||
				string.Compare(IstanceName, (serverElements.Length > 1) ? serverElements[1] : string.Empty, true, CultureInfo.InvariantCulture) != 0)
			{
				// se non caricato l'elenco dei servers (ad es. ho modificato solo il testo del server)
				// forzo il loro caricamento
				if (this.allSqlServers == null)
				{
					ServerName	= serverElements[0];
					IstanceName = (serverElements.Length > 1) ? serverElements[1] : string.Empty;
					if (OnClearOwnerDb != null)
						OnClearOwnerDb(this);

					LoadIstanceSQL();
				}
			}

			if (this.SelectedItem == null)
			{
				int pos = this.FindStringExact(server, -1);
				
				if (pos >= 0)
					this.SelectedIndex = pos;
				else
				{
					// non c'è lo aggiungo
					this.Items.Add(server);
					pos = this.FindStringExact(server, -1);
					if (pos >= 0)
						this.SelectedIndex = pos;
					else
					{
						// lo segno come non caricato
						this.ForeColor = Color.Red;
						return;
					}
				}
			}

			if ((string.Compare(server.Substring(0), "(local)", true, CultureInfo.InvariantCulture) == 0) || 
				(string.Compare(server.Substring(0), "(localhost)", true, CultureInfo.InvariantCulture) == 0) ||
				(string.Compare(server.Substring(0), "local", true, CultureInfo.InvariantCulture) == 0) ||
				(string.Compare(server.Substring(0), "localhost", true, CultureInfo.InvariantCulture) == 0) ||
				(string.Compare(server.Substring(0), ".", true, CultureInfo.InvariantCulture) == 0)
				)
			{
				int posDelete = this.FindString(server, -1);
				string currentElement = string.Empty;
				// ho trovato l'elemento da sostituire
				if (posDelete != -1)
				{
					//faccio la sostituzione
					currentElement = this.Items[posDelete].ToString();
					currentElement = currentElement.Replace(server, System.Net.Dns.GetHostName());
				}
				else
					currentElement = server.Replace(server, System.Net.Dns.GetHostName());

				int existElement = this.FindStringExact(currentElement, -1);
				// l'elemento sostituito esiste già
				if (existElement == -1)
					existElement = this.Items.Add(currentElement);

				server = this.Items[existElement].ToString();
				this.SelectedIndex = existElement;
			}

			this.ForeColor = Color.Black;
			SelectedIstanceSQLServer = server;
			SetServerName(SelectedIstanceSQLServer);
			SetIsPrimaryIstance(SelectedIstanceSQLServer);
			
			if (!IsPrimaryIstance)
			{
				SetIstanceName(SelectedIstanceSQLServer);
				if (OnSelectedServerSQL != null)
					OnSelectedServerSQL(this, ServerName + Path.DirectorySeparatorChar + IstanceName);
			}
			else
			{
				IstanceName = string.Empty;
				if (OnSelectedServerSQL != null)
					OnSelectedServerSQL(this, ServerName);
			}
		}
		# endregion

		# region OnDropDown event
		//---------------------------------------------------------------------
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown(e);

			if (this.Items.Count > 1)
			{
				// non ricarico tutte le volte i server
				// ClearListOfSqlServer();
				// LoadIstanceSQL();

				string server = this.Text;

				int pos = this.FindStringExact(server, -1);
				if (pos >= 0)
				{
					this.SelectedIndex = pos;
					this.Text = server;
					SetServerName(server);
				}
			}
			else
			{
				ClearListOfSqlServer();
				LoadIstanceSQL();
			}

			//sparo un evento alla form che contiene il controllo
			if (OnDropDownServerSQL != null)
				OnDropDownServerSQL(this);
		}
		# endregion

		# region OnSelectedIndexChanged event
		//---------------------------------------------------------------------
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			string server = this.Text;

			try
			{
				if (IstanceName.Length == 0)
				{

					if (ServerName.Length > 0 && 
						string.Compare(server, ServerName, true, CultureInfo.InvariantCulture) != 0)
						if (OnClearOwnerDb != null)
							OnClearOwnerDb(this);
				}
				else
				{
					string serverPrimary = Path.GetDirectoryName(server);
					string serverIstance = Path.GetFileName(server);

					if (
						ServerName.Length > 0 && IstanceName.Length > 0 && 
						(string.Compare(serverPrimary, ServerName, true, CultureInfo.InvariantCulture) != 0 || 
						string.Compare(serverIstance, IstanceName, true, CultureInfo.InvariantCulture) != 0)
						)
						if (OnClearOwnerDb != null)
							OnClearOwnerDb(this);
				}
			}
			catch(System.NullReferenceException)
			{
			}
		}
		# endregion

		# region OnTextChanged event
		//---------------------------------------------------------------------
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
		}
		# endregion
		
		# endregion
	}
}
