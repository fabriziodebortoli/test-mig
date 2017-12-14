using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;


namespace Microarea.Tools.TBLocalizer
{
	
	/// <summary>
	/// Contiene le informazioni relative agli errori rilevati durante 
	/// il controllo dei conflitti di '&' nelle varie risorse.
	/// </summary>
	//=========================================================================
	internal class AmpersandInfo 
	{
		public	string		NodePath;
		public	string		ResourceName;
		public	string		PostLetter;
		private ArrayList	repetitionList = new ArrayList();
		//--------------------------------------------------------------------------------
		public	ArrayList	RepetitionList  { get {return repetitionList;}}
		private int			counter = 2;
		//--------------------------------------------------------------------------------
		public	int			Counter			{ get {return counter;} set {counter = value;}}

		//-----------------------------------------------------------------------------
		public AmpersandInfo(string nodePath, string resourceName, string postLetter)
		{	
			if (nodePath	 == null)	nodePath	 = String.Empty;
			if (resourceName == null)	resourceName = String.Empty;
			if (postLetter	 == null)	postLetter	 = String.Empty;
			NodePath		= nodePath;
			ResourceName	= resourceName;
			PostLetter		= postLetter;
		}

		//-----------------------------------------------------------------------------
		public void AddRepetition(string s)
		{
			if (s != null && s!= String.Empty)
				repetitionList.Add(s);
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return 
				(
				NodePath.GetHashCode() + 
				ResourceName.GetHashCode() +
				PostLetter.GetHashCode() +
				repetitionList.GetHashCode()+ 
				counter.GetHashCode()
				);
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (!(obj is AmpersandInfo)) return false;
			AmpersandInfo objToCompare = (AmpersandInfo)obj;

			return	
				String.Compare(NodePath,	 objToCompare.NodePath,		true) == 0 &&
				String.Compare(ResourceName, objToCompare.ResourceName, true) == 0 &&
				String.Compare(PostLetter,	 objToCompare.PostLetter,	true) == 0 	;
		}
		
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			if (RepetitionList != null && RepetitionList.Count > 0)
			{
				for (int i = 0; i < RepetitionList.Count; i++)
				{
					sb.Append("\t");
					sb.Append(RepetitionList[i] as string);
					if (i < (RepetitionList.Count - 1))
						sb.Append(Environment.NewLine);
				}
			}
			sb.Append(Environment.NewLine);
			return String.Format
				(
				Strings.AmpersandRepetition, 
				Counter.ToString(), 
				PostLetter, 
				NodePath, 
				sb.ToString()
				);
		}
	}

	/// <summary>
	/// Contiene le informazioni relative agli errori rilevati 
	/// durante il controllo dei placeHolder nelle varie risorse
	/// </summary>
	//=========================================================================
	internal class PlaceHolderInfo 
	{
		public	string		NodePath;
		public	string		ResourceName;
		private ArrayList	errorList		= new ArrayList();
		//--------------------------------------------------------------------------------
		public	ArrayList	ErrorList		{ get {return errorList;}}

		//-----------------------------------------------------------------------------
		public PlaceHolderInfo(string nodePath, string resourceName)
		{
			if (nodePath == null)		nodePath	 = String.Empty;
			if (resourceName == null)	resourceName = String.Empty;
			NodePath		= nodePath;
			ResourceName	= resourceName;
		}

		//-----------------------------------------------------------------------------
		public void AddError(PlaceHolderValidity phv)
		{
			//Costruzione della stringa del messaggio di errore
			StringBuilder errorMark	= new StringBuilder();
			errorMark.Append(phv.TranslationValid ? String.Empty : Strings.Coherence);
			if (!phv.SequenceValid)
			{	
				if (!phv.TranslationValid)
					errorMark.Append("+");
				errorMark.Append(Strings.Sequence);
			}
			errorList.Add(String.Format(Strings.PlaceHolderErrorSpec, errorMark, phv.BaseString));
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return 
				(
				NodePath.GetHashCode() + 
				ResourceName.GetHashCode() +
				errorList.GetHashCode()
				);
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (!(obj is PlaceHolderInfo)) return false;
			PlaceHolderInfo objToCompare = (PlaceHolderInfo)obj;

			return	
				String.Compare(NodePath, objToCompare.NodePath, true) == 0 &&
				String.Compare(ResourceName, objToCompare.ResourceName, true) == 0;
		}
		
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			if (errorList != null && errorList.Count > 0)
			{
				for (int i = 0; i < errorList.Count; i++)
				{
					sb.Append("\t");
					sb.Append(errorList[i] as string);
					if (i < (errorList.Count - 1))
						sb.Append(Environment.NewLine);
				}
			}
			sb.Append(Environment.NewLine);
			sb.Append(Environment.NewLine);
			return String.Format
				(
				Strings.PlaceHoldersError, 
				errorList.Count.ToString(), 
				NodePath, 
				sb.ToString()
				);

		}
	}

	//=========================================================================
	public class AsyncZipDictionaryManager : ZipDictionaryManager
	{
		private bool worked = false;
		private string baseDir;
		private bool result;
        private string[] baseDirs;
        private bool recursive;
        private string startFolder;

		public AsyncZipDictionaryManager(string zipFileFullName, Logger logWriter, bool writeMode)
            : base(zipFileFullName, logWriter, writeMode)
		{
		}

        //---------------------------------------------------------------------
        public new void ZipDictionary(string[] baseDirs, bool recursive, string startFolder)
        {
            this.baseDirs = baseDirs;
            this.recursive = recursive;
            this.startFolder = startFolder;
            Thread t = new Thread(new ThreadStart(ZipDictionary));
            worked = false;
            t.Start();
        }
        //---------------------------------------------------------------------
        private void ZipDictionary()
        {
            result = base.ZipDictionary(baseDirs, recursive, startFolder);
            worked = true;
        }

        //---------------------------------------------------------------------
        private void UnzipFile()
        {
            result = file.ExtractAll(baseDir);
            worked = true;
        }

        //---------------------------------------------------------------------
        public void UnzipFile(string baseDir)
        {
           this.baseDir = baseDir;

            Thread t = new Thread(new ThreadStart(UnzipFile));
            worked = false;
            t.Start();
        }
	               
        //---------------------------------------------------------------------
		public bool WaitResult()
		{
			while (!worked)
			{
				System.Windows.Forms.Application.DoEvents();
				Thread.Sleep(5);
			}
			return result;
		}
	}
    
	//=========================================================================
	public class ZipDictionaryManager : IDisposable
	{
		public const string rootInfoFileName = "DictionaryRoot.tblidx";

        protected CompressedFile file = null;
	    string zipFileFullName = null;
		Logger logWriter;

		//---------------------------------------------------------------------
		public ZipDictionaryManager(string zipFileFullName, Logger logWriter, bool writeMode)
		{
			if (zipFileFullName != null)
			{
                file = new CompressedFile(zipFileFullName, writeMode ? CompressedFile.OpenMode.CreateAlways : CompressedFile.OpenMode.Read);
                this.zipFileFullName = zipFileFullName;
            }
			
			this.logWriter = logWriter;
			file.EndCompressFile += new CompressedFile.CompressEventHandler(ZipDictionaryManager_EndCompressOperation);
			file.EndCompressFolder += new CompressedFile.CompressEventHandler(ZipDictionaryManager_EndCompressOperation);
			file.EndUncompressFile += new CompressedFile.CompressEventHandler(ZipDictionaryManager_EndCompressOperation);
			file.EndUncompressFolder += new CompressedFile.CompressEventHandler(ZipDictionaryManager_EndCompressOperation);
		}

        //---------------------------------------------------------------------
		public void AddRootInfoToZip(string root)
		{
			try
			{
                using (StreamWriter sw = new StreamWriter(rootInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(root);
                    sw.Flush();
                    sw.Close();
                }

                if (file.AddFile(rootInfoFileName))
                {
				    if (File.Exists(rootInfoFileName))
					    File.Delete(rootInfoFileName);
                }
            }
            catch (CompressionException)
            {
            }
		}

        //---------------------------------------------------------------------
        public bool ZipDictionary(string[] baseDirs, bool recursive, string startFolder)
        {
            if (file == null || file.IsAborted) 
                return false;

            bool result = true;
            try
            {
                foreach (string baseDir in baseDirs)
                {
                    DirectoryInfo di = new DirectoryInfo(baseDir);
                    result = result && file.AddFolder(di.FullName, recursive, startFolder);
                }
            }
            catch (CompressionException)
            {
                return false;
            }

            return result;
        }

		//---------------------------------------------------------------------
		public string GetRootInfoFromZip()
		{
			string outDir = Path.GetTempPath();
            this.file.ExtractFile(rootInfoFileName, outDir);

			string file = Path.Combine(outDir, rootInfoFileName);
			if (!File.Exists(file))
				return null;
			
			try
			{
				using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
				{
					return sr.ReadLine();
				}
			}
			finally
			{
				File.Delete(file);
			}
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
            file.Close();
		}

		#endregion

		//---------------------------------------------------------------------
		private void ZipDictionaryManager_EndCompressOperation(object sender, CompressEventArgs e)
		{
			if (logWriter == null)
				return;

            string logMessage = string.Format
                (
                    "{0} operation of package {1} ({2}) ended with {3} result",
                    e.Action.ToString(),
                    e.PackageFileName,
                    e.CurrentProcessingFileName,
                    e.Result.ToString()
                );
           
            DictionaryCreator.MainContext.BeginInvoke(new WriteLogFunction(WriteLog), new object[] {logMessage });
		}

        //---------------------------------------------------------------------
        private delegate void WriteLogFunction(string logMessage);
        private void WriteLog(string logMessage)
        { 
            logWriter.WriteLog(logMessage);
        }
	}
	/// <summary>
	/// Specifica il tipo di errore rilevato per la stringa durante il controllo dei placeholders
	/// </summary>
	//=========================================================================
	public struct PlaceHolderValidity
	{
		public bool		TranslationValid;
		public bool		SequenceValid;
		public string	BaseString;

		//---------------------------------------------------------------------
		public PlaceHolderValidity(bool translatioValid, bool sequenceValid, string baseString)
		{
			TranslationValid = translatioValid;
			SequenceValid	 = sequenceValid;
			BaseString		 = baseString;
		}

	}

	//=========================================================================
	public class DictionaryTreeNode : LocalizerTreeNode
	{		
		private static LocalizerXmlNodeList emptyNodeList = new LocalizerXmlNodeList();

		string groupIdentifier	= string.Empty;
		string resourceType		= string.Empty;

		public string GroupIdentifier	{ get { return groupIdentifier; } set { groupIdentifier = value; } }
		public string ResourceType		{ get { return resourceType; } set { resourceType = value; } }
		public string Culture			{ get { return GetTypedParentNode(NodeType.LANGUAGE).Name; }  }
		
		//---------------------------------------------------------------------
		public LocalizerDocument Document 
		{
			get 
			{
				XmlElement el = GetResourceNode(); 
				if (el == null)
					return null;
				return el.OwnerDocument as LocalizerDocument;
			}
		}
	
		//---------------------------------------------------------------------
		public override object Clone()
		{
			DictionaryTreeNode clone = new DictionaryTreeNode(FileSystemPath, Name, Type);
			clone.groupIdentifier = groupIdentifier;
			clone.resourceType = resourceType;
			clone.ImageIndex = ImageIndex;
			clone.SelectedImageIndex = SelectedImageIndex;
			return clone;
		}

		//---------------------------------------------------------------------
		public DictionaryTreeNode(string nodePath, string nodeName, NodeType nodeType)
			: base(nodePath, nodeName, nodeType)
		{
		}

		//---------------------------------------------------------------------
		public DictionaryTreeNode(NodeTag tag)
			: base(tag)
		{
		}

		//---------------------------------------------------------------------
		public override SourceControlStatus SourceControlStatus
		{ 
			get
			{
				return DictionaryCreator.MainContext.SourceControlManager.GetSourceControlStatus(FileSystemPath, Type);
			}
		}

		//---------------------------------------------------------------------
		public XmlNodeList GetStringNodes()
		{
			XmlElement resource = GetResourceNode();
			return GetStringNodes(resource);
		}
		//---------------------------------------------------------------------
		public XmlNodeList GetStringNodes(string dictionaryFile)
		{
			XmlElement resource = GetResourceNode(dictionaryFile);
			return GetStringNodes(resource);
		
		}
		//---------------------------------------------------------------------
		public XmlNodeList GetStringNodes(LocalizerDocument dictionaryDocument)
		{
			XmlElement resource = dictionaryDocument.GetResourceNode(this);
			return GetStringNodes(resource);
		}

		//---------------------------------------------------------------------
		public XmlNodeList GetStringNodes(XmlElement resource)
		{
			if (resource == null)
				return emptyNodeList;

			return resource.GetElementsByTagName(AllStrings.stringTag);
		}

		//---------------------------------------------------------------------
		public XmlElement GetResourceNode()
		{
			return GetResourceNode(FileSystemPath);
		}

		//---------------------------------------------------------------------
		public XmlElement GetResourceNode(string dictionaryFile)
		{
			try
			{
				LocalizerDocument doc = LocalizerDocument.GetStandardXmlDocument(dictionaryFile, true);
				if (doc == null)
					return null;
				return doc.GetResourceNode(this);
			}
			catch (Exception ex)
			{
				DictionaryCreator.MainContext.GlobalLogger.WriteLog(string.Format("Errore reading dictionary file {0}:\r\n'{1}'", dictionaryFile, ex.Message), TypeOfMessage.error);
				return null;
			}
		}

		//---------------------------------------------------------------------
		public string GetCorrespondingNodePath(string targetCulture)
		{
			string token = string.Format("\\{0}\\", Culture);
			string fullPath = FullPath;
			if (fullPath.IndexOf(token) == -1)
				return null;

			return fullPath.Replace(token, string.Format("\\{0}\\", targetCulture));
			
		}

		//---------------------------------------------------------------------
		public DictionaryTreeNode GetCorrespondingNode(string targetCulture)
		{
			string path = GetCorrespondingNodePath(targetCulture);
			if (path == null)
				return null;
			LocalizerTreeNode targetNode = null;
			if (DictionaryCreator.MainContext.GetNodeFromPath(path, out targetNode))
				return targetNode as DictionaryTreeNode;
			return null;
		}

		/// <summary>
		/// Carica i dati relativi alla lingua di supporto, merg-ando i due dizionari ed aggiungendo un attributo temporaneo.
		/// </summary>
		/// <param name="fileSupport">file omologo del dizionario di supporto</param>
		/// <param name="supportLanguage">codice della lingua di supporto</param>
		//---------------------------------------------------------------------
		internal bool MergeWithSupport(string supportLanguage)
		{
			XmlNodeList listOfString = GetStringNodes();
			if (listOfString == null || listOfString.Count == 0)
				return true;
			
			DictionaryTreeNode supportDictNode = GetCorrespondingNode(supportLanguage);
			if (supportDictNode == null)
				return false;

			XmlElement supportNode = supportDictNode.GetResourceNode();
			
			foreach (XmlElement node in listOfString)
			{
				XmlAttribute att = node.Attributes[AllStrings.baseTag];
				if (att == null) continue;
				string nodeValue = att.Value;
				string toSearchComplete = String.Concat
					(
					AllStrings.stringTag,
					CommonFunctions.XPathWhereClause(AllStrings.baseTag, nodeValue),
					"/@",
					AllStrings.target
					);

				XmlNode xmlNodeSupport = supportNode.SelectSingleNode(toSearchComplete);

				string supportValue = null;
				if (xmlNodeSupport != null)
					supportValue = xmlNodeSupport.Value;
				else
					supportValue = node.GetAttribute(AllStrings.baseTag);
				
				node.SetAttribute(AllStrings.support, supportValue);

				bool supporttemporaryvalue = false;
				string query = String.Concat
					(
					AllStrings.stringTag,
					CommonFunctions.XPathWhereClause(AllStrings.baseTag, nodeValue),
					"/@",
					AllStrings.temporary
					);
				XmlNode xmlNodeSupportTemporary = supportNode.SelectSingleNode(query);
				if (xmlNodeSupportTemporary != null)
					supporttemporaryvalue = String.Compare(xmlNodeSupportTemporary.Value, bool.TrueString, true) == 0;

				node.SetAttribute(AllStrings.supportTemporary, supporttemporaryvalue ? AllStrings.trueTag : AllStrings.falseTag);

			}
			return true;
		}
		//---------------------------------------------------------------------
		public bool References(DictionaryTreeNode node)
		{
			XmlElement resource = GetResourceNode();
			if (resource == null) return false;

			string nodePath = node.FullPath;
			foreach (XmlElement stringNode in resource.ChildNodes)
			{
				string nodePathTarget, id;
				if (LocalizerDocument.GetReferencedNodeElements(stringNode, out nodePathTarget, out id) &&
					string.Compare(nodePathTarget, nodePath, true) == 0)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		public DictionaryTreeNode AddUnique(string nodePath, string nodeName, NodeType nodeType)
		{
			NodeTag tag = new NodeTag(nodePath, nodeName, nodeType);
			foreach (DictionaryTreeNode n in Nodes)
				if (n.Tag.IsEqual(tag))
					return n;
			DictionaryTreeNode newNode = new DictionaryTreeNode(tag);
			TreeNodeComparer cmp = new TreeNodeComparer();
			int i = 0;
			for (i = 0; i < Nodes.Count; i++)
			{
				if (cmp.Compare(Nodes[i], newNode) > 0)
					break;
			}
			Nodes.Insert(i, newNode);
			
			return newNode;
		}

		//---------------------------------------------------------------------
		protected override void GetTypedChildNodes(NodeType aType, bool deep, string nameFilter, bool ignoreCase, string cultureFilter, ArrayList list)
		{
			if (cultureFilter != null && string.Compare(cultureFilter, Culture, true) != 0)
				return;

			base.GetTypedChildNodes (aType, deep, nameFilter, ignoreCase, cultureFilter, list);
		}

		//---------------------------------------------------------------------
		public DictionaryTreeNode GetAdjacentLeafNode(Direction direction)
		{
			if (Type != NodeType.LASTCHILD) return null;
			
			DictionaryTreeNode node =  (direction == Direction.NEXT)
				? (DictionaryTreeNode)NextNode 
				: (DictionaryTreeNode)PrevNode;
			
			if (node != null && node.Type == NodeType.LASTCHILD)
				return node;

			string culture = this.Culture;

			LocalizerTreeNode currNode = Parent as LocalizerTreeNode;
			while (currNode != null)
			{
				try
				{
					LocalizerTreeNode parentNode = (direction == Direction.NEXT)  
						? (LocalizerTreeNode)currNode.NextNode 
						: (LocalizerTreeNode)currNode.PrevNode;
				
					if (parentNode == null)
						continue;

					if (parentNode.Type == NodeType.LANGUAGE && 
						string.Compare(((DictionaryTreeNode)parentNode).Culture, culture, true) != 0)
						continue;

					ArrayList nodes = parentNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, culture);
					if (nodes == null || nodes.Count == 0)
						continue;

					return  (direction == Direction.NEXT) 
						? (DictionaryTreeNode)nodes[0]  
						: (DictionaryTreeNode)nodes[nodes.Count - 1];
					
				}
				finally
				{
					currNode = currNode.Parent as LocalizerTreeNode;
				}
			}

			return null;
		}

		//---------------------------------------------------------------------
		internal bool SaveToFileSystem()
		{
			return LocalizerDocument.SaveStandardXmlDocument(FileSystemPath, Document);
		}
	}
}
