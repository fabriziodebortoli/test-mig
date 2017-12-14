using System;
using System.IO;
using System.Xml;

namespace Microarea.WebServices.TbServices
{
	/// <summary>
	/// Summary description for QueryEnumService.
	/// </summary>
	public class QueryEnumService
	{
		private string ServerRoot;
		private string rootpath; 
		public QueryEnumService(string newServerRoot)
		{
			//
			// TODO: Add constructor logic here
			//
			this.ServerRoot = newServerRoot;
			rootpath =  ServerRoot + @"\Standard\Applications";
		}
		//
		public string getEnumXML(string language ,string enumNameSpace,int enumPosValue)
		{
			char[] splitstr = {'.'};
 
			string[] enumNampeStr = enumNameSpace.Split(splitstr);
			int count = enumNampeStr.Length;
			if( count>= 2 )
			{
				enumNameSpace = enumNampeStr[count-2] + "/" + enumNampeStr[count-1];
			}
			else
			{
				return "";
			}

			XmlNode enumNode = GetEnumXmlStr (enumNameSpace,enumPosValue);
			XmlNode LangNode = getEnumLanguage (enumNameSpace,language , enumNode);
			return  getEnumXml (enumNode, LangNode ); 
		}
		//Get Enum Definition
		private XmlNode GetEnumXmlStr(string enumNameSpace,int enumPosValue)
		{
			//string rootpath = ServerRoot + @"\Standard\Applications";
			string FileObjectname = @"ModuleObjects\Enums.xml";
			string Path = rootpath + "/" + enumNameSpace + "/" +  FileObjectname;
            if (!File.Exists(Path))
				return null;


			StreamReader sr = File.OpenText(Path);
			String xmlStr ="";
			String input;
			while ((input = sr.ReadLine()) !=  null) 
			{
				xmlStr = xmlStr + input ;
			}
			sr.Close();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlStr);
			//
			XmlNode rootNode = xmlDoc.DocumentElement;
			XmlNode enumNode = rootNode.SelectSingleNode("/Enums/Tag[@value=\"" + enumPosValue + "\"]");
			return enumNode;
		}
        
		//Get Language forEnum
		private XmlNode getEnumLanguage(string enumNameSpace,string language,XmlNode enumNode)
		{
				//get language value
				string TagName = "";
				foreach( XmlAttribute it in enumNode.Attributes )
				{
					if (it.LocalName.Equals("name"))
					{
						TagName = it.InnerText;
					}
				}

				string languagePath = rootpath + "/" + enumNameSpace + "/Dictionary/" + language + "/other/Enums.xml";
				
				if (!File.Exists(languagePath))
					return null;
				
				StreamReader sr1 = File.OpenText(languagePath);
				String xmlStr1 ="";
				String input1;
				while ((input1 = sr1.ReadLine()) !=  null) 
				{
					xmlStr1 = xmlStr1 + input1 ;
				}
				sr1.Close();

				XmlDocument xmlDoc1 = new XmlDocument();
				xmlDoc1.LoadXml(xmlStr1);
				//
				XmlNode rootNode1 = xmlDoc1.DocumentElement;
				XmlNode enumNode1 = rootNode1.SelectSingleNode("/enums/enum[@name=\"" + TagName + "\"]");
				return enumNode1;

		}
		//create new xml for enum
		private string getEnumXml(XmlNode enumNode, XmlNode languageNode)
		{
			if (enumNode == null)
				return "";
             
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\""));
			xmlDoc.AppendChild(xmlDoc.CreateElement("enum"));
			XmlNode docNode = xmlDoc.DocumentElement;
			//create description
			XmlNode descNode = (XmlNode)xmlDoc.CreateElement("description");
			docNode.AppendChild(descNode);
			//create item node
			for( int i = 0 ; i < enumNode.ChildNodes.Count ; i++)
			{
				XmlNode xmlNode = enumNode.ChildNodes[i];
				if ( xmlNode.NodeType == XmlNodeType.Text) //hanlde description
				{
					string defalut = xmlNode.InnerText;
					string NodeTag = getAttribute (enumNode, "name");
					string langDesc = getLanguageAttribute(languageNode,NodeTag);
					if ( langDesc == null || langDesc.Trim().Length == 0)
						langDesc = defalut;
					descNode.AppendChild(xmlDoc.CreateTextNode(langDesc));

				}
				else if ( xmlNode.NodeType == XmlNodeType.Element ) //handle Item
				{
					XmlNode ItemNode = (XmlNode)xmlDoc.CreateElement("item");
					docNode.AppendChild (ItemNode);

					XmlNode desc1Node = (XmlNode)xmlDoc.CreateElement("description");
					ItemNode.AppendChild(desc1Node);
					string name = getAttribute(xmlNode,"name");
					string langDesc = getLanguageAttribute(languageNode,name);
					if ( langDesc == null || langDesc.Trim().Length == 0)
						langDesc = name;
					desc1Node.AppendChild ( xmlDoc.CreateTextNode(langDesc));
				     
					XmlNode storedNode = (XmlNode)xmlDoc.CreateElement("stored");
					ItemNode.AppendChild(storedNode);
					string storedValue = getAttribute(xmlNode,"stored");
					storedNode.AppendChild(xmlDoc.CreateTextNode(storedValue));
				}
			}
	              
			return xmlDoc.OuterXml;
		}
		//Get Attribute Value
		private string getAttribute(XmlNode xmlNode,string attr)
		{
			foreach( XmlAttribute it in xmlNode.Attributes )
			{
				if (it.LocalName.Equals(attr))
				{
					return it.InnerText;
				}
			}
			return "";
		}
		private string getLanguageAttribute(XmlNode languageNode,string attr)
		{
			if (languageNode == null)
				return attr;
			XmlNode xmlNode = languageNode.SelectSingleNode("string[@base=\"" + attr + "\"]");
			if (xmlNode != null)
			{ 
				return getAttribute(xmlNode,"target");
			}
			return attr;
		}
	}
}
