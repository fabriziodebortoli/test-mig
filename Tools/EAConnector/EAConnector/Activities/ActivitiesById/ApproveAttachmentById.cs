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

namespace EAConnector.Activities.ActivitiesById {
	[ToolboxItem(typeof(EAToolBoxItemApprove))]
	[Designer("EADesigner.ActivitiesDesigner.EAActivityControlApprove, EADesigner")]
	[Description("Approve Attachment by Id")]
	public class ApproveAttachmentById : EAActivity {
		
		[Category("Input Fields"), RequiredArgument()]
		public InArgument<int> attachmentId { get; set; }
		[Category("Input Fields"), RequiredArgument()]
        public InArgument<string> requesterUserName { get; set; }
        [Category("Input Fields"), RequiredArgument()]
        public InArgument<string> approvalUserName { get; set; }
        [Category("Input Fields")]
        public InArgument<string> approvalComments { get; set; }
		

		protected override void CacheMetadata(NativeActivityMetadata metadata) {
			base.CacheMetadata(metadata);
			if (attachmentId == null || requesterUserName==null || approvalUserName==null)
				metadata.AddValidationError(
											 new ValidationError("Please specify the input object or at least one of the other input variables!", false,
																"MyApplicationObject"));
		}
		protected override void Execute(NativeActivityContext context) {

			try {
                var easyAttachmentSync = new MicroareaEasyAttachmentSync();
                easyAttachmentSync.Url = GetConnectionConfiguration(context).ServiceUri;
				easyAttachmentSync.Approve(attachmentId.Get(context), requesterUserName.Get(context), approvalUserName.Get(context), 1, approvalComments.Get(context) );
			} catch (Exception e) {
				throw new EAConnectionException("Errore in fase di connessione al web server di EasyAttachment, durante il tentativo di chiamare il metodo Approve", e);
			}
		}
	}
}