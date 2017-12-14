using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	static public class ErrorMessages
	{
		//---------------------------------------------------------------------
		static public string GetErrorMessage(int errorId)
		{
			string msg = null;
			string msgID = string.Format(CultureInfo.InvariantCulture, "PL{0:0000}", Math.Abs(errorId));
			try
			{
				msg = LocalizedPLErrorMessages.ResourceManager.GetString(msgID);
			}
			catch { } // alla peggio mi becco la versione cablata

			return msg == null
				? string.Format(CultureInfo.InvariantCulture, LocalizedStrings.PostaLiteErrorNoTextFoundMask, errorId)
				: string.Format(CultureInfo.InvariantCulture, LocalizedStrings.PostaLiteErrorMask, msg, errorId);
		}
	}
}
