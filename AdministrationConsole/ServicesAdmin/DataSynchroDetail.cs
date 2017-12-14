﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    ///<summary>
    /// Visualizzazione dettagli del WebService DataSynchronizer
    ///</summary>
    //================================================================================
    public partial class DataSynchroDetail : Form
    {
        private int originalMsgColHeaderWidth = 0;
        private int nrEventsToShow = 300;

        private DataSynchronizer dataSynch = null;
        private LoginManager loginManager = null;

        private ServicesLVItemComparer dsLVComparer = new ServicesLVItemComparer();

        ///<summary>
        /// Constructor
        ///</summary>
        //--------------------------------------------------------------------------------
        public DataSynchroDetail(DataSynchronizer eaSync, LoginManager loginManager, ImageList images)
        {
            InitializeComponent();

            this.dataSynch = eaSync;
            this.loginManager = loginManager;

            originalMsgColHeaderWidth = MessageColHeader.Width;

            DSPictureBox.Image = images.Images[PlugInTreeNode.GetDataSynchroStateImageIndex];
            DSLogListView.SmallImageList = DSLogListView.LargeImageList = images;

            LblSubtitle.Text = string.Format(Strings.OperationsTrace, NameSolverStrings.DataSynchronizer, nrEventsToShow.ToString());

            DSLogListView.ListViewItemSorter = dsLVComparer;
        }

        //--------------------------------------------------------------------------------
        private void DataSynchroDetail_Load(object sender, EventArgs e)
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
            DSLogListView.Items.Clear();

            // prima controllo che l'EventLog esista
            if (!CheckDataSynchEventLog())
            {
                ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
                emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                emptyItem.SubItems.Add(string.Format(Strings.WSEventLogUnavailable, NameSolverStrings.DataSynchronizer));
                DSLogListView.Items.Add(emptyItem);
                return;
            }

            EventLog eaEventLog = new EventLog();
            eaEventLog.Log = Diagnostic.EventLogName;
            eaEventLog.Source = NameSolverStrings.DataSynchronizer;

            // se non ci sono entries non procedo
            if (eaEventLog.Entries == null || eaEventLog.Entries.Count == 0)
            {
                ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
                emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                emptyItem.SubItems.Add(Strings.NoMsgAvailable);
                DSLogListView.Items.Add(emptyItem);
                return;
            }

            DSLogListView.BeginUpdate();

            int count = 0;

            Diagnostic currDiagnostic = null;
            DiagnosticType currDiagnosticType = DiagnosticType.Information;
            string explain = string.Empty;

            // gli eventi risultano ordinati cronologicamente dal più vecchio al più recente, quindi li scorro dal fondo
            for (int i = eaEventLog.Entries.Count - 1; i >= 0; i--)
            {
                EventLogEntry entry = eaEventLog.Entries[i];

                // se il testo NON inizia con l'istanza corrente di installazione continuo (altrimenti vedo tutti i log insieme)
                if (entry == null ||
                    !entry.Message.StartsWith(InstallationData.InstallationName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // considero solo gli eventi di DataSynchronizer
                // oppure quelli di LoginManager che contengono la Init di DataSynchronizer
                if (
                    string.Compare(entry.Source, NameSolverStrings.DataSynchronizer, StringComparison.InvariantCultureIgnoreCase) == 0
                    ||
                    (
                        string.Compare(entry.Source, NameSolverStrings.LoginManager, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                        entry.Message.IndexOf(NameSolverStrings.DataSynchronizer) > 0
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
                    currDiagnostic = new Diagnostic("DSLogLV");
                    currDiagnostic.Set(currDiagnosticType, explain);
                    entryItem.Tag = currDiagnostic;

                    DSLogListView.Items.Add(entryItem);
                }
            }

            AdjustColumnsWidth();

            DSLogListView.EndUpdate();
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
            if (DSLogListView.Columns == null || DSLogListView.Columns.Count == 0 || MessageColHeader == null)
                return;

            int colswidth = 0;
            for (int i = 0; i < DSLogListView.Columns.Count; i++)
                colswidth += DSLogListView.Columns[i].Width;

            if (colswidth == DSLogListView.DisplayRectangle.Width)
                return;

            int newMsgColumnWidth = MessageColHeader.Width + DSLogListView.DisplayRectangle.Width - colswidth;
            newMsgColumnWidth = Math.Max(originalMsgColHeaderWidth, newMsgColumnWidth);

            if (newMsgColumnWidth != MessageColHeader.Width)
            {
                MessageColHeader.Width = newMsgColumnWidth;
                DSLogListView.PerformLayout();
            }
        }

        //-------------------------------------------------------------------------------------------
        private bool CheckDataSynchEventLog()
        {
            try
            {
                // The Source can be any random string, but the name must be distinct from other
                // sources on the computer. It is common for the source to be the name of the 
                // application or another identifying string. An attempt to create a duplicated 
                // Source value throws an exception. However, a single event log can be associated
                // with multiple sources.
                if (EventLog.SourceExists(NameSolverStrings.DataSynchronizer))
                {
                    string existingLogName = EventLog.LogNameFromSourceName(NameSolverStrings.DataSynchronizer, ".");
                    if (String.Compare(Diagnostic.EventLogName, existingLogName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;

                    EventLog.DeleteEventSource(NameSolverStrings.DataSynchronizer);
                }
                EventLog.CreateEventSource(NameSolverStrings.DataSynchronizer, Diagnostic.EventLogName);

                if (!EventLog.Exists(Diagnostic.EventLogName))
                    return false;

                return true;
            }
            catch (SecurityException se)
            {
                Debug.Fail("EventSource creation failed in CheckDataSynchEventLog: " + se.Message);
                return false;
            }
            catch (Exception exception)
            {
                Debug.Fail("EventSource creation failed in CheckDataSynchEventLog: " + exception.Message);
                return false;
            }
        }

        ///<summary>
        /// Sul doppio click di una riga della listview mostro l'eventuale Diagnostic memorizzato nel Tag
        ///</summary>
        //---------------------------------------------------------------------------
        private void DSLogListView_DoubleClick(object sender, EventArgs e)
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
        private void DSLogListView_Resize(object sender, EventArgs e)
        {
            AdjustColumnsWidth();
        }

        ///<summary>
        /// Intercetto l'F5 sulla listview e ricarico le righe
        ///</summary>
        //--------------------------------------------------------------------------------
        private void DSLogListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 && e.Modifiers == Keys.None)
                LoadLogMessagesInListView();
        }

        //--------------------------------------------------------------------------------
        private void DSLogListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == dsLVComparer.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (dsLVComparer.Order == SortOrder.Ascending)
                    dsLVComparer.Order = SortOrder.Descending;
                else
                    dsLVComparer.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                dsLVComparer.SortColumn = e.Column;
                dsLVComparer.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            DSLogListView.Sort();

            // consente di visualizzare la freccina nel columnheader
            //EALogListView.SetSortIcon(e.Column, eaLVComparer.Order);
        }

        ///<summary>
        /// Clicco sul menu di contesto per eseguire il ricaricamento dei messaggi dell'EventViewer
        ///</summary>
        //--------------------------------------------------------------------------------
        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadLogMessagesInListView();
        }

        //--------------------------------------------------------------------------------
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // prima controllo che l'EventLog esista
            if (!CheckDataSynchEventLog())
                return;

            EventLog eaEventLog = new EventLog();
            eaEventLog.Log = Diagnostic.EventLogName;
            eaEventLog.Source = NameSolverStrings.DataSynchronizer;

            // carico gli ultimi messaggi
            LoadLogMessagesInListView();

            // se non ci sono entries non procedo
            if (eaEventLog.Entries == null || eaEventLog.Entries.Count == 0)
                return;

            if (MessageBox.Show(Strings.WarnClearEntriesInEventLog, string.Empty, MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            try
            {
                eaEventLog.Clear();
            }
            catch
            { }

            DSLogListView.BeginUpdate();
            DSLogListView.Items.Clear();
            DSLogListView.EndUpdate();
        }

        //--------------------------------------------------------------------------------
        private void TabDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)sender).SelectedTab == TabPageOpTrace)
            {
                LblSubtitle.Text = string.Format(Strings.OperationsTrace, NameSolverStrings.DataSynchronizer, nrEventsToShow.ToString());
                LoadLogMessagesInListView(); // rieseguo la load dei messaggi
            }

            if (((TabControl)sender).SelectedTab == TabPageActions)
                LblSubtitle.Text = string.Format(Strings.ActionsDescri, NameSolverStrings.DataSynchronizer);
        }

        ///<summary>
        /// Eseguo la Init del web service
        ///</summary>
        //--------------------------------------------------------------------------------
        private void BtnInit_Click(object sender, EventArgs e)
        {
            if (dataSynch.IsAlive())
            {
                if (dataSynch.Reboot("{2E8164FA-7A8B-4352-B0DC-479984070222}"))
                    MessageBox.Show(string.Format(Strings.WSInitWithSuccess, NameSolverStrings.DataSynchronizer), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(string.Format(Strings.WSInitWithError, NameSolverStrings.DataSynchronizer), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
                MessageBox.Show(string.Format(Strings.WSInitWithError, NameSolverStrings.DataSynchronizer), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}