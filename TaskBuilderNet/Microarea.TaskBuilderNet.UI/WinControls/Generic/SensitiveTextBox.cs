using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public partial class SensitiveTextBox : System.Windows.Forms.UserControl
	{
		//=====================================================================
		private enum SensitiveTextBoxState { Init, Typing, TypingEnded };

		Rectangle imageRectangle = Rectangle.Empty;

		private System.Timers.Timer timer = new System.Timers.Timer();
		private double timerTickMillSecs = 750;
		private SensitiveTextBoxState currentState = SensitiveTextBoxState.Init;

		private Image searchImage;
		private Image clearSearchImage;
		private bool isClearSearchImageDisplayed;

		public event EventHandler<EventArgs> TypingStarted;
		public event EventHandler<SensitiveTextBoxEventArgs> TypingEnded;

		#region Public contructors

		//---------------------------------------------------------------------
		public SensitiveTextBox()
		{
            InitializeComponent();

			timer.Interval = timerTickMillSecs;
			timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);

            this.TextBox.BorderStyle = BorderStyle.None;
		}

		#endregion

		#region Protected methods

		//---------------------------------------------------------------------
		protected virtual void OnTypingStarted(object sender, EventArgs e)
		{
			if (TypingStarted != null)
				TypingStarted(this, e);

		}

		//---------------------------------------------------------------------
		protected virtual void OnTypingEnded(object sender, SensitiveTextBoxEventArgs e)
		{
			if (TypingEnded != null)
				TypingEnded(this, e);

		}

        //---------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            UpdateImageRectangle();
        }
        
        //---------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateImageRectangle();
        }

        //---------------------------------------------------------------------
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.ImagePictureBox.BackColor = this.TextBoxPanel.BackColor = this.TextBox.BackColor = this.BackColor;
        }

        //---------------------------------------------------------------------
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            this.ImagePictureBox.ForeColor = this.TextBoxPanel.ForeColor = this.TextBox.ForeColor = this.ForeColor;
        }

        //---------------------------------------------------------------------
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            this.TextBox.Font = this.Font;

            UpdateImageRectangle();
        }

		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (searchImage != null)
				Image = searchImage;

			UpdateImageRectangle();
		}

		#endregion

		#region Private methods

		//---------------------------------------------------------------------
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (IsHandleCreated)
				this.BeginInvoke(new TimerElapsedDelegate(Timer_ElapsedInternal));
		}

		private delegate void TimerElapsedDelegate();

		//---------------------------------------------------------------------
		private void Timer_ElapsedInternal()
		{
			timer.Stop();

			currentState = SensitiveTextBoxState.TypingEnded;

			OnTypingEnded(this, new SensitiveTextBoxEventArgs(this.Text));
		}

		//---------------------------------------------------------------------
		private void RestartTimer()
		{
			if (timer.Enabled)
				timer.Stop();

			timer.Start();
		}

        //---------------------------------------------------------------------
        private void UpdateImageRectangle()
        {
            if (this.ImagePictureBox.Image == null)
            {
                imageRectangle = Rectangle.Empty;
                this.ImagePictureBox.Location = Point.Empty;
                this.ImagePictureBox.Size = Size.Empty;
                this.ImagePictureBox.Visible = false;
                return;
            }

            Rectangle boxRectangle = this.ClientRectangle;

            imageRectangle = new Rectangle(
                                    boxRectangle.X + boxRectangle.Width - this.ImagePictureBox.Image.Width,
                                    boxRectangle.Y + (boxRectangle.Height - this.ImagePictureBox.Image.Height) / 2,
                                    this.ImagePictureBox.Image.Width,
                                    this.ImagePictureBox.Image.Height
                                    );

            this.ImagePictureBox.Location = imageRectangle.Location;
            this.ImagePictureBox.Size = imageRectangle.Size;
            this.ImagePictureBox.Visible = true;

            UpdateTextBoxSize();
        }
  
        //---------------------------------------------------------------------
        private void UpdateTextBoxSize()
        {
            this.TextBox.Location = new Point(1, (this.TextBoxPanel.Height - this.TextBox.Height) / 2);
            this.TextBox.Width = this.TextBoxPanel.Width - 2;
        }

		//---------------------------------------------------------------------
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (currentState != SensitiveTextBoxState.Typing)
			{
				currentState = SensitiveTextBoxState.Typing;

				OnTypingStarted(this, EventArgs.Empty);
			}

			RestartTimer();
		}

		#endregion

		#region Public properties

		//---------------------------------------------------------------------
		public double TimerTickMillSecs
		{
			get { return timerTickMillSecs; }
			set
			{
				if (timerTickMillSecs < 1)
					throw new ArgumentException("Invalide tick");

				timer.Stop();

				timerTickMillSecs = value;

				RestartTimer();
			}
		}

		//---------------------------------------------------------------------
		protected Image Image
		{
            get { return this.ImagePictureBox.Image; }
			set
			{
                this.ImagePictureBox.Image = value;

                UpdateImageRectangle();
			}
		}

		//---------------------------------------------------------------------
		public Image SearchImage
		{
			get { return this.searchImage; }
			set { this.searchImage = value; }
		}

		//---------------------------------------------------------------------
		public Image ClearSearchImage
		{
			get { return this.clearSearchImage; }
			set { this.clearSearchImage = value; }
		}

        //---------------------------------------------------------------------
        public new string Text
        {
            get { return this.TextBox.Text; }
            set { this.TextBox.Text = value; }
        }
        
        //---------------------------------------------------------------------
        public int MaxLength
        {
            get { return this.TextBox.MaxLength; }
            set { this.TextBox.MaxLength = value; }
        }

        //---------------------------------------------------------------------
        public bool ShortcutsEnabled
        {
            get { return this.TextBox.ShortcutsEnabled; }
            set { this.TextBox.ShortcutsEnabled = value; }
        }

		#endregion

		//---------------------------------------------------------------------
		private void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (TextBox.Text.Length > 0)
			{
				if (!isClearSearchImageDisplayed)
				{
					Image = clearSearchImage;
					isClearSearchImageDisplayed = true;
				}
			}
			else
			{
				if (isClearSearchImageDisplayed)
				{
					Image = searchImage;
					isClearSearchImageDisplayed = false;
				}
			}
		}

		//---------------------------------------------------------------------
		private void ImagePictureBox_Click(object sender, EventArgs e)
		{
			if (isClearSearchImageDisplayed)
			{
				TextBox.Text = String.Empty;
				OnTypingEnded(this, new SensitiveTextBoxEventArgs(String.Empty));
			}
		}
	}

	//=========================================================================
	public class SensitiveTextBoxEventArgs : EventArgs
	{
		private string sensitiveTextBoxText;

		//---------------------------------------------------------------------
		public string SensitiveTextBoxText
		{
			get { return sensitiveTextBoxText; }
		}

		//---------------------------------------------------------------------
		public SensitiveTextBoxEventArgs(string sensitiveTextBoxText)
		{
			this.sensitiveTextBoxText = sensitiveTextBoxText;
		}
	}
}
