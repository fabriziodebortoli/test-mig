using System;
using Xamarin.Forms;

namespace TBMobileUI
{
	public class GestureFrame : Frame
	{
		public GestureFrame ()
		{
		}
			
		public event EventHandler SwipeLeft;
		public event EventHandler SwipeRight;
		public event EventHandler SwipeTop;
		public event EventHandler SwipeDown;

		public void OnSwipeLeft()
		{
			if (SwipeLeft != null)
				SwipeLeft(this, null);
		}
		public void OnSwipeRight()
		{
			if (SwipeRight != null)
				SwipeRight(this, null);
		}
		public void OnSwipeTop()
		{
			if (SwipeTop != null)
				SwipeTop(this, null);
		}
		public void OnSwipeDown()
		{
			if (SwipeDown != null)
				SwipeDown(this, null);
		}
	}
}

