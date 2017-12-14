using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;
using System.Diagnostics;


namespace Microarea.Tools.TBLocalizer
{
	internal class ImportExport
	{
		private const string xmlVersion = "1.0";
		private string baseOrSupport;

		DictionaryCreator dictionaryCreator = DictionaryCreator.MainContext;
		Logger logWriter;
        private static ProjectFilterDialog filterDialog = new ProjectFilterDialog();

        //---------------------------------------------------------------------
        private bool ApplyFilter { get { return filterDialog.ApplyFilter; } }
        //---------------------------------------------------------------------
        private string[] Filters { get { return filterDialog.Filters; } }

        public ImportExport(Logger logWriter)
		{
			this.logWriter = logWriter;
			baseOrSupport = SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable ? AllStrings.support : AllStrings.baseTag;
		}

		//---------------------------------------------------------------------
		private void ShowFileListOptions(string filePath, string nodeName, FileListOptions.ModeType mode)
		{
			FileListOptions dialog = new FileListOptions(filePath, nodeName, mode);
			dialog.ShowDialog(dictionaryCreator);
		}

		//---------------------------------------------------------------------
		private bool OkTranslationTable(LocalizerTreeNode node, string ext, string filter, string culture, out string file)
		{
			string nodeName = node.Name;
			SaveFileDialog saveDialog = new SaveFileDialog();
            string fileName = String.Format(AllStrings.TRANSLATIONTABLE, nodeName, culture, ext);
			saveDialog.FileName = fileName;
			saveDialog.Title = Strings.SaveFileList;
			saveDialog.Filter = filter;
			DialogResult r = saveDialog.ShowDialog(dictionaryCreator);
			file = saveDialog.FileName;
			return r == DialogResult.OK;
		}

        //---------------------------------------------------------------------
        private bool OkExportFolder(out string path)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = Strings.SaveFileList;
            DialogResult r = folderDialog.ShowDialog(dictionaryCreator);
            path = folderDialog.SelectedPath;
            return r == DialogResult.OK;
        }

        //---------------------------------------------------------------------
        public bool SelectFilters(LocalizerTreeNode node)
        {
            filterDialog.GlobalFiltersOnly = true;

            if (filterDialog.ShowDialog(null) != DialogResult.OK)
                return false;

            return true;
        }

        //---------------------------------------------------------------------
        private bool NodeToSkip(DictionaryTreeNode aNode)
        {
            if (!ApplyFilter)
                return false;

            foreach (string filter in Filters)
                if (string.Compare(filter, aNode.ResourceType, true) == 0)
                    return false;

            return true;
        }

        //---------------------------------------------------------------------
		private string WriteTranslationTable
            (
                LocalizerTreeNode selectedNode, 
                string culture, 
                string filePath, 
                string nodeName, 
                bool translated, 
                bool notTranslated,
                bool on3Columns,
                bool baseStringAsAttribute,
                bool fillWithSupport,
                bool skipUntranslatable,
                out bool bOneFound
            )
		{
            bOneFound = false;

            logWriter.WriteLog(string.Format("Exporting {0} ({1})", nodeName, filePath), TypeOfMessage.info);

			HtmlWriter html = new HtmlWriter();
			try
			{
				dictionaryCreator.Cursor = Cursors.WaitCursor;
				html.Open(Strings.TranslationTable);

				//html.WriteParagraph(String.Format(Strings.Legend, selectedNode.FullPath, "<br>"));

				foreach (DictionaryTreeNode node in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, culture))
				{
                    if (NodeToSkip(node))
                        continue;
                    
                    if (SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable)
					{
						if (!node.MergeWithSupport(SolutionDocument.LocalInfo.SupportLanguage))
							continue;
					}

					//html.Newline();

					XmlNodeList list = node.GetStringNodes();
					if (list == null) continue;
					ArrayHashTable tableList = CommonFunctions.FilterPerItem(list);
					foreach (DictionaryEntry de in tableList)
					{
                        bool bFirstTableLine = true;
                        bool bIsDatabaseScript = false;

						ArrayList elements = de.Value as ArrayList;
						foreach (XmlElement n in elements)
						{
							//SCRIVI TABELLA TRADUZIONE
							bool valid = true;
                            if (n.HasAttribute(AllStrings.valid))
                            {
                                try { valid = bool.Parse(n.GetAttribute(AllStrings.valid)); }
                                catch { }
                            }

                            // always skip invalid strings
                            if (!valid)
                                continue;

							//string temporarySymbol = HtmlWriter.Blank;
							bool temporary = false;
                            if (n.HasAttribute(AllStrings.temporary))
                            {
                                try { temporary = bool.Parse(n.GetAttribute(AllStrings.temporary)); }
                                catch { }
                            }
							//if (temporary) temporarySymbol = AllStrings.TemporaryRow;
							string target = String.Empty;
							target = n.GetAttribute(AllStrings.target);

							//se viene richiesta la stampa solo di quelli non tradotti, devo verificare che target sia diverso da stringa vuota
							// e che se c'è temporary, ci sia il fla di contarlo come tradotto o meno. 
							bool isTranslated = target != String.Empty && (!temporary || dictionaryCreator.CountTemporaryAsTranslated);

							if (isTranslated && !translated)
								continue;

							if (!isTranslated && !notTranslated)
								continue;

                            if (skipUntranslatable && n.HasAttribute(AllStrings.support))
                            {
                                if (n.GetAttribute(AllStrings.baseTag) == n.GetAttribute(AllStrings.support))
                                    continue;
                            }

                            if (bFirstTableLine)
                            {
                                //SCRIVI NOME RISORSA
                                //html.OpenDiv(node.FullPath);
                                //html.WriteRedParagraph(node.FullPath);
                                //html.WriteParagraph(HtmlWriter.Blank);
                                //html.WriteLine();
                                html.WriteEmptyLine();
                                //html.WriteParagraph(HtmlWriter.Blank);
                                html.OpenTable(node.FullPath, 1, 0, 1);
                                html.WriteTitleRow(node.FullPath);
                                bFirstTableLine = false;

                                bIsDatabaseScript = node.FullPath.Contains(AllStrings.database);
                            }
                            
                            //if (target == String.Empty) target = HtmlWriter.Blank;

							//string simbol = valid ? ((!temporary && target != HtmlWriter.Blank) ? HtmlWriter.Blank : AllStrings.NotTranslatedRow) : AllStrings.NotValidRow;

							html.OpenRow(n.GetAttribute(AllStrings.id), 0, false);

							//html.OpenColumn(2, true);
							//html.Write(simbol);
							//html.CloseColumn();

							//html.OpenColumn(2, true);
							//html.Write(temporarySymbol);
							//html.CloseColumn();

                            int nWidth = on3Columns ? 32 : 48;

                            if (baseStringAsAttribute)
                            {
                                html.OpenColumnWithAttribute(AllStrings.id, 4, true, AllStrings.refIdClass, AllStrings.baseTag, html.Encode(n.GetAttribute(AllStrings.baseTag)));
                                html.Write(html.Encode(n.GetAttribute(AllStrings.id)));
                                html.CloseColumn();
                            }
                            else
                            {
                                html.OpenColumn(AllStrings.id, 4, true, AllStrings.refIdClass);
                                html.Write(html.Encode(n.GetAttribute(AllStrings.id)));
                                html.CloseColumn();

                                html.OpenColumn(AllStrings.baseTag, nWidth, true, AllStrings.contentClass);
                                html.Write(html.Encode(n.GetAttribute(AllStrings.baseTag)));
                                html.CloseColumn();
                            }

                            if (on3Columns)
                            {
                                string support = n.GetAttribute(AllStrings.support);
                                if (bIsDatabaseScript)
                                    support = TrySeparate(support);
                                html.OpenColumn(AllStrings.support, nWidth, true, AllStrings.contentClass);
                                html.Write(html.Encode(support));
                                html.CloseColumn();
                            }

                            html.OpenColumn(AllStrings.target, nWidth, true, AllStrings.contentClass);

                            // some translators require to repeat the string to be translated in the target column 
                            if (fillWithSupport)
                            {
                                string support = n.GetAttribute(baseOrSupport);
                                if (baseOrSupport == AllStrings.support && bIsDatabaseScript)
                                    support = TrySeparate(support);
                                html.Write(html.Encode(support));
                            }
                            else
                                html.Write(html.Encode(target));
							html.CloseColumn();

                            html.CloseRow();

                            bOneFound = true;
                        }
                        if (!bFirstTableLine)
                        {
                            html.CloseTable();
                            //html.CloseDiv();
                        }
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(dictionaryCreator, ex.Message);
			}
			finally
			{
				html.Close();
				dictionaryCreator.Cursor = Cursors.Default;

			}
			return html.Source;

		}

        // try separate words in the column name, i.e.: TaxAmountRoundingDigit -> Tax Amount Rounding Digit
        private string TrySeparate(string columnName)
        {
            // already blanks in it, nothing to do
            if (columnName.Contains(' '))
                return columnName;

            string result = columnName;
            for (int i = 1; i < result.Count(); i++)
            {
                if (Char.IsUpper(result[i]) && !Char.IsUpper(result[i - 1]))
                {
                    result = result.Insert(i, " ");
                    i = i+2;
                }
                else if (i > 1 && !Char.IsUpper(result[i]) && Char.IsUpper(result[i - 1]) && result[i - 2] != ' ')
                {
                    result = result.Insert(i - 1, " ");
                    i = i + 1;
                }
            }

            return result;
        }

        //---------------------------------------------------------------------
        public void ExportToHtml(string culture, bool translated, bool nonTranslated)
		{

			LocalizerTreeNode n = dictionaryCreator.SelectedNode;
            bool bExported = false;

            AskHTMLExport ask = new AskHTMLExport();
            if (ask.ShowDialog() != DialogResult.OK)
                return;

            if (n.Type != NodeType.SOLUTION || ask.SeparateFiles == false)
            {
			    string file;
                if (
                        SelectFilters(n) &&
                        OkTranslationTable(n, AllStrings.htmExtension, AllStrings.FILTERHTM, culture, out file)
                   )
                {
                    string html = WriteTranslationTable
                        (
                        n,
                        culture,
                        file,
                        n.Name,
                        translated,
                        nonTranslated,
                        ask.On3Columns,
                        ask.BaseStringAsAttribute,
                        ask.FillTargetWithSupport,
                        ask.SkipUntranslatable,
                        out bExported
                        );

                    if (html == null || html == String.Empty || !bExported)
                    {
                        MessageBox.Show(dictionaryCreator, Strings.StringsNotFound, Strings.TranslatorCaption);
                        return;
                    }
                    SaveTranslationTable(file, html);
                    ShowFileListOptions(file, n.Name, FileListOptions.ModeType.TranslationTable);
                }
            }
            else
            {
 			    string path;
                if (
                        SelectFilters(n) &&
                        OkExportFolder(out path)
                   )
                {
                    string strTemplate = string.Empty;
                    if (translated && !nonTranslated)
                        strTemplate = AllStrings.GLOSSARYTABLE;
                    else if (!translated && nonTranslated)
                        strTemplate = AllStrings.TOBETRANSLATEDTABLE;
                    else
                        strTemplate = AllStrings.TRANSLATIONTABLE;

                    foreach (LocalizerTreeNode node in n.GetTypedChildNodes(NodeType.PROJECT, true, null, true, culture))
                    {
                        string fileName = Path.Combine(path, String.Format(strTemplate, node.Name, culture, AllStrings.htmExtension));
                        string html = WriteTranslationTable
                           (
                                node,
                                culture,
                                fileName,
                                node.Name,
                                translated,
                                nonTranslated,
                                ask.On3Columns,
                                ask.BaseStringAsAttribute,
                                ask.FillTargetWithSupport,
                                ask.SkipUntranslatable,
                                out bExported
                           );

                        if (html == null || html == String.Empty)
                        {
                            MessageBox.Show(dictionaryCreator, Strings.StringsNotFound, Strings.TranslatorCaption);
                            break;
                        }
                        if (bExported)
                            SaveTranslationTable(fileName, html);
                    }
               }
            }
		}

		//---------------------------------------------------------------------
		public void ExportToXml(string culture, bool translated, bool nonTranslated)
		{
			string file;
			LocalizerTreeNode selectedNode = dictionaryCreator.SelectedNode;
			if  (
                    SelectFilters(selectedNode) &&
                    OkTranslationTable(selectedNode, AllStrings.xmlExtension, AllStrings.FILTERXML, culture, out file)
                )
			{
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateXmlDeclaration(AllStrings.version, "utf-8", null));
				doc.AppendChild(doc.CreateElement("DictionaryNodes"));
				doc.DocumentElement.SetAttribute("version", xmlVersion);
				try
				{
					dictionaryCreator.Cursor = Cursors.WaitCursor;

					foreach (DictionaryTreeNode node in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, culture))
					{
						if (SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable)
							node.MergeWithSupport(SolutionDocument.LocalInfo.SupportLanguage);

						logWriter.WriteLog(string.Format(Strings.ProcessingNode, node.FullPath), TypeOfMessage.info);

						XmlElement nodeElement = doc.CreateElement("Node");

						nodeElement.SetAttribute("path", node.FullPath);

						foreach (XmlElement stringNode in node.GetStringNodes())
						{
							bool temporary = stringNode.GetAttribute(AllStrings.temporary) == AllStrings.trueTag;
							bool isTranslated = (!temporary || dictionaryCreator.CountTemporaryAsTranslated) && stringNode.GetAttribute(AllStrings.target) != String.Empty;

							if (!translated && isTranslated)
								continue;

							if (!nonTranslated && !isTranslated)
								continue;

							nodeElement.AppendChild(doc.ImportNode(stringNode.CloneNode(true), true));

						}

						if (nodeElement.HasChildNodes)
							doc.DocumentElement.AppendChild(nodeElement);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(dictionaryCreator, ex.Message);
				}
				finally
				{
					dictionaryCreator.Cursor = Cursors.Default;
				}

				doc.Save(file);

				ShowFileListOptions(file, selectedNode.Name, FileListOptions.ModeType.TranslationTable);

			}
		}

		//---------------------------------------------------------------------
		public void ImportXml()
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = AllStrings.FILTERXML;
			if (DialogResult.OK != ofd.ShowDialog())
				return;

			try
			{
				dictionaryCreator.Cursor = Cursors.WaitCursor;

				XmlDocument doc = new XmlDocument();
				doc.Load(ofd.FileName);

				if (doc.DocumentElement.GetAttribute("version") != xmlVersion)
				{
					logWriter.WriteLog("Invalid file version", TypeOfMessage.error);
					return;
				}
				foreach (XmlElement nodeEl in doc.DocumentElement.ChildNodes)
				{
					bool modified = false;

					string nodePath = nodeEl.GetAttribute("path");
					logWriter.WriteLog(string.Format(Strings.ProcessingNode, nodePath), TypeOfMessage.info);

					LocalizerTreeNode node;
					if (!dictionaryCreator.GetNodeFromPath(nodePath, out node))
					{
						logWriter.WriteLog(string.Format(Strings.NodeNotFound, nodePath), TypeOfMessage.error);
						continue;
					}

					DictionaryTreeNode dictNode = node as DictionaryTreeNode;
					if (dictNode == null)
					{
						logWriter.WriteLog(string.Format(Strings.InvalidNode, nodePath), TypeOfMessage.error);
						continue;
					}
					XmlNodeList targetNodes = dictNode.GetStringNodes();
					foreach (XmlElement sourceStringNode in nodeEl.ChildNodes)
					{
						if (sourceStringNode.GetAttribute(AllStrings.target) == string.Empty)
							continue;

						XmlElement targetStringNode = null;
						foreach (XmlElement n in targetNodes)
						{
							if (n.GetAttribute(AllStrings.baseTag) == sourceStringNode.GetAttribute(AllStrings.baseTag))
							{
								targetStringNode = n;
								break;
							}
						}

						if (targetStringNode == null)
						{
                            logWriter.WriteLog(string.Format(Strings.StringNodeNotFound, sourceStringNode.GetAttribute(AllStrings.id), sourceStringNode.GetAttribute(AllStrings.baseTag)), TypeOfMessage.error);
							continue;
						}

						if (targetStringNode.GetAttribute(AllStrings.target) != string.Empty)
						{
							logWriter.WriteLog(string.Format(Strings.StringNodeAlreadyTranslated, sourceStringNode.GetAttribute(AllStrings.id), sourceStringNode.GetAttribute(AllStrings.baseTag)), TypeOfMessage.warning);
							continue;
						}

						targetStringNode.SetAttribute(AllStrings.target, sourceStringNode.GetAttribute(AllStrings.target));
						modified = true;
					}

					if (modified)
					{
						dictNode.SaveToFileSystem();
					}
				}
			}
			catch (Exception ex)
			{
				logWriter.WriteLog(ex.Message, TypeOfMessage.error);

			}
			finally
			{
				dictionaryCreator.Cursor = Cursors.Default;
			}
		}

		//---------------------------------------------------------------------
        public void ImportHTML()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = AllStrings.FILTERHTM;
            ofd.Multiselect = true;
            if (DialogResult.OK != ofd.ShowDialog())
                return;

            AskHTMLImport ask = new AskHTMLImport();
            if (DialogResult.OK != ask.ShowDialog())
                return;

            foreach (string fn in ofd.FileNames)
            {
                try
                {
                    dictionaryCreator.Cursor = Cursors.WaitCursor;

                    WebBrowser wB = new WebBrowser();
                    wB.Navigate(fn);
                    // Attendo il caricamento del documento
                    while (wB.Document.Body == null)
                        Application.DoEvents();

                    Dictionary<string, LocalizerDocument> modifiedNodes = new Dictionary<string,LocalizerDocument>();
                    HtmlElement hElem = wB.Document.Body;

                    DictionaryTreeNode dictNode = null;
                    ParseHTMLElement(hElem, ref modifiedNodes, ask.ImportFlags, ref dictNode);

                    foreach (KeyValuePair<string, LocalizerDocument> p in modifiedNodes)
                        LocalizerDocument.SaveStandardXmlDocument(p.Key, p.Value);

                }
                catch (Exception ex)
                {
                    logWriter.WriteLog(ex.Message, TypeOfMessage.error);

                }
                finally
                {
                    dictionaryCreator.Cursor = Cursors.Default;
                }
            }

            logWriter.StatusBarLog("Completed");

            dictionaryCreator.UpdateDetailsAsync();
        }

        //---------------------------------------------------------------------
        private void FindNodePath(HtmlElement hTable, out string nodePath)
        {
            nodePath = string.Empty;
            // well-formed HTML, the node path is in the id of the table node
            if (hTable.Id != null && hTable.Id.Length > 0)
            {
                nodePath = hTable.Id;
                return;
            }

            // id was removed (i.e.: HTML saved by Word), extract the node path from the first column - first row of the table
            foreach (HtmlElement hBody in hTable.Children)
            {
                if (!hBody.TagName.Equals("tbody", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                // search for row children
                foreach (HtmlElement hPath in hBody.Children)
                {
                    if (!hPath.TagName.Equals("tr", StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    nodePath = hPath.InnerText;
                    break;
                }
            }
       }

        //---------------------------------------------------------------------
        private bool FindNodeID(HtmlElement hRow, out int nodeID)
        {
            nodeID = -1;

             // well-formed HTML, the node ID is in the id of the row node
            if (hRow.Id != null && hRow.Id.Length != 0)
            {
                try
                {
                    nodeID = int.Parse(hRow.Id);
                    return true;
                }
                catch
                {
                    return false; // bad formed node id attribute
                }
            }

            // stripped ID (i.e.: HTML saved by Word), we try to decode from content
            // id was exported in the text of the first column
            foreach (HtmlElement hId in hRow.Children)
            {
                if (!hId.TagName.Equals("td", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                try
                {
                    nodeID = int.Parse(hId.InnerText);
                    return true;
                }
                catch
                {
                }
                break;
            }

            // stripped ID, we will try to manage anyway later
            return true;
        }

        //---------------------------------------------------------------------
        private int GetNodeID(XmlElement node)
        {
            try { return int.Parse(node.GetAttribute(AllStrings.id)); }
            catch { return 0;  }
        }

        //---------------------------------------------------------------------
        private string Strip(string s)
        {
            return s.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        }

        //---------------------------------------------------------------------
        private bool FindNodeByBaseAndID(XmlNodeList nodes, string inputBase, int inputID, out XmlElement nodeToTranslate)
        {
            nodeToTranslate = null;

            if (nodes == null)
                return false;

            // first, try by exact match, including the imported node ID if available
            foreach (XmlElement node in nodes)
            {
                if  (
                        node.GetAttribute(AllStrings.baseTag) == inputBase && 
                        (
                            inputID == -1 ||
                            inputID == GetNodeID(node)
                        )
                    )
                {
                    nodeToTranslate = node;
                    return true;
                }
            }

            // then, try to match the base string, ignoring spaces, capitals, linefeed and also the id
            foreach (XmlElement node in nodes)
            {
                if (Strip(node.GetAttribute(AllStrings.baseTag)).Equals(Strip(inputBase), StringComparison.CurrentCultureIgnoreCase))
                {
                    nodeToTranslate = node;
                    return true;
                }
            }

            // node to translate not found
            return false;
        }

        //---------------------------------------------------------------------
        private bool FindNodeByID(XmlNodeList nodes, int inputID, out XmlElement nodeToTranslate)
        {
            nodeToTranslate = null;

            if (nodes == null)
                return false;

            if (inputID == -1)
                return false;

            foreach (XmlElement node in nodes)
            {
                if (GetNodeID(node) == inputID)
                {
                    nodeToTranslate = node;
                    return true;
                }
            }

            return false;
        }

        //---------------------------------------------------------------------
        private void SetDictionaryNodeFromTable(HtmlElement hTable, ref DictionaryTreeNode dictNode, HTMLImportFlags importFlags)
        {
            string nodePath = string.Empty;

            // well-formed HTML, the node path is in the id of the table node
            if (hTable.Id == null || hTable.Id.Length == 0)
                return;

            SetDictionaryNode(hTable.Id, ref dictNode, importFlags);
        }

        //---------------------------------------------------------------------
        private void SetDictionaryNodeFromTR(HtmlElement hRow, ref DictionaryTreeNode dictNode, HTMLImportFlags importFlags)
        {
            if (hRow.InnerText == null || hRow.InnerText == string.Empty)
                return;

            if (!hRow.Children[0].TagName.Equals("td", StringComparison.CurrentCultureIgnoreCase))
                return;

            SetDictionaryNode(hRow.InnerText.TrimEnd(), ref dictNode, importFlags);
        }

        //---------------------------------------------------------------------
        private void SetDictionaryNode(string nodePath, ref DictionaryTreeNode dictNode, HTMLImportFlags importFlags)
        {
            if (nodePath == string.Empty)
                return;

            if (importFlags.VerboseOutput)
                logWriter.WriteLog(string.Format(Strings.ProcessingNode, nodePath), TypeOfMessage.info);
            else
                logWriter.StatusBarLog(string.Format(Strings.ProcessingNode, nodePath));

            LocalizerTreeNode node;
            if (!dictionaryCreator.GetNodeFromPath(nodePath, out node))
            {
                LogNodePath(nodePath, importFlags);
                logWriter.WriteLog(string.Format(Strings.NodeNotFound, nodePath), TypeOfMessage.error);
                return;
            }

            dictNode = node as DictionaryTreeNode;
            if (dictNode == null)
            {
                LogNodePath(nodePath, importFlags);
                logWriter.WriteLog(string.Format(Strings.InvalidNode, nodePath), TypeOfMessage.error);
                return;
            }
        }

        //---------------------------------------------------------------------
        private bool TranslateNode(int nodeID, string strBase, string strTarget, ref DictionaryTreeNode dictNode, HTMLImportFlags importFlags)
        {
            bool bDifferentBaseString = false;
            XmlElement nodeToTranslate;
            XmlNodeList targetNodes = dictNode.GetStringNodes();

            // try finding the node to translate by matching the base string AND the id, 
            // or the base only if the id is not available
            if (!FindNodeByBaseAndID(targetNodes, strBase, nodeID, out nodeToTranslate))
            {
                // try matching by id only, but in this case the base string is changed 
                if (!FindNodeByID(targetNodes, nodeID, out nodeToTranslate))
                {
                    LogNodePath(dictNode.FullPath, importFlags);
                    logWriter.WriteLog(string.Format(Strings.StringNodeNotFound, nodeID, strBase), TypeOfMessage.error);
                    return false;
                }
                else
                    bDifferentBaseString = true;
            }

            // string already translated, keep the existing unless overwrite was forced
            if (nodeToTranslate.GetAttribute(AllStrings.target) != string.Empty && !importFlags.OverwriteExisting)
                return false;

            // matched by id only 
            if (bDifferentBaseString)
            {
                LogNodePath(dictNode.FullPath, importFlags);
                logWriter.WriteLog(string.Format(Strings.BaseStringsDoNotMatch, nodeID, nodeToTranslate.GetAttribute(AllStrings.baseTag), strBase), TypeOfMessage.warning);

                // skip if requested
                if (importFlags.skipIfBaseDoNotMatch)
                {
                    logWriter.WriteLog(string.Format(Strings.TranslationSkipped), TypeOfMessage.warning);
                    return false;
                }

                // else set as temporary and warn the user
                logWriter.WriteLog(string.Format(Strings.TranslationApplied, strTarget), TypeOfMessage.warning);
                nodeToTranslate.SetAttribute(AllStrings.temporary, AllStrings.trueTag);
            }

            nodeToTranslate.SetAttribute(AllStrings.target, strTarget);

            return true;
        }

        private string currNodePath = string.Empty;

        //---------------------------------------------------------------------
        private void LogNodePath(string nodePath, HTMLImportFlags importFlags)
        {
            if (importFlags.verboseOutput)
                return;

            if (currNodePath.Equals(nodePath, StringComparison.CurrentCultureIgnoreCase))
                return;
            currNodePath = nodePath;
            logWriter.WriteLog("\r\n");
            logWriter.WriteLog(string.Format(Strings.ProcessingNode, nodePath), TypeOfMessage.info);
        }

        //---------------------------------------------------------------------
        public void ParseHTMLElement(HtmlElement hElem, ref Dictionary<string, LocalizerDocument> modifiedNodes, HTMLImportFlags importFlags, ref DictionaryTreeNode dictNode)
        {
            foreach (HtmlElement hE in hElem.Children)
            {
                if (!hE.TagName.Equals("tr", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (hE.TagName.Equals("table", StringComparison.CurrentCultureIgnoreCase))
                    {
                        dictNode = null; // new table, the dictionary node is changing
                        SetDictionaryNodeFromTable(hE, ref dictNode, importFlags);
                    }
                    ParseHTMLElement(hE, ref modifiedNodes, importFlags, ref dictNode);
                }
                else
                {
                    if (hE.Children.Count == 1)
                    {
                        // if still node path not found, try to extract it from the first table row
                        if (dictNode == null)
                            SetDictionaryNodeFromTR(hE, ref dictNode, importFlags);
                    }
                    else
                    {
                        // at this point, the dictionary node should be already found
                        if (dictNode == null)
                        {
                            logWriter.WriteLog(string.Format("Current node is invalid, skipped row: {0}", hE.InnerText), TypeOfMessage.error);
                            continue;
                        }

                        int nodeID;
                        if (!FindNodeID(hE, out nodeID))
                        {
                            LogNodePath(dictNode.FullPath, importFlags);
                            logWriter.WriteLog(string.Format(Strings.InvalidEntry, hE.InnerText), TypeOfMessage.error);
                            continue;
                        }

                        string strBase;
                        string strTarget;

                        if (!DecodeHTMLRow(hE, out strBase, out strTarget))
                        {
                            LogNodePath(dictNode.FullPath, importFlags);
                            logWriter.WriteLog(string.Format(Strings.InvalidEntry, hE.InnerText), TypeOfMessage.error);
                            continue;
                        }
                        else
                        {
                            if (strTarget.Length == 0)
                                continue; // no error or warning, just translation not provided

                            if (TranslateNode(nodeID, strBase, strTarget, ref dictNode, importFlags))
                            {
                                if (!modifiedNodes.ContainsKey(dictNode.FileSystemPath))
                                    modifiedNodes.Add(dictNode.FileSystemPath, dictNode.Document);
                            }
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        public bool DecodeHTMLRow(HtmlElement hRow, out string strBase, out string strTarget)
        {
            strBase = string.Empty;
            strTarget = string.Empty;

            int count = 0;
            foreach (HtmlElement hC in hRow.Children)
            {
                if (!hC.TagName.Equals("td", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                count++;

                if (hC.InnerText == null)
                    continue;

                if (
                        (
                            hC.Id != null &&
                            hC.Id == AllStrings.baseTag                           
                        ) ||
                        (
                            hC.Id == null &&
                            count == 2
                        )
                    )
                    strBase = HtmlWriter.Decode(hC.InnerText);

                if (
                        hC.Id != null &&
                        hC.Id == AllStrings.id &&
                        hC.GetAttribute(AllStrings.baseTag) != string.Empty
                    )
                    strBase = HtmlWriter.Decode(hC.GetAttribute(AllStrings.baseTag));

                if (
                        (
                            hC.Id != null &&
                            hC.Id == AllStrings.target
                        ) ||
                        (
                            hC.Id == null &&
                            count == 3
                        )
                    )
                    strTarget = HtmlWriter.Decode(hC.InnerText);
            }

            return strBase.Length > 0;
        }

        //---------------------------------------------------------------------
        private string CSVEncode(string strText)
        {
            string strEncoded = strText;

            if (strEncoded.Contains(';'))
            {
                strEncoded = strEncoded.Replace("\"", "\"\"");
                strEncoded = "\"" + strEncoded + "\"";
            }

            if (strEncoded.Contains('\n'))
                strEncoded = strEncoded.Replace('\n',' ');

            if (strEncoded.Contains('\r'))
                strEncoded = strEncoded.Replace('\r',' ');

            return strEncoded;
        }

		//---------------------------------------------------------------------
        public void ExportToCSV(string culture, bool translated, bool nonTranslated)
        {
            string file;
            LocalizerTreeNode selectedNode = dictionaryCreator.SelectedNode;
            if (
                    SelectFilters(selectedNode) &&
                    OkTranslationTable(selectedNode, AllStrings.csvExtension, AllStrings.FILTERCSV, culture, out file)
               )
            {
                StringBuilder csv = new StringBuilder(String.Empty);
			    try
			    {
				    dictionaryCreator.Cursor = Cursors.WaitCursor;

				    foreach (DictionaryTreeNode node in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, culture))
				    {
                        if (NodeToSkip(node))
                            continue;
                        
                        if (SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable)
					    {
						    if (!node.MergeWithSupport(SolutionDocument.LocalInfo.SupportLanguage))
							    continue;
					    }

					    XmlNodeList list = node.GetStringNodes();
					    if (list == null) continue;
					    ArrayHashTable tableList = CommonFunctions.FilterPerItem(list);
					    foreach (DictionaryEntry de in tableList)
                        {
						    ArrayList elements = de.Value as ArrayList;
						    foreach (XmlElement n in elements)
						    {
							    bool valid = true;
                                if (n.HasAttribute(AllStrings.valid))
                                {
                                    try { valid = bool.Parse(n.GetAttribute(AllStrings.valid)); }
                                    catch { }
                                }

							    bool temporary = false;
                                if (n.HasAttribute(AllStrings.temporary))
                                {
                                    try { temporary = bool.Parse(n.GetAttribute(AllStrings.temporary)); }
                                    catch { }
                                }

                                string target = String.Empty;
							    target = n.GetAttribute(AllStrings.target);

							    //se viene richiesta la stampa solo di quelli non tradotti, devo verificare che target sia diverso da stringa vuota
							    // e che se c'è temporary, ci sia il fla di contarlo come tradotto o meno. 
							    bool isTranslated = target != String.Empty && (!temporary || dictionaryCreator.CountTemporaryAsTranslated);

							    if (isTranslated && !translated)
								    continue;

							    if (!isTranslated && !nonTranslated)
								    continue;

                                csv.Append(CSVEncode(n.GetAttribute(baseOrSupport)));
                                csv.Append(";");
                                csv.Append(CSVEncode(target));
                                csv.Append("\r\n");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dictionaryCreator, ex.Message);
                }
                finally
                {
                    dictionaryCreator.Cursor = Cursors.Default;
                }


                SaveTranslationTable(file, csv.ToString());
                ShowFileListOptions(file, selectedNode.Name, FileListOptions.ModeType.TranslationTable);
            }
        }

		//---------------------------------------------------------------------
		public static void SaveTranslationTable(string file, string html)
		{
			using (StreamWriter w = new StreamWriter(file, false, System.Text.Encoding.UTF8))
				w.Write(html);
		}
    }
}
