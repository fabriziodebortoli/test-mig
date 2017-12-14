using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyAttachment.Core
{
	///<summary>
	/// Oggetto con le informazioni di un dizionario ad uso dell'OCR
	///</summary>
	//================================================================================
	public class OCRDictionary
	{
		private string name;
		private string path;

		//--------------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }

		//--------------------------------------------------------------------------------
		public string Path 
		{ 
			get { return path; } 
			set 
			{ 
				path = value;
				CultureName = string.IsNullOrWhiteSpace(path) ? string.Empty : System.IO.Path.GetFileName(path);
			} 
		}

		//--------------------------------------------------------------------------------
		public string CultureName { get; private set; }

		//--------------------------------------------------------------------------------
		public OCRDictionary()
		{
		}
	}

	///<summary>
	/// Helper che consente, dato un LCID, di individuare il path del dizionario OCR da utilizzare
	///</summary>
	//================================================================================
	public class OCRDictionaryHelper
	{
		private const string suffixOcrDictPath = @"TaskBuilder\Extensions\EasyAttachment\OCR";

		// nomi dizionari OCR riconosciuti da GdPicture.NET
		//--------------------------------------------------------------------------------
		public const string EnglishDictionaryName = "eng";
		public const string BulgarianDictionaryName = "bul";
		public const string GermanDictionaryName = "deu";
		public const string GreekDictionaryName = "ell";
		public const string SpanishDictionaryName = "spa";
		public const string FrenchDictionaryName = "fra";
		public const string CroatianDictionaryName = "hrv";
		public const string HungarianDictionaryName = "hun";
		public const string ItalianDictionaryName = "ita";
		public const string PolishDictionaryName = "pol";
		public const string PortugueseDictionaryName = "por";
		public const string RomanianDictionaryName = "ron";
		public const string SerbianDictionaryName = "srp";
		public const string SlovenianDictionaryName = "slv";
		public const string TurkishDictionaryName = "tur";
		public const string ChineseTraditionalDictionaryName = "chi_tra";
		public const string ChineseSimplifiedDictionaryName = "chi_sim";

		// culturename
		//--------------------------------------------------------------------------------
		public const string EnglishCultureName = "en";
		public const string BulgarianCultureName = "bg-BG";
		public const string GermanCultureName = "de-CH";
		public const string GreekCultureName = "el-GR";
		public const string SpanishCultureName = "es-CL";
		public const string FrenchCultureName = "fr-FR";
		public const string CroatianCultureName = "hr-HR";
		public const string HungarianCultureName = "hu-HU";
		public const string ItalianCultureName = "it-IT";
		public const string ItalianCHCultureName = "it-CH";
		public const string PolishCultureName = "pl-PL";
		public const string PortugueseCultureName = "pt-BR";
		public const string RomanianCultureName = "ro-RO";
		public const string SerbianCyrlCultureName = "sr-Cyrl";
		public const string SerbianLatnCultureName = "sr-Latn";
		public const string SlovenianCultureName = "sl-SI";
		public const string TurkishCultureName = "tr-TR";
		public const string ChineseTraditionalCultureName = "zh-CHT";
		public const string ChineseSimplifiedCultureName = "zh-CHS";

		//--------------------------------------------------------------------------------
		public static OCRDictionary GetOCRDictionaryFromLCID(int lcid)
		{
			OCRDictionary dict = new OCRDictionary();
			dict.Name = EnglishDictionaryName;
			dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, EnglishCultureName);

			switch (lcid)
			{
				case 1026: // Bulgarian (Bulgaria)
					dict.Name = BulgarianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, BulgarianCultureName);
					break;

				case 1050: // Croatian (Croatia)
					//case 4122: // Croatian (Latin, Bosnia and Herzegovina)
					dict.Name = CroatianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, CroatianCultureName);
					break;
					
				case 2060:	//French (Belgium)
				case 3084:	//French (Canada)	
				case 1036:	//French (France)
				case 5132:	//French (Luxembourg)
				case 6156:	//French (Principality of Monaco)
				case 4108:	//French (Switzerland)
					dict.Name = FrenchDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, FrenchCultureName);
					break;

				case 3079: // German (Austria)
				case 1031: // German (Germany)
				case 5127: // German (Liechtenstein)
				case 4103: // German (Luxembourg)
				case 2055: // German (Switzerland)
					dict.Name = GermanDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, GermanCultureName);
					break;

				case 1032: // Greek (Greece)
					dict.Name = GreekDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, GreekCultureName);
					break;

				case 1038: // Hungarian (Hungary)
					dict.Name = HungarianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, HungarianCultureName);
					break;

				case 1040: // Italian (Italy)
					dict.Name = ItalianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, ItalianCultureName);
					break;

				case 2064: // Italian (Switzerland)
					dict.Name = ItalianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, ItalianCHCultureName);
					break;

				case 1045: // Polish (Poland)
					dict.Name = PolishDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, PolishCultureName);
					break;

				case 1046: // Portuguese (Brazil)
				case 2070: // Portuguese (Portugal)
					dict.Name = PortugueseDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, PortugueseCultureName);
					break;

				case 1048: // Romanian (Romania)
					dict.Name = RomanianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, RomanianCultureName);
					break;

				case 7194: // Serbian (Cyrillic, Bosnia and Herzegovina)
				case 3098: // Serbian (Cyrillic, Serbia)
					dict.Name = SerbianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, SerbianCyrlCultureName);
					break;

				case 6170: // Serbian (Latin, Bosnia and Herzegovina)
				case 2074: // Serbian (Latin, Serbia)
					dict.Name = SerbianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, SerbianLatnCultureName);
					break;

				case 1060: // Slovenian (Slovenia)
					dict.Name = SlovenianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, SlovenianCultureName);
					break;

				case 11268: // Spanish (Argentina)
				case 16394: // Spanish (Bolivia)
				case 13322: // Spanish (Chile)
				case 9226:	// Spanish (Colombia)
				case 5130:	// Spanish (Costa Rica)
				case 7172:	// Spanish (Dominican Republic)
				case 12298: // Spanish (Ecuador)
				case 17418: // Spanish (El Salvador)
				case 4106:  // Spanish (Guatemala)
				case 18442: // Spanish (Honduras)
				case 2058:	// Spanish (Mexico)
				case 19460: // Spanish (Nicaragua)
				case 6154:	// Spanish (Panama)
				case 15364: // Spanish (Paraguay)
				case 10250: // Spanish (Peru)
				case 20490: // Spanish (Puerto Rico)
				case 3082:	// Spanish (Spain)
				case 21514: // Spanish (United States)
				case 14346: // Spanish (Uruguay)
				case 8202:	// Spanish (Venezuela)
					dict.Name = SpanishDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, SpanishCultureName);
					break;

				case 1055: // Turkish (Turkey)
					dict.Name = SlovenianDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, TurkishCultureName);
					break;

				case 3076: // Chinese (Hong Kong S.A.R.)
				case 5124: // Chinese (Macao S.A.R.)
				case 1028: // Chinese (Taiwan)
					dict.Name = ChineseTraditionalDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, ChineseTraditionalCultureName);
					break;

				case 2052: // Chinese (People's Republic of China)
				case 4100: // Chinese (Singapore)
					dict.Name = ChineseSimplifiedDictionaryName;
					dict.Path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), suffixOcrDictPath, ChineseSimplifiedCultureName);
					break;

				default:
					return dict;
			}

			return dict;
		}
	}
}
