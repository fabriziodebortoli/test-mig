using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for SelectFileType.
	/// </summary>
	public partial class SelectFileType : System.Windows.Forms.Form
	{
		public ArrayList FileTypeList = new ArrayList();

		public SelectFileType()
		{
			InitializeComponent();
		}

		private void CMDOk_Click(object sender, System.EventArgs e)
		{
			foreach (LookUpFileType luft in CHKFileTypeList.CheckedItems)
			{
				FileTypeList.Add(luft);
			}

			Close();
		}

		private void SelectFileType_Load(object sender, System.EventArgs e)
		{
			FileTypeList.Clear();
			CHKFileTypeList.Items.Clear();

			CHKFileTypeList.Items.Add(LookUpFileType.ClientDocuments);
			CHKFileTypeList.Items.Add(LookUpFileType.Dbts);
			CHKFileTypeList.Items.Add(LookUpFileType.Documents);
			CHKFileTypeList.Items.Add(LookUpFileType.ReferenceObjects);
			CHKFileTypeList.Items.Add(LookUpFileType.Report);
			CHKFileTypeList.Items.Add(LookUpFileType.Structure);
			CHKFileTypeList.Items.Add(LookUpFileType.Tables);
			CHKFileTypeList.Items.Add(LookUpFileType.WebMethods);
			CHKFileTypeList.Items.Add(LookUpFileType.XTech);
			CHKFileTypeList.Items.Add(LookUpFileType.Activation);
			CHKFileTypeList.Items.Add(LookUpFileType.Misc);
		}

		private void CMDSelectAll_Click(object sender, System.EventArgs e)
		{
			for (int idx = 0; idx < CHKFileTypeList.Items.Count; idx++)
			{
				CHKFileTypeList.SetItemChecked(idx, true);
			}
		}

		private void CMDSelectNone_Click(object sender, System.EventArgs e)
		{
			for (int idx = 0; idx < CHKFileTypeList.Items.Count; idx++)
			{
				CHKFileTypeList.SetItemChecked(idx, false);
			}
		}
	}
}
