using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	public class PageSqlDescription : WizardPage
	{
		private SqlDescriptionControl MyUC = new SqlDescriptionControl();

		public PageSqlDescription()
		{
			Name = "PageTableList";
			ParentChanged += new EventHandler(PChanged);

			Controls.Add(MyUC);
		}

		new private void PChanged(object sender, EventArgs e)
		{
			base.PChanged(sender, e);

			MyUC.Size = new System.Drawing.Size(Width, Height);
			MyUC.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));

			MyUC.ResizeControls();
			
		}

		public void SetParser(SqlParserUpdater parser)
		{
			MyUC.SetParser(parser);
		}

		public override bool Save()
		{
			return MyUC.Save() && base.Save();
		}
	}

	public class PagePrimaryConstraint : WizardPage
	{
		private PrimaryConstraintControl MyUC = new PrimaryConstraintControl();

		public PagePrimaryConstraint(SqlTable table)
		{
			Location = new System.Drawing.Point(4, 25);
			Name = "PagePrimaryConstraint";
			ParentChanged += new EventHandler(PChanged);
			MyUC.SetTable(table);

			InitControls();

			Enter += new EventHandler(MyUC.InitPrimaryConstraints);
			Leave += new EventHandler(MyUC.TabLeave);
		}

		new private void PChanged(object sender, EventArgs e)
		{
			Size = new System.Drawing.Size(Parent.Width - 8, Parent.Height - 29);

			MyUC.Size = new System.Drawing.Size(Width, Height);
			MyUC.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
		}

		private void InitControls()
		{
			MyUC.OnValidateControls += new Microarea.Library.SqlScriptUtilityControls.PrimaryConstraintControl.ValidateControlsHandler(MyUC_OnValidateControls);
			Controls.Add(MyUC);
		}

		private void MyUC_OnValidateControls(bool bResult)
		{
			ValidateControls(bResult);
		}
	}

	public class WizardPage : TabPage
	{
		public delegate void ValidateControlsHandler(bool bResult);
		
		public virtual event ValidateControlsHandler OnValidateControls;

		public WizardPage()
		{
			Location = new System.Drawing.Point(4, 25);
			ParentChanged += new EventHandler(PChanged);
		}

		public void PChanged(object sender, EventArgs e)
		{
			if (Parent == null)
				return;

			Size = new System.Drawing.Size(Parent.Width - 8, Parent.Height - 29);
		}

		public virtual bool Save()
		{
			return true;
		}

		public void ValidateControls(bool bResult)
		{
			if (OnValidateControls != null)
				OnValidateControls(bResult);
		}
	}

	public class ParsedDataTable : DataTable
	{
		private SqlTable table = null;
		public delegate void AddRowEventHandler(string defaultConstraintName);
		public event AddRowEventHandler OnAddRow;
		
		public delegate string UpdateDefaultConstraintEventHandler(string nTabella, string nColonna);
		public event UpdateDefaultConstraintEventHandler OnUpdateDefaultConstraint;
		
		public delegate void CreateRowEventHandler(SqlTable tabella, TableColumn colonna);
		public event CreateRowEventHandler OnCreateRow;
		
		public delegate void EditRowEventHandler(SqlTable tabella, TableColumn colonna);
		public event EditRowEventHandler OnEditRow;

		private bool allowLocalDelegate = true;
		private string oldValue = string.Empty;

		private const string NameDataColumnName					= "Name";
		private const string TypeDataColumnName					= "Type";
		private const string LengthDataColumnName				= "Length";
		private const string NullableDataColumnName				= "Nullable";
		private const string DefaultValueDataColumnName			= "DefaultValue";
		private const string DefaultConstraintDataColumnName	= "DefaultConstraint";
		private const string TableColumnDataColumnName			= "TableColumn";

		//-----------------------------------------------------------------------------
		public SqlTable Table { get { return table; } }

		//-----------------------------------------------------------------------------
		public ParsedDataTable(string aTableName, SqlTable aTable)
		{
			this.TableName = aTableName;

			DataColumn nameDataColumn = new DataColumn(NameDataColumnName, typeof(string));
			Columns.Add(nameDataColumn);

			DataColumn dataTypeDataColumn = new DataColumn(TypeDataColumnName, typeof(string));
			Columns.Add(dataTypeDataColumn);

			DataColumn dataLengthDataColumn  = new DataColumn(LengthDataColumnName, typeof(int));
			Columns.Add(dataLengthDataColumn );

			DataColumn nullableDataColumn = new DataColumn(NullableDataColumnName, typeof(bool));
			Columns.Add(nullableDataColumn);

			DataColumn defaultValueDataColumn  = new DataColumn(DefaultValueDataColumnName, typeof(string));
			Columns.Add(defaultValueDataColumn);

			DataColumn defaultConstraintDataColumn = new DataColumn(DefaultConstraintDataColumnName, typeof(string));
			Columns.Add(defaultConstraintDataColumn);

			DataColumn tableColumnDataColumn = new DataColumn(TableColumnDataColumnName, typeof(TableColumn), "", System.Data.MappingType.Hidden);
			Columns.Add(tableColumnDataColumn);

			table = aTable;

			ColumnChanged += new DataColumnChangeEventHandler(MyColumnChanged);
			ColumnChanging += new DataColumnChangeEventHandler(MyColumnChanging);
			RowDeleted += new DataRowChangeEventHandler(MyRowDeleted);
			RowDeleting += new DataRowChangeEventHandler(MyRowDeleting);
		}

		//-----------------------------------------------------------------------------
		public void Fill()
		{
			Rows.Clear();

			if (table == null || table.Columns == null)
				return;

			allowLocalDelegate = false;
			foreach (TableColumn aColumn in table.Columns)
			{
				DataRow dRow = NewRow();

				dRow[NameDataColumnName] = aColumn.Name;
				dRow[TypeDataColumnName] = aColumn.DataType;

				switch (aColumn.DataType)
				{
					case "char":
						dRow[LengthDataColumnName] = aColumn.DataLength;
						break;
					case "varchar":
						dRow[LengthDataColumnName] = aColumn.DataLength;
						break;
					default:
						dRow[LengthDataColumnName] = 0;
						break;
				}

				dRow[NullableDataColumnName] = aColumn.IsNullable;
				dRow[DefaultValueDataColumnName] = aColumn.DefaultValue;
				dRow[TableColumnDataColumnName] = aColumn;
				dRow[DefaultConstraintDataColumnName] = aColumn.DefaultConstraintName;
				
				if (OnAddRow != null)
					OnAddRow(aColumn.DefaultConstraintName);
				
				Rows.Add(dRow);
			}

			AddEmptyRow();

			allowLocalDelegate = true;
		}

		//-----------------------------------------------------------------------------
		public void AggiungiRiga(string defaultConstraintName)
		{
			if (OnAddRow != null)
				OnAddRow(defaultConstraintName);
		}

		//-----------------------------------------------------------------------------
		private void AddEmptyRow()
		{
			DataRow emptyRow = NewRow();

			emptyRow[NameDataColumnName] = SqlScriptUtilityControlsStrings.NewFieldNameItem;
			emptyRow[TypeDataColumnName] = string.Empty;
			emptyRow[LengthDataColumnName] = "0";
			emptyRow[NullableDataColumnName] = false;
			emptyRow[DefaultValueDataColumnName] = string.Empty;
			emptyRow[TableColumnDataColumnName] = DBNull.Value;
			emptyRow[DefaultConstraintDataColumnName] = string.Empty;
				
			Rows.Add(emptyRow);
		}

		//-----------------------------------------------------------------------------
		public void UpdateRow(DataRow aRow)
		{
			if (aRow == null || aRow[TableColumnDataColumnName] == DBNull.Value)
				return;

			TableColumn tableColumn = (TableColumn)aRow[TableColumnDataColumnName];
			if (tableColumn == null)
				return;

			allowLocalDelegate = false;

			aRow[NameDataColumnName] = tableColumn.Name;
			aRow[TypeDataColumnName] = tableColumn.DataType;

			switch (tableColumn.DataType)
			{
				case "char":
					aRow[LengthDataColumnName] = tableColumn.DataLength;
					break;
				case "varchar":
					aRow[LengthDataColumnName] = tableColumn.DataLength;
					break;
				default:
					aRow[LengthDataColumnName] = 0;
					break;
			}

			aRow[NullableDataColumnName] = tableColumn.IsNullable;
			aRow[DefaultValueDataColumnName] = tableColumn.DefaultValue;
			aRow[DefaultConstraintDataColumnName] = tableColumn.DefaultConstraintName;
			if (OnAddRow != null)
				OnAddRow(tableColumn.DefaultConstraintName);

			if (tableColumn.IsNew)
			{
				if (!tableColumn.IsChanging)
				{
					AddEmptyRow();
					if (OnCreateRow != null)
						OnCreateRow(table, tableColumn);
					table.Columns.Add(tableColumn);
				}
			}
			else
			{
				if (OnEditRow != null)
					OnEditRow(table, tableColumn);
			}
				
			allowLocalDelegate = true;
		}

		//-----------------------------------------------------------------------------
		private void MyColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			if (!allowLocalDelegate)
				return;

			switch (e.Column.ColumnName)
			{
				case NameDataColumnName:
					ChangeColumnName(e.Row);
					break;
				case DefaultValueDataColumnName:
					SetDefaultConstraintName(e.Row);
					break;
				case TableColumnDataColumnName:
					UpdateRow(e.Row);
					break;
			}
		}

		//-----------------------------------------------------------------------------
		private void ChangeColumnName(DataRow aRow)
		{
			if (oldValue == string.Empty)
				AddColumn(aRow);
			else if (aRow != null && aRow[TableColumnDataColumnName] != DBNull.Value)
				((TableColumn)aRow[TableColumnDataColumnName]).Rename(aRow[NameDataColumnName].ToString());
		}

		//-----------------------------------------------------------------------------
		private void AddColumn(DataRow aRow)
		{
			if (aRow == null || aRow[NameDataColumnName] == DBNull.Value || table == null)
				return;

			string columnName = (string)aRow[NameDataColumnName];

			TableColumn column = table.GetColumn(columnName);
			if (column == null)
				return;

			table.AddColumn(columnName);
			aRow[TableColumnDataColumnName] = column;
		}

		//-----------------------------------------------------------------------------
		private void SetDefaultConstraintName(DataRow aRow)
		{
			if (aRow == null || aRow[DefaultConstraintDataColumnName] == DBNull.Value)
				return;
			
			string defaultConstraintName = (string)aRow[DefaultConstraintDataColumnName];

			if 
				(
				(defaultConstraintName == null || defaultConstraintName.Length == 0) &&
				aRow.Table != null &&
				aRow[NameDataColumnName] != DBNull.Value
				)
			{
				string tableName = aRow.Table.TableName;
				string columnName = (string)aRow[NameDataColumnName];

				if (OnUpdateDefaultConstraint != null)
					aRow[DefaultConstraintDataColumnName] = OnUpdateDefaultConstraint(tableName, columnName);
				else
					aRow[DefaultConstraintDataColumnName] = String.Empty;
			}
		}

		//-----------------------------------------------------------------------------
		private void MyColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			if (!allowLocalDelegate)
				return;

			oldValue = e.Row[e.Column.ColumnName].ToString();
		}

		//-----------------------------------------------------------------------------
		private void MyRowDeleting(object sender, DataRowChangeEventArgs e)
		{
			if (!allowLocalDelegate)
				return;

			oldValue = e.Row[NameDataColumnName].ToString();
		}

		//-----------------------------------------------------------------------------
		private void MyRowDeleted(object sender, DataRowChangeEventArgs e)
		{
			if (!allowLocalDelegate)
				return;

			if (oldValue != null && oldValue.Length > 0)
				table.DeleteColumn(oldValue);
		}

		//-----------------------------------------------------------------------------
		public bool TestColumnName(string colName)
		{
			if (table== null || table.GetColumn(colName) != null)
				return false;

			return true;
		}
	}
}
