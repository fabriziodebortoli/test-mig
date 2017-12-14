using System;
using System.IO;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for ResourceIndexDocument.
	/// </summary>
	//================================================================================
	public class ResourceIndexDocument : DictionaryDocument
	{
		string root;

		//--------------------------------------------------------------------------------
		public ResourceIndexDocument(string root)
		{
			this.root = root;
		}

		//---------------------------------------------------------------------
		public bool IsIndexedType(string type)
		{
			return type == AllStrings.dialog ||	type == AllStrings.menu || type == AllStrings.stringtable;
		}

		//---------------------------------------------------------------------
		internal override void SaveAndLogError(Logger logWriter)
		{
			base.SaveAndLogError(logWriter, CommonFunctions.GetResourceIndexPath(root));
		}

		//---------------------------------------------------------------------
		internal bool LoadIndex()
		{		
			string fileIndex = "";
			try
			{
				fileIndex = CommonFunctions.GetResourceIndexPath(root);
				if (!File.Exists(fileIndex))
					return false;
				currentDocument.Load(fileIndex);
				FileName = fileIndex;
				rootNode = currentDocument.DocumentElement;
				return true;
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// (hrc)Aggiungge una risorsa-id al file di indice delle risorse.
		/// </summary>
		/// <param name="name">nome della risorsa</param>
		/// <param name="id">id della risorsa</param>
		/// <param name="url">file di dizionario nel quale è segnata la risorsa</param>
		/// <param name="typeOfResource">tipologia di risorsa</param>
		//---------------------------------------------------------------------
		internal void AddResource(string name, double id, string url, string typeOfResource, Logger logWriter)
		{		
			XmlElement resNode = rootNode.SelectSingleNode(typeOfResource) as XmlElement;
			
			if (resNode == null)
			{
				resNode = currentDocument.CreateElement(typeOfResource);
				rootNode.AppendChild(resNode);
			}
			
			if (resNode.SelectSingleNode(AllStrings.resource + CommonFunctions.XPathWhereClause(AllStrings.name, name)) != null)
			{
				logWriter.WriteLog(string.Format("Resource index item already defined: {0}", name), TypeOfMessage.error);
				return;
			}

			XmlElement resource	= currentDocument.CreateElement(AllStrings.resource);

			resource.SetAttribute(AllStrings.name , name);

			resource.SetAttribute(AllStrings.id , id.ToString());
				
			resource.SetAttribute(AllStrings.url, url);

			resNode.AppendChild(resource);

			modified = true;
		}

		//--------------------------------------------------------------------------------
		internal XmlNodeList GetResourceIndexItems(string type)
		{
			string xPath = string.Format
				(
				"//{0}/{1}",
				type,
				AllStrings.resource
				);
			
			return currentDocument.SelectNodes(xPath);
		}

		//--------------------------------------------------------------------------------
		internal uint GetResourceIdByName(string type, string name)
		{
			string xPath = string.Format
				(
				"//{0}/{1}{2}/@{3}",
				type,
				AllStrings.resource,
				CommonFunctions.XPathWhereClause(AllStrings.name, name),
				AllStrings.id 
				);
			
			XmlNode thisNode =  currentDocument.SelectSingleNode(xPath);
			
			if (thisNode == null)
				return 0;
			
			uint id = 0;
			try
			{
				id = uint.Parse(thisNode.Value); 
			}
			catch (FormatException ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message);
				id = 0;
			}

			return id;
		}

		//--------------------------------------------------------------------------------
		internal string GetResourceNameById(string type, string id)
		{
			string xPath = string.Format
				(
				"//{0}/{1}{2}/@{3}",
				type,
				AllStrings.resource,
				CommonFunctions.XPathWhereClause(AllStrings.id, id),
				AllStrings.name 
				);
			XmlNode thisNode =  currentDocument.SelectSingleNode (xPath);
			
			if (thisNode == null)
				return null;
			
			return thisNode.Value; 
		}
	}
}
