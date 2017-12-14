using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	public partial class SetGrants : PlugInsForm
	{   
		public delegate void AfterModifyFormStateHandler(object sender,  bool isEditingState);
		public event AfterModifyFormStateHandler OnAfterModifyFormStateHandler;

		public  GrantsDataGrid	grantsDataGrid		= null;
		public  ArrayList		grantsArrayList		= null;
		public  ArrayList		arrayListRoleGrants	= null;

		private SqlConnection sqlConnection	= null;

		private DataTable	rolesTable	= null;
		private DataTable	usersTable	= null;

		private int		companyId				= -1;
		private int		firstRoleOrUserId		= -1;
		private string	objectNameSpace			= String.Empty;
		private int		objectTypeFromDataBase	= -1;
		private int		objectId				= -1;
		private bool	isRoleFirstSelection	= false;

		private string	roleOrUserText		= string.Empty;
		private int		averageCharWidth	= 1;

		private const string	firstItemText = "------------";

		#region Classi
		//=========================================================================
		public class Role
		{
			private int    roleId = -1;
			private string roleName = String.Empty;

			public int    RoleId	{ get { return roleId; } }
			public string RoleName	{ get { return roleName; } }
			
			public Role (int aRoleId, string aRoleName)
			{
				roleId = aRoleId;
				roleName = aRoleName;
			}
		}
		#endregion

		#region Costruttori
		//---------------------------------------------------------------------
		public SetGrants(ShowObjectsTree aShowObjectsTree)
		{
			InitializeComponent();
            this.SuspendLayout();
			if (aShowObjectsTree == null)
			{
				Debug.Fail("Error in SetGrantsForm constructor: null ShowObjectsTree control.");
				return;
			}

			if (aShowObjectsTree.Connection == null ||
				aShowObjectsTree.Connection.State != ConnectionState.Open ||
				aShowObjectsTree.CurrentMenuXmlNode == null)
			{
				Debug.Fail("Error in SetGrantsForm constructor: invalid ShowObjectsTree control.");
				return;
			}
			
			//Funzioni x le inizializzazioni
			InitVariables(aShowObjectsTree);
			InitializeLabel(aShowObjectsTree);
			SetCharWidth();
			LoadAllRoles();
			LoadAllUsers();		
			InitDataGrid();
			FindGrants();
			CreateGrantsDataGrid(firstRoleOrUserId, isRoleFirstSelection);
			AddRow();

			if (isRoleFirstSelection)
			{
				ShowRolesButton.Visible = false;
				RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(aShowObjectsTree.LoginName, -1);
			}
			else 
			{
				ShowRolesButton.Visible = (((ArrayList)GetUserRoles(firstRoleOrUserId)).Count != 0 );
				UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(aShowObjectsTree.LoginName, -1);

			}

			if (ImportExportFunction.IsProtected(objectNameSpace, objectTypeFromDataBase, companyId, sqlConnection))
			{
				ShowRolesButton.Enabled = AreUserRolesPresent(firstRoleOrUserId);
				InitLockPicture();
				EnabledObject(true);
			}
			else
				EnabledObject(false);

			if (!isRoleFirstSelection)
				ShowBaloonForUser(firstRoleOrUserId);

			this.PerformAutoScale();
		}
		#endregion


		//----------------------------------------------------------------------
		private void ShowBaloonForUser(int userId)
		{
            bool isEasyBuilderDevUser = GrantsFunctions.isEBdevelopment(userId, sqlConnection, companyId);
			bool isCurrentReportOwner = GrantsFunctions.isReportOwner(userId, sqlConnection, companyId);
            bool isAdminUser = GrantsFunctions.isAdmin(userId, sqlConnection, companyId);
			
			if (isEasyBuilderDevUser || isCurrentReportOwner || isAdminUser)
			{
				CurrentUserPictureWithBalloon.Blick = false;
				CurrentUserPictureWithBalloon.Visible = true;
				SetBaloonLayout(isEasyBuilderDevUser, isCurrentReportOwner, isAdminUser);
			}
			else
				CurrentUserPictureWithBalloon.Visible = false;
		}


		//----------------------------------------------------------------------
		private void SetBaloonLayout(bool isEasyBuilderDevUser, bool isCurrentReportOwner, bool isAdminUser)
		{
			SetBaloonImage(isEasyBuilderDevUser, isCurrentReportOwner, isAdminUser);
			SetBaloonMessage(isEasyBuilderDevUser, isCurrentReportOwner, isAdminUser);
		}

		//----------------------------------------------------------------------
		private void SetBaloonImage(bool isEasyBuilderDevUser, bool isCurrentReportOwner, bool isAdminUser)
		{
			string file = string.Empty;

			if (isEasyBuilderDevUser)
				file = "Microarea.Console.Plugin.SecurityAdmin.img.EasyBuilder.gif";

			if (isCurrentReportOwner)
				file = "Microarea.Console.Plugin.SecurityAdmin.img.ReportsAdmin.gif";

			if (isAdminUser)
				file = "Microarea.Console.Plugin.SecurityAdmin.img.Admin.png";

			using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
			{
				if (iconStream == null)
					return;

				CurrentUserPictureWithBalloon.SetImage(new Bitmap(iconStream));
			}
		}

		//----------------------------------------------------------------------
		private void SetBaloonMessage(bool isEasyBuilderDevUser, bool isCurrentReportOwner, bool isAdminUser)
		{
			string message = string.Empty;

			if (isEasyBuilderDevUser)
				message = Strings.EasyBuilderDevUserToolTip;

			if (isCurrentReportOwner)
				message = Strings.CurrentReportOwnerToolTip;

			if (isAdminUser)
				message = Strings.AdminUserToolTip;


			Advertisement currentAdv = new Microarea.TaskBuilderNet.Core.Generic.Advertisement
				  ("", "", message, true, DateTime.Today, MessageType.Default, 0, Guid.NewGuid().ToString(), true, false, MessageSensation.Information);
			currentAdv.Sensation = MessageSensation.Information;//anomalia nel costruttore per cuui non impostava sen, corretta subito dopo uscita 381
			CurrentUserPictureWithBalloon.BringToFront();
			IList<Advertisement> l = new List<Advertisement> { currentAdv };
			//SetStatusBarImage(l.ToArray());

			AdvRendererManager advRendererManager = new AdvRendererManager();
			advRendererManager.CanSave = false;
			advRendererManager.BtnTrashVisible = false;
			advRendererManager.CompactView = true;
			advRendererManager.AdvertisementRenderer = GetAdvRenderer();
			CurrentUserPictureWithBalloon.ContentManager = new AdvContentManager(advRendererManager);
			currentAdv = new Microarea.TaskBuilderNet.Core.Generic.Advertisement
			("", "", message, true, DateTime.Today, MessageType.Default, 0, Guid.NewGuid().ToString(), true, false, MessageSensation.Information);
			advRendererManager.Advertisements = new List<Advertisement> { currentAdv };

			string tooltip = Strings.SpecialUserDescription;
			CurrentUserPictureWithBalloon.Show(tooltip, this.Text);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private BaseAdvRenderer GetAdvRenderer()
		{
			BaseAdvRenderer advRenderer = null;
			try
			{
				advRenderer = new HtmlAdvRenderer();

				((HtmlAdvRenderer)advRenderer).OnLine = true;
				((HtmlAdvRenderer)advRenderer).LoginManagerUrl = @"http://localhost/Development/LoginManager/LoginManager.asmx";
			}
			catch { advRenderer = new TextAdvRenderer(); }

			return advRenderer;
		}
		

	
		//----------------------------------------------------------------------
		private void OnResizeDataGridWidth(object sender, int difference)
		{
			SetToolPosition(grantsDataGrid.Width + grantsDataGrid.Left);		
		}

		//----------------------------------------------------------------------
		private void SetToolPosition (int left)
		{
			left += 20;
			GroupLegend.Left = left;

			ShowRolesButton.Left = left;
			AllowAllButton.Left = left;
			DenyAllButton.Left = left;

			UserLabel.Left = left;
			UsersComboBox.Left = left;
			RolesLabel.Left = left;
			RolesComboBox.Left = left;
			CurrentUserPictureWithBalloon.Left = UserLabel.Left + UserLabel.Width + 10;
			NamespaceLabel.Size = new Size(this.Right - 30 - NamespaceLabel.Left, NamespaceLabel.Height);
		}



		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);
			
			//Set dynamic form layout
			grantsDataGrid.CaptionVisible = false;
			grantsDataGrid.Left   = 4;
			grantsDataGrid.Top    = NamespaceLabel.Bottom + 4;

			Controls.Add(grantsDataGrid);

			AddLegend(grantsDataGrid.GetType().Assembly);
			GroupLegend.Visible = true;

			SetToolPosition(grantsDataGrid.Width + grantsDataGrid.Left);
		}

		//--------------------------------------------------------------------
		private void grantsDataGrid_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
		}
		
		//---------------------------------------------------------------------
		private void onModifyColumnValue(object sender, int rowNumber)
		{
			SetEditState();
		}

		//---------------------------------------------------------------------
		private void SetEditState()
		{
			this.State = StateEnums.Editing;

			if (OnAfterModifyFormStateHandler != null)
				OnAfterModifyFormStateHandler(this, true);
		}

		//---------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			// Invoke base class implementation
			base.OnResize(e);

			if (grantsDataGrid != null)
			{
                this.SuspendLayout();
				grantsDataGrid.ResizeToFit(this);
				grantsDataGrid.Height = (GroupLegend.Bottom - grantsDataGrid.Top);
			}
			if (NamespaceLabel != null)
				NamespaceLabel.Size = new Size(this.Right - 30 - NamespaceLabel.Left, NamespaceLabel.Height);
		}
		
		#region Funzioni d'inizializzazione

		#region inizializzo il GrantsDataGrid
		//---------------------------------------------------------------------
		void CreateGrantsDataGrid(int aRoleOrUserId, bool aIsRole)
		{
			CommonObjectTreeFunction.LoadStoredProcedure(sqlConnection, ref grantsArrayList, aIsRole, 
				companyId, aRoleOrUserId, objectId);
		}
		//---------------------------------------------------------------------
		private void AddRow()
		{
			for (int i=0; i < grantsArrayList.Count; i++)
			{
				if (grantsArrayList[i] != null && grantsArrayList[i] is GrantsRow)
					grantsDataGrid.AddGrantsRow((GrantsRow)grantsArrayList[i]);
			}

			grantsDataGrid.ResizeToFit(this);
			grantsDataGrid.Height = (GroupLegend.Bottom - grantsDataGrid.Top);
		}
		//---------------------------------------------------------------------
		#endregion

		#region Funzione che cerca i Grants Associati alla tipologia
	
		//---------------------------------------------------------------------
		/// <summary>
		/// Cerca i Grants ai quali è associata la tipologia di oggetto che ho
		/// selezionato nel Tree della Carlotta
		/// </summary>
		private void FindGrants ()
		{
			if (grantsArrayList == null)
				grantsArrayList = new ArrayList();

			if (arrayListRoleGrants == null)
				arrayListRoleGrants = new ArrayList();

			//Mi tiro su tutti i tipi di permesso per quel tipo di oggetto			
			string select = @"SELECT MSD_ObjectTypeGrants.GrantName, 
                                    MSD_ObjectTypeGrants.GrantMask, 
                                    MSD_Objects.ObjectId
							   FROM MSD_ObjectTypeGrants INNER JOIN
								  MSD_Objects ON MSD_Objects.TypeId = MSD_ObjectTypeGrants.TypeId
							 WHERE (MSD_Objects.NameSpace= @NameSpace AND MSD_Objects.TypeId= @TypeId)
							 ORDER BY MSD_ObjectTypeGrants.GrantMask";

			SqlCommand mySqlCommand = new SqlCommand(select, sqlConnection);
            mySqlCommand.Parameters.AddWithValue("@NameSpace", objectNameSpace);
            mySqlCommand.Parameters.AddWithValue("@TypeId", objectTypeFromDataBase);
			SqlDataReader myReader = null;

			try
			{
				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					grantsArrayList.Add(new GrantsRow(Convert.ToInt32(myReader["GrantMask"]), Convert.ToInt32(myReader["ObjectId"]), GrantsString.GetGrantDescription(myReader["GrantName"].ToString())));
					arrayListRoleGrants.Add(new GrantsRow(Convert.ToInt32(myReader["GrantMask"]), myReader["GrantName"].ToString(), String.Empty));
				}
				myReader.Close();
				mySqlCommand.Dispose();
			}
			catch (SqlException err)
			{
				if (myReader != null) myReader.Close();
				mySqlCommand.Dispose();
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);	
			}
		}

		#endregion

		//----------------------------------------------------------------------
		private void EnabledObject(bool areEnabled)
		{
			ShowRolesButton.Enabled = areEnabled;
			LockPictureBox.Visible	= areEnabled;
			AllowAllButton.Enabled	= areEnabled;
			DenyAllButton.Enabled	= areEnabled;
			UsersComboBox.Enabled	= areEnabled;
			RolesComboBox.Enabled	= areEnabled;
		}

		//----------------------------------------------------------------------
		private void InitLockPicture()
		{
			//Setto l'icona del lucchetto
			Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.LOCKCLOSE.BMP");
			
			if (myStream != null)
			{
				Bitmap lockBitmap = new Bitmap(Image.FromStream(myStream, true));
				lockBitmap.MakeTransparent(Color.Magenta);
				LockPictureBox.Image = lockBitmap;
			}
		}

		//----------------------------------------------------------------------
		private void InitDataGrid()
		{
			grantsArrayList		= new ArrayList();
			arrayListRoleGrants	= new ArrayList();

			grantsDataGrid = new GrantsDataGrid(false, isRoleFirstSelection, this);
			grantsDataGrid.OnModifyColumnValueHandle += new GrantsDataGrid.ModifyColumnValueHandle(onModifyColumnValue);
			grantsDataGrid.OnResizeWidth +=  new GrantsDataGrid.ResizeWidth(OnResizeDataGridWidth);
		}

		//----------------------------------------------------------------------
		private void SetCharWidth()
		{
			System.Drawing.Graphics currentGraphics = this.CreateGraphics();
			System.Drawing.SizeF sampleStringSize = currentGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", this.Font);
			currentGraphics.Dispose();
			averageCharWidth =(int)Math.Ceiling(sampleStringSize.Width)/52;
		}

		//----------------------------------------------------------------------
		private void InitVariables(ShowObjectsTree aShowObjectsTree)
		{
			sqlConnection			= aShowObjectsTree.Connection;
			companyId				= aShowObjectsTree.CompanyId;
			firstRoleOrUserId		= aShowObjectsTree.RoleOrUserId;
			isRoleFirstSelection	= aShowObjectsTree.IsRoleLogin;
			objectNameSpace			= aShowObjectsTree.CurrentMenuXmlNode.ItemObject;
            objectTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(aShowObjectsTree.CurrentMenuXmlNode, sqlConnection);
			roleOrUserText			= aShowObjectsTree.LoginName;

			objectId = CommonObjectTreeFunction.GetObjectId(objectNameSpace, objectTypeFromDataBase, sqlConnection);
		
			if (objectId == -1)
			{
				if(CommonObjectTreeFunction.InsertObjectInDB(aShowObjectsTree.MenuManagerWinControl.CurrentCommandNode, sqlConnection))
					objectId = CommonObjectTreeFunction.GetObjectId(objectNameSpace, objectTypeFromDataBase, sqlConnection);
			}
		}

		//---------------------------------------------------------------------
		private void InitializeLabel(ShowObjectsTree aShowObjectsTree)
		{
			ShowRolesButton.Text = Strings.ShowRole;
			
			string obj = aShowObjectsTree.CurrentMenuXmlNode.Title;
			string des = aShowObjectsTree.CurrentMenuXmlNode.ExternalDescription;
			NamespaceLabel.Text	 = 
				 obj +
				((des != null && des != "" && string.Compare(des, obj, true, CultureInfo.InvariantCulture) != 0) ? " - " + des : "") +
				"\r\n[" + objectNameSpace + "]" ;

			MenuInfo.SetExternalDescription(aShowObjectsTree.pathFinder, aShowObjectsTree.CurrentMenuXmlNode);
		}

		//---------------------------------------------------------------------
		private void AddLegend(Assembly executingAssembly)
		{
			//Pic ALLOWED
			Stream imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.allowed.bmp");
			Bitmap bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			this.pctAllowed.Image = bitmap;

			//PIC FORBIDDEN
			imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.forbidden.bmp");
			bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			pctForbidden.Image = bitmap;

			//PIC INHERIT
			imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.inherit.bmp");
			bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			pctInherit.Image = bitmap;
					
			//PIC NA
			imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.notassigned.bmp");
			bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			pctNa.Image = bitmap;

			//PIC INHERIT
			imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.notexist.bmp");
			bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			pctNothing.Image = bitmap;
		}

		//---------------------------------------------------------------------
		private void LoadAllRoles()
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return ;

			string sSelect = "SELECT RoleId, Role, Disabled FROM MSD_CompanyRoles where CompanyId=" + this.companyId + "ORDER BY Role";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, sqlConnection);
			SqlDataReader myReader  = mySqlCommand.ExecuteReader();

			RolesComboBox.DataSource = null;
			this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);

			if (rolesTable == null)
			{
				rolesTable = new DataTable();
				rolesTable.Columns.Add(new DataColumn("Role",	Type.GetType("System.String")));
				rolesTable.Columns.Add(new DataColumn("RoleId",	Type.GetType("System.Int32")));
			}
			else
				rolesTable.Clear();

			DataRow row = rolesTable.NewRow();
			row["Role"]	= firstItemText;
			row["RoleId"] = -1;
			rolesTable.Rows.Add(row);

			int maxlength = 0;

			while (myReader.Read())
			{
				row = rolesTable.NewRow();
				row["Role"]	= myReader["Role"].ToString();
				row["RoleId"] = Convert.ToInt32(myReader["RoleId"]);
				rolesTable.Rows.Add(row);

				if (myReader["Role"].ToString().Length > maxlength)
					maxlength = myReader["Role"].ToString().Length;
			}

			myReader.Close();
			mySqlCommand.Dispose();

			//Setto le proprietà di DataSource, DisplayMember, ValueMember
			RolesComboBox.DataSource	= rolesTable;
			RolesComboBox.DisplayMember	= "Role";
			RolesComboBox.ValueMember	= "RoleId";
			maxlength = (maxlength * averageCharWidth) + RolesComboBox.Height + 10 ;

			if (maxlength > RolesComboBox.Width)
				RolesComboBox.Width = maxlength;

			this.RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
		}

		//---------------------------------------------------------------------
		private void LoadAllUsers()
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return ;

			int EasyLookLoginId = CommonObjectTreeFunction.GetApplicationUserID(NameSolverStrings.EasyLookSystemLogin, sqlConnection);

			string sSelect =@"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login, MSD_CompanyLogins.Disabled,
								MSD_CompanyLogins.Admin FROM MSD_CompanyLogins INNER JOIN
								MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId where
								CompanyId=" + companyId + " AND MSD_CompanyLogins.LoginId <> " + EasyLookLoginId + " ORDER BY MSD_Logins.Login" ;

			SqlCommand mySqlCommand = new SqlCommand(sSelect, sqlConnection);
			SqlDataReader myReader  = mySqlCommand.ExecuteReader();

			UsersComboBox.DataSource = null;

			this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);

			int maxlength = 0;

			if (usersTable == null)
			{
				usersTable = new DataTable();
				usersTable.Columns.Add(new DataColumn("Login",		Type.GetType("System.String")));
				usersTable.Columns.Add(new DataColumn("LoginId",	Type.GetType("System.Int32")));
			}
			else
				usersTable.Clear();

			DataRow row		= usersTable.NewRow();
			row["Login"]	= firstItemText;
			row["LoginId"]	= -1;
			usersTable.Rows.Add(row);

			while (myReader.Read())
			{
				row = usersTable.NewRow();
				row["Login"]	= myReader["Login"].ToString();
				row["LoginId"] = Convert.ToInt32(myReader["LoginId"]);
				usersTable.Rows.Add(row);

				if (myReader["Login"].ToString().Length > maxlength)
					maxlength = myReader["Login"].ToString().Length;
			}

			myReader.Close();
			mySqlCommand.Dispose();

			maxlength = (maxlength * averageCharWidth) + UsersComboBox.Height + 10 ;

			//Setto le proprietà di DataSource, DisplayMember, ValueMember
			UsersComboBox.DataSource	= usersTable;
			UsersComboBox.DisplayMember	= "Login";
			UsersComboBox.ValueMember	= "LoginId";

			if (maxlength > UsersComboBox.Width)
			{
				//Se cresce la combo devo spostare tutto
				RolesLabel.Left = RolesLabel.Left + (maxlength - UsersComboBox.Width);
				RolesComboBox.Left = this.RolesComboBox.Left + (maxlength - UsersComboBox.Width);
				UsersComboBox.Width = maxlength;
			}

			this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
		}

		
		#endregion

		#region Salvataggio nuovi Grants

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che salva i Grant nel DB viene chiamata quando clicco
		/// sul pulsante della console Salva
		/// </summary>
		public void SaveGrants()
		{
			//Per prima cosa devo controllare se l'oggetto è protetto
			if (!ImportExportFunction.IsProtected(objectNameSpace, objectTypeFromDataBase, companyId, sqlConnection))
			{
				DiagnosticViewer.ShowWarning(Strings.FirstProtect, Strings.Error);
				return;
			}

			int	grants		= 0;
			int	inheritMask	= 0;
			
			grantsDataGrid.Focus();
			
			DataTable dataTable = (DataTable)grantsDataGrid.DataSource.Tables["Grants"];
		
			int currentId = -1; 
			bool isRole = false; 
			//aseconda che sia selezionato un rulo o un utente dalle combobox devo 
			//salvare su quello
			if (RolesComboBox.Text == firstItemText)
				currentId = Convert.ToInt32(UsersComboBox.SelectedValue);
			else
			{
				isRole = true;
				currentId = Convert.ToInt32(RolesComboBox.SelectedValue);
			}

			GrantsFunctions.ApplyGrantsRules(ref dataTable, ref grants, ref inheritMask, grantsArrayList, !isRole);

			//Cancello i permessi che eventualmente avevo salvato prima
			string sDelete = ""; 
			if (isRole)
				sDelete = @"DELETE FROM MSD_ObjectGrants 
					WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND RoleId=@ParamId";
			else
				sDelete = @"DELETE FROM MSD_ObjectGrants 
					WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND LoginId=@ParamId";

			SqlCommand mySqlCommandDel = new SqlCommand(sDelete, sqlConnection);
            mySqlCommandDel.Parameters.AddWithValue("@CompanyId", companyId);
            mySqlCommandDel.Parameters.AddWithValue("@ObjectId", objectId);
            mySqlCommandDel.Parameters.AddWithValue("@ParamId", currentId);
			mySqlCommandDel.ExecuteNonQuery();
			mySqlCommandDel.Dispose();

			string sInsert = @"INSERT INTO MSD_ObjectGrants
						(CompanyId, ObjectId, LoginId, RoleId, Grants, InheritMask)
						VALUES 
						(@CompanyId, @ObjectId, @LoginId,  @RoleId, @Grants, @InheritMask)";

			SqlCommand mySqlCommand = new SqlCommand(sInsert, sqlConnection);
            mySqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
            mySqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
            mySqlCommand.Parameters.AddWithValue("@Grants", grants);
            mySqlCommand.Parameters.AddWithValue("@InheritMask", inheritMask);
			mySqlCommand.Parameters.Add("@LoginId",		SqlDbType.Int );
			mySqlCommand.Parameters.Add("@RoleId",		SqlDbType.Int);

			mySqlCommand.Parameters["@LoginId"].Value = isRole ? 0 : currentId;
			mySqlCommand.Parameters["@RoleId"].Value  = isRole ? currentId : 0;

			try
			{
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();
				
				CreateGrantsDataGrid(currentId, isRole);

				bool addRelation = false;

				if (grantsDataGrid.DataSource.Relations.Count>1 || grantsDataGrid.DataSource.Relations.Count ==1)
					addRelation = true;

				grantsDataGrid.DataSource = null;
				grantsDataGrid.SetDataSource(false, isRole);	
				AddRow();

				if (addRelation)
				{
					ShowRolesButton.Enabled	 = true;
					ShowRolesButton.Text = Strings.ShowRole;
				}

				State = StateEnums.None;
                
                BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

				if (OnAfterModifyFormStateHandler != null)
					OnAfterModifyFormStateHandler(this, false);
			}
			catch(SqlException err)
			{
				mySqlCommand.Dispose();
				State = StateEnums.None;
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
			}
		}
		
		#endregion

		#region Funzioni x mostrare i Grants dei ruoli

		#region Bottone che li mostra
		
		//---------------------------------------------------------------------
		private void ShowRolesButton_Click(object sender, System.EventArgs e)
		{
			grantsDataGrid.Focus();

			if (String.Compare(ShowRolesButton.Text, Strings.ShowRole, true, CultureInfo.InvariantCulture) == 0)
			{
				if (ShowRoleRelation())
					ShowRolesButton.Text = Strings.HideRoles;
				else
					ShowRolesButton.Enabled = false;
			}
			else
			{
				HideRoleRelation();
				ShowRolesButton.Text = Strings.ShowRole;
			}
		}

		//---------------------------------------------------------------------
		private void HideRoleRelation()
		{
			grantsDataGrid.CaptionVisible = false;
			if (grantsDataGrid.DataSource.Relations == null || grantsDataGrid.DataSource.Relations.Count == 0)
				return;	
																			
			grantsDataGrid.DataSource.Relations.Clear();
		}

		//---------------------------------------------------------------------
		private bool ShowRoleRelation()
		{
			if (arrayListRoleGrants == null)
				arrayListRoleGrants	= new ArrayList();

			//Prima controllo se ho almeno un ruolo
			ArrayList arrayListRoles = GetUserRoles(Convert.ToInt32(UsersComboBox.SelectedValue));

			if (arrayListRoles == null || arrayListRoles.Count == 0 ) 
			{
				DiagnosticViewer.ShowWarning(Strings.NoUserRoles, SecurityConstString.SecurityAdminPlugIn);
				return false;
			}

			if (grantsDataGrid.DataSource.Tables["Roles"].Rows.Count != 0)
			{
				grantsDataGrid.AddRolesRelation();
				grantsDataGrid.CaptionVisible = true;
				return true;
			}

			for (int i=0; i< arrayListRoles.Count; i++)
			{
				if (arrayListRoles[i] == null || !(arrayListRoles[i] is Role) || ((Role)arrayListRoles[i]).RoleId == -1)
					continue;

				Role aRole = (Role)arrayListRoles[i];
				grantsDataGrid.LoadRoleRow(aRole.RoleId, aRole.RoleName, companyId, objectId, sqlConnection, ref arrayListRoleGrants);
				
				for (int j=0; j < arrayListRoleGrants.Count; j++)
				{
					GrantsRow rowGrants = new GrantsRow(((GrantsRow)arrayListRoleGrants[j]).Mask, ((GrantsRow)arrayListRoleGrants[j]), aRole.RoleName);
					grantsDataGrid.AddRoleRow(rowGrants);
				}
			}

			grantsDataGrid.AddRolesRelation();	
			grantsDataGrid.CaptionVisible = true;
			return true;
		}
		//---------------------------------------------------------------------
		#endregion

		#region Carico i Fuoli dell'Utente

		//---------------------------------------------------------------------
		private bool AreUserRolesPresent(int roleOrUserId)
		{
			SqlCommand mySqlCommand = null;
			try
			{
				string sSelect = @"SELECT COUNT(*)	FROM MSD_CompanyRolesLogins INNER JOIN
								MSD_CompanyRoles ON MSD_CompanyRolesLogins.RoleId = MSD_CompanyRoles.RoleId
								WHERE MSD_CompanyRolesLogins.CompanyId = " + companyId.ToString() + " AND " + 
								"MSD_CompanyRolesLogins.LoginId = " + roleOrUserId.ToString();

				mySqlCommand = new SqlCommand(sSelect, sqlConnection);

				int recordsCount = (int)mySqlCommand.ExecuteScalar();

				mySqlCommand.Dispose();

				return (recordsCount > 0);
			}
			catch (SqlException)
			{
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
				return false;
			}
		}

		//---------------------------------------------------------------------
		private ArrayList GetUserRoles(int userId)
		{
			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;
			try
			{
				ArrayList arrayListRoles = new ArrayList();

				string sSelect = @"SELECT MSD_CompanyRolesLogins.RoleId, MSD_CompanyRoles.Role
								FROM MSD_CompanyRolesLogins INNER JOIN 
								MSD_CompanyRoles ON MSD_CompanyRolesLogins.RoleId = MSD_CompanyRoles.RoleId
								WHERE MSD_CompanyRolesLogins.CompanyId = " + companyId.ToString() + " AND " + 
								"MSD_CompanyRolesLogins.LoginId = " + userId.ToString();

				mySqlCommand = new SqlCommand(sSelect, sqlConnection);

				myReader = mySqlCommand.ExecuteReader();
				while ( myReader.Read())
				{
					arrayListRoles.Add(new Role(Convert.ToInt32(myReader["RoleId"]), myReader["Role"].ToString())); 
				}
				
				myReader.Close();
				mySqlCommand.Dispose();
				
				return arrayListRoles;
			}
			catch (SqlException)
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();

				return null;
			}
		}

		#endregion

		#endregion

		#region Bottoni AllDeny AllAllow
		//---------------------------------------------------------------------
		private void AllowAllButton_Click(object sender, System.EventArgs e)
		{
			GrantsFunctions.SetValueForAllGrants( this.grantsDataGrid, GrantOperationType.Allow);
			SetEditState();
		}

		//---------------------------------------------------------------------
		private void DenyAllButton_Click(object sender, System.EventArgs e)
		{
			GrantsFunctions.SetValueForAllGrants( this.grantsDataGrid, GrantOperationType.Deny);
			SetEditState();
		}

		#endregion

		//---------------------------------------------------------------------
		private void UsersComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (UsersComboBox.Text == firstItemText)
			{
				if (isRoleFirstSelection)
				{
					this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(roleOrUserText);
					RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(firstItemText);
					this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					ShowRolesButton.Visible = false;
					HideRoleRelation();
					CurrentUserPictureWithBalloon.Visible = false;
				}					
				else
				{

					this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(roleOrUserText);
					this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(firstItemText);
					RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					ShowRolesButton.Visible = true;
					ShowRolesButton.Text = Strings.ShowRole;
					if (isRoleFirstSelection)
						ShowBaloonForUser(firstRoleOrUserId);
					
				}
				RefreshGrantsGrid(firstRoleOrUserId, isRoleFirstSelection);
				return;
			}

			RefreshGrantsGrid(Convert.ToInt32(UsersComboBox.SelectedValue), false);
			this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
			RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(firstItemText);
			this.RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
			ShowRolesButton.Visible = true;
			ShowRolesButton.Enabled = AreUserRolesPresent(Convert.ToInt32(UsersComboBox.SelectedValue));
			HideRoleRelation();
			SetToolPosition(grantsDataGrid.Width + grantsDataGrid.Left);

			if (!isRoleFirstSelection)
				ShowBaloonForUser(Convert.ToInt32(UsersComboBox.SelectedValue));

		}

		//---------------------------------------------------------------------
		private void RolesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (RolesComboBox.Text == firstItemText)
			{
				if (isRoleFirstSelection)
				{
					this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(roleOrUserText);
					RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(firstItemText);
					this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					ShowRolesButton.Visible = false;
					HideRoleRelation();
				}					
				else
				{
					this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(roleOrUserText);
					this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
					this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					RolesComboBox.SelectedIndex = RolesComboBox.FindStringExact(firstItemText);
					RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
					ShowRolesButton.Visible = true;	
				}
				RefreshGrantsGrid(firstRoleOrUserId, isRoleFirstSelection);
				return;
			}

			RefreshGrantsGrid(Convert.ToInt32(RolesComboBox.SelectedValue), true);
			this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
			UsersComboBox.SelectedIndex = UsersComboBox.FindStringExact(firstItemText, -1);
			this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
			ShowRolesButton.Visible = false;
			HideRoleRelation();
			ShowRolesButton.Text = Strings.ShowRole;
		}

		//---------------------------------------------------------------------
		private void RefreshGrantsGrid(int aRoleOrUserId, bool aIsRole)
		{
			grantsDataGrid.DataSource.Clear();
			grantsDataGrid.CreateGrantsTableStyle(false, aIsRole);
			grantsDataGrid.CreateRolesTableStyle();
			CreateGrantsDataGrid(aRoleOrUserId, aIsRole);
			AddRow();
		}

        private void SetGrants_Load(object sender, EventArgs e)
        {

        }

		private void SetGrants_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{

		}

		private void SetGrants_Leave(object sender, EventArgs e)
		{

		}

		private void SetGrants_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{

		}

		private void SetGrants_Deactivate(object sender, EventArgs e)
		{

		}

		private void SetGrants_ParentChanged(object sender, EventArgs e)
		{
			if (Parent == null)
				CurrentUserPictureWithBalloon.Close();
				
		}

		
	}

}
