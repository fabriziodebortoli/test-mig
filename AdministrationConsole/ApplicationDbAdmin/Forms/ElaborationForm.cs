using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	/// <summary>
	/// Summary description for ElaborationForm.
	/// </summary>
	//=========================================================================
	public partial class ElaborationForm : PlugInsForm
	{
		# region Variabili
		private DatabaseManager dbMng = null;
		private	ListViewItem	selItem	= null;

		private int sqlcount = -1;
		private int xmlcount = -1;
		#endregion

		# region Costruttore
		//---------------------------------------------------------------------------
		public ElaborationForm(DatabaseManager databaseManager)
		{
			dbMng = databaseManager;

			InitializeComponent();
			InitializeImageList();
		}
		# endregion
		
		# region Inizializzazione ImageList
		//---------------------------------------------------------------------------
		public void InitializeImageList()
		{
			myImages = new ImagesListManager().ImageList;

			// inizializzo imagelist della listview che visualizza gli script sql
			SqlProgressListView.LargeImageList = myImages;
			SqlProgressListView.SmallImageList = myImages;

			SqlProgressListView.Columns.Add(string.Empty,					20,  HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(Strings.ElabFormColScript,		250, HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(Strings.ElabFormColStep,		50,  HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(Strings.ElabFormColApplication,	100, HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(Strings.ElabFormColModule,		180, HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(Strings.ElabFormColDetails,		300, HorizontalAlignment.Left);
			SqlProgressListView.Columns.Add(string.Empty,					0,	 HorizontalAlignment.Left);

			XmlProgressListView.Columns.Clear();

			// inizializzo imagelist della listview che visualizza i file xml
			XmlProgressListView.LargeImageList = myImages;
			XmlProgressListView.SmallImageList = myImages;

			XmlProgressListView.Columns.Add(string.Empty,					20,  HorizontalAlignment.Left);
			XmlProgressListView.Columns.Add(Strings.ElabFormColFile,		250, HorizontalAlignment.Left);
			XmlProgressListView.Columns.Add(Strings.ElabFormColApplication,	100, HorizontalAlignment.Left);
			XmlProgressListView.Columns.Add(Strings.ElabFormColModule,		180, HorizontalAlignment.Left);
			XmlProgressListView.Columns.Add(Strings.ElabFormColDetails,		300, HorizontalAlignment.Left);
			XmlProgressListView.Columns.Add(string.Empty,					0,   HorizontalAlignment.Left);

			// la seconda list view è visibile se e solo se l'utente ha scelto di importare i dati di default
			XmlProgressListView.Visible = dbMng.ImportDefaultData;
			XmlDescriptionLabel.Visible = dbMng.ImportDefaultData;

			SqlProgressListView.Height = (dbMng.ImportDefaultData) ? 320 : 540;
		}
		# endregion

		# region Valorizzazione label
		//---------------------------------------------------------------------
		public void PopolateText()
		{
			CompanyLabel.Text = string.Format(CompanyLabel.Text, dbMng.ContextInfo.CompanyName);
		}
		#endregion

		# region Funzioni legate ad eventi sparati dal PlugIn, che agiscono sui singoli controls della Form
		/// <summary>
		/// Inserisce una riga nella list view con l'elenco degli script sql elaborati
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertInSqlProgressListView
			(
			bool ok, 
			string app, 
			string mod, 
			string script, 
			string step, 
			string detail,
			string fullPath,
			ExtendedInfo ei
			)
		{
			Cursor.Current = Cursors.WaitCursor;

			ListViewItem item = new ListViewItem();
			sqlcount++;

			item.ImageIndex = (ok) ? ImagesListManager.GetCheckedBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex(); 
			
			string message = string.Empty;

			if (ok)
			{
				message = (string.Compare(detail, DatabaseLayerConsts.OK, StringComparison.InvariantCultureIgnoreCase) == 0)
							? string.Format(DatabaseLayerStrings.ScriptOK, script, app, mod, fullPath) // aggiornamento db
							: detail; // creazione colonne obbligatorie TB*
			}
			else
			{
				message = string.Format(DatabaseLayerStrings.ScriptWithError, script, app, mod, fullPath, detail);
				item.ForeColor = Color.White;
				item.BackColor = Color.Red;
			}

			// solo per test grafo
			//Debug.WriteLine(string.Format(DatabaseLayerStrings.ScriptName, script) + " - " +
			//	string.Format(DatabaseLayerStrings.DirectoryName, fullPath) + string.Format(" ({0} - {1})", app, mod));

			Diagnostic diagnostic = new Diagnostic("SqlListView");
			diagnostic.Set((ok) ? DiagnosticType.Information : DiagnosticType.Error, message, ei);
			item.SubItems.Add(script);
			item.SubItems.Add(step);
			item.SubItems.Add(app);
			item.SubItems.Add(mod);
			item.SubItems.Add(detail);
			item.SubItems.Add(fullPath);
			item.Tag = diagnostic;

			// aggiungo le informazioni nel diagnostico, così lo salvo nel file di log (escluse le colonne TBCreated e TBModified)
			if (string.Compare(detail, DatabaseLayerStrings.CreatedMandatoryColumns, StringComparison.InvariantCultureIgnoreCase) != 0)
				dbMng.DBManagerDiagnostic.Set(diagnostic);

			SqlProgressListView.Items.Add(item);
			SqlProgressListView.Items[sqlcount].Selected = true;
			SqlProgressListView.EnsureVisible(sqlcount);

			Application.DoEvents();
			Cursor.Current = Cursors.WaitCursor;
		}

		/// <summary>
		/// Inserisce una riga nella list view con l'elenco dei file xml elaborati
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertInXmlProgressListView
			(
			bool	ok, 
			string	app, 
			string	mod, 
			string	script, 
			string	detail,
			string	fullPathScript
			)
		{
			Cursor.Current = Cursors.WaitCursor;

			ListViewItem item = new ListViewItem();
			xmlcount++;

			item.ImageIndex = (ok) ? ImagesListManager.GetCheckedBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex(); 

			if (!ok)
			{
				item.ForeColor = Color.White;
				item.BackColor = Color.Red;
			}

			StringCollection myStrings = new StringCollection();
			myStrings.Add(string.Format(DatabaseLayerStrings.FileName, script));
			myStrings.Add(string.Format(DatabaseLayerStrings.DirectoryName, fullPathScript));
			myStrings.Add(string.Format(DatabaseLayerStrings.ApplicationName, app));
			myStrings.Add(string.Format(DatabaseLayerStrings.ModuleName, mod));
			myStrings.Add((string.Compare(detail, DatabaseLayerConsts.OK, StringComparison.InvariantCultureIgnoreCase) == 0)
				? DatabaseLayerStrings.SuccessOperation
				: string.Format(DatabaseLayerStrings.ErrorOperation, detail));

			Diagnostic diagnostic = new Diagnostic("XmlListView");
			diagnostic.Set((ok) ? DiagnosticType.Information : DiagnosticType.Error, myStrings);

			item.SubItems.Add(script);
			item.SubItems.Add(app);
			item.SubItems.Add(mod);
			item.SubItems.Add(detail);
			item.SubItems.Add(fullPathScript);
			item.Tag = diagnostic;

			XmlProgressListView.Items.Add(item);

			XmlProgressListView.Items[xmlcount].Selected = true;
			XmlProgressListView.EnsureVisible(xmlcount);

			Application.DoEvents();
			Cursor.Current = Cursors.WaitCursor;
		}

		/// <summary>
		/// inserisce una stringa contenente "Elaborazione del modulo {0} in corso"
		/// nella text box di elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertModuleNameInLabel(string module)
		{
			ModuleCounterLabel.Text = string.Format(DatabaseLayerStrings.FormModuleCounter, module);
			ModuleCounterLabel.Update();
		}

		/// <summary>
		/// inserisce una stringa contenente "Aggiunta colonne obbligatorie alla tabella {0} in corso..."
		/// nella text box di elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertTableNameWithMandatoryColsInLabel(string table)
		{
			ModuleCounterLabel.Text = string.Format(DatabaseLayerStrings.FormTableWithMandatoryColsCounter, table);
			ModuleCounterLabel.Update();
		}

		/// <summary>
		/// inserisce una stringa contenente "Importazione {0} del modulo {1} in corso..."
		/// nella text box di elaborazione (per l'import dei dati di default)
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertFileNameInLabelForDefault(string file, string module)
		{
			ModuleCounterLabel.Text = string.Format(DatabaseLayerStrings.FormDefaultFileCounter, file, module);
			ModuleCounterLabel.Update();
		}

		/// <summary>
		/// inserisce la stringa passata come parametro nella text box di elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertStringInLabel(string str)
		{
			ModuleCounterLabel.Text = string.Format(str);
			ModuleCounterLabel.Update();
		}

		/// <summary>
		/// inserisce una stringa contenente "Creazione vista materializzata {0} in corso"
		/// nella text box di elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		public void InsertMViewInLabel(string mView)
		{
			ModuleCounterLabel.Text = string.Format(DatabaseLayerStrings.FormMViewCounter, mView);
			ModuleCounterLabel.Update();
		}

		/// <summary>
		/// nel caso in cui non esistano file da importare per la lingua prescelta, ridefinisco la list view e
		/// propongo un messaggio opportuno.
		/// </summary>
		//---------------------------------------------------------------------
		public void RedefineXmlProgressListView()
		{
			if (XmlProgressListView.Items != null && XmlProgressListView.Items.Count < 1)
			{
				XmlProgressListView.Columns.Clear();

				XmlProgressListView.LargeImageList = myImages;
				XmlProgressListView.SmallImageList = myImages;

				XmlProgressListView.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
				XmlProgressListView.Columns.Add(Strings.ElabFormColDetails, 500, HorizontalAlignment.Left);

				ListViewItem item = new ListViewItem();
				item.ImageIndex = ImagesListManager.GetDummyStateBitmapIndex();
				item.Font = new Font(item.Font, item.Font.Style | FontStyle.Bold);
				item.SubItems.Add(Strings.MsgImportFilesNotExist);
				XmlProgressListView.Items.Add(item);
			}
		}
		# endregion

		# region Evento sul bottone Aggiorna
		//---------------------------------------------------------------------
		public bool StartElaboration()
		{
			bool success = true;

			PopolateText();

			this.State = StateEnums.Processing;
			Cursor.Current = Cursors.WaitCursor;

			InsertStringInLabel((success) ? Strings.MsgStatusBarEnd : Strings.MsgUpdateDBFailed);

			this.State = StateEnums.View;
			Cursor.Current = Cursors.Default;

			return success;
		}
		# endregion

		# region Eventi intercettati sulle list view

		# region ListView Script SQL
		/// <summary>
		/// aggancio il menu di contesto alla list view degli script di sql
		/// </summary>
		//---------------------------------------------------------------------
		private void SqlProgressListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewContextMenu.MenuItems.Clear();
			bool isMandatoryColItem = false;

			if (SqlProgressListView.SelectedItems.Count == 1)
			{
				selItem = SqlProgressListView.GetItemAt(e.X, e.Y);

				if (selItem != null)
				{
					isMandatoryColItem = (selItem.SubItems[5].Text.IndexOf
						(DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) > 0);

					switch (e.Button)
					{
						case MouseButtons.Right:// col tasto dx faccio vedere il context menu
						{	
							if (!isMandatoryColItem)
							{
								ListViewContextMenu.MenuItems.Add
									(
									new MenuItem(Strings.PathCopyContextMenu,  
									new System.EventHandler(OnCopyPath))
									);
							}

							ListViewContextMenu.MenuItems.Add
								(
									new MenuItem(Strings.DetailsContextMenu,  
									new System.EventHandler(OnShowDiagnostic))
								);

							if (!isMandatoryColItem)
							{
								if (selItem.SubItems[2].Text.IndexOf(NameSolverStrings.Log, StringComparison.InvariantCultureIgnoreCase) >= 0)
									ListViewContextMenu.MenuItems.Add
										(
										new MenuItem(Strings.OpensWithExplorerContextMenu,  
										new System.EventHandler(OpenWithIExplore))
										);
								else
									ListViewContextMenu.MenuItems.Add
										(
										new MenuItem(Strings.OpensWithNotePadContextMenu,  
										new System.EventHandler(OpenWithNotePad))
										);
							}
							break;
						}

						case MouseButtons.Left:
						case MouseButtons.Middle:
						case MouseButtons.None:
							break;
						default: 
							break;
					}
				}
			}
		}

		/// <summary>
		/// intercetto il doubleclick sulla list view degli script e visualizzo un diagnostico di dettaglio
		/// </summary>
		//---------------------------------------------------------------------------
		private void SqlProgressListView_DoubleClick(object sender, System.EventArgs e)
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

		# region ListView file XML
		/// <summary>
		/// aggancio il menu di contesto alla list view dei file xml
		/// </summary>
		//---------------------------------------------------------------------------
		private void XmlProgressListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewContextMenu.MenuItems.Clear();

			if (XmlProgressListView.SelectedItems.Count == 1)
			{
				selItem = XmlProgressListView.GetItemAt(e.X, e.Y);
				
				if (selItem != null)
				{
					switch (e.Button)
					{
						case MouseButtons.Right:// col tasto dx faccio vedere il context menu
						{	
							ListViewContextMenu.MenuItems.Add
								(
									new MenuItem(Strings.PathCopyContextMenu,  
									new System.EventHandler(OnCopyPath))
								);
							
							ListViewContextMenu.MenuItems.Add
								(
									new MenuItem(Strings.DetailsContextMenu,  
									new System.EventHandler(OnShowDiagnostic))
								);

							ListViewContextMenu.MenuItems.Add
								(
									new MenuItem(Strings.OpensWithExplorerContextMenu,  
									new System.EventHandler(OpenWithIExplore))
								);
							
							break;
						}

						case MouseButtons.Left:
						case MouseButtons.Middle:
						case MouseButtons.None:
							break;
						default: 
							break;
					}
				}
			}
		}

		/// <summary>
		/// intercetto il doubleclick sulla list view degli xml e visualizzo un diagnostico di dettaglio
		/// </summary>
		//---------------------------------------------------------------------------
		private void XmlProgressListView_DoubleClick(object sender, System.EventArgs e)
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

		/// <summary>
		/// copio nella clipboard il contenuto dell'ultimo subItem della listview (che è nascosto)
		/// </summary>
		//---------------------------------------------------------------------------
		private void OnCopyPath(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(selItem.SubItems[selItem.SubItems.Count - 1].Text);
		}

		//---------------------------------------------------------------------------
		private void OnShowDiagnostic(object sender, System.EventArgs e)
		{
			ShowDiagnosticOnListViewItem(selItem);
		}

		/// <summary>
		/// apre con lo script sql selezionato con notepad.exe, se il file esiste
		/// </summary>
		//---------------------------------------------------------------------------
		private void OpenWithNotePad(object sender, System.EventArgs e)
		{
			if (
				selItem != null && 
				File.Exists(selItem.SubItems[selItem.SubItems.Count - 1].Text)
				)
				Process.Start(selItem.SubItems[selItem.SubItems.Count - 1].Text);
		}

		/// <summary>
		/// apre con il file xml selezionato con internet explorer, se il file esiste
		/// </summary>
		//---------------------------------------------------------------------------
		private void OpenWithIExplore(object sender, System.EventArgs e)
		{
			if (
				selItem != null &&
				File.Exists(selItem.SubItems[selItem.SubItems.Count - 1].Text)
				)
				Process.Start(selItem.SubItems[selItem.SubItems.Count - 1].Text);
		}

		/// <summary>
		/// mostro un Diagnostico relativo all'item selezionato nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void ShowDiagnosticOnListViewItem(ListViewItem currItem)
		{
			if (currItem != null && currItem.Tag != null)
			{
				if (selItem.SubItems[2].Text.IndexOf(NameSolverStrings.Log, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					// se si tratta della riga del file di log visualizzo il file xml dinamicamente dentro
					// il DiagnosticView, consentendo anche di effettuare il sort per tipo di messaggio (info-error) vedi ultimo param.
					DiagnosticView view = new DiagnosticView((Diagnostic)(currItem.Tag), OrderType.None, true);
					view.LoadXmlFile(currItem.SubItems[6].Text);
				}
				else
					DiagnosticViewer.ShowDiagnostic((Diagnostic)(currItem.Tag));
			}
		}
		#endregion

	}
}
