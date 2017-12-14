
namespace Microarea.Console.Core.DataManager
{
    partial class ShowDataSetForm
    {
        private System.Windows.Forms.DataGrid TableValuesDataGrid;
        private System.ComponentModel.Container components = null;

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
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ShowDataSetForm));
            this.TableValuesDataGrid = new System.Windows.Forms.DataGrid();
            ((System.ComponentModel.ISupportInitialize)(this.TableValuesDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // TableValuesDataGrid
            // 
            this.TableValuesDataGrid.AccessibleDescription = resources.GetString("TableValuesDataGrid.AccessibleDescription");
            this.TableValuesDataGrid.AccessibleName = resources.GetString("TableValuesDataGrid.AccessibleName");
            this.TableValuesDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TableValuesDataGrid.Anchor")));
            this.TableValuesDataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TableValuesDataGrid.BackgroundImage")));
            this.TableValuesDataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("TableValuesDataGrid.CaptionFont")));
            this.TableValuesDataGrid.CaptionText = resources.GetString("TableValuesDataGrid.CaptionText");
            this.TableValuesDataGrid.DataMember = "";
            this.TableValuesDataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TableValuesDataGrid.Dock")));
            this.TableValuesDataGrid.Enabled = ((bool)(resources.GetObject("TableValuesDataGrid.Enabled")));
            this.TableValuesDataGrid.Font = ((System.Drawing.Font)(resources.GetObject("TableValuesDataGrid.Font")));
            this.TableValuesDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TableValuesDataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TableValuesDataGrid.ImeMode")));
            this.TableValuesDataGrid.Location = ((System.Drawing.Point)(resources.GetObject("TableValuesDataGrid.Location")));
            this.TableValuesDataGrid.Name = "TableValuesDataGrid";
            this.TableValuesDataGrid.ReadOnly = true;
            this.TableValuesDataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TableValuesDataGrid.RightToLeft")));
            this.TableValuesDataGrid.Size = ((System.Drawing.Size)(resources.GetObject("TableValuesDataGrid.Size")));
            this.TableValuesDataGrid.TabIndex = ((int)(resources.GetObject("TableValuesDataGrid.TabIndex")));
            this.TableValuesDataGrid.Visible = ((bool)(resources.GetObject("TableValuesDataGrid.Visible")));
            // 
            // ShowDataSetForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.TableValuesDataGrid);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "ShowDataSetForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            ((System.ComponentModel.ISupportInitialize)(this.TableValuesDataGrid)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
