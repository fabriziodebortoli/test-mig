using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	//=========================================================================
	public partial class ProductGrid : UserControl
	{
		#region DATA
		private DataTable tableArticles;
		private DataView articleView;
		private CurrencyManager articlesCurrencyManager;
		private SerialColumnStyle CSSerial;
		private DataGridBoolColumn CSCheck;
		private DataGridTextBoxColumn CSArticle;
		private DataGridTableStyle DGArticlesStyle;
		private ToolTip ToolTipOnDG;

		#endregion

		private IList<SerialsModuleInfo> serialsList = new List<SerialsModuleInfo>();
		private Hashtable shortNames = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		private ClientStub clientStub;
		private bool dirty = false;
		/// <summary>Nome dell'installazione</summary>
		private string installation = string.Empty;
		private string currentRelease = string.Empty;
		private ErrorsHolder errorsHolder = null;
		//gestione moduli phasedout
		private ArrayList phasedOut = new ArrayList();
		//private Hashtable overloaded = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		private Hashtable originalRows = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		private ArrayList ndb_phasedOut = new ArrayList();
		private ArrayList ndb = new ArrayList();
		private bool reloadPhasedOut = false;
		private bool reloadNdb = false;
		private ArrayList rows = new ArrayList();
		//private bool demo = true;
		internal static Diagnostic diagnostic = new Diagnostic("LicensedConfigurationForm");//??
		private ProductInfo currentProd = null;
		public event EventHandler Dirty;
		public LicensedConfigurationForm ParentGridsContainer;
		public SerialNumberInfo StartSerial;
        internal bool NeedlessActivated = false;
        //---------------------------------------------------------------------
        public ProductGrid(ClientStub clientStub, ProductInfo currProd, SerialNumberInfo startSerial, LicensedConfigurationForm parent)
        {
            InitializeComponent();
            this.clientStub = clientStub;
            currentProd = currProd;
            StartSerial = startSerial;
            ParentGridsContainer = parent;
        }

        //---------------------------------------------------------------------
        public void PostInitializeComponent()
        {
            MakeColumnStyle();
            this.DGArticles.SizeChanged += new System.EventHandler(this.DGArticles_SizeChanged);
            SetToolTip();
            //LoadPhasedOutTable();
            FillForm();
        }

        Hashtable progressiveTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		//---------------------------------------------------------------------
		private void LoadPhasedOutTable()
        {
        //    this.progressiveTable.Add("ERP-Ent.Financials@NDB", "MjMzMjk3");
        //    this.progressiveTable.Add("ERP-Ent.Full@NDB", "MjMzMzcx");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package@MSD", "MjM1OTcz");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package@NDB", "MjMzMjgx");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package@ORA", "MjMzMjk5");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package@SQL", "MjMzOTYy");
        //    this.progressiveTable.Add("ERP-Pro.Commercial Package@MSD", "MjMzMjg1");
        //    this.progressiveTable.Add("ERP-Pro.Commercial Package@ORA", "MjMzMjgx");
        //    this.progressiveTable.Add("ERP-Pro.Commercial Package@SQL", "MjMzMjg3");
        //    this.progressiveTable.Add("EducationalPackage1@MSD", "MjMzMjk0");
        //    this.progressiveTable.Add("EducationalPackage2@MSD", "MjMzMjg3");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package@MSD", "MjM1MDQ1");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package@NDB", "MjMzMjgx");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package@ORA", "MjMzMjgz");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package@SQL", "MjMzNDUx");
        //    this.progressiveTable.Add("ERP-Pro.Server@MSD", "MjMzOTE3");
        //    this.progressiveTable.Add("ERP-Pro.Server@NDB", "MjMzMjgx");
        //    this.progressiveTable.Add("ERP-Pro.Server@ORA", "MjMzMzE2");
        //    this.progressiveTable.Add("ERP-Pro.Server@SQL", "MjMzNDM4");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package@MSD", "MjQzNzY0");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package@NDB", "MjMzMjgx");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package@ORA", "MjMzMzE2");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package@SQL", "MjMzOTE3");
        //    this.progressiveTable.Add("ERP-Std.Small Business Package@MSD", "MjMzNTAy");
        //    this.progressiveTable.Add("ERP-Std.Commercial Package@MSD", "MjMzMjg4");
        //    this.progressiveTable.Add("ERP-Std.Server@MSD", "MjMzNDM1");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package.Overload@NDB", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Small Business Package.Overload@MSD", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package.Overload@NDB", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Advanced Package.Overload@MSD", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package.Overload@NDB", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Trade Package.Overload@MSD", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Commercial Package.Overload@NDB", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Pro.Commercial Package.Overload@MSD", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Std.Small Business Package.Overload@MSD", "MjMzMjgw");
        //    this.progressiveTable.Add("ERP-Std.Commercial Package.Overload@MSD", "MjMzMjgw");
		}
       
		//---------------------------------------------------------------------
		private void MakeColumnStyle()
		{
			CSCheck = new DataGridBoolColumn();
			CSArticle = new DataGridTextBoxColumn();
			CSSerial = new SerialColumnStyle();

			// CSSerial			
			CSSerial.HeaderText = LicenceStrings.SerialNumber;
			CSSerial.NullText = String.Empty;
			CSSerial.ReadOnly = false;
			CSSerial.Modified += new ModificationManager(CSSerial_Modified);
			CSSerial.ExceptionRaised += new ExceptionManager(CSSerial_ExceptionRaised);
			// CSCheck 
			CSCheck.HeaderText = String.Empty;
			CSCheck.Alignment = HorizontalAlignment.Center;
			CSCheck.AllowNull = false;
			CSCheck.FalseValue = false;
			CSCheck.TrueValue = true;
            

			// CSArticle 
			CSArticle.Alignment = HorizontalAlignment.Left;
			CSArticle.HeaderText = LicenceStrings.ModuleName;
			CSArticle.NullText = String.Empty;
			CSArticle.ReadOnly = true;


			DGArticlesStyle.GridColumnStyles.Add(CSCheck);
			DGArticlesStyle.GridColumnStyles.Add(CSArticle);
			DGArticlesStyle.GridColumnStyles.Add(CSSerial);
		}

		//---------------------------------------------------------------------
		void CSSerial_ExceptionRaised(object sender, ExceptionEventArgs e)
		{
			if (e.ExceptionRaised.GetType() == typeof(System.Security.Cryptography.CryptographicException))
			{
				SetAndViewError(LicenceStrings.Win2kNoSP4, LicenceStrings.Message, e.ExceptionRaised.ToString());
			}
		}

		//---------------------------------------------------------------------
		private void SetToolTip()
		{
			ToolTipOnDG = new ToolTip();
			ToolTipOnDG.AutoPopDelay = 1500;
		}
		//---------------------------------------------------------------------
		private void CSSerial_Modified(object sender, EventArgs e)
		{
			SomethingHasChanged(sender, e);
		}

		/*//---------------------------------------------------------------------
		private void CSSerial_Exception(object sender, ExceptionEventArgs e)
		{
			if (e.ExceptionRaised.GetType() == typeof(System.Security.Cryptography.CryptographicException))
			{
				SetAndViewError(LicenceStrings.Win2kNoSP4, LicenceStrings.Message, e.ExceptionRaised.ToString());
			}
		}*/

		//---------------------------------------------------------------------
		private bool Save(object sender)
		{
			GridLostFocus(sender, EventArgs.Empty);
			return Save();
		}

		//---------------------------------------------------------------------
		public bool Check()
		{
			//popola il raccoglitore di errori con eventuali errori e/o warning
			return FindIncompleteData(true);
		}

		//---------------------------------------------------------------------
		public IList<FillingError> GetErrors()
		{
			return errorsHolder.GetAllFillingError();
		}

		/*//---------------------------------------------------------------------
		public bool ShowErrors()
		{
			ArrayList list = errorsHolder.GetAllFillingError();
			StringBuilder sb = new StringBuilder();
			bool showError = false;
			bool onlyWarning = true;
			bool procede = true;
			foreach (FillingError error in list)
			{
				if (error.Error == FillingError.ErrorType.None) continue;
				onlyWarning = (onlyWarning && error.Error == FillingError.ErrorType.Warning);
				showError = true;
				sb.Append(error.ToString());
			}
			if (showError)
			{
				ErrorViewer dialog = new ErrorViewer(sb.ToString(), onlyWarning);
				DialogResult r = dialog.ShowDialog(this);
				procede = (r == DialogResult.OK);
			}
			return procede;		
		}*/

		//---------------------------------------------------------------------
		public bool Save()
		{
			ProductInfo p = UnparseForm();
			XmlDocument doc = ProductInfo.WriteToLicensed(p);
			if (doc != null)
                return clientStub.SaveLicensed(doc.OuterXml, p.CompleteName);
            clientStub.DeleteLicensed(currentProd.CompleteName);
			return true;
		}

        //---------------------------------------------------------------------
        public bool HasRows()
        {
            return (tableArticles != null && tableArticles.Rows != null && tableArticles.Rows.Count > 0);

        }

        //---------------------------------------------------------------------
        private void SomethingHasChanged(object sender, EventArgs e)
		{
            dirty = true;
			if (Dirty != null)
				Dirty(sender,e);
		}

		//---------------------------------------------------------------------
		private void DGArticles_CurrentCellChanged(object sender, System.EventArgs e)
		{ 
			BindingManagerBase bm = DGArticles.BindingContext[this.DGArticles.DataSource, this.DGArticles.DataMember];
			DataRow dr = ((DataRowView)bm.Current).Row;
			bool hasSerial		= (bool)dr[TableStrings.ColumnHasSerial];
			bool namedcal		= (bool)dr[TableStrings.ColumnNamedCal];
			CalTypeEnum caltype = (CalTypeEnum)dr[TableStrings.ColumnCalType];
			bool hasCal			= ArticleInfo.HasCal(caltype);
			CSSerial.Editable = hasSerial && !hasCal;
			//necessario per avere check a click singolo 
			afterCurrentCellChanged = true;
			//la finestra per aggiungere più seriali solo per master,masternew e server , auto, autofunctional
			//tpgate in via di estinzione....mah
			if (DGArticles.CurrentCell.ColumnNumber == ColumnIndexer.ColumnSerial && 
				(caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew || 
				caltype == CalTypeEnum.Server || caltype == CalTypeEnum.AutoFunctional ||
                caltype == CalTypeEnum.Auto || caltype == CalTypeEnum.AutoTbs || caltype == CalTypeEnum.WmsMobile || caltype == CalTypeEnum.TPGateNew || caltype == CalTypeEnum.tbf))
			{
				string moduleName		= dr[TableStrings.ColumnSignature] as string;
				string localizedName	= dr[TableStrings.ColumnNameInCulture] as string;
				string producer			= dr[TableStrings.ColumnProducer] as string;
				string internalCode		= dr[TableStrings.ColumnInternalCode] as string;

				ModuleDialog md = new ModuleDialog(caltype == CalTypeEnum.Master && namedcal);
				md.AllowPKAdding = (caltype == CalTypeEnum.TPGateNew || caltype == CalTypeEnum.MasterNew || caltype == CalTypeEnum.AutoFunctional);
				md.ExceptionRaised += new ExceptionManager(CSSerial_ExceptionRaised);
				if (!md.Prepare(GetSerialsModuleInfo(moduleName), IsModuleChecked(moduleName), ActivationObjectHelper.IsPowerProducer((producer==null || producer.Length == 0)?internalCode:producer), namedcal))
				{
					SetAndViewError(String.Format(LicenceStrings.MsgSerialUnmanageable, localizedName), null, null);
					return;
				}
				
				md.PKList = GetSerialsModuleInfo(moduleName).PKList;
				md.Modified += new ModificationManager(SomethingHasChanged);
				DialogResult result = md.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					string name = (string)dr[TableStrings.ColumnSignature];
					VerifyModuleToReload(name, md.SmsList, caltype);
					SetSerialsList(name, md.SmsList, md.PKList);
				}
				md.Dispose();
				RefreshCell(dr);
			}
			ReloadModules();				
		}

        ////---------------------------------------------------------------------
        //private string VerifyOverload(IList<SerialNumberInfo> smsList, DataRow dr)
        //{
        //    CalTypeEnum caltype = (CalTypeEnum)dr[TableStrings.ColumnCalType];
        //    string moduleName = (string)dr[TableStrings.ColumnSignature];
        //    string moduleNameLower = moduleName.ToLower(CultureInfo.InvariantCulture);
        //    bool isOverloaded = (moduleNameLower.EndsWith(overloadMark));

        //    string originalName = moduleNameLower;
        //    if (isOverloaded)
        //        originalName = moduleNameLower.Substring(0, moduleNameLower.LastIndexOf(overloadMark));
			

        //    if ((smsList == null || smsList.Count == 0) && isOverloaded)
        //    {

        //        IList<SerialNumberInfo> list = GetSerialsList(originalName);
        //        if (list != null && list.Count > 0)
        //        {
        //            DataRow originalRow = originalRows[originalName] as DataRow;
        //            if (originalRow != null)
        //            {
        //                moduleName = originalName;
        //                ReplaceRowValues(dr, originalRow);
        //            }
        //        }
        //    }

        //    else if ((smsList != null && smsList.Count > 0))
        //        foreach (SerialNumberInfo sni in smsList)
        //        {
        //            SerialNumber sn = new SerialNumber(sni.GetSerialWOSeparator(), caltype);
        //            string shortName = sn.Module;
        //            if (shortName == null || shortName.Length == 0 || !shortNames.Contains(shortName))
        //                continue;
        //            ModuleNameInfo mni = shortNames[shortName] as ModuleNameInfo;
        //            string nameByShortName = mni.Name;
        //            if (String.Compare(nameByShortName, moduleName, true, CultureInfo.InvariantCulture) != 0 &&
        //                String.Compare(nameByShortName, originalName, true, CultureInfo.InvariantCulture) == 0)
        //            {
        //                moduleName = originalName;
        //                DataRow originalRow = originalRows[originalName] as DataRow;
        //                if (originalRow != null)
        //                    ReplaceRowValues(dr, originalRow);
        //            }

        //            if (String.Compare(nameByShortName, moduleName, true, CultureInfo.InvariantCulture) != 0 &&
        //                String.Compare(nameByShortName, moduleName+overloadMark, true, CultureInfo.InvariantCulture) == 0)
        //            {
        //                moduleName = nameByShortName;
        //                DataRow overloadedRow = overloaded[nameByShortName] as DataRow;
        //                ReplaceRowValues(dr, overloadedRow);

        //            }
        //        }
        //    return moduleName;
        //}

		//---------------------------------------------------------------------
		private DataRow CloneRow(DataRow toClone)
		{
			DataRow temp = tableArticles.NewRow();
			temp.ItemArray = new object[]{	toClone[TableStrings.ColumnSignature],
											 toClone[TableStrings.ColumnHasSerial],
											 toClone[TableStrings.ColumnSerial],
											 toClone[TableStrings.ColumnCheck],
                                             toClone[TableStrings.ColumnModifiable],
                                             toClone[TableStrings.ColumnEdition],
											 toClone[TableStrings.ColumnCalType],					 
							                 toClone[TableStrings.ColumnNamedCal],
                                             toClone[TableStrings.ColumnAvailable],
                                             toClone[TableStrings.ColumnMandatory],
											 toClone[TableStrings.ColumnMaxCal],
											 toClone[TableStrings.ColumnNameInCulture],
											 toClone[TableStrings.ColumnDependency],
											 toClone[TableStrings.ColumnProducer],
											 toClone[TableStrings.ColumnProdID],
                                             toClone[TableStrings.ColumnAcceptdemo],
											 toClone[TableStrings.ColumnInternalCode],
											 toClone[TableStrings.ColumnPrivateCode],
											 toClone[TableStrings.ColumnModules],
											 toClone[TableStrings.ColumnIncludedSM],
											 toClone[TableStrings.ColumnMode],
                                             toClone[TableStrings.ColumnNeedless],
                                             toClone[TableStrings.ColumnCalUse]};
			return temp;
		}

		//---------------------------------------------------------------------
		private void ReplaceRowValues(DataRow oldRow, DataRow newRow)
		{
			oldRow[TableStrings.ColumnCheck]		= true;
			oldRow[TableStrings.ColumnEdition]		= newRow[TableStrings.ColumnEdition];
            oldRow[TableStrings.ColumnModifiable]   = newRow[TableStrings.ColumnModifiable];
            oldRow[TableStrings.ColumnHasSerial]	= newRow[TableStrings.ColumnHasSerial];	
			oldRow[TableStrings.ColumnCalType]		= newRow[TableStrings.ColumnCalType];		
			oldRow[TableStrings.ColumnNamedCal]		= newRow[TableStrings.ColumnNamedCal];
            oldRow[TableStrings.ColumnAvailable]    = newRow[TableStrings.ColumnAvailable];
            oldRow[TableStrings.ColumnMandatory]	= newRow[TableStrings.ColumnMandatory];	
			oldRow[TableStrings.ColumnMaxCal]		= newRow[TableStrings.ColumnMaxCal];		
			oldRow[TableStrings.ColumnNameInCulture]= newRow[TableStrings.ColumnNameInCulture];
			oldRow[TableStrings.ColumnSignature]	= newRow[TableStrings.ColumnSignature];
			oldRow[TableStrings.ColumnProducer]		= newRow[TableStrings.ColumnProducer];		
			oldRow[TableStrings.ColumnProdID]       = newRow[TableStrings.ColumnProdID];       
			oldRow[TableStrings.ColumnAcceptdemo]   = newRow[TableStrings.ColumnAcceptdemo];       
            oldRow[TableStrings.ColumnInternalCode]	= newRow[TableStrings.ColumnInternalCode];	
			oldRow[TableStrings.ColumnPrivateCode]	= newRow[TableStrings.ColumnPrivateCode];	
			oldRow[TableStrings.ColumnDependency]	= newRow[TableStrings.ColumnDependency];	
			oldRow[TableStrings.ColumnModules]		= newRow[TableStrings.ColumnModules];		
			oldRow[TableStrings.ColumnIncludedSM]	= newRow[TableStrings.ColumnIncludedSM];	
			oldRow[TableStrings.ColumnMode]			= newRow[TableStrings.ColumnMode];
            oldRow[TableStrings.ColumnNeedless]	    = newRow[TableStrings.ColumnNeedless];
            oldRow[TableStrings.ColumnCalUse]		= newRow[TableStrings.ColumnCalUse];
			oldRow[TableStrings.ColumnSerial]		= newRow[TableStrings.ColumnSerial];	
		}

		//---------------------------------------------------------------------
		private void VerifyModuleToReload(string moduleName, IList<SerialNumberInfo> smsList, CalTypeEnum caltype)
		{
			reloadNdb = reloadNdb || (!IsNDB(smsList));
            bool verified = false;
            string db = GetDb(smsList);
			if (string.IsNullOrEmpty(db)) return;
			string key = String.Concat(moduleName,"@",db);
			//string cabledVal = progressiveTable[key] as string;//verifica dei moduli ndb o phasedout da tabella di valori cablata
			//bool verified = VerifyPhasedOutValue(smsList, cabledVal, caltype);
			//if (!verified)//verifica dei moduli ndb o phasedout da file
			//{
				string obj = clientStub.GetPhasedOutValue(key);
            verified = (obj == "OK");
					//verified =VerifyPhasedOutValue(smsList, obj.ToString(), caltype);
			//}
			reloadPhasedOut = reloadPhasedOut || verified;
		}
		
		//---------------------------------------------------------------------
		private string GetDb(IList<SerialNumberInfo> smsList)
		{
			if (smsList == null || smsList.Count == 0)
				return string.Empty;
			foreach (SerialNumberInfo sni in smsList)
			{
				DatabaseVersion db = SerialNumberInfo.GetDatabaseVersion(sni);
				switch (db)
				{
					case DatabaseVersion.All:			return "NDB";
					case DatabaseVersion.Ndb:			return "NDB";
					case DatabaseVersion.SqlServer2000: return "SQL";
					case DatabaseVersion.Oracle:		return "ORA";
					case DatabaseVersion.MSDE:			return "MSD";
				}		
			}
			return string.Empty;
		
		}

		//---------------------------------------------------------------------
		private void ReloadModules()
		{
            try
            {
                if (reloadPhasedOut)
                {
                    ReloadModules(ndb);
                    ReloadModules(ndb_phasedOut);
                    ReloadModules(phasedOut);
                }
                else if (reloadNdb)
                    ReloadModules(ndb);

                reloadPhasedOut = reloadNdb = false;
            }
            catch (Exception exc){
                SetMessage(exc.Message ,null, null, DiagnosticType.Error);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
            }
		}

		//---------------------------------------------------------------------
		private void ReloadModules(ArrayList list)
		{
			if (list.Count > 0)
			{
				foreach (DataRow r in list)
					if (!tableArticles.Rows.Contains(r[TableStrings.ColumnSignature]))
						tableArticles.Rows.Add(r);
				list.Clear();
			}
		}

		//---------------------------------------------------------------------
		private bool IsNDB(IList<SerialNumberInfo> smsList)
		{
			if (smsList != null || smsList.Count > 0)
				foreach (SerialNumberInfo sni in smsList)
					return (SerialNumberInfo.GetDBNetworkType(sni) == DBNetworkType.Small);
			return true;//cosi non vengono mostrati i moduli ndb
		}

		//---------------------------------------------------------------------
		private bool VerifyPhasedOutValue(IList<SerialNumberInfo> smsList, string val, CalTypeEnum caltype)
		{
			if (smsList == null || smsList.Count == 0 || val == null || val.Length == 0)
				return false;//cosi non vengono mostrati i moduli phasedout			

			foreach (SerialNumberInfo sni in smsList)
			{
				Object o = shortNames[SerialNumberInfo.GetModuleShortNameFromSerial(sni, caltype)];
				if (o != null)
					return (SerialNumberInfo.GetProgressive(sni) <= UnparseProgressive(val));
				else
					if (sni.IsSpecial(caltype))
						return true;

			}
			return false;
		}

		//---------------------------------------------------------------------
		private int UnparseProgressive(string val)
		{				
			byte[] source = Convert.FromBase64String(val);
			string result = string.Empty;
			StringBuilder resultBuilder = new StringBuilder(source.Length);
			foreach (byte b in source)
				resultBuilder.Append((char)b);
			return int.Parse(resultBuilder.ToString());
		}

		/*UTILITY
		//---------------------------------------------------------------------
		private string ParseProgressive(int c)
		{
			string val = c.ToString();
			byte[] result = new byte[val.Length];
			for (int i = 0; i < val.Length; i++)
				result[i] = (byte)val[i];
				
			return Convert.ToBase64String(result);
		}*/


		//---------------------------------------------------------------------
		private void RefreshCell(DataRow r)
		{
			if (GetIsModuleWithCal((CalTypeEnum)r[TableStrings.ColumnCalType]))
			{
				string name = r[TableStrings.ColumnSignature] as string;
				if (GetSerialsList(name).Count > 0)
					r[TableStrings.ColumnSerial] = LicenceStrings.MsgEditSerial;
				else
					r[TableStrings.ColumnSerial] = LicenceStrings.MsgInsertSerial;
			}
			tableArticles.AcceptChanges();
			DGArticles.CurrentCell = new DataGridCell(DGArticles.CurrentCell.RowNumber,ColumnIndexer.ColumnArticle);

		}

		//necessario per avere check a click singolo
		//---------------------------------------------------------------------
		private bool afterCurrentCellChanged = false;

		//necessario per avere check a click singolo
		//---------------------------------------------------------------------
		private void DGArticles_Click(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Point p = Control.MousePosition;
			if  (e.Button == MouseButtons.Right) return;
		
			if (DGArticles.DataMember == null || DGArticles.DataSource == null)
				return;
			Point pt = DGArticles.PointToClient(p);
			DataGrid.HitTestInfo hti = DGArticles.HitTest(pt);
			BindingManagerBase bmb = this.BindingContext[DGArticles.DataSource, DGArticles.DataMember];
			if (afterCurrentCellChanged					&& 
				(hti.Row < bmb.Count)					&& 
				(hti.Type == DataGrid.HitTestType.Cell) &&
				(hti.Column == ColumnIndexer.ColumnCheck)		
				)
			{
             
                    DGArticles.CurrentCell = new DataGridCell(hti.Row, hti.Column);
                    bool checkstatus = ((bool)DGArticles[hti.Row, ColumnIndexer.ColumnCheck]);
                    DataRow dr = ((DataRowView)bmb.Current).Row;
                
                if (((bool)dr[TableStrings.ColumnModifiable]))
                {
                    DataRowChangeEventArgs drcea = new DataRowChangeEventArgs(dr, DataRowAction.Nothing);
                    DGArticles[hti.Row, ColumnIndexer.ColumnCheck] = !checkstatus;
                    SomethingHasChanged(sender, e);
                }
                else

                    MessageBox.Show(this,((string)dr[TableStrings.ColumnNameInCulture]) + Environment.NewLine+ LicenceStrings.CantModify, currentProd.ViewName);
            }
			afterCurrentCellChanged = false;
			//dopo un sort faccio un cambio di cella, altrimenti potrebbe non funzionare il cambio alla tab specifica.
			if ((hti.Row < 0) && (hti.Type == DataGrid.HitTestType.ColumnHeader))
				DGArticles.CurrentCell = new DataGridCell(DGArticles.CurrentCell.RowNumber, ColumnIndexer.ColumnArticle);
			else if (hti.Column == ColumnIndexer.ColumnCheck)//Per eseguire il cambio cella ed avere click singolo
				DGArticles.CurrentCell = new DataGridCell(hti.Row, ColumnIndexer.ColumnArticle);

		}

		/// <summary>
		/// Accade dopo che la riga della tabella è stata cambiata
		/// </summary>
		//---------------------------------------------------------------------
		private void RowChanged(object sender,  DataRowChangeEventArgs e)
		{
			if (e.Action == DataRowAction.Delete) return;

			if (((DataTable)sender).TableName == tableArticles.TableName)
			{
				RefreshSeriasList(e.Row);
			}
		}	
		
		//---------------------------------------------------------------------
		private void GridLostFocus(object sender, System.EventArgs e)
		{
		
			EndEditManager(!(sender is TabControl));
			DGArticles.CurrentCell = new DataGridCell(0,1);
		}

		//---------------------------------------------------------------------
		private void RefreshSeriasList(DataRow r)
		{
			bool hascal = GetIsModuleWithCal((CalTypeEnum) r[TableStrings.ColumnCalType]);
			if (hascal) return;//ha cal, quindi più serial
			string serial = r[TableStrings.ColumnSerial] as string;

			IList<SerialNumberInfo> list = new List<SerialNumberInfo>();
			if (serial != null && serial != String.Empty)
				list.Add(new SerialNumberInfo(serial));
			SetSerialsList(r[TableStrings.ColumnSignature] as string, list, null);
		}

		/// <summary>
		/// Da chiamare quando clicco fuori dal datagrid e sui quei controlli, come la toolbar, 
		/// che non attivano l'endEdit
		/// </summary>
		//---------------------------------------------------------------------
		private bool EndEditManager(bool isRight)
		{		
			if (isRight)
			{
				DGArticles.CurrentCell = new DataGridCell(DGArticles.CurrentRowIndex, ColumnIndexer.ColumnArticle);
				DGArticles.EndEdit(CSSerial, DGArticles.CurrentRowIndex, false);
				if (articlesCurrencyManager == null)return false;
				try
				{
					articlesCurrencyManager.EndCurrentEdit();
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					return false;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool IsModuleChecked(string moduleName)
		{
			foreach (DataRow r in tableArticles.Rows)
			{
				string name = r[TableStrings.ColumnSignature] as string;
				if (String.Compare(name, moduleName, true, CultureInfo.InvariantCulture) == 0)
					return (bool)r[TableStrings.ColumnCheck];
			}
			return false;
		}

		//---------------------------------------------------------------------
		private void LicensedConfigurationForm_Load(object sender, System.EventArgs e)
		{
			DGArticles.CurrentCell = new DataGridCell(0,1);
			dirty = false;

		}

		List<string> originalLicensedArticlesList = new List<string>();
        string currentKey = null;
		//---------------------------------------------------------------------
		private void FillArticlesGrid()
		{
			if (currentProd == null)
				return;

			MakeTableArticles();
			SetMappingName();

			serialsList.Clear();
			shortNames.Clear();
			//overloaded.Clear();
			rows.Clear();
			originalRows.Clear();

			Hashtable producersList = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			ArticleInfo[] articlesOldList = currentProd.Articles;
            List<ArticleInfo> articles = new List<ArticleInfo>(articlesOldList);
           
			ArrayList nameList = new ArrayList();
            currentKey = currentProd.ActivationKey;
            
			foreach (ArticleInfo art in articles)
			{
				//verifico che il nome del modulo non sia ripetuto più volte
				if (nameList.Contains(art.Name))
				{
					SetError(String.Format(LicenceStrings.ModuleListedTwice, art.LocalizedName), null, null);
					continue;
				}
				//se è un modulo 'di servizio' non lo mostro nella griglia
				if (art.IsBackModule())
					continue;
				if (EvaluateNeedless(art.Needless))
                {
                    if (!String.IsNullOrEmpty(art.NeedlessText))
                    {  
                        string s = GetNeedLessString(art);
                        art.Licensed = true;
                        art.LocalizedName = s;
                        art.Modifiable = false;
                        NeedlessActivated = true;
                    }
                   // continue;
                }
                EvaluateCountry(art);
                if (art.ImplicitActivation) NeedlessActivated = true;

                //				//se è un modulo che non devo mostrare in demo, e sono in demo, salto
                //				if (art.IsNoDemoModule() && demo)
                //					continue;
                //bool isOverloaded = (art.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(overloadMark));

                nameList.Add(art.Name);
				DataRow artRow = tableArticles.NewRow();
                
				if (StartSerial != null && !String.IsNullOrEmpty(StartSerial.GetSerialWOSeparator()))
				{
					SerialNumber s = new SerialNumber(StartSerial.GetSerialWOSeparator());
					if (string.Compare(s.ProductCode.Product, art.ProdID, StringComparison.InvariantCultureIgnoreCase) == 0)
						foreach (string sn in art.ShortNames)
							if (string.Compare(s.RawData, sn, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								art.SerialList.Add(StartSerial);
								art.Licensed = true;
							}
				
				}

				if (!art.HasSerial)
					artRow[TableStrings.ColumnSerial] = LicenceStrings.NotSerialRequired;

				else if (art.HasCal())
				{
					if (art.SerialList.Count > 0)
						artRow[TableStrings.ColumnSerial] = LicenceStrings.MsgEditSerial;
					else
						artRow[TableStrings.ColumnSerial] = LicenceStrings.MsgInsertSerial;
				}

				else if (art.SerialList.Count > 0 && !art.HasCal())
					artRow[TableStrings.ColumnSerial] = ((art.SerialList[0]) as SerialNumberInfo).GetSerialWSeparator();

				FillSerialList(art);
				bool isdemo = IsDemo(art.SerialList, art.CalType) ;

				artRow[TableStrings.ColumnCheck]		= art.Licensed;
                artRow[TableStrings.ColumnModifiable] = art.Modifiable;
                artRow[TableStrings.ColumnEdition]		= art.Edition;
				artRow[TableStrings.ColumnHasSerial]	= art.HasSerial;
				artRow[TableStrings.ColumnCalType]		= art.CalType;
				artRow[TableStrings.ColumnNamedCal]		= art.NamedCal;
                artRow[TableStrings.ColumnAvailable] = art.Available;
                artRow[TableStrings.ColumnMandatory]	= art.Mandatory;
				artRow[TableStrings.ColumnMaxCal]		= art.MaxCal;	
				artRow[TableStrings.ColumnNameInCulture]= art.LocalizedName;
				artRow[TableStrings.ColumnSignature]	= art.Name;
				artRow[TableStrings.ColumnProducer]		= art.Producer;
                artRow[TableStrings.ColumnProdID]       = art.ProdID;
				artRow[TableStrings.ColumnAcceptdemo]   = art.AcceptDEMO;
                artRow[TableStrings.ColumnInternalCode]	= art.InternalCode;
				artRow[TableStrings.ColumnPrivateCode]	= art.PrivateCode;
				artRow[TableStrings.ColumnDependency]	= art.DependencyExpression;
				artRow[TableStrings.ColumnModules]		= art.Modules;
				artRow[TableStrings.ColumnIncludedSM]	= art.IncludedSM;
				artRow[TableStrings.ColumnMode]			= art.ModuleMode;
                artRow[TableStrings.ColumnNeedless] = art.Needless;
                artRow[TableStrings.ColumnCalUse]		= art.CalUse;
				
				artRow.EndEdit();
				//il warning sui prodotti disattivati solo per microarea e non demo, perchè è inutile dare quel tipo di warning per seriali speciali (demo , rnfs, dvlp)
				string p = (String.IsNullOrEmpty(art.InternalCode)) ? art.Producer : art.InternalCode;
				if (art.Licensed && art.HasSerial && art.SerialList != null && art.SerialList.Count > 0 && ActivationObjectHelper.IsPowerProducer(p)  )
					foreach (SerialNumberInfo sni in art.SerialList)
                    {
                        if(!sni.IsSpecial(art.CalType))
						    originalLicensedArticlesList.Add(sni.GetSerialWOSeparator());
                    }
				//se i moduli sono phased-out o NDB 
				//non li carico nella griglia 
				//ma me li tengo in memoria.
				if (art.ModuleMode == ModuleModeEnum.PhasedOut)
					phasedOut.Add(artRow);
					//I moduli ndb vanno mostrati in demo perchè sono quelli 
					//non presenti nella lite ma presenti nella pro NDB
				else if (art.ModuleMode == ModuleModeEnum.NDB)//todo 
					ndb.Add(artRow);
				else if (art.ModuleMode == ModuleModeEnum.NDB_PhasedOut)
					ndb_phasedOut.Add(artRow);
                //else if (isOverloaded)
                //{
                //    overloaded.Add(art.Name, CloneRow(artRow));
                //    rows.Add(artRow);	
                //}
				else
					rows.Add(artRow);	
				
				FillShortNames(art);

				//verifico se in seguito all'insertimento 
				//di serial number server è il caso di 
				//reloadare alcuni articoli phased out e ndb
				VerifyModuleToReload(art.Name, art.SerialList, art.CalType);	
			}
			foreach (DataRow artRow in rows)
            {
                tableArticles.Rows.Add(artRow);

                //string name = artRow[TableStrings.ColumnSignature] as String;
                //// overload
                //if (name.ToLower(CultureInfo.InvariantCulture).EndsWith(overloadMark))
                //    tableArticles.Rows.Add(artRow) ;
                ////originale
                //else
                //{
                //    DataRow r = overloaded[name+overloadMark] as DataRow;
                //    if (r == null)
                //        tableArticles.Rows.Add(artRow);
                //    else
                //        originalRows.Add(name, CloneRow(artRow));
                //}    
			}
            //foreach (DataRow artRow in tableArticles.Rows)
            //{
            //    VerifyOverload(GetSerialsList((string)artRow[TableStrings.ColumnSignature]), artRow);
            //}
			//Reloado (se è il caso) i moduli phasedout e ndb
			ReloadModules();

			GetViewArticles();
            articleView.Sort = TableStrings.ColumnNameInCulture;
			DGArticles.DataSource = articleView;
			if (DGArticles.BindingContext != null)
				articlesCurrencyManager	= (CurrencyManager)DGArticles.BindingContext[articleView];
			tableArticles.RowChanged += new DataRowChangeEventHandler(RowChanged);
			SetWidths();
           
           
		}

        //---------------------------------------------------------------------
        private void EvaluateCountry(ArticleInfo art)
        {
            string userCountry = ParentGridsContainer.GetCountry();
            if (userCountry.IsNullOrWhiteSpace()) return;

            if (!String.IsNullOrEmpty(art.OptionalCountry))
            {
                string[] optionals = art.OptionalCountry.Split(',');
                foreach (string optional in optionals)
                    if (String.Compare(optional.Trim(), userCountry, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        SomethingHasChanged(this, EventArgs.Empty);
                        art.Licensed = true;
                        art.Modifiable = true;
                        NeedlessActivated = true;
                    }

            }

            if (!String.IsNullOrEmpty(art.MandatoryCountry))
            {
                string[] mandatories = art.MandatoryCountry.Split(',');
                foreach (string mandatory in mandatories)
                    if (String.Compare(mandatory.Trim(), userCountry, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {

                        SomethingHasChanged(this, EventArgs.Empty);
                        art.Licensed = true;
                        art.Modifiable = false;
                        NeedlessActivated = true;
                    }
            }
        }

        //aggiunge informazioni al nome del modulo needless
        //---------------------------------------------------------------------
        private string GetNeedLessString(ArticleInfo a)
        {
            if (string.IsNullOrEmpty(a.NeedlessText)) return string.Empty;
            switch (a.NeedlessText.ToLower(CultureInfo.InvariantCulture))
            {
                case "tbfincluded":
                    return String.Concat(a.LocalizedName, " ", string.Format ( LicenceStrings.TbfIncluded, InstallationData.BrandLoader.GetBrandedStringBySourceString("M4"))); 
                default:
                    return string.Empty;
            }  
        }

        /// <summary>
        /// needless indica che tale modulo è superfluo quindi di default selzionato e non modificabile se è presente ed attivato un altro prodotto ( o un altro modulo di un altro prodotto)
        /// tranne se indica tbs allora è un marchio che serve ad altro ( cerca "tbs")
        /// </summary>
        /// <param name="needless">nome di prodotto di cui verificare l'esistenza o nome di modulo di cui verificare l'attivazione</param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        private bool EvaluateNeedless(string needless)
        {
            if (String.IsNullOrEmpty(needless)) return false;
            string[] needlessList = needless.Split(';');

            
            foreach (ProductInfo p in clientStub.ActivationObj.Products)
            {
                foreach (string nd in needlessList)
                {
                    if (string.Compare(p.ProductName, nd, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;

                    foreach (ArticleInfo a in p.Articles)
                    {
                        if (!a.Licensed) continue;

                        if (string.Compare(a.Name, nd, StringComparison.InvariantCultureIgnoreCase) == 0)
                            return true;
                        foreach (IncludedSMInfo isi in a.IncludedSM)
                            if (string.Compare(isi.Name, nd, StringComparison.InvariantCultureIgnoreCase) == 0)
                                return true;
                    }
                }
            }

            return false;
          
        }
		
		//---------------------------------------------------------------------
		private SerialsModuleInfo GetSerialsModuleInfo(string moduleName)
		{
			foreach (SerialsModuleInfo hcmi in serialsList)
			{
				if (String.Compare(hcmi.SalesModuleName, moduleName, true, CultureInfo.InvariantCulture) == 0)
				{
					return hcmi;
				}
			}
			Debug.WriteLine("GetSerialsModuleInfo: SerialModuleInfo non trovato per il modulo " + moduleName);
			return null;
		}

		//---------------------------------------------------------------------
		private IList<SerialNumberInfo> GetSerialsList(string moduleName)
		{
			foreach (SerialsModuleInfo hcmi in serialsList)
			{
				if (String.Compare(hcmi.SalesModuleName, moduleName, true, CultureInfo.InvariantCulture) == 0)
				{
					return hcmi.SerialList;
				}
			}
			return new List<SerialNumberInfo>() ;
		}

		//---------------------------------------------------------------------
		private ArrayList GetPKList(string moduleName)
		{
			foreach (SerialsModuleInfo hcmi in serialsList)
			{
				if (String.Compare(hcmi.SalesModuleName, moduleName, true, CultureInfo.InvariantCulture) == 0)
				{
					return hcmi.PKList;
				}
			}
			return new ArrayList();
		}
		//---------------------------------------------------------------------
		private bool GetIsModuleWithCal(CalTypeEnum caltype)
		{
			return ArticleInfo.HasCal(caltype);
		}

		//---------------------------------------------------------------------
		private void SetSerialsList(string name, IList<SerialNumberInfo> list, IList pks)
		{
			
			foreach (SerialsModuleInfo hcmi in serialsList)
			{
				if (String.Compare(hcmi.SalesModuleName, name, true, CultureInfo.InvariantCulture) == 0)
				{
					hcmi.SerialList = list;
					if (pks != null && pks.Count > 0)
						hcmi.PKList = new ArrayList(pks);
				}
			}
		}
		
		/// <summary>
		/// Restituisce le view con le property corrette per datasource del datagrid
		/// </summary>
		//---------------------------------------------------------------------
		private void GetViewArticles()
		{
			articleView = new DataView(tableArticles);
			SetViewProperties(articleView);
		}

		/// <summary>
		/// Restituisce le view con le property corrette per datasource del datagrid
		/// </summary>
		//---------------------------------------------------------------------
		private void SetViewProperties(DataView view)
		{
			view.AllowNew	= false;
			view.AllowDelete= false;
			view.AllowEdit	= true;
		}
	
		//---------------------------------------------------------------------
		private void MakeTableArticles()
		{
			tableArticles = new DataTable(TableStrings.TableArticles);
			tableArticles.Locale = CultureInfo.InvariantCulture;

			//-----creo le colonne--------
			DataColumn colName				= new  DataColumn();
            DataColumn colModifiable = new DataColumn();
            DataColumn colHasSerial			= new  DataColumn();
			DataColumn colSerial			= new  DataColumn();
			DataColumn colCalType			= new  DataColumn();
			DataColumn colNamedCal			= new  DataColumn();
			DataColumn colEdition			= new  DataColumn();
			DataColumn colLicensed			= new  DataColumn();
			DataColumn colMandatory			= new  DataColumn();
			DataColumn colMaxcal			= new  DataColumn();
			DataColumn colProduct			= new  DataColumn();
			DataColumn colProducer			= new  DataColumn();
            DataColumn colProdID            = new DataColumn();
            DataColumn colAcceptDEMO = new DataColumn();
			DataColumn colNameInCulture		= new  DataColumn();
			DataColumn colDependencies		= new  DataColumn();
			DataColumn colModules			= new  DataColumn();
			DataColumn colIncludedSM		= new  DataColumn();
			DataColumn colInternalCode		= new  DataColumn();
			DataColumn colPrivateCode		= new  DataColumn();
			DataColumn colMode				= new  DataColumn();
			DataColumn colCalUse			= new  DataColumn();
            DataColumn colneedless          = new DataColumn();
            
            DataColumn colAvailable = new DataColumn();
            
            //-----associo le colonne--------
            colName.ColumnName = TableStrings.ColumnSignature;
			tableArticles.Columns.Add(colName);

			colHasSerial.ColumnName = TableStrings.ColumnHasSerial;
			colHasSerial.DataType = typeof(bool);
			tableArticles.Columns.Add(colHasSerial);

			colSerial.ColumnName = TableStrings.ColumnSerial;
			tableArticles.Columns.Add(colSerial);

			colLicensed.ColumnName = TableStrings.ColumnCheck;
			colLicensed.DataType = typeof(bool);
			tableArticles.Columns.Add(colLicensed);

            colModifiable.ColumnName = TableStrings.ColumnModifiable;
            colModifiable.DataType = typeof(bool);
            tableArticles.Columns.Add(colModifiable);

            colEdition.ColumnName = TableStrings.ColumnEdition;
			tableArticles.Columns.Add(colEdition);

			colCalType.ColumnName = TableStrings.ColumnCalType;
			colCalType.DataType = typeof(CalTypeEnum);
			tableArticles.Columns.Add(colCalType);

			colNamedCal.ColumnName = TableStrings.ColumnNamedCal;
			colNamedCal.DataType = typeof(bool);
			tableArticles.Columns.Add(colNamedCal);

            colAvailable.ColumnName = TableStrings.ColumnAvailable;
            colAvailable.DataType = typeof(ArticleInfo.Avaiability);
            tableArticles.Columns.Add(colAvailable);


            colMandatory.ColumnName = TableStrings.ColumnMandatory;
			colMandatory.DataType = typeof(bool);
			tableArticles.Columns.Add(colMandatory);

			colMaxcal.ColumnName = TableStrings.ColumnMaxCal;
			tableArticles.Columns.Add(colMaxcal);

			colNameInCulture.ColumnName = TableStrings.ColumnNameInCulture;
			tableArticles.Columns.Add(colNameInCulture);

			colDependencies.ColumnName = TableStrings.ColumnDependency;
			colDependencies.DataType = typeof(string);
			tableArticles.Columns.Add(colDependencies);

			colProducer.ColumnName = TableStrings.ColumnProducer;
			tableArticles.Columns.Add(colProducer);
            
            colProdID.ColumnName = TableStrings.ColumnProdID;
            tableArticles.Columns.Add(colProdID);

            colAcceptDEMO.ColumnName = TableStrings.ColumnAcceptdemo;
            tableArticles.Columns.Add(colAcceptDEMO);

			colInternalCode.ColumnName = TableStrings.ColumnInternalCode;
			tableArticles.Columns.Add(colInternalCode);

			colPrivateCode.ColumnName = TableStrings.ColumnPrivateCode;
			tableArticles.Columns.Add(colPrivateCode);

			colModules.ColumnName = TableStrings.ColumnModules;
			colModules.DataType = typeof(ModuleInfo[]);
			tableArticles.Columns.Add(colModules);
			
			colIncludedSM.ColumnName = TableStrings.ColumnIncludedSM;
			colIncludedSM.DataType = typeof(IncludedSMInfo[]);
			tableArticles.Columns.Add(colIncludedSM);

			colMode.ColumnName = TableStrings.ColumnMode;
			colMode.DataType = typeof(ModuleModeEnum);
			tableArticles.Columns.Add(colMode);

            colneedless.ColumnName = TableStrings.ColumnNeedless;
            colneedless.DataType = typeof(string);
            tableArticles.Columns.Add(colneedless);

            colCalUse.ColumnName = TableStrings.ColumnCalUse;
			colCalUse.DataType = typeof(CalUseEnum);
			tableArticles.Columns.Add(colCalUse);

			tableArticles.PrimaryKey = new System.Data.DataColumn[]{colName};
		}

		//---------------------------------------------------------------------
		private void SetMappingName()
		{
			DGArticlesStyle.MappingName = tableArticles.TableName;
			CSArticle.MappingName		= TableStrings.ColumnNameInCulture;
			CSCheck.MappingName			= TableStrings.ColumnCheck;
			CSSerial.MappingName		= TableStrings.ColumnSerial;
		}

		//---------------------------------------------------------------------
		private void FillShortNames(ArticleInfo art)
		{
			bool error = false;
			foreach (string s in art.ShortNames)
			{
				if (string.IsNullOrWhiteSpace(s))
					continue;
				if (shortNames.Contains(s))
				{
					string name = String.Empty;
					string artName = String.Empty;
					ModuleNameInfo mni = ((ModuleNameInfo)shortNames[s]);
					if (mni != null)
					{
						name = mni.LocalizedName;
						artName = mni.Name;
					}
					if (String.Compare(art.Name, artName, true, CultureInfo.InvariantCulture) == 0) continue;
					SetWarning(LicenceStrings.IncongruityData, LicenceStrings.Detail, String.Format(LicenceStrings.DuplicatedShortName, name, art.LocalizedName));
					error = true;
					continue;
				}
				shortNames.Add(s, new ModuleNameInfo(art.Name, art.LocalizedName, art.NamedCalNumber));
			}
			if (error)
				ViewDiagnostic();
		}

		//---------------------------------------------------------------------
		private void FillSerialList(ArticleInfo art)
		{
			if (art.HasSerial)
				serialsList.Add(new SerialsModuleInfo(art.Name, art.CalUse, art.LocalizedName, art.SerialList, art.CalType, art.PKs));
		}
	
		////---------------------------------------------------------------------
		//internal bool ConfirmSaving()
		//{
		//    DialogResult r = MessageBox.Show(Strings.MsgConfirmKeyRequest, ""/*Product*/, MessageBoxButtons.OKCancel);
		//    return r == DialogResult.OK;
		//}

		//---------------------------------------------------------------------
		private void FillForm()
		{
			if (clientStub.ActivationObj == null)
			{
				SetAndViewError(LicenceStrings.MsgNoData, null, null);
				return;
			}


			if (currentProd == null) 
				return;


			currentRelease = currentProd.Release;
			//	SafeGui.ControlText(LblRelease, string.Format(LicenceStrings.Release, currentRelease));

			FillArticlesGrid();
			dirty = false;
			
			
		}
	
		/// <summary>
		/// Setta la larghezza delle colonne nel datagrid in base all'area disponibile.
		/// </summary>
		//---------------------------------------------------------------------
		private void SetWidths()
		{
			//il 5% per la colonna check( della size iniziale )= 30
			int widthSmall	= 30;		
			// il resto per nome e serial, considerando un po' di spazio per le line di separazione
			int widthBig	= (int)((DGArticles.Width - widthSmall) / (2.1)); 
			CSCheck.Width	= widthSmall;
			//la colonna serial è grande giusta giusta per farci stare il controllo che prende i serial
			CSSerial.Width	= widthBig  - ((int)(widthBig*0.15));
			CSArticle.Width = widthBig  + ((int)(widthBig*0.17));
			DGArticles.Refresh();
		}

		//---------------------------------------------------------------------
		private void DGArticles_SizeChanged(object sender, System.EventArgs e)
		{
			SetWidths();
		}

		/// <summary>
		/// Crea un ProductInfo con le informazioni contenute nel form.
		/// </summary>
		//---------------------------------------------------------------------
		private ProductInfo UnparseForm()
		{
			ArrayList articles	= new ArrayList();
			if (tableArticles == null || tableArticles.Rows == null) return null; 
           
			bool oneCheck = false;
			foreach (DataRow aRow in tableArticles.Rows)
			{
				if (aRow == null) continue;
				ArticleInfo articleInfo		= new ArticleInfo();
				articleInfo.Name			= aRow[TableStrings.ColumnSignature] as string;
				articleInfo.Producer		= aRow[TableStrings.ColumnProducer] as string;
                articleInfo.ProdID          = aRow[TableStrings.ColumnProdID] as string;
                articleInfo.AcceptDEMO = aRow[TableStrings.ColumnAcceptdemo] as string;
                articleInfo.InternalCode	= aRow[TableStrings.ColumnInternalCode] as string;
				articleInfo.LocalizedName	= aRow[TableStrings.ColumnNameInCulture] as string;
				articleInfo.HasSerial		= (bool)aRow[TableStrings.ColumnHasSerial];
				articleInfo.Licensed		= (bool)aRow[TableStrings.ColumnCheck];
				articleInfo.ModuleMode		= (ModuleModeEnum)aRow[TableStrings.ColumnMode];
				articleInfo.Modules			= (ModuleInfo[])aRow[TableStrings.ColumnModules];
				articleInfo.SerialList		= GetSerialsList(articleInfo.Name);
				articleInfo.PKs				= GetPKList(articleInfo.Name);
				articles.Add(articleInfo);
				oneCheck = oneCheck || articleInfo.Licensed;
			}
			if (!oneCheck) return null;
			ArticleInfo[] articleInfos = (ArticleInfo[])articles.ToArray(typeof(ArticleInfo));
			string country = String.Empty;

			if (clientStub.ActivationObj != null && clientStub.ActivationObj.User != null)
				country = clientStub.ActivationObj.User.Country;
			ProductInfo pi			= new ProductInfo();
			pi.Articles				= articleInfos;
			pi.Country				= country;
            pi.ProductName = currentProd.ProductName;
            pi.CompleteName = currentProd.CompleteName;
			pi.Release				= currentRelease;//this.LblRelease.Text;// ServedRelease;
			pi.ActivationVersion = clientStub.ActivationObj.ActivationVersion;
            pi.ActivationKey = currentKey;
           
			return	pi;
		}

		#region GRID COMPLIANCE

		//---------------------------------------------------------------------
		private bool FindIncompleteData(bool alsoKey)
		{
			errorsHolder = new ErrorsHolder();
			bool skipControl = false;
			GridCompliance(out skipControl);
			return errorsHolder.IsOK();
		}


      public  List<ModuleDependecies> ModuleDependeciesList = new List<ModuleDependecies>();
		/// <summary>
		/// Effettua tutti i controlli di congruenza sui serial number e sulle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void GridCompliance(out bool skipControl)
		{
			skipControl = false;
            ModuleDependeciesList.Clear();
			if (tableArticles == null) 
			{
				Debug.Fail("Errore tabella nulla");
				return;
			}
			List<string> currentModules = new List<string>();
			SerialUniformity.ClearAll();
            bool isNFS = false; // distributor or reseller
            bool isDeveloper = false;
            bool isDeveloperPlus = false;
            bool isDeveloperPlusk = false; 
            bool isDemo = false;
            //bool		isCalStandAlone			= false;
			bool		atLeastOneSelected		= false;
			bool		atLeastOneSerial		= false;
			bool		mastercalExist			= false;
			bool		serialPresence			= false;
			bool		doAllCheck				= false;
			ArrayList	masterlist				= new ArrayList(); 
			ArrayList	missingMandatoryList	= new ArrayList();
			ArrayList	masterLicensedList		= new ArrayList(); 
			int			totalCalEnt				= 0;
			bool		isEnt					= false;

			bool powerProd = true;
            IList<string> plusSelected = new List<string>();
            IList<string> nodemomods = new List<string>();
			foreach (DataRow aRow in tableArticles.Rows)
			{
				string		producer	= aRow[TableStrings.ColumnProducer] as string;
                string		prodid      = aRow[TableStrings.ColumnProdID] as string;
                string		AcceptDEMO      = aRow[TableStrings.ColumnAcceptdemo] as string;
                
				string		internalCode= aRow[TableStrings.ColumnInternalCode] as string;
				string		edition		= aRow[TableStrings.ColumnEdition] as string;
				string		name		= aRow[TableStrings.ColumnSignature] as string;
				string		localizedName	= aRow[TableStrings.ColumnNameInCulture] as string;
				string		maxCal		= aRow[TableStrings.ColumnMaxCal] as string;
				string		expression	= aRow[TableStrings.ColumnDependency] as string;
				bool		licensed	= (bool)aRow[TableStrings.ColumnCheck];
				bool		hasSerial	= (bool)aRow[TableStrings.ColumnHasSerial];
                string acceptdemo = aRow[TableStrings.ColumnAcceptdemo] as string;
				bool		mandatory	= (bool)aRow[TableStrings.ColumnMandatory];
				CalTypeEnum caltype		= (CalTypeEnum)aRow[TableStrings.ColumnCalType];
				ModuleModeEnum modMode		= (ModuleModeEnum)aRow[TableStrings.ColumnMode];
				CalUseEnum caluse		= (CalUseEnum)aRow[TableStrings.ColumnCalUse];
				bool		namedcal	= (bool)aRow[TableStrings.ColumnNamedCal];
                ArticleInfo.Avaiability available =  (ArticleInfo.Avaiability)aRow[TableStrings.ColumnAvailable];
                IncludedSMInfo[] includedSM = (IncludedSMInfo[])aRow[TableStrings.ColumnIncludedSM];
                string needless = aRow[TableStrings.ColumnNeedless] as string;
                producer = (producer==null || producer.Length <=0)?internalCode:producer;
                
                // creo lista di moduli dvlpplus selezionati.
                if (modMode == ModuleModeEnum.DVLPPlus && licensed) plusSelected.Add(localizedName);
                //addo i nodemo
                if (modMode == ModuleModeEnum.NoDemo && licensed) nodemomods.Add(localizedName);

				IList<SerialNumberInfo> list = GetSerialsList(name);
				foreach (SerialNumberInfo sni in list)
					if (hasSerial && licensed)
						currentModules.Add(sni.GetSerialWOSeparator());


				serialPresence	= serialPresence || hasSerial;
				bool		isMicroarea		= ActivationObjectHelper.IsPowerProducer(producer) && hasSerial && licensed;
				bool		mastercal		= (caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew);

				VerifyCRC(list, caltype, localizedName);

                //se sono già in stato di serial number spciale non posso trovare un altro serial number speciale, questi serial number devono essere unici all'interno del prodotto.
                if ((IsSpecialSerial(list, caltype) && licensed) && (isDeveloper || isDeveloperPlus || isNFS || isDemo)) 
                { SerialUniformity.ForceAddRepeatedSerial(localizedName, list[0].GetSerialWOSeparator(), caltype, producer); }

                if ((IsSpecialSerial(list, caltype) && licensed) && (!(caltype == CalTypeEnum.Master || caltype == CalTypeEnum.AutoFunctional || caltype == CalTypeEnum.tbf || caltype == CalTypeEnum.MasterNew || caltype == CalTypeEnum.AutoDemo|| caltype == CalTypeEnum.AutoDemo || caltype == CalTypeEnum.AutoTbs))) //hanno messo un serial speciale su un modulo non server...
                {
                    errorsHolder.InvalidSerialNumberForModule.PutError();
                    errorsHolder.InvalidSerialNumberForModule.AddObject(String.Format(LicenceStrings.InvalidSpecialSerialForModule, localizedName));
                }

        		//Se il serial number è un serial per developer( o reseller o distributor) ignorerò alcuni controlli:				
                isDeveloper = isDeveloper || IsDeveloper(list, caltype) && licensed;
                isDeveloperPlus = isDeveloperPlus || IsDeveloperPlus(list, caltype) && licensed;
                isDeveloperPlusk = isDeveloperPlusk || IsDeveloperK(list, caltype) && licensed;
                isNFS = isNFS || IsNFS(list, caltype) && licensed;
				isDemo = isDemo || IsDemo(list, caltype) && licensed;
               if (isDeveloperPlusk &&  !ThereIsARnfsMasterProduct())
                {
                    errorsHolder.NotUniformedSerial.PutError(); 
                    errorsHolder.NotUniformedSerial.AddObject(LicenceStrings.Missing_SerialNumberRNFS/*"Missing Not For Sale Serial"*/);
                }

                if (available == ArticleInfo.Avaiability.Unavailable && licensed)
                {
                    errorsHolder.NotAvailableModule.PutWarning(String.Format(LicenceStrings.NotAvailableModule, localizedName));
                }
                if (available == ArticleInfo.Avaiability.AvailableNFS && licensed)
                {
                    errorsHolder.NotAvailableNFSModule.PutWarning(String.Format(LicenceStrings.NotAvailableModule, localizedName));
                }
                ModuleDependeciesList.Add(new ModuleDependecies(name, localizedName, licensed, expression, includedSM));
                //if (isDeveloper)

                //	break;
				//Se il serial number è un serial per cal stand alone:				
				//isCalStandAlone = isCalStandAlone || IsCalStandAlone(list, caltype) && licensed;

				atLeastOneSerial = atLeastOneSerial || (list != null && list.Count > 0);

				if (ActivationObjectHelper.IsPowerProducer(producer))
					//Se siamo microarea controllo coerenza di edition
					FindUncorrectEdition(list, licensed, name, localizedName, edition);
				//mastercal sono quelli che numerano le cal di tutto l'applicativo
				//I due attributi mandatory e mastercal non possono coesistere
				//(versioni precedenti di mago che avevano 
				//server e sbp con entrambi gli attributi(erroneamente))
				if (mastercal) mandatory = false;
				//errore se mancano serial ma il modulo è selezionato
				FindMissingSerial(list, licensed, hasSerial, localizedName, needless);
				//se il modulo non è obbligatorio controllo che sia stato selezionato(altrimenti avrei doppio controllo)!
				if (!mandatory && !mastercal)
					FindMissingCheck(list, licensed, localizedName);
				//aggiungo alla lista i moduli obbligatori
				if (mandatory && !licensed)
					missingMandatoryList.Add(localizedName);
				//aggiungo alla lista i moduli mastercal
				if (mastercal && namedcal)
				{
					masterlist.Add(localizedName);
					if (licensed)masterLicensedList.Add(localizedName);
					if (!mastercalExist && licensed) mastercalExist = true;
				}
				
				if (ActivationObjectHelper.GetEditionFromString(edition) == Edition.Enterprise && namedcal)
				{
					isEnt = true;
					totalCalEnt += ActivationObjectHelper.CalculateCalEnt(list, caltype);
				}
				//verifica di conformità delle informazioni contenute nel serial
				//if (!isDeveloper /*&& !isDemo*/)
                    FindMissingSerialCompliance(list, name, localizedName, caltype, caluse, licensed, hasSerial, producer, namedcal, prodid, isDemo, isDeveloper, isNFS);
				if (licensed)
					atLeastOneSelected = true;
				powerProd = ActivationObjectHelper.IsPowerProducer(producer);
				//se non è microarea non procedo con ulteriori controlli
				
				//controllo eliminato perchè troppo restrittivo e non accettava or(esempio: server or sbp)
				//FindMissingCalCompliance(maxCal, licensed, name,  localizedName);
				//se è licenziato preparo le informazioni per la verifica di uniformità tra serial
				if (licensed)
					PrepareUniformityCheck(localizedName, list, caltype, producer);
				if (!isMicroarea) 
					continue;
				doAllCheck = true;
			} 

			if (powerProd && totalCalEnt < 10 && isEnt && !isDeveloper && !isNFS & !isDemo)
				errorsHolder.MinorThanTenCal.PutError();
			//verifico che ci sia almeno un modulo, altrimenti che cosa installa?????
			//questo ora vale solo per il prodotto master possiamo pensare che 
			//se non ci sono moduli su un verticale vuol dire che 
			//non lo si vuole attivare e quindi sarebbe opportuno cancellarne il licensed,
			//si da un warning invece che un error.
			bool missingModule = MissingModule(atLeastOneSelected);

			if (IsMasterProduct())
				//verifico che ci sia almeno un modulo mastercal e che il moduli obbligatori siano selezionati
				FindMissingMasterCal(masterlist, mastercalExist, masterLicensedList, missingMandatoryList);
			//se all'inizio non ho nessun modulo selezionato non capisco quale sia il master product 
			//e non mi accorgerei che manca almeno un mastercal, quindi controllo in più:
			else
				if (!mastercalExist && masterlist.Count> 0)//warning per mancanza di mastercal
			{
				
				foreach (string s in masterlist)
				{
					if (!errorsHolder.MissingMandatory.ContainsSubject(s))
					{
						errorsHolder.MissingMasterCal.PutError();
						errorsHolder.MissingMasterCal.AddObject(s);
					}
				}
			}

			//cerco serial ripetuti per TUTTI
			FindRepeatedSerials();

            //se sono stati selezionati dei modulidevPlus (vedi easybuilder) 
            //bisogna che il serial number inserito sia dvlpplus o serial number veri, 
            //non sono accettati seriali demo dvlp rnfs dnfs
            //errore bloccante così se ne accorgono altrimenti poi si presentano

            if ((isDeveloper ) && !isDeveloperPlus && plusSelected.Count>0)
            {
                errorsHolder.DevelopmentPlusIncompatibility.PutError();
                errorsHolder.DevelopmentPlusIncompatibility.AddObject(GetCSV(plusSelected));
            }

            //se siamo in isDemo e sono stati selezionati dei moduli noDemo allora rilascio errore bloccante
            if (isDemo && nodemomods != null && nodemomods.Count > 0)
            {
                errorsHolder.NoDemoModules.PutError();
                errorsHolder.NoDemoModules.AddObject(GetCSV(nodemomods));
            }

            //se versione developer salto i controlli
			//se non è stato inserito neanche un serial number 
			//lascio passare i controlli, sarà una demo nella versione 2.0 .
		    //per i verticali è permesso non indicare nessun serial number a patto che il prodotto master esprima un serial demo.
			if (isDeveloper || isDemo)
			{
				skipControl = true;
				errorsHolder.ClearErrorsForDemoOrDevelopment();
				return;
			}
            //se versione developer salto i controlli
            //se non è stato inserito neanche un serial number 
            //lascio passare i controlli, sarà una demo nella versione 2.0 .
            if (isNFS)
            {
                skipControl = true;
                errorsHolder.ClearErrorsForNFS();
            }

			//controllo seriali disabilitati per date warning su disattivazione mlu.
			List<string> disabledModules = new List<string>();
			foreach (string originalSerial in originalLicensedArticlesList)
				if (!currentModules.Contains(originalSerial))
					disabledModules.Add(originalSerial);
			if (disabledModules.Count > 0)
			{
                string s = GetCSV(disabledModules);
				s += "?";
				errorsHolder.DisablingModule.PutWarning(null);
				errorsHolder.DisablingModule.AddObject(s);
			}

			//tutti i controlli seguenti sono fatti solo per microarea 
			if (doAllCheck)
			{
				//non è stato scritto nessun serialnumber?
				CheckSerialPresence(serialPresence);
              
				//se sono stati scritti i serial e sono tutti conformi verifico la country con quella delle userinfo
				if (!isDeveloper && 
					!ThereIsADemoOrDVLPMasterProduct(true) &&//todo rnfs controlla country?
                    !FindNotUniformedSerials() && 
					errorsHolder.NotSerialPresence.Subjects.Count == 0)
					VerifyCountry();
			}
		}
        
        //---------------------------------------------------------------------
        private string GetCSV(IList<string> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            string res = null;
            foreach (string s in list)
                if (!String.IsNullOrWhiteSpace(s))
                    res += " " + s + ",";

            return res.Remove(res.Length - 1);
        }

		//---------------------------------------------------------------------
		private void VerifyCRC(IList<SerialNumberInfo> list, CalTypeEnum caltype, string localizedName)
		{
		
			foreach (SerialNumberInfo s in list)
			{
				try
				{
					SerialNumber ss = new SerialNumber(s.GetSerialWOSeparator(), caltype);
				
					if (!ss.HasCorrectCrc)
					{
						errorsHolder.CRCFailed.PutError();
						errorsHolder.CRCFailed.AddObject(String.Format("{0} ({1})", localizedName, s.GetSerialWSeparator()));
					}
				}
				catch (SerialNumberFormatException)
				{
					errorsHolder.CRCFailed.PutError();
					errorsHolder.CRCFailed.AddObject(String.Format("{0} ({1})", localizedName, s.GetSerialWSeparator()));
				}
			}
		}

        //---------------------------------------------------------------------
        private bool IsDemo(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
            foreach (SerialNumberInfo s in list)
                if (s.IsDemo(caltype))
                    return true;
            return false;
        }
        
        //---------------------------------------------------------------------
        private bool IsDeveloperK(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
            foreach (SerialNumberInfo s in list)
                if (s.IsDeveloperPlusK(caltype))
                    return true;
            return false;
        }

        //---------------------------------------------------------------------
        private bool IsDeveloper(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
            foreach (SerialNumberInfo s in list)
                if (s.IsDeveloper(caltype))
                    return true;
            return false;
        }

        //---------------------------------------------------------------------
        private bool IsDeveloperPlus(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
           foreach (SerialNumberInfo s in list)
                if (s.IsDeveloperPlus(caltype))
                    return true;
            return false;
        }
		
        //---------------------------------------------------------------------
		private bool IsSpecialSerial(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
            foreach (SerialNumberInfo s in list)
                if (s.IsSpecial(caltype))
                    return true;
            return false;
        }

        //---------------------------------------------------------------------
		private bool IsNFS(IList<SerialNumberInfo> list, CalTypeEnum caltype)
        {
            foreach (SerialNumberInfo s in list)
                if (s.IsNFS(caltype))
                    return true;
            return false;
        }

        //		//---------------------------------------------------------------------
        //		private bool IsCalStandAlone(ArrayList list, CalTypeEnum caltype)
        //		{
        //			foreach (SerialNumberInfo s in list)
        //				if (s.IsCalStandAlone(caltype))
        //					return true;
        //			return false;
        //		}

        //---------------------------------------------------------------------
        private void VerifyCountry()
        {

            string serialCountry = SerialUniformity.GetCountry();
            if (String.IsNullOrWhiteSpace(serialCountry))
                return;
            string userCountry = ParentGridsContainer.GetCountry();
            //21/02/2017 con Germano è stato deciso che sanmarino e vaticano seguono la law info it
            if (String.Compare(userCountry, "sm", StringComparison.InvariantCultureIgnoreCase) == 0 || String.Compare(userCountry, "va", StringComparison.InvariantCultureIgnoreCase) == 0)
                userCountry = "IT";

            bool ok = String.Compare(userCountry, serialCountry, true, CultureInfo.InvariantCulture) == 0;
            if (!ok)
            {
                errorsHolder.NotUniformedCountry.PutError();
                errorsHolder.NotUniformedCountry.AddObject(String.Format(LicenceStrings.UserCountry, userCountry));
                errorsHolder.NotUniformedCountry.AddObject(String.Format(LicenceStrings.SerialCountry, serialCountry));
            }


        }

        //---------------------------------------------------------------------
        private void CheckSerialPresence(bool serialPresence)
		{
			if (!serialPresence)
			{
				errorsHolder.NotSerialPresence.PutWarning(String.Empty);
				errorsHolder.NotSerialPresence.AddObject(null);
			}
		}

		//---------------------------------------------------------------------
		private void PrepareUniformityCheck(string localizedName, IList<SerialNumberInfo> serialsList, CalTypeEnum caltype, string producer)
		{
			foreach (SerialNumberInfo serial in serialsList)
				SerialUniformity.AddSerial(localizedName, serial.GetSerialWOSeparator(), caltype, producer);
		}

		//---------------------------------------------------------------------
		private bool FindNotUniformedSerials()
		{
			if (SerialUniformity.GetNotUniformedList() == null || SerialUniformity.GetNotUniformedList().Length == 0)
				return false;
			foreach (string s in SerialUniformity.GetNotUniformedList())
			{
				errorsHolder.NotUniformedSerial.PutError();
				errorsHolder.NotUniformedSerial.AddObject(s);
			}
			return true;
		}

		//---------------------------------------------------------------------
		private void FindRepeatedSerials()
		{
			string[] list = SerialUniformity.GetRepeatedList();
			if (list == null)
				return;
			foreach (string s in list)
			{
				errorsHolder.RepeatedSerials.PutError();
				errorsHolder.RepeatedSerials.AddObject(s);
			}
		}

		/// <summary>
		///Verifica che che i moduli licenziati e con serial, specifichino effettivamente dei serial.
		/// </summary>
		//--------------------------------------------------------------------- 
		private void FindMissingSerial(IList<SerialNumberInfo> serialList, bool licensed, bool hasSerial, string localizedName, string needless )
		{
            //check ma non serial - permesso solo a verticali se il master ha serial demo //non più così, anche rbf e dvlp
           
			if	((serialList == null || serialList.Count == 0) && licensed && hasSerial) 
			{
                //spiegazione :se siamo con serial nfs o con serial veri il tbs (che ha il marchio tbs nel needless attribute ( fa schifo lo so)) deve avere serial , invece può non averlo se siamo in demo o dvlp
                if ((ThereIsADemoOrDVLPMasterProduct(needless== "tbs")) || (!string.IsNullOrEmpty(needless) && (needless != "tbs")))
				return;
				errorsHolder.MissingSerial.PutError();
				errorsHolder.MissingSerial.AddObject(localizedName);
			}
		}
        

        //---------------------------------------------------------------------
        public bool ThereIsADvlpKProduct()
        {
            return (dirty) ? IsDVLPKInModifiedTab() : currentProd.DevelopPlusVersion;
        }

        //---------------------------------------------------------------------
        public bool IsMasterRNFS()
        {
            bool ok = IsMasterProduct();

           return  (ok)?  ((dirty) ? IsRNFSInModifiedTab() : currentProd.ResellerVersion): false;
     
        }


        //---------------------------------------------------------------------
        public bool IsMasterDemoOrDVLPOrRNFS(bool alsornfs)//todo no rnfs
        {
            bool ok = IsMasterProduct();
            bool acceptdemo = IsMasterProductExtended();
            bool demo = (dirty) ? IsDemoInModifiedTab() : currentProd.DemoVersion;
            bool dvlp = (dirty) ? IsDVLPInModifiedTab() : currentProd.DevelopVersion;
            bool rnfs = (dirty) ? IsRNFSInModifiedTab() : currentProd.ResellerVersion;
            //serialsList demo tortno true in caso si ok o acceptdemo se dvlp toprno solo se ok
            if (ok) return demo || dvlp || ((!alsornfs && rnfs));
            else 
                return demo ? acceptdemo : false;
        }

		/// <summary>
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsDemoInModifiedTab()
		{
			foreach (SerialsModuleInfo hcmi in serialsList)
			{
				bool dm = IsDemo(hcmi.SerialList, hcmi.CalType);
                
				if (dm) return true;
               

			}
			return false;
		}

        /// <summary>
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDVLPKInModifiedTab()
        {
            foreach (SerialsModuleInfo hcmi in serialsList)
            {

                bool dm = IsDeveloperK(hcmi.SerialList, hcmi.CalType);

                if (dm) return true;

            }
            return false;
        }
        
        /// <summary>
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDVLPInModifiedTab()
        {
            foreach (SerialsModuleInfo hcmi in serialsList)
            {

                bool dm = IsDeveloper(hcmi.SerialList, hcmi.CalType);

                if (dm) return true;

            }
            return false;
        }

        /// <summary>
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsRNFSInModifiedTab()
        {
            foreach (SerialsModuleInfo hcmi in serialsList)
            {

                bool dm = IsNFS(hcmi.SerialList, hcmi.CalType);

                if (dm) return true;

            }
            return false;
        }

        //--------------------------------------------------------------------- 
        private bool ThereIsARnfsMasterProduct()
        {
            //devo guardare nelle tab, non nell'oggetto in memoria, perchè magari ho modificato e non ancora salvato.
            if (ParentGridsContainer == null)
                return false;
            return ParentGridsContainer.ThereIsARnfsMasterProduct();
        }


        //--------------------------------------------------------------------- 
        private bool ThereIsADemoOrDVLPMasterProduct(bool alsornfs)
		{
			//devo guardare nelle tab, non nell'oggetto in memoria, perchè magari ho modificato e non ancora salvato.
			if (ParentGridsContainer == null)
				return false;
			return ParentGridsContainer.ThereIsADemoOrDvlpMasterProduct(alsornfs);
		}

		/// <summary>
		///Verifica che che i moduli licenziati e con serial, specifichino effettivamente dei serial.
		/// </summary>
		//--------------------------------------------------------------------- 
		private void FindUncorrectEdition(IList<SerialNumberInfo> serialList, bool licensed, string name, string localizedName, string edition)
		{
			if (!licensed || serialList == null) return;
			
			foreach (SerialNumberInfo s in serialList)
			{	
				Edition serialEdition = SerialNumberInfo.GetEdition(s);
				Edition moduleEdition = Edition.Undefined;
				if (edition != null && edition != String.Empty)
					moduleEdition = ActivationObjectHelper.GetEditionFromString(edition);
                //questo codice non può pìù esistere perchè non indicheremo più l'edition nella signature da M4Go in poi
                //else moduleEdition = GetEditionFromModuleName(name);
				if (serialEdition != moduleEdition)
                {
                    if (moduleEdition != Edition.ALL)
                    {
                        errorsHolder.EditionUncorrect.PutError();
                        errorsHolder.EditionUncorrect.AddObject(localizedName);
                    }
				}
			}
		}

        ////--------------------------------------------------------------------- 
        //private Edition GetEditionFromModuleName(string name)
        //{
        //    //supponiamo che magonet-PRO.server.xml sia il nome da prendre in considerazione per ricavare edition del modulo?
        //    //DA M4GO non più valido, unica signature.....
        //    int dotIndex = name.IndexOf(".");
        //    int minusIndex = name.IndexOf("-");
        //    int len = dotIndex - minusIndex - 1; 
        //    if (minusIndex == -1 || len < 1)
        //        return Edition.Undefined;
        //    string moduleEdition = name.Substring(minusIndex + 1, len);
        //    if (String.Compare(moduleEdition, NameSolverStrings.StdEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Standard;
        //    if (String.Compare(moduleEdition, NameSolverStrings.ProEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Professional;
        //    if (String.Compare(moduleEdition, NameSolverStrings.EntEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Enterprise;
        //    return Edition.Undefined;
        //}
		
        ////--------------------------------------------------------------------- 
        //private Edition GetEditionFromString(string edition)
        //{
        //    if (String.Compare(edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Standard;
        //    if (String.Compare(edition, NameSolverStrings.ProfessionalEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Professional;
        //    if (String.Compare(edition, NameSolverStrings.EnterpriseEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Enterprise;
        //    return Edition.Undefined;
        //}

		/// <summary>
		/// Verifica che il modulo specifichi serial e sia selezionato, è solo un warning.
		/// </summary>
		//---------------------------------------------------------------------
		private void FindMissingCheck(IList<SerialNumberInfo> serialList, bool licensed, string localizedName)
		{
			//serial ma non check
			if	(serialList != null && serialList.Count > 0 && !licensed) 
			{
				errorsHolder.MissingCheck.PutWarning(LicenceStrings.IgnoreMissingCheck);
				errorsHolder.MissingCheck.AddObject(localizedName);
			}
		}

//		/// <summary>
//		/// Verifica che il numero di cal specificato dal modulo, sia nel massimo consentito dall'attributo MaxCal
//		/// </summary>
//		//---------------------------------------------------------------------
//		private void FindMissingCalCompliance(string maxCalModule, bool licensed, string name, string localizedName)
//		{
//			//coerenza maxcal
//			if (maxCalModule != null && maxCalModule != String.Empty && licensed)
//			{
//				int cal = GetCalInSpecificPage(name);
//				int maxCal = GetCalInSpecificPage(maxCalModule);
//				if (cal > maxCal)
//				{
//					errorsHolder.MissingCalCompliance.PutError();
//					errorsHolder.MissingCalCompliance.AddObject(localizedName);
//				}
//			}
//		}

		/// <summary>
		/// Verifica che i moduli che devono esprimere serial di cal lo facciano, 
		/// e che ogni modulo abbia il proprio serial(che magari non esprima il serial di un altro modulo).
		/// </summary>
		//---------------------------------------------------------------------
        private void FindMissingSerialCompliance(IList<SerialNumberInfo> serialList, string name, string localizedName, CalTypeEnum caltype, CalUseEnum caluse, bool licensed, bool hasserial, string producer, bool namedcal, string prodID, bool isDemo, bool isDvlp, bool isNFS)
		{
			//coerenza serial-modulo

			//non segnalo quelli già inseriti nella lista di quelli cui manca la selezione pur avendo il serial
			//if (errorsHolder.MissingCheck.ContainsSubject(localizedName)) return;

			//non segnalo quelli non selezionati
			if (!licensed || serialList == null) return;
			
			bool hasAssociatedCal	= false;
			bool hasSerialForModule = false;
			foreach (SerialNumberInfo s in serialList)
			{	
				string	shortName		= SerialNumberInfo.GetModuleShortNameFromSerial(s, caltype);

                if (SerialNumber.IsMeaningLess(shortName, ActivationObjectHelper.IsPowerProducer(producer)) || 
                    SerialNumber.IsIntegrativeSerial(shortName, ActivationObjectHelper.IsPowerProducer(producer)))
                {
                    if (prodID!= SerialNumberInfo.GetProdId(s))
                    { 
                        errorsHolder.MissingSerialCompliance.PutError();
                        errorsHolder.MissingSerialCompliance.AddObject(String.Format(localizedName));
                    }
                    else continue;
                }

                //se è cal wms mobile va bene
                if (SerialNumber.IsWMSMobile(shortName, ActivationObjectHelper.IsPowerProducer(producer)))
                    continue;

				bool	hasCalNumber	= SerialNumberInfo.GetCalNumberFromSerial(name, s, caltype, namedcal) > 0;
				if (shortName != null && shortName != String.Empty)
				{
					//esiste short name
					ModuleNameInfo longNameInfo = shortNames[shortName] as ModuleNameInfo ;
					if (longNameInfo != null)
					{
						//lo short name non corrisponde al nome del modulo
						if (String.Compare(longNameInfo.Name, name, true, CultureInfo.InvariantCulture) != 0)
						{
							errorsHolder.MissingSerialCompliance.PutError();
							errorsHolder.MissingSerialCompliance.AddObject(String.Format(LicenceStrings.MsgMistakenSerial, localizedName, longNameInfo.LocalizedName));
						}
						else 
							hasSerialForModule = true;

					}
						//lo short name non riporta a nessun modulo se demo e dvlp esco  da if
					else if (!isDemo && !isDvlp && !isNFS)
					{
							errorsHolder.MissingSerialCompliance.PutError();
							errorsHolder.MissingSerialCompliance.AddObject(String.Format(localizedName));
					}

                    //Coerenza ProdId tra modulo e serial, se non corrisponde do errore.
                    if (prodID != null && prodID.Length > 0)
                    {
                        string prodid = SerialNumberInfo.GetProdId(s);
                        if (String.Compare(prodid, prodID, true, CultureInfo.InvariantCulture) != 0)
                        {
                            if ((!isDemo && !isDvlp) || String.Compare(prodid, currentProd.GetAcceptDemo(), true, CultureInfo.InvariantCulture) != 0)
                            {
                                errorsHolder.MissingSerialCompliance.PutError();
                                errorsHolder.MissingSerialCompliance.AddObject(String.Format(LicenceStrings.MsgMistakenSerial, localizedName, longNameInfo != null ? longNameInfo.LocalizedName : ("UNKNOWN: " + s.GetSerialWOSeparator())));
                            }
                        }
                    }

				}
				
				else //non esiste lo short name, deve essere un serial di CAL
				{
					if (!hasCalNumber)
					{
						if (caluse == CalUseEnum.Function)	
						{
							//devo accettare i numeri di serie di cal di terze parti
						}
						else
						{
							errorsHolder.MissingSerialCompliance.PutError();
							errorsHolder.MissingSerialCompliance.AddObject(localizedName);
						}
					}
				}
				hasAssociatedCal = hasAssociatedCal || hasCalNumber;

			}//foreach

			//Completezza dei serial number per quei moduli che essendo mastercal hanno bisogno di serial per modulo e serial per cal
			if (hasserial && (caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew) && licensed && (!hasAssociatedCal || !hasSerialForModule))
			{

				if (!hasAssociatedCal)//non ha il serial di cal
				{
					errorsHolder.MissingCalSerial.PutError();
					errorsHolder.MissingCalSerial.AddObject(localizedName);
				}
				else//non ha il serial di modulo
				{
						errorsHolder.MissingSerialCompleteness.PutError();
						errorsHolder.MissingSerialCompleteness.AddObject(localizedName);
				}
			}
			//Completezza dei serial number per quei moduli che essendo server  hanno bisogno di serial per modulo e 
			//serial per cal facoltativi, ma obbligatoriamente associati alla presenza di un serial server
			if (hasserial && (caltype == CalTypeEnum.Server || caltype == CalTypeEnum.TPGate) && licensed && hasAssociatedCal && !hasSerialForModule)
			{
				errorsHolder.MissingSerialCompleteness.PutError();
				errorsHolder.MissingSerialCompleteness.AddObject(localizedName);
			}
			
		}
		/// <summary>
		/// Verifica che nella configurazione selezionata ci sia almeno un modulo selezionato.
		/// </summary>
		//---------------------------------------------------------------------
		private bool MissingModule(bool atLeastOneSelected)
		{
			if (!atLeastOneSelected)
			{
				if (IsMasterProduct())
					errorsHolder.MissingModule.PutError();
				else
					errorsHolder.MissingModule.PutWarning(LicenceStrings.NoModuleSelectedWarning);

                errorsHolder.MissingModule.AddObject(currentProd.CompleteName);
			}
			return !atLeastOneSelected;
		}

		//---------------------------------------------------------------------
		private bool IsMasterProduct()
		{
            bool ok = false;
            ok = clientStub.ActivationObj.GetMasterProductID() == currentProd.ProductId;
            if (!ok) ok = clientStub.ActivationObj.GetMasterProductID() == currentProd.GetAcceptDemo();
            return ok;
        }

        //---------------------------------------------------------------------
        private bool IsMasterProductExtended()
        {
            bool ok = clientStub.ActivationObj.GetMasterProductID() == currentProd.ProductId;
            if (!ok) ok = clientStub.ActivationObj.GetMasterProductID() == currentProd.GetAcceptDemo();
            return ok;
        }

		/// <summary>
		/// Verifica che nella configurazione selezionata ci sia almeno un mastercal.
		/// </summary>
		//---------------------------------------------------------------------
		private void FindMissingMasterCal(ArrayList masterlist, bool mastercalExist, ArrayList masterLicensedList, ArrayList missingMandatoryList)
		{
			if (missingMandatoryList.Count > 0)//errore perchè se sono obbligatori devono essere selezionati tutti.
			{
				errorsHolder.MissingMandatory.PutError();
				foreach (string s in missingMandatoryList)
					errorsHolder.MissingMandatory.AddObject(s);
			}

			if (!mastercalExist || masterlist == null)//warning per mancanza di mastercal
			{
				
				foreach (string s in masterlist)
				{
					if (!errorsHolder.MissingMandatory.ContainsSubject(s))
					{
						errorsHolder.MissingMasterCal.PutError();
						errorsHolder.MissingMasterCal.AddObject(s);
					}
				}
			}
			else if (masterLicensedList.Count > 1)//se ci sono più di un mastercal do errore
			{
				errorsHolder.TooManyMasterCal.PutError();
				foreach (string s in masterLicensedList)
					errorsHolder.TooManyMasterCal.AddObject(s);
			}
		}

		

		#endregion

		//---------------------------------------------------------------------
		private void DGArticles_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			DataGrid.HitTestInfo myHitTest	= DGArticles.HitTest(e.X, e.Y);
			if (myHitTest == null)
				return;

			int column = myHitTest.Column;
			if (column != (int)ColumnIndexer.ColumnArticle)
				return;

			int row	= myHitTest.Row;
			if (row > -1) 
			{
				DataRow dr = null;
				if (articleView != null && articleView[row] != null)
					dr = articleView[row].Row;
				if (dr == null) 
					return;
				//visualizzo internal code
				string producer = dr[TableStrings.ColumnPrivateCode] as string;
				if (producer == null || producer.Length == 0)
					return;
				string stringToShow = String.Format(LicenceStrings.Producer, producer);	
				ToolTipOnDG.SetToolTip(DGArticles, stringToShow);
			}
		}

		//---------------------------------------------------------------------------
		private void ViewDiagnostic()
		{
			DiagnosticViewer.ShowDiagnostic(diagnostic);
		}

		//---------------------------------------------------------------------
		public static void SetAndViewError(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Error);
			DiagnosticViewer.ShowDiagnostic(diagnostic);
		}
		//---------------------------------------------------------------------
		public static void SetError(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Error);
		}

		//---------------------------------------------------------------------
		public static void SetWarning(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Warning);
		}
		//---------------------------------------------------------------------
		public static void SetInformation(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Information);
		}

		//---------------------------------------------------------------------
		public static void SetMessage(string message, string extendedInfoName, string extendedInfoValue, DiagnosticType type)
		{
			if (message == null || message == String.Empty)
				return;
			ExtendedInfo infos = null;
			if (extendedInfoName != null && extendedInfoName != String.Empty && extendedInfoValue != null && extendedInfoValue != String.Empty)
				infos = new ExtendedInfo(extendedInfoName, extendedInfoValue);
			if (infos == null)
				diagnostic.SetInformation(message);
			else
				diagnostic.Set(type, message, infos);
		}

		//---------------------------------------------------------------------
		private delegate void SetCursorDelegate(Cursor cursor);
		//---------------------------------------------------------------------
		private void SetCursor(Cursor cursor)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new SetCursorDelegate(SetCursor), new object[] { cursor });
			}
			else
				Cursor = cursor;
		}

	}

	#region TableString
	//=========================================================================
	public class TableStrings
	{
		public static string TableArticles		= "Articles";
		public static string ColumnMandatory	= "Mandatory";
		public static string ColumnMaxCal		= "MaxCal";
		public static string ColumnProducer		= "Producer";
        public static string ColumnProdID       = "ProdID";
		public static string ColumnInternalCode	= "InternalCode";
		public static string ColumnPrivateCode	= "PrivateCode";
		public static string ColumnEdition		= "Edition";
		public static string ColumnCalType		= "CalType";
		public static string ColumnNamedCal		= "NamedCal";
        public static string ColumnAvailable = "Available";
        public static string ColumnHasSerial	= "HasSerial";
		public static string ColumnSerial		= "Serial";
		public static string ColumnSignature	= "Signature";
		public static string ColumnNameInCulture= "NameInCulture";
		public static string ColumnDependency	= "Dependency";
        public static string ColumnAcceptdemo   = "AcceptDEMO";
		public static string ColumnCheck		= "Check";
        public static string ColumnModifiable = "Modifiable"; 
        public static string ColumnModules		= "Modules";
		public static string ColumnIncludedSM	= "IncludedSM";
		public static string ColumnMode		    = "Mode";
		public static string ColumnCalUse	    = "CalUse";
        public static string ColumnNeedless = "Needless";
        


    }
	#endregion

	#region ColumnIndexer
	//=========================================================================
	public class ColumnIndexer
	{
		public static int ColumnCheck	= 0;
		public static int ColumnArticle	= 1;
		public static int ColumnSerial	= 2;
	}
	#endregion
	
	#region SerialUniformity
	/// <summary>
	/// Continene i metodi per immagazzinare i serial number e suddividerli 
	/// per poi identificare quelli che non hanno caratteristiche omogenee agli altri
	/// </summary>
	//=========================================================================
	public class SerialUniformity
	{
		private static Hashtable first = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		private static Hashtable second = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		private static	Hashtable	repeated	= null;

		/// <summary>
		/// Aggiunge i serial number a liste diverse in base ai dati che specificano
		/// </summary>
		//---------------------------------------------------------------------
		public static void ClearAll()
		{
			first.Clear();
			second.Clear();
			if (repeated != null)
				repeated = null;
		}

		//=====================================================================
		private struct SnInfo
		{
			public string LocalizedName;
			public string Producer;
			public SnInfo(string localizedName, string producer)
			{
				LocalizedName	= localizedName;
				Producer		= producer;
			}

		}

		/// <summary>
		/// Aggiunge i serial number a liste diverse in base ai dati che specificano
		/// </summary>
		//---------------------------------------------------------------------
		internal static bool AddSerial(string localizedName, string serialNumber, CalTypeEnum caltype, string producer)
		{
			SerialNumber sn = null;
			try
			{
				sn = new SerialNumber(serialNumber, caltype);
			}
			catch (SerialNumberFormatException)
			{
				return !ActivationObjectHelper.IsPowerProducer(producer);
				//return false;
			}
			if (sn == null) return false;
			//comincio a popolare la prima lista, che verrà presa come esempio
			if (first.Count == 0)
			{
				first.Add(sn, new SnInfo(localizedName, producer));
				return true;
			}
			//caso di serial ripetuto(Verifica ripetizione per produttore)
			bool rep = false;
			if (first.Contains(sn))
			{
				SnInfo sni = (SnInfo)first[sn];
				rep = String.Compare(sni.Producer, producer, true, CultureInfo.InvariantCulture) == 0;
			}
			if (!rep && second.Contains(sn))
			{
				SnInfo sni = (SnInfo)second[sn];
				rep = String.Compare(sni.Producer, producer, true, CultureInfo.InvariantCulture) == 0;
			}
			if (rep)
			{
				if (repeated == null)
					repeated = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
				if (!repeated.Contains(sn))
					repeated.Add(sn, new SnInfo(localizedName, producer));
				return true; 
			}
			//se corrisponde al primo tipo viene aggiunto, altrimenti viene aggiunto alla seconda lista
			if (VerifyUniformity(sn))
				first.Add(sn, new SnInfo(localizedName, producer));
			else
				second.Add(sn, new SnInfo(localizedName, producer));
			return true;
		}

        //---------------------------------------------------------------------
        internal static void ForceAddRepeatedSerial(string localizedName, string serialNumber, CalTypeEnum caltype, string producer)
        {
            SerialNumber sn = null;
            if (string.IsNullOrWhiteSpace(serialNumber)) return;
            try
            {
                sn = new SerialNumber(serialNumber, caltype);
            }
            catch (SerialNumberFormatException)
            {
                return;
            }

            if (repeated == null)
                repeated = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            if (!repeated.Contains(sn))
                repeated.Add(sn, new SnInfo(localizedName, producer));

        }

		/// <summary>
		/// Verifica che il serial number passato sia dello stesso tipo della lista che fa da esempio
		/// </summary>
		//---------------------------------------------------------------------
		public static bool VerifyUniformity(SerialNumber sn)
		{
			//basta controllare solo il primo (a meno che non abbia il db all, allora vedo se ne trovo un altro)
			if (first == null || first.Count == 0) return false;
			foreach (DictionaryEntry entry in first)
			{
				SerialNumber inList = entry.Key as SerialNumber;
				//se mi sta arrivando un dbfree non mi importa di cosa ci sia in lista
				if (sn.IsDbFree())
					return sn.IsTheSameType(inList);
				else if (inList.IsDbFree()) //altrimenti se il primo della lista e dbfree continuo a cercarne un altro
					continue;
				else //altrimenti verifico normalemte
					return sn.IsTheSameType(inList);
			}
			return true;// possibile caso di tutti in lista dbfree
		}

		/// <summary>
		/// Restituisce la lista dei nomi dei moduli che contengono serial number sbagliati
		/// o supposti tali, basandosi sul numero di occorrenze di ogni lista e 
		/// supponendo che la lista col maggior numero di entry sia quella corretta
		/// </summary>
		//---------------------------------------------------------------------
		public static string[] GetNotUniformedList()
		{
			//recupero la lista col minor numero di entry, supponendo che sia quella che con i serial sbagliati
			ArrayList incorrectList = new ArrayList();
			Hashtable hashtable = (first.Count > second.Count)? second : first;
			foreach (DictionaryEntry entry in hashtable)
			{
				SnInfo sni = (SnInfo)entry.Value;
				incorrectList.Add(sni.LocalizedName);
			}
			return (string[])incorrectList.ToArray(typeof(string));
		}

		/// <summary>
		/// Restituisce la lista dei nomi dei moduli che contengono serial number ripetuti
		/// </summary>
		//---------------------------------------------------------------------
		public static string[] GetRepeatedList()
		{
			if (repeated == null) return null;
			ArrayList repeatedList = new ArrayList();
			foreach (DictionaryEntry entry in repeated)
			{
				SnInfo sni = (SnInfo)entry.Value;
				repeatedList.Add(sni.LocalizedName);
			}
			return (string[])repeatedList.ToArray(typeof(string));
		}

		/// <summary>
		/// Restituisce la country del primo serialNumber della lista
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetCountry()
		{
			if (first == null || first.Count == 0)
				return String.Empty;
			string country = String.Empty;
			foreach (DictionaryEntry e in first)
			{
				SerialNumber sn = e.Key as SerialNumber;
				if (sn != null)
					return sn.Country;
			}
			return country;
		}
	}

	#endregion

	#region FillingError
	//=========================================================================
	public class FillingError
	{
		public enum			ErrorType {Error, Warning, None};
		/// <summary>Tipologia d'errore</summary> 
		public ErrorType	Error = ErrorType.None;
		/// <summary>Testo del messaggio che spiega l'errore</summary> 
		public string		Text;
		/// <summary>Testo del messaggio che spiega l'errore</summary> 
		public string		WarningText;
		/// <summary>Lista dei nomi dei moduli(localizzati)/SERIALI che hanno riportato un errore</summary> 
		public ArrayList	Subjects = new ArrayList();

		//---------------------------------------------------------------------
		public FillingError (ErrorType errorType, string text, ArrayList list)
		{
			Text = text;
			Error = errorType;
			if (list != null)
				Subjects = list;
		}

		//---------------------------------------------------------------------
		public FillingError (ErrorType errorType, string text)
		{
			Text  = text;
			Error = errorType;
		}

		//---------------------------------------------------------------------
		public void AddObject (string name)
		{
			if (name == null || name == String.Empty)
				return;
			if (!Subjects.Contains(name))
				Subjects.Add(name);
		}

		//---------------------------------------------------------------------
		public bool ContainsSubject(string name)
		{
			return (Subjects.Contains(name));
		}

		//---------------------------------------------------------------------
		public bool IsOK()
		{
			return (Error == ErrorType.None);
		}

		//---------------------------------------------------------------------
		public void PutError()
		{
			if (Error == ErrorType.None)
				Error = ErrorType.Error;
		}

		//---------------------------------------------------------------------
		public void PutWarning(string warningText)
		{
			if (Error == ErrorType.None)
			{
				Error = ErrorType.Warning;
				WarningText = warningText;
			}
		}

		/// <summary>
		/// Ripulisce gli errori e setta l'errore a none.
		/// </summary>
		//---------------------------------------------------------------------
		public void Clear()
		{
			Error = ErrorType.None;
			Subjects.Clear();
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (Error == ErrorType.None) return String.Empty;
			StringBuilder sb = new StringBuilder();
			sb.Append(Text);
			sb.Append(Environment.NewLine);
			//Se è solo uno potrei non mettere la numerazione....
			if (Subjects.Count ==1)
			{
				sb.Append(Subjects[0] as string);
				sb.Append(Environment.NewLine);
			}
			else
			for (int i = 0; i < Subjects.Count; i++)
			{	
				int index = i + 1;
				sb.Append(index.ToString());
				sb.Append(". ");
				sb.Append(Subjects[i] as string);
				sb.Append(Environment.NewLine); sb.Append(Environment.NewLine); 
			}

			if (Error == ErrorType.Warning && WarningText != null && WarningText != String.Empty)
			{
				sb.Append(WarningText);
				sb.Append(Environment.NewLine);
			}
			sb.Append(Environment.NewLine);
			return sb.ToString();
		}
	}
	#endregion

	
	#region ErrorsHolder
	//=========================================================================
	/// <summary>
	/// Contiene suddivise per tipologia di errore , i vari errori di compilazione del licensedForm
	/// </summary>
	public class ErrorsHolder
	{
		/// <summary>Segnala i moduli selezionati cui manca il serialNumber </summary>
		public FillingError MissingSerial = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingSerial);
		/// <summary>Segnala i moduli cui manca la selezione, ma con il serial </summary>
		public FillingError MissingCheck = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingCheck);
		/// <summary>Segnala i moduli che hanno un numero di cal maggiore del possibile</summary>
		public FillingError MissingCalCompliance = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingCalCompliance);
		/// <summary>Segnala la mancanza delle chiavi di attivazione </summary>
		public FillingError MissingKey = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingKey);
		/// <summary>Segnala che non è stato selezionato neanche un modulo mastercal</summary>
		public FillingError MissingMasterCal = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingMasterCal);
		/// <summary>Segnala che non è stato selezionato neanche un modulo obbligatorio</summary>
		public FillingError MissingMandatory = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingMandatory);
		/// <summary>Segnala i moduli col serial number sbagliato(e l'eventuale modulo cui corrisponderebbe il serial inserito)</summary>
		public FillingError MissingSerialCompliance = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingSerialCompliance);
		/// <summary>Segnala i moduli col incompletezza diserial number, nel caso ci sia bisogno di serial di modulo più serial di cal</summary>
		public FillingError MissingSerialCompleteness = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingSerialCompleteness);
		/// <summary>Segnala i moduli col incompletezza diserial number, nel caso ci sia bisogno di serial di modulo più serial di cal</summary>
		public FillingError MissingCalSerial = new FillingError(FillingError.ErrorType.None, LicenceStrings.MissingCalCompleteness);
		/// <summary>Segnala la presenza di più moduli mastercal selezionati contemporaneamente, deve essercene uno e uno solo.</summary>
		public FillingError TooManyMasterCal = new FillingError(FillingError.ErrorType.None, LicenceStrings.TooManyMasterCal);
		/// <summary>Segnala la presenza di serial number che specificano dati non omogenei agli altri serials.</summary>
		public FillingError NotUniformedSerial = new FillingError(FillingError.ErrorType.None, LicenceStrings.NotUniformedSerial);
		/// <summary>Segnala la presenza di serialnumber uguali ripetuti.</summary>
		public FillingError RepeatedSerials = new FillingError(FillingError.ErrorType.None, LicenceStrings.RepeatedSerials);
		/// <summary>Segnala la non conformità tra la country dello userinfo e quella specificata da tutti i serialnumber.</summary>
		public FillingError NotUniformedCountry = new FillingError(FillingError.ErrorType.None, LicenceStrings.Country_Not_Valid);
		/// <summary>Segnala la mancanza di serial number, almeno uno ci deve essere.</summary>
		public FillingError NotSerialPresence = new FillingError(FillingError.ErrorType.None, LicenceStrings.NoSerialPresence);
		/// <summary>Segnala la mancanza di serial number, almeno uno ci deve essere.</summary>
		public FillingError EditionUncorrect = new FillingError(FillingError.ErrorType.None, LicenceStrings.EditionUncorrect);
		/// <summary>Segnala la mancanza di serial number, almeno uno ci deve essere.</summary>
		public FillingError MissingModule = new FillingError(FillingError.ErrorType.None, LicenceStrings.NoModuleSelected);
		/// <summary>Segnala crc non corrisponde</summary>
		public FillingError CRCFailed = new FillingError(FillingError.ErrorType.None, LicenceStrings.CRCFailed);
		/// <summary>Segnala dipendenze funzionali non soddisfatte</summary>
		public  FillingError DependenciesMissing	= new FillingError(FillingError.ErrorType.None, "");
		/// <summary>Meno di 10 cal enterprise</summary>
		public FillingError MinorThanTenCal = new FillingError(FillingError.ErrorType.None, LicenceStrings.TenCalNeeded);
		/// <summary>Disattivazione di moduli</summary>
		public FillingError DisablingModule = new FillingError(FillingError.ErrorType.None, LicenceStrings.DisablingModule);
        
		/// <summary>presenza di moduli nodemo su attivazione demo (inserito per ibsuite in prova 19/06/2012. spero non faccia casini altrove)</summary>
        public FillingError NoDemoModules = new FillingError(FillingError.ErrorType.None, LicenceStrings.NoDemoModules);

        /// <summary>Modulo Development plus selezionato ma non abilitabile</summary>
        public FillingError DevelopmentPlusIncompatibility = new FillingError(FillingError.ErrorType.None, LicenceStrings.DevelopmentPlusIncompatibility);
        /// <summary>Segnala serial number errati rispetto al modulo</summary>
        public FillingError InvalidSerialNumberForModule = new FillingError(FillingError.ErrorType.None, "" );

        /// <summary>Segnala moduli non disponibili- ma per nfs e dviu si</summary>
        public FillingError NotAvailableNFSModule = new FillingError(FillingError.ErrorType.None, "");

        /// <summary>Segnala moduli non disponibili</summary>
        public FillingError NotAvailableModule = new FillingError(FillingError.ErrorType.None, "");

        /// <summary>
        /// Restituisce la lista di errori contenuti nell'oggetto
        /// </summary>
        //---------------------------------------------------------------------
        public IList<FillingError> GetAllFillingError()
		{
			IList<FillingError> list = new List<FillingError>();
			list.Add(CRCFailed);
			list.Add(DependenciesMissing);
			list.Add(NotSerialPresence);
			list.Add(MissingSerial);
			list.Add(MissingCheck);
			list.Add(MissingCalCompliance);
			list.Add(MissingMasterCal);
			list.Add(MissingMandatory);
			list.Add(TooManyMasterCal);
			list.Add(MissingSerialCompliance);
			list.Add(MissingSerialCompleteness);
			list.Add(MissingCalSerial);
			list.Add(NotUniformedSerial);
			list.Add(RepeatedSerials);
			list.Add(NotUniformedCountry);
			list.Add(EditionUncorrect);
			list.Add(MissingModule);
			list.Add(MissingKey);
			list.Add(MinorThanTenCal);
            list.Add(DisablingModule); 
            list.Add(NoDemoModules);
            list.Add(DevelopmentPlusIncompatibility);
            list.Add(InvalidSerialNumberForModule);
            list.Add(NotAvailableModule); list.Add(NotAvailableNFSModule);
            return list;
		}
		//---------------------------------------------------------------------
		public bool IsOK()
		{
			return CRCFailed.IsOK() &&
				DependenciesMissing.IsOK() &&
				NotSerialPresence.IsOK() &&
				MissingSerial.IsOK() &&
				MissingCheck.IsOK() &&
				MissingCalCompliance.IsOK() &&
				MissingMasterCal.IsOK() &&
				MissingMandatory.IsOK() &&
				TooManyMasterCal.IsOK() &&
				MissingSerialCompliance.IsOK() &&
				MissingSerialCompleteness.IsOK() &&
				MissingCalSerial.IsOK() &&
				NotUniformedSerial.IsOK() &&
				RepeatedSerials.IsOK() &&
				NotUniformedCountry.IsOK() &&
				EditionUncorrect.IsOK() &&
				MissingModule.IsOK() &&
				MissingKey.IsOK() &&
				MinorThanTenCal.IsOK() &&
				DisablingModule.IsOK() &&
                NoDemoModules.IsOK() && 
                DevelopmentPlusIncompatibility.IsOK() &&
                NotAvailableModule.IsOK() && NotAvailableNFSModule.IsOK() &&
                InvalidSerialNumberForModule.IsOK();
		}

		//---------------------------------------------------------------------
		public IList<FillingError> GetLightFillingError()
		{
			IList<FillingError> list = new List<FillingError>();
			list.Add(MissingSerial);
			list.Add(MissingCalSerial);
			list.Add(MissingSerialCompleteness);
			list.Add(NotAvailableNFSModule);
			list.Add(NotUniformedCountry);
			return list;
		}
        //---------------------------------------------------------------------
		public IList<FillingError> GetLightNFSFillingError()
        {
			IList<FillingError> list = new List<FillingError>();
            list.Add(MissingSerial);
            list.Add(MissingCalSerial);
            list.Add(MissingSerialCompleteness);
            list.Add(MissingSerialCompliance);
            list.Add(NotAvailableNFSModule);
            return list; 
        }

		//---------------------------------------------------------------------
		public void ClearAllErrors()
		{
			foreach (FillingError error in GetAllFillingError())
				error.Clear();
		}

        //---------------------------------------------------------------------
        public void ClearErrorsForNFS()
        {
            foreach (FillingError error in GetLightNFSFillingError())
                error.Clear();
        }

		//---------------------------------------------------------------------
		public void ClearErrorsForDemoOrDevelopment()
		{
			foreach (FillingError error in GetLightFillingError())
				error.Clear();
		}


	}
	#endregion

		
	#region ActivationKeyGeneratorException
	//=========================================================================
	public class ActivationKeyGeneratorException : ApplicationException
	{

		public string GenericMessage = LicenceStrings.Unknown_Error;
		public int ErrorCode			= -1;
		public int ErrorId				= -1;
		public string Details; 
		private string specificMessage = null;
		public string SpecificMessage				
		{
			get 
			{
				if (specificMessage == null)
					return ActivationKeyErrors.GetStringFromErrorCode(ErrorCode, Details);
				return specificMessage;
			}
			
		}
		public string Title							{get {return ActivationKeyErrors.GetTitleFromErrorCode(ErrorCode);}}
		public ActivationKeyErrors.MessageType Type {get {return ActivationKeyErrors.GetMessageTypeFromErrorCode(ErrorCode);}}
		//---------------------------------------------------------------------
		public ActivationKeyGeneratorException ()
		{}

		//---------------------------------------------------------------------
		public ActivationKeyGeneratorException (int errorCode, int errorId, string details)
		{
			this.ErrorCode			= errorCode;
			this.ErrorId			= errorId;
			this.Details			= details;
			string errorIdentifier = String.Format(LicenceStrings.ErrorIdentifier, errorCode.ToString(), errorId.ToString());
			GenericMessage			= String.Concat(errorIdentifier, " ", SpecificMessage);
		}

		//---------------------------------------------------------------------
		public ActivationKeyGeneratorException (string message)
		{
			if (message != null && message.Length > 0) 
				specificMessage = message;
		}

	}
	//=========================================================================
	public class ActivationKeyGeneratorSuccessEvent : ActivationKeyGeneratorException
	{
		public new string SpecificMessage { get {return null;}}

		//---------------------------------------------------------------------
		public ActivationKeyGeneratorSuccessEvent (int errorCode)
		{
			GenericMessage	= ActivationKeyErrors.GetStringFromErrorCode(errorCode, null);
			ErrorCode		= errorCode;
		}

	}


	#endregion

	/// <summary>
	/// ColumnStyle che fa visualizzare la colonna (solo se la cella è attivata) con un SerialTextBoxes control.
	/// </summary>
	//=========================================================================
	public class SerialColumnStyle : DataGridColumnStyle
	{
		private int xMargin = 2;
		private int yMargin = 1;
		private string oldVal = new string(string.Empty.ToCharArray());
		private bool inEdit = false;

		private SerialField serialField;
		private bool editable = true;

		public bool Editable
		{
			get { return editable; }
			set { editable = value; }
		}

		public bool Enabled
		{
			get { return serialField.Enabled; }
			set { serialField.Enabled = value; }
		}

		public event ModificationManager Modified;
		public event ExceptionManager ExceptionRaised;
		//---------------------------------------------------------------------
		public SerialColumnStyle()
		{
			serialField = new SerialField();
			serialField.Visible = false;
			serialField.Modified += new ModificationManager(serialField_Modified);
			serialField.ExceptionRaised += new ExceptionManager(serialField_ExceptionRaised);
		}

		//---------------------------------------------------------------------
		private void serialField_ExceptionRaised(object sender, ExceptionEventArgs e)
		{
			if (ExceptionRaised != null)
				ExceptionRaised(sender, e);
		}

		//---------------------------------------------------------------------
		private void serialField_Modified(object sender, EventArgs e)
		{
			if (Modified != null)
				Modified(sender, e);
		}
		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber)
		{
			System.Diagnostics.Debug.WriteLine("Abort()");
			RollBack();
			HideSerialField();
			EndEdit();
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource, int rowNumber)
		{
			HideSerialField();
			if (!inEdit) return true;

			try
			{
				object Value = serialField.SerialString;
				if (Value == null)
					return false;
				if (NullText.Equals(Value))
					Value = System.Convert.DBNull;
				SetColumnValueAtRow(dataSource, rowNumber, Value);
			}
			catch
			{
				RollBack();
				return false;
			}

			this.EndEdit();
			return true;
		}

		//---------------------------------------------------------------------
		protected override void ConcedeFocus()
		{
			serialField.Visible = false;
		}

		//---------------------------------------------------------------------
		protected override void Edit
			(
			CurrencyManager source,
			int rowNumber,
			Rectangle bounds,
			bool readOnly,
			string instantText,
			bool cellIsVisible
			)
		{
			if (!Editable) return;

			Rectangle originalBounds = bounds;
			oldVal = serialField.SerialString;

			if (cellIsVisible)
			{
				bounds.Offset(xMargin, yMargin);
				bounds.Width -= xMargin * 2;
				serialField.Bounds = bounds;
				serialField.Visible = true;
			}
			else
			{
				serialField.Bounds = originalBounds;
				serialField.Visible = false;
			}

			instantText = GetText(GetColumnValueAtRow(source, rowNumber));

			if (instantText != null)
				serialField.Fill(instantText);

			serialField.RightToLeft = this.DataGridTableStyle.DataGrid.RightToLeft;

			if (instantText == null)
				serialField.Fill(String.Empty);

			if (serialField.Visible)
				DataGridTableStyle.DataGrid.Invalidate(originalBounds);

			inEdit = true;
			serialField.Activate();
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight()
		{
			return serialField.Height + yMargin;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g, object objectValue)
		{
			int newLineIndex = 0;
			int newLines = 0;
			string valueString = this.GetText(objectValue);
			do
			{
				newLineIndex = valueString.IndexOf("r\n", newLineIndex + 1);
				newLines += 1;
			}
			while (newLineIndex != -1);

			return FontHeight * newLines + yMargin;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue)
		{
			Size size = Size.Ceiling(g.MeasureString(GetText(objectValue), this.DataGridTableStyle.DataGrid.Font));
			size.Width += xMargin * 2 + DataGridTableGridLineWidth;
			size.Height += yMargin;
			return size;
		}

		//---------------------------------------------------------------------
		protected override void SetDataGridInColumn(DataGrid dataGrid)
		{
			base.SetDataGridInColumn(dataGrid);
			if (serialField.Parent != dataGrid && serialField.Parent != null)
				serialField.Parent.Controls.Remove(serialField);
			if (dataGrid != null)
				dataGrid.Controls.Add(serialField);
		}

		//---------------------------------------------------------------------
		protected override void UpdateUI(CurrencyManager source, int rowNumber, string instantText)
		{
			serialField.Fill(GetText(GetColumnValueAtRow(source, rowNumber)));
		}

		//----------------------------------------------------------------------
		private int DataGridTableGridLineWidth
		{
			get
			{
				return (this.DataGridTableStyle.GridLineStyle == DataGridLineStyle.Solid) ? 1 : 0;
			}
		}

		//---------------------------------------------------------------------
		public void EndEdit()
		{
			try
			{
				inEdit = false;
				Invalidate();
			}
			catch (Exception err)
			{
				Debug.WriteLine("SerialColumnStyle.EndEdit: " + err.Message);
			}
		}

		//---------------------------------------------------------------------
		private string GetText(object objectValue)
		{
			if (objectValue == System.DBNull.Value)
				return NullText;
			if (objectValue != null)
				return objectValue.ToString();
			else
				return string.Empty;
		}

		//---------------------------------------------------------------------
		private void HideSerialField()
		{
			if (serialField.Focused)
				this.DataGridTableStyle.DataGrid.Focus();
			serialField.Visible = false;
		}

		//---------------------------------------------------------------------
		private void RollBack()
		{
			serialField.Fill(oldVal);
		}

		//---------------------------------------------------------------------
		protected string GetColumnSerialAtRow(CurrencyManager source, int rowNumber)
		{
			try
			{
				object myObject = GetColumnValueAtRow(source, rowNumber);
				if (myObject == null) return null;
				return myObject as string;
			}
			catch (Exception)
			{
				return null;
			}
		}

		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber)
		{
			SolidBrush backBrush = new SolidBrush(Color.White);
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			string s = GetColumnSerialAtRow(source, rowNumber);
			g.DrawString(s, new Font("Verdana", 9.75f), Brushes.Black, bounds.X, bounds.Y);
		}

		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber, bool alignToRight)
		{
			SolidBrush backBrush = new SolidBrush(Color.White);
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			string s = GetColumnSerialAtRow(source, rowNumber);
			g.DrawString(s, new Font("Verdana", 9.75f), Brushes.Black, bounds.X, bounds.Y);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source,
			int rowNumber,
			Brush backBrush,
			Brush foreBrush,
			bool alignToRight
			)
		{
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			string s = GetColumnSerialAtRow(source, rowNumber);
			g.DrawString(s, new Font("Verdana", 9.75f), Brushes.Black, bounds.X, bounds.Y);
		}


	}

	//=========================================================================
	public class ExceptionEventArgs : EventArgs
	{
		private Exception exceptionRaised;

		//---------------------------------------------------------------------
		public Exception ExceptionRaised
		{
			get { return exceptionRaised; }
		}

		//---------------------------------------------------------------------
		public ExceptionEventArgs(Exception e)
		{
			exceptionRaised = e;
		}
	}

}
