using Microarea.EasyBuilder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.UI
{
	internal partial class RenameItem : ThemedForm
	{
		private string oldPath;
		private string newFileLocat;
		private string newFileName;

		public string NewFileName { get { return newNameText.Text; } }
		public string NewFileLocat { get { return fileLocationText.Text; } }

		//--------------------------------------------------------------------------------
		public RenameItem()
		{
			InitializeComponent();
		}

		public RenameItem(string path)
		{
			InitializeComponent();
			this.oldPath = path;
			newFileLocat = fileLocationText.Text = Path.GetDirectoryName(path);
			newFileName = newNameText.Text = Path.GetFileName(path);
		}

		//--------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				if (String.IsNullOrEmpty(NewFileName) || String.IsNullOrEmpty(NewFileLocat))
				{
					e.Cancel = true;
				}
				if (HasInvalidChars())
				{
					e.Cancel = true;
					MessageBox.Show(this, Resources.InvalidChars, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private bool HasInvalidChars()
		{
			List<char> invalid = new List<char>();
			invalid.AddRange(Path.GetInvalidFileNameChars());
			foreach (var ch in NewFileName)
			{
				if (invalid.Contains(ch))
					return true;
			}
			return false;
		}

		private void newNameText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}
	}
}
