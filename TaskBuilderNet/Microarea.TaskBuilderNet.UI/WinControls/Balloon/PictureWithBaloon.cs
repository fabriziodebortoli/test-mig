using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{

	//=========================================================================
	public partial class PictureWithBalloon : System.Windows.Forms.UserControl
	{
		private PictureBox pictureBox;
		private ToolTip toolTip;
		private Balloon balloon;
		private bool rememberToShowBalloon;
		private bool blick = true;

		public event EventHandler BalloonShowed;
		public event EventHandler BalloonClosed;
		public event EventHandler PictureShowed;
		public event EventHandler PictureHidden;

		private bool isBlinking;
		public System.Timers.Timer blinkTimer = new System.Timers.Timer();
		private const int blinkingPeriodMillSecs = 500;
		private const int maxBlinkTimes = 2000;
		private int blinkCount;

		private IContentManager contentManager;
		public IContentManager ContentManager {get {return contentManager;} set {contentManager = value;}}
		public bool Blick { get { return blick; } set { blick = value; } }
        bool immediateMode = false;
		//--------------------------------------------------------------------------- 
		public PictureWithBalloon()
		{
			InitializeComponent();

			pictureBox.Image = GetImage();
			pictureBox.Click += new EventHandler(PictureClick);
			pictureBox.MouseHover += new EventHandler(PictureMouseHover);

			blinkTimer.Interval = blinkingPeriodMillSecs;
			blinkTimer.Elapsed += new System.Timers.ElapsedEventHandler(BlinkTimer_Elapsed);
		}

		//--------------------------------------------------------------------------- 
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Form parentForm = this.ParentForm;
			if (parentForm != null)
				parentForm.Activated += new EventHandler(ParentForm_Activated);
		}

		//--------------------------------------------------------------------------- 
		private void ParentForm_Activated(object sender, EventArgs e)
		{
			if (rememberToShowBalloon)
			{
				rememberToShowBalloon = false;
				try
				{
					new Thread(new ThreadStart(new Action(() => ShowBalloonSafe()))).Start();
				}
				catch { }
			}
		}

		//--------------------------------------------------------------------------- 
		private void ShowBalloonSafe()
		{
			this.BeginInvoke(new Action(() => ShowBalloon()));
		}

        //object ticket = new object();
		//--------------------------------------------------------------------------- 
		private void ShowBalloon()
		{
            //lock (ticket)
            {
                if (balloonOpened)
                    return;

                balloonOpened = true;
      
                if (balloon != null && !balloon.IsDisposed)
                    balloon.VisibleChanged -= new EventHandler(Baloon_VisibleChanged);

                balloon = new Balloon(BalloonType.Info, pictureBox);
                balloon.BorderColor = Color.Transparent;
                balloon.TopMost = true;
                balloon.ContentManager = ContentManager;
                balloon.VisibleChanged += new EventHandler(Baloon_VisibleChanged);
                balloon.Closed += new EventHandler(Baloon_Closed);
                balloon.Show();
            }
		}

		public bool balloonOpened = false;
		//--------------------------------------------------------------------------- 
		private void PictureClick(object sender, EventArgs e)
		{
            //if (!balloonOpened)
            //{
            //    balloonOpened = true;
                ShowBalloonSafe();
            //}
		}

		//--------------------------------------------------------------------------- 
		private void PictureMouseHover(object sender, EventArgs e)
		{
			StopBlinking();
			pictureBox.Visible = true;
		}

		//--------------------------------------------------------------------------- 
		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
			PictureMouseHover(this, e);
		}

		//--------------------------------------------------------------------------- 
		private void Baloon_VisibleChanged(object sender, EventArgs e)
		{
			if (balloon.Visible)
				OnBaloonShowed(e);
			else
				OnBaloonClosed(e);
		}

		//--------------------------------------------------------------------------- 
		private void Baloon_Closed(object sender, EventArgs e)
		{
			balloonOpened = false;
			OnBaloonClosed(e);
		}

		//--------------------------------------------------------------------------- 
		private void BlinkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsDisposed)
                return;
			if (blick)
				this.Invoke(new EventHandler(TogglePictureVisibility), new object[] { sender, e });
		}

		//--------------------------------------------------------------------------- 
		private void TogglePictureVisibility(object sender, EventArgs e)
		{
			// Non sincronizzo perchè, essendoci in questo metodo un'operazione
			// di interfaccia, è necessario usare il trucco dell'Invoke e quindi
			// è sempre e solo il thread di interfaccia che accede a blinkCount.
			pictureBox.Visible = !pictureBox.Visible;

			blinkCount++;
			if (blinkCount > maxBlinkTimes)
			{
				blinkCount = 0;
                ShowBalloonSafe();
			}
		}

		//--------------------------------------------------------------------------- 
		protected virtual void OnBaloonShowed(EventArgs e)
		{
			StopBlinking();
			pictureBox.Visible = true;
            
			if (BalloonShowed != null)
				BalloonShowed(this, e);

		}

		//--------------------------------------------------------------------------- 
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!Visible)
				StopBlinking();
		}

		//--------------------------------------------------------------------------- 
		protected virtual void OnBaloonClosed(EventArgs e)
		{
			if (BalloonClosed != null)
				BalloonClosed(this, e);

			if (balloon != null && !balloon.IsDisposed)
			{
				balloon.VisibleChanged	-= new EventHandler(Baloon_VisibleChanged);
				balloon.Closed			-= new EventHandler(Baloon_Closed);
			}
			this.Visible = isBlinking = false;
            DisposeNotifyIcon();
		}

		//--------------------------------------------------------------------------- 
		protected virtual void OnPictureShowed(EventArgs e)
		{
			if (PictureShowed != null)
				PictureShowed(this, e);
		}

		//--------------------------------------------------------------------------- 
		protected virtual void OnPictureHidden(EventArgs e)
		{
			if (PictureHidden != null)
				PictureHidden(this, e);
		}
        //--------------------------------------------------------------------------- 
        public void ShowImmediate(string notifyIconTitle = null)
        {
            if (balloonOpened) return;
            immediateMode = true;
            Show(string.Empty, notifyIconTitle);
            rememberToShowBalloon = false;
            blinkTimer.Stop();
            System.Diagnostics.Debug.WriteLine("SHOW BALLOON IMMEDIATE!*****************************");
            PictureClick(null, null);// ShowBalloon();
            immediateMode = false;
        }
		//--------------------------------------------------------------------------- 
		public void Close()
		{
			blinkTimer.Stop();
			if (balloon != null)
				balloon.Close();
		}

		//--------------------------------------------------------------------------- 
        public void Show(string tooltip, string notifyiconTitle = null)
		{
            if (balloonOpened) return;

			toolTip.SetToolTip(pictureBox, tooltip);
			base.Show();
           

			if (balloon == null || balloon.IsDisposed || !balloon.Visible)
			{
				pictureBox.Visible = true;

                if (!immediateMode)
                //  return; 
                // Lampeggio della taskBar del parent se il parent non ha il fuoco,
                // altrimenti mostra il balloon direttamente
                {
                    try
                    {
                        Form parent = this.ParentForm;
                        if (parent != null)
                        {
                            bool needToFlash = SafeNativeMethods.FlashWindowEx(parent.Handle);
                            if (!needToFlash)
                            {
                                if (!isBlinking)
                                    StartBlinking();

                            }
                            else
                                rememberToShowBalloon = true;

                        }
                    }
                    catch { }
                }
			}
            NotifyIcon(tooltip, notifyiconTitle);
		}

        //--------------------------------------------------------------------------- 
        private void NotifyIcon(string tooltip, string title)
        {
            CreateNotifyIcon1();

            notifyIcon1.BalloonTipClicked += new EventHandler(notifyIcon1_BalloonTipClicked);
            notifyIcon1.Click += new EventHandler(notifyIcon1_Click);

            notifyIcon1.Text = tooltip;
            notifyIcon1.Visible = true;
            if (String.IsNullOrWhiteSpace(tooltip)) tooltip = WinControlsStrings.NewMessages;//immediate
            if (String.IsNullOrWhiteSpace(title)) title = WinControlsStrings.NewMessagesTitle;
                notifyIcon1.ShowBalloonTip(10000, title, tooltip, ToolTipIcon.Info);

        }

        //--------------------------------------------------------------------------- 
        void notifyIcon1_Click(object sender, EventArgs e)
        {
            
            ShowImmediate();
            //DisposeNotifyIcon();
        }

        //--------------------------------------------------------------------------- 
        void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
           
            ShowImmediate(); 
           // DisposeNotifyIcon();
        }

        
        //--------------------------------------------------------------------------- 
        void CreateNotifyIcon1()
        {

            if (notifyIcon1 == null)
            {
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.Megaphone.ico");

                if (s != null)
                {
                    this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
                    this.notifyIcon1.Icon = new System.Drawing.Icon(s);
                }

            }
        }
        //--------------------------------------------------------------------------- 
        private void DisposeNotifyIcon()
        {
            if (notifyIcon1 == null) return;

            notifyIcon1.Text = null;
            notifyIcon1.Visible = false;
            notifyIcon1.Icon = null;
            notifyIcon1.BalloonTipClicked -= new EventHandler(notifyIcon1_BalloonTipClicked);
            notifyIcon1.Click -= new EventHandler(notifyIcon1_Click);
           
            notifyIcon1.Dispose();
            notifyIcon1 = null;
        }

		//--------------------------------------------------------------------------- 
		private Bitmap GetImage()
		{
			using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.mail.png"))
			{
				if (iconStream == null) 
					return null;

				System.Drawing.Bitmap icon = new Bitmap(iconStream);
				return icon; 
			}
		}

		//--------------------------------------------------------------------------- 
		private void StartBlinking()
		{
			if (IsDisposed)
				return;
	
			if (!isBlinking)
				isBlinking = blinkTimer.Enabled = true;
		}

		//--------------------------------------------------------------------------- 
		private void StopBlinking()
		{
			if (IsDisposed)
				return;

			if (isBlinking)
			{
				isBlinking = blinkTimer.Enabled = false;
			}
		}

		//--------------------------------------------------------------------------- 
		public void SetImage(System.Drawing.Bitmap icon)
		{
			pictureBox.Image = icon;
		}
	}
}
