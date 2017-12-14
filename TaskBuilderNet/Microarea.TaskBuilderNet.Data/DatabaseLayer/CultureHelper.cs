using System;
using System.Collections;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	/// <summary>
	/// Helper di gestione delle varie COLLATION suddivise per tipo database
	/// MySQL viene lasciato per ricordo!
	/// </summary>
	//============================================================================
	public class CultureHelper
	{
		public enum SupportedDBMS : short
		{
			SQL2000	= 0x0001,
			SQL2005	= 0x0002,
			Oracle	= 0x0003,
			MySQL	= 0x0004,
            Postgre = 0x0005,
			Default = SQL2000
		}
		
		#region CultureHelper LCID constants

		public const int AfrikaansNeutralCultureLCID			= 0x0036;
		public const int AlbanianNeutralCultureLCID				= 0x001C;
		public const int ArabicNeutralCultureLCID				= 0x0001;
		public const int ArmenianNeutralCultureLCID				= 0x002B;
		public const int AzeriNeutralCultureLCID				= 0x002C;
		public const int BasqueNeutralCultureLCID				= 0x002D;
		public const int ByelorussianNeutralCultureLCID			= 0x0023;
		public const int BulgarianNeutralCultureLCID			= 0x0002;
		public const int CatalanNeutralCultureLCID				= 0x0003;
		public const int ChineseSimplifiedNeutralCultureLCID	= 0x0004;
		public const int ChineseTraditionalNeutralCultureLCID	= 0x7C04;
		public const int CroatianNeutralCultureLCID				= 0x001A;
		public const int CzechNeutralCultureLCID				= 0x0005;
		public const int DanishNeutralCultureLCID				= 0x0006;
		public const int DivehiNeutralCultureLCID				= 0x0065;
		public const int DutchNeutralCultureLCID				= 0x0013;
		public const int EnglishNeutralCultureLCID				= 0x0009;
		public const int EstonianNeutralCultureLCID				= 0x0025;
		public const int FaroeseNeutralCultureLCID				= 0x0038;
		public const int FarsiNeutralCultureLCID				= 0x0029;
		public const int FinnishNeutralCultureLCID				= 0x000B;
		public const int FrenchNeutralCultureLCID				= 0x000C;
		public const int GalicianNeutralCultureLCID				= 0x0056;
		public const int GeorgianNeutralCultureLCID				= 0x0037;
		public const int GermanNeutralCultureLCID				= 0x0007;
		public const int GreekNeutralCultureLCID				= 0x0008;
		public const int GujaratiNeutralCultureLCID				= 0x0047;
		public const int HebrewNeutralCultureLCID				= 0x000D;
		public const int HindiNeutralCultureLCID				= 0x0039;
		public const int HungarianNeutralCultureLCID			= 0x000E;
		public const int IcelandicNeutralCultureLCID			= 0x000F;
		public const int IndonesianNeutralCultureLCID			= 0x0021;
		public const int InvariantNeutralCultureLCID			= 0x007F;
		public const int ItalianNeutralCultureLCID				= 0x0010;
		public const int JapaneseNeutralCultureLCID				= 0x0011;
		public const int KannadaNeutralCultureLCID				= 0x004B;
		public const int KazakhNeutralCultureLCID				= 0x003F;
		public const int KonkaniNeutralCultureLCID				= 0x0057;
		public const int KoreanNeutralCultureLCID				= 0x0012;
		public const int KyrgyzNeutralCultureLCID				= 0x0040;
		public const int LatvianNeutralCultureLCID				= 0x0026;
		public const int LithuanianNeutralCultureLCID			= 0x0027;
		public const int MacedonianNeutralCultureLCID			= 0x002F;
		public const int MalayNeutralCultureLCID				= 0x003E;
		public const int MarathiNeutralCultureLCID				= 0x004E;
		public const int MongolianNeutralCultureLCID			= 0x0050;
		public const int NorwegianNeutralCultureLCID			= 0x0014;
		public const int PolishNeutralCultureLCID				= 0x0015;
		public const int PortugueseNeutralCultureLCID			= 0x0016;
		public const int PunjabiNeutralCultureLCID				= 0x0046;
		public const int RomanianNeutralCultureLCID				= 0x0018;
		public const int RussianNeutralCultureLCID				= 0x0019;
		public const int SanskritNeutralCultureLCID				= 0x004F;
		public const int SerbianNeutralCultureLCID				= 0x7C1A;
		public const int SlovakNeutralCultureLCID				= 0x001B;
		public const int SlovenianNeutralCultureLCID			= 0x0024;
		public const int SpanishNeutralCultureLCID				= 0x000A;
		public const int SwahiliNeutralCultureLCID				= 0x0041;
		public const int SwedishNeutralCultureLCID				= 0x001D;
		public const int SyriacNeutralCultureLCID				= 0x005A;
		public const int TamilNeutralCultureLCID				= 0x0049;
		public const int TatarNeutralCultureLCID				= 0x0044;
		public const int TeluguNeutralCultureLCID				= 0x004A;
		public const int ThaiNeutralCultureLCID					= 0x001E;
		public const int TurkishNeutralCultureLCID				= 0x001F;
		public const int UkrainianNeutralCultureLCID			= 0x0022;
		public const int UrduNeutralCultureLCID					= 0x0020;
		public const int UzbekNeutralCultureLCID				= 0x0043;
		public const int VietnameseNeutralCultureLCID			= 0x002A;

		// Locale identifiers 
		public const int AfrikaansCultureLCID								= 0x0436; // Afrikaans (South Africa)
		public const int AlbanianCultureLCID								= 0x041C; // Albanian 
		public const int AlsatianCultureLCID								= 0x0484; // WinVista and later: Alsatian (France)  
		public const int AmharicUnicodeCultureLCID							= 0x045E; // WinVista and later: Amharic (Ethiopia). This is Unicode only. 
		public const int ArabicAlgeriaCultureLCID							= 0x1401; // Arabic (Algeria) 
		public const int ArabicBahrainCultureLCID							= 0x3C01; // Arabic (Bahrain) 
		public const int ArabicEgyptCultureLCID								= 0x0C01; // Arabic (Egypt) 
		public const int ArabicIraqCultureLCID								= 0x0801; // Arabic (Iraq) 
		public const int ArabicJordanCultureLCID							= 0x2C01; // Arabic (Jordan) 
		public const int ArabicKuwaitCultureLCID							= 0x3401; // Arabic (Kuwait) 
		public const int ArabicLebanonCultureLCID							= 0x3001; // Arabic (Lebanon) 
		public const int ArabicLibyaCultureLCID								= 0x1001; // Arabic (Libya) 
		public const int ArabicMoroccoCultureLCID							= 0x1801; // Arabic (Morocco) 
		public const int ArabicOmanCultureLCID								= 0x2001; // Arabic (Oman) 
		public const int ArabicQatarCultureLCID								= 0x4001; // Arabic (Qatar) 
		public const int ArabicSaudiArabiaCultureLCID						= 0x0401; // Arabic (Saudi Arabia) 
		public const int ArabicSyriaCultureLCID								= 0x2801; // Arabic (Syria) 
		public const int ArabicTunisiaCultureLCID							= 0x1C01; // Arabic (Tunisia) 
		public const int ArabicUAECultureLCID								= 0x3801; // Arabic (U.A.E.) 
		public const int ArabicYemenCultureLCID								= 0x2401; // Arabic (Yemen) 
		public const int ArmenianUnicodeCultureLCID							= 0x042B; // Windows 2000/XP: Armenian. This is Unicode only. 
		public const int AssameseUnicodeCultureLCID							= 0x044D; // Win Vista: Assamese (India). This is Unicode only. 
		public const int AzeriLatinCultureLCID								= 0x042C; // Azeri Latin (Azerbaijan) 
		public const int AzeriCyrillicCultureLCID							= 0x082C; // Azeri Cyrillic (Azerbaijan) 
		public const int BashkirCultureLCID									= 0x046D; // Win Vista: Bashkir (Russia)
		public const int BasqueCultureLCID									= 0x042D; // Basque 
		public const int BelarusianCultureLCID								= 0x0423; // Belarusian 
		public const int BengaliIndiaUnicodeCultureLCID						= 0x0445; // Windows XP SP2: Bengali (India). This is Unicode only. 
		public const int BosnianBosniaAndHerzegovinaCyrillicCultureLCID		= 0x201A; // Windows XP SP2: Bosnian Cyrillic (Bosnia and Herzegovina) 
		public const int BosnianBosniaAndHerzegovinaLatinCultureLCID		= 0x141A; // Windows XP SP2: Bosnian Latin (Bosnia and Herzegovina) 
		public const int BretonCultureLCID									= 0x047E; // Breton (France)
		public const int BulgarianCultureLCID								= 0x0402; // Bulgarian 
		public const int BurmeseCultureLCID									= 0x0455; // Burmese - NOT SUPPORTED
		public const int CatalanCultureLCID									= 0x0403; // Catalan (Catalan)
		public const int ChineseHongKongSARPRCCultureLCID					= 0x0C04; // Chinese (Hong Kong SAR, PRC) 
		public const int ChineseMacaoSARCultureLCID							= 0x1404; // Windows 98/Me, Windows 2000/XP: Chinese (Macao SAR) 
		public const int ChinesePRCCultureLCID								= 0x0804; // Chinese (PRC) 
		public const int ChineseSingaporeCultureLCID						= 0x1004; // Chinese (Singapore) 
		public const int ChineseTaiwanCultureLCID							= 0x0404; // Chinese (Taiwan) 
		public const int CroatianCultureLCID								= 0x041A; // Croatian 
		public const int CroatianBosniaAndHerzegovinaCultureLCID			= 0x101A; // Croatian (Bosnia and Herzegovina) 
		public const int CzechCultureLCID									= 0x0405; // Czech Republic
		public const int DanishCultureLCID									= 0x0406; // Danish (Denmark)
		public const int DivehiUnicodeCultureLCID							= 0x0465; // Windows XP: Divehi (Maldives). This is Unicode only. 
		public const int DutchBelgiumCultureLCID							= 0x0813; // Dutch (Belgium) 
		public const int DutchNetherlandsCultureLCID						= 0x0413; // Dutch (Netherlands) 
		public const int EnglishAustraliaCultureLCID						= 0x0C09; // English (Australia) 
		public const int EnglishBelizeCultureLCID							= 0x2809; // English (Belize) 
		public const int EnglishCanadaCultureLCID							= 0x1009; // English (Canada) 
		public const int EnglishCaribbeanCultureLCID						= 0x2409; // English (Caribbean) 
		public const int EnglishIndiaCultureLCID							= 0x4009; // Win Vista: English (India) 
		public const int EnglishIrelandCultureLCID							= 0x1809; // English (Ireland) 
		public const int EnglishJamaicaCultureLCID							= 0x2009; // English (Jamaica) 
		public const int EnglishMalaysiaCultureLCID							= 0x4409; // Win Vista: English (Malaysia) 
		public const int EnglishNewZealandCultureLCID						= 0x1409; // English (New Zealand) 
		public const int EnglishPhilippinesCultureLCID						= 0x3409; // Windows 98/Me, Windows 2000/XP: English (Philippines) 
		public const int EnglishSingaporeCultureLCID						= 0x4809; // Win Vista: English (Singapore) 
		public const int EnglishSouthAfricaCultureLCID						= 0x1C09; // English (South Africa) 
		public const int EnglishTrinidadAndTobagoCultureLCID				= 0x2C09; // English (Trinidad and Tobago) 
		public const int EnglishUnitedKingdomCultureLCID					= 0x0809; // English (United Kingdom) 
		public const int EnglishUnitedStatesCultureLCID						= 0x0409; // English (United States) 
		public const int EnglishZimbabweCultureLCID							= 0x3009; // Windows 98/Me, Windows 2000/XP: English (Zimbabwe) 
		public const int EstonianCultureLCID								= 0x0425; // Estonian 
		public const int FaroeseCultureLCID									= 0x0438; // Faroese (Faroe Islands)
		public const int FilipinoPhilippinesCultureLCID						= 0x0464; // Win XP/Vista: Filipino (Philippines)
		public const int FinnishCultureLCID									= 0x040B; // Finnish (Finland)
		public const int FrenchBelgiumCultureLCID							= 0x080C; // French (Belgium) 
		public const int FrenchCanadaCultureLCID							= 0x0C0C; // French (Canada) 
		public const int FrenchStandardCultureLCID							= 0x040C; // French (Standard) 
		public const int FrenchLuxembourgCultureLCID						= 0x140C; // French (Luxembourg) 
		public const int FrenchMonacoCultureLCID							= 0x180C; // French (Monaco) 
		public const int FrenchSwitzerlandCultureLCID						= 0x100C; // French (Switzerland) 
		public const int FrisianNetherlandsCultureLCID						= 0x0462; // Win XP/Vista: Frisian (Netherlands)
		public const int GalicianCultureLCID								= 0x0456; // Windows XP: Galician (Spain)
		public const int GeorgianUnicodeCultureLCID							= 0x0437; // Windows 2000/XP: Georgian (Georgia). This is Unicode only. 
		public const int GermanAustriaCultureLCID							= 0x0C07; // German (Austria) 
		public const int GermanStandardCultureLCID							= 0x0407; // German (Standard) 
		public const int GermanLiechtensteinCultureLCID						= 0x1407; // German (Liechtenstein) 
		public const int GermanLuxembourgCultureLCID						= 0x1007; // German (Luxembourg) 
		public const int GermanSwitzerlandCultureLCID						= 0x0807; // German (Switzerland) 
		public const int GreekCultureLCID									= 0x0408; // Greek 
		public const int GreenlandicCultureLCID								= 0x046F; // Win Vista: Greenlandic (Greenland)
		public const int GujaratiUnicodeCultureLCID							= 0x0447; // Windows XP: Gujarati (India). This is Unicode only. 
		public const int HausaNigeriaCultureLCID							= 0x0468; // Win Vista: Hausa (Nigeria - Latin)
		public const int HebrewCultureLCID									= 0x040D; // Hebrew (Israel)
		public const int HindiUnicodeCultureLCID							= 0x0439; // Windows 2000/XP: Hindi (India). This is Unicode only. 
		public const int HungarianCultureLCID								= 0x040E; // Hungarian 
		public const int IcelandicCultureLCID								= 0x040F; // Icelandic 
		public const int IgboNigeriaCultureLCID								= 0x0470; // Igbo (Nigeria)
		public const int IndonesianCultureLCID								= 0x0421; // Indonesian 
		public const int InuktitutCanadaCultureLCID							= 0x085D; // Win XP: Inuktitut (Canada - Latin)
		public const int InuktitutCanadaUnicodeCultureLCID					= 0x045D; // Win XP/Vista: Inuktitut (Canada - Syllabics). This is Unicode only.
		public const int IrishCultureLCID									= 0x083C; // Win XP/Vista: Irish (Ireland) 
		public const int ItalianStandardCultureLCID							= 0x0410; // Italian (Standard) 
		public const int ItalianSwitzerlandCultureLCID						= 0x0810; // Italian (Switzerland) 
		public const int JapaneseCultureLCID								= 0x0411; // Japanese 
		public const int KannadaUnicodeCultureLCID							= 0x044B; // Win XP: Kannada (India). This is Unicode only. 
		public const int KazakhCultureLCID									= 0x043F; // Win XP: Kazakh (Kazakhstan)
		public const int KhmerUnicodeCultureLCID							= 0x0453; // Win Vista: Khmer (Cambodia). This is Unicode only.
		public const int KicheGuatemalaCultureLCID							= 0x0486; // Win Vista: K'iche (Guatemala).
		public const int KinyarwandaRwandaCultureLCID						= 0x0487; // Win Vista: Kinyarwanda (Rwanda).
		public const int KonkaniUnicodeCultureLCID							= 0x0457; // Windows 2000/XP: Konkani (India). This is Unicode only. 
		public const int KoreanJohabCultureLCID								= 0x0812; // Windows 95, Windows NT 4.0 only: Korean (Johab) 
		public const int KoreanCultureLCID									= 0x0412; // Korean (Korea)
		public const int KyrgyzCultureLCID									= 0x0440; // Windows XP: Kyrgyz (Kyrgyzstan) 
		public const int LaoUnicodeCultureLCID								= 0x0454; // Win Vista: Lao (Lao PDR). This is Unicode only.
		public const int LatvianCultureLCID									= 0x0426; // Latvian (Latvia)
		public const int LithuanianCultureLCID								= 0x0427; // Lithuanian (Lithuania)
		public const int LithuanianClassicCultureLCID						= 0x0827; // Windows 98 only: Lithuanian (Classic) 
		public const int LowerSorbianCultureLCID							= 0x082E; // Win Vista: Lower Sorbian (Germany)
		public const int LuxembourgishCultureLCID							= 0x046E; // Win XP/Vista: Luxembourgish (Luxembourg)
		public const int MacedonianCultureLCID								= 0x042F; // Macedonian Cyrillic (Macedonia, FYROM) 
		public const int MalayBruneiDarussalamCultureLCID					= 0x083E; // Malay (Brunei Darussalam) 
		public const int MalayMalaysiaCultureLCID							= 0x043E; // Malay (Malaysia) 
		public const int MalayalamIndiaUnicodeCultureLCID					= 0x044C; // Win XP: Malayalam (India). This is Unicode only. 
		public const int MalteseMaltaCultureLCID							= 0x043A; // Win XP: Maltese (Malta) 
		public const int MaoriNewZealandCultureLCID							= 0x0481; // Win XP: Maori (New Zealand) 
		public const int MapudungunChileCultureLCID							= 0x047A; // Win XP/Vista: Mapudungun (Chile)
		public const int MarathiUnicodeCultureLCID							= 0x044E; // Windows 2000/XP: Marathi (India). This is Unicode only. 
		public const int MohawkCanadaCultureLCID							= 0x047C; // Windows XP/Vista: Mohawk (Canada)
		public const int MongolianCultureLCID								= 0x0450; // Windows XP: Mongolian Cyrillic (Mongolia)
		public const int MongolianUnicodeCultureLCID						= 0x0850; // Windows Vista: Mongolian (PRC). This is Unicode only. 
		public const int NepaliNepalUnicodeCultureLCID						= 0x0461; // Windows XP/Vista: Nepali (Nepal). This is Unicode only.
		public const int NorwegianBokmalCultureLCID							= 0x0414; // Norwegian (Bokmal) 
		public const int NorwegianNynorskCultureLCID						= 0x0814; // Norwegian (Nynorsk) 
		public const int OccitaneCultureLCID								= 0x0482; // Occitane (France)
		public const int OriyaIndiaUnicodeCultureLCID						= 0x0448; // Oriya (India). This is Unicode only.
		public const int PashtoAfghanistanCultureLCID						= 0x0463; // Pashto (Afghanistan)
		public const int PersianCultureLCID									= 0x0429; // Persian (Iran) (ex Farsi)
		public const int PolishCultureLCID									= 0x0415; // Polish 
		public const int PortugueseBrazilCultureLCID						= 0x0416; // Portuguese (Brazil) 
		public const int PortuguesePortugalCultureLCID						= 0x0816; // Portuguese (Portugal) 
		public const int PunjabiUnicodeCultureLCID							= 0x0446; // Windows XP: Punjabi (India). This is Unicode only. 
		public const int QuechuaBoliviaCultureLCID							= 0x046B; // Windows XP: Quechua (Bolivia) 
		public const int QuechuaEcuadorCultureLCID							= 0x086B; // Windows XP: Quechua (Ecuador) 
		public const int QuechuaPeruCultureLCID								= 0x0C6B; // Windows XP: Quechua (Peru) 
		public const int RomanianCultureLCID								= 0x0418; // Romanian 
		public const int RomanshSwitzerlandCultureLCID						= 0x0417; // Win XP/Vista: Romansh (Switzerland)
		public const int RussianCultureLCID									= 0x0419; // Russian 
		public const int SamiInariFinlandCultureLCID						= 0x243B; // Windows XP: Sami, Inari (Finland) 
		public const int SamiLuleNorwayCultureLCID							= 0x103B; // Windows XP: Sami, Lule (Norway) 
		public const int SamiLuleSwedenCultureLCID							= 0x143B; // Windows XP: Sami, Lule (Sweden) 
		public const int SamiNorthernFinlandCultureLCID						= 0x0C3B; // Windows XP: Sami, Northern (Finland) 	
		public const int SamiNorthernNorwayCultureLCID						= 0x043B; // Windows XP: Sami, Northern (Norway) 
		public const int SamiNorthernSwedenCultureLCID						= 0x083B; // Windows XP: Sami, Northern (Sweden) 	
		public const int SamiSkoltFinlandCultureLCID						= 0x203B; // Windows XP: Sami, Skolt (Finland) 
		public const int SamiSouthernNorwayCultureLCID						= 0x183B; // Windows XP: Sami, Southern (Norway) 
		public const int SamiSouthernSwedenCultureLCID						= 0x1C3B; // Windows XP: Sami, Southern (Sweden) 
		public const int SanskritUnicodeCultureLCID							= 0x044F; // Windows 2000/XP: Sanskrit (India). This is Unicode only. 
		public const int SerbianCyrillicCultureLCID							= 0x0C1A; // Windows XP: Serbian Cyrillic (Serbia)
		public const int SerbianCyrillicBosniaAndHerzegovinaCultureLCID		= 0x1C1A; // Windows XP: Serbian Cyrillic (Bosnia and Herzegovina) 
		public const int SerbianLatinCultureLCID							= 0x081A; // Serbian Latin (Serbia)
		public const int SerbianLatinBosniaAndHerzegovinaCultureLCID		= 0x181A; // Serbian Latin (Bosnia and Herzegovina) 
		public const int SesothoSaLeboaNorthernSothoSouthAfricaCultureLCID	= 0x046C; // Sesotho sa Leboa/Northern Sotho (South Africa) 
		public const int SetswanaTswanaSouthAfricaCultureLCID				= 0x0432; // Setswana/Tswana (South Africa) 
		public const int SinhalaSriLankaUnicodeCultureLCID					= 0x045B; // Win Vista: Sinhala (Sri Lanka). This is Unicode only. 
		public const int SlovakCultureLCID									= 0x041B; // Slovak 
		public const int SlovenianCultureLCID								= 0x0424; // Slovenian 
		public const int SpanishArgentinaCultureLCID						= 0x2C0A; // Spanish (Argentina) 
		public const int SpanishBoliviaCultureLCID							= 0x400A; // Spanish (Bolivia) 
		public const int SpanishChileCultureLCID							= 0x340A; // Spanish (Chile) 
		public const int SpanishColombiaCultureLCID							= 0x240A; // Spanish (Colombia) 
		public const int SpanishCostaRicaCultureLCID						= 0x140A; // Spanish (Costa Rica) 
		public const int SpanishDominicanRepublicCultureLCID				= 0x1C0A; // Spanish (Dominican Republic) 
		public const int SpanishEcuadorCultureLCID							= 0x300A; // Spanish (Ecuador) 
		public const int SpanishElSalvadorCultureLCID						= 0x440A; // Spanish (El Salvador) 
		public const int SpanishGuatemalaCultureLCID						= 0x100A; // Spanish (Guatemala) 
		public const int SpanishHondurasCultureLCID							= 0x480A; // Spanish (Honduras) 
		public const int SpanishMexicoCultureLCID							= 0x080A; // Spanish (Mexico) 
		public const int SpanishNicaraguaCultureLCID						= 0x4C0A; // Spanish (Nicaragua) 
		public const int SpanishPanamaCultureLCID							= 0x180A; // Spanish (Panama) 
		public const int SpanishParaguayCultureLCID							= 0x3C0A; // Spanish (Paraguay) 
		public const int SpanishPeruCultureLCID								= 0x280A; // Spanish (Peru) 
		public const int SpanishPuertoRicoCultureLCID						= 0x500A; // Spanish (Puerto Rico) 
		public const int SpanishSpainTraditionalSortCultureLCID				= 0x040A; // Spanish (Spain, Traditional Sort) 
		public const int SpanishSpainModernSortCultureLCID					= 0x0C0A; // Spanish (Spain, Modern Sort) 
		public const int SpanishUnitedStatesCultureLCID						= 0x540A; // Win Vista: Spanish (United States) 
		public const int SpanishUruguayCultureLCID							= 0x380A; // Spanish (Uruguay) 
		public const int SpanishVenezuelaCultureLCID						= 0x200A; // Spanish (Venezuela) 
		public const int SutuCultureLCID									= 0x0430; // Sutu - NOT SUPPORTED
		public const int SwahiliKenyaCultureLCID							= 0x0441; // Swahili (Kenya) 
		public const int SwedishFinlandCultureLCID							= 0x081D; // Swedish (Finland) 
		public const int SwedishCultureLCID									= 0x041D; // Swedish (Sweden)
		public const int SyriacUnicodeCultureLCID							= 0x045A; // Windows XP: Syriac. This is Unicode only. 
		public const int TajikTajikistanCultureLCID							= 0x0428; // Win Vista: Tajik (Tajikistan)
		public const int TamazightAlgeriaCultureLCID						= 0x085F; // Win Vista: Tamazight (Algeria)
		public const int TamilUnicodeCultureLCID							= 0x0449; // Windows 2000/XP: Tamil. This is Unicode only. 
		public const int TatarTatarstanCultureLCID							= 0x0444; // Windows XP: Tatar (Russia) 
		public const int TeluguIndiaUnicodeCultureLCID						= 0x044A; // Windows XP: Telugu (India). This is Unicode only. 
		public const int ThaiCultureLCID									= 0x041E; // Thai (Thailand)
		public const int TibetanBhutanUnicodeCultureLCID					= 0x0851; // Win Vista: Tibetan (Bhutan). This is Unicode only.
		public const int TibetanPRCUnicodeCultureLCID						= 0x0451; // Win Vista: Tibetan (PRC). This is Unicode only.
		public const int TurkishCultureLCID									= 0x041F; // Turkish 
		public const int TurkmenTurkmenistanCultureLCID						= 0x0442; // Win Vista: Turkmen (Turkmenistan)
		public const int UighurPRCCultureLCID								= 0x0480; // Win Vista: Uighur (PRC)
		public const int UkrainianCultureLCID								= 0x0422; // Ukrainian 
		public const int UpperSorbianCultureLCID							= 0x042E; // Win Vista: Upper Sorbian (Germany)
		public const int UrduIndiaCultureLCID								= 0x0820; // Urdu (India) 
		public const int UrduPakistanCultureLCID							= 0x0420; // Windows 98/Me, Windows 2000/XP: Urdu (Pakistan) 
		public const int UzbekCyrillicCultureLCID							= 0x0843; // Uzbek Cyrillic (Uzbekistan)
		public const int UzbekLatinCultureLCID								= 0x0443; // Uzbek Latin (Uzbekistan)
		public const int VietnameseCultureLCID								= 0x042A; // Windows 98/Me, Windows NT 4.0 and later: Vietnamese
		public const int WelshUnitedKingdomCultureLCID						= 0x0452; // Windows XP: Welsh (United Kingdom) 
		public const int WolofSenegalCultureLCID							= 0x0488; // Windows Vista: Wolof (Senegal) 
		public const int XhosaisiXhosaSouthAfricaCultureLCID				= 0x0434; // Windows XP: Xhosa/isiXhosa (South Africa) 
		public const int YakutRussiaCultureLCID								= 0x0485; // Windows Vista: Yakut (Russia)
		public const int YiPRCUnicodeCultureLCID							= 0x0478; // Windows Vista: Yi (PRC). This is Unicode only.
		public const int YorubaNigeriaCultureLCID							= 0x046A; // Windows Vista: Yoruba (Nigeria)
		public const int ZuluisiZuluSouthAfricaCultureLCID					= 0x0435; // Windows XP: Zulu/isiZulu (South Africa) 
																
		#endregion //CultureHelper LCID constants

		#region CultureHelper Windows Collations constant strings

		public const string AlbanianWindowsCollationDesignator					= "Albanian";
		public const string ArabicWindowsCollationDesignator					= "Arabic";
		public const string AzeriCyrillic90WindowsCollationDesignator			= "Azeri_Cyrillic_90";
		public const string AzeriLatin90WindowsCollationDesignator				= "Azeri_Latin_90";
		public const string ChineseHongKongStroke90WindowsCollationDesignator	= "Chinese_Hong_Kong_Stroke_90_CI_AS";
		public const string ChinesePRCWindowsCollationDesignator				= "Chinese_PRC";
		public const string ChinesePRC90WindowsCollationDesignator				= "Chinese_PRC_90_CI_AS";
		public const string ChinesePRCStrokeWindowsCollationDesignator			= "Chinese_PRC_Stroke";
		public const string ChineseTaiwanBopomofoWindowsCollationDesignator		= "Chinese_Taiwan_Bopomofo";
		public const string ChineseTaiwanStrokeWindowsCollationDesignator		= "Chinese_Taiwan_Stroke";
		public const string CroatianWindowsCollationDesignator					= "Croatian";
		public const string CyrillicGeneralWindowsCollationDesignator			= "Cyrillic_General";
		public const string CzechWindowsCollationDesignator						= "Czech";
		public const string DanishNorwegianWindowsCollationDesignator			= "Danish_Norwegian";
		public const string Divehi90WindowsCollationDesignator					= "Divehi_90";
		public const string EstonianWindowsCollationDesignator					= "Estonian";
		public const string FinnishSwedishWindowsCollationDesignator			= "Finnish_Swedish";
		public const string FrenchWindowsCollationDesignator					= "French";
		public const string GeorgianModernSortWindowsCollationDesignator		= "Georgian_Modern_Sort";
		public const string GermanPhoneBookWindowsCollationDesignator			= "German_PhoneBook";
		public const string GreekWindowsCollationDesignator						= "Greek";
		public const string HebrewWindowsCollationDesignator					= "Hebrew";
		public const string HindiWindowsCollationDesignator						= "Hindi";
		public const string HungarianWindowsCollationDesignator					= "Hungarian";
		public const string HungarianTechnicalWindowsCollationDesignator		= "Hungarian_Technical";
		public const string IcelandicWindowsCollationDesignator					= "Icelandic";
		public const string IndicGeneral90WindowsCollationDesignator			= "Indic_General_90";
		public const string JapaneseWindowsCollationDesignator					= "Japanese";
		public const string JapaneseUnicodeWindowsCollationDesignator			= "Japanese_Unicode";
		public const string Kazakh90WindowsCollationDesignator					= "Kazakh_90";
		public const string KoreanWansungWindowsCollationDesignator				= "Korean_Wansung";
		public const string KoreanWansungUnicodeWindowsCollationDesignator		= "Korean_Wansung_Unicode";
		public const string Latin1GeneralWindowsCollationDesignator				= "Latin1_General";
		public const string LatvianWindowsCollationDesignator					= "Latvian";
		public const string LithuanianWindowsCollationDesignator				= "Lithuanian";
		public const string LithuanianClassicWindowsCollationDesignator			= "Lithuanian_Classic";
		public const string MacedonianWindowsCollationDesignator				= "Macedonian";
		public const string MacedonianFYRO90WindowsCollationDesignator			= "Macedonian_FYROM_90";
		public const string ModernSpanishWindowsCollationDesignator				= "Modern_Spanish";
		public const string PolishWindowsCollationDesignator					= "Polish";
		public const string RomanianWindowsCollationDesignator					= "Romanian";
		public const string SlovakWindowsCollationDesignator					= "Slovak";
		public const string SlovenianWindowsCollationDesignator					= "Slovenian";
		public const string Syriac90WindowsCollationDesignator					= "Syriac_90";
		public const string Tatar90WindowsCollationDesignator					= "Tatar_90";
		public const string ThaiWindowsCollationDesignator						= "Thai";
		public const string TraditionalSpanishWindowsCollationDesignator		= "Traditional_Spanish";
		public const string TurkishWindowsCollationDesignator					= "Turkish";
		public const string UkrainianWindowsCollationDesignator					= "Ukrainian";
		public const string UzbekLatin90WindowsCollationDesignator				= "Uzbek_Latin_90";
		public const string VietnameseWindowsCollationDesignator				= "Vietnamese";
		
		public const string WindowsCollationCaseInsensitivePostFix		= "_CI";
		public const string WindowsCollationCaseSensitivePostFix		= "_CS";
		public const string WindowsCollationAccentInsensitivePostFix	= "_AI";
		public const string WindowsCollationAccentSensitivePostFix		= "_AS";
		public const string WindowsCollationKanatypeSensitivePostFix	= "_KS";
		public const string WindowsCollationWidthSensitivePostFix		= "_WS";
		public const string WindowsCollationBinPostFix					= "_BIN";
		public const string SQLCollationPreFix							= "SQL_";

		#endregion // CultureHelper Windows Collations constant strings

		#region CultureHelper Oracle Languages constant strings

		public const string AmericanOracleLanguage				= "AMERICAN";
		public const string ArabicOracleLanguage				= "ARABIC";
		public const string AzerbaijaniOracleLanguage			= "AZERBAIJANI";
		public const string BulgarianOracleLanguage				= "BULGARIAN";
		public const string CatalanOracleLanguage				= "CATALAN";
		public const string TraditionalChineseOracleLanguage	= "TRADITIONAL CHINESE";
		public const string SimplifiedChineseOracleLanguage		= "SIMPLIFIED CHINESE";
		public const string CroatianOracleLanguage				= "CROATIAN";
		public const string CzechOracleLanguage					= "CZECH";
		public const string DanishOracleLanguage				= "DANISH";
		public const string DutchOracleLanguage					= "DUTCH";
		public const string EnglishOracleLanguage				= "ENGLISH";
		public const string EstonianOracleLanguage				= "ESTONIAN";
		public const string FinnishOracleLanguage				= "FINNISH";
		public const string FrenchOracleLanguage				= "FRENCH";
		public const string CanadianFrenchOracleLanguage		= "CANADIAN FRENCH";
		public const string GermanOracleLanguage				= "GERMAN";
		public const string GreekOracleLanguage					= "GREEK";
		public const string GujaratiOracleLanguage				= "GUJARATI";
		public const string HebrewOracleLanguage				= "HEBREW";
		public const string HindiOracleLanguage					= "HINDI";
		public const string HungarianOracleLanguage				= "HUNGARIAN";
		public const string IcelandicOracleLanguage				= "ICELANDIC";
		public const string IndonesianOracleLanguage			= "INDONESIAN";
		public const string ItalianOracleLanguage				= "ITALIAN";
		public const string JapaneseOracleLanguage				= "JAPANESE";
		public const string KannadaOracleLanguage				= "KANNADA";
		public const string CyrillicKazakhOracleLanguage		= "CYRILLIC KAZAKH";
		public const string KoreanOracleLanguage				= "KOREAN";
		public const string LatvianOracleLanguage				= "LATVIAN";
		public const string LithuanianOracleLanguage			= "LITHUANIAN";
		public const string MacedonianOracleLanguage			= "MACEDONIAN";
		public const string MalayOracleLanguage					= "MALAY";
		public const string MarathiOracleLanguage				= "MARATHI";
		public const string NorwegianOracleLanguage				= "NORWEGIAN";
		public const string PolishOracleLanguage				= "POLISH";
		public const string PortugueseOracleLanguage			= "PORTUGUESE";
		public const string BrazilianPortugueseOracleLanguage	= "BRAZILIAN PORTUGUESE";
		public const string PunjabiOracleLanguage				= "PUNJABI";
		public const string RomanianOracleLanguage				= "ROMANIAN";
		public const string RussianOracleLanguage				= "RUSSIAN";
		public const string CyrillicSerbianOracleLanguage		= "CYRILLIC SERBIAN";
		public const string LatinSerbianOracleLanguage			= "LATIN SERBIAN";
		public const string SlovakOracleLanguage				= "SLOVAK";
		public const string SlovenianOracleLanguage				= "SLOVENIAN";
		public const string SpanishOracleLanguage				= "SPANISH";
		public const string LatinAmericanSpanishOracleLanguage	= "LATIN AMERICAN SPANISH";
		public const string MexicanSpanishOracleLanguage		= "MEXICAN SPANISH";
		public const string SwedishOracleLanguage				= "SWEDISH";
		public const string TamilOracleLanguage					= "TAMIL";
		public const string TeluguOracleLanguage				= "TELUGU";
		public const string ThaiOracleLanguage					= "THAI";
		public const string TurkishOracleLanguage				= "TURKISH";
		public const string UkrainianOracleLanguage				= "UKRAINIAN";
		public const string CyrillicUzbekOracleLanguage			= "CYRILLIC UZBEK";
		public const string LatinUzbekOracleLanguage			= "LATIN UZBEK";
		public const string VietnameseOracleLanguage			= "VIETNAMESE";
		
		#endregion //CultureHelper Oracle Languages constant strings

		#region CultureHelper Oracle Territories constant strings

		// Arabic Oracle Language Territories
		public const string AlgeriaOracleTerritory				= "ALGERIA";
		public const string BahrainOracleTerritory				= "BAHRAIN";
		public const string EgyptOracleTerritory				= "EGYPT";
		public const string IraqOracleTerritory					= "IRAQ";
		public const string JordanOracleTerritory				= "JORDAN";
		public const string KuwaitOracleTerritory				= "KUWAIT";
		public const string LebanonOracleTerritory				= "LEBANON";
		public const string LibyaOracleTerritory				= "LIBYA";
		public const string MoroccoOracleTerritory				= "MOROCCO";
		public const string OmanOracleTerritory					= "OMAN";
		public const string QatarOracleTerritory				= "QATAR";
		public const string SaudiArabiaOracleTerritory			= "SAUDI ARABIA";
		public const string SyriaOracleTerritory				= "SYRIA";
		public const string TunisiaOracleTerritory				= "TUNISIA";
		public const string UnitedArabEmiratesOracleTerritory	= "UNITED ARAB EMIRATES";
		public const string YemenOracleTerritory				= "YEMEN";

		// Azerbaijani Oracle Language Territories
		public const string AzerbaijanOracleTerritory	= "AZERBAIJAN";
		
		// Bulgarian Oracle Language Territories
		public const string BulgariaOracleTerritory		= "BULGARIA";

		// Catalan Oracle Language Territories
		public const string CataloniaOracleTerritory	= "CATALONIA";

		// Chinese Oracle Language Territories
		public const string HongKongOracleTerritory		= "HONG KONG";
		public const string ChinaOracleTerritory		= "CHINA";
		public const string SingaporeOracleTerritory	= "SINGAPORE";
		public const string TaiwanOracleTerritory		= "TAIWAN";

		// Croatian Oracle Language Territories
		public const string CroatiaOracleTerritory		= "CROATIA";

		// Czech Oracle Language Territories
		public const string CzechRepublicOracleTerritory	= "CZECH REPUBLIC";

		// Danish Oracle Language Territories
		public const string DenmarkRepublicOracleTerritory	= "DENMARK";

		// Dutch Oracle Language Territories
		public const string BelgiumOracleTerritory			= "BELGIUM";
		public const string TheNetherlandsOracleTerritory	= "THE NETHERLANDS";

		// English Oracle Language Territories
		public const string AustraliaOracleTerritory		= "AUSTRALIA";
		public const string CanadaOracleTerritory			= "CANADA";
		public const string IrelandOracleTerritory			= "IRELAND";
		public const string NewZealandOracleTerritory		= "NEW ZEALAND";
		public const string PhilippinesOracleTerritory		= "PHILIPPINES";
		public const string SouthAfricaOracleTerritory		= "SOUTH AFRICA";
		public const string UnitedKingdomOracleTerritory	= "UNITED KINGDOM";
		public const string AmericaOracleTerritory			= "AMERICA";

		// Estonian Oracle Language Territories
		public const string EstoniaOracleTerritory	= "ESTONIA";

		// Finnish Oracle Language Territories
		public const string FinlandOracleTerritory	= "FINLAND";

		// French Oracle Language Territories
		// public const string BelgiumOracleTerritory = "BELGIUM"; // already in the Dutch group
		// public const string CanadaOracleTerritory = "CANADA"; // already in the English group
		public const string FranceOracleTerritory			= "FRANCE";
		public const string LuxembourgOracleTerritory		= "LUXEMBOURG";
		public const string SwitzerlandOracleTerritory		= "SWITZERLAND";

		// Canadian French Oracle Language Territories
		// public const string CanadaOracleTerritory = "CANADA"; // already in the English group

		// German Oracle Language Territories
		public const string AustriaOracleTerritory			= "AUSTRIA";
		public const string GermanyOracleTerritory			= "GERMANY";
		// public const string LuxembourgOracleTerritory = "LUXEMBOURG"; // already in the French group
		// public const string SwitzerlandOracleTerritory = "SWITZERLAND"; // already in the French group

		// Greek Oracle Language Territories
		public const string GreeceOracleTerritory	= "GREECE";
		
		// Gujarati, Hindi, Kannada, Punjabi, Tamil, Telugu Oracle Languages Territories
		public const string IndiaOracleTerritory	= "INDIA";

		// Hebrew Oracle Language Territories
		public const string IsraelOracleTerritory	= "ISRAEL";
 
		// Hungarian Oracle Language Territories
		public const string HungaryOracleTerritory	= "HUNGARY";

		// Icelandic Oracle Language Territories
		public const string IcelandOracleTerritory	= "ICELAND";

		// Indonesian Oracle Language Territories
		public const string IndonesiaOracleTerritory	= "INDONESIA";

		// Italian Oracle Language Territories
		public const string ItalyOracleTerritory	= "ITALY";
		// public const string SwitzerlandOracleTerritory = "SWITZERLAND"; // already in the French group

		// Japanese Oracle Language Territories
		public const string JapanOracleTerritory	= "JAPAN";

		// Cyrillic Kazakh Oracle Language Territories
		public const string KazakhstanOracleTerritory	= "KAZAKHSTAN";

		// Korean Oracle Language Territories
		public const string KoreaOracleTerritory	= "KOREA";

		// Latvian Oracle Language Territories
		public const string LatviaOracleTerritory	= "LATVIA";

		// Lithuanian Oracle Language Territories
		public const string LithuaniaOracleTerritory	= "LITHUANIA";

		// Macedonian Oracle Language Territories
		public const string FyrMacedoniaOracleTerritory	= "FYR MACEDONIA";

		// Malay Oracle Language Territories
		public const string MalaysiaOracleTerritory	= "MALAYSIA";

		// Norwegian Oracle Language Territories
		public const string NorwayOracleTerritory	= "NORWAY";

		// Polish Oracle Language Territories
		public const string PolandOracleTerritory	= "POLAND";

		// Portuguese Oracle Language Territories
		public const string BrazilOracleTerritory	= "BRAZIL";
		public const string PortugalOracleTerritory	= "PORTUGAL";

		// Romanian Oracle Language Territories
		public const string RomaniaOracleTerritory	= "ROMANIA";

		// Russian Oracle Language Territories
		public const string RussiaOracleTerritory	= "RUSSIA";

		// Cyrillic Serbian and Latin Serbian Oracle Languages Territories
		public const string SerbiaAndMontenegroOracleTerritory	= "SERBIA AND MONTENEGRO";

		// Slovak Oracle Language Territories
		public const string SlovakiaOracleTerritory	= "SLOVAKIA";

		// Slovenian Oracle Language Territories
		public const string SloveniaOracleTerritory	= "SLOVENIA";

		// Spanish Oracle Language Territories
		public const string ArgentinaOracleTerritory	= "ARGENTINA";
		public const string ChileOracleTerritory		= "CHILE";
		public const string ColombiaOracleTerritory		= "COLOMBIA";
		public const string CostaRicaOracleTerritory	= "COSTA RICA";
		public const string EcuadorOracleTerritory		= "ECUADOR";
		public const string GuatemalaOracleTerritory	= "GUATEMALA";
		public const string MexicoOracleTerritory		= "MEXICO";
		public const string NicaraguaOracleTerritory	= "NICARAGUA";
		public const string PanamaOracleTerritory		= "PANAMA";
		public const string PeruOracleTerritory			= "PERU";
		public const string PuertoRicoOracleTerritory	= "PUERTO RICO";
		public const string SpainOracleTerritory		= "SPAIN";
		public const string VenezuelaOracleTerritory	= "VENEZUELA";

		// Swedish Oracle Language Territories
		// public const string FinlandOracleTerritory = "FINLAND"; // already in the Finnish group
		public const string SwedenOracleTerritory	= "SWEDEN";

		// Thai Oracle Language Territories
		public const string ThailandOracleTerritory	= "THAILAND";

		// Turkish Oracle Language Territories
		public const string TurkeyOracleTerritory	= "TURKEY";

		// Ukrainian Oracle Language Territories
		public const string UkraineOracleTerritory	= "UKRAINE";

		// Cyrillic Uzbek and Latin Uzbek Oracle Languages Territories
		public const string UzbekistanOracleTerritory	= "UZBEKISTAN";

		// Vietnamese Oracle Language Territories
		public const string VietnamOracleTerritory	= "VIETNAM";

		#endregion //CultureHelper Oracle Territories constant strings

		#region CultureHelper MySQL Character Sets constant strings
		// This is a subset of available character sets in MySQL 5.0
		// Here are indicated only the char sets will be used for the mapping with CultureInfo class.

		// Western European character sets
		public const string Latin1MySQLCharacterSet		= "latin1";		// cp1252 West European

		// Central European character sets
		public const string Keybcs2MySQLCharacterSet	= "keybcs2";	// DOS Kamenicky Czech-Slovak
		public const string Latin2MySQLCharacterSet		= "latin2";		// ISO 8859-2 Central European
		
		// South European and Middle Eastern character sets
		public const string Armscii8MySQLCharacterSet	= "armscii8";	// ARMSCII-8 Armenian
		public const string Cp1256MySQLCharacterSet		= "cp1256";		// Windows Arabic
		public const string Geostd8MySQLCharacterSet	= "geostd8";	// GEOSTD8 Georgian
		public const string GreekMySQLCharacterSet		= "greek";		// ISO 8859-7 Greek
		public const string HebrewMySQLCharacterSet		= "hebrew";		// ISO 8859-8 Hebrew
		public const string Latin5MySQLCharacterSet		= "latin5";		// ISO 8859-9 Turkish

		// Baltic character sets
		public const string Cp1257MySQLCharacterSet		= "cp1257";		// Windows Baltic
		public const string Latin7MySQLCharacterSet		= "latin7";		// ISO 8859-13 Baltic

		// Cyrillic character sets
		public const string Cp1251MySQLCharacterSet		= "cp1251";		// Windows Cyrillic

		// Asian character sets
		public const string Big5MySQLCharacterSet		= "big5";		// Big5 Traditional Chinese
		public const string Cp932MySQLCharacterSet		= "cp932";		// SJIS for Windows Japanese
		public const string EuckrMySQLCharacterSet		= "euckr";		// EUC-KR Korean
		public const string GbkMySQLCharacterSet		= "gbk";		// GBK Simplified Chinese
		public const string Tis620MySQLCharacterSet		= "tis620";		// TIS620 Thai

		// Unicode character sets
		public const string Utf8MySQLCharacterSet		= "utf8";		// UTF-8 Unicode

		#endregion //CultureHelper MySQL Character Sets constant strings
		
		#region CultureHelper MySQL Collations constant strings
		// This is a subset of available collations in MySQL 5.0 (associated to previous character sets).
		// Here are indicated only the collations will be used for the mapping with CultureInfo class.

		// Western European collation designator
		public const string Latin1DanishMySQLCollate		= "latin1_danish";
		public const string Latin1GeneralMySQLCollate		= "latin1_general";
		public const string Latin1German2MySQLCollate		= "latin1_german2";	// Phone-book rules
		public const string Latin1SpanishMySQLCollate		= "latin1_spanish";	
		public const string Latin1SwedishMySQLCollate		= "latin1_swedish";	// default collate of latin1 char set

		// Central European collation designator
		public const string Keybcs2GeneralMySQLCollate		= "keybcs2_general";	// default collate of keybcs2 char set
		public const string Latin2CroatianMySQLCollate		= "latin2_croatian";	
		public const string Latin2GeneralMySQLCollate		= "latin2_general";		// default collate of latin2 char set
		public const string Latin2HungarianMySQLCollate		= "latin2_hungarian";

		// South European and Middle Eastern collation designator
		public const string Armscii8GeneralMySQLCollate		= "armscii8_general";	// default collate of armscii8 char set
		public const string Cp1256GeneralMySQLCollate		= "cp1256_general";		// default collate of cp1256 char set
		public const string Geostd8GeneralMySQLCollate		= "geostd8_general";	// default collate of geostd8 char set
		public const string GreekGeneralMySQLCollate		= "greek_general";		// default collate of greek char set
		public const string HebrewGeneralMySQLCollate		= "hebrew_general";		// default collate of hebrew char set
		public const string Latin5TurkishMySQLCollate		= "latin5_turkish";		// default collate of latin5 char set

		// Baltic collation designator
		public const string Cp1257GeneralMySQLCollate		= "cp1257_general";
		public const string Cp1257LithuanianMySQLCollate	= "cp1257_lithuanian";
		public const string Latin7GeneralMySQLCollate		= "latin7_general";	// default collate of latin7 char set

		// Cyrillic collation designator
		public const string Cp1251BulgarianMySQLCollate		= "cp1251_bulgarian";
		public const string Cp1251GeneralMySQLCollate		= "cp1251_general";	// default collate of cp1251 char set
		public const string Cp1251UkrainianMySQLCollate		= "cp1251_ukrainian";

		// Asian collation designator
		public const string Big5ChineseMySQLCollate		= "big5_chinese";	// default collate of big5 char set (Traditional Chinese)
		public const string Cp932JapaneseMySQLCollate	= "cp932_japanese";	// default collate of cp932 char set
		public const string EuckrKoreanMySQLCollate		= "euckr_korean";	// default collate of euckr char set
		public const string GbkChineseMySQLCollate		= "gbk_chinese";	// default collate of gbk char set (Simplified Chinese)
		public const string Tis620ThaiMySQLCollate		= "tis620_thai";	// default collate of tis620 char set

		// Unicode collation designator
		public const string Utf8CzechMySQLCollate		= "utf8_czech";		
		public const string Utf8DanishMySQLCollate		= "utf8_danish";		
		public const string Utf8EstonianMySQLCollate	= "utf8_estonian";	
		public const string Utf8GeneralMySQLCollate		= "utf8_general";	// default collate of utf8 char set
		public const string Utf8HungarianMySQLCollate	= "utf8_hungarian";		
		public const string Utf8IcelandicMySQLCollate	= "utf8_icelandic";		
		public const string Utf8LatvianMySQLCollate		= "utf8_latvian";		
		public const string Utf8LithuanianMySQLCollate	= "utf8_lithuanian";		
		public const string Utf8PersianMySQLCollate		= "utf8_persian";		
		public const string Utf8PolishMySQLCollate		= "utf8_polish";		
		public const string Utf8RomanianMySQLCollate	= "utf8_romanian";		
		public const string Utf8SlovakMySQLCollate		= "utf8_slovak";		
		public const string Utf8SlovenianMySQLCollate	= "utf8_slovenian";
		public const string Utf8SpanishMySQLCollate		= "utf8_spanish";		
		public const string Utf8SwedishMySQLCollate		= "utf8_swedish";		
		public const string Utf8TurkishMySQLCollate		= "utf8_turkish";		
		public const string Utf8UnicodeMySQLCollate		= "utf8_unicode";

		public const string MySqlCollationCaseInsensitivePostFix	= "_ci";
		public const string MySqlCollationCaseSensitivePostFix		= "_cs";
		public const string MySqlCollationBinPostFix				= "_bin";

		#endregion //CultureHelper MySQL Collations constant strings

		//---------------------------------------------------------------------
		public static CultureInfo GetNeutralParentCultureInfo(CultureInfo aCultureInfo)
		{
			if (aCultureInfo == null)
				return null;

			if (aCultureInfo.IsNeutralCulture)
				return aCultureInfo;

			return (aCultureInfo.Parent != null && aCultureInfo.Parent.IsNeutralCulture) ? aCultureInfo.Parent : null;
		}

		//---------------------------------------------------------------------
		public static int GetNeutralParentCultureLCID(CultureInfo aCultureInfo)
		{
			if (aCultureInfo == null)
				return 0;

			if (aCultureInfo.IsNeutralCulture)
				return aCultureInfo.LCID;

			if (aCultureInfo.Parent == null || !aCultureInfo.Parent.IsNeutralCulture)
				return 0;

			return aCultureInfo.Parent.LCID;
		}

		//---------------------------------------------------------------------
		public static string GetNeutralParentCultureName(CultureInfo aCultureInfo)
		{
			int neutralCultureLCID = GetNeutralParentCultureLCID(aCultureInfo);
			if (neutralCultureLCID == 0)
				return String.Empty;
			
			CultureInfo neutralCultureInfo = new CultureInfo(neutralCultureLCID);

			return (neutralCultureInfo != null && neutralCultureInfo.IsNeutralCulture) 
				? neutralCultureInfo.Name : String.Empty;
		}

		# region GetWindowsCollationDesignator (Sql Server)
		///<summary>
		/// GetWindowsCollationDesignator (per SQL Server)
		/// Data una CultureInfo ritorna la collation designator specifica
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollationDesignator(CultureInfo aCultureInfo, SupportedDBMS supportedDBMS)
		{
			int neutralCultureLCID = GetNeutralParentCultureLCID(aCultureInfo);
			if (neutralCultureLCID == 0)
				return String.Empty;

			switch (neutralCultureLCID)
			{
				case AlbanianNeutralCultureLCID:
					return AlbanianWindowsCollationDesignator;

				case ArabicNeutralCultureLCID:
				case FarsiNeutralCultureLCID:
				case UrduNeutralCultureLCID:
					return ArabicWindowsCollationDesignator;
				
				case AzeriNeutralCultureLCID:
					if (aCultureInfo.LCID == AzeriCyrillicCultureLCID) // Azeri Cyrillic
						return (supportedDBMS == SupportedDBMS.SQL2005) ? AzeriCyrillic90WindowsCollationDesignator : CyrillicGeneralWindowsCollationDesignator;
					if (aCultureInfo.LCID == AzeriLatinCultureLCID && supportedDBMS == SupportedDBMS.SQL2005) // Azeri Latin
						return AzeriLatin90WindowsCollationDesignator;
					break;

				case ByelorussianNeutralCultureLCID:
				case BulgarianNeutralCultureLCID:
				case KyrgyzNeutralCultureLCID:
				case MongolianNeutralCultureLCID:
				case RussianNeutralCultureLCID:
				case SerbianNeutralCultureLCID:
					return CyrillicGeneralWindowsCollationDesignator;

				case ChineseSimplifiedNeutralCultureLCID:
				case ChineseTraditionalNeutralCultureLCID:
				{
					if (aCultureInfo.LCID == ChineseHongKongSARPRCCultureLCID && supportedDBMS == SupportedDBMS.SQL2005) // Hong Kong S.A.R.
						return ChineseHongKongStroke90WindowsCollationDesignator;
					if ((aCultureInfo.LCID == ChineseSingaporeCultureLCID || aCultureInfo.LCID == ChineseMacaoSARCultureLCID) && supportedDBMS == SupportedDBMS.SQL2005) // Singapore, Macau S.A.R.
						return ChinesePRC90WindowsCollationDesignator;
					if (aCultureInfo.LCID == 0x20804) 
						return ChinesePRCStrokeWindowsCollationDesignator;
					if (aCultureInfo.LCID == 0x30404) 
						return ChineseTaiwanBopomofoWindowsCollationDesignator;
					if (aCultureInfo.LCID == ChineseTaiwanCultureLCID) 
						return ChineseTaiwanStrokeWindowsCollationDesignator;
					return ChinesePRCWindowsCollationDesignator;
				}

				case CroatianNeutralCultureLCID:
					return CroatianWindowsCollationDesignator;

				case CzechNeutralCultureLCID:
					return CzechWindowsCollationDesignator;

				case DanishNeutralCultureLCID:
				case NorwegianNeutralCultureLCID:
					return DanishNorwegianWindowsCollationDesignator;
				
				case DivehiNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return Divehi90WindowsCollationDesignator;
					break;

				case EstonianNeutralCultureLCID:
					return EstonianWindowsCollationDesignator;

				case FinnishNeutralCultureLCID:
				case SwedishNeutralCultureLCID:
					return FinnishSwedishWindowsCollationDesignator;

				case FrenchNeutralCultureLCID:
					return FrenchWindowsCollationDesignator;

				case GeorgianNeutralCultureLCID:
					return GeorgianModernSortWindowsCollationDesignator;

				case GermanNeutralCultureLCID:
					if (aCultureInfo.LCID == 0x10407) // German PhoneBook Sort
						return GermanPhoneBookWindowsCollationDesignator;
					break;

				case GreekNeutralCultureLCID:
					return GreekWindowsCollationDesignator;

				case GujaratiNeutralCultureLCID:
				case KannadaNeutralCultureLCID:
				case KonkaniNeutralCultureLCID:
				case MarathiNeutralCultureLCID:
				case PunjabiNeutralCultureLCID:
				case SanskritNeutralCultureLCID:
				case TamilNeutralCultureLCID:
				case TeluguNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return IndicGeneral90WindowsCollationDesignator;
					break;

				case HebrewNeutralCultureLCID:
					return HebrewWindowsCollationDesignator;

				case HindiNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return IndicGeneral90WindowsCollationDesignator;
					return HindiWindowsCollationDesignator;

				case HungarianNeutralCultureLCID:
					if (aCultureInfo.LCID == 0x104E) // Hungarian Technical
						return HungarianTechnicalWindowsCollationDesignator;
					return HungarianWindowsCollationDesignator;

				case IcelandicNeutralCultureLCID:
					return IcelandicWindowsCollationDesignator;

				case JapaneseNeutralCultureLCID:
					if (aCultureInfo.LCID == 0x10411)
						return JapaneseUnicodeWindowsCollationDesignator;
					return JapaneseWindowsCollationDesignator;

				case KazakhNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return Kazakh90WindowsCollationDesignator;
					break;

				case KoreanNeutralCultureLCID:
					if (aCultureInfo.LCID == 0x10412)
						return KoreanWansungUnicodeWindowsCollationDesignator;
					return KoreanWansungWindowsCollationDesignator;

				case LatvianNeutralCultureLCID:
					return LatvianWindowsCollationDesignator;

				case LithuanianNeutralCultureLCID:
					if (aCultureInfo.LCID == LithuanianClassicCultureLCID)
						return LithuanianClassicWindowsCollationDesignator;
					return LithuanianWindowsCollationDesignator;

				case MacedonianNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return MacedonianFYRO90WindowsCollationDesignator;
					return MacedonianWindowsCollationDesignator;

				case PolishNeutralCultureLCID:
					return PolishWindowsCollationDesignator;

				case RomanianNeutralCultureLCID:
					return RomanianWindowsCollationDesignator;

				case SlovakNeutralCultureLCID:
					return SlovakWindowsCollationDesignator;

				case SlovenianNeutralCultureLCID:
					return SlovenianWindowsCollationDesignator;

				case SpanishNeutralCultureLCID:
					if 
						(
						aCultureInfo.LCID == SpanishMexicoCultureLCID ||				// Spanish (Mexico)
						aCultureInfo.LCID == SpanishSpainTraditionalSortCultureLCID		// Spanish (Traditional Sort)
						)
						return TraditionalSpanishWindowsCollationDesignator;
					return ModernSpanishWindowsCollationDesignator;

				case SyriacNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return Syriac90WindowsCollationDesignator;
					break;

				case TatarNeutralCultureLCID:
					if (supportedDBMS == SupportedDBMS.SQL2005)
						return Tatar90WindowsCollationDesignator;
					break;

				case ThaiNeutralCultureLCID:
					return ThaiWindowsCollationDesignator;

				case TurkishNeutralCultureLCID:
					return TurkishWindowsCollationDesignator;

				case UkrainianNeutralCultureLCID:
					return UkrainianWindowsCollationDesignator;

				case UzbekNeutralCultureLCID:
					if (aCultureInfo.LCID == UzbekCyrillicCultureLCID) // Uzbek Cyrillic
						return CyrillicGeneralWindowsCollationDesignator;
					if (aCultureInfo.LCID == UzbekLatinCultureLCID && supportedDBMS == SupportedDBMS.SQL2005) // Uzbek Latin
						return UzbekLatin90WindowsCollationDesignator;
					break;

				case VietnameseNeutralCultureLCID:
					return VietnameseWindowsCollationDesignator;

				default:
					break;
			}

			return Latin1GeneralWindowsCollationDesignator;
		}
	
		///<summary>
		/// GetWindowsCollationDesignator (overload) (per Sql Server)
		/// Data una CultureInfo ritorna la collation designator specifica
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollationDesignator(CultureInfo aCultureInfo)
		{
			return GetWindowsCollationDesignator(aCultureInfo, SupportedDBMS.Default);
		}

		///<summary>
		/// GetWindowsCollationDesignator (overload) (per Sql Server)
		/// Data una CultureInfo ritorna la collation designator specifica
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollationDesignator(int aLCID, SupportedDBMS supportedDBMS)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetWindowsCollationDesignator(new CultureInfo(aLCID), supportedDBMS);
		}

		///<summary>
		/// GetWindowsCollationDesignator (overload) (per Sql Server)
		/// Data una CultureInfo ritorna la collation designator specifica
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollationDesignator(int aLCID)
		{
			return GetWindowsCollationDesignator(aLCID, SupportedDBMS.Default);
		}
		# endregion

		# region GetWindowsCollation (Sql Server)
		///<summary>
		/// GetWindowsCollation (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation
			(
			CultureInfo		aCultureInfo, 
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive,
			bool			kanatypeSensitive,
			bool			widthSensitive
			)
		{
			string collationDesignator = GetWindowsCollationDesignator(aCultureInfo, supportedDBMS);
			if (collationDesignator == null || collationDesignator.Length == 0)
				return String.Empty;

			string collationName = collationDesignator;

			if (caseSensitive)
				collationName += WindowsCollationCaseSensitivePostFix;
			else
				collationName += WindowsCollationCaseInsensitivePostFix;

			if (accentSensitive)
				collationName += WindowsCollationAccentSensitivePostFix;
			else
				collationName += WindowsCollationAccentInsensitivePostFix;

			if (kanatypeSensitive)
				collationName += WindowsCollationKanatypeSensitivePostFix;

			if (widthSensitive)
				collationName += WindowsCollationWidthSensitivePostFix;

			return collationName;
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation
			(
			int				aLCID,
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive,
			bool			kanatypeSensitive,
			bool			widthSensitive
			)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetWindowsCollation
				(new CultureInfo(aLCID), supportedDBMS, caseSensitive, accentSensitive, kanatypeSensitive, widthSensitive);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation
			(
			CultureInfo		aCultureInfo,
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive
			)
		{
			return GetWindowsCollation(aCultureInfo, supportedDBMS, caseSensitive, accentSensitive, false, false);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation
			(
			int				aLCID,
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive
			)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetWindowsCollation(new CultureInfo(aLCID), supportedDBMS, caseSensitive, accentSensitive);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation(CultureInfo aCultureInfo, SupportedDBMS supportedDBMS)
		{
			return GetWindowsCollation(aCultureInfo, supportedDBMS, false, true);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation(int aLCID, SupportedDBMS supportedDBMS)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetWindowsCollation(new CultureInfo(aLCID), supportedDBMS);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation(CultureInfo aCultureInfo)
		{
			return GetWindowsCollation(aCultureInfo, SupportedDBMS.Default);
		}

		///<summary>
		/// GetWindowsCollation (overload) (per Sql Server)
		/// Aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetWindowsCollation(int aLCID)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetWindowsCollation(new CultureInfo(aLCID));
		}
		# endregion

		# region GetDatabaseCollation (Sql Server)
		///<summary>
		/// GetDatabaseCollation (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation
			(
			CultureInfo		aCultureInfo, 
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive,
			bool			kanatypeSensitive,
			bool			widthSensitive
			)
		{
			if (supportedDBMS == SupportedDBMS.SQL2000 || supportedDBMS == SupportedDBMS.SQL2005)
				return GetWindowsCollation(aCultureInfo, supportedDBMS, caseSensitive, accentSensitive, kanatypeSensitive, widthSensitive);

			//@@TODO Oracle
			return String.Empty;
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation
			(
			int				aLCID, 
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive,
			bool			kanatypeSensitive,
			bool			widthSensitive
			)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetDatabaseCollation
				(new CultureInfo(aLCID), supportedDBMS, caseSensitive, accentSensitive, kanatypeSensitive, widthSensitive);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation
			(
			CultureInfo		aCultureInfo, 
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive
			)
		{
			return GetDatabaseCollation(aCultureInfo, supportedDBMS, caseSensitive, accentSensitive, false, false);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation
			(
			int				aLCID, 
			SupportedDBMS	supportedDBMS,
			bool			caseSensitive,
			bool			accentSensitive
			)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetDatabaseCollation(new CultureInfo(aLCID), supportedDBMS, caseSensitive, accentSensitive);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation(CultureInfo aCultureInfo, SupportedDBMS supportedDBMS)
		{
			return GetDatabaseCollation(aCultureInfo, supportedDBMS, false, true);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation(int aLCID, SupportedDBMS supportedDBMS)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetDatabaseCollation(new CultureInfo(aLCID), supportedDBMS);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation(CultureInfo aCultureInfo)
		{
			return GetDatabaseCollation(aCultureInfo, SupportedDBMS.Default);
		}

		///<summary>
		/// GetDatabaseCollation (overload) (per Sql Server)
		/// richiama la GetWindowsCollation (che aggiunge al collation designator i postfissi e prefissi specifici (_ci, _cs, SQL_, etc)
		///</summary>
		//---------------------------------------------------------------------
		public static string GetDatabaseCollation(int aLCID)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetDatabaseCollation(new CultureInfo(aLCID));
		}
		# endregion

		# region SplitCollationNameComponents (Sql Server)
		///<summary>
		/// SplitCollationNameComponents (per Sql Server)
		/// Estrapola da nome della collation i vari prefissi e postfissi per ritornare il collation designator
		///</summary>
		//---------------------------------------------------------------------
		public static bool SplitCollationNameComponents
			(
			string			aCollationName, 
			SupportedDBMS	supportedDBMS, 
			out string		collationDesignator,
			out bool		isCaseSensitive,
			out bool		isAccentSensitive,
			out bool		isKanatypeSensitive,
			out bool		isWidthSensitive,
			out bool		isSQLCollationName,
			out int			codePage
			)
		{
			collationDesignator = String.Empty;
			isCaseSensitive = false;
			isAccentSensitive = false;
			isKanatypeSensitive = false;
			isWidthSensitive = false;
			isSQLCollationName = false;
			codePage = -1;

			if (aCollationName == null || aCollationName.Length == 0)
				return false;

			collationDesignator = aCollationName.Trim();
			
			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationWidthSensitivePostFix))
			{
				isWidthSensitive = true;
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationWidthSensitivePostFix.Length);
			}

			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationKanatypeSensitivePostFix))
			{
				isKanatypeSensitive = true;
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationKanatypeSensitivePostFix.Length);
			}

			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationAccentInsensitivePostFix))
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationAccentInsensitivePostFix.Length);
			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationAccentSensitivePostFix))
			{
				isAccentSensitive = true;
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationAccentSensitivePostFix.Length);
			}

			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationCaseInsensitivePostFix))
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationCaseInsensitivePostFix.Length);
			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationCaseSensitivePostFix))
			{
				isCaseSensitive = true;
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationCaseSensitivePostFix.Length);
			}

			if (collationDesignator.ToUpper(CultureInfo.InvariantCulture).EndsWith(WindowsCollationBinPostFix))
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - WindowsCollationBinPostFix.Length);

			isSQLCollationName = (supportedDBMS == SupportedDBMS.SQL2000 || supportedDBMS == SupportedDBMS.SQL2005) &&
										collationDesignator.ToUpper(CultureInfo.InvariantCulture).StartsWith(SQLCollationPreFix);
			
			bool upperCasePreference = false;
			if (isSQLCollationName)
			{
				collationDesignator = collationDesignator.Substring(SQLCollationPreFix.Length);
				int codePageIdx = collationDesignator.LastIndexOf("_CP");
				if (codePageIdx >= 0 && collationDesignator.Length > codePageIdx + 3)
				{
					try
					{
						codePage = int.Parse(collationDesignator.Substring(codePageIdx + 3, collationDesignator.Length - (codePageIdx + 3)), NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
						// CP1 specifies code page 1252, for all other code pages the complete code page number is specified
						if (codePage == 1)
							codePage = 1252;
					}
					catch(Exception)
					{
					}

					collationDesignator = collationDesignator.Substring(0, codePageIdx);
				}
				upperCasePreference = collationDesignator.EndsWith("_Pref");
				if (upperCasePreference)
					collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - 5);
			
				if (
					String.Compare(collationDesignator, "1xCompat", true, CultureInfo.InvariantCulture) == 0 ||
					String.Compare(collationDesignator, "AltDiction", true, CultureInfo.InvariantCulture) == 0
					)
					collationDesignator = Latin1GeneralWindowsCollationDesignator.ToUpper(CultureInfo.InvariantCulture);
			}
			return true;
		}
		# endregion

		# region GetCompatibleLocaleIDs (Sql Server)
		///<summary>
		/// GetCompatibleLocaleIDs (per Sql Server)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetCompatibleLocaleIDs
			(
			string aCollationName, 
			SupportedDBMS supportedDBMS, 
			CultureTypes cultureTypes
			)
		{
			if (aCollationName == null || aCollationName.Length == 0)
				return null;

			CultureInfo[] cultures = CultureInfo.GetCultures(cultureTypes);
			if (cultures == null || cultures.Length == 0)
				return null;

			string	collationDesignator = String.Empty;
			bool	isCaseSensitive = false;
			bool	isAccentSensitive = false;
			bool	isKanatypeSensitive = false;
			bool	isWidthSensitive = false;
			bool	isSQLCollationName = false;
			int		codePage = -1;

			if (!SplitCollationNameComponents
				(
					aCollationName, 
					supportedDBMS, 
					out collationDesignator,
					out isCaseSensitive,
					out isAccentSensitive,
					out isKanatypeSensitive,
					out isWidthSensitive,
					out isSQLCollationName,
					out codePage
				)
				)
				return null;

			if (collationDesignator == null || collationDesignator.Length == 0)
				return null;

			ArrayList localeIDs = new ArrayList();

			foreach(CultureInfo aCultureInfo in cultures)
			{
				string cultureCollationDesignator = GetWindowsCollationDesignator(aCultureInfo, supportedDBMS);
				if 
					(
					String.Compare(cultureCollationDesignator, collationDesignator, true, CultureInfo.InvariantCulture) == 0 ||
					(isSQLCollationName && String.Compare(Latin1GeneralWindowsCollationDesignator, collationDesignator, true, CultureInfo.InvariantCulture) == 0)
					)
				{
					if (!isSQLCollationName || codePage == -1 || codePage == aCultureInfo.TextInfo.ANSICodePage || codePage == aCultureInfo.TextInfo.OEMCodePage)
						localeIDs.Add(aCultureInfo.LCID);
					continue;
				}

			}

			return (localeIDs != null && localeIDs.Count > 0) ? (int[])localeIDs.ToArray(typeof(int)) : null;
		}

		///<summary>
		/// GetCompatibleLocaleIDs (overload) (per Sql Server)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetCompatibleLocaleIDs(string aCollationName, SupportedDBMS supportedDBMS)
		{
			return GetCompatibleLocaleIDs(aCollationName, supportedDBMS, CultureTypes.SpecificCultures);
		}

		///<summary>
		/// GetCompatibleLocaleIDs (overload) (per Sql Server)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetCompatibleLocaleIDs(string aCollationName, CultureTypes cultureTypes)
		{
			return GetCompatibleLocaleIDs(aCollationName, SupportedDBMS.Default, cultureTypes);
		}

		///<summary>
		/// GetCompatibleLocaleIDs (overload) (per Sql Server)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetCompatibleLocaleIDs(string aCollationName)
		{
			return GetCompatibleLocaleIDs(aCollationName, SupportedDBMS.Default);
		}
		# endregion

		# region IsCollationCompatibleWithCulture (Sql Server)
		///<summary>
		/// IsCollationCompatibleWithCulture (per Sql Server)
		/// Date una CultureInfo e una collation stabilisce se sono compatibili
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsCollationCompatibleWithCulture
			(
			CultureInfo aCultureInfo, 
			string aCollationName, 
			SupportedDBMS supportedDBMS
			)
		{
			if (aCultureInfo == null)
				return false;

			if (aCollationName == null || aCollationName.Length == 0)
				return true;

			string	collationDesignator = String.Empty;
			bool	isCaseSensitive = false;
			bool	isAccentSensitive = false;
			bool	isKanatypeSensitive = false;
			bool	isWidthSensitive = false;
			bool	isSQLCollationName = false;
			int		codePage = -1;

			if (!SplitCollationNameComponents
				(
				aCollationName, 
				supportedDBMS, 
				out collationDesignator,
				out isCaseSensitive,
				out isAccentSensitive,
				out isKanatypeSensitive,
				out isWidthSensitive,
				out isSQLCollationName,
				out codePage
				)
				)
				return false;

			string databaseCollation = GetDatabaseCollation(aCultureInfo, supportedDBMS, isCaseSensitive, isAccentSensitive, isKanatypeSensitive, isWidthSensitive); 
			if (databaseCollation == null || databaseCollation.Length == 0)
				return false;

			if (String.Compare(databaseCollation, aCollationName, true, CultureInfo.InvariantCulture) == 0)
				return true;

			string	databaseCollationDesignator = String.Empty;
			bool	isDatabaseCollationCaseSensitive = false;
			bool	isDatabaseCollationAccentSensitive = false;
			bool	isDatabaseCollationKanatypeSensitive = false;
			bool	isDatabaseCollationWidthSensitive = false;
			bool	isDatabaseCollationSQLCollationName = false;
			int		databaseCollationCodePage = -1;

			if (!SplitCollationNameComponents
				(
				databaseCollation, 
				supportedDBMS, 
				out databaseCollationDesignator,
				out isDatabaseCollationCaseSensitive,
				out isDatabaseCollationAccentSensitive,
				out isDatabaseCollationKanatypeSensitive,
				out isDatabaseCollationWidthSensitive,
				out isDatabaseCollationSQLCollationName,
				out databaseCollationCodePage
				)
				)
				return false;
			
			if (String.Compare(collationDesignator, databaseCollationDesignator, true, CultureInfo.InvariantCulture) != 0)
				return false;

			if (isSQLCollationName && codePage >= 0)	
				return (codePage == databaseCollationCodePage || codePage == aCultureInfo.TextInfo.ANSICodePage || codePage == aCultureInfo.TextInfo.OEMCodePage);

			return true;
		}

		///<summary>
		/// IsCollationCompatibleWithCulture (overload) (per Sql Server)
		/// Date una CultureInfo e una collation stabilisce se sono compatibili
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsCollationCompatibleWithCulture(CultureInfo aCultureInfo, string aCollationName)
		{
			return IsCollationCompatibleWithCulture(aCultureInfo, aCollationName, SupportedDBMS.Default);
		}

		///<summary>
		/// IsCollationCompatibleWithCulture (overload) (per Sql Server)
		/// Date una CultureInfo e una collation stabilisce se sono compatibili
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsCollationCompatibleWithCulture(int aLCID, string aCollationName)
		{
			if (aLCID <= 0)
				return false;

			return IsCollationCompatibleWithCulture(new CultureInfo(aLCID), aCollationName);
		}
		# endregion

		# region GetCompatibleLocaleIDsWithOracleLanguageAndTerritory (ORACLE)
		///<summary>
		/// GetCompatibleLocaleIDsWithOracleLanguageAndTerritory (per Oracle)
		/// Dati un language ed un territory ritorna un array di LCID compatibili con essi
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetCompatibleLocaleIDsWithOracleLanguageAndTerritory
			(
			string	anOracleLanguage,
			string	anOracleTerritory
			)
		{
			if (anOracleLanguage == null)
				return null;

			anOracleLanguage = anOracleLanguage.Trim();
			if (anOracleLanguage.Length == 0)
				return null;
			
			if (anOracleTerritory != null)
				anOracleTerritory = anOracleTerritory.Trim();

			bool unspecifiedTerritory = (anOracleTerritory == null || anOracleTerritory.Length == 0);

			ArrayList localeIDs = new ArrayList();

			if (String.Compare(anOracleLanguage, AmericanOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(EnglishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AmericaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishUnitedStatesCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, ArabicOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(ArabicNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AlgeriaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicAlgeriaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, BahrainOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicBahrainCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, EgyptOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicEgyptCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IraqOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicIraqCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, JordanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicJordanCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, KuwaitOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicKuwaitCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LebanonOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicLebanonCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LibyaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicLibyaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, MoroccoOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicMoroccoCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, OmanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicOmanCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, QatarOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicQatarCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SaudiArabiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicSaudiArabiaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SyriaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicSyriaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, TunisiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicTunisiaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, UnitedArabEmiratesOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicUAECultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, YemenOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ArabicYemenCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, AzerbaijaniOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(AzeriNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AzerbaijanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(AzeriLatinCultureLCID);
					localeIDs.Add(AzeriCyrillicCultureLCID);
				}
			}
			else if (String.Compare(anOracleLanguage, BulgarianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(BulgarianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, BulgariaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(BulgarianCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, CatalanOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(CatalanNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CataloniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(CatalanCultureLCID);
			}
			else if
				(
				String.Compare(anOracleLanguage, TraditionalChineseOracleLanguage, true, CultureInfo.InvariantCulture) == 0 ||
				String.Compare(anOracleLanguage, SimplifiedChineseOracleLanguage, true, CultureInfo.InvariantCulture) == 0
				)
			{
				if (String.Compare(anOracleLanguage, TraditionalChineseOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ChineseTraditionalNeutralCultureLCID);
				else
					localeIDs.Add(ChineseSimplifiedNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, HongKongOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ChineseHongKongSARPRCCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ChinaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ChinesePRCCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SingaporeOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ChineseSingaporeCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, TaiwanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ChineseTaiwanCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, CroatianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(CroatianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CroatiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(CroatianCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, CzechOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(CzechNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CzechRepublicOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(CzechCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, DanishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(DanishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, DenmarkRepublicOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(DanishCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, DutchOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(DutchNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, BelgiumOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(DutchBelgiumCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, TheNetherlandsOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(DutchNetherlandsCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, EnglishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(EnglishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AustraliaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishAustraliaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CanadaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishCanadaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IrelandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishIrelandCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, NewZealandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishNewZealandCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PhilippinesOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishPhilippinesCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SouthAfricaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishSouthAfricaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, UnitedKingdomOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishUnitedKingdomCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AmericaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EnglishUnitedStatesCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, EstonianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(EstonianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, EstoniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(EstonianCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, FinnishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(FinnishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, FinlandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FinnishCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, FrenchOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(FrenchNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, BelgiumOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FrenchBelgiumCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CanadaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FrenchCanadaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, FranceOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FrenchStandardCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LuxembourgOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FrenchLuxembourgCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SwitzerlandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(FrenchSwitzerlandCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, GermanOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(GermanNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, AustriaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GermanAustriaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, GermanyOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GermanStandardCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LuxembourgOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GermanLuxembourgCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SwitzerlandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GermanSwitzerlandCultureLCID);
			}
			else if (String.Compare(anOracleLanguage, GreekOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(GreekNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, GreeceOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GreekCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, GujaratiOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(GujaratiNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(GujaratiUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, HebrewOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(HebrewNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IsraelOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(HebrewCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, HindiOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(HindiNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(HindiUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, HungarianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(HungarianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, HungaryOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(HungarianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, IcelandicOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(IcelandicNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IcelandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(IcelandicCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, IndonesianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(IndonesianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndonesiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(IndonesianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, ItalianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(ItalianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ItalyOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ItalianStandardCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SwitzerlandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ItalianSwitzerlandCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, JapaneseOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(JapaneseNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, JapanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(JapaneseCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, KannadaOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(KannadaNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(KannadaUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, CyrillicKazakhOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(KazakhNeutralCultureLCID);
				localeIDs.Add(KyrgyzNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, KazakhstanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(KazakhCultureLCID);
					localeIDs.Add(KyrgyzCultureLCID);
				}
			}
			else if	(String.Compare(anOracleLanguage, KoreanOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(KoreanNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, KoreaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(KoreanCultureLCID);
					localeIDs.Add(KoreanJohabCultureLCID);
				}
			}
			else if	(String.Compare(anOracleLanguage, LatvianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(LatvianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LatviaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(LatvianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, LithuanianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(LithuanianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, LithuaniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(LithuanianCultureLCID);
					localeIDs.Add(LithuanianClassicCultureLCID);
				}
			}
			else if	(String.Compare(anOracleLanguage, MacedonianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(MacedonianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, FyrMacedoniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(MacedonianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, MalayOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(MalayNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, MalaysiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(MalayMalaysiaCultureLCID);
				if (unspecifiedTerritory)
					localeIDs.Add(MalayBruneiDarussalamCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, MarathiOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(MarathiNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(MarathiUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, NorwegianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(NorwegianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, NorwayOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(NorwegianBokmalCultureLCID);
					localeIDs.Add(NorwegianNynorskCultureLCID);
				}
			}
			else if	(String.Compare(anOracleLanguage, PolishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(PolishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PolandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(PolishCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, PortugueseOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(PortugueseNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PortugalOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(PortuguesePortugalCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, BrazilianPortugueseOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(PortugueseNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, BrazilOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(PortugueseBrazilCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, PunjabiOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(PunjabiNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(PunjabiUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, RomanianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(RomanianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, RomaniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(RomanianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, RussianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(RussianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, RussiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(RussianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, CyrillicSerbianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(SerbianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SerbiaAndMontenegroOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(SerbianCyrillicCultureLCID);
					localeIDs.Add(SerbianCyrillicBosniaAndHerzegovinaCultureLCID);
				}			
			}
			else if	(String.Compare(anOracleLanguage, LatinSerbianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(SerbianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SerbiaAndMontenegroOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
				{
					localeIDs.Add(SerbianLatinCultureLCID);
					localeIDs.Add(SerbianLatinBosniaAndHerzegovinaCultureLCID);
				}			
			}
			else if	(String.Compare(anOracleLanguage, SlovakOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(SlovakNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SlovakiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SlovakCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, SlovenianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(SlovenianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SloveniaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SlovenianCultureLCID);
			}
			else if	
				(
				String.Compare(anOracleLanguage, SpanishOracleLanguage, true, CultureInfo.InvariantCulture) == 0 ||
				String.Compare(anOracleLanguage, LatinAmericanSpanishOracleLanguage, true, CultureInfo.InvariantCulture) == 0 ||
				String.Compare(anOracleLanguage, MexicanSpanishOracleLanguage, true, CultureInfo.InvariantCulture) == 0
				)
			{
				localeIDs.Add(SpanishNeutralCultureLCID);
				localeIDs.Add(SpanishSpainTraditionalSortCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ArgentinaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishArgentinaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ChileOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishChileCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ColombiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishColombiaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, CostaRicaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishCostaRicaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, EcuadorOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishEcuadorCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, GuatemalaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishGuatemalaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, MexicoOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishMexicoCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, NicaraguaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishNicaraguaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, EcuadorOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishEcuadorCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PanamaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishPanamaCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PeruOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishPeruCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, PuertoRicoOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishPuertoRicoCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SpainOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishSpainModernSortCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, VenezuelaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SpanishVenezuelaCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, SwedishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(SwedishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, FinlandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SwedishFinlandCultureLCID);
				if (unspecifiedTerritory || String.Compare(anOracleTerritory, SwedenOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(SwedishCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, TamilOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(TamilNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(TamilUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, TeluguOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(TeluguNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, IndiaOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(TeluguIndiaUnicodeCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, ThaiOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(ThaiNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, ThailandOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(ThaiCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, TurkishOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(TurkishNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, TurkeyOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(TurkishCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, UkrainianOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(UkrainianNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, UkraineOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(UkrainianCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, CyrillicUzbekOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(UzbekNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, UzbekistanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(UzbekCyrillicCultureLCID);
			}
			else if	(String.Compare(anOracleLanguage, LatinUzbekOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(UzbekNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, UzbekistanOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(LatinUzbekOracleLanguage);
			}
			else if	(String.Compare(anOracleLanguage, VietnameseOracleLanguage, true, CultureInfo.InvariantCulture) == 0)
			{
				localeIDs.Add(VietnameseNeutralCultureLCID);

				if (unspecifiedTerritory || String.Compare(anOracleTerritory, VietnamOracleTerritory, true, CultureInfo.InvariantCulture) == 0)
					localeIDs.Add(VietnameseCultureLCID);
			}
		
			return (localeIDs != null && localeIDs.Count > 0) ? (int[])localeIDs.ToArray(typeof(int)) : null;
		}
		# endregion

		# region GetMySqlDatabaseCollation (MySql)
		///<summary>
		/// GetMySqlDatabaseCollation (per MySql)
		/// Data una Culture estrapola il collate designator aggiungendo gli eventuali postfissi
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlDatabaseCollation(CultureInfo aCultureInfo, bool caseSensitive, bool useUnicode)
		{
			string collationDesignator = GetMySqlCollationDesignator(aCultureInfo, useUnicode);
			
			if (collationDesignator == null || collationDesignator.Length == 0)
				return String.Empty;

			string collationName = collationDesignator;

			if (caseSensitive)
				collationName += MySqlCollationCaseSensitivePostFix;
			else
				collationName += MySqlCollationCaseInsensitivePostFix;

			return collationName;
		}

		///<summary>
		/// GetMySqlDatabaseCollation (overload) (per MySql)
		/// Data una Culture estrapola il collate designator aggiungendo gli eventuali postfissi
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlDatabaseCollation(int aLCID, bool caseSensitive, bool useUnicode)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetMySqlDatabaseCollation(new CultureInfo(aLCID), caseSensitive, useUnicode);
		}

		///<summary>
		/// GetMySqlDatabaseCollation (overload) (per MySql)
		/// Data una Culture estrapola il collate designator aggiungendo gli eventuali postfissi
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlDatabaseCollation(CultureInfo aCultureInfo, bool useUnicode)
		{
			return GetMySqlDatabaseCollation(aCultureInfo, false, useUnicode);
		}

		///<summary>
		/// GetMySqlDatabaseCollation (overload) (per MySql)
		/// Data una Culture estrapola il collate designator aggiungendo gli eventuali postfissi
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlDatabaseCollation(int aLCID, bool useUnicode)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetMySqlDatabaseCollation(new CultureInfo(aLCID), useUnicode);
		}
		# endregion

		# region GetMySqlCollationDesignator (MySql)
		///<summary>
		/// GetMySqlCollationDesignator (overload) (per MySql)
		/// Data una CultureInfo ritorna un collation designator
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlCollationDesignator(int aLCID, bool useUnicode)
		{
			if (aLCID <= 0)
				return String.Empty;

			return GetMySqlCollationDesignator(new CultureInfo(aLCID), useUnicode);
		}

		///<summary>
		/// GetMySqlCollationDesignator (per MySql)
		/// Data una CultureInfo ritorna un collation designator
		///</summary>
		//---------------------------------------------------------------------
		public static string GetMySqlCollationDesignator(CultureInfo aCultureInfo, bool useUnicode)
		{
			int neutralCultureLCID = GetNeutralParentCultureLCID(aCultureInfo);
			if (neutralCultureLCID == 0)
				return String.Empty;
			
			switch (neutralCultureLCID)
			{
				case ArabicNeutralCultureLCID:
				case UrduNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1256GeneralMySQLCollate;

				case ArmenianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Armscii8GeneralMySQLCollate;

				case AzeriNeutralCultureLCID:
					if (aCultureInfo.LCID == AzeriCyrillicCultureLCID) // Azeri Cyrillic
						return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;
					if (aCultureInfo.LCID == AzeriLatinCultureLCID) // Azeri Latin
						return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin1GeneralMySQLCollate;
					break;

				case ByelorussianNeutralCultureLCID:
				case KyrgyzNeutralCultureLCID:
				//case MacedonianNeutralCultureLCID: // ???
				case MongolianNeutralCultureLCID:
				case RussianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;

				case BulgarianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251BulgarianMySQLCollate;

				case SerbianNeutralCultureLCID:
					{
						if (aCultureInfo.LCID == SerbianCyrillicBosniaAndHerzegovinaCultureLCID ||
							aCultureInfo.LCID == SerbianCyrillicCultureLCID)
							return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;
						if (aCultureInfo.LCID == SerbianLatinBosniaAndHerzegovinaCultureLCID ||
							aCultureInfo.LCID == SerbianLatinCultureLCID)
							return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin2GeneralMySQLCollate;
						break;
					}

				case ChineseSimplifiedNeutralCultureLCID:
				case ChineseTraditionalNeutralCultureLCID:
					{
						if (aCultureInfo.LCID == ChineseHongKongSARPRCCultureLCID ||
							aCultureInfo.LCID == ChineseTaiwanCultureLCID ||
							aCultureInfo.LCID == ChineseTraditionalNeutralCultureLCID)
							return (useUnicode) ? Utf8UnicodeMySQLCollate : Big5ChineseMySQLCollate;
						if (aCultureInfo.LCID == ChineseMacaoSARCultureLCID ||
							aCultureInfo.LCID == ChinesePRCCultureLCID ||
							aCultureInfo.LCID == ChineseSimplifiedNeutralCultureLCID ||
							aCultureInfo.LCID == ChineseSingaporeCultureLCID)
							return (useUnicode) ? Utf8UnicodeMySQLCollate : GbkChineseMySQLCollate;
						break;
					}

				case CroatianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin2CroatianMySQLCollate;

				case CzechNeutralCultureLCID:
					return (useUnicode) ? Utf8CzechMySQLCollate : Keybcs2GeneralMySQLCollate;

				case DanishNeutralCultureLCID:
					return (useUnicode) ? Utf8DanishMySQLCollate : Latin1DanishMySQLCollate;

				// unicode only
				case DivehiNeutralCultureLCID:
				case GujaratiNeutralCultureLCID:
				case HindiNeutralCultureLCID:
				case KannadaNeutralCultureLCID:
				case KonkaniNeutralCultureLCID:
				case MarathiNeutralCultureLCID:
				case PunjabiNeutralCultureLCID:
				case SanskritNeutralCultureLCID:
				case SyriacNeutralCultureLCID:
				case TamilNeutralCultureLCID:
				case TeluguNeutralCultureLCID:
					return Utf8UnicodeMySQLCollate;

				case EstonianNeutralCultureLCID:
					return (useUnicode) ? Utf8EstonianMySQLCollate : Latin7GeneralMySQLCollate;

				case FarsiNeutralCultureLCID:
					return (useUnicode) ? Utf8PersianMySQLCollate : Cp1256GeneralMySQLCollate;

				case FinnishNeutralCultureLCID:
				case SwedishNeutralCultureLCID:
					return (useUnicode) ? Utf8SwedishMySQLCollate : Latin1SwedishMySQLCollate;

				case GeorgianNeutralCultureLCID: // unicode only
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Geostd8GeneralMySQLCollate;

				case GermanNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin1German2MySQLCollate;

				case GreekNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : GreekGeneralMySQLCollate;

				case HebrewNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : HebrewGeneralMySQLCollate;

				case HungarianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin2HungarianMySQLCollate;

				case IcelandicNeutralCultureLCID:
					return (useUnicode) ? Utf8IcelandicMySQLCollate : Latin1GeneralMySQLCollate;

				case JapaneseNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp932JapaneseMySQLCollate;

				case KazakhNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;

				case KoreanNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : EuckrKoreanMySQLCollate;

				case LatvianNeutralCultureLCID:
					return (useUnicode) ? Utf8LatvianMySQLCollate : Cp1257GeneralMySQLCollate;

				case LithuanianNeutralCultureLCID:
					return (useUnicode) ? Utf8LithuanianMySQLCollate : Cp1257LithuanianMySQLCollate;

				case MacedonianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;

				case PolishNeutralCultureLCID:
					return (useUnicode) ? Utf8PolishMySQLCollate : Latin2GeneralMySQLCollate;

				case RomanianNeutralCultureLCID:
					return (useUnicode) ? Utf8RomanianMySQLCollate : Latin2GeneralMySQLCollate;

				case SlovakNeutralCultureLCID:
					return (useUnicode) ? Utf8SlovakMySQLCollate : Latin2GeneralMySQLCollate;

				case SlovenianNeutralCultureLCID:
					return (useUnicode) ? Utf8SlovenianMySQLCollate : Latin2GeneralMySQLCollate;

				case SpanishNeutralCultureLCID:
					return (useUnicode) ? Utf8SpanishMySQLCollate : Latin1SpanishMySQLCollate;

				case TatarNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;

				case ThaiNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Tis620ThaiMySQLCollate;

				case TurkishNeutralCultureLCID:
					return (useUnicode) ? Utf8TurkishMySQLCollate : Latin5TurkishMySQLCollate;

				case UkrainianNeutralCultureLCID:
					return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251UkrainianMySQLCollate;

				case UzbekNeutralCultureLCID:
					if (aCultureInfo.LCID == UzbekCyrillicCultureLCID) // Uzbek Cyrillic
						return (useUnicode) ? Utf8UnicodeMySQLCollate : Cp1251GeneralMySQLCollate;
					if (aCultureInfo.LCID == UzbekLatinCultureLCID) // Uzbek Latin
						return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin1GeneralMySQLCollate;
					break;

				case VietnameseNeutralCultureLCID: // non esiste una collate
					return Utf8UnicodeMySQLCollate;

				default:
					break;
			}

			// Afrikaans, Albanian, Basque, Catalan, Dutch, English, Faroese, French, Galician, Indonesian, Italian
			// Malay, Norwegian, Portoguese, Swahili
			return (useUnicode) ? Utf8UnicodeMySQLCollate : Latin1GeneralMySQLCollate;
		}
		# endregion

		# region SplitMySqlCollationNameComponents (MySql)
		///<summary>
		/// SplitMySqlCollationNameComponents (per MySql)
		/// Estrapola da nome della collation i vari prefissi e postfissi per ritornare il collation designator
		///</summary>
		//---------------------------------------------------------------------
		public static bool SplitMySqlCollationNameComponents
			(string aCollationName, out string collationDesignator, out bool isCaseSensitive)
		{
			collationDesignator = String.Empty;
			isCaseSensitive = false;

			if (aCollationName == null || aCollationName.Length == 0)
				return false;

			collationDesignator = aCollationName.Trim();

			if (collationDesignator.ToLower(CultureInfo.InvariantCulture).EndsWith(MySqlCollationCaseInsensitivePostFix))
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - MySqlCollationCaseInsensitivePostFix.Length);
			if (collationDesignator.ToLower(CultureInfo.InvariantCulture).EndsWith(MySqlCollationCaseSensitivePostFix))
			{
				isCaseSensitive = true;
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - MySqlCollationCaseSensitivePostFix.Length);
			}

			if (collationDesignator.ToLower(CultureInfo.InvariantCulture).EndsWith(MySqlCollationBinPostFix))
				collationDesignator = collationDesignator.Substring(0, collationDesignator.Length - MySqlCollationBinPostFix.Length);

			return true;
		}
		# endregion

		# region GetMySqlCompatibleLocaleIDs (MySql)
		///<summary>
		/// GetMySqlCompatibleLocaleIDs (per MySql)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetMySqlCompatibleLocaleIDs(string aCollationName, CultureTypes cultureTypes, bool useUnicode)
		{
			if (aCollationName == null || aCollationName.Length == 0)
				return null;

			CultureInfo[] cultures = CultureInfo.GetCultures(cultureTypes);
			if (cultures == null || cultures.Length == 0)
				return null;

			string collationDesignator = String.Empty;
			bool isCaseSensitive = false;

			if (!SplitMySqlCollationNameComponents(aCollationName, out collationDesignator, out isCaseSensitive))
				return null;

			if (collationDesignator == null || collationDesignator.Length == 0)
				return null;

			ArrayList localeIDs = new ArrayList();

			foreach (CultureInfo aCultureInfo in cultures)
			{
				string cultureCollationDesignator = GetMySqlCollationDesignator(aCultureInfo, useUnicode);
				if(String.Compare(cultureCollationDesignator, collationDesignator, true, CultureInfo.InvariantCulture) == 0 ||
					(String.Compare(Latin1GeneralMySQLCollate, collationDesignator, true, CultureInfo.InvariantCulture) == 0))
				{
					localeIDs.Add(aCultureInfo.LCID);
					continue;
				}
			}

			return (localeIDs != null && localeIDs.Count > 0) ? (int[])localeIDs.ToArray(typeof(int)) : null;
		}

		///<summary>
		/// GetMySqlCompatibleLocaleIDs (overload) (per MySql)
		/// Data una collation ritorna un array di LCID compatibili con la cultura estrapolata dalla collation
		///</summary>
		//---------------------------------------------------------------------
		public static int[] GetMySqlCompatibleLocaleIDs(string aCollationName, bool useUnicode)
		{
			return GetMySqlCompatibleLocaleIDs(aCollationName, CultureTypes.SpecificCultures, useUnicode);
		}
		# endregion 

		# region IsMySqlCollationCompatibleWithCulture (MySql)
		///<summary>
		/// IsMySqlCollationCompatibleWithCulture (per MySql)
		/// Date una CultureInfo e una collation stabilisce se sono compatibili
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsMySqlCollationCompatibleWithCulture(CultureInfo aCultureInfo, string aCollationName, bool useUnicode)
		{
			if (aCultureInfo == null)
				return false;

			if (aCollationName == null || aCollationName.Length == 0)
				return true;

			string collationDesignator = String.Empty;
			bool isCaseSensitive = false;

			if (!SplitMySqlCollationNameComponents(aCollationName, out collationDesignator, out isCaseSensitive))
				return false;

			string databaseCollation = GetMySqlDatabaseCollation(aCultureInfo, isCaseSensitive, useUnicode);
			if (databaseCollation == null || databaseCollation.Length == 0)
				return false;

			if (String.Compare(databaseCollation, aCollationName, true, CultureInfo.InvariantCulture) == 0)
				return true;

			string databaseCollationDesignator = String.Empty;
			bool isDatabaseCollationCaseSensitive = false;

			if (!SplitMySqlCollationNameComponents(databaseCollation, out databaseCollationDesignator, out isDatabaseCollationCaseSensitive))
				return false;

			if (String.Compare(collationDesignator, databaseCollationDesignator, true, CultureInfo.InvariantCulture) != 0)
				return false;

			return true;
		}

		///<summary>
		/// IsMySqlCollationCompatibleWithCulture (overload) (per MySql)
		/// Date una CultureInfo e una collation stabilisce se sono compatibili
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsMySqlCollationCompatibleWithCulture(int aLCID, string aCollationName, bool useUnicode)
		{
			if (aLCID <= 0)
				return false;

			return IsMySqlCollationCompatibleWithCulture(new CultureInfo(aLCID), aCollationName, useUnicode);
		}
		# endregion
	}
}