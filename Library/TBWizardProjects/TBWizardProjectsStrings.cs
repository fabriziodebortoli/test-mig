using System.Reflection;
using System.Resources;

namespace Microarea.Library.TBWizardProjects
{
	/// <summary>
	/// Summary description for TBWizardProjectsStrings.
	/// </summary>
	public class TBWizardProjectsStrings
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.TBWizardProjects.Strings", Assembly.GetExecutingAssembly());

		public static string ExceptionRaisedDuringLoadErrMsg	{ get { return rm.GetString ("ExceptionRaisedDuringLoadErrMsg"); }}
		public static string InvalidProjectFileErrMsg			{ get { return rm.GetString ("InvalidProjectFileErrMsg"); }}
		public static string InvalidApplicationInfoErrMsg		{ get { return rm.GetString ("InvalidApplicationInfoErrMsg"); }}		
		public static string LoadFromFileXmlExceptionErrMsg		{ get { return rm.GetString ("LoadFromFileXmlExceptionErrMsg"); }}
		public static string EmptyApplicationNameErrMsg			{ get { return rm.GetString ("EmptyApplicationNameErrMsg"); }}
		public static string EmptyApplicationShortNameErrMsg	{ get { return rm.GetString ("EmptyApplicationShortNameErrMsg"); }}
		public static string SaveFileXmlExceptionErrMsg			{ get { return rm.GetString ("SaveFileXmlExceptionErrMsg"); }}
		public static string GenericXmlExceptionErrMsg			{ get { return rm.GetString ("GenericXmlExceptionErrMsg"); }}
		public static string IOExceptionErrorMsg				{ get { return rm.GetString ("IOExceptionErrorMsg"); }}
		public static string UnauthorizedAccessExceptionErrorMsg{ get { return rm.GetString ("UnauthorizedAccessExceptionErrorMsg"); }}
		public static string PathTooLongExceptionErrorMsg		{ get { return rm.GetString ("PathTooLongExceptionErrorMsg"); }}
		public static string DirectoryNotFoundExceptionErrorMsg	{ get { return rm.GetString ("DirectoryNotFoundExceptionErrorMsg"); }}
		public static string NotSupportedExceptionErrorMsg		{ get { return rm.GetString ("NotSupportedExceptionErrorMsg"); }}
		public static string InvalidPathStringErrorMsg			{ get { return rm.GetString ("InvalidPathStringErrorMsg"); }}
		public static string EnsureDirectoryExistenceErrorMsg	{ get { return rm.GetString ("EnsureDirectoryExistenceErrorMsg"); }}
		public static string WriteXmlMenuFileErrorMsg			{ get { return rm.GetString ("WriteXmlMenuFileErrorMsg"); }}
		public static string InvalidObjectNameExceptionErrorMsg	{ get { return rm.GetString ("InvalidObjectNameExceptionErrorMsg"); }}
		public static string InvalidClassNameExceptionErrorMsg	{ get { return rm.GetString ("InvalidClassNameExceptionErrorMsg"); }}
		public static string DefaultLibraryNameFmtText			{ get { return rm.GetString ("DefaultLibraryNameFmtText"); }}
		public static string UndefinedDataTypeDescription		{ get { return rm.GetString ("UndefinedDataTypeDescription"); }}
		public static string StringDataTypeDescription			{ get { return rm.GetString ("StringDataTypeDescription"); }}
		public static string ShortDataTypeDescription			{ get { return rm.GetString ("ShortDataTypeDescription"); }}
		public static string LongDataTypeDescription			{ get { return rm.GetString ("LongDataTypeDescription"); }}
		public static string DoubleDataTypeDescription			{ get { return rm.GetString ("DoubleDataTypeDescription"); }}
		public static string MonetaryDataTypeDescription		{ get { return rm.GetString ("MonetaryDataTypeDescription"); }}
		public static string QuantityDataTypeDescription		{ get { return rm.GetString ("QuantityDataTypeDescription"); }}
		public static string PercentDataTypeDescription			{ get { return rm.GetString ("PercentDataTypeDescription"); }}
		public static string DateDataTypeDescription			{ get { return rm.GetString ("DateDataTypeDescription"); }}
		public static string BooleanDataTypeDescription			{ get { return rm.GetString ("BooleanDataTypeDescription"); }}
		public static string EnumDataTypeDescription			{ get { return rm.GetString ("EnumDataTypeDescription"); }}
		public static string TextDataTypeDescription			{ get { return rm.GetString ("TextDataTypeDescription"); }}
		public static string NTextDataTypeDescription			{ get { return rm.GetString ("NTextDataTypeDescription"); } }
		public static string GuidDataTypeDescription			{ get { return rm.GetString ("GuidDataTypeDescription"); }}
		public static string NotEndedInjectionPointErrorMsg		{ get { return rm.GetString ("NotEndedInjectionPointErrorMsg"); }}
		public static string EmptyInjectionPointHeaderMsg		{ get { return rm.GetString ("EmptyInjectionPointHeaderMsg"); }}	
		public static string InjectionPointHeaderNotUniqueMsg	{ get { return rm.GetString ("InjectionPointHeaderNotUniqueMsg"); }}	
		public static string NestedInjectionPointsNotAllowedMsg	{ get { return rm.GetString ("NestedInjectionPointsNotAllowedMsg"); }}	
		public static string NotOpenedInjectionPointsErrorMsg	{ get { return rm.GetString ("NotOpenedInjectionPointsErrorMsg"); }}	
		public static string NotClosedInjectionPointsErrorMsg	{ get { return rm.GetString ("NotClosedInjectionPointsErrorMsg"); }}	
		public static string MissingEnumTypeErrorMsg			{ get { return rm.GetString ("MissingEnumTypeErrorMsg"); }}	
		public static string ClassNamesConflictFmtMessage		{ get { return rm.GetString ("ClassNamesConflictFmtMessage"); }}	
	}
}
