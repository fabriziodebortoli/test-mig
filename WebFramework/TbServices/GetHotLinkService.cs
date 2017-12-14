using System.IO;
using System.Xml;

namespace Microarea.WebServices.TbServices
{
	/// <summary>
	/// Summary description for GetHotLinkService.
	/// </summary>
	public class GetHotLinkService
	{
		private string ServerRoot;
		public GetHotLinkService(string newServerRoot)
		{
			//
			// TODO: Add constructor logic here
			//
			this.ServerRoot = newServerRoot;
		}
		//---------------------------------------------------------------
		public string GetHotLinkDef(string docNamespace, 
			string nsUri, 
			string fieldXPath,
			string companyName
			)
		{
			string rootpath = ServerRoot + @"\Custom\Companies\";
			string AppStr = @"Applications\";

			//Document.ERP.PurchaseOrders.Documents.PurchaseOrd
			char[] splitStr = new char[1];
			splitStr[0] = '.';
			string[] documentPath = docNamespace.Split(splitStr);
			AppStr += documentPath[1] + @"\" + documentPath[2] + @"\ModuleObjects\" + documentPath[4] + @"\ExportProfiles\";
           
			char[] splitStr2 = new char[1];
			splitStr2[0] = '/' ;
            
			string USERS = "Users";
			string ALLUSERS ="AllUsers";
            
			int AllUser = nsUri.IndexOf( ALLUSERS);
         

			string[] nsUriPath = nsUri.Split(splitStr2);
			int uriSeg = nsUriPath.Length ; 
			string UserPath = nsUriPath[uriSeg-2];
			if ( AllUser ==-1)
			{
               UserPath = USERS + @"\" + UserPath; 
			}
			string schemaName = nsUriPath[uriSeg-1];
			int pos =schemaName.IndexOf(".");
			if (pos>0)
				schemaName =schemaName.Substring(0,pos);

			string hotLinkName = "HotKeyLink.xml";
			string htoLinkFilePath = rootpath + companyName + @"\" + AppStr + UserPath + @"\" + schemaName + @"\" + hotLinkName;
			if (!File.Exists(htoLinkFilePath))
				return null;

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(htoLinkFilePath);
			

			XmlNode docNode =xmlDoc.DocumentElement;
			//  
			string fieldXPath1 = fieldXPath;
			fieldXPath1 = fieldXPath1.Replace("/ns:", ".");
			string[] hotNameSpace = fieldXPath1.Split(splitStr);
			int   hotNameSpaceCount =  hotNameSpace.Length;
			
			string  DBTXPath = "";
			for ( int i =1 ;i< documentPath.Length ; i++)
			{
				if ( i ==1)
					DBTXPath = documentPath[i];
				else
					DBTXPath +=  "." + documentPath[i];
	  
			}
												

			DBTXPath = DBTXPath +"." + hotNameSpace[3];
			string  fieldName = hotNameSpace[hotNameSpaceCount -1];
            
			string xPAth = "//DBT[@namespace=\"" + DBTXPath +"\"]/*/Field[@name=\"" + fieldName + "\"]"; 
			
			XmlNode hotLinkNode = docNode.SelectSingleNode(xPAth);
			
			if ( hotLinkNode != null)
			{
				//modify the FielName 
				//modify the FielName 
				hotLinkNode.Attributes["name"].InnerText = fieldXPath;
				
				//Update Field
				XmlNodeList hotLinkResultList = hotLinkNode.SelectNodes("Results/Result");
				if ( hotLinkResultList != null)
				{
					for (int i = 0; i < hotLinkResultList.Count ; i++)
					{
						string oldXDocPath = hotLinkResultList[i].Attributes["documentField"].InnerText;
						oldXDocPath = oldXDocPath.Replace("/" , "/ns:");
						string newXDocPath = "/ns:" + documentPath[4] + "/ns:Data/ns:" + oldXDocPath ;
						hotLinkResultList[i].Attributes["documentField"].InnerText = newXDocPath;
					}
				}
				return hotLinkNode.OuterXml ;
			}
			return "";
           
		}
	}
}
