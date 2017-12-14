using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.TbSenderBL.Pdf
{
	public class PdfCoverData
	{
		public string FaxText { get; set; }
		public string SenderMultiRow { get; set; }
		public string[] AddressRows { get; set; }
		public List<string> DocDescriptions { get; set; }
		public string InAttachTextLocalized { get; set; }
		public bool DrawRectangles { get; set; }
		public bool DrawRulers { get; set; }
	}
}
