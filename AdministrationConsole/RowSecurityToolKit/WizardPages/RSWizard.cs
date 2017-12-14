using System;
using System.Data.SqlClient;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	//================================================================================
	public class RSWizard : WizardManager
	{
		protected ContextInfo contextInfo;
		protected BrandLoader brandLoader;
		public ImageList StateImageList;
		public RSSelections Selections = null;
		public DatabaseDiagnostic EntityDiagnostic;

		// Events
		//--------------------------------------------------------------------------------
		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;

		public delegate DatabaseStatus CheckCompanyDBForRSLDelegate(string companyId);
		public event CheckCompanyDBForRSLDelegate CheckCompanyDBForRSL;

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public RSWizard(ContextInfo context, BrandLoader brand, ImageList stateImageList)
		{
			contextInfo = context;
			brandLoader = brand;
			StateImageList = stateImageList;
			wizardFormIcon = Strings.RSL;

			EntityDiagnostic = new DatabaseDiagnostic();

			// istanzio la classe delle selezioni del wizard
			Selections = new RSSelections(contextInfo, brandLoader);
		}

		///<summary> 
		/// Aggiungo le pagine al contentitore del wizard
		///</summary>
		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle = "Entities management wizard";
			
			AddWizardPage(new ChooseEntityPage());
			AddWizardPage(new ChooseCompanyPage());
			AddWizardPage(new SelectMasterTablePage());
			AddWizardPage(new SelectMasterInfoPage());
			AddWizardPage(new SelectEntityRelationsPage());
			AddWizardPage(new SetEntityPrioritiesPage());
			AddWizardPage(new SummaryPage());
		}

		///<summary> 
		/// Metto a disposizione la chiamata per l'elenco delle aziende alle pagine del wizard
		///</summary>
		//---------------------------------------------------------------------
		public void LoadCompaniesList()
		{
			Selections.CompaniesList.Clear();

			SqlDataReader companiesReader = null;

			if (OnGetCompanies != null)
				companiesReader = OnGetCompanies();

			if (companiesReader == null || companiesReader.IsClosed)
				return;

			try
			{
				while (companiesReader.Read())
				{
					CompanyItem item = new CompanyItem();
					item.CompanyId				= companiesReader["CompanyId"].ToString();
					item.Company				= companiesReader["Company"].ToString();
					item.DbName					= companiesReader["CompanyDBName"].ToString();
					item.DbOwner				= companiesReader["CompanyDBOwner"].ToString();
					item.DbServer				= companiesReader["CompanyDBServer"].ToString();
					item.ProviderId				= companiesReader["ProviderId"].ToString();
					item.Provider				= companiesReader["Provider"].ToString();
					item.DBAuthenticationWindows= bool.Parse(companiesReader["CompanyDBWindowsAuthentication"].ToString());
					item.Disabled				= bool.Parse(companiesReader["Disabled"].ToString());
					item.UseUnicode				= bool.Parse(companiesReader["UseUnicode"].ToString());
					item.IsValid				= bool.Parse(companiesReader["IsValid"].ToString());
					item.DatabaseCulture		= Convert.ToInt32(companiesReader["DatabaseCulture"].ToString());
					item.SupportColumnCollation = bool.Parse(companiesReader["SupportColumnCollation"].ToString());
					Selections.CompaniesList.Add(item);
				}
			}
			catch (SqlException)
			{
			}
			finally
			{
				if (!companiesReader.IsClosed)
				{
					companiesReader.Close();
					companiesReader.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------
		public DatabaseStatus GetCompanyStatus(string companyId)
		{
			DatabaseStatus dbStatus = DatabaseStatus.EMPTY;

			if (CheckCompanyDBForRSL != null)
				dbStatus = CheckCompanyDBForRSL(companyId);

			return dbStatus;
		}
	}
}
