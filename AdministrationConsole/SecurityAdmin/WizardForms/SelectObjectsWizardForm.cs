using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
    //=================================================================================================================
	public partial class SelectObjectsWizardForm : InteriorWizardPage
	{

		#region DataMenber Privati

		private WizardParameters	wizardParameters	= null;
		private int					totTypeObjects		= 0;
		private bool				allObjects			= true;

        private SecurityObjectsImages soi = new SecurityObjectsImages();

		private System.Windows.Forms.Button AllObjectsButton;

		#endregion

		#region Costruttore
		//-------------------------------------------------------------------------------------------------------------
		public SelectObjectsWizardForm()
		{
			InitializeComponent();			
		}
		#endregion

		#region form attiva quindi alla visualizzazione

        //-------------------------------------------------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
 
			wizardParameters = ((SecurityWizardManager)this.WizardManager).GetImportSelections();

			if (wizardParameters == null)
				return false;
			
			if (wizardParameters.ObejctTypeArrayList == null || wizardParameters.ObejctTypeArrayList.Count ==0)
			{
				InitializeMyControls();
				SelectAllObjectType(true);
				this.WizardForm.SetWizardButtons(WizardButton.Back);
				this.AllObjectsButton.Text = Strings.UnselectAll;
			}
			else
			{
				LoadOldSelections();
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			}
			
			return true;
		}
		

		#endregion

		#region funzioni d'inizializzazione
        //-------------------------------------------------------------------------------------------------------------
		private void LoadOldSelections()
		{
			if (wizardParameters.ObjectSelectionType == WizardParametersType.AllObjects)
			{
				AllObjectsButton.Text = Strings.UnselectAll;
				allObjects = true;
				return;
			}

			foreach (ListViewItem itemOfArray in wizardParameters.ObejctTypeArrayList)
			{
				foreach (ListViewItem item in this.ObjectTypesListView.Items)
				{
					if (itemOfArray.Tag == item.Tag)
						item.Checked = true;
				}
			}

			AllObjectsButton.Text = Strings.SelectAll;
			allObjects = false;
		}

        //-------------------------------------------------------------------------------------------------------------
		private void InitializeMyControls()
		{
            this.m_titleLabel.Text = WizardStringMaker.GetSelectObjectOperationTitle(wizardParameters.OperationType);
            this.m_subtitleLabel.Text = WizardStringMaker.GetSelectObjectOperationDescription(wizardParameters.OperationType);
			LoadObjectImage();	
			FillObjectTypesListView();
		}

        //-------------------------------------------------------------------------------------------------------------
		private void LoadObjectImage()
		{
			ObjectTypesListView.SmallImageList = soi.imageListObjects;
		}

        //-------------------------------------------------------------------------------------------------------------
		public void FillObjectTypesListView() 
		{
			ObjectTypesListView.Items.Clear();

            SqlCommand mySqlCommand = null;
            SqlDataReader myReader = null;

            try
            {
                //@@TODO Da vedere con Ricky la questione constraint (Type 9)
                string sSelect = @"SELECT TypeId, Type FROM MSD_ObjectTypes 
								WHERE Type  = 3 OR Type  = 4 OR Type  = 5 OR Type  = 7 OR Type  = 17 OR Type  = 10 ORDER BY TypeName";

                mySqlCommand = new SqlCommand(sSelect, wizardParameters.ShowObjectsTreeForm.Connection);

                myReader = mySqlCommand.ExecuteReader();

                while (myReader.Read())
                {
                    int objectTypeId = Convert.ToInt32(myReader["TypeId"]);
					int objectType   = Convert.ToInt32(myReader["Type"]);
                    ListViewItem objectTypeItem = ObjectTypesListView.Items.Add(new ListViewItem(ControlsString.GetControlDescription((SecurityType)objectType)));
					objectTypeItem.Tag = objectTypeId;
					objectTypeItem.Checked = true;
					objectTypeItem.ImageIndex = soi.GetImageIndex(objectType);
					totTypeObjects  =  totTypeObjects +1;
                }
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

		#endregion

		#region eventi sui Check e sulla checkBox tutti gli oggetti
        //-------------------------------------------------------------------------------------------------------------
		private void SelectAllObjectType(bool check)
		{
			if (check)
				wizardParameters.ObejctTypeArrayList.Clear();

			foreach (ListViewItem item in ObjectTypesListView.Items)
				item.Checked = check;

			allObjects = check;
		}

        //-------------------------------------------------------------------------------------------------------------
		private void ObjectTypesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{

			int tot = ObjectTypesListView.CheckedItems.Count;
			
			if (  e.NewValue == CheckState.Unchecked)
				tot = tot -1;
			else
				tot = tot +1;

			if (totTypeObjects == tot) 
			{
				AllObjectsButton.Text = Strings.UnselectAll;
				allObjects = true;
			}
			else
			{
				AllObjectsButton.Text = Strings.SelectAll;
				allObjects = false;
			}

			if (ObjectTypesListView.CheckedItems.Count <= 1 && e.NewValue == CheckState.Unchecked)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}
		
		#endregion

		#region form disattiva quindi quando clicco su avanti

        //-------------------------------------------------------------------------------------------------------------
        public override bool OnKillActive()
		{
			if (wizardParameters == null)
				wizardParameters = new WizardParameters();

			if (wizardParameters.ObejctTypeArrayList == null)
				wizardParameters.ObejctTypeArrayList = new ArrayList();
			else
				wizardParameters.ObejctTypeArrayList.Clear();

            foreach (ListViewItem item in ObjectTypesListView.Items)
            {
                if (item.Checked)
                    wizardParameters.ObejctTypeArrayList.Add(item);
                else if (wizardParameters.ObejctTypeArrayList.Contains(item))
                    wizardParameters.ObejctTypeArrayList.Remove(item);
            }
			if (this.allObjects)
				wizardParameters.ObjectSelectionType = WizardParametersType.AllObjects;
			else
				wizardParameters.ObjectSelectionType = WizardParametersType.SelectObjectsType;

			return base.OnKillActive();
		}

		#endregion

        //-------------------------------------------------------------------------------------------------------------
		private void SelectObjectsWizardForm_Load(object sender, System.EventArgs e)
		{
		
		}

        //-------------------------------------------------------------------------------------------------------------
		private void AllObjectsButton_Click(object sender, System.EventArgs e)
		{
			if (!allObjects)
				SelectAllObjectType(true);
			else
				SelectAllObjectType(false);
		}

	}

    //=================================================================================================================
    public class SecurityObjectsImages
    {
        #region indici immagini

        public int runFunctionImageIndex = -1;
        public int reportImageIndex = -1;
 //       public int textImageIndex = -1;
        public int dataEntryImageIndex = -1;
        //public int windowsImageIndex = -1;
        public int batchImageIndex = -1;
 //       public int tabImageIndex = -1;
        public int tableImageIndex = -1;
        //public int hotLinkImageIndex = -1;
        //public int tableGroupImageIndex = -1;
        //public int viewImageIndex = -1;
        //public int rowImageIndex = -1;
        //public int gridImageIndex = -1;
        //public int columnImageIndex = -1;
        public int singleControlImageIndex = -1;
        //public int finderImageIndex = -1;
        //public int wordDocumentImageIndex = -1;
        //public int excelDocumentImageIndex = -1;
        //public int wordTemplateImageIndex = -1;
        //public int excelTemplateImageIndex = -1;

        //public int tabberImageIndex = -1;
        //public int tileManagerImageIndex = -1;
        //public int tileImageIndex = -1;
        //public int toolBarImageIndex = -1;
        //public int toolBarButtonImageIndex = -1;
        //public int embeddedViewImageIndex = -1;

        public int dummyImageIndex = -1;
        public int appImageIndex = -1;
        public int groupImageIndex = -1;
        public int menuImageIndex = -1;

        #endregion

        public ImageList imageListObjects = new ImageList();

        //-------------------------------------------------------------------------------------------------------------
        public SecurityObjectsImages()
        {
            PrepareObjectImageList();
        }

        //-------------------------------------------------------------------------------------------------------------
        private void PrepareObjectImageList()
        {
            //imageIndex
            imageListObjects.TransparentColor = Color.Magenta;

            Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Dummy d = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Dummy();
            Assembly menuMngWinCtrlAssembly = d.GetType().Assembly;
            string menuMngWinCtrlBitmapsNamespace = d.GetType().Namespace + ".Bitmaps.";

            Stream imageStream;

             //----
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "DummyState.bmp");
            if (imageStream != null)
                dummyImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
          
            imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "ExpandedMenu.bmp");
            if (imageStream != null)
                menuImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

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
            ////Testo
            //imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunText.bmp");
            //if (imageStream != null)
            //    textImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //
            ////Ricercatore (dove lo trovo???)
            //imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.FINDER.BMP");
            //if (imageStream != null)
            //    finderImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            ////HotLink
            //imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.RADAR.BMP");
            //if (imageStream != null)
            //    hotLinkImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            //imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.VIEW.bmp");
            //if (imageStream != null)
            //    viewImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //Table di DB

            Assembly securityAssembly = Assembly.GetExecutingAssembly();

            imageStream = securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.TABLE.BMP");
            if (imageStream != null)
                tableImageIndex  = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            ////Documento Word
            //imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunWordDocument.bmp");
            //if (imageStream != null)
            //    wordDocumentImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            ////Foglio Excel
            //imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunExcelDocument.bmp");
            //if (imageStream != null)
            //    excelDocumentImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            ////Template Word
            //imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunWordTemplate.bmp");
            //if (imageStream != null)
            //    wordTemplateImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);

            ////Template Excel
            //imageStream = menuMngWinCtrlAssembly.GetManifestResourceStream(menuMngWinCtrlBitmapsNamespace + "RunExcelTemplate.bmp");
            //if (imageStream != null)
            //    excelTemplateImageIndex = imageListObjects.Images.Add(Image.FromStream(imageStream, true), imageListObjects.TransparentColor);
            //

            Image img = null;
            string streamSecurity = null;
            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityControl.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                singleControlImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTabDlg.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    tabImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);


            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTileManager.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    tileManagerImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTile.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    tileImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTabMngr.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    tabberImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityToolbar.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    toolBarImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityToolbarButton.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    toolBarButtonImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.Column.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    columnImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.Table.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    gridImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecuritySlaveView.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    windowsImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityRowSlaveView.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    rowImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

            //streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityEmbeddedSlaveView.png"), ImageSize.Size20x20);
            //img = Image.FromFile(streamSecurity, true);
            //if (img != null)
            //    embeddedViewImageIndex = imageListObjects.Images.Add(img, imageListObjects.TransparentColor);

        }

        //-------------------------------------------------------------------------------------------------------------
        public int GetImageIndex(int atype)
        {
            int idx = getImageIndex(atype);
            if (idx >= imageListObjects.Images.Count)
                return dummyImageIndex;
            if (idx < 0)
                return dummyImageIndex;

            return idx;
        }

        //-------------------------------------------------------------------------------------------------------------
        int getImageIndex(int atype)
        {
            switch (atype)
            {
                case 3:
                    return runFunctionImageIndex;
                case 4:
                    return reportImageIndex;
                case 5:
                    return dataEntryImageIndex;

                case 7:
                    return batchImageIndex;
                case 10:
                    return tableImageIndex;
                case 17:
                    return singleControlImageIndex;
               default:
                    return -1;
            }
        }

    }
}

