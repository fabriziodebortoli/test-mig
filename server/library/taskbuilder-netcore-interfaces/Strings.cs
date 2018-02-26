
namespace TaskBuilderNetCore.Interfaces
{
    

    // Definizioni di ELEMENT e ATTRIBUTE dei files *.xml di ReferenceObjects
    //=========================================================================
    public sealed class ReferenceObjectsXML
    {
        //---------------------------------------------------------------------
        private ReferenceObjectsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string HotKeyLink = "HotKeyLink";
            public const string Function = "Function";
            public const string DbField = "DbField";
            public const string ControlData = "ControlData";
            public const string Param = "Param";
            public const string Query = "Query";
            public const string ComboBox = "ComboBox";
            public const string Column = "Column";
            public const string DbTable = "DbTable";
            public const string DbFieldDescription = "DbFieldDescription";
            public const string RadarReport = "RadarReport";
            public const string ClassName = "ClassName";

            public const string Auxdata = "Auxdata";
            public const string Header = "Header";
            public const string Fieldtype = "Fieldtype";
            public const string Elements = "Elements";
            public const string Elem = "Element";
            public const string Field = "Field";

        }

        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
            public const string Value = "value";
            public const string Type = "type";
            public const string Mode = "mode";
            public const string Server = "server";
            public const string Port = "port";
            public const string Service = "service";
            public const string ServiceNamespace = "serviceNamespace";
            public const string Localize = "localize";
            public const string BaseType = "basetype";
            public const string Namespace = "namespace";
            public const string Source = "source";
            public const string Length = "length";
            public const string When = "when";
            public const string Formatter = "formatter";
            public const string Datafile = "datafile";
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files WebMethods.xml       
    //=========================================================================
    public sealed class OutDateObjectsXML
    {
        //---------------------------------------------------------------------
        private OutDateObjectsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string OutDateObjects = "OutDateObjects";
            public const string Reports = "Reports";
            public const string Report = "Report";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Namespace = "namespace";
            public const string Release = "release";
            public const string Operator = "operator";
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files WebMethods.xml       
    //=========================================================================
    public sealed class WebMethodsXML
    {
        //-----------------------------------------------------------------
        private WebMethodsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string Param = "Param";
            public const string FunctionObjects = "FunctionObjects";
            public const string Functions = "Functions";
            public const string Function = "Function";
            public const string Events = "Events";
            public const string Event = "Event";
            public const string Arguments = "Arguments";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
            public const string Type = "type";
            public const string Mode = "mode";
            public const string Localize = "localize";
            public const string Namespace = "namespace";
            public const string BaseType = "basetype";
            public const string ClassType = "classType";
            public const string SourceInfo = "sourceInfo";
            public const string Value = "value";
            public const string Optional = "optional";
            public const string Row = "row";

            public const string Server = "server";
            public const string Port = "port";
            public const string Service = "service";
            public const string ServiceNamespace = "serviceNamespace";

            public const string Report = "report";
            public const string DefaultSecurityRoles = "defaultsecurityroles";
            public const string Securityhidden = "securityhidden";
            public const string InEasyStudio = "inEasyStudio";
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files DatabaseObjects.xml       
    //=========================================================================
    public sealed class DataBaseObjectsXML
    {
        //-----------------------------------------------------------------
        private DataBaseObjectsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string DatabaseObjects		= "DatabaseObjects";
            public const string Signature			= "Signature";
			public const string PreviousSignature	= "PreviousSignature";
			public const string Release				= "Release";
            public const string Tables				= "Tables";
            public const string Table				= "Table";
            public const string Views				= "Views";
            public const string View				= "View";
            public const string Procedures			= "Procedures";
            public const string Procedure			= "Procedure";
            public const string Create				= "Create";
        }

        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name		= "name";
            public const string Namespace	= "namespace";
            public const string Release		= "release";
            public const string Createstep	= "createstep";
            public const string Dms			= "dms";
            public const string Mastertable = "mastertable";
			public const string Application = "application";
			public const string Module		= "module";
		}
	}

    // Definizioni di ELEMENT e ATTRIBUTE dei files di descrizione oggetti di database
    //=========================================================================
    public sealed class DBObjectXML
    {
        //-----------------------------------------------------------------
        private DBObjectXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string RootElement = "DBObjects";

            public const string Tables = "Tables";
            public const string Table = "Table";
            public const string Views = "Views";
            public const string View = "View";
            public const string Procedures = "Procedures";
            public const string Procedure = "Procedure";

            public const string ExtraAddedColumns = "ExtraAddedColumns";
            public const string ExtraAddedColumn = "ExtraAddedColumn";

            public const string Columns = "Columns";
            public const string Column = "Column";
            public const string Parameters = "Parameters";
            public const string Parameter = "Parameter";
        }

        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
            public const string TableName = "table_name";
            public const string TbNamespace = "tb_namespace";
            public const string DbReleaseNumber = "db_release_number";
            public const string LibraryNamespace = "library_namespace";
            public const string CreateStep = "createstep";

            public const string Localize = "localize";
            public const string DataType = "data_type";
            public const string DataLength = "data_length";
            public const string OutParam = "out_param";
            public const string BaseType = "basetype";
        }
    }

    //=========================================================================
    public sealed class SettingsConfigXML
    {
        //-----------------------------------------------------------------
        private SettingsConfigXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string Section = "Section";
            public const string Setting = "Setting";
            public const string ValueTag = "Value";
            public const string ParameterSettings = "ParameterSettings";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Localize = "localize";
            public const string Value = "value";
            public const string Name = "name";
            public const string Release = "release";
            public const string Type = "type";
            public const string BaseType = "basetype";
            public const string Hidden = "hidden";
            public const string AllowNewSettings = "allowNewSettings";
            public const string UserSetting = "userSetting";
        }
    }

    //=========================================================================
    public sealed class ApplicationConfigXML
    {
        //-----------------------------------------------------------------
        private ApplicationConfigXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string ApplicationInfo = "ApplicationInfo";
            public const string Icon = "Icon";
            public const string Type = "Type";
            public const string WelcomeBmp = "WelcomeBmp";
            public const string DbSignature = "DbSignature";
            public const string Version = "Version";
            public const string Uuid = "Uuid";
            public const string Visible = "Visible";
            public const string HelpModule = "HelpModule";
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files  module.config
    //=========================================================================
    public sealed class ModuleConfigXML
    {
        //-----------------------------------------------------------------
        private ModuleConfigXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string FilesExtension = "FilesExtension";
            public const string Components = "Components";
            public const string Library = "Library";
            public const string ModuleInfo = "ModuleInfo";
            public const string DbObjectsInfo = "DbObjectsInfo";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string DestinationFolder = "destinationfolder";
            public const string Localize = "localize";
            public const string Name = "name";
            public const string AggregateName = "aggregatename";
            public const string Value = "value";
            public const string Optional = "optional";
            public const string MenuViewOrder = "menuvieworder";
            public const string Signature = "signature";
            public const string Release = "release";
        }
    }

    //=========================================================================
    public sealed class ServerConnectionInfoXML
    {
        //-----------------------------------------------------------------
        private ServerConnectionInfoXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }
            public static string WmsCalPurgeable = "WmsCalPurgeable";
            public static string Connection = "Connection";
            public static string SysDBConnectionString = "SysDBConnectionString";
            public static string PreferredLanguage = "PreferredLanguage";
            public static string ApplicationLanguage = "ApplicationLanguage";
            public static string MasterSolutionName = "MasterSolutionName";
            public static string PasswordDuration = "PasswordDuration";
            public static string MinPasswordLength = "MinPasswordLength";
            public static string WebServicesPort = "WebServicesPort";
            public static string WebServicesTimeOut = "WebServicesTimeOut";
            public static string MaxTBLoaderForHotLink = "MaxTBLoaderForHotLink";
            public static string MaxTBLoader = "MaxTBLoader";
            public static string MaxLoginPerTBLoader = "MaxLoginPerTBLoader";
            public static string MaxLoginFailed = "MaxLoginFailed";
            public static string UseStrongPwd = "UseStrongPwd";
            public static string UseAutologin = "UseAutologin";
            public static string MinDBSizeToWarn = "MinDBSizeToWarn";
            public static string SMTPServer = "SMTPServer";
            public static string SMTPUseDefaultCredentials = "SMTPUseDefaultCredentials";
            public static string SMTPUseSSL = "SMTPUseSSL";
            public static string SMTPPort = "SMTPPort";
            public static string SMTPUserName = "SMTPUserName";
            public static string SMTPPassword = "SMTPPassword";
            public static string SMTPDomain = "SMTPDomain";
            public static string SMTPFromAddress = "SMTPFromAddress";
            public static string TempBuild = "TempBuild";
            public static string ForceApplicationDate = "ForceApplicationDate";
            public static string TBLoaderTimeOut = "TBLoaderTimeOut";
            public static string TbWCFDefaultTimeout = "TbWCFDefaultTimeout";
            public static string TbWCFDataTransferTimeout = "TbWCFDataTransferTimeout";
            public static string EnableVerboseLog = "EnableVerboseLog";
            public static string PingMailRecipient = "PingMailRecipient";
            public static string SendPingMail = "SendPingMail";
            public static string CheckVATNr = "CheckVATNr";

        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Value = "value";
            public const string Localize = "localize";
            public const string LMAttr = "loginmanager";
            public const string EAAttr = "easyattachmentsync";
        }
    }

    //=========================================================================
    public sealed class UrlHelpInfoXML
    {
        //-----------------------------------------------------------------
        private UrlHelpInfoXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public static string HelpConfiguration = "HelpConfiguration";
            public static string LanguageInstalled = "LanguageInstalled";
            public static string WebSiteOnLine = "WebSiteOnLine";
            public static string WebSiteOffLine = "WebSiteOffLine";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Value = "value";
            public const string Localize = "localize";
            public const string Name = "name";
        }

    }

    //=========================================================================
    public sealed class ClientConfigurationXML
    {
        //-----------------------------------------------------------------
        private ClientConfigurationXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string applications = "Applications";
            public const string module = "Module";
            public const string application = "Application";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
        }
    }

    //=========================================================================
    public sealed class DocumentsObjectsXML
    {
        //-----------------------------------------------------------------
        private DocumentsObjectsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string DocumentObjects = "DocumentObjects";
            public const string Documents = "Documents";
            public const string Document = "Document";
            public const string Title = "Title";
            public const string Description = "Description";
            public const string ClientDocuments = "ClientDocuments";
            public const string ServerDocument = "ServerDocument";
            public const string ViewModes = "ViewModes";
            public const string Mode = "Mode";
            public const string ClientDocument = "ClientDocument";
            public const string Components = "Components";
            public const string Component = "Component";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Namespace = "namespace";
            public const string Localize = "localize";
            public const string Name = "name";
            public const string Type = "type";
            public const string DocumentClass = "class";
            public const string Classhierarchy = "classhierarchy";
            public const string Schedulable = "schedulable";
            public const string DefaultSecurityRoles = "defaultsecurityroles";
            public const string Securityhidden = "securityhidden";
            public const string TransferDisabled = "transferdisabled";
            public const string Dynamic = "dynamic";
            public const string Designable = "designable";
            public const string Activation = "activation";
            public const string Published = "published";
            public const string RunnableAlone = "runnableAlone";
            public const string AllowISO = "allowISO";
            public const string DenyISO = "denyISO";
            public const string MainObjectNamespace = "mainObjectNamespace";
        }
    }

    //=========================================================================
    public sealed class AddOnDatabaseObjectsXML
    {
        //-----------------------------------------------------------------
        private AddOnDatabaseObjectsXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string AddOnDatabaseObjects = "AddOnDatabaseObjects";
            public const string AdditionalColumns = "AdditionalColumns";
            public const string Table = "Table";
            public const string AlterTable = "AlterTable";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Release = "release";
            public const string Createstep = "createstep";
            public const string NameSpace = "namespace";
			public const string Virtual = "virtual";
		}
    }

    //=========================================================================
    public sealed class DependenciesMapXML
    {
        //-----------------------------------------------------------------
        private DependenciesMapXML()
        { }

        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string AggregateName = "AggregateName";
            public const string Aggregations = "Aggregations";
            public const string DependenciesMap = "DependenciesMap";
            public const string Dependency = "Dependency";
            public const string Library = "Library";
        }
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Application = "application";
            public const string BuildDate = "builddate";
            public const string Module = "module";
            public const string Modules = "modules";
            public const string Name = "name";
            public const string Policy = "policy";
        }
    }

    //=========================================================================
    public sealed class ReportsXML
    {
        //-----------------------------------------------------------------
        private ReportsXML()
        { }
        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }
            public const string ReportObjects = "ReportObjects";
            public const string Reports = "Reports";
            public const string Report = "Report";
            public const string DefaultReport = "DefaultReport";
        }

        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }
            public const string DefaultReport = "defaultReport";
            public const string NameSpace = "namespace";
            public const string DenyISO = "denyISO";
            public const string AllowISO = "allowISO";
        }
    }

    //=========================================================================
    public sealed class ExternalReferencesXML
    {
        private ExternalReferencesXML()
        { }

        //=====================================================================
        public sealed class Element
        {
            public const string MainExternalReferences = "MainExternalReferences";
            public const string DBT = "DBT";
            public const string ExternalReference = "ExternalReference";

            private Element()
            { }
        }

        //=====================================================================
        public sealed class Attribute
        {
            public const string NameSpace = "namespace";

            private Attribute()
            { }
        }
    }

    //=========================================================================
    public sealed class DBTSXML
    {
        private DBTSXML()
        { }

        //=====================================================================
        public sealed class Element
        {
            public const string DBTs = "DBTs";
            public const string Master = "Master";
            public const string Table = "Table";
            public const string Slaves = "Slaves";
            public const string SlaveBuffered = "SlaveBuffered";
            public const string Slave = "Slave";
            public const string UniversalKeys = "UniversalKeys";
            public const string UniversalKey = "UniversalKey";
            public const string Segment = "Segment";
            public const string FixedKeys = "FixedKeys";

            private Element()
            { }
        }

        //=====================================================================
        public sealed class Attribute
        {
            public const string NameSpace = "namespace";
            public const string Name = "name";

            private Attribute()
            { }
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files RowSecurityObjects.xml       
    //=========================================================================
    public sealed class RowSecurityObjectsXML
    {
        //-----------------------------------------------------------------
        private RowSecurityObjectsXML()
        { }

        //-----------------------------------------------------------------
        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string RowSecurityObjects = "RowSecurityObjects";
            public const string Entities = "Entities";
            public const string Entity = "Entity";
            public const string RSColumns = "RSColumns";
            public const string RSColumn = "RSColumn";
            public const string Tables = "Tables";
            public const string Table = "Table";
            public const string MasterTableNamespace = "MasterTableNamespace";
            public const string DocumentNamespace = "DocumentNamespace";
            public const string HKLNamespace = "HKLNamespace";
            public const string NumbererNamespace = "NumbererNamespace";
            public const string Description = "Description";
            public const string Priority = "Priority";
        }

        //-----------------------------------------------------------------
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
            public const string Namespace = "namespace";
            public const string EntityColumn = "entitycolumn";
        }
    }

    // Definizioni di ELEMENT e ATTRIBUTE dei files BehaviourObjects.xml       
    //=========================================================================
    public sealed class BehaviourObjectsXML
    {
        //-----------------------------------------------------------------
        private BehaviourObjectsXML()
        { }

        //-----------------------------------------------------------------
        public sealed class Element
        {
            //-----------------------------------------------------------------
            private Element()
            { }

            public const string BehaviourObjects = "BehaviourObjects";
            public const string Entities = "Entities";
            public const string Entity = "Entity";
            public const string Services = "Services";
            public const string Service = "Service";
        }

        //-----------------------------------------------------------------
        public sealed class Attribute
        {
            //-----------------------------------------------------------------
            private Attribute()
            { }

            public const string Name = "name";
            public const string Namespace = "namespace";
            public const string Localize = "localize";
            public const string Service = "service";
        }
    }

    //=========================================================================
    public sealed class NameSpaceSegment
    {
        //-----------------------------------------------------------------
        private NameSpaceSegment()
        { }
        public const string Application = "Application";
        public const string Module = "Module";
        public const string Report = "Report";
        public const string NotValid = "NotValid";
        public const string Library = "Library";
        public const string Image = "Image";
        public const string Text = "Text";
        public const string File = "File";
        public const string DataFile = "DataFile";
        public const string View = "View";
        public const string Document = "Document";
        public const string Hotlink = "Hotlink";
        public const string HotKeyLink = "HotKeyLink";
        public const string Table = "Table";
        public const string Procedure = "Procedure";
        public const string Function = "Function";
        public const string ExcelDocument = "ExcelDocument";
        public const string ExcelTemplate = "ExcelTemplate";
        public const string WordDocument = "WordDocument";
        public const string WordTemplate = "WordTemplate";
        public const string ExcelDocument2007 = "ExcelDocument2007";
        public const string ExcelTemplate2007 = "ExcelTemplate2007";
        public const string WordDocument2007 = "WordDocument2007";
        public const string WordTemplate2007 = "WordTemplate2007";
        public const string ReportSchema = "ReportSchema";
        public const string DocumentSchema = "DocumentSchema";
        public const string Dbt = "Dbt";
        public const string Setting = "Setting";
        public const string Control = "Control";
        public const string Form = "Form";
        public const string Customization = "Customization";
        public const string Documents = "Documents";
        public const string ToolbarButton = "ToolbarButton";
        public const string Toolbar = "Toolbar";
        public const string Component = "Component";
    }

    //=========================================================================
    public sealed class NameSolverXmlStrings
    {
        //-----------------------------------------------------------------
        private NameSolverXmlStrings()
        { }
        public const string Default = "DEFAULT";

        public const string DataEntry = "dataentry";
        public const string Finder = "finder";
        public const string Batch = "batch";
        public const string Silent = "silent";
        public const string Module = "module";
        public const string Release = "Release";

        public static string PreferredLanguage = "PreferredLanguage";
        public static string UserUpdates = "UserUpdates";
        public static string User = "User";
        public static string Data = "data";

        // XML Attributes
        public const string Container = "container";
    }

    //=========================================================================
    public sealed class NameSolverDatabaseStrings
    {
        //-----------------------------------------------------------------
        private NameSolverDatabaseStrings()
        { }
        public const string SQLOLEDBProvider = "SQLOLEDB";
        public const string OraOLEDBProvider = "OraOLEDB.Oracle";
        public const string MSDAORAProvider = "MSDAORA";
        public const string SQLODBCProvider = "SQL Server Native Client 10.0";
        public const string ODBCProvider = "MSDASQL"; // generic ODBC provider

        public const string PostgreOdbcProvider = "PostgreSQL ODBC Driver(UNICODE)"; // Postgre provider

        public const string ProviderConnAttribute = "Provider={0}; ";
        public const string DriverConnAttribute = "Driver= {{{0}}}; ";

        public const string SQLWinNtConnection = "Data Source={0};Initial Catalog='{1}';Integrated Security='SSPI';Connect Timeout=30;Pooling=false;";
        public const string SQLConnection = "Data Source={0};Initial Catalog='{1}';User ID='{2}';Password='{3}';Connect Timeout=30;Pooling=false;";
        public const string SQLAzureConnection = "Server=tcp:{0};Database='{1}';User ID='{2}';Password='{3}';Connect Timeout=30;";

        public const string SQLWinNtConnectionRedux = "Data Source={0};Initial Catalog='{1}';Integrated Security='SSPI';";
        public const string SQLConnectionRedux = "Data Source={0};Initial Catalog='{1}';User ID='{2}';Password='{3}';";

        public const string OracleWinNtConnection = "Data Source={0};Integrated Security=yes;";
        public const string OracleConnection = "Data Source={0};User ID='{1}';Password='{2}';";
        public const string OracleWinNtConnectionWithProvider = "Provider={0}; Data Source={1}; OSAuthent=1";

        public const string PostgreWinNtConnection = "Server={0};Port={1} ;Database={2};SearchPath={3};Integrated Security=true;Pooling=False"; // SearchPath da cambiare probabilmente
        public const string PostgreConnection = "Server={0};Port={1};Database={2};User Id={3};Password={4};SearchPath={5};Pooling=False";

        public const string MySqlConnection = "server={0};uid={1};pwd={2};database={3}";

        public const string ODBCConnectionString = "Server={0};Database={1}; Uid={2}; Pwd={3};";
        public const string ODBCWinAuthConnectionString = "Server={0}; Database={1}; Trusted_Connection=yes;";

        public const string ODBCConnectionStringForPostgre = "Server={0};Port={1};Database={2};A6=set search_path to {3}; Uid={4};Pwd={5};";
        public const string ReleaseNumDirectory = "Release_";

        public const string SQLLatinCollation = "Latin1_General_CI_AS";
    }

    //=========================================================================
    public sealed class NameSolverStrings
    {
        //-----------------------------------------------------------------
        private NameSolverStrings()
        { }
        public const char Directoryseparetor = '\\';
        // TODO - devono essere tutte split-tate per tipologia e divenire const.
        public const string RunWithoutMenu = "RunWithoutMenu";
        public const string AuthenticationToken = "AuthenticationToken";
        public const string Release = "Release";
        public const string Text = "Text";
        public const string Companies = "Companies";
        public const string AllCompanies = "AllCompanies";
        public const string ServerConnection = "ServerConnection";
        public const string ClientConfiguration = "ClientConfiguration";
        public const string LockLogFile = "LockLog.xml";
        public const string Users = "Users";
        public const string Declarations = "Declarations";

        public const string Microarea = "Microarea";

        public const string Services = "Services";
        public const string EasyLookSystemLogin = "EasyLookSystem";
        public const string EasyLookSystemPwd = "EasyLookSystem1.1";
        public const string GuestLogin = "Anonymous";
        public const string GuestPwd = "Anonymous1.1";

        public const string MasterProductNameKey = "MasterProductName";

        public const string LoginMngSessionFile = "LoginMngSession.cfg";
        public const string MessagesQueueFile = "MessagesQueue.bin";
        public const string DatabaseObjectsBinFile = "DatabaseObjects.bin";

        public const string FloatingMarkString = "FloatingMark";
        public const int FloatingMarkNumber = 9999;

        public const string NotificationService = "NotificationService";

        #region Edition
        public const string StandardEdition = "Standard";
        public const string ProfessionalEdition = "Professional";
        public const string EnterpriseEdition = "Enterprise";
        public const string StdEdition = "Std";
        public const string ProEdition = "Pro";
        public const string EntEdition = "Ent";
        #endregion

        #region IsoStateLanguage
        public const string IsoStateItalian = "IT";
        public const string IsoStateChinese = "CN";
        public const string IsoStateJapanese = "JP";
        public const string IsoStateHungarian = "HU";
        public const string IsoStatePolish = "PL";
        public const string IsoStateSerbian = "CS";
        public const string IsoStateBulgarian = "BG";
        public const string IsoStateTurkish = "TR";
        public const string IsoStateRomanian = "RO";
        public const string IsoStateSlovenian = "SI";
        public const string IsoStateCroatian = "HR";
        public const string IsoStateGreek = "GR";
        #endregion

        #region Constants
        public const string XmlDeclarationEncoding = "UTF-8";
        public const string XmlDeclarationVersion = "1.0";
        #endregion

        #region Nomi cartelle
        public const string AppData = "App_Data";
        public const string Running = "Running";
        public const string Description = "Description";
        public const string JsonForms = "JsonForms";
        public const string DownLoadImage = "DownLoadImage";
        public const string DownLoadCache = "DownLoadCache";
        public const string BeforeRunning = "BeforeRunning";
        public const string ServicesUpdates = "ServicesUpdates";
        public const string ServicesCache = "ServicesCache";
        public const string Standard = "Standard";
        public const string Custom = "Custom";
        public const string Solutions = "Solutions";
        public const string SolutionsUpdate = "SolutionsUpdate";
        public const string Modules = "Modules";
        public const string Activation = "Activation";
        public const string LogFiles = "LogFiles";
        public const string MigrationLog = "MigrationLog";
        public const string RegressionTestSettings = "RegressionTestSettings";
        public const string LocalState = "LocalState";
        public const string ErpProducts = "ERP Products";
        public const string LocalHelpFolder = "Helps";
        public const string Licenses = "Licenses";
        public const string WebUpdaterServices = "WebUpdater Services";
        public const string Extensions = "Extensions";
        public const string Themes = "Themes";
        public const string Thumbnails = "Thumbnails";
        public const string DesignerFolderName = "designer";

        #endregion

        #region File xml
        public const string GenericBrandFile = "Generic" + BrandExtension;
        public const string MainBrandFile = "Main" + BrandExtension;
        public const string ModuleObjectsXml = "ModuleObjects.xml";
        public const string DocumentObjectsXml = "DocumentObjects.xml";
        public const string DatabaseObjectsXml = "DatabaseObjects.xml";
        public const string RowSecurityObjectsXml = "RowSecurityObjects.xml";
        public const string BehaviourObjectsXml = "BehaviourObjects.xml";
        public const string EnumsXml = "Enums.xml";
        public const string EventHandlerObjectsXml = "EventHandlerObjects.xml";
        public const string AddOnDatabaseObjectsXml = "AddOnDatabaseObjects.xml";
        public const string FunctionObjectsXml = "FunctionObjects.xml";
        public const string OutDateObjectsXml = "OutDateObjects.xml";
        public const string WebMethodsXml = "WebMethods.xml";
        public const string WebUpdaterFile = "WebUpdater.xml";
        public const string MicroareaConsoleFile = "MicroareaConsole.xml";
        public const string ReportXml = "Reports.xml";
        public const string ClientDocumentObjectsXxml = "ClientDocumentObjects.xml";
        public const string ProducerActivationXml = "Microarea.Activation.xml";
        public const string DependenciesMapXml = "DependenciesMap.xml";
        public const string DbtsXml = "Dbts.xml";
        public const string DefaultsXml = "Defaults.xml";
        public const string DocumentXml = "Document.xml";
        public const string ExternalReferencesXml = "ExternalReferences.xml";
        public const string MigrationFile = "MigrationInfo.xml";
        public const string ImportSqlFile = "Professional.sql";
        public const string DevelopmentEnvironmentXml = "DevelopmentEnvironment.xml";
        public const string GlobalSettingsXml = "GlobalSettings.xml";
        public const string ProxiesXml = "Proxies.xml";
        public const string ActionsXml = "Actions.xml";
        public const string RadarsXml = "Radars.xml";
        public const string FieldXml = "Field.xml";
        public const string HotKeyLinkXml = "HotKeyLink.xml";
        public const string ComputedBrandsXml = "InstallationAbstract.xml";
        public const string FileSystemCacheXml = "FileSystemCache.xml";
        public const string LocalizableApplicationConfig = "LocalizableApplication.config";

        #endregion

        #region File ini e config
        public const string FormatsIniFile = "Formats.ini";
        public const string FontsIniFile = "Fonts.ini";
        public const string EnumsIniFile = "Enums.ini";
        public const string ShsFile = "Shs.config";
        #endregion

        #region Estensioni file
        public const string BrandExtension = ".Brand" + XmlExtension;
        public const string BrandSolutionExtension = "." + Solution + BrandExtension;
        public const string CompatibilityListExtension = ".CompatibilityList" + ConfigExtension;
        public const string DecryptedArticleExtension = XmlExtension;
        public const string LicensedExtension = "." + Licensed + ConfigExtension;
        public const string SolutionExtension = "." + Solution + XmlExtension;
        public const string ConfigExtension = ".config";
        public const string CsmExtension = ".csm";
        public const string DllExtension = ".dll";
        public const string PdbExtension = ".pdb";
        public const string BakExtension = ".bak";
        public const string ExcelDocumentExtension = ".xls";
        public const string ExcelTemplateExtension = ".xlt";
        public const string Excel2007DocumentExtension = ".xlsx";
        public const string Excel2007TemplateExtension = ".xltx";
        public const string ExeExtension = ".exe";
        public const string ManifestExeExtension = ".exe.manifest";
        public const string MenuExtension = ".menu";
        public const string RtpExtension = ".rtp";
        public const string SchemaExtension = ".xsd";
        public const string SplashExtension = ".Splash.*";
        public const string WordDocumentExtension = ".doc";
        public const string WordTemplateExtension = ".dot";
        public const string Word2007DocumentExtension = ".docx";
        public const string Word2007TemplateExtension = ".dotx";
        public const string WrmExtension = ".wrm";
        public const string WrmExtensionSearchCriteria = "*.wrm";
        public const string XmlExtension = ".xml";
        public const string XmlLogExtension = ".xlog";
        public const string SqlExtension = ".sql";
        public const string XamlExtension = ".xaml";
        public const string DBXmlExtension = ".dbxml";
        public const string JsonExtension = ".json";
        public const string TbjsonExtension = ".tbjson";
        public const string HjsonExtension = ".hjson";
        public const string CSharpExtension = ".cs";
        public const string CustomListFileExtension = ".CustomList.xml";
        public const string CustomListFileSearchCriteria = "*.CustomList.xml";
        public const string StandardListFileExtension = ".StandardList.xml";
        public const string StandardListFileSearchCriteria = "*.StandardList.xml";
        public const string StandardModuleWrapperListFileExtension = ".TBWrapperList.xml";
        public const string StandardModuleWrapperListFileSearchCriteria = "*.TBWrapperList.xml";
        public const string ManifestExtension = ".Manifest";
        public const string ManifestExtensionSearchCriteria = "*.Manifest";
        public const string CustomAppStateFile = "CustomAppState.bin";
        public const string DllSearchCriteria = "*.dll";
        public const string AllFilesSearchCriteria = "*.*";
        public const string EasyStudioDesigner = "EasyStudioDesigner";
        public const string EbsExtension = ".ebs";
        public const string CrsExtension = ".crs"; // CryptedRowSecurity
        public const string ThemeExtension = ".theme"; // theme
        public const string EbLinkExtension = ".ebl";


        public const string PdbExtensionSearchCriteria = "*" + PdbExtension;


        #endregion

        #region Nomi file
        public const string InstallationSettings = "InstallationSettings";
        public const string Licensed = "Licensed";
        public const string PostMigration = "PostMigration";
        public const string SchedulerState = "SchedulerState";
        public const string Service = "Service";
        public const string Solution = "Solution";
        public const string Storage = "Storage";
        public const string UpdateMachineState = "UpdateMachineState";
        public const string UserInfo = "UserInfo";
        public const string InstallationVersion = "Installation.ver";
        #endregion

        #region File
        public const string FileUserInfo = UserInfo + ConfigExtension;
        public const string FileStorage = Storage + ConfigExtension;
        public const string FileService = Service + ConfigExtension;
        public const string FileInstallationSettings = InstallationSettings + ConfigExtension;
        public const string FileStateScheduler = SchedulerState + XmlExtension;
        public const string FileStateMachine = UpdateMachineState + XmlExtension;
        public const string FileZipUpdates = "Update.zip";
        #endregion

        #region File masks
        public const string MaskFileStorage = "*." + NameSolverStrings.FileStorage;
        public const string MaskFileLicensed = "*" + LicensedExtension;
        public const string MaskFileEncryptedArticle = "*" + CsmExtension;
        public const string MaskFileDecryptedArticle = "*" + DecryptedArticleExtension;
        public const string MaskFileXml = "*" + XmlExtension;
        public const string MaskFileConfig = "*" + ConfigExtension;
        public const string MaskFileMenu = "*" + MenuExtension;
        public const string SourcesFolderNameMask = "{0}_src";
        #endregion

        #region ApplicationDBAdmin
        public const string UpgradeInfoXml = "UpgradeInfo.xml";
        public const string CreateInfoXml = "CreateInfo.xml";
        public const string DatabaseScript = "DatabaseScript";
        public const string Migration = "Migration";
        public const string CreateScript = "Create";
        public const string UpgradeScript = "Upgrade";
        public const string All = "all";
        public const string SqlServer = "SqlServer";
		public const string SQLAzure = "SQLAzure";
		public const string Oracle = "Oracle";
        public const string Postgre = "Postgre";
		#endregion

		#region Contenitori di applicazioni
		public const string TaskBuilder = "TaskBuilder";
        public const string TaskBuilderApplications = "Applications";
        public const string TbApplication = "TbApplication";
        public const string EasyStudio = "EasyStudio";
        public const string EasyStudioHome = "ESHome";
        public const string EasyStudioHomeWeb = "ESHome";

        #endregion

        #region Path
        public const string TbLoader = "TbLoader";
        public const string OcBin = "OcBin";
        public const string ClientNet = "ClientNet";
        public const string MenuManager = "MenuManager";
        public const string Setup = "Setup";
        public const string Framework = "Framework";
        public const string WebFramework = "WebFramework";
        public const string Data = "Data";
        public const string DataTransfer = "DataTransfer";
        public const string Log = "Log";
        public const string DataManager = "DataManager";
        public const string Default = "Default";
        public const string Sample = "Sample";
        public const string XTech = "XTech";
        public const string Bin = "Bin";
        public const string Apps = "Apps";
        public const string Publish = "Publish";
        public const string ClickOnceDeployer = "ClickOnceDeployer";
        public const string AllUsers = "AllUsers";
        public const string ModuleObjects = "ModuleObjects";
        public const string DBInfo = "DBInfo";
        public const string Report = "Report";
        public const string Help = "Help";
        public const string Image = "Image";
        public const string Files = "Files";
        public const string File = "File";
        public const string Images = "Images";
        public const string Texts = "Texts";
        public const string OfficeFiles = "OfficeFiles";
        public const string Excel = "Excel";
        public const string Word = "Word";
        public const string Schema = "Schema";
        public const string Others = "Others";
        public const string Settings = "Settings";
        public const string MicroareaServerManager = "MicroareaServerManager";
        public const string TbApps = "TbApps";
        public const string ReferenceObjects = "ReferenceObjects";
        public const string Start = "Start";
        public const string TestManager = "TestManager";
        public const string Temp = "Temp";
        public const string ExportProfiles = "ExportProfiles";
        public const string Backup = "Backup";
        public const string Templates = "Templates";

        public const string StandardDictionaryFile = "Dictionary.bin";

        // SynchroConnector folder and file
        public const string SynchroConnectorModule = "SynchroConnector";
        public const string SynchroProfilesXmlFile = "SynchroProfiles.xml";
        public const string SynchroMassiveProfilesXmlFile = "SynchroMassiveProfiles.xml";
        public const string SynchroProvidersXmlFolder = "SynchroProviders";
        public const string SynchroProfilesActionsXmlFolder = "Actions";

        //SOSConnector folder and file
        public const string SOSConnectorModule = "SOSConnector";
        public const string SOSConfigurationXmlFile = "SOSConfiguration.xml";

        #endregion


        #region Security
        public const string ReportEditorRole = "ReportEditor";
        #endregion

        //Tag Xml
        public const string Configuration = "Configuration";
        public const string Application = "Application";
        public const string Applications = "Applications";
        public const string Customization = "Customization";
        public const string CustomizationsLog = "CustomizationsLog";
        public const string Module = "Module";
        public const string Favorites = "Favorites";
        public const string Section = "Section";
        public const string BuildLocations = "BuildLocations";
        public const string Build = "Build";
        public const string ServerLocations = "ServerLocations";
        public const string Menu = "Menu";
        public const string Dictionary = "Dictionary";
        public const string Base = "base";
        public const string Debug = "debug";
        public const string LockManager = "LockManager";
        public const string LoginManager = "LoginManager";
        public const string RESTGate = "RESTGate";
        public const string TbLoaderLauncher = "TbLoaderLauncher";
        public const string TbServices = "TbServices";
        public const string TbSender = "TbSender";
        public const string PLProxy = "PLProxy";
        public const string TbHermes = "TbHermes";
        public const string TbHermesModule = "Mail";
        public const string SOSProxy = "SOSProxy";
        public const string EasyLook = "EasyLook";
        public const string EasyLookService = "EasyLookService";
        public const string TbConfiguration = "Configuration";
        public const string TbDontDownLoad = "DontDownLoad";
        public const string TbUser = "User";
        public const string TbPassword = "Password";
        public const string Security = "Security";
        public const string Company = "Company";
        public const string TbStart = "TbStart";
        public const string TbLanguage = "Language";
        public const string NoResize = "NoResize";
        public const string DefaultLanguage = "en";
        public const string DefaultInstallationPrefix = "My";
        public const string DefaultInstallationName = "MyERP";
        public const string TBPort = "TBPort";
        public const string ClearCache = "ClearCache";
        public const string DebugSymbols = "DebugSymbols";
        public const string DynamicLibraryName = "DynamicDocuments";
        public const string EasyAttachmentSync = "EasyAttachmentSync";
        public const string DataSynchronizer = "DataSynchronizer";
        public const string BackupExtension = ".backup";
        public const string ActivationObject = "ActivationObject";

        public const string TbMailer = "TbMailer";
        public const string DataFile = "DataFile";


        public const string RemoteClientsFile = "RemoteClients.xml";

        //funzionalità  menu
        public const string MenuHistory = "MenuHistory";
        public const string MenuMostUsed = "MenuMostUsed";
        public const string ShowFullMenuItems = "ShowFullMenuItems";
    }

    //=========================================================================
    public sealed class ReportObjectsXML
    {
        private ReportObjectsXML()
        { }
        public sealed class Element
        {
            private Element()
            { }
            public const string ReportObjects = "ReportObjects";
            public const string Reports = "Reports";
            public const string Report = "Report";
        }
        public sealed class Attribute
        {
            private Attribute()
            { }
            public const string DefaultReport = "defaultReport";
            public const string NameSpace = "namespace";
            public const string Localize = "localize";
        }
    }

    //========================================================================
    public sealed class ClientDocumentObjectsXML
    {
        private ClientDocumentObjectsXML()
        { }
        public sealed class Element
        {
            private Element()
            { }
            public const string ClientDocumentObjects = "ClientDocumentObjects";
            public const string ClientDocuments = "ClientDocuments";
            public const string ServerDocument  = "ServerDocument";
            public const string ClientDocument  = "ClientDocument";
            public const string ClientForms     = "ClientForms";
            public const string ClientForm      = "ClientForm";
        }
        public sealed class Attribute
        {
            private Attribute()
            { }
            public const string Type        = "type";
            public const string Class       = "class";
            public const string Localize    = "localize";
            public const string Namespace   = "namespace";
            public const string Server      = "server";
            public const string Name        = "name";
            public const string Family      = "family";
        }
    }

    //========================================================================
    public sealed class InstallationAbstractXML
    {
        private InstallationAbstractXML()
        { }
        public sealed class Element
        {
            private Element()
            { }
            public const string Root = "Root";
            public const string LatestUpdate = "LatestUpdate";
        }
        public sealed class Attribute
        {
            private Attribute()
            { }
            public const string UtcDate = "utcDate";
        }
    }

    //=========================================================================
    public sealed class ConstString
    {
        public const string providerNT = "WinNT://";
        public const string MicroareaSite = "www.microarea.it";
        public const string JavaScripts = @"function ToggleVisibility(elementId)
		{
		currentElement = document.getElementById(elementId);
		if (currentElement != null) 
	{
		currentVisibleValue = currentElement.style.display;
		if (currentVisibleValue == 'none')
		currentElement.style.display = 'block';
		else currentElement.style.display = 'none';
	}
}
function callWebService() {
 callObj = service.createCallOptions(); 
 callObj.async = false;
 callObj.funcName = 'SendAccessMail';
 var res = service.LoginManager.callService(callObj);
 if (res.error || !res.value) {
 return '<html><head><link rel=\'stylesheet\' href=\'microareatable.css\' type=\'text/css\'><link rel=\'stylesheet\' href=\'microareanew.css\' type=\'text/css\'></head><body><div id=\'result\'  align=center>Error sending mail, please visit <a href=\'http://www.microarea.it\' target=\'_blank\'>www.microarea.it</a></div></body></html>';
 }
 return '<html><head><link rel=\'stylesheet\' href=\'microareatable.css\' type=\'text/css\'><link rel=\'stylesheet\' href=\'microareanew.css\' type=\'text/css\'></head><body><div id=\'result\'  align=center>Mail correctly sent.</div></body></html>';
 }
function init() {
 service.useService('##LOGINMANAGERURL##?WSDL', 'LoginManager');
 }
";
    }
}
