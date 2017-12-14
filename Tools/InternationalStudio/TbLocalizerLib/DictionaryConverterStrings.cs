namespace Microarea.Tools.TBLocalizer
{
	class DictionaryConverterStrings
	{
		private static System.Resources.ResourceManager rm = new System.Resources.ResourceManager ("Microarea.Tools.TBLocalizer.DictionaryConverterStrings", System.Reflection.Assembly.GetExecutingAssembly());
		public static string FileNotFound { get { return rm.GetString("FileNotFound"); } }
		public static string ResourceNotFound { get { return rm.GetString("ResourceNotFound"); } }
		public static string LackTranslation { get { return rm.GetString("LackTranslation"); } }
		public static string ErrorImportingDictionary { get { return rm.GetString("ErrorImportingDictionary"); } }
		public static string ImportingDictionary { get { return rm.GetString("ImportingDictionary"); } }
	}
}
