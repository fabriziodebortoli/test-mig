using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//================================================================================
	internal partial class RenameCompanyFolders : ThemedForm
	{
		/// <remarks/>
		public string NewCompanyName { get { return comboNewCustomizationName.Text; } }
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public RenameCompanyFolders(string originalCompanyName)
		{
			InitializeComponent();
			lblDescription.Text = string.Format(Resources.ChooseDifferentCompany, originalCompanyName, Environment.NewLine);

			comboNewCustomizationName.Items.AddRange(GetCustomCompanies());
			comboNewCustomizationName.Text = originalCompanyName;
		}

		/// <summary>
		/// </summary>
		//-----------------------------------------------------------------------------
		public static string[] GetCustomCompanies()
		{	
			List<string> companies = new List<string>();

			string path = BasePathFinder.BasePathFinderInstance.GetCustomCompaniesPath();
			string[] dirs = Directory.GetDirectories(path);
			foreach (string dir in dirs)
			{
				DirectoryInfo di = new DirectoryInfo(dir);
				if (di == null)
					continue;

				if (di.Name.CompareNoCase(NameSolverStrings.AllCompanies))
					continue;

				companies.Add(di.Name);
			}
			return companies.ToArray();
		}	

		//-----------------------------------------------------------------------------
		private void btnAdd_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void RenameCompanyFolders_FormClosing(object sender, FormClosingEventArgs e)
		{
			//se è un nome non valido impedisco l'uscita dalla form
			if (
				this.DialogResult == DialogResult.OK &&
				string.IsNullOrEmpty(comboNewCustomizationName.Text) &&
				comboNewCustomizationName.Text.Contains(" ")
				)
			{
				MessageBox.Show
					(
						this,
						Resources.InvalidCompanyName, 
						Resources.InvalidCompanyNameDescription, 
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);
				e.Cancel = true;
			}
		}

		//-----------------------------------------------------------------------------
		private void comboNewCustomizationName_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)Keys.Space)
				return;

			e.Handled = true;
		}
	}
}
