using Limilabs.Mail;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public class MailHolder
	{
		public IMail Mail { get; set; }
		public OM_MailMessagePop3 Pop3Uid { get; set; }
		public IOM_MailAccount Account { get; set; }
	}
}
