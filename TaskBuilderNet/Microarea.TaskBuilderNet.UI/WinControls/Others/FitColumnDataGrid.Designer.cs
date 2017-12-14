namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class FitColumnDataGrid
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
                if (toolTip != null)
                {
                    toolTip.Dispose();
                    toolTip = null;
                }
                
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FitColumnDataGrid));
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // FitColumnDataGrid
            // 
            this.AlternatingBackColor = System.Drawing.Color.White;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundColor = System.Drawing.Color.Lavender;
            this.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
            resources.ApplyResources(this, "$this");
            this.CaptionForeColor = System.Drawing.Color.White;
            this.ForeColor = System.Drawing.Color.Navy;
            this.GridLineColor = System.Drawing.Color.White;
            this.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
            this.HeaderForeColor = System.Drawing.Color.MidnightBlue;
            this.LinkColor = System.Drawing.Color.MidnightBlue;
            this.ParentRowsBackColor = System.Drawing.Color.White;
            this.ParentRowsForeColor = System.Drawing.Color.Navy;
            this.SelectionBackColor = System.Drawing.Color.RoyalBlue;
            this.SelectionForeColor = System.Drawing.Color.White;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
