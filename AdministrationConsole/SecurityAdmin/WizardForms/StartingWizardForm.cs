using System;
using System.Drawing;
using System.IO;
using Microarea.Console.Core.PlugIns;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public partial class StartingWizardForm : PlugInsForm
	{

		public delegate void RefreshWorkingAreaEventHandler(object sender, EventArgs e);
		public event RefreshWorkingAreaEventHandler OnRefreshWorkingAreaEventHandler = null;


		#region Data Member privati
		private ShowObjectsTree						showObjectsTree	= null;
		#endregion

		#region Costruttore
		//---------------------------------------------------------------------
		public StartingWizardForm(ShowObjectsTree aShowObjectsTree)
		{
			InitializeComponent();

			showObjectsTree = aShowObjectsTree;
		}
		//---------------------------------------------------------------------
		#endregion

		#region Move sulle LinkLabel
		private void ProtectWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetInformation(Strings.ProtectDescription, WizardOperationType.Protect);
		}
		//---------------------------------------------------------------------
		private void UnProtectWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetInformation(Strings.UnProtectDescription, WizardOperationType.Unprotect);
		}
		//---------------------------------------------------------------------
		private void CancelGrantsWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetInformation(Strings.CancelGrantsDescription, WizardOperationType.DeleteGrants);
		}
		//---------------------------------------------------------------------
		private void SetGrantsWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetInformation(Strings.SetGrantsDescription, WizardOperationType.SetGrants);
		}
		//---------------------------------------------------------------------

		private void SetInformation(string operationDescription, WizardOperationType type)
		{
			DetailsLabel.Text = operationDescription;
            Stream myStream = WizardStringMaker.GetImageByOperationType(type);
			if (myStream == null) return;
			OperationPictureBox.Image = Image.FromStream (myStream, true);
			
		}
		#endregion

		#region Click sulle LinkLabel
		private void ProtectWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowWizard(WizardOperationType.Protect);
		}
		//---------------------------------------------------------------------
		private WizardParameters SetParametersForWizard(WizardOperationType type)
		{
			WizardParameters wizardParameters		= new WizardParameters();
			wizardParameters.OperationType			= type;
			wizardParameters.ShowObjectsTreeForm	= showObjectsTree;
	
			return wizardParameters;
		}
		//---------------------------------------------------------------------
		private void ShowWizard(WizardOperationType operationType)
		{
			SecurityWizardManager wizardManager = new SecurityWizardManager(SetParametersForWizard(operationType));
			
			wizardManager.AddWizardPage(new WelcomeWizardForm());
			wizardManager.AddWizardPage(new SelectObjectsWizardForm());

			if (operationType == WizardOperationType.SetGrants || operationType == WizardOperationType.DeleteGrants)
				wizardManager.AddWizardPage(new SelectRolesAndUsersForm());
			if (operationType == WizardOperationType.SetGrants)
				wizardManager.AddWizardPage(new SetGrantsWizardForm());
			
			SummaryWizardForm summaryWizardForm = new SummaryWizardForm();
			summaryWizardForm.OnRefreshWorkingAreaEventHandler += new SummaryWizardForm.RefreshWorkingAreaEventHandler(RefreshWorkingArea);
			wizardManager.AddWizardPage(summaryWizardForm);

			wizardManager.Run();
			
		}

		//---------------------------------------------------------------------
		private void RefreshWorkingArea(object sender, EventArgs e)
		{
			if (OnRefreshWorkingAreaEventHandler != null)
				OnRefreshWorkingAreaEventHandler (sender, new EventArgs());

		}

		//---------------------------------------------------------------------
		private void UnProtectWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowWizard(WizardOperationType.Unprotect);
		}
		//---------------------------------------------------------------------
		private void CancelGrantsWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowWizard(WizardOperationType.DeleteGrants);
		}
		//---------------------------------------------------------------------
		private void SetGrantsWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowWizard(WizardOperationType.SetGrants);
		}

		#endregion
	}
}

