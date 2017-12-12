using System;
using System.Net;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	public partial class TFSSourceBinding : Form
	{
		bool refreshWorkspaces = true;

		//------------------------------------------------------------------------------------
		public TFSBinding CurrentBinding
		{
			get 
			{
				return tFSBindingBindingSource.Current as TFSBinding; 
			}
		}

		//------------------------------------------------------------------------------------
		public TFSSourceBinding(TFSBinding binding)
		{
			InitializeComponent();
			IvalidateWorkspaces();
			tFSBindingBindingSource.Add(new TFSBinding(binding));
		}

		//------------------------------------------------------------------------------------
		private void txtServer_TextChanged(object sender, EventArgs e)
		{
			IvalidateWorkspaces();
		}

		//------------------------------------------------------------------------------------
		private void IvalidateWorkspaces()
		{
			refreshWorkspaces = true;
		}

		//------------------------------------------------------------------------------------
		private void txtUser_TextChanged(object sender, EventArgs e)
		{
			IvalidateWorkspaces();
		}

		//------------------------------------------------------------------------------------
		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			IvalidateWorkspaces();
		}

		//------------------------------------------------------------------------------------
		private void cbWorkspace_DropDown(object sender, EventArgs e)
		{
			if (!refreshWorkspaces)
				return;

			try
			{
				cbWorkspace.Items.Clear();

				using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(CommonFunctions.GetUri(CurrentBinding.Server, CurrentBinding.Port), new NetworkCredential(CurrentBinding.User, CurrentBinding.Password)))
				{
					
					//tfs.Authenticate();
					VersionControlServer vcs = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));

					foreach (Workspace ws in vcs.QueryWorkspaces(null, txtUser.Text, Environment.MachineName))
					{
						cbWorkspace.Items.Add(ws);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
			}

			refreshWorkspaces = false;

		}

		//------------------------------------------------------------------------------------
		private bool FormOK()
		{
			try
			{
				if (CurrentBinding.Workspace == null)
				{
					MessageBox.Show(this, "Invalid workspace!");
					return false;
				}

				if (CurrentBinding.IsValid)
					return true;
				
				MessageBox.Show(this, CurrentBinding.InvalidReason);
				return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				return false;
			}
		}

		//------------------------------------------------------------------------------------
		private void TFSSourceBinding_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
				e.Cancel = !FormOK();

		}
	}


	
}
