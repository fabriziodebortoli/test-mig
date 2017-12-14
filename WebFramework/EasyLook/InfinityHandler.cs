using System;
using System.Web;
using System.Web.SessionState;

namespace Microarea.Web.EasyLook
{
	//================================================================================
	public class InfinityHandler : IHttpHandler, IReadOnlySessionState
	{
		//--------------------------------------------------------------------------------
		public bool IsReusable
		{
			// Return false in case your Managed Handler cannot be reused for another request.
			// Usually this would be false in case you have some state information preserved per request.
			get { return true; }
		}

		//--------------------------------------------------------------------------------
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/xml";

			// leggo i parametri passati nella query string
			string company = context.Request.Params["company"];
			string user = context.Request.Params["user"];
			string menuFile = context.Request.Params["menuFile"];
			string start_uid = context.Request.Params["start_uid"];
			string sDepth = context.Request.Params["depth"];

			// se i parametri sono nulli li inizializzo
			if (company == null) company = "Inf_Mago";
			if (user == null) user = "sa";
			if (menuFile == null) menuFile = "";
			if (start_uid == null) start_uid = "";
			int depth = 0;
			int.TryParse(sDepth, out depth);

			// dubbio: ma se utente o company non sono validi ha senso continuare?
			// e caricare quindi un menu che poi non sara' in grado di aprire alcun documento (perche' manca la login sottostante)
			if (string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(user))
				System.Diagnostics.Debug.WriteLine("**** Company o user null!!!");

			// richiamo il caricamento del menu
			if (string.IsNullOrWhiteSpace(start_uid) && string.IsNullOrWhiteSpace(menuFile)) // GettingStarted
				context.Response.Write(InfinityMenuLoader.GetInfinityMenuDescription(company, user));
			else
				context.Response.Write(InfinityMenuLoader.GetInfinityMenuDescription(company, user, menuFile, start_uid, depth));
		}
	}
}
