namespace Microarea.Library.DataStructureUtils.UI
{
    partial class DBObjectsDefControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBObjectsDefControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.GridSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DataStructureGrid = new System.Windows.Forms.DataGridView();
            this.StructureDetailsDataGrid = new System.Windows.Forms.DataGridView();
            this.DataStructureTypeSelLabel = new System.Windows.Forms.Label();
            this.ShowDefsLabel = new System.Windows.Forms.Label();
            this.ShowDefsTypeComboBox = new System.Windows.Forms.ComboBox();
            this.BricksPictureBox = new System.Windows.Forms.PictureBox();
            this.EditModeButton = new System.Windows.Forms.CheckBox();
            this.EditModeImageList = new System.Windows.Forms.ImageList(this.components);
            this.ControlToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.GridSplitContainer.Panel1.SuspendLayout();
            this.GridSplitContainer.Panel2.SuspendLayout();
            this.GridSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataStructureGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StructureDetailsDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BricksPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // GridSplitContainer
            // 
            resources.ApplyResources(this.GridSplitContainer, "GridSplitContainer");
            this.GridSplitContainer.BackColor = System.Drawing.Color.Transparent;
            this.GridSplitContainer.Name = "GridSplitContainer";
            // 
            // GridSplitContainer.Panel1
            // 
            this.GridSplitContainer.Panel1.Controls.Add(this.DataStructureGrid);
            // 
            // GridSplitContainer.Panel2
            // 
            this.GridSplitContainer.Panel2.Controls.Add(this.StructureDetailsDataGrid);
            // 
            // DataStructureGrid
            // 
            this.DataStructureGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DataStructureGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.DataStructureGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.DataStructureGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.RoyalBlue;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataStructureGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DataStructureGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DataStructureGrid.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.DataStructureGrid, "DataStructureGrid");
            this.DataStructureGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.DataStructureGrid.EnableHeadersVisualStyles = false;
            this.DataStructureGrid.GridColor = System.Drawing.Color.Silver;
            this.DataStructureGrid.MultiSelect = false;
            this.DataStructureGrid.Name = "DataStructureGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.RoyalBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataStructureGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.DataStructureGrid.RowTemplate.Height = 24;
            this.DataStructureGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // StructureDetailsDataGrid
            // 
            this.StructureDetailsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.StructureDetailsDataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.StructureDetailsDataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.StructureDetailsDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.RoyalBlue;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.StructureDetailsDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.StructureDetailsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.StructureDetailsDataGrid.DefaultCellStyle = dataGridViewCellStyle5;
            resources.ApplyResources(this.StructureDetailsDataGrid, "StructureDetailsDataGrid");
            this.StructureDetailsDataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.StructureDetailsDataGrid.EnableHeadersVisualStyles = false;
            this.StructureDetailsDataGrid.GridColor = System.Drawing.Color.Silver;
            this.StructureDetailsDataGrid.MultiSelect = false;
            this.StructureDetailsDataGrid.Name = "StructureDetailsDataGrid";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Verdana", 9F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.RoyalBlue;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.StructureDetailsDataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.StructureDetailsDataGrid.RowTemplate.Height = 24;
            this.StructureDetailsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // DataStructureTypeSelLabel
            // 
            resources.ApplyResources(this.DataStructureTypeSelLabel, "DataStructureTypeSelLabel");
            this.DataStructureTypeSelLabel.AutoEllipsis = true;
            this.DataStructureTypeSelLabel.BackColor = System.Drawing.Color.Transparent;
            this.DataStructureTypeSelLabel.Name = "DataStructureTypeSelLabel";
            // 
            // ShowDefsLabel
            // 
            resources.ApplyResources(this.ShowDefsLabel, "ShowDefsLabel");
            this.ShowDefsLabel.AutoEllipsis = true;
            this.ShowDefsLabel.BackColor = System.Drawing.Color.Transparent;
            this.ShowDefsLabel.Name = "ShowDefsLabel";
            // 
            // ShowDefsTypeComboBox
            // 
            resources.ApplyResources(this.ShowDefsTypeComboBox, "ShowDefsTypeComboBox");
            this.ShowDefsTypeComboBox.BackColor = System.Drawing.Color.White;
            this.ShowDefsTypeComboBox.ForeColor = System.Drawing.Color.Navy;
            this.ShowDefsTypeComboBox.FormattingEnabled = true;
            this.ShowDefsTypeComboBox.Name = "ShowDefsTypeComboBox";
            this.ShowDefsTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.ShowDefsTypeComboBox_SelectedIndexChanged);
            // 
            // BricksPictureBox
            // 
            this.BricksPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.BricksPictureBox.Image = global::Microarea.Library.DataStructureUtils.UI.Strings.Bricks;
            resources.ApplyResources(this.BricksPictureBox, "BricksPictureBox");
            this.BricksPictureBox.Name = "BricksPictureBox";
            this.BricksPictureBox.TabStop = false;
            // 
            // EditModeButton
            // 
            resources.ApplyResources(this.EditModeButton, "EditModeButton");
            this.EditModeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(228)))), ((int)(((byte)(255)))));
            this.EditModeButton.FlatAppearance.BorderColor = System.Drawing.Color.RoyalBlue;
            this.EditModeButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.LightSteelBlue;
            this.EditModeButton.ImageList = this.EditModeImageList;
            this.EditModeButton.Name = "EditModeButton";
            this.ControlToolTip.SetToolTip(this.EditModeButton, resources.GetString("EditModeButton.ToolTip"));
            this.EditModeButton.UseVisualStyleBackColor = false;
            this.EditModeButton.CheckedChanged += new System.EventHandler(this.EditModeButton_CheckedChanged);
            // 
            // EditModeImageList
            // 
            this.EditModeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("EditModeImageList.ImageStream")));
            this.EditModeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.EditModeImageList.Images.SetKeyName(0, "ReadOnlyModeButton.png");
            this.EditModeImageList.Images.SetKeyName(1, "EditModeButton.png");
            // 
            // DBObjectsDefControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.EditModeButton);
            this.Controls.Add(this.BricksPictureBox);
            this.Controls.Add(this.ShowDefsTypeComboBox);
            this.Controls.Add(this.ShowDefsLabel);
            this.Controls.Add(this.DataStructureTypeSelLabel);
            this.Controls.Add(this.GridSplitContainer);
            this.ForeColor = System.Drawing.Color.Navy;
            this.MinimumSize = new System.Drawing.Size(686, 222);
            this.Name = "DBObjectsDefControl";
            this.GridSplitContainer.Panel1.ResumeLayout(false);
            this.GridSplitContainer.Panel2.ResumeLayout(false);
            this.GridSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataStructureGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StructureDetailsDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BricksPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer GridSplitContainer;
        private System.Windows.Forms.DataGridView DataStructureGrid;
        private System.Windows.Forms.DataGridView StructureDetailsDataGrid;
        private System.Windows.Forms.Label DataStructureTypeSelLabel;
        private System.Windows.Forms.Label ShowDefsLabel;
        private System.Windows.Forms.ComboBox ShowDefsTypeComboBox;
        private System.Windows.Forms.PictureBox BricksPictureBox;
        private System.Windows.Forms.CheckBox EditModeButton;
        private System.Windows.Forms.ToolTip ControlToolTip;
        private System.Windows.Forms.ImageList EditModeImageList;
    }
}
