using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IInvalidDocumentNotifier
	{
		void LogInvalidDocumentFormat
			(
			string loginID,
			string userID,
			int lotID,
			string[] details
			);
		string Url { get; set; }
	}
}
