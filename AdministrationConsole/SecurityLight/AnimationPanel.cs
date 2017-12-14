using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for AnimationPanel.
	/// </summary>
	public partial class AnimationPanel	 : System.Windows.Forms.Panel
	{
		private AnimatedImageSizeMode sizeMode = AnimatedImageSizeMode.Normal;

		private bool animationInProgress = false;
		
    	public AnimationPanel(string animatedImageResource)
		{
			if (animatedImageResource != null && animatedImageResource.Length > 0 && animatedImageResource != null && animatedImageResource.Length > 0)
			{
				Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(animatedImageResource);
				if (imageStream == null)
					return;
					
				this.AnimatedImage = Image.FromStream(imageStream);
			}
		}

		public AnimationPanel(System.Drawing.Image aAnimatedImage)
		{
			this.AnimatedImage = aAnimatedImage;
		}

		public AnimationPanel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}
		
		#region AnimationPanel private methods

		//---------------------------------------------------------------------------
		private void SizeToAnimatedImage()
		{
			if (animatedImage != null)
			{
				Size newSize = new Size(animatedImage.Width, animatedImage.Height);

				if (this.BorderStyle == BorderStyle.FixedSingle)
				{
					newSize.Width += (2 * SystemInformation.BorderSize.Width);
					newSize.Height += (2 * SystemInformation.BorderSize.Height);
				}
				else if (this.BorderStyle == BorderStyle.Fixed3D)
				{
					newSize.Width += (2 * SystemInformation.Border3DSize.Width);
					newSize.Height += (2 * SystemInformation.Border3DSize.Height);
				}

				this.Size = newSize;
			}
		}

		#endregion

		#region AnimationPanel protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e) 
		{
			if (e == null || e.Graphics == null)
				return;

			System.Drawing.SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
			e.Graphics.FillRectangle(bkgndBrush, this.ClientRectangle);
			bkgndBrush.Dispose();

			if (animatedImage != null)
			{
				//Get the next frame ready for rendering.
				ImageAnimator.UpdateFrames();
				//Draw the next frame in the animation.
				if (sizeMode == AnimatedImageSizeMode.Normal || sizeMode == AnimatedImageSizeMode.AutoSize)
					e.Graphics.DrawImage(animatedImage, new Point(0, 0));
				else if (sizeMode == AnimatedImageSizeMode.CenterImage) 
					e.Graphics.DrawImage(animatedImage, new Point((this.Width - animatedImage.Width)/2, (this.Height - animatedImage.Height)/2));
				else if (sizeMode == AnimatedImageSizeMode.StretchImage)
					e.Graphics.DrawImage(animatedImage, 0, 0, this.Width, this.Height);
			}
		}

		#endregion

		#region AnimationPanel event handlers
		//---------------------------------------------------------------------------
		private void Animation_FrameChanged(object sender, EventArgs e) 
		{
			//Force a call to the Paint event handler.
			this.Invalidate();
		}
		
		#endregion

		#region AnimationPanel public properties
		
		//---------------------------------------------------------------------------
		public new BorderStyle BorderStyle
		{
			get { return base.BorderStyle;} 

			set 
			{
				base.BorderStyle = value;

				if (sizeMode == AnimatedImageSizeMode.AutoSize)
					SizeToAnimatedImage();
				
				Refresh(); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Image AnimatedImage 
		{
			get { return animatedImage;} 

			set
			{
				animatedImage = value;

				if (sizeMode == AnimatedImageSizeMode.AutoSize)
					SizeToAnimatedImage();
				
				Refresh(); 
			}
		}
		
		//---------------------------------------------------------------------------
		public AnimatedImageSizeMode SizeMode 
		{
			get { return sizeMode;} 
			set 
			{
				if (sizeMode == value)
					return;

				sizeMode = value; 

				if (sizeMode == AnimatedImageSizeMode.AutoSize)
					SizeToAnimatedImage();
				
				Refresh(); 
			}
		}

		//---------------------------------------------------------------------------
		public bool AnimationInProgress { get { return animationInProgress; } }

		#endregion

		#region AnimationPanel public methods

		//---------------------------------------------------------------------------
		public void StartAnimation() 
		{
			if (animatedImage != null)
			{
				System.Drawing.ImageAnimator.Animate(animatedImage, new EventHandler(Animation_FrameChanged));
				animationInProgress = true;
			}
		}

		//---------------------------------------------------------------------------
		public void StopAnimation() 
		{
			if (animatedImage != null)
			{
				System.Drawing.ImageAnimator.StopAnimate(animatedImage, new EventHandler(this.Animation_FrameChanged));
				animationInProgress = false;
			}
		}
		
		#endregion
	}

    public enum AnimatedImageSizeMode
    {
        Normal = 0x0001,	// The image is placed in the upper-left corner of the panel. The image is clipped if it is larger than the panel it is contained in.
        AutoSize = 0x0002,	// The panel is sized equal to the size of the image that it contains.
        StretchImage = 0x0003,	// The image within the panel is stretched or shrunk to fit the size of the panel.	
        CenterImage = 0x0004	// The image is displayed in the center if the panel is larger than the image. If the image is larger than the panel, the picture is placed in the center of the panel and the outside edges are clipped.
    }
}
