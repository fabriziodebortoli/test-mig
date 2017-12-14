using System.Text.RegularExpressions;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using Microarea.TaskBuilderNet.UI.Properties;

namespace Microarea.TaskBuilderNet.UI.WinControls.Generic
{
	//================================================================================
	public partial class MessageWindow : Form
	{
        public enum ImageType { Info, Warning, Error };

		//--------------------------------------------------------------------------------
        private MessageWindow(string message, MessageBoxButtons buttons, ImageType imageType)
		{
			InitializeComponent();
			this.labelMessage.Text = message;
			switch (buttons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					btn1.Text = Resources.Abort;
					btn1.DialogResult = DialogResult.Abort;
					btn2.Text = Resources.Retry;
					btn2.DialogResult = DialogResult.Retry;
					btn3.Text = Resources.Ignore;
					btn3.DialogResult = DialogResult.Ignore;
					break;
				case MessageBoxButtons.OK:
					btn1.Visible = false;
					btn2.Visible = false;
					btn3.Text = Resources.Ok;
					btn3.DialogResult = DialogResult.OK;
					break;
				case MessageBoxButtons.OKCancel:
					btn1.Visible = false;
					btn2.Text = Resources.Ok;
					btn2.DialogResult = DialogResult.OK;
					btn3.Text = Resources.Cancel;
					btn3.DialogResult = DialogResult.Cancel;
					break;
				case MessageBoxButtons.RetryCancel:
					btn1.Visible = false;
					btn2.Text = Resources.Retry;
					btn2.DialogResult = DialogResult.Retry;
					btn3.Text = Resources.Cancel;
					btn3.DialogResult = DialogResult.Cancel;
					break;
				case MessageBoxButtons.YesNo:
					btn1.Visible = false;
					btn2.Text = Resources.Yes;
					btn2.DialogResult = DialogResult.Yes;
					btn3.Text = Resources.No;
					btn3.DialogResult = DialogResult.No;
					break;
				case MessageBoxButtons.YesNoCancel:
					btn1.Text = Resources.Yes;
					btn1.DialogResult = DialogResult.Yes;
					btn2.Text = Resources.No;
					btn2.DialogResult = DialogResult.No;
					btn3.Text = Resources.Cancel;
					btn3.DialogResult = DialogResult.Cancel;
					break;
				default:
					break;
			}

            switch (imageType)
            {
                case ImageType.Error: this.pictureBox1.Image = Microarea.TaskBuilderNet.UI.Properties.Resources.Error; break;
                case ImageType.Warning: this.pictureBox1.Image = Microarea.TaskBuilderNet.UI.Properties.Resources.Warning; break;
                default: break;//default già disegnata
            }
		}

		//--------------------------------------------------------------------------------
		public static DialogResult ShowDialog(string message, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK)
		{
            return ShowDialog(null, message, caption, buttons);
		}

		//--------------------------------------------------------------------------------
        public static DialogResult ShowDialog(IWin32Window owner, string message, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, ImageType imageType = ImageType.Info)
		{
			DialogResult ret;
			if (IsHiddenMessage(message, out ret))
				return ret;

            MessageWindow window = new MessageWindow(message, buttons, imageType);
			window.Text = caption;
			using (SafeThreadCallContext context = new SafeThreadCallContext())
				ret = window.ShowDialog(owner);

			if (window.chkDontAskAgain.Checked)
				AddHiddenMessage(message, ret);
			return ret;
		}

		//--------------------------------------------------------------------------------
		private static void AddHiddenMessage(string message, DialogResult ret)
		{
			ApplicationCache cache = ApplicationCache.Load();
			ShownMessages msgs = cache.GetObject<ShownMessages>();
			if (msgs == null)
			{
				msgs = new ShownMessages();
				cache.PutObject<ShownMessages>(msgs);
			}
			ShownMessage m = new ShownMessage();
			m.Message = message;
			m.Result = ret;
			msgs.Add(m);
			cache.Save();
		}

		//--------------------------------------------------------------------------------
		private static bool IsHiddenMessage(string message, out DialogResult ret)
		{
			ret = DialogResult.None;
			ApplicationCache cache = ApplicationCache.Load();
			ShownMessages msgs = cache.GetObject<ShownMessages>();
			if (msgs == null)
				return false;
			
			foreach (ShownMessage msg in msgs)
			{
				if (Regex.Replace(msg.Message, "\\s+", " ") == Regex.Replace(message, "\\s+", " "))
				{
					ret = msg.Result;
					return true;
				}
			}
			return false;
		}
	}
}
