using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	//========================================================================= 
	public partial class LogViewer : Form
	{
		private IBasePathFinder pathFinder = null;

		private string customCompaniesDir = string.Empty;
		private List<CompanyFolderItem> availableCompanies = new List<CompanyFolderItem>();

		private DateTimePicker fromDatePicker = new DateTimePicker();
		private DateTimePicker toDatePicker = new DateTimePicker();

		private DiagnosticView diagnosticV = null;
		private Diagnostic diagnostic = null;

		// Declare a Hashtable array in which to store the groups.
		private Hashtable[] groupTables;
		// Declare a variable to store the current grouping column.
		int groupColumn = 0;

		///<summary> 
		/// Costruttore
		///</summary>
		//---------------------------------------------------------------------
		public LogViewer(IBasePathFinder pathFinder)
		{
			InitializeComponent();

			// la data di partenza è impostata con il 1° del mese corrente
			fromDatePicker.Value = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

			// aggancio gli eventi del DateTimePicker
			fromDatePicker.ValueChanged += new EventHandler(FromDatePicker_ValueChanged);
			toDatePicker.ValueChanged += new EventHandler(ToDatePicker_ValueChanged);
			fromDatePicker.CloseUp += new EventHandler(DatePicker_CloseUp);
			toDatePicker.CloseUp += new EventHandler(DatePicker_CloseUp);

			FiltersToolStrip.Items.Insert(1, new ToolStripControlHost(fromDatePicker));
			FiltersToolStrip.Items.Add(new ToolStripControlHost(toDatePicker));

			this.pathFinder = pathFinder;

			// carico i file di log da file system
			LoadAvailableLogFiles();
			// inizializzo la listview con i file 
			InitFilesListView();
		}

		# region Lettura files e creazione struttura in memoria (azienda-utente-file)
		///<summary>
		/// Carico da File System tutte le directory sotto la Custom\Companies ed estraggo tutti 
		/// i file che sono potenziali file di log
		///</summary>
		//---------------------------------------------------------------------
		private void LoadAvailableLogFiles()
		{
			customCompaniesDir = pathFinder.GetCustomCompaniesPath();

			DirectoryInfo dir = new DirectoryInfo(customCompaniesDir);
			CompanyFolderItem ci = null;
			UserFolderItem ui = null;
			FileLogItem fi = null;

			availableCompanies.Clear();
			string customCompanyPath = string.Empty;

			if (!dir.Exists)
				return;

			// se e solo se la directory esiste... allora cerco le companies al suo interno
			foreach (DirectoryInfo info in dir.GetDirectories())
			{
				customCompanyPath = pathFinder.GetCustomCompanyLogPath(info.Name);

				if (!Directory.Exists(customCompanyPath))
					continue;

				ci = new CompanyFolderItem(info.Name);
				ci.CustomLogPath = customCompanyPath;

				dir = new DirectoryInfo(ci.CustomLogPath);

				foreach (DirectoryInfo dInfo in dir.GetDirectories()) // il controllo di esistenza è già stato effettuato
				{
					if (string.Compare(dInfo.Name, NameSolverStrings.AllUsers, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						if (dInfo.GetFiles("*.xml").Length > 0)
						{
							ui = new UserFolderItem(dInfo.Name);

							foreach (FileInfo fInfo in dInfo.GetFiles("*.xml"))
							{
								fi = new FileLogItem(fInfo);
								if (fi.CheckLogItem())
									ui.Files.Add(fi);
							}

							if (ui.Files.Count > 0)
								ci.Users.Add(ui);
						}
						continue;
					}

					if (string.Compare(dInfo.Name, NameSolverStrings.Users, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						foreach (DirectoryInfo ddInfo in dInfo.GetDirectories())
						{
							if (ddInfo.GetFiles("*.xml").Length > 0)
							{
								ui = new UserFolderItem(ddInfo.Name);

								foreach (FileInfo fInfo in ddInfo.GetFiles("*.xml"))
								{
									fi = new FileLogItem(fInfo);
									if (fi.CheckLogItem())
										ui.Files.Add(fi);
								}

								if (ui.Files.Count > 0)
									ci.Users.Add(ui);
							}
						}
					}
				}

				availableCompanies.Add(ci);
			}
		}
		# endregion

		///<summary>
		/// Leggo le strutture in memoria che ho riempito e inserisco i dati relativi ai file nella listview
		///</summary>
		//---------------------------------------------------------------------
		private void InitFilesListView()
		{
			FilesListView.Items.Clear();

			DateTime fromDateSelected = fromDatePicker.Value;
			DateTime toDateSelected = toDatePicker.Value;

			ListViewItem item = null;

			foreach (CompanyFolderItem cfi in availableCompanies)
			{
				foreach (UserFolderItem ufi in cfi.Users)
				{
					foreach (FileLogItem fli in ufi.Files)
					{
						// carico solo i file che rispettano il range di date
						if (fli.CreationDate.Date > toDateSelected.Date ||
							fli.CreationDate.Date < fromDateSelected.Date)
							continue;

						// TODO: mettere delle icone agli item della list view.
						item = new ListViewItem(fli.FileLogType.ToString());

						switch (fli.FileLogType)
						{
							case LogType.Application:
								item.ImageIndex = 3;
								break;

							case LogType.Database:
								item.ImageIndex = 4;
								break;

							case LogType.ImportExport:
								item.ImageIndex = 10;
								break;

							case LogType.Generic:
							default:
								item.ImageIndex = 5;
								break;
						}

						item.SubItems.Add(fli.FInfo.Name);
						item.SubItems.Add(cfi.Name);
						item.SubItems.Add(ufi.Name);
						item.SubItems.Add(fli.CreationDate.ToShortDateString());
						item.Tag = fli;
						FilesListView.Items.Add(item);
					}
				}
			}

			// Create the groupsTable array and populate it with one hash table for each column.
			groupTables = new Hashtable[FilesListView.Columns.Count];

			// Create a hash table containing all the groups needed for a single column.
			for (int column = 0; column < FilesListView.Columns.Count; column++)
				groupTables[column] = CreateGroupsTable(column);

			// Start with the groups created for the Title column.
			SetGroups(0);
		}

		# region Metodi per la visualizzazione del diagnostico
		/// <summary>
		/// LoadDiagnostic
		/// Visualizzo la diagnostica contenuta nel file selezionato
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadDiagnostic(Diagnostic diagnostic, DiagnosticType filterType)
		{
			MessagesListView.Items.Clear();
			MessagesListView.BeginUpdate();

			DiagnosticItems items = diagnostic.AllMessages() as DiagnosticItems;
			items.Reverse(); // xchè le info nel file sono in ordinamento decrescente

			Diagnostic currDiagnostic = null;
			string explain = string.Empty;

			if (items != null)
			{
				foreach (DiagnosticItem item in items)
				{
					ListViewItem itemListView = new ListViewItem();

					// filtro le info a seconda della scelta dell'utente
					if ((filterType & item.Type) != item.Type)
						continue;

					switch (item.Type)
					{
						case DiagnosticType.Error:
							itemListView.ImageIndex = 0;
							break;
						case DiagnosticType.Warning:
							itemListView.ImageIndex = 1;
							break;
						case DiagnosticType.Information:
							itemListView.ImageIndex = 2;
							break;
					}

					itemListView.Text = item.Occurred.ToShortDateString();
					explain = item.FullExplain.Replace("\n", ""); // per evitare di visualizzare i quadratini nel testo
					itemListView.SubItems.Add(item.Occurred.ToLongTimeString());
					itemListView.SubItems.Add(explain);

					currDiagnostic = new Diagnostic("MessagesListView");
					currDiagnostic.Set(item.Type, explain, item.ExtendedInfo);
					itemListView.Tag = currDiagnostic; // creo un diagnostico da assegnare al singolo item

					MessagesListView.Items.Add(itemListView);
				}

				int totErrors = diagnostic.TotalErrors;
				if (totErrors > 1)
					totErrors--;

				TotErrorsToolStripLabel.Text = string.Format(DiagnosticViewerStrings.ErrorsAndWarnings, totErrors.ToString(), diagnostic.TotalWarnings.ToString());
			}

			MessagesListView.EndUpdate();
		}

		/// <summary>
		/// mostro un Diagnostico relativo all'item selezionato nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void ShowDiagnosticOnListViewItem(ListViewItem currItem)
		{
			if (currItem != null && currItem.Tag != null)
				DiagnosticViewer.ShowDiagnostic(((Diagnostic)(currItem.Tag)));
		}
		# endregion

		# region Gestione toolbar con i filtri Error - Warning - Information
		/// <summary>
		/// in base ai pulsanti della toolbar con Checked=true stabilisco come filtrare le info del Diagnostic
		/// </summary>
		//---------------------------------------------------------------------------
		private DiagnosticType EvaluateDiagnosticType()
		{
			DiagnosticType filterType = DiagnosticType.None;

			bool toViewErrors = ErrorToolStripButton.Checked;
			bool toViewWarnings = WarningToolStripButton.Checked;
			bool toViewInformations = InfoToolStripButton.Checked;

			filterType = (toViewErrors) ? DiagnosticType.Error : filterType;
			filterType = (toViewWarnings) ? filterType | DiagnosticType.Warning : filterType;
			filterType = (toViewInformations) ? filterType | DiagnosticType.Information : filterType;

			return filterType;
		}

		//---------------------------------------------------------------------------
		private void ChangeFilterToolbar()
		{
			if (FilesListView.SelectedItems == null || FilesListView.SelectedItems.Count == 0)
				return;

			ListViewItem itemActivated = FilesListView.SelectedItems[0];

			if (itemActivated == null || itemActivated.Tag == null)
				return;

			if (((FileLogItem)(itemActivated.Tag)).FileDiagnostic == null)
				return;

			LoadDiagnostic(((FileLogItem)(itemActivated.Tag)).FileDiagnostic, EvaluateDiagnosticType());
		}

		// Filtro Error
		//---------------------------------------------------------------------------
		private void ErrorToolStripButton_CheckedChanged(object sender, EventArgs e)
		{
			ChangeFilterToolbar();
		}

		// Filtro Warning
		//---------------------------------------------------------------------------
		private void WarningToolStripButton_CheckedChanged(object sender, EventArgs e)
		{
			ChangeFilterToolbar();
		}

		// Filtro Information
		//---------------------------------------------------------------------------
		private void InfoToolStripButton_CheckedChanged(object sender, EventArgs e)
		{
			ChangeFilterToolbar();
		}
		# endregion

		# region Eventi sulle listview della form
		///<summary>
		/// Attivo un item nella listview dei file e carico il contenuto
		///</summary>
		//---------------------------------------------------------------------
		private void FilesListView_ItemActivate(object sender, EventArgs e)
		{
			if (FilesListView.SelectedItems.Count == 0)
				return;

			ListViewItem itemActivated = FilesListView.SelectedItems[0];

			if (itemActivated == null || itemActivated.Tag == null)
				return;

			if (((FileLogItem)(itemActivated.Tag)).FileDiagnostic == null)
			{
				diagnostic = new Diagnostic(((FileLogItem)(itemActivated.Tag)).FInfo.Name);
				diagnosticV = new DiagnosticView(diagnostic);
				diagnosticV.ReadXmlFile(((FileLogItem)(itemActivated.Tag)).FInfo.FullName);
				((FileLogItem)(itemActivated.Tag)).FileDiagnostic = diagnostic;
			}

			LoadDiagnostic(((FileLogItem)(itemActivated.Tag)).FileDiagnostic, EvaluateDiagnosticType());
		}

		///<summary>
		/// Cambio di selezione dell'item sulla list view dei files (per aggiornare la visualizzazione del contenuto)
		///</summary>
		//---------------------------------------------------------------------------
		private void FilesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (e.IsSelected)
			{
				ListViewItem itemActivated = e.Item;

				if (itemActivated == null || itemActivated.Tag == null)
					return;

				if (((FileLogItem)(itemActivated.Tag)).FileDiagnostic == null)
				{
					diagnostic = new Diagnostic(((FileLogItem)(itemActivated.Tag)).FInfo.Name);
					diagnosticV = new DiagnosticView(diagnostic);
					diagnosticV.ReadXmlFile(((FileLogItem)(itemActivated.Tag)).FInfo.FullName);
					((FileLogItem)(itemActivated.Tag)).FileDiagnostic = diagnostic;
				}

				LoadDiagnostic(((FileLogItem)(itemActivated.Tag)).FileDiagnostic, EvaluateDiagnosticType());
			}
		}

		///<summary>
		/// Ordinamento per colonna sulla list view dei files
		///</summary>
		//---------------------------------------------------------------------------
		private void FilesListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Set the sort order to ascending when changing
			// column groups; otherwise, reverse the sort order.
			if (FilesListView.Sorting == SortOrder.Descending || e.Column != groupColumn)
				FilesListView.Sorting = SortOrder.Ascending;
			else
				FilesListView.Sorting = SortOrder.Descending;

			groupColumn = e.Column;

			// Set the groups to those created for the clicked column.
			SetGroups(e.Column);
		}

		///<summary>
		/// Doppio click sulla listview dei messaggi
		///</summary>
		//---------------------------------------------------------------------
		private void MessagesListView_DoubleClick(object sender, EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null ||
				list.SelectedItems.Count == 0
				|| list.SelectedItems.Count > 1)
				return;

			ListViewItem item = list.SelectedItems[0];
			ShowDiagnosticOnListViewItem(item);
		}
		# endregion

		# region Gestione gruppi per la listview dei files (raggruppamento per tipi colonna)
		///<summary>
		/// Sets myListView to the groups created for the specified column.
		///</summary>
		//---------------------------------------------------------------------
		private void SetGroups(int column)
		{
			// Remove the current groups.
			FilesListView.Groups.Clear();

			// Retrieve the hash table corresponding to the column.
			Hashtable groups = (Hashtable)groupTables[column];

			// Copy the groups for the column to an array.
			ListViewGroup[] groupsArray = new ListViewGroup[groups.Count];
			groups.Values.CopyTo(groupsArray, 0);

			if (column == 4) // si tratta della colonna di tipo data
			{
				// Sort the groups and add them to myListView.
				Array.Sort(groupsArray, new ListViewDateTimeGroupSorter(FilesListView.Sorting));
				FilesListView.Groups.AddRange(groupsArray);
			}
			else
			{
				// Sort the groups and add them to myListView.
				Array.Sort(groupsArray, new ListViewGroupSorter(FilesListView.Sorting));
				FilesListView.Groups.AddRange(groupsArray);
			}

			// Iterate through the items in myListView, assigning each one to the appropriate group.
			foreach (ListViewItem item in FilesListView.Items)
			{
				// Retrieve the subitem text corresponding to the column.
				string subItemText = item.SubItems[column].Text;

				// For the Title column, use only the first letter.
				//				if (column == 0) subItemText = subItemText.Substring(0, 1);

				// Assign the item to the matching group.
				item.Group = (ListViewGroup)groups[subItemText];
			}
		}

		///<summary>
		/// Creates a Hashtable object with one entry for each unique subitem value 
		/// (or initial letter for the parent item) in the specified column.
		///</summary>
		//---------------------------------------------------------------------
		private Hashtable CreateGroupsTable(int column)
		{
			// Create a Hashtable object.
			Hashtable groups = new Hashtable();

			// Iterate through the items in myListView.
			foreach (ListViewItem item in FilesListView.Items)
			{
				// Retrieve the text value for the column.
				string subItemText = item.SubItems[column].Text;

				// Use the initial letter instead if it is the first column.
				//				if (column == 0) subItemText = subItemText.Substring(0, 1);

				// If the groups table does not already contain a group
				// for the subItemText value, add a new group using the 
				// subItemText value for the group header and Hashtable key.
				if (!groups.Contains(subItemText))
					groups.Add(subItemText, new ListViewGroup(subItemText, HorizontalAlignment.Left));
			}

			// Return the Hashtable object.
			return groups;
		}
		# endregion

		# region DateTimePicker events
		/// <summary>
		/// Check sul range della data
		/// </summary>
		//---------------------------------------------------------------------------
		private void FromDatePicker_ValueChanged(object sender, System.EventArgs e)
		{
			toDatePicker.MinDate = ((DateTimePicker)sender).Value;
		}

		/// <summary>
		/// Check sul range della data
		/// </summary>
		//---------------------------------------------------------------------------
		private void ToDatePicker_ValueChanged(object sender, System.EventArgs e)
		{
			fromDatePicker.MaxDate = ((DateTimePicker)sender).Value;
		}

		/// <summary>
		/// Sulla chiusura del controllo ricarico i dati nella list-view
		/// </summary>
		//---------------------------------------------------------------------------
		private void DatePicker_CloseUp(object sender, System.EventArgs e)
		{
			InitFilesListView();
		}
		# endregion

		# region Menu strip associato al ListViewItem della FileListView
		///<summary>
		/// Apre il file con Internet Explorer
		///</summary>
		//---------------------------------------------------------------------------
		private void OpenWithInternetExplorerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (FilesListView.SelectedItems == null || FilesListView.SelectedItems.Count == 0 || FilesListView.SelectedItems.Count > 1)
				return;

			ListViewItem selItem = FilesListView.SelectedItems[0];

			if (selItem != null && selItem.Tag != null)
			{
				string path = ((FileLogItem)(selItem.Tag)).FInfo.FullName;

				if (File.Exists(path))
					Process.Start(path);
			}
		}

		///<summary>
		/// Copia il path del file nella clipboard
		///</summary>
		//---------------------------------------------------------------------------
		private void CopyPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (FilesListView.SelectedItems == null ||
				FilesListView.SelectedItems.Count == 0 || 
				FilesListView.SelectedItems.Count > 1)
				return;

			ListViewItem selItem = FilesListView.SelectedItems[0];

			if (selItem != null && selItem.Tag != null)
			{
				string path = ((FileLogItem)(selItem.Tag)).FInfo.FullName;

				if (File.Exists(path))
					Clipboard.SetDataObject(path);
			}
		}

		///<summary>
		/// Sposta il file nel Recycle Bin
		///</summary>
		//---------------------------------------------------------------------------
		private void FilesDeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (FilesListView.SelectedItems == null ||
				FilesListView.SelectedItems.Count == 0
				|| FilesListView.SelectedItems.Count > 1)
				return;

			ListViewItem selItem = FilesListView.SelectedItems[0];

			if (selItem != null && selItem.Tag != null)
			{
				string path = ((FileLogItem)(selItem.Tag)).FInfo.FullName;
				string name = ((FileLogItem)(selItem.Tag)).FInfo.Name;

				// se il file esiste chiedo se si desidera eliminarlo.
				if (File.Exists(path))
				{
					DialogResult dr =
						MessageBox.Show
						(
						string.Format(DiagnosticViewerStrings.DeleteFile, name),
						DiagnosticViewerStrings.ConfirmFileDelete,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question
						);

					if (dr == DialogResult.Yes)
					{
						File.Delete(path);

						// ri-carico i file di log da file system
						LoadAvailableLogFiles();
						// re-inizializzo la listview con i file 
						InitFilesListView();
					}
				}
			}
		}
		# endregion
	}

	# region ListViewGroupSorter class
	///<summary>
	/// Sorts ListViewGroup objects by header value.
	/// Per il sorting degli item della list view
	///</summary>
	//========================================================================= 
	public class ListViewGroupSorter : IComparer
	{
		private SortOrder order;

		// Stores the sort order.
		//---------------------------------------------------------------------
		public ListViewGroupSorter(SortOrder theOrder)
		{
			order = theOrder;
		}

		// Compares the groups by header value, using the saved sort order to return the correct value.
		//---------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			int result = String.Compare(((ListViewGroup)x).Header, ((ListViewGroup)y).Header);

			if (order == SortOrder.Ascending)
				return result;
			else
				return -result;
		}
	}
	# endregion

	# region ListViewDateTimeGroupSorter class
	///<summary>
	/// Per il sorting dell'item di tipo data della list view
	///</summary>
	//========================================================================= 
	public class ListViewDateTimeGroupSorter : IComparer
	{
		private SortOrder order;

		// Stores the sort order.
		//---------------------------------------------------------------------
		public ListViewDateTimeGroupSorter(SortOrder theOrder)
		{
			order = theOrder;
		}

		// Compares the groups by header value, using the saved sort order to return the correct value.
		//---------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			DateTime from = Convert.ToDateTime(((ListViewGroup)x).Header);
			DateTime to = Convert.ToDateTime(((ListViewGroup)y).Header);

			int result = DateTime.Compare(from, to);

			if (order == SortOrder.Ascending)
				return result;
			else
				return -result;
		}
	}
	# endregion

	///<summary>
	/// CompanyFolderItem - classe per una generica azienda sotto la Custom\Companies
	///</summary>
	//========================================================================= 
	public class CompanyFolderItem
	{
		public string Name = string.Empty;
		public string CustomLogPath = string.Empty;

		public List<UserFolderItem> Users = new List<UserFolderItem>();

		//---------------------------------------------------------------------
		public CompanyFolderItem(string name)
		{
			this.Name = name;
		}
	}

	///<summary>
	/// UserFolderItem - classe per un generico utente
	///</summary>
	//========================================================================= 
	public class UserFolderItem
	{
		public string Name = string.Empty;
		public List<FileLogItem> Files = new List<FileLogItem>();

		//---------------------------------------------------------------------
		public UserFolderItem(string name)
		{
			this.Name = name;
		}
	}

	///<summary>
	/// FileLogItem - classe per gestire un singolo file di log
	///</summary>
	//========================================================================= 
	public class FileLogItem
	{
		public FileInfo FInfo = null;

		public DateTime CreationDate = DateTime.UtcNow;
		public LogType FileLogType = LogType.Generic;

		public Diagnostic FileDiagnostic = null;

		//---------------------------------------------------------------------
		public FileLogItem(FileInfo file)
		{
			FInfo = file;
		}

		///<summary>
		/// Apro un XmlReader e leggo gli attributi per capire se si tratta di un vero e proprio file di log
		///</summary>
		//---------------------------------------------------------------------
		public bool CheckLogItem()
		{
			bool isLogFile = false;

			try
			{
				StreamReader sr = File.OpenText(this.FInfo.FullName);
				XmlReader xReader = XmlReader.Create(sr);

				while (xReader.Read())
				{
					if (string.Compare(xReader.Name, "File", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						CreationDate = Convert.ToDateTime(xReader.GetAttribute("creationdate"));

						switch (xReader.GetAttribute("logtype"))
						{
							case "Application":
								FileLogType = LogType.Application;
								break;

							case "Database":
								FileLogType = LogType.Database;
								break;

							case "Migration":
								FileLogType = LogType.Migration;
								break;

							case "WebService":
								FileLogType = LogType.WebService;
								break;

							case "ImportExport":
								FileLogType = LogType.ImportExport;
								break;

							case "Generic":
							default:
								FileLogType = LogType.Generic;
								break;
						}

						// se ho trovato l'elemento File con gli attributi creationdate e logtype 
						// allora si tratta di un file di log ed esco subito dal loop
						isLogFile = true;
						if (isLogFile)
							return isLogFile;
					}
				}
			}
			catch (XmlException e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
			}

			return isLogFile;
		}
	}
}