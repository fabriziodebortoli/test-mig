using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using System.Activities.Validation;
using System.ServiceModel.Channels;
using EAConnector.EASync;
using System.IO;
using EAConnector.Exceptions;

namespace EAConnector.Activities.ActivitiesById {
	[ToolboxItem(typeof(EAToolBoxItemStandard))]
	[Designer("EADesigner.ActivitiesDesigner.EAActivityControlStandard, EADesigner")]
	[Description("Get the Attachment Description by Id")]
	public class GetDescriptionById : EAActivity {
		
		[Category("Input Fields"), RequiredArgument()]
		public InArgument<int> attachmentId { get; set; }
		[Category("Input Fields"), RequiredArgument()]
		public InArgument<string> requesterUserName { get; set; }

		[Category("Output Fields")]
		public OutArgument<string> Description { get; set; }

		protected override void CacheMetadata(NativeActivityMetadata metadata) {
			base.CacheMetadata(metadata);
			if (attachmentId == null)
				metadata.AddValidationError(
											 new ValidationError("Please specify the input object or at least one of the other input variables!", false,
																"MyApplicationObject"));
		}
		protected override void Execute(NativeActivityContext context) {

			try {
				var easyAttachmentSync = new MicroareaEasyAttachmentSync();
                easyAttachmentSync.Url = GetConnectionConfiguration(context).ServiceUri;
                var description= easyAttachmentSync.GetDescription(attachmentId.Get(context), requesterUserName.Get(context));
                Description.Set(context, description);
			} catch (Exception e) {
				throw new EAConnectionException("Errore in fase di connessione al web server di EasyAttachment, durante il tentativo di chiamare il metodo GetDescription", e);
			}

		}
	}
}