
namespace Microarea.Console.Core.FileConverter
{
	/// <summary>
	/// Summary description for AskMissingObject.
	/// </summary>
	//================================================================================
	public class AskMissingObject : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnOk;
		public System.Windows.Forms.TextBox txtOld;
		public System.Windows.Forms.TextBox txtNew;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox txtCurrentLine;
		public System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------
		public AskMissingObject()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AskMissingObject));
			this.btnOk = new System.Windows.Forms.Button();
			this.txtOld = new System.Windows.Forms.TextBox();
			this.txtNew = new System.Windows.Forms.TextBox();
			this.txtCurrentLine = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblMessage = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			// 
			// txtOld
			// 
			resources.ApplyResources(this.txtOld, "txtOld");
			this.txtOld.Name = "txtOld";
			this.txtOld.ReadOnly = true;
			// 
			// txtNew
			// 
			resources.ApplyResources(this.txtNew, "txtNew");
			this.txtNew.Name = "txtNew";
			this.txtNew.TextChanged += new System.EventHandler(this.txtNew_TextChanged);
			// 
			// txtCurrentLine
			// 
			resources.ApplyResources(this.txtCurrentLine, "txtCurrentLine");
			this.txtCurrentLine.Name = "txtCurrentLine";
			this.txtCurrentLine.ReadOnly = true;
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// lblMessage
			// 
			resources.ApplyResources(this.lblMessage, "lblMessage");
			this.lblMessage.Name = "lblMessage";
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.button1, "button1");
			this.button1.Name = "button1";
			// 
			// AskMissingObject
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtOld);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtNew);
			this.Controls.Add(this.txtCurrentLine);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.button1);
			this.Name = "AskMissingObject";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void txtNew_TextChanged(object sender, System.EventArgs e)
		{
			btnOk.Enabled = txtNew.Text != string.Empty;
		}
	}
}
