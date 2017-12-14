using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;


namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	/// <summary>
	/// Summary description for MenuLoadingPanel.
	/// </summary>
	public partial class MenuLoadingPanel : System.Windows.Forms.Panel
	{
		System.Drawing.Bitmap		animatedImage = null;
		private int					standardWidth = 0;
		private int					standardHeight = 0;

		//---------------------------------------------------------------------------
		public int StandardWidth { get { return standardWidth; } }
		public int StandardHeight { get { return standardHeight; } }
		
		//---------------------------------------------------------------------------
		public MenuLoadingPanel()
		{
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Animations.LoadMenus.gif");
			if (imageStream != null)
			{
				animatedImage = new System.Drawing.Bitmap(imageStream);
				//Begin the animation.
				System.Drawing.ImageAnimator.Animate(animatedImage, new EventHandler(this.OnFrameChanged));
			}
			standardWidth = this.Width;
			standardHeight = this.Height;
		}

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e) 
		{
			base.OnPaint(e);

			if (e == null || e.Graphics == null)
				return;

			ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, System.Windows.Forms.Border3DStyle.Etched);
			
			if (animatedImage != null)
			{
				//Get the next frame ready for rendering.
				ImageAnimator.UpdateFrames();
				//Draw the next frame in the animation.
				e.Graphics.DrawImage(animatedImage, new Point(4, this.Height - animatedImage.Height - 4));
			}
		}

		//---------------------------------------------------------------------------
		private void OnFrameChanged(object o, EventArgs e) 
		{
			//Force a call to the Paint event handler.
			this.Invalidate();
		}

		
		//---------------------------------------------------------------------------
		public void StopAnimation() 
		{
			if (animatedImage != null)
				ImageAnimator.StopAnimate(animatedImage, new EventHandler(this.OnFrameChanged));
		}
	}
}
