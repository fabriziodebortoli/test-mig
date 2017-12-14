using System;
using System.Drawing;
using System.Windows.Forms;

namespace HttpNamespaceManager.UI
{
    public partial class InputBox : Form
    {
		public delegate bool OnCheckMethod(string val);
		public OnCheckMethod OnCheck; 
        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string title, string prompt, OnCheckMethod onCheck)
        {
            InitializeComponent();
            this.Text = title;
            this.labelPrompt.Text = prompt;
			this.OnCheck = onCheck;
            this.Size = new Size(Math.Max(this.labelPrompt.Width + 31, 290), this.labelPrompt.Height + 103);
        }

        public static DialogResult Show(string title, string prompt, OnCheckMethod onCheck, out string result)
        {
            InputBox input = new InputBox(title, prompt, onCheck);
            DialogResult retval = input.ShowDialog();
            result = input.textInput.Text;
            return retval;
        }
		private void InputBox_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = !Check();
		}

		private bool Check()
		{
			if (OnCheck == null)
				return true;
			return OnCheck(textInput.Text);
			
		}
    }
}