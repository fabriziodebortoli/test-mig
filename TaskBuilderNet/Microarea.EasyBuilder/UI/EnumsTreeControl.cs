using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder.UI
{
	//=========================================================================
	/// <remarks/>
	internal partial class EnumsTreeControl : UserControl, IDirtyManager
	{
		private const ushort maxTagValue = 32767;//Limitato a Int16.MaxValue per via di una _tstol in DataEnum::AssignFromXMLString che limita il valore.
		private const ushort minTagValue = 15000;
		private const ushort maxItemValue = ushort.MaxValue;
		private const ushort minItemValue = 300;

		private static Hashtable enumTagValuesTable = new Hashtable();
		private static readonly object placeHolder = new object();

		EnumTags enumTagsForCurrentEasyBuilderApp = new EnumTags();
		private EnumTag oldEnumTag;
		private EnumItem oldEnumItem;
		
		private bool isDirty;
		private bool suspendDirtyChanges;

		ISelectionService selectionService;
		TreeNode rootNode;

		/// <remarks/>
		public event EventHandler<EventArgs> OpenProperties;
		/// <summary>
		/// Raised when the dirty flag is changed
		/// </summary>
		public event EventHandler<DirtyChangedEventArgs> DirtyChanged;

		Editor editor;

		//---------------------------------------------------------------------
		private void OnOpenProperties(object sender, EventArgs e)
		{
			if (OpenProperties != null)
				OpenProperties(this, e);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
        public EnumsTreeControl(
			ISelectionService selectionService,
			Editor editor
			)
		{
			this.selectionService = selectionService;
			InitializeComponent();

			this.Text = Resources.EnumsFormTitle;

			EnumsTreeView.ImageList = ImageLists.EnumsTreeControl;

			EnumsTreeView.TreeViewNodeSorter = new TreeNodeComparer();

			rootNode = new TreeNode(
				EnumsDeclarator.EnumsClassName,
				(int)ImageLists.EnumsTreeImageIndex.Root,
				(int)ImageLists.EnumsTreeImageIndex.Root
				);
			EnumsTreeView.Nodes.Add(rootNode);

			this.editor = editor;
			editor.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(FormEditor_DirtyChanged);
		}

		//---------------------------------------------------------------------
		void FormEditor_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			//Se mi arriva un e.Dirty = false dal form editor significa che
			//lui ha salvato, quindi salvo anche io.
			if (!e.Dirty)
				SaveEnums();
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			LoadTree();
		}

		//---------------------------------------------------------------------
		private void LoadTree()
		{
			Enums enums = new Enums();
			enums.LoadXml(false);

			if (enums.Tags == null || enums.Tags.Count == 0)
				return;

			IBaseModuleInfo ownerModuleInfo = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo;
			string ownerAppInfoName = ownerModuleInfo.ParentApplicationName;

			string enumModuleName = null;
			string enumApplicationName = null;
			bool readOnly = false;

			TreeNode currentEnumNode = null;
			TreeNode currentEnumItemNode = null;
			foreach (EnumTag enumTag in enums.Tags)
			{
				if (enumTag.Value > minTagValue)
					enumTagValuesTable[enumTag.Value] = placeHolder;

				currentEnumNode = new TreeNode(
					EnumsDeclarator.Purge(enumTag.Name),
					(int)ImageLists.EnumsTreeImageIndex.Enum,
					(int)ImageLists.EnumsTreeImageIndex.Enum
					);
				currentEnumNode.Tag = enumTag;

				enumTag.Site = new TBSite(enumTag, null, null, enumTag.Name);

				enumModuleName = enumTag.OwnerModule.Name;
				enumApplicationName = enumTag.OwnerModule.ParentApplicationName;

				//Le properties sono readonly see la current easybuilder app non è quella che apporta l'enumerativo.
				readOnly =
					String.Compare(enumModuleName, ownerModuleInfo.Name, StringComparison.InvariantCultureIgnoreCase) != 0 &&
					String.Compare(enumApplicationName, ownerAppInfoName, StringComparison.InvariantCultureIgnoreCase) != 0;

				enumTag.SetAllPropertiesReadOnly(readOnly);

				rootNode.Nodes.Add(currentEnumNode);

				foreach (EnumItem enumItem in enumTag.EnumItems)
				{
					currentEnumItemNode = new TreeNode(
						EnumsDeclarator.Purge(enumItem.Name),
						(int)ImageLists.EnumsTreeImageIndex.Enum,
						(int)ImageLists.EnumsTreeImageIndex.Enum
						);
					currentEnumNode.Nodes.Add(currentEnumItemNode);

					currentEnumItemNode.Tag = enumItem;

					enumItem.Site = new TBSite(enumItem, enumTag, null, enumItem.Name);

					enumModuleName = enumItem.OwnerModule.Name;
					enumApplicationName = enumItem.OwnerModule.ParentApplicationName;

					//Le properties sono readonly see la current easybuilder app non è quella che apporta l'enumerativo.
					readOnly =
						String.Compare(enumModuleName, ownerModuleInfo.Name, StringComparison.InvariantCultureIgnoreCase) != 0 &&
						String.Compare(enumApplicationName, ownerAppInfoName, StringComparison.InvariantCultureIgnoreCase) != 0;

					enumItem.SetAllPropertiesReadOnly(readOnly);

					//Se l'enumerativo appartiene alla customizzazione corrente, carico tutto nella variabile
					//enumTagsForCurrentEasyBuilderApp che tiene l'equivalente del file Enums.xml per la coppia
					//<applicazione, modulo> corrente (cioè la CurrentEasybuilderApp)
					if (
						String.Compare(enumModuleName, ownerModuleInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
						String.Compare(enumApplicationName, ownerAppInfoName, StringComparison.InvariantCultureIgnoreCase) == 0
						)
					{
						EnumTag workingTag = enumTagsForCurrentEasyBuilderApp.AddTag(
							enumItem.Owner.OwnerModule,
							enumItem.Owner.Name,
							enumItem.Owner.Value,
							enumItem.Owner.DefaultValue
							);
						if (workingTag == null)//se è null significa che è già presente nella collezione => lo recupero
							workingTag = enumTagsForCurrentEasyBuilderApp.GetTag(enumItem.Owner.Name);
						else
						{
							//Altrimenti significa che è stato aggiunto adesso => sottoscrivo i suoi eventi.
							workingTag.PropertyChanged += new PropertyChangedEventHandler(EnumTag_PropertyChanged);
							workingTag.PropertyChanging += new PropertyChangingEventHandler(EnumTag_PropertyChanging);
						}

						//Sostituisco l'enumTag caricato dal file con questo creato adesso nel Tag del nodo del tree.
						currentEnumNode.Tag = workingTag;

						workingTag.EnumItems.Add(enumItem);
						enumItem.PropertyChanged += new PropertyChangedEventHandler(EnumItem_PropertyChanged);
						enumItem.PropertyChanging += new PropertyChangingEventHandler(EnumItem_PropertyChanging);
					}
				}
			}

			//Mostro tutti i tipi di enumerativi
			rootNode.Expand();
		}

		//---------------------------------------------------------------------
		private void EnumsTreeView_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			try
			{
				//Se non ho selezionato un nodo oppure se il nodo selezionato ha il Tag nullo (non è associato ad un DataEnum)
				//allora non inizio il drag and drop.
				if (
					EnumsTreeView.SelectedNode == null ||
					EnumsTreeView.SelectedNode.Tag as EnumItem == null
					)
					return;

				DoDragDrop(
					new EnumsDropObject(EnumsTreeView.SelectedNode.FullPath.Replace(EnumsTreeView.PathSeparator, ".")),
						DragDropEffects.Copy
					);
			}
			catch { }
		}

		//---------------------------------------------------------------------
		private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnOpenProperties(sender, e);
		}

		//---------------------------------------------------------------------
		private void EnableActions(TreeNode node)
		{
			EnumTag aEnumTag = node.Tag as EnumTag;
			EnumItem aEnumItem = node.Tag as EnumItem;

			tsProperties.Enabled = aEnumItem != null || aEnumTag != null;

			IBaseModuleInfo ownerModule = null;
			IEasyBuilderApp currentEasyBuilderApp = null;

			//Abilito 'Delete' se e solo se:
			//1) sono su un EnumTag la coppia <Applicazione, Modulo> che ha apportato tale EnumTag è la CurrentCustomization
			//oppure
			//2) sono su un EnumItem la coppia <Applicazione, Modulo> che ha apportato tale EnumTag è la CurrentCustomization
			//	 AND L'EnumItem che sto cancellando NON è l'ultimo presente nel tag.
			tsDelete.Enabled =
				aEnumTag != null &&
				String.Compare(
					(ownerModule = aEnumTag.OwnerModule).Name,
					(currentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp).ModuleName,
					StringComparison.InvariantCultureIgnoreCase
				) == 0 &&
				String.Compare(
					ownerModule.ParentApplicationName,
					currentEasyBuilderApp.ApplicationName,
					StringComparison.InvariantCultureIgnoreCase
				) == 0
				||
				aEnumItem != null &&
				String.Compare(
					(ownerModule = aEnumItem.OwnerModule).Name,
					(currentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp).ModuleName,
					StringComparison.InvariantCultureIgnoreCase
				) == 0 &&
				String.Compare(
					ownerModule.ParentApplicationName,
					currentEasyBuilderApp.ApplicationName,
					StringComparison.InvariantCultureIgnoreCase
				) == 0 &&
				aEnumItem.Owner.EnumItems.Count > 1;

			tsAddEnumItem.Enabled = aEnumTag != null || aEnumItem != null;
		}

		//---------------------------------------------------------------------
		private void TsAddEnumTag_Click(object sender, EventArgs e)
		{
			IBaseModuleInfo ownerModuleInfo = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo;

			string newEnumTagName = GenerateNewName(rootNode, Resources.NewEnumTagNameMask);

			EnumTag newEnumTag = GenerateNewEnumTag(ownerModuleInfo, newEnumTagName);

			TreeNode newEnumTagTreeNode = AddEnumTagTreeNode(newEnumTagName, newEnumTag);


			string newEnumItemName = GenerateNewName(newEnumTagTreeNode, Resources.NewEnumItemNameMask);

			EnumItem newEnumItem = GenerateNewEnumItem(ownerModuleInfo, newEnumTag, newEnumItemName, 0);

			newEnumTag.EnumItems.Add(newEnumItem);

			AddEnumItemTreeNode(newEnumTagTreeNode, newEnumItemName, newEnumItem);

			//Aggiungo il tag al modello degli enumerativi in memoria.
			enumTagsForCurrentEasyBuilderApp.Add(newEnumTag);


			EnumsTreeView.SelectedNode = newEnumTagTreeNode;

			SetDirty(true);

			LoadEnumTagInTB(newEnumTag);

			selectionService.SetSelectedComponents(new Object[] { newEnumTagTreeNode.Tag });

			OnOpenProperties(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		private void TsAddEnumItem_Click(object sender, EventArgs e)
		{
			IBaseModuleInfo ownerModuleInfo = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo;
			string ownerAppInfoName = ownerModuleInfo.ParentApplicationName;

			TreeNode workingNode = EnumsTreeView.SelectedNode;
			if (workingNode == null)
				return;

			EnumTag selectedEnumTag = workingNode.Tag as EnumTag;
			EnumItem selectedEnumItem = workingNode.Tag as EnumItem;
			if (selectedEnumTag == null)
			{
				if (selectedEnumItem == null)
					return;

				selectedEnumTag = selectedEnumItem.Owner;
				workingNode = workingNode.Parent;
			}

			string newEnumItemName = GenerateNewName(workingNode, Resources.NewEnumItemNameMask);
			ushort newEnumItemValue = 0;

			string enumModuleName = selectedEnumTag.OwnerModule.Name;
			string enumApplicationName = selectedEnumTag.OwnerModule.ParentApplicationName;

			//Se non ho apportato io il tag allora genero un valore a caso per minimizzare i conflitti in caso di installazione
			//di diverse customizzazioni che modificano lo stesso tag
			//Altrimenti genero i valori come sequenza 0 1 2 3 come di consueto per mago
			if (
				String.Compare(enumModuleName, ownerModuleInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(enumApplicationName, ownerAppInfoName, StringComparison.InvariantCultureIgnoreCase) == 0
				)
				newEnumItemValue = GenerateNewValueForMyEnumItem(workingNode);
			else
				newEnumItemValue = GenerateNewValueForEnumItemAddedToErp(workingNode);

			EnumItem newEnumItem = GenerateNewEnumItem(ownerModuleInfo, selectedEnumTag, newEnumItemName, newEnumItemValue);

			EnumsTreeView.SelectedNode = AddEnumItemTreeNode(workingNode, newEnumItemName, newEnumItem);

			//Aggiungo l'item al tag che in memoria rappresenta il modello degli enumerativi.
			selectedEnumTag.EnumItems.Add(newEnumItem);

			//Sto aggiungendo un item ad un tag che non possiedo
			if (String.Compare(ownerModuleInfo.Name, selectedEnumTag.OwnerModule.Name, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				//Aggiungo l'intero tag a enumTagsForCurrentEasyBuilderApp che
				//rappresenta il modello in memoria del mio file Enums.xml.
				//Siccome il value non si sovrappone a quelli di Mago, TB ne farà un merge
				//proprio perchè il tag è uguale.
				EnumTag workingTag = enumTagsForCurrentEasyBuilderApp.AddTag(
					selectedEnumTag.OwnerModule,
					selectedEnumTag.Name,
					selectedEnumTag.Value,
					selectedEnumTag.DefaultValue
					);

				if (workingTag == null)
					workingTag = enumTagsForCurrentEasyBuilderApp.GetTag(selectedEnumTag.Name);

                if (!Object.ReferenceEquals(selectedEnumTag, workingTag))
                {
                    workingTag.EnumItems.Add(newEnumItem);
                }
			}

			SetDirty(true);

			LoadEnumItemInTB(newEnumItem);

			selectionService.SetSelectedComponents(new Object[] { newEnumItem });

			OnOpenProperties(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		private EnumTag GenerateNewEnumTag(
			IBaseModuleInfo ownerModuleInfo,
			string newEnumTagName
			)
		{
			ushort tagValue = GenerateNewEnumTagValue();

			EnumTag newEnumTag = new EnumTag(ownerModuleInfo, newEnumTagName, tagValue, 0);

			newEnumTag.Description = newEnumTagName;
			newEnumTag.Site = new TBSite(newEnumTag, null, null, newEnumTagName);

			newEnumTag.PropertyChanged += new PropertyChangedEventHandler(EnumTag_PropertyChanged);
			newEnumTag.PropertyChanging += new PropertyChangingEventHandler(EnumTag_PropertyChanging);

			return newEnumTag;
		}

		//---------------------------------------------------------------------
		private EnumItem GenerateNewEnumItem(
			IBaseModuleInfo ownerModuleInfo,
			EnumTag selectedEnumTag,
			string newEnumItemName,
			ushort newEnumItemValue
			)
		{
			EnumItem newEnumItem = new EnumItem(selectedEnumTag, newEnumItemName, newEnumItemValue, ownerModuleInfo);

			//Calcolo il valore stored
			DataEnum de = new DataEnum(selectedEnumTag.Value, newEnumItem.Value);
			int stored = 0;
			Int32.TryParse(de.ToString(), out stored);
			newEnumItem.Stored = stored;

			newEnumItem.Description = newEnumItemName;
			newEnumItem.Site = new TBSite(newEnumItem, selectedEnumTag, null, newEnumItemName);

			newEnumItem.PropertyChanged += new PropertyChangedEventHandler(EnumItem_PropertyChanged);
			newEnumItem.PropertyChanging += new PropertyChangingEventHandler(EnumItem_PropertyChanging);

			return newEnumItem;
		}

		//---------------------------------------------------------------------
		private TreeNode AddEnumTagTreeNode(
			string newEnumTagName,
			EnumTag newEnumTag
			)
		{
			TreeNode newEnumTagTreeNode = new TreeNode(
									 newEnumTagName,
									 (int)ImageLists.EnumsTreeImageIndex.Enum,
									 (int)ImageLists.EnumsTreeImageIndex.Enum
									);
			newEnumTagTreeNode.Tag = newEnumTag;

			rootNode.Nodes.Add(newEnumTagTreeNode);

			return newEnumTagTreeNode;
		}

		//---------------------------------------------------------------------
		private static TreeNode AddEnumItemTreeNode(
			TreeNode enumTagTreeNode,
			string newEnumItemName,
			EnumItem newEnumItem
			)
		{
			TreeNode newEnumItemTreeNode = new TreeNode(
									 newEnumItemName,
									 (int)ImageLists.EnumsTreeImageIndex.Enum,
									 (int)ImageLists.EnumsTreeImageIndex.Enum
									);
			newEnumItemTreeNode.Tag = newEnumItem;

			enumTagTreeNode.Nodes.Add(newEnumItemTreeNode);

			return newEnumItemTreeNode;
		}

		//---------------------------------------------------------------------
		void EnumTag_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			EnumTag tag = sender as EnumTag;
			if (tag == null)
				return;

			if (e.PropertyName == "Value")
			{
				//controllo range massimo e minimo e valori duplicati
				if (tag.Value > maxTagValue || tag.Value < 0)
				{
					using (SafeThreadCallContext stc = new SafeThreadCallContext())
						MessageBox.Show(String.Format(Resources.InvalidTagValue, maxTagValue));

					tag.Value = oldEnumTag.Value;
					return;
				}
				if (IsTagValueAlreadyPresent(tag))
				{
					using (SafeThreadCallContext stc = new SafeThreadCallContext())
						MessageBox.Show(Resources.TagValueAlreadyPresent);

					tag.Value = oldEnumTag.Value;
					return;
				}
			}

			if (e.PropertyName == "Name")
			{
				TreeNode tagNode = FindNode(rootNode, tag);
				if (tagNode != null)
				{
					tagNode.Text = tag.Name;
					tagNode.ToolTipText = tag.Name;
				}
			}

			if (oldEnumTag != null)
			{
				CUtility.DeleteEnumTag(oldEnumTag);
				oldEnumTag = null;
			}
			CUtility.AddEnumTag(tag);

			SetDirty(true);
		}

		//---------------------------------------------------------------------
		private bool IsTagValueAlreadyPresent(EnumTag tag)
		{
			EnumTag currentTag = null;
			foreach (TreeNode enumTagNode in rootNode.Nodes)
			{
				currentTag = enumTagNode.Tag as EnumTag;
				if (currentTag != null && currentTag != tag)
				{
					if (currentTag.Value == tag.Value)
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------
		void EnumTag_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			EnumTag aTag = sender as EnumTag;
			if (aTag == null)
				return;

			this.oldEnumTag = aTag.Clone() as EnumTag;
		}

		//---------------------------------------------------------------------
		void EnumItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			EnumItem item = sender as EnumItem;
			if (item == null)
				return;

			if (e.PropertyName == "Value")
			{
				//controllo range massimo e minimo e valori duplicati
				if (item.Value > maxItemValue || item.Value < 0)
				{
					using (SafeThreadCallContext stc = new SafeThreadCallContext())
						MessageBox.Show(String.Format(Resources.InvalidItemValue, maxItemValue));

					item.Value = oldEnumItem.Value;
					return;
				}
				if (IsItemValueAlreadyPresent(item))
				{
					using (SafeThreadCallContext stc = new SafeThreadCallContext())
						MessageBox.Show(Resources.ItemValueAlreadyPresent);

					item.Value = oldEnumItem.Value;
					return;
				}
			}

			if (e.PropertyName == "Name")
			{
				TreeNode tagNode = FindNode(rootNode, item.Owner);
                if (tagNode != null)
                {
                    TreeNode itemNode = FindNode(tagNode, item);
                    if (itemNode != null)
                    {
                        itemNode.Text = item.Name;
                        itemNode.ToolTipText = item.Name;
                    }
                }
			}

			if (oldEnumItem != null)
			{
				CUtility.DeleteEnumItem(oldEnumItem);
				oldEnumItem = null;
			}
			CUtility.AddEnumItem(item);

			SetDirty(true);
		}

		//---------------------------------------------------------------------
		private bool IsItemValueAlreadyPresent(EnumItem item)
		{
			foreach (EnumItem aItem in item.Owner.EnumItems)
			{
				if (aItem != null && aItem != item)
				{
					if (aItem.Value == item.Value)
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------
		void EnumItem_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			EnumItem aItem = sender as EnumItem;
			if (aItem == null)
				return;

			this.oldEnumItem = aItem.Clone() as EnumItem;
		}

		//---------------------------------------------------------------------
		private TreeNode FindNode(TreeNode contextNode, object nodeTagObject)
		{
			foreach (TreeNode treeNode in contextNode.Nodes)
			{
				if (treeNode.Tag == nodeTagObject)
					return treeNode;
			}

			return null;
		}

		//---------------------------------------------------------------------
		internal void SaveEnums()
		{
			if (!isDirty)
				return;

			IEasyBuilderApp currentApp = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;
			IBaseModuleInfo ownerModuleInfo = currentApp.ModuleInfo;

			string enumsFilePath = ownerModuleInfo.GetEnumsPath();

			try
			{
				//Se il file non esiste allora, oltre a crearlo, devo aggiungerlo alla customization list
				if (!File.Exists(enumsFilePath))
					BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.AddToCustomList(enumsFilePath);

				DirectoryInfo enumsDirInfo = new DirectoryInfo(Path.GetDirectoryName(enumsFilePath));
				if (!enumsDirInfo.Exists)
					enumsDirInfo.Create();

				enumTagsForCurrentEasyBuilderApp.SaveXml(enumsFilePath);
			}
			catch (Exception exc)
			{
				MessageBox.Show(
					this,
					String.Format(
						"Error saving enums for {0}, {1}: {2}",
						currentApp.ApplicationName,
						currentApp.ModuleName,
						exc.ToString()
						),
						Resources.EnumsFormTitle,
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
			}
			SetDirty(false);

			RefreshEnumsDll();
		}

		//---------------------------------------------------------------------
		private void RefreshEnumsDll()
		{
            string enumsDllPath = PathFinderWrapper.GetEasyStudioEnumsAssemblyName();

			FileInfo enumsDllFileInfo = new FileInfo(enumsDllPath);
			if (enumsDllFileInfo.Exists)
			{
				try { enumsDllFileInfo.Delete(); }
				catch { }

			}
			FileInfo enumsCsFileInfo = new FileInfo(Path.ChangeExtension(enumsDllPath, NewCustomizationInfos.CSSourceFileExtension));
			if (enumsCsFileInfo.Exists)
			{
				try { enumsCsFileInfo.Delete(); }
				catch { }

			}
			FileInfo enumsPdbFileInfo = new FileInfo(Path.ChangeExtension(enumsDllPath, NameSolverStrings.PdbExtension));
			if (enumsPdbFileInfo.Exists)
			{
				try { enumsPdbFileInfo.Delete(); }
				catch { }

			}

			StaticFunctions.GenerateEasyBuilderEnumsDllIfNecessary();

			editor.Sources.RemoveReferencedAssembly(Path.GetFileNameWithoutExtension(enumsDllPath));
			editor.Sources.RefreshReferencedAssemblies(false);//false perchè ho già rimosso io la dll incriminata
		}

		//---------------------------------------------------------------------
		private static string GenerateNewName(TreeNode contextNode, string tag)
		{
			string newName = null;
			bool nameAlreadyPresent = false;
			for (int i = 0; i <= EnumTag.MaxValue; i++)
			{
				newName = String.Format(tag, i.ToString());
				nameAlreadyPresent = false;
				foreach (TreeNode item in contextNode.Nodes)
				{
					if (String.Compare(item.Text, newName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						nameAlreadyPresent = true;
						break;
					}
				}
				if (nameAlreadyPresent)
					continue;

				break;
			}

			return newName;
		}

		//---------------------------------------------------------------------
		private static ushort GenerateNewEnumTagValue()
		{
			Random rnd = new Random(Environment.TickCount);
			ushort tagValue = 0;
			int range = maxTagValue - minTagValue;
			int iterations = 0;
			bool overflow = false;
			do
			{
				tagValue = (ushort)rnd.Next(minTagValue, maxTagValue);
				
				iterations++;
				
				overflow = iterations >= range;
				if (overflow)
					throw new EnumsException("No more EnumTag values free");

			} while (IsEnumTagValueAlreadyUsed(tagValue));

			return tagValue;
		}

		//---------------------------------------------------------------------
		private static bool IsEnumTagValueAlreadyUsed(ushort tagValue)
		{
			return enumTagValuesTable[tagValue] != null;
		}

		//---------------------------------------------------------------------
		private ushort GenerateNewValueForEnumItemAddedToErp(TreeNode enumTagTreeNode)
		{
			if (enumTagTreeNode == null)
				return 0;

			EnumTag currentEnumTag = enumTagTreeNode.Tag as EnumTag;
			if (currentEnumTag == null || currentEnumTag.EnumItems == null || currentEnumTag.EnumItems.Count == 0)
				return 0;

			//Se è già presente un item che ho aggiunto io, allora assegno il suo value +1 se libero,
			//se non libero passo a +2 ecc.
			//Se non è già presente un mio item allora genero un valore a caso per minimizzare
			//la probablità di conflitti.
			//Questo per mantenere una convenzione in essere per la quale i valori degli item di
			//un tag sono consecutivi
			bool alreadyHasAMyItem = false;
			ushort myMaxValue = 0;
			foreach (EnumItem item in currentEnumTag.EnumItems)
			{
				if (IsMyItem(item))
				{
					alreadyHasAMyItem = true;
					if (item.Value > myMaxValue)
						myMaxValue = item.Value;
				}
			}

			ushort newValue = 0;
			Func<ushort> generateNewValueFunc = null;
			if (alreadyHasAMyItem)
			{
				newValue = myMaxValue;
				generateNewValueFunc = new Func<ushort>(() => { return ++newValue; });
			}
			else
			{
				Random rnd = new Random(Environment.TickCount);
				generateNewValueFunc = new Func<ushort>(() => { return (ushort)rnd.Next(minItemValue, maxItemValue); });
			}

			bool valueAlreadyPresent = false;
			for (ushort i = 0; i <= ushort.MaxValue; i++)
			{
				newValue = generateNewValueFunc();
				valueAlreadyPresent = false;
				foreach (EnumItem enumItem in currentEnumTag.EnumItems)
				{
					if (enumItem.Value == newValue)
					{
						valueAlreadyPresent = true;
						break;
					}
				}
				if (valueAlreadyPresent)
					continue;

				break;
			}

			return newValue;
		}

		//---------------------------------------------------------------------
		private bool IsMyItem(EnumItem item)
		{
			if (item == null)
				return false;

			string itemOwnerModuleInfoName = item.OwnerModule.Name;
			string itemOwnerAppInfoName = item.OwnerModule.ParentApplicationName;

			string myModuleInfoName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName;
			string myAppInfoName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;

			return
				String.Compare(itemOwnerModuleInfoName, myModuleInfoName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(itemOwnerAppInfoName, myAppInfoName, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		//---------------------------------------------------------------------
		private ushort GenerateNewValueForMyEnumItem(TreeNode enumTagTreeNode)
		{
			if (enumTagTreeNode == null)
				return 0;

			EnumTag currentEnumTag = enumTagTreeNode.Tag as EnumTag;
			if (currentEnumTag == null || currentEnumTag.EnumItems == null || currentEnumTag.EnumItems.Count == 0)
				return 0;

			bool valueAlreadyPresent = false;
			ushort i = 0;
			for (; i <= ushort.MaxValue; i++)
			{
				foreach (EnumItem enumItem in currentEnumTag.EnumItems)
				{
					if (enumItem.Value == i)
					{
						valueAlreadyPresent = true;
						break;
					}
				}
				if (valueAlreadyPresent)
				{
					valueAlreadyPresent = false;
					continue;
				}

				break;
			}

			return i;
		}

		//---------------------------------------------------------------------
		private void TsDelete_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = EnumsTreeView.SelectedNode;
			if (selectedNode == null)
				return;

			EnumTag enumTag = selectedNode.Tag as EnumTag;
			EnumItem enumItem = selectedNode.Tag as EnumItem;

			bool isEnumTagNull = enumTag == null;
			bool isEnumItemNull = enumItem == null;

			if (isEnumTagNull && isEnumItemNull)
				return;

			if (!isEnumTagNull)
			{
				enumTagsForCurrentEasyBuilderApp.Remove(enumTag);
				UnloadEnumTagFromTBMemory(enumTag);
			}
			else
			{
                //Rimuovo l'enumItem dal TB C# in memoria.
                EnumTag owner = enumItem.Owner;
				owner.EnumItems.Remove(enumItem);

				if (owner.DefaultValue == enumItem.Value && owner.EnumItems.Count > 0)
                {
                    owner.DefaultValue = owner.EnumItems[0].Value;
                }

                //Rimuovo l'enumItem dall'enumtag che mi permette di gestire gli enumerativi della mia personalizzaizone.
                if (enumTagsForCurrentEasyBuilderApp.Count > 0)
                {
                    enumTag = enumTagsForCurrentEasyBuilderApp.GetTag(owner.Name);
                    if (enumTag != null)
                    {
                        enumTag.EnumItems.Remove(enumItem);
                    }
                }

                //Rimuovo l'enumItem dal TB C++ in memoria.
                CUtility.DeleteEnumItem(enumItem);
            }

			TreeNode toBeDeletedNode = selectedNode;
			this.EnumsTreeView.SelectedNode = selectedNode.NextNode != null ? selectedNode.NextNode : selectedNode.Parent;
			toBeDeletedNode.Parent.Nodes.Remove(toBeDeletedNode);

			selectionService.SetSelectedComponents(new Object[] { this.EnumsTreeView.SelectedNode.Tag });

			SetDirty(true);
		}

		//---------------------------------------------------------------------
		private static void LoadEnumItemInTB(EnumItem newEnumItem)
		{
			//Aggiungo l'enum item al TB C++ in memoria.
			CUtility.AddEnumItem(newEnumItem);
			//Aggiungo l'enum item al TB C# in memoria.
			DataType.RefreshEnumTypes();
		}

		//---------------------------------------------------------------------
		private static void LoadEnumTagInTB(EnumTag newEnumTag)
		{
			//Aggiungo l'enumTag al TB C++ in memoria.
			CUtility.AddEnumTag(newEnumTag);
			//Aggiungo l'enum item al TB C# in memoria.
			DataType.Enums.Tags.Add(newEnumTag);
			DataType.RefreshEnumTypes();
		}

		//---------------------------------------------------------------------
		private static void UnloadEnumTagFromTBMemory(EnumTag enumTag)
		{
			//Rimuovo l'enumTag al TB C++ in memoria.
			CUtility.DeleteEnumTag(enumTag);
			//Rimuovo l'enum item dal TB C# in memoria.
			DataType.Enums.Tags.DeleteTag(enumTag.Name);
			DataType.RefreshEnumTypes();
		}

		//---------------------------------------------------------------------
		private void EnumsTreeView_MouseUp(object sender, MouseEventArgs e)
		{
			TreeNode node = EnumsTreeView.GetNodeAt(e.X, e.Y);
			EnumsTreeView.SelectedNode = node;
			EnableActions(node);

			if (node != null)
				selectionService.SetSelectedComponents(new Object[] { node.Tag });
		}

		//---------------------------------------------------------------------
		private void EnumsTreeView_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				//Se il nodo selezionato è nullo o diverso dal precedente selezionato, 
				//viene impostato il nodo su cui si è effettuata il click per il drag drop
				TreeNode node = EnumsTreeView.GetNodeAt(e.Location);
				if (EnumsTreeView.SelectedNode == null || EnumsTreeView.SelectedNode != node)
					EnumsTreeView.SelectedNode = node;
			}
			catch { }
		}

		#region IDirtyManager Members

		//---------------------------------------------------------------------
		/// <summary>
		/// Sets the dirty flag
		/// </summary>
		/// <param name="dirty"></param>
		public void SetDirty(bool dirty)
		{
			if (suspendDirtyChanges)
				return;

			if (this.isDirty == dirty)
				return;

			this.isDirty = dirty;
			OnDirtyChanged(new DirtyChangedEventArgs(isDirty));
		}

		//---------------------------------------------------------------------
		private void OnDirtyChanged(DirtyChangedEventArgs e)
		{
			if (DirtyChanged != null)
				DirtyChanged(this, e);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating if the document should be saved
		/// </summary>
		public bool IsDirty
		{
			get { return isDirty; }
		}

		#endregion

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Suspends dirty changes
		/// </summary>
		public bool SuspendDirtyChanges
		{
			get
			{
				return this.suspendDirtyChanges;
			}
			set
			{
				this.suspendDirtyChanges = value;
			}
		}
	}

	//=========================================================================
	class EnumsDropObject
	{
		string enumSourceCodeString;

		//---------------------------------------------------------------------
		public string EnumSourceCodeString
		{
			get { return enumSourceCodeString; }
		}

		//---------------------------------------------------------------------
		public EnumsDropObject(string enumSourceCodeString)
		{
			this.enumSourceCodeString = enumSourceCodeString;
		}
	}
}
