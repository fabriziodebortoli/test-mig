using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Forms.Containers;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Forms.DataBinding;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.Data;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms
{
    //================================================================================================================
    /// <summary>
    /// TBWFCUIGrid
    /// </summary>
    internal class TBWFCUIGrid : TBWFCUIControl
    {
        //================================================================================================================
        private class TBWFCUIGridCache : Dictionary<string, TBWFCUIGridCacheItems>, IDisposable
        {
            //---------------------------------------------------------------------
            internal TBWFCUIGridCacheItem GetColumnCache(string fieldName, string columnName)
            {
                TBWFCUIGridCacheItems items = null;

                if (!TryGetValue(fieldName, out items))
                {
                    items = new TBWFCUIGridCacheItems();
                    Add(fieldName, items);
                }

                TBWFCUIGridCacheItem item = items.GetGridCacheItem(columnName);
                if (item == null)
                {
                    item = new TBWFCUIGridCacheItem(columnName);
                    items.Add(item);
                }
                return item;
            }

            //---------------------------------------------------------------------
            internal void RemoveColumnCache(string fieldName, string columnName)
            {
                TBWFCUIGridCacheItems items = null;

                if (TryGetValue(fieldName, out items))
                {
                    TBWFCUIGridCacheItem item = items.GetGridCacheItem(columnName);
                    if (item != null)
                    {
                        item.Dispose();
                        items.Remove(item);
                        item = null;
                    }
                }
            }

            //---------------------------------------------------------------------
            public void Dispose()
            {
                foreach (TBWFCUIGridCacheItems value in Values)
                {
                    if (value != null)
                        value.Dispose();
                }
                Clear();
            }
        }

        //================================================================================================================
        private class TBWFCUIGridCacheItems : List<TBWFCUIGridCacheItem>, IDisposable
        {
            //---------------------------------------------------------------------
            internal TBWFCUIGridCacheItem GetGridCacheItem(string columnName)
            {
                foreach (TBWFCUIGridCacheItem item in this)
                {
                    if (item.ColumnName == columnName)
                        return item;
                }
                return null;
            }

            //---------------------------------------------------------------------
            public void Dispose()
            {
                foreach (TBWFCUIGridCacheItem item in this)
                {
                    item.Dispose();
                }
                Clear();
            }
        }

        //================================================================================================================
        private class TBWFCUIGridCacheItem : IDisposable
        {
            public ITBFormatterProvider FormatterProvider { get; set; }
            public UIGridEditor Editor { get; set; }
            public string ColumnName { get; set; }

            //---------------------------------------------------------------------
            public TBWFCUIGridCacheItem(string colName)
            {
                this.ColumnName = colName;
            }

            //---------------------------------------------------------------------
            public void Dispose()
            {
                if (Editor != null)
                    Editor.Dispose();
            }
        }

        //================================================================================================================
        private class GridDataSourceSwitcher
        {
            private UIGrid grid;
            private MDBTSlaveBuffered dbtPrototype;
            private MDBTSlaveBuffered currentParent;

            //---------------------------------------------------------------------------
            public GridDataSourceSwitcher()
            {

            }
            //---------------------------------------------------------------------------
            public void ParentDBT_CurrentRowChanged(object sender, RowEventArgs e)
            {
                SwitchDataSource((MDBTSlaveBuffered)sender);
            }

            //---------------------------------------------------------------------------
            private void SwitchDataSource(MDBTSlaveBuffered parentDbt)
            {
                MDBTSlaveBuffered currentSlave = parentDbt.GetCurrentSlave(dbtPrototype.Name) as MDBTSlaveBuffered;
                if (currentSlave == null || currentSlave == dbtPrototype)
                {
                    grid.DataSource = null;
                    ((TBWFCUIGrid)grid.CUI).AttachDataManager(null);
                }
                else
                {
                    grid.DataSource = currentSlave.BindableDataSource;
                    ((TBWFCUIGrid)grid.CUI).AttachDataManager(currentSlave);
                }
            }
            //---------------------------------------------------------------------------
            void GrandFather_CurrentRowChanged(object sender, RowEventArgs e)
            {
                currentParent.CurrentRowChanged -= new EventHandler<RowEventArgs>(ParentDBT_CurrentRowChanged);
                currentParent.Disposed -= new EventHandler(ParentDBT_Disposed);
                currentParent = ((MDBTSlaveBuffered)sender).GetCurrentSlave(currentParent.Name) as MDBTSlaveBuffered;
                currentParent.CurrentRowChanged += new EventHandler<RowEventArgs>(ParentDBT_CurrentRowChanged);
                currentParent.Disposed += new EventHandler(ParentDBT_Disposed);

                //forzo il refresh del datasource simulando un cambio di riga
                SwitchDataSource(currentParent);

            }
            //---------------------------------------------------------------------------
            public void ParentDBT_Disposed(object sender, EventArgs e)
            {
                MDBTSlaveBuffered dbtSlaveBuffered = sender as MDBTSlaveBuffered;
                if (dbtSlaveBuffered != null)
                {
                    dbtSlaveBuffered.Disposed -= new EventHandler(ParentDBT_Disposed);
                    dbtSlaveBuffered.CurrentRowChanged -= new EventHandler<RowEventArgs>(ParentDBT_CurrentRowChanged);
                }
            }

            //---------------------------------------------------------------------------
            internal void AttachEvents(UIGrid grid, MDBTSlaveBuffered dbtPrototype)
            {
                this.grid = grid;

                AttachEvents(dbtPrototype);

            }

            //---------------------------------------------------------------------------
            private void AttachEvents(MDBTSlaveBuffered dbtPrototype)
            {
                this.dbtPrototype = dbtPrototype;
                currentParent = (MDBTSlaveBuffered)dbtPrototype.ParentComponent;
                currentParent.CurrentRowChanged += new EventHandler<RowEventArgs>(ParentDBT_CurrentRowChanged);
                currentParent.Disposed += new EventHandler(ParentDBT_Disposed);

                MDBTSlaveBuffered grandFatherDbtPrototype = currentParent.ParentComponent as MDBTSlaveBuffered;
                if (grandFatherDbtPrototype != null)
                {
                    grandFatherDbtPrototype.CurrentRowChanged += new EventHandler<RowEventArgs>(GrandFather_CurrentRowChanged);
                    grandFatherDbtPrototype.Disposed += new EventHandler(GrandFather_Disposed);
                }
            }

            //---------------------------------------------------------------------------
            void GrandFather_Disposed(object sender, EventArgs e)
            {
                MDBTSlaveBuffered dbtSlaveBuffered = sender as MDBTSlaveBuffered;
                if (dbtSlaveBuffered != null)
                {
                    dbtSlaveBuffered.Disposed -= new EventHandler(GrandFather_Disposed);
                    dbtSlaveBuffered.CurrentRowChanged -= new EventHandler<RowEventArgs>(GrandFather_CurrentRowChanged);
                }
            }
        }

        Dictionary<string, List<ITBUIExtenderProvider>> extendersCache = new Dictionary<string, List<ITBUIExtenderProvider>>();
        TBWFCUIGridCache cache = new TBWFCUIGridCache();

        private MDBTSlaveBuffered DBTSlaveBuffered { get { return DataBinding != null ? DataBinding.Data as MDBTSlaveBuffered : null; } }
        private CurrencyManager CurrencyManager { get { return Grid.DataSource == null ? null : Grid.BindingContext[Grid.DataSource] as CurrencyManager; } }

        private Font underlineFont;
        private Font defaultFont;
        private Color defaultForeColor;
        private MouseButtons mouseButtonClicked;

        private UIForm formRowView;

        internal UIGrid Grid { get { return Control as UIGrid; } }
        public bool ReadOnly { get { return Grid.ReadOnly; } set { Grid.ReadOnly = value; } }

        private Font UnderlineFont
        {
            get
            {
                if (underlineFont == null)
                    underlineFont = new Font(DefaultFont, FontStyle.Underline | FontStyle.Bold);
                return underlineFont;
            }
        }
        private static Color UnderlineForeColor { get { return Color.Blue; } }
        private Font DefaultFont { get { return defaultFont; } set { defaultFont = value; } }
        private Color DefaultForeColor { get { return defaultForeColor; } set { defaultForeColor = value; } }

        //-------------------------------------------------------------------------
        public void AddTbBinding(UIGrid grid, IDataManager dataManager)
        {
            grid.DataSource = dataManager.BindableDataSource;
            AttachDataManager(dataManager);
            MDBTSlaveBuffered dbt = (MDBTSlaveBuffered)dataManager;
            if (dbt.ParentComponent is MDBTSlaveBuffered)
            {
                GridDataSourceSwitcher switcher = new GridDataSourceSwitcher();
                switcher.AttachEvents(grid, dbt);

            }
        }
        //-------------------------------------------------------------------------
        void AttachDataManager(IDataManager dataManager)
        {
            if (dataManager == null)
            {
                this.Document = null;
                DataBinding = null;
            }
            else
            {
                this.Document = (IMAbstractFormDoc)dataManager.Document;
                DataBinding = new DBTDataBinding((MDBTObject)dataManager);
            }
        }
        //---------------------------------------------------------------------------
        public override IDataBinding DataBinding
        {
            get
            {
                return base.DataBinding;
            }
            set
            {
                if (DBTSlaveBuffered != null)
                {
                    DetachDBTEvents();
                }
                if (CurrencyManager != null)
                {
                    CurrencyManager.PositionChanged -= new EventHandler(CurrencyManager_PositionChanged);
                }

                DBTDataBinding dbtDataBinding = value as DBTDataBinding;
                if (dbtDataBinding == null)
                {
                    base.DataBinding = dbtDataBinding;
                    return;
                }

                MDBTSlaveBuffered dbtSlaveBuffered = dbtDataBinding.Data as MDBTSlaveBuffered;
                if (dbtSlaveBuffered == null)
                {
                    Debug.Assert(false, "OOOPS!! Si sta tentando di bindare alla griglia un oggetto diverso da un DBTSlaveBuffered!!");
                    return;
                }

                base.DataBinding = value;

                if (DBTSlaveBuffered != null)
                {
                    DBTSlaveBuffered.RemovingRecord += new EventHandler<RowEventArgs>(DBTSlaveBuffered_RemovingRecord);
                    DBTSlaveBuffered.DeletingRow += new EventHandler<RowEventArgs>(DBTSlaveBuffered_RemovingRecord);
                    DBTSlaveBuffered.LoadingData += new EventHandler<EventArgs>(DBTSlaveBuffered_LoadingData);
                    DBTSlaveBuffered.DataLoaded += new EventHandler<EventArgs>(DBTSlaveBuffered_DataLoaded);
                    DBTSlaveBuffered.ReadOnlyChanged += new EventHandler(DBTSlaveBuffered_ReadOnlyChanged);

                    AttachColumn(Grid.UIColumns);
                }

                if (CurrencyManager != null)
                {
                    CurrencyManager.PositionChanged += new EventHandler(CurrencyManager_PositionChanged);
                }

                //TODO evento? OnDataBindingChanged(this, new DataBindingChangedEventArgs(oldValue));
            }
        }

        //---------------------------------------------------------------------
        void DBTSlaveBuffered_ReadOnlyChanged(object sender, EventArgs e)
        {
            Grid.ReadOnly = DBTSlaveBuffered.ReadOnly;
        }

        //---------------------------------------------------------------------
        void CurrencyManager_PositionChanged(object sender, EventArgs e)
        {
            int gridPosition = CurrencyManager.Position;
            if (gridPosition == -1)
            {
                DBTSlaveBuffered.CurrentRow = -1;
                return;
            }

            //Allineo la current row del dbt a quella che midice il currency manager
            MSqlRecord recordGrid = CurrencyManager.Current as MSqlRecord;
            DBTSlaveBuffered.SetCurrentRowByRecord(recordGrid);

            //Allinea eventuali hot link sulla riga.
            foreach (IRecordField recordField in recordGrid.Fields)
            {
                SetPrototypeHotlinkTo(recordField.DataObj as MDataObj);
            }
        }

        //---------------------------------------------------------------------------
        public TBWFCUIGrid(UIGrid grid)
            : base(grid, Interfaces.NameSpaceObjectType.Grid)
        {
            Grid.EditorRequired += new Telerik.WinControls.UI.EditorRequiredEventHandler(Grid_EditorRequired);
            Grid.ViewCellFormatting += new CellFormattingEventHandler(Grid_ViewCellFormatting);
            Grid.CellBeginEdit += new GridViewCellCancelEventHandler(Grid_CellBeginEdit);
            Grid.CellClick += new GridViewCellEventHandler(Grid_CellClick);
            Grid.AllowAddNewRow = false;

            Grid.CellValidating += new CellValidatingEventHandler(Grid_CellValidating);
            Grid.CellValidated += new CellValidatedEventHandler(Grid_CellValidated);

            Grid.UserDeletingRow += new GridViewRowCancelEventHandler(Grid_UserDeletingRow);
            Grid.ContextMenuOpening += new ContextMenuOpeningEventHandler(Grid_ContextMenuOpening);

            Grid.MultiSelect = true;
            Grid.SelectionMode = GridViewSelectionMode.FullRowSelect;

            Grid.MouseClick += new MouseEventHandler(Grid_MouseClick);
            Grid.GridBehavior = new StandardInputBehaviour();
            Grid.UIColumns.CollectionChanged += new NotifyCollectionChangedEventHandler(UIColumns_CollectionChanged);
            // font changed handler registration
            Grid.FontChanged += new EventHandler(Grid_FontChanged);
            // create row handler registration
            Grid.RowHeightChanging += new RowHeightChangingEventHandler(Grid_RowHeightChanging);            
            Grid.DataBindingComplete += new GridViewBindingCompleteEventHandler(Grid_DataBindingComplete);
        }

        /// <summary>
        /// Handles the change font used in the grid.
        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //---------------------------------------------------------------------
        void Grid_FontChanged(object sender, EventArgs e)
        {
            Grid.MinRowHeight = GetMaxRowHeight(Grid.UIColumns);
        }

        //------------------------------------------------------------------------------------
        void Grid_RowHeightChanging(object sender, RowHeightChangingEventArgs e)
        {
            if (e.NewHeight < Grid.MinRowHeight)
                e.Cancel = true;
        }

        //---------------------------------------------------------------------------
        void Grid_DataBindingComplete(object sender, GridViewBindingCompleteEventArgs e)
        {
            DocumentFormModeChanged(this.Document, EventArgs.Empty);
        }

        // si notifica agli eventi del dataobj di prototipo per gestire
        // il readonly delle colonne della griglia e gli altri eventi di colonna
        //---------------------------------------------------------------------------        
        void AttachColumn(IList columnsCollection) 
        { 
            Int32 maxRowHeight;
            AttachColumn(columnsCollection, out maxRowHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnsCollection">The given columns to be attached.</param>
        /// <param name="maxRowHeight">The max row height according to the given columns, i.e. 
        /// the height required to properly display their editor.</param>
        // si notifica agli eventi del dataobj di prototipo per gestire
        // il readonly delle colonne della griglia e gli altri eventi di colonna
        //---------------------------------------------------------------------------        
        void AttachColumn(IList columnsCollection, out Int32 maxRowHeight)
        {
            maxRowHeight = 0;
            // input domain check
            if (columnsCollection == null)
            {
                return;
            }
            foreach (IUIGridColumn col in columnsCollection)
            {
                if (string.IsNullOrEmpty(col.FieldName))
                    continue;

                // for performance reasons row height calculation based on columns is 
                // done here inside the attach loop.
                int extraVerticalSpace = Grid.GetExtraVerticalSpace();
                int currHeight = SizeHelper.GetEditorHeight(col as IUIGridColumn, extraVerticalSpace, Grid.Font);

                if (currHeight > maxRowHeight) 
                {
                    maxRowHeight = currHeight;
                }

                MDataObj dataObj = GetPrototypeDataObj(col.FieldName);
                if (dataObj != null)
                {
                    dataObj.ReadOnlyChanged += new EventHandler<EasyBuilderEventArgs>(dataObj_ReadOnlyChanged);
                    dataObj.VisibleChanged += new EventHandler<EasyBuilderEventArgs>(dataObj_VisibleChanged);

                    TBWFCUIGridCacheItem item = cache.GetColumnCache(dataObj.Name, col.Name);
                    item.FormatterProvider = TBFormatterProvider.Create(dataObj);


                    GridViewDataColumn c = ((GridViewDataColumn) col);
                    col.InitializeFormatter(dataObj, item.FormatterProvider);

                    dataObj_ReadOnlyChanged(dataObj, EasyBuilderEventArgs.Empty);
                    col.Width = (int)col.GetColumnWidth(dataObj, item.FormatterProvider, Grid.Font);
                   
                }
            }
            
        }



        //---------------------------------------------------------------------------
        void dataObj_VisibleChanged(object sender, EasyBuilderEventArgs e)
        {
            MDataObj dataObj = sender as MDataObj;
            if (dataObj == null)
                return;

            TBWFCUIGridCacheItems items = null;
            if (cache.TryGetValue(dataObj.Name, out items))
            {
                foreach (TBWFCUIGridCacheItem item in items)
                {
                    GridViewDataColumn col = Grid.UIColumns[item.ColumnName];
                    if (col != null)
                        col.IsVisible = dataObj.Visible;
                }
            }
        }

        // si sgancia dagli eventi di dataobj collegati alla colonna 
        //---------------------------------------------------------------------------
        void DetachColumn(IList columnsCollection) 
        {
            Int32 maxRowHeight;
            DetachColumn(columnsCollection, out maxRowHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnsCollection">The given columns to be detached.</param>
        /// <param name="maxRowHeight">The max row height according to the given columns, i.e. 
        /// the height required to properly display their editor.</param>
        // si sgancia dagli eventi di dataobj collegati alla colonna 
        //---------------------------------------------------------------------------
        void DetachColumn(IList columnsCollection, out Int32 maxRowHeight)
        {
            maxRowHeight = 0;
            // input domain check
            if (columnsCollection == null)
            {
                return;
            }
            foreach (GridViewDataColumn col in columnsCollection)
            {
                if (string.IsNullOrEmpty(col.FieldName))
                    continue;

                // for performance reasons row height calculation based on columns is 
                // done here inside the attach loop.
                int extraVerticalSpace = Grid.GetExtraVerticalSpace();
                int currHeight = SizeHelper.GetEditorHeight(col as IUIGridColumn, extraVerticalSpace, Grid.Font);

                if (currHeight > maxRowHeight)
                {
                    maxRowHeight = currHeight;
                }

                MDataObj dataObj = GetPrototypeDataObj(col.FieldName);
                if (dataObj != null)
                {
                    dataObj.ReadOnlyChanged -= new EventHandler<EasyBuilderEventArgs>(dataObj_ReadOnlyChanged);
                    dataObj.VisibleChanged -= new EventHandler<EasyBuilderEventArgs>(dataObj_VisibleChanged);
                }
                //rimozione dalla cache
                cache.RemoveColumnCache(dataObj.Name, col.Name);
            }
        }

        //---------------------------------------------------------------------------
        void dataObj_ReadOnlyChanged(object sender, EasyBuilderEventArgs e)
        {
            MDataObj dataObj = sender as MDataObj;
            if (dataObj == null)
                return;

            TBWFCUIGridCacheItems items = null;
            if (cache.TryGetValue(dataObj.Name, out items))
            {
                foreach (TBWFCUIGridCacheItem item in items)
                {
                    IUIGridColumn col = Grid.UIColumns[item.ColumnName] as IUIGridColumn;
                    if (col != null)
                        col.ReadOnly = dataObj.ReadOnly;
                }
            }
        }

        //---------------------------------------------------------------------------
        void UIColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int addedMaxHeight = 0;
            int removedMaxHeight = 0;
            if (DBTSlaveBuffered == null)
            {
                // min row height can be calculated as well.                
                removedMaxHeight = GetMaxRowHeight(e.OldItems);
                addedMaxHeight = GetMaxRowHeight(e.NewItems);

            }
            else
            {
                // we have a valid DBTSlaveBuffered,
                // need to perform attach/detach of columns.
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AttachColumn(e.NewItems, out addedMaxHeight);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        DetachColumn(e.NewItems, out removedMaxHeight);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        DetachColumn(e.OldItems, out removedMaxHeight);
                        AttachColumn(e.NewItems, out addedMaxHeight);
                        break;
                    case NotifyCollectionChangedAction.Batch:
                        AttachColumn(e.NewItems, out addedMaxHeight);
                        break;
                }
            }
            UpdateRowHeight(addedMaxHeight, removedMaxHeight);
        }

        /// <summary>
        /// Updates, if needed, the row height according to
        /// the row heights required by the removed and added columns.
        /// </summary>
        /// <param name="addedMaxHeight">The row height required by the added rows.</param>
        /// <param name="removedMaxHeight">The row height required by the removed rows.</param>
        //---------------------------------------------------------------------------        
        private void UpdateRowHeight(int addedMaxHeight, int removedMaxHeight)
        {
            // set proper row height so that each row is 
            // able to properly show current columns editors.

            // the current grid row height.
            int currentRowHeight = Grid.RowHeight;

            if (Grid.RowHeight < addedMaxHeight)
            {
                // row height must be increased because 
                // of newly added rows. We can ignore the removed 
                // columns.
                Grid.RowHeight = addedMaxHeight;
            }
            else
            {
                // newly added columns do not affect 
                // row height as they require a row height 
                // lower than the current one.
                // Still, we need to know whether the removed columns 
                // were driving row height.
                if (Grid.RowHeight == removedMaxHeight)
                {
                    // one among the deleted rows was driving 
                    // the row height, it must be recalculated 
                    // according to the current columns, i.e. row height must
                    // be lowered according to current columns.
                    int iNewRowHeight = 0;
                    Font oGridFont = Grid.Font;

                    // get vertical padding and borders around each grid cell.
                    int iVerticalBorderThickness = Grid.GetExtraVerticalSpace();
                    // scan current columns in order to get the required row height.
                    foreach (IUIGridColumn column in Grid.UIColumns)
                    {
                        IUIGridColumn uiColumn = column as IUIGridColumn;
                        int iRowHeight = SizeHelper.GetEditorHeight(uiColumn, iVerticalBorderThickness, oGridFont);
                        if (iRowHeight > iNewRowHeight)
                        {
                            iNewRowHeight = iRowHeight;
                        }
                    }
                    Grid.RowHeight = iNewRowHeight;
                }
            }
        }

        /// <summary>
        /// Evaluates the max row height imposed by 
        /// the editors for the given grid columns.
        /// </summary>
        /// <param name="oldItems">The given columns.</param>
        /// <returns>The max row height for the given columns.</returns>
        //---------------------------------------------------------------------        
        private int GetMaxRowHeight(IList columnsCollection)
        {
            int iMaxHeight = 0;
            // Scan all given columns looking for the max 
            // height required by them.
            if (columnsCollection != null)
            {
                foreach (IUIGridColumn col in columnsCollection)
                {
                    
                    int borders = GetExtraVerticalSpace();
                    int iCurrHeight = SizeHelper.GetEditorHeight(col, borders, Grid.Font);
                    if (iCurrHeight > iMaxHeight) 
                    {
                        // update max height found.
                        iMaxHeight = iCurrHeight;
                    }
                }               
            }
            return iMaxHeight;
        }

        /// <summary>
        /// The extra vertical space for each grid row, 
        /// i.e. vertical padding and borders.
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------
        private int GetExtraVerticalSpace()
        {            
            return this.Grid.GetExtraVerticalSpace();
        }

        //---------------------------------------------------------------------
        public void DeleteSelectedRows()
        {
            Grid.GridNavigator.DeleteSelectedRows();
        }

        //---------------------------------------------------------------------
        void Grid_MouseClick(object sender, MouseEventArgs e)
        {
            mouseButtonClicked = e.Button;
        }

        //---------------------------------------------------------------------
        void DBTSlaveBuffered_LoadingData(object sender, EventArgs e)
        {
            Grid.BeginUpdate();
        }

        //---------------------------------------------------------------------
        void DBTSlaveBuffered_DataLoaded(object sender, EventArgs e)
        {
            Grid.EndUpdate();
        }

        //---------------------------------------------------------------------------
        void DBTSlaveBuffered_RemovingRecord(object sender, RowEventArgs e)
        {
            foreach (IRecordField field in e.Record.Fields)
            {
                MDataObj dataObj = field.DataObj as MDataObj;
                if (dataObj.CurrentHotLink != null)
                {
                    ResetPrototypeHotlink(dataObj);
                }
            }
        }

        //---------------------------------------------------------------------------
        void Grid_CellValidated(object sender, CellValidatedEventArgs e)
        {
            //Qui chiamiamo la nostra setmodified (di griglia), perche' non ci viene passato il control dell'editor
            if (Grid.IsInEditMode)
            {
                MDataObj dataObj = GetDataObj(e.Column.FieldName);
                if (dataObj == null)
                    return;

                TBWFCUIGridCacheItem item = cache.GetColumnCache(dataObj.Name, e.Column.Name);
                if (item != null)
                {
                    UIGridEditor gridEditor = item.Editor as UIGridEditor;
                    if (gridEditor == null)
                    {
                        Debug.Assert(false, "OOOPS!!!Non c'e' editor associato alla cella corrente in fase di Cell_Validated!!");
                        return;
                    }
                    IUIGridEditorControl control = gridEditor.TBCui.Component as IUIGridEditorControl;
                    if (control != null)
                    {
                        control.FireValidated();
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        void Grid_CellValidating(object sender, CellValidatingEventArgs e)
        {
            if (Grid.IsInEditMode)
            {
                if (e.ActiveEditor != null)
                {
                    UIGridEditor baseEditor = e.ActiveEditor as UIGridEditor;
                    IUIGridEditorControl control = baseEditor.TBCui.Component as IUIGridEditorControl;
                    if (control != null)
                    {
                        CancelEventArgs args = new CancelEventArgs();
                        control.FireValidating(args);
                        e.Cancel = args.Cancel;                        
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        void Grid_UserDeletingRow(object sender, GridViewRowCancelEventArgs e)
        {
            e.Cancel = !DoDeleteRow();
        }

		//---------------------------------------------------------------------------
		void Grid_ContextMenuOpening(object theSender, ContextMenuOpeningEventArgs e)
		{
			if (ReadOnly)
			{
				return;
			}
			UIMenuItem addItem = new UIMenuItem(Resources.AddRowGridContextMenuItem);
			addItem.Click += (sender, args) => { DoAddInsertRow(false); };
			e.ContextMenu.Items.Add(addItem);

			UIMenuItem insertItem = new UIMenuItem(Resources.InsertRowGridContextMenuItem);
			insertItem.Click += (sender, args) => { DoAddInsertRow(true); };

            e.ContextMenu.Items.Add(insertItem);
        }

        //---------------------------------------------------------------------------
        internal void DoAddInsertRow(bool insert = false)
        {
            if (!(Document.FormMode == FormModeType.New || Document.FormMode == FormModeType.Edit))
                return;

            int insertPos = CurrencyManager == null || CurrencyManager.Position < 0 ? 0 : CurrencyManager.Position;

            if (DBTSlaveBuffered == null)
            {
                if (Grid.DataSource != null)
                {
                    DoBaseAddInsert(insert, insertPos);
                }
                return;
            }

            DoDbtAddInsert(insert, insertPos);
            Grid.BeginEdit();
        }

        //---------------------------------------------------------------------------
        private void DoDbtAddInsert(bool insert, int insertPos)
        {
            //Add con logica di DBTSlaveBuffered
            MSqlRecord rec = insert ? DBTSlaveBuffered.InsertRecord(insertPos) : DBTSlaveBuffered.AddRecord();
            if (!insert)
                CurrencyManager.Position = DBTSlaveBuffered.Rows.Count - 1;

            Grid.Rows[CurrencyManager.Position].IsCurrent = true;

            //qui cerco il primo dataObj non readonly
            foreach (GridViewColumn col in Grid.UIColumns)
            {
                MDataObj dataObj = GetDataObj(rec, col.FieldName);

                if (dataObj != null && !dataObj.ReadOnly)
                {
                    col.IsCurrent = true;
                    break;
                }
            }
        }

        //---------------------------------------------------------------------------
        private void DoBaseAddInsert(bool insert, int insertPos)
        {
			ITBBindingList tbBindingList = Grid.DataSource as ITBBindingList;
            if (tbBindingList != null)
            {
                if (insert && tbBindingList.AllowInsert)
                    tbBindingList.Insert(insertPos);
                else
                    tbBindingList.AddNew();
                return;
            }

            IBindingList bindingList = Grid.DataSource as IBindingList;
            if (bindingList != null)
                bindingList.AddNew();
        }

        //---------------------------------------------------------------------------
        internal bool DoDeleteRow()
        {
            //se non c'e' il DBT allora segue il suo comportamento di default
            if (DBTSlaveBuffered == null)
                return true;

            if (Grid.ReadOnly || !(Document.FormMode == FormModeType.New || Document.FormMode == FormModeType.Edit))
                return false;

            if (DBTSlaveBuffered != null && CurrencyManager.Position < DBTSlaveBuffered.RowsCount)
                return DBTSlaveBuffered.DeleteRow(CurrencyManager.Position);
            return false;
        }

        //---------------------------------------------------------------------------
        void Grid_CellClick(object sender, GridViewCellEventArgs e)
        {
            if (!Grid.ReadOnly || e.RowIndex < 0 || ((mouseButtonClicked & MouseButtons.Left) != MouseButtons.Left))
                return;

            MDataObj dataObj = GetDataObj(e.Column.FieldName);

            MHotLink hotlink = null;
            if (dataObj == null || (hotlink = dataObj.CurrentHotLink as MHotLink) == null)
            {
                return;
            }
            hotlink.RefreshHyperLink(dataObj);
            hotlink.FollowHyperLink(null, true);
        }

        //---------------------------------------------------------------------------
        void ResetPrototypeHotlink(MDataObj currentDataObj)
        {
            if (this.DataBinding != null && currentDataObj != null)
            {
                MDBTSlaveBuffered dbt = this.DataBinding.Data as MDBTSlaveBuffered;
                if (dbt != null)
                {
                    MDataObj obj = dbt.Record.GetData(currentDataObj.Name) as MDataObj; ;
                    if (obj.CurrentHotLink != null)
                    {
                        obj.CurrentHotLink = currentDataObj.CurrentHotLink;
                        currentDataObj.CurrentHotLink = null;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        void SetPrototypeHotlinkTo(MDataObj currentDataObj)
        {
            if (this.DataBinding != null && currentDataObj != null)
            {
                MDBTSlaveBuffered dbt = this.DataBinding.Data as MDBTSlaveBuffered;
                if (dbt != null)
                {
                    MDataObj obj = dbt.Record.GetData(currentDataObj.Name) as MDataObj; ;
                    if (obj.CurrentHotLink != null)
                    {
                        currentDataObj.CurrentHotLink = obj.CurrentHotLink;
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        void Grid_CellBeginEdit(object sender, GridViewCellCancelEventArgs e)
        {
            UIGridEditor editor = e.ActiveEditor as UIGridEditor;
            if (editor == null)
            {
                return;
            }
            MDataObj dataObj = GetDataObj(Grid.CurrentCell.ColumnInfo.FieldName);
            if (dataObj == null)
            {
                return;
            }

            if (dataObj.ReadOnly)
            {
                e.Cancel = true;
                return;
            }


            SetPrototypeHotlinkTo(dataObj);

            if (editor.TBCui != null)
            {
                //era in cache, gli riattaccho gli eventi
                editor.TBCui.AttachDataObj(dataObj, Document);
                IList<ITBUIExtenderProvider> extenders = GetExtenders(Grid.CurrentCell.ColumnInfo.Name);
                if (extenders != null && extenders.Count > 0)
                {
                    foreach (var extender in extenders)
                    {
                        editor.TBCui.ExtendersManager.AddExtender(extender);
                    }
                }
            }
            else
            {
                //non era in cache, prima inizializzazione
                editor.DataObj = dataObj;
                editor.Document = Document;
                editor.GridController = this;
            }
        }

        //---------------------------------------------------------------------------
        MDataObj GetPrototypeDataObj(string fieldName)
        {
            if (DBTSlaveBuffered == null || DBTSlaveBuffered.Record == null)
                return null;

            PropertyInfo pi = DBTSlaveBuffered.Record.GetType().GetProperty(fieldName);
            if (pi == null)
            {
                Debug.Assert(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, "OOOPS!! Sembra che una colonna della gridView sia bindata a un campo {0} che il Record non contiene!!! ", fieldName));
                return null;
            }
            return pi.GetValue(DBTSlaveBuffered.Record, null) as MDataObj;
        }

        //---------------------------------------------------------------------------
        void Grid_ViewCellFormatting(object sender, CellFormattingEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1 || DBTSlaveBuffered == null)
            {
                return;
            }
            string fieldName = e.Column.FieldName;
            if (String.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            MDataObj prototypeDataObj = GetPrototypeDataObj(fieldName);

            if (DefaultFont == null)
                DefaultFont = e.CellElement.Font;

            if (DefaultForeColor == Color.Empty)
                DefaultForeColor = e.CellElement.ForeColor;

            if (prototypeDataObj.CurrentHotLink != null && Grid.ReadOnly)
            {

                e.CellElement.Font = UnderlineFont;
                e.CellElement.ForeColor = UnderlineForeColor;
            }
            else
            {
                e.CellElement.Font = DefaultFont;
                e.CellElement.ForeColor = DefaultForeColor;
            }
            TBWFCUIGridCacheItem item = cache.GetColumnCache(prototypeDataObj.Name, e.Column.Name);
            if (item != null)
            {
                e.CellElement.Text = TBBinding.FormatData(prototypeDataObj.DataType, item.FormatterProvider, e.CellElement.Value).ToString();
            }
        }

        //---------------------------------------------------------------------------
        void Grid_EditorRequired(object sender, Telerik.WinControls.UI.EditorRequiredEventArgs e)
        {
            string fieldName = Grid.CurrentCell.ColumnInfo.FieldName;
            MDataObj dataObj = GetDataObj(fieldName);
            if (dataObj == null)
            {
                return;
            }

            //Cerco l'editor nella cache
            TBWFCUIGridCacheItem item = cache.GetColumnCache(dataObj.Name, Grid.CurrentCell.ColumnInfo.Name);
            if (item != null)
            {
                if (item.Editor == null)
                {   //Metto l'editor istanziato nella cache
                    IUIGridColumn column = Grid.CurrentCell.ColumnInfo as IUIGridColumn;
                    IUIControl control = Activator.CreateInstance(column.ControlType) as IUIControl;
                    column.InitializeControl(control);
                    item.Editor = new UIGridEditor(control);
                }
                e.Editor = item.Editor;
                return;
            }

            Debug.Assert(false, "OOOPS!! Column editor not found!");
        }

        //---------------------------------------------------------------------------
        private MDataObj GetDataObj(string fieldName)
        {
            CurrencyManager cm = CurrencyManager;
            if (cm == null || cm.Position == -1)
            {
                return null;
            }

            return GetDataObj(cm.Current, fieldName);
        }

        //---------------------------------------------------------------------------
        private static MDataObj GetDataObj(object dataSourceItem, string fieldName)
        {
            if (dataSourceItem == null)
                return null;

            PropertyInfo pi = dataSourceItem.GetType().GetProperty(fieldName);
            if (pi == null)
            {
                return null;
            }

            return pi.GetValue(dataSourceItem, null) as MDataObj;
        }

        //-------------------------------------------------------------------------
        public void SetNumCharsPerLine(string gridColumnName, int numCharsPerLine)
        {
            GridViewDataColumn col = Grid.UIColumns[gridColumnName];

            if (col == null)
                Debug.Assert(false, "Column not found, check the column name!");

            MDataObj prototypeDataObj = GetPrototypeDataObj(col.FieldName);
            if (prototypeDataObj == null)
            {
                prototypeDataObj = GetDataObj(col.FieldName);
            }

            TBWFCUIGridCacheItem item = cache.GetColumnCache(prototypeDataObj.Name, col.Name);
            if (item == null)
            {
                Debug.Assert(false, "OOPS, column is not bound yet!!!. Call AddTbBinding on grid first.");
                return;
            }

            float width = SizeHelper.GetTextWidth(numCharsPerLine, Grid.Font);

            IUIGridColumn column = col as IUIGridColumn;
            float defaultWidth = column.GetColumnWidth(prototypeDataObj, item.FormatterProvider, Grid.Font);
            float f = defaultWidth / width;
            int numLine = (int)Math.Ceiling(f);
            if (numLine > 1)
            {
                col.Width = (int)width;
                Grid.LinesPerRow = numLine;

            }
        }

        //-------------------------------------------------------------------------
        public void SetFormatStyle(string gridColumnName, string formatStyle)
        {
            GridViewDataColumn col = Grid.UIColumns[gridColumnName];

            if (col == null)
                Debug.Assert(false, "Column not found, check the column name!");

            MDataObj prototypeDataObj = GetPrototypeDataObj(col.FieldName);
            if (prototypeDataObj == null)
            {
                prototypeDataObj = GetDataObj(col.FieldName);
            }

            TBWFCUIGridCacheItem item = cache.GetColumnCache(prototypeDataObj.Name, col.Name);
            if (item == null)
            {
                Debug.Assert(false, "OOPS, column is not bound yet!!!. Call AddTbBinding on grid first.");
                return;
            }
            item.FormatterProvider = TBFormatterProvider.Create(formatStyle, Namespace);
        }

        //---------------------------------------------------------------------------
        public void AddExtender(string gridColumnName, ITBUIExtenderProvider extenderProvider)
        {
            List<ITBUIExtenderProvider> extenders = null;
            if (!this.extendersCache.TryGetValue(gridColumnName, out extenders))
            {
                extenders = new List<ITBUIExtenderProvider>();
                this.extendersCache.Add(gridColumnName, extenders);
            }
            extenders.Add(extenderProvider);
        }

        //---------------------------------------------------------------------------
        public IList<ITBUIExtenderProvider> GetExtenders(string gridColumnName)
        {
            List<ITBUIExtenderProvider> extenders = null;
            this.extendersCache.TryGetValue(gridColumnName, out extenders);
            return extenders;
        }

        //---------------------------------------------------------------------------
        protected override void Dispose(bool isDisposed)
        {
            if (isDisposed)
            {
                if (DBTSlaveBuffered != null)
                {
                    DetachDBTEvents();
                }

                Grid.EditorRequired -= new Telerik.WinControls.UI.EditorRequiredEventHandler(Grid_EditorRequired);
                Grid.ViewCellFormatting -= new CellFormattingEventHandler(Grid_ViewCellFormatting);
                Grid.CellBeginEdit -= new GridViewCellCancelEventHandler(Grid_CellBeginEdit);
                Grid.CellClick -= new GridViewCellEventHandler(Grid_CellClick);

                Grid.CellValidating -= new CellValidatingEventHandler(Grid_CellValidating);
                Grid.CellValidated -= new CellValidatedEventHandler(Grid_CellValidated);

                Grid.UserDeletingRow -= new GridViewRowCancelEventHandler(Grid_UserDeletingRow);
                Grid.ContextMenuOpening -= new ContextMenuOpeningEventHandler(Grid_ContextMenuOpening);

                Grid.MouseClick -= new MouseEventHandler(Grid_MouseClick);
                Grid.UIColumns.CollectionChanged -= new NotifyCollectionChangedEventHandler(UIColumns_CollectionChanged);
                // font changed handler detachment
                Grid.FontChanged -= new EventHandler(Grid_FontChanged);
                // create row handler detachment
                Grid.RowHeightChanging -= new RowHeightChangingEventHandler(Grid_RowHeightChanging);

                cache.Dispose();

                IDisposable disposable = null;
                foreach (var extendersList in extendersCache.Values)
                {
                    foreach (var extender in extendersList)
                    {
                        disposable = extender as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                    extendersList.Clear();
                }
                extendersCache.Clear();

                if (underlineFont != null)
                {
                    underlineFont.Dispose();
                    underlineFont = null;
                }

            }

            base.Dispose(isDisposed);
        }

        //---------------------------------------------------------------------------
        private void DetachDBTEvents()
        {
            DBTSlaveBuffered.RemovingRecord -= new EventHandler<RowEventArgs>(DBTSlaveBuffered_RemovingRecord);
            DBTSlaveBuffered.DeletingRow -= new EventHandler<RowEventArgs>(DBTSlaveBuffered_RemovingRecord);

            DBTSlaveBuffered.LoadingData -= new EventHandler<EventArgs>(DBTSlaveBuffered_LoadingData);
            DBTSlaveBuffered.DataLoaded -= new EventHandler<EventArgs>(DBTSlaveBuffered_DataLoaded);

            DBTSlaveBuffered.ReadOnlyChanged -= new EventHandler(DBTSlaveBuffered_ReadOnlyChanged);
        }

        //---------------------------------------------------------------------------
        protected override void DocumentFormModeChanged(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            IMAbstractFormDoc doc = sender as IMAbstractFormDoc;
            if (doc == null)
                return;

            switch (doc.FormMode)
            {
                case FormModeType.Browse:
                case FormModeType.None:
                case FormModeType.Design:
                    ReadOnly = true;
                    break;
                case FormModeType.New:
                case FormModeType.Edit:
                case FormModeType.Find:
                    ReadOnly = false;
                    break;
                default:
                    break;
            }

            Grid.ToolbarContainer.DocumentFormModeChanged(doc);
        }

        //---------------------------------------------------------------------------
        public void ShowRowView()
        {
            UIDynamicView rowView;

            if (formRowView != null && formRowView.Visible)
            {
                formRowView.Activate();
                return;
            }

            formRowView = CreateForm<UIDynamicView>(out rowView);
            rowView.CreateAutomaticControls(DBTSlaveBuffered);
            formRowView.Show();
        }

        //stesso medodo di UIUserControl, ma riportato qui per evitare una risalita nella catena dei parent fino a trovare il UIUserControl, 
        //e per far si che possa funzionare anche se la grid non e' in un UIUserControl
        //-------------------------------------------------------------------------
        protected UIForm CreateForm<T>(out T ctrl) where T : UIUserControl
        {
            UIForm parentForm = (UIForm)Grid.FindForm();
            UIForm form = new UIForm();
            form.Owner = parentForm;

            ctrl = Activator.CreateInstance<T>();
            ctrl.CUI.Document = Document;
            ctrl.CUI.UIManager = (ITBCUIManager)parentForm.CUI;
            ctrl.BindingContext = Grid.BindingContext;
            form.OwnsDocument = false;
            form.Show(ctrl, Document, IntPtr.Zero);

            return form;
        }

        //metodi per dimensionamento righe/celle

        //---------------------------------------------------------------------
        internal void IncreaseRowHeight()
        {
            Grid.LinesPerRow++;
        }

        //---------------------------------------------------------------------
        internal void DecreaseRowHeight()
        {
            Grid.LinesPerRow--;
        }

        //---------------------------------------------------------------------
        public void ClearFilter()
        {
            foreach (GridViewRowInfo row in Grid.Rows)
            {
                GridViewCellInfoCollection cells = row.Cells;

                row.IsVisible = true;

                for (int i = 0; i < Grid.ColumnCount; i++)
                {
                    cells[i].Style.Reset();
                    row.InvalidateRow();
                }
            }
        }

        //---------------------------------------------------------------------
        internal void ApplyFilter()
        {
            
            ClearFilter();

            string searchText = Grid.ToolbarContainer.SearchTextBox.Text;

          
            if (!string.IsNullOrEmpty(searchText))
            {
                foreach (GridViewRowInfo row in Grid.Rows)
                {
                    GridViewCellInfoCollection cells = row.Cells;
                    row.IsVisible = false;

                    for (int i = 0; i < Grid.ColumnCount; i++)
                    {
                        GridViewCellInfo cellInfo = cells[i];
                        GridViewDataColumn currentCol = Grid.UIColumns[i];
                        if (String.IsNullOrWhiteSpace(currentCol.FieldName))
                        {
                            break;
                        }
                        string cellText = cellInfo.Value.ToString();
                        MDataObj prototypeDataObj = GetPrototypeDataObj(currentCol.FieldName);
                        if (prototypeDataObj == null)
                        {
                            prototypeDataObj = GetDataObj(currentCol.FieldName);
                        }

                        if (prototypeDataObj != null)
                        {
                            TBWFCUIGridCacheItem item = cache.GetColumnCache(prototypeDataObj.Name, currentCol.Name);

                            if (item != null)
                            {
                                cellText = TBBinding.FormatData(prototypeDataObj.DataType, item.FormatterProvider, cellInfo.Value).ToString();
                            }
                        }

                        if (cellText.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            row.IsVisible = true;
                            cellInfo.Style.CustomizeFill = true;
                            cellInfo.Style.DrawFill = true;
                            cellInfo.Style.BackColor = Color.FromArgb(201, 252, 254);
                        }
                        else
                        {
                            cellInfo.Style.Reset();
                            row.InvalidateRow();
                        }
                    }
                }
            }
            Grid.MasterTemplate.Refresh();
        }
    }
}
