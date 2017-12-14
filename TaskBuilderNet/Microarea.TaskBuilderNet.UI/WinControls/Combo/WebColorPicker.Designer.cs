
namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
    partial class WebColorPicker
    {
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPickerComboBox WebColorComboBox;
        private WebColorSelectionButton SelectColorButton;

        private System.ComponentModel.IContainer components = null;


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WebColorPicker));
			this.WebColorComboBox = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPickerComboBox();
			this.SelectColorButton = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker.WebColorSelectionButton();
            this.SuspendLayout();
            // 
            // WebColorComboBox
            // 
            this.WebColorComboBox.AccessibleDescription = resources.GetString("WebColorComboBox.AccessibleDescription");
            this.WebColorComboBox.AccessibleName = resources.GetString("WebColorComboBox.AccessibleName");
            this.WebColorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WebColorComboBox.Anchor")));
            this.WebColorComboBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WebColorComboBox.BackgroundImage")));
            this.WebColorComboBox.CustomColor = System.Drawing.Color.Empty;
            this.WebColorComboBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WebColorComboBox.Dock")));
            this.WebColorComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.WebColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WebColorComboBox.Enabled = ((bool)(resources.GetObject("WebColorComboBox.Enabled")));
            this.WebColorComboBox.Font = ((System.Drawing.Font)(resources.GetObject("WebColorComboBox.Font")));
            this.WebColorComboBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WebColorComboBox.ImeMode")));
            this.WebColorComboBox.IntegralHeight = ((bool)(resources.GetObject("WebColorComboBox.IntegralHeight")));
            this.WebColorComboBox.ItemHeight = ((int)(resources.GetObject("WebColorComboBox.ItemHeight")));
            this.WebColorComboBox.Location = ((System.Drawing.Point)(resources.GetObject("WebColorComboBox.Location")));
            this.WebColorComboBox.MaxDropDownItems = ((int)(resources.GetObject("WebColorComboBox.MaxDropDownItems")));
            this.WebColorComboBox.MaxLength = ((int)(resources.GetObject("WebColorComboBox.MaxLength")));
            this.WebColorComboBox.Name = "WebColorComboBox";
            this.WebColorComboBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WebColorComboBox.RightToLeft")));
            this.WebColorComboBox.SelectedColor = System.Drawing.Color.Empty;
            this.WebColorComboBox.Size = ((System.Drawing.Size)(resources.GetObject("WebColorComboBox.Size")));
            this.WebColorComboBox.TabIndex = ((int)(resources.GetObject("WebColorComboBox.TabIndex")));
            this.WebColorComboBox.Text = resources.GetString("WebColorComboBox.Text");
            this.WebColorComboBox.Visible = ((bool)(resources.GetObject("WebColorComboBox.Visible")));
            this.WebColorComboBox.SelectedIndexChanged += new System.EventHandler(this.WebColorComboBox_SelectedIndexChanged);
            // 
            // SelectColorButton
            // 
            this.SelectColorButton.AccessibleDescription = resources.GetString("SelectColorButton.AccessibleDescription");
            this.SelectColorButton.AccessibleName = resources.GetString("SelectColorButton.AccessibleName");
            this.SelectColorButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelectColorButton.Anchor")));
            this.SelectColorButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SelectColorButton.BackgroundImage")));
            this.SelectColorButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelectColorButton.Dock")));
            this.SelectColorButton.Enabled = ((bool)(resources.GetObject("SelectColorButton.Enabled")));
            this.SelectColorButton.Font = ((System.Drawing.Font)(resources.GetObject("SelectColorButton.Font")));
            this.SelectColorButton.ForeColor = System.Drawing.Color.Navy;
            this.SelectColorButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelectColorButton.ImeMode")));
            this.SelectColorButton.Location = ((System.Drawing.Point)(resources.GetObject("SelectColorButton.Location")));
            this.SelectColorButton.Name = "SelectColorButton";
            this.SelectColorButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelectColorButton.RightToLeft")));
            this.SelectColorButton.Size = ((System.Drawing.Size)(resources.GetObject("SelectColorButton.Size")));
            this.SelectColorButton.TabIndex = ((int)(resources.GetObject("SelectColorButton.TabIndex")));
            this.SelectColorButton.Text = resources.GetString("SelectColorButton.Text");
            this.SelectColorButton.Visible = ((bool)(resources.GetObject("SelectColorButton.Visible")));
            this.SelectColorButton.Click += new System.EventHandler(this.SelectColorButton_Click);
            // 
            // WebColorPicker
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackColor = System.Drawing.Color.Lavender;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.SelectColorButton);
            this.Controls.Add(this.WebColorComboBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "WebColorPicker";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.ResumeLayout(false);

        }
        #endregion

    }
}
