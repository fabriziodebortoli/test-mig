using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PAT.Workflow.Runtime;
using System.Drawing.Design;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Activities;
using PAT.CRM.WSC4.WorkflowObjects;
using EAConnector.Exceptions;

namespace EAConnector.Activities {

	public class EAActivity: WorkflowActivity {

		[Category("Connection")]
		public InArgument<ConnectionReference<EAConnection>> Connection { get; set; }
		
		/// <summary>
		/// Questo metodo viene utilizzato per recuperare le informazioni sulla connessione, 
		/// ovvero per chiamare i metodi di EasyAttachment (approvazione e rifiuto allegato)
		/// </summary>
		/// <param name="Context"></param>
		/// <returns></returns>
		protected ConnectionConfiguration GetConnectionConfiguration(NativeActivityContext Context) {
			ConnectionReference reference = Context.GetValue(Connection);
			//obtain the connection details
			ApplicationConnection appConnection =
				ConnectionRepository.Instance.GetConnectionByCode(reference.ApplicationProviderID, reference.Code);
			ConnectionConfiguration config = null;
			if (appConnection != null)
				config =
					(ConnectionConfiguration)
					ApplicationProviderRepository.Current.GetApplicationProviderItem(reference.ApplicationProviderID)
												.ApplicationProvider.GetConnectionConfiguration(appConnection);
			return config;
		}

		protected override void Execute(NativeActivityContext context) {
		}
	}

    
    
	internal class EAToolBoxItemApprove : ToolboxItem {
		public EAToolBoxItemApprove() {
			try {
                Bitmap = EAConnector.Properties.Resources.magonet_approve;
			} catch (Exception e) {
				throw new EAFileLoadException("Errore nel caricamento dell'immagine magonet_approve.png per la toolbox", e); 
			}
		}
	}

	internal class EAToolBoxItemReject : ToolboxItem {
		public EAToolBoxItemReject() {
			try {
                Bitmap = EAConnector.Properties.Resources.magonet_reject;
			} catch (Exception e) {
				throw new EAFileLoadException("Errore nel caricamento dell'immagine magonet_reject.png per la toolbox", e);
			}
		}
	}

	internal class EAToolBoxItemStandard : ToolboxItem {
		public EAToolBoxItemStandard() {
			try {
                Bitmap = EAConnector.Properties.Resources.magonet_std;
			} catch (Exception e) {
				throw new EAFileLoadException("Errore nel caricamento dell'immagine magonet_std.png per la toolbox", e);
			}
		}
	}
}
