using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Services;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using System.Collections;
using System.Web;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for EasyLookService.
	/// </summary>
	//================================================================================
	public class EasyLookService : System.Web.Services.WebService
	{
		//---------------------------------------------------------------------
		public EasyLookService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		private string AuthenticationToken(out LoginManager lm, string user, string password, string company, bool useNTAuth)
		{
			// crea ed esegue la login attraverso il WebService
			lm = new LoginManager();

			int result = lm.ValidateUser(user, password, useNTAuth);
			if (result != 0)
				return null;

			result = lm.Login(company, ProcessType.EasyLook, true);
			if (result != 0)
			{
				return null;
			}

			return lm.AuthenticationToken;
		}

		//---------------------------------------------------------------------
		private string ErrorDom(string parameters, string user, string password, string company)
		{
			XmlDocument dom = new XmlDocument();
			dom.LoadXml(parameters);

			string prefix = dom.DocumentElement.Prefix;
			string schema = dom.DocumentElement.NamespaceURI;
			string errorText = string.Format("Bad Authentication, invalid info--> User: {0}, Password: {1}, Company: {2}", user, password, company);
			XmlElement errorNode = dom.CreateElement(prefix, "Error", schema);

			XmlElement code = dom.CreateElement(prefix, "Code", schema);
			code.InnerText = SoapTypes.To(1);
			errorNode.AppendChild(code);

			XmlElement message = dom.CreateElement(prefix, "Message", schema);
			message.InnerText = SoapTypes.To(errorText);
			errorNode.AppendChild(message);

			XmlElement source = dom.CreateElement(prefix, "Source", schema);
			source.InnerText = SoapTypes.To("EasyLookService");
			errorNode.AppendChild(source);


			XmlNodeList elemList = dom.DocumentElement.GetElementsByTagName("Parameters", schema);
			if (elemList != null && elemList.Count == 1)
				dom.DocumentElement.ReplaceChild(errorNode, elemList[0]);

			return dom.InnerXml;
		}

		#region Component Designer generated code

		//Required by the Web Services Designer 
		private IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		/// <summary>
		/// WebMethod that returns a xml representing report's data
		/// </summary>
		//--------------------------------------------------------------------------------
		[WebMethod]
		//---------------------------------------------------------------------
		public StringCollection XmlExecuteReport
			(
				string authenticationToken,
				string parameters,
				DateTime applicationDate,
				string impersonatedUser,
				bool useApproximation
			)
		{
			try
			{
				XmlReportEngine xml = new XmlReportEngine
				(
					authenticationToken,
					parameters,
					applicationDate,
					impersonatedUser,
					useApproximation,
					this.Context,
					false
				);
				return xml.XmlExecuteReport();
			}
			catch (Exception exc)
			{
				StringCollection sc = new StringCollection();
				sc.Add(exc.ToString());

				return sc;
			}
		}

        /// <summary>
        /// WebMethod that returns a xml representing report's data, including not valid field
        /// e.g: in an outerjoin, if a field is missing in database, is written in xml with attribute isValid=false 
        /// </summary>
        //--------------------------------------------------------------------------------
        [WebMethod]
        //---------------------------------------------------------------------
        public StringCollection XmlExecuteReportWithNotValidFields
            (
                string authenticationToken,
                string parameters,
                DateTime applicationDate,
                string impersonatedUser,
                bool useApproximation
            )
        {
            try
            {
                XmlReportEngine xml = new XmlReportEngine
                (
                    authenticationToken,
                    parameters,
                    applicationDate,
                    impersonatedUser,
                    useApproximation,
                    this.Context,
                    false
                );

                xml.WriteNotValidField = true;

                return xml.XmlExecuteReport();
            }
            catch (Exception exc)
            {
                StringCollection sc = new StringCollection();
                sc.Add(exc.ToString());

                return sc;
            }
        }
		/// <summary>
		/// WebMethod that returns a xml representing report's data for Electronic Invoice purpouse
		/// </summary>
		//--------------------------------------------------------------------------------
		[WebMethod]
		//---------------------------------------------------------------------
		public StringCollection XmlExecuteReportEI
			(
				string authenticationToken,
				string parameters,
				DateTime applicationDate,
				string impersonatedUser,
				bool useApproximation
			)
		{
			try
			{
				XmlReportEngine xml = new XmlReportEngine
				(
					authenticationToken,
					parameters,
					applicationDate,
					impersonatedUser,
					useApproximation,
					this.Context,
					true
				);

				// xml.ReportSession.EInvoice = true;

				return xml.XmlExecuteReport();
			}
			catch (Exception exc)
			{
				StringCollection sc = new StringCollection();
				sc.Add(exc.ToString());

				return sc;
			}
		}

		/// <summary>
		/// WebMethod that returns a memory byte stream representing pdf print of report
		/// </summary>
		//--------------------------------------------------------------------------------
		[WebMethod(EnableSession = true)]
		//---------------------------------------------------------------------
		public byte[] PdfExecuteReport
			(
				string authenticationToken,
				string parameters,
				DateTime applicationDate,
				string impersonatedUser,
				bool useApproximation,
				ref string diagnostic
			)
		{
			try
			{
				PdfReportEngine pdfReportEngine = new PdfReportEngine
				(
					authenticationToken,
					parameters,
					applicationDate,
					impersonatedUser,
					useApproximation,
					this.Context
				);

				return pdfReportEngine.ExecuteReport(ref diagnostic);
			}
			catch (Exception exc)
			{
				diagnostic = exc.ToString();
				return new byte[0]; //array vuoto
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		//---------------------------------------------------------------------
		public String XmlGetParameters
			(
				string authenticationToken,
				string parameters,
				DateTime applicationDate,
				string impersonatedUser,
				bool useApproximation
			)
		{
			XmlReportEngine xml = new XmlReportEngine
				(
					authenticationToken,
					parameters,
					applicationDate,
					impersonatedUser,
					useApproximation,
					this.Context,
					false
				);

			return xml.XmlGetParameters();
		}
	}
}
