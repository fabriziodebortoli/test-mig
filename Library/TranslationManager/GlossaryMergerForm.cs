using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for GlossaryMergerForm.
	/// </summary>
	public partial class GlossaryMergerForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private GlossaryMerger gm = null;

		public GlossaryMergerForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public GlossaryMergerForm(GlossaryMerger gMerger)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			gm = gMerger;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		private void CMDOk_Click(object sender, System.EventArgs e)
		{
			foreach (string g in LSTGlossaries.Items)
				gm.AddGlossary(g);
			
			ArrayList errors =  gm.Execute();

			LSTGlossaries.Items.Clear();
			foreach (string s in errors)
			{
				LSTGlossaries.Items.Add(s);
			}

			CMDAdd.Enabled = false;
			CMDRemove.Enabled = false;
			CMDOk.Enabled = false;
			CMDReset.Enabled = false;
		}

		private void CMDAdd_Click(object sender, System.EventArgs e)
		{
			if (openGlossaryDialog.ShowDialog() == DialogResult.OK && openGlossaryDialog.FileName != string.Empty)
				LSTGlossaries.Items.Add(openGlossaryDialog.FileName);
		}

		private void CMDReset_Click(object sender, System.EventArgs e)
		{
			LSTGlossaries.Items.Clear();
		}

		private void CMDRemove_Click(object sender, System.EventArgs e)
		{
			if (LSTGlossaries.SelectedIndex < 0)
				return;

			LSTGlossaries.Items.Remove(LSTGlossaries.SelectedItem);
		}
	}
}
