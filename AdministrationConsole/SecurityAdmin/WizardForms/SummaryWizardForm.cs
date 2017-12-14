using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public partial class SummaryWizardForm : ExteriorWizardPage
	{

		private WizardParameters wizardParameters = null;

		public delegate void RefreshWorkingAreaEventHandler(object sender, EventArgs e);
		public event RefreshWorkingAreaEventHandler OnRefreshWorkingAreaEventHandler = null;

		public SummaryWizardForm()
		{
			InitializeComponent();
			
		}
		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			wizardParameters = ((SecurityWizardManager)this.WizardManager).GetImportSelections();
			if (wizardParameters == null)
				return false;

            m_titleLabel.Text = WizardStringMaker.GetSummaryOperationTitle(wizardParameters.OperationType);
			SetDescriptionLabel();
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);

            Stream myStream = WizardStringMaker.GetImageByOperationType(wizardParameters.OperationType);
			if (myStream == null) return false ;
			m_watermarkPicture.Image = Image.FromStream (myStream, true);

			return true;
		}
		//---------------------------------------------------------------------
		private void SetDescriptionLabel()
		{
			this.SummaryText.Clear();
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);

			//Descrizione operazione
            SummaryText.AppendText(WizardStringMaker.GetSummaryTitleDescription(wizardParameters.OperationType));
			SummaryText.AppendText("\r\n");
			SummaryText.AppendText("\r\n");
			
			//Metto la parte degli oggetti
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);
			SummaryText.AppendText(Strings.SelectedObjectsType + "\r\n");

			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Regular);

			SummaryText.SelectionBullet = true;

			if (wizardParameters.ObjectSelectionType == WizardParametersType.AllObjects)
				SummaryText.AppendText(Strings.AllObjects + "\r\n");
			else
			{
				foreach(ListViewItem item in wizardParameters.ObejctTypeArrayList)
					SummaryText.AppendText(item.Text  + "\r\n");
			}
			SummaryText.SelectionBullet = false;
			
			if (wizardParameters.OperationType == WizardOperationType.DeleteGrants ||
				wizardParameters.OperationType == WizardOperationType.SetGrants)
			{
				SummaryText.AppendText("\r\n");
				AddRolesAndSelections();
			}
			if (wizardParameters.OperationType == WizardOperationType.SetGrants)
			{
				SummaryText.AppendText("\r\n");
				AddGrantsSelections();

			}
			
		}
		//---------------------------------------------------------------------
		private void AddRolesAndSelections()
		{
			//Ruoli o Utenti 
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);
			SummaryText.AppendText(Strings.RolesOrUsersSelectTitle+ "\r\n");

			
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Regular);

			string operationType = "";
			SummaryText.SelectionBullet = true;

			//Tutti gli Utenti e tutti i Ruoli
			if (wizardParameters.UsersOrRolesSelectionType == WizardParametersType.AllUsersAndRoles)
			{
				operationType = Strings.AllUsersAndRoles;
				SummaryText.AppendText(operationType);
				SummaryText.AppendText("\r\n");
				SummaryText.SelectionBullet = false;
				return;
			}
		
			if(wizardParameters.UsersOrRolesSelectionType == WizardParametersType.SelectedUserOrRoleByTree)
			{
				operationType = Strings.RoleOuUserSelecteByTree;
				SummaryText.AppendText(operationType);
				SummaryText.AppendText("\r\n");
				SummaryText.SelectionBullet = false;
				return;
			}
			else
				operationType = Strings.RolesOrUsersSelected;

			SummaryText.AppendText(operationType + "\r\n");
			SummaryText.SelectionBullet = false;

			if (wizardParameters.UsersArrayList != null && wizardParameters.UsersArrayList.Count >0)
			{
				SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);
				SummaryText.AppendText(Strings.UsersList + "\r\n");
				SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Regular);
				foreach (ListViewItem item in wizardParameters.UsersArrayList)
				{
					SummaryText.AppendText("\t");
					SummaryText.AppendText(item.Text  + "\r\n");
				}
				SummaryText.SelectionBullet = false;
			}

			if (wizardParameters.RolesArrayList != null && wizardParameters.RolesArrayList.Count >0)
			{
				SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);
				SummaryText.AppendText(Strings.RolesList+ "\r\n");
				SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Regular);

				foreach (ListViewItem item in wizardParameters.RolesArrayList)
				{
					SummaryText.AppendText("\t");
					SummaryText.AppendText(item.Text  + "\r\n");
				}
			}
			SummaryText.SelectionBullet = false;

		}
		//---------------------------------------------------------------------
		private void AddGrantsSelections()
		{
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Bold);
			SummaryText.AppendText(Strings.SelectedGrants + "\r\n");

		//	SummaryText.SelectionBullet = true;
			SummaryText.SelectionFont = new Font(SummaryText.Font, SummaryText.Font.Style | FontStyle.Regular);
			foreach (DataRow dr in wizardParameters.GrantsDataTable.Rows)
		//		SummaryText.AppendText(dr[securityGrants.Grant] + "\t\t" + StringsMaker.GetGrantsValueDescription(Convert.ToInt32(dr[securityGrants.Assign])) + "\r\n");
                SummaryText.AppendText(dr[securityGrants.Grant] + "  --> " + WizardStringMaker.GetGrantsValueDescription(Convert.ToInt32(dr[securityGrants.Assign])) + "\r\n");

		}
		//---------------------------------------------------------------------
		#endregion 

		# region OnWizardFinish
        public override bool OnWizardFinish()
		{
			DialogResult result = DiagnosticViewer.ShowQuestion(Strings.ConfirmMessage, Strings.Warning);
			
			if (result != DialogResult.Yes)
				return false; 
			
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			this.SummaryText.Cursor = Cursors.WaitCursor;
//			Thread workerThread = new Thread(new ThreadStart(this.ThreadProc));
//			workerThread.CurrentCulture		= System.Threading.Thread.CurrentThread.CurrentCulture;
//			workerThread.CurrentUICulture	= System.Threading.Thread.CurrentThread.CurrentUICulture;
//			workerThread.Start();

//			do
//			{
//				Application.DoEvents();
//			}while(workerThread.IsAlive && !workerThread.Join(10));
//
			if (wizardParameters != null && wizardParameters.ShowObjectsTreeForm != null)
				wizardParameters.ShowObjectsTreeForm.ApplyGrantsToMenuXmlNode(wizardParameters.ShowObjectsTreeForm.CurrentMenuXmlNode, wizardParameters);

			if (wizardParameters != null && wizardParameters.ShowObjectsTreeForm != null)
			{
				if (wizardParameters.ShowObjectsTreeForm.CurrentMenuXmlNode.IsMenu)
					wizardParameters.ShowObjectsTreeForm.MenuManagerWinControl.AdjustSubTreeNodeStateImageIndex(wizardParameters.ShowObjectsTreeForm.MenuManagerWinControl.CurrentMenuTreeNode);
				else
					wizardParameters.ShowObjectsTreeForm.MenuManagerWinControl.RefreshMenuTreeView();

				if (OnRefreshWorkingAreaEventHandler != null)
					OnRefreshWorkingAreaEventHandler (this, new EventArgs());
			}

            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			this.SummaryText.Cursor = currentConsoleFormCursor;
			return base.OnWizardFinish();
		}
//		//---------------------------------------------------------------------
//		private void ThreadProc()
//		{
//			if (wizardParameters != null && wizardParameters.ShowObjectsTreeForm != null)
//				wizardParameters.ShowObjectsTreeForm.ApplyGrantsToMenuXmlNode(wizardParameters.ShowObjectsTreeForm.CurrentMenuXmlNode, wizardParameters);
//		}
		//---------------------------------------------------------------------
		# endregion

	}
}

