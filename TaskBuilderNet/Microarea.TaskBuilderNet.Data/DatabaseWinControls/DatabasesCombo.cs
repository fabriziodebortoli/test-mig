using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	///<summary>
	/// DatabasesCombo
	/// ComboBox derivata per visualizzare l'elenco dei database disponibili su un server
	/// Non utilizza componenti SQLDMO
	///</summary>
	//=========================================================================
	public partial class DatabasesCombo : ComboBox
	{
		#region Variables
		private Diagnostic diagnostic	= new Diagnostic("DatabasesCombo");
		private DBMSType providerType	= DBMSType.SQLSERVER;

		private string	dataSourceName	= string.Empty;
		private string	serverName		= string.Empty;
		private string	userName		= string.Empty;
		private string	userPassword	= string.Empty;
		private int		port			= 0;
		private bool	isWindowsAuth	= false;
		#endregion

		#region Properties
		public int DBItems				{ get { return this.Items.Count; } }
		public string ServerName		{ get { return serverName; } set { serverName = value; } }
		public string DataSourceName	{ get { return dataSourceName; } set { dataSourceName = value; } }
		public string UserName			{ get { return userName; } set { userName = value; } }
		public string UserPassword		{ get { return userPassword; } set { userPassword = value; } }
		public int PortNumber			{ get { return port; } set { port = value; } }
		public bool IsWindowsAuthentication { get { return isWindowsAuth; } set { isWindowsAuth = value; } }

		public DBMSType ProviderType { get { return providerType; } set { providerType = value; } }
		public Diagnostic Diagnostic { get { return diagnostic; } set { diagnostic = value; } }
		#endregion

		#region Events and delegates
		public delegate void DropDownDatabases(object sender);
		public event DropDownDatabases OnDropDownDatabases;

		public delegate void SelectDatabases(string dbName);
		public event SelectDatabases OnSelectDatabases;
		#endregion

		# region Costruttori
		//---------------------------------------------------------------------
		public DatabasesCombo()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public DatabasesCombo(IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}
		# endregion

		#region Funzioni Pubbliche richiamabili al di fuori dello User Control

		#region IstanceItem - Ritorna il db specificato da pos all'interno della collection dei dbs
		/// <summary>
		/// IstanceItem
		/// </summary>
		//---------------------------------------------------------------------
		public string IstanceItem(int pos)
		{
			this.SelectedIndex = pos;
			return (string)this.SelectedItem;
		}
		#endregion

		#region SelectIndex - Imposta il db nella posizione pos all'interno della collection, come il db corrente
		/// <summary>
		/// SelectIndex
		/// </summary>
		/// <param name="pos"></param>
		//---------------------------------------------------------------------
		public void SelectIndex(int pos)
		{
			this.SelectedIndex = pos;
			DataSourceName = (this.SelectedItem != null) ? this.SelectedItem.ToString() : string.Empty;
		}
		#endregion

		#region LoadAllDataBases - Dato il Server, carica tutti i DBs di quel server
		//---------------------------------------------------------------------
		public bool LoadAllDataBases(string databaseNameToSkip = "")
		{
			if (providerType == DBMSType.SQLSERVER)
				return LoadAllSQLDatabases(databaseNameToSkip);

			return false;
		}

		/// <summary>
		/// LoadAllSQLDatabases
		/// carica i database di tipo SQL
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoadAllSQLDatabases(string databaseNameToSkip)
		{
			if (string.IsNullOrWhiteSpace(serverName))
			{
				// Il nome del server è vuoto, non carico niente
				diagnostic.Set(DiagnosticType.Error, DatabaseWinControlsStrings.NoServerSelected);
				return false;
			}

			if (string.IsNullOrWhiteSpace(userName))
			{
				// Il nome della login è vuoto, non carico niente
				diagnostic.Set(DiagnosticType.Error, DatabaseWinControlsStrings.NoLoginSelected);
				return false;
			}

			// Pulisco la lista dei DBs
			ClearListOfDb();
			DataSourceName = string.Empty;

			string connectionString = (isWindowsAuth)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, userName, userPassword);

			try
			{
				using (TBConnection myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					// skippo i database di sistema e di esempio di SQL
					string query = @"SELECT name FROM sysdatabases
									WHERE name NOT IN ('master','model','msdb','tempdb','pubs','Northwind','AdventureWorks','AdventureWorksLT','ReportServer','ReportServerTempDB')";

					using (TBCommand myCommand = new TBCommand(query, myConnection))
					{
						using (IDataReader myReader = myCommand.ExecuteReader())
						{
							while (myReader.Read())
							{
								string dbName = myReader["name"].ToString();

								// skippo il nome del db, se specificato
								if (!string.IsNullOrWhiteSpace(databaseNameToSkip) && string.Compare(dbName, databaseNameToSkip, StringComparison.InvariantCultureIgnoreCase) == 0)
									continue;

								this.Items.Add(dbName);
							}
						}
					}
				}
			}
			catch(TBException e)
			{
				string message = string.Format(DatabaseWinControlsStrings.CannotContactSQLServer, serverName);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, serverName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "LoadAllSQLDatabases");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseWinControls");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region ClearListOfDb - Pulisce la collection dei DBs
		/// <summary>
		/// ClearListOfDb
		/// </summary>
		//---------------------------------------------------------------------
		public void ClearListOfDb()
		{
			this.Items.Clear();
		}
		#endregion

		#region Add - Aggiunge un elemento alla collection dei DBs
		/// <summary>
		/// Add
		/// </summary>
		//---------------------------------------------------------------------
		public void Add(string itemToAdd)
		{
			this.Items.Clear();
			this.Items.Add(itemToAdd);
			this.SelectedItem = itemToAdd;
		}
		#endregion

		#endregion

		//---------------------------------------------------------------------
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown(e);
			OnDropDownDatabases?.Invoke(this);
		}

		//---------------------------------------------------------------------
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave (e);
		}

		//---------------------------------------------------------------------
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			if (this.SelectedItem != null)
			{
				DataSourceName = this.SelectedItem.ToString();
				if (OnSelectDatabases != null)
					OnSelectDatabases(DataSourceName);
			}
		}
	}
}
