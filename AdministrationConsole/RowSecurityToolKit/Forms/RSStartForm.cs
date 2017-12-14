using System.Data.SqlClient;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	///<summary>
	/// Start Form per accedere alle entita' e relativa gestione
	///</summary>
	//================================================================================
	public partial class RSStartForm : PlugInsForm
	{
		private DatabaseDiagnostic dbDiagnostic = new DatabaseDiagnostic();

		private PathFinder pathFinder;
		private BrandLoader brandLoader;
		private ContextInfo context;
		private LicenceInfo licenceInfo;
		private ImageList stateImageList;
		private ImageList imageList;

		// Events
		//--------------------------------------------------------------------------------
		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;

		public delegate DatabaseStatus CheckCompanyDBForRSLDelegate(string companyId);
		public event CheckCompanyDBForRSLDelegate CheckCompanyDBForRSL;

		///<summary>
		/// Costruttore
		///</summary>
		//--------------------------------------------------------------------------------
		public RSStartForm
			(
			PathFinder pathFinder,
			BrandLoader brandLoader,
			ContextInfo context,
			LicenceInfo licenceInfo,
			ImageList stateImageList,
			ImageList imageList
			)
		{
			InitializeComponent();

			this.pathFinder = pathFinder;
			this.brandLoader = brandLoader;
			this.context = context;
			this.licenceInfo = licenceInfo;
			this.stateImageList = stateImageList;
			this.imageList = imageList;
		}
		
		///<summary>
		/// Apertura wizard gestione entita'
		///</summary>
		//--------------------------------------------------------------------------------
		private void EntitiesLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			EntityManager em = new EntityManager(context, brandLoader, stateImageList);
			em.OnGetCompanies += new EntityManager.GetCompanies(em_OnGetCompanies);
			em.CheckCompanyDBForRSL += new EntityManager.CheckCompanyDBForRSLDelegate(em_CheckCompanyDBForRSL);
			em.RunWizard();
		}

		//--------------------------------------------------------------------------------
		public DatabaseStatus em_CheckCompanyDBForRSL(string companyId)
		{
			DatabaseStatus dbStatus = DatabaseStatus.EMPTY;

			if (CheckCompanyDBForRSL != null)
				dbStatus = CheckCompanyDBForRSL(companyId);

			return dbStatus;
		}

		//--------------------------------------------------------------------------------
		public SqlDataReader em_OnGetCompanies()
		{
			if (OnGetCompanies != null)
				return OnGetCompanies();

			return null;
		}

		///<summary>
		/// Apertura form visualizzatore entita'
		///</summary>
		//--------------------------------------------------------------------------------
		private void EntitiesOverviewLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			EntitiesOverview overview = new EntitiesOverview(context, brandLoader, imageList);
			overview.ShowDialog(this);
		}

		///<summary>
		/// Apertura form per la criptazione dei files
		///</summary>
		//--------------------------------------------------------------------------------
		private void CryptLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CrsGenerator crsGen = new CrsGenerator();
			crsGen.ShowDialog(this);
		}
	}
}
