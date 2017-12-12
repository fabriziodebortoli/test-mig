using System.Collections;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	//=========================================================================
	public partial class SelectedTables : System.Windows.Forms.Form
	{
		private ArrayList tableNodes = new ArrayList();
		private ApplicationsTree applicationsTree = null;
		private PlugInTreeNode selectNode = null;

		// events e delegates
		public delegate void AfterClickOkButton(object sender, DynamicEventsArgs e);
		public event AfterClickOkButton OnAfterClickOkButton;

		public delegate void AfterClickCloseButton(object sender, DynamicEventsArgs e);
		public event AfterClickCloseButton OnAfterClickCloseButton;

		//---------------------------------------------------------------------
		public SelectedTables(ApplicationsTree tree)
		{
			InitializeComponent();
			applicationsTree = tree;
		}
		
		#region Caricamento e visualizzazione delle tabelle nella listview
		/// <summary>
		///carico i nodi del modulo che soddisfando l'operazione richiesta
		///- inserimento in tracciatura
		///		le sole tabelle non tracciate
		///- eliminazione dalla tracciatura
		///		le tabelle tracciate
		///		le tabelle con tracciatura sospesa
		///- sospendi tracciatura
		///		le sole tabelle tracciate
		///- riprendi tracciatura
		///le sole tabelle con tracciatura sospesa		
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadModuleTables(PlugInTreeNode modNode, ApplicationsTree.EnumOperations operation)
		{
			foreach (PlugInTreeNode node in modNode.Nodes)
			{
				switch (operation)
				{
					case ApplicationsTree.EnumOperations.Insert:
						if (node.Type == AuditConstStrings.NoTracedTable)
							tableNodes.Add(node); break;

					case ApplicationsTree.EnumOperations.Stop:
						if (node.Type == AuditConstStrings.TracedTable ||
							node.Type == AuditConstStrings.PauseTraceTable)
							tableNodes.Add(node); break;
					
					case ApplicationsTree.EnumOperations.Pause:
						if (node.Type == AuditConstStrings.TracedTable)
							tableNodes.Add(node); break;
					
					case ApplicationsTree.EnumOperations.Restart:
						if (node.Type == AuditConstStrings.PauseTraceTable)
							tableNodes.Add(node); break;
				}
			}
		}
	
		/// <summary>
		/// carico le tabelle che verificano l'operazione prescelta
		/// </summary>
		//----------------------------------------------------------------------
		public void LoadTables(PlugInTreeNode node, ApplicationsTree.EnumOperations operation)
		{
			tableNodes.Clear();
			
			if (node.Type == AuditConstStrings.Application)
			{
				foreach (PlugInTreeNode modNode in node.Nodes)
					LoadModuleTables(modNode, operation);
			}
			else
				if (node.Type == AuditConstStrings.Module)
					LoadModuleTables(node, operation);
		}
		
		/// <summary>
		/// carico le tabelle nella listview e visualizzo la form
		/// </summary>
		//---------------------------------------------------------------------
		public void ShowTables(PlugInTreeNode node, ApplicationsTree.EnumOperations operation)
		{
			if (node == null)
				return;
			
			selectNode = node;
			LoadTables(node, operation);

			if (tableNodes.Count == 0)
			{
				DynamicEventsArgs ee = new DynamicEventsArgs();
				if (OnAfterClickCloseButton != null)
					OnAfterClickCloseButton(this, ee);
				return;
			}

			switch (operation)
			{
				case ApplicationsTree.EnumOperations.Insert:
					Text = Strings.InsertInAuditing; 
					break; 
				
				case ApplicationsTree.EnumOperations.Stop:
					Text = Strings.StopAuditing; 
					break; 

				case ApplicationsTree.EnumOperations.Pause:
					Text = Strings.PauseAuditing; 
					break; 

				case ApplicationsTree.EnumOperations.Restart:
					Text = Strings.RestartAuditing; 
					break; 
			}

			lstTables.Clear();
			lstTables.View				= View.Details;
			lstTables.CheckBoxes		= true;
			lstTables.AllowColumnReorder= true;
			lstTables.Activation		= ItemActivation.OneClick;
			
			lstTables.Columns.Add(Strings.TableName,		150,HorizontalAlignment.Left);
			lstTables.Columns.Add(Strings.ModuleName,		150,HorizontalAlignment.Left);
			lstTables.Columns.Add(Strings.ApplicationName,	200,HorizontalAlignment.Left);
						
			foreach (PlugInTreeNode tableNode in tableNodes)
			{
				ListViewItem listViewItem = new ListViewItem();
				listViewItem.Checked= true;
				listViewItem.Tag	= tableNode;
				listViewItem.Text	= tableNode.Text;
				
				listViewItem.SubItems.Add(tableNode.Parent.Text); //module name
				listViewItem.SubItems.Add(tableNode.Parent.Parent.Text); //application name
				lstTables.Items.Add(listViewItem);
			}
			//visualizzo
			Show();
		}
		#endregion

		//----------------------------------------------------------------------
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem listItemTable in lstTables.Items)
				if (listItemTable.Checked == false)
					tableNodes.Remove(listItemTable.Tag);		
						
		//	Visible = false;
			DynamicEventsArgs list = new DynamicEventsArgs(tableNodes);

			if (OnAfterClickOkButton != null)
				OnAfterClickOkButton(sender, list);
			
			Close();
		}

		//---------------------------------------------------------------------
		private void SelectedTables_Closed(object sender, System.EventArgs e)
		{
			DynamicEventsArgs ee = new DynamicEventsArgs();
			Visible = false;
			
			if (OnAfterClickCloseButton != null)
				OnAfterClickCloseButton(sender, ee);
		}

		//---------------------------------------------------------------------
		private void btnCancel_Click(object sender, System.EventArgs e)
		{			
			Close();
		}
	}
}
