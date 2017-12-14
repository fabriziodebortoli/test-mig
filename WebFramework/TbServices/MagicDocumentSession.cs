using System;
using System.Globalization;
using System.Web;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;


namespace Microarea.WebServices.TbServices
{
	/// <summary>
	/// Summary description for MagicDocumentSession.
	/// </summary>
	public class MagicDocumentSession
	{
		private HttpContext	httpContext;
		private TbReportSession Session1;
		
		//--------------------------------------------------------------------------------------
		public MagicDocumentSession(HttpContext httpContext, string authenticationToken, string language)
		{
		    this.httpContext =  httpContext;
		    Init(authenticationToken, language);
		}
		
		//--------------------------------------------------------------------------------------
		private bool Init(string authenticationToken, string language)
		{	
			UserInfo ui = new UserInfo();
			if (!(ui.Login(authenticationToken)))
				return false;
           
            //directly set culture for the current thread with preferred culture
			CultureInfo ci =  new CultureInfo((language == string.Empty) ? ui.LoginManager.PreferredLanguage : language);

			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
			// istanzio la mia sessione di lavoro 
		    Session1 = new TbReportSession(ui);
			bool sessionOk = Session1.LoadSessionInfo();
			return sessionOk;
		}
		//--------------------------------------------------------------------------------------
		public string GetXMLEnum(ushort TagValue)
		{
			try
			{
				Enums enums  =  Session1.Enums;
				EnumTags Tags = enums.Tags;
				EnumTag enumTag = Tags.GetTag(TagValue);

				if (enumTag.Hidden)
					return string.Empty;
			
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.AppendChild(xmlDoc.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\""));
				xmlDoc.AppendChild(xmlDoc.CreateElement("enum"));
				XmlNode docNode = xmlDoc.DocumentElement;
				//create description
				XmlNode descNode = (XmlNode)xmlDoc.CreateElement("description");
				docNode.AppendChild(descNode);
				descNode.AppendChild(xmlDoc.CreateTextNode(enumTag.LocalizedName));
				//
				for ( int i =0 ;i<enumTag.EnumItems.Count ; i++)
				{
					//item
					XmlNode ItemNode = (XmlNode)xmlDoc.CreateElement("item");
					docNode.AppendChild (ItemNode);
                
					EnumItem Item = (EnumItem)enumTag.EnumItems[i];
					if (Item.Hidden)
						continue;
					//desc
					XmlNode desc1Node = (XmlNode)xmlDoc.CreateElement("description");
					ItemNode.AppendChild(desc1Node);
 
					desc1Node.AppendChild ( xmlDoc.CreateTextNode(Item.LocalizedName));

					//stored
					XmlNode storedNode = (XmlNode)xmlDoc.CreateElement("stored");
					ItemNode.AppendChild(storedNode);

					DataEnum dEnum = new DataEnum(enumTag.Value, Item.Value);
					storedNode.AppendChild(xmlDoc.CreateTextNode(dEnum.ToString()));
				}
				return xmlDoc.OuterXml;
			}
			catch(Exception)
			{
				return string.Empty;
			}
		
		}
	}
}
