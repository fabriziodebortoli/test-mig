using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	class ResizableExtenderControl : Panel
	{
		protected Point mouseDragInitialPosition;
		public bool drag = false;
		public int ResizeNS = 0;
		public int ResizeEW = 0;
		Control resizableControl = null;
		public bool Resizing { get { return drag; } }
		public int BorderWidth { get; set; }
		private ResizeExtender.ResizeBorderKind resizeBorder = ResizeExtender.ResizeBorderKind.None;
		public ResizeExtender.ResizeBorderKind ResizeBorder
		{
			get { return resizeBorder; }
			set
			{
				resizeBorder = value;
				Cursor = GetCursor();
				switch (ResizeBorder)
				{
					case ResizeExtender.ResizeBorderKind.None:
						break;
					case ResizeExtender.ResizeBorderKind.Top:
						ResizeNS = 1;
						break;
					case ResizeExtender.ResizeBorderKind.Right:
						ResizeEW = 1;
						break;
					case ResizeExtender.ResizeBorderKind.Bottom:
						ResizeNS = -1;
						break;
					case ResizeExtender.ResizeBorderKind.Left:
						ResizeEW = -1;
						break;
					default:
						break;
				}
				Align();
			}
		}
		public ResizableExtenderControl(Control resizableControl)
		{
			this.resizableControl = resizableControl;
			this.BackColor = Color.RoyalBlue;
			BorderWidth = 2;
			resizableControl.LocationChanged += new EventHandler(innerControl_LocationChanged);
			resizableControl.SizeChanged += new EventHandler(innerControl_SizeChanged);
			resizableControl.ParentChanged += new EventHandler(resizableControl_ParentChanged); 
			ResizeBorder = ResizeExtender.ResizeBorderKind.None;
			AlignParent();
			
		}

		void resizableControl_ParentChanged(object sender, EventArgs e)
		{
			AlignParent();
		}

		private void AlignParent()
		{
			Control parent = resizableControl.Parent;
			if (parent != null)
				Parent = parent;
			else
			{
				IntPtr hParent = ExternalAPI.GetParent(resizableControl.Handle);
				if (!hParent.Equals(0))
					ExternalAPI.SetParent(Handle, hParent);
			}
			resizableControl.BringToFront();
		}
		
		void innerControl_SizeChanged(object sender, EventArgs e)
		{
			Align();
		}

		void innerControl_LocationChanged(object sender, EventArgs e)
		{
			Align();
		}

		private void Align()
		{
			switch (ResizeBorder)
			{
				case ResizeExtender.ResizeBorderKind.None:
					break;
				case ResizeExtender.ResizeBorderKind.Top:
					this.Location = new Point(resizableControl.Left, resizableControl.Top - BorderWidth);
					this.Size = new Size(resizableControl.Width, BorderWidth);
					break;
				case ResizeExtender.ResizeBorderKind.Right:
					this.Location = new Point(resizableControl.Right, resizableControl.Top);
					this.Size = new Size(BorderWidth, resizableControl.Height);
					break;
				case ResizeExtender.ResizeBorderKind.Bottom:
					this.Location = new Point(resizableControl.Left, resizableControl.Bottom);
					this.Size = new Size(resizableControl.Width, BorderWidth);
					break;
				case ResizeExtender.ResizeBorderKind.Left:
					this.Location = new Point(resizableControl.Left - BorderWidth, resizableControl.Top);
					this.Size = new Size(BorderWidth, resizableControl.Height);
					break;
				default:
					break;
			}
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				resizableControl.LocationChanged -= new EventHandler(innerControl_LocationChanged);
				resizableControl.SizeChanged -= new EventHandler(innerControl_SizeChanged);
				resizableControl.ParentChanged -= new EventHandler(resizableControl_ParentChanged); 
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (!this.drag)
				return;

			Point pt = Control.MousePosition;
			if (ResizeNS != 0 || ResizeEW != 0)
			{
				resizableControl.Height += (ResizeNS * (pt.Y - mouseDragInitialPosition.Y));
				if (ResizeNS < 0)
					resizableControl.Top += (pt.Y - mouseDragInitialPosition.Y);
				resizableControl.Width += (ResizeEW * (pt.X - mouseDragInitialPosition.X));
				if (ResizeEW < 0)
					resizableControl.Left += (pt.X - mouseDragInitialPosition.X);
			}
			this.drag = false;
			Capture = false;

		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Point pt = Control.MousePosition;

			if (drag)
			{
				IntPtr hParent = ExternalAPI.GetParent(Handle);
				Point mouse = pt;
				ExternalAPI.ScreenToClient(hParent, ref mouse);
				
				switch (ResizeBorder)
				{
					case ResizeExtender.ResizeBorderKind.None:
						break;
					case ResizeExtender.ResizeBorderKind.Top:
						this.Location = new Point(resizableControl.Left, mouse.Y);
						break;
					case ResizeExtender.ResizeBorderKind.Right:
						this.Location = new Point(mouse.X, resizableControl.Top);
						break;
					case ResizeExtender.ResizeBorderKind.Bottom:
						this.Location = new Point(resizableControl.Left, mouse.Y);
						break;
					case ResizeExtender.ResizeBorderKind.Left:
						this.Location = new Point(mouse.X, resizableControl.Top);
						break;
					default:
						break;
				}
			}
		}

		private Cursor GetCursor()
		{
			switch (ResizeBorder)
			{
				case ResizeExtender.ResizeBorderKind.None:
					return Cursors.Default;
				case ResizeExtender.ResizeBorderKind.Top:
					return Cursors.SizeNS;
				case ResizeExtender.ResizeBorderKind.Right:
					return Cursors.SizeWE;
				case ResizeExtender.ResizeBorderKind.Bottom:
					return Cursors.SizeNS;
				case ResizeExtender.ResizeBorderKind.Left:
					return Cursors.SizeWE;
				default:
					return Cursors.Default;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			mouseDragInitialPosition = Control.MousePosition;
			Capture = true;
			this.drag = true;
		}



	}
}
