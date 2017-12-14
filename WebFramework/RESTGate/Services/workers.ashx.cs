using System.Web;
using System.Web.UI;
using RESTGate.OrganizerCore;
using RESTGate.Helpers;
using RESTGate.Services.WebHelpers;

namespace RESTGate.Services
{
    //================================================================================
    public class workers : Page, IHttpHandler
    {
        //--------------------------------------------------------------------------------
        public override void ProcessRequest(HttpContext context)
        {
            string token = context.Request.Params["token"];

            if (!WebFrontLine.ServicesFirstCall(context, token))
            {
                return;
            }

            TaskTodo taskManager = new TaskTodo();
            taskManager.Token = token;
            taskManager.LoadWorkers();

            string response = JSONHelper.ToJSON(taskManager.Workers);
            context.Response.ContentType = "text/json";
            context.Response.Write(response);
        }

        new public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}