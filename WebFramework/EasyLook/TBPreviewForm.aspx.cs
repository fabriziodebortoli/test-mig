using System;

namespace Microarea.Web.EasyLook
{
	public partial class TBPreviewForm : System.Web.UI.Page
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			wcfSnapshot.ObjectNamespace = Request["ObjectNamespace"];

		}
		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected override void OnError(EventArgs e)
		{
			base.OnError(e);
			Helper.RedirectToErrorPageIfPossible();
		}
	}
}
