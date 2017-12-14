
namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class SetArticles
    {
        private System.Windows.Forms.RichTextBox DescriptionLabel;
        private System.Windows.Forms.ListView lstUsers;
        private System.Windows.Forms.ListView lstArticols;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetArticles));
            this.lstArticols = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.DescriptionLabel = new System.Windows.Forms.RichTextBox();
            this.lstUsers = new System.Windows.Forms.ListView();
            this.LblConnectedUsers = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LstMobile = new System.Windows.Forms.ListView();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.LblTitle = new System.Windows.Forms.Label();
            this.BtnClearAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // lstArticols
            // 
            resources.ApplyResources(this.lstArticols, "lstArticols");
            this.lstArticols.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstArticols.FullRowSelect = true;
            this.lstArticols.HideSelection = false;
            this.lstArticols.MultiSelect = false;
            this.lstArticols.Name = "lstArticols";
            this.lstArticols.SmallImageList = this.imageList1;
            this.lstArticols.UseCompatibleStateImageBehavior = false;
            this.lstArticols.View = System.Windows.Forms.View.Details;
            this.lstArticols.SelectedIndexChanged += new System.EventHandler(this.lstArticols_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "User.png");
            this.imageList1.Images.SetKeyName(1, "Stop.png");
            this.imageList1.Images.SetKeyName(2, "Play.png");
            // 
            // DescriptionLabel
            // 
            resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
            this.DescriptionLabel.BackColor = System.Drawing.SystemColors.Control;
            this.DescriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DescriptionLabel.Name = "DescriptionLabel";
            // 
            // lstUsers
            // 
            resources.ApplyResources(this.lstUsers, "lstUsers");
            this.lstUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.SmallImageList = this.imageList1;
            this.lstUsers.UseCompatibleStateImageBehavior = false;
            this.lstUsers.View = System.Windows.Forms.View.SmallIcon;
            this.lstUsers.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstUsers_ColumnClick);
            this.lstUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstUsers_ItemCheck);
            this.lstUsers.DoubleClick += new System.EventHandler(this.lstUsers_DoubleClick);
            // 
            // LblConnectedUsers
            // 
            resources.ApplyResources(this.LblConnectedUsers, "LblConnectedUsers");
            this.LblConnectedUsers.ForeColor = System.Drawing.Color.Red;
            this.LblConnectedUsers.Name = "LblConnectedUsers";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.lstUsers);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackColor = System.Drawing.SystemColors.Window;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.LstMobile);
            this.panel2.Controls.Add(this.lstArticols);
            this.panel2.Name = "panel2";
            // 
            // LstMobile
            // 
            resources.ApplyResources(this.LstMobile, "LstMobile");
            this.LstMobile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LstMobile.FullRowSelect = true;
            this.LstMobile.HideSelection = false;
            this.LstMobile.MultiSelect = false;
            this.LstMobile.Name = "LstMobile";
            this.LstMobile.SmallImageList = this.imageList1;
            this.LstMobile.UseCompatibleStateImageBehavior = false;
            this.LstMobile.View = System.Windows.Forms.View.Details;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Microarea.Console.Plugin.ServicesAdmin.Strings.People53;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // LblTitle
            // 
            this.LblTitle.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.LblTitle, "LblTitle");
            this.LblTitle.Name = "LblTitle";
            // 
            // BtnClearAll
            // 
            resources.ApplyResources(this.BtnClearAll, "BtnClearAll");
            this.BtnClearAll.Name = "BtnClearAll";
            this.BtnClearAll.UseVisualStyleBackColor = true;
            this.BtnClearAll.Click += new System.EventHandler(this.BtnClearAll_Click);
            // 
            // SetArticles
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.BtnClearAll);
            this.Controls.Add(this.LblTitle);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LblConnectedUsers);
            this.Controls.Add(this.DescriptionLabel);
            this.Name = "SetArticles";
            this.Load += new System.EventHandler(this.SetArticols_Load);
            this.ParentChanged += new System.EventHandler(this.SetArticols_ParentChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.Label LblConnectedUsers;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Button BtnClearAll;
        private System.Windows.Forms.ListView LstMobile;
    }
}
