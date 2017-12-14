using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public partial class AnimatedForm : System.Windows.Forms.Form
	{
		#region Private data members

		private const double minimumOpacity = 0.4;
		private const double maximumOpacity = 0.99;
		private const double opacityStep = 0.15;

		private IAnimationPath animationPath;
		private IAnimationSize animationSize;

		private bool isAnimationRunning;

		#endregion

		#region Properties

		//---------------------------------------------------------------------
		public IAnimationPath AnimationPath
		{
			get { return animationPath; }
			set 
            {
                if (IAnimationPath.Equals(animationPath, value))
                    return;

                animationPath = value; 
            }
		}

		//---------------------------------------------------------------------
		public IAnimationSize AnimationSize
		{
			get { return animationSize; }
			set { animationSize = value; }
		}

        //---------------------------------------------------------------------
        protected bool IsAnimationRunning { get { return isAnimationRunning; } }

		#endregion

		#region Contructors

		//---------------------------------------------------------------------
		public AnimatedForm()
			: this(null, null)
		{ }

		//---------------------------------------------------------------------
		public AnimatedForm(IAnimationPath animationPath, IAnimationSize animationSize)
		{
			InitializeComponent();

			this.SuspendLayout();
			this.Opacity = maximumOpacity;
			this.ResumeLayout(false);

			if (animationPath != null)
				this.animationPath = animationPath;

			if (animationSize != null)
				this.animationSize = animationSize;
		}

		#endregion

		#region Show and Hide methods overrides

		//---------------------------------------------------------------------
		public new void Show()
		{
            Show(null);
		}

		//---------------------------------------------------------------------
		public new void Show(IWin32Window owner)
		{
            Thread animationThread = null;
			if (animationPath != null && animationSize != null)
			{
			    isAnimationRunning = true;

                animationThread = new Thread(new ThreadStart(this.PerformShowAnimation));
                
                animationThread.Start();

			    while (isAnimationRunning)
				    Thread.Sleep(40);//Sleep del thread principale ma qui non ho ancora mostrato nulla per cui non blocco l'interfaccia.
            }

            if (animationThread != null)
                animationThread.Join();

            if (owner != null)
                base.Show(owner);
            else
                base.Show();
        }

		//---------------------------------------------------------------------
		public new DialogResult ShowDialog()
		{
			if (animationPath == null || animationSize == null)
				return base.ShowDialog();

            base.StartPosition = FormStartPosition.CenterScreen;
            
            isAnimationRunning = true;
			new Thread(new ThreadStart(this.PerformShowAnimation)).Start();

			while (isAnimationRunning)
				Thread.Sleep(40);//Sleep del thread principale ma qui non ho ancora mostrato nulla per cui non blocco l'interfaccia.

			return base.ShowDialog();
		}
        
		//---------------------------------------------------------------------
		public new DialogResult ShowDialog(IWin32Window owner)
		{
            if (animationPath == null || animationSize == null)
            {
                base.StartPosition = (owner != null) ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                return base.ShowDialog(owner);
            }
            base.StartPosition = FormStartPosition.Manual;

			isAnimationRunning = true;
			new Thread(new ThreadStart(this.PerformShowAnimation)).Start();

			while (isAnimationRunning)
				Thread.Sleep(40);//Sleep del thread principale ma qui non ho ancora mostrato nulla per cui non blocco l'interfaccia.

			return base.ShowDialog(owner);
		}

		//---------------------------------------------------------------------
		public new void Hide()
		{
			base.Hide();

			if (animationPath == null || animationSize == null)
				return;

			isAnimationRunning = true;
			new Thread(new ThreadStart(this.PerformHideAnimation)).Start();
		}

		#endregion

		//---------------------------------------------------------------------
		protected override void OnClosed(EventArgs e)
		{
		    base.OnClosed(e);

		    if (animationPath == null || animationSize == null)
		        return;

		    isAnimationRunning = true;
		    new Thread(new ThreadStart(this.PerformHideAnimation)).Start();
		}

		#region Opacity management

		//---------------------------------------------------------------------
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

            Keys modKey = Control.ModifierKeys;
            if ((modKey & Keys.Control) == Keys.Control)
            {
				double currentOpacity = this.Opacity;
				double desiredOpacity = 0.0;
				if (e.Delta < 0 && currentOpacity > minimumOpacity)
				{
					desiredOpacity = currentOpacity - opacityStep;
					this.Opacity = (desiredOpacity < minimumOpacity) ? minimumOpacity : desiredOpacity;
				}
				else if (e.Delta > 0 && currentOpacity < maximumOpacity)
				{
					desiredOpacity = currentOpacity + opacityStep;
					this.Opacity = (desiredOpacity > maximumOpacity) ? maximumOpacity : desiredOpacity;
				}
			}
		}

		//---------------------------------------------------------------------
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

            Keys modKey = Control.ModifierKeys;
            if ((modKey & Keys.Control) != Keys.Control)
                this.Opacity = maximumOpacity;
		}

		#endregion

		#region Animation worker methods

		//---------------------------------------------------------------------
		private void PerformShowAnimation()
		{
			Point[] animationPoints = this.animationPath.GetPathPoints();
			int interval = this.animationPath.MillSecsBetweenPoints;

			Size[] sizes = this.animationSize.GetSizes();

			// inizio animazione.
            Rectangle frameRect = Rectangle.Empty;
			for (int i = 0; i < animationPoints.Length; i++)
			{
                if (!frameRect.IsEmpty)
				{
					ControlPaint.DrawReversibleFrame(
                        frameRect, Color.Transparent, FrameStyle.Thick
						);
				}

                frameRect = new Rectangle(animationPoints[i], sizes[i]);

            	ControlPaint.DrawReversibleFrame(
                    frameRect, Color.Transparent, FrameStyle.Thick
					);

				Thread.Sleep(interval);
			}
            if (!frameRect.IsEmpty)
                ControlPaint.DrawReversibleFrame(
                    frameRect, 
                    Color.Transparent,
                    FrameStyle.Thick
					);
			// fine animazione
			isAnimationRunning = false;

            SetLocation(animationPoints[animationPoints.Length - 1]);
        }

        private delegate void SetLocationDelegate(Point newLocation);
        //---------------------------------------------------------------------
        public void SetLocation(Point newLocation)
        {
            if (this.InvokeRequired)
                this.Invoke(new SetLocationDelegate(SetLocation), new Object[] { newLocation });
            else
                this.Location = newLocation;
        }

		//---------------------------------------------------------------------
		private void PerformHideAnimation()
		{
			this.animationPath.EndPoint = this.Location;

			Point[] animationPoints = this.animationPath.GetPathPoints();
			int interval = this.animationPath.MillSecsBetweenPoints;

			Size[] sizes = this.animationSize.GetSizes();

			// inizio animazione.
            Rectangle frameRect = Rectangle.Empty;
            for (int i = animationPoints.Length - 1; i > -1; --i)
			{
                if (!frameRect.IsEmpty)
                {
					ControlPaint.DrawReversibleFrame(
                        frameRect, Color.Black, FrameStyle.Thick
						);
				}

                frameRect = new Rectangle(animationPoints[i], sizes[i]);
				ControlPaint.DrawReversibleFrame(
                    frameRect, Color.Black, FrameStyle.Thick
					);

				Thread.Sleep(interval);
			}

            if (!frameRect.IsEmpty)
                ControlPaint.DrawReversibleFrame(
                    frameRect, Color.Black, FrameStyle.Thick
					);
			// fine animazione
			isAnimationRunning = false;
		}

		#endregion
	}
}
