using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer
{
	//=========================================================================
	public class BaseAdvRenderer : System.Windows.Forms.UserControl
	{

        public string AutToken;
       

		//--------------------------------------------------------------------
		protected BaseAdvRenderer()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.UpdateStyles();
			this.BackColor = Color.Transparent;
		}

		//--------------------------------------------------------------------
		public virtual void RenderAdvertisement(IAdvertisement advertisement)
		{}

		//--------------------------------------------------------------------
		public virtual void RenderAdvertisement(Uri documentUri)
		{}

		//--------------------------------------------------------------------
		public virtual void Clear()
		{}
	}
}