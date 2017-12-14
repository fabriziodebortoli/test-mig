
namespace Microarea.TaskBuilderNet.UI.WinControls
{
     //--------------------------------------------------------------------
	partial class CircularProgressBar : System.Windows.Forms.UserControl 
    { 
		// Required by the Windows Form Designer
		private System.ComponentModel.IContainer components = null; 

		// UserControl1 overrides dispose to clean up the component list.
		[ System.Diagnostics.DebuggerNonUserCode() ]
		protected override void Dispose( bool disposing ) 
		{ 
			if ( disposing && components != null ) 
			{ 
				components.Dispose(); 
			} 
			base.Dispose( disposing ); 
		} 
        
        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [ System.Diagnostics.DebuggerStepThrough() ]
        private void InitializeComponent() 
        {
			this.SuspendLayout();
			// 
			// CircularProgressBar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "CircularProgressBar";
			this.Size = new System.Drawing.Size(30, 30);
			this.EnabledChanged += new System.EventHandler(this.SpinningProgress_EnabledChanged);
			this.ResumeLayout(false);
        } 
    } 
} 