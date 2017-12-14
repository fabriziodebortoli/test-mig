using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;

namespace Microarea.Console.Core.DBLibrary.Forms
{
	/// <summary>
	/// Summary description for ExecutionForm.
	/// </summary>
	//=========================================================================
	public partial class ExecutionForm : System.Windows.Forms.Form
	{
		# region Variabili protette per le form derivate....
        protected Images myImages = null;
		protected bool executionIsRunning	= false;
		protected bool isAborted			= false;
		protected int  count = -1;
        # endregion

		# region Variabili private
        private Thread			myThread = null;
		private ListViewItem	selItem = null;

		private bool errorOccurred = false;
        # endregion

		# region Events e Delegates
		public delegate void IsAborted(bool abort);
		public event IsAborted OnIsAborted;
		# endregion

		# region Costruttore + OnLoad
		//---------------------------------------------------------------------------
		public ExecutionForm()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// spostata dal costruttore per problemi di visualizzazione in DesignMode.
			InitializeListView();
		}
		# endregion

		# region Lancio thread separato
		//---------------------------------------------------------------------------
		public void InitializeSeparateThread(Thread t)
		{
			myThread = t;
		}
		# endregion

		# region Inizializzazione ImageList e Label varie
		//---------------------------------------------------------------------------
		private void InitializeExecutionListView()
		{
			// serve per caricare il DesignMode delle finestre derivate senza errori!
			if (this.DesignMode)
				return;

			myImages = new Images();

			// inizializzo imagelist della listview
			ExecutionListView.SmallImageList = myImages.ImageList;
			ExecutionListView.LargeImageList = myImages.ImageList;

			ExecutionListView.Columns.Add(DBLibraryStrings.ExecFormColTable, 250, HorizontalAlignment.Left);
			ExecutionListView.Columns.Add(DBLibraryStrings.ExecFormColDetails, 300, HorizontalAlignment.Left);
			ExecutionListView.Columns.Add(DBLibraryStrings.ExecFormColFile, 250, HorizontalAlignment.Left);
		}
	
		//---------------------------------------------------------------------
		public virtual void PopolateTextLabel(string text)
		{
			TypeOperationLabel.Text = text;
		}
		# endregion

		# region Funzioni per visualizzare i messaggi nella form "sparati" da fuori
        /// <summary>
		/// Passa alla Form gli argomenti da visualizzare nell'apposita list-view
		/// </summary>
		//---------------------------------------------------------------------
		public void AddTextInListView(bool success, string tableName, string fileName, string detail, string fullPath)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { AddTextInListView(success, tableName, fileName, detail, fullPath); });
				return;
			}

			Insert(success, tableName, fileName, detail, fullPath);
		}

		/// <summary>
		/// Inserisce una riga di testo nella list view, valorizzando le colonne
		/// </summary>
		//---------------------------------------------------------------------
		public void Insert(bool success, string tableName, string fileName, string detail, string fullPath)
		{
			ListViewItem item = new ListViewItem();
			count++;

			// al primo item associo una bitmap
			item.ImageIndex = (success) ? Images.GetCheckedBitmapIndex() : Images.GetUncheckedBitmapIndex();

			if (!success)
			{
				item.ForeColor = Color.Red;
				errorOccurred = true;
			}
			
			item.Text = tableName;
			item.SubItems.Add(detail);
			item.SubItems.Add(fileName);
			item.Tag = fullPath;
			ExecutionListView.Items.Add(item);
			ExecutionListView.Items[count].Selected = true;
			ExecutionListView.EnsureVisible(count);

			Cursor.Current = Cursors.WaitCursor;

            PerformProgressBarStepParent();
		}

        /// <summary>
		/// A fine elaborazione setta la progress bar al massimo e reimposta il cursore con il default
		/// </summary>
		//---------------------------------------------------------------------
		public virtual void SetFinishInProgress(string message)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetFinishInProgress(message); });
				return;
			}

            ExecutionProgressBar.Value = ExecutionProgressBar.Maximum;
            ExecutionProgressBar.Invalidate();
                
            Cursor.Current = Cursors.Default;

			StopCloseFormButton.Text = DBLibraryStrings.CloseButton;
            StopCloseFormButton.Enabled = true;

            if (message.Length > 0)
                PopolateTextLabel(message);

			// visualizzo la label con un avvertimento se è stato segnalato un errore nell'elaborazione
			ErrorLabel.Visible = errorOccurred;
			// altrimenti visualizzo una label con l'indicazione che l'operazione si e' conclusa con successo
			ElabCompleteLabel.Visible = !errorOccurred;

            // terminata l'elaborazione tolgo lo stato di running
            executionIsRunning = false;

            if (OnIsAborted != null)
                OnIsAborted(false);
		}

        /// <summary>
        /// Gestisce l'incremento di step della ExecutionProgressBar 
        /// </summary>
		//---------------------------------------------------------------------
		public void PerformProgressBarStepParent()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { PerformProgressBarStepParent(); });
				return;
			}

            // se la progress bar ha già raggiunto il max, la reimposto al minimo
            ExecutionProgressBar.PerformStep();
            if (ExecutionProgressBar.Value == ExecutionProgressBar.Maximum)
                ExecutionProgressBar.Value = ExecutionProgressBar.Minimum;
		}
		# endregion

		# region Eventi sulla form
		/// <summary>
		/// appena carico la form imposto a true lo stato di running
		/// </summary>
		//---------------------------------------------------------------------------
		private void ExecutionForm_Load(object sender, System.EventArgs e)
		{
			LoadingForm();
		}

		/// <summary>
		/// prima di chiudere la form controllo che sia conclusa l'elaborazione, altrimenti
		/// non lo permetto
		/// </summary>
		//---------------------------------------------------------------------------
		private void ExecutionForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ClosingForm(sender, e);
		}

		/// <summary>
		/// sul click del mouse sulla list-view consento due comportamenti distinti:
		/// 1. tasto sx: nella label superiore visualizzo il path del file dell'item selezionato
		/// 2. tasto dx: si apre un context menu con la Copia percorso
		/// </summary>
		//---------------------------------------------------------------------------
		private void ExecutionListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			MouseDownOnListView(sender, e);
		}

		/// <summary>
		/// copio nella clipboard il contenuto del tag dell'item della list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void OnCopyPath(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(selItem.Tag.ToString());
		}

		/// <summary>
		/// per fare in modo che, in fase di elaborazione, la label con il path del file che stiamo
		/// analizzando venga aggiornata.
		/// </summary>
		//---------------------------------------------------------------------------
		private void ExecutionListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SelectedIndexChangedOnListView(sender, e);
		}

		/// <summary>
		/// doppio click sulla list view
		/// </summary>
		//---------------------------------------------------------------------
		private void ExecutionListView_DoubleClick(object sender, System.EventArgs e)
		{
			DoubleClickOnListView(sender, e);
		}
		# endregion

		# region Evento sul pulsante Interrompi/Chiudi
		/// <summary>
		/// quando clicco sul pulsante posso sospendere l'elaborazione
		/// </summary>
		//---------------------------------------------------------------------------
		private void StopCloseFormButton_Click(object sender, System.EventArgs e)
		{
			ClickOnStopCloseButton(sender, e);
		}

		/// <summary>
		/// evento sul click del pulsante Dettagli
		/// </summary>
		//---------------------------------------------------------------------
		private void DetailsButton_Click(object sender, System.EventArgs e)
		{
			ClickOnDetailsButton(sender, e);
		}
		# endregion

		# region Metodi virtual da utilizzare nelle form inherited
		/// <summary>
		/// per personalizzare le colonne e l'image list da utilizzare
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void InitializeListView()
		{
			InitializeExecutionListView();
		}

		/// <summary>
		/// sull'evento Load della form
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void LoadingForm()
		{
			// quando faccio la load della form significa che l'elaborazione è iniziata 
			executionIsRunning = true;
			StopCloseFormButton.Text = DBLibraryStrings.CancelButton;
		}

		/// <summary>
		/// sull'evento Closing della form
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void ClosingForm(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// se sto runnando non permetto di chiudere la form
			e.Cancel = executionIsRunning;
		}

		/// <summary>
		/// sull'evento Click del pulsante Dettagli
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void ClickOnDetailsButton(object sender, System.EventArgs e)
		{ }

		/// <summary>
		/// doppio click sulla list view
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void DoubleClickOnListView(object sender, System.EventArgs e)
		{ 
			ListView list = (ListView)sender;
			
			if (list.SelectedItems == null || list.SelectedItems.Count == 0) 
				return;

			ListViewItem item = list.SelectedItems[0];
			if (item != null && item.ImageIndex == 0) // è un errore
				DiagnosticViewer.ShowErrorTrace(item.SubItems[1].Text, string.Empty, item.SubItems[2].Text);
		}

		/// <summary>
		/// mouse down sulla list view
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void MouseDownOnListView(object sender, System.Windows.Forms.MouseEventArgs e)
		{ 
			ListViewContextMenu.MenuItems.Clear();

			if (ExecutionListView.SelectedItems.Count == 1)
			{
				selItem = ExecutionListView.GetItemAt(e.X, e.Y);
				
				if (selItem != null)
				{
					PathTextBox.Text = string.Empty;
					switch (e.Button)
					{
						// cliccando col tasto sx faccio vedere il relativo path del file nella label
						case MouseButtons.Left:
						{
							PathTextBox.Text = selItem.Tag.ToString();
							break;
						}

							// cliccando col tasto dx permetto di copiare il path 
						case MouseButtons.Right:
						{	
							PathTextBox.Text = selItem.Tag.ToString();
							MenuItem menuItem = new MenuItem(DBLibraryStrings.PathCopyContextMenu, new System.EventHandler(OnCopyPath));
							ListViewContextMenu.MenuItems.Add(menuItem);
							break;
						}

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
		/// evento sul Selected Index dell'item della list view
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void SelectedIndexChangedOnListView(object sender, System.EventArgs e)
		{
			if (ExecutionListView.SelectedItems != null &&
				ExecutionListView.SelectedItems.Count == 1 && 
				ExecutionListView.SelectedItems[0] != null)
				PathTextBox.Text = ExecutionListView.SelectedItems[0].Tag.ToString();
		}

		/// <summary>
		/// evento sul click del bottone di Stop/Close
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void ClickOnStopCloseButton(object sender, System.EventArgs e)
		{
			// se sto runnando sospendo il thread e chiedo all'utente se vuole continuare
            if (executionIsRunning)
            {
                ThreadPriority currentThreadPriority = myThread.Priority;
                if (myThread.ThreadState == ThreadState.Running)
                {
                    //myThread.Suspend();

                    try
                    {
                        myThread.Priority = ThreadPriority.Lowest;
                    }
                    catch (Exception)
                    {
                    }

                }

                DialogResult res =
					DiagnosticViewer.ShowQuestion(DBLibraryStrings.MsgStopElaboration, DBLibraryStrings.LblAttention);

                isAborted = (res == DialogResult.Yes) ? true : false;

                try
                {
                    myThread.Priority = currentThreadPriority;
                }
                catch (Exception)
                {
                }

                if (isAborted)
                {
                    // disabilito il pulsante
					StopCloseFormButton.Text = DBLibraryStrings.WaitButton;
                    StopCloseFormButton.Enabled = false;

                    // segnalo al DataManager che l'utente vuole interrompere l'elaborazione
                    if (OnIsAborted != null)
                        OnIsAborted(isAborted);
                }
            }
            else
                Close(); // chiudo la finestra
		}
		# endregion
	}

	# region STRUTTURA IN MEMORIA SERIALIZZARE LE INFO IN UN FILE DI LOG
	//=========================================================================
	public class LogTableInfo
	{
		private string		tableName		= string.Empty;		// contenuto dell'attributo name del tag <Table>
		private string		tableDescription= string.Empty;		// contenuto dell'attributo description del tag <Table>
		private ArrayList 	messagesList	= new ArrayList();	// contenuto dell'attributo dependencyname del tag <Library>

		//---------------------------------------------------------------------
		public LogTableInfo() {	}

		//---------------------------------------------------------------------
		public string Name			{ get { return tableName; } set { tableName = value; } }
		//---------------------------------------------------------------------
		public string Description	{ get { return tableDescription; } set { tableDescription = value; } }
	}
	
	//=========================================================================
	public class LogMessageInfo
	{
		private string	messageType	= string.Empty;	// contenuto dell'attributo type del tag <Message>
		private string	messageText	= string.Empty;	// contenuto dell'attributo text del tag <Message>

		//---------------------------------------------------------------------
		public LogMessageInfo() {	}

		//---------------------------------------------------------------------
		public string Type	{ get { return messageType; } set { messageType = value; } }
		//---------------------------------------------------------------------
		public string Text	{ get { return messageText; } set { messageText = value; } }
	}
	# endregion
}

# region Per gestire un eventuale file di log
			// per il file di log (non utilizzato)
/*			LogTableInfo logTableInfo = new LogTableInfo();
			logTableInfo.Name = tableName;
			logTableInfo.Description = fileName;
			logInfoList.Add(logTableInfo); */

		/// <summary>
		/// funzione che scrive	il file di log, contenente le operazioni effettuate e gli
		/// eventuali errori verificatisi durante l'elaborazione
		/// </summary>
		/// <param name="nameFile">nome del file (varia a seconda della procedura)</param>
		//---------------------------------------------------------------------------
/*		public string WriteLogFile(string nameFile)
		{
			xDoc = new XmlDocument();
			
			XmlDeclaration xDec = xDoc.CreateXmlDeclaration("1.0", "utf-8", null);
			xDoc.AppendChild(xDec);

			XmlElement xLog = xDoc.CreateElement(nameFile + "Log");
			xLog.SetAttribute("creationdate", DateTime.UtcNow.ToString("s"));
			xDoc.AppendChild(xLog);

			XmlElement xDir = xDoc.CreateElement("DirectoryPath");
			xDir.SetAttribute("name", pathOutput);
			xDoc.DocumentElement.AppendChild(xDir);

			XmlElement tableElement = null;

			foreach (LogTableInfo logTable in logInfoList)
			{
				tableElement = xDoc.CreateElement("Table");
				tableElement.SetAttribute("name", logTable.Name); 
				tableElement.SetAttribute("description", string.Concat("Nome file ", logTable.Description)); 
				xDoc.DocumentElement.AppendChild(tableElement);
			}

			// copio il file passato nella cartella dove sono stati generati anche gli xml
			string path = pathOutput + Path.DirectorySeparatorChar + nameFile + ".log";

			// se il file è readonly lo rendo scrivibile, prima di trasferire i dati.
			FileInfo fileInfo = new FileInfo(path);
			if (
				fileInfo.Exists && 
				((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				)
				fileInfo.Attributes -= FileAttributes.ReadOnly;

			// scrivo il file xml, in formato indentato.
			XmlTextWriter tr = new XmlTextWriter(path, null);
			tr.Formatting = Formatting.Indented;
			xDoc.WriteContentTo(tr);
			tr.Close();

			return nameFile + ".log";
		
		}*/		
# endregion
