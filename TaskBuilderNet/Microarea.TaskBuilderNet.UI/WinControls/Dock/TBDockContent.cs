using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//================================================================================
	public partial class TBDockContent<T> : DockContent, ITBDockContent<T> where T: Control
	{
		private IntPtr hostedHandle;
		private Control hostedControl;
		private static TBDockContent<T> hosting = null;

		//--------------------------------------------------------------------------------
		public T HostedControl
		{
			get { return hostedControl as T; }
		}

		//--------------------------------------------------------------------------------
		public TBDockContent(IntPtr hostedHandle, Control hostedControl)
		{
			this.hostedHandle = hostedHandle;
			this.hostedControl = hostedControl;
			InitializeComponent();
		}
		
		//--------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			ExternalAPI.MoveWindow(hostedHandle, 0, 0, ClientRectangle.Width, ClientRectangle.Height, true);
		}

		/// <summary>
		/// Crea un panel dockabile nel thread di documento e lo appiccica all'hosting panel 
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public static TBDockContent<T> CreateDockablePane(DockPanel hostingPanel, WeifenLuo.WinFormsUI.Docking.DockState dockState, Icon icon, params object[] args)
		{
			IntPtr hostingHandle = IntPtr.Zero;
			Control hostedControl = (Control)Activator.CreateInstance(typeof(T), args);
			hostedControl.TextChanged += new EventHandler(hostedControl_TextChanged);
			IntPtr hostedHandle = hostedControl.Handle;
			
			hosting = new TBDockContent<T>(hostedHandle, hostedControl);
            hosting.Text = hostedControl.Text;
            hosting.Icon = icon;
			hosting.Show(hostingPanel, dockState);
			hostingHandle = hosting.Handle;
			//mi appiccico al contenitore
			hostedControl.Dock = DockStyle.Fill;
			hosting.Controls.Add(hostedControl);
			return hosting;
		}

		/// <summary>
		/// Crea un panel dockabile nel thread di documento e lo appiccica all'hosting panel 
		/// </summary>
		//--------------------------------------------------------------------------------------------------
		public static TBDockContent<T> CreateDockablePane(
			DockPane previousPane, DockAlignment alignment, double proportion,
			Icon icon,
			params object[] args
			)
		{
			IntPtr hostingHandle = IntPtr.Zero;
			Control hostedControl = (Control)Activator.CreateInstance(typeof(T), args);
			hostedControl.TextChanged += new EventHandler(hostedControl_TextChanged);
			IntPtr hostedHandle = hostedControl.Handle;

			hosting = new TBDockContent<T>(hostedHandle, hostedControl);
			hosting.Text = hostedControl.Text;
			hosting.Icon = icon;
			hosting.Show(previousPane, alignment, proportion);
			hostingHandle = hosting.Handle;
			//mi appiccico al contenitore
			hostedControl.Dock = DockStyle.Fill;
			hosting.Controls.Add(hostedControl);
			return hosting;
		}

		//--------------------------------------------------------------------------------------------------
		public static void hostedControl_TextChanged(object sender, EventArgs e)
		{
			if (hosting == null)
				return;

			if (hosting != null && hosting.InvokeRequired)
			{
				hosting.Invoke((Action)delegate { hostedControl_TextChanged(sender, e); });
				return;
			}

			hosting.Text = ((Control)(sender)).Text;
		}
	}
}
