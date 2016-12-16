using System.Collections;
using System.Xml;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IBrandInfo
	{
		bool AddInfoFromXmlNode(XmlNode aBrandXmlNode, bool overwrite);
		string Company { get; }
		string CompanyMenuLogo { get; }
		string ConsoleSplash { get; }
		IApplicationBrandInfo GetApplicationBrandInfo(string aApplicationName);
        IList GetBrandedKeysInfo();
		object GetPropertyValue(string propertyName);
		bool InfoLoaded { get; }
		bool IsMagoNet();
		bool IsMain { get; }
		string MenuManagerSplash { get; }
		string Name { get; }
		string ProductTitle { get; }
	}
}
