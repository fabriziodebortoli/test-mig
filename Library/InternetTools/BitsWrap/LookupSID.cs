using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.Internet.BitsWrap
{

	public class LookupSID
	{

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern bool ConvertStringSidToSidW(string stringSID, IntPtr SID);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern bool LookupAccountSidW
			(string lpSystemName, 
			IntPtr SID, 
			StringBuilder name, 
			long cbName, 
			StringBuilder domainName, 
			long cbDomainName, 
			int psUse
			);

		public static string GetName(string SID)
		{
			int size = 255;
			string domainName;
			string userName;
			long cbUserName = size;
			long cbDomainName = size;
			IntPtr ptrSID = IntPtr.Zero;
			int psUse = 0;
			StringBuilder bufName = new StringBuilder(size);
			StringBuilder bufDomain = new StringBuilder(size);
			if (ConvertStringSidToSidW(SID, ptrSID))
			{
				if (LookupAccountSidW(string.Empty, 
					ptrSID, bufName, 
					cbUserName, bufDomain, 
					cbDomainName, psUse))
				{
					userName = bufName.ToString();
					domainName = bufDomain.ToString();
					return string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", domainName, userName);
				}
				else
					return "";
			}
			else
				return "";
		}
	}
}