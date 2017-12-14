using System.Windows.Forms.Integration;

namespace Microarea.TaskBuilderNet.UI.CodeEditor.UserControls
{
	partial class WoormEditorControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();

			elementHost = new ElementHost();
			textEditorControl = new ICSharpCode.AvalonEdit.TextEditor();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WoormEditorControl));
			this.parserThreadLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.iconList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// textAreaPanel
			// 
			//this.textAreaPanel.Size = new System.Drawing.Size(356, 281);
			// 
			// parserThreadLabel
			// 
			this.parserThreadLabel.Name = "parserThreadLabel";
			this.parserThreadLabel.Size = new System.Drawing.Size(23, 23);
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

			elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Controls.Add(elementHost);

			elementHost.Child = textEditorControl;
			// 
			// 
			// CodeEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "CodeEditor";
			this.Size = new System.Drawing.Size(356, 281);
			//this.Controls.SetChildIndex(this.textAreaPanel, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripStatusLabel parserThreadLabel;
		internal System.Windows.Forms.ImageList iconList;

		private ElementHost						elementHost;
		private ICSharpCode.AvalonEdit.TextEditor textEditorControl;
	}
}
