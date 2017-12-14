using System;
using System.Resources;

namespace Microarea.Library.DependencyManager
{
	public class Strings
	{
		#region Constructors
		//---------------------------------------------------------------------
		private Strings()
		{
		}
		#endregion

		private static ResourceManager resources = new ResourceManager(typeof(Strings));

		public static string InvalidLibrary	{ get { return resources.GetString("InvalidLibrary"); } }
		public static string NoAppsDetected	{ get { return resources.GetString("NoAppsDetected"); } }
	}
}