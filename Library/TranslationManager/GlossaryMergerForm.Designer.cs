using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Library.TranslationManager
{
    partial class GlossaryMergerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button CMDOk;
        private System.Windows.Forms.ListBox LSTGlossaries;
        private System.Windows.Forms.OpenFileDialog openGlossaryDialog;
        private System.Windows.Forms.Button CMDAdd;
        private System.Windows.Forms.Button CMDRemove;
        private System.Windows.Forms.Button CMDReset;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlossaryMergerForm));
            this.CMDOk = new System.Windows.Forms.Button();
            this.LSTGlossaries = new System.Windows.Forms.ListBox();
            this.openGlossaryDialog = new System.Windows.Forms.OpenFileDialog();
            this.CMDAdd = new System.Windows.Forms.Button();
            this.CMDRemove = new System.Windows.Forms.Button();
            this.CMDReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CMDOk
            // 
            resources.ApplyResources(this.CMDOk, "CMDOk");
            this.CMDOk.Name = "CMDOk";
            this.CMDOk.Click += new System.EventHandler(this.CMDOk_Click);
            // 
            // LSTGlossaries
            // 
            resources.ApplyResources(this.LSTGlossaries, "LSTGlossaries");
            this.LSTGlossaries.Name = "LSTGlossaries";
            // 
            // openGlossaryDialog
            // 
            this.openGlossaryDialog.DefaultExt = "*.xml";
            resources.ApplyResources(this.openGlossaryDialog, "openGlossaryDialog");
            // 
            // CMDAdd
            // 
            resources.ApplyResources(this.CMDAdd, "CMDAdd");
            this.CMDAdd.Name = "CMDAdd";
            this.CMDAdd.Click += new System.EventHandler(this.CMDAdd_Click);
            // 
            // CMDRemove
            // 
            resources.ApplyResources(this.CMDRemove, "CMDRemove");
            this.CMDRemove.Name = "CMDRemove";
            this.CMDRemove.Click += new System.EventHandler(this.CMDRemove_Click);
            // 
            // CMDReset
            // 
            resources.ApplyResources(this.CMDReset, "CMDReset");
            this.CMDReset.Name = "CMDReset";
            this.CMDReset.Click += new System.EventHandler(this.CMDReset_Click);
            // 
            // GlossaryMergerForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.CMDReset);
            this.Controls.Add(this.CMDRemove);
            this.Controls.Add(this.CMDAdd);
            this.Controls.Add(this.LSTGlossaries);
            this.Controls.Add(this.CMDOk);
            this.Name = "GlossaryMergerForm";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
