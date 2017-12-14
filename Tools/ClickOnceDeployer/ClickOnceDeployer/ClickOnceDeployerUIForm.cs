using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ClickOnceDeployer
{
	public partial class ClickOnceDeployerUIForm : Form
	{
		
		string command = string.Empty;


		internal Program.Action Action { get; set; }
		internal string Version { get { return groupBoxVersion.Tag.ToString(); } set { groupBoxVersion.Tag = value; } }
		internal string Root { get { return root; } set { root = value; } }
		internal string InstallationName { get { return txtboxInstallationName.Text; } set { txtboxInstallationName.Text = value; } }
		internal string UICulture { get { return txtboxUiCulture.Text; } set { txtboxUiCulture.Text = value; } }
		internal string WebServicesPort { get { return txtboxWebServicePort.Text; } set { txtboxWebServicePort.Text = value; } }
		internal string User { get { return txtboxUser.Text; } set { txtboxUser.Text = value; } }
		internal string InstallationPath { get { return installationpath; } set { installationpath = value; } }
		internal string RootBack { get { return txtboxRoot.Text; } }

		string root = string.Empty;
		string installationpath =string.Empty;

		public ClickOnceDeployerUIForm()
		{
			InitializeComponent();

			this.radioButtonDebug.CheckedChanged += (sender, e) => SetVersion(sender, e);
			this.radioButtonRelease.CheckedChanged += (sender, e) => SetVersion(sender, e);
			this.comboBoxActions.TextChanged += (sender ,e ) => CheckAction(sender);
		}

	
		private void SetAction(object sender)
		{
			if (sender is RadioButton )
			{this.groupBoxActions.Tag = (sender as RadioButton).Tag;}		
		}

		private void SetVersion(object sender,EventArgs e)
		{
			if (sender is RadioButton)
			{ this.groupBoxVersion.Tag = (sender as RadioButton).Tag; }
		}



		private void ClickOnceDeployerUIForm_Load(object sender, EventArgs e)
		{
			this.PopulateActionsComboBox();
					
		}

		private void btnRunCmd_Click(object sender, EventArgs e)
		{
			
		   this.Action = (Program.Action)(comboBoxActions.SelectedItem);
		
		}		
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	
		private void PopulateActionsComboBox()
		{
			foreach (object o in Enum.GetValues(typeof(Program.Action)))


				if (!(o as Enum).Equals(Program.Action.None) && !(o as Enum).Equals(Program.Action.UI))
				{ comboBoxActions.Items.Add(o); }
				

			    comboBoxActions.SelectedItem = Program.Action.Deploy;

		}
		private void  CheckAction(object sender)
		{

			if ((sender as ComboBox).SelectedItem.Equals(Program.Action.RegisterWcf) || (sender as ComboBox).SelectedItem.Equals(Program.Action.UnregisterWcf))
			{
				this.txtboxRoot.Text = installationpath;
			
			}
			else
			{				
				this.txtboxRoot.Text = root;
			}
		}
	


	}
}
