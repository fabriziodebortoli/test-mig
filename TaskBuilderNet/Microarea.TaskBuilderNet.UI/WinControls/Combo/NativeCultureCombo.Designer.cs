
namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
    partial class NativeCultureCombo
    {
        private System.Windows.Forms.CheckBox CkbNativeName;
        private CultureCombo CmbCultureApplication;
        private System.Windows.Forms.Label LblRegionalSettings;
        private System.ComponentModel.Container components = null;

        /// <summary> 
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NativeCultureCombo));
            this.CkbNativeName = new System.Windows.Forms.CheckBox();
            this.CmbCultureApplication = new CultureCombo();
            this.LblRegionalSettings = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CkbNativeName
            // 
            this.CkbNativeName.AccessibleDescription = resources.GetString("CkbNativeName.AccessibleDescription");
            this.CkbNativeName.AccessibleName = resources.GetString("CkbNativeName.AccessibleName");
            this.CkbNativeName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbNativeName.Anchor")));
            this.CkbNativeName.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbNativeName.Appearance")));
            this.CkbNativeName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbNativeName.BackgroundImage")));
            this.CkbNativeName.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNativeName.CheckAlign")));
            this.CkbNativeName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbNativeName.Dock")));
            this.CkbNativeName.Enabled = ((bool)(resources.GetObject("CkbNativeName.Enabled")));
            this.CkbNativeName.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbNativeName.FlatStyle")));
            this.CkbNativeName.Font = ((System.Drawing.Font)(resources.GetObject("CkbNativeName.Font")));
            this.CkbNativeName.Image = ((System.Drawing.Image)(resources.GetObject("CkbNativeName.Image")));
            this.CkbNativeName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNativeName.ImageAlign")));
            this.CkbNativeName.ImageIndex = ((int)(resources.GetObject("CkbNativeName.ImageIndex")));
            this.CkbNativeName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbNativeName.ImeMode")));
            this.CkbNativeName.Location = ((System.Drawing.Point)(resources.GetObject("CkbNativeName.Location")));
            this.CkbNativeName.Name = "CkbNativeName";
            this.CkbNativeName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbNativeName.RightToLeft")));
            this.CkbNativeName.Size = ((System.Drawing.Size)(resources.GetObject("CkbNativeName.Size")));
            this.CkbNativeName.TabIndex = ((int)(resources.GetObject("CkbNativeName.TabIndex")));
            this.CkbNativeName.Text = resources.GetString("CkbNativeName.Text");
            this.CkbNativeName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNativeName.TextAlign")));
            this.CkbNativeName.Visible = ((bool)(resources.GetObject("CkbNativeName.Visible")));
            this.CkbNativeName.CheckedChanged += new System.EventHandler(this.CkbNativeName_CheckedChanged);
            // 
            // CmbCultureApplication
            // 
            this.CmbCultureApplication.AccessibleDescription = resources.GetString("CmbCultureApplication.AccessibleDescription");
            this.CmbCultureApplication.AccessibleName = resources.GetString("CmbCultureApplication.AccessibleName");
            this.CmbCultureApplication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbCultureApplication.Anchor")));
            this.CmbCultureApplication.ApplicationLanguage = "";
            this.CmbCultureApplication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbCultureApplication.BackgroundImage")));
            this.CmbCultureApplication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbCultureApplication.Dock")));
            this.CmbCultureApplication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbCultureApplication.Enabled = ((bool)(resources.GetObject("CmbCultureApplication.Enabled")));
            this.CmbCultureApplication.Font = ((System.Drawing.Font)(resources.GetObject("CmbCultureApplication.Font")));
            this.CmbCultureApplication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbCultureApplication.ImeMode")));
            this.CmbCultureApplication.IntegralHeight = ((bool)(resources.GetObject("CmbCultureApplication.IntegralHeight")));
            this.CmbCultureApplication.ItemHeight = ((int)(resources.GetObject("CmbCultureApplication.ItemHeight")));
            this.CmbCultureApplication.Location = ((System.Drawing.Point)(resources.GetObject("CmbCultureApplication.Location")));
            this.CmbCultureApplication.MaxDropDownItems = ((int)(resources.GetObject("CmbCultureApplication.MaxDropDownItems")));
            this.CmbCultureApplication.MaxLength = ((int)(resources.GetObject("CmbCultureApplication.MaxLength")));
            this.CmbCultureApplication.Name = "CmbCultureApplication";
            this.CmbCultureApplication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbCultureApplication.RightToLeft")));
            this.CmbCultureApplication.Size = ((System.Drawing.Size)(resources.GetObject("CmbCultureApplication.Size")));
            this.CmbCultureApplication.TabIndex = ((int)(resources.GetObject("CmbCultureApplication.TabIndex")));
            this.CmbCultureApplication.Text = resources.GetString("CmbCultureApplication.Text");
            this.CmbCultureApplication.Visible = ((bool)(resources.GetObject("CmbCultureApplication.Visible")));
            // 
            // LblRegionalSettings
            // 
            this.LblRegionalSettings.AccessibleDescription = resources.GetString("LblRegionalSettings.AccessibleDescription");
            this.LblRegionalSettings.AccessibleName = resources.GetString("LblRegionalSettings.AccessibleName");
            this.LblRegionalSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblRegionalSettings.Anchor")));
            this.LblRegionalSettings.AutoSize = ((bool)(resources.GetObject("LblRegionalSettings.AutoSize")));
            this.LblRegionalSettings.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblRegionalSettings.Dock")));
            this.LblRegionalSettings.Enabled = ((bool)(resources.GetObject("LblRegionalSettings.Enabled")));
            this.LblRegionalSettings.Font = ((System.Drawing.Font)(resources.GetObject("LblRegionalSettings.Font")));
            this.LblRegionalSettings.Image = ((System.Drawing.Image)(resources.GetObject("LblRegionalSettings.Image")));
            this.LblRegionalSettings.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblRegionalSettings.ImageAlign")));
            this.LblRegionalSettings.ImageIndex = ((int)(resources.GetObject("LblRegionalSettings.ImageIndex")));
            this.LblRegionalSettings.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblRegionalSettings.ImeMode")));
            this.LblRegionalSettings.Location = ((System.Drawing.Point)(resources.GetObject("LblRegionalSettings.Location")));
            this.LblRegionalSettings.Name = "LblRegionalSettings";
            this.LblRegionalSettings.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblRegionalSettings.RightToLeft")));
            this.LblRegionalSettings.Size = ((System.Drawing.Size)(resources.GetObject("LblRegionalSettings.Size")));
            this.LblRegionalSettings.TabIndex = ((int)(resources.GetObject("LblRegionalSettings.TabIndex")));
            this.LblRegionalSettings.Text = resources.GetString("LblRegionalSettings.Text");
            this.LblRegionalSettings.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblRegionalSettings.TextAlign")));
            this.LblRegionalSettings.Visible = ((bool)(resources.GetObject("LblRegionalSettings.Visible")));
            // 
            // NativeCultureCombo
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.CmbCultureApplication);
            this.Controls.Add(this.CkbNativeName);
            this.Controls.Add(this.LblRegionalSettings);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "NativeCultureCombo";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.ResumeLayout(false);

        }
        #endregion

    }
}
