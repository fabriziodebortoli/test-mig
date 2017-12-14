using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public class LinearAnimationPath : IAnimationPath
	{
		private const int stepsPerSecs = 100;

		private int startX;
		private int startY;
		private int endX;
		private int endY;
		private int animationDurationMillSecs;

		private int totalPointsNumber;

		//---------------------------------------------------------------------
		public LinearAnimationPath(
			Point startPoint,
			Point endPoint,
			int animationDurationMillSecs
			)
			: this (startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, animationDurationMillSecs)
		{ }

		//---------------------------------------------------------------------
		public LinearAnimationPath(
			int startX,
			int startY,
			int endX,
			int endY,
			int animationDurationMillSecs
			)
		{
			this.startX = startX;
			this.startY = startY;
			this.endX = endX;
			this.endY = endY;
			this.animationDurationMillSecs = animationDurationMillSecs;

			this.totalPointsNumber = (int)(((float)animationDurationMillSecs) / 1000F * stepsPerSecs);
		}

		#region IAnimationPath Members

		//---------------------------------------------------------------------
		public Point StartPoint
		{
			get { return new Point(startX, startY); }
			set
			{
				startX = value.X;
				startY = value.Y;
			}
		}

		//---------------------------------------------------------------------
		public Point EndPoint
		{
			get { return new Point(endX, endY); }
			set
			{
				endX = value.X;
				endY = value.Y;
			}
		}

		//---------------------------------------------------------------------
		public int AnimationSteps
		{
			get { return totalPointsNumber; }
		}

		//---------------------------------------------------------------------
		public int MillSecsBetweenPoints
		{
			get { return animationDurationMillSecs / totalPointsNumber; }
		}

		//---------------------------------------------------------------------
		public System.Drawing.Point[] GetPathPoints()
		{
			int deltaBetweenX = (endX - startX) / totalPointsNumber;
			int deltaBetweenY = (endY - startY) / totalPointsNumber;

			Point[] points = new Point[totalPointsNumber];

			points[0] = new Point(startX, startY);
			points[points.Length - 1] = new Point(endX, endY);

			for (int i = 1; i < totalPointsNumber - 1; i++)
				points[i] = new Point(
					startX + (deltaBetweenX * i),
					startY + (deltaBetweenY * i)
					);

			return points;
		}

		#endregion
	}
}
