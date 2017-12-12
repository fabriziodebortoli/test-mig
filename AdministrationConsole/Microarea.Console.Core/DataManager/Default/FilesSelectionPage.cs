using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.Console.Core.DataManager.Common;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Core.StringLoader;

namespace Microarea.Console.Core.DataManager.Default
{
	//=========================================================================
	public partial class FilesSelectionPage : InteriorWizardPage
	{
		private ArrayList moduleFileList = new ArrayList();
		private ArrayList appendFileList = new ArrayList();

		private DefaultSelections	defaultSel = null;
		private Images				myImages = null;		

		//---------------------------------------------------------------------
		public FilesSelectionPage()
		{
			InitializeComponent();
			InitializeImageList();
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();
			
			LoadAvailableFiles();

			if (FilesListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			return "BaseColumnsPage"; 
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			return "ChooseOperationPage";
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerDefault + "FilesSelectionPage");
			return true;
		}
		#endregion

		# region Inizializzazione ImageList
		//---------------------------------------------------------------------
		private void InitializeImageList()
		{
			myImages = new Images();

			// inizializzo imagelist del tree
			SourceFilesTreeView.ImageList = myImages.ImageList;

			// inizializzo imagelist della listview
			FilesListView.LargeImageList = myImages.ImageList;
			FilesListView.SmallImageList = myImages.ImageList;

			FilesListView.Columns.Add(string.Empty, 250, HorizontalAlignment.Left);

			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];
		}
		# endregion

		# region Caricamento delle informazioni suddivise per applicazione+modulo+ tabella/files nel TreeView
		/// <summary>
		/// per caricare i nomi dei folder e dei files (letti dal file system) nel tree
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAvailableFiles()
		{
			// se non ci sono elementi nel tree e volevo selezionare i singoli file
			// oppure ho forzato la clear degli items...
			if (SourceFilesTreeView.Nodes.Count == 0 || defaultSel.ImportSel.ClearItems)
			{
				FilesListView.Items.Clear();

				SourceFilesTreeView.BeginUpdate();
				SourceFilesTreeView.Nodes.Clear();

				PlugInTreeNode appNode = null; 
				PlugInTreeNode modNode = null; 
				PlugInTreeNode tableNode = null; 

				foreach (AddOnApplicationDBInfo appDBInfo in defaultSel.AppDBStructInfo.ApplicationDBInfoList)
				{
					appNode = new PlugInTreeNode(appDBInfo.BrandTitle);
					appNode.Tag					= appDBInfo.ApplicationName;
					appNode.ImageIndex			= Images.GetApplicationBitmapIndex();
					appNode.SelectedImageIndex	= Images.GetApplicationBitmapIndex();
					appNode.Type				= DataManagerConsts.ApplicationNode;
					
					foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
					{
						modNode = new PlugInTreeNode(modInfo.Title);
						modNode.Tag					= modInfo.ModuleName;
						modNode.ImageIndex			= Images.GetModuleBitmapIndex();
						modNode.SelectedImageIndex	= Images.GetModuleBitmapIndex();
						modNode.Type				= DataManagerConsts.ModuleNode; 
						GetModuleFiles(appDBInfo.ApplicationName, modInfo.ModuleName);

						// nel caso in cui il modulo non apportasse propri oggetti di database, non e' detto
						// che non apporti dati di Append, quindi li salvo nell'array dei file di Append
						if (modInfo.TablesList.Count == 0)
						{
							foreach (FileInfo file in moduleFileList)
							{
								// se il file finisce con Append e non e' ancora presente nell'array lo aggiungo
								if (
									Path.GetFileNameWithoutExtension(file.Name).EndsWith(DataManagerConsts.Append, StringComparison.InvariantCultureIgnoreCase) && 
									!appendFileList.Contains(file)
									)
									appendFileList.Add(file);
							}
						}

						foreach (EntryDBInfo tabEntry in modInfo.TablesList)
						{
							tableNode = new PlugInTreeNode(tabEntry.Name);
							
							if (tabEntry.Exist)
							{
								tableNode.ImageIndex			= Images.GetTableBitmapIndex();
								tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
								tableNode.Type					= DataManagerConsts.TableNode; 
								AddFilesToTableNode(ref tableNode);							
							}
							else
							{
								tableNode.ImageIndex			= Images.GetTableBitmapIndex();
								tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
								tableNode.ForeColor				= Color.Red;
								tableNode.Type					= DataManagerConsts.NoExistTableNode;
							}			
											
							if (tableNode.NodesCount > 0)
                                modNode.Nodes.Add(tableNode);
						}

						if (modNode.NodesCount > 0)
                            appNode.Nodes.Add(modNode);
					}

					if (appNode.NodesCount > 0)
						SourceFilesTreeView.Nodes.Add(appNode);
				}

				// gestione per i file in Append
				ManageAppendFiles();

				SourceFilesTreeView.EndUpdate();
				defaultSel.ImportSel.ClearItems = false;
			}
		}		

		# region Gestione file di Append
		/// <summary>
		/// Gestione per i file in Append
		/// 1) per i moduli che aggiungono delle info in append ad altri moduli aggiungo nei moduli di appartenenza
		/// ad es: se il modulo AdditionalCharges ha un file MA_InventoryReasonsAppend.xml, questo file comparirà sotto
		/// il nodo Inventory -> tabella MA_InventoryReasons...
		/// 2) dopo aver gestito i file di Append verifico se ne sono rimasti da processare di tipo "orfano", ovvero
		/// che si agganciano a tabelle che non possiedono dei dati di default standard (nometabella.xml).
		/// Per visualizzarli devo aggiungere nel TreeView gli eventuali nodi relativi ad applicazione+modulo+tabella.
		/// </summary>
		//---------------------------------------------------------------------
		private void ManageAppendFiles()
		{
			if (appendFileList.Count > 0)
			{
				string tableName = string.Empty;

				// faccio il ciclo al contrario, così alla fine rimuovo l'elemento processato
				for (int i = appendFileList.Count - 1; i >= 0; i--)
				{
					FileInfo file = (FileInfo)appendFileList[i];
					// estrapolo il nome della tabella togliendo Append
					tableName = file.Name.Substring(0, file.Name.IndexOf(DataManagerConsts.Append, StringComparison.InvariantCultureIgnoreCase));

					foreach (PlugInTreeNode app in SourceFilesTreeView.Nodes) // loop sulle applicazioni
					{
						foreach (PlugInTreeNode mod in app.Nodes) // loop sui moduli
						{
							foreach (PlugInTreeNode table in mod.Nodes) // loop sulle tabelle
							{
								if (string.Compare(table.Text, tableName, true, CultureInfo.InvariantCulture) == 0)
								{
									PlugInTreeNode fileNode = new PlugInTreeNode(file.Name);
									fileNode.ImageIndex			= Images.GetYellowXmlFileBitmapIndex();
									fileNode.SelectedImageIndex	= Images.GetYellowXmlFileBitmapIndex();
									fileNode.Tag				= file;
									fileNode.Type				= DataManagerConsts.AppendFileNode; 
									INameSpace ns = defaultSel.ContextInfo.PathFinder.GetAppModNSFromFilePath(file.FullName);
									if (ns != null)
									{
										IBaseApplicationInfo ai = defaultSel.ContextInfo.PathFinder.GetApplicationInfoByName(ns.Application);
										IBaseModuleInfo mi = defaultSel.ContextInfo.PathFinder.GetModuleInfoByName(ns.Application, ns.Module);
										fileNode.Id	= string.Format
											(
											DataManagerStrings.ToolTipForAppendFile,
											(ai != null) ? ai.Name : ns.Application, 
											(mi != null) ? mi.Title : ns.Module
											);
									}
                                    table.Nodes.Add(fileNode);
									// rimuovo l'elemento appena processato dall'array, così dopo elaboro
									// i file rimanenti (se ce ne sono) e che sarebbero "orfani" 
									appendFileList.RemoveAt(i); 
								}
							}
						}
					}
				}
			}

			// file di Append "orfani", ovvero la tabella a cui va in Append non ha dati di default standard
			if (appendFileList.Count > 0)
				AssignOrphanAppendFile(); 
		}

		/// <summary>
		/// Dall'array contenente i file di append rimasti dopo la prima elaborazione vado a gestire
		/// i cosiddetti file orfani, ovvero quelli che vanno in Append a tabelle che non hanno propri file con
		/// dati di default standard.
		/// </summary>
		//---------------------------------------------------------------------
		private void AssignOrphanAppendFile()
		{
			string tableName = string.Empty;

			foreach (FileInfo file in appendFileList)
			{
				// estrapolo il nome della tabella togliendo Append
				tableName = file.Name.Substring(0, file.Name.IndexOf(DataManagerConsts.Append));

				PlugInTreeNode appNode = new PlugInTreeNode();
				PlugInTreeNode modNode = new PlugInTreeNode();
				
				PlugInTreeNode tableNode, fileNode;

				foreach (AddOnApplicationDBInfo appDBInfo in defaultSel.AppDBStructInfo.ApplicationDBInfoList)
				{
					foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
					{
						foreach (EntryDBInfo tabEntry in modInfo.TablesList)
						{
							tableNode = new PlugInTreeNode();

							if (string.Compare(tableName, tabEntry.Name, true, CultureInfo.InvariantCulture) == 0 && 
								CheckTableFile(file, tableName))
							{
								// controllo se il nodo esiste già (applicazione+modulo+tabella)
								FindNodeInTreeView(tableName, modInfo.Title, appDBInfo.BrandTitle, 
									out tableNode, out modNode, out appNode);

								if (tableNode == null)
								{
									tableNode = new PlugInTreeNode(tabEntry.Name);
									tableNode.ImageIndex			= Images.GetTableBitmapIndex();
									tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
									tableNode.Type					= DataManagerConsts.TableNode; 
								}

								fileNode = new PlugInTreeNode(file.Name);
								fileNode.ImageIndex			= Images.GetYellowXmlFileBitmapIndex();
								fileNode.SelectedImageIndex	= Images.GetYellowXmlFileBitmapIndex();
								fileNode.Tag				= file;
								fileNode.Type				= DataManagerConsts.AppendFileNode; 
								
								INameSpace ns = defaultSel.ContextInfo.PathFinder.GetAppModNSFromFilePath(file.FullName);
								if (ns != null)
								{
									IBaseApplicationInfo ai = defaultSel.ContextInfo.PathFinder.GetApplicationInfoByName(ns.Application);
									IBaseModuleInfo mi = defaultSel.ContextInfo.PathFinder.GetModuleInfoByName(ns.Application, ns.Module);
									fileNode.Id	= string.Format
										(
										DataManagerStrings.ToolTipForAppendFile,
										(ai != null) ? ai.Name : ns.Application, 
										(mi != null) ? mi.Title : ns.Module
										);
								}

                                tableNode.Nodes.Add(fileNode);

								if (modNode == null)
								{
									modNode = new PlugInTreeNode();
									modNode.Text				= modInfo.Title;
									modNode.Tag					= modInfo.ModuleName;
									modNode.ImageIndex			= Images.GetModuleBitmapIndex();
									modNode.SelectedImageIndex	= Images.GetModuleBitmapIndex();
									modNode.Type				= DataManagerConsts.ModuleNode;
                                    modNode.Nodes.Add(tableNode);
								}
								else
								{
									bool found = false;
									foreach (PlugInTreeNode node in modNode.Nodes)
									{
										if (string.Compare(node.Text, tableNode.Text, true, CultureInfo.InvariantCulture) == 0)
											found = true;
									}
									// se il nodo non esiste allora lo aggiungo
									if (!found)
                                        modNode.Nodes.Add(tableNode);
								}

								if (appNode == null)
								{
									appNode = new PlugInTreeNode();
									appNode.Text				= appDBInfo.BrandTitle;
									appNode.Tag					= appDBInfo.ApplicationName;
									appNode.ImageIndex			= Images.GetApplicationBitmapIndex();
									appNode.SelectedImageIndex	= Images.GetApplicationBitmapIndex();
									appNode.Type				= DataManagerConsts.ApplicationNode;
                                    appNode.Nodes.Add(modNode);
									SourceFilesTreeView.Nodes.Add(appNode);
								}
								else 
								{
									bool found = false;
									foreach (PlugInTreeNode node in appNode.Nodes)
									{
										if (string.Compare(node.Text, modNode.Text, true, CultureInfo.InvariantCulture) == 0)
											found = true;
									}
									// se il nodo non esiste allora lo aggiungo
									if (!found)
                                        appNode.Nodes.Add(modNode);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// cerco nei nodi già presenti nel tree il nome tabella + modulo + applicazione, per evitare di inserirli
		/// una seconda volta. Ritorno il puntatore a tutti e 3 i nodi e poi decido se crearne di nuovi o agganciarmi
		/// a quelli esistenti.
		/// </summary>
		//---------------------------------------------------------------------
		private void FindNodeInTreeView
			(
			string tableName, 
			string moduleName, 
			string appName, 
			out PlugInTreeNode tableNode,
			out PlugInTreeNode moduleNode,
			out PlugInTreeNode appNode
			)
		{
			PlugInTreeNode table1Node = null;
			PlugInTreeNode module1Node = null;
			PlugInTreeNode app1Node = null;

			foreach (PlugInTreeNode app in SourceFilesTreeView.Nodes) // loop sui nodi di tipo applicazione
			{
				if (string.Compare(app.Text, appName, true, CultureInfo.InvariantCulture) != 0)
					continue;
				else
					app1Node = app;

				foreach (PlugInTreeNode mod in app.Nodes) // loop sui nodi di tipo modulo
				{
					if (string.Compare(mod.Text, moduleName, true, CultureInfo.InvariantCulture) != 0)
						continue;
					else 
						module1Node = mod;

					foreach (PlugInTreeNode table in mod.Nodes) // loop sui nodi di tipo tabella
					{
						// ho trovato il nodo di quella tabella!
						if (string.Compare(table.Text, tableName, true, CultureInfo.InvariantCulture) == 0)
							table1Node = table;
					}
				}	
			}

			tableNode = table1Node;
			moduleNode= module1Node;
			appNode	  = app1Node;
		}
		# endregion

		/// <summary>
		/// Il PathFinder mi ritorna un array di oggetti di tipo FileInfo... ovvero tutti i file xml di default
		/// suddivisi per applicazione+modulo
		/// </summary>
		//---------------------------------------------------------------------
		private void GetModuleFiles(string application, string module)
		{
			moduleFileList.Clear();
			// controllo se alla tabella sono associati dei dati di default opzionali
			// andando a leggere prima nella custom e poi nella standard
			DirectoryInfo customDir = new DirectoryInfo
			(
				Path.Combine
				(
				defaultSel.ContextInfo.PathFinder.GetCustomDataManagerDefaultPath(application, module, defaultSel.SelectedIsoState),
				defaultSel.SelectedConfiguration
				)
			);

			DirectoryInfo standardDir = new DirectoryInfo
			(
				Path.Combine
				(
                defaultSel.ContextInfo.PathFinder.GetStandardDataManagerDefaultPath(application, module, defaultSel.SelectedIsoState),
				defaultSel.SelectedConfiguration
				)
			);

			defaultSel.ContextInfo.PathFinder.GetXMLFilesInPath(standardDir, customDir, ref moduleFileList);						
		}

		/// <summary>
		/// per ogni tabella viene controllato se esistono dei file contenenti dati di default e opzionali
		/// </summary> 
		//---------------------------------------------------------------------
		private void AddFilesToTableNode(ref PlugInTreeNode tableNode)
		{
			PlugInTreeNode fileNode = null;	

			foreach (FileInfo file in moduleFileList)
			{
				// estraggo il nome del file togliendo l'estensione
				 string fileName = Path.GetFileNameWithoutExtension(file.Name);

				// controllo che il nome sia uguale al nome della tabella oppure uguale
				// al nome della tabella + il suffisso Append
				// al termine scorro il file xml e controllo il nome della tabella e se é un file opzionale o meno
				if 
					(
					(string.Compare(tableNode.Text, fileName, true, CultureInfo.InvariantCulture) == 0 ||
					string.Compare(tableNode.Text + DataManagerConsts.Append, fileName, true, CultureInfo.InvariantCulture) == 0) 
					&& CheckTableFile(file, tableNode.Text)
					)
				{
					fileNode = new PlugInTreeNode(file.Name);
					fileNode.ImageIndex			= Images.GetXmlFileBitmapIndex();
					fileNode.SelectedImageIndex	= Images.GetXmlFileBitmapIndex();
					fileNode.Tag				= file;
					fileNode.Type				= DataManagerConsts.FileNode;
                    tableNode.Nodes.Add(fileNode);
				}
				else
				{
					// se entro in questo else sta a significare che potrei aver incontrato un file Append che 
					// aggiunge dati in coda ad un altro modulo (infatti non viene riconosciuto il nome della tabella)
					// memorizzo allora il nome del file in un array separato, 
					// in modo tale da caricarli nei moduli giusti successivamente
					if (fileName.EndsWith(DataManagerConsts.Append, StringComparison.InvariantCultureIgnoreCase) && !appendFileList.Contains(file))
					{
						// array generico con file di Append e le loro FileInfo (utilizzato per la visualizzazione dei
						// file sotto applicazione/modulo a cui vanno in Append
						appendFileList.Add(file);
					}
				}
			}			
		}

		/// <summary>
		/// carica i nomi dei file (solo quelli con estensione xml) contenuti nei folder
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckTableFile(FileInfo file, string tableName)
		{
			//istanzio un XmlTextReader x leggere le info contenuti nel file xml
			XmlTextReader reader = new XmlTextReader(file.FullName); 
			reader.WhitespaceHandling = WhitespaceHandling.None;
			
			bool isOptional = false;
			bool hasReference = false;

			try
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						if (reader.Name.CompareTo(DataManagerConsts.DataTables) == 0)
						{
							// controllo se é un file opzionale o meno
/*							if (
								(!reader.MoveToAttribute(DataManagerConsts.Optional) ||
								reader.Value == bool.FalseString)
								)
							{
								if (defaultSel.ImportSel.NoOptional)
									continue;
								else
									return false;
							}
							else
							{
								if (!defaultSel.ImportSel.NoOptional)
									continue;
								else
									return false;
							}
*/
							// mi segno se e' un file optional
							isOptional = (reader.MoveToAttribute(DataManagerConsts.Optional) &&
								string.Compare(reader.Value, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);

							// mi segno se e' un file con reference
							hasReference = (reader.MoveToAttribute(DataManagerConsts.HasReference) &&
								string.Compare(reader.Value, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);

							// se nel file c'e' l'attributo optional=true
							if (isOptional)
							{
								// l'utente ha scelto i file opzionali, continuo l'elaborazione
								if (!defaultSel.ImportSel.NoOptional)
								{
									// se e' indicato un reference va bene cosi
									if (hasReference)
										return true;
									else // altrimenti continuo
										continue;
								}
								else // l'utente NON ha scelto i file opzionali e non continuo
									return false;
							}
							else
							{
								if (defaultSel.ImportSel.NoOptional)
								{
									// se ha un riferimento indicato ritorno subito, perche' non ci sono righe dopo
									if (hasReference)
										return true;
									else // altrimenti continuo con l'elaborazione
										continue;
								}
								else
									return false;
							}
						}

						if (reader.Name.CompareTo(DataManagerConsts.Schema) == 0)
							reader.Skip(); //skip the schema

						return !reader.EOF && (reader.Name.CompareTo(tableName) == 0);
					}
				}
			}
			catch (XmlException e)
			{
				Debug.Fail(string.Format("Exception: {0}", e.Message));
				Debug.WriteLine(string.Format("Exception: {0}", e.Message));
				return false;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}

			return false;			
		}
		# endregion

		# region Funzioni per spostare oggetti di tipo Tabella dal TreeView alla ListView e viceversa
		/// <summary>
		/// da un nodo di tipo Table vado ad inserire tutti suoi file nella list view 
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertTableInfo(PlugInTreeNode selectedNode)
		{
			foreach (PlugInTreeNode tableNode in selectedNode.Nodes)
				InsertFileInListView(tableNode);

			selectedNode.Nodes.Clear();
		}
		
		/// <summary>
		/// inserisce l'elemento file nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertFileInListView(PlugInTreeNode fileNode)
		{			
			FilesListView.BeginUpdate();
			
			defaultSel.ImportSel.AddItemInImportList(((FileInfo)fileNode.Tag).DirectoryName, fileNode.Text);
			
			ListViewItem item; 
			item = new ListViewItem();
			item.ImageIndex = 
				(fileNode.Type == DataManagerConsts.FileNode) 
				? Images.GetXmlFileBitmapIndex()
				: Images.GetYellowXmlFileBitmapIndex();

			item.Text = fileNode.Text;
			// memorizzo le info relative al parent nel nodo e al fileinfo in modo che in caso di
			// remove dalla list view riesco a rintracciare il parent nel nodo e ad inserire le
			// informazioni corrette relativamente al file
			// per i file di append mi porto dietro anche il nome dell'applicazione+modulo che lo ha apportato
			item.Tag  = new FileListViewInfo
				(
				fileNode.Parent, 
				((FileInfo)fileNode.Tag),
				(fileNode.Type == DataManagerConsts.AppendFileNode) ? fileNode.Id : string.Empty 
				);
			FilesListView.Items.Add(item);

			FilesListView.EndUpdate();
		}

		/// <summary>
		/// rimuove il singolo elemento tabella dalla listview e lo inserisce nell'albero
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveSingleItem(ListViewItem item)
		{
			// costruisco il nodo da aggiungere all'albero alla tabella di appartenenza del file
			PlugInTreeNode fileNode = new PlugInTreeNode(item.Text);
			fileNode.ImageIndex			= item.ImageIndex;
			fileNode.SelectedImageIndex	= item.ImageIndex;
			fileNode.Tag				= ((FileListViewInfo)item.Tag).FileInfo;

			fileNode.Type = 
				(fileNode.ImageIndex == Images.GetXmlFileBitmapIndex()) 
				? DataManagerConsts.FileNode
				: DataManagerConsts.AppendFileNode;

			// se è un file di append porto sul nodo le info dell'applicazione+modulo che lo ha apportato
			if (fileNode.Type == DataManagerConsts.AppendFileNode)
				fileNode.Id = ((FileListViewInfo)item.Tag).AppendedBy;

            ((PlugInTreeNode)((FileListViewInfo)item.Tag).ParentNode).Nodes.Add(fileNode);
					
			defaultSel.ImportSel.RemoveItemFromImportList(((FileInfo)fileNode.Tag).DirectoryName, fileNode.Text);
			item.Remove();

			if (FilesListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}
		# endregion

		# region Eventi sui bottoni >, >>, <<
		/// <summary>
		/// evento sul pulsante > (aggiungo l'entry selezionato nel tree alla list view)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddButton_Click(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)SourceFilesTreeView.SelectedNode;

			if (selectedNode != null && selectedNode.Index != -1)
			{
				switch (selectedNode.Type)
				{
					case DataManagerConsts.ApplicationNode:
						foreach (PlugInTreeNode moduleNode in selectedNode.Nodes)
							foreach (PlugInTreeNode tableNode in moduleNode.Nodes)
								InsertTableInfo(tableNode); //sposto tutti i file della tabella
						break;

					case DataManagerConsts.ModuleNode:
						foreach (PlugInTreeNode tableNode in selectedNode.Nodes)
							InsertTableInfo(tableNode); //sposto tutti i file della tabella
						break;

					case DataManagerConsts.TableNode:
						InsertTableInfo(selectedNode); //sposto tutti i file della tabella
						break;

					case DataManagerConsts.FileNode:
					case DataManagerConsts.AppendFileNode:
						InsertFileInListView(selectedNode);// sposto solo il singolo file
						selectedNode.Remove(); 
						break;				
				}
			}

			if (FilesListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		/// <summary>
		/// evento sul pulsante Minore rimuovo le tabelle selezionati sulla list view
		/// e le sposto nell'albero associandoli al modulo di appartenenza
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in FilesListView.SelectedItems)
				RemoveSingleItem(item);
		}

		/// <summary>
		/// evento sul pulsante minore minore (rimuovo tutti gli item presenti nella list view)
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in FilesListView.Items)
				RemoveSingleItem(item);
		}
		# endregion

		# region Eventi sul TreeView
		/// <summary>
		/// evento sul doppio click su un nodo del treeview
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceFilesTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)SourceFilesTreeView.SelectedNode;

			if (selectedNode != null && selectedNode.Index != -1)
			{
				switch (selectedNode.Type)
				{
					case DataManagerConsts.FileNode:
					case DataManagerConsts.AppendFileNode:
						InsertFileInListView(selectedNode);// sposto solo il singolo file
						selectedNode.Remove(); 
						break;				

					case DataManagerConsts.ApplicationNode:
					case DataManagerConsts.ModuleNode:
					case DataManagerConsts.TableNode:
						break;
				}
			}

			if (FilesListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		/// <summary>
		/// evento sul "move" del mouse sul treeview faccio comparire un tooltip 
		/// con la traduzione in lingua (se esiste) del nome della tabella)
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceFilesTreeView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			PlugInTreeNode item = SourceFilesTreeView.GetNodeAt(e.X, e.Y) as PlugInTreeNode;

			if (item == null)
				return; 

			if (item.Type == DataManagerConsts.FileNode)
			{
				string tableLocalized = DatabaseLocalizer.GetLocalizedDescription(item.Text, item.Text);

				// se il testo del nodo è diverso dal testo localizzato e tradotto allora
				// faccio apparire il tooltip
				if (string.Compare(item.Text, tableLocalized, true, CultureInfo.InvariantCulture) != 0)
				{
					if (FileToolTip.GetToolTip(SourceFilesTreeView) != tableLocalized)
						FileToolTip.SetToolTip(SourceFilesTreeView, tableLocalized);
				}
			}

			if (item.Type == DataManagerConsts.AppendFileNode)
			{
				if (FileToolTip.GetToolTip(SourceFilesTreeView) != item.Id)
					FileToolTip.SetToolTip(SourceFilesTreeView, item.Id);
			}
		}			
		# endregion

		# region Eventi sulla ListView
		/// <summary>
		/// evento sul "move" del mouse sulla listview faccio comparire un tooltip
		/// </summary>
		//---------------------------------------------------------------------
		private void FilesListView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem item = FilesListView.GetItemAt(e.X, e.Y);

			if (item != null)
			{
				if (item.ImageIndex == Images.GetXmlFileBitmapIndex())
				{
					if (FileToolTip.GetToolTip(FilesListView) != item.Text)
						FileToolTip.SetToolTip(FilesListView, item.Text);
				}

				if (item.ImageIndex == Images.GetYellowXmlFileBitmapIndex())
				{
					if (FileToolTip.GetToolTip(FilesListView) != ((FileListViewInfo)item.Tag).AppendedBy)
						FileToolTip.SetToolTip(FilesListView, ((FileListViewInfo)item.Tag).AppendedBy);
				}
			}
			else
				FileToolTip.RemoveAll();
		}
		# endregion
	}

	# region Classe di appoggio per memorizzare negli oggetti di form più informazioni
	//=========================================================================
	public class FileListViewInfo 
	{
		public	PlugInTreeNode	ParentNode;
		public	FileInfo		FileInfo;
		public	string			AppendedBy;

		//---------------------------------------------------------------------
		public FileListViewInfo(PlugInTreeNode parent, FileInfo file, string appendInfo)
		{
			ParentNode	= parent;
			FileInfo	= file;
			AppendedBy	= appendInfo;
		}
	}
	# endregion
}
