
namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	/// <summary>
	/// Summary description for DictionaryViewer.
	/// </summary>
	public class DictionaryViewer : System.Windows.Forms.Form
	{
		private Microarea.TaskBuilderNet.Core.StringLoader.DictionaryViewerControl dictionaryViewerControl1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DictionaryViewer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dictionaryViewerControl1 = new Microarea.TaskBuilderNet.Core.StringLoader.DictionaryViewerControl();
			this.SuspendLayout();
			// 
			// dictionaryViewerControl1
			// 
			this.dictionaryViewerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dictionaryViewerControl1.Location = new System.Drawing.Point(0, 0);
			this.dictionaryViewerControl1.Name = "dictionaryViewerControl1";
			this.dictionaryViewerControl1.Size = new System.Drawing.Size(648, 350);
			this.dictionaryViewerControl1.TabIndex = 0;
			// 
			// DictionaryViewer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 350);
			this.Controls.Add(this.dictionaryViewerControl1);
			this.Name = "DictionaryViewer";
			this.Text = "Dictionary Viewer";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
