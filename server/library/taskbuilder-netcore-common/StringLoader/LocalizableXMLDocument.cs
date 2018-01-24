using Microarea.Common.NameSolver;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.StringLoader
{

    /// <summary>
    /// Traduce un file XML usando i servizi di TBStringLoader
    /// </summary>
    //================================================================================
    public class LocalizableXmlDocument : XmlDocument
	{
		public const string localizable				= "localizable";
		public const string localize				= "localize";
		public const string baseLocalize			= "baseLocalize";
		public const string namespaceURI			= "urn:TBStringLoader";
		
		private static readonly string localizableAttributesPattern = "//@" + localize;
		private static readonly string localizableNodesPattern = "//node()[@" + localizable + "='true']/text()";

		private const string namespacePrefix		= "sl";
		private const string namespaceURI_TAG		= "xmlns:";
		private const string dictionary				= "dictionary";
		private const string XMLTrue				= "true";
		
		private string					dictionaryPath	= null;
		private string					application		= null;
		private string					module			= null;
		private string					fileName		= null;
		private PathFinder				pathFinder		= null;

		//--------------------------------------------------------------------------------
		public static string NamespaceUri { get { return namespaceURI; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="appName">Nome dell'applicazione a cui appartiene il file XML</param>
		/// <param name="moduleName">Nome del modulo a cui appartiene il file XML</param>
		/// <param name="pathFinder">Oggetto PathFinder necessario per ottenere i percorsi su file system</param>
		//---------------------------------------------------------------------
		public LocalizableXmlDocument(string appName, string moduleName, PathFinder pathFinder) : base()
		{	
			this.application = appName;
			this.module		 = moduleName;
			this.dictionaryPath = null;
			this.pathFinder = pathFinder;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="dictionaryPath">Path della root del dizionario</param>
		/// <param name="pathFinder">Oggetto PathFinder necessario per ottenere i percorsi su file system</param>
		//---------------------------------------------------------------------
		public LocalizableXmlDocument(string dictionaryPath, PathFinder pathFinder) : base()
		{	
			this.dictionaryPath = dictionaryPath; 
			this.pathFinder = pathFinder;
		}

		/// <summary>
		/// Carica il file XML e lo trasforma
		/// </summary>
		/// <param name="file">Il percorso del file da caricare</param>
		//---------------------------------------------------------------------
		public override void Load(string file)
		{
            FileStream stream = File.OpenRead(file);

            base.Load(stream); 
            fileName = file;
			if (pathFinder != null && Helper.Culture != string.Empty)
				LoadTransform();
            stream.Dispose(); // release also xml file handle
		}

		//---------------------------------------------------------------------
		public void LoadXml(string xml, string file)
		{
			base.LoadXml (xml);
			fileName = file;
			if (pathFinder != null && Helper.Culture != string.Empty)
				LoadTransform();
		}

		//---------------------------------------------------------------------
		private void LoadTransform()
		{   
			
			string specificDictionaryPath = string.Empty;
			if (dictionaryPath != null)
			{
				specificDictionaryPath = Helper.GetSpecificDictionaryFilePath(dictionaryPath);
			}
			else if (application != null &&
				module != null &&
				fileName != null &&
				pathFinder != null)
				specificDictionaryPath = Helper.GetSpecificDictionaryFilePath(application, module, fileName, pathFinder);
			else
			{
				Trace.WriteLine("Cannot calculate dictionary path");     
				return;
			}

			DocumentElement.SetAttribute(namespaceURI_TAG + namespacePrefix, namespaceURI);

			foreach (XmlText localizableNode in SelectNodes(localizableNodesPattern))
			{
				string localizedText = LoadXMLString(localizableNode.Value, fileName, specificDictionaryPath);
				if (localizedText == localizableNode.Value)
					continue;
				XmlElement parentNode = (XmlElement)localizableNode.ParentNode;
				parentNode.SetAttribute(parentNode.Name, namespaceURI, localizableNode.Value);
				localizableNode.Value = localizedText;
			}

			foreach (XmlAttribute localizableNode in SelectNodes(localizableAttributesPattern))
			{
				string localizedText = LoadXMLString(localizableNode.Value, fileName, specificDictionaryPath);
				if (localizedText == localizableNode.Value)
					continue;
				localizableNode.OwnerElement.SetAttribute(baseLocalize, namespaceURI, localizableNode.Value);
				localizableNode.Value = localizedText;
			}

		}

		/// <summary>
		/// Salva il file XML preventivamente trasformato con il foglio XSLT associato all'evento di save 
		/// </summary>
		/// <param name="filename">Il percorso del file da salvare</param>
		//---------------------------------------------------------------------
		public override void Save(string file)
		{
			fileName = file;
			if (pathFinder != null && Helper.Culture != string.Empty)
			{
				XmlDocument tmpDoc =  new XmlDocument(NameTable);
				if (SaveTransform(tmpDoc))
					tmpDoc.Save(File.OpenWrite(file));
			}
			else
				base.Save(File.OpenWrite(file));
		}

		//--------------------------------------------------------------------------------
		public void SaveTranslatedContents(string file)
		{
			base.Save(File.OpenWrite(file));
		}

		//---------------------------------------------------------------------
		private bool SaveTransform(XmlDocument output)
		{
			try
			{	
				MemoryStream ms = new MemoryStream();
				base.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);
				output.Load(ms);

				foreach (XmlText localizableNode in output.SelectNodes(localizableNodesPattern))
				{
					XmlElement parentNode = (XmlElement) localizableNode.ParentNode;
					if (parentNode.HasAttribute(parentNode.Name, namespaceURI))
					{
						string baseText = parentNode.GetAttribute(parentNode.Name, namespaceURI);
						localizableNode.Value = baseText;
						parentNode.RemoveAttribute(parentNode.Name, namespaceURI);
					}
				}

				foreach (XmlAttribute localizableNode in output.SelectNodes(localizableAttributesPattern))
				{
					if (localizableNode.OwnerElement.HasAttribute(baseLocalize, namespaceURI))
					{
						string baseText = localizableNode.OwnerElement.GetAttribute(baseLocalize, namespaceURI);
						localizableNode.Value = baseText;
						localizableNode.OwnerElement.RemoveAttribute(baseLocalize, namespaceURI);
					}
				}
				
				return true;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------
		public static string LoadXMLString(string baseString, string fileName, string dictionaryPath)
		{
			if (dictionaryPath == string.Empty)
				return baseString;

			string bareFile = Path.GetFileNameWithoutExtension(fileName).ToLower();
			DictionaryStringBlock dictionary = StringLoader.GetDictionary(dictionaryPath, GlobalConstants.xml, bareFile, bareFile);
			if (dictionary != null)
				return Helper.FindString(dictionary, baseString );

			return baseString; 
		}

		/// <summary>
		/// Restituisce il testo del nodo così com'era prima della traduzione
		/// </summary>
		/// <param name="aNode">Nodo di cui si vuole conoscere il testo "non tradotto"</param>
		/// <param name="text">Testo desiderato</param>
		/// <returns>true o false a seconda dell'esito dell'operazione</returns>
		//-----------------------------------------------------------------------------
		public bool GetLocalizableText(XmlElement aNode, out string text)
		{
			text = "";
			
			try
			{
				if ( aNode != null )
				{
					text = aNode.GetAttribute (namespacePrefix + ":" + aNode.Name);
					return true;
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return false;
			}

			return false;
		}

		/// <summary>
		/// Modifica il testo del nodo com'era prima della traduzione
		/// </summary>
		/// <param name="aNode">Nodo di cui si vuole modificare il testo "non tradotto"</param>
		/// <param name="text">Testo da impostare</param>
		/// <returns>true o false a seconda dell'esito dell'operazione</returns>
		//-----------------------------------------------------------------------------
		public bool SetLocalizableText(XmlElement aNode, string text)
		{
			return SetLocalizableText (aNode, text, null);
		}

		/// <summary>
		/// Modifica il testo del nodo com'era prima della traduzione
		/// </summary>
		/// <param name="aNode">Nodo di cui si vuole modificare il testo "non tradotto"</param>
		/// <param name="text">Testo da impostare</param>
		/// <param name="dictionaryName">Nome del file di dizionario nella foema Applicazione.Modulo.File</param>
		/// <returns>true o false a seconda dell'esito dell'operazione</returns>
		//-----------------------------------------------------------------------------
		public bool SetLocalizableText(XmlElement aNode, string text, string dictionaryName)
		{
			try
			{
				if (DocumentElement != null && aNode!= null)
				{
					DocumentElement.SetAttribute(namespaceURI_TAG + namespacePrefix, namespaceURI);
							
					aNode.SetAttribute(namespacePrefix + ":" + aNode.Name, text);
					aNode.SetAttribute(localizable, XMLTrue);
					if (dictionaryName != null)
						aNode.SetAttribute(dictionary, dictionaryName);

					return true;
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return false;
			}

			return false;
		}
	}
}
