using System.Web;
using System.Web.SessionState;

namespace Microarea.TaskBuilderNet.Woorm.WoormController
{
	class WoormHandler : IHttpHandler, IReadOnlySessionState
	{

		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			HttpGenericHandler handler = new HttpGenericHandler(context.Request, context.Response);
			handler.ProcessRequest();
		}

		//--------------------------------------------------------------------------------
		public bool IsReusable
		{
			get { return true; }
		}		
	}
}
