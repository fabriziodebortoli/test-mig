
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class SelectApplicationCommandDlg
    {
        private System.Windows.Forms.Label Commandlabel;
        private System.Windows.Forms.Label ModuleLabel;
        private System.Windows.Forms.Label AppLabel;
        private ApplicationsComboBox AppComboBox;
        private System.Windows.Forms.ComboBox ModuleComboBox;
        private System.Windows.Forms.ComboBox CommandComboBox;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Label LibraryLabel;
        private System.Windows.Forms.ComboBox LibraryComboBox;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectApplicationCommandDlg));
			this.Commandlabel = new System.Windows.Forms.Label();
			this.ModuleLabel = new System.Windows.Forms.Label();
			this.AppLabel = new System.Windows.Forms.Label();
			this.CommandComboBox = new System.Windows.Forms.ComboBox();
			this.ModuleComboBox = new System.Windows.Forms.ComboBox();
			this.AppComboBox = new ApplicationsComboBox();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.OkBtn = new System.Windows.Forms.Button();
			this.LibraryLabel = new System.Windows.Forms.Label();
			this.LibraryComboBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// Commandlabel
			// 
			resources.ApplyResources(this.Commandlabel, "Commandlabel");
			this.Commandlabel.Name = "Commandlabel";
			// 
			// ModuleLabel
			// 
			resources.ApplyResources(this.ModuleLabel, "ModuleLabel");
			this.ModuleLabel.Name = "ModuleLabel";
			// 
			// AppLabel
			// 
			resources.ApplyResources(this.AppLabel, "AppLabel");
			this.AppLabel.Name = "AppLabel";
			// 
			// CommandComboBox
			// 
			this.CommandComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.CommandComboBox, "CommandComboBox");
			this.CommandComboBox.Name = "CommandComboBox";
			this.CommandComboBox.Sorted = true;
			// 
			// ModuleComboBox
			// 
			this.ModuleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.ModuleComboBox, "ModuleComboBox");
			this.ModuleComboBox.Name = "ModuleComboBox";
			this.ModuleComboBox.Sorted = true;
			this.ModuleComboBox.SelectedIndexChanged += new System.EventHandler(this.ModuleComboBox_SelectedIndexChanged);
			// 
			// AppComboBox
			// 
			this.AppComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.AppComboBox, "AppComboBox");
			this.AppComboBox.Name = "AppComboBox";
			this.AppComboBox.PathFinder = null;
			this.AppComboBox.Sorted = true;
			this.AppComboBox.SelectedIndexChanged += new System.EventHandler(this.AppComboBox_SelectedIndexChanged);
			// 
			// CancelBtn
			// 
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.CancelBtn, "CancelBtn");
			this.CancelBtn.Name = "CancelBtn";
			// 
			// OkBtn
			// 
			this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.OkBtn, "OkBtn");
			this.OkBtn.Name = "OkBtn";
			this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// LibraryLabel
			// 
			resources.ApplyResources(this.LibraryLabel, "LibraryLabel");
			this.LibraryLabel.Name = "LibraryLabel";
			// 
			// LibraryComboBox
			// 
			this.LibraryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.LibraryComboBox, "LibraryComboBox");
			this.LibraryComboBox.Name = "LibraryComboBox";
			this.LibraryComboBox.Sorted = true;
			this.LibraryComboBox.SelectedIndexChanged += new System.EventHandler(this.LibraryComboBox_SelectedIndexChanged);
			// 
			// SelectApplicationCommandDlg
			// 
			resources.ApplyResources(this, "$this");
			this.ControlBox = false;
			this.Controls.Add(this.LibraryLabel);
			this.Controls.Add(this.Commandlabel);
			this.Controls.Add(this.ModuleLabel);
			this.Controls.Add(this.AppLabel);
			this.Controls.Add(this.LibraryComboBox);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OkBtn);
			this.Controls.Add(this.CommandComboBox);
			this.Controls.Add(this.ModuleComboBox);
			this.Controls.Add(this.AppComboBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SelectApplicationCommandDlg";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

    }
}
