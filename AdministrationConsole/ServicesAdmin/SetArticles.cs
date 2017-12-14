using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microarea.Console.Plugin.ServicesAdmin
{

	//=========================================================================
	/// <summary>
	/// Form per gestire l'assegnazione delle cal agli utenti.
    /// In principio nasce per cal named, quindi quelle che vengono assegnate agli utenti ed ad essi rimangono assegnate anche quando tali utenti non sono connessi.
    /// Si possono selezionare gli utenti NAMED che possono usare i vari moduli che esprimono cal NAMED, fino ad un numero massimo che è quello delle cal espresse da singolo modulo.
    /// Dal maggio 2013 è possibile anche assegnare gli utenti di ENTERPRISE alle funzionalità che esprimono cal (che saranno quindi full e financial per mago, più gli eventuali verticali )
    /// Vengono mostrati gli utenti named ed  i moduli autofunctional. 
    /// L'assegnazione avverte quando si raggiunge il massimo di cal, col bollino rosso, ma permette di selezionare ancora utenti, 
    /// perchè le cal floating non rimangono assegnate agli utenti alla loro disconnessione, ed un utente può anche connettersi più volte allo stesso tempo.
    /// ho utilizzato in tutto e per tutto il codice e il db esistente in modo da minimizzare le modifiche e rendere la userexperience analoga a quella già conosciuta.
    /// Per gestire lo startup e non modificare il lavoro di utentei ai quali non interessa questa problematica, le cal verranno inizialmente autoaassegnate con l'utilizzo, 
    /// saranno poi gli utenti che vogliono modulare l'accesso alle singole funzionalità, al fine di impedire la saturazione delle cal, 
    /// a venire su questa form a selezionare le associazione prescelte.
	/// </summary>
    public partial class SetArticles : PlugInsForm
	{
		#region DataMember 
		private SqlConnection	sqlConnection		= null;
		private DataSet			aDataSet			= null;
		private string			codArticle			= string.Empty;
		private bool			uncheck				= false;
		private int				indexCheck			= -1;
		public  PathFinder		consolePathFinder	= null;
		public	LoginManager	loginManager		= null;
        public bool EntMode = false;//COMPORTAMENTO CONDIZIONATO SE ENT (FLOATING) o PRO\STD (NAMED)
		private bool			visible				= true;
		private bool			suspendEdit	        = true;//quando siamo noi a popolare i check la fomr non deve andare in edit, deve farlo solo se è l'utente che modifica
		private bool			usersReadOnly		= false;//se ci sono utenti connessi la form è disabilitata ma consultabile
        
		private Diagnostic		diagnostic			= new Diagnostic("SetArticols");
		private ServicesAdmin.ConnectionParameters aConnectionParameters;
		#endregion

		#region Events and Delegates
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);

		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;
		#endregion

		#region Constructors
		/// <summary>
		/// SetArticols
		/// </summary>
		//---------------------------------------------------------------------
		public SetArticles
			(
			ServicesAdmin.ConnectionParameters connectionParameters, 
			PathFinder		aconsolePathFinder,
			SqlConnection	aSqlConnection, 
			LoginManager	aLoginManager, 
            bool entMode
			)
		{
            EntMode = entMode;
			consolePathFinder = aconsolePathFinder;
			InitializeComponent();
            LblTitle.Text = EntMode ? Strings.Floating : Strings.Named;
            DescriptionLabel.Text = Strings.SetArticles;//Strings.Descr1 + "\r\n" + Strings.Descr2 + "\r\n" + Strings.Descr3;
			
			loginManager = aLoginManager;
			aConnectionParameters = connectionParameters;
			sqlConnection = aSqlConnection;
            Prepare();
           
		}

		/// <summary>
		/// SetArticols
		/// </summary>
		//---------------------------------------------------------------------
		public SetArticles()
		{
			InitializeComponent();
		}
		#endregion

        //--------------------------------------------------------------------
        private void Prepare()
        { 
            aDataSet = new DataSet();
			this.lstUsers.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.lstUsers_ItemCheck);
			
			LoadAllUsers();
			
			if (LoadAllArticles())
			{
				SetCAL();
				CreateRelationsTable();
				lstArticols.Enabled = true;
				lstUsers.Enabled = true;
				visible = false;
			}
			else
				visible = true;
        }

		# region SelectFirstArticles
		/// <summary>
		/// SelectFirstArticles
		/// </summary>
		//--------------------------------------------------------------------
		private void SelectFirstArticles()
		{
			CustomListViewItem listItemArticles	= (CustomListViewItem)lstArticols.Items[0];

			if (listItemArticles == null)
				return;

			codArticle	= listItemArticles.CodString;
			DataRow[] dr = aDataSet.Tables["Selection"].Select("ArticleName ='" + codArticle + "'");
				
			if (dr != null && dr.Length > 0)
				VisualRelation(codArticle);
		}
		# endregion

		# region UnVisible
		//---------------------------------------------------------------------
		public bool UnVisible()
		{
			return visible;
		}
		# endregion

        

		#region LoadAllUsers
		/// <summary>
		/// LoadAllUsers
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadAllUsers()
		{
            DataTable dtUsers = null;
			//Data Table degli Utenti
			 dtUsers = new DataTable("Users");
			dtUsers.Columns.Add("UserId", Type.GetType("System.Int32"));
            dtUsers.Columns.Add("User", Type.GetType("System.String"));
		
			lstUsers.Clear();
			lstUsers.View				= View.Details;
			lstUsers.AllowColumnReorder	= true;
			lstUsers.CheckBoxes			= true;
            lstUsers.Columns.Add(Strings.Users, -2, HorizontalAlignment.Left);
           
			SqlCommand sqlCommand	= new SqlCommand();
			SqlDataReader myReader	= null;

            try
            {
                string usertype = EntMode ? "ConcurrentAccess = 1 OR " : "";

                sqlCommand.CommandText = String.Format("SELECT Description, LoginId, Login FROM MSD_Logins where Disabled = 0 and (ConcurrentAccess = 1 OR  SmartClientAccess = 1)", usertype);
                sqlCommand.Connection = sqlConnection;
                myReader = sqlCommand.ExecuteReader();
                DataRow dr = null;

                lstUsers.Height = 20;
                while (myReader.Read())
                {
                    CustomListViewItem listItemUser = new CustomListViewItem();
                    listItemUser.Text = myReader["Login"].ToString();
                    listItemUser.Id = Convert.ToInt32(myReader["LoginId"]);
                    listItemUser.ImageIndex = 0;
                    lstUsers.Items.Add(listItemUser);

                    dr = dtUsers.NewRow();
                    dr["UserId"] = Convert.ToInt32(myReader["LoginId"]);
                    dr["User"] = myReader["Login"].ToString();
                    dtUsers.Rows.Add(dr);

                }

                lstUsers.Height =20+( 25 * dtUsers.Rows.Count);
                aDataSet.Tables.Add(dtUsers);
               

            }
            catch (SqlException)
            {
            }

            finally
            {
                if (myReader != null && !myReader.IsClosed) myReader.Close();
                if (sqlCommand != null) sqlCommand.Dispose();
            
            }
		}


		# endregion

		# region LoadAllArticles
		/// <summary>
		/// LoadAllArticles
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoadAllArticles()
		{
			//Data Tabledegli Utenti
			DataTable dtArticles = new DataTable("Articles");
			dtArticles.Columns.Add("ArticleName", Type.GetType("System.String"));
			dtArticles.Columns.Add("ArticleCal", Type.GetType("System.Int32"));

			lstArticols.Clear();
			lstArticols.View				= View.Details;
			lstArticols.AllowColumnReorder	= true;
			lstArticols.Activation		    = ItemActivation.OneClick;
            lstArticols.Columns.Add(Strings.Articles, panel2.Width-10, HorizontalAlignment.Left);
            
			DataRow dr = null;
			ModuleNameInfo[] infos = EntMode? loginManager.GetArticlesWithFloatingCal(): loginManager.GetArticlesWithNamedCal();
			if (infos != null)
			{
				foreach (ModuleNameInfo m in infos)
				{
                    if (m == null) continue;
					CustomListViewItem listItemArticles = new CustomListViewItem();
                    listItemArticles.Text = m.LocalizedName + " (" + m.CAL + ")";
					listItemArticles.CodString	= m.Name;
                    listItemArticles.ImageIndex = -1;
					lstArticols.Items.Add(listItemArticles);

					dr = dtArticles.NewRow();
					dr["ArticleName"]= m.Name;
					dr["ArticleCal"] = m.CAL;//anche se per ent non ha molto senso contarle perchè posso assegnarne di più ed al logoff vengono liberate, non rimangono assegnate come la named
					dtArticles.Rows.Add(dr);	
				}
                //MOBILE*****
                int namedCal = 0, gdiConcurrent = 0, unNamedCal = 0, officeCal = 0, tpCal = 0, wmscal = 0, manufacturingCal = 0;
                namedCal = loginManager.GetCalNumber(out gdiConcurrent, out unNamedCal, out officeCal, out tpCal, out wmscal, out manufacturingCal);
                if ( wmscal > 0)
                {
                    CustomListViewItem listItemArticles = new CustomListViewItem();
                    listItemArticles.Text = "WMS Mobile" + " (" + wmscal + ")";
                    listItemArticles.CodString = WMS;
                    listItemArticles.ImageIndex = -1;
                    lstArticols.Items.Add(listItemArticles);

                    dr = dtArticles.NewRow();
                    dr["ArticleName"] = WMS;
                    dr["ArticleCal"] = wmscal;//anche se per ent non ha molto senso contarle perchè posso assegnarne di più ed al logoff vengono liberate, non rimangono assegnate come la named
                    dtArticles.Rows.Add(dr);	
                }
                if (manufacturingCal > 0 )
                {
                    CustomListViewItem listItemArticles = new CustomListViewItem();
                    listItemArticles.Text = "Manufacturing Mobile" + " (" + manufacturingCal + ")";
                    listItemArticles.CodString = MANUFACTURING;
                    listItemArticles.ImageIndex = -1;
                    lstArticols.Items.Add(listItemArticles);

                    dr = dtArticles.NewRow();
                    dr["ArticleName"] = MANUFACTURING;
                    dr["ArticleCal"] = wmscal;//anche se per ent non ha molto senso contarle perchè posso assegnarne di più ed al logoff vengono liberate, non rimangono assegnate come la named
                    dtArticles.Rows.Add(dr);
                }
			}
			aDataSet.Tables.Add(dtArticles); 
			return true;
		}

       
		# endregion

		# region SetCAL

    

		/// <summary>
		/// SetCAL
		/// </summary>
		//---------------------------------------------------------------------
		private void SetCAL()
		{
            foreach (CustomListViewItem articleItem in lstArticols.Items)
            {
              if (articleItem.ImageIndex != -1 ) articleItem.ImageIndex = 2;
                articleItem.ForeColor = SystemColors.WindowText;
            }

			//Creo la tabella delle associazioni con i totali delle call Usate
			DataTable dtRelations = new DataTable("Relations");
			dtRelations.Columns.Add("ArticleName", Type.GetType("System.String"));
			dtRelations.Columns.Add("UsedCall", Type.GetType("System.Int32"));

			SqlCommand sqlCommand = new SqlCommand();
			SqlDataReader myReader= null;

            try
            {
                sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_LoginsArticles where Article = @ArticolName";
                sqlCommand.Connection = sqlConnection;
                sqlCommand.Parameters.Add("@ArticolName", SqlDbType.NVarChar);
                DataRow dr = null;
                for (int i = 0; i < aDataSet.Tables["Articles"].Rows.Count; i++)
                {
                    sqlCommand.Parameters["@ArticolName"].Value = aDataSet.Tables["Articles"].Rows[i]["ArticleName"];
                    myReader = sqlCommand.ExecuteReader();
                    myReader.Read();
                    dr = dtRelations.NewRow();
                    dr["ArticleName"] = aDataSet.Tables["Articles"].Rows[i]["ArticleName"];
                    dr["UsedCall"] = Convert.ToInt32(myReader[0]);
                    dtRelations.Rows.Add(dr);
                    if (Convert.ToInt32(myReader[0]) == Convert.ToInt32(aDataSet.Tables["Articles"].Rows[i]["ArticleCal"].ToString()) ||
                        Convert.ToInt32(myReader[0]) > Convert.ToInt32(aDataSet.Tables["Articles"].Rows[i]["ArticleCal"].ToString()))
                    {
                        //Setto l'elemento della lista di colore rosso
                        foreach (CustomListViewItem articleItem in lstArticols.Items)
                        {
                            if (articleItem.CodString == aDataSet.Tables["Articles"].Rows[i]["ArticleName"].ToString())
                            {
                                articleItem.ForeColor = Color.Red;
                                articleItem.ImageIndex = 1;

                            }
                            else if (articleItem.ImageIndex == -1) articleItem.ImageIndex = 2;
                        }
                    }

                    myReader.Close();
                }

                if (aDataSet.Tables.Count < 4)
                    aDataSet.Tables.Add(dtRelations);

                if (!myReader.IsClosed) myReader.Close();

                //quelli rimasti senza simbolo li metto liberi
                foreach (CustomListViewItem articleItem in lstArticols.Items)
                {
                    if (articleItem.ImageIndex == -1) articleItem.ImageIndex = 2;
                    articleItem.ForeColor = SystemColors.WindowText;
                }

            }
            catch (SqlException)
            {
              
            }
            finally
            {
                if (myReader != null && !myReader.IsClosed) myReader.Close();
                if (sqlCommand != null) sqlCommand.Dispose();
            }
		}
		# endregion

		# region CreateRelationsTable
		//---------------------------------------------------------------------
		private void CreateRelationsTable()
		{
			DataTable dtSelection = new DataTable("Selection");
			dtSelection.Columns.Add("ArticleName", Type.GetType("System.String"));
			dtSelection.Columns.Add("User", Type.GetType("System.Int32"));

			//Popolo la tabel con i dati del DB
			SqlCommand sqlCommand	= new SqlCommand();
			SqlDataReader myReader	= null;

            try
            {
                sqlCommand.CommandText = "SELECT * FROM MSD_LoginsArticles";
                sqlCommand.Connection = sqlConnection;
                myReader = sqlCommand.ExecuteReader();
                DataRow dr = null;
                while (myReader.Read())
                {
                    dr = dtSelection.NewRow();
                    dr["ArticleName"] = myReader["Article"].ToString();
                    dr["User"] = Convert.ToInt32(myReader["LoginId"]);
                    dtSelection.Rows.Add(dr);
                }
                aDataSet.Tables.Add(dtSelection);
                if (!myReader.IsClosed) myReader.Close();
              
            }
            catch (SqlException)
            {
              
            }
            finally
            {
                if (myReader != null && !myReader.IsClosed) myReader.Close();
                if (sqlCommand != null) sqlCommand.Dispose();
            }
		}
		# endregion

		# region Main
		//---------------------------------------------------------------------
		[STAThread]
		static void Main() 
		{
			Application.Run(new SetArticles());
		}
		# endregion

		# region Controls events
		/// <summary>
		/// Load della form
		/// </summary>
		//---------------------------------------------------------------------
		private void SetArticols_Load(object sender, System.EventArgs e)
		{
			if (lstArticols.Items != null && lstArticols.Items.Count > 0 )
			{
				SelectFirstArticles();
                lstArticols.Items[0].Selected = true;
			}

			this.lstUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstUsers_ItemCheck);	

			if (loginManager.GetLoggedUsersNumber() > 0)
		        Disable();
//altrimenti lock in modo che non sia usabile mago mentre sono in editing di questa pagina
            else 
				SetUpdatingFlagOnCompanies(true/*, companiesReader*/);
           
            suspendEdit = false;
		}

        //---------------------------------------------------------------------
        private void Disable()
        {
            lstUsers.Enabled = false;
            LblConnectedUsers.Visible = true;
            pictureBox1.Visible = true;
            BtnClearAll.Enabled = false;
            usersReadOnly = true;

        }

		/// <summary>
		/// intercetto l'evento di ParentChanged
		/// se il Parent è null significa che ho pulito la working area e vado ad
		/// aggiornare il flag Updating sulla tabella MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------
		private void SetArticols_ParentChanged(object sender, System.EventArgs e)
		{
			// se il parent è null significa che ho pulito la workingarea
			// ricarico l'elenco delle companies e setto il flag updating a false
			if (this.Parent == null)
				SetUpdatingFlagOnCompanies(false/*, companiesReader*/);
		}

       
		//---------------------------------------------------------------------
		private void lstArticols_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            suspendEdit = true;
			//Devo settare gli incroci già presenti
			//così quando cambio articolo vedo i check di prima 
			ListView myListArticles = (ListView)sender;

			if (usersReadOnly) // è stato impostato dall'esterno e quindi prevale sugli altri casi
				this.lstUsers.Enabled = false;
			else
			{
				if (myListArticles.SelectedItems != null && myListArticles.SelectedItems.Count != 0)
					this.lstUsers.Enabled = true;
			}

            CustomListViewItem listItemArticles = (CustomListViewItem)myListArticles.FocusedItem; 
			if (listItemArticles == null)
				return;

            foreach (CustomListViewItem i in myListArticles.Items)
            {
                i.BackColor = myListArticles.BackColor;
                if (i.ImageIndex == 1) i.ForeColor = Color.Red;
                else i.ForeColor = System.Drawing.SystemColors.ControlText; 
            }
            listItemArticles.BackColor = System.Drawing.SystemColors.Highlight;//così mantiene selezionato anche quando perde il fuoco che se non non si capisce!
            listItemArticles.ForeColor = System.Drawing.SystemColors.HighlightText;
			ShowLoginArticle(listItemArticles);
            suspendEdit = false;
		}

		//---------------------------------------------------------------------
        private void lstUsers_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            //se utenti connessi disabilito la pagina.
            if (loginManager.GetLoggedUsersNumber() > 0)
            {
                Disable();
                return;
            }

            if (State != StateEnums.Editing && !suspendEdit)
                State = StateEnums.Editing;

            uncheck = false;
            ListView listView = (ListView)sender;
            try
            {
                CustomListViewItem it = (CustomListViewItem)lstUsers.Items[e.Index];
                if (loginManager.IsUserLogged(it.Id))
                {
                    DiagnosticViewer.ShowWarning(Strings.WarnExistLoggedUsers, Strings.Error);
                    e.NewValue = e.CurrentValue;
                    return;
                }

                if (codArticle.Length == 0)
                {
                    DiagnosticViewer.ShowWarning(Strings.NoArticolSel, Strings.Error);
                    e.NewValue = CheckState.Unchecked;
                    return;
                }

                DataRow[] dataRow = aDataSet.Tables["Articles"].Select("ArticleName = '" + codArticle + "'");
                int maxCal = Convert.ToInt32(dataRow[0]["ArticleCal"].ToString());
                dataRow = aDataSet.Tables["Relations"].Select("ArticleName = '" + codArticle + "'");
                int UserdCal = 0;
                if (dataRow.Length != 0)
                    UserdCal = Convert.ToInt32(dataRow[0]["UsedCall"].ToString());
                int totoCheck = 0;

                foreach (ListViewItem item in this.lstUsers.Items)
                    if (item.Checked)
                        totoCheck += 1;
                int tot = totoCheck;
                if (maxCal == totoCheck && e.NewValue != CheckState.Unchecked && !EntMode && !IsMobile(codArticle))
                {
                    e.NewValue = CheckState.Unchecked;
                    lstUsers.Refresh();
                    uncheck = true;
                    indexCheck = e.Index;

                }
                if (e.NewValue == CheckState.Unchecked && e.CurrentValue != CheckState.Unchecked) tot--;

                else if (e.NewValue != CheckState.Unchecked && e.CurrentValue == CheckState.Unchecked) tot++;

                if (maxCal <= tot)
                {
                    if (lstArticols.SelectedItems != null && lstArticols.SelectedItems.Count > 0)
                    {
                        lstArticols.SelectedItems[0].ImageIndex = 1;
                        lstArticols.SelectedItems[0].ForeColor = Color.Red;
                    }
                    else if (lstArticols.FocusedItem != null)//potrebbe essere focused ma non selected.
                    {
                        lstArticols.FocusedItem.ImageIndex = 1;
                        //lstArticols.FocusedItem.ForeColor = Color.Red;//non imposto rosso, perchè sta male con lo sfondo selezionato
                    }
                }

                else
                    if (lstArticols.SelectedItems != null && lstArticols.SelectedItems.Count > 0)
                    {
                        lstArticols.SelectedItems[0].ImageIndex = 2;
                        lstArticols.SelectedItems[0].ForeColor = SystemColors.WindowText;
                    }
                    else if (lstArticols.FocusedItem != null)//potrebbe essere focused ma non selected.
                    {
                        lstArticols.FocusedItem.ImageIndex = 2;
                        lstArticols.FocusedItem.ForeColor = SystemColors.WindowText;
                    }
            }
            catch (Exception err)
            {
                string a = err.Message;

            }
        }

		//---------------------------------------------------------------------
		private void lstUsers_DoubleClick(object sender, System.EventArgs e)
		{
			//se utenti connessi disabilito la pagina.
			if (loginManager.GetLoggedUsersNumber() > 0)
			{
                Disable();
				return;
			}

			if (uncheck)
			{
				ListView myList = (ListView) sender;
				myList.Items[indexCheck].Checked = false;
			}
		}
		# endregion

		# region SetUpdatingFlagOnCompanies
		/// <summary>
		/// SetUpdatingFlagOnCompanies
		/// imposto il flag Updating per tutte le aziende nella tabella MSD_Companies
		/// (true o false a seconda del parametro)
		/// </summary>
		//---------------------------------------------------------------------
		public void SetUpdatingFlagOnCompanies(bool updatingSetting/*, SqlDataReader companiesReader*/)
		{
            
            SqlDataReader companiesReader = null;
            if (OnGetCompanies != null)
                companiesReader = OnGetCompanies();
            
            if (companiesReader == null || companiesReader.IsClosed)
				return;

			SqlCommand myCommand = new SqlCommand();
			myCommand.CommandText = "UPDATE MSD_Companies SET Updating = @updatingSet WHERE CompanyId = @id";
			myCommand.Connection = this.sqlConnection;
			
			myCommand.Parameters.Add("@updatingSet", SqlDbType.Bit);
			myCommand.Parameters.Add("@id", SqlDbType.Int);
            try
            {
                while (companiesReader.Read())
                {
                    myCommand.Parameters["@updatingSet"].Value = updatingSetting;
                    myCommand.Parameters["@id"].Value = companiesReader["CompanyId"].ToString();
                    myCommand.ExecuteNonQuery();
                }
              
            }
            catch (SqlException e)
            {
                diagnostic.SetError(e.Message);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
            }
            finally
            { 
                if (!companiesReader.IsClosed)
                    companiesReader.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }
			
		}
		# endregion

		# region DeleteTable
		/// <summary>
		/// DeleteTable
		/// </summary>
		//---------------------------------------------------------------------
		private void DeleteTable()
		{
			SqlCommand sqlCommand = new SqlCommand();

            try
            {
                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles";
                sqlCommand.Connection = sqlConnection;
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
            catch (SqlException)
            {
                if (sqlCommand != null) sqlCommand.Dispose();
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
            }
		}
		# endregion

		# region ShowLoginArticle
		/// <summary>
		/// ShowLoginArticle
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowLoginArticle(CustomListViewItem listItemArticles)
		{
			if (this.codArticle.Length == 0)
			{
				codArticle = listItemArticles.CodString;

				//Deseleziono gli eventuali vecchi selezionati
				foreach (ListViewItem listViewItem in lstUsers.Items)
					listViewItem.Checked = false;

				VisualRelation(codArticle);

				if (usersReadOnly)	// è stato impostato dall'esterno e quindi prevale sugli altri casi
					this.lstUsers.Enabled = false;
				else
					if (listItemArticles.ForeColor != Color.Red && !EntMode && !IsMobile(codArticle))
						this.lstUsers.Enabled = true;

				return;
			}

			if (string.Compare(codArticle, listItemArticles.CodString, true, CultureInfo.InvariantCulture) == 0)
			{
				//Vuol dire che  è uguale quindi sto passando su quello già selezionato e 
				//devo salvare le selezioni in memoria
				SaveRelation(codArticle);
				return;
			}
			else
			{
                SaveRelation(codArticle);
				codArticle = listItemArticles.CodString;
				//Vuol dire che sono su quello nuovo quindi devo prima deselezionare le vecchie selezioni
				if (usersReadOnly)	// è stato impostato dall'esterno e quindi prevale sugli altri casi
					this.lstUsers.Enabled = false;
				else
                    if (listItemArticles.ForeColor != Color.Red && !EntMode && !IsMobile(codArticle))
						this.lstUsers.Enabled = true;

				foreach (ListViewItem listViewItem in lstUsers.Items)
					listViewItem.Checked = false;

				VisualRelation(codArticle);
			}
		}
		# endregion

		# region SaveRelation
		/// <summary>
		/// SaveRelation
		/// </summary>
		//---------------------------------------------------------------------
		public void SaveRelation(string codArticleToSave)
		{
			DataRow[] dr = null;
			string cod = string.Empty;
			if (codArticleToSave.Length == 0)
				cod = codArticle;
			else
				cod = codArticleToSave;

			dr = aDataSet.Tables["Selection"].Select("ArticleName ='" + cod + "'");

			if (dr.Length == 0)
			{
				foreach(ListViewItem listViewItem in lstUsers.Items)
				{
					if (listViewItem.Checked == false) 
						continue;
					CustomListViewItem customListViewItem = (CustomListViewItem)listViewItem;
					DataRow dr1 =aDataSet.Tables["Selection"].NewRow();
					dr1["ArticleName"]= cod;
					dr1["User"]= Convert.ToInt32(customListViewItem.Id);
					aDataSet.Tables["Selection"].Rows.Add(dr1);
				}
			}
			else
			{
				foreach (DataRow dataRow in aDataSet.Tables["Selection"].Select("ArticleName ='" + cod + "'"))
					dataRow.Delete();

				foreach(ListViewItem listViewItem in lstUsers.Items)
				{
					if (listViewItem.Checked == false) 
						continue;
					CustomListViewItem customListViewItem = (CustomListViewItem)listViewItem;
					DataRow dr1 =aDataSet.Tables["Selection"].NewRow();
					dr1["ArticleName"]= cod;
					dr1["User"]= Convert.ToInt32(customListViewItem.Id);
					aDataSet.Tables["Selection"].Rows.Add(dr1);
				}
			}
		}
		# endregion

		# region SaveArticles
		/// <summary>
		/// SaveArticles
		/// </summary>
		//---------------------------------------------------------------------
		public void SaveArticles()
		{
			SaveRelation(codArticle);

			DeleteTable();

			SqlCommand sqlCommand = new SqlCommand();

			try
            {
                sqlCommand.CommandText = "INSERT INTO MSD_LoginsArticles (LoginId, Article) VALUES (@LoginId, @Article)";
                sqlCommand.Connection = sqlConnection;
                sqlCommand.Parameters.Add("@LoginId", SqlDbType.Int);
                sqlCommand.Parameters.Add("@Article", SqlDbType.NVarChar);
                bool floatingmark = false;

                bool onlymobile = true;
                foreach (DataRow relationDataRow in aDataSet.Tables["Selection"].Rows)
                {
                    int  uid = Convert.ToInt32(relationDataRow["User"]);
                    if (!floatingmark && uid == NameSolverStrings.FloatingMarkNumber)floatingmark = true;
                    sqlCommand.Parameters["@LoginId"].Value = uid;
                    sqlCommand.Parameters["@Article"].Value = relationDataRow["ArticleName"].ToString();
                    sqlCommand.ExecuteNonQuery();
                    onlymobile &= IsMobile(relationDataRow["ArticleName"]);
                }

                //tolgo floating mark se non ci sono assegnazioni
                if (aDataSet.Tables["Selection"].Rows.Count == 0 || onlymobile)

                    DeleteFloatingMark();

                //AddFloatingMark se non è già stato messo(per capire se ho gestito le associazioni o meno, se no non posso capirlo, se non c'è il mark allora permetto tutto come era prima)
                else if (EntMode && !floatingmark)
                {
                    sqlCommand.Parameters["@Article"].Value = NameSolverStrings.FloatingMarkString;
                    sqlCommand.Parameters["@LoginId"].Value = NameSolverStrings.FloatingMarkNumber;
                    int t = sqlCommand.ExecuteNonQuery();
                }
                loginManager.RefreshFloatingMark();


            }
			catch (SqlException)
			{	
			}
			finally
			{
                if (sqlCommand != null) sqlCommand.Dispose();
				State = StateEnums.None;
				SetCAL();
			}
		}
		# endregion

        //---------------------------------------------------------------------
        private bool IsMobile(object articlename)
        {
            string name = articlename as string;
            if (string.IsNullOrWhiteSpace(name)) return false;

            return (name == WMS || name == MANUFACTURING);
        }

        private const string WMS = "WMS";
        private const string MANUFACTURING = "MANUFACTURING";

		# region VisualRelation
		/// <summary>
		/// VisualRelation
		/// </summary>
		//---------------------------------------------------------------------
		private void VisualRelation(string codArticle)
		{
			//Estraggo il numero massimo di cal
			DataRow[] dataRowMax = aDataSet.Tables["Articles"].Select("ArticleName = '" + codArticle + "'");
			int maxCal = Convert.ToInt32(dataRowMax[0]["ArticleCal"].ToString());
			int selected = 0;

			//Check provenienti dalle selezioni
			DataRow[] dr = aDataSet.Tables["Selection"].Select("ArticleName ='" + codArticle + "'");
			if (dr.Length != 0)
			{
				foreach (DataRow dataRow in dr)
				{
					foreach (ListViewItem listViewElement in lstUsers.Items)
					{

                        if (selected == maxCal && !EntMode && !IsMobile(dataRow["ArticleName"]))// ent può selezionarne di più tanto non sono assegnate
							return;

						CustomListViewItem customListViewItem = (CustomListViewItem)listViewElement;
						if (customListViewItem.Id == Convert.ToInt32(dataRow["User"]))
						{
							customListViewItem.Checked = true;
							selected += 1;
						}
					}
				}
			}
		}
		# endregion

		# region DeleteTable
		/// <summary>
		/// DeleteTable
		/// </summary>
		//---------------------------------------------------------------------
		static public void DeleteUserProducts(int loginId, SqlConnection sqlConnection)
		{
			SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles WHERE LoginId = @LoginId";
                sqlCommand.Parameters.AddWithValue("@LoginId", loginId);
                sqlCommand.Connection = sqlConnection;
                sqlCommand.ExecuteNonQuery();
                
            }
            catch (SqlException)
            {
                
            }
            finally
            {
                if (sqlCommand != null) sqlCommand.Dispose();
            }
		}

        /// <summary>
        /// DeleteTable
        /// </summary>
        //---------------------------------------------------------------------
        private void DeleteFloatingMark()
        {
            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles WHERE LoginId = @LoginId";
                sqlCommand.Parameters.Add("@LoginId", SqlDbType.Int);
                sqlCommand.Parameters["@LoginId"].Value = NameSolverStrings.FloatingMarkNumber;
                sqlCommand.Connection = sqlConnection;
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException)
            {
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
            }
        }

		# endregion

		//---------------------------------------------------------------------
		public void SetReadOnlyUsersListView(bool isReadOnly)
		{
			lstUsers.Enabled = BtnClearAll.Enabled= !isReadOnly;
			usersReadOnly = isReadOnly;
		}

        //---------------------------------------------------------------------
        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(Strings.ClearAllAssociation, Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (res != System.Windows.Forms.DialogResult.Yes) return;

            DeleteTable();
            Prepare();
            this.lstUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstUsers_ItemCheck);

            loginManager.RefreshFloatingMark();//in caso di named fa la init degli slot per cancellare gli slot assegnati ( quell oche viene fatto al salva, che per il cleal all non è chiamato)
        }

        //---------------------------------------------------------------------
        private void lstUsers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lstUsers.Sorting == System.Windows.Forms.SortOrder.Descending)
                lstUsers.Sorting = System.Windows.Forms.SortOrder.Ascending;
            else
                lstUsers.Sorting = System.Windows.Forms.SortOrder.Descending;
            lstUsers.Sort();
        } 

	}

	# region CustomListViewItem
	//=========================================================================
	public class CustomListViewItem : ListViewItem
	{
		private int		elementId = -1;
		private string	codString = ""; 
		private string	loginString = ""; 

		//---------------------------------------------------------------------
		public int Id { get { return elementId; } set { elementId = value; } }
		//---------------------------------------------------------------------
		public string CodString { get { return codString; } set { codString = value; } }
		//---------------------------------------------------------------------
		public string LoginString { get { return loginString; } set { loginString = value; } }
		
		//---------------------------------------------------------------------
		public CustomListViewItem() 
		{}
	}
	# endregion

}