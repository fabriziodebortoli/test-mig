
namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class SetGrants
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button ShowRolesButton;
        private System.Windows.Forms.Panel GroupLegend;
        private System.Windows.Forms.Label NothingLabel;
        private System.Windows.Forms.Label NotDefined;
        private System.Windows.Forms.Label InheritLabe;
        private System.Windows.Forms.Label ForbiddenLabel;
        private System.Windows.Forms.Label AllowedLabel;
        private System.Windows.Forms.PictureBox pctNothing;
        private System.Windows.Forms.PictureBox pctNa;
        private System.Windows.Forms.PictureBox pctInherit;
        private System.Windows.Forms.PictureBox pctForbidden;
        private System.Windows.Forms.PictureBox pctAllowed;
        private System.Windows.Forms.TextBox NamespaceLabel;
        private System.Windows.Forms.Button AllowAllButton;
        private System.Windows.Forms.Button DenyAllButton;
        private System.Windows.Forms.Label UserLabel;
        private System.Windows.Forms.ComboBox UsersComboBox;
        private System.Windows.Forms.Label RolesLabel;
        private System.Windows.Forms.ComboBox RolesComboBox;
        private System.Windows.Forms.ToolTip toolTipSetGrants;
        private System.Windows.Forms.PictureBox LockPictureBox;

        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        //---------------------------------------------------------------------
        #region Windows Form Designer generated code

        //---------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetGrants));
			this.LockPictureBox = new System.Windows.Forms.PictureBox();
			this.ShowRolesButton = new System.Windows.Forms.Button();
			this.GroupLegend = new System.Windows.Forms.Panel();
			this.NothingLabel = new System.Windows.Forms.Label();
			this.NotDefined = new System.Windows.Forms.Label();
			this.InheritLabe = new System.Windows.Forms.Label();
			this.ForbiddenLabel = new System.Windows.Forms.Label();
			this.AllowedLabel = new System.Windows.Forms.Label();
			this.pctNothing = new System.Windows.Forms.PictureBox();
			this.pctNa = new System.Windows.Forms.PictureBox();
			this.pctInherit = new System.Windows.Forms.PictureBox();
			this.pctForbidden = new System.Windows.Forms.PictureBox();
			this.pctAllowed = new System.Windows.Forms.PictureBox();
			this.NamespaceLabel = new System.Windows.Forms.TextBox();
			this.AllowAllButton = new System.Windows.Forms.Button();
			this.DenyAllButton = new System.Windows.Forms.Button();
			this.UserLabel = new System.Windows.Forms.Label();
			this.UsersComboBox = new System.Windows.Forms.ComboBox();
			this.RolesLabel = new System.Windows.Forms.Label();
			this.RolesComboBox = new System.Windows.Forms.ComboBox();
			this.toolTipSetGrants = new System.Windows.Forms.ToolTip(this.components);
			this.CurrentUserPictureWithBalloon = new Microarea.TaskBuilderNet.UI.WinControls.PictureWithBalloon();
			((System.ComponentModel.ISupportInitialize)(this.LockPictureBox)).BeginInit();
			this.GroupLegend.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pctNothing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctNa)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctInherit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctForbidden)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctAllowed)).BeginInit();
			this.SuspendLayout();
			// 
			// LockPictureBox
			// 
			this.LockPictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.LockPictureBox, "LockPictureBox");
			this.LockPictureBox.Name = "LockPictureBox";
			this.LockPictureBox.TabStop = false;
			this.toolTipSetGrants.SetToolTip(this.LockPictureBox, resources.GetString("LockPictureBox.ToolTip"));
			// 
			// ShowRolesButton
			// 
			resources.ApplyResources(this.ShowRolesButton, "ShowRolesButton");
			this.ShowRolesButton.Name = "ShowRolesButton";
			this.toolTipSetGrants.SetToolTip(this.ShowRolesButton, resources.GetString("ShowRolesButton.ToolTip"));
			this.ShowRolesButton.Click += new System.EventHandler(this.ShowRolesButton_Click);
			// 
			// GroupLegend
			// 
			this.GroupLegend.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.GroupLegend.Controls.Add(this.NothingLabel);
			this.GroupLegend.Controls.Add(this.NotDefined);
			this.GroupLegend.Controls.Add(this.InheritLabe);
			this.GroupLegend.Controls.Add(this.ForbiddenLabel);
			this.GroupLegend.Controls.Add(this.AllowedLabel);
			this.GroupLegend.Controls.Add(this.pctNothing);
			this.GroupLegend.Controls.Add(this.pctNa);
			this.GroupLegend.Controls.Add(this.pctInherit);
			this.GroupLegend.Controls.Add(this.pctForbidden);
			this.GroupLegend.Controls.Add(this.pctAllowed);
			resources.ApplyResources(this.GroupLegend, "GroupLegend");
			this.GroupLegend.Name = "GroupLegend";
			// 
			// NothingLabel
			// 
			resources.ApplyResources(this.NothingLabel, "NothingLabel");
			this.NothingLabel.Name = "NothingLabel";
			// 
			// NotDefined
			// 
			resources.ApplyResources(this.NotDefined, "NotDefined");
			this.NotDefined.Name = "NotDefined";
			// 
			// InheritLabe
			// 
			resources.ApplyResources(this.InheritLabe, "InheritLabe");
			this.InheritLabe.Name = "InheritLabe";
			// 
			// ForbiddenLabel
			// 
			resources.ApplyResources(this.ForbiddenLabel, "ForbiddenLabel");
			this.ForbiddenLabel.Name = "ForbiddenLabel";
			// 
			// AllowedLabel
			// 
			resources.ApplyResources(this.AllowedLabel, "AllowedLabel");
			this.AllowedLabel.Name = "AllowedLabel";
			// 
			// pctNothing
			// 
			this.pctNothing.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctNothing, "pctNothing");
			this.pctNothing.Name = "pctNothing";
			this.pctNothing.TabStop = false;
			// 
			// pctNa
			// 
			this.pctNa.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctNa, "pctNa");
			this.pctNa.Name = "pctNa";
			this.pctNa.TabStop = false;
			// 
			// pctInherit
			// 
			this.pctInherit.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctInherit, "pctInherit");
			this.pctInherit.Name = "pctInherit";
			this.pctInherit.TabStop = false;
			// 
			// pctForbidden
			// 
			this.pctForbidden.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctForbidden, "pctForbidden");
			this.pctForbidden.Name = "pctForbidden";
			this.pctForbidden.TabStop = false;
			// 
			// pctAllowed
			// 
			this.pctAllowed.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctAllowed, "pctAllowed");
			this.pctAllowed.Name = "pctAllowed";
			this.pctAllowed.TabStop = false;
			// 
			// NamespaceLabel
			// 
			this.NamespaceLabel.BackColor = System.Drawing.SystemColors.Control;
			this.NamespaceLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.NamespaceLabel.CausesValidation = false;
			resources.ApplyResources(this.NamespaceLabel, "NamespaceLabel");
			this.NamespaceLabel.Name = "NamespaceLabel";
			this.NamespaceLabel.ReadOnly = true;
			this.toolTipSetGrants.SetToolTip(this.NamespaceLabel, resources.GetString("NamespaceLabel.ToolTip"));
			// 
			// AllowAllButton
			// 
			resources.ApplyResources(this.AllowAllButton, "AllowAllButton");
			this.AllowAllButton.Name = "AllowAllButton";
			this.AllowAllButton.Click += new System.EventHandler(this.AllowAllButton_Click);
			// 
			// DenyAllButton
			// 
			resources.ApplyResources(this.DenyAllButton, "DenyAllButton");
			this.DenyAllButton.Name = "DenyAllButton";
			this.DenyAllButton.Click += new System.EventHandler(this.DenyAllButton_Click);
			// 
			// UserLabel
			// 
			resources.ApplyResources(this.UserLabel, "UserLabel");
			this.UserLabel.Name = "UserLabel";
			// 
			// UsersComboBox
			// 
			this.UsersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.UsersComboBox, "UsersComboBox");
			this.UsersComboBox.Name = "UsersComboBox";
			this.toolTipSetGrants.SetToolTip(this.UsersComboBox, resources.GetString("UsersComboBox.ToolTip"));
			this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
			// 
			// RolesLabel
			// 
			resources.ApplyResources(this.RolesLabel, "RolesLabel");
			this.RolesLabel.Name = "RolesLabel";
			// 
			// RolesComboBox
			// 
			this.RolesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.RolesComboBox, "RolesComboBox");
			this.RolesComboBox.Name = "RolesComboBox";
			this.toolTipSetGrants.SetToolTip(this.RolesComboBox, resources.GetString("RolesComboBox.ToolTip"));
			this.RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
			// 
			// CurrentUserPictureWithBalloon
			// 
			this.CurrentUserPictureWithBalloon.ContentManager = null;
			resources.ApplyResources(this.CurrentUserPictureWithBalloon, "CurrentUserPictureWithBalloon");
			this.CurrentUserPictureWithBalloon.Name = "CurrentUserPictureWithBalloon";
			// 
			// SetGrants
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			resources.ApplyResources(this, "$this");
			this.ControlBox = false;
			this.Controls.Add(this.CurrentUserPictureWithBalloon);
			this.Controls.Add(this.RolesComboBox);
			this.Controls.Add(this.RolesLabel);
			this.Controls.Add(this.UsersComboBox);
			this.Controls.Add(this.UserLabel);
			this.Controls.Add(this.DenyAllButton);
			this.Controls.Add(this.AllowAllButton);
			this.Controls.Add(this.NamespaceLabel);
			this.Controls.Add(this.GroupLegend);
			this.Controls.Add(this.ShowRolesButton);
			this.Controls.Add(this.LockPictureBox);
			this.Name = "SetGrants";
			this.ShowInTaskbar = false;
			this.Deactivate += new System.EventHandler(this.SetGrants_Deactivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetGrants_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SetGrants_FormClosed);
			this.Load += new System.EventHandler(this.SetGrants_Load);
			this.Leave += new System.EventHandler(this.SetGrants_Leave);
			this.ParentChanged += new System.EventHandler(this.SetGrants_ParentChanged);
			((System.ComponentModel.ISupportInitialize)(this.LockPictureBox)).EndInit();
			this.GroupLegend.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pctNothing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctNa)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctInherit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctForbidden)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctAllowed)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private TaskBuilderNet.UI.WinControls.PictureWithBalloon CurrentUserPictureWithBalloon;
    }
}
