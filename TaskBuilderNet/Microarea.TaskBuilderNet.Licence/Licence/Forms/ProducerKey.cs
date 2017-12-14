using System.Collections;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	/// <summary>
	/// Summary description for ProducerKey.
	/// </summary>
	//---------------------------------------------------------------------
	public class ProducerKey : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label LblPK;
		private System.Windows.Forms.TextBox TxtPK;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Label LblMsg;
		public IList PKs = null;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ListBox LbPK;
		private System.Windows.Forms.Button BtnAdd;
		private System.Windows.Forms.Button BtnRemove;
		private System.Windows.Forms.Label LblMsg2;
		private System.Windows.Forms.Button BtnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//---------------------------------------------------------------------
		public ProducerKey(IList pks)
		{
			InitializeComponent();
			PKs = pks;
			LbPK.Items.Clear();
			if (pks == null) 
				pks = new ArrayList();
			else
				foreach (string s in PKs)
					if (s != null && s.Trim().Length > 0)
						LbPK.Items.Add(s);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProducerKey));
			this.LblPK = new System.Windows.Forms.Label();
			this.TxtPK = new System.Windows.Forms.TextBox();
			this.BtnOk = new System.Windows.Forms.Button();
			this.LblMsg = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.LbPK = new System.Windows.Forms.ListBox();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.LblMsg2 = new System.Windows.Forms.Label();
			this.BtnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// LblPK
			// 
			resources.ApplyResources(this.LblPK, "LblPK");
			this.LblPK.Name = "LblPK";
			// 
			// TxtPK
			// 
			resources.ApplyResources(this.TxtPK, "TxtPK");
			this.TxtPK.Name = "TxtPK";
			this.TxtPK.TextChanged += new System.EventHandler(this.TxtPK_TextChanged);
			// 
			// BtnOk
			// 
			resources.ApplyResources(this.BtnOk, "BtnOk");
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// LblMsg
			// 
			this.LblMsg.ForeColor = System.Drawing.Color.Red;
			resources.ApplyResources(this.LblMsg, "LblMsg");
			this.LblMsg.Name = "LblMsg";
			// 
			// pictureBox1
			// 
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			// 
			// LbPK
			// 
			resources.ApplyResources(this.LbPK, "LbPK");
			this.LbPK.Name = "LbPK";
			// 
			// BtnAdd
			// 
			resources.ApplyResources(this.BtnAdd, "BtnAdd");
			this.BtnAdd.Name = "BtnAdd";
			this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// BtnRemove
			// 
			resources.ApplyResources(this.BtnRemove, "BtnRemove");
			this.BtnRemove.Name = "BtnRemove";
			this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
			// 
			// LblMsg2
			// 
			this.LblMsg2.ForeColor = System.Drawing.Color.Red;
			resources.ApplyResources(this.LblMsg2, "LblMsg2");
			this.LblMsg2.Name = "LblMsg2";
			// 
			// BtnCancel
			// 
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// ProducerKey
			// 
			resources.ApplyResources(this, "$this");
			this.BackColor = System.Drawing.Color.Lavender;
			this.ControlBox = false;
			this.Controls.Add(this.LblMsg2);
			this.Controls.Add(this.LbPK);
			this.Controls.Add(this.TxtPK);
			this.Controls.Add(this.LblPK);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.LblMsg);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.BtnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProducerKey";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			if (PKs == null)
				PKs = new ArrayList();
			PKs.Clear();
			if (LbPK.Items != null)
				foreach (object o in LbPK.Items)
					PKs.Add(o.ToString());	
			this.DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private bool IsValidPK()
		{
			return TxtPK.Text.Trim().Length > 0;
		}		

		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			if (LbPK.Items != null && LbPK.Items.Count > 0 && LbPK.SelectedIndex > -1)
			{
				TxtPK.Text = LbPK.Items[LbPK.SelectedIndex] as string;
				LbPK.Items.RemoveAt(LbPK.SelectedIndex);
			}
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			if (!IsValidPK())
			{
				LblMsg.Visible = true;
				return;
			}
			string val = TxtPK.Text.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			if (!LbPK.Items.Contains(val))
			{
				TxtPK.Text = string.Empty;
				LbPK.Items.Add(val);
				LblMsg.Visible = false;
				LblMsg2.Visible = false;
			}
			else
				LblMsg2.Visible = true;
		}

		//---------------------------------------------------------------------
		private void TxtPK_TextChanged(object sender, System.EventArgs e)
		{
			LblMsg.Visible =!IsValidPK();
			LblMsg2.Visible = false;
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
