using System;
using System.Windows.Forms;
namespace Microarea.MenuManager
{
	partial class MenuEditorDockableForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (menuDesignerForm != null)
				{
					if (!menuDesignerForm.IsDisposed)
					{
						menuDesignerForm.FormClosed -= new FormClosedEventHandler(MenuDesignerForm_FormClosed);
						menuDesignerForm.TextChanged -= new EventHandler(MenuDesignerForm_TextChanged);
						menuDesignerForm.Dispose();
					}

					menuDesignerForm = null;
				}
				if (contentPanel != null)
				{
					if (!contentPanel.IsDisposed)
						contentPanel.Dispose();

					contentPanel = null;
				}
				if (outerPanel != null)
				{
					if (!outerPanel.IsDisposed)
						outerPanel.Dispose();

					outerPanel = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuEditorDockableForm));
			this.SuspendLayout();
			// 
			// MenuEditorDockableForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "MenuEditorDockableForm";
			this.ResumeLayout(false);

		}

		#endregion
	}
}