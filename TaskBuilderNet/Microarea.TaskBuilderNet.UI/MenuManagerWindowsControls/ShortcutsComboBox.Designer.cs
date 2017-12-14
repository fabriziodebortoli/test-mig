
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuShortcutsCompositeComboBox
    {

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

        //---------------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuShortcutsCompositeComboBox));
			this.ComboPanel = new System.Windows.Forms.Panel();
			this.ShortcutContextMenu = new System.Windows.Forms.ContextMenu();
			this.ShortcutToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.RunShortcutButton = new MenuShortcutsRunButton();
			this.ShortcutsComboBox = new MenuShortcutsComboBox();
			this.ComboPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ComboPanel
			// 
			this.ComboPanel.Controls.Add(this.ShortcutsComboBox);
			resources.ApplyResources(this.ComboPanel, "ComboPanel");
			this.ComboPanel.Name = "ComboPanel";
			// 
			// ShortcutContextMenu
			// 
			this.ShortcutContextMenu.Popup += new System.EventHandler(this.ShortcutContextMenu_Popup);
			// 
			// RunShortcutButton
			// 
			resources.ApplyResources(this.RunShortcutButton, "RunShortcutButton");
			this.RunShortcutButton.Name = "RunShortcutButton";
			this.RunShortcutButton.Click += new System.EventHandler(this.RunShortcutButton_Click);
			this.RunShortcutButton.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RunShortcutButton_KeyUp);
			// 
			// ShortcutsComboBox
			// 
			resources.ApplyResources(this.ShortcutsComboBox, "ShortcutsComboBox");
			this.ShortcutsComboBox.ContextMenu = this.ShortcutContextMenu;
			this.ShortcutsComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.ShortcutsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ShortcutsComboBox.ForeColor = System.Drawing.Color.Navy;
			this.ShortcutsComboBox.ImageList = null;
			this.ShortcutsComboBox.MaxDropDownItems = 20;
			this.ShortcutsComboBox.MenuMngWinCtrl = null;
			this.ShortcutsComboBox.Name = "ShortcutsComboBox";
			this.ShortcutsComboBox.SelectedIndexChanged += new System.EventHandler(this.ShortcutsComboBox_SelectedIndexChanged);
			// 
			// MenuShortcutsCompositeComboBox
			// 
			this.Controls.Add(this.RunShortcutButton);
			this.Controls.Add(this.ComboPanel);
			resources.ApplyResources(this, "$this");
			this.Name = "MenuShortcutsCompositeComboBox";
			this.ComboPanel.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        private System.ComponentModel.IContainer components;
    }

    partial class MenuShortcutsComboBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        
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
    }
}
