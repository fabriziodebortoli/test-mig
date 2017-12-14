using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        /// <summary>
        /// DragHandlerBase is the base class for drag handlers. The derived class should:
        ///   1. Define its public method BeginDrag. From within this public BeginDrag method,
        ///      DragHandlerBase.BeginDrag should be called to initialize the mouse capture
        ///      and message filtering.
        ///   2. Override the OnDragging and OnEndDrag methods.
        /// </summary>
        private abstract class DragHandlerBase : NativeWindow
        {
            protected DragHandlerBase()
            {
            }

            protected abstract Control DragControl
            {
                get;
            }

            private Point m_startMousePosition = Point.Empty;
            protected Point StartMousePosition
            {
                get { return m_startMousePosition; }
                private set { m_startMousePosition = value; }
            }
			
			Control FindOuterParent(Control c)
			{
				Control parent = c;
				while (parent.Parent != null)
					parent = parent.Parent;
				return parent;
			}

            protected bool BeginDrag()
            {
                // Avoid re-entrance;
                lock (this)
                {
                    if (DragControl == null)
                        return false;
					Control form = FindOuterParent(DragControl);
					if (form == null)
						return false;
					
                    StartMousePosition = Control.MousePosition;

                    if (!NativeMethods.DragDetect(DragControl.Handle, StartMousePosition))
                        return false;

					form.Capture = true;
					AssignHandle(form.Handle);
                    return true;
                }
            }

            protected abstract void OnDragging();

            protected abstract void OnEndDrag(bool abort);

            private void EndDrag(bool abort)
            {
                ReleaseHandle();
                FindOuterParent(DragControl).Capture = false;

                OnEndDrag(abort);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)Win32.Msgs.WM_CANCELMODE || m.Msg == (int)Win32.Msgs.WM_CAPTURECHANGED)
                    EndDrag(true);
				else if (m.Msg == (int)Win32.Msgs.WM_MOUSEMOVE)
					OnDragging();
				else if (m.Msg == (int)Win32.Msgs.WM_LBUTTONUP)
					EndDrag(false);
				else if (m.Msg == (int)Win32.Msgs.WM_CAPTURECHANGED)
					EndDrag(true);
				else if (m.Msg == (int)Win32.Msgs.WM_KEYDOWN && (int)m.WParam == (int)Keys.Escape)
					EndDrag(true);
                
				base.WndProc(ref m);
            }
        }

        private abstract class DragHandler : DragHandlerBase
        {
            private DockPanel m_dockPanel;

            protected DragHandler(DockPanel dockPanel)
            {
                m_dockPanel = dockPanel;
            }

            public DockPanel DockPanel
            {
                get { return m_dockPanel; }
            }

            private IDragSource m_dragSource;
            protected IDragSource DragSource
            {
                get { return m_dragSource; }
                set { m_dragSource = value; }
            }

            protected sealed override Control DragControl
            {
                get { return DragSource == null ? null : DragSource.DragControl; }
            }

			protected override void WndProc(ref Message m)
			{  
				if ((m.Msg == (int)Win32.Msgs.WM_KEYDOWN || m.Msg == (int)Win32.Msgs.WM_KEYUP) &&
                    ((int)m.WParam == (int)Keys.ControlKey || (int)m.WParam == (int)Keys.ShiftKey))
                    OnDragging();

				base.WndProc(ref m);
			}
        }
    }
}
