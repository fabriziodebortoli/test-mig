using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Gestisce il salvataggio sicuro con integrazione con Source safe
	/// </summary>
	//================================================================================
	public class LocalizerDocument : XmlDocument
	{
		//================================================================================
		private struct BaseLanguageItem
		{
			public string Id;
			public string Base;

			public BaseLanguageItem(string id, string aBase)
			{
				this.Id = id;
				this.Base = aBase;
			}
		}

		//================================================================================
		private class SavingContext
		{
			Thread savingThread = null;
			ArrayList documents = new ArrayList();
			ArrayList paths = new ArrayList();
			ManualResetEvent saveNeeded = new ManualResetEvent(false);
			bool stopped = false;

			//---------------------------------------------------------------------
			public SavingContext()
			{
				savingThread = new Thread(new ThreadStart(this.Save));
				savingThread.Priority = ThreadPriority.Lowest;
				savingThread.Start();
			}

			//---------------------------------------------------------------------
			public void Stop()
			{
				stopped = true;
				saveNeeded.Set();
				savingThread.Join();
			}
			//---------------------------------------------------------------------
			public void WaitUntilDocumentsHaveBeenSaved()
			{
				while (documents.Count > 0)
					Thread.Sleep(1);
			}

			//---------------------------------------------------------------------
			public void Add(LocalizerDocument d, string path)
			{
				lock (documents)
				{
					documents.Add(d);
					paths.Add(path);
					Debug.WriteLine(string.Format("Added {0}", path));
					saveNeeded.Set();
				}
			}

			//---------------------------------------------------------------------
			public void Save()
			{
				while (!stopped)
				{
					saveNeeded.WaitOne();
					while (documents.Count > 0)
					{
						try
						{
							LocalizerDocument d;
							string path;
							lock (documents)
							{
								d = documents[0] as LocalizerDocument;
								path = paths[0] as string;
								documents.RemoveAt(0);
								paths.RemoveAt(0);

								Debug.WriteLine(string.Format("Saving {0}", path));

							}

							LocalizerDocument.SaveStandardXmlDocumentProc(path, d);
						}
						catch (Exception ex)
						{
							Debug.Fail(ex.Message);
						}
					}

					saveNeeded.Reset();
				}
			}
		}


		static SavingContext savingContext = new SavingContext();
		static NameTable dictionaryNameTable = new NameTable();
		static Hashtable dictionaryDocuments = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		static StringCollection dictionaryDocumentCounter = new StringCollection();
		static int cacheSize = 2000;
		static int cacheReducedSize = Convert.ToInt32((float)cacheSize * 0.70);

		public string Filename = "";

		//---------------------------------------------------------------------
		public LocalizerDocument()
		{
		}

		//---------------------------------------------------------------------
		public LocalizerDocument(NameTable nt)
			: base(nt)
		{
		}
		//---------------------------------------------------------------------
		internal static void StopSavingContext()
		{
			savingContext.Stop();
		}

		//---------------------------------------------------------------------
		internal static void RemoveAllDictionariesFromCache()
		{
			dictionaryDocuments.Clear();
			dictionaryDocumentCounter.Clear();
			if (System.GC.GetTotalMemory(false) > 50000000)
				System.GC.Collect();
		}

		//---------------------------------------------------------------------
		internal void RemoveDictionaryFromCache()
		{
			RemoveDictionaryFromCache(Filename);
		}

		//---------------------------------------------------------------------
		internal static void RemoveDictionaryFromCache(string file)
		{
			dictionaryDocuments.Remove(file);
			dictionaryDocumentCounter.Remove(file);
		}

		/// <summary>
		/// Carica in un document un file gestendo l'errore; può essere un normale xml oppure un resx 
		/// (in questo caso lo trasforma in un xml nel formato standard)
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static LocalizerDocument GetStandardXmlDocument(string path, bool cached)
		{
			lock (typeof(LocalizerDocument))
			{
				LocalizerDocument doc = null;
				if (cached)
				{
					doc = dictionaryDocuments[path] as LocalizerDocument;
					if (doc != null)
					{
						dictionaryDocumentCounter.Remove(path);
						dictionaryDocumentCounter.Insert(0, path);
						return doc;
					}
				}
				
				try
				{
					DictionaryDocument document = null;

					if (CommonFunctions.IsXML(path))
						document = MergeFilesXML(path);
					else if (CommonFunctions.IsResx(path))
						document = MergeFilesResx(path);

					if (document == null)
						return null;

					doc = document.Document;
					return doc;
				}
				catch (Exception exc)
				{
					throw new ApplicationException(string.Format("Error opening document {0}\r\n{1}", path, exc.Message));
				}
				finally
				{
					if (dictionaryDocumentCounter.Count > cacheSize)
					{
						while (dictionaryDocumentCounter.Count > cacheReducedSize)
							RemoveDictionaryFromCache(dictionaryDocumentCounter[dictionaryDocumentCounter.Count - 1]);
					}

					dictionaryDocuments[path] = doc;
					dictionaryDocumentCounter.Insert(0, path);
					if (doc != null)
					{
						doc.Filename = path;
					
						//perform this operation only for chached documents to avoid recursion
						RecoverOldTranslations(path, doc);
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		public static bool GetReferencedNodeElements(XmlElement stringNode, out string nodePath, out string id)
		{
			nodePath = "";
			id = "";
			string oldNode = stringNode.GetAttribute(AllStrings.oldNode);
			if (oldNode.Length == 0)
				return false;
			
			int index = oldNode.LastIndexOf('-');
			if (index == -1)
				return false;

			nodePath = oldNode.Substring(0, index);
			id = oldNode.Substring(index + 1);
			return nodePath.Length > 0 && id.Length > 0;
		}

		//--------------------------------------------------------------------------------
		private static void RecoverOldTranslations(string path, LocalizerDocument document)
		{
			string culture = CommonFunctions.GetCulture(path);
			foreach (XmlElement stringNode in document.SelectNodes(string.Format("//string[@{0}]", AllStrings.oldNode)))
			{
				if (stringNode.GetAttribute(AllStrings.target).Length == 0)
				{
					string nodePath, id;
					if (!GetReferencedNodeElements(stringNode, out nodePath, out id))
						continue;

					LocalizerTreeNode node = null;
					nodePath = CommonFunctions.GetCorrespondingLanguagePath(nodePath, culture);
					DictionaryCreator.MainContext.GetNodeFromPath(nodePath, out node);
					
					if (node == null)
						continue;
					
					XmlElement resource = ((DictionaryTreeNode)node).GetResourceNode();
					if (resource != null)
					{
						string xPath = string.Format
							(
							"string{0}",
							CommonFunctions.XPathWhereClause(AllStrings.id, id)
							);
						XmlElement targetStringNode = resource.SelectSingleNode(xPath) as XmlElement;
						if (targetStringNode != null)
							stringNode.SetAttribute(AllStrings.target, targetStringNode.GetAttribute(AllStrings.target));
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		public static void RetrieveHelperInfos(XmlElement stringElement, out bool xmlTemporary, out bool xmlCurrent, out string matchType)
		{
			xmlTemporary = false;
			xmlCurrent = false;

			XmlNode valueNode = stringElement.SelectSingleNode("./@" + AllStrings.temporary);
			if (valueNode != null)
			{
				try { xmlTemporary = bool.Parse(valueNode.Value); }
				catch (FormatException ex) { Debug.Fail(ex.Message); }
			}

			valueNode = stringElement.SelectSingleNode("./@" + AllStrings.current);
			if (valueNode != null)
			{
				try { xmlCurrent = bool.Parse(valueNode.Value); }
				catch (FormatException ex) { Debug.Fail(ex.Message); }
			}

			matchType = stringElement.GetAttribute(AllStrings.matchType);
		}

		//--------------------------------------------------------------------------------
		internal static DictionaryDocument MergeFilesResx(string fileDictionary)
		{
			return MergeFilesResx(fileDictionary, false, null);
		}

		//--------------------------------------------------------------------------------
		internal static DictionaryDocument MergeFilesResx(string fileDictionary, bool trustPath, string resourceName)
		{
			string baseLanguageFileDictionary =
				trustPath
				? fileDictionary
				: CommonFunctions.GetCorrespondingBaseLanguagePath(fileDictionary);

			bool fromBase = baseLanguageFileDictionary == fileDictionary;

			//namespace . filename . lingua . resx
			LocalizerDocument baseLanguageDictionary = null;
			LocalizerDocument dictionary = null;
			try
			{
				if (!File.Exists(baseLanguageFileDictionary))
					return null;

				baseLanguageDictionary = new LocalizerDocument(dictionaryNameTable);
				baseLanguageDictionary.Load(baseLanguageFileDictionary);
			}
			catch (Exception exc)
			{
				throw new ApplicationException(string.Format("Error loading dictionary file {0}: '{1}'", baseLanguageFileDictionary, exc.Message));
			}
			
			if (!fromBase)
			{
				string realFileDictionary = CommonFunctions.GetPhysicalDictionaryPath(fileDictionary);
				try
				{
					if (File.Exists(realFileDictionary))
					{
						dictionary = new LocalizerDocument(dictionaryNameTable);
						dictionary.Load(realFileDictionary);
					}
				}
				catch (Exception exc)
				{
					throw new ApplicationException(string.Format("Error loading dictionary file {0}: '{1}'", realFileDictionary, exc.Message));
				}
			}
			

			bool isForm;
			XmlNodeList listOfText = DataDocumentFunctions.GetResxData(baseLanguageDictionary, out isForm);

			if (resourceName == null)
				resourceName = Path.GetFileNameWithoutExtension(baseLanguageFileDictionary);
			string resourceBlockType = isForm ? AllStrings.forms : AllStrings.strings;
			DictionaryFile dFile = new DictionaryFile(fileDictionary);
			DictionaryDocument dictionaryDocument = dFile.GetResource(resourceName, resourceBlockType);

			XmlElement resourceNode = dictionaryDocument.WriteResource(AllStrings.resxTag, resourceName);

			string oldStringsFile = CommonFunctions.GetOldResxDictionaryFile(baseLanguageFileDictionary);
			LocalizerXmlNodeList baseLanguageNodes = new LocalizerXmlNodeList();
			if (File.Exists(oldStringsFile))
			{
				LocalizerDocument d = new LocalizerDocument(dictionaryNameTable);
				d.Load(oldStringsFile);
				baseLanguageNodes.AddRange(d.SelectNodes("//string"));
			}

			baseLanguageNodes.AddRange(listOfText);

			WriteDictionaryResxStrings(resourceNode, baseLanguageNodes, resourceBlockType, resourceName, dictionary, !fromBase);

			dictionaryDocument.CloseResource();
			return dictionaryDocument;
		}

		//---------------------------------------------------------------------
		private static void WriteDictionaryResxStrings(
			XmlElement resourceNode,
			LocalizerXmlNodeList baseLanguageNodes,
			string resourceBlockType,
			string resourceName,
			LocalizerDocument targetDictionary,
			bool verifyTarget
			)
		{
			foreach (XmlElement baseNode in baseLanguageNodes)
			{
				string baseString = "";
				if (baseNode.LocalName == AllStrings.stringTag)
					baseString = baseNode.GetAttribute(AllStrings.baseTag);
				else
				{
					XmlNodeList list = baseNode.GetElementsByTagName(AllStrings.valueTag);
					if (list.Count == 1)
						baseString = list[0].InnerText;
				}
				baseString = baseString.Trim(' ');

				if (baseString.Length == 0)
					continue;

				XmlElement stringNode = resourceNode.OwnerDocument.CreateElement(AllStrings.stringTag);
				stringNode.SetAttribute(AllStrings.baseTag, baseString);
				foreach (XmlAttribute attr in baseNode.Attributes)
					stringNode.SetAttribute(attr.LocalName, attr.Value);

				int startIndex = 0;
				if (!MatchTranslation(resourceBlockType, resourceName, resourceName, AllStrings.resxTag, stringNode, targetDictionary, GetIdList(stringNode, baseLanguageNodes), ref startIndex)
					&&
					verifyTarget
					&&
					stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag
					)
					continue;

				resourceNode.AppendChild(stringNode);
			}
		}

		//---------------------------------------------------------------------
		private static BaseLanguageItem[] GetIdList(XmlElement stringNode, XmlNodeList baseLanguageNodes)
		{

			ArrayList baseLanguageItemList = new ArrayList();
			baseLanguageItemList.Add(new BaseLanguageItem(stringNode.GetAttribute(AllStrings.id), stringNode.GetAttribute(AllStrings.baseTag)));

			string tempId = stringNode.GetAttribute(AllStrings.temporaryId);
			while (tempId.Length > 0)
			{
				string nextId = "";
				foreach (XmlElement baseStringNode in baseLanguageNodes)
				{
					if (baseStringNode.GetAttribute(AllStrings.id) == tempId)
					{
						baseLanguageItemList.Add(new BaseLanguageItem(tempId, baseStringNode.GetAttribute(AllStrings.baseTag)));
						nextId = baseStringNode.GetAttribute(AllStrings.temporaryId);
						break;
					}
				}

				tempId = nextId;
			}

			return (BaseLanguageItem[])baseLanguageItemList.ToArray(typeof(BaseLanguageItem));
		}

		//Salva partendo da una struttura xml nostra le modifiche nei file resx
		//---------------------------------------------------------------------
		internal static void SaveResxFromXml(LocalizerDocument xmlDocument, string fileDictionary, bool onlyMergeTranslations, Logger logWriter)
		{
			XmlDocument copy = xmlDocument.Clone() as XmlDocument;
			ArrayList toRemove = new ArrayList();
			XmlNodeList list = copy.SelectNodes("//string");
			foreach (XmlElement stringNode in list)
			{
				if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
				{
					continue;
				}
				else
				{
					toRemove.Add(stringNode);
				}
			}

	
			LocalizerDocument resxDoc = new LocalizerDocument(dictionaryNameTable);
			resxDoc.Load(fileDictionary);
			// all strings are invalid
			if (toRemove.Count == 0)
			{
				bool isForm;
				foreach  (XmlElement el in DataDocumentFunctions.GetResxData(resxDoc, out isForm))
				{
					el.ParentNode.RemoveChild(el);
				}

			}
			else
			{
				foreach (XmlElement stringNode in toRemove)
				{
					foreach (XmlElement data in DataDocumentFunctions.GetResxDataByName(resxDoc, stringNode.GetAttribute(AllStrings.name)))
					{
						if (onlyMergeTranslations)
						{
							string target = stringNode.GetAttribute(AllStrings.target);

							if (target.Length > 0)
							{
								string baseString = stringNode.GetAttribute(AllStrings.baseTag);
								PlaceHolderValidity phv = CommonFunctions.IsPlaceHolderValid(baseString, target, CommonFunctions.ParametersMode.CS, false);
								if (phv.TranslationValid && phv.SequenceValid)
								{
									XmlText valueNode = data.SelectSingleNode("value/text()") as XmlText;
									valueNode.Value = target;
								}
								else
								{
									logWriter.WriteLog(string.Format("Invalid placeholder translation: '{0}' -> '{1}'; file: {2};\r\ntranslation ignored", baseString, target, fileDictionary), TypeOfMessage.warning);
								}
							}
						}
						else
						{
							data.SetAttribute(AllStrings.id, stringNode.GetAttribute(AllStrings.id));
							if (stringNode.HasAttribute(AllStrings.temporaryId))
								data.SetAttribute(AllStrings.temporaryId, stringNode.GetAttribute(AllStrings.temporaryId));
							if (stringNode.HasAttribute(AllStrings.matchType))
								data.SetAttribute(AllStrings.matchType, stringNode.GetAttribute(AllStrings.matchType));

						}
					}

					if (!onlyMergeTranslations)
						Functions.RemoveNodeAndEmptyAncestors(stringNode);

				}

				if (!onlyMergeTranslations)
				{
					//if there are nodes without id attribute, remove it!
					bool isForm;
					foreach (XmlElement el in DataDocumentFunctions.GetResxData(resxDoc, out isForm))
					{
						if (el.GetAttribute(AllStrings.id).Length == 0)
							el.ParentNode.RemoveChild(el);
					}
				}
			}
			
			resxDoc.Save(fileDictionary);

			if (!onlyMergeTranslations && toRemove.Count != list.Count)
				SaveXmlFromXml(copy, CommonFunctions.GetOldResxDictionaryFile(fileDictionary));

		}

		//---------------------------------------------------------------------
		internal static bool SaveStandardXmlDocument(string path, LocalizerDocument doc)
		{
			if (!CommonFunctions.TryToCheckOut(CommonFunctions.GetPhysicalDictionaryPath(path)))
				return false;

			savingContext.Add(doc, path);
			return true;
		}

		//---------------------------------------------------------------------
		internal static bool SaveStandardXmlDocumentProc(string path, LocalizerDocument doc)
		{
			try
			{

				if (CommonFunctions.IsXML(path))
				{
					SaveXmlFromXml(doc, path);
				}
				else if (CommonFunctions.IsResx(path))
				{
					SaveXmlFromXml(doc, path);
				}

				doc.RemoveDictionaryFromCache();
			}
			catch (UnauthorizedAccessException exc)
			{
				throw exc;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		internal static DictionaryDocument MergeFilesXML(string fileDictionary)
		{
			string baseLanguageFile = CommonFunctions.GetCorrespondingBaseLanguagePath(fileDictionary);

			if (!File.Exists(baseLanguageFile))
				return null;

			LocalizerDocument baseDictionary = new LocalizerDocument(dictionaryNameTable);
			baseDictionary.Load(baseLanguageFile);

			if (baseLanguageFile == fileDictionary)
				return new DictionaryDocument(baseDictionary, baseDictionary.DocumentElement);

			ArrayList toRemove = new ArrayList();
			if (!File.Exists(fileDictionary))
			{
				foreach (XmlElement stringNode in baseDictionary.SelectNodes("//string"))
				{
					if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
						toRemove.Add(stringNode);
				}
			}
			else
			{
				LocalizerDocument targetDictionary = new LocalizerDocument(dictionaryNameTable);
				targetDictionary.Load(fileDictionary);
				foreach (XmlElement resourceBlock in baseDictionary.DocumentElement.ChildNodes)
				{
					string resourceBlockType = resourceBlock.LocalName;
					foreach (XmlElement resourceGroup in resourceBlock.ChildNodes)
					{
						string groupIdentifier = resourceGroup.GetAttribute(AllStrings.id);
						foreach (XmlElement resource in resourceGroup.ChildNodes)
						{
							string resourceName = resource.GetAttribute(AllStrings.name);
							string resourceType = resource.LocalName;
							XmlNodeList baseLanguageNodes = resource.ChildNodes;
							foreach (XmlElement stringNode in baseLanguageNodes)
							{
								if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
								{
									bool isRemoved = false;
									foreach (XmlElement tmpNode in stringNode.ParentNode.ChildNodes)
									{
										if (tmpNode != stringNode
											&& tmpNode.GetAttribute(AllStrings.id) == stringNode.GetAttribute(AllStrings.id)
											&& tmpNode.GetAttribute(AllStrings.valid) != AllStrings.falseTag)
										{
											toRemove.Add(stringNode);
											isRemoved = true;
											break;
										}
									}
									if (isRemoved)
										continue;
								}

								int startIndex = 0;
								if (!MatchTranslation(resourceBlockType, groupIdentifier, resourceName, resourceType, stringNode, targetDictionary, GetIdList(stringNode, baseLanguageNodes), ref startIndex)
									&&
									stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
									toRemove.Add(stringNode);
							}
						}
					}
				}
			}

			foreach (XmlElement stringNode in toRemove)
				stringNode.ParentNode.RemoveChild(stringNode);

			return new DictionaryDocument(baseDictionary, baseDictionary.DocumentElement);
		}

		//---------------------------------------------------------------------
		private static bool MatchTranslation
			(
			string resourceBlockType,
			string groupIdentifier,
			string resourceName,
			string resourceType,
			XmlElement stringNode,
			LocalizerDocument targetDictionary,
			BaseLanguageItem[] baseLanguageItemList,
			ref int currentId
			)
		{
			if (targetDictionary == null)
				return false;
			
			string xPath = string.Format
				(
				"resources/{0}/resources{1}/{2}{3}/string{4}",
				resourceBlockType,
				CommonFunctions.XPathWhereClause(AllStrings.id, groupIdentifier),
				resourceType,
				CommonFunctions.XPathWhereClause(AllStrings.name, resourceName),
				CommonFunctions.XPathWhereClause(AllStrings.id, baseLanguageItemList[currentId].Id)
				);

			XmlElement targetStringNode = targetDictionary.SelectSingleNode(xPath) as XmlElement;
			if (targetStringNode == null)
			{
				if (++currentId == baseLanguageItemList.Length)
					return false;

				if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
					return false;

				return MatchTranslation(resourceBlockType, groupIdentifier, resourceName, resourceType, stringNode, targetDictionary, baseLanguageItemList, ref currentId);
			}

			foreach (XmlAttribute attr in targetStringNode.Attributes)
			{
				if (attr.Name == AllStrings.baseTag ||
					attr.Name == AllStrings.id ||
					attr.Name == AllStrings.name ||
					attr.Name == AllStrings.valid)
					continue;

				stringNode.SetAttribute(attr.Name, attr.Value);
			}
			if (currentId > 0)
			{
				stringNode.SetAttribute(AllStrings.temporary, AllStrings.trueTag);
				DictionaryDocument.StringComparisonFlags flag;
				DictionaryDocumentFunctions.MatchString
					(
					stringNode.GetAttribute(AllStrings.baseTag),
					baseLanguageItemList[currentId].Base,
					DictionaryDocument.StringComparisonFlags.IGNORE_ALL,
					out flag
					);

				stringNode.SetAttribute(AllStrings.matchType, ((int)flag).ToString());
			}

			return true;
		}

		//---------------------------------------------------------------------
		internal static void SaveXmlFromXml(XmlDocument xmlDocument, string fileDictionary)
		{
			string baseLanguageFile = CommonFunctions.GetCorrespondingBaseLanguagePath(fileDictionary);
			if (baseLanguageFile == fileDictionary)
			{
				CommonFunctions.TryToCheckOut(fileDictionary);
				xmlDocument.Save(fileDictionary);
				return;
			}

			XmlDocument copy = xmlDocument.Clone() as XmlDocument;
			ArrayList toRemove = new ArrayList();
			XmlNodeList list = copy.SelectNodes("//string");
			foreach (XmlElement stringNode in list)
			{
				bool isRemoved = false;
				if (stringNode.GetAttribute(AllStrings.target).Length == 0)
				{
					toRemove.Add(stringNode);
					isRemoved = true;
				}
				
				if (isRemoved)
					continue;

				if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
				{
					foreach (XmlElement targetnode in stringNode.ParentNode.ChildNodes)
					{
						if (targetnode != stringNode
							&& targetnode.GetAttribute(AllStrings.id) == stringNode.GetAttribute(AllStrings.id)
							&& targetnode.GetAttribute(AllStrings.valid) != AllStrings.falseTag)
						{
							toRemove.Add(stringNode);
							isRemoved = true;
							break;
						}
					}
				}

				if (isRemoved)
					continue;

				stringNode.RemoveAttribute(AllStrings.oldNode);
				stringNode.RemoveAttribute(AllStrings.baseTag);
				stringNode.RemoveAttribute(AllStrings.valid);
				stringNode.RemoveAttribute(AllStrings.name);
				stringNode.RemoveAttribute(AllStrings.support);
				stringNode.RemoveAttribute(AllStrings.supportTemporary);
			}

			//no string has been translated
			if (list.Count == toRemove.Count)
			{
				if (File.Exists(fileDictionary))
					File.Delete(fileDictionary);
				return;
			}

			foreach (XmlElement el in toRemove)
				Functions.RemoveNodeAndEmptyAncestors(el);

			if (CommonFunctions.IsResx(fileDictionary))
			{
				DictionaryFile dFile = new DictionaryFile(DictionaryFile.ResxDictionaryFileName);
				string root = null;
				CommonFunctions.GetCulture(fileDictionary, out root);

				ResourceGroup[] groups = DictionaryFile.ExtractResourceGroups(copy);
				if (groups.Length != 1)
					return;
				ResourceGroup group = groups[0];
				DictionaryDocument d = dFile.GetResource(group.GroupIdentifier, group.GroupType, root);
				if (d.Root.FirstChild != null)
					d.Root.RemoveChild(d.Root.FirstChild);
				d.Root.AppendChild(d.Document.ImportNode(group.ResourceNode, true));
				dFile.Save(root);
			}
			else
			{
				string folder = Path.GetDirectoryName(fileDictionary);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				CommonFunctions.TryToCheckOut(fileDictionary);
				copy.Save(fileDictionary);
			}
		}

		//--------------------------------------------------------------------------------
		public override void Load(string filename)
		{
            TryXTimes(() => base.Load(filename), typeof(IOException));
            
			this.Filename = filename;
		}

		//--------------------------------------------------------------------------------
		public override void Save(string filename)
		{
			string folder = Path.GetDirectoryName(filename);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			CommonFunctions.TryToCheckOut(filename);

            TryXTimes(() => base.Save(filename), typeof(IOException));

			RemoveDictionaryFromCache();
		}

        //--------------------------------------------------------------------------------
        private void TryXTimes(Action action, Type onExceptionType,  int times = 5)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception exc)
                {
                    if (exc.GetType() == onExceptionType)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(40);
                        if (retries > times)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

		//--------------------------------------------------------------------------------
		public override void Save(System.IO.Stream outStream)
		{
			throw new NotImplementedException(Strings.SaveNotImplemented);
		}

		//--------------------------------------------------------------------------------
		public override void Save(System.IO.TextWriter writer)
		{
			throw new NotImplementedException(Strings.SaveNotImplemented);
		}

		//--------------------------------------------------------------------------------
		public override void Save(XmlWriter w)
		{
			throw new NotImplementedException(Strings.SaveNotImplemented);
		}

		//--------------------------------------------------------------------------------
		public bool MergeStandardDocumentWith(string file)
		{
			LocalizerDocument d = GetStandardXmlDocument(file, false);
			if (d == null)
				return false;

			if (ChildNodes.Count == 0)
			{
				AppendChild(CreateXmlDeclaration(AllStrings.version, "utf-8", null));
			}

			if (DocumentElement == null)
			{
				AppendChild(ImportNode(d.DocumentElement, true));
				return true;
			}

			foreach (XmlElement el1 in d.DocumentElement.ChildNodes)
			{
				XmlNode resourceNode = DocumentElement.SelectSingleNode(el1.LocalName);
				if (resourceNode == null)
				{
					DocumentElement.AppendChild(ImportNode(el1, true));
					continue;
				}

				foreach (XmlElement el2 in el1.ChildNodes)
				{
					string id = el2.GetAttribute(AllStrings.id);
					XmlNode fileNode = resourceNode.SelectSingleNode("node()" + CommonFunctions.XPathWhereClause(AllStrings.id, id));
					if (fileNode == null)
					{
						resourceNode.AppendChild(ImportNode(el2, true));
						continue;
					}


					foreach (XmlElement el3 in el2.ChildNodes)
					{
						string name = el3.GetAttribute(AllStrings.name);
						XmlNode resourceBlockNode = fileNode.SelectSingleNode("node()" + CommonFunctions.XPathWhereClause(AllStrings.name, name));
						if (resourceBlockNode == null)
						{
							fileNode.AppendChild(ImportNode(el3, true));
							continue;
						}

						foreach (XmlElement el4 in el3.ChildNodes)
							resourceBlockNode.AppendChild(ImportNode(el4, true));
					}

				}
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public static void ReplaceIdInBaseLanguageFile(DictionaryTreeNode dictionaryNode, List<string> oldIds, List<string> nodePaths)
		{
			savingContext.WaitUntilDocumentsHaveBeenSaved();

			string baseLanguageFile = CommonFunctions.GetCorrespondingBaseLanguagePath(dictionaryNode.FileSystemPath);
			if (CommonFunctions.IsXML(baseLanguageFile))
			{
				LocalizerDocument dictionaryDocument = new LocalizerDocument();
				dictionaryDocument.Load(baseLanguageFile);
				XmlElement resourcenode = dictionaryDocument.GetResourceNode(dictionaryNode);
				foreach (XmlElement stringNode in resourcenode.ChildNodes)
				{
					if (oldIds.Count == 0)
						break;
					ReplaceIds(oldIds, nodePaths, stringNode);
				}
				
				dictionaryDocument.Save(baseLanguageFile);
			}
			else if (CommonFunctions.IsResx(baseLanguageFile))
			{ 
				LocalizerDocument dictionaryDocument = new LocalizerDocument();
				dictionaryDocument.Load(baseLanguageFile);
				bool isForm;
				foreach (XmlElement stringNode in DataDocumentFunctions.GetResxData(dictionaryDocument, out isForm))
				{ 
					if (oldIds.Count == 0)
					    break;
					ReplaceIds(oldIds, nodePaths, stringNode);
				}

				dictionaryDocument.Save(baseLanguageFile);
			
			}
			else
			{
				Debug.Fail("Invalid file type");
			}

			DictionaryCreator.MainContext.RefreshTree();
		}
		//--------------------------------------------------------------------------------
		public static void RemoveReferenceInBaseLanguageFile(DictionaryTreeNode dictionaryNode, string nodePath)
		{
			savingContext.WaitUntilDocumentsHaveBeenSaved();

			string baseLanguageFile = CommonFunctions.GetCorrespondingBaseLanguagePath(dictionaryNode.FileSystemPath);
			if (CommonFunctions.IsXML(baseLanguageFile))
			{
				LocalizerDocument dictionaryDocument = new LocalizerDocument();
				dictionaryDocument.Load(baseLanguageFile);
				XmlElement resourcenode = dictionaryDocument.GetResourceNode(dictionaryNode);
				foreach (XmlElement stringNode in resourcenode.ChildNodes)
				{
					string nodePathTarget, id;
					if (GetReferencedNodeElements(stringNode, out nodePathTarget, out id) &&
						string.Compare(nodePathTarget, nodePath, true) == 0)
						stringNode.RemoveAttribute(AllStrings.oldNode);
				}
				
				dictionaryDocument.Save(baseLanguageFile);
			}
			else if (CommonFunctions.IsResx(baseLanguageFile))
			{ 
				LocalizerDocument dictionaryDocument = new LocalizerDocument();
				dictionaryDocument.Load(baseLanguageFile);
				bool isForm;
				foreach (XmlElement stringNode in DataDocumentFunctions.GetResxData(dictionaryDocument, out isForm))
				{ 
					string nodePathTarget, id;
					if (GetReferencedNodeElements(stringNode, out nodePathTarget, out id) &&
						string.Compare(nodePathTarget, nodePath, true) == 0)
						stringNode.RemoveAttribute(AllStrings.oldNode);
				}

				dictionaryDocument.Save(baseLanguageFile);
			
			}
			else
			{
				Debug.Fail("Invalid file type");
			}

			DictionaryCreator.MainContext.RefreshTree();
		}

		//--------------------------------------------------------------------------------
		private static void ReplaceIds(List<string> oldIds, List<string> nodePaths, XmlElement stringNode)
		{
			string id = stringNode.GetAttribute(AllStrings.id);
			for (int i = 0; i < oldIds.Count; i++)
			{
				if (oldIds[i] == id)
				{
					string nodePath = nodePaths[i];
					if (nodePath.Length > 0)
						stringNode.SetAttribute(AllStrings.oldNode, nodePath);
					oldIds.RemoveAt(i);
					nodePaths.RemoveAt(i);
					break;
				}
			}
		}

		//--------------------------------------------------------------------------------
		public XmlElement GetResourceNode(DictionaryTreeNode dictionaryNode)
		{
			foreach (XmlElement resource in DocumentElement.GetElementsByTagName(dictionaryNode.ResourceType))
			{
				foreach (XmlNode group in resource.ChildNodes)
				{
					if (!(group is XmlElement))
						continue;

					if (string.Compare(((XmlElement)group).GetAttribute(AllStrings.id), dictionaryNode.GroupIdentifier, true) != 0)
						continue;

					foreach (XmlNode node in group.ChildNodes)
					{
						if (!(node is XmlElement))
							continue;

						if (string.Compare(((XmlElement)node).GetAttribute(AllStrings.name), dictionaryNode.Name, true) != 0)
							continue;

						return (XmlElement)node;
					}
				}
			}
			return null;
		}
	}	
}
