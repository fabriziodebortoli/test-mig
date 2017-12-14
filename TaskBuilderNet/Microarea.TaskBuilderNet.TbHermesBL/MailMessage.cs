using System;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public class MailMessage
	{
		public string From { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public DateTime Sent { get; set; }
	}
}
