using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Test
{
	public partial class Content : DockContent
	{
		public Content()
		{
			InitializeComponent();
		}

		private void cdsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
