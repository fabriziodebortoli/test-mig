using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Core.RegressionTestLibrary.WizardPages
{
	//=========================================================================
	public partial class AreasSelectionsListPage : InteriorWizardPage
	{
		# region Variabili private
    	//private System.Windows.Forms.ToolTip TableToolTip;

		private RegressionTestSelections dataSel = null;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public AreasSelectionsListPage()
		{
			InitializeComponent();
			InitializeImageList();
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
			
			dataSel = ((RegressionTestWizard)this.WizardManager).DataSelections;

			LoadAvailableUnits();

			SetControlsValue();
		
			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			
			return true;
		}
		# endregion

		# region Inizializzazione ImageList
		//---------------------------------------------------------------------
		private void InitializeImageList()
		{
			SelectedUnitsTreeView.ImageList = myImages;
			SourceUnitsTreeView.ImageList = myImages;
		}
		# endregion

		# region Caricamento delle informazioni suddivise per area+unit nel TreeView
		/// <summary>
		/// per caricare i nomi unit (che a loro volta contengono i nome e i path dei relativi dataset)
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAvailableUnits()
		{
			SelectedUnitsTreeView.Nodes.Clear();
			SourceUnitsTreeView.BeginUpdate();
			SourceUnitsTreeView.Nodes.Clear();

			NRTTreeNode areaNode = null; 
			NRTTreeNode unitNode = null; 

			DirectoryInfo mainDir = new DirectoryInfo(dataSel.RepositoryPath);

			ArrayList DataSets = new ArrayList();
			foreach (DirectoryInfo subDir in mainDir.GetDirectories())
			{
				DataSets.Clear();

				if (File.Exists(Path.Combine(subDir.FullName, "Area.config")))
				{
					string nomeArea = string.Empty;
					nomeArea = subDir.FullName.Substring(subDir.FullName.LastIndexOf("\\") + 1);
					areaNode = new NRTTreeNode(nomeArea, "");
					areaNode.ImageIndex = 1;
					areaNode.SelectedImageIndex = 1;
					SourceUnitsTreeView.Nodes.Add(areaNode);

					XmlDocument xDoc = new XmlDocument();
					xDoc.Load(Path.Combine(subDir.FullName, "Area.config"));

					foreach (XmlNode n in xDoc.SelectNodes("Area/Unit"))
					{
						string nomeUnit = n.Attributes["Name"].Value;
						string dataset = n.Attributes["DataSet"].Value;
						string dataSetPath = Path.Combine(Path.Combine(subDir.FullName, "Data"), dataset);

						if (!DataSets.Contains(dataset))
						{
							unitNode = new NRTTreeNode(nomeUnit, "", dataset, dataSetPath);
							unitNode.ImageIndex = 0;
							unitNode.SelectedImageIndex = 0;

							DataSets.Add(dataset);
							areaNode.Nodes.Add(unitNode);
						}
					}
				}
			}
			SourceUnitsTreeView.EndUpdate();
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();
			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
			return base.OnWizardBack();
		}
		# endregion

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			foreach (AreaItem aItem in dataSel.AreaItems.Values)
			{
				AddSelectedArea(aItem);
			}
			//TODO Fabio: Ho tolto questa parte per il momento
			/*foreach (UnitListViewItem ulvi in dataSel.UnitListViewItems)
			{
				NRTTreeNode selectedNode = GetNodeByUnitListViewItem(ulvi);

				if (selectedNode == null)
					continue;

				InsertUnitInfo(selectedNode); // sposto la singola unit
				selectedNode.Remove();
			}*/
		}

		private void AddSelectedArea(AreaItem aItem)
		{
			foreach(DataSetItem dsItem in aItem.DataSetItems.Values)
			{
				NRTTreeNode areaNode = null;
				foreach (NRTTreeNode item in SourceUnitsTreeView.Nodes)
				{
					if (item.Text == aItem.Name)
					{
						areaNode = item;
						break;
					}
				}
				if (areaNode == null)
					break;

				AddSelectedDataSet(areaNode, dsItem);

				if (areaNode.Nodes.Count == 0)
					areaNode.Remove();
			}
		}

		private void AddSelectedDataSet(NRTTreeNode areaNode, DataSetItem dsItem)
		{
			foreach (NRTTreeNode item in areaNode.Nodes)
			{
				if (item.Text == dsItem.Name)
				{
					InsertUnitInfo(item);
					item.Remove();
					return;
				}
			}
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo alla DataSelections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			dataSel.AreaItems.Clear();

			foreach (NRTTreeNode tAreaNode in SelectedUnitsTreeView.Nodes)
			{
				dataSel.AreaItems.Add(tAreaNode.Text, tAreaNode.MyNRTObject);
				foreach (NRTTreeNode tDataSetNode in tAreaNode.Nodes)
				{
					AreaItem aItem = (AreaItem) dataSel.AreaItems[tAreaNode.Text];
					
					DataSetItem aDSItem = (DataSetItem)tDataSetNode.MyNRTObject;
					aItem.AddDataSet(tDataSetNode.Text, aDSItem.Name, aDSItem.Description, aDSItem.Path);
				}
			}
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage
				(
				this, 
				RegressionTestLibraryConsts.NamespacePlugIn, 
				RegressionTestLibraryConsts.SearchParameter + "AreasSelectionsListPage"
				);

			return true;
		}
		#endregion

		# region Funzioni per spostare oggetti di tipo Tabella dal TreeView alla ListView e viceversa
		/// <summary>
		/// da un nodo di tipo Area vado ad inserire tutte le sue unit nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertAreaInfo(NRTTreeNode selectedNode)
		{
			foreach (NRTTreeNode tableNode in selectedNode.Nodes)
				InsertUnitInfo(tableNode);
		}

		private bool ContainsTreeNode(TreeView view, NRTTreeNode child)
		{
			foreach (NRTTreeNode aree in view.Nodes)
			{
				if (aree.Text == child.Text)
					return true;
			}
			return false;
		}

		private NRTTreeNode GetTreeNode(TreeView view, string name)
		{
			foreach (NRTTreeNode aree in view.Nodes)
			{
				if (aree.Text == name)
					return aree;
			}
			return null;
		}

		/// <summary>
		/// inserisce l'elemento unit nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertUnitInfo(NRTTreeNode unitNode)
		{			
			SelectedUnitsTreeView.BeginUpdate();
			
			if (!ContainsTreeNode(SelectedUnitsTreeView, (NRTTreeNode)unitNode.Parent))
			{
				NRTTreeNode oldParentNode = (NRTTreeNode) unitNode.Parent;
				NRTTreeNode newParentNode = new NRTTreeNode(oldParentNode.Text, oldParentNode.DataDescription);
				newParentNode.ImageIndex = 1;
				newParentNode.SelectedImageIndex = 1;
				SelectedUnitsTreeView.Nodes.Add(newParentNode);
			}

			NRTTreeNode parentNode = GetTreeNode(SelectedUnitsTreeView, unitNode.Parent.Text);

			NRTTreeNode newChildNode = new NRTTreeNode(unitNode.Text, unitNode.DataDescription, unitNode.DataSetName, unitNode.DataSetPath);

			newChildNode.ImageIndex = 0;
			newChildNode.SelectedImageIndex = 0;

			parentNode.Nodes.Add(newChildNode);

			SelectedUnitsTreeView.EndUpdate();
		}

		/// <summary>
		/// rimuove il singolo elemento unit dalla listview e lo inserisce nell'albero
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveSingleItem(NRTTreeNode unitNode)
		{
			SourceUnitsTreeView.BeginUpdate();
			
			if (!ContainsTreeNode(SourceUnitsTreeView, (NRTTreeNode)unitNode.Parent))
			{
				NRTTreeNode oldParentNode = (NRTTreeNode) unitNode.Parent;
				NRTTreeNode newParentNode = new NRTTreeNode(oldParentNode.Text, oldParentNode.DataDescription);
				newParentNode.ImageIndex = 1;
				newParentNode.SelectedImageIndex = 1;
				SourceUnitsTreeView.Nodes.Add(newParentNode);
			}

			NRTTreeNode parentNode = GetTreeNode(SourceUnitsTreeView, unitNode.Parent.Text);

			NRTTreeNode newChildNode = new NRTTreeNode(unitNode.Text, unitNode.DataDescription, unitNode.DataSetName, unitNode.DataSetPath);

			newChildNode.ImageIndex = 0;
			newChildNode.SelectedImageIndex = 0;

			parentNode.Nodes.Add(newChildNode);

			NRTTreeNode ParentDestNode = (NRTTreeNode) unitNode.Parent;

			unitNode.Remove();
			if (ParentDestNode.Nodes.Count == 0)
				ParentDestNode.Remove();

			SourceUnitsTreeView.EndUpdate();
		}

		private NRTTreeNode GetNodeByUnitListViewItem(UnitListViewItem item)
		{
			NRTTreeNode nParent = (NRTTreeNode)item.Tag;
			
			foreach (NRTTreeNode tNode in nParent.Nodes)
			{
				if (tNode.Text == item.Text)
					return tNode;
			}

			return null;
		}

		# endregion
		
		# region Eventi sui bottoni >, >>, <<
		/// <summary>
		/// evento sul pulsante > (aggiungo l'entry selezionato)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddButton_Click(object sender, System.EventArgs e)
		{
			NRTTreeNode selectedNode = (NRTTreeNode)SourceUnitsTreeView.SelectedNode;

			if (selectedNode.Index != -1)
			{
				if (selectedNode.IsUnit)
				{
					InsertUnitInfo(selectedNode); // sposto la singola unit
					NRTTreeNode parentNode = (NRTTreeNode) selectedNode.Parent;
					selectedNode.Remove();
					if (parentNode.Nodes.Count == 0)
						parentNode.Remove();
				}
				else
				{
					InsertAreaInfo(selectedNode);// sposto tutte le Units dell'Area
					selectedNode.Nodes.Clear();
					selectedNode.Remove();
				}
			}

			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Next | WizardButton.Back);
		}

		/// <summary>
		/// evento sul pulsante maggiore maggiore (aggiungo tutti gli item presenti)
		/// </summary>
		//---------------------------------------------------------------------
		private void AddAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (NRTTreeNode item in SourceUnitsTreeView.Nodes)
			{
				SourceUnitsTreeView.SelectedNode = item;
				AddButton_Click(sender, e);
			}
			
			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Next | WizardButton.Back);
		}
		/// <summary>
		/// evento sul pulsante Minore rimuovo le unit selezionate
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveButton_Click(object sender, System.EventArgs e)
		{
			NRTTreeNode selectedNode = (NRTTreeNode)SelectedUnitsTreeView.SelectedNode;

			if (selectedNode == null)
				return;

			if (!((NRTTreeNode)SelectedUnitsTreeView.SelectedNode).IsUnit)
			{
				for (int idx = SelectedUnitsTreeView.SelectedNode.Nodes.Count - 1; idx >= 0; idx --)
				{
					RemoveSingleItem((NRTTreeNode)SelectedUnitsTreeView.SelectedNode.Nodes[idx]);
				}
			}
			else
				RemoveSingleItem((NRTTreeNode)SelectedUnitsTreeView.SelectedNode);

			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}

		/// <summary>
		/// evento sul pulsante minore minore (rimuovo tutti gli item presenti)
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (NRTTreeNode item in SelectedUnitsTreeView.Nodes)
			{
				SelectedUnitsTreeView.SelectedNode = item;
				RemoveButton_Click(sender, e);
			}
			
			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}
		# endregion

		# region Eventi sul TreeView
		/// <summary>
		/// evento sul doppio click su un nodo del tree
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceUnitsTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			NRTTreeNode selectedNode = (NRTTreeNode)SourceUnitsTreeView.SelectedNode;

			if (selectedNode.Index != -1)
			{
				if (selectedNode.IsUnit)
				{
					InsertUnitInfo(selectedNode); // sposto solo la singola unit
					selectedNode.Remove(); 
				}
			}

			if (SelectedUnitsTreeView.Nodes.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Next | WizardButton.Back);
		}		
		# endregion
	}

	//=========================================================================
	public class NRTTreeNode : TreeNode
	{
		public string		DataSetName		= string.Empty;
		public string		DataDescription = string.Empty;
		public string		DataSetPath		= string.Empty;
		public bool			IsUnit			= false;
		public NRTObject	MyNRTObject		= null;

		public NRTTreeNode(string name, string description, string dataset, string dataSetPath)
		{
			Text = dataset;
			DataSetName = dataset;
			DataDescription = description;
			DataSetPath = dataSetPath;
			IsUnit = true;
			MyNRTObject = new DataSetItem(name, description, dataSetPath);
		}

		public NRTTreeNode(string name, string description)
		{
			Text = name;
			DataDescription = description;
			IsUnit = false;
			MyNRTObject = new AreaItem(name, description);
		}
	}

	//=========================================================================
	public class UnitListViewItem : ListViewItem
	{
		public string DataSetName = string.Empty;
		public string DataSetPath = string.Empty;

		public UnitListViewItem(string dataset, string dataSetPath)
		{
			DataSetName = dataset;
			DataSetPath = dataSetPath;
		}
	}
}
