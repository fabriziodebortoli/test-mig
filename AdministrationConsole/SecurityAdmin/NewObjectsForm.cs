using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;


namespace Microarea.Console.Plugin.SecurityAdmin
{
	/// <summary>
	/// Summary description for NewObjectsForm.
	/// </summary>
	public partial class NewObjectsForm : System.Windows.Forms.Form
	{
		
		#region private Data Member
		private int companyId = -1;
		private ArrayList		newObjectsArrayList = null;
		private SqlConnection	sqlConnection		= null;
		private	string			path				= string.Empty;
		private DataTable		newObjectsDataTable	= null;
		#endregion

		//---------------------------------------------------------------------------
		public NewObjectsForm(ArrayList newObjectsArrayList, int companyId, SqlConnection sqlConnection, PathFinder pathFinder)
		{
			InitializeComponent();

			this.newObjectsArrayList	= newObjectsArrayList;
			this.companyId				= companyId;
			this.sqlConnection			= sqlConnection;
			
			path = Path.Combine(pathFinder.GetCustomCompanyPath(CommonObjectTreeFunction.GetCompanyName(companyId, sqlConnection)) , SecurityConstString.SecurityAdmin);
			path = Path.Combine(path, SecurityConstString.LogFolder);

			NewObjectTextLabel.Text = NewObjectTextLabel.Text + "(" +  path + " )";
			AddListViewColumns();
		
			if (newObjectsDataTable == null)
				InitNewObjectsDataTable();

			AddRows();
		}


		//---------------------------------------------------------------------
		public void AddRow(MenuXmlNode node, SqlConnection sqlOSLConnection)
		{
			if (node == null)
				return;
			
			DataRow dr = newObjectsDataTable.NewRow();
		//	dr[SecurityConstString.ObjectId]	= CommonObjectTreeFunction.GetObjectId(node, sqlOSLConnection);
			dr[SecurityConstString.Description]	= node.Title;
            dr[SecurityConstString.Type] = ControlsString.GetControlDescription(CommonObjectTreeFunction.GetSecurityNodeTypeFromCommandNode(node));
			dr[SecurityConstString.NameSpace]	= node.ItemObject;
			dr[SecurityConstString.Protected]	= true;

			newObjectsDataTable.Rows.Add(dr);

		}
		//---------------------------------------------------------------------
		public void InitNewObjectsDataTable()
		{
			newObjectsDataTable = new DataTable("NewObject");
		//	newObjectsDataTable.Columns.Add(SecurityConstString.ObjectId,Type.GetType("System.Int32"));
			newObjectsDataTable.Columns.Add(SecurityConstString.Description, Type.GetType("System.String"));
			newObjectsDataTable.Columns.Add(SecurityConstString.Type, Type.GetType("System.String"));
			newObjectsDataTable.Columns.Add(SecurityConstString.NameSpace, Type.GetType("System.String"));
			newObjectsDataTable.Columns.Add(SecurityConstString.Protected, Type.GetType("System.Boolean"));
		}

		//---------------------------------------------------------------------------
		private void AddListViewColumns()
		{
			NewObjectsListView.Columns.Add(Strings.TitleColumn, -1, HorizontalAlignment.Left);
			NewObjectsListView.Columns.Add(Strings.TypeColumn, -1, HorizontalAlignment.Left);
			NewObjectsListView.Columns.Add(Strings.NameSpaceColumn, -1, HorizontalAlignment.Left);
		}

		//---------------------------------------------------------------------------
		private void AddRows()
		{
			ListViewItem item = null;

			foreach(MenuXmlNode node in newObjectsArrayList)
			{
				item = new ListViewItem();
				item.Checked = true;
				item.Text = node.Title;
                item.SubItems.Add(ControlsString.GetControlDescription(CommonObjectTreeFunction.GetSecurityNodeTypeFromCommandNode(node)));
				item.SubItems.Add(node.ItemObject);
				item.Tag = node;

				NewObjectsListView.Items.Add(item);

				AddRow((MenuXmlNode)item.Tag, sqlConnection);
			}
		}


		//---------------------------------------------------------------------------
		private void OkButton_Click(object sender, System.EventArgs e)
		{
			DeleteButton.Enabled = false;
			
			IMessageFilter aFilter = SecurityAdmin.DisableUserInteraction();
			
			foreach(ListViewItem item in NewObjectsListView.Items)
			{
				CommonObjectTreeFunction.WriteToDB((MenuXmlNode)item.Tag, sqlConnection);
				if (!item.Checked)
					continue;

				CommonObjectTreeFunction.ProtectObject(companyId, 
														Convert.ToInt32(CommonObjectTreeFunction.GetObjectId((MenuXmlNode)item.Tag, sqlConnection)),
														sqlConnection, false);

			//	AddRow((MenuXmlNode)item.Tag, sqlConnection);
			}


			if (this.CreateLogCheckBox.Checked)
				CreteLogFile();

			SecurityAdmin.RestoreUserInteraction(aFilter);
			
			MessageBox.Show(this, Strings.ProcedureEnded, Strings.ProtectedObject, 
							MessageBoxButtons.OK, MessageBoxIcon.Information);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		//---------------------------------------------------------------------------
		private void CreteLogFile()
		{
			DataSet ds = new DataSet("NewObjects");

			if (newObjectsDataTable == null)
				return;

			ds.Tables.Add(newObjectsDataTable);

			string date = DateTime.Now.ToString("s");

			date = date.Replace(":", "-");

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			string fileName= SecurityConstString.LogFileName + date + NameSolverStrings.XmlExtension;

			ds.WriteXml(Path.Combine(path, fileName));
		}


		//---------------------------------------------------------------------------
		private void ModifyChecState(bool state)
		{
			foreach(ListViewItem item in NewObjectsListView.Items)
				item.Checked = state;
		}

		//---------------------------------------------------------------------------
		private void NewObjectsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			ListViewItem item = (ListViewItem)NewObjectsListView.Items[e.Index];

			if (item == null)
				return;
			
			DataRow[] dr = newObjectsDataTable.Select(SecurityConstString.NameSpace+ "='" + item.SubItems[2].Text + "'" );
			
			if (dr == null || dr.Length == 0)
				return;

			if (e.NewValue == CheckState.Checked)
				dr[0][SecurityConstString.Protected] = true;
			else
				dr[0][SecurityConstString.Protected] = false;

			dr[0].AcceptChanges();
			
		}

		//---------------------------------------------------------------------------
		private void UnselectAllButton_Click(object sender, System.EventArgs e)
		{
			if(string.Compare(UnselectAllButton.Text, Strings.UnselectAll) == 0)
			{
				ModifyChecState(false);
				UnselectAllButton.Text = Strings.SelectAll;
			}
			else
			{
				ModifyChecState(true);
				UnselectAllButton.Text = Strings.UnselectAll;
			}
		}

		//---------------------------------------------------------------------
		private void NewObjectsListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			this.NewObjectsListView.ListViewItemSorter = new ListViewItemComparer(e.Column);
		}

	}

	/// <summary>
	/// ListViewItemComparer
	/// Utilizzata per l'ordinamento in una listView
	/// </summary>
	//=========================================================================
	public class ListViewItemComparer : IComparer
	{
		private int col;

		//---------------------------------------------------------------------------
		public ListViewItemComparer()
		{
			col = 0;
		}

		//---------------------------------------------------------------------------
		public ListViewItemComparer(int column)
		{
			col = column;
		}

		//---------------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
		}
	}
}
