using System.Xml;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	/// <remarks/>
	public interface IDomUpdater
	{
		//---------------------------------------------------------------------
		/// <remarks/>
		XmlDocument Dom { get; }

		//---------------------------------------------------------------------
		/// <remarks/>
		void AddNode(
			string parentXPathQuery,
			string beforeItemXPathQuery,
			XmlElement nodeToBeAdded
			);

		//---------------------------------------------------------------------
		/// <remarks/>
		XmlNode RemoveNode(string xPathQuery);

		//---------------------------------------------------------------------
		/// <remarks/>
		void ClearDom();

		//---------------------------------------------------------------------
		/// <remarks/>
		void UpdateNodeProperty(
			string propertyValue,
			XmlNodeType propertyXmlNodeType,
			string propertyXmlNodeName,
			string xPathQuery
			);
	}
}
