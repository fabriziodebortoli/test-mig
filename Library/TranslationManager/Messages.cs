namespace Microarea.Library.TranslationManager
{
	public class Messages
	{
		private static System.Resources.ResourceManager rm = new System.Resources.ResourceManager ("Microarea.Library.TranslationManager.Messages", System.Reflection.Assembly.GetExecutingAssembly());
		public static string CPPTranslatorToolName { get { return rm.GetString("CPPTranslatorToolName"); } }
		public static string StartProcessingFile { get { return rm.GetString("StartProcessingFile"); } }
		public static string EndProcessingFile { get { return rm.GetString("EndProcessingFile"); } }
		public static string ErrorProcessingFile { get { return rm.GetString("ErrorProcessingFile"); } }
	}
}
