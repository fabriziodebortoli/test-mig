
namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
    partial class SelectObjectsWizardForm
    {
        private System.Windows.Forms.ListView ObjectTypesListView;
        private System.Windows.Forms.ColumnHeader ObjectTypesListViewColumn;
        private System.ComponentModel.IContainer components = null;

        
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SelectObjectsWizardForm));
			this.ObjectTypesListView = new System.Windows.Forms.ListView();
			this.ObjectTypesListViewColumn = new System.Windows.Forms.ColumnHeader();
			this.AllObjectsButton = new System.Windows.Forms.Button();
			this.m_headerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			this.m_headerPanel.Name = "m_headerPanel";
			this.m_headerPanel.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPanel.Size")));
			// 
			// m_headerPicture
			// 
			this.m_headerPicture.Image = ((System.Drawing.Image)(resources.GetObject("m_headerPicture.Image")));
			this.m_headerPicture.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPicture.Location")));
			this.m_headerPicture.Name = "m_headerPicture";
			this.m_headerPicture.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPicture.Size")));
			this.m_headerPicture.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("m_headerPicture.SizeMode")));
			// 
			// m_titleLabel
			// 
			this.m_titleLabel.Font = ((System.Drawing.Font)(resources.GetObject("m_titleLabel.Font")));
			this.m_titleLabel.Location = ((System.Drawing.Point)(resources.GetObject("m_titleLabel.Location")));
			this.m_titleLabel.Name = "m_titleLabel";
			this.m_titleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_titleLabel.Size")));
			// 
			// m_subtitleLabel
			// 
			this.m_subtitleLabel.Location = ((System.Drawing.Point)(resources.GetObject("m_subtitleLabel.Location")));
			this.m_subtitleLabel.Name = "m_subtitleLabel";
			this.m_subtitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_subtitleLabel.Size")));
			// 
			// ObjectTypesListView
			// 
			this.ObjectTypesListView.AccessibleDescription = resources.GetString("ObjectTypesListView.AccessibleDescription");
			this.ObjectTypesListView.AccessibleName = resources.GetString("ObjectTypesListView.AccessibleName");
			this.ObjectTypesListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("ObjectTypesListView.Alignment")));
			this.ObjectTypesListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ObjectTypesListView.Anchor")));
			this.ObjectTypesListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ObjectTypesListView.BackgroundImage")));
			this.ObjectTypesListView.CausesValidation = false;
			this.ObjectTypesListView.CheckBoxes = true;
			this.ObjectTypesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								  this.ObjectTypesListViewColumn});
			this.ObjectTypesListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ObjectTypesListView.Dock")));
			this.ObjectTypesListView.Enabled = ((bool)(resources.GetObject("ObjectTypesListView.Enabled")));
			this.ObjectTypesListView.Font = ((System.Drawing.Font)(resources.GetObject("ObjectTypesListView.Font")));
			this.ObjectTypesListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ObjectTypesListView.ImeMode")));
			this.ObjectTypesListView.LabelWrap = ((bool)(resources.GetObject("ObjectTypesListView.LabelWrap")));
			this.ObjectTypesListView.Location = ((System.Drawing.Point)(resources.GetObject("ObjectTypesListView.Location")));
			this.ObjectTypesListView.Name = "ObjectTypesListView";
			this.ObjectTypesListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ObjectTypesListView.RightToLeft")));
			this.ObjectTypesListView.Size = ((System.Drawing.Size)(resources.GetObject("ObjectTypesListView.Size")));
			this.ObjectTypesListView.TabIndex = ((int)(resources.GetObject("ObjectTypesListView.TabIndex")));
			this.ObjectTypesListView.Text = resources.GetString("ObjectTypesListView.Text");
			this.ObjectTypesListView.View = System.Windows.Forms.View.Details;
			this.ObjectTypesListView.Visible = ((bool)(resources.GetObject("ObjectTypesListView.Visible")));
			this.ObjectTypesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ObjectTypesListView_ItemCheck);
			// 
			// ObjectTypesListViewColumn
			// 
			this.ObjectTypesListViewColumn.Text = resources.GetString("ObjectTypesListViewColumn.Text");
			this.ObjectTypesListViewColumn.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ObjectTypesListViewColumn.TextAlign")));
			this.ObjectTypesListViewColumn.Width = ((int)(resources.GetObject("ObjectTypesListViewColumn.Width")));
			// 
			// AllObjectsButton
			// 
			this.AllObjectsButton.AccessibleDescription = resources.GetString("AllObjectsButton.AccessibleDescription");
			this.AllObjectsButton.AccessibleName = resources.GetString("AllObjectsButton.AccessibleName");
			this.AllObjectsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AllObjectsButton.Anchor")));
			this.AllObjectsButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AllObjectsButton.BackgroundImage")));
			this.AllObjectsButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AllObjectsButton.Dock")));
			this.AllObjectsButton.Enabled = ((bool)(resources.GetObject("AllObjectsButton.Enabled")));
			this.AllObjectsButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("AllObjectsButton.FlatStyle")));
			this.AllObjectsButton.Font = ((System.Drawing.Font)(resources.GetObject("AllObjectsButton.Font")));
			this.AllObjectsButton.Image = ((System.Drawing.Image)(resources.GetObject("AllObjectsButton.Image")));
			this.AllObjectsButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllObjectsButton.ImageAlign")));
			this.AllObjectsButton.ImageIndex = ((int)(resources.GetObject("AllObjectsButton.ImageIndex")));
			this.AllObjectsButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AllObjectsButton.ImeMode")));
			this.AllObjectsButton.Location = ((System.Drawing.Point)(resources.GetObject("AllObjectsButton.Location")));
			this.AllObjectsButton.Name = "AllObjectsButton";
			this.AllObjectsButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AllObjectsButton.RightToLeft")));
			this.AllObjectsButton.Size = ((System.Drawing.Size)(resources.GetObject("AllObjectsButton.Size")));
			this.AllObjectsButton.TabIndex = ((int)(resources.GetObject("AllObjectsButton.TabIndex")));
			this.AllObjectsButton.Text = resources.GetString("AllObjectsButton.Text");
			this.AllObjectsButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllObjectsButton.TextAlign")));
			this.AllObjectsButton.Visible = ((bool)(resources.GetObject("AllObjectsButton.Visible")));
			this.AllObjectsButton.Click += new System.EventHandler(this.AllObjectsButton_Click);
			// 
			// SelectObjectsWizardForm
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.AllObjectsButton);
			this.Controls.Add(this.ObjectTypesListView);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "SelectObjectsWizardForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Load += new System.EventHandler(this.SelectObjectsWizardForm_Load);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.ObjectTypesListView, 0);
			this.Controls.SetChildIndex(this.AllObjectsButton, 0);
			this.m_headerPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    }
}
