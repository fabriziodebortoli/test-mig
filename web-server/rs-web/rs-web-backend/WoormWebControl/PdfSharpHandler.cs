using System.IO;
using System.Web;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	class PdfSharpHandler : IHttpHandler, IReadOnlySessionState 
	{
		/// <summary>
		/// Process the request for the image
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			ReportController controller = (ReportController)context.Session[context.Request.QueryString["SessionTag"]];
			if (controller == null || controller.StateMachine == null)
			{
				context.Response.ContentType = "text/html";
				context.Response.Write(WoormWebControlStrings.SessionExpired);
				return;
			}

			controller.StateMachine.ReportSession.UserInfo.SetCulture();

			WoormDocument woorm = controller.StateMachine.Woorm;

			PdfRender render = new PdfRender(woorm);

			int current = woorm.RdeReader.CurrentPage;

			for (int i = 1; i <= woorm.RdeReader.TotalPages; i++)
			{
				woorm.LoadPage(i);
				render.ReportPage();
			}

			woorm.RdeReader.CurrentPage = current;

			string filePath = Path.GetTempFileName();
			FileInfo f = new FileInfo(filePath);

			render.SaveToFileAndClose(filePath);
			//se ho il titolo del report uso quello come nome del file (rimuovendo gli spazi che danno fastidio ad alcuni browser (mozilla 3.6.10))
			string sTitle =  woorm.Properties != null && !string.IsNullOrEmpty(woorm.Properties.Title) ? woorm.Properties.Title.Replace(" ", "") : string.Empty;
            if (string.IsNullOrWhiteSpace(sTitle))
            {
                sTitle = woorm.Namespace.Report.Substring(0, woorm.Namespace.Report.Length - 4);
            }
            string fileName = !string.IsNullOrEmpty(sTitle) ? sTitle : filePath;
           
			context.Response.AddHeader("Content-Disposition" ,string.Format("attachment; filename={0}.pdf", fileName));
			context.Response.AddHeader("Content-Length",f.Length.ToString());
			context.Response.ContentType = "application/pdf";
			context.Response.WriteFile(filePath,true);
			File.Delete(filePath);
		}

		public bool IsReusable
		{
			get { return true; }
		}

	}
}
