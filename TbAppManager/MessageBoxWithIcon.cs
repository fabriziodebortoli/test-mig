using System;
using System.Windows.Forms;

namespace Microarea.MenuManager
{
    public partial class MessageBoxWithIcon : Form
    {
        public enum IconType { Error, Warning, OK };
        public MessageBoxWithIcon(IconType icon, string text, string title)
        {
            InitializeComponent();
            this.Text = title;
            PbError.Visible = (icon == IconType.Error);
            PbWarning.Visible = (icon == IconType.Warning);
            PbOK.Visible = (icon == IconType.OK);
            LblInfo.Text = text;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
