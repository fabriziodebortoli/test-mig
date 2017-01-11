using System.Web;

namespace Microarea.TaskBuilderNet.Woorm.WebControls
{
	class LinkDocument : IHttpHandler
	{
		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			string action = context.Request.QueryString["Action"];
			string parameters = context.Request.QueryString["Parameters"];
			if (parameters == null)
				parameters = "";

			string alert = context.Request.QueryString["Alert"];
			if (alert == null)
				alert = "";

			string htmlDoc = WebControlsStrings.LinkDocument;
			htmlDoc = htmlDoc.Replace("##InputName##", Helper.DocumentParametersControlName);
			htmlDoc = htmlDoc.Replace("##LoadingMessage##", WebControlsStrings.Loading);
			htmlDoc = htmlDoc.Replace("##Action##", action);
			htmlDoc = htmlDoc.Replace("##Parameters##", parameters);
			htmlDoc = htmlDoc.Replace("##Alert##", alert);
			
			context.Response.ContentType = "text/html";
			context.Response.Write(htmlDoc);
		}

		#endregion
	}
}
