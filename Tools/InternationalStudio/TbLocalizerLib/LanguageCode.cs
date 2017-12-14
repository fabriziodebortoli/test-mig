using System;
using System.Collections;
using System.Globalization;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Gestisce la lista dei linguaggi disponibili coi loro codici.
	/// </summary>
	//=========================================================================
	public class LanguageManager
	{
		/// <summary>
		/// Restituisce un array di LanguageInfo completo.
		/// </summary>
		//---------------------------------------------------------------------
		public static CultureInfo[] GetAllCode()
		{
			return GetAllCode(String.Empty);
		}

		/// <summary>
		/// Restituisce un array di LanguageInfo completo, eventualmente privato della lingua specificata.
		/// </summary>
		/// <param name="languageToRemove">Codice della lingua da rimuovere dalla lista.</param>
		//---------------------------------------------------------------------
		public static CultureInfo[] GetAllCode(string languageToRemove)
		{
			ArrayList languageCodes = new ArrayList();
			
			foreach (CultureInfo cInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
				if (String.Compare(languageToRemove, cInfo.Name, true) != 0)
					languageCodes.Add(new CultureInfo(cInfo.Name));
			return (CultureInfo[])languageCodes.ToArray(typeof(CultureInfo));
		}

		//--------------------------------------------------------------------------------
		public static CultureInfo[] GetAllCode(LocalizerTreeNode[] cultureTreeNodes)
		{
			ArrayList languageCodes = new ArrayList();
			
			foreach (LocalizerTreeNode n in cultureTreeNodes)
			{
				string culture = n.Name;
				bool exist = false;
				foreach (CultureInfo ci in languageCodes)
					if (string.Compare(ci.Name, culture, true) == 0)
					{
						exist = true;
						break;
					}
				
				if (!exist)
					languageCodes.Add(new CultureInfo(culture));
			}
			return (CultureInfo[])languageCodes.ToArray(typeof(CultureInfo));
		}

		/// <summary>
		/// Dal codice della lingua restituisce la descrizione
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetDescriptionByCode(string code)
		{	
			try
			{
				CultureInfo ci = new CultureInfo(code);
				return ci.DisplayName;
			}
			catch {return String.Empty;}
		}
	}

	
}
