using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PAT.Workflow.Runtime;
using System.ComponentModel;
using EAConnector.Activities;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.IO;
using PAT.CRM.WSC4.WorkflowObjects;
using EAConnector.EASync;
using EAConnector.Exceptions;

namespace EAConnector
{
	//Description                    --> Descrizione della connessione visualizzata nel Designer
	//WorkflowObject                 --> GUID univoco che identifica questo tipo di connessione
	//WorkflowObjectConfiguration    --> Tipo della classe che contiene le informazioni per la connessione (che variano da applicativo a applicativo)
	//ConfigurationWizard            --> Classe che rappresenta il wizard usato per inserire le informazioni per la connessione (definito nel progetto di Design))
	//ToolboxItem                    --> rappresenta l'icona con la quale verranno mostrati le activity nella toolbox del designer
	[Description("EasyAttachments")]
	[WorkflowObject("123ED732-D3ED-4328-8E1B-93334E8422B3")]
	[WorkflowObjectConfiguration(typeof(ConnectionConfiguration))]
	[ConfigurationWizard("EADesigner.EAConnectionDesigner, EADesigner")]
	[ToolboxItem(typeof(EAToolBoxItemApprove))]
	public class EAConnection: ApplicationProvider
	{

		/// <summary>
		/// Metodo utilizzato per testare la connessione al web service di easyattachment 9in fase di registrazione della connessione
		/// </summary>
		/// <param name="ConfigurationObject"></param>
		/// <returns></returns>
		public override PAT.CRM.WSC4.WorkflowObjects.TestConnectionResult TestConnection(object ConfigurationObject)
		{
			bool result = false;

			//casto Configuration Object alla mia classe ConnectionConfiguration, 
			//utilizzata nel wizard per la registrazione della connessione
			ConnectionConfiguration connectionConf = (ConnectionConfiguration)ConfigurationObject;

			//controllo la connessione chiamando il metodo IsAlive di EASync
			try
			{
				var easyAttachmentSync = new MicroareaEasyAttachmentSync();
				easyAttachmentSync.Url = connectionConf.ServiceUri;
				result = easyAttachmentSync.IsAlive();
			}
			catch(Exception e)
			{
				throw new EAConnectionException("Errore in fase di connessione al webServer di EasyAttachment all'interno del metodo TestConnection", e);
			}
			return new TestConnectionResult { Valid = result, Message = " risultato della chiamata al Web method IsAlive()" };
		}


		public override void DisposeConnection(ApplicationConnection Connection)
		{

		}

		public override PAT.CRM.WSC4.WorkflowObjects.Ws.Assemblys GetAssembly(object ConfigurationObject, string Code)
		{
			var assemblys = new PAT.CRM.WSC4.WorkflowObjects.Ws.Assemblys();
			return assemblys;
		}

		public override void InitConnection(ApplicationConnection Connection)
		{

		}

		public override void RegisterWorkflowEngine(ApplicationConnection Connection)
		{

		}
	}

	/// <summary>
	/// Contiene la stringa di connessione al webservice di EasyAttachment, viene utilizzata per passare il nome dell'installazione
	/// dalla maschera di registrazione al metodo testconnection result
	/// </summary>
	public class ConnectionConfiguration
	{
		public string ServiceUri { get; set; }

		public void SetServiceUri(string server, string installationName)
		{
			this.ServiceUri = "http://" + server + "/" + installationName + "/EasyAttachmentSync/EasyAttachmentSync.asmx?WSDL";
		}
	}
}
