using System.Web;
using System.Web.UI;
using System.Net;
using RESTGate.OrganizerCore;

namespace RESTGate.Services
{
    /// <summary>
    /// rest service to refresh organizer cache
    /// </summary>
    //================================================================================
    public class cacheRefresh : Page, IHttpHandler
    {
        //--------------------------------------------------------------------------------
        public override void ProcessRequest(HttpContext context)
        {
            bool result = OrganizerCache.Instance.ReloadData();
            HttpStatusCode httpStatus = result ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            context.Response.StatusCode = (int)httpStatus;
        }

        //--------------------------------------------------------------------------------
        new public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}