namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class SetEntityPrioritiesPage
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetEntityPrioritiesPage));
			this.EntitiesListView = new System.Windows.Forms.ListView();
			this.EntityColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.PriorityColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BtnMoveUp = new System.Windows.Forms.Button();
			this.BtnMoveDown = new System.Windows.Forms.Button();
			this.PrioritiesToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// m_headerPicture
			// 
			this.m_headerPicture.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecretSmall;
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// EntitiesListView
			// 
			this.EntitiesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.EntityColumnHeader,
            this.PriorityColumnHeader});
			this.EntitiesListView.FullRowSelect = true;
			this.EntitiesListView.GridLines = true;
			this.EntitiesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			resources.ApplyResources(this.EntitiesListView, "EntitiesListView");
			this.EntitiesListView.MultiSelect = false;
			this.EntitiesListView.Name = "EntitiesListView";
			this.EntitiesListView.UseCompatibleStateImageBehavior = false;
			this.EntitiesListView.View = System.Windows.Forms.View.Details;
			this.EntitiesListView.SelectedIndexChanged += new System.EventHandler(this.EntitiesListView_SelectedIndexChanged);
			// 
			// EntityColumnHeader
			// 
			resources.ApplyResources(this.EntityColumnHeader, "EntityColumnHeader");
			// 
			// PriorityColumnHeader
			// 
			resources.ApplyResources(this.PriorityColumnHeader, "PriorityColumnHeader");
			// 
			// BtnMoveUp
			// 
			this.BtnMoveUp.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.MoveUp;
			resources.ApplyResources(this.BtnMoveUp, "BtnMoveUp");
			this.BtnMoveUp.Name = "BtnMoveUp";
			this.BtnMoveUp.UseVisualStyleBackColor = true;
			this.BtnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);
			this.BtnMoveUp.MouseHover += new System.EventHandler(this.BtnMoveUp_MouseHover);
			// 
			// BtnMoveDown
			// 
			this.BtnMoveDown.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.MoveDown;
			resources.ApplyResources(this.BtnMoveDown, "BtnMoveDown");
			this.BtnMoveDown.Name = "BtnMoveDown";
			this.BtnMoveDown.UseVisualStyleBackColor = true;
			this.BtnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);
			this.BtnMoveDown.MouseHover += new System.EventHandler(this.BtnMoveDown_MouseHover);
			// 
			// PrioritiesToolTip
			// 
			this.PrioritiesToolTip.AutoPopDelay = 5000;
			this.PrioritiesToolTip.InitialDelay = 300;
			this.PrioritiesToolTip.ReshowDelay = 100;
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// SetEntityPrioritiesPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BtnMoveDown);
			this.Controls.Add(this.BtnMoveUp);
			this.Controls.Add(this.EntitiesListView);
			this.Name = "SetEntityPrioritiesPage";
			this.Controls.SetChildIndex(this.EntitiesListView, 0);
			this.Controls.SetChildIndex(this.BtnMoveUp, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.BtnMoveDown, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView EntitiesListView;
		private System.Windows.Forms.ColumnHeader EntityColumnHeader;
		private System.Windows.Forms.ColumnHeader PriorityColumnHeader;
		private System.Windows.Forms.Button BtnMoveUp;
		private System.Windows.Forms.Button BtnMoveDown;
		private System.Windows.Forms.ToolTip PrioritiesToolTip;
		private System.Windows.Forms.Label label1;
	}
}
