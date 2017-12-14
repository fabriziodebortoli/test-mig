using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;


namespace Microarea.Console.Plugin.SecurityAdmin
{

	public partial class ViewObjectGrants : PlugInsForm
	{   

		private  ArrayList		grantsArrayList	= null;
		private SqlConnection	sqlConnection	= null;
		
		private string	sqlConnectionString = string.Empty;
		private string	objectNameSpace		= String.Empty;
		
		private int	companyId				= -1;
		private int	objectTypeFromDataBase	= -1;
		private int	objectId = -1;
		private int	roleOrUserId			= -1;

		private bool isRole		= true;
		private bool isReport	= false;


		#region Costruttori
		//---------------------------------------------------------------------
		public ViewObjectGrants(ShowObjectsTree aShowObjectsTree)
		{
			InitializeComponent();

			if (aShowObjectsTree == null)
			{
				Debug.Fail("Error in SetGrantsForm constructor: null ShowObjectsTree control.");
				return;
			}

			if 
				(
				aShowObjectsTree.Connection == null ||
				aShowObjectsTree.Connection.State != ConnectionState.Open ||
				aShowObjectsTree.CurrentMenuXmlNode == null
				)
			{
				Debug.Fail("Error in SetGrantsForm constructor: invalid ShowObjectsTree control.");
				return;
			}

			sqlConnectionString		= aShowObjectsTree.ConnectionString;
			sqlConnection			= aShowObjectsTree.Connection;
			companyId				= aShowObjectsTree.CompanyId;
			roleOrUserId			= aShowObjectsTree.RoleOrUserId;
			objectNameSpace			= aShowObjectsTree.CurrentMenuXmlNode.ItemObject;
			int codeType = MenuSecurityFilter.GetType(aShowObjectsTree.CurrentMenuXmlNode);
			isReport = codeType == 4;
			objectTypeFromDataBase	= CommonObjectTreeFunction.GetObjectTypeId(aShowObjectsTree.CurrentMenuXmlNode, sqlConnection);
			
			objectId = CommonObjectTreeFunction.GetObjectId(objectNameSpace, objectTypeFromDataBase, sqlConnection);

			if (objectId == -1)
			{
				if(CommonObjectTreeFunction.InsertObjectInDB(aShowObjectsTree.MenuManagerWinControl.CurrentCommandNode, sqlConnection))
					objectId = CommonObjectTreeFunction.GetObjectId(objectNameSpace, objectTypeFromDataBase, sqlConnection);
			}

			grantsArrayList	= new ArrayList();
			
			ObjectLabel.AutoSize		= true;
			ObjectLabel.Text			= Strings.Object  + " " + aShowObjectsTree.CurrentMenuXmlNode.Title;
			NamespaceLabel.Text			= "NameSpace: " + objectNameSpace;
			
			MenuInfo.SetExternalDescription(aShowObjectsTree.pathFinder, aShowObjectsTree.CurrentMenuXmlNode);
			
			if (ImportExportFunction.IsProtected(objectNameSpace, objectTypeFromDataBase, companyId, sqlConnection))
			{
				//Setto l'icona del lucchetto
				Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.LOCKCLOSE.BMP");
				if (myStream != null)
				{
					Bitmap lockBitmap = new Bitmap(Image.FromStream(myStream, true));
					lockBitmap.MakeTransparent(Color.Magenta);

					LockPictureBox.Image = lockBitmap;
				}
				LockPictureBox.Visible = true;
			}
			else
				LockPictureBox.Visible = false;

			isRole = aShowObjectsTree.IsRoleLogin;
			
			if (isRole)
				this.AllRolesRadioButton.Checked = true;
			else
				this.AllUsersRadioButton.Checked = true;
		}

		#endregion

		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (sqlConnectionString == null || sqlConnection == null)
				return;
			
			FindGrants();
			AddRows();
			AddLegend();

			if (!ImportExportFunction.IsProtected(objectNameSpace, objectTypeFromDataBase, companyId, sqlConnection))
				grantsDataGrid.Enabled = false;

			AdjustLayoutGrid();
		}

		//---------------------------------------------------------------------
		private void AdjustLayoutGrid()
		{
			grantsDataGrid.Width = 55;
			if (grantsDataGrid.Rows.Count > 0)
			{
				foreach (DataGridViewColumn col in grantsDataGrid.Columns)
				{
					if (col.Visible)
						grantsDataGrid.Width = col.Width + grantsDataGrid.Width + grantsDataGrid.VerticalScrollingOffset;
				}
			}
		}

		//---------------------------------------------------------------------
		private void AddLegend()
		{
			GrantsDataGrid dg = new GrantsDataGrid(false);
			Assembly executingAssembly = dg.GetType().Assembly;

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
			imageStream = executingAssembly.GetManifestResourceStream(SecurityConstString.SecurityLibraryNamespace + ".img.notexist.bmp");
			bitmap = new Bitmap(imageStream);
			bitmap.MakeTransparent();
			pctNothing.Image = bitmap;

		}
	
		
		#region Funzioni d'inizializzazione

		#region inizializzo il GrantsDataGrid
	
		//---------------------------------------------------------------------
		private void AddRows()
		{
			string  sSelect = string.Empty;
			int		cod		= -1;
			string	name	= string.Empty;

			if (isRole)
				sSelect = @"SELECT RoleId, Role FROM MSD_CompanyRoles WHERE CompanyId = " + this.companyId;
			else
				sSelect = @"SELECT MSD_Logins.Login, MSD_CompanyLogins.LoginId
								FROM MSD_CompanyLogins INNER JOIN
										MSD_Logins ON MSD_CompanyLogins.LoginId = MSD_Logins.LoginId
										WHERE     (MSD_CompanyLogins.CompanyId = " + this.companyId + @") AND (MSD_CompanyLogins.Admin <> 1)";


			grantsDataGrid.Columns[2].Visible = !isRole;

			SqlCommand mySqlCommand = new SqlCommand(sSelect, sqlConnection);
			SqlDataReader myReader = null;
			try
			{
				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					if(isRole)
					{
						cod =Convert.ToInt32(myReader["RoleId"]);
						name = myReader["Role"].ToString();
					}
					else
					{
						cod =Convert.ToInt32(myReader["LoginId"]);
						name = myReader["Login"].ToString();
					}

					GlobalDataGridGrants.LoadStoredProcedure(sqlConnectionString, ref grantsArrayList, isRole, 
						companyId, cod, objectId);
					
					if (grantsArrayList == null || grantsArrayList.Count == 0)
						continue;

					AddGrantsRow(cod, name, grantsArrayList);
				}
				myReader.Close();
				mySqlCommand.Dispose();
			}
			catch(Exception exc )
			{
				Debug.WriteLine(exc.ToString());
				if (myReader != null)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}

			GetSpecialUserIcon();

		}

		//---------------------------------------------------------------------
		public void AddGrantsRow(int userId, string name, ArrayList grantsArrayList)
		{
			int index = grantsDataGrid.Rows.Add(name, GetIcon(name, (GrantInfo)grantsArrayList[0]), null, userId);
		}

		//---------------------------------------------------------------------
		public void GetSpecialUserIcon()
		{
			if (isRole)
				return;

			foreach(DataGridViewRow row in grantsDataGrid.Rows)
			{
				int userId = Convert.ToInt32(row.Cells[3].Value);

                if (GrantsFunctions.isEBdevelopment(userId, sqlConnection, companyId))
				{
					row.Cells[2].Value = Resource.EasyBuilder;
					row.Cells[2].ToolTipText = Strings.EasyBuilderDevUser;
					if (isReport)
						row.Cells[1].Value = Resource.allowed;

				}
				else if (GrantsFunctions.isReportOwner(userId, sqlConnection, companyId))
				{
					row.Cells[2].Value = Resource.ReportsAdmin;
					row.Cells[2].ToolTipText = Strings.CurrentReportOwner;
					if (isReport)
						row.Cells[1].Value = Resource.allowed;
				}
                else if (GrantsFunctions.isAdmin(userId, sqlConnection, companyId))
				{
					row.Cells[2].Value = Resource.Admin;
					row.Cells[2].ToolTipText = Strings.AdminUser;
					row.Cells[1].Value = Resource.allowed;
				}
				else
					((DataGridViewImageCell)row.Cells[2]).Value = Resource.NULL;
			}
		}

		//---------------------------------------------------------------------
		public Image GetIcon(string name, GrantInfo grant)
		{
			if (grant.TotalIconState == GrantInfo.IconState.IconAllowed)
				return Resource.allowed;

			if (grant.TotalIconState == GrantInfo.IconState.IconForbidden)
				return Resource.forbidden;

			if (grant.TotalIconState == GrantInfo.IconState.IconInherit)
				return Resource.inherit;

			if (grant.TotalIconState == GrantInfo.IconState.IconNotAssigned)
				return Resource.notassigned;

			if (grant.TotalIconState == GrantInfo.IconState.IconNotExist)
				return Resource.notexist;

			return null;
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
					grantsArrayList.Add(new GrantInfo(Convert.ToInt32(myReader["GrantMask"]), myReader["GrantName"].ToString(), GrantsString.GetGrantDescription(myReader["GrantName"].ToString())));
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

		//---------------------------------------------------------------------------
		private void AllRolesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				isRole = true;
				CreateDataSource();
				AdjustLayoutGrid();
			}
		}

		//---------------------------------------------------------------------------
		private void CreateDataSource()
		{
			if (grantsDataGrid != null)
			{
				grantsDataGrid.Rows.Clear();
				grantsDataGrid.DataSource = null;
				if (grantsArrayList == null)
					return;
				AddRows();
			}
		}

		#endregion

		//---------------------------------------------------------------------------
		private void AllUsersRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				isRole = false;
				CreateDataSource();
				AdjustLayoutGrid();
			}
		}

		private void NothingLabel_Click(object sender, EventArgs e)
		{

		}

	}
}
