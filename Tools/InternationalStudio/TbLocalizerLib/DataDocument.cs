using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.XmlPersister;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{	
	/// <summary>Gestisce la visualizzazione dell'avanzamento del parsing.</summary>
	public delegate void IncrementEventHandler(object sender, IncrementEventArgs e);
	public delegate void FileNameChangedEventHandler(object sender, FileNameChangedEventArgs args);

	/// <summary>
	/// Gestisce in lettura e scrittura i file xml necessari al TBLocalizer.
	/// </summary>
	//========================================================================
	public abstract class DataDocument
	{
		/// <summary>Documento xml sul quale vengono eseguite le operazioni di lettura e scrittura</summary>
		internal LocalizerDocument	currentDocument;
		/// <summary>path del file xml relativo al documento</summary>
		private string			fileName		= null;
		/// <summary>Xml element che è la root del documento xml</summary>
		internal XmlElement		rootNode;
		/// <summary>Xml element child della root element</summary>
		internal XmlElement		resource;
		/// <summary> Specifica se il file è stato modificato dal momento della creazione o dall'ultimo salvataggio. </summary> 
		internal bool			modified	= false;
	
		public event FileNameChangedEventHandler FileNameChanged;
		
		//---------------------------------------------------------------------
		public  DataDocument()
		{
			currentDocument = new LocalizerDocument();
		}
	
		//--------------------------------------------------------------------------------
		public DataDocument(LocalizerDocument doc, XmlElement rootNode)
		{
			this.currentDocument = doc;
			this.rootNode = rootNode;
		}

		//--------------------------------------------------------------------------------
		public LocalizerDocument Document { get { return currentDocument; } }
		//--------------------------------------------------------------------------------
		public XmlElement Resource { get { return resource; } }
		//--------------------------------------------------------------------------------
		public XmlElement Root { get { return rootNode; } }
		
		//--------------------------------------------------------------------------------
		public string FileName 
		{
			get
			{
				return fileName;
			}
			set 
			{
				if (fileName == value) return;

				FileNameChangedEventArgs args = new FileNameChangedEventArgs(fileName, value);
				fileName = value; 

				if (FileNameChanged != null) 
					FileNameChanged(this, args); 
			}
		}

		/// <summary>
		/// (all)Inizializza un documento xml con declaration e root.
		/// </summary>
		/// <param name="root">nome del tag root</param>
		//---------------------------------------------------------------------
		internal  void InitDocument(string root)
		{
			InitDocument(root, null, null, null, null);
		}

		/// <summary>
		/// (all)Inizializza un documento xml(o ne carica uno esistente)con declaration, root e schema.
		/// </summary>
		/// <param name="root">nome del tag root</param>
		/// <param name="dsSchema">schema da inserire nel file xml</param>
		/// <param name="fileToLoad">eventuale file da caricare se esiste</param>
		//---------------------------------------------------------------------
		internal  void InitDocument(string root, DataSet dsSchema, string fileToLoad)
		{
			InitDocument(root, null, dsSchema, fileToLoad, null);
		}

		//---------------------------------------------------------------------
		internal  void InitDocument(LocalizerDocument docToLoad)
		{
			Load(docToLoad);
		}

		/// <summary>
		/// (all)Inizializza un documento xml(o lo carica se esiste)con declaration, 
		///root(attribute) e schema.
		/// </summary>
		/// <param name="root">nome del tag root</param>
		/// <param name="typeAttribute">valore dell'attributo 'type' della root</param>
		/// <param name="dsSchema">schema da inserire nel file xml</param>
		/// <param name="fileToLoad">eventuale file da caricare se esiste</param>
		//---------------------------------------------------------------------
		internal  void InitDocument(string root, string typeAttribute, DataSet dsSchema, string fileToLoad, string xslFile)
		{
			if (fileToLoad != null && File.Exists(fileToLoad) && Load(fileToLoad))
				return;

			if (currentDocument.DocumentElement != null)
				return;
			
			XmlDeclaration declaration = currentDocument.CreateXmlDeclaration
				(AllStrings.version, AllStrings.encoding, null);
			currentDocument.AppendChild(declaration);
			
			if (xslFile != null && xslFile != string.Empty)
				SetXsl(xslFile);

			rootNode = currentDocument.CreateElement(root);
			currentDocument.AppendChild(rootNode);
			if (typeAttribute != null)
			{
				XmlAttribute attribute = currentDocument.CreateAttribute(AllStrings.type);
				attribute.Value = typeAttribute.ToLower();
				rootNode.Attributes.Append(attribute);
			}
			if (dsSchema != null)
			{
				try
				{
					LocalizerDocument schemaDoc = new LocalizerDocument ();
					schemaDoc.LoadXml(dsSchema.GetXmlSchema ());
					XmlNode schemaNode = currentDocument.ImportNode(schemaDoc.DocumentElement, true);
					rootNode.AppendChild(schemaNode);
				}
				catch (Exception exc)
				{
					MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, "DataDocument - InitDocument");
				}
			}

			modified = true;
		}

		//-----------------------------------------------------------------
		public void SetXsl(string xslPath)
		{
			string text = string.Format("type='text/xsl' href='{0}'", xslPath);
			XmlProcessingInstruction newPI = currentDocument.CreateProcessingInstruction("xml-stylesheet", text);
			currentDocument.AppendChild(newPI);
		}

		/// <summary>
		/// (all)Carica un file nel currentDocument.
		/// </summary>
		/// <param name="file">percorso del file da caricare</param>
		//---------------------------------------------------------------------
		internal virtual bool Load(string file)
		{
			currentDocument.Load(file);
			FileName = file;
			rootNode = currentDocument.DocumentElement;
			return true;
		}

		/// <summary>
		/// (all)Carica un dom nel currentDocument.
		/// </summary>
		/// <param name="doc">document da caricare</param>
		//---------------------------------------------------------------------
		internal  bool Load(LocalizerDocument doc)
		{
			try
			{
				currentDocument = doc;
				FileName = String.Empty;
				rootNode = currentDocument.DocumentElement;
				return true;
			}
			catch (Exception exc)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, Strings.WarningCaption);
				return false;
			}
		}

		/// <summary>
		/// (all)Chiude l'elemento resource solo se ha figli.
		/// </summary>
		//---------------------------------------------------------------------
		internal  void CloseResource()
		{
			if (rootNode != null && 
				resource != null && 
				resource.HasChildNodes &&
				resource.ParentNode != rootNode
				)
			{
				rootNode.AppendChild(resource);
				modified = true;
			}
		}

		/// <summary>
		/// (all)Scrive gli elementi resource del file xml di dizionario.
		/// </summary>
		/// <param name="typeOfResource">nome che si vuole dare al nodo</param>
		/// <param name="idd">attributo name del nodo</param>
		/// <param name="validTag">se true verrà messo il tag valid</param>
		//---------------------------------------------------------------------
		internal  XmlElement WriteResource(string typeOfResource, string idd)
		{
			if (idd == null) 
			{
				Debug.Fail("idd can't be null");
				return resource;
			}
			
			resource = rootNode.SelectSingleNode
				(
				typeOfResource + CommonFunctions.XPathWhereClause(AllStrings.name, idd)
				) as XmlElement;
			
			if (resource != null ) 	
				return resource;
			
			resource = currentDocument.CreateElement(typeOfResource);
			if (idd != null)
			{
				XmlAttribute name = currentDocument.CreateAttribute(AllStrings.name);
				name.Value		  = idd ;
				resource.Attributes.SetNamedItem(name);
			}

			modified = true;
			return resource;
		}

		/// <summary>
		/// (all)Elimina tutti i nodi dal documento corrente.
		/// </summary>
		/// <param name="mod">specifica se settare il file come mai salvato</param>
		//---------------------------------------------------------------------
		internal virtual void ClearDocument()
		{	
			currentDocument.RemoveAll();
			modified = false;
			rootNode = null;
			resource = null;
			FileName = string.Empty;
		}

		/// <summary>
		/// (all)Dato il nodo restituisce l'attributo richiesto.
		/// </summary>
		/// <param name="n">nodo interessato</param>
		/// <param name="attrib">attributo richiesto</param>
		//---------------------------------------------------------------------
		internal  XmlNode GetAttributeOfNode(XmlNode n, string attrib)
		{
			return n.Attributes.GetNamedItem(attrib);
		}

		//filepath é il nome del file da salvare, se è stringa vuota 
		//vuol dire che sto salvando il file che ho in canna.
		//writeINI specifica se a seguito del salvataggio devo popolare 
		//il file tblocalizer.tbl (quindi quando salvo il file di indice)
		//allowNoChild specifica se permetto al file xml di avere una root 
		//senza child (file della solution e tblocalizer.tbl)
		/// <summary>
		///Salva un file xml di dizionario giá salvato in precedenza, ritorna un messaggio di errore o stringa vuota.
		/// </summary>
		//---------------------------------------------------------------------
		internal  string  SaveXML()
		{	
			return SaveXML(String.Empty,  false);
		}

		/// <summary>
		/// Salva un file xml di dizionario per la prima volta, ritorna un messaggio di errore o stringa vuota.
		/// </summary>
		/// <param name="filepath"> nome del file da salvare, se è stringa vuota 
		///							vuol dire che sto salvando il file che ho in canna.</param>
		//---------------------------------------------------------------------
		internal  string SaveXML(string filepath)
		{	
			return SaveXML(filepath, false);
		}

		/// <summary>
		/// Salva un file xml giá salvato in precedenza, ritorna un messaggio di errore o stringa vuota.
		/// </summary>
		/// <param name="allowNoChild"> specifica se permetto al file xml di avere una root 
		///								senza child (es: file della solution e tblocalizer.tbl)</param>
		//---------------------------------------------------------------------
		internal  string SaveXML( bool allowNoChild)
		{	
			return SaveXML(String.Empty, allowNoChild);
		}

		/// <summary>
		/// Salva un file xml, ritorna un messaggio di errore o stringa vuota. 
		/// </summary>
		/// <param name="filepath"> nome del file da salvare, se è stringa vuota 
		///							vuol dire che sto salvando il file che ho in canna.</param>
		/// <param name="allowNoChild">	specifica se permetto al file xml di avere una root 
		///								senza child (es: file della solution e tblocalizer.tbl)</param>
		//---------------------------------------------------------------------
		internal virtual string SaveXML(string filepath, bool allowNoChild)
		{	
			if (filepath != String.Empty) FileName = filepath;
			if (FileName == String.Empty || FileName == null) return Strings.EmptyFileName;
			if (modified)
			{
				//non salvo se non ci sono child o se c'`e solo lo schema
				if
					(
					rootNode == null || 
					(!rootNode.HasChildNodes && !allowNoChild) || 
					(rootNode.FirstChild != null && 
					rootNode.FirstChild.Name == AllStrings.schema &&
					rootNode.ChildNodes.Count == 1)
					)
					return String.Empty;
				
				if (!Directory.Exists(Path.GetDirectoryName(FileName)))
				{
					try
					{
						Directory.CreateDirectory((Path.GetDirectoryName(FileName)));
					}
					catch (Exception) 
					{
						return  String.Format(Strings.FolderCreationError, Path.GetDirectoryName(FileName));
					} 
				}

				try
				{
					currentDocument.Save(FileName);
				}
				catch (UnauthorizedAccessException exc) 
				{
					return exc.Message;
				} 
				catch (Exception) 
				{
					return String.Format(Strings.SaveFileError, FileName);
				} 
				modified = false;
			}
			return String.Empty;
		}

		/// <summary>
		/// Salva il file xml con SaveXML e mostra gli eventuali errori in messagebox.
		/// </summary>
		/// <param name="caption">titolo della messageBox</param>
		/// <param name="fileName">path del file da salvare</param>
		//---------------------------------------------------------------------
		internal  void SaveAndShowError(string caption, string fileName)
		{
			string message = SaveXML(fileName);
			if (message != String.Empty) MessageBox.Show(DictionaryCreator.ActiveForm, message, caption);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e mostra gli eventuali errori in messagebox.
		/// </summary>
		/// <param name="caption">titolo della messageBox</param>
		/// <param name="savexml">specifica se permetto di salvare anche se la root non ha children</param>
		//---------------------------------------------------------------------
		internal  void SaveAndShowError(string caption, bool savexml)
		{
			string message = SaveXML(savexml);
			if (message != String.Empty) MessageBox.Show(DictionaryCreator.ActiveForm, message, caption);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e mostra gli eventuali errori in messagebox.
		/// </summary>
		/// <param name="caption">titolo della messageBox</param>
		/// <param name="fileName">path del file da salvare</param>
		/// <param name="savexml">specifica se permetto di salvare anche se la root non ha children</param>
		//---------------------------------------------------------------------
		internal  void SaveAndShowError(string caption,string fileName, bool savexml)
		{
			string message = SaveXML(fileName, savexml);
			if (message != String.Empty) MessageBox.Show(DictionaryCreator.ActiveForm, message, caption);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e restituisce gli eventuali errori
		/// </summary>
		/// <param name="fileName">path del file da salvare</param>
		/// <param name="savexml">specifica se permetto di salvare anche se la root non ha children</param>
		//---------------------------------------------------------------------
		internal string SaveAndReturnError(string fileName, bool savexml)
		{
			return SaveXML(fileName, savexml);
		}


		/// <summary>
		/// Salva il file xml con SaveXML e logga gli eventuali errori.
		/// </summary>
		/// <param name="logWriter">scrittore di log</param>
		//---------------------------------------------------------------------
		internal virtual void SaveAndLogError(Logger logWriter)
		{
			SaveAndLogError(logWriter, string.Empty, false);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e logga gli eventuali errori.
		/// </summary>
		/// <param name="logWriter">scrittore di log</param>
		/// <param name="fileName">path del file da salvare</param>
		//---------------------------------------------------------------------
		internal  void SaveAndLogError(Logger logWriter, string fileName)
		{
			SaveAndLogError(logWriter, fileName, false);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e logga gli eventuali errori.
		/// </summary>
		/// <param name="logWriter">scrittore di log</param>
		/// <param name="savexml">specifica se permetto di salvare anche se la root non ha children</param>
		//---------------------------------------------------------------------
		internal  void SaveAndLogError(Logger logWriter, bool savexml)
		{
			SaveAndLogError(logWriter, String.Empty, savexml);
		}

		/// <summary>
		/// Salva il file xml con SaveXML e logga gli eventuali errori.
		/// </summary>
		/// <param name="logWriter">scrittore di log</param>
		/// <param name="fileName">path del file da salvare</param>
		/// /// <param name="savexml">specifica se permetto di salvare anche se la root non ha children</param>
		//---------------------------------------------------------------------
		internal  void SaveAndLogError(Logger logWriter, string fileName,  bool savexml)
		{
			string message = SaveXML(fileName, savexml);
			if (message != String.Empty && logWriter != null)
				logWriter.WriteLog(message, TypeOfMessage.warning); 
		}

		//---------------------------------------------------------------------
		private XmlNode GetObjectNode(string name, bool create)
		{
			if (currentDocument == null || rootNode == null)
				return null;

			XmlNode objectNode = rootNode.SelectSingleNode(name);
			if (objectNode == null && create)
			{
				objectNode = currentDocument.CreateElement(name);
				rootNode.AppendChild(objectNode);
				modified = true;
			}

			return objectNode;
		}

		//---------------------------------------------------------------------
		public bool WriteObjects(string name, object []objs, bool removeExisting)
		{
			XmlNode objectNode = GetObjectNode(name, true);
			if (objectNode == null)
				return false;

			if (removeExisting)
				objectNode.RemoveAll();

			foreach (object obj in objs)
				if (!WriteObject(objectNode, obj))
					return false;
			return true;

		}

		//---------------------------------------------------------------------
		public bool WriteObject(object obj, bool removeExisting)
		{
			return WriteObject(obj.GetType().Name, obj, removeExisting);
		}

		//---------------------------------------------------------------------
		public bool WriteObject(string name, object obj, bool removeExisting)
		{
			XmlNode objectNode = GetObjectNode(name, true);
			if (objectNode == null)
				return false;

			if (removeExisting)
				objectNode.RemoveAll();

			return WriteObject(objectNode, obj);
		}

		//---------------------------------------------------------------------
		public bool WriteObject(XmlNode parentNode, object obj)
		{
			if (currentDocument == null || parentNode == null)
				return false;

			XmlNode n = SerializerUtility.SerializeToXmlNode(obj);
			if (n == null)
				return false;
			
			modified = true;
			bool result = parentNode.AppendChild(currentDocument.ImportNode(n, true)) != null;
			modified = result;
			return result;

		}

		//---------------------------------------------------------------------
		public object[] ReadObjects(string name, Type systemType)
		{
			XmlNode objectNode = GetObjectNode(name, false);
			if (objectNode == null)
				return new object[0];
			
			object[] retVal = new object[objectNode.ChildNodes.Count];
			for (int i = 0; i < objectNode.ChildNodes.Count; i++)
			{
				if ((retVal[i] = SerializerUtility.DeserializeFromXmlNode(objectNode.ChildNodes[i], systemType)) == null)
					return new object[0];
			}

			return retVal;			
		}

		//---------------------------------------------------------------------
		public object[] ReadObjects(Type systemType)
		{
			return ReadObjects(systemType.Name, systemType);
		}

		//---------------------------------------------------------------------
		public object ReadSingleObject(Type systemType)
		{
			object[] ar = ReadObjects(systemType);
			if (ar.Length == 0)
				return null;

			return ar[0];
		}

		/// <summary>
		/// (all)Scrive un nodo tipo: (nodeName name=idd)
		/// </summary>
		/// <param name="nodeName">nome del tag</param>
		/// <param name="idd">valore dell'attributo name</param>
		/// <param name="overwrite">specifica se sovrascrivere un valore già settato in precedenza</param>
		//---------------------------------------------------------------------
		internal  void WriteNode(string nodeName, string idd, bool overwrite)
		{
			if (idd == null) return;
			XmlNode existingNode = rootNode.SelectSingleNode (nodeName);

			//se esiste diverso ed overwrite  = true gli setto il nuovo attributo
			if (existingNode != null && existingNode.Attributes[AllStrings.name].Value != idd && overwrite)
			{
				existingNode.Attributes[AllStrings.name].Value = idd;
				modified = true;
			}
			else if (existingNode == null)
			{
				resource = currentDocument.CreateElement(nodeName);
				if (idd != null)
					resource.SetAttribute(AllStrings.name, idd);
				
				rootNode.AppendChild(resource); 
				modified = true;
			}
			
		}

		//---------------------------------------------------------------------
		internal string ReadNode(string nodeName)
		{
			XmlNode node = GetObjectNode(nodeName, false);
			if (node == null)
				return "";
			return ((XmlElement)node).GetAttribute(AllStrings.name);
		}
	}

	/// <summary>
	/// To pass parameters to the Increment Event.
	/// </summary>
	//=========================================================================
	public class IncrementEventArgs : EventArgs 
	{
		private ProgressBar				progressBar;
		private string					message;
		private StatusBar				statusBar;
		private RichTextBox				txtOutput;
		private TypeOfMessage	type;
		
		/// <summary>ProgressBar che viene incrementata durante la creazione dei dizionari</summary>
		//--------------------------------------------------------------------------------
		internal	ProgressBar				ProgressBar	{ get {return progressBar;}	set {progressBar  = value;} }
		
		/// <summary>Messaggio</summary>
		//--------------------------------------------------------------------------------
		internal	string					Message		{ get {return message;}		set {message = value;} }
		
		/// <summary>StatusBar che visualizza il nome del progetto durante la creazione del dizionario</summary>
		//--------------------------------------------------------------------------------
		internal	StatusBar				StatusBar	{ get {return statusBar;}	set {statusBar  = value;} }
		
		/// <summary>RichTextBox nel quale viene scritto l'output durante la creazione del dizionario</summary>
		//--------------------------------------------------------------------------------
		internal	RichTextBox				TxtOutput	{ get {return txtOutput;}	set {txtOutput  = value;} }
		
		/// <summary>Tipo di messaggio (errore, warning, ...</summary>
		//--------------------------------------------------------------------------------
		internal	TypeOfMessage	Type		{ get {return type;}		set {type  = value;} }

		//---------------------------------------------------------------------
		internal IncrementEventArgs(ProgressBar pb, StatusBar sb, string message, TypeOfMessage type, RichTextBox tb)
		{
			ProgressBar	= pb; 
			StatusBar	= sb;
			Message		= message;
			TxtOutput	= tb;
			Type		= type;
		}
	}

	//========================================================================
	internal struct DictionaryReference
	{
		public string Name;
		public string Project;
			
		//---------------------------------------------------------------------
		public DictionaryReference(string name, string project)
		{
			this.Name	 = name;
			this.Project = project;
		}			
	}

	/// <summary>
	/// Contiene un array di document(see Document)
	/// </summary>
	//========================================================================
	internal struct ModifyList
	{
		public	NamedLocalizerDocument[]		Documents;

		//---------------------------------------------------------------------
		public int GetCount()
		{
			if (Documents == null) return 0;
			return Documents.Length;
		}

		//---------------------------------------------------------------------
		public NamedLocalizerDocument GetDocumentAt(int index)
		{
			return  (NamedLocalizerDocument)Documents.GetValue(index);
		}
			
		//---------------------------------------------------------------------
		public ModifyList(NamedLocalizerDocument[] doclist)
		{
			Documents = doclist;
		}

		//---------------------------------------------------------------------
		public void Add(ModifyList list)
		{
			ArrayList work = new ArrayList();
			work.AddRange(Documents);
			work.AddRange(list.Documents);
			Documents = (NamedLocalizerDocument[])work.ToArray(typeof(NamedLocalizerDocument));
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			Documents = new NamedLocalizerDocument[]{};
		}
	}
	
	/// <summary>
	/// Identifica xmldocument col path dal quale è stato caricato
	/// </summary>
	//========================================================================
	public class NamedLocalizerDocument
	{
		public string		Path;
		public LocalizerDocument	Doc;

		//---------------------------------------------------------------------
		public NamedLocalizerDocument (string path, LocalizerDocument doc)
		{
			Path = path;
			Doc  = doc;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			NamedLocalizerDocument target = obj as NamedLocalizerDocument;
			if (target == null)
				return false;

			return string.Compare(Path, target.Path, true) == 0;
		}


	}

	//=========================================================================
	public class FileNameChangedEventArgs : EventArgs
	{
		//--------------------------------------------------------------------------------
		public FileNameChangedEventArgs(string OldName, string NewName)
		{
			this.OldName = OldName;
			this.NewName = NewName;
		}
			
		public string OldName;
		public string NewName;
	}

}

