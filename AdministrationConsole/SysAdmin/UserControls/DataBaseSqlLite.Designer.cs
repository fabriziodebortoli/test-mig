using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
    //=========================================================================
    partial class DataBaseSqlLite
    {
        private Label LblServerName;
        private Label lblOwner;

        private ApplicationUsers cbUserOwner;
        private IContainer components = null;

        /// <summary> 
        /// Dispose
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBaseSqlLite));
			this.LblServerName = new System.Windows.Forms.Label();
			this.lblOwner = new System.Windows.Forms.Label();
			this.TxtDatabaseName = new System.Windows.Forms.TextBox();
			this.TxtServerName = new System.Windows.Forms.TextBox();
			this.cbUserOwner = new Microarea.Console.Plugin.SysAdmin.UserControls.ApplicationUsers();
			this.LblDatabaseName = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// LblServerName
			// 
			resources.ApplyResources(this.LblServerName, "LblServerName");
			this.LblServerName.Name = "LblServerName";
			// 
			// lblOwner
			// 
			resources.ApplyResources(this.lblOwner, "lblOwner");
			this.lblOwner.Name = "lblOwner";
			// 
			// TxtDatabaseName
			// 
			resources.ApplyResources(this.TxtDatabaseName, "TxtDatabaseName");
			this.TxtDatabaseName.Name = "TxtDatabaseName";
			this.TxtDatabaseName.TextChanged += new System.EventHandler(this.TxtDatabaseName_TextChanged);
			this.TxtDatabaseName.Leave += new System.EventHandler(this.TxtDatabaseName_Leave);
			// 
			// TxtServerName
			// 
			resources.ApplyResources(this.TxtServerName, "TxtServerName");
			this.TxtServerName.Name = "TxtServerName";
			// 
			// cbUserOwner
			// 
			resources.ApplyResources(this.cbUserOwner, "cbUserOwner");
			this.cbUserOwner.CurrentConnection = null;
			this.cbUserOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbUserOwner.Name = "cbUserOwner";
			this.cbUserOwner.SelectedUserId = "";
			this.cbUserOwner.SelectedUserIsWinNT = false;
			this.cbUserOwner.SelectedUserName = "";
			this.cbUserOwner.SelectedUserPwd = "";
			this.cbUserOwner.SelectedIndexChanged += new System.EventHandler(this.cbUserOwner_SelectedIndexChanged);
			// 
			// LblDatabaseName
			// 
			resources.ApplyResources(this.LblDatabaseName, "LblDatabaseName");
			this.LblDatabaseName.Name = "LblDatabaseName";
			// 
			// DataBaseSqlLite
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LblDatabaseName);
			this.Controls.Add(this.TxtDatabaseName);
			this.Controls.Add(this.TxtServerName);
			this.Controls.Add(this.cbUserOwner);
			this.Controls.Add(this.lblOwner);
			this.Controls.Add(this.LblServerName);
			this.Name = "DataBaseSqlLite";
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
		private TextBox TxtServerName;
		private TextBox TxtDatabaseName;
		private Label LblDatabaseName;
	}
}
