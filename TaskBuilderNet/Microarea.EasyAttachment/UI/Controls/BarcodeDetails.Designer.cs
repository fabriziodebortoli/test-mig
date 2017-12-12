namespace Microarea.EasyAttachment.UI.Controls
{
	partial class BarcodeDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BarcodeDetails));
            this.BCPanel = new System.Windows.Forms.Panel();
            this.LblNoBarcode = new System.Windows.Forms.Label();
            this.BarcodeViewer = new Microarea.TBPicComponents.TBPicViewer();
            this.TxtBCValue = new System.Windows.Forms.TextBox();
            this.LblValue = new System.Windows.Forms.Label();
            this.BtnShowBarcode = new System.Windows.Forms.Button();
            this.BCToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BCPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BCPanel
            // 
            this.BCPanel.Controls.Add(this.LblNoBarcode);
            this.BCPanel.Controls.Add(this.BarcodeViewer);
            this.BCPanel.Controls.Add(this.TxtBCValue);
            this.BCPanel.Controls.Add(this.LblValue);
            this.BCPanel.Controls.Add(this.BtnShowBarcode);
            resources.ApplyResources(this.BCPanel, "BCPanel");
            this.BCPanel.Name = "BCPanel";
            this.BCPanel.MouseEnter += new System.EventHandler(this.BCPanel_MouseEnter);
            // 
            // LblNoBarcode
            // 
            resources.ApplyResources(this.LblNoBarcode, "LblNoBarcode");
            this.LblNoBarcode.BackColor = System.Drawing.Color.Lavender;
            this.LblNoBarcode.Name = "LblNoBarcode";
            // 
            // BarcodeViewer
            // 
            this.BarcodeViewer.AllowDrop = true;
            resources.ApplyResources(this.BarcodeViewer, "BarcodeViewer");
            this.BarcodeViewer.AnimateGIF = false;
            this.BarcodeViewer.BackColor = System.Drawing.Color.Lavender;
            this.BarcodeViewer.ContinuousViewMode = true;
            this.BarcodeViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.BarcodeViewer.DisplayQuality = Microarea.TBPicComponents.TBPicDisplayQuality.DisplayQualityBicubicHQ;
            this.BarcodeViewer.DisplayQualityAuto = true;
            this.BarcodeViewer.DocumentAlignment = Microarea.TBPicComponents.TBPicViewerDocumentAlignment.DocumentAlignmentMiddleLeft;
            this.BarcodeViewer.DocumentPosition = Microarea.TBPicComponents.TBPicViewerDocumentPosition.DocumentPositionMiddleLeft;
            this.BarcodeViewer.EnableMenu = true;
            this.BarcodeViewer.EnableMouseWheel = true;
            this.BarcodeViewer.ForceScrollBars = false;
            this.BarcodeViewer.ForceTemporaryModeForImage = false;
            this.BarcodeViewer.ForceTemporaryModeForPDF = false;
            this.BarcodeViewer.ForeColor = System.Drawing.Color.Black;
            this.BarcodeViewer.Gamma = 1F;
            this.BarcodeViewer.HQAnnotationRendering = true;
            this.BarcodeViewer.IgnoreDocumentResolution = false;
            this.BarcodeViewer.KeepDocumentPosition = false;
            this.BarcodeViewer.LockViewer = false;
            this.BarcodeViewer.MouseButtonForMouseMode = Microarea.TBPicComponents.TBPicMouseButton.MouseButtonLeft;
            this.BarcodeViewer.MouseMode = Microarea.TBPicComponents.TBPicViewerMouseMode.MouseModePan;
            this.BarcodeViewer.MouseWheelMode = Microarea.TBPicComponents.TBPicViewerMouseWheelMode.MouseWheelModeVerticalScroll;
            this.BarcodeViewer.Name = "BarcodeViewer";
            this.BarcodeViewer.OptimizeDrawingSpeed = true;
            this.BarcodeViewer.PdfDisplayFormField = true;
            this.BarcodeViewer.PdfEnableLinks = true;
            this.BarcodeViewer.PdfShowDialogForPassword = true;
            this.BarcodeViewer.RectBorderColor = System.Drawing.Color.Red;
            this.BarcodeViewer.RectBorderSize = 3;
            this.BarcodeViewer.RectIsEditable = false;
            this.BarcodeViewer.RegionsAreEditable = false;
            this.BarcodeViewer.ScrollBars = true;
            this.BarcodeViewer.ScrollLargeChange = ((short)(50));
            this.BarcodeViewer.ScrollSmallChange = ((short)(1));
            this.BarcodeViewer.SilentMode = false;
            this.BarcodeViewer.Zoom = 0.001D;
            this.BarcodeViewer.ZoomCenterAtMousePosition = false;
            this.BarcodeViewer.ZoomMode = Microarea.TBPicComponents.TBPicViewerZoomMode.ZoomModeFitToViewer;
            this.BarcodeViewer.ZoomStep = 25;
            // 
            // TxtBCValue
            // 
            resources.ApplyResources(this.TxtBCValue, "TxtBCValue");
            this.TxtBCValue.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TxtBCValue.Name = "TxtBCValue";
            this.TxtBCValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtBCValue_KeyPress);
            this.TxtBCValue.Leave += new System.EventHandler(this.TxtBCValue_Leave);
            // 
            // LblValue
            // 
            resources.ApplyResources(this.LblValue, "LblValue");
            this.LblValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblValue.Name = "LblValue";
            // 
            // BtnShowBarcode
            // 
            this.BtnShowBarcode.Image = global::Microarea.EasyAttachment.Properties.Resources.Barcode48x48;
            resources.ApplyResources(this.BtnShowBarcode, "BtnShowBarcode");
            this.BtnShowBarcode.Name = "BtnShowBarcode";
            this.BtnShowBarcode.UseVisualStyleBackColor = false;
            this.BtnShowBarcode.Click += new System.EventHandler(this.BtnShowBarcode_Click);
            this.BtnShowBarcode.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BtnShowBarcode_MouseMove);
            // 
            // BCToolTip
            // 
            this.BCToolTip.AutoPopDelay = 3000;
            this.BCToolTip.InitialDelay = 30;
            this.BCToolTip.ReshowDelay = 60;
            this.BCToolTip.ShowAlways = true;
            // 
            // BarcodeDetails
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.BCPanel);
            this.Name = "BarcodeDetails";
            this.VisibleChanged += new System.EventHandler(this.BarcodeDetails_VisibleChanged);
            this.BCPanel.ResumeLayout(false);
            this.BCPanel.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel BCPanel;
		private System.Windows.Forms.Button BtnShowBarcode;
		private System.Windows.Forms.ToolTip BCToolTip;
		private System.Windows.Forms.Label LblValue;
		private System.Windows.Forms.TextBox TxtBCValue;
        internal TBPicComponents.TBPicViewer BarcodeViewer;
		private System.Windows.Forms.Label LblNoBarcode;
	}
}
