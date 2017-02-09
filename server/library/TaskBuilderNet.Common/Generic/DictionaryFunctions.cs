using System;
using System.Globalization;
using System.Threading;

namespace Microarea.Common.Generic
{
	/// <summary>
	/// Funzioni generiche relative ai dizionari e culture infos
	/// </summary>
	public class DictionaryFunctions
	{
		//-----------------------------------------------------------------------
		public static void SetCultureInfo(string primaryCulture, string applicationCulture)
		{
			SetCultureInfo(primaryCulture);
			SetApplicationCultureInfo(applicationCulture);
		}

		//-----------------------------------------------------------------------
		private static bool SetCultureInfo(string culture)
		{

			try
			{
				CultureInfo ci = new CultureInfo(culture);

				//if (ClickOnceDeploy.IsClickOnceClient)
				//	ClickOnceDeploy.DownloadGroup(culture, string.Format(GenericStrings.Dictionaries, ci.DisplayName));
				
               // Thread.CurrentThread.CurrentUICulture = ci; todo rsweb
			}
			catch
			{
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		private static bool SetApplicationCultureInfo(string culture)
		{
			try
			{
				CultureInfo ci =  new CultureInfo(culture);
				//Thread.CurrentThread.cCurrentCulture = ci;          TODO rsweb
			}
			catch
			{
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------------
		public static bool IsAsianCulture()
		{
            string name = "";// TODO rsweb Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

			return string.Compare(name, "zh", StringComparison.OrdinalIgnoreCase) == 0;
		}

	}
	
}
