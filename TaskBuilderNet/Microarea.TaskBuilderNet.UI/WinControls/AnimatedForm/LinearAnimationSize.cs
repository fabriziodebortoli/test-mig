using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public class LinearAnimationSize : IAnimationSize
	{
		private int animationSteps;
		private int startWidth;
		private int startHeight;
		private int endWidth;
		private int endHeight;

		//---------------------------------------------------------------------
		public LinearAnimationSize(
			int animationSteps,
			Size startSize,
			Size endSize
			)
			: this (animationSteps, startSize.Width, startSize.Height, endSize.Width, endSize.Height)
		{ }

		//---------------------------------------------------------------------
		public LinearAnimationSize(
			int animationSteps,
			int startWidth,
			int startHeight,
			int endWidth,
			int endHeight
			)
		{
			this.animationSteps	= animationSteps;
			this.startWidth		= startWidth;
			this.startHeight	= startHeight;
			this.endWidth		= endWidth;
			this.endHeight		= endHeight;
		}

		#region IAnimationSize Members

		//---------------------------------------------------------------------
		public Size StartSize
		{
			get
			{
				return new Size(startWidth, startHeight);
			}
			set
			{
				startWidth = value.Width;
				startHeight = value.Height;
			}
		}

		//---------------------------------------------------------------------
		public Size EndSize
		{
			get
			{
				return new Size(endWidth, endHeight);
			}
			set
			{
				endWidth = value.Width;
				endHeight = value.Height;
			}
		}

		//---------------------------------------------------------------------
		public Size[] GetSizes()
		{
			int deltaBetweenWidths = (int)((float)(endWidth - startWidth) / (float)animationSteps);
			int deltaBetweenHeights = (int)((float)(endHeight - startHeight) / (float)animationSteps);

            Size[] sizes = new Size[animationSteps];

            sizes[0] = new Size(startWidth, startHeight);
            sizes[sizes.Length - 1] = new Size(endWidth, endHeight);

			for (int i = 1; i < sizes.Length - 1; i++)
			{
                sizes[i] = new Size(
					startWidth + (deltaBetweenWidths * i),
					startHeight + (deltaBetweenHeights * i)
					);
			}

			return sizes;
		}

		#endregion
	}
}
