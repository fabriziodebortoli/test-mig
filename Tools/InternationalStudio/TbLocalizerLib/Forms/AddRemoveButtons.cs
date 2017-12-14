using System;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for AddRemoveButtonscs.
	/// </summary>
	//================================================================================
	public class AddRemoveButtons : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnRemove;
		public event EventHandler AddClicked;
		public event EventHandler RemoveClicked;

		private ListBox linkedBox;
		public ListBox LinkedBox { get { return linkedBox; } set { linkedBox = value; }}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------
		public AddRemoveButtons()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnRemove = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAdd.Location = new System.Drawing.Point(2, 0);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(40, 23);
			this.btnAdd.TabIndex = 0;
			this.btnAdd.Text = "Add";
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRemove.Location = new System.Drawing.Point(50, 0);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(56, 23);
			this.btnRemove.TabIndex = 1;
			this.btnRemove.Text = "Remove";
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// AddRemoveButtons
			// 
			this.AllowDrop = true;
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.btnRemove);
			this.Name = "AddRemoveButtons";
			this.Size = new System.Drawing.Size(105, 23);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void btnAdd_Click(object sender, System.EventArgs e)
		{
			if (AddClicked != null)
				AddClicked(this, e);
		}

		//--------------------------------------------------------------------------------
		private void btnRemove_Click(object sender, System.EventArgs e)
		{
			if (RemoveClicked != null)
				RemoveClicked(this, e);
		}
	}
}
