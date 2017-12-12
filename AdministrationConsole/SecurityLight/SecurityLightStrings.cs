using System.Reflection;

namespace Microarea.Console.Plugin.SecurityLight
{
	class GenericStrings
	{
		public static readonly string SecurityLightPlugIn = Assembly.GetExecutingAssembly().GetName().Name;
		public const string SecurityLightNamespace = "Microarea.Console.Plugin.SecurityLight";
	}
}
