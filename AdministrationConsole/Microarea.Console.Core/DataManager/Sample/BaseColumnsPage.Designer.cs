
namespace Microarea.Console.Core.DataManager.Sample
{
    partial class BaseColumnsPage
    {
        private System.Windows.Forms.GroupBox ParamsGroupBox;
        private System.Windows.Forms.CheckBox TBCreatedCheckBox;
        private System.Windows.Forms.CheckBox TBModifiedCheckBox;
        private System.Windows.Forms.GroupBox DateTimeGroupBox;
        private System.Windows.Forms.CheckBox UseUtcFormatCheckBox;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseColumnsPage));
			this.ParamsGroupBox = new System.Windows.Forms.GroupBox();
			this.TBModifiedCheckBox = new System.Windows.Forms.CheckBox();
			this.TBCreatedCheckBox = new System.Windows.Forms.CheckBox();
			this.DateTimeGroupBox = new System.Windows.Forms.GroupBox();
			this.UseUtcFormatCheckBox = new System.Windows.Forms.CheckBox();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.ParamsGroupBox.SuspendLayout();
			this.DateTimeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
			// 
			// m_headerPicture
			// 
			resources.ApplyResources(this.m_headerPicture, "m_headerPicture");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// ParamsGroupBox
			// 
			this.ParamsGroupBox.Controls.Add(this.TBModifiedCheckBox);
			this.ParamsGroupBox.Controls.Add(this.TBCreatedCheckBox);
			this.ParamsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ParamsGroupBox, "ParamsGroupBox");
			this.ParamsGroupBox.Name = "ParamsGroupBox";
			this.ParamsGroupBox.TabStop = false;
			// 
			// TBModifiedCheckBox
			// 
			resources.ApplyResources(this.TBModifiedCheckBox, "TBModifiedCheckBox");
			this.TBModifiedCheckBox.Name = "TBModifiedCheckBox";
			// 
			// TBCreatedCheckBox
			// 
			resources.ApplyResources(this.TBCreatedCheckBox, "TBCreatedCheckBox");
			this.TBCreatedCheckBox.Name = "TBCreatedCheckBox";
			// 
			// DateTimeGroupBox
			// 
			this.DateTimeGroupBox.Controls.Add(this.UseUtcFormatCheckBox);
			this.DateTimeGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.DateTimeGroupBox, "DateTimeGroupBox");
			this.DateTimeGroupBox.Name = "DateTimeGroupBox";
			this.DateTimeGroupBox.TabStop = false;
			// 
			// UseUtcFormatCheckBox
			// 
			resources.ApplyResources(this.UseUtcFormatCheckBox, "UseUtcFormatCheckBox");
			this.UseUtcFormatCheckBox.Name = "UseUtcFormatCheckBox";
			// 
			// BaseColumnsPage
			// 
			this.Controls.Add(this.DateTimeGroupBox);
			this.Controls.Add(this.ParamsGroupBox);
			this.Name = "BaseColumnsPage";
			resources.ApplyResources(this, "$this");
			this.Controls.SetChildIndex(this.ParamsGroupBox, 0);
			this.Controls.SetChildIndex(this.DateTimeGroupBox, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.ParamsGroupBox.ResumeLayout(false);
			this.DateTimeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion
    }
}
