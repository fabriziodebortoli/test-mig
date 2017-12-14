using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using RESTGate.OrganizerCore;

namespace RESTGate.Services
{
    /// <summary>
    /// Summary description for IsAlive
    /// </summary>
    public class IsAlive : Page, IHttpHandler
    {

        public override void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("RESTGate is alive and ready to serve");
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}