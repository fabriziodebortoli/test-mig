using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	public class AgreementsManager
	{
		protected readonly string licensesPath;
		public const string AgreementExtension			= ".xhtml";

		//---------------------------------------------------------------------
		public AgreementsManager(string licensesPath)
		{
			this.licensesPath = licensesPath;
		}

		//---------------------------------------------------------------------
		public string GetAgreementFileName(string product, Agreements agreement, string isoCountry)
		{
			if (isoCountry == null || isoCountry.Length == 0)
				return GetGenericAgreementFileName(agreement, product);
			else
				return GetAgreementFileName(agreement, product, isoCountry);
		}

		//---------------------------------------------------------------------
		public string GetAgreementFilePath(string product, Agreements agreement, string isoCountry)
		{
			// if country-specific file exists, returns it
			// else if continent-specific file exists, returns it
			// else return generic all-purpose english file
			string genericFileName = GetGenericAgreementFileName(agreement, product);			// search for generic
			string specificFileName = 
				(isoCountry != null && isoCountry.Length != 0) ? 
				GetAgreementFileName(agreement, product, isoCountry):	// search for specific
				genericFileName;
	
			// locate agreement file
			string filePath = Path.Combine(licensesPath, specificFileName);
			if (!File.Exists(filePath))
			{
				string contFileName = null;
				if (isoCountry != null && isoCountry.Length != 0)
				{
					Asia asia = new Asia();
					if (asia.Contains(isoCountry))
					{
						contFileName = GetAgreementFileName(agreement, product, asia.Name);
						string asianFilePath = Path.Combine(licensesPath, contFileName);
						if (File.Exists(asianFilePath))
							return asianFilePath;
					}
				}

				filePath = Path.Combine(licensesPath, genericFileName);
				if (!File.Exists(filePath))
					return string.Empty;
			}
			return filePath;
		}

		public static string GetAgreementFileName(Agreements agreement, string product, string isoCountry)
		{
			return string.Concat(product, ".", agreement.ToString(), ".", isoCountry, AgreementExtension);
		}
		public static string GetGenericAgreementFileName(Agreements agreement, string product)
		{
			return string.Concat(product, ".", agreement.ToString(), AgreementExtension);
		}

		//---------------------------------------------------------------------
	}
	public enum Agreements {Privacy, Eula, Mlu, Mgs}

	
	//======================================================================
	public abstract class Continent
	{
		private readonly string name;
		private Hashtable continent;
		public Continent(string name, string[] countries)
		{
			this.name = name;
			continent = new Hashtable
				(
				countries.Length, 
				StringComparer.InvariantCultureIgnoreCase
				);
			foreach (string iso in countries)
				continent[iso] = iso;
			Debug.WriteLine("countries.Length: " + countries.Length);
			Debug.WriteLine("continent.Count: " + continent.Count);
		}
		public string Name { get { return this.name; } }
		public bool Contains(string country)
		{
			return continent.Contains(country);
		}
	}
	public class Asia : Continent
	{
		// NOTE: some are repeated (e.g. Yemen)
		public static string[] asianCountries = 
			{
				//	Eastern Asia: 
				"CN",//	China
				"HK",//	China, Hong Kong SAR
				"MO",//	China, Macao SAR
				"TW",//	China, Twain
				"KP",//	Democratic People’s Republic of Korea
				"JP",//	Japan
				"MN",//	Mongolia
				"KR",//	Republic of Korea
				
				//	South-central Asia: 
				"AF",//	Afghanistan
				"BD",//	Bangladesh
				"BT",//	Bhutan
				"IN",//	India
				"IR",//	Iran (Islamic Republic of)
				"KZ",//	Kazakhstan
				"KG",//	Kyrgyzstan
				"MV",//	Maldives
				"NP",//	Nepal
				"PK",//	Pakistan
				"LK",//	Sri Lanka
				"TJ",//	Tajikistan
				"TM",//	Turkmenistan
				"UZ",//	Uzbekistan
				
				//	South-eastern Asia:
				"BN",// 	Brunei Darussalam
				"KH",//	Cambodia
				"TL",//	Democratic Republic of Timor-Leste
				"ID",//	Indonesia
				"LA",//	Lao People’s Democratic Republic
				"MY",//	Malaysia
				"MM",//	Myanmar
				"PH",//	Philippines
				"SG",//	Singapore
				"TH",//	Thailand
				"VN",//  	Vietnam
				
				//	Western Asia:
				"AM",//	Armenia
				"AZ",//	Azerbaijan
				"BH",//	Bahrain
				//"CY",//	Cyprus //Cyprus is under European domain
				"GE",//	Georgia
				"IQ",//	Iraq
				"IL",//	Israel
				"JO",//	Jordan
				"KW",//	Kuwait
				"LB",//	Lebanon
				"PS",//	Occupied Palestinian Territory
				"OM",//	Oman
				"QA",//	Qatar
				"SA",//	Saudi Arabia
				"SY",//	Syrian Arab Republic
				//"TR",//	Turkey - commented out: Turkey is under European domain
				"AE",//	United Arab Emirates
				"YE",//	Yemen

				//	Middle East:
				"BH",//	Bahrain
				"IQ",//	Iraq
				"IL",//	Israel
				"JO",//	Jordan
				"KW",//	Kuwait
				"LB",//	Lebanon
				"PS",//	Occupied Palestine Territory
				"OM",//	Oman
				"QA",//	Qatar
				"SA",//	Saudi Arabia
				"SY",//	Syria
				"AE",//	United Arab Emirates
				"YE"//	Yemen
			};
		public Asia() : base("asia", asianCountries)
		{
		}
//
//		private static Asia asia;
//		static Asia()
//		{
//			if (asia == null)
//				asia = new Asia();
//			asia;
//		}
	}

}
