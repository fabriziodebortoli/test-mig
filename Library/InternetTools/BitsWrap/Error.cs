using System;
using System.Threading;

namespace Microarea.Library.Internet.BitsWrap
{
	public class BitsError
	{
		string m_Description;

		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
			}
		}
		internal BitsError(IBackgroundCopyError bErr)
		{
			bErr.GetErrorDescription(Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID), out m_Description);
		}
	}
}