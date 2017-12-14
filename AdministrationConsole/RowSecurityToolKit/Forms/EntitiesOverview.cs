using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	///<summary>
	/// Form per la visualizzazione delle entita' e relative tabelle definite nell'applicazione
	/// Esse sono caricate dai file di configurazione RowSecurityObjects.xml
	///</summary>
	//================================================================================
	public partial class EntitiesOverview : Form
	{
		private RSSelections rsSelections = null;

		// lista oggetti caricati dai file di configurazione
		private List<RowSecurityObjectsInfo> rowSecurityObjectsList = new List<RowSecurityObjectsInfo>();

		// struttura in memoria globale con le entita' e tutte le tabelle referenziate, caricate dai file RowSecurityObjects.xml
		private Dictionary<string, RSEntityInfo> entitiesDictionary = new Dictionary<string, RSEntityInfo>(StringComparer.OrdinalIgnoreCase);

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public EntitiesOverview(ContextInfo context, BrandLoader brand, ImageList imageList)
		{
			InitializeComponent();
			EntitiesTreeView.ImageList = imageList;

			rsSelections = new RSSelections(context, brand);

			LoadRowSecurityObjectsInfo();
			FillTreeView();

			if (entitiesDictionary.Count == 0)
				DiagnosticViewer.ShowWarning(Strings.NoFilesAvailable, string.Empty);
		}

		//--------------------------------------------------------------------------------
		private void LoadRowSecurityObjectsInfo()
		{
			rsSelections.LoadRowSecurityObjectsInfo();

			foreach (RowSecurityObjectsInfo rsoi in rsSelections.RowSecurityObjectsList)
			{
				// primo giro per memorizzare nel dictionary le entita'
				if (rsoi != null && rsoi.RSEntities != null)
				{
					foreach (RSEntity item in rsoi.RSEntities)
					{
						RSEntityInfo rs;
						// se non la trovo la inserisco (e se ce ne fosse una duplicata con lo stesso nome?)
						if (!entitiesDictionary.TryGetValue(item.Name, out rs))
							entitiesDictionary.Add(item.Name, new RSEntityInfo(item));
					}
				}

				// memorizzo le info in una lista di appoggio, che mi serve dopo
				rowSecurityObjectsList.Add(rsoi);
			}

			// seconda passata per assegnare le tabelle alle entita'
			foreach (RowSecurityObjectsInfo rsoi in rowSecurityObjectsList)
			{
				if (rsoi.RSTables != null)
				{
					// per ogni tabella
					foreach (RSTable rst in rsoi.RSTables)
					{
						// per ogni Entity di base
						foreach (RSEntityBase entityBase in rst.RsEntityBaseList)
						{
							// cerco nel dictionary l'entity corrispondente (se non la trovo significa che e' sbagliato il nome)
							// e aggiungo tutte le tabelle che la referenziano
							RSEntityInfo rsi;
							if (entitiesDictionary.TryGetValue(entityBase.Name, out rsi))
							{
								RSTableInfo tbl = new RSTableInfo(rst);
								tbl.RsColumns.AddRange(entityBase.RsColumns);
								rsi.RsTablesInfo.Add(tbl);
							}
						}
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void FillTreeView()
		{
			EntitiesTreeView.BeginUpdate();
			EntitiesTreeView.Nodes.Clear();

			foreach (KeyValuePair<string, RSEntityInfo> kvp in entitiesDictionary)
			{
				RSEntityInfo rsei = kvp.Value;

				string entityName = string.Format("{0}", rsei.Name);
				if (!string.IsNullOrWhiteSpace(rsei.Description))
					entityName += string.Format(" ({0})", rsei.Description);

				// nodo entity
				TreeNode entityNode = new TreeNode(entityName);
				entityNode.ImageIndex = entityNode.SelectedImageIndex = PlugInTreeNode.GetModuleImageIndex;

				// nodo information
				TreeNode infoNode = new TreeNode(Strings.Information);
				infoNode.ImageIndex = infoNode.SelectedImageIndex = PlugInTreeNode.GetInformationImageIndex;
				
				TreeNode masterTblNode = new TreeNode(string.Format(Strings.MasterTable, rsei.MasterTableName));
				infoNode.Nodes.Add(masterTblNode);
				
				TreeNode docNsNode = new TreeNode(string.Format(Strings.DocumentNamespace, rsei.DocumentNamespace));
				infoNode.Nodes.Add(docNsNode);

				TreeNode priorityNode = new TreeNode(string.Format(Strings.Priority, rsei.Priority.ToString()));
				infoNode.Nodes.Add(priorityNode);

				// nodo contenitore colonne
				TreeNode colContainerNode = new TreeNode(Strings.Columns);
				colContainerNode.ImageIndex = colContainerNode.SelectedImageIndex = PlugInTreeNode.GetDefaultImageIndex;
				foreach (RSColumn rsCol in rsei.RsColumns)
				{
					TreeNode rsColNode = new TreeNode(rsCol.Name);
					rsColNode.ImageIndex = rsColNode.SelectedImageIndex = PlugInTreeNode.GetColumnDefaultImageIndex;
					colContainerNode.Nodes.Add(rsColNode);
				}
				infoNode.Nodes.Add(colContainerNode);

				// nodo contenitore Tables
				TreeNode tblContainerNode = new TreeNode(Strings.Tables);
				tblContainerNode.ImageIndex = tblContainerNode.SelectedImageIndex = PlugInTreeNode.GetDefaultImageIndex;

				foreach (RSTableInfo rsTbl in rsei.RsTablesInfo)
				{
					TreeNode rsTblNode = new TreeNode(rsTbl.NameSpace);
					rsTblNode.ImageIndex = rsTblNode.SelectedImageIndex = PlugInTreeNode.GetTableDefaultImageIndex;

					foreach (RSColumns rsColumns in rsTbl.RsColumns)
					{
						TreeNode columnsContainerNode = new TreeNode(Strings.Columns);
						rsTblNode.Nodes.Add(columnsContainerNode);
						foreach (RSColumn rsColumn in rsColumns.RSColumnList)
						{
							TreeNode colNode = new TreeNode(string.Format("{0} ({1}: {2})", rsColumn.Name, Strings.EntityColumn, rsColumn.EntityColumn));
							colNode.ImageIndex = colNode.SelectedImageIndex = 30;
							columnsContainerNode.Nodes.Add(colNode);
						}
					}
					tblContainerNode.Nodes.Add(rsTblNode);
				}
				
				entityNode.Nodes.Add(tblContainerNode);
				EntitiesTreeView.Nodes.Add(entityNode);
			}

			EntitiesTreeView.EndUpdate();
		}

		///<summary>
		/// Se clicco su Esc chiudo la form
		///</summary>
		//--------------------------------------------------------------------------------
		private void EntitiesTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == (int)Keys.Escape)
				this.Close();
		}
	}
}
