using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime Now { get { return DateTime.Now; } }
	}
}
