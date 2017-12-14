using System.Xml;

namespace Microarea.TaskBuilderNet.Interfaces
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
