using System.Collections;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IBrandLoader
	{
		string FindBrandedStringValue(string propertyName);
		IList GetBrandedKeysInfo();
		string GetBrandedStringBySourceString(string source);
        string GetCompanyName(string aBrandName = null);
        string GetConsoleSplash(string aBrandName = null);
        string GetMenuManagerSplash(string aBrandName = null);
        IBrandInfo GetBrandInfo(string aBrandName = null);
		IApplicationBrandInfo GetApplicationBrandInfo(string aApplicationName);
	}
}
