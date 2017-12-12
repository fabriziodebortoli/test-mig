using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.Library.TBWizardProjects;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Library.DBObjects;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.SqlScriptUtility;
using Microarea.Library.DataStructureUtils;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Library.DataStructureUtils.UI
{
    public partial class DBObjectsDefControl : UserControl
    {
        #region private constant strings

        private const string DBObjectsDataSetName = "DBObjects";
        private const string DBObjectDefsTableName = "DBObjectDefs";
        private const string DBObjectTypeColumnName = "DBObjectName";
        private const string TableDefinitionTableName = "TableDefinition";
        private const string TableNameColumnName = "TableName";
        private const string TableTbNameSpaceColumnName = "TbNameSpace";
        private const string TableCreationDbReleaseNumberColumnName = "TableCreationDbReleaseNumber";
        private const string TableColumnDefinitionTableName = "TableColumnDefinition";
        private const string ViewDefinitionTableName = "ViewDefinition";
        private const string ViewNameColumnName = "ViewName";
        private const string ViewTbNameSpaceColumnName = "TbNameSpace";
        private const string ViewCreationDbReleaseNumberColumnName = "TableCreationDbReleaseNumber";
        private const string ViewColumnDefinitionTableName = "ViewColumnDefinition";
        private const string ColumnNameColumnName = "ColumnName";
        private const string ColumnDataTypeColumnName = "ColumnDataType";
        private const string ColumnDataLengthColumnName = "ColumnDataLength";
        private const string ColumnDefaultValueColumnName = "ColumnDefaultValue";
        private const string ColumnIsCollateSensitiveColumnName = "IsCollateSensitive";
        private const string StoredProcedureDefinitionTableName = "StoredProcedureDefinition";
        private const string ProcedureNameColumnName = "ProcedureName";
        private const string ProcedureTbNameSpaceColumnName = "TbNameSpace";
        private const string ProcedureCreationDbReleaseNumberColumnName = "ProcedureCreationDbReleaseNumber";
        private const string StoredProcedureParameterDefinitionTableName = "StoredProcedureParameterDefinition";
        private const string ProcedureParameterNameColumnName = "ProcedureParameterName";
        private const string ProcedureParameterDataTypeColumnName = "ProcedureParameterDataType";
        private const string ProcedureParameterDataLengthColumnName = "ProcedureParameterDataLength";
        private const string ProcedureParameterIsOutColumnName = "ProcedureParameterIsOut";
        private const string ProcedureParameterIsCollateSensitiveColumnName = "ProcedureParameterIsCollateSensitive";
        private const string TableExtraAddedColumnsDefinitionTableName = "TableExtraAddedColumnsDefinition";
        private const string ExtraAddedColumnDefinitionTableName = "ExtraAddedColumnDefinition";
        private const string TableDBObjectsRelationName = "Tables";
        private const string TableColumnsRelationName = "TableColumns";
        private const string ViewDBObjectsRelationName = "Views";
        private const string ViewColumnsRelationName = "ViewColumns";
        private const string StoredProcedureDBObjectsRelationName = "StoredProcedures";
        private const string StoredProcedureParametersRelationName = "StoredProcedureParameters";
        private const string TableAddOnColsDBObjectsRelationName = "TableAddOnColumns";
        private const string AddOnColumnRelationName = "AddOnColumn";

        #endregion // private constant strings

        private BindingSource masterBindingSource = null;
        private BindingSource detailsBindingSource = null;

        private bool isInEditMode = false;
        private bool showEditModeButton = true;

        private enum DBObjectType : ushort
        {
            Undefined = 0x0000,
            Table = 0x0001,
            View = 0x0002,
            StoredProcedure = 0x0003,
            ExtraAddedColumns = 0x0004
        }

        public event EventHandler EditModeChanged = null;

        //--------------------------------------------------------------------------------------------------------------------------------
        private static class DataObjectTypeEnumHelper
        {
            public static string GetGenreDescription(DBObjectType aDataObjectType)
            {
                switch (aDataObjectType)
                {
                    case DBObjectType.Table:
                        return Strings.TableDataObjectTypeDescription;
                    case DBObjectType.View:
                        return Strings.ViewDataObjectTypeDescription;
                    case DBObjectType.StoredProcedure:
                        return Strings.StoredProcedureDataObjectTypeDescription;
                    case DBObjectType.ExtraAddedColumns:
                        return Strings.ExtraAddedColumnsDataObjectTypeDescription;
                    default:
                        return String.Empty;
                }
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            public static DBObjectType GetDataObjectTypeFromGenreDescription(string aDataObjectTypeDescription)
            {
                if (String.Compare(aDataObjectTypeDescription, Strings.TableDataObjectTypeDescription) == 0)
                    return DBObjectType.Table;
                if (String.Compare(aDataObjectTypeDescription, Strings.ViewDataObjectTypeDescription) == 0)
                    return DBObjectType.View;
                if (String.Compare(aDataObjectTypeDescription, Strings.StoredProcedureDataObjectTypeDescription) == 0)
                    return DBObjectType.StoredProcedure;
                if (String.Compare(aDataObjectTypeDescription, Strings.ExtraAddedColumnsDataObjectTypeDescription) == 0)
                    return DBObjectType.ExtraAddedColumns;

                return DBObjectType.Undefined;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DBObjectsDefControl()
        {
            InitializeComponent();

            InitDataSource();

            UpdateDataStructureGridViewColumns();

            this.ShowDefsTypeComboBox.Items.Add(DataObjectTypeEnumHelper.GetGenreDescription(DBObjectType.Table));
            this.ShowDefsTypeComboBox.Items.Add(DataObjectTypeEnumHelper.GetGenreDescription(DBObjectType.View));
            this.ShowDefsTypeComboBox.Items.Add(DataObjectTypeEnumHelper.GetGenreDescription(DBObjectType.StoredProcedure));
            this.ShowDefsTypeComboBox.Items.Add(DataObjectTypeEnumHelper.GetGenreDescription(DBObjectType.ExtraAddedColumns));

            this.IsInEditMode = false;
        }

        #region DBObjectsDefControl private methods

        //--------------------------------------------------------------------------------------------------------------------------------
        private void InitDataSource()
        {
            if (this.DesignMode)
                return;

            this.DataStructureGrid.DataSource = null;
            
            DataSet dbObjectDefsDataSet = new DataSet(DBObjectsDataSetName);

            DataTable dbObjectDefsTable = CreateDBObjectDefsTable();
            dbObjectDefsDataSet.Tables.Add(dbObjectDefsTable);

            // Tables
            DataRow tableDBObjectDefRow = dbObjectDefsTable.NewRow();
            tableDBObjectDefRow[DBObjectTypeColumnName] = DBObjectType.Table;
            dbObjectDefsTable.Rows.Add(tableDBObjectDefRow);

            DataTable tableDefTable = CreateTableDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(tableDefTable);
            DataRelation tableDBObjectDefsRelation = new DataRelation
                                                   (
                                                   TableDBObjectsRelationName,
                                                   dbObjectDefsTable.Columns[DBObjectTypeColumnName],
                                                   tableDefTable.Columns[DBObjectTypeColumnName]
                                                   );
            dbObjectDefsDataSet.Relations.Add(tableDBObjectDefsRelation);
            DataTable columnDefTable = CreateTableColumnDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(columnDefTable);
            DataRelation tableColumnsRelation = new DataRelation
                                                    (
                                                    TableColumnsRelationName,
                                                    tableDefTable.Columns[TableNameColumnName],
                                                    columnDefTable.Columns[TableNameColumnName]
                                                    );
            dbObjectDefsDataSet.Relations.Add(tableColumnsRelation);

            // Views
            DataRow viewDBObjectDefRow = dbObjectDefsTable.NewRow();
            viewDBObjectDefRow[DBObjectTypeColumnName] = DBObjectType.View;
            dbObjectDefsTable.Rows.Add(viewDBObjectDefRow);

            DataTable viewDefTable = CreateViewDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(viewDefTable);
            DataRelation viewDBObjectDefsRelation = new DataRelation
                                                   (
                                                   ViewDBObjectsRelationName,
                                                   dbObjectDefsTable.Columns[DBObjectTypeColumnName],
                                                   viewDefTable.Columns[DBObjectTypeColumnName]
                                                   );
            dbObjectDefsDataSet.Relations.Add(viewDBObjectDefsRelation);
            DataTable viewColumnDefTable = CreateViewColumnDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(viewColumnDefTable);
            DataRelation viewColumnsRelation = new DataRelation
                                                   (
                                                   ViewColumnsRelationName,
                                                   viewDefTable.Columns[ViewNameColumnName],
                                                   viewColumnDefTable.Columns[ViewNameColumnName]
                                                   );
            dbObjectDefsDataSet.Relations.Add(viewColumnsRelation);

            // Stored Procedures
            DataRow storedProcDBObjectDefRow = dbObjectDefsTable.NewRow();
            storedProcDBObjectDefRow[DBObjectTypeColumnName] = DBObjectType.StoredProcedure;
            dbObjectDefsTable.Rows.Add(storedProcDBObjectDefRow);

            DataTable storedProcedureDefTable = CreateStoredProcedureDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(storedProcedureDefTable);
            DataRelation storedProcedureDBObjectDefsRelation = new DataRelation
                                                                   (
                                                                   StoredProcedureDBObjectsRelationName,
                                                                   dbObjectDefsTable.Columns[DBObjectTypeColumnName],
                                                                   storedProcedureDefTable.Columns[DBObjectTypeColumnName]
                                                                   );
            dbObjectDefsDataSet.Relations.Add(storedProcedureDBObjectDefsRelation);
            DataTable storedProcedureParameterDefTable = CreateStoredProcedureParameterDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(storedProcedureParameterDefTable);
            DataRelation storedProcedureParametersRelation = new DataRelation
                                                                 (
                                                                 StoredProcedureParametersRelationName,
                                                                 storedProcedureDefTable.Columns[ProcedureNameColumnName],
                                                                 storedProcedureParameterDefTable.Columns[ProcedureNameColumnName]
                                                                 );
            dbObjectDefsDataSet.Relations.Add(storedProcedureParametersRelation);

            // AddOnColumns
            DataRow addOnColsDBObjectDefRow = dbObjectDefsTable.NewRow();
            addOnColsDBObjectDefRow[DBObjectTypeColumnName] = DBObjectType.ExtraAddedColumns;
            dbObjectDefsTable.Rows.Add(addOnColsDBObjectDefRow);

            DataTable tableAddOnColsDefTable = CreateTableExtraAddedColumnsDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(tableAddOnColsDefTable);
            DataRelation addOnColsDBObjectDefsRelation = new DataRelation
                                                             (
                                                             TableAddOnColsDBObjectsRelationName,
                                                             dbObjectDefsTable.Columns[DBObjectTypeColumnName],
                                                             tableAddOnColsDefTable.Columns[DBObjectTypeColumnName]
                                                             );
            dbObjectDefsDataSet.Relations.Add(addOnColsDBObjectDefsRelation);
            DataTable addOnColumnDefTable = CreateExtraAddedColumnDefinitionTable();
            dbObjectDefsDataSet.Tables.Add(addOnColumnDefTable);
            DataRelation addOnColumnRelation = new DataRelation
                                    (
                                    AddOnColumnRelationName,
                                    tableAddOnColsDefTable.Columns[TableNameColumnName],
                                    addOnColumnDefTable.Columns[TableNameColumnName]
                                    );
            dbObjectDefsDataSet.Relations.Add(addOnColumnRelation);
           

            masterBindingSource = new BindingSource();
            masterBindingSource.DataSource = dbObjectDefsDataSet;

            detailsBindingSource = new BindingSource();
            detailsBindingSource.DataSource = masterBindingSource;
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateDBObjectDefsTable()
        {
            DataTable dbObjectDefsTable = new DataTable(DBObjectDefsTableName);
            dbObjectDefsTable.Columns.Add(DBObjectTypeColumnName, typeof(DBObjectType));

            return dbObjectDefsTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateTableDefinitionTable()
        {
            DataTable tableDefTable = new DataTable(TableDefinitionTableName);
            tableDefTable.Columns.Add(DBObjectTypeColumnName, typeof(DBObjectType));
            tableDefTable.Columns.Add(TableNameColumnName, typeof(string));
            tableDefTable.Columns.Add(TableTbNameSpaceColumnName, typeof(string));
            tableDefTable.Columns.Add(TableCreationDbReleaseNumberColumnName, typeof(uint));
            tableDefTable.PrimaryKey = new DataColumn[] { tableDefTable.Columns[TableNameColumnName] };

            return tableDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateTableColumnDefinitionTable()
        {
            DataTable columnDefTable = new DataTable(TableColumnDefinitionTableName);
            columnDefTable.Columns.Add(TableNameColumnName, typeof(string));
            columnDefTable.Columns.Add(ColumnNameColumnName, typeof(string));
            columnDefTable.Columns.Add(ColumnDataTypeColumnName, typeof(WizardTableColumnDataType.DataType));
            columnDefTable.Columns.Add(ColumnDataLengthColumnName, typeof(uint));
            columnDefTable.Columns.Add(ColumnDefaultValueColumnName, typeof(object));
            columnDefTable.PrimaryKey = new DataColumn[] { columnDefTable.Columns[TableNameColumnName], columnDefTable.Columns[ColumnNameColumnName] };

            return columnDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillTablesData(WizardTableInfo[] tableDefs)
        {
            if (tableDefs == null || tableDefs.Length == 0)
                return;

            DataSet dbObjectDefsDataSet = masterBindingSource.DataSource as DataSet;
            if (dbObjectDefsDataSet == null)
                throw new Exception("BindingSource is null or invalid.");

            DataTable tableDefTable = dbObjectDefsDataSet.Tables[TableDefinitionTableName];
            if (tableDefTable == null)
                return;

            DataTable columnDefTable = dbObjectDefsDataSet.Tables[TableColumnDefinitionTableName];

            foreach (WizardTableInfo aTableDef in tableDefs)
            {
                DataRow aTableDefRow = tableDefTable.NewRow();
                aTableDefRow[DBObjectTypeColumnName] = DBObjectType.Table;
                aTableDefRow[TableNameColumnName] = aTableDef.Name;
                aTableDefRow[TableTbNameSpaceColumnName] = aTableDef.TbNameSpace;
                aTableDefRow[TableCreationDbReleaseNumberColumnName] = aTableDef.CreationDbReleaseNumber;
                tableDefTable.Rows.Add(aTableDefRow);

                if (columnDefTable != null && aTableDef.ColumnsInfo != null && aTableDef.ColumnsCount > 0)
                {
                    foreach (WizardTableColumnInfo aColumnInfo in aTableDef.ColumnsInfo)
                    {
                        DataRow aColumnDefRow = columnDefTable.NewRow();
                        aColumnDefRow[TableNameColumnName] = aTableDef.Name;
                        aColumnDefRow[ColumnNameColumnName] = aColumnInfo.Name;
                        aColumnDefRow[ColumnDataTypeColumnName] = aColumnInfo.DataType.Type;
                        if (WizardTableColumnDataType.IsTextualDataType((WizardTableColumnDataType.DataType)aColumnDefRow[ColumnDataTypeColumnName]))
                            aColumnDefRow[ColumnDataLengthColumnName] = aColumnInfo.DataLength;
                        aColumnDefRow[ColumnDefaultValueColumnName] = aColumnInfo.DefaultValue;

                        columnDefTable.Rows.Add(aColumnDefRow);
                    }
                }
            }
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateViewDefinitionTable()
        {
            DataTable viewDefTable = new DataTable(ViewDefinitionTableName);
            viewDefTable.Columns.Add(DBObjectTypeColumnName, typeof(DBObjectType));
            viewDefTable.Columns.Add(ViewNameColumnName, typeof(string));
            viewDefTable.Columns.Add(ViewTbNameSpaceColumnName, typeof(string));
            viewDefTable.Columns.Add(ViewCreationDbReleaseNumberColumnName, typeof(uint));
            viewDefTable.PrimaryKey = new DataColumn[] { viewDefTable.Columns[TableNameColumnName] };

            return viewDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateViewColumnDefinitionTable()
        {
            DataTable viewColumnDefTable = new DataTable(ViewColumnDefinitionTableName);
            viewColumnDefTable.Columns.Add(ViewNameColumnName, typeof(string));
            viewColumnDefTable.Columns.Add(ColumnNameColumnName, typeof(string));
            viewColumnDefTable.Columns.Add(ColumnDataTypeColumnName, typeof(WizardTableColumnDataType.DataType));
            viewColumnDefTable.Columns.Add(ColumnDataLengthColumnName, typeof(uint));
            viewColumnDefTable.Columns.Add(ColumnIsCollateSensitiveColumnName, typeof(bool));
            viewColumnDefTable.PrimaryKey = new DataColumn[] { viewColumnDefTable.Columns[ViewNameColumnName], viewColumnDefTable.Columns[ColumnNameColumnName] };

            return viewColumnDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillViewsData(SqlView[] viewDefs)
        {
            if (viewDefs == null || viewDefs.Length == 0)
                return;

            DataSet dbObjectDefsDataSet = masterBindingSource.DataSource as DataSet;
            if (dbObjectDefsDataSet == null)
                throw new Exception("BindingSource is null or invalid.");
            
            DataTable viewDefTable = dbObjectDefsDataSet.Tables[ViewDefinitionTableName];
            if (viewDefTable == null)
                return;
            
            DataTable viewColumnDefTable = dbObjectDefsDataSet.Tables[ViewColumnDefinitionTableName];

            foreach (SqlView aViewDef in viewDefs)
            {
                DataRow aViewDefRow = viewDefTable.NewRow();
                aViewDefRow[DBObjectTypeColumnName] = DBObjectType.View;
                aViewDefRow[ViewNameColumnName] = aViewDef.Name;
                aViewDefRow[ViewTbNameSpaceColumnName] = aViewDef.TbNameSpace;
                aViewDefRow[ViewCreationDbReleaseNumberColumnName] = aViewDef.CreationDbReleaseNumber;
                viewDefTable.Rows.Add(aViewDefRow);

                if (viewColumnDefTable != null && aViewDef.Columns != null && aViewDef.Columns.Count > 0)
                {
                    foreach (ViewColumn aViewColumn in aViewDef.Columns)
                    {
                        DataRow aViewColumnDefRow = viewColumnDefTable.NewRow();
                        aViewColumnDefRow[ViewNameColumnName] = aViewDef.Name;
                        aViewColumnDefRow[ColumnNameColumnName] = aViewColumn.Name;

                        WizardTableColumnDataType.DataType dataType = WizardTableColumnDataType.DataType.Undefined;
                        try
                        {
                            if (!String.IsNullOrEmpty(aViewColumn.DataType))
                                dataType = (WizardTableColumnDataType.DataType)Enum.Parse(typeof(WizardTableColumnDataType.DataType), aViewColumn.DataType, true);
                        }
                        catch (ArgumentException ex)
                        {
                            System.Diagnostics.Debug.Fail(ex.Message);
                            dataType = WizardTableColumnDataType.DataType.Undefined;
                        }
                        aViewColumnDefRow[ColumnDataTypeColumnName] = dataType;
                        if (WizardTableColumnDataType.IsTextualDataType(dataType))
                            aViewColumnDefRow[ColumnDataLengthColumnName] = aViewColumn.DataLength;

                        aViewColumnDefRow[ColumnDataLengthColumnName] = aViewColumn.DataLength;
                        aViewColumnDefRow[ColumnIsCollateSensitiveColumnName] = aViewColumn.IsCollateSensitive;

                        viewColumnDefTable.Rows.Add(aViewColumnDefRow);
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateStoredProcedureDefinitionTable()
        {
            DataTable storedProcedureDefTable = new DataTable(StoredProcedureDefinitionTableName);
            storedProcedureDefTable.Columns.Add(DBObjectTypeColumnName, typeof(DBObjectType));
            storedProcedureDefTable.Columns.Add(ProcedureNameColumnName, typeof(string));
            storedProcedureDefTable.Columns.Add(ProcedureTbNameSpaceColumnName, typeof(string));
            storedProcedureDefTable.Columns.Add(ProcedureCreationDbReleaseNumberColumnName, typeof(uint));
            storedProcedureDefTable.PrimaryKey = new DataColumn[] { storedProcedureDefTable.Columns[ProcedureNameColumnName] };

            return storedProcedureDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateStoredProcedureParameterDefinitionTable()
        {
            DataTable storedProcedureParameterDefTable = new DataTable(StoredProcedureParameterDefinitionTableName);
            storedProcedureParameterDefTable.Columns.Add(ProcedureNameColumnName, typeof(string));
            storedProcedureParameterDefTable.Columns.Add(ProcedureParameterNameColumnName, typeof(string));
            storedProcedureParameterDefTable.Columns.Add(ProcedureParameterDataTypeColumnName, typeof(WizardTableColumnDataType.DataType));
            storedProcedureParameterDefTable.Columns.Add(ProcedureParameterDataLengthColumnName, typeof(uint));
            storedProcedureParameterDefTable.Columns.Add(ProcedureParameterIsOutColumnName, typeof(bool));
            storedProcedureParameterDefTable.Columns.Add(ProcedureParameterIsCollateSensitiveColumnName, typeof(bool));
            storedProcedureParameterDefTable.PrimaryKey = new DataColumn[] { storedProcedureParameterDefTable.Columns[ProcedureNameColumnName], storedProcedureParameterDefTable.Columns[ProcedureParameterNameColumnName] };

            return storedProcedureParameterDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillStoredProceduresData(SqlProcedure[] storedProcDefs)
        {
            if (storedProcDefs == null || storedProcDefs.Length == 0)
                return;

            DataSet dbObjectDefsDataSet = masterBindingSource.DataSource as DataSet;
            if (dbObjectDefsDataSet == null)
                throw new Exception("BindingSource is null or invalid.");

            DataTable storedProcedureDefTable = dbObjectDefsDataSet.Tables[StoredProcedureDefinitionTableName];
            if (storedProcedureDefTable == null)
                return;

            DataTable storedProcedureParameterDefTable = dbObjectDefsDataSet.Tables[StoredProcedureParameterDefinitionTableName];

            foreach (SqlProcedure aStoredProcDef in storedProcDefs)
            {
                DataRow aStoredProcedureDefRow = storedProcedureDefTable.NewRow();
                aStoredProcedureDefRow[DBObjectTypeColumnName] = DBObjectType.StoredProcedure;
                aStoredProcedureDefRow[ProcedureNameColumnName] = aStoredProcDef.Name;
                aStoredProcedureDefRow[ProcedureTbNameSpaceColumnName] = aStoredProcDef.TbNameSpace;
                aStoredProcedureDefRow[ProcedureCreationDbReleaseNumberColumnName] = aStoredProcDef.CreationDbReleaseNumber;
                storedProcedureDefTable.Rows.Add(aStoredProcedureDefRow);

                if (storedProcedureParameterDefTable != null && aStoredProcDef.Parameters != null && aStoredProcDef.Parameters.Count > 0)
                {
                    foreach (ProcedureParameter aStoredProcParamDef in aStoredProcDef.Parameters)
                    {
                        DataRow aStoredProcParamDefRow = storedProcedureParameterDefTable.NewRow();
                        aStoredProcParamDefRow[ProcedureNameColumnName] = aStoredProcDef.Name;
                        aStoredProcParamDefRow[ProcedureParameterNameColumnName] = aStoredProcParamDef.Name;
                        WizardTableColumnDataType.DataType dataType = WizardTableColumnDataType.DataType.Undefined;
                        try
                        {
                            if (!String.IsNullOrEmpty(aStoredProcParamDef.DataType))
                                dataType = (WizardTableColumnDataType.DataType)Enum.Parse(typeof(WizardTableColumnDataType.DataType), aStoredProcParamDef.DataType, true);
                        }
                        catch (ArgumentException ex)
                        {
                            System.Diagnostics.Debug.Fail(ex.Message);
                            dataType = WizardTableColumnDataType.DataType.Undefined;
                        }
                        aStoredProcParamDefRow[ProcedureParameterDataTypeColumnName] = dataType;
                        if (WizardTableColumnDataType.IsTextualDataType(dataType))
                            aStoredProcParamDefRow[ProcedureParameterDataLengthColumnName] = aStoredProcParamDef.DataLength;

                        aStoredProcParamDefRow[ProcedureParameterIsOutColumnName] = aStoredProcParamDef.IsOut;
                        aStoredProcParamDefRow[ProcedureParameterIsCollateSensitiveColumnName] = aStoredProcParamDef.IsCollateSensitive;

                        storedProcedureParameterDefTable.Rows.Add(aStoredProcParamDefRow);
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateTableExtraAddedColumnsDefinitionTable()
        {
            DataTable extraAddedColumnsDefTable = new DataTable(TableExtraAddedColumnsDefinitionTableName);
            extraAddedColumnsDefTable.Columns.Add(DBObjectTypeColumnName, typeof(DBObjectType));
            extraAddedColumnsDefTable.Columns.Add(TableNameColumnName, typeof(string));
            extraAddedColumnsDefTable.PrimaryKey = new DataColumn[] { extraAddedColumnsDefTable.Columns[TableNameColumnName] };

            return extraAddedColumnsDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DataTable CreateExtraAddedColumnDefinitionTable()
        {
            DataTable extraAddedColumnDefTable = new DataTable(ExtraAddedColumnDefinitionTableName);
            extraAddedColumnDefTable.Columns.Add(TableNameColumnName, typeof(string));
            extraAddedColumnDefTable.Columns.Add(ColumnNameColumnName, typeof(string));
            extraAddedColumnDefTable.Columns.Add(ColumnDataTypeColumnName, typeof(WizardTableColumnDataType.DataType));
            extraAddedColumnDefTable.Columns.Add(ColumnDataLengthColumnName, typeof(uint));
            extraAddedColumnDefTable.Columns.Add(ColumnDefaultValueColumnName, typeof(object));
            extraAddedColumnDefTable.PrimaryKey = new DataColumn[] { extraAddedColumnDefTable.Columns[TableNameColumnName], extraAddedColumnDefTable.Columns[ColumnNameColumnName] };

            return extraAddedColumnDefTable;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillExtraAddedColumnsData(WizardExtraAddedColumnsInfo[] extraAddedColumnsDefs)
        {
            if (extraAddedColumnsDefs == null || extraAddedColumnsDefs.Length == 0)
                return;

            DataSet dbObjectDefsDataSet = masterBindingSource.DataSource as DataSet;
            if (dbObjectDefsDataSet == null)
                throw new Exception("BindingSource is null or invalid.");

            DataTable extraAddedColumnsDefTable = dbObjectDefsDataSet.Tables[TableExtraAddedColumnsDefinitionTableName];
            if (extraAddedColumnsDefTable == null)
                return;

            DataTable addedColumnDefTable = dbObjectDefsDataSet.Tables[ExtraAddedColumnDefinitionTableName];

            foreach (WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumnsDefs)
            {
                DataRow tableRow = extraAddedColumnsDefTable.Rows.Find(aExtraAddedColumnsInfo.TableName);
                if (tableRow == null)
                {
                    tableRow = extraAddedColumnsDefTable.NewRow();
                    tableRow[TableNameColumnName] = aExtraAddedColumnsInfo.TableName;
                    extraAddedColumnsDefTable.Rows.Add(tableRow);
                }
                if (addedColumnDefTable != null && aExtraAddedColumnsInfo.ColumnsInfo != null && aExtraAddedColumnsInfo.ColumnsCount > 0)
                {
                    foreach (WizardTableColumnInfo aColumnInfo in aExtraAddedColumnsInfo.ColumnsInfo)
                    {
                        DataRow aColumnDefRow = addedColumnDefTable.NewRow();
                        aColumnDefRow[TableNameColumnName] = aExtraAddedColumnsInfo.TableName;
                        aColumnDefRow[ColumnNameColumnName] = aColumnInfo.Name;
                        aColumnDefRow[ColumnDataTypeColumnName] = aColumnInfo.DataType.Type;
                        if (WizardTableColumnDataType.IsTextualDataType((WizardTableColumnDataType.DataType)aColumnDefRow[ColumnDataTypeColumnName]))
                            aColumnDefRow[ColumnDataLengthColumnName] = aColumnInfo.DataLength;
                        aColumnDefRow[ColumnDefaultValueColumnName] = aColumnInfo.DefaultValue;

                        addedColumnDefTable.Rows.Add(aColumnDefRow);
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void UpdateDataStructureGridViewColumns()
        {
            this.DataStructureGrid.Columns.Clear();

            if (this.DataStructureGrid.DataSource == null)
                return;

            DBObjectType selDataObjectType = this.SelectedDBObjectType();
            if (selDataObjectType == DBObjectType.Undefined)
            {
                this.DataStructureGrid.AutoGenerateColumns = true;
                return;
            }

            if (selDataObjectType == DBObjectType.Table)
            {
                this.DataStructureGrid.AutoGenerateColumns = false;

                DataGridViewTextBoxColumn tableNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                tableNameDataGridViewColumn.HeaderText = Strings.TableNameDataGridColumnHeaderText;
                tableNameDataGridViewColumn.DataPropertyName = TableNameColumnName;
                this.DataStructureGrid.Columns.Add(tableNameDataGridViewColumn);

                DataGridViewTextBoxColumn tableTbNameSpaceDataGridViewColumn = new DataGridViewTextBoxColumn();
                tableTbNameSpaceDataGridViewColumn.HeaderText = Strings.TbNameSpaceDataGridColumnHeaderText;
                tableTbNameSpaceDataGridViewColumn.DataPropertyName = TableTbNameSpaceColumnName;
                this.DataStructureGrid.Columns.Add(tableTbNameSpaceDataGridViewColumn);

                DataGridViewTextBoxColumn tableCreationDbReleaseNumberDataGridViewColumn = new DataGridViewTextBoxColumn();
                tableCreationDbReleaseNumberDataGridViewColumn.HeaderText = Strings.CreationDbReleaseNumberDataGridColumnHeaderText;
                tableCreationDbReleaseNumberDataGridViewColumn.DataPropertyName = TableCreationDbReleaseNumberColumnName;
                this.DataStructureGrid.Columns.Add(tableCreationDbReleaseNumberDataGridViewColumn);
            }
            else if (selDataObjectType == DBObjectType.View)
            {
                this.DataStructureGrid.AutoGenerateColumns = false;

                DataGridViewTextBoxColumn viewNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                viewNameDataGridViewColumn.HeaderText = Strings.ViewNameDataGridColumnHeaderText;
                viewNameDataGridViewColumn.DataPropertyName = ViewNameColumnName;
                this.DataStructureGrid.Columns.Add(viewNameDataGridViewColumn);

                DataGridViewTextBoxColumn viewTbNameSpaceDataGridViewColumn = new DataGridViewTextBoxColumn();
                viewTbNameSpaceDataGridViewColumn.HeaderText = Strings.TbNameSpaceDataGridColumnHeaderText;
                viewTbNameSpaceDataGridViewColumn.DataPropertyName = ViewTbNameSpaceColumnName;
                this.DataStructureGrid.Columns.Add(viewTbNameSpaceDataGridViewColumn);

                DataGridViewTextBoxColumn viewCreationDbReleaseNumberDataGridViewColumn = new DataGridViewTextBoxColumn();
                viewCreationDbReleaseNumberDataGridViewColumn.HeaderText = Strings.CreationDbReleaseNumberDataGridColumnHeaderText;
                viewCreationDbReleaseNumberDataGridViewColumn.DataPropertyName = ViewCreationDbReleaseNumberColumnName;
                this.DataStructureGrid.Columns.Add(viewCreationDbReleaseNumberDataGridViewColumn);
            }
            else if (selDataObjectType == DBObjectType.StoredProcedure)
            {
                this.DataStructureGrid.AutoGenerateColumns = false;

                DataGridViewTextBoxColumn procedureNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                procedureNameDataGridViewColumn.HeaderText = Strings.StoredProcedureNameDataGridColumnHeaderText;
                procedureNameDataGridViewColumn.DataPropertyName = ProcedureNameColumnName;
                this.DataStructureGrid.Columns.Add(procedureNameDataGridViewColumn);

                DataGridViewTextBoxColumn procedureTbNameSpaceDataGridViewColumn = new DataGridViewTextBoxColumn();
                procedureTbNameSpaceDataGridViewColumn.HeaderText = Strings.TbNameSpaceDataGridColumnHeaderText;
                procedureTbNameSpaceDataGridViewColumn.DataPropertyName = ProcedureTbNameSpaceColumnName;
                this.DataStructureGrid.Columns.Add(procedureTbNameSpaceDataGridViewColumn);

                DataGridViewTextBoxColumn procedureCreationDbReleaseNumberDataGridViewColumn = new DataGridViewTextBoxColumn();
                procedureCreationDbReleaseNumberDataGridViewColumn.HeaderText = Strings.CreationDbReleaseNumberDataGridColumnHeaderText;
                procedureCreationDbReleaseNumberDataGridViewColumn.DataPropertyName = ProcedureCreationDbReleaseNumberColumnName;
                this.DataStructureGrid.Columns.Add(procedureCreationDbReleaseNumberDataGridViewColumn);
            }
            else if (selDataObjectType == DBObjectType.ExtraAddedColumns)
            {
                this.DataStructureGrid.AutoGenerateColumns = false;

                DataGridViewTextBoxColumn tableNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                tableNameDataGridViewColumn.HeaderText = Strings.TableNameDataGridColumnHeaderText;
                tableNameDataGridViewColumn.DataPropertyName = TableNameColumnName;
                this.DataStructureGrid.Columns.Add(tableNameDataGridViewColumn);
            }
            else
                this.DataStructureGrid.AutoGenerateColumns = true;

            UpdateStructureDetailsDataGridViewColumns();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void UpdateStructureDetailsDataGridViewColumns()
        {
            this.StructureDetailsDataGrid.SuspendLayout();

            this.StructureDetailsDataGrid.Columns.Clear();

            if (this.StructureDetailsDataGrid.DataSource != null)
            {
                DBObjectType selDataObjectType = this.SelectedDBObjectType();
                if (selDataObjectType == DBObjectType.Table)
                {
                    this.StructureDetailsDataGrid.AutoGenerateColumns = false;

                    DataGridViewTextBoxColumn columnNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                    columnNameDataGridViewColumn.HeaderText = Strings.ColumnNameDataGridColumnHeaderText;
                    columnNameDataGridViewColumn.Name = ColumnNameColumnName;
                    columnNameDataGridViewColumn.DataPropertyName = ColumnNameColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnNameDataGridViewColumn);

                    DataGridViewUintTextBoxColumn columnDataLengthDataGridViewColumn = new DataGridViewUintTextBoxColumn();
                    columnDataLengthDataGridViewColumn.HeaderText = Strings.ColumnDataLengthDataGridColumnHeaderText;
                    columnDataLengthDataGridViewColumn.Name = ColumnDataLengthColumnName;
                    columnDataLengthDataGridViewColumn.DataPropertyName = ColumnDataLengthColumnName;
                    DataGridViewDataTypeComboBoxColumn columnDataTypeDataGridViewColumn = new DataGridViewDataTypeComboBoxColumn(columnDataLengthDataGridViewColumn);
                    columnDataTypeDataGridViewColumn.HeaderText = Strings.ColumnDataTypeDataGridColumnHeaderText;
                    columnDataTypeDataGridViewColumn.Name = ColumnDataTypeColumnName;
                    columnDataTypeDataGridViewColumn.DataPropertyName = ColumnDataTypeColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnDataLengthDataGridViewColumn);
                    this.StructureDetailsDataGrid.Columns.Add(columnDataTypeDataGridViewColumn);
                }
                else if (selDataObjectType == DBObjectType.View)
                {
                    this.StructureDetailsDataGrid.AutoGenerateColumns = false;

                    DataGridViewTextBoxColumn columnNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                    columnNameDataGridViewColumn.HeaderText = Strings.ColumnNameDataGridColumnHeaderText;
                    columnNameDataGridViewColumn.Name = ColumnNameColumnName;
                    columnNameDataGridViewColumn.DataPropertyName = ColumnNameColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnNameDataGridViewColumn);

                    DataGridViewUintTextBoxColumn columnDataLengthDataGridViewColumn = new DataGridViewUintTextBoxColumn();
                    columnDataLengthDataGridViewColumn.HeaderText = Strings.ColumnDataLengthDataGridColumnHeaderText;
                    columnDataLengthDataGridViewColumn.Name = ColumnDataLengthColumnName;
                    columnDataLengthDataGridViewColumn.DataPropertyName = ColumnDataLengthColumnName;
                    DataGridViewDataTypeComboBoxColumn columnDataTypeDataGridViewColumn = new DataGridViewDataTypeComboBoxColumn(columnDataLengthDataGridViewColumn);
                    columnDataTypeDataGridViewColumn.HeaderText = Strings.ColumnDataTypeDataGridColumnHeaderText;
                    columnDataTypeDataGridViewColumn.DataPropertyName = ColumnDataTypeColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnDataTypeDataGridViewColumn);
                    this.StructureDetailsDataGrid.Columns.Add(columnDataLengthDataGridViewColumn);

                    DataGridViewCheckBoxColumn columnIsCollateSensitiveDataGridViewColumn = new DataGridViewCheckBoxColumn();
                    columnIsCollateSensitiveDataGridViewColumn.HeaderText = Strings.ColumnIsCollateSensitiveDataGridColumnHeaderText;
                    columnIsCollateSensitiveDataGridViewColumn.Name = ColumnIsCollateSensitiveColumnName;
                    columnIsCollateSensitiveDataGridViewColumn.DataPropertyName = ColumnIsCollateSensitiveColumnName;
                    columnIsCollateSensitiveDataGridViewColumn.ValueType = typeof(bool);
                    this.StructureDetailsDataGrid.Columns.Add(columnIsCollateSensitiveDataGridViewColumn);
                }
                else if (selDataObjectType == DBObjectType.StoredProcedure)
                {
                    this.StructureDetailsDataGrid.AutoGenerateColumns = false;

                    DataGridViewTextBoxColumn paramNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                    paramNameDataGridViewColumn.HeaderText = Strings.ParameterNameDataGridColumnHeaderText;
                    paramNameDataGridViewColumn.Name = ProcedureParameterNameColumnName;
                    paramNameDataGridViewColumn.DataPropertyName = ProcedureParameterNameColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(paramNameDataGridViewColumn);

                    DataGridViewUintTextBoxColumn paramDataLengthDataGridViewColumn = new DataGridViewUintTextBoxColumn();
                    paramDataLengthDataGridViewColumn.HeaderText = Strings.ParameterDataLengthDataGridColumnHeaderText;
                    paramDataLengthDataGridViewColumn.Name = ProcedureParameterDataLengthColumnName;
                    paramDataLengthDataGridViewColumn.DataPropertyName = ProcedureParameterDataLengthColumnName;
                    DataGridViewDataTypeComboBoxColumn paramDataTypeDataGridViewColumn = new DataGridViewDataTypeComboBoxColumn(paramDataLengthDataGridViewColumn);
                    paramDataTypeDataGridViewColumn.HeaderText = Strings.ParameterDataTypeDataGridColumnHeaderText;
                    paramDataTypeDataGridViewColumn.Name = ProcedureParameterDataTypeColumnName;
                    paramDataTypeDataGridViewColumn.DataPropertyName = ProcedureParameterDataTypeColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(paramDataTypeDataGridViewColumn);
                    this.StructureDetailsDataGrid.Columns.Add(paramDataLengthDataGridViewColumn);

                    DataGridViewCheckBoxColumn paramIsOutDataGridViewColumn = new DataGridViewCheckBoxColumn();
                    paramIsOutDataGridViewColumn.HeaderText = Strings.ParameterIsOutDataGridColumnHeaderText;
                    paramIsOutDataGridViewColumn.Name = ProcedureParameterIsOutColumnName;
                    paramIsOutDataGridViewColumn.DataPropertyName = ProcedureParameterIsOutColumnName;
                    paramIsOutDataGridViewColumn.ValueType = typeof(bool);
                    this.StructureDetailsDataGrid.Columns.Add(paramIsOutDataGridViewColumn);

                    DataGridViewCheckBoxColumn paramIsCollateSensitiveDataGridViewColumn = new DataGridViewCheckBoxColumn();
                    paramIsCollateSensitiveDataGridViewColumn.HeaderText = Strings.ParameterIsCollateSensitiveDataGridColumnHeaderText;
                    paramIsCollateSensitiveDataGridViewColumn.Name = ProcedureParameterIsCollateSensitiveColumnName;
                    paramIsCollateSensitiveDataGridViewColumn.DataPropertyName = ProcedureParameterIsCollateSensitiveColumnName;
                    paramIsCollateSensitiveDataGridViewColumn.ValueType = typeof(bool);
                    this.StructureDetailsDataGrid.Columns.Add(paramIsCollateSensitiveDataGridViewColumn);
                }
                else if (selDataObjectType == DBObjectType.ExtraAddedColumns)
                {
                    this.StructureDetailsDataGrid.AutoGenerateColumns = false;

                    DataGridViewTextBoxColumn columnNameDataGridViewColumn = new DataGridViewTextBoxColumn();
                    columnNameDataGridViewColumn.HeaderText = Strings.ColumnNameDataGridColumnHeaderText;
                    columnNameDataGridViewColumn.Name = ColumnNameColumnName;
                    columnNameDataGridViewColumn.DataPropertyName = ColumnNameColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnNameDataGridViewColumn);

                    DataGridViewUintTextBoxColumn columnDataLengthDataGridViewColumn = new DataGridViewUintTextBoxColumn();
                    columnDataLengthDataGridViewColumn.HeaderText = Strings.ColumnDataLengthDataGridColumnHeaderText;
                    columnDataLengthDataGridViewColumn.Name = ColumnDataLengthColumnName;
                    columnDataLengthDataGridViewColumn.DataPropertyName = ColumnDataLengthColumnName;
                    
                    DataGridViewDataTypeComboBoxColumn columnDataTypeDataGridViewColumn = new DataGridViewDataTypeComboBoxColumn(columnDataLengthDataGridViewColumn);
                    columnDataTypeDataGridViewColumn.HeaderText = Strings.ColumnDataTypeDataGridColumnHeaderText;
                    columnDataTypeDataGridViewColumn.Name = ColumnDataTypeColumnName;
                    columnDataTypeDataGridViewColumn.DataPropertyName = ColumnDataTypeColumnName;
                    this.StructureDetailsDataGrid.Columns.Add(columnDataTypeDataGridViewColumn);
                    this.StructureDetailsDataGrid.Columns.Add(columnDataLengthDataGridViewColumn);
                }
                else
                    this.StructureDetailsDataGrid.AutoGenerateColumns = true;
            }
            this.StructureDetailsDataGrid.ResumeLayout();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool SelectDBObjectType(DBObjectType aDBObjectType)
        {
            int dbObjectTypeIndex = this.ShowDefsTypeComboBox.FindStringExact(DataObjectTypeEnumHelper.GetGenreDescription(aDBObjectType));
            if (dbObjectTypeIndex == -1)
                return false;

            this.ShowDefsTypeComboBox.SelectedIndex = dbObjectTypeIndex;
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private DBObjectType SelectedDBObjectType()
        {
            return DataObjectTypeEnumHelper.GetDataObjectTypeFromGenreDescription(this.ShowDefsTypeComboBox.Text);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetDBObjectTypeDefaultSelection()
        {
            if (SelectedDBObjectType() != DBObjectType.Undefined)
                return;
            
            DataSet dbObjectDefsDataSet = masterBindingSource.DataSource as DataSet;
            if (dbObjectDefsDataSet == null)
                throw new Exception("BindingSource is null or invalid.");

            DataTable tableDefTable = dbObjectDefsDataSet.Tables[TableDefinitionTableName];
            if (tableDefTable != null && tableDefTable.Rows.Count > 0)
            {
                SelectDBObjectType(DBObjectType.Table);
                return;
            }

            DataTable viewDefTable = dbObjectDefsDataSet.Tables[ViewDefinitionTableName];
            if (viewDefTable != null && viewDefTable.Rows.Count > 0)
            {
                SelectDBObjectType(DBObjectType.View);
                return;
            }

            DataTable storedProcedureDefTable = dbObjectDefsDataSet.Tables[StoredProcedureDefinitionTableName];
            if (storedProcedureDefTable != null && storedProcedureDefTable.Rows.Count > 0)
            {
                SelectDBObjectType(DBObjectType.StoredProcedure);
                return;
            }

            DataTable extraAddedColumnsDefTable = dbObjectDefsDataSet.Tables[TableExtraAddedColumnsDefinitionTableName];
            if (extraAddedColumnsDefTable != null && extraAddedColumnsDefTable.Rows.Count > 0)
                SelectDBObjectType(DBObjectType.ExtraAddedColumns);
        }


        #region DBObjectsDefControl private event handlers

        //--------------------------------------------------------------------------------------------------------------------------------
        private void ShowDefsTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DataStructureGrid.DataSource = null;
            this.StructureDetailsDataGrid.DataSource = null;

            if (this.ShowDefsTypeComboBox.SelectedIndex == -1)
                return;

            DataSet masterDataSet = ((masterBindingSource != null) ? masterBindingSource.DataSource : null) as DataSet;
            DBObjectType selDataObjectType = this.SelectedDBObjectType();
            if (selDataObjectType != DBObjectType.Undefined && masterDataSet != null)
            {
                if (selDataObjectType == DBObjectType.Table && masterDataSet.Tables.Contains(TableDefinitionTableName))
                {
                    masterBindingSource.DataMember = TableDefinitionTableName;

                    this.DataStructureGrid.DataSource = masterBindingSource;

                    if (masterDataSet.Relations.Contains(TableColumnsRelationName))
                    {
                        detailsBindingSource.DataMember = TableColumnsRelationName;

                        this.StructureDetailsDataGrid.DataSource = detailsBindingSource;
                    }
                }
                else if (selDataObjectType == DBObjectType.View && masterDataSet.Tables.Contains(ViewDefinitionTableName))
                {
                    masterBindingSource.DataMember = ViewDefinitionTableName;

                    this.DataStructureGrid.DataSource = masterBindingSource;

                    if (masterDataSet.Relations.Contains(ViewColumnsRelationName))
                    {
                        detailsBindingSource.DataMember = ViewColumnsRelationName;

                        this.StructureDetailsDataGrid.DataSource = detailsBindingSource;
                    }
                }
                else if (selDataObjectType == DBObjectType.StoredProcedure && masterDataSet.Tables.Contains(StoredProcedureDefinitionTableName))
                {
                    masterBindingSource.DataMember = StoredProcedureDefinitionTableName;

                    this.DataStructureGrid.DataSource = masterBindingSource;

                    if (masterDataSet.Relations.Contains(StoredProcedureParametersRelationName))
                    {
                        detailsBindingSource.DataMember = StoredProcedureParametersRelationName;

                        this.StructureDetailsDataGrid.DataSource = detailsBindingSource;
                    }
                }
            }
            UpdateDataStructureGridViewColumns();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void EditModeButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.EditModeButton.Checked == isInEditMode)
                return;

            this.EditModeButton.ImageIndex = this.EditModeButton.Checked ? 1 : 0;
            this.IsInEditMode = this.EditModeButton.Checked;
        }

        #endregion // DBObjectsDefControl private event handlers

        #endregion // DBObjectsDefControl private methods

        //--------------------------------------------------------------------------------------------------------------------------------
        protected void OnEditModeChanged(EventArgs e)
        {
            if (EditModeChanged != null)
                EditModeChanged(this, e);
        }


        #region DBObjectsDefControl public properties

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set
            {
                isInEditMode = value;

                this.EditModeButton.Checked = isInEditMode;
                
                this.DataStructureGrid.AllowUserToAddRows = isInEditMode;
                this.DataStructureGrid.AllowUserToDeleteRows = isInEditMode;
                this.DataStructureGrid.ReadOnly = !isInEditMode;
                this.DataStructureGrid.EditMode = isInEditMode ? DataGridViewEditMode.EditOnEnter : DataGridViewEditMode.EditProgrammatically;

                this.StructureDetailsDataGrid.AllowUserToAddRows = isInEditMode;
                this.StructureDetailsDataGrid.AllowUserToDeleteRows = isInEditMode;
                this.StructureDetailsDataGrid.ReadOnly = !isInEditMode;
                this.StructureDetailsDataGrid.EditMode = isInEditMode ? DataGridViewEditMode.EditOnEnter : DataGridViewEditMode.EditProgrammatically;

                OnEditModeChanged(EventArgs.Empty);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ShowEditModeButton
        {
            get { return showEditModeButton; }
            set 
            {
                showEditModeButton = value;
                
                this.EditModeButton.Visible = showEditModeButton;

                int rightOffset = this.Width - this.DataStructureGrid.Right;
                if (showEditModeButton)
                {
                    rightOffset *= 2;
                    rightOffset += this.EditModeButton.Width;
                }

                this.DataStructureTypeSelLabel.Size = new Size(this.Width - this.DataStructureTypeSelLabel.Left - rightOffset, this.DataStructureTypeSelLabel.Height);
                this.ShowDefsTypeComboBox.Size = new Size(this.Width - this.ShowDefsTypeComboBox.Left - rightOffset, this.ShowDefsTypeComboBox.Height);
            }
        }
        
        #endregion //DBObjectsDefControl public properties


        #region DBObjectsDefControl public methods
        
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool LoadDBObjects(IBasePathFinder aPathFinder, IBaseModuleInfo aModuleInfo)
        {
            if (masterBindingSource == null || masterBindingSource.DataSource == null)
                throw new Exception("BindingSource is null or invalid.");

            if (aPathFinder == null || aModuleInfo == null)
                return false;

            Cursor currentCursor = Cursor.Current;

			try
            {
                Cursor.Current = IDECursors.WaitCursor;

                DBObjectsLoader dbObjsloader = new DBObjectsLoader(aPathFinder, DBMSType.UNKNOWN);
                if (!dbObjsloader.LoadModuleDbObjects(aModuleInfo))
                    return false;

                Dictionary<string, ExtendedWizardTableInfo> tableDefs = dbObjsloader.Tables;
                if (tableDefs != null && tableDefs.Count > 0)
                {
                    //FillTablesData(tableDefs.Values.Cast<WizardTableInfo>().ToArray());
                    List<WizardTableInfo> tablesInfo = new List<WizardTableInfo>();
                    foreach (ExtendedWizardTableInfo exTableInfo in tableDefs.Values)
                        tablesInfo.Add((WizardTableInfo)exTableInfo);
                    FillTablesData(tablesInfo.ToArray());
                }
                Dictionary<string, ExtendedSqlView> viewDefs = dbObjsloader.Views;
                if (viewDefs != null && viewDefs.Count > 0)
                {
                    //FillViewsData(viewDefs.Cast<SqlView>().ToArray());
                    List<SqlView> viewsInfo = new List<SqlView>();
                    foreach (ExtendedSqlView exViewInfo in viewDefs.Values)
                        viewsInfo.Add((SqlView)exViewInfo);
                    FillViewsData(viewsInfo.ToArray());
                }

                Dictionary<string, ExtendedSqlProcedure> storedProcDefs = dbObjsloader.Procedures;
				if (storedProcDefs != null && storedProcDefs.Count > 0)
					FillStoredProceduresData(storedProcDefs.Cast<SqlProcedure>().ToArray());

                Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>> extraAddedColumnsDefs = dbObjsloader.AddedColumns;
                if (extraAddedColumnsDefs != null && extraAddedColumnsDefs.Count > 0)
                {
                    List<WizardExtraAddedColumnsInfo> allExtraAddedColumnsDefs = new List<WizardExtraAddedColumnsInfo>();
					foreach (List<ExtendedWizardExtraAddedColumnsInfo> columnslist in extraAddedColumnsDefs.Values)
						allExtraAddedColumnsDefs.AddRange(columnslist.Cast<WizardExtraAddedColumnsInfo>().ToArray());
					FillExtraAddedColumnsData(allExtraAddedColumnsDefs.ToArray());
                }

                SetDBObjectTypeDefaultSelection();
                
                return true;
            }
            catch 
            {
                throw;
            }
            finally
            {
                Cursor.Current = currentCursor;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool LoadFile(string aFileName)
        {
            if (masterBindingSource == null || masterBindingSource.DataSource == null)
                throw new Exception("BindingSource is null or invalid.");
            
            if (String.IsNullOrEmpty(aFileName) || !File.Exists(aFileName))
                return false;

            FileStream xmlStream = null;
            Cursor currentCursor = Cursor.Current;
            try
            {
                Cursor.Current = IDECursors.WaitCursor;

                if (!DBObjectDefsFileValidator.IsValiDBObjectsDefFile(aFileName))
                    return false;

                DBObjectDefsHelper dbObjectDefsHelper = new DBObjectDefsHelper(aFileName);

                IList<WizardTableInfo> tableDefs = dbObjectDefsHelper.ParseFileTables();
                if (tableDefs != null && tableDefs.Count > 0)
                    FillTablesData(tableDefs.ToArray());

                SqlViewList viewDefs = dbObjectDefsHelper.ParseViewsInfoNode();
                if (viewDefs != null && viewDefs.Count > 0)
                    FillViewsData(viewDefs.ToArray());

                SqlProcedureList storedProcDefs = dbObjectDefsHelper.ParseProceduresInfoNode();
                if (storedProcDefs != null && storedProcDefs.Count > 0)
                    FillStoredProceduresData(storedProcDefs.ToArray());

                IList<WizardExtraAddedColumnsInfo> extraAddedColumnsDefs = dbObjectDefsHelper.ParseExtraAddedColumnsInfo();
                if (extraAddedColumnsDefs != null && extraAddedColumnsDefs.Count > 0)
                    FillExtraAddedColumnsData(extraAddedColumnsDefs.ToArray());
                
                SetDBObjectTypeDefaultSelection();

                return true;
            }
            catch 
            {
                throw;
            }
            finally
            {
                if (xmlStream != null)
                    xmlStream.Close();

                Cursor.Current = currentCursor;
            }

        }

        #endregion // DBObjectsDefControl public methods

    }
}
