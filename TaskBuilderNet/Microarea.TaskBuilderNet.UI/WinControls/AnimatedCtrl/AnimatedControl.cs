using System;
using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls.AnimatedControls
{
	/// <summary>
	/// Summary description for AnimatedControl.
	/// </summary>
	public partial  class AnimatedControl : System.Windows.Forms.UserControl
	{
		public AnimatedControl()
		{
			InitializeComponent();
		}

		public AnimatedControl(Bitmap image)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.AnimatedImage = image;
		}

		//Create a Bitmpap Object.
		private	Bitmap	animatedImage		= null;
		public 	Bitmap	AnimatedImage
		{
			get
			{
				return animatedImage;
			}
			set
			{
				animatedImage = value;
			}
		}

		private bool	currentlyAnimating	= false;
		
		//This method begins the animation.
		public void AnimateImage() 
		{
			if (!currentlyAnimating) 
			{
				//Begin the animation only once.
				ImageAnimator.Animate(AnimatedImage, new EventHandler(this.Pino));
				currentlyAnimating = true;
			}
		}

		public void Pino(object o, EventArgs e) 
		{
			//Force a call to the Paint event handler.
			this.Invalidate();
		}

		private void AnimatedControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if (AnimatedImage == null)
				return;

			//Begin the animation.
			AnimateImage();
			//Get the next frame ready for rendering.
			ImageAnimator.UpdateFrames();
			//Draw the next frame in the animation.
			e.Graphics.DrawImage(this.AnimatedImage, new Point(0, 0));
			
		}
	}
}
