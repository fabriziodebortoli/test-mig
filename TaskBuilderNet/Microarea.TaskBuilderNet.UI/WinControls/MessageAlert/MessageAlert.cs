using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    #region Enums
    //---------------------------------------------------------------------
    public enum MessageAlertType
    {
        Undefined = 0x0000,
        Information = 0x0001,
        Warning = 0x0002,
        Error = 0x0003,
        Question = 0x0004,
        HelpAndSupport = 0x0005,
        Hand = 0x0006
    }

    //---------------------------------------------------------------------
    [Flags]
    public enum MessageAlertButtons
    {
		None = 0,
        OK = 1,
        Yes = 2,
        No = 4,
        Retry = 8,
        Cancel = 16,
        OKCancel = OK | Cancel,
        YesNo = Yes | No,
        YesNoCancel = Yes | No,
        RetryCancel = Retry | Cancel,
        All = OK | Yes | No | Retry | Cancel
    }

    #endregion

    //=========================================================================
    public partial class MessageAlert : System.Windows.Forms.Form 
    {
        #region DataMember 

        private string  message = string.Empty;

        private const int buttonOffset = 15;
        private const int offset = 15;

        private MessageAlertButtons buttons = MessageAlertButtons.None;
        #endregion

        #region Properties

        //---------------------------------------------------------------------
        public string Message 
        {
            get { return (this.MessageLabel != null) ? this.MessageLabel.Text : String.Empty; } 
            set 
            {
                this.MessageLabel.Text = value;

                Graphics g = this.CreateGraphics();

                StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
                sf.Trimming = StringTrimming.None;

                SizeF msgSizeF = g.MeasureString(value, this.MessageLabel.Font, this.MessageLabel.Width, sf);

                g.Dispose();

                int minMsgHeight = (int)Math.Ceiling(msgSizeF.Height);

                if (minMsgHeight > this.MessageLabel.Height)
                    this.Height += (minMsgHeight - this.MessageLabel.Height);
            } 
        }
      
        //---------------------------------------------------------------------
        protected Image Image { get { return this.AlertPictureBox.Image; } set { this.AlertPictureBox.Image = value; } }
        
        //---------------------------------------------------------------------
        public MessageAlertButtons MessageButtons 
        { 
            get { return buttons; } 
            set 
            {
                if (buttons == value)
                    return;

                buttons = value;

                if ((buttons & MessageAlertButtons.Yes) != MessageAlertButtons.Yes)
                    RemoveButton(YesButton);

                if ((buttons & MessageAlertButtons.No) != MessageAlertButtons.No)
                    RemoveButton(NoButton);

                if ((buttons & MessageAlertButtons.OK) != MessageAlertButtons.OK)
                    RemoveButton(Okbtn);

                if ((buttons & MessageAlertButtons.Retry) != MessageAlertButtons.Retry)
                    RemoveButton(RetryButton);

                if ((buttons & MessageAlertButtons.Cancel) != MessageAlertButtons.Cancel)
                    RemoveButton(UndoButton);

                AlignButtons();
            } 
        }

        #endregion
        
        #region Constructors
        //---------------------------------------------------------------------
        public MessageAlert(string message, MessageAlertButtons aButtonsCombination, Image anImage)
         {
            InitializeComponent();
            
            this.Message = message;
            
            CenterLabel();

            this.MessageButtons = aButtonsCombination;

            this.Image = anImage;
        }

        //---------------------------------------------------------------------
        public MessageAlert(string message, MessageAlertButtons aButtonsCombination, MessageAlertType aMessageAlertImage)
            : this(message, aButtonsCombination, GetAlertImage(aMessageAlertImage))
        {
        }

        //---------------------------------------------------------------------
        public MessageAlert()
            : this(String.Empty, MessageAlertButtons.OK, MessageAlertType.Undefined)
        {
        }

        
        //---------------------------------------------------------------------
        private void AlignButtons()
        {
            int width = 0;

            foreach (Control control in Controls)
            {
                if (control is Button)
                    width += control.Width + buttonOffset;
            }

            width -= buttonOffset;

            int startPointX = (this.Size.Width - width) / 2;

            foreach (Control control in Controls)
            {
                if (control is Button)
                {
                    control.Location = new Point(startPointX, control.Location.Y);
                    startPointX += control.Width + buttonOffset;
                }
            }

        }

        //---------------------------------------------------------------------
        private void CenterLabel()
        {
            if (this.MessageLabel == null || (this.AlertPictureBox != null && this.AlertPictureBox.Width >= (this.Width - offset)))
                return;

            int emptyReagionWith = this.Width - offset - this.AlertPictureBox.Width;

            int startX = (emptyReagionWith - this.MessageLabel.Width) / 2;
            int startY = this.MessageLabel.Location.Y;
         
            this.MessageLabel.Location = new Point(startX, startY);
        }

        #endregion

        //---------------------------------------------------------------------
        private void RemoveButton(Button button)
        {
            if (Controls.Contains(button))
                Controls.Remove(button);
        }

        //---------------------------------------------------------------------
        public static Image GetAlertImage(MessageAlertType aMessageAlertImage)
        {
            Stream imageStream = null;

            switch (aMessageAlertImage)
            {
                case MessageAlertType.Information:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.Info.png");
                        break;
                    }

                case MessageAlertType.Error:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.Error.png");
                        break;
                    }

                case MessageAlertType.Warning:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.Warning.png");
                        break;
                    }


                case MessageAlertType.Question:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.Question.png");
                        break;
                    }

                case MessageAlertType.HelpAndSupport:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.HelpAndSupport.png");
                        break;
                    }

                case MessageAlertType.Hand:
                    {
                        imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Images.Hand.png");
                        break;
                    }

                default:
                    break;
            }
            return (imageStream != null) ? Image.FromStream(imageStream) : null;
        }

        //---------------------------------------------------------------------
        public static Icon GetAlertIcon(MessageAlertType aMessageAlertImage)
        {
            Stream iconStream = null;

            switch (aMessageAlertImage)
            {
                case MessageAlertType.Information:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.Info.ico");
                        break;
                    }

                case MessageAlertType.Error:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.Error.ico");
                        break;
                    }

                case MessageAlertType.Warning:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.Warning.ico");
                        break;
                    }


                case MessageAlertType.Question:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.Question.ico");
                        break;
                    }

                case MessageAlertType.HelpAndSupport:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.HelpAndSupport.ico");
                        break;
                    }

                case MessageAlertType.Hand:
                    {
                        iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.MessageAlert.Icons.Hand.ico");
                        break;
                    }

                default:
                    break;
            }
            return (iconStream != null) ? new Icon(iconStream) : null;
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons, 
            Image anImage,
            Icon anIcon
            )
        {
            MessageAlert messageAlert = new MessageAlert(message, buttons, anImage);
            messageAlert.Icon = anIcon;
           
            return messageAlert.ShowDialog(owner);
        }
  
        //---------------------------------------------------------------------
        public static DialogResult ShowMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons,
            MessageAlertType aMessageAlertImage
            )
        {
            return ShowMessageAlert(owner, message, buttons, GetAlertImage(aMessageAlertImage), GetAlertIcon(aMessageAlertImage));
        }
        
        //---------------------------------------------------------------------
        public static DialogResult ShowInformationMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.Information);
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowWarningMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.Warning);
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowErrorMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.Error);
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowQuestionMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.Question);
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowHelpAndSupportMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.HelpAndSupport);
        }

        //---------------------------------------------------------------------
        public static DialogResult ShowHandMessageAlert(
            IWin32Window owner,
            string message,
            MessageAlertButtons buttons
            )
        {
            return ShowMessageAlert(owner, message, buttons, MessageAlertType.Hand);
        }
    }
}