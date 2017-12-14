using System;
using System.Web.Routing;
using RESTGate.OrganizerCore;

namespace RESTGate
{
    //================================================================================
    public class Global : System.Web.HttpApplication
    {
        //--------------------------------------------------------------------------------
        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
            OrganizerCache.Instance.ReloadData();
        }

        //--------------------------------------------------------------------------------
        void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute("RouteRoot", "", "~/Services/IsAlive.ashx");
            routes.MapPageRoute("CacheRefresh", "cacheRefresh", "~/Services/cacheRefresh.ashx");
            routes.MapPageRoute("Appointments", "appointments", "~/Services/appointments.ashx");
            routes.MapPageRoute("Workers", "workers", "~/Services/workers.ashx");
            routes.MapPageRoute("AppointmentsSingle", "appointments/{id}", "~/Services/appointments.ashx");
        }

        //--------------------------------------------------------------------------------
        void Application_End(object sender, EventArgs e)
        {
        }

        //--------------------------------------------------------------------------------
        void Application_Error(object sender, EventArgs e)
        {
        }

        //--------------------------------------------------------------------------------
        void Session_Start(object sender, EventArgs e)
        {
        }

        //--------------------------------------------------------------------------------
        void Session_End(object sender, EventArgs e)
        {
        }

    }
}
