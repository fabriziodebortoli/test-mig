using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microarea.WebServices.TbHermes
{
	public class ApplicationHolder
	{
		public ApplicationHolder(HttpApplicationState application)
		{
			this.Application = application;
		}
		public HttpApplicationState Application { get; internal set; }
	}
}