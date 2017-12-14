using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class DictionaryFile
	{
		public const string DictionaryFileName = "Dictionary.xml";
		public const string ResxDictionaryFileName = "ResxDictionary.xml";
		/// <summary>
		/// Tipologie di risorse che traducono elementi contenuti in una dll C++ o C# - i loro dizionari sono in una sottocartella dove si trova la dll
		/// </summary>
		public static readonly string[] DllResourceTypes = { "dialog", "menu", "stringtable", "source", "strings", "forms", "jsonforms"};
		/// <summary>
		/// Tipologie di risorse che traducono elementi NON contenuti in una dll C++ o C# (ossia i metadati)
		/// i loro dizionari sono nella dictionary
		/// </summary>
        // le DBInfo non vengono messe nei bin, nè lato server nè lato client perchè non vengono mai "consumate" dall'interfaccia
		public static readonly string[] MetadataResourceTypes = { "xml", "databasescript", "report", "web", "other" };

        /// <summary>
        /// Tipologie di risorse che traducono elementi web client Angular
        /// i loro dizionari sono nella dictionary del progetto angular in formato json
        /// </summary>
        public static readonly string[] NgResourceTypes = { "web" };

		private LocalizerDocument document = null;
		private string fileName;
		private enum State { START, ROOT, RESOURCE_FILE_BLOCK, RESOURCE_FILE, RESOURCE_BLOCK, RESOURCE } 
			
		//--------------------------------------------------------------------------------
		public string FileName { get { return fileName; } }
		
		//--------------------------------------------------------------------------------
		public DictionaryFile(string fileName)
		{
			this.fileName = fileName;
		}

		//--------------------------------------------------------------------------------
		public DictionaryFile()
			 : this(DictionaryFileName)
		{
		}

		//--------------------------------------------------------------------------------
		public static ResourceGroup[] ExtractResourceGroups(XmlDocument xmlDocument)
		{
			ArrayList list = new ArrayList();

			if (xmlDocument != null && xmlDocument.DocumentElement != null)
			{
				foreach (XmlElement resourceGroupNode in xmlDocument.DocumentElement.ChildNodes)
				{
					string resourceGroupType = resourceGroupNode.Name;
					foreach (XmlElement resourceFileNode in resourceGroupNode.ChildNodes)
					{
						string id = resourceFileNode.GetAttribute(AllStrings.id);
						foreach (XmlElement resourceBlockNode in resourceFileNode.ChildNodes)
						{
							string name = resourceBlockNode.GetAttribute(AllStrings.name);
							list.Add(new ResourceGroup(resourceGroupType, id, name, resourceBlockNode));
						}
					}
				}
			}
			return list.ToArray(typeof(ResourceGroup)) as ResourceGroup[];
		}

		//--------------------------------------------------------------------------------
		public virtual void Save(string root)
		{
			if (document == null)
				return;

			string path = Path.Combine(root, FileName);
			document.Save(path);
		}

		//--------------------------------------------------------------------------------
		public DictionaryDocument GetResource(string dictionaryFileId, string sourceType)
		{
			return GetResource(dictionaryFileId, sourceType, null);
		}

		//--------------------------------------------------------------------------------
		public void InitDocument(string root)
		{
			if (document == null)
			{
				document = new LocalizerDocument();
				if (root != null)
				{
					string path = Path.Combine(root, FileName);
			
					if (File.Exists(path))
						document.Load(path);
				}
			}

			if (document.DocumentElement == null)
			{
				XmlDeclaration declaration = document.CreateXmlDeclaration
					(AllStrings.version, AllStrings.encoding, null);
				document.AppendChild(declaration);

				document.AppendChild(document.CreateElement(AllStrings.resources));	
			}
		}

		//--------------------------------------------------------------------------------
		public DictionaryDocument GetResource(string dictionaryFileId, string sourceType, string root)
		{
			InitDocument(root);

			string dictionarySourceType = ToDictionaryClassType(sourceType);
			XmlElement typeNode = document.DocumentElement.SelectSingleNode(dictionarySourceType) as XmlElement;
			if (typeNode == null)
			{
				typeNode = document.CreateElement(dictionarySourceType);
				document.DocumentElement.AppendChild(typeNode);
			}

			foreach (XmlElement resource in typeNode.ChildNodes)
			{
				if (resource.GetAttribute(AllStrings.id) == dictionaryFileId)
					return new DictionaryDocument(document, resource);
			}

			XmlElement el = document.CreateElement(AllStrings.resources);
			el.SetAttribute(AllStrings.id, dictionaryFileId);
			
			typeNode.AppendChild(el);
			return new DictionaryDocument(document, el);
		}

		//--------------------------------------------------------------------------------
		public string ToDictionaryClassType(string sourceType)
		{
			if (sourceType == AllStrings.font || 
				sourceType == AllStrings.format ||
				sourceType == AllStrings.enumTag)
				return AllStrings.other;
			
			if (sourceType == AllStrings.sqlScript)
				return AllStrings.database;

			return sourceType;
		}

		//--------------------------------------------------------------------------------
		public bool HasUniqueFile(string sourceType, string fileID)
		{
			if (sourceType == AllStrings.other)
				return fileID == AllStrings.formats || fileID == AllStrings.fonts;
			
			if (sourceType == AllStrings.xml ||
				sourceType == AllStrings.source ||
				sourceType == AllStrings.stringtable ||
				sourceType == AllStrings.database ||
				sourceType == AllStrings.oldStrings || 
                sourceType == AllStrings.web
                )
                return true;

			return false;
		}

		//--------------------------------------------------------------------------------
		public void Parse(DictionaryTreeNode cultureNode, string filePath)
		{
			XmlTextReader reader = null;
			State state = State.START;
			string currentType = null, currentId = null;
			try
			{
				reader = new XmlTextReader(filePath);

				DictionaryTreeNode fileBlockNode = null, fileNode = null, resourceNode = null; 
				while (reader.Read())	// resource
				{
					switch(reader.NodeType)
					{
						case XmlNodeType.Element:
						{
							switch (state)
							{
								case State.START:
									state = State.ROOT;
									break;
								case State.ROOT:
									currentType = reader.LocalName;
									fileBlockNode = cultureNode.AddUnique(null, currentType, NodeType.RESOURCE);
									fileBlockNode.ImageIndex = fileBlockNode.SelectedImageIndex = (int) Images.FOLDERCLOSED;
									state = State.RESOURCE_FILE_BLOCK;
									break;
								case State.RESOURCE_FILE_BLOCK:
									if (!reader.IsEmptyElement)
									{
										reader.MoveToAttribute(AllStrings.id);
										currentId = reader.Value;
										if (HasUniqueFile(currentType, currentId))
										{
											fileNode = fileBlockNode;
										}
										else
										{
											fileNode = fileBlockNode.AddUnique(null, currentId, NodeType.RESOURCE);
											fileNode.ImageIndex = fileNode.SelectedImageIndex = (int) Images.FILE;
										}
										state = State.RESOURCE_FILE;
									}
									break;
								case State.RESOURCE_FILE:
									if (!reader.IsEmptyElement)
									{
										reader.MoveToAttribute(AllStrings.name);
										string modifiedPath = CommonFunctions.GetCorrespondingLanguagePath(filePath, cultureNode.Name);
										resourceNode = fileNode.AddUnique (modifiedPath, reader.Value, NodeType.LASTCHILD);
									
										resourceNode.GroupIdentifier = currentId;
										resourceNode.ResourceType = currentType;
										resourceNode.ImageIndex = resourceNode.SelectedImageIndex = 
											CommonFunctions.GetImageIndexFromName(currentType, reader.Value);
									
										state = State.RESOURCE_BLOCK;
									}
									break;
								case State.RESOURCE_BLOCK:
									if (!reader.IsEmptyElement)
										state = State.RESOURCE; //otherwise EndElement is not found, and state cannot be restored
									break;
	
							}
							break;
						}
						case XmlNodeType.EndElement:
						{
							switch (state)
							{
								case State.ROOT:
									state = State.START;
									break;
								case State.RESOURCE_FILE_BLOCK:
									state = State.ROOT;
									break;
								case State.RESOURCE_FILE:
									state = State.RESOURCE_FILE_BLOCK;
									break;
								case State.RESOURCE_BLOCK:
									state = State.RESOURCE_FILE;
									break;
								case State.RESOURCE:
									state = State.RESOURCE_BLOCK;
									break;
							}
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format(Strings.ErrorParsingDictionaryFile, filePath), ex);
			}
			finally
			{
				reader.Close();
			}
		}
	
		//--------------------------------------------------------------------------------
		public bool CompileTo (ResourceIndexDocument resourceIndex, DictionaryBinaryFile dictionary, IList<string> types, bool lowerCaseTags = true)
		{
			if (document == null)
				document = LocalizerDocument.GetStandardXmlDocument(fileName, false);
			
			if (document == null)
				return false;
			
			foreach (XmlElement resourceType in document.DocumentElement.ChildNodes)
			{
				string type = resourceType.LocalName;
				if (!types.Contains(type))
					continue;

				foreach (XmlElement resourceId in resourceType.ChildNodes)
				{
					string id = resourceId.GetAttribute(AllStrings.id);

					foreach (XmlElement resourceName in resourceId.ChildNodes)
					{
						string name = resourceName.GetAttribute(AllStrings.name);
							
						DictionaryStringBlock sb = new DictionaryStringBlock();
						foreach (XmlElement el in resourceName.SelectNodes(AllStrings.stringTag))
						{
							string baseString = string.Empty;
							DictionaryStringItem item = new DictionaryStringItem();

                            // skip stringhe non valide
                            if (
                                    el.Attributes[AllStrings.valid] != null &&
                                    el.Attributes[AllStrings.valid].Value == AllStrings.falseTag
                                )
                                continue;

							foreach (XmlAttribute attr in el.Attributes)
							{
								switch (attr.LocalName)
								{
									case AllStrings.valid: 
										if (attr.Value == AllStrings.falseTag)
											continue;
										break;
									case AllStrings.baseTag:
										baseString = attr.Value;
										break;
									case AllStrings.target:
										item.Target = attr.Value;
										break;
									case AllStrings.name:
									case AllStrings.support:
									case AllStrings.file:
									case AllStrings.temporary:
									case AllStrings.matchType:
									case AllStrings.id:
										break;
									default:
										item[attr.LocalName] = attr.Value;
										break;
								}
								
							}

							if (item.Target.Length > 0 && baseString.Length > 0)
								sb.AddStringItem (baseString, item);
						}

						if (sb.Count == 0)
							continue;

						if (type == AllStrings.stringtable)
						{
							name = "";
						}
						else if (resourceIndex == null ? false : resourceIndex.IsIndexedType(type))
						{
							id = ""; //file name is irrilevant for indexed types
							name = resourceIndex.GetResourceIdByName(type, name).ToString();
						}
						else if (type == AllStrings.report)
						{
							name = AllStrings.report; //all report elements for a single report file are gathered togheter
						}
						dictionary.AddDictionaryStringBlock(type, id, name, sb, lowerCaseTags);
					}		
				}
			}
			return true;
		}
	}

	
	//================================================================================
	public class ResxDictionaryFile : DictionaryFile
	{
		private Logger logWriter;
		private ProjectDocument tblPrjWriter;	
		private string sourceRoot;
		private string resxDictionaryFile;

		public string ResxFile { get { return resxDictionaryFile; }	}
		

		//--------------------------------------------------------------------------------
		public ResxDictionaryFile
			(
			string fileName,
			string sourceRoot,
			ProjectDocument tblPrjWriter,
			Logger logWriter
			)
			: base (fileName)
		{
			this.logWriter = logWriter;
			this.tblPrjWriter = tblPrjWriter;
			this.sourceRoot = sourceRoot;
		}

		//--------------------------------------------------------------------------------
		public override void Save(string root)
		{
			string defaultNamespace = tblPrjWriter.ReadNode(AllStrings.namespaceTag);
			ProcessCSSourceFile(FileName, root, defaultNamespace);
		}
		//--------------------------------------------------------------------------------
		private bool ProcessCSSourceFile(
			string sourceResxFile,
			string dictionaryFolder,
			string defaultNamespace
			)
		{
			if (!CommonFunctions.IsResx(sourceResxFile))
				return false;

			string relPath = Functions.CalculateRelativePath(Path.GetDirectoryName(sourceResxFile), sourceRoot, true);
			string additionalToken = "";
			if (relPath != null && relPath.Length > 0)
				additionalToken = string.Concat(".", relPath.Replace(Path.DirectorySeparatorChar, '.'));

			resxDictionaryFile = String.Concat
				(
				defaultNamespace,
				additionalToken,
				".",
				Path.GetFileName(sourceResxFile)
				);
			//nome del file: namespace.(ulteriore annidamento namespace.)nomefile.lingua.resx
			try
			{
				string folder;
				bool isForm, isEmpty;
				DataDocumentFunctions.CheckContent (sourceResxFile, out isForm, out isEmpty);
				if (isEmpty)
					return false;
				if (isForm)
				{
					folder			= Path.Combine(dictionaryFolder, AllStrings.forms);
					resxDictionaryFile = Path.Combine(folder, resxDictionaryFile);
				}
				else 
				{
					folder			= Path.Combine(dictionaryFolder, AllStrings.strings);
					resxDictionaryFile = Path.Combine(folder, resxDictionaryFile);
				}

				return CSUpdatingProcedure(sourceResxFile);
			}
			catch (Exception exc)
			{
				logWriter.WriteLog(String.Format(Strings.DictionaryFileError, resxDictionaryFile), TypeOfMessage.error);
				logWriter.WriteLog(exc.Message, TypeOfMessage.error);
				return false;
			}
			
		}
		//---------------------------------------------------------------------
		private bool CSUpdatingProcedure(string sourceFile)
		{
			bool updateNeeded = true;
			try
			{
				if (!File.Exists(resxDictionaryFile))
				{
					updateNeeded = false;
					Functions.SafeCopyFile(sourceFile, resxDictionaryFile, true);
				}

				string resourceName = Path.GetFileNameWithoutExtension(resxDictionaryFile);
				DictionaryDocument oldDictionaryDocument = LocalizerDocument.MergeFilesResx(resxDictionaryFile, true, resourceName);
				if (oldDictionaryDocument == null) 
					return false; 
				LocalizerDocument oldDoc = oldDictionaryDocument.Document;
				if (oldDoc == null) 
					return false; 
				
				if (updateNeeded)
				{
					DictionaryDocument newDictionaryDocument = LocalizerDocument.MergeFilesResx(sourceFile, true, resourceName);
					if (newDictionaryDocument == null) 
						return false; 
					LocalizerDocument newDoc = newDictionaryDocument.Document;
					if (newDoc == null) 
						return false; 
			
					UpdateXml(newDoc, oldDictionaryDocument, logWriter);

					CommonFunctions.TryToCheckOut(resxDictionaryFile);
					File.Copy(sourceFile, resxDictionaryFile, true);
					File.SetAttributes(resxDictionaryFile, FileAttributes.Normal);
				}
				else
				{
					long targetID = 0;
					foreach (XmlElement stringNode in oldDoc.SelectNodes("//string"))
						stringNode.SetAttribute(AllStrings.id, (++targetID).ToString());
			
				}

				//even if no update is needed, this operation adds id attributes, so is useful
				LocalizerDocument.SaveResxFromXml(oldDoc, resxDictionaryFile, false, logWriter);

				return true;
			}
			catch (Exception exc)
			{
				logWriter.WriteLog(String.Format(Strings.DictionaryFileError, resxDictionaryFile), TypeOfMessage.error);
				logWriter.WriteLog(exc.Message, TypeOfMessage.error);
				return false;
			}

		}

		//---------------------------------------------------------------------
		private void UpdateXml(LocalizerDocument newDocument, DictionaryDocument oldDictionaryDocument, Logger logWriter)
		{	
			try
			{
				string xPath = "//node()[@name and string]";
				foreach (XmlElement stringNode in oldDictionaryDocument.Document.SelectNodes("//string"))
					stringNode.SetAttribute(AllStrings.valid, AllStrings.falseTag);

				foreach (XmlElement resourceNode in newDocument.SelectNodes(xPath))
				{
						
					oldDictionaryDocument.WriteResource(resourceNode.LocalName, resourceNode.GetAttribute(AllStrings.name));
					foreach (XmlElement stringNode in resourceNode.ChildNodes)
					{
						string nameValue	= stringNode.GetAttribute(AllStrings.name);
						string baseValue	= stringNode.GetAttribute(AllStrings.baseTag);
						string targetValue	= stringNode.GetAttribute(AllStrings.target);
						string validValue	= stringNode.GetAttribute(AllStrings.valid);
						if (string.Compare(validValue, AllStrings.falseTag,	true) != 0)
							oldDictionaryDocument.WriteString(baseValue, nameValue);
					}
				
					oldDictionaryDocument.CloseResource();
				}
			}
			finally
			{
			}
		}
	}
	
	//================================================================================
	public class DictionaryFileCollection : ArrayList
	{
		Logger logger;

		//--------------------------------------------------------------------------------
		public DictionaryFileCollection(Logger logger)
		{
			this.logger = logger;
		}

		//--------------------------------------------------------------------------------
		public DictionaryFile this[string file]
		{
			get
			{
				foreach (DictionaryFile f in this)
				{
					if (string.Compare(f.FileName, file, true) == 0)
						return f;
				}
				DictionaryFile dFile = new DictionaryFile(file);
				Add(dFile);
				return dFile;
			}
			set
			{
				DictionaryFile found = null;
				foreach (DictionaryFile f in this)
				{
					if (string.Compare(f.FileName, file, true) == 0)
					{
						found = f;
						break;
					}
				}

				if (found != null)
					Remove(found);

				Add(value);
			}
		}

		//--------------------------------------------------------------------------------
		public void Save(string root)
		{
			logger.SetRange(this.Count);
			logger.WriteLog(Strings.SavingStrings);
			foreach (DictionaryFile f in this)
			{
				try
				{
					logger.PerformStep();
					f.Save(root);
				}
				catch (Exception ex)
				{
					string message = string.Format(Strings.ErrorProcessingFile, f.FileName, ex.Message);
					logger.WriteLog(message, TypeOfMessage.error);
				}
			}
		}
	}

	//================================================================================
	public class ResourceGroup
	{
		public ResourceGroup
			(
			string groupType,
			string groupIdentifier,
			string resourceType,
			XmlElement resourceNode
			)
		{
			this.GroupType = groupType;
			this.GroupIdentifier = groupIdentifier;
			this.ResourceType = resourceType;
			this.ResourceNode = resourceNode;
		}

		public string GroupType;
		public string GroupIdentifier;
		public string ResourceType;
		public XmlElement ResourceNode;

	}
}
