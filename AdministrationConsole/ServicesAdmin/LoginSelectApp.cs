using System;
using System.Collections;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	//=========================================================================
	public partial class LoginSelectApp : Form
	{
		public ArrayList SelectedApplicationList = new ArrayList();

		//--------------------------------------------------------------------------			
		public LoginSelectApp()
		{
			InitializeComponent();
		}

		//--------------------------------------------------------------------------			
		public LoginSelectApp(ArrayList applicationList)
		{
			InitializeComponent();
			foreach (string app in applicationList)
				AppListBox.Items.Add(app);

		}

		//--------------------------------------------------------------------------			
		private void OKButton_Click(object sender, EventArgs e)
		{
			if (AppListBox.SelectedItems == null || AppListBox.SelectedItems.Count == 0)
			{
				MessageBox.Show(this.Parent,"Nessun elemento selezionato", this.Text);
				return;
			}
			foreach (string app in AppListBox.SelectedItems)
				SelectedApplicationList.Add(app);

			DialogResult = DialogResult.OK;
			Close();
		}

		//--------------------------------------------------------------------------			
		private void CancelButton_Click(object sender, EventArgs e)
		{
			SelectedApplicationList.Clear();
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}