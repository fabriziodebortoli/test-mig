using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Microarea.TaskBuilderNet.Forms.Designers;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Themes;
using Telerik.WinControls.Layouts;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    /// <summary>
    /// ToolbarMode, it can be one or more elements combination, determines the elements
    /// shown in toolbar
    /// </summary>
    [Flags]
    public enum ToolbarModes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 1,
        /// <summary>
        /// Add row button
        /// </summary>
        AddRowButton = None << 1,
        /// <summary>
        /// Remove row button
        /// </summary>
        RemoveRowButton = AddRowButton << 1,
        /// <summary>
        /// Alternate color button
        /// </summary>
        AlternateColorButton = RemoveRowButton << 1,
        /// <summary>
        /// row view button
        /// </summary>
        RowViewButton = AlternateColorButton << 1,
        /// <summary>
        /// Adjust height button
        /// </summary>
        AdjustHeightButton = RowViewButton << 1,
        /// <summary>
        /// Find section
        /// </summary>
        FindSection = AdjustHeightButton << 1,
        /// <summary>
        /// All buttons + Find section
        /// </summary>
        All = (FindSection << 1) - 1
    }

    //=========================================================================
    public partial class UIGrid : RadGridView, IUIContainer
    {
        TBWFCUIGrid cui;
        UIGridToolbarContainer toolbarContainer;
        ToolbarModes toolbarShowMode;
        int linesPerRow;

        public int RowHeight
        {
            get { return TableElement.RowHeight; }
            set
            {
                TableElement.RowHeight = value;
            }
        }
        public int LinesPerRow
        {
            get { return linesPerRow; }

            set
            {
                if (value < 1)
                    return;
                linesPerRow = value;
                RowHeight = MinRowHeight * linesPerRow;
            }
        }

        /// <summary>
        /// Minimum grid row height.
        /// </summary>
        private int m_iMinRowHeight = 0;

        public int MinRowHeight
        {
            get { return m_iMinRowHeight; }
            set
            {
                if (m_iMinRowHeight != value)
                {
                    m_iMinRowHeight = value;

                    this.TableElement.RowHeight = m_iMinRowHeight;
                    // set the new minimum row height to the already created rows.
                    GridViewRowCollection oCollection = this.Rows;
                    foreach (GridViewDataRowInfo oRowInfo in oCollection)
                    {
                        oRowInfo.MinHeight = MinRowHeight;
                    }

                }
            }
        }

        //---------------------------------------------------------------------
        public ToolbarModes ToolbarShowMode
        {
            get { return toolbarShowMode; }
            set
            {
                toolbarShowMode = value;
                UpdateToolbar();
            }
        }

        //---------------------------------------------------------------------
        public UIGridToolbarContainer ToolbarContainer
        {
            get { return toolbarContainer; }
        }

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui as TBWFCUIGrid; } }

        //---------------------------------------------------------------------
        public bool AlternateColor
        {
            get { return EnableAlternatingRowColor; }
            set
            {
                TableElement.AlternatingRowColor = TBThemeManager.Theme.GridAlternateColor;
                EnableAlternatingRowColor = value;
            }
        }

        //---------------------------------------------------------------------
        public UIGrid()
        {
            cui = new TBWFCUIGrid(this);
            ThemeClassName = typeof(RadGridView).ToString();
            InitializeComponent();
            CreateDefaultToolbar();
            ToolbarShowMode = ToolbarModes.All;
            AutoSizeRows = false;
            this.AllowRowResize = false;
            MasterTemplate.BestFitColumns(BestFitColumnMode.DisplayedCells);
            MasterTemplate.HorizontalScrollState = ScrollState.AutoHide;
            MasterTemplate.VerticalScrollState = ScrollState.AutoHide;
            
            LinesPerRow = 1;
        }

        /// <summary>
        /// The extra vertical space for each row, i.e. vertical padding and borders.
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------
        internal int GetExtraVerticalSpace()
        {
            // TODO: This method should be also defined in a IUIGrid interface so that 
            // the controller TBWFCUIGrid can reference it.
            int borders = (int)(4 * (this.TableElement.Padding.Vertical +
                this.TableElement.BorderTopWidth + this.TableElement.BorderBottomWidth));
            return borders;
        }

        //---------------------------------------------------------------------
        private void UpdateToolbar()
        {
            ClearToolbar();

            if (toolbarShowMode == ToolbarModes.None)
            {
                GridViewElement.Panel.Children.Remove(toolbarContainer);
                return;
            }
            if ((ToolbarShowMode & ToolbarModes.AddRowButton) == ToolbarModes.AddRowButton)
            {
                toolbarContainer.AddToolbarButton("New", Resources.InsertRowGridContextMenuItem, (sender, args) => { cui.DoAddInsertRow(true); }, Resources.ToolbarGrid_NewRow, true);
            }
            if ((ToolbarShowMode & ToolbarModes.RemoveRowButton) == ToolbarModes.RemoveRowButton)
            {
                toolbarContainer.AddToolbarButton("Delete", Resources.Grid_DeleteRow, (sender, args) => { cui.DeleteSelectedRows(); }, Resources.ToolbarGrid_DeleteRow, true);
            }
            if ((ToolbarShowMode & ToolbarModes.AlternateColorButton) == ToolbarModes.AlternateColorButton)
            {
                toolbarContainer.AddToolbarToggleButton("Alternate", Resources.Grid_AlternateColor, (sender, args) => { AlternateColor = !AlternateColor; }, Resources.ToolbarGrid_AlternateColor);
            }
            if ((ToolbarShowMode & ToolbarModes.RowViewButton) == ToolbarModes.RowViewButton)
            {
                toolbarContainer.AddToolbarButton("Row", Resources.Grid_RowView, (sender, args) => { cui.ShowRowView(); }, Resources.ToolbarGrid_RowView);
            }
            if ((ToolbarShowMode & ToolbarModes.AdjustHeightButton) == ToolbarModes.AdjustHeightButton)
            {
                toolbarContainer.AddToolbarButton("IncreaseRowHeight", Resources.Grid_IncreaseRowHeight, (sender, args) => { cui.IncreaseRowHeight(); }, Resources.ToolbarGrid_IncreaseRowHeight);
                toolbarContainer.AddToolbarButton("DecreaseRowHeight", Resources.Grid_DecreaseRowHeight, (sender, args) => { cui.DecreaseRowHeight(); }, Resources.ToolbarGrid_DecreaseRowHeight);
            }
            if ((ToolbarShowMode & ToolbarModes.FindSection) == ToolbarModes.FindSection)
            {
                toolbarContainer.AddFinderSection();
                toolbarContainer.SearchTextBox.TextChanged += new EventHandler(SearchTextBox_TextChanged);
            }
        }

        //---------------------------------------------------------------------
        private void ClearToolbar()
        {
            toolbarContainer.Clear();
            if (toolbarContainer.SearchTextBox != null)
            {
                toolbarContainer.SearchTextBox.TextChanged -= new EventHandler(SearchTextBox_TextChanged);
            }
        }

        //Create the default  toolbar      
        //---------------------------------------------------------------------
        private void CreateDefaultToolbar()
        {
            TableElement.StretchVertically = false;
            toolbarContainer = new UIGridToolbarContainer(this);
            GridViewElement.Panel.Children.Insert(0, toolbarContainer);
            toolbarContainer.SetValue(DockLayoutPanel.DockProperty, Telerik.WinControls.Layouts.Dock.Top);
        }

        //---------------------------------------------------------------------
        public void AddToolbarButton(string name, string tooltip, EventHandler action, Image image = null, bool formModeChangeSensitive = false)
        {
            toolbarContainer.AddToolbarButton(name, tooltip, action, image, formModeChangeSensitive);
            toolbarContainer.DocumentFormModeChanged(CUI.Document);
        }

        //---------------------------------------------------------------------
        public void AddToolbarToggleButton(string name, string tooltip, EventHandler action, Image image = null, bool formModeChangeSensitive = false)
        {
            toolbarContainer.AddToolbarToggleButton(name, tooltip, action, image, formModeChangeSensitive);
            toolbarContainer.DocumentFormModeChanged(CUI.Document);
        }

        //---------------------------------------------------------------------
        public void ShowToolbarButton(string name, bool visible)
        {
            toolbarContainer.ShowToolbarButton(name, visible);
        }

        //---------------------------------------------------------------------
        public void EnableToolbarButton(string name, bool enable)
        {
            toolbarContainer.EnableToolbarButton(name, enable);
        }

        //---------------------------------------------------------------------
        void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            cui.ApplyFilter();
        }

        //---------------------------------------------------------------------
        public void AddTbBinding(IDataManager dataManager)
        {
            cui.AddTbBinding(this, dataManager);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (toolbarContainer != null && toolbarContainer.SearchTextBox != null)
                    toolbarContainer.SearchTextBox.TextChanged -= new EventHandler(SearchTextBox_TextChanged);

                if (cui != null)
                {
                    cui.Dispose();
                    cui = null;
                }
            }
        }

        //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return toolbarContainer.ChildrenElements; }
        }


        [Browsable(true)]
        [Editor(typeof(UIGridColumnsDesigner), typeof(UITypeEditor))]
        //[Obsolete("do not use this property to perform an AddRange operation. Use AddColumnsRange(IList columnsCollection) instead")]
        //-----------------------------------------------------------------------------------------
        public GridViewColumnCollection UIColumns
        {
            // TODO: I would use an inner collection storing references to 
            // IUIGridColumn so that we can reference just such a type.
            // I would expect to not need all GridViewColumnCollection 
            // methods. This method could return a ReadOnlyCollection<T>.
            get { return base.Columns; }
        }

        /// <summary>
        /// Use this to add a range of columns, as the native UIColumns.AddRange(IList)
        /// does not fire the collectionChanging event.
        /// </summary>
        /// <param name="columnsCollection"></param>
        //-----------------------------------------------------------------------------------------
        public void AddColumnsRange(IList columnsCollection)
        {
            if (columnsCollection != null)
            {

                // columnsCollection == null should not happen.
                // We are we guaranteed the columns 
                // collection is always != null by telerik as such a 
                // collection is instantiated and managed by them.
                foreach (GridViewDataColumn item in columnsCollection)
                {
                    // TODO: shouldn't we check for IUIGridColumn type as well?
                    base.MasterTemplate.Columns.Add(item);
                }
            }
        }

        [Browsable(false)]
        [Obsolete("do not use Items, use UIColumns instead")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //-----------------------------------------------------------------------------------------
        public new GridViewColumnCollection Columns
        {
            get { return base.Columns; }
        }

    }
}
