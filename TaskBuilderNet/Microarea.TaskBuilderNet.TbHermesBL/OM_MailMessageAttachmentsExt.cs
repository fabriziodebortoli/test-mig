using System;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public partial class OM_MailMessageAttachments
	{
		static public OM_MailMessageAttachments CreateDBAttachment()
		{
			OM_MailMessageAttachments item = new OM_MailMessageAttachments();
			DateTime now = DateTime.Now;
			item.TBCreated = now;
			item.TBModified = now;
			return item;
		}
	}
}
