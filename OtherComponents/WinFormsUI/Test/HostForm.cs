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
	public partial class HostForm : Form
	{
		public HostForm()
		{
			InitializeComponent();
		}
		static int index = 1;
		private void button1_Click(object sender, EventArgs e)
		{
			Content c = new Content();
			int xx = new Random().Next(15);
			c.Text = "Company " + index++;
			c.Show(dockPanel1);
			
			c = new Content();
			xx = new Random().Next(15);
			c.Text = "Company " + index++;
			c.Show(dockPanel1);

			//dockPanel1.ac
			//c.Pane.HideDocumentTabs = true;//non voglio vedere la linguetta del documento
			//c.Pane.Controls.Remove(c.Pane.TabStripControl);

			//Content c1 = new Content();
			//xx = new Random().Next(15);
			//c1.Text = "Company " + new string('*', xx) + index++;
			//c1.Show(dockPanel1, new Rectangle(0,0, 300,300));
		}

		private void button2_Click(object sender, EventArgs e)
		{
			dockPanel1.ActivatePrevious();
		}

		
	}
}
