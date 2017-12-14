using System;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Core;

namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// Classe di appoggio con le info minime per un OCR template
	///</summary>
	//================================================================================
	public class OCRTemplateInfo
	{
		private DMSOrchestrator dmsOrchestrator = null;

		public BookmarksDataTable FieldsDT = null;
		public DMS_Collection Template = null;
		public string NewTemplateName = string.Empty;

		//---------------------------------------------------------------------
		public OCRTemplateInfo(DMSOrchestrator dmsOrch, int attachmentID)
		{
			dmsOrchestrator = dmsOrch;

			//Template = dmsOrchestrator.OCRManager.GetOCRTemplate(attachmentID);
			//FieldsDT = dmsOrchestrator.OCRManager.GetOCRFields(Template);

			NewTemplateName = Template.TemplateName;
		}

		//---------------------------------------------------------------------
		internal bool HasChanges()
		{
			if (string.Compare(NewTemplateName, Template.TemplateName, StringComparison.InvariantCultureIgnoreCase) != 0 ||
				FieldsDT.GetChanges() != null)
				return true;

			return false;
		}
	}
}
