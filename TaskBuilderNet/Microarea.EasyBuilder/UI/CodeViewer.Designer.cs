namespace Microarea.EasyBuilder.UI
{
	partial class CodeViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeViewer));
			this.codeEditor = new Microarea.EasyBuilder.UI.CodeEditorControl();
			this.SuspendLayout();
			// 
			// codeEditor
			// 
			this.codeEditor.AllowDrop = true;
			resources.ApplyResources(this.codeEditor, "codeEditor");
			this.codeEditor.Name = "codeEditor";
			// 
			// CodeViewer
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.codeEditor);
			this.Name = "CodeViewer";
			this.ResumeLayout(false);

		}

		#endregion

		private CodeEditorControl codeEditor;
	}
}