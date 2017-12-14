using Microarea.TaskBuilderNet.UI.WinControls.Others;
namespace Microarea.EasyBuilder.UI
{
	partial class CodeEditorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeEditorForm));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.methodHeader = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainerHorizontal = new System.Windows.Forms.SplitContainer();
			this.toolStrip1 = new Microarea.TaskBuilderNet.UI.WinControls.Others.ClickThroughToolStrip();
			this.tsSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.tsComment = new System.Windows.Forms.ToolStripButton();
			this.tsUncomment = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsFormatCode = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.tsShowLineNumbers = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tsExit = new System.Windows.Forms.ToolStripButton();
			this.tsMethodsCombo = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.codeEditor = new Microarea.EasyBuilder.UI.CodeEditorControl();
			this.dgBuildResult = new System.Windows.Forms.DataGridView();
			this.Image = new System.Windows.Forms.DataGridViewImageColumn();
			this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ErrorNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cmsReferences = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addReferenceToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.toolStripContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).BeginInit();
			this.splitContainerHorizontal.Panel1.SuspendLayout();
			this.splitContainerHorizontal.Panel2.SuspendLayout();
			this.splitContainerHorizontal.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgBuildResult)).BeginInit();
			this.cmsReferences.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
			resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
			this.toolStripContainer1.Name = "toolStripContainer1";
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			// 
			// methodHeader
			// 
			this.methodHeader.Name = "methodHeader";
			resources.ApplyResources(this.methodHeader, "methodHeader");
			// 
			// splitContainerHorizontal
			// 
			resources.ApplyResources(this.splitContainerHorizontal, "splitContainerHorizontal");
			this.splitContainerHorizontal.Name = "splitContainerHorizontal";
			// 
			// splitContainerHorizontal.Panel1
			// 
			this.splitContainerHorizontal.Panel1.Controls.Add(this.toolStrip1);
			this.splitContainerHorizontal.Panel1.Controls.Add(this.codeEditor);
			// 
			// splitContainerHorizontal.Panel2
			// 
			this.splitContainerHorizontal.Panel2.Controls.Add(this.dgBuildResult);
			// 
			// toolStrip1
			// 
			this.toolStrip1.ClickThrough = true;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSave,
            this.toolStripSeparator4,
            this.tsComment,
            this.tsUncomment,
            this.toolStripSeparator2,
            this.tsFormatCode,
            this.toolStripSeparator5,
            this.tsShowLineNumbers,
            this.toolStripSeparator3,
            this.tsExit,
            this.tsMethodsCombo,
            this.toolStripLabel1});
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// tsSave
			// 
			this.tsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsSave, "tsSave");
			this.tsSave.Image = global::Microarea.EasyBuilder.Properties.Resources.ApplyChanges;
			this.tsSave.Name = "tsSave";
			this.tsSave.Click += new System.EventHandler(this.TsSave_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
			// 
			// tsComment
			// 
			this.tsComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsComment.Image = global::Microarea.EasyBuilder.Properties.Resources.CommentCode;
			resources.ApplyResources(this.tsComment, "tsComment");
			this.tsComment.Name = "tsComment";
			this.tsComment.Click += new System.EventHandler(this.tsComment_Click);
			// 
			// tsUncomment
			// 
			this.tsUncomment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsUncomment.Image = global::Microarea.EasyBuilder.Properties.Resources.UnCommentCode;
			resources.ApplyResources(this.tsUncomment, "tsUncomment");
			this.tsUncomment.Name = "tsUncomment";
			this.tsUncomment.Click += new System.EventHandler(this.tsUncomment_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// tsFormatCode
			// 
			this.tsFormatCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsFormatCode.Image = global::Microarea.EasyBuilder.Properties.Resources.Formatting;
			resources.ApplyResources(this.tsFormatCode, "tsFormatCode");
			this.tsFormatCode.Name = "tsFormatCode";
			this.tsFormatCode.Click += new System.EventHandler(this.tsFormatCode_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
			// 
			// tsShowLineNumbers
			// 
			this.tsShowLineNumbers.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsShowLineNumbers.Image = global::Microarea.EasyBuilder.Properties.Resources.LineNumbers;
			resources.ApplyResources(this.tsShowLineNumbers, "tsShowLineNumbers");
			this.tsShowLineNumbers.Name = "tsShowLineNumbers";
			this.tsShowLineNumbers.Click += new System.EventHandler(this.tsShowLineNumbers_Click);
			// 
			// toolStripSeparator3
			// 
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			// 
			// tsExit
			// 
			this.tsExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsExit.Image = global::Microarea.EasyBuilder.Properties.Resources.Exit24;
			resources.ApplyResources(this.tsExit, "tsExit");
			this.tsExit.Name = "tsExit";
			this.tsExit.Click += new System.EventHandler(this.TsExit_Click);
			// 
			// tsMethodsCombo
			// 
			this.tsMethodsCombo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			resources.ApplyResources(this.tsMethodsCombo, "tsMethodsCombo");
			this.tsMethodsCombo.DropDownHeight = 200;
			this.tsMethodsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.tsMethodsCombo.DropDownWidth = 300;
			this.tsMethodsCombo.Name = "tsMethodsCombo";
			this.tsMethodsCombo.SelectedIndexChanged += new System.EventHandler(this.tsMethodsCombo_SelectedIndexChanged);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripLabel1.Name = "toolStripLabel1";
			resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
			// 
			// codeEditor
			// 
			this.codeEditor.AllowDrop = true;
			resources.ApplyResources(this.codeEditor, "codeEditor");
			this.codeEditor.CodeEditorControlMode = Microarea.EasyBuilder.UI.CodeEditorControlMode.CodeEditor;
			this.codeEditor.Name = "codeEditor";
			// 
			// dgBuildResult
			// 
			this.dgBuildResult.AllowUserToAddRows = false;
			this.dgBuildResult.AllowUserToDeleteRows = false;
			this.dgBuildResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgBuildResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dgBuildResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgBuildResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Image,
            this.Description,
            this.ErrorNumber,
            this.Line,
            this.Column});
			resources.ApplyResources(this.dgBuildResult, "dgBuildResult");
			this.dgBuildResult.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.dgBuildResult.GridColor = System.Drawing.SystemColors.ControlLightLight;
			this.dgBuildResult.MultiSelect = false;
			this.dgBuildResult.Name = "dgBuildResult";
			this.dgBuildResult.ReadOnly = true;
			this.dgBuildResult.RowHeadersVisible = false;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgBuildResult.RowsDefaultCellStyle = dataGridViewCellStyle1;
			this.dgBuildResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgBuildResult.ShowEditingIcon = false;
			this.dgBuildResult.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgBuildResult_CellMouseDoubleClick);
			// 
			// Image
			// 
			this.Image.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.Image.FillWeight = 129.4416F;
			resources.ApplyResources(this.Image, "Image");
			this.Image.Name = "Image";
			this.Image.ReadOnly = true;
			// 
			// Description
			// 
			this.Description.FillWeight = 237.849F;
			resources.ApplyResources(this.Description, "Description");
			this.Description.Name = "Description";
			this.Description.ReadOnly = true;
			// 
			// ErrorNumber
			// 
			this.ErrorNumber.FillWeight = 71.3547F;
			resources.ApplyResources(this.ErrorNumber, "ErrorNumber");
			this.ErrorNumber.Name = "ErrorNumber";
			this.ErrorNumber.ReadOnly = true;
			// 
			// Line
			// 
			this.Line.FillWeight = 39.6415F;
			resources.ApplyResources(this.Line, "Line");
			this.Line.Name = "Line";
			this.Line.ReadOnly = true;
			// 
			// Column
			// 
			this.Column.FillWeight = 31.7132F;
			resources.ApplyResources(this.Column, "Column");
			this.Column.Name = "Column";
			this.Column.ReadOnly = true;
			// 
			// cmsReferences
			// 
			this.cmsReferences.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addReferenceToolStripMenuItem2,
            this.deleteReferenceToolStripMenuItem});
			this.cmsReferences.Name = "cmsReferences";
			resources.ApplyResources(this.cmsReferences, "cmsReferences");
			// 
			// addReferenceToolStripMenuItem2
			// 
			this.addReferenceToolStripMenuItem2.Name = "addReferenceToolStripMenuItem2";
			resources.ApplyResources(this.addReferenceToolStripMenuItem2, "addReferenceToolStripMenuItem2");
			// 
			// deleteReferenceToolStripMenuItem
			// 
			this.deleteReferenceToolStripMenuItem.Name = "deleteReferenceToolStripMenuItem";
			resources.ApplyResources(this.deleteReferenceToolStripMenuItem, "deleteReferenceToolStripMenuItem");
			// 
			// CodeEditorForm
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerHorizontal);
			this.Controls.Add(this.toolStripContainer1);
			this.KeyPreview = true;
			this.Name = "CodeEditorForm";
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.splitContainerHorizontal.Panel1.ResumeLayout(false);
			this.splitContainerHorizontal.Panel1.PerformLayout();
			this.splitContainerHorizontal.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).EndInit();
			this.splitContainerHorizontal.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgBuildResult)).EndInit();
			this.cmsReferences.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.SplitContainer	splitContainerHorizontal;
		private System.Windows.Forms.DataGridView dgBuildResult;
		private System.Windows.Forms.ContextMenuStrip cmsReferences;
		private System.Windows.Forms.ToolStripMenuItem addReferenceToolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem deleteReferenceToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripStatusLabel methodHeader;
		private CodeEditorControl codeEditor;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private ClickThroughToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripButton tsSave;
		private System.Windows.Forms.ToolStripButton tsExit;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.DataGridViewImageColumn Image;
		private System.Windows.Forms.DataGridViewTextBoxColumn Description;
		private System.Windows.Forms.DataGridViewTextBoxColumn ErrorNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn Line;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column;
		private System.Windows.Forms.ToolStripButton tsComment;
		private System.Windows.Forms.ToolStripButton tsUncomment;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton tsFormatCode;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripComboBox tsMethodsCombo;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripButton tsShowLineNumbers;
	}
}
