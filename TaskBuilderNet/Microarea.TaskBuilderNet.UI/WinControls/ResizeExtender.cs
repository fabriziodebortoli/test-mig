using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	public partial class ResizeExtender : Component
	{
		public ResizeExtender()
		{
			InitializeComponent();
		}


		public ResizeExtender(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		[Flags]
		public enum ResizeBorderKind { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

		Control resizableControl;
		ResizableExtenderControl innerControl;


		public bool Resizing { get { return innerControl != null && innerControl.Resizing; } }
		public bool Visible
		{
			get { return innerControl != null && innerControl.Visible; }
			set
			{
				if (innerControl != null) innerControl.Visible = value;
			}
		}

		public ResizeExtender.ResizeBorderKind ResizeBorder
		{
			get { return innerControl == null ? ResizeBorderKind.None : innerControl.ResizeBorder; }
			set { if (innerControl != null) innerControl.ResizeBorder = value; }
		}

		public int BorderWidth
		{
			get { return innerControl == null ? 0 : innerControl.BorderWidth; }
			set { if (innerControl != null) innerControl.BorderWidth = value; }
		}
		public Control ResizableControl
		{
			get { return resizableControl; }
			set
			{
				if (innerControl != null)
					innerControl.Dispose();
				resizableControl = value;
				innerControl = new ResizableExtenderControl(resizableControl);
			}
		}
	}
}
