
using Microarea.Console.Core.SecurityLibrary;
namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class SecurityLightMigrationForm
    {
        private System.Windows.Forms.PictureBox SecuritylightMigrationPictureBox;
        private EllipsisLabel MigrationStepDescriptionLabel;
        private EllipsisLabel NameSpaceLabel;
        private System.Windows.Forms.ProgressBar ExecutionProgressBar;
        private System.Windows.Forms.Button UndoButton;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityLightMigrationForm));
            this.MigrationStepDescriptionLabel = new EllipsisLabel();
            this.ExecutionProgressBar = new System.Windows.Forms.ProgressBar();
            this.UndoButton = new System.Windows.Forms.Button();
            this.NameSpaceLabel = new EllipsisLabel();
            this.SecuritylightMigrationPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.SecuritylightMigrationPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MigrationStepDescriptionLabel
            // 
            resources.ApplyResources(this.MigrationStepDescriptionLabel, "MigrationStepDescriptionLabel");
            this.MigrationStepDescriptionLabel.ForeColor = System.Drawing.Color.Navy;
            this.MigrationStepDescriptionLabel.Name = "MigrationStepDescriptionLabel";
            // 
            // ExecutionProgressBar
            // 
            resources.ApplyResources(this.ExecutionProgressBar, "ExecutionProgressBar");
            this.ExecutionProgressBar.Name = "ExecutionProgressBar";
            // 
            // UndoButton
            // 
            resources.ApplyResources(this.UndoButton, "UndoButton");
            this.UndoButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // NameSpaceLabel
            // 
            resources.ApplyResources(this.NameSpaceLabel, "NameSpaceLabel");
            this.NameSpaceLabel.ForeColor = System.Drawing.Color.Navy;
            this.NameSpaceLabel.Name = "NameSpaceLabel";
            // 
            // SecuritylightMigrationPictureBox
            // 
            resources.ApplyResources(this.SecuritylightMigrationPictureBox, "SecuritylightMigrationPictureBox");
            this.SecuritylightMigrationPictureBox.Name = "SecuritylightMigrationPictureBox";
            this.SecuritylightMigrationPictureBox.TabStop = false;
            // 
            // SecurityLightMigrationForm
            // 
            this.AcceptButton = this.UndoButton;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.CancelButton = this.UndoButton;
            this.Controls.Add(this.UndoButton);
            this.Controls.Add(this.ExecutionProgressBar);
            this.Controls.Add(this.NameSpaceLabel);
            this.Controls.Add(this.MigrationStepDescriptionLabel);
            this.Controls.Add(this.SecuritylightMigrationPictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SecurityLightMigrationForm";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.SecuritylightMigrationPictureBox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
