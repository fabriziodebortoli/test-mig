using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Security.Permissions;

using Writer = Microarea.Library.SalesModulesWriter;

using Microarea.Library.SMBaseHandler;
using Microarea.TaskBuilderNet.Licence.SalesModulesReader;

namespace Tester
{
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button BtnBrowse;
		private System.Windows.Forms.TextBox TxtFile;
		private System.Windows.Forms.Button BtnDecrypt;
		private System.Windows.Forms.Button BtnClear;
		private System.Windows.Forms.Button BtnSimpleCrypt;
		private System.Windows.Forms.Button BtnAdd;
		private System.Windows.Forms.Button BtnRemove;
		private System.Windows.Forms.ListBox ListFile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtlogin;
		private System.Windows.Forms.TextBox txtpsw;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox TxtForceProducer;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox TxtPort;
		private System.Windows.Forms.TextBox TxtServer;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.RichTextBox label1;
		private System.Windows.Forms.Label label11;
		private System.ComponentModel.Container components = null;
		
		//---------------------------------------------------------------------
		public Form1()
		{
			InitializeComponent();
			TxtForceProducer.Visible = TxtForceProducer.Enabled = IsDevelopment();
			
		}

		/// <summary>
		/// Verifica se è stata definita la variabile di ambiente 
		/// MicroareaDevelopment settata a 1 o a true e se si è in DEBUG.
		/// </summary>
		//-----------------------------------------------------------------------
		public static bool IsDevelopment()
		{
			string MicroareaDevelopment = String.Empty;
			MicroareaDevelopment = Environment.GetEnvironmentVariable("MicroareaDevelopment");
#if DEBUG
			return	String.Compare(MicroareaDevelopment, "1", true) == 0 || 
				String.Compare(MicroareaDevelopment, bool.TrueString, true) == 0;
#else
			return false;
#endif
		}



		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//---------------------------------------------------------------------
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new Form1());
		}

		//---------------------------------------------------------------------
		private void CryptByWS(ArrayList filenames, string login, string password, string product)
		{
			if (filenames.Count == 0)
			{
				label1.Text = "Nessun file selezionato";
				return;
			}
			foreach (string filename in filenames)
			{
				if (filename == String.Empty || !File.Exists(filename))
				{
					label1.Text = "Path non valido: " + filename;
					return ;
				}
			}
			Components.WSRequestWrapper c = new  Tester.Components.WSRequestWrapper();
			string dest;
			Microarea.Library.SMBaseHandler.MessagesInfoWriter writer;
			
			bool ok = c.CallWS(filenames,TxtForceProducer.Text, out writer, out dest,  login, password, product);
			Microarea.Library.SMBaseHandler.MessagesInfoReader reader = new Microarea.Library.SMBaseHandler.MessagesInfoReader(writer);
			string s= null;

			if (reader.Messages != null)
			{
				foreach (Microarea.Library.SMBaseHandler.MessageInfo msg in reader.Messages)
				{
					s += msg.ToString();
					s += Environment.NewLine;
				}
			}
			label1.Text = s;
			if (ok)
				Decrypt(dest);

		}

		//---------------------------------------------------------------------
		private void BtnSimpleCrypt_Click(object sender, System.EventArgs e)
		{
			string product = textBox1.Text;
			if (product == null || product.Length == 0)
			{
				label1.Text = "Manca nome prodotto.";
				return;
			}
			if (txtlogin.Text.Length == 0)
			{
				label1.Text = "Manca login.";
				return;
			}
			if (txtpsw.Text.Length == 0)
			{
				label1.Text = "Manca password.";
				return;
			}
			ArrayList list = new ArrayList();
			foreach (string item in ListFile.Items)
				list.Add(item.ToString());
			string login = txtlogin.Text;
			string password = txtpsw.Text;
			
			CryptByWS(list, login, password, textBox1.Text);	
		}

		
		//---------------------------------------------------------------------
		private bool Decrypt(string path)
		{
			XmlDocument doc;
			bool ok = SMManager.HandlerOff(path, out doc);
			if (!ok)
			{
				label1.Text =SMManager.ErrorMessage;
				return false;
			}
			richTextBox1.Text = path + Environment.NewLine + doc.OuterXml;
			return true;
		
		}		

		//---------------------------------------------------------------------
		private void BtnClear_Click(object sender, System.EventArgs e)
		{
			richTextBox1.Clear();
			label1.Text = "";
		}

		//---------------------------------------------------------------------
		private void BtnBrowse_Click(object sender, System.EventArgs e)
		{
			label1.Text = "";
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			ofd.FileName = TxtFile.Text;
			DialogResult res =  ofd.ShowDialog(this);
			if (res == DialogResult.Cancel)
				return;
			string[] files = ofd.FileNames;
			if (files != null && files.Length > 0)
			{
				TxtFile.Text = files[0];
				foreach (string s in files)
					ListFile.Items.Add(s);
			}
	
		}

		//---------------------------------------------------------------------
		private void BtnDecrypt_Click(object sender, System.EventArgs e)
		{
			label1.Text = "";
			bool ok = true;
			foreach (string s in ListFile.Items)
				ok = ok && (Decrypt(s));
			if(ok)
				label1.Text = "Procedura terminata correttamente.";
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			if (TxtFile.Text.Length > 0)
				ListFile.Items.Add(TxtFile.Text);
		}

		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			if (ListFile.SelectedItem != null)
				ListFile.Items.Remove(ListFile.SelectedItem);
		}

		//---------------------------------------------------------------------
		private void button1_Click(object sender, System.EventArgs e)
		{
			ListFile.Items.Clear();
		}

		//---------------------------------------------------------------------
		private void ListFile_DoubleClick(object sender, System.EventArgs e)
		{
			if (ListFile.SelectedItem == null)
				return;

			string s = ListFile.SelectedItem as string;
			if (s == null)
				return;
			Process.Start(s);

		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this.BtnDecrypt = new System.Windows.Forms.Button();
			this.TxtFile = new System.Windows.Forms.TextBox();
			this.BtnBrowse = new System.Windows.Forms.Button();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.BtnSimpleCrypt = new System.Windows.Forms.Button();
			this.BtnClear = new System.Windows.Forms.Button();
			this.ListFile = new System.Windows.Forms.ListBox();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtlogin = new System.Windows.Forms.TextBox();
			this.txtpsw = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.TxtForceProducer = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.TxtPort = new System.Windows.Forms.TextBox();
			this.TxtServer = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.RichTextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// BtnDecrypt
			// 
			this.BtnDecrypt.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnDecrypt.Location = new System.Drawing.Point(120, 352);
			this.BtnDecrypt.Name = "BtnDecrypt";
			this.BtnDecrypt.Size = new System.Drawing.Size(72, 23);
			this.BtnDecrypt.TabIndex = 3;
			this.BtnDecrypt.Text = "Decrypt";
			this.BtnDecrypt.Click += new System.EventHandler(this.BtnDecrypt_Click);
			// 
			// TxtFile
			// 
			this.TxtFile.Location = new System.Drawing.Point(26, 176);
			this.TxtFile.Name = "TxtFile";
			this.TxtFile.Size = new System.Drawing.Size(686, 21);
			this.TxtFile.TabIndex = 0;
			this.TxtFile.Text = "C:\\Development\\Standard\\Applications\\ERP\\Solutions\\Modules";
			// 
			// BtnBrowse
			// 
			this.BtnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnBrowse.Location = new System.Drawing.Point(720, 176);
			this.BtnBrowse.Name = "BtnBrowse";
			this.BtnBrowse.Size = new System.Drawing.Size(24, 23);
			this.BtnBrowse.TabIndex = 1;
			this.BtnBrowse.Text = "...";
			this.BtnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(24, 408);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(784, 136);
			this.richTextBox1.TabIndex = 8;
			this.richTextBox1.TabStop = false;
			this.richTextBox1.Text = "";
			// 
			// BtnSimpleCrypt
			// 
			this.BtnSimpleCrypt.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnSimpleCrypt.Location = new System.Drawing.Point(24, 352);
			this.BtnSimpleCrypt.Name = "BtnSimpleCrypt";
			this.BtnSimpleCrypt.Size = new System.Drawing.Size(72, 23);
			this.BtnSimpleCrypt.TabIndex = 19;
			this.BtnSimpleCrypt.Text = "Crypt";
			this.BtnSimpleCrypt.Click += new System.EventHandler(this.BtnSimpleCrypt_Click);
			// 
			// BtnClear
			// 
			this.BtnClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnClear.Location = new System.Drawing.Point(728, 544);
			this.BtnClear.Name = "BtnClear";
			this.BtnClear.Size = new System.Drawing.Size(75, 23);
			this.BtnClear.TabIndex = 18;
			this.BtnClear.Text = "Clear";
			this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// ListFile
			// 
			this.ListFile.HorizontalScrollbar = true;
			this.ListFile.Location = new System.Drawing.Point(24, 224);
			this.ListFile.Name = "ListFile";
			this.ListFile.Size = new System.Drawing.Size(784, 108);
			this.ListFile.TabIndex = 20;
			this.ListFile.DoubleClick += new System.EventHandler(this.ListFile_DoubleClick);
			// 
			// BtnAdd
			// 
			this.BtnAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnAdd.Location = new System.Drawing.Point(752, 176);
			this.BtnAdd.Name = "BtnAdd";
			this.BtnAdd.Size = new System.Drawing.Size(48, 23);
			this.BtnAdd.TabIndex = 21;
			this.BtnAdd.Text = "Add";
			this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// BtnRemove
			// 
			this.BtnRemove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnRemove.Location = new System.Drawing.Point(552, 352);
			this.BtnRemove.Name = "BtnRemove";
			this.BtnRemove.Size = new System.Drawing.Size(128, 23);
			this.BtnRemove.TabIndex = 22;
			this.BtnRemove.Text = "Rimuovi selezionato";
			this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 208);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(264, 16);
			this.label2.TabIndex = 24;
			this.label2.Text = "Lista dei file che hai selezionato:";
			// 
			// txtlogin
			// 
			this.txtlogin.Location = new System.Drawing.Point(16, 35);
			this.txtlogin.Name = "txtlogin";
			this.txtlogin.Size = new System.Drawing.Size(200, 21);
			this.txtlogin.TabIndex = 25;
			// 
			// txtpsw
			// 
			this.txtpsw.Location = new System.Drawing.Point(256, 35);
			this.txtpsw.Name = "txtpsw";
			this.txtpsw.PasswordChar = '*';
			this.txtpsw.Size = new System.Drawing.Size(200, 21);
			this.txtpsw.TabIndex = 26;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 19);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 23);
			this.label3.TabIndex = 27;
			this.label3.Text = "Login:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(256, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 23);
			this.label4.TabIndex = 28;
			this.label4.Text = "Password:";
			// 
			// TxtForceProducer
			// 
			this.TxtForceProducer.Location = new System.Drawing.Point(704, 35);
			this.TxtForceProducer.Name = "TxtForceProducer";
			this.TxtForceProducer.Size = new System.Drawing.Size(32, 21);
			this.TxtForceProducer.TabIndex = 29;
			this.TxtForceProducer.Visible = false;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(704, 19);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 16);
			this.label5.TabIndex = 30;
			this.label5.Text = "Forza produttore diverso da login:";
			this.label5.Visible = false;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 160);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(336, 23);
			this.label6.TabIndex = 31;
			this.label6.Text = "Seleziona file che vuoi criptare o decriptare:";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 392);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(100, 16);
			this.label7.TabIndex = 32;
			this.label7.Text = "Contenuto:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(240, 16);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(100, 23);
			this.label8.TabIndex = 33;
			this.label8.Text = "Porta:";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 16);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 23);
			this.label9.TabIndex = 34;
			this.label9.Text = "Server:";
			// 
			// TxtPort
			// 
			this.TxtPort.Location = new System.Drawing.Point(240, 32);
			this.TxtPort.Name = "TxtPort";
			this.TxtPort.Size = new System.Drawing.Size(100, 21);
			this.TxtPort.TabIndex = 35;
			// 
			// TxtServer
			// 
			this.TxtServer.Location = new System.Drawing.Point(16, 32);
			this.TxtServer.Name = "TxtServer";
			this.TxtServer.Size = new System.Drawing.Size(216, 21);
			this.TxtServer.TabIndex = 36;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.TxtServer);
			this.groupBox2.Controls.Add(this.TxtPort);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(24, 80);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(352, 64);
			this.groupBox2.TabIndex = 37;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Firewall";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.textBox1);
			this.groupBox3.Controls.Add(this.TxtForceProducer);
			this.groupBox3.Controls.Add(this.txtpsw);
			this.groupBox3.Controls.Add(this.txtlogin);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(24, 8);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(792, 64);
			this.groupBox3.TabIndex = 38;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Credenziali del sito";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(488, 35);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(200, 21);
			this.textBox1.TabIndex = 29;
			this.textBox1.Text = "MagoNet-Pro";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(488, 19);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(144, 23);
			this.label10.TabIndex = 30;
			this.label10.Text = "Nome solution:";
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(696, 352);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(112, 24);
			this.button1.TabIndex = 18;
			this.button1.Text = "Rimuovi tutti";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(24, 584);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(784, 96);
			this.label1.TabIndex = 0;
			this.label1.Text = "";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(24, 568);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(232, 16);
			this.label11.TabIndex = 32;
			this.label11.Text = "Messaggi (se zero vuol dire OK!):";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.BackColor = System.Drawing.Color.Lavender;
			this.ClientSize = new System.Drawing.Size(818, 696);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.TxtFile);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.ListFile);
			this.Controls.Add(this.BtnSimpleCrypt);
			this.Controls.Add(this.BtnBrowse);
			this.Controls.Add(this.BtnDecrypt);
			this.Controls.Add(this.BtnClear);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.groupBox2);
			this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.Text = "Crypting";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

	}
}
