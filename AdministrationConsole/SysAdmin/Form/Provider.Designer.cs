using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class Provider
    {
        private Label lblProvider;
        private Label LabelTitle;
        private Label LblExplication;
        private ComboBox cbProvider;
        private GroupBox groupBoxParametri;
        private TextBox tbProviderId;
        private CheckBox cbStripTrailingSpaces;
        private CheckBox cbUseConstParameter;
        private Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Provider));
			this.lblProvider = new System.Windows.Forms.Label();
			this.cbProvider = new System.Windows.Forms.ComboBox();
			this.groupBoxParametri = new System.Windows.Forms.GroupBox();
			this.cbStripTrailingSpaces = new System.Windows.Forms.CheckBox();
			this.cbUseConstParameter = new System.Windows.Forms.CheckBox();
			this.tbProviderId = new System.Windows.Forms.TextBox();
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.groupBoxParametri.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblProvider
			// 
			resources.ApplyResources(this.lblProvider, "lblProvider");
			this.lblProvider.Name = "lblProvider";
			// 
			// cbProvider
			// 
			resources.ApplyResources(this.cbProvider, "cbProvider");
			this.cbProvider.Name = "cbProvider";
			this.cbProvider.SelectedIndexChanged += new System.EventHandler(this.cbProvider_SelectedIndexChanged);
			// 
			// groupBoxParametri
			// 
			resources.ApplyResources(this.groupBoxParametri, "groupBoxParametri");
			this.groupBoxParametri.Controls.Add(this.cbStripTrailingSpaces);
			this.groupBoxParametri.Controls.Add(this.cbUseConstParameter);
			this.groupBoxParametri.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBoxParametri.Name = "groupBoxParametri";
			this.groupBoxParametri.TabStop = false;
			// 
			// cbStripTrailingSpaces
			// 
			resources.ApplyResources(this.cbStripTrailingSpaces, "cbStripTrailingSpaces");
			this.cbStripTrailingSpaces.Name = "cbStripTrailingSpaces";
			this.cbStripTrailingSpaces.CheckedChanged += new System.EventHandler(this.cbStripTrailingSpaces_CheckedChanged);
			// 
			// cbUseConstParameter
			// 
			resources.ApplyResources(this.cbUseConstParameter, "cbUseConstParameter");
			this.cbUseConstParameter.Name = "cbUseConstParameter";
			this.cbUseConstParameter.CheckedChanged += new System.EventHandler(this.cbUseConstParameter_CheckedChanged);
			// 
			// tbProviderId
			// 
			resources.ApplyResources(this.tbProviderId, "tbProviderId");
			this.tbProviderId.Name = "tbProviderId";
			// 
			// LabelTitle
			// 
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelTitle.ForeColor = System.Drawing.Color.White;
			this.LabelTitle.Name = "LabelTitle";
			// 
			// LblExplication
			// 
			resources.ApplyResources(this.LblExplication, "LblExplication");
			this.LblExplication.Name = "LblExplication";
			// 
			// Provider
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.tbProviderId);
			this.Controls.Add(this.groupBoxParametri);
			this.Controls.Add(this.cbProvider);
			this.Controls.Add(this.lblProvider);
			this.Name = "Provider";
			this.ShowInTaskbar = false;
			this.Deactivate += new System.EventHandler(this.Provider_Deactivate);
			this.VisibleChanged += new System.EventHandler(this.Provider_VisibleChanged);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Provider_Closing);
			this.Load += new System.EventHandler(this.Provider_Load);
			this.groupBoxParametri.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
    }
}
