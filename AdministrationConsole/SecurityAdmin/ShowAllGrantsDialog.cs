using System;
using System.Collections;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.SecurityLayer;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	public partial class ShowAllGrantsDialog : Form
	{
		private SqlConnection sqlConnection;
        private ShowObjectsTree showObjectsTree = null;
		private MenuXmlParser menuXmlParser = null;
		private int companyId = -1;
		private DataTable rolesTable = null;
		private DataTable usersTable = null;
		private int averageCharWidth = 1;
		private string firstItemText = "------------------";
        private Microarea.Console.Plugin.SecurityAdmin.WizardForms.SecurityObjectsImages soi = new Microarea.Console.Plugin.SecurityAdmin.WizardForms.SecurityObjectsImages();
        private XmlDocument xmlDocument = null;
        private bool loadChildObjects = false;
        private bool showSpecificUserGrants = false;

		//---------------------------------------------------------------------
        public ShowAllGrantsDialog(ShowObjectsTree showOT, SqlConnection aSqlConnection, int aCompanyId)
		{
			InitializeComponent();
            
			sqlConnection = aSqlConnection;
            showObjectsTree = showOT;
            menuXmlParser = showObjectsTree.CurrentParser;
			companyId = aCompanyId;

			LoadAllUsers();
			LoadAllRoles();
		}

		//---------------------------------------------------------------------
		private void LoadAllRoles()
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return;

			string sSelect = "SELECT RoleId, Role, Disabled FROM MSD_CompanyRoles where CompanyId=" + companyId + "ORDER BY Role";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, sqlConnection);
			SqlDataReader myReader = mySqlCommand.ExecuteReader();

			RolesComboBox.DataSource = null;
			this.RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);

			if (rolesTable == null)
			{
				rolesTable = new DataTable();
				rolesTable.Columns.Add(new DataColumn("Role", Type.GetType("System.String")));
				rolesTable.Columns.Add(new DataColumn("RoleId", Type.GetType("System.Int32")));
			}
			else
				rolesTable.Clear();

			DataRow row = rolesTable.NewRow();

			row["Role"] = firstItemText;
			row["RoleId"] = -1;
			rolesTable.Rows.Add(row);

			row = rolesTable.NewRow();
			row["Role"] = Strings.All;
			row["RoleId"] = -1;
			rolesTable.Rows.Add(row);

			int maxlength = 0;

			while (myReader.Read())
			{
				row = rolesTable.NewRow();
				row["Role"] = myReader["Role"].ToString();
				row["RoleId"] = Convert.ToInt32(myReader["RoleId"]);
				rolesTable.Rows.Add(row);

				if (myReader["Role"].ToString().Length > maxlength)
					maxlength = myReader["Role"].ToString().Length;
			}

			myReader.Close();
			mySqlCommand.Dispose();

			//Setto le proprietà di DataSource, DisplayMember, ValueMember
			RolesComboBox.DataSource = rolesTable;
			RolesComboBox.DisplayMember = "Role";
			RolesComboBox.ValueMember = "RoleId";
			maxlength = (maxlength * averageCharWidth) + RolesComboBox.Height + 10;

			if (maxlength > RolesComboBox.Width)
				RolesComboBox.Width = maxlength;

			this.RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
		}

		//---------------------------------------------------------------------
		private void LoadAllUsers()
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return;

			int EasyLookLoginId = CommonObjectTreeFunction.GetApplicationUserID(NameSolverStrings.EasyLookSystemLogin, sqlConnection);

			string sSelect = @"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login, MSD_CompanyLogins.Disabled, 
								MSD_CompanyLogins.Admin FROM MSD_CompanyLogins INNER JOIN
								MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId where
								CompanyId=" + companyId + " AND MSD_CompanyLogins.LoginId <> " + EasyLookLoginId + " ORDER BY MSD_Logins.Login";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, sqlConnection);
			SqlDataReader myReader = mySqlCommand.ExecuteReader();

			UsersComboBox.DataSource = null;

			this.UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);

			int maxlength = 0;

			if (usersTable == null)
			{
				usersTable = new DataTable();
				usersTable.Columns.Add(new DataColumn("Login", Type.GetType("System.String")));
				usersTable.Columns.Add(new DataColumn("LoginId", Type.GetType("System.Int32")));
			}
			else
				usersTable.Clear();

			DataRow row = usersTable.NewRow();
			row["Login"] = firstItemText;
			row["LoginId"] = -1;
			usersTable.Rows.Add(row);

			row = usersTable.NewRow();
			row["Login"] = Strings.All;
			row["LoginId"] = -1;
			usersTable.Rows.Add(row);

			while (myReader.Read())
			{
				row = usersTable.NewRow();
				row["Login"] = myReader["Login"].ToString();
				row["LoginId"] = Convert.ToInt32(myReader["LoginId"]);
				usersTable.Rows.Add(row);

				if (myReader["Login"].ToString().Length > maxlength)
					maxlength = myReader["Login"].ToString().Length;
			}

			myReader.Close();
			mySqlCommand.Dispose();

			maxlength = (maxlength * averageCharWidth) + UsersComboBox.Height + 10;

			//Setto le proprietà di DataSource, DisplayMember, ValueMember
			UsersComboBox.DataSource = usersTable;
			UsersComboBox.DisplayMember = "Login";
			UsersComboBox.ValueMember = "LoginId";

			if (maxlength > UsersComboBox.Width)
			{
				//Se cresce la combo devo spostare tutto
				this.SearchButton.Left = this.UsersComboBox.Left + (maxlength + UsersComboBox.Width);
				UsersComboBox.Width = maxlength;
			}

			this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
		}

		//---------------------------------------------------------------------
		private void RolesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (RolesComboBox.Text != firstItemText)
			{
				UsersComboBox.SelectedIndexChanged -= new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
				UsersComboBox.Text = firstItemText;
				UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
			}

		}

		//---------------------------------------------------------------------
		private void UsersComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (UsersComboBox.Text != firstItemText)
			{
				RolesComboBox.SelectedIndexChanged -= new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
				RolesComboBox.Text = firstItemText;
				RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
			}
		}

		//---------------------------------------------------------------------

		private void SearchButton_Click(object sender, System.EventArgs e)
		{
			GrantsTreeView.Nodes.Clear();

			if (string.Compare(firstItemText, this.RolesComboBox.Text) == 0 &&
						string.Compare(firstItemText, this.UsersComboBox.Text) == 0)
			{
				MessageBox.Show(Strings.SelectRoleOrUser, Strings.Error,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			this.Cursor = Cursors.WaitCursor;
            this.loadChildObjects = this.checkShowChildObjects.Checked;
            this.GrantsTreeView.BeginUpdate();
            this.GrantsTreeView.ImageList = this.soi.imageListObjects;  //SmallImageList

            xmlDocument = new XmlDocument();
            XmlDeclaration declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDocument.AppendChild(declaration);
			TreeNode root = new TreeNode("All Objects");
            XmlElement rootElement = xmlDocument.CreateElement("AllObjects");
            xmlDocument.AppendChild(rootElement);

			ArrayList applications = menuXmlParser.Root.ApplicationsItems;

			if (applications != null && applications.Count > 0)
			{
                int rangeMax = 0;
                foreach (MenuXmlNode a in applications)
                {
                    if (a.IsApplication)
                    {
                        ArrayList g = a.GroupItems;
                        if (g != null)
                            rangeMax += g.Count;
                    }
                }
                this.progressAppGroups.Maximum = rangeMax;
                this.progressAppGroups.Value = 0;

				foreach (MenuXmlNode application in applications)
				{
                    XmlElement xApp = xmlDocument.CreateElement("Application");
                    TreeNode appNode = AddDescendantNode(application, xApp);
                    if (appNode != null)
                    {
                        root.Nodes.Add(appNode);

                        xApp.SetAttribute("title", application.Title);
                        rootElement.AppendChild(xApp);
                    }
				}
			}

			this.GrantsTreeView.Nodes.Add(root);
			this.GrantsTreeView.ExpandAll();

			if (this.GrantsTreeView.Nodes != null && this.GrantsTreeView.Nodes.Count > 0)
				this.GrantsTreeView.SelectedNode = this.GrantsTreeView.Nodes[0];

            this.GrantsTreeView.EndUpdate();
			this.Cursor = Cursors.Default;
		}

		//---------------------------------------------------------------------
		private TreeNode AddDescendantNode(MenuXmlNode xmlNode, XmlElement xParent)
		{
			TreeNode itemNode = null;

			if (xmlNode.IsApplication)
			{
				ArrayList groupChildren = xmlNode.GroupItems;
				if (groupChildren != null && groupChildren.Count > 0)
				{
					foreach (MenuXmlNode menuNode in groupChildren)
					{
                        TreeNode addedNode = AddDescendantNode(menuNode, xParent);
                        this.progressAppGroups.Value += 1;
                        Application.DoEvents();

						if (addedNode != null)
						{
                            if (itemNode == null)
                            {
                                itemNode = new TreeNode(xmlNode.Title);
                                itemNode.StateImageIndex = soi.appImageIndex; 
                            }
                            itemNode.Nodes.Add(addedNode);
						}
					}
				}
			}

			if (xmlNode.IsGroup || xmlNode.IsMenu)
			{
				ArrayList menuChildren = xmlNode.MenuItems;

				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode menuNode in menuChildren)
					{
                        XmlElement xMenu = xmlDocument.CreateElement(xmlNode.IsGroup ? "Group" : "Menu");
						TreeNode addedNode = AddDescendantNode(menuNode, xMenu);
						if (addedNode != null)
						{
                            if (itemNode == null)
                            {
                                itemNode = new TreeNode(xmlNode.Title);
                                if (xmlNode.IsGroup)
                                    itemNode.ForeColor = Color.Brown;
                                itemNode.ImageIndex = xmlNode.IsGroup ? soi.groupImageIndex : soi.menuImageIndex;
                            }

                            xMenu.SetAttribute("Title", xmlNode.Title);
                            xParent.AppendChild(xMenu);

							itemNode.Nodes.Add(addedNode);
						}
					}
				}
			}

			if (xmlNode.IsMenu)
			{
				ArrayList commandChildren = xmlNode.CommandItems;

				if (commandChildren != null && commandChildren.Count > 0)
				{
					foreach (MenuXmlNode commandNode in commandChildren)
					{
                        XmlElement xMenu = xmlDocument.CreateElement("Menu");
						TreeNode addedNode = AddDescendantNode(commandNode, xMenu);
						if (addedNode != null)
						{
                            if (itemNode == null)
                            {
                                itemNode = new TreeNode(xmlNode.Title);
                                //itemNode.ForeColor = Color.Brown;
                                itemNode.ImageIndex = soi.menuImageIndex;
                            }

                            xMenu.SetAttribute("Title", xmlNode.Title);
                            xParent.AppendChild(xMenu);

							itemNode.Nodes.Add(addedNode);
						}
					}
				}
			}

			if (xmlNode.IsCommand)
			{
 				string title = RolesComboBox.Text;
				bool isRole = true;
				int id = Convert.ToInt32(RolesComboBox.SelectedValue);

				if (string.Compare(this.RolesComboBox.Text, firstItemText) == 0)
				{
					isRole = false;
					id = Convert.ToInt32(UsersComboBox.SelectedValue);
					title = UsersComboBox.Text;
				}

                XmlElement xObj = null;
                int oslType = 0;
                int objectTypeIDFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(xmlNode, sqlConnection, out oslType);
				int objectId = CommonObjectTreeFunction.GetObjectId(xmlNode.ItemObject, objectTypeIDFromDataBase, sqlConnection);
                string objType = xmlNode.Name;
                if (objType.CompareNoCase("ExternalItem"))
                {
                    try
                    {
                        objType = ((SecurityType)oslType).ToString();
                    }
                    catch (Exception)
                    {
                    }
                }

				if (string.Compare(this.UsersComboBox.Text, Strings.All) == 0)
				{
                    xObj = xmlDocument.CreateElement("Object");
 					foreach (DataRow row in usersTable.Rows)
					{
						if (string.Compare(Strings.All, row["Login"].ToString()) == 0 ||
							string.Compare(firstItemText, row["Login"].ToString()) == 0)
							continue;

                        TreeNode grantNode = WriteGrantNode(xmlNode, xObj, objectTypeIDFromDataBase, false, Convert.ToInt32(row["LoginId"]), objectId, row["Login"].ToString());
						if (grantNode != null)
						{
                            if (itemNode == null)
                            {
                                itemNode = new TreeNode(xmlNode.Title);
                                itemNode.ForeColor = Color.Blue;
                                itemNode.Text += " - " + objType + ":" + xmlNode.ItemObject;
                                itemNode.ImageIndex = this.soi.GetImageIndex(oslType);

                                xObj.SetAttribute("Title", xmlNode.Title);
                                xObj.SetAttribute("Type", objType);
                                xObj.SetAttribute("Namespace", xmlNode.ItemObject);
                                xObj.SetAttribute("DBType", oslType.ToString());
                                xParent.AppendChild(xObj);
                            }
							itemNode.Nodes.Add(grantNode);
						}
					}
				}
				else if (string.Compare(this.RolesComboBox.Text, Strings.All) == 0)
				{
                    xObj = xmlDocument.CreateElement("Object");
 				    foreach (DataRow row in rolesTable.Rows)
					{
						if (string.Compare(Strings.All, row["Role"].ToString()) == 0 ||
							string.Compare(firstItemText, row["Role"].ToString()) == 0)
							continue;

                        TreeNode grantNode = WriteGrantNode(xmlNode, xObj, objectTypeIDFromDataBase, true, Convert.ToInt32(row["RoleId"]), objectId, row["Role"].ToString());
						if (grantNode != null)
						{
                            if (itemNode == null)
                            {
                                itemNode = new TreeNode(xmlNode.Title);
                                itemNode.ForeColor = Color.Blue;
                                itemNode.Text += " - " + objType + ":" + xmlNode.ItemObject;
                                itemNode.ImageIndex = this.soi.GetImageIndex(oslType);

                                xObj.SetAttribute("Title", xmlNode.Title);
                                xObj.SetAttribute("Type", objType);
                                xObj.SetAttribute("Namespace", xmlNode.ItemObject);
                                xObj.SetAttribute("DBType", oslType.ToString());
                                xParent.AppendChild(xObj);
                           }

						   itemNode.Nodes.Add(grantNode);
						}
					}
				}
				else
				{
                    xObj = xmlDocument.CreateElement("Object");
                    TreeNode grantNode = WriteGrantNode(xmlNode, xObj, objectTypeIDFromDataBase, isRole, id, objectId, title);
					if (grantNode != null)
					{
                        if (itemNode == null)
                        {
                            itemNode = new TreeNode(xmlNode.Title);
                            itemNode.ForeColor = Color.Blue;
                            itemNode.Text += " - " + objType + ":" + xmlNode.ItemObject;
                            itemNode.ImageIndex = this.soi.GetImageIndex(oslType);

                            xObj.SetAttribute("Title", xmlNode.Title);
                            xObj.SetAttribute("Type", objType);
                            xObj.SetAttribute("Namespace", xmlNode.ItemObject);
                            xObj.SetAttribute("DBType", oslType.ToString());

                            xParent.AppendChild(xObj);
                        }
					    itemNode.Nodes.Add(grantNode);
					}
				}

                if (
                        itemNode != null && loadChildObjects &&
                        (   
                            oslType == (int)SecurityType.DataEntry || oslType == (int)SecurityType.Batch ||

                            oslType == (int)SecurityType.ChildForm || 
                            oslType == (int)SecurityType.RowView || 
                            oslType == (int)SecurityType.EmbeddedView ||

                            oslType == (int)SecurityType.Tabber || oslType == (int)SecurityType.Tab ||
                            oslType == (int)SecurityType.TileManager || oslType == (int)SecurityType.Tile ||

                            oslType == (int)SecurityType.Grid || 
                            oslType == (int)SecurityType.Toolbar ||

                            oslType == (int)SecurityType.Tabber ||			
		                    oslType == (int)SecurityType.TileManager ||					
		                    oslType == (int)SecurityType.Tile ||					
			
		                    oslType == (int)SecurityType.ToolbarButton  ||				
		                    oslType == (int)SecurityType.PropertyGrid       

                        )
                    )
                {
                    try
                    {
                       showObjectsTree.AddAuxiliaryChildrenObjectsToCommandNode(xmlNode);
                    }
                    catch (Exception)
                    {
                        return itemNode; 
                    }

                    ArrayList childs = xmlNode.CommandItems;
                    if (childs != null && childs.Count > 0)
                    {
                        foreach (MenuXmlNode childNode in childs)
                        {
                            try
                            {
                                TreeNode addedNode = AddDescendantNode(childNode, xObj);
                                if (addedNode != null)
                                {
                                    itemNode.Nodes.Add(addedNode);
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
			}

			return itemNode;
		}

		//---------------------------------------------------------------------
        private TreeNode WriteGrantNode(MenuXmlNode xmlNode, XmlElement xParent, int objectTypeFromDataBase, bool isRole, int id, int objectId, string name)
		{
			ArrayList grantsArrayList = FindGrants(this.sqlConnection,
				xmlNode.ItemObject,
				objectTypeFromDataBase);

			CommonObjectTreeFunction.LoadStoredProcedure(this.sqlConnection,
				ref grantsArrayList,
				isRole,
				this.companyId,
				id,
				objectId);

            if (grantsArrayList == null || grantsArrayList.Count == 0)
            {
                return null;
            }

            string totalGrants = string.Empty;
			bool showName = true;

            string userGrants = string.Empty;
            bool showUser = true;
            bool userHasDeny = false;

			for (int i = 0; i < grantsArrayList.Count; i++)
			{
                GrantsRow row = grantsArrayList[i] as GrantsRow;
                if (row == null) continue;

                //TODO filtro per mostrare grant espliciti del solo utente
                //if (((GrantInfo.IconState)(row.UserIconState)) == GrantInfo.IconState.IconAllowed)

				if (((GrantInfo.IconState)(row.TotalIconState)) == GrantInfo.IconState.IconAllowed)
				{
					if (showName)
					{
						totalGrants = String.Concat(name, ": ", ((GrantsRow)grantsArrayList[i]).Description);
						showName = false;
					}
					else
						totalGrants = String.Concat(totalGrants, ", ", ((GrantsRow)grantsArrayList[i]).Description);
				}

                if (!isRole && showSpecificUserGrants)
                {
                    bool deny = false;
                    if (
                        ((GrantInfo.IconState)(row.UserIconState)) == GrantInfo.IconState.IconAllowed
                        ||
                        (deny = ((GrantInfo.IconState)(row.UserIconState)) == GrantInfo.IconState.IconForbidden)
                        )
                    {
                        if (deny)
                            userHasDeny = true;

                        if (showUser)
                        {
                            userGrants = String.Concat("USER", ": ", deny ? " DENY " : "",((GrantsRow)grantsArrayList[i]).Description);
                            showUser = false;
                        }
                        else
                            userGrants = String.Concat(userGrants, ", ", deny ? " DENY " : "", ((GrantsRow)grantsArrayList[i]).Description);
                    }
                }
			}

			if (totalGrants != string.Empty)
			{
				TreeNode grantNode = new TreeNode(totalGrants);
				grantNode.ForeColor = Color.Green;

                XmlElement xGrant = xmlDocument.CreateElement("Grant");
                int idx = totalGrants.IndexOf(':');
                xGrant.SetAttribute("Owner", totalGrants.Left(idx));
                xGrant.InnerText = totalGrants.Mid(idx + 1);

                if (userGrants != string.Empty)
                {
                    TreeNode userNode = new TreeNode(userGrants);
                    userNode.ForeColor = userHasDeny ? Color.Red : Color.Green;

                    grantNode.Nodes.Add(userNode);

                    XmlElement xUserGrant = xmlDocument.CreateElement("UserGrant");
                    idx = userGrants.IndexOf(':');
                    xUserGrant.SetAttribute("Owner", userGrants.Left(idx));
                    xUserGrant.InnerText = userGrants.Mid(idx + 1);

                    xGrant.AppendChild(xUserGrant);
                }

                xParent.AppendChild(xGrant);

				return grantNode;
			}

			return null;
		}

		#region Funzione che cerca i Grants Associati alla tipologia

		//---------------------------------------------------------------------
		/// <summary>
		/// Cerca i Grants ai quali è associata la tipologia di oggetto che ho
		/// selezionato nel Tree della Carlotta
		/// </summary>
		public ArrayList FindGrants(SqlConnection sqlConnection, string objectNameSpace, int objectTypeFromDataBase)
		{
			ArrayList grantsArrayList = new ArrayList();


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
					grantsArrayList.Add(new GrantsRow(Convert.ToInt32(myReader["GrantMask"]), myReader["GrantName"].ToString(), GrantsString.GetGrantDescription(myReader["GrantName"].ToString())));
				}
				myReader.Close();
				mySqlCommand.Dispose();
			}
			catch (SqlException)
			{
				if (myReader != null) myReader.Close();
				mySqlCommand.Dispose();
				return null;
			}

			return grantsArrayList;
		}

		#endregion

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.GrantsTreeView.Nodes == null || this.GrantsTreeView.Nodes.Count < 1)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.ValidateNames = true;
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

            DialogResult saveFileDlgResult = saveFileDialog.ShowDialog();

            if (saveFileDlgResult != DialogResult.OK)
                return;
 
            //XmlDocument xmlDocument = new XmlDocument();
            //XmlDeclaration declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            //xmlDocument.AppendChild(declaration);
            //XmlElement rootElement = xmlDocument.CreateElement("Root");
            //xmlDocument.AppendChild(rootElement);
            //NavigateTree(xmlDocument, rootElement, this.GrantsTreeView.Nodes[0]);

            xmlDocument.Save(saveFileDialog.FileName);
        }

        private void NavigateTree(XmlDocument xmlDocument, XmlElement parent, TreeNode t)
        {
            if (parent == null) return;
            if (t == null) return;

            XmlElement element = xmlDocument.CreateElement("Node");
            parent.AppendChild(element);

            foreach (TreeNode childT in t.Nodes)
            {
                NavigateTree(xmlDocument, element, childT);
            }
        }
	}
}