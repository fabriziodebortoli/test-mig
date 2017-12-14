using Microarea.TaskBuilderNet.UI.CodeEditor.UserControls;
using Microarea.TaskBuilderNet.UI.CodeEditor.Woorm;
namespace Microarea.TaskBuilderNet.UI.CodeEditor
{
	partial class ScriptEditor
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
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
			this.splitVertical = new System.Windows.Forms.SplitContainer();
			this.operatorPad = new Microarea.TaskBuilderNet.UI.CodeEditor.Woorm.OperatorPad();
			this.treeCodeElements = new System.Windows.Forms.TreeView();
			this.iconList = new System.Windows.Forms.ImageList(this.components);
			this.splitHorizontal = new System.Windows.Forms.SplitContainer();
			this.codeEditor = new Microarea.TaskBuilderNet.UI.CodeEditor.UserControls.WoormEditorControl();
			this.txtHelper = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitVertical)).BeginInit();
			this.splitVertical.Panel1.SuspendLayout();
			this.splitVertical.Panel2.SuspendLayout();
			this.splitVertical.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitHorizontal)).BeginInit();
			this.splitHorizontal.Panel1.SuspendLayout();
			this.splitHorizontal.Panel2.SuspendLayout();
			this.splitHorizontal.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitVertical
			// 
			resources.ApplyResources(this.splitVertical, "splitVertical");
			this.splitVertical.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitVertical.Name = "splitVertical";
			// 
			// splitVertical.Panel1
			// 
			this.splitVertical.Panel1.Controls.Add(this.operatorPad);
			this.splitVertical.Panel1.Controls.Add(this.treeCodeElements);
			// 
			// splitVertical.Panel2
			// 
			this.splitVertical.Panel2.Controls.Add(this.splitHorizontal);
			// 
			// operatorPad
			// 
			resources.ApplyResources(this.operatorPad, "operatorPad");
			this.operatorPad.BackColor = System.Drawing.SystemColors.Control;
			this.operatorPad.Name = "operatorPad";
			this.operatorPad.OperatorSelected += new System.EventHandler<Microarea.TaskBuilderNet.UI.CodeEditor.Woorm.OperatorSelectedEventArgs>(this.operatorPad_OperatorSelected);
			// 
			// treeCodeElements
			// 
			resources.ApplyResources(this.treeCodeElements, "treeCodeElements");
			this.treeCodeElements.ImageList = this.iconList;
			this.treeCodeElements.Name = "treeCodeElements";
			this.treeCodeElements.DoubleClick += new System.EventHandler(this.treeCodeElements_DoubleClick);
			this.treeCodeElements.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeCodeElements_MouseMove);
			// 
			// iconList
			// 
			this.iconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconList.ImageStream")));
			this.iconList.TransparentColor = System.Drawing.Color.Transparent;
			this.iconList.Images.SetKeyName(0, "Class.png");
			this.iconList.Images.SetKeyName(1, "Methods.png");
			this.iconList.Images.SetKeyName(2, "Properties.png");
			this.iconList.Images.SetKeyName(3, "Fields.png");
			this.iconList.Images.SetKeyName(4, "Enums.png");
			this.iconList.Images.SetKeyName(5, "Namespace.png");
			this.iconList.Images.SetKeyName(6, "Events.png");
			this.iconList.Images.SetKeyName(7, "Table.PNG");
			this.iconList.Images.SetKeyName(8, "Column.PNG");
			this.iconList.Images.SetKeyName(9, "Container.PNG");
			this.iconList.Images.SetKeyName(10, "Keywords.PNG");
			this.iconList.Images.SetKeyName(11, "EnumTag.PNG");
			this.iconList.Images.SetKeyName(12, "EnumItem.PNG");
			// 
			// splitHorizontal
			// 
			resources.ApplyResources(this.splitHorizontal, "splitHorizontal");
			this.splitHorizontal.Name = "splitHorizontal";
			// 
			// splitHorizontal.Panel1
			// 
			this.splitHorizontal.Panel1.Controls.Add(this.codeEditor);
			// 
			// splitHorizontal.Panel2
			// 
			this.splitHorizontal.Panel2.Controls.Add(this.txtHelper);
			this.splitHorizontal.TabStop = false;
			// 
			// codeEditor
			// 
			resources.ApplyResources(this.codeEditor, "codeEditor");
			this.codeEditor.IconList = this.iconList;
			this.codeEditor.Name = "codeEditor";
			// 
			// txtHelper
			// 
			resources.ApplyResources(this.txtHelper, "txtHelper");
			this.txtHelper.Name = "txtHelper";
			this.txtHelper.ReadOnly = true;
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// ScriptEditor
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitVertical);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Name = "ScriptEditor";
			this.ShowInTaskbar = false;
			this.splitVertical.Panel1.ResumeLayout(false);
			this.splitVertical.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitVertical)).EndInit();
			this.splitVertical.ResumeLayout(false);
			this.splitHorizontal.Panel1.ResumeLayout(false);
			this.splitHorizontal.Panel2.ResumeLayout(false);
			this.splitHorizontal.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitHorizontal)).EndInit();
			this.splitHorizontal.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitVertical;
		private System.Windows.Forms.TreeView treeCodeElements;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtHelper;
		private System.Windows.Forms.SplitContainer splitHorizontal;
		private OperatorPad operatorPad;
		internal System.Windows.Forms.ImageList iconList;
		protected WoormEditorControl codeEditor;
	}
}
