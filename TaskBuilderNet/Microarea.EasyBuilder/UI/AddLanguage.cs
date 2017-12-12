using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.UI
{
	internal partial class AddLanguage : ThemedForm
	{
		List<CultureInfo> languages;
		//--------------------------------------------------------------------------------
		public AddLanguage(List<CultureInfo> languages)
		{
			InitializeComponent();
			this.languages = languages;
		}

		//--------------------------------------------------------------------------------
		public CultureInfo SelectedLanguage { get { return comboLanguages.SelectedItem as CultureInfo; } }

		//--------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			comboLanguages.Items.AddRange(languages.ToArray());
			comboLanguages.Sorted = true;
			comboLanguages.DisplayMember = "DisplayName";
		}

		//--------------------------------------------------------------------------------
		private void btnAdd_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		//--------------------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}

		//--------------------------------------------------------------------------------
		private void AddLanguage_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != System.Windows.Forms.DialogResult.OK)
				return;

			if (comboLanguages.SelectedIndex < 0)
			{
				MessageBox.Show(this, Resources.SelectLanguageToAdd, Resources.AddLanguage, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				e.Cancel = true;
				return;
			}
		}
	}
}
