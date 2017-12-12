using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	/// <summary>
	/// Classe che tiene in memoria le informazioni di un client doc
	/// </summary>
	//=========================================================================
	public class ClientDocumentInfo
	{
		private NameSpace	nameSpace;
		private string		title;
		
		/// <summary>
		/// namespace del documento
		/// </summary>
		public NameSpace NameSpace { get { return nameSpace; } }
		
		/// <summary>
		/// Nome del ClientDoc
		/// </summary>
		public string Title { get { return title; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">NameSpace del clientDoc</param>
		//---------------------------------------------------------------------
		public ClientDocumentInfo(NameSpace aNameSpace, string aTitle)
		{
			nameSpace = aNameSpace;
			title = aTitle;
		}
	}

	//=========================================================================
	
	/// <summary>
	/// Classe che mantiene in memoria i dati di un serverdocument
	/// </summary>
	//=========================================================================
	public class ServerDocumentInfo
	{
		private NameSpace	nameSpace;
		private string		type;
		private string		documentClass;
		private ArrayList	clientDocsInfos;

		/// <summary>
		/// namespace del documento
		/// </summary>
		public NameSpace NameSpace { get { return nameSpace; } }
		
		/// <summary>
		/// Tipo del server document es. family
		/// </summary>
		public string Type { get { return type; } }
		
		/// <summary>
		/// Classe del server document
		/// </summary>
		public string DocumentClass { get { return documentClass; } }
		
		/// <summary>
		/// Array dei client doc contenuti
		/// Gli elementi dono di tipo ClientDocumentInfo
		/// </summary>
		public ArrayList ClientDocsInfos { get { return clientDocsInfos; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">namespace del server doc</param>
		/// <param name="aType">tipo del server doc</param>
		/// <param name="aDocumentClass">classe del server doc</param>
		//---------------------------------------------------------------------
		public ServerDocumentInfo(NameSpace aNameSpace, string aType, string aDocumentClass)
		{
			nameSpace = aNameSpace;
			type = aType;
			documentClass = aDocumentClass;
			clientDocsInfos = new ArrayList();
		}

		/// <summary>
		/// Aggiunge un client doc al server doc
		/// </summary>
		/// <param name="aClientDocumentInfo">Il clientDoc da aggiungere</param>
		/// <returns>la posizione in cui è stato inserito il client doc</returns>
		//---------------------------------------------------------------------
		public int AddClientDoc(ClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null)
			{
				Debug.Fail("Error in ServerDocumentInfo.AddClientDoc");
				return -1;
			}

			if (clientDocsInfos == null)
				clientDocsInfos = new ArrayList();

			return clientDocsInfos.Add(aClientDocumentInfo);
		}
	}
	
	//=========================================================================
	public class ClientDocumentsObjectInfo : IClientDocumentsObjectInfo
	{
		private string			filePath;
		private	bool			valid;
		private	string			parsingError;

		private IBaseModuleInfo	parentModuleInfo;
		private ArrayList		serverDocuments;
		
		public	string	FilePath	{ get { return filePath; } }
		public	bool	Valid		{ get { return valid; } }
		public	string	ParsingError{ get { return parsingError; } }

		/// <summary>
		/// Array dei documenti gestiti dal modulo
		/// </summary>
		public IList ServerDocuments { get { return serverDocuments; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aFilePath">path del file documentsObject del modulo</param>
		//---------------------------------------------------------------------
		public ClientDocumentsObjectInfo(string aFilePath, IBaseModuleInfo aParentModuleInfo)
		{
			if (aFilePath == null || aFilePath.Length == 0 || aParentModuleInfo == null)
			{
				Debug.Fail("Error in ClientDocumentsObjectInfo");
			}

			filePath			= aFilePath;
			valid				= true;
			parsingError		= string.Empty;
			parentModuleInfo	= aParentModuleInfo;
			Parse();
		}

		//---------------------------------------------------------------------
		public bool Parse()
		{
			if (!File.Exists(filePath))
				return false;

			LocalizableXmlDocument clientObjectsDocument = new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.PathFinder
				);

			//leggo il file
			clientObjectsDocument.Load(filePath);
			
			//root del documento (ClientDocumentObjects)
			XmlElement root = clientObjectsDocument.DocumentElement;

			//nodo contenitore dei clientDocuments
			XmlNodeList clientDocumentsElements = root.GetElementsByTagName("ClientDocuments");
			if (clientDocumentsElements != null && clientDocumentsElements.Count == 1)
			{
				//documenti server
				XmlNodeList serverDocumentElements = ((XmlElement) clientDocumentsElements[0]).GetElementsByTagName(DocumentsObjectsXML.Element.ServerDocument);
				if (serverDocumentElements == null)
					return true;

				ParseServerDocuments(serverDocumentElements);
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool ParseServerDocuments(XmlNodeList serverDocumentElements)
		{
			if (serverDocumentElements == null)
				return false;

			//inizializzo l'array dei client docs
			if (serverDocuments == null)
				serverDocuments = new ArrayList();
			else
				serverDocuments.Clear();

			NameSpace	nameSpace			= null;
			string		fullNameSpaceString = null;
			string		type				= null;
			string		documentClass		= null;
 
			//scorro i documenti server
			foreach (XmlElement serverDocumentElement in serverDocumentElements)
			{
				//namespace del documento
				fullNameSpaceString = serverDocumentElement.GetAttribute(ClientDocumentObjectsXML.Attribute.Namespace);
				//tipo del documento
				type = serverDocumentElement.GetAttribute(ClientDocumentObjectsXML.Attribute.Type);
				
				//classe del documento
				documentClass = serverDocumentElement.GetAttribute(ClientDocumentObjectsXML.Attribute.Class);

				if (fullNameSpaceString != null && fullNameSpaceString.Length >0)
					nameSpace = new NameSpace(fullNameSpaceString, NameSpaceObjectType.Document);
				
				//creo l'oggetto che tiene le info raccolte
				ServerDocumentInfo aServerDocumentInfo = new ServerDocumentInfo(nameSpace, type, documentClass);
				
				XmlNodeList clientDocumentElements = serverDocumentElement.GetElementsByTagName(DocumentsObjectsXML.Element.ClientDocument);
				if (clientDocumentElements == null)
					return true;

				//scorro i clientDoc
				string fullNameSpace="";
				string title = "";
				NameSpace aNameSpace = null;

				foreach(XmlElement clientDocumentElemnt in clientDocumentElements)
				{
					//namespace del client documento
					fullNameSpace = clientDocumentElemnt.GetAttribute(ClientDocumentObjectsXML.Attribute.Namespace);
					title = clientDocumentElemnt.GetAttribute(ClientDocumentObjectsXML.Attribute.Localize);
		
					aNameSpace = new NameSpace(fullNameSpace, NameSpaceObjectType.Document);
					ClientDocumentInfo aClientDocumentInfo = new ClientDocumentInfo
						(
						aNameSpace, 
						title
						);

					aServerDocumentInfo.AddClientDoc(aClientDocumentInfo);
					serverDocuments.Add(aServerDocumentInfo);
				}
			}
			return true;
		}
	}
}
