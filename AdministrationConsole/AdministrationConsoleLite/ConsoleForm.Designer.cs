
using Microarea.Console.Core.PlugIns;
namespace Microarea.Console
{
    partial class ConsoleForm
    {
        public System.Windows.Forms.Panel plugInBottomWorkingArea;
        public System.Windows.Forms.Panel plugInWorkingArea;
        private System.Windows.Forms.Splitter splitterHorizontal;
        public PlugInsTreeView consoleTree;
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Splitter splitterVertical;
        private System.Windows.Forms.ToolTip toolTipTreeNode;


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


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConsoleForm));
            this.plugInBottomWorkingArea = new System.Windows.Forms.Panel();
            this.consoleTree = new Microarea.Console.Core.PlugIns.PlugInsTreeView();
            this.splitterHorizontal = new System.Windows.Forms.Splitter();
            this.splitterVertical = new System.Windows.Forms.Splitter();
            this.plugInWorkingArea = new System.Windows.Forms.Panel();
            this.toolTipTreeNode = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // plugInBottomWorkingArea
            // 
            resources.ApplyResources(this.plugInBottomWorkingArea, "plugInBottomWorkingArea");
            this.plugInBottomWorkingArea.BackColor = System.Drawing.SystemColors.Control;
            this.plugInBottomWorkingArea.Name = "plugInBottomWorkingArea";
            // 
            // consoleTree
            // 
            this.consoleTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.consoleTree, "consoleTree");
            this.consoleTree.HideSelection = false;
            this.consoleTree.ItemHeight = 20;
            this.consoleTree.Name = "consoleTree";
            this.consoleTree.ShowNodeToolTips = true;
            this.consoleTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.consoleTree_AfterExpand);
            this.consoleTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.consoleTree_BeforeSelect);
            this.consoleTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.consoleTree_AfterSelect);
            this.consoleTree.DoubleClick += new System.EventHandler(this.consoleTree_DoubleClick);
            this.consoleTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.consoleTree_KeyDown);
            this.consoleTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.consoleTree_MouseDown);
            // 
            // splitterHorizontal
            // 
            resources.ApplyResources(this.splitterHorizontal, "splitterHorizontal");
            this.splitterHorizontal.Name = "splitterHorizontal";
            this.splitterHorizontal.TabStop = false;
            // 
            // splitterVertical
            // 
            this.splitterVertical.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.splitterVertical, "splitterVertical");
            this.splitterVertical.Name = "splitterVertical";
            this.splitterVertical.TabStop = false;
            // 
            // plugInWorkingArea
            // 
            resources.ApplyResources(this.plugInWorkingArea, "plugInWorkingArea");
            this.plugInWorkingArea.Name = "plugInWorkingArea";
            this.plugInWorkingArea.TabStop = true;
            // 
            // ConsoleForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.plugInWorkingArea);
            this.Controls.Add(this.splitterVertical);
            this.Controls.Add(this.splitterHorizontal);
            this.Controls.Add(this.consoleTree);
            this.Controls.Add(this.plugInBottomWorkingArea);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConsoleForm";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
