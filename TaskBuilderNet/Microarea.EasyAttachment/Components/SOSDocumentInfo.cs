using System;
using System.Data.SqlTypes;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// Classe di appoggio con le info minime per un sos document
	///</summary>
	//================================================================================
	public class SOSDocumentInfo
	{
		public int AttachmentId { get; set; }
		
		public string FileName { get; set; }
		public long Size { get; set; }
		public string DescriptionKeys { get; set; }
		public string HashCode { get; set; }
		public StatoDocumento DocumentStatus { get; set; }
		public string AbsoluteCode { get; set; }
		public string LotID { get; set; }

		public DateTime ArchivedDate = (DateTime)SqlDateTime.MinValue;
		public DateTime RegistrationDate = (DateTime)SqlDateTime.MinValue;

		public int EnvelopeID { get; set; }

		public string TaxJournal { get; set; }
		public string DocumentType { get; set; }
		public string FiscalYear { get; set; }
		public SendingType SendingType { get; set; }

		// per il caricamento on demand del binario (al momento non serve)
		//private byte[] pdfaBinary = null;
		//public bool VeryLargeFile = false;

		private DMSOrchestrator dmsOrchestrator = null;

		//---------------------------------------------------------------------
		public SOSDocumentInfo(int attachmentId, DMSOrchestrator dmsOrch)
		{
			dmsOrchestrator = dmsOrch;
			AttachmentId = attachmentId;
		}
	}
}
