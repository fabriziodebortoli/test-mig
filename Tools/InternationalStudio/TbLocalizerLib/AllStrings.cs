using System.IO;
using System.Reflection;
using System;

namespace Microarea.Tools.TBLocalizer
{

	//=========================================================================
	/// <summary>
	/// All strings used in TBLocalizer that do not need to be localized
	/// </summary>
	public  class AllStrings                                                                                                                                                                                                                                                                                            
	{
		//per scrivere i file xml si usa tutto minuscolo.
		//le stringhe capitalizzate leggono il .vcproj o altri file xml esistenti
		//il file tblocalizer.tbl si trova nella initdirectory
		//il fie ResourceIndex.xml si trova dentro la cartella della \\Dictionary\lingua
		//initdirectory è dove vengono salvate le soluzioni


		public const string	applicationsCap	= "Applications";	//tag
		public const string	applicationCap	= "Application";	//tag
		public const string	assemblyNameCap	= "AssemblyName";	//tag
		public const string	assemblyTag		= "assembly";		//tag
		public const string	args			= "args";		//attribute

		public const string	baseTag			= "base";			//tag
		public const string	binExtension	= ".bin";			//estensione
		public const string	bold			= "bold";			//tag
		public const string	buildCap		= "Build";			//tag

		public const string	cExtension		= ".c";				//estensione
		public const string	codeCap			= "Code";		//attribute value di csproj
		public const string	configExtension	= ".config";		//estensione
		public const string	configuration	= "configuration";	//tag
		public const string	configurationCap= "Configuration";	//tag
        public const string contentClass    = "content";
		public const string	cppExtension	= ".cpp";			//estensione
		public const string	creation		= "creation";		//testo
		public const string	crystaldecisions= "crystaldecisions"; //namespace non nostro
		public const string	csExtension		= ".cs";			//estensione
		public const string	csProjExtension	= ".csproj";		//estensione
        public const string csvExtension    = ".csv";			//estensione
        public const string cultureCap      = "Culture";		//
		public const string	current			= "current";		//tag
		public const string	custom			= "custom";			//tag
		
		public const string	data			= "data";			//tag
		public const string	database		= "databasescript";	//folder
		public const string	dateFormat		= "yyyyMMddHHmmss";	//FormatPattern
		public const string	datetime		= "datetime";		//tag
        public const string dbinfo          = "dbinfo";         //tag
		public const string	default_root	= "Solution";		//testo 
		public const string	dependentUpon	= "DependentUpon";	//tag
		public const string	dialog			= "dialog";			//tag + folder
		public const string	dialogs			= "dialogs";		//tag
		public const string	dictionary	    = "dictionary";		//folder
		public const string	dictionaryCap	= "Dictionary";		//folder
		public const string	dictionaryFile	= "dictionaryFile";	//tag
		
		public const string	encoding		= "utf-8";			//tag
		public const string	enums			= "enums";			//tag
		public const string	formats			= "formats";		//tag
		public const string	format			= "format";			//tag
		public const string	fonts			= "fonts";			//tag
		public const string	font			= "font";			//tag
		public const string	enumTag			= "enum";			//tag + folder
		public const string extension		= "extension";		//tag
		public const string externalGlossaries = "externalglossaries";		//tag
		public const string error			= "Error";			//tag

		public const string	falseTag		= "false";			//attribute value
		public const string	fileCap			= "File";			//tag
		public const string	file			= "file";			//tag
		public const string	fileLink		= "\"file:\\\\\\{0}\"";		//testo
		public const string	filesCap		= "Files";			//tag module.config ex Libraries
		public const string	folder			= "folder";			//tag
		public const string	fontDefault		= "Verdana";		//testo
		public const string	fontname		= "fontname";		//tag
		public const string	fontsize		= "fontsize";		//tag
		//public const string	formCap			= "Form";			//attributeValue di csproj
		public const string	forms			= "forms";			//folder

		public const string	glossary		= "glossary";		//tag
		public const string	glossaries		= "glossaries";		//tag

		public const string	hash			= "hash";			//tag
		public const string	hExtension		= ".h";				//estensione
		public const string	hrcExtension	= ".hrc";			//estensione
		public const string	htmExtension	= ".htm";			//estensione
		public const string	htmlExtension	= ".html";			//estensione
		public const string	htmsTag			= "htms";			//tag
		public const string	htmTag			= "htm";			//tag
		
		public const string	id				= "id";				//tag
		public const string	italic			= "italic";			//tag
		public const string	item			= "Item";			//tag
		public const string	include			= "Include";		//tag
		public const string	iniExtension	= ".ini";			//estensione

        public const string jsonforms = "jsonforms";			//tag
		
		public const string	language		= "language";		//tag
		public const string	localizable		= "localizable";	//tag
		public const string	localize		= "localize";		//tag

		public const string	matchType		= "matchtype";		//tag
		public const string	menu			= "menu";			//tag + folder
		public const string	menuExtension	= ".menu";			//estensione
		public const string	menus			= "menus";			//tag
		public const string	message			= "message";		//tag
		public const string	messages		= "messages";		//tag
		public const string	microsoft		= "microsoft";		//namespace name
		public const string	mimetype		= "mimetype";		//attributo
		public const string	module			= "module";			//tag
		public const string	moduleconfig	= "module.config";	//file
		public const string	moduleCap		= "Module";			//tag
		
		public const string	name			= "name";			//tag
		public const string	nameCap			= "Name";			//tag
		public const string	namespaceTag	= "namespace";		//tag
		public const string	nodeFunction	= "node()";			//funzione XPath

		public const string	occurrenciesTag	= "occurrencies";	//tag 
		public const string	oldStrings		= "oldstrings";		//tag
		public const string oldNode			= "old";			//tag
		public const string	other			= "other";			//tag + folder
		public const string	outputFileCap	= "OutputFile";		//tag
		public const string outputPaths		= "outputPaths";	//tag

		public const string	path			= "path";			//tag
		public const string	paths			= "paths";			//tag
		public const string	project			= "project";		//tag
		public const string projectReference = "ProjectReference";		//tag
		public const string	projects		= "projects";		//tag
		public const string	propertyText			= ".Text";			//property name
		public const string	propertyToolTipText		= ".ToolTip";		//property name
		public const string	propertyTitleText		= ".Title";			//property name
		public const string	propertyToolTip			= ".ToolTipText";	//property name
		public const string	propertyCaptionText		= ".CaptionText";	//property name
		public const string	propertyHeaderText		= ".HeaderText";	//property name
		public const string	propertyItems			= ".Items";			//property name
		public const string	propertyNullText		= ".NullText";		//property name
		public const string	prjExtension	= ".tblprj";		//estensione

		public const string	rcExtension		= ".rc";			//estensione
		public const string	referenceCap	= "Reference";		//tag
		public const string	reference		= "reference";		//tag
		public const string	referencesCap	= "References";		//tag
		public const string	references		= "references";		//tag
        public const string refIdClass      = "refId";
		public const string	regeneration	= "regeneration";	//testo
		public const string	relativePathCap	= "RelativePath";	//tag
		public const string	relPathCap		= "RelPath";		//tag
		public const string	report			= "report";			//tag + folder
		public const string	reportIdentifier  = "layout";			//tag + folder
		public const string	reportLocalizable = "localizableStrings";//tag + folder
		public const string	reports			= "reports";		//tag
		public const string	resource		= "resource";		//tag
		public const string	resources		= "resources";		//tag
		public const string	resourceCap		= "Resource";		//tag
		public const string	resourceIndexTag= "resourceindex";		//tag
		public const string	resourceIndex	= "ResourceIndex.xml";//file
		public const string	resxExtension	= ".resx";			//estensione
		public const string	resxsTag		= "resxs";			//tag
		public const string	resxTag			= "resx";			//tag + folder
		public const string	rootNamespaceCap= "RootNamespace";	//tag

		public const string	schema			= "xs:schema";		//tag
		public const string	settingsCap		= "Settings";		//tag
		public const string	slnExtension	= ".tblsln";		//estensione
		public const string	solution		= "solution";		//tag
		public const string	solutions		= "solutions";		//tag
		public const string	source			= "source";			//tag + folder
		public const string	sources			= "sources";		//tag + folder
		public const string	sourcexml		= "sources.xml";	//file
		public const string	strings			= "strings";		//tag
		public const string	stringtable		= "stringtable";	//tag
		public const string	stringtables	= "stringtables";	//tag
		public const string	stringTag		= "string";			//tag(e di conseguenza nome table)
		public const string	sqlExtension	= ".sql";			//estensione
		public const string	subTypeCap		= "SubType";		//Attribute csproj
		public const string	sqlScripts		= "scripts";		//tag
		public const string	sqlScript		= "script";			//tag
		public const string	support			= "support";		//tag
		public const string	supportTemporary= "suptemp";		//tag
		public const string	system			= "system";			//namespace name
		
		public const string	tag				= "Tag";			//tag
		public const string	target			= "target";			//tag
		public const string	temporary		= "temporary";		//tag
		public const string	temporaryId		= "temporaryId";	//tag
		public const string	text			= "text";			//tag
		public const string	translatedTag	= "translated";     //tag 
        public const string tsExtension     = ".ts";           //estensione
        public const string	tools			= "tools";			//tag
		public const string	tool			= "tool";			//tag 
		public const string	trueTag			= "true";			//attribute value
		public const string	type			= "type";			//tag

		public const string	updating		= "updating";		//testo log
		public const string	url				= "url";			//tag
		
		public const string	valid			= "valid";			//tag
		public const string	valueTag		= "value";			//tag
		public const string	vcExtension		= ".vcproj";		//estensione
		public const string vcxExtension	= ".vcxproj";		//estensione
		public const string	version			= "1.0";	
		public const string versionTag		= "version";		//tag
		public const string frameworkVersionTag = "frameworkVersion";       //tag


        public const string web             = "web";            //tag
        public const string	width			= "width";			//tag
        public const string	wrmExtension	= ".wrm";			//estensione
		public const string	wordTag			= "Word";			
		public const string	wordsTag		= "Words";			

		public const string	x				= "x";				//tag
		public const string	xml				= "xml";			//tag + folder
		public const string	xsl				= "xsl";			//tag 
		public const string	xmlExtension	= ".xml";			//estensione
		public const string	xslExtension	= ".xsl";			//estensione
		public const string	xsltExtension	= ".xslt";			//estensione
		public const string	zipExtension	= ".zip";			//estensione
		public const string	xmls			= "xmls";			//tag

		public const string	y				= "y";				//tag
		
		public const string	beginSkipParse	= "BEGIN_TBLOCALIZER_SKIP";
		public const string	endSkipParse	= "END_TBLOCALIZER_SKIP";
		public const string	stringToken1	= "_TB";
		public const string	stringToken2	= "TB_LOCALIZED";
        public const string stringToken3    = "_TB_STRING";//usata per le stringhe ex RC

		public const string	ZippedDictionaryStandard	= "_Dictionary";
		public const string	ZippedDictionaryServices	= "Services_Dictionary";
		public const string	ServicesFolder				= "Services";


		public const char	separator		= '|'; 
		public static string MainAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		public static string FILTERXML			= Strings.XmlFiles + "|*.xml";
		public static string FILTERZIP			= Strings.ZipFiles + "|*.zip";
		public static string FILTERHTM			= Strings.HtmFiles + "|*.htm";
		public static string FILTERALL			= Strings.AllFiles + "|*.*";
		public static string FILTERTXT			= Strings.TxtFiles + "|*.txt";
		public static string FILTERXMLANDALL	= AllStrings.FILTERXML + "|" + AllStrings.FILTERALL;
        public static string FILTERCSV          = Strings.CsvFiles + "|*.csv";
		
		public static string FILTERSLN			= string.Format(Strings.SolutionFiles, slnExtension, slnExtension);
		public static string FILTERPRJ = Strings.ProjectFiles + " (*.tblprj; *.csproj; *.vcproj; *.vcxproj; package.json) | *.tblprj; *.csproj; *.vcproj; *.vcxproj; package.json";

		public static string GLOSSARYNAME		= "glossary.{0}.xml";
        public static string GLOSSARYDEFAULTFOLDER = AppDataPath + @"\Glossary";
        public static string GLOSSARYBACKUP = AppDataPath + @"\Glossary\{0}.{1}.xml";
        public static string INI = AppDataPath + @"\TBLocalizer.tbl";
        public static string LOGPATH = AppDataPath + @"\Logging";
        public static string LOG = LOGPATH + @"\log{0}.xml";
        public static string XSLPATH = MainAssemblyPath + @"\LogManager.xsl";
		public static string WORDLOG			= LOGPATH		+ @"\words.xml";
        public static string HELP = AppDataPath + @"\Help\Index.htm";
		public static string TRANSLATIONTABLE   = "TranslationTable_{0}({1}){2}";
        public static string TOBETRANSLATEDTABLE = "ToBeTranslated_{0}({1}){2}";
        public static string GLOSSARYTABLE = "Glossary_{0}({1}){2}";
        public static string FILELIST = "FileList_{0}.txt";
		public static string NotValidRow		= "?";
		public static string TemporaryRow		= "T";
		public static string LooseMatchRow		= "!!";
		public static string NotTranslatedRow	= "*";

        public const string XslFileName = "LogManager.xsl";
	}

}
