using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	///<summary>
	/// Form per gestire le ottimizzazioni di uno schema Oracle
	/// L'utente puo' controllare se l'owner ha i privilegi necessari e se lo schema
	/// contiene le viste materializzate ottimizzate.
	/// E' possibile eliminarle oppure ri-crearle on demand
	///</summary>
	//================================================================================
	public partial class MatViews : Form
	{
		private ImageList myImages;
		private DatabaseManager dbManager;

		private bool userHasPrivileges = false;
		private List<string> mViewsList = new List<string>();
		private int count = -1;

		private bool processIsRunning = false;
		private bool disableClosingForm = false;

		//--------------------------------------------------------------------------------
		public MatViews(DatabaseManager dbManager)
		{
			InitializeComponent(); 
			InitializeControls();

			this.dbManager = dbManager;
			this.dbManager.OnAddMessageForMView += new DatabaseManager.AddMessageForMView(AddMessageForMView);
			this.dbManager.OnMViewsProcessElaborationCompleted += new DatabaseManager.MViewsProcessElaborationCompleted(MViewsProcessElaborationCompleted);
		}

		//---------------------------------------------------------------------------
		public void InitializeControls()
		{
			GBoxActions.Enabled = RBtnCreateViews.Enabled = RBtnDropViews.Enabled = false;
			BtnOk.Enabled = false;

			ListMsg.Items.Clear();
			ListMsg.Columns.Clear();

			ListMsg.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
			ListMsg.Columns.Add(string.Empty, 350, HorizontalAlignment.Left);

			myImages = new ImagesListManager().ImageList;

			// inizializzo imagelist della listview
			ListMsg.LargeImageList = myImages;
			ListMsg.SmallImageList = myImages;
		}

		
		/// <summary>
		/// Metodo richiamato esternamente per disabilitare i controlli di uscita della form
		/// per forzare l'elaborazione
		/// Utilizzato quando la form viene richiamata dal plugin dell'Auditing in caso di 
		/// modifiche alla struttura
		/// </summary>
		//---------------------------------------------------------------------------
		public void DisableControls()
		{
			BtnCancel.Enabled = false;
			disableClosingForm = true;
		}

		///<summary>
		/// Durante l'elaborazione arrivano degli eventi per inserire la messaggistica nella listview
		///</summary>
		//---------------------------------------------------------------------------
		private void AddMessageForMView(DiagnosticType type, string message)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { AddMessageForMView(type, message); });
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			ListMsg.BeginUpdate();

			ListViewItem lvi = new ListViewItem();
			count++;

			switch (type)
			{
				case DiagnosticType.Warning:
					lvi.ImageIndex = ImagesListManager.GetWarningBitmapIndex();
					break;
				case DiagnosticType.Information:
					lvi.ImageIndex = ImagesListManager.GetInformationBitmapIndex();
					break;

				case DiagnosticType.Error:
					lvi.ImageIndex = ImagesListManager.GetRedFlagBitmapIndex();
					break;

				case DiagnosticType.LogInfo:
					lvi.ImageIndex = ImagesListManager.GetGreenFlagBitmapIndex();
					break;

				case DiagnosticType.Banner:
				case DiagnosticType.FatalError:
				case DiagnosticType.None:
				case DiagnosticType.All:
				default:
					break;
			}

			lvi.SubItems.Add(message);
			ListMsg.Items.Add(lvi);
			ListMsg.EnsureVisible(count);
			
			ListMsg.EndUpdate();

			Application.DoEvents();
		}

		///<summary>
		/// Esegue il controllo della situazione sul db (se l'owner ha i privilegi richiesti e se 
		/// esistono le view materializzate)
		///</summary>
		//---------------------------------------------------------------------------
		private void BtnCheck_Click(object sender, EventArgs e)
		{
			ListMsg.Items.Clear();
			count = -1;

			// richiamo le funzioni del DatabaseManager per eseguire il check preventivo della situazione nello schema Oracle
			dbManager.CheckOracleMatViewsStatus(out userHasPrivileges, out mViewsList);

			// abilito i controls della form
			GBoxActions.Enabled = RBtnCreateViews.Enabled = true;
			RBtnDropViews.Enabled = (mViewsList.Count > 0);
			if (!RBtnDropViews.Enabled)
				RBtnCreateViews.Checked = true;
			BtnOk.Enabled = true;
		}

		///<summary>
		/// Esegue la procedura di DROP e CREATE delle viste materializzate (a seconda delle opzioni scelte dall'utente)
		/// Al termine ri-esegue il check per aggiornare le strutture in caso di una successiva 
		///</summary>
		//---------------------------------------------------------------------------
		private void BtnOk_Click(object sender, EventArgs e)
		{
			// se ho deciso di cancellare solamente e le view non esistono non procedo
			if (RBtnDropViews.Checked && mViewsList.Count == 0)
			{
				MessageBox.Show(this, Strings.NoAvailableMViews, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			if (MessageBox.Show(this, Strings.StartMViewCreationProcess, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			processIsRunning = true;

			PictBoxProgress.Visible = true;
			Cursor.Current = Cursors.WaitCursor;
			Application.DoEvents();

			ListMsg.Items.Clear();
			count = -1;
			SetButtons(false);

			dbManager.ManageOracleMatViews(RBtnDropViews.Checked, userHasPrivileges, mViewsList);
		}

		//---------------------------------------------------------------------------
		private void MViewsProcessElaborationCompleted()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { MViewsProcessElaborationCompleted(); });
				return;
			}

			// alla fine cmq ri-eseguo il check dello status e ri-carico le info
			// (per pararmi nel caso in cui uno clicchi di nuovo su OK)
			dbManager.CheckOracleMatViewsStatus(out userHasPrivileges, out mViewsList);

			SetButtons(true);
			PictBoxProgress.Visible = false;

			Cursor.Current = Cursors.Default;
			Application.DoEvents();

			processIsRunning = false;
			disableClosingForm = false;
		}

		//---------------------------------------------------------------------------
		private void SetButtons(bool enable)
		{
			BtnCancel.Enabled = enable;
			BtnOk.Enabled = enable;
			BtnCheck.Enabled = enable;
			GBoxActions.Enabled = enable;
		}

		//---------------------------------------------------------------------------
		private void MatViews_FormClosing(object sender, FormClosingEventArgs e)
		{
			// se dall'esterno mi hanno disabilitato la chiusura devo dare un msg e non procedere
			if (disableClosingForm)
			{
				MessageBox.Show("Before close form you have to run this procedure!");
				e.Cancel = disableClosingForm;
				return;
			}

			// se sto runnando non permetto di chiudere la form
			e.Cancel = processIsRunning;
		}
	}
}
