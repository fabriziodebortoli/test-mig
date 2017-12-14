using System;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Themes;

namespace Microarea.TaskBuilderNet.Forms
{

	//================================================================================================================
	class TBWinHyperLinkExtender : TBWinExtender, ITBHyperLinkUIProvider
	{
		UIPanel clickPanel;

		private MHotLink HotLink { get { return Data != null ? Data.CurrentHotLink as MHotLink: null; } }

        //----------------------------------------------------------------------------
        public TBWinHyperLinkExtender(ITBCUI tbObject)
			: base(tbObject)
		{
			Position = UIExtenderPosition.Over;
			Data.ValueChanged += new EventHandler<EasyBuilderEventArgs>(Data_ValueChanged);
        }

		//----------------------------------------------------------------------------
		public override void OnControlEnabledChanged(bool newEnabled)
		{
			if (this.Controller == null || clickPanel == null)
			{
				return;
			}
            TBThemeManager.Theme.SetAsHyperLink(this.Controller.Control, !newEnabled);
            clickPanel.Visible = !newEnabled;
		}

		//----------------------------------------------------------------------------
		void Data_ValueChanged(object sender, EasyBuilderEventArgs e)
		{
			if (Data == null || Data.Empty)
				return;
			
			HotLink.RefreshHyperLink(Data);
		}

		//----------------------------------------------------------------------------
		void clickPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			if (!IsHyperLinkAvailable())
				return;

			HotLink.FollowHyperLink(null, true);
		}

		//----------------------------------------------------------------------------
		private bool IsHyperLinkAvailable()
		{
			if (Controller == null)
				return false;

			if (Data.Empty)
				return false;

			return Controller.Control != null && !Controller.Control.Enabled;
		}

		//----------------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
		{
			if (Data == null || HotLink == null || Controller == null || this.Controller.Control == null) 
				return null;

			clickPanel = new UIPanel();
			clickPanel.MouseClick += new MouseEventHandler(clickPanel_MouseClick);
			clickPanel.MouseMove += new MouseEventHandler(clickPanel_MouseMove);

			OnControlEnabledChanged(Controller.Control.Enabled);

			return clickPanel;
		}

		//----------------------------------------------------------------------------
		void clickPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (!IsHyperLinkAvailable())
				return;

			Cursor.Current = Data == null || Data.Empty
				? Cursors.Default
				: Cursors.Hand;
		}
	
        //-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (Data != null)
				{
					Data.ValueChanged -= new EventHandler<EasyBuilderEventArgs>(Data_ValueChanged);
				}
				DestroyExtenderUIControl();
            }
        }

        //-------------------------------------------------------------------------
		public override void DestroyExtenderUIControl()
        {
			if (clickPanel != null)
			{
				clickPanel.MouseClick -= new MouseEventHandler(clickPanel_MouseClick);
				clickPanel.MouseMove -= new MouseEventHandler(clickPanel_MouseMove);
				
				clickPanel.Dispose();
				clickPanel = null;
			}
        }
	}
}
