using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Console.Core.DBLibrary.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Interfaces;
	
namespace Microarea.Console.Core.RegressionTestLibrary.Forms
{
	/// <summary>
	/// Summary description for ProcessingForm.
	/// </summary>
	//=========================================================================
	public partial class ProcessingForm : ExecutionForm
	{
		# region Variabili private
		//private DetailsForm detailsForm = null;
		private TimeSpan singleDbDateTime;
		private TimeSpan totalDbDateTime;
		private TimeSpan singleDbEstimated;
		private TimeSpan totalDbEstimated;
		private int oldTime = Environment.TickCount;
		private bool firstDb = true;
		# endregion

		# region Events and Delegates
		//public delegate void AsyncInsertText(bool success, string operation, string detail, object obj, bool updateItem);
        public delegate void AddTextInListViewFromProcessingFormCallBack(bool success, string operation, string detail, object obj, bool updateItem);
        public delegate void PerformProgressBarStepCallBack();
		# endregion

		# region Costruttore
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------
		public ProcessingForm()
		{
			InitializeComponent();
			ExecutionProgressBar.Step = 1;	
			InitializeTimer();
		}
		# endregion

		# region Gestione Timer

		# region Initialize Timer
		//---------------------------------------------------------------------
		private void InitializeTimer()
		{
			ProcessTimer.Enabled	= true;
			ProcessTimer.AutoReset	= true;
			ProcessTimer.Interval	= 1000;
		}
		# endregion

		# region ProcessTimer_Elapsed - Evento per la gestione dei tempi
		/// <summary>
		/// ProcessTimer_Elapsed
		/// A ogni tick valorizza le label con i tempi
		/// </summary>
		//---------------------------------------------------------------------
		private void ProcessTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			int currentTime = Environment.TickCount;
			int seconds	= (currentTime - oldTime) / 1000;
			oldTime = currentTime;

			singleDbDateTime = singleDbDateTime.Subtract(TimeSpan.FromSeconds(seconds));
			totalDbDateTime = totalDbDateTime.Subtract(TimeSpan.FromSeconds(seconds));

			MissingSingleValue.Text = singleDbDateTime.ToString();
			MissingTotalValue.Text = totalDbDateTime.ToString();

			// appena mi sto avvicinando ai -5 secondi incremento di 30 (sia i tempi totali che quelli singoli
			if (singleDbDateTime.TotalSeconds <= 5)
			{
				singleDbDateTime += TimeSpan.FromSeconds(30);
				singleDbEstimated += TimeSpan.FromSeconds(30);
				EstimatedSingleValue.Text = singleDbEstimated.ToString();
				MissingSingleValue.Text = singleDbDateTime.ToString();

				totalDbDateTime	+= TimeSpan.FromSeconds(30);
				totalDbEstimated += TimeSpan.FromSeconds(30);
				EstimatedTotalValue.Text = totalDbEstimated.ToString();
				MissingTotalValue.Text = totalDbDateTime.ToString();
			}

			PastSingleValue.Text = ((TimeSpan)(singleDbEstimated - singleDbDateTime)).ToString();
			
			// appena mi sto avvicinando ai -5 secondi incremento di 30
			if (totalDbDateTime.TotalSeconds <= 5)
			{
				totalDbDateTime	+= TimeSpan.FromSeconds(30);
				totalDbEstimated += TimeSpan.FromSeconds(30);
				EstimatedTotalValue.Text = totalDbEstimated.ToString();
				MissingTotalValue.Text = totalDbDateTime.ToString();
			}

			PastTotalValue.Text = ((TimeSpan)(totalDbEstimated - totalDbDateTime)).ToString();
		}
		#endregion

		# region Inizializzazione variabili dei tempi
		// inserisco il tempo stimato per singolo database
		//---------------------------------------------------------------------
		public void SetSingleDbTimes(int singleDbMinutes)
		{
			// se non sono nell'analisi del primo database, decremento i tempi totali del tempo mancante avanzato
			if (!firstDb)
			{
				totalDbDateTime			= ((TimeSpan)(totalDbDateTime - singleDbDateTime));
				totalDbEstimated		= ((TimeSpan)(totalDbEstimated - singleDbDateTime));
				EstimatedTotalValue.Text= totalDbEstimated.ToString();
				MissingTotalValue.Text	= totalDbDateTime.ToString();
			}

			singleDbDateTime	= TimeSpan.FromMinutes(singleDbMinutes);
			singleDbEstimated	= singleDbDateTime;

			EstimatedSingleValue.Text	= singleDbDateTime.ToString();
			MissingSingleValue.Text		= singleDbDateTime.ToString();

			firstDb = false;
		}

		// inserisco il tempo stimato totale per tutti i database
		//---------------------------------------------------------------------
		public void SetTotalDbTimes(int totalDbMinutes)
		{
			oldTime = Environment.TickCount;

			totalDbDateTime		= TimeSpan.FromMinutes(totalDbMinutes);
			totalDbEstimated	= totalDbDateTime;

			EstimatedTotalValue.Text	= totalDbDateTime.ToString();
			MissingTotalValue.Text		= totalDbDateTime.ToString();

			ProcessTimer.Start();
		}
		# endregion

		#endregion

		# region Override dei metodi della form padre (DBLibrary\ExecutionForm)
		/// <summary>
		/// Inizializzazione imagelist
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void InitializeListView()
		{
			myImages = new Images();

			// inizializzo imagelist della listview
			ExecutionListView.SmallImageList = myImages.ImageList;
			ExecutionListView.LargeImageList = myImages.ImageList;

			ExecutionListView.Columns.Add(RegressionTestLibraryStrings.ColOperationTitle,	370, HorizontalAlignment.Left);
			ExecutionListView.Columns.Add(RegressionTestLibraryStrings.ColDetailsTitle,		400, HorizontalAlignment.Left);
		}

		//---------------------------------------------------------------------
		public override void PopolateTextLabel(string text)
		{
			base.PopolateTextLabel(text);
		}

		//---------------------------------------------------------------------
		public override void SetFinishInProgress(string message)
		{
			base.SetFinishInProgress(message);
			DetailsButton.Enabled = !executionIsRunning;
			
			ProcessTimer.Stop();

			// setto i tempi mancanti a zero
			MissingTotalValue.Text = TimeSpan.Zero.ToString();
			MissingSingleValue.Text = TimeSpan.Zero.ToString();
		}

		/// <summary>
		/// evento chiamato in fase di caricamento della form
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void LoadingForm()
		{
			base.LoadingForm();
			DetailsButton.Enabled = !executionIsRunning;
			
			// rendo visibili i control che mi interessano...
			DetailsButton.Visible = true;

			// faccio partire il timer
			ProcessTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProcessTimer_Elapsed);
		}

		/// <summary>
		/// sull'evento Closing della form
		/// </summary>
		//---------------------------------------------------------------------
		protected override void ClosingForm(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// se sto runnando non permetto di chiudere la form
			e.Cancel = executionIsRunning;
		}

		/// <summary>
		/// sul click del pulsante Stop
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void ClickOnStopCloseButton(object sender, System.EventArgs e)
		{
			base.ClickOnStopCloseButton(sender, e);
		}

		/// <summary>
		/// mouse down sulla list view
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void MouseDownOnListView(object sender, System.Windows.Forms.MouseEventArgs e)
		{ }

		/// <summary>
		/// selezione di uno specifico item sulla list view
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void SelectedIndexChangedOnListView(object sender, System.EventArgs e)
		{ 
			ListView list = (ListView)sender;

			if (list.SelectedItems != null		&&
				list.SelectedItems.Count == 1	&& 
				list.SelectedItems[0] != null	)
			{
				PathTextBox.Text = list.SelectedItems[0].Text;
			}
		}

		/// <summary>
		/// doppio click sulla list view
		/// </summary>
		//---------------------------------------------------------------------
		protected override void DoubleClickOnListView(object sender, System.EventArgs e)
		{ 
			if (executionIsRunning)
				return;

			ListView list = (ListView)sender;
			
			if (list.SelectedItems == null || 
				list.SelectedItems.Count == 0 
				|| list.SelectedItems.Count > 1) 
				return;

			ListViewItem item = list.SelectedItems[0];
			ShowDiagnosticOnListViewItem(item);
		}

		/// <summary>
		/// sull'evento Click del pulsante Dettagli
		/// </summary>
		//---------------------------------------------------------------------
		protected override void ClickOnDetailsButton(object sender, System.EventArgs e)
		{ 
			if (executionIsRunning)
				return;

			if (ExecutionListView.SelectedItems == null || 
				ExecutionListView.SelectedItems.Count == 0 
				|| ExecutionListView.SelectedItems.Count > 1) 
				return;

			ListViewItem item = ExecutionListView.SelectedItems[0];
			ShowDiagnosticOnListViewItem(item);
		}

		/// <summary>
		/// mostro un Diagnostico relativo all'item selezionato nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void ShowDiagnosticOnListViewItem(ListViewItem currItem)
		{
			if (currItem != null)
			{
				if (currItem.Tag != null)
					DiagnosticViewer.ShowDiagnostic((Diagnostic)(currItem.Tag));
				else
				{
					string detail = currItem.SubItems[1].Text;
					//detailsForm = new DetailsForm(currItem.Text, detail);
					//detailsForm.ShowDialog();
				}
			}
		}
		# endregion

		# region Funzioni x inserire gli item nella list view
		//---------------------------------------------------------------------------
		public void AddTextInListViewFromProcessingForm(bool success, string operation, string detail, object obj, bool updateItem)
		{
			object []args = { success, operation, detail, obj , updateItem};
    		//BeginInvoke(new AsyncInsertText(InsertText), args);
            if (this.InvokeRequired)
            {
                AddTextInListViewFromProcessingFormCallBack d =
                    new AddTextInListViewFromProcessingFormCallBack(AddTextInListViewFromProcessingForm);
                this.Invoke(d, args);
            }
            else
                InsertText(success, operation, detail, obj, updateItem);
		}

		//---------------------------------------------------------------------
		public void PerformProgressBarStep()
		{
            if (this.InvokeRequired)
            {
                PerformProgressBarStepCallBack d = new PerformProgressBarStepCallBack(PerformProgressBarStep);
                this.Invoke(d, null);
            }
            else
            {
                ExecutionProgressBar.PerformStep();
                // se la progress bar ha già raggiunto il max, la reimposto al minimo
                if (ExecutionProgressBar.Value == ExecutionProgressBar.Maximum)
                    ExecutionProgressBar.Value = ExecutionProgressBar.Minimum;
            }
		}
		
		//---------------------------------------------------------------------
		private void InsertText(bool success, string operation, string detail, object obj, bool updateCurrentItem)
		{
			ListViewItem item;
			if (updateCurrentItem)
				item = ExecutionListView.Items[count];
			else
			{
				item = new ListViewItem();
				count++;
			}

			Diagnostic diagnostic = null;
			if (obj != null)
				diagnostic = (Diagnostic)obj;
			else
			{
				diagnostic = new Diagnostic("RegressionTest");
				diagnostic.Set((success) ? DiagnosticType.Information : DiagnosticType.Error, operation, detail);
			}
			
			// al primo item associo una bitmap
			item.ImageIndex = (success) ? Images.GetCheckedBitmapIndex() : Images.GetUncheckedBitmapIndex();

			if (!success)
				item.ForeColor = Color.Red;

			item.Text = operation;

			if (updateCurrentItem)
				item.SubItems[1].Text = detail;
			else
				item.SubItems.Add(detail);

			// non spostare sopra xchè si tromba! (ho già provato e fallito io!)
			item.Tag = diagnostic;

			if (updateCurrentItem)
				ExecutionListView.Items[count] = item;
			else
				ExecutionListView.Items.Add(item);

			ExecutionListView.Items[count].Selected = true;
			ExecutionListView.EnsureVisible(count);

			Cursor.Current = Cursors.WaitCursor;
			Application.DoEvents();
			Cursor.Current = Cursors.WaitCursor;

			PerformProgressBarStep();
		}
		# endregion
	}
}
