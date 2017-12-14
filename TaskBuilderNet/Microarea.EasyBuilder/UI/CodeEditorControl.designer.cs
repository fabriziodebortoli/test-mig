using System.Windows.Forms.Integration;
using Microarea.EasyBuilder.CodeCompletion;

namespace Microarea.EasyBuilder.UI
{
	partial class CodeEditorControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

      
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeEditorControl));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.bottomToolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.tsText = new System.Windows.Forms.ToolStripTextBox();
			this.tsNextFind = new System.Windows.Forms.ToolStripButton();
			this.tsPreviousFind = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsClose = new System.Windows.Forms.ToolStripButton();
			this.tslblMatched = new System.Windows.Forms.ToolStripLabel();
			this.ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsChangeFont = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tsIncreaseFont = new System.Windows.Forms.ToolStripButton();
			this.tsDecreseFont = new System.Windows.Forms.ToolStripButton();
			this.toolStripSelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripGenerateTestPlan = new System.Windows.Forms.ToolStripMenuItem();
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.miniToolStrip = new System.Windows.Forms.ToolStrip();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.elementHost = new System.Windows.Forms.Integration.ElementHost();
			this.textEditorControl = new Microarea.EasyBuilder.CodeCompletion.CodeTextEditor();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.bottomToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.bottomToolStrip);
			this.toolStripContainer1.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.elementHost);
			resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
			resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
			this.toolStripContainer1.Name = "toolStripContainer1";
			// 
			// bottomToolStrip
			// 
			this.bottomToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.bottomToolStrip.CanOverflow = false;
			resources.ApplyResources(this.bottomToolStrip, "bottomToolStrip");
			this.bottomToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.bottomToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tsText,
            this.tsNextFind,
            this.tsPreviousFind,
            this.toolStripSeparator1,
            this.tsClose,
            this.tslblMatched,
            this.ToolStripSeparator2,
            this.tsChangeFont,
            this.toolStripSeparator3,
            this.tsIncreaseFont,
            this.tsDecreseFont});
			this.bottomToolStrip.Name = "bottomToolStrip";
			this.bottomToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
			// 
			// tsText
			// 
			this.tsText.Name = "tsText";
			resources.ApplyResources(this.tsText, "tsText");
			this.tsText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TsText_KeyDown);
			this.tsText.TextChanged += new System.EventHandler(this.tsText_TextChanged);
			// 
			// tsNextFind
			// 
			this.tsNextFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsNextFind.Image = global::Microarea.EasyBuilder.Properties.Resources.Next;
			resources.ApplyResources(this.tsNextFind, "tsNextFind");
			this.tsNextFind.Name = "tsNextFind";
			this.tsNextFind.Click += new System.EventHandler(this.tsNextFind_Click);
			// 
			// tsPreviousFind
			// 
			this.tsPreviousFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsPreviousFind.Image = global::Microarea.EasyBuilder.Properties.Resources.Previous;
			resources.ApplyResources(this.tsPreviousFind, "tsPreviousFind");
			this.tsPreviousFind.Name = "tsPreviousFind";
			this.tsPreviousFind.Click += new System.EventHandler(this.tsPreviousFind_Click);
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			// 
			// tsClose
			// 
			this.tsClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsClose.Image = global::Microarea.EasyBuilder.Properties.Resources.Close;
			resources.ApplyResources(this.tsClose, "tsClose");
			this.tsClose.Name = "tsClose";
			this.tsClose.Click += new System.EventHandler(this.tsClose_Click);
			// 
			// tslblMatched
			// 
			this.tslblMatched.Name = "tslblMatched";
			resources.ApplyResources(this.tslblMatched, "tslblMatched");
			// 
			// ToolStripSeparator2
			// 
			resources.ApplyResources(this.ToolStripSeparator2, "ToolStripSeparator2");
			this.ToolStripSeparator2.Name = "ToolStripSeparator2";
			// 
			// tsChangeFont
			// 
			this.tsChangeFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsChangeFont.Image = global::Microarea.EasyBuilder.Properties.Resources.ChangeFont;
			resources.ApplyResources(this.tsChangeFont, "tsChangeFont");
			this.tsChangeFont.Name = "tsChangeFont";
			this.tsChangeFont.Click += new System.EventHandler(this.tsChangeFont_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// tsIncreaseFont
			// 
			this.tsIncreaseFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsIncreaseFont.Image = global::Microarea.EasyBuilder.Properties.Resources.ZoomIn;
			resources.ApplyResources(this.tsIncreaseFont, "tsIncreaseFont");
			this.tsIncreaseFont.Name = "tsIncreaseFont";
			this.tsIncreaseFont.Click += new System.EventHandler(this.tsIncreaseFont_Click);
			// 
			// tsDecreseFont
			// 
			this.tsDecreseFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsDecreseFont.Image = global::Microarea.EasyBuilder.Properties.Resources.ZoomOut;
			resources.ApplyResources(this.tsDecreseFont, "tsDecreseFont");
			this.tsDecreseFont.Name = "tsDecreseFont";
			this.tsDecreseFont.Click += new System.EventHandler(this.tsDecreseFont_Click);
			// 
			// toolStripSelectAll
			// 
			this.toolStripSelectAll.Name = "toolStripSelectAll";
			resources.ApplyResources(this.toolStripSelectAll, "toolStripSelectAll");
			// 
			// toolStripCopy
			// 
			this.toolStripCopy.Name = "toolStripCopy";
			resources.ApplyResources(this.toolStripCopy, "toolStripCopy");
			// 
			// toolStripPaste
			// 
			this.toolStripPaste.Name = "toolStripPaste";
			resources.ApplyResources(this.toolStripPaste, "toolStripPaste");
			// 
			// toolStripGenerateTestPlan
			// 
			this.toolStripGenerateTestPlan.Name = "toolStripGenerateTestPlan";
			resources.ApplyResources(this.toolStripGenerateTestPlan, "toolStripGenerateTestPlan");
			// 
			// BottomToolStripPanel
			// 
			resources.ApplyResources(this.BottomToolStripPanel, "BottomToolStripPanel");
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// miniToolStrip
			// 
			this.miniToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.miniToolStrip.CanOverflow = false;
			this.miniToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			resources.ApplyResources(this.miniToolStrip, "miniToolStrip");
			this.miniToolStrip.Name = "miniToolStrip";
			this.miniToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// TopToolStripPanel
			// 
			resources.ApplyResources(this.TopToolStripPanel, "TopToolStripPanel");
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// RightToolStripPanel
			// 
			resources.ApplyResources(this.RightToolStripPanel, "RightToolStripPanel");
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// LeftToolStripPanel
			// 
			resources.ApplyResources(this.LeftToolStripPanel, "LeftToolStripPanel");
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// ContentPanel
			// 
			resources.ApplyResources(this.ContentPanel, "ContentPanel");
			// 
			// elementHost
			// 
			resources.ApplyResources(this.elementHost, "elementHost");
			this.elementHost.Name = "elementHost";
			this.elementHost.Child = this.textEditorControl;
			// 
			// CodeEditorControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "CodeEditorControl";
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.bottomToolStrip.ResumeLayout(false);
			this.bottomToolStrip.PerformLayout();
			this.ResumeLayout(false);

        }

	

		#endregion
       // private System.Windows.Controls.ContextMenu optionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripGenerateTestPlan;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStrip miniToolStrip;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStrip bottomToolStrip;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox tsText;
		private System.Windows.Forms.ToolStripButton tsNextFind;
		private System.Windows.Forms.ToolStripButton tsPreviousFind;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsClose;
		private System.Windows.Forms.ToolStripLabel tslblMatched;
		internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator2;
		private System.Windows.Forms.ToolStripButton tsChangeFont;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton tsIncreaseFont;
		private System.Windows.Forms.ToolStripButton tsDecreseFont;
		private ElementHost elementHost;
		private CodeTextEditor textEditorControl;
	}
}
