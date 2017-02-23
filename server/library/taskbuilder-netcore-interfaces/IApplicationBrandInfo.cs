

using System.Xml;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IApplicationBrandInfo
	{
		string MenuImage { get; }
		string MenuTitle { get; }
		string Name { get; }
		void SetInfoFromXmlNode(XmlNode aAppBrandInfoXmlNode);
	}
}
