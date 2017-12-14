using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
	/// <summary>
	/// Summary description for Generics.
	/// </summary>
	public class Generics
	{
		#region constant data members

		#region public constant data members

		public const string DatabaseScriptsFolderName			= "DatabaseScript";
		public const string CreateScriptsSubFolderName			= "Create";
		public const string UpgradeScriptsSubFolderName			= "Upgrade";
		public const string UpgradeScriptsReleaseFolderFormat	= "Release_{0}";
		public const string SQLServerScriptsSubFolderName		= "All";
		public const string OracleScriptsSubFolderName			= "Oracle";
		public const string DatabaseScriptExtension				= ".sql";
		public const string CppExtension						= ".cpp";
		public const string CppHeaderExtension					= ".h";
        public const string CsExtension                         = ".cs";
		public const string CppResourceFileExtension			= ".rc";
		public const string CppResourceHeaderExtension			= ".hrc";
		public const string CppProjectExtension					= ".vcxproj";
		public const string NetSolutionExtension				= ".sln";
		public const string TBLocalizerSolutionExtension		= ".tblsln";
		public const string ModuleDocumentObjectsFilename		= "DocumentObjects.xml";
		public const string ModuleClientDocumentObjectsFilename	= "ClientDocumentObjects.xml";
		public const string InfoFilenamePostFix					= "Info";

		public const string XmlTagDbCreateInfo					= "CreateInfo";
		public const string XmlTagDbUpgradeInfo					= "UpgradeInfo";
		public const string XmlTagDbInfoModuleinfo				= "ModuleInfo";
		public const string XmlTagDbInfoLevel1					= "Level1";
		public const string XmlTagDbInfoLevel2					= "Level2";
		public const string XmlTagDbInfoStep					= "Step";
		public const string XmlTagDbInfoDbRelease				= "DBRel";
		public const string XmlTagDbInfoDependency				= "Dependency";
		public const string XmlAttributeDbInfoModuleName		= "name";
		public const string XmlAttributeDbInfoStepNumstep		= "numstep";
		public const string XmlAttributeDbInfoStepScript		= "script";
		public const string XmlAttributeDbInfoDbReleaseNumber	= "numrel";
		public const string XmlAttributeDbInfoApplication		= "app";
		public const string XmlAttributeDbInfoModule			= "module";

		public const string TaskBuilderApplicationConfigText =  "TbApplication";

		public const string DescriptionFolder			= "Description";
		public const string DocumentDescriptionFilename	= "Document.xml";
		public const string DocumentDBTsFilename		= "Dbts.xml";
		public const string DocumentExtRefFilename		= "ExternalReferences.xml";
		public const string ReportsListFilename			= "Reports.xml";

		public const string ModuleObjectsFolderName		= "ModuleObjects";
		public const string ReferenceObjectsFolderName	= "ReferenceObjects";
		public const string ReportsFolderName			= "Report";
		public const string AddOnDatabaseObjectsRootTag	= "AddOnDatabaseObjects";

		public const ushort TBReservedResourceIdUpperLimit = 3000;
		public const ushort FirstValidResourceId = 20000;	// First symbol value that will be used for a 
		public const ushort MinimumResourceId = 1;			// dialog resource, menu resource, and so on. 
		public const ushort MaximumResourceId = 28671;		// The valid range for resource symbol values 
		// is 1 to 0x6FFF (=28671)
		public const ushort DefaultReservedResourceIdsRange = 100;

		public const ushort TBReservedControlIdUpperLimit = 3500;
		public const ushort FirstValidControlId = 40000;// First symbol value that will be used for a 
		public const ushort MinimumControlId = 8;		// dialog control. The valid range for dialog 
		public const ushort MaximumControlId = 57343;	// control symbol values is 8 to 0xDFFF (=57343).
		public const ushort DefaultReservedControlIdsRange = 400;

		public const ushort TBReservedCommandIdUpperLimit = 34000;
		public const ushort FirstValidCommandId = 50500;// First symbol value that will be used for a 
		public const ushort MinimumCommandId = 32768;	// command identification. The valid range for 
		public const ushort MaximumCommandId = 57343;	// command symbol values is 0x8000 (=32768) to 
		// 0xDFFF(=57343).
		public const ushort DefaultReservedCommandIdsRange = 100;

		public const ushort TBReservedSymedIdUpperLimit = 2400;
		public const ushort FirstValidSymedId = 21000;  // First symbol value that will be issued when 
		public const ushort MinimumSymedId = 1;			// you manually assign a symbol value using the 
		public const ushort MaximumSymedId = UInt16.MaxValue; // New command in the Symbol Browser
		public const ushort DefaultReservedSymedIdsRange = 100;
	
		public const string CppHeaderFilesDialogFilter	= "C++ Include Files (*.h;hpp;hxx;inl;tlh)|*.h;*.hpp;*.hxx;*.inl;*.tlh|All files (*.*)|*.*";

		public const int ApplicationDbSignatureMaxLength	= 20;
		public const int ModuleDbSignatureMaxLength			= 40;

		public const int SQLServerDBObjectNameMaximumLength = 128;
		public const int OracleDBObjectNameMaximumLength = 30;

		public const string TBGuidColumnName = "TBGuid";
		public const string TBCreatedColumnName = "TBCreated";
		public const string TBModifiedColumnName = "TBModified";
        public const string TBCreatedIDColumnName = "TBCreatedID";
        public const string TBModifiedIDColumnName = "TBModifiedID";

		// Voglio specificare qual'è la lunghezza massima in caratteri delle colonne
		// dei BodyEdit generati in automatico, in modo da evitare di incappare in fase
		// di esecuzione del codice nei noti inconvenienti causati da colonne troppo ampie
		public const int MaxBodyEditColumnWidth = 32;

		#region Language Identifiers

		public const ushort LANG_NEUTRAL		= 0x00;
		public const ushort LANG_INVARIANT		= 0x7F;
		public const ushort LANG_AFRIKAANS		= 0x36;
		public const ushort LANG_ALBANIAN		= 0x1C;
		public const ushort LANG_ARABIC			= 0x01;
		public const ushort LANG_ARMENIAN		= 0x2B;
		public const ushort LANG_ASSAMESE		= 0x4D;
		public const ushort LANG_AZERI			= 0x2C;
		public const ushort LANG_BASQUE			= 0x2D;
		public const ushort LANG_BELARUSIAN		= 0x23;
		public const ushort LANG_BENGALI		= 0x45;
		public const ushort LANG_BULGARIAN		= 0x02;
		public const ushort LANG_CATALAN		= 0x03;
		public const ushort LANG_CHINESE		= 0x04;
		public const ushort LANG_CROATIAN		= 0x1A;
		public const ushort LANG_CZECH			= 0x05;
		public const ushort LANG_DANISH			= 0x06;
		public const ushort LANG_DIVEHI			= 0x65;
		public const ushort LANG_DUTCH			= 0x13;
		public const ushort LANG_ENGLISH		= 0x09;
		public const ushort LANG_ESTONIAN		= 0x25;
		public const ushort LANG_FAEROESE		= 0x38;
		public const ushort LANG_FARSI			= 0x29;
		public const ushort LANG_FINNISH		= 0x0B;
		public const ushort LANG_FRENCH			= 0x0C;
		public const ushort LANG_GALICIAN		= 0x56;
		public const ushort LANG_GEORGIAN		= 0x37;
		public const ushort LANG_GERMAN			= 0x07;
		public const ushort LANG_GREEK			= 0x08;
		public const ushort LANG_GUJARATI		= 0x47;
		public const ushort LANG_HEBREW			= 0x0D;
		public const ushort LANG_HINDI			= 0x39;
		public const ushort LANG_HUNGARIAN		= 0x0E;
		public const ushort LANG_ICELANDIC		= 0x0F;
		public const ushort LANG_INDONESIAN		= 0x21;
		public const ushort LANG_ITALIAN		= 0x10;
		public const ushort LANG_JAPANESE		= 0x11;
		public const ushort LANG_KANNADA		= 0x4B;
		public const ushort LANG_KASHMIRI		= 0x60;
		public const ushort LANG_KAZAK			= 0x3F;
		public const ushort LANG_KONKANI		= 0x57;
		public const ushort LANG_KOREAN			= 0x12;
		public const ushort LANG_KYRGYZ			= 0x40;
		public const ushort LANG_LATVIAN		= 0x26;
		public const ushort LANG_LITHUANIAN		= 0x27;
		public const ushort LANG_MACEDONIAN		= 0x2F; // the Former Yugoslav Republic of Macedonia
		public const ushort LANG_MALAY			= 0x3E;
		public const ushort LANG_MALAYALAM		= 0x4C;
		public const ushort LANG_MANIPURI		= 0x58;
		public const ushort LANG_MARATHI		= 0x4E;
		public const ushort LANG_MONGOLIAN		= 0x50;
		public const ushort LANG_NEPALI			= 0x61;
		public const ushort LANG_NORWEGIAN		= 0x14;
		public const ushort LANG_ORIYA			= 0x48;
		public const ushort LANG_POLISH			= 0x15;
		public const ushort LANG_PORTUGUESE		= 0x16;
		public const ushort LANG_PUNJABI		= 0x46;
		public const ushort LANG_ROMANIAN		= 0x18;
		public const ushort LANG_RUSSIAN		= 0x19;
		public const ushort LANG_SANSKRIT		= 0x4F;
		public const ushort LANG_SERBIAN		= 0x1A;
		public const ushort LANG_SINDHI			= 0x59;
		public const ushort LANG_SLOVAK			= 0x1B;
		public const ushort LANG_SLOVENIAN		= 0x24;
		public const ushort LANG_SPANISH		= 0x0A;
		public const ushort LANG_SWAHILI		= 0x41;
		public const ushort LANG_SWEDISH		= 0x1D;
		public const ushort LANG_SYRIAC			= 0x5A;
		public const ushort LANG_TAMIL			= 0x49;
		public const ushort LANG_TATAR			= 0x44;
		public const ushort LANG_TELUGU			= 0x4A;
		public const ushort LANG_THAI			= 0x1E;
		public const ushort LANG_TURKISH		= 0x1F;
		public const ushort LANG_UKRAINIAN		= 0x22;
		public const ushort LANG_URDU			= 0x20;
		public const ushort LANG_UZBEK			= 0x43;
		public const ushort LANG_VIETNAMESE		= 0x2A;
			
		#endregion
							  
		#region Sublanguage Identifiers
		
		public const ushort SUBLANG_NEUTRAL						= 0x00; // language neutral
		public const ushort SUBLANG_DEFAULT						= 0x01; // user default
		public const ushort SUBLANG_SYS_DEFAULT					= 0x02; // system default
		public const ushort SUBLANG_ARABIC_SAUDI_ARABIA			= 0x01; // Arabic (Saudi Arabia)
		public const ushort SUBLANG_ARABIC_IRAQ					= 0x02; // Arabic (Iraq)
		public const ushort SUBLANG_ARABIC_EGYPT				= 0x03; // Arabic (Egypt)
		public const ushort SUBLANG_ARABIC_LIBYA				= 0x04; // Arabic (Libya)
		public const ushort SUBLANG_ARABIC_ALGERIA				= 0x05; // Arabic (Algeria)
		public const ushort SUBLANG_ARABIC_MOROCCO				= 0x06; // Arabic (Morocco)
		public const ushort SUBLANG_ARABIC_TUNISIA				= 0x07; // Arabic (Tunisia)
		public const ushort SUBLANG_ARABIC_OMAN				 	= 0x08; // Arabic (Oman)
		public const ushort SUBLANG_ARABIC_YEMEN				= 0x09; // Arabic (Yemen)
		public const ushort SUBLANG_ARABIC_SYRIA				= 0x0A; // Arabic (Syria)
		public const ushort SUBLANG_ARABIC_JORDAN				= 0x0B; // Arabic (Jordan)
		public const ushort SUBLANG_ARABIC_LEBANON				= 0x0C; // Arabic (Lebanon)
		public const ushort SUBLANG_ARABIC_KUWAIT				= 0x0D; // Arabic (Kuwait)
		public const ushort SUBLANG_ARABIC_UAE           		= 0x0E; // Arabic (U.A.E)
		public const ushort SUBLANG_ARABIC_BAHRAIN			 	= 0x0F; // Arabic (Bahrain)
		public const ushort SUBLANG_ARABIC_QATAR				= 0x10; // Arabic (Qatar)
		public const ushort SUBLANG_AZERI_LATIN					= 0x01; // Azeri (Latin)
		public const ushort SUBLANG_AZERI_CYRILLIC				= 0x02; // Azeri (Cyrillic)
		public const ushort SUBLANG_CHINESE_TRADITIONAL 		= 0x01; // Chinese (Taiwan)
		public const ushort SUBLANG_CHINESE_SIMPLIFIED			= 0x02; // Chinese (PR China)
		public const ushort SUBLANG_CHINESE_HONGKONG			= 0x03; // Chinese (Hong Kong S.A.R., P.R.C.)
		public const ushort SUBLANG_CHINESE_SINGAPORE			= 0x04; // Chinese (Singapore)
		public const ushort SUBLANG_CHINESE_MACAU      			= 0x05; // Chinese (Macau S.A.R.)
		public const ushort SUBLANG_DUTCH               		= 0x01; // Dutch
		public const ushort SUBLANG_DUTCH_BELGIAN       		= 0x02; // Dutch (Belgian)
		public const ushort SUBLANG_ENGLISH_US          		= 0x01; // English (USA)
		public const ushort SUBLANG_ENGLISH_UK          		= 0x02; // English (UK)
		public const ushort SUBLANG_ENGLISH_AUS					= 0x03; // English (Australian)
		public const ushort SUBLANG_ENGLISH_CAN					= 0x04; // English (Canadian)
		public const ushort SUBLANG_ENGLISH_NZ          		= 0x05; // English (New Zealand)
		public const ushort SUBLANG_ENGLISH_EIRE				= 0x06; // English (Irish)
		public const ushort SUBLANG_ENGLISH_SOUTH_AFRICA		= 0x07; // English (South Africa)
		public const ushort SUBLANG_ENGLISH_JAMAICA		 		= 0x08; // English (Jamaica)
		public const ushort SUBLANG_ENGLISH_CARIBBEAN			= 0x09; // English (Caribbean)
		public const ushort SUBLANG_ENGLISH_BELIZE      		= 0x0A; // English (Belize)
		public const ushort SUBLANG_ENGLISH_TRINIDAD    		= 0x0B; // English (Trinidad)
		public const ushort SUBLANG_ENGLISH_ZIMBABWE    		= 0x0C; // English (Zimbabwe)
		public const ushort SUBLANG_ENGLISH_PHILIPPINES			= 0x0D; // English (Philippines)
		public const ushort SUBLANG_FRENCH				   		= 0x01; // French
		public const ushort SUBLANG_FRENCH_BELGIAN				= 0x02; // French (Belgian)
		public const ushort SUBLANG_FRENCH_CANADIAN		    	= 0x03; // French (Canadian)
		public const ushort SUBLANG_FRENCH_SWISS				= 0x04; // French (Swiss)
		public const ushort SUBLANG_FRENCH_LUXEMBOURG   		= 0x05; // French (Luxembourg)
		public const ushort SUBLANG_FRENCH_MONACO       		= 0x06; // French (Monaco)
		public const ushort SUBLANG_GERMAN              		= 0x01; // German
		public const ushort SUBLANG_GERMAN_SWISS        		= 0x02; // German (Swiss)
		public const ushort SUBLANG_GERMAN_AUSTRIAN     		= 0x03; // German (Austrian)
		public const ushort SUBLANG_GERMAN_LUXEMBOURG   		= 0x04; // German (Luxembourg)
		public const ushort SUBLANG_GERMAN_LIECHTENSTEIN		= 0x05; // German (Liechtenstein)
		public const ushort SUBLANG_ITALIAN             		= 0x01; // Italian
		public const ushort SUBLANG_ITALIAN_SWISS       		= 0x02; // Italian (Swiss)
		public const ushort SUBLANG_KASHMIRI_SASIA				= 0x02; // Kashmiri (South Asia)
		public const ushort SUBLANG_KASHMIRI_INDIA				= 0x02; // For app compatibility only
		public const ushort SUBLANG_KOREAN              		= 0x01; // Korean (Extended Wansung)
		public const ushort SUBLANG_LITHUANIAN          		= 0x01; // Lithuanian
		public const ushort SUBLANG_MALAY_MALAYSIA				= 0x01; // Malay (Malaysia)
		public const ushort SUBLANG_MALAY_BRUNEI_DARUSSAM		= 0x02; // Malay (Brunei Darussalam)
		public const ushort SUBLANG_NEPALI_INDIA				= 0x02; // Nepali (India)
		public const ushort SUBLANG_NORWEGIAN_BOKMAL			= 0x01; // Norwegian (Bokmal)
		public const ushort SUBLANG_NORWEGIAN_NYNORSK			= 0x02; // Norwegian (Nynorsk)
		public const ushort SUBLANG_PORTUGUESE					= 0x02; // Portuguese
		public const ushort SUBLANG_PORTUGUESE_BRAZILIAN		= 0x01; // Portuguese (Brazilian)
		public const ushort SUBLANG_SERBIAN_LATIN				= 0x02; // Serbian (Latin)
		public const ushort SUBLANG_SERBIAN_CYRILLIC			= 0x03; // Serbian (Cyrillic)
		public const ushort SUBLANG_SPANISH						= 0x01; // Spanish (Castilian)
		public const ushort SUBLANG_SPANISH_MEXICAN				= 0x02; // Spanish (Mexican)
		public const ushort SUBLANG_SPANISH_MODERN				= 0x03; // Spanish (Spain)
		public const ushort SUBLANG_SPANISH_GUATEMALA    		= 0x04; // Spanish (Guatemala)
		public const ushort SUBLANG_SPANISH_COSTA_RICA   		= 0x05; // Spanish (Costa Rica)
		public const ushort SUBLANG_SPANISH_PANAMA       		= 0x06; // Spanish (Panama)
		public const ushort SUBLANG_SPANISH_DOMINICAN_REPUBLIC	= 0x07; // Spanish (Dominican Republic)
		public const ushort SUBLANG_SPANISH_VENEZUELA     		= 0x08; // Spanish (Venezuela)
		public const ushort SUBLANG_SPANISH_COLOMBIA      		= 0x09; // Spanish (Colombia)
		public const ushort SUBLANG_SPANISH_PERU          		= 0x0a; // Spanish (Peru)
		public const ushort SUBLANG_SPANISH_ARGENTINA     		= 0x0b; // Spanish (Argentina)
		public const ushort SUBLANG_SPANISH_ECUADOR       		= 0x0c; // Spanish (Ecuador)
		public const ushort SUBLANG_SPANISH_CHILE         		= 0x0d; // Spanish (Chile)
		public const ushort SUBLANG_SPANISH_URUGUAY       		= 0x0e; // Spanish (Uruguay)
		public const ushort SUBLANG_SPANISH_PARAGUAY      		= 0x0f; // Spanish (Paraguay)
		public const ushort SUBLANG_SPANISH_BOLIVIA       		= 0x10; // Spanish (Bolivia)
		public const ushort SUBLANG_SPANISH_EL_SALVADOR   		= 0x11; // Spanish (El Salvador)
		public const ushort SUBLANG_SPANISH_HONDURAS      		= 0x12; // Spanish (Honduras)
		public const ushort SUBLANG_SPANISH_NICARAGUA     		= 0x13; // Spanish (Nicaragua)
		public const ushort SUBLANG_SPANISH_PUERTO_RICO   		= 0x14; // Spanish (Puerto Rico)
		public const ushort SUBLANG_SWEDISH						= 0x01; // Swedish
		public const ushort SUBLANG_SWEDISH_FINLAND				= 0x02; // Swedish (Finland)
		public const ushort SUBLANG_URDU_PAKISTAN				= 0x01; // Urdu (Pakistan)
		public const ushort SUBLANG_URDU_INDIA					= 0x02; // Urdu (India)
		public const ushort SUBLANG_UZBEK_LATIN					= 0x01; // Uzbek (Latin)
		public const ushort SUBLANG_UZBEK_CYRILLIC				= 0x02;	// Uzbek (Cyrillic)
		
		#endregion CodePages

		public const ushort CODEPAGE_ANSI_OEM_THAI					= 874;	// ANSI/OEM - Thai (same as 28605, ISO 8859-15)
		public const ushort CODEPAGE_ANSI_OEM_JAPANESE				= 932;	// ANSI/OEM - Japanese, Shift-JIS
		public const ushort CODEPAGE_ANSI_OEM_SIMPLIFIED_CHINESE	= 936;	// ANSI/OEM - Simplified Chinese (PRC, Singapore)
		public const ushort CODEPAGE_ANSI_OEM_KOREAN				= 949;	// ANSI/OEM - Korean (Unified Hangeul Code)
		public const ushort CODEPAGE_ANSI_OEM_TRADITIONAL_CHINESE	= 950;	// ANSI/OEM - Traditional Chinese (Taiwan; Hong Kong SAR, PRC)
		public const ushort CODEPAGE_ANSI_CENTRAL_EUROPEAN			= 1250;	// ANSI - Central European
		public const ushort CODEPAGE_ANSI_CYRILLIC					= 1251;	// ANSI - Cyrillic 
		public const ushort CODEPAGE_ANSI_LATIN_I					= 1252;	// ANSI - Latin I
		public const ushort CODEPAGE_ANSI_GREEK						= 1253;	// ANSI - Greek 
		public const ushort CODEPAGE_ANSI_TURKISH					= 1254;	// ANSI - Turkish 
		public const ushort CODEPAGE_ANSI_HEBREW					= 1255;	// ANSI - Hebrew 
		public const ushort CODEPAGE_ANSI_ARABIC					= 1256;	// ANSI - Arabic
		public const ushort CODEPAGE_ANSI_BALTIC					= 1257;	// ANSI - Baltic
		public const ushort CODEPAGE_ANSI_OEM_VIETNAMESE			= 1258;	// ANSI/OEM - Vietnamese

		// Font weights
		public const int FW_DONTCARE	= 0;
		public const int FW_THIN		= 100;
		public const int FW_EXTRALIGHT	= 200;
		public const int FW_LIGHT		= 300;
		public const int FW_NORMAL		= 400;
		public const int FW_MEDIUM		= 500;
		public const int FW_SEMIBOLD	= 600;
		public const int FW_BOLD		= 700;
		public const int FW_EXTRABOLD	= 800;
		public const int FW_HEAVY		= 900;
	
		// Font charsets
		public const int ANSI_CHARSET			= 0;
		public const int DEFAULT_CHARSET		= 1;
		public const int SYMBOL_CHARSET			= 2;
		public const int SHIFTJIS_CHARSET		= 128;
		public const int HANGEUL_CHARSET		= 129;
		public const int HANGUL_CHARSET			= 129;
		public const int GB2312_CHARSET			= 134;
		public const int CHINESEBIG5_CHARSET	= 136;
		public const int OEM_CHARSET			= 255;
		public const int JOHAB_CHARSET			= 130;
		public const int HEBREW_CHARSET			= 177;
		public const int ARABIC_CHARSET			= 178;
		public const int GREEK_CHARSET			= 161;
		public const int TURKISH_CHARSET		= 162;
		public const int VIETNAMESE_CHARSET		= 163;
		public const int THAI_CHARSET			= 222;
		public const int EASTEUROPE_CHARSET		= 238;
		public const int RUSSIAN_CHARSET		= 204;
		public const int MAC_CHARSET			= 77;
		public const int BALTIC_CHARSET			= 186;
		
		#endregion

		#region private constant data members
		
		private const int microsoftMaxIdentifierLength = 247;
		
		#endregion
		
		#endregion

		#region static data members

		public static char[] InvalidFolderNameChars = new char[]{'.','\'','\"','/','\\','[',']',';',':','|','=',',','?','*','+','<','>'};
		
		private static string[] ReservedApplicationSubFolderNames =
									{
										DatabaseScriptsFolderName,
										ModuleObjectsFolderName,
										ReferenceObjectsFolderName,
										ReportsFolderName
									};

		public static int DBObjectNameMaximumLength = Math.Min(SQLServerDBObjectNameMaximumLength, OracleDBObjectNameMaximumLength);
		public static char[] InvalidDBObjectNameChars = new char[]{' ','~','\'','!','%','@','^','&','*','(',')','+','-','=','{','}','[',']','|','\\',':',';','\"','<','>',',','.','?','/'};
		
		private static string[] reservedLanguageKeyWords = {
															   "align",
															   "__alignof",
															   "allocate",
															   "__asm",
															   "__assume",
															   "auto",
															   "__based",
															   "bool",
															   "break",
															   "case",
															   "catch",
															   "__cdecl",
															   "char",
															   "class",
															   "const",
															   "const_cast",
															   "continue",
															   "__declspec",
															   "default",
															   "delete",
															   "dllexport",
															   "dllimport",
															   "do",
															   "double",
															   "dynamic_cast",
															   "else",
															   "enum",
															   "__except",
															   "explicit",
															   "extern",
															   "false",
															   "__fastcall",
															   "__finally",
															   "float",
															   "for",
															   "__forceinline",
															   "friends",
															   "goto",
															   "if",
															   "__inline",
															   "inline",
															   "__int16",
															   "__int32",
															   "__int64",
															   "__int8",
															   "int",
															   "__interface",
															   "interface", 
															   "__leave",
															   "long",
															   "main",
															   "__multiple_inheritance",
															   "mutable",
															   "naked",
															   "namespace",
															   "new",
															   "noinline",
															   "__noop",
															   "noreturn",
															   "nothrow",
															   "novtable",
															   "operator",
															   "private",
															   "property",
															   "protected",
															   "__ptr64",
															   "public",
															   "register",
															   "reinterpret_cast",
															   "return",
															   "selectany",
															   "short",
															   "signed",
															   "__single_inheritance",
															   "sizeof",
															   "static_cast",
															   "static",
															   "__stdcall",
															   "struct", 
															   "__super",
															   "super",
															   "switch",
															   "template",
															   "this",
															   "thread",
															   "throw",
															   "true",
															   "__try",
															   "try",
															   "typedef",
															   "typeid",
															   "typename",
															   "union",
															   "unsigned",
															   "using",
															   "using declaration",
															   "using directive",
															   "__uuidof",
															   "uuid",
															   "virtual",
															   "__virtual_inheritance",
															   "void",
															   "volatile",
															   "while",
															   "wmain"
														   };

		private static string[] ReservedTableColumnNames =
									{
										TBCreatedColumnName,
										TBModifiedColumnName,
										TBCreatedIDColumnName,
										TBModifiedIDColumnName,
										TBGuidColumnName
									};

		#endregion

		#region LOGFONT structure definition
		
		private const short CCFACENAME = 32;
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)] 
			public struct LOGFONT 
		{ 
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCFACENAME)] 
			public string lfFaceName;
		}

		#endregion

		#region static methods

		//---------------------------------------------------------------------
		public static string CheckValidVersion(string aVersionToCheck)
		{
			if (aVersionToCheck == null || aVersionToCheck.Trim().Length == 0)
				return String.Empty;

			aVersionToCheck = aVersionToCheck.Trim();

			if (!System.Text.RegularExpressions.Regex.IsMatch(aVersionToCheck, @"^[1-9]{1,5}(\.[0-9]{1,5})*$"))
				return String.Empty;

			return aVersionToCheck;
		}
		
		//---------------------------------------------------------------------
		public static string CheckValidURL(string aURLToCheck)
		{
			if (aURLToCheck == null || aURLToCheck.Trim().Length == 0)
				return String.Empty;

			aURLToCheck = aURLToCheck.Trim();

			// A URL is a compact representation of the location and access method for a 
			// resource located on the Internet. Each URL consists of a scheme (HTTP, HTTPS, 
			// FTP, or Gopher) and a scheme-specific string. This string can also include a
			// combination of a directory path, search string, or name of the resource. 
			// The format of a URL is:
			//       <protocol>://[<user-info>]<host>[<port-info>]/[<url-path>]

			int protocolLength = aURLToCheck.IndexOf("://");
			if (protocolLength == 0)
				aURLToCheck = "http" + aURLToCheck;
			if (protocolLength < 0)
				aURLToCheck = "http://" + aURLToCheck;

			try
			{
				// A URI (Uniform Resource Identifier) is an address string referring to an 
				// object, typically on the Internet. 
				// The most common type of URI is the URL, in which the address maps onto an 
				// access algorithm using network protocols. Sometimes URI and URL are used
				// interchangeably.
				Uri uri = new Uri(aURLToCheck);
			}
			catch(UriFormatException)
			{
				return String.Empty;
			}

			return aURLToCheck;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidClassName(string aClassName)
		{
			return (IsValidLanguageIdentifier(aClassName));
		}

		//---------------------------------------------------------------------------
		// What constitutes a legal name in C++ is governed by very simple rules: 
		// A name can have one or more characters (C++ places no limits to the length 
		//   of an identifier). 
		// Only alphabetic characters, numeric digits, and the underscore character (_)
		// are legal in an identifier. 
		// The first character of an identifier must be alphabetic or an underscore 
		// (it cannot be a numeric digit). 
		// Upper case letters are considered distinct from lower case letters; that is,
		// identifiers are case sensitive. 
		// C++ reserved words cannot be used for identifiers. 
		//---------------------------------------------------------------------------
		public static bool IsValidLanguageIdentifier(string aIdentifier)
		{
			if (aIdentifier == null || aIdentifier.Length == 0)
				return false;

			aIdentifier = aIdentifier.Trim();

			if (aIdentifier.Length == 0 || aIdentifier.Length > microsoftMaxIdentifierLength)
				return false;

			// "Identifiers" or "symbols" are the names you supply for variables,
			// types, functions, and labels in your program.
			// Identifier names must differ in spelling and case from any keywords.
			// You cannot use keywords (either C or Microsoft) as identifiers; they
			// are reserved for special use. 

			// The first character of an identifier name must be a nondigit (that is, 
			// the first character must be an underscore or an uppercase or lowercase
			// letter).
			if (!Char.IsLetter(aIdentifier[0]))
			{
				if (aIdentifier[0] != '_')
					return false;
				
				// The ANSI C standard allows identifier names that begin with two
				// underscores or with an underscore followed by an uppercase letter
				// to be reserved for compiler use.
				// By convention, Microsoft uses an underscore and an uppercase letter 
				// to begin macro names and double underscores for Microsoft-specific
				// keyword names.
				if (aIdentifier.Length == 1 || !Char.IsLetter(aIdentifier[1]) || Char.IsUpper(aIdentifier[1]))
					return false;
			}

			foreach (char aCharacter in aIdentifier)
			{
				// Only alphabetic characters, numeric digits, and the underscore 
				// character (_) are legal in an identifier.
				if 
					(
					!Char.IsLetter(aCharacter) && 
					!Char.IsDigit(aCharacter) && 
					aCharacter != '_'
					)
					return false;
			}
			
			foreach (string keyWord in reservedLanguageKeyWords)
			{
				if (String.Compare(aIdentifier, keyWord) == 0)
					return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public static string SubstitueInvalidCharacterInIdentifier(string aIdentifier)
		{
			if (aIdentifier == null)
				return String.Empty;

			aIdentifier = aIdentifier.Trim();

			if (
				aIdentifier.Length == 0 ||
				IsValidLanguageIdentifier(aIdentifier)
				)
				return aIdentifier;

			if (aIdentifier.Length > microsoftMaxIdentifierLength)
				aIdentifier = aIdentifier.Substring(0, microsoftMaxIdentifierLength);

			char[] identifierCharacters = aIdentifier.ToCharArray();
			for (int i = 0; i < identifierCharacters.Length; i++)
			{
				// Only alphabetic characters, numeric digits, and the underscore 
				// character (_) are legal in an identifier.
				// The first character of an identifier must be alphabetic or an  
				// underscore(it cannot be a numeric digit). 
				if 
					(
					identifierCharacters[i] != '_' &&
					!Char.IsLetter(identifierCharacters[i]) && 
					(i == 0 || !Char.IsDigit(identifierCharacters[i]))
					)
					identifierCharacters[i] = '_';
			}

			return new string(identifierCharacters);
		}
			
		//---------------------------------------------------------------------------
		public static bool IsValidNameSpaceToken(string aToken)
		{
			return
				(
				aToken != null &&
				aToken.Trim().Length > 0 &&
				aToken.IndexOf('.') == -1 &&
				aToken.IndexOf(' ') == -1
				);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidApplicationName(string aApplicationName)
		{
			return (IsValidFolderName(aApplicationName) && IsValidNameSpaceToken(aApplicationName));
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidModuleName(string aModuleName)
		{
			if (!IsValidFolderName(aModuleName) || !IsValidNameSpaceToken(aModuleName))
				return false;

			foreach(string aReservedFolderName in ReservedApplicationSubFolderNames)
			{
				if (String.Compare(aReservedFolderName, aModuleName, true) == 0)
					return false;
			}

			return true;
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidLibraryName(string aLibraryName)
		{
			return
				(
				aLibraryName != null &&
				aLibraryName.Trim().Length > 0
				);
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidDocumentName(string aDocumentName)
		{
			return IsValidFolderName(aDocumentName) && IsValidNameSpaceToken(aDocumentName);
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidDBTName(string aDBTName)
		{
			return IsValidFolderName(aDBTName) && IsValidNameSpaceToken(aDBTName);
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidHotLinkName(string aHotLinkName)
		{
			return IsValidFolderName(aHotLinkName) && IsValidNameSpaceToken(aHotLinkName);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidFolderName(string aFolderName)
		{
			return
				(
				aFolderName != null &&
				aFolderName.Trim().Length > 0  &&
				aFolderName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1 &&
				aFolderName.IndexOfAny(InvalidFolderNameChars) == -1
				);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidPathName(string aPathName)
		{
			if
				(
				aPathName == null ||
				aPathName.Trim().Length == 0  ||
                aPathName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1
				)
				return false;

			aPathName = aPathName.Trim();
			
			if (Path.IsPathRooted(aPathName))
			{
				string pathRoot = Path.GetPathRoot(aPathName);
				if (pathRoot == null || pathRoot.Length == 0)
					return false;

				string pathToCheck = aPathName.Substring(pathRoot.Length);
				if (pathToCheck != null && pathToCheck.Length > 0 && pathToCheck[0] == Path.DirectorySeparatorChar)
					return false;
			}
			else if (aPathName.IndexOf(Path.VolumeSeparatorChar) != -1)
				return false;

			return true;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidFullPathName(string aFullPathName)
		{
			if (!IsValidPathName(aFullPathName))
				return false;

			return Path.IsPathRooted(aFullPathName);
		}

		//---------------------------------------------------------------------------
		// Gli identificatori degli oggetti di un database SQL Server, quali tabelle, 
		// viste e colonne, possono includere al massimo 128 caratteri, non possono 
		// contenere determinati caratteri e devono iniziare con una lettera o con '_'.
		// In Oracle, invece, non devono superare i 30 caratteri e devono sempre 
		// cominciare con una lettera, consistere interamente di caratteri alfanumerici
		// o dei caratteri speciali '_', '$' e '#'. 
		// In entrambi i casi non devono, ovviamente, coincidere con parole riservate.
		//---------------------------------------------------------------------------
		public static bool IsValidSQLServerDBObjectName(string aDBObjectName)
		{
			if (aDBObjectName == null)
				return false;

			string trimmedName = aDBObjectName.Trim();
			return
				(
				trimmedName.Length > 0  &&
				trimmedName.Length <= SQLServerDBObjectNameMaximumLength  &&
				trimmedName.IndexOfAny(InvalidDBObjectNameChars) == -1 &&
				(Char.IsLetter(trimmedName[0]) || trimmedName[0] == '_')
				);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidOracleDBObjectName(string aDBObjectName)
		{
			if (aDBObjectName == null)
				return false;

			string trimmedName = aDBObjectName.Trim();
			if
				(
				trimmedName.Length == 0  ||
				trimmedName.Length > OracleDBObjectNameMaximumLength ||
				!Char.IsLetter(trimmedName[0])
				)
				return false;

			if (trimmedName.Length > 1)
			{
				for (int i=1; i < trimmedName.Length; i++)
				{
					if 
						(
						!Char.IsLetterOrDigit(trimmedName[i]) && 
						trimmedName[i] != '_' && 
						trimmedName[i] != '$' && 
						trimmedName[i] != '#'
						)
						return false;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidDBObjectName(string aDBObjectName)
		{
			return (IsValidSQLServerDBObjectName(aDBObjectName));// && IsValidOracleDBObjectName(aDBObjectName));
		}

		// Il nome di una tabella può includere al massimo 128 caratteri, ad eccezione
		// dei nomi di tabelle temporanee locali, ovvero i nomi preceduti da un simbolo
		// di cancelletto (#), i quali devono includere al massimo 116 caratteri.
		//---------------------------------------------------------------------------
		public static bool IsValidSQLServerTableName(string aTableName)
		{
			return 
				(
				IsValidSQLServerDBObjectName(aTableName) &&
				(aTableName.Trim()[0] != '#' || aTableName.Trim().Length <= 116)
				);
		}
	
		//---------------------------------------------------------------------------
		public static bool IsValidOracleTableName(string aTableName)
		{
			return IsValidOracleDBObjectName(aTableName);
		}
	
		//---------------------------------------------------------------------------
		public static bool IsValidTableName(string aTableName)
		{
			return (IsValidSQLServerTableName(aTableName));// && IsValidOracleTableName(aTableName));
		}

		//---------------------------------------------------------------------------
		public static bool IsValidTableColumnName(string aColumnName)
		{
			return (IsValidDBObjectName(aColumnName) && !IsReservedTableColumnName(aColumnName));
		}

		//---------------------------------------------------------------------------
		public static bool IsTBGuidColumnName(string aColumnName)
		{
			return (String.Compare(aColumnName.Trim(), TBGuidColumnName, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		//---------------------------------------------------------------------------
		public static bool IsReservedTableColumnName(string aColumnName)
		{
			if (aColumnName == null)
				return false;

			string nameToCheck = aColumnName.Trim();
			if (nameToCheck.Length == 0)
				return false;

			foreach(string aReservedColumnName in ReservedTableColumnNames)
			{
				if (String.Compare(aReservedColumnName, nameToCheck, true) == 0)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidEnumName(string aEnumName)
		{
			return 
				(
				aEnumName != null && 
				aEnumName.Length > 0
				); //@@TODO vedere se a questo riguardo vengono aggiunte ulteriori restrizioni in TB
		}
		
		public const ushort MaxEnumValue = 65500;
		//---------------------------------------------------------------------------
		public static bool IsValidEnumValue(ushort aEnumValue)
		{
			return (aEnumValue > 0 && aEnumValue < MaxEnumValue);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidEnumItemName(string aEnumItemName)
		{
			return 
				(
				aEnumItemName != null && 
				aEnumItemName.Length > 0
				); //@@TODO vedere se a questo riguardo vengono aggiunte restizioni in TB
		}
		
		//---------------------------------------------------------------------------
		public static bool IsValidEnumItemValue(ushort aEnumItemValue)
		{
			return true; //@@TODO vedere se a questo riguardo vengono aggiunte restizioni in TB
		}

		//---------------------------------------------------------------------------
		// Metodo che va a vedere tutte le applicazioni correntemente installate e restituisce 
		// il valore massimo assunto dai tag degli enumerativi usati.
		// Se utilizzo tale valore per iniziare a numerare gli enumerativi di una nuova 
		// applicazione posso rendere più improbabile il pericolo che essi vadano a scontrarsi
		// con enumerativi esistenti.
		//---------------------------------------------------------------------------
		public static ushort GetInstalledApplicationsEnumsTagMaximum(BasePathFinder aPathFinder, string aApplicationNameToSkip)
		{
			if (aPathFinder == null || aPathFinder.ApplicationInfos == null || aPathFinder.ApplicationInfos.Count == 0)
				return 0;
	
			ushort maxEnumTag = 0;
			foreach (BaseApplicationInfo aApplicationInfo in aPathFinder.ApplicationInfos)
			{
				if 
					(
					aApplicationInfo.ApplicationType != ApplicationType.TaskBuilderApplication ||
					aApplicationInfo.Modules == null ||
					aApplicationInfo.Modules.Count == 0 ||
					(aApplicationNameToSkip != null && aApplicationNameToSkip.Length > 0 && String.Compare(aApplicationNameToSkip, aApplicationInfo.Name) == 0)
					)
					continue;

				foreach (BaseModuleInfo aModuleInfo in aApplicationInfo.Modules)
				{
					string moduleObjectsPath = aModuleInfo.GetModuleObjectPath();
					if (moduleObjectsPath == null || moduleObjectsPath.Length == 0)
						continue;

					string moduleEnumsFilename = moduleObjectsPath + 
						Path.DirectorySeparatorChar	+
						NameSolverStrings.EnumsXml;

					if (!File.Exists(moduleEnumsFilename))
						continue;

					Enums aEnumsParser = new Enums();
							
					try
					{
                        aEnumsParser.LoadXml(moduleEnumsFilename, aModuleInfo, false);
					}
					catch (EnumsException e)
					{
						Debug.Fail("EnumsException raised in Generics.GetInstalledApplicationsEnumsTagMaximum: " + e.Message);
						continue;
					}
					
					if (aEnumsParser.Tags == null || aEnumsParser.Tags.Count == 0)
						continue;
	
					foreach (EnumTag aEnumTag in aEnumsParser.Tags)
					{
						if (aEnumTag.Value > maxEnumTag)
							maxEnumTag = aEnumTag.Value;
					}
				}		
			}
			
			return maxEnumTag;
		}

		//---------------------------------------------------------------------------
		public static ushort GetInstalledApplicationsEnumsTagMaximum(string aApplicationNameToSkip)
		{
            return GetInstalledApplicationsEnumsTagMaximum(BasePathFinder.BasePathFinderInstance, aApplicationNameToSkip);
		}
		
		//---------------------------------------------------------------------------
		public static ushort GetInstalledApplicationsEnumsTagMaximum()
		{
			return GetInstalledApplicationsEnumsTagMaximum(null);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidCultureName(string aCultureName)
		{
			if (aCultureName == null || aCultureName.Length == 0)
				return false;
			try
			{
				CultureInfo ci = new CultureInfo(aCultureName);

				return true;
			}
			catch(ArgumentException)
			{
				// aCultureName is not a valid culture name.
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidResourceId(ushort aResourceId)
		{
			return (aResourceId >= MinimumResourceId && aResourceId <= MaximumResourceId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidResourceIdsRange(ushort aRange, ushort firstResourceId)
		{
			return ((firstResourceId + aRange) <= MaximumResourceId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidControlId(ushort aControlId)
		{
			return (aControlId >= MinimumControlId && aControlId <= MaximumControlId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidControlIdsRange(ushort aRange, ushort firstControlId)
		{
			return ((firstControlId + aRange) <= MaximumControlId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidCommandId(ushort aCommandId)
		{
			return (aCommandId >= MinimumCommandId && aCommandId <= MaximumCommandId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidCommandIdsRange(ushort aRange, ushort firstCommandId)
		{
			return ((firstCommandId + aRange) <= MaximumCommandId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidSymedId(ushort aSymedId)
		{
			return (aSymedId >= MinimumSymedId && aSymedId <= MaximumSymedId);
		}

		//---------------------------------------------------------------------------
		public static bool IsValidSymedIdsRange(ushort aRange, ushort firstSymedId)
		{
			return ((firstSymedId + aRange) <= MaximumSymedId);
		}
		
		//---------------------------------------------------------------------------
		public static string MakeRelativeTo(string aAbsolutePath, string aPathToMakeRelative)
		{
			try
			{
				if
					(
					aAbsolutePath == null ||
					aAbsolutePath.Trim().Length == 0  ||
					aPathToMakeRelative == null ||
					aPathToMakeRelative.Trim().Length == 0  ||
                    aAbsolutePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1 ||
                    aPathToMakeRelative.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1 ||
					!System.IO.Path.IsPathRooted(aAbsolutePath) ||
					!System.IO.Path.IsPathRooted(aPathToMakeRelative)
					)
					return String.Empty;

				if (String.Compare(Directory.GetDirectoryRoot(aAbsolutePath), Directory.GetDirectoryRoot(aPathToMakeRelative), true) != 0)
					return aPathToMakeRelative;

				char[] charsToTrim = new char [] {'\\', ' '};
				aAbsolutePath.TrimEnd(charsToTrim);
				aPathToMakeRelative.TrimEnd(charsToTrim);
				if (String.Compare(aAbsolutePath, aPathToMakeRelative, true) == 0)
					return String.Empty; // sono la stessa directory

				if 
					(
					aPathToMakeRelative.StartsWith(aAbsolutePath) && 
					aPathToMakeRelative.Length > aAbsolutePath.Length &&
					aPathToMakeRelative[aAbsolutePath.Length] == Path.DirectorySeparatorChar
					) // è una sottodirectory
					return aPathToMakeRelative.Substring(aAbsolutePath.Length + 1);

				DirectoryInfo dirInfo = new DirectoryInfo(aPathToMakeRelative);
				if (dirInfo == null || dirInfo.Parent == null)
					return String.Empty;

				string relativePath = dirInfo.Name;
				string matchingPath = dirInfo.Parent.FullName;

				while (matchingPath.Length > 0 && !(aAbsolutePath.ToLower().StartsWith(matchingPath.ToLower()) && aAbsolutePath[matchingPath.Length] == Path.DirectorySeparatorChar))
				{						
					relativePath = dirInfo.Parent.Name + Path.DirectorySeparatorChar + relativePath;

					dirInfo = dirInfo.Parent;
					if (dirInfo == null || dirInfo.Parent == null)
						break;

					matchingPath = dirInfo.Parent.FullName;
				}

				string moveUpDirSequence = ".." + Path.DirectorySeparatorChar;
				string moveUpDirString = moveUpDirSequence;

				if (aAbsolutePath.Length > matchingPath.Length)
				{
					string moveupPath = aAbsolutePath.Substring(matchingPath.Length + 1);
					int slashIdx = moveupPath.IndexOf(Path.DirectorySeparatorChar);
					while (slashIdx != -1 && slashIdx < (moveupPath.Length - 1))
					{
						moveUpDirString += moveUpDirSequence;
						moveupPath = moveupPath.Substring(slashIdx + 1);
						if (moveupPath == null || moveupPath.Length == 0)
							break;
						slashIdx = moveupPath.IndexOf(Path.DirectorySeparatorChar);
					}
				}

				return moveUpDirString + relativePath;
			}
			catch(Exception e)
			{
				Debug.Fail("Exception thrown in Generics.MakeRelativeTo: " + e.Message);
				return String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public static string BuildFullPath(string aAbsolutePath, string aRelativePath)
		{
			if
				(
				aAbsolutePath == null ||
				aAbsolutePath.Trim().Length == 0  ||
                aAbsolutePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1 ||
				aRelativePath == null ||
                aRelativePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1
				)
				return String.Empty;

			aRelativePath = aRelativePath.Trim();
			if
				(
				aRelativePath.Length == 0  ||
				!System.IO.Path.IsPathRooted(aAbsolutePath) ||
				System.IO.Path.IsPathRooted(aRelativePath)
				)
				return aRelativePath;

			DirectoryInfo dirInfo = new DirectoryInfo(aAbsolutePath);
			if (dirInfo == null)
				return aRelativePath;

			if (aRelativePath[0] == Path.DirectorySeparatorChar)
				aRelativePath = aRelativePath.Substring(1);
			
			string moveUpDirSequence = ".." + Path.DirectorySeparatorChar;
			int moveUpDirIndex = aRelativePath.Trim().IndexOf(moveUpDirSequence);
			while (moveUpDirIndex == 0 && aRelativePath.Trim().Length > 0)
			{

				dirInfo = dirInfo.Parent;
				if (dirInfo == null)
					break;
				
				aAbsolutePath = dirInfo.FullName;
				aRelativePath = aRelativePath.Trim().Substring(3);
				moveUpDirIndex = aRelativePath.Trim().IndexOf(moveUpDirSequence);
			}
			
			return Path.Combine(aAbsolutePath, aRelativePath);
		}

		//---------------------------------------------------------------------------
		public static string GetInstalledUICultureName()
		{
			return CultureInfo.InstalledUICulture.Name;
		}

		//---------------------------------------------------------------------------
		public static string GetCultureDisplayName(string aCultureName)
		{
			if (aCultureName == null || aCultureName.Length == 0)
				return String.Empty;

			try
			{
				CultureInfo ci = new CultureInfo(aCultureName);

				return ci.DisplayName;
			}
			catch(ArgumentException)
			{
				// aCultureName is not a valid culture name.
			}
			return String.Empty;
		}
		
		//---------------------------------------------------------------------------
		public static int GetCultureLCID(string aCultureName)
		{
			if (aCultureName == null || aCultureName.Length == 0)
				return 0;

			try
			{
				CultureInfo ci = new CultureInfo(aCultureName);

				return ci.LCID;
			}
			catch(ArgumentException)
			{
				// aCultureName is not a valid culture name.
			}
			return 0;
		}

		//---------------------------------------------------------------------
		//  A language ID is a 16 bit value which is the combination of a
		//  primary language ID and a secondary language ID.  The bits are
		//  allocated as follows:
		//
		//       +-----------------------+-------------------------+
		//       |     Sublanguage ID    |   Primary Language ID   |
		//       +-----------------------+-------------------------+
		//        15                   10 9                       0   bit
		//---------------------------------------------------------------------
		public static ushort MAKELANGID(ushort primaryLanguageID, ushort subLanguageId)
		{
			return (ushort)((subLanguageId << 10) | primaryLanguageID);
		}
		
		//---------------------------------------------------------------------
		public static ushort GetLanguageIdentifier(ushort languageID)
		{
			return (ushort)(languageID & 0x3ff);
		}

		//---------------------------------------------------------------------
		public static ushort GetSubLanguageIdentifier(ushort languageID)
		{
			return (ushort)(languageID >> 10);
		}

		//---------------------------------------------------------------------
		public static ushort GetCultureLanguageDefaultCodepage(CultureInfo aCultureInfo)
		{
			ushort languageId = GetLanguageIdentifier((ushort)aCultureInfo.LCID);

			switch(languageId)
			{
				case LANG_ARABIC:// 0x01
				case LANG_URDU: // 0x12	(Pakistan)
				case LANG_FARSI: // 0x12(Iran)
					return CODEPAGE_ANSI_ARABIC;		
				
				case LANG_ALBANIAN: // 0x1C
				case LANG_CZECH: // 0x05
				case LANG_HUNGARIAN: // 0x0E
				case LANG_POLISH: // 0x15
				case LANG_SLOVAK: // 0x1B
				case LANG_SLOVENIAN: // 0x24
					return CODEPAGE_ANSI_CENTRAL_EUROPEAN; // 1250

				case LANG_BELARUSIAN: // 0x23
				case LANG_BULGARIAN: // 0x02
				case LANG_KAZAK: // 0x3f
				case LANG_MACEDONIAN: // 0x2F
				case LANG_RUSSIAN: // 0x19
				case LANG_TATAR: // 0x25
				case LANG_UKRAINIAN: // 0x22
					return CODEPAGE_ANSI_CYRILLIC;

				case LANG_AZERI: // Azerbaijan
				{
					ushort subLanguageId = GetSubLanguageIdentifier((ushort)aCultureInfo.LCID);
					switch(subLanguageId)
					{
						case SUBLANG_AZERI_LATIN:
							return CODEPAGE_ANSI_CENTRAL_EUROPEAN;
						case SUBLANG_AZERI_CYRILLIC:
							return CODEPAGE_ANSI_CYRILLIC;
						default:
							break;
					}
					break;
				}

				case LANG_SERBIAN:
				{
					ushort subLanguageId = GetSubLanguageIdentifier((ushort)aCultureInfo.LCID);
					switch(subLanguageId)
					{
						case SUBLANG_SERBIAN_LATIN:
							return CODEPAGE_ANSI_CENTRAL_EUROPEAN;
						case SUBLANG_SERBIAN_CYRILLIC:
							return CODEPAGE_ANSI_CYRILLIC;
						default:
							break;
					}
					break;
				}
			
				case LANG_UZBEK:
				{
					ushort subLanguageId = GetSubLanguageIdentifier((ushort)aCultureInfo.LCID);
					switch(subLanguageId)
					{
						case SUBLANG_UZBEK_LATIN: // = 0x01 Uzbek (Latin)
							return CODEPAGE_ANSI_CENTRAL_EUROPEAN;
						case SUBLANG_UZBEK_CYRILLIC: // = 0x02 Uzbek (Cyrillic)
							return CODEPAGE_ANSI_CYRILLIC;
						default:
							break;
					}
					break;
				}

				case LANG_ESTONIAN: // 0x25
				case LANG_LATVIAN: // 0x26
				case LANG_LITHUANIAN: // 0x27
					return CODEPAGE_ANSI_BALTIC; // 1257

				case LANG_CHINESE: // 0x04
				{
					ushort subLanguageId = GetSubLanguageIdentifier((ushort)aCultureInfo.LCID);
					switch(subLanguageId)
					{						
						case SUBLANG_CHINESE_TRADITIONAL:
							return CODEPAGE_ANSI_OEM_TRADITIONAL_CHINESE;

						case SUBLANG_CHINESE_SIMPLIFIED:
						case SUBLANG_CHINESE_HONGKONG:
						case SUBLANG_CHINESE_SINGAPORE:
						case SUBLANG_CHINESE_MACAU:
							return CODEPAGE_ANSI_OEM_SIMPLIFIED_CHINESE;
						default:
							break;
					}
					break;
				}
				
				case LANG_KOREAN: // 0x12	
					return CODEPAGE_ANSI_OEM_KOREAN;			

				case LANG_JAPANESE: // 0x12	
					return CODEPAGE_ANSI_OEM_JAPANESE;			

				case LANG_THAI: // 0x1E Thailand
					return CODEPAGE_ANSI_OEM_THAI;			
				
				case LANG_VIETNAMESE: // 0x2A
					return CODEPAGE_ANSI_OEM_VIETNAMESE;
			
				case LANG_GREEK:// 0x01
					return CODEPAGE_ANSI_GREEK;		
				
				case LANG_TURKISH:// 0x01
					return CODEPAGE_ANSI_TURKISH;		
				
				case LANG_HEBREW:// 0x01
					return CODEPAGE_ANSI_HEBREW;		
				
				default:
					break;
			}			
			return CODEPAGE_ANSI_LATIN_I;
		}

		//---------------------------------------------------------------------
		public static string GetCultureLanguageIdentifierText(CultureInfo aCultureInfo)
		{
			if (aCultureInfo == null)
				return String.Empty;
			
			ushort languageId = GetLanguageIdentifier((ushort)aCultureInfo.LCID);

			switch(languageId)
			{
				case LANG_NEUTRAL: // 0x00
					return "LANG_NEUTRAL";
				case LANG_INVARIANT: // 0x7F
					return "LANG_INVARIANT";	
				case LANG_AFRIKAANS: // 0x36
					return "LANG_AFRIKAANS";
				case LANG_ALBANIAN: // 0x1C
					return "LANG_ALBANIAN";
				case LANG_ARABIC:// 0x01
					return "LANG_ARABIC";		
				case LANG_ARMENIAN: // 0x2B
					return "LANG_ARMENIAN";
				case LANG_ASSAMESE: // 0x4D
					return "LANG_ASSAMESE";
				case LANG_AZERI:// 0x2C
					return "LANG_AZERI";
				case LANG_BASQUE: // 0x2D
					return "LANG_BASQUE";		
				case LANG_BELARUSIAN: // 0x23
					return "LANG_BELARUSIAN";
				case LANG_BENGALI: // 0x45
					return "LANG_BENGALI";
				case LANG_BULGARIAN: // 0x02
					return "LANG_BULGARIAN";
				case LANG_CATALAN: // 0x03
					return "LANG_CATALAN";	
				case LANG_CHINESE: // 0x04
					return "LANG_CHINESE";
				case LANG_CROATIAN: // 0x1A
					return "LANG_CROATIAN";		
				case LANG_CZECH: // 0x05
					return "LANG_CZECH";
				case LANG_DANISH: // 0x06
					return "LANG_DANISH";
				case LANG_DIVEHI: // 0x65
					return "LANG_DIVEHI";
				case LANG_DUTCH: // 0x13
					return "LANG_DUTCH";
				case LANG_ENGLISH: // 0x09
					return "LANG_ENGLISH";
				case LANG_ESTONIAN: // 0x25
					return "LANG_ESTONIAN";
				case LANG_FAEROESE: // 0x38
					return "LANG_FAEROESE";
				case LANG_FARSI: // 0x29
					return "LANG_FARSI";
				case LANG_FINNISH: // 0x0B
					return "LANG_FINNISH";
				case LANG_FRENCH: // 0x0C
					return "LANG_FRENCH";
				case LANG_GALICIAN: // 0x56
					return "LANG_GALICIAN";
				case LANG_GEORGIAN: // 0x37
					return "LANG_GEORGIAN";
				case LANG_GERMAN: // 0x07
					return "LANG_GERMAN";
				case LANG_GREEK: // 0x08
					return "LANG_GREEK";		
				case LANG_GUJARATI: // 0x47
					return "LANG_GUJARATI";		
				case LANG_HEBREW: // 0x0D
					return "LANG_HEBREW";		
				case LANG_HINDI: // 0x39
					return "LANG_HINDI";		
				case LANG_HUNGARIAN: // 0x0E
					return "LANG_HUNGARIAN";
				case LANG_ICELANDIC: // 0x0F
					return "LANG_ICELANDIC";
				case LANG_INDONESIAN: // 0x21
					return "LANG_INDONESIAN";
				case LANG_ITALIAN: // 0x10
					return "LANG_ITALIAN";
				case LANG_JAPANESE: // 0x11
					return "LANG_JAPANESE";
				case LANG_KANNADA: // 0x4B
					return "LANG_KANNADA";		
				case LANG_KASHMIRI: // 0x60
					return "LANG_KASHMIRI";
				case LANG_KAZAK: // 0x3f
					return "LANG_KAZAK";	
				case LANG_KONKANI: // 0x57
					return "LANG_KONKANI";
				case LANG_KOREAN: // 0x12	
					return "LANG_KOREAN";
				case LANG_KYRGYZ: // 0x40
					return "LANG_KYRGYZ";
				case LANG_LATVIAN: // 0x26
					return "LANG_LATVIAN";	
				case LANG_LITHUANIAN: // 0x27
					return "LANG_LITHUANIAN";
				case LANG_MACEDONIAN: // 0x2F
					return "LANG_MACEDONIAN";
				case LANG_MALAY: // 0x3E
					return "LANG_MALAY";
				case LANG_MALAYALAM: // 0x4D
					return "LANG_MALAYALAM";
				case LANG_MANIPURI: // 0x58
					return "LANG_MANIPURI";
				case LANG_MARATHI: // 0x4E
					return "LANG_MARATHI";		
				case LANG_MONGOLIAN: // 0x50
					return "LANG_MONGOLIAN";
				case LANG_NEPALI: // 0x61
					return "LANG_NEPALI";
				case LANG_NORWEGIAN: // 0x14
					return "LANG_NORWEGIAN";
				case LANG_ORIYA: // 0x48
					return "LANG_ORIYA";
				case LANG_POLISH: // 0x15
					return "LANG_POLISH";
				case LANG_PORTUGUESE: // 0x16
					return "LANG_PORTUGUESE";
				case LANG_PUNJABI: // 0x46
					return "LANG_PUNJABI";
				case LANG_ROMANIAN: // 0x18
					return "LANG_ROMANIAN";
				case LANG_RUSSIAN: // 0x19
					return "LANG_RUSSIAN";
				case LANG_SANSKRIT: // 0x4F
					return "LANG_SANSKRIT";
					// case LANG_SERBIAN: // 0x1A == LANG_CROATIAN
					//		return "LANG_SERBIAN";
				case LANG_SINDHI: // 0x59
					return "LANG_SINDHI";
				case LANG_SLOVAK: // 0x1B
					return "LANG_SLOVAK";
				case LANG_SLOVENIAN: // 0x24
					return "LANG_SLOVENIAN";
				case LANG_SPANISH: // 0x0A
					return "LANG_SPANISH";
				case LANG_SWAHILI: // 0x41
					return "LANG_SWAHILI";		
				case LANG_SWEDISH: // 0x1D
					return "LANG_SWEDISH";	
				case LANG_SYRIAC: // 0x5A
					return "LANG_SYRIAC";
				case LANG_TAMIL: // 0x49
					return "LANG_TAMIL";
				case LANG_TATAR: // 0x44
					return "LANG_TATAR";	
				case LANG_TELUGU: // 0x4A
					return "LANG_TELUGU";	
				case LANG_THAI: // 0x1E
					return "LANG_THAI";			
				case LANG_TURKISH: // 0x1F
					return "LANG_TURKISH";
				case LANG_UKRAINIAN: // 0x22
					return "LANG_UKRAINIAN";
				case LANG_URDU: // 0x20
					return "LANG_URDU";
				case LANG_UZBEK: // 0x43
					return "LANG_UZBEK";
				case LANG_VIETNAMESE: // 0x2A
					return "LANG_VIETNAMESE";

				default:
					break;
			}			
			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetCultureLanguageIdentifierText(string aCultureName)
		{
			if (aCultureName == null || aCultureName.Length == 0)
				return String.Empty;

			try
			{
				CultureInfo ci = new CultureInfo(aCultureName);

				return GetCultureLanguageIdentifierText(ci);
			}
			catch(ArgumentException)
			{
				// aCultureName is not a valid culture name.
			}
			return String.Empty;
		}
		
		//---------------------------------------------------------------------
		public static string GetCultureSubLanguageIdentifierText(CultureInfo aCultureInfo)
		{
			if (aCultureInfo == null)
				return String.Empty;

			ushort languageId = GetLanguageIdentifier((ushort)aCultureInfo.LCID);
			ushort subLanguageId = GetSubLanguageIdentifier((ushort)aCultureInfo.LCID);

			if (languageId == LANG_ARABIC)
			{
				switch(subLanguageId)
				{
					case SUBLANG_ARABIC_SAUDI_ARABIA:
						return "SUBLANG_ARABIC_SAUDI_ARABIA"; // = 0x01 Arabic (Saudi Arabia)
					case SUBLANG_ARABIC_IRAQ:
						return "SUBLANG_ARABIC_IRAQ"; // = 0x02 Arabic (Iraq)
					case SUBLANG_ARABIC_EGYPT:
						return "SUBLANG_ARABIC_EGYPT"; // = 0x03 Arabic (Egypt)
					case SUBLANG_ARABIC_LIBYA:
						return "SUBLANG_ARABIC_LIBYA"; // = 0x04 Arabic (Libya)
					case SUBLANG_ARABIC_ALGERIA:
						return "SUBLANG_ARABIC_ALGERIA"; // = 0x05 Arabic (Algeria)
					case SUBLANG_ARABIC_MOROCCO:
						return "SUBLANG_ARABIC_MOROCCO"; // = 0x06 Arabic (Morocco)
					case SUBLANG_ARABIC_TUNISIA:
						return "SUBLANG_ARABIC_TUNISIA"; // = 0x07 Arabic (Tunisia)
					case SUBLANG_ARABIC_OMAN:
						return "SUBLANG_ARABIC_OMAN"; // = 0x08 Arabic (Oman)
					case SUBLANG_ARABIC_YEMEN:
						return "SUBLANG_ARABIC_YEMEN"; // = 0x09 Arabic (Yemen)
					case SUBLANG_ARABIC_SYRIA:
						return "SUBLANG_ARABIC_SYRIA"; // = 0x0A Arabic (Syria)
					case SUBLANG_ARABIC_JORDAN:
						return "SUBLANG_ARABIC_JORDAN"; // = 0x0B Arabic (Jordan)
					case SUBLANG_ARABIC_LEBANON:
						return "SUBLANG_ARABIC_LEBANON"; // = 0x0C Arabic (Lebanon)
					case SUBLANG_ARABIC_KUWAIT:
						return "SUBLANG_ARABIC_KUWAIT"; // = 0x0D Arabic (Kuwait)
					case SUBLANG_ARABIC_UAE:
						return "SUBLANG_ARABIC_UAE"; // = 0x0E Arabic (U.A.E)
					case SUBLANG_ARABIC_BAHRAIN:
						return "SUBLANG_ARABIC_BAHRAIN"; // = 0x0F Arabic (Bahrain)
					case SUBLANG_ARABIC_QATAR:
						return "SUBLANG_ARABIC_QATAR"; // = 0x10 Arabic (Qatar)
					default:
						break;
				}			
			}
			else if (languageId == LANG_AZERI) // Azerbaijan
			{
				switch(subLanguageId)
				{
					case SUBLANG_AZERI_LATIN:
						return "SUBLANG_AZERI_LATIN"; // = 0x01 Azeri (Latin)
					case SUBLANG_AZERI_CYRILLIC:
						return "SUBLANG_AZERI_CYRILLIC"; // = 0x02 Azeri (Cyrillic)
					default:
						break;
				}
			}
			else if (languageId == LANG_CHINESE)
			{
				switch(subLanguageId)
				{
					case SUBLANG_CHINESE_TRADITIONAL:
						return "SUBLANG_CHINESE_TRADITIONAL"; // = 0x01 Chinese (Taiwan)
					case SUBLANG_CHINESE_SIMPLIFIED:
						return "SUBLANG_CHINESE_SIMPLIFIED"; // = 0x02 Chinese (PR China)
					case SUBLANG_CHINESE_HONGKONG:
						return "SUBLANG_CHINESE_HONGKONG"; // = 0x03 Chinese (Hong Kong S.A.R., P.R.C.)
					case SUBLANG_CHINESE_SINGAPORE:
						return "SUBLANG_CHINESE_SINGAPORE"; // = 0x04 Chinese (Singapore)
					case SUBLANG_CHINESE_MACAU:
						return "SUBLANG_CHINESE_MACAU"; // = 0x05 Chinese (Macau S.A.R.)
					default:
						break;
				}
			}
			else if (languageId == LANG_DUTCH)
			{
				switch(subLanguageId)
				{
					case SUBLANG_DUTCH:
						return "SUBLANG_DUTCH"; // = 0x01 Dutch
					case SUBLANG_DUTCH_BELGIAN:
						return "SUBLANG_DUTCH_BELGIAN"; // = 0x02 Dutch (Belgian)
					default:
						break;
				}
			}
			else if (languageId == LANG_ENGLISH)
			{
				switch(subLanguageId)
				{
					case SUBLANG_ENGLISH_US:
						return "SUBLANG_ENGLISH_US"; // = 0x01 English (USA)
					case SUBLANG_ENGLISH_UK:
						return "SUBLANG_ENGLISH_UK"; // = 0x02 English (UK)
					case SUBLANG_ENGLISH_AUS:
						return "SUBLANG_ENGLISH_AUS"; // = 0x03 English (Australian)
					case SUBLANG_ENGLISH_CAN:
						return "SUBLANG_ENGLISH_CAN"; // = 0x04 English (Canadian)
					case SUBLANG_ENGLISH_NZ:
						return "SUBLANG_ENGLISH_NZ"; // = 0x05 English (New Zealand)
					case SUBLANG_ENGLISH_EIRE:
						return "SUBLANG_ENGLISH_EIRE"; // = 0x06 English (Irish)
					case SUBLANG_ENGLISH_SOUTH_AFRICA:
						return "SUBLANG_ENGLISH_SOUTH_AFRICA"; // = 0x07 English (South Africa)
					case SUBLANG_ENGLISH_JAMAICA:
						return "SUBLANG_ENGLISH_JAMAICA"; // = 0x08 English (Jamaica)
					case SUBLANG_ENGLISH_CARIBBEAN:
						return "SUBLANG_ENGLISH_CARIBBEAN"; // = 0x09 English (Caribbean)
					case SUBLANG_ENGLISH_BELIZE:
						return "SUBLANG_ENGLISH_BELIZE"; // = 0x0A English (Belize)
					case SUBLANG_ENGLISH_TRINIDAD:
						return "SUBLANG_ENGLISH_TRINIDAD"; // = 0x0B English (Trinidad)
					case SUBLANG_ENGLISH_ZIMBABWE:
						return "SUBLANG_ENGLISH_ZIMBABWE"; // = 0x0C English (Zimbabwe)
					case SUBLANG_ENGLISH_PHILIPPINES:
						return "SUBLANG_ENGLISH_PHILIPPINES"; // = 0x0D English (Philippines)
					default:
						break;
				}
			}
			else if (languageId == LANG_FRENCH)
			{
				switch(subLanguageId)
				{
					case SUBLANG_FRENCH:
						return "SUBLANG_FRENCH"; // = 0x01 French
					case SUBLANG_FRENCH_BELGIAN:
						return "SUBLANG_FRENCH_BELGIAN"; // = 0x02 French (Belgian)
					case SUBLANG_FRENCH_CANADIAN:
						return "SUBLANG_FRENCH_CANADIAN"; // = 0x03 French (Canadian)
					case SUBLANG_FRENCH_SWISS:
						return "SUBLANG_FRENCH_SWISS"; // = 0x04 French (Swiss)
					case SUBLANG_FRENCH_LUXEMBOURG:
						return "SUBLANG_FRENCH_LUXEMBOURG"; // = 0x05 French (Luxembourg)
					case SUBLANG_FRENCH_MONACO:
						return "SUBLANG_FRENCH_MONACO"; // = 0x06 French (Monaco)
					default:
						break;
				}
			}
			else if (languageId == LANG_GERMAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_GERMAN:
						return "SUBLANG_GERMAN"; // = 0x01 German
					case SUBLANG_GERMAN_SWISS:
						return "SUBLANG_GERMAN_SWISS"; // = 0x02 German (Swiss)
					case SUBLANG_GERMAN_AUSTRIAN:
						return "SUBLANG_GERMAN_AUSTRIAN"; // = 0x03 German (Austrian)
					case SUBLANG_GERMAN_LUXEMBOURG:
						return "SUBLANG_GERMAN_LUXEMBOURG"; // = 0x04 German (Luxembourg)
					case SUBLANG_GERMAN_LIECHTENSTEIN:
						return "SUBLANG_GERMAN_LIECHTENSTEIN"; // = 0x05 German (Liechtenstein)
					default:
						break;
				}
			}
			else if (languageId == LANG_ITALIAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_ITALIAN:
						return "SUBLANG_ITALIAN"; // = 0x01 Italian
					case SUBLANG_ITALIAN_SWISS:
						return "SUBLANG_ITALIAN_SWISS"; // = 0x02 Italian (Swiss)
					default:
						break;
				}
			}
			else if (languageId == LANG_KASHMIRI)
			{
				switch(subLanguageId)
				{
					case SUBLANG_KASHMIRI_SASIA:
					{
						System.OperatingSystem osInfo = System.Environment.OSVersion;
						
						if 
							(
							osInfo.Platform >= PlatformID.Win32NT &&
							osInfo.Version.Major >= 5 &&
							osInfo.Version.Minor >= 1
							)
							return "SUBLANG_KASHMIRI_SASIA"; // = 0x02 Kashmiri (South Asia) (Minimum system required = Windows XP)

						return "SUBLANG_KASHMIRI_INDIA"; // = 0x02 For app compatibility only
					}
					default:
						break;
				}
			}
			else if (languageId == LANG_KOREAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_KOREAN:
						return "SUBLANG_KOREAN"; // = 0x01 Korean (Extended Wansung)
					default:
						break;
				}
			}
			else if (languageId == LANG_LITHUANIAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_LITHUANIAN:
						return "SUBLANG_LITHUANIAN"; // = 0x01 Lithuanian
					default:
						break;
				}
			}
			else if (languageId == LANG_MALAY)
			{
				switch(subLanguageId)
				{
					case SUBLANG_MALAY_MALAYSIA:
						return "SUBLANG_MALAY_MALAYSIA"; // = 0x01 Malay (Malaysia)
					case SUBLANG_MALAY_BRUNEI_DARUSSAM:
						return "SUBLANG_MALAY_BRUNEI_DARUSSAM"; // = 0x02 Malay (Brunei Darussalam)
					default:
						break;
				}
			}
			else if (languageId == LANG_NEPALI)
			{
				switch(subLanguageId)
				{
					case SUBLANG_NEPALI_INDIA:
						return "SUBLANG_NEPALI_INDIA"; // = 0x02 Nepali (India)
					default:
						break;
				}
			}
			else if (languageId == LANG_NORWEGIAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_NORWEGIAN_BOKMAL:
						return "SUBLANG_NORWEGIAN_BOKMAL"; // = 0x01 Norwegian (Bokmal)
					case SUBLANG_NORWEGIAN_NYNORSK:
						return "SUBLANG_NORWEGIAN_NYNORSK"; // = 0x02 Norwegian (Nynorsk)
					default:
						break;
				}
			}
			else if (languageId == LANG_PORTUGUESE)
			{
				switch(subLanguageId)
				{
					case SUBLANG_PORTUGUESE_BRAZILIAN:
						return "SUBLANG_PORTUGUESE_BRAZILIAN"; // = 0x01 Portuguese (Brazilian)
					case SUBLANG_PORTUGUESE:
						return "SUBLANG_PORTUGUESE"; // = 0x02 Portuguese
					default:
						break;
				}
			}
			else if (languageId == LANG_SERBIAN)
			{
				switch(subLanguageId)
				{
					case SUBLANG_SERBIAN_LATIN:
						return "SUBLANG_SERBIAN_LATIN"; // = 0x02 Serbian (Latin)
					case SUBLANG_SERBIAN_CYRILLIC:
						return "SUBLANG_SERBIAN_CYRILLIC"; // = 0x03 Serbian (Cyrillic)
					default:
						break;
				}
			}
			else if (languageId == LANG_SPANISH)
			{
				switch(subLanguageId)
				{
					case SUBLANG_SPANISH:
						return "SUBLANG_SPANISH"; // = 0x01 Spanish (Castilian)
					case SUBLANG_SPANISH_MEXICAN:
						return "SUBLANG_SPANISH_MEXICAN"; // = 0x02 Spanish (Mexican)
					case SUBLANG_SPANISH_MODERN:
						return "SUBLANG_SPANISH_MODERN"; // = 0x03 Spanish (Spain)
					case SUBLANG_SPANISH_GUATEMALA:
						return "SUBLANG_SPANISH_GUATEMALA"; // = 0x04 Spanish (Guatemala)
					case SUBLANG_SPANISH_COSTA_RICA:
						return "SUBLANG_SPANISH_COSTA_RICA"; // = 0x05 Spanish (Costa Rica)
					case SUBLANG_SPANISH_PANAMA:
						return "SUBLANG_SPANISH_PANAMA"; // = 0x06 Spanish (Panama)
					case SUBLANG_SPANISH_DOMINICAN_REPUBLIC:
						return "SUBLANG_SPANISH_DOMINICAN_REPUBLIC"; // = 0x07	 Spanish (Dominican Republic)
					case SUBLANG_SPANISH_VENEZUELA:
						return "SUBLANG_SPANISH_VENEZUELA"; // = 0x08 Spanish (Venezuela)
					case SUBLANG_SPANISH_COLOMBIA:
						return "SUBLANG_SPANISH_COLOMBIA"; // = 0x09 Spanish (Colombia)
					case SUBLANG_SPANISH_PERU:
						return "SUBLANG_SPANISH_PERU"; // = 0x0A Spanish (Peru)
					case SUBLANG_SPANISH_ARGENTINA:
						return "SUBLANG_SPANISH_ARGENTINA"; // = 0x0B Spanish (Argentina)
					case SUBLANG_SPANISH_ECUADOR:
						return "SUBLANG_SPANISH_ECUADOR"; // = 0x0C Spanish (Ecuador)
					case SUBLANG_SPANISH_CHILE:
						return "SUBLANG_SPANISH_CHILE"; // = 0x0D Spanish (Chile)
					case SUBLANG_SPANISH_URUGUAY:
						return "SUBLANG_SPANISH_URUGUAY"; // = 0x0E Spanish (Uruguay)
					case SUBLANG_SPANISH_PARAGUAY:
						return "SUBLANG_SPANISH_PARAGUAY"; // = 0x0F Spanish (Paraguay)
					case SUBLANG_SPANISH_BOLIVIA:
						return "SUBLANG_SPANISH_BOLIVIA"; // = 0x10 Spanish (Bolivia)
					case SUBLANG_SPANISH_EL_SALVADOR:
						return "SUBLANG_SPANISH_EL_SALVADOR"; // = 0x11 Spanish (El Salvador)
					case SUBLANG_SPANISH_HONDURAS:
						return "SUBLANG_SPANISH_HONDURAS"; // = 0x12 Spanish (Honduras)
					case SUBLANG_SPANISH_NICARAGUA:
						return "SUBLANG_SPANISH_NICARAGUA"; // = 0x13 Spanish (Nicaragua)
					case SUBLANG_SPANISH_PUERTO_RICO:
						return "SUBLANG_SPANISH_PUERTO_RICO"; // = 0x14 Spanish (Puerto Rico)
					default:
						break;
				}
			}
			else if (languageId == LANG_SWEDISH)
			{
				switch(subLanguageId)
				{
					case SUBLANG_SWEDISH:
						return "SUBLANG_SWEDISH"; // = 0x01 Swedish
					case SUBLANG_SWEDISH_FINLAND:
						return "SUBLANG_SWEDISH_FINLAND"; // = 0x02 Swedish (Finland)
					default:
						break;
				}
			}
			else if (languageId == LANG_URDU)
			{
				switch(subLanguageId)
				{
					case SUBLANG_URDU_PAKISTAN:
						return "SUBLANG_URDU_PAKISTAN"; // = 0x01 Urdu (Pakistan)
					case SUBLANG_URDU_INDIA:
						return "SUBLANG_URDU_INDIA"; // = 0x02 Urdu (India)	
					default:
						break;
				}
			}
			else if (languageId == LANG_UZBEK)
			{
				switch(subLanguageId)
				{
					case SUBLANG_UZBEK_LATIN: // = 0x01 Uzbek (Latin)
						return "SUBLANG_UZBEK_LATIN"; 
					case SUBLANG_UZBEK_CYRILLIC: // = 0x02 Uzbek (Cyrillic)
						return "SUBLANG_UZBEK_CYRILLIC"; 		
					default:
						break;
				}
			}

			switch(subLanguageId)
			{
				case SUBLANG_NEUTRAL: // = 0x00 language neutral
					return "SUBLANG_NEUTRAL";
				case SUBLANG_DEFAULT: // = 0x01 user default
					return "SUBLANG_DEFAULT";
				case SUBLANG_SYS_DEFAULT: // = 0x02 system default
					return "SUBLANG_SYS_DEFAULT";
				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetCultureSubLanguageIdentifierText(string aCultureName)
		{
			if (aCultureName == null || aCultureName.Length == 0)
				return String.Empty;

			try
			{
				CultureInfo ci = new CultureInfo(aCultureName);

				return GetCultureSubLanguageIdentifierText(ci);
			}
			catch(ArgumentException)
			{
				// aCultureName is not a valid culture name.
			}
			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static bool CopyEmbeddedResourceToFile(string resourceName, string outputFilename)
		{
			if
				(
				resourceName == null || 
				resourceName.Length == 0 || 
				outputFilename == null || 
				outputFilename.Length == 0 ||
				!IsValidFullPathName(outputFilename)
				)
				return false;

			System.IO.Stream streamToCopy = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);		
			if (streamToCopy == null)
				return false;

			int bytesToRead = (int)streamToCopy.Length;
			byte[] bytesBuffer = new byte[bytesToRead];
			int totalBytesRead = 0;
			System.IO.FileStream outputStream = null;
			BinaryWriter outputWriter = null;

			try
			{
				outputStream = new FileStream(outputFilename, FileMode.OpenOrCreate);
				outputWriter = new BinaryWriter(outputStream);

				streamToCopy.Position = 0;
				while (bytesToRead > 0) 
				{
					// Read may return anything from 0 to numBytesToRead.
					int bytesRead = streamToCopy.Read(bytesBuffer, totalBytesRead, bytesBuffer.Length);
		
					// The end of the file is reached.
					if (bytesRead==0)
						break;

					outputWriter.Write(bytesBuffer);
							
					totalBytesRead += bytesRead;
					bytesToRead -= bytesRead;
				}

				if (totalBytesRead != streamToCopy.Length)
					return false;
					
				outputWriter.Flush();

				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in Generics.CopyEmbeddedResourceToFile:" + exception.Message);
				return false;
			}
			finally
			{
				if (outputWriter != null)
					outputWriter.Close();
				if (outputStream != null)
					outputStream.Close();
				streamToCopy.Close();
			}
		}
	
		//----------------------------------------------------------------------------
		public static System.Diagnostics.Process OpenSolutionInDevEnv(string solutionFileToOpen)
		{
			if (solutionFileToOpen == null || solutionFileToOpen.Length == 0 || !File.Exists(solutionFileToOpen))
				return null;

			try
			{
				string arguments = String.Empty;
				string devEnvCommand = GetDefaultDevEnvCommand(solutionFileToOpen, ref arguments);
				if (devEnvCommand == null || devEnvCommand.Length == 0)
					return null;

				System.Diagnostics.Process devEnvProcess = new System.Diagnostics.Process();

				devEnvProcess.StartInfo.FileName = devEnvCommand;
				devEnvProcess.StartInfo.Arguments = arguments;
				devEnvProcess.StartInfo.UseShellExecute = true;
				devEnvProcess.EnableRaisingEvents = true;

				devEnvProcess.Start();

				return devEnvProcess;
			}
			catch(Exception)
			{
				return null;
			}
		}
		
		//----------------------------------------------------------------------------
		public static string GetDefaultDevEnvCommand(string solutionFileToOpen, ref string arguments)
		{
			arguments = String.Empty;
			string devEnvCommandLine = String.Empty;

			try
			{
				string devEnvToUse = String.Empty;
				string registryKeyPath = NetSolutionExtension;
				Microsoft.Win32.RegistryKey defaultDevEnvRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(registryKeyPath);
				if (defaultDevEnvRegKey == null)
					devEnvToUse = "VisualStudio.Solution";
				else
					devEnvToUse = defaultDevEnvRegKey.GetValue("").ToString();

				// Controllo che esista la chiave nel registry
				if (Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(devEnvToUse, false) == null)
					return null;

				registryKeyPath = devEnvToUse + "\\Shell\\open\\Command";
				defaultDevEnvRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(registryKeyPath, false);
				if (defaultDevEnvRegKey == null)
					return null;
			
				devEnvCommandLine = defaultDevEnvRegKey.GetValue("").ToString();
			}
			catch(SecurityException exception)
			{
				// The user does not have RegistryPermission.SetInclude(delete, currentKey) access.
				Debug.Fail("SecurityException thrown in Generics.GetDefaultDevEnvCommand", exception.Message);
				return null;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception thrown in Generics.GetDefaultDevEnvCommand", exception.Message);
				return null;
			}
			
			if (devEnvCommandLine == null || devEnvCommandLine.Length == 0)
				return null;

			devEnvCommandLine = devEnvCommandLine.Replace("\"", "");

			int exeExtensionIndex = devEnvCommandLine.IndexOf(".exe ");
			
			arguments = devEnvCommandLine.Substring(exeExtensionIndex + 4);

			if (arguments.IndexOf("%1") != -1)
				arguments = arguments.Replace("%1", solutionFileToOpen);
			else
				arguments += " " + solutionFileToOpen;

			return devEnvCommandLine.Substring(0, exeExtensionIndex + 4);
		}
		
		//----------------------------------------------------------------------------
		public static bool IsDefaultDevEnvShellCommandDefined()
		{
			try
			{
				string devEnvToUse = String.Empty;
				string registryKeyPath = NetSolutionExtension;
				Microsoft.Win32.RegistryKey defaultDevEnvRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(registryKeyPath);
				if (defaultDevEnvRegKey == null)
					devEnvToUse = "VisualStudio.Solution";
				else
					devEnvToUse = defaultDevEnvRegKey.GetValue("").ToString();

				// Controllo che esista la chiave nel registry
				if (Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(devEnvToUse, false) == null)
					return false;

				registryKeyPath = devEnvToUse + "\\Shell\\open\\Command";
				defaultDevEnvRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(registryKeyPath, false);
				if (defaultDevEnvRegKey == null)
					return false;
			
				string devEnvCommandLine = defaultDevEnvRegKey.GetValue("").ToString();
				return (devEnvCommandLine != null && devEnvCommandLine.Length > 0);
			}
			catch(Exception)
			{
				return false;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool IsProcessRunning(System.Diagnostics.Process aProcess)
		{
			try
			{
				return (aProcess != null && !aProcess.HasExited);
			}
			catch(Win32Exception)
			{
				// The process associated with currentTBProcess has exited but
				// the exit code for the process could not be retrieved.
				return false;
			}
			catch(InvalidOperationException)
			{
				// There is no process associated with currentTBProcess
				return false;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public static void SetProcessMainWindowInForeground(System.Diagnostics.Process aProcess)
		{		
			System.IntPtr processMainWindowHandle = IntPtr.Zero;
			try
			{
				if (!IsProcessRunning(aProcess))
					return;
			
				// If you have just started a process and want to use its main window handle, 
				// consider using the WaitForInputIdle method to allow the process to finish 
				// starting, ensuring that the main window handle has been created. Otherwise, 
				// an exception will be thrown.
				aProcess.WaitForInputIdle();
				
				processMainWindowHandle = aProcess.MainWindowHandle;
			}
			catch(InvalidOperationException)
			{
				// The MainWindowHandle is not defined because the process has exited.
				return;
			}
			catch(NotSupportedException)
			{
				// You are attempting to retrieve the MainWindowHandle for a process that is 
				// running on a remote computer.
				return;
			}
			
			if (processMainWindowHandle == IntPtr.Zero)
				return;
		
			User32.WINDOWPLACEMENT windowPlacement = new User32.WINDOWPLACEMENT();
			windowPlacement.length = 44;
			if 
				(
				User32.GetWindowPlacement(processMainWindowHandle, ref windowPlacement) &&
				windowPlacement.showCmd == User32.SW_SHOWMINIMIZED
				)
			{
				User32.ShowWindow(processMainWindowHandle, User32.SW_RESTORE);
			}
			User32.SetForegroundWindow(processMainWindowHandle);
		}

		//----------------------------------------------------------------------------
		public static int GetFontWeight(System.Drawing.Font aFont)
		{
			if (aFont == null)
				return FW_DONTCARE;
				
			LOGFONT logFont = new LOGFONT();
			aFont.ToLogFont(logFont);

			return logFont.lfWeight;
		}
			
		//----------------------------------------------------------------------------
		public static int GetFontCharSet(System.Drawing.Font aFont)
		{
			if (aFont == null)
				return FW_DONTCARE;
				
			LOGFONT logFont = new LOGFONT();
			aFont.ToLogFont(logFont);

			return logFont.lfCharSet;
		}
	
		// Characters such as the > and < characters are XML markup characters
		// and have special meaning in XML. When these characters are specified
		// in an XPath queries, they must be properly encoded (also referred to
		// as entity encoding). 
		// Special character	Special meaning		Entity encoding 
		//		>					Begins a tag.		&gt; 
		//		<					Ends a tag.			&lt; 
		//		'					Apostrophe.			&apos; 
		//		"					Quotation mark.		&quot;  
		//		&					Ampersand.			&amp;  
		private static char[] XMLReservedCharacters = new char[]{'&','<','>','\'','\"'};
		private static string[] XMLEntityReferences = new string[]{"&amp;","&lt;","&gt;","&quot;","&apos;"};
		//---------------------------------------------------------------------
		public static string SubstituteXMLReservedCharacters(string aTextToCheck)
		{
			if (aTextToCheck == null || aTextToCheck.Trim().Length == 0)
				return aTextToCheck;

			for(int i = 0; i < XMLReservedCharacters.Length; i++)
			{
				int firstOccurenceIndex = aTextToCheck.IndexOf(XMLReservedCharacters[i]);
				if (firstOccurenceIndex == -1)
					continue;
				if (!aTextToCheck.Substring(firstOccurenceIndex).StartsWith(XMLEntityReferences[i]))
				{
					do
					{
						string tmpText = String.Empty;
						if (firstOccurenceIndex > 0)
							tmpText = aTextToCheck.Substring(0, firstOccurenceIndex);
						tmpText += XMLEntityReferences[i];
						if (firstOccurenceIndex < (aTextToCheck.Length - 1))
							tmpText += aTextToCheck.Substring(firstOccurenceIndex + 1);
						aTextToCheck = tmpText;
						firstOccurenceIndex += XMLEntityReferences[i].Length;
						firstOccurenceIndex = aTextToCheck.IndexOf(XMLReservedCharacters[i], firstOccurenceIndex);
					}while(firstOccurenceIndex >= 0);
				}
			}
			return aTextToCheck;
		}

		private const string FrameworkTbGenlibModuleNamespace = "Module.Framework.TbGenlib";
		private const string SettingsConfigFileName = "Settings.config";
		private const string HotlinkComboDefaultFieldsXPathExpression = "ParameterSettings/Section[@name='Forms']/Setting[@name='HotlinkComboDefaultFields']";
		private const string ParameterSettingsValueAttribute = "value";
		//---------------------------------------------------------------------
		private static string[] GetHotlinkComboDefaultFields(string aSettingsFileName)
		{
			if 
				(
				aSettingsFileName == null || 
				aSettingsFileName.Trim().Length == 0 ||
				!File.Exists(aSettingsFileName)
				)
				return null;

			try
			{
				XmlDocument settingsDocument = new XmlDocument();
					
				settingsDocument.Load(aSettingsFileName);

				XmlNode nodeFound = settingsDocument.SelectSingleNode(HotlinkComboDefaultFieldsXPathExpression);
				if 
					(
					nodeFound != null && 
					nodeFound is XmlElement &&
					((XmlElement)nodeFound).HasAttribute(ParameterSettingsValueAttribute) 
					)
				{
					string fieldsValue = ((XmlElement)nodeFound).GetAttribute(ParameterSettingsValueAttribute);
					if (fieldsValue == null || fieldsValue.Trim().Length == 0)
						return null;
					string[] fields = fieldsValue.Split(new char[] {','});
			
					if (fields == null || fields.Length == 0) 
						return null;

					ArrayList validFieldsNames = new ArrayList();
					for(int i = 0; i < fields.Length; i++)
					{
						if (fields[i] == null || fields[i].Trim().Length == 0)
							continue;
						validFieldsNames.Add(fields[i]);
					}
					
					return (validFieldsNames.Count > 0) ? (string[])validFieldsNames.ToArray(typeof(string)) : null;
				}
			}
			catch(XmlException)
			{
			}

			return null;
		}
		
		//---------------------------------------------------------------------
		public static string[] GetHotLinkComboDefaultColumnNames(IBasePathFinder aPathFinder)
		{
            if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;

            IBaseModuleInfo frameworkTbGenlibInfo = aPathFinder.GetModuleInfo(new NameSpace(FrameworkTbGenlibModuleNamespace));
			if (frameworkTbGenlibInfo == null)
				return null;

			return GetHotlinkComboDefaultFields(frameworkTbGenlibInfo.GetStandardSettingsFullFilename(SettingsConfigFileName));
		}

		#endregion

		#region GDI32 class

		public class GDI32
		{
			private const short CCDEVICENAME = 32;
			private const short CCFORMNAME = 32;
			[StructLayout(LayoutKind.Sequential)] 
				public struct DEVMODE 
			{ 
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCDEVICENAME)] 
				public string dmDeviceName;
				public short dmSpecVersion;
				public short dmDriverVersion;
				public short dmSize;
				public short dmDriverExtra;
				public int dmFields;
				public short dmOrientation;
				public short dmPaperSize;
				public short dmPaperLength;
				public short dmPaperWidth;
				public short dmScale;
				public short dmCopies;
				public short dmDefaultSource;
				public short dmPrintQuality;
				public short dmColor;
				public short dmDuplex;
				public short dmYResolution;
				public short dmTTOption;
				public short dmCollate;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCFORMNAME)] 
				public string dmFormName; 
				public short dmUnusedPadding;
				public short dmBitsPerPel;
				public int dmPelsWidth;
				public int dmPelsHeight;
				public int dmDisplayFlags;
				public int dmDisplayFrequency;
			}
		
			[DllImport("gdi32.dll", EntryPoint="CreateIC", CharSet=CharSet.Auto)]
			public static extern IntPtr CreateIC(string lpDriverName, string lpDeviceName, string lpOutput, ref DEVMODE lpInitData);
			[DllImport("gdi32.dll",EntryPoint="DeleteDC", CharSet=CharSet.Auto)]
			public static extern IntPtr DeleteDC(IntPtr hDc);			
		
			#region static methods

			//----------------------------------------------------------------------------
			public static bool Baseunit(string fontFamilyName, float fontEmSize, out double width, out double height)
			{
				DEVMODE nullDevmode = new DEVMODE();
				IntPtr displayHdc = CreateIC("DISPLAY", null, null, ref nullDevmode);
				width = 0; height = 0;
				if (displayHdc == IntPtr.Zero)
					return false;

				System.Drawing.Graphics displayGraphics = Graphics.FromHdc(displayHdc);
				if (displayGraphics == null)
					return false;

				System.Drawing.Font currFont = new Font(fontFamilyName, fontEmSize, FontStyle.Regular, GraphicsUnit.Point);

				System.Drawing.SizeF baseUnitSize = displayGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", currFont);

				baseUnitSize.Width = (baseUnitSize.Width / 26 + 1) / 2;
				baseUnitSize.Height = currFont.Height;

				currFont.Dispose();
				displayGraphics.Dispose();
				DeleteDC(displayHdc);

				width = (1 + 4 / (double)baseUnitSize.Width); 
				height = (1 + 8 / (double)baseUnitSize.Height);
				return true;
			}

			//----------------------------------------------------------------------------
			public static System.Drawing.Size LUtoDU(string fontFamilyName, float fontEmSize, System.Drawing.Size sizeToConvert)
			{
				if (sizeToConvert == Size.Empty)
					return Size.Empty;

				if (sizeToConvert.Width == 0 && sizeToConvert.Height == 0)
					return sizeToConvert;

				DEVMODE nullDevmode = new DEVMODE();
				IntPtr displayHdc = CreateIC("DISPLAY", null, null, ref nullDevmode);
				if (displayHdc == IntPtr.Zero)
					return Size.Empty;

				System.Drawing.Graphics displayGraphics = Graphics.FromHdc(displayHdc);
				if (displayGraphics == null)
					return Size.Empty;
				
				System.Drawing.Font currFont = new Font(fontFamilyName, fontEmSize, FontStyle.Regular, GraphicsUnit.Point);

				System.Drawing.SizeF baseUnitSize = displayGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", currFont);
				
				baseUnitSize.Width  = (baseUnitSize.Width / 26 + 1) / 2;
				baseUnitSize.Height = currFont.Height;

				currFont.Dispose();
								
				displayGraphics.Dispose();

				DeleteDC(displayHdc);

				return new System.Drawing.Size
					(
					(int)Math.Ceiling(sizeToConvert.Width * 4.0 / (double)baseUnitSize.Width),
					(int)Math.Ceiling(sizeToConvert.Height * 8.0 / (double)baseUnitSize.Height)
					); 
			}
			
			//---------------------------------------------------------------------
			public static System.Drawing.Size GetTextDisplaySize(string aTextToMeasure, string fontFamilyName, float fontEmSize)
			{
				if (aTextToMeasure == null || aTextToMeasure.Length == 0)
					return Size.Empty;

				DEVMODE nullDevmode = new DEVMODE();
				IntPtr displayHdc = CreateIC("DISPLAY", null, null, ref nullDevmode);
				if (displayHdc == IntPtr.Zero)
					return Size.Empty;
	
				System.Drawing.Graphics displayGraphics = Graphics.FromHdc(displayHdc);
				if (displayGraphics == null)
					return Size.Empty;
				
				System.Drawing.Font currFont = new Font(fontFamilyName, fontEmSize, FontStyle.Regular, GraphicsUnit.Point);

				SizeF textRSizeF = displayGraphics.MeasureString(aTextToMeasure + "W", currFont);
				int textWidth = Convert.ToInt32(Math.Ceiling(textRSizeF.Width));
				int textHeight = Convert.ToInt32(Math.Ceiling(textRSizeF.Height));

				currFont.Dispose();
								
				displayGraphics.Dispose();

				DeleteDC(displayHdc);

				return LUtoDU(fontFamilyName, fontEmSize, new Size(textWidth, textHeight));
			}

			#endregion		
		}
		
		#endregion

		#region GDI32 class

		public class User32
		{
			[StructLayout(LayoutKind.Sequential, Pack=8, CharSet=CharSet.Auto)]
				public struct WINDOWPLACEMENT 
			{
				public UInt32  length;
				public UInt32  flags;
				public UInt32  showCmd;
				public UInt32  ptMinPositionX;
				public UInt32  ptMinPositionY;
				public UInt32  ptMaxPositionX;
				public UInt32  ptMaxPositionY;
				public UInt32  rcNormalPositionLeft;
				public UInt32  rcNormalPositionTop;
				public UInt32  rcNormalPositionRight;
				public UInt32  rcNormalPositionBottom;
			}
			public const UInt32 SW_HIDE             = 0;
			public const UInt32 SW_SHOWNORMAL       = 1;
			public const UInt32 SW_NORMAL           = 1;
			public const UInt32 SW_SHOWMINIMIZED    = 2;
			public const UInt32 SW_SHOWMAXIMIZED    = 3;
			public const UInt32 SW_MAXIMIZE         = 3;
			public const UInt32 SW_SHOWNOACTIVATE   = 4;
			public const UInt32 SW_SHOW             = 5;
			public const UInt32 SW_MINIMIZE         = 6;
			public const UInt32 SW_SHOWMINNOACTIVE  = 7;
			public const UInt32 SW_SHOWNA           = 8;
			public const UInt32 SW_RESTORE          = 9;
			public const UInt32 SW_SHOWDEFAULT      = 10;
			public const UInt32 SW_FORCEMINIMIZE    = 11;
			public const UInt32 SW_MAX              = 11;

			[DllImport("User32", CharSet=CharSet.Auto)]
			public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT windowPlacement); 
			[DllImport("User32", CharSet=CharSet.Auto)]
			public static extern bool ShowWindow(IntPtr hWnd, UInt32 showCmd); 
			[DllImport("user32.dll")]
			public static extern UInt32 SetForegroundWindow(IntPtr hWnd);
		}

		#endregion

		public static BasePathFinder CreateWizardPathFinder ()
		{
			return BasePathFinder.BasePathFinderInstance;
		}
	}
}
