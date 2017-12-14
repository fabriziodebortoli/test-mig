using System;
using System.Diagnostics;
using System.Security;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	///<summary>
    /// Visualizzazione dettagli del WebService TBSenderDetails
	///</summary>
	//================================================================================
	public partial class TBSenderDetails : Form
	{
		private int originalMsgColHeaderWidth = 0;
		private int nrEventsToShow = 300;

        private TbSenderWrapper tbsender = null;

		private ServicesLVItemComparer listviewComparer = new ServicesLVItemComparer();

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
        public TBSenderDetails(TbSenderWrapper tbsender, ImageList images)
		{
			InitializeComponent();

            this.tbsender = tbsender;

			originalMsgColHeaderWidth = MessageColHeader.Width;

			PBTbSender.Image = images.Images[PlugInTreeNode.GetTBSenderStateImageIndex];
			LwLogs.SmallImageList = LwLogs.LargeImageList = images;

            LblTbsenderSubtitle.Text = string.Format(Strings.OperationsTrace, NameSolverStrings.TbSender, nrEventsToShow.ToString());

			LwLogs.ListViewItemSorter = listviewComparer;
		}

		//--------------------------------------------------------------------------------
		private void TbSenderDetail_Load(object sender, EventArgs e)
		{
			// carico le informazioni dall'eventlog e le mostro nella listview
			LoadLogMessagesInListView();
		}

		///<summary>
		/// Caricamento info dall'EventLog di sistema
		///</summary>
		//--------------------------------------------------------------------------------
		private void LoadLogMessagesInListView()
		{
			LwLogs.Items.Clear();

			// prima controllo che l'EventLog esista
			if (!CheckTBSenderEventLog())
			{
				ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
				emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
				emptyItem.SubItems.Add(Strings.EventLogUnavailable);
				LwLogs.Items.Add(emptyItem);
				return;
			}

			EventLog theEventLog = new EventLog();
			theEventLog.Log = Diagnostic.EventLogName;
			theEventLog.Source = NameSolverStrings.TbSender;
			
			// se non ci sono entries non procedo
			if (theEventLog.Entries == null || theEventLog.Entries.Count == 0)
			{
				ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
				emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                emptyItem.SubItems.Add(String.Format(Strings.NoMsgAvailable, NameSolverStrings.TbSender));
				LwLogs.Items.Add(emptyItem);
				return;
			}

			LwLogs.BeginUpdate();

			int count = 0;

			Diagnostic currDiagnostic = null;
			DiagnosticType currDiagnosticType = DiagnosticType.Information;
			string explain = string.Empty;

			// gli eventi risultano ordinati cronologicamente dal più vecchio al più recente, quindi li scorro dal fondo
			for (int i = theEventLog.Entries.Count - 1; i >= 0; i--)
			{
				EventLogEntry entry = theEventLog.Entries[i];

                //// se il testo NON inizia con l'istanza corrente di installazione continuo (altrimenti vedo tutti i log insieme)
                //if (entry == null || 
                //    !entry.Message.StartsWith(InstallationData.InstallationName, StringComparison.InvariantCultureIgnoreCase))
                //    continue;

				// considero solo gli eventi di tbsender
				// oppure quelli di LoginManager che contengono la Init di tbsender
				if (
					string.Compare(entry.Source, NameSolverStrings.TbSender, StringComparison.InvariantCultureIgnoreCase) == 0
					||
					(
						string.Compare(entry.Source, NameSolverStrings.LoginManager, StringComparison.InvariantCultureIgnoreCase) == 0 &&
						entry.Message.IndexOf(NameSolverStrings.TbSender) > 0
					))
				{
					count++;
					if (count > nrEventsToShow) // visualizzo solo i primi n messaggi
						break;
					
					ListViewItem entryItem = new ListViewItem(entry.EntryType.ToString());

					// assegno un'immagine a seconda del tipo messaggio
					switch (entry.EntryType)
					{
						case EventLogEntryType.Information:
							entryItem.ImageIndex = PlugInTreeNode.GetInformationStateImageIndex;
							currDiagnosticType = DiagnosticType.Information;
							break;
						case EventLogEntryType.Error:
							entryItem.ImageIndex = PlugInTreeNode.GetErrorStateImageIndex;
							currDiagnosticType = DiagnosticType.Error;
							break;
						case EventLogEntryType.Warning:
							entryItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
							currDiagnosticType = DiagnosticType.Warning;
							break;
					}

					explain = ReplaceInvalidMessageCharacters(entry.Message);
					entryItem.SubItems.Add(explain);
					entryItem.SubItems.Add(entry.TimeGenerated.ToShortDateString());
					entryItem.SubItems.Add(entry.TimeGenerated.ToShortTimeString());

					// creo un Diagnostico da assegnare al Tag del corrente ListViewItem
                    currDiagnostic = new Diagnostic("TbSender");
                    currDiagnostic.Set(currDiagnosticType, entry.TimeGenerated, explain);
					entryItem.Tag = currDiagnostic;

					LwLogs.Items.Add(entryItem);
				}
			}

			AdjustColumnsWidth();

			LwLogs.EndUpdate();
		}

		///<summary>
		/// Sostituisce i caratteri non validi nel testo del messaggio
		///</summary>
		//-------------------------------------------------------------------------------------------
		private string ReplaceInvalidMessageCharacters(string aText)
		{
			if (string.IsNullOrWhiteSpace(aText))
				return String.Empty;

			string newText = aText.Trim();
			newText = newText.Replace("\r\n", " ");
			newText = newText.Replace('\n', ' ');
			newText = newText.Replace('\t', ' ');
			newText = newText.Replace('\v', ' ');

			return newText;
		}

		///<summary>
		/// Effettua un aggiustamento della larghezza delle colonne sulla base dell'area disponibile
		///</summary>
		//-------------------------------------------------------------------------------------------
		private void AdjustColumnsWidth()
		{
			if (LwLogs.Columns == null || LwLogs.Columns.Count == 0 || MessageColHeader == null)
				return;

			int colswidth = 0;
			for (int i = 0; i < LwLogs.Columns.Count; i++)
				colswidth += LwLogs.Columns[i].Width;

			if (colswidth == LwLogs.DisplayRectangle.Width)
				return;

			int newMsgColumnWidth = MessageColHeader.Width + LwLogs.DisplayRectangle.Width - colswidth;
			newMsgColumnWidth = Math.Max(originalMsgColHeaderWidth, newMsgColumnWidth);

			if (newMsgColumnWidth != MessageColHeader.Width)
			{
				MessageColHeader.Width = newMsgColumnWidth;
				LwLogs.PerformLayout();
			}
		}

		//-------------------------------------------------------------------------------------------
		private bool CheckTBSenderEventLog()
		{
			try
			{
				// The Source can be any random string, but the name must be distinct from other
				// sources on the computer. It is common for the source to be the name of the 
				// application or another identifying string. An attempt to create a duplicated 
				// Source value throws an exception. However, a single event log can be associated
				// with multiple sources.
				if (EventLog.SourceExists(NameSolverStrings.TbSender))
				{
					string existingLogName = EventLog.LogNameFromSourceName(NameSolverStrings.TbSender, ".");
					if (String.Compare(Diagnostic.EventLogName, existingLogName, StringComparison.InvariantCultureIgnoreCase) == 0)
						return true;

                    EventLog.DeleteEventSource(NameSolverStrings.TbSender);
				}
                EventLog.CreateEventSource(NameSolverStrings.TbSender, Diagnostic.EventLogName);

				if (!EventLog.Exists(Diagnostic.EventLogName))
					return false;

				return true;
			}
			catch (SecurityException se)
			{
                Debug.Fail("EventSource creation failed in CheckLoginManagerEventLog: " + se.Message);
				return false;
			}
			catch (Exception exception)
			{
                Debug.Fail("EventSource creation failed in CheckLoginManagerEventLog: " + exception.Message);
				return false;
			}
		}

		///<summary>
		/// Sul doppio click di una riga della listview mostro l'eventuale Diagnostic memorizzato nel Tag
		///</summary>
		//---------------------------------------------------------------------------
		private void TBSenderLogListView_DoubleClick(object sender, EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null ||
				list.SelectedItems.Count == 0
				|| list.SelectedItems.Count > 1)
				return;

			ListViewItem item = list.SelectedItems[0];
			ShowDiagnosticOnListViewItem(item);
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

		///<summary>
		/// Dopo il resize della listview e ri-aggiusto la larghezza delle colonne
		///</summary>
		//--------------------------------------------------------------------------------
		private void TbSenderLogListView_Resize(object sender, EventArgs e)
		{
			AdjustColumnsWidth();
		}

		///<summary>
		/// Intercetto l'F5 sulla listview e ricarico le righe
		///</summary>
		//--------------------------------------------------------------------------------
		private void TbSenderLogListView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5 && e.Modifiers == Keys.None)
				LoadLogMessagesInListView();
		}

		//--------------------------------------------------------------------------------
		private void TbSenderLogListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Determine if clicked column is already the column that is being sorted.
			if (e.Column == listviewComparer.SortColumn)
			{
				// Reverse the current sort direction for this column.
				if (listviewComparer.Order == SortOrder.Ascending)
					listviewComparer.Order = SortOrder.Descending;
				else
					listviewComparer.Order = SortOrder.Ascending;
			}
			else
			{
				// Set the column number that is to be sorted; default to ascending.
				listviewComparer.SortColumn = e.Column;
				listviewComparer.Order = SortOrder.Ascending;
			}

			// Perform the sort with these new sort options.
			LwLogs.Sort();

			// consente di visualizzare la freccina nel columnheader
			//LwLogs.SetSortIcon(e.Column, listviewComparer.Order);
		}

		///<summary>
		/// Clicco sul menu di contesto per eseguire il ricaricamento dei messaggi dell'EventViewer
		///</summary>
		//--------------------------------------------------------------------------------
		private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LoadLogMessagesInListView();
		}

		

        ////--------------------------------------------------------------------------------
        //private void TabDetails_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (((TabControl)sender).SelectedTab == TabPageOpTrace)
        //    {
        //        EASubtitleLabel.Text = string.Format(Strings.EAOperationsTrace, nrEventsToShow.ToString());
        //        LoadLogMessagesInListView(); // rieseguo la load dei messaggi
        //    }

        //    if (((TabControl)sender).SelectedTab == TabPageActions)
        //        EASubtitleLabel.Text = Strings.EAActions;
        //}

		///<summary>
		/// Eseguo la clear della struttura caricata in memoria ad uso del web service
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnClear_Click(object sender, EventArgs e)
		{
            //if (this.tbsender.Clear())
            //    MessageBox.Show(Strings.EAClearStructureWithSuccess, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //else
            //    MessageBox.Show(Strings.EAClearStructureWithError, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		///<summary>
		/// Eseguo la Init del web service
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnInit_Click(object sender, EventArgs e)
		{
			if (this.tbsender.Init())
				MessageBox.Show(Strings.WSInitWithSuccess, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
			else
				MessageBox.Show(Strings.WSInitWithError, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		///<summary>
		/// Stoppo il web service
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnStop_Click(object sender, EventArgs e)
        {
        //    if (this.tbsender.Stop())
        //        MessageBox.Show(Strings.EAStopServiceWithSuccess, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    else
        //        MessageBox.Show(Strings.EAStopServiceWithError, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}
}