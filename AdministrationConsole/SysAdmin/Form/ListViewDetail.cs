using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// ListView visualizzata nel panel di destra dell'Administration Console
	/// </summary>
	//=========================================================================
	public partial class ListViewDetail : PlugInsForm
	{
		#region Variables
		private SqlConnection	currentConnection;
		private View			typeOfView;
		private string			connectionString;
		private string			objectId;
		private string			roleId;
		private string			parentId;
		private bool			isStandardEdition = false;
		private Diagnostic		diagnostic = new Diagnostic("SysAdminPlugIn.ListViewDetail");
		#endregion

		#region Events and delegates
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;

		// Eventi da inviati al SysAdmin per le aperture delle form corrispondenti agli item della ListView
		public delegate void ModifyProvider(object sender, string id);
		public event ModifyProvider OnModifyProvider;

		public delegate void ModifyUser(object sender, string id);
		public event ModifyUser OnModifyUser;

		public delegate void ModifyCompanyUser(object sender, string id, string companyId);
		public event ModifyCompanyUser OnModifyCompanyUser;

		public delegate void ModifyContainerCompanyUser(object sender, CompanyUser cUser);
		public event ModifyContainerCompanyUser OnModifyContainerCompanyUser;

		public delegate void ModifyRole(object sender, string id, string companyId);
		public event ModifyRole OnModifyRole;

		public delegate void ModifyCompanyUserRole(object sender, string id, string companyId, string roleId);
		public event ModifyCompanyUserRole OnModifyCompanyUserRole;

		public delegate void ModifyCompany(object sender, string id);
		public event ModifyCompany OnModifyCompany;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		#endregion
		
		#region Properties
		public string			ObjectId			{ set { objectId = value;} }
		public string			RoleId				{ set { roleId   = value;} }
		public string			ParentId			{ set { parentId = value;} }
		public string			ConnectionString	{ get { return connectionString; } set { connectionString  = value; } }
        public SqlConnection	CurrentConnection	{ get { return currentConnection;} set { currentConnection = value;} }
		public View				TypeOfView 			{ get { return typeOfView;       } set { typeOfView		   = value;} }
		#endregion
		
		#region Constructor
		/// <summary>
		/// ListViewDetail
		/// costruisce e carica i dati a seconda del tipo di oggetto
		/// </summary>
		//---------------------------------------------------------------------
		public ListViewDetail(View typeOfView, ImageList treeViewImages, bool isStandardEdition)
		{
			InitializeComponent();
			
			this.TabIndex					= 3;
			this.TabStop					= true;
			this.typeOfView					= typeOfView;
			
			listViewData.TabIndex			= 3;
			listViewData.TabStop			= false;
			listViewData.View				= typeOfView;
			listViewData.Dock				= DockStyle.Fill;
			listViewData.AllowColumnReorder = true;
            listViewData.Sorting			= System.Windows.Forms.SortOrder.Ascending;
			listViewData.LargeImageList		= listViewData.SmallImageList = treeViewImages;

			this.isStandardEdition			= isStandardEdition;
		}
		#endregion
		
		#region SettingListView - A seconda dell'oggetto selezionato, costruisce l'Header della lista e chiama la funzione opportuna
		/// <summary>
		/// SettingListView
		/// </summary>
		//---------------------------------------------------------------------
		public void SettingListView(string objectOfList, ImageList treeImage)
		{
			switch (objectOfList)
			{
				case ConstString.containerUsers:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.MaxTimePassword, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.PreferredLanguage, -2, HorizontalAlignment.Left); //Lingua
						listViewData.Columns.Add(Strings.RegionalSettings, -2, HorizontalAlignment.Left); //Impostazioni regionali
						listViewData.Columns.Add(Strings.CAL, -2, HorizontalAlignment.Left); // accedi attraverso
						ListUsers();
						break;
					}

				case ConstString.containerProviders:
					{
						listViewData.Columns.Add(Strings.Provider, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.StripTrailingSpaces, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.UseConstParameter, -2, HorizontalAlignment.Left);
						ListProviders();
						break;
					}

				case ConstString.containerCompanyRoles:
					{
						listViewData.Columns.Add(Strings.Role, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						ListRolesCompany();
						break;
					}

				case ConstString.containerCompanyUsers:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Admin, -2, HorizontalAlignment.Left);
						if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
							listViewData.Columns.Add(Strings.EBDeveloper, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.DBUser, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Disable, -2, HorizontalAlignment.Left);
						ListUsersCompany();
						break;
					}
				
				case ConstString.containerCompanyUsersRoles:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Admin, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.DBUser, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Disable, -2, HorizontalAlignment.Left);
						ListUsersRolesCompany();
						break;
					}

				case ConstString.containerCompanies:
					{
						listViewData.Columns.Add(Strings.Company, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.ServerName, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.DataBaseName, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.PreferredLanguage, -2, HorizontalAlignment.Left); 
						listViewData.Columns.Add(Strings.RegionalSettings, -2, HorizontalAlignment.Left);
						if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
							listViewData.Columns.Add(Strings.CompanyUseEasyAttachment, -2, HorizontalAlignment.Left);
						ListCompanies();
						break;
					}

				case ConstString.itemRole:
					{
						listViewData.Columns.Add(Strings.Role, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						ListRole();
						break;
					}

				case ConstString.itemCompany:
					{
						listViewData.Columns.Add(Strings.Company, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.ServerName, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.DataBaseName, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.PreferredLanguage, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.RegionalSettings, -2, HorizontalAlignment.Left);
						if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
							listViewData.Columns.Add(Strings.CompanyUseEasyAttachment, -2, HorizontalAlignment.Left);
						ListCompany();
						break;
					}

				case ConstString.itemCompanyUser:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Admin, -2, HorizontalAlignment.Left);
						if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
							listViewData.Columns.Add(Strings.EBDeveloper, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.DBUser, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Disable, -2, HorizontalAlignment.Left);
						ListUserCompany();
						break;
					}

				case ConstString.itemRoleCompanyUser:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						ListUsersRolesCompany();
						break;
					}

				case ConstString.itemUser:
					{
						listViewData.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.MaxTimePassword, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.PreferredLanguage, -2, HorizontalAlignment.Left); //Lingua
						listViewData.Columns.Add(Strings.RegionalSettings, -2, HorizontalAlignment.Left); //Impostazioni regionali
						listViewData.Columns.Add(Strings.CAL, -2, HorizontalAlignment.Left); // accedi attraverso
						ListUser();
						break;
					}

				case ConstString.itemProvider:
					{
						listViewData.Columns.Add(Strings.Provider, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.StripTrailingSpaces, -2, HorizontalAlignment.Left);
						listViewData.Columns.Add(Strings.UseConstParameter, -2, HorizontalAlignment.Left);
						ListProvider();
						break;
					}
			}
		}
		#endregion

		#region Funzioni che costruiscono la lista di oggetti

		#region ListCompanies - Visualizza tutte le aziende
		/// <summary>
		/// ListCompanies - visualizza tutte le companies
		/// </summary>
		//---------------------------------------------------------------------
		private void ListCompanies()
		{
			listViewData.Items.Clear();
			ArrayList companies				= new ArrayList();
			CompanyDb companyDb				= new CompanyDb();
			companyDb.CurrentSqlConnection	= currentConnection;
			companyDb.ConnectionString		= connectionString;
			
			if (!companyDb.SelectAllCompanies(out companies))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
					diagnostic.Set(companyDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				companies.Clear();
			}

			// se sono nella Standard Edition considero solo le prime 2 companies caricate,
			// altrimenti considero tutte quelle presenti l'array delle companies
			int companiesCounter = (isStandardEdition && companies.Count > 2) ? 2 : companies.Count;

			for (int i = 0; i < companiesCounter; i++)
			{
				CompanyItem companyItem = (CompanyItem)companies[i];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(companyItem, ConstString.containerCompanies, companyItem.CompanyId);
                item.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				item.Text = companyItem.Company;
				item.SubItems.Add(companyItem.DbServer);
				item.SubItems.Add(companyItem.DbName);
				item.SubItems.Add(companyItem.PreferredLanguage);
				item.SubItems.Add(companyItem.ApplicationLanguage);
				if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
					item.SubItems.Add(companyItem.UseDBSlave ? Strings.Yes : Strings.No);
				if (companyItem.Disabled || !companyItem.IsValid)
					item.ForeColor = Color.Red;
				listViewData.Items.Add(item);
			}
		}
		#endregion

		#region ListUsers - Visualizza tutti gli utenti applicativi
		/// <summary>
		/// ListUsers - visualizza tutti gli utenti
		/// </summary>
		//---------------------------------------------------------------------
		private void ListUsers()
		{
			listViewData.Items.Clear();
			ArrayList users				= new ArrayList();
			UserDb userDb				= new UserDb();
			userDb.ConnectionString		= connectionString;
			userDb.CurrentSqlConnection = currentConnection;
			
			if (!userDb.SelectAllUsers(out users, true))
			{
				if (userDb.Diagnostic.Error || userDb.Diagnostic.Information || userDb.Diagnostic.Warning)
					diagnostic.Set(userDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UsersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
			else
			{
				foreach (UserItem userItem in users)
				{
					ListViewItem item = new ListViewItem();
					item.Tag = new ItemType(userItem, ConstString.containerUsers, userItem.LoginId);
					
					item.ImageIndex = (userItem.WindowsAuthentication) 
						? PlugInTreeNode.GetLoginsDefaultImageIndex 
						: PlugInTreeNode.GetUserDefaultImageIndex;

					item.Text = userItem.Login;
					item.SubItems.Add(userItem.Description.Replace("\r\n"," "));
					
					item.SubItems.Add((userItem.PasswordNeverExpired) 
						? Strings.PasswordNeverExpire 
						: Convert.ToDateTime(userItem.ExpiredDatePassword).Date.ToShortDateString());

					item.SubItems.Add(userItem.PreferredLanguage);
					item.SubItems.Add(userItem.ApplicationLanguage);

					if (userItem.ConcurrentAccess)
						item.SubItems.Add(Strings.CALFloating);
					if (userItem.SmartClientAccess)
						item.SubItems.Add(Strings.CALNamed);
					if (userItem.WebAccess)
						item.SubItems.Add(Strings.CALEasyLook);

					if (userItem.Disabled)
						item.ForeColor = Color.Red;
					if (userItem.Locked)
						item.ForeColor = Color.Gray;
					
					listViewData.Items.Add(item);
				}
			}
		}
		#endregion

		#region ListProviders - Visualizza tutti i Providers
		/// <summary>
		/// ListProviders
		/// </summary>
		//---------------------------------------------------------------------
		private void ListProviders()
		{
			listViewData.Items.Clear();
			ArrayList providers				 = new ArrayList();
			ProviderDb providerDb			 = new ProviderDb();
			providerDb.ConnectionString		 = connectionString;
			providerDb.CurrentSqlConnection	 = currentConnection;

			if (!providerDb.SelectAllProviders(out providers))
			{
				if (providerDb.Diagnostic.Error || providerDb.Diagnostic.Warning || providerDb.Diagnostic.Information)
                    diagnostic.Set(providerDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ProvidersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				providers.Clear();
			}

			for (int i = 0; i < providers.Count ; i++)
			{
				ProviderItem providerItem = (ProviderItem) providers[i];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(providerItem, ConstString.containerProviders, providerItem.ProviderId);
                item.ImageIndex = PlugInTreeNode.GetSqlServerGroupDefaultImageIndex;
				item.Text = providerItem.Description.Replace("\r\n", " ");
				item.SubItems.Add((providerItem.StripTrailingSpaces) ? Strings.Yes : Strings.No);
				item.SubItems.Add((providerItem.UseConstParameter) ? Strings.Yes : Strings.No);
				listViewData.Items.Add(item);
			}
		}
		#endregion

		#region ListRoles - Visualizza tutti i Ruoli di una Azienda
		/// <summary>
		/// ListRoles
		/// </summary>
		//---------------------------------------------------------------------
		private void ListRolesCompany()
		{
			listViewData.Items.Clear();
			ArrayList roles				= new ArrayList();
			RoleDb roleDb				= new RoleDb();
			roleDb.ConnectionString		= connectionString;
			roleDb.CurrentSqlConnection = currentConnection;
			if (!roleDb.SelectAllRolesOfCompany(out roles, this.parentId))
			{
				if (roleDb.Diagnostic.Error || roleDb.Diagnostic.Information || roleDb.Diagnostic.Warning)
					diagnostic.Set(roleDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				roles.Clear();
			}
			
			for (int i = 0; i < roles.Count; i++)
			{
				RoleItem roleItem = (RoleItem) roles[i];
				ListViewItem item = new ListViewItem();
				item.Tag		= new ItemType(roleItem, ConstString.containerCompanyRoles, roleItem.RoleId);
				item.ImageIndex	= PlugInTreeNode.GetRoleDefaultImageIndex;
				item.Text		= roleItem.Role;
				item.SubItems.Add(roleItem.Description.Replace("\r\n"," "));
				if (roleItem.Disabled)
					item.ForeColor = Color.Red;
				listViewData.Items.Add(item);
			}
		}
		#endregion
	
		#region ListUsersCompany - Visualizza tutti gli Utenti associati a una Azienda
		/// <summary>
		/// ListUsersCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ListUsersCompany()
		{
			listViewData.Items.Clear();
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			ArrayList usersOfCompany			= new ArrayList();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			if (!companyUserDb.SelectAll(out usersOfCompany, parentId))
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
                    diagnostic.Set(companyUserDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersOfCompany.Clear();
			}

			for (int i = 0; i < usersOfCompany.Count; i++)
			{
				CompanyUser userCompanyItem = (CompanyUser)usersOfCompany[i];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(userCompanyItem, ConstString.containerCompanyUsers, userCompanyItem.LoginId);
				item.ImageIndex = (userCompanyItem.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex: PlugInTreeNode.GetUserDefaultImageIndex;
				item.Text = userCompanyItem.Login;
				item.SubItems.Add(userCompanyItem.Description.Replace("\r\n"," "));
				item.SubItems.Add(userCompanyItem.Admin ? Strings.Yes : Strings.No);
				
				if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
					item.SubItems.Add(userCompanyItem.EasyBuilderDeveloper ? Strings.Yes : Strings.No);
	
				item.SubItems.Add(userCompanyItem.DBDefaultUser);
				if (userCompanyItem.Disabled)
				{
					item.SubItems.Add(Strings.Yes);
					item.ForeColor = Color.Red;
				}
				else
					item.SubItems.Add(Strings.No);
				listViewData.Items.Add(item);
			}
		}
		#endregion

		#region ListUsersRolesCompany - Visualizza tutti gli Utenti associati al Ruolo di una Azienda
		/// <summary>
		/// ListUsersRolesCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ListUsersRolesCompany()
		{
			listViewData.Items.Clear();
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			ArrayList usersRolesOfCompany			= new ArrayList();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;

			if (!companyRoleLoginDb.SelectLoginCompanyRole(out usersRolesOfCompany, parentId, roleId, objectId))
			{
				if (companyRoleLoginDb.Diagnostic.Error || companyRoleLoginDb.Diagnostic.Warning || companyRoleLoginDb.Diagnostic.Information)
					diagnostic.Set(companyRoleLoginDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRolesUsersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersRolesOfCompany.Clear();
			}
			
			for (int i = 0; i < usersRolesOfCompany.Count; i++)
			{
				CompanyUser userCompanyItem = (CompanyUser) usersRolesOfCompany[i];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(userCompanyItem, ConstString.itemRoleCompanyUser, roleId);
				item.ImageIndex = (userCompanyItem.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				item.Text = userCompanyItem.Login;
				item.SubItems.Add(userCompanyItem.Description.Replace("\r\n"," "));
				item.SubItems.Add((userCompanyItem.Admin) ? Strings.Yes : Strings.No);
				item.SubItems.Add(userCompanyItem.DBDefaultUser);
				if (userCompanyItem.Disabled)
				{
					item.SubItems.Add(Strings.Yes);
					item.ForeColor = Color.Red;
				}
				else
					item.SubItems.Add(Strings.No);
				listViewData.Items.Add(item);
			}
		}
		#endregion

		#region ListCompany - Visualizza i dati di una Azienda
		/// <summary>
		/// ListCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ListCompany()
		{
			listViewData.Items.Clear();

			ArrayList company				= new ArrayList();
			CompanyDb companyDb				= new CompanyDb();
			companyDb.ConnectionString		= connectionString;
			companyDb.CurrentSqlConnection	= currentConnection;

			if (!companyDb.GetAllCompanyFieldsById(out company, objectId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
					diagnostic.Set(companyDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				company.Clear();
			}
			
			if (company.Count > 0)
			{
				CompanyItem itemCompany = (CompanyItem) company[0];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(itemCompany, ConstString.itemCompany, itemCompany.CompanyId);
                item.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				if (itemCompany.Disabled || !itemCompany.IsValid)
					item.ForeColor = Color.Red;
				item.Text = itemCompany.Company;
				item.SubItems.Add(itemCompany.DbServer);
				item.SubItems.Add(itemCompany.DbName);
				item.SubItems.Add(itemCompany.PreferredLanguage);
				item.SubItems.Add(itemCompany.ApplicationLanguage);
				if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
					item.SubItems.Add(itemCompany.UseDBSlave ? Strings.Yes : Strings.No);
				listViewData.Items.Add(item);
			}
		}
		#endregion
		
		#region ListRole - Visualizza i dati di un Ruolo
		/// <summary>
		/// ListRole
		/// </summary>
		//---------------------------------------------------------------------
		private void ListRole()
		{
			listViewData.Items.Clear();
			ArrayList role				= new ArrayList();
			ArrayList roleLogins		= new ArrayList();
			RoleDb roleDb				= new RoleDb();
			roleDb.ConnectionString		= connectionString;
			roleDb.CurrentSqlConnection = currentConnection;

			if (!roleDb.GetAllRoleFieldsById(out role, objectId, parentId))
			{
				if (roleDb.Diagnostic.Information || roleDb.Diagnostic.Warning || roleDb.Diagnostic.Error)
					diagnostic.Set(roleDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				role.Clear();
			}

			if (role.Count > 0)
			{
				RoleItem itemRole = (RoleItem) role[0];
				CompanyRoleLoginDb loginDb	 = new CompanyRoleLoginDb();
				loginDb.ConnectionString	 = connectionString;
				loginDb.CurrentSqlConnection = currentConnection;
				if (!loginDb.SelectAll(out roleLogins, itemRole.CompanyId, itemRole.RoleId))
				{
					if (roleDb.Diagnostic.Error || roleDb.Diagnostic.Information || roleDb.Diagnostic.Warning)
						diagnostic.Set(roleDb.Diagnostic);
					else
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					roleLogins.Clear();
				}

				if (roleLogins.Count > 0)
				{
					for (int j = 0; j < roleLogins.Count; j++)
					{
						CompanyRoleLogin loginItem = (CompanyRoleLogin)roleLogins[j];
						ListViewItem item = new ListViewItem();
						item.Tag = new ItemType(roleLogins[j], ConstString.itemRole, loginItem.RoleId);
						item.ImageIndex = 
							((CompanyRoleLogin)roleLogins[j]).WindowsAuthentication ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
						item.Text = itemRole.Role;
						item.SubItems.Add(loginItem.Login);
						item.SubItems.Add(itemRole.Description.Replace("\r\n"," "));
						if (loginItem.Disabled)
							item.ForeColor = Color.Red;
						listViewData.Items.Add(item);
					}
				}
				else
				{
					ListViewItem item = new ListViewItem();
					item.Tag = new ItemType(itemRole, ConstString.itemRole, itemRole.RoleId);
					item.ImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
					item.Text = itemRole.Role;
					item.SubItems.Add(string.Empty);
					item.SubItems.Add(itemRole.Description.Replace("\r\n"," "));
					if (itemRole.Disabled)
						item.ForeColor = Color.Red;
					listViewData.Items.Add(item);
				}
			}
		}
		#endregion

		#region ListUserCompany - Visualizza i dati di un Utente associato a una Azienda
		/// <summary>
		/// ListUserCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ListUserCompany()
		{
			listViewData.Items.Clear();
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			ArrayList userCompany				= new ArrayList();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;
			companyUserDb.GetUserCompany(out userCompany, objectId, parentId);

			if (userCompany.Count > 0)
			{
				CompanyUser itemUserCompany = (CompanyUser)userCompany[0];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(itemUserCompany, ConstString.itemCompanyUser, itemUserCompany.LoginId);
				item.ImageIndex = (itemUserCompany.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				item.Text = itemUserCompany.Login;
				item.SubItems.Add(itemUserCompany.Description.Replace("\r\n"," "));
				item.SubItems.Add(itemUserCompany.Admin ? Strings.Yes : Strings.No);
	
				if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
					item.SubItems.Add(itemUserCompany.EasyBuilderDeveloper ? Strings.Yes : Strings.No);
				
				item.SubItems.Add(itemUserCompany.DBDefaultUser);
				if (itemUserCompany.Disabled)
				{
					item.SubItems.Add(Strings.Yes);
					item.ForeColor = Color.Red;
				}
				else
					item.SubItems.Add(Strings.No);
				listViewData.Items.Add(item);
			}
			else
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CompanyUserReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
		}
		#endregion

		#region ListUser - Visualizza i dati di un Utente Applicativo
		/// <summary>
		/// ListUser
		/// </summary>
		//---------------------------------------------------------------------
		private void ListUser()
		{
			listViewData.Items.Clear();
			ArrayList user				= new ArrayList();
			UserDb userDb				= new UserDb();
			userDb.ConnectionString		= connectionString;
			userDb.CurrentSqlConnection = currentConnection;

			if (!userDb.GetAllUserFieldsById(out user, objectId))
			{
				if (userDb.Diagnostic.Error || userDb.Diagnostic.Warning || userDb.Diagnostic.Information)
					diagnostic.Set(userDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UnableToFindUser);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				user.Clear();
			}

			if (user.Count > 0)
			{
				UserItem itemUser = (UserItem) user[0];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(itemUser, ConstString.itemUser, objectId);
                
				item.ImageIndex = (itemUser.WindowsAuthentication) 
					? PlugInTreeNode.GetLoginsDefaultImageIndex 
					: PlugInTreeNode.GetUserDefaultImageIndex;

				item.Text = itemUser.Login;
				item.SubItems.Add(itemUser.Description.Replace("\r\n"," "));

				item.SubItems.Add((itemUser.PasswordNeverExpired) 
					? Strings.PasswordNeverExpire 
					: Convert.ToDateTime(itemUser.ExpiredDatePassword).Date.ToShortDateString());

				item.SubItems.Add(itemUser.PreferredLanguage);
				item.SubItems.Add(itemUser.ApplicationLanguage);

				if (itemUser.ConcurrentAccess)
					item.SubItems.Add(Strings.CALFloating);
				if (itemUser.SmartClientAccess)
					item.SubItems.Add(Strings.CALNamed);
				if (itemUser.WebAccess)
					item.SubItems.Add(Strings.CALEasyLook);

				if (itemUser.Disabled)
					item.ForeColor = Color.Red;
				if (itemUser.Locked)
					item.ForeColor = Color.Gray;

				listViewData.Items.Add(item);
			}
		}
		#endregion

		#region ListProvider - Visualizza i dati di un Provider
		/// <summary>
		/// ListProvider
		/// </summary>
		//---------------------------------------------------------------------
		private void ListProvider()
		{
			listViewData.Items.Clear();
			ArrayList provider				= new ArrayList();
			ProviderDb providerDb			= new ProviderDb();
			providerDb.ConnectionString		= connectionString;
			providerDb.CurrentSqlConnection = currentConnection;
			
			if (!providerDb.GetAllFieldsProviderById(out provider, objectId))
			{
				if (providerDb.Diagnostic.Error || providerDb.Diagnostic.Warning || providerDb.Diagnostic.Information)
					diagnostic.Set(providerDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.ProviderReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				provider.Clear();
			}

			if (provider.Count > 0)
			{
				ProviderItem itemProvider = (ProviderItem)provider[0];
				ListViewItem item = new ListViewItem();
				item.Tag = new ItemType(itemProvider, ConstString.itemProvider, objectId);
                item.ImageIndex = PlugInTreeNode.GetSqlServerGroupDefaultImageIndex;
				item.Text = itemProvider.Description.Replace("\r\n"," ");
				item.SubItems.Add((itemProvider.StripTrailingSpaces) ? Strings.Yes : Strings.No);
				item.SubItems.Add((itemProvider.UseConstParameter) ? Strings.Yes : Strings.No);
				listViewData.Items.Add(item);
			}
		}
		#endregion

		#endregion
	
		#region Funzioni di Update della ListView
		/// <summary>
		/// ListViewDetail_SizeChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_SizeChanged(object sender, System.EventArgs e)
		{
			listViewData.Update();
		}

		/// <summary>
		/// ListViewDetail_KeyUp
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			listViewData.Focus();
			if (listViewData.Items.Count > 0)
				listViewData.Items[0].Focused = true;
		}
		#endregion

		#region Eventi di chiusura, VisibleChange e Resize della form
		/// <summary>
		/// ListViewDetail_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ListViewDetail_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ListViewDetail_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		/// <summary>
		/// ListViewDetail_Resize
		/// Per far sì che le colonne siano impostate alla giusta dimensione
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ListViewDetail_Resize(object sender, System.EventArgs e)
		{
			if (listViewData.Columns.Count > 0)
			{
				for (int i = 0; i < listViewData.Columns.Count; i++)
					listViewData.Columns[i].Width = -2;
			}
		}
		#endregion

		///<summary>
		/// Evento di DoubleClick sulla listview visualizzata nella workingarea della console
		/// A seconda del tipo di item sparo un evento al SysAdmin che apre la form corrispondente
		///</summary>
		//---------------------------------------------------------------------
		private void listViewData_DoubleClick(object sender, EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null || 
				list.SelectedItems.Count == 0 
				|| list.SelectedItems.Count > 1)
				return;

			ListViewItem item = list.SelectedItems[0];

			if (item == null || item.Tag == null)
				return;

			// a seconda del tipo dell'Item apro la form corrispondente
			switch (((ItemType)(item.Tag)).NodeType)
			{
				// Azienda (singolo e contenitore)
				case ConstString.itemCompany:
				case ConstString.containerCompanies:
					{
						if (OnModifyCompany != null)
							OnModifyCompany(sender, ((ItemType)(item.Tag)).NodeId);
						break;
					}

				// Utente applicativo (singolo e contenitore)
				case ConstString.itemUser:
				case ConstString.containerUsers:
					{
						if (OnModifyUser != null)
							OnModifyUser(sender, ((ItemType)(item.Tag)).NodeId);
						break;
					}

				// Utente applicativo associati all'azienda (singolo)
				case ConstString.itemCompanyUser:
					{
						if (OnModifyCompanyUser != null)
							OnModifyCompanyUser(sender, ((ItemType)(item.Tag)).NodeId, ((CompanyUser)(((ItemType)(item.Tag)).ClassCaller)).CompanyId);
						break;
					}

				// Utenti applicativi associati all'azienda (contenitore)
				case ConstString.containerCompanyUsers:
					{
						if (OnModifyContainerCompanyUser != null)
							OnModifyContainerCompanyUser(sender, ((CompanyUser)(((ItemType)(item.Tag)).ClassCaller)));
						break;
					}

				// Ruolo (singolo)
				case ConstString.itemRole:
					{
						if ((((ItemType)(item.Tag)).ClassCaller) is CompanyRoleLogin)
							if (OnModifyRole != null)
								OnModifyRole(sender, ((ItemType)(item.Tag)).NodeId, ((CompanyRoleLogin)(((ItemType)(item.Tag)).ClassCaller)).CompanyId);
						if ((((ItemType)(item.Tag)).ClassCaller) is RoleItem)
							if (OnModifyRole != null)
								OnModifyRole(sender, ((ItemType)(item.Tag)).NodeId, ((RoleItem)(((ItemType)(item.Tag)).ClassCaller)).CompanyId);
						break;
					}

				// Ruoli (contenitore)
				case ConstString.containerCompanyRoles:
					{
						if (OnModifyRole != null)
							OnModifyRole(sender, ((ItemType)(item.Tag)).NodeId, ((RoleItem)(((ItemType)(item.Tag)).ClassCaller)).CompanyId);
						break;
					}
				
				// Utente associati al ruolo di un'azienda (singolo)
				case ConstString.itemRoleCompanyUser:
					{
						if (OnModifyCompanyUserRole != null)
							OnModifyCompanyUserRole
								(sender, 
								((CompanyUser)(((ItemType)(item.Tag)).ClassCaller)).LoginId, 
								((CompanyUser)(((ItemType)(item.Tag)).ClassCaller)).CompanyId, 
								((ItemType)(item.Tag)).NodeId);
						break;
					}

				// Provider (singolo e contenitore)
				case ConstString.itemProvider:
				case ConstString.containerProviders:
					{
						if (OnModifyProvider != null)
							OnModifyProvider(sender, ((ItemType)(item.Tag)).NodeId);
						break;
					}

				default:
					break;
			}
		}
	}

	///<summary>
	/// Classe generica di appoggio da associare alla property Tag di ogni ListViewItem
	/// in modo da risalire sull'evento di SelectedItem al vero e proprio oggetto selezionato
	///</summary>
	//=========================================================================
	public class ItemType
	{
		public object ClassCaller	= null;
		public string NodeType		= string.Empty;
		public string NodeId		= string.Empty;

		//---------------------------------------------------------------------
		public ItemType(object classCaller, string nodeType, string nodeId)
		{
			ClassCaller	= classCaller;
			NodeType	= nodeType;
			NodeId		= nodeId;
		}
	}
}
