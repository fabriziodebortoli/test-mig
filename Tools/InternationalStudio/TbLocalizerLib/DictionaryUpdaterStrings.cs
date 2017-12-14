namespace Microarea.Tools.TBLocalizer
{
	class DictionaryUpdaterStrings
	{
		private static System.Resources.ResourceManager rm = new System.Resources.ResourceManager ("Microarea.Tools.TBLocalizer.DictionaryUpdaterStrings", System.Reflection.Assembly.GetExecutingAssembly());
		public static string DictionaryAlreadyExists { get { return rm.GetString("DictionaryAlreadyExists"); } }
		public static string BeginExport { get { return rm.GetString("BeginExport"); } }
		public static string EndExport { get { return rm.GetString("EndExport"); } }
		public static string Begin { get { return rm.GetString("Begin"); } }
		public static string End { get { return rm.GetString("End"); } }
		public static string CreatedProjectFile { get { return rm.GetString("CreatedProjectFile"); } }
		public static string CreatedSolutionFile { get { return rm.GetString("CreatedSolutionFile"); } }
		public static string ProjectAdded { get { return rm.GetString("ProjectAdded"); } }
		public static string RenamedFile { get { return rm.GetString("RenamedFile"); } }
		public static string StartITCompiling { get { return rm.GetString("StartITCompiling"); } }
		public static string CompilingPrj { get { return rm.GetString("CompilingPrj"); } }
		public static string EndITCompiling { get { return rm.GetString("EndITCompiling"); } }
		public static string EndConvertBase { get { return rm.GetString("EndConvertBase"); } }
		public static string StartConvertBase { get { return rm.GetString("StartConvertBase"); } }
		public static string EmptyString { get { return rm.GetString("EmptyString"); } }
		public static string DataNotAvailable { get { return rm.GetString("DataNotAvailable"); } }
		public static string NoDictionaryInvolved { get { return rm.GetString("NoDictionaryInvolved"); } }
		public static string NoNodeInvolved { get { return rm.GetString("NoNodeInvolved"); } }
		public static string NoFileInvolved { get { return rm.GetString("NoFileInvolved"); } }
		public static string DamagedFile { get { return rm.GetString("DamagedFile"); } }
		public static string StringAlreadyTranslated { get { return rm.GetString("StringAlreadyTranslated"); } }
		public static string CannotReplaceFile { get { return rm.GetString("CannotReplaceFile"); } }
		public static string CannotFindAppInfo { get { return rm.GetString("CannotFindAppInfo"); } }
	}
}
