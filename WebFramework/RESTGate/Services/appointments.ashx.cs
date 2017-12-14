using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Net;
using RESTGate.OrganizerCore;
using RESTGate.Helpers;
using RESTGate.Services.WebHelpers;

namespace RESTGate.Services
{
    public class appointments : Page, IHttpHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            string httpMethod = context.Request.HttpMethod;
            string token = context.Request.Params["token"];

            if (!WebFrontLine.ServicesFirstCall(context, token))
            {
                return;
            }

            switch (httpMethod)
            { 
                case "GET":

                    TaskTodo tasks = new TaskTodo();
                    tasks.Token = token;
                    tasks.LoadTasks(1, 1, new DateTime(2013, 01, 01), new DateTime(2013, 12, 01), TaskKind.All);
                    string response = JSONHelper.ToJSON(tasks.Appointments);
                    context.Response.ContentType = "text/json";
                    context.Response.Write(response);

                break;

                case "PUT":

                    string title = context.Request.Form["title"];
                    string description = context.Request.Form["description"];
                    string resource = context.Request.Form["resource"];
                    string company = LivingTokens.Instance.GetCompanyFromToken(token);
                    OM_Commitments commitment = new OM_Commitments();
                    commitment.Subject = title;
                    commitment.Description = description;
                    commitment.StartDate = DateTime.Now;
                    commitment.EndDate = DateTime.Now.AddDays(1);
                    commitment.RecurrenceEndDate = DateTime.Now.AddDays(1);
                    commitment.ReminderTime = DateTime.Now.AddDays(1);
                    commitment.WorkerID = 1;
                    OrganizerCache.Instance.AddCommitment(company, commitment);

                break;
            }

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