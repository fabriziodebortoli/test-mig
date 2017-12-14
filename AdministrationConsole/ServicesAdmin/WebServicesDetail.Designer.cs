using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class WebServicesDetail
    {
        private Label LabelTitle;
        private ContextMenuStrip webContextMenu;
        private ListView ViewDetails;
        private Panel DetailsPanel;
        private Splitter HorizontalSplitter;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebServicesDetail));
            this.LabelTitle = new System.Windows.Forms.Label();
            this.ViewDetails = new System.Windows.Forms.ListView();
            this.DetailsPanel = new System.Windows.Forms.Panel();
            this.HorizontalSplitter = new System.Windows.Forms.Splitter();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // LabelTitle
            // 
            this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.LabelTitle, "LabelTitle");
            this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LabelTitle.ForeColor = System.Drawing.Color.White;
            this.LabelTitle.Name = "LabelTitle";
            // 
            // ViewDetails
            // 
            this.ViewDetails.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.ViewDetails.AllowDrop = true;
            resources.ApplyResources(this.ViewDetails, "ViewDetails");
            this.ViewDetails.FullRowSelect = true;
            this.ViewDetails.HideSelection = false;
            this.ViewDetails.Name = "ViewDetails";
            this.ViewDetails.UseCompatibleStateImageBehavior = false;
            this.ViewDetails.View = System.Windows.Forms.View.Details;
            this.ViewDetails.ItemActivate += new System.EventHandler(this.ViewDetails_ItemActivate);
            this.ViewDetails.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ViewDetails_MouseDown);
            // 
            // DetailsPanel
            // 
            this.DetailsPanel.BackColor = System.Drawing.SystemColors.Control;
            this.DetailsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.DetailsPanel, "DetailsPanel");
            this.DetailsPanel.Name = "DetailsPanel";
            // 
            // HorizontalSplitter
            // 
            resources.ApplyResources(this.HorizontalSplitter, "HorizontalSplitter");
            this.HorizontalSplitter.Name = "HorizontalSplitter";
            this.HorizontalSplitter.TabStop = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Copy.png");
            this.imageList1.Images.SetKeyName(1, "Play.png");
            this.imageList1.Images.SetKeyName(2, "Detail-32x32.png");
            // 
            // WebServicesDetail
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.HorizontalSplitter);
            this.Controls.Add(this.DetailsPanel);
            this.Controls.Add(this.ViewDetails);
            this.Controls.Add(this.LabelTitle);
            this.Name = "WebServicesDetail";
            this.ShowInTaskbar = false;
            this.Resize += new System.EventHandler(this.WebServicesDetail_Resize);
            this.ResumeLayout(false);

        }
        #endregion

        private ImageList imageList1;
        private IContainer components;
    }
}
