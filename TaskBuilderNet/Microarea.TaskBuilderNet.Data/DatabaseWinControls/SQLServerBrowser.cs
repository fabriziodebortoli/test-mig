using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;
using Microsoft.Win32;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	///<summary>
	/// Form di visualizzazione dei server SQL disponibili, stile SQL Server Management Studio 2008
	/// In una tab sono visualizzate le istanze locali, mentre nella seconda tab, su un thread separato,
	/// sono caricati tutti i nomi e la versione dei server in rete (che hanno attivo il servizio SQL Server Browser)
	///</summary>
	// ========================================================================
	public partial class SQLServerBrowser : Form
	{
		private List<string> networkServers = new List<string>();
		private bool isRetrievingData = false;
		private Thread loadingThread;

		private string selectedServer = string.Empty;
		public string SelectedServer { get { return selectedServer; } }

		//---------------------------------------------------------------------
		public SQLServerBrowser()
		{
			InitializeComponent();
		}

		///<summary>
		/// Load della form carico subito i server locali (leggendo dal registry) e poi carico quelli sulla rete
		///</summary>
		//---------------------------------------------------------------------
		private void SQLServerBrowser_Load(object sender, EventArgs e)
		{
			// carico subito i server locali leggendo dal registry
			LoadLocalServers();

			// su un thread separato carico i server di rete
			loadingThread = new Thread(new ThreadStart(LoadNetworkServers));
			loadingThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			loadingThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			loadingThread.Start();
		}

		///<summary>
		/// Leggo nel registry i nomi delle istanze locali di SQL Server
		///</summary>
		//---------------------------------------------------------------------
		private void LoadLocalServers()
		{
			try
			{
				RegistryKey rk = RegisterKeyChecker.GetRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Microsoft SQL Server");
				String[] instances = (String[])rk.GetValue("InstalledInstances");

				if (instances != null && instances.Length > 0)
				{
					foreach (String element in instances)
					{
						if (string.Compare(element, "MSSQLSERVER", StringComparison.InvariantCultureIgnoreCase) == 0)
							LocalServersListBox.Items.Add(System.Net.Dns.GetHostName().ToUpperInvariant());
						else
							LocalServersListBox.Items.Add(Path.Combine(System.Net.Dns.GetHostName().ToUpperInvariant(), element));
					}
				}
			}
			catch 
			{
			}

			// nel dubbio seleziono il primo della lista
			if (LocalServersListBox.Items.Count > 0)
				LocalServersListBox.SelectedIndex = 0;
		}

		///<summary>
		/// Su un thread separato utilizzo il SqlDataSourceEnumerator per caricare
		/// i server SQL disponibili in rete (che hanno il servizio SQL Server Browser attivo)
		///</summary>
		//---------------------------------------------------------------------
		private void LoadNetworkServers()
		{
			networkServers.Clear();

			isRetrievingData = true;

			try
			{
				SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
				DataTable table = instance.GetDataSources();
				if (table == null)
					return;

				string serverName, instanceName, version;

				foreach (DataRow row in table.Rows)
				{
					serverName = string.Empty;
					instanceName = string.Empty;
					version = string.Empty;

					foreach (DataColumn col in table.Columns)
					{
						if (string.Compare(col.ColumnName, "ServerName", StringComparison.InvariantCultureIgnoreCase) == 0)
							serverName = row[col].ToString();

						if (string.Compare(col.ColumnName, "InstanceName", StringComparison.InvariantCultureIgnoreCase) == 0)
							instanceName = row[col].ToString();

						if (string.Compare(col.ColumnName, "Version", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							string[] tokenVer = row[col].ToString().Split('.');
							if (tokenVer != null && tokenVer.Length >= 2)
								version = string.Concat("(", tokenVer[0], ".", tokenVer[1], ")");
						}
					}

					// aggiungo, se esistenti, il nome istanza e i primi 2 numeri di versione
					if (!string.IsNullOrEmpty(serverName))
						networkServers.Add
							(
							(instanceName.Length > 0)
							? Path.Combine(serverName, instanceName) + ((version.Length > 0) ? ("\t" + version) : string.Empty)
							: serverName + ((version.Length > 0) ? ("\t" + version) : string.Empty)
							);
				}
			}
			catch
			{
			}

			// ordine alfabetico
			networkServers.Sort();

			isRetrievingData = false;

			// ho finito di caricare i server, quindi li mostro
			PopulateNetworkServersTab();
		}

		///<summary>
		/// Quando clicco sulla tab dei server di rete mostro l'elenco oppure il fatto che li sto caricando
		///</summary>
		//---------------------------------------------------------------------
		private void NetworkServersTab_Enter(object sender, EventArgs e)
		{
			try
			{
				if (isRetrievingData)
				{
					if (NetworkServersListBox.Items.Count == 0)
						NetworkServersListBox.Items.Add(DatabaseWinControlsStrings.RetrievingDataFromNetwork);
				}
				else
					PopulateNetworkServersTab();
			}
			catch
			{
			}
		}

		///<summary>
		/// Chiamata thread-safe per visualizzare la tab con l'elenco dei server di rete
		///</summary>
		//---------------------------------------------------------------------
		private void PopulateNetworkServersTab()
		{
			// se l'handle della form e' stato creato allora visualizzo le info (nel caso in cui chiudo la form
			// prima che il thread abbia terminato l'elaborazione)
			try
			{
				if (this.IsHandleCreated)
					BeginInvoke((ThreadStart)delegate
					{
						NetworkServersListBox.BeginUpdate();
						NetworkServersListBox.DataSource = networkServers;
						NetworkServersListBox.EndUpdate();
					});
			}
			catch
			{
			}
		}

		///<summary>
		/// Sul click del pulsante Ok imposto la proprieta' che ritorna il server selezionato
		/// e poi chiudo la finestra
		///</summary>
		//---------------------------------------------------------------------
		private void OKButton_Click(object sender, EventArgs e)
		{
			if (ServersTabControl.SelectedTab == LocalServersTab)
				selectedServer = (LocalServersListBox.SelectedItem != null) 
					? LocalServersListBox.SelectedItem.ToString()
					: System.Net.Dns.GetHostName().ToUpperInvariant();

			if (ServersTabControl.SelectedTab == NetworkServersTab)
			{
				if (NetworkServersListBox.SelectedItem != null)
				{
					int idxTab = NetworkServersListBox.SelectedItem.ToString().IndexOf("\t");
					selectedServer = (idxTab > 0) 
					? NetworkServersListBox.SelectedItem.ToString().Substring(0, idxTab)
					: NetworkServersListBox.SelectedItem.ToString();
				}
				else
					selectedServer = System.Net.Dns.GetHostName().ToUpperInvariant();
			}

			this.Close();
		}

		///<summary>
		/// Sul click del pulsante Cancel chiudo la finestra
		///</summary>
		//---------------------------------------------------------------------
		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		///<summary>
		/// Sulla chiusura della form chiudo il thread, per evitare che mi rimanga appeso
		///</summary>
		//---------------------------------------------------------------------
		private void SQLServerBrowser_FormClosing(object sender, FormClosingEventArgs e)
		{
			// nel dubbio sulla chiusura della form distruggo il thread
			try
			{
				if (loadingThread != null)
					loadingThread.Abort();
			}
			catch
			{
			}
		}
	}
}