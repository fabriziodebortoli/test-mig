using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for LocaleIDManager.
	/// </summary>
	public class LocaleInfo
	{
		private readonly string languageIdentifier;			// LOCALE_ILANGUAGE
		private readonly string languageOSLocalizedName;	// LOCALE_SLANGUAGE
		private readonly string abbreviatedLanguageName;	// LOCALE_SABBREVLANGNAME
		private readonly string languageNativeName;			// LOCALE_SNATIVELANGNAME
		private readonly string countryPhoneCode;			// LOCALE_ICOUNTRY
		private readonly string countryOSLocalizedName;		// LOCALE_SCOUNTRY
		//			LOCALE_SABBREVCTRYNAME	= 7,
		private readonly string countryNativeName;			// LOCALE_SNATIVECTRYNAME
		private readonly string defaultLanguageID;			// LOCALE_IDEFAULTLANGUAGE
		private readonly string defaultCountryCode;			// LOCALE_IDEFAULTCOUNTRY
		private readonly string defaultCodePage;			// LOCALE_IDEFAULTCODEPAGE
		private readonly string iso639LanguageName;			// LOCALE_SISO639LANGNAME	= 89,	// Windows 98/Me, Windows NT 4.0 and later: The abbreviated name of the language based entirely on the ISO Standard 639 values.
		private readonly string iso3166CountryCode;			// LOCALE_SISO3166CTRYNAME	= 90,	// Windows 98/Me, Windows NT 4.0 and later: Country/region name, based on ISO Standard 3166.
		private readonly string languageEnglishName;		// LOCALE_SENGLANGUAGE
		private readonly string countryEnglishName;			// LOCALE_SENGCOUNTRY
		private readonly string defaultAnsiCodePage;		// LOCALE_IDEFAULTANSICODEPAGE

		//---------------------------------------------------------------------
		public LocaleInfo(int locale)
		{
			this.languageIdentifier			= GetLocaleInfo(locale, LCTYPE.LOCALE_ILANGUAGE);
			this.languageOSLocalizedName	= GetLocaleInfo(locale, LCTYPE.LOCALE_SLANGUAGE);
			this.abbreviatedLanguageName	= GetLocaleInfo(locale, LCTYPE.LOCALE_SABBREVLANGNAME);
			this.languageNativeName			= GetLocaleInfo(locale, LCTYPE.LOCALE_SNATIVELANGNAME);
			this.countryPhoneCode			= GetLocaleInfo(locale, LCTYPE.LOCALE_ICOUNTRY);
			this.countryOSLocalizedName		= GetLocaleInfo(locale, LCTYPE.LOCALE_SCOUNTRY);
			this.countryNativeName			= GetLocaleInfo(locale, LCTYPE.LOCALE_SNATIVECTRYNAME);
			this.defaultLanguageID			= GetLocaleInfo(locale, LCTYPE.LOCALE_IDEFAULTLANGUAGE);
			this.defaultCountryCode			= GetLocaleInfo(locale, LCTYPE.LOCALE_IDEFAULTCOUNTRY);
			this.defaultCodePage			= GetLocaleInfo(locale, LCTYPE.LOCALE_IDEFAULTCODEPAGE);
			this.iso639LanguageName			= GetLocaleInfo(locale, LCTYPE.LOCALE_SISO639LANGNAME);
			this.iso3166CountryCode			= GetLocaleInfo(locale, LCTYPE.LOCALE_SISO3166CTRYNAME);
			this.languageEnglishName		= GetLocaleInfo(locale, LCTYPE.LOCALE_SENGLANGUAGE);
			this.countryEnglishName			= GetLocaleInfo(locale, LCTYPE.LOCALE_SENGCOUNTRY);
			this.defaultAnsiCodePage		= GetLocaleInfo(locale, LCTYPE.LOCALE_IDEFAULTANSICODEPAGE);
		}

		//---------------------------------------------------------------------
		public string Iso3166CountryCode	{ get { return this.iso3166CountryCode; } }

		/*
		//---------------------------------------------------------------------
		public void Test()
		{
			System.Globalization.CultureInfo chs = new System.Globalization.CultureInfo("zh-chs");	// 0x0004 (4)
			System.Globalization.CultureInfo cht = new System.Globalization.CultureInfo("zh-cht");	// 0x7C04 (31748)
			System.Globalization.CultureInfo ci1 = new System.Globalization.CultureInfo(1040);
			System.Globalization.CultureInfo ci2 = new System.Globalization.CultureInfo(2052);	// Chinese - China
			System.Globalization.CultureInfo ci3 = new System.Globalization.CultureInfo(1028);	// Chinese - Taiwan
			System.Globalization.CultureInfo ci4 = new System.Globalization.CultureInfo("en-gb");

			PrintLocaleStuff(1033);
			PrintLocaleStuff(2057);
			PrintLocaleStuff(1040);
			PrintLocaleStuff(2052);
			PrintLocaleStuff(1028);
		}
		
		//---------------------------------------------------------------------
		private void PrintLocaleStuff(int locale)
		{
			//			int len = 2;
			//			StringBuilder isoSB = new StringBuilder(len);
			//			if (GetLocaleInfo(locale, (int)LCTYPE.LOCALE_SISO3166CTRYNAME, isoSB, len + 1) == 0)
			//				throw new Win32Exception(Marshal.GetLastWin32Error());
			//			len = 6;
			//			StringBuilder langSB = new StringBuilder(len);
			//			if (GetLocaleInfo(locale, (int)LCTYPE.LOCALE_SISO639LANGNAME, langSB, len + 1) == 0)
			//				throw new Win32Exception(Marshal.GetLastWin32Error());
			//
			//			string iso = isoSB.ToString();
			//			Debug.WriteLine(string.Format("ISO 3166 (2-letters) for locale {0} is {1}", locale, iso));
			//
			//			string lang = langSB.ToString();
			//			Debug.WriteLine(string.Format("ISO 639 for locale {0} is {1}", locale, lang));

			int len = 100;
			StringBuilder sb = new StringBuilder(len);
			foreach (LCTYPE lcType in Enum.GetValues(typeof(LCTYPE)))
			{
				if (GetLocaleInfo(locale, (int)lcType, sb, len + 1) == 0)
					throw new Win32Exception(Marshal.GetLastWin32Error());
				Debug.WriteLine(string.Format("{0} for locale {1} is {2}", lcType.ToString(), locale, sb.ToString()));
			}
		}
		*/

		//---------------------------------------------------------------------
		private string GetLocaleInfo(int locale, LCTYPE localeType)
		{
			int len = 100;
			StringBuilder sb = new StringBuilder(len);
			if (GetLocaleInfo(locale, (int)localeType, sb, len + 1) == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
			return sb.ToString();
		}

		//---------------------------------------------------------------------
		private enum LCTYPE	// Locale Information
		{
			LOCALE_ILANGUAGE		= 1,
			LOCALE_SLANGUAGE		= 2,
			LOCALE_SABBREVLANGNAME	= 3,
			LOCALE_SNATIVELANGNAME	= 4,
			LOCALE_ICOUNTRY			= 5,
			LOCALE_SCOUNTRY			= 6,
			LOCALE_SABBREVCTRYNAME	= 7,
			LOCALE_SNATIVECTRYNAME	= 8,
			LOCALE_IDEFAULTLANGUAGE	= 9,
			LOCALE_IDEFAULTCOUNTRY	= 10,
			LOCALE_IDEFAULTCODEPAGE	= 11,
			LOCALE_SISO639LANGNAME	= 89,			// Windows 98/Me, Windows NT 4.0 and later: The abbreviated name of the language based entirely on the ISO Standard 639 values.
			LOCALE_SISO3166CTRYNAME	= 90,			// Windows 98/Me, Windows NT 4.0 and later: Country/region name, based on ISO Standard 3166.
			LOCALE_SENGLANGUAGE		= 0x1001,
			LOCALE_SENGCOUNTRY		= 0x1002,
			LOCALE_IDEFAULTANSICODEPAGE = 0x1004
		}

		//---------------------------------------------------------------------
		// Fred: Works as unicode for NT and above :-)
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern int GetLocaleInfo(
			int Locale,				// locale identifier
			int LCType,				// information type
			StringBuilder lpLCData,	// information buffer
			int cchData				// size of buffer
			);
		//int GetLocaleInfo(
		//	LCID Locale,      // locale identifier
		//	LCTYPE LCType,    // information type
		//	LPTSTR lpLCData,  // information buffer
		//	int cchData       // size of buffer
		//	);
	}
}
