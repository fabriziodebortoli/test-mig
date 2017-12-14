using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using System.Activities.Validation;
using System.ServiceModel.Channels;
using System.IO;
using EAConnector.EASync;
using EAConnector.Exceptions;

namespace EAConnector.Activities.EATestActivity {
	[ToolboxItem(typeof(EAToolBoxItemStandard))]
	[Designer("EADesigner.ActivitiesDesigner.EAActivityControlStandard, EADesigner")]
	[Description("Easy Attachment's IsAlive")]
	public class EasyAttachmentIsAlive : EAActivity {

		protected override void Execute(NativeActivityContext context) {
			
			try {
				var easyAttachmentSync = new MicroareaEasyAttachmentSync();
                easyAttachmentSync.Url = GetConnectionConfiguration(context).ServiceUri;
                easyAttachmentSync.IsAlive();
            }
            catch (Exception e)
            {
				throw new EAConnectionException("Errore in fase di connessione al web server di EasyAttachment, durante il tentativo di chiamare il metodo Approve", e);
			}
		}
	}
}