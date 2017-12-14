namespace HttpNamespaceManager.UI
{
	partial class NamespaceMngForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NamespaceMngForm));
			this.nsControl = new HttpNamespaceManager.UI.NamespaceMngControl();
			this.SuspendLayout();
			// 
			// nsControl
			// 
			resources.ApplyResources(this.nsControl, "nsControl");
			this.nsControl.Name = "nsControl";
			// 
			// NamespaceMngForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.nsControl);
			this.Name = "NamespaceMngForm";
			this.ResumeLayout(false);

		}

		#endregion

		private NamespaceMngControl nsControl;
	}
}