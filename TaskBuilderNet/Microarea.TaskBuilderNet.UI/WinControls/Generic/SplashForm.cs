using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for SplashForm.
	/// </summary>
	public partial class SplashForm : System.Windows.Forms.Form
	{
		private System.Drawing.Bitmap	splashImage = null;
		private System.Drawing.Image	animatedImage = null;
		private System.Drawing.Image	animationBackgroundImage = null;
		
		/// <summary>
		/// Graphics object used to render splash screen.
		/// Cached for performance.
		/// </summary>
		private Graphics splashGraphics = null;

		/// <summary>
		/// The region of the screen filled by the background.
		/// Cached for performance.
		/// </summary>
		private Rectangle splashRegion = new Rectangle(0,0,0,0);

		/// <summary>
		/// The source region of the background draw.
		/// Cached for performance.
		/// </summary>
		private Rectangle splashSourceRect = new Rectangle(0,0,0,0);

		/// <summary>
		/// Image attributes specifying transperancy color.
		/// Cached for performance.
		/// </summary>
		private ImageAttributes splashImageAttributes = new ImageAttributes();

		/// <summary>
		/// Timer used to update the screen at regular intervals.
		/// </summary>
		private System.Threading.Timer splashTimer = null;

		/// <summary>
		/// Source region for redrawing the background.
		/// Cached for performance.
		/// </summary>
		private Rectangle redrawSourceRect = new Rectangle(0,0,0,0);

		/// <summary>
		/// The number of updates that the splash screen timer triggered.
		/// </summary>
		private int updatesCount = 0;

		/// <summary>
		/// Time between screen updates (ms)
		/// </summary>
		private int timerInterval = 0;
		private AnimationPanel AnimationPanel;

		private bool killingMe = false;

		/// <summary>
		/// Constructor for the splash screen form.  Creates the background
		/// and animation Bitmap objects
		/// </summary>
		/// <param name="timerInterval">Length of time between screen updates (ms)</param>
		public SplashForm(int aTimerInterval, System.Drawing.Bitmap	aSplashImage)
		{
			// Store the timer interval
			timerInterval = aTimerInterval;

			splashImage = aSplashImage;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public SplashForm(int aTimerInterval) : this (aTimerInterval, null)
		{
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing (e);

			AnimationPanel.StopAnimation();
		}

		/// <summary>
		/// Return the amount of time the splash screen has been displayed in
		/// milliseconds.  This is based on the number of times the timer has
		/// triggered and the interval of the timer.  This is not completely
		/// accurate but good enough for the purposes of this function.
		/// </summary>
		/// <returns></returns>
		public int GetUpMilliseconds()
		{
			return updatesCount * timerInterval;
		}

		/// The form is ready to be displayed so initialize all of the splash screen data 
		/// and draw the first frame.
		protected override void OnLoad(EventArgs e)
		{
			this.Visible = false;

			base.OnLoad (e);

			// Make the form full screen
			//this.Text = "";
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ControlBox = false;
			this.FormBorderStyle = FormBorderStyle.None;
			this.WindowState = FormWindowState.Maximized;
			this.Menu = null;

			// Center the splash screen background
			if (splashImage != null)
			{
				splashRegion.X = (Screen.PrimaryScreen.Bounds.Width - splashImage.Width) / 2;
				splashRegion.Y = (Screen.PrimaryScreen.Bounds.Height - splashImage.Height) / 2;
				splashRegion.Width = splashImage.Width;
				splashRegion.Height = splashImage.Height;
			}
			else
			{
				splashRegion.X = (Screen.PrimaryScreen.Bounds.Width - this.Width) / 2;
				splashRegion.Y = (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2;
				splashRegion.Width = this.Width;
				splashRegion.Height = this.Height;
			}

			// Set up the rectangle from which the background will be drawn
			splashSourceRect.X = 0;
			splashSourceRect.Y = 0;
			if (splashImage != null)
			{
				splashSourceRect.Width = splashImage.Width;
				splashSourceRect.Height = splashImage.Height;
			}
			else
			{
				splashSourceRect.Width = this.Width;
				splashSourceRect.Height = this.Height;
			}

			this.Show();

			// Create the graphics object and set its clipping region
			splashGraphics = CreateGraphics();

            splashGraphics.Clip = new Region(splashRegion);

			// Draw the screen once with the full background update
			// No need to use Application.DoEvents to force OnPaint.
			Draw();

			if (animatedImage != null)
			{	
				AnimationPanel.Dock = DockStyle.None;
				AnimationPanel.Location = new Point(splashRegion.X, splashRegion.Y);
				AnimationPanel.Size = new Size(splashRegion.Width, splashRegion.Height);
				
				AnimationPanel.BackColor = Color.Transparent;
				AnimationPanel.BorderStyle = BorderStyle.None;
				
				AnimationPanel.SizeMode = AnimatedImageSizeMode.AutoSize;
				AnimationPanel.AnimatedImage = animatedImage;
				
				if (animationBackgroundImage != null)
					AnimationPanel.BackgroundImage = animationBackgroundImage;

				AnimationPanel.BringToFront();

				AnimationPanel.Visible = true;
				AnimationPanel.StartAnimation();
			}
			else
				AnimationPanel.Visible = false;

			// Start a timer that will call Draw every 200 ms
			System.Threading.TimerCallback splashDelegate = new System.Threading.TimerCallback(this.Draw);
			this.splashTimer = new System.Threading.Timer(splashDelegate, null, timerInterval, timerInterval);

		}

		/// <summary>
		/// If a paint event is generated then redraw the splash screen
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			Draw();
		}

		/// <summary>
		/// Do not respond to paint background events
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaintBackground(PaintEventArgs e){}

		/// <summary>
		/// Kill this form
		/// </summary>
		/// <param name="o">Not used</param>
		/// <param name="e">Not used</param>
		public void KillMe(object o, EventArgs e)
		{
			killingMe = true;

			// Stop the timer first so there are no racing issues
			if (splashTimer != null)
			{
				splashTimer.Dispose();
				splashTimer = null;
			}

			if (splashGraphics != null)
			{
				splashGraphics.Dispose();
				splashGraphics = null;
			}

			// Shut down the form
			this.Close();
		}

		/// <summary>
		/// Draw the screen.  This is the callback for the timer
		/// </summary>
		/// <param name="state">Not used - timer data</param>
		protected void Draw(Object state)
		{
			updatesCount++;

			Draw();
		}

		/// <summary>
		/// Draw the screen
		/// </summary>
		/// <param name="bFullImage">true if the entire background should be updated</param>
		/// <param name="bUpdateAnim">true if the animation position and cell should be updated</param>
		protected void Draw()
		{
			if (killingMe || splashGraphics == null || splashImage == null)
				return;

			// The lock keyword marks a statement block as a critical section by obtaining the 
			// mutual-exclusion lock for a given object, executing a statement, and then releasing
			// the lock.
			// lock ensures that one thread does not enter a critical section while another thread
			// is in the critical section of code. If another thread attempts to enter a locked code, 
			// it will wait (block) until the object is released.

			lock (this) // Make sure it is safe to access the form
			{
				try
				{
					splashGraphics.DrawImage(splashImage, splashRegion, splashSourceRect, GraphicsUnit.Pixel);
				}
				catch(Exception)
				{
				}
			}
		}

		#region SplashForm public properties
		
		//---------------------------------------------------------------------------
		public System.Drawing.Image	AnimatedImage 
		{
			get { return animatedImage; }
			set 
			{
				bool restartAnimation = false;
				if (AnimationPanel.AnimationInProgress)
				{
					AnimationPanel.StopAnimation();
					restartAnimation = true;
				}
				
				animatedImage = value; 
			
				AnimationPanel.AnimatedImage = animatedImage;
				
				if (animatedImage != null)
				{
					AnimationPanel.BringToFront();

					AnimationPanel.Visible = true;
					if (restartAnimation)
						AnimationPanel.StartAnimation();
				}
			}
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Image	AnimationBackgroundImage
		{
			get { return animationBackgroundImage; }
			set 
			{
				bool restartAnimation = false;
				if (AnimationPanel.AnimationInProgress)
				{
					AnimationPanel.StopAnimation();
					restartAnimation = true;
				}
				
				animationBackgroundImage = value; 
		
				AnimationPanel.BackgroundImage = animationBackgroundImage;

				if (animatedImage != null)
				{
					AnimationPanel.BringToFront();

					AnimationPanel.Visible = true;
					if (restartAnimation)
						AnimationPanel.StartAnimation();
				}
			}
		}

		#endregion // SplashForm public properties
	}
}
