using System.Globalization;
using System.Threading;

namespace Microarea.TaskBuilderNet.Core.Generic
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
				if (ClickOnceDeploy.IsClickOnceClient)
					ClickOnceDeploy.DownloadGroup(culture, string.Format(GenericStrings.Dictionaries, ci.DisplayName));
				Thread.CurrentThread.CurrentUICulture = ci;
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
				CultureInfo ci =  CultureInfo.CreateSpecificCulture(culture);
				Thread.CurrentThread.CurrentCulture = ci;
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
			string name = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

			return string.Compare(name, "zh", true, CultureInfo.InvariantCulture) == 0;
		}

	}
	
}
