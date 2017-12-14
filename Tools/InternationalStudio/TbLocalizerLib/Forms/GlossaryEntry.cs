using System;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Dialog to insert or modify entries in glossary.
	/// </summary>
	public class GlossaryEntry : System.Windows.Forms.Form
	{
		#region Controls
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Label LblTarget;
		private System.Windows.Forms.Label LblBase;
		private System.Windows.Forms.TextBox TxtBase;
		private System.Windows.Forms.TextBox TxtTarget;
		private System.Windows.Forms.Label LblMessages;
		private System.Windows.Forms.Label LblSupport;
		private System.Windows.Forms.TextBox TxtSupport;
		private System.ComponentModel.Container components = null;
		#endregion

		public string Base		= null;
		public string Target	= null;
		public string Support	= null;

		//---------------------------------------------------------------------
		public GlossaryEntry(bool usesupport): this("", "", "", usesupport){}
		public GlossaryEntry(string aBase, string aTarget, string aSupport, bool usesupport)
		{
			InitializeComponent();
			TxtBase.Text	= aBase;
			TxtTarget.Text	= aTarget;
			TxtSupport.Text	= aSupport;
			TxtSupport.Visible = LblSupport.Visible = usesupport;
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			if (TxtBase.Text == String.Empty || 
				TxtTarget.Text == String.Empty)
			{
				LblMessages.Visible = true;
				return;
			}
			Base	= TxtBase.Text;
			Target	= TxtTarget.Text;
			Support = TxtSupport.Text;
			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		//---------------------------------------------------------------------
		private void Txt_TextChanged(object sender, System.EventArgs e)
		{
			LblMessages.Visible = false;
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
			this.TxtBase = new System.Windows.Forms.TextBox();
			this.TxtTarget = new System.Windows.Forms.TextBox();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.LblTarget = new System.Windows.Forms.Label();
			this.LblBase = new System.Windows.Forms.Label();
			this.LblMessages = new System.Windows.Forms.Label();
			this.LblSupport = new System.Windows.Forms.Label();
			this.TxtSupport = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// TxtBase
			// 
			this.TxtBase.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.TxtBase.Location = new System.Drawing.Point(112, 16);
			this.TxtBase.Name = "TxtBase";
			this.TxtBase.Size = new System.Drawing.Size(288, 23);
			this.TxtBase.TabIndex = 0;
			this.TxtBase.Text = "";
			this.TxtBase.TextChanged += new System.EventHandler(this.Txt_TextChanged);
			// 
			// TxtTarget
			// 
			this.TxtTarget.Location = new System.Drawing.Point(112, 72);
			this.TxtTarget.Name = "TxtTarget";
			this.TxtTarget.Size = new System.Drawing.Size(288, 23);
			this.TxtTarget.TabIndex = 2;
			this.TxtTarget.Text = "";
			this.TxtTarget.TextChanged += new System.EventHandler(this.Txt_TextChanged);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnCancel.Location = new System.Drawing.Point(328, 112);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.TabIndex = 4;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnOk
			// 
			this.BtnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnOk.Location = new System.Drawing.Point(232, 112);
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.TabIndex = 3;
			this.BtnOk.Text = "OK";
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// LblTarget
			// 
			this.LblTarget.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblTarget.Location = new System.Drawing.Point(8, 72);
			this.LblTarget.Name = "LblTarget";
			this.LblTarget.TabIndex = 4;
			this.LblTarget.Text = "Target string:";
			// 
			// LblBase
			// 
			this.LblBase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblBase.Location = new System.Drawing.Point(8, 16);
			this.LblBase.Name = "LblBase";
			this.LblBase.TabIndex = 5;
			this.LblBase.Text = "Base string:";
			// 
			// LblMessages
			// 
			this.LblMessages.ForeColor = System.Drawing.Color.Red;
			this.LblMessages.Location = new System.Drawing.Point(8, 112);
			this.LblMessages.Name = "LblMessages";
			this.LblMessages.Size = new System.Drawing.Size(160, 23);
			this.LblMessages.TabIndex = 6;
			this.LblMessages.Text = "Invalid entries!";
			this.LblMessages.Visible = false;
			// 
			// LblSupport
			// 
			this.LblSupport.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblSupport.Location = new System.Drawing.Point(8, 44);
			this.LblSupport.Name = "LblSupport";
			this.LblSupport.TabIndex = 8;
			this.LblSupport.Text = "Support string:";
			// 
			// TxtSupport
			// 
			this.TxtSupport.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.TxtSupport.Location = new System.Drawing.Point(112, 44);
			this.TxtSupport.Name = "TxtSupport";
			this.TxtSupport.Size = new System.Drawing.Size(288, 23);
			this.TxtSupport.TabIndex = 1;
			this.TxtSupport.Text = "";
			// 
			// GlossaryEntry
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
			this.ClientSize = new System.Drawing.Size(410, 144);
			this.Controls.Add(this.LblSupport);
			this.Controls.Add(this.TxtSupport);
			this.Controls.Add(this.LblMessages);
			this.Controls.Add(this.LblBase);
			this.Controls.Add(this.LblTarget);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.TxtTarget);
			this.Controls.Add(this.TxtBase);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "GlossaryEntry";
			this.Text = "GlossaryEntry";
			this.ResumeLayout(false);

		}
		#endregion

	}
}
