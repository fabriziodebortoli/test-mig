using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.Library.SqlScriptUtility;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
	#region Enums
	[Flags]
	public enum FilesToGenerate : int
	{
		Undefined				= 0x00000,
		ConfigurationFiles		= 0x00001,
		MenuFiles				= 0x00002,
		LibrariesSourceCode		= 0x00004,
		TablesWoormReports		= 0x00008,
		DatabaseScripts			= 0x00010,
		AllFileTypes			= 0x00FFF,
		PreserveInjectedCode	= 0x01000,
		GenerateCSharp          = 0x10000,
		Default					= AllFileTypes | PreserveInjectedCode
	}

	public enum WritingCodeBehaviour
	{
		Undefined		= 0x0000,
		WriteNew		= 0x0001,
		Overwrite		= 0x0002,
		OverwriteAll	= 0x0003,
		Skip			= 0x0004,
		SkipAll			= 0x0005,
		Abend			= 0x0006,
		Default			= Abend
	}
	#endregion

	#region public delegates
	
	public delegate WritingCodeBehaviour CodeOverwriteEventHandler(string aFilename);
	public delegate bool CodeWriteFailureEventHandler(string aFilename, string errorMessage);
	public delegate void UnresolvedInjectionPointsEventHandler(object sender, string originalFile, string temporaryFile, InjectionPointsCollection unresolvedInjectionPoints, Encoding encoding);
	public delegate void TBWizardCodeGeneratorEventHandler(object sender, string eventText);
	
	#endregion

	/// <summary>
	/// Summary description for WizardCodeGenerator.
	/// </summary>
	//=================================================================================
	public class WizardCodeGenerator
	{
		#region WizardCodeGenerator private data members

		private FilesToGenerate					filesToGenerate = FilesToGenerate.Default;
		private WizardApplicationInfo			applicationInfo = null;
		private WritingCodeBehaviour			commonBehaviourToApply = WritingCodeBehaviour.Undefined;
		private CodeOverwriteEventHandler		codeOverwriteEventHandler = null;
		private CodeWriteFailureEventHandler	codeWriteFailureEventHandler = null;

		private const string ApplicationConfigXmlVersion		= "1.0";
		private const string ApplicationConfigXmlEncoding		= "UTF-8";
		private const string XML_APP_CFG_APPLICATIONINFO_TAG	= "ApplicationInfo";
		private const string XML_APP_CFG_TYPE_TAG				= "Type";
		private const string XML_APP_CFG_DBSIGNATURE_TAG		= "DbSignature";
		private const string XML_APP_CFG_VERSION_TAG			= "Version";		

		private const string ApplicationSolutionXmlVersion							= "1.0";
		private const string ApplicationSolutionXmlEncoding							= "UTF-8";
		private const string XML_APP_SOLUTION_PRODUCT_TAG							= "Product";
		private const string XML_APP_SOLUTION_SALESMODULE_TAG						= "SalesModule";
		private const string XML_APP_SOLUTION_SHORT_NAMES_TAG						= "ShortNames";
		private const string XML_APP_SOLUTION_SHORT_NAME_TAG						= "ShortName";
		private const string XML_APP_SOLUTIONMODULE_TAG								= "Application";
		private const string XML_MOD_SOLUTIONMODULE_TAG								= "Module";
		private const string XML_APP_SOLUTION_PRODUCT_TITLE_ATTRIBUTE				= "localize";
		private const string XML_APP_SOLUTION_PRODUCT_ACTIVATION_VERSION_ATTRIBUTE	= "activationversion";
		private const string XML_APP_SOLUTION_MODULE_NAME_ATTRIBUTE					= "name";
		private const string XML_APP_SOLUTIONMODULE_PRODUCER_ATTRIBUTE				= "producer";
		private const string XML_APP_SOLUTIONMODULE_TITLE_ATTRIBUTE					= "localize";
		private const string XML_APP_SOLUTIONMODULE_EDITION_ATTRIBUTE				= "edition";
		private const string XML_APP_SOLUTIONMODULE_APPNAME_ATTRIBUTE				= "name";
		private const string XML_APP_SOLUTIONMODULE_MODNAME_ATTRIBUTE				= "name";
		private const string XML_APP_SOLUTIONMODULE_CONTAINER_ATTRIBUTE				= "container";
		private const string XML_APP_SOLUTIONMODULE_SHORTNAME_ATTRIBUTE				= "name";

		private const string ApplicationStorageConfigXmlVersion			= "1.0";
		private const string ApplicationStorageConfigXmlEncoding		= "UTF-8";
		private const string XML_APP_STORAGE_CONFIG_PRODUCT_STATE_TAG	= "InstalledProductState";
		private const string XML_APP_STORAGE_CONFIG_STORAGE_NAME_TAG	= "StorageName";
		private const string XML_APP_STORAGE_CONFIG_BRANDED_PRODUCT_TAG	= "BrandedProduct";
		private const string XML_APP_STORAGE_CONFIG_RUNNING_IMAGE_TAG	= "RunningImage";
		private const string XML_APP_STORAGE_CONFIG_SOLUTION_TYPE_TAG	= "Type";
		private const string XML_APP_STORAGE_CONFIG_ADDON_SOLUTION		= "AddOn";
		private const string XML_APP_STORAGE_CONFIG_MASTER_SOLUTION		= "Master";
		private const string XML_APP_STORAGE_CONFIG_CURR_REL_EXT_TAG	= "CurrentReleaseExtended";
		private const string XML_APP_STORAGE_CONFIG_CURR_REL_EXT_TEXT	= "0.0.0.00000000";

		private const string ModuleConfigXmlVersion						= "1.0";
		private const string ModuleConfigXmlEncoding					= "UTF-8";
		private const string XML_MOD_CFG_MODULEINFO_TAG					= "ModuleInfo";
		private const string XML_MOD_CFG_COMPONENTS_TAG					= "Components";
		private const string XML_MOD_CFG_LIBRARY_TAG					= "Library";
		private const string XML_MOD_CFG_LOCALIZE_ATTRIBUTE				= "localize";
		private const string XML_MOD_CFG_DESTINATIONFOLDER_ATTRIBUTE	= "destinationfolder";
		private const string XML_MOD_CFG_LIB_NAME_ATTRIBUTE				= "name";
		private const string XML_MOD_CFG_LIB_SOURCEFOLDER_ATTRIBUTE		= "sourcefolder";
		private const string XML_MOD_CFG_LIB_DEPLOYMENTPOLICY_ATTRIBUTE	= "deploymentpolicy";

		private const string DatabaseInfoXmlVersion						= "1.0";
		private const string DatabaseInfoXmlEncoding					= "UTF-8";

		private const string ModuleDatabaseObjectsXmlVersion			= "1.0";
		private const string ModuleDatabaseObjectsXmlEncoding			= "UTF-8";

		#region DocumentObjects.xml constant strings
		
		private const string DocumentObjectsXmlVersion	= "1.0";
		private const string DocumentObjectsXmlEncoding	= "UTF-8";
		private const string DefaultViewModeValue		= "Default";

		private const string XML_MOD_DOCOBJS_DOCUMENTOBJECTS_TAG		= "DocumentObjects";
		private const string XML_MOD_DOCOBJS_DOCUMENTS_TAG				= "Documents";
		private const string XML_MOD_DOCOBJS_DOCUMENT_TAG				= "Document";
		private const string XML_MOD_DOCOBJS_NAME_INTERFACECLASS_TAG	= "InterfaceClass";
		private const string XML_MOD_DOCOBJS_VIEWMODES_TAG				= "ViewModes";
		private const string XML_MOD_DOCOBJS_MODE_TAG					= "Mode";
		private const string XML_MOD_DOCOBJS_NAMESPACE_ATTRIBUTE		= "namespace";
		private const string XML_MOD_DOCOBJS_LOCALIZE_ATTRIBUTE			= "localize";
		private const string XML_MOD_DOCOBJS_CLASSHIERARCHY_ATTRIBUTE	= "classhierarchy";
		private const string XML_MOD_DOCOBJS_NAME_ATTRIBUTE				= "name";
		
		#endregion

		#region ClientDocumentObjects.xml constant strings
		
		private const string ClientDocumentObjectsXmlVersion	= "1.0";
		private const string ClientDocumentObjectsXmlEncoding	= "UTF-8";

		private const string XML_MOD_CLIENTDOCS_CLIENTDOCUMENTOBJECTS_TAG	= "ClientDocumentObjects";
		private const string XML_MOD_CLIENTDOCS_CLIENTDOCUMENTS_TAG			= "ClientDocuments";
		private const string XML_MOD_CLIENTDOCS_SERVERDOCUMENT_TAG			= "ServerDocument";
		private const string XML_MOD_CLIENTDOCS_CLIENTDOCUMENT_TAG			= "ClientDocument";
		private const string XML_MOD_CLIENTDOCS_NAMESPACE_ATTRIBUTE			= "namespace";
		private const string XML_MOD_CLIENTDOCS_TYPE_ATTRIBUTE				= "type";
		private const string XML_MOD_CLIENTDOCS_CLASS_ATTRIBUTE				= "class";
		private const string XML_MOD_CLIENTDOCS_LOCALIZE_ATTRIBUTE			= "localize";
		
		private const string XML_MOD_CLIENTDOCS_FAMILY_ATTRIBUTE_VALUE	= "family";

		#endregion

		#region Enums.xml constant strings

		private const string EnumsXmlVersion	= "1.0";
		private const string EnumsXmlEncoding	= "UTF-8";

		private const string XML_MOD_ENUMS_ENUMS_TAG	= "Enums";
		private const string XML_MOD_ENUMS_TAG			= "Tag";
		private const string XML_MOD_ENUMS_ITEM_TAG		= "Item";

		private const string XML_MOD_ENUMS_NAME_ATTRIBUTE			= "name";
		private const string XML_MOD_ENUMS_VALUE_ATTRIBUTE			= "value";
		private const string XML_MOD_ENUMS_STORED_ATTRIBUTE			= "stored";
		private const string XML_MOD_ENUMS_DEFAULTVALUE_ATTRIBUTE	= "defaultValue";

		#endregion

		#region EventHandlerObjects.xml constant strings

		private const string EventHandlerObjectsXmlVersion	= "1.0";
		private const string EventHandlerObjectsXmlEncoding	= "UTF-8";
		
		private const string XML_MOD_EVENT_HANDLER_OBJS_FUNCTIONOBJECTS_TAG	= "FunctionObjects";
		private const string XML_MOD_EVENT_HANDLER_OBJS_FUNCTIONS_TAG		= "Functions";
		private const string XML_MOD_EVENT_HANDLER_OBJS_FUNCTION_TAG		= "Function";
		private const string XML_MOD_EVENT_HANDLER_OBJS_NAMESPACE_ATTRIBUTE	= "namespace";
		private const string XML_MOD_EVENT_HANDLER_OBJS_TYPE_ATTRIBUTE		= "type";
		private const string XML_MOD_EVENT_HANDLER_OBJS_LOCALIZE_ATTRIBUTE	= "localize";

		private const string ApplicationDateChangeDefaultText	= "Application Date Change";
		private const string CompanyConnectionChangeDefaultText	= "Company Connection Change";

		#endregion

		#region Document description files constant string

		private const string DocumentDescriptionXmlVersion		= "1.0";
		private const string DocumentDescriptionXmlEncoding		= "UTF-8";
		private const string XML_DOCDESCR_DOCUMENT_TAG			= "Document";
		private const string XML_DOCDESCR_VERSION_TAG			= "Version";
		private const string XML_DOCDESCR_MAXDOCUMENTS_TAG		= "MaxDocuments";
		private const string XML_DOCDESCR_MAXDIMENSION_TAG		= "MaxDimension";
		private const string XML_DOCDESCR_DATAURL_TAG			= "DataUrl";
		private const string XML_DOCDESCR_ENVELOPECLASS_TAG		= "EnvelopeClass";
		private const string XML_DOCDESCR_EXTENSION_ATTRIBUTE	= "extension";
		private const string XML_DOCDESCR_LOCALIZABLE_ATTRIBUTE = "localizable";

		private const string XML_DOCDBTSDESCR_DBTS_TAG				= "DBTs";
		private const string XML_DOCDBTSDESCR_MASTER_TAG			= "Master";
		private const string XML_DOCDBTSDESCR_DBT_TITLE_TAG			= "Title";
		private const string XML_DOCDBTSDESCR_DBT_TABLE_TAG			= "Table";
		private const string XML_DOCDBTSDESCR_SLAVES_TAG			= "Slaves";
		private const string XML_DOCDBTSDESCR_SLAVE_TAG				= "Slave";
		private const string XML_DOCDBTSDESCR_SLAVEBUFFERED_TAG		= "SlaveBuffered";
		private const string XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE	= "namespace";
		private const string XML_DOCDBTSDESCR_LOCALIZABLE_ATTRIBUTE = "localizable";

		private const string XML_DOC_EXT_REFS_MAIN_TAG				= "MainExternalReferences";
		private const string XML_DOC_EXT_REFS_DBT_TAG				= "DBT";
		private const string XML_DOC_EXT_REFS_DBT_EXPORT_TAG		= "Export";
		private const string XML_DOC_EXT_REFS_DBT_EXT_REFS_TAG		= "ExternalReferences";
		private const string XML_DOC_EXT_REFS_DBT_EXT_REF_TAG		= "ExternalReference";
		private const string XML_DOC_EXT_REFS_KEYS_TAG				= "Keys";
		private const string XML_DOC_EXT_REFS_KEY_SEGMENT_TAG		= "KeySegment";
		private const string XML_DOC_EXT_REFS_FOREIGN_KEYSEG_TAG	= "Foreign";
		private const string XML_DOC_EXT_REFS_PRIMARY_KEYSEG_TAG	= "Primary";

		private const int defaultDocumentVersion = 1;
		private const int defaultDocumentMaxDocuments = 10;
		private const int defaultDocumentMaxDimension = 100;

		#endregion 

		#region Report.xml constant string

		private const string ReportsXmlVersion	= "1.0";
		private const string ReportsXmlEncoding	= "UTF-8";
		private const string XML_REPORTS_REPORTOBJECTS_TAG	= "ReportObjects";
		private const string XML_REPORTS_REPORTS_TAG		= "Reports";
		private const string XML_REPORTS_REPORT_TAG			= "Report";

		private const string XML_REPORTS_DEFAULTREPORT_ATTRIBUTE	= "defaultReport";
		private const string XML_REPORTS_NAMESPACE_ATTRIBUTE		= "namespace";
		private const string XML_REPORTS_LOCALIZE_ATTRIBUTE			= "localize";

		#endregion 

		#region ReferenceObjects File constant strings

		private const string ReferenceObjectsXmlVersion	= "1.0";
		private const string ReferenceObjectsXmlEncoding = "UTF-8";

		public const string XML_HOTLINK_CLASS_NAME_TAG = "ClassName";

		#endregion


		#endregion

		#region WizardCodeGenerator constructors

		//---------------------------------------------------------------------------
		public WizardCodeGenerator
			(
			WizardApplicationInfo			aApplicationInfo, 
			FilesToGenerate					filesToGenerateFlags,
			CodeOverwriteEventHandler		aCodeOverwriteEventHandler,
			CodeWriteFailureEventHandler	aCodeWriteFailureEventHandler
			)
		{
			applicationInfo					= aApplicationInfo;
			filesToGenerate					= filesToGenerateFlags;
			codeOverwriteEventHandler		= aCodeOverwriteEventHandler;
			codeWriteFailureEventHandler	= aCodeWriteFailureEventHandler;
		}

		//---------------------------------------------------------------------------
		public WizardCodeGenerator
			(
			WizardApplicationInfo			aApplicationInfo, 
			CodeOverwriteEventHandler		aCodeOverwriteEventHandler,
			CodeWriteFailureEventHandler	aCodeWriteFailureEventHandler
			)
			:
			this(aApplicationInfo, FilesToGenerate.Default, aCodeOverwriteEventHandler, aCodeWriteFailureEventHandler)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardCodeGenerator (WizardApplicationInfo aApplicationInfo)
			:
			this(aApplicationInfo, FilesToGenerate.Default, null, null)
		{
		}

		#endregion

		#region public events
	
		public event TBWizardCodeGeneratorEventHandler StartingFileGeneration = null;
		public event TBWizardCodeGeneratorEventHandler StartingFileOverwrite = null;
		public event TBWizardCodeGeneratorEventHandler SubstitutionInProgress = null;
		public event UnresolvedInjectionPointsEventHandler ResolveCustomInjectionPoints = null;

		#endregion
		
		#region WizardCodeGenerator private methods

		//---------------------------------------------------------------------------
		private static bool EnsureDirectoryExistence(string aPathToCheck)
		{
			if (string.IsNullOrEmpty(aPathToCheck))
				return false;
			
			try
			{
				if (!Directory.Exists(aPathToCheck))
					Directory.CreateDirectory(aPathToCheck);
				
				return true;
			}
			catch(Exception exception)
			{				
				string errorMessage = null;
				if (exception is IOException)
					errorMessage = TBWizardProjectsStrings.IOExceptionErrorMsg;
				else if (exception is UnauthorizedAccessException)
					errorMessage = TBWizardProjectsStrings.UnauthorizedAccessExceptionErrorMsg;
				else if (exception is PathTooLongException)
					errorMessage = TBWizardProjectsStrings.PathTooLongExceptionErrorMsg;
				else if (exception is DirectoryNotFoundException)
					errorMessage = TBWizardProjectsStrings.DirectoryNotFoundExceptionErrorMsg;
				else if (exception is ArgumentException)
					errorMessage = TBWizardProjectsStrings.InvalidPathStringErrorMsg;
				else if (exception is NotSupportedException)
					errorMessage = TBWizardProjectsStrings.NotSupportedExceptionErrorMsg;
				else
					errorMessage = exception.Message;
				
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.EnsureDirectoryExistenceErrorMsg, aPathToCheck, errorMessage));
			}
		}

		//---------------------------------------------------------------------------
		private static bool EnsureFileDirectoryExistence(string aFullFileName)
		{
			if (string.IsNullOrEmpty(aFullFileName))
				return false;

			try
			{
				return EnsureDirectoryExistence(Path.GetDirectoryName(aFullFileName));
			}
			catch(TBWizardException exception)
			{
				throw exception;
			}
			catch(ArgumentException)// Potrebbe essere sollevata da Path.GetDirectoryName
			{
				throw new TBWizardException(TBWizardProjectsStrings.InvalidPathStringErrorMsg);
			}
		}
		
		//---------------------------------------------------------------------------
		private WritingCodeBehaviour IsCodeFileToWrite(string aFileName)
		{
			if (string.IsNullOrEmpty(aFileName))
				return WritingCodeBehaviour.Skip;

			FileStream stream = null;
			try
			{
				WritingCodeBehaviour behaviourToApply = WritingCodeBehaviour.Default;
			
				if (EnsureFileDirectoryExistence(aFileName))
				{
					if (!File.Exists(aFileName))
					{
						if (StartingFileGeneration != null)
							StartingFileGeneration(this, aFileName);

						return WritingCodeBehaviour.WriteNew;
					}

					if (commonBehaviourToApply == WritingCodeBehaviour.Undefined)
					{
						if (codeOverwriteEventHandler != null)
							behaviourToApply = codeOverwriteEventHandler(aFileName);

						if (behaviourToApply == WritingCodeBehaviour.OverwriteAll)
							commonBehaviourToApply = behaviourToApply = WritingCodeBehaviour.Overwrite;
						else if (behaviourToApply == WritingCodeBehaviour.SkipAll)
							commonBehaviourToApply = behaviourToApply = WritingCodeBehaviour.Skip;
						else
							commonBehaviourToApply = WritingCodeBehaviour.Undefined;
					}
					else
						behaviourToApply = commonBehaviourToApply;

					if (behaviourToApply == WritingCodeBehaviour.Overwrite)
					{
						// Provo ad aprire e a chiudere subito dopo il file in modo da
						// verificarne l'accessibilità
						stream = File.Open(aFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
						if (stream.CanRead && stream.CanWrite)
						{
							if (StartingFileOverwrite != null)
								StartingFileOverwrite(this, aFileName);

							return WritingCodeBehaviour.Overwrite;
						}
					}
				}
				return behaviourToApply;
			}
			catch(Exception exception)
			{				
				if (codeWriteFailureEventHandler != null)
				{
					string errorMessage = null;
					if (exception is IOException)
						errorMessage = TBWizardProjectsStrings.IOExceptionErrorMsg;
					else if (exception is UnauthorizedAccessException)
						errorMessage = TBWizardProjectsStrings.UnauthorizedAccessExceptionErrorMsg;
					else if (exception is PathTooLongException)
						errorMessage = TBWizardProjectsStrings.PathTooLongExceptionErrorMsg;
					else if (exception is DirectoryNotFoundException)
						errorMessage = TBWizardProjectsStrings.DirectoryNotFoundExceptionErrorMsg;
					else if (exception is NotSupportedException)
						errorMessage = TBWizardProjectsStrings.NotSupportedExceptionErrorMsg;
					else
						errorMessage = exception.Message;
				
					if (codeWriteFailureEventHandler(aFileName, errorMessage))
						return WritingCodeBehaviour.Skip;
				}
				else
					Debug.Fail("Exception raised in WizardCodeGenerator.IsCodeFileToWrite:" + exception.Message);
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
			return WritingCodeBehaviour.Abend;
		}

		//---------------------------------------------------------------------------
		private bool GenerateApplicationConfigurationFile(IBasePathFinder aPathFinder)
		{
			if (applicationInfo == null || string.IsNullOrEmpty(applicationInfo.Name) || aPathFinder == null)
				return false;

			if (!GenerateConfigurationFiles)
				return true;

			string applicationPath = GetStandardApplicationPath(aPathFinder);
			if (string.IsNullOrEmpty(applicationPath))
				return false;

			string configFilename = applicationPath + 
				Path.DirectorySeparatorChar		+
				NameSolverStrings.Application	+ 
				NameSolverStrings.ConfigExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(configFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			try
			{	 
				XmlDocument configXmlDocument = new XmlDocument();
				XmlDeclaration configDeclaration = configXmlDocument.CreateXmlDeclaration(ApplicationConfigXmlVersion, ApplicationConfigXmlEncoding, null);
				if (configDeclaration != null)
					configXmlDocument.AppendChild(configDeclaration);
				
				XmlElement appInfoNode = configXmlDocument.CreateElement(XML_APP_CFG_APPLICATIONINFO_TAG);
				if (appInfoNode == null)
					return false;

				XmlElement typeNode = configXmlDocument.CreateElement(XML_APP_CFG_TYPE_TAG);
				ApplicationType appType = applicationInfo.Type;
				if (appType == ApplicationType.Undefined)
					appType = ApplicationType.TaskBuilderApplication;
				if (appType == ApplicationType.TaskBuilderApplication)
					typeNode.InnerText = Generics.TaskBuilderApplicationConfigText;
				else
					typeNode.InnerText = BaseApplicationInfo.GetApplicationTypeString(appType);
				appInfoNode.AppendChild(typeNode);
					
				XmlElement dbSignatureNode = configXmlDocument.CreateElement(XML_APP_CFG_DBSIGNATURE_TAG);
				string appDbSignature = applicationInfo.DbSignature;
				if (string.IsNullOrEmpty(appDbSignature))
					appDbSignature = applicationInfo.Name.ToUpper();
				dbSignatureNode.InnerText = appDbSignature;
				appInfoNode.AppendChild(dbSignatureNode);

				XmlElement versionNode = configXmlDocument.CreateElement(XML_APP_CFG_VERSION_TAG);
				versionNode.InnerText = applicationInfo.Version;
				appInfoNode.AppendChild(versionNode);

				configXmlDocument.AppendChild(appInfoNode);

				configXmlDocument.Save(configFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateApplicationConfigurationFile:", exception.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateApplicationSolutionFiles(IBasePathFinder aPathFinder)
		{
			return GenerateApplicationSolutionFile(aPathFinder) &&
				GenerateApplicationModulesSolutionFile(aPathFinder);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateApplicationSolutionFile(IBasePathFinder aPathFinder)
		{
			if (applicationInfo == null || string.IsNullOrEmpty(applicationInfo.Name) || aPathFinder == null)
				return false;

			if (!GenerateConfigurationFiles)
				return true;
		
			string solutionFilename = applicationInfo.GetCodeSolutionFileName(aPathFinder);
			if (string.IsNullOrEmpty(solutionFilename))
				return false;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(solutionFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			try
			{	 
				XmlDocument solutionXmlDocument = new XmlDocument();

				XmlDeclaration solutionDeclaration = solutionXmlDocument.CreateXmlDeclaration(ApplicationSolutionXmlVersion, ApplicationSolutionXmlEncoding, null);
				if (solutionDeclaration != null)
					solutionXmlDocument.AppendChild(solutionDeclaration);
				
				XmlElement productNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTION_PRODUCT_TAG);
				if (productNode == null)
					return false;

				productNode.SetAttribute(XML_APP_SOLUTION_PRODUCT_TITLE_ATTRIBUTE, applicationInfo.Title);

				int activationVersion = 2;
				productNode.SetAttribute(XML_APP_SOLUTION_PRODUCT_ACTIVATION_VERSION_ATTRIBUTE, activationVersion.ToString());

				XmlElement salesModuleNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTION_SALESMODULE_TAG);
				if (salesModuleNode == null)
					return false;

				salesModuleNode.SetAttribute(XML_APP_SOLUTION_MODULE_NAME_ATTRIBUTE, applicationInfo.Name);

				productNode.AppendChild(salesModuleNode);
		
				solutionXmlDocument.AppendChild(productNode);

				solutionXmlDocument.Save(solutionFilename);
	
				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateApplicationSolutionFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateApplicationModulesSolutionFile(IBasePathFinder aPathFinder)
		{
			if (applicationInfo == null || string.IsNullOrEmpty(applicationInfo.Name) || aPathFinder == null)
				return false;

			if (!GenerateConfigurationFiles)
				return true;

			string solutionModulesPath = applicationInfo.GetCodeSolutionModulesPath(aPathFinder);
			if (string.IsNullOrEmpty(solutionModulesPath))
				return false;
			
			string solutionModulesFilename = solutionModulesPath + 
				Path.DirectorySeparatorChar		+
				applicationInfo.Name + 
				NameSolverStrings.XmlExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(solutionModulesFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			try
			{	 
				XmlDocument solutionXmlDocument = new XmlDocument();

				XmlDeclaration solutionDeclaration = solutionXmlDocument.CreateXmlDeclaration(ApplicationSolutionXmlVersion, ApplicationSolutionXmlEncoding, null);
				if (solutionDeclaration != null)
					solutionXmlDocument.AppendChild(solutionDeclaration);

				XmlElement salesModuleNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTION_SALESMODULE_TAG);
				if (salesModuleNode == null)
					return false;

				salesModuleNode.SetAttribute(XML_APP_SOLUTIONMODULE_PRODUCER_ATTRIBUTE, applicationInfo.Producer);
				salesModuleNode.SetAttribute(XML_APP_SOLUTIONMODULE_TITLE_ATTRIBUTE, applicationInfo.Title);
				
				if (applicationInfo.Edition == WizardApplicationInfo.SolutionEdition.Standard)
					salesModuleNode.SetAttribute(XML_APP_SOLUTIONMODULE_EDITION_ATTRIBUTE, NameSolverStrings.StandardEdition);
				else if (applicationInfo.Edition == WizardApplicationInfo.SolutionEdition.Professional)
					salesModuleNode.SetAttribute(XML_APP_SOLUTIONMODULE_EDITION_ATTRIBUTE, NameSolverStrings.ProfessionalEdition);
				else if (applicationInfo.Edition == WizardApplicationInfo.SolutionEdition.Enterprise)
					salesModuleNode.SetAttribute(XML_APP_SOLUTIONMODULE_EDITION_ATTRIBUTE, NameSolverStrings.EnterpriseEdition);

				XmlElement shortNamesNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTION_SHORT_NAMES_TAG);
				if (shortNamesNode == null)
					return false;
				
				XmlElement shortNameNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTION_SHORT_NAME_TAG);
				if (shortNameNode == null)
					return false;
			
                shortNameNode.SetAttribute(XML_APP_SOLUTIONMODULE_SHORTNAME_ATTRIBUTE, applicationInfo.ShortName);

				shortNamesNode.AppendChild(shortNameNode);

				salesModuleNode.AppendChild(shortNamesNode);

				XmlElement applicationNode = solutionXmlDocument.CreateElement(XML_APP_SOLUTIONMODULE_TAG);
				if (applicationNode == null)
					return false;

				applicationNode.SetAttribute(XML_APP_SOLUTIONMODULE_CONTAINER_ATTRIBUTE, NameSolverStrings.TaskBuilderApplications);
				applicationNode.SetAttribute(XML_APP_SOLUTIONMODULE_APPNAME_ATTRIBUTE, applicationInfo.Name);

				if (applicationInfo.ModulesCount > 0)
				{
					foreach(WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
					{
						XmlElement moduleNode = solutionXmlDocument.CreateElement(XML_MOD_SOLUTIONMODULE_TAG);
						if (moduleNode == null)
							return false;

						moduleNode.SetAttribute(XML_APP_SOLUTIONMODULE_MODNAME_ATTRIBUTE, aModuleInfo.Name);
						
						applicationNode.AppendChild(moduleNode);
					}
				}
				
				salesModuleNode.AppendChild(applicationNode);
		
				solutionXmlDocument.AppendChild(salesModuleNode);

				solutionXmlDocument.Save(solutionModulesFilename);
	
				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateApplicationModulesSolutionFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateModulesCode(IBasePathFinder aPathFinder)
		{
			if (applicationInfo == null || aPathFinder == null)
				return false;

			if (applicationInfo.ModulesCount == 0)
				return true;
			
			foreach(WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
			{
				if (!GenerateModuleCode(aModuleInfo, aPathFinder))
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		private bool GenerateApplicationCodeSolutionFile(IBasePathFinder aPathFinder)
		{
			if (applicationInfo == null || aPathFinder == null)
				return false;

			return GenerateApplicationSourceFile(aPathFinder, "Application_sln.tmpl", applicationInfo.Name + Generics.NetSolutionExtension);
		}

		//---------------------------------------------------------------------------
		private bool GenerateApplicationSourceFile
			(
			IBasePathFinder aPathFinder,
			string templateResourceName,
			string codeFileName
			)
		{
			if (applicationInfo == null || aPathFinder == null ||
				string.IsNullOrEmpty(templateResourceName) || string.IsNullOrEmpty(codeFileName))
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string applicationPath = GetStandardApplicationPath(aPathFinder);

			string outputFileName = applicationPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardApplicationCodeTemplateParser codeParser = new WizardApplicationCodeTemplateParser(applicationInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);
			
			return codeParser.WriteFileFromTemplate("Application." + templateResourceName, outputFileName, saveInjectionPointsCode);
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleConfigurationFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || 
				string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			if (!GenerateConfigurationFiles)
				return true;
			
			string modulePath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(modulePath))
				return false;

			string configFilename = modulePath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.Module	+ 
				NameSolverStrings.ConfigExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(configFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			try
			{	 
				XmlDocument configXmlDocument = new XmlDocument();
				XmlDeclaration configDeclaration = configXmlDocument.CreateXmlDeclaration(ModuleConfigXmlVersion, ModuleConfigXmlEncoding, null);
				if (configDeclaration != null)
					configXmlDocument.AppendChild(configDeclaration);
			
				XmlElement moduleInfoNode = configXmlDocument.CreateElement(XML_MOD_CFG_MODULEINFO_TAG);
				if (moduleInfoNode == null)
					return false;
				moduleInfoNode.SetAttribute(XML_MOD_CFG_LOCALIZE_ATTRIBUTE, aModuleInfo.Title);
				moduleInfoNode.SetAttribute(XML_MOD_CFG_DESTINATIONFOLDER_ATTRIBUTE, NameSolverStrings.TbApps);
 
				XmlElement componentsNode = configXmlDocument.CreateElement(XML_MOD_CFG_COMPONENTS_TAG);
				moduleInfoNode.AppendChild(componentsNode);

				if (aModuleInfo.LibrariesCount > 0)
				{
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlElement libraryNode = configXmlDocument.CreateElement(XML_MOD_CFG_LIBRARY_TAG);

						libraryNode.SetAttribute(XML_MOD_CFG_LIB_NAME_ATTRIBUTE, aLibraryInfo.Name);
						libraryNode.SetAttribute(XML_MOD_CFG_LIB_SOURCEFOLDER_ATTRIBUTE, aLibraryInfo.SourceFolder);
						libraryNode.SetAttribute(XML_MOD_CFG_LIB_DEPLOYMENTPOLICY_ATTRIBUTE, "full");

						componentsNode.AppendChild(libraryNode);
					}
				}

				configXmlDocument.AppendChild(moduleInfoNode);

				configXmlDocument.Save(configFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleConfigurationFile:", exception.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateModuleDatabaseCreationScripts(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null)
				return false;

			if (!GenerateDatabaseScripts || (!aModuleInfo.HasTables && !aModuleInfo.HasExtraAddedColumns))
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string createInfoFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.CreateScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.CreateInfoXml;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(createInfoFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			XmlDocument createInfoDocument = null;
			XmlElement	level1Element = null;

			try
			{
				if (behaviourToApply != WritingCodeBehaviour.Skip)
				{
					createInfoDocument = new XmlDocument();
					XmlDeclaration createInfoDeclaration = createInfoDocument.CreateXmlDeclaration(DatabaseInfoXmlVersion, DatabaseInfoXmlEncoding, null);
					if (createInfoDeclaration != null)
						createInfoDocument.AppendChild(createInfoDeclaration);
			
					XmlElement createInfoElement = createInfoDocument.CreateElement(Generics.XmlTagDbCreateInfo);
					if (createInfoElement != null)
					{
						createInfoDocument.AppendChild(createInfoElement);
						XmlElement moduleInfoElement = createInfoDocument.CreateElement(Generics.XmlTagDbInfoModuleinfo);
						if (moduleInfoElement != null)
						{
							moduleInfoElement.SetAttribute(Generics.XmlAttributeDbInfoModuleName, aModuleInfo.Name);
							createInfoElement.AppendChild(moduleInfoElement);
						}
						
						level1Element = createInfoDocument.CreateElement(Generics.XmlTagDbInfoLevel1);
						if (level1Element != null)
							createInfoElement.AppendChild(level1Element);

						XmlElement level2Element = createInfoDocument.CreateElement(Generics.XmlTagDbInfoLevel2);
						if (level2Element != null)
							createInfoElement.AppendChild(level2Element);
					}
				}

				string moduleObjectsPath = GetStandardModuleObjectsPath(aPathFinder, aModuleInfo);
				if (string.IsNullOrEmpty(moduleObjectsPath))
					return false;

				string databaseObjectsFilename = moduleObjectsPath + 
					Path.DirectorySeparatorChar	+
					NameSolverStrings.DatabaseObjectsXml;

				behaviourToApply = IsCodeFileToWrite(databaseObjectsFilename);
			
				if (behaviourToApply == WritingCodeBehaviour.Abend)
					return false;
			
				XmlDocument moduleDatabaseObjectsDocument = null;
				XmlElement	tablesElement = null;
				
				if (behaviourToApply != WritingCodeBehaviour.Skip)
				{
					moduleDatabaseObjectsDocument = new XmlDocument();
					XmlDeclaration moduleDatabaseObjectsDeclaration = moduleDatabaseObjectsDocument.CreateXmlDeclaration(ModuleDatabaseObjectsXmlVersion, ModuleDatabaseObjectsXmlEncoding, null);
					if (moduleDatabaseObjectsDeclaration != null)
						moduleDatabaseObjectsDocument.AppendChild(moduleDatabaseObjectsDeclaration);
			
					XmlElement moduleDatabaseObjectsElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.DatabaseObjects);
					if (moduleDatabaseObjectsElement != null)
					{
						moduleDatabaseObjectsDocument.AppendChild(moduleDatabaseObjectsElement);
					
						XmlElement signatureElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Signature);
						if (signatureElement != null)
						{
							signatureElement.InnerText = aModuleInfo.DbSignature;
							moduleDatabaseObjectsElement.AppendChild(signatureElement);
						}

						XmlElement releaseElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Release);
						if (releaseElement != null)
						{
							releaseElement.InnerText = aModuleInfo.DbReleaseNumber.ToString();
							moduleDatabaseObjectsElement.AppendChild(releaseElement);
						}

						tablesElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Tables);
						moduleDatabaseObjectsElement.AppendChild(tablesElement);
					}
				}

				XmlDocument moduleAddOnDatabaseObjectsDocument = null;
				XmlElement	additionalColumnsElement = null;
				string addOnDatabaseObjectsFilename = moduleObjectsPath + 
					Path.DirectorySeparatorChar	+
					NameSolverStrings.AddOnDatabaseObjectsXml;

				if (aModuleInfo.HasExtraAddedColumns)
				{
					behaviourToApply = IsCodeFileToWrite(addOnDatabaseObjectsFilename);
			
					if (behaviourToApply == WritingCodeBehaviour.Abend)
						return false;
						
					if (behaviourToApply != WritingCodeBehaviour.Skip)
					{
						moduleAddOnDatabaseObjectsDocument = new XmlDocument();
					
						XmlDeclaration moduleAddOnDatabaseObjectsDeclaration = moduleAddOnDatabaseObjectsDocument.CreateXmlDeclaration(ModuleDatabaseObjectsXmlVersion, ModuleDatabaseObjectsXmlEncoding, null);
						if (moduleAddOnDatabaseObjectsDeclaration != null)
							moduleAddOnDatabaseObjectsDocument.AppendChild(moduleAddOnDatabaseObjectsDeclaration);
			
						XmlElement moduleAddOnDatabaseObjectsElement = moduleAddOnDatabaseObjectsDocument.CreateElement(Generics.AddOnDatabaseObjectsRootTag);
						if (moduleAddOnDatabaseObjectsElement != null)
						{
							moduleAddOnDatabaseObjectsDocument.AppendChild(moduleAddOnDatabaseObjectsElement);
					
							additionalColumnsElement = moduleAddOnDatabaseObjectsDocument.CreateElement(AddOnDatabaseObjectsXML.Element.AdditionalColumns);
							moduleAddOnDatabaseObjectsElement.AppendChild(additionalColumnsElement);
						}
					}
				}

				int scriptStepCount = 0;
				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.TablesCount > 0)
					{
						foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
						{
							if (aTableInfo.CreationDbReleaseNumber > aModuleInfo.DbReleaseNumber)
								continue;

							if (!GenerateTableCreationScripts(aTableInfo, aPathFinder))
								return false;

							scriptStepCount++;

							if (createInfoDocument != null && level1Element != null)
							{
								XmlElement stepElement = createInfoDocument.CreateElement(Generics.XmlTagDbInfoStep);
								if (stepElement != null)
								{
									stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepNumstep, scriptStepCount.ToString());
									stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepScript, aTableInfo.Name + Generics.DatabaseScriptExtension);

									// Se la tabella viene usata in relazione a tabelle contenute in altri moduli o
									// addirittura referenziate (e quindi definite da altre applicazioni) è corretto
									// specificare anche la dipendenza da esse!
									if (applicationInfo != null && applicationInfo.ModulesCount > 0)
									{
										WizardDBTInfoCollection dbts = applicationInfo.GetDBTsReferredToTable(aTableInfo);
										if (dbts != null && dbts.Count > 0)
										{
											WizardModuleInfoCollection dependenciesModules = new WizardModuleInfoCollection();
											
											foreach (WizardDBTInfo aDBTInfo in dbts)
											{
												if (!aDBTInfo.IsSlave && !aDBTInfo.IsSlaveBuffered)
													continue;

												WizardTableInfo masterTableInfo = aDBTInfo.GetRelatedTableInfo();
												if 
													(
													masterTableInfo == null || 
													masterTableInfo.Library == null ||
													masterTableInfo.Library.Module == null ||
													masterTableInfo.Library.Module.Application == null ||
													(String.Compare(masterTableInfo.Library.Module.Application.Name, aModuleInfo.Application.Name) == 0 && 
													String.Compare(masterTableInfo.Library.Module.Name, aModuleInfo.Name) == 0)
													)
													continue;

												if (dependenciesModules != null && dependenciesModules.Count > 0)
												{
													WizardModuleInfo insertedModule = dependenciesModules.GetModuleInfoByName(masterTableInfo.Library.Module.Name);
													if (insertedModule != null && String.Compare(masterTableInfo.Library.Module.Application.Name, insertedModule.Application.Name) == 0)
														continue;
												}

												dependenciesModules.Add(masterTableInfo.Library.Module);
											}
											if (dependenciesModules.Count > 0)
											{
												foreach(WizardModuleInfo aDependencyModuleInfo in dependenciesModules)
												{
													XmlElement dependencyElement = createInfoDocument.CreateElement(Generics.XmlTagDbInfoDependency);

													dependencyElement.SetAttribute(Generics.XmlAttributeDbInfoApplication, aDependencyModuleInfo.Application.Name);
													dependencyElement.SetAttribute(Generics.XmlAttributeDbInfoModule, aDependencyModuleInfo.Name);

													stepElement.AppendChild(dependencyElement);
												}
											}
										}
									}

									level1Element.AppendChild(stepElement);
								}
							}
							if (moduleDatabaseObjectsDocument != null && tablesElement != null)
							{
								XmlElement tableElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Table);
								if (tableElement != null)
								{
									tableElement.SetAttribute(DataBaseObjectsXML.Attribute.Namespace, aLibraryInfo.GetNameSpace() + "." + aTableInfo.Name);
									tablesElement.AppendChild(tableElement);
									XmlElement tableCreateElement = moduleDatabaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Create);
									if (tableCreateElement != null)
									{
										tableCreateElement.SetAttribute(DataBaseObjectsXML.Attribute.Release, aTableInfo.CreationDbReleaseNumber.ToString());
										tableCreateElement.SetAttribute(DataBaseObjectsXML.Attribute.Createstep, scriptStepCount.ToString());
										tableElement.AppendChild(tableCreateElement);
									}
								}
							}
						}
					}

					if (aLibraryInfo.ExtraAddedColumnsCount > 0)
					{
						foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in aLibraryInfo.ExtraAddedColumnsInfo)
						{
							if (string.IsNullOrEmpty(aExtraAddedColumnsInfo.TableNameSpace) ||
								aExtraAddedColumnsInfo.CreationDbReleaseNumber > aModuleInfo.DbReleaseNumber
								)
								continue;

							NameSpace tableNameSpace = new NameSpace(aExtraAddedColumnsInfo.TableNameSpace, NameSpaceObjectType.Table);
							if (!tableNameSpace.IsValid())
								continue;

							if (!GenerateAlterTableScripts(aExtraAddedColumnsInfo, aPathFinder))
								return false;

							scriptStepCount++;

							if (createInfoDocument != null && level1Element != null)
							{
								XmlElement stepElement = createInfoDocument.CreateElement(Generics.XmlTagDbInfoStep);
								if (stepElement != null)
								{
									stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepNumstep, scriptStepCount.ToString());
									stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepScript, "Alter" + aExtraAddedColumnsInfo.TableName + Generics.DatabaseScriptExtension);

									if 
										(
										(aModuleInfo.Application != null && String.Compare(aModuleInfo.Application.Name, tableNameSpace.Application) != 0) ||
										String.Compare(aModuleInfo.Name, tableNameSpace.Module) != 0
										)
									{
										XmlElement dependencyElement = createInfoDocument.CreateElement(Generics.XmlTagDbInfoDependency);
										if (dependencyElement != null)
										{
											dependencyElement.SetAttribute(Generics.XmlAttributeDbInfoApplication, tableNameSpace.Application);
											dependencyElement.SetAttribute(Generics.XmlAttributeDbInfoModule, tableNameSpace.Module);

											stepElement.AppendChild(dependencyElement);
										}
									}
									
									level1Element.AppendChild(stepElement);
								}
							}
							if (moduleAddOnDatabaseObjectsDocument != null && additionalColumnsElement != null)
							{
								XmlElement tableElement = moduleAddOnDatabaseObjectsDocument.CreateElement(AddOnDatabaseObjectsXML.Element.Table);
								if (tableElement != null)
								{
									tableElement.SetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace, aExtraAddedColumnsInfo.TableNameSpace);
									additionalColumnsElement.AppendChild(tableElement);
									
									XmlElement alterTableElement = moduleAddOnDatabaseObjectsDocument.CreateElement(AddOnDatabaseObjectsXML.Element.AlterTable);
									if (alterTableElement != null)
									{
										alterTableElement.SetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace, aLibraryInfo.GetNameSpace());
										alterTableElement.SetAttribute(AddOnDatabaseObjectsXML.Attribute.Release, aExtraAddedColumnsInfo.CreationDbReleaseNumber.ToString());
										alterTableElement.SetAttribute(AddOnDatabaseObjectsXML.Attribute.Createstep, scriptStepCount.ToString());
										tableElement.AppendChild(alterTableElement);
									}
								}
							}
						}
					}
				}

				if (createInfoDocument != null)
					createInfoDocument.Save(createInfoFilename);

				if (moduleDatabaseObjectsDocument != null)
					moduleDatabaseObjectsDocument.Save(databaseObjectsFilename);

				if (moduleAddOnDatabaseObjectsDocument != null)
					moduleAddOnDatabaseObjectsDocument.Save(addOnDatabaseObjectsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleDatabaseCreationScripts:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		private bool GenerateTableCreationScripts(WizardTableInfo aTableInfo, IBasePathFinder aPathFinder)
		{
			return 
				(
				aTableInfo != null &&
				aTableInfo.Library != null &&
				aTableInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateSQlServerTableCreationScript(aTableInfo, aPathFinder) &&
				GenerateOracleTableCreationScript(aTableInfo, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		private bool GenerateSQlServerTableCreationScript(WizardTableInfo aTableInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null ||
				string.IsNullOrEmpty(aTableInfo.Name) || aTableInfo.Library == null || aTableInfo.Library.Module == null)
				return false;

			if (!GenerateDatabaseScripts)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aTableInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.CreateScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.SQLServerScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return GenerateTableCreationScript(aTableInfo, scriptFilename, DBMSType.SQLSERVER);
		}

		//---------------------------------------------------------------------------
		public bool GenerateTableCreationScript(WizardTableInfo aTableInfo, string scriptFilename, DBMSType dbType)
		{
			// Il file non esiste oppure lo posso sovrascrivere!
			WizardTableCodeTemplateParser codeParser = new WizardTableCodeTemplateParser(aTableInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleCreateScript.tmpl";
					break;
			}

			return codeParser.WriteFileFromTemplate(tmpl, scriptFilename, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateOracleTableCreationScript(WizardTableInfo aTableInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null ||
				string.IsNullOrEmpty(aTableInfo.Name) || aTableInfo.Library == null || aTableInfo.Library.Module == null)
				return false;

			if (!GenerateDatabaseScripts)
				return true;
			
			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aTableInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.CreateScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.OracleScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return GenerateTableCreationScript(aTableInfo, scriptFilename, DBMSType.ORACLE);
		}

		//---------------------------------------------------------------------
		private bool GenerateAlterTableScripts(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, IBasePathFinder aPathFinder)
		{
			return 
				(
				aExtraAddedColumnsInfo != null &&
				aExtraAddedColumnsInfo.Library != null &&
				aExtraAddedColumnsInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateSQlServerAlterTableScript(aExtraAddedColumnsInfo, aPathFinder) &&
				GenerateOracleAlterTableScript(aExtraAddedColumnsInfo, aPathFinder)
				);
		}
		
        //---------------------------------------------------------------------
        public bool GenerateAlterTableScripts(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, string scriptFilename, DBMSType dbType)
        {
            return GenerateAlterTableScripts(aExtraAddedColumnsInfo, null, scriptFilename, dbType);
        }

		//---------------------------------------------------------------------
        public bool GenerateAlterTableScripts(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, TableUpdate currentTablesUpdate, string scriptFilename, DBMSType dbType)
		{	
			// Il file non esiste oppure lo posso sovrascrivere!
            WizardAdditionalColumnsCodeTemplateParser codeParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, dbType);
            codeParser.CurrentColumnUpdate = currentTablesUpdate;
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerAdditionalColumnsScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleAdditionalColumnsScript.tmpl";
					break;
			}
			return codeParser.WriteFileFromTemplate(tmpl, scriptFilename, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateSQlServerAlterTableScript(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aExtraAddedColumnsInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnsInfo.TableNameSpace) || 
				aExtraAddedColumnsInfo.Library == null || aExtraAddedColumnsInfo.Library.Module == null)
				return false;

			if (!GenerateDatabaseScripts)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aExtraAddedColumnsInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.CreateScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.SQLServerScriptsSubFolderName + 
				Path.DirectorySeparatorChar	+
				"Alter" + aExtraAddedColumnsInfo.TableName + 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return GenerateAlterTableScripts(aExtraAddedColumnsInfo, scriptFilename, DBMSType.SQLSERVER);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateOracleAlterTableScript(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aExtraAddedColumnsInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnsInfo.TableNameSpace) || 
				aExtraAddedColumnsInfo.Library == null || aExtraAddedColumnsInfo.Library.Module == null)
				return false;

			if (!GenerateDatabaseScripts)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aExtraAddedColumnsInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.CreateScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.OracleScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				"Alter" + aExtraAddedColumnsInfo.TableName + 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return GenerateAlterTableScripts(aExtraAddedColumnsInfo, scriptFilename, DBMSType.ORACLE);

		}

		//---------------------------------------------------------------------
		private bool GenerateAdditionalColumnsUpgradeScripts
			(
			WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			uint aDbReleaseNumber,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aExtraAddedColumnsInfo != null &&
				aExtraAddedColumnsInfo.Library != null &&
				aExtraAddedColumnsInfo.Library.Module != null &&
				aPathFinder != null &&
				aDbReleaseNumber > 1 &&
				aExtraAddedColumnsInfo.IsToUpgrade(aDbReleaseNumber) &&
				GenerateSQlServerAdditionalColumnsUpgradeScript(aExtraAddedColumnsInfo, aDbReleaseNumber, aPathFinder) &&
				GenerateOracleAdditionalColumnsUpgradeScript(aExtraAddedColumnsInfo, aDbReleaseNumber, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		private bool GenerateSQlServerAdditionalColumnsUpgradeScript
			(
			WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			uint aDbReleaseNumber,
			IBasePathFinder aPathFinder
			)
		{
			if (aPathFinder == null || applicationInfo == null || aExtraAddedColumnsInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnsInfo.TableNameSpace) || aExtraAddedColumnsInfo.Library == null || 
				aExtraAddedColumnsInfo.Library.Module == null || aDbReleaseNumber <= 1)
				return false;

			if (!GenerateDatabaseScripts)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aExtraAddedColumnsInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.UpgradeScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.SQLServerScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				String.Format(Generics.UpgradeScriptsReleaseFolderFormat, aDbReleaseNumber.ToString()) + 
				Path.DirectorySeparatorChar	+
				"Alter" + aExtraAddedColumnsInfo.TableName + 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			WizardAdditionalColumnsHistoryStepCodeTemplateParser codeParser = new WizardAdditionalColumnsHistoryStepCodeTemplateParser(aExtraAddedColumnsInfo, aDbReleaseNumber, DBMSType.SQLSERVER);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			return codeParser.WriteFileFromTemplate("Tables.SQLServerUpgradeScript.tmpl", scriptFilename, false);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateOracleAdditionalColumnsUpgradeScript
			(
			WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			uint aDbReleaseNumber,
			IBasePathFinder aPathFinder
			)
		{
			if (aPathFinder == null || applicationInfo == null || aExtraAddedColumnsInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnsInfo.TableNameSpace) || aExtraAddedColumnsInfo.Library == null ||
				aExtraAddedColumnsInfo.Library.Module == null || aDbReleaseNumber <= 1)
				return false;

			if (!GenerateDatabaseScripts)
				return true;
			
			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aExtraAddedColumnsInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.UpgradeScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.OracleScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				String.Format(Generics.UpgradeScriptsReleaseFolderFormat, aDbReleaseNumber.ToString()) + 
				Path.DirectorySeparatorChar	+
				"Alter" + aExtraAddedColumnsInfo.TableName + 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			WizardAdditionalColumnsHistoryStepCodeTemplateParser codeParser = new WizardAdditionalColumnsHistoryStepCodeTemplateParser(aExtraAddedColumnsInfo, aDbReleaseNumber, DBMSType.ORACLE);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			return codeParser.WriteFileFromTemplate("Tables.OracleUpgradeScript.tmpl", scriptFilename, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleDatabaseUpgradeScripts(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null)
				return false;

			if (!GenerateDatabaseScripts || (!aModuleInfo.HasTables && !aModuleInfo.HasExtraAddedColumns) || aModuleInfo.DbReleaseNumber <= 1)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string upgradeInfoFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.UpgradeScriptsSubFolderName + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.UpgradeInfoXml;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(upgradeInfoFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			XmlDocument upgradeInfoDocument = null;

			try
			{
				XmlElement upgradeInfoElement = null;
				if (behaviourToApply != WritingCodeBehaviour.Skip)
				{
					upgradeInfoDocument = new XmlDocument();
					XmlDeclaration upgradeInfoDeclaration = upgradeInfoDocument.CreateXmlDeclaration(DatabaseInfoXmlVersion, DatabaseInfoXmlEncoding, null);
					if (upgradeInfoDeclaration != null)
						upgradeInfoDocument.AppendChild(upgradeInfoDeclaration);
					
					upgradeInfoElement = upgradeInfoDocument.CreateElement(Generics.XmlTagDbUpgradeInfo);
					if (upgradeInfoElement != null)
					{
						upgradeInfoDocument.AppendChild(upgradeInfoElement);
						
						XmlElement moduleInfoElement = upgradeInfoDocument.CreateElement(Generics.XmlTagDbInfoModuleinfo);
						if (moduleInfoElement != null)
						{
							moduleInfoElement.SetAttribute(Generics.XmlAttributeDbInfoModuleName, aModuleInfo.Name);
							upgradeInfoElement.AppendChild(moduleInfoElement);
						}
					}
				}
				
				// Per ogni scatto di database release del modulo devo generare i relativi file di upgrade
				for (uint aDbReleaseNumber = 2; aDbReleaseNumber <= aModuleInfo.DbReleaseNumber; aDbReleaseNumber++)
				{
					if (!aModuleInfo.HasTablesToUpgrade(aDbReleaseNumber))
						continue;
					
					XmlElement	level1Element = null;

					if (upgradeInfoElement != null)
					{
						XmlElement dbReleaseInfoElement = upgradeInfoDocument.CreateElement(Generics.XmlTagDbInfoDbRelease);
						if (dbReleaseInfoElement != null)
						{
							dbReleaseInfoElement.SetAttribute(Generics.XmlAttributeDbInfoDbReleaseNumber, aDbReleaseNumber.ToString());
							upgradeInfoElement.AppendChild(dbReleaseInfoElement);
						}

						level1Element = upgradeInfoDocument.CreateElement(Generics.XmlTagDbInfoLevel1);
						if (level1Element != null)
							dbReleaseInfoElement.AppendChild(level1Element);
					}

					int tableScriptCount = 0;
					
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo.TablesCount > 0)
						{
							foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
							{
								if (!aTableInfo.IsToUpgrade(aDbReleaseNumber))
									continue;
					
								if (!GenerateTableUpgradeScripts(aTableInfo, aDbReleaseNumber, aPathFinder))
									return false;

								tableScriptCount++;
							
								if (level1Element != null)
								{
									XmlElement stepElement = upgradeInfoDocument.CreateElement(Generics.XmlTagDbInfoStep);
									if (stepElement != null)
									{
										stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepNumstep, tableScriptCount.ToString());
										stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepScript, aTableInfo.Name + Generics.DatabaseScriptExtension);

										level1Element.AppendChild(stepElement);
									}
								}
							}
						}
					
						if (aLibraryInfo.ExtraAddedColumnsCount > 0)
						{
							foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in aLibraryInfo.ExtraAddedColumnsInfo)
							{
								if (!aExtraAddedColumnsInfo.IsToUpgrade(aDbReleaseNumber))
									continue;
					
								if (!GenerateAdditionalColumnsUpgradeScripts(aExtraAddedColumnsInfo, aDbReleaseNumber, aPathFinder))
									return false;

								tableScriptCount++;
							
								if (level1Element != null)
								{
									XmlElement stepElement = upgradeInfoDocument.CreateElement(Generics.XmlTagDbInfoStep);
									if (stepElement != null)
									{
										stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepNumstep, tableScriptCount.ToString());
										stepElement.SetAttribute(Generics.XmlAttributeDbInfoStepScript, "Alter" + aExtraAddedColumnsInfo.TableName + Generics.DatabaseScriptExtension);

										level1Element.AppendChild(stepElement);
									}
								}
							}
						}
					}
				
					if (upgradeInfoDocument != null)
						upgradeInfoDocument.Save(upgradeInfoFilename);
				}

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleDatabaseUpgradeScripts:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		private bool GenerateTableUpgradeScripts(WizardTableInfo aTableInfo, uint aDbReleaseNumber, IBasePathFinder aPathFinder)
		{
			return 
				(
				aTableInfo != null &&
				aTableInfo.Library != null &&
				aTableInfo.Library.Module != null &&
				aPathFinder != null &&
				aDbReleaseNumber > 1 &&
				aTableInfo.IsToUpgrade(aDbReleaseNumber) &&
				GenerateSQlServerTableUpgradeScript(aTableInfo, aDbReleaseNumber, aPathFinder) &&
				GenerateOracleTableUpgradeScript(aTableInfo, aDbReleaseNumber, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		private bool GenerateSQlServerTableUpgradeScript(WizardTableInfo aTableInfo, uint aDbReleaseNumber, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null || string.IsNullOrEmpty(aTableInfo.Name) || 
				aTableInfo.Library == null || aTableInfo.Library.Module == null || aDbReleaseNumber <= 1)
				return false;

			if (!GenerateDatabaseScripts)
				return true;

			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aTableInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.UpgradeScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.SQLServerScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				String.Format(Generics.UpgradeScriptsReleaseFolderFormat, aDbReleaseNumber.ToString()) + 
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			WizardTableHistoryStepCodeTemplateParser codeParser = new WizardTableHistoryStepCodeTemplateParser(aTableInfo, aDbReleaseNumber, DBMSType.SQLSERVER);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			return codeParser.WriteFileFromTemplate("Tables.SQLServerUpgradeScript.tmpl", scriptFilename, false);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateOracleTableUpgradeScript(WizardTableInfo aTableInfo, uint aDbReleaseNumber, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null || string.IsNullOrEmpty(aTableInfo.Name) || 
				aTableInfo.Library == null || aTableInfo.Library.Module == null || aDbReleaseNumber <= 1)
				return false;

			if (!GenerateDatabaseScripts)
				return true;
			
			string databaseScriptsPath = GetStandardDatabaseScriptsPath(aPathFinder, aTableInfo.Library.Module);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return false;

			string scriptFilename = databaseScriptsPath + 
				Path.DirectorySeparatorChar	+
				Generics.UpgradeScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				Generics.OracleScriptsSubFolderName	+ 
				Path.DirectorySeparatorChar	+
				String.Format(Generics.UpgradeScriptsReleaseFolderFormat, aDbReleaseNumber.ToString()) + 
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				Generics.DatabaseScriptExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(scriptFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			WizardTableHistoryStepCodeTemplateParser codeParser = new WizardTableHistoryStepCodeTemplateParser(aTableInfo, aDbReleaseNumber, DBMSType.ORACLE);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			return codeParser.WriteFileFromTemplate("Tables.OracleUpgradeScript.tmpl", scriptFilename, false);
		}

		///<summary>
		/// GenerateViewCreationScript
		/// Genera lo script SQL per la creazione di una View
		///</summary>
		//---------------------------------------------------------------------------
		public bool GenerateViewCreationScript(SqlView aViewInfo, string scriptFilename, DBMSType dbType)
		{
			// Il file non esiste oppure lo posso sovrascrivere!
			WizardViewCodeTemplateParser codeParser = new WizardViewCodeTemplateParser(aViewInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerViewCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleViewCreateScript.tmpl";
					break;
			}

			return codeParser.WriteFileFromTemplate(tmpl, scriptFilename, false);
		}

		///<summary>
		/// GenerateProcedureCreationScript
		/// Genera lo script SQL per la creazione di una Procedure
		///</summary>
		//---------------------------------------------------------------------------
		public bool GenerateProcedureCreationScript(SqlProcedure aProcedureInfo, string scriptFilename, DBMSType dbType)
		{
			// Il file non esiste oppure lo posso sovrascrivere!
			WizardProcedureCodeTemplateParser codeParser = new WizardProcedureCodeTemplateParser(aProcedureInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerProcedureCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleProcedureCreateScript.tmpl";
					break;
			}

			return codeParser.WriteFileFromTemplate(tmpl, scriptFilename, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleObjectsFiles(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			return 
				GenerateModuleDocumentObjectsFile(aModuleInfo, aPathFinder) &&
				GenerateModuleClientDocumentObjectsFile(aModuleInfo, aPathFinder) &&
				GenerateModuleEnumsFile(aModuleInfo, aPathFinder) &&
				GenerateModuleEventHandlerObjectsFile(aModuleInfo, aPathFinder);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateModuleDocumentObjectsFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null )
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;

			// Creo il Path e i file name
			string moduleObjectsPath = GetStandardModuleObjectsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleObjectsPath))
				return false;

			string moduleDocumentObjectsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				Generics.ModuleDocumentObjectsFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(moduleDocumentObjectsFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument moduleDocumentObjectsDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = moduleDocumentObjectsDocument.CreateXmlDeclaration(DocumentObjectsXmlVersion, DocumentObjectsXmlEncoding, null);
				if (configDeclaration != null)
					moduleDocumentObjectsDocument.AppendChild(configDeclaration);
			
				//ROOT ovvero <DocumentObjects>
				XmlElement documentObjectsElement = moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_DOCUMENTOBJECTS_TAG);
				if (documentObjectsElement == null)
					return false;
				moduleDocumentObjectsDocument.AppendChild(documentObjectsElement);

				//<Documents>
				XmlElement documentsElement = moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_DOCUMENTS_TAG);
				if (documentsElement == null)
					return false;
			
				documentObjectsElement.AppendChild(documentsElement);

				// Controllo che ci siano librerie e che esse contengano effettivamente dei documenti
				if (aModuleInfo.LibrariesCount > 0 && aModuleInfo.HasDocuments)
				{
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo.DocumentsCount == 0)
							continue;
	
						foreach (WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
						{
							//<Document>
							XmlElement documentElement =  moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_DOCUMENT_TAG);
							if (documentElement == null)
								return false;

							//Setto l'attributo namespace
							documentElement.SetAttribute(XML_MOD_DOCOBJS_NAMESPACE_ATTRIBUTE, aDocumentInfo.GetNameSpace());
							//Setto l'attributo localize
							documentElement.SetAttribute(XML_MOD_DOCOBJS_LOCALIZE_ATTRIBUTE, aDocumentInfo.Title);
							//Setto l'attributo classhierarchy
							documentElement.SetAttribute(XML_MOD_DOCOBJS_CLASSHIERARCHY_ATTRIBUTE, aDocumentInfo.ClassName);
					
							documentsElement.AppendChild(documentElement);
					
							//Aggiungo il TAG <InterfaceClass> anche se vuoto
							XmlElement interfaceClassElement =  moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_NAME_INTERFACECLASS_TAG);
							if (interfaceClassElement != null)
								documentElement.AppendChild(interfaceClassElement);
	
							//Aggiungo il TAG <ViewModes> 
							XmlElement viewModesElement =  moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_VIEWMODES_TAG);
							if (viewModesElement != null)
								documentElement.AppendChild(viewModesElement);
							else
								return false;

							//Aggiungo il TAG <Mode> con solo il Default
							XmlElement modeElement =  moduleDocumentObjectsDocument.CreateElement(XML_MOD_DOCOBJS_MODE_TAG);
							if (viewModesElement != null)
							{
								modeElement.SetAttribute(XML_MOD_DOCOBJS_NAME_ATTRIBUTE, DefaultViewModeValue);
								viewModesElement.AppendChild(modeElement);
							}
							else
								return false;

							//@@TODO(da vedere se ha senso creare in automatico un report per ciascun documento)
							//if(!GenerateDocumentReportsListFile(aDocumentInfo, aPathFinder))
							//	return false;
						}
					}
				}

				moduleDocumentObjectsDocument.Save(moduleDocumentObjectsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleDocumentObjectsFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleClientDocumentObjectsFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;

			// Creo il Path e i file name
			string moduleObjectsPath = GetStandardModuleObjectsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleObjectsPath))
				return false;

			string moduleClientDocumentObjectsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				Generics.ModuleClientDocumentObjectsFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(moduleClientDocumentObjectsFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument clientDocumentObjectsDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = clientDocumentObjectsDocument.CreateXmlDeclaration(ClientDocumentObjectsXmlVersion, ClientDocumentObjectsXmlEncoding, null);
				if (configDeclaration != null)
					clientDocumentObjectsDocument.AppendChild(configDeclaration);
			
				//ROOT ovvero <ClientDocumentObjects>
				XmlElement clientDocumentObjectsElement = clientDocumentObjectsDocument.CreateElement(XML_MOD_CLIENTDOCS_CLIENTDOCUMENTOBJECTS_TAG);
				if (clientDocumentObjectsElement == null)
					return false;
				clientDocumentObjectsDocument.AppendChild(clientDocumentObjectsElement);

				//<ClientDocuments>
				XmlElement clientDocumentsElement = clientDocumentObjectsDocument.CreateElement(XML_MOD_CLIENTDOCS_CLIENTDOCUMENTS_TAG);
				if (clientDocumentsElement == null)
					return false;
			
				clientDocumentObjectsElement.AppendChild(clientDocumentsElement);

				// Controllo che ci siano librerie e che esse contengano effettivamente dei client document
				if (aModuleInfo.LibrariesCount > 0 && aModuleInfo.HasClientDocuments)
				{
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo.ClientDocumentsCount == 0)
							continue;
	
						foreach (WizardClientDocumentInfo aClientDocumentInfo in aLibraryInfo.ClientDocumentsInfo)
						{
							if (aClientDocumentInfo.ServerDocumentInfo == null)
								continue;

							// Se in qualche passaggio predcedente di questo ciclo sui client document è già stato 
							// inserito un elemento relativo al corrente server document prendo quello, altrimenti 
							// lo creo
							XmlNode serverDocumentNode = null;
							if (aClientDocumentInfo.AttachToFamily)
								serverDocumentNode = clientDocumentsElement.SelectSingleNode("child::" + XML_MOD_CLIENTDOCS_SERVERDOCUMENT_TAG + "[@" + XML_MOD_CLIENTDOCS_TYPE_ATTRIBUTE + "='" + XML_MOD_CLIENTDOCS_FAMILY_ATTRIBUTE_VALUE + "' and @" + XML_MOD_CLIENTDOCS_CLASS_ATTRIBUTE + "='" + aClientDocumentInfo.FamilyToAttachClassName + "']");
							else
								serverDocumentNode = clientDocumentsElement.SelectSingleNode("child::" + XML_MOD_CLIENTDOCS_SERVERDOCUMENT_TAG + "[@" + XML_MOD_CLIENTDOCS_NAMESPACE_ATTRIBUTE + "='" + aClientDocumentInfo.ServerDocumentInfo.GetNameSpace() + "']");

							XmlElement serverDocumentElement = null;
							if (serverDocumentNode == null || !(serverDocumentNode is XmlElement))
							{
								serverDocumentElement = clientDocumentObjectsDocument.CreateElement(XML_MOD_CLIENTDOCS_SERVERDOCUMENT_TAG);
								if (serverDocumentElement == null)
									return false;

								if (aClientDocumentInfo.AttachToFamily)
								{
									serverDocumentElement.SetAttribute(XML_MOD_CLIENTDOCS_TYPE_ATTRIBUTE, XML_MOD_CLIENTDOCS_FAMILY_ATTRIBUTE_VALUE);
									serverDocumentElement.SetAttribute(XML_MOD_CLIENTDOCS_CLASS_ATTRIBUTE, aClientDocumentInfo.FamilyToAttachClassName);
								}
								else
									serverDocumentElement.SetAttribute(XML_MOD_CLIENTDOCS_NAMESPACE_ATTRIBUTE, aClientDocumentInfo.ServerDocumentInfo.GetNameSpace());

								clientDocumentsElement.AppendChild(serverDocumentElement);

							}
							else
								serverDocumentElement = (XmlElement)serverDocumentNode;

							XmlElement clientDocumentElement =  clientDocumentObjectsDocument.CreateElement(XML_MOD_CLIENTDOCS_CLIENTDOCUMENT_TAG);
							if (clientDocumentElement == null)
								return false;

							clientDocumentElement.SetAttribute(XML_MOD_CLIENTDOCS_NAMESPACE_ATTRIBUTE, aClientDocumentInfo.GetNameSpace());
							clientDocumentElement.SetAttribute(XML_MOD_CLIENTDOCS_LOCALIZE_ATTRIBUTE, aClientDocumentInfo.Title);

							serverDocumentElement.AppendChild(clientDocumentElement);
						}
					}
				}

				clientDocumentObjectsDocument.Save(moduleClientDocumentObjectsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleClientDocumentObjectsFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		//@@LARA
		//@@ Ma quale report???? Il Wizard mica crea un report per documento!!!(Carlotta)
		private bool GenerateDocumentReportsListFile(WizardDocumentInfo aDocumentInfo, IBasePathFinder aPathFinder)
		{
			if (aDocumentInfo == null || aPathFinder == null || string.IsNullOrEmpty(aDocumentInfo.Name) || aDocumentInfo.Library == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;
			
			string documentDescriptionPath = GetStandardDocumentDescriptionPath(aPathFinder, aDocumentInfo);

			if (string.IsNullOrEmpty(documentDescriptionPath))
				return false;

			string reportFilename = documentDescriptionPath + 
				Path.DirectorySeparatorChar +
				Generics.ReportsListFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(reportFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument reportDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = reportDocument.CreateXmlDeclaration(ReportsXmlVersion, ReportsXmlEncoding, null);
				if (configDeclaration != null)
					reportDocument.AppendChild(configDeclaration);

				//ROOT ovvero <ReportObjects>
				XmlElement reportObjectsElement = reportDocument.CreateElement(XML_REPORTS_REPORTOBJECTS_TAG);
				if (reportObjectsElement == null)
					return false;
				reportDocument.AppendChild(reportObjectsElement);

				//<Reports>
				XmlElement reportsElement = reportDocument.CreateElement(XML_REPORTS_REPORTS_TAG);
				if (reportsElement == null)
					return false;

				reportsElement.SetAttribute(XML_REPORTS_DEFAULTREPORT_ATTRIBUTE, aDocumentInfo.GetNameSpace());
				reportObjectsElement.AppendChild(reportsElement);

				//<Report>
				XmlElement reportElement = reportDocument.CreateElement(XML_REPORTS_REPORT_TAG);
				if (reportElement == null)
					return false;

				reportElement.SetAttribute(XML_REPORTS_NAMESPACE_ATTRIBUTE, aDocumentInfo.GetNameSpace());
				reportElement.SetAttribute(XML_REPORTS_LOCALIZE_ATTRIBUTE, aDocumentInfo.Title);
				reportsElement.AppendChild(reportElement);

				reportDocument.Save(reportFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateDocumentReportsListFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleEventHandlerObjectsFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;

			// Creo il Path e i file name
			string moduleObjectsPath = GetStandardModuleObjectsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleObjectsPath))
				return false;

			string moduleEventHandlerObjectsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.EventHandlerObjectsXml;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(moduleEventHandlerObjectsFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument moduleEventHandlerObjectsDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = moduleEventHandlerObjectsDocument.CreateXmlDeclaration(EventHandlerObjectsXmlVersion, EventHandlerObjectsXmlEncoding, null);
				if (configDeclaration != null)
					moduleEventHandlerObjectsDocument.AppendChild(configDeclaration);
			
				// Creazione della root ovvero <FunctionObjects>
				XmlElement functionObjectsElement = moduleEventHandlerObjectsDocument.CreateElement(XML_MOD_EVENT_HANDLER_OBJS_FUNCTIONOBJECTS_TAG);
				if (functionObjectsElement == null)
					return false;

				moduleEventHandlerObjectsDocument.AppendChild(functionObjectsElement);

				XmlElement functionsElement = moduleEventHandlerObjectsDocument.CreateElement(XML_MOD_EVENT_HANDLER_OBJS_FUNCTIONS_TAG);
				if (functionsElement == null)
					return false;

				functionObjectsElement.AppendChild(functionsElement);

				if (aModuleInfo.LibrariesCount > 0)
				{
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlElement applicationDateChangedFunctionElement = moduleEventHandlerObjectsDocument.CreateElement(XML_MOD_EVENT_HANDLER_OBJS_FUNCTION_TAG);
						if (applicationDateChangedFunctionElement == null)
							return false;
						
						applicationDateChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_NAMESPACE_ATTRIBUTE, aLibraryInfo.GetNameSpace() + ".ApplicationDateChanged");
						applicationDateChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_TYPE_ATTRIBUTE, "bool");
						applicationDateChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_LOCALIZE_ATTRIBUTE, ApplicationDateChangeDefaultText);
						
						functionsElement.AppendChild(applicationDateChangedFunctionElement);

						XmlElement onDSNChangedFunctionElement = moduleEventHandlerObjectsDocument.CreateElement(XML_MOD_EVENT_HANDLER_OBJS_FUNCTION_TAG);
						if (onDSNChangedFunctionElement == null)
							return false;
						
						onDSNChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_NAMESPACE_ATTRIBUTE, aLibraryInfo.GetNameSpace() + ".OnDSNChanged");
						onDSNChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_TYPE_ATTRIBUTE, "bool");
						onDSNChangedFunctionElement.SetAttribute(XML_MOD_EVENT_HANDLER_OBJS_LOCALIZE_ATTRIBUTE, CompanyConnectionChangeDefaultText);

						functionsElement.AppendChild(onDSNChangedFunctionElement);
					}
				}

				moduleEventHandlerObjectsDocument.Save(moduleEventHandlerObjectsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleEventHandlerObjectsFile:", exception.Message);
				return false;
			}
			
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateModuleEnumsFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;

			// Creo il Path e i file name
			string moduleObjectsPath = GetStandardModuleObjectsPath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleObjectsPath))
				return false;

			string moduleEnumsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.EnumsXml;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(moduleEnumsFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument moduleEnumsDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = moduleEnumsDocument.CreateXmlDeclaration(EnumsXmlVersion, EnumsXmlEncoding, "yes");
				if (configDeclaration != null)
					moduleEnumsDocument.AppendChild(configDeclaration);
			
				// Creazione della root ovvero <Emuns>
				XmlElement enumsElement = moduleEnumsDocument.CreateElement(XML_MOD_ENUMS_ENUMS_TAG);
				if (enumsElement == null)
					return false;
				moduleEnumsDocument.AppendChild(enumsElement);

				//Ora inizio a inserire gli enumerativi
				if (aModuleInfo.EnumsCount > 0)
				{
					foreach(WizardEnumInfo aEnumInfo in aModuleInfo.EnumsInfo)
					{
						//<Document>
						XmlElement tagElement =  moduleEnumsDocument.CreateElement(XML_MOD_ENUMS_TAG);
						if (tagElement == null)
							return false;
					
						tagElement.SetAttribute(XML_MOD_ENUMS_NAME_ATTRIBUTE, aEnumInfo.Name);

						tagElement.SetAttribute(XML_MOD_ENUMS_VALUE_ATTRIBUTE, aEnumInfo.Value.ToString());
					
						WizardEnumItemInfo defaultItem = aEnumInfo.DefaultItem;
						if (defaultItem != null && aEnumInfo.ItemsInfo.IndexOf(defaultItem) != 0)
							tagElement.SetAttribute(XML_MOD_ENUMS_DEFAULTVALUE_ATTRIBUTE, defaultItem.Value.ToString());
					
						enumsElement.AppendChild(tagElement);
					
						//Loop sugli item dell'enumerativo
						foreach(WizardEnumItemInfo item in aEnumInfo.ItemsInfo)
						{
							//Item
							XmlElement itemElement =  moduleEnumsDocument.CreateElement(XML_MOD_ENUMS_ITEM_TAG);
							if (itemElement == null)
								return false;

							itemElement.SetAttribute(XML_MOD_ENUMS_NAME_ATTRIBUTE, item.Name.ToString());
							itemElement.SetAttribute(XML_MOD_ENUMS_VALUE_ATTRIBUTE, item.Value.ToString());
							itemElement.SetAttribute(XML_MOD_ENUMS_STORED_ATTRIBUTE, aEnumInfo.GetItemStoredValue(item).ToString());
										
							tagElement.AppendChild(itemElement);
						}
					}
				}

				moduleEnumsDocument.Save(moduleEnumsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleEnumsFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleMenu(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			return GenerateModuleMenuFile(aModuleInfo, aPathFinder) &&
					GenerateModuleMenuImage(aModuleInfo, aPathFinder);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateModuleMenuFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			if (!GenerateMenuFiles)
				return true;

			string moduleMenuPath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleMenuPath))
				return false;

			moduleMenuPath += Path.DirectorySeparatorChar;
			moduleMenuPath += NameSolverStrings.Menu;

			string menuFilename = moduleMenuPath + 
				Path.DirectorySeparatorChar	+
				aModuleInfo.Name + 
				NameSolverStrings.MenuExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(menuFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			try
			{
				MenuXmlParser aMenuParser = new MenuXmlParser();

				string applicationTitle = applicationInfo.Title;
				if (string.IsNullOrEmpty(applicationTitle))
					applicationTitle = applicationInfo.Name;

				MenuXmlNode appMenuNode = aMenuParser.CreateApplicationNode(applicationInfo.Name, applicationTitle);
				if (appMenuNode == null)
				{
					if (codeWriteFailureEventHandler != null)
					{
						if (codeWriteFailureEventHandler(menuFilename, TBWizardProjectsStrings.WriteXmlMenuFileErrorMsg))
							return true;
					}
					return false;
				}
				appMenuNode.OriginalTitle = String.Empty;

				string moduleTitle = aModuleInfo.Title;
				if (string.IsNullOrEmpty(moduleTitle))
					moduleTitle = aModuleInfo.Name;
			
				MenuXmlNode groupNode = aMenuParser.CreateGroupNode(appMenuNode, applicationInfo.Name + "." + aModuleInfo.Name, moduleTitle);
			
				if (groupNode == null)
					return false;

				groupNode.OriginalTitle = String.Empty;

				if (aModuleInfo.LibrariesCount > 0)
				{
					foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						string title = aLibraryInfo.MenuTitle;
						if (string.IsNullOrEmpty(title))
							title = aLibraryInfo.Name;

						MenuXmlNode libraryNode = aMenuParser.CreateMenuNode(groupNode, title);
					
						if (libraryNode == null)
							return false;

						libraryNode.OriginalTitle = String.Empty;

						if (aLibraryInfo.DocumentsCount > 0)
						{
							//Creo un nodo per ogni documento definito nella libreria
							foreach(WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
							{
								MenuXmlNode documentNode = null;
								if (aDocumentInfo.DefaultType == WizardDocumentInfo.DocumentType.DataEntry)
								{
									documentNode = aMenuParser.CreateDocumentCommandNode
										(
										libraryNode, 
										!string.IsNullOrEmpty(aDocumentInfo.Title) ? aDocumentInfo.Title : aDocumentInfo.Name, 
										String.Empty, 
										aDocumentInfo.GetNameSpace(),
										String.Empty
										);
								}
								else if (aDocumentInfo.DefaultType == WizardDocumentInfo.DocumentType.Batch)
								{
									documentNode = aMenuParser.CreateBatchCommandNode
										(
										libraryNode, 
										!string.IsNullOrEmpty(aDocumentInfo.Title) ? aDocumentInfo.Title : aDocumentInfo.Name, 
										String.Empty, 
										aDocumentInfo.GetNameSpace(),
										String.Empty
										);
								}

								if (documentNode == null)
									continue;
								documentNode.OriginalTitle = String.Empty;
							}
						}

						if (aLibraryInfo.TablesCount > 0)
						{
							//Creo un nodo per ogni report relativo alle tabelle definite nella libreria
							foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
							{
								MenuXmlNode reportNode = aMenuParser.CreateReportCommandNode
									(
									libraryNode, 
									aTableInfo.Name, 
									String.Empty, 
									aModuleInfo.GetNameSpace() + "." + aTableInfo.Name,
									String.Empty
									);
								
								if (reportNode == null)
									continue;
								reportNode.OriginalTitle = String.Empty;
							}
						}
					}
				}

				aMenuParser.MenuXmlDoc.Save(menuFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateModuleMenuFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleMenuImage(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			if (!GenerateMenuFiles)
				return true;

			string moduleMenuImagePath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(moduleMenuImagePath))
				return false;

			moduleMenuImagePath += Path.DirectorySeparatorChar;
			moduleMenuImagePath += NameSolverStrings.Menu;

			string moduleMenuImageFilename = moduleMenuImagePath + 
				Path.DirectorySeparatorChar	+
				applicationInfo.Name + 
				"." +
				aModuleInfo.Name + 
				".gif";

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(moduleMenuImageFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			if (!Generics.CopyEmbeddedResourceToFile("Microarea.Library.TBWizardProjects.CodeTemplates.Modules.Application.Module.gif", moduleMenuImageFilename))
				return false;

			string applicationMenuImageFilename = moduleMenuImagePath + 
				Path.DirectorySeparatorChar	+
				applicationInfo.Name + 
				".gif";

			behaviourToApply = IsCodeFileToWrite(applicationMenuImageFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return Generics.CopyEmbeddedResourceToFile("Microarea.Library.TBWizardProjects.CodeTemplates.Application.Application.gif", applicationMenuImageFilename);
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleEnumsHeaderFile(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null || string.IsNullOrEmpty(aModuleInfo.Name) || applicationInfo == null)
				return false;

			return GenerateModuleSourceFile(aModuleInfo, aPathFinder, "ModuleEnums_h.tmpl", aModuleInfo.Name + "Enums" + Generics.CppHeaderExtension);
		}

		//---------------------------------------------------------------------------
		private bool GenerateModuleSourceFile
			(
			WizardModuleInfo aModuleInfo,
			IBasePathFinder aPathFinder,
			string templateResourceName,
			string codeFileName
			)
		{
			if (aPathFinder == null || string.IsNullOrEmpty(templateResourceName) || string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null || aModuleInfo == null || string.IsNullOrEmpty(aModuleInfo.Name))
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string modulePath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(modulePath))
				return false;

			string outputFileName = modulePath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardModuleCodeTemplateParser codeParser = new WizardModuleCodeTemplateParser(aModuleInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);

			return codeParser.WriteFileFromTemplate("Modules." + templateResourceName, outputFileName, saveInjectionPointsCode);
		}

		//---------------------------------------------------------------------------
		private bool GenerateLibrarySourceFile
			(
			WizardLibraryInfo aLibraryInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName,
			Encoding encoding
			)
		{
			if (aPathFinder == null || string.IsNullOrEmpty(templateResourceName) || string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null || aLibraryInfo == null || string.IsNullOrEmpty(aLibraryInfo.Name) || 
				aLibraryInfo.Module == null)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string libraryPath = GetStandardLibraryPath(aPathFinder, aLibraryInfo);

			string outputFileName = libraryPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardLibraryCodeTemplateParser codeParser = new WizardLibraryCodeTemplateParser(aLibraryInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);
			
			return codeParser.WriteFileFromTemplate("Libraries." + templateResourceName, outputFileName, encoding, saveInjectionPointsCode, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateLibrarySourceFile
			(
			WizardLibraryInfo		aLibraryInfo,
			IBasePathFinder aPathFinder,
			string					templateResourceName,
			string					codeFileName
			)
		{
			return GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, templateResourceName, codeFileName, null);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateLibraryProjectFile(WizardLibraryInfo aLibraryInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aLibraryInfo == null || 
				string.IsNullOrEmpty(aLibraryInfo.Name) || aLibraryInfo.Module == null)
				return false;

			if (!GenerateLibrariesSourceCode)
				return true;

			string libraryPath = GetStandardLibraryPath(aPathFinder, aLibraryInfo);

			string outputLibraryFileName = libraryPath + Path.DirectorySeparatorChar + aLibraryInfo.Name + Generics.CppProjectExtension;
            string outputFilterFileName = libraryPath + Path.DirectorySeparatorChar + aLibraryInfo.Name + Generics.CppProjectExtension + ".filters";

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputLibraryFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Prima genero, sulla base di un template, il file di progetto della libreria.
			// In esso vengono così inserite le informazioni generiche del progetto.
			WizardLibraryCodeTemplateParser libraryParser = new WizardLibraryCodeTemplateParser(aLibraryInfo);
			
			libraryParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (!libraryParser.WriteFileFromTemplate("Libraries.Library_vcxproj.tmpl", outputLibraryFileName, false, true))
				return false;

            WizardLibraryCodeTemplateParser filterParser = new WizardLibraryCodeTemplateParser(aLibraryInfo);

            filterParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

            if (!filterParser.WriteFileFromTemplate("Libraries.Library_vcxproj_filters.tmpl", outputFilterFileName, false, true))
                return false;

            try
			{
				XmlDocument libraryProject = new XmlDocument();

				libraryProject.Load(outputLibraryFileName);

				XmlElement root = libraryProject.DocumentElement;
				if (root == null)
					return false;

				// La dichiarazione XML del file di progetto la devo inserire a posteriori, cioè dopo averlo creato 
				// a partire da un file di template mediante WizardLibraryCodeTemplateParser, in quanto, utilizzando
				// quest'ultimo come Encoding "UTF-8", inserirebbe all'inizio del file i caratteri di BOM.
				// È per questo motivo che la dichiarazione XML non può stare nel file di template.
				XmlDeclaration projectDeclaration = libraryProject.CreateXmlDeclaration("1.0", "utf-8", null);
				if (projectDeclaration != null)
					libraryProject.InsertBefore(projectDeclaration, root);

                XmlNamespaceManager nsmVcxproj = new XmlNamespaceManager(libraryProject.NameTable);
                string prefix = libraryProject.DocumentElement.Prefix != string.Empty ? libraryProject.DocumentElement.Prefix : "ns";
                nsmVcxproj.AddNamespace(prefix, libraryProject.DocumentElement.NamespaceURI);

                if (aLibraryInfo.Dependencies != null && aLibraryInfo.Dependencies.Count > 0)
				{
                    XmlNode projectReferencesNode = root.SelectSingleNode(string.Format("{0}:ItemGroup[{0}:ProjectReference][1]", prefix), nsmVcxproj);
                    if (projectReferencesNode == null)
                        projectReferencesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));
                    else
                        projectReferencesNode.RemoveAll();

					foreach(WizardLibraryInfo aProjectDependency in aLibraryInfo.Dependencies)
					{
						if (aProjectDependency.IsReferenced)
							continue;

						XmlElement projectReferenceElement = libraryProject.CreateElement("ProjectReference", libraryProject.DocumentElement.NamespaceURI);
                        projectReferenceElement.SetAttribute("Include", Path.Combine(GetStandardLibraryPath(aPathFinder, aProjectDependency), aProjectDependency.Name) + Generics.CppProjectExtension);
                        XmlElement projectElement = libraryProject.CreateElement("Project", libraryProject.DocumentElement.NamespaceURI);
                        projectElement.InnerText = aProjectDependency.Guid.ToString("B").ToUpper();

                        projectReferenceElement.AppendChild(projectElement);

                        projectReferencesNode.AppendChild(projectReferenceElement);
					}
				}

                XmlNode projectSourceFilesNode = root.SelectSingleNode(string.Format("{0}:ItemGroup[{0}:ClCompile][1]",prefix), nsmVcxproj);
                XmlNode projectIncludeFilesNode = root.SelectSingleNode(string.Format("{0}:ItemGroup[{0}:ClInclude][1]", prefix), nsmVcxproj);
                XmlNode projectResourcesNode = root.SelectSingleNode(string.Format("{0}:ItemGroup[{0}:ResourceCompile][1]", prefix), nsmVcxproj);
                XmlNode projectResourcesHdrNode = root.SelectSingleNode(string.Format("{0}:ItemGroup[{0}:None][1]", prefix), nsmVcxproj);

				// Adesso aggiungo le informazioni relative ai vari oggetti contenuti nella libreria

				// --------------------------------------------------------------------------
				// Tabelle, DBT e documenti
				// --------------------------------------------------------------------------
                //	<ItemGroup>
                //		<ClCompile Include="nome_tabella.cpp" />
                //		<ClCompile Include="nome_DBT.cpp" />
                //		<ClCompile Include="nome_documento.cpp" />
                //	</ItemGroup>
                //	<ItemGroup>
                //		<ClInclude Include="nome_tabella.h" />
                //		<ClInclude Include="nome_DBT.h" />
                //		<ClInclude Include="nome_documento.h" />
                //	</ItemGroup>
                //	<ItemGroup>
                //		<ResourceCompile Include="nome_documento.rc">
                //          <ExcludedFromBuild Condition="'$(Configuration)|$(Platform)'=='Debug|Win32'">true</ExcludedFromBuild>
                //          <ExcludedFromBuild Condition="'$(Configuration)|$(Platform)'=='Release|Win32'">true</ExcludedFromBuild>
                //      </ResourceCompile>
                //	</ItemGroup>
                //  <ItemGroup>
                //    <None Include="nome_documento.hrc" />
                //  </ItemGroup>
                // --------------------------------------------------------------------------

                if (aLibraryInfo.TablesCount > 0)
				{
                    if (projectSourceFilesNode == null)
                        projectSourceFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectIncludeFilesNode == null)
                        projectIncludeFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

 					foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
					{
                        XmlElement fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
						fileElement.SetAttribute("Include",aTableInfo.ClassName + Generics.CppExtension);
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aTableInfo.ClassName + Generics.CppHeaderExtension);
                        projectIncludeFilesNode.AppendChild(fileElement);
					}
				}

                if (aLibraryInfo.DBTsCount > 0)
				{
                    if (projectSourceFilesNode == null)
                        projectSourceFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectIncludeFilesNode == null)
                        projectIncludeFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    foreach (WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
					{
                        XmlElement fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aDBTInfo.ClassName + Generics.CppExtension);
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aDBTInfo.ClassName + Generics.CppHeaderExtension);
                        projectIncludeFilesNode.AppendChild(fileElement);
					}
				}

                if (aLibraryInfo.DocumentsCount > 0)
				{
                    if (projectSourceFilesNode == null)
                        projectSourceFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectIncludeFilesNode == null)
                        projectIncludeFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectResourcesNode == null)
                        projectResourcesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectResourcesHdrNode == null)
                        projectResourcesHdrNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    foreach (WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
					{
                        XmlElement fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aDocumentInfo.ClassName + Generics.CppExtension);
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aDocumentInfo.ClassName + Generics.CppHeaderExtension);
                        projectIncludeFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", GetDocumentViewFileName(aDocumentInfo, Generics.CppExtension));
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", GetDocumentViewFileName(aDocumentInfo, Generics.CppHeaderExtension));
                        projectIncludeFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("None", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceHeaderExtension, true));
                        projectResourcesHdrNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ResourceCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceFileExtension, true));

                        XmlElement excludedFromBuildElement = libraryProject.CreateElement("ExcludedFromBuild", libraryProject.DocumentElement.NamespaceURI);
                        excludedFromBuildElement.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|Win32'");
                        excludedFromBuildElement.InnerText = "true";
						fileElement.AppendChild(excludedFromBuildElement);

                        excludedFromBuildElement = libraryProject.CreateElement("ExcludedFromBuild", libraryProject.DocumentElement.NamespaceURI);
                        excludedFromBuildElement.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Release|Win32'");
                        excludedFromBuildElement.InnerText = "true";
                        fileElement.AppendChild(excludedFromBuildElement);

                        projectResourcesNode.AppendChild(fileElement);
					}
				}

                if (aLibraryInfo.ClientDocumentsCount > 0)
				{
                    if (projectSourceFilesNode == null)
                        projectSourceFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectIncludeFilesNode == null)
                        projectIncludeFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectResourcesNode == null)
                        projectResourcesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectResourcesHdrNode == null)
                        projectResourcesHdrNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    foreach (WizardClientDocumentInfo aClientDocumentInfo in aLibraryInfo.ClientDocumentsInfo)
					{
                        XmlElement fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aClientDocumentInfo.ClassName + Generics.CppExtension);
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aClientDocumentInfo.ClassName + Generics.CppHeaderExtension);
                        projectIncludeFilesNode.AppendChild(fileElement);

						if (aClientDocumentInfo.IsInterfacePresent)
						{
                            fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                            fileElement.SetAttribute("Include", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppExtension));
                            projectSourceFilesNode.AppendChild(fileElement);

                            fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                            fileElement.SetAttribute("Include", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppHeaderExtension));
                            projectIncludeFilesNode.AppendChild(fileElement);

                            fileElement = libraryProject.CreateElement("None", libraryProject.DocumentElement.NamespaceURI);
                            fileElement.SetAttribute("Include", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppResourceHeaderExtension, true));
                            projectResourcesHdrNode.AppendChild(fileElement);

                            fileElement = libraryProject.CreateElement("ResourceCompile", libraryProject.DocumentElement.NamespaceURI);
                            fileElement.SetAttribute("Include", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppResourceFileExtension, true));

                            XmlElement excludedFromBuildElement = libraryProject.CreateElement("ExcludedFromBuild", libraryProject.DocumentElement.NamespaceURI);
                            excludedFromBuildElement.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|Win32'");
                            excludedFromBuildElement.InnerText = "true";
                            fileElement.AppendChild(excludedFromBuildElement);

                            excludedFromBuildElement = libraryProject.CreateElement("ExcludedFromBuild", libraryProject.DocumentElement.NamespaceURI);
                            excludedFromBuildElement.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Release|Win32'");
                            excludedFromBuildElement.InnerText = "true";
                            fileElement.AppendChild(excludedFromBuildElement);

                            projectResourcesNode.AppendChild(fileElement);
						}
					}
				}

                if (aLibraryInfo.ExtraAddedColumnsCount > 0)
				{
                    if (projectSourceFilesNode == null)
                        projectSourceFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    if (projectIncludeFilesNode == null)
                        projectIncludeFilesNode = root.AppendChild(libraryProject.CreateElement("ItemGroup", libraryProject.DocumentElement.NamespaceURI));

                    foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in aLibraryInfo.ExtraAddedColumnsInfo)
					{
                        XmlElement fileElement = libraryProject.CreateElement("ClCompile", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aExtraAddedColumnsInfo.ClassName + Generics.CppExtension);
                        projectSourceFilesNode.AppendChild(fileElement);

                        fileElement = libraryProject.CreateElement("ClInclude", libraryProject.DocumentElement.NamespaceURI);
                        fileElement.SetAttribute("Include", aExtraAddedColumnsInfo.ClassName + Generics.CppHeaderExtension);
                        projectIncludeFilesNode.AppendChild(fileElement);
					}
				}

				libraryProject.Normalize();
				
				libraryProject.Save(outputLibraryFileName);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateLibraryProjectFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateTableClasses
			(
			WizardTableInfo		aTableInfo,
			IBasePathFinder aPathFinder
			)
		{
			bool ok = 
				(
				aTableInfo != null &&
				aTableInfo.Library != null &&
				aTableInfo.Library.Module != null &&
				aPathFinder != null
				);


            if (GenerateCSharpCode)
                ok = ok && GenerateTableSourceFile(aTableInfo, aPathFinder, "Table_cs.tmpl", aTableInfo.ClassName + Generics.CsExtension);
            else
                ok = ok &&  GenerateTableSourceFile(aTableInfo, aPathFinder, "Table_cpp.tmpl", aTableInfo.ClassName + Generics.CppExtension) &&
                            GenerateTableSourceFile(aTableInfo, aPathFinder, "Table_h.tmpl", aTableInfo.ClassName + Generics.CppHeaderExtension);
            return ok;
        }

		//---------------------------------------------------------------------------
		private bool GenerateTableSourceFile
			(
			WizardTableInfo	aTableInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName
			)
		{
			if (aPathFinder == null || string.IsNullOrEmpty(templateResourceName) || string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null || aTableInfo == null || string.IsNullOrEmpty(aTableInfo.Name) || 
				aTableInfo.Library == null || aTableInfo.Library.Module == null)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aTableInfo.Library);

            if (codeFileName.EndsWith(Generics.CsExtension))
                sourcesPath = GetStandardLibraryPathForCS(aPathFinder, aTableInfo.Library);

			if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardTableCodeTemplateParser codeParser = new WizardTableCodeTemplateParser(aTableInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);
			
			return codeParser.WriteFileFromTemplate("Tables." + templateResourceName, outputFileName, saveInjectionPointsCode);
		}

		//---------------------------------------------------------------------------
		private bool GenerateTableReport(WizardTableInfo aTableInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null ||
				string.IsNullOrEmpty(aTableInfo.Name) || aTableInfo.Library == null || aTableInfo.Library.Module == null)
				return false;
			
			if (!GenerateTablesWoormReports)
				return true;

			string reportsPath = GetStandardModuleReportsPath(aPathFinder, aTableInfo.Library.Module);

			string reportFilename = reportsPath + 
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				NameSolverStrings.WrmExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(reportFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			WizardTableCodeTemplateParser codeParser = new WizardTableCodeTemplateParser(aTableInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			return codeParser.WriteFileFromTemplate("Tables.Report.tmpl", reportFilename, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateTableDBInfoFile(WizardTableInfo aTableInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aTableInfo == null ||	
				string.IsNullOrEmpty(aTableInfo.Name) || aTableInfo.Library == null || aTableInfo.Library.Module == null	)
				return false;
			
			if (!GenerateConfigurationFiles)
				return true;

			string dbInfoPath = GetStandardModuleObjectsPath(aPathFinder, aTableInfo.Library.Module);

			string dbInfoFilename = dbInfoPath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.DBInfo +
				Path.DirectorySeparatorChar	+
				aTableInfo.Name	+ 
				NameSolverStrings.XmlExtension;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(dbInfoFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			return DataParser.GenerateTableDBInfoFile(aTableInfo, dbInfoFilename);
		}

		//---------------------------------------------------------------------------
		private bool GenerateDBTClasses
			(
			WizardDBTInfo		aDBTInfo,
			IBasePathFinder aPathFinder
			)
		{
			if (
				    aDBTInfo == null ||
				    aDBTInfo == null ||
				    aDBTInfo.Library == null ||
				    aDBTInfo.Library.Module == null ||
				    aPathFinder == null
				)
				return false;
            
            if (aDBTInfo.IsMaster)
            {
                if (GenerateCSharpCode)
                    return GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTMasterInfo_cs.tmpl", aDBTInfo.ClassName + Generics.CsExtension);

                return GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTMasterInfo_cpp.tmpl", aDBTInfo.ClassName + Generics.CppExtension) &&
                       GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTMasterInfo_h.tmpl", aDBTInfo.ClassName + Generics.CppHeaderExtension);
            }

            if (aDBTInfo.IsSlave)
            {
                if (GenerateCSharpCode)
                    return GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveInfo_cs.tmpl", aDBTInfo.ClassName + Generics.CsExtension);
                
                return  GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveInfo_cpp.tmpl", aDBTInfo.ClassName + Generics.CppExtension) &&
                        GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveInfo_h.tmpl", aDBTInfo.ClassName + Generics.CppHeaderExtension);
            }

            if (aDBTInfo.IsSlaveBuffered)
            {
                if (GenerateCSharpCode)
                    return GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveBufferedInfo_cs.tmpl", aDBTInfo.ClassName + Generics.CsExtension);

                return GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveBufferedInfo_cpp.tmpl", aDBTInfo.ClassName + Generics.CppExtension) &&
                       GenerateDBTSourceFile(aDBTInfo, aPathFinder, "DBTSlaveBufferedInfo_h.tmpl", aDBTInfo.ClassName + Generics.CppHeaderExtension);
            }
			
			return true;
		}

		//---------------------------------------------------------------------------
		private bool GenerateDBTSourceFile
			(
			WizardDBTInfo aDBTInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName
			)
		{
			if (aPathFinder == null || string.IsNullOrEmpty(templateResourceName) || string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null || aDBTInfo == null || string.IsNullOrEmpty(aDBTInfo.Name) || 
				aDBTInfo.Library == null || aDBTInfo.Library.Module == null)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aDBTInfo.Library);

            if (codeFileName.EndsWith(Generics.CsExtension))
                sourcesPath = GetStandardLibraryPathForCS(aPathFinder, aDBTInfo.Library);
            
            if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardDBTCodeTemplateParser codeParser = new WizardDBTCodeTemplateParser(aDBTInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);

			return codeParser.WriteFileFromTemplate("DBTs." + templateResourceName, outputFileName, saveInjectionPointsCode);
		}

		//---------------------------------------------------------------------------
		private bool GenerateDocumentSourceFile
			(
			WizardDocumentInfo aDocumentInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName,
			Encoding encoding
			)
		{
			if (aPathFinder == null || string.IsNullOrEmpty(templateResourceName) ||
				string.IsNullOrEmpty(codeFileName) || applicationInfo == null || aDocumentInfo == null ||
				string.IsNullOrEmpty(aDocumentInfo.Name) || aDocumentInfo.Library == null || aDocumentInfo.Library.Module == null)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aDocumentInfo.Library);
            
            if (codeFileName.EndsWith(Generics.CsExtension))
                sourcesPath = GetStandardLibraryPathForCS(aPathFinder, aDocumentInfo.Library);
            
            if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardDocumentCodeTemplateParser codeParser = new WizardDocumentCodeTemplateParser(aDocumentInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);

			return codeParser.WriteFileFromTemplate("Documents." + templateResourceName, outputFileName, encoding, saveInjectionPointsCode, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateDocumentSourceFile
			(
			WizardDocumentInfo	aDocumentInfo,
			IBasePathFinder aPathFinder,
			string				templateResourceName,
			string				codeFileName
			)
		{
			return GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, templateResourceName, codeFileName, null);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateDocumentDescriptionFiles(WizardDocumentInfo aDocumentInfo, IBasePathFinder aPathFinder)
		{
			return GenerateDocumentDescriptionFile(aDocumentInfo, aPathFinder) &&
				GenerateDocumentDBTsDescriptionFile(aDocumentInfo, aPathFinder) &&
				GenerateDocumentExternalReferencesDescriptionFile(aDocumentInfo, aPathFinder);
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateDocumentDescriptionFile(WizardDocumentInfo aDocumentInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aDocumentInfo == null ||
				string.IsNullOrEmpty(aDocumentInfo.Name) || aDocumentInfo.Library == null || aDocumentInfo.Library.Module == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;
			
			string documentDescriptionPath = GetStandardDocumentDescriptionPath(aPathFinder, aDocumentInfo);

			if (string.IsNullOrEmpty(documentDescriptionPath))
				return false;

			string documentDescriptionFilename = documentDescriptionPath + 
				Path.DirectorySeparatorChar +
				Generics.DocumentDescriptionFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(documentDescriptionFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument descriptionDocument = new XmlDocument();

				XmlDeclaration configDeclaration = descriptionDocument.CreateXmlDeclaration(DocumentDescriptionXmlVersion, DocumentDescriptionXmlEncoding, null);
				if (configDeclaration != null)
					descriptionDocument.AppendChild(configDeclaration);

				XmlElement documentElement = descriptionDocument.CreateElement(XML_DOCDESCR_DOCUMENT_TAG);
				if (documentElement == null)
					return false;
				
				descriptionDocument.AppendChild(documentElement);
				
				XmlElement versionElement = descriptionDocument.CreateElement(XML_DOCDESCR_VERSION_TAG);
				if (versionElement == null)
					return false;
				versionElement.InnerText = defaultDocumentVersion.ToString();
				documentElement.AppendChild(versionElement);

				XmlElement maxDocumentsElement = descriptionDocument.CreateElement(XML_DOCDESCR_MAXDOCUMENTS_TAG);
				if (maxDocumentsElement == null)
					return false;
				maxDocumentsElement.InnerText = defaultDocumentMaxDocuments.ToString();
				documentElement.AppendChild(maxDocumentsElement);

				XmlElement maxDimensionElement = descriptionDocument.CreateElement(XML_DOCDESCR_MAXDIMENSION_TAG);
				if (maxDimensionElement == null)
					return false;
				maxDimensionElement.InnerText = defaultDocumentMaxDimension.ToString();
				documentElement.AppendChild(maxDimensionElement);

				XmlElement dataUrlElement = descriptionDocument.CreateElement(XML_DOCDESCR_DATAURL_TAG);
				if (dataUrlElement == null)
					return false;
				dataUrlElement.InnerText = aDocumentInfo.Name + NameSolverStrings.XmlExtension;
				documentElement.AppendChild(dataUrlElement);

				XmlElement envelopeClassElement = descriptionDocument.CreateElement(XML_DOCDESCR_ENVELOPECLASS_TAG);
				if (envelopeClassElement == null)
					return false;
				envelopeClassElement.SetAttribute(XML_DOCDESCR_EXTENSION_ATTRIBUTE, String.Empty);
				envelopeClassElement.SetAttribute(XML_DOCDESCR_LOCALIZABLE_ATTRIBUTE, TBWizardProjectParser.TrueAttributeValue);
				envelopeClassElement.InnerText = aDocumentInfo.Name;
				documentElement.AppendChild(envelopeClassElement);

				descriptionDocument.Save(documentDescriptionFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateDocumentDescriptionFile:", exception.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateDocumentDBTsDescriptionFile(WizardDocumentInfo aDocumentInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aDocumentInfo == null ||
				string.IsNullOrEmpty(aDocumentInfo.Name) || aDocumentInfo.Library == null || aDocumentInfo.Library.Module == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;
			
			string documentDescriptionPath = GetStandardDocumentDescriptionPath(aPathFinder, aDocumentInfo);
			if (string.IsNullOrEmpty(documentDescriptionPath))
				return false;

			string dbtsDocumentDescriptionFilename = documentDescriptionPath + 
				Path.DirectorySeparatorChar +
				Generics.DocumentDBTsFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(dbtsDocumentDescriptionFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument dbtsDescriptionDocument = new XmlDocument();

				XmlDeclaration configDeclaration = dbtsDescriptionDocument.CreateXmlDeclaration(DocumentDescriptionXmlVersion, DocumentDescriptionXmlEncoding, null);
				if (configDeclaration != null)
					dbtsDescriptionDocument.AppendChild(configDeclaration);

				XmlElement dbtsElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_DBTS_TAG);
				if (dbtsElement == null)
					return false;
				
				dbtsElement.SetAttribute("xmlns:sl", LocalizableXmlDocument.NamespaceUri);
				dbtsDescriptionDocument.AppendChild(dbtsElement);

				WizardDBTInfo masterInfo = aDocumentInfo.DBTMaster;
				if (masterInfo != null)
				{
					XmlElement masterElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_MASTER_TAG);

					masterElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, aDocumentInfo.GetNameSpace() + "." + masterInfo.Name);
		
					XmlElement masterTitleElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_DBT_TITLE_TAG);
					masterTitleElement.SetAttribute(XML_DOCDBTSDESCR_LOCALIZABLE_ATTRIBUTE,  TBWizardProjectParser.TrueAttributeValue);
					masterTitleElement.InnerText = aDocumentInfo.Title;
                    masterElement.AppendChild(masterTitleElement);

					WizardTableInfo masterTable = masterInfo.GetTableInfo();
					if (masterTable != null)
					{
						XmlElement masterTableElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_DBT_TABLE_TAG);
						masterTableElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, masterTable.GetNameSpace());
						masterTableElement.InnerText = masterTable.Name;
						masterElement.AppendChild(masterTableElement);
					}
					
					WizardDBTInfoCollection slaves = aDocumentInfo.DBTsSlaves;
					if (slaves != null && slaves.Count > 0)
					{
						XmlElement slavesElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_SLAVES_TAG);
						foreach (WizardDBTInfo slaveInfo in slaves)
						{
							XmlElement slaveElement = dbtsDescriptionDocument.CreateElement(slaveInfo.IsSlaveBuffered ? XML_DOCDBTSDESCR_SLAVEBUFFERED_TAG : XML_DOCDBTSDESCR_SLAVE_TAG);

							slaveElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, aDocumentInfo.GetNameSpace() + "." + slaveInfo.Name);
						
							XmlElement slaveTitleElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_DBT_TITLE_TAG);
							slaveTitleElement.SetAttribute(XML_DOCDBTSDESCR_LOCALIZABLE_ATTRIBUTE,  TBWizardProjectParser.TrueAttributeValue);
							slaveTitleElement.InnerText = slaveInfo.SlaveTabTitle;
							slaveElement.AppendChild(slaveTitleElement);

							WizardTableInfo slaveTable = slaveInfo.GetTableInfo();
							if (slaveTable != null)
							{
								XmlElement slaveTableElement = dbtsDescriptionDocument.CreateElement(XML_DOCDBTSDESCR_DBT_TABLE_TAG);
								slaveTableElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, slaveTable.GetNameSpace());
								slaveTableElement.InnerText = slaveTable.Name;
								slaveElement.AppendChild(slaveTableElement);
							}

							slavesElement.AppendChild(slaveElement);
						}
						masterElement.AppendChild(slavesElement);
					}
					
					dbtsElement.AppendChild(masterElement);
				}	
				
				dbtsDescriptionDocument.Save(dbtsDocumentDescriptionFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateDocumentDBTsDescriptionFile:", exception.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateDocumentExternalReferencesDescriptionFile(WizardDocumentInfo aDocumentInfo, IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || applicationInfo == null || aDocumentInfo == null ||
				string.IsNullOrEmpty(aDocumentInfo.Name) || aDocumentInfo.Library == null || aDocumentInfo.Library.Module == null)
				return false;

			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;
			
			string documentDescriptionPath = GetStandardDocumentDescriptionPath(aPathFinder, aDocumentInfo);
			if (string.IsNullOrEmpty(documentDescriptionPath))
				return false;

			string externalReferencesDescriptionFilename = documentDescriptionPath + 
				Path.DirectorySeparatorChar +
				Generics.DocumentExtRefFilename;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(externalReferencesDescriptionFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument externalReferencesDocument = new XmlDocument();

				XmlDeclaration configDeclaration = externalReferencesDocument.CreateXmlDeclaration(DocumentDescriptionXmlVersion, DocumentDescriptionXmlEncoding, null);
				if (configDeclaration != null)
					externalReferencesDocument.AppendChild(configDeclaration);

				XmlElement externalReferencesElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_MAIN_TAG);
				if (externalReferencesElement == null)
					return false;
				
				externalReferencesDocument.AppendChild(externalReferencesElement);

				if (aDocumentInfo.DBTsCount > 0)
				{
					foreach (WizardDBTInfo aDBTInfo in aDocumentInfo.DBTsInfo)
					{
						XmlElement dbtElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_DBT_TAG);
						dbtElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, aDocumentInfo.GetNameSpace() + "." + aDBTInfo.Name);
						dbtElement.AppendChild(externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_DBT_EXPORT_TAG));

						XmlElement dbtExternalReferencesElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_DBT_EXT_REFS_TAG);
						if (aDBTInfo.ColumnsCount > 0 && aDBTInfo.HasHKLDefinedColumns())
						{
							foreach(WizardDBTColumnInfo aColumnInfo in aDBTInfo.ColumnsInfo)
							{
								if (!aColumnInfo.IsHKLDefined)
									continue;

								XmlElement externalReferenceElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_DBT_EXT_REF_TAG);
								if (!aColumnInfo.HotKeyLink.IsReferenced) 
								{
									object hotLinkParent = aDBTInfo.GetHotKeyLinkParent(aColumnInfo.HotKeyLink);
									if (hotLinkParent == null)
										continue;

									string libraryNameSpace = String.Empty;
									if (hotLinkParent is WizardTableInfo && ((WizardTableInfo)hotLinkParent).Library != null)
										libraryNameSpace = ((WizardTableInfo)hotLinkParent).Library.GetNameSpace();
									if (hotLinkParent is WizardDocumentInfo && ((WizardDocumentInfo)hotLinkParent).Library != null)
										libraryNameSpace = ((WizardDocumentInfo)hotLinkParent).Library.GetNameSpace();
									
									if (string.IsNullOrEmpty(libraryNameSpace))
										continue;
									externalReferenceElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, libraryNameSpace + "." + aColumnInfo.HotKeyLink.Name);
								}
								else
									externalReferenceElement.SetAttribute(XML_DOCDBTSDESCR_NAMESPACE_ATTRIBUTE, aColumnInfo.HotKeyLink.ReferencedNameSpace);

								XmlElement keysElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_KEYS_TAG);

								XmlElement keySegmentElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_KEY_SEGMENT_TAG);
								
								XmlElement foreignKeySegmentElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_FOREIGN_KEYSEG_TAG);
								foreignKeySegmentElement.InnerText = aColumnInfo.HotKeyLink.CodeColumnName;
								keySegmentElement.AppendChild(foreignKeySegmentElement);
								
								XmlElement primaryKeySegmentElement = externalReferencesDocument.CreateElement(XML_DOC_EXT_REFS_PRIMARY_KEYSEG_TAG);
								primaryKeySegmentElement.InnerText = aColumnInfo.ColumnName;
								keySegmentElement.AppendChild(primaryKeySegmentElement);

								keysElement.AppendChild(keySegmentElement);

								externalReferenceElement.AppendChild(keysElement);

								dbtExternalReferencesElement.AppendChild(externalReferenceElement);
							}
						}
						dbtElement.AppendChild(dbtExternalReferencesElement);

						externalReferencesElement.AppendChild(dbtElement);
					}
				}
				
				externalReferencesDocument.Save(externalReferencesDescriptionFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateDocumentExternalReferencesDescriptionFile:", exception.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool GenerateHotLinkFile(WizardLibraryInfo aLibraryInfo, WizardHotKeyLinkInfo aHotKeyLinkInfo, IBasePathFinder aPathFinder)
		{
			if 
				(
				aPathFinder == null || 
				applicationInfo == null ||
				aLibraryInfo == null ||
				aLibraryInfo.Module == null ||
				aHotKeyLinkInfo == null ||
				!aHotKeyLinkInfo.IsDefined ||
				string.IsNullOrEmpty(aHotKeyLinkInfo.Name)
				)
				return false;
			
			// Controllo se si deve generare il file
			if (!GenerateConfigurationFiles)
				return true;

			//Creo il path per la folder ReferenceObjects
			string referenceObjectsPath = GetStandardReferenceObjectsPath(aPathFinder, aLibraryInfo.Module);
			if (string.IsNullOrEmpty(referenceObjectsPath))
				return false;

			string referenceObjectsFilename = referenceObjectsPath + 
				Path.DirectorySeparatorChar	+
				aHotKeyLinkInfo.Name + NameSolverStrings.XmlExtension;

			// Controlla se devo scrivere o meno il file e verifica se esiste la cartella, altrimenti la crea
			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(referenceObjectsFilename);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;

			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			try
			{
				XmlDocument referenceObjectsDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = referenceObjectsDocument.CreateXmlDeclaration(ReferenceObjectsXmlVersion, ReferenceObjectsXmlEncoding, null);
				if (configDeclaration != null)
					referenceObjectsDocument.AppendChild(configDeclaration);

				//ROOT ovvero <HotKeyLink>
				XmlElement hotKeyLinkElement = referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.HotKeyLink);
				if (hotKeyLinkElement == null)
					return false;

				referenceObjectsDocument.AppendChild(hotKeyLinkElement);

				//<Function>
				XmlElement functionElement =  referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.Function);
				if (functionElement == null)
					return false;

				//Attributo namespace
				functionElement.SetAttribute(ReferenceObjectsXML.Attribute.Namespace, aLibraryInfo.GetHotKeyLinkNamespace(aHotKeyLinkInfo));
				//Attributo localize
				functionElement.SetAttribute(ReferenceObjectsXML.Attribute.Localize, aHotKeyLinkInfo.Title);
				//Attributo type
				functionElement.SetAttribute(ReferenceObjectsXML.Attribute.Type, aHotKeyLinkInfo.CodeColumn.GetDataTypeTBXmlValue());
				
				hotKeyLinkElement.AppendChild(functionElement);

				//<DBField>
				XmlElement dbFieldElement =  referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.DbField);
				if (dbFieldElement == null)
					return false;

				dbFieldElement.SetAttribute(ReferenceObjectsXML.Attribute.Name, aHotKeyLinkInfo.Table.Name + "." + aHotKeyLinkInfo.CodeColumnName);
				hotKeyLinkElement.AppendChild(dbFieldElement);

				if (aHotKeyLinkInfo.ShowCombo)
				{
					XmlElement comboBoxElement = referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.ComboBox);
					if (comboBoxElement == null)
						return false;
				
					//		<ComboBox>
					// 			<Column source="<HotKeyLink_CodeColumnName>" />
					// 			<Column source="<HotKeyLink_CodeColumnName>" />
					//		</ComboBox>
					XmlElement columnComboBoxElement = referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.Column);
					columnComboBoxElement.SetAttribute(ReferenceObjectsXML.Attribute.Source, aHotKeyLinkInfo.Table.Name + "." + aHotKeyLinkInfo.CodeColumnName);
					comboBoxElement.AppendChild(columnComboBoxElement);
					
					if (!string.IsNullOrEmpty(aHotKeyLinkInfo.DescriptionColumnName))
					{
						columnComboBoxElement = referenceObjectsDocument.CreateElement(ReferenceObjectsXML.Element.Column);
						columnComboBoxElement.SetAttribute(ReferenceObjectsXML.Attribute.Source, aHotKeyLinkInfo.Table.Name + "." + aHotKeyLinkInfo.DescriptionColumnName);
						comboBoxElement.AppendChild(columnComboBoxElement);
					}

					hotKeyLinkElement.AppendChild(comboBoxElement);
				}

				XmlElement classNameElement =  referenceObjectsDocument.CreateElement(XML_HOTLINK_CLASS_NAME_TAG);
				if (classNameElement == null)
					return false;

				classNameElement.InnerText = aHotKeyLinkInfo.ClassName;
				hotKeyLinkElement.AppendChild(classNameElement);

				referenceObjectsDocument.Save(referenceObjectsFilename);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in WizardCodeGenerator.GenerateHotLinkFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private bool GenerateClientDocumentSourceFile
			(
			WizardClientDocumentInfo aClientDocumentInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName,
			Encoding encoding
			)
		{
			if (
				aPathFinder == null || 
				string.IsNullOrEmpty(templateResourceName) ||
				string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null ||
				aClientDocumentInfo == null ||
				string.IsNullOrEmpty(aClientDocumentInfo.Name) || 
				aClientDocumentInfo.Library == null ||
				aClientDocumentInfo.Library.Module == null ||
				aClientDocumentInfo.ServerDocumentInfo == null
				)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aClientDocumentInfo.Library);
			if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardClientDocumentCodeTemplateParser codeParser = new WizardClientDocumentCodeTemplateParser(aClientDocumentInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);

			return codeParser.WriteFileFromTemplate("Documents." + templateResourceName, outputFileName, encoding, saveInjectionPointsCode, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateClientDocumentSourceFile
			(
			WizardClientDocumentInfo	aClientDocumentInfo,
			IBasePathFinder aPathFinder,
			string						templateResourceName,
			string						codeFileName
			)
		{
			return GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, templateResourceName, codeFileName, null);
		}

		//---------------------------------------------------------------------------
		private bool GenerateClientDocumentBitmapFile
			(
			WizardClientDocumentInfo aClientDocumentInfo,
			IBasePathFinder aPathFinder,
			string embeddedResourceName,
			string destinationBitmapName
			)
		{
			if 	(
				aPathFinder == null || 
				string.IsNullOrEmpty(embeddedResourceName) ||
				string.IsNullOrEmpty(destinationBitmapName) ||
				applicationInfo == null ||
				aClientDocumentInfo == null ||
				string.IsNullOrEmpty(aClientDocumentInfo.Name) || 
				aClientDocumentInfo.Library == null ||
				aClientDocumentInfo.Library.Module == null
				)
				return false;

			if (!GenerateLibrariesSourceCode || !aClientDocumentInfo.CreateSlaveFormView)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aClientDocumentInfo.Library);
			if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + "res" + Path.DirectorySeparatorChar + destinationBitmapName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			return Generics.CopyEmbeddedResourceToFile("Microarea.Library.TBWizardProjects.CodeTemplates.Documents." + embeddedResourceName, outputFileName);
		}

		//---------------------------------------------------------------------------
		private bool GenerateAdditionalColumnsSourceFile
			(
			WizardExtraAddedColumnsInfo	aExtraAddedColumnsInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName,
			Encoding encoding
			)
		{
			if 
				(
				aPathFinder == null || 
				string.IsNullOrEmpty(templateResourceName) ||
				string.IsNullOrEmpty(codeFileName) ||
				applicationInfo == null ||
				aExtraAddedColumnsInfo == null ||
				aExtraAddedColumnsInfo.ColumnsCount == 0 || 
				aExtraAddedColumnsInfo.Library == null ||
				aExtraAddedColumnsInfo.Library.Module == null
				)
				return false;
			
			if (!GenerateLibrariesSourceCode)
				return true;

			string sourcesPath = GetStandardLibraryPath(aPathFinder, aExtraAddedColumnsInfo.Library);
			if (sourcesPath == null || sourcesPath.Trim().Length == 0)
				return false;

			string outputFileName = sourcesPath + Path.DirectorySeparatorChar + codeFileName;

			WritingCodeBehaviour behaviourToApply = IsCodeFileToWrite(outputFileName);
			
			if (behaviourToApply == WritingCodeBehaviour.Abend)
				return false;
			
			if (behaviourToApply == WritingCodeBehaviour.Skip)
				return true;

			// Il file non esiste oppure lo posso sovrascrivere!
			bool saveInjectionPointsCode = (PreserveInjectedCode && behaviourToApply != WritingCodeBehaviour.WriteNew);
			
			WizardAdditionalColumnsCodeTemplateParser codeParser = new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			if (saveInjectionPointsCode)
				codeParser.ResolveCustomInjectionPoints += new UnresolvedInjectionPointsEventHandler(this.CodeParser_ResolveCustomInjectionPoints);

			return codeParser.WriteFileFromTemplate("Tables." + templateResourceName, outputFileName, encoding, saveInjectionPointsCode, false);
		}

		//---------------------------------------------------------------------------
		private bool GenerateAdditionalColumnsSourceFile
			(
			WizardExtraAddedColumnsInfo	aExtraAddedColumnsInfo,
			IBasePathFinder aPathFinder,
			string	templateResourceName,
			string	codeFileName
			)
		{
			return GenerateAdditionalColumnsSourceFile(aExtraAddedColumnsInfo, aPathFinder, templateResourceName, codeFileName, null);
		}

		//---------------------------------------------------------------------------
		private void CodeParser_SubstitutionInProgress(object sender, string parsedText)
		{
			string eventText = (sender != null && sender is CodeTemplateParser) ? ((CodeTemplateParser)sender).CurrentFileName : parsedText;

			if (SubstitutionInProgress != null)
				SubstitutionInProgress(this, eventText);
		}

		//---------------------------------------------------------------------------
		private void CodeParser_ResolveCustomInjectionPoints(object sender, string originalFile, string temporaryFile, InjectionPointsCollection unresolvedInjectionPoints, Encoding encoding)
		{
			if 
				(
				!PreserveInjectedCode || 
				unresolvedInjectionPoints == null ||
				unresolvedInjectionPoints.Count == 0 ||
				originalFile == null ||
				originalFile.Trim().Length == 0 ||
				temporaryFile == null ||
				temporaryFile.Trim().Length == 0 ||
				!File.Exists(originalFile) ||
				!File.Exists(temporaryFile)
				)
				return;

			if (ResolveCustomInjectionPoints != null)
				ResolveCustomInjectionPoints(this, originalFile, temporaryFile, unresolvedInjectionPoints, encoding);
		}

        //---------------------------------------------------------------------------
        private string GetDocumentViewFileName(WizardDocumentInfo aDocumentInfo, string ext, bool bResourceFile = false)
        {
            // se il nome della classe del documento è "D" + namespace
            // allora imposta il nome come "UI" + namespace + estensione (attuale standard)
            // altrimenti usa nome classe + "View" (escluso risorse) + estensione (vecchio standard)
            string filename = "";
            //if (aDocumentInfo.ClassName == "D" + aDocumentInfo.Name)
                //filename = "UI" + aDocumentInfo.Name + ext;
            //else
            //    filename = aDocumentInfo.ClassName + /*(bResourceFile ? "" : "View")*/"View" + ext;

            // se il nome della classe inizia con "D"
            // allora imposta il nome come "UI" + nomeclasse.mid(1) + ext
            // altrimenti "UI" + namespace + ext
            if (aDocumentInfo.ClassName.Substring(0, 1) == "D")
                filename = "UI" + aDocumentInfo.ClassName.Substring(1) + ext;
            else
                filename = "UI" + aDocumentInfo.Name + ext;

            return filename;
        }

        //---------------------------------------------------------------------------
        private string GetClientDocumentViewFileName(WizardClientDocumentInfo aDocumentInfo, string ext, bool bResourceFile = false)
        {
            // se il nome della classe del documento è "D" + namespace
            // allora imposta il nome come "UI" + namespace + estensione (attuale standard)
            // altrimenti usa nome classe + "View" (escluso risorse) + estensione (vecchio standard)
            string filename = "";
            //if (aDocumentInfo.ClassName == "CD" + aDocumentInfo.Name)
                //filename = "UI" + aDocumentInfo.Name + ext;
            //else
            //    filename = aDocumentInfo.ClassName + /*(bResourceFile ? "" : "View")*/"View" + ext;

            // se il nome della classe inizia con "D"
            // allora imposta il nome come "UI" + nomeclasse.mid(2) + ext
            // altrimenti "UI" + namespace + ext
            if (aDocumentInfo.ClassName.Substring(0, 2) == "CD")
                filename = "UI" + aDocumentInfo.ClassName.Substring(2) + ext;
            else
                filename = "UI" + aDocumentInfo.Name + ext;

            return filename;
        }


		#endregion

		#region WizardCodeGenerator public properties

		//---------------------------------------------------------------------------
		internal CodeOverwriteEventHandler CodeOverwriteEventHandler	{ get { return codeOverwriteEventHandler; } set { codeOverwriteEventHandler = value; } }
		//---------------------------------------------------------------------------
		internal CodeWriteFailureEventHandler CodeWriteFailureEventHandler{ get { return codeWriteFailureEventHandler; } set { codeWriteFailureEventHandler = value; } }
		//---------------------------------------------------------------------------
		public WizardApplicationInfo ApplicationInfo { get { return applicationInfo;} }
			
		//---------------------------------------------------------------------------
		public bool GenerateDatabaseScripts
		{
			get { return (filesToGenerate & FilesToGenerate.DatabaseScripts) == FilesToGenerate.DatabaseScripts; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.DatabaseScripts;
				else
					filesToGenerate &= ~FilesToGenerate.DatabaseScripts;
			}
		}

		//---------------------------------------------------------------------------
		public bool GenerateConfigurationFiles
		{
			get { return (filesToGenerate & FilesToGenerate.ConfigurationFiles) == FilesToGenerate.ConfigurationFiles; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.ConfigurationFiles;
				else
					filesToGenerate &= ~FilesToGenerate.ConfigurationFiles;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool GenerateMenuFiles
		{
			get { return (filesToGenerate & FilesToGenerate.MenuFiles) == FilesToGenerate.MenuFiles; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.MenuFiles;
				else
					filesToGenerate &= ~FilesToGenerate.MenuFiles;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool GenerateLibrariesSourceCode
		{
			get { return (filesToGenerate & FilesToGenerate.LibrariesSourceCode) == FilesToGenerate.LibrariesSourceCode; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.LibrariesSourceCode;
				else
					filesToGenerate &= ~FilesToGenerate.LibrariesSourceCode;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool GenerateTablesWoormReports
		{
			get { return (filesToGenerate & FilesToGenerate.TablesWoormReports) == FilesToGenerate.TablesWoormReports; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.TablesWoormReports;
				else
					filesToGenerate &= ~FilesToGenerate.TablesWoormReports;
			}
		}

		//---------------------------------------------------------------------------
		public bool PreserveInjectedCode
		{
			get { return (filesToGenerate & FilesToGenerate.PreserveInjectedCode) == FilesToGenerate.PreserveInjectedCode; }

			set
			{
				if (value)
					filesToGenerate |= FilesToGenerate.PreserveInjectedCode;
				else
					filesToGenerate &= ~FilesToGenerate.PreserveInjectedCode;
			}
		}

        //---------------------------------------------------------------------------
        public bool GenerateCSharpCode
        {
            get { return (filesToGenerate & FilesToGenerate.GenerateCSharp) == FilesToGenerate.GenerateCSharp; }

            set
            {
                if (value)
                    filesToGenerate |= FilesToGenerate.GenerateCSharp;
                else
                    filesToGenerate &= ~FilesToGenerate.GenerateCSharp;
            }
        }
        #endregion
	
		#region WizardCodeGenerator public methods

		//---------------------------------------------------------------------------
		public bool GenerateApplicationCode(IBasePathFinder aPathFinder, WritingCodeBehaviour wcb = WritingCodeBehaviour.Undefined)
		{
			commonBehaviourToApply = wcb;
			return 
				(
				aPathFinder != null &&
				GenerateApplicationConfigurationFile(aPathFinder) &&
				GenerateApplicationSolutionFiles(aPathFinder) &&
				GenerateModulesCode(aPathFinder) &&
				GenerateApplicationCodeSolutionFile(aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateModuleCode(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			return 
				(
				aModuleInfo != null &&
				aPathFinder != null &&
				GenerateModuleConfigurationFile(aModuleInfo, aPathFinder) &&
				GenerateModuleDatabaseCreationScripts(aModuleInfo, aPathFinder) &&
				GenerateModuleDatabaseUpgradeScripts(aModuleInfo, aPathFinder) &&
				GenerateModuleEnumsHeaderFile(aModuleInfo, aPathFinder) &&
				GenerateLibrariesCode(aModuleInfo, aPathFinder) &&
				GenerateModuleObjectsFiles(aModuleInfo, aPathFinder) &&
				GenerateModuleMenu(aModuleInfo, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibrariesCode(WizardModuleInfo aModuleInfo, IBasePathFinder aPathFinder)
		{
			if (aModuleInfo == null || aPathFinder == null)
				return false;

			if (aModuleInfo.LibrariesCount == 0)
				return true;
			
			foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
			{
				if (!GenerateLibraryCode(aLibraryInfo, aPathFinder))
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibraryCode(WizardLibraryInfo aLibraryInfo, IBasePathFinder aPathFinder)
		{
			return 
				(
				aLibraryInfo != null &&
				aLibraryInfo.Module != null &&
				aPathFinder != null &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "beginh_dex.tmpl", "beginh.dex") &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "endh_dex.tmpl", "endh.dex") &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "stdafx_cpp.tmpl", "stdafx.cpp") &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "stdafx_h.tmpl", "stdafx.h") &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "Library_cpp.tmpl", aLibraryInfo.Name + Generics.CppExtension) &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "Library_rc.tmpl", aLibraryInfo.Name + Generics.CppResourceFileExtension, Encoding.ASCII) &&
				GenerateLibrarySourceFile(aLibraryInfo, aPathFinder, "LibraryInterface_cpp.tmpl", aLibraryInfo.Name + "Interface" + Generics.CppExtension) &&
				GenerateLibraryTablesCode(aLibraryInfo, aPathFinder) &&
				GenerateLibraryDBTsCode(aLibraryInfo, aPathFinder) &&
				GenerateLibraryDocumentsCode(aLibraryInfo, aPathFinder) &&
				GenerateLibraryClientDocumentsCode(aLibraryInfo, aPathFinder) &&
				GenerateLibraryAdditionalColumnsCode(aLibraryInfo, aPathFinder) &&
				GenerateLibraryProjectFile(aLibraryInfo, aPathFinder)
				);
		}
	
		//---------------------------------------------------------------------------
		public bool GenerateLibraryTablesCode(WizardLibraryInfo	aLibraryInfo, IBasePathFinder aPathFinder)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null || aPathFinder == null)
				return false;

			if (aLibraryInfo.TablesCount == 0)
				return true;
			
			foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
			{
				if (!GenerateTableCode(aTableInfo, aPathFinder))
					return false;
			}
			return true;
		}
			
		//---------------------------------------------------------------------------
		public bool GenerateTableCode
			(
			WizardTableInfo		aTableInfo,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aTableInfo != null &&
				aTableInfo.Library != null &&
				aTableInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateTableClasses(aTableInfo, aPathFinder) &&
				GenerateTableReport(aTableInfo, aPathFinder) &&
				GenerateTableDBInfoFile(aTableInfo, aPathFinder) &&
				(!aTableInfo.IsHKLDefined || GenerateHotLinkFile(aTableInfo.Library, aTableInfo.HotKeyLink, aPathFinder))
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibraryDBTsCode
			(
			WizardLibraryInfo	aLibraryInfo,
			IBasePathFinder aPathFinder
			)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null || aPathFinder == null)
				return false;

			if (aLibraryInfo.DBTsCount == 0)
				return true;
			
			foreach(WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
			{
				if (!GenerateDBTCode(aDBTInfo, aPathFinder))
					return false;
			}
			return true;
		}
		
		//---------------------------------------------------------------------------
		public bool GenerateDBTCode
			(
			WizardDBTInfo		aDBTInfo,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aDBTInfo != null &&
				aDBTInfo.Library != null &&
				aDBTInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateDBTClasses(aDBTInfo, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibraryDocumentsCode
			(
			WizardLibraryInfo	aLibraryInfo,
			IBasePathFinder aPathFinder
			)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null || aPathFinder == null)
				return false;

			if (aLibraryInfo.DocumentsCount == 0)
				return true;
			
			foreach(WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
			{
				switch (aDocumentInfo.DefaultType)
				{
					case WizardDocumentInfo.DocumentType.DataEntry:
						if (!GenerateDocumentCode(aDocumentInfo, aPathFinder))
							return false;
						break;

					case WizardDocumentInfo.DocumentType.Batch:
						if (!GenerateBatchDocumentCode(aDocumentInfo, aPathFinder))
							return false;
						break;

					default:
						break;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool GenerateDocumentCode
			(
			WizardDocumentInfo	aDocumentInfo,
			IBasePathFinder aPathFinder
			)
		{
			bool ok =   aDocumentInfo != null &&
				        aDocumentInfo.Library != null &&
				        aDocumentInfo.Library.Module != null &&
				        aPathFinder != null;

            if (GenerateCSharpCode)
                return ok && GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_cs.tmpl", aDocumentInfo.ClassName + Generics.CsExtension, Encoding.ASCII) &&
                             GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "DocumentView_cs.tmpl", aDocumentInfo.ClassName + "View" + Generics.CsExtension, Encoding.ASCII) &&
                             GenerateDocumentDescriptionFiles(aDocumentInfo, aPathFinder);

            return ok && 
                GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_cpp.tmpl", aDocumentInfo.ClassName + Generics.CppExtension) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_h.tmpl", aDocumentInfo.ClassName + Generics.CppHeaderExtension) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "DocumentView_cpp.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppExtension)) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "DocumentView_h.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppHeaderExtension)) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_rc.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceFileExtension, true), Encoding.ASCII) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_hrc.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceHeaderExtension, true), Encoding.ASCII) &&
				GenerateDocumentDescriptionFiles(aDocumentInfo, aPathFinder) &&
				(!aDocumentInfo.IsHKLDefined || GenerateHotLinkFile(aDocumentInfo.Library, aDocumentInfo.HotKeyLink, aPathFinder));
		}
		
		//---------------------------------------------------------------------------
		public bool GenerateBatchDocumentCode
			(
			WizardDocumentInfo	aDocumentInfo,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aDocumentInfo != null &&
				aDocumentInfo.Library != null &&
				aDocumentInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "BatchDocument_cpp.tmpl", aDocumentInfo.ClassName + Generics.CppExtension) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "BatchDocument_h.tmpl", aDocumentInfo.ClassName + Generics.CppHeaderExtension)&&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "BatchDocumentView_cpp.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppExtension)) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "BatchDocumentView_h.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppHeaderExtension)) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_rc.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceFileExtension, true), Encoding.ASCII) &&
				GenerateDocumentSourceFile(aDocumentInfo, aPathFinder, "Document_hrc.tmpl", GetDocumentViewFileName(aDocumentInfo, Generics.CppResourceHeaderExtension, true), Encoding.ASCII) &&
				GenerateDocumentDescriptionFiles(aDocumentInfo, aPathFinder)
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibraryClientDocumentsCode
			(
			WizardLibraryInfo	aLibraryInfo,
			IBasePathFinder aPathFinder
			)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null || aPathFinder == null)
				return false;

			if (aLibraryInfo.ClientDocumentsCount == 0)
				return true;
			
			foreach(WizardClientDocumentInfo aClientDocumentInfo in aLibraryInfo.ClientDocumentsInfo)
			{
				if (!GenerateClientDocumentCode(aClientDocumentInfo, aPathFinder))
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool GenerateClientDocumentCode
			(
			WizardClientDocumentInfo	aClientDocumentInfo,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aClientDocumentInfo != null &&
				aClientDocumentInfo.Library != null &&
				aClientDocumentInfo.Library.Module != null &&
				aClientDocumentInfo.ServerDocumentInfo != null &&
				aPathFinder != null &&
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocument_cpp.tmpl", aClientDocumentInfo.ClassName + Generics.CppExtension) &&
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocument_h.tmpl", aClientDocumentInfo.ClassName + Generics.CppHeaderExtension) &&
				(
				!aClientDocumentInfo.IsInterfacePresent ||
				(
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocumentView_cpp.tmpl", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppExtension)) &&
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocumentView_h.tmpl", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppHeaderExtension)) &&
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocument_rc.tmpl", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppResourceFileExtension, true), Encoding.ASCII) &&
				GenerateClientDocumentSourceFile(aClientDocumentInfo, aPathFinder, "ClientDocument_hrc.tmpl", GetClientDocumentViewFileName(aClientDocumentInfo, Generics.CppResourceHeaderExtension, true), Encoding.ASCII) &&
				GenerateClientDocumentBitmapFile(aClientDocumentInfo, aPathFinder, "ClientDocumentButtonLarge.bmp", GetClientDocumentViewFileName(aClientDocumentInfo, "Large.bmp", true)) &&
				GenerateClientDocumentBitmapFile(aClientDocumentInfo, aPathFinder, "ClientDocumentButtonSmall.bmp", GetClientDocumentViewFileName(aClientDocumentInfo, "Small.bmp", true)) &&
				GenerateClientDocumentBitmapFile(aClientDocumentInfo, aPathFinder, "ClientDocumentButtonLargeDisabled.bmp", GetClientDocumentViewFileName(aClientDocumentInfo, "LargeDisabled.bmp", true)) &&
				GenerateClientDocumentBitmapFile(aClientDocumentInfo, aPathFinder, "ClientDocumentButtonSmallDisabled.bmp", GetClientDocumentViewFileName(aClientDocumentInfo, "SmallDisabled.bmp", true)) 
				)
				)
				);
		}

		//---------------------------------------------------------------------------
		public bool GenerateLibraryAdditionalColumnsCode
			(
			WizardLibraryInfo	aLibraryInfo,
			IBasePathFinder aPathFinder
			)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null || aPathFinder == null)
				return false;

			if (aLibraryInfo.ExtraAddedColumnsCount == 0)
				return true;
			
			foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in aLibraryInfo.ExtraAddedColumnsInfo)
			{
				if (!GenerateAdditionalColumnsCode(aExtraAddedColumnsInfo, aPathFinder))
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool GenerateAdditionalColumnsCode
			(
			WizardExtraAddedColumnsInfo	aExtraAddedColumnsInfo,
			IBasePathFinder aPathFinder
			)
		{
			return 
				(
				aExtraAddedColumnsInfo != null &&
				aExtraAddedColumnsInfo.ColumnsCount > 0 &&
				aExtraAddedColumnsInfo.Library != null &&
				aExtraAddedColumnsInfo.Library.Module != null &&
				aPathFinder != null &&
				GenerateAdditionalColumnsSourceFile(aExtraAddedColumnsInfo, aPathFinder, "AdditionalColumns_cpp.tmpl", aExtraAddedColumnsInfo.ClassName + Generics.CppExtension) &&
				GenerateAdditionalColumnsSourceFile(aExtraAddedColumnsInfo, aPathFinder, "AdditionalColumns_h.tmpl", aExtraAddedColumnsInfo.ClassName + Generics.CppHeaderExtension) 
				);
		}

		#region Functions returning paths

		#region Application Path
		
		//---------------------------------------------------------------------------
		public static string GetStandardApplicationContainerPath(IBasePathFinder aPathFinder, ApplicationType aApplicationInfoType)
		{
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;

			return aPathFinder.GetStandardApplicationContainerPath(aApplicationInfoType);
		}
		
		//---------------------------------------------------------------------------
		public static string GetStandardApplicationContainerPath(IBasePathFinder aPathFinder, WizardApplicationInfo aApplicationInfo)
		{
			return GetStandardApplicationContainerPath(aPathFinder, (aApplicationInfo != null) ? aApplicationInfo.Type : ApplicationType.TaskBuilderApplication);
		}

		//---------------------------------------------------------------------------
		public static string GetStandardTaskBuilderApplicationContainerPath(IBasePathFinder aPathFinder)
		{
			return GetStandardApplicationContainerPath(aPathFinder, ApplicationType.TaskBuilderApplication);
		}

		//---------------------------------------------------------------------------
		public static string GetStandardTaskBuilderApplicationContainerPath()
		{
			return GetStandardTaskBuilderApplicationContainerPath(null);
		}

        //---------------------------------------------------------------------------
        public static string GetStandardApplicationPath
            (
            IBasePathFinder aPathFinder,
            WizardApplicationInfo aApplicationInfo,
            bool substituteEnvironmentVariables
            )
        {
            if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;

            string applicationPath = String.Empty;
            if (substituteEnvironmentVariables)
            {
                string envTBAppsPath = Environment.GetEnvironmentVariable("TBApps");
                if (!string.IsNullOrEmpty(envTBAppsPath))
                    applicationPath = "$(TBApps)";
            }
            if (string.IsNullOrEmpty(applicationPath))
                applicationPath = GetStandardApplicationContainerPath(aPathFinder, aApplicationInfo);

            if (string.IsNullOrEmpty(applicationPath))
                return String.Empty;

            if (aApplicationInfo != null && aApplicationInfo.Name != null && aApplicationInfo.Name.Trim().Length > 0)
            {
                applicationPath += Path.DirectorySeparatorChar;
                applicationPath += aApplicationInfo.Name;
            }

            return applicationPath;
        }

        //---------------------------------------------------------------------------
        public static string GetStandardApplicationPath(IBasePathFinder aPathFinder, WizardApplicationInfo aApplicationInfo)
        {
            return GetStandardApplicationPath(aPathFinder, aApplicationInfo, false);
        }

		//---------------------------------------------------------------------------
		public string GetStandardApplicationPath(IBasePathFinder aPathFinder)
		{
			return GetStandardApplicationPath(aPathFinder, applicationInfo);
		}

		//---------------------------------------------------------------------------
		public static string GetStandardApplicationPath(WizardApplicationInfo aApplicationInfo)
		{
			return GetStandardApplicationPath(null, aApplicationInfo);
		}
		
		//---------------------------------------------------------------------------
		public string GetStandardApplicationPath()
		{
			return GetStandardApplicationPath(null, applicationInfo);
		}


		#endregion

		#region Module Path

		//---------------------------------------------------------------------------
		public static string GetStandardModulePath(IBasePathFinder aPathFinder, WizardModuleInfo aModuleInfo)
		{
			if (aModuleInfo == null || aModuleInfo.Application == null)
				return String.Empty;
			
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string modulePath = GetStandardApplicationPath(aPathFinder, aModuleInfo.Application);
			if (string.IsNullOrEmpty(modulePath))
				return String.Empty;

			if (aModuleInfo != null && aModuleInfo.Name != null && aModuleInfo.Name.Trim().Length > 0)
			{
				modulePath += Path.DirectorySeparatorChar;
				modulePath += aModuleInfo.Name;
			}

			return modulePath;
		}


		//---------------------------------------------------------------------------
		public static string GetStandardModulePath(WizardModuleInfo aModuleInfo)
		{
			return GetStandardModulePath(null, aModuleInfo);
		}

		#endregion

		#region Library Path
		
		//---------------------------------------------------------------------------
		public static string GetStandardLibraryPath(IBasePathFinder aPathFinder, WizardLibraryInfo aLibraryInfo)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null)
				return String.Empty;

			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string libraryPath = GetStandardModulePath(aPathFinder, aLibraryInfo.Module);
			if (string.IsNullOrEmpty(libraryPath))
				return String.Empty;

			if (aLibraryInfo != null)
			{
				if (aLibraryInfo.SourceFolder != null && aLibraryInfo.SourceFolder.Trim().Length > 0)
				{
					libraryPath += Path.DirectorySeparatorChar;
					libraryPath += aLibraryInfo.SourceFolder;
				}
				else if (aLibraryInfo.Name != null && aLibraryInfo.Name.Trim().Length > 0)
				{
					libraryPath += Path.DirectorySeparatorChar;
					libraryPath += aLibraryInfo.Name;
				}
			}

			return libraryPath;
		}

		//---------------------------------------------------------------------------
		public static string GetStandardLibraryPath(WizardLibraryInfo aLibraryInfo)
		{
			return GetStandardLibraryPath(null, aLibraryInfo);
		}

		#endregion

		#region ReferenceObjects Path
		
		//---------------------------------------------------------------------------
		public string GetStandardReferenceObjectsPath(WizardModuleInfo aModuleInfo)
		{
			return GetStandardReferenceObjectsPath(null, aModuleInfo);
		}
		
		//---------------------------------------------------------------------------
		public static string GetStandardReferenceObjectsPath(IBasePathFinder aPathFinder, WizardModuleInfo aModuleInfo)
		{
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string referenceObjectsPath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(referenceObjectsPath))
				return String.Empty;

			referenceObjectsPath += Path.DirectorySeparatorChar;
			referenceObjectsPath += Generics.ReferenceObjectsFolderName;

			return referenceObjectsPath;
		}

		#endregion

		#region DatabaseScripts Path
		
		//---------------------------------------------------------------------------
		public static string GetStandardDatabaseScriptsPath(IBasePathFinder	aPathFinder, WizardModuleInfo aModuleInfo)
		{
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string databaseScriptsPath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return String.Empty;

			databaseScriptsPath += Path.DirectorySeparatorChar;
			databaseScriptsPath += Generics.DatabaseScriptsFolderName;

			return databaseScriptsPath;
		}

		//---------------------------------------------------------------------------
		public string GetStandardDatabaseScriptsPath(WizardModuleInfo aModuleInfo)
		{
			return GetStandardDatabaseScriptsPath(null, aModuleInfo);
		}
		
		#endregion

		#region ModuleObjects Path
		
		//---------------------------------------------------------------------------
		public static string GetStandardModuleObjectsPath(IBasePathFinder aPathFinder, WizardModuleInfo aModuleInfo)
		{
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string databaseScriptsPath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(databaseScriptsPath))
				return String.Empty;

			databaseScriptsPath += Path.DirectorySeparatorChar;
			databaseScriptsPath += Generics.ModuleObjectsFolderName;

			return databaseScriptsPath;
		}

		//---------------------------------------------------------------------------
		public string GetStandardModuleObjectsPath(WizardModuleInfo aModuleInfo)
		{
			return GetStandardModuleObjectsPath(null, aModuleInfo);
		}

		#endregion

		#region Reports Path

		//---------------------------------------------------------------------------
		public static string GetStandardModuleReportsPath(IBasePathFinder aPathFinder, WizardModuleInfo	aModuleInfo)
		{		
			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string reportsPath = GetStandardModulePath(aPathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(reportsPath))
				return String.Empty;

			reportsPath += Path.DirectorySeparatorChar;
			reportsPath += Generics.ReportsFolderName;

			return reportsPath;
		}

		//---------------------------------------------------------------------------
		public string GetStandardModuleReportsPath(WizardModuleInfo aModuleInfo)
		{
			return GetStandardModuleReportsPath(null, aModuleInfo);
		}

		#endregion

		#region Description Folder

		//---------------------------------------------------------------------------
		public static string GetStandardDocumentDescriptionPath
			(
			IBasePathFinder aPathFinder, 
			WizardDocumentInfo		aDocumentInfo
			)
		{
			if (aDocumentInfo == null || aDocumentInfo.Library == null)
				return String.Empty;

			if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;
			
			string documentDescriptionPath = GetStandardModuleObjectsPath(aPathFinder, aDocumentInfo.Library.Module);
			if (string.IsNullOrEmpty(documentDescriptionPath))
				return String.Empty;

			documentDescriptionPath += Path.DirectorySeparatorChar;
			documentDescriptionPath += aDocumentInfo.Name;
			documentDescriptionPath += Path.DirectorySeparatorChar;
			documentDescriptionPath += Generics.DescriptionFolder;

			return documentDescriptionPath;
		}

		//---------------------------------------------------------------------------
		public string GetStandardDocumentDescriptionPath(WizardDocumentInfo aDocumentInfo)
		{
			return GetStandardDocumentDescriptionPath(null, aDocumentInfo);
		}
	
		#endregion

		#endregion

		#endregion

		///<summary>
		/// GenerateTableCreationStatement
		/// Genera lo statement per la creazione di una tabella, caricando l'apposito template
		///</summary>
		//---------------------------------------------------------------------------
		public bool GenerateTableCreationStatement(WizardTableInfo aTableInfo, out string statement, DBMSType dbType)
		{
			// Il file non esiste oppure lo posso sovrascrivere!
			WizardTableCodeTemplateParser codeParser = new WizardTableCodeTemplateParser(aTableInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleCreateScript.tmpl";
					break;
			}

			statement = string.Empty;

			return codeParser.WriteStatementFromTemplate(tmpl, out statement, Encoding.UTF8);
		}

		///<summary>
		/// GenerateAlterTableStatement
		/// Genera lo statement per la creazione dell'alter di una tabella, caricando l'apposito template
		///</summary>
		//---------------------------------------------------------------------
		public bool GenerateAlterTableStatement
			(
			WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo,
			TableUpdate currentTablesUpdate,
			out string statement,
			DBMSType dbType
			)
		{
			WizardAdditionalColumnsCodeTemplateParser codeParser =
				new WizardAdditionalColumnsCodeTemplateParser(aExtraAddedColumnsInfo, dbType);
			codeParser.CurrentColumnUpdate = currentTablesUpdate;
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);

			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerAdditionalColumnsScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleAdditionalColumnsScript.tmpl";
					break;
			}
			
			statement = string.Empty;
			return codeParser.WriteStatementFromTemplate(tmpl, out statement, Encoding.UTF8);
		}

		///<summary>
		/// GenerateViewCreationStatement
		/// Genera lo statement per la creazione di una View
		///</summary>
		//---------------------------------------------------------------------------
		public bool GenerateViewCreationStatement(SqlView aViewInfo, out string statement, DBMSType dbType)
		{
			// Il file non esiste oppure lo posso sovrascrivere!
			WizardViewCodeTemplateParser codeParser = new WizardViewCodeTemplateParser(aViewInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerViewCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleViewCreateScript.tmpl";
					break;
			}

			statement = string.Empty;
			return codeParser.WriteStatementFromTemplate(tmpl, out statement, Encoding.UTF8);
		}

		///<summary>
		/// GenerateProcedureCreationStatement
		/// Genera lo statement per la creazione di una Procedure
		///</summary>
		//---------------------------------------------------------------------------
		public bool GenerateProcedureCreationStatement(SqlProcedure aProcedureInfo, out string statement, DBMSType dbType)
		{
			WizardProcedureCodeTemplateParser codeParser = new WizardProcedureCodeTemplateParser(aProcedureInfo, dbType);
			codeParser.SubstitutionInProgress += new TBWizardCodeGeneratorEventHandler(this.CodeParser_SubstitutionInProgress);
			
			string tmpl = null;
			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					tmpl = "Tables.SQLServerProcedureCreateScript.tmpl";
					break;
				case DBMSType.ORACLE:
					tmpl = "Tables.OracleProcedureCreateScript.tmpl";
					break;
				}
			
			statement = string.Empty;
			return codeParser.WriteStatementFromTemplate(tmpl, out statement, Encoding.UTF8);
		}

		//---------------------------------------------------------------------------
		public static string GetStandardLibraryPathForCS(IBasePathFinder aPathFinder, WizardLibraryInfo aLibraryInfo)
		{
			if (aLibraryInfo == null || aLibraryInfo.Module == null)
				return String.Empty;

			if (aPathFinder == null)
				aPathFinder = BasePathFinder.BasePathFinderInstance;

			string modPath = GetStandardModulePath(aPathFinder, aLibraryInfo.Module);

			return Path.Combine(modPath, string.Concat(aLibraryInfo.Application.Name, ".", aLibraryInfo.Module.Name, ".",		aLibraryInfo.SourceFolder));
		}

	}
}
