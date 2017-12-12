using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class ListViewDetail
    {
        private ListView listViewData;
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListViewDetail));
			this.listViewData = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// listViewData
			// 
			this.listViewData.AllowDrop = true;
			this.listViewData.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.listViewData, "listViewData");
			this.listViewData.ForeColor = System.Drawing.SystemColors.ControlText;
			this.listViewData.FullRowSelect = true;
			this.listViewData.HideSelection = false;
			this.listViewData.Name = "listViewData";
			this.listViewData.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewData.UseCompatibleStateImageBehavior = false;
			this.listViewData.View = System.Windows.Forms.View.Details;
			this.listViewData.DoubleClick += new System.EventHandler(this.listViewData_DoubleClick);
			// 
			// ListViewDetail
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.listViewData);
			this.Name = "ListViewDetail";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ListViewDetail_Closing);
			this.Deactivate += new System.EventHandler(this.ListViewDetail_Deactivate);
			this.SizeChanged += new System.EventHandler(this.ListViewDetail_SizeChanged);
			this.VisibleChanged += new System.EventHandler(this.ListViewDetail_VisibleChanged);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListViewDetail_KeyUp);
			this.Resize += new System.EventHandler(this.ListViewDetail_Resize);
			this.ResumeLayout(false);

        }
        #endregion
    }
}
