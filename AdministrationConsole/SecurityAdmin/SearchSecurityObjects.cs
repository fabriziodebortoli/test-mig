using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{

    //=============================================================================
    public partial class SearchSecurityObjectsForm : Form
    {

        public enum SearchProtectionCriteria
		{
            Protected	= 0x0000,		
            Unprotected	= 0x0001,				
            All		    = 0x0002
        }

      
        #region Private Data Member
        private PathFinder                  pathFinder = null;
        private MenuXmlParser               menuXmlParser = null;
        private MenuXmlNodeCollection       lastSearchResults = null;
        private SqlConnection               connection = null;
        private SearchProtectionCriteria    searchProtectionCriteria = SearchProtectionCriteria.All;
        #endregion

        #region indici immagini

        private int runFunctionImageIndex = -1;
        private int reportImageIndex = -1;
        private int textImageIndex = -1;

        private int finderImageIndex = -1;
        private int dataEntryImageIndex = -1;
        private int batchImageIndex = -1;
        
        private int windowsImageIndex = -1;
        private int rowImageIndex = -1;
        private int embeddedviewImageIndex = -1;

        private int tabImageIndex = -1;
        private int tabberImageIndex = -1;

        private int tileImageIndex = -1;
        private int tileManagerImageIndex = -1;

        private int toolbarImageIndex = -1;
        private int toolbarbuttonImageIndex = -1;

        private int hotLinkImageIndex = -1;

        private int tableGroupImageIndex = -1;
        private int tableImageIndex = -1;
        private int viewImageIndex = -1;

        private int gridImageIndex = -1;
        private int columnImageIndex = -1;

        private int singleControlImageIndex = -1;

        private int wordDocumentImageIndex = -1;
        private int excelDocumentImageIndex = -1;
        private int wordTemplateImageIndex = -1;
        private int excelTemplateImageIndex = -1;

        #endregion

        public delegate void SearchSecurityObjectsEventHandler(object sender, MenuXmlNode node);
        public event SearchSecurityObjectsEventHandler SelectFoundSecurityObject;

        #region Constructors
        //-------------------------------------------------------------------------
        public SearchSecurityObjectsForm(PathFinder aPathFinder, 
                                        MenuXmlParser aMenuXmlParser,  
                                        SqlConnection aConnection,
                                        System.Windows.Forms.Form ownerForm)
        {
            InitializeComponent();

            connection = aConnection;
            pathFinder = aPathFinder;
            menuXmlParser = aMenuXmlParser;
            this.Owner = ownerForm;

            LoadObjectImage();
            FillObjectTypesListView();

            this.SearchProtectedComboBox.Items.Add(Strings.AllObjectsType);
            this.SearchProtectedComboBox.Items.Add(Strings.Protected);
            this.SearchProtectedComboBox.Items.Add(Strings.Unprotected);

            this.SearchProtectedComboBox.SelectedIndex = 0;

        }

        //-------------------------------------------------------------------------
        public SearchSecurityObjectsForm(PathFinder aPathFinder, MenuXmlParser aMenuXmlParser, SqlConnection aConnection)
            : this(aPathFinder, aMenuXmlParser, aConnection, null)
		{
        }
        #endregion

        //---------------------------------------------------------------------
        private void LoadObjectImage()
        {

            //imageIndex
            ImageList imageListObjects = new ImageList();
            imageListObjects.TransparentColor = Color.Magenta;
            
            MenuMngWinCtrl menuMngWinCtrl = new MenuMngWinCtrl();
            Assembly menuMngWinCtrlAssembly = menuMngWinCtrl.GetType().Assembly;
            string menuMngWinCtrlBitmapsNamespace = menuMngWinCtrl.GetType().Namespace + ".Bitmaps.";

            Stream imageStream;

            //Batch
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunBatch.bmp");
            if (imageStream != null)
                batchImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //DataEntry
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunDocument.bmp");
            if (imageStream != null)
                dataEntryImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Funzione
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunFunction.bmp");
            if (imageStream != null)
                runFunctionImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Report
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunReport.bmp");
            if (imageStream != null)
                reportImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Testo
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunText.bmp");
            if (imageStream != null)
                textImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            Assembly securityAssembly = Assembly.GetExecutingAssembly();
            //Ricercatore (dove lo trovo???)
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.FINDER.BMP");
            if (imageStream != null)
                finderImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //HotLink
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.RADAR.BMP");
            if (imageStream != null)
                hotLinkImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //View di DB)
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.VIEW.bmp");
            if (imageStream != null)
                viewImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Table di DB
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.TABLE.BMP");
            if (imageStream != null)
                tableGroupImageIndex = tableImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Controllo schede
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.tab.bmp");
            if (imageStream != null)
                tabImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Colonna di controllo griglia 
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.column.bmp");
            if (imageStream != null)
                columnImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Controllo griglia
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.grid.bmp");
            if (imageStream != null)
                gridImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            
            //Finestra di dettaglio
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.ROW.bmp");
            if (imageStream != null)
                rowImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Controllo
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.singleControls.bmp");
            if (imageStream != null)
                singleControlImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Documento Word
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunWordDocument.bmp");
            if (imageStream != null)
                wordDocumentImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Foglio Excel
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunExcelDocument.bmp");
            if (imageStream != null)
                excelDocumentImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Template Word
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunWordTemplate.bmp");
            if (imageStream != null)
                wordTemplateImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Template Excel
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunExcelTemplate.bmp");
            if (imageStream != null)
                excelTemplateImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

          
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.tabber.bmp");
            if (imageStream != null)
                tabberImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.tileManager.bmp");
            if (imageStream != null)
                tileManagerImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);


            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.Tile.bmp");
            if (imageStream != null)
                tileImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.ToolBar.bmp");
            if (imageStream != null)
                toolbarbuttonImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Template Word
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.ToolbarButton.bmp");
            if (imageStream != null)
                wordTemplateImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //Template Excel
            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.EmbeddedView.bmp");
            if (imageStream != null)
                embeddedviewImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            SearchResultsListView.SmallImageList = imageListObjects;
        }

        //---------------------------------------------------------------------
        public void FillObjectTypesListView()
        {
            FilteredByComboBox.Items.Clear();

            if (connection == null)
                return;

            if (connection.State != ConnectionState.Open)
                connection.Open();

            SqlCommand mySqlCommand = null;
            SqlDataReader myReader = null;

            try
            {
                //@@TODO Da vedere con Ricky la questione constraint (Type 9)
                string sSelect = @"SELECT Type, TypeName FROM MSD_ObjectTypes 
								WHERE Type <> 9 ORDER BY TypeName";

                mySqlCommand = new SqlCommand(sSelect, connection);

                myReader = mySqlCommand.ExecuteReader();

                FilteredByComboBox.Items.Add(Strings.All);

                while (myReader.Read())
                {
                    int objectType = Convert.ToInt32(myReader["Type"]);
                    FilteredByComboBox.Items.Add(ControlsString.GetControlDescription((SecurityType)objectType));
                }

                FilteredByComboBox.SelectedIndex = 0;
            }
                
            catch (SqlException err)
            {
                DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
            }
            finally
            {
                if (myReader != null && !myReader.IsClosed)
                    myReader.Close();
                if (mySqlCommand != null)
                    mySqlCommand.Dispose();
            }
        }

        //-------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.PerformLayout();
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        private void SearchCriteriaComboBox_TextChanged(object sender, System.EventArgs e)
        {
            SearchButton.Enabled = (SearchCriteriaComboBox.Text != null && SearchCriteriaComboBox.Text.Length > 0);
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        private void SearchButton_Click(object sender, System.EventArgs e)
        {
            this.SearchResultsListView.Items.Clear();

            if (SearchCriteriaComboBox.Text == null || SearchCriteriaComboBox.Text.Trim().Length == 0)
                return;

            SearchButton.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            MenuSearchEngine appsSearchEngine = null;

            if (this.SearchProtectedComboBox.SelectedIndex != 0)
                searchProtectionCriteria = (SearchProtectionCriteria)this.SearchProtectedComboBox.SelectedIndex;

            try
            {
                if (lastSearchResults == null)
                    lastSearchResults = new MenuXmlNodeCollection();

                appsSearchEngine = new MenuSearchEngine(pathFinder, menuXmlParser);

                //Setto i parametri per la ricerca
                appsSearchEngine.CaseSensitive = MatchCaseCheckBox.Checked;
                appsSearchEngine.ExactWord = SearchExactWordsCheckBox.Checked;
                appsSearchEngine.SearchDescriptions = !SearchInTitlesOnlyCheckBox.Checked;
                appsSearchEngine.SearchItemObjects = SearchInNamespaceCheckBox.Checked;
                appsSearchEngine.SearchInPreviousResult = SearchInPreviousResultsCheckBox.Checked;
                
                if (appsSearchEngine.SearchInPreviousResult && lastSearchResults != null)
                    appsSearchEngine.LastResults = lastSearchResults;

                SetExtractionTypes(ref appsSearchEngine);

                appsSearchEngine.SearchExpression(SearchCriteriaComboBox.Text);
                lastSearchResults = appsSearchEngine.LastResults;

                

            }
            catch (MenuXmlParserException exception)
            {
                MessageBox.Show(this, String.Format(Strings.MenuSearchFailedMessageText, exception.Message), Strings.MenuSearchFailedMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception)
            {
                MessageBox.Show(this, Strings.MenuSearchGenericFailureMessageText, Strings.MenuSearchFailedMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor.Current = Cursors.Default;
            }

           
            SearchButton.Enabled = true;

            FillSearchResultsListView(ref appsSearchEngine);

            if (lastSearchResults == null || lastSearchResults.Count == 0)
            {
                SearchInPreviousResultsCheckBox.Checked = false;
                SearchInPreviousResultsCheckBox.Enabled = false;
                MessageBox.Show(Strings.NotFind, Strings.SearchResult, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
                SearchInPreviousResultsCheckBox.Enabled = true;

            if (SearchCriteriaComboBox.FindStringExact(SearchCriteriaComboBox.Text.Trim()) == -1)
                SearchCriteriaComboBox.Items.Add(SearchCriteriaComboBox.Text.Trim());

            Cursor.Current = Cursors.Default;
            
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        private void SetExtractionTypes(ref MenuSearchEngine appsSearchEngine)
        { 
            string selectedType = FilteredByComboBox.Text;

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.All))) == 0 || string.IsNullOrEmpty(selectedType))
            {
                    appsSearchEngine.ExtractAll = true;
                    return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.Batch)))==0)
            {
                    appsSearchEngine.ExtractBatches = true;
                    return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.DataEntry)))==0)
            {
                    appsSearchEngine.ExtractDocuments = true;
                     return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.Function)))==0)
            {
                    appsSearchEngine.ExtractFunctions = true;
                      return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.Executable)))==0)
            {
                   appsSearchEngine.ExtractExecutables = true;
                   return;
            }

            if (string.Compare(selectedType,ControlsString.GetControlDescription((SecurityType.Text)))==0)
            {
                    appsSearchEngine.ExtractTexts = true;
                       return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.Report)))==0)
            {
                    appsSearchEngine.ExtractReports = true;
                    return;
            }

            if (string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.WordDocument))) == 0 ||
                string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.WordTemplate))) == 0 ||
                string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.ExcelDocument))) == 0 ||
                string.Compare(selectedType, ControlsString.GetControlDescription((SecurityType.ExcelTemplate))) == 0)
            {
                appsSearchEngine.ExtractOfficeItems = true;
                return;
            }

            appsSearchEngine.ExtractExternalItems = true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillSearchResultsListView(ref MenuSearchEngine appsSearchEngine)
        {
            SearchResultsListView.Items.Clear();

            if (lastSearchResults != null && lastSearchResults.Count > 0)
            {
                foreach (MenuXmlNode aCommandNode in lastSearchResults)
                {
                    if ((appsSearchEngine.ExtractExternalItems && !appsSearchEngine.ExtractAll) &&
                        string.Compare(aCommandNode.ExternalItemType, ControlsString.GetSecurityTypeFromControlDescription(this.FilteredByComboBox.Text).ToString()) != 0 )
                        continue;
                    ListViewItem item = new ListViewItem();
                    item.Text = aCommandNode.Title;
                    item.SubItems.Add(aCommandNode.ItemObject);
                    item.ImageIndex = GetImageIndex(aCommandNode);
                    item.Tag = aCommandNode;

                    if (searchProtectionCriteria != SearchProtectionCriteria.All)
                    {
                        if (!IsSecurityCriteria(aCommandNode))
                            continue;
                    }
                    
                    SearchResultsListView.Items.Add(item);
                }
            }
        }

        //---------------------------------------------------------------------
        private bool IsSecurityCriteria(MenuXmlNode aCommandNode)
        {
            int objectTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(aCommandNode, connection);
            int objectId = CommonObjectTreeFunction.GetObjectId(aCommandNode.ItemObject, objectTypeFromDataBase, connection);

            if (objectId == -1)
                return false;

            bool isProtected = ImportExportFunction.IsProtected(aCommandNode.ItemObject, objectTypeFromDataBase, connection);

            if (isProtected && searchProtectionCriteria != SearchProtectionCriteria.Protected)
                return false;


            if (!isProtected && searchProtectionCriteria != SearchProtectionCriteria.Unprotected)
                return false;

            return true;

        }

        //---------------------------------------------------------------------
        public int GetImageIndex(MenuXmlNode aCommandNode)
        {
            if (aCommandNode.IsRunFunction)
                return runFunctionImageIndex;
             
             if (aCommandNode.IsRunReport)
                return reportImageIndex;
            
             if (aCommandNode.IsRunDocument)
                return dataEntryImageIndex;

            if (aCommandNode.IsRunBatch)
                return batchImageIndex;

            if (aCommandNode.IsRunText)
                return textImageIndex;
            
             if (aCommandNode.IsExternalItem)
             {
                 switch ((SecurityType)Enum.Parse(typeof(SecurityType), aCommandNode.ExternalItemType))
                 {
                     case SecurityType.ChildForm:
                         return windowsImageIndex;
                     case SecurityType.EmbeddedView:
                         return embeddedviewImageIndex;
                     case SecurityType.RowView:
                         return rowImageIndex;

                     case SecurityType.Tab:
                         return tabImageIndex;
                     case SecurityType.Tabber:
                         return tabberImageIndex;

                     case SecurityType.Tile:
                         return tileImageIndex;

                     case SecurityType.TileManager:
                         return tileManagerImageIndex;

                     case SecurityType.Toolbar:
                         return toolbarImageIndex;
                     case SecurityType.ToolbarButton:
                         return toolbarbuttonImageIndex;

                     case SecurityType.Table:
                         return tableImageIndex;

                     case SecurityType.View:
                         return viewImageIndex;

                     case SecurityType.HotKeyLink:
                         return hotLinkImageIndex;

                     case SecurityType.Grid:
                         return gridImageIndex;

                     case SecurityType.GridColumn:
                         return columnImageIndex;

                     case SecurityType.Control:
                         return singleControlImageIndex;

                     case SecurityType.Finder:
                         return finderImageIndex;
                    case SecurityType.PropertyGrid:
                        return gridImageIndex;
                        //TODO LARA
                }
             }

             if (aCommandNode.IsWordDocument || aCommandNode.IsWordDocument2007)
                 return wordDocumentImageIndex;

             if (aCommandNode.IsExcelDocument || aCommandNode.IsExcelDocument2007)
                 return excelDocumentImageIndex;

            if (aCommandNode.IsWordTemplate || aCommandNode.IsWordTemplate2007)
                 return wordTemplateImageIndex;
            
            if (aCommandNode.IsExcelTemplate || aCommandNode.IsExcelTemplate2007)
                 return excelTemplateImageIndex;

            return -1;
        }

        //--------------------------------------------------------------------
        private void SearchResultsListView_DoubleClick(object sender, EventArgs e)
        {
            if (SearchResultsListView.SelectedItems == null)
                return;

            if (SearchResultsListView.SelectedItems[0].Tag == null || !(SearchResultsListView.SelectedItems[0].Tag is MenuXmlNode))
                return;

            if (SelectFoundSecurityObject != null)
                SelectFoundSecurityObject(this, (MenuXmlNode)SearchResultsListView.SelectedItems[0].Tag);

        }

        //---------------------------------------------------------------------
        private void SearchResultsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SearchResultsListView.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        //=========================================================================
        class ListViewItemComparer : IComparer
        {
            private int col;

            //---------------------------------------------------------------------
            public ListViewItemComparer()
            {
                col = 0;
            }

            //---------------------------------------------------------------------
            public ListViewItemComparer(int column)
            {
                col = column;
            }

            //---------------------------------------------------------------------
            public int Compare(object x, object y)
            {

                if (col == 0)
                {

                }

                ////Se é il resoconto dei files parsati lo lascio in fondo
                //if (string.Compare(((ListViewItem)y).SubItems[col].Text, Strings.SummaryCheckCompleteMessage) == 0)
                //    return 0;

                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);

            }
        }

    }
}