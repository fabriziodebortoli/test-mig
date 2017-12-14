using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	public sealed class TBWinAddressExtender : TBWinButtonExtender
	{
		//---------------------------------------------------------------------
		public TBWinAddressExtender(ITBCUI controller)
			: base(controller, Resources.AddressMenu)
		{
			Position = UIExtenderPosition.Left;
		}
		
		//---------------------------------------------------------------------
		public override void OnDataReadOnlyChanged(bool newReadOnly)
		{
			base.OnDataReadOnlyChanged(newReadOnly);

			if (Button != null)
				Button.Enabled = true;
		}

		//---------------------------------------------------------------------
		protected override void OnButtonClicking(CancelEventArgs args)
		{
			base.OnButtonClicking(args);

			if (Data == null || Data.Value == null)
			{
				args.Cancel = true;
				return;
			}

			string url = Data.Value.ToString();
			try
			{
				Uri uri = new Uri(url);
				if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
				{
					args.Cancel = true;
					return;
				}
			}
			catch
			{
				args.Cancel = true;
				return;
			}
		}

		//---------------------------------------------------------------------
		protected override void OnButtonClicked(TBWinButtonExtenderEventArgs args)
		{
			base.OnButtonClicked(args);

			Process.Start(Data.Value.ToString());
		}
	}
}
