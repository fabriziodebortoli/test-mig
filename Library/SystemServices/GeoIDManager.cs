using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Microarea.Library.SystemServices
{
	public class GeoIDManager
	{
//		private readonly string nation;				// GEO_NATION      =       0x0001,
//		private readonly string latitude;			// GEO_LATITUDE    =       0x0002,
//		private readonly string longitude;			// GEO_LONGITUDE   =       0x0003,
		private readonly string iso2;				// GEO_ISO2        =       0x0004,
		private readonly string iso3;				// GEO_ISO3        =       0x0005,
		//private readonly string rfc1766;			// GEO_RFC1766     =       0x0006,
//		private readonly string lcid;				// GEO_LCID        =       0x0007,
		private readonly string friendlyName;		// GEO_FRIENDLYNAME=       0x0008,
		private readonly string officialName;		// GEO_OFFICIALNAME=       0x0009,
//		private readonly string timeZones;			// GEO_TIMEZONES   =       0x000A,
//		private readonly string officialLanguages;	// GEO_OFFICIALLANGUAGES = 0x000B

		//---------------------------------------------------------------------
		public GeoIDManager(int geoId)
		{
			this.iso2			= GetGeoInfo(geoId, SYSGEOTYPE.GEO_ISO2, 2);
			this.iso3			= GetGeoInfo(geoId, SYSGEOTYPE.GEO_ISO3, 3);
			//this.rfc1766		= GetGeoInfo(geoId, SYSGEOTYPE.GEO_RFC1766, 100); // BUGGY ("it-cn" on my machine for CN 45 geoId)
			this.friendlyName	= GetGeoInfo(geoId, SYSGEOTYPE.GEO_FRIENDLYNAME, 100);
			this.officialName	= GetGeoInfo(geoId, SYSGEOTYPE.GEO_OFFICIALNAME, 100);
		}

		public string TwoLettersIsoCountryCode		{ get { return this.iso2; } }
		public string ThreeLettersIsoCountryCode	{ get { return this.iso3; } }
		public string FriendlyName					{ get { return this.friendlyName; } }
		public string OfficialName					{ get { return this.officialName; } }

		//---------------------------------------------------------------------
		static public string GetCurrentSystemIso3166CountryCode()
		{
			// current geoID is read from registry
			// HKEY_CURRENT_USER\Control Panel\International\Geo\Nation
			// if the registy key is not present (e.g. OSs before WinXP, Me...)
			// then infer it from the current culture localeID

			int geoId = GetCurrentSystemGeoID();
			if (geoId != -1)
				try
				{
					return GetGeoInfo(geoId, SYSGEOTYPE.GEO_ISO2, 2);
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.ToString());
					return ReadIso3166CountryCodeFromCurrentLocale();
				}
			else // then infer it from the current culture localeID
				return ReadIso3166CountryCodeFromCurrentLocale();
		}

		//---------------------------------------------------------------------
		static string ReadIso3166CountryCodeFromCurrentLocale()
		{
			int locale = CultureInfo.CurrentCulture.LCID;
			LocaleInfo li = new LocaleInfo(locale);
			return li.Iso3166CountryCode;
		}

		//---------------------------------------------------------------------
		static int GetCurrentSystemGeoID()
		{
			string regKey = "Control Panel\\International\\Geo";
			RegistryKey geoKey = Registry.CurrentUser.OpenSubKey(regKey);
			if (geoKey == null)
				return -1;

			string s = geoKey.GetValue("Nation") as string;
			if (s == null)
				return -1;
			int geoId = -1;
			try
			{
				geoId = Int32.Parse(s, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
			}
			return geoId;
		}

		//---------------------------------------------------------------------
		static public void Test()
		{
			Debug.WriteLine("geoId\tiso\tfname\tname");
			// Enumerate all the Geo IDs available on the System.
			EnumGeoInfoProcDelegate del = new EnumGeoInfoProcDelegate(EnumGeoInfoProc);
			int i = (int)SYSGEOCLASS.GEOCLASS_NATION;
			EnumSystemGeoID(i, 0, del);
		}

		//---------------------------------------------------------------------
		private static string GetGeoInfo(int geoId, SYSGEOTYPE geoType, int len)
		{
			StringBuilder sb = new StringBuilder(len);
			if (GetGeoInfo(geoId, geoType, sb, len + 1, 0x0000) == 0)
				//throw new Win32Exception(Marshal.GetLastWin32Error()); // commented out: api docs doesn't state it, set SetLastError=false
				return string.Empty;
			return sb.ToString();
		}

		//---------------------------------------------------------------------
		delegate bool EnumGeoInfoProcDelegate(int geoId);
		static private bool EnumGeoInfoProc(int geoId)
		{
			string iso = GetGeoInfo(geoId, SYSGEOTYPE.GEO_ISO2, 2);
			string name = GetGeoInfo(geoId, SYSGEOTYPE.GEO_OFFICIALNAME, 100);
			string fname = GetGeoInfo(geoId, SYSGEOTYPE.GEO_FRIENDLYNAME, 100);
			Debug.WriteLine(geoId + "\t" + iso + "\t" + fname + "\t" + name);
			return true;
		}

		//---------------------------------------------------------------------
		// Fred: Works only for Me, XP and 2003
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool EnumSystemGeoID
			(
			int GeoClass,							// geographical type to enumerate
			int  ParentGeoId,						// parent GeoID
			EnumGeoInfoProcDelegate lpGeoEnumProc	// callback function
			);
		//BOOL EnumSystemGeoID(
		//	GEOCLASS GeoClass,       // geographical type to enumerate
		//	GEOID  ParentGeoId,         // parent GeoID
		//	GEO_ENUMPROC lpGeoEnumProc  // callback function
		//	);

		//---------------------------------------------------------------------
		// Fred: Works only for Me, XP and 2003
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=false)]
		private static extern int GetGeoInfo
			(
			int GeoId,					// location identifier
			SYSGEOTYPE GeoType,			// type of information requested
			StringBuilder lpGeoData,	// buffer for results
			int cchData,				// size of buffer
			ushort language				// language id
			);
		//int GetGeoInfo(
		//	GEOID GeoId,         // location identifier
		//	GEOTYPE GeoType,     // type of information requested
		//	LPTSTR lpGeoData,    // buffer for results
		//	int cchData,         // size of buffer
		//	LANGID language      // language id
		//	);

		//---------------------------------------------------------------------
		enum SYSGEOTYPE
		{
			GEO_NATION      =       0x0001,
			GEO_LATITUDE    =       0x0002,
			GEO_LONGITUDE   =       0x0003,
			GEO_ISO2        =       0x0004,
			GEO_ISO3        =       0x0005,
			GEO_RFC1766     =       0x0006,
			GEO_LCID        =       0x0007,
			GEO_FRIENDLYNAME=       0x0008,
			GEO_OFFICIALNAME=       0x0009,
			GEO_TIMEZONES   =       0x000A,
			GEO_OFFICIALLANGUAGES = 0x000B
		}

		//---------------------------------------------------------------------
		enum SYSGEOCLASS
		{
			GEOCLASS_NATION  = 16,
			GEOCLASS_REGION  = 14
		}
	}
}
