using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	public partial class SecurityLightMigrationForm : System.Windows.Forms.Form
	{
		private SecurityLightMigrationEngine securityLightMigrationEngine = null;

		//--------------------------------------------------------------------------------
		public event System.EventHandler ProcedureStarted;
		public event System.EventHandler ProcedureEnded;
		public event System.EventHandler ProcedureAborted;

		//--------------------------------------------------------------------------------
		public SecurityLightMigrationForm(string connectionString, int sourceCompanyId, int desctinationCompanyId)
		{
			InitializeComponent();

			securityLightMigrationEngine = new SecurityLightMigrationEngine(connectionString, Convert.ToInt32(sourceCompanyId), Convert.ToInt32(desctinationCompanyId));
			securityLightMigrationEngine.SetProgressBarMaxValue += new SecurityLightMigrationEngine.SetProgressBarMaxValueEventHandler(SecurityMigrationProcess_SetProgressBarMaxValue);
			securityLightMigrationEngine.ChangeProgressBarText += new SecurityLightMigrationEngine.ChangeProgressBarTextEventHandler(SecurityMigrationProcess_ChangeProgressBarText);
			securityLightMigrationEngine.PerformProgressBarStep += new SecurityLightMigrationEngine.PerformProgressBarStepEventHandler(SecurityMigrationProcess_ProgressBarStep);
			securityLightMigrationEngine.ProcedureEnded += new System.EventHandler(SecurityMigrationProcess_ProcedureEnded);
		}

		//---------------------------------------------------------------------
		public void SecurityMigrationProcess_ProcedureEnded(object sender, System.EventArgs e)
		{
			UpdateUndoButtonText();

			if (ProcedureEnded != null)
				ProcedureEnded(this, System.EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		public void SecurityMigrationProcess_ProgressBarStep(object sender, string nameSpace)
		{
			ProgressBarPerformStep();
			UpdateNameSpaceLabelText(nameSpace);
		}

		//---------------------------------------------------------------------
		private void SecurityMigrationProcess_SetProgressBarMaxValue(object sender, int progressValue)
		{
			SetProgressBarMinimumValue();
			SetProgressBarMaximumValue(progressValue);
		}

		//---------------------------------------------------------------------
		private void SecurityMigrationProcess_ChangeProgressBarText(object sender, string message)
		{
			ChangeProgressBarText(message);
		}

		//---------------------------------------------------------------------
		private void UndoButton_Click(object sender, System.EventArgs e)
		{
			AbortMigrationProcedure();
		}

		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (securityLightMigrationEngine != null)
			{
				securityLightMigrationEngine.StartMigrationThread();
				if (ProcedureStarted != null && securityLightMigrationEngine.IsProcessing)
					ProcedureStarted(this, EventArgs.Empty);
			}
		}

		//---------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel && securityLightMigrationEngine != null && securityLightMigrationEngine.IsProcessing)
				AbortMigrationProcedure();
		}

		//---------------------------------------------------------------------
		private void AbortMigrationProcedure()
		{
			if (securityLightMigrationEngine == null || !securityLightMigrationEngine.IsProcessing)
				return;

			securityLightMigrationEngine.SuspendMigrationThread();

			if (MessageBox.Show(this, Strings.StopProcedureQuestion, Strings.MigrationFromSecurityLight,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
				return;

			securityLightMigrationEngine.AbortMigrationThread();

			if (ProcedureAborted != null)
				ProcedureAborted(this, System.EventArgs.Empty);

			this.Close();
		}

		# region Metodi gestione cross-thread

		//---------------------------------------------------------------------
		private void UpdateUndoButtonText()
		{
			Invoke(new MethodInvoker(() => { this.UndoButton.Text = Strings.ProcedureEndedUndoButtonText; }));
		}
		
		//---------------------------------------------------------------------
		private void ProgressBarPerformStep()
		{
			Invoke(new MethodInvoker(() => { ExecutionProgressBar.PerformStep(); }));
		}

		//---------------------------------------------------------------------
		private void UpdateNameSpaceLabelText(string nameSpace)
		{
			Invoke(new MethodInvoker(() => { NameSpaceLabel.Text = nameSpace; }));
		}

		//---------------------------------------------------------------------
		private void SetProgressBarMinimumValue()
		{
			Invoke(new MethodInvoker(() => { ExecutionProgressBar.Minimum = 1; }));
		}

		//---------------------------------------------------------------------
		private void SetProgressBarMaximumValue(int progressValue)
		{
			Invoke(new MethodInvoker(() => { ExecutionProgressBar.Maximum = progressValue; }));
		}

		//---------------------------------------------------------------------
		private void ChangeProgressBarText(string message)
		{
			Invoke(new MethodInvoker(() => { MigrationStepDescriptionLabel.Text = message; }));
		}
		#endregion
	}
}
