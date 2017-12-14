using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
    class RSApiHandler : IHttpHandler, IReadOnlySessionState
    {
        /// <summary>
        /// Process the request for the image
        /// </summary>
        /// <param name="context">The current HTTP context</param>
        //--------------------------------------------------------------------------------
        void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
        {
            ReportController controller = ReportController.FromSession(context.Request.Params);
            controller.InitStateMachine(false); //TODO dal URL capire se è prima pagina o successive
            controller.StateMachine.Step();

/*
            while (controller.RSEngine.CurrentState != State.RunViewer)
            {
                controller.RSEngine.Step();
            }
*/

           // Debug.Assert(controller.RSEngine.CurrentState == State.

            controller.StateMachine.ReportSession.UserInfo.SetCulture();
            WoormDocument woorm = controller.StateMachine.Woorm;
            woorm.LoadPage(1);

            JsonRender render = new JsonRender(woorm);

            //TODO
            // render.GetJsonLayout();  render.GetJsonLayout();
            string pageLayout = @"{""layoutname"":""default"",""objects"":[]}";
            string pagePage = @"{""PageData"":" + woorm.RdeReader.CurrentPage.ToString() + @",""values"":[]}"; 
            
            //----
            string resp = pageLayout + ";" + pagePage;

            context.Response.ContentType = "application/json";
            context.Response.Write(pageLayout);
        }

        public bool IsReusable
        {
            get { return true; }
        }

     }
}
