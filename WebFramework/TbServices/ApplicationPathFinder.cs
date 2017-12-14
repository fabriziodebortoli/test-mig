
namespace Microarea.WebServices.TbServices
{
	/// <summary>
	/// Summary description for ApplicationPathFinder.
	/// </summary>
	public class ApplicationPathFinder
	{
		public static string rootPath = "";
		public static bool   init     =false;
		public ApplicationPathFinder()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static bool IsInit()
		{
			return init;
		}
		//
		public static string RootPath
		{
			get  {return rootPath;}
	    }
		//
		public static string GetCurrentApplicationRoot(string path)
		{
			if ( path != null)
			{
				int standardIndex = path.IndexOf("\\Standard");
				if (standardIndex >0)
					path = path.Substring(0,standardIndex);
			}
			rootPath = path;
			return rootPath;
		}
	}
}
