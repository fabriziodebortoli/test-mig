using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.Text.RegularExpressions;

namespace Microarea.EasyBuilder.UI
{
	internal partial class TableSourceControl : UserControl
	{
		public event EventHandler NoSelection;
		public event EventHandler SelectedTableChanged;

		public string TableName { get { return cbxTableName.SelectedItem as string; } }
		public bool TableChangeable { get { return cbxTableName.Enabled; } set { cbxTableName.Enabled = value; } }
		public string ObjectName { get { return txtName.Text; } set { txtName.Text = value; } }
		public Color SetMessageColor(Color color)
		{
			Color old = lblDBTCreated.ForeColor;
			lblDBTCreated.ForeColor = color;
			lblDBTCreated.Update();
			return old;
		}
		public void SetMessage(string text)
		{
			lblDBTCreated.Text = text;
		}

		public TableSourceControl()
		{
			InitializeComponent();
		}

		public bool InitTables(string initialTableName)
		{
			//provo a selezionare la tabella che ha scelto l'utente, se non ci riesco devo disabilitare la finestra
			if (!initialTableName.IsNullOrEmpty())
			{
				cbxTableName.SelectedItem = initialTableName;
				string selectedItemText = cbxTableName.SelectedItem.ToString();
				if (selectedItemText.IsNullOrEmpty() || selectedItemText.CompareNoCase(Resources.NoneItem))
				{
					cbxTableName.Enabled = false;
					txtName.Enabled = false;
					return false;
				}
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		public void FillCatalogCombo(Dictionary<string, IRecord> dbObjects)
		{
			cbxTableName.Items.Add(Resources.NoneItem);

			foreach (string record in dbObjects.Keys)
				cbxTableName.Items.Add(record);

			cbxTableName.SelectedIndex = 0;
		}

		//-----------------------------------------------------------------------------
		internal bool ValidateData()
		{
			string tableName = cbxTableName.SelectedItem as string;
			if (string.IsNullOrEmpty(tableName) || tableName == Resources.NoneItem)
				return false;

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (string.IsNullOrEmpty(tableName))
			{
				sb.Append(Microarea.EasyBuilder.Properties.Resources.MissingObjectTable);
				sb.Append(Environment.NewLine);
			}

			if (string.IsNullOrEmpty(txtName.Text))
			{
				sb.Append(Microarea.EasyBuilder.Properties.Resources.MissingObjectName);
				sb.Append(Environment.NewLine);
			}

			Regex rgx = new Regex(@"[^a-zA-Z0-9]+");//tutto ciò che non è lettera o numero non è valido
			Match match = rgx.Match(txtName.Text);
			if (match.Success)
			{
				sb.Append(string.Format(Resources.InvalidObjectName, match.Value));
				sb.Append(Environment.NewLine);
			}

			if (!string.IsNullOrEmpty(sb.ToString()))
			{
				MessageBox.Show(sb.ToString());
				return false;
			}
			return Enabled;
		}
		//-----------------------------------------------------------------------------
		private void cbxTableName_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				string nameToFind = string.Empty;
				bool noSelection = cbxTableName.SelectionLength == 0;

				// back space
				if (e.KeyChar == (char)Keys.Back)
				{
					if (cbxTableName.SelectionStart <= 1)
					{
						cbxTableName.Text = string.Empty;
						return;
					}

					nameToFind = noSelection ?
									cbxTableName.Text.Substring(0, cbxTableName.Text.Length - 1) :
									cbxTableName.Text.Substring(0, cbxTableName.SelectionStart - 1);
				}
				else
					nameToFind = noSelection ?
									cbxTableName.Text + e.KeyChar :
									cbxTableName.Text.Substring(0, cbxTableName.SelectionStart) + e.KeyChar;

				int foundIdx = cbxTableName.FindString(nameToFind);

				if (foundIdx < 0)
				{
					if (NoSelection != null)
						NoSelection(this, EventArgs.Empty);

					txtName.Text = string.Empty;
					return;
				}

				cbxTableName.SelectedText = string.Empty;
				cbxTableName.SelectedIndex = foundIdx;

				cbxTableName.SelectionStart = nameToFind.Length;
				cbxTableName.SelectionLength = cbxTableName.Text.Length;
				e.Handled = true;
			}
			catch (Exception)
			{
			}
		}

		private void cbxTableName_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedTableChanged != null)
				SelectedTableChanged(this, EventArgs.Empty);
		} 
		
	}
}
