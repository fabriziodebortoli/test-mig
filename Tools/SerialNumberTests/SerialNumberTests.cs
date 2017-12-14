using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;

namespace Microarea.SerialNumberTests
{
	//---------------------------------------------------------------------
	public class SerialNumberTests : System.Windows.Forms.Form
	{
		private IContainer components;
		private Button BtnTest;
		private ListView listView1;
		private ImageList imageList1;
		private Label label3;
		private TextBox textBox2;
        private RichTextBox richTextBox1;
		private Label label1;

		string repository = null;
		int count = 0;
		string custom = null;
		ActivationObject ao = null;
        private Button BtnClear;
        private ContextMenuStrip contextMenu1;
        private ToolStripMenuItem testToolStripMenuItem;
		BasePathFinder pf =  BasePathFinder.BasePathFinderInstance;

		//---------------------------------------------------------------------
		public SerialNumberTests()
		{
			InitializeComponent();
			listView1.Columns.Add(string.Empty,	20,  HorizontalAlignment.Left);
			listView1.Columns.Add(" File N_C_EL_MD_ML_WM",500, HorizontalAlignment.Left);
			custom = @"C:\{0}\Standard\TaskBuilder\WebFramework\LoginManager\App_Data";
		}

		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialNumberTests));
            this.BtnTest = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnClear = new System.Windows.Forms.Button();
            this.contextMenu1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnTest
            // 
            this.BtnTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.BtnTest.Location = new System.Drawing.Point(539, 24);
            this.BtnTest.Name = "BtnTest";
            this.BtnTest.Size = new System.Drawing.Size(75, 23);
            this.BtnTest.TabIndex = 1;
            this.BtnTest.Text = "Test all";
            this.BtnTest.Click += new System.EventHandler(this.button1_Click);
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.Lavender;
            this.listView1.ContextMenuStrip = this.contextMenu1;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(20, 72);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(692, 399);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.contextMenu1.Name = "contextMenu1";
            this.contextMenu1.Size = new System.Drawing.Size(97, 26);
            this.contextMenu1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu1_Opening);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("testToolStripMenuItem.Image")));
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.testToolStripMenuItem.Text = "Test";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "lin-agt-wrench.ico");
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(20, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Installation ";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(123, 24);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(264, 22);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "Development";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(20, 500);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(692, 80);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(20, 479);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Messaggi:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnClear
            // 
            this.BtnClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.BtnClear.Location = new System.Drawing.Point(633, 24);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(75, 23);
            this.BtnClear.TabIndex = 1;
            this.BtnClear.Text = "Clear all";
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // SerialNumberTests
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(7, 15);
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(720, 592);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.BtnClear);
            this.Controls.Add(this.BtnTest);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SerialNumberTests";
            this.Text = "SerialNumberTests";
            this.contextMenu1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//---------------------------------------------------------------------
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new SerialNumberTests());
		}

		//---------------------------------------------------------------------
		private bool Prepare()
		{
			if(textBox2.Text.Length == 0)
			{MessageBox.Show("Installation mancante");return false;}

			custom = String.Format(custom, textBox2.Text.Trim());
			if  (!Directory.Exists(custom))
			{MessageBox.Show("Installation non valida");return false;}
			
			repository =   Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))), "Files");
			count = 0;
			if (repository.Length == 0 || !Directory.Exists(repository)) 
			{MessageBox.Show("repository mancante");return false;}
			
			return true;
		}

		//---------------------------------------------------------------------
		private void Clean()
		{
			foreach(string fis1 in Directory.GetFiles(custom, "*.Licensed.config"))
				File.Delete(fis1);
		}

		//---------------------------------------------------------------------
		private void button1_Click(object sender, System.EventArgs e)
		{
            BtnClear_Click(sender, e);


			if (!Prepare ())
				return;

            int namedCal = 0, gdiConcurrent = 0, unNamedCal = 0, officeCal = 0, tpCal = 0, WMSCal = 0;
			Hashtable dummy2 = null;
			Hashtable dummy1 = null;
			bool error = false;

			foreach(string fis in Directory.GetFiles(repository, "*_Test_*"))
			{
				Clean();//pulisco
				//copia pinga testa
				FileInfo fi = new FileInfo(fis);

				ListViewItem item = new ListViewItem();
				item.SubItems.Add(fi.Name);
				item.ImageIndex = 1;
				string res = Path.GetExtension(fi.FullName).Remove(0,1);//tolgo il punto
				string[] ress = res.Split('_');
				if(ress.Length < 7)
				{
					richTextBox1.AppendText("-->");
					richTextBox1.AppendText("Formato file non corretto: " + fi.Name);
					richTextBox1.AppendText(Environment.NewLine);
					listView1.Items.Add(item);
					error = true;
					continue;
				}
                int n, c, el, md, ml, wm = 0;
				try
				{
					n = Int32.Parse(ress[0]);
					c = Int32.Parse(ress[1]);
					el = Int32.Parse(ress[2]);
					md = Int32.Parse(ress[3]);
					ml = Int32.Parse(ress[4]);
                    wm = Int32.Parse(ress[5]);
				}
				catch (Exception exc)
				{
					richTextBox1.AppendText("-->");
					richTextBox1.AppendText("Eccezione per formato file non corretto: " + fi.Name);
					richTextBox1.AppendText(Environment.NewLine);
					richTextBox1.AppendText(exc.ToString());
					richTextBox1.AppendText(Environment.NewLine);
					listView1.Items.Add(item);
					error = true;
					continue;
				}

				File.Copy(fi.FullName, Path.Combine(custom, Path.GetFileNameWithoutExtension(fi.FullName)));
				Debug.WriteLine("Ping  sul file:" + fi.Name);
				if (!Init()) 
				{
					listView1.Items.Add(item);
					error = true;
					continue;
				}

				ao.GetCalNumber(out namedCal, out gdiConcurrent, out unNamedCal, out officeCal, out tpCal, out WMSCal,out dummy1, out dummy2);
				if (n ==  namedCal &&
                    c == gdiConcurrent && wm == WMSCal &&
					(el ==  unNamedCal  || (el ==999 && unNamedCal == Int32.MaxValue )) && 
					(md ==  officeCal  || (md ==999 && officeCal == Int32.MaxValue )) &&
					(ml ==  tpCal  || (ml ==999 && tpCal == Int32.MaxValue )) )
					item.ImageIndex = 0;
				else
					error = true;
				listView1.Items.Add(item);
				
				listView1.EnsureVisible(count);
				count ++;
				Application.DoEvents();
			}


			ListViewItem lastItem = new ListViewItem();
			string app = error? " con errori": " correttamente";
			lastItem.SubItems.Add("Procedura terminata" + app);

			lastItem.ImageIndex = error? 2: 3;
			listView1.Items.Add(lastItem);
			listView1.EnsureVisible(count);
		}

		//---------------------------------------------------------------------
		private bool Init()
		{
			try
			{
				FSProviderForInstalled fsProvider = 
					new FSProviderForInstalled(pf);
				ao = new ActivationObject(fsProvider);
			}
			catch (Exception exc)
			{richTextBox1.AppendText("-->");
				richTextBox1.AppendText("Eccezione inizializzando ActObj");
				richTextBox1.AppendText(Environment.NewLine);
				richTextBox1.AppendText(exc.ToString());
				richTextBox1.AppendText(Environment.NewLine);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			if (repository.Length == 0 || !Directory.Exists(repository)) 
			{MessageBox.Show("repository mancante");return;}

			ListView list = (ListView)sender;

			if (list.SelectedItems == null ||
				list.SelectedItems.Count == 0
				|| list.SelectedItems.Count > 1)
				return;

			ListViewItem item = list.SelectedItems[0];
			if (item.ImageIndex > 1 ) return;
			string file = Path.Combine(repository, item.SubItems[1].Text);
			if (!File.Exists(file)) return;
			ProcessStartInfo s = new ProcessStartInfo("notepad", file);
		
			System.Diagnostics.Process.Start(s);
		}

		//---------------------------------------------------------------------
		private void TestSingle()
		{
			if (!Prepare())
			{
				MessageBox.Show("Error");
				return;
			}
			//pulisce copia pinga scrive
			Clean();
			FileInfo fi = new FileInfo(Path.Combine(repository,((ListViewItem)listView1.SelectedItems[0]).SubItems[1].Text));
			
			if (!File.Exists(fi.FullName)) return;
			File.Copy(fi.FullName, Path.Combine(custom, Path.GetFileNameWithoutExtension(fi.FullName)));
			Debug.WriteLine("Ping  sul file:" + fi.Name);
			if (!Init()) 
			{
				MessageBox.Show("Error");
				return;
			}
            int namedCal = 0, gdiConcurrent = 0, unNamedCal = 0, officeCal = 0, tpCal = 0, WMSCal=0;
			Hashtable dummy2 = null;
			Hashtable dummy1= null;

			ao.GetCalNumber(out namedCal, out gdiConcurrent, out unNamedCal, out officeCal, out tpCal, out WMSCal, out dummy1, out dummy2);

            if (unNamedCal == Int32.MaxValue) unNamedCal = 999;
            if (officeCal == Int32.MaxValue) officeCal = 999;
            if (tpCal == Int32.MaxValue) tpCal = 999;
            MessageBox.Show(namedCal.ToString() + "_" + gdiConcurrent.ToString() + "_" + unNamedCal.ToString() + "_" + officeCal.ToString() + "_" + tpCal.ToString() + "_" + WMSCal.ToString());
		}
	
        //---------------------------------------------------------------------
        private void BtnClear_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            richTextBox1.Clear();
        }

        //---------------------------------------------------------------------
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null && listView1.SelectedItems.Count ==1)
            TestSingle();
        }

        //---------------------------------------------------------------------
        private void contextMenu1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count != 1)
                e.Cancel = true;
        }

  
	}
}


