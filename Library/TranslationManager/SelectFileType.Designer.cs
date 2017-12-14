using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Library.TranslationManager
{
    partial class SelectFileType
    {
        private System.Windows.Forms.CheckedListBox CHKFileTypeList;
        private System.Windows.Forms.Button CMDOk;
        private System.Windows.Forms.Button CMDSelectAll;
        private System.Windows.Forms.Button CMDSelectNone;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectFileType));
            this.CHKFileTypeList = new System.Windows.Forms.CheckedListBox();
            this.CMDOk = new System.Windows.Forms.Button();
            this.CMDSelectAll = new System.Windows.Forms.Button();
            this.CMDSelectNone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CHKFileTypeList
            // 
            resources.ApplyResources(this.CHKFileTypeList, "CHKFileTypeList");
            this.CHKFileTypeList.Name = "CHKFileTypeList";
            // 
            // CMDOk
            // 
            resources.ApplyResources(this.CMDOk, "CMDOk");
            this.CMDOk.Name = "CMDOk";
            this.CMDOk.Click += new System.EventHandler(this.CMDOk_Click);
            // 
            // CMDSelectAll
            // 
            resources.ApplyResources(this.CMDSelectAll, "CMDSelectAll");
            this.CMDSelectAll.Name = "CMDSelectAll";
            this.CMDSelectAll.Click += new System.EventHandler(this.CMDSelectAll_Click);
            // 
            // CMDSelectNone
            // 
            resources.ApplyResources(this.CMDSelectNone, "CMDSelectNone");
            this.CMDSelectNone.Name = "CMDSelectNone";
            this.CMDSelectNone.Click += new System.EventHandler(this.CMDSelectNone_Click);
            // 
            // SelectFileType
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.CMDSelectNone);
            this.Controls.Add(this.CMDSelectAll);
            this.Controls.Add(this.CMDOk);
            this.Controls.Add(this.CHKFileTypeList);
            this.Name = "SelectFileType";
            this.Load += new System.EventHandler(this.SelectFileType_Load);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
