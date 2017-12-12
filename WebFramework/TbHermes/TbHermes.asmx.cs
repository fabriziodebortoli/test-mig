using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.WebServices.TbHermes
{
	/// <summary>
	/// Summary description for Service1
	/// </summary>
	[WebService(Namespace = "http://microarea.it/TbHermes/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class TbHermes : System.Web.Services.WebService
	{
		// Classe che realizza il plumbing per la comunicazione SOAP
		// Niente logica di business qui, solo invocazione degli equivalenti metodi nell'engine

		//---------------------------------------------------------------------
		[WebMethod]
		public bool IsAlive()
		{
			return true;
		}

		//-------------------------------------------------------------------------------
		[WebMethod]
		public void WakeUp() // per fare partire la prima volta (visto che non è un service)
		{
			// nop
		}

		//---------------------------------------------------------------------------
		[WebMethod]
		public bool Init()
		{
			// TODO valutare se il caso di reinizializzare anche il timer
			return Global.HermesEngine.Init();
		}

		//---------------------------------------------------------------------------
		[WebMethod]
        public string TickVS(out string errorMessage) // Test only
		{
            errorMessage = "";
            string code = "";
            try
            {
                code = Global.HermesEngine.Tick();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return errorMessage;
            }
            return code;
		}

        //---------------------------------------------------------------------------
        [WebMethod]
        public string Tick() // Test only
        {
            string errorMessage = "";
            string code = "";
            try
            {
                code = Global.HermesEngine.Tick();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return errorMessage;
            }
            return code;
        }

		//---------------------------------------------------------------------------
		[WebMethod]
		public ServiceStatus GetUpdatedStatus(string company, ClientIdentifier clientIdentifier)
		{
			return Global.HermesEngine.GetUpdatedStatus(company, clientIdentifier);
		}

		//---------------------------------------------------------------------------
		[WebMethod]
		public void UploadMessage(string company, MailMessage mailMessage)
		{
			Global.HermesEngine.UploadMessage(company, mailMessage);
		}
	}
}