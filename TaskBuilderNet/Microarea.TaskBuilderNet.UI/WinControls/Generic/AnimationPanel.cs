using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	public enum AnimatedImageSizeMode 
	{
		Normal			= 0x0001,	// The image is placed in the upper-left corner of the panel. The image is clipped if it is larger than the panel it is contained in.
		AutoSize		= 0x0002,	// The panel is sized equal to the size of the image that it contains.
		StretchImage	= 0x0003,	// The image within the panel is stretched or shrunk to fit the size of the panel.	
		CenterImage		= 0x0004	// The image is displayed in the center if the panel is larger than the image. If the image is larger than the panel, the picture is placed in the center of the panel and the outside edges are clipped.
	}
	
	/// <summary>
	/// Summary description for AnimationPanel.
	/// </summary>
	public partial class AnimationPanel	 : System.Windows.Forms.Panel
	{
		private System.Drawing.Image animatedImage = null;
		private System.Drawing.Imaging.FrameDimension animatedFrameDimensions = null;
		private int animatedFramesNumber = 0;
		private int animatedFramesCount = 0;

		private AnimatedImageSizeMode sizeMode = AnimatedImageSizeMode.Normal;

		private bool animationInProgress = false;
		private bool repeatAnimationForever = false;

		public AnimationPanel(string animatedImageResource)
		{
			if (animatedImageResource != null && animatedImageResource.Length > 0 && animatedImageResource != null && animatedImageResource.Length > 0)
			{
				Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(animatedImageResource);
				if (imageStream == null)
					return;
					
				AnimatedImage = Image.FromStream(imageStream);
			}
		}

		public AnimationPanel(System.Drawing.Image aAnimatedImage)
		{
			AnimatedImage = aAnimatedImage;
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

			if (this.BackColor != Color.Transparent)
			{
				System.Drawing.SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
				e.Graphics.FillRectangle(bkgndBrush, this.ClientRectangle);
				bkgndBrush.Dispose();
			}

			if (animatedImage != null)
			{
				//Get the next frame ready for rendering.
				if (repeatAnimationForever || animatedFramesCount < animatedFramesNumber)
					ImageAnimator.UpdateFrames(animatedImage);

				//Draw the next frame in the animation.
				if (sizeMode == AnimatedImageSizeMode.Normal || sizeMode == AnimatedImageSizeMode.AutoSize)
					e.Graphics.DrawImageUnscaled(animatedImage, new Point(0, 0));
				else if (sizeMode == AnimatedImageSizeMode.CenterImage) 
					e.Graphics.DrawImage(animatedImage, new Point((this.Width - animatedImage.Width)/2, (this.Height - animatedImage.Height)/2));
				else if (sizeMode == AnimatedImageSizeMode.StretchImage)
					e.Graphics.DrawImage(animatedImage, 0, 0, this.Width, this.Height);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e){}
		
		#endregion

		#region AnimationPanel event handlers
		//---------------------------------------------------------------------------
		private void Animation_FrameChanged(object sender, EventArgs e) 
		{
			animatedFramesCount++;

			if (!repeatAnimationForever && animatedFramesCount > animatedFramesNumber)
				return;

			//Force a call to the Paint event handler.
			this.Invalidate();
		}
		
		#endregion

		#region AnimationPanel public properties
		
		//---------------------------------------------------------------------------
		public new System.Windows.Forms.BorderStyle BorderStyle
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

				animatedFrameDimensions = null;
				animatedFramesNumber = 0;
				animatedFramesCount = 0;

				if (animatedImage != null)
				{
					if (sizeMode == AnimatedImageSizeMode.AutoSize)
						SizeToAnimatedImage();

					 //Create a new FrameDimension object from this image
					animatedFrameDimensions = new System.Drawing.Imaging.FrameDimension(animatedImage.FrameDimensionsList[0]);
					//Determine the number of frames in the image
					//Note that all images contain at least 1 frame, but an animated GIF
					//will contain more than 1 frame.
					animatedFramesNumber = animatedImage.GetFrameCount(animatedFrameDimensions);
				}
				
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

		//---------------------------------------------------------------------------
		public bool RepeatAnimationForever  { get { return repeatAnimationForever; } set { repeatAnimationForever = value; } }

		#endregion

		#region AnimationPanel public methods

		//---------------------------------------------------------------------------
		public void StartAnimation() 
		{
			if (animatedImage != null && System.Drawing.ImageAnimator.CanAnimate(animatedImage))
			{
				System.Drawing.ImageAnimator.Animate(animatedImage, new EventHandler(this.Animation_FrameChanged));
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
}
