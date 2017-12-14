using System.Collections;
using System.Xml;

namespace Microarea.TaskBuilderNet.Interfaces
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
		bool IsMain { get; }
		string MenuManagerSplash { get; }
		string ProductTitle { get; }
	}
}
