using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL.Exceptions
{
	public abstract class PostaLiteException : ApplicationException
	{
		public PostaLiteException(string message, string company)
			: base(message)
		{
			this.Company = company;
		}
		public string Company { get; private set; }
	}
}
