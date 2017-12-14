using System;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;

namespace Microarea.Console.SecurityAdminPlugIn
{
	//=========================================================================
	/// <summary>
	/// Classe contenente le informazioni relative agli oggetti Function presenti
	/// nel file .xml
	/// </summary>
	public class Function
	{
		/// <summary>
		/// Valore dell'attributo type
		/// </summary>
		private		string		type;
		/// <summary>
		/// Valore dell'attributo nameSpace
		/// </summary>
		private		NameSpace	nameSpace;
		/// <summary>
		/// Valore dell'attributo localize
		/// </summary>
		private		string		localize;
		
		/// <summary>
		/// Get del valore dell'attributo NameSpace
		/// </summary>
		public NameSpace	NameSpace	{ get { return nameSpace; } }
		/// <summary>
		/// Get del valore dell'attributo type
		/// </summary>
		public string		Type		{ get { return type		; } }
		/// <summary>
		/// Get del valore dell'attributo localize
		/// </summary>
		public string		Localize	{ get { return localize ; } }
		

		//---------------------------------------------------------------------
		/// <summary>
		/// Costruttore nel quale valorizzo i Data Member
		/// </summary>
		/// <param name="aNameSpace"></param>
		/// <param name="atype"></param>
		/// <param name="alocalize"></param>
		public Function(NameSpace aNameSpace, string atype, string alocalize)
		{
			nameSpace	= aNameSpace;
			type		= atype;
			localize	= alocalize;
		}
		//---------------------------------------------------------------------
	}
	//=========================================================================
	/// <summary>
	/// Classe che parsa i file contenuti nella cartella ReferenceObjects dei moduli
	/// </summary>
	public class ReferenceObjectsInfo
	{
		/// <summary>
		/// Path e node del file
		/// </summary>
		private string		filePath;
		/// <summary>
		/// Indica se ci sono stati degli errori durante il Parsing del file
		/// </summary>
		private	bool		valid;
		/// <summary>
		/// Modulo contenete il file
		/// </summary>
		private ModuleInfo	parentModuleInfo;
		/// <summary>
		/// Array dei HotKeyLink ricavati da parsing
		/// </summary>
		private ArrayList	hotKeyLinkArray;
		/// <summary>
		/// Diagnostic degli eventuali errori di Parsing
		/// </summary>
		private Diagnostic	diagnostic	= null;
		
		/// <summary>
		/// Get del valid
		/// </summary>
		public	bool		Valid				{ get { return valid; } }
		/// <summary>
		/// Get del modulo padre del file
		/// </summary>
		public  ModuleInfo	ParentModuleInfo	{ get { return parentModuleInfo; } }
		/// <summary>
		/// Get del Diagnostic del parsing
		/// </summary>
		public	Diagnostic	Diagnostic			{ get { return  diagnostic; }}
		

		/// <summary>
		/// Array dei HotKeyLinks
		/// </summary>
		public ArrayList	HotKeyLinks { get { return hotKeyLinkArray; }  }
		
		/// <summary>
		/// Costruttore che è capace a leggere un hotlink rappresentato dal namespace
		/// </summary>
		/// <param name="nsHotlink">namespace dell'hotlink richiesto</param>
		//---------------------------------------------------------------------
		public ReferenceObjectsInfo(NameSpace nsHotlink, ModuleInfo aParentModuleInfo)
		{
			diagnostic	= new Diagnostic("DiagnosticReferenceObjectParser");
			if (nsHotlink == null || !nsHotlink.IsValid())
			{
				SetError(ErrorStrings.NoHotKeyLinks);
			}

			filePath			= aParentModuleInfo.GetReferenceObjectFileName (nsHotlink);
			valid				= true;
			parentModuleInfo	= aParentModuleInfo;
			Parse();
		}

		/// <summary>
		/// Costruttore che è capace a leggere tutti gli hotlinks del modulo
		/// </summary>
		/// <param name="aFilePath">path del file DatabaseObjects del modulo</param>
		//---------------------------------------------------------------------
		public ReferenceObjectsInfo(ModuleInfo aParentModuleInfo)
		{
			diagnostic	= new Diagnostic("DiagnosticReferenceObjectParser");
			if (aParentModuleInfo == null)
			{
				SetError(ErrorStrings.NoHotKeyLinks);
			}

			parentModuleInfo	= aParentModuleInfo;
			valid				= true;
	
			filePath = parentModuleInfo.GetReferenceObjectsPath();
			if (!Directory.Exists(filePath))
				return;

			string[] hklfiles = Directory.GetFiles (filePath, NameSolverStrings.MaskFileXml); 	
			
			foreach (string file in hklfiles)
			{
				filePath = file;
				Parse();
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Legge il file e crea gli array di HotKeyLink 
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		public bool Parse()
		{
			string nameSpace = "";
			string type		 = "";
			string localize  = "";

			if (hotKeyLinkArray == null)
				hotKeyLinkArray = new ArrayList();
			if	(
				!File.Exists(filePath)		|| 
				parentModuleInfo == null	|| 
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument referenceObjectsDocument = 
				new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.PathFinder
				);

			try
			{
				//leggo il file
				if (!File.Exists(filePath))
				{
					SetError(ErrorStrings.FileNotFound);
					return false;
				}

				referenceObjectsDocument.Load(filePath);
				// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella 
				// con i parametri giusti
				XmlNode root = referenceObjectsDocument.DocumentElement;

				if (root != null )
				{
					string xpath = string.Format
						(
						"/{0}/{1}",
						ReferenceObjectsXML.Element.HotKeyLink,
						ReferenceObjectsXML.Element.Function
						);

					XmlNodeList HotKeyLinks = root.SelectNodes(xpath);
					if (HotKeyLinks == null) 
					{
						SetWarning(ErrorStrings.NoHotKeyLinks);
						return false;
					}
					XmlAttribute typeAttribute = null;
					foreach (XmlElement HotKeyLink in HotKeyLinks)
					{
						nameSpace		= HotKeyLink.GetAttributeNode( ReferenceObjectsXML.Attribute.Namespace).Value;
						typeAttribute	= HotKeyLink.GetAttributeNode( ReferenceObjectsXML.Attribute.Type);
						if (typeAttribute == null) 
							type = "string";
						else
							type = typeAttribute.Value;
						localize	= HotKeyLink.GetAttributeNode( ReferenceObjectsXML.Attribute.Localize).Value;
						
						NameSpace aNameSpace = new NameSpace(nameSpace, NameSpaceObjectType.Hotlink);
						Function aFunction = new Function(aNameSpace, type, localize);
						hotKeyLinkArray.Add (aFunction);
					}
				}
			}
			catch(XmlException err)
			{
				SetError(err.Message, err.LineNumber);
				valid = false;
				return false;
			}
			catch(Exception e)
			{
				SetError(e.Message);
				valid = false;
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Inserisce l'errore nel Diagnostic
		/// </summary>
		/// <param name="explain"></param>
		public void SetError(string explain)
		{   
			diagnostic.Set(DiagnosticType.Error, explain) ;
		}	

		//---------------------------------------------------------------------
		/// <summary>
		/// Inserisce l'errore nel Diagnostic indicando anche la riga del Log
		/// </summary>
		/// <param name="explain"></param>
		/// <param name="line"></param>
		public void SetError(string explain, long line)
		{   
			ExtendedInfo info = new ExtendedInfo();

			info.Add("Line", line);
			info.Add("FilePath", filePath);

			diagnostic.Set(DiagnosticType.Error, this.ParentModuleInfo.Name + "   " + explain, info);
		}	

		//---------------------------------------------------------------------
		/// <summary>
		/// Cancella dal Diagnostic tutti i messaggi con tipo = Error
		/// </summary>
		public void ClearError ()
		{
			diagnostic.Clear(DiagnosticType.Error);
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Cancella dal Diagnostic tutti i messaggi con tipo = Warning
		/// </summary>
		/// <param name="explain"></param>
		public void SetWarning(string explain)
		{   
			diagnostic.Set(DiagnosticType.Warning, explain);
		}	

	}
	//=========================================================================
	
}
