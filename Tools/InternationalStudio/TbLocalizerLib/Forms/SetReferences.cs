using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Mostra la form per settare le references dei progetti c#.
	/// </summary>
	//=========================================================================
	public class SetReferences : System.Windows.Forms.Form
	{
		private Container	components	= null;
		private Label		LblTitle;
		private ListBox		LbAll;
		private Button		BtnOk;
		private Button		BtnCancel;

		internal string		project			= "";
		private string		none			= "<none>";
		private string		parentProject	= "";

		/// <summary>
		/// Form per settare le references dei progetti c#
		/// </summary>
		/// <param name="listPrj">lista dei nomi dei progetti della solution</param>
		/// <param name="reference">nome della reference che si vuole settare</param>
		/// <param name="project">nome del progetto che fosse gi� eventualmente settato</param>
		//---------------------------------------------------------------------
		public SetReferences(ArrayList listPrj, string reference, string project, string parent)
		{
			InitializeComponent();
			PostInitialize(listPrj, reference, project, parent);
		}

		/// <summary>
		/// Post inizializzazione, seleziona l'eventuale progetto gi� settato
		/// </summary>
		/// <param name="listPrj">lista dei nomi dei progetti della solution</param>
		/// <param name="reference">nome della reference che si vuole settare</param>
		/// <param name="project">nome del progetto che fosse gi� eventualmente settato</param>
		//---------------------------------------------------------------------
		public void PostInitialize(ArrayList listPrj, string reference, string project, string parent)
		{
			listPrj.Sort();
			listPrj.Insert(0,  none);
			LbAll.DataSource	 = listPrj;
			LbAll.SelectedItem = project;
			parentProject		 = parent;
			LblTitle.Text		 = String.Format(LblTitle.Text, reference);
		}

		/// <summary>
		/// ok
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			OkAndClose();
		}

		/// <summary>
		/// ok con double click
		/// </summary>
		//---------------------------------------------------------------------
		private void LbAll_DoubleClick(object sender, System.EventArgs e)
		{
			OkAndClose();
		}
		
		/// <summary>
		/// Chiudo e comunico il progetto selezionato
		/// </summary>
		//---------------------------------------------------------------------
		private void OkAndClose()
		{
			//TODO (da implementare)controllo che non sia gi� assegnato
			string selected	= LbAll.SelectedItem.ToString();
			bool isTheSame	= (String.Compare(selected, parentProject, true) == 0);
			if (isTheSame) 
			{
				MessageBox.Show(this, Strings.CircularReference, Strings.WarningCaption);
				return;
			}
			
			DialogResult = DialogResult.OK;
			project		= (selected == none) ? "" : selected;
			Close();
		}

		/// <summary>
		/// ok
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
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
				System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetReferences));
				this.LbAll = new System.Windows.Forms.ListBox();
				this.LblTitle = new System.Windows.Forms.Label();
				this.BtnOk = new System.Windows.Forms.Button();
				this.BtnCancel = new System.Windows.Forms.Button();
				this.SuspendLayout();
				// 
				// LbAll
				// 
				this.LbAll.AccessibleDescription = resources.GetString("LbAll.AccessibleDescription");
				this.LbAll.AccessibleName = resources.GetString("LbAll.AccessibleName");
				this.LbAll.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LbAll.Anchor")));
				this.LbAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LbAll.BackgroundImage")));
				this.LbAll.ColumnWidth = ((int)(resources.GetObject("LbAll.ColumnWidth")));
				this.LbAll.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LbAll.Dock")));
				this.LbAll.Enabled = ((bool)(resources.GetObject("LbAll.Enabled")));
				this.LbAll.Font = ((System.Drawing.Font)(resources.GetObject("LbAll.Font")));
				this.LbAll.HorizontalExtent = ((int)(resources.GetObject("LbAll.HorizontalExtent")));
				this.LbAll.HorizontalScrollbar = ((bool)(resources.GetObject("LbAll.HorizontalScrollbar")));
				this.LbAll.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LbAll.ImeMode")));
				this.LbAll.IntegralHeight = ((bool)(resources.GetObject("LbAll.IntegralHeight")));
				this.LbAll.ItemHeight = ((int)(resources.GetObject("LbAll.ItemHeight")));
				this.LbAll.Location = ((System.Drawing.Point)(resources.GetObject("LbAll.Location")));
				this.LbAll.Name = "LbAll";
				this.LbAll.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LbAll.RightToLeft")));
				this.LbAll.ScrollAlwaysVisible = ((bool)(resources.GetObject("LbAll.ScrollAlwaysVisible")));
				this.LbAll.Size = ((System.Drawing.Size)(resources.GetObject("LbAll.Size")));
				this.LbAll.TabIndex = ((int)(resources.GetObject("LbAll.TabIndex")));
				this.LbAll.Visible = ((bool)(resources.GetObject("LbAll.Visible")));
				this.LbAll.DoubleClick += new System.EventHandler(this.LbAll_DoubleClick);
				// 
				// LblTitle
				// 
				this.LblTitle.AccessibleDescription = resources.GetString("LblTitle.AccessibleDescription");
				this.LblTitle.AccessibleName = resources.GetString("LblTitle.AccessibleName");
				this.LblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitle.Anchor")));
				this.LblTitle.AutoSize = ((bool)(resources.GetObject("LblTitle.AutoSize")));
				this.LblTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitle.Dock")));
				this.LblTitle.Enabled = ((bool)(resources.GetObject("LblTitle.Enabled")));
				this.LblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
				this.LblTitle.Font = ((System.Drawing.Font)(resources.GetObject("LblTitle.Font")));
				this.LblTitle.Image = ((System.Drawing.Image)(resources.GetObject("LblTitle.Image")));
				this.LblTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.ImageAlign")));
				this.LblTitle.ImageIndex = ((int)(resources.GetObject("LblTitle.ImageIndex")));
				this.LblTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTitle.ImeMode")));
				this.LblTitle.Location = ((System.Drawing.Point)(resources.GetObject("LblTitle.Location")));
				this.LblTitle.Name = "LblTitle";
				this.LblTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTitle.RightToLeft")));
				this.LblTitle.Size = ((System.Drawing.Size)(resources.GetObject("LblTitle.Size")));
				this.LblTitle.TabIndex = ((int)(resources.GetObject("LblTitle.TabIndex")));
				this.LblTitle.Text = resources.GetString("LblTitle.Text");
				this.LblTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.TextAlign")));
				this.LblTitle.Visible = ((bool)(resources.GetObject("LblTitle.Visible")));
				// 
				// BtnOk
				// 
				this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
				this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
				this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
				this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
				this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
				this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
				this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
				this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
				this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
				this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
				this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
				this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
				this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
				this.BtnOk.Name = "BtnOk";
				this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
				this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
				this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
				this.BtnOk.Text = resources.GetString("BtnOk.Text");
				this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
				this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
				this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
				// 
				// BtnCancel
				// 
				this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
				this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
				this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
				this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
				this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
				this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
				this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
				this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
				this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
				this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
				this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
				this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
				this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
				this.BtnCancel.Name = "BtnCancel";
				this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
				this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
				this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
				this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
				this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
				this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
				this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
				// 
				// SetReferences
				// 
				this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
				this.AccessibleName = resources.GetString("$this.AccessibleName");
				this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
				this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
				this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
				this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
				this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
				this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
				this.Controls.Add(this.LblTitle);
				this.Controls.Add(this.BtnCancel);
				this.Controls.Add(this.BtnOk);
				this.Controls.Add(this.LbAll);
				this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
				this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
				this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
				this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
				this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
				this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
				this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
				this.Name = "SetReferences";
				this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
				this.ShowInTaskbar = false;
				this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
				this.Text = resources.GetString("$this.Text");
				this.ResumeLayout(false);

			}
		#endregion

		
		}
}