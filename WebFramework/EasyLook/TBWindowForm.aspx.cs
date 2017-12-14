using System;

namespace Microarea.Web.EasyLook
{
	public partial class TBWindowForm : System.Web.UI.Page
	{
		bool partialRender = false;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			wcf.ObjectNamespace = Request["ObjectNamespace"];
		
		}
		protected void Page_Load(object sender, EventArgs e)
		{

		}
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if (Page.Request.Headers["X-TBAjax"] == "true")
			{
				partialRender = true;
				this.wcf.RenderControl(writer);
			}
			else
			{
				partialRender = false;
				base.Render(writer);
			}
		}

		public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
		{
			if (partialRender)
				return;
			base.VerifyRenderingInServerForm(control);
		}

		protected override void OnError (EventArgs e)
		{
			base.OnError(e);
			Helper.RedirectToErrorPageIfPossible();
		}
	}
}
