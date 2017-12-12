using System.Collections;
using System.Drawing;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IBrandLoader
	{
        string FindBrandedStringValue(string propertyName);
		IList GetBrandedKeysInfo();
		string GetBrandedStringBySourceString(string source, bool allowNullWhenNotFound = false);
        string GetCompanyName();
        string GetMenuPage();
        Image GetConsoleSplash();
        Image GetMenuManagerSplash();
        Image GetCompanyLogo();
        string GetApplicationBrandMenuTitle(string aApplicationName);
        string GetApplicationBrandMenuImage(string aApplicationName);
    }
}
