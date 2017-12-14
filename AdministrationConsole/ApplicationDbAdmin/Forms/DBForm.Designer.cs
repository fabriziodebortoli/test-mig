
namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
    partial class DBForm
    {
        private System.Windows.Forms.ImageList myImages;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TreeView DirTreeView;
        private System.Windows.Forms.ListView TablesListView;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.CheckBox ImportDefaultDataCheckBox;
        private System.Windows.Forms.ToolTip TableToolTip;
        private System.Windows.Forms.Label ApplicationLabel;
        private System.Windows.Forms.Label ModuleLabel;
        private System.Windows.Forms.Label TableLabel;
        private System.Windows.Forms.Label ViewLabel;
        private System.Windows.Forms.PictureBox ApplicationPictureBox;
        private System.Windows.Forms.PictureBox ModulePictureBox;
        private System.Windows.Forms.PictureBox TablePictureBox;
        private System.Windows.Forms.PictureBox ViewPictureBox;
        private System.Windows.Forms.PictureBox StoredProcPictureBox;
        private System.Windows.Forms.Label CompanyLabel;
        private System.Windows.Forms.Panel LegendPanel2;
        private System.Windows.Forms.Panel LegendPanel1;
        private System.Windows.Forms.Label InfosLabel;
        private System.Windows.Forms.ComboBox DefaultConfigComboBox;
        private System.Windows.Forms.Label StoredProcLabel;


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------------
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
        //---------------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBForm));
			this.DirTreeView = new System.Windows.Forms.TreeView();
			this.TablesListView = new System.Windows.Forms.ListView();
			this.UpdateButton = new System.Windows.Forms.Button();
			this.ImportDefaultDataCheckBox = new System.Windows.Forms.CheckBox();
			this.TableToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.LegendPanel2 = new System.Windows.Forms.Panel();
			this.StoredProcLabel = new System.Windows.Forms.Label();
			this.StoredProcPictureBox = new System.Windows.Forms.PictureBox();
			this.ViewPictureBox = new System.Windows.Forms.PictureBox();
			this.TablePictureBox = new System.Windows.Forms.PictureBox();
			this.ViewLabel = new System.Windows.Forms.Label();
			this.TableLabel = new System.Windows.Forms.Label();
			this.ModulePictureBox = new System.Windows.Forms.PictureBox();
			this.ApplicationPictureBox = new System.Windows.Forms.PictureBox();
			this.ModuleLabel = new System.Windows.Forms.Label();
			this.ApplicationLabel = new System.Windows.Forms.Label();
			this.CompanyLabel = new System.Windows.Forms.Label();
			this.LegendPanel1 = new System.Windows.Forms.Panel();
			this.InfosLabel = new System.Windows.Forms.Label();
			this.DefaultConfigComboBox = new System.Windows.Forms.ComboBox();
			this.CreateOracleMViewCheckBox = new System.Windows.Forms.CheckBox();
			this.TreeSplitContainer = new System.Windows.Forms.SplitContainer();
			this.LegendPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StoredProcPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ViewPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TablePictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ModulePictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationPictureBox)).BeginInit();
			this.LegendPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TreeSplitContainer)).BeginInit();
			this.TreeSplitContainer.Panel1.SuspendLayout();
			this.TreeSplitContainer.Panel2.SuspendLayout();
			this.TreeSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// DirTreeView
			// 
			resources.ApplyResources(this.DirTreeView, "DirTreeView");
			this.DirTreeView.HideSelection = false;
			this.DirTreeView.ItemHeight = 16;
			this.DirTreeView.Name = "DirTreeView";
			this.DirTreeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DirTreeView_MouseMove);
			// 
			// TablesListView
			// 
			resources.ApplyResources(this.TablesListView, "TablesListView");
			this.TablesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.TablesListView.MultiSelect = false;
			this.TablesListView.Name = "TablesListView";
			this.TablesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.TablesListView.UseCompatibleStateImageBehavior = false;
			this.TablesListView.View = System.Windows.Forms.View.Details;
			// 
			// UpdateButton
			// 
			resources.ApplyResources(this.UpdateButton, "UpdateButton");
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// ImportDefaultDataCheckBox
			// 
			resources.ApplyResources(this.ImportDefaultDataCheckBox, "ImportDefaultDataCheckBox");
			this.ImportDefaultDataCheckBox.Name = "ImportDefaultDataCheckBox";
			this.ImportDefaultDataCheckBox.CheckedChanged += new System.EventHandler(this.ImportDefaultDataCheckBox_CheckedChanged);
			// 
			// LegendPanel2
			// 
			resources.ApplyResources(this.LegendPanel2, "LegendPanel2");
			this.LegendPanel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LegendPanel2.Controls.Add(this.StoredProcLabel);
			this.LegendPanel2.Controls.Add(this.StoredProcPictureBox);
			this.LegendPanel2.Controls.Add(this.ViewPictureBox);
			this.LegendPanel2.Controls.Add(this.TablePictureBox);
			this.LegendPanel2.Controls.Add(this.ViewLabel);
			this.LegendPanel2.Controls.Add(this.TableLabel);
			this.LegendPanel2.Name = "LegendPanel2";
			// 
			// StoredProcLabel
			// 
			resources.ApplyResources(this.StoredProcLabel, "StoredProcLabel");
			this.StoredProcLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.StoredProcLabel.Name = "StoredProcLabel";
			// 
			// StoredProcPictureBox
			// 
			this.StoredProcPictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.StoredProcPictureBox, "StoredProcPictureBox");
			this.StoredProcPictureBox.Name = "StoredProcPictureBox";
			this.StoredProcPictureBox.TabStop = false;
			// 
			// ViewPictureBox
			// 
			this.ViewPictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.ViewPictureBox, "ViewPictureBox");
			this.ViewPictureBox.Name = "ViewPictureBox";
			this.ViewPictureBox.TabStop = false;
			// 
			// TablePictureBox
			// 
			this.TablePictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TablePictureBox, "TablePictureBox");
			this.TablePictureBox.Name = "TablePictureBox";
			this.TablePictureBox.TabStop = false;
			// 
			// ViewLabel
			// 
			resources.ApplyResources(this.ViewLabel, "ViewLabel");
			this.ViewLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ViewLabel.Name = "ViewLabel";
			// 
			// TableLabel
			// 
			resources.ApplyResources(this.TableLabel, "TableLabel");
			this.TableLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TableLabel.Name = "TableLabel";
			// 
			// ModulePictureBox
			// 
			this.ModulePictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.ModulePictureBox, "ModulePictureBox");
			this.ModulePictureBox.Name = "ModulePictureBox";
			this.ModulePictureBox.TabStop = false;
			// 
			// ApplicationPictureBox
			// 
			this.ApplicationPictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.ApplicationPictureBox, "ApplicationPictureBox");
			this.ApplicationPictureBox.Name = "ApplicationPictureBox";
			this.ApplicationPictureBox.TabStop = false;
			// 
			// ModuleLabel
			// 
			this.ModuleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ModuleLabel, "ModuleLabel");
			this.ModuleLabel.Name = "ModuleLabel";
			// 
			// ApplicationLabel
			// 
			this.ApplicationLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ApplicationLabel, "ApplicationLabel");
			this.ApplicationLabel.Name = "ApplicationLabel";
			// 
			// CompanyLabel
			// 
			this.CompanyLabel.AllowDrop = true;
			resources.ApplyResources(this.CompanyLabel, "CompanyLabel");
			this.CompanyLabel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.CompanyLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CompanyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanyLabel.ForeColor = System.Drawing.Color.White;
			this.CompanyLabel.Name = "CompanyLabel";
			// 
			// LegendPanel1
			// 
			resources.ApplyResources(this.LegendPanel1, "LegendPanel1");
			this.LegendPanel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LegendPanel1.Controls.Add(this.ApplicationPictureBox);
			this.LegendPanel1.Controls.Add(this.ApplicationLabel);
			this.LegendPanel1.Controls.Add(this.ModuleLabel);
			this.LegendPanel1.Controls.Add(this.ModulePictureBox);
			this.LegendPanel1.Name = "LegendPanel1";
			// 
			// InfosLabel
			// 
			resources.ApplyResources(this.InfosLabel, "InfosLabel");
			this.InfosLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InfosLabel.Name = "InfosLabel";
			// 
			// DefaultConfigComboBox
			// 
			resources.ApplyResources(this.DefaultConfigComboBox, "DefaultConfigComboBox");
			this.DefaultConfigComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DefaultConfigComboBox.Name = "DefaultConfigComboBox";
			// 
			// CreateOracleMViewCheckBox
			// 
			resources.ApplyResources(this.CreateOracleMViewCheckBox, "CreateOracleMViewCheckBox");
			this.CreateOracleMViewCheckBox.Name = "CreateOracleMViewCheckBox";
			this.CreateOracleMViewCheckBox.UseVisualStyleBackColor = true;
			// 
			// TreeSplitContainer
			// 
			resources.ApplyResources(this.TreeSplitContainer, "TreeSplitContainer");
			this.TreeSplitContainer.Name = "TreeSplitContainer";
			// 
			// TreeSplitContainer.Panel1
			// 
			this.TreeSplitContainer.Panel1.Controls.Add(this.DirTreeView);
			// 
			// TreeSplitContainer.Panel2
			// 
			this.TreeSplitContainer.Panel2.Controls.Add(this.TablesListView);
			// 
			// DBForm
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.TreeSplitContainer);
			this.Controls.Add(this.CreateOracleMViewCheckBox);
			this.Controls.Add(this.DefaultConfigComboBox);
			this.Controls.Add(this.InfosLabel);
			this.Controls.Add(this.LegendPanel1);
			this.Controls.Add(this.CompanyLabel);
			this.Controls.Add(this.LegendPanel2);
			this.Controls.Add(this.ImportDefaultDataCheckBox);
			this.Controls.Add(this.UpdateButton);
			this.Name = "DBForm";
			this.LegendPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.StoredProcPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ViewPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TablePictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ModulePictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationPictureBox)).EndInit();
			this.LegendPanel1.ResumeLayout(false);
			this.TreeSplitContainer.Panel1.ResumeLayout(false);
			this.TreeSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TreeSplitContainer)).EndInit();
			this.TreeSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.CheckBox CreateOracleMViewCheckBox;
		private System.Windows.Forms.SplitContainer TreeSplitContainer;

    }
}
