using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	internal class TabbedPanelSplitter : Control
	{
		public TabbedPanelSplitter()
		{
			SetStyle(ControlStyles.Selectable, false);
		}

		public override DockStyle Dock
		{
			get	{	return base.Dock;	}
			set
			{
				SuspendLayout();
				base.Dock = value;

				if (Dock == DockStyle.Left || Dock == DockStyle.Right)
					Width = DockManager.MeasureContainer.DragSize;
				else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
					Height = DockManager.MeasureContainer.DragSize;
				else
					Bounds = Rectangle.Empty;

				if (Dock == DockStyle.Left || Dock == DockStyle.Right)
					Cursor = Cursors.VSplit;
				else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
					Cursor = Cursors.HSplit;
				else
					Cursor = Cursors.Default;
					
				ResumeLayout();

				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			TabbedPanel tabWindow = Parent as TabbedPanel;
			if (tabWindow == null)
				return;

			if (tabWindow.DockState == DockState.Document)
				DrawDocumentWindowBorder(e.Graphics);
			else if (DockManager.IsDockStateAutoHide(tabWindow.DockState))
				DrawAutoHideWindowBorder(e.Graphics);
		}

		private void DrawDocumentWindowBorder(Graphics g)
		{
			TabbedPanel tabWindow = Parent as TabbedPanel;
			if (Dock == DockStyle.Left)
			{
				Rectangle rectTabStrip = ClientRectangle;
				rectTabStrip.Height = tabWindow.TabStripRectangle.Height;
				g.FillRectangle(SystemBrushes.ControlLightLight, rectTabStrip);
				Point ptRightTop = new Point(ClientRectangle.Right - 1, ClientRectangle.Top);
				Point ptRightBottom = new Point(ClientRectangle.Right - 1, ClientRectangle.Bottom);
				g.DrawLine(SystemPens.ControlDark, ptRightTop, ptRightBottom);
			}
			else if (Dock == DockStyle.Top)
			{
				Point ptLeftBottom = new Point(ClientRectangle.Left, ClientRectangle.Bottom - 1);
				Point ptRightBottom = new Point(ClientRectangle.Right, ClientRectangle.Bottom - 1);
				g.DrawLine(SystemPens.ControlDark, ptLeftBottom, ptRightBottom);
			}
		}

		private void DrawAutoHideWindowBorder(Graphics g)
		{
			if (Dock == DockStyle.Top)
				g.DrawLine(SystemPens.ControlLightLight, 0, 1, ClientRectangle.Right, 1);
			else if (Dock == DockStyle.Left)
				g.DrawLine(SystemPens.ControlLightLight, 1, 0, 1, ClientRectangle.Bottom);
			else if (Dock == DockStyle.Bottom)
			{
				g.DrawLine(SystemPens.ControlDark, 0, ClientRectangle.Height - 2, ClientRectangle.Right, ClientRectangle.Height - 2);
				g.DrawLine(SystemPens.ControlDarkDark, 0, ClientRectangle.Height - 1, ClientRectangle.Right, ClientRectangle.Height - 1);
			}
			else if (Dock == DockStyle.Right)
			{
				g.DrawLine(SystemPens.ControlDark, ClientRectangle.Width - 2, 0, ClientRectangle.Width - 2, ClientRectangle.Bottom);
				g.DrawLine(SystemPens.ControlDarkDark, ClientRectangle.Width - 1, 0, ClientRectangle.Width - 1, ClientRectangle.Bottom);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button != MouseButtons.Left)
				return;

			TabbedPanel tabbedPanel = Parent as TabbedPanel;
			if (tabbedPanel == null)
				return;

			tabbedPanel.BeginDrag();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)Win32.Msgs.WM_MOUSEACTIVATE)
			{
				return;
			}
			base.WndProc(ref m);
		}
	}
}
